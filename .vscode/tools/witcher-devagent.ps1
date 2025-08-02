# WitcherDevAgent - DZIP Analysis Task Runner
# Adapted from DandelionCI pattern for Witcher Smart Save Manager
# Provides secure file-based task execution for DZIP decompression analysis

param(
    [Parameter(Mandatory = $false)]
    [string]$TaskFile = "",
    [Parameter(Mandatory = $false)]
    [switch]$Watch = $false,
    [Parameter(Mandatory = $false)]
    [switch]$Help = $false
)

# Configuration
$ALLOWED_COMMANDS_FILE = "allowed_commands.json"
$POLLING_INTERVAL = 5
$LOG_FILE = "witcher-devagent.log"

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    Add-Content -Path $LOG_FILE -Value $logEntry
    Write-Host $logEntry
}

function Show-Help {
    Write-Host "WitcherDevAgent - DZIP Analysis Task Runner" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Yellow
    Write-Host "  .\witcher-devagent.ps1 -TaskFile task.json    # Process single task"
    Write-Host "  .\witcher-devagent.ps1 -Watch                # Watch for task files"
    Write-Host "  .\witcher-devagent.ps1 -Help                 # Show this help"
    Write-Host ""
    Write-Host "Available Tasks:" -ForegroundColor Yellow
    
    if (Test-Path $ALLOWED_COMMANDS_FILE) {
        $commands = Get-Content $ALLOWED_COMMANDS_FILE | ConvertFrom-Json
        $commands.PSObject.Properties | ForEach-Object {
            Write-Host "  $($_.Name) - $($_.Value.description)" -ForegroundColor Green
        }
    }
    
    Write-Host ""
    Write-Host "Example Task Files:" -ForegroundColor Yellow
    Write-Host '  {"task": "dzip-analyze"}' -ForegroundColor Gray
    Write-Host '  {"task": "dzip-decompress", "save-path": "savesAnalysis/_backup/AutoSave_0039.sav"}' -ForegroundColor Gray
    Write-Host '  {"task": "build", "project": "witcherSmartSaveManager.sln"}' -ForegroundColor Gray
    Write-Host '  {"task": "list-projects"}' -ForegroundColor Gray
}

function Load-AllowedCommands {
    if (-not (Test-Path $ALLOWED_COMMANDS_FILE)) {
        Write-Log "SECURITY: allowed_commands.json not found" "ERROR"
        return $null
    }
    
    try {
        return Get-Content $ALLOWED_COMMANDS_FILE | ConvertFrom-Json
    }
    catch {
        Write-Log "SECURITY: Failed to parse allowed_commands.json - $($_.Exception.Message)" "ERROR"
        return $null
    }
}

function Validate-TaskName {
    param([string]$TaskName)
    
    # Only allow alphanumeric, dash, underscore
    if ($TaskName -notmatch '^[a-zA-Z0-9_-]+$') {
        Write-Log "SECURITY: Invalid task name '$TaskName' - contains illegal characters" "ERROR"
        return $false
    }
    
    return $true
}

function Validate-FilePath {
    param([string]$FilePath, [string[]]$AllowedExtensions)
    
    # Block absolute paths and path traversal
    if ([System.IO.Path]::IsPathRooted($FilePath)) {
        Write-Log "SECURITY: Absolute path blocked - '$FilePath'" "ERROR"
        return $false
    }
    
    if ($FilePath -match '\.\.[/\\]') {
        Write-Log "SECURITY: Path traversal blocked - '$FilePath'" "ERROR"
        return $false
    }
    
    # Check extension if specified
    if ($AllowedExtensions -and $AllowedExtensions.Count -gt 0) {
        $extension = [System.IO.Path]::GetExtension($FilePath)
        if ($extension -notin $AllowedExtensions) {
            Write-Log "SECURITY: Invalid file extension '$extension' for '$FilePath'" "ERROR"
            return $false
        }
    }
    
    return $true
}

