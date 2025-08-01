# Simple Witcher 2 Save File Analysis
Write-Host "Witcher 2 Save File Format Analyzer" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green

$backupDir = ".\savesAnalysis\_backup"
if (-not (Test-Path $backupDir)) {
    Write-Host "ERROR: Backup directory not found" -ForegroundColor Red
    exit 1
}

# Get save files
$saveFiles = Get-ChildItem -Path $backupDir -Filter "*.sav" | Sort-Object LastWriteTime | Select-Object -First 3
Write-Host "Found" $saveFiles.Count "save files for analysis" -ForegroundColor Cyan

# Show file info
foreach ($file in $saveFiles) {
    $size = [math]::Round($file.Length / 1KB, 2)
    Write-Host "  " $file.Name "(" $size "KB )" -ForegroundColor White
}

# Analyze first file
Write-Host ""
Write-Host "Analyzing first file:" $saveFiles[0].Name -ForegroundColor Yellow
$bytes = [System.IO.File]::ReadAllBytes($saveFiles[0].FullName)
Write-Host "File size:" $bytes.Length "bytes" -ForegroundColor White

# Show first 32 bytes as hex
Write-Host ""
Write-Host "First 32 bytes (hex):" -ForegroundColor Yellow
$first32 = $bytes[0..31]
for ($i = 0; $i -lt 32; $i += 16) {
    $hexLine = ""
    $asciiLine = ""
    for ($j = 0; $j -lt 16 -and ($i + $j) -lt 32; $j++) {
        $byte = $first32[$i + $j]
        $hexLine += $byte.ToString("X2") + " "
        if ($byte -ge 32 -and $byte -le 126) {
            $asciiLine += [char]$byte
        }
        else {
            $asciiLine += "."
        }
    }
    $offset = ($i).ToString("X4")
    Write-Host "  $offset  $hexLine  $asciiLine" -ForegroundColor White
}

# Look for readable strings in first 512 bytes
Write-Host ""
Write-Host "Readable strings (first 512 bytes):" -ForegroundColor Yellow
$stringBuilder = ""
$maxBytes = [Math]::Min(512, $bytes.Length)
for ($i = 0; $i -lt $maxBytes; $i++) {
    $byte = $bytes[$i]
    if ($byte -ge 32 -and $byte -le 126) {
        $stringBuilder += [char]$byte
    }
    else {
        if ($stringBuilder.Length -ge 4) {
            $offset = ($i - $stringBuilder.Length).ToString("X4")
            Write-Host "  0x$offset : '$stringBuilder'" -ForegroundColor Green
        }
        $stringBuilder = ""
    }
}

# Look for timestamps
Write-Host ""
Write-Host "Potential timestamps:" -ForegroundColor Yellow
for ($i = 0; $i -le $bytes.Length - 4; $i += 4) {
    $timestamp = [BitConverter]::ToUInt32($bytes, $i)
    # Check reasonable range (2015-2030)
    if ($timestamp -ge 1420070400 -and $timestamp -le 1893456000) {
        $date = [DateTimeOffset]::FromUnixTimeSeconds($timestamp).DateTime
        $offset = $i.ToString("X4")
        Write-Host "  0x$offset : $timestamp (" $date.ToString("yyyy-MM-dd HH:mm:ss") ")" -ForegroundColor Cyan
    }
}

# Compare with second file if available
if ($saveFiles.Count -ge 2) {
    Write-Host ""
    Write-Host "Comparing with second file..." -ForegroundColor Yellow
    $bytes2 = [System.IO.File]::ReadAllBytes($saveFiles[1].FullName)
    
    $minLen = [Math]::Min($bytes.Length, $bytes2.Length)
    $same = 0
    $diff = 0
    
    # Check first 64 bytes
    for ($i = 0; $i -lt [Math]::Min(64, $minLen); $i++) {
        if ($bytes[$i] -eq $bytes2[$i]) {
            $same++
        }
        else {
            $diff++
            if ($diff -le 5) {
                # Show first 5 differences
                $offset = $i.ToString("X2")
                Write-Host "  0x$offset : " $bytes[$i].ToString("X2") " -> " $bytes2[$i].ToString("X2") -ForegroundColor Red
            }
        }
    }
    
    $percent = [math]::Round(($same / 64) * 100, 1)
    Write-Host "First 64 bytes: $same same, $diff different ($percent% identical)" -ForegroundColor White
}

Write-Host ""
Write-Host "Analysis complete!" -ForegroundColor Green
Write-Host "Use this data to understand the save file structure." -ForegroundColor Yellow
