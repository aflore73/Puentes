[CmdletBinding()]
param(
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
$sourceRoot = Split-Path $PSScriptRoot -Parent
$releaseRoot = Join-Path $sourceRoot "release\Puentes"
$apiOutput = Join-Path $releaseRoot "api"
$orchestratorOutput = Join-Path $releaseRoot "orchestrator"

New-Item -ItemType Directory -Force -Path $apiOutput, $orchestratorOutput | Out-Null

dotnet publish (Join-Path $sourceRoot "Puentes\Puentes.csproj") `
    -c Release -r $Runtime --self-contained true -o $apiOutput
if ($LASTEXITCODE -ne 0) { throw "No se pudo publicar la API." }

dotnet publish (Join-Path $sourceRoot "Puentes.Orchestrator\Puentes.Orchestrator.csproj") `
    -c Release -r $Runtime --self-contained true -o $orchestratorOutput
if ($LASTEXITCODE -ne 0) { throw "No se pudo publicar el orquestador." }

$dataDirectory = Join-Path $apiOutput "Data"
$databaseTarget = Join-Path $dataDirectory "Puentes.db"
New-Item -ItemType Directory -Force -Path $dataDirectory | Out-Null
if (-not (Test-Path -LiteralPath $databaseTarget)) {
    Copy-Item (Join-Path $sourceRoot "Puentes\Data\Puentes.db") $databaseTarget
}

Copy-Item (Join-Path $PSScriptRoot "Start-Puentes.ps1") $releaseRoot -Force
Copy-Item (Join-Path $PSScriptRoot "Set-PuentesApiKey.ps1") $releaseRoot -Force
Copy-Item (Join-Path $PSScriptRoot "Initialize-PuentesApiKey.ps1") $releaseRoot -Force
Copy-Item (Join-Path $PSScriptRoot "Install-PuentesStartup.ps1") $releaseRoot -Force
Copy-Item (Join-Path $PSScriptRoot "Uninstall-PuentesStartup.ps1") $releaseRoot -Force

Write-Host "Release generado en: $releaseRoot"
