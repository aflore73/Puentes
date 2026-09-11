using Microsoft.Data.Sqlite;

namespace VoiceAssistant.Core.Services;

public class TemasBloqueadosService
{
    private readonly string _connectionString;

    public TemasBloqueadosService(string dbPath = "assistant.db")
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public async Task<string?> CheckBlockedAsync(string input)
    {
        var inputLower = input.ToLower();
        
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Nombre, PalabrasClave, Respuesta
            FROM TemasBloqueados
            WHERE Activo = 1";
        
        await using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var palabrasClave = reader.GetString(1);
            var respuesta = reader.GetString(2);
            
            var palabras = palabrasClave.Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var palabra in palabras)
            {
                var palabraLimpia = palabra.Trim().ToLower();
                
                if (!string.IsNullOrEmpty(palabraLimpia) && inputLower.Contains(palabraLimpia))
                {
                    return respuesta;
                }
            }
        }
        
        return null;
    }
}