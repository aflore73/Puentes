namespace Puentes.LocalInterpreter.Models;

public sealed class InterpretationResult
{
    public string Original { get; init; } = "";
    public string Normalized { get; init; } = "";
    public PersonResult Person { get; init; } = new();
    public IntentResult Intent { get; init; } = new();
    public string Origin { get; init; } = "LOCAL";
    public string Confidence { get; init; } = "BAJA";
    public string Reason { get; init; } = "";
    public string Interpretation { get; init; } = "";
}