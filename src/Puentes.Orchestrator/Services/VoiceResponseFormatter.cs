namespace Puentes.Orchestrator.Services;

public static class VoiceResponseFormatter
{
    public static string Prepare(string message) => message.StartsWith(
        "FAKE AI",
        StringComparison.Ordinal)
        ? "Se que esto te preocupa. Ezequiel puede estar trabajando o en su casa. " +
          "Si queres, podes enviarle un mensaje y cuando pueda te va a contestar."
        : message;
}
