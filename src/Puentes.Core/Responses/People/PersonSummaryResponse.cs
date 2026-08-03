namespace Puentes.Shared.Responses.People;

public class PersonSummaryResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    public string City { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;
}
