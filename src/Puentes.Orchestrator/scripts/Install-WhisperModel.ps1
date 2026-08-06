param(
    [string]$Model = "base"
)

$ErrorActionPreference = "Stop"
$modelDirectory = Join-Path $PSScriptRoot "..\models"
$modelPath = Join-Path $modelDirectory "ggml-$Model.bin"
$modelUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-$Model.bin"

New-Item -ItemType Directory -Force -Path $modelDirectory | Out-Null
Invoke-WebRequest -Uri $modelUrl -OutFile $modelPath

Write-Host "Modelo de Whisper descargado en $modelPath."
