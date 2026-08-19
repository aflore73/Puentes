using Dapper;
using Puentes.Api.Requests.Medications;
using Puentes.Shared.Domain;
using Puentes.Shared.Enums;
using Puentes.Shared.Requests;
using Puentes.Shared.Responses;
using Puentes.Shared.Requests.Medication;
using Puentes.Shared.Requests.People;
using Puentes.Shared.Responses.People;
using Puentes.Shared.Responses.LifeEvents;
using Puentes.Infrastructure.Configuration;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Repositories;
using System.Text.Json.Serialization;
using Puentes.Api;

var builder = WebApplication.CreateBuilder(args);
//Acceso a la base de datos
var connectionString =
    builder.Configuration.GetConnectionString("Puentes")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'Puentes'.");
builder.Services.AddSingleton<AccessDb>(_ => new AccessDb(connectionString));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<EventRepository>();
builder.Services.AddScoped<MedicationRepository>();
builder.Services.AddScoped<MedicationScheduleRepository>();
builder.Services.AddScoped<PersonRepository>();
builder.Services.AddScoped<PersonRelationshipRepository>();
builder.Services.AddScoped<LifeEventRepository>();
builder.Services.AddScoped<PersonRoutineRepository>();
builder.Services.AddScoped<PersonPreferenceRepository>();
builder.Services.AddScoped<PersonSupportContentRepository>();
builder.Services.AddScoped<PersonBelongingRepository>();
builder.Services.AddScoped<PersonTrustedContactRepository>();
builder.Services.AddScoped<ContentTopicRepository>();
builder.Services.AddScoped<PersonAgendaItemRepository>();
// Repositorio de registros de medicación
builder.Services.AddSingleton<MedicationRecordRepository>();
//Configuración para serialización JSON de enums
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

/**/
// Agregar servicios de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Puentes API",
        Version = "v1",
        Description = "API para eventos de puentes"
    });
});

// Configurar serialización JSON para usar nombres de propiedad en camelCase
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//Dapper
SqlMapper.AddTypeHandler(new GuidTypeHandler());
SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
var app = builder.Build();
//Initialze database
using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();

// Panel local de mantenimiento para la familia.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Puentes API V1");
    c.RoutePrefix = "family";
});
List<Event> events = [];

app.MapGet("/health", (AccessDb accessDb) =>
{
    using var connection = accessDb.OpenConnection();
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT 1;";
    command.ExecuteScalar();

    return Results.Ok(new
    {
        status = "healthy",
        database = "available"
    });
});

app.MapGet("/events",
async (EventRepository repository) =>
{
    var eventos = await repository.GetAllAsync();

    return Results.Ok(eventos);
});

app.MapPost("/events",
async (RegisterEventRequest request,
       EventRepository repository) =>
{
    var evento = new Event
    {
        Id = Guid.NewGuid(),
        PersonId = request.PersonId,
        Type = request.Type,
        Description = request.Description,
        OccurredAt = DateTimeOffset.UtcNow
    };

    await repository.AddAsync(evento);

    return Results.Created($"/events/{evento.Id}", evento);
});
//Medications

app.MapGet("/medications",
async (MedicationRepository repository) =>
{
    var medications = await repository.GetAllAsync();

    return Results.Ok(medications);
});
app.MapGet("/medications/{id:guid}",
async (Guid id,
       MedicationRepository repository) =>
{
    var medication = await repository.GetByIdAsync(id);

    if (medication is null)
        return Results.NotFound();

    return Results.Ok(medication);
});
app.MapPost("/medications",
async (
    MedicationRequest request,
    MedicationRepository repository) =>
{
    var medication = new Medication
    {
        Id = Guid.NewGuid(),
        Name = request.Name,
        Dose = request.Dose,
        Form = request.Form,
        Shape = request.Shape,
        Color = request.Color ?? string.Empty,
        Instructions = request.Instructions,
        IsActive = request.IsActive
    };

    await repository.AddAsync(medication);

    return Results.Created(
        $"/medications/{medication.Id}",
        medication);
});
app.MapPut("/medications/{id:guid}",
async (
    Guid id,
    MedicationRequest request,
    MedicationRepository repository) =>
{
    var medication = await repository.GetByIdAsync(id);

    if (medication is null)
        return Results.NotFound();

    medication.Name = request.Name;
    medication.Dose = request.Dose;
    medication.Form = request.Form;
    medication.Shape = request.Shape;
    medication.Color = request.Color ?? string.Empty;
    medication.Instructions = request.Instructions;
    medication.IsActive = request.IsActive;

    await repository.UpdateAsync(medication);

    return Results.NoContent();
});
//Medication Turns Schedules
app.MapPut("/medications/schedules",
async (
    List<MedicationScheduleBatchRequest> request,
    MedicationScheduleRepository repository) =>
{
    await repository.ReplaceBatchAsync(request);

    return Results.NoContent();
});

