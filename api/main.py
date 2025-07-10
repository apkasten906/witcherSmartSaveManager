from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from routes import saves

app = FastAPI()

# Allow frontend access
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(saves.router, prefix="/api", tags=["saves"])