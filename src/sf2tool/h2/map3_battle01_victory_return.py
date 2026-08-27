"""Static Battle 01 victory, after-battle, and MainLoop return contract."""

from __future__ import annotations

import hashlib
import json
import re
import subprocess
from copy import deepcopy
from pathlib import Path
from typing import Any

from jsonschema import Draft7Validator

from sf2tool.h2.map3_battle01_turn_finalization import (
    FIXTURE as R3D_FIXTURE,
)
from sf2tool.h2.map3_battle01_turn_finalization import (
    build_map3_battle01_turn_finalization_static,
)
from sf2tool.h2.map_event_item_transactions import (
    normalize_map_event_item_transactions_later_owner_index,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

ID = "sf2-map3-battle01-victory-return-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map3-battle01-victory-return-static-v1.json")
SCHEMA = repo_path("schemas/h2/map3-battle01-victory-return-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")

_LISTING = Path("build/sf2build-h1.lst")
_H1_BINARY = Path("build/sf2build-h1.bin")
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"

_SOURCE_SURFACE = (
    "code/gameflow/battle/battleloop_2.asm",
    "code/gameflow/battle/battleloop/heallivingandimmortalallies.asm",
    "code/common/scripting/text/textfunctions_1.asm",
    "code/common/stats/combatantstats_1.asm",
    "code/gameflow/battle/battlefunctions/battlefunctions_0.asm",
    "code/common/stats/gameflags.asm",
    "code/gameflow/battle/cutscenes/afterbattlecutscenesstart.asm",
    "code/common/scripting/map/mapscriptengine_2.asm",
    "data/battles/cutscenes/afterbattlecutscenes.asm",
    "data/battles/entries/battle01/cs_afterbattle.asm",
    "code/gameflow/battle/cutscenes/afterbattlecutscenesend.asm",
    "data/battles/cutscenes/afterbattlejoins.asm",
    "code/common/stats/battleparty.asm",
    "code/gameflow/mainloop.asm",
    "code/common/maps/mapinit_0.asm",
    "code/gameflow/exploration/explorationfunctions_2.asm",
)

_FUNCTIONS = {
    "BattleLoop_Victory": 0x23CBA,
    "HealLivingAndImmortalAllies": 0x23BFC,
    "UpdateForceAndGetFirstBattlePartyMemberIndex": 0x64F6,
    "GetCombatantX": 0x8436,
    "GetCombatantY": 0x8448,
    "GetEntityIndexForCombatant": 0x22F30,
    "ExecuteAfterBattleCutscene": 0x47CBC,
    "ExecuteMapScript": 0x4712C,
    "EndAfterBattleCutscene": 0x47D54,
    "JoinForce": 0x9956,
    "MainLoop": 0x75C4,
    "SwitchMap": 0x7956,
    "ExplorationLoop": 0x257C0,
    "ClearFlag": 0x98D4,
    "SetFlag": 0x98C4,
    "BattleLoop": 0x23A84,
}

_ALIASES = {
    "j_GetCombatantX": (0x8014, "GetCombatantX"),
    "j_GetCombatantY": (0x8008, "GetCombatantY"),
    "j_ExecuteAfterBattleCutscene": (0x4404C, "ExecuteAfterBattleCutscene"),
    "j_ExecuteMapScript": (0x4403C, "ExecuteMapScript"),
    "j_ClearFlag": (0x826C, "ClearFlag"),
    "j_SetFlag": (0x8268, "SetFlag"),
    "j_JoinForce": (0x8274, "JoinForce"),
    "j_BattleLoop": (0x20034, "BattleLoop"),
    "j_ExplorationLoop": (0x20018, "ExplorationLoop"),
}

_ANCHORS = (
    ("victoryReturnSpine.functionAddresses.HealLivingAndImmortalAllies", 0x23BFC, 2, None),
    ("victoryReturnSpine.victoryBody.entryAddress", 0x23CBA, 0x8A, 0x23D44),
    ("victoryReturnSpine.victoryBody.fairyWoodsPredicate", 0x23CBE, 6, None),
    ("victoryReturnSpine.victoryBody.closeTimerCall", 0x23CC6, 6, None),
    ("victoryReturnSpine.victoryBody.savedMapWrite", 0x23CCC, 6, None),
    ("victoryReturnSpine.victoryBody.updateForceCall", 0x23CD2, 4, None),
    (
        "victoryReturnSpine.functionAddresses.UpdateForceAndGetFirstBattlePartyMemberIndex",
        0x64F6,
        2,
        None,
    ),
    ("victoryReturnSpine.victoryBody.getCombatantXCall", 0x23CD6, 6, None),
    ("victoryReturnSpine.functionAddresses.GetCombatantX", 0x8436, 2, None),
    ("victoryReturnSpine.victoryBody.getCombatantYCall", 0x23CE4, 6, None),
    ("victoryReturnSpine.functionAddresses.GetCombatantY", 0x8448, 2, None),
    ("victoryReturnSpine.victoryBody.getEntityIndexCall", 0x23CF2, 4, None),
    ("victoryReturnSpine.functionAddresses.GetEntityIndexForCombatant", 0x22F30, 2, None),
    ("victoryReturnSpine.victoryBody.entityIndexMapWriteAddress", 0x23D02, 6, None),
    ("victoryReturnSpine.victoryBody.afterBattleCallAddress", 0x23D08, 6, None),
    ("victoryReturnSpine.victoryBody.clearUnlockedFlagCallAddress", 0x23D18, 6, None),
    ("victoryReturnSpine.functionAddresses.ClearFlag", 0x98D4, 2, None),
    ("victoryReturnSpine.victoryBody.setCompletedFlagCallAddress", 0x23D22, 6, None),
    ("victoryReturnSpine.functionAddresses.SetFlag", 0x98C4, 2, None),
    ("victoryReturnSpine.victoryBody.resultAddress", 0x23D40, 2, None),
    ("victoryReturnSpine.victoryBody.returnAddress", 0x23D42, 2, None),
    ("victoryReturnSpine.afterBattleRouting.entryAddress", 0x47CBC, 0x38, 0x47CF4),
    ("victoryReturnSpine.afterBattleRouting.completionCheckAddress", 0x47CCA, 6, None),
    ("victoryReturnSpine.afterBattleRouting.completedBranchAddress", 0x47CD0, 4, None),
    ("victoryReturnSpine.afterBattleRouting.routeLoadAddress", 0x47CE0, 8, None),
    ("victoryReturnSpine.afterBattleRouting.tableAddress", 0x47CF4, 4, None),
    ("victoryReturnSpine.afterBattleRouting.battle01RowAddress", 0x47CF6, 2, None),
    ("victoryReturnSpine.afterBattleRouting.executeScriptCallAddress", 0x47CE8, 4, None),
    ("victoryReturnSpine.functionAddresses.ExecuteMapScript", 0x4712C, 2, None),
    ("victoryReturnSpine.afterBattleRouting.executeScriptResumeAddress", 0x47CEC, 8, None),
    ("victoryReturnSpine.afterBattleJoin.entryAddress", 0x47D54, 0x16, 0x47D6A),
    ("victoryReturnSpine.afterBattleJoin.joinCallAddress", 0x47D5E, 6, None),
    ("victoryReturnSpine.functionAddresses.JoinForce", 0x9956, 2, None),
    ("victoryReturnSpine.afterBattleJoin.returnAddress", 0x47D68, 2, None),
    ("victoryReturnSpine.afterBattleJoin.tableAddress", 0x47D6A, 2, None),
    ("victoryReturnSpine.afterBattleJoin.battle01RowAddress", 0x47D6B, 1, None),
    ("victoryReturnSpine.battle01AfterBattleProgram.entryAddress", 0x496DC, 0x132, 0x4980E),
    ("victoryReturnSpine.battle01AfterBattleProgram.programEndAddress", 0x497F4, 2, None),
    ("victoryReturnSpine.battle01AfterBattleProgram.entityTableAddress", 0x497F6, 2, None),
    ("victoryReturnSpine.battle01AfterBattleProgram.entityTableEndAddress", 0x4980C, 2, None),
    ("victoryReturnSpine.mainLoopReturn.mainLoopEntryAddress", 0x75C4, 2, None),
    ("victoryReturnSpine.mainLoopReturn.battleLoopCallAddress", 0x75DA, 6, None),
    ("victoryReturnSpine.mainLoopReturn.postBattleResumeAddress", 0x75E0, 4, None),
    ("victoryReturnSpine.functionAddresses.SwitchMap", 0x7956, 2, None),
    ("victoryReturnSpine.mainLoopReturn.explorationCallAddress", 0x75E4, 6, None),
    ("victoryReturnSpine.functionAddresses.ExplorationLoop", 0x257C0, 2, None),
)

