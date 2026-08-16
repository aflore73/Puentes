namespace Puentes.Shared.Responses.People;

public sealed class PersonTrustedContactResponse
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public Guid ContactPersonId { get; set; }
    public string ContactPersonName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}
