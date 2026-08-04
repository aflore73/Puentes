using Dapper;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public class LifeEventRepository
{
    private readonly AccessDb _accessDb;

    public LifeEventRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public async Task CreateTablesAsync()
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(LifeEventScripts.CreateTables);

        var columns = await connection.QueryAsync<string>(
            "SELECT name FROM pragma_table_info('PersonLifeEvents');");

        if (columns.Contains("Type", StringComparer.OrdinalIgnoreCase))
        {
            await connection.ExecuteAsync(LifeEventScripts.RemoveTypeColumn);
        }
    }

    public async Task AddAsync(LifeEvent lifeEvent)
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(LifeEventScripts.Insert, lifeEvent);
    }

    public async Task AddParticipantAsync(LifeEventParticipant participant)
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(
            LifeEventScripts.InsertParticipant,
            participant);
    }

    public async Task<IEnumerable<LifeEvent>> GetByPersonAsync(Guid personId)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QueryAsync<LifeEvent>(
            LifeEventScripts.SelectByPersonId,
            new { PersonId = personId });
    }

    public async Task<IEnumerable<LifeEventParticipant>> GetParticipantsAsync(
        Guid lifeEventId)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QueryAsync<LifeEventParticipant>(
            LifeEventScripts.SelectParticipantsByLifeEventId,
            new { LifeEventId = lifeEventId });
    }
}
