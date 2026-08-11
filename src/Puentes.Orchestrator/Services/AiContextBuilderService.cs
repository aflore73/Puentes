using Puentes.Shared.Responses;
using Puentes.Orchestrator.AI.Models;
using Puentes.Shared.Domain.Knowledge;
using Puentes.Shared.Domain;
using Puentes.Shared.Responses.People;
using Puentes.Shared.Responses.LifeEvents;

public class AiContextBuilderService
{
    public ConversationContext BuildConversationContext(
        ConversationRequest request,
        MedicationPlanResponse? plan = null,
        IReadOnlyCollection<MemoryFact>? memoryFacts = null,
        IReadOnlyCollection<PersonConnectionResponse>? relationships = null,
        IReadOnlyCollection<LifeEventResponse>? lifeEvents = null,
        IReadOnlyCollection<PersonRoutineResponse>? routines = null,
        IReadOnlyCollection<PersonPreferenceResponse>? preferences = null,
        IReadOnlyCollection<PersonSupportContentResponse>? supportContents = null,
        IReadOnlyCollection<PersonBelongingResponse>? belongings = null,
        IReadOnlyCollection<ConversationHistoryItemContext>? conversationHistory = null,
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
                relationships,
                lifeEvents,
                routines,
                preferences,
                supportContents,
                belongings),

            ConversationHistory = conversationHistory?
                .Select(message => new ConversationHistoryItemContext
                {
                    Role = message.Role,
                    Content = message.Content
                })
                .ToList() ?? [],

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
        IReadOnlyCollection<PersonConnectionResponse>? relationships,
        IReadOnlyCollection<LifeEventResponse>? lifeEvents,
        IReadOnlyCollection<PersonRoutineResponse>? routines,
        IReadOnlyCollection<PersonPreferenceResponse>? preferences,
        IReadOnlyCollection<PersonSupportContentResponse>? supportContents,
        IReadOnlyCollection<PersonBelongingResponse>? belongings)
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
                .ToList() ?? [],
            LifeEvents = lifeEvents?
                .OrderBy(lifeEvent => lifeEvent.StartDate)
                .ThenBy(lifeEvent => lifeEvent.Title)
                .Select(BuildLifeEventContext)
                .ToList() ?? [],
            Routines = routines?
                .Where(routine => routine.IsActive)
                .OrderBy(routine => routine.PersonName)
                .ThenBy(routine => routine.Title)
                .Select(routine => new PersonRoutineContext
                {
                    PersonName = routine.PersonName,
                    Title = routine.Title,
                    Notes = routine.Notes
                })
                .ToList() ?? [],
            Preferences = preferences?
                .Where(preference => preference.IsActive)
                .OrderBy(preference => preference.PersonName)
                .ThenBy(preference => preference.Title)
                .Select(preference => new PersonPreferenceContext
                {
                    PersonName = preference.PersonName,
                    Title = preference.Title,
                    Notes = preference.Notes
                })
                .ToList() ?? [],
            SupportContents = supportContents?
                .Where(content => content.IsActive)
                .OrderBy(content => content.Title)
                .Select(content => new PersonSupportContentContext
                {
                    Title = content.Title,
                    Content = content.Content,
                    Attribution = content.Attribution,
                    Reference = content.Reference
                })
                .ToList() ?? [],
            Belongings = belongings?
                .Where(belonging => belonging.IsActive)
                .OrderBy(belonging => belonging.Name)
                .Select(belonging => new PersonBelongingContext
                {
                    Name = belonging.Name,
                    Notes = belonging.Notes
                })
                .ToList() ?? []
        };
    }

    private static LifeEventContext BuildLifeEventContext(
        LifeEventResponse lifeEvent)
    {
        return new LifeEventContext
        {
            PersonName = lifeEvent.PersonName,
            StartDate = lifeEvent.StartDate,
            EndDate = lifeEvent.EndDate,
            DatePrecision = lifeEvent.DatePrecision,
            Title = lifeEvent.Title,
            Description = lifeEvent.Description,
            Place = lifeEvent.Place,
            IsPositiveMemory = lifeEvent.IsPositiveMemory,
            Participants = lifeEvent.Participants
                .Select(participant => new LifeEventParticipantContext
                {
                    PersonName = participant.PersonName,
                    Role = participant.Role
                })
                .ToList()
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
