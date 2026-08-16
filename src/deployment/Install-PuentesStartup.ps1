$ErrorActionPreference = "Stop"
$launcher = Join-Path $PSScriptRoot "Start-Puentes.ps1"
if (-not (Test-Path -LiteralPath $launcher)) {
    throw "No se encontro Start-Puentes.ps1."
}

$action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument `
    "-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$launcher`""
$trigger = New-ScheduledTaskTrigger -AtLogOn -User $env:USERNAME
$principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME `
    -LogonType Interactive -RunLevel Limited
$settings = New-ScheduledTaskSettingsSet -RestartCount 5 `
    -RestartInterval (New-TimeSpan -Minutes 1) `
    -ExecutionTimeLimit (New-TimeSpan -Days 3650)

Register-ScheduledTask -TaskName "Puentes" -Action $action `
    -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null
Write-Host "Puentes se iniciara automaticamente cuando ingrese a Windows."
