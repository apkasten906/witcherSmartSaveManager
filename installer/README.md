# Witcher Smart Save Manager - Windows Installer

This directory contains the WiX-based Windows installer for the Witcher Smart Save Manager application.

## 🚀 Quick Build

```powershell
# From the installer directory
.\Build-Installer.ps1
```

## 📋 Requirements

### For Building
- **WiX Toolset v3.11+** - Download from [WiX Toolset Releases](https://wixtoolset.org/releases/)
- **PowerShell 5.1+** (included with Windows 10/11)
- **.NET SDK 8.0+** (for building the main application)

### For Installation
- **Windows 10/11** (64-bit)
- **.NET Framework 4.8+** (installer will check and prompt if missing)

## 🔧 Build Options

```powershell
# Build in Release mode (default)
.\Build-Installer.ps1

# Build in Debug mode
.\Build-Installer.ps1 -Configuration Debug

# Skip rebuilding the main application
.\Build-Installer.ps1 -SkipBuild

# Verbose output for troubleshooting
.\Build-Installer.ps1 -Verbose
```

## 📦 What Gets Installed

### Application Files
- Main executable (`WitcherGuiApp.exe`)
- Application DLL and dependencies
- Runtime configuration files
- Resource files (localization, themes)

### Configuration
- Default configuration files
- User settings template
- Logging configuration

### Documentation
- README.md
- Development principles
- Git hooks documentation

### Registry Entries
- **Install Path**: `HKLM\Software\WitcherSmartSaveManager\InstallPath`
- **Version**: `HKLM\Software\WitcherSmartSaveManager\Version`
- **Uninstall Info**: Standard Windows uninstall registry entries

### Start Menu Integration
- Start Menu shortcut in "Witcher Smart Save Manager" folder
- Optional desktop shortcut
- Proper uninstall integration

## 🎯 Installation Options

### Interactive Installation
```cmd
WitcherSmartSaveManagerInstaller.msi
```

### Silent Installation
```cmd
msiexec /i WitcherSmartSaveManagerInstaller.msi /quiet
```

### Silent Installation with Log
```cmd
msiexec /i WitcherSmartSaveManagerInstaller.msi /quiet /l*v install.log
```

### Uninstall
```cmd
msiexec /x WitcherSmartSaveManagerInstaller.msi /quiet
```

## 🛠 Customization

### Custom Install Directory
```cmd
msiexec /i WitcherSmartSaveManagerInstaller.msi INSTALLFOLDER="C:\MyGames\WitcherSaveManager"
```

### Skip Desktop Shortcut
The installer UI allows users to choose whether to create a desktop shortcut.

## 📁 Output Structure

After building, you'll find:

```
installer/
├── bin/Release/
│   └── WitcherSmartSaveManagerInstaller.msi
├── obj/Release/
│   └── (temporary build files)
└── Build-Installer.ps1
```

## 🔍 Troubleshooting

### WiX Not Found
- Download and install WiX Toolset v3.11+
- Ensure the WiX bin directory is in your PATH
- Restart your terminal/PowerShell session

### Application Not Built
```powershell
# Build the application first
cd ..\frontend
dotnet build --configuration Release
```

### Missing Dependencies
The installer checks for .NET Framework 4.8+ and will prompt users to install if missing.

### Build Errors
Run with `-Verbose` flag for detailed output:
```powershell
.\Build-Installer.ps1 -Verbose
```

## 🧪 Testing

### Test Installation
1. Build the installer
2. Run `WitcherSmartSaveManagerInstaller.msi`
3. Verify installation in `C:\Program Files\WitcherSmartSaveManager`
4. Check Start Menu shortcuts
5. Launch application from Start Menu

### Test Uninstallation
1. Use Windows "Add or Remove Programs"
2. Or run: `msiexec /x WitcherSmartSaveManagerInstaller.msi`
3. Verify all files and registry entries are removed

## 📋 Checklist for Issue #51

- ✅ **WiX Toolset**: Using WiX Toolset for installer creation
- ✅ **Silent Install**: Supports `/quiet` parameter for silent installation
- ✅ **Registry Entries**: InstallPath, Version, and standard uninstall info
- ✅ **Assets & Config**: Includes all application files, config, and documentation
- ✅ **No Custom Branding**: Uses standard WiX UI without custom branding
- ✅ **.NET Runtime Check**: Validates .NET Framework 4.8+ and prompts if missing

## 🚀 Deployment

Once built, the MSI installer can be:
- Distributed via GitHub Releases
- Deployed through Group Policy
- Used for automated enterprise deployment
- Shared directly with end users

The installer is self-contained and includes all necessary files for a complete installation.
