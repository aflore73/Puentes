public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly MedicationWorkflowService _workflow;

    public Worker(
        ILogger<Worker> logger,
        MedicationWorkflowService workflow)
    {
        _logger = logger;
        _workflow = workflow;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _workflow.ProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error en el flujo de medicamentos.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(1),
                stoppingToken);
        }
    }
}