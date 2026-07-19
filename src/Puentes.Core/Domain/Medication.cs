namespace Puentes.Core.Domain;

public enum MedicationShape
{
    Unknown = 0,
    Round = 1,
    Oval = 2,
    Oblong = 3,
    Capsule = 4,
    Other = 99
}
public enum MedicationForm
{
    Unknown = 0,
    Tablet = 1,      // Comprimido
    Capsule = 2,     // Cápsula
    Syrup = 3,       // Jarabe
    Drops = 4,       // Gotas
    Injection = 5,   // Inyección
    Cream = 6,       // Crema
    Ointment = 7,    // Pomada
    Spray = 8,       // Aerosol
    Inhaler = 9,     // Inhalador
    Patch = 10,      // Parche
    Other = 99
}
public class Medication
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Dose { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public MedicationShape Shape { get; set; }
    public MedicationForm Form { get; set; }
    public string Color { get; set; } = string.Empty;   

    public string? Instructions { get; set; }

    public bool IsActive { get; set; } = true;
}