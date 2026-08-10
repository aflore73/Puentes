using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;
using Puentes.Orchestrator.Services;
using Puentes.Shared.Enums;

namespace Puentes.Tests;

public class MedicationQueryTests
{
    [Theory]
    [InlineData("¿Qué medicación debo tomar?")]
    [InlineData("¿Cuáles son mis remedios?")]
    [InlineData("Decime qué pastillas me corresponden")]
    [InlineData("¿Tengo alguna cápsula ahora?")]
    public void DetectsMedicationQuestions(string input)
    {
        Assert.True(MedicationIntentDetector.IsMedicationQuery(input));
    }

    [Fact]
    public void DoesNotTreatUnrelatedConversationAsMedicationQuery()
    {
        Assert.False(MedicationIntentDetector.IsMedicationQuery(
            "¿Dónde está Ezequiel?"));
    }

    [Theory]
    [InlineData(8, MedicationTurnType.Morning)]
    [InlineData(13, MedicationTurnType.Midday)]
    [InlineData(17, MedicationTurnType.Afternoon)]
    [InlineData(22, MedicationTurnType.Night)]
    [InlineData(2, MedicationTurnType.Night)]
    public void ResolvesEveryMedicationTurn(
        int hour,
        MedicationTurnType expected)
    {
        var dateTime = new DateTime(2026, 8, 7, hour, 0, 0);

        Assert.Equal(expected, MedicationTurnResolver.Resolve(dateTime));
    }

    [Fact]
    public void MedicationQueryPromptRestrictsResponseToCurrentPlan()
    {
        var context = new ConversationContext
        {
            Scenario = ConversationScenario.MedicationQuery,
            UserInput = "¿Qué medicación debo tomar?"
        };

        var prompt = new PromptFactory().Create(context);

        Assert.Contains(
            "únicamente con los medicamentos presentes",
            prompt.SystemMessage);
        Assert.Contains(
            "ni inventes tomas futuras",
            prompt.SystemMessage);
        Assert.Contains(
            "No afirmes que la persona tomó",
            prompt.SystemMessage);
    }
}
