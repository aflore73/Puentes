using System;
using System.Collections.Generic;
using System.Linq;
using Puentes.LocalInterpreter.Models;

namespace Puentes.LocalInterpreter.Context;

public static class PeopleContext
{
    public static readonly List<Person> People = new()
    {
        new Person
        {
            Name = "Ezequiel",
            Relation = "hijo",
            Aliases = new[]
            {
                "ezequiel",
                "eze",
                "ezequi",
                "sequi"
            }
        },

        new Person
        {
            Name = "Alejandro",
            Relation = "hijo",
            Aliases = new[]
            {
                "alejandro",
                "ale",
                "alejo"
            }
        },

        new Person
        {
            Name = "Laura",
            Relation = "hija",
            Aliases = new[]
            {
                "laura"
            }
        },

        new Person
        {
            Name = "Marta",
            Relation = "hermana",
            Aliases = new[]
            {
                "marta"
            }
        }
    };

    public static Person? GetContextualPerson(string relation)
    {
        if (relation.Equals(
            "hijo",
            StringComparison.OrdinalIgnoreCase))
        {
            return People.FirstOrDefault(
                p => p.Name.Equals(
                    "Ezequiel",
                    StringComparison.OrdinalIgnoreCase));
        }

        if (relation.Equals(
            "hija",
            StringComparison.OrdinalIgnoreCase))
        {
            return People.FirstOrDefault(
                p => p.Name.Equals(
                    "Laura",
                    StringComparison.OrdinalIgnoreCase));
        }

        if (relation.Equals(
            "hermana",
            StringComparison.OrdinalIgnoreCase))
        {
            return People.FirstOrDefault(
                p => p.Name.Equals(
                    "Marta",
                    StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }
}