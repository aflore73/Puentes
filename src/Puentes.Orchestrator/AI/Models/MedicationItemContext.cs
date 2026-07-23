namespace Puentes.Orchestrator.AI.Models;

public class MedicationItemContext
{
    public string Name { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public bool SpeakName { get; set; }
    public string? Form { get; set; }

    public string? Shape { get; set; }

    public string? Color { get; set; }
}