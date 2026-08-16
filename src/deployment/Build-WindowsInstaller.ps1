[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$candidates = @(
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
)
$compiler = $candidates |
    Where-Object { Test-Path -LiteralPath $_ } |
    Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($compiler)) {
    throw "Inno Setup 6 no esta instalado. Instalelo y vuelva a ejecutar este script."
}

& (Join-Path $PSScriptRoot "Build-PuentesRelease.ps1")
if ($LASTEXITCODE -ne 0) { throw "No se pudo generar el release." }

$apiKey = $env:PUENTES_API_KEY
if ([string]::IsNullOrWhiteSpace($apiKey)) {
    throw "Falta PUENTES_API_KEY para generar un instalador con clave incluida."
}

$seedFile = Join-Path $PSScriptRoot "..\release\Puentes\config\openai.seed"
New-Item -ItemType Directory -Force -Path (Split-Path $seedFile) | Out-Null
try {
    Set-Content -LiteralPath $seedFile -Value $apiKey -NoNewline
    & $compiler (Join-Path $PSScriptRoot "Puentes.iss")
    if ($LASTEXITCODE -ne 0) { throw "No se pudo generar el instalador." }
}
finally {
    Remove-Item -LiteralPath $seedFile -Force -ErrorAction SilentlyContinue
    $apiKey = $null
}

Write-Host "Instalador generado en src\installer."
