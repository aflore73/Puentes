#define MyAppName "Puentes"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "Puentes"

[Setup]
AppId={{D00DF264-92C2-45E8-B321-262538A5A70A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\Puentes
DefaultGroupName=Puentes
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\installer
OutputBaseFilename=Puentes-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\orchestrator\Puentes.Orchestrator.exe
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "..\release\Puentes\*"; DestDir: "{app}"; \
    Flags: ignoreversion recursesubdirs createallsubdirs; \
    Excludes: "config\openai.key,logs\*"

[Icons]
Name: "{group}\Iniciar Puentes"; \
    Filename: "powershell.exe"; \
    Parameters: "-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File ""{app}\Start-Puentes.ps1"""
Name: "{group}\Mantenimiento de Puentes"; \
    Filename: "http://127.0.0.1:5121/family"

[Run]
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\Initialize-PuentesApiKey.ps1"""; \
    Flags: runhidden waituntilterminated; Check: SeedExists
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\Install-PuentesStartup.ps1"""; \
    Description: "Iniciar Puentes con Windows"; \
    Flags: postinstall waituntilterminated
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File ""{app}\Start-Puentes.ps1"""; \
    Description: "Iniciar Puentes ahora"; \
    Flags: postinstall nowait skipifsilent

[UninstallRun]
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\Uninstall-PuentesStartup.ps1"""; \
    Flags: runhidden waituntilterminated; RunOnceId: "RemovePuentesStartup"

[Code]
function SeedExists(): Boolean;
begin
  Result := FileExists(ExpandConstant('{app}\config\openai.seed'));
end;
