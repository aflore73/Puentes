using Puentes.Orchestrator.AI.Models;

namespace Puentes.Orchestrator.Services;

public class MemoryConversationWorkflowService
{
    private readonly ApiClient _apiClient;
    private readonly AiContextBuilderService _contextBuilder;
    private readonly IConversationService _conversationService;

    public MemoryConversationWorkflowService(
        ApiClient apiClient,
        AiContextBuilderService contextBuilder,
        IConversationService conversationService)
    {
        _apiClient = apiClient;
        _contextBuilder = contextBuilder;
        _conversationService = conversationService;
    }

    public async Task<AssistantResponse> ProcessAsync(
        Guid personId,
        string userInput,
        CancellationToken cancellationToken = default)
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
        var lifeEvents = new List<Puentes.Shared.Responses.LifeEvents.LifeEventResponse>();
        var routines = new List<Puentes.Shared.Responses.People.PersonRoutineResponse>();

        foreach (var ownerId in lifeEventOwners)
        {
            lifeEvents.AddRange(await _apiClient
                .GetPersonLifeEventsAsync(ownerId, cancellationToken));
            routines.AddRange(await _apiClient
                .GetPersonRoutinesAsync(ownerId, cancellationToken));
        }
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
            person: person);

        return await _conversationService.ProcessAsync(
            context,
            cancellationToken);
    }
}
