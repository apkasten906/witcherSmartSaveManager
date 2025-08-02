# Database Disconnect Automation Script
# Automatically handles SQLite database disconnection for Git operations

param(
    [string]$DatabasePath = "database/witcher_save_manager.db",
    [switch]$ForceKill = $false
)

Write-Host "Disconnecting from database: $DatabasePath" -ForegroundColor Cyan

# Step 1: Kill running application processes that might hold database locks
Write-Host "Terminating application processes..." -ForegroundColor Yellow

$processNames = @("WitcherSmartSaveManager", "dotnet")
$killedCount = 0

foreach ($processName in $processNames) {
    try {
        $processes = Get-Process -Name $processName -ErrorAction SilentlyContinue
        if ($processes) {
            $processes | ForEach-Object {
                Write-Host "   Killing: $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Red
                Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
                $killedCount++
            }
        }
    }
    catch {
        # Process might not exist, continue
    }
}

if ($killedCount -eq 0) {
    Write-Host "   No application processes found running" -ForegroundColor Green
}
else {
    Write-Host "   Terminated $killedCount processes" -ForegroundColor Green
    Start-Sleep -Seconds 2  # Give processes time to cleanup
}

# Step 2: Check if database files are still locked
Write-Host "Checking database file locks..." -ForegroundColor Yellow

$dbFiles = @(
    $DatabasePath,
    "$DatabasePath-shm",
    "$DatabasePath-wal"
)

$lockedFiles = @()
foreach ($file in $dbFiles) {
    if (Test-Path $file) {
        try {
            # Try to open file exclusively to test if it's locked
            $stream = [System.IO.File]::Open($file, 'Open', 'ReadWrite', 'None')
            $stream.Close()
            Write-Host "   $file - Available" -ForegroundColor Green
        }
        catch {
            Write-Host "   $file - LOCKED" -ForegroundColor Red
            $lockedFiles += $file
        }
    }
}

# Step 3: If files are still locked and ForceKill is specified, try additional cleanup
if ($lockedFiles.Count -gt 0 -and $ForceKill) {
    Write-Host "Force cleanup mode - attempting file handle release..." -ForegroundColor Magenta
    
    # Use handle.exe if available (SysInternals tool)
    $handleExe = Get-Command "handle.exe" -ErrorAction SilentlyContinue
    if ($handleExe) {
        foreach ($file in $lockedFiles) {
            $fileName = Split-Path $file -Leaf
            Write-Host "   Searching for handles to: $fileName" -ForegroundColor Yellow
            & handle.exe -p -u $fileName 2>$null | ForEach-Object {
                if ($_ -match "(\d+):") {
                    $processId = $matches[1]
                    Write-Host "   Force killing PID: $processId" -ForegroundColor Red
                    Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
                }
            }
        }
        Start-Sleep -Seconds 2
    }
    else {
        Write-Host "   handle.exe not available - install SysInternals for advanced cleanup" -ForegroundColor Yellow
    }
}

# Step 4: Final status check
Write-Host "Final database status check..." -ForegroundColor Cyan

$stillLocked = @()
foreach ($file in $dbFiles) {
    if (Test-Path $file) {
        try {
            $stream = [System.IO.File]::Open($file, 'Open', 'ReadWrite', 'None')
            $stream.Close()
        }
        catch {
            $stillLocked += $file
        }
    }
}

if ($stillLocked.Count -eq 0) {
    Write-Host "Success: All database files are now available for Git operations!" -ForegroundColor Green
    
    # Optional: Reset Git index for database files
    Write-Host "Resetting Git index for database files..." -ForegroundColor Cyan
    git restore $DatabasePath 2>$null
    git restore "$DatabasePath-shm" 2>$null  
    git restore "$DatabasePath-wal" 2>$null
    
    Write-Host "COMPLETE: Database disconnected and Git-ready!" -ForegroundColor Green
    return $true
}
else {
    Write-Host "WARNING: Some files are still locked:" -ForegroundColor Red
    $stillLocked | ForEach-Object { Write-Host "   - $_" -ForegroundColor Red }
    Write-Host "Try: .\Disconnect-Database.ps1 -ForceKill" -ForegroundColor Yellow
    return $false
}
