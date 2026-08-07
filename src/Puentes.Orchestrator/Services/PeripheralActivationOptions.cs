namespace Puentes.Orchestrator.Services;

public sealed class PeripheralActivationOptions
{
    public bool Enabled { get; set; } = true;

    public Guid PersonId { get; set; } =
        Guid.Parse("20f78dba-4fd8-494e-9bd7-7867bff67df3");

    public int RecordingSeconds { get; set; } = 7;

    public int SilenceMilliseconds { get; set; } = 900;

    public float SpeechThreshold { get; set; } = 0.015f;

    public int CooldownSeconds { get; set; } = 5;
}
