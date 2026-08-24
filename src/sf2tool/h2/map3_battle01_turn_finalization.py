"""Static source/H1 inventory for the Battle 01 turn-finalization continuation."""

from __future__ import annotations

import hashlib
import json
import re
import subprocess
from pathlib import Path
from typing import Any

from jsonschema import Draft7Validator

from sf2tool.h2.map3_battle01_action_completion import (
    FIXTURE as R3C_FIXTURE,
)
from sf2tool.h2.map3_battle01_action_completion import (
    build_map3_battle01_action_completion_static,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

ID = "sf2-map3-battle01-turn-finalization-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map3-battle01-turn-finalization-static-v1.json")
SCHEMA = repo_path("schemas/h2/map3-battle01-turn-finalization-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")

_LISTING = Path("build/sf2build-h1.lst")
_H1_BINARY = Path("build/sf2build-h1.bin")
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"

_SOURCE_SURFACE = (
    "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
    "code/gameflow/battle/battlescenes/initializebattlescene.asm",
    "code/gameflow/battle/battlescenes/battlesceneengine_0.asm",
    "code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm",
    "code/gameflow/battle/battlefunctions/loadBattle.asm",
    "code/gameflow/battle/battleloop_1.asm",
    "code/gameflow/battle/cutscenes/battleendcutscenesstart.asm",
    "code/gameflow/battle/battleloop/processkilledcombatants.asm",
    "code/gameflow/battle/battleloop/countremainingcombatants.asm",
    "code/gameflow/battle/battleloop/processafterturneffects.asm",
    "code/gameflow/battle/battleloop_2.asm",
)

_ANCHORS = (
    ("turnFinalizationSpine.replay.resume", 0x24106, 4, None),
    ("turnFinalizationSpine.replay.musicSelector", 0x2410A, 0x9A, 0x241A4),
    ("turnFinalizationSpine.replay.actorSelector", 0x241A4, 0x20, 0x241C4),
    ("turnFinalizationSpine.replay.initializeCall", 0x241C8, 6, None),
    ("turnFinalizationSpine.replay.executeCall", 0x241D4, 6, None),
    ("turnFinalizationSpine.replay.endCall", 0x241DA, 6, None),
    ("turnFinalizationSpine.replay.leaderDeathPositionsCall", 0x241E0, 6, None),
    ("turnFinalizationSpine.replay.loadBattleCall", 0x241F4, 4, None),
    ("turnFinalizationSpine.replay.reloadRange", 0x241FA, 0x14, 0x2420E),
    ("turnFinalizationSpine.replay.return", 0x2423E, 4, 0x24242),
    ("turnFinalizationSpine.functionAddresses.InitializeBattlescene", 0x18012, 2, None),
    ("turnFinalizationSpine.functionAddresses.ExecuteBattlesceneScript", 0x18398, 2, None),
    ("turnFinalizationSpine.replay.executeEnd", 0x183EA, 2, None),
    ("turnFinalizationSpine.functionAddresses.EndBattlescene", 0x1924A, 2, None),
    (
        "turnFinalizationSpine.functionAddresses.ApplyPositionsAfterEnemyLeaderDies",
        0x47D9E,
        2,
        None,
    ),
    ("turnFinalizationSpine.functionAddresses.LoadBattle", 0x25610, 2, None),
    ("turnFinalizationSpine.outerLoop.executeTurnCall", 0x23B54, 4, None),
    ("turnFinalizationSpine.outerLoop.executeTurnResume", 0x23B58, 0x12, 0x23B6A),
    ("turnFinalizationSpine.outerLoop.defeatedCutsceneCall", 0x23B6A, 4, None),
    ("turnFinalizationSpine.outerLoop.processKilledFirstCall", 0x23B70, 4, None),
    ("turnFinalizationSpine.outerLoop.countFirstCall", 0x23B76, 4, None),
    ("turnFinalizationSpine.outerLoop.preAfterTurnOutcome", 0x23B7A, 0xC, 0x23B86),
    ("turnFinalizationSpine.outerLoop.defeatedCutscene", 0x23B86, 0xE, 0x23B94),
    ("turnFinalizationSpine.outerLoop.afterTurnCall", 0x23B94, 4, None),
    ("turnFinalizationSpine.outerLoop.processKilledSecondCall", 0x23B98, 4, None),
    ("turnFinalizationSpine.outerLoop.countSecondCall", 0x23B9E, 4, None),
    ("turnFinalizationSpine.outerLoop.postAfterTurnOutcome", 0x23BA2, 0xC, 0x23BAE),
    ("turnFinalizationSpine.outerLoop.nextTurnBackedge", 0x23BAE, 6, 0x23BB4),
    ("turnFinalizationSpine.functionAddresses.BattleEndCutscenesStart", 0x47B92, 2, None),
    ("turnFinalizationSpine.functionAddresses.ProcessKilledCombatants", 0x24518, 2, None),
    ("turnFinalizationSpine.functionAddresses.CountRemainingCombatants", 0x23C58, 2, None),
    ("turnFinalizationSpine.functionAddresses.ProcessAfterTurnEffects", 0x24242, 2, None),
    ("turnFinalizationSpine.outcomeBoundaries.victory", 0x23CBA, 2, None),
    ("turnFinalizationSpine.outcomeBoundaries.defeat", 0x23D44, 2, None),
)

