using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;

namespace Puentes.Orchestrator.Services;

public class FakeAiAssistantService : IAssistantService
{
    public Task<AssistantResponse> ProcessAsync(
    AssistantPrompt prompt,
    CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AssistantResponse
        {
            Message = $"FAKE AI\n\nSystem:\n{prompt.SystemMessage}\n\nUser:\n{prompt.UserMessage}"
        });
    }
}