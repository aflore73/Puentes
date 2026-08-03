namespace Puentes.Orchestrator.AI.Models;

public class MemoryFactContext
{
    public string Topic { get; set; } = string.Empty;

    public string CurrentSituation { get; set; } = string.Empty;

    public List<string> PositiveMemories { get; set; } = [];

    public string? SuggestedAction { get; set; }
}