_FUNCTIONS = {
    "ExecuteIndividualTurn": 0x23EB0,
    "InitializeBattlescene": 0x18012,
    "ExecuteBattlesceneScript": 0x18398,
    "EndBattlescene": 0x1924A,
    "ApplyPositionsAfterEnemyLeaderDies": 0x47D9E,
    "LoadBattle": 0x25610,
    "BattleLoop": 0x23A84,
    "ExecuteBattleCutscene_Defeated": 0x47B92,
    "ProcessKilledCombatants": 0x24518,
    "CountRemainingCombatants": 0x23C58,
    "ProcessAfterTurnEffects": 0x24242,
    "BattleLoop_Victory": 0x23CBA,
    "BattleLoop_Defeat": 0x23D44,
    "j_ExecuteBattleCutscene_Defeated": 0x44048,
    "j_InitializeBattlescene": 0x18004,
    "j_ExecuteBattlesceneScript": 0x18008,
    "j_EndBattlescene": 0x1800C,
    "j_ApplyPositionsAfterEnemyLeaderDies": 0x44050,
}

_OWNER_RECORD_IDS = (
    "battle.functions.execute-turn",
    "battle.replay.execute-script",
    "battle.scene.initialize",
    "battle.cutscene.leader-death-positions",
    "battle.functions.load-battle",
    "battle.control.main-loop",
    "battle.cutscene.battle-end-start",
    "battle.loop.process-killed",
    "battle.loop.count-remaining",
    "battle.status.after-turn-expiry",
    "battle.control.outcomes",
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
)


def _disasm_root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _read_source_surface(root: Path) -> tuple[dict[str, str], list[dict[str, str]]]:
    """Read exactly the scoped finalization sources with stable public identities."""
    text: dict[str, str] = {}
    identities: list[dict[str, str]] = []
    for relative in _SOURCE_SURFACE:
        path = root / relative
        if not path.is_file():
            raise ValueError(f"Map 3 Battle 01 turn/finalization source is missing: {relative}")
        data = path.read_bytes()
        identities.append({"path": relative, "sha256": hashlib.sha256(data).hexdigest().upper()})
        text[relative] = data.decode("utf-8").replace("\r\n", "\n")
    if len(identities) != 11:
        raise ValueError("Map 3 Battle 01 turn/finalization source denominator drift")
    return text, identities


