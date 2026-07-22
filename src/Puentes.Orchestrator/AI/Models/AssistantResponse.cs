namespace Puentes.Orchestrator.AI.Models
{
    public class AssistantResponse
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public AssistantIntent Intent { get; set; }
        public float ConfidenceScore { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public DateTime Timestamp { get; set; }

        public AssistantResponse()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
            Metadata = new Dictionary<string, object>();
        }
    }
}
