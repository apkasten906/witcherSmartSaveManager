# 🧙‍♂️ Witcher Smart Save Manager

The goal of the Windows Smart Save Manager is to provide an intelligent, cross-version **save game manager** for the Witcher series (starting with **The Witcher 2**). The games in the Witcher series were all developed without cloud storage and mind and as a consequence, save games tend to accumulate rapidly and overwhelm the small amount of free cloud storage offered by GoG Galaxy and Steam. Therefore, the Witcher Smart Save Managers seeks to reduce clutter, optimize cloud usage, and preserve key decision points — all while giving you full control over your files.

---

## ✨ Key Features

### 🐺 Orphaned Screenshot Cleanup (NEW!)
**Kikimora Detection & Cleanup** - When save files are deleted while the game is running, screenshot files can become "orphaned" and remain on disk. The Smart Save Manager now:
- **Automatically detects** orphaned screenshots after loading saves
- **Witchery-themed notifications** alert you to orphaned files with lore-friendly messages
- **Handles locked files gracefully** - if files are locked by the game, shows which files couldn't be deleted and why
- **Persistent file counters** display total loaded saves and backed up files in real-time

### 📊 Smart File Management
- **Backup before delete** - Always creates backups before removing save files
- **Cross-platform paths** - Automatic detection of Steam/GOG save locations
- **Multi-language support** - English and German with full localization
- **Real-time counters** - Live display of loaded saves and backup file counts

### 🎮 Game-Specific Support
- **Dynamic file extensions** - Supports different save file formats per game
- **Screenshot handling** - Manages both save files and their associated screenshot files
- **Metadata preservation** - Maintains file timestamps and properties during operations

---

## 🧱 Architecture

* Follow **MVVM** (Model-View-ViewModel) for all WPF UI components.
* All save game logic must reside in services – never in the UI code.
* UI updates must happen via bindings to ViewModels only.
* Use **dependency injection** for all services and utilities.
* No tight coupling between components – keep them modular.

---

## 🚀 Getting Started

### ✅ Prerequisites

- .NET SDK 8.0+
- Git, PowerShell (for optional scaffolding)

---

### 🖥 Setup (C# WinForms)

```bash
cd frontend
dotnet build
dotnet run
```

---

## 📦 Folder Structure

```
frontend/        → C# WPF frontend
.github/         → CI and PR templates
docs/            → Documentation and planning resources
installer/       → Scripts and files for building the installer
scripts/         → Utility scripts for project management
WitcherSmartSaveManagerTests/ → Unit and integration tests
```

---

## 🔮 Roadmap

- [x] Add support for Witcher 2
- [x] Steam/GOG save path detection
- [x] Backup feature before deletion
- [ ] Display save metadata for easier decision making
- [ ] Add save analysis logic for critical decision points
- [ ] Add support for Witcher 1 and Witcher 3
- [ ] Cloud sync support (OneDrive/GOG Galaxy) (if possible)

---

## 📚 Documentation

- **[Development Principles](PRINCIPLES.md)** - Architecture guidelines and coding standards
- **[Orphaned Screenshot Cleanup](docs/orphaned-screenshot-cleanup.md)** - Detailed feature documentation
- **[Changelog](CHANGELOG.md)** - Version history and release notes
- **[Installation Guide](installer/README.md)** - Building and installing the application

---

## 🤝 Contributing

Want to help build a better way to manage Geralt's journey?

1. Fork and clone the repo
2. Install Git hooks: `./Install-GitHooks.ps1` (ensures code quality)
3. Create a feature branch following the naming convention: `feat/{issue-number}-{description}`
   - ✅ Example: `feat/56-link-branch-to-issue`, `feat/123-add-new-feature`
   - This ensures automatic linking to GitHub issues for better project tracking
4. Make your changes using **Conventional Commits** for automatic versioning:
   ```bash
   # Examples:
   git commit -m "feat: add cloud sync functionality"        # Minor bump
   git commit -m "fix: resolve save file corruption"         # Patch bump  
   git commit -m "feat!: redesign save file format"          # Major bump
   git commit -m "docs: update installation instructions"    # Patch bump
   ```
5. Hooks will validate code quality and branch naming on commit
6. Open a pull request — PR template is in `.github/pull_request_template.md`
7. **Automatic Releases**: When merged to `main`, CI will automatically:
   - Analyze your commit messages for semantic versioning
   - Create appropriate version tags (major/minor/patch)
   - Generate categorized release notes from commit history
   - Build and publish releases with installers

### 📋 Commit Message Guidelines

Use **Conventional Commits** for automatic semantic versioning:
- `feat:` → Minor version bump (1.0.0 → 1.1.0)
- `fix:` → Patch version bump (1.0.0 → 1.0.1)  
- `docs:`, `chore:`, `style:`, `refactor:`, `test:` → Patch version bump
- `BREAKING CHANGE:` or `feat!:` → Major version bump (1.0.0 → 2.0.0)

---

## 🗂 GitHub Project Management

This repository uses GitHub Projects to track issues, features, and progress. The project board is organized into the following columns:

- **Todo**: Tasks that are planned but not yet started.
- **In Progress**: Tasks currently being worked on.
- **Done**: Completed tasks.

### How to Contribute to the Project Board

1. When creating a new issue, ensure it is linked to the appropriate project.
2. Use labels to categorize the issue (e.g., `bug`, `enhancement`, `documentation`).
3. Move the issue to the correct column as work progresses.

For more details, visit the [GitHub Project Board](https://github.com/apkasten906/witcherSmartSaveManager/projects).

---

## 🏷 Versioning

This project uses Git tags to manage versioning. The build process extracts the version number from the latest Git tag to dynamically update the application and installer versions.

### How to Create a Git Tag

1. Ensure your changes are committed.
2. Create a new tag with the desired version number:
   ```bash
   git tag -a v1.0.0 -m "Release version 1.0.0"
   ```
3. Push the tag to the remote repository:
   ```bash
   git push origin v1.0.0
   ```

The build process will fail if no Git tag is found. Always create a tag before building a release.

---

## 📦 Installer Location

The installer for Witcher Smart Save Manager is generated in the `Output` directory of the `installer` folder. After building the installer, you can find it at:

```
installer\Output\WitcherSaveManagerInstaller.exe
```

Use this file to install the application on Windows systems.

---

## 🌍 Supported Languages

The Witcher Smart Save Manager currently supports the following languages with full localization:

- **English** - Complete interface and witchery-themed messaging
- **German** - Vollständige deutsche Übersetzung mit hexerischen Nachrichten

### Recent Localization Enhancements
- Orphaned screenshot cleanup messages with lore-friendly themes
- Real-time file counter labels
- Comprehensive error message translations
- Resource-based localization system using `.resx` files

If you'd like to contribute translations for additional languages, feel free to open a pull request or contact the maintainers.

---

## 📜 License

MIT — do what you want, but don’t sell this to the Wild Hunt.

---

## 👑 Credits

Project by [@apkasten906](https://github.com/apkasten906). Built with 🪟 C# and a love for narrative-driven RPGs.