def _anchor_projection(h1_binary: bytes, rom: bytes) -> list[dict[str, Any]]:
    """Prove every bounded H1 anchor is identical in the canonical ROM."""
    anchors: list[dict[str, Any]] = []
    for identifier, address, width, end_address in _ANCHORS:
        h1 = h1_binary[address : address + width]
        if len(h1) != width or rom[address : address + width] != h1:
            raise ValueError(f"Map 3 Battle 01 turn/finalization H1/ROM anchor drift: {identifier}")
        item: dict[str, Any] = {
            "id": identifier,
            "address": address,
            "width": width,
            "sha256": hashlib.sha256(h1).hexdigest().upper(),
        }
        if end_address is not None:
            item["endAddressExclusive"] = end_address
        anchors.append(item)
    if len(anchors) != 34:
        raise ValueError("Map 3 Battle 01 turn/finalization H1/ROM anchor denominator drift")
    return anchors


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8") + b"\n"


def _canonical(value: Any) -> bytes:
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8")


def _without_comments(source: str) -> str:
    return "\n".join(line.split(";", maxsplit=1)[0].rstrip() for line in source.splitlines())


def _normalized(source: str) -> str:
    return "\n".join(
        re.sub(r"\s*,\s*", ",", " ".join(line.split()))
        for line in _without_comments(source).splitlines()
    )


def _function_section(source: str, entry: str, context: str) -> str:
    lines = source.splitlines()
    matches = [index for index, line in enumerate(lines) if _normalized(line).strip() == entry]
    if len(matches) != 1:
        raise ValueError(
            f"Map 3 Battle 01 turn/finalization source-use drift in {context}: {entry}"
        )
    marker = f"End of function {entry.rstrip(':')}"
    for index in range(matches[0] + 1, len(lines)):
        if marker in lines[index]:
            return "\n".join(lines[matches[0] : index])
    raise ValueError(f"Map 3 Battle 01 turn/finalization source-use drift in {context}: end")


def _require_order(
    source: str, expected: tuple[str, ...], context: str, *, entry: str | None = None
) -> None:
    bounded = _function_section(source, entry, context) if entry else source
    lines = _normalized(bounded).splitlines()
    cursor = 0
    for fragment in expected:
        for found in range(cursor, len(lines)):
            if lines[found] == fragment:
                break
        else:
            raise ValueError(
                f"Map 3 Battle 01 turn/finalization source-use drift in {context}: {fragment}"
            )
        cursor = found + 1


