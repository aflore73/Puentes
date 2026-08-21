using Puentes.Orchestrator.AI.Models;
using Puentes.Shared.Responses.People;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Puentes.Orchestrator.Services;

public class MemoryConversationWorkflowService
{
    private readonly ApiClient _apiClient;
    private readonly AiContextBuilderService _contextBuilder;
    private readonly IConversationService _conversationService;
    private readonly InMemoryConversationStore _conversationStore;
    private readonly IConversationContextSelector _contextSelector;
    private readonly ILogger<MemoryConversationWorkflowService> _logger;

    public MemoryConversationWorkflowService(
        ApiClient apiClient,
        AiContextBuilderService contextBuilder,
        IConversationService conversationService,
        InMemoryConversationStore conversationStore,
        IConversationContextSelector contextSelector,
        ILogger<MemoryConversationWorkflowService> logger)
    {
        _apiClient = apiClient;
        _contextBuilder = contextBuilder;
        _conversationService = conversationService;
        _conversationStore = conversationStore;
        _contextSelector = contextSelector;
        _logger = logger;
    }

    public async Task<AssistantResponse> ProcessAsync(
        Guid personId,
        string userInput,
        CancellationToken cancellationToken = default)
    {
        return await ProcessCoreAsync(
            personId,
            userInput,
            [],
            conversationId: null,
            cancellationToken,
            waitingForProposalChoice: false,
            offeredProposalCategories: [],
            pendingOffer: null,
            pendingOffers: [],
            recentProposalCategories: [],
            recentMemoryIds: []);
    }

    public async Task<MemoryConversationTurnResponse> StartAsync(
        Guid personId,
        string userInput,
        CancellationToken cancellationToken = default,
        string? focusedPersonName = null)
    {
        var conversationId = _conversationStore.Create(personId);

        try
        {
            var response = await ProcessCoreAsync(
                personId,
                userInput,
                [],
                conversationId,
                cancellationToken,
                focusedPersonName,
                waitingForProposalChoice: false,
                offeredProposalCategories: [],
                pendingOffer: null,
                pendingOffers: [],
                recentProposalCategories: [],
                recentMemoryIds: []);

            return new MemoryConversationTurnResponse
            {
                ConversationId = conversationId,
                Response = response
            };
        }
        catch
        {
            _conversationStore.Remove(conversationId);
            throw;
        }
    }

    public async Task<MemoryConversationTurnResponse> ContinueAsync(
        Guid conversationId,
        string userInput,
        CancellationToken cancellationToken = default,
        string? focusedPersonName = null)
    {
        var conversation = _conversationStore.Get(conversationId)
            ?? throw new InvalidOperationException(
                "La conversación no existe o expiró.");
        var response = await ProcessCoreAsync(
            conversation.PersonId,
            userInput,
            conversation.History,
            conversationId,
            cancellationToken,
            focusedPersonName ?? conversation.FocusedPersonName,
            conversation.WaitingForCompanionProposalChoice,
            conversation.CompanionProposalCategories,
            conversation.PendingOffer,
            conversation.PendingOffers,
            conversation.RecentProposalCategories,
            conversation.RecentMemoryIds);

        return new MemoryConversationTurnResponse
        {
            ConversationId = conversationId,
            Response = response
        };
    }

