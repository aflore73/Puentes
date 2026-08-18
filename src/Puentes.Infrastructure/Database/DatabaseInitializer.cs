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
        var lifeEventRepository = new LifeEventRepository(_accessDb);
        var routineRepository = new PersonRoutineRepository(_accessDb);
        var preferenceRepository = new PersonPreferenceRepository(_accessDb);
        var supportContentRepository =
            new PersonSupportContentRepository(_accessDb);
        var belongingRepository = new PersonBelongingRepository(_accessDb);
        var trustedContactRepository =
            new PersonTrustedContactRepository(_accessDb);
        var contentTopicRepository = new ContentTopicRepository(_accessDb);
        var agendaRepository = new PersonAgendaItemRepository(_accessDb);
        var guidNormalizationRepository =
            new GuidNormalizationRepository(_accessDb);

        await _medicationRepository.CreateTableAsync();

        await _medicationScheduleRepository.CreateTableAsync();

        await _medicationRecordRepository.CreateTableAsync();

        await personRepository.CreateTableAsync();

        await relationshipRepository.CreateTableAsync();

        await lifeEventRepository.CreateTablesAsync();

        await routineRepository.CreateTableAsync();

        await preferenceRepository.CreateTableAsync();

        await supportContentRepository.CreateTableAsync();

        await belongingRepository.CreateTableAsync();

        await trustedContactRepository.CreateTableAsync();

        await agendaRepository.CreateTablesAsync();

        await contentTopicRepository.CreateTablesAndMigrateAsync();

        await guidNormalizationRepository.NormalizeAsync();

    }
}
