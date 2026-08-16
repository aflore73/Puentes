[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$apiDirectory = Join-Path $root "api"
$orchestratorDirectory = Join-Path $root "orchestrator"
$logDirectory = Join-Path $root "logs"
$keyFile = Join-Path $root "config\openai.key"
$apiExecutable = Join-Path $apiDirectory "Puentes.exe"
$orchestratorExecutable = Join-Path $orchestratorDirectory "Puentes.Orchestrator.exe"

if (-not (Test-Path -LiteralPath $apiExecutable) -or
    -not (Test-Path -LiteralPath $orchestratorExecutable)) {
    throw "El release esta incompleto. Ejecute Build-PuentesRelease.ps1 nuevamente."
}
if (-not (Test-Path -LiteralPath $keyFile)) {
    throw "Falta configurar OpenAI. Ejecute Set-PuentesApiKey.ps1."
}

$secureKey = Get-Content -LiteralPath $keyFile | ConvertTo-SecureString
$credential = [pscredential]::new("puentes", $secureKey)
$env:PUENTES_API_KEY = $credential.GetNetworkCredential().Password
New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null

$api = $null
$orchestrator = $null
try {
    $api = Start-Process -FilePath $apiExecutable `
        -ArgumentList "--urls", "http://127.0.0.1:5121" `
        -WorkingDirectory $apiDirectory -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $logDirectory "api.log") `
        -RedirectStandardError (Join-Path $logDirectory "api-error.log")

    $ready = $false
    foreach ($attempt in 1..30) {
        Start-Sleep -Milliseconds 500
        try {
            $health = Invoke-RestMethod "http://127.0.0.1:5121/health" -TimeoutSec 2
            if ($health.status -eq "healthy") { $ready = $true; break }
        } catch { }
    }
    if (-not $ready) { throw "La API no quedo disponible." }

    $orchestrator = Start-Process -FilePath $orchestratorExecutable `
        -ArgumentList "--urls", "http://127.0.0.1:5130" `
        -WorkingDirectory $orchestratorDirectory -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $logDirectory "orchestrator.log") `
        -RedirectStandardError (Join-Path $logDirectory "orchestrator-error.log")

    while (-not $orchestrator.HasExited) {
        if ($api.HasExited) { throw "La API se detuvo inesperadamente." }
        Start-Sleep -Seconds 2
    }
    throw "El orquestador se detuvo inesperadamente."
}
finally {
    if ($null -ne $orchestrator -and -not $orchestrator.HasExited) {
        Stop-Process -Id $orchestrator.Id
    }
    if ($null -ne $api -and -not $api.HasExited) {
        Stop-Process -Id $api.Id
    }
    $env:PUENTES_API_KEY = $null
}
