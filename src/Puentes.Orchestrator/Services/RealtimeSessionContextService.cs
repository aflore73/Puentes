using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;

namespace Puentes.Orchestrator.Services;

public sealed class RealtimeSessionContextService
{
    private readonly ApiClient _apiClient;
    private readonly AiContextBuilderService _contextBuilder;
    private readonly PromptFactory _promptFactory;

    public RealtimeSessionContextService(
        ApiClient apiClient,
        AiContextBuilderService contextBuilder,
        PromptFactory promptFactory)
    {
        _apiClient = apiClient;
        _contextBuilder = contextBuilder;
        _promptFactory = promptFactory;
    }

    public async Task<RealtimeSessionContext> BuildAsync(
        Guid personId,
        CancellationToken cancellationToken)
    {
        var personTask = _apiClient.GetPersonAsync(personId, cancellationToken);
        var relationshipsTask = _apiClient.GetPersonRelationshipsAsync(
            personId, cancellationToken);
        var supportContentsTask = _apiClient.GetPersonSupportContentsAsync(
            personId, cancellationToken);
        var belongingsTask = _apiClient.GetPersonBelongingsAsync(
            personId, cancellationToken);
        var trustedContactsTask = _apiClient.GetPersonTrustedContactsAsync(
            personId, cancellationToken);
        await Task.WhenAll(
            personTask,
            relationshipsTask,
            supportContentsTask,
            belongingsTask,
            trustedContactsTask);

        var person = await personTask ?? throw new InvalidOperationException(
            $"No se encontro la persona {personId}.");
        var relationships = await relationshipsTask;
        var ownerIds = relationships
            .Select(relationship => relationship.OtherPerson.Id)
            .Append(personId)
            .Distinct()
            .ToArray();
        var contextTasks = ownerIds.Select(async ownerId =>
        {
            var eventsTask = _apiClient.GetPersonLifeEventsAsync(
                ownerId, cancellationToken);
            var routinesTask = _apiClient.GetPersonRoutinesAsync(
                ownerId, cancellationToken);
            var preferencesTask = _apiClient.GetPersonPreferencesAsync(
                ownerId, cancellationToken);
            await Task.WhenAll(eventsTask, routinesTask, preferencesTask);
            return (
                Events: await eventsTask,
                Routines: await routinesTask,
                Preferences: await preferencesTask);
        });
        var contextItems = await Task.WhenAll(contextTasks);
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = null
        };
        var context = _contextBuilder.BuildConversationContext(
            request,
            relationships: relationships,
            lifeEvents: contextItems.SelectMany(item => item.Events).ToList(),
            routines: contextItems.SelectMany(item => item.Routines).ToList(),
            preferences: contextItems
                .SelectMany(item => item.Preferences)
                .ToList(),
            supportContents: await supportContentsTask,
            belongings: await belongingsTask,
            trustedContacts: await trustedContactsTask,
            person: person);
        var prompt = _promptFactory.Create(context);
        var knownPersonItems = relationships
            .Select(relationship => new RealtimeKnownPerson(
                relationship.OtherPerson.Name,
                $"relacion {relationship.Type}, " +
                $"{relationship.Notes ?? "sin notas adicionales"}"))
            .ToList();
        var knownPeople = string.Join(
            "\n",
            knownPersonItems.Select(knownPerson =>
                $"- {knownPerson.Name}: {knownPerson.Description}."));

        var instructions = "Estas conversando con la persona identificada en el contexto. " +
            "Cuando mencione un nombre que coincide con una persona de sus relaciones, " +
            "recuerdos o rutinas, interpreta primero que habla de esa persona. " +
            "Usa otro significado solamente si lo indica de manera explicita o si el " +
            "contexto de la pregunta muestra claramente que habla de otra cosa.\n\n" +
            $"Personas conocidas por {person.Name}:\n{knownPeople}\n\n" +
            $"{prompt.SystemMessage}\n\nContexto actual de la persona:\n" +
            prompt.UserMessage +
            "\n\nResponde siempre en espanol argentino, incluso si una frase se " +
            "transcribe en otro idioma. Responde con audio, de forma breve, " +
            "calida y conversacional. No leas ni menciones el JSON. Antes de " +
            "responder sobre alguien, compara su nombre con la lista de personas " +
            "conocidas. Una mencion sin aclaraciones se refiere a la coincidencia " +
            "de esa lista.";

        return new RealtimeSessionContext(
            personId,
            person.Name,
            instructions,
            knownPersonItems);
    }
}
