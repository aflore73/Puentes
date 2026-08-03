using Puentes.Shared.Domain.Knowledge;

namespace Puentes.Orchestrator.AI.Models;
public class MemorySupportContext
{
    public List<MemoryFactContext> Facts { get; set; } = [];

    public List<PersonRelationshipContext> Relationships { get; set; } = [];
}
