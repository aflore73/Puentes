using Puentes.Shared.Enums;
namespace Puentes.Shared.Responses;

public class MedicationPlanResponse
{
    public MedicationTurnType Turn { get; set; }

    public List<MedicationPlanItemResponse> Medications { get; set; } = [];
}