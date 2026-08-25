using System;
using System.Text.RegularExpressions;

namespace Puentes.LocalInterpreter.Services;

public static class TextNormalizer
{
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        string text = input.Trim().ToLowerInvariant();

        text = text.Replace("...", " ");
        text = text.Replace("…", " ");

        text = ReplaceWholeWord(text, "ta", "está");
        text = ReplaceWholeWord(text, "ijo", "hijo");
        text = ReplaceWholeWord(text, "manana", "mañana");

        text = Regex.Replace(
            text,
            @"\s+",
            " ")
            .Trim();

        return text;
    }

    private static string ReplaceWholeWord(
        string text,
        string oldValue,
        string newValue)
    {
        string pattern =
            $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(oldValue)}(?![\p{{L}}\p{{N}}])";

        return Regex.Replace(
            text,
            pattern,
            newValue,
            RegexOptions.IgnoreCase);
    }
}