app.MapPut("/medications/{id:guid}/schedules",
async (
    Guid id,
    List<MedicationScheduleItemRequest> request,
    MedicationRepository medicationRepository,
    MedicationScheduleRepository scheduleRepository) =>
{
    var medication = await medicationRepository.GetByIdAsync(id);

    if (medication is null)
        return Results.NotFound();

    var schedules = request.Select(x => new MedicationSchedule
    {
        Id = Guid.NewGuid(),
        MedicationId = id,
        Turn = x.Turn,
        Quantity = x.Quantity,
        IsActive = true
    });

    await scheduleRepository.ReplaceAsync(id, schedules);

    return Results.NoContent();
});
app.MapGet("/medications/{id:guid}/schedules",
async (
    Guid id,
    MedicationScheduleRepository repository) =>
{
    var schedules = await repository.GetByMedicationIdAsync(id);

    return Results.Ok(schedules);
});
app.MapGet("/medication-plan",
async (MedicationScheduleRepository repository) =>
{
    var rows = await repository.GetPlanAsync();

    var result = rows
        .GroupBy(x => x.Turn)
        .Select(g => new MedicationPlanResponse
        {
            Turn = g.Key,
            Medications = g.Select(x => new MedicationPlanItemResponse
            {
                Name = x.Name,
                Quantity = x.Quantity,
                Form = x.Form,
                Shape = x.Shape,
                Color = x.Color,
                SpeakName = x.SpeakName
            }).ToList()
        });

    return Results.Ok(result);
});
//Medication Records
app.MapPost("/medication-records",
async (
    RegisterMedicationRecordRequest request,
    MedicationRecordRepository repository) =>
{
    var existing = await repository.GetTodayAsync(
    request.PatientId,
    request.Turn);

    if (existing is not null)
    {
        return Results.Conflict(
            "Ya existe un registro para este turno en el día de hoy.");
    }

    var record = new MedicationRecord
    {
        Id = Guid.NewGuid(),
        PatientId = request.PatientId,
        Turn = request.Turn,
        RecordedAt = DateTime.Now,
        Confirmed = request.Confirmed,
        Notes = request.Notes
    };

    await repository.AddAsync(record);

    return Results.Created(
        $"/medication-records/{record.Id}",
        record);
});

app.MapGet(
"/patients/{patientId:guid}/medication-records/today",
async (
    Guid patientId,
    MedicationRecordRepository repository) =>
{
    var records = await repository.GetTodayAsync(patientId);

    return Results.Ok(records);
});

app.MapGet(
"/patients/{patientId:guid}/medication-records/today/{turn}",
async (
    Guid patientId,
    MedicationTurnType turn,
    MedicationRecordRepository repository) =>
{
    var record = await repository.GetTodayAsync(
        patientId,
        turn);

    if (record is null)
        return Results.NotFound();

    return Results.Ok(record);
});

// People
app.MapGet("/people",
async (PersonRepository repository) =>
{
    var people = await repository.GetAllAsync();
    return Results.Ok(people);
});

app.MapGet("/people/{id:guid}",
async (Guid id, PersonRepository repository) =>
{
    var person = await repository.GetByIdAsync(id);
    return person is null
        ? Results.NotFound()
        : Results.Ok(person);
});

app.MapPost("/people",
async (CreatePersonRequest request, PersonRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.Name)] = ["El nombre es obligatorio."]
        });
    }

    var person = new Person
    {
        Id = Guid.NewGuid(),
        Name = request.Name.Trim(),
        BirthDate = request.BirthDate?.ToDateTime(TimeOnly.MinValue),
        City = request.City.Trim(),
        Province = request.Province.Trim(),
        Country = request.Country.Trim(),
        Notes = request.Notes?.Trim()
    };

    await repository.AddAsync(person);

    return Results.Created($"/people/{person.Id}", person);
});

