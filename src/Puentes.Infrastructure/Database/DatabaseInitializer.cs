using Dapper;

namespace Puentes.Infrastructure.Database;

public class DatabaseInitializer(AccessDb database)
{
    public void Initialize()
    {
        using var connection = database.CreateConnection();

        connection.Execute("""
            CREATE TABLE IF NOT EXISTS Events
            (
                Id TEXT PRIMARY KEY,
                PersonId TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Description TEXT NOT NULL,
                OccurredAt TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );
        """);
    }
}