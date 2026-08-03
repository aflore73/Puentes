using Puentes.Shared.Enums;

namespace Puentes.Shared.Responses.People;

public class PersonConnectionResponse
{
    public Guid RelationshipId { get; set; }

    public PersonRelationshipType Type { get; set; }

    public RelationshipDirection Direction { get; set; }

    public PersonSummaryResponse OtherPerson { get; set; } = new();

    public string? Notes { get; set; }
}
