using Puentes.Shared.Enums;

namespace Puentes.Orchestrator.AI.Models;

public class MedicationContext
{
    public MedicationTurnType Turn { get; set; }

    public List<MedicationItemContext> Medications { get; set; } = [];
}