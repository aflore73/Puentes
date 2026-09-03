using Microsoft.ML;
using VoiceAssistant.Core.Interfaces;

namespace VoiceAssistant.Core.Services;

public class TextClassifier : ITextClassifier
{
    private readonly MLContext _mlContext;
    private ITransformer _model;
    private PredictionEngine<TextData, TextPrediction> _predictionEngine;
    private bool _modelTrained = false;

    public TextClassifier()
    {
        _mlContext = new MLContext();
        TrainInitialModel();
    }

    private void TrainInitialModel()
    {
        var trainingData = new List<TrainingData>
        {
            // EVENTOS
            new TrainingData { Text = "Tengo una reunion manana", Label = "EVENTO" },
            new TrainingData { Text = "Mi cumpleanos es en mayo", Label = "EVENTO" },
            new TrainingData { Text = "Tengo cita con el dentista", Label = "EVENTO" },
            new TrainingData { Text = "Tengo turno con el medico", Label = "EVENTO" },
            new TrainingData { Text = "Fui al medico la semana pasada", Label = "EVENTO" },
            
            // RECUERDOS
            new TrainingData { Text = "Recuerdo cuando era nino", Label = "RECUERDO" },
            new TrainingData { Text = "Me acuerdo de mi primer trabajo", Label = "RECUERDO" },
            new TrainingData { Text = "En el pasado jugaba futbol", Label = "RECUERDO" },
            
            // RUTINAS
            new TrainingData { Text = "Todos los dias hago ejercicio", Label = "RUTINA" },
            new TrainingData { Text = "Siempre tomo cafe por la manana", Label = "RUTINA" },
            
            // TAREAS
            new TrainingData { Text = "Necesito completar el proyecto", Label = "TAREA" },
            new TrainingData { Text = "Tengo que comprar viveres", Label = "TAREA" },
            
            // MEDICINA
            new TrainingData { Text = "Necesito tomar mi medicamento", Label = "MEDICINA" },
            new TrainingData { Text = "Tengo que tomar la pastilla", Label = "MEDICINA" },
            
            // NOTAS
            new TrainingData { Text = "El cielo esta nublado", Label = "NOTA" },
            new TrainingData { Text = "El libro es muy bueno", Label = "NOTA" }
        };

        TrainModel(trainingData);
    }

    public void TrainModel(IEnumerable<TrainingData> trainingData)
    {
        var data = trainingData.Select(d => new TextData
        {
            Text = d.Text,
            Label = d.Label
        });

        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
            .Append(_mlContext.Transforms.Text.FeaturizeText("Features", "Text"))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        _model = pipeline.Fit(dataView);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<TextData, TextPrediction>(_model);
        _modelTrained = true;
    }

    public Task<ClassificationResult> ClassifyAsync(string text)
    {
        if (!_modelTrained || _predictionEngine == null)
        {
            return Task.FromResult(new ClassificationResult
            {
                Category = "NOTA",
                Confidence = 0.5f
            });
        }

        var prediction = _predictionEngine.Predict(new TextData { Text = text });
        
        return Task.FromResult(new ClassificationResult
        {
            Category = prediction.Category,
            Confidence = prediction.Confidence
        });
    }

    public void SaveModel(string path)
    {
        if (_model != null)
        {
            _mlContext.Model.Save(_model, null, path);
        }
    }

    public void LoadModel(string path)
    {
        if (File.Exists(path))
        {
            _model = _mlContext.Model.Load(path, out var schema);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<TextData, TextPrediction>(_model);
            _modelTrained = true;
        }
    }

    private class TextData
    {
        public string Text { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    private class TextPrediction
    {
        public string PredictedLabel { get; set; } = string.Empty;
        public float[] Score { get; set; } = Array.Empty<float>();

        public string Category => PredictedLabel;
        public float Confidence => Score.Length > 0 ? Score.Max() : 0.5f;
    }
}