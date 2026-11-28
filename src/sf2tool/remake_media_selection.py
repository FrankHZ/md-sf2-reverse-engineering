"""Resolve a single reviewed media selection relative to the configured private input root."""

import json
import os
import re
from pathlib import Path

from sf2tool.paths import REPO_ROOT

OPENING_INPUTS = (
    "SF2_PRIVATE_BATTLE01_DATA",
    "SF2_PRIVATE_BATTLE01_SCENE",
    "SF2_PRIVATE_BATTLE01_TERRAIN",
    "SF2_PRIVATE_STATIC_DATA",
    "SF2_PRIVATE_ENEMY_DATA",
    "SF2_PRIVATE_ENEMY_GOLD",
)

METADATA = REPO_ROOT / "remake/docs/content/metadata"
DEFAULT_SELECTION = METADATA / "selection.json"
ROLES = ("gameplay", "opening", "entry", "display-catalog")


def unique_keys(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f"duplicate selection field {key}")
        result[key] = value
    return result


def read_selection(path=DEFAULT_SELECTION):
    roles = json.loads(Path(path).read_bytes(), object_pairs_hook=unique_keys)
    if not isinstance(roles, dict) or set(roles) != set(ROLES):
        raise ValueError("select exactly gameplay, opening, entry and display-catalog roots")
    for relative in roles.values():
        if not isinstance(relative, str) or not re.fullmatch(
            r"[a-zA-Z0-9_-]+(?:/[a-zA-Z0-9_-]+)+", relative
        ):
            raise ValueError("each role requires a portable shared-relative directory")
        if any(Path(part).is_reserved() for part in relative.split("/")):
            raise ValueError("reserved role path")
    return roles


def shared_root(root=None):
    value = Path(root or os.environ.get("SF2_SHARED_INPUT_ROOT", ""))
    if not value.is_absolute() or not value.is_dir():
        raise ValueError("select an absolute, existing shared input root")
    return value


def package_arguments(roles, root):
    return {
        "campaign": root / roles["gameplay"],
        "entry": root / roles["entry"] / "entry.json",
        "opening": root / roles["opening"] / "opening/opening.json",
        "catalog": root / roles["display-catalog"] / "world.zh-CN.json",
    }


def debug_environment(roles, root):
    files = package_arguments(roles, root)
    campaign = files["campaign"]
    paths = {
        "SF2_PRIVATE_EXPLORATION_CONTENT": campaign / "world.json",
        "SF2_PRIVATE_BATTLE_SCENE_CONTENT": campaign / "main-scenes.json",
        "SF2_PRIVATE_ENTRY_CONTENT": files["entry"],
        "SF2_ENTRY_DISPLAY_CATALOG": files["entry"].with_name("entry.zh-CN.json"),
        "SF2_PRIVATE_OPENING_CONTENT": files["opening"],
        "SF2_DISPLAY_CATALOG": files["catalog"],
    }
    paths.update({key: campaign / "opening-inputs" / (key + ".json") for key in OPENING_INPUTS})
    return {key: str(value) for key, value in paths.items()}


def main():
    import argparse

    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--selection", type=Path, default=DEFAULT_SELECTION)
    parser.add_argument("--shared-root", type=Path)
    args = parser.parse_args()
    print(
        json.dumps(debug_environment(read_selection(args.selection), shared_root(args.shared_root)))
    )


if __name__ == "__main__":
    main()
