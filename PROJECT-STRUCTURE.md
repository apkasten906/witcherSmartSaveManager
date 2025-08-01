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
│   │   └── IWitcherSaveFileService.cs
│   ├── Data/                    # Data access, file IO abstraction
│   │   └── ISaveGameRepository.cs
│   ├── Models/                  # Domain models (SaveGame, GameSource enum, etc.)
│   │   └── GameConstants.cs
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
├── App.config                   # Global config (referenced by DI container)
├── README.md
├── PRINCIPLES.md                # Project architecture guidelines
└── PROJECT-STRUCTURE.md         # This file
```

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
- **SQLite**: Save file metadata storage
- **NLog**: Logging framework
- **NUnit**: Testing framework
- **Inno Setup**: Installer creation

## 📋 Migration Status

The project is currently migrating from a single-project structure to a clean multi-project architecture:

- ✅ **WitcherCore**: New core logic layer established
- ✅ **Testing**: Comprehensive test coverage
- 🔄 **Services**: Legacy services being moved to WitcherCore
- ✅ **Frontend**: MVVM patterns established

## 🎮 Witcher Game Support

The application supports save file management for:
- **Witcher 1**: `.WitcherSave` files
- **Witcher 2**: `.sav` files with screenshot relationships
- **Witcher 3**: `.sav` files with quest integration

Each game has specific file patterns and metadata handling defined in `GameSaveExtensions` and related configuration classes.