function Process-Task {
    param([PSObject]$Task, [string]$TaskId)
    
    Write-Log "Processing task '$($Task.task)' with ID '$TaskId'"
    
    # Load allowed commands
    $allowedCommands = Load-AllowedCommands
    if (-not $allowedCommands) {
        return @{ success = $false; error = "Failed to load allowed commands" }
    }
    
    # Validate task name
    if (-not (Validate-TaskName $Task.task)) {
        return @{ success = $false; error = "Invalid task name" }
    }
    
    # Check if task is allowed
    if (-not $allowedCommands.PSObject.Properties.Name.Contains($Task.task)) {
        Write-Log "SECURITY: Unauthorized task '$($Task.task)'" "ERROR"
        return @{ success = $false; error = "Task not allowed" }
    }
    
    $commandConfig = $allowedCommands.($Task.task)
    $command = $commandConfig.command
    
    # Validate and substitute parameters
    foreach ($param in $commandConfig.parameters) {
        if ($Task.PSObject.Properties.Name.Contains($param)) {
            $value = $Task.$param
            
            # Validate parameter based on configuration
            if ($commandConfig.validation.PSObject.Properties.Name.Contains($param)) {
                $validation = $commandConfig.validation.$param
                
                if ($validation.type -eq "file-path") {
                    if (-not (Validate-FilePath $value $validation.extensions)) {
                        return @{ success = $false; error = "Invalid file path parameter '$param'" }
                    }
                    
                    # Check if file exists (if required)
                    if ($validation.required -and -not (Test-Path $value)) {
                        return @{ success = $false; error = "Required file not found: '$value'" }
                    }
                }
            }
            
            # Safe parameter substitution
            $escapedValue = $value -replace '"', '\"'
            $command = $command -replace "--", "-- `"$escapedValue`""
        }
    }
    
    Write-Log "Executing: $command"
    
    try {
        $output = Invoke-Expression $command 2>&1
        $exitCode = $LASTEXITCODE
        
        $result = @{
            success   = ($exitCode -eq 0)
            exitCode  = $exitCode
            output    = $output -join "`n"
            task      = $Task.task
            timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        }
        
        if ($result.success) {
            Write-Log "Task '$($Task.task)' completed successfully"
        }
        else {
            Write-Log "Task '$($Task.task)' failed with exit code $exitCode" "ERROR"
        }
        
        return $result
        
    }
    catch {
        Write-Log "Task '$($Task.task)' threw exception: $($_.Exception.Message)" "ERROR"
        return @{
            success   = $false
            error     = $_.Exception.Message
            task      = $Task.task
            timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        }
    }
}

function Process-TaskFile {
    param([string]$TaskFilePath)
    
    $taskId = [System.IO.Path]::GetFileNameWithoutExtension($TaskFilePath) -replace '^task-', ''
    $resultFilePath = "result-task-$taskId.json"
    
    try {
        $taskContent = Get-Content $TaskFilePath -Raw | ConvertFrom-Json
        $result = Process-Task $taskContent $taskId
        
        # Write result file
        $result | ConvertTo-Json -Depth 3 | Out-File -FilePath $resultFilePath -Encoding UTF8
        
        # Clean up task file
        Remove-Item $TaskFilePath -Force
        
        Write-Log "Result written to '$resultFilePath'"
        
    }
    catch {
        Write-Log "Failed to process task file '$TaskFilePath': $($_.Exception.Message)" "ERROR"
        
        # Write error result
        @{
            success   = $false
            error     = $_.Exception.Message
            timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        } | ConvertTo-Json | Out-File -FilePath $resultFilePath -Encoding UTF8
    }
}

# Main execution
if ($Help) {
    Show-Help
    exit 0
}

if (-not (Test-Path $ALLOWED_COMMANDS_FILE)) {
    Write-Host "Error: $ALLOWED_COMMANDS_FILE not found in current directory" -ForegroundColor Red
    Write-Host "Please run from the .vscode/tools directory" -ForegroundColor Yellow
    exit 1
}

Write-Log "WitcherDevAgent starting..."

if ($TaskFile) {
    # Process single task file
    if (-not (Test-Path $TaskFile)) {
        Write-Host "Error: Task file '$TaskFile' not found" -ForegroundColor Red
        exit 1
    }
    
    Process-TaskFile $TaskFile
    
}
elseif ($Watch) {
    # Watch mode - continuously monitor for task files
    Write-Log "Starting watch mode - monitoring for task-*.json files every $POLLING_INTERVAL seconds"
    Write-Host "WitcherDevAgent watching for tasks... (Press Ctrl+C to stop)" -ForegroundColor Green
    
    while ($true) {
        $taskFiles = Get-ChildItem -Filter "task-*.json" | Sort-Object CreationTime
        
        foreach ($taskFile in $taskFiles) {
            Process-TaskFile $taskFile.FullName
        }
        
        Start-Sleep -Seconds $POLLING_INTERVAL
    }
    
}
else {
    Show-Help
}
