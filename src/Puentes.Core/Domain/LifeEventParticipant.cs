namespace Puentes.Shared.Domain;

public class LifeEventParticipant
{
    public Guid LifeEventId { get; set; }

    public Guid PersonId { get; set; }

    public string? Role { get; set; }
}