app.MapPut("/people/{id:guid}",
async (Guid id, CreatePersonRequest request, PersonRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) ||
        string.IsNullOrWhiteSpace(request.City) ||
        string.IsNullOrWhiteSpace(request.Province) ||
        string.IsNullOrWhiteSpace(request.Country))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.Name)] =
                ["Nombre, ciudad, provincia y pais son obligatorios."]
        });
    }

    var person = new Person
    {
        Id = id,
        Name = request.Name.Trim(),
        BirthDate = request.BirthDate?.ToDateTime(TimeOnly.MinValue),
        City = request.City.Trim(),
        Province = request.Province.Trim(),
        Country = request.Country.Trim(),
        Notes = request.Notes?.Trim()
    };

    return await repository.UpdateAsync(person)
        ? Results.NoContent()
        : Results.NotFound();
});

app.MapGet("/people/{id:guid}/relationships",
async (
    Guid id,
    PersonRepository personRepository,
    PersonRelationshipRepository relationshipRepository) =>
{
    if (await personRepository.GetByIdAsync(id) is null)
    {
        return Results.NotFound();
    }

    var relationships = await relationshipRepository
        .GetAllForPersonAsync(id);

    var connections = new List<PersonConnectionResponse>();

    foreach (var relationship in relationships)
    {
        var isOutgoing = relationship.PersonId == id;
        var otherPersonId = isOutgoing
            ? relationship.RelatedPersonId
            : relationship.PersonId;
        var otherPerson = await personRepository
            .GetByIdAsync(otherPersonId);

        if (otherPerson is null)
        {
            continue;
        }

        connections.Add(new PersonConnectionResponse
        {
            RelationshipId = relationship.Id,
            Type = relationship.Type,
            Direction = isOutgoing
                ? RelationshipDirection.Outgoing
                : RelationshipDirection.Incoming,
            OtherPerson = new PersonSummaryResponse
            {
                Id = otherPerson.Id,
                Name = otherPerson.Name,
                BirthDate = otherPerson.BirthDate,
                City = otherPerson.City,
                Province = otherPerson.Province,
                Country = otherPerson.Country
            },
            Notes = relationship.Notes
        });
    }

    return Results.Ok(connections);
});

app.MapPost("/people/{id:guid}/relationships",
async (
    Guid id,
    CreatePersonRelationshipRequest request,
    PersonRepository personRepository,
    PersonRelationshipRepository relationshipRepository) =>
{
    if (id == request.RelatedPersonId)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.RelatedPersonId)] =
                ["Una persona no puede relacionarse consigo misma."]
        });
    }

    if (request.Type == PersonRelationshipType.Unknown
        || !Enum.IsDefined(request.Type))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.Type)] = ["El tipo de relación no es válido."]
        });
    }

    var person = await personRepository.GetByIdAsync(id);
    var relatedPerson = await personRepository
        .GetByIdAsync(request.RelatedPersonId);

    if (person is null || relatedPerson is null)
    {
        return Results.NotFound(
            "La persona de origen o la persona relacionada no existe.");
    }

    var relationship = new PersonRelationship
    {
        Id = Guid.NewGuid(),
        PersonId = id,
        RelatedPersonId = request.RelatedPersonId,
        Type = request.Type,
        Notes = request.Notes
    };

    await relationshipRepository.AddAsync(relationship);

    return Results.Created(
        $"/people/{id}/relationships/{relationship.Id}",
        relationship);
});

app.MapGet("/people/{id:guid}/life-events",
async (
    Guid id,
    PersonRepository personRepository,
    LifeEventRepository lifeEventRepository,
    ContentTopicRepository topicRepository) =>
{
    var owner = await personRepository.GetByIdAsync(id);
    if (owner is null)
    {
        return Results.NotFound();
    }

    var lifeEvents = await lifeEventRepository.GetByPersonAsync(id);
    var response = new List<LifeEventResponse>();

    foreach (var lifeEvent in lifeEvents)
    {
        var participants = await lifeEventRepository
            .GetParticipantsAsync(lifeEvent.Id);
        var participantResponses = new List<LifeEventParticipantResponse>();

        foreach (var participant in participants)
        {
            var person = await personRepository.GetByIdAsync(
                participant.PersonId);

            if (person is null)
            {
                continue;
            }

            participantResponses.Add(new LifeEventParticipantResponse
            {
                PersonName = person.Name,
                Role = participant.Role
            });
        }

        response.Add(new LifeEventResponse
        {
            Id = lifeEvent.Id,
            PersonId = lifeEvent.PersonId,
            PersonName = owner.Name,
            StartDate = lifeEvent.StartDate,
            EndDate = lifeEvent.EndDate,
            DatePrecision = lifeEvent.DatePrecision,
            Title = lifeEvent.Title,
            TopicCodes = [.. await topicRepository
                .GetLifeEventCodesAsync(lifeEvent.Id)],
            Description = lifeEvent.Description,
            Place = lifeEvent.Place,
            IsPositiveMemory = lifeEvent.IsPositiveMemory,
            Participants = participantResponses
        });
    }

    return Results.Ok(response);
});

