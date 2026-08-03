namespace Puentes.Orchestrator.AI.Models;

public class PersonContext
{
    public string Name { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string Language { get; set; } = "es-AR";
    public string TimeZone { get; set; } = "America/Argentina/Buenos_Aires";
}
