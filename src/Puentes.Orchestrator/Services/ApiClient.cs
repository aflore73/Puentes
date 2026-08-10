using Puentes.Shared.Responses;
using System.Text.Json;
using System.Text.Json.Serialization;
using Puentes.Shared.Domain;
using Puentes.Shared.Enums;
using System.Net;
using System.Net.Http.Json;
using Puentes.Shared.Responses.People;
using Puentes.Shared.Responses.LifeEvents;

namespace Puentes.Orchestrator.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
    public async Task<List<MedicationPlanResponse>> GetMedicationPlanAsync(
     CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            "medication-plan",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(
            cancellationToken);
        // Console.WriteLine(json); // Diagnostico: respuesta JSON completa.
        return JsonSerializer.Deserialize<List<MedicationPlanResponse>>(
            json,
            JsonOptions
        ) ?? throw new InvalidOperationException(
            "No se pudo deserializar el plan de medicación.");
    }
    public async Task<MedicationRecord?> GetTodayMedicationRecordAsync(
        Guid patientId,
        MedicationTurnType turn,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"patients/{patientId}/medication-records/today/{turn}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MedicationRecord>(
            JsonOptions,
            cancellationToken);
    }

    public async Task<bool> RegisterMedicationTakenAsync(
        Guid patientId,
        MedicationTurnType turn,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var request = new RegisterMedicationRecordRequest
        {
            PatientId = patientId,
            Turn = turn,
            Confirmed = true,
            Notes = notes
        };

        var response = await _httpClient.PostAsJsonAsync(
            "medication-records",
            request,
            JsonOptions,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<Person?> GetPersonAsync(
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"people/{personId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Person>(
            JsonOptions,
            cancellationToken);
    }

    public async Task<List<PersonConnectionResponse>>
        GetPersonRelationshipsAsync(
            Guid personId,
            CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"people/{personId}/relationships",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<PersonConnectionResponse>>(
                JsonOptions,
                cancellationToken) ?? [];
    }

    public async Task<List<LifeEventResponse>> GetPersonLifeEventsAsync(
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"people/{personId}/life-events",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<LifeEventResponse>>(
                JsonOptions,
                cancellationToken) ?? [];
    }

    public async Task<List<PersonRoutineResponse>> GetPersonRoutinesAsync(
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"people/{personId}/routines",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<PersonRoutineResponse>>(
                JsonOptions,
                cancellationToken) ?? [];
    }

    public async Task<List<PersonPreferenceResponse>> GetPersonPreferencesAsync(
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"people/{personId}/preferences",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<PersonPreferenceResponse>>(
                JsonOptions,
                cancellationToken) ?? [];
    }
}
