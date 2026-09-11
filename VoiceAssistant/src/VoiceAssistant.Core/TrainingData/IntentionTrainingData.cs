using VoiceAssistant.Core.Interfaces;

namespace VoiceAssistant.Core.Training;

public static class IntentionTrainingData
{
    public static IEnumerable<TrainingData> GetTrainingData()
    {
        return new List<TrainingData>
        {
            // OBJETO_PERDIDO
            new TrainingData { Text = "no encuentro las llaves", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "donde deje las llaves", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "no encuentro mi celular", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "donde esta mi celular", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "no encuentro la tarjeta", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "donde deje la tarjeta", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "perdi las llaves", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "perdi el celular", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "no encuentro mis llaves", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "donde estan las llaves", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "no se donde deje las llaves", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "no encuentro la billetera", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "donde puse las llaves", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "busco las llaves", Label = "OBJETO_PERDIDO" },
            new TrainingData { Text = "no aparece el celular", Label = "OBJETO_PERDIDO" },

            // ============================================
            // CONVERSACION - CANTANTES
            // ============================================
            new TrainingData { Text = "quien fue sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "quien es sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "contame de sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "que sabes de sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "quien fue gardel", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "contame de gardel", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "quien fue mercedes sosa", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "que sabes de mercedes sosa", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "quien es palito ortega", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "contame de los beatles", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "quien fue frank sinatra", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "que sabes de julio iglesias", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "quien fue edith piaf", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "contame de elvis", Label = "CONVERSACION_CANTANTES" },

            new TrainingData { Text = "decime cuando murio sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "decime de sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "decime quien fue sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "decime de gardel", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "decime quien fue gardel", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "decime sobre sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "decime algo de sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "decime algo de gardel", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "contame cuando murio sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "contame cuando nacio sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "sabes cuando murio sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "sabes de sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "conoces a sandro", Label = "CONVERSACION_CANTANTES" },
            new TrainingData { Text = "conoces a gardel", Label = "CONVERSACION_CANTANTES" },

            // ============================================
            // CONVERSACION - HISTORIA
            // ============================================
            new TrainingData { Text = "que paso en 1810", Label = "CONVERSACION_HISTORIA" },
            new TrainingData { Text = "quien fue san martin", Label = "CONVERSACION_HISTORIA" },
            new TrainingData { Text = "quien fue belgrano", Label = "CONVERSACION_HISTORIA" },
            new TrainingData { Text = "que es el 25 de mayo", Label = "CONVERSACION_HISTORIA" },
            new TrainingData { Text = "que es el 9 de julio", Label = "CONVERSACION_HISTORIA" },
            new TrainingData { Text = "contame de la historia argentina", Label = "CONVERSACION_HISTORIA" },
            new TrainingData { Text = "quien fue sarmiento", Label = "CONVERSACION_HISTORIA" },

            // ============================================
            // CONVERSACION - CLIMA
            // ============================================
            new TrainingData { Text = "va a llover", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "que tiempo hace", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "va a hacer calor", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "va a hacer frio", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "como esta el clima", Label = "CONVERSACION_CLIMA" },

            new TrainingData { Text = "que temperatura hace", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "que temperatura hay", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "cuantos grados hace", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "esta frio hoy", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "esta caluroso", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "va a llover hoy", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "como esta el tiempo", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "que clima hace", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "hace frio", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "hace calor", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "esta nublado", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "esta soleado", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "va a hacer frio", Label = "CONVERSACION_CLIMA" },
            new TrainingData { Text = "va a hacer calor", Label = "CONVERSACION_CLIMA" },

            // ============================================
            // CONVERSACION - COCINA
            // ============================================
            new TrainingData { Text = "como se hace una torta", Label = "CONVERSACION_COCINA" },
            new TrainingData { Text = "receta de empanadas", Label = "CONVERSACION_COCINA" },
            new TrainingData { Text = "como hacer milanesas", Label = "CONVERSACION_COCINA" },
            new TrainingData { Text = "receta de locro", Label = "CONVERSACION_COCINA" },
            new TrainingData { Text = "receta de puchero", Label = "CONVERSACION_COCINA" },

            // ============================================
            // CONVERSACION - REFRANES
            // ============================================
            new TrainingData { Text = "decime un refran", Label = "CONVERSACION_REFRANES" },
            new TrainingData { Text = "que significa mas vale tarde", Label = "CONVERSACION_REFRANES" },
            new TrainingData { Text = "dichos populares", Label = "CONVERSACION_REFRANES" },
            new TrainingData { Text = "refranes argentinos", Label = "CONVERSACION_REFRANES" },

            // ============================================
            // CONVERSACION - CHISTES
            // ============================================
            new TrainingData { Text = "contame un chiste", Label = "CONVERSACION_CHISTES" },
            new TrainingData { Text = "sabes algun chiste", Label = "CONVERSACION_CHISTES" },
            new TrainingData { Text = "decime algo gracioso", Label = "CONVERSACION_CHISTES" },
            new TrainingData { Text = "contame algo divertido", Label = "CONVERSACION_CHISTES" },

            // ============================================
            // CONVERSACION - NATURALEZA
            // ============================================
            new TrainingData { Text = "que plantas puedo tener", Label = "CONVERSACION_NATURALEZA" },
            new TrainingData { Text = "hablame de los pajaros", Label = "CONVERSACION_NATURALEZA" },
            new TrainingData { Text = "como cuido las plantas", Label = "CONVERSACION_NATURALEZA" },
            new TrainingData { Text = "animales domesticos", Label = "CONVERSACION_NATURALEZA" },

            // ============================================
            // CONVERSACION - LITERATURA
            // ============================================
            new TrainingData { Text = "decime un poema", Label = "CONVERSACION_LITERATURA" },
            new TrainingData { Text = "quien fue borges", Label = "CONVERSACION_LITERATURA" },
            new TrainingData { Text = "poemas de amor", Label = "CONVERSACION_LITERATURA" },
            new TrainingData { Text = "que libros me recomendas", Label = "CONVERSACION_LITERATURA" },
            new TrainingData { Text = "quien fue cortazar", Label = "CONVERSACION_LITERATURA" },

            // ============================================
            // CONVERSACION - OFICIOS
            // ============================================
            new TrainingData { Text = "que es un zapatero", Label = "CONVERSACION_OFICIOS" },
            new TrainingData { Text = "que hacia un lechero", Label = "CONVERSACION_OFICIOS" },
            new TrainingData { Text = "oficios antiguos", Label = "CONVERSACION_OFICIOS" },
            new TrainingData { Text = "trabajos de antes", Label = "CONVERSACION_OFICIOS" },

            // ============================================
            // CONVERSACION - BIBLIA
            // ============================================
            new TrainingData { Text = "quien fue jesus", Label = "CONVERSACION_BIBLIA" },
            new TrainingData { Text = "que dice la biblia", Label = "CONVERSACION_BIBLIA" },
            new TrainingData { Text = "quien fue moises", Label = "CONVERSACION_BIBLIA" },
            new TrainingData { Text = "que es el genesis", Label = "CONVERSACION_BIBLIA" },
            new TrainingData { Text = "quien fue david", Label = "CONVERSACION_BIBLIA" },
            new TrainingData { Text = "cual es el salmo 23", Label = "CONVERSACION_BIBLIA" },

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