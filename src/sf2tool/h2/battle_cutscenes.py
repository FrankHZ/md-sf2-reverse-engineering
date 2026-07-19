from __future__ import annotations

import hashlib
import json
import subprocess
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battlefield import _require_ordered_fragments
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-battle-cutscenes-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/cutscenes")
MANIFEST = repo_path("manifests/extractions/battle-cutscenes-static.json")
SCHEMA = repo_path("schemas/battle-cutscenes-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-cutscenes-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-cutscenes-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

REPRESENTATIVE_SYMBOLS = {
    "afterbattlecutscenesend.asm": "EndAfterBattleCutscene",
    "afterbattlecutscenesstart.asm": "ExecuteAfterBattleCutscene",
    "afterenemyleaderdeathpositions.asm": "ApplyPositionsAfterEnemyLeaderDies",
    "battleendcutscenesend.asm": "loc_47C48",
    "battleendcutscenesstart.asm": "ExecuteBattleCutscene_Defeated",
    "battlestartcutscenesend.asm": "loc_47B8C",
    "battlestartcutscenesstart.asm": "ExecuteBattleStartCutscene",
    "beforebattlecutscenesend.asm": "loc_47AE8",
    "beforebattlecutscenesstart.asm": "ExecuteBeforeBattleCutscene",
    "regionactivatedcutscenes.asm": "ExecuteBattleRegionCutscene",
}


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _cutscene_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        root / "beforebattlecutscenesstart.asm",
        [
            "addi.w  #BATTLE_INTRO_CUTSCENE_FLAGS_START,d1",
            "jsr     j_CheckFlag",
            "bne.w   loc_47AE8",
            "move.w  rpt_BeforeBattleCutscenes(pc,d0.w),d0",
            "bsr.w   ExecuteMapScript",
        ],
    )
    _require_ordered_fragments(
        root / "battlestartcutscenesstart.asm",
        [
            "addi.w  #BATTLE_INTRO_CUTSCENE_FLAGS_START,d1",
            "jsr     j_CheckFlag",
            "bne.w   loc_47B8C",
            "jsr     j_SetFlag",
            "move.w  rpt_BattleStartCutscenes(pc,d0.w),d0",
            "bsr.w   ExecuteMapScript",
        ],
    )
    _require_ordered_fragments(
        root / "afterbattlecutscenesstart.asm",
        [
            "addi.w  #BATTLE_COMPLETED_FLAGS_START,d1",
            "jsr     j_CheckFlag",
            "bne.w   EndAfterBattleCutscene",
            "move.w  rpt_AfterBattleCutscenes(pc,d0.w),d0",
            "bsr.w   ExecuteMapScript",
        ],
    )
    _require_ordered_fragments(
        root / "afterbattlecutscenesend.asm",
        [
            "move.b  table_AfterBattleJoins(pc,d0.w),d0",
            "jsr     j_JoinForce",
        ],
    )
    _require_ordered_fragments(
        root / "battleendcutscenesstart.asm",
        [
            "moveq   #ALLY_BOWIE,d0",
            "jsr     j_GetCurrentHp",
            "move.w  #COMBATANT_ENEMIES_START,d0",
            "jsr     j_GetCurrentHp",
            "addi.w  #BATTLE_COMPLETED_FLAGS_START,d1",
            "jsr     j_CheckFlag",
            "move.w  rpt_EnemyDefeatedCutscenes(pc,d0.w),d0",
            "bsr.w   ExecuteMapScript",
        ],
    )
    _require_ordered_fragments(
        root / "battleendcutscenesend.asm",
        [
            "move.b  table_EnemyLeaderPresentFlags(pc,d0.w),d0",
            "move.w  #COMBATANT_ENEMIES_START,d0",
            "jsr     j_GetCurrentHp",
            "move.b  d0,(a0,d1.w)",
            "move.b  #-1,1(a0,d1.w)",
            "addq.w  #1,(DEAD_COMBATANTS_LIST_LENGTH).l",
        ],
    )
    _require_ordered_fragments(
        root / "afterenemyleaderdeathpositions.asm",
        [
            "moveq   #ALLY_BOWIE,d0",
            "move.w  #COMBATANT_ENEMIES_START,d0",
            "lea     table_AfterBattlePositions(pc), a0",
            "cmpi.w  #-1,(a0)",
            "move.w  #-1,d1",
            "jsr     j_SetCombatantX",
            "jsr     j_SetCurrentHp",
            "movea.l 2(a0),a0",
            "move.b  1(a0),d1",
            "jsr     j_SetCombatantX",
            "move.b  2(a0),d1",
            "jsr     j_SetCombatantY",
        ],
    )
    _require_ordered_fragments(
        root / "regionactivatedcutscenes.asm",
        [
            "lea     table_BattleRegionCutscenes-8(pc), a0",
            "addq.w  #8,a0",
            "cmpi.w  #TERMINATOR_WORD,(a0)",
            "cmp.b   (a0),d1",
            "move.w  2(a0),d1",
            "jsr     j_CheckFlag",
            "move.b  1(a0),d0",
            "jsr     j_CheckTriggerRegionFlag",
            "jsr     j_SetFlag",
            "movea.l 4(a0),a0",
            "trap    #MAPSCRIPT",
        ],
    )
    return {
        "intro": {
            "beforeBattleChecksSharedIntroFlag": True,
            "beforeBattleSetsFlag": False,
            "battleStartChecksSharedIntroFlag": True,
            "battleStartSetsFlagBeforeScript": True,
            "dispatchesByCurrentBattle": True,
        },
        "afterBattle": {
            "skipsScriptWhenBattleCompleted": True,
            "dispatchesByCurrentBattle": True,
            "joinTableRunsAtSharedEnd": True,
        },
        "enemyDefeated": {
            "requiresBowieAlive": True,
            "requiresLeaderSlotDead": True,
            "skipsScriptWhenBattleCompleted": True,
            "queuesLivingEnemiesWhenLeaderPresent": True,
            "deadListEntryTerminator": 255,
        },
        "leaderDeathPositions": {
            "requiresBowieAliveAndLeaderDead": True,
            "battleTableEntryBytes": 6,
            "movesAllSlotsOffscreen": True,
            "setsAllEnemyHpToZero": True,
            "positionEntryBytes": 4,
            "positionTerminator": -1,
            "unreachableDeadListWritePresent": True,
        },
        "region": {
            "tableEntryBytes": 8,
            "terminator": -1,
            "checksBattleThenPlayedFlagThenRegion": True,
            "setsPlayedFlagBeforeMapScript": True,
            "scriptDispatch": "MAPSCRIPT trap",
        },
    }


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
        raise ValueError(f"battle cutscenes require SF2DISASM {expected}, got {commit}")
    disasm = upstream_path / "disasm"
    return disasm, commit, toolchain


def build_battle_cutscene_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    paths = sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if {Path(row["path"]).name for row in files} != set(REPRESENTATIVE_SYMBOLS):
        raise ValueError("battle cutscene file set drift")
    labels = {label for row in files for label in row["globalLabels"]}
    calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if Path(record["sourcePath"]).parent == SOURCE_ROOT
    ]
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(calls.values()),
        "indirectCallSiteCount": sum(row["indirectCallSiteCount"] for row in files),
        "uniqueDirectTargetCount": len(calls),
        "internalDirectTargetCount": sum(target in labels for target in calls),
        "externalDirectTargetCount": sum(target not in labels for target in calls),
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
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "cutsceneFacts": _cutscene_facts(disasm),
        "files": files,
    }


def verify_battle_cutscene_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_cutscene_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle cutscene static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("battle cutscene provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle cutscene summary drift")
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(f"battle cutscene symbol drift: {filename}::{symbol}")
    if output["cutsceneFacts"] != fixture["expected"]["cutsceneFacts"]:
        raise ValueError("battle cutscene model drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("battle cutscene canonical hash drift")
    destination = output_path or repo_path("local/derived/battle-cutscenes-static.json")
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