_OWNER_RECORD_IDS = (
    "battle.control.outcomes",
    "battle.loop.heal-living-allies",
    "battle.cutscene.after-battle-start",
    "scripting.map.mapscriptengine-2",
    "battle.routing.after-battle",
    "battle.cutscene.data.battle01.afterbattle",
    "battle.cutscene.after-battle-end",
    "battle.routing.after-battle-joins",
    "gameflow.main-loop",
    "maps.switch-map",
    "gameflow.exploration.loop",
)

# This is deliberately a closed, accepted-base delta rather than a broad
# fixture-presence query.  Each tuple is (address ID, kind, ROM address,
# fixture field); the final hash proves the rest of the index record survived
# unchanged after the authorized document, addresses, and evidence are removed.
_INDEX_CONTRACT = {
    "battle.control.outcomes": (
        "code/gameflow/battle/battleloop_2.asm",
        (
            ("entry", "symbol", 0x23CBA, "victoryReturnSpine.victoryBody.entryAddress"),
            (
                "after-battle-call",
                "observation",
                0x23D08,
                "victoryReturnSpine.victoryBody.afterBattleCallAddress",
            ),
            (
                "clear-unlocked-call",
                "observation",
                0x23D18,
                "victoryReturnSpine.victoryBody.clearUnlockedFlagCallAddress",
            ),
            (
                "set-completed-call",
                "observation",
                0x23D22,
                "victoryReturnSpine.victoryBody.setCompletedFlagCallAddress",
            ),
            (
                "victory-result",
                "observation",
                0x23D40,
                "victoryReturnSpine.victoryBody.resultAddress",
            ),
            (
                "victory-return",
                "observation",
                0x23D42,
                "victoryReturnSpine.victoryBody.returnAddress",
            ),
        ),
        "6CE54A5E8258EB434827AF4F9EF9E73114B1FBF84C94DB7DD3C5D892352FF6CA",
    ),
    "battle.loop.heal-living-allies": (
        "code/gameflow/battle/battleloop/heallivingandimmortalallies.asm",
        (
            (
                "entry",
                "symbol",
                0x23BFC,
                "victoryReturnSpine.functionAddresses.HealLivingAndImmortalAllies",
            ),
        ),
        "A21C8BCDE8C516362F800D25628E0EB26462A13AFF534E2BF76AC251ECFF1E8E",
    ),
    "battle.cutscene.after-battle-start": (
        "code/gameflow/battle/cutscenes/afterbattlecutscenesstart.asm",
        (
            ("entry", "symbol", 0x47CBC, "victoryReturnSpine.afterBattleRouting.entryAddress"),
            (
                "completion-check",
                "observation",
                0x47CCA,
                "victoryReturnSpine.afterBattleRouting.completionCheckAddress",
            ),
            (
                "route-load",
                "observation",
                0x47CE0,
                "victoryReturnSpine.afterBattleRouting.routeLoadAddress",
            ),
            (
                "execute-script-call",
                "observation",
                0x47CE8,
                "victoryReturnSpine.afterBattleRouting.executeScriptCallAddress",
            ),
            (
                "execute-script-resume",
                "observation",
                0x47CEC,
                "victoryReturnSpine.afterBattleRouting.executeScriptResumeAddress",
            ),
        ),
        "4F2251549DF0103362D0EDDF692440D05E0616E691969486736DFDAD7295B43E",
    ),
    "scripting.map.mapscriptengine-2": (
        "code/common/scripting/map/mapscriptengine_2.asm",
        (("entry", "symbol", 0x4712C, "victoryReturnSpine.functionAddresses.ExecuteMapScript"),),
        "20D7B7415F49DD591963BD9D3A7D039DE9BD265A27C53F692E60E63A2A9250D3",
    ),
    "battle.routing.after-battle": (
        "data/battles/cutscenes/afterbattlecutscenes.asm",
        (
            ("entry", "symbol", 0x47CF4, "victoryReturnSpine.afterBattleRouting.tableAddress"),
            (
                "battle01-row",
                "observation",
                0x47CF6,
                "victoryReturnSpine.afterBattleRouting.battle01RowAddress",
            ),
        ),
        "F5645D32454F48328A13062EB6E7BBD260D2F5462D69B2262D9CD64EDD3A8CC1",
    ),
    "battle.cutscene.data.battle01.afterbattle": (
        "data/battles/entries/battle01/cs_afterbattle.asm",
        (
            (
                "entry",
                "symbol",
                0x496DC,
                "victoryReturnSpine.battle01AfterBattleProgram.entryAddress",
            ),
            (
                "program-end",
                "observation",
                0x497F4,
                "victoryReturnSpine.battle01AfterBattleProgram.programEndAddress",
            ),
            (
                "entity-table",
                "observation",
                0x497F6,
                "victoryReturnSpine.battle01AfterBattleProgram.entityTableAddress",
            ),
        ),
        "B2727C7DE6A822B309084CF3B2E8C53E2BC59075F969FDD4872E01EE67B45353",
    ),
    "battle.cutscene.after-battle-end": (
        "code/gameflow/battle/cutscenes/afterbattlecutscenesend.asm",
        (
            ("entry", "symbol", 0x47D54, "victoryReturnSpine.afterBattleJoin.entryAddress"),
            (
                "join-call",
                "observation",
                0x47D5E,
                "victoryReturnSpine.afterBattleJoin.joinCallAddress",
            ),
            ("return", "observation", 0x47D68, "victoryReturnSpine.afterBattleJoin.returnAddress"),
        ),
        "8637ABCAC9CB27287CF6A475E51A6F24A55B17E65392088108753BA9F5BDF63F",
    ),
    "battle.routing.after-battle-joins": (
        "data/battles/cutscenes/afterbattlejoins.asm",
        (
            ("entry", "symbol", 0x47D6A, "victoryReturnSpine.afterBattleJoin.tableAddress"),
            (
                "battle01-row",
                "observation",
                0x47D6B,
                "victoryReturnSpine.afterBattleJoin.battle01RowAddress",
            ),
        ),
        "7170C15A377F66AC496948A3DA4098BD3DE816B0D2A2857D76DD39D1AD13FDB6",
    ),
    "gameflow.main-loop": (
        "code/gameflow/mainloop.asm",
        (
            ("entry", "symbol", 0x75C4, "victoryReturnSpine.mainLoopReturn.mainLoopEntryAddress"),
            (
                "battle-loop-call",
                "observation",
                0x75DA,
                "victoryReturnSpine.mainLoopReturn.battleLoopCallAddress",
            ),
            (
                "post-battle-resume",
                "observation",
                0x75E0,
                "victoryReturnSpine.mainLoopReturn.postBattleResumeAddress",
            ),
            (
                "exploration-call",
                "observation",
                0x75E4,
                "victoryReturnSpine.mainLoopReturn.explorationCallAddress",
            ),
        ),
        "F6013E1F3EAFCC7143226AEC137A47FBBACABF30F58F505E0F2ACD31BA983341",
    ),
    "maps.switch-map": (
        "code/common/maps/mapinit_0.asm",
        (("entry", "symbol", 0x7956, "victoryReturnSpine.functionAddresses.SwitchMap"),),
        "9FCD04B675D6D2129A46008EA7056C04E9882C45740E7839521F233858D9114A",
    ),
    "gameflow.exploration.loop": (
        "code/gameflow/exploration/explorationfunctions_2.asm",
        (("entry", "symbol", 0x257C0, "victoryReturnSpine.functionAddresses.ExplorationLoop"),),
        "77EB4C67AC40CADF12CDC0CF6702774825A20F7DA58A6ABE403E50FE77211F3C",
    ),
}
_OWNER_DOCUMENT = "docs/research/map3-battle01-victory-return.md"

