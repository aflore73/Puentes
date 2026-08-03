using Puentes.Shared.Enums;
using Puentes.Shared.Responses.People;

namespace Puentes.Orchestrator.AI.Models;

public class PersonRelationshipContext
{
    public string OtherPersonName { get; set; } = string.Empty;

    public DateOnly? OtherPersonBirthDate { get; set; }

    public string? OtherPersonResidence { get; set; }

    public PersonRelationshipType Type { get; set; }

    public RelationshipDirection Direction { get; set; }

    public string? Notes { get; set; }
}
