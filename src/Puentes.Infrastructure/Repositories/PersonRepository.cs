using Dapper;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Database.Scripts;
using Puentes.Shared.Domain;

namespace Puentes.Infrastructure.Repositories;

public class PersonRepository
{
    private readonly AccessDb _accessDb;

    public PersonRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public async Task CreateTableAsync()
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonScripts.CreateTable);

        var columns = (await connection.QueryAsync<string>(
                "SELECT name FROM pragma_table_info('People');"))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!columns.Contains("BirthDate"))
        {
            await connection.ExecuteAsync(PersonScripts.AddBirthDateColumn);
        }

        if (!columns.Contains("City"))
        {
            await connection.ExecuteAsync(PersonScripts.AddCityColumn);
        }

        if (!columns.Contains("Province"))
        {
            await connection.ExecuteAsync(PersonScripts.AddProvinceColumn);
        }

        if (!columns.Contains("Country"))
        {
            await connection.ExecuteAsync(PersonScripts.AddCountryColumn);
        }

        if (!columns.Contains("Notes"))
        {
            await connection.ExecuteAsync(PersonScripts.AddNotesColumn);
        }
    }

    public async Task AddAsync(Person person)
    {
        using var connection = _accessDb.OpenConnection();
        await connection.ExecuteAsync(PersonScripts.Insert, person);
    }

    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QueryAsync<Person>(PersonScripts.SelectAll);
    }

    public async Task<Person?> GetByIdAsync(Guid id)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.QuerySingleOrDefaultAsync<Person>(
            PersonScripts.SelectById,
            new { Id = id });
    }

    public async Task<bool> UpdateAsync(Person person)
    {
        using var connection = _accessDb.OpenConnection();
        return await connection.ExecuteAsync(PersonScripts.Update, person) > 0;
    }
}
