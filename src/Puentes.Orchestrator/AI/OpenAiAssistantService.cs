using OpenAI;
using OpenAI.Chat;
using Puentes.Shared.Contexts;
using System.Text.Json;

namespace Puentes.Orchestrator.AI;

public class OpenAiAssistantService : IAssistantService
{
    private readonly ChatClient _chatClient;

    public OpenAiAssistantService(OpenAiOptions options)
    {
        var client = new OpenAIClient(options.ApiKey);

        _chatClient = client.GetChatClient(options.Model);
    }
    public async Task<string> GenerateAsync(
       MedicationReminderContext context)
    {
        var jsonContext = JsonSerializer.Serialize(context);

        ChatCompletion completion = await _chatClient.CompleteChatAsync(
        [
            new SystemChatMessage(
                MedicationReminderPrompt.System),

            new UserChatMessage(
                jsonContext)
        ]);

        return completion.Content[0].Text;
    }
}