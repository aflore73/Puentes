using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;

public interface IAssistantService
{
    Task<AssistantResponse> ProcessAsync(
      AssistantPrompt prompt,
      CancellationToken cancellationToken = default);
}