using Puentes.Shared.Enums;

namespace Puentes.Shared.Domain;

public class MedicationSchedule
{
    public Guid Id { get; set; }
    public Guid MedicationId { get; set; }
    public MedicationTurnType Turn { get; set; }
    public decimal Quantity { get; set; }
    public bool IsActive { get; set; } = true;
}