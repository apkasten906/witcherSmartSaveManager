# Set Python path for imports
$env:PYTHONPATH = "api"

# Activate the virtual environment in this shell (optional if only running in new window)
. "$PSScriptRoot\venv\Scripts\Activate.ps1"

# Start the FastAPI server in a new PowerShell window, with activation inside that window
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    ". '$PSScriptRoot\venv\Scripts\Activate.ps1'; uvicorn main:app --reload --app-dir api"
)

# Wait briefly, then open Swagger docs
Start-Sleep -Seconds 2
Start-Process "http://localhost:8000/docs"
