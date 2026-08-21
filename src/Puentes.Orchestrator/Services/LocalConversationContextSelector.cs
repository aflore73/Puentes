namespace Puentes.Orchestrator.Services;

public static class LocalConversationContextSelector
{
    public static ConversationContextSelection? TrySelect(
        LocalConversationContextInput input)
    {
        var selectedCategory = CompanionProposalModeDetector.FindSelectedCategory(
            input.UserInput, input.OfferedProposalCategories);
        var categoryKind = KindForCategory(selectedCategory);
        if (categoryKind is not null)
        {
            return Selection(input, categoryKind.Value);
        }

        var explicitMode = CompanionProposalModeDetector.Resolve(
            input.UserInput,
            waitingForChoice: false);
        var explicitKind = KindForMode(explicitMode);
        if (explicitKind is not null)
        {
            return Selection(input, explicitKind.Value);
        }

        selectedCategory = input.PendingOffers.Count == 1
            ? input.PendingOffers.First().CategoryCode
            : null;
        categoryKind = KindForCategory(selectedCategory);
        if (categoryKind is not null)
        {
            return Selection(input, categoryKind.Value);
        }

        var proposalMode = CompanionProposalModeDetector.Resolve(
            input.UserInput,
            input.WaitingForProposalChoice);
        if (proposalMode == CompanionProposalMode.CategoriesOnly)
        {
            return Selection(input, ConversationContextKind.Companion);
        }

        // No inferir el tipo de contexto por coincidencias de palabras con los
        // datos almacenados. Una coincidencia léxica puede ser accidental y
        // hacer que se omita el selector semántico. Los turnos que no son una
        // continuación determinística se delegan a IConversationContextSelector.
        return null;
    }

    private static ConversationContextSelection Selection(
        LocalConversationContextInput input,
        ConversationContextKind kind,
        ConversationTimeFrame timeFrame = ConversationTimeFrame.None) => new()
        {
            FocusedPersonName = input.ExplicitFocusedPersonName,
            Kinds = [kind],
            TimeFrame = timeFrame
        };

    private static ConversationContextKind? KindForCategory(string? category) =>
        category switch
        {
            { } value when value.StartsWith("memory.",
                StringComparison.OrdinalIgnoreCase) => ConversationContextKind.Memory,
            { } value when value.StartsWith("interest.",
                StringComparison.OrdinalIgnoreCase) => ConversationContextKind.Preference,
            { } value when value.StartsWith("reading.",
                StringComparison.OrdinalIgnoreCase) => ConversationContextKind.Reading,
            _ => null
        };

    private static ConversationContextKind? KindForMode(
        CompanionProposalMode mode) => mode switch
        {
            CompanionProposalMode.PositiveMemoriesOnly =>
                ConversationContextKind.Memory,
            CompanionProposalMode.InterestsOnly =>
                ConversationContextKind.Preference,
            CompanionProposalMode.ReadingsOnly =>
                ConversationContextKind.Reading,
            CompanionProposalMode.CategoriesOnly =>
                ConversationContextKind.Companion,
            _ => null
        };
}
