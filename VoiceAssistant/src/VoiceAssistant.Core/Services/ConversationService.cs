using Microsoft.Data.Sqlite;

namespace VoiceAssistant.Core.Services;

public class ConversationService
{
    private readonly string _connectionString;

    public ConversationService(string dbPath = "assistant.db")
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public async Task SaveAsync(string userInput, string assistantResponse, string category = "", double confidence = 0)
    {
        // Si es la primera conversaciÃ³n del dÃ­a, borrar las anteriores
        await CleanIfNewDayAsync();
        
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Conversations (UserInput, AssistantResponse, Category, Confidence)
            VALUES (@input, @response, @category, @confidence)";
        
        command.Parameters.AddWithValue("@input", userInput);
        command.Parameters.AddWithValue("@response", assistantResponse);
        command.Parameters.AddWithValue("@category", category);
        command.Parameters.AddWithValue("@confidence", confidence);
        
        await command.ExecuteNonQueryAsync();
    }

    private async Task CleanIfNewDayAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        // Â¿Hay conversaciones de HOY?
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = @"
            SELECT COUNT(*) FROM Conversations 
            WHERE DATE(Timestamp) = DATE('now', 'localtime')";
        
        var countHoy = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
        
        if (countHoy == 0)
        {
            // Es la primera del dÃ­a â†’ borrar las anteriores
            var deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = @"
                DELETE FROM Conversations 
                WHERE DATE(Timestamp) < DATE('now', 'localtime')";
            
            await deleteCommand.ExecuteNonQueryAsync();
        }
    }

    public async Task<List<ChatMessage>> GetHistoryAsync(int limit = 5)
    {
        var messages = new List<ChatMessage>();
        
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT UserInput, AssistantResponse
            FROM Conversations
            WHERE DATE(Timestamp) = DATE('now', 'localtime')
            ORDER BY Timestamp DESC
            LIMIT @limit";
        
        command.Parameters.AddWithValue("@limit", limit);
        
        var temp = new List<(string user, string assistant)>();
        
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            temp.Add((reader.GetString(0), reader.IsDBNull(1) ? "" : reader.GetString(1)));
        }
        
        // Invertir para que queden en orden cronolÃ³gico
        temp.Reverse();
        
        foreach (var (user, assistant) in temp)
        {
            messages.Add(new ChatMessage { Role = "user", Content = user });
            
            if (!string.IsNullOrEmpty(assistant))
            {
                messages.Add(new ChatMessage { Role = "assistant", Content = assistant });
            }
        }
        
        return messages;
    }
}

public class ChatMessage
{
    public string Role { get; set; } = "";
    public string Content { get; set; } = "";
}