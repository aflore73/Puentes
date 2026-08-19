namespace Puentes.Shared.Requests.People;

public class CreatePersonRequest
{
    public string Name { get; set; } = string.Empty;

    public DateOnly? BirthDate { get; set; }

    public string City { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;

    public string Country { get; set; } = "Argentina";

    public string? Notes { get; set; }
}
