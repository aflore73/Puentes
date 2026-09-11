using VoiceAssistant.Core.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

var classifier = new IntentionClassifier();
var musicService = new MusicService();
var dbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Data", "Puentes.db");
var database = new DatabaseService(dbPath);
var emotionService = new EmotionService(dbPath);
var textExtractor = new TextExtractorService();
var temasBloqueados = new TemasBloqueadosService(dbPath);
var temasPermitidos = new TemasPermitidosService(dbPath);
var weatherService = new WeatherService(dbPath);
var conversationService = new ConversationService(dbPath);
var apiKey = Environment.GetEnvironmentVariable("PUENTES_API_KEY");

await database.InitializeAsync();
var knownPeople = await database.GetAllPersonNamesAsync();
var personDetector = new PersonDetector(dbPath);

var responseService = new ResponseService(
    classifier, musicService, database, emotionService,
    personDetector, textExtractor, temasBloqueados, temasPermitidos,
    apiKey, weatherService, conversationService);

Console.WriteLine("VoiceAssistant");
Console.WriteLine("=============");
Console.WriteLine("Personas: " + string.Join(", ", knownPeople));
Console.WriteLine("");

string? emocionActual = null;

while (true)
{
    Console.Write("Tu: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "salir")
        break;

    if (emocionActual != null &&
        (input.ToLower().Contains("biblia") || input.ToLower().Contains("recuerdo") ||
         input.ToLower().Contains("musica") || input.ToLower().Contains("familia") ||
         input.ToLower().Contains("actividad")))
    {
        var eleccion = input.ToLower().Trim();
        var respuesta = await responseService.ProcessEmotionChoiceAsync(emocionActual, eleccion);
        Console.WriteLine("\n" + respuesta);
        emocionActual = null;
        Console.WriteLine("\n==========================================");
        continue;
    }

    var resultado = await responseService.ProcessAsync(input);
    Console.WriteLine("\n" + resultado);

    // Guardar en historial
    await conversationService.SaveAsync(input, resultado);

    if (resultado.Contains("[EMOCION:"))
    {
        var emocion = emotionService.DetectEmotion(input);
        if (!string.IsNullOrEmpty(emocion))
        {
            emocionActual = emocion;
        }
    }

    Console.WriteLine("\n==========================================");
}

Console.WriteLine("Hasta luego!");