_REQUEST_CONSUMPTION_FIXTURE_ID = "sf2-map-event-request-consumption-static-v1"
_REQUEST_CONSUMPTION_FIXTURE = "tests/fixtures/h2/map-event-request-consumption-static-v1.json"
_REQUEST_CONSUMPTION_VERIFIER = "src/sf2tool/h2/map_event_request_consumption.py"
_REQUEST_CONSUMPTION_DOCUMENT = "docs/research/map-event-request-consumption.md"
_REQUEST_CONSUMPTION_BINDINGS = {
    "menus.shop-actions": (
        (
            "get-shop-inventory-address",
            "eventRequestConsumption.consumerContexts.getShopInventoryAddress.entryAddress",
        ),
    ),
    "gameflow.exploration.loop": (
        ("entry", "eventRequestConsumption.consumerContexts.explorationLoop.entryAddress"),
        ("wait-for-event", "eventRequestConsumption.consumerContexts.waitForEvent.entryAddress"),
        (
            "process-map-event",
            "eventRequestConsumption.consumerContexts.processMapEvent.entryAddress",
        ),
    ),
    "menus.field-main": (
        ("entry", "eventRequestConsumption.consumerContexts.fieldMenu.entryAddress"),
    ),
    "battle.loop.egress-position": (
        (
            "entry",
            "eventRequestConsumption.consumerContexts.getEgressPositionForBattle.entryAddress",
        ),
    ),
    "scripting.map.mapfunctions": (
        (
            "declare-raft-entity",
            "eventRequestConsumption.consumerContexts.declareRaftEntity.entryAddress",
        ),
    ),
    "scripting.map.followersfunctions-2": (
        ("raft-refresh", "eventRequestConsumption.consumerContexts.raftRefresh.entryAddress"),
    ),
}
_REQUEST_CONSUMPTION_ADDRESSES = {
    ("menus.shop-actions", "get-shop-inventory-address", 133202),
    ("gameflow.exploration.loop", "process-map-event", 153930),
    ("scripting.map.mapfunctions", "declare-raft-entity", 278954),
    ("scripting.map.followersfunctions-2", "raft-refresh", 279556),
}
_REQUEST_CONSUMPTION_ADDRESS_IDS = frozenset(
    address_id for _, address_id, _ in _REQUEST_CONSUMPTION_ADDRESSES
)

_UNKNOWN_KEYS = (
    "naturalContinuity",
    "initializedSnapshot",
    "naturalFirstActor",
    "actorControlBranch",
    "playerInputChronology",
    "aiCommandSelected",
    "movementPath",
    "target",
    "action",
    "preResolutionArrival",
    "dispatchBranchReached",
    "perTargetResult",
    "statusOutcome",
    "targetDeath",
    "expAward",
    "goldAward",
    "dropOutcome",
    "followupOutcome",
    "postEffectArrival",
    "afterTurn",
    "multiRoundPlaythrough",
    "victory",
    "playerReady",
    "primaryTargetLoopCompletion",
    "doubleAttackReached",
    "counterAttackReached",
    "explosionReached",
    "itemBreakOutcome",
    "actionConstructionCompletion",
    "writeBattlesceneReturn",
    "battleSceneReplay",
    "executeIndividualTurnReturn",
    "nextTurnDispatch",
    "battleSceneMusicBranch",
    "battleSceneInitialization",
    "battleSceneTeardown",
    "battlefieldReload",
    "defeatedCutscene",
    "preAfterTurnOutcomeGate",
    "postAfterTurnOutcomeGate",
    "afterBattleCutsceneReached",
    "afterBattleProgramReached",
    "afterBattleProgramCompleted",
    "afterBattleJoinOutcome",
    "victoryFlagMutationsReached",
    "victoryReturnReached",
    "mainLoopPostBattleResume",
    "postVictorySwitchMapOutcome",
    "explorationReentry",
    "postVictoryEndpointState",
)

_PROGRAM_FORMS = (
    "textCursor",
    "resetForceBattleStats",
    "loadMapFadeIn",
    "loadMapEntities",
    "setActscriptWait",
    "setPos",
    "fadeInB",
    "csWait",
    "nextSingleText",
    "setFacing",
    "shiver",
    "mapFadeOutToWhite",
    "mapFadeInFromWhite",
    "setActscript",
    "animEntityFX",
    "setSprite",
    "entityActionsWait",
    "moveUp",
    "endActions",
    "moveDown",
    "setCamDest",
    "entityActions",
    "hide",
    "csc_end",
    "mainEntity",
    "entity",
    "cscEntitiesEnd",
)
_PROGRAM_OPERATIONS_SHA256 = "EA23D2F0D4E52505FC2D6D26E2BD69DA2E0A3B1D4B3ECD42572CA0AE74BD25DC"


def _canonical(value: Any) -> bytes:
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8")


def _disasm_root(upstream: Path) -> Path:
    return upstream / "disasm" if (upstream / "disasm").is_dir() else upstream


