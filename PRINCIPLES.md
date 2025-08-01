# Witcher Smart Save Manager – Development Principles

## 🖥️ Platform Requirements

* **Target Platform**: Windows 10 (version 1607+) and Windows 11 (64-bit)
* **.NET Runtime**: .NET 8.0 Desktop Runtime
* **Legacy Support**: Windows 7/8/8.1 not supported (due to .NET 8.0 requirements)
* **Installation**: No restart required - registry entries are data storage only

## 🧱 Architecture

* Follow **MVVM** for all WPF UI components.
* All save game logic must reside in services – never in the UI code.
* UI updates must happen via bindings to ViewModels only.
* Use **dependency injection** for all services and utilities.
* No tight coupling between components – keep them modular.

## 🧪 Testing

* Use **NUnit** for all unit tests.
* All file system operations (save, delete, backup) must be mockable and covered by tests.
* ViewModels should be testable without UI context.
* All test scripts must be written using Pester and integrated into the Visual Studio project structure.
* Tests should be runnable directly from Visual Studio or as part of the build process.

## 🧾 UI & UX

* Maintain a consistent **dark fantasy Witcher aesthetic**.
* Reuse **named styles** for all controls (e.g., NavButtonStyle, ActionButtonStyle).
* No hardcoded strings in XAML – bind through ViewModels or resource dictionaries.
* All text must support **multi-language localization**:
  - Use `.resx` files for all user-facing strings
  - Access via `ResourceHelper.GetString()` and `ResourceHelper.GetFormattedString()`
  - Maintain consistency between English (`Strings.en.resx`) and German (`Strings.de.resx`)
  - Use witchery-themed messaging for enhanced user experience (e.g., "Kikimora alerts" for file cleanup)
* Asset paths (e.g., for icons or images) should be bound via ViewModel properties.
* **Error Handling in UI**:
  - Always provide user-friendly error messages with context
  - Handle locked file scenarios gracefully with clear explanations
  - Use `StringToImageSourceConverter` for safe image binding
  - Display progress and status information for long-running operations

## 🔢 Code Practices

* **No hardcoded strings or paths** – use enums, config files, or static constants.
* Prefer clear, self-documenting names (e.g., `timestamp` instead of `ts`).
* Use `enum` types for:

  * Game identifiers (Witcher1, Witcher2, Witcher3)
  * Source (Steam, GOG)

Avoid using hardcoded strings for programmatic decision making. This ensures these values are easy to revise without making sweeping changes to the code base when they are used frequently (such as game iteration identifiers). Additionally, this reduces the chance of error due to typos, because they are incorporated into the C# type-safety scheme.

* Define display names and paths in a config-backed model, not inline.

## 📁 File Management Patterns

* **Dynamic File Extensions**: Always use `GameSaveExtensions.GetExtensionForGame(gameKey)` instead of hardcoded patterns
  - Ensures consistency between save loading and backup counting
  - Supports multiple game types with different file formats
* **Orphaned File Cleanup**: 
  - Detect orphaned files after save operations
  - Provide user choice for cleanup with clear explanations
  - Handle locked files gracefully with detailed error reporting
  - Use witchery-themed messaging for enhanced user experience
* **Robust Error Handling**:
  - Always check if files exist before operations
  - Handle `IOException` for locked files during gameplay
  - Provide detailed error context to users
  - Log errors appropriately without excessive debug noise
* **Backup Operations**:
  - Always create backups before destructive operations
  - Maintain consistent counting logic across all file operations
  - Update UI counters immediately after file changes

## 📁 Asset Use

* Only include artwork that is:

  * Legally licensed
  * Self-generated (e.g. mockup image or AI-generated with license)
* Store assets in an `/Assets` folder and reference them via bound properties.

## 🛠 Configuration

* Keep default save paths and file extensions in `App.config`.
* Use a centralized `GameConstants.cs` class to manage known strings/enums.
* If multiple install paths are found, always defer to user choice via prompt in the UI.

