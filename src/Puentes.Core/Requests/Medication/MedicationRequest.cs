using Puentes.Core.Enums;

namespace Puentes.Core.Requests.Medication;

public class MedicationRequest
{
    public string Name { get; set; } = string.Empty;

    public string Dose { get; set; } = string.Empty;

    public MedicationForm Form { get; set; }

    public MedicationShape Shape { get; set; }

    public string? Color { get; set; }

    public string? Instructions { get; set; }

    public bool IsActive { get; set; } = true;
}