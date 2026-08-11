namespace Puentes.Shared.Responses.People;

public class PersonSupportContentResponse
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public string PersonName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? Attribution { get; set; }

    public string? Reference { get; set; }

    public string? Tags { get; set; }

    public bool IsActive { get; set; }
}
