namespace Puentes.Orchestrator.AI.Models;

public class MemoryConversationRequest
{
    public Guid PersonId { get; set; }

    public string UserInput { get; set; } = string.Empty;
}
