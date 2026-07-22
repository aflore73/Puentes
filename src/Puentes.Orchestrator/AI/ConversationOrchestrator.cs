using Puentes.Orchestrator.Services;

namespace Puentes.Orchestrator.AI;
    public class ConversationOrchestrator
    {
    private readonly AiContextBuilderService _contextBuilder;
    private readonly IAssistantService _assistant;

    public ConversationOrchestrator(
        AiContextBuilderService contextBuilder,
        IAssistantService assistant)
    {
        _contextBuilder = contextBuilder;
        _assistant = assistant;
    }
}
