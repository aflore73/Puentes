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

    public async Task InitializeAsync()
    {
        var _medicationRepository = new MedicationRepository(_accessDb);
        var _medicationScheduleRepository = new MedicationScheduleRepository(_accessDb);
        var _medicationRecordRepository = new MedicationRecordRepository(_accessDb);
        var personRepository = new PersonRepository(_accessDb);
        var relationshipRepository = new PersonRelationshipRepository(_accessDb);

        await _medicationRepository.CreateTableAsync();

        await _medicationScheduleRepository.CreateTableAsync();

        await _medicationRecordRepository.CreateTableAsync();

        await personRepository.CreateTableAsync();

        await relationshipRepository.CreateTableAsync();
    }
}
