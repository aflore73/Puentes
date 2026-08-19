# Puentes - Guia de mantenimiento de la API

Esta guia contiene ejemplos de los metodos disponibles para mantener la informacion de Puentes y la traduccion al español de los campos que aparecen en ingles.

## Inicio y acceso

Desde PowerShell:

```powershell
cd C:\Users\aflorentin\source\repos\Repo_Puentes\src\Puentes
dotnet run --launch-profile http
```

Direcciones:

- Swagger de mantenimiento: `http://localhost:5121/family`
- Estado de la API: `http://localhost:5121/health`
- URL base utilizada en los ejemplos: `http://localhost:5121`

En Swagger, seleccionar un metodo, presionar **Try it out**, completar los datos y presionar **Execute**.

## Metodos HTTP

| Metodo | Traduccion y uso |
|---|---|
| `GET` | Consultar o leer informacion. No modifica la base. |
| `POST` | Crear un registro nuevo. La API genera su `id`. |
| `PUT` | Actualizar o reemplazar un registro existente. Requiere su `id`. |
| `DELETE` | Eliminar. Actualmente no hay metodos `DELETE` de mantenimiento. |

Convenciones usadas en las rutas:

- `{personId}`: identificador de la persona dueña de la informacion.
- `{id}`: identificador del registro que se quiere modificar.
- Los valores entre llaves son marcadores: deben reemplazarse por un GUID real.
- Las fechas de agenda incluyen el huso horario de Argentina: `-03:00`.

## Personas

### Consultar personas

```http
GET /people
```

El resultado permite obtener el `id` que se utiliza en los demas metodos.

### Crear una persona

```http
POST /people
Content-Type: application/json
```

```json
{
  "name": "Marta Loiacono",
  "birthDate": "1950-07-01",
  "city": "Villa Ballester",
  "province": "Buenos Aires",
  "country": "Argentina",
  "notes": "Marta es viuda."
}
```

| Campo | Traduccion |
|---|---|
| `name` | nombre |
| `birthDate` | fecha de nacimiento |
| `city` | ciudad o localidad |
| `province` | provincia |
| `country` | pais |
| `notes` | informacion actual confirmada de la persona |

### Modificar una persona

```http
PUT /people/{id}
Content-Type: application/json
```

Se envia el objeto completo con nombre, fecha de nacimiento, domicilio y notas.
Por ejemplo, `notes` puede contener un estado actual estable como
`"Marta es viuda."`.

## Relaciones entre personas

### Consultar relaciones

```http
GET /people/{personId}/relationships
```

### Crear una relacion

El tipo se interpreta desde la persona de la URL hacia la persona relacionada.

```http
POST /people/{personId}/relationships
Content-Type: application/json
```

```json
{
  "relatedPersonId": "ID-DE-EZEQUIEL",
  "type": "Child",
  "notes": "Ezequiel es hijo de Marta."
}
```

| Valor tecnico | Español |
|---|---|
| `Child` | hijo o hija |
| `Parent` | padre o madre |
| `Partner` | pareja |
| `Spouse` | conyuge |
| `Sibling` | hermano o hermana |
| `Grandchild` | nieto o nieta |
| `Grandparent` | abuelo o abuela |
| `Friend` | amigo o amiga |
| `Caregiver` | persona cuidadora |
| `Cohabitant` | conviviente |
| `NieceNephew` | sobrina o sobrino |
| `AuntUncle` | tia o tio |
| `DaughterSonInLaw` | nuera o yerno |
| `ParentInLaw` | suegra o suegro |
| `Cousin` | prima o primo |
| `Neighbor` | vecino o vecina |
| `Other` | otra relacion |

## Rutinas

### Consultar rutinas

```http
GET /people/{personId}/routines
```

### Crear una rutina

```http
POST /people/{personId}/routines
Content-Type: application/json
```

```json
{
  "title": "Trabajo",
  "notes": "Trabaja en Nordelta. Algunos dias trabaja desde su casa.",
  "daysOfWeek": "Monday,Tuesday,Wednesday,Thursday,Friday",
  "startTime": "09:00",
  "endTime": "18:00",
  "isActive": true
}
```

