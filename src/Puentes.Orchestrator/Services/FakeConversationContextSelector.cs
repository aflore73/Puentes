using Puentes.Orchestrator.AI.Models;

namespace Puentes.Orchestrator.Services;

public sealed class FakeConversationContextSelector :
    IConversationContextSelector
{
    public Task<ConversationContextSelection> SelectAsync(
        string userInput,
        IReadOnlyCollection<ConversationHistoryItemContext> history,
        string assistedPersonName,
        IReadOnlyCollection<string> knownPersonNames,
        string? previousFocus,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new ConversationContextSelection
        {
            FocusedPersonName = previousFocus,
            Kinds =
            [
                ConversationContextKind.Relationship,
                ConversationContextKind.Routine,
                ConversationContextKind.Memory,
                ConversationContextKind.Preference,
                ConversationContextKind.Reading,
                ConversationContextKind.Belonging,
                ConversationContextKind.Agenda,
                ConversationContextKind.Companion
            ]
        });
}
