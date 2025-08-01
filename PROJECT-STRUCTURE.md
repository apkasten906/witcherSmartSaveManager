# Witcher Smart Save Manager - Project Structure

This document outlines the architectural structure of the Witcher Smart Save Manager solution, following strict MVVM separation and clean architecture principles.

## 📁 Solution Overview

```
WitcherSmartSaveManager.sln
│
├── frontend/                    # WPF UI project (MVVM-driven)
│   ├── App.xaml
│   ├── MainWindow.xaml
│   ├── Views/                   # XAML views
│   │   └── MainView.xaml
│   ├── ViewModels/              # ViewModels (pure logic, no UI code)
│   │   └── MainViewModel.cs
│   ├── Converters/              # IValueConverter implementations
│   ├── Resources/               # .resx files and shared styles
│   ├── Assets/                  # Static UI assets (icons, images)
│   ├── Styles/                  # App-wide styles (merged into App.xaml)
│   ├── Services/                # Service interfaces and WPF-specific wrappers
│   │   └── IFolderDialogService.cs
│   ├── Utils/                   # UI helpers (ResourceHelper, etc.)
│   └── WitcherSmartSaveManager.csproj
│
├── WitcherCore/                 # Core logic, no WPF references
│   ├── Services/                # Business logic (SaveFileService, BackupService, etc.)
│   │   ├── IWitcherSaveFileService.cs
│   │   ├── WitcherSaveFileService.cs
│   │   ├── SaveFileMetadataService.cs  # Database-enhanced metadata management
│   │   └── MetadataExtractor.cs        # Save file parsing logic
│   ├── Data/                    # Data access, file IO abstraction
│   │   └── ISaveGameRepository.cs
│   ├── Models/                  # Domain models (SaveGame, GameSource enum, etc.)
│   │   ├── GameConstants.cs
│   │   ├── WitcherSaveFile.cs         # Core save file model
│   │   └── SaveFileMetadata.cs        # Database metadata model
│   └── WitcherCore.csproj
│
├── Services/                    # Legacy services (being migrated to WitcherCore)
│   ├── WitcherSaveFileService.cs
│   └── Services.csproj
│
├── Shared/                      # Common types and utilities
│   ├── Models/                  # Shared model definitions
│   └── Shared.csproj
│
├── WitcherSmartSaveManagerTests/ # NUnit Test project
│   ├── Services/                # Unit tests for services
│   ├── ViewModels/              # Unit tests for viewmodels
│   ├── MockSaveFileService.cs   # Test mocks and utilities
│   └── WitcherSmartSaveManagerTests.csproj
│
├── installer/                   # Inno Setup installer scripts
│   ├── Build-Installer.ps1
│   ├── setup.iss
│   └── INSTALLATION.md
│
├── .github/                     # Workflows, issues, templates, CI/CD
│   ├── workflows/
│   └── copilot-instructions.md
│
├── scripts/                     # Development and automation scripts
│   └── Manage-GitHubIssues.ps1
│
├── docs/                        # Documentation
│   └── docs-wpf-checkbox-best-practices.md
│
├── database/                    # Database schema and scripts
│   └── initialize_database.sql         # SQLite schema with hybrid architecture
│
├── App.config                   # Global config (referenced by DI container)
├── README.md
├── PRINCIPLES.md                # Project architecture guidelines
└── PROJECT-STRUCTURE.md         # This file
```

## 🗃️ Hybrid Database Architecture

The application implements a **hybrid approach** combining file-based core functionality with optional database enhancements:

### **File-Based Core (Always Available)**
- Basic save file operations work without database
- File system enumeration and metadata extraction
- Backup and deletion operations

### **Database Enhancement Layer (Optional)**
- **SQLite database**: `witcher_save_manager.db`
- **Enhanced metadata storage**: Quest information, character variables, inventory
- **Performance optimization**: Cached file metadata and parsing results
- **Advanced features**: Save file analysis, quest tracking, character progression
- **Automated schema initialization**: `InitializeDatabaseAsync()` creates tables for testing/deployment
- **Graceful degradation**: System works even when database features are unavailable

