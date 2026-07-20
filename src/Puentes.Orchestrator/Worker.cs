public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ApiClient _apiClient;
    public Worker(
         ILogger<Worker> logger,
         ApiClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)

    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var plan = await _apiClient.GetMedicationPlanAsync();

            foreach (var turn in plan)
            {
                _logger.LogInformation(
       "Valor: {Value} - Tipo: {Type}",
       (int)turn.Turn,
       turn.Turn.GetType().FullName);
                _logger.LogInformation("===== {Turn} =====", turn.Turn);

                foreach (var med in turn.Medications)
                {
                    _logger.LogInformation("{Name} ({Quantity})",
                        med.Name,
                        med.Quantity);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}