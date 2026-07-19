using Dapper;
using Puentes.Core.Domain;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;

namespace Puentes.Infrastructure.Repositories;

public class MedicationTurnRepository
{
    private readonly AccessDb _accessDb;

    public MedicationTurnRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public async Task AddAsync(MedicationTurn turn)
    {
        using var connection = _accessDb.OpenConnection();

        await connection.ExecuteAsync(
            MedicationTurnScripts.Insert,
            turn);
    }

    public async Task<IEnumerable<MedicationTurn>> GetAllAsync()
    {
        using var connection = _accessDb.OpenConnection();

        return await connection.QueryAsync<MedicationTurn>(
            MedicationTurnScripts.SelectAll);
    }

    public async Task<MedicationTurn?> GetByIdAsync(Guid id)
    {
        using var connection = _accessDb.OpenConnection();

        return await connection.QuerySingleOrDefaultAsync<MedicationTurn>(
            MedicationTurnScripts.SelectById,
            new { Id = id });
    }

    public async Task UpdateAsync(MedicationTurn turn)
    {
        using var connection = _accessDb.OpenConnection();

        await connection.ExecuteAsync(
            MedicationTurnScripts.Update,
            turn);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _accessDb.OpenConnection();

        await connection.ExecuteAsync(
            MedicationTurnScripts.Delete,
            new { Id = id });
    }
}