[Setup]
AppId={{DB-LITE-DESKTOP-GUID}}
AppName=DB Lite Desktop
AppVersion=1.0.0
AppPublisher=LightDB
DefaultDirName={autopf}\DbLiteDesktop
DefaultGroupName=DbLiteDesktop
AllowNoIcons=yes
OutputDir=D:\zhuomian\LightDB-Client\publish
OutputBaseFilename=DbLiteDesktop-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Files]
Source: "D:\zhuomian\LightDB-Client\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\DB Lite Desktop"; Filename: "{app}\DbLiteDesktop.exe"
Name: "{group}\卸载 DB Lite Desktop"; Filename: "{uninstallexe}"
Name: "{autodesktop}\DB Lite Desktop"; Filename: "{app}\DbLiteDesktop.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加图标:"; Flags: unchecked

[Run]
Filename: "{app}\DbLiteDesktop.exe"; Description: "启动 DB Lite Desktop"; Flags: nowait postinstall skipifsilent
