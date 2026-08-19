using Puentes.Orchestrator.Services;
using Puentes.Shared.Responses.LifeEvents;

namespace Puentes.Tests;

public sealed class MemoryCandidateSelectorTests
{
    [Fact]
    public void SelectsUnusedMemoryFromRequestedCategory()
    {
        var first = Memory("Primero", "memory.family");
        var second = Memory("Segundo", "memory.family");
        var third = Memory("Tercero", "memory.family");

        var selection = MemoryCandidateSelector.Select(
            [first, second, third],
            "Marta",
            "memory.family",
            [first.Id, second.Id],
            new Random(1));

        Assert.NotNull(selection);
        Assert.Equal(third.Id, selection.LifeEvent.Id);
        Assert.False(selection.ResetCycle);
    }

    [Fact]
    public void IgnoresMemoriesFromOtherCategoriesOrPeople()
    {
        var expected = Memory("Familia", "memory.family");
        var travel = Memory("Viaje", "memory.travel");
        var anotherPerson = Memory("Otro", "memory.family");
        anotherPerson.PersonName = "Ezequiel";

        var selection = MemoryCandidateSelector.Select(
            [travel, anotherPerson, expected],
            "Marta",
            "memory.family",
            [],
            new Random(1));

        Assert.NotNull(selection);
        Assert.Equal(expected.Id, selection.LifeEvent.Id);
    }

    [Fact]
    public void StartsNewCycleAfterAllCandidatesWereUsed()
    {
        var first = Memory("Primero", "memory.family");
        var second = Memory("Segundo", "memory.family");

        var selection = MemoryCandidateSelector.Select(
            [first, second],
            "Marta",
            "memory.family",
            [first.Id, second.Id],
            new Random(1));

        Assert.NotNull(selection);
        Assert.True(selection.ResetCycle);
        Assert.Contains(selection.LifeEvent.Id, new[] { first.Id, second.Id });
    }

    private static LifeEventResponse Memory(string title, string topic) =>
        new()
        {
            Id = Guid.NewGuid(),
            PersonName = "Marta",
            Title = title,
            TopicCodes = [topic],
            IsPositiveMemory = true
        };
}
