using Puentes.Shared.Enums;

namespace Puentes.Shared.Domain;

public class PersonRelationship
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public Guid RelatedPersonId { get; set; }

    public PersonRelationshipType Type { get; set; }

    public string? Notes { get; set; }
}
