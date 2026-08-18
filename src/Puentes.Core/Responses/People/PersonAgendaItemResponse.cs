using Puentes.Shared.Domain;
using Puentes.Shared.Responses.LifeEvents;

namespace Puentes.Shared.Responses.People;

public sealed class PersonAgendaItemResponse
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Place { get; set; }
    public AgendaItemStatus Status { get; set; }
    public List<string> TopicCodes { get; set; } = [];
    public List<LifeEventParticipantResponse> Participants { get; set; } = [];
}
