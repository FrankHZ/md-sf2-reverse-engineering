from __future__ import annotations

import hashlib
import json
import subprocess
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battlefield import (
    _evaluate_equate,
    _load_equates,
    _require_ordered_fragments,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-battle-control-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle")
MANIFEST = repo_path("manifests/extractions/battle-control-static.json")
SCHEMA = repo_path("schemas/battle-control-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-control-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-control-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

REPRESENTATIVE_SYMBOLS = {
    "battledebugfunction1B120A.asm": "BattleDebugFunction1B120A",
    "battleloop_1.asm": "BattleLoop",
    "battleloop_2.asm": "BattleLoop_Victory",
    "battlemusic.asm": "PlayMapMusic",
    "battlevints.asm": "SetBaseVIntFunctions",
    "getbattlespritesetsubsection.asm": "GetBattleSpritesetSubsection",
    "getcombatantstartingposition.asm": "GetCombatantStartingPosition",
    "getdifficulty.asm": "GetDifficulty",
    "getlaserfacing.asm": "GetLaserFacing",
}


def _build_control_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        root / "battleloop_1.asm",
        [
            "chkFlg  88",
            "move.l  ((SAVED_SECONDS_COUNTER-$1000000)).w,((SECONDS_COUNTER-$1000000)).w",
            "jsr     j_ClearAiMemory",
            "bsr.w   LoadBattle",
            "clr.l   ((SECONDS_COUNTER-$1000000)).w",
            "jsr     j_ExecuteBeforeBattleCutscene",
            "bsr.w   HealLivingAndImmortalAllies",
            "jsr     j_InitializeAllAlliesBattlePositions",
            "jsr     j_InitializeAllEnemiesBattlePositions",
            "jsr     j_ExecuteBattleStartCutscene",
            "bsr.w   ActivateEnemies",
            "jsr     j_PopulateTargetsListWithSpawningEnemies",
            "bsr.w   GenerateBattleTurnOrder",
            "bsr.w   ExecuteIndividualTurn",
            "jsr     ProcessKilledCombatants(pc)",
            "bsr.w   CountRemainingCombatants",
            "bsr.w   ProcessAfterTurnEffects",
            "addq.b  #TURN_ORDER_ENTRY_SIZE,((CURRENT_BATTLE_TURN-$1000000)).w",
        ],
    )
    _require_ordered_fragments(
        root / "battleloop_2.asm",
        [
            "BattleLoop_Victory:",
            "bsr.w   HealLivingAndImmortalAllies",
            "jsr     j_ExecuteAfterBattleCutscene",
            "addi.w  #BATTLE_UNLOCKED_FLAGS_START,d1",
            "jsr     j_ClearFlag",
            "addi.w  #BATTLE_UNLOCKED_TO_COMPLETED_FLAGS_OFFSET,d1",
            "jsr     j_SetFlag",
            "moveq   #1,d4",
            "BattleLoop_Defeat:",
            "bsr.w   UpdateBattleUnlockedFlag",
            "jsr     j_GetMaxHp",
            "jsr     j_SetCurrentHp",
            "jsr     j_GetGold",
            "lsr.l   #1,d1",
            "jsr     j_SetGold",
            "jsr     GetEgressPositionForBattle(pc)",
            "moveq   #-1,d4",
            "jsr     j_UpgradeBattle",
            "clr.w   d4",
        ],
    )
    _require_ordered_fragments(
        root / "getdifficulty.asm",
        [
            "move.w  #FLAG_INDEX_DIFFICULTY1,d1",
            "move.w  #1,d2",
            "move.w  #FLAG_INDEX_DIFFICULTY2,d1",
            "addq.w  #2,d2",
            "move.w  d2,d1",
        ],
    )
    _require_ordered_fragments(
        root / "getbattlespritesetsubsection.asm",
        [
            "tst.b   d2",
            "lea     BATTLESPRITESET_OFFSET_ALLY_ENTRIES(a0),a0",
            "mulu.w  #BATTLESPRITESET_ENTITY_ENTRY_SIZE,d0",
            "mulu.w  #BATTLESPRITESET_REGION_ENTRY_SIZE,d0",
        ],
    )
    _require_ordered_fragments(
        root / "getcombatantstartingposition.asm",
        [
            "btst    #COMBATANT_BIT_ENEMY,d0",
            "move.w  #BATTLESPRITESET_SUBSECTION_ALLIES,d1",
            "move.w  #-1,d1",
            "move.w  #-1,d2",
            "move.w  #BATTLESPRITESET_SUBSECTION_ENEMIES,d1",
            "move.b  BATTLESPRITESET_ENTITYOFFSET_STARTING_X(a0),d1",
            "move.b  BATTLESPRITESET_ENTITYOFFSET_STARTING_Y(a0),d2",
        ],
    )
    _require_ordered_fragments(
        root / "getlaserfacing.asm",
        [
            "lea     list_BattlesWithLasers(pc), a0",
            "lea     pt_LaserEnemyFacingForBattle(pc), a0",
            "cmpi.b  #-1,d6",
            "moveq   #0,d3",
            "jsr     j_ClearTotalMoveCostsAndMovableGridArrays",
            "jsr     j_SetMovableSpace",
            "jsr     j_GetCombatantOccupyingSpace",
            "move.b  d0,(a0,d3.w)",
            "move.w  d3,(a0)",
        ],
    )
    vint_source = (root / "battlevints.asm").read_text(encoding="utf-8")
    vint_functions = [
        "VInt_UpdateMapPlanes",
        "VInt_UpdateEntities",
        "VInt_UpdateViewData",
        "VInt_UpdateScrollingData",
        "VInt_UpdateSprites",
        "VInt_UpdateWindows",
        "VInt_UpdateMapAnimations",
    ]
    if any(vint_source.count(name) != 1 for name in vint_functions):
        raise ValueError("battle VInt function set drift")

    definitions = _load_equates(disasm / "sf2const.asm", disasm / "sf2enums.asm")
    memo: dict[str, int] = {}

    def constant(name: str) -> int:
        return _evaluate_equate(name, definitions, memo)

    return {
        "mainLoop": {
            "suspendFlag": 88,
            "newBattleClearsSeconds": True,
            "suspendRestoresSavedSeconds": True,
            "newBattleClearsRegionFlagRange": [
                constant("BATTLE_REGION_FLAGS_START"),
                constant("BATTLE_REGION_FLAGS_END"),
            ],
            "roundOrder": [
                "activate-enemies",
                "region-cutscene",
                "spawn-enemies",
                "generate-turn-order",
            ],
            "turnEndOrder": [
                "defeated-cutscene",
                "process-killed",
                "count-remaining",
                "process-after-turn",
                "process-killed",
                "count-remaining",
                "advance-turn-index",
            ],
            "turnOrderTerminator": 255,
        },
        "outcomes": {
            "victoryReturn": 1,
            "defeatReturn": -1,
            "victoryHealsParty": True,
            "victoryClearsUnlockedAndSetsCompleted": True,
            "completedFlagOffset": constant("BATTLE_UNLOCKED_TO_COMPLETED_FLAGS_OFFSET"),
            "defeatRestoresLeaderHp": True,
            "defeatGoldOperation": "unsigned floor divide by 2",
            "defeatUsesEgressPosition": True,
            "battle4DefeatReturn": 0,
            "battle4DefeatUpgradesBattle": True,
        },
        "difficulty": {
            "flagIndexes": [
                constant("FLAG_INDEX_DIFFICULTY1"),
                constant("FLAG_INDEX_DIFFICULTY2"),
            ],
            "weights": [1, 2],
            "resultRange": [0, 3],
        },
        "spriteset": {
            "subsections": ["sizes", "allies", "enemies", "regions", "ai-points"],
            "entityEntryBytes": constant("BATTLESPRITESET_ENTITY_ENTRY_SIZE"),
            "regionEntryBytes": constant("BATTLESPRITESET_REGION_ENTRY_SIZE"),
            "invalidStartingPosition": [-1, -1],
            "startingCoordinatesAreUnsignedBytes": True,
        },
        "music": {
            "outsideBattlePreservesMapMusic": True,
            "battleTheme3Inputs": [
                constant("MUSIC_NOTHING"),
                constant("MUSIC_TOWN"),
                constant("MUSIC_MITULA"),
            ],
            "battleTheme1Inputs": [
                constant("MUSIC_MITULA_SHRINE"),
                constant("MUSIC_CASTLE"),
            ],
        },
        "vint": {
            "clearsExistingFunctions": True,
            "functions": vint_functions,
        },
        "laser": {
            "nonLaserBattleTargetCount": 0,
            "facingMinusOneTargetCount": 0,
            "marksEverySpaceUntilMapEdge": True,
            "collectsEveryOccupiedSpace": True,
            "output": "TARGETS_LIST plus TARGETS_LIST_LENGTH",
        },
        "debugFunction": {
            "upstreamMarksUnused": True,
            "endsInSelfLoop": True,
            "mustNotBeTreatedAsReachableGameplay": True,
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
        raise ValueError(f"battle-control inventory requires SF2DISASM {expected}, got {commit}")
    disasm = upstream_path / "disasm"
    if not (disasm / SOURCE_ROOT).is_dir():
        raise ValueError(f"battle-control source root is missing: {disasm / SOURCE_ROOT}")
    return disasm, commit, toolchain


def build_battle_control_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    source_paths = sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in source_paths]
    if {Path(row["path"]).name for row in files} != set(REPRESENTATIVE_SYMBOLS):
        raise ValueError("battle-control source file set drift")
    all_labels = {label for row in files for label in row["globalLabels"]}
    direct_calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            direct_calls[call["target"]] += call["siteCount"]
    index = load_json(RESEARCH_INDEX)
    records = [
        record for record in index["records"] if Path(record["sourcePath"]).parent == SOURCE_ROOT
    ]
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
        "indexedRecordCount": len(records),
        "indexedFileCount": len({record["sourcePath"] for record in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "internalDirectCallTargets": sorted(
            target for target in direct_calls if target in all_labels
        ),
        "externalDirectCallTargets": sorted(
            target for target in direct_calls if target not in all_labels
        ),
        "controlFacts": _build_control_facts(disasm),
        "files": files,
    }


def verify_battle_control_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_control_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle-control static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("battle-control fixture provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle-control static summary drift")
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(f"battle-control representative symbol drift: {filename}::{symbol}")
    if output["controlFacts"] != fixture["expected"]["controlFacts"]:
        raise ValueError("battle-control model drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError(
            "battle-control static hash mismatch: expected "
            f"{manifest['outputSha256']}, got {digest}"
        )
    destination = output_path or repo_path("local/derived/battle-control-static.json")
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
