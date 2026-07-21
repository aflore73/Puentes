using Puentes.Shared.Enums;

public class MedicationIntake
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public MedicationTurnType Turn { get; set; }

    public DateTime TakenAt { get; set; }

    public bool Confirmed { get; set; }

    public string? Notes { get; set; }
}