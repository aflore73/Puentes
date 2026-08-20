using System.Globalization;
using System.Text;

namespace Puentes.Orchestrator.Services;

public static class CompanionProposalModeDetector
{
    public static CompanionProposalMode Resolve(
        string userInput,
        bool waitingForChoice,
        string? selectedCategory = null)
    {
        var text = Normalize(userInput);
        if (!string.IsNullOrWhiteSpace(selectedCategory))
        {
            return selectedCategory.StartsWith("reading.",
                StringComparison.OrdinalIgnoreCase)
                ? CompanionProposalMode.ReadingsOnly
                : selectedCategory.StartsWith("interest.",
                    StringComparison.OrdinalIgnoreCase)
                    ? CompanionProposalMode.InterestsOnly
                    : CompanionProposalMode.PositiveMemoriesOnly;
        }
        var selectedMode = ResolveSelectedCategory(text);
        if (selectedMode is not null)
        {
            return selectedMode.Value;
        }

        if (waitingForChoice)
        {
            return IsRejection(text)
                ? CompanionProposalMode.NoDetails
                : CompanionProposalMode.CategoriesOnly;
        }

        return IsCompanionshipRequest(text)
            ? CompanionProposalMode.CategoriesOnly
            : CompanionProposalMode.FullContext;
    }

    public static string? FindSelectedCategory(
        string userInput,
        IReadOnlyCollection<string> offeredCategories)
    {
        var text = Normalize(userInput);
        foreach (var category in offeredCategories)
        {
            var separator = category.IndexOf('.');
            var value = separator >= 0 ? category[(separator + 1)..] : category;
            var aliases = GetAliases(value);
            if (aliases.Any(alias => text.Contains(alias,
                StringComparison.Ordinal)))
            {
                return category;
            }
        }

        return null;
    }

    public static bool MentionsCategory(string text, string category)
    {
        var normalized = Normalize(text);
        var separator = category.IndexOf('.');
        var value = separator >= 0 ? category[(separator + 1)..] : category;
        return GetAliases(value).Any(alias => normalized.Contains(
            alias, StringComparison.Ordinal));
    }

    private static CompanionProposalMode? ResolveSelectedCategory(string text)
    {
        if (ContainsAny(text, "lectura", "leer", "leeme", "biblia",
            "versiculo", "salmo", "poema"))
        {
            return CompanionProposalMode.ReadingsOnly;
        }

        if (ContainsAny(text, "recuerdo", "recordar", "recordemos", "acordarnos",
            "momento lindo", "historia de mi vida"))
        {
            return CompanionProposalMode.PositiveMemoriesOnly;
        }

        if (ContainsAny(text, "algo que me gusta", "mis gustos", "interes",
            "musica", "cantante", "plantas", "pelicula"))
        {
            return CompanionProposalMode.InterestsOnly;
        }

        return null;
    }

    private static bool IsCompanionshipRequest(string text) =>
        ContainsAny(text,
            "estoy triste",
            "me siento triste",
            "me siento sola",
            "me siento solo",
            "estoy sola",
            "estoy solo",
            "estoy aburrida",
            "estoy aburrido",
            "me siento aburrida",
            "me siento aburrido",
            "estoy angustiada",
            "estoy angustiado",
            "me siento angustiada",
            "me siento angustiado",
            "quiero conversar",
            "quiero hablar con alguien",
            "haceme compania");

    private static bool IsRejection(string text) =>
        text is "no" or "ninguna" or "ninguno" or "ahora no" ||
        text.StartsWith("no quiero", StringComparison.Ordinal);

    private static bool ContainsAny(string text, params string[] values) =>
        values.Any(value => text.Contains(value, StringComparison.Ordinal));

    private static string[] GetAliases(string category) =>
        Normalize(category) switch
        {
            "religious" => ["religiosa", "religioso", "biblia"],
            "poetry" => ["poesia", "poema"],
            "story" => ["historia", "cuento"],
            "travel" => ["viaje", "viajes", "vacaciones"],
            "childhood" => ["infancia", "ninez", "de chica", "de chico"],
            "family" => ["familia", "familiar"],
            "life story" => ["historia de vida", "mi vida"],
            var value => [value]
        };

    private static string Normalize(string value)
    {
        var decomposed = value.ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) !=
                UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.IsLetterOrDigit(character)
                    ? character
                    : ' ');
            }
        }

        return string.Join(' ', builder.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
