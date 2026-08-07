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
            cancellationToken);
    }

    public async Task<MemoryConversationTurnResponse> StartAsync(
        Guid personId,
        string userInput,
        CancellationToken cancellationToken = default)
    {
        var conversationId = _conversationStore.Create(personId);

        try
        {
            var response = await ProcessCoreAsync(
                personId,
                userInput,
                [],
                conversationId,
                cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        var conversation = _conversationStore.Get(conversationId)
            ?? throw new InvalidOperationException(
                "La conversación no existe o expiró.");
        var response = await ProcessCoreAsync(
            conversation.PersonId,
            userInput,
            conversation.History,
            conversationId,
            cancellationToken);

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
        CancellationToken cancellationToken)
    {
        var person = await _apiClient.GetPersonAsync(
            personId,
            cancellationToken) ?? throw new InvalidOperationException(
                $"No se encontró la persona {personId}.");
        var relationships = await _apiClient
            .GetPersonRelationshipsAsync(personId, cancellationToken);
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
            await Task.WhenAll(lifeEventsTask, routinesTask);
            return (
                LifeEvents: await lifeEventsTask,
                Routines: await routinesTask);
        });
        var contextItems = await Task.WhenAll(contextTasks);
        var lifeEvents = contextItems
            .SelectMany(item => item.LifeEvents)
            .ToList();
        var routines = contextItems
            .SelectMany(item => item.Routines)
            .ToList();
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = userInput
        };
        var context = _contextBuilder.BuildConversationContext(
            request,
            relationships: relationships,
            lifeEvents: lifeEvents,
            routines: routines,
            conversationHistory: history,
            person: person);

        var response = await _conversationService.ProcessAsync(
            context,
            cancellationToken);

        if (conversationId is not null)
        {
            _conversationStore.AddExchange(
                conversationId.Value,
                userInput,
                response.Message);
        }

        return response;
    }
}
