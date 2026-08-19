using Puentes.Orchestrator.AI.Models;

namespace Puentes.Orchestrator.Services;

public class MemoryConversationWorkflowService
{
    private readonly ApiClient _apiClient;
    private readonly AiContextBuilderService _contextBuilder;
    private readonly IConversationService _conversationService;
    private readonly InMemoryConversationStore _conversationStore;

    public MemoryConversationWorkflowService(
        ApiClient apiClient,
        AiContextBuilderService contextBuilder,
        IConversationService conversationService,
        InMemoryConversationStore conversationStore)
    {
        _apiClient = apiClient;
        _contextBuilder = contextBuilder;
        _conversationService = conversationService;
        _conversationStore = conversationStore;
    }

    public async Task<AssistantResponse> ProcessAsync(
        Guid personId,
        string userInput,
        CancellationToken cancellationToken = default)
    {
        return await ProcessCoreAsync(
            personId,
            userInput,
            [],
            conversationId: null,
            cancellationToken,
            waitingForProposalChoice: false,
            offeredProposalCategories: [],
            pendingOffer: null,
            recentProposalCategories: []);
    }

    public async Task<MemoryConversationTurnResponse> StartAsync(
        Guid personId,
        string userInput,
        CancellationToken cancellationToken = default,
        string? focusedPersonName = null)
    {
        var conversationId = _conversationStore.Create(personId);

        try
        {
            var response = await ProcessCoreAsync(
                personId,
                userInput,
                [],
                conversationId,
                cancellationToken,
                focusedPersonName,
                waitingForProposalChoice: false,
                offeredProposalCategories: [],
                pendingOffer: null,
                recentProposalCategories: []);

            return new MemoryConversationTurnResponse
            {
                ConversationId = conversationId,
                Response = response
            };
        }
        catch
        {
            _conversationStore.Remove(conversationId);
            throw;
        }
    }

    public async Task<MemoryConversationTurnResponse> ContinueAsync(
        Guid conversationId,
        string userInput,
        CancellationToken cancellationToken = default,
        string? focusedPersonName = null)
    {
        var conversation = _conversationStore.Get(conversationId)
            ?? throw new InvalidOperationException(
                "La conversación no existe o expiró.");
        var response = await ProcessCoreAsync(
            conversation.PersonId,
            userInput,
            conversation.History,
            conversationId,
            cancellationToken,
            conversation.PendingOffer is null ? focusedPersonName : null,
            conversation.WaitingForCompanionProposalChoice,
            conversation.CompanionProposalCategories,
            conversation.PendingOffer,
            conversation.RecentProposalCategories);

        return new MemoryConversationTurnResponse
        {
            ConversationId = conversationId,
            Response = response
        };
    }

    private async Task<AssistantResponse> ProcessCoreAsync(
        Guid personId,
        string userInput,
        IReadOnlyCollection<ConversationHistoryItemContext> history,
        Guid? conversationId,
        CancellationToken cancellationToken,
        string? focusedPersonName = null,
        bool waitingForProposalChoice = false,
        IReadOnlyCollection<string>? offeredProposalCategories = null,
        DialogueOffer? pendingOffer = null,
        IReadOnlyCollection<string>? recentProposalCategories = null)
    {
        var person = await _apiClient.GetPersonAsync(
            personId,
            cancellationToken) ?? throw new InvalidOperationException(
                $"No se encontró la persona {personId}.");
        var relationships = await _apiClient
            .GetPersonRelationshipsAsync(personId, cancellationToken);
        var supportContents = await _apiClient
            .GetPersonSupportContentsAsync(personId, cancellationToken);
        var belongings = await _apiClient
            .GetPersonBelongingsAsync(personId, cancellationToken);
        var trustedContacts = await _apiClient
            .GetPersonTrustedContactsAsync(personId, cancellationToken);
        var agenda = await _apiClient.GetPersonAgendaAsync(
            personId, cancellationToken);
        var lifeEventOwners = relationships
            .Select(relationship => relationship.OtherPerson.Id)
            .Append(personId)
            .Distinct();
        var contextTasks = lifeEventOwners.Select(async ownerId =>
        {
            var lifeEventsTask = _apiClient.GetPersonLifeEventsAsync(
                ownerId, cancellationToken);
            var routinesTask = _apiClient.GetPersonRoutinesAsync(
                ownerId, cancellationToken);
            var preferencesTask = _apiClient.GetPersonPreferencesAsync(
                ownerId, cancellationToken);
            await Task.WhenAll(
                lifeEventsTask,
                routinesTask,
                preferencesTask);
            return (
                LifeEvents: await lifeEventsTask,
                Routines: await routinesTask,
                Preferences: await preferencesTask);
        });
        var contextItems = await Task.WhenAll(contextTasks);
        var lifeEvents = contextItems
            .SelectMany(item => item.LifeEvents)
            .Where(item => focusedPersonName is null || item.PersonName.Equals(
                focusedPersonName,
                StringComparison.OrdinalIgnoreCase))
            .ToList();
        var routines = contextItems
            .SelectMany(item => item.Routines)
            .Where(item => focusedPersonName is null || item.PersonName.Equals(
                focusedPersonName,
                StringComparison.OrdinalIgnoreCase))
            .ToList();
        var preferences = contextItems
            .SelectMany(item => item.Preferences)
            .Where(item => focusedPersonName is null || item.PersonName.Equals(
                focusedPersonName,
                StringComparison.OrdinalIgnoreCase))
            .ToList();
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = userInput
        };
        var selectedProposalCategory =
            CompanionProposalModeDetector.FindSelectedCategory(
                userInput,
                offeredProposalCategories ?? []);
        var proposalMode = CompanionProposalModeDetector.Resolve(
            userInput,
            waitingForProposalChoice,
            selectedProposalCategory);
        var context = _contextBuilder.BuildConversationContext(
            request,
            relationships: relationships,
            lifeEvents: lifeEvents,
            routines: routines,
            preferences: preferences,
            supportContents: supportContents,
            belongings: belongings,
            trustedContacts: trustedContacts,
            agenda: agenda,
            conversationHistory: history,
            person: person,
            companionProposalMode: proposalMode,
            companionProposalCategory: selectedProposalCategory,
            companionProposalCategories: offeredProposalCategories,
            pendingOffer: pendingOffer,
            recentProposalCategories: recentProposalCategories);

