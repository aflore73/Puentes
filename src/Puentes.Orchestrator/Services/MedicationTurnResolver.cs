using Puentes.Shared.Enums;

namespace Puentes.Orchestrator.Services;

public static class MedicationTurnResolver
{
    public static MedicationTurnType Resolve(DateTime dateTime) =>
        dateTime.Hour switch
        {
            >= 6 and < 12 => MedicationTurnType.Morning,
            >= 12 and < 15 => MedicationTurnType.Midday,
            >= 15 and < 20 => MedicationTurnType.Afternoon,
            _ => MedicationTurnType.Night
        };
}
