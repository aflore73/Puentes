using Puentes.Shared.Domain;

namespace Puentes.Shared.Requests.People;

public sealed class PersonAgendaItemRequest
{
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Place { get; set; }
    public AgendaItemStatus Status { get; set; } = AgendaItemStatus.Scheduled;
    public List<string> TopicCodes { get; set; } = [];
    public List<LifeEventParticipantRequest> Participants { get; set; } = [];
}
