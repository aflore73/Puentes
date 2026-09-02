using Dapper;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public sealed class PersonAliasRepository
{
    private readonly AccessDb _accessDb;

    public PersonAliasRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public async Task CreateTableAsync()
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonAliasScripts.CreateTable);
    }

    public async Task AddAsync(PersonAlias alias)
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonAliasScripts.Insert, alias);
    }

    public async Task<IEnumerable<PersonAlias>> GetAllAsync()
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QueryAsync<PersonAlias>(PersonAliasScripts.SelectAll);
    }

    public async Task<IEnumerable<PersonAlias>> GetByPersonAsync(Guid personId)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QueryAsync<PersonAlias>(
            PersonAliasScripts.SelectByPersonId,
            new { PersonId = personId });
    }
}
