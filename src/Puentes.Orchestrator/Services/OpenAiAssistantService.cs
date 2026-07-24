using OpenAI;
using OpenAI.Chat;
using Puentes.Orchestrator.AI;
using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;

namespace Puentes.Orchestrator.Services;

public class OpenAiAssistantService : IAssistantService
{
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

        ChatCompletion completion =
            await _chatClient.CompleteChatAsync(
                messages,
                cancellationToken: cancellationToken);

        var message = completion.Content.Count > 0
            ? completion.Content[0].Text
            : string.Empty;

        return new AssistantResponse
        {
            Message = message
        };
    }
}