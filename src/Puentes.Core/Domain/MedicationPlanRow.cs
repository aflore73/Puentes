using Puentes.Shared.Enums;

public class MedicationPlanRow
{
    public MedicationTurnType Turn { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool SpeakName { get; set; }
    public string Dose { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public MedicationForm Form { get; set; }

    public MedicationShape Shape { get; set; }

    public string Color { get; set; } = string.Empty;
}