namespace Puentes.Shared.Requests.People;

public sealed class PersonPreferenceRequest
{
    public string Title { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public bool IsActive { get; set; } = true;
}
