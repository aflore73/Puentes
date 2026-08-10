namespace Puentes.Orchestrator.Services;

public sealed record RealtimeSessionContext(
    Guid PersonId,
    string AssistedPersonName,
    string Instructions,
    IReadOnlyList<RealtimeKnownPerson> KnownPeople);

public sealed record RealtimeKnownPerson(string Name, string Description);