def _validate_source_contract(text: dict[str, str]) -> dict[str, str]:
    """Guard source order only; all branch outcomes remain explicitly Unknown."""
    execute = text[_SOURCE_SURFACE[0]]
    initialize = text[_SOURCE_SURFACE[1]]
    script = text[_SOURCE_SURFACE[2]]
    positions = text[_SOURCE_SURFACE[3]]
    load = text[_SOURCE_SURFACE[4]]
    loop = text[_SOURCE_SURFACE[5]]
    cutscene = text[_SOURCE_SURFACE[6]]
    killed = text[_SOURCE_SURFACE[7]]
    count = text[_SOURCE_SURFACE[8]]
    after_turn = text[_SOURCE_SURFACE[9]]
    outcomes = text[_SOURCE_SURFACE[10]]
    _require_order(
        execute,
        (
            "@WriteBattlesceneScript:",
            "move.w combatant(a6),d0",
            "@GetFirstBattlesceneEnemy:",
            "@InitBattlescene:",
            "jsr j_InitializeBattlescene",
            "jsr j_ExecuteBattlesceneScript",
            "jsr j_EndBattlescene",
            "jsr j_ApplyPositionsAfterEnemyLeaderDies",
            "jsr LoadBattle(pc)",
            "@Done:",
            "unlk a6",
            "rts",
        ),
        "ExecuteIndividualTurn replay continuation",
        entry="ExecuteIndividualTurn:",
    )
    _require_order(
        initialize,
        ("InitializeBattlescene:", "rts"),
        "InitializeBattlescene",
        entry="InitializeBattlescene:",
    )
    _require_order(
        script,
        ("ExecuteBattlesceneScript:", "@Loop:", "@End:", "clr.w d0", "rts"),
        "ExecuteBattlesceneScript",
        entry="ExecuteBattlesceneScript:",
    )
    _require_order(script, ("EndBattlescene:", "rts"), "EndBattlescene", entry="EndBattlescene:")
    _require_order(
        positions,
        ("ApplyPositionsAfterEnemyLeaderDies:", "@Done:", "rts"),
        "leader positions",
        entry="ApplyPositionsAfterEnemyLeaderDies:",
    )
    _require_order(
        load,
        (
            "LoadBattle:",
            "jsr j_PositionBattleEntities",
            "jsr j_LoadBattleTerrainData",
            "@Return:",
            "rts",
        ),
        "LoadBattle",
        entry="LoadBattle:",
    )
    _require_order(
        loop,
        (
            "@IndividualTurns_Loop:",
            "bsr.w ExecuteIndividualTurn",
            "tst.b ((DEBUG_MODE_TOGGLE-$1000000)).w",
            "beq.s @IsBattleEnd",
            "cmpi.b #INPUT_UP|INPUT_B|INPUT_C|INPUT_A,((PLAYER_1_INPUT-$1000000)).w",
            "bne.s @IsBattleEnd",
            "bsr.w KillRemainingEnemies",
            "@IsBattleEnd:",
            "jsr j_ExecuteBattleCutscene_Defeated",
            "jsr ProcessKilledCombatants(pc)",
            "bsr.w CountRemainingCombatants",
            "tst.w d2",
            "beq.w BattleLoop_Defeat",
            "tst.w d3",
            "beq.w BattleLoop_Victory",
            "bsr.w ProcessAfterTurnEffects",
            "jsr ProcessKilledCombatants(pc)",
            "bsr.w CountRemainingCombatants",
            "addq.b #TURN_ORDER_ENTRY_SIZE,((CURRENT_BATTLE_TURN-$1000000)).w",
            "bra.s @IndividualTurns_Loop",
        ),
        "BattleLoop finalization spine",
        entry="BattleLoop:",
    )
    _require_order(
        cutscene,
        ("ExecuteBattleCutscene_Defeated:", "bsr.w ExecuteMapScript"),
        "defeated cutscene",
        entry="ExecuteBattleCutscene_Defeated:",
    )
    _require_order(
        killed,
        ("ProcessKilledCombatants:", "@NoneKilled:", "rts"),
        "killed cleanup",
        entry="ProcessKilledCombatants:",
    )
    _require_order(
        count,
        ("CountRemainingCombatants:", "@Return:", "rts"),
        "remaining count",
        entry="CountRemainingCombatants:",
    )
    _require_order(
        after_turn,
        ("ProcessAfterTurnEffects:", "@Skip:", "rts"),
        "after-turn effects",
        entry="ProcessAfterTurnEffects:",
    )
    _require_order(
        outcomes, ("BattleLoop_Victory:", "rts"), "victory boundary", entry="BattleLoop_Victory:"
    )
    _require_order(
        outcomes, ("BattleLoop_Defeat:", "rts"), "defeat boundary", entry="BattleLoop_Defeat:"
    )
    return {"sourceContract": "confirmed"}


def _word(data: bytes, address: int) -> int:
    value = data[address : address + 2]
    if len(value) != 2:
        raise ValueError(f"Map 3 Battle 01 turn/finalization H1 word truncated at {address:#x}")
    return int.from_bytes(value, "big")


def _long(data: bytes, address: int) -> int:
    value = data[address : address + 4]
    if len(value) != 4:
        raise ValueError(f"Map 3 Battle 01 turn/finalization H1 long truncated at {address:#x}")
    return int.from_bytes(value, "big")


def _require_relative_target(data: bytes, address: int, opcode: int, expected: int) -> None:
    if _word(data, address) != opcode:
        raise ValueError(f"Map 3 Battle 01 turn/finalization opcode drift at {address:#x}")
    target = address + 2 + int.from_bytes(data[address + 2 : address + 4], "big", signed=True)
    if target != expected:
        raise ValueError(f"Map 3 Battle 01 turn/finalization target drift at {address:#x}")


def _require_bsr_target(data: bytes, address: int, expected: int) -> None:
    _require_relative_target(data, address, 0x6100, expected)


