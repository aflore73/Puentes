using System.Net.Http.Json;
using System.Text.Json;
using Puentes.LocalInterpreter.Models;

namespace Puentes.LocalInterpreter.Services;

public sealed class OllamaInputValidator : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaInputValidator(
        string baseUrl = "http://127.0.0.1:11434",
        string model = "qwen2.5:1.5b")
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(30)
        };
        _model = model;
    }

    public async Task<OllamaValidationResult> ValidateAsync(
        string transcript,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return new OllamaValidationResult
            {
                IsValid = false,
                Reason = "La transcripción está vacía."
            };
        }

        var request = new
        {
            model = _model,
            stream = false,
            format = "json",
            options = new
            {
                temperature = 0
            },
            prompt = $$"""
                Sos un corrector de transcripciones de voz en español argentino para una aplicación de asistencia
                familiar. Determiná si el texto transmite una intención comprensible y devolvé una versión corregida.
                Corregí errores fonéticos, palabras truncadas, omisiones, tildes, puntuación y errores típicos de
                reconocimiento de voz. Conservá exactamente los nombres propios, alias y el sentido original.
                No agregues información ni respondas la pregunta. Si podés inferir razonablemente la intención,
                valid debe ser true, aunque el texto original tenga errores importantes.
                Respondé únicamente JSON válido con esta forma:
                {"valid":true,"text":"texto corregido opcionalmente","reason":"motivo breve"}
                Si no es comprensible, usá valid=false.
                Transcripción: {{transcript}}
                """
        };

        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/generate",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        using JsonDocument envelope = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        if (!envelope.RootElement.TryGetProperty("response", out JsonElement responseText))
        {
            throw new InvalidOperationException(
                "Ollama no devolvió el campo 'response'.");
        }

        OllamaValidationResult? result = JsonSerializer.Deserialize<OllamaValidationResult>(
            responseText.GetString() ?? "",
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result == null)
        {
            throw new InvalidOperationException(
                "Ollama devolvió una validación vacía.");
        }

        // Algunos modelos pequeños escriben "valid": false aunque expliquen que la frase es comprensible.
        if (!result.IsValid &&
            result.Reason.Contains("válida", StringComparison.OrdinalIgnoreCase) &&
            (result.Reason.Contains("comprensible", StringComparison.OrdinalIgnoreCase) ||
             result.Reason.Contains("entiende", StringComparison.OrdinalIgnoreCase)))
        {
            return new OllamaValidationResult
            {
                IsValid = true,
                Text = result.Text,
                Reason = result.Reason
            };
        }

        return result;
    }

    public void Dispose() => _httpClient.Dispose();
}
