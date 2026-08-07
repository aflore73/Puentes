namespace Puentes.Orchestrator.Services;

public interface IStreamingSpeechSynthesisService
{
    IAsyncEnumerable<byte[]> GenerateSpeechStreamAsync(
        string text,
        CancellationToken cancellationToken = default);
}
