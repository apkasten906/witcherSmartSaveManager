# DZIP Analysis Script for WitcherCI
# Analyzes DZIP header structure of Witcher 2 save files

param(
    [string]$SavePath = "savesAnalysis\_backup",
    [string]$OutputFormat = "console"
)

$ErrorActionPreference = "Stop"

function Write-Status {
    param([string]$Message)
    Write-Host "[DZIP-ANALYZE] $Message" -ForegroundColor Cyan
}

try {
    Write-Status "Starting DZIP header analysis"
    Write-Status "Save Path: $SavePath"
    Write-Status "Output Format: $OutputFormat"
    
    # Navigate to project root (go up from .vscode/tools/witcherci/scripts/)
    $projectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
    Set-Location $projectRoot
    
    # Build the core library if needed
    dotnet build WitcherCore --verbosity quiet | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build WitcherCore"
    }
    
    # Use our DZipDecompressor directly via PowerShell for header analysis
    Write-Status "Loading WitcherCore assembly for DZIP header analysis..."
    Add-Type -Path "WitcherCore\bin\Debug\net8.0-windows\WitcherCore.dll"
    
    # Create decompressor instance
    $decompressor = New-Object WitcherCore.Services.DZipDecompressor
    
    # Find save files to analyze
    $backupDir = "savesAnalysis\_backup"
    if (Test-Path $SavePath -PathType Leaf) {
        $saveFiles = @($SavePath)
        Write-Status "Analyzing specific file: $SavePath"
    }
    elseif (Test-Path $backupDir) {
        $saveFiles = Get-ChildItem -Path $backupDir -Filter "*.sav" | Select-Object -First 5 | ForEach-Object { $_.FullName }
        Write-Status "Analyzing headers of $($saveFiles.Count) files from backup directory"
    }
    else {
        throw "No save files found. Expected directory: $backupDir"
    }
    
    # Analyze headers
    $results = @()
    foreach ($saveFile in $saveFiles) {
        Write-Host ""
        Write-Host "Analyzing: $(Split-Path -Leaf $saveFile)" -ForegroundColor White
        Write-Host ("-" * 60) -ForegroundColor Gray
        
        try {
            $fileData = [System.IO.File]::ReadAllBytes($saveFile)
            Write-Host "File Size: $($fileData.Length) bytes" -ForegroundColor Cyan
            
            # Show hex dump of header
            $hexDump = ($fileData[0..63] | ForEach-Object { $_.ToString("X2") }) -join " "
            $hexLines = for ($i = 0; $i -lt $hexDump.Length; $i += 48) {
                $hexDump.Substring($i, [Math]::Min(48, $hexDump.Length - $i))
            }
            Write-Host "Header Hex Dump (first 64 bytes):" -ForegroundColor Yellow
            foreach ($line in $hexLines) {
                Write-Host "  $line" -ForegroundColor Yellow
            }
            
            # Parse header using our decompressor
            $result = $decompressor.DecompressSaveFile($saveFile)
            if ($result.Header) {
                Write-Host ""
                Write-Host "DZIP Header Analysis:" -ForegroundColor Green
                Write-Host "  Magic: $($result.Header.MagicBytes | ForEach-Object { $_.ToString('X2') } | Join-String -Separator ' ')" -ForegroundColor White
                Write-Host "  Version: $($result.Header.Version)" -ForegroundColor White
                Write-Host "  Compression Type: $($result.Header.CompressionType)" -ForegroundColor White
                Write-Host "  Data Type: $($result.Header.DataType)" -ForegroundColor White
                Write-Host "  Expected Uncompressed Size: $($result.Header.UncompressedSize)" -ForegroundColor White
                Write-Host "  Reserved: $($result.Header.Reserved)" -ForegroundColor White
                
                # Calculate compression ratio
                $compressedPayloadSize = $fileData.Length - 24
                $expectedRatio = $compressedPayloadSize / $result.Header.UncompressedSize
                Write-Host "  Compression Ratio: $($expectedRatio.ToString('P2')) ($compressedPayloadSize -> $($result.Header.UncompressedSize))" -ForegroundColor Cyan
                
                # Store for output formatting
                if ($OutputFormat -eq "json" -or $OutputFormat -eq "csv") {
                    $results += [PSCustomObject]@{
                        FileName = Split-Path -Leaf $saveFile
                        FileSize = $fileData.Length
                        Version = $result.Header.Version
                        CompressionType = $result.Header.CompressionType
                        DataType = $result.Header.DataType
                        UncompressedSize = $result.Header.UncompressedSize
                        CompressionRatio = $expectedRatio
                        Success = $result.Success
                    }
                }
            }
        }
        catch {
            Write-Host "✗ Exception: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    # Output formatted results if requested
    if ($OutputFormat -eq "json" -and $results.Count -gt 0) {
        Write-Host ""
        Write-Host "JSON Output:" -ForegroundColor Green
        $results | ConvertTo-Json -Depth 2
    }
    elseif ($OutputFormat -eq "csv" -and $results.Count -gt 0) {
        Write-Host ""
        Write-Host "CSV Output:" -ForegroundColor Green
        $results | ConvertTo-Csv -NoTypeInformation
    }
    
    Write-Status "DZIP analysis completed successfully"
}
catch {
    Write-Error "DZIP Analysis failed: $($_.Exception.Message)"
    exit 1
}
