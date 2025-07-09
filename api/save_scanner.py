import os
from pathlib import Path
from typing import List, Dict
from datetime import datetime

DEFAULT_WITCHER2_PATH = Path.home() / "Documents" / "Witcher 2" / "gamesaves"

def scan_saves(path: Path = DEFAULT_WITCHER2_PATH) -> List[Dict]:
    if not path.exists():
        return ['The Witcher 2 save path does not exist. Please check the path.']

    saves = []
    for file in path.glob("*.save"):
        save_info = {
            "filename": file.name,
            "filepath": str(file),
            "timestamp": datetime.fromtimestamp(file.stat().st_mtime).isoformat(),
            "screenshot": str(file.with_suffix('.bmp')) if file.with_suffix('.bmp').exists() else None
        }
        saves.append(save_info)

    return saves
