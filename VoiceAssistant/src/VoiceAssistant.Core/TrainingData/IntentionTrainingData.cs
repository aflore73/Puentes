using VoiceAssistant.Core.Interfaces;

namespace VoiceAssistant.Core.Training;

public static class IntentionTrainingData
{
    public static IEnumerable<TrainingData> GetTrainingData()
    {
        return new List<TrainingData>
        {
            // MUSICA
            new TrainingData { Text = "quiero escuchar a sandro", Label = "MUSICA" },
            new TrainingData { Text = "quisiera escuchar a sandro", Label = "MUSICA" },
            new TrainingData { Text = "me gustaria escuchar a sandro", Label = "MUSICA" },
            new TrainingData { Text = "escuchar a sandro", Label = "MUSICA" },
            new TrainingData { Text = "escucar a sandro", Label = "MUSICA" },
            new TrainingData { Text = "pon sandro", Label = "MUSICA" },
            new TrainingData { Text = "poner sandro", Label = "MUSICA" },
            new TrainingData { Text = "pon musica de sandro", Label = "MUSICA" },
            new TrainingData { Text = "reproduce sandro", Label = "MUSICA" },
            new TrainingData { Text = "toca sandro", Label = "MUSICA" },
            new TrainingData { Text = "play sandro", Label = "MUSICA" },
            new TrainingData { Text = "quiero oir sandro", Label = "MUSICA" },
            new TrainingData { Text = "escuchar algo de sandro", Label = "MUSICA" },
            new TrainingData { Text = "quiero musica de sandro", Label = "MUSICA" },
            new TrainingData { Text = "quiero escuchar musica", Label = "MUSICA" },
            new TrainingData { Text = "pon musica", Label = "MUSICA" },
            new TrainingData { Text = "reproduce musica", Label = "MUSICA" },
            new TrainingData { Text = "escuchar musica", Label = "MUSICA" },
            new TrainingData { Text = "pon tango", Label = "MUSICA" },
            new TrainingData { Text = "escuchar tango", Label = "MUSICA" },
            new TrainingData { Text = "musica de gardel", Label = "MUSICA" },
            new TrainingData { Text = "pon gardel", Label = "MUSICA" },
            new TrainingData { Text = "escuchar algo", Label = "MUSICA" },
            new TrainingData { Text = "pon algo", Label = "MUSICA" },
            new TrainingData { Text = "play musica", Label = "MUSICA" },
            new TrainingData { Text = "escuchame algo", Label = "MUSICA" },
            new TrainingData { Text = "ponme musica", Label = "MUSICA" },

            // LISTAR_MUSICA
            new TrainingData { Text = "que tenes de musica", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "que hay de musica", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "que musica tenes", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "que canciones tenes", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "mostrame canciones", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "lista de canciones", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "ver canciones", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "que tenes de sandro", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "que hay de sandro", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "que tenes para escuchar", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "canciones disponibles", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "tenes algo de sandro", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "tenes sandro", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "tenes tango", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "tenes musica", Label = "LISTAR_MUSICA" },
            new TrainingData { Text = "tenes canciones", Label = "LISTAR_MUSICA" },

            // DETENER
            new TrainingData { Text = "para la musica", Label = "DETENER" },
            new TrainingData { Text = "deten la musica", Label = "DETENER" },
            new TrainingData { Text = "basta de musica", Label = "DETENER" },
            new TrainingData { Text = "apaga la musica", Label = "DETENER" },
            new TrainingData { Text = "corta la musica", Label = "DETENER" },
            new TrainingData { Text = "silencio", Label = "DETENER" },
            new TrainingData { Text = "basta", Label = "DETENER" },
            new TrainingData { Text = "para", Label = "DETENER" },

            // SIGUIENTE
            new TrainingData { Text = "siguiente cancion", Label = "SIGUIENTE" },
            new TrainingData { Text = "otra cancion", Label = "SIGUIENTE" },
            new TrainingData { Text = "cambia de cancion", Label = "SIGUIENTE" },
            new TrainingData { Text = "pon otra", Label = "SIGUIENTE" },
            new TrainingData { Text = "siguiente", Label = "SIGUIENTE" },
            new TrainingData { Text = "otra", Label = "SIGUIENTE" },

            // PERSONA
            new TrainingData { Text = "no se nada de alejandro", Label = "PERSONA" },
            new TrainingData { Text = "de alejandro no se nada", Label = "PERSONA" },
            new TrainingData { Text = "donde esta alejandro", Label = "PERSONA" },
            new TrainingData { Text = "que hace alejandro", Label = "PERSONA" },
            new TrainingData { Text = "rutinas de alejandro", Label = "PERSONA" },
            new TrainingData { Text = "alejandro no me contesta", Label = "PERSONA" },
            new TrainingData { Text = "no se nada de ezequiel", Label = "PERSONA" },
            new TrainingData { Text = "donde esta ezequiel", Label = "PERSONA" },
            new TrainingData { Text = "que hace ezequiel", Label = "PERSONA" },
            new TrainingData { Text = "no se nada de equiel", Label = "PERSONA" },
            new TrainingData { Text = "ezequiel no vino", Label = "PERSONA" },
            new TrainingData { Text = "alejandro no vino", Label = "PERSONA" },
            new TrainingData { Text = "donde esta ana", Label = "PERSONA" },
            new TrainingData { Text = "que hace marta", Label = "PERSONA" },
            new TrainingData { Text = "no se nada de ana", Label = "PERSONA" },
            new TrainingData { Text = "no se nada de marta", Label = "PERSONA" },

            // EVENTO
            new TrainingData { Text = "tengo una reunion maÃ±ana", Label = "EVENTO" },
            new TrainingData { Text = "tengo cita con el medico", Label = "EVENTO" },
            new TrainingData { Text = "cuando fui al doctor", Label = "EVENTO" },
            new TrainingData { Text = "que eventos tengo", Label = "EVENTO" },
            new TrainingData { Text = "tengo turno medico", Label = "EVENTO" },
            new TrainingData { Text = "que tengo en la agenda", Label = "EVENTO" },

            // RECUERDO
            new TrainingData { Text = "recuerdo cuando era niÃ±o", Label = "RECUERDO" },
            new TrainingData { Text = "me acuerdo de mi primer trabajo", Label = "RECUERDO" },
            new TrainingData { Text = "cuando era pequeÃ±o", Label = "RECUERDO" },
            new TrainingData { Text = "recuerdo mi infancia", Label = "RECUERDO" },
            new TrainingData { Text = "me acuerdo de", Label = "RECUERDO" },

            // RUTINA
            new TrainingData { Text = "todos los dias hago ejercicio", Label = "RUTINA" },
            new TrainingData { Text = "siempre tomo cafe", Label = "RUTINA" },
            new TrainingData { Text = "mi rutina incluye", Label = "RUTINA" },
            new TrainingData { Text = "cada noche medito", Label = "RUTINA" },
            new TrainingData { Text = "rutinas", Label = "RUTINA" },

            // TAREA
            new TrainingData { Text = "necesito completar", Label = "TAREA" },
            new TrainingData { Text = "tengo que comprar", Label = "TAREA" },
            new TrainingData { Text = "debo llamar", Label = "TAREA" },
            new TrainingData { Text = "tengo pendiente", Label = "TAREA" },
            new TrainingData { Text = "tareas", Label = "TAREA" },

            // MEDICINA
            new TrainingData { Text = "necesito tomar mi medicamento", Label = "MEDICINA" },
            new TrainingData { Text = "tengo que tomar la pastilla", Label = "MEDICINA" },
            new TrainingData { Text = "que medicina tomo", Label = "MEDICINA" },
            new TrainingData { Text = "medicamentos", Label = "MEDICINA" },
            new TrainingData { Text = "pastillas", Label = "MEDICINA" },

            // NOTA
            new TrainingData { Text = "el cielo esta nublado", Label = "NOTA" },
            new TrainingData { Text = "hace calor", Label = "NOTA" },
            new TrainingData { Text = "que dia es hoy", Label = "NOTA" },
            new TrainingData { Text = "que hora es", Label = "NOTA" }
        };
    }
}