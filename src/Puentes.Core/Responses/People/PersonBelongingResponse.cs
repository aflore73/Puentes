namespace Puentes.Shared.Responses.People;

public class PersonBelongingResponse
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public string PersonName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string? Tags { get; set; }

    public bool IsActive { get; set; }
}
