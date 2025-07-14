from fastapi import APIRouter
from pathlib import Path
from typing import List, Optional
from pydantic import BaseModel
from utils import format_iso
import os

from config import config

router = APIRouter()


class SaveFile(BaseModel):
    game: str
    file_name: str
    modified_time: float
    modified_time_iso: str
    size: int
    full_path: str
    screenshot_path: Optional[str] = None
    metadata: Optional[dict] = None


@router.get("/saves/witcher2", response_model=List[SaveFile])
def list_witcher2_saves():
    raw_path = config["witcher2"]["save_path"]
    resolved_path = Path(os.path.expandvars(raw_path))

    if not resolved_path.exists():
        return []

    saves = []
    for file in resolved_path.glob("*.sav"):
        base_name = file.stem
        bmp_path = resolved_path / f"{base_name}_640x360.bmp"
        mtime = file.stat().st_mtime

        saves.append(
            SaveFile(
                game="witcher2",
                file_name=file.name,
                modified_time=mtime,
                modified_time_iso=format_iso(mtime),
                size=file.stat().st_size,
                full_path=str(file),
                screenshot_path=str(bmp_path) if bmp_path.exists() else None,
                metadata=None  # to be expanded later
            )
        )
    return saves