    private async Task<AssistantResponse> ProcessCoreAsync(
        Guid personId,
        string userInput,
        IReadOnlyCollection<ConversationHistoryItemContext> history,
        Guid? conversationId,
        CancellationToken cancellationToken,
        string? focusedPersonName = null,
        bool waitingForProposalChoice = false,
        IReadOnlyCollection<string>? offeredProposalCategories = null,
        DialogueOffer? pendingOffer = null,
        IReadOnlyCollection<DialogueOffer>? pendingOffers = null,
        IReadOnlyCollection<string>? recentProposalCategories = null,
        IReadOnlyCollection<Guid>? recentMemoryIds = null)
    {
        var person = await _apiClient.GetPersonAsync(
            personId,
            cancellationToken) ?? throw new InvalidOperationException(
                $"No se encontró la persona {personId}.");
        var relationships = await _apiClient
            .GetPersonRelationshipsAsync(personId, cancellationToken);
        var previousFocusedPersonName = focusedPersonName;
        var explicitFocusedPersonName = ConversationFocusResolver.Resolve(
            userInput, person.Name, relationships, previousFocus: null);
        focusedPersonName = explicitFocusedPersonName ??
            previousFocusedPersonName;
        var knownPersonNames = relationships
            .Select(item => item.OtherPerson.Name)
            .Append(person.Name)
            .ToArray();
        var supportContents = await _apiClient
            .GetPersonSupportContentsAsync(personId, cancellationToken);
        var belongings = await _apiClient
            .GetPersonBelongingsAsync(personId, cancellationToken);
        var trustedContacts = await _apiClient
            .GetPersonTrustedContactsAsync(personId, cancellationToken);
        var agenda = await _apiClient.GetPersonAgendaAsync(
            personId, cancellationToken);
        var lifeEventOwners = relationships
            .Select(relationship => relationship.OtherPerson.Id)
            .Append(personId)
            .Distinct();
        var contextTasks = lifeEventOwners.Select(async ownerId =>
        {
            var lifeEventsTask = _apiClient.GetPersonLifeEventsAsync(
                ownerId, cancellationToken);
            var routinesTask = _apiClient.GetPersonRoutinesAsync(
                ownerId, cancellationToken);
            var preferencesTask = _apiClient.GetPersonPreferencesAsync(
                ownerId, cancellationToken);
            await Task.WhenAll(
                lifeEventsTask,
                routinesTask,
                preferencesTask);
            return (
                LifeEvents: await lifeEventsTask,
                Routines: await routinesTask,
                Preferences: await preferencesTask);
        });
        var contextItems = await Task.WhenAll(contextTasks);
        var allLifeEvents = contextItems
            .SelectMany(item => item.LifeEvents).ToList();
        var allRoutines = contextItems
            .SelectMany(item => item.Routines).ToList();
        var allPreferences = contextItems
            .SelectMany(item => item.Preferences).ToList();
        var provisionalLifeEvents = FilterByPerson(
            allLifeEvents, focusedPersonName, item => item.PersonName);
        var provisionalRoutines = FilterByPerson(
            allRoutines, focusedPersonName, item => item.PersonName);
        var provisionalPreferences = FilterByPerson(
            allPreferences, focusedPersonName, item => item.PersonName);
        var selection = LocalConversationContextSelector.TrySelect(new()
        {
            UserInput = userInput,
            ExplicitFocusedPersonName = explicitFocusedPersonName,
            WaitingForProposalChoice = waitingForProposalChoice,
            OfferedProposalCategories = offeredProposalCategories ?? [],
            PendingOffers = pendingOffers ?? [],
            Routines = provisionalRoutines,
            LifeEvents = provisionalLifeEvents,
            Preferences = provisionalPreferences,
            SupportContents = supportContents,
            Belongings = belongings,
            Agenda = agenda
        });
        var selectionSource = "local";
        if (selection is null)
        {
            selectionSource = "OpenAI";
            selection = await _contextSelector.SelectAsync(
                userInput,
                history,
                person.Name,
                knownPersonNames,
                focusedPersonName,
                cancellationToken);
        }
        _logger.LogInformation(
            "Contexto seleccionado por {SelectionSource}: {Kinds}",
            selectionSource,
            string.Join(", ", selection.Kinds));
        focusedPersonName = ConversationFocusResolver.ResolveFinal(
            explicitFocusedPersonName,
            previousFocusedPersonName,
            selection.FocusedPersonName,
            selection.Kinds.Contains(ConversationContextKind.External));
        var focusIsAssistedPerson = focusedPersonName?.Equals(
            person.Name, StringComparison.OrdinalIgnoreCase) == true;
        var filteredRelationships = focusedPersonName is null ||
            focusIsAssistedPerson
            ? relationships
            : relationships.Where(relationship =>
                relationship.OtherPerson.Name.Equals(
                    focusedPersonName,
                    StringComparison.OrdinalIgnoreCase)).ToList();
        var lifeEvents = FilterByPerson(
            allLifeEvents, focusedPersonName, item => item.PersonName);
        var routines = FilterByPerson(
            allRoutines, focusedPersonName, item => item.PersonName);
        var preferences = FilterByPerson(
            allPreferences, focusedPersonName, item => item.PersonName);
        var availableRoutineTitles = routines.Select(item => item.Title)
            .ToArray();
        var availableLifeEventTitles = lifeEvents.Select(item => item.Title)
            .ToArray();
        var availablePreferenceTitles = preferences.Select(item => item.Title)
            .ToArray();
        var availableSupportContentTitles = supportContents
            .Select(item => item.Title).ToArray();
        var availableBelongingNames = belongings.Select(item => item.Name)
            .ToArray();
        var availableAgendaTitles = agenda.Select(item => item.Title)
            .ToArray();
        if (!selection.Includes(ConversationContextKind.Routine) &&
            HasLexicalRoutineMatch(userInput, routines))
        {
            selection.Kinds.Add(ConversationContextKind.Routine);
        }
        var companionScope = selection.Includes(
            ConversationContextKind.Companion);
        if (!selection.Includes(ConversationContextKind.Memory) &&
            !companionScope)
            lifeEvents = [];
        if (!selection.Includes(ConversationContextKind.Routine))
            routines = [];
        else if (selection.TimeFrame == ConversationTimeFrame.Current)
            routines = routines.Where(routine =>
                AiContextBuilderService.AppliesNow(routine, DateTime.Now)
                    is not false).ToList();
        else if (selection.TimeFrame ==
                 ConversationTimeFrame.YesterdayEvening)
            routines = routines.Where(routine =>
                AiContextBuilderService.AppliesYesterdayEvening(
                    routine, DateTime.Now) is not false ||
                AiContextBuilderService.AppliesNow(
                    routine, DateTime.Now) is not false).ToList();
        if (!selection.Includes(ConversationContextKind.Preference) &&
            !companionScope)
            preferences = [];
        if (!selection.Includes(ConversationContextKind.Reading) &&
            !companionScope)
            supportContents = [];
        if (!selection.Includes(ConversationContextKind.Belonging))
            belongings = [];
        if (!selection.Includes(ConversationContextKind.Agenda))
            agenda = [];
        if (!selection.Includes(ConversationContextKind.Companion))
            trustedContacts = [];
        if (selection.Kinds.Contains(ConversationContextKind.External))
            filteredRelationships = [];
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = userInput
        };
        var selectedProposalCategory =
            CompanionProposalModeDetector.FindSelectedCategory(
                userInput,
                offeredProposalCategories ?? []);
        selectedProposalCategory ??=
            CompanionProposalModeDetector.FindSelectedCategory(
                userInput,
                lifeEvents.SelectMany(item => item.TopicCodes)
                    .Concat(preferences.SelectMany(item => item.TopicCodes))
                    .Concat(supportContents.SelectMany(item => item.TopicCodes))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray());
        var effectiveProposalCategory = selectedProposalCategory ??
            (pendingOffers?.Count == 1 &&
                pendingOffers.First().Type == DialogueOfferType.Category
                ? pendingOffers.First().CategoryCode
                : null);
        var proposalMode = CompanionProposalModeDetector.Resolve(
            userInput,
            waitingForProposalChoice,
            effectiveProposalCategory);
        MemorySelection? selectedMemory = null;
        if (effectiveProposalCategory?.StartsWith("memory.",
                StringComparison.OrdinalIgnoreCase) == true)
        {
            selectedMemory = MemoryCandidateSelector.Select(
                lifeEvents,
                person.Name,
                effectiveProposalCategory,
                recentMemoryIds ?? []);
            lifeEvents = selectedMemory is null
                ? []
                : [selectedMemory.LifeEvent];
        }
        else if (proposalMode == CompanionProposalMode.PositiveMemoriesOnly)
        {
            selectedMemory = MemoryCandidateSelector.SelectAny(
                lifeEvents,
                person.Name,
                recentMemoryIds ?? []);
            lifeEvents = selectedMemory is null
                ? []
                : [selectedMemory.LifeEvent];
        }
        var context = _contextBuilder.BuildConversationContext(
            request,
            relationships: filteredRelationships,
            lifeEvents: lifeEvents,
            routines: routines,
            preferences: preferences,
            supportContents: supportContents,
            belongings: belongings,
            trustedContacts: trustedContacts,
            agenda: agenda,
            conversationHistory: history,
            person: selection.Kinds.Contains(ConversationContextKind.External)
                ? new Puentes.Shared.Domain.Person { Name = person.Name }
                : person,
            companionProposalMode: proposalMode,
            companionProposalCategory: effectiveProposalCategory,
            companionProposalCategories: offeredProposalCategories,
            pendingOffer: pendingOffer,
            pendingOffers: pendingOffers,
            recentProposalCategories: recentProposalCategories,
            focusedPersonName: focusedPersonName);

