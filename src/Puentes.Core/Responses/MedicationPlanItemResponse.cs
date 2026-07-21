using Puentes.Shared.Enums;

namespace Puentes.Shared.Responses;

public class MedicationPlanItemResponse
{
    public string Name { get; set; } = string.Empty;

    //public string Dose { get; set; } = string.Empty;
    public bool SpeakName { get; set; }
    public decimal Quantity { get; set; }
    public MedicationForm Form { get; set; }

    public MedicationShape Shape { get; set; }

    public string Color { get; set; } = string.Empty;
}