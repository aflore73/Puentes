using NAudio.Wave;
using NAudio.CoreAudioApi;
using Puentes.Orchestrator.AI;
using System.Runtime.Versioning;

namespace Puentes.Orchestrator.Services;

public sealed class PuentesDiagnosticsService
{
    private readonly ApiClient _apiClient;
    private readonly OpenAiOptions _openAiOptions;

    public PuentesDiagnosticsService(
        ApiClient apiClient,
        OpenAiOptions openAiOptions)
    {
        _apiClient = apiClient;
        _openAiOptions = openAiOptions;
    }

    public async Task<PuentesDiagnosticResult> CheckAsync(
        CancellationToken cancellationToken)
    {
        var apiAvailable = await _apiClient.IsHealthyAsync(cancellationToken);
        var (microphoneCount, speakerCount) = OperatingSystem.IsWindows()
            ? GetWindowsAudioDeviceCounts()
            : (0, 0);
        var apiKeyConfigured = !string.IsNullOrWhiteSpace(
            _openAiOptions.ApiKey);

        return new PuentesDiagnosticResult(
            apiAvailable && microphoneCount > 0 &&
            speakerCount > 0 && apiKeyConfigured,
            apiAvailable,
            apiKeyConfigured,
            microphoneCount,
            speakerCount);
    }

    [SupportedOSPlatform("windows")]
    private static (int Microphones, int Speakers)
        GetWindowsAudioDeviceCounts()
    {
        using var devices = new MMDeviceEnumerator();
        var speakers = devices.EnumerateAudioEndPoints(
            DataFlow.Render,
            DeviceState.Active).Count;
        return (WaveInEvent.DeviceCount, speakers);
    }
}

public sealed record PuentesDiagnosticResult(
    bool Ready,
    bool ApiAvailable,
    bool OpenAiApiKeyConfigured,
    int MicrophoneCount,
    int SpeakerCount);
