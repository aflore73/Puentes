using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.Services;

namespace Puentes.Tests;

public sealed class ConcreteConcernLocationValidationTests
{
    [Fact]
    public void RejectsSpeculativeLocationForLastNightConcern()
    {
        var context = new ConversationContext
        {
            Person = new PersonContext { Name = "Marta" },
            UserInput = "No dormí anoche esperando a Ezequiel.",
            State = new ConversationState
            {
                FocusedPersonName = "Ezequiel"
            },
            MemorySupport = new MemorySupportContext()
        };
        var response = new AssistantResponse
        {
            Message = "Anoche Ezequiel podía haber estado con Ana en la calle " +
                "Murías de Caseros."
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response, ["Marta", "Ezequiel"]);

        Assert.Contains(errors, error => error.Contains(
            "ubicación posible", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RejectsSuggestionToCheckInferredPlace()
    {
        var context = new ConversationContext
        {
            Person = new PersonContext { Name = "Marta" },
            UserInput = "No dormí anoche esperando a Ezequiel.",
            State = new ConversationState
            {
                FocusedPersonName = "Ezequiel"
            },
            MemorySupport = new MemorySupportContext()
        };
        var response = new AssistantResponse
        {
            Message = "Podés revisar ese lugar como referencia de ese momento."
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response, ["Marta", "Ezequiel"]);

        Assert.Contains(errors, error => error.Contains(
            "No sugieras revisar", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AllowsBriefUnknownLocationAnswer()
    {
        var context = new ConversationContext
        {
            Person = new PersonContext { Name = "Marta" },
            UserInput = "No dormí anoche esperando a Ezequiel.",
            State = new ConversationState
            {
                FocusedPersonName = "Ezequiel"
            },
            MemorySupport = new MemorySupportContext()
        };
        var response = new AssistantResponse
        {
            Message = "No sé dónde estuvo Ezequiel anoche."
        };

        var errors = ResponseEvidenceValidator.Validate(
            context, response, ["Marta", "Ezequiel"]);

        Assert.Empty(errors);
    }
}
