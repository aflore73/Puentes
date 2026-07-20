namespace Puentes.Core.Enums;

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

public enum MedicationRecordStatus
{
    Taken = 1,
    Skipped = 2,
    Postponed = 3
}
public enum MedicationTurnType
{
    Morning = 1,
    Midday = 2,
    Afternoon = 3,
    Night = 4
}