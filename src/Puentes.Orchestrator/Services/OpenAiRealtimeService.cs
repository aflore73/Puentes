using System.Net.WebSockets;
using System.Globalization;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using NAudio.Wave;
using Puentes.Orchestrator.AI;
using Puentes.Orchestrator.AI.Models;

namespace Puentes.Orchestrator.Services;

public sealed class OpenAiRealtimeService : IAsyncDisposable
{
    private readonly OpenAiRealtimeOptions _options;
    private readonly ILogger<OpenAiRealtimeService> _logger;
    private readonly MemoryConversationWorkflowService _conversationWorkflow;
    private readonly MedicationQueryWorkflowService _medicationQueryWorkflow;
    private readonly IStreamingSpeechSynthesisService _speechService;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly Channel<string> _transcriptions =
        Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true
        });
    private ClientWebSocket? _socket;
    private CancellationTokenSource? _sessionCancellation;
    private Task? _receiveTask;
    private TaskCompletionSource? _sessionUpdated;
    private RealtimeSessionContext? _context;
    private Guid? _conversationId;
    private RealtimeKnownPerson? _focusedPerson;

    public OpenAiRealtimeService(
        OpenAiRealtimeOptions options,
        ILogger<OpenAiRealtimeService> logger,
        MemoryConversationWorkflowService conversationWorkflow,
        MedicationQueryWorkflowService medicationQueryWorkflow,
        IStreamingSpeechSynthesisService speechService)
    {
        _options = options;
        _logger = logger;
        _conversationWorkflow = conversationWorkflow;
        _medicationQueryWorkflow = medicationQueryWorkflow;
        _speechService = speechService;
    }

    public async Task RunTurnAsync(
        RealtimeSessionContext context,
        PeripheralActivationOptions activationOptions,
        CancellationToken cancellationToken)
    {
        await EnsureConnectedAsync(context, cancellationToken);
        while (_transcriptions.Reader.TryRead(out _))
        {
        }
        await SendEventAsync(new
        {
            type = "input_audio_buffer.clear"
        }, cancellationToken);

        var turnTimer = Stopwatch.StartNew();

        var audioChannel = Channel.CreateUnbounded<byte[]>(new()
        {
            SingleReader = true,
            SingleWriter = true
        });
        var sendTask = SendAudioAsync(audioChannel.Reader, cancellationToken);
        await CaptureAudioAsync(
            audioChannel.Writer, activationOptions, cancellationToken);
        audioChannel.Writer.TryComplete();
        await sendTask;
        await SendEventAsync(new
        {
            type = "input_audio_buffer.commit"
        }, cancellationToken);

        string transcript;
        try
        {
            transcript = await _transcriptions.Reader.ReadAsync(
                    cancellationToken)
                .AsTask()
                .WaitAsync(TimeSpan.FromSeconds(20), cancellationToken);
        }
        catch (TimeoutException)
        {
            InvalidateConnection();
            _logger.LogWarning(
                "OpenAI Realtime no devolvio la transcripcion dentro de 20 segundos.");
            await PlayStreamingSpeechAsync(
                "No te entendí bien, Marta. ¿Podés repetirme lo que dijiste?",
                cancellationToken);
            return;
        }
        _logger.LogInformation(
            "Latencia hasta transcripcion: {ElapsedMilliseconds} ms.",
            turnTimer.ElapsedMilliseconds);

        await GenerateHybridResponseAsync(transcript, cancellationToken);
        _logger.LogInformation(
            "Latencia total del turno: {ElapsedMilliseconds} ms.",
            turnTimer.ElapsedMilliseconds);
    }

    public Task PrepareAsync(
        RealtimeSessionContext context,
        CancellationToken cancellationToken) =>
        EnsureConnectedAsync(context, cancellationToken);

    public async ValueTask DisposeAsync()
    {
        _sessionCancellation?.Cancel();
        if (_socket?.State == WebSocketState.Open)
        {
            await _socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Puentes finalizado",
                CancellationToken.None);
        }

        _socket?.Dispose();
        _sessionCancellation?.Dispose();
        _sendLock.Dispose();
        _connectionLock.Dispose();
    }

    private async Task EnsureConnectedAsync(
        RealtimeSessionContext context,
        CancellationToken cancellationToken)
    {
        if (_socket?.State == WebSocketState.Open)
        {
            return;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_socket?.State == WebSocketState.Open)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException(
                    "Falta la variable PUENTES_API_KEY.");
            }

            _sessionCancellation?.Cancel();
            _sessionCancellation?.Dispose();
            _socket?.Dispose();
            _sessionCancellation = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken);
            _context = context;
            _socket = new ClientWebSocket();
            _socket.Options.SetRequestHeader(
                "Authorization", $"Bearer {_options.ApiKey}");
            var uri = new Uri(
                "wss://api.openai.com/v1/realtime?model=" +
                Uri.EscapeDataString(_options.Model));
            await _socket.ConnectAsync(uri, cancellationToken);
            _receiveTask = ReceiveEventsAsync(
                _socket, _sessionCancellation.Token);

            _sessionUpdated = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            await SendEventAsync(new
            {
                type = "session.update",
                session = new
                {
                    type = "realtime",
                    model = _options.Model,
                    output_modalities = new[] { "audio" },
                    audio = new
                    {
                        input = new
                        {
                            format = new { type = "audio/pcm", rate = 24000 },
                            transcription = new
                            {
                                model = "gpt-live-transcribe",
                                prompt = "Conversacion familiar en espanol argentino. " +
                                    "Prestar especial atencion a los nombres propios.",
                                keywords = context.KnownPeople
                                    .Select(person => person.Name
                                        .Replace("\r", " ")
                                        .Replace("\n", " "))
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .ToArray(),
                                languages = new[] { "es" },
                                delay = "medium"
                            },
                            turn_detection = (object?)null
                        },
                        output = new
                        {
                            format = new { type = "audio/pcm", rate = 24000 },
                            voice = _options.Voice
                        }
                    },
                    instructions = context.Instructions
                }
            }, cancellationToken);
            await _sessionUpdated.Task.WaitAsync(
                TimeSpan.FromSeconds(10), cancellationToken);
            _logger.LogInformation(
                "Sesion Realtime conectada con {Model}.", _options.Model);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task CaptureAudioAsync(
        ChannelWriter<byte[]> channel,
        PeripheralActivationOptions options,
        CancellationToken cancellationToken)
    {
        var silenceDetected = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var heardSpeech = false;
        var lastSpeechAt = DateTimeOffset.UtcNow;
        using var recorder = new WaveInEvent
        {
            WaveFormat = new WaveFormat(24000, 16, 1),
            BufferMilliseconds = 100
        };
        recorder.DataAvailable += (_, args) =>
        {
            var audio = args.Buffer.AsSpan(0, args.BytesRecorded).ToArray();
            channel.TryWrite(audio);
            var peak = GetPeakAmplitude(audio);
            if (peak >= options.SpeechThreshold)
            {
                heardSpeech = true;
                lastSpeechAt = DateTimeOffset.UtcNow;
            }
            else if (heardSpeech && DateTimeOffset.UtcNow - lastSpeechAt >=
                TimeSpan.FromMilliseconds(
                    Math.Max(600, options.SilenceMilliseconds)))
            {
                silenceDetected.TrySetResult();
            }
        };

        recorder.StartRecording();
        var maximumDuration = Task.Delay(
            TimeSpan.FromSeconds(Math.Max(1, options.RecordingSeconds)),
            cancellationToken);
        await Task.WhenAny(silenceDetected.Task, maximumDuration);
        cancellationToken.ThrowIfCancellationRequested();
        recorder.StopRecording();
    }

    private async Task SendAudioAsync(
        ChannelReader<byte[]> channel,
        CancellationToken cancellationToken)
    {
        await foreach (var audio in channel.ReadAllAsync(cancellationToken))
        {
            await SendEventAsync(new
            {
                type = "input_audio_buffer.append",
                audio = Convert.ToBase64String(audio)
            }, cancellationToken);
        }
    }

    private async Task ReceiveEventsAsync(
        ClientWebSocket socket,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        try
        {
            while (socket.State == WebSocketState.Open &&
                !cancellationToken.IsCancellationRequested)
            {
                using var message = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(
                        buffer, cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }
                    message.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                using var document = JsonDocument.Parse(message.ToArray());
                HandleServerEvent(document.RootElement);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Se interrumpio la sesion Realtime.");
        }
    }

    private void HandleServerEvent(JsonElement serverEvent)
    {
        if (!serverEvent.TryGetProperty("type", out var typeElement))
        {
            return;
        }

        switch (typeElement.GetString())
        {
            case "session.updated":
                _sessionUpdated?.TrySetResult();
                break;
            case "conversation.item.input_audio_transcription.completed":
                if (serverEvent.TryGetProperty("transcript", out var inputTranscript))
                {
                    var transcriptText = inputTranscript.GetString() ?? string.Empty;
                    _logger.LogInformation(
                        "Marta (transcripcion): {Transcript}",
                        transcriptText);
                    _transcriptions.Writer.TryWrite(transcriptText);
                }
                break;
            case "error":
                var error = serverEvent.TryGetProperty("error", out var detail)
                    ? detail.ToString()
                    : serverEvent.ToString();
                var exception = new InvalidOperationException(error);
                _sessionUpdated?.TrySetException(exception);
                _logger.LogError("OpenAI Realtime devolvio un error: {Error}", error);
                break;
        }
    }

    private async Task GenerateHybridResponseAsync(
        string transcript,
        CancellationToken cancellationToken)
    {
        var stageTimer = Stopwatch.StartNew();
        var matches = FindKnownPeople(transcript);
        if (matches.Count > 0)
        {
            _focusedPerson = matches[0];
        }
        else if (_focusedPerson is not null)
        {
            matches.Add(_focusedPerson);
        }
        _logger.LogInformation(
            "Contexto resuelto para el turno: {Matches}",
            matches.Count == 0
                ? "sin coincidencias"
                : string.Join(", ", matches.Select(person =>
                    $"{person.Name} ({person.Description})")));
        var resolvedInput = matches.Count == 0
            ? transcript
            : transcript + "\n[Contexto resuelto por Puentes: " +
              string.Join("; ", matches.Select(person =>
                  $"{person.Name}, {person.Description}")) + "]";
        AssistantResponse assistantResponse;
        if (MedicationIntentDetector.IsMedicationQuery(transcript))
        {
            _logger.LogInformation(
                "Consulta enrutada al flujo de medicacion.");
            assistantResponse = await _medicationQueryWorkflow.ProcessAsync(
                _context!.PersonId,
                transcript,
                cancellationToken);
        }
        else
        {
            var turn = _conversationId is null
                ? await _conversationWorkflow.StartAsync(
                    _context!.PersonId,
                    resolvedInput,
                    cancellationToken,
                    _focusedPerson?.Name)
                : await _conversationWorkflow.ContinueAsync(
                    _conversationId.Value,
                    resolvedInput,
                    cancellationToken,
                    _focusedPerson?.Name);
            _conversationId = turn.ConversationId;
            assistantResponse = turn.Response;
        }

        var responseText = ValidateResponse(
            VoiceResponseFormatter.Prepare(assistantResponse.Message),
            matches);
        _logger.LogInformation(
            "Latencia de respuesta de texto: {ElapsedMilliseconds} ms.",
            stageTimer.ElapsedMilliseconds);

        _logger.LogInformation("Puentes: {Response}", responseText);
        await PlayStreamingSpeechAsync(responseText, cancellationToken);
    }

    private async Task SendEventAsync(
        object value,
        CancellationToken cancellationToken)
    {
        var socket = _socket ?? throw new InvalidOperationException(
            "La sesion Realtime no esta conectada.");
        var json = JsonSerializer.Serialize(value);
        var content = Encoding.UTF8.GetBytes(json);
        await _sendLock.WaitAsync(cancellationToken);
        try
        {
            await socket.SendAsync(
                content,
                WebSocketMessageType.Text,
                true,
                cancellationToken);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private void InvalidateConnection()
    {
        _sessionCancellation?.Cancel();
        _socket?.Abort();
        _socket?.Dispose();
        _socket = null;
    }

    private static string ValidateResponse(
        string response,
        IReadOnlyCollection<RealtimeKnownPerson> matches)
    {
        var normalized = Normalize(response);
        string[] unsafeExpressions =
        [
            "pachucho",
            "enfermo",
            "enferma",
            "accidente",
            "hospital",
            "seguro que",
            "debe estar"
        ];
        if (!unsafeExpressions.Any(normalized.Contains))
        {
            return response;
        }

        return matches.Count > 0
            ? $"Entiendo que estes preocupada por {matches.First().Name}. " +
              "Podes enviarle un mensaje y cuando pueda te va a contestar."
            : "Entiendo. Contame un poco mas asi puedo ayudarte con tranquilidad.";
    }

    private async Task PlayStreamingSpeechAsync(
        string text,
        CancellationToken cancellationToken)
    {
        const int pcmBytesPerSecond = 24000 * 2;
        const int startupBufferBytes = pcmBytesPerSecond / 4;
        var timer = Stopwatch.StartNew();
        var buffer = new BufferedWaveProvider(new WaveFormat(24000, 16, 1))
        {
            BufferDuration = TimeSpan.FromSeconds(30),
            DiscardOnBufferOverflow = false,
            ReadFully = true
        };
        using var output = new WaveOutEvent
        {
            DesiredLatency = 100,
            NumberOfBuffers = 3
        };
        output.Init(buffer);
        var playbackStarted = false;

        await foreach (var chunk in _speechService.GeneratePcmSpeechStreamAsync(
            text,
            cancellationToken))
        {
            if (chunk.Length == 0)
            {
                continue;
            }

            buffer.AddSamples(chunk, 0, chunk.Length);
            if (!playbackStarted &&
                buffer.BufferedBytes >= startupBufferBytes)
            {
                output.Play();
                playbackStarted = true;
                _logger.LogInformation(
                    "Latencia hasta primer audio TTS: {ElapsedMilliseconds} ms.",
                    timer.ElapsedMilliseconds);
            }
        }

        if (!playbackStarted && buffer.BufferedBytes > 0)
        {
            output.Play();
            playbackStarted = true;
            _logger.LogInformation(
                "Latencia hasta primer audio TTS: {ElapsedMilliseconds} ms.",
                timer.ElapsedMilliseconds);
        }

        while (playbackStarted && buffer.BufferedBytes > 0)
        {
            await Task.Delay(25, cancellationToken);
        }

        output.Stop();
        _logger.LogInformation(
            "Duracion total de TTS streaming y reproduccion: " +
            "{ElapsedMilliseconds} ms.",
            timer.ElapsedMilliseconds);
    }

    private static float GetPeakAmplitude(byte[] buffer)
    {
        var peak = 0f;
        for (var index = 0; index + 1 < buffer.Length; index += 2)
        {
            var sample = (short)(buffer[index] | buffer[index + 1] << 8);
            peak = Math.Max(peak, Math.Abs(sample / 32768f));
        }
        return peak;
    }

    private List<RealtimeKnownPerson> FindKnownPeople(string transcript)
    {
        var transcriptWords = Normalize(transcript)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length >= 4)
            .ToArray();
        var matches = new List<RealtimeKnownPerson>();

        foreach (var person in _context?.KnownPeople ?? [])
        {
            var nameWords = Normalize(person.Name)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var matched = nameWords.Any(nameWord =>
                transcriptWords.Any(transcriptWord =>
                    IsSimilarName(transcriptWord, nameWord)));
            if (matched)
            {
                matches.Add(person);
            }
        }

        return matches;
    }

    private static bool IsSimilarName(string heard, string known)
    {
        if (heard.Equals(known, StringComparison.Ordinal))
        {
            return true;
        }

        var maximumLength = Math.Max(heard.Length, known.Length);
        if (maximumLength < 5 || Math.Abs(heard.Length - known.Length) > 3)
        {
            return false;
        }

        var distance = LevenshteinDistance(heard, known);
        return 1d - (double)distance / maximumLength >= 0.6d;
    }

    private static string Normalize(string value)
    {
        var decomposed = value.ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) ==
                UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(character) ? character : ' ');
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static int LevenshteinDistance(string left, string right)
    {
        var previous = Enumerable.Range(0, right.Length + 1).ToArray();
        var current = new int[right.Length + 1];

        for (var leftIndex = 1; leftIndex <= left.Length; leftIndex++)
        {
            current[0] = leftIndex;
            for (var rightIndex = 1; rightIndex <= right.Length; rightIndex++)
            {
                var substitutionCost = left[leftIndex - 1] == right[rightIndex - 1]
                    ? 0
                    : 1;
                current[rightIndex] = Math.Min(
                    Math.Min(
                        current[rightIndex - 1] + 1,
                        previous[rightIndex] + 1),
                    previous[rightIndex - 1] + substitutionCost);
            }
            (previous, current) = (current, previous);
        }

        return previous[right.Length];
    }
}
