using Dapper;
using Puentes.Infrastructure.Database;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public class PersonBelongingRepository
{
    private const string SelectActiveByPersonId = """
        SELECT Id, PersonId, Name, Notes, Tags, IsActive
        FROM PersonBelongings
        WHERE PersonId = @PersonId
          AND IsActive = 1
        ORDER BY Name;
        """;

    private readonly AccessDb _accessDb;

    public PersonBelongingRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public async Task<IEnumerable<PersonBelonging>> GetActiveByPersonAsync(
        Guid personId)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QueryAsync<PersonBelonging>(
            SelectActiveByPersonId,
            new { PersonId = personId });
    }
}
