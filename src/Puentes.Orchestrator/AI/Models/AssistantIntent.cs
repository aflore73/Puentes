namespace Puentes.Orchestrator.AI.Models;

public class AssistantIntent
{
    public string Name { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];
    public string Description { get; set; } = string.Empty;

  
}
