using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Puentes.Orchestrator.Services;

public class MedicationConfirmationService
{
    private static readonly Regex NonLetterOrDigit = new(
        "[^a-z0-9]+",
        RegexOptions.Compiled);

    private static readonly string[] RejectionExpressions =
    [
        "no", "todavia no", "aun no", "sin tomar",
        "me falta", "me faltan", "falta tomar", "faltan tomar",
        "despues", "mas tarde", "ahora la tomo", "ahora las tomo",
        "voy a tomar", "tengo que tomar", "debo tomar",
        "creo", "me parece", "quizas", "tal vez", "puede ser",
        "no recuerdo", "no me acuerdo", "no estoy segura",
        "no estoy seguro"
    ];

    private static readonly string[] ConfirmationExpressions =
    [
        "si", "sip", "claro", "ya esta", "ya estan",
        "listo", "lista", "hecho", "cumplido",
        "ya la tome", "ya las tome", "ya lo tome", "ya tome",
        "me la tome", "me las tome", "me lo tome", "me los tome",
        "la tome", "las tome", "lo tome", "los tome",
        "tome todo", "tome todos", "tome toda", "tome todas",
        "me tome todo", "me tome todos", "me tome toda", "me tome todas",
        "ya he tomado", "he tomado todo", "he tomado todos",
        "he tomado toda", "he tomado todas",
        "ya termine", "termine de tomar", "ya termine de tomar",
        "estan tomadas", "ya estan tomadas"
    ];

    public bool IsExplicitConfirmation(string? userInput)
    {
        var normalized = Normalize(userInput);

        if (string.IsNullOrEmpty(normalized))
        {
            return false;
        }

        if (userInput!.Contains('?', StringComparison.Ordinal)
            || RejectionExpressions.Any(expression =>
            ContainsExpression(normalized, expression)))
        {
            return false;
        }

        return ConfirmationExpressions.Any(expression =>
            ContainsExpression(normalized, expression));
    }

    private static bool ContainsExpression(string input, string expression) =>
        input == expression
        || input.StartsWith($"{expression} ", StringComparison.Ordinal)
        || input.EndsWith($" {expression}", StringComparison.Ordinal)
        || input.Contains($" {expression} ", StringComparison.Ordinal);

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character)
                != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return NonLetterOrDigit.Replace(builder.ToString(), " ").Trim();
    }
}
