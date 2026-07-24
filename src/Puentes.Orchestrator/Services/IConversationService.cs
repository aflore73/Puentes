using Puentes.Orchestrator.AI.Models;

public interface IConversationService
{
    Task<AssistantResponse> ProcessAsync(
        ConversationContext context,
        CancellationToken cancellationToken = default);
}