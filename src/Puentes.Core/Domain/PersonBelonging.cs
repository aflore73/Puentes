namespace Puentes.Shared.Domain;

public class PersonBelonging
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string? Tags { get; set; }

    public bool IsActive { get; set; } = true;
}
