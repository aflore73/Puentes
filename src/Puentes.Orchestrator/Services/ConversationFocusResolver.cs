using Puentes.Shared.Responses.People;
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

        return previousFocus;
    }
}
