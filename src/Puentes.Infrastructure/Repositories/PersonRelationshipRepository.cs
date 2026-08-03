using Dapper;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public class PersonRelationshipRepository
{
    private readonly AccessDb _accessDb;

    public PersonRelationshipRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public async Task CreateTableAsync()
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonRelationshipScripts.CreateTable);
    }

    public async Task AddAsync(PersonRelationship relationship)
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(
            PersonRelationshipScripts.Insert,
            relationship);
    }

    public async Task<IEnumerable<PersonRelationship>> GetAllForPersonAsync(
        Guid personId)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QueryAsync<PersonRelationship>(
            PersonRelationshipScripts.SelectByParticipantId,
            new { PersonId = personId });
    }
}
