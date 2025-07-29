# Verify-Environment.ps1
# This script checks for prerequisites and cleans up previous installations.

# Check for .NET 8.0 Desktop Runtime
Write-Host "Checking for .NET 8.0 Desktop Runtime..."
$dotnetRuntime = Get-Command "dotnet" -ErrorAction SilentlyContinue
if (-not $dotnetRuntime) {
    Write-Host "Error: .NET runtime is not installed. Please install .NET 8.0 Desktop Runtime." -ForegroundColor Red
    exit 1
}

# Verify PowerShell Execution Policy
Write-Host "Verifying PowerShell Execution Policy..."
$executionPolicy = Get-ExecutionPolicy
if ($executionPolicy -ne "RemoteSigned" -and $executionPolicy -ne "Unrestricted") {
    Write-Host "Setting Execution Policy to RemoteSigned..."
    Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned -Force
}

# Clean up previous installations
Write-Host "Cleaning up previous installations..."
$installPath = "C:\Program Files\WitcherSmartSaveManager"
if (Test-Path $installPath) {
    Write-Host "Removing previous installation at $installPath..."
    Remove-Item -Recurse -Force $installPath
}

# Check for residual registry entries
Write-Host "Checking for residual registry entries..."
$registryPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\WitcherSmartSaveManager"
if (Test-Path $registryPath) {
    Write-Host "Removing residual registry entries..."
    Remove-Item -Recurse -Force $registryPath
}

Write-Host "Environment verification complete. Ready for installation." -ForegroundColor Green
