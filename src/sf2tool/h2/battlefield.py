from __future__ import annotations

import ast
import hashlib
import json
import re
import subprocess
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-battlefield-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/battlefield")
MANIFEST = repo_path("manifests/extractions/battlefield-static.json")
SCHEMA = repo_path("schemas/battlefield-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battlefield-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battlefield-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

REPRESENTATIVE_SYMBOLS = {
    "battlefieldengine.asm": "ConvertCoordinatesToAddress",
    "buildactionrangegrids.asm": "BuildMovementRangeGrid",
    "buildmovementarrays.asm": "BuildMovementArrays",
    "buildmovestringfunctions.asm": "BuildCancelMoveString",
    "buildtargetsarray.asm": "BuildTargetsArrayWithTeammatesOfTarget",
    "checkfortrappedchest.asm": "CheckForTrappedChest",
    "determineattackposition.asm": "DetermineAttackPosition",
    "determineattackpositionformoveorder.asm": "DetermineAttackPositionForMoveOrder",
    "getactionrange.asm": "GetAttackRange",
    "getcurrentterraintypeformoveorder.asm": "GetCurrentTerrainTypeForMoveOrder",
    "getmoveorderposition.asm": "GetMoveOrderPosition",
    "getmovestringdestination.asm": "GetMoveStringDestination",
    "getreachabletargets.asm": "GetTargetsReachableByAttack",
    "initializemovementarrays.asm": "InitializeMovementArrays",
    "populatetargetslist.asm": "PopulateTargetsListForItemUse",
    "updatemovablegrid.asm": "UpdateMovableGrid",
    "updateoccupiedterrainfunctions.asm": "UpdateOccupiedByOpponentsTerrain",
}

EQUATE_RE = re.compile(
    r"^\s*([A-Za-z_][A-Za-z0-9_]*):?\s+equ\s+(.+?)(?:\s*;.*)?$",
    re.IGNORECASE,
)


def _load_equates(*paths: Path) -> dict[str, str]:
    definitions: dict[str, str] = {}
    for path in paths:
        for line in path.read_text(encoding="utf-8").splitlines():
            match = EQUATE_RE.match(line)
            if match:
                definitions[match.group(1)] = match.group(2).strip()
    return definitions


def _evaluate_equate(name: str, definitions: dict[str, str], memo: dict[str, int]) -> int:
    if name in memo:
        return memo[name]
    if name not in definitions:
        raise ValueError(f"missing upstream equate: {name}")
    expression = re.sub(r"\$([0-9A-Fa-f]+)", r"0x\1", definitions[name])

    def evaluate(node: ast.AST) -> int:
        if isinstance(node, ast.Expression):
            return evaluate(node.body)
        if isinstance(node, ast.Constant) and isinstance(node.value, int):
            return node.value
        if isinstance(node, ast.Name):
            return _evaluate_equate(node.id, definitions, memo)
        if isinstance(node, ast.UnaryOp) and isinstance(node.op, ast.USub):
            return -evaluate(node.operand)
        if isinstance(node, ast.BinOp):
            left = evaluate(node.left)
            right = evaluate(node.right)
            if isinstance(node.op, ast.Add):
                return left + right
            if isinstance(node.op, ast.Sub):
                return left - right
            if isinstance(node.op, ast.Mult):
                return left * right
            if isinstance(node.op, ast.Div):
                return left // right
        raise ValueError(f"unsupported equate expression for {name}: {expression}")

    value = evaluate(ast.parse(expression, mode="eval"))
    memo[name] = value
    return value


def _require_ordered_fragments(path: Path, fragments: list[str]) -> None:
    source = path.read_text(encoding="utf-8")
    position = 0
    for fragment in fragments:
        found = source.find(fragment, position)
        if found < 0:
            raise ValueError(f"battlefield instruction sequence drift in {path.name}: {fragment}")
        position = found + len(fragment)


def _occupied_entry(entry: int, *, set_flag: bool) -> int:
    if entry == 0xFF or (not set_flag and entry & 0x40):
        return entry
    return entry | 0x80 if set_flag else entry & 0x7F


def _neighbor_outcome(
    *, terrain_entry: int, move_cost: int, budget: int, expansion_depth: int
) -> dict[str, int | str | bool]:
    if terrain_entry & 0x80:
        return {"status": "rejected-occupied"}
    if move_cost < 0 or move_cost > budget:
        return {"status": "rejected-unaffordable"}
    if move_cost == budget:
        total = expansion_depth + move_cost
        return {
            "status": "marked-final",
            "totalMoveCost": total & 0xFF,
            "movableGrid": (total >> 8) & 0xFF,
            "queued": False,
        }
    return {
        "status": "queued",
        "remainingBudgetBucket": (budget - move_cost) & 0x1F,
        "queued": True,
    }


def _build_core_model(disasm: Path) -> dict[str, Any]:
    definitions = _load_equates(disasm / "sf2const.asm", disasm / "sf2enums.asm")
    memo: dict[str, int] = {}

    def constant(name: str) -> int:
        return _evaluate_equate(name, definitions, memo)

    source_root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        source_root / "battlefieldengine.asm",
        [
            "mulu.w  #MAP_SIZE_MAX_TILEWIDTH,d2",
            "add.w   d1,d2",
            "adda.l  d2,a0",
        ],
    )
    _require_ordered_fragments(
        source_root / "buildmovementarrays.asm",
        [
            "tst.b   1(a3,d5.w)",
            "tst.b   -1(a3,d5.w)",
            "tst.b   -MAP_SIZE_MAX_TILEWIDTH(a3,d5.w)",
            "tst.b   MAP_SIZE_MAX_TILEWIDTH(a3,d5.w)",
            "btst    #TERRAIN_BIT_OCCUPIED,d1",
            "andi.w  #BATTLEFIELD_TERRAIN_ENTRY_MASK,d1",
            "cmp.w   d2,d0",
        ],
    )
    _require_ordered_fragments(
        source_root / "updateoccupiedterrainfunctions.asm",
        [
            "cmpi.b  #TERRAIN_OBSTRUCTED,d4",
            "btst    #TERRAIN_BIT_IMPASSABLE,d4",
            "bclr    #TERRAIN_BIT_OCCUPIED,(a0)",
            "bset    #TERRAIN_BIT_OCCUPIED,(a0)",
        ],
    )

    width = constant("MAP_SIZE_MAX_TILEWIDTH")
    height = constant("MAP_SIZE_MAX_TILEHEIGHT")
    array_bytes = constant("MAP_ARRAY_BYTESIZE")
    if array_bytes != width * height:
        raise ValueError("battlefield map-array dimension drift")
    return {
        "grid": {
            "width": width,
            "height": height,
            "bytes": array_bytes,
            "longwords": constant("MAP_ARRAY_LONGSIZE"),
            "coordinateFormula": "y * 48 + x",
            "coordinateExamples": [
                {"x": 0, "y": 0, "offset": 0},
                {"x": 47, "y": 0, "offset": 47},
                {"x": 0, "y": 47, "offset": 2256},
                {"x": 47, "y": 47, "offset": 2303},
            ],
        },
        "arrays": {
            "totalMoveCosts": constant("FF4400_LOADING_SPACE"),
            "movableGrid": constant("FF4D00_LOADING_SPACE"),
            "targets": constant("FF5600_LOADING_SPACE"),
            "battleTerrain": constant("BATTLE_TERRAIN_ARRAY"),
            "moveCostsTable": constant("MOVECOSTS_TABLE"),
        },
        "initialization": {
            "clearFillByte": 255,
            "clearBytesPerGrid": array_bytes,
            "moveBudgetMultiplier": 2,
            "terrainTypeCount": constant("TERRAIN_TYPES_COUNTER") + 1,
            "obstructedMoveCostStoredAs": 255,
        },
        "movementExpansion": {
            "stackBytes": constant("BUILD_MOVEMENT_ARRAYS_STACK_BYTESIZE"),
            "budgetBucketCount": constant("BATTLEFIELD_MOVE_BUDGET_MASK") + 1,
            "initialBucketWord": constant("BUILD_MOVEMENT_ARRAYS_STACK_INITIAL_PATTERN") >> 16,
            "processedBit": constant("BATTLEFIELD_PROCESSED_SPACE_BIT"),
            "budgetMask": constant("BATTLEFIELD_MOVE_BUDGET_MASK"),
            "terrainMask": constant("BATTLEFIELD_TERRAIN_ENTRY_MASK"),
            "neighborOrder": ["right", "left", "up", "down"],
            "rejects": ["outside-array", "occupied", "unaffordable"],
            "examples": [
                _neighbor_outcome(terrain_entry=0x80, move_cost=1, budget=4, expansion_depth=2),
                _neighbor_outcome(terrain_entry=0, move_cost=5, budget=4, expansion_depth=2),
                _neighbor_outcome(terrain_entry=0, move_cost=4, budget=4, expansion_depth=2),
                _neighbor_outcome(terrain_entry=0, move_cost=3, budget=4, expansion_depth=2),
            ],
        },
        "occupancy": {
            "alliesScanned": constant("COMBATANT_ALLIES_COUNTER") + 1,
            "enemiesScanned": constant("COMBATANT_ENEMIES_COUNTER") + 1,
            "coordinateUpperBoundExclusive": width,
            "occupiedBit": constant("TERRAIN_BIT_OCCUPIED"),
            "impassableBit": constant("TERRAIN_BIT_IMPASSABLE"),
            "obstructedByte": constant("TERRAIN_OBSTRUCTED") & 0xFF,
            "examples": [
                {"entry": 0x03, "set": True, "result": _occupied_entry(0x03, set_flag=True)},
                {"entry": 0x83, "set": False, "result": _occupied_entry(0x83, set_flag=False)},
                {"entry": 0x43, "set": False, "result": _occupied_entry(0x43, set_flag=False)},
                {"entry": 0x43, "set": True, "result": _occupied_entry(0x43, set_flag=True)},
                {"entry": 0xFF, "set": False, "result": _occupied_entry(0xFF, set_flag=False)},
            ],
        },
    }


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _resolve_upstream(upstream_path: Path) -> tuple[Path, str, dict[str, Any]]:
    upstream_path = upstream_path.resolve(strict=True)
    toolchain = load_json(TOOLCHAIN)
    commit = subprocess.run(
        ["git", "rev-parse", "HEAD"],
        cwd=upstream_path,
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    expected = toolchain["sf2disasm"]["commit"]
    if commit != expected:
        raise ValueError(f"battlefield inventory requires SF2DISASM {expected}, got {commit}")
    disasm = upstream_path / "disasm"
    if not (disasm / SOURCE_ROOT).is_dir():
        raise ValueError(f"battlefield source root is missing: {disasm / SOURCE_ROOT}")
    return disasm, commit, toolchain


def build_battlefield_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    source_root = disasm / SOURCE_ROOT
    source_paths = sorted(source_root.rglob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in source_paths]
    if {Path(row["path"]).name for row in files} != set(REPRESENTATIVE_SYMBOLS):
        raise ValueError("battlefield source file set drift")

    all_labels = {label for row in files for label in row["globalLabels"]}
    direct_calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            direct_calls[call["target"]] += call["siteCount"]

    index = load_json(RESEARCH_INDEX)
    indexed_records = sorted(
        record["id"]
        for record in index["records"]
        if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
    )
    indexed_files = sorted(
        {
            record["sourcePath"]
            for record in index["records"]
            if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
        }
    )
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(direct_calls.values()),
        "indirectCallSiteCount": sum(row["indirectCallSiteCount"] for row in files),
        "uniqueDirectTargetCount": len(direct_calls),
        "internalDirectTargetCount": sum(target in all_labels for target in direct_calls),
        "externalDirectTargetCount": sum(target not in all_labels for target in direct_calls),
        "indexedRecordCount": len(indexed_records),
        "indexedFileCount": len(indexed_files),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": toolchain["sf2disasm"]["repository"],
            "commit": commit,
        },
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "indexedRecordIds": indexed_records,
        "indexedSourcePaths": indexed_files,
        "internalDirectCallTargets": sorted(
            target for target in direct_calls if target in all_labels
        ),
        "externalDirectCallTargets": sorted(
            target for target in direct_calls if target not in all_labels
        ),
        "coreModel": _build_core_model(disasm),
        "files": files,
    }


def verify_battlefield_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    rom_manifest = load_json(ROM_MANIFEST)
    output = build_battlefield_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battlefield static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != rom_manifest["hashes"]["sha256"]
    ):
        raise ValueError("battlefield fixture provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError(
            f"battlefield static summary drift: expected {manifest['summary']}, "
            f"got {output['summary']}"
        )
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(f"battlefield representative symbol drift: {filename}::{symbol}")
    if output["coreModel"] != fixture["expected"]["coreModel"]:
        raise ValueError("battlefield core array model drift")
    if output["coreModel"]["arrays"] != fixture["ram"]:
        raise ValueError("battlefield RAM address binding drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError(
            f"battlefield static inventory hash mismatch: expected "
            f"{manifest['outputSha256']}, got {digest}"
        )
    destination = output_path or repo_path("local/derived/battlefield-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "GlobalLabels": output["summary"]["globalLabelCount"],
        "DirectCallSites": output["summary"]["directCallSiteCount"],
        "IndexedRecords": output["summary"]["indexedRecordCount"],
        "Status": "PASS",
    }
