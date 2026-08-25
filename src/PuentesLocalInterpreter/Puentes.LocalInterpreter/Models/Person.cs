namespace Puentes.LocalInterpreter.Models;

public sealed class Person
{
    public string Name { get; init; } = "";
    public string Relation { get; init; } = "";
    public string[] Aliases { get; init; } = Array.Empty<string>();
}