def _read_source_surface(root: Path) -> tuple[dict[str, str], list[dict[str, str]]]:
    text: dict[str, str] = {}
    identities: list[dict[str, str]] = []
    for relative in _SOURCE_SURFACE:
        path = root / relative
        if not path.is_file():
            raise ValueError(f"Map 3 Battle 01 victory/return source missing: {relative}")
        data = path.read_bytes()
        text[relative] = data.decode("utf-8").replace("\r\n", "\n")
        identities.append({"path": relative, "sha256": hashlib.sha256(data).hexdigest().upper()})
    if len(identities) != 16:
        raise ValueError("Map 3 Battle 01 victory/return source denominator drift")
    return text, identities


def _anchor_projection(h1_binary: bytes, rom: bytes) -> list[dict[str, Any]]:
    anchors: list[dict[str, Any]] = []
    for identifier, address, width, end in _ANCHORS:
        h1 = h1_binary[address : address + width]
        if len(h1) != width or rom[address : address + width] != h1:
            raise ValueError(f"Map 3 Battle 01 victory/return H1/ROM anchor drift: {identifier}")
        item: dict[str, Any] = {
            "id": identifier,
            "address": address,
            "width": width,
            "sha256": hashlib.sha256(h1).hexdigest().upper(),
        }
        if end is not None:
            item["endAddressExclusive"] = end
        anchors.append(item)
    if len(anchors) != 46:
        raise ValueError("Map 3 Battle 01 victory/return H1/ROM anchor denominator drift")
    return anchors


def _without_comments(source: str) -> list[str]:
    return [line.split(";", maxsplit=1)[0].rstrip() for line in source.splitlines()]


def _normal(line: str) -> str:
    return re.sub(r"\s*,\s*", ",", " ".join(line.split()))


def _function_section(source: str, entry: str, context: str) -> str:
    lines = source.splitlines()
    matches = [index for index, line in enumerate(lines) if _normal(line).strip() == entry]
    if len(matches) != 1:
        raise ValueError(f"Map 3 Battle 01 victory/return source-use drift in {context}: {entry}")
    for index in range(matches[0] + 1, len(lines)):
        if "End of function" in lines[index] or "END OF FUNCTION CHUNK" in lines[index]:
            return "\n".join(lines[matches[0] : index])
    raise ValueError(f"Map 3 Battle 01 victory/return source-use drift in {context}: end")


def _require_order(source: str, entry: str, expected: tuple[str, ...], context: str) -> None:
    lines = [_normal(line) for line in _without_comments(_function_section(source, entry, context))]
    cursor = 0
    for fragment in expected:
        for found in range(cursor, len(lines)):
            if lines[found] == fragment:
                break
        else:
            raise ValueError(
                f"Map 3 Battle 01 victory/return source-use drift in {context}: {fragment}"
            )
        cursor = found + 1


def _program_operations(source: str) -> tuple[list[dict[str, Any]], list[str]]:
    started = False
    operations: list[dict[str, Any]] = []
    for raw in _without_comments(source):
        line = raw.strip()
        if not line:
            continue
        label = re.match(r"^(?P<label>[A-Za-z0-9_]+):(?P<rest>.*)$", line)
        if label is not None:
            if label["label"] == "abcs_battle01":
                started = True
            line = label["rest"].strip()
        if not started or not line:
            continue
        parts = line.split(None, 1)
        command = parts[0]
        operands = [] if len(parts) == 1 else [part.strip() for part in parts[1].split(",")]
        row = {"id": len(operations), "command": command, "operands": operands}
        row["sha256"] = hashlib.sha256(_canonical(row)).hexdigest().upper()
        operations.append(row)
        if command == "cscEntitiesEnd":
            break
    forms = list(dict.fromkeys(row["command"] for row in operations))
    if len(operations) != 80 or tuple(forms) != _PROGRAM_FORMS:
        raise ValueError("Map 3 Battle 01 victory/return program corpus denominator/order drift")
    if operations[-1]["command"] != "cscEntitiesEnd":
        raise ValueError("Map 3 Battle 01 victory/return program boundary drift")
    return operations, forms


def _table_rows(source: str, directive: str) -> list[str]:
    rows: list[str] = []
    for raw in _without_comments(source):
        line = raw.strip()
        if not line:
            continue
        match = re.match(r"^[A-Za-z0-9_]+:\s*(.*)$", line)
        if match is not None:
            line = match[1].strip()
        if line.startswith(directive):
            rows.append(_normal(line[len(directive) :].strip()))
    return rows


