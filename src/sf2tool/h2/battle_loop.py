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

ID = "sf2-battle-loop-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/battleloop")
MANIFEST = repo_path("manifests/extractions/battle-loop-static.json")
SCHEMA = repo_path("schemas/battle-loop-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-loop-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-loop-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

REPRESENTATIVE_SYMBOLS = {
    "activateenemies.asm": "ActivateEnemies",
    "clearaimemory.asm": "ClearAiMemory",
    "cleardeadcombatantslist.asm": "ClearDeadCombatantsListLength",
    "countremainingcombatants.asm": "CountRemainingCombatants",
    "getegresspositionforbattle.asm": "GetEgressPositionForBattle",
    "hasjarojoined.asm": "HasJaroJoinedTheForce",
    "heallivingandimmortalallies.asm": "HealLivingAndImmortalAllies",
    "initializecombatants.asm": "InitializeAllAlliesBattlePositions",
    "killallenemies.asm": "KillRemainingEnemies",
    "loadbattleterraindata.asm": "LoadBattleTerrainData",
    "populateenemyspawns.asm": "PopulateTargetsListWithSpawningEnemies",
    "printdefcons.asm": "PrintAllActivatedDefCons",
    "processafterturneffects.asm": "ProcessAfterTurnEffects",
    "processkilledcombatants.asm": "ProcessKilledCombatants",
    "spawnenemy.asm": "SpawnEnemySkipCamera",
    "triggerregions.asm": "TriggerRegionsAndActivateEnemies",
    "turnorderfunctions.asm": "GenerateBattleTurnOrder",
    "upgradeenemies.asm": "IsBattleUpgradable",
}


def _build_lifecycle_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        root / "clearaimemory.asm",
        [
            "move.w  #48,d1",
            "move.b  #-1,(a0,d0.w)",
            "move.b  #0,(a1,d0.w)",
            "subq.w  #1,d1",
        ],
    )
    _require_ordered_fragments(
        root / "countremainingcombatants.asm",
        [
            "move.w  #COMBATANT_ALLIES_COUNTER,d7",
            "jsr     j_GetCombatantX",
            "jsr     j_GetCurrentHp",
            "addq.w  #1,d2",
            "move.w  #COMBATANT_ENEMIES_START,d0",
            "move.w  #COMBATANT_ENEMIES_COUNTER,d7",
            "addq.w  #1,d3",
            "clr.w   d0",
            "jsr     j_GetCurrentHp",
            "clr.w   d2",
        ],
    )
    _require_ordered_fragments(
        root / "killallenemies.asm",
        [
            "clr.w   ((DEAD_COMBATANTS_LIST_LENGTH-$1000000)).w",
            "jsr     j_GetCombatantX",
            "jsr     j_GetCombatantY",
            "jsr     j_GetCurrentHp",
            "move.b  d0,(a0)+",
            "jsr     j_SetCurrentHp",
        ],
    )
    _require_ordered_fragments(
        root / "heallivingandimmortalallies.asm",
        [
            "cmpi.b  #ALLY_PETER,d0",
            "cmpi.b  #ALLY_LEMON,d0",
            "jsr     j_GetCurrentHp",
            "jsr     j_GetMaxHp",
            "jsr     j_SetCurrentHp",
            "jsr     j_GetMaxMp",
            "jsr     j_SetCurrentMp",
            "andi.w  #STATUSEFFECT_STUN|STATUSEFFECT_POISON|STATUSEFFECT_CURSE,d1",
            "jsr     j_UpdateCombatantStats",
        ],
    )
    _require_ordered_fragments(
        root / "loadbattleterraindata.asm",
        [
            "lea     pt_BattleTerrainData(pc), a0",
            "lsl.l   #2,d1",
            "movea.l (a0,d1.w),a0",
            "lea     (BATTLE_TERRAIN_ARRAY).l,a1",
            "jsr     (LoadStackCompressedData).w",
        ],
    )
    _require_ordered_fragments(
        root / "populateenemyspawns.asm",
        [
            "andi.w  #AIBITFIELD_INITIALIZATION_MASK,d1",
            "cmpi.w  #AIBITFIELD_HIDDEN,d1",
            "cmpi.w  #AIBITFIELD_RESPAWN,d1",
            "cmpi.w  #AIBITFIELD_RESPAWN|AIBITFIELD_HIDDEN,d1",
            "move.w  d5,(a0)",
        ],
    )
    _require_ordered_fragments(
        root / "processkilledcombatants.asm",
        [
            "tst.w   ((DEAD_COMBATANTS_LIST_LENGTH-$1000000)).w",
            "jsr     j_IncreaseDefeats",
            "jsr     j_IncreaseKills",
            "jsr     j_SetCombatantX",
            "jsr     j_SetCombatantY",
            "jsr     j_SetStatusEffects",
            "jsr     j_UpdateCombatantStats",
            "jsr     SetEntityPosition",
        ],
    )

    definitions = _load_equates(disasm / "sf2const.asm", disasm / "sf2enums.asm")
    memo: dict[str, int] = {}

    def constant(name: str) -> int:
        return _evaluate_equate(name, definitions, memo)

    preserved_status_mask = (
        constant("STATUSEFFECT_STUN")
        | constant("STATUSEFFECT_POISON")
        | constant("STATUSEFFECT_CURSE")
    )
    return {
        "rosters": {
            "allySlots": constant("COMBATANT_ALLIES_COUNTER") + 1,
            "enemySlots": constant("COMBATANT_ENEMIES_COUNTER") + 1,
            "enemyStart": constant("COMBATANT_ENEMIES_START"),
        },
        "aiMemoryReset": {
            "entryCount": 48,
            "lastTargetFill": 255,
            "memoryFill": 0,
        },
        "remainingCombatants": {
            "requiresPlacedX": True,
            "requiresPositiveHp": True,
            "leaderDeathForcesAllyCountZero": True,
            "allyResultRegister": "d2.w",
            "enemyResultRegister": "d3.w",
        },
        "killRemainingEnemies": {
            "requiresPlacedXAndY": True,
            "requiresPositiveHp": True,
            "clearsDeadListFirst": True,
            "appendsCombatantBeforeSettingHpZero": True,
        },
        "betweenBattleHealing": {
            "immortalAllies": [constant("ALLY_PETER"), constant("ALLY_LEMON")],
            "otherDeadAlliesSkipped": True,
            "restoresHpAndMpToMaximum": True,
            "preservedStatusMask": preserved_status_mask,
            "updatesDerivedStats": True,
        },
        "terrainLoad": {
            "pointerStrideBytes": 4,
            "destination": constant("BATTLE_TERRAIN_ARRAY"),
            "compression": "Stack",
        },
        "enemySpawnSelection": {
            "enemySlots": constant("COMBATANT_ENEMIES_NUMBER"),
            "initializationModes": [
                constant("AIBITFIELD_HIDDEN"),
                constant("AIBITFIELD_RESPAWN"),
                constant("AIBITFIELD_HIDDEN") | constant("AIBITFIELD_RESPAWN"),
            ],
            "output": "TARGETS_LIST plus TARGETS_LIST_LENGTH",
            "resetFailureSkipped": True,
        },
        "killedCombatants": {
            "zeroLengthReturnsImmediately": True,
            "allyDeathIncrementsDefeats": True,
            "enemyDeathCreditsBattlesceneFirstAlly": True,
            "clearsCoordinatesToMinusOne": True,
            "clearsStatus": True,
            "updatesDerivedStats": True,
            "movesEntityOffMap": 0x7000,
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
        raise ValueError(f"battle-loop inventory requires SF2DISASM {expected}, got {commit}")
    disasm = upstream_path / "disasm"
    if not (disasm / SOURCE_ROOT).is_dir():
        raise ValueError(f"battle-loop source root is missing: {disasm / SOURCE_ROOT}")
    return disasm, commit, toolchain


def build_battle_loop_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    source_paths = sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in source_paths]
    if {Path(row["path"]).name for row in files} != set(REPRESENTATIVE_SYMBOLS):
        raise ValueError("battle-loop source file set drift")

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
        "lifecycleFacts": _build_lifecycle_facts(disasm),
        "files": files,
    }


def verify_battle_loop_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_loop_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle-loop static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("battle-loop fixture provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle-loop static summary drift")
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(f"battle-loop representative symbol drift: {filename}::{symbol}")
    if output["lifecycleFacts"] != fixture["expected"]["lifecycleFacts"]:
        raise ValueError("battle-loop lifecycle model drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError(
            f"battle-loop static hash mismatch: expected {manifest['outputSha256']}, got {digest}"
        )
    destination = output_path or repo_path("local/derived/battle-loop-static.json")
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
