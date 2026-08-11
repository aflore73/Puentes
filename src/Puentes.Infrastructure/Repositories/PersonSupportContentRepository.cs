using Dapper;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public class PersonSupportContentRepository
{
    private readonly AccessDb _accessDb;

    public PersonSupportContentRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public async Task CreateTableAsync()
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(
            PersonSupportContentScripts.CreateTable);
    }

    public async Task AddAsync(PersonSupportContent supportContent)
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(
            PersonSupportContentScripts.Insert,
            supportContent);
    }

    public async Task<IEnumerable<PersonSupportContent>>
        GetActiveByPersonAsync(Guid personId)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QueryAsync<PersonSupportContent>(
            PersonSupportContentScripts.SelectActiveByPersonId,
            new { PersonId = personId });
    }
}
