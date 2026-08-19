using Dapper;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public class PersonRoutineRepository
{
    private readonly AccessDb _accessDb;

    public PersonRoutineRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public async Task CreateTableAsync()
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonRoutineScripts.CreateTable);

        var columns = (await connection.QueryAsync<string>(
                "SELECT name FROM pragma_table_info('PersonRoutines');"))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!columns.Contains("DaysOfWeek"))
        {
            await connection.ExecuteAsync(
                PersonRoutineScripts.AddDaysOfWeekColumn);
        }

        if (!columns.Contains("StartTime"))
        {
            await connection.ExecuteAsync(PersonRoutineScripts.AddStartTimeColumn);
        }

        if (!columns.Contains("EndTime"))
        {
            await connection.ExecuteAsync(PersonRoutineScripts.AddEndTimeColumn);
        }
    }

    public async Task AddAsync(PersonRoutine routine)
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonRoutineScripts.Insert, routine);
    }

    public async Task<IEnumerable<PersonRoutine>> GetActiveByPersonAsync(
        Guid personId)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QueryAsync<PersonRoutine>(
            PersonRoutineScripts.SelectActiveByPersonId,
            new { PersonId = personId });
    }

    public async Task<bool> UpdateAsync(PersonRoutine routine)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.ExecuteAsync(
            PersonRoutineScripts.Update, routine) == 1;
    }
}
