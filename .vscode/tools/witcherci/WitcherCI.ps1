# WitcherCI Task Runner
# Custom task runner for Witcher Smart Save Manager tools
# Follows DandelionCI pattern but specialized for game save analysis

param(
    [string]$TaskFile = "task.json",
    [switch]$Watch = $false,
    [switch]$Help = $false,
    [int]$WatchInterval = 5
)

$ErrorActionPreference = "Stop"
$CommandsFile = Join-Path $PSScriptRoot "witcher_commands.json"
$TasksDir = Join-Path $PSScriptRoot "tasks"

# Ensure tasks directory exists
if (-not (Test-Path $TasksDir)) {
    New-Item -ItemType Directory -Path $TasksDir -Force | Out-Null
}

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] [$Level] $Message"
}

function Show-Help {
    Write-Host @"
WitcherCI - Witcher Smart Save Manager Task Runner
==================================================

A secure task runner for Witcher save file analysis operations.

Usage:
  .\WitcherCI.ps1 -Watch                    # Start in watch mode
  .\WitcherCI.ps1 -TaskFile "task.json"     # Run single task
  .\WitcherCI.ps1 -Help                     # Show this help

Available Commands:
  dzip-analyze       - Analyze DZIP header structure
  dzip-decompress    - Decompress and analyze save content  
  build-tools        - Build all analysis tools
  hex-analyze        - Perform hex pattern analysis

Example Tasks:
  {"command": "dzip-decompress", "count": 5}
  {"command": "dzip-analyze", "output-format": "json"}
  {"command": "build-tools", "configuration": "Release"}

Task files are monitored in: $TasksDir
Commands defined in: $CommandsFile
"@
}

function Get-Commands {
    if (-not (Test-Path $CommandsFile)) {
        throw "Commands file not found: $CommandsFile"
    }
    
    $commands = Get-Content $CommandsFile | ConvertFrom-Json
    Write-Log "Loaded $($commands.PSObject.Properties.Count) commands"
    return $commands
}

function Test-Parameters {
    param($Task, $CommandDef)
    
    foreach ($param in $CommandDef.parameters) {
        $validation = $CommandDef.validation.$param
        $value = $Task.$param
        
        if ($validation.required -and (-not $value)) {
            throw "Required parameter missing: $param"
        }
        
        if ($value -and $validation.type -eq "file-path") {
            if ($validation.extensions -and $value) {
                $ext = [System.IO.Path]::GetExtension($value)
                if ($ext -notin $validation.extensions) {
                    throw "Invalid file extension for ${param}: $ext"
                }
            }
        }
        
        if ($value -and $validation.type -eq "enum") {
            if ($value -notin $validation.values) {
                throw "Invalid value for ${param}: $value. Allowed: $($validation.values -join ', ')"
            }
        }
    }
}

function Invoke-Task {
    param($TaskPath)
    
    try {
        Write-Log "Processing task: $TaskPath"
        $task = Get-Content $TaskPath | ConvertFrom-Json
        $commands = Get-Commands
        
        $commandName = $task.command
        if (-not $commands.$commandName) {
            throw "Unknown command: $commandName"
        }
        
        $commandDef = $commands.$commandName
        Write-Log "Executing command: $($commandDef.description)"
        
        # Validate parameters
        Test-Parameters -Task $task -CommandDef $commandDef
        
        # Build command
        if ($commandDef.command -eq "powershell") {
            $scriptPath = Join-Path $PSScriptRoot $commandDef.script
            if (-not (Test-Path $scriptPath)) {
                throw "Script not found: $scriptPath"
            }
            
            $scriptArgs = @()
            foreach ($param in $commandDef.parameters) {
                $value = $task.$param
                if (-not $value) {
                    $value = $commandDef.validation.$param.default
                }
                if ($value) {
                    $scriptArgs += "-$param"
                    $scriptArgs += $value
                }
            }
            
            Write-Log "Running: powershell -File $scriptPath $($scriptArgs -join ' ')"
            & powershell -File $scriptPath @scriptArgs
        }
        else {
            throw "Unsupported command type: $($commandDef.command)"
        }
        
        Write-Log "Task completed successfully"
        Remove-Item $TaskPath -Force
        
    }
    catch {
        Write-Log "Task failed: $($_.Exception.Message)" -Level "ERROR"
        $errorFile = $TaskPath -replace "\.json$", ".error"
        $_.Exception.Message | Out-File $errorFile
    }
}

function Start-WatchMode {
    Write-Log "Starting watch mode (interval: ${WatchInterval}s)"
    Write-Log "Watching directory: $TasksDir"
    
    while ($true) {
        $taskFiles = Get-ChildItem -Path $TasksDir -Filter "*.json" -File
        
        foreach ($taskFile in $taskFiles) {
            Invoke-Task -TaskPath $taskFile.FullName
        }
        
        Start-Sleep -Seconds $WatchInterval
    }
}

# Main execution
try {
    if ($Help) {
        Show-Help
        exit 0
    }
    
    Write-Log "WitcherCI Task Runner Starting"
    
    if ($Watch) {
        Start-WatchMode
    }
    else {
        $taskPath = Join-Path $TasksDir $TaskFile
        if (Test-Path $taskPath) {
            Invoke-Task -TaskPath $taskPath
        }
        else {
            Write-Log "Task file not found: $taskPath" -Level "ERROR"
            exit 1
        }
    }
}
catch {
    Write-Log "Fatal error: $($_.Exception.Message)" -Level "ERROR"
    exit 1
}
