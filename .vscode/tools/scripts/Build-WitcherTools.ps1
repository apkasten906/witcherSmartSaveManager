# Build Script for WitcherCI
# Builds all Witcher analysis tools

param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

function Write-Status {
    param([string]$Message)
    Write-Host "[BUILD-TOOLS] $Message" -ForegroundColor Yellow
}

try {
    Write-Status "Building Witcher Smart Save Manager tools"
    Write-Status "Configuration: $Configuration"
    
    # Navigate to project root (go up from .vscode/tools/scripts/)
    $projectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
    Set-Location $projectRoot
    
    # Build main solution
    Write-Status "Building main solution..."
    dotnet build --configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build main solution"
    }
    
    # Build DZIP tools specifically
    Write-Status "Building DZIP Analysis Console..."
    dotnet build DZipAnalysisConsole --configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build DZipAnalysisConsole"
    }
    
    Write-Status "Building DZIP Test Console..."
    dotnet build DZipTestConsole --configuration $Configuration  
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build DZipTestConsole"
    }
    
    Write-Status "All tools built successfully!"
}
catch {
    Write-Error "Build failed: $($_.Exception.Message)"
    exit 1
}
