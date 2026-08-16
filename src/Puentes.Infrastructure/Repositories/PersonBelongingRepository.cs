using Dapper;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public class PersonBelongingRepository
{
    private readonly AccessDb _accessDb;

    public PersonBelongingRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public async Task CreateTableAsync()
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonBelongingScripts.CreateTable);
    }

    public async Task AddAsync(PersonBelonging belonging)
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonBelongingScripts.Insert, belonging);
    }

    public async Task<IEnumerable<PersonBelonging>> GetActiveByPersonAsync(
        Guid personId)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QueryAsync<PersonBelonging>(
            PersonBelongingScripts.SelectActiveByPersonId,
            new { PersonId = personId });
    }

    public async Task<bool> UpdateAsync(PersonBelonging belonging)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.ExecuteAsync(
            PersonBelongingScripts.Update, belonging) == 1;
    }
}