def _validate_source_contract(text: dict[str, str]) -> dict[str, Any]:
    victory = text[_SOURCE_SURFACE[0]]
    _require_order(
        victory,
        "BattleLoop_Victory:",
        (
            "BattleLoop_Victory:",
            "bsr.w HealLivingAndImmortalAllies",
            "cmpi.b #BATTLE_FAIRY_WOODS,((CURRENT_BATTLE-$1000000)).w",
            "bne.s @Continue",
            "jsr j_CloseTimerWindow",
            "move.b ((CURRENT_MAP-$1000000)).w,((MAP_EVENT_PARAM_2-$1000000)).w",
            "jsr (UpdateForceAndGetFirstBattlePartyMemberIndex).w",
            "jsr j_GetCombatantX",
            "move.b d1,((MAP_EVENT_PARAM_3-$1000000)).w",
            "jsr j_GetCombatantY",
            "move.b d1,((MAP_EVENT_PARAM_4-$1000000)).w",
            "bsr.w GetEntityIndexForCombatant",
            "lsl.w #ENTITYDEF_SIZE_BITS,d0",
            "lea ((ENTITY_DATA-$1000000)).w,a0",
            "move.b ENTITYDEF_OFFSET_FACING(a0,d0.w),((MAP_EVENT_PARAM_5-$1000000)).w",
            "move.b #0,((MAP_EVENT_PARAM_1-$1000000)).w",
            "jsr j_ExecuteAfterBattleCutscene",
            "clr.w d1",
            "addi.w #BATTLE_UNLOCKED_FLAGS_START,d1",
            "jsr j_ClearFlag",
            "addi.w #BATTLE_UNLOCKED_TO_COMPLETED_FLAGS_OFFSET,d1",
            "jsr j_SetFlag",
            "moveq #1,d4",
            "rts",
        ),
        "victory body",
    )
    _require_order(
        text[_SOURCE_SURFACE[1]],
        "HealLivingAndImmortalAllies:",
        (
            "HealLivingAndImmortalAllies:",
            "jsr j_GetMaxHp",
            "jsr j_SetCurrentHp",
            "jsr j_GetMaxMp",
            "jsr j_SetCurrentMp",
            "jsr j_GetStatusEffects",
            "jsr j_SetStatusEffects",
            "jsr j_UpdateCombatantStats",
            "rts",
        ),
        "victory heal helper",
    )
    _require_order(
        text[_SOURCE_SURFACE[2]],
        "UpdateForceAndGetFirstBattlePartyMemberIndex:",
        (
            "UpdateForceAndGetFirstBattlePartyMemberIndex:",
            "jsr j_UpdateForce",
            "move.b (BATTLE_PARTY_MEMBERS).l,d0",
            "rts",
        ),
        "victory force helper",
    )
    stats = text[_SOURCE_SURFACE[3]]
    _require_order(
        stats,
        "GetCombatantX:",
        ("GetCombatantX:", "moveq #COMBATANT_OFFSET_X,d7", "rts"),
        "GetCombatantX",
    )
    _require_order(
        stats,
        "GetCombatantY:",
        ("GetCombatantY:", "moveq #COMBATANT_OFFSET_Y,d7", "rts"),
        "GetCombatantY",
    )
    _require_order(
        text[_SOURCE_SURFACE[4]],
        "GetEntityIndexForCombatant:",
        (
            "GetEntityIndexForCombatant:",
            "lea ((ENTITY_INDEX_LIST-$1000000)).w,a0",
            "rts",
        ),
        "GetEntityIndexForCombatant",
    )
    flags = text[_SOURCE_SURFACE[5]]
    _require_order(
        flags, "SetFlag:", ("SetFlag:", "bsr.w GetFlag", "or.b d0,(a0)", "rts"), "SetFlag"
    )
    _require_order(
        flags,
        "ClearFlag:",
        ("ClearFlag:", "bsr.w GetFlag", "eori.b #$FF,d0", "and.b d0,(a0)", "rts"),
        "ClearFlag",
    )
    _require_order(
        text[_SOURCE_SURFACE[6]],
        "ExecuteAfterBattleCutscene:",
        (
            "ExecuteAfterBattleCutscene:",
            "addi.w #BATTLE_COMPLETED_FLAGS_START,d1",
            "jsr j_CheckFlag",
            "bne.w EndAfterBattleCutscene",
            "move.w rpt_AfterBattleCutscenes(pc,d0.w),d0",
            "bsr.w ExecuteMapScript",
            "bra.w EndAfterBattleCutscene",
        ),
        "after-battle routing",
    )
    _require_order(
        text[_SOURCE_SURFACE[7]],
        "ExecuteMapScript:",
        (
            "ExecuteMapScript:",
            "move.w (a6)+,d0",
            "cmpi.w #-1,d0",
            "add.w d0,d0",
            "jsr rjt_cutsceneScriptCommands(pc,d0.w)",
            "rts",
        ),
        "ExecuteMapScript",
    )
    route_rows = _table_rows(text[_SOURCE_SURFACE[8]], "dc.w")
    if len(route_rows) != 48 or route_rows[1] != "abcs_battle01-rpt_AfterBattleCutscenes":
        raise ValueError("Map 3 Battle 01 victory/return after-battle route table drift")
    operations, forms = _program_operations(text[_SOURCE_SURFACE[9]])
    if hashlib.sha256(_canonical(operations)).hexdigest().upper() != _PROGRAM_OPERATIONS_SHA256:
        raise ValueError("Map 3 Battle 01 victory/return source operation corpus drift")
    _require_order(
        text[_SOURCE_SURFACE[10]],
        "EndAfterBattleCutscene:",
        (
            "EndAfterBattleCutscene:",
            "move.b table_AfterBattleJoins(pc,d0.w),d0",
            "jsr j_JoinForce",
            "rts",
        ),
        "after-battle join",
    )
    joins = _table_rows(text[_SOURCE_SURFACE[11]], "dc.b")
    if len(joins) != 52 or any(row != "0" for row in joins):
        raise ValueError("Map 3 Battle 01 victory/return after-battle join table drift")
    _require_order(
        text[_SOURCE_SURFACE[12]],
        "JoinForce:",
        (
            "JoinForce:",
            "addi.w #FORCEMEMBER_JOINED_FLAGS_START,d1",
            "bsr.w SetFlag",
            "rts",
        ),
        "JoinForce",
    )
    _require_order(
        text[_SOURCE_SURFACE[13]],
        "MainLoop:",
        (
            "MainLoop:",
            "bsr.w SwitchMap",
            "bsr.w CheckBattle",
            "cmpi.w #-1,d7",
            "jsr j_BattleLoop",
            "alt_MainLoopEntry:",
            "bsr.w SwitchMap",
            "jsr j_ExplorationLoop",
            "bra.s @Start",
        ),
        "MainLoop return",
    )
    _require_order(
        text[_SOURCE_SURFACE[14]],
        "SwitchMap:",
        (
            "SwitchMap:",
            "lea table_FlagSwitchedMaps(pc),a0",
            "jsr j_CheckFlag",
            "rts",
        ),
        "SwitchMap",
    )
    _require_order(
        text[_SOURCE_SURFACE[15]],
        "ExplorationLoop:",
        (
            "ExplorationLoop:",
            "clr.w ((MAP_EVENT_TYPE-$1000000)).w",
            "jsr HealLivingAndImmortalAllies",
        ),
        "ExplorationLoop",
    )
    return {"operations": operations, "commandForms": forms}


def _word(data: bytes, address: int) -> int:
    return int.from_bytes(data[address : address + 2], "big")


def _long(data: bytes, address: int) -> int:
    return int.from_bytes(data[address : address + 4], "big")


def _require_alias(data: bytes, alias: str) -> None:
    address, target = _ALIASES[alias]
    if _word(data, address) != 0x4EFA:
        raise ValueError(f"Map 3 Battle 01 victory/return alias opcode drift: {alias}")
    resolved = address + 2 + int.from_bytes(data[address + 2 : address + 4], "big", signed=True)
    if resolved != _FUNCTIONS[target]:
        raise ValueError(f"Map 3 Battle 01 victory/return alias target drift: {alias}")


def _require_long_jsr(data: bytes, address: int, target: int) -> None:
    if _word(data, address) != 0x4EB9 or _long(data, address + 2) != target:
        raise ValueError(f"Map 3 Battle 01 victory/return long JSR drift at {address:#x}")


def _require_bsr(data: bytes, address: int, target: int) -> None:
    if _word(data, address) != 0x6100:
        raise ValueError(f"Map 3 Battle 01 victory/return BSR opcode drift at {address:#x}")
    resolved = address + 2 + int.from_bytes(data[address + 2 : address + 4], "big", signed=True)
    if resolved != target:
        raise ValueError(f"Map 3 Battle 01 victory/return BSR target drift at {address:#x}")


