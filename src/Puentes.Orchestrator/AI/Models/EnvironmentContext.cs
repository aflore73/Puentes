namespace Puentes.Orchestrator.AI.Models;

public class EnvironmentContext
{
    public DateTime CurrentDateTime { get; set; }

    public DayOfWeek DayOfWeek => CurrentDateTime.DayOfWeek;
}