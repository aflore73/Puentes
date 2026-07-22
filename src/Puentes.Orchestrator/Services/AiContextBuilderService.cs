using Puentes.Shared.Responses;

public class AiContextBuilderService
{
    public ConversationContext BuildConversationContext(
        ConversationRequest request,
        MedicationPlanResponse? plan = null)
    {
        return new ConversationContext
        {
            Scenario = request.Scenario,
            UserInput = request.UserInput,
            Medication = plan,
            PersonName = "Marta",
            WaitingMedicationConfirmation =
                request.WaitingMedicationConfirmation
        };
    }
}