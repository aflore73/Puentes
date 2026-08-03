namespace Puentes.Orchestrator.AI.Models;

public class ConversationContext
{
    public PersonContext Person { get; set; } = new();

    public EnvironmentContext Environment { get; set; } = new();

    public ConversationScenario Scenario { get; set; }

    public MedicationContext? Medication { get; set; }

    public MemorySupportContext? MemorySupport { get; set; }

    public ConversationState State { get; set; } = new();

    public string? UserInput { get; set; }
}
