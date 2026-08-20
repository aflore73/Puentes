namespace Puentes.Orchestrator.AI;

public class OpenAiAudioOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string TranscriptionModel { get; set; } = "gpt-4o-mini-transcribe";

    public string SpeechModel { get; set; } =
        "gpt-4o-mini-tts-2025-12-15";

    public string StreamingSpeechModel { get; set; } =
        "gpt-4o-mini-tts-2025-12-15";

    public string SpeechInstructions { get; set; } =
        "Habla siempre con la misma voz femenina adulta, con acento argentino " +
        "natural, tono cercano y sereno, ritmo parejo y entonacion " +
        "conversacional. Manten estable el timbre entre respuestas. Evita " +
        "dramatizar, susurrar, cantar o cambiar de personaje.";
}
