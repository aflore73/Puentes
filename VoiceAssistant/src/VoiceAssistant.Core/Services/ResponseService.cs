using System.Text.Json;
using System.Net.Http.Json;

namespace VoiceAssistant.Core.Services;

public class ResponseService
{
    private readonly IntentionClassifier _classifier;
    private readonly MusicService _musicService;
    private readonly DatabaseService _database;
    private readonly EmotionService _emotionService;
    private readonly PersonDetector _personDetector;
    private readonly TextExtractorService _textExtractor;
    private readonly string? _apiKey;

    public ResponseService(
        IntentionClassifier classifier,
        MusicService musicService,
        DatabaseService database,
        EmotionService emotionService,
        PersonDetector personDetector,
        TextExtractorService textExtractor,
        string? apiKey)
    {
        _classifier = classifier;
        _musicService = musicService;
        _database = database;
        _emotionService = emotionService;
        _personDetector = personDetector;
        _textExtractor = textExtractor;
        _apiKey = apiKey;
    }

    public async Task<string> ProcessAsync(string input)
    {
        var inputLower = input.ToLower().Trim();
        
        // 1. EmociÃ³n
        var emocion = _emotionService.DetectEmotion(input);
        if (!string.IsNullOrEmpty(emocion))
        {
            return await ProcessEmotionAsync(emocion, input);
        }
        
        // 2. Biblia directa
        if (inputLower.Contains("biblia") || inputLower.Contains("leer"))
        {
            return await _emotionService.GetBibleTextAsync("tristeza");
        }
        
        // 3. Persona (detectar antes de ML.NET)
        var persona = _personDetector.DetectPerson(input);
        if (persona != null)
        {
            return await ProcessPersonAsync(input);
        }
        
        // 4. ML.NET
        var prediction = _classifier.Predict(input);
        var intention = prediction.PredictedLabel;
        
        return intention switch
        {
            "FECHA_HORA" => GetDateTimeResponse(),
            "OBJETO_PERDIDO" => await ProcessLostObjectAsync(input),
            "MUSICA" => await _musicService.PlayMusicAsync(songName: _textExtractor.ExtractSearchTerm(input)),
            "LISTAR_MUSICA" => await ListMusicAsync(),
            "DETENER" => _musicService.StopMusic(),
            "SIGUIENTE" => await _musicService.NextSongAsync(),
            "PERSONA" => await ProcessPersonAsync(input),
            _ => await _database.GetContextForCategoryAsync(intention)
        };
    }

    private async Task<string> ProcessEmotionAsync(string emocion, string input)
    {
        var result = new System.Text.StringBuilder();
        result.AppendLine("[EMOCION: " + emocion + "]");
        
        var opciones = await _emotionService.GetEmotionOptionsAsync(emocion);
        result.AppendLine("\nOpciones:");
        result.AppendLine(opciones);
        result.AppendLine("\nÂ¿Que preferis? (biblia/recuerdo/musica/familia/actividad)");
        
        return result.ToString();
    }

    public async Task<string> ProcessEmotionChoiceAsync(string emocion, string eleccion)
    {
        return eleccion switch
        {
            "biblia" => await _emotionService.GetBibleTextAsync(emocion),
            "recuerdo" => await _database.GetContextForCategoryAsync("RECUERDO"),
            "musica" => await _musicService.PlayMusicAsync(),
            "familia" => string.Join(", ", await _database.GetAllPersonNamesAsync()),
            "actividad" => await _database.GetContextForCategoryAsync("RUTINA"),
            _ => "No entendi la eleccion."
        };
    }

    private string GetDateTimeResponse()
    {
        var hoy = DateTime.Now;
        return "Hoy es " + hoy.ToString("dddd") + " " + 
               hoy.ToString("dd 'de' MMMM 'de' yyyy") + 
               ", son las " + hoy.ToString("HH:mm");
    }

    private async Task<string> ProcessLostObjectAsync(string input)
    {
        var belonging = await _database.GetBelongingContextAsync(input);
        
        if (string.IsNullOrEmpty(belonging))
        {
            return "No encontre informacion sobre ese objeto.";
        }
        
        return belonging;
    }

    private async Task<string> ListMusicAsync()
    {
        var songs = await _musicService.GetAvailableSongsAsync();
        
        if (songs.Count == 0)
        {
            return "No hay canciones.";
        }
        
        var result = new System.Text.StringBuilder();
        result.AppendLine("Canciones (" + songs.Count + "):");
        
        foreach (var s in songs.Take(10))
        {
            result.AppendLine("  - " + Path.GetFileNameWithoutExtension(s));
        }
        
        return result.ToString();
    }

    private async Task<string> ProcessPersonAsync(string input)
    {
        var person = _personDetector.DetectPerson(input);
        
        if (person == null)
        {
            return "No identifique a la persona.";
        }
        
        var rutinas = await _database.GetPersonRoutineContextAsync(person);
        
        // Si no hay rutinas, responder directamente SIN OpenAI
        if (rutinas.Contains("No se encontraron rutinas"))
        {
            return "No tengo informacion sobre las rutinas de " + person + ".";
        }
        
        if (!string.IsNullOrEmpty(_apiKey))
        {
            return await HumanizarRespuestaAsync(rutinas, person, input);
        }
        
        return rutinas;
    }

    private async Task<string> HumanizarRespuestaAsync(string datosBD, string persona, string inputUsuario)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiKey);
        
        var hoy = DateTime.Now;
        var diaHoy = TraducirDia(hoy.ToString("dddd"));
        var fechaHoy = hoy.ToString("dd/MM/yyyy");
        var horaActual = hoy.ToString("HH:mm");
        
        var systemPrompt = "Eres un asistente para una persona mayor. " +
                           "Hablas en espanol, con tono amable pero sobrio. " +
                           "Sin diminutivos, sin abrazos, sin 'querido'. " +
                           "SOLO informas donde esta la persona y que esta haciendo ahora.";
        
        var userPrompt = "HOY ES: " + diaHoy + " " + fechaHoy + " a las " + horaActual + "\n\n";
        userPrompt += "DATOS:\n" + datosBD + "\n\n";
        userPrompt += "PERSONA: " + persona + "\n";
        userPrompt += "LO QUE DIJO: \"" + inputUsuario + "\"\n\n";
        userPrompt += "Usa el nombre de la persona. Decime donde esta ahora y que esta haciendo. Una frase corta.";
        
        var request = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0,
            max_tokens = 100
        };
        
        var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OpenAIResponse>(json);
        
        return result?.choices?[0]?.message?.content ?? "Sin respuesta";
    }

    private string TraducirDia(string dia)
    {
        return dia switch
        {
            "Monday" => "Lunes",
            "Tuesday" => "Martes",
            "Wednesday" => "MiÃ©rcoles",
            "Thursday" => "Jueves",
            "Friday" => "Viernes",
            "Saturday" => "SÃ¡bado",
            "Sunday" => "Domingo",
            _ => dia
        };
    }
}

public class OpenAIResponse
{
    public Choice[]? choices { get; set; }
}

public class Choice
{
    public Message? message { get; set; }
}

public class Message
{
    public string content { get; set; } = "";
}