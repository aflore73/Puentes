using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.Services;
using Puentes.Shared.Domain;
using Puentes.Shared.Enums;
using Puentes.Shared.Responses.People;

namespace Puentes.Tests;

public sealed class ConversationArchitectureTests
{
    [Fact]
    public void StoreKeepsMultiplePendingOffersWithoutSelectingOne()
    {
        var store = new InMemoryConversationStore();
        var id = store.Create(Guid.NewGuid());

        store.SetPendingOffers(id,
        [
            new DialogueOffer
            {
                Type = DialogueOfferType.Category,
                CategoryCode = "interest.music"
            },
            new DialogueOffer
            {
                Type = DialogueOfferType.Category,
                CategoryCode = "memory.family"
            }
        ]);

        var snapshot = Assert.IsType<ConversationSnapshot>(store.Get(id));
        Assert.Null(snapshot.PendingOffer);
        Assert.Equal(2, snapshot.PendingOffers.Count);
        Assert.Contains(snapshot.PendingOffers,
            offer => offer.CategoryCode == "interest.music");
        Assert.Contains(snapshot.PendingOffers,
            offer => offer.CategoryCode == "memory.family");
    }

    [Fact]
    public void StoreKeepsFocusedPersonAcrossTurns()
    {
        var store = new InMemoryConversationStore();
        var id = store.Create(Guid.NewGuid());

        store.SetFocusedPerson(id, "Daniel Benitez");

        Assert.Equal("Daniel Benitez", store.Get(id)?.FocusedPersonName);
    }

    [Fact]
    public void StoreReplacesPreviousProposalBatchWithLatestCategories()
    {
        var store = new InMemoryConversationStore();
        var id = store.Create(Guid.NewGuid());
        store.SetPendingOffers(id,
        [
            Category("interest.music"),
            Category("reading.religious"),
            Category("memory.family")
        ]);
        store.SetPendingOffers(id,
        [
            Category("interest.plants"),
            Category("reading.poetry"),
            Category("memory.travel")
        ]);

        var recent = Assert.IsType<ConversationSnapshot>(store.Get(id))
            .RecentProposalCategories;
        Assert.Equal(3, recent.Count);
        Assert.DoesNotContain("interest.music", recent);
        Assert.Contains("interest.plants", recent);
    }

    [Fact]
    public void CompanionScopeDoesNotEnableUnrelatedContextKinds()
    {
        var selection = new ConversationContextSelection
        {
            Kinds = [ConversationContextKind.Companion]
        };

        Assert.True(selection.Includes(ConversationContextKind.Companion));
        Assert.False(selection.Includes(ConversationContextKind.Routine));
        Assert.False(selection.Includes(ConversationContextKind.Agenda));
    }

    [Fact]
    public void FocusResolverUsesExplicitNameThenKeepsItForPronounTurn()
    {
        PersonConnectionResponse[] relationships =
        [
            Connection("Mateo Benitez"),
            Connection("Daniel Benitez")
        ];

        var explicitFocus = ConversationFocusResolver.Resolve(
            "Que sabes de Daniel?", "Rosa Benitez", relationships, null);
        var continuedFocus = ConversationFocusResolver.Resolve(
            "Donde trabaja el?", "Rosa Benitez", relationships,
            explicitFocus);

        Assert.Equal("Daniel Benitez", explicitFocus);
        Assert.Equal("Daniel Benitez", continuedFocus);
    }

    [Fact]
    public void PreviousFocusWinsOverSelectorOnContinuationWithoutName()
    {
        var focus = ConversationFocusResolver.ResolveFinal(
            explicitFocus: null,
            previousFocus: "Daniel Benitez",
            selectorFocus: "Rosa Benitez",
            external: false);

        Assert.Equal("Daniel Benitez", focus);
    }

    [Fact]
    public void EvidenceValidatorRejectsPersonAndRoutineOutsideSelectedContext()
    {
        var context = ContextForDaniel();
        var response = new AssistantResponse
        {
            Message = "Mateo esta en entrenamiento.",
            Evidence = new ResponseEvidence
            {
                PersonNames = ["Mateo Benitez"],
                RoutineTitles = ["Entrenamiento"]
            }
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response,
            ["Rosa Benitez", "Mateo Benitez", "Daniel Benitez"]);

        Assert.Contains(errors,
            error => error.Contains("personName fuera del contexto"));
        Assert.Contains(errors,
            error => error.Contains("routineTitle fuera del contexto"));
        Assert.Contains(errors,
            error => error.Contains("persona fuera del contexto"));
    }

    [Fact]
    public void EvidenceValidatorAcceptsSelectedPersonAndRoutine()
    {
        var context = ContextForDaniel();
        var response = new AssistantResponse
        {
            Message = "Daniel suele trabajar desde casa.",
            Evidence = new ResponseEvidence
            {
                PersonNames = ["Daniel"],
                RoutineTitles = ["Trabajo desde casa"]
            }
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response,
            ["Rosa Benitez", "Mateo Benitez", "Daniel Benitez"]);

        Assert.Empty(errors);
    }

    [Fact]
    public void EvidenceValidatorRejectsOvernightInferenceFromRoutine()
    {
        var context = ContextForDaniel();
        var response = new AssistantResponse
        {
            Message = "También pudo haber pasado la noche fuera por esa actividad.",
            Evidence = new ResponseEvidence
            {
                PersonNames = ["Daniel"],
                RoutineTitles = ["Trabajo desde casa"]
            }
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response, ["Rosa Benitez", "Daniel Benitez"]);

        Assert.Contains(errors,
            error => error.Contains("no permite afirmar ni sugerir"));
    }

