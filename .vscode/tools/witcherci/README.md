# WitcherCI - Witcher Smart Save Manager Task Runner

A custom task runner inspired by DandelionCI, specifically designed for Witcher save file analysis and game research operations.

## 🎯 Purpose

WitcherCI provides a secure, file-based task execution system for:
- **DZIP save file analysis** - Header structure analysis and decompression
- **Hex pattern analysis** - Binary save file pattern detection  
- **Build automation** - Specialized build tasks for analysis tools
- **Research workflows** - Coordinated analysis pipelines

## 🚀 Quick Start

### 1. Start from Tools Directory
```batch
# From .vscode\tools\ directory
.\witcher-devagent.bat          # Start in watch mode
.\witcher-devagent.bat help     # Show help
```

### 2. Or Run Directly
```powershell
# From .vscode\tools\witcherci\ directory
.\WitcherCI.ps1 -Watch                    # Start in watch mode
.\WitcherCI.ps1 -TaskFile "task.json"     # Run single task
.\WitcherCI.ps1 -Help                     # Show help
```

### 3. Submit Tasks via JSON Files

Drop JSON task files in `tasks/` and WitcherCI will automatically process them.

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

## 📁 Directory Structure

```
.vscode/tools/
├── witcher-devagent.bat       # Entry point batch file
├── witcherci/                 # WitcherCI tool directory
│   ├── WitcherCI.ps1          # Main task runner
│   ├── witcher_commands.json  # Command definitions & validation
│   ├── README.md              # This file
│   ├── tasks/                 # Task queue (JSON files)
│   │   └── test-decompression.json
│   └── scripts/               # PowerShell execution scripts
│       ├── Invoke-DZipAnalysis.ps1
│       ├── Invoke-DZipDecompression.ps1
│       ├── Invoke-HexAnalysis.ps1
│       └── Build-WitcherTools.ps1
└── (legacy files...)
```

## 🛡️ Security Features

Following DandelionCI patterns:
- ✅ **Command Whitelisting** - Only approved commands in `witcher_commands.json`
- ✅ **Parameter Validation** - Type checking, file extension validation
- ✅ **Path Sanitization** - Prevents directory traversal attacks
- ✅ **Error Isolation** - Failed tasks don't crash the runner
- ✅ **Audit Logging** - Full trace of all operations

## 🔄 Workflow Integration

### AI Agent Integration
AI agents can drop task files and monitor results:

1. **Submit Task**: Create JSON file in `witcherci/tasks/` directory
2. **Monitor Progress**: Watch for completion (file deletion) or error files
3. **Collect Results**: Read console output or generated analysis files

### Manual Usage
For manual research workflows:

```batch
# Start background task runner
.\witcher-devagent.bat

# Submit tasks by creating JSON files in witcherci\tasks\
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

## 📊 Current Status (August 2025)

### Infrastructure Status: ✅ COMPLETE
- **WitcherCI Task Runner**: Fully operational
- **Command Validation**: Working with security features
- **Build Integration**: 1.3s compile time, all projects successful
- **Automation**: No pause prompts, CI/CD ready
- **Test Data**: 270+ Witcher 2 save files available

### Key Fixes Applied
- ✅ Removed pause commands from batch files
- ✅ Fixed PowerShell parameter naming (kebab-case support)
- ✅ Validated all command definitions and scripts
- ✅ Tested with real Witcher 2 save file data
- ✅ Error isolation and comprehensive logging

### Ready for Phase 1.1
**DZIP Decompression Testing** - Infrastructure complete, ready for save file analysis

---

**Last Updated**: August 2, 2025  
**Phase**: 1.1 - DZIP Decompression Testing
