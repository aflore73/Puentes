using Dapper;
using System.Data;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public sealed class ContentTopicRepository
{
    private readonly AccessDb _accessDb;

    public ContentTopicRepository(AccessDb accessDb) => _accessDb = accessDb;

    public async Task CreateTablesAndMigrateAsync()
    {
        using var connection = _accessDb.OpenConnection();
        await EnsureAgendaGroupAsync(connection);
        await connection.ExecuteAsync(ContentTopicScripts.CreateTables);
        await connection.ExecuteAsync(ContentTopicScripts.SeedTopics);
        await MigrateLegacyCategoryAsync(connection, "PersonLifeEvents",
            "PersonLifeEventTopics", "LifeEventId", "Memory");
        await MigrateLegacyCategoryAsync(connection, "PersonSupportContents",
            "PersonSupportContentTopics", "SupportContentId", "Reading");
        await SeedExistingContentTopicsAsync(connection);
        await SeedPreferenceTopicsAsync(connection);
    }

    public Task<IReadOnlyList<string>> GetLifeEventCodesAsync(Guid id) =>
        GetCodesAsync("PersonLifeEventTopics", "LifeEventId", id);

    public Task<IReadOnlyList<string>> GetSupportContentCodesAsync(Guid id) =>
        GetCodesAsync("PersonSupportContentTopics", "SupportContentId", id);

    public Task<IReadOnlyList<string>> GetPreferenceCodesAsync(Guid id) =>
        GetCodesAsync("PersonPreferenceTopics", "PreferenceId", id);

    public Task<IReadOnlyList<string>> GetAgendaItemCodesAsync(Guid id) =>
        GetCodesAsync("PersonAgendaItemTopics", "AgendaItemId", id);

    public Task ReplaceSupportContentCodesAsync(Guid id,
        IEnumerable<string> codes) => ReplaceCodesAsync(
            "PersonSupportContentTopics", "SupportContentId",
            "PersonSupportContents", id, codes, "Reading");

    public Task ReplaceLifeEventCodesAsync(Guid id,
        IEnumerable<string> codes) => ReplaceCodesAsync(
            "PersonLifeEventTopics", "LifeEventId", "PersonLifeEvents",
            id, codes, "Memory");

    public async Task<IReadOnlyList<ContentTopic>> GetAllAsync()
    {
        using var connection = _accessDb.OpenConnection();
        return (await connection.QueryAsync<ContentTopic>(
            "SELECT Id, Code, Name, GroupName FROM ContentTopics " +
            "ORDER BY GroupName, Name;")).ToList();
    }

    public Task ReplacePreferenceCodesAsync(Guid id,
        IEnumerable<string> codes) => ReplaceCodesAsync(
            "PersonPreferenceTopics", "PreferenceId", "PersonPreferences",
            id, codes, "Interest");

    public async Task<bool> AreCodesValidAsync(IEnumerable<string> codes,
        string expectedGroup)
    {
        var normalized = NormalizeCodes(codes);
        if (normalized.Length == 0) return true;
        using var connection = _accessDb.OpenConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT count(*) FROM ContentTopics WHERE GroupName = @GroupName " +
            "AND Code IN @Codes;", new { GroupName = expectedGroup,
                Codes = normalized });
        return count == normalized.Length;
    }

    private async Task<IReadOnlyList<string>> GetCodesAsync(
        string table, string key, Guid id)
    {
        using var connection = _accessDb.OpenConnection();
        var sql = string.Format(ContentTopicScripts.SelectCodes, table, key);
        return (await connection.QueryAsync<string>(sql,
            new { ItemId = id.ToString("D").ToUpperInvariant() })).ToList();
    }

    private async Task ReplaceCodesAsync(string table, string key,
        string sourceTable, Guid id, IEnumerable<string> codes,
        string expectedGroup)
    {
        var normalized = NormalizeCodes(codes);
        using var connection = _accessDb.OpenConnection();
        using var transaction = connection.BeginTransaction();
        var storedItemId = await connection.QuerySingleOrDefaultAsync<string>(
            $"SELECT Id FROM {sourceTable} WHERE Id = @ItemId;",
            new { ItemId = id.ToString("D").ToUpperInvariant() },
            transaction);
        if (storedItemId is null)
            throw new ArgumentException("El registro asociado no existe.");
        var topics = (await connection.QueryAsync<ContentTopic>(
            "SELECT Id, Code FROM ContentTopics WHERE GroupName = @GroupName " +
            "AND Code IN @Codes;", new { GroupName = expectedGroup,
                Codes = normalized }, transaction)).ToArray();
        if (topics.Length != normalized.Length)
            throw new ArgumentException("Uno o mas TopicCodes no existen o pertenecen a otro grupo.");
        await connection.ExecuteAsync($"DELETE FROM {table} WHERE {key} = @ItemId;",
            new { ItemId = storedItemId }, transaction);
        foreach (var topic in topics)
            await connection.ExecuteAsync(
                $"INSERT INTO {table} ({key}, TopicId) VALUES (@ItemId, @TopicId);",
                new
                {
                    ItemId = storedItemId,
                    TopicId = topic.Id.ToString("D").ToUpperInvariant()
                },
                transaction);
        transaction.Commit();
    }

    private static string[] NormalizeCodes(IEnumerable<string> codes) =>
        codes.Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim()).Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static async Task MigrateLegacyCategoryAsync(IDbConnection connection,
        string sourceTable, string relationTable, string relationKey,
        string groupName)
    {
        var columns = await connection.QueryAsync<string>(
            $"SELECT name FROM pragma_table_info('{sourceTable}');");
        if (!columns.Contains("Category", StringComparer.OrdinalIgnoreCase)) return;

        var prefix = groupName == "Memory" ? "memory." : "reading.";
        await connection.ExecuteAsync($"""
            INSERT OR IGNORE INTO {relationTable} ({relationKey}, TopicId)
            SELECT s.Id, t.Id
            FROM {sourceTable} s
            INNER JOIN ContentTopics t
              ON t.Code = @Prefix || CASE lower(s.Category)
                WHEN 'travel' THEN 'travel'
                WHEN 'childhood' THEN 'childhood'
                WHEN 'family' THEN 'family'
                WHEN 'religious' THEN 'religious'
                WHEN 'poetry' THEN 'poetry'
                WHEN 'story' THEN 'story'
                ELSE CASE WHEN @GroupName = 'Memory' THEN 'life-story' ELSE 'story' END
              END
            WHERE s.Category IS NOT NULL;
            """, new { Prefix = prefix, GroupName = groupName });
        await connection.ExecuteAsync(
            $"ALTER TABLE {sourceTable} DROP COLUMN Category;");
    }

    private static Task SeedPreferenceTopicsAsync(IDbConnection connection) =>
        connection.ExecuteAsync("""
            INSERT OR IGNORE INTO PersonPreferenceTopics (PreferenceId, TopicId)
            SELECT p.Id, t.Id FROM PersonPreferences p
            JOIN ContentTopics t ON t.Code = CASE
              WHEN lower(p.Title) LIKE '%musica%' OR lower(p.Title) LIKE '%música%'
                OR lower(p.Title) LIKE '%sica%'
                OR lower(coalesce(p.Tags, '')) LIKE '%musica%'
                THEN 'interest.music'
              WHEN lower(p.Title) LIKE '%planta%'
                OR lower(coalesce(p.Tags, '')) LIKE '%planta%'
                THEN 'interest.plants'
              ELSE NULL END;
            """);

    private static async Task EnsureAgendaGroupAsync(IDbConnection connection)
    {
        var tableSql = await connection.ExecuteScalarAsync<string?>(
            "SELECT sql FROM sqlite_master WHERE type = 'table' " +
            "AND name = 'ContentTopics';");
        if (tableSql is null || tableSql.Contains("'Agenda'",
            StringComparison.OrdinalIgnoreCase)) return;

        await connection.ExecuteAsync("PRAGMA foreign_keys = OFF;");
        using var transaction = connection.BeginTransaction();
        try
        {
            await connection.ExecuteAsync("""
                CREATE TABLE ContentTopics_New
                (
                    Id TEXT PRIMARY KEY,
                    Code TEXT NOT NULL UNIQUE COLLATE NOCASE,
                    Name TEXT NOT NULL,
                    GroupName TEXT NOT NULL,
                    CHECK (length(trim(Code)) > 0),
                    CHECK (length(trim(Name)) > 0),
                    CHECK (GroupName IN
                        ('Memory', 'Reading', 'Interest', 'Agenda'))
                );
                INSERT INTO ContentTopics_New (Id, Code, Name, GroupName)
                SELECT Id, Code, Name, GroupName FROM ContentTopics;
                DROP TABLE ContentTopics;
                ALTER TABLE ContentTopics_New RENAME TO ContentTopics;
                """, transaction: transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
        }
    }

    private static async Task SeedExistingContentTopicsAsync(
        IDbConnection connection)
    {
        await connection.ExecuteAsync("""
            INSERT OR IGNORE INTO PersonSupportContentTopics
                (SupportContentId, TopicId)
            SELECT c.Id, t.Id
            FROM PersonSupportContents c
            JOIN ContentTopics t ON t.Code = CASE
              WHEN lower(coalesce(c.Reference, '')) LIKE '%rvr%'
                OR lower(coalesce(c.Attribution, '')) LIKE '%biblia%'
                OR lower(coalesce(c.Tags, '')) LIKE '%biblia%'
                OR lower(c.Title) LIKE '%salmo%'
                OR lower(c.Title) LIKE '%mateo%'
                OR lower(c.Title) LIKE '%romanos%'
                OR lower(c.Title) LIKE '%pedro%'
                OR lower(c.Title) LIKE '%filipenses%'
                OR lower(c.Title) LIKE '%juan%'
                OR lower(c.Title) LIKE '%isaias%'
                OR lower(c.Title) LIKE '%isaías%' THEN 'reading.religious'
              WHEN lower(coalesce(c.Tags, '')) LIKE '%poema%'
                OR lower(coalesce(c.Tags, '')) LIKE '%poesia%'
                OR lower(coalesce(c.Tags, '')) LIKE '%poesía%'
                THEN 'reading.poetry'
              ELSE 'reading.story' END;
            """);
        await connection.ExecuteAsync("""
            INSERT OR IGNORE INTO PersonLifeEventTopics (LifeEventId, TopicId)
            SELECT e.Id, t.Id
            FROM PersonLifeEvents e
            JOIN ContentTopics t ON t.Code = CASE
              WHEN lower(e.Title || ' ' || coalesce(e.Description, '') || ' ' ||
                coalesce(e.Place, '')) LIKE '%vacacion%'
                OR lower(coalesce(e.Place, '')) LIKE '%mar del plata%'
                OR lower(coalesce(e.Place, '')) LIKE '%miramar%'
                THEN 'memory.travel'
              WHEN lower(e.Title || ' ' || coalesce(e.Description, ''))
                LIKE '%de chica%'
                OR lower(e.Title || ' ' || coalesce(e.Description, ''))
                LIKE '%de chico%'
                OR lower(e.Title) LIKE '%infancia%' THEN 'memory.childhood'
              WHEN lower(e.Title || ' ' || coalesce(e.Description, ''))
                LIKE '%familia%'
                OR lower(e.Title) LIKE '%cumplea%' THEN 'memory.family'
              ELSE 'memory.life-story' END
            WHERE e.IsPositiveMemory = 1;
            """);
    }
}
