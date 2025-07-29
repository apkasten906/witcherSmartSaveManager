# Witcher Smart Save Manager - Windows Installer

This directory contains the **Inno Setup**-based Windows installer for the Witcher Smart Save Manager application.

## 🚀 Quick Build

```powershell
# From the installer directory
# Use the build script to generate the installer
.\Build-Installer.ps1
```

## 📋 Requirements

### For Building
- **Inno Setup** - Download and install from [Inno Setup Downloads](https://jrsoftware.org/isdl.php)
- **PowerShell 5.1+** (included with Windows 10/11)
- **.NET SDK 8.0+** (for building the main application)

### For Installation
- **Windows 10 (version 1607+) or Windows 11** (64-bit)
- **Windows Server 2016/2019/2022** (64-bit) 
- **.NET 8.0 Desktop Runtime** (installer will check and prompt if missing)
- **No restart required** - registry entries are non-system data storage only

### Compatibility Notes
- **Not supported**: Windows 7, 8, 8.1, or Server 2012 (due to .NET 8.0 requirements)
- **Architecture**: 64-bit Windows required for .NET 8.0 Desktop Runtime
- **Privileges**: Administrator privileges required for installation to Program Files

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
- Main executable (`WitcherSmartSaveManager.exe`)
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
WitcherSmartSaveManagerInstaller.exe
```

### Silent Installation
```cmd
WitcherSmartSaveManagerInstaller.exe /VERYSILENT
```

### Silent Installation with Log
```cmd
WitcherSmartSaveManagerInstaller.exe /VERYSILENT /LOG=install.log
```

### Uninstall
```cmd
# Via Windows Settings/Control Panel (Recommended)
# Windows 10/11: Settings → Apps → "Witcher Smart Save Manager" → Uninstall
# Windows 10: Control Panel → Programs → "Witcher Smart Save Manager" → Uninstall

# Silent uninstall via command line
WitcherSmartSaveManagerInstaller.exe /VERYSILENT /UNINSTALL

# Interactive uninstall via command line  
WitcherSmartSaveManagerInstaller.exe /UNINSTALL
```

## 🛠 Customization

### Custom Install Directory
```cmd
WitcherSmartSaveManagerInstaller.exe /DIR="C:\MyGames\WitcherSaveManager"
```

### Skip Desktop Shortcut
The installer UI allows users to choose whether to create a desktop shortcut.

## 📁 Output Structure

After building, you'll find:

```
installer/
├── Output/
│   └── WitcherSmartSaveManagerInstaller.exe
└── Build-Installer.ps1
```

## 🔍 Troubleshooting

### Inno Setup Not Found
- Download and install Inno Setup from [Inno Setup Downloads](https://jrsoftware.org/isdl.php)
- Ensure the Inno Setup directory is in your PATH
- Restart your terminal/PowerShell session

### Application Not Built
```powershell
# Build the application first
cd ..\frontend
dotnet build --configuration Release
```

### Missing Dependencies
The installer checks for .NET 8.0 Desktop Runtime and will prompt users to install if missing.

### Build Errors
Run with `-Verbose` flag for detailed output:
```powershell
.\Build-Installer.ps1 -Verbose
```

## 🧪 Testing

### Test Installation
1. Build the installer
2. Run `WitcherSmartSaveManagerInstaller.exe`
3. Verify installation in `C:\Program Files\WitcherSmartSaveManager`
4. Check Start Menu shortcuts
5. Launch application from Start Menu

### Test Uninstallation
1. Use Windows "Add or Remove Programs"
2. Or run: `WitcherSmartSaveManagerInstaller.exe /UNINSTALL`
3. Verify all files and registry entries are removed

### What Gets Removed During Uninstall
- **Application Files**: All installed program files in `C:\Program Files\WitcherSmartSaveManager`
- **Start Menu Shortcuts**: Application shortcuts and folder
- **Desktop Shortcut**: If created during installation
- **Registry Entries**: All installation tracking and uninstall registry entries
- **Windows Integration**: Removes from "Add or Remove Programs" list

### What Remains After Uninstall (By Design)
- **User Settings**: Any saved user preferences or custom save locations
- **User-Created Backups**: Backup files created by the application
- **Application Data**: Logs or temporary files in user directories

This follows Windows installer best practices - user data is preserved during uninstall.

## 📋 Checklist for Issue #51

- ✅ **Inno Setup**: Using Inno Setup for installer creation
- ✅ **Silent Install**: Supports `/VERYSILENT` parameter for silent installation
- ✅ **Registry Entries**: InstallPath, Version, and standard uninstall info
- ✅ **Assets & Config**: Includes all application files, config, and documentation
- ✅ **No Custom Branding**: Uses standard Inno Setup UI without custom branding
- ✅ **.NET Runtime Check**: Validates .NET 8.0 Desktop Runtime and prompts if missing

## 🚀 Deployment

Once built, the installer can be:
- Distributed via GitHub Releases
- Deployed through Group Policy
- Used for automated enterprise deployment
- Shared directly with end users

The installer is self-contained and includes all necessary files for a complete installation.

## 🧪 Automated Testing

### Pester Tests
We have implemented automated tests using Pester to validate the installer. These tests cover:

1. **Default Installation**: Ensures the application installs to the default path.
2. **Custom Path Installation**: Validates installation to a user-specified path.
3. **Silent Installation**: Confirms the installer works without user interaction.
4. **Uninstallation**: Ensures all files and registry entries are removed.
5. **Shortcut Validation**: Checks that Start Menu and Desktop shortcuts are created.
6. **Registry Validation**: Verifies correct registry entries are created.
7. **Error Handling**: Tests for appropriate error messages in edge cases.

### Running Tests
To run the tests, navigate to the `Tests` folder and execute:
```powershell
Invoke-Pester
```
This will run all test scripts and provide a detailed report of the results.
