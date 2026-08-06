namespace Puentes.Orchestrator.AI.Models;

public class VoiceConversationResponse
{
    public Guid ConversationId { get; set; }

    public string Transcript { get; set; } = string.Empty;

    public string ResponseText { get; set; } = string.Empty;

    public string AudioContentType { get; set; } = "audio/mpeg";

    public string AudioBase64 { get; set; } = string.Empty;
}
