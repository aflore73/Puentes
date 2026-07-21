using Puentes.Shared.Contexts;

namespace Puentes.Orchestrator.AI;

public interface IAssistantService
{
    Task<string> GenerateAsync(
        MedicationReminderContext context);
}