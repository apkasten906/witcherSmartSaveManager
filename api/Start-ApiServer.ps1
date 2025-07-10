# Start-ApiServer.ps1

# Set PYTHONPATH so imports like `from routes import saves` work
$env:PYTHONPATH = "api"

# Activate the virtual environment
. .\venv\Scripts\Activate.ps1

# Start the FastAPI server with auto-reload
uvicorn main:app --reload --app-dir api
