from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_global_data import _statements
from sf2tool.h2.battle_scene_animations import _listing_address
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import read_upstream_text

ID = "sf2-battle-cutscene-data-static-v1"
SOURCE_ROOT = Path("data/battles/entries")
STORAGE_PATH = SOURCE_ROOT / "battlecutscenesstorage.asm"
ORPHAN_PATH = SOURCE_ROOT / "battle01/cs_regiontriggered_1.asm"
MANIFEST = repo_path("manifests/extractions/battle-cutscene-data-static.json")
SCHEMA = repo_path("schemas/battle-cutscene-data-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-cutscene-data-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-cutscene-data-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _cutscene_type(path: str) -> str:
    name = Path(path).stem
    prefixes = {
        "cs_beforebattle": "before-battle",
        "cs_afterbattle": "after-battle",
        "cs_battleend": "battle-end",
        "cs_battlestart": "battle-start",
        "cs_regiontriggered": "region-triggered",
    }
    for prefix, cutscene_type in prefixes.items():
        if name.startswith(prefix):
            return cutscene_type
    raise ValueError(f"unknown battle cutscene filename shape: {path}")


def _command_facts(sources: dict[str, str], built_paths: list[str]) -> dict[str, Any]:
    commands: Counter[str] = Counter()
    for path in built_paths:
        for statement in _statements(sources[path]):
            command = statement.split(None, 1)[0]
            if command.endswith(":"):
                continue
            commands[command] += 1
    return {
        "statementCount": sum(commands.values()),
        "uniqueCommandCount": len(commands),
        "endCommandCount": commands["csc_end"],
        "mostFrequentCommands": [
            {"command": command, "count": count}
            for command, count in sorted(commands.items(), key=lambda item: (-item[1], item[0]))[:8]
        ],
    }


def build_battle_cutscene_data_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle-cutscene-data H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    root = disasm / SOURCE_ROOT
    paths = sorted(root.rglob("*.asm"))
    if len(paths) != 61:
        raise ValueError(
            f"battle cutscene data boundary drift: expected 61 files, got {len(paths)}"
        )
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    rows_by_path = {row["path"]: row for row in files}

    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    if STORAGE_PATH.as_posix().replace("/", "\\") not in layout:
        raise ValueError("battle cutscene storage is absent from the original layout")
    storage_source = read_upstream_text(disasm / STORAGE_PATH)
    built_paths = sorted(
        match.replace("\\", "/")
        for match in re.findall(r'^\s*include\s+"([^"]+)"', storage_source, re.MULTILINE)
    )
    if len(built_paths) != 59 or len(set(built_paths)) != len(built_paths):
        raise ValueError("battle cutscene storage include boundary drift")
    if set(built_paths) - set(rows_by_path):
        raise ValueError("battle cutscene storage references a source outside the inventory")
    excluded_paths = sorted(set(rows_by_path) - set(built_paths))
    expected_excluded = sorted([STORAGE_PATH.as_posix(), ORPHAN_PATH.as_posix()])
    if excluded_paths != expected_excluded:
        raise ValueError(
            "battle cutscene data exclusion drift: "
            f"expected {expected_excluded}, got {excluded_paths}"
        )
    if rows_by_path[STORAGE_PATH.as_posix()]["globalLabels"]:
        raise ValueError("battle cutscene storage unexpectedly gained a symbol")
    if rows_by_path[ORPHAN_PATH.as_posix()]["globalLabels"] != ["rbcs_battle01"]:
        raise ValueError("orphan Battle 01 region cutscene shape drift")
    if any(not rows_by_path[path]["globalLabels"] for path in built_paths):
        raise ValueError("a built battle cutscene is unexpectedly unlabeled")

    representative_symbols = {
        path: rows_by_path[path]["globalLabels"][0] for path in built_paths
    }
    representative_addresses = {
        symbol: _listing_address(listing, symbol) for symbol in representative_symbols.values()
    }
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if record["sourcePath"].startswith(f"{SOURCE_ROOT.as_posix()}/")
    ]
    sources = {
        path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths
    }
    type_counts = Counter(_cutscene_type(path) for path in built_paths)
    battle_indexes = {
        int(match.group(1))
        for path in built_paths
        if (match := re.search(r"/battle(\d{2})/", path))
    }
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "storageFileCount": 1,
        "builtCutsceneFileCount": len(built_paths),
        "orphanCutsceneFileCount": 1,
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
        "storagePath": STORAGE_PATH.as_posix(),
        "builtCutscenePaths": built_paths,
        "excludedPaths": excluded_paths,
        "exclusions": {
            STORAGE_PATH.as_posix(): (
                "layout-owned include container with no label or emitted bytes"
            ),
            ORPHAN_PATH.as_posix(): "labeled region cutscene not referenced by the original layout",
        },
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "facts": {
            "builtBattleCount": len(battle_indexes),
            "builtBattleIndexes": sorted(battle_indexes),
            "cutsceneTypeCounts": dict(sorted(type_counts.items())),
            "commands": _command_facts(sources, built_paths),
            "storyContentParsed": False,
            "existingRoutingOwner": {
                "verifier": "src/sf2tool/h2/battle_cutscenes.py",
                "document": "docs/research/battle-cutscenes.md",
            },
        },
        "runtimeQuestions": [
            "battle01-orphan-region-cutscene-provenance-and-reachability",
            "cutscene-command-presentation-and-timing",
            "story-side-effects-across-map-entity-and-battle-state",
        ],
        "files": files,
    }


def verify_battle_cutscene_data_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_cutscene_data_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle cutscene data static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("battle cutscene data provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle cutscene data summary drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("battle cutscene data H1 address drift")
    for field in ("facts", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"battle cutscene data {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("battle cutscene data canonical hash drift")
    destination = output_path or repo_path("local/derived/battle-cutscene-data-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "BuiltCutscenes": output["summary"]["builtCutsceneFileCount"],
        "ExcludedFiles": len(output["excludedPaths"]),
        "IndexedFiles": output["summary"]["indexedFileCount"],
        "Status": "PASS",
    }
