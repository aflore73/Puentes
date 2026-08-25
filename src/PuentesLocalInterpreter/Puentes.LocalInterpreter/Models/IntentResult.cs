namespace Puentes.LocalInterpreter.Models;

public sealed class IntentResult
{
    public string Intent { get; init; } = "DESCONOCIDA";
    public int Score { get; init; }
    public string Evidence { get; init; } = "";
}