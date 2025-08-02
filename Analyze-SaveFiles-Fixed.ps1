# Witcher 2 Save File Hex Analysis Script
# Phase 1: Structure Discovery

Write-Host "Witcher 2 Save File Format Analyzer" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host "Phase 1: Structure Discovery" -ForegroundColor Yellow
Write-Host ""

$backupDir = ".\savesAnalysis\_backup"

if (-not (Test-Path $backupDir)) {
    Write-Host "ERROR: Backup directory not found: $backupDir" -ForegroundColor Red
    exit 1
}

# Get first few save files for analysis
$saveFiles = Get-ChildItem -Path $backupDir -Filter "*.sav" | Sort-Object LastWriteTime | Select-Object -First 5

Write-Host "Found $($saveFiles.Count) save files for analysis:" -ForegroundColor Cyan
foreach ($file in $saveFiles) {
    $size = [math]::Round($file.Length / 1KB, 2)
    $timeStr = $file.LastWriteTime.ToString('yyyy-MM-dd HH:mm')
    Write-Host "   $($file.Name) ($size KB, $timeStr)" -ForegroundColor White
}
Write-Host ""

# Analyze first file in detail
$firstFile = $saveFiles[0]
Write-Host "Detailed analysis of: $($firstFile.Name)" -ForegroundColor Cyan
Write-Host ("=" * 50) -ForegroundColor Gray

$bytes = [System.IO.File]::ReadAllBytes($firstFile.FullName)
Write-Host "File size: $($bytes.Length) bytes" -ForegroundColor White

# Show first 16 bytes as hex
Write-Host ""
Write-Host "First 16 bytes (hex):" -ForegroundColor Yellow
$first16 = $bytes[0..15]
$hexString = ($first16 | ForEach-Object { $_.ToString("X2") }) -join " "
Write-Host "   $hexString" -ForegroundColor White

# Try to find readable strings
Write-Host ""
Write-Host "Looking for readable strings:" -ForegroundColor Yellow
$stringBuilder = ""
$strings = @()
for ($i = 0; $i -lt [Math]::Min(1024, $bytes.Length); $i++) {
    $byte = $bytes[$i]
    if ($byte -ge 32 -and $byte -le 126) {
        $stringBuilder += [char]$byte
    }
    else {
        if ($stringBuilder.Length -ge 4) {
            $strings += "0x$($i - $stringBuilder.Length):$($stringBuilder.ToString('X4')): '$stringBuilder'"
        }
        $stringBuilder = ""
    }
}

$strings | Select-Object -First 10 | ForEach-Object {
    Write-Host "   $_" -ForegroundColor White
}

# Look for potential timestamps (Unix timestamps in reasonable range)
Write-Host ""
Write-Host "Looking for potential timestamps:" -ForegroundColor Yellow
for ($i = 0; $i -le $bytes.Length - 4; $i += 4) {
    $timestamp = [BitConverter]::ToUInt32($bytes, $i)
    # Check if it's in a reasonable range (2020-2030)
    if ($timestamp -ge 1577836800 -and $timestamp -le 1893456000) {
        $date = [DateTimeOffset]::FromUnixTimeSeconds($timestamp).DateTime
        Write-Host "   0x$($i.ToString('X4')): $timestamp ($($date.ToString('yyyy-MM-dd HH:mm:ss')))" -ForegroundColor White
    }
}

# Compare first few files to see what changes
if ($saveFiles.Count -ge 2) {
    Write-Host ""
    Write-Host "Comparing first 2 files to find differences:" -ForegroundColor Cyan
    Write-Host ("=" * 50) -ForegroundColor Gray
    
    $file1Bytes = [System.IO.File]::ReadAllBytes($saveFiles[0].FullName)
    $file2Bytes = [System.IO.File]::ReadAllBytes($saveFiles[1].FullName)
    
    $minLength = [Math]::Min($file1Bytes.Length, $file2Bytes.Length)
    $differences = 0
    $staticBytes = 0
    
    Write-Host "File 1: $($saveFiles[0].Name) ($($file1Bytes.Length) bytes)" -ForegroundColor White
    Write-Host "File 2: $($saveFiles[1].Name) ($($file2Bytes.Length) bytes)" -ForegroundColor White
    Write-Host ""
    
    # Check first 256 bytes for structure
    Write-Host "Static bytes (same in both files - first 32 bytes):" -ForegroundColor Yellow
    for ($i = 0; $i -lt [Math]::Min(32, $minLength); $i++) {
        if ($file1Bytes[$i] -eq $file2Bytes[$i]) {
            Write-Host "   0x$($i.ToString('X2')): 0x$($file1Bytes[$i].ToString('X2'))" -ForegroundColor Green
            $staticBytes++
        }
    }
    
    Write-Host ""
    Write-Host "Variable bytes (different between files - first 10):" -ForegroundColor Yellow
    $variableCount = 0
    for ($i = 0; $i -lt $minLength -and $variableCount -lt 10; $i++) {
        if ($file1Bytes[$i] -ne $file2Bytes[$i]) {
            Write-Host "   0x$($i.ToString('X4')): 0x$($file1Bytes[$i].ToString('X2')) -> 0x$($file2Bytes[$i].ToString('X2'))" -ForegroundColor Red
            $differences++
            $variableCount++
        }
    }
    
    $staticPercentage = [math]::Round(($staticBytes / 32) * 100, 1)
    Write-Host ""
    Write-Host "Analysis summary:" -ForegroundColor Cyan
    Write-Host "   Static bytes in first 32: $staticBytes/32 ($staticPercentage %)" -ForegroundColor White
    Write-Host "   Total differences found: $differences" -ForegroundColor White
}

Write-Host ""
Write-Host "Basic analysis complete!" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "   1. Look for patterns in static bytes (file structure)" -ForegroundColor White
Write-Host "   2. Analyze variable bytes for game progress data" -ForegroundColor White
Write-Host "   3. Search for known strings (quest names, player names)" -ForegroundColor White
Write-Host "   4. Use this data to implement real parser logic" -ForegroundColor White
