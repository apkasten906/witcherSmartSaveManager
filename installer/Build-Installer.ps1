# Build script for Witcher Smart Save Manager Installer
# This script builds the WiX installer after ensuring the application is built

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory=$false)]
    [switch]$Verbose
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
} else {
    Write-Host "`nStep 1: Skipping application build..." -ForegroundColor Yellow
}

# Step 2: Verify WiX Toolset is installed
Write-Host "`nStep 2: Checking WiX Toolset installation..." -ForegroundColor Green

$WixPath = Get-Command "candle.exe" -ErrorAction SilentlyContinue
if (-not $WixPath) {
    # Try common installation paths
    $PossiblePaths = @(
        "${env:ProgramFiles(x86)}\WiX Toolset v3.11\bin\candle.exe",
        "${env:ProgramFiles}\WiX Toolset v3.11\bin\candle.exe",
        "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v7.0A\Bin\NETFX 4.0 Tools\candle.exe"
    )
    
    foreach ($Path in $PossiblePaths) {
        if (Test-Path $Path) {
            $WixBinDir = Split-Path -Parent $Path
            $env:PATH = "$WixBinDir;$env:PATH"
            $WixPath = Get-Command "candle.exe" -ErrorAction SilentlyContinue
            break
        }
    }
}

if (-not $WixPath) {
    Write-Error @"
WiX Toolset not found! Please install WiX Toolset v3.11 or newer.
Download from: https://wixtoolset.org/releases/

After installation, ensure the WiX bin directory is in your PATH.
"@
    exit 1
}

Write-Host "WiX Toolset found at: $($WixPath.Source)" -ForegroundColor Green

# Step 3: Check if application files exist
Write-Host "`nStep 3: Verifying application files..." -ForegroundColor Green

$RequiredFiles = @(
    "WitcherGuiApp.exe",
    "WitcherGuiApp.dll"
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
    New-Item -ItemType Directory -Path "bin\$Configuration" -Force | Out-Null
    New-Item -ItemType Directory -Path "obj\$Configuration" -Force | Out-Null
    
    # Compile WiX source
    Write-Host "Compiling WiX source files..." -ForegroundColor Yellow
    $CandleArgs = @(
        "WitcherSmartSaveManagerInstaller.wxs"
        "-ext", "WixNetFxExtension"
        "-out", "obj\$Configuration\"
    )
    
    if ($Verbose) {
        $CandleArgs += "-v"
    }
    
    & candle.exe @CandleArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "WiX compilation failed with exit code $LASTEXITCODE"
    }
    
    # Link installer
    Write-Host "Linking installer..." -ForegroundColor Yellow
    $LightArgs = @(
        "obj\$Configuration\WitcherSmartSaveManagerInstaller.wixobj"
        "-ext", "WixNetFxExtension"
        "-ext", "WixUIExtension"
        "-out", "bin\$Configuration\WitcherSmartSaveManagerInstaller.msi"
    )
    
    if ($Verbose) {
        $LightArgs += "-v"
    }
    
    & light.exe @LightArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "WiX linking failed with exit code $LASTEXITCODE"
    }
    
    Write-Host "`nInstaller built successfully!" -ForegroundColor Green
    $OutputPath = Join-Path $InstallerDir "bin\$Configuration\WitcherSmartSaveManagerInstaller.msi"
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
