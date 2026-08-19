namespace Puentes.Orchestrator.AI.Models;

public class ConversationState
{
    public bool WaitingMedicationConfirmation { get; set; }

    public bool ReminderAlreadySent { get; set; }

    public PendingOfferContext? PendingOffer { get; set; }

    public RequiredDialogueAction RequiredDialogueAction { get; set; }
}
