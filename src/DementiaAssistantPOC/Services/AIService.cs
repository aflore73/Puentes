using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace DementiaAssistantPOC.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        
        public AIService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
            _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        }
        
        public async Task<string> GetResponseAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_apiKey) || _apiKey == "TU_API_KEY_AQUI")
            {
                return "⚠️ API Key no configurada.";
            }
            
            var request = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "Eres un asistente terapéutico." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 300
            };
            
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            
            try
            {
                var response = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/chat/completions", 
                    content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);
                    return result?.Choices?[0]?.Message?.Content ?? "No response";
                }
                
                return $"Error: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                return $"Error de conexión: {ex.Message}";
            }
        }
        
        public string GetSimulatedResponse(string context, string userInput)
        {
            // Detectar si menciona a Juan (esposo fallecido)
            if (userInput.Contains("Juan", StringComparison.OrdinalIgnoreCase))
            {
                if (userInput.Contains("extraño") || userInput.Contains("triste") || userInput.Contains("extrañ"))
                {
                    return "Entiendo que extrañes mucho a Juan. Tuvieron momentos muy hermosos juntos. ¿Te gustaría recordar algún momento especial con él?";
                }
                if (userInput.Contains("dónde") || userInput.Contains("donde esta"))
                {
                    return "Juan fue una persona muy importante en tu vida. Aunque ya no está físicamente, siempre vivirá en tus recuerdos y en tu corazón.";
                }
            }
            
            // Detectar si menciona a Carlos (hijo)
            if (userInput.Contains("Carlos", StringComparison.OrdinalIgnoreCase))
            {
                if (userInput.Contains("dónde") || userInput.Contains("donde esta"))
                {
                    return "Carlos viene a visitarte hoy. Debe estar en camino. ¿Quieres que preparemos algo para cuando llegue?";
                }
                return "Carlos te quiere mucho y viene a verte seguido. ¿Te gustaría hablar de él?";
            }
            
            // Respuestas emocionales
            if (userInput.Contains("triste") || userInput.Contains("mal") || userInput.Contains("deprim"))
                return "Lamento escuchar que te sientes así. Es normal sentirse triste a veces. ¿Quieres hablar de lo que te preocupa?";
            
            if (userInput.Contains("feliz") || userInput.Contains("contento") || userInput.Contains("alegre"))
                return "¡Qué bueno que te sientas así! Me alegra mucho verte contento/a. ¿Qué te hace sentir feliz hoy?";
            
            if (userInput.Contains("miedo") || userInput.Contains("asustado") || userInput.Contains("tengo miedo"))
                return "Es normal sentir miedo a veces. Estás en un lugar seguro y yo estoy aquí contigo. Respiremos juntos profundamente.";
            
            if (userInput.Contains("solo") || userInput.Contains("sola") || userInput.Contains("abandon"))
                return "No estás solo/a. Tienes personas que te quieren mucho, como Carlos que viene a visitarte. Yo también estoy aquí para acompañarte.";
            
            // Resto de respuestas...
            if (userInput.Contains("familia") || userInput.Contains("hijo") || userInput.Contains("hija"))
                return "Tu familia es muy importante. ¿Te gustaría recordar algún momento especial con ellos?";
            
            if (userInput.Contains("medic") || userInput.Contains("pastilla"))
                return "Es importante tomar la medicación como indica el doctor. ¿Necesitas ayuda para recordar cuándo tomarla?";
            
            if (userInput.Contains("dónde") || userInput.Contains("perdido") || userInput.Contains("lugar"))
                return "No te preocupes, estás en un lugar seguro. Estoy aquí para ayudarte. ¿Puedes decirme qué estás buscando?";
            
            if (userInput.Contains("hambre") || userInput.Contains("comer"))
                return "Vamos a ver qué puedes comer. ¿Te gustaría que preparemos algo juntos?";
            
            if (userInput.Contains("hoy") || userInput.Contains("hacer"))
                return "Hoy tienes algunas actividades planificadas. Te acompaño a revisar tu agenda juntos.";
            
            // Respuesta por defecto
            return "Te escucho. Cuéntame más sobre cómo te sientes en este momento.";
        }
    }
    
    public class OpenAIResponse
    {
        public Choice[]? Choices { get; set; }
    }
    
    public class Choice
    {
        public Message? Message { get; set; }
    }
    
    public class Message
    {
        public string? Content { get; set; }
    }
}