app.MapGet("/people/{id:guid}/routines",
async (
    Guid id,
    PersonRepository personRepository,
    PersonRoutineRepository routineRepository) =>
{
    var person = await personRepository.GetByIdAsync(id);
    if (person is null)
    {
        return Results.NotFound();
    }

    var routines = await routineRepository.GetActiveByPersonAsync(id);
    var response = routines.Select(routine => new PersonRoutineResponse
    {
        Id = routine.Id,
        PersonId = routine.PersonId,
        PersonName = person.Name,
        Title = routine.Title,
        Notes = routine.Notes,
        IsActive = routine.IsActive
    });

    return Results.Ok(response);
});

app.MapGet("/people/{id:guid}/preferences",
async (
    Guid id,
    PersonRepository personRepository,
    PersonPreferenceRepository preferenceRepository,
    ContentTopicRepository topicRepository) =>
{
    var person = await personRepository.GetByIdAsync(id);
    if (person is null)
    {
        return Results.NotFound();
    }

    var preferences = await preferenceRepository.GetActiveByPersonAsync(id);
    var response = new List<PersonPreferenceResponse>();
    foreach (var preference in preferences)
    {
        response.Add(new PersonPreferenceResponse
        {
            Id = preference.Id,
            PersonId = preference.PersonId,
            PersonName = person.Name,
            Title = preference.Title,
            Notes = preference.Notes,
            Tags = preference.Tags,
            TopicCodes = [.. await topicRepository
                .GetPreferenceCodesAsync(preference.Id)],
            IsActive = preference.IsActive
        });
    }

    return Results.Ok(response);
});

app.MapGet("/people/{id:guid}/support-contents",
async (
    Guid id,
    PersonRepository personRepository,
    PersonSupportContentRepository supportContentRepository,
    ContentTopicRepository topicRepository) =>
{
    var person = await personRepository.GetByIdAsync(id);
    if (person is null)
    {
        return Results.NotFound();
    }

    var contents = await supportContentRepository
        .GetActiveByPersonAsync(id);
    var response = new List<PersonSupportContentResponse>();
    foreach (var content in contents)
    {
        response.Add(new PersonSupportContentResponse
        {
            Id = content.Id,
            PersonId = content.PersonId,
            PersonName = person.Name,
            Title = content.Title,
            Content = content.Content,
            TopicCodes = [.. await topicRepository
                .GetSupportContentCodesAsync(content.Id)],
            Attribution = content.Attribution,
            Reference = content.Reference,
            Tags = content.Tags,
            IsActive = content.IsActive
        });
    }

    return Results.Ok(response);
});

app.MapGet("/people/{id:guid}/belongings",
async (
    Guid id,
    PersonRepository personRepository,
    PersonBelongingRepository belongingRepository) =>
{
    var person = await personRepository.GetByIdAsync(id);
    if (person is null)
    {
        return Results.NotFound();
    }

    var belongings = await belongingRepository.GetActiveByPersonAsync(id);
    var response = belongings.Select(belonging =>
        new PersonBelongingResponse
        {
            Id = belonging.Id,
            PersonId = belonging.PersonId,
            PersonName = person.Name,
            Name = belonging.Name,
            Notes = belonging.Notes,
            Tags = belonging.Tags,
            IsActive = belonging.IsActive
        });

    return Results.Ok(response);
});

app.MapGet("/people/{id:guid}/trusted-contacts", async (
    Guid id,
    PersonRepository people,
    PersonTrustedContactRepository repository) =>
{
    if (await people.GetByIdAsync(id) is null) return Results.NotFound();
    var contacts = await repository.GetActiveAsync(id);
    var response = new List<PersonTrustedContactResponse>();
    foreach (var contact in contacts)
    {
        var person = await people.GetByIdAsync(contact.ContactPersonId);
        if (person is null) continue;
        response.Add(new PersonTrustedContactResponse
        {
            Id = contact.Id,
            PersonId = contact.PersonId,
            ContactPersonId = contact.ContactPersonId,
            ContactPersonName = person.Name,
            Priority = contact.Priority,
            Notes = contact.Notes,
            IsActive = contact.IsActive
        });
    }
    return Results.Ok(response);
});

app.MapFamilyMaintenanceEndpoints();

app.Run();
