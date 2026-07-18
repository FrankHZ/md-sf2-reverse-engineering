from __future__ import annotations

import hashlib
import json
import re
import subprocess
from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.rom import inspect_rom

FIXTURE = repo_path("tests/fixtures/h2/enemy-gold-v1.json")
SCHEMA = repo_path("schemas/enemy-gold-data.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-enemy-gold-fixture.schema.json")
MANIFEST = repo_path("manifests/extractions/enemy-gold-data.json")
SOURCE_PATH = Path("data/stats/enemies/enemygold.asm")
WORD_PATTERN = re.compile(r"\bdc\.w\s+(\d+)")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _parse_source(disasm: Path) -> tuple[list[int], list[int]]:
    source = (disasm / SOURCE_PATH).read_text(encoding="utf-8")
    parts = source.split("; unused", 1)
    if len(parts) != 2 or "table_EnemyGold:" not in parts[0]:
        raise ValueError("enemy gold source no longer has one explicit unused boundary")
    used = [int(value) for value in WORD_PATTERN.findall(parts[0])]
    unused = [int(value) for value in WORD_PATTERN.findall(parts[1])]
    return used, unused


def _read_rom_words(rom_path: Path, start: int, end: int) -> list[int]:
    data = rom_path.read_bytes()[start:end]
    if len(data) != end - start or len(data) % 2:
        raise ValueError("enemy gold ROM range is truncated or odd-sized")
    return [int.from_bytes(data[index : index + 2], "big") for index in range(0, len(data), 2)]


def verify_enemy_gold(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    upstream_path = upstream_path.resolve(strict=True)
    rom_path = rom_path.resolve(strict=True)
    rom_identity = inspect_rom(rom_path)
    if rom_identity["sha256"] != fixture["romSha256"]:
        raise ValueError("enemy gold fixture ROM identity mismatch")
    commit = subprocess.run(
        ["git", "rev-parse", "HEAD"],
        cwd=upstream_path,
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    if commit != fixture["upstreamCommit"]:
        raise ValueError("enemy gold upstream identity mismatch")
    disasm = upstream_path / "disasm"

    used, unused = _parse_source(disasm)
    addresses = fixture["function"]
    rom_words = _read_rom_words(rom_path, addresses["tableAddress"], addresses["endAddress"])
    source_words = used + unused
    if rom_words != source_words:
        mismatch = next(
            index
            for index, (source, rom) in enumerate(zip(source_words, rom_words, strict=True))
            if source != rom
        )
        raise ValueError(
            f"enemy gold source-ROM parity mismatch at word {mismatch}: "
            f"source={source_words[mismatch]}, ROM={rom_words[mismatch]}"
        )

    facts = {
        "wordCount": len(source_words),
        "usedCount": len(used),
        "unusedCount": len(unused),
        "unusedNonzeroCount": sum(value != 0 for value in unused),
        "maximumUsedGold": max(used),
        "maximumUsedGoldIndex": used.index(max(used)),
        "zeroUsedCount": sum(value == 0 for value in used),
        "finalUnusedWord": unused[-1],
    }
    if facts != fixture["expected"]:
        raise ValueError("enemy gold table shape disagrees with fixture")
    if addresses["usedEndAddress"] != addresses["tableAddress"] + len(used) * 2:
        raise ValueError("enemy gold used-range boundary drift")
    if addresses["endAddress"] != addresses["tableAddress"] + len(source_words) * 2:
        raise ValueError("enemy gold full-range boundary drift")

    output = {
        "schemaVersion": 1,
        "id": fixture["id"],
        "upstreamCommit": commit,
        "romSha256": rom_identity["sha256"],
        "sourcePath": SOURCE_PATH.as_posix(),
        "romRange": {
            "start": addresses["tableAddress"],
            "usedEndExclusive": addresses["usedEndAddress"],
            "endExclusive": addresses["endAddress"],
            "wordCount": len(source_words),
        },
        "usedGold": used,
        "unusedWords": unused,
    }
    validate_json(output, SCHEMA, owner="enemy gold extraction")
    encoded = _canonical_bytes(output)
    if encoded != _canonical_bytes(output):
        raise AssertionError("enemy gold canonical serializer is not deterministic")
    digest = hashlib.sha256(encoded).hexdigest().upper()
    if manifest["outputSha256"] != "PENDING" and digest != manifest["outputSha256"]:
        raise ValueError(
            "enemy gold extraction hash mismatch: "
            f"expected {manifest['outputSha256']}, got {digest}"
        )
    destination = output_path or repo_path(manifest["outputPath"])
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(encoded)
    return {
        "Fixture": fixture["id"],
        "Output": display_path(destination),
        "SHA256": digest,
        "UsedEntries": len(used),
        "UnusedWords": len(unused),
        "SourceRomMismatches": 0,
        "Status": "PASS",
    }
