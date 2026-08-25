using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Puentes.LocalInterpreter.Models;

namespace Puentes.LocalInterpreter.Services;

public static class Interpreter
{
    public static InterpretationResult Interpret(string input)
    {
        string normalized =
            TextNormalizer.Normalize(input);

        PersonResult person =
            PersonDetector.Detect(normalized);

        IntentResult intent =
            IntentDetector.Detect(normalized);

        bool hasKnownPerson =
            person.Person != null;

        string confidence;
        string reason;

        if (hasKnownPerson && intent.Score > 0)
        {
            confidence = "ALTA";

            reason = person.State == "Contexto"
                ? "Persona resuelta por contexto e intención identificada localmente."
                : "Persona e intención identificadas localmente.";
        }
        else if (intent.Score > 0)
        {
            confidence = "MEDIA";

            reason =
                "La intención fue determinada localmente, pero la persona no pertenece al contexto conocido.";
        }
        else if (person.Person != null)
        {
            confidence = "MEDIA";

            reason =
                "Persona identificada localmente, pero la intención no pudo determinarse con suficiente confianza.";
        }
        else
        {
            confidence = "BAJA";

            reason =
                "No se pudo identificar persona ni intención con suficiente confianza.";
        }

        return new InterpretationResult
        {
            Original = input,
            Normalized = normalized,
            Person = person,
            Intent = intent,
            Origin = "LOCAL",
            Confidence = confidence,
            Reason = reason,
            Interpretation =
                BuildInterpretation(
                    normalized,
                    person,
                    intent)
        };
    }

    private static string BuildInterpretation(
        string text,
        PersonResult person,
        IntentResult intent)
    {
        string? personName =
            person.Person?.Name;

        switch (intent.Intent)
        {
            case "UBICACION":

                if (!string.IsNullOrEmpty(personName))
                    return $"¿Dónde está {personName}?";

                string unknownPerson =
                    ExtractLocationSubject(text);

                if (!string.IsNullOrEmpty(unknownPerson))
                    return $"¿Dónde está {unknownPerson}?";

                if (person.Relation.Equals(
                    "hijo",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "¿Dónde está mi hijo?";
                }

                if (person.Relation.Equals(
                    "hija",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "¿Dónde está mi hija?";
                }

                if (person.Relation.Equals(
                    "hermana",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "¿Dónde está mi hermana?";
                }

                return "¿Dónde está?";

            case "AGENDA":
            case "EVENTO":
            case "NECESIDAD":

                if (!string.IsNullOrEmpty(personName))
                {
                    return ReplacePersonReference(
                        text,
                        person,
                        personName);
                }

                return CapitalizeFirst(text);

            case "PERSONA":

                if (!string.IsNullOrEmpty(personName))
                    return $"¿Quién es {personName}?";

                return CapitalizeFirst(text);

            case "EMOCION":
                return CapitalizeFirst(text);

            default:
                return CapitalizeFirst(text);
        }
    }

    private static string ReplacePersonReference(
        string text,
        PersonResult person,
        string personName)
    {
        if (person.Person == null)
            return CapitalizeFirst(text);

        string result = text;

        foreach (string alias in person.Person.Aliases
                     .OrderByDescending(x => x.Length))
        {
            result = ReplaceWholeWord(
                result,
                alias,
                personName);
        }

        if (!string.IsNullOrEmpty(person.Relation))
        {
            result = ReplaceWholeWord(
                result,
                person.Relation,
                personName);
        }

        // mi hijo -> Ezequiel
        // mi hija -> Laura
        // mi hermana -> Marta
        result = Regex.Replace(
            result,
            $@"\bmi\s+{Regex.Escape(personName)}\b",
            personName,
            RegexOptions.IgnoreCase);

        return CapitalizeFirst(result);
    }

    private static string ExtractLocationSubject(
        string text)
    {
        Match match = Regex.Match(
            text,
            @"\b(?:dónde|donde)\b",
            RegexOptions.IgnoreCase);

        if (!match.Success)
            return "";

        string before =
            text.Substring(0, match.Index).Trim();

        if (string.IsNullOrWhiteSpace(before))
            return "";

        string[] words =
            before.Split(
                new[] { ' ', '\t', '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
            return "";

        string candidate =
            words[^1];

        string[] ignored =
        {
            "mi",
            "el",
            "la",
            "los",
            "las",
            "un",
            "una",
            "hijo",
            "hija",
            "hermano",
            "hermana"
        };

        if (ignored.Contains(
            candidate,
            StringComparer.OrdinalIgnoreCase))
        {
            return "";
        }

        return CapitalizeFirst(candidate);
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

    private static string CapitalizeFirst(
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        text = text.Trim();

        if (text.Length == 1)
            return text.ToUpperInvariant();

        return char.ToUpper(
                   text[0],
                   CultureInfo.GetCultureInfo("es-AR"))
               + text.Substring(1);
    }
}