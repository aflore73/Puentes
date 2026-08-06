namespace Puentes.Orchestrator.Services;

public interface ISpeechSynthesisService
{
    Task<GeneratedSpeech> GenerateSpeechAsync(
        string text,
        CancellationToken cancellationToken = default);
}

public sealed record GeneratedSpeech(
    byte[] Content,
    string ContentType,
    string FileName);
