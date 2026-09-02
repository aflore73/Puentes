using System;
using System.Linq;
using System.Threading.Tasks;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Repositories;
using Puentes.LocalInterpreter.Context;
using Puentes.Shared.Domain;

namespace Puentes.LocalInterpreter.Services;

/// <summary>Seeds the real DB's PersonAliases table from the hardcoded PeopleContext, by matching on name.</summary>
public static class AliasSeeder
{
    public static async Task<int> SeedFromLocalContextAsync(AccessDb accessDb)
    {
        var personRepository = new PersonRepository(accessDb);
        var aliasRepository = new PersonAliasRepository(accessDb);

        var dbPeople = (await personRepository.GetAllAsync()).ToList();

        int insertedCount = 0;

        foreach (Models.Person localPerson in PeopleContext.People)
        {
            Person? dbPerson = dbPeople.FirstOrDefault(p =>
                p.Name.Equals(localPerson.Name, StringComparison.OrdinalIgnoreCase));

            if (dbPerson == null)
            {
                Console.WriteLine(
                    $"  (sin coincidencia en la BD para \"{localPerson.Name}\", se omite)");
                continue;
            }

            var existingAliases = (await aliasRepository.GetByPersonAsync(dbPerson.Id))
                .Select(a => a.Alias)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (string alias in localPerson.Aliases)
            {
                if (existingAliases.Contains(alias))
                    continue;

                await aliasRepository.AddAsync(new PersonAlias
                {
                    Id = Guid.NewGuid(),
                    PersonId = dbPerson.Id,
                    Alias = alias
                });

                Console.WriteLine($"  + \"{alias}\" -> {dbPerson.Name}");
                insertedCount++;
            }
        }

        return insertedCount;
    }
}
