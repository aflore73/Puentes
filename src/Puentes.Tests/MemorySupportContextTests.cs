using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;
using Puentes.Shared.Domain.Knowledge;
using Puentes.Shared.Domain;
using Puentes.Shared.Enums;
using Puentes.Shared.Responses.People;
using Puentes.Shared.Responses.LifeEvents;
using Puentes.Orchestrator.Services;

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
    public void MemoryContextProjectsActivePreferencesWithoutSearchTags()
    {
        var preferences = new List<PersonPreferenceResponse>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PersonId = Guid.NewGuid(),
                PersonName = "Marta",
                Title = "Música",
                Notes = "A Marta le gusta escuchar a Sandro.",
                Tags = "musica,sandro,canciones",
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                PersonId = Guid.NewGuid(),
                PersonName = "Marta",
                Title = "Preferencia anterior",
                Notes = "No debe enviarse.",
                Tags = "inactiva",
                IsActive = false
            }
        };
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "Quiero escuchar música."
        };

        var context = new AiContextBuilderService()
            .BuildConversationContext(request, preferences: preferences);
        var preference = Assert.Single(
            context.MemorySupport!.Preferences);
        var prompt = new PromptFactory().Create(context);

        Assert.Equal("Música", preference.Title);
        Assert.Contains("Sandro", preference.Notes);
        Assert.Contains("\"preferences\"", prompt.UserMessage);
        Assert.DoesNotContain("musica,sandro,canciones", prompt.UserMessage);
        Assert.DoesNotContain("No debe enviarse", prompt.UserMessage);
        Assert.Contains(
            "memorySupport.preferences contiene gustos e intereses",
            prompt.SystemMessage);
    }

    [Fact]
    public void MemoryContextProjectsActiveSupportContentWithoutTagsOrIds()
    {
        var activeId = Guid.NewGuid();
        var contents = new List<PersonSupportContentResponse>
        {
            new()
            {
                Id = activeId,
                PersonId = Guid.NewGuid(),
                PersonName = "Marta",
                Title = "Un texto de consuelo",
                Content = "Contenido elegido para Marta.",
                Attribution = "Autor",
                Reference = "Referencia",
                Tags = "tristeza,consuelo",
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                PersonId = Guid.NewGuid(),
                PersonName = "Marta",
                Title = "Texto inactivo",
                Content = "No debe enviarse.",
                Tags = "inactivo",
                IsActive = false
            }
        };
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "Me siento triste."
        };

        var context = new AiContextBuilderService()
            .BuildConversationContext(request, supportContents: contents);
        var content = Assert.Single(
            context.MemorySupport!.SupportContents);
        var prompt = new PromptFactory().Create(context);

        Assert.Equal("Un texto de consuelo", content.Title);
        Assert.Contains("Contenido elegido", prompt.UserMessage);
        Assert.DoesNotContain(activeId.ToString(), prompt.UserMessage);
        Assert.DoesNotContain("tristeza,consuelo", prompt.UserMessage);
        Assert.DoesNotContain("No debe enviarse", prompt.UserMessage);
        Assert.Contains(
            "no leas el contenido hasta que confirme cuál quiere escuchar",
            prompt.SystemMessage);
        Assert.Contains(
            "conversationHistory muestre que esa lectura concreta fue ofrecida",
            prompt.SystemMessage);
        Assert.Contains(
            "No vuelvas a preguntar si quiere una lectura general",
            prompt.SystemMessage);
    }

    [Fact]
    public void MemoryContextProjectsActiveBelongingsWithoutTagsOrIds()
    {
        var activeId = Guid.NewGuid();
        var belongings = new List<PersonBelongingResponse>
        {
            new()
            {
                Id = activeId,
                PersonId = Guid.NewGuid(),
                PersonName = "Marta",
                Name = "Llaves",
                Notes = "Suelen quedar en el recipiente de la entrada.",
                Tags = "llaves,llavero,entrada",
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                PersonId = Guid.NewGuid(),
                PersonName = "Marta",
                Name = "Objeto inactivo",
                Notes = "No debe enviarse.",
                Tags = "inactivo",
                IsActive = false
            }
        };
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "No encuentro las llaves."
        };

        var context = new AiContextBuilderService()
            .BuildConversationContext(request, belongings: belongings);
        var belonging = Assert.Single(context.MemorySupport!.Belongings);
        var prompt = new PromptFactory().Create(context);

        Assert.Equal("Llaves", belonging.Name);
        Assert.Contains("recipiente de la entrada", prompt.UserMessage);
        Assert.DoesNotContain(activeId.ToString(), prompt.UserMessage);
        Assert.DoesNotContain("llaves,llavero,entrada", prompt.UserMessage);
        Assert.DoesNotContain("Objeto inactivo", prompt.UserMessage);
        Assert.Contains(
            "Sugerí revisar un solo lugar por turno",
            prompt.SystemMessage);
        Assert.Contains(
            "no vuelvas a sugerir un lugar",
            prompt.SystemMessage);
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
            "No uses siempre las mismas palabras",
            prompt.SystemMessage);
        Assert.Contains(
            "un único párrafo de dos o tres oraciones",
            prompt.SystemMessage);
        Assert.Contains(
            "no como una lista, ficha, informe ni resumen de datos",
            prompt.SystemMessage);
        Assert.Contains(
            "lectura en voz alta fluida y natural",
            prompt.SystemMessage);
        Assert.Contains(
            "evitá puntos, puntos suspensivos, saltos de línea o pausas largas",
            prompt.SystemMessage);
        Assert.Contains(
            "dos alternativas si ambas son compatibles con el día y la hora actuales",
            prompt.SystemMessage);
        Assert.Contains(
            "compará de forma obligatoria sus días y horarios",
            prompt.SystemMessage);
        Assert.Contains(
            "no menciones esa actividad los sábados ni los domingos",
            prompt.SystemMessage);
        Assert.Contains(
            "descartala por completo para responder dónde puede estar",
            prompt.SystemMessage);
        Assert.Contains(
            "Integrá esas alternativas en una misma oración",
            prompt.SystemMessage);
        Assert.Contains(
            "no repitas una estructura fija",
            prompt.SystemMessage);
        Assert.Contains(
            "no la repitas salvo que la persona la pregunte de nuevo",
            prompt.SystemMessage);
        Assert.Contains(
            "no desvíes la respuesta hacia su trabajo, domicilio o rutina",
            prompt.SystemMessage);
        Assert.Contains(
            "No inventes explicaciones posibles",
            prompt.SystemMessage);
        Assert.Contains(
            "sin completar la respuesta con frases vagas",
            prompt.SystemMessage);
        Assert.Contains(
            "no lo introduzcas con frases como \"solo tengo la información\"",
            prompt.SystemMessage);
        Assert.Contains(
            "expresiones que sugieran monitoreo",
            prompt.SystemMessage);
        Assert.Contains(
            "información general sobre música, canciones, artistas",
            prompt.SystemMessage);
        Assert.Contains(
            "no digas que el tema, artista o canción no está en el contexto",
            prompt.SystemMessage);
        Assert.Contains(
            "No describas tu manera de acompañar",
            prompt.SystemMessage);
        Assert.Contains(
            "usá voseo de manera consistente",
            prompt.SystemMessage);
        Assert.Contains(
            "no sugieras enviar otro",
            prompt.SystemMessage);
        Assert.Contains(
            "coincida con memorySupport.relationships.otherPersonName",
            prompt.SystemMessage);
        Assert.Contains(
            "histórica, religiosa, ficticia, famosa ni externa al JSON",
            prompt.SystemMessage);
        Assert.Contains(
            "solamente cuando userInput lo pida explícitamente",
            prompt.SystemMessage);
        Assert.Contains(
            "No uses sucesos, rutinas ni características de otra persona",
            prompt.SystemMessage);
        Assert.Contains(
            "no es una lista de personas disponibles para conversar",
            prompt.SystemMessage);
        Assert.Contains(
            "no nombres a ninguna persona como contacto sugerido",
            prompt.SystemMessage);
        Assert.Contains(
            "No propongas hablar de otra persona",
            prompt.SystemMessage);
        Assert.Contains(
            "No uses fórmulas técnicas como",
            prompt.SystemMessage);
        Assert.Contains(
            "Formulá las propuestas con palabras directas y cotidianas",
            prompt.SystemMessage);
        Assert.Contains(
            "No menciones dos o más títulos de lecturas",
            prompt.SystemMessage);
    }

    [Fact]
    public void MemoryContextIncludesRecentConversationHistory()
    {
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "Está lloviendo y él está con la moto."
        };
        var history = new List<ConversationHistoryItemContext>
        {
            new()
            {
                Role = "user",
                Content = "No sé nada de Ezequiel."
            },
            new()
            {
                Role = "assistant",
                Content = "Si querés, podés enviarle un mensaje."
            },
            new()
            {
                Role = "user",
                Content = "Ya le envié mensajes."
            }
        };

        var context = new AiContextBuilderService()
            .BuildConversationContext(
                request,
                conversationHistory: history);
        var prompt = new PromptFactory().Create(context);

        Assert.Equal(3, context.ConversationHistory.Count);
        Assert.Equal(
            "Ya le envié mensajes.",
            context.ConversationHistory[^1].Content);
        Assert.Contains("\"conversationHistory\"", prompt.UserMessage);
        Assert.Contains(
            "no trates cada mensaje como una conversación nueva",
            prompt.SystemMessage);
        Assert.Contains(
            "No repitas preguntas, datos ni sugerencias",
            prompt.SystemMessage);
    }

    [Fact]
    public void ConversationStoreKeepsOnlyTenRecentMessages()
    {
        var store = new InMemoryConversationStore();
        var conversationId = store.Create(Guid.NewGuid());

        for (var turn = 1; turn <= 6; turn++)
        {
            store.AddExchange(
                conversationId,
                $"Usuario {turn}",
                $"Asistente {turn}");
        }

        var snapshot = Assert.IsType<ConversationSnapshot>(
            store.Get(conversationId));

        Assert.Equal(10, snapshot.History.Count);
        Assert.Equal("Usuario 2", snapshot.History[0].Content);
        Assert.Equal("Asistente 6", snapshot.History[^1].Content);
    }

    [Fact]
    public void ConversationStoreKeepsStructuredPendingOffer()
    {
        var store = new InMemoryConversationStore();
        var conversationId = store.Create(Guid.NewGuid());
        store.SetPendingOffer(conversationId, new DialogueOffer
        {
            Type = DialogueOfferType.Category,
            CategoryCode = "reading.religious"
        });

        var snapshot = Assert.IsType<ConversationSnapshot>(
            store.Get(conversationId));

        Assert.Equal(DialogueOfferType.Category, snapshot.PendingOffer?.Type);
        Assert.Equal("reading.religious",
            snapshot.PendingOffer?.CategoryCode);
        Assert.Contains("reading.religious",
            snapshot.RecentProposalCategories);
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
