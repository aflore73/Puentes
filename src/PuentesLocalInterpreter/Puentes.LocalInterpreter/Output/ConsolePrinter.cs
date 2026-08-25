using System;
using Puentes.LocalInterpreter.Models;

namespace Puentes.LocalInterpreter.Output;

public static class ConsolePrinter
{
    public static void Print(
        InterpretationResult result)
    {
        Console.WriteLine(
            "----------------------------------------");

        Console.WriteLine(
            $"Texto original : {result.Original}");

        Console.WriteLine(
            $"Texto normaliz.: {result.Normalized}");

        Console.WriteLine();

        Console.WriteLine("PERSONA");

        string personName =
            result.Person.Person?.Name ??
            "DESCONOCIDO";

        string relation =
            string.IsNullOrEmpty(
                result.Person.Relation)
                ? "-"
                : result.Person.Relation;

        string word =
            string.IsNullOrEmpty(
                result.Person.Word)
                ? "-"
                : result.Person.Word;

        Console.WriteLine(
            $"  Nombre    : {personName}");

        Console.WriteLine(
            $"  Relación  : {relation}");

        Console.WriteLine(
            $"  Palabra   : {word}");

        Console.WriteLine(
            $"  Score     : {result.Person.Score}");

        Console.WriteLine(
            $"  Estado    : {result.Person.State}");

        Console.WriteLine();

        Console.WriteLine("INTENCIÓN");

        Console.WriteLine(
            $"  {result.Intent.Intent}");

        Console.WriteLine(
            $"  Score     : {result.Intent.Score}");

        Console.WriteLine(
            $"  Evidencia : {result.Intent.Evidence}");

        Console.WriteLine();

        Console.WriteLine("RESOLUCIÓN");

        Console.WriteLine(
            $"  Origen    : {result.Origin}");

        Console.WriteLine(
            $"  Confianza : {result.Confidence}");

        Console.WriteLine(
            $"  Motivo    : {result.Reason}");

        Console.WriteLine();

        Console.WriteLine("INTERPRETACIÓN");

        Console.WriteLine(
            result.Interpretation);

        Console.WriteLine(
            "----------------------------------------");
    }
}