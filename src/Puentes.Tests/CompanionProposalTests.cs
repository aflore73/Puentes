using Puentes.Orchestrator.AI.Models;
using Puentes.Orchestrator.AI.Prompts;
using Puentes.Orchestrator.Services;
using Puentes.Shared.Domain;
using Puentes.Shared.Responses.LifeEvents;
using Puentes.Shared.Responses.People;

namespace Puentes.Tests;

public sealed class CompanionProposalTests
{
    [Theory]
    [InlineData("Estoy triste")]
    [InlineData("Me siento sola")]
    [InlineData("Estoy aburrida")]
    [InlineData("Quiero hablar con alguien")]
    public void DetectsCompanionshipRequests(string userInput)
    {
        Assert.Equal(
            CompanionProposalMode.CategoriesOnly,
            CompanionProposalModeDetector.Resolve(userInput, false));
    }

    [Theory]
    [InlineData("Quiero una lectura", CompanionProposalMode.ReadingsOnly)]
    [InlineData("Recordemos algo lindo", CompanionProposalMode.PositiveMemoriesOnly)]
    [InlineData("Hablemos de música", CompanionProposalMode.InterestsOnly)]
    public void DetectsSelectedCategory(
        string userInput,
        CompanionProposalMode expected)
    {
        Assert.Equal(
            expected,
            CompanionProposalModeDetector.Resolve(userInput, true));
    }

    [Fact]
    public void PendingMemoryCategoryDeterminesModeForBriefAnswer()
    {
        Assert.Equal(
            CompanionProposalMode.PositiveMemoriesOnly,
            CompanionProposalModeDetector.Resolve(
                "Respuesta afirmativa de prueba",
                waitingForChoice: false,
                selectedCategory: "memory.family"));
    }

    [Fact]
    public void CategoryOfferContainsOnlyGroupedCategoryNames()
    {
        var context = BuildContext(CompanionProposalMode.CategoriesOnly);
        var memory = context.MemorySupport!;

        Assert.Empty(memory.LifeEvents);
        Assert.Empty(memory.Preferences);
        Assert.Empty(memory.SupportContents);
        Assert.Empty(memory.Relationships);
        Assert.Equal(3, memory.ProposalCandidates.Count);
        var topicCodes = memory.ProposalCandidates
            .Select(item => item.TopicCode).ToArray();
        Assert.Contains("interest.music", topicCodes);
        Assert.Contains("memory.travel", topicCodes);
        Assert.Single(topicCodes, code => code.StartsWith("reading."));
        Assert.All(topicCodes, code => Assert.Contains(code,
            new[] { "interest.music", "memory.travel",
                "reading.religious", "reading.poetry" }));

        var userMessage = new PromptFactory().Create(context).UserMessage;
        Assert.DoesNotContain("Sandro", userMessage);
        Assert.DoesNotContain("Salmos 23", userMessage);
        Assert.DoesNotContain("Vacaciones en Mar del Plata", userMessage);
    }

    [Fact]
    public void SelectedConcreteCategoryFiltersItsContent()
    {
        var selected = CompanionProposalModeDetector.FindSelectedCategory(
            "Prefiero una lectura religiosa",
            ["reading.religious", "interest.music", "memory.travel"]);

        Assert.Equal("reading.religious", selected);
        var memory = BuildContext(
            CompanionProposalMode.ReadingsOnly,
            selected).MemorySupport!;
        var reading = Assert.Single(memory.SupportContents);
        Assert.Contains("reading.religious", reading.TopicCodes);
        Assert.Equal("Salmos 23", reading.Title);
    }

    [Fact]
    public void SelectedInterestContainsOnlyPreferenceDetails()
    {
        var memory = BuildContext(
            CompanionProposalMode.InterestsOnly).MemorySupport!;

        Assert.Single(memory.Preferences);
        Assert.Empty(memory.ProposalCandidates);
        Assert.Empty(memory.LifeEvents);
        Assert.Empty(memory.SupportContents);
        Assert.Empty(memory.Routines);
    }

