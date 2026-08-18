using Puentes.Infrastructure.Repositories;
using Puentes.Shared.Domain;
using Puentes.Shared.Requests.People;
using Puentes.Shared.Responses.LifeEvents;
using Puentes.Shared.Responses.People;

namespace Puentes.Api;

public static class FamilyMaintenanceEndpoints
{
    public static void MapFamilyMaintenanceEndpoints(this WebApplication app)
    {
        app.MapPost("/people/{personId:guid}/routines", AddRoutineAsync);
        app.MapPut("/people/{personId:guid}/routines/{id:guid}", UpdateRoutineAsync);
        app.MapPost("/people/{personId:guid}/preferences", AddPreferenceAsync);
        app.MapPut("/people/{personId:guid}/preferences/{id:guid}", UpdatePreferenceAsync);
        app.MapPost("/people/{personId:guid}/support-contents", AddSupportContentAsync);
        app.MapPut("/people/{personId:guid}/support-contents/{id:guid}", UpdateSupportContentAsync);
        app.MapPost("/people/{personId:guid}/belongings", AddBelongingAsync);
        app.MapPut("/people/{personId:guid}/belongings/{id:guid}", UpdateBelongingAsync);
        app.MapPost("/people/{personId:guid}/trusted-contacts", AddTrustedContactAsync);
        app.MapPut("/people/{personId:guid}/trusted-contacts/{id:guid}", UpdateTrustedContactAsync);
        app.MapGet("/content-topics", async (ContentTopicRepository topics) =>
            Results.Ok(await topics.GetAllAsync()));
        app.MapPut("/life-events/{id:guid}/topics", UpdateLifeEventTopicsAsync);
        app.MapPost("/people/{personId:guid}/life-events", AddLifeEventAsync);
        app.MapGet("/people/{personId:guid}/agenda", GetAgendaAsync);
        app.MapPost("/people/{personId:guid}/agenda", AddAgendaItemAsync);
    }

    private static async Task<IResult> AddRoutineAsync(
        Guid personId, PersonRoutineRequest request,
        PersonRepository people, PersonRoutineRepository repository)
    {
        if (await people.GetByIdAsync(personId) is null) return Results.NotFound();
        if (Missing(request.Title, request.Notes)) return Results.BadRequest();
        var item = new PersonRoutine
        {
            Id = Guid.NewGuid(), PersonId = personId,
            Title = request.Title.Trim(), Notes = request.Notes.Trim(),
            IsActive = request.IsActive
        };
        await repository.AddAsync(item);
        return Results.Created($"/people/{personId}/routines/{item.Id}", item);
    }

