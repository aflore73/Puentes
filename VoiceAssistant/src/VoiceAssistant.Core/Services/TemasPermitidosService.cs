using Microsoft.Data.Sqlite;

namespace VoiceAssistant.Core.Services;

public class TemasPermitidosService
{
    private readonly string _connectionString;

    public TemasPermitidosService(string dbPath = "assistant.db")
    {
        _connectionString = $"Data Source={dbPath}";
    }

    /// <summary>
    /// Devuelve el tema permitido por su Codigo.
    /// El Codigo viene de ML.NET (ej: "cantantes", "historia").
    /// </summary>
    public async Task<TemaPermitido?> GetTopicByCodeAsync(string codigo)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Codigo, Nombre, PromptSistema
            FROM TemasPermitidos
            WHERE Codigo = @codigo AND Activo = 1
            LIMIT 1";
        
        command.Parameters.AddWithValue("@codigo", codigo);
        
        await using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return new TemaPermitido
            {
                Codigo = reader.GetString(0),
                Nombre = reader.GetString(1),
                PromptSistema = reader.GetString(2)
            };
        }
        
        return null;
    }

    public async Task<List<TemaPermitido>> GetAllAllowedTopicsAsync()
    {
        var temas = new List<TemaPermitido>();
        
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Codigo, Nombre, PromptSistema
            FROM TemasPermitidos
            WHERE Activo = 1
            ORDER BY Nombre";
        
        await using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            temas.Add(new TemaPermitido
            {
                Codigo = reader.GetString(0),
                Nombre = reader.GetString(1),
                PromptSistema = reader.GetString(2)
            });
        }
        
        return temas;
    }
}

public class TemaPermitido
{
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string PromptSistema { get; set; } = "";
}