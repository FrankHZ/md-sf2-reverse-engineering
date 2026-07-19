from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_animations import _listing_address
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import read_upstream_text

ID = "sf2-battle-spriteset-data-static-v1"
SOURCE_ROOT = Path("data/battles/spritesets")
ENTRIES_PATH = SOURCE_ROOT / "entries.asm"
MANIFEST = repo_path("manifests/extractions/battle-spriteset-data-static.json")
SCHEMA = repo_path("schemas/battle-spriteset-data-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-spriteset-data-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-spriteset-data-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _header_counts(source: str, path: str) -> dict[str, int]:
    values = [
        match.group(1).strip()
        for match in re.finditer(r"^\s*dc\.b\s+([^;\r\n]+)", source, re.MULTILINE)
    ][:4]
    if len(values) != 4 or any(not value.isdecimal() for value in values):
        raise ValueError(f"battle spriteset header drift: {path}")
    return dict(
        zip(
            ("allies", "enemies", "aiRegions", "aiPoints"),
            map(int, values),
            strict=True,
        )
    )


def build_battle_spriteset_data_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle-spriteset-data H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    root = disasm / SOURCE_ROOT
    paths = sorted(root.glob("*.asm"))
    if len(paths) != 46:
        raise ValueError(f"battle spriteset boundary drift: expected 46 files, got {len(paths)}")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    rows_by_path = {row["path"]: row for row in files}

    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    if ENTRIES_PATH.as_posix().replace("/", "\\") not in layout:
        raise ValueError("battle spriteset entries table is absent from the original layout")
    entries_source = read_upstream_text(disasm / ENTRIES_PATH)
    nested_paths = [
        match.replace("\\", "/")
        for match in re.findall(r'^\s*include\s+"([^"]+)"', entries_source, re.MULTILINE)
    ]
    expected_nested = [f"{SOURCE_ROOT.as_posix()}/spriteset{index:02d}.asm" for index in range(45)]
    if nested_paths != expected_nested:
        raise ValueError("battle spriteset include order or boundary drift")
    pointer_symbols = re.findall(
        r"^\s*dc\.l\s+(BattleSpriteset\d{2})\b", entries_source, re.MULTILINE
    )
    expected_symbols = [f"BattleSpriteset{index:02d}" for index in range(45)]
    if pointer_symbols != expected_symbols:
        raise ValueError("battle spriteset pointer order or boundary drift")

    representative_symbols = {ENTRIES_PATH.as_posix(): "pt_BattleSpritesets"}
    for path, symbol in zip(nested_paths, expected_symbols, strict=True):
        if rows_by_path[path]["globalLabels"] != [symbol]:
            raise ValueError(f"battle spriteset label drift: {path}")
        representative_symbols[path] = symbol
    representative_addresses = {
        symbol: _listing_address(listing, symbol) for symbol in representative_symbols.values()
    }
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if record["sourcePath"].startswith(f"{SOURCE_ROOT.as_posix()}/")
    ]
    sources = {path: read_upstream_text(disasm / path) for path in nested_paths}
    headers = [_header_counts(sources[path], path) for path in nested_paths]
    totals = {field: sum(row[field] for row in headers) for field in headers[0]}
    ranges = {
        field: {
            "minimum": min(row[field] for row in headers),
            "maximum": max(row[field] for row in headers),
        }
        for field in headers[0]
    }
    macro_counts = {
        macro: sum(
            len(re.findall(rf"^\s*{macro}\b", source, re.MULTILINE))
            for source in sources.values()
        )
        for macro in ("allyCombatant", "enemyCombatant", "combatantAiAndItem", "combatantBehavior")
    }
    if macro_counts["allyCombatant"] != totals["allies"]:
        raise ValueError("battle spriteset ally header/macro count mismatch")
    if macro_counts["enemyCombatant"] != totals["enemies"]:
        raise ValueError("battle spriteset enemy header/macro count mismatch")
    if macro_counts["combatantAiAndItem"] != totals["allies"] + totals["enemies"]:
        raise ValueError("battle spriteset AI/item macro count mismatch")
    if macro_counts["combatantBehavior"] != totals["allies"] + totals["enemies"]:
        raise ValueError("battle spriteset behavior macro count mismatch")

    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "directLayoutFileCount": 1,
        "nestedSpritesetFileCount": len(nested_paths),
        "representativeAddressCount": len(representative_addresses),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({record["sourcePath"] for record in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "entriesPath": ENTRIES_PATH.as_posix(),
        "nestedSpritesetPaths": nested_paths,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "facts": {
            "battleSlotCount": len(pointer_symbols),
            "battleSlotRange": {"first": 0, "last": len(pointer_symbols) - 1},
            "headerTotals": totals,
            "headerRanges": ranges,
            "combatantMacroCounts": macro_counts,
            "rowContentTracked": False,
            "existingBattle01Owner": {
                "verifier": "src/sf2tool/h2/battle01.py",
                "document": "docs/research/battle01-placement.md",
            },
        },
        "runtimeQuestions": [
            "non-battle01-roster-placement-and-ai-region-integration",
            "battle-slot-selection-across-map-and-story-routing",
            "hidden-delayed-spawn-and-follow-target-state-transitions",
        ],
        "files": files,
    }


def verify_battle_spriteset_data_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_spriteset_data_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle spriteset data static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("battle spriteset data provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle spriteset data summary drift")
    if output["representativeAddresses"] != fixture["table"]:
        raise ValueError("battle spriteset data H1 address drift")
    for field in ("facts", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"battle spriteset data {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("battle spriteset data canonical hash drift")
    destination = output_path or repo_path("local/derived/battle-spriteset-data-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "BattleSlots": output["facts"]["battleSlotCount"],
        "IndexedFiles": output["summary"]["indexedFileCount"],
        "Status": "PASS",
    }
