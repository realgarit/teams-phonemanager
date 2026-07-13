; Inno Setup script for Teams Phone Manager.
; Built in CI:  iscc /DAppVersion=<version> /DPublishDir=<publish output> installer\windows\teams-phonemanager.iss
; Per-user install (no admin/UAC), Start-menu shortcut, uninstaller.

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\..\publish\win-x64"
#endif

#define AppName "Teams Phone Manager"
#define AppExeName "teams-phonemanager.exe"
#define AppPublisher "Patrik Lleshaj"
#define AppURL "https://github.com/realgarit/teams-phonemanager"

[Setup]
; Stable AppId so newer installers upgrade in place.
AppId={{8E1D3A5B-6C0F-4D9E-9B7A-2F4C1A7E5D21}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}/issues
AppUpdatesURL={#AppURL}/releases
DefaultDirName={autopf}\TeamsPhoneManager
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog commandline
UsePreviousPrivileges=yes
OutputBaseFilename=teams-phonemanager-win-x64-setup
SetupIconFile=..\..\assets\icon.ico
UninstallDisplayIcon={app}\{#AppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
LicenseFile=..\..\LICENSE
CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent runasoriginaluser
Filename: "{app}\{#AppExeName}"; Flags: nowait skipifnotsilent runasoriginaluser; Check: ShouldRestartApplication

[Code]
function ShouldRestartApplication(): Boolean;
begin
  Result := ExpandConstant('{param:RESTARTAPP|0}') = '1';
end;
