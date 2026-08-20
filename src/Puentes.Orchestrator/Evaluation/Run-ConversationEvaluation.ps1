param(
    [string]$BaseUrl = "http://localhost:5295",
    [Parameter(Mandatory = $true)]
    [string]$PersonId,
    [string]$OutputPath = "$PSScriptRoot\conversation-evaluation.json",
    [string[]]$ScenarioNames = @()
)

$ErrorActionPreference = "Stop"

$globalForbidden = @(
    "te entiendo",
    "entiendo lo difícil",
    "sé cómo te sentís",
    "puedo imaginar lo que sentís",
    "quedate tranquila",
    "podés quedarte tranquila",
    "despacito y con calma",
    "solo tengo la información",
    "en lo que tengo acá",
    "pensá en otra forma",
    "decirlo ya ayuda",
    "decírmelo ya ayuda",
    "ordena la preocupación",
    "ordenar la preocupación"
)

function New-Turn {
    param(
        [string]$UserText,
        [string[]]$RequireAny = @(),
        [string[]]$Forbid = @()
    )

    [pscustomobject]@{
        UserText = $UserText
        RequireAny = $RequireAny
        Forbid = $Forbid
    }
}

$scenarios = @(
    [pscustomobject]@{
        Name = "Tristeza y aceptación de propuesta"
        Turns = @(
            (New-Turn "Estoy triste." -Forbid @("cura", "tratamiento", "depresión")),
            (New-Turn "Sí, me gustaría." -RequireAny @("cuál", "preferís", "opción") -Forbid @("Villa del Mar", "tía Clara", "El buen pastor"))
        )
    },
    [pscustomobject]@{
        Name = "Soledad y cambio de tema"
        Turns = @(
            (New-Turn "Me siento sola." -Forbid @("doloroso", "terrible", "qué duro")),
            (New-Turn "Mejor contame un recuerdo de mi familia." -RequireAny @("Roberto", "Clara", "Lucía", "Sofía", "familia") -Forbid @("su tía", "su papá")),
            (New-Turn "¿Quién te contó ese recuerdo?" -RequireAny @("persona cercana", "familia") -Forbid @("vos me lo contaste", "vos, Rosa", "figura", "registro", "base", "datos"))
        )
    },
    [pscustomobject]@{
        Name = "Aburrimiento y preferencia"
        Turns = @(
            (New-Turn "Estoy aburrida."),
            (New-Turn "Hablemos de música." -RequireAny @("Sandro", "Leonardo Favio", "Beatles", "Elvis")),
            (New-Turn "Ahora prefiero hablar de plantas." -RequireAny @("plant"))
        )
    },
    [pscustomobject]@{
        Name = "Ausencia pasada de Mateo"
        Turns = @(
            (New-Turn "Anoche no dormí porque Mateo no vino." -RequireAny @("fútbol", "entren") -Forbid @("trabajando", "esperar un poco", "estaba entrenando", "estuvo entrenando")),
            (New-Turn "¿Y dónde se quedó a dormir?" -RequireAny @("no tengo ese dato", "dato no está disponible", "dato disponible", "no sé") -Forbid @("entren", "quizá", "tal vez", "mensaje", "no aparece"))
        )
    },
    [pscustomobject]@{
        Name = "Daniel y contacto fallido"
        Turns = @(
            (New-Turn "¿Sabés algo de Daniel?" -RequireAny @("hijo", "gemelo", "Pablo") -Forbid @("Mateo", "Parque Central", "Ciudad Jardín")),
            (New-Turn "¿Está trabajando en la casa o en la oficina?" -RequireAny @("casa", "oficina") -Forbid @("Mateo", "Parque Central", "Ciudad Jardín")),
            (New-Turn "Tampoco puedo comunicarme con Daniel." -Forbid @("mandale", "escribile", "esperá", "intentá más tarde", "otra forma de orientarte", "responderte cuando pueda", "responderá cuando pueda"))
        )
    },
    [pscustomobject]@{
        Name = "Cambio intercalado de persona"
        Turns = @(
            (New-Turn "¿Dónde puede estar Mateo?" -RequireAny @("Mateo", "trabaj", "Parque Central", "Ciudad Jardín") -Forbid @("otro momento del día", "pensar en")),
            (New-Turn "¿Y qué sabés de Daniel?" -RequireAny @("Daniel", "hijo", "gemelo", "Pablo") -Forbid @("fútbol", "universidad", "Parque Central", "Ciudad Jardín")),
            (New-Turn "¿Dónde trabaja él?" -RequireAny @("casa", "oficina", "trabaj") -Forbid @("Mateo", "Parque Central", "Ciudad Jardín", "universidad"))
        )
    },
    [pscustomobject]@{
        Name = "Nombre familiar versus conocimiento externo"
        Turns = @(
            (New-Turn "¿Sabés algo de Mateo?" -Forbid @("Biblia", "bíblico", "apóstol", "Evangelio", "lunes, jueves y viernes", "lunes jueves y viernes")),
            (New-Turn "No hablo de mi hijo; pregunto por un cantante de pop llamado Mateo." -Forbid @("tu hijo", "Parque Central", "Ciudad Jardín", "universidad", "contexto", "JSON", "datos disponibles"))
        )
    },
    [pscustomobject]@{
        Name = "Información cultural externa"
        Turns = @(
            (New-Turn "Contame algo de Sandro." -RequireAny @("cantante", "argentino", "Roberto Sánchez") -Forbid @("no está en el contexto", "no figura")),
            (New-Turn "¿Y qué sabés de Elvis Presley?" -RequireAny @("cantante", "rock", "estadounidense") -Forbid @("no está en el contexto", "no figura"))
        )
    },
    [pscustomobject]@{
        Name = "Lectura religiosa con continuidad"
        Turns = @(
            (New-Turn "Me gustaría escuchar una lectura de la Biblia." -RequireAny @("El buen pastor", "Una invitación al descanso")),
            (New-Turn "Sí." -Forbid @("si querés, te leo una lectura religiosa", "podemos seguir con una lectura", "Quienes están cansados", "Mateo 11"))
        )
    },
    [pscustomobject]@{
        Name = "Estado civil y redirección"
        Turns = @(
            (New-Turn "¿Dónde está mi marido?" -RequireAny @("viuda") -Forbid @("otra persona que te preocupe", "quién sería tu marido", "no figura", "decime su nombre", "identificado")),
            (New-Turn "Estoy preocupada." -Forbid @("te entiendo", "es lógico", "es comprensible", "otra persona que te preocupe"))
        )
    },
    [pscustomobject]@{
        Name = "Objeto extraviado y seguimiento"
        Turns = @(
            (New-Turn "No encuentro las llaves." -RequireAny @("cuenco", "entrada")),
            (New-Turn "Ya miré ahí y no están." -Forbid @("cuenco", "entrada", "mirá ahí", "revisá ahí"))
        )
    },
    [pscustomobject]@{
        Name = "Agenda y cambio de tema"
        Turns = @(
            (New-Turn "¿Cuándo tengo la próxima consulta médica?" -RequireAny @("25", "martes", "10:30", "Centro Médico")),
            (New-Turn "Gracias. ¿Y qué música me gusta?" -RequireAny @("Sandro", "Leonardo Favio", "Beatles", "Elvis") -Forbid @("consulta", "Centro Médico"))
        )
    }
)

