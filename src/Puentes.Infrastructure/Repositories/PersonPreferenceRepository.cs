using Dapper;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public class PersonPreferenceRepository
{
    private readonly AccessDb _accessDb;

    public PersonPreferenceRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public async Task CreateTableAsync()
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonPreferenceScripts.CreateTable);
    }

    public async Task AddAsync(PersonPreference preference)
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(
            PersonPreferenceScripts.Insert,
            preference);
    }

    public async Task<IEnumerable<PersonPreference>> GetActiveByPersonAsync(
        Guid personId)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QueryAsync<PersonPreference>(
            PersonPreferenceScripts.SelectActiveByPersonId,
            new { PersonId = personId });
    }
}
