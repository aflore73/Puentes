namespace Puentes.Orchestrator.Services;

public enum PeripheralInputMode
{
    Audio,
    Keyboard
}

public sealed class PeripheralActivationOptions
{
    public bool Enabled { get; set; } = true;

    public bool UseRealtime { get; set; } = true;

    public PeripheralInputMode InputMode { get; set; } =
        PeripheralInputMode.Audio;

    public int ActivationVirtualKey { get; set; } = 13;

    public bool EnableMouseActivation { get; set; }

    public bool EnableAudioCues { get; set; } = true;

    public Guid PersonId { get; set; } =
        Guid.Parse("20f78dba-4fd8-494e-9bd7-7867bff67df3");

    public int RecordingSeconds { get; set; } = 7;

    public int SilenceMilliseconds { get; set; } = 900;

    public float SpeechThreshold { get; set; } = 0.015f;

    public double CooldownSeconds { get; set; } = 0.25;
}
