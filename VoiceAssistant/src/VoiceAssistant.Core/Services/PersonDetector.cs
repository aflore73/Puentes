using Microsoft.Data.Sqlite;

namespace VoiceAssistant.Core.Services;

public class PersonDetector
{
    private readonly string _connectionString;

    public PersonDetector(string dbPath = "assistant.db")
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public string DetectPerson(string text)
    {
        var textLower = text.ToLower().Trim();
        
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Name 
            FROM People 
            WHERE @text LIKE '%' || Name || '%'
               OR (Alias IS NOT NULL AND @text LIKE '%' || Alias || '%')
            LIMIT 1";
        
        command.Parameters.AddWithValue("@text", textLower);
        
        var result = command.ExecuteScalar();
        
        return result?.ToString() ?? "";
    }
}