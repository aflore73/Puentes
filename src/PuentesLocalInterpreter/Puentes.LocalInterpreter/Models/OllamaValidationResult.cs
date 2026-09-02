using System.Text.Json.Serialization;

namespace Puentes.LocalInterpreter.Models;

public sealed class OllamaValidationResult
{
    [JsonPropertyName("valid")]
    public bool IsValid { get; init; }

    [JsonPropertyName("text")]
    public string Text { get; init; } = "";

    [JsonPropertyName("reason")]
    public string Reason { get; init; } = "";
}
