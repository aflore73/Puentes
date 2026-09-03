using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using VoiceAssistant.Core.Interfaces;
using VoiceAssistant.Core.Services;

var classifier = new TextClassifier();

var dbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Data", "Puentes.db");
var database = new DatabaseService(dbPath);

var apiKey = Environment.GetEnvironmentVariable("PUENTES_API_KEY");

if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("ERROR: No se encontro PUENTES_API_KEY");
    return;
}

Console.WriteLine("VoiceAssistant con OpenAI");
Console.WriteLine("=========================");
Console.WriteLine("Escribe 'salir' para terminar");
Console.WriteLine("");

await database.InitializeAsync();

var knownPeople = await database.GetAllPersonNamesAsync();
var personDetector = new PersonDetector(knownPeople);
Console.WriteLine("Personas conocidas: " + string.Join(", ", knownPeople));
Console.WriteLine("");

while (true)
{
    Console.Write("Tu: ");
    var input = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "salir")
        break;
    
    Console.WriteLine("\n[1] Analizando...");
    
    var detectedPerson = personDetector.DetectPerson(input);
    string category = "";
    string dbContext = "";
    
    if (detectedPerson != null)
    {
        Console.WriteLine("Persona: " + detectedPerson);
        dbContext = await database.GetPersonRoutineContextAsync(detectedPerson);
    }
    else
    {
        var classification = await classifier.ClassifyAsync(input);
        category = classification.Category;
        Console.WriteLine("Categoria: " + category);
        dbContext = await database.GetContextForCategoryAsync(category);
    }
    
    Console.WriteLine("\n[2] Contexto BD:");
    Console.WriteLine(dbContext);
    
    Console.WriteLine("\n[3] Consultando OpenAI...");
    
    var openAIPrompt = @"
CONTEXTO DE BASE DE DATOS:
" + dbContext + @"

LO QUE DICE LA PERSONA:
" + input + @"

INSTRUCCIONES ESTRICTAS:
1. SOLO informa los datos que estan en el contexto.
2. NO opines, NO comentes, NO agregues nada.
3. NO digas 'esta ocupado', 'que bueno', ni nada similar.
4. Responde en una o dos frases.
5. Si no hay datos, di: 'No tengo esa informacion.'
6. Habla con calidez pero sin opiniones.
";
    
    try
    {
        using var openAIClient = new HttpClient();
        openAIClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
        
        var openAIRequest = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = "Eres un asistente que SOLO informa datos. No opinas, no comentas, solo informas." },
                new { role = "user", content = openAIPrompt }
            },
            temperature = 0.1,
            max_tokens = 100
        };
        
        var openAIResponse = await openAIClient.PostAsJsonAsync(
            "https://api.openai.com/v1/chat/completions", openAIRequest);
        
        openAIResponse.EnsureSuccessStatusCode();
        
        var openAIJson = await openAIResponse.Content.ReadAsStringAsync();
        var openAIResult = JsonSerializer.Deserialize<OpenAIResponse>(openAIJson);
        
        var answer = openAIResult?.choices?[0]?.message?.content ?? "Sin respuesta";
        
        Console.WriteLine("\n[4] Respuesta:");
        Console.WriteLine(answer);
    }
    catch (Exception ex)
    {
        Console.WriteLine("\nError: " + ex.Message);
    }
    
    Console.WriteLine("\n==========================================");
}

Console.WriteLine("Hasta luego!");

public class OpenAIResponse
{
    public Choice[] choices { get; set; }
}

public class Choice
{
    public Message message { get; set; }
}

public class Message
{
    public string content { get; set; }
}