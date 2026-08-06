using OpenAI;
using OpenAI.Audio;
using Puentes.Orchestrator.AI;

namespace Puentes.Orchestrator.Services;

public class OpenAiAudioService : IAudioService, ISpeechSynthesisService
{
    private readonly AudioClient _transcriptionClient;
    private readonly AudioClient _speechClient;

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
            GeneratedSpeechVoice.Alloy,
            new SpeechGenerationOptions
            {
                SpeedRatio = 0.9f
            },
            cancellationToken);

        return new GeneratedSpeech(
            audio.ToArray(),
            "audio/mpeg",
            "puentes-response.mp3");
    }
}
