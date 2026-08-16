$ErrorActionPreference = "Stop"
$configDirectory = Join-Path $PSScriptRoot "config"
$keyFile = Join-Path $configDirectory "openai.key"
New-Item -ItemType Directory -Force -Path $configDirectory | Out-Null

$key = Read-Host "Ingrese la API key de OpenAI" -AsSecureString
$key | ConvertFrom-SecureString | Set-Content -LiteralPath $keyFile
Write-Host "La clave quedo cifrada para el usuario actual de Windows."
