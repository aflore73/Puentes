using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;

namespace Puentes.Tests;

public sealed class PromptFactoryContextIsolationTests
{
    [Fact]
    public void OtherPersonFocusRemovesAssistedPersonPrivateDetails()
    {
        var context = new ConversationContext
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "No dormí anoche esperando a Ezequiel.",
            Person = new PersonContext
            {
                Name = "Marta",
                BirthDate = new DateOnly(1950, 7, 1),
                Notes = "DATO_PRIVADO_QUE_NO_DEBE_LLEGAR_AL_MODELO",
                Language = "es-AR"
            },
            State = new ConversationState
            {
                FocusedPersonName = "Ezequiel"
            }
        };

        var prompt = new PromptFactory().Create(context);

        Assert.DoesNotContain(
            "DATO_PRIVADO_QUE_NO_DEBE_LLEGAR_AL_MODELO",
            prompt.UserMessage);
        Assert.DoesNotContain("1950-07-01", prompt.UserMessage);
        Assert.Contains("Ezequiel", prompt.UserMessage);

        // La fábrica no debe modificar permanentemente el contexto original.
        Assert.Equal(
            "DATO_PRIVADO_QUE_NO_DEBE_LLEGAR_AL_MODELO",
            context.Person.Notes);
        Assert.Equal(new DateOnly(1950, 7, 1), context.Person.BirthDate);
    }

    [Fact]
    public void OtherPersonFocusAddsNoSpontaneousProposalRule()
    {
        var context = new ConversationContext
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "No dormí anoche esperando a Ezequiel.",
            Person = new PersonContext { Name = "Marta" },
            State = new ConversationState
            {
                FocusedPersonName = "Ezequiel"
            }
        };

        var prompt = new PromptFactory().Create(context);

        Assert.Contains(
            "No cambies de tema ni ofrezcas por iniciativa propia",
            prompt.SystemMessage);
        Assert.Contains("Ezequiel", prompt.SystemMessage);
    }

    [Fact]
    public void AssistedPersonFocusKeepsHerOwnDetailsAvailable()
    {
        var context = new ConversationContext
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "¿Qué sabés de mí?",
            Person = new PersonContext
            {
                Name = "Marta",
                Notes = "Le gustan las plantas."
            },
            State = new ConversationState
            {
                FocusedPersonName = "Marta"
            }
        };

        var prompt = new PromptFactory().Create(context);

        Assert.Contains("Le gustan las plantas.", prompt.UserMessage);
        Assert.DoesNotContain(
            "No cambies de tema ni ofrezcas por iniciativa propia",
            prompt.SystemMessage);
    }
}
