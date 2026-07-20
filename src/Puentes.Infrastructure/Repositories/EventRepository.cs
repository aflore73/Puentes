using Puentes.Infrastructure.Database;
using Dapper;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public class EventRepository: RepositoryBase
{
    public EventRepository(AccessDb _accessDb)
        : base(_accessDb)
    {
    }
    public async Task AddAsync(Event evento)
    {
        using var connection = CreateConnection();

        const string sql = """
        INSERT INTO Events
        (
            Id,
            PersonId,
            Type,
            Description,
            OccurredAt,
            CreatedAt
        )
        VALUES
        (
            @Id,
            @PersonId,
            @Type,
            @Description,
            @OccurredAt,
            @CreatedAt
        );
        """;

        await connection.ExecuteAsync(sql, evento);
    }
    public async Task<Event?> GetByIdAsync(Guid id)
    {
        using var connection = CreateConnection();

        const string sql = """
        SELECT *
        FROM Events
        WHERE Id = @Id;
        """;

        return await connection.QuerySingleOrDefaultAsync<Event>(
            sql,
            new { Id = id });
    }
    public async Task UpdateAsync(Event evento)
    {
        using var connection = CreateConnection();

        const string sql = """
        UPDATE Events
        SET
            PersonId = @PersonId,
            Type = @Type,
            Description = @Description,
            OccurredAt = @OccurredAt,
            CreatedAt = @CreatedAt
        WHERE Id = @Id;
        """;

        await connection.ExecuteAsync(sql, evento);
    }
    public async Task DeleteAsync(Guid id)
    {
        using var connection = CreateConnection();

        const string sql = """
        DELETE FROM Events
        WHERE Id = @Id;
        """;

        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public Task<IEnumerable<Event>> GetByPersonAsync(Guid personId)
    {
        return null!;
    }

    public Task<IEnumerable<Event>> GetTodayAsync(Guid personId) 
    {
        return null!;
    }

    public Task<IEnumerable<Event>> GetByTypeAsync(
        Guid personId,
        EventType type)
    {
        return null!;
    }
    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        using var connection = CreateConnection();

        const string sql = """
        SELECT *
        FROM Events
        ORDER BY OccurredAt DESC;
        """;

        return await connection.QueryAsync<Event>(sql);
    }
}
