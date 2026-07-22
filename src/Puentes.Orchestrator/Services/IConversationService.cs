using Puentes.Orchestrator.AI.Models;

namespace Puentes.Orchestrator.Services;

    public interface IConversationService
    {
    Task<AssistantResponse> ProcessAsync(
    ConversationContext context);
}
