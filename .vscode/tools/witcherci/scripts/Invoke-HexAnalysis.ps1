# Hex Analysis Script for WitcherCI
# Performs hex pattern analysis on save files

param(
    [Parameter(Mandatory = $true)]
    [string]${save-path},
    [string]$pattern = "quest-data"
)

$ErrorActionPreference = "Stop"

function Write-Status {
    param([string]$Message)
    Write-Host "[HEX-ANALYZE] $Message" -ForegroundColor Magenta
}

try {
    Write-Status "Starting hex pattern analysis"
    Write-Status "Save Path: '${save-path}'"
    Write-Status "Pattern: '$pattern'"
    
    # Navigate to project root
    $projectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
    Set-Location $projectRoot
    Write-Status "Working directory: $(Get-Location)"
    
    if (-not (Test-Path ${save-path})) {
        throw "Save file not found: ${save-path}"
    }
    
    $fileInfo = Get-Item ${save-path}
    Write-Status "Found file: $($fileInfo.Name) ($($fileInfo.Length) bytes)"
    
    Write-Status "Executing WitcherAI Hex Analysis Engine..."
    
    # Path to our advanced hex analyzer
    $witcherAIPath = Join-Path $projectRoot "WitcherAI"
    $hexAnalyzer = Join-Path $witcherAIPath "witcher_hex_analyzer.py"
    
    if (-not (Test-Path $hexAnalyzer)) {
        Write-Status "WitcherAI hex analyzer not found, creating autonomous implementation..."
        
        # Fallback: Create simplified autonomous analysis
        $pythonScript = @"
import struct
import os
from pathlib import Path

def analyze_save_file(file_path, pattern):
    print('🎯 WitcherAI Autonomous Hex Analysis')
    print('=' * 45)
    
    with open(file_path, 'rb') as f:
        data = f.read()
    
    file_size = len(data)
    print(f'📊 File: {Path(file_path).name}')
    print(f'📊 Size: {file_size:,} bytes')
    print()
    
    # Check DZIP format
    if data[:4] == b'DZIP':
        print('✅ DZIP format detected')
        if len(data) >= 12:
            version = struct.unpack('<I', data[4:8])[0]
            size = struct.unpack('<I', data[8:12])[0]
            print(f'  Version: {version}')
            print(f'  Uncompressed size: {size:,} bytes')
        print()
    
    # Pattern analysis based on Phase 2B taxonomy
    patterns = {
        'quest-data': [b'quest', b'chapter', b'active', b'completed'],
        'character-data': [b'Geralt', b'Triss', b'Roche', b'Iorveth'],
        'political-data': [b'political', b'faction', b'stance'],
        'save-metadata': [b'save', b'screenshot', b'header']
    }
    
    target_patterns = patterns.get(pattern, [])
    if not target_patterns:
        target_patterns = [item for sublist in patterns.values() for item in sublist]
    
    print(f'🔍 Pattern Analysis ({pattern}):')
    found_any = False
    
    for search_pattern in target_patterns:
        count = data.count(search_pattern)
        if count > 0:
            found_any = True
            pos = data.find(search_pattern)
            pattern_str = search_pattern.decode('utf-8', errors='ignore')
            print(f'  ✅ {pattern_str}: {count} occurrences (first at 0x{pos:08X})')
    
    if not found_any:
        print('  ❓ No specific patterns detected')
    
    print()
    print('🔬 Hex Preview (first 128 bytes):')
    for i in range(0, min(128, len(data)), 16):
        chunk = data[i:i+16]
        hex_part = ' '.join(f'{b:02x}' for b in chunk)
        ascii_part = ''.join(chr(b) if 32 <= b < 127 else '.' for b in chunk)
        print(f'{i:08x}: {hex_part:<48} |{ascii_part}|')
    
    print()
    print('✅ Autonomous hex analysis complete!')

# Execute analysis
analyze_save_file(r'${save-path}', '$pattern')
"@
        
        $tempScript = Join-Path $env:TEMP "autonomous_hex_analysis.py"
        $pythonScript | Out-File -FilePath $tempScript -Encoding UTF8
        
        $pythonExe = "C:/Users/apkas/AppData/Local/Programs/Python/Python313/python.exe"
        & $pythonExe $tempScript
        
        Remove-Item $tempScript -Force -ErrorAction SilentlyContinue
    }
    else {
        Write-Status "Using WitcherAI advanced hex analyzer..."
        
        # Use our advanced hex analyzer
        $pythonExe = "C:/Users/apkas/AppData/Local/Programs/Python/Python313/python.exe"
        & $pythonExe $hexAnalyzer ${save-path} $pattern
    }
    
    Write-Status "WitcherAI hex analysis completed successfully"
}
catch {
    Write-Error "Hex Analysis failed: $($_.Exception.Message)"
    exit 1
}
