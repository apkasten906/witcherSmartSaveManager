# Witcher Smart Save Manager - AI Coding Instructions

## 🎯 Project Overview
This is a **WPF MVVM application** for managing Witcher game save files with features like backup management, orphaned screenshot cleanup, and multi-language support. Built on **.NET 8.0** with strict architectural principles.

## 🖥️ Development Environment
- **Primary OS**: Windows 10/11
- **Shell**: PowerShell 5.1+ (default)
- **Command Syntax**: Use PowerShell syntax with `;` for command chaining
- **Paths**: Use Windows-style paths (backslashes) in examples
- **Scripts**: All automation uses `.ps1` PowerShell scripts
- **IDE**: Visual Studio Code / Visual Studio
- **Runtime**: .NET 8.0 Windows Desktop

## 📚 ERROR LEARNING PROTOCOL - HIGHEST PRIORITY

### Core Learning Principle
**EVERY ERROR is a learning opportunity that MUST be documented to prevent repetition.**

### Learning Process - MANDATORY
1. **Capture the Error**: Record exact error message, context, and conditions
2. **Root Cause Analysis**: Identify the fundamental cause, not just symptoms
3. **Document Solution**: Record the exact fix that resolved the issue
4. **Create Prevention Rule**: Add guideline to prevent this error class
5. **Update Instructions**: Add to this file for future reference

### Learning Categories
- **Syntax/Language Issues**: PowerShell, C#, XAML parsing errors
- **Architectural Violations**: MVVM, file organization, separation concerns
- **Configuration Problems**: Build, dependency, environment setup
- **Logic Errors**: Business logic, data flow, state management
- **Integration Issues**: File paths, external tools, cross-component communication
- **Project Management Issues**: Missing cleanup, documentation, incomplete task closure

### Documentation Standard
```
**Error**: [Exact error message or symptom]
- **Cause**: [Root cause analysis]
- **Solution**: [Specific fix applied]
- **Prevention**: [Rule/guideline to avoid repetition]
```

This learning protocol applies to ALL development work, not just scripts.

## 🏗️ Architecture & Project Structure

### Core Projects
- **`frontend/`** - Main WPF application (WitcherSmartSaveManager.csproj)
- **`WitcherCore/`** - Core business logic library with Models, Services, Data access
- **`Shared/`** - Common types and utilities
- **`WitcherSmartSaveManagerTests/`** - NUnit test project

