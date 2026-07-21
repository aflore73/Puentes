
using Puentes.Orchestrator.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5121/");
});
builder.Services.AddSingleton<MedicationReminderService>();
builder.Services.AddSingleton<MedicationReminderStateService>();
builder.Services.AddSingleton<MedicationMessageService>();
builder.Services.AddHostedService<Worker>();
var host = builder.Build();
host.Run();
