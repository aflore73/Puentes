namespace Puentes.Shared.Domain;

public sealed class PersonTrustedContact
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public Guid ContactPersonId { get; set; }
    public int Priority { get; set; } = 1;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
