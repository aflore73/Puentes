using System.Text.Json;
using System.Net.Http.Json;
using VoiceAssistant.Core.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

var intentionClassifier = new IntentionClassifier();
var musicService = new MusicService();
var dbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Data", "Puentes.db");
var database = new DatabaseService(dbPath);
var apiKey = Environment.GetEnvironmentVariable("PUENTES_API_KEY");

Console.WriteLine("VoiceAssistant");
Console.WriteLine("=============");

await database.InitializeAsync();
var knownPeople = await database.GetAllPersonNamesAsync();
var personDetector = new PersonDetector(knownPeople);
Console.WriteLine("Personas: " + string.Join(", ", knownPeople));
Console.WriteLine("");

while (true)
{
    Console.Write("Tu: ");
    var input = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "salir")
        break;
    
    var prediction = intentionClassifier.Predict(input);
    var intention = prediction.PredictedLabel;
    var confidence = prediction.Score.Max();
    
    Console.WriteLine("\nIntencion: " + intention + " (" + confidence.ToString("P0") + ")");
    
    switch (intention)
    {
        case "MUSICA":
            var searchTerm = ExtractSearchTerm(input);
            Console.WriteLine(await musicService.PlayMusicAsync(songName: searchTerm));
            break;
            
        case "LISTAR_MUSICA":
            var songs = await musicService.GetAvailableSongsAsync();
            if (songs.Count == 0)
                Console.WriteLine("No hay canciones.");
            else
                foreach (var s in songs.Take(10))
                    Console.WriteLine("  - " + Path.GetFileNameWithoutExtension(s));
            break;
            
        case "DETENER":
            Console.WriteLine(musicService.StopMusic());
            break;
            
        case "SIGUIENTE":
            Console.WriteLine(await musicService.NextSongAsync());
            break;
            
        case "PERSONA":
            var person = personDetector.DetectPerson(input);
            if (person != null)
            {
                Console.WriteLine("Persona: " + person);
                var rutinas = await database.GetPersonRoutineContextAsync(person);
                Console.WriteLine("Datos BD:");
                Console.WriteLine(rutinas);
                
                if (!string.IsNullOrEmpty(apiKey))
                {
                    var respuesta = await HumanizarRespuesta(apiKey, rutinas, person, input);
                    Console.WriteLine("\nRespuesta final:");
                    Console.WriteLine(respuesta);
                }
            }
            break;
            
        default:
            var context = await database.GetContextForCategoryAsync(intention);
            Console.WriteLine(context);
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                var respuesta = await HumanizarRespuesta(apiKey, context, null, input);
                Console.WriteLine("\nRespuesta final:");
                Console.WriteLine(respuesta);
            }
            break;
    }
    
    Console.WriteLine("\n==========================================");
}

Console.WriteLine("Hasta luego!");

static string ExtractSearchTerm(string input)
{
    var filler = new[] { 
        "quiero", "quisiera", "me", "gustaria", "puedes", "podes", "a", "algo", "de", 
        "un", "una", "la", "el", "musica", "music", "cancion", "canciones", "para", 
        "escuchar", "escucar", "oir", "poner", "pon", "reproducir", "tocar", "play", 
        "tenes", "hay", "tienes", "que", "ver", "lista", "mostra" 
    };
    
    var words = input.ToLower()
        .Replace("Â¿", " ")
        .Replace("?", " ")
        .Replace("Â¡", " ")
        .Replace("!", " ")
        .Split(' ', StringSplitOptions.RemoveEmptyEntries);
    
    return string.Join(" ", words.Where(w => !filler.Contains(w))).Trim();
}

static async Task<string> HumanizarRespuesta(string apiKey, string datosBD, string? persona, string inputUsuario)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
    
    var hoy = DateTime.Now;
    var diaHoy = hoy.ToString("dddd");
    var fechaHoy = hoy.ToString("dd/MM/yyyy");
    var horaActual = hoy.ToString("HH:mm");
    
    var systemPrompt = "Eres un asistente. SOLO informas datos de la base de datos. " +
                       "NO agregas comentarios, deseos, opiniones ni suposiciones. " +
                       "NO usas frases como 'espero que', 'seguro que', 'quizas', 'probablemente', 'ojala'. " +
                       "Responde con UNA sola frase. Solo datos.";
    
    var userPrompt = "HOY: " + diaHoy + " " + fechaHoy + " " + horaActual + "\n\n";
    userPrompt += "DATOS DE LA BD:\n" + datosBD + "\n\n";
    
    if (!string.IsNullOrEmpty(persona))
    {
        userPrompt += "PERSONA: " + persona + "\n";
    }
    
    userPrompt += "LO QUE DIJO: \"" + inputUsuario + "\"\n\n";
    userPrompt += "RESPONDE SOLO CON LOS DATOS. UNA FRASE. NADA MAS.";
    
    var request = new
    {
        model = "gpt-4o-mini",
        messages = new[]
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userPrompt }
        },
        temperature = 0,
        max_tokens = 60
    };
    
    var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request);
    response.EnsureSuccessStatusCode();
    
    var json = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<OpenAIResponse>(json);
    
    return result?.choices?[0]?.message?.content ?? "Sin respuesta";
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