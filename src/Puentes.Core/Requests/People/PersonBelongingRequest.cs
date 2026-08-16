namespace Puentes.Shared.Requests.People;

public sealed class PersonBelongingRequest
{
    public string Name { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public bool IsActive { get; set; } = true;
}
