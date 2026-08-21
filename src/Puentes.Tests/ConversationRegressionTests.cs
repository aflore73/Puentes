using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.Services;
using Puentes.Shared.Enums;
using Puentes.Shared.Responses.People;

namespace Puentes.Tests;

public sealed class ConversationRegressionTests
{
    [Fact]
    public void PersonConcernWithPendingReadingIsDeferredToSemanticSelector()
    {
        var selection = LocalConversationContextSelector.TrySelect(new()
        {
            UserInput = "Sí, Daniel no me contesta y no puedo hablar con él.",
            ExplicitFocusedPersonName = "Daniel Benitez",
            PendingOffers =
            [
                new DialogueOffer
                {
                    Type = DialogueOfferType.Category,
                    CategoryCode = "reading.religious"
                }
            ]
        });

        Assert.Null(selection);
    }

    [Fact]
    public void UniqueSpouseReferenceChangesPreviousPersonFocus()
    {
        PersonConnectionResponse[] relationships =
        [
            Child("Daniel Benitez"),
            new PersonConnectionResponse
            {
                Type = PersonRelationshipType.Spouse,
                Direction = RelationshipDirection.Outgoing,
                OtherPerson = new PersonSummaryResponse
                {
                    Name = "Carlos Duarte"
                }
            }
        ];

        var focus = ConversationFocusResolver.Resolve(
            "Mi marido tampoco vino anoche.",
            "Rosa Benitez",
            relationships,
            previousFocus: "Daniel Benitez");

        Assert.Equal("Carlos Duarte", focus);
    }

    [Fact]
    public void AmbiguousChildReferenceDoesNotChooseOneAtRandom()
    {
        PersonConnectionResponse[] relationships =
        [
            Child("Daniel Benitez"),
            Child("Lucas Benitez")
        ];

        var focus = ConversationFocusResolver.Resolve(
            "No sé nada de mi hijo.",
            "Rosa Benitez",
            relationships,
            previousFocus: null);

        Assert.Null(focus);
    }

    [Theory]
    [InlineData("Decime algo de la música de un cantante, hablame de él.")]
    [InlineData("Dime algo de la música de un cantante, háblame de él.")]
    [InlineData("Quisiera información sobre la música de un cantante.")]
    public void TopicInformationRequestsAreNotClassifiedByLocalKeywordRules(
        string userInput)
    {
        var selection = LocalConversationContextSelector.TrySelect(new()
        {
            UserInput = userInput,
            Preferences =
            [
                new PersonPreferenceResponse
                {
                    PersonName = "Rosa Benitez",
                    Title = "Música",
                    Notes = "Le gusta escuchar música popular.",
                    TopicCodes = ["interest.music"]
                }
            ]
        });

        Assert.Null(selection);
    }

    [Fact]
    public void AvoidAssistedPersonNameRejectsThirdPersonAnswer()
    {
        var context = new ConversationContext
        {
            Person = new PersonContext { Name = "Rosa Benitez" },
            State = new ConversationState
            {
                AvoidAssistedPersonName = true
            },
            MemorySupport = new MemorySupportContext()
        };
        var response = new AssistantResponse
        {
            Message = "A Rosa le gusta escuchar música.",
            Evidence = new ResponseEvidence()
        };

        var errors = ResponseEvidenceValidator.Validate(
            context,
            response,
            ["Rosa Benitez"]);

        Assert.Contains(errors,
            error => error.Contains("no uses su nombre"));
    }

    [Fact]
    public void UnknownOvernightLocationMustNotBeInventedFromRoutine()
    {
        var context = new ConversationContext
        {
            Person = new PersonContext { Name = "Rosa Benitez" },
            UserInput = "Daniel no vino anoche.",
            State = new ConversationState
            {
                FocusedPersonName = "Daniel Benitez"
            },
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
                        Title = "Trabajo"
                    }
                ]
            }
        };
        var response = new AssistantResponse
        {
            Message = "Quizá estuvo en el trabajo durante la noche.",
            Evidence = new ResponseEvidence
            {
                PersonNames = ["Daniel"],
                RoutineTitles = ["Trabajo"]
            }
        };

        var errors = ResponseEvidenceValidator.Validate(
            context,
            response,
            ["Rosa Benitez", "Daniel Benitez"]);

        Assert.Contains(errors,
            error => error.Contains("ubicación posible"));
    }

    private static PersonConnectionResponse Child(string name) => new()
    {
        Type = PersonRelationshipType.Child,
        Direction = RelationshipDirection.Incoming,
        OtherPerson = new PersonSummaryResponse { Name = name }
    };
}
