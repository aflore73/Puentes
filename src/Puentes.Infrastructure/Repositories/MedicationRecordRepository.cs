using Dapper;
using Puentes.Infrastructure.Database;
using Puentes.Shared.Domain;
using Puentes.Shared.Enums;

public class MedicationRecordRepository
{
    private readonly AccessDb _accessDb;

    public MedicationRecordRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public async Task CreateTableAsync()
    {
        using var connection = _accessDb.OpenConnection();

        await connection.ExecuteAsync(
            MedicationRecordScripts.CreateTable);

        await connection.ExecuteAsync(
            MedicationRecordScripts.CreateIndex);
    }

    public async Task AddAsync(MedicationRecord record)
    {
        using var connection = _accessDb.OpenConnection();

        await connection.ExecuteAsync(
            MedicationRecordScripts.Insert,
            record);
    }

    public async Task<IEnumerable<MedicationRecord>> GetTodayAsync(Guid patientId)
    {
        using var connection = _accessDb.OpenConnection();

        return await connection.QueryAsync<MedicationRecord>(
            MedicationRecordScripts.SelectToday,
            new
            {
                PatientId = patientId
            });
    }
    public async Task<MedicationRecord?> GetTodayAsync(
        Guid patientId,
        MedicationTurnType turn)
    {
        using var connection = _accessDb.OpenConnection();

        return await connection.QueryFirstOrDefaultAsync<MedicationRecord>(
            MedicationRecordScripts.SelectTodayByTurn,
            new
            {
                PatientId = patientId,
                Turn = turn
            });
    }
}