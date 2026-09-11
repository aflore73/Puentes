using Microsoft.Data.Sqlite;

namespace VoiceAssistant.Core.Services;

public class PersonDetector
{
    private readonly string _connectionString;

    public PersonDetector(string dbPath = "assistant.db")
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public string? DetectPerson(string text)
    {
        var textLower = text.ToLower().Trim();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Name, Alias FROM People";

        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var name = reader.GetString(0);
            var nameLower = name.ToLower();
            
            // Buscar por nombre completo
            if (textLower.Contains(nameLower))
                return name;
            
            // Buscar por cada alias individual
            if (!reader.IsDBNull(1))
            {
                var aliases = reader.GetString(1).Split(',', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var alias in aliases)
                {
                    var aliasLimpio = alias.Trim().ToLower();
                    
                    if (aliasLimpio.Length >= 3 && textLower.Contains(aliasLimpio))
                        return name;
                }
            }
        }
        
        return null;
    }
}