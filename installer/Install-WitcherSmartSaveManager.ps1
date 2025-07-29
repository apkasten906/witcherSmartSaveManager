# Witcher Smart Save Manager Interactive Installer
# This script provides a user-friendly way to install the application with directory selection

param(
    [string]$InstallPath = "",
    [switch]$Silent = $false
)

Write-Host "=== Witcher Smart Save Manager Installer ===" -ForegroundColor Cyan
Write-Host ""

# Function to show folder browser dialog
function Show-FolderBrowser {
    param([string]$Description = "Select installation folder")
    
    Add-Type -AssemblyName System.Windows.Forms
    $folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
    $folderBrowser.Description = $Description
    $folderBrowser.RootFolder = [System.Environment+SpecialFolder]::MyComputer
    $folderBrowser.SelectedPath = "$env:ProgramFiles\WitcherSmartSaveManager"
    
    if ($folderBrowser.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
        return $folderBrowser.SelectedPath
    }
    return $null
}

# Get installation path
if (-not $Silent -and -not $InstallPath) {
    Write-Host "Choose installation directory..." -ForegroundColor Yellow
    Write-Host "Default: $env:ProgramFiles\WitcherSmartSaveManager" -ForegroundColor Gray
    Write-Host ""
    
    $choice = Read-Host "Use default location? (Y/n)"
    if ($choice -eq "n" -or $choice -eq "N") {
        $InstallPath = Show-FolderBrowser -Description "Select Witcher Smart Save Manager installation folder"
        if (-not $InstallPath) {
            Write-Host "Installation cancelled by user." -ForegroundColor Red
            exit 1
        }
    }
    else {
        $InstallPath = "$env:ProgramFiles\WitcherSmartSaveManager"
    }
}

if (-not $InstallPath) {
    $InstallPath = "$env:ProgramFiles\WitcherSmartSaveManager"
}

Write-Host "Installation path: $InstallPath" -ForegroundColor Green
Write-Host ""

# Check if MSI exists
$msiPath = Join-Path $PSScriptRoot "bin\WitcherSmartSaveManagerInstaller.msi"
if (-not (Test-Path $msiPath)) {
    Write-Host "ERROR: Installer MSI not found at: $msiPath" -ForegroundColor Red
    Write-Host "Please run Build-Installer.ps1 first to create the installer." -ForegroundColor Yellow
    exit 1
}

# Install the application
Write-Host "Installing Witcher Smart Save Manager..." -ForegroundColor Yellow

try {
    if ($Silent) {
        $msiArgs = @(
            "/i", "`"$msiPath`"",
            "/quiet",
            "INSTALLFOLDER=`"$InstallPath`""
        )
    }
    else {
        $msiArgs = @(
            "/i", "`"$msiPath`"",
            "INSTALLFOLDER=`"$InstallPath`""
        )
    }
    
    Write-Host "Running: msiexec $($msiArgs -join ' ')" -ForegroundColor Gray
    $process = Start-Process -FilePath "msiexec.exe" -ArgumentList $msiArgs -Wait -PassThru
    
    if ($process.ExitCode -eq 0) {
        Write-Host ""
        Write-Host "✅ Installation completed successfully!" -ForegroundColor Green
        Write-Host "Application installed to: $InstallPath" -ForegroundColor Green
        Write-Host ""
        Write-Host "You can now find 'Witcher Smart Save Manager' in your Start Menu." -ForegroundColor Cyan
    }
    else {
        Write-Host ""
        Write-Host "❌ Installation failed with exit code: $($process.ExitCode)" -ForegroundColor Red
        exit $process.ExitCode
    }
}
catch {
    Write-Host ""
    Write-Host "❌ Installation failed with error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
