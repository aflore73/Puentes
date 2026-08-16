namespace Puentes.Shared.Requests.People;

public sealed class PersonTrustedContactRequest
{
    public Guid ContactPersonId { get; set; }
    public int Priority { get; set; } = 1;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
