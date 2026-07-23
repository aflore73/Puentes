using Puentes.Shared.Enums;
namespace Puentes.Orchestrator.Prompts;

public static class PromptFactory
{
    public static string Obtener(ScenarioType escenario)
    {
        var promptEspecifico = escenario switch
        {
            ScenarioType.MedicationReminder => PromptRecordatorioMedicacion.Contenido,
            ScenarioType.Conversation => PromptConversacion.Contenido,
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(promptEspecifico))
        {
            return PromptBase.Contenido;
        }

        return $"""
            {PromptBase.Contenido}

            {promptEspecifico}
            """;
    }
}