        var response = await _conversationService.ProcessAsync(
            context,
            cancellationToken);
        if (MustClarifyMultipleOffers(
            selectedProposalCategory, pendingOffers, response))
        {
            context.State.RequiredDialogueAction =
                RequiredDialogueAction.ClarifyOfferChoice;
            response = await _conversationService.ProcessAsync(
                context,
                cancellationToken);
        }
        else if (MustOfferSuggestedContent(context, response))
        {
            context.State.RequiredDialogueAction =
                RequiredDialogueAction.OfferSuggestedContent;
            response = await _conversationService.ProcessAsync(
                context,
                cancellationToken);
        }
        else if (MustContinueWithAcceptedCategory(context, response))
        {
            context.State.RequiredDialogueAction =
                RequiredDialogueAction.ContinueWithAcceptedCategory;
            response = await _conversationService.ProcessAsync(
                context,
                cancellationToken);
        }
        if (context.State.AvoidAssistedPersonName &&
            MentionsAssistedPersonName(response.Message, person.Name))
        {
            response = await _conversationService.ProcessAsync(
                context,
                cancellationToken);
        }
        var validationPersonNames = selection.Kinds.Contains(
            ConversationContextKind.External)
            ? [person.Name]
            : knownPersonNames;
        var validationErrors = ResponseEvidenceValidator.Validate(
            context, response, validationPersonNames);
        AddRequiredEvidenceErrors(
            selection, context, response, validationErrors);
        AddExcludedContextErrors(
            context, response, validationErrors,
            availableRoutineTitles,
            availableLifeEventTitles,
            availablePreferenceTitles,
            availableSupportContentTitles,
            availableBelongingNames,
            availableAgendaTitles);
        AddExternalLanguageErrors(selection, response, validationErrors);
        AddProposalCandidateErrors(context, response, validationErrors);
        AddConversationalLanguageErrors(selection, response, validationErrors);
        if (validationErrors.Count > 0)
        {
            context.State.RequiredDialogueAction =
                RequiredDialogueAction.CorrectInvalidResponse;
            context.State.ResponseValidationErrors = [.. validationErrors];
            response = await _conversationService.ProcessAsync(
                context, cancellationToken);
            validationErrors = ResponseEvidenceValidator.Validate(
                context, response, validationPersonNames);
            AddRequiredEvidenceErrors(
                selection, context, response, validationErrors);
            AddExcludedContextErrors(
                context, response, validationErrors,
                availableRoutineTitles,
                availableLifeEventTitles,
                availablePreferenceTitles,
                availableSupportContentTitles,
                availableBelongingNames,
                availableAgendaTitles);
            AddExternalLanguageErrors(selection, response, validationErrors);
            AddProposalCandidateErrors(context, response, validationErrors);
            AddConversationalLanguageErrors(
                selection, response, validationErrors);
            if (validationErrors.Count > 0)
            {
                _logger.LogWarning(
                    "OpenAI no produjo una respuesta limitada al contexto " +
                    "seleccionado después del reintento: {Errors}",
                    string.Join(" | ", validationErrors));
                response = new AssistantResponse
                {
                    Message = !string.IsNullOrWhiteSpace(focusedPersonName)
                        ? $"Quiero asegurarme de haberte entendido bien sobre " +
                          $"{focusedPersonName}. ¿Podés repetírmelo?"
                        : "Quiero asegurarme de haberte entendido bien. " +
                          "¿Podés repetírmelo?"
                };
            }
        }
        response.OfferedAction = ValidateOfferedAction(
            response.OfferedAction,
            lifeEvents,
            preferences,
            supportContents);
        if (ConsumedImmediateCategory(context))
        {
            response.OfferedAction = new DialogueOffer();
        }

