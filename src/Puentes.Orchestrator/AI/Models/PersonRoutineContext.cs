namespace Puentes.Orchestrator.AI.Models;

public class PersonRoutineContext
{
    public string PersonName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string? DaysOfWeek { get; set; }

    public string? StartTime { get; set; }

    public string? EndTime { get; set; }

    public bool? AppliesNow { get; set; }

    public bool? AppliesYesterdayEvening { get; set; }
}
