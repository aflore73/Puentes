using OpenAI;
using OpenAI.Chat;
using Puentes.Orchestrator.AI;
using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;
using System.Text.Json;
using System.Text.Json.Nodes;

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
            },
            "evidence": {
              "type": "object",
              "properties": {
                "personNames": { "type": "array", "items": { "type": "string" } },
                "routineTitles": { "type": "array", "items": { "type": "string" } },
                "lifeEventTitles": { "type": "array", "items": { "type": "string" } },
                "preferenceTitles": { "type": "array", "items": { "type": "string" } },
                "supportContentTitles": { "type": "array", "items": { "type": "string" } },
                "agendaTitles": { "type": "array", "items": { "type": "string" } },
                "belongingNames": { "type": "array", "items": { "type": "string" } }
              },
              "required": ["personNames", "routineTitles", "lifeEventTitles", "preferenceTitles", "supportContentTitles", "agendaTitles", "belongingNames"],
              "additionalProperties": false
            }
          },
          "required": ["message", "pendingOfferDisposition", "offeredAction", "evidence"],
          "additionalProperties": false
        }
        """);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private const string ResolvedContextMarker =
        "\n[Contexto resuelto por Puentes:";
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
        var prepared = PrepareUserMessage(prompt.UserMessage);

        Console.WriteLine($"Marta: {prepared.SpokenInput}");
        Console.WriteLine($"Contexto enviado: {prepared.UserMessage}");

        List<ChatMessage> messages =
        [
            new SystemChatMessage(prompt.SystemMessage),
            new UserChatMessage(prepared.UserMessage)
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

        if (prepared.DiscardedPendingOffer)
        {
            structured.PendingOfferDisposition = "None";
            structured.OfferedAction = new StructuredOffer();
        }

        Console.WriteLine($"OpenAI: {structured.Message}");

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
            },
            Evidence = structured.Evidence
        };
    }

    private static PreparedUserMessage PrepareUserMessage(string userMessage)
    {
        try
        {
            var root = JsonNode.Parse(userMessage)?.AsObject();
            if (root is null)
            {
                return new PreparedUserMessage(userMessage, userMessage, false);
            }

            var fullInput = root["userInput"]?.GetValue<string>() ?? string.Empty;
            var spokenInput = StripResolvedContext(fullInput);
            var state = root["state"] as JsonObject;
            var focusedPersonName = state?["focusedPersonName"]?
                .GetValue<string>();
            var pendingOffer = state?["pendingOffer"] as JsonObject;
            var categoryCode = pendingOffer?["categoryCode"]?
                .GetValue<string>();

            var shouldDiscard =
                !string.IsNullOrWhiteSpace(focusedPersonName) &&
                pendingOffer is not null &&
                !CompanionProposalModeDetector.IsSimpleAcceptance(spokenInput) &&
                (string.IsNullOrWhiteSpace(categoryCode) ||
                 !CompanionProposalModeDetector.MentionsCategory(
                     spokenInput, categoryCode));

            if (shouldDiscard && state is not null)
            {
                state["pendingOffer"] = null;
                state["pendingOffers"] = new JsonArray();
            }

            return new PreparedUserMessage(
                root.ToJsonString(),
                spokenInput,
                shouldDiscard);
        }
        catch (JsonException)
        {
            return new PreparedUserMessage(
                userMessage,
                StripResolvedContext(userMessage),
                false);
        }
    }

    private static string StripResolvedContext(string value)
    {
        var index = value.IndexOf(
            ResolvedContextMarker,
            StringComparison.Ordinal);
        return (index >= 0 ? value[..index] : value).Trim();
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

    private sealed record PreparedUserMessage(
        string UserMessage,
        string SpokenInput,
        bool DiscardedPendingOffer);

    private sealed class StructuredOutput
    {
        public string Message { get; set; } = string.Empty;
        public string PendingOfferDisposition { get; set; } = "None";
        public StructuredOffer OfferedAction { get; set; } = new();
        public ResponseEvidence Evidence { get; set; } = new();
    }

    private sealed class StructuredOffer
    {
        public string Type { get; set; } = "None";
        public string CategoryCode { get; set; } = string.Empty;
        public string ContentTitle { get; set; } = string.Empty;
    }
}
