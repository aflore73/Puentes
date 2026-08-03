using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.Services;
using Puentes.Shared.Enums;

public class MedicationWorkflowService
{
    private readonly ApiClient _apiClient;
    private readonly IConversationService _conversationService;
    private readonly AiContextBuilderService _contextBuilder;
    private readonly IConfiguration _configuration;
    private readonly MedicationConfirmationService _confirmationService;
    public MedicationWorkflowService(
        ApiClient apiClient,
        IConversationService conversationService,
        AiContextBuilderService contextBuilder,
        IConfiguration configuration,
        MedicationConfirmationService confirmationService)
    {
        _apiClient = apiClient;
        _conversationService = conversationService;
        _contextBuilder = contextBuilder;
        _configuration = configuration;
        _confirmationService = confirmationService;
    }
    public async Task ProcessAsync(
        CancellationToken cancellationToken)
    {
        //debugging purposes, you can log the current time to see when the workflow is being executed.
        var interactive =
        _configuration.GetValue<bool>(
            "Developer:InteractiveConversation");
        //fin debugging purposes
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

        var patientId = _configuration.GetValue<Guid?>("Patient:Id")
            ?? throw new InvalidOperationException(
                "No se configuró Patient:Id para registrar la medicación.");

        var existingRecord = await _apiClient
            .GetTodayMedicationRecordAsync(
                patientId,
                currentTurn,
                cancellationToken);

        if (existingRecord?.Confirmed == true)
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

        //Degugging purposes, you can log the response to see what the assistant is saying.
        if (interactive)
        {
            Console.WriteLine(response.Message);

            while (true)
            {
                Console.Write("Marta: ");

                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    break;

                if (_confirmationService.IsExplicitConfirmation(input))
                {
                    var created = await _apiClient
                        .RegisterMedicationTakenAsync(
                            patientId,
                            currentTurn,
                            input,
                            cancellationToken);

                    Console.WriteLine(created
                        ? "Gracias, Marta. Ya registré que tomaste la medicación."
                        : "Gracias, Marta. La toma ya estaba registrada.");

                    break;
                }

                context.UserInput = input;
                context.State.WaitingMedicationConfirmation = true;

                response = await _conversationService
                    .ProcessAsync(context, cancellationToken);

                Console.WriteLine(response.Message);
            }
        }
        //fin debugging purposes
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
