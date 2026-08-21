using Microsoft.Extensions.Options;
using NAudio.Wave;
using Puentes.Orchestrator.AI.Models;

namespace Puentes.Orchestrator.Services;

public sealed class PeripheralActivationWorker : BackgroundService
{
    private readonly ILogger<PeripheralActivationWorker> _logger;
    private readonly IAudioService _audioService;
    private readonly ISpeechSynthesisService _speechService;
    private readonly MemoryConversationWorkflowService _conversationWorkflow;
    private readonly MedicationQueryWorkflowService _medicationQueryWorkflow;
    private readonly RealtimeSessionContextService _realtimeContext;
    private readonly OpenAiRealtimeService _realtimeService;
    private readonly AudioCueService _audioCues;
    private readonly PeripheralActivationOptions _options;
    private int _busy;
    private DateTimeOffset _availableAt = DateTimeOffset.MinValue;
    private Guid? _conversationId;
    private DateTimeOffset _lastConversationActivityUtc = DateTimeOffset.MinValue;
    private Task? _realtimePreparationTask;
    private RealtimeSessionContext? _realtimeContextData;

    public PeripheralActivationWorker(
        ILogger<PeripheralActivationWorker> logger,
        IAudioService audioService,
        ISpeechSynthesisService speechService,
        MemoryConversationWorkflowService conversationWorkflow,
        MedicationQueryWorkflowService medicationQueryWorkflow,
        RealtimeSessionContextService realtimeContext,
        OpenAiRealtimeService realtimeService,
        AudioCueService audioCues,
        IOptions<PeripheralActivationOptions> options)
    {
        _logger = logger;
        _audioService = audioService;
        _speechService = speechService;
        _conversationWorkflow = conversationWorkflow;
        _medicationQueryWorkflow = medicationQueryWorkflow;
        _realtimeContext = realtimeContext;
        _realtimeService = realtimeService;
        _audioCues = audioCues;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("La activacion por perifericos esta deshabilitada.");
            return;
        }

        if (_options.InputMode == PeripheralInputMode.Keyboard)
        {
            await RunKeyboardInputAsync(stoppingToken);
            return;
        }

        if (!OperatingSystem.IsWindows())
        {
            _logger.LogInformation(
                "La activacion por audio solo esta disponible en Windows.");
            return;
        }

        using var listener = new GlobalInputListener(
            () => TryActivate(stoppingToken),
            _options.ActivationVirtualKey,
            _options.EnableMouseActivation);

        _logger.LogInformation(
            "Puentes esta en espera. Presione Enter para hablar.");
        if (_options.UseRealtime)
        {
            _realtimePreparationTask = PrepareRealtimeAsync(stoppingToken);
        }

