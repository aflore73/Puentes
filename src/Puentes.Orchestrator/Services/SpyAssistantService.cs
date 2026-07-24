using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;

public sealed class SpyAssistantService : IAssistantService
{
    public string? SystemPrompt { get; private set; }

    public string? UserMessage { get; private set; }

    public Task<AssistantResponse> ProcessAsync(
        AssistantPrompt prompt,
        CancellationToken cancellationToken = default)
    {
        SystemPrompt = prompt.SystemMessage;
        UserMessage = prompt.UserMessage;

        return Task.FromResult(new AssistantResponse
        {
            Message = "Marta, es hora de tomar tus pastillas."
        });
    }
}