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
    
    # Navigate to project root (go up from .vscode/tools/witcherci/scripts/)
    $projectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
    Set-Location $projectRoot
    
    # Build the core library if needed
    dotnet build WitcherCore --verbosity quiet | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build WitcherCore"
    }
    
    # Use our DZipDecompressor directly via PowerShell
    Write-Status "Loading WitcherCore assembly for DZIP decompression..."
    Add-Type -Path "WitcherCore\bin\Debug\net8.0-windows\WitcherCore.dll"
    
    # Create decompressor instance
    $decompressor = New-Object WitcherCore.Services.DZipDecompressor
    
    # Find save files to test
    $backupDir = "savesAnalysis\_backup"
    if (Test-Path $SavePath -PathType Leaf) {
        $saveFiles = @($SavePath)
        Write-Status "Testing specific file: $SavePath"
    }
    elseif (Test-Path $backupDir) {
        $saveFiles = Get-ChildItem -Path $backupDir -Filter "*.sav" | Select-Object -First $Count | ForEach-Object { $_.FullName }
        Write-Status "Testing $($saveFiles.Count) files from backup directory"
    }
    else {
        throw "No save files found. Expected directory: $backupDir"
    }
    
    # Test decompression on each file
    foreach ($saveFile in $saveFiles) {
        Write-Host ""
        Write-Host "Testing: $(Split-Path -Leaf $saveFile)" -ForegroundColor White
        Write-Host ("-" * 50) -ForegroundColor Gray
        
        try {
            $result = $decompressor.DecompressSaveFile($saveFile)
            
            if ($result.Success) {
                Write-Host "Success: Uncompressed size: $($result.UncompressedSize) bytes" -ForegroundColor Green
                Write-Host "Compression Ratio: $($result.CompressionRatio.ToString('P2'))" -ForegroundColor Green
                
                if ($result.Header) {
                    Write-Host "DZIP Version: $($result.Header.Version)" -ForegroundColor Cyan
                    Write-Host "Compression Type: $($result.Header.CompressionType)" -ForegroundColor Cyan
                    Write-Host "Expected Size: $($result.Header.UncompressedSize)" -ForegroundColor Cyan
                }
                
                # Show data preview
                if ($result.UncompressedData -and $result.UncompressedData.Length -ge 16) {
                    $hexPreview = ($result.UncompressedData[0..15] | ForEach-Object { $_.ToString("X2") }) -join " "
                    Write-Host "Data Preview: $hexPreview" -ForegroundColor Yellow
                }
            }
            else {
                Write-Host "Failed: $($result.ErrorMessage)" -ForegroundColor Red
            }
        }
        catch {
            Write-Host "Exception: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    Write-Status "DZIP decompression analysis completed successfully"
}
catch {
    Write-Error "DZIP Decompression failed: $($_.Exception.Message)"
    exit 1
}
