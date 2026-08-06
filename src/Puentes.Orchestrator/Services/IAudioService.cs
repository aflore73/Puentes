namespace Puentes.Orchestrator.Services;

public interface IAudioService
{
    Task<string> TranscribeAsync(
        Stream audio,
        string filename,
        CancellationToken cancellationToken = default);

}
