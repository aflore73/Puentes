namespace Puentes.Shared.Domain;

public class Event
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public EventType Type { get; set; }
    public string Description { get; set; } = string.Empty;
}
