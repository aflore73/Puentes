namespace Puentes.Core.Domain;

public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
}
