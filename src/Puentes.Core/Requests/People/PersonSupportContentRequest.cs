namespace Puentes.Shared.Requests.People;

public sealed class PersonSupportContentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Attribution { get; set; }
    public string? Reference { get; set; }
    public string? Tags { get; set; }
    public bool IsActive { get; set; } = true;
}
