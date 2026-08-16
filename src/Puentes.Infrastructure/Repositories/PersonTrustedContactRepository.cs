using Dapper;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public sealed class PersonTrustedContactRepository
{
    private readonly AccessDb _accessDb;
    public PersonTrustedContactRepository(AccessDb accessDb) => _accessDb = accessDb;
    public async Task CreateTableAsync()
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonTrustedContactScripts.CreateTable);
    }
    public async Task AddAsync(PersonTrustedContact contact)
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonTrustedContactScripts.Insert, contact);
    }
    public async Task<IEnumerable<PersonTrustedContact>> GetActiveAsync(Guid personId)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QueryAsync<PersonTrustedContact>(
            PersonTrustedContactScripts.SelectActive, new { PersonId = personId });
    }
    public async Task<bool> UpdateAsync(PersonTrustedContact contact)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.ExecuteAsync(PersonTrustedContactScripts.Update, contact) == 1;
    }
}
