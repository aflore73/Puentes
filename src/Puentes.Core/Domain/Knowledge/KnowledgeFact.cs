namespace Puentes.Shared.Domain.Knowledge;

public abstract class KnowledgeFact
{
    public Guid Id { get; set; }

    public string Topic { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int Priority { get; set; }

    public List<string> Keywords { get; set; } = [];
}