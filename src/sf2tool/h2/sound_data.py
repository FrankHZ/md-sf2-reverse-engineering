from __future__ import annotations

import hashlib
import json
import posixpath
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import read_upstream_text

ID = "sf2-sound-data-static-v1"
SOURCE_ROOT = Path("data/sound")
BANK_SOURCES = {
    "bank0": SOURCE_ROOT / "musicbank0/musicbank0.asm",
    "bank1": SOURCE_ROOT / "musicbank1/musicbank1.asm",
}
BANK_OUTPUTS = {
    "bank0": SOURCE_ROOT / "musicbank0.bin",
    "bank1": SOURCE_ROOT / "musicbank1.bin",
}
BANK_ROM_OFFSETS = {"bank1": 0x1F0000, "bank0": 0x1F8000}
BANK_SIZE = 0x8000
MANIFEST = repo_path("manifests/extractions/sound-data-static.json")
SCHEMA = repo_path("schemas/sound-data-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/sound-data-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-sound-data-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest().upper()


def _resolve_include(source_path: str, target: str) -> str:
    normalized = target.replace("\\", "/")
    return posixpath.normpath(posixpath.join(posixpath.dirname(source_path), normalized))


def build_sound_data_inventory(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    expected_rom = load_json(ROM_MANIFEST)
    rom = rom_path.read_bytes()
    if len(rom) != expected_rom["sizeBytes"] or _sha256(rom) != expected_rom["hashes"]["sha256"]:
        raise ValueError("sound data ROM identity drift")

    root = disasm / SOURCE_ROOT
    paths = sorted(root.rglob("*.asm"))
    if len(paths) != 41:
        raise ValueError(f"sound data boundary drift: expected 41 files, got {len(paths)}")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    sources = {
        path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths
    }

    include_edges = []
    for source_path, source in sources.items():
        for target in re.findall(r'^\s*include\s+"([^"]+)"', source, re.MULTILINE):
            include_edges.append(
                {
                    "sourcePath": source_path,
                    "targetPath": _resolve_include(source_path, target),
                }
            )
    targets = [edge["targetPath"] for edge in include_edges]
    if len(include_edges) != 41 or len(set(targets)) != 39:
        raise ValueError("sound data include graph boundary drift")
    if set(targets) - set(sources):
        raise ValueError("sound bank includes a source outside data/sound")
    entry_paths = sorted(path.as_posix() for path in BANK_SOURCES.values())
    reachable = set(entry_paths)
    outgoing: dict[str, list[str]] = {}
    for edge in include_edges:
        outgoing.setdefault(edge["sourcePath"], []).append(edge["targetPath"])
    queue = list(entry_paths)
    while queue:
        for target in outgoing.get(queue.pop(), []):
            if target not in reachable:
                reachable.add(target)
                queue.append(target)
    if reachable != set(sources):
        raise ValueError("sound bank entry points do not reach the complete source directory")

    bank_facts: dict[str, dict[str, Any]] = {}
    for bank, source_path in BANK_SOURCES.items():
        output_path = disasm / BANK_OUTPUTS[bank]
        if not output_path.is_file():
            raise ValueError(f"generated {bank} binary is missing: {output_path}")
        payload = output_path.read_bytes()
        if len(payload) != BANK_SIZE:
            raise ValueError(f"generated {bank} size drift: {len(payload)}")
        offset = BANK_ROM_OFFSETS[bank]
        rom_payload = rom[offset : offset + BANK_SIZE]
        if payload != rom_payload:
            raise ValueError(f"generated {bank} does not match the canonical ROM slice")
        source = sources[source_path.as_posix()]
        pointers = re.findall(r"^\s*dw\s+(Music_\d+)\s*$", source, re.MULTILINE)
        song_includes = [
            target
            for target in re.findall(r'^\s*include\s+"([^"]+)"', source, re.MULTILINE)
            if target.lower().startswith("music") and target.lower().endswith(".asm")
        ]
        bank_facts[bank] = {
            "sourcePath": source_path.as_posix(),
            "romOffset": offset,
            "sizeBytes": len(payload),
            "sha256": _sha256(payload),
            "pointerSlotCount": len(pointers),
            "uniquePointerTargetCount": len(set(pointers)),
            "songIncludeCount": len(song_includes),
            "romParity": True,
        }

    song_paths = sorted(
        path
        for path in sources
        if re.search(r"/music\d+\.asm$", path)
    )
    if len(song_paths) != 37:
        raise ValueError("sound song-file boundary drift")
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "bankEntryFileCount": len(entry_paths),
        "sharedDefinitionFileCount": 2,
        "songFileCount": len(song_paths),
        "transitiveIncludeFileCount": len(set(targets)),
        "strictH1IndexedFileCount": 0,
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "rom": {"id": expected_rom["id"], "sha256": expected_rom["hashes"]["sha256"]},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "entryPaths": entry_paths,
        "includeEdges": include_edges,
        "songPaths": song_paths,
        "facts": {
            "assemblyCpu": "z80",
            "bankAddressSpaceOrigin": 0x8000,
            "banks": bank_facts,
            "romLayoutOrder": ["bank1", "bank0"],
            "sourceContentParsed": False,
            "musicSemanticsParsed": False,
        },
        "strictIndexExclusion": (
            "music sources are assembled in a separate Z80 address space and included into the "
            "68000 ROM as unlabeled bank binaries"
        ),
        "runtimeQuestions": [
            "music-command-and-channel-interpreter-semantics",
            "tempo-loop-and-instrument-timing",
            "bank-selection-and-cross-bank-fallback-behavior",
        ],
        "files": files,
    }


def verify_sound_data_inventory(
    rom_path: Path,
    upstream_path: Path,
    *,
    output_path: Path | None = None,
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_sound_data_inventory(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="sound data static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["rom"]["sha256"]
    ):
        raise ValueError("sound data provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("sound data summary drift")
    for field in ("facts", "strictIndexExclusion", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"sound data {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("sound data canonical hash drift")
    destination = output_path or repo_path("local/derived/sound-data-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "Songs": output["summary"]["songFileCount"],
        "Banks": len(output["facts"]["banks"]),
        "RomParity": all(bank["romParity"] for bank in output["facts"]["banks"].values()),
        "Status": "PASS",
    }
