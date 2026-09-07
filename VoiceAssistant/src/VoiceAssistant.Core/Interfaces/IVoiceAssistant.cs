namespace VoiceAssistant.Core.Interfaces;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<string> GetContextForCategoryAsync(string category);
    Task<string> GetPersonRoutineContextAsync(string personName);
    Task<string> GetBelongingContextAsync(string searchTerm);
    Task<List<string>> GetAllPersonNamesAsync();
}

public record TrainingData
{
    public string Text { get; init; } = "";
    public string Label { get; init; } = "";
}