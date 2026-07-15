using Puentes.Infrastructure.Database;
using Dapper;
using Puentes.Core.Domain;

namespace Puentes.Infrastructure.Repositories;

public class EventRepository(AccessDb database)
{
    public async Task AddAsync(Event evento)
    {
        using var connection = database.CreateConnection();

        const string sql =
        """
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
    public Task<Event?> GetByIdAsync(Guid id) 
    {
        return null!;                 
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
    public Task<Event?> GetAllAsync()
    {
        return null!;
    }
}
