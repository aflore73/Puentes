using Puentes.Shared.Enums;

public class RegisterMedicationRecordRequest
{
    public Guid PatientId { get; set; }

    public MedicationTurnType Turn { get; set; }

    public bool Confirmed { get; set; }

    public string? Notes { get; set; }
}