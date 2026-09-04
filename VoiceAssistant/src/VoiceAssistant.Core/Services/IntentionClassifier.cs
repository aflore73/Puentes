using Microsoft.ML;
using VoiceAssistant.Core.Training;

namespace VoiceAssistant.Core.Services;

public class IntentionClassifier
{
    private readonly MLContext _mlContext;
    private ITransformer _model;
    private PredictionEngine<IntentionData, IntentionPrediction> _predictionEngine;

    public IntentionClassifier()
    {
        _mlContext = new MLContext();
        TrainModel();
    }

    private void TrainModel()
    {
        var trainingData = IntentionTrainingData.GetTrainingData()
            .Select(d => new IntentionData
            {
                Text = d.Text,
                Label = d.Label
            });

        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
            .Append(_mlContext.Transforms.Text.FeaturizeText("Features", "Text"))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        _model = pipeline.Fit(dataView);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<IntentionData, IntentionPrediction>(_model);
    }

    public IntentionPrediction Predict(string text)
    {
        return _predictionEngine.Predict(new IntentionData { Text = text });
    }

    private class IntentionData
    {
        public string Text { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class IntentionPrediction
    {
        public string PredictedLabel { get; set; } = string.Empty;
        public float[] Score { get; set; } = Array.Empty<float>();
    }
}