using Microsoft.Data.Sqlite;
using System.Text;
using VoiceAssistant.Core.Interfaces;
using VoiceAssistant.Core.Models;

namespace VoiceAssistant.Core.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    private bool _initialized = false;

    public DatabaseService(string dbPath = "assistant.db")
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Conversations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserInput TEXT NOT NULL,
                AssistantResponse TEXT,
                Category TEXT,
                Confidence REAL,
                Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ";
        await command.ExecuteNonQueryAsync();
        
        _initialized = true;
    }

    public async Task<string> GetContextForCategoryAsync(string category)
    {
        await InitializeAsync();
        
        var context = new StringBuilder();
        
        switch (category)
        {
            case "EVENTO":
                context.AppendLine(await GetAgendaContextAsync());
                break;
            case "RECUERDO":
                context.AppendLine(await GetMemoriesContextAsync());
                break;
            case "RUTINA":
                context.AppendLine(await GetRoutinesContextAsync());
                break;
            case "TAREA":
                context.AppendLine(await GetTasksContextAsync());
                break;
            case "MEDICINA":
                context.AppendLine(await GetMedicationsContextAsync());
                break;
            default:
                context.AppendLine("Sin contexto especifico");
                break;
        }
        
        return context.ToString();
    }

    public async Task<string> GetPersonRoutineContextAsync(string personName)
    {
        await InitializeAsync();
        
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT pr.Title, pr.DaysOfWeek, pr.StartTime, pr.EndTime, pr.Notes
            FROM PersonRoutines pr
            JOIN People p ON pr.PersonId = p.Id
            WHERE p.Name LIKE @name AND pr.IsActive = 1
            ORDER BY pr.DaysOfWeek, pr.StartTime";
        
        command.Parameters.AddWithValue("@name", "%" + personName + "%");
        
        var result = new StringBuilder();
        result.AppendLine("Rutinas de " + personName + ":");
        
        await using var reader = await command.ExecuteReaderAsync();
        var hasRows = false;
        
        while (await reader.ReadAsync())
        {
            hasRows = true;
            var title = reader.GetString(0);
            var days = reader.IsDBNull(1) ? "Todos los dias" : TranslateDays(reader.GetString(1));
            var startTime = reader.IsDBNull(2) ? "Sin hora" : reader.GetString(2);
            var endTime = reader.IsDBNull(3) ? "Sin hora fin" : reader.GetString(3);
            var notes = reader.IsDBNull(4) ? "" : reader.GetString(4);
            
            result.AppendLine("- " + title);
            result.AppendLine("  Dias: " + days);
            result.AppendLine("  Horario: " + startTime + " a " + endTime);
            if (!string.IsNullOrEmpty(notes))
            {
                result.AppendLine("  Notas: " + notes);
            }
        }
        
        if (!hasRows)
        {
            result.AppendLine("No se encontraron rutinas para " + personName);
        }
        
        return result.ToString();
    }

    private string TranslateDays(string days)
    {
        if (string.IsNullOrEmpty(days)) return "Todos los dias";
        
        var translations = new Dictionary<string, string>
        {
            ["Monday"] = "Lunes",
            ["Tuesday"] = "Martes",
            ["Wednesday"] = "Miercoles",
            ["Thursday"] = "Jueves",
            ["Friday"] = "Viernes",
            ["Saturday"] = "Sabado",
            ["Sunday"] = "Domingo"
        };
        
        var dayList = days.Split(',');
        var translated = new List<string>();
        
        foreach (var day in dayList)
        {
            var trimmed = day.Trim();
            if (translations.ContainsKey(trimmed))
            {
                translated.Add(translations[trimmed]);
            }
            else
            {
                translated.Add(trimmed);
            }
        }
        
        return string.Join(", ", translated);
    }

    public async Task<List<string>> GetAllPersonNamesAsync()
    {
        await InitializeAsync();
        
        var names = new List<string>();
        
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Name FROM People ORDER BY Name";
        
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }
        
        return names;
    }

    private async Task<string> GetAgendaContextAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Title, ScheduledAt, Place 
            FROM PersonAgendaItems 
            ORDER BY ScheduledAt DESC
            LIMIT 10";
        
        var result = new StringBuilder();
        result.AppendLine("Eventos registrados:");
        
        await using var reader = await command.ExecuteReaderAsync();
        var hasRows = false;
        
        while (await reader.ReadAsync())
        {
            hasRows = true;
            result.AppendLine("- " + reader.GetString(0) + " | Fecha: " + reader.GetString(1) + " | " + (reader.IsDBNull(2) ? "Sin lugar" : reader.GetString(2)));
        }
        
        if (!hasRows)
        {
            result.AppendLine("No hay eventos registrados");
        }
        
        return result.ToString();
    }

    private async Task<string> GetMemoriesContextAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Title, StartDate, Place, IsPositiveMemory
            FROM PersonLifeEvents 
            ORDER BY StartDate DESC 
            LIMIT 5";
        
        var result = new StringBuilder();
        result.AppendLine("Recuerdos:");
        
        await using var reader = await command.ExecuteReaderAsync();
        var hasRows = false;
        
        while (await reader.ReadAsync())
        {
            hasRows = true;
            var isPositive = reader.GetInt32(3) == 1 ? "positivo" : "negativo";
            result.AppendLine("- " + reader.GetString(0) + " | " + (reader.IsDBNull(1) ? "Sin fecha" : reader.GetString(1)) + " (" + isPositive + ")");
        }
        
        if (!hasRows)
        {
            result.AppendLine("No hay recuerdos registrados");
        }
        
        return result.ToString();
    }

    private async Task<string> GetRoutinesContextAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT p.Name, pr.Title, pr.DaysOfWeek, pr.StartTime, pr.Notes
            FROM PersonRoutines pr
            JOIN People p ON pr.PersonId = p.Id
            WHERE pr.IsActive = 1 
            ORDER BY p.Name, pr.StartTime
            LIMIT 10";
        
        var result = new StringBuilder();
        result.AppendLine("Rutinas de todas las personas:");
        
        await using var reader = await command.ExecuteReaderAsync();
        var hasRows = false;
        
        while (await reader.ReadAsync())
        {
            hasRows = true;
            result.AppendLine("- " + reader.GetString(0) + ": " + reader.GetString(1) + " | " + (reader.IsDBNull(2) ? "Todos los dias" : TranslateDays(reader.GetString(2))));
            if (!reader.IsDBNull(4) && !string.IsNullOrEmpty(reader.GetString(4)))
            {
                result.AppendLine("  Notas: " + reader.GetString(4));
            }
        }
        
        if (!hasRows)
        {
            result.AppendLine("No hay rutinas registradas");
        }
        
        return result.ToString();
    }

    private async Task<string> GetTasksContextAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Title, Status
            FROM PersonAgendaItems 
            WHERE Status = 0 
            LIMIT 5";
        
        var result = new StringBuilder();
        result.AppendLine("Pendientes:");
        
        await using var reader = await command.ExecuteReaderAsync();
        var hasRows = false;
        
        while (await reader.ReadAsync())
        {
            hasRows = true;
            result.AppendLine("- " + reader.GetString(0));
        }
        
        if (!hasRows)
        {
            result.AppendLine("No hay tareas pendientes");
        }
        
        return result.ToString();
    }

    private async Task<string> GetMedicationsContextAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT m.Name, m.Dose, ms.Turn
            FROM Medications m
            JOIN MedicationSchedules ms ON m.Id = ms.MedicationId
            WHERE m.IsActive = 1 AND ms.IsActive = 1
            ORDER BY ms.Turn
            LIMIT 10";
        
        var result = new StringBuilder();
        result.AppendLine("Medicamentos:");
        
        await using var reader = await command.ExecuteReaderAsync();
        var hasRows = false;
        
        while (await reader.ReadAsync())
        {
            hasRows = true;
            result.AppendLine("- " + reader.GetString(0) + " | Dosis: " + reader.GetString(1) + " | Turno: " + reader.GetInt32(2));
        }
        
        if (!hasRows)
        {
            result.AppendLine("No hay medicamentos registrados");
        }
        
        return result.ToString();
    }
}