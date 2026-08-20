# Evaluación conversacional anonimizada

Fecha: 2026-08-20

## Alcance

- Base SQLite temporal con datos exclusivamente ficticios.
- Persona asistida ficticia: Rosa Benítez.
- Personas relacionadas ficticias: Mateo, Daniel y Pablo.
- Se probaron sentimientos, propuestas, recuerdos, preferencias, rutinas,
  ausencias pasadas, continuidad de pronombres, conocimiento externo,
  lecturas, estado civil, objetos y agenda.
- La evaluación no incluyó audio, transcripción, síntesis de voz ni el flujo
  específico de medicación.

## Ejecuciones

1. Primera matriz: 10 conversaciones, 24 turnos. Resultado automático: 22/24.
2. Segunda matriz ampliada: 12 conversaciones, 28 turnos. Resultado automático:
   23/28.
3. Corrida final después de ajustar reglas generales: 12 conversaciones,
   28 turnos. Resultado automático: 21/28.

El resultado menor de la tercera corrida no indica una regresión lineal. Expone
variación del modelo y demuestra que una única ejecución no alcanza para validar
una respuesta generativa.

## Revisión final

La revisión humana de la última corrida clasificó 15 de 28 respuestas como
correctas sin reservas, 6 como correctas en el dato principal pero con una
continuación innecesaria o poco natural, y 7 como incorrectas.

Fallas repetibles o relevantes:

- Una aceptación breve después de tres opciones selecciona una categoría sin
  que la persona haya indicado cuál.
- Una pregunta sin dato, como dónde durmió alguien, puede volver a incorporar
  una rutina que no responde la pregunta.
- Las solicitudes explícitas sobre artistas externos pueden ser sustituidas por
  las preferencias personales almacenadas.
- Una aceptación ambigua entre dos lecturas puede hacer que el modelo elija una
  por su cuenta.
- El modelo puede sugerir volver a enviar un mensaje después de que la persona
  dijo que no logra comunicarse.
- Algunas continuaciones introducen otra persona, un recuerdo o una cita sin que
  se haya solicitado.
- Puede hablar de la persona asistida en tercera persona durante una
  conversación directa.
- Las prohibiciones por frases no detectan paráfrasis equivalentes.

## Conclusión técnica

`PromptMemorySupport` mejora la probabilidad de una respuesta correcta, pero no
puede garantizarla. Seguir agregando instrucciones produce cumplimiento variable
y puede causar regresiones en otros escenarios.

Para elevar la confiabilidad se necesitan cambios estructurales:

1. Representar todas las opciones ofrecidas en el estado conversacional, no una
   sola `DialogueOffer`.
2. Cuando una aceptación sea ambigua, resolver el siguiente paso en el workflow
   antes de pedir una redacción al modelo.
3. Proyectar al prompt únicamente datos de la persona y del tema enfocados en el
   turno, en lugar de confiar en que el modelo descarte el resto.
4. Incorporar una validación posterior basada en hechos y acciones estructuradas,
   con reintento cuando la respuesta use datos no seleccionados.
5. Separar explícitamente el modo de conocimiento externo del modo de contexto
   personal para que las preferencias no sustituyan la consulta general.
6. Ejecutar cada escenario varias veces y medir estabilidad, no solo éxito en una
   corrida.

El detalle íntegro de la última corrida está en
`conversation-evaluation.json`.

## Arquitectura implementada posteriormente

Después de esta evaluación se incorporaron las siguientes protecciones:

- La sesión conserva `PendingOffers` y no convierte varias categorías ofrecidas
  en una única elección implícita.
- Una aceptación ambigua activa `ClarifyOfferChoice` y obliga a preguntar qué
  opción prefiere.
- La sesión conserva `FocusedPersonName`; un seguimiento sin nombre mantiene el
  foco anterior y no acepta que el selector lo cambie por otra persona.
- Un selector semántico previo devuelve ámbitos de contexto y marco temporal.
- Las rutinas se filtran por persona y por `Current` o
  `YesterdayEvening` antes de construir el prompt.
- `Companion` no habilita agenda ni rutinas como efecto secundario.
- OpenAI devuelve evidencia estructurada con los nombres y títulos utilizados.
- El workflow rechaza evidencia fuera del contexto, evidencia obligatoria
  omitida, más de una lectura para una oferta concreta y títulos excluidos que
  reaparezcan desde el historial.
- Después de un reintento inválido se entrega una respuesta de seguridad en vez
  de reproducir contenido que no superó la validación.

Verificación posterior:

- 90 pruebas locales aprobadas.
- Las pruebas dirigidas confirmaron separación entre personas, conservación del
  foco, selección de rutina de la noche anterior y exclusión de esa rutina en la
  pregunta posterior sobre dónde durmió.
- Riesgo residual observado: una sugerencia de enviar un mensaje puede repetirse
  con otra redacción. Para bloquearla estructuralmente debe persistirse también
  un conjunto de acciones conversacionales recientes.
