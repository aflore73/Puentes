namespace Puentes.Orchestrator.Services;

public static class LocalConversationContextSelector
{
    public static ConversationContextSelection? TrySelect(
        LocalConversationContextInput input)
    {
        // El selector local sólo resuelve estado conversacional explícito.
        // La intención expresada en lenguaje natural se deja al selector
        // semántico para no limitar el sistema a listas de palabras o frases.
        var selectedCategory = CompanionProposalModeDetector.FindSelectedCategory(
            input.UserInput, input.OfferedProposalCategories);
        var categoryKind = KindForCategory(selectedCategory);
        if (categoryKind is not null)
        {
            return Selection(input, categoryKind.Value);
        }

        // Una única oferta pendiente sólo determina el contexto ante una
        // aceptación breve. Cualquier respuesta con contenido propio se deja al
        // selector semántico para que el tema actual tenga prioridad.
        if (input.PendingOffers.Count == 1 &&
            CompanionProposalModeDetector.IsSimpleAcceptance(input.UserInput))
        {
            categoryKind = KindForCategory(
                input.PendingOffers.First().CategoryCode);
            if (categoryKind is not null)
            {
                return Selection(input, categoryKind.Value);
            }
        }

        if (input.WaitingForProposalChoice &&
            CompanionProposalModeDetector.IsSimpleAcceptance(input.UserInput))
        {
            return Selection(input, ConversationContextKind.Companion);
        }

        return null;
    }

    private static ConversationContextSelection Selection(
        LocalConversationContextInput input,
        ConversationContextKind kind) => new()
        {
            FocusedPersonName = input.ExplicitFocusedPersonName,
            Kinds = [kind],
            TimeFrame = ConversationTimeFrame.None
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
}
