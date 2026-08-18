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
            columns = await connection.QueryAsync<string>(
                "SELECT name FROM pragma_table_info('PersonLifeEvents');");
        }

    }

    public async Task AddAsync(LifeEvent lifeEvent)
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(LifeEventScripts.Insert, lifeEvent);
    }

    public async Task AddWithTopicsAsync(
        LifeEvent lifeEvent,
        IEnumerable<string> topicCodes,
        IEnumerable<LifeEventParticipant>? participants = null)
    {
        var codes = topicCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        using var connection = _accessDb.OpenConnection();
        using var transaction = connection.BeginTransaction();
        var storedPersonId = await connection.ExecuteScalarAsync<string?>(
            "SELECT Id FROM People WHERE Id = @PersonId;",
            new
            {
                PersonId = lifeEvent.PersonId.ToString("D").ToUpperInvariant()
            }, transaction);
        if (storedPersonId is null)
        {
            throw new ArgumentException("La persona asociada no existe.");
        }
        var topics = (await connection.QueryAsync<ContentTopic>(
            "SELECT Id, Code, Name, GroupName FROM ContentTopics " +
            "WHERE GroupName = 'Memory' AND Code IN @Codes;",
            new { Codes = codes }, transaction)).ToArray();
        if (topics.Length != codes.Length)
        {
            throw new ArgumentException(
                "Uno o mas TopicCodes no existen o no son temas de recuerdos.");
        }

        var participantItems = participants?.ToArray() ?? [];
        var participantIds = participantItems
            .Select(item => item.PersonId.ToString("D").ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var storedParticipantIds = participantIds.Length == 0
            ? []
            : (await connection.QueryAsync<string>(
                "SELECT Id FROM People WHERE Id IN @Ids;",
                new { Ids = participantIds }, transaction)).ToArray();
        if (storedParticipantIds.Length != participantIds.Length)
        {
            throw new ArgumentException(
                "Uno o mas participantes no existen.");
        }

        await connection.ExecuteAsync(LifeEventScripts.Insert, new
        {
            Id = lifeEvent.Id.ToString("D").ToUpperInvariant(),
            PersonId = storedPersonId,
            lifeEvent.StartDate,
            lifeEvent.EndDate,
            lifeEvent.DatePrecision,
            lifeEvent.Title,
            lifeEvent.Description,
            lifeEvent.Place,
            lifeEvent.IsPositiveMemory
        }, transaction);
        foreach (var topic in topics)
        {
            await connection.ExecuteAsync(
                "INSERT INTO PersonLifeEventTopics " +
                "(LifeEventId, TopicId) VALUES (@LifeEventId, @TopicId);",
                new
                {
                    LifeEventId = lifeEvent.Id.ToString("D").ToUpperInvariant(),
                    TopicId = topic.Id.ToString("D").ToUpperInvariant()
                },
                transaction);
        }
        foreach (var participant in participantItems)
        {
            await connection.ExecuteAsync(
                LifeEventScripts.InsertParticipant,
                new
                {
                    LifeEventId = lifeEvent.Id.ToString("D").ToUpperInvariant(),
                    PersonId = participant.PersonId.ToString("D")
                        .ToUpperInvariant(),
                    participant.Role
                },
                transaction);
        }

        transaction.Commit();
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

    public async Task<bool> ExistsAsync(Guid id)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT count(*) FROM PersonLifeEvents WHERE Id = @Id;",
            new { Id = id.ToString("D").ToUpperInvariant() }) == 1;
    }
}
