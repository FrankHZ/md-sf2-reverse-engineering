"""Public H2 contract for Battle 01 turn/control through pre-resolution."""

from __future__ import annotations

import hashlib
import json
import re
import subprocess
from pathlib import Path
from typing import Any

from jsonschema import Draft7Validator

from sf2tool.h2.map3_battle01_admission import (
    FIXTURE as R2C_FIXTURE,
)
from sf2tool.h2.map3_battle01_admission import (
    build_map3_battle01_admission_static,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

ID = "sf2-map3-battle01-turn-control-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map3-battle01-turn-control-static-v1.json")
SCHEMA = repo_path("schemas/h2/map3-battle01-turn-control-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")

_LISTING = Path("build/sf2build-h1.lst")
_H1_BINARY = Path("build/sf2build-h1.bin")
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"

_SOURCE_SURFACE = (
    "code/gameflow/battle/battleloop_1.asm",
    "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
    "code/gameflow/battle/battlefunctions/battlefunctions_2.asm",
    "code/gameflow/battle/ai/startaicontrol.asm",
    "code/gameflow/battle/ai/executeaicommand.asm",
    "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
    "code/gameflow/battle/battleactions/battleactionsengine_1.asm",
    "code/gameflow/battle/battleactions/battleactionsengine_2.asm",
    "data/battles/global/aicommandsets.asm",
    "data/battles/spritesets/spriteset01.asm",
)

_FUNCTIONS = {
    "BattleLoop": 0x23A84,
    "ExecuteIndividualTurn": 0x23EB0,
    "ProcessBattleEntityControlPlayerInput": 0x24662,
    "ExecuteAiControl": 0x252FA,
    "StartAiControl": 0xDEFC,
    "pt_AiCommandsets": 0xE1AC,
    "ExecuteAiCommand": 0xE294,
    "j_WriteBattlesceneScript": 0x820C,
    "WriteBattlesceneScript": 0x9B92,
    "battlesceneScript_DetermineTargetsByAction": 0x9DD6,
    "battlesceneScript_InitializeBattlesceneProperties": 0x9F28,
    "battlesceneScript_DetermineIneffectiveAttack": 0x9EC4,
    "battlesceneScript_InitializeActors": 0x9E60,
    "battlesceneScript_ApplyActionEffect": 0xA3F4,
}

# These spans are a closed, source-named H1/ROM checksum surface.  The three
# range anchors carry their source-defined exclusive end to avoid treating an
# adjacent function/table byte as turn/control evidence.
_ANCHORS = (
    ("turnOrderConsumer.loopStart", 0x23B40, 2, None),
    ("turnOrderConsumer.currentTurnRead", 0x23B42, 4, None),
    ("turnOrderConsumer.orderBase", 0x23B46, 4, None),
    ("turnOrderConsumer.actorRead", 0x23B4A, 4, None),
    ("turnOrderConsumer.ffCompare", 0x23B4E, 4, None),
    ("turnOrderConsumer.restartBranch", 0x23B52, 2, None),
    ("turnOrderConsumer.executeCall", 0x23B54, 4, None),
    ("controlDispatch.executeIndividualTurnRange", 0x23EB0, 0x392, 0x24242),
    ("playerConstructionHandoff.playerCall", 0x23FE6, 4, None),
    ("playerConstructionHandoff.playerEntry", 0x24662, 2, None),
    ("aiConstructionHandoff.aiCall", 0x24036, 4, None),
    ("aiConstructionHandoff.aiEntry", 0x252FA, 2, None),
    ("controlDispatch.commonConvergence", 0x2403A, 2, None),
    ("aiConstructionHandoff.startAiControlRange", 0xDEFC, 0x1BA, 0xE0B6),
    ("battle01ControlInputs.ptAiCommandsetsRange", 0xE1AC, 0xB0, 0xE25C),
    ("aiConstructionHandoff.executeAiCommandRange", 0xE294, 0x15A, 0xE3EE),
    ("battle01ControlInputs.battleSpriteset01Range", 0x1B32E2, 0x94, 0x1B3376),
    ("commonActionConstruction.actorReload", 0x240FC, 4, None),
    ("commonActionConstruction.writeCall", 0x24100, 4, None),
    ("commonActionConstruction.writeAlias", 0x820C, 2, None),
    ("commonActionConstruction.writeEntry", 0x9B92, 2, None),
    ("commonActionConstruction.determineTargets", 0x9C26, 4, None),
    ("commonActionConstruction.initializeProperties", 0x9C2A, 4, None),
    ("commonActionConstruction.determineIneffective", 0x9C2E, 4, None),
    ("commonActionConstruction.initializeActors", 0x9C32, 4, None),
    ("preResolutionHandoff.applyEffectCall", 0x9CD0, 4, None),
    ("preResolutionHandoff.applyEffectEntry", 0xA3F4, 2, None),
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
    "resolutionEffects",
    "afterTurn",
    "multiRoundPlaythrough",
    "victory",
    "playerReady",
)

