@echo off
setlocal enabledelayedexpansion

echo WitcherDevAgent - DZIP Analysis Task Runner
echo ==========================================

cd /d "%~dp0\witcherci"

if "%1"=="" (
    echo Starting watch mode...
    powershell.exe -ExecutionPolicy Bypass -File "WitcherCI.ps1" -Watch
) else if "%1"=="help" (
    powershell.exe -ExecutionPolicy Bypass -File "WitcherCI.ps1" -Help
) else (
    echo Processing task file: %1
    powershell.exe -ExecutionPolicy Bypass -File "WitcherCI.ps1" -TaskFile "%1"
)
