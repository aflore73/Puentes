using Dapper;
using Puentes.Core.Domain;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;

namespace Puentes.Infrastructure.Repositories;

public class MedicationScheduleRepository
{
    private readonly AccessDb _accessDb;

    public MedicationScheduleRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }
    public async Task ReplaceBatchAsync(
    IEnumerable<MedicationScheduleBatchRequest> request)
    {
        using var connection = _accessDb.OpenConnection();

        using var transaction = connection.BeginTransaction();

        foreach (var item in request)
        {
            await connection.ExecuteAsync(
                MedicationScheduleScripts.DeleteByMedicationId,
                new { item.MedicationId },
                transaction);

            var schedules = item.Schedules.Select(x =>
                new MedicationSchedule
                {
                    Id = Guid.NewGuid(),
                    MedicationId = item.MedicationId,
                    Turn = x.Turn,
                    Quantity = x.Quantity,
                    IsActive = true
                });

            await connection.ExecuteAsync(
                MedicationScheduleScripts.Insert,
                schedules,
                transaction);
        }

        transaction.Commit();
    }
    public async Task ReplaceAsync(
    Guid medicationId,
    IEnumerable<MedicationSchedule> schedules)
    {
        using var connection = _accessDb.OpenConnection();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            MedicationScheduleScripts.DeleteByMedicationId,
            new { MedicationId = medicationId },
            transaction);

        await connection.ExecuteAsync(
            MedicationScheduleScripts.Insert,
            schedules,
            transaction);

        transaction.Commit();
    }
    public async Task<IEnumerable<MedicationSchedule>> GetByMedicationIdAsync(
    Guid medicationId)
    {
        using var connection = _accessDb.OpenConnection();

        return await connection.QueryAsync<MedicationSchedule>(
            MedicationScheduleScripts.SelectByMedicationId,
            new { MedicationId = medicationId });
    }
}