using Puentes.Core.Enums;

namespace Puentes.Api.Requests.Medications;

public class MedicationScheduleItemRequest
{
    public MedicationTurnType Turn { get; set; }

    public decimal Quantity { get; set; }
}