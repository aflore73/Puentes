using Puentes.Orchestrator.Services;
using Puentes.Shared.Enums;
using Puentes.Shared.Responses;

public class MedicationReminderService
{
    private MedicationTurnType? _lastProcessedTurn;
    private DateTime _lastProcessedDate;
    private readonly ApiClient _apiClient;
    private readonly ILogger<MedicationReminderService> _logger;
    private readonly MedicationReminderStateService _stateService;
    private readonly MedicationMessageService _messageService;
    public MedicationReminderService(
        ApiClient apiClient,
        ILogger<MedicationReminderService> logger, MedicationReminderStateService stateService, MedicationMessageService messageService)
    {
        _apiClient = apiClient;
        _logger = logger;
        _stateService = stateService;
        _messageService = messageService;
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
                currentTurn);

            return;
        }
        //log the current turn and the medications for that turn
        _logger.LogInformation(
       "Turno actual: {Turn}",
       currentTurn.Turn);

        var message = _messageService.BuildMessage(currentTurn);
        _logger.LogInformation(
            "{Message}",
            message);
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