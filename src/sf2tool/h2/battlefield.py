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


def _parse_spell_range_rings(path: Path) -> list[dict[str, Any]]:
    label_pattern = re.compile(r"^SpellRange(\d+):\s+dc\.b\s+(\d+)")
    pair_pattern = re.compile(r"^\s*dc\.b\s+(-?\d+)\s*,\s*(-?\d+)")
    rings: list[dict[str, Any]] = []
    current: dict[str, Any] | None = None
    for line in path.read_text(encoding="utf-8").splitlines():
        label = label_pattern.match(line)
        if label:
            current = {
                "radius": int(label.group(1)),
                "declaredCount": int(label.group(2)),
                "coordinates": [],
            }
            rings.append(current)
            continue
        pair = pair_pattern.match(line)
        if pair and current is not None:
            current["coordinates"].append([int(pair.group(1)), int(pair.group(2))])
    if [ring["radius"] for ring in rings] != [0, 1, 2, 3]:
        raise ValueError("spell range ring set drift")
    for ring in rings:
        coordinates = ring["coordinates"]
        if ring["declaredCount"] != len(coordinates):
            raise ValueError(f"spell range {ring['radius']} count drift")
        if any(abs(x) + abs(y) != ring["radius"] for x, y in coordinates):
            raise ValueError(f"spell range {ring['radius']} geometry drift")
    return rings


def _targeting_facts(targeting: dict[str, Any]) -> dict[str, Any]:
    rings = targeting["spellRangeRings"]
    coordinate_hash = (
        hashlib.sha256(json.dumps(rings, separators=(",", ":")).encode("utf-8")).hexdigest().upper()
    )
    return {
        "spellRangeCounts": [ring["declaredCount"] for ring in rings],
        "spellRangeCoordinateSha256": coordinate_hash,
        "unarmedAttackRanges": targeting["unarmedAttackRanges"],
        "spellTargetSide": targeting["spellTargetSide"],
        "targetGridAdmission": targeting["targetGridAdmission"],
        "areaEffects": targeting["areaEffects"],
        "reachableTargets": targeting["reachableTargets"],
    }


def _annulus_offsets(maximum: int, minimum: int) -> list[list[int]]:
    return [
        [x, y]
        for y in range(-maximum, maximum + 1)
        for x in range(-(maximum - abs(y)), maximum - abs(y) + 1)
        if abs(x) + abs(y) >= minimum
    ]


