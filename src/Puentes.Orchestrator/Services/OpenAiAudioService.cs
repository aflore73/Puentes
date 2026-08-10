using OpenAI;
using OpenAI.Audio;
using Puentes.Orchestrator.AI;

namespace Puentes.Orchestrator.Services;

public class OpenAiAudioService :
    IAudioService,
    ISpeechSynthesisService,
    IStreamingSpeechSynthesisService
{
    private readonly AudioClient _transcriptionClient;
    private readonly AudioClient _speechClient;
    private readonly AudioClient _streamingSpeechClient;

    public OpenAiAudioService(OpenAiAudioOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException(
                "No se encontró la variable PUENTES_API_KEY.");
        }

        var client = new OpenAIClient(options.ApiKey);
        _transcriptionClient = client.GetAudioClient(
            options.TranscriptionModel);
        _speechClient = client.GetAudioClient(options.SpeechModel);
        _streamingSpeechClient = client.GetAudioClient(
            options.StreamingSpeechModel);
    }

    public async Task<string> TranscribeAsync(
        Stream audio,
        string filename,
        CancellationToken cancellationToken = default)
    {
        AudioTranscription transcription = await _transcriptionClient
            .TranscribeAudioAsync(
                audio,
                filename,
                new AudioTranscriptionOptions(),
                cancellationToken);

        return transcription.Text.Trim();
    }

    public async Task<GeneratedSpeech> GenerateSpeechAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        BinaryData audio = await _speechClient.GenerateSpeechAsync(
            text,
            GeneratedSpeechVoice.Nova,
            new SpeechGenerationOptions
            {
                SpeedRatio = 0.95f
            },
            cancellationToken);

        return new GeneratedSpeech(
            audio.ToArray(),
            "audio/mpeg",
            "puentes-response.mp3");
    }

#pragma warning disable OPENAI001
    public async IAsyncEnumerable<byte[]> GenerateSpeechStreamAsync(
        string text,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var options = new SpeechGenerationOptions
        {
            ResponseFormat = GeneratedSpeechFormat.Mp3,
            SpeedRatio = 0.9f
        };

        await foreach (StreamingSpeechUpdate update in _streamingSpeechClient
            .GenerateSpeechStreamingAsync(
                text,
                GeneratedSpeechVoice.Nova,
                options,
                cancellationToken))
        {
            if (update is StreamingSpeechAudioDeltaUpdate audioUpdate)
            {
                yield return audioUpdate.AudioBytes.ToArray();
            }
        }
    }

    public async IAsyncEnumerable<byte[]> GeneratePcmSpeechStreamAsync(
        string text,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var options = new SpeechGenerationOptions
        {
            ResponseFormat = GeneratedSpeechFormat.Pcm,
            SpeedRatio = 0.95f
        };

        await foreach (StreamingSpeechUpdate update in _streamingSpeechClient
            .GenerateSpeechStreamingAsync(
                text,
                GeneratedSpeechVoice.Nova,
                options,
                cancellationToken))
        {
            if (update is StreamingSpeechAudioDeltaUpdate audioUpdate)
            {
                yield return audioUpdate.AudioBytes.ToArray();
            }
        }
    }
#pragma warning restore OPENAI001
}
