using System.Text;
using NAudio.Wave;
using Puentes.Orchestrator.AI;
using Whisper.net;

namespace Puentes.Orchestrator.Services;

public sealed class WhisperAudioService : IAudioService, IDisposable
{
    private static readonly HashSet<string> SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".wav", ".m4a", ".mp3" };

    private readonly WhisperOptions _options;
    private readonly string _modelPath;
    private readonly SemaphoreSlim _transcriptionLock = new(1, 1);
    private WhisperFactory? _factory;

    public WhisperAudioService(
        WhisperOptions options,
        IHostEnvironment environment)
    {
        _options = options;
        _modelPath = Path.IsPathRooted(options.ModelPath)
            ? options.ModelPath
            : Path.GetFullPath(
                options.ModelPath,
                environment.ContentRootPath);
    }

    public async Task<string> TranscribeAsync(
        Stream audio,
        string filename,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_modelPath))
        {
            throw new InvalidOperationException(
                $"No se encontro el modelo de Whisper en '{_modelPath}'.");
        }

        var extension = Path.GetExtension(filename);
        if (!SupportedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Whisper local admite audio WAV, M4A o MP3.");
        }

        var inputPath = Path.Combine(
            Path.GetTempPath(),
            $"puentes-input-{Guid.NewGuid():N}{extension}");
        try
        {
            await using (var inputFile = File.Create(inputPath))
            {
                await audio.CopyToAsync(inputFile, cancellationToken);
            }

            await _transcriptionLock.WaitAsync(cancellationToken);
            try
            {
                _factory ??= WhisperFactory.FromPath(_modelPath);
                var processorBuilder = _factory.CreateBuilder()
                    .WithLanguage(_options.Language);
                if (!string.IsNullOrWhiteSpace(_options.InitialPrompt))
                {
                    processorBuilder.WithPrompt(_options.InitialPrompt);
                }

                using var processor = processorBuilder.Build();
                var transcript = new StringBuilder();
                await using var normalizedAudio = NormalizeAudio(inputPath);

                await foreach (var segment in processor
                    .ProcessAsync(normalizedAudio)
                    .WithCancellation(cancellationToken))
                {
                    transcript.Append(segment.Text);
                }

                return transcript.ToString().Trim();
            }
            finally
            {
                _transcriptionLock.Release();
            }
        }
        finally
        {
            File.Delete(inputPath);
        }
    }

    public void Dispose()
    {
        _factory?.Dispose();
        _transcriptionLock.Dispose();
    }

    private static MemoryStream NormalizeAudio(string inputPath)
    {
        using var reader = new MediaFoundationReader(inputPath);
        using var resampler = new MediaFoundationResampler(
            reader,
            new WaveFormat(16000, 16, 1))
        {
            ResamplerQuality = 60
        };
        var normalizedAudio = new MemoryStream();
        WaveFileWriter.WriteWavFileToStream(normalizedAudio, resampler);
        normalizedAudio.Position = 0;
        return normalizedAudio;
    }
}