def _replay_move_string(x: int, y: int, directions: list[int]) -> dict[str, int]:
    deltas = {0: (1, 0), 1: (0, -1), 2: (-1, 0), 3: (0, 1)}
    for direction in directions:
        if direction not in deltas:
            break
        dx, dy = deltas[direction]
        x += dx
        y += dy
    return {"x": x, "y": y}


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
    _require_ordered_fragments(
        source_root / "buildactionrangegrids.asm",
        [
            "bsr.w   ClearTargetsArray",
            "bsr.w   ClearTotalMoveCostsAndMovableGridArrays",
            "btst    #SPELLPROPS_BIT_TARGETING,SPELLDEF_OFFSET_PROPS(a0)",
            "movea.l -(a1),a0",
            "bsr.w   ApplyRelativeCoordinatesListToGrid",
        ],
    )
    _require_ordered_fragments(
        source_root / "buildtargetsarray.asm",
        [
            "jsr     GetCurrentHp",
            "jsr     GetActivationBitfield",
            "btst    #AIBITFIELD_BIT_NEUTRAL,d1",
            "jsr     GetCombatantY",
            "jsr     GetCombatantX",
            "move.b  d0,(a0)",
        ],
    )
    _require_ordered_fragments(
        source_root / "determineattackposition.asm",
        [
            "move.b  #-1,candidateMoveCost(a6)",
            "neg.b   d6",
            "cmp.b   d4,d0",
            "bsr.w   GetMoveCostToDestination",
            "tst.w   d0",
            "btst    #15,d0",
            "cmp.b   candidateMoveCost(a6),d0",
            "bcc.w   @Next",
            "bsr.w   GetCombatantOccupyingSpace",
        ],
    )
    _require_ordered_fragments(
        source_root / "getmovestringdestination.asm",
        [
            "cmpi.b  #-1,d0",
            "addq.w  #1,d1",
            "subq.w  #1,d2",
            "subq.w  #1,d1",
            "addq.w  #1,d2",
        ],
    )
    _require_ordered_fragments(
        source_root / "buildmovestringfunctions.asm",
        [
            "subq.w  #1,d4",
            "addq.w  #1,d2",
            "subq.w  #1,d2",
            "subi.w  #48,d2",
            "addi.w  #48,d2",
            "eori.b  #2,d1",
            "eori.b  #2,d0",
            "sub.w   d3,d6",
        ],
    )
    _require_ordered_fragments(
        source_root / "getmoveorderposition.asm",
        [
            "btst    #AIORDER_BIT_MOVE_TO,d0",
            "jsr     j_GetCombatantY",
            "jsr     j_GetCombatantX",
            "moveq   #BATTLESPRITESET_SUBSECTION_AI_POINTS,d1",
            "andi.w  #BYTE_LOWER_NIBBLE_MASK,d0",
            "add.w   d0,d0",
            "move.b  (a0),d1",
            "move.b  1(a0),d2",
        ],
    )
    _require_ordered_fragments(
        source_root / "checkfortrappedchest.asm",
        [
            "move.w  #BATTLESPRITESET_SUBSECTION_ENEMIES,d1",
            "move.w  #COMBATANT_ENEMIES_START,d0",
            "cmp.b   d1,d6",
            "cmp.b   d2,d7",
            "cmpi.w  #AIBITFIELD_HIDDEN,d1",
            "cmpi.w  #AI_TRIGGER_REGION_NONE,d1",
            "cmpi.w  #AI_TRIGGER_REGION_NONE,d2",
            "jsr     j_GetMaxHp",
            "bsr.w   ResetSpawningEnemyStats",
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
        "targeting": {
            "spellRangeRings": _parse_spell_range_rings(
                disasm / "data/stats/spells/spellranges.asm"
            ),
            "unarmedAttackRanges": {
                "default": {"minimum": 1, "maximum": 1},
                "brassGunner": {
                    "class": constant("CLASS_BRGN"),
                    "minimum": 1,
                    "maximum": 2,
                },
                "krakenArm": {
                    "enemy": constant("ENEMY_KRAKEN_ARM"),
                    "minimum": 1,
                    "maximum": 2,
                },
                "krakenHead": {
                    "enemy": constant("ENEMY_KRAKEN_HEAD"),
                    "minimum": 1,
                    "maximum": 3,
                },
            },
            "spellTargetSide": {
                "propertyBit": constant("SPELLPROPS_BIT_TARGETING"),
                "propertyClearMeaning": "opponents",
                "propertySetMeaning": "teammates",
                "matrix": [
                    {"caster": "ally", "propertySet": False, "targets": "enemies"},
                    {"caster": "ally", "propertySet": True, "targets": "allies"},
                    {"caster": "enemy", "propertySet": False, "targets": "allies"},
                    {"caster": "enemy", "propertySet": True, "targets": "enemies"},
                ],
            },
            "targetGridAdmission": {
                "requiresAlive": True,
                "excludesNeutral": True,
                "coordinateUpperBoundExclusive": width,
                "invalidEntry": 255,
                "relativeCoordinateBytes": constant("BATTLEFIELD_COORDINATES_ENTRY_SIZE"),
                "obstructedTerrainSkipsGridMarkButNotOccupantLookup": True,
            },
            "areaEffects": {
                "burstRockSpell": constant("SPELL_B_ROCK"),
                "burstRockBuildsAllCombatants": True,
                "burstRockExcludesCenterRing": True,
                "auraLevel4SpellEntry": constant("SPELL_AURA") | constant("SPELL_LV4"),
                "shineSpell": constant("SPELL_SHINE"),
                "mapWideListsRequirePositionAndAlive": True,
                "mapWideListsDoNotCheckNeutral": True,
            },
            "reachableTargets": {
                "confusionFlipsActorSideMask": constant("COMBATANT_MASK_ENEMY_BIT"),
                "normalRoster": "opponents",
                "requiresAlive": True,
                "requiresAttackPosition": True,
                "storesTotalMoveCostLowByte": True,
                "listCapacityFromRamLayout": constant("TARGETS_REACHABLE_BY_SPELL_LIST")
                - constant("TARGETS_REACHABLE_BY_ATTACK_LIST"),
            },
        },
        "pathSelection": {
            "attackPosition": {
                "candidateInitialMoveCost": 255,
                "scanOrder": "top-to-bottom then left-to-right within Manhattan annulus",
                "radius2Minimum1Offsets": _annulus_offsets(2, 1),
                "invalidCoordinates": 255,
                "standingCostZeroReturnsImmediately": True,
                "obstructedSignalWordBit": 15,
                "acceptedCostRelation": "unsigned low byte less than current best",
                "equalCostTieBreak": "first scanned",
                "requiresUnoccupiedDestination": True,
                "upstreamHigherCostCommentConflictsWithInstructionComparison": True,
                "moveOrderFallbackRanges": [[0, 0], [1, 1]],
            },
            "moveString": {
                "ramAddress": constant("BATTLE_ENTITY_MOVE_STRING"),
                "directionCodes": {"right": 0, "up": 1, "left": 2, "down": 3},
                "terminator": 255,
                "otherCodeAlsoStopsReplay": True,
                "backtrackNeighborOrder": ["right", "left", "up", "down"],
                "boundsCheckOccursAfterNeighborRead": True,
                "backtrackChoosesLowestCostAtMostCurrentMinusOne": True,
                "avoidsRepeatingPreviousBacktrackDirectionWhenAlternativeExists": True,
                "aiReversesBuiltString": True,
                "aiOppositeDirectionXor": 2,
                "partialPathStopCost": "max(destinationCost - movementBudget, 0)",
                "replayExample": {
                    "start": {"x": 2, "y": 2},
                    "directions": [0, 1, 2, 3, 255],
                    "destination": _replay_move_string(2, 2, [0, 1, 2, 3, 255]),
                },
            },
        },
        "lateHelpers": {
            "moveOrderPosition": {
                "moveToBit": constant("AIORDER_BIT_MOVE_TO"),
                "bitClearMeaning": "follow combatant index",
                "bitSetMeaning": "AI point",
                "aiPointSubsection": constant("BATTLESPRITESET_SUBSECTION_AI_POINTS"),
                "aiPointIndexMask": 15,
                "aiPointEntryBytes": 2,
            },
            "moveOrderTerrain": {
                "targetTypeArgumentUnused": True,
                "testsTerrainBit": constant("TERRAIN_BIT_OCCUPIED"),
                "clearResult": 0,
                "setResult": 255,
                "doesNotTestImpassableBitSeparately": True,
            },
            "trappedChest": {
                "enemySubsection": constant("BATTLESPRITESET_SUBSECTION_ENEMIES"),
                "firstCombatant": constant("COMBATANT_ENEMIES_START"),
                "entryBytes": constant("BATTLESPRITESET_ENTITY_ENTRY_SIZE"),
                "coordinateComparisonUsesLowBytes": True,
                "activationBitfieldMustEqual": constant("AIBITFIELD_HIDDEN"),
                "bothTriggerRegionsMustEqual": constant("AI_TRIGGER_REGION_NONE"),
                "maximumHpMustEqual": 0,
                "matchResetsSpawningEnemyStats": True,
                "noMatchResult": 65535,
            },
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
    for key, expected in fixture["expected"]["coreModel"].items():
        if output["coreModel"][key] != expected:
            raise ValueError(f"battlefield core array model drift: {key}")
    if _targeting_facts(output["coreModel"]["targeting"]) != fixture["expected"]["targetingFacts"]:
        raise ValueError("battlefield range and targeting model drift")
    if output["coreModel"]["pathSelection"] != fixture["expected"]["pathSelectionFacts"]:
        raise ValueError("battlefield attack-position or move-string model drift")
    if output["coreModel"]["lateHelpers"] != fixture["expected"]["lateHelperFacts"]:
        raise ValueError("battlefield move-order or trapped-chest model drift")
    for name, address in output["coreModel"]["arrays"].items():
        if fixture["ram"][name] != address:
            raise ValueError(f"battlefield RAM address binding drift: {name}")
    if (
        output["coreModel"]["pathSelection"]["moveString"]["ramAddress"]
        != fixture["ram"]["moveString"]
    ):
        raise ValueError("battlefield move-string RAM address binding drift")
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
