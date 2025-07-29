# Testing the Windows Installer

This guide walks you through testing the WiX-based installer for Witcher Smart Save Manager.

## 🛠️ Prerequisites Setup

### Step 1: Install WiX Toolset v6
**Updated**: Our installer now uses **WiX v6** with modern syntax and improved tooling.

**Option A: .NET Global Tool (Recommended)**
```powershell
# Install WiX v6 as .NET global tool
dotnet tool install --global wix

# Verify installation
wix --version
```

**Option B: Manual Installation**
1. **Download WiX v6**: Go to https://github.com/wixtoolset/wix/releases
2. **Install**: Download and install with default settings
3. **Add to PATH**: Ensure the WiX tools directory is in your PATH

**Verify Installation**: Open a new PowerShell window and run:
```powershell
wix --version
```

**Note**: WiX v6 uses a single `wix` command instead of separate `candle` and `light` tools.

### Step 2: Ensure Application is Built
```powershell
# From the repository root
cd frontend
dotnet build --configuration Release
```

## 🧪 Testing Procedure

### Build the Installer
```powershell
# From the installer directory
cd installer

# Option A: Use the build script (if it works)
.\Build-WixInstaller.ps1

# Option B: Build directly with WiX v6
wix build -arch x64 -d SourceDir="C:\Development\witcherSmartSaveManager\" -o bin\WitcherSmartSaveManagerInstaller.msi WitcherSmartSaveManagerInstaller.v6-simple.wxs
```

**Expected Output:**
- ✅ Application build verification
- ✅ WiX v6 compilation success  
- ✅ Installer creation: `bin\WitcherSmartSaveManagerInstaller.msi`

### Test Installation

#### Option A: Interactive Testing
1. **Run Installer**: Double-click `WitcherSmartSaveManagerInstaller.msi`
2. **Follow Wizard**: Complete installation steps
3. **Verify Installation**:
   - Check: `C:\Program Files\WitcherSmartSaveManager\`
   - Check Start Menu: "Witcher Smart Save Manager"
   - Optional: Check Desktop shortcut

#### Option B: Silent Testing
```powershell
# Silent install
msiexec /i WitcherSmartSaveManagerInstaller.msi /quiet

# Verify files exist
Get-ChildItem "C:\Program Files\WitcherSmartSaveManager"

# Check registry entries
Get-ItemProperty "HKLM:\Software\WitcherSmartSaveManager"
```

### Test Application Launch
1. **From Start Menu**: Launch "Witcher Smart Save Manager"
2. **From Install Directory**: Run `WitcherSmartSaveManager.exe`
3. **Verify**: Application loads without errors

### Test Uninstallation

#### Via Windows Settings (Recommended)
1. **Windows 10/11**: Settings → Apps → "Witcher Smart Save Manager" → Uninstall
2. **Windows 10**: Control Panel → Programs → "Witcher Smart Save Manager" → Uninstall

#### Via Command Line
```powershell
# Silent uninstall
msiexec /x WitcherSmartSaveManagerInstaller.msi /quiet

# Verify cleanup
Test-Path "C:\Program Files\WitcherSmartSaveManager" # Should be False
```

## ✅ Verification Checklist

### After Installation
- [ ] Files installed in `C:\Program Files\WitcherSmartSaveManager\`
- [ ] Start Menu shortcut created
- [ ] Desktop shortcut created (if selected)
- [ ] Registry entries present:
  - [ ] `HKLM\Software\WitcherSmartSaveManager\InstallPath`
  - [ ] `HKLM\Software\WitcherSmartSaveManager\Version`
- [ ] Appears in "Add or Remove Programs"
- [ ] Application launches successfully

### After Uninstallation
- [ ] All program files removed
- [ ] Start Menu shortcuts removed
- [ ] Desktop shortcut removed
- [ ] Registry entries cleaned up
- [ ] Removed from "Add or Remove Programs"
- [ ] User data preserved (if any)

## 🔍 Troubleshooting

### Build Issues

**"WiX not found"**
```powershell
# Check WiX v6 installation
wix --version

# If not found, install as .NET global tool
dotnet tool install --global wix

# Or check if it's installed but not in PATH
dotnet tool list --global
```

**"Application not built"**
```powershell
# Build the application first
cd ..\frontend
dotnet build --configuration Release
```

**"File not found errors"**
- Check that all source files exist in expected locations
- Verify the WiX file paths match your project structure

### Installation Issues

**"Administrator privileges required"**
- Right-click PowerShell → "Run as Administrator"
- Or use elevated command prompt

**".NET Runtime missing"**
- The installer should prompt to download .NET 8.0 Desktop Runtime
- Download manually from: https://dotnet.microsoft.com/download/dotnet/8.0

### Testing on Clean Machine

For thorough testing, use a VM or clean machine:
1. **Windows 10/11 VM** without development tools
2. **Install only .NET 8.0 Desktop Runtime**
3. **Test installer end-to-end**

## 📊 Test Results Template

```
## Test Results - [Date]

### Environment
- Windows Version: 
- .NET Runtime: 
- WiX Version: 

### Build Test
- [ ] ✅/❌ Build script executed successfully
- [ ] ✅/❌ MSI file generated
- File Size: ___ MB

### Installation Test
- [ ] ✅/❌ Interactive installation
- [ ] ✅/❌ Silent installation
- [ ] ✅/❌ Files installed correctly
- [ ] ✅/❌ Shortcuts created
- [ ] ✅/❌ Registry entries present

### Application Test
- [ ] ✅/❌ Application launches
- [ ] ✅/❌ No runtime errors
- [ ] ✅/❌ Core functionality works

### Uninstallation Test
- [ ] ✅/❌ Uninstalls via Windows Settings
- [ ] ✅/❌ Silent uninstall works
- [ ] ✅/❌ Complete cleanup

### Notes
[Any issues or observations]
```

## 🚀 Next Steps

Once testing is complete:
1. **Fix any issues** found during testing
2. **Document known limitations**
3. **Create release build** for distribution
4. **Update issue #51** with test results

This installer addresses all requirements from issue #51! 🎯

## 🧪 Automated Testing with Pester

We have implemented automated tests using Pester to validate the installer. These tests ensure that the installation, uninstallation, and other scenarios work as expected.

### Pester Test Scenarios
1. **Default Installation**: Ensures the application installs to the default path.
2. **Custom Path Installation**: Validates installation to a user-specified path.
3. **Silent Installation**: Confirms the installer works without user interaction.
4. **Uninstallation**: Ensures all files and registry entries are removed.
5. **Shortcut Validation**: Checks that Start Menu and Desktop shortcuts are created.
6. **Registry Validation**: Verifies correct registry entries are created.
7. **Error Handling**: Tests for appropriate error messages in edge cases.

### Running Pester Tests
To run the tests, navigate to the `Tests` folder and execute:
```powershell
Invoke-Pester
```
This will run all test scripts and provide a detailed report of the results.

For more details, see the `README.md` in the `installer` folder.
