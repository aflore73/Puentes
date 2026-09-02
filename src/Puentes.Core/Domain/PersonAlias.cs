namespace Puentes.Shared.Domain;

public sealed class PersonAlias
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public string Alias { get; set; } = string.Empty;
}
