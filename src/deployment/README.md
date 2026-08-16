# Release local de Puentes

1. Ejecute `Build-PuentesRelease.ps1` para generar `release\Puentes`.
2. Dentro del release, ejecute una vez `Set-PuentesApiKey.ps1`.
3. Pruebe el sistema ejecutando `Start-Puentes.ps1`.
4. Ejecute `Install-PuentesStartup.ps1` para iniciarlo con Windows.

## Instalador para otra computadora

Instale Inno Setup 6 y ejecute `Build-WindowsInstaller.ps1`. El instalador se
genera en `src\installer`. Si `PUENTES_API_KEY` esta definida al compilar, se
incluye como clave inicial. En el equipo destino se cifra para el usuario de
Windows y se elimina el archivo temporal. El instalador debe tratarse como un
secreto mientras esa clave siga activa.

Marta activa la escucha únicamente con `Enter`. Un tono agudo indica el inicio
de la escucha y un tono más grave confirma que Puentes terminó de capturar la
pregunta. Dos tonos descendentes indican que el turno falló y puede intentarse
nuevamente.

Los registros operativos se guardan en `release\Puentes\logs`. No se guardan
grabaciones de voz. La base existente se conserva al volver a publicar sobre el
mismo directorio.

Diagnóstico:

- API: `http://127.0.0.1:5121/health`
- Orquestador: `http://127.0.0.1:5130/health`
