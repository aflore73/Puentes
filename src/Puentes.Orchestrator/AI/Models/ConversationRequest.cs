using Puentes.Orchestrator.AI.Models;

public class ConversationRequest
{
    public ConversationScenario Scenario { get; set; }

    public string? UserInput { get; set; }

    public bool WaitingMedicationConfirmation { get; set; }
}