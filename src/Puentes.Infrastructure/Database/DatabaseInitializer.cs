using Dapper;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Infrastructure.Repositories;
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
        var _medicationRepository = new MedicationRepository(_accessDb);
        var _medicationScheduleRepository = new MedicationScheduleRepository(_accessDb);
        var _medicationRecordRepository = new MedicationRecordRepository(_accessDb);

        _ = _medicationRepository.CreateTableAsync();

        _ = _medicationScheduleRepository.CreateTableAsync();

        _ = _medicationRecordRepository.CreateTableAsync();
    }
}