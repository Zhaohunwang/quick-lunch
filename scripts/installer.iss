#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#define AppName "ProjectHub"
#define ExeName "ProjectHub.Desktop"

#ifndef PublishDir
  #define PublishDir "."
#endif

#ifndef OutputDir
  #define OutputDir "."
#endif

#ifndef RuntimeId
  #define RuntimeId "win-x64"
#endif

#if StringCount(AppVersion, ".") == 2
  #define FullVersion AppVersion + ".0"
#else
  #define FullVersion AppVersion
#endif

#ifndef ProjectDir
  #define ProjectDir ".."
#endif

[Setup]
AppId={{B7E3F5A2-8C4D-4E6F-9A1B-2D3E4F5A6B7C}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppName}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
OutputDir={#OutputDir}
OutputBaseFilename={#AppName}-Setup-{#AppVersion}-{#RuntimeId}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
UninstallDisplayName={#AppName} {#AppVersion}
UninstallDisplayIcon={app}\{#ExeName}.exe
VersionInfoVersion={#FullVersion}
VersionInfoCompany={#AppName}
VersionInfoDescription={#AppName} Installation Program
VersionInfoProductName={#AppName}
VersionInfoProductVersion={#AppVersion}
SetupIconFile={#ProjectDir}\ProjectHub.Desktop\Assets\app-icon.ico
UninstallLogfile={app}\uninstall.log

[Languages]
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#ExeName}.exe"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#ExeName}.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#AppName}"; Filename: "{app}\{#ExeName}.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#ExeName}.exe"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup: Boolean;
begin
  Result := True;
end;
