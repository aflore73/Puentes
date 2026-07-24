using Puentes.Orchestrator.AI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Puentes.Orchestrator.AI.Prompts;

public class PromptFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public AssistantPrompt Create(ConversationContext context)
    {
        var userMessage = JsonSerializer.Serialize(
            context,
            JsonOptions);

        return new AssistantPrompt(
            PromptBase.Contenido,
            userMessage);
    }
}