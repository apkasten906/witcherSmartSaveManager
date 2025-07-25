# 🧙‍♂️ Witcher Smart Save Manager

The goal of the Windows Smart Save Manager is to provide an intelligent, cross-version **save game manager** for the Witcher series (starting with **The Witcher 2**). The games in the Witcher series were all developed without cloud storage and mind and as a consequence, save games tend to accumulate rapidly and overwhelm the small amount of free cloud storage offered by GoG Galaxy and Steam.  Therefore, the Witcher Smart Save Managers seeks to reduce clutter, optimize cloud usage, and preserve key decision points — all while giving you full control over your files.

Current Version: v0.1
- Supports detection of **Witcher 2** save files based on Manufacturer Default Witcher 2 installation location (%userpath%\Documents\Witcher 2\gamesaves)
- Supports individual and bulk backing up of saves to chosen location
- Supports backup verification
- Offers Thumbnails, save date, and save game file name for identification
- Supports individual and bulk Deleting of saves

  INSTALL: Compile to Release and copy files from bin
  NOTE: In this version, if you do not want your saves to be downloaded from cloud, you will either need to turn off your cloud sync and/or delete the save games out of your cloud storage.

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


## 📦 Folder Structure

```
frontend/        → C# WPF frontend
.github/         → CI and PR templates
docs/            → Planning docs (e.g. Trello CSV)
```

---

## 🛠 Current Features

- Auto-detect or manually select Witcher 2 save folder
- Display list of saves with:
  - Quest label (if extractable)
  - Timestamp
  - Screenshot thumbnail
- Multi-select save deletion
- REST API + decoupled desktop frontend

---

## 🔮 Roadmap

- [ ] Add save analysis logic for critical decision points
- [x] Add support for Witcher 1, Witcher 2, and Witcher 3
- [ ] Steam/GOG save path detection
- [ ] Backup feature before deletion
- [ ] Cloud sync support (OneDrive/GOG Galaxy)

---

## 🤝 Contributing

Want to help build a better way to manage Geralt's journey?

1. Fork and clone the repo
2. Create a feature branch (`feature/xyz`)
3. Open a pull request — PR template is in `.github/pull_request_template.md`
4. Use the Trello board for reference (`docs/witcher_save_manager_user_stories.csv`)

---

## 📜 License

MIT — do what you want, but don’t sell this to the Wild Hunt.

---

## 👑 Credits

Project by [@apkasten906](https://github.com/apkasten906). Built with 🐍 Python, 🪟 C#, and a love for narrative-driven RPGs.
