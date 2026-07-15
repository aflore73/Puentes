using Puentes.Core;
using Puentes.Core.Domain;
using Puentes.Core.Requests;
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();


app.MapGet("/status", () =>
{
    return Results.Ok(new
    {
        Name = "Puentes",
        Version = "0.1",
        Status = "Running"
    });
});
app.MapPost("/events", (RegisterEventRequest request) =>
{
    var evento = new Event
    {
        Id = Guid.NewGuid(),
        PersonId = request.PersonId,
        Type = request.Type,
        Description = request.Description,
        OccurredAt = DateTimeOffset.UtcNow
    };

    return Results.Created($"/events/{evento.Id}", evento);
});
app.Run();
