# Simple DZIP Header Analysis - PowerShell Native
# Quick Phase 1.1 implementation for immediate results

param(
    [Parameter(Mandatory = $true)]
    [string]${save-path},
    [string]${output-format} = "console"
)

$ErrorActionPreference = "Stop"

function Write-Status {
    param([string]$Message)
    Write-Host "[DZIP-SIMPLE] $Message" -ForegroundColor Green
}

function Read-DZipHeader {
    param([string]$FilePath)
    
    $bytes = [System.IO.File]::ReadAllBytes($FilePath)
    
    # Read first 64 bytes for header analysis
    $header = $bytes[0..63]
    
    # Check for DZIP magic number
    $magic = [System.Text.Encoding]::ASCII.GetString($header[0..3])
    
    # Look for Polish text patterns in first 1KB (CD Projekt RED is Polish)
    $searchBytes = $bytes[0..[Math]::Min(1023, $bytes.Length - 1)]
    $polishIndicators = @()
    
    # Common Polish characters that might appear (using safe ASCII representations)
    $polishCharBytes = @(
        @(196, 133),  # ą
        @(196, 135),  # ć  
        @(196, 153),  # ę
        @(197, 130),  # ł
        @(197, 132),  # ń
        @(195, 179),  # ó
        @(197, 155),  # ś
        @(197, 186),  # ź
        @(197, 188)   # ż
    )
    
    # Search for Polish character byte patterns
    for ($i = 0; $i -lt ($searchBytes.Length - 1); $i++) {
        foreach ($polishChar in $polishCharBytes) {
            if ($searchBytes[$i] -eq $polishChar[0] -and $searchBytes[$i + 1] -eq $polishChar[1]) {
                $polishIndicators += "Polish_char_at_$i"
            }
        }
    }
    
    $searchText = [System.Text.Encoding]::UTF8.GetString($searchBytes)
    
    # Look for potential quest-related strings (both English and Polish)
    $questKeywords = @("quest", "zadanie", "misja", "cel", "objective", "progress")
    $foundKeywords = @()
    
    foreach ($keyword in $questKeywords) {
        if ($searchText.ToLower().Contains($keyword.ToLower())) {
            $foundKeywords += $keyword
        }
    }
    
    $result = @{
        FilePath           = $FilePath
        FileSize           = $bytes.Length
        Magic              = $magic
        IsDZIP             = ($magic -eq "DZIP")
        HeaderBytes        = $header
        FirstBytesHex      = ($header[0..15] | ForEach-Object { "{0:X2}" -f $_ }) -join " "
        PolishCharsFound   = $polishIndicators
        QuestKeywordsFound = $foundKeywords
        HasPolishContent   = $polishIndicators.Count -gt 0
    }
    
    return $result
}

try {
    Write-Status "Starting simple DZIP header analysis"
    Write-Status "Save Path: '${save-path}'"
    Write-Status "Output Format: '${output-format}'"
    
    # Navigate to project root and resolve the save path correctly
    $projectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
    Set-Location $projectRoot
    Write-Status "Working directory: $(Get-Location)"
    
    # Resolve full path for save file
    $fullSavePath = Join-Path $projectRoot ${save-path}
    Write-Status "Looking for save file: $fullSavePath"
    
    if (-not (Test-Path $fullSavePath)) {
        throw "Save file not found: $fullSavePath"
    }
    
    $analysis = Read-DZipHeader $fullSavePath
    
    Write-Status "Analysis Results:"
    Write-Host "  File: $($analysis.FilePath)" -ForegroundColor White
    Write-Host "  Size: $($analysis.FileSize) bytes" -ForegroundColor White
    Write-Host "  Magic: '$($analysis.Magic)'" -ForegroundColor $(if ($analysis.IsDZIP) { "Green" } else { "Red" })
    Write-Host "  DZIP Format: $($analysis.IsDZIP)" -ForegroundColor $(if ($analysis.IsDZIP) { "Green" } else { "Red" })
    Write-Host "  First 16 bytes: $($analysis.FirstBytesHex)" -ForegroundColor Cyan
    
    # Polish language awareness (CD Projekt RED)
    if ($analysis.HasPolishContent) {
        Write-Host "  🇵🇱 Polish chars found: $($analysis.PolishCharsFound -join ', ')" -ForegroundColor Yellow
    }
    else {
        Write-Host "  🇵🇱 No Polish characters detected in first 1KB" -ForegroundColor Gray
    }
    
    if ($analysis.QuestKeywordsFound.Count -gt 0) {
        Write-Host "  🎯 Quest keywords: $($analysis.QuestKeywordsFound -join ', ')" -ForegroundColor Magenta
    }
    
    if (${output-format} -eq "json") {
        $jsonOutput = $analysis | ConvertTo-Json -Depth 3
        Write-Host "JSON Output:" -ForegroundColor Yellow
        Write-Host $jsonOutput -ForegroundColor White
    }
    
    Write-Status "Simple DZIP analysis completed successfully"
}
catch {
    Write-Error "Simple DZIP Analysis failed: $($_.Exception.Message)"
    exit 1
}
