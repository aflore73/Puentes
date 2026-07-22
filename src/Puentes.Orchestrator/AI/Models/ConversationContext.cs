using Puentes.Orchestrator.AI.Models;
using Puentes.Shared.Responses;

public class ConversationContext
{
    public string PersonName { get; set; } = "";

    public ConversationScenario Scenario { get; set; } = new ConversationScenario();

    public MedicationPlanResponse? Medication { get; set; }

    public string? UserInput { get; set; }

    public bool WaitingMedicationConfirmation { get; set; }
}