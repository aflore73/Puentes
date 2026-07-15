using Microsoft.Data.Sqlite;
using System.Data;

namespace Puentes.Infrastructure.Database;

public class AccessDb(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}