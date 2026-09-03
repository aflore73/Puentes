namespace VoiceAssistant.Core.Interfaces;

public interface IVoiceAssistant
{
    Task<AssistantResponse> ProcessTextInput(string text);
}

public record AssistantResponse
{
    public string Text { get; init; } = "";
    public string Category { get; init; } = "";
    public float Confidence { get; init; }
}

public interface ITextClassifier
{
    Task<ClassificationResult> ClassifyAsync(string text);
    void TrainModel(IEnumerable<TrainingData> trainingData);
    void SaveModel(string path);
    void LoadModel(string path);
}

public record ClassificationResult
{
    public string Category { get; init; } = "";
    public float Confidence { get; init; }
}

public record TrainingData
{
    public string Text { get; init; } = "";
    public string Label { get; init; } = "";
}

public interface IOllamaService
{
    Task<OllamaResult> GenerateResponseAsync(string text, string category, string context);
}

public record OllamaResult
{
    public string Text { get; init; } = "";
    public string Category { get; init; } = "";
}

public interface ITextToSpeechService
{
    void Speak(string text);
}

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<string> GetContextForCategoryAsync(string category);
    Task<string> GetPersonRoutineContextAsync(string personName);
    Task<List<string>> GetAllPersonNamesAsync();
    Task SaveConversationAsync(string userInput, string assistantResponse, string category, double confidence);
}

public record AssistantInteraction
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Input { get; init; } = "";
    public string Output { get; init; } = "";
    public string Category { get; init; } = "";
}
