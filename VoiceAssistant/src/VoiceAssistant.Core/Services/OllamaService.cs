using System.Net.Http.Json;
using System.Text.Json;
using VoiceAssistant.Core.Interfaces;

namespace VoiceAssistant.Core.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _model;

    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _baseUrl = "http://localhost:11434";
        _model = "qwen2.5:7b"; // Usar qwen2.5:7b (mÃ¡s potente) o qwen2.5:1.5b (mÃ¡s rÃ¡pido)
    }

    public async Task<OllamaResult> GenerateResponseAsync(string text, string category, string context)
    {
        try
        {
            var prompt = BuildPrompt(text, category, context);
            
            var request = new
            {
                model = _model,
                prompt = prompt,
                stream = false,
                format = "json",
                options = new
                {
                    temperature = 0.7,
                    top_p = 0.9,
                    context_length = 2048,
                    num_predict = 256
                }
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/generate", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaApiResponse>();
            
            if (result == null || string.IsNullOrEmpty(result.Response))
            {
                return new OllamaResult 
                { 
                    Text = "No pude generar una respuesta.", 
                    Category = category 
                };
            }

            return ParseResponse(result.Response, category);
        }
        catch (HttpRequestException)
        {
            return new OllamaResult 
            { 
                Text = "El servicio de IA no estÃ¡ disponible. Verifica que Ollama estÃ© ejecutÃ¡ndose.", 
                Category = category 
            };
        }
        catch (Exception ex)
        {
            return new OllamaResult 
            { 
                Text = $"Error: {ex.Message}", 
                Category = category 
            };
        }
    }

    private string BuildPrompt(string text, string category, string context)
    {
        return $@"
[CONTEXTO]
CategorÃ­a detectada: {category}

[DATOS RELEVANTES]
{context}

[ENTRADA DEL USUARIO]
{text}

[INSTRUCCIONES]
Eres un asistente personal. Responde de manera natural en espaÃ±ol.
La categorÃ­a detectada es {category}.
Proporciona una respuesta Ãºtil y relevante.

Responde en formato JSON:
{{
    ""text"": ""tu respuesta aquÃ­"",
    ""category"": ""{category}""
}}
";
    }

    private OllamaResult ParseResponse(string json, string fallbackCategory)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var parsed = JsonSerializer.Deserialize<OllamaResult>(json, options);
            
            if (parsed != null && !string.IsNullOrEmpty(parsed.Text))
            {
                return parsed;
            }
        }
        catch (JsonException)
        {
            // Si no es JSON, devolver texto tal cual
        }

        return new OllamaResult 
        { 
            Text = json, 
            Category = fallbackCategory 
        };
    }

    private class OllamaApiResponse
    {
        public string Response { get; set; } = string.Empty;
        public bool Done { get; set; }
    }
}