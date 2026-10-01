#ifndef FromVersion
  #define FromVersion "0.0.0"
#endif
#ifndef ToVersion
  #define ToVersion "0.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\delta-staging"
#endif
#ifndef OutputDir
  #define OutputDir "..\release-output"
#endif

#define MyAppName "Project Alpha"
#define MyAppExeName "Baraban.exe"
#define MyAppId "{{5C23B63A-5308-42A5-8EAB-68FF65A70D31}"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#ToVersion}
AppVerName={#MyAppName} {#ToVersion}
DefaultDirName={localappdata}\Programs\Project Alpha
UsePreviousAppDir=yes
Uninstallable=yes
UninstallDisplayName=Project Alpha
UninstallDisplayIcon={app}\{#MyAppExeName}
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
OutputDir={#OutputDir}
OutputBaseFilename=ProjectAlpha-Update-from-v{#FromVersion}-to-v{#ToVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
SetupLogging=yes
DisableProgramGroupPage=yes
DisableReadyPage=yes
DisableFinishedPage=yes

[Files]
Source: "{#SourceDir}\*"; DestDir: "{tmp}\ProjectAlphaUpdate"; Flags: ignoreversion recursesubdirs createallsubdirs deleteafterinstall

[Code]

var
  DeleteUserDataOnUninstall: Boolean;

function InitializeUninstall: Boolean;
begin
  DeleteUserDataOnUninstall := False;

  if not UninstallSilent then
    DeleteUserDataOnUninstall :=
      MsgBox(
        'Удалить также настройки и рабочие данные?',
        mbConfirmation,
        MB_YESNO
      ) = IDYES;

  Result := True;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  UserDataDir: String;
begin
  if (CurUninstallStep = usPostUninstall) and DeleteUserDataOnUninstall then
  begin
    UserDataDir := ExpandConstant('{localappdata}\Baraban');
    if DirExists(UserDataDir) then
      DelTree(UserDataDir, True, True, True);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  PowerShellExe: String;
  ScriptPath: String;
begin
  if CurStep = ssPostInstall then
  begin
    PowerShellExe := ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe');
    ScriptPath := ExpandConstant('{tmp}\ProjectAlphaUpdate\Apply-Update.ps1');

    if not Exec(
      PowerShellExe,
      '-NoProfile -ExecutionPolicy Bypass -File "' + ScriptPath + '"',
      '',
      SW_SHOW,
      ewWaitUntilTerminated,
      ResultCode
    ) then
      RaiseException('Не удалось запустить механизм обновления.');

    if ResultCode <> 0 then
      RaiseException('Обновление не применено. Код ошибки: ' + IntToStr(ResultCode));
  end;
end;
