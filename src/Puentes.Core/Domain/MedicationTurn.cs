namespace Puentes.Core.Domain;

public class MedicationTurn
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public TimeOnly? ReferenceTime { get; set; }

    public bool IsActive { get; set; } = true;
}