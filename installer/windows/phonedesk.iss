; Inno Setup script for PhoneDesk.
; Built in CI:  iscc /DAppVersion=<version> /DPublishDir=<publish output> installer\windows\phonedesk.iss
; Per-user install (no admin/UAC), Start-menu shortcut, uninstaller.

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\..\publish\win-x64"
#endif

#define AppName "PhoneDesk"
#define AppExeName "phonedesk.exe"
#define AppPublisher "Patrik Lleshaj"
#define AppURL "https://github.com/realgarit/phonedesk"

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
DefaultDirName={autopf}\PhoneDesk
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog commandline
UsePreviousPrivileges=yes
OutputBaseFilename=phonedesk-win-x64-setup
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

[InstallDelete]
; Pre-rebrand leftovers (app was "Teams Phone Manager" with teams-phonemanager.exe
; until v3.23.x): the renamed exe and shortcuts are not overwritten by the new
; file set, so remove them explicitly on upgrade.
Type: files; Name: "{app}\teams-phonemanager.exe"
Type: files; Name: "{autoprograms}\Teams Phone Manager.lnk"
Type: files; Name: "{autodesktop}\Teams Phone Manager.lnk"

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
