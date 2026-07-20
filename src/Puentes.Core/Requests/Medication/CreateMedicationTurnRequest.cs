using Puentes.Shared.Enums;

namespace Puentes.Shared.Requests.Medication;

public class CreateMedicationTurnRequest
{
    public MedicationTurnType Type { get; set; }
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public TimeOnly? ReferenceTime { get; set; }
}