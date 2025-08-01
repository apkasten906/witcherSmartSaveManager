# Witcher Smart Save Manager - AI Coding Instructions

## 🎯 Project Overview
This is a **WPF MVVM application** for managing Witcher game save files with features like backup management, orphaned screenshot cleanup, multi-language support, and **database-enhanced metadata**. Built on **.NET 8.0** with strict architectural principles and a **hybrid storage approach**.

## 🖥️ Development Environment
- **Primary OS**: Windows 10/11
- **Shell**: PowerShell 5.1+ (default)
- **Command Syntax**: Use PowerShell syntax with `;` for command chaining
- **Paths**: Use Windows-style paths (backslashes) in examples
- **Scripts**: All automation uses `.ps1` PowerShell scripts
- **IDE**: Visual Studio Code / Visual Studio
- **Runtime**: .NET 8.0 Windows Desktop
- **Database**: SQLite with DBCode extension for development

## 🏗️ Architecture & Project Structure

### Core Projects
- **`frontend/`** - Main WPF application (WitcherSmartSaveManager.csproj)
- **`WitcherCore/`** - Core business logic library with Models, Services, Data access
- **`Shared/`** - Common types and utilities
- **`WitcherSmartSaveManagerTests/`** - NUnit test project
- **`database/`** - SQLite schema and initialization scripts

### Critical Dependencies
```xml
<!-- Core packages across projects -->
<PackageReference Include="System.Data.SQLite" Version="1.0.118" />
<PackageReference Include="NLog" Version="6.0.2" />
<PackageReference Include="Microsoft.Extensions.Configuration.*" />
```

## 🗃️ Hybrid Database Architecture

### **Two-Tier Storage Strategy**
The application uses a **hybrid approach** that ensures reliability while providing enhancements:

1. **File-Based Core (Always Available)**
   - Basic save file operations work without database
   - File system enumeration and metadata extraction
   - Backup and deletion operations

2. **Database Enhancement Layer (Optional)**
   - **SQLite database**: `witcher_save_manager.db`
   - **Enhanced metadata**: Quest information, character variables, inventory
   - **Performance optimization**: Cached parsing results
   - **Advanced features**: Save file analysis, quest tracking

### **Database Schema Pattern**
```sql
SaveFiles                    # Basic file tracking
├── SaveFileMetadata        # Enhanced file metadata + foreign key
│   ├── QuestInfo          # Parsed quest states (FK to SaveFileMetadata)
│   ├── CharacterVariables # Character stats (FK to SaveFileMetadata)
│   └── InventoryItems     # Future: Inventory tracking
├── LanguageResources      # Localization enhancements
└── DatabaseVersion        # Schema versioning
```

### **Service Layer Pattern**
```csharp
// Hybrid service coordinates file + database operations
public class WitcherSaveFileService
{
    private readonly SaveFileMetadataService _metadataService;
    
    public List<WitcherSaveFile> GetSaveFiles()
    {
        // 1. Get files from filesystem (always works)
        var files = Directory.EnumerateFiles(...);
        
        // 2. Enhance with database metadata asynchronously
        _ = Task.Run(() => EnhanceWithDatabaseMetadataAsync(saveFile));
    }
}

// Database operations service
public class SaveFileMetadataService
{
    public async Task<Dictionary<string, object>> GetEnhancedMetadataAsync(string fileName);
    public async Task<bool> StoreQuestDataAsync(string fileName, List<QuestInfo> quests);
}
```

### **Database Schema Management (Critical Practice)**
- **ALL entity creation scripts** must be saved in `database/` folder
- **Idempotent scripts** using `CREATE TABLE IF NOT EXISTS` patterns
- **Migration scripts** numbered sequentially (e.g., `001_initial.sql`, `002_add_quests.sql`)
- **Schema documentation** with table relationships and foreign keys
- **Why this matters**: Enables easy database recreation, reliable deployments, clear audit trail, and team collaboration

## 🎨 MVVM Patterns

### Strict MVVM Rules
- **NO business logic in code-behind** - ViewModels handle all logic
- **NO direct file operations in ViewModels** - delegate to services
- **Commands over event handlers** - use `RelayCommand` pattern
- **Binding-only UI updates** - `INotifyPropertyChanged` everywhere
- **Database operations in services only** - ViewModels never touch SQLite directly

### Enhanced ViewModel Pattern
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
    private readonly SaveFileMetadataService _metadataService;
    
    // Always use game-specific extensions
    string extensionPattern = GameSaveExtensions.GetExtensionForGame(gameKey);
    
    // Handle screenshot relationships
    var screenshotName = saveName + "_640x360.bmp";
    
    // Hybrid approach: file + database
    public List<WitcherSaveFile> GetSaveFiles()
    {
        // 1. File system operations (always work)
        var files = Directory.EnumerateFiles(_saveFolder, extensionPattern);
        
        // 2. Enhance with database metadata asynchronously
        _ = Task.Run(async () => await EnhanceWithDatabaseMetadataAsync(saveFile));
        
        return saveFiles;
    }
    
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

### Database Service Pattern
```csharp
public class SaveFileMetadataService
{
    // Async database operations - never block UI
    public async Task<Dictionary<string, object>> GetEnhancedMetadataAsync(string fileName)
    {
        using var connection = new SQLiteConnection(_connectionString);
        // Query enhanced metadata from SaveFileMetadata, QuestInfo, etc.
    }
    
    // Store parsed save file data
    public async Task<bool> StoreQuestDataAsync(string fileName, List<QuestInfo> quests)
    {
        // Store quest data in QuestInfo table with FK to SaveFileMetadata
    }
}
```

### Orphaned File Detection
- **Automatic detection** after save operations
- **User choice** for cleanup with clear explanations  
- **Graceful locked file handling** with detailed reporting

## 🎨 Enhanced UI Patterns

### SaveFileViewModel Enhancement
```csharp
public class SaveFileViewModel : INotifyPropertyChanged
{
    // Database-enhanced properties
    public bool HasDatabaseMetadata => SaveFile.Metadata.ContainsKey("database_enhanced");
    public string CurrentQuest => GetActiveQuest();
    public int QuestCount => GetQuestCount();
    public string QuestDisplay => QuestCount > 0 ? $"{CurrentQuest} ({QuestCount} total)" : "No Quest Data";
    public string MetadataStatus => HasDatabaseMetadata ? $"Enhanced ({QuestCount} quests)" : "Basic metadata";
}
```

### UI Column Integration
```xml
<!-- Enhanced DataGrid columns show database metadata -->
<DataGridTextColumn Header="Current Quest" Binding="{Binding QuestDisplay}" Width="200"/>
<DataGridTextColumn Header="Metadata" Binding="{Binding MetadataStatus}" Width="150"/>
```

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
7. **Database operations are async** - never block UI with SQLite calls
8. **Hybrid approach first** - ensure file operations work without database
9. **Use SaveFileMetadataService** for all database operations, not direct SQLite
10. **Always save database scripts** - every schema change goes in `database/` folder

## 🔍 Key Files to Reference
- **`frontend/ViewModels/MainViewModel.cs`** - Primary MVVM example
- **`WitcherCore/Services/WitcherSaveFileService.cs`** - Service layer pattern
- **`WitcherCore/Services/SaveFileMetadataService.cs`** - Database operations
- **`frontend/ViewModels/SaveFileViewModel.cs`** - Enhanced UI properties
- **`frontend/Utils/ResourceHelper.cs`** - Localization utilities
- **`database/initialize_database.sql`** - Database schema reference
- **`PRINCIPLES.md`** - Complete architectural guidelines
- **`installer/Build-Installer.ps1`** - Build automation example
