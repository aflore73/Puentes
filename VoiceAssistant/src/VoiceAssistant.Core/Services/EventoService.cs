using Microsoft.Data.Sqlite;

namespace VoiceAssistant.Core.Services;

public class EventoService
{
    private readonly string _connectionString;

    public EventoService(string dbPath = "assistant.db")
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public async Task<EventoEncontrado?> FindEventoAsync(string input, int puntajeMinimo = 2)
    {
        var palabras = Normalizar(input)
            .Replace("Â¿", " ")
            .Replace("?", " ")
            .Replace("Â¡", " ")
            .Replace("!", " ")
            .Replace(".", " ")
            .Replace(",", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.Length >= 4)
            .Distinct()
            .ToList();

        if (palabras.Count == 0) return null;

        var eventos = await GetAllEventosAsync();

        EventoEncontrado? mejor = null;
        int mejorPuntaje = 0;

        foreach (var evento in eventos)
        {
            int puntaje = 0;

            foreach (var palabra in palabras)
            {
                // Title (peso alto)
                if (!string.IsNullOrEmpty(evento.Title) && 
                    Normalizar(evento.Title).Contains(palabra))
                    puntaje += 5;

                // Description (peso medio)
                if (!string.IsNullOrEmpty(evento.Description) && 
                    Normalizar(evento.Description).Contains(palabra))
                    puntaje += 3;

                // Lugar.Nombre (peso bajo)
                if (!string.IsNullOrEmpty(evento.LugarNombre) && 
                    Normalizar(evento.LugarNombre).Contains(palabra))
                    puntaje += 2;

                // Lugar.Alias (peso bajo)
                if (!string.IsNullOrEmpty(evento.LugarAlias))
                {
                    var aliasList = evento.LugarAlias.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var a in aliasList)
                    {
                        var aliasLimpio = Normalizar(a.Trim());
                        if (aliasLimpio.Length >= 4 && aliasLimpio.Contains(palabra))
                        {
                            puntaje += 2;
                            break;
                        }
                    }
                }

                // Lugar.Localidad (peso mÃ­nimo)
                if (!string.IsNullOrEmpty(evento.LugarLocalidad) && 
                    Normalizar(evento.LugarLocalidad).Contains(palabra))
                    puntaje += 1;
            }

            if (puntaje > mejorPuntaje)
            {
                mejorPuntaje = puntaje;
                mejor = evento;
            }
        }

        Console.WriteLine("  [DEBUG Evento] Palabras: " + string.Join(", ", palabras));
        Console.WriteLine("  [DEBUG Evento] Mejor puntaje: " + mejorPuntaje);
        Console.WriteLine("  [DEBUG Evento] Mejor evento: " + (mejor?.Title ?? "ninguno"));
        
        return mejorPuntaje >= puntajeMinimo ? mejor : null;
    }

    private string Normalizar(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        
        return texto.ToLower()
            .Replace("\u00E1", "a")
            .Replace("\u00E9", "e")
            .Replace("\u00ED", "i")
            .Replace("\u00F3", "o")
            .Replace("\u00FA", "u")
            .Replace("\u00FC", "u")
            .Replace("\u00F1", "n");
    }
    private async Task<List<EventoEncontrado>> GetAllEventosAsync()
    {
        var eventos = new List<EventoEncontrado>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                e.Id, e.Title, e.Description, e.StartDate, e.EndDate, 
                e.IsPositiveMemory,
                l.Nombre AS LugarNombre,
                l.Alias AS LugarAlias,
                l.Localidad AS LugarLocalidad,
                l.Direccion AS LugarDireccion,
                (SELECT GROUP_CONCAT(p.Name || CASE WHEN lep.Role IS NOT NULL AND lep.Role != '' THEN ' (' || lep.Role || ')' ELSE '' END, ', ')
                 FROM LifeEventParticipants lep
                 JOIN People p ON lep.PersonId = p.Id
                 WHERE lep.LifeEventId = e.Id) AS Participantes
            FROM PersonLifeEvents e
            LEFT JOIN Lugares l ON e.LugarId = l.Id";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            eventos.Add(new EventoEncontrado
            {
                Id = reader.GetString(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                StartDate = reader.IsDBNull(3) ? null : reader.GetString(3),
                EndDate = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsPositiveMemory = reader.GetInt32(5) == 1,
                LugarNombre = reader.IsDBNull(6) ? null : reader.GetString(6),
                LugarAlias = reader.IsDBNull(7) ? null : reader.GetString(7),
                LugarLocalidad = reader.IsDBNull(8) ? null : reader.GetString(8),
                LugarDireccion = reader.IsDBNull(9) ? null : reader.GetString(9),
                Participantes = reader.IsDBNull(10) ? null : reader.GetString(10)
            });
        }

        return eventos;
    }
}

public class EventoEncontrado
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public bool IsPositiveMemory { get; set; }
    public string? LugarNombre { get; set; }
    public string? LugarAlias { get; set; }
    public string? LugarLocalidad { get; set; }
    public string? LugarDireccion { get; set; }
    public string? Participantes { get; set; }
}