using System;
using System.Text;
using System.Threading.Tasks;
using Puentes.LocalInterpreter.Models;
using Puentes.LocalInterpreter.Data;
using Puentes.LocalInterpreter.Output;
using Puentes.LocalInterpreter.Services;
using Puentes.LocalInterpreter.Voice;

namespace Puentes.LocalInterpreter
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            using var ollamaValidator = new OllamaInputValidator();

            if (args.Length > 0 && args[0].Equals("seed-aliases", StringComparison.OrdinalIgnoreCase))
            {
                await RunSeedAliasesScriptAsync();
                return;
            }

            Console.WriteLine("========================================");
            Console.WriteLine("PUENTES - INTERPRETE LOCAL");
            Console.WriteLine("========================================");
            Console.WriteLine();

            await DatabaseConnectionFactory.GetOrCreateAsync();

            await RunExamplesAsync();

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("MODO INTERACTIVO");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("Escribí una frase.");
            Console.WriteLine("Escribí VOZ para hablar por micrófono.");
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

                if (input.Trim().Equals(
                    "voz",
                    StringComparison.OrdinalIgnoreCase))
                {
                    await RunVoiceTurnAsync(ollamaValidator);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                Console.WriteLine();

                OllamaValidationResult validation =
                    await ollamaValidator.ValidateAsync(input);

                if (!validation.IsValid)
                {
                    var localResult = Interpreter.Interpret(input);
                    if (!localResult.Confidence.Equals("ALTA", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(
                            $"No se pudo validar la frase: {validation.Reason}");
                        Console.WriteLine("Probá de nuevo.");
                        Console.WriteLine();
                        continue;
                    }

                    input = localResult.Normalized;
                }

                if (!string.IsNullOrWhiteSpace(validation.Text))
                    input = validation.Text.Trim();

                var result =
                    Interpreter.Interpret(input);

                var dbContext =
                    await DatabaseContextService.EnrichAsync(result.Person, result.Intent);

                ConsolePrinter.Print(result, dbContext);

                Console.WriteLine();
            }
        }

        private static async Task RunSeedAliasesScriptAsync()
        {
            var accessDb = await DatabaseConnectionFactory.GetOrCreateAsync();

            Console.WriteLine("Cargando alias en la base de datos...");

            int inserted = await AliasSeeder.SeedFromLocalContextAsync(accessDb);

            Console.WriteLine(
                inserted == 0
                    ? "No había alias nuevos para cargar."
                    : $"Se cargaron {inserted} alias nuevos.");
        }

        private static async Task RunVoiceTurnAsync(
            OllamaInputValidator ollamaValidator)
        {
            using var audio = MicrophoneRecorder.RecordUntilKeyPress();

            Console.WriteLine("Transcribiendo...");

            string transcript = await SpeechTranscriber.TranscribeAsync(audio);

            if (string.IsNullOrWhiteSpace(transcript))
            {
                Console.WriteLine("No se entendió nada, probá de nuevo.");
                return;
            }

            Console.WriteLine($"Escuché: {transcript}");
            Console.WriteLine();

            Console.WriteLine("Validando transcripción con Ollama...");

            OllamaValidationResult validation =
                await ollamaValidator.ValidateAsync(transcript);

            if (!validation.IsValid)
            {
                var localResult = Interpreter.Interpret(transcript);
                if (!localResult.Confidence.Equals("ALTA", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(
                        $"No se pudo validar la frase: {validation.Reason}");
                    Console.WriteLine("Probá de nuevo.");
                    return;
                }

                transcript = localResult.Normalized;
            }

            if (!string.IsNullOrWhiteSpace(validation.Text))
            {
                transcript = validation.Text.Trim();
                Console.WriteLine($"Texto validado: {transcript}");
                Console.WriteLine();
            }

            var result = Interpreter.Interpret(transcript);

            var dbContext =
                await DatabaseContextService.EnrichAsync(result.Person, result.Intent);

            ConsolePrinter.Print(result, dbContext);
            Console.WriteLine();

            string spokenReply = dbContext is { Found: true }
                ? $"{result.Interpretation} {dbContext.Summary}"
                : result.Interpretation;

            TextToSpeechService.Speak(spokenReply);
        }

        private static async Task RunExamplesAsync()
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

                var dbContext =
                    await DatabaseContextService.EnrichAsync(result.Person, result.Intent);

                ConsolePrinter.Print(result, dbContext);
            }
        }
    }
}