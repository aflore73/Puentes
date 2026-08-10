namespace Puentes.Orchestrator.AI;

public sealed class OpenAiRealtimeOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gpt-realtime-2.1-mini";

    public string Voice { get; set; } = "marin";
}
