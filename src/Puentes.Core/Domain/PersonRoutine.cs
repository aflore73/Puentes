namespace Puentes.Shared.Domain;

public class PersonRoutine
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