`daysOfWeek`, `startTime` y `endTime` son opcionales, pero deben enviarse los
tres juntos cuando la rutina tiene un horario conocido. Puentes usa estos
campos para enviar a OpenAI solamente las rutinas correspondientes al dia y
la hora actuales. Las horas utilizan el formato de 24 horas `HH:mm`.

### Modificar una rutina

```http
PUT /people/{personId}/routines/{id}
Content-Type: application/json
```

Se envia el objeto completo con los valores actualizados. Para desactivar una rutina sin borrarla, usar `"isActive": false`.

Valores permitidos en `daysOfWeek`:

| Valor tecnico | Espanol |
|---|---|
| `Monday` | lunes |
| `Tuesday` | martes |
| `Wednesday` | miercoles |
| `Thursday` | jueves |
| `Friday` | viernes |
| `Saturday` | sabado |
| `Sunday` | domingo |

Para indicar varios dias se separan con comas, sin corchetes.

## Preferencias

### Consultar preferencias

```http
GET /people/{personId}/preferences
```

### Crear una preferencia

```http
POST /people/{personId}/preferences
Content-Type: application/json
```

```json
{
  "title": "Musica preferida",
  "notes": "Le gustan Sandro, Leonardo Favio, The Beatles y Elvis Presley.",
  "tags": "musica,cantantes",
  "topicCodes": ["interest.music"],
  "isActive": true
}
```

### Modificar una preferencia

```http
PUT /people/{personId}/preferences/{id}
```

Se envia el mismo formato completo utilizado en el `POST`.

## Lecturas y contenidos de apoyo

### Consultar contenidos

```http
GET /people/{personId}/support-contents
```

### Crear un contenido

```http
POST /people/{personId}/support-contents
Content-Type: application/json
```

```json
{
  "title": "Jehová es mi pastor",
  "content": "Jehová es mi pastor; nada me faltará.",
  "topicCodes": ["reading.religious"],
  "attribution": "Biblia Reina-Valera 1960",
  "reference": "Salmos 23:1-4",
  "tags": "fe,aliento,tranquilidad",
  "isActive": true
}
```

### Modificar un contenido

```http
PUT /people/{personId}/support-contents/{id}
```

Se envia el mismo formato completo utilizado en el `POST`.

## Recuerdos e historia personal

### Consultar recuerdos

```http
GET /people/{personId}/life-events
```

### Crear un recuerdo

```http
POST /people/{personId}/life-events
Content-Type: application/json
```

```json
{
  "startDate": "2026-08-12",
  "endDate": null,
  "datePrecision": "ExactDate",
  "title": "Visita a la doctora psiquiatra",
  "description": "Marta fue con Alejandro a la consulta.",
  "place": "Calle Federico Lacroze, CABA",
  "isPositiveMemory": false,
  "topicCodes": ["memory.family"],
  "participants": [
    {
      "personId": "ID-DE-ALEJANDRO",
      "role": "Acompañante"
    }
  ]
}
```

Valores de `datePrecision`:

| Valor tecnico | Español |
|---|---|
| `Unknown` | precision desconocida |
| `ExactDate` | fecha exacta |
| `Month` | solo se conoce el mes |
| `Year` | solo se conoce el año |
| `Approximate` | fecha aproximada |

### Modificar solamente los temas de un recuerdo

```http
PUT /life-events/{id}/topics
Content-Type: application/json
```

```json
{
  "topicCodes": ["memory.family", "memory.life-story"]
}
```

Actualmente la API no tiene un `PUT` para modificar los demas campos de un recuerdo ya creado.

## Agenda: citas, salidas y paseos futuros

### Consultar la agenda futura

```http
GET /people/{personId}/agenda
```

Para incluir tambien elementos pasados:

```http
GET /people/{personId}/agenda?includePast=true
```

### Crear una cita medica

```http
POST /people/{personId}/agenda
Content-Type: application/json
```

```json
{
  "scheduledAt": "2026-09-10T10:00:00-03:00",
  "endAt": "2026-09-10T11:00:00-03:00",
  "title": "Turno con el flebologo",
  "description": "Llevar los estudios anteriores.",
  "place": "Villa Ballester",
  "status": "Scheduled",
  "topicCodes": ["agenda.medical-appointment"],
  "participants": [
    {
      "personId": "ID-DE-EZEQUIEL",
      "role": "Acompañante"
    }
  ]
}
```