        if (conversationId is not null)
        {
            _conversationStore.AddExchange(
                conversationId.Value,
                userInput,
                response.Message);
            _conversationStore.SetWaitingForCompanionProposalChoice(
                conversationId.Value,
                proposalMode == CompanionProposalMode.CategoriesOnly,
                context.MemorySupport?.ProposalCandidates
                    .Select(item => item.TopicCode));
            var offersToStore = proposalMode ==
                CompanionProposalMode.CategoriesOnly &&
                context.MemorySupport?.ProposalCandidates.Count > 1
                ? context.MemorySupport.ProposalCandidates.Select(candidate =>
                    new DialogueOffer
                    {
                        Type = DialogueOfferType.Category,
                        CategoryCode = candidate.TopicCode
                    })
                : response.OfferedAction.Type == DialogueOfferType.None
                    ? []
                    : [response.OfferedAction];
            _conversationStore.SetPendingOffers(
                conversationId.Value, offersToStore);
            _conversationStore.SetFocusedPerson(
                conversationId.Value, focusedPersonName);
            var acceptedPendingMemory =
                pendingOffer?.Type == DialogueOfferType.Category &&
                ConsumedImmediateCategory(context);
            var directMemoryRequest = pendingOffer?.Type !=
                DialogueOfferType.Category;
            if (selectedMemory is not null &&
                (acceptedPendingMemory || directMemoryRequest))
            {
                _conversationStore.AddRecentMemory(
                    conversationId.Value,
                    selectedMemory.LifeEvent.Id,
                    selectedMemory.ResetCycle);
            }
        }

