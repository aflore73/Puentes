using OpenAI;
using OpenAI.Chat;
using Puentes.Orchestrator.AI;
using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;
using System.Text.Json;

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

        var client = new OpenAIClient(options.ApiKey);

        _chatClient = client.GetChatClient(options.Model);
    }

    public async Task<AssistantResponse> ProcessAsync(
        ConversationContext context)
    {
        var jsonContext = JsonSerializer.Serialize(context);

        ChatCompletion completion =
            await _chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(
                    MedicationReminderPrompt.System),

                new UserChatMessage(
                    jsonContext)
            ]);

        var message = completion.Content[0].Text;

        return new AssistantResponse
        {
            Message = message,
            Intent = new AssistantIntent
            {
                Name = "Conversation"
            }
        };
    }
}