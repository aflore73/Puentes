using Dapper;
using Puentes.Infrastructure.Configuration;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Infrastructure.Repositories;
using Puentes.Shared.Domain;

namespace Puentes.Tests;

public sealed class ContentTopicRepositoryTests
{
    [Fact]
    public async Task GuidParametersUseCanonicalDatabaseFormat()
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        var path = Path.Combine(Path.GetTempPath(), $"puentes-{Guid.NewGuid():N}.db");
        try
        {
            var database = new AccessDb($"Data Source={path};Pooling=False");
            using var connection = database.OpenConnection();
            var id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var stored = await connection.ExecuteScalarAsync<string>(
                "SELECT @Id;", new { Id = id });
            Assert.Equal(id.ToString("D").ToUpperInvariant(), stored);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task MigratesExistingRecordsAndReadsTheirTopicCodes()
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        var path = Path.Combine(Path.GetTempPath(), $"puentes-{Guid.NewGuid():N}.db");
        try
        {
            var database = new AccessDb($"Data Source={path};Pooling=False");
            using (var connection = database.OpenConnection())
            {
                await connection.ExecuteAsync("""
                    CREATE TABLE People (Id TEXT PRIMARY KEY);
                    INSERT INTO People VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa');
                    """);
                await connection.ExecuteAsync(LifeEventScripts.CreateTables);
                await connection.ExecuteAsync(PersonSupportContentScripts.CreateTable);
                await connection.ExecuteAsync(PersonPreferenceScripts.CreateTable);
                await connection.ExecuteAsync("""
                    INSERT INTO PersonLifeEvents
                      (Id, PersonId, DatePrecision, Title, Description,
                       IsPositiveMemory)
                    VALUES ('bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb',
                      'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 0,
                      'Vacaciones en Mar del Plata', 'De chica viajaba', 1);
                    INSERT INTO PersonSupportContents
                      (Id, PersonId, Title, Content, Attribution, Reference)
                    VALUES ('cccccccccccccccccccccccccccccccc',
                      'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Dios sostiene',
                      'Texto', 'Biblia - Reina-Valera 1960', 'Salmos 55:22');
                    INSERT INTO PersonPreferences
                      (Id, PersonId, Title, Notes)
                    VALUES ('dddddddddddddddddddddddddddddddd',
                      'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Musica',
                      'Le gusta Sandro');
                    """);
            }

            var repository = new ContentTopicRepository(database);
            await repository.CreateTablesAndMigrateAsync();
            await new GuidNormalizationRepository(database).NormalizeAsync();

            using (var connection = database.OpenConnection())
            {
                Assert.Equal(13, await connection.ExecuteScalarAsync<int>(
                    "SELECT count(*) FROM ContentTopics;"));
                Assert.Equal(1, await connection.ExecuteScalarAsync<int>(
                    "SELECT count(*) FROM PersonLifeEventTopics;"));
                Assert.Equal(1, await connection.ExecuteScalarAsync<int>(
                    "SELECT count(*) FROM PersonSupportContentTopics;"));
                Assert.Equal(1, await connection.ExecuteScalarAsync<int>(
                    "SELECT count(*) FROM PersonPreferenceTopics;"));
            }

            Assert.Equal(["memory.travel"],
                await repository.GetLifeEventCodesAsync(
                    Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")));
            Assert.Equal(["reading.religious"],
                await repository.GetSupportContentCodesAsync(
                    Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc")));
            Assert.Equal(["interest.music"],
                await repository.GetPreferenceCodesAsync(
                    Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd")));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task AddsLifeEventAndTopicsInOneTransaction()
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        var path = Path.Combine(Path.GetTempPath(), $"puentes-{Guid.NewGuid():N}.db");
        try
        {
            var database = new AccessDb($"Data Source={path};Pooling=False");
            using (var connection = database.OpenConnection())
            {
                await connection.ExecuteAsync("""
                    CREATE TABLE People (Id TEXT PRIMARY KEY);
                    INSERT INTO People VALUES ('aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa');
                    INSERT INTO People VALUES ('eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee');
                    """);
                await connection.ExecuteAsync(LifeEventScripts.CreateTables);
                await connection.ExecuteAsync(PersonSupportContentScripts.CreateTable);
                await connection.ExecuteAsync(PersonPreferenceScripts.CreateTable);
            }
            var topics = new ContentTopicRepository(database);
            await topics.CreateTablesAndMigrateAsync();
            await new GuidNormalizationRepository(database).NormalizeAsync();
            using (var connection = database.OpenConnection())
            {
                Assert.Equal("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA",
                    await connection.ExecuteScalarAsync<string>(
                        "SELECT Id FROM People;"));
            }
            var events = new LifeEventRepository(database);
            var lifeEvent = new LifeEvent
            {
                Id = Guid.NewGuid(),
                PersonId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Title = "Un viaje feliz",
                DatePrecision = DatePrecision.Year,
                IsPositiveMemory = true
            };

            await events.AddWithTopicsAsync(lifeEvent,
                ["memory.travel", "memory.family"],
                [new LifeEventParticipant
                {
                    LifeEventId = lifeEvent.Id,
                    PersonId = Guid.Parse(
                        "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                    Role = "Acompañante"
                }]);

            Assert.Equal(["memory.family", "memory.travel"],
                await topics.GetLifeEventCodesAsync(lifeEvent.Id));
            var participant = Assert.Single(
                await events.GetParticipantsAsync(lifeEvent.Id));
            Assert.Equal("Acompañante", participant.Role);

            var invalidEvent = new LifeEvent
            {
                Id = Guid.NewGuid(), PersonId = lifeEvent.PersonId,
                Title = "No debe guardarse", DatePrecision = DatePrecision.Year
            };
            await Assert.ThrowsAsync<ArgumentException>(() =>
                events.AddWithTopicsAsync(invalidEvent, ["interest.music"]));
            Assert.False(await events.ExistsAsync(invalidEvent.Id));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task AddsAndReadsFutureAgendaItem()
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
        var path = Path.Combine(Path.GetTempPath(), $"puentes-{Guid.NewGuid():N}.db");
        try
        {
            var database = new AccessDb($"Data Source={path};Pooling=False");
            using (var connection = database.OpenConnection())
            {
                await connection.ExecuteAsync("""
                    CREATE TABLE People (Id TEXT PRIMARY KEY);
                    INSERT INTO People VALUES ('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA');
                    INSERT INTO People VALUES ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE');
                    """);
                await connection.ExecuteAsync(LifeEventScripts.CreateTables);
                await connection.ExecuteAsync(PersonSupportContentScripts.CreateTable);
                await connection.ExecuteAsync(PersonPreferenceScripts.CreateTable);
            }
            var agenda = new PersonAgendaItemRepository(database);
            await agenda.CreateTablesAsync();
            var topics = new ContentTopicRepository(database);
            await topics.CreateTablesAndMigrateAsync();
            await new GuidNormalizationRepository(database).NormalizeAsync();
            var item = new PersonAgendaItem
            {
                Id = Guid.NewGuid(),
                PersonId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ScheduledAt = new DateTimeOffset(2026, 9, 10, 10, 0, 0,
                    TimeSpan.FromHours(-3)),
                Title = "Consulta médica"
            };
            await agenda.AddAsync(item, ["agenda.medical-appointment"],
                [new LifeEventParticipant
                {
                    PersonId = Guid.Parse(
                        "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                    Role = "Acompañante"
                }]);

            Assert.Single(await agenda.GetByPersonAsync(item.PersonId, false,
                item.ScheduledAt.AddDays(-1)));
            Assert.Equal(["agenda.medical-appointment"],
                await topics.GetAgendaItemCodesAsync(item.Id));
            Assert.Single(await agenda.GetParticipantsAsync(item.Id));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
