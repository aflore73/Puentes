namespace Puentes.Orchestrator.AI.Models;

public class PersonPreferenceContext
{
    public string PersonName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public List<string> TopicCodes { get; set; } = [];
}
