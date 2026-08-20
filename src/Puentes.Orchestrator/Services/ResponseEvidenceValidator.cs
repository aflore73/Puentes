using Puentes.Orchestrator.AI.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Puentes.Orchestrator.Services;

public static class ResponseEvidenceValidator
{
    public static IReadOnlyList<string> Validate(
        ConversationContext context,
        AssistantResponse response,
        IReadOnlyCollection<string> knownPersonNames)
    {
        var errors = new List<string>();
        var memory = context.MemorySupport;
        if (memory is null) return errors;

        var allowedPeople = memory.Relationships
            .Select(item => item.OtherPersonName)
            .Append(context.Person.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        ValidatePersonNames(response.Evidence.PersonNames, allowedPeople,
            errors);
        ValidateValues(response.Evidence.RoutineTitles,
            memory.Routines.Select(item => item.Title), "routineTitle", errors);
        ValidateValues(response.Evidence.LifeEventTitles,
            memory.LifeEvents.Select(item => item.Title), "lifeEventTitle", errors);
        ValidateValues(response.Evidence.PreferenceTitles,
            memory.Preferences.Select(item => item.Title), "preferenceTitle", errors);
        ValidateValues(response.Evidence.SupportContentTitles,
            memory.SupportContents.Select(item => item.Title),
            "supportContentTitle", errors);
        ValidateValues(response.Evidence.AgendaTitles,
            memory.Agenda.Select(item => item.Title), "agendaTitle", errors);
        ValidateValues(response.Evidence.BelongingNames,
            memory.Belongings.Select(item => item.Name), "belongingName", errors);

        foreach (var knownPersonName in knownPersonNames)
        {
            var firstName = knownPersonName.Split(' ',
                StringSplitOptions.RemoveEmptyEntries)[0];
            if (!Regex.IsMatch(response.Message,
                    $@"\b{Regex.Escape(firstName)}\b",
                    RegexOptions.IgnoreCase |
                    RegexOptions.CultureInvariant))
            {
                continue;
            }

            if (!allowedPeople.Contains(knownPersonName))
            {
                errors.Add($"La respuesta menciona una persona fuera del " +
                    $"contexto seleccionado: {knownPersonName}.");
            }
        }

        RequireMentionedTitles(response.Message,
            response.Evidence.RoutineTitles,
            memory.Routines.Select(item => item.Title), "rutina", errors);
        RequireMentionedTitles(response.Message,
            response.Evidence.LifeEventTitles,
            memory.LifeEvents.Select(item => item.Title), "suceso", errors);
        RequireMentionedTitles(response.Message,
            response.Evidence.PreferenceTitles,
            memory.Preferences.Select(item => item.Title), "preferencia", errors);
        RequireMentionedTitles(response.Message,
            response.Evidence.SupportContentTitles,
            memory.SupportContents.Select(item => item.Title), "lectura", errors);
        RequireMentionedTitles(response.Message,
            response.Evidence.AgendaTitles,
            memory.Agenda.Select(item => item.Title), "agenda", errors);
        RequireMentionedTitles(response.Message,
            response.Evidence.BelongingNames,
            memory.Belongings.Select(item => item.Name), "objeto", errors);

        RejectUnsupportedOvernightInference(response, errors);
        RejectPastRoutineInPresentTense(response, errors);
        RejectInventedContactPurpose(context, response, errors);

        if (response.OfferedAction.Type == DialogueOfferType.Content &&
            !string.IsNullOrWhiteSpace(response.OfferedAction.ContentTitle))
        {
            var offeredTitle = response.OfferedAction.ContentTitle;
            if (!response.Evidence.SupportContentTitles.Contains(
                    offeredTitle, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add("La lectura ofrecida no fue declarada en evidence.");
            }
            if (response.Evidence.SupportContentTitles.Any(title =>
                    !title.Equals(offeredTitle,
                        StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add("La respuesta usa más de una lectura concreta.");
            }
        }

        return errors;
    }

    private static void RejectUnsupportedOvernightInference(
        AssistantResponse response,
        ICollection<string> errors)
    {
        if (response.Evidence.RoutineTitles.Count == 0 ||
            response.Evidence.LifeEventTitles.Count > 0 ||
            response.Evidence.AgendaTitles.Count > 0)
        {
            return;
        }

        foreach (var sentence in Regex.Split(response.Message, @"[.!?]+"))
        {
            var normalized = RemoveDiacritics(sentence.ToLowerInvariant());
            var mentionsOvernightStay = Regex.IsMatch(normalized,
                @"\b(paso|pasado|pasar|durmio|dormir|quedo|quedarse|estuvo)\b.{0,45}\b(noche|afuera|fuera)\b") ||
                Regex.IsMatch(normalized,
                    @"\b(noche|afuera|fuera)\b.{0,45}\b(durmio|dormir|quedo|quedarse|estuvo)\b");
            if (!mentionsOvernightStay) continue;

            var statesThatItIsUnknown = Regex.IsMatch(normalized,
                @"\b(no tengo|no hay|no se sabe|no puedo saber|desconozco)\b");
            if (!statesThatItIsUnknown)
            {
                errors.Add("Una rutina no permite afirmar ni sugerir dónde " +
                    "durmió o pasó la noche una persona.");
                return;
            }
        }
    }

    private static void RejectPastRoutineInPresentTense(
        AssistantResponse response,
        ICollection<string> errors)
    {
        if (response.Evidence.RoutineTitles.Count == 0) return;

        foreach (var sentence in Regex.Split(response.Message, @"[.!?]+"))
        {
            var normalized = RemoveDiacritics(sentence.ToLowerInvariant());
            if (Regex.IsMatch(normalized, @"\b(ayer|anoche)\b") &&
                Regex.IsMatch(normalized, @"\bsuele\b"))
            {
                errors.Add("Una rutina referida a ayer o anoche debe " +
                    "expresarse como posibilidad pasada, no con 'suele'.");
                return;
            }
        }
    }

    private static void RejectInventedContactPurpose(
        ConversationContext context,
        AssistantResponse response,
        ICollection<string> errors)
    {
        var input = RemoveDiacritics(
            (context.UserInput ?? string.Empty).ToLowerInvariant());
        var requestedMessageHelp = Regex.IsMatch(input,
            @"\b(que le digo|que le escribo|como le escribo|ayudame.{0,20}mensaje|redactar.{0,20}mensaje)\b");
        if (requestedMessageHelp) return;

        var message = RemoveDiacritics(response.Message.ToLowerInvariant());
        var assignsPurpose = Regex.IsMatch(message,
            @"\b(escribirle|mandarle|enviarle|llamarlo|llamarla)\b.{0,35}\bpara\b") ||
            Regex.IsMatch(message,
                @"\bmensaje\b.{0,20}\bpara\b");
        if (assignsPurpose)
        {
            errors.Add("No inventes el contenido ni el propósito de un " +
                "mensaje o llamada que la persona no pidió redactar.");
        }
    }

    private static string RemoveDiacritics(string value)
    {
        var decomposed = value.Normalize(System.Text.NormalizationForm.FormD);
        return string.Concat(decomposed.Where(character =>
            CharUnicodeInfo.GetUnicodeCategory(character) !=
            UnicodeCategory.NonSpacingMark));
    }

    private static void ValidateValues(
        IEnumerable<string> actual,
        IEnumerable<string> allowed,
        string evidenceType,
        ICollection<string> errors)
    {
        var allowedSet = allowed.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var value in actual.Where(value =>
                     !string.IsNullOrWhiteSpace(value)))
        {
            if (!allowedSet.Contains(value))
            {
                errors.Add($"Evidencia {evidenceType} fuera del contexto: " +
                    $"{value}.");
            }
        }
    }

    private static void ValidatePersonNames(
        IEnumerable<string> actual,
        IReadOnlyCollection<string> allowed,
        ICollection<string> errors)
    {
        foreach (var value in actual.Where(value =>
                     !string.IsNullOrWhiteSpace(value)))
        {
            if (!allowed.Any(person => MatchesPerson(value, person)))
            {
                errors.Add($"Evidencia personName fuera del contexto: " +
                    $"{value}.");
            }
        }
    }

    private static bool MatchesPerson(string value, string personName)
    {
        var firstName = personName.Split(' ',
            StringSplitOptions.RemoveEmptyEntries)[0];
        return value.Equals(personName, StringComparison.OrdinalIgnoreCase) ||
            value.Equals(firstName, StringComparison.OrdinalIgnoreCase);
    }

    private static void RequireMentionedTitles(
        string message,
        IReadOnlyCollection<string> declared,
        IEnumerable<string> available,
        string evidenceType,
        ICollection<string> errors)
    {
        foreach (var title in available.Where(title =>
                     !string.IsNullOrWhiteSpace(title)))
        {
            if (message.Contains(title, StringComparison.OrdinalIgnoreCase) &&
                !declared.Contains(title, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"Falta declarar evidencia de {evidenceType}: " +
                    $"{title}.");
            }
        }
    }
}
