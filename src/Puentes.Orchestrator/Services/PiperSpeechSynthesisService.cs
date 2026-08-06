using System.Diagnostics;
using System.Globalization;
using NAudio.Wave;
using Puentes.Orchestrator.AI;

namespace Puentes.Orchestrator.Services;

public sealed class PiperSpeechSynthesisService : ISpeechSynthesisService
{
    private readonly PiperOptions _options;
    private readonly string _contentRootPath;

    public PiperSpeechSynthesisService(
        PiperOptions options,
        IHostEnvironment environment)
    {
        _options = options;
        _contentRootPath = environment.ContentRootPath;
    }

    public async Task<GeneratedSpeech> GenerateSpeechAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var modelPath = ResolvePath(_options.ModelPath);
        var configPath = $"{modelPath}.json";

        if (!File.Exists(modelPath) || !File.Exists(configPath))
        {
            throw new InvalidOperationException(
                $"No se encontro el modelo de Piper en '{modelPath}' " +
                "o falta su archivo .onnx.json.");
        }

        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"puentes-{Guid.NewGuid():N}.wav");

        try
        {
            using var process = new Process
            {
                StartInfo = CreateStartInfo(modelPath, outputPath, text)
            };

            try
            {
                process.Start();
            }
            catch (Exception exception) when (
                exception is InvalidOperationException
                or System.ComponentModel.Win32Exception)
            {
                throw new InvalidOperationException(
                    $"No se pudo iniciar Piper con '{_options.ExecutablePath}'.",
                    exception);
            }

            using var cancellationRegistration = cancellationToken.Register(
                () =>
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(entireProcessTree: true);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }
                });
            var standardErrorTask = process.StandardError.ReadToEndAsync(
                cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            var standardError = await standardErrorTask;

            if (process.ExitCode != 0 || !File.Exists(outputPath))
            {
                throw new InvalidOperationException(
                    $"Piper no pudo generar el audio. {standardError}".Trim());
            }

            return new GeneratedSpeech(
                AddSilencePadding(outputPath),
                "audio/wav",
                "puentes-response.wav");
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    private ProcessStartInfo CreateStartInfo(
        string modelPath,
        string outputPath,
        string text)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _options.ExecutablePath,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _contentRootPath
        };

        foreach (var argument in _options.ExecutableArguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        startInfo.ArgumentList.Add("-m");
        startInfo.ArgumentList.Add(modelPath);
        startInfo.ArgumentList.Add("-f");
        startInfo.ArgumentList.Add(outputPath);
        startInfo.ArgumentList.Add("--length-scale");
        startInfo.ArgumentList.Add(_options.LengthScale.ToString(
            CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add("--sentence-silence");
        startInfo.ArgumentList.Add(_options.SentenceSilence.ToString(
            CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add("--");
        startInfo.ArgumentList.Add(text);

        return startInfo;
    }

    private string ResolvePath(string path) => Path.IsPathRooted(path)
        ? path
        : Path.GetFullPath(path, _contentRootPath);

    private byte[] AddSilencePadding(string audioPath)
    {
        using var reader = new WaveFileReader(audioPath);
        using var output = new MemoryStream();
        using (var writer = new WaveFileWriter(output, reader.WaveFormat))
        {
            WriteSilence(writer, _options.StartSilenceMilliseconds);
            reader.CopyTo(writer);
            WriteSilence(writer, _options.EndSilenceMilliseconds);
        }

        return output.ToArray();
    }

    private static void WriteSilence(
        WaveFileWriter writer,
        int milliseconds)
    {
        var byteCount = writer.WaveFormat.AverageBytesPerSecond
            * Math.Max(0, milliseconds) / 1000;
        byteCount -= byteCount % writer.WaveFormat.BlockAlign;
        writer.Write(new byte[byteCount]);
    }
}