def _require_short_target(data: bytes, address: int, opcode: int, expected: int) -> None:
    instruction = _word(data, address)
    if instruction >> 8 != opcode:
        raise ValueError(f"Map 3 Battle 01 turn/finalization branch opcode drift at {address:#x}")
    target = address + 2 + int.from_bytes(bytes((instruction & 0xFF,)), "big", signed=True)
    if target != expected:
        raise ValueError(f"Map 3 Battle 01 turn/finalization branch target drift at {address:#x}")


def _require_jsr_pc_target(data: bytes, address: int, expected: int) -> None:
    _require_relative_target(data, address, 0x4EBA, expected)


def _require_alias(data: bytes, alias: int, expected: int) -> None:
    if _word(data, alias) != 0x4EFA:
        raise ValueError(f"Map 3 Battle 01 turn/finalization alias opcode drift at {alias:#x}")
    target = alias + 2 + int.from_bytes(data[alias + 2 : alias + 4], "big", signed=True)
    if target != expected:
        raise ValueError(f"Map 3 Battle 01 turn/finalization alias target drift at {alias:#x}")


def _parse_turn_finalization(h1_binary: bytes) -> dict[str, Any]:
    """Derive bounded post-action topology from H1 instructions and effective aliases."""
    if h1_binary[0x24106:0x2410A] != bytes.fromhex("302EFFFE"):
        raise ValueError("Map 3 Battle 01 turn/finalization continuation resume drift")
    for address, alias, target in (
        (0x241C8, "j_InitializeBattlescene", "InitializeBattlescene"),
        (0x241D4, "j_ExecuteBattlesceneScript", "ExecuteBattlesceneScript"),
        (0x241DA, "j_EndBattlescene", "EndBattlescene"),
        (0x241E0, "j_ApplyPositionsAfterEnemyLeaderDies", "ApplyPositionsAfterEnemyLeaderDies"),
    ):
        if (
            _word(h1_binary, address) != 0x4EB9
            or _long(h1_binary, address + 2) != _FUNCTIONS[alias]
        ):
            raise ValueError(f"Map 3 Battle 01 turn/finalization call drift at {address:#x}")
        _require_alias(h1_binary, _FUNCTIONS[alias], _FUNCTIONS[target])
    _require_jsr_pc_target(h1_binary, 0x241F4, _FUNCTIONS["LoadBattle"])
    if h1_binary[0x2423E:0x24242] != bytes.fromhex("4E5E4E75"):
        raise ValueError("Map 3 Battle 01 turn/finalization continuation return drift")
    _require_bsr_target(h1_binary, 0x23B54, _FUNCTIONS["ExecuteIndividualTurn"])
    if h1_binary[0x23B58:0x23B5C] != bytes.fromhex("4A38B0A9"):
        raise ValueError("Map 3 Battle 01 turn/finalization debug-gate predicate drift")
    _require_short_target(h1_binary, 0x23B5C, 0x67, 0x23B6A)
    if h1_binary[0x23B5E:0x23B64] != bytes.fromhex("0C380071DE97"):
        raise ValueError("Map 3 Battle 01 turn/finalization debug-cheat predicate drift")
    _require_short_target(h1_binary, 0x23B64, 0x66, 0x23B6A)
    _require_bsr_target(h1_binary, 0x23B66, 0x23BB4)
    if (
        _word(h1_binary, 0x23B6A) != 0x4EB9
        or _long(h1_binary, 0x23B6C) != _FUNCTIONS["j_ExecuteBattleCutscene_Defeated"]
    ):
        raise ValueError("Map 3 Battle 01 turn/finalization defeated-cutscene call drift")
    _require_alias(
        h1_binary,
        _FUNCTIONS["j_ExecuteBattleCutscene_Defeated"],
        _FUNCTIONS["ExecuteBattleCutscene_Defeated"],
    )
    _require_jsr_pc_target(h1_binary, 0x23B70, _FUNCTIONS["ProcessKilledCombatants"])
    _require_bsr_target(h1_binary, 0x23B76, _FUNCTIONS["CountRemainingCombatants"])
    if _word(h1_binary, 0x23B7A) != 0x4A42 or _word(h1_binary, 0x23B80) != 0x4A43:
        raise ValueError("Map 3 Battle 01 turn/finalization first outcome predicate drift")
    _require_relative_target(h1_binary, 0x23B7C, 0x6700, _FUNCTIONS["BattleLoop_Defeat"])
    _require_relative_target(h1_binary, 0x23B82, 0x6700, _FUNCTIONS["BattleLoop_Victory"])
    _require_bsr_target(h1_binary, 0x23B94, _FUNCTIONS["ProcessAfterTurnEffects"])
    _require_jsr_pc_target(h1_binary, 0x23B98, _FUNCTIONS["ProcessKilledCombatants"])
    _require_bsr_target(h1_binary, 0x23B9E, _FUNCTIONS["CountRemainingCombatants"])
    if _word(h1_binary, 0x23BA2) != 0x4A42 or _word(h1_binary, 0x23BA8) != 0x4A43:
        raise ValueError("Map 3 Battle 01 turn/finalization second outcome predicate drift")
    _require_relative_target(h1_binary, 0x23BA4, 0x6700, _FUNCTIONS["BattleLoop_Defeat"])
    _require_relative_target(h1_binary, 0x23BAA, 0x6700, _FUNCTIONS["BattleLoop_Victory"])
    _require_short_target(h1_binary, 0x23BB2, 0x60, 0x23B40)
    return {
        "functionAddresses": {
            key: value for key, value in _FUNCTIONS.items() if not key.startswith("j_")
        },
        "replayContinuation": {
            "resumeAddress": 0x24106,
            "musicSelectorRange": [0x2410A, 0x241A4],
            "actorSelectorRange": [0x241A4, 0x241C4],
            "initializeBattlesceneCallAddress": 0x241C8,
            "executeBattlesceneCallAddress": 0x241D4,
            "executeBattlesceneScriptEndAddress": 0x183EA,
            "endBattlesceneCallAddress": 0x241DA,
            "leaderDeathPositionsCallAddress": 0x241E0,
            "loadBattleCallAddress": 0x241F4,
            "reloadRange": [0x241FA, 0x2420E],
            "returnAddress": 0x24240,
            "orderedSteps": [
                "musicSelector",
                "actorSelector",
                "InitializeBattlescene",
                "ExecuteBattlesceneScript",
                "EndBattlescene",
                "ApplyPositionsAfterEnemyLeaderDies",
                "LoadBattle",
                "return",
            ],
        },
        "outerLoop": {
            "executeTurnCallAddress": 0x23B54,
            "executeTurnResumeAddress": 0x23B58,
            "defeatedCutsceneCallAddress": 0x23B6A,
            "processKilledFirstCallAddress": 0x23B70,
            "countFirstCallAddress": 0x23B76,
            "preAfterTurnOutcomeRange": [0x23B7A, 0x23B86],
            "defeatedCutsceneRange": [0x23B86, 0x23B94],
            "processKilledSecondCallAddress": 0x23B98,
            "afterTurnCallAddress": 0x23B94,
            "afterTurnResumeAddress": 0x23B98,
            "countSecondCallAddress": 0x23B9E,
            "postAfterTurnOutcomeRange": [0x23BA2, 0x23BAE],
            "nextTurnDispatchAddress": 0x23BB2,
            "backedgeAddress": 0x23BAE,
            "backedgeTarget": 0x23B40,
        },
        "outcomeBoundaries": {"victoryAddress": 0x23CBA, "defeatAddress": 0x23D44},
    }


