using Microsoft.Data.Sqlite;

namespace VoiceAssistant.Core.Services;

public class LugarService
{
    private readonly string _connectionString;

    public LugarService(string dbPath = "assistant.db")
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public async Task<Lugar?> FindLugarAsync(string input)
    {
        var palabras = input.ToLower()
            .Replace("Â¿", " ")
            .Replace("?", " ")
            .Replace("Â¡", " ")
            .Replace("!", " ")
            .Replace(".", " ")
            .Replace(",", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.Length >= 3)
            .ToList();

        if (palabras.Count == 0) return null;

        var lugares = await GetAllLugaresAsync();

        Lugar? mejor = null;
        int mejorPuntaje = 0;

        foreach (var lugar in lugares)
        {
            int puntaje = 0;

            // Nombre
            foreach (var palabra in palabras)
                if (lugar.Nombre.ToLower().Contains(palabra)) puntaje += 2;

            // Alias (split por coma)
            if (!string.IsNullOrEmpty(lugar.Alias))
            {
                var aliasList = lugar.Alias.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var palabra in palabras)
                {
                    foreach (var a in aliasList)
                    {
                        var aliasLimpio = a.Trim().ToLower();
                        if (aliasLimpio.Length >= 3 && aliasLimpio.Contains(palabra))
                        {
                            puntaje += 2;
                            break;
                        }
                    }
                }
            }

            // Localidad
            if (!string.IsNullOrEmpty(lugar.Localidad))
            {
                foreach (var palabra in palabras)
                    if (lugar.Localidad.ToLower().Contains(palabra)) puntaje += 1;
            }

            // Provincia
            if (!string.IsNullOrEmpty(lugar.Provincia))
            {
                foreach (var palabra in palabras)
                    if (lugar.Provincia.ToLower().Contains(palabra)) puntaje += 1;
            }

            // Direccion
            if (!string.IsNullOrEmpty(lugar.Direccion))
            {
                foreach (var palabra in palabras)
                    if (lugar.Direccion.ToLower().Contains(palabra)) puntaje += 1;
            }

            if (puntaje > mejorPuntaje)
            {
                mejorPuntaje = puntaje;
                mejor = lugar;
            }
        }

        return mejorPuntaje >= 3 ? mejor : null;
    }

    private async Task<List<Lugar>> GetAllLugaresAsync()
    {
        var lugares = new List<Lugar>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Nombre, TipoId, Direccion, Localidad, Provincia, Pais, Latitud, Longitud, Notas, Alias
            FROM Lugares
            WHERE Activo = 1";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lugares.Add(new Lugar
            {
                Id = reader.GetString(0),
                Nombre = reader.GetString(1),
                TipoId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                Direccion = reader.IsDBNull(3) ? null : reader.GetString(3),
                Localidad = reader.IsDBNull(4) ? null : reader.GetString(4),
                Provincia = reader.IsDBNull(5) ? null : reader.GetString(5),
                Pais = reader.IsDBNull(6) ? null : reader.GetString(6),
                Latitud = reader.IsDBNull(7) ? null : reader.GetDouble(7),
                Longitud = reader.IsDBNull(8) ? null : reader.GetDouble(8),
                Notas = reader.IsDBNull(9) ? null : reader.GetString(9),
                Alias = reader.IsDBNull(10) ? null : reader.GetString(10)
            });
        }

        return lugares;
    }
}

public class Lugar
{
    public string Id { get; set; } = "";
    public string Nombre { get; set; } = "";
    public int? TipoId { get; set; }
    public string? Direccion { get; set; }
    public string? Localidad { get; set; }
    public string? Provincia { get; set; }
    public string? Pais { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
    public string? Notas { get; set; }
    public string? Alias { get; set; }
}