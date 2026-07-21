using Puentes.Shared.Enums;
using Puentes.Shared.Responses;
using Puentes.Orchestrator.AI;

namespace Puentes.Orchestrator.Services;

public class MedicationReminderService
{
    private MedicationTurnType? _lastProcessedTurn;
    private DateTime _lastProcessedDate;
    private readonly ApiClient _apiClient;
    private readonly ILogger<MedicationReminderService> _logger;
    private readonly MedicationReminderStateService _stateService;
   private readonly AiContextBuilderService _aiContextBuilder;
    private readonly IAssistantService _aiAssistantService;
    public MedicationReminderService(
        ApiClient apiClient,
        ILogger<MedicationReminderService> logger, MedicationReminderStateService stateService, AiContextBuilderService aiContextBuilder, IAssistantService aiAssistantService )
    {
        _apiClient = apiClient;
        _logger = logger;
        _stateService = stateService;
        _aiContextBuilder = aiContextBuilder;
        _aiAssistantService = aiAssistantService;
    }
    public void MarkAsProcessed(
        MedicationTurnType turn)
    {
        _lastProcessedDate = DateTime.Today;
        _lastProcessedTurn = turn;
    }
    public bool WasProcessedToday(
        MedicationTurnType turn)
    {
        return _lastProcessedDate == DateTime.Today
            && _lastProcessedTurn == turn;
    }
    public async Task ProcessAsync()
    {
        var plan = await _apiClient.GetMedicationPlanAsync();
        var currentTurn = GetCurrentTurn(plan);

        if (currentTurn is null)
            return;
        // 1) Primero verificamos si ya fue procesado
        if (_stateService.WasProcessedToday(currentTurn.Turn))
        {
            _logger.LogInformation(
                "Turno {Turn} ya procesado hoy",
                currentTurn.Turn);

            return;
        }
        //log the current turn and the medications for that turn
        _logger.LogInformation(
       "Turno actual: {Turn}",
       currentTurn.Turn);
        // 2) Crear contexto para IA
        var context = _aiContextBuilder.Build(currentTurn);
        var message = await _aiAssistantService.GenerateAsync(context);

        _logger.LogInformation(message);
        //_logger.LogInformation(
        //    JsonSerializer.Serialize(context));
        // 3) Acá después irá la llamada a la IA
        // var message = await _assistantService.GenerateAsync(context);

        // 4) Marcar como procesado
        _stateService.MarkAsProcessed(currentTurn.Turn);
    }
    private MedicationPlanResponse? GetCurrentTurn(
    List<MedicationPlanResponse> plan)
    {
        var now = DateTime.Now;

        var turn = now.Hour switch
        {
            >= 8 and < 12 => MedicationTurnType.Morning,
            >= 12 and < 18 => MedicationTurnType.Midday,
            _ => MedicationTurnType.Night
        };

        return plan.FirstOrDefault(x => x.Turn == turn);
    }
}