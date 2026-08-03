using Puentes.Shared.Enums;

namespace Puentes.Shared.Requests.People;

public class CreatePersonRelationshipRequest
{
    public Guid RelatedPersonId { get; set; }

    public PersonRelationshipType Type { get; set; }

    public string? Notes { get; set; }
}