### **Database Schema**
```sql
SaveFiles                    # Basic file tracking
├── SaveFileMetadata        # Enhanced file metadata
│   ├── QuestInfo          # Parsed quest states
│   ├── CharacterVariables # Character stats and variables
│   └── InventoryItems     # Future: Inventory tracking
├── LanguageResources      # Localization enhancements
└── DatabaseVersion        # Schema versioning
```

### **Hybrid Service Pattern**
- **WitcherSaveFileService**: Coordinates file + database operations
- **SaveFileMetadataService**: Handles all database operations with error handling
- **MetadataExtractor**: Parses save file content for database storage
- **Database Testing**: Comprehensive integration tests verify hybrid functionality
- **Schema Management**: All database entity scripts saved in `database/` folder

## 🎯 Architecture Principles

### **1. Strict MVVM Separation**
- **Frontend**: Pure WPF UI layer with ViewModels handling all logic
- **WitcherCore**: Business logic with no UI dependencies
- **Services**: Legacy layer being migrated to WitcherCore

### **2. Dependency Flow**
```
frontend → Services → WitcherCore → Shared
    ↓
   Tests (can reference all layers for integration testing)
```

### **3. Key Architectural Decisions**

#### **UI Layer (frontend/)**
- **ViewModels**: All business logic, command handling, property binding
- **Views**: Pure XAML with minimal code-behind
- **Services**: WPF-specific implementations (dialogs, etc.)
- **Utils**: UI helpers like `ResourceHelper` for localization

#### **Core Layer (WitcherCore/)**
- **Services**: Business logic interfaces and implementations
- **Data**: File I/O abstraction and repository patterns
- **Models**: Domain models and constants
- **Configuration**: Settings and path resolution

#### **Legacy (Services/)**
- Contains `WitcherSaveFileService` - being migrated to WitcherCore
- Will be deprecated once migration is complete

#### **Testing (WitcherSmartSaveManagerTests/)**
- NUnit-based unit and integration tests
- Mock services for isolated testing
- Covers ViewModels, Services, and Core logic

## 🔧 Key Technologies

- **.NET 8.0**: Core framework
- **WPF**: UI framework with MVVM
- **SQLite**: Enhanced save file metadata storage (hybrid approach)
- **System.Data.SQLite**: Database access layer
- **NLog**: Logging framework
- **NUnit**: Testing framework
- **Inno Setup**: Installer creation
- **DBCode**: Database management and query execution (development)

## 📋 Migration Status

The project is currently migrating from a single-project structure to a clean multi-project architecture:

- ✅ **WitcherCore**: New core logic layer established
- ✅ **Database Integration**: Hybrid SQLite architecture implemented with automated schema initialization
- ✅ **Enhanced Metadata**: SaveFileMetadataService for database operations with graceful degradation
- ✅ **UI Enhancements**: Database-powered save file information display (Current Quest, Metadata Status columns)
- ✅ **Testing**: Comprehensive database integration test coverage (6/7 passing)
- ✅ **Hybrid Architecture**: File-based core + optional database enhancements working end-to-end
- 🔄 **Services**: Legacy services being moved to WitcherCore
- ✅ **Frontend**: MVVM patterns established
- 🚧 **Save File Parsing**: Witcher 2 content parsing (ready for implementation)

## 🎮 Witcher Game Support

The application supports save file management for:
- **Witcher 1**: `.WitcherSave` files
- **Witcher 2**: `.sav` files with screenshot relationships and enhanced quest parsing
- **Witcher 3**: `.sav` files with quest integration

### **Enhanced Witcher 2 Features**
- **Quest tracking**: Database storage of quest states and progression
- **Character variables**: Parsed character stats and game variables
- **Save file analysis**: Enhanced metadata with database persistence
- **Screenshot correlation**: Automatic `.bmp` screenshot association

Each game has specific file patterns and metadata handling defined in `GameSaveExtensions` and related configuration classes.
