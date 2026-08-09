from __future__ import annotations

import hashlib
import json
import re
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
from sf2tool.h2.entity_action_scripts import _global_access_rows
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-battle-functions-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/battlefunctions")
MANIFEST = repo_path("manifests/extractions/battle-functions-static.json")
SCHEMA = repo_path("schemas/battle-functions-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-functions-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-functions-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

PLAYER_CONTROL_FUNCTIONS = {
    "ControlCursorEntity": Path("battlefunctions_0.asm"),
    "ControlCursorEntity_ChooseTarget": Path("battlefunctions_0.asm"),
    "SetCursorDestinationToNextBattleEntity": Path("battlefunctions_0.asm"),
    "ProcessBattleEntityControlPlayerInput": Path("battlefunctions_2.asm"),
    "BattlefieldMenu": Path("battlefunctions_2.asm"),
    "PerformAiTargetingVisualAct": Path("battlefunctions_2.asm"),
}

FUNCTION_ADDRESS_SYMBOLS = {
    "pulsatingGridAddress": "CreatePulsatingBlocksForGrid",
    "angelWingAddress": "ExecuteBattleaction_AngelWing",
    "updateTargetsAddress": "UpdateTargetsListForCombatant",
    "relativeMoveTableAddress": "table_RelativeTileMoveX",
    "executeTurnAddress": "ExecuteIndividualTurn",
    "loadBattleAddress": "LoadBattle",
    "setMoveSfxAddress": "SetMoveSfx",
    "controlCursorAddress": "ControlCursorEntity",
    "chooseTargetAddress": "ControlCursorEntity_ChooseTarget",
    "setCursorTargetAddress": "SetCursorDestinationToNextBattleEntity",
    "processPlayerInputAddress": "ProcessBattleEntityControlPlayerInput",
    "battlefieldMenuAddress": "BattlefieldMenu",
    "aiTargetVisualAddress": "PerformAiTargetingVisualAct",
    "equipInBattleAddress": "EquipNewItemInBattle",
    "checkGoldChestAddress": "CheckGoldChest",
}

REPRESENTATIVE_SYMBOLS = {
    "battlefunctions_0.asm": "CreatePulsatingBlocksForGrid",
    "battlefunctions_1.asm": "ExecuteBattleaction_AngelWing",
    "battlefunctions_2.asm": "UpdateTargetsListForCombatant",
    "battlefunctions_3.asm": "table_RelativeTileMoveX",
    "executeindividualturn.asm": "ExecuteIndividualTurn",
    "loadBattle.asm": "LoadBattle",
    "setmovesfx.asm": "SetMoveSfx",
}


def _index_records_for_source_root(source_paths: set[str]) -> dict[str, Any]:
    """Join every research record whose source belongs to this inventory.

    The source path is the sole ownership selector.  A record can therefore
    remain owned by another subsystem's evidence and still belong here when it
    names one of the seven discovered battle-functions sources.
    """
    records_by_source_path: dict[str, list[str]] = {}
    for record in load_json(RESEARCH_INDEX)["records"]:
        source_path = record["sourcePath"]
        path = Path(source_path)
        if not path.is_relative_to(SOURCE_ROOT):
            continue
        if ".." in path.parts or source_path != path.as_posix():
            raise ValueError(f"invalid battle-functions indexed source path: {source_path}")
        if source_path not in source_paths:
            raise ValueError(
                "battle-functions indexed source is absent from the discovered root "
                f"inventory: {source_path}"
            )
        records_by_source_path.setdefault(source_path, []).append(record["id"])

    missing_paths = sorted(source_paths - set(records_by_source_path))
    if missing_paths:
        raise ValueError(
            "battle-functions discovered source lacks a research-index record: "
            + ", ".join(missing_paths)
        )
    indexed_records_by_source_path = [
        {"sourcePath": source_path, "recordIds": sorted(record_ids)}
        for source_path, record_ids in sorted(records_by_source_path.items())
    ]
    indexed_record_ids = [
        record_id
        for row in indexed_records_by_source_path
        for record_id in row["recordIds"]
    ]
    if len(indexed_record_ids) != len(set(indexed_record_ids)):
        raise ValueError("battle-functions research-index duplicate record ID")
    return {
        "indexedRecordIds": sorted(indexed_record_ids),
        "indexedSourcePaths": [
            row["sourcePath"] for row in indexed_records_by_source_path
        ],
        "indexedRecordsBySourcePath": indexed_records_by_source_path,
    }


def _verify_indexed_record_join(
    output: dict[str, Any],
    expected: dict[str, Any],
    discovered_source_paths: list[str],
) -> None:
    """Reject schema-valid source-membership relation drift before writing."""
    relation = output["indexedRecordsBySourcePath"]
    relation_source_paths = [row["sourcePath"] for row in relation]
    relation_record_ids = [
        record_id for row in relation for record_id in row["recordIds"]
    ]
    if len(relation_source_paths) != len(set(relation_source_paths)):
        raise ValueError("battle-functions indexed relation duplicate source path")
    if len(relation_record_ids) != len(set(relation_record_ids)):
        raise ValueError("battle-functions indexed relation duplicate record ID")
    if set(relation_source_paths) != set(discovered_source_paths):
        raise ValueError("battle-functions indexed relation source inventory drift")
    if relation_source_paths != sorted(relation_source_paths):
        raise ValueError("battle-functions indexed relation source order drift")
    if any(row["recordIds"] != sorted(row["recordIds"]) for row in relation):
        raise ValueError("battle-functions indexed relation record order drift")

    indexed_record_ids = output["indexedRecordIds"]
    indexed_source_paths = output["indexedSourcePaths"]
    if indexed_record_ids != sorted(relation_record_ids):
        raise ValueError("battle-functions indexedRecordIds relation drift")
    if indexed_source_paths != relation_source_paths:
        raise ValueError("battle-functions indexedSourcePaths relation order drift")
    if output["summary"]["indexedRecordCount"] != len(indexed_record_ids) or output[
        "summary"
    ]["indexedRecordCount"] != len(relation_record_ids):
        raise ValueError("battle-functions summary indexedRecordCount relation drift")
    if output["summary"]["indexedFileCount"] != len(indexed_source_paths) or output[
        "summary"
    ]["indexedFileCount"] != len(relation_source_paths):
        raise ValueError("battle-functions summary indexedFileCount relation drift")

    file_paths = [row["path"] for row in output["files"]]
    if len(file_paths) != len(set(file_paths)):
        raise ValueError("battle-functions source inventory duplicate path")
    if file_paths != discovered_source_paths:
        raise ValueError("battle-functions source inventory drift")
    if indexed_source_paths != discovered_source_paths:
        raise ValueError("battle-functions indexedSourcePaths source inventory drift")
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        if output[field] != expected[field]:
            raise ValueError(f"battle-functions {field} source-membership drift")


def _verify_fixture_provenance(fixture: dict[str, Any], output: dict[str, Any]) -> None:
    """Derive fixture provenance from the independently pinned manifests."""
    toolchain = load_json(TOOLCHAIN)["sf2disasm"]
    output_upstream = output["upstream"]
    if (
        fixture["upstreamCommit"] != toolchain["commit"]
        or fixture["upstreamCommit"] != output_upstream["commit"]
    ):
        raise ValueError("battle-functions fixture upstream provenance drift")
    if output_upstream["repository"] != toolchain["repository"]:
        raise ValueError("battle-functions output upstream provenance drift")
    if fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("battle-functions fixture ROM provenance drift")


def _function_segments(source: str, name: str) -> list[dict[str, Any]]:
    main = re.search(
        rf"^{re.escape(name)}:\s*\n(?P<body>.*?)"
        rf"^\s*; End of function {re.escape(name)}\s*$",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if main is None:
        raise ValueError(f"battle player-control function body is missing: {name}")
    matches = [("function", main)]
    if name == "ProcessBattleEntityControlPlayerInput":
        chunks = list(
            re.finditer(
                rf"^; START OF FUNCTION CHUNK FOR {re.escape(name)}\s*\n"
                rf"(?P<body>.*?)"
                rf"^; END OF FUNCTION CHUNK FOR {re.escape(name)}\s*$",
                source,
                re.MULTILINE | re.DOTALL,
            )
        )
        if len(chunks) != 3:
            raise ValueError(f"battle player-control chunk boundary drift: {len(chunks)}")
        matches.extend(("chunk", match) for match in chunks)
    return [
        {
            "kind": kind,
            "startLine": source.count("\n", 0, match.start("body")) + 1,
            "endLine": source.count("\n", 0, match.end("body")) + 1,
            "body": match.group("body"),
        }
        for kind, match in matches
    ]


def _direct_calls(statements: list[str]) -> Counter[str]:
    calls: Counter[str] = Counter()
    for statement in statements:
        match = re.match(
            r"^(?:bsr|jsr|jmp)(?:\.[bswl])?\s+\(?([A-Za-z_][A-Za-z0-9_]*)",
            statement,
        )
        if match:
            calls[match.group(1)] += 1
    return calls


def _control_statements(source: str) -> list[str]:
    statements = []
    pending = ""
    for raw_line in source.splitlines():
        line = raw_line.split(";", 1)[0].strip()
        if not line:
            continue
        line = re.sub(r"^[A-Za-z_@][A-Za-z0-9_@.]*:\s*", "", line)
        if not line:
            continue
        pending = f"{pending} {line}".strip()
        if pending.endswith("&"):
            pending = pending[:-1].rstrip()
            continue
        if pending not in {"module", "modend"}:
            statements.append(pending)
        pending = ""
    if pending:
        raise ValueError("unterminated battle player-control continuation")
    return statements


def _branch_targets(statements: list[str]) -> list[str]:
    rows = []
    branch = re.compile(
        r"^(?:b(?:ra|sr|cc|cs|eq|ne|ge|gt|hi|le|ls|lt|mi|pl|vc|vs)"
        r"(?:\.[bswl])?|dbf)\s+(.+)$"
    )
    for statement in statements:
        match = branch.match(statement)
        if match:
            target = match.group(1).rsplit(",", 1)[-1].strip()
            rows.append(target)
    return rows


def _player_control_catalog(disasm: Path, addresses: dict[str, int]) -> dict[str, Any]:
    rows = []
    for name, relative_path in PLAYER_CONTROL_FUNCTIONS.items():
        source_path = SOURCE_ROOT / relative_path
        source = read_upstream_text(disasm / source_path)
        segments = _function_segments(source, name)
        body = "\n".join(segment["body"] for segment in segments)
        statements = _control_statements(body)
        calls = _direct_calls(statements)
        branch_targets = _branch_targets(statements)
        global_accesses = _global_access_rows(statements)
        global_names = {row["name"] for row in global_accesses}
        rows.append(
            {
                "name": name,
                "address": addresses[name],
                "sourcePath": source_path.as_posix(),
                "sourceRanges": [
                    {
                        "kind": segment["kind"],
                        "startLine": segment["startLine"],
                        "endLine": segment["endLine"],
                    }
                    for segment in segments
                ],
                "sourceSha256": hashlib.sha256(body.encode()).hexdigest().upper(),
                "statementCount": len(statements),
                "branchSiteCount": len(branch_targets),
                "branchTargets": branch_targets,
                "directCallSiteCount": sum(calls.values()),
                "directCalls": dict(sorted(calls.items())),
                "globalStateAccesses": global_accesses,
                "inputBits": sorted(set(re.findall(r"\b(INPUT_BIT_[A-Z0-9_]+)\b", body))),
                "battleActionConstants": sorted(
                    set(re.findall(r"\b(BATTLEACTION_[A-Z0-9_]+)\b", body))
                    - global_names
                ),
                "menuConstants": sorted(set(re.findall(r"\b(MENU_[A-Z0-9_]+)\b", body))),
                "textIds": sorted(
                    {
                        int(value)
                        for value in re.findall(
                            r"^\s*txt\s+(\d+)", body, re.MULTILINE
                        )
                    }
                ),
            }
        )
    selected = set(PLAYER_CONTROL_FUNCTIONS)
    direct_target_counts = Counter(
        {
            target: sum(row["directCalls"].get(target, 0) for row in rows)
            for target in {
                target for row in rows for target in row["directCalls"]
            }
        }
    )
    global_states = {
        access["name"] for row in rows for access in row["globalStateAccesses"]
    }
    return {
        "summary": {
            "functionCount": len(rows),
            "sourceRangeCount": sum(len(row["sourceRanges"]) for row in rows),
            "statementCount": sum(row["statementCount"] for row in rows),
            "branchSiteCount": sum(row["branchSiteCount"] for row in rows),
            "directCallSiteCount": sum(row["directCallSiteCount"] for row in rows),
            "uniqueDirectCallTargetCount": len(direct_target_counts),
            "selectedFunctionCallEdgeCount": sum(
                count for target, count in direct_target_counts.items() if target in selected
            ),
            "globalStateCount": len(global_states),
            "inputBitCount": len(
                {value for row in rows for value in row["inputBits"]}
            ),
            "battleActionConstantCount": len(
                {value for row in rows for value in row["battleActionConstants"]}
            ),
            "menuConstantCount": len(
                {value for row in rows for value in row["menuConstants"]}
            ),
        },
        "selectedCallEdges": [
            {"target": target, "siteCount": count}
            for target, count in sorted(direct_target_counts.items())
            if target in selected
        ],
        "globalStates": sorted(global_states),
        "inputBits": sorted({value for row in rows for value in row["inputBits"]}),
        "battleActionConstants": sorted(
            {value for row in rows for value in row["battleActionConstants"]}
        ),
        "menuConstants": sorted(
            {value for row in rows for value in row["menuConstants"]}
        ),
        "functions": rows,
    }


def _player_control_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    source_0 = root / "battlefunctions_0.asm"
    source_2 = root / "battlefunctions_2.asm"
    _require_ordered_fragments(
        source_0,
        [
            "ControlCursorEntity:",
            "andi.w  #INPUT_B|INPUT_C|INPUT_A,d0",
            "move.b  d2,((BATTLE_ENTITY_CHOSEN_X-$1000000)).w",
            "move.b  d3,((BATTLE_ENTITY_CHOSEN_Y-$1000000)).w",
            "ControlCursorEntity_ChooseTarget:",
            "move.w  ((TARGETS_LIST_LENGTH-$1000000)).w,d7",
            "moveq   #-1,d0",
            "btst    #INPUT_BIT_UP,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_LEFT,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_DOWN,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_RIGHT,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_B,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_C,((CURRENT_PLAYER_INPUT-$1000000)).w",
            "btst    #INPUT_BIT_A,((CURRENT_PLAYER_INPUT-$1000000)).w",
        ],
    )
    _require_ordered_fragments(
        source_2,
        [
            "ProcessBattleEntityControlPlayerInput:",
            "btst    #INPUT_BIT_B,d4",
            "move.w  #-1,((CURRENT_BATTLEACTION-$1000000)).w",
            "jsr     j_ExecuteDiamondMenu",
            "tst.w   d0",
            "cmpi.w  #1,d0",
            "move.w  #BATTLEACTION_CAST_SPELL,((CURRENT_BATTLEACTION-$1000000)).w",
            "cmpi.w  #2,d0",
            "move.w  #BATTLEACTION_USE_ITEM,((CURRENT_BATTLEACTION-$1000000)).w",
            "EquipNewItemInBattle:",
            "jsr     j_UnequipItemBySlotIfNotCursed",
            "txt     43",
            "jsr     j_EquipItemBySlot",
            "txt     34",
            "btst    #ITEMTYPE_BIT_CURSED,ITEMDEF_OFFSET_TYPE(a0)",
            "txt     441             ; \"The equipment is cursed.{W1}\"",
            "jsr     j_RemoveItemBySlot",
            "bclr    #ITEMENTRY_BIT_EQUIPPED,d1",
            "jsr     j_AddItem",
            "move.w  #BATTLEACTION_STAY,((CURRENT_BATTLEACTION-$1000000)).w",
            "btst    #ITEMTYPE_BIT_UNSELLABLE,ITEMDEF_OFFSET_TYPE(a0)",
            "jsr     j_alt_YesNoPrompt",
            "btst    #ITEMTYPE_BIT_RARE,ITEMDEF_OFFSET_TYPE(a0)",
            "jsr     j_AddItemToDeals",
            "jsr     j_CheckForTrappedChest",
            "move.w  #BATTLEACTION_TRAPPED_CHEST,((CURRENT_BATTLEACTION-$1000000)).w",
            "bsr.w   SpawnEnemySkipCamera",
            "CheckGoldChest:",
            "cmpi.w  #ITEMINDEX_GOLDCHESTS_START,d2",
            "bsr.w   GetChestGoldAmount",
            "jsr     j_IncreaseGold",
            "jsr     j_AddItem",
            "jsr     (CloseChest).w",
            "BattlefieldMenu:",
            "moveq   #MENU_BATTLEFIELD,d2",
            "jsr     j_BuildMinimapScreen",
            "jsr     j_BuildBattlefieldSettingsScreen",
            "move.w  ((CURRENT_SAVE_SLOT-$1000000)).w,d0",
            "jsr     (SaveGame).l",
            "jmp     (WitchSuspend).w",
        ],
    )
    return {
        "cursorControl": {
            "confirmInputBits": ["A", "B", "C"],
            "storesDestinationAsChosenTile": True,
            "hidesCursorAfterSelection": True,
        },
        "targetSelection": {
            "emptyListReturn": -1,
            "cancelInput": "B",
            "confirmInputs": ["A", "C"],
            "directionInputsWrap": ["UP", "LEFT", "DOWN", "RIGHT"],
            "returnValueOnConfirm": "combatant-index",
        },
        "battleActionMenu": {
            "choiceOrder": ["attack", "magic", "item", "search-or-stay"],
            "cancelRestoresOriginalPosition": True,
            "movementCancelAction": -1,
            "itemChoiceOrder": ["use", "give", "equip", "drop"],
            "committedActions": [
                "attack",
                "cast-spell",
                "use-item",
                "stay",
                "trapped-chest",
            ],
        },
        "battlefieldMenu": {
            "choiceOrder": ["members", "minimap", "options", "suspend"],
            "battleZeroRejectsSuspend": True,
            "suspendCopiesSecondsAndSetsFlag88": True,
            "debugStartReturnsToMenuAfterSave": True,
            "normalSuspendTransfersToWitchSuspend": True,
        },
        "equipmentAndItems": {
            "curseBlocksEquipmentExchange": True,
            "newCursedEquipmentShowsCurseDialogue": True,
            "equippedCursedItemCannotBeGivenOrDropped": True,
            "giveToFullInventoryTradesItems": True,
            "transferredItemsClearEquippedBit": True,
            "giveCommitsStayAction": True,
            "dropRequiresConfirmation": True,
            "droppedRareItemMovesToDeals": True,
        },
        "chestSearch": {
            "noChestContentsCommitsStay": True,
            "emptyChestCommitsStay": True,
            "trapCommitsTrappedChestActionAndSpawnsEnemy": True,
            "goldChestUsesThresholdAndIncreaseGold": True,
            "itemChestAddsToActor": True,
            "fullInventoryClosesChestAndReturnsToMenu": True,
            "resolvedNonTrapChestCommitsStay": True,
        },
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
            "deeperBranchSemanticsRemainQueued": False,
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
    listing_path = disasm.parent / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle-functions H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    function_addresses = {
        field: addresses[symbol] for field, symbol in FUNCTION_ADDRESS_SYMBOLS.items()
    }
    player_control = _player_control_catalog(disasm, addresses)
    player_control["behaviorFacts"] = _player_control_facts(disasm)
    paths = sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if {Path(row["path"]).name for row in files} != set(REPRESENTATIVE_SYMBOLS):
        raise ValueError("battle-functions source file set drift")
    source_paths = [row["path"] for row in files]
    if len(source_paths) != len(set(source_paths)) or source_paths != sorted(source_paths):
        raise ValueError("battle-functions discovered source inventory drift")
    all_labels = {label for row in files for label in row["globalLabels"]}
    direct_calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            direct_calls[call["target"]] += call["siteCount"]
    indexed_records = _index_records_for_source_root(set(source_paths))
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
        "indexedRecordCount": len(indexed_records["indexedRecordIds"]),
        "indexedFileCount": len(indexed_records["indexedSourcePaths"]),
        "playerControlFunctionCount": player_control["summary"]["functionCount"],
        "playerControlStatementCount": player_control["summary"]["statementCount"],
        "playerControlBranchSiteCount": player_control["summary"]["branchSiteCount"],
        "playerControlGlobalStateCount": player_control["summary"]["globalStateCount"],
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "function": function_addresses,
        **indexed_records,
        "internalDirectCallTargets": sorted(t for t in direct_calls if t in all_labels),
        "externalDirectCallTargets": sorted(t for t in direct_calls if t not in all_labels),
        "functionFacts": _build_function_facts(disasm),
        "playerControl": player_control,
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
    disasm, _, _ = _resolve_upstream(upstream_path)
    discovered_source_paths = sorted(
        path.relative_to(disasm).as_posix()
        for path in (disasm / SOURCE_ROOT).glob("*.asm")
    )
    expected_indexed_records = _index_records_for_source_root(
        set(discovered_source_paths)
    )
    _verify_indexed_record_join(
        output, expected_indexed_records, discovered_source_paths
    )
    _verify_fixture_provenance(fixture, output)
    if fixture["function"] != output["function"]:
        raise ValueError("battle-functions function address drift")
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"battle-functions {field} drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle-functions static summary drift")
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(f"battle-functions representative symbol drift: {filename}::{symbol}")
    if output["functionFacts"] != fixture["expected"]["functionFacts"]:
        raise ValueError("battle-functions model drift")
    if output["playerControl"]["summary"] != fixture["expected"]["playerControlSummary"]:
        raise ValueError("battle player-control summary drift")
    if output["playerControl"]["behaviorFacts"] != fixture["expected"]["playerControlFacts"]:
        raise ValueError("battle player-control behavior drift")
    for fixture_field, output_field in (
        ("playerControlInputBits", "inputBits"),
        ("playerControlBattleActions", "battleActionConstants"),
        ("playerControlMenus", "menuConstants"),
        ("playerControlSelectedCallEdges", "selectedCallEdges"),
    ):
        if fixture["expected"][fixture_field] != output["playerControl"][output_field]:
            raise ValueError(f"battle player-control {output_field} drift")
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