        return response;
    }

    private static bool MustOfferSuggestedContent(
        ConversationContext context,
        AssistantResponse response)
    {
        var pending = context.State.PendingOffer;
        return pending?.Type == DialogueOfferType.Category &&
            !string.IsNullOrWhiteSpace(pending.SuggestedContentTitle) &&
            response.PendingOfferDisposition ==
                PendingOfferDisposition.Accepted &&
            (response.OfferedAction.Type != DialogueOfferType.Content ||
                !string.Equals(
                    response.OfferedAction.ContentTitle,
                    pending.SuggestedContentTitle,
                    StringComparison.OrdinalIgnoreCase));
    }

    private static void AddExternalLanguageErrors(
        ConversationContextSelection selection,
        AssistantResponse response,
        IReadOnlyCollection<string> currentErrors)
    {
        if (!selection.Kinds.Contains(ConversationContextKind.External) ||
            currentErrors is not List<string> errors)
        {
            return;
        }

        if (response.Message.Contains("contexto",
                StringComparison.OrdinalIgnoreCase) ||
            response.Message.Contains("JSON",
                StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Una respuesta externa no debe mencionar el contexto.");
        }
    }

    private static void AddProposalCandidateErrors(
        ConversationContext context,
        AssistantResponse response,
        IReadOnlyCollection<string> currentErrors)
    {
        if (currentErrors is not List<string> errors ||
            context.MemorySupport?.ProposalCandidates is not { Count: > 0 }
                candidates)
        {
            return;
        }

        foreach (var candidate in candidates)
        {
            if (!CompanionProposalModeDetector.MentionsCategory(
                    response.Message, candidate.TopicCode))
            {
                errors.Add("La respuesta omitió una categoría propuesta: " +
                    candidate.TopicCode + ".");
            }
        }
    }

    private static void AddConversationalLanguageErrors(
        ConversationContextSelection selection,
        AssistantResponse response,
        IReadOnlyCollection<string> currentErrors)
    {
        if (currentErrors is not List<string> errors) return;

        string[] limitationPhrases =
        [
            "no tengo más datos",
            "no tengo más información",
            "no cuento con más datos",
            "es lo único que sé",
            "es la única información"
        ];
        if (limitationPhrases.Any(phrase => response.Message.Contains(
                phrase, StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add("No enfatices limitaciones internas ni digas que no " +
                "hay más datos sobre una persona conocida.");
        }

        var relationshipOnly = selection.Kinds.Count == 1 &&
            selection.Includes(ConversationContextKind.Relationship);
        if (relationshipOnly && Regex.IsMatch(response.Message,
                @"\b(mensaje|escribirle|llamarlo|llamarla|contactarlo|contactarla)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            errors.Add("Una consulta informativa sobre una relación no debe " +
                "convertirse en una sugerencia de contacto.");
        }
    }

    private static void AddRequiredEvidenceErrors(
        ConversationContextSelection selection,
        ConversationContext context,
        AssistantResponse response,
        IReadOnlyCollection<string> currentErrors)
    {
        if (currentErrors is not List<string> errors ||
            context.MemorySupport is not { } memory)
        {
            return;
        }

        RequireEvidence(selection, ConversationContextKind.Routine,
            memory.Routines.Count, response.Evidence.RoutineTitles.Count,
            "rutina", errors);
        RequireEvidence(selection, ConversationContextKind.Memory,
            memory.LifeEvents.Count, response.Evidence.LifeEventTitles.Count,
            "recuerdo", errors);
        RequireEvidence(selection, ConversationContextKind.Preference,
            memory.Preferences.Count, response.Evidence.PreferenceTitles.Count,
            "preferencia", errors);
        RequireEvidence(selection, ConversationContextKind.Reading,
            memory.SupportContents.Count,
            response.Evidence.SupportContentTitles.Count,
            "lectura", errors);
        RequireEvidence(selection, ConversationContextKind.Belonging,
            memory.Belongings.Count, response.Evidence.BelongingNames.Count,
            "objeto", errors);
        RequireEvidence(selection, ConversationContextKind.Agenda,
            memory.Agenda.Count, response.Evidence.AgendaTitles.Count,
            "agenda", errors);
    }

    private static void AddExcludedContextErrors(
        ConversationContext context,
        AssistantResponse response,
        IReadOnlyCollection<string> currentErrors,
        IReadOnlyCollection<string> routineTitles,
        IReadOnlyCollection<string> lifeEventTitles,
        IReadOnlyCollection<string> preferenceTitles,
        IReadOnlyCollection<string> supportContentTitles,
        IReadOnlyCollection<string> belongingNames,
        IReadOnlyCollection<string> agendaTitles)
    {
        if (currentErrors is not List<string> errors ||
            context.MemorySupport is not { } memory)
        {
            return;
        }

        RejectExcludedTitles(response.Message, routineTitles,
            memory.Routines.Select(item => item.Title), "rutina", errors);
        RejectExcludedTitles(response.Message, lifeEventTitles,
            memory.LifeEvents.Select(item => item.Title), "recuerdo", errors);
        RejectExcludedTitles(response.Message, preferenceTitles,
            memory.Preferences.Select(item => item.Title), "preferencia", errors);
        RejectExcludedTitles(response.Message, supportContentTitles,
            memory.SupportContents.Select(item => item.Title), "lectura", errors);
        RejectExcludedTitles(response.Message, belongingNames,
            memory.Belongings.Select(item => item.Name), "objeto", errors);
        RejectExcludedTitles(response.Message, agendaTitles,
            memory.Agenda.Select(item => item.Title), "agenda", errors);
    }

    private static void RejectExcludedTitles(
        string message,
        IEnumerable<string> available,
        IEnumerable<string> selected,
        string label,
        ICollection<string> errors)
    {
        var selectedSet = selected.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var title in available.Where(title =>
                     title.Length >= 6 && !selectedSet.Contains(title)))
        {
            if (message.Contains(title, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"La respuesta reutiliza una {label} excluida: " +
                    $"{title}.");
            }
        }
    }

    private static void RequireEvidence(
        ConversationContextSelection selection,
        ConversationContextKind kind,
        int availableCount,
        int evidenceCount,
        string label,
        ICollection<string> errors)
    {
        if (selection.Includes(kind) && availableCount > 0 &&
            evidenceCount == 0)
        {
            errors.Add($"La respuesta debe usar evidencia de {label}.");
        }
    }

    private static bool MustClarifyMultipleOffers(
        string? selectedProposalCategory,
        IReadOnlyCollection<DialogueOffer>? pendingOffers,
        AssistantResponse response) =>
        string.IsNullOrWhiteSpace(selectedProposalCategory) &&
        pendingOffers?.Count > 1 &&
        response.PendingOfferDisposition == PendingOfferDisposition.Accepted;

    private static bool MustContinueWithAcceptedCategory(
        ConversationContext context,
        AssistantResponse response)
    {
        var pending = context.State.PendingOffer;
        return pending?.Type == DialogueOfferType.Category &&
            IsImmediateCategory(pending.CategoryCode) &&
            response.PendingOfferDisposition ==
                PendingOfferDisposition.Accepted;
    }

    private static bool ConsumedImmediateCategory(
        ConversationContext context) =>
        context.State.RequiredDialogueAction ==
            RequiredDialogueAction.ContinueWithAcceptedCategory;

    private static bool IsImmediateCategory(string? categoryCode) =>
        categoryCode?.StartsWith("memory.",
            StringComparison.OrdinalIgnoreCase) == true ||
        categoryCode?.StartsWith("interest.",
            StringComparison.OrdinalIgnoreCase) == true;

    private static bool MentionsAssistedPersonName(
        string message,
        string personName)
    {
        var firstName = personName.Split(' ',
            StringSplitOptions.RemoveEmptyEntries)[0];
        return Regex.IsMatch(
            message,
            $@"\b{Regex.Escape(firstName)}\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static bool HasLexicalRoutineMatch(
        string userInput,
        IReadOnlyCollection<PersonRoutineResponse> routines)
    {
        var inputTokens = NormalizeTokens(userInput);
        if (inputTokens.Count == 0) return false;
        return routines.Any(routine =>
        {
            var routineTokens = NormalizeTokens(
                $"{routine.Title} {routine.Notes}");
            return inputTokens.Overlaps(routineTokens);
        });
    }

    private static List<T> FilterByPerson<T>(
        IEnumerable<T> items,
        string? focusedPersonName,
        Func<T, string> getPersonName) => items
        .Where(item => focusedPersonName is null || getPersonName(item).Equals(
            focusedPersonName,
            StringComparison.OrdinalIgnoreCase))
        .ToList();

    private static HashSet<string> NormalizeTokens(string value)
    {
        var decomposed = value.ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) ==
                UnicodeCategory.NonSpacingMark)
            {
                continue;
            }
            builder.Append(char.IsLetterOrDigit(character) ? character : ' ');
        }
        return builder.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length >= 4)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static DialogueOffer ValidateOfferedAction(
        DialogueOffer offer,
        IReadOnlyCollection<Puentes.Shared.Responses.LifeEvents.LifeEventResponse>
            lifeEvents,
        IReadOnlyCollection<Puentes.Shared.Responses.People.PersonPreferenceResponse>
            preferences,
        IReadOnlyCollection<Puentes.Shared.Responses.People.PersonSupportContentResponse>
            supportContents)
    {
        if (offer.Type == DialogueOfferType.Category &&
            !string.IsNullOrWhiteSpace(offer.CategoryCode))
        {
            var knownCode = lifeEvents.SelectMany(item => item.TopicCodes)
                .Concat(preferences.SelectMany(item => item.TopicCodes))
                .Concat(supportContents.SelectMany(item => item.TopicCodes))
                .FirstOrDefault(code => code.Equals(
                    offer.CategoryCode,
                    StringComparison.OrdinalIgnoreCase));
            if (knownCode is not null)
            {
                return new DialogueOffer
                {
                    Type = DialogueOfferType.Category,
                    CategoryCode = knownCode
                };
            }
        }

        if (offer.Type == DialogueOfferType.Content &&
            !string.IsNullOrWhiteSpace(offer.ContentTitle))
        {
            var content = supportContents.FirstOrDefault(item =>
                item.IsActive && item.Title.Equals(
                    offer.ContentTitle,
                    StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrWhiteSpace(offer.CategoryCode) ||
                    item.TopicCodes.Contains(
                        offer.CategoryCode,
                        StringComparer.OrdinalIgnoreCase)));
            if (content is not null)
            {
                return new DialogueOffer
                {
                    Type = DialogueOfferType.Content,
                    CategoryCode = content.TopicCodes.FirstOrDefault(code =>
                        string.IsNullOrWhiteSpace(offer.CategoryCode) ||
                        code.Equals(offer.CategoryCode,
                            StringComparison.OrdinalIgnoreCase)),
                    ContentTitle = content.Title
                };
            }
        }

        return new DialogueOffer();
    }
}
