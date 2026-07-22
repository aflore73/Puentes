using Puentes.Shared.Responses;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        Console.WriteLine(json);
        return JsonSerializer.Deserialize<List<MedicationPlanResponse>>(
            json,
            JsonOptions
        ) ?? throw new InvalidOperationException(
            "No se pudo deserializar el plan de medicación.");
    }
}