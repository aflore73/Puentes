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
    ContinueWithAcceptedCategory,
    ClarifyOfferChoice,
    CorrectInvalidResponse
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

public sealed class ResponseEvidence
{
    public List<string> PersonNames { get; set; } = [];
    public List<string> RoutineTitles { get; set; } = [];
    public List<string> LifeEventTitles { get; set; } = [];
    public List<string> PreferenceTitles { get; set; } = [];
    public List<string> SupportContentTitles { get; set; } = [];
    public List<string> AgendaTitles { get; set; } = [];
    public List<string> BelongingNames { get; set; } = [];
}
