namespace Puentes.Orchestrator.AI.Models;

public sealed class PersonTrustedContactContext
{
    public string ContactPersonName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string? Notes { get; set; }
}
