using Puentes.Core.Domain;

namespace Puentes.Core.Requests.Medication;

public class CreateMedicationTurnRequest
{
    public MedicationTurnType Type { get; set; }
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public TimeOnly? ReferenceTime { get; set; }
}