    private static async Task<IResult> UpdateRoutineAsync(
        Guid personId, Guid id, PersonRoutineRequest request,
        PersonRoutineRepository repository)
    {
        if (Missing(request.Title, request.Notes)) return Results.BadRequest();
        var item = new PersonRoutine
        {
            Id = id, PersonId = personId, Title = request.Title.Trim(),
            Notes = request.Notes.Trim(), IsActive = request.IsActive
        };
        return await repository.UpdateAsync(item)
            ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> AddPreferenceAsync(
        Guid personId, PersonPreferenceRequest request,
        PersonRepository people, PersonPreferenceRepository repository,
        ContentTopicRepository topics)
    {
        if (await people.GetByIdAsync(personId) is null) return Results.NotFound();
        if (Missing(request.Title, request.Notes)) return Results.BadRequest();
        if (!await topics.AreCodesValidAsync(request.TopicCodes, "Interest"))
            return Results.BadRequest("TopicCodes de preferencias invalidos.");
        var item = Preference(Guid.NewGuid(), personId, request);
        await repository.AddAsync(item);
        await topics.ReplacePreferenceCodesAsync(item.Id, request.TopicCodes);
        return Results.Created($"/people/{personId}/preferences/{item.Id}", item);
    }

    private static async Task<IResult> UpdatePreferenceAsync(
        Guid personId, Guid id, PersonPreferenceRequest request,
        PersonPreferenceRepository repository, ContentTopicRepository topics)
    {
        if (Missing(request.Title, request.Notes)) return Results.BadRequest();
        if (!await topics.AreCodesValidAsync(request.TopicCodes, "Interest"))
            return Results.BadRequest("TopicCodes de preferencias invalidos.");
        if (!await repository.UpdateAsync(Preference(id, personId, request)))
            return Results.NotFound();
        await topics.ReplacePreferenceCodesAsync(id, request.TopicCodes);
        return Results.NoContent();
    }

    private static async Task<IResult> AddSupportContentAsync(
        Guid personId, PersonSupportContentRequest request,
        PersonRepository people, PersonSupportContentRepository repository,
        ContentTopicRepository topics)
    {
        if (await people.GetByIdAsync(personId) is null) return Results.NotFound();
        if (Missing(request.Title, request.Content)) return Results.BadRequest();
        if (!await topics.AreCodesValidAsync(request.TopicCodes, "Reading"))
            return Results.BadRequest("TopicCodes de lecturas invalidos.");
        var item = SupportContent(Guid.NewGuid(), personId, request);
        await repository.AddAsync(item);
        await topics.ReplaceSupportContentCodesAsync(item.Id, request.TopicCodes);
        return Results.Created(
            $"/people/{personId}/support-contents/{item.Id}", item);
    }

    private static async Task<IResult> UpdateSupportContentAsync(
        Guid personId, Guid id, PersonSupportContentRequest request,
        PersonSupportContentRepository repository, ContentTopicRepository topics)
    {
        if (Missing(request.Title, request.Content)) return Results.BadRequest();
        if (!await topics.AreCodesValidAsync(request.TopicCodes, "Reading"))
            return Results.BadRequest("TopicCodes de lecturas invalidos.");
        if (!await repository.UpdateAsync(SupportContent(id, personId, request)))
            return Results.NotFound();
        await topics.ReplaceSupportContentCodesAsync(id, request.TopicCodes);
        return Results.NoContent();
    }

    private static async Task<IResult> AddBelongingAsync(
        Guid personId, PersonBelongingRequest request,
        PersonRepository people, PersonBelongingRepository repository)
    {
        if (await people.GetByIdAsync(personId) is null) return Results.NotFound();
        if (Missing(request.Name, request.Notes)) return Results.BadRequest();
        var item = Belonging(Guid.NewGuid(), personId, request);
        await repository.AddAsync(item);
        return Results.Created($"/people/{personId}/belongings/{item.Id}", item);
    }

    private static async Task<IResult> UpdateBelongingAsync(
        Guid personId, Guid id, PersonBelongingRequest request,
        PersonBelongingRepository repository)
    {
        if (Missing(request.Name, request.Notes)) return Results.BadRequest();
        return await repository.UpdateAsync(Belonging(id, personId, request))
            ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> AddTrustedContactAsync(
        Guid personId, PersonTrustedContactRequest request,
        PersonRepository people, PersonTrustedContactRepository repository)
    {
        if (request.ContactPersonId == personId || request.Priority < 1 ||
            await people.GetByIdAsync(personId) is null ||
            await people.GetByIdAsync(request.ContactPersonId) is null)
        {
            return Results.BadRequest();
        }
        var item = TrustedContact(Guid.NewGuid(), personId, request);
        await repository.AddAsync(item);
        return Results.Created(
            $"/people/{personId}/trusted-contacts/{item.Id}", item);
    }

    private static async Task<IResult> UpdateTrustedContactAsync(
        Guid personId, Guid id, PersonTrustedContactRequest request,
        PersonTrustedContactRepository repository)
    {
        if (request.ContactPersonId == personId || request.Priority < 1)
            return Results.BadRequest();
        return await repository.UpdateAsync(TrustedContact(id, personId, request))
            ? Results.NoContent() : Results.NotFound();
    }

    private static bool Missing(string first, string second) =>
        string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second);

    private static async Task<IResult> UpdateLifeEventTopicsAsync(
        Guid id, TopicAssignmentRequest request, LifeEventRepository events,
        ContentTopicRepository topics)
    {
        if (!await events.ExistsAsync(id)) return Results.NotFound();
        if (!await topics.AreCodesValidAsync(request.TopicCodes, "Memory"))
            return Results.BadRequest("TopicCodes de recuerdos invalidos.");
        await topics.ReplaceLifeEventCodesAsync(id, request.TopicCodes);
        return Results.NoContent();
    }

    private static async Task<IResult> AddLifeEventAsync(
        Guid personId,
        PersonLifeEventRequest request,
        PersonRepository people,
        LifeEventRepository events,
        ContentTopicRepository topics)
    {
        var person = await people.GetByIdAsync(personId);
        if (person is null) return Results.NotFound();
        if (string.IsNullOrWhiteSpace(request.Title) ||
            request.EndDate is not null && request.StartDate is not null &&
            request.EndDate < request.StartDate)
        {
            return Results.BadRequest(
                "El titulo es obligatorio y EndDate no puede ser anterior a StartDate.");
        }
        if (!await topics.AreCodesValidAsync(request.TopicCodes, "Memory"))
        {
            return Results.BadRequest(
                "TopicCodes contiene temas inexistentes o que no pertenecen a Memory.");
        }

        var lifeEvent = new LifeEvent
        {
            Id = Guid.NewGuid(),
            PersonId = personId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DatePrecision = request.DatePrecision,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Place = request.Place?.Trim(),
            IsPositiveMemory = request.IsPositiveMemory
        };
        var participantResponses = new List<LifeEventParticipantResponse>();
        var participantItems = new List<LifeEventParticipant>();
        foreach (var participant in request.Participants
            .GroupBy(item => item.PersonId)
            .Select(group => group.First()))
        {
            if (participant.PersonId == personId)
            {
                return Results.BadRequest(
                    "La persona dueña del evento no debe repetirse como participante.");
            }
            var participantPerson = await people.GetByIdAsync(
                participant.PersonId);
            if (participantPerson is null)
            {
                return Results.BadRequest(
                    $"El participante {participant.PersonId} no existe.");
            }
            var role = participant.Role?.Trim();
            participantItems.Add(new LifeEventParticipant
            {
                LifeEventId = lifeEvent.Id,
                PersonId = participant.PersonId,
                Role = role
            });
            participantResponses.Add(new LifeEventParticipantResponse
            {
                PersonName = participantPerson.Name,
                Role = role
            });
        }
        await events.AddWithTopicsAsync(
            lifeEvent, request.TopicCodes, participantItems);

        return Results.Created(
            $"/people/{personId}/life-events/{lifeEvent.Id}",
            new LifeEventResponse
            {
                Id = lifeEvent.Id,
                PersonId = lifeEvent.PersonId,
                PersonName = person.Name,
                StartDate = lifeEvent.StartDate,
                EndDate = lifeEvent.EndDate,
                DatePrecision = lifeEvent.DatePrecision,
                Title = lifeEvent.Title,
                Description = lifeEvent.Description,
                Place = lifeEvent.Place,
                IsPositiveMemory = lifeEvent.IsPositiveMemory,
                TopicCodes = [.. request.TopicCodes
                    .Where(code => !string.IsNullOrWhiteSpace(code))
                    .Select(code => code.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)],
                Participants = participantResponses
            });
    }

    private static async Task<IResult> GetAgendaAsync(
        Guid personId,
        bool? includePast,
        PersonRepository people,
        PersonAgendaItemRepository agenda,
        ContentTopicRepository topics)
    {
        var person = await people.GetByIdAsync(personId);
        if (person is null) return Results.NotFound();
        var items = await agenda.GetByPersonAsync(
            personId, includePast ?? false, DateTimeOffset.UtcNow);
        var response = new List<PersonAgendaItemResponse>();
        foreach (var item in items)
        {
            var participantResponses = new List<LifeEventParticipantResponse>();
            foreach (var participant in await agenda.GetParticipantsAsync(item.Id))
            {
                var participantPerson = await people.GetByIdAsync(
                    participant.PersonId);
                if (participantPerson is not null)
                    participantResponses.Add(new LifeEventParticipantResponse
                    {
                        PersonName = participantPerson.Name,
                        Role = participant.Role
                    });
            }
            response.Add(new PersonAgendaItemResponse
            {
                Id = item.Id, PersonId = item.PersonId,
                PersonName = person.Name, ScheduledAt = item.ScheduledAt,
                EndAt = item.EndAt, Title = item.Title,
                Description = item.Description, Place = item.Place,
                Status = item.Status,
                TopicCodes = [.. await topics.GetAgendaItemCodesAsync(item.Id)],
                Participants = participantResponses
            });
        }
        return Results.Ok(response);
    }

    private static async Task<IResult> AddAgendaItemAsync(
        Guid personId,
        PersonAgendaItemRequest request,
        PersonRepository people,
        PersonAgendaItemRepository agenda,
        ContentTopicRepository topics)
    {
        var person = await people.GetByIdAsync(personId);
        if (person is null) return Results.NotFound();
        if (string.IsNullOrWhiteSpace(request.Title) ||
            request.ScheduledAt == default ||
            request.EndAt is not null && request.EndAt < request.ScheduledAt)
            return Results.BadRequest(
                "Titulo y ScheduledAt son obligatorios; EndAt no puede ser anterior.");
        if (!Enum.IsDefined(request.Status))
            return Results.BadRequest("El estado de agenda no es valido.");
        if (!await topics.AreCodesValidAsync(request.TopicCodes, "Agenda"))
            return Results.BadRequest("TopicCodes contiene temas que no son de Agenda.");

        var participantItems = new List<LifeEventParticipant>();
        var participantResponses = new List<LifeEventParticipantResponse>();
        foreach (var participant in request.Participants
            .GroupBy(item => item.PersonId).Select(group => group.First()))
        {
            if (participant.PersonId == personId)
                return Results.BadRequest(
                    "La persona de la agenda no debe repetirse como participante.");
            var participantPerson = await people.GetByIdAsync(participant.PersonId);
            if (participantPerson is null)
                return Results.BadRequest(
                    $"El participante {participant.PersonId} no existe.");
            var role = participant.Role?.Trim();
            participantItems.Add(new LifeEventParticipant
            {
                PersonId = participant.PersonId, Role = role
            });
            participantResponses.Add(new LifeEventParticipantResponse
            {
                PersonName = participantPerson.Name, Role = role
            });
        }
        var item = new PersonAgendaItem
        {
            Id = Guid.NewGuid(), PersonId = personId,
            ScheduledAt = request.ScheduledAt, EndAt = request.EndAt,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Place = request.Place?.Trim(), Status = request.Status
        };
        await agenda.AddAsync(item, request.TopicCodes, participantItems);
        return Results.Created($"/people/{personId}/agenda/{item.Id}",
            new PersonAgendaItemResponse
            {
                Id = item.Id, PersonId = personId, PersonName = person.Name,
                ScheduledAt = item.ScheduledAt, EndAt = item.EndAt,
                Title = item.Title, Description = item.Description,
                Place = item.Place, Status = item.Status,
                TopicCodes = [.. request.TopicCodes],
                Participants = participantResponses
            });
    }

    private static PersonPreference Preference(
        Guid id, Guid personId, PersonPreferenceRequest request) => new()
        {
            Id = id, PersonId = personId, Title = request.Title.Trim(),
            Notes = request.Notes.Trim(), Tags = request.Tags?.Trim(),
            IsActive = request.IsActive
        };

    private static PersonSupportContent SupportContent(
        Guid id, Guid personId, PersonSupportContentRequest request) => new()
        {
            Id = id, PersonId = personId, Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            Attribution = request.Attribution?.Trim(),
            Reference = request.Reference?.Trim(), Tags = request.Tags?.Trim(),
            IsActive = request.IsActive
        };

    private static PersonBelonging Belonging(
        Guid id, Guid personId, PersonBelongingRequest request) => new()
        {
            Id = id, PersonId = personId, Name = request.Name.Trim(),
            Notes = request.Notes.Trim(), Tags = request.Tags?.Trim(),
            IsActive = request.IsActive
        };

    private static PersonTrustedContact TrustedContact(
        Guid id, Guid personId, PersonTrustedContactRequest request) => new()
        {
            Id = id, PersonId = personId,
            ContactPersonId = request.ContactPersonId,
            Priority = request.Priority, Notes = request.Notes?.Trim(),
            IsActive = request.IsActive
        };
}
