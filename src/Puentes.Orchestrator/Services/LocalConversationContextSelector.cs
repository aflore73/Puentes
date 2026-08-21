using System.Globalization;
using System.Text;

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

        // Una única oferta pendiente sólo determina el contexto cuando la
        // respuesta es una aceptación breve y no introduce un tema nuevo.
        // Ej.: "sí", "dale", "bueno". Una frase como "De Alejandro no sé nada"
        // debe volver al selector semántico en lugar de heredar Reading/Memory/etc.
        selectedCategory = input.PendingOffers.Count == 1 &&
            IsSimplePendingOfferAcceptance(input.UserInput)
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

        // Expresiones conversacionales como "te acordás" pueden compartir
        // palabras con una rutina almacenada, pero su intención no se puede
        // decidir con una coincidencia léxica. En esos casos dejamos que el
        // selector semántico determine el contexto.
        if (RequiresSemanticSelection(input.UserInput))
            return null;

        var inputTokens = NormalizeTokens(input.UserInput);
        var kinds = new List<ConversationContextKind>();
        AddIfMatches(kinds, ConversationContextKind.Routine, inputTokens,
            input.Routines.Select(item => $"{item.Title} {item.Notes}"));
        AddIfMatches(kinds, ConversationContextKind.Memory, inputTokens,
            input.LifeEvents.Select(item =>
                $"{item.Title} {item.Description} {item.Place} " +
                string.Join(' ', item.TopicCodes)));
        AddIfMatches(kinds, ConversationContextKind.Preference, inputTokens,
            input.Preferences.Select(item =>
                $"{item.Title} {item.Notes} {item.Tags} " +
                string.Join(' ', item.TopicCodes)));
        AddIfMatches(kinds, ConversationContextKind.Reading, inputTokens,
            input.SupportContents.Select(item =>
                $"{item.Title} {item.Reference} {item.Attribution} {item.Tags} " +
                string.Join(' ', item.TopicCodes)));
        AddIfMatches(kinds, ConversationContextKind.Belonging, inputTokens,
            input.Belongings.Select(item =>
                $"{item.Name} {item.Notes} {item.Tags}"));
        AddIfMatches(kinds, ConversationContextKind.Agenda, inputTokens,
            input.Agenda.Select(item =>
                $"{item.Title} {item.Description} {item.Place} " +
                string.Join(' ', item.TopicCodes)));

        if (kinds.Count == 1)
        {
            return Selection(input, kinds[0],
                kinds[0] == ConversationContextKind.Routine
                    ? ResolveRoutineTimeFrame(input.UserInput)
                    : ConversationTimeFrame.None);
        }

        return null;
    }

    private static bool IsSimplePendingOfferAcceptance(string input)
    {
        var normalized = NormalizePhrase(input);
        return normalized is
            "si" or
            "dale" or
            "bueno" or
            "ok" or
            "okay" or
            "esta bien" or
            "si dale" or
            "si quiero" or
            "quiero";
    }

    private static bool RequiresSemanticSelection(string input)
    {
        var tokens = NormalizeTokens(input, minimumLength: 3);
        return tokens.Contains("acordas") ||
            tokens.Contains("acordar") ||
            tokens.Contains("recordas") ||
            tokens.Contains("recordar") ||
            tokens.Contains("recuerdo") ||
            tokens.Contains("recuerdos");
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

    private static void AddIfMatches(
        ICollection<ConversationContextKind> kinds,
        ConversationContextKind kind,
        HashSet<string> inputTokens,
        IEnumerable<string> candidateTexts)
    {
        if (candidateTexts.Any(text => inputTokens.Overlaps(
                NormalizeTokens(text))))
        {
            kinds.Add(kind);
        }
    }

    private static ConversationTimeFrame ResolveRoutineTimeFrame(string input)
    {
        var tokens = NormalizeTokens(input, minimumLength: 3);
        if (tokens.Contains("anoche") || tokens.Contains("ayer"))
            return ConversationTimeFrame.YesterdayEvening;
        if (tokens.Contains("ahora") || tokens.Contains("hoy"))
            return ConversationTimeFrame.Current;
        return ConversationTimeFrame.None;
    }

    private static string NormalizePhrase(string value)
    {
        var decomposed = value.ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) ==
                UnicodeCategory.NonSpacingMark) continue;
            builder.Append(char.IsLetterOrDigit(character) ? character : ' ');
        }

        return string.Join(' ', builder.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static HashSet<string> NormalizeTokens(
        string value,
        int minimumLength = 4)
    {
        return NormalizePhrase(value)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length >= minimumLength)
            .ToHashSet(StringComparer.Ordinal);
    }
}