def _parse_h1(h1: bytes, program: dict[str, Any]) -> dict[str, Any]:
    if (
        program.get("commandForms") != list(_PROGRAM_FORMS)
        or [row.get("id") for row in program.get("operations", [])] != list(range(80))
        or hashlib.sha256(_canonical(program.get("operations", []))).hexdigest().upper()
        != _PROGRAM_OPERATIONS_SHA256
    ):
        raise ValueError("Map 3 Battle 01 victory/return program corpus contract drift")
    _require_bsr(h1, 0x23CBA, _FUNCTIONS["HealLivingAndImmortalAllies"])
    if h1[0x23CBE:0x23CC6] != bytes.fromhex("0C38002CF7126606"):
        raise ValueError("Map 3 Battle 01 victory/return victory branch polarity drift")
    if (
        _word(h1, 0x23CD2) != 0x4EB8
        or _word(h1, 0x23CD4) != _FUNCTIONS["UpdateForceAndGetFirstBattlePartyMemberIndex"]
    ):
        raise ValueError("Map 3 Battle 01 victory/return force helper call drift")
    for address, alias in (
        (0x23CD6, "j_GetCombatantX"),
        (0x23CE4, "j_GetCombatantY"),
        (0x23D08, "j_ExecuteAfterBattleCutscene"),
    ):
        _require_long_jsr(h1, address, _ALIASES[alias][0])
        _require_alias(h1, alias)
    _require_bsr(h1, 0x23CF2, _FUNCTIONS["GetEntityIndexForCombatant"])
    if h1[0x23D0E:0x23D18] != bytes.fromhex("42411238F71206410190"):
        raise ValueError("Map 3 Battle 01 victory/return F401 arithmetic drift")
    _require_long_jsr(h1, 0x23D18, _ALIASES["j_ClearFlag"][0])
    _require_alias(h1, "j_ClearFlag")
    if h1[0x23D1E:0x23D22] != bytes.fromhex("06410064"):
        raise ValueError("Map 3 Battle 01 victory/return F501 arithmetic drift")
    _require_long_jsr(h1, 0x23D22, _ALIASES["j_SetFlag"][0])
    _require_alias(h1, "j_SetFlag")
    if h1[0x23D40:0x23D44] != bytes.fromhex("78014E75"):
        raise ValueError("Map 3 Battle 01 victory/return D4 result/RTS drift")
    _require_long_jsr(h1, 0x47CCA, 0x8264)
    if h1[0x47CD0:0x47CD4] != bytes.fromhex("66000082"):
        raise ValueError("Map 3 Battle 01 victory/return completed route polarity drift")
    if h1[0x47CD8:0x47CE8] != bytes.fromhex("42401038F712D040303B001241FB000E"):
        raise ValueError("Map 3 Battle 01 victory/return route load drift")
    _require_bsr(h1, 0x47CE8, _FUNCTIONS["ExecuteMapScript"])
    if _word(h1, 0x47CF6) != 0x19E8 or 0x47CF4 + _word(h1, 0x47CF6) != 0x496DC:
        raise ValueError("Map 3 Battle 01 victory/return Battle01 route row drift")
    _require_long_jsr(h1, 0x47D5E, _ALIASES["j_JoinForce"][0])
    _require_alias(h1, "j_JoinForce")
    if h1[0x47D68:0x47D6B] != bytes.fromhex("4E7500") or h1[0x47D6B] != 0:
        raise ValueError("Map 3 Battle 01 victory/return join return/row drift")
    if (
        h1[0x497F4:0x497F6] != bytes.fromhex("FFFF")
        or h1[0x4980C:0x4980E] != bytes.fromhex("FFFF")
        or len(program["operations"]) != 80
    ):
        raise ValueError("Map 3 Battle 01 victory/return program range drift")
    _require_long_jsr(h1, 0x75DA, _ALIASES["j_BattleLoop"][0])
    _require_alias(h1, "j_BattleLoop")
    _require_bsr(h1, 0x75E0, _FUNCTIONS["SwitchMap"])
    _require_long_jsr(h1, 0x75E4, _ALIASES["j_ExplorationLoop"][0])
    _require_alias(h1, "j_ExplorationLoop")
    return {
        "functionAddresses": dict(_FUNCTIONS),
        "victoryBody": {
            "entryAddress": 0x23CBA,
            "entityIndexMapWriteAddress": 0x23D02,
            "afterBattleCallAddress": 0x23D08,
            "clearUnlockedFlagCallAddress": 0x23D18,
            "setCompletedFlagCallAddress": 0x23D22,
            "unlockedFlag": 401,
            "completedFlag": 501,
            "resultRegister": {"name": "d4", "value": 1},
            "resultAddress": 0x23D40,
            "returnAddress": 0x23D42,
            "orderedSteps": [
                "HealLivingAndImmortalAllies",
                "ExecuteAfterBattleCutscene",
                "ClearFlag",
                "SetFlag",
                "d4=1",
                "RTS",
            ],
        },
        "afterBattleRouting": {
            "entryAddress": 0x47CBC,
            "completionCheckAddress": 0x47CCA,
            "routeLoadAddress": 0x47CE0,
            "executeScriptCallAddress": 0x47CE8,
            "executeScriptResumeAddress": 0x47CEC,
            "tableAddress": 0x47CF4,
            "battle01RowAddress": 0x47CF6,
            "battle01RowIndex": 1,
            "battle01ProgramAddress": 0x496DC,
        },
        "battle01AfterBattleProgram": {
            "entryAddress": 0x496DC,
            "programEndAddress": 0x497F4,
            "entityTableAddress": 0x497F6,
            "operations": program["operations"],
            "commandForms": program["commandForms"],
            "operationsSha256": hashlib.sha256(_canonical(program["operations"]))
            .hexdigest()
            .upper(),
        },
        "afterBattleJoin": {
            "entryAddress": 0x47D54,
            "joinCallAddress": 0x47D5E,
            "returnAddress": 0x47D68,
            "tableAddress": 0x47D6A,
            "battle01RowAddress": 0x47D6B,
            "battle01RowValue": 0,
        },
        "mainLoopReturn": {
            "mainLoopEntryAddress": 0x75C4,
            "battleLoopCallAddress": 0x75DA,
            "postBattleResumeAddress": 0x75E0,
            "switchMapCallAddress": 0x75E0,
            "explorationCallAddress": 0x75E4,
            "explorationCallTarget": 0x257C0,
            "explorationTargetNotEntered": True,
            "orderedSteps": ["BattleLoop", "SwitchMap", "ExplorationLoop"],
        },
    }


