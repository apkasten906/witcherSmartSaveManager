# utils.py
import datetime
from typing import Optional, Union


def format_iso(timestamp: Union[float, int, str]) -> Optional[str]:
    """Safely convert a timestamp to ISO 8601 string (without microseconds).
    
    Returns None if conversion fails.
    """
    try:
        numeric_timestamp = float(timestamp)
        return datetime.datetime.fromtimestamp(numeric_timestamp).isoformat(timespec="seconds")
    except (ValueError, TypeError, OSError) as error:
        # Optional: log the error or handle as needed
        print(f"[format_iso] Error formatting timestamp {timestamp}: {error}")
        return None
