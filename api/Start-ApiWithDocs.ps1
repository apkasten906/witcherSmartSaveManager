# Start-ApiWithDocs.ps1

# Set Python path for imports
$env:PYTHONPATH = "api"

# Activate the virtual environment
. .\venv\Scripts\Activate.ps1

# Start the FastAPI server in the background
Start-Process powershell -ArgumentList "-NoExit", "-Command", "uvicorn main:app --reload --app-dir api"

# Wait briefly, then open Swagger docs
Start-Sleep -Seconds 2
Start-Process "http://localhost:8000/docs"