## 🧩 Extensibility

* Plan for future support of Witcher 1 and 3 (currently building for Witcher 2).
* Smart Save State logic should be a pluggable module (strategy pattern preferred).

## 📚 Documentation

* Update this document when any architectural rule or pattern changes.
* Link this document in `README.md`.
* Keep project layout, file responsibilities, and patterns clear for future contributors.

## 🔗 Git Workflow & Versioning

* **Branch Naming**: All feature branches must follow `feat/{issue-number}-{description}` format
  - Examples: `feat/56-link-branch-to-issue`, `feat/123-add-new-feature`
  - This ensures automatic linking to GitHub issues for project tracking
* **Commit Messages**: Use **Conventional Commits** format for automatic semantic versioning:
  - `feat:` → Minor version bump (1.0.0 → 1.1.0)
  - `fix:` → Patch version bump (1.0.0 → 1.0.1)
  - `docs:`, `chore:`, `style:`, `refactor:`, `test:` → Patch version bump
  - `BREAKING CHANGE:` or `feat!:` → Major version bump (1.0.0 → 2.0.0)
  - **Examples**:
    ```
    feat: add cloud sync functionality
    fix: resolve save file corruption issue
    feat!: redesign save file format
    docs: update installation instructions
    ```
* **Automated Versioning**: 
  - Releases are automatically created when merging to `main` branch
  - Version numbers follow semantic versioning based on commit messages
  - CI/CD pipeline analyzes commit history since last tag to determine version bump
  - **Release notes are automatically generated** from commit messages, categorized by type:
    - 🚨 Breaking Changes, ✨ New Features, 🐛 Bug Fixes, 📚 Documentation, 🔧 Maintenance
* **GitHub Environments**:
  - **Production Environment**: Used for `main` branch with protection rules and releases
    - Requires approval from designated reviewers before deployment
    - Optional wait timer for additional safety (5 minutes recommended)
    - Complete audit trail of all deployments and approvals
  - **Development Environment**: Used for `dev` branch with fast feedback and no versioning
  - Environment-specific variables and secrets support
* **Manual Version Override**: Include `version: 1.2.3` in commit message to specify exact version
* **Large Batch Handling**: Merges with >20 commits automatically get major version bump
* **Pre-commit Hooks**: Always install Git hooks (`.\Install-GitHooks.ps1`) for:
  - Branch naming validation
  - Code compilation verification
  - Unit test execution
* **Special Branches**: `main`, `dev`, `master`, and `hotfix/*` are exempt from naming validation

## 🤖 AI-Assisted Development

* **Git Workflow Discipline**: AI tools must **never** suggest shortcuts that bypass proper Git workflow
  - ❌ **Never commit directly** to `main` or `dev` branches
  - ❌ **Never bypass** feature branches for "quick fixes"
  - ❌ **Never skip** PR review process, even for "trivial" changes
  - ✅ **Always follow** the complete workflow: Issue → Feature Branch → Dev → Main
* **Best Practice Enforcement**: AI should be the **voice of best practices**, not an enabler of shortcuts
  - Explain WHY proper workflow matters before suggesting alternatives
  - Point out risks and technical debt created by shortcuts
  - Make good practices feel natural and easy to follow
  - Resist user pressure for "just this once" exceptions
* **Traceability & Accountability**: Every change must maintain clear paper trail
  - Link commits back to specific issues and feature branches
  - Preserve context and reasoning for future developers
  - Enable easy rollback and debugging when issues arise
  - Support collaborative development and code review processes
* **Learning Responsibility**: AI has responsibility to teach proper development practices
  - Junior developers often learn from AI suggestions
  - Solo developers may adopt AI shortcuts as "best practices"
  - Bad habits spread quickly when reinforced by AI tools
  - **Mutual Accountability**: Both human and AI should remind each other of standards

---

> 🧠 "Write code as if the next person to maintain it knows nothing — and might be you in six months."