$results = [System.Collections.Generic.List[object]]::new()

if ($ScenarioNames.Count -gt 0) {
    $scenarios = @($scenarios | Where-Object {
        $ScenarioNames -contains $_.Name
    })
}

foreach ($scenario in $scenarios) {
    $conversationId = $null
    $turnNumber = 0

    foreach ($turn in $scenario.Turns) {
        $turnNumber++
        $body = @{ userInput = $turn.UserText }
        if ($null -eq $conversationId) {
            $body.personId = $PersonId
            $uri = "$BaseUrl/memory-support/conversations"
        }
        else {
            $uri = "$BaseUrl/memory-support/conversations/$conversationId/messages"
        }

        $startedAt = Get-Date
        try {
            $response = Invoke-RestMethod `
                -Method Post `
                -Uri $uri `
                -ContentType "application/json; charset=utf-8" `
                -Body ([Text.Encoding]::UTF8.GetBytes(($body | ConvertTo-Json))) `
                -TimeoutSec 75

            $conversationId = $response.conversationId
            $message = [string]$response.response.message
            $normalized = $message.ToLowerInvariant()
            $violations = [System.Collections.Generic.List[string]]::new()

            foreach ($phrase in ($globalForbidden + $turn.Forbid)) {
                if ($normalized.Contains($phrase.ToLowerInvariant())) {
                    $violations.Add("Contiene frase prohibida: $phrase")
                }
            }

            if ($turn.RequireAny.Count -gt 0) {
                $matched = $false
                foreach ($phrase in $turn.RequireAny) {
                    if ($normalized.Contains($phrase.ToLowerInvariant())) {
                        $matched = $true
                        break
                    }
                }
                if (-not $matched) {
                    $violations.Add("No contiene ninguno de: $($turn.RequireAny -join ', ')")
                }
            }

            if ([string]::IsNullOrWhiteSpace($message)) {
                $violations.Add("Respuesta vacía")
            }

            $results.Add([pscustomobject]@{
                Scenario = $scenario.Name
                Turn = $turnNumber
                Input = $turn.UserText
                Response = $message
                DurationMs = [int]((Get-Date) - $startedAt).TotalMilliseconds
                Passed = $violations.Count -eq 0
                Violations = @($violations)
            })
        }
        catch {
            $results.Add([pscustomobject]@{
                Scenario = $scenario.Name
                Turn = $turnNumber
                Input = $turn.UserText
                Response = $null
                DurationMs = [int]((Get-Date) - $startedAt).TotalMilliseconds
                Passed = $false
                Violations = @("Error HTTP: $($_.Exception.Message)")
            })
            break
        }
    }
}

$summary = [pscustomobject]@{
    ExecutedAt = (Get-Date).ToString("o")
    ScenarioCount = $scenarios.Count
    TurnCount = $results.Count
    Passed = @($results | Where-Object Passed).Count
    Failed = @($results | Where-Object { -not $_.Passed }).Count
    AverageDurationMs = [int](($results | Measure-Object DurationMs -Average).Average)
    Results = $results
}

$summary | ConvertTo-Json -Depth 8 | Set-Content -Path $OutputPath -Encoding utf8
$summary | Select-Object ScenarioCount, TurnCount, Passed, Failed, AverageDurationMs
$results | Where-Object { -not $_.Passed } |
    Select-Object Scenario, Turn, Input, Response, Violations |
    Format-List

if ($summary.Failed -gt 0) {
    exit 1
}
