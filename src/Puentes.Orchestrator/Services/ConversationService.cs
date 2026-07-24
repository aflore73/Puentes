using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;

public class ConversationService : IConversationService
{
    private readonly PromptFactory _promptFactory;
    private readonly IAssistantService _assistant;

    public ConversationService(
        PromptFactory promptFactory,
        IAssistantService assistant)
    {
        _promptFactory = promptFactory;
        _assistant = assistant;
    }

    public Task<AssistantResponse> ProcessAsync(
        ConversationContext context,
        CancellationToken cancellationToken = default)
    {
        var prompt = _promptFactory.Create(context);

        return _assistant.ProcessAsync(
            prompt,
            cancellationToken);
    }
}