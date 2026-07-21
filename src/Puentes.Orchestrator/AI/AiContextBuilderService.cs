using Puentes.Shared.Contexts;
using Puentes.Shared.Responses;

namespace Puentes.Orchestrator.AI;

public class AiContextBuilderService
{
    public MedicationReminderContext Build(
        MedicationPlanResponse plan)
    {
        return new MedicationReminderContext
        {
            Person = "Marta",
            Turn = plan.Turn,
            Medications = plan.Medications
        };
    }
}