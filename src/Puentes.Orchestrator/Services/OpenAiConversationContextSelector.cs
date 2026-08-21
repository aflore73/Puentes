using OpenAI;
using OpenAI.Chat;
using Puentes.Orchestrator.AI;
using Puentes.Orchestrator.AI.Models;
using System.Text.Json;

namespace Puentes.Orchestrator.Services;

public sealed class OpenAiConversationContextSelector :
    IConversationContextSelector
{
    private static readonly BinaryData Schema = BinaryData.FromString("""
        {
          "type": "object",
          "properties": {
            "focusedPersonName": { "type": "string" },
            "kinds": {
              "type": "array",
              "items": {
                "type": "string",
                "enum": ["Relationship", "Routine", "Memory", "Preference", "Reading", "Belonging", "Agenda", "Companion", "External", "None"]
              }
            },
            "timeFrame": {
              "type": "string",
              "enum": ["None", "Current", "YesterdayEvening", "Other"]
            }
          },
          "required": ["focusedPersonName", "kinds", "timeFrame"],
          "additionalProperties": false
        }
        """);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly ChatClient _client;
    private readonly ILogger<OpenAiConversationContextSelector> _logger;

    public OpenAiConversationContextSelector(
        OpenAiOptions options,
        ILogger<OpenAiConversationContextSelector> logger)
    {
        var client = new OpenAIClient(options.ApiKey);
        _client = client.GetChatClient(options.Model);
        _logger = logger;
    }

    public async Task<ConversationContextSelection> SelectAsync(
        string userInput,
        IReadOnlyCollection<ConversationHistoryItemContext> history,
        string assistedPersonName,
        IReadOnlyCollection<string> knownPersonNames,
        string? previousFocus,
        CancellationToken cancellationToken = default)
    {
        var input = JsonSerializer.Serialize(new
        {
            userInput,
            assistedPersonName,
            knownPersonNames,
            previousFocus,
            conversationHistory = history.TakeLast(4)
        });
        ChatMessage[] messages =
        [
            new SystemChatMessage("""
                Clasificá qué tipos de contexto necesita el turno actual.
                La intención del userInput actual tiene prioridad. Usá conversationHistory solamente para resolver referencias, continuidad y pronombres; no arrastres el tema, la categoría ni la intención de un turno anterior cuando el userInput actual introduce semánticamente otro tema.
                Relationship: vínculo o información general de una persona conocida.
                Routine: ubicación probable, trabajo, horario, actividad habitual o una ausencia en un momento concreto que pueda compararse con una rutina. Incluí Routine para preguntas que contrastan casa y oficina. No la incluyas cuando preguntan dónde durmió.
                Memory: recuerdos o acontecimientos pasados confirmados.
                Preference: gustos personales de la persona asistida. No uses Preference sólo porque un tema mencionado también aparece dentro de una preferencia; elegila únicamente cuando la intención actual sea hablar del gusto personal.
                Reading: textos configurados para leer.
                Belonging: búsqueda de objetos.
                Agenda: citas o actividades programadas.
                Companion: tristeza, soledad, aburrimiento o pedido de compañía cuando ese sea el tema principal; permite proponer categorías. No lo uses como agregado a una consulta factual sobre otra persona.
                External: conocimiento general explícito sobre artistas, cultura u otros temas externos.
                None: el dato pedido no corresponde a ninguno de los contextos anteriores.
                timeFrame es Current para ahora u hoy, YesterdayEvening para anoche, Other para otro momento y None cuando no aplica. Para preguntas sobre casa u oficina en este momento elegí Routine y Current.
                Elegí solo los tipos necesarios. No agregues Agenda salvo que pregunten por una cita o actividad programada. Una pregunta sobre dónde durmió alguien no es Routine: una rutina no demuestra dónde durmió.
                Si el usuario aclara que no habla de la persona conocida, usá External y focusedPersonName vacío.
                focusedPersonName debe ser un nombre exacto de knownPersonNames o vacío. Conservá previousFocus para pronombres solo si el tema sigue siendo personal.
                """),
            new UserChatMessage(input)
        ];
        var options = new ChatCompletionOptions
        {
            Temperature = 0,
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "puentes_context_selection", Schema,
                jsonSchemaIsStrict: true)
        };
        ChatCompletion completion = await _client.CompleteChatAsync(
            messages, options, cancellationToken);
        var text = completion.Content.Count == 0
            ? string.Empty
            : completion.Content[0].Text;
        var output = JsonSerializer.Deserialize<SelectionOutput>(
            text, JsonOptions) ?? new SelectionOutput();
        var knownFocus = knownPersonNames.FirstOrDefault(name =>
            name.Equals(output.FocusedPersonName,
                StringComparison.OrdinalIgnoreCase));
        var kinds = output.Kinds
            .Select(value => Enum.TryParse<ConversationContextKind>(
                value, true, out var parsed)
                ? parsed
                : ConversationContextKind.None)
            .Distinct()
            .ToList();
        if (kinds.Count > 1)
            kinds.Remove(ConversationContextKind.Companion);
        if (Enum.TryParse<ConversationTimeFrame>(output.TimeFrame, true,
                out var selectedTimeFrame) &&
            selectedTimeFrame is ConversationTimeFrame.Current or
                ConversationTimeFrame.YesterdayEvening &&
            !kinds.Contains(ConversationContextKind.Routine))
        {
            kinds.Add(ConversationContextKind.Routine);
        }
        if (kinds.Count == 0) kinds.Add(ConversationContextKind.None);

        var parsedTimeFrame = Enum.TryParse<ConversationTimeFrame>(
            output.TimeFrame, true, out var timeFrame)
            ? timeFrame
            : ConversationTimeFrame.None;

        // Para una preocupación sobre un momento pasado, mantenemos disponibles
        // las rutinas de la persona. El contexto final marca por separado cuáles
        // aplicaban entonces y cuáles aplican ahora, de modo que el modelo pueda
        // orientar el presente sin convertir una rutina actual en evidencia del
        // pasado.
        var effectiveTimeFrame =
            parsedTimeFrame == ConversationTimeFrame.YesterdayEvening &&
            kinds.Contains(ConversationContextKind.Routine)
                ? ConversationTimeFrame.None
                : parsedTimeFrame;

        var selection = new ConversationContextSelection
        {
            FocusedPersonName = knownFocus,
            Kinds = kinds,
            TimeFrame = effectiveTimeFrame
        };
        _logger.LogInformation(
            "Contexto seleccionado: {Kinds}; foco: {Focus}; tiempo: {TimeFrame}",
            string.Join(',', selection.Kinds),
            selection.FocusedPersonName ?? "sin foco",
            selection.TimeFrame);
        return selection;
    }

    private sealed class SelectionOutput
    {
        public string FocusedPersonName { get; set; } = string.Empty;
        public List<string> Kinds { get; set; } = [];
        public string TimeFrame { get; set; } = "None";
    }
}
