using Puentes.Orchestrator.AI;
using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;
using Puentes.Orchestrator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.Configure<PeripheralActivationOptions>(
    builder.Configuration.GetSection("PeripheralActivation"));
builder.Services.AddSingleton<AudioCueService>();
builder.Services.AddHostedService<PeripheralActivationWorker>();
builder.Services.AddSingleton<PuentesDiagnosticsService>();
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["PuentesApi:BaseUrl"]
        ?? "http://localhost:5121/");
});
builder.Services.AddSingleton<MedicationReminderStateService>();
builder.Services.AddSingleton<MedicationConfirmationService>();
builder.Services.AddSingleton<AiContextBuilderService>();
builder.Services.AddSingleton<InMemoryConversationStore>();
builder.Services.AddSingleton<OpenAiAudioService>();
builder.Services.AddSingleton<IStreamingSpeechSynthesisService>(services =>
    services.GetRequiredService<OpenAiAudioService>());
if (builder.Configuration.GetValue<bool>("UseOpenAi"))
{
    builder.Services.AddSingleton<IAssistantService, OpenAiAssistantService>();
}
else
{
    builder.Services.AddSingleton<IAssistantService, FakeAiAssistantService>();
}
builder.Services.AddSingleton<MedicationWorkflowService>();
builder.Services.AddSingleton<MedicationQueryWorkflowService>();
builder.Services.AddSingleton<MemoryConversationWorkflowService>();
builder.Services.AddSingleton<RealtimeSessionContextService>();
builder.Services.AddSingleton<OpenAiRealtimeService>();
builder.Services.AddSingleton<IConversationService,ConversationService>();
builder.Services.AddSingleton<PromptFactory>();
var openAiOptions = new OpenAiOptions
{
    ApiKey = Environment.GetEnvironmentVariable("PUENTES_API_KEY") ?? string.Empty,
    Model = Environment.GetEnvironmentVariable("OPENAI_MODEL")
        ?? "gpt-5.4-mini"
};

builder.Services.AddSingleton(openAiOptions);
var openAiAudioOptions = new OpenAiAudioOptions
{
    ApiKey = Environment.GetEnvironmentVariable("PUENTES_API_KEY")
        ?? string.Empty,
    TranscriptionModel = Environment.GetEnvironmentVariable(
        "OPENAI_TRANSCRIPTION_MODEL") ?? "gpt-4o-mini-transcribe",
    SpeechModel = Environment.GetEnvironmentVariable(
        "OPENAI_SPEECH_MODEL") ?? "gpt-4o-mini-tts",
    StreamingSpeechModel = Environment.GetEnvironmentVariable(
        "OPENAI_STREAMING_SPEECH_MODEL") ?? "gpt-4o-mini-tts"
};
builder.Services.AddSingleton(openAiAudioOptions);
var openAiRealtimeOptions = new OpenAiRealtimeOptions
{
    ApiKey = Environment.GetEnvironmentVariable("PUENTES_API_KEY")
        ?? string.Empty,
    Model = Environment.GetEnvironmentVariable("OPENAI_REALTIME_MODEL")
        ?? "gpt-realtime-2.1-mini",
    Voice = Environment.GetEnvironmentVariable("OPENAI_REALTIME_VOICE")
        ?? "marin"
};
builder.Services.AddSingleton(openAiRealtimeOptions);
var whisperOptions = builder.Configuration
    .GetSection("Audio:Whisper")
    .Get<WhisperOptions>() ?? new WhisperOptions();
whisperOptions.ModelPath = Environment.GetEnvironmentVariable(
    "WHISPER_MODEL_PATH") ?? whisperOptions.ModelPath;
builder.Services.AddSingleton(whisperOptions);

if (builder.Configuration["Audio:TranscriptionProvider"]
    ?.Equals("Whisper", StringComparison.OrdinalIgnoreCase) == true)
{
    builder.Services.AddSingleton<IAudioService, WhisperAudioService>();
}
else
{
    builder.Services.AddSingleton<IAudioService>(services =>
        services.GetRequiredService<OpenAiAudioService>());
}
var piperOptions = builder.Configuration
    .GetSection("Audio:Piper")
    .Get<PiperOptions>() ?? new PiperOptions();
piperOptions.ExecutablePath = Environment.GetEnvironmentVariable(
    "PIPER_EXECUTABLE_PATH") ?? piperOptions.ExecutablePath;
piperOptions.ModelPath = Environment.GetEnvironmentVariable(
    "PIPER_MODEL_PATH") ?? piperOptions.ModelPath;
builder.Services.AddSingleton(piperOptions);

if (builder.Configuration["Audio:SpeechProvider"]
    ?.Equals("Piper", StringComparison.OrdinalIgnoreCase) == true)
{
    builder.Services.AddSingleton<ISpeechSynthesisService,
        PiperSpeechSynthesisService>();
}
else
{
    builder.Services.AddSingleton<ISpeechSynthesisService>(services =>
        services.GetRequiredService<OpenAiAudioService>());
}
var host = builder.Build();

host.MapGet("/health", async (
    PuentesDiagnosticsService diagnostics,
    CancellationToken cancellationToken) =>
{
    var result = await diagnostics.CheckAsync(cancellationToken);
    return result.Ready
        ? Results.Ok(result)
        : Results.Json(result, statusCode: StatusCodes.Status503ServiceUnavailable);
});

