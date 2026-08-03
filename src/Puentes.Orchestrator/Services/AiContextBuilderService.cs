using Puentes.Shared.Responses;
using Puentes.Orchestrator.AI.Models;
using Puentes.Shared.Domain.Knowledge;
using Puentes.Shared.Domain;
using Puentes.Shared.Responses.People;

public class AiContextBuilderService
{
    public ConversationContext BuildConversationContext(
        ConversationRequest request,
        MedicationPlanResponse? plan = null,
        IReadOnlyCollection<MemoryFact>? memoryFacts = null,
        IReadOnlyCollection<PersonConnectionResponse>? relationships = null,
        Person? person = null)
    {
        return new ConversationContext
        {
            Scenario = request.Scenario,
            UserInput = request.UserInput,

            Person = new PersonContext
            {
                Name = person?.Name ?? "Marta",
                BirthDate = person is null
                    ? new DateOnly(1950, 7, 1)
                    : person.BirthDate is null
                        ? null
                        : DateOnly.FromDateTime(person.BirthDate.Value),
                Language = "es-AR"
            },

            Environment = new EnvironmentContext
            {
                CurrentDateTime = DateTime.Now
            },

            Medication = BuildMedicationContext(plan),

            MemorySupport = BuildMemorySupportContext(
                request.Scenario,
                memoryFacts,
                relationships),

            State = new ConversationState
            {
                WaitingMedicationConfirmation =
                    request.WaitingMedicationConfirmation,

                ReminderAlreadySent = false
            }
        };
    }

    private static MemorySupportContext? BuildMemorySupportContext(
        ConversationScenario scenario,
        IReadOnlyCollection<MemoryFact>? memoryFacts,
        IReadOnlyCollection<PersonConnectionResponse>? relationships)
    {
        if (scenario != ConversationScenario.MemorySupport)
        {
            return null;
        }

        var facts = memoryFacts?
            .Where(fact => fact.IsActive)
            .OrderByDescending(fact => fact.Priority)
            .Select(fact => new MemoryFactContext
            {
                Topic = fact.Topic,
                CurrentSituation = fact.CurrentSituation,
                PositiveMemories = [.. fact.PositiveMemories],
                SuggestedAction = fact.SuggestedAction
            })
            .ToList() ?? [];

        return new MemorySupportContext
        {
            Facts = facts,
            Relationships = relationships?
                .Select(BuildRelationshipContext)
                .ToList() ?? []
        };
    }

    private static PersonRelationshipContext BuildRelationshipContext(
        PersonConnectionResponse relationship)
    {
        var otherPerson = relationship.OtherPerson;
        var residenceParts = new[]
        {
            otherPerson.City,
            otherPerson.Province,
            otherPerson.Country
        }
        .Where(value => !string.IsNullOrWhiteSpace(value));
        var residence = string.Join(", ", residenceParts);

        return new PersonRelationshipContext
        {
            OtherPersonName = otherPerson.Name,
            OtherPersonBirthDate = otherPerson.BirthDate is null
                ? null
                : DateOnly.FromDateTime(otherPerson.BirthDate.Value),
            OtherPersonResidence = string.IsNullOrWhiteSpace(residence)
                ? null
                : residence,
            Type = relationship.Type,
            Direction = relationship.Direction,
            Notes = relationship.Notes
        };
    }

    private static MedicationContext? BuildMedicationContext(
        MedicationPlanResponse? plan)
    {
        if (plan is null)
        {
            return null;
        }

        return new MedicationContext
        {
            Turn = plan.Turn,

            Medications = plan.Medications
                .Select(medication => new MedicationItemContext
                {
                    Name = medication.Name,
                    Quantity = medication.Quantity,
                    SpeakName = medication.SpeakName,
                    Form = medication.Form.ToString(),
                    Shape = medication.Shape.ToString(),
                    Color = medication.Color.ToString()
                })
                .ToList()
        };
    }
}
