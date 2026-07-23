using Puentes.Shared.Responses;
using Puentes.Orchestrator.AI.Models;

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

            Person = new PersonContext
            {
                Name = "Marta",
                BirthDate = new DateOnly(1950, 7, 1),
                Language = "es-AR"
            },

            Environment = new EnvironmentContext
            {
                CurrentDateTime = DateTime.Now
            },

            Medication = BuildMedicationContext(plan),

            State = new ConversationState
            {
                WaitingMedicationConfirmation =
                    request.WaitingMedicationConfirmation,

                ReminderAlreadySent = false
            }
        };
    }

    private static MedicationContext? BuildMedicationContext(
        MedicationPlanResponse? plan)
    {
        if (plan is null)
        {
            return null;
        }

        return new MedicationContext
        {
            Turn = plan.Turn,

            Medications = plan.Medications
                .Select(medication => new MedicationItemContext
                {
                    Name = medication.Name,
                    Quantity = medication.Quantity,
                    SpeakName = medication.SpeakName,
                    Form = medication.Form.ToString(),
                    Shape = medication.Shape.ToString(),
                    Color = medication.Color.ToString()
                })
                .ToList()
        };
    }
}