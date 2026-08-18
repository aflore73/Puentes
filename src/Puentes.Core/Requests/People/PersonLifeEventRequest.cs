using Puentes.Shared.Domain;

namespace Puentes.Shared.Requests.People;

public sealed class PersonLifeEventRequest
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DatePrecision DatePrecision { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Place { get; set; }
    public bool IsPositiveMemory { get; set; }
    public List<string> TopicCodes { get; set; } = [];
    public List<LifeEventParticipantRequest> Participants { get; set; } = [];
}

public sealed class LifeEventParticipantRequest
{
    public Guid PersonId { get; set; }
    public string? Role { get; set; }
}