### Crear una salida o paseo

```json
{
  "scheduledAt": "2026-09-12T16:00:00-03:00",
  "endAt": null,
  "title": "Paseo y merienda",
  "description": "Pasear por la plaza y luego tomar la merienda.",
  "place": "Plaza de Villa Ballester",
  "status": "Scheduled",
  "topicCodes": ["agenda.personal"],
  "participants": [
    {
      "personId": "ID-DE-ALEJANDRO",
      "role": "Acompañante"
    }
  ]
}
```

Estados de agenda:

| Valor tecnico | Español |
|---|---|
| `Scheduled` | programado |
| `Completed` | realizado o completado |
| `Cancelled` | cancelado |

Actualmente la API permite crear y consultar la agenda, pero no tiene un `PUT` para modificar o cancelar una actividad ya creada.

## Objetos personales

Sirve para registrar lugares habituales de objetos que Marta busca con frecuencia.

### Consultar objetos

```http
GET /people/{personId}/belongings
```

### Crear un objeto

```http
POST /people/{personId}/belongings
Content-Type: application/json
```

```json
{
  "name": "Llaves de casa",
  "notes": "Suelen estar en el recipiente de la mesa junto a la entrada.",
  "tags": "llaves,entrada,mesa",
  "isActive": true
}
```

### Modificar un objeto

```http
PUT /people/{personId}/belongings/{id}
```

## Contactos de confianza

Esta informacion es opcional en el funcionamiento actual.

### Consultar contactos

```http
GET /people/{personId}/trusted-contacts
```

### Crear un contacto de confianza

```http
POST /people/{personId}/trusted-contacts
Content-Type: application/json
```

```json
{
  "contactPersonId": "ID-DE-ALEJANDRO",
  "priority": 1,
  "notes": "Hijo de Marta.",
  "isActive": true
}
```

### Modificar un contacto

```http
PUT /people/{personId}/trusted-contacts/{id}
```

## Medicacion

### Consultar medicamentos

```http
GET /medications
GET /medications/{id}
```

### Crear un medicamento

```http
POST /medications
Content-Type: application/json
```

```json
{
  "name": "Nombre del medicamento",
  "dose": "10 mg",
  "form": "Pill",
  "shape": "Round",
  "color": "Blanco",
  "instructions": "Tomar con agua.",
  "isActive": true
}
```

### Modificar un medicamento

```http
PUT /medications/{id}
```

Se envia el mismo formato completo utilizado en el `POST`.

Formas de medicacion (`form`):

| Valor tecnico | Español |
|---|---|
| `Pill` | comprimido |
| `Capsule` | capsula |
| `Syrup` | jarabe |
| `Drops` | gotas |
| `Injection` | inyeccion |
| `Cream` | crema |
| `Ointment` | pomada |
| `Spray` | aerosol |
| `Inhaler` | inhalador |
| `Patch` | parche |
| `Other` | otra forma |

Formas fisicas (`shape`):

| Valor tecnico | Español |
|---|---|
| `Round` | redondo |
| `Oval` | ovalado |
| `Oblong` | alargado |
| `Capsule` | con forma de capsula |
| `Other` | otra forma |

### Configurar horarios de un medicamento

```http
PUT /medications/{id}/schedules
Content-Type: application/json
```

```json
[
  {
    "turn": "Morning",
    "quantity": 1
  },
  {
    "turn": "Night",
    "quantity": 0.5
  }
]
```

Turnos:

| Valor tecnico | Español |
|---|---|
| `Morning` | mañana |
| `Midday` | mediodia |
| `Afternoon` | tarde |
| `Night` | noche |

## Temas disponibles

Consultar siempre los codigos vigentes antes de asociar temas:

```http
GET /content-topics
```

| Codigo | Uso |
|---|---|
| `memory.travel` | recuerdos de viajes |
| `memory.childhood` | recuerdos de infancia |
| `memory.family` | recuerdos familiares |
| `memory.life-story` | historias de vida |
| `reading.religious` | lectura religiosa |
| `reading.poetry` | poesia |
| `reading.story` | historia o relato |
| `interest.music` | interes por la musica |
| `interest.plants` | interes por las plantas |
| `agenda.medical-appointment` | consulta medica |
| `agenda.personal` | actividad personal, salida o paseo |
| `agenda.family` | actividad familiar |
| `agenda.errand` | tramite o mandado |

