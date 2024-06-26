; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "AdamStudio"
#define MyAppPublisher "AdamSoftware"
#define MyAppURL "https://robotco.ru/"
#define MySupportURL = "https://github.com/Adam-Software/AdamStudio"
#define MyAppExeName "AdamController.exe"
#define InstallerIconPath "AdamController.Core\Properties\Icons\main_app_icon.ico"
#define AppReleaseFolderPath "AdamController\bin\Release\net8.0-windows7.0\"
#define MyAppVersion GetVersionNumbersString(AppReleaseFolderPath + MyAppExeName)

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{3844462D-D61F-436E-B805-CAF0A814F605}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf64}\{#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename={#MyAppName}.{#MyAppVersion}.x64
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupIconFile={#InstallerIconPath}
UninstallDisplayIcon={#InstallerIconPath}
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64
RestartIfNeededByRun=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
[Files]
Source: "{#AppReleaseFolderPath}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "AdamInstallBundle\Utils\7za.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall;
Source: "AdamInstallBundle\Source\BlocklySource.zip"; DestDir: "{tmp}"; Flags: deleteafterinstall;
Source: "AdamInstallBundle\Components\MicrosoftEdgeWebView2RuntimeInstaller.125.0.2535.51.X64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall;
Source: "AdamInstallBundle\Components\VC_redist.14.29.30135.0.x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall;
Source: "AdamInstallBundle\Components\windowsdesktop-runtime-8.0.5-win-x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall;
Source: "AdamInstallBundle\Components\aspnetcore-runtime-8.0.5-win-x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall;


[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent;
Filename: "{tmp}\7za.exe"; Parameters: "x ""{tmp}\BlocklySource.zip"" -o""{commonappdata}\AdamController\BlocklySource"" * -r -aoa"; Flags: runasoriginaluser runhidden;
Filename: "{tmp}\MicrosoftEdgeWebView2RuntimeInstaller.125.0.2535.51.X64.exe"; Parameters: "/silent /install";
Filename: "{tmp}\VC_redist.14.29.30135.0.x64.exe"; Parameters: "/install /quiet /norestart"; 
Filename: "{tmp}\windowsdesktop-runtime-8.0.5-win-x64.exe"; Parameters: "/install /quiet /norestart"; 
Filename: "{tmp}\aspnetcore-runtime-8.0.5-win-x64.exe"; Parameters: "/install /quiet /norestart"; 

[Registry]
Root: HKLM64; Subkey: "SOFTWARE\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "AppVersion"; ValueData: "{#MyAppVersion}"; Permissions: users-modify; Flags: uninsdeletevalue


[UninstallDelete]
Type: filesandordirs; Name: "{commonappdata}\AdamController" 



