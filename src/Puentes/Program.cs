using Puentes.Core;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
app.MapPost("/events", (Event event) =>
{
    return Results.Ok(new
    {
        Message = "Event received",
        Event = event
    });
});
app.Run();
