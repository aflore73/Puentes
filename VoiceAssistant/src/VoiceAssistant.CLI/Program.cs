using System.Text.Json;
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
            if (string.IsNullOrEmpty(searchTerm))
            {
                Console.WriteLine(await musicService.PlayMusicAsync());
            }
            else
            {
                Console.WriteLine("Buscando: " + searchTerm);
                Console.WriteLine(await musicService.PlayMusicAsync(songName: searchTerm));
            }
            break;
            
        case "LISTAR_MUSICA":
            // MOSTRAR TODAS LAS CANCIONES
            var allSongs = await musicService.GetAvailableSongsAsync();
            if (allSongs.Count == 0)
            {
                Console.WriteLine("No hay canciones en las carpetas de musica.");
                Console.WriteLine("Carpetas: " + string.Join(", ", musicService.GetMusicFolders()));
            }
            else
            {
                Console.WriteLine("Canciones disponibles (" + allSongs.Count + "):");
                foreach (var s in allSongs.Take(20))
                {
                    Console.WriteLine("  - " + Path.GetFileNameWithoutExtension(s));
                }
            }
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
                var ctx = await database.GetPersonRoutineContextAsync(person);
                Console.WriteLine(ctx);
            }
            break;
            
        default:
            var context = await database.GetContextForCategoryAsync(intention);
            Console.WriteLine(context);
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
    
    var searchWords = words.Where(w => !filler.Contains(w)).ToList();
    
    return string.Join(" ", searchWords).Trim();
}