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
        var focusIsAnotherKnownPerson =
            context.Scenario == ConversationScenario.MemorySupport &&
            !string.IsNullOrWhiteSpace(context.State.FocusedPersonName) &&
            !context.State.FocusedPersonName.Equals(
                context.Person.Name,
                StringComparison.OrdinalIgnoreCase);

        var userMessage = SerializeContext(
            context,
            removeAssistedPersonPrivateDetails: focusIsAnotherKnownPerson);

        var scenarioPrompt = context.Scenario switch
        {
            ConversationScenario.MemorySupport =>
                PromptMemorySupport.Contenido,
            ConversationScenario.MedicationQuery =>
                PromptMedicationQuery.Contenido,
            _ => string.Empty
        };

        var systemMessage = string.IsNullOrEmpty(scenarioPrompt)
            ? PromptBase.Contenido
            : $"{PromptBase.Contenido}\n\n{scenarioPrompt}";

        if (focusIsAnotherKnownPerson)
        {
            systemMessage += "\n\n" +
                "REGLA DEL TURNO ACTUAL: la conversación está enfocada en " +
                $"{context.State.FocusedPersonName}. Respondé solamente a ese " +
                "tema con los datos seleccionados para esta persona. No uses " +
                "notas personales de la persona asistida para completar la " +
                "respuesta. No cambies de tema ni ofrezcas por iniciativa propia " +
                "recuerdos, lecturas, gustos, actividades o propuestas de " +
                "compañía. Solamente podés hacerlo si userInput lo pide de forma " +
                "explícita. Si la persona expresa preocupación por dónde estuvo " +
                "alguien en un momento concreto, no conviertas relaciones, " +
                "recuerdos, rutinas, direcciones ni otros datos del contexto en " +
                "una hipótesis de ubicación. No digas que pudo, podría o quizá " +
                "estuvo en un lugar o con una persona si ese momento no está " +
                "confirmado explícitamente. Tampoco sugieras ir, revisar, buscar " +
                "o usar como referencia un lugar inferido. Cuando el dato no está " +
                "confirmado, decí brevemente que no sabés dónde estuvo.";
        }

        return new AssistantPrompt(systemMessage, userMessage);
    }

    private static string SerializeContext(
        ConversationContext context,
        bool removeAssistedPersonPrivateDetails)
    {
        if (!removeAssistedPersonPrivateDetails)
        {
            return JsonSerializer.Serialize(context, JsonOptions);
        }

        var originalNotes = context.Person.Notes;
        var originalBirthDate = context.Person.BirthDate;

        try
        {
            context.Person.Notes = null;
            context.Person.BirthDate = null;
            return JsonSerializer.Serialize(context, JsonOptions);
        }
        finally
        {
            context.Person.Notes = originalNotes;
            context.Person.BirthDate = originalBirthDate;
        }
    }
}
