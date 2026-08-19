using Puentes.Shared.Responses.LifeEvents;

namespace Puentes.Orchestrator.Services;

public static class MemoryCandidateSelector
{
    public static MemorySelection? Select(
        IReadOnlyCollection<LifeEventResponse> lifeEvents,
        string assistedPersonName,
        string categoryCode,
        IReadOnlyCollection<Guid> recentMemoryIds,
        Random? random = null)
    {
        var candidates = lifeEvents.Where(item =>
                item.IsPositiveMemory &&
                item.PersonName.Equals(assistedPersonName,
                    StringComparison.OrdinalIgnoreCase) &&
                item.TopicCodes.Contains(categoryCode,
                    StringComparer.OrdinalIgnoreCase))
            .ToArray();
        if (candidates.Length == 0) return null;

        var recent = recentMemoryIds.ToHashSet();
        var unused = candidates.Where(item => !recent.Contains(item.Id))
            .ToArray();
        var resetCycle = unused.Length == 0;
        var pool = resetCycle ? candidates : unused;
        var selected = pool[(random ?? Random.Shared).Next(pool.Length)];
        return new MemorySelection(selected, resetCycle);
    }
}

public sealed record MemorySelection(
    LifeEventResponse LifeEvent,
    bool ResetCycle);
