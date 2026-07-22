using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.Services;

public class ConversationService : IConversationService
{
    private readonly AiContextBuilderService _contextBuilder;
    private readonly IAssistantService _assistant;

    public ConversationService(
        AiContextBuilderService contextBuilder,
        IAssistantService assistant)
    {
        _contextBuilder = contextBuilder;
        _assistant = assistant;
    }
    public Task<AssistantResponse> ProcessAsync(
       ConversationContext context)
    {
        return _assistant.ProcessAsync(context);
    }
}