        await Task.Run(() => listener.Run(stoppingToken), stoppingToken);
    }

    private void TryActivate(CancellationToken cancellationToken)
    {
        if (DateTimeOffset.UtcNow < _availableAt ||
            Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
        {
            return;
        }

        _ = ProcessActivationAsync(cancellationToken);
    }

    private async Task ProcessActivationAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _audioCues.ListeningAsync();
            _logger.LogInformation("Puentes activado. Escuchando...");
            if (_options.UseRealtime)
            {
                _realtimePreparationTask ??=
                    PrepareRealtimeAsync(cancellationToken);
                await _realtimePreparationTask;
                Console.WriteLine("Podés hablar.");
                await _realtimeService.RunTurnAsync(
                    _realtimeContextData!, _options, cancellationToken);
                return;
            }

            Console.WriteLine("Podés hablar.");
            var audio = await RecordAsync(cancellationToken);
            await _audioCues.CapturedAsync();
            await using var audioStream = new MemoryStream(audio);
            var transcript = await _audioService.TranscribeAsync(
                audioStream, "entrada-periferico.wav", cancellationToken);

            if (string.IsNullOrWhiteSpace(transcript))
            {
                _logger.LogInformation("No se detecto voz.");
                return;
            }
            await ProcessTranscriptAsync(transcript, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (_options.UseRealtime)
            {
                _realtimePreparationTask = null;
            }
            _logger.LogError(exception,
                "No se pudo completar la conversacion activada por perifericos.");
            await _audioCues.ErrorAsync();
        }
        finally
        {
            _availableAt = DateTimeOffset.UtcNow.AddSeconds(
                Math.Max(0, _options.CooldownSeconds));
            Interlocked.Exchange(ref _busy, 0);
        }
    }

    private async Task RunKeyboardInputAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Modo teclado habilitado. Escriba una pregunta y presione Enter.");
        if (_options.UseRealtime)
        {
            _realtimeContextData = await _realtimeContext.BuildAsync(
                _options.PersonId, cancellationToken);
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write("Marta: ");
            string? userInput;
            try
            {
                userInput = await Console.In.ReadLineAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (userInput is null)
            {
                break;
            }

            userInput = userInput.Trim();
            if (userInput.Length == 0)
            {
                continue;
            }

            try
            {
                if (_options.UseRealtime)
                {
                    await _realtimeService.RunTextTurnAsync(
                        _realtimeContextData!, userInput, cancellationToken);
                }
                else
                {
                    await ProcessTranscriptAsync(userInput, cancellationToken);
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "No se pudo completar la conversacion por teclado.");
                await _audioCues.ErrorAsync();
            }
        }
    }

    private async Task ProcessTranscriptAsync(
        string transcript,
        CancellationToken cancellationToken)
    {
        AssistantResponse assistantResponse;
        if (DateTimeOffset.UtcNow - _lastConversationActivityUtc >=
            TimeSpan.FromMinutes(30))
        {
            _conversationId = null;
        }
        if (MedicationIntentDetector.IsMedicationQuery(transcript))
        {
            assistantResponse = await _medicationQueryWorkflow.ProcessAsync(
                _options.PersonId,
                transcript,
                cancellationToken);
        }
        else
        {
            var turn = _conversationId is null
                ? await _conversationWorkflow.StartAsync(
                    _options.PersonId, transcript, cancellationToken)
                : await _conversationWorkflow.ContinueAsync(
                    _conversationId.Value, transcript, cancellationToken);
            _conversationId = turn.ConversationId;
            assistantResponse = turn.Response;
        }
        _lastConversationActivityUtc = DateTimeOffset.UtcNow;

        var responseText = VoiceResponseFormatter.Prepare(
            assistantResponse.Message);
        _logger.LogInformation("Marta: {Transcript}", transcript);
        _logger.LogInformation("Puentes: {Response}", responseText);

        var speech = await _speechService.GenerateSpeechAsync(
            responseText, cancellationToken);
        await PlayAsync(speech, cancellationToken);
    }

    private async Task PrepareRealtimeAsync(CancellationToken cancellationToken)
    {
        _realtimeContextData = await _realtimeContext.BuildAsync(
            _options.PersonId, cancellationToken);
        await _realtimeService.PrepareAsync(
            _realtimeContextData, cancellationToken);
    }

    private async Task<byte[]> RecordAsync(CancellationToken cancellationToken)
    {
        await using var output = new MemoryStream();
        var silenceDetected = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var heardSpeech = false;
        var lastSpeechAt = DateTimeOffset.UtcNow;
        using var recorder = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1),
            BufferMilliseconds = 100
        };
        using var writer = new WaveFileWriter(output, recorder.WaveFormat);
        recorder.DataAvailable += (_, args) =>
        {
            writer.Write(args.Buffer, 0, args.BytesRecorded);

            var peak = GetPeakAmplitude(args.Buffer, args.BytesRecorded);
            if (peak >= _options.SpeechThreshold)
            {
                heardSpeech = true;
                lastSpeechAt = DateTimeOffset.UtcNow;
            }
            else if (heardSpeech &&
                DateTimeOffset.UtcNow - lastSpeechAt >=
                TimeSpan.FromMilliseconds(
                    Math.Max(300, _options.SilenceMilliseconds)))
            {
                silenceDetected.TrySetResult();
            }
        };

        recorder.StartRecording();
        var maximumDuration = Task.Delay(
            TimeSpan.FromSeconds(Math.Max(1, _options.RecordingSeconds)),
            cancellationToken);
        await Task.WhenAny(silenceDetected.Task, maximumDuration);
        cancellationToken.ThrowIfCancellationRequested();
        recorder.StopRecording();
        writer.Flush();
        return output.ToArray();
    }

    private static float GetPeakAmplitude(byte[] buffer, int byteCount)
    {
        var peak = 0f;
        for (var index = 0; index + 1 < byteCount; index += 2)
        {
            var sample = (short)(buffer[index] | buffer[index + 1] << 8);
            peak = Math.Max(peak, Math.Abs(sample / 32768f));
        }

        return peak;
    }

    private static async Task PlayAsync(
        GeneratedSpeech speech,
        CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(speech.Content);
        using WaveStream reader = speech.ContentType.Contains("wav",
            StringComparison.OrdinalIgnoreCase)
            ? new WaveFileReader(stream)
            : new Mp3FileReader(stream);
        using var output = new WaveOutEvent();
        var completion = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        output.PlaybackStopped += (_, args) =>
        {
            if (args.Exception is not null)
            {
                completion.TrySetException(args.Exception);
            }
            else
            {
                completion.TrySetResult();
            }
        };
        using var registration = cancellationToken.Register(output.Stop);
        output.Init(reader);
        output.Play();
        await completion.Task;
    }
}
