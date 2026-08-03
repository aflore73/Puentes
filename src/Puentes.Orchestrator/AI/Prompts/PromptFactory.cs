using Puentes.Orchestrator.AI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Puentes.Orchestrator.AI.Prompts;

public class PromptFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
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

        var scenarioPrompt = context.Scenario switch
        {
            ConversationScenario.MemorySupport =>
                PromptMemorySupport.Contenido,
            _ => string.Empty
        };

        var systemMessage = string.IsNullOrEmpty(scenarioPrompt)
            ? PromptBase.Contenido
            : $"{PromptBase.Contenido}\n\n{scenarioPrompt}";

        return new AssistantPrompt(systemMessage, userMessage);
    }
}
