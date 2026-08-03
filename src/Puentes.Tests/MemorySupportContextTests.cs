using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;
using Puentes.Shared.Domain.Knowledge;
using Puentes.Shared.Domain;
using Puentes.Shared.Enums;
using Puentes.Shared.Responses.People;

namespace Puentes.Tests;

public class MemorySupportContextTests
{
    [Fact]
    public void BuilderIncludesOnlyActiveFactsOrderedByPriority()
    {
        var facts = new List<MemoryFact>
        {
            CreateFact("familia", priority: 2, isActive: true),
            CreateFact("casa anterior", priority: 1, isActive: false),
            CreateFact("trabajo", priority: 5, isActive: true)
        };
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "Extraño mi trabajo"
        };

        var context = new AiContextBuilderService()
            .BuildConversationContext(request, memoryFacts: facts);

        Assert.NotNull(context.MemorySupport);
        Assert.Equal(
            ["trabajo", "familia"],
            context.MemorySupport.Facts.Select(fact => fact.Topic));
        Assert.Null(context.Medication);
    }

    [Fact]
    public void MemoryPromptContainsScenarioRulesAndSafeFactProjection()
    {
        var fact = CreateFact("familia", priority: 10, isActive: true);
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "¿Dónde está Ana?"
        };
        var context = new AiContextBuilderService()
            .BuildConversationContext(request, memoryFacts: [fact]);

        var prompt = new PromptFactory().Create(context);

        Assert.Contains(
            "ayudar a la persona cuando habla de recuerdos",
            prompt.SystemMessage);
        Assert.Contains("\"memorySupport\"", prompt.UserMessage);
        Assert.Contains("\"topic\":\"familia\"", prompt.UserMessage);
        Assert.DoesNotContain(fact.Id.ToString(), prompt.UserMessage);
        Assert.DoesNotContain("keywords", prompt.UserMessage);
        Assert.DoesNotContain("priority", prompt.UserMessage);
        Assert.DoesNotContain("medication", prompt.UserMessage);
    }

    [Fact]
    public void NonMemoryScenarioDoesNotExposeMemoryFacts()
    {
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.GeneralConversation
        };

        var context = new AiContextBuilderService()
            .BuildConversationContext(
                request,
                memoryFacts: [CreateFact("familia", 1, true)]);

        Assert.Null(context.MemorySupport);
    }

    [Fact]
    public void MemoryContextIncludesRelationshipDirectionAndOtherPerson()
    {
        var marta = new Person
        {
            Id = Guid.NewGuid(),
            Name = "Marta",
            BirthDate = new DateTime(1950, 7, 1)
        };
        var connection = new PersonConnectionResponse
        {
            RelationshipId = Guid.NewGuid(),
            Type = PersonRelationshipType.Child,
            Direction = RelationshipDirection.Incoming,
            OtherPerson = new PersonSummaryResponse
            {
                Id = Guid.NewGuid(),
                Name = "Ezequiel",
                BirthDate = new DateTime(1991, 4, 4),
                City = "Caseros",
                Province = "Buenos Aires",
                Country = "Argentina"
            },
            Notes = "Ezequiel es hijo de Marta."
        };
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "¿Dónde vive mi hijo?"
        };

        var context = new AiContextBuilderService()
            .BuildConversationContext(
                request,
                relationships: [connection],
                person: marta);

        var relationship = Assert.Single(
            context.MemorySupport!.Relationships);
        Assert.Equal("Ezequiel", relationship.OtherPersonName);
        Assert.Equal(PersonRelationshipType.Child, relationship.Type);
        Assert.Equal(RelationshipDirection.Incoming, relationship.Direction);
        Assert.Equal(
            "Caseros, Buenos Aires, Argentina",
            relationship.OtherPersonResidence);
    }

    private static MemoryFact CreateFact(
        string topic,
        int priority,
        bool isActive) => new()
    {
        Id = Guid.NewGuid(),
        Topic = topic,
        Priority = priority,
        IsActive = isActive,
        Keywords = [topic, "recuerdo"],
        CurrentSituation = "Es información confirmada por la familia.",
        PositiveMemories = ["Un recuerdo positivo."],
        SuggestedAction = "Mirar el álbum familiar."
    };
}
