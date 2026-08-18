using Dapper;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public sealed class PersonAgendaItemRepository
{
    private readonly AccessDb _accessDb;

    public PersonAgendaItemRepository(AccessDb accessDb) => _accessDb = accessDb;

    public async Task CreateTablesAsync()
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonAgendaItemScripts.CreateTables);
    }

    public async Task AddAsync(PersonAgendaItem item,
        IEnumerable<string> topicCodes,
        IEnumerable<LifeEventParticipant> participants)
    {
        var codes = topicCodes.Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim()).Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var participantItems = participants.GroupBy(item => item.PersonId)
            .Select(group => group.First()).ToArray();
        var participantIds = participantItems.Select(participant =>
            participant.PersonId.ToString("D").ToUpperInvariant()).ToArray();

        using var connection = _accessDb.OpenConnection();
        using var transaction = connection.BeginTransaction();
        var topics = (await connection.QueryAsync<ContentTopic>(
            "SELECT Id, Code, Name, GroupName FROM ContentTopics " +
            "WHERE GroupName = 'Agenda' AND Code IN @Codes;",
            new { Codes = codes }, transaction)).ToArray();
        if (topics.Length != codes.Length)
            throw new ArgumentException("Uno o mas temas no pertenecen a Agenda.");
        var storedParticipants = participantIds.Length == 0 ? [] :
            (await connection.QueryAsync<string>(
                "SELECT Id FROM People WHERE Id IN @Ids;",
                new { Ids = participantIds }, transaction)).ToArray();
        if (storedParticipants.Length != participantIds.Length)
            throw new ArgumentException("Uno o mas participantes no existen.");

        await connection.ExecuteAsync(PersonAgendaItemScripts.Insert, new
        {
            Id = item.Id.ToString("D").ToUpperInvariant(),
            PersonId = item.PersonId.ToString("D").ToUpperInvariant(),
            item.ScheduledAt, item.EndAt, item.Title, item.Description,
            item.Place, item.Status
        }, transaction);
        foreach (var topic in topics)
            await connection.ExecuteAsync(
                "INSERT INTO PersonAgendaItemTopics (AgendaItemId, TopicId) " +
                "VALUES (@AgendaItemId, @TopicId);",
                new { AgendaItemId = item.Id.ToString("D").ToUpperInvariant(),
                    TopicId = topic.Id.ToString("D").ToUpperInvariant() },
                transaction);
        foreach (var participant in participantItems)
            await connection.ExecuteAsync(
                "INSERT INTO AgendaItemParticipants " +
                "(AgendaItemId, PersonId, Role) VALUES " +
                "(@AgendaItemId, @PersonId, @Role);",
                new { AgendaItemId = item.Id.ToString("D").ToUpperInvariant(),
                    PersonId = participant.PersonId.ToString("D")
                        .ToUpperInvariant(), participant.Role }, transaction);
        transaction.Commit();
    }

    public async Task<IReadOnlyList<PersonAgendaItem>> GetByPersonAsync(
        Guid personId, bool includePast, DateTimeOffset now)
    {
        using var connection = _accessDb.OpenConnection();
        return (await connection.QueryAsync<PersonAgendaItem>(
            PersonAgendaItemScripts.SelectByPerson,
            new { PersonId = personId, IncludePast = includePast, Now = now }))
            .ToList();
    }

    public async Task<IReadOnlyList<LifeEventParticipant>> GetParticipantsAsync(
        Guid agendaItemId)
    {
        using var connection = _accessDb.OpenConnection();
        return (await connection.QueryAsync<LifeEventParticipant>("""
            SELECT AgendaItemId LifeEventId, PersonId, Role
            FROM AgendaItemParticipants
            WHERE AgendaItemId = @AgendaItemId
            ORDER BY PersonId;
            """, new { AgendaItemId = agendaItemId })).ToList();
    }
}
