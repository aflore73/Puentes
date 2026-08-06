namespace Puentes.Orchestrator.AI;

public sealed class WhisperOptions
{
    public string ModelPath { get; set; } = "models/ggml-base.bin";

    public string Language { get; set; } = "es";

    public string InitialPrompt { get; set; } = string.Empty;
}
