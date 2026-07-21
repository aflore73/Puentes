using Puentes.Shared.Responses;

namespace Puentes.Orchestrator.Services;

public class MedicationMessageService
{
    public string BuildMessage(
        MedicationPlanResponse plan)
    {
        var message = new System.Text.StringBuilder();

        message.Append(
            $"Marta, es momento de tus medicamentos de {plan.Turn.ToString().ToLower()}. ");

        message.Append("Tenés que tomar ");

        for (int i = 0; i < plan.Medications.Count; i++)
        {
            var medication = plan.Medications[i];

            message.Append(
                $"{medication.Name}");

            if (i < plan.Medications.Count - 2)
                message.Append(", ");
            else if (i == plan.Medications.Count - 2)
                message.Append(" y ");
        }

        message.Append(".");

        return message.ToString();
    }
}