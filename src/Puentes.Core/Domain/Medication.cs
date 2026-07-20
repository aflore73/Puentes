namespace Puentes.Shared.Enums;

public class Medication
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Dose { get; set; } = string.Empty;
    public MedicationShape Shape { get; set; }
    public MedicationForm Form { get; set; }
    public string Color { get; set; } = string.Empty;   

    public string? Instructions { get; set; }

    public bool IsActive { get; set; } = true;
}