### File Organization Rules
- **One class per file** - Never put multiple public classes in the same file
- **Models in Models/** - All data models belong in `WitcherCore/Models/` or `Shared/Models/`
- **Services focused** - Service classes should be single-purpose and delegate to models
- **No nested public classes** - Extract nested classes to separate files with proper naming
- **Consistent naming** - `SaveFileData.cs`, `QuestState.cs`, `ParseResult.cs` etc.

### Critical Dependencies
```xml
<!-- Core packages across projects -->
<PackageReference Include="System.Data.SQLite" Version="1.0.118" />
<PackageReference Include="NLog" Version="6.0.2" />
<PackageReference Include="Microsoft.Extensions.Configuration.*" />
```

## 🎨 MVVM Patterns

### Strict MVVM Rules
- **NO business logic in code-behind** - ViewModels handle all logic
- **NO direct file operations in ViewModels** - delegate to services
- **Commands over event handlers** - use `RelayCommand` pattern
- **Binding-only UI updates** - `INotifyPropertyChanged` everywhere

### ViewModel Example Pattern
```csharp
public class MainViewModel : INotifyPropertyChanged
{
    private readonly WitcherSaveFileService _saveFileService;
    
    // Localized display properties
    public string TotalSaveFilesDisplay => 
        Utils.ResourceHelper.GetFormattedString("TotalSaveFiles", TotalSaveFiles);
    
    // Commands with enable conditions
    public ICommand DeleteSelectedCommand { get; }
    
    public MainViewModel()
    {
        DeleteSelectedCommand = new RelayCommand(
            _ => DeleteSelectedSaves(), 
            _ => Saves.Any(s => s.IsSelected)
        );
    }
}
```

## 🌍 Localization System

### Critical Patterns
- **ALL user-facing strings** must use `ResourceHelper.GetString()` or `ResourceHelper.GetFormattedString()`
- **No hardcoded strings** in XAML or ViewModels
- Support **English/German** with `.resx` files
- **Witchery-themed messaging** for enhanced UX (e.g., "Kikimora alerts")

```csharp
// Correct localization usage
StatusMessage = Utils.ResourceHelper.GetFormattedString("Status_SavesLoaded", saveCount);

// Language switching updates all bound properties
private void SetLanguage(string lang)
{
    var culture = new CultureInfo(lang);
    Thread.CurrentThread.CurrentUICulture = culture;
    Utils.ResourceDictionaryHelper.UpdateResourcesForCulture(culture);
    OnPropertyChanged(nameof(StatusMessage)); // Refresh bindings
}
```

## 📁 File Operations & Services

### Service Layer Pattern
```csharp
public class WitcherSaveFileService
{
    // Always use game-specific extensions
    string extensionPattern = GameSaveExtensions.GetExtensionForGame(gameKey);
    
    // Handle screenshot relationships
    var screenshotName = saveName + "_640x360.bmp";
    
    // Robust error handling with user feedback
    public bool DeleteSaveFile(WitcherSaveFile save)
    {
        try 
        {
            File.Delete(save.FullPath);
            // Handle related screenshot cleanup
        }
        catch (IOException ex) when (ex.Message.Contains("being used by another process"))
        {
            Logger.Warn($"File locked: {save.FullPath}");
            return false;
        }
    }
}
```

### Orphaned File Detection
- **Automatic detection** after save operations
- **User choice** for cleanup with clear explanations  
- **Graceful locked file handling** with detailed reporting

## 🧪 Testing Requirements

### NUnit Test Patterns
```csharp
[TestFixture]
public class WitcherSaveFileServiceTests
{
    [SetUp]
    public void SetUp()
    {
        _tempSaveDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _service = new WitcherSaveFileService(GameKey.Witcher2, _tempSaveDir, _tempBackupDir);
    }
    
    [Test]
    public void DeleteSaveFile_RemovesSaveAndScreenshot_ReturnsTrue()
    {
        // Test file operations with temp directories
        // Mock external dependencies
        // Verify both save and screenshot handling
    }
}
```

## 🔧 Configuration Management

### Config Hierarchy
1. **`App.config`** - Default game paths and extensions
2. **`userpaths.json`** - User customizations (optional)
3. **`SavePathResolver`** - Centralized path resolution

```csharp
// Configuration pattern
public static string GetSavePath(GameKey gameKey)
{
    // 1. Check user overrides in userpaths.json
    var userPath = _userConfig?[$"SavePaths:{gameKey}"];
    if (!string.IsNullOrWhiteSpace(userPath))
        return Environment.ExpandEnvironmentVariables(userPath);
    
    // 2. Fallback to App.config defaults
    var defaultPath = ConfigurationManager.AppSettings[$"{gameKey}DefaultSaveFolder"];
    return Environment.ExpandEnvironmentVariables(defaultPath);
}
```

## 🛠️ WitcherCI Tool Infrastructure

### Organization Structure
```
.vscode/tools/
├── witcher-devagent.bat              # Entry point batch file
└── witcherci/                        # WitcherCI framework directory
    ├── WitcherCI.ps1                 # Main task runner
    ├── witcher_commands.json         # Command definitions
    ├── scripts/                      # PowerShell execution scripts
    │   ├── Invoke-DZipAnalysis.ps1
    │   ├── Invoke-DZipDecompression.ps1
    │   ├── Invoke-HexAnalysis.ps1
    │   └── Build-WitcherTools.ps1
    └── tasks/                        # JSON task definitions
        ├── test-decompression.json
        ├── build-test.json
        └── final-test.json
```

### WitcherCI Design Patterns
- **JSON Task Definitions**: All tasks defined in declarative JSON format
- **Command Whitelisting**: Security through allowed command validation
- **Parameter Validation**: Type-safe parameter handling with validation
- **Watch Mode**: Continuous monitoring for automated task execution
- **Direct Assembly Integration**: PowerShell scripts load WitcherCore.dll directly
- **Error Isolation**: Failed tasks don't crash the entire runner

### Usage Examples
```powershell
# Start watch mode for continuous task monitoring
.\.vscode\tools\witcher-devagent.bat

# Run single task file
.\.vscode\tools\witcher-devagent.bat "test-decompression.json"

# Show help and available commands
.\.vscode\tools\witcher-devagent.bat "help"
```

### Task File Format
```json
{
    "command": "dzip-analyze",
    "save-path": "savesAnalysis/_backup/AutoSave_0039.sav",
    "output-format": "detailed",
    "pattern": "quest-data"
}
```

## 🚀 Build & Development

### Essential Commands (PowerShell)
```powershell
# Build and run
cd frontend ; dotnet build ; dotnet run

# Run tests
dotnet test WitcherSmartSaveManagerTests/

# Build installer (requires Inno Setup)
.\installer\Build-Installer.ps1

# Clean solution
dotnet clean witcherSmartSaveManager.sln

# Restore packages
dotnet restore witcherSmartSaveManager.sln
```

### Git Workflow
- **Feature branches**: `feat/{issue-number}-{description}`
- **Conventional commits** for automatic versioning:
  - `feat:` → minor bump, `fix:` → patch, `feat!:` → major
- **Strict workflow**: Issue → Feature Branch → Dev → Main
- **Auto-releases** on main branch with generated release notes

## ⚠️ Common Pitfalls

1. **Never bypass MVVM** - no business logic in code-behind
2. **Always use ResourceHelper** for user-facing strings
3. **Use GameSaveExtensions.GetExtensionForGame()** not hardcoded patterns
4. **Handle locked files gracefully** during gameplay
5. **Update UI counters immediately** after file operations
6. **Test with temp directories** - never use real save folders in tests
7. **APPLY ERROR LEARNING PROTOCOL** - document every error for prevention
8. **Verify file existence** - use Test-Path before assuming path resolution issues
9. **Clean up temporary projects** - remove console apps and prototyping code after integration
10. **Organize tooling properly** - follow WitcherCI structure for all development tools

## 📝 COMPREHENSIVE ERROR LEARNING LOG

### PowerShell Script Failures
**Error**: "Die Zeichenfolge hat kein Abschlusszeichen" (String not terminated)
- **Cause**: Missing closing braces in foreach loops or if statements
- **Solution**: Always verify brace matching, use proper indentation
- **Prevention**: Count opening/closing braces before running scripts

**Error**: Unicode symbols causing parse failures in PowerShell 5.1
- **Cause**: Using ✓, ❌, 🎯 symbols in PowerShell scripts
- **Solution**: Replace with plain ASCII: "Success:", "Error:", "Warning:"
- **Prevention**: Stick to basic ASCII characters in all .ps1 files

### Architectural Violations
**Error**: Nested public classes in service files
- **Cause**: Putting multiple classes in SaveFileHexAnalyzer.cs
- **Solution**: Extract to separate Model files (HexAnalysisResult.cs, DZipInfo.cs)
- **Prevention**: Follow "one class per file" rule strictly

**Error**: Business logic in ViewModels instead of Services
- **Cause**: Direct file operations or complex logic in ViewModel
- **Solution**: Move to dedicated Service classes, inject via constructor
- **Prevention**: ViewModels should only coordinate UI and delegate to services

### File Path Issues
**Error**: Mixed path separators causing cross-platform issues
- **Cause**: Using forward slashes in Windows-specific code
- **Solution**: Use Path.Combine() or consistent backslashes for Windows
- **Prevention**: Always use Windows-style paths in Windows-specific code

### Configuration & Build Issues
**Error**: Missing package references causing compilation failures
- **Cause**: Adding functionality without proper NuGet package dependencies
- **Solution**: Add appropriate PackageReference entries to .csproj files
- **Prevention**: Check dependencies before implementing new features

### C# Language & Syntax Issues
**Error**: Null reference exceptions in property binding
- **Cause**: Not initializing collections or objects before binding
- **Solution**: Initialize in constructor or use null-coalescing operators
- **Prevention**: Always initialize bound properties and use defensive coding

### XAML & WPF Issues
**Error**: Binding failures with no error indication
- **Cause**: Incorrect property names or missing INotifyPropertyChanged
- **Solution**: Verify property names, implement proper change notification
- **Prevention**: Use compile-time binding validation and consistent naming

### Project Management & Task Completion Issues
**Error**: Incomplete task closure without cleanup and documentation
- **Cause**: Focusing on implementation without proper task completion procedures
- **Solution**: Always clean up, document, and verify completion before moving to next task
- **Prevention**: Follow cleanup checklist after each subtask/issue completion

### CI/CD Tool Organization Issues
**Error**: WitcherCI tool infrastructure scattered across multiple directories
- **Cause**: Rapid prototyping without proper organizational structure
- **Solution**: Reorganize into dedicated .vscode\tools\witcherci\ subdirectory with clean separation
- **Prevention**: Follow DandelionCI patterns with dedicated tool directories from start

**Error**: Batch file pause commands breaking automation workflows
- **Cause**: Including `pause` commands in CI/CD batch files for debugging
- **Solution**: Remove all `pause` commands from automation scripts
- **Prevention**: Use conditional pause only in debug mode, never in production automation

**Error**: PowerShell parameter naming mismatch between task definitions and scripts
- **Cause**: Using different parameter conventions (kebab-case vs PascalCase) in JSON vs PowerShell
- **Solution**: Standardize on kebab-case parameters with PowerShell ${parameter-name} syntax
- **Prevention**: Establish consistent parameter naming conventions across all tooling

**Error**: Temporary console applications left in codebase after prototyping
- **Cause**: Creating console apps for quick testing without cleanup plan
- **Solution**: Remove temporary projects, integrate directly with core assemblies
- **Prevention**: Use PowerShell scripts with direct assembly loading instead of console apps

### File System & Path Resolution Issues
**Error**: VS Code editor context showing files that don't exist on file system
- **Cause**: Assuming editor file paths match actual file system locations
- **Solution**: Always verify file existence with Test-Path before path resolution debugging
- **Prevention**: Check physical file existence before investigating path resolution logic

**Error**: PowerShell assembly loading failures with "Mindestens ein Typ in der Assembly kann nicht geladen werden"
- **Cause**: Missing dependency resolution when loading .NET assemblies in PowerShell
- **Solution**: Ensure all required dependencies are available or use alternative integration approaches
- **Prevention**: Test assembly loading in isolation before complex script integration

## 🚨 PowerShell Script Guidelines - CRITICAL

### Unicode and Special Characters
- **NEVER use Unicode symbols** in PowerShell scripts (✓, ❌, 🎯, etc.)
- **Causes parsing errors** in PowerShell 5.1 on Windows
- **Use plain ASCII only**: "Success:", "Error:", "Warning:"
- **File encoding**: Save .ps1 files as UTF-8 without BOM or ASCII

### PowerShell Syntax Rules
- **Always close braces properly** - missing `}` causes cascading parse errors
- **Use consistent quoting** - prefer double quotes for strings with variables
- **Test scripts before committing** - syntax errors break automation
- **Avoid complex Unicode formatting** - stick to basic colored output

### Error Recovery Patterns
```powershell
# Good - Plain ASCII status messages
Write-Host "Success: DZIP format confirmed!" -ForegroundColor Green
Write-Host "Error: File not found" -ForegroundColor Red
Write-Host "Warning: Large file size" -ForegroundColor Yellow

# Bad - Unicode symbols cause parsing failures  
Write-Host "✓ DZIP format confirmed!" -ForegroundColor Green
Write-Host "❌ File not found" -ForegroundColor Red
```

### Learning from Errors
- **Missing braces**: Always count opening/closing braces in complex scripts
- **Unicode issues**: Test all scripts on target PowerShell version (5.1)
- **String termination**: Ensure all quotes are properly closed
- **Variable scope**: Watch for uninitialized variables in script blocks

## 📝 Error Patterns & Solutions - Learning Log

### PowerShell Script Failures
**Error**: "Die Zeichenfolge hat kein Abschlusszeichen" (String not terminated)
- **Cause**: Missing closing braces in foreach loops or if statements
- **Solution**: Always verify brace matching, use proper indentation
- **Prevention**: Count opening/closing braces before running scripts

**Error**: Unicode symbols causing parse failures in PowerShell 5.1
- **Cause**: Using ✓, ❌, 🎯 symbols in PowerShell scripts
- **Solution**: Replace with plain ASCII: "Success:", "Error:", "Warning:"
- **Prevention**: Stick to basic ASCII characters in all .ps1 files

### Architectural Violations
**Error**: Nested public classes in service files
- **Cause**: Putting multiple classes in SaveFileHexAnalyzer.cs
- **Solution**: Extract to separate Model files (HexAnalysisResult.cs, DZipInfo.cs)
- **Prevention**: Follow "one class per file" rule strictly

### File Path Issues
**Error**: Mixed path separators causing cross-platform issues
- **Cause**: Using forward slashes in Windows-specific code
- **Solution**: Use Path.Combine() or consistent backslashes for Windows
- **Prevention**: Always use Windows-style paths in Windows-specific code

## 🔍 Key Files to Reference
- **`frontend/ViewModels/MainViewModel.cs`** - Primary MVVM example
- **`WitcherCore/Services/WitcherSaveFileService.cs`** - Service layer pattern
- **`frontend/Utils/ResourceHelper.cs`** - Localization utilities
- **`PRINCIPLES.md`** - Complete architectural guidelines
- **`installer/Build-Installer.ps1`** - Build automation example
- **`.vscode/tools/witcherci/WitcherCI.ps1`** - Task runner infrastructure
- **`.vscode/tools/witcher-devagent.bat`** - WitcherCI entry point
