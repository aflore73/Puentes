namespace Puentes.Orchestrator.AI.Models;

public enum PendingOfferDisposition
{
    None,
    Accepted,
    Rejected,
    Unclear
}

public enum DialogueOfferType
{
    None,
    Category,
    Content
}

public enum RequiredDialogueAction
{
    None,
    OfferSuggestedContent,
    ContinueWithAcceptedCategory
}

public sealed class DialogueOffer
{
    public DialogueOfferType Type { get; set; }
    public string? CategoryCode { get; set; }
    public string? ContentTitle { get; set; }
}

public sealed class PendingOfferContext
{
    public DialogueOfferType Type { get; set; }
    public string? CategoryCode { get; set; }
    public string? ContentTitle { get; set; }
    public string? SuggestedContentTitle { get; set; }
    public string? SuggestedContentReference { get; set; }
}