    [Fact]
    public void EvidenceValidatorAllowsSayingOvernightLocationIsUnknown()
    {
        var context = ContextForDaniel();
        var response = new AssistantResponse
        {
            Message = "No tengo información sobre dónde durmió esa noche.",
            Evidence = new ResponseEvidence
            {
                PersonNames = ["Daniel"],
                RoutineTitles = ["Trabajo desde casa"]
            }
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response, ["Rosa Benitez", "Daniel Benitez"]);

        Assert.Empty(errors);
    }

    [Fact]
    public void EvidenceValidatorRejectsHabitualPresentForLastNight()
    {
        var context = ContextForDaniel();
        var response = new AssistantResponse
        {
            Message = "Anoche Daniel suele trabajar desde casa.",
            Evidence = new ResponseEvidence
            {
                PersonNames = ["Daniel"],
                RoutineTitles = ["Trabajo desde casa"]
            }
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response, ["Rosa Benitez", "Daniel Benitez"]);

        Assert.Contains(errors,
            error => error.Contains("posibilidad pasada"));
    }

    [Fact]
    public void EvidenceValidatorRejectsInventedMessagePurpose()
    {
        var context = ContextForDaniel();
        context.UserInput = "Daniel no volvió anoche.";
        var response = new AssistantResponse
        {
            Message = "Podés escribirle para avisarle cómo te quedaste.",
            Evidence = new ResponseEvidence()
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response, ["Rosa Benitez", "Daniel Benitez"]);

        Assert.Contains(errors,
            error => error.Contains("propósito de un mensaje"));
    }

    [Fact]
    public void EvidenceValidatorRejectsTwoReadingsForOneContentOffer()
    {
        var context = new ConversationContext
        {
            Person = new PersonContext { Name = "Rosa Benitez" },
            MemorySupport = new MemorySupportContext
            {
                SupportContents =
                [
                    new PersonSupportContentContext { Title = "Lectura uno" },
                    new PersonSupportContentContext { Title = "Lectura dos" }
                ]
            }
        };
        var response = new AssistantResponse
        {
            Message = "Lectura uno y Lectura dos.",
            OfferedAction = new DialogueOffer
            {
                Type = DialogueOfferType.Content,
                ContentTitle = "Lectura uno"
            },
            Evidence = new ResponseEvidence
            {
                SupportContentTitles = ["Lectura uno", "Lectura dos"]
            }
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response, ["Rosa Benitez"]);

        Assert.Contains(errors,
            error => error.Contains("más de una lectura"));
    }

    [Fact]
    public void LocalSelectorUsesRoutineWhenOnlyRoutineDataMatches()
    {
        var selection = LocalConversationContextSelector.TrySelect(new()
        {
            UserInput = "Trabaja hoy desde la oficina?",
            ExplicitFocusedPersonName = "Daniel Benitez",
            Routines =
            [
                new PersonRoutineResponse
                {
                    PersonName = "Daniel Benitez",
                    Title = "Trabajo",
                    Notes = "Trabaja desde la oficina martes y miercoles."
                }
            ]
        });

        Assert.NotNull(selection);
        Assert.Equal([ConversationContextKind.Routine], selection.Kinds);
        Assert.Equal(ConversationTimeFrame.Current, selection.TimeFrame);
    }

    [Fact]
    public void LocalSelectorLeavesAmbiguousQuestionForSemanticSelector()
    {
        var selection = LocalConversationContextSelector.TrySelect(new()
        {
            UserInput = "Ezequiel no vino anoche.",
            ExplicitFocusedPersonName = "Ezequiel",
            Routines =
            [
                new PersonRoutineResponse
                {
                    PersonName = "Ezequiel",
                    Title = "Entrenamiento de futbol",
                    Notes = "Lunes, miercoles y viernes de 19 a 23."
                }
            ]
        });

        Assert.Null(selection);
    }

    [Fact]
    public void LocalSelectorResolvesPendingReadingWithoutSemanticCall()
    {
        var selection = LocalConversationContextSelector.TrySelect(new()
        {
            UserInput = "si",
            PendingOffers =
            [
                new DialogueOffer
                {
                    Type = DialogueOfferType.Category,
                    CategoryCode = "reading.religious"
                }
            ]
        });

        Assert.NotNull(selection);
        Assert.Equal([ConversationContextKind.Reading], selection.Kinds);
    }

    [Fact]
    public void ExplicitMemoryRequestOverridesPendingReading()
    {
        var selection = LocalConversationContextSelector.TrySelect(new()
        {
            UserInput = "¿Qué recuerdo tenés?",
            PendingOffers =
            [
                new DialogueOffer
                {
                    Type = DialogueOfferType.Category,
                    CategoryCode = "reading.religious"
                }
            ]
        });

        Assert.NotNull(selection);
        Assert.Equal([ConversationContextKind.Memory], selection.Kinds);
    }

    private static PersonConnectionResponse Connection(string name) => new()
    {
        Type = PersonRelationshipType.Child,
        OtherPerson = new PersonSummaryResponse { Name = name }
    };

    private static DialogueOffer Category(string code) => new()
    {
        Type = DialogueOfferType.Category,
        CategoryCode = code
    };

    private static ConversationContext ContextForDaniel() => new()
    {
        Person = new PersonContext { Name = "Rosa Benitez" },
        State = new ConversationState { FocusedPersonName = "Daniel Benitez" },
        MemorySupport = new MemorySupportContext
        {
            Relationships =
            [
                new PersonRelationshipContext
                {
                    OtherPersonName = "Daniel Benitez"
                }
            ],
            Routines =
            [
                new PersonRoutineContext
                {
                    PersonName = "Daniel Benitez",
                    Title = "Trabajo desde casa"
                }
            ]
        }
    };
}
