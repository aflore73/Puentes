public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ApiClient _apiClient;
    private readonly MedicationReminderService _reminder;
    public Worker(ILogger<Worker> logger, ApiClient apiClient, MedicationReminderService reminder)
    {
        _logger = logger;
        _apiClient = apiClient;
        _reminder = reminder;
    }
    protected override async Task ExecuteAsync(
    CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _reminder.ProcessAsync();

            await Task.Delay(
                TimeSpan.FromMinutes(1),
                stoppingToken);
        }
    }
}