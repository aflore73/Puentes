param(
    [string]$Python = "python",
    [string]$Voice = "es_AR-daniela-high"
)

$ErrorActionPreference = "Stop"
$voiceDirectory = Join-Path $PSScriptRoot "..\voices"

New-Item -ItemType Directory -Force -Path $voiceDirectory | Out-Null
& $Python -m pip install piper-tts
& $Python -m piper.download_voices --data-dir $voiceDirectory $Voice

$pythonPath = (Get-Command $Python).Source
[Environment]::SetEnvironmentVariable(
    "PIPER_EXECUTABLE_PATH",
    $pythonPath,
    "User")

Write-Host "Piper y la voz $Voice quedaron instalados."
Write-Host "Reinicie Visual Studio para tomar PIPER_EXECUTABLE_PATH."
