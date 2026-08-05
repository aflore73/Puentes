using Puentes.Orchestrator.AI;
using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;
using Puentes.Orchestrator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5121/");
});
builder.Services.AddSingleton<MedicationReminderStateService>();
builder.Services.AddSingleton<MedicationConfirmationService>();
builder.Services.AddSingleton<AiContextBuilderService>();
builder.Services.AddSingleton<InMemoryConversationStore>();
if (builder.Configuration.GetValue<bool>("UseOpenAi"))
{
    builder.Services.AddSingleton<IAssistantService, OpenAiAssistantService>();
}
else
{
    builder.Services.AddSingleton<IAssistantService, FakeAiAssistantService>();
}
builder.Services.AddSingleton<MedicationWorkflowService>();
builder.Services.AddSingleton<MemoryConversationWorkflowService>();
builder.Services.AddSingleton<IConversationService,ConversationService>();
builder.Services.AddSingleton<PromptFactory>();
var openAiOptions = new OpenAiOptions
{
    ApiKey = Environment.GetEnvironmentVariable("PUENTES_API_KEY") ?? string.Empty,
    Model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-5.5"
};

builder.Services.AddSingleton(openAiOptions);
var host = builder.Build();

host.MapPost("/memory-support/simulate",
async (
    MemoryConversationRequest request,
    MemoryConversationWorkflowService workflow,
    CancellationToken cancellationToken) =>
{
    if (request.PersonId == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.PersonId)] = ["La persona es obligatoria."]
        });
    }

    if (string.IsNullOrWhiteSpace(request.UserInput))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.UserInput)] = ["La pregunta es obligatoria."]
        });
    }

    try
    {
        var response = await workflow.ProcessAsync(
            request.PersonId,
            request.UserInput.Trim(),
            cancellationToken);

        return Results.Ok(response);
    }
    catch (InvalidOperationException exception)
    {
        return Results.NotFound(exception.Message);
    }
});

host.MapPost("/memory-support/conversations",
async (
    MemoryConversationRequest request,
    MemoryConversationWorkflowService workflow,
    CancellationToken cancellationToken) =>
{
    if (request.PersonId == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.PersonId)] = ["La persona es obligatoria."]
        });
    }

    if (string.IsNullOrWhiteSpace(request.UserInput))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.UserInput)] = ["El mensaje es obligatorio."]
        });
    }

    try
    {
        var response = await workflow.StartAsync(
            request.PersonId,
            request.UserInput.Trim(),
            cancellationToken);

        return Results.Ok(response);
    }
    catch (InvalidOperationException exception)
    {
        return Results.NotFound(exception.Message);
    }
});

host.MapPost("/memory-support/conversations/{conversationId:guid}/messages",
async (
    Guid conversationId,
    ContinueMemoryConversationRequest request,
    MemoryConversationWorkflowService workflow,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.UserInput))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.UserInput)] = ["El mensaje es obligatorio."]
        });
    }

    try
    {
        var response = await workflow.ContinueAsync(
            conversationId,
            request.UserInput.Trim(),
            cancellationToken);

        return Results.Ok(response);
    }
    catch (InvalidOperationException exception)
    {
        return Results.NotFound(exception.Message);
    }
});

host.Run();
