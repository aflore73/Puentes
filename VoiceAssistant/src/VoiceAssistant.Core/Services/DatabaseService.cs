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

    private async Task<string> GetAgendaContextAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        // Obtener TODOS los eventos (pasados y futuros)
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
            var title = reader.GetString(0);
            var dateStr = reader.GetString(1);
            var place = reader.IsDBNull(2) ? "Sin lugar" : reader.GetString(2);
            
            result.AppendLine("- " + title + " | Fecha: " + dateStr + " | " + place);
        }
        
        if (!hasRows)
        {
            result.AppendLine("No hay eventos registrados en la agenda");
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
            SELECT Title, DaysOfWeek, StartTime, EndTime
            FROM PersonRoutines 
            WHERE IsActive = 1 
            LIMIT 5";
        
        var result = new StringBuilder();
        result.AppendLine("Rutinas:");
        
        await using var reader = await command.ExecuteReaderAsync();
        var hasRows = false;
        
        while (await reader.ReadAsync())
        {
            hasRows = true;
            result.AppendLine("- " + reader.GetString(0) + " | " + (reader.IsDBNull(1) ? "Todos los dias" : reader.GetString(1)));
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
	    public async Task<string> GetPersonRoutineContextAsync(string personName)
    {
        await InitializeAsync();
        
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT p.Name, pr.Title, pr.DaysOfWeek, pr.StartTime, pr.EndTime, pr.Notes
            FROM PersonRoutines pr
            JOIN People p ON pr.PersonId = p.Id
            WHERE p.Name LIKE @name AND pr.IsActive = 1
            ORDER BY pr.StartTime";
        
        command.Parameters.AddWithValue("@name", "%" + personName + "%");
        
        var result = new StringBuilder();
        result.AppendLine("Rutinas de " + personName + ":");
        
        await using var reader = await command.ExecuteReaderAsync();
        var hasRows = false;
        
        while (await reader.ReadAsync())
        {
            hasRows = true;
            var title = reader.GetString(1);
            var days = reader.IsDBNull(2) ? "Todos los dias" : reader.GetString(2);
            var startTime = reader.IsDBNull(3) ? "Sin hora especifica" : reader.GetString(3);
            var endTime = reader.IsDBNull(4) ? "Sin hora fin" : reader.GetString(4);
            
            result.AppendLine("- " + title);
            result.AppendLine("  Dias: " + days);
            result.AppendLine("  Horario: " + startTime + " a " + endTime);
        }
        
        if (!hasRows)
        {
            result.AppendLine("No se encontraron rutinas para " + personName);
        }
        
        return result.ToString();
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


    public async Task SaveConversationAsync(string userInput, string assistantResponse, string category, double confidence)
    {
        await InitializeAsync();
        
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Conversations (UserInput, AssistantResponse, Category, Confidence) VALUES (@input, @response, @category, @confidence)";
        
        command.Parameters.AddWithValue("@input", userInput);
        command.Parameters.AddWithValue("@response", assistantResponse);
        command.Parameters.AddWithValue("@category", category);
        command.Parameters.AddWithValue("@confidence", confidence);
        
        await command.ExecuteNonQueryAsync();
    }
}