using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;
using Puentes.Shared.Domain.Knowledge;
using Puentes.Shared.Domain;
using Puentes.Shared.Enums;
using Puentes.Shared.Responses.People;
using Puentes.Shared.Responses.LifeEvents;

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

    [Fact]
    public void MemoryContextIncludesOrderedLifeEventsWithoutInternalIds()
    {
        var eventId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var lifeEvents = new List<LifeEventResponse>
        {
            new()
            {
                Id = eventId,
                PersonId = personId,
                PersonName = "Ezequiel",
                StartDate = new DateOnly(2025, 1, 1),
                DatePrecision = DatePrecision.Year,
                Title = "Mudanza a Caseros",
                Place = "Calle Murías, Caseros",
                IsPositiveMemory = true,
                Participants =
                [
                    new LifeEventParticipantResponse
                    {
                        PersonName = "Ana",
                        Role = "Conviviente"
                    }
                ]
            },
            new()
            {
                Id = Guid.NewGuid(),
                PersonId = personId,
                PersonName = "Ezequiel",
                StartDate = new DateOnly(2024, 1, 1),
                DatePrecision = DatePrecision.Year,
                Title = "Graduación universitaria",
                IsPositiveMemory = true
            }
        };
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "¿Cuándo me mudé con Ana?"
        };

        var context = new AiContextBuilderService()
            .BuildConversationContext(request, lifeEvents: lifeEvents);

        Assert.Equal(
            ["Graduación universitaria", "Mudanza a Caseros"],
            context.MemorySupport!.LifeEvents.Select(item => item.Title));
        var move = context.MemorySupport.LifeEvents[1];
        Assert.Equal(DatePrecision.Year, move.DatePrecision);
        Assert.Equal("Ezequiel", move.PersonName);
        Assert.Equal("Ana", Assert.Single(move.Participants).PersonName);

        var prompt = new PromptFactory().Create(context);
        Assert.Contains("\"lifeEvents\"", prompt.UserMessage);
        Assert.Contains("\"personName\":\"Ana\"", prompt.UserMessage);
        Assert.DoesNotContain(eventId.ToString(), prompt.UserMessage);
        Assert.DoesNotContain(personId.ToString(), prompt.UserMessage);
        Assert.Contains("Respetá datePrecision", prompt.SystemMessage);
    }

    [Fact]
    public void MemoryContextIncludesOnlyActiveRoutinesWithoutInternalIds()
    {
        var routineId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var routines = new List<PersonRoutineResponse>
        {
            new()
            {
                Id = routineId,
                PersonId = personId,
                PersonName = "Ezequiel",
                Title = "Trabajo",
                Notes = "De lunes a viernes trabaja de 9:00 a 18:00 en Nordelta.",
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                PersonId = personId,
                PersonName = "Ezequiel",
                Title = "Rutina anterior",
                Notes = "Ya no corresponde.",
                IsActive = false
            }
        };
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "¿Dónde está Ezequiel?"
        };

        var context = new AiContextBuilderService()
            .BuildConversationContext(request, routines: routines);

        var routine = Assert.Single(context.MemorySupport!.Routines);
        Assert.Equal("Ezequiel", routine.PersonName);
        Assert.Equal("Trabajo", routine.Title);

        var prompt = new PromptFactory().Create(context);
        Assert.Contains("\"routines\"", prompt.UserMessage);
        Assert.Contains("Nordelta", prompt.UserMessage);
        Assert.DoesNotContain("Rutina anterior", prompt.UserMessage);
        Assert.DoesNotContain(routineId.ToString(), prompt.UserMessage);
        Assert.DoesNotContain(personId.ToString(), prompt.UserMessage);
        Assert.Contains(
            "Una rutina no confirma la ubicación actual",
            prompt.SystemMessage);
        Assert.Contains(
            "No digas \"no puedo confirmar dónde está\"",
            prompt.SystemMessage);
        Assert.Contains(
            "podés enviarles un mensaje y cuando puedan te van a contestar",
            prompt.SystemMessage);
        Assert.Contains(
            "un único párrafo de dos o tres oraciones",
            prompt.SystemMessage);
        Assert.Contains(
            "no como una lista, ficha, informe ni resumen de datos",
            prompt.SystemMessage);
        Assert.Contains(
            "hasta dos alternativas tranquilizadoras compatibles con el día y la hora actuales",
            prompt.SystemMessage);
        Assert.Contains(
            "Integrá esas alternativas en una misma oración",
            prompt.SystemMessage);
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
