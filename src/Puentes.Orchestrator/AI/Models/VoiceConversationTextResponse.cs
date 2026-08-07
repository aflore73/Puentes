namespace Puentes.Orchestrator.AI.Models;

public sealed class VoiceConversationTextResponse
{
    public Guid ConversationId { get; set; }

    public string Transcript { get; set; } = string.Empty;

    public string ResponseText { get; set; } = string.Empty;
}