## Traduccion general de campos

| Campo tecnico | Español |
|---|---|
| `id` | identificador del registro |
| `personId` | identificador de la persona |
| `title` | titulo |
| `description` | descripcion |
| `notes` | notas u observaciones |
| `tags` | palabras clave separadas por comas |
| `topicCodes` | codigos de temas o categorias |
| `isActive` | indica si el registro esta activo |
| `startDate` | fecha de inicio |
| `endDate` | fecha de finalizacion |
| `scheduledAt` | fecha y hora programadas |
| `endAt` | fecha y hora de finalizacion |
| `place` | lugar |
| `status` | estado |
| `participants` | participantes o acompañantes |
| `role` | funcion de la persona en la actividad |
| `content` | texto completo del contenido |
| `attribution` | autor, obra o procedencia |
| `reference` | referencia, capitulo o versiculo |
| `priority` | prioridad; 1 es la mas alta |
| `quantity` | cantidad |
| `confirmed` | confirmado |

## Valores posibles de campos codificados

Esta seccion indica que valores deben escribirse literalmente en el JSON. Los
valores tecnicos se envian en ingles porque son los nombres que reconoce la API.

### `role` - funcion del participante

`role` describe por que participa o que funcion cumple una persona dentro de un
recuerdo o una actividad de agenda.

Actualmente **no es un campo codificado ni una clave foranea**. Es texto libre y
tambien puede enviarse como `null`. Para mantener la informacion uniforme se
recomienda utilizar estos valores:

| Valor recomendado | Significado |
|---|---|
| `Acompañante` | acompaña a la persona a una cita, salida o tramite |
| `Familiar` | participa como integrante de la familia |
| `Amigo` / `Amiga` | participa como amistad |
| `Cuidador` / `Cuidadora` | brinda cuidado o asistencia |
| `Profesional` | participa como medico, terapeuta u otro profesional |
| `Organizador` / `Organizadora` | organiza la actividad o el encuentro |
| `Invitado` / `Invitada` | asiste como invitado |
| `Conductor` / `Conductora` | realiza el traslado |
| `null` | no se desea especificar una funcion |

Ejemplo:

```json
"participants": [
  {
    "personId": "ID-DE-EZEQUIEL",
    "role": "Acompañante"
  }
]
```

La persona indicada mediante `personId` debe existir previamente en `People`.
La persona dueña de la agenda o del recuerdo no debe repetirse en
`participants`.

### `isActive` - registro activo

| Valor JSON | Español | Efecto |
|---|---|---|
| `true` | activo | el orquestador puede consultar y utilizar el registro |
| `false` | inactivo | el registro se conserva, pero no aparece entre los activos |

Se utiliza en rutinas, preferencias, contenidos, objetos, contactos y
medicamentos. Es un booleano JSON, por lo que no debe escribirse entre comillas.

### `status` - estado de agenda

| Valor tecnico | Español | Uso |
|---|---|---|
| `Scheduled` | programado | la cita o actividad esta pendiente |
| `Completed` | completado | la actividad ya se realizo |
| `Cancelled` | cancelado | la actividad fue cancelada |

`Scheduled` es el valor predeterminado. Debe enviarse `"status": "Scheduled"`,
no `"Schedule"`.

### `datePrecision` - precision de una fecha de recuerdo

| Valor tecnico | Español |
|---|---|
| `Unknown` | desconocida |
| `ExactDate` | fecha exacta |
| `Month` | se conoce el mes |
| `Year` | se conoce el año |
| `Approximate` | fecha aproximada |

### `type` - tipo de relacion

| Valor tecnico | Español |
|---|---|
| `Child` | hijo o hija |
| `Parent` | padre o madre |
| `Partner` | pareja |
| `Spouse` | conyuge |
| `Sibling` | hermano o hermana |
| `Grandchild` | nieto o nieta |
| `Grandparent` | abuelo o abuela |
| `Friend` | amigo o amiga |
| `Caregiver` | cuidador o cuidadora |
| `Cohabitant` | conviviente |
| `NieceNephew` | sobrina o sobrino |
| `AuntUncle` | tia o tio |
| `DaughterSonInLaw` | nuera o yerno |
| `ParentInLaw` | suegra o suegro |
| `Cousin` | prima o primo |
| `Neighbor` | vecino o vecina |
| `Other` | otro tipo de relacion |

