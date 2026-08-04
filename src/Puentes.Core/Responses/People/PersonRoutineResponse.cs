namespace Puentes.Shared.Responses.People;

public class PersonRoutineResponse
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public string PersonName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