        var response = await _conversationService.ProcessAsync(
            context,
            cancellationToken);
        if (MustOfferSuggestedContent(context, response))
        {
            context.State.RequiredDialogueAction =
                RequiredDialogueAction.OfferSuggestedContent;
            response = await _conversationService.ProcessAsync(
                context,
                cancellationToken);
        }
        response.OfferedAction = ValidateOfferedAction(
            response.OfferedAction,
            lifeEvents,
            preferences,
            supportContents);

        if (conversationId is not null)
        {
            _conversationStore.AddExchange(
                conversationId.Value,
                userInput,
                response.Message);
            _conversationStore.SetWaitingForCompanionProposalChoice(
                conversationId.Value,
                proposalMode == CompanionProposalMode.CategoriesOnly,
                context.MemorySupport?.ProposalCandidates
                    .Select(item => item.TopicCode));
            _conversationStore.SetPendingOffer(
                conversationId.Value,
                response.OfferedAction);
        }

        return response;
    }

    private static bool MustOfferSuggestedContent(
        ConversationContext context,
        AssistantResponse response)
    {
        var pending = context.State.PendingOffer;
        return pending?.Type == DialogueOfferType.Category &&
            !string.IsNullOrWhiteSpace(pending.SuggestedContentTitle) &&
            response.PendingOfferDisposition ==
                PendingOfferDisposition.Accepted &&
            (response.OfferedAction.Type != DialogueOfferType.Content ||
                !string.Equals(
                    response.OfferedAction.ContentTitle,
                    pending.SuggestedContentTitle,
                    StringComparison.OrdinalIgnoreCase));
    }

    private static DialogueOffer ValidateOfferedAction(
        DialogueOffer offer,
        IReadOnlyCollection<Puentes.Shared.Responses.LifeEvents.LifeEventResponse>
            lifeEvents,
        IReadOnlyCollection<Puentes.Shared.Responses.People.PersonPreferenceResponse>
            preferences,
        IReadOnlyCollection<Puentes.Shared.Responses.People.PersonSupportContentResponse>
            supportContents)
    {
        if (offer.Type == DialogueOfferType.Category &&
            !string.IsNullOrWhiteSpace(offer.CategoryCode))
        {
            var knownCode = lifeEvents.SelectMany(item => item.TopicCodes)
                .Concat(preferences.SelectMany(item => item.TopicCodes))
                .Concat(supportContents.SelectMany(item => item.TopicCodes))
                .FirstOrDefault(code => code.Equals(
                    offer.CategoryCode,
                    StringComparison.OrdinalIgnoreCase));
            if (knownCode is not null)
            {
                return new DialogueOffer
                {
                    Type = DialogueOfferType.Category,
                    CategoryCode = knownCode
                };
            }
        }

        if (offer.Type == DialogueOfferType.Content &&
            !string.IsNullOrWhiteSpace(offer.ContentTitle))
        {
            var content = supportContents.FirstOrDefault(item =>
                item.IsActive && item.Title.Equals(
                    offer.ContentTitle,
                    StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrWhiteSpace(offer.CategoryCode) ||
                    item.TopicCodes.Contains(
                        offer.CategoryCode,
                        StringComparer.OrdinalIgnoreCase)));
            if (content is not null)
            {
                return new DialogueOffer
                {
                    Type = DialogueOfferType.Content,
                    CategoryCode = content.TopicCodes.FirstOrDefault(code =>
                        string.IsNullOrWhiteSpace(offer.CategoryCode) ||
                        code.Equals(offer.CategoryCode,
                            StringComparison.OrdinalIgnoreCase)),
                    ContentTitle = content.Title
                };
            }
        }

        return new DialogueOffer();
    }
}