_CONTROL_PASS_POLARITY = {
    "MUDDLE": "AI",
    "AI_CONTROLLED": "AI",
    "enemyOpponentControl": {"false": "AI", "true": "player"},
    "allyAutoBattle": {"false": "player", "true": "AI"},
}


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Emit the sole canonical UTF-8 representation for this public fixture."""
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8") + b"\n"


def _canonical(value: Any) -> bytes:
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8")


def _disasm_root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _without_comments(source: str) -> str:
    return "\n".join(line.split(";", maxsplit=1)[0].rstrip() for line in source.splitlines())


def _normalized(source: str) -> str:
    return "\n".join(
        re.sub(r"\s*,\s*", ",", " ".join(line.split()))
        for line in _without_comments(source).splitlines()
    )


def _require_order(source: str, expected: tuple[str, ...], context: str) -> None:
    clean = _normalized(source)
    cursor = 0
    for fragment in expected:
        found = clean.find(fragment, cursor)
        if found < 0:
            raise ValueError(f"Map 3 Battle 01 turn/control {context} source-use drift: {fragment}")
        cursor = found + len(fragment)


def _require_section_range(source: str, expected: str, context: str) -> None:
    if expected not in source:
        raise ValueError(f"Map 3 Battle 01 turn/control {context} source range drift")


def _read_source_surface(root: Path) -> tuple[dict[str, str], list[dict[str, str]]]:
    text: dict[str, str] = {}
    identities: list[dict[str, str]] = []
    for relative in _SOURCE_SURFACE:
        path = root / relative
        if not path.is_file():
            raise ValueError(f"Map 3 Battle 01 turn/control source is missing: {relative}")
        data = path.read_bytes()
        identities.append({"path": relative, "sha256": hashlib.sha256(data).hexdigest().upper()})
        text[relative] = data.decode("utf-8").replace("\r\n", "\n")
    if len(identities) != 10:
        raise ValueError("Map 3 Battle 01 turn/control source denominator drift")
    return text, identities


def _anchor_projection(h1_binary: bytes, rom: bytes) -> list[dict[str, Any]]:
    anchors: list[dict[str, Any]] = []
    for identifier, address, width, end_address in _ANCHORS:
        h1 = h1_binary[address : address + width]
        if len(h1) != width or rom[address : address + width] != h1:
            raise ValueError(f"Map 3 Battle 01 turn/control H1/ROM anchor drift: {identifier}")
        item: dict[str, Any] = {
            "id": identifier,
            "address": address,
            "width": width,
            "sha256": hashlib.sha256(h1).hexdigest().upper(),
        }
        if end_address is not None:
            item["endAddressExclusive"] = end_address
        anchors.append(item)
    if len(anchors) != 27:
        raise ValueError("Map 3 Battle 01 turn/control H1/ROM anchor denominator drift")
    return anchors


def _validate_source_contract(text: dict[str, str]) -> dict[str, Any]:
    """Parse only the joins owned by this cross-owner control spine."""
    loop = text["code/gameflow/battle/battleloop_1.asm"]
    execute = text["code/gameflow/battle/battlefunctions/executeindividualturn.asm"]
    player_ai = text["code/gameflow/battle/battlefunctions/battlefunctions_2.asm"]
    start_ai = text["code/gameflow/battle/ai/startaicontrol.asm"]
    command_dispatch = text["code/gameflow/battle/ai/executeaicommand.asm"]
    interfaces = text["code/common/tech/jumpinterfaces/s02_jumpinterface.asm"]
    actions = text["code/gameflow/battle/battleactions/battleactionsengine_1.asm"]
    effects = text["code/gameflow/battle/battleactions/battleactionsengine_2.asm"]
    commandsets = text["data/battles/global/aicommandsets.asm"]
    spriteset = text["data/battles/spritesets/spriteset01.asm"]

    _require_order(
        loop,
        (
            "@IndividualTurns_Loop:",
            "clr.w d0",
            "move.b ((CURRENT_BATTLE_TURN-$1000000)).w,d0",
            "lea ((BATTLE_TURN_ORDER-$1000000)).w,a0",
            "move.b (a0,d0.w),d0",
            "cmpi.b #-1,d0",
            "beq.s @Start",
            "bsr.w ExecuteIndividualTurn",
        ),
        "turn-order consumer",
    )
    _require_section_range(
        execute, "; 0x23EB0..0x24242 : Execute Individual Turn function", "ExecuteIndividualTurn"
    )
    _require_order(
        execute,
        (
            "jsr j_GetCurrentHp",
            "tst.w d1",
            "beq.w @Done",
            "andi.w #STATUSEFFECT_MUDDLE,d1",
            "bne.w @Call_StartAiControl",
            "andi.w #AIBITFIELD_AI_CONTROLLED,d1",
            "bne.w @Call_StartAiControl",
            "tst.b d0",
            "bpl.s @CheckAutoBattleCheat1",
            "tst.b ((CONTROL_OPPONENT_TOGGLE-$1000000)).w",
            "beq.w @Call_StartAiControl",
            "bra.s @Goto_PlayerControl",
            "@CheckAutoBattleCheat1:",
            "tst.b ((AUTO_BATTLE_TOGGLE-$1000000)).w",
            "bne.w @Call_StartAiControl",
            "@Goto_PlayerControl:",
            "bra.w @PlayerControl",
            "@Call_StartAiControl:",
            "jsr j_StartAiControl",
        ),
        "preparation control-dispatch polarity",
    )
    _require_order(
        execute,
        (
            "@PlayerControl:",
            "move.w combatant(a6),d0",
            "jsr j_GetStatusEffects",
            "andi.w #STATUSEFFECT_MUDDLE,d1",
            "bne.w @Call_ExecuteAiControl",
            "andi.w #AIBITFIELD_AI_CONTROLLED,d1",
            "bne.w @Call_ExecuteAiControl",
            "tst.b d0",
            "bpl.s @CheckAutoBattleCheat2",
            "tst.b ((CONTROL_OPPONENT_TOGGLE-$1000000)).w",
            "beq.w @Call_ExecuteAiControl",
            "bra.s @ProcessPlayerInput",
            "@CheckAutoBattleCheat2:",
            "tst.b ((AUTO_BATTLE_TOGGLE-$1000000)).w",
            "bne.w @Call_ExecuteAiControl",
            "@ProcessPlayerInput:",
        ),
        "execution control-dispatch polarity",
    )
    _require_order(
        execute,
        (
            "@Call_ExecuteAiControl:",
            "bsr.w ExecuteAiControl",
            "@CheckBattleaction_CastEgress:",
        ),
        "control convergence",
    )
    _require_order(
        execute,
        (
            "@ProcessPlayerInput:",
            "bsr.w ProcessBattleEntityControlPlayerInput",
            "cmpi.w #-1,d0",
            "bne.w @CheckBattleaction_CastEgress",
        ),
        "player construction handoff",
    )
    _require_order(
        execute,
        (
            "@WriteBattlesceneScript:",
            "jsr (WaitForVInt).w",
            "jsr (WaitForVInt).w",
            "move.w combatant(a6),d0",
            "jsr j_WriteBattlesceneScript",
        ),
        "common action handoff",
    )
    _require_order(
        player_ai,
        (
            "ExecuteAiControl:",
            "movem.l d0-a6,-(sp)",
            "move.w combatant(a6),d0",
            "jsr j_BuildMovementRangeGrid",
        ),
        "AI execution entry",
    )
    _require_section_range(
        start_ai, "; 0xDEFC..0xE1AC : AI engine : preparatory phase", "StartAiControl"
    )
    _require_order(
        start_ai,
        (
            "StartAiControl:",
            "btst #COMBATANT_BIT_ENEMY,d0",
            "bne.s @Enemy",
            "move.w #AICOMMANDSET_ATTACKER1,d5",
        ),
        "AI ally commandset preparation",
    )
    _require_order(
        start_ai,
        (
            "@Enemy:",
            "bsr.w GetAiCommandset",
            "@HandlePathfindingModes:",
            "move.w d7,d0",
            "bsr.w GetAiCommandset",
            "move.w d1,d5",
            "@HandleAiCommandset:",
            "move.w d5,d1",
            "lea pt_AiCommandsets(pc),a0",
            "lsl.l #INDEX_SHIFT_COUNT,d1",
            "movea.l (a0,d1.w),a1",
            "move.b (a1),d2",
            "subi.b #1,d2",
            "@HandleAiCommandset_Loop:",
            "move.b (a1),d1",
            "bsr.w ExecuteAiCommand",
            "tst.b d1",
            "bne.s @NextAiCommand",
            "bra.w @Done",
            "@NextAiCommand:",
            "dbf d2,@HandleAiCommandset_Loop",
        ),
        "AI enemy commandset traversal",
    )
    _require_section_range(
        commandsets, "; 0xE1AC..0xE25B : AI commands data", "AI commandset table"
    )
    _require_section_range(
        command_dispatch, "; 0xE294..0xE3EE : Execute AI command function", "ExecuteAiCommand"
    )
    _require_order(
        interfaces,
        ("j_WriteBattlesceneScript:", "jmp WriteBattlesceneScript(pc)"),
        "write jump-interface alias",
    )
    _require_order(
        actions,
        (
            "WriteBattlesceneScript:",
            "bsr.w battlesceneScript_DetermineTargetsByAction",
            "bsr.w battlesceneScript_InitializeBattlesceneProperties",
            "bsr.w battlesceneScript_DetermineIneffectiveAttack",
            "bsr.w battlesceneScript_InitializeActors",
            "bsr.w battlesceneScript_ApplyActionEffect",
        ),
        "action construction to pre-resolution",
    )
    _require_order(
        effects,
        ("battlesceneScript_ApplyActionEffect:", "movem.l d0-d3/a0,-(sp)"),
        "pre-resolution function boundary",
    )

    clean_commands = _without_comments(commandsets)
    pointer_rows = re.findall(r"^\s*dc\.l\s+(AiCommandset\d+)", clean_commands, re.MULTILINE)
    if len(pointer_rows) != 16 or pointer_rows[6:8] != ["AiCommandset06", "AiCommandset07"]:
        raise ValueError("Map 3 Battle 01 turn/control commandset pointer source-use drift")

    def commands(index: int) -> list[str]:
        match = re.search(
            rf"^AiCommandset{index:02d}:.*?(?=^AiCommandset\d{{2}}:|\Z)",
            clean_commands,
            re.MULTILINE | re.DOTALL,
        )
        if match is None:
            raise ValueError(f"Map 3 Battle 01 turn/control commandset {index} is missing")
        values = re.findall(
            r"\b(?:HEAL[123]|SUPPORT|ATTACK[1234]|MOVE_ORDER[1-5]|MOVE[123]|STAY)\b", match.group(0)
        )
        if not values:
            raise ValueError(f"Map 3 Battle 01 turn/control commandset {index} is empty")
        return values

    parsed_commandsets = {"6": commands(6), "7": commands(7)}
    if parsed_commandsets != {
        "6": ["ATTACK1", "HEAL1", "SUPPORT", "MOVE1", "STAY"],
        "7": ["MOVE_ORDER1", "ATTACK1", "HEAL1", "SUPPORT", "MOVE1", "STAY"],
    }:
        raise ValueError("Map 3 Battle 01 turn/control commandset sequence drift")

    clean_spriteset = _without_comments(spriteset)
    rows = re.findall(
        r"^\s*(allyCombatant|enemyCombatant)\s+[^\n]+\n\s*combatantAiAndItem\s+(\w+),",
        clean_spriteset,
        re.MULTILINE,
    )
    if rows != [
        ("allyCombatant", "HEALER1"),
        ("allyCombatant", "HEALER1"),
        ("allyCombatant", "HEALER1"),
        ("enemyCombatant", "ATTACKER1"),
        ("enemyCombatant", "ATTACKER1"),
        ("enemyCombatant", "ATTACKER1"),
        ("enemyCombatant", "ATTACKER1"),
        ("enemyCombatant", "ATTACKER2"),
        ("enemyCombatant", "ATTACKER2"),
    ]:
        raise ValueError("Map 3 Battle 01 turn/control BattleSpriteset01 assignment drift")

    return {
        "turnOrderConsumer": {
            "functionAddresses": {"BattleLoop": _FUNCTIONS["BattleLoop"]},
            "orderedSteps": [
                "currentBattleTurn",
                "turnOrderBase",
                "actorRead",
                "ffSentinel",
                "restartGeneration",
                "executeIndividualTurn",
            ],
            "sentinel": "FF",
        },
        "controlDispatch": {
            "functionAddresses": {
                "ExecuteIndividualTurn": _FUNCTIONS["ExecuteIndividualTurn"],
                "ProcessBattleEntityControlPlayerInput": _FUNCTIONS[
                    "ProcessBattleEntityControlPlayerInput"
                ],
                "ExecuteAiControl": _FUNCTIONS["ExecuteAiControl"],
            },
            "deadActor": "exit",
            "passes": {
                "preparation": _CONTROL_PASS_POLARITY,
                "execution": _CONTROL_PASS_POLARITY,
            },
            "commonConvergenceAddress": 0x2403A,
        },
        "battle01ControlInputs": {
            "tableAddresses": {
                "pt_AiCommandsets": _FUNCTIONS["pt_AiCommandsets"],
                "BattleSpriteset01": 0x1B32E2,
            },
            "allyAlternativeAiCommandset": "HEALER1",
            "enemyCommandsetCounts": {"ATTACKER1": 4, "ATTACKER2": 2},
            "commandsets": parsed_commandsets,
        },
        "playerConstructionHandoff": {
            "functionAddresses": {
                "ProcessBattleEntityControlPlayerInput": _FUNCTIONS[
                    "ProcessBattleEntityControlPlayerInput"
                ]
            },
            "callAddress": 0x23FE6,
            "cancelSentinel": -1,
            "commonConvergenceAddress": 0x2403A,
        },
        "aiConstructionHandoff": {
            "functionAddresses": {
                "StartAiControl": _FUNCTIONS["StartAiControl"],
                "ExecuteAiCommand": _FUNCTIONS["ExecuteAiCommand"],
            },
            "preparationCommandsetEntries": [6, 7],
            "commandsetTraversal": [
                "enemyGetAiCommandsetToD5",
                "ptAiCommandsetsLookup",
                "boundedCommandLoop",
                "ExecuteAiCommand",
                "firstSuccessExit",
            ],
            "executionAddress": 0x24036,
            "commonConvergenceAddress": 0x2403A,
        },
        "commonActionConstruction": {
            "functionAddresses": {"WriteBattlesceneScript": _FUNCTIONS["WriteBattlesceneScript"]},
            "orderedSteps": [
                "WaitForVInt",
                "WaitForVInt",
                "actorReload",
                "j_WriteBattlesceneScript",
            ],
            "constructionCalls": [
                "DetermineTargetsByAction",
                "InitializeBattlesceneProperties",
                "DetermineIneffectiveAttack",
                "InitializeActors",
            ],
        },
        "preResolutionHandoff": {
            "functionAddresses": {
                "battlesceneScript_ApplyActionEffect": _FUNCTIONS[
                    "battlesceneScript_ApplyActionEffect"
                ]
            },
            "callAddress": 0x9CD0,
            "state": "unentered",
        },
    }


def _retained_r2c(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    fixture = load_json(R2C_FIXTURE)
    if fixture.get("id") != "sf2-map3-battle01-admission-static-v1":
        raise ValueError("Map 3 Battle 01 turn/control retained R2c fixture identity drift")
    fresh = build_map3_battle01_admission_static(rom_path, upstream_path)
    if fixture != fresh:
        raise ValueError("Map 3 Battle 01 turn/control retained R2c fixture projection drift")
    projection = {
        "fixtureId": fixture["id"],
        "fixtureSha256": hashlib.sha256(R2C_FIXTURE.read_bytes()).hexdigest().upper(),
        "admissionStaticSha256": hashlib.sha256(_canonical(fresh["static"])).hexdigest().upper(),
        "retainedAdmissionTurnGenerationAddress": fresh["sourceContext"]["functionAddresses"][
            "GenerateBattleTurnOrder"
        ],
    }
    projection["sha256"] = hashlib.sha256(_canonical(projection)).hexdigest().upper()
    return projection


def _structural_schema() -> dict[str, Any]:
    schema = load_json(SCHEMA)
    fixture = schema.get("$defs", {}).get("fixture")
    if not isinstance(fixture, dict):
        raise ValueError("Map 3 Battle 01 turn/control fixture schema definition is missing")
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
            "Map 3 Battle 01 turn/control structural schema validation failed "
            f"at {location}: {message}"
        )


def build_map3_battle01_turn_control_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the deterministic H2 control spine; no H3 execution is involved."""
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != _ROM_SHA256:
        raise ValueError("Map 3 Battle 01 turn/control canonical ROM SHA-256 drift")
    upstream = upstream_path.resolve(strict=True)
    revision = subprocess.run(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    if revision != _UPSTREAM_COMMIT:
        raise ValueError("Map 3 Battle 01 turn/control upstream revision drift")
    root = _disasm_root(upstream)
    text, source_identities = _read_source_surface(root)
    h1_binary = (upstream / _H1_BINARY).read_bytes()
    rom = rom_path.resolve(strict=True).read_bytes()
    addresses = listing_symbol_addresses((upstream / _LISTING).read_text(encoding="utf-8"))
    if {name: addresses.get(name) for name in _FUNCTIONS} != _FUNCTIONS:
        raise ValueError("Map 3 Battle 01 turn/control H1 symbol projection drift")
    retained = _retained_r2c(rom_path, upstream_path)
    parsed = _validate_source_contract(text)
    toolchain = load_json(TOOLCHAIN)
    output = {
        "schemaVersion": 1,
        "id": ID,
        "system": ID,
        "romSha256": load_json(ROM_MANIFEST)["hashes"]["sha256"],
        "upstream": {
            "repository": toolchain["sf2disasm"]["repository"],
            "commit": toolchain["sf2disasm"]["commit"],
        },
        "retainedR2c": retained,
        "sourceContext": {
            "sourceIdentities": source_identities,
            "h1RomAnchors": _anchor_projection(h1_binary, rom),
        },
        **parsed,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": {
            "sourceFiles": 10,
            "h1RomAnchors": 27,
            "battle01EnemyAssignments": 6,
            "unknowns": 15,
        },
    }
    _validate_structural_output(output)
    return output


def verify_map3_battle01_turn_control_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Validate the checked-in fixture against fresh source/H1/ROM derivation."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    retained_before_output = _retained_r2c(rom_path, upstream_path)
    output = build_map3_battle01_turn_control_static(rom_path, upstream_path)
    retained_at_golden = _retained_r2c(rom_path, upstream_path)
    if (
        retained_before_output != retained_at_golden
        or output["retainedR2c"] != retained_at_golden
        or fixture["retainedR2c"] != retained_at_golden
    ):
        raise ValueError(
            "Map 3 Battle 01 turn/control retained R2c golden-boundary projection drift"
        )
    if fixture != output:
        raise ValueError("Map 3 Battle 01 turn/control complete semantic fixture drift")
    return output
