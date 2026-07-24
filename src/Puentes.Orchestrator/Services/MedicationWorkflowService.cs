using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.Services;
using Puentes.Shared.Enums;

public class MedicationWorkflowService
{
    private readonly ApiClient _apiClient;
    private readonly IConversationService _conversationService;
    private readonly AiContextBuilderService _contextBuilder;

    public MedicationWorkflowService(
        ApiClient apiClient,
        IConversationService conversationService,
        AiContextBuilderService contextBuilder)
    {
        _apiClient = apiClient;
        _conversationService = conversationService;
        _contextBuilder = contextBuilder;
    }

    public async Task ProcessAsync(
        CancellationToken cancellationToken)
    {
        var plans = await _apiClient
            .GetMedicationPlanAsync(cancellationToken);

        if (plans.Count == 0)
        {
            return;
        }

        var currentTurn = GetCurrentTurn();

        var plan = plans.FirstOrDefault(
            x => x.Turn == currentTurn);

        if (plan is null)
        {
            return;
        }

        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MedicationReminder,
            UserInput = null,
            WaitingMedicationConfirmation = false
        };

        var context = _contextBuilder
            .BuildConversationContext(request, plan);

        var response = await _conversationService
            .ProcessAsync(context, cancellationToken);

        Console.WriteLine(response.Message);
    }

    private static MedicationTurnType GetCurrentTurn()
    {
        var hour = DateTime.Now.Hour;

        return hour switch
        {
            >= 6 and < 12 => MedicationTurnType.Morning,
            >= 12 and < 18 => MedicationTurnType.Midday,
            _ => MedicationTurnType.Night
        };
    }
}