# Native PowerShell DZIP Decompression
# Phase 1.2 - Extract compressed save file content

param(
    [Parameter(Mandatory = $true)]
    [string]${save-path},
    [int]${bytes-to-extract} = 1024,
    [string]${output-format} = "console"
)

$ErrorActionPreference = "Stop"

function Write-Status {
    param([string]$Message)
    Write-Host "[DZIP-EXTRACT] $Message" -ForegroundColor Blue
}

function Extract-DZipContent {
    param([string]$FilePath, [int]$BytesToExtract)
    
    $bytes = [System.IO.File]::ReadAllBytes($FilePath)
    
    # DZIP Header Analysis
    $magic = [System.Text.Encoding]::ASCII.GetString($bytes[0..3])
    if ($magic -ne "DZIP") {
        throw "Not a DZIP file: $magic"
    }
    
    # Parse DZIP header structure
    $version = [BitConverter]::ToUInt32($bytes, 4)
    $compressionType = [BitConverter]::ToUInt32($bytes, 8) 
    $flags = [BitConverter]::ToUInt32($bytes, 12)
    
    Write-Status "DZIP Header parsed - Version: $version, Compression: $compressionType, Flags: $flags"
    
    # Look for compressed data start (after header)
    $headerSize = 16  # Standard DZIP header
    $compressedData = $bytes[$headerSize..($bytes.Length - 1)]
    
    Write-Status "Found $($compressedData.Length) bytes of compressed data"
    
    # Try to decompress using .NET Deflate
    try {
        $memoryStream = New-Object System.IO.MemoryStream(,$compressedData)
        $deflateStream = New-Object System.IO.Compression.DeflateStream($memoryStream, [System.IO.Compression.CompressionMode]::Decompress)
        
        $buffer = New-Object byte[] $BytesToExtract
        $bytesRead = $deflateStream.Read($buffer, 0, $BytesToExtract)
        
        $deflateStream.Close()
        $memoryStream.Close()
        
        if ($bytesRead -gt 0) {
            $decompressedData = $buffer[0..($bytesRead - 1)]
            Write-Status "Successfully decompressed $bytesRead bytes"
            return $decompressedData
        } else {
            Write-Status "No data decompressed - trying alternative method"
        }
    }
    catch {
        Write-Status "Deflate decompression failed: $($_.Exception.Message)"
    }
    
    # If deflate fails, try zlib format (deflate with header)
    try {
        # Skip potential zlib header (2 bytes)
        $zlibData = $compressedData[2..($compressedData.Length - 1)]
        $memoryStream = New-Object System.IO.MemoryStream(,$zlibData)
        $deflateStream = New-Object System.IO.Compression.DeflateStream($memoryStream, [System.IO.Compression.CompressionMode]::Decompress)
        
        $buffer = New-Object byte[] $BytesToExtract
        $bytesRead = $deflateStream.Read($buffer, 0, $BytesToExtract)
        
        $deflateStream.Close()
        $memoryStream.Close()
        
        if ($bytesRead -gt 0) {
            $decompressedData = $buffer[0..($bytesRead - 1)]
            Write-Status "Successfully decompressed $bytesRead bytes with zlib format"
            return $decompressedData
        }
    }
    catch {
        Write-Status "Zlib decompression failed: $($_.Exception.Message)"
    }
    
    # Return raw header for analysis if decompression fails
    Write-Status "Decompression failed - returning raw header data for analysis"
    return $compressedData[0..[Math]::Min($BytesToExtract - 1, $compressedData.Length - 1)]
}

function Analyze-DecompressedData {
    param([byte[]]$Data)
    
    # Convert to text and look for readable strings
    $text = [System.Text.Encoding]::UTF8.GetString($Data)
    $asciiText = [System.Text.Encoding]::ASCII.GetString($Data)
    
    # Look for quest-related strings
    $questPatterns = @("quest", "zadanie", "objective", "progress", "geralt", "triss", "iorveth", "roche", "letho")
    $foundPatterns = @()
    
    foreach ($pattern in $questPatterns) {
        if ($text.ToLower().Contains($pattern.ToLower()) -or $asciiText.ToLower().Contains($pattern.ToLower())) {
            $foundPatterns += $pattern
        }
    }
    
    # Look for null-terminated strings
    $strings = @()
    $currentString = ""
    foreach ($byte in $Data) {
        if ($byte -eq 0) {
            if ($currentString.Length -gt 3) {
                $strings += $currentString
            }
            $currentString = ""
        } elseif ($byte -ge 32 -and $byte -le 126) {
            $currentString += [char]$byte
        } else {
            if ($currentString.Length -gt 3) {
                $strings += $currentString
            }
            $currentString = ""
        }
    }
    
    return @{
        TextPreview = $text.Substring(0, [Math]::Min(200, $text.Length))
        QuestPatternsFound = $foundPatterns
        ExtractedStrings = $strings[0..[Math]::Min(9, $strings.Length - 1)]
        DataSize = $Data.Length
    }
}

try {
    Write-Status "Starting DZIP decompression"
    Write-Status "Save Path: '${save-path}'"
    Write-Status "Bytes to Extract: ${bytes-to-extract}"
    
    # Navigate to project root and resolve path
    $projectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
    Set-Location $projectRoot
    
    $fullSavePath = Join-Path $projectRoot ${save-path}
    if (-not (Test-Path $fullSavePath)) {
        throw "Save file not found: $fullSavePath"
    }
    
    Write-Status "Extracting from: $fullSavePath"
    $decompressedData = Extract-DZipContent $fullSavePath ${bytes-to-extract}
    
    $analysis = Analyze-DecompressedData $decompressedData
    
    Write-Status "Decompression Analysis Results:"
    Write-Host "  Data Size: $($analysis.DataSize) bytes" -ForegroundColor White
    Write-Host "  Quest Patterns: $($analysis.QuestPatternsFound -join ', ')" -ForegroundColor Green
    Write-Host "  Text Preview: $($analysis.TextPreview)" -ForegroundColor Cyan
    
    if ($analysis.ExtractedStrings.Count -gt 0) {
        Write-Host "  Extracted Strings:" -ForegroundColor Yellow
        foreach ($str in $analysis.ExtractedStrings) {
            Write-Host "    - $str" -ForegroundColor White
        }
    }
    
    if (${output-format} -eq "hex") {
        Write-Host "  Hex Dump (first 256 bytes):" -ForegroundColor Magenta
        $hexBytes = $decompressedData[0..[Math]::Min(255, $decompressedData.Length - 1)]
        for ($i = 0; $i -lt $hexBytes.Length; $i += 16) {
            $lineBytes = $hexBytes[$i..[Math]::Min($i + 15, $hexBytes.Length - 1)]
            $hex = ($lineBytes | ForEach-Object { "{0:X2}" -f $_ }) -join " "
            $ascii = -join ($lineBytes | ForEach-Object { if ($_ -ge 32 -and $_ -le 126) { [char]$_ } else { "." } })
            Write-Host "    $("{0:X4}" -f $i): $hex $ascii" -ForegroundColor Gray
        }
    }
    
    Write-Status "DZIP decompression analysis completed successfully"
}
catch {
    Write-Error "DZIP Decompression failed: $($_.Exception.Message)"
    exit 1
}
