using Puentes.Shared.Domain;

namespace Puentes.Orchestrator.AI.Models;

public class LifeEventContext
{
    public string PersonName { get; set; } = string.Empty;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DatePrecision DatePrecision { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Place { get; set; }

    public bool IsPositiveMemory { get; set; }

    public List<LifeEventParticipantContext> Participants { get; set; } = [];
}
