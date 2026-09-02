using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Versioning;
using System.Speech.Synthesis;

namespace Puentes.LocalInterpreter.Voice;

/// <summary>Speaks text back to the user using the OS-installed voices (Windows only).</summary>
[SupportedOSPlatform("windows")]
public static class TextToSpeechService
{
    public static void Speak(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        using var synthesizer = new SpeechSynthesizer();

        TrySelectSpanishVoice(synthesizer);

        synthesizer.SetOutputToDefaultAudioDevice();
        synthesizer.Speak(text);
    }

    private static void TrySelectSpanishVoice(SpeechSynthesizer synthesizer)
    {
        bool hasSpanishVoice = synthesizer.GetInstalledVoices()
            .Any(v => v.VoiceInfo.Culture.TwoLetterISOLanguageName
                .Equals("es", StringComparison.OrdinalIgnoreCase));

        if (!hasSpanishVoice)
            return;

        try
        {
            synthesizer.SelectVoiceByHints(
                VoiceGender.NotSet,
                VoiceAge.NotSet,
                0,
                CultureInfo.GetCultureInfo("es-ES"));
        }
        catch (ArgumentException)
        {
            // no exact es-ES voice installed; keep the default voice
        }
    }
}
