from fastapi import APIRouter
from pathlib import Path
from typing import List
from pydantic import BaseModel
import os

router = APIRouter()

class SaveFile(BaseModel):
    file_name: str
    modified_time: float
    size: int
    full_path: str

@router.get("/saves", response_model=List[SaveFile])
def list_witcher2_saves():
    save_dir = Path(os.path.expandvars(r"%USERPROFILE%\Documents\Witcher 2\gamesaves"))
    if not save_dir.exists():
        return []

    saves = []
    for file in save_dir.glob("*.sav"):
        saves.append(SaveFile(
            file_name=file.name,
            modified_time=file.stat().st_mtime,
            size=file.stat().st_size,
            full_path=str(file)
        ))
    return saves
