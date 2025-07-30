; Inno Setup Script for Witcher Save Manager
; Generated on July 29, 2025

[Setup]
AppName=Witcher Save Manager
AppVersion=1.1.0
DefaultDirName={code:GetInstallFolder|C:\Program Files (x86)\WitcherSmartSaveManager}
DefaultGroupName=Witcher Save Manager
OutputBaseFilename=WitcherSaveManagerInstaller
SetupIconFile=../frontend/Views/Assets/icon_wolf_save.ico
LicenseFile=../license.txt
PrivilegesRequired=admin
Compression=lzma
SolidCompression=yes
UninstallDisplayName=Witcher Save Manager
UninstallDisplayIcon={app}\WitcherSmartSaveManager.exe
MinVersion=10.0

[Files]
Source: "../publish/*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\Witcher Save Manager"; Filename: "{app}\WitcherSmartSaveManager.exe"
Name: "{userdesktop}\Witcher Save Manager"; Filename: "{app}\WitcherSmartSaveManager.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\WitcherSmartSaveManager.exe"; Description: "Launch Witcher Save Manager"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
function CmdLineParamExists(const Param: string): Boolean;
var
  I: Integer;
begin
  Result := False;
  for I := 1 to ParamCount do
  begin
    if CompareText(Param, ParamStr(I)) = 0 then
    begin
      Result := True;
      Exit;
    end;
  end;
end;

function CmdLineParamStr(const Param: string): string;
var
  I: Integer;
begin
  Result := '';
  for I := 1 to ParamCount do
  begin
    if CompareText(Param, ParamStr(I)) = 0 then
    begin
      if I < ParamCount then
        Result := ParamStr(I + 1);
      Exit;
    end;
  end;
end;

function GetInstallFolder(Default: string): string;
begin
  if CmdLineParamExists('/INSTALLFOLDER') then
    Result := CmdLineParamStr('/INSTALLFOLDER')
  else
    Result := Default;
end;
