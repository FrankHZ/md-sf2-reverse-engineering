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

ID = "sf2-battle-functions-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/battlefunctions")
MANIFEST = repo_path("manifests/extractions/battle-functions-static.json")
SCHEMA = repo_path("schemas/battle-functions-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-functions-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-functions-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

REPRESENTATIVE_SYMBOLS = {
    "battlefunctions_0.asm": "CreatePulsatingBlocksForGrid",
    "battlefunctions_1.asm": "ExecuteBattleaction_AngelWing",
    "battlefunctions_2.asm": "UpdateTargetsListForCombatant",
    "battlefunctions_3.asm": "table_RelativeTileMoveX",
    "executeindividualturn.asm": "ExecuteIndividualTurn",
    "loadBattle.asm": "LoadBattle",
    "setmovesfx.asm": "SetMoveSfx",
}


def _build_function_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        root / "executeindividualturn.asm",
        [
            "bsr.w   ClearDeadCombatantsListLength",
            "jsr     j_GetCurrentHp",
            "andi.w  #STATUSEFFECT_MUDDLE,d1",
            "andi.w  #AIBITFIELD_AI_CONTROLLED,d1",
            "tst.b   ((AUTO_BATTLE_TOGGLE-$1000000)).w",
            "jsr     j_StartAiControl",
            "jsr     j_BuildMovementRangeGrid",
            "andi.w  #STATUSEFFECT_SLEEP,d1",
            "andi.w  #STATUSEFFECT_STUN,d1",
            "bsr.w   ProcessBattleEntityControlPlayerInput",
            "bsr.w   ExecuteAiControl",
            "cmpi.w  #SPELL_EGRESS,d0",
            "cmpi.w  #ITEM_ANGEL_WING,d0",
            "cmpi.w  #BATTLEACTION_STAY,((CURRENT_BATTLEACTION-$1000000)).w",
            "cmpi.w  #BATTLEACTION_TRAPPED_CHEST,((CURRENT_BATTLEACTION-$1000000)).w",
            "cmpi.w  #CLASS_MNST,d1",
            "jsr     (GenerateRandomNumber).w",
            "jsr     j_WriteBattlesceneScript",
            "jsr     j_ExecuteBattlesceneScript",
            "jsr     j_EndBattlescene",
            "jsr     LoadBattle(pc)",
        ],
    )
    _require_ordered_fragments(
        root / "battlefunctions_1.asm",
        [
            "ExecuteBattleaction_AngelWing:",
            "jsr     j_RemoveItemBySlot",
            "ExecuteBattleaction_Egress:",
            "jsr     j_GetSpellCost",
            "jsr     j_DecreaseCurrentMp",
            "bsr.w   UpdateBattleUnlockedFlag",
            "bsr.w   GetEgressPositionForBattle",
            "clr.w   d4",
        ],
    )
    _require_ordered_fragments(
        root / "loadBattle.asm",
        [
            "bsr.w   FadeOutToBlackAll",
            "jsr     (LoadMapTilesets).w",
            "jsr     j_PositionBattleEntities",
            "jsr     (InitializeSprites).w",
            "jsr     (LoadMap).w",
            "jsr     (LoadEntityMapsprites).w",
            "bsr.w   SetBaseVIntFunctions",
            "jsr     j_LoadBattleTerrainData",
            "jsr     (PlayMapMusic).w",
            "jsr     (FadeInFromBlack).w",
            "jsr     j_OpenTimerWindow",
        ],
    )
    _require_ordered_fragments(
        root / "setmovesfx.asm",
        [
            "clr.w   ((MOVE_SFX-$1000000)).w",
            "move.w  #SFX_WALKING,((MOVE_SFX-$1000000)).w",
            "jsr     j_GetEquippedRing",
            "cmpi.w  #ITEM_CHIRRUP_SANDALS,d1",
            "move.w  #SFX_BLOAB,((MOVE_SFX-$1000000)).w",
        ],
    )
    definitions = _load_equates(disasm / "sf2const.asm", disasm / "sf2enums.asm")
    memo: dict[str, int] = {}

    def constant(name: str) -> int:
        return _evaluate_equate(name, definitions, memo)

    return {
        "turnControl": {
            "deadActorSkipsTurn": True,
            "aiCauses": ["muddle", "ai-controlled-bit", "ally-auto-battle", "uncontrolled-enemy"],
            "playerControlledEnemyRequiresToggle": True,
            "sleepAndStunSkipAction": True,
            "staySkipsAction": True,
            "egressAndAngelWingExitBeforeBattlescene": True,
            "ordinaryActionWritesAndExecutesBattlesceneThenReloadsBattle": True,
        },
        "kiwiFlameBreath": {
            "class": constant("CLASS_MNST"),
            "requiresPhysicalAttack": True,
            "rngUpperBound": constant("CHANCE_TO_PERFORM_KIWI_FLAME_BREATH"),
            "rngSuccessValue": 0,
            "upgradeLevels": [
                constant("KIWI_FLAME_BREATH_UPGRADE_LEVEL1"),
                constant("KIWI_FLAME_BREATH_UPGRADE_LEVEL2"),
                constant("KIWI_FLAME_BREATH_UPGRADE_LEVEL3"),
            ],
            "spellLevels": [0, 1, 2, 3],
        },
        "egress": {
            "angelWingConsumesItemFirst": True,
            "spellConsumesDefinitionMp": True,
            "bothCloseBattlefieldWindows": True,
            "bothUpdateUnlockedFlag": True,
            "bothReturnEgressPosition": True,
            "returnCode": 0,
        },
        "loadBattle": {
            "order": [
                "fade-out",
                "load-tilesets",
                "position-entities",
                "initialize-sprites",
                "load-map",
                "load-entity-sprites",
                "set-battle-vints",
                "load-terrain",
                "play-map-music",
                "fade-in",
            ],
            "fairyWoodsOpensTimer": True,
        },
        "moveSfx": {
            "outsideBattle": 0,
            "battleDefault": "walking",
            "chirrupSandalsOverride": "bloab",
            "equipmentOverrideAlsoAppliesOutsideBattle": True,
        },
        "largeHelperBoundary": {
            "cursorEntityAndGridFileInventoried": True,
            "targetingAndPlayerInputFileInventoried": True,
            "relativeMovementTableFileInventoried": True,
            "deeperBranchSemanticsRemainQueued": True,
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
        raise ValueError(f"battle-functions inventory requires SF2DISASM {expected}, got {commit}")
    disasm = upstream_path / "disasm"
    if not (disasm / SOURCE_ROOT).is_dir():
        raise ValueError(f"battle-functions source root is missing: {disasm / SOURCE_ROOT}")
    return disasm, commit, toolchain


def build_battle_functions_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    paths = sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if {Path(row["path"]).name for row in files} != set(REPRESENTATIVE_SYMBOLS):
        raise ValueError("battle-functions source file set drift")
    all_labels = {label for row in files for label in row["globalLabels"]}
    direct_calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            direct_calls[call["target"]] += call["siteCount"]
    index = load_json(RESEARCH_INDEX)
    records = [r for r in index["records"] if Path(r["sourcePath"]).parent == SOURCE_ROOT]
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
        "indexedFileCount": len({r["sourcePath"] for r in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "indexedRecordIds": sorted(r["id"] for r in records),
        "indexedSourcePaths": sorted({r["sourcePath"] for r in records}),
        "internalDirectCallTargets": sorted(t for t in direct_calls if t in all_labels),
        "externalDirectCallTargets": sorted(t for t in direct_calls if t not in all_labels),
        "functionFacts": _build_function_facts(disasm),
        "files": files,
    }


def verify_battle_functions_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_functions_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle-functions static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("battle-functions fixture provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle-functions static summary drift")
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(f"battle-functions representative symbol drift: {filename}::{symbol}")
    if output["functionFacts"] != fixture["expected"]["functionFacts"]:
        raise ValueError("battle-functions model drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("battle-functions static canonical hash drift")
    destination = output_path or repo_path("local/derived/battle-functions-static.json")
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
