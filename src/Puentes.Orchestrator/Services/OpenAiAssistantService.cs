using OpenAI;
using OpenAI.Chat;
using Puentes.Orchestrator.AI;
using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;
using System.Text.Json;

namespace Puentes.Orchestrator.Services;

public class OpenAiAssistantService : IAssistantService
{
    private static readonly BinaryData ResponseSchema = BinaryData.FromString("""
        {
          "type": "object",
          "properties": {
            "message": { "type": "string" },
            "pendingOfferDisposition": {
              "type": "string",
              "enum": ["None", "Accepted", "Rejected", "Unclear"]
            },
            "offeredAction": {
              "type": "object",
              "properties": {
                "type": {
                  "type": "string",
                  "enum": ["None", "Category", "Content"]
                },
                "categoryCode": { "type": "string" },
                "contentTitle": { "type": "string" }
              },
              "required": ["type", "categoryCode", "contentTitle"],
              "additionalProperties": false
            }
          },
          "required": ["message", "pendingOfferDisposition", "offeredAction"],
          "additionalProperties": false
        }
        """);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly ChatClient _chatClient;

    public OpenAiAssistantService(OpenAiOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException(
                "No se encontró la variable PUENTES_API_KEY.");
        }

        if (string.IsNullOrWhiteSpace(options.Model))
        {
            throw new InvalidOperationException(
                "No se configuró el modelo de OpenAI.");
        }

        var client = new OpenAIClient(options.ApiKey);

        _chatClient = client.GetChatClient(options.Model);
    }

    public async Task<AssistantResponse> ProcessAsync(
        AssistantPrompt prompt,
        CancellationToken cancellationToken = default)
    {
        List<ChatMessage> messages =
        [
            new SystemChatMessage(prompt.SystemMessage),
            new UserChatMessage(prompt.UserMessage)
        ];

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "puentes_assistant_response",
                ResponseSchema,
                jsonSchemaIsStrict: true)
        };
        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            messages,
            options,
            cancellationToken);

        var content = completion.Content.Count > 0
            ? completion.Content[0].Text
            : string.Empty;
        var structured = JsonSerializer.Deserialize<StructuredOutput>(
            content, JsonOptions) ?? throw new InvalidOperationException(
                "OpenAI no devolvio una respuesta estructurada valida.");

        return new AssistantResponse
        {
            Message = structured.Message,
            PendingOfferDisposition = ParseDisposition(
                structured.PendingOfferDisposition),
            OfferedAction = new DialogueOffer
            {
                Type = ParseOfferType(structured.OfferedAction.Type),
                CategoryCode = EmptyToNull(
                    structured.OfferedAction.CategoryCode),
                ContentTitle = EmptyToNull(
                    structured.OfferedAction.ContentTitle)
            }
        };
    }

    private static PendingOfferDisposition ParseDisposition(string value) =>
        Enum.TryParse<PendingOfferDisposition>(value, true, out var parsed)
            ? parsed
            : PendingOfferDisposition.Unclear;

    private static DialogueOfferType ParseOfferType(string value) =>
        Enum.TryParse<DialogueOfferType>(value, true, out var parsed)
            ? parsed
            : DialogueOfferType.None;

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed class StructuredOutput
    {
        public string Message { get; set; } = string.Empty;
        public string PendingOfferDisposition { get; set; } = "None";
        public StructuredOffer OfferedAction { get; set; } = new();
    }

    private sealed class StructuredOffer
    {
        public string Type { get; set; } = "None";
        public string CategoryCode { get; set; } = string.Empty;
        public string ContentTitle { get; set; } = string.Empty;
    }
}
