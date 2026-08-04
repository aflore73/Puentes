namespace Puentes.Shared.Domain;

public class LifeEvent
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DatePrecision DatePrecision { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Place { get; set; }

    public bool IsPositiveMemory { get; set; }
}