    [Fact]
    public void SelectedMemoryCategoryContainsOnlyMatchingPositiveMemories()
    {
        LifeEventResponse[] memories =
        [
            new()
            {
                PersonName = "Marta",
                Title = "Pesca con Vicente",
                TopicCodes = ["memory.family"],
                IsPositiveMemory = true
            },
            new()
            {
                PersonName = "Marta",
                Title = "Vacaciones",
                TopicCodes = ["memory.travel"],
                IsPositiveMemory = true
            }
        ];
        var context = new AiContextBuilderService().BuildConversationContext(
            new ConversationRequest
            {
                Scenario = ConversationScenario.MemorySupport,
                UserInput = "Respuesta afirmativa de prueba"
            },
            lifeEvents: memories,
            person: new Person { Name = "Marta" },
            companionProposalMode: CompanionProposalMode.PositiveMemoriesOnly,
            companionProposalCategory: "memory.family",
            pendingOffer: new DialogueOffer
            {
                Type = DialogueOfferType.Category,
                CategoryCode = "memory.family"
            });

        var memory = Assert.Single(context.MemorySupport!.LifeEvents);
        Assert.Equal("Pesca con Vicente", memory.Title);
    }

    [Fact]
    public void PendingReadingCategorySelectsConfiguredContentCandidate()
    {
        PersonSupportContentResponse[] readings =
        [
            new()
            {
                Title = "Salmos 23",
                Reference = "Salmos 23:1-4",
                Content = "Texto uno.",
                TopicCodes = ["reading.religious"],
                IsActive = true
            },
            new()
            {
                Title = "Mateo 11",
                Reference = "Mateo 11:28-29",
                Content = "Texto dos.",
                TopicCodes = ["reading.religious"],
                IsActive = true
            }
        ];
        var context = new AiContextBuilderService().BuildConversationContext(
            new ConversationRequest
            {
                Scenario = ConversationScenario.MemorySupport,
                UserInput = "Respuesta afirmativa de prueba"
            },
            supportContents: readings,
            pendingOffer: new DialogueOffer
            {
                Type = DialogueOfferType.Category,
                CategoryCode = "reading.religious"
            });

        var pending = Assert.IsType<PendingOfferContext>(
            context.State.PendingOffer);
        Assert.Contains(pending.SuggestedContentTitle,
            readings.Select(item => item.Title));
        Assert.Contains(pending.SuggestedContentReference,
            readings.Select(item => item.Reference));
    }

    [Fact]
    public void RecentlyUsedReadingCategoryYieldsToOtherAvailableCategories()
    {
        var context = BuildContext(
            CompanionProposalMode.CategoriesOnly,
            recentProposalCategories: ["reading.religious"]);
        var codes = context.MemorySupport!.ProposalCandidates
            .Select(item => item.TopicCode)
            .ToArray();

        Assert.DoesNotContain("reading.religious", codes);
        Assert.Contains("interest.music", codes);
        Assert.Contains("memory.travel", codes);
    }

    private static ConversationContext BuildContext(
        CompanionProposalMode mode,
        string? category = null,
        IReadOnlyCollection<string>? recentProposalCategories = null)
    {
        var person = new Person { Name = "Marta" };
        var request = new ConversationRequest
        {
            Scenario = ConversationScenario.MemorySupport,
            UserInput = "Me siento sola."
        };
        PersonPreferenceResponse[] preferences =
        [
            new()
            {
                PersonName = "Marta",
                Title = "Música",
                Notes = "Le gusta Sandro.",
                TopicCodes = ["interest.music"],
                IsActive = true
            }
        ];
        PersonSupportContentResponse[] readings =
        [
            new()
            {
                PersonName = "Marta",
                Title = "Salmos 23",
                Content = "Texto de lectura.",
                TopicCodes = ["reading.religious"],
                IsActive = true
            },
            new()
            {
                PersonName = "Marta",
                Title = "Un poema",
                Content = "Texto de un poema.",
                TopicCodes = ["reading.poetry"],
                IsActive = true
            }
        ];
        LifeEventResponse[] memories =
        [
            new()
            {
                PersonName = "Marta",
                Title = "Vacaciones en Mar del Plata",
                TopicCodes = ["memory.travel"],
                Description = "Un recuerdo lindo.",
                IsPositiveMemory = true
            }
        ];

        return new AiContextBuilderService().BuildConversationContext(
            request,
            lifeEvents: memories,
            preferences: preferences,
            supportContents: readings,
            person: person,
            companionProposalMode: mode,
            companionProposalCategory: category,
            recentProposalCategories: recentProposalCategories);
    }
}
