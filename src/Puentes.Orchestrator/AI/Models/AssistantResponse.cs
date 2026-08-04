namespace Puentes.Orchestrator.AI.Models
{
    public class AssistantResponse
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Message { get; set; } = string.Empty;
        public AssistantIntent Intent { get; set; } = new();
        public float ConfidenceScore { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = [];
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
