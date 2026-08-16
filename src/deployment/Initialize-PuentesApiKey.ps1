$ErrorActionPreference = "Stop"
$configDirectory = Join-Path $PSScriptRoot "config"
$seedFile = Join-Path $configDirectory "openai.seed"
$keyFile = Join-Path $configDirectory "openai.key"

if (-not (Test-Path -LiteralPath $seedFile)) {
    throw "El instalador no contiene una clave inicial de OpenAI."
}

try {
    $plainTextKey = Get-Content -LiteralPath $seedFile -Raw
    if ([string]::IsNullOrWhiteSpace($plainTextKey)) {
        throw "La clave inicial de OpenAI esta vacia."
    }

    $plainTextKey = $plainTextKey.Trim()
    $plainTextKey |
        ConvertTo-SecureString -AsPlainText -Force |
        ConvertFrom-SecureString |
        Set-Content -LiteralPath $keyFile
}
finally {
    Remove-Item -LiteralPath $seedFile -Force -ErrorAction SilentlyContinue
    $plainTextKey = $null
}
