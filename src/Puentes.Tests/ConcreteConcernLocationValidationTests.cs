using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.Services;

namespace Puentes.Tests;

public sealed class ConcreteConcernLocationValidationTests
{
    [Fact]
    public void RejectsSpeculativeLocationForLastNightConcern()
    {
        var context = ContextForFocusedPerson();
        var response = new AssistantResponse
        {
            Message = "Anoche Daniel podía haber estado con otra persona en otro lugar."
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response, ["Rosa Benitez", "Daniel Benitez"]);

        Assert.Contains(errors, error => error.Contains(
            "ubicación posible", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RejectsSuggestionToCheckInferredPlace()
    {
        var context = ContextForFocusedPerson();
        var response = new AssistantResponse
        {
            Message = "Podés revisar ese lugar como referencia de ese momento."
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response, ["Rosa Benitez", "Daniel Benitez"]);

        Assert.Contains(errors, error => error.Contains(
            "No sugieras revisar", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AllowsBriefUnknownLocationAnswer()
    {
        var context = ContextForFocusedPerson();
        var response = new AssistantResponse
        {
            Message = "No sé dónde estuvo Daniel anoche."
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response, ["Rosa Benitez", "Daniel Benitez"]);

        Assert.Empty(errors);
    }

    private static ConversationContext ContextForFocusedPerson() => new()
    {
        Person = new PersonContext { Name = "Rosa Benitez" },
        UserInput = "No dormí anoche esperando a Daniel.",
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
            ]
        }
    };
}
