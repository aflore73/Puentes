using Puentes.Shared.Enums;
namespace Puentes.Orchestrator.Services;

public class MedicationReminderStateService
{
    private MedicationTurnType? _lastProcessedTurn;
    private DateTime _lastProcessedDate;


    public bool WasProcessedToday(
        MedicationTurnType turn)
    {
        return _lastProcessedDate == DateTime.Today
            && _lastProcessedTurn == turn;
    }


    public void MarkAsProcessed(
        MedicationTurnType turn)
    {
        _lastProcessedDate = DateTime.Today;
        _lastProcessedTurn = turn;
    }
}