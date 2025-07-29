# Build script for Witcher Smart Save Manager
# This script builds the main application

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory = $false)]
    [switch]$VerboseOutput,
    
    [Parameter(Mandatory = $false)]
    [string]$Version = "1.0.0"
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$FrontendDir = Join-Path $RootDir "frontend"

Write-Host "Building Witcher Smart Save Manager..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Step 1: Build the main application
if (-not $SkipBuild) {
    Write-Host "`nStep 1: Building main application..." -ForegroundColor Green
    
    Push-Location $FrontendDir
    try {
        Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
        dotnet restore WitcherSmartSaveManager.csproj
        
        Write-Host "Building application in $Configuration mode..." -ForegroundColor Yellow
        dotnet build WitcherSmartSaveManager.csproj --configuration $Configuration --no-restore
        
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

# Step 1.5: Publish the application
Write-Host "`nStep 1.5: Publishing the application..." -ForegroundColor Green

$PublishDir = Join-Path $RootDir "publish"
if (Test-Path -Path $PublishDir) {
    Remove-Item -Path "$PublishDir/*" -Recurse -Force
}

Push-Location $FrontendDir
try {
    Write-Host "Publishing application to $PublishDir..." -ForegroundColor Yellow
    dotnet publish WitcherSmartSaveManager.csproj --configuration $Configuration --output $PublishDir

    if ($LASTEXITCODE -ne 0) {
        throw "Application publish failed with exit code $LASTEXITCODE"
    }

    Write-Host "Application published successfully!" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Clean the publish folder before building
if (Test-Path -Path "../publish") {
    Remove-Item -Path "../publish/*" -Recurse -Force
}

# Step 2: Verify application files exist
Write-Host "`nStep 2: Verifying application files..." -ForegroundColor Green

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

# Extract version from Git tag
Write-Host "Extracting version from Git tag..." -ForegroundColor Cyan
$Version = git describe --tags --abbrev=0
if (-not $Version) {
    Write-Error "No Git tag found. Please create a tag for versioning."
    exit 1
}

# Remove 'v' prefix if present
$Version = $Version -replace '^v', ''
Write-Host "Version extracted: $Version" -ForegroundColor Green

# Update version in .csproj
$CsprojPath = Join-Path $FrontendDir "WitcherSmartSaveManager.csproj"
(Get-Content $CsprojPath) -replace '<Version>.*</Version>', "<Version>$Version</Version>" |
Set-Content $CsprojPath
Write-Host "Updated project version to $Version" -ForegroundColor Green

# Update version in setup.iss
$SetupScriptPath = Join-Path $ScriptDir "setup.iss"
(Get-Content $SetupScriptPath) -replace 'AppVersion=.*', "AppVersion=$Version" |
Set-Content $SetupScriptPath
Write-Host "Updated installer version to $Version" -ForegroundColor Green

# Step 3: Generate the installer
Write-Host "`nStep 3: Generating the installer..." -ForegroundColor Green

$InnoSetupCompiler = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $InnoSetupCompiler)) {
    Write-Error "Inno Setup Compiler not found at $InnoSetupCompiler. Please install Inno Setup 6."
    exit 1
}

& $InnoSetupCompiler $SetupScriptPath
if ($LASTEXITCODE -ne 0) {
    throw "Installer generation failed with exit code $LASTEXITCODE"
}

Write-Host "Installer generated successfully!" -ForegroundColor Green

Write-Host "`n✅ Build completed successfully!" -ForegroundColor Green
Write-Host "Application binaries are available in: $BinPath" -ForegroundColor Cyan
