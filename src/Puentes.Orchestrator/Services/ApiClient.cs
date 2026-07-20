using Puentes.Shared.Responses;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public async Task<List<MedicationPlanResponse>> GetMedicationPlanAsync()
    {
        var response = await _httpClient.GetAsync("medication-plan");

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<MedicationPlanResponse>>(
            json,
            JsonOptions
        ) ?? [];
    }
}