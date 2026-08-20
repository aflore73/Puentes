param([string]$ApiBaseUrl = "http://localhost:5396")

$ErrorActionPreference = "Stop"

function Post-Json([string]$Path, [hashtable]$Body) {
    Invoke-RestMethod `
        -Method Post `
        -Uri "$ApiBaseUrl$Path" `
        -ContentType "application/json; charset=utf-8" `
        -Body ([Text.Encoding]::UTF8.GetBytes(($Body | ConvertTo-Json -Depth 8)))
}

function Add-Person(
    [string]$Name,
    [string]$BirthDate,
    [string]$City,
    [string]$Notes = "") {
    Post-Json "/people" @{
        name = $Name
        birthDate = $BirthDate
        city = $City
        province = "Provincia de Prueba"
        country = "País de Prueba"
        notes = $Notes
    }
}

$rosa = Add-Person "Rosa Benítez" "1952-06-10" "Villa Modelo" `
    "Rosa es viuda. Todos los datos de esta persona son ficticios y se usan para evaluación."
$mateo = Add-Person "Mateo Benítez" "1990-03-12" "Ciudad Jardín"
$daniel = Add-Person "Daniel Benítez" "1975-05-18" "Barrio Norte"
$pablo = Add-Person "Pablo Benítez" "1975-05-18" "Barrio Sur"

Post-Json "/people/$($mateo.id)/relationships" @{
    relatedPersonId = $rosa.id; type = "Child"
    notes = "Mateo es hijo de Rosa."
} | Out-Null
Post-Json "/people/$($daniel.id)/relationships" @{
    relatedPersonId = $rosa.id; type = "Child"
    notes = "Daniel es hijo de Rosa y gemelo de Pablo."
} | Out-Null
Post-Json "/people/$($pablo.id)/relationships" @{
    relatedPersonId = $rosa.id; type = "Child"
    notes = "Pablo es hijo de Rosa y gemelo de Daniel."
} | Out-Null

Post-Json "/people/$($mateo.id)/routines" @{
    title = "Trabajo"; notes = "Trabaja en Parque Central. Algunos días trabaja desde su casa de Ciudad Jardín."
    daysOfWeek = "Monday,Tuesday,Wednesday,Thursday,Friday"
    startTime = "09:00"; endTime = "18:00"; isActive = $true
} | Out-Null
Post-Json "/people/$($mateo.id)/routines" @{
    title = "Entrenamiento de fútbol"; notes = "Entrena fútbol con el club de la universidad."
    daysOfWeek = "Monday,Wednesday,Friday"
    startTime = "19:00"; endTime = "23:00"; isActive = $true
} | Out-Null
Post-Json "/people/$($daniel.id)/routines" @{
    title = "Trabajo en la oficina"; notes = "Trabaja en la oficina los martes y miércoles."
    daysOfWeek = "Tuesday,Wednesday"
    startTime = "09:00"; endTime = "18:00"; isActive = $true
} | Out-Null
Post-Json "/people/$($daniel.id)/routines" @{
    title = "Trabajo desde casa"; notes = "Trabaja desde su casa los lunes, jueves y viernes."
    daysOfWeek = "Monday,Thursday,Friday"
    startTime = "09:00"; endTime = "18:00"; isActive = $true
} | Out-Null

foreach ($preference in @(
    @{ title = "Música"; notes = "Le gusta escuchar a Sandro, Leonardo Favio, The Beatles y Elvis Presley."; tags = "música,cantantes"; topicCodes = @("interest.music") },
    @{ title = "Plantas"; notes = "Le gustan mucho las plantas y conversar sobre su cuidado."; tags = "plantas,jardinería"; topicCodes = @("interest.plants") }
)) {
    $preference.isActive = $true
    Post-Json "/people/$($rosa.id)/preferences" $preference | Out-Null
}

foreach ($memory in @(
    @{ startDate = "1962-01-01"; datePrecision = "Year"; title = "Tardes con la tía Clara"; description = "De chica pasaba tardes con su tía Clara y sus primas Lucía y Sofía en el barrio del Parque, cerca de unos estudios de televisión."; place = "Barrio del Parque"; topicCodes = @("memory.childhood", "memory.family") },
    @{ startDate = "1964-01-01"; datePrecision = "Year"; title = "Vacaciones junto al mar"; description = "Su tía Clara y su tío la llevaban de vacaciones en un automóvil blanco. Su tío manejaba con prudencia."; place = "Villa del Mar"; topicCodes = @("memory.travel", "memory.childhood", "memory.family") },
    @{ startDate = "1968-01-01"; datePrecision = "Approximate"; title = "Pesca con su papá Roberto"; description = "Pasaba lindos momentos pescando con su papá Roberto. Cuando pescaban mucho, él compartía los pescados con los vecinos."; place = "Río del Norte"; topicCodes = @("memory.family", "memory.life-story") },
    @{ startDate = "1972-01-01"; datePrecision = "Year"; title = "Trabajo en la fábrica Aurora"; description = "Trabajó en una fábrica de electrodomésticos y allí conoció a su amiga Cecilia."; place = "Fábrica Aurora"; topicCodes = @("memory.life-story") }
)) {
    $memory.endDate = $null
    $memory.isPositiveMemory = $true
    $memory.participants = @()
    Post-Json "/people/$($rosa.id)/life-events" $memory | Out-Null
}

foreach ($content in @(
    @{ title = "El buen pastor"; content = "El pastor acompaña, guía hacia el descanso y permanece cerca aun en los momentos oscuros."; reference = "Salmos 23:1-4"; attribution = "Paráfrasis de evaluación"; tags = "fe,aliento" },
    @{ title = "Una invitación al descanso"; content = "Quienes están cansados pueden acercarse y encontrar descanso para el alma."; reference = "Mateo 11:28-29"; attribution = "Paráfrasis de evaluación"; tags = "fe,descanso" }
)) {
    $content.topicCodes = @("reading.religious")
    $content.isActive = $true
    Post-Json "/people/$($rosa.id)/support-contents" $content | Out-Null
}

Post-Json "/people/$($rosa.id)/belongings" @{
    name = "Llaves"; notes = "Suele dejarlas en el cuenco de la entrada."
    tags = "llaves,entrada"; isActive = $true
} | Out-Null

Post-Json "/people/$($rosa.id)/agenda" @{
    scheduledAt = "2026-08-25T10:30:00-03:00"
    endAt = "2026-08-25T11:30:00-03:00"
    title = "Consulta con la médica clínica"
    description = "Control programado."
    place = "Centro Médico del Parque"
    status = "Scheduled"
    topicCodes = @("agenda.medical-appointment")
    participants = @()
} | Out-Null

[pscustomobject]@{
    PersonId = $rosa.id
    PersonName = $rosa.name
    Synthetic = $true
}
