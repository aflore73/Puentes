namespace Puentes.Orchestrator.AI.Models;

public class PersonSupportContentContext
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public List<string> TopicCodes { get; set; } = [];

    public string? Attribution { get; set; }

    public string? Reference { get; set; }
}
