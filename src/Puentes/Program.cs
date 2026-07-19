using Dapper;
using Puentes.Core.Domain;
using Puentes.Core.Requests;
using Puentes.Core.Requests.Medication;
using Puentes.Infrastructure.Configuration;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Repositories;
using System.Text.Json.Serialization;

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
var app = builder.Build();
//Initialze database
using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    initializer.Initialize();

// Habilitar Swagger en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Puentes API V1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz (http://localhost:5000/)
    });
}
List<Event> events = [];

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
app.MapPost("/medications",
async (
    CreateMedicationRequest request,
    MedicationRepository repository) =>
{
    var medication = new Medication
    {
        Id = Guid.NewGuid(),
        Name = request.Name,
        Dose = request.Dose,
        Quantity = request.Quantity,
        Form = request.Form,
        Shape = request.Shape,
        Color = request.Color ?? string.Empty,
        Instructions = request.Instructions,
        IsActive = true
    };

    await repository.AddAsync(medication);

    return Results.Created(
        $"/medications/{medication.Id}",
        medication);
});
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

app.Run();
