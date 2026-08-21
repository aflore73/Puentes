using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.Services;
using Puentes.Shared.Responses.People;

namespace Puentes.Tests;

public sealed class ContextSelectionRegressionTests
{
    [Fact]
    public void LexicalMatchDoesNotBypassSemanticSelector()
    {
        var selection = LocalConversationContextSelector.TrySelect(new()
        {
            UserInput = "¿Te acordás de cuando Ezequiel trabajaba en la oficina?",
            ExplicitFocusedPersonName = "Ezequiel",
            Routines =
            [
                new PersonRoutineResponse
                {
                    PersonName = "Ezequiel",
                    Title = "Trabajo",
                    Notes = "Trabaja desde la oficina los martes."
                }
            ]
        });

        Assert.Null(selection);
    }

    [Fact]
    public void DeterministicPendingOfferStillUsesLocalSelector()
    {
        var selection = LocalConversationContextSelector.TrySelect(new()
        {
            UserInput = "sí",
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
    public void NewPersonTopicDoesNotInheritPendingReadingOffer()
    {
        var selection = LocalConversationContextSelector.TrySelect(new()
        {
            UserInput = "De Alejandro no sé nada",
            ExplicitFocusedPersonName = "Alejandro",
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

    [Theory]
    [InlineData("dale")]
    [InlineData("bueno")]
    [InlineData("ok")]
    [InlineData("sí, dale")]
    public void BriefAcceptanceStillUsesPendingOffer(string userInput)
    {
        var selection = LocalConversationContextSelector.TrySelect(new()
        {
            UserInput = userInput,
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
}
