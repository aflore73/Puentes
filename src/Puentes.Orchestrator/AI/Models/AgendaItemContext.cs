using Puentes.Shared.Domain;

namespace Puentes.Orchestrator.AI.Models;

public sealed class AgendaItemContext
{
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Place { get; set; }
    public AgendaItemStatus Status { get; set; }
    public List<string> TopicCodes { get; set; } = [];
    public List<LifeEventParticipantContext> Participants { get; set; } = [];
}
