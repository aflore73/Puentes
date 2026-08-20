using Puentes.Orchestrator.AI.Models;
using Puentes.Shared.Responses.LifeEvents;
using Puentes.Shared.Responses.People;

namespace Puentes.Orchestrator.Services;

public enum ConversationContextKind
{
    Relationship,
    Routine,
    Memory,
    Preference,
    Reading,
    Belonging,
    Agenda,
    Companion,
    External,
    None
}

public enum ConversationTimeFrame
{
    None,
    Current,
    YesterdayEvening,
    Other
}

public sealed class ConversationContextSelection
{
    public string? FocusedPersonName { get; set; }
    public List<ConversationContextKind> Kinds { get; set; } = [];
    public ConversationTimeFrame TimeFrame { get; set; }

    public bool Includes(ConversationContextKind kind) =>
        Kinds.Contains(kind);
}

public interface IConversationContextSelector
{
    Task<ConversationContextSelection> SelectAsync(
        string userInput,
        IReadOnlyCollection<ConversationHistoryItemContext> history,
        string assistedPersonName,
        IReadOnlyCollection<string> knownPersonNames,
        string? previousFocus,
        CancellationToken cancellationToken = default);
}

public sealed class LocalConversationContextInput
{
    public required string UserInput { get; init; }
    public string? ExplicitFocusedPersonName { get; init; }
    public bool WaitingForProposalChoice { get; init; }
    public IReadOnlyCollection<string> OfferedProposalCategories { get; init; } = [];
    public IReadOnlyCollection<DialogueOffer> PendingOffers { get; init; } = [];
    public IReadOnlyCollection<PersonRoutineResponse> Routines { get; init; } = [];
    public IReadOnlyCollection<LifeEventResponse> LifeEvents { get; init; } = [];
    public IReadOnlyCollection<PersonPreferenceResponse> Preferences { get; init; } = [];
    public IReadOnlyCollection<PersonSupportContentResponse> SupportContents { get; init; } = [];
    public IReadOnlyCollection<PersonBelongingResponse> Belongings { get; init; } = [];
    public IReadOnlyCollection<PersonAgendaItemResponse> Agenda { get; init; } = [];
}
