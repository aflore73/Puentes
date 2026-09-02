using System;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Puentes.Infrastructure.Configuration;
using Puentes.Infrastructure.Database;

namespace Puentes.LocalInterpreter.Data;

/// <summary>Opens the shared Puentes SQLite database and ensures its schema exists.</summary>
public static class DatabaseConnectionFactory
{
    private static AccessDb? _accessDb;
    private static bool _typeHandlersRegistered;

    public static async Task<AccessDb> GetOrCreateAsync()
    {
        if (_accessDb != null)
            return _accessDb;

        RegisterDapperTypeHandlers();

        string dbPath = ResolveDatabasePath();

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        _accessDb = new AccessDb($"Data Source={dbPath}");

        await new DatabaseInitializer(_accessDb).InitializeAsync();

        return _accessDb;
    }

    private static void RegisterDapperTypeHandlers()
    {
        if (_typeHandlersRegistered)
            return;

        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        _typeHandlersRegistered = true;
    }

    private static string ResolveDatabasePath()
    {
        string? overridePath = Environment.GetEnvironmentVariable("PUENTES_DB_PATH");

        if (!string.IsNullOrWhiteSpace(overridePath))
            return overridePath;

        return Path.Combine(
            Directory.GetCurrentDirectory(),
            "Data",
            "Puentes.db");
    }
}