`Unknown` existe internamente, pero la API no lo acepta para crear relaciones.

### `direction` - direccion de una relacion consultada

Este campo aparece en las respuestas de consulta, no se envia al crear la
relacion.

| Valor tecnico | Español |
|---|---|
| `Outgoing` | saliente: la persona consultada es el origen de la relacion |
| `Incoming` | entrante: la persona consultada es la persona relacionada |

### `turn` - turno de medicacion

| Valor tecnico | Español |
|---|---|
| `Morning` | mañana |
| `Midday` | mediodia |
| `Afternoon` | tarde |
| `Night` | noche |

### `form` - presentacion de un medicamento

| Valor tecnico | Español |
|---|---|
| `Pill` | comprimido |
| `Capsule` | capsula |
| `Syrup` | jarabe |
| `Drops` | gotas |
| `Injection` | inyeccion |
| `Cream` | crema |
| `Ointment` | pomada |
| `Spray` | aerosol |
| `Inhaler` | inhalador |
| `Patch` | parche |
| `Other` | otra presentacion |

`Unknown` existe internamente para indicar que la presentacion es desconocida.

### `shape` - forma fisica de un medicamento

| Valor tecnico | Español |
|---|---|
| `Round` | redondo |
| `Oval` | ovalado |
| `Oblong` | alargado |
| `Capsule` | con forma de capsula |
| `Other` | otra forma |

`Unknown` existe internamente para indicar que la forma es desconocida.

### `confirmed` - confirmacion de una toma

| Valor JSON | Español |
|---|---|
| `true` | la persona confirmo que realizo la toma |
| `false` | la toma no esta confirmada |

### `includePast` - incluir agenda pasada

Este valor se envia en la URL, no en el cuerpo JSON:

| Valor | Español |
|---|---|
| omitido o `false` | devuelve solamente agenda futura |
| `true` | incluye tambien actividades pasadas |

```http
GET /people/{personId}/agenda?includePast=true
```

### `topicCodes` - temas permitidos

No admite cualquier texto: cada codigo debe existir en `ContentTopics` y debe
pertenecer al grupo correspondiente al registro.

| Prefijo o grupo | Se utiliza en | Ejemplos |
|---|---|---|
| `memory.*` / `Memory` | recuerdos | `memory.family`, `memory.childhood` |
| `reading.*` / `Reading` | contenidos de apoyo | `reading.religious`, `reading.poetry` |
| `interest.*` / `Interest` | preferencias | `interest.music`, `interest.plants` |
| `agenda.*` / `Agenda` | agenda | `agenda.medical-appointment`, `agenda.personal` |

Los codigos vigentes pueden consultarse mediante `GET /content-topics`.

### Campos que relacionan registros existentes

| Campo | Registro relacionado | Como obtenerlo |
|---|---|---|
| `{personId}` en la URL | persona dueña del dato | `GET /people` |
| `relatedPersonId` | otra persona de una relacion | `GET /people` |
| `participants[].personId` | participante de agenda o recuerdo | `GET /people` |
| `contactPersonId` | persona elegida como contacto | `GET /people` |
| `{id}` de rutina, preferencia, contenido u objeto | registro que se actualiza | consultar la coleccion correspondiente |
| `{id}` de medicamento | medicamento que se actualiza | `GET /medications` |
| `topicCodes` | temas almacenados en `ContentTopics` | `GET /content-topics` |

Los campos terminados en `Id` contienen un GUID. No se debe escribir el nombre
de la persona en lugar de su identificador.

## Respuestas habituales

| Codigo HTTP | Significado |
|---|---|
| `200 OK` | consulta realizada correctamente |
| `201 Created` | registro creado correctamente |
| `204 No Content` | registro actualizado correctamente |
| `400 Bad Request` | faltan datos o algun valor no es valido |
| `404 Not Found` | no se encontro la persona o el registro |
| `409 Conflict` | el registro entra en conflicto con uno existente |
| `500 Internal Server Error` | error interno; revisar la consola de Puentes |
