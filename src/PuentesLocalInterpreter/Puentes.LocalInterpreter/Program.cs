using System;
using System.Text;

using Puentes.LocalInterpreter.Output;
using Puentes.LocalInterpreter.Services;

namespace Puentes.LocalInterpreter
{
    internal class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("========================================");
            Console.WriteLine("PUENTES - INTERPRETE LOCAL");
            Console.WriteLine("========================================");
            Console.WriteLine();

            RunExamples();

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("MODO INTERACTIVO");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("Escribí una frase.");
            Console.WriteLine("Escribí SALIR para terminar.");
            Console.WriteLine();

            while (true)
            {
                Console.Write(">>> ");

                string? input = Console.ReadLine();

                if (input == null)
                    break;

                if (input.Trim().Equals(
                    "salir",
                    StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                Console.WriteLine();

                var result =
                    Interpreter.Interpret(input);

                ConsolePrinter.Print(result);

                Console.WriteLine();
            }
        }

        private static void RunExamples()
        {
            string[] examples =
            {
                "Eze... dónde ta mi ijo",
                "Eze... vino hoy",
                "Eze... cuándo viene",
                "Eze dónde está",
                "Ezequi vino",
                "Sequi dónde está",
                "dónde ta mi ijo",
                "mi hijo vino hoy",
                "cuándo viene mi hijo",
                "mi hija viene mañana",
                "mi hermana está acá",
                "Ale... dónde está",
                "Ale vino ayer",
                "Alejo está acá",
                "Laura viene hoy",
                "dónde está Laura",
                "cuándo viene Laura",
                "quién es Marta",
                "estoy triste",
                "me siento sola",
                "quiero hablar con Eze",
                "Pedro dónde está"
            };

            foreach (string example in examples)
            {
                var result =
                    Interpreter.Interpret(example);

                ConsolePrinter.Print(result);
            }
        }
    }
}