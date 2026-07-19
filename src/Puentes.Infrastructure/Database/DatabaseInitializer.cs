using Dapper;
using Puentes.Infrastructure.Database.Scripts;
using System.Data;

namespace Puentes.Infrastructure.Database;

public class DatabaseInitializer
{
    private readonly AccessDb _accessDb;

    public DatabaseInitializer(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public void Initialize()
    {
        using var connection = _accessDb.OpenConnection();

        //connection.Execute(EventScripts.CreateTable);

        connection.Execute(MedicationScripts.CreateTable);

       // connection.Execute(MedicationTurnScripts.CreateTable);

        //connection.Execute(MedicationScheduleScripts.CreateTable);

        //connection.Execute(MedicationRecordScripts.CreateTable);

        SeedMedicationTurns(connection);
    }

    private static void SeedMedicationTurns(IDbConnection connection)
    {
        // Lo implementaremos en el siguiente paso.
    }
}