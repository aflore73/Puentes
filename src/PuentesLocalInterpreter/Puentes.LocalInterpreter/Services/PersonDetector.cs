using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Puentes.LocalInterpreter.Context;
using Puentes.LocalInterpreter.Models;

namespace Puentes.LocalInterpreter.Services;

public static class PersonDetector
{
    public static PersonResult Detect(string text)
    {
        Person? bestPerson = null;
        string bestWord = "";
        int bestScore = 0;

        // PERSONA EXPLÍCITA
        foreach (Person person in PeopleContext.People)
        {
            foreach (string alias in person.Aliases)
            {
                if (!ContainsWholeWord(text, alias))
                    continue;

                int score = 100;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPerson = person;
                    bestWord = alias;
                }
            }
        }

        if (bestPerson != null)
        {
            return new PersonResult
            {
                Person = bestPerson,
                Relation = bestPerson.Relation,
                Word = bestWord,
                Score = bestScore,
                State = "Confirmado"
            };
        }

        // RELACIÓN FAMILIAR
        string? relation = DetectRelation(text);

        if (relation == null)
        {
            return new PersonResult
            {
                Person = null,
                Relation = "",
                Word = "",
                Score = 0,
                State = "Desconocido"
            };
        }

        List<Person> candidates = PeopleContext.People
            .Where(p =>
                p.Relation.Equals(
                    relation,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Un único candidato
        if (candidates.Count == 1)
        {
            return CreateContextResult(
                candidates[0],
                relation);
        }

        // Contexto familiar preferente
        Person? contextualPerson =
            PeopleContext.GetContextualPerson(relation);

        if (contextualPerson != null)
        {
            return CreateContextResult(
                contextualPerson,
                relation);
        }

        // Ambiguo
        return new PersonResult
        {
            Person = null,
            Relation = relation,
            Word = relation,
            Score = 100,
            State = "Ambiguo"
        };
    }

    private static PersonResult CreateContextResult(
        Person person,
        string relation)
    {
        return new PersonResult
        {
            Person = person,
            Relation = relation,
            Word = relation,
            Score = 100,
            State = "Contexto"
        };
    }

    private static string? DetectRelation(string text)
    {
        string[] relations =
        {
            "hijo",
            "hija",
            "hermano",
            "hermana",
            "padre",
            "madre",
            "esposo",
            "esposa",
            "marido",
            "mujer"
        };

        foreach (string relation in relations)
        {
            if (ContainsWholeWord(text, relation))
                return relation;
        }

        return null;
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