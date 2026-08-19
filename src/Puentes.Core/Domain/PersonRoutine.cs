namespace Puentes.Shared.Domain;

public class PersonRoutine
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string? DaysOfWeek { get; set; }

    public string? StartTime { get; set; }

    public string? EndTime { get; set; }

    public bool IsActive { get; set; } = true;
}
