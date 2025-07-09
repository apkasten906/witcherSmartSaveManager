from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from save_scanner import scan_saves

app = FastAPI()

# Allow frontend access
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/saves")
def get_saves():
    return scan_saves()
