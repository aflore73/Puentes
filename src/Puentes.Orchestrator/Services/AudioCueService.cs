using System.Runtime.Versioning;

namespace Puentes.Orchestrator.Services;

public sealed class AudioCueService
{
    private readonly PeripheralActivationOptions _options;

    public AudioCueService(
        Microsoft.Extensions.Options.IOptions<PeripheralActivationOptions> options)
    {
        _options = options.Value;
    }

    public Task ListeningAsync() => PlayAsync([(880, 75)]);

    public Task CapturedAsync() => PlayAsync([(660, 60)]);

    public Task ErrorAsync() => PlayAsync([(440, 90), (330, 120)]);

    private async Task PlayAsync((int Frequency, int Duration)[] tones)
    {
        if (!_options.EnableAudioCues || !OperatingSystem.IsWindows())
        {
            return;
        }

#pragma warning disable CA1416
        await Task.Run(() => PlayWindowsTones(tones));
#pragma warning restore CA1416
    }

    [SupportedOSPlatform("windows")]
    private static void PlayWindowsTones(
        (int Frequency, int Duration)[] tones)
    {
        try
        {
            foreach (var tone in tones)
            {
                Console.Beep(tone.Frequency, tone.Duration);
            }
        }
        catch (PlatformNotSupportedException)
        {
        }
    }
}
