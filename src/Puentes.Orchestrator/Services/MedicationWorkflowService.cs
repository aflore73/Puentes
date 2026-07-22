using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.Services;
using System.Numerics;

public class MedicationWorkflowService
{
    private readonly ApiClient _apiClient;
    private readonly IConversationService _conversationService;

    public MedicationWorkflowService(
        ApiClient apiClient,
        IConversationService conversationService)
    {
        _apiClient = apiClient;
        _conversationService = conversationService;
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

        var plan = plans.FirstOrDefault();
        var context = new ConversationContext
        {
            PersonName = "Marta",
            Scenario = ConversationScenario.MedicationReminder,
            Medication = plan
        };

        var response = await _conversationService.ProcessAsync(context);
        Console.WriteLine(response.Message);
    }
}