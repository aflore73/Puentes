using Puentes.Shared.Enums;
using Puentes.Shared.Responses.People;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Puentes.Orchestrator.Services;

public static class ConversationFocusResolver
{
    public static string? ResolveFinal(
        string? explicitFocus,
        string? previousFocus,
        string? selectorFocus,
        bool external) =>
        external ? null : explicitFocus ?? previousFocus ?? selectorFocus;

    public static string? Resolve(
        string userInput,
        string assistedPersonName,
        IReadOnlyCollection<PersonConnectionResponse> relationships,
        string? previousFocus)
    {
        var candidates = relationships
            .Select(relationship => relationship.OtherPerson.Name)
            .Append(assistedPersonName);
        foreach (var candidate in candidates)
        {
            var firstName = candidate.Split(' ',
                StringSplitOptions.RemoveEmptyEntries)[0];
            if (Regex.IsMatch(userInput,
                $@"\b{Regex.Escape(firstName)}\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                return candidate;
            }
        }

        var relationshipFocus = ResolveRelationshipReference(
            userInput, relationships);
        return relationshipFocus ?? previousFocus;
    }

    public static bool HasRelationshipReference(string userInput) =>
        RelationshipTypesMentioned(Normalize(userInput)).Count > 0;

    public static bool MentionsRelationshipType(
        string userInput,
        PersonRelationshipType type) =>
        RelationshipTypesMentioned(Normalize(userInput)).Contains(type);

    private static string? ResolveRelationshipReference(
        string userInput,
        IReadOnlyCollection<PersonConnectionResponse> relationships)
    {
        var normalized = Normalize(userInput);
        var requestedTypes = RelationshipTypesMentioned(normalized);
        if (requestedTypes.Count == 0) return null;

        var matches = relationships
            .Where(relationship => requestedTypes.Contains(relationship.Type))
            .Where(RelationshipDescribesOtherPerson)
            .Select(relationship => relationship.OtherPerson.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return matches.Length == 1 ? matches[0] : null;
    }

    private static HashSet<PersonRelationshipType> RelationshipTypesMentioned(
        string normalized)
    {
        var types = new HashSet<PersonRelationshipType>();
        AddIfMentioned(types, PersonRelationshipType.Spouse, normalized,
            "marido", "esposo", "esposa", "conyuge");
        AddIfMentioned(types, PersonRelationshipType.Partner, normalized,
            "pareja");
        AddIfMentioned(types, PersonRelationshipType.Child, normalized,
            "hijo", "hija");
        AddIfMentioned(types, PersonRelationshipType.Parent, normalized,
            "padre", "papa", "madre", "mama");
        AddIfMentioned(types, PersonRelationshipType.Sibling, normalized,
            "hermano", "hermana");
        AddIfMentioned(types, PersonRelationshipType.Grandchild, normalized,
            "nieto", "nieta");
        AddIfMentioned(types, PersonRelationshipType.Grandparent, normalized,
            "abuelo", "abuela");
        AddIfMentioned(types, PersonRelationshipType.Friend, normalized,
            "amigo", "amiga");
        AddIfMentioned(types, PersonRelationshipType.Caregiver, normalized,
            "cuidador", "cuidadora");
        AddIfMentioned(types, PersonRelationshipType.NieceNephew, normalized,
            "sobrino", "sobrina");
        AddIfMentioned(types, PersonRelationshipType.AuntUncle, normalized,
            "tio", "tia");
        AddIfMentioned(types, PersonRelationshipType.Cousin, normalized,
            "primo", "prima");
        AddIfMentioned(types, PersonRelationshipType.Neighbor, normalized,
            "vecino", "vecina");
        return types;
    }

    private static void AddIfMentioned(
        ISet<PersonRelationshipType> types,
        PersonRelationshipType type,
        string normalized,
        params string[] terms)
    {
        if (terms.Any(term => Regex.IsMatch(
                normalized,
                $@"\bmi\s+{Regex.Escape(term)}\b|\b{Regex.Escape(term)}\s+mio\b|\b{Regex.Escape(term)}\s+mia\b",
                RegexOptions.CultureInvariant)))
        {
            types.Add(type);
        }
    }

    private static bool RelationshipDescribesOtherPerson(
        PersonConnectionResponse relationship) =>
        relationship.Direction == RelationshipDirection.Incoming ||
        relationship.Type is PersonRelationshipType.Spouse or
            PersonRelationshipType.Partner or
            PersonRelationshipType.Friend or
            PersonRelationshipType.Neighbor or
            PersonRelationshipType.Cohabitant;

    private static string Normalize(string value)
    {
        var decomposed = value.ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) ==
                UnicodeCategory.NonSpacingMark)
            {
                continue;
            }
            builder.Append(char.IsLetterOrDigit(character) ? character : ' ');
        }
        return string.Join(' ', builder.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
