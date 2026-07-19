from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_animations import _listing_address
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import read_upstream_text

ID = "sf2-remaining-core-static-v1"
SOURCE_PATHS = (
    Path("code/romheader.asm"),
    Path("code/common/windows/windowengine.asm"),
    Path("code/gameflow/special/battletest.asm"),
    Path("code/gameflow/special/configurationmode.asm"),
    Path("code/gameflow/special/debugmodebattleactions.asm"),
)
MANIFEST = repo_path("manifests/extractions/remaining-core-static.json")
SCHEMA = repo_path("schemas/remaining-core-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/remaining-core-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-remaining-core-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _require_fragments(source: str, fragments: tuple[str, ...], owner: str) -> None:
    missing = [fragment for fragment in fragments if fragment not in source]
    if missing:
        raise ValueError(f"{owner} source-shape drift: missing {missing}")


def _header_facts(source: str) -> dict[str, Any]:
    vectors = source.split("aSegaGenesis:", 1)[0]
    vector_count = len(
        re.findall(r"^\s*(?:[A-Za-z_][A-Za-z0-9_]*:\s*)?dc\.l\s+", vectors, re.MULTILINE)
    )
    if vector_count != 64:
        raise ValueError("ROM vector table count drift")
    _require_fragments(
        source,
        (
            "dc.l HInt",
            "dc.l VInt",
            "dc.l Trap0_SoundCommand",
            "dc.l Trap9_ManageContextualFunctions",
            "dc.b 'GM MK-1315 -00'",
            "dc.w $8921",
            "dc.l $1FFFFF",
            "dc.l $200001",
            "dc.l $203FFF",
            "dc.b 'U               '",
        ),
        "ROM header",
    )
    return {
        "vectorEntryCount": vector_count,
        "horizontalInterruptLevel": 4,
        "verticalInterruptLevel": 6,
        "namedTrapRange": [0, 9],
        "productCode": "GM MK-1315 -00",
        "headerChecksum": 35105,
        "romEndAddress": 2097151,
        "sramStartAddress": 2097153,
        "sramEndAddress": 2113535,
        "regionCode": "U",
    }


def _window_facts(source: str) -> dict[str, Any]:
    _require_fragments(
        source,
        (
            "moveq   #WINDOW_ENTRIES_COUNTER,d7",
            "adda.w  #WINDOW_ENTRY_SIZE,a0",
            "moveq   #-1,d0",
            "moveq   #7,d7",
            "SPECIAL_TURBO_TOGGLE",
            "moveq   #1,d2",
            "MOVING_WINDOWS_BITFIELD",
            "VInt_UpdateWindows:",
            "GetWindowTileAddress:",
        ),
        "window engine",
    )
    return {
        "windowSlotCount": 8,
        "windowEntrySizeBytes": 16,
        "createFailureSentinel": -1,
        "specialTurboAnimationLength": 1,
        "movementUsesPerSlotBitfield": True,
        "movementUsesLinearIntegerInterpolation": True,
        "vintOwnsWindowAnimationAndDma": True,
        "windowTileAddressUsesPackedCoordinates": True,
    }


def _debug_facts(sources: dict[str, str]) -> dict[str, Any]:
    battle = sources["code/gameflow/special/battletest.asm"]
    config = sources["code/gameflow/special/configurationmode.asm"]
    actions = sources["code/gameflow/special/debugmodebattleactions.asm"]
    joined_allies = re.findall(r"moveq\s+#(ALLY_[A-Z0-9_]+),d0\s+bsr\.w\s+j_JoinForce", battle)
    if len(joined_allies) != 29:
        raise ValueError("debug battle-test force roster drift")
    action_table = re.search(
        r"rjt_DebugModeBattleactions:(?P<body>.*?)(?=^@Attack:)", actions, re.MULTILINE | re.DOTALL
    )
    if not action_table:
        raise ValueError("debug battle-action table is missing")
    action_targets = re.findall(r"dc\.w\s+@([A-Za-z0-9_]+)-", action_table.group("body"))
    _require_fragments(
        battle,
        (
            "move.w  #99,d1",
            "#BATTLES_DEBUG_MAX_INDEX",
            "#SHOPS_DEBUG_MAX_INDEX",
            "LevelUpWholeForce:",
        ),
        "battle test",
    )
    _require_fragments(
        config,
        (
            "SPECIAL_TURBO_TOGGLE",
            "CONTROL_OPPONENT_TOGGLE",
            "AUTO_BATTLE_TOGGLE",
            "bset    #7,(SAVE_FLAGS).l",
            "bclr    #7,(SAVE_FLAGS).l",
            "j_SoundTest",
        ),
        "configuration mode",
    )
    _require_fragments(
        actions,
        (
            "seq     debugDodge(a2)",
            "seq     debugCritical(a2)",
            "seq     debugDouble(a2)",
            "seq     debugCounter(a2)",
        ),
        "debug hit selection",
    )
    return {
        "battleTestJoinedAllyCount": len(joined_allies),
        "battleTestWholeForceCount": 30,
        "battleTestMaxBattleIndex": 49,
        "battleTestMaxShopIndex": 100,
        "battleTestBowieStatValue": 99,
        "configurationToggleCount": 4,
        "configurationToggles": [
            "special-turbo",
            "control-opponent",
            "auto-battle",
            "game-completed",
        ],
        "soundTestRequiresStartUpAndCompletedFlag": True,
        "debugBattleActionTargets": action_targets,
        "debugBattleActionCount": len(action_targets),
        "debugHitOverrideCount": 4,
        "debugHitOverrides": ["dodge", "critical", "double", "counter"],
    }


def build_remaining_core_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"remaining core H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = [disasm / path for path in SOURCE_PATHS]
    if not all(path.is_file() for path in paths):
        raise ValueError("remaining core source boundary is incomplete")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    for row in files:
        if row["path"].replace("/", "\\") not in layout:
            raise ValueError(f"remaining core source is absent from layout: {row['path']}")
        if not row["globalLabels"]:
            raise ValueError(f"unexpected unlabeled remaining core file: {row['path']}")
    representative_symbols = {row["path"]: row["globalLabels"][0] for row in files}
    representative_addresses = {
        symbol: _listing_address(listing, symbol) for symbol in representative_symbols.values()
    }
    source_paths = {path.as_posix() for path in SOURCE_PATHS}
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if record["sourcePath"] in source_paths
    ]
    labels = {label for row in files for label in row["globalLabels"]}
    calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    sources = {path.as_posix(): read_upstream_text(disasm / path) for path in SOURCE_PATHS}
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(calls.values()),
        "uniqueDirectTargetCount": len(calls),
        "internalDirectTargetCount": sum(target in labels for target in calls),
        "externalDirectTargetCount": sum(target not in labels for target in calls),
        "layoutIncludedFileCount": len(files),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({record["sourcePath"] for record in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scopes": [path.as_posix() for path in SOURCE_PATHS],
        "summary": summary,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "headerFacts": _header_facts(sources["code/romheader.asm"]),
        "windowFacts": _window_facts(sources["code/common/windows/windowengine.asm"]),
        "debugFacts": _debug_facts(sources),
        "runtimeQuestions": [
            "window-animation-hide-scroll-and-dma-frames",
            "debug-configuration-input-and-menu-presentation",
        ],
        "files": files,
    }


def verify_remaining_core_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_remaining_core_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="remaining core static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("remaining core provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("remaining core summary drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("remaining core H1 address drift")
    for field in ("headerFacts", "windowFacts", "debugFacts", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"remaining core {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("remaining core canonical hash drift")
    destination = output_path or repo_path("local/derived/remaining-core-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "WindowSlots": output["windowFacts"]["windowSlotCount"],
        "DebugActions": output["debugFacts"]["debugBattleActionCount"],
        "RuntimeQuestions": len(output["runtimeQuestions"]),
        "Status": "PASS",
    }
