using System;
using System.Text.RegularExpressions;
using Puentes.LocalInterpreter.Models;

namespace Puentes.LocalInterpreter.Services;

public static class IntentDetector
{
    public static IntentResult Detect(string text)
    {
        // UBICACIÓN
        if (ContainsAny(
            text,
            "dónde",
            "donde",
            "ubicación",
            "ubicacion"))
        {
            return new IntentResult
            {
                Intent = "UBICACION",
                Score = 100,
                Evidence = "detectó 'dónde'"
            };
        }

        // PERSONA
        if (ContainsAny(
            text,
            "quién es",
            "quien es",
            "quién",
            "quien"))
        {
            return new IntentResult
            {
                Intent = "PERSONA",
                Score = 95,
                Evidence = "detectó pregunta sobre identidad"
            };
        }

        // EMOCIÓN
        if (ContainsAny(
            text,
            "estoy triste",
            "triste",
            "me siento sola",
            "me siento solo",
            "sola",
            "solo",
            "angustiada",
            "angustiado",
            "preocupada",
            "preocupado",
            "feliz",
            "contenta",
            "contento",
            "enojada",
            "enojado"))
        {
            return new IntentResult
            {
                Intent = "EMOCION",
                Score = 90,
                Evidence = "detectó indicador emocional"
            };
        }

        // NECESIDAD
        if (ContainsAny(
            text,
            "quiero hablar",
            "necesito hablar",
            "quiero ver",
            "necesito ver",
            "quiero llamar",
            "necesito llamar",
            "necesito",
            "quiero"))
        {
            return new IntentResult
            {
                Intent = "NECESIDAD",
                Score = 85,
                Evidence = "detectó necesidad"
            };
        }

        // AGENDA
        if (ContainsAny(
            text,
            "cuándo",
            "cuando",
            "hoy",
            "ayer",
            "mañana",
            "pasado mañana",
            "esta tarde",
            "esta noche",
            "semana",
            "lunes",
            "martes",
            "miércoles",
            "miercoles",
            "jueves",
            "viernes",
            "sábado",
            "sabado",
            "domingo"))
        {
            return new IntentResult
            {
                Intent = "AGENDA",
                Score = 90,
                Evidence = "detectó indicador temporal"
            };
        }

        // EVENTO
        if (ContainsAny(
            text,
            "vino",
            "viene",
            "venía",
            "venia",
            "llegó",
            "llego",
            "llega",
            "está",
            "esta",
            "estuvo",
            "fue",
            "va",
            "salió",
            "salio",
            "sale",
            "llegaron",
            "vinieron"))
        {
            return new IntentResult
            {
                Intent = "EVENTO",
                Score = 80,
                Evidence = "detectó verbo o evento"
            };
        }

        return new IntentResult
        {
            Intent = "DESCONOCIDA",
            Score = 0,
            Evidence = "no se detectó una intención conocida"
        };
    }

    private static bool ContainsAny(
        string text,
        params string[] values)
    {
        foreach (string value in values)
        {
            if (ContainsWholeWord(text, value))
                return true;
        }

        return false;
    }

    private static bool ContainsWholeWord(
        string text,
        string word)
    {
        if (string.IsNullOrWhiteSpace(text) ||
            string.IsNullOrWhiteSpace(word))
        {
            return false;
        }

        string pattern =
            $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(word)}(?![\p{{L}}\p{{N}}])";

        return Regex.IsMatch(
            text,
            pattern,
            RegexOptions.IgnoreCase);
    }
}