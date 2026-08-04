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
}
