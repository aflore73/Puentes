using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Puentes.LocalInterpreter.Context;
using Whisper.net;
using Whisper.net.Ggml;

namespace Puentes.LocalInterpreter.Voice;

/// <summary>Transcribes recorded audio to Spanish text using a local Whisper.net model.</summary>
public static class SpeechTranscriber
{
    private const string ModelFileName = "ggml-base.bin";
    private const GgmlType ModelType = GgmlType.Base;

    public static async Task<string> TranscribeAsync(Stream wavAudio)
    {
        await EnsureModelDownloadedAsync();

        using WhisperFactory whisperFactory =
            WhisperFactory.FromPath(ModelFileName);

        using WhisperProcessor processor = whisperFactory
            .CreateBuilder()
            .WithLanguage("es")
            .WithPrompt(BuildKnownNamesPrompt())
            .Build();

        var transcript = new StringBuilder();

        await foreach (SegmentData segment in processor.ProcessAsync(wavAudio))
        {
            transcript.Append(segment.Text);
        }

        return transcript.ToString().Trim();
    }

    // Biases Whisper towards the known family names/relations so it stops mishearing them.
    private static string BuildKnownNamesPrompt()
    {
        string names = string.Join(
            ", ",
            PeopleContext.People.Select(p => p.Name));

        return $"Conversación en español sobre la familia: {names}.";
    }

    private static async Task EnsureModelDownloadedAsync()
    {
        if (File.Exists(ModelFileName))
            return;

        Console.WriteLine(
            $"Descargando modelo de voz ({ModelType})... esto solo pasa la primera vez.");

        using Stream modelStream =
            await new WhisperGgmlDownloader(new HttpClient()).GetGgmlModelAsync(ModelType);

        using FileStream fileWriter =
            File.OpenWrite(ModelFileName);

        await modelStream.CopyToAsync(fileWriter);
    }
}
