namespace Puentes.Orchestrator.AI.Models;

public class MemoryConversationTurnResponse
{
    public Guid ConversationId { get; set; }

    public AssistantResponse Response { get; set; } = new();
}
