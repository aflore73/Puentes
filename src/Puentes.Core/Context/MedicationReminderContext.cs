using Puentes.Shared.Enums;
using Puentes.Shared.Responses;
namespace Puentes.Shared.Contexts;

public class MedicationReminderContext
{
    public string Person { get; set; } = string.Empty;

    public MedicationTurnType Turn { get; set; }

    public List<MedicationPlanItemResponse> Medications { get; set; } = [];
}