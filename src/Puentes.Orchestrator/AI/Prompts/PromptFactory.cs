using Puentes.Orchestrator.AI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Puentes.Orchestrator.AI.Prompts;

public class PromptFactory
{
    private const string ConversationBoundaryPrefix = "Límite conversacional:";

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

        var focusedRelationship = focusIsAnotherKnownPerson
            ? context.MemorySupport?.Relationships.FirstOrDefault(item =>
                item.OtherPersonName.Equals(
                    context.State.FocusedPersonName,
                    StringComparison.OrdinalIgnoreCase))
            : null;
        var conversationBoundary = ExtractConversationBoundary(
            focusedRelationship?.Notes);

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

        if (context.Scenario == ConversationScenario.MemorySupport)
        {
            systemMessage += "\n\n" +
                "REGLA DE TONO: hablá de manera cotidiana, cercana y simple, " +
                "como en una conversación familiar. Cuando un dato no esté " +
                "confirmado, preferí 'no sé bien' o una formulación igualmente " +
                "natural. No uses expresiones frías o administrativas como " +
                "'no está confirmado', 'no tengo información que confirme', " +
                "'no puedo determinar', 'ese dato no está disponible', " +
                "'puede servir como referencia' o 'según la información " +
                "disponible'. Nunca describas las limitaciones internas del " +
                "contexto ni digas frases como 'no tengo una rutina', 'no hay " +
                "una rutina', 'no tengo datos para ubicarlo' o equivalentes. " +
                "Si el mensaje se refiere a un momento pasado y ese momento no " +
                "está confirmado, reconocelo brevemente sin inventar. Si además " +
                "hay una rutina con appliesNow=true que sea útil para orientar " +
                "el presente, podés mencionarla enseguida, dejando claro que " +
                "hablás de ahora y no del momento pasado. No uses una rutina " +
                "actual como explicación de lo ocurrido antes. No agregues una " +
                "rutina si no es pertinente al mensaje actual. Mantené la " +
                "respuesta breve y cálida, sin sonar técnica, administrativa ni " +
                "clínica.";
        }

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
                "explícita. Si userInput no pide explícitamente contactar a la " +
                "persona enfocada ni ayuda para redactar un contacto, no sugieras " +
                "llamar, escribir, mandar mensajes, esperar una respuesta ni " +
                "intentar comunicarse más tarde. Tampoco inventes qué habría que " +
                "decir, preguntar, avisar o para qué habría que contactar. Si la " +
                "persona expresa preocupación por dónde estuvo alguien en un " +
                "momento concreto, no conviertas relaciones, recuerdos, rutinas, " +
                "direcciones ni otros datos del contexto en una hipótesis de " +
                "ubicación. No digas que pudo, podría o quizá estuvo en un lugar " +
                "o con una persona si ese momento no está confirmado " +
                "explícitamente. Tampoco sugieras ir, revisar, buscar o usar como " +
                "referencia un lugar inferido. Cuando el dato no esté confirmado, " +
                "decilo de forma natural y cercana, por ejemplo 'no sé bien dónde " +
                "estuvo', y evitá fórmulas como 'no está confirmado'. No " +
                "menciones que faltan rutinas, datos o información del sistema. " +
                "Si existe una rutina actual aplicable, usala sólo para orientar " +
                "el presente y diferenciá claramente ese momento del pasado " +
                "consultado.";
        }

        if (!string.IsNullOrWhiteSpace(conversationBoundary))
        {
            systemMessage += "\n\n" +
                "LÍMITE CONVERSACIONAL OBLIGATORIO PARA EL TURNO ACTUAL: " +
                conversationBoundary + " No conviertas este límite en tema de " +
                "conversación ni se lo expliques a la persona. Aplicalo en " +
                "silencio. Si el límite indica no hablar, recordar o profundizar " +
                "sobre la persona enfocada, respondé sólo lo mínimo necesario " +
                "para orientarla y no propongas recuerdos, historias, preguntas " +
                "ni actividades relacionadas con esa persona.";
        }

        return new AssistantPrompt(systemMessage, userMessage);
    }

    private static string? ExtractConversationBoundary(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return null;

        var index = notes.IndexOf(
            ConversationBoundaryPrefix,
            StringComparison.OrdinalIgnoreCase);
        if (index < 0) return null;

        var boundary = notes[(index + ConversationBoundaryPrefix.Length)..]
            .Trim();
        return string.IsNullOrWhiteSpace(boundary) ? null : boundary;
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
