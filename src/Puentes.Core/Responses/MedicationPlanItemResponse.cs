namespace Puentes.Shared.Responses;

public class MedicationPlanItemResponse
{
    public string Name { get; set; } = string.Empty;

    public string Dose { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string Form { get; set; } = string.Empty;

    public string Shape { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;
}