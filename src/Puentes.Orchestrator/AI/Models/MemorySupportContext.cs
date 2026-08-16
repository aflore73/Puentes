using Puentes.Shared.Domain.Knowledge;

namespace Puentes.Orchestrator.AI.Models;
public class MemorySupportContext
{
    public List<MemoryFactContext> Facts { get; set; } = [];

    public List<PersonRelationshipContext> Relationships { get; set; } = [];

    public List<LifeEventContext> LifeEvents { get; set; } = [];

    public List<PersonRoutineContext> Routines { get; set; } = [];

    public List<PersonPreferenceContext> Preferences { get; set; } = [];

    public List<PersonSupportContentContext> SupportContents { get; set; } = [];

    public List<PersonBelongingContext> Belongings { get; set; } = [];

    public List<PersonTrustedContactContext> TrustedContacts { get; set; } = [];

}
