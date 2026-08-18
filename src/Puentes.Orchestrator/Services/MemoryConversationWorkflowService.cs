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
            offeredProposalCategories: []);
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
                offeredProposalCategories: []);

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
            focusedPersonName,
            conversation.WaitingForCompanionProposalChoice,
            conversation.CompanionProposalCategories);

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
        IReadOnlyCollection<string>? offeredProposalCategories = null)
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
            companionProposalCategories: offeredProposalCategories);

        var response = await _conversationService.ProcessAsync(
            context,
            cancellationToken);

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
        }

        return response;
    }
}
