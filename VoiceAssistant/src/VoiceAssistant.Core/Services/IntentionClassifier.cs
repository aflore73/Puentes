using Microsoft.ML;
using VoiceAssistant.Core.Interfaces;

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
        var trainingData = new List<IntentionData>
        {
            // MUSICA - Reproducir
            new IntentionData { Text = "quiero escuchar a sandro", Label = "MUSICA" },
            new IntentionData { Text = "quisiera escuchar a sandro", Label = "MUSICA" },
            new IntentionData { Text = "me gustaria escuchar a sandro", Label = "MUSICA" },
            new IntentionData { Text = "escuchar a sandro", Label = "MUSICA" },
            new IntentionData { Text = "escucar a sandro", Label = "MUSICA" },
            new IntentionData { Text = "pon sandro", Label = "MUSICA" },
            new IntentionData { Text = "poner sandro", Label = "MUSICA" },
            new IntentionData { Text = "pon musica de sandro", Label = "MUSICA" },
            new IntentionData { Text = "reproduce sandro", Label = "MUSICA" },
            new IntentionData { Text = "toca sandro", Label = "MUSICA" },
            new IntentionData { Text = "play sandro", Label = "MUSICA" },
            new IntentionData { Text = "quiero oir sandro", Label = "MUSICA" },
            new IntentionData { Text = "escuchar algo de sandro", Label = "MUSICA" },
            new IntentionData { Text = "quiero musica de sandro", Label = "MUSICA" },
            new IntentionData { Text = "quiero escuchar musica", Label = "MUSICA" },
            new IntentionData { Text = "pon musica", Label = "MUSICA" },
            new IntentionData { Text = "reproduce musica", Label = "MUSICA" },
            new IntentionData { Text = "escuchar musica", Label = "MUSICA" },
            new IntentionData { Text = "pon tango", Label = "MUSICA" },
            new IntentionData { Text = "escuchar tango", Label = "MUSICA" },
            new IntentionData { Text = "musica de gardel", Label = "MUSICA" },
            new IntentionData { Text = "pon gardel", Label = "MUSICA" },
            
            // LISTAR_MUSICA
            new IntentionData { Text = "que tenes de musica", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "que hay de musica", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "que tienes de musica", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "que musica tenes", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "que canciones tenes", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "mostrame canciones", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "lista de canciones", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "ver canciones", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "que tenes de sandro", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "que hay para escuchar", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "canciones disponibles", Label = "LISTAR_MUSICA" },
            
            // DETENER
            new IntentionData { Text = "para la musica", Label = "DETENER" },
            new IntentionData { Text = "deten la musica", Label = "DETENER" },
            new IntentionData { Text = "basta de musica", Label = "DETENER" },
            new IntentionData { Text = "apaga la musica", Label = "DETENER" },
            new IntentionData { Text = "corta la musica", Label = "DETENER" },
            new IntentionData { Text = "silencio", Label = "DETENER" },
            new IntentionData { Text = "basta", Label = "DETENER" },
            new IntentionData { Text = "para", Label = "DETENER" },
            
            // SIGUIENTE
            new IntentionData { Text = "siguiente cancion", Label = "SIGUIENTE" },
            new IntentionData { Text = "otra cancion", Label = "SIGUIENTE" },
            new IntentionData { Text = "cambia de cancion", Label = "SIGUIENTE" },
            new IntentionData { Text = "pon otra", Label = "SIGUIENTE" },
            new IntentionData { Text = "siguiente", Label = "SIGUIENTE" },
            new IntentionData { Text = "otra", Label = "SIGUIENTE" },
            
            new IntentionData { Text = "que musica tienes", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "tenes algo de sandro", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "tienes algo de sandro", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "tenes sandro", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "tienes sandro", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "hay algo de sandro", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "tenes tango", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "tienes tango", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "hay tango", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "tenes musica", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "tienes musica", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "que tenes para oir", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "que hay para oir", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "tenes algo para escuchar", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "hay algo para escuchar", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "que tienes para escuchar", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "tenes canciones", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "tienes canciones", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "hay canciones", Label = "LISTAR_MUSICA" },
            new IntentionData { Text = "tenes algo de musica", Label = "LISTAR_MUSICA" },            // PERSONA
            new IntentionData { Text = "no se nada de alejandro", Label = "PERSONA" },
            new IntentionData { Text = "donde esta alejandro", Label = "PERSONA" },
            new IntentionData { Text = "que hace alejandro", Label = "PERSONA" },
            new IntentionData { Text = "rutinas de alejandro", Label = "PERSONA" },
            new IntentionData { Text = "no se nada de ezequiel", Label = "PERSONA" },
            new IntentionData { Text = "donde esta ezequiel", Label = "PERSONA" },
            new IntentionData { Text = "que hace ezequiel", Label = "PERSONA" },
            new IntentionData { Text = "no se nada de equiel", Label = "PERSONA" },
            new IntentionData { Text = "ezequiel no vino", Label = "PERSONA" },
            
            // EVENTO
            new IntentionData { Text = "tengo una reunion maÃ±ana", Label = "EVENTO" },
            new IntentionData { Text = "tengo cita con el medico", Label = "EVENTO" },
            new IntentionData { Text = "que eventos tengo", Label = "EVENTO" },
            
            // RECUERDO
            new IntentionData { Text = "recuerdo cuando era niÃ±o", Label = "RECUERDO" },
            new IntentionData { Text = "me acuerdo de mi primer trabajo", Label = "RECUERDO" },
            
            // RUTINA
            new IntentionData { Text = "todos los dias hago ejercicio", Label = "RUTINA" },
            new IntentionData { Text = "siempre tomo cafe", Label = "RUTINA" },
            
            // TAREA
            new IntentionData { Text = "necesito completar", Label = "TAREA" },
            new IntentionData { Text = "tengo que comprar", Label = "TAREA" },
            
            // MEDICINA
            new IntentionData { Text = "necesito tomar mi medicamento", Label = "MEDICINA" },
            new IntentionData { Text = "tengo que tomar la pastilla", Label = "MEDICINA" },
            
            // NOTA
            new IntentionData { Text = "el cielo esta nublado", Label = "NOTA" },
            new IntentionData { Text = "hace calor", Label = "NOTA" }
        };

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