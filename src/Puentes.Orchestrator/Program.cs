using Puentes.Orchestrator.Services;
using Puentes.Orchestrator.AI;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5121/");
});
builder.Services.AddSingleton<MedicationReminderStateService>();
builder.Services.AddSingleton<AiContextBuilderService>();
builder.Services.AddSingleton<IAssistantService, OpenAiAssistantService>();
builder.Services.AddSingleton<MedicationWorkflowService>();
builder.Services.AddSingleton<IConversationService,ConversationService>();
builder.Services.AddHostedService<Worker>();
var openAiOptions = new OpenAiOptions
{
    ApiKey = Environment.GetEnvironmentVariable("PUENTES_API_KEY") ?? string.Empty,
    Model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-5.5"
};

builder.Services.AddSingleton(openAiOptions);
var host = builder.Build();
host.Run();
