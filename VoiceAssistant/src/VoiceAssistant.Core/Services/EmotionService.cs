using Microsoft.Data.Sqlite;
using System.Text;

namespace VoiceAssistant.Core.Services;

public class EmotionService
{
    private readonly string _connectionString;

    public EmotionService(string dbPath = "assistant.db")
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public async Task<string> GetEmotionOptionsAsync(string emotionCodigo)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT e.Nombre, eo.Tipo, eo.Titulo, eo.Prioridad
            FROM Emociones e
            JOIN EmocionOpciones eo ON e.Id = eo.EmocionId
            WHERE e.Codigo = @codigo AND e.Activo = 1
            ORDER BY eo.Prioridad";
        
        command.Parameters.AddWithValue("@codigo", emotionCodigo);
        
        var result = new StringBuilder();
        
        await using var reader = await command.ExecuteReaderAsync();
        var hasRows = false;
        
        while (await reader.ReadAsync())
        {
            hasRows = true;
            var tipo = reader.GetString(1);
            var titulo = reader.GetString(2);
            result.AppendLine("- " + titulo + " [" + tipo + "]");
        }
        
        if (!hasRows)
        {
            return "No hay opciones para esta emocion.";
        }
        
        return result.ToString();
    }

    public string DetectEmotion(string input)
    {
        var inputLower = input.ToLower();
        
        if (inputLower.Contains("triste") || inputLower.Contains("bajon") || 
            inputLower.Contains("deprim") || inputLower.Contains("angust") ||
            inputLower.Contains("no me siento bien") || inputLower.Contains("me siento mal") ||
            inputLower.Contains("estoy mal") || inputLower.Contains("no estoy bien"))
            return "tristeza";
        
        if (inputLower.Contains("aburr") || inputLower.Contains("no se que hacer") ||
            inputLower.Contains("no tengo nada que hacer"))
            return "aburrimiento";
        
        if (inputLower.Contains("sola") || inputLower.Contains("solo") || 
            inputLower.Contains("soledad") || inputLower.Contains("extrano") ||
            inputLower.Contains("no tengo a nadie"))
            return "soledad";
        
        if (inputLower.Contains("content") || inputLower.Contains("feliz") || 
            inputLower.Contains("alegre") || inputLower.Contains("alegria"))
            return "alegria";
        
        if (inputLower.Contains("ansios") || inputLower.Contains("ansiedad") || 
            inputLower.Contains("nervios") || inputLower.Contains("preocup"))
            return "ansiedad";
        
        if (inputLower.Contains("enoj") || inputLower.Contains("molest") || 
            inputLower.Contains("bronca") || inputLower.Contains("harto") ||
            inputLower.Contains("harta"))
            return "enojo";
        
        if (inputLower.Contains("miedo") || inputLower.Contains("asustad") || 
            inputLower.Contains("temor"))
            return "miedo";
        
        return "";
    }

    public async Task<string> GetBibleTextAsync(string emocionCodigo)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT psc.Title, psc.Content, psc.Reference
            FROM PersonSupportContents psc
            WHERE psc.IsActive = 1
              AND EXISTS (
                  SELECT 1 
                  FROM EmocionTags et 
                  JOIN Emociones e ON et.EmocionId = e.Id
                  WHERE e.Codigo = @codigo 
                    AND psc.Tags LIKE '%' || et.Tag || '%'
              )
            ORDER BY RANDOM()
            LIMIT 1";
        
        command.Parameters.AddWithValue("@codigo", emocionCodigo);
        
        await using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return reader.GetString(0) + "\n\n" + reader.GetString(1) + "\n\n" + reader.GetString(2);
        }
        
        return "No encontre un texto para esta emocion.";
    }
}