host.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features
            .Get<IExceptionHandlerFeature>()?.Error;

        if (exception is ClientResultException clientException)
        {
            context.Response.StatusCode = clientException.Status == 429
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status502BadGateway;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "El servicio de voz no está disponible temporalmente."
            });
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Ocurrió un error inesperado."
        });
    });
});

const long MaximumAudioLength = 20 * 1024 * 1024;

host.MapPost("/audio/transcriptions",
async (
    IFormFile audio,
    IAudioService audioService,
    CancellationToken cancellationToken) =>
{
    if (audio.Length == 0 || audio.Length > MaximumAudioLength)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(audio)] =
                ["El audio debe tener contenido y no superar 20 MB."]
        });
    }

    await using var stream = audio.OpenReadStream();
    var text = await audioService.TranscribeAsync(
        stream,
        audio.FileName,
        cancellationToken);

    return Results.Ok(new TranscriptionResponse { Text = text });
})
.DisableAntiforgery();

host.MapPost("/audio/speech",
async (
    SpeechRequest request,
    ISpeechSynthesisService speechService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Text)
        || request.Text.Length > 4096)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.Text)] =
                ["El texto es obligatorio y no puede superar 4096 caracteres."]
        });
    }

    var audio = await speechService.GenerateSpeechAsync(
        request.Text.Trim(),
        cancellationToken);

    return Results.File(audio.Content, audio.ContentType, audio.FileName);
});

host.MapPost("/audio/speech/stream",
async (
    SpeechRequest request,
    IStreamingSpeechSynthesisService speechService,
    HttpContext context,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Text)
        || request.Text.Length > 4096)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "El texto es obligatorio y no puede superar 4096 caracteres."
        }, cancellationToken);
        return;
    }

    context.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
    context.Response.StatusCode = StatusCodes.Status200OK;
    context.Response.ContentType = "audio/mpeg";
    context.Response.Headers.CacheControl = "no-store";
    context.Response.Headers.ContentDisposition =
        "inline; filename=puentes-response.mp3";

    await foreach (var audioChunk in speechService
        .GenerateSpeechStreamAsync(request.Text.Trim(), cancellationToken))
    {
        await context.Response.Body.WriteAsync(
            audioChunk,
            cancellationToken);
        await context.Response.Body.FlushAsync(cancellationToken);
    }
});

host.MapPost("/memory-support/voice/text",
async (
    [FromForm] Guid? personId,
    [FromForm] Guid? conversationId,
    IFormFile audio,
    IAudioService audioService,
    MemoryConversationWorkflowService workflow,
    CancellationToken cancellationToken) =>
{
    if (conversationId is null
        && (personId is null || personId == Guid.Empty))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(personId)] = ["La persona es obligatoria."]
        });
    }

    if (audio.Length == 0 || audio.Length > MaximumAudioLength)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(audio)] =
                ["El audio debe tener contenido y no superar 20 MB."]
        });
    }

    await using var stream = audio.OpenReadStream();
    var transcript = await audioService.TranscribeAsync(
        stream,
        audio.FileName,
        cancellationToken);
    var turn = conversationId is null
        ? await workflow.StartAsync(
            personId!.Value,
            transcript,
            cancellationToken)
        : await workflow.ContinueAsync(
            conversationId.Value,
            transcript,
            cancellationToken);
    var responseText = VoiceResponseFormatter.Prepare(turn.Response.Message);

    return Results.Ok(new VoiceConversationTextResponse
    {
        ConversationId = turn.ConversationId,
        Transcript = transcript,
        ResponseText = responseText
    });
})
.DisableAntiforgery();

host.MapPost("/memory-support/voice",
async (
    [FromForm] Guid? personId,
    [FromForm] Guid? conversationId,
    IFormFile audio,
    IAudioService audioService,
    ISpeechSynthesisService speechService,
    MemoryConversationWorkflowService workflow,
    CancellationToken cancellationToken) =>
{
    if (conversationId is null
        && (personId is null || personId == Guid.Empty))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(personId)] = ["La persona es obligatoria."]
        });
    }

    if (audio.Length == 0 || audio.Length > MaximumAudioLength)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(audio)] =
                ["El audio debe tener contenido y no superar 20 MB."]
        });
    }

    await using var stream = audio.OpenReadStream();
    var transcript = await audioService.TranscribeAsync(
        stream,
        audio.FileName,
        cancellationToken);

    var turn = conversationId is null
        ? await workflow.StartAsync(
            personId!.Value,
            transcript,
            cancellationToken)
        : await workflow.ContinueAsync(
            conversationId.Value,
            transcript,
            cancellationToken);
    var responseText = VoiceResponseFormatter.Prepare(turn.Response.Message);
    var speech = await speechService.GenerateSpeechAsync(
        responseText,
        cancellationToken);

    return Results.Ok(new VoiceConversationResponse
    {
        ConversationId = turn.ConversationId,
        Transcript = transcript,
        ResponseText = responseText,
        AudioContentType = speech.ContentType,
        AudioBase64 = Convert.ToBase64String(speech.Content)
    });
})
.DisableAntiforgery();

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
