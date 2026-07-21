using Puentes.Shared.Enums;
namespace Puentes.Shared.Domain;

public class MedicationRecord
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public MedicationTurnType Turn { get; set; }

    public DateTimeOffset RecordedAt { get; set; }

    public bool Confirmed { get; set; }

    public string? Notes { get; set; }
}