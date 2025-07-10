import tomllib
from pathlib import Path

CONFIG_FILE = Path(__file__).parent / "settings.toml"

with open(CONFIG_FILE, "rb") as f:
    config = tomllib.load(f)
