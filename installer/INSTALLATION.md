# Witcher Smart Save Manager Installation Guide

## 🎮 Quick Installation

### Option 1: Interactive PowerShell Installer (Recommended)
```powershell
.\Install-WitcherSmartSaveManager.ps1
```
This will:
- Ask if you want to use the default location
- Open a folder browser if you want to choose a custom location
- Install the application with proper configuration

### Option 2: Default Location Installation
```cmd
msiexec /i WitcherSmartSaveManagerInstaller.msi
```
Installs to: `C:\Program Files\WitcherSmartSaveManager`

### Option 3: Custom Location Installation
```cmd
msiexec /i WitcherSmartSaveManagerInstaller.msi INSTALLFOLDER="C:\Your\Custom\Path"
```

### Option 4: Silent Installation
```cmd
msiexec /i WitcherSmartSaveManagerInstaller.msi /quiet
```

## 🔧 Installation Issues

If you experience issues with the installer UI:

1. **Small buttons/text**: This is a known issue with WiX v6 UI. Use the PowerShell installer instead.
2. **Folder selection not working**: Use the command-line options above to specify a custom path.
3. **Permission errors**: Run as Administrator if installing to Program Files.

## 📁 Installation Contents

The installer will create:
- Main application: `WitcherSmartSaveManager.exe`
- Configuration files: `userpaths.json`, `App.config`, `NLog.config`
- Documentation: `README.md`, `PRINCIPLES.md`
- Resources: Language files and UI resources
- Start Menu shortcut
- Desktop shortcut (optional)

## 🗑️ Uninstallation

Use Windows "Add or Remove Programs" or:
```cmd
msiexec /x {8B5A2E4C-3F1D-4B9E-A6D7-2C4E8F9A1B3D}
```

## 🎯 Build from Source

To build the installer yourself:
```powershell
.\Build-Installer.ps1
```

This will:
1. Build the application in Release mode
2. Verify all dependencies
3. Create the MSI installer
4. Place the output in the `bin` folder

## 🐺 Wolf Icon

The installer uses a custom wolf icon (`icon_wolf_save.ico`) for:
- Installer branding
- Application shortcuts
- Windows Add/Remove Programs listing

## 🧪 Automated Testing

We have implemented automated tests using Pester to validate the installer. These tests ensure that the installation, uninstallation, and other scenarios work as expected.

### Running Tests
To run the tests, navigate to the `Tests` folder and execute:
```powershell
Invoke-Pester
```
This will run all test scripts and provide a detailed report of the results.

For more details, see the `README.md` in the `installer` folder.

---

**Note**: This installer is built with WiX Toolset v6 and requires .NET 8.0 Desktop Runtime.
