"""Extend a prepared private entry bank using the accepted Witch Continue rows."""

from __future__ import annotations

import argparse
import json
import re
import shutil
import subprocess
from pathlib import Path

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path
from sf2tool.remake_localization import _jsonl, _topology

CONTINUE_IDS = (224, 225, 226)
GREETING_IDS = tuple(range(216, 222))


def prepare(upstream: Path, corpus: Path, entry: Path, catalog: Path, output: Path) -> None:
    if not output.is_absolute() or output.exists() or output.is_relative_to(entry.parent):
        raise ValueError("select a fresh absolute output outside the input entry bank")
    disasm, commit, _ = _resolve_upstream(upstream)
    script = subprocess.run(
        ["git", "show", f"{commit}:disasm/code/specialscreens/witch/witchstart.asm"],
        cwd=disasm.parent,
        capture_output=True,
        check=True,
        text=True,
        encoding="utf-8",
    ).stdout
    source: dict[int, str] = {}
    for raw_id, message in re.findall(r'txt\s+(22[456])\s*;\s*"([^"]+)"', script):
        identity = int(raw_id)
        if source.setdefault(identity, message) != message:
            raise ValueError("conflicting Witch text binding")
    if set(source) != set(CONTINUE_IDS):
        raise ValueError("missing Witch text binding")

    bank, overlay = load_json(entry), load_json(catalog)
    rom = load_json(repo_path("manifests/roms/sf2-us.json"))["hashes"]["sha256"]
    if (
        bank["formatVersion"] != 1
        or bank["profile"] != "private-local-entry"
        or bank["upstreamCommit"] != commit
        or bank["romSha256"] != rom
        or set(bank["dialogue"]) != set(map(str, GREETING_IDS))
        or overlay["locale"] != "zh-CN"
        or overlay["package"] != bank["profile"]
        or overlay["source"]
        != {
            "repository": "ShiningForceCentral/SF2DISASM",
            "commit": commit,
            "romSha256": rom,
        }
        or [(row["namespace"], row["id"]) for row in overlay["texts"]]
        != [("entry.text", identity) for identity in GREETING_IDS]
    ):
        raise ValueError("entry predecessor identity or greeting rows drifted")

    rows = _jsonl(corpus / "source.jsonl", lambda row: (row["namespace"], row["id"]))
    for identity in CONTINUE_IDS:
        row = rows["world.text", identity]
        original, translation = row["source"], row["translation"]
        if source[identity] != original or not translation or not _topology(original, translation):
            raise ValueError(f"Witch source or translation control drift: {identity}")
        bank["dialogue"][str(identity)] = display_line(original)
        overlay["texts"].append(
            {
                "namespace": "entry.text",
                "id": identity,
                "text": display_line(translation),
            }
        )

    resources = bank["resources"] + bank.get("hdResources", [])
    images = [row["file"] for row in resources]
    if any(not re.fullmatch(r"[a-z-]+(?:\.hd)?\.png", name) for name in images):
        raise ValueError("unexpected entry resource filename")
    output.mkdir(parents=True, exist_ok=False)
    for name in images:
        shutil.copyfile(entry.parent / name, output / name)
    for name, content in [("entry.json", bank), ("entry.zh-CN.json", overlay)]:
        (output / name).write_text(
            json.dumps(content, ensure_ascii=False, indent=2) + "\n",
            encoding="utf-8",
        )


def display_line(message: str) -> str:
    # The host acknowledges each message; keep the name operand for display binding.
    return message.removesuffix("{W1}").removesuffix("{W2}").replace("{N}", "\n")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("upstream", "corpus", "entry", "catalog", "output"):
        parser.add_argument(f"--{name}", required=True, type=Path)
    args = parser.parse_args()
    prepare(
        args.upstream,
        args.corpus,
        args.entry.resolve(strict=True),
        args.catalog,
        args.output.resolve(),
    )


if __name__ == "__main__":
    main()
