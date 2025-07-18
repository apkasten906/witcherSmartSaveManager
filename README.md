# 🧙‍♂️ Witcher Smart Save Manager

An intelligent, cross-version **save game manager** for the Witcher series (starting with **The Witcher 2**). Designed to reduce save clutter, optimize cloud usage, and preserve key decision points — all while giving you full control over your files.

---

### 🪟 C# (.NET WinForms GUI)
- Windows-native desktop application.
- Integrated service layer for managing files

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
frontend/        → C# WinForms frontend
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
