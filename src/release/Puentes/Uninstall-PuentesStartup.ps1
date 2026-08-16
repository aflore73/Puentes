$task = Get-ScheduledTask -TaskName "Puentes" -ErrorAction SilentlyContinue
if ($null -ne $task) {
    Unregister-ScheduledTask -TaskName "Puentes" -Confirm:$false
}
Write-Host "Se quito el inicio automatico de Puentes."
