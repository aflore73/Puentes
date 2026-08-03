using Microsoft.Data.Sqlite;
using System.Data;

namespace Puentes.Infrastructure.Database;

public class AccessDb(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public IDbConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);

        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();

        return connection;
    }
}
