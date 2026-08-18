namespace Puentes.Shared.Responses.People;

public class PersonPreferenceResponse
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public string PersonName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string? Tags { get; set; }

    public List<string> TopicCodes { get; set; } = [];

    public bool IsActive { get; set; }
}
