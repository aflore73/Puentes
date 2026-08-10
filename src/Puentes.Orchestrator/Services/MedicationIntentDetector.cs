using System.Globalization;
using System.Text;

namespace Puentes.Orchestrator.Services;

public static class MedicationIntentDetector
{
    private static readonly string[] MedicationTerms =
    [
        "medicacion",
        "medicamento",
        "medicamentos",
        "remedio",
        "remedios",
        "pastilla",
        "pastillas",
        "capsula",
        "capsulas"
    ];

    public static bool IsMedicationQuery(string input)
    {
        var words = Normalize(input)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Any(word => MedicationTerms.Contains(
            word,
            StringComparer.Ordinal));
    }

    private static string Normalize(string value)
    {
        var decomposed = value.ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) ==
                UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(character) ? character : ' ');
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
