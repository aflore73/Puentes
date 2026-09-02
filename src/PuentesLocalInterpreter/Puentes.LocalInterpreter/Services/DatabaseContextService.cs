using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Puentes.Infrastructure.Database;
using Puentes.Infrastructure.Repositories;
using Puentes.LocalInterpreter.Data;
using Puentes.LocalInterpreter.Models;
using Puentes.Shared.Domain;
using DbPerson = Puentes.Shared.Domain.Person;

namespace Puentes.LocalInterpreter.Services;

/// <summary>Enriches a local interpretation with facts from the real Puentes SQLite database.</summary>
public static class DatabaseContextService
{
    public static async Task<DatabaseContextResult?> EnrichAsync(
        PersonResult localPerson,
        IntentResult intent)
    {
        if (localPerson.Person == null)
            return null;

        AccessDb accessDb = await DatabaseConnectionFactory.GetOrCreateAsync();

        DbPerson? dbPerson = await FindMatchingPersonAsync(
            accessDb,
            localPerson.Person.Name,
            localPerson.Word);

        if (dbPerson == null)
            return new DatabaseContextResult { Found = false };

        return intent.Intent switch
        {
            "UBICACION" or "EVENTO" =>
                await BuildLifeEventContextAsync(accessDb, dbPerson),
            "AGENDA" =>
                await BuildAgendaContextAsync(accessDb, dbPerson),
            "EMOCION" =>
                await BuildSupportContentContextAsync(accessDb, dbPerson),
            _ => new DatabaseContextResult { Found = false }
        };
    }

    private static async Task<DbPerson?> FindMatchingPersonAsync(
        AccessDb accessDb,
        string resolvedName,
        string recognizedWord)
    {
        var personRepository = new PersonRepository(accessDb);
        var aliasRepository = new PersonAliasRepository(accessDb);

        var people = (await personRepository.GetAllAsync()).ToList();

        DbPerson? byName = people.FirstOrDefault(p =>
            p.Name.Equals(resolvedName, StringComparison.OrdinalIgnoreCase));

        if (byName != null)
            return byName;

        var aliases = await aliasRepository.GetAllAsync();

        Guid? aliasedPersonId = aliases
            .Where(a => a.Alias.Equals(recognizedWord, StringComparison.OrdinalIgnoreCase))
            .Select(a => (Guid?)a.PersonId)
            .FirstOrDefault();

        return aliasedPersonId == null
            ? null
            : people.FirstOrDefault(p => p.Id == aliasedPersonId);
    }

    private static async Task<DatabaseContextResult> BuildLifeEventContextAsync(
        AccessDb accessDb,
        DbPerson person)
    {
        var repository = new LifeEventRepository(accessDb);

        LifeEvent? latest = (await repository.GetByPersonAsync(person.Id))
            .OrderByDescending(e => e.StartDate)
            .FirstOrDefault();

        if (latest == null)
            return new DatabaseContextResult { Found = false };

        string place = string.IsNullOrWhiteSpace(latest.Place)
            ? "un lugar sin especificar"
            : latest.Place;

        return new DatabaseContextResult
        {
            Found = true,
            Summary =
                $"Según el registro, la última vez estuvo en {place} ({latest.Title})."
        };
    }

    private static async Task<DatabaseContextResult> BuildAgendaContextAsync(
        AccessDb accessDb,
        DbPerson person)
    {
        var repository = new PersonAgendaItemRepository(accessDb);

        PersonAgendaItem? next = (await repository.GetByPersonAsync(
            person.Id,
            includePast: false,
            now: DateTimeOffset.Now))
            .FirstOrDefault();

        if (next == null)
            return new DatabaseContextResult { Found = false };

        string when = next.ScheduledAt.ToString(
            "dd/MM HH:mm",
            CultureInfo.GetCultureInfo("es-AR"));

        return new DatabaseContextResult
        {
            Found = true,
            Summary = $"El próximo evento en la agenda es \"{next.Title}\" el {when}."
        };
    }

    private static async Task<DatabaseContextResult> BuildSupportContentContextAsync(
        AccessDb accessDb,
        DbPerson person)
    {
        var repository = new PersonSupportContentRepository(accessDb);

        PersonSupportContent? content = (await repository.GetActiveByPersonAsync(person.Id))
            .FirstOrDefault();

        if (content == null)
            return new DatabaseContextResult { Found = false };

        return new DatabaseContextResult
        {
            Found = true,
            Summary = $"Recordá esto: \"{content.Content}\""
        };
    }
}
