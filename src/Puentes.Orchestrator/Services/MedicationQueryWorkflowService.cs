using Puentes.Orchestrator.AI.Models;
using Puentes.Shared.Responses;

namespace Puentes.Orchestrator.Services;

public sealed class MedicationQueryWorkflowService
{
    private readonly ApiClient _apiClient;
    private readonly AiContextBuilderService _contextBuilder;
    private readonly IConversationService _conversationService;

    public MedicationQueryWorkflowService(
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
        var personTask = _apiClient.GetPersonAsync(personId, cancellationToken);
        var plansTask = _apiClient.GetMedicationPlanAsync(cancellationToken);
        await Task.WhenAll(personTask, plansTask);

        var person = await personTask ?? throw new InvalidOperationException(
            $"No se encontró la persona {personId}.");
        var currentTurn = MedicationTurnResolver.Resolve(DateTime.Now);
        var plan = (await plansTask).FirstOrDefault(item =>
            item.Turn == currentTurn) ?? new MedicationPlanResponse
            {
                Turn = currentTurn
            };
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MedicationQuery,
            UserInput = userInput
        };
        var context = _contextBuilder.BuildConversationContext(
            request,
            plan,
            person: person);

        return await _conversationService.ProcessAsync(
            context,
            cancellationToken);
    }
}
