namespace Puentes.Orchestrator.AI.Models;

public class EnvironmentContext
{
    public DateTime CurrentDateTime { get; set; } = DateTime.Now;

    public DayOfWeek DayOfWeek => CurrentDateTime.DayOfWeek;

    public DateOnly YesterdayDate =>
        DateOnly.FromDateTime(CurrentDateTime.AddDays(-1));

    public DayOfWeek YesterdayDayOfWeek =>
        CurrentDateTime.AddDays(-1).DayOfWeek;

    public DateOnly TomorrowDate =>
        DateOnly.FromDateTime(CurrentDateTime.AddDays(1));

    public DayOfWeek TomorrowDayOfWeek =>
        CurrentDateTime.AddDays(1).DayOfWeek;
}
