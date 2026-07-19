using Dapper;
using Puentes.Core.Domain;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;

namespace Puentes.Infrastructure.Repositories;

public class MedicationRepository
{
    private readonly AccessDb _accessDb;

    public MedicationRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public async Task AddAsync(Medication medication)
    {
        using var connection = _accessDb.OpenConnection();

        await connection.ExecuteAsync(
            MedicationScripts.Insert,
            medication);
    }

    //public async Task<IEnumerable<dynamic>> GetAllAsync()
    //{
    //    using var connection = _accessDb.OpenConnection();

    //    return await connection.QueryAsync(MedicationScripts.SelectAll);
    //}
    public async Task<IEnumerable<Medication>> GetAllAsync()
    {
        using var connection = _accessDb.OpenConnection();

        return await connection.QueryAsync<Medication>(
            MedicationScripts.SelectAll);
    }

    public async Task<Medication?> GetByIdAsync(Guid id)
    {
        using var connection = _accessDb.OpenConnection();

        return await connection.QuerySingleOrDefaultAsync<Medication>(
            MedicationScripts.SelectById,
            new { Id = id });
    }

    public async Task UpdateAsync(Medication medication)
    {
        using var connection = _accessDb.OpenConnection();

        await connection.ExecuteAsync(
            MedicationScripts.Update,
            medication);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _accessDb.OpenConnection();

        await connection.ExecuteAsync(
            MedicationScripts.Delete,
            new { Id = id });
    }
}