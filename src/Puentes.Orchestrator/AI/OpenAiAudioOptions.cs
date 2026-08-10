namespace Puentes.Orchestrator.AI;

public class OpenAiAudioOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string TranscriptionModel { get; set; } = "gpt-4o-mini-transcribe";

    public string SpeechModel { get; set; } = "gpt-4o-mini-tts";

    public string StreamingSpeechModel { get; set; } = "gpt-4o-mini-tts";
}
