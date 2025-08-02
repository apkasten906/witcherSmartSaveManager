# WitcherCI - Witcher Smart Save Manager Task Runner

A custom task runner inspired by DandelionCI, specifically designed for Witcher save file analysis and game research operations.

## 🎯 Purpose

WitcherCI provides a secure, file-based task execution system for:
- **DZIP save file analysis** - Header structure analysis and decompression
- **Hex pattern analysis** - Binary save file pattern detection  
- **Build automation** - Specialized build tasks for analysis tools
- **Research workflows** - Coordinated analysis pipelines

## 🚀 Quick Start

### 1. Start the Task Runner
```powershell
# Run in watch mode (recommended)
.\.vscode\tools\WitcherCI.ps1 -Watch

# Or run single task
.\.vscode\tools\WitcherCI.ps1 -TaskFile "my-task.json"
```

### 2. Submit Tasks via JSON Files

Drop JSON task files in `.vscode\tools\tasks\` and WitcherCI will automatically process them.

#### Example: Analyze DZIP Headers
```json
{
  "command": "dzip-analyze",
  "save-path": "savesAnalysis/_backup/AutoSave_0039.sav",
  "output-format": "console"
}
```

#### Example: Decompress and Analyze Save Content
```json
{
  "command": "dzip-decompress", 
  "count": 5
}
```

#### Example: Build All Tools
```json
{
  "command": "build-tools",
  "configuration": "Release"
}
```

## 📋 Available Commands

| Command | Description | Parameters |
|---------|-------------|------------|
| `dzip-analyze` | Analyze DZIP header structure | `save-path`, `output-format` |
| `dzip-decompress` | Decompress save files and show content | `save-path`, `count` |
| `hex-analyze` | Perform hex pattern analysis | `save-path`, `pattern` |
| `build-tools` | Build all analysis tools | `configuration` |

## 🔧 Command Details

### dzip-analyze
- **Purpose**: Analyze DZIP header structure and compression info
- **Parameters**: 
  - `save-path` (optional): Specific .sav file or directory
  - `output-format` (optional): `console`, `json`, `csv` (default: console)
- **Output**: Header analysis with magic bytes, compression ratios, data preview

### dzip-decompress  
- **Purpose**: Test decompression strategies and analyze uncompressed content
- **Parameters**:
  - `save-path` (optional): Specific .sav file or directory  
  - `count` (optional): Number of files to process (default: 3)
- **Output**: Decompression results with binary data preview

### build-tools
- **Purpose**: Build all Witcher analysis tools with proper dependencies
- **Parameters**:
  - `configuration` (optional): `Debug` or `Release` (default: Debug)
- **Output**: Build status for all projects

## 🛡️ Security Features

Following DandelionCI patterns:
- ✅ **Command Whitelisting** - Only approved commands in `witcher_commands.json`
- ✅ **Parameter Validation** - Type checking, file extension validation
- ✅ **Path Sanitization** - Prevents directory traversal attacks
- ✅ **Error Isolation** - Failed tasks don't crash the runner
- ✅ **Audit Logging** - Full trace of all operations

## 📁 Directory Structure

```
.vscode/tools/
├── WitcherCI.ps1              # Main task runner
├── witcher_commands.json      # Command definitions & validation
├── tasks/                     # Task queue (JSON files)
├── scripts/                   # PowerShell execution scripts
│   ├── Invoke-DZipAnalysis.ps1
│   ├── Invoke-DZipDecompression.ps1
│   └── Build-WitcherTools.ps1
└── README.md                  # This file
```

## 🔄 Workflow Integration

### AI Agent Integration
AI agents can drop task files and monitor results:

1. **Submit Task**: Create JSON file in `tasks/` directory
2. **Monitor Progress**: Watch for completion (file deletion) or error files
3. **Collect Results**: Read console output or generated analysis files

### Manual Usage
For manual research workflows:

```powershell
# Start background task runner
Start-Process powershell -ArgumentList ".\.vscode\tools\WitcherCI.ps1 -Watch"

# Submit tasks by creating JSON files
@{command="dzip-analyze"; "save-path"="specific_file.sav"} | ConvertTo-Json | Out-File .vscode\tools\tasks\analyze-task.json
```

## 🧪 Example Research Workflow

1. **Build Tools**: `{"command": "build-tools"}`
2. **Analyze Headers**: `{"command": "dzip-analyze", "output-format": "json"}`
3. **Test Decompression**: `{"command": "dzip-decompress", "count": 10}`
4. **Pattern Analysis**: `{"command": "hex-analyze", "pattern": "quest-data"}`

## 🔍 Debugging

- **Logs**: Console output shows detailed operation logs
- **Error Files**: Failed tasks create `.error` files with diagnostic info
- **Validation**: Parameter validation prevents common issues
- **Build Check**: Tools are built automatically before execution

This system provides the flexibility of DandelionCI with specialized focus on Witcher save file research and analysis workflows.
