from fastapi import APIRouter
from pathlib import Path
from typing import List
from pydantic import BaseModel
import os

from config import config

router = APIRouter()


class SaveFile(BaseModel):
    file_name: str
    modified_time: float
    size: int
    full_path: str


@router.get("/saves", response_model=List[SaveFile])
def list_witcher2_saves():
    raw_path = config["witcher2"]["save_path"]
    resolved_path = Path(os.path.expandvars(raw_path))

    if not resolved_path.exists():
        return []

    saves = []
    for file in resolved_path.glob("*.sav"):
        saves.append(
            SaveFile(
                file_name=file.name,
                modified_time=file.stat().st_mtime,
                size=file.stat().st_size,
                full_path=str(file),
            )
        )
    return saves
