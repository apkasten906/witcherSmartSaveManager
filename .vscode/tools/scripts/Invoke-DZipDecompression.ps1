# DZIP Decompression Script for WitcherCI  
# Decompresses DZIP save files and analyzes content structure

param(
    [string]$SavePath = "savesAnalysis\_backup",
    [int]$Count = 3
)

$ErrorActionPreference = "Stop"

function Write-Status {
    param([string]$Message)
    Write-Host "[DZIP-DECOMPRESS] $Message" -ForegroundColor Green
}

try {
    Write-Status "Starting DZIP decompression analysis"
    Write-Status "Save Path: $SavePath"
    Write-Status "File Count: $Count"
    
    # Navigate to project root (go up from .vscode/tools/scripts/)
    $projectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
    Set-Location $projectRoot
    
    # Build the decompression tool if needed
    dotnet build DZipTestConsole --verbosity quiet | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build DZipTestConsole"
    }
    
    # Run the decompression test
    if (Test-Path $SavePath -PathType Leaf) {
        # Specific file decompression
        Write-Status "Decompressing specific file: $SavePath"
        dotnet run --project DZipTestConsole -- --file $SavePath
    }
    else {
        # Directory decompression (default behavior)
        Write-Status "Decompressing files from directory (default behavior)"
        dotnet run --project DZipTestConsole -- --count $Count
    }
    
    if ($LASTEXITCODE -ne 0) {
        throw "DZIP decompression failed"
    }
    
    Write-Status "DZIP decompression analysis completed successfully"
}
catch {
    Write-Error "DZIP Decompression failed: $($_.Exception.Message)"
    exit 1
}
