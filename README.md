# 🧙‍♂️ Witcher Smart Save Manager

An intelligent, cross-version **save game manager** for the Witcher series (starting with **The Witcher 2**). Designed to reduce save clutter, optimize cloud usage, and preserve key decision points — all while giving you full control over your files.

---

## 🧩 Architecture: Python + C# Hybrid

This project follows a **loosely coupled hybrid architecture**:

### ⚙️ `api/` — Python (FastAPI Backend)
- REST API that handles:
  - Scanning save files
  - Extracting metadata (timestamps, quest names, etc.)
  - Serving screenshot previews
  - Deleting selected saves
- Runs independently as a local service.
- Can be tested or extended without the frontend.

### 🪟 `frontend/` — C# (.NET WinForms GUI)
- Windows-native desktop application.
- Queries the REST API to:
  - Display saves visually (like in-game)
  - Select and delete multiple saves
  - Let users override save folder paths
- No tight integration — talks to Python backend via HTTP only.

---

## 🚀 Getting Started

### ✅ Prerequisites

- Python 3.11+
- .NET SDK 8.0+
- Git, PowerShell (for optional scaffolding)

---

### 🐍 Backend Setup (FastAPI)

```bash
cd api
python -m venv .venv
.venv\Scripts\activate    # or source .venv/bin/activate on Linux/mac
pip install -r requirements.txt
uvicorn main:app --reload
```

API will be live at: `http://localhost:8000`

---

### 🖥 Frontend Setup (C# WinForms)

```bash
cd frontend
dotnet build
dotnet run
```

Make sure the backend is running before starting the GUI.

---

## 📦 Folder Structure

```
api/             → FastAPI backend (scan/delete saves, serve images)
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
- [ ] Add support for Witcher 1 and Witcher 2
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
