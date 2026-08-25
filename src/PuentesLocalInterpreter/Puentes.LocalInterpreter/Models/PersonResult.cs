namespace Puentes.LocalInterpreter.Models;

public sealed class PersonResult
{
    public Person? Person { get; init; }
    public string Relation { get; init; } = "";
    public string Word { get; init; } = "";
    public int Score { get; init; }
    public string State { get; init; } = "Desconocido";
}