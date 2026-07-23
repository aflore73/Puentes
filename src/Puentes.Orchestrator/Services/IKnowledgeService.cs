using Puentes.Shared.Domain.Knowledge;

namespace Puentes.Orchestrator.Services;

public interface IKnowledgeService
{
    Task<IReadOnlyList<KnowledgeFact>> SearchAsync(
string userInput);
}
