namespace Puentes.Shared.Requests.People;

public sealed class PersonRoutineRequest
{
    public string Title { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
