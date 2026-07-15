namespace Puentes.Core.Domain;

public class Event
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public DateTime OccurredAt { get; set; }
    public EventType Type { get; set; }
    public string Description { get; set; } = string.Empty;
}