def _retained_r3d(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    fixture = load_json(R3D_FIXTURE)
    fresh = build_map3_battle01_turn_finalization_static(rom_path, upstream_path)
    if fixture.get("id") != "sf2-map3-battle01-turn-finalization-static-v1" or fixture != fresh:
        raise ValueError("Map 3 Battle 01 victory/return retained R3d projection drift")
    projection = {
        "fixtureId": fixture["id"],
        "fixtureSha256": hashlib.sha256(R3D_FIXTURE.read_bytes()).hexdigest().upper(),
        "turnFinalizationSpineSha256": hashlib.sha256(_canonical(fresh["turnFinalizationSpine"]))
        .hexdigest()
        .upper(),
    }
    projection["sha256"] = hashlib.sha256(_canonical(projection)).hexdigest().upper()
    return projection


def _retained_fixture(path: str, fixture_id: str) -> dict[str, str]:
    fixture_path = repo_path(path)
    fixture = load_json(fixture_path)
    if fixture.get("id") != fixture_id:
        raise ValueError(
            f"Map 3 Battle 01 victory/return retained owner identity drift: {fixture_id}"
        )
    projection = {
        "fixtureId": fixture_id,
        "fixtureSha256": hashlib.sha256(fixture_path.read_bytes()).hexdigest().upper(),
        "semanticSha256": hashlib.sha256(_canonical(fixture)).hexdigest().upper(),
    }
    projection["sha256"] = hashlib.sha256(_canonical(projection)).hexdigest().upper()
    return projection


def _retained_owners() -> dict[str, dict[str, str]]:
    owners = {
        "battleControl": (
            "tests/fixtures/h2/battle-control-static-v1.json",
            "sf2-battle-control-static-v1",
        ),
        "battleLoop": ("tests/fixtures/h2/battle-loop-static-v1.json", "sf2-battle-loop-static-v1"),
        "battleCutscenes": (
            "tests/fixtures/h2/battle-cutscenes-static-v1.json",
            "sf2-battle-cutscenes-static-v1",
        ),
        "battleRoutingData": (
            "tests/fixtures/h2/battle-routing-data-static-v1.json",
            "sf2-battle-routing-data-static-v1",
        ),
        "battleCutsceneData": (
            "tests/fixtures/h2/battle-cutscene-data-static-v1.json",
            "sf2-battle-cutscene-data-static-v1",
        ),
        "mapScriptEngine": (
            "tests/fixtures/h2/map-script-engine-static-v1.json",
            "sf2-map-script-engine-static-v1",
        ),
        "commonScripting": (
            "tests/fixtures/h2/common-scripting-static-v1.json",
            "sf2-common-scripting-static-v1",
        ),
        "commonStats": (
            "tests/fixtures/h2/common-stats-static-v1.json",
            "sf2-common-stats-static-v1",
        ),
        "battleFunctions": (
            "tests/fixtures/h2/battle-functions-static-v1.json",
            "sf2-battle-functions-static-v1",
        ),
        "gameflowCore": (
            "tests/fixtures/h2/gameflow-core-static-v1.json",
            "sf2-gameflow-core-static-v1",
        ),
        "commonMaps": ("tests/fixtures/h2/common-maps-static-v1.json", "sf2-common-maps-static-v1"),
    }
    return {key: _retained_fixture(*value) for key, value in owners.items()}


def _remove_request_consumption_later_owner_index_delta(
    index: dict[str, Any], *, require_document_terminal: bool
) -> dict[str, Any]:
    """Validate and remove only the exact request-consumption later-owner delta."""
    records = index.get("records")
    if not isinstance(records, list):
        raise ValueError("Map 3 Battle 01 victory/return request-consumption index shape drift")

    removed_records: set[str] = set()
    removed_addresses: set[tuple[str, str, int]] = set()
    removed_address_rows: list[tuple[str, str, int]] = []
    for record in records:
        record_id = record.get("id")
        evidence_items = record.get("evidence")
        documents = record.get("documents")
        addresses = record.get("addresses")
        if not isinstance(record_id, str):
            raise ValueError(
                "Map 3 Battle 01 victory/return request-consumption record identity drift"
            )
        if not isinstance(evidence_items, list):
            raise ValueError(
                "Map 3 Battle 01 victory/return request-consumption evidence shape drift"
            )
        if not isinstance(addresses, list):
            raise ValueError(
                "Map 3 Battle 01 victory/return request-consumption address shape drift"
            )
        request_evidence = [
            item
            for item in evidence_items
            if isinstance(item, dict) and item.get("fixtureId") == _REQUEST_CONSUMPTION_FIXTURE_ID
        ]
        if not request_evidence:
            if isinstance(documents, list) and _REQUEST_CONSUMPTION_DOCUMENT in documents:
                raise ValueError(
                    "Map 3 Battle 01 victory/return request-consumption document owner drift"
                )
            if any(
                isinstance(address, dict) and address.get("id") in _REQUEST_CONSUMPTION_ADDRESS_IDS
                for address in addresses
            ):
                raise ValueError(
                    "Map 3 Battle 01 victory/return request-consumption address owner drift"
                )
            continue
        binding_rows = _REQUEST_CONSUMPTION_BINDINGS.get(record_id)
        if binding_rows is None:
            raise ValueError(
                "Map 3 Battle 01 victory/return request-consumption owner-record drift"
            )
        expected_evidence = {
            "level": "H2",
            "fixture": _REQUEST_CONSUMPTION_FIXTURE,
            "fixtureId": _REQUEST_CONSUMPTION_FIXTURE_ID,
            "verifier": _REQUEST_CONSUMPTION_VERIFIER,
            "bindings": [
                {"addressId": address_id, "fixtureField": fixture_field}
                for address_id, fixture_field in binding_rows
            ],
        }
        if len(request_evidence) != 1 or request_evidence[0] != expected_evidence:
            raise ValueError("Map 3 Battle 01 victory/return request-consumption evidence drift")
        if not isinstance(documents, list) or documents.count(_REQUEST_CONSUMPTION_DOCUMENT) != 1:
            raise ValueError("Map 3 Battle 01 victory/return request-consumption document drift")
        if require_document_terminal and documents[-1] != _REQUEST_CONSUMPTION_DOCUMENT:
            raise ValueError(
                "Map 3 Battle 01 victory/return request-consumption document order drift"
            )
        retained_addresses = []
        for address in addresses:
            if not isinstance(address, dict):
                raise ValueError(
                    "Map 3 Battle 01 victory/return request-consumption address shape drift"
                )
            address_id = address.get("id")
            if address_id not in _REQUEST_CONSUMPTION_ADDRESS_IDS:
                retained_addresses.append(address)
                continue
            candidate = (record_id, address_id, address.get("value"))
            if candidate not in _REQUEST_CONSUMPTION_ADDRESSES:
                raise ValueError("Map 3 Battle 01 victory/return request-consumption address drift")
            if address.get("space") != "rom" or address.get("kind") != "observation":
                raise ValueError("Map 3 Battle 01 victory/return request-consumption address drift")
            removed_addresses.add(candidate)
            removed_address_rows.append(candidate)

        record["evidence"] = [item for item in evidence_items if item not in request_evidence]
        documents.remove(_REQUEST_CONSUMPTION_DOCUMENT)
        record["addresses"] = retained_addresses
        removed_records.add(record_id)

    if removed_records != set(_REQUEST_CONSUMPTION_BINDINGS):
        raise ValueError(
            "Map 3 Battle 01 victory/return request-consumption owner-record set drift"
        )
    if removed_addresses != _REQUEST_CONSUMPTION_ADDRESSES or len(removed_address_rows) != len(
        _REQUEST_CONSUMPTION_ADDRESSES
    ):
        raise ValueError("Map 3 Battle 01 victory/return request-consumption address set drift")
    return index


def _normalize_request_consumption_later_owner_index(index: dict[str, Any]) -> dict[str, Any]:
    """Remove only the exact later request-consumption delta before R4a checks."""
    _remove_request_consumption_later_owner_index_delta(
        deepcopy(index), require_document_terminal=False
    )
    normalized = normalize_map_event_item_transactions_later_owner_index(index)
    return _remove_request_consumption_later_owner_index_delta(
        normalized, require_document_terminal=True
    )


def _owner_evidence(index: dict[str, Any]) -> list[dict[str, Any]]:
    records = {record["id"]: record for record in index["records"]}
    if len(records) != len(index["records"]):
        raise ValueError("Map 3 Battle 01 victory/return duplicate research-index record ID")
    evidence: list[dict[str, Any]] = []
    for record_id in _OWNER_RECORD_IDS:
        record = records.get(record_id)
        if record is None:
            raise ValueError(f"Map 3 Battle 01 victory/return owner record drift: {record_id}")
        source_path, addresses, baseline = _INDEX_CONTRACT[record_id]
        if record.get("sourcePath") != source_path:
            raise ValueError(f"Map 3 Battle 01 victory/return owner source drift: {record_id}")
        expected_addresses = [
            {"id": address_id, "space": "rom", "kind": kind, "value": value}
            for address_id, kind, value, _ in addresses
        ]
        if record.get("addresses", []).count(expected_addresses[0]) != 1:
            raise ValueError(f"Map 3 Battle 01 victory/return entry address drift: {record_id}")
        added_addresses = expected_addresses[1:]
        candidates = [
            offset
            for offset in range(len(record.get("addresses", [])) - len(added_addresses) + 1)
            if record["addresses"][offset : offset + len(added_addresses)] == added_addresses
        ]
        if added_addresses and len(candidates) != 1:
            raise ValueError(f"Map 3 Battle 01 victory/return address delta drift: {record_id}")
        address_start = candidates[0] if candidates else 0
        if (
            record.get("documents", []).count(_OWNER_DOCUMENT) != 1
            or record["documents"][-1] != _OWNER_DOCUMENT
        ):
            raise ValueError(f"Map 3 Battle 01 victory/return owner document drift: {record_id}")
        expected = {
            "level": "H2",
            "fixture": "tests/fixtures/h2/map3-battle01-victory-return-static-v1.json",
            "fixtureId": ID,
            "verifier": "src/sf2tool/h2/map3_battle01_victory_return.py",
            "bindings": [
                {"addressId": address_id, "fixtureField": fixture_field}
                for address_id, _, _, fixture_field in addresses
            ],
        }
        found = [item for item in record["evidence"] if item.get("fixtureId") == ID]
        if len(found) != 1 or found[0] != expected or record["evidence"][-1] != expected:
            raise ValueError(f"Map 3 Battle 01 victory/return owner evidence drift: {record_id}")
        accepted_base = json.loads(json.dumps(record))
        accepted_base["evidence"].pop()
        accepted_base["documents"].pop()
        if added_addresses:
            del accepted_base["addresses"][address_start : address_start + len(added_addresses)]
        actual = (
            hashlib.sha256(
                json.dumps(accepted_base, sort_keys=True, separators=(",", ":")).encode("utf-8")
            )
            .hexdigest()
            .upper()
        )
        if actual != baseline:
            raise ValueError(
                f"Map 3 Battle 01 victory/return accepted-base index drift: {record_id}"
            )
        evidence.append(expected)
    if {
        record["id"]
        for record in index["records"]
        if any(item.get("fixtureId") == ID for item in record["evidence"])
    } != set(_OWNER_RECORD_IDS):
        raise ValueError("Map 3 Battle 01 victory/return extra owner evidence drift")
    return evidence


def _summary(
    sources: list[dict[str, str]],
    anchors: list[dict[str, Any]],
    evidence: list[dict[str, Any]],
    unknowns: dict[str, str],
    program: dict[str, Any],
) -> dict[str, int]:
    return {
        "sourceFiles": len(sources),
        "h1RomAnchors": len(anchors),
        "indexObjects": len(evidence),
        "indexBindings": sum(len(item["bindings"]) for item in evidence),
        "unknowns": len(unknowns),
        "programOperations": len(program["operations"]),
        "programCommandForms": len(program["commandForms"]),
    }


def _structural_schema() -> dict[str, Any]:
    schema = load_json(SCHEMA)
    return {"$schema": schema["$schema"], "$ref": "#/$defs/fixture", "$defs": schema["$defs"]}


def _validate_structural_output(value: dict[str, Any]) -> None:
    errors = sorted(
        Draft7Validator(_structural_schema()).iter_errors(value),
        key=lambda error: list(error.absolute_path),
    )
    if errors:
        location = ".".join(str(part) for part in errors[0].absolute_path) or "<root>"
        raise ValueError(
            "Map 3 Battle 01 victory/return structural schema validation failed "
            f"at {location}: {errors[0].message}"
        )
    if list(value) != [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "system",
        "summary",
        "retainedR3d",
        "retainedOwners",
        "sourceContext",
        "victoryReturnSpine",
        "unknowns",
    ]:
        raise ValueError("Map 3 Battle 01 victory/return fixture root order drift")
    if list(value["unknowns"]) != list(_UNKNOWN_KEYS):
        raise ValueError("Map 3 Battle 01 victory/return Unknown queue order drift")
    if [row["path"] for row in value["sourceContext"]["sourceIdentities"]] != list(_SOURCE_SURFACE):
        raise ValueError("Map 3 Battle 01 victory/return source identity order drift")
    if [row["id"] for row in value["sourceContext"]["h1RomAnchors"]] != [
        row[0] for row in _ANCHORS
    ]:
        raise ValueError("Map 3 Battle 01 victory/return anchor identity order drift")
    if list(value["victoryReturnSpine"]["ownerRecordIds"]) != list(_OWNER_RECORD_IDS):
        raise ValueError("Map 3 Battle 01 victory/return owner-record order drift")


def build_map3_battle01_victory_return_static(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != _ROM_SHA256:
        raise ValueError("Map 3 Battle 01 victory/return canonical ROM SHA-256 drift")
    upstream = upstream_path.resolve(strict=True)
    revision = subprocess.run(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    if revision != _UPSTREAM_COMMIT:
        raise ValueError("Map 3 Battle 01 victory/return upstream revision drift")
    root = _disasm_root(upstream)
    text, sources = _read_source_surface(root)
    program = _validate_source_contract(text)
    h1 = (upstream / _H1_BINARY).read_bytes()
    rom = rom_path.resolve(strict=True).read_bytes()
    symbols = listing_symbol_addresses((upstream / _LISTING).read_text(encoding="utf-8"))
    if {name: symbols.get(name) for name in _FUNCTIONS} != _FUNCTIONS:
        raise ValueError("Map 3 Battle 01 victory/return H1 symbol projection drift")
    r3d_before, owners_before = _retained_r3d(rom_path, upstream_path), _retained_owners()
    spine = _parse_h1(h1, program)
    r3d_after, owners_after = _retained_r3d(rom_path, upstream_path), _retained_owners()
    if r3d_before != r3d_after or owners_before != owners_after:
        raise ValueError(
            "Map 3 Battle 01 victory/return pre-construction retained projection drift"
        )
    index = _normalize_request_consumption_later_owner_index(load_json(RESEARCH_INDEX))
    evidence = _owner_evidence(index)
    unknowns = {key: "Unknown" for key in _UNKNOWN_KEYS}
    spine["ownerRecordIds"] = list(_OWNER_RECORD_IDS)
    anchors = _anchor_projection(h1, rom)
    toolchain = load_json(TOOLCHAIN)
    output = {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": toolchain["sf2disasm"]["repository"],
            "commit": toolchain["sf2disasm"]["commit"],
        },
        "romSha256": load_json(ROM_MANIFEST)["hashes"]["sha256"],
        "system": ID,
        "summary": _summary(sources, anchors, evidence, unknowns, program),
        "retainedR3d": r3d_after,
        "retainedOwners": owners_after,
        "sourceContext": {"sourceIdentities": sources, "h1RomAnchors": anchors},
        "victoryReturnSpine": spine,
        "unknowns": unknowns,
    }
    _validate_structural_output(output)
    return output


def verify_map3_battle01_victory_return_static(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    _validate_structural_output(fixture)
    output = build_map3_battle01_victory_return_static(rom_path, upstream_path)
    if (
        fixture["retainedR3d"] != output["retainedR3d"]
        or fixture["retainedOwners"] != output["retainedOwners"]
    ):
        raise ValueError("Map 3 Battle 01 victory/return retained golden-boundary projection drift")
    if fixture != output:
        raise ValueError("Map 3 Battle 01 victory/return complete semantic fixture drift")
    return output
