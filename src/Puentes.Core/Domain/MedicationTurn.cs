namespace Puentes.Core.Enums;

public class MedicationTurn
{
    public Guid Id { get; set; }
    public MedicationTurnType Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public TimeOnly? ReferenceTime { get; set; }

    public bool IsActive { get; set; } = true;
}