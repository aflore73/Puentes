namespace Puentes.Orchestrator.AI.Models;

public class ConversationState
{
    public bool WaitingMedicationConfirmation { get; set; }

    public bool ReminderAlreadySent { get; set; }

    public PendingOfferContext? PendingOffer { get; set; }

    public List<PendingOfferContext> PendingOffers { get; set; } = [];

    public RequiredDialogueAction RequiredDialogueAction { get; set; }

    public bool AvoidAssistedPersonName { get; set; }

    public string? FocusedPersonName { get; set; }

    public List<string> ResponseValidationErrors { get; set; } = [];
}
