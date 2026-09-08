namespace VoiceAssistant.Core.Services;

public class TextExtractorService
{
    private static readonly string[] FillerWords = new[] 
    { 
        "quiero", "quisiera", "me", "gustaria", "puedes", "podes", "a", "algo", "de", 
        "un", "una", "la", "el", "las", "los", "musica", "music", "cancion", "canciones", "para", 
        "escuchar", "escucar", "oir", "poner", "pon", "reproducir", "tocar", "play", 
        "tenes", "hay", "tienes", "que", "ver", "lista", "mostra", "no", "se",
        "donde", "deje", "perdi", "esta", "estan", "puse", "busco", "aparece",
        "mi", "mis", "tu", "tus", "su", "sus"
    };

    public string ExtractSearchTerm(string input)
    {
        var words = CleanText(input)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        return string.Join(" ", words.Where(w => !FillerWords.Contains(w))).Trim();
    }

    public string CleanText(string input)
    {
        return input.ToLower()
            .Replace("Â¿", " ")
            .Replace("?", " ")
            .Replace("Â¡", " ")
            .Replace("!", " ")
            .Replace(".", " ")
            .Replace(",", " ");
    }
}