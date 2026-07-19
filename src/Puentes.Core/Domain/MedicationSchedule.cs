namespace Puentes.Core.Domain;

public class MedicationSchedule
{
    public Guid Id { get; set; }

    public Guid MedicationId { get; set; }

    public Guid MedicationTurnId { get; set; }

    public bool IsActive { get; set; } = true;
}