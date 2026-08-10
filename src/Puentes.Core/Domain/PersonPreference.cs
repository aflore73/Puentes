namespace Puentes.Shared.Domain;

public class PersonPreference
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string? Tags { get; set; }

    public bool IsActive { get; set; } = true;
}
