namespace Puentes.Shared.Domain.Knowledge;

public class MemoryFact : KnowledgeFact
{
    public string CurrentSituation { get; set; } = string.Empty;

    public List<string> PositiveMemories { get; set; } = [];

    public string? SuggestedAction { get; set; }
}