def _retained_r3c(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    fixture = load_json(R3C_FIXTURE)
    if fixture.get("id") != "sf2-map3-battle01-action-completion-static-v1":
        raise ValueError("Map 3 Battle 01 turn/finalization retained R3c identity drift")
    fresh = build_map3_battle01_action_completion_static(rom_path, upstream_path)
    if fixture != fresh:
        raise ValueError("Map 3 Battle 01 turn/finalization retained R3c projection drift")
    projection = {
        "fixtureId": fixture["id"],
        "fixtureSha256": hashlib.sha256(R3C_FIXTURE.read_bytes()).hexdigest().upper(),
        "actionCompletionSpineSha256": hashlib.sha256(_canonical(fresh["actionCompletionSpine"]))
        .hexdigest()
        .upper(),
    }
    projection["sha256"] = hashlib.sha256(_canonical(projection)).hexdigest().upper()
    return projection


def _retained_fixture(path: Path, fixture_id: str, field: str) -> dict[str, Any]:
    fixture = load_json(path)
    if fixture.get("id") != fixture_id:
        raise ValueError(
            f"Map 3 Battle 01 turn/finalization retained owner identity drift: {field}"
        )
    projection = {
        "fixtureId": fixture_id,
        "fixtureSha256": hashlib.sha256(path.read_bytes()).hexdigest().upper(),
        "semanticSha256": hashlib.sha256(_canonical(fixture)).hexdigest().upper(),
    }
    projection["sha256"] = hashlib.sha256(_canonical(projection)).hexdigest().upper()
    return projection


def _retained_owners() -> dict[str, Any]:
    owners = {
        "battleFunctions": (
            "tests/fixtures/h2/battle-functions-static-v1.json",
            "sf2-battle-functions-static-v1",
        ),
        "battleSceneEngine": (
            "tests/fixtures/h2/battle-scene-engine-static-v1.json",
            "sf2-battle-scene-engine-static-v1",
        ),
        "battleCutscenes": (
            "tests/fixtures/h2/battle-cutscenes-static-v1.json",
            "sf2-battle-cutscenes-static-v1",
        ),
        "battleLoop": ("tests/fixtures/h2/battle-loop-static-v1.json", "sf2-battle-loop-static-v1"),
        "battleControl": (
            "tests/fixtures/h2/battle-control-static-v1.json",
            "sf2-battle-control-static-v1",
        ),
        "afterTurnStatusLifecycle": (
            "tests/fixtures/h3/after-turn-status-lifecycle-v1.json",
            "sf2-after-turn-status-lifecycle-v1",
        ),
    }
    return {
        key: _retained_fixture(repo_path(path), fixture_id, key)
        for key, (path, fixture_id) in owners.items()
    }


def _owner_record_ids(index: dict[str, Any]) -> list[str]:
    expected = {
        "battle.functions.execute-turn": "ExecuteIndividualTurn",
        "battle.replay.execute-script": "ExecuteBattlesceneScript",
        "battle.scene.initialize": "InitializeBattlescene",
        "battle.cutscene.leader-death-positions": "ApplyPositionsAfterEnemyLeaderDies",
        "battle.functions.load-battle": "LoadBattle",
        "battle.control.main-loop": "BattleLoop",
        "battle.cutscene.battle-end-start": "ExecuteBattleCutscene_Defeated",
        "battle.loop.process-killed": "ProcessKilledCombatants",
        "battle.loop.count-remaining": "CountRemainingCombatants",
        "battle.status.after-turn-expiry": "ProcessAfterTurnEffects",
        "battle.control.outcomes": "BattleLoop_Victory",
    }
    if tuple(expected) != _OWNER_RECORD_IDS:
        raise ValueError("Map 3 Battle 01 turn/finalization owner declaration drift")
    records = {record["id"]: record for record in index["records"]}
    for record_id, symbol in expected.items():
        if records.get(record_id, {}).get("symbol") != symbol:
            raise ValueError(f"Map 3 Battle 01 turn/finalization owner record drift: {record_id}")
    return list(_OWNER_RECORD_IDS)


def _owner_evidence(index: dict[str, Any], record_ids: list[str]) -> list[dict[str, Any]]:
    records = {record["id"]: record for record in index["records"]}
    evidence = [
        item
        for record_id in record_ids
        for item in records[record_id]["evidence"]
        if item.get("fixtureId") == ID
    ]
    if len(evidence) != len(record_ids):
        raise ValueError("Map 3 Battle 01 turn/finalization owner evidence drift")
    return evidence


def _summary(
    source_identities: list[dict[str, str]],
    anchors: list[dict[str, Any]],
    owner_evidence: list[dict[str, Any]],
    unknowns: dict[str, str],
) -> dict[str, int]:
    return {
        "sourceFiles": len(source_identities),
        "h1RomAnchors": len(anchors),
        "indexObjects": len(owner_evidence),
        "indexBindings": sum(len(item["bindings"]) for item in owner_evidence),
        "unknowns": len(unknowns),
    }


def _structural_schema() -> dict[str, Any]:
    schema = load_json(SCHEMA)
    fixture = schema.get("$defs", {}).get("fixture")
    if not isinstance(fixture, dict):
        raise ValueError("Map 3 Battle 01 turn/finalization fixture schema definition is missing")
    return {"$schema": schema["$schema"], "$ref": "#/$defs/fixture", "$defs": schema["$defs"]}


def _validate_structural_output(value: dict[str, Any]) -> None:
    errors = sorted(
        Draft7Validator(_structural_schema()).iter_errors(value),
        key=lambda error: list(error.absolute_path),
    )
    if errors:
        location = ".".join(str(part) for part in errors[0].absolute_path) or "<root>"
        message = errors[0].message
        raise ValueError(
            "Map 3 Battle 01 turn/finalization structural schema validation failed "
            f"at {location}: {message}"
        )


def build_map3_battle01_turn_finalization_static(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Build static caller finalization through the unentered next-turn backedge."""
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != _ROM_SHA256:
        raise ValueError("Map 3 Battle 01 turn/finalization canonical ROM SHA-256 drift")
    upstream = upstream_path.resolve(strict=True)
    revision = subprocess.run(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    if revision != _UPSTREAM_COMMIT:
        raise ValueError("Map 3 Battle 01 turn/finalization upstream revision drift")
    root = _disasm_root(upstream)
    text, source_identities = _read_source_surface(root)
    h1_binary = (upstream / _H1_BINARY).read_bytes()
    rom = rom_path.resolve(strict=True).read_bytes()
    symbols = listing_symbol_addresses((upstream / _LISTING).read_text(encoding="utf-8"))
    if {key: symbols.get(key) for key in _FUNCTIONS} != _FUNCTIONS:
        raise ValueError("Map 3 Battle 01 turn/finalization H1 symbol projection drift")
    _validate_source_contract(text)
    r3c_before = _retained_r3c(rom_path, upstream_path)
    owners_before = _retained_owners()
    spine = _parse_turn_finalization(h1_binary)
    r3c_after = _retained_r3c(rom_path, upstream_path)
    owners_after = _retained_owners()
    if r3c_before != r3c_after or owners_before != owners_after:
        raise ValueError(
            "Map 3 Battle 01 turn/finalization pre-construction retained projection drift"
        )
    index = load_json(RESEARCH_INDEX)
    spine["ownerRecordIds"] = _owner_record_ids(index)
    owner_evidence = _owner_evidence(index, spine["ownerRecordIds"])
    anchors = _anchor_projection(h1_binary, rom)
    unknowns = {key: "Unknown" for key in _UNKNOWN_KEYS}
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
        "summary": _summary(source_identities, anchors, owner_evidence, unknowns),
        "retainedR3c": r3c_after,
        "retainedOwners": owners_after,
        "sourceContext": {
            "sourceIdentities": source_identities,
            "h1RomAnchors": anchors,
        },
        "turnFinalizationSpine": spine,
        "unknowns": unknowns,
    }
    _validate_structural_output(output)
    return output


def verify_map3_battle01_turn_finalization_static(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    output = build_map3_battle01_turn_finalization_static(rom_path, upstream_path)
    if (
        fixture["retainedR3c"] != output["retainedR3c"]
        or fixture["retainedOwners"] != output["retainedOwners"]
    ):
        raise ValueError(
            "Map 3 Battle 01 turn/finalization retained golden-boundary projection drift"
        )
    if fixture != output:
        raise ValueError("Map 3 Battle 01 turn/finalization complete semantic fixture drift")
    return output
