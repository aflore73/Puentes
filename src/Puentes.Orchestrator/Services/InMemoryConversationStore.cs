using System.Collections.Concurrent;
using Puentes.Orchestrator.AI.Models;

namespace Puentes.Orchestrator.Services;

public class InMemoryConversationStore
{
    private const int MaximumHistoryItems = 10;
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(30);
    private readonly ConcurrentDictionary<Guid, Session> _sessions = new();

    public Guid Create(Guid personId)
    {
        RemoveExpiredSessions();

        var session = new Session
        {
            Id = Guid.NewGuid(),
            PersonId = personId,
            LastActivityUtc = DateTimeOffset.UtcNow
        };

        _sessions[session.Id] = session;
        return session.Id;
    }

    public ConversationSnapshot? Get(Guid conversationId)
    {
        RemoveExpiredSessions();

        if (!_sessions.TryGetValue(conversationId, out var session))
        {
            return null;
        }

        lock (session.SyncRoot)
        {
            session.LastActivityUtc = DateTimeOffset.UtcNow;
            return new ConversationSnapshot
            {
                PersonId = session.PersonId,
                WaitingForCompanionProposalChoice =
                    session.WaitingForCompanionProposalChoice,
                CompanionProposalCategories =
                    [.. session.CompanionProposalCategories],
                PendingOffer = Clone(session.PendingOffer),
                PendingOffers = session.PendingOffers
                    .Select(offer => Clone(offer)!).ToList(),
                FocusedPersonName = session.FocusedPersonName,
                RecentProposalCategories =
                    [.. session.RecentProposalCategories],
                RecentMemoryIds = [.. session.RecentMemoryIds],
                History = session.Messages
                    .TakeLast(MaximumHistoryItems)
                    .Select(message => new ConversationHistoryItemContext
                    {
                        Role = message.Role,
                        Content = message.Content
                    })
                    .ToList()
            };
        }
    }

    public void AddExchange(
        Guid conversationId,
        string userMessage,
        string assistantMessage)
    {
        if (!_sessions.TryGetValue(conversationId, out var session))
        {
            return;
        }

        lock (session.SyncRoot)
        {
            session.Messages.Add(new ConversationHistoryItemContext
            {
                Role = "user",
                Content = userMessage
            });
            session.Messages.Add(new ConversationHistoryItemContext
            {
                Role = "assistant",
                Content = assistantMessage
            });
            session.LastActivityUtc = DateTimeOffset.UtcNow;

            if (session.Messages.Count > MaximumHistoryItems)
            {
                session.Messages.RemoveRange(
                    0,
                    session.Messages.Count - MaximumHistoryItems);
            }
        }
    }

    public void Remove(Guid conversationId)
    {
        _sessions.TryRemove(conversationId, out _);
    }

    public void SetWaitingForCompanionProposalChoice(
        Guid conversationId,
        bool waiting,
        IEnumerable<string>? categories = null)
    {
        if (!_sessions.TryGetValue(conversationId, out var session))
        {
            return;
        }

        lock (session.SyncRoot)
        {
            session.WaitingForCompanionProposalChoice = waiting;
            session.CompanionProposalCategories.Clear();
            if (waiting && categories is not null)
            {
                session.CompanionProposalCategories.AddRange(categories);
            }
            session.LastActivityUtc = DateTimeOffset.UtcNow;
        }
    }

    public void SetPendingOffer(Guid conversationId, DialogueOffer offer)
    {
        SetPendingOffers(conversationId,
            offer.Type == DialogueOfferType.None ? [] : [offer]);
    }

    public void SetPendingOffers(
        Guid conversationId,
        IEnumerable<DialogueOffer> offers)
    {
        if (!_sessions.TryGetValue(conversationId, out var session))
        {
            return;
        }

        lock (session.SyncRoot)
        {
            session.PendingOffers.Clear();
            session.PendingOffers.AddRange(offers
                .Where(offer => offer.Type != DialogueOfferType.None)
                .Select(offer => Clone(offer)!));
            session.PendingOffer = session.PendingOffers.Count == 1
                ? Clone(session.PendingOffers[0])
                : null;
            var proposedCategories = session.PendingOffers
                .Where(offer => offer.Type == DialogueOfferType.Category &&
                    !string.IsNullOrWhiteSpace(offer.CategoryCode))
                .Select(offer => offer.CategoryCode!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .ToArray();
            if (proposedCategories.Length > 0)
            {
                session.RecentProposalCategories.Clear();
                session.RecentProposalCategories.AddRange(proposedCategories);
            }
            session.LastActivityUtc = DateTimeOffset.UtcNow;
        }
    }

    public void SetFocusedPerson(Guid conversationId, string? personName)
    {
        if (!_sessions.TryGetValue(conversationId, out var session)) return;
        lock (session.SyncRoot)
        {
            session.FocusedPersonName = string.IsNullOrWhiteSpace(personName)
                ? null
                : personName;
            session.LastActivityUtc = DateTimeOffset.UtcNow;
        }
    }

    public void AddRecentMemory(
        Guid conversationId,
        Guid memoryId,
        bool resetCycle)
    {
        if (!_sessions.TryGetValue(conversationId, out var session)) return;

        lock (session.SyncRoot)
        {
            if (resetCycle) session.RecentMemoryIds.Clear();
            session.RecentMemoryIds.Remove(memoryId);
            session.RecentMemoryIds.Add(memoryId);
            session.LastActivityUtc = DateTimeOffset.UtcNow;
        }
    }

    private static DialogueOffer? Clone(DialogueOffer? offer) => offer is null
        ? null
        : new DialogueOffer
        {
            Type = offer.Type,
            CategoryCode = offer.CategoryCode,
            ContentTitle = offer.ContentTitle
        };

    private void RemoveExpiredSessions()
    {
        var expiration = DateTimeOffset.UtcNow - SessionLifetime;

        foreach (var session in _sessions)
        {
            if (session.Value.LastActivityUtc < expiration)
            {
                _sessions.TryRemove(session.Key, out _);
            }
        }
    }

    private sealed class Session
    {
        public Guid Id { get; init; }
        public Guid PersonId { get; init; }
        public DateTimeOffset LastActivityUtc { get; set; }
        public bool WaitingForCompanionProposalChoice { get; set; }
        public List<string> CompanionProposalCategories { get; } = [];
        public DialogueOffer? PendingOffer { get; set; }
        public List<DialogueOffer> PendingOffers { get; } = [];
        public string? FocusedPersonName { get; set; }
        public List<string> RecentProposalCategories { get; } = [];
        public List<Guid> RecentMemoryIds { get; } = [];
        public List<ConversationHistoryItemContext> Messages { get; } = [];
        public object SyncRoot { get; } = new();
    }
}

public class ConversationSnapshot
{
    public Guid PersonId { get; set; }

    public bool WaitingForCompanionProposalChoice { get; set; }

    public List<string> CompanionProposalCategories { get; set; } = [];

    public DialogueOffer? PendingOffer { get; set; }

    public List<DialogueOffer> PendingOffers { get; set; } = [];

    public string? FocusedPersonName { get; set; }

    public List<string> RecentProposalCategories { get; set; } = [];

    public List<Guid> RecentMemoryIds { get; set; } = [];

    public List<ConversationHistoryItemContext> History { get; set; } = [];
}
