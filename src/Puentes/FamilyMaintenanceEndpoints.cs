using Puentes.Infrastructure.Repositories;
using Puentes.Shared.Domain;
using Puentes.Shared.Requests.People;

namespace Puentes.Api;

public static class FamilyMaintenanceEndpoints
{
    public static void MapFamilyMaintenanceEndpoints(this WebApplication app)
    {
        app.MapPost("/people/{personId:guid}/routines", AddRoutineAsync);
        app.MapPut("/people/{personId:guid}/routines/{id:guid}", UpdateRoutineAsync);
        app.MapPost("/people/{personId:guid}/preferences", AddPreferenceAsync);
        app.MapPut("/people/{personId:guid}/preferences/{id:guid}", UpdatePreferenceAsync);
        app.MapPost("/people/{personId:guid}/support-contents", AddSupportContentAsync);
        app.MapPut("/people/{personId:guid}/support-contents/{id:guid}", UpdateSupportContentAsync);
        app.MapPost("/people/{personId:guid}/belongings", AddBelongingAsync);
        app.MapPut("/people/{personId:guid}/belongings/{id:guid}", UpdateBelongingAsync);
        app.MapPost("/people/{personId:guid}/trusted-contacts", AddTrustedContactAsync);
        app.MapPut("/people/{personId:guid}/trusted-contacts/{id:guid}", UpdateTrustedContactAsync);
    }

    private static async Task<IResult> AddRoutineAsync(
        Guid personId, PersonRoutineRequest request,
        PersonRepository people, PersonRoutineRepository repository)
    {
        if (await people.GetByIdAsync(personId) is null) return Results.NotFound();
        if (Missing(request.Title, request.Notes)) return Results.BadRequest();
        var item = new PersonRoutine
        {
            Id = Guid.NewGuid(), PersonId = personId,
            Title = request.Title.Trim(), Notes = request.Notes.Trim(),
            IsActive = request.IsActive
        };
        await repository.AddAsync(item);
        return Results.Created($"/people/{personId}/routines/{item.Id}", item);
    }

    private static async Task<IResult> UpdateRoutineAsync(
        Guid personId, Guid id, PersonRoutineRequest request,
        PersonRoutineRepository repository)
    {
        if (Missing(request.Title, request.Notes)) return Results.BadRequest();
        var item = new PersonRoutine
        {
            Id = id, PersonId = personId, Title = request.Title.Trim(),
            Notes = request.Notes.Trim(), IsActive = request.IsActive
        };
        return await repository.UpdateAsync(item)
            ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> AddPreferenceAsync(
        Guid personId, PersonPreferenceRequest request,
        PersonRepository people, PersonPreferenceRepository repository)
    {
        if (await people.GetByIdAsync(personId) is null) return Results.NotFound();
        if (Missing(request.Title, request.Notes)) return Results.BadRequest();
        var item = Preference(Guid.NewGuid(), personId, request);
        await repository.AddAsync(item);
        return Results.Created($"/people/{personId}/preferences/{item.Id}", item);
    }

    private static async Task<IResult> UpdatePreferenceAsync(
        Guid personId, Guid id, PersonPreferenceRequest request,
        PersonPreferenceRepository repository)
    {
        if (Missing(request.Title, request.Notes)) return Results.BadRequest();
        return await repository.UpdateAsync(Preference(id, personId, request))
            ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> AddSupportContentAsync(
        Guid personId, PersonSupportContentRequest request,
        PersonRepository people, PersonSupportContentRepository repository)
    {
        if (await people.GetByIdAsync(personId) is null) return Results.NotFound();
        if (Missing(request.Title, request.Content)) return Results.BadRequest();
        var item = SupportContent(Guid.NewGuid(), personId, request);
        await repository.AddAsync(item);
        return Results.Created(
            $"/people/{personId}/support-contents/{item.Id}", item);
    }

    private static async Task<IResult> UpdateSupportContentAsync(
        Guid personId, Guid id, PersonSupportContentRequest request,
        PersonSupportContentRepository repository)
    {
        if (Missing(request.Title, request.Content)) return Results.BadRequest();
        return await repository.UpdateAsync(SupportContent(id, personId, request))
            ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> AddBelongingAsync(
        Guid personId, PersonBelongingRequest request,
        PersonRepository people, PersonBelongingRepository repository)
    {
        if (await people.GetByIdAsync(personId) is null) return Results.NotFound();
        if (Missing(request.Name, request.Notes)) return Results.BadRequest();
        var item = Belonging(Guid.NewGuid(), personId, request);
        await repository.AddAsync(item);
        return Results.Created($"/people/{personId}/belongings/{item.Id}", item);
    }

    private static async Task<IResult> UpdateBelongingAsync(
        Guid personId, Guid id, PersonBelongingRequest request,
        PersonBelongingRepository repository)
    {
        if (Missing(request.Name, request.Notes)) return Results.BadRequest();
        return await repository.UpdateAsync(Belonging(id, personId, request))
            ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> AddTrustedContactAsync(
        Guid personId, PersonTrustedContactRequest request,
        PersonRepository people, PersonTrustedContactRepository repository)
    {
        if (request.ContactPersonId == personId || request.Priority < 1 ||
            await people.GetByIdAsync(personId) is null ||
            await people.GetByIdAsync(request.ContactPersonId) is null)
        {
            return Results.BadRequest();
        }
        var item = TrustedContact(Guid.NewGuid(), personId, request);
        await repository.AddAsync(item);
        return Results.Created(
            $"/people/{personId}/trusted-contacts/{item.Id}", item);
    }

    private static async Task<IResult> UpdateTrustedContactAsync(
        Guid personId, Guid id, PersonTrustedContactRequest request,
        PersonTrustedContactRepository repository)
    {
        if (request.ContactPersonId == personId || request.Priority < 1)
            return Results.BadRequest();
        return await repository.UpdateAsync(TrustedContact(id, personId, request))
            ? Results.NoContent() : Results.NotFound();
    }

    private static bool Missing(string first, string second) =>
        string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second);

    private static PersonPreference Preference(
        Guid id, Guid personId, PersonPreferenceRequest request) => new()
        {
            Id = id, PersonId = personId, Title = request.Title.Trim(),
            Notes = request.Notes.Trim(), Tags = request.Tags?.Trim(),
            IsActive = request.IsActive
        };

    private static PersonSupportContent SupportContent(
        Guid id, Guid personId, PersonSupportContentRequest request) => new()
        {
            Id = id, PersonId = personId, Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            Attribution = request.Attribution?.Trim(),
            Reference = request.Reference?.Trim(), Tags = request.Tags?.Trim(),
            IsActive = request.IsActive
        };

    private static PersonBelonging Belonging(
        Guid id, Guid personId, PersonBelongingRequest request) => new()
        {
            Id = id, PersonId = personId, Name = request.Name.Trim(),
            Notes = request.Notes.Trim(), Tags = request.Tags?.Trim(),
            IsActive = request.IsActive
        };

    private static PersonTrustedContact TrustedContact(
        Guid id, Guid personId, PersonTrustedContactRequest request) => new()
        {
            Id = id, PersonId = personId,
            ContactPersonId = request.ContactPersonId,
            Priority = request.Priority, Notes = request.Notes?.Trim(),
            IsActive = request.IsActive
        };
}
