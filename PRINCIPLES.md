# Witcher Smart Save Manager – Development Principles

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

## 🧾 UI & UX

* Maintain a consistent **dark fantasy Witcher aesthetic**.
* Reuse **named styles** for all controls (e.g., NavButtonStyle, ActionButtonStyle).
* No hardcoded strings in XAML – bind through ViewModels or resource dictionaries.
* All text must support future localization (via `.resx` or bindings).
* Asset paths (e.g., for icons or images) should be bound via ViewModel properties.

## 🔢 Code Practices

* **No hardcoded strings or paths** – use enums, config files, or static constants.
* Prefer clear, self-documenting names (e.g., `timestamp` instead of `ts`).
* Use `enum` types for:

  * Game identifiers (Witcher1, Witcher2, Witcher3)
  * Source (Steam, GOG)

Avoid using hardcoded strings for programmatic decision making. This ensures these values are easy to revise without making sweeping changes to the code base when they are used frequently (such as game iteration identifiers). Additionally, this reduces the chance of error due to typos, because they are incorporated into the C# type-safety scheme.

* Define display names and paths in a config-backed model, not inline.

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

## 🔗 Git Workflow

* **Branch Naming**: All feature branches must follow `feat/{issue-number}-{description}` format
  - Examples: `feat/56-link-branch-to-issue`, `feat/123-add-new-feature`
  - This ensures automatic linking to GitHub issues for project tracking
* **Commit Messages**: Use conventional commit format with clear, descriptive messages
* **Pre-commit Hooks**: Always install Git hooks (`.\Install-GitHooks.ps1`) for:
  - Branch naming validation
  - Code compilation verification
  - Unit test execution
* **Special Branches**: `main`, `dev`, `master`, and `hotfix/*` are exempt from naming validation

---

> 🧠 "Write code as if the next person to maintain it knows nothing — and might be you in six months."
