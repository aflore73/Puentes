namespace Puentes.Shared.Enums;

public class MedicationRecord
{
    public Guid Id { get; set; }

    public Guid MedicationId { get; set; }

    public Guid MedicationTurnId { get; set; }

    public DateTimeOffset TakenAt { get; set; }

    public string? Notes { get; set; }
    public MedicationRecordStatus Status { get; set; }
}