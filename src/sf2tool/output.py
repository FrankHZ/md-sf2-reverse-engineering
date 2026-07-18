from __future__ import annotations

import json
from collections.abc import Mapping
from typing import Any


def print_record(record: Mapping[str, Any]) -> None:
    width = max(len(key) for key in record)
    for key, value in record.items():
        if isinstance(value, list):
            value = ", ".join(str(item) for item in value)
        print(f"{key:<{width}} : {value}")


def print_json(value: Any) -> None:
    print(json.dumps(value, ensure_ascii=False, indent=2))
