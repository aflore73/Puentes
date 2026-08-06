namespace Puentes.Orchestrator.AI;

public sealed class PiperOptions
{
    public string ExecutablePath { get; set; } = "python";

    public string ModelPath { get; set; } =
        "voices/es_AR-daniela-high.onnx";

    public string[] ExecutableArguments { get; set; } = ["-m", "piper"];

    public double LengthScale { get; set; } = 1.25;

    public double SentenceSilence { get; set; } = 0.4;

    public int StartSilenceMilliseconds { get; set; } = 500;

    public int EndSilenceMilliseconds { get; set; } = 700;
}
