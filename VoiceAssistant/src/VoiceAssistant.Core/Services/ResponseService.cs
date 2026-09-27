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
    private readonly TemasBloqueadosService _temasBloqueados;
    private readonly TemasPermitidosService _temasPermitidos;
    private readonly string? _apiKey;
    private readonly WeatherService? _weatherService;
    private readonly ConversationService? _conversationService;
    private readonly LugarService? _lugarService;
    private readonly EventoService? _eventoService;

    public ResponseService(
        IntentionClassifier classifier,
        MusicService musicService,
        DatabaseService database,
        EmotionService emotionService,
        PersonDetector personDetector,
        TextExtractorService textExtractor,
        TemasBloqueadosService temasBloqueados,
        TemasPermitidosService temasPermitidos,
        string? apiKey,
        WeatherService? weatherService = null,
        ConversationService? conversationService = null,
        LugarService? lugarService = null,
        EventoService? eventoService = null)
    {
        _classifier = classifier;
        _musicService = musicService;
        _database = database;
        _emotionService = emotionService;
        _personDetector = personDetector;
        _textExtractor = textExtractor;
        _temasBloqueados = temasBloqueados;
        _temasPermitidos = temasPermitidos;
        _apiKey = apiKey;
        _weatherService = weatherService;
        _conversationService = conversationService;
        _lugarService = lugarService;
        _eventoService = eventoService;
    }

    public async Task<string> ProcessAsync(string input)
    {
        var inputLower = input.ToLower().Trim();

        // 1. Emocion
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

        // 3. Persona
        var persona = _personDetector.DetectPerson(input);
        if (persona != null)
        {
            return await ProcessPersonAsync(input);
        }

        // 4. Temas BLOQUEADOS
        var bloqueado = await _temasBloqueados.CheckBlockedAsync(input);
        if (!string.IsNullOrEmpty(bloqueado))
        {
            return bloqueado;
        }

        // 4.5. Continuacion conversacional
        var conectores = new[] { "y ", "pero ", "entonces ", "ademas ", "tambien " };
        if (conectores.Any(c => inputLower.StartsWith(c)) && inputLower.Length < 40)
        {
            return await ConversarConOpenAIAsync(input,
                "Sos un asistente para una persona mayor. Responde en 2 frases cortas. " +
                "Si el usuario dice 'y X' o 'pero X', se refiere a una continuacion de la conversacion anterior.");
        }

        // 5. ML.NET
        var prediction = _classifier.Predict(input);
        var intention = prediction.PredictedLabel;

        Console.WriteLine("  [DEBUG] Intencion: " + intention + " (" + prediction.Score.Max().ToString("P0") + ")");

        // 6. CONVERSACION_CLIMA
        if (intention == "CONVERSACION_CLIMA")
        {
            return await GetWeatherResponseAsync();
        }

        // 7. CONVERSACION_XXX
        if (intention.StartsWith("CONVERSACION_"))
        {
            var codigo = intention.Replace("CONVERSACION_", "").ToLower();
            var tema = await _temasPermitidos.GetTopicByCodeAsync(codigo);

            if (tema != null)
            {
                return await ConversarConOpenAIAsync(input, tema.PromptSistema);
            }

            return await ConversarConOpenAIAsync(input,
                "Sos un asistente para una persona mayor. Respondes breve y con calidez.");
        }

        // 8. Otras intenciones
        var result = intention switch
        {
            "FECHA_HORA" => GetDateTimeResponse(),
            "CONSULTA_LUGAR" => await ProcessLugarAsync(input),
            "OBJETO_PERDIDO" => await ProcessLostObjectAsync(input),
            "MUSICA" => await _musicService.PlayMusicAsync(songName: _textExtractor.ExtractSearchTerm(input)),
            "LISTAR_MUSICA" => await ListMusicAsync(),
            "DETENER" => _musicService.StopMusic(),
            "SIGUIENTE" => await _musicService.NextSongAsync(),
            _ => null
        };

        if (result != null) return result;

        // 9. No coincide nada
        return "No entendi. Podrias repetirlo de otra forma?";
    }

    private async Task<string> ProcessEmotionAsync(string emocion, string input)
    {
        var result = new System.Text.StringBuilder();
        result.AppendLine("[EMOCION: " + emocion + "]");

        var opciones = await _emotionService.GetEmotionOptionsAsync(emocion);
        result.AppendLine("\nOpciones:");
        result.AppendLine(opciones);
        result.AppendLine("\nQue preferis? (biblia/recuerdo/musica/familia/actividad)");

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

    private async Task<string> GetWeatherResponseAsync()
    {
        if (_weatherService == null)
        {
            return "No tengo acceso al clima en este momento.";
        }

        var clima = await _weatherService.GetCurrentWeatherAsync();

        if (string.IsNullOrEmpty(clima))
        {
            return "No pude obtener el clima en este momento.";
        }

        return clima;
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

    private async Task<string> ProcessLugarAsync(string input)
    {
        // 0. Buscar turnos FUTUROS
        var turnoFuturo = await _database.FindTurnoFuturoAsync(input);

        if (turnoFuturo != null)
        {
            return "Tu proximo turno es: " + turnoFuturo + ".";
        }

        // 1. Buscar evento pasado
        if (_eventoService != null)
        {
            var evento = await _eventoService.FindEventoAsync(input);

            if (evento != null)
            {
                var contextoEvento = new System.Text.StringBuilder();
                contextoEvento.AppendLine("Evento: " + evento.Title);

                if (!string.IsNullOrEmpty(evento.StartDate))
                    contextoEvento.AppendLine("Fecha: " + evento.StartDate);

                if (!string.IsNullOrEmpty(evento.Description))
                    contextoEvento.AppendLine("Descripcion: " + evento.Description);

                if (!string.IsNullOrEmpty(evento.LugarNombre))
                    contextoEvento.AppendLine("Lugar: " + evento.LugarNombre);

                if (!string.IsNullOrEmpty(evento.LugarDireccion))
                    contextoEvento.AppendLine("Direccion: " + evento.LugarDireccion);

                if (!string.IsNullOrEmpty(evento.LugarLocalidad))
                    contextoEvento.AppendLine("Localidad: " + evento.LugarLocalidad);
                
                if (!string.IsNullOrEmpty(evento.Participantes))
                    contextoEvento.AppendLine("Participantes: " + evento.Participantes);

                var systemPromptEvento = "Sos un asistente para una persona mayor. " +
                         "Responde en maximo 2 frases cortas. " +
                         "Usa SOLO los datos que te doy. " +
                         "SIEMPRE respondé con el lugar REAL de los DATOS. " +
                         "NO menciones lugares que el usuario dijo si no aparecen en los datos. " +
                         "Si los datos mencionan personas, nombralas. " +
                         "Si no hay datos, deci que no tenes informacion.";

                var userPromptEvento = "DATOS:\n" + contextoEvento.ToString() + "\n\nPREGUNTA: " + input;

                return await ConversarConOpenAIAsync(userPromptEvento, systemPromptEvento, incluirHistorial: false);
            }
        }

        // 2. Fallback: buscar lugar
        if (_lugarService == null)
        {
            return "No tengo acceso a los lugares en este momento.";
        }

        var lugar = await _lugarService.FindLugarAsync(input);

        if (lugar == null)
        {
            return "No encontre informacion sobre eso.";
        }

        var contexto = new System.Text.StringBuilder();
        contexto.AppendLine("Lugar: " + lugar.Nombre);

        if (!string.IsNullOrEmpty(lugar.Direccion))
            contexto.AppendLine("Direccion: " + lugar.Direccion);

        if (!string.IsNullOrEmpty(lugar.Localidad))
            contexto.AppendLine("Localidad: " + lugar.Localidad);

        if (!string.IsNullOrEmpty(lugar.Notas))
            contexto.AppendLine("Notas: " + lugar.Notas);

        var turnos = await _database.GetTurnosByLugarAsync(lugar.Id);
        if (!string.IsNullOrEmpty(turnos))
            contexto.AppendLine("Turnos proximos:\n" + turnos);

        var eventos = await _database.GetEventosByLugarAsync(lugar.Id);
        if (!string.IsNullOrEmpty(eventos))
            contexto.AppendLine("Eventos pasados:\n" + eventos);

        var systemPrompt = "Sos un asistente para una persona mayor. " +
                           "Responde en maximo 2 frases cortas. " +
                           "Usa SOLO los datos que te doy. Si no estan, deci que no los tenes.";

        var userPrompt = "DATOS:\n" + contexto.ToString() + "\n\nPREGUNTA: " + input;

        return await ConversarConOpenAIAsync(userPrompt, systemPrompt, incluirHistorial: false);
    }

    private async Task<string> ConversarConOpenAIAsync(string input, string promptSistema, bool incluirHistorial = true)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiKey);

        // Normalizar saltos de linea (Windows a Unix) para OpenAI
        var promptFinal = (promptSistema + " IMPORTANTE: Responde en maximo 2 frases muy cortas. NO mas de 40 palabras. Se breve.")
            .Replace("\r\n", "\n");

        var inputLimpio = input.Replace("\r\n", "\n");

        var messages = new List<object>();
        messages.Add(new { role = "system", content = promptFinal });

        // Agregar historial si existe
        if (incluirHistorial && _conversationService != null)
        {
            var historial = await _conversationService.GetHistoryAsync(5);
            foreach (var msg in historial)
            {
                messages.Add(new { role = msg.Role, content = msg.Content });
            }
        }

        messages.Add(new { role = "user", content = inputLimpio });

        var request = new
        {
            model = "gpt-4o-mini",
            messages = messages,
            temperature = 0.3,
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
            "Wednesday" => "Miercoles",
            "Thursday" => "Jueves",
            "Friday" => "Viernes",
            "Saturday" => "Sabado",
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