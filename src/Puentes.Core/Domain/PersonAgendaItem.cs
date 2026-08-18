namespace Puentes.Shared.Domain;

public sealed class PersonAgendaItem
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Place { get; set; }
    public AgendaItemStatus Status { get; set; } = AgendaItemStatus.Scheduled;
}

public enum AgendaItemStatus
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2
}
