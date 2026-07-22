using Puentes.Orchestrator.AI.Models;

public interface IAssistantService
{
    Task<AssistantResponse> ProcessAsync(
        ConversationContext context);
}