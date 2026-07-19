using Puentes.Core.Domain;

namespace Puentes.Core.Requests.Medication;

public class CreateMedicationRequest
{
    public string Name { get; set; } = string.Empty;

    public string Dose { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public MedicationForm Form { get; set; }

    public MedicationShape Shape { get; set; }

    public string? Color { get; set; }

    public string? Instructions { get; set; }
}