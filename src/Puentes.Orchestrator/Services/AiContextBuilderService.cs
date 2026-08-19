using Puentes.Shared.Responses;
using Puentes.Orchestrator.AI.Models;
using Puentes.Shared.Domain.Knowledge;
using Puentes.Shared.Domain;
using Puentes.Shared.Responses.People;
using Puentes.Shared.Responses.LifeEvents;
using Puentes.Orchestrator.Services;
using System.Globalization;

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
        IReadOnlyCollection<PersonTrustedContactResponse>? trustedContacts = null,
        IReadOnlyCollection<PersonAgendaItemResponse>? agenda = null,
        IReadOnlyCollection<ConversationHistoryItemContext>? conversationHistory = null,
        Person? person = null,
        CompanionProposalMode companionProposalMode =
            CompanionProposalMode.FullContext,
        string? companionProposalCategory = null,
        IReadOnlyCollection<string>? companionProposalCategories = null,
        DialogueOffer? pendingOffer = null,
        IReadOnlyCollection<string>? recentProposalCategories = null,
        DateTime? currentDateTimeOverride = null)
    {
        var currentDateTime = currentDateTimeOverride ?? DateTime.Now;
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
                Notes = person?.Notes,
                Language = "es-AR"
            },

            Environment = new EnvironmentContext
            {
                CurrentDateTime = currentDateTime
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
                belongings,
                trustedContacts,
                agenda,
                person?.Name ?? "Marta",
                companionProposalMode,
                companionProposalCategory,
                companionProposalCategories,
                recentProposalCategories,
                currentDateTime),

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

                ReminderAlreadySent = false,
                PendingOffer = BuildPendingOfferContext(
                    pendingOffer, supportContents)
            }
        };
    }

    private static PendingOfferContext? BuildPendingOfferContext(
        DialogueOffer? pendingOffer,
        IReadOnlyCollection<PersonSupportContentResponse>? supportContents)
    {
        if (pendingOffer is null ||
            pendingOffer.Type == DialogueOfferType.None)
        {
            return null;
        }

        var result = new PendingOfferContext
        {
            Type = pendingOffer.Type,
            CategoryCode = pendingOffer.CategoryCode,
            ContentTitle = pendingOffer.ContentTitle
        };
        if (pendingOffer.Type != DialogueOfferType.Category ||
            string.IsNullOrWhiteSpace(pendingOffer.CategoryCode))
        {
            return result;
        }

        var candidates = supportContents?
            .Where(item => item.IsActive && item.TopicCodes.Contains(
                pendingOffer.CategoryCode,
                StringComparer.OrdinalIgnoreCase))
            .ToArray() ?? [];
        if (candidates.Length == 0)
        {
            return result;
        }

        var candidate = candidates[Random.Shared.Next(candidates.Length)];
        result.SuggestedContentTitle = candidate.Title;
        result.SuggestedContentReference = candidate.Reference;
        return result;
    }

    private static MemorySupportContext? BuildMemorySupportContext(
        ConversationScenario scenario,
        IReadOnlyCollection<MemoryFact>? memoryFacts,
        IReadOnlyCollection<PersonConnectionResponse>? relationships,
        IReadOnlyCollection<LifeEventResponse>? lifeEvents,
        IReadOnlyCollection<PersonRoutineResponse>? routines,
        IReadOnlyCollection<PersonPreferenceResponse>? preferences,
        IReadOnlyCollection<PersonSupportContentResponse>? supportContents,
        IReadOnlyCollection<PersonBelongingResponse>? belongings,
        IReadOnlyCollection<PersonTrustedContactResponse>? trustedContacts,
        IReadOnlyCollection<PersonAgendaItemResponse>? agenda,
        string assistedPersonName,
        CompanionProposalMode companionProposalMode,
        string? companionProposalCategory,
        IReadOnlyCollection<string>? companionProposalCategories,
        IReadOnlyCollection<string>? recentProposalCategories,
        DateTime currentDateTime)
    {
        if (scenario != ConversationScenario.MemorySupport)
        {
            return null;
        }

        var includeFullContext =
            companionProposalMode == CompanionProposalMode.FullContext;
        var selectedTopicCode = companionProposalCategory;
        var facts = includeFullContext ? memoryFacts?
            .Where(fact => fact.IsActive)
            .OrderByDescending(fact => fact.Priority)
            .Select(fact => new MemoryFactContext
            {
                Topic = fact.Topic,
                CurrentSituation = fact.CurrentSituation,
                PositiveMemories = [.. fact.PositiveMemories],
                SuggestedAction = fact.SuggestedAction
            })
            .ToList() ?? [] : [];

        return new MemorySupportContext
        {
            Facts = facts,
            Relationships = includeFullContext ? relationships?
                .Select(BuildRelationshipContext)
                .ToList() ?? [] : [],
            LifeEvents = includeFullContext || companionProposalMode ==
                CompanionProposalMode.PositiveMemoriesOnly ? lifeEvents?
                .Where(lifeEvent => includeFullContext ||
                    lifeEvent.IsPositiveMemory && lifeEvent.PersonName.Equals(
                        assistedPersonName,
                        StringComparison.OrdinalIgnoreCase) &&
                    (selectedTopicCode is null || lifeEvent.TopicCodes.Contains(
                        selectedTopicCode, StringComparer.OrdinalIgnoreCase)))
                .OrderBy(lifeEvent => lifeEvent.StartDate)
                .ThenBy(lifeEvent => lifeEvent.Title)
                .Select(BuildLifeEventContext)
                .ToList() ?? [] : [],
            Routines = includeFullContext ? routines?
                .Where(routine => routine.IsActive &&
                    IsApplicableRoutine(routine, currentDateTime))
                .OrderBy(routine => routine.PersonName)
                .ThenBy(routine => routine.Title)
                .Select(routine => new PersonRoutineContext
                {
                    PersonName = routine.PersonName,
                    Title = routine.Title,
                    Notes = routine.Notes,
                    DaysOfWeek = routine.DaysOfWeek,
                    StartTime = routine.StartTime,
                    EndTime = routine.EndTime
                })
                .ToList() ?? [] : [],
            Preferences = includeFullContext || companionProposalMode ==
                CompanionProposalMode.InterestsOnly ? preferences?
                .Where(preference => preference.IsActive &&
                    (includeFullContext || preference.PersonName.Equals(
                        assistedPersonName,
                        StringComparison.OrdinalIgnoreCase) &&
                    (selectedTopicCode is null || preference.TopicCodes.Contains(
                        selectedTopicCode, StringComparer.OrdinalIgnoreCase))))
                .OrderBy(preference => preference.PersonName)
                .ThenBy(preference => preference.Title)
                .Select(preference => new PersonPreferenceContext
                {
                    PersonName = preference.PersonName,
                    Title = preference.Title,
                    Notes = preference.Notes,
                    TopicCodes = [.. preference.TopicCodes]
                })
                .ToList() ?? [] : [],
            SupportContents = includeFullContext || companionProposalMode ==
                CompanionProposalMode.ReadingsOnly ? supportContents?
                .Where(content => content.IsActive &&
                    (includeFullContext || selectedTopicCode is null ||
                        content.TopicCodes.Contains(selectedTopicCode,
                            StringComparer.OrdinalIgnoreCase)))
                .OrderBy(content => content.Title)
                .Select(content => new PersonSupportContentContext
                {
                    Title = content.Title,
                    Content = content.Content,
                    TopicCodes = [.. content.TopicCodes],
                    Attribution = content.Attribution,
                    Reference = content.Reference
                })
                .ToList() ?? [] : [],
            Belongings = includeFullContext ? belongings?
                .Where(belonging => belonging.IsActive)
                .OrderBy(belonging => belonging.Name)
                .Select(belonging => new PersonBelongingContext
                {
                    Name = belonging.Name,
                    Notes = belonging.Notes
                })
                .ToList() ?? [] : [],
            TrustedContacts = includeFullContext ? trustedContacts?
                .Where(contact => contact.IsActive)
                .OrderBy(contact => contact.Priority)
                .Select(contact => new PersonTrustedContactContext
                {
                    ContactPersonName = contact.ContactPersonName,
                    Priority = contact.Priority,
                    Notes = contact.Notes
                })
                .ToList() ?? [] : [],
            Agenda = includeFullContext ? agenda?
                .Where(item => item.Status == AgendaItemStatus.Scheduled)
                .OrderBy(item => item.ScheduledAt)
                .Select(item => new AgendaItemContext
                {
                    ScheduledAt = item.ScheduledAt,
                    EndAt = item.EndAt,
                    Title = item.Title,
                    Description = item.Description,
                    Place = item.Place,
                    Status = item.Status,
                    TopicCodes = [.. item.TopicCodes],
                    Participants = item.Participants.Select(participant =>
                        new LifeEventParticipantContext
                        {
                            PersonName = participant.PersonName,
                            Role = participant.Role
                        }).ToList()
                }).ToList() ?? [] : [],
            ProposalCandidates = companionProposalMode ==
                CompanionProposalMode.CategoriesOnly
                ? companionProposalCategories?.Count > 0
                    ? companionProposalCategories.Take(3)
                        .Select(topicCode => new CompanionProposalContext
                        {
                            TopicCode = topicCode
                        }).ToList()
                    : BuildCompanionProposalCategories(
                        assistedPersonName,
                        lifeEvents,
                        preferences,
                        supportContents,
                        recentProposalCategories)
                : []
        };
    }

    private static bool IsApplicableRoutine(
        PersonRoutineResponse routine,
        DateTime currentDateTime)
    {
        var hasDays = !string.IsNullOrWhiteSpace(routine.DaysOfWeek);
        var hasStart = !string.IsNullOrWhiteSpace(routine.StartTime);
        var hasEnd = !string.IsNullOrWhiteSpace(routine.EndTime);

        // Legacy routines remain available until their schedules are migrated.
        if (!hasDays && !hasStart && !hasEnd) return true;
        if (!hasDays || !hasStart || !hasEnd) return false;

        var appliesToday = routine.DaysOfWeek!.Split(',',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Any(value => Enum.TryParse<DayOfWeek>(value, true, out var day) &&
                day == currentDateTime.DayOfWeek);
        if (!appliesToday) return false;

        const string format = "HH:mm";
        if (!TimeOnly.TryParseExact(routine.StartTime, format,
                CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var start) ||
            !TimeOnly.TryParseExact(routine.EndTime, format,
                CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var end))
        {
            return false;
        }

        var currentTime = TimeOnly.FromDateTime(currentDateTime);
        return currentTime >= start && currentTime < end;
    }

    private static List<CompanionProposalContext>
        BuildCompanionProposalCategories(
        string assistedPersonName,
        IReadOnlyCollection<LifeEventResponse>? lifeEvents,
        IReadOnlyCollection<PersonPreferenceResponse>? preferences,
        IReadOnlyCollection<PersonSupportContentResponse>? supportContents,
        IReadOnlyCollection<string>? recentProposalCategories)
    {
        var recent = recentProposalCategories?.ToHashSet(
            StringComparer.OrdinalIgnoreCase) ??
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var proposals = new List<CompanionProposalContext>();
        var readingTopics = supportContents?
            .Where(item => item.IsActive)
            .SelectMany(item => item.TopicCodes)
            .Where(code => code.StartsWith("reading.",
                StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(code => !recent.Contains(code))
            .ToArray() ?? [];
        if (readingTopics.Length > 0)
        {
            proposals.Add(new CompanionProposalContext
            {
                TopicCode = readingTopics[Random.Shared.Next(readingTopics.Length)]
            });
        }

        var interestTopics = preferences?
            .Where(item => item.IsActive && item.PersonName.Equals(
                assistedPersonName,
                StringComparison.OrdinalIgnoreCase))
            .SelectMany(item => item.TopicCodes)
            .Where(code => code.StartsWith("interest.",
                StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(code => !recent.Contains(code))
            .ToArray() ?? [];
        if (interestTopics.Length > 0)
        {
            proposals.Add(new CompanionProposalContext
            {
                TopicCode = interestTopics[Random.Shared.Next(interestTopics.Length)]
            });
        }

        var memoryTopics = lifeEvents?
            .Where(item => item.IsPositiveMemory && item.PersonName.Equals(
                assistedPersonName,
                StringComparison.OrdinalIgnoreCase))
            .SelectMany(item => item.TopicCodes)
            .Where(code => code.StartsWith("memory.",
                StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(code => !recent.Contains(code))
            .ToArray() ?? [];
        if (memoryTopics.Length > 0)
        {
            proposals.Add(new CompanionProposalContext
            {
                TopicCode = memoryTopics[Random.Shared.Next(memoryTopics.Length)]
            });
        }

        if (proposals.Count == 0 && recent.Count > 0)
        {
            return BuildCompanionProposalCategories(
                assistedPersonName,
                lifeEvents,
                preferences,
                supportContents,
                recentProposalCategories: []);
        }

        for (var index = proposals.Count - 1; index > 0; index--)
        {
            var other = Random.Shared.Next(index + 1);
            (proposals[index], proposals[other]) =
                (proposals[other], proposals[index]);
        }

        return proposals.Take(3).ToList();
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
            TopicCodes = [.. lifeEvent.TopicCodes],
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
