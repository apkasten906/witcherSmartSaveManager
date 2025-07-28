# Build script for Witcher Smart Save Manager Installer
# This script builds the WiX installer after ensuring the application is built

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory = $false)]
    [switch]$VerboseOutput
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$InstallerDir = $ScriptDir
$FrontendDir = Join-Path $RootDir "frontend"

Write-Host "Building Witcher Smart Save Manager Installer..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Step 1: Build the main application if not skipped
if (-not $SkipBuild) {
    Write-Host "`nStep 1: Building main application..." -ForegroundColor Green
    
    Push-Location $FrontendDir
    try {
        Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
        dotnet restore
        
        Write-Host "Building application in $Configuration mode..." -ForegroundColor Yellow
        dotnet build --configuration $Configuration --no-restore
        
        if ($LASTEXITCODE -ne 0) {
            throw "Application build failed with exit code $LASTEXITCODE"
        }
        
        Write-Host "Application build completed successfully!" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Host "`nStep 1: Skipping application build..." -ForegroundColor Yellow
}

# Step 2: Verify WiX Toolset is installed
Write-Host "`nStep 2: Checking WiX Toolset installation..." -ForegroundColor Green

$WixPath = Get-Command "wix" -ErrorAction SilentlyContinue
if (-not $WixPath) {
    Write-Error @"
WiX Toolset v6 not found! Please install WiX Toolset v6 as a .NET global tool:

    dotnet tool install --global wix

For more information, visit: https://wixtoolset.org/
"@
    exit 1
}

# Check WiX version
$WixVersion = & wix --version
Write-Host "WiX Toolset found: v$WixVersion" -ForegroundColor Green

# Step 3: Check if application files exist
Write-Host "`nStep 3: Verifying application files..." -ForegroundColor Green

$RequiredFiles = @(
    "WitcherSmartSaveManager.exe",
    "WitcherSmartSaveManager.dll"
)

$BinPath = Join-Path $FrontendDir "bin\$Configuration\net8.0-windows"
foreach ($File in $RequiredFiles) {
    $FilePath = Join-Path $BinPath $File
    if (-not (Test-Path $FilePath)) {
        Write-Error "Required file not found: $FilePath`nPlease ensure the application is built first."
        exit 1
    }
}

Write-Host "All required application files found!" -ForegroundColor Green

# Step 4: Build the installer
Write-Host "`nStep 4: Building WiX installer..." -ForegroundColor Green

Push-Location $InstallerDir
try {
    # Clean previous build
    if (Test-Path "bin") {
        Remove-Item "bin" -Recurse -Force
    }
    if (Test-Path "obj") {
        Remove-Item "obj" -Recurse -Force
    }
    
    # Create output directories
    New-Item -ItemType Directory -Path "bin" -Force | Out-Null
    
    # Build installer using WiX v6
    Write-Host "Building installer with WiX v6..." -ForegroundColor Yellow
    
    $WixArgs = @(
        "build"
        "-arch", "x64"
        "-d", "SourceDir=$RootDir\"
        "-o", "bin\WitcherSmartSaveManagerInstaller.msi"
        "WitcherSmartSaveManagerInstaller.wxs"
    )
    
    if ($VerboseOutput) {
        $WixArgs += "-v"
    }
    
    & wix @WixArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "WiX build failed with exit code $LASTEXITCODE"
    }
    
    Write-Host "`nInstaller built successfully!" -ForegroundColor Green
    $OutputPath = Join-Path $InstallerDir "bin\WitcherSmartSaveManagerInstaller.msi"
    Write-Host "Output: $OutputPath" -ForegroundColor Cyan
    
    # Display file size
    $FileInfo = Get-Item $OutputPath
    $FileSizeMB = [math]::Round($FileInfo.Length / 1MB, 2)
    Write-Host "Size: $FileSizeMB MB" -ForegroundColor Cyan
}
catch {
    Write-Error "Installer build failed: $_"
    exit 1
}
finally {
    Pop-Location
}

Write-Host "`n✅ Build completed successfully!" -ForegroundColor Green
Write-Host "`nTo install silently, run:" -ForegroundColor Yellow
Write-Host "msiexec /i WitcherSmartSaveManagerInstaller.msi /quiet" -ForegroundColor White
