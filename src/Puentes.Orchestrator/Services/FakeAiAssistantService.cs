using Puentes.Orchestrator.AI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Puentes.Orchestrator.Services;

public class FakeAiAssistantService : IAssistantService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
    public Task<AssistantResponse> ProcessAsync(
        ConversationContext context)
    {
        var json = JsonSerializer.Serialize(context, JsonOptions);

        Console.WriteLine(json);
        var response = context.Scenario switch
        {
            ConversationScenario.MedicationReminder =>
                BuildMedicationReminder(context),

            ConversationScenario.WaitingMedicationConfirmation =>
                BuildConfirmation(context),

            _ => new AssistantResponse
            {
                Message = "Hola, Marta.",
                Intent = new AssistantIntent
                {
                    Name = "Greeting",
                    Confidence = 1
                }
            }
        };

        return Task.FromResult(response);
    }

    private static AssistantResponse BuildMedicationReminder(
        ConversationContext context)
    {
        var medications = context.Medication?.Medications
            .Where(m => m.SpeakName)
            .Select(m => $"{m.Quantity} {m.Name}");

        var message = medications is null
            ? "No hay medicación pendiente."
            : $"Marta, es hora de tomar: {string.Join(", ", medications)}.";

        return new AssistantResponse
        {
            Message = message,
            Intent = new AssistantIntent
            {
                Name = "MedicationReminder",
                Confidence = 1
            }
        };
    }
    private static AssistantResponse BuildConfirmation(
        ConversationContext context)
    {
        return new AssistantResponse
        {
            Message = "Perfecto, ya registré que tomaste la medicación.",
            Intent = new AssistantIntent
            {
                Name = "MedicationTaken",
                Confidence = 1
            }
        };
    }
}