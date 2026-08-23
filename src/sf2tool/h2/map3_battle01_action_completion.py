"""Public H2 contract for Battle 01 action completion through the write return."""

from __future__ import annotations

import hashlib
import json
import re
import subprocess
from pathlib import Path
from typing import Any

from jsonschema import Draft7Validator

from sf2tool.h2 import battle_actions
from sf2tool.h2.map3_battle01_action_effect import (
    FIXTURE as R3B_FIXTURE,
)
from sf2tool.h2.map3_battle01_action_effect import (
    build_map3_battle01_action_effect_static,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

ID = "sf2-map3-battle01-action-completion-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map3-battle01-action-completion-static-v1.json")
SCHEMA = repo_path("schemas/h2/map3-battle01-action-completion-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")

_LISTING = Path("build/sf2build-h1.lst")
_H1_BINARY = Path("build/sf2build-h1.bin")
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"

_SOURCE_SURFACE = (
    "code/gameflow/battle/battleactions/battleactionsengine_1.asm",
    "code/gameflow/battle/battleactions/battleactionsengine_2.asm",
    "code/gameflow/battle/battleactions/animateaction.asm",
    "code/gameflow/battle/battleactions/breakuseditem.asm",
    "code/gameflow/battle/battleactions/isabletocounterattack.asm",
    "code/gameflow/battle/battleactions/createbattlescenemessage.asm",
    "code/gameflow/battle/battleactions/createbattlesceneanimation.asm",
    "code/gameflow/battle/battleactions/giveexpandgold.asm",
    "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
)

_FUNCTIONS = {
    "WriteBattlesceneScript": 0x9B92,
    "battlesceneScript_DetermineTargetsByAction": 0x9DD6,
    "battlesceneScript_DisplayActionMessage": 0xA05C,
    "battlesceneScript_PerformAnimation": 0xA200,
    "battlesceneScript_End": 0xA34E,
    "battlesceneScript_ValidateDoubleAttack": 0xA45E,
    "battlesceneScript_ValidateCounterAttack": 0xA49C,
    "battlesceneScript_SwitchTargets": 0xA702,
    "battlesceneScript_MakeActorIdle": 0xA7D0,
    "battlesceneScript_GiveExpAndGold": 0xA7F8,
    "battlesceneScript_BreakUsedItem": 0xBBE6,
    "ExecuteIndividualTurn": 0x23EB0,
    "j_WriteBattlesceneScript": 0x820C,
}
_APPLY_ACTION_EFFECT_ADDRESS = 0xA3F4
_DROP_ENEMY_ITEM_ADDRESS = 0xBD24
_CALLEE_ADDRESSES = {
    **_FUNCTIONS,
    "battlesceneScript_ApplyActionEffect": _APPLY_ACTION_EFFECT_ADDRESS,
    "battlesceneScript_DropEnemyItem": _DROP_ENEMY_ITEM_ADDRESS,
}

# Each anchor has one exact source/H1/ROM use in the bounded completion spine.
_ANCHORS = (
    ("actionCompletionSpine.mainRange", 0x9CD8, 0xFE, 0x9DD6),
    ("actionCompletionSpine.primaryTargetLoop.resume", 0x9CD8, 8, None),
    ("actionCompletionSpine.followupBranches.itemBreak.idleCall", 0x9CE0, 4, None),
    ("actionCompletionSpine.followupBranches.itemBreak.breakCall", 0x9CE4, 4, None),
    ("actionCompletionSpine.followupBranches.doubleAttack.validatorCall", 0x9CF0, 4, None),
    ("actionCompletionSpine.followupBranches.doubleAttack.decision", 0x9CF4, 6, 0x9CFA),
    ("actionCompletionSpine.followupBranches.doubleAttack.block", 0x9CFA, 0x44, 0x9D3E),
    ("actionCompletionSpine.startResumes.secondAttack", 0x9D3A, 4, None),
    ("actionCompletionSpine.followupBranches.counterAttack.validatorCall", 0x9D46, 4, None),
    ("actionCompletionSpine.followupBranches.counterAttack.decision", 0x9D4A, 6, 0x9D50),
    ("actionCompletionSpine.followupBranches.counterAttack.block", 0x9D50, 0x4C, 0x9D9C),
    ("actionCompletionSpine.startResumes.counterAttack", 0x9D98, 4, None),
    ("actionCompletionSpine.explosionBackedge.range", 0x9D9C, 0x28, 0x9DC4),
    ("actionCompletionSpine.endSequence.writeRange", 0x9DC4, 0x12, 0x9DD6),
    ("actionCompletionSpine.endSequence.battlesceneEndRange", 0xA34E, 0xA6, 0xA3F4),
    ("actionCompletionSpine.followupBranches.doubleAttack.validatorRange", 0xA45E, 0x3E, 0xA49C),
    ("actionCompletionSpine.followupBranches.counterAttack.validatorRange", 0xA49C, 0xB2, 0xA54E),
    (
        "actionCompletionSpine.functionAddresses.battlesceneScript_SwitchTargets",
        0xA702,
        0xCE,
        0xA7D0,
    ),
    (
        "actionCompletionSpine.functionAddresses.battlesceneScript_MakeActorIdle",
        0xA7D0,
        0x28,
        0xA7F8,
    ),
    (
        "actionCompletionSpine.functionAddresses.battlesceneScript_BreakUsedItem",
        0xBBE6,
        0x10A,
        0xBCF0,
    ),
    ("actionCompletionSpine.endSequence.determineTargetsEntry", 0x9DD6, 2, None),
    (
        "actionCompletionSpine.functionAddresses.battlesceneScript_DisplayActionMessage",
        0xA05C,
        2,
        None,
    ),
    ("actionCompletionSpine.functionAddresses.battlesceneScript_PerformAnimation", 0xA200, 2, None),
    ("actionCompletionSpine.functionAddresses.battlesceneScript_GiveExpAndGold", 0xA7F8, 2, None),
    ("actionCompletionSpine.executeIndividualTurnHandoff.writeCall", 0x24100, 6, None),
    ("actionCompletionSpine.executeIndividualTurnHandoff.resume", 0x24106, 4, None),
)

_OWNER_RECORD_IDS = (
    "battle.actions.engine",
    "battle.actions.break-used-item",
    "battle.actions.perform-animation",
    "battle.actions.display-message",
    "battle.actions.animate",
    "battle.followup.validate-double",
    "battle.followup.validate-counter",
    "battle.replay.end",
    "battle.replay.give-exp-and-gold",
    "battle.functions.execute-turn",
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
)


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


def _function_section(source: str, entry_label: str, context: str) -> str:
    raw_lines = source.splitlines()
    matches = [
        index for index, line in enumerate(raw_lines) if _normalized(line).strip() == entry_label
    ]
    if len(matches) != 1:
        raise ValueError(
            "Map 3 Battle 01 action/completion source-use drift in "
            f"{context}: expected exactly one {entry_label}"
        )
    end_marker = f"End of function {entry_label.rstrip(':')}"
    for end_index in range(matches[0] + 1, len(raw_lines)):
        if end_marker in raw_lines[end_index]:
            return "\n".join(raw_lines[matches[0] : end_index])
    raise ValueError(
        f"Map 3 Battle 01 action/completion source-use drift in {context}: missing {end_marker}"
    )


def _require_order(
    source: str,
    expected: tuple[str, ...],
    context: str,
    *,
    function_entry: str | None = None,
) -> None:
    bounded_source = (
        _function_section(source, function_entry, context) if function_entry else source
    )
    lines = _normalized(bounded_source).splitlines()
    cursor = 0
    for fragment in expected:
        for found in range(cursor, len(lines)):
            if lines[found] == fragment:
                break
        else:
            raise ValueError(
                f"Map 3 Battle 01 action/completion source-use drift in {context}: {fragment}"
            )
        cursor = found + 1


def _read_source_surface(root: Path) -> tuple[dict[str, str], list[dict[str, str]]]:
    text: dict[str, str] = {}
    identities: list[dict[str, str]] = []
    for relative in _SOURCE_SURFACE:
        path = root / relative
        if not path.is_file():
            raise ValueError(f"Map 3 Battle 01 action/completion source is missing: {relative}")
        data = path.read_bytes()
        identities.append({"path": relative, "sha256": hashlib.sha256(data).hexdigest().upper()})
        text[relative] = data.decode("utf-8").replace("\r\n", "\n")
    if len(identities) != 9:
        raise ValueError("Map 3 Battle 01 action/completion source denominator drift")
    return text, identities


def _validate_source_contract(text: dict[str, str]) -> dict[str, str]:
    """Guard the bounded source order without assigning runtime branch outcomes."""
    engine_1 = text["code/gameflow/battle/battleactions/battleactionsengine_1.asm"]
    engine_2 = text["code/gameflow/battle/battleactions/battleactionsengine_2.asm"]
    animate = text["code/gameflow/battle/battleactions/animateaction.asm"]
    break_item = text["code/gameflow/battle/battleactions/breakuseditem.asm"]
    counter = text["code/gameflow/battle/battleactions/isabletocounterattack.asm"]
    message = text["code/gameflow/battle/battleactions/createbattlescenemessage.asm"]
    animation = text["code/gameflow/battle/battleactions/createbattlesceneanimation.asm"]
    rewards = text["code/gameflow/battle/battleactions/giveexpandgold.asm"]
    execute = text["code/gameflow/battle/battlefunctions/executeindividualturn.asm"]

    _require_order(
        engine_1,
        (
            "@ApplyActionOnTargets_Loop:",
            "bsr.w battlesceneScript_SwitchTargets",
            "bsr.w battlesceneScript_ApplyActionEffect",
            "bsr.w battlesceneScript_DropEnemyItem",
            "addq.w #1,a5",
            "moveq #2,d6",
            "dbf d7,@ApplyActionOnTargets_Loop",
            "bsr.w battlesceneScript_MakeActorIdle",
            "bsr.w battlesceneScript_BreakUsedItem",
            "bsr.w battlesceneScript_ValidateDoubleAttack",
            "tst.b doubleAttack(a2)",
            "beq.s @CounterAttack",
            "move.w #BATTLEACTION_ATTACKTYPE_SECOND,((BATTLESCENE_ATTACK_TYPE-$1000000)).w",
            "makeActorIdleAndEndAnimation",
            "exg a4,a5",
            "bsr.w battlesceneScript_SwitchTargets",
            "exg a4,a5",
            "bsr.w battlesceneScript_DisplayActionMessage",
            "bsr.w battlesceneScript_PerformAnimation",
            "bsr.w battlesceneScript_SwitchTargets",
            "bsr.w battlesceneScript_ApplyActionEffect",
            "bsr.w battlesceneScript_DropEnemyItem",
            "bsr.w battlesceneScript_MakeActorIdle",
            "@CounterAttack:",
            "lea ((TARGETS_LIST-$1000000)).w,a4",
            "lea ((BATTLESCENE_ACTOR-$1000000)).w,a5",
            "bsr.w battlesceneScript_ValidateCounterAttack",
            "tst.b counterAttack(a2)",
            "beq.s @CheckExplode",
            "move.w #BATTLEACTION_ATTACKTYPE_COUNTER,((BATTLESCENE_ATTACK_TYPE-$1000000)).w",
            "move.b d1,ineffectiveAttackToggle(a2)",
            "makeActorIdleAndEndAnimation",
            "exg a4,a5",
            "bsr.w battlesceneScript_SwitchTargets",
            "exg a4,a5",
            "lea ((BATTLESCENE_ACTOR-$1000000)).w,a5",
            "bsr.w battlesceneScript_DisplayActionMessage",
            "bsr.w battlesceneScript_PerformAnimation",
            "bsr.w battlesceneScript_SwitchTargets",
            "bsr.w battlesceneScript_ApplyActionEffect",
            "bsr.w battlesceneScript_DropEnemyItem",
            "bsr.w battlesceneScript_MakeActorIdle",
            "@CheckExplode:",
            "lea ((BATTLESCENE_ACTOR-$1000000)).w,a4",
            "lea ((TARGETS_LIST-$1000000)).w,a5",
            "tst.b explode(a2)",
            "beq.s @End",
            "move.b #0,explode(a2)",
            "move.w #BATTLEACTION_BURST_ROCK,(a3)",
            "move.b explodingActor(a2),(a4)",
            "makeActorIdleAndEndAnimation",
            "bsr.w battlesceneScript_DetermineTargetsByAction",
            "bra.w @Continue",
            "@End:",
            "bsr.w battlesceneScript_End",
            "unlk a2",
            "movem.l (sp)+,d0-a6",
            "rts",
        ),
        "completion engine spine",
        function_entry="WriteBattlesceneScript:",
    )
    _function_section(
        engine_1,
        "battlesceneScript_DetermineTargetsByAction:",
        "target-determination entry",
    )
    _require_order(
        engine_2,
        (
            "battlesceneScript_End:",
            "endAnimation",
            "bsr.w battlesceneScript_SwitchTargets",
            "tst.b curseInaction(a2)",
            "tst.b silencedActor(a2)",
            "tst.b stunInaction(a2)",
            "bsr.w battlesceneScript_GiveExpAndGold",
            "lea allCombatantsCurrentHpTable(a2),a0",
            "move.w #COMBATANT_ALLIES_START,d0",
            "bra.s loc_A3BE",
            "loc_A3BC:",
            "addq.w #1,d0",
            "loc_A3BE:",
            "cmpi.w #COMBATANT_ALLIES_END,d0",
            "bgt.s loc_A3CE",
            "move.w -(a0),d1",
            "jsr SetCurrentHp",
            "bra.s loc_A3BC",
            "loc_A3CE:",
            "move.w #COMBATANT_ENEMIES_START,d0",
            "bra.s loc_A3D6",
            "loc_A3D4:",
            "addq.w #1,d0",
            "loc_A3D6:",
            "cmpi.w #COMBATANT_ENEMIES_END,d0",
            "bgt.s byte_A3E6",
            "move.w -(a0),d1",
            "jsr SetCurrentHp",
            "bra.s loc_A3D4",
            "byte_A3E6:",
            "bscHideTextBox",
            "bscEnd",
            "rts",
        ),
        "battlescene end sequence",
        function_entry="battlesceneScript_End:",
    )
    _require_order(
        engine_2,
        (
            "battlesceneScript_ValidateDoubleAttack:",
            "tst.b doubleAttack(a2)",
            "tst.b targetDies(a2)",
            "tst.b muddledActor(a2)",
            "tst.b targetIsOnSameSide(a2)",
            "clr.b doubleAttack(a2)",
            "tst.b debugDouble(a2)",
            "move.b #-1,doubleAttack(a2)",
            "rts",
        ),
        "double validator",
        function_entry="battlesceneScript_ValidateDoubleAttack:",
    )
    _require_order(
        counter,
        (
            "battlesceneScript_ValidateCounterAttack:",
            "tst.b counterAttack(a2)",
            "tst.b targetDies(a2)",
            "tst.b muddledActor(a2)",
            "tst.b targetIsOnSameSide(a2)",
            "jsr j_GetStatusEffects",
            "andi.w #STATUSEFFECT_SLEEP,d1",
            "andi.w #STATUSEFFECT_STUN,d1",
            "jsr GetDistanceBetweenCombatants",
            "jsr GetAttackRange",
            "clr.b counterAttack(a2)",
            "tst.b debugCounter(a2)",
            "move.b #-1,counterAttack(a2)",
            "rts",
        ),
        "counter validator",
        function_entry="battlesceneScript_ValidateCounterAttack:",
    )
    _require_order(
        animate,
        (
            "battlesceneScript_SwitchTargets:",
            "jsr GetCurrentHp",
            "tst.w d1",
            "beq.w @Done",
            "bscHideTextBox",
            "rts",
        ),
        "target-switch entry",
        function_entry="battlesceneScript_SwitchTargets:",
    )
    _require_order(
        animate,
        (
            "battlesceneScript_MakeActorIdle:",
            "jsr GetCurrentHp",
            "tst.w d1",
            "beq.w @Done",
            "makeAllyIdle",
            "makeEnemyIdle",
            "rts",
        ),
        "actor-idle entry",
        function_entry="battlesceneScript_MakeActorIdle:",
    )
    _require_order(
        break_item,
        (
            "battlesceneScript_BreakUsedItem:",
            "cmpi.w #BATTLEACTION_USE_ITEM,(a3)",
            "jsr GetEquipmentType",
            "jsr GetItemDefinitionAddress",
            "btst #ITEMTYPE_BIT_BREAKABLE,ITEMDEF_OFFSET_TYPE(a0)",
            "btst #COMBATANT_BIT_ENEMY,(a4)",
            "btst #ITEMENTRY_BIT_BROKEN,d0",
            "jsr (GenerateRandomOrDebugNumber).w",
            "jsr BreakItemBySlot",
            "jsr RemoveItemBySlot",
            "rts",
        ),
        "item break routes",
        function_entry="battlesceneScript_BreakUsedItem:",
    )
    _require_order(
        message,
        ("battlesceneScript_DisplayActionMessage:", "movem.l d0-d3/a0,-(sp)", "rts"),
        "action message entry",
        function_entry="battlesceneScript_DisplayActionMessage:",
    )
    _require_order(
        animation,
        ("battlesceneScript_PerformAnimation:", "movem.l d0-d3/a0,-(sp)", "rts"),
        "action animation entry",
        function_entry="battlesceneScript_PerformAnimation:",
    )
    _require_order(
        rewards,
        ("battlesceneScript_GiveExpAndGold:", "movem.l d0-d1/a0,-(sp)", "rts"),
        "experience and gold entry",
        function_entry="battlesceneScript_GiveExpAndGold:",
    )
    _require_order(
        execute,
        (
            "@WriteBattlesceneScript:",
            "jsr (WaitForVInt).w",
            "jsr (WaitForVInt).w",
            "move.w combatant(a6),d0",
            "jsr j_WriteBattlesceneScript",
            "move.w combatant(a6),d0",
        ),
        "individual-turn write handoff",
    )
    return {"sourceContract": "confirmed"}


def _word(data: bytes, address: int) -> int:
    value = data[address : address + 2]
    if len(value) != 2:
        raise ValueError(f"Map 3 Battle 01 action/completion H1 word is truncated at {address:#x}")
    return int.from_bytes(value, "big")


def _long(data: bytes, address: int) -> int:
    value = data[address : address + 4]
    if len(value) != 4:
        raise ValueError(f"Map 3 Battle 01 action/completion H1 long is truncated at {address:#x}")
    return int.from_bytes(value, "big")


def _require_relative_target(data: bytes, address: int, opcode: int, expected: int) -> None:
    if _word(data, address) != opcode:
        raise ValueError(f"Map 3 Battle 01 action/completion opcode drift at {address:#x}")
    displacement = int.from_bytes(data[address + 2 : address + 4], "big", signed=True)
    target = address + 2 + displacement
    if target != expected:
        raise ValueError(
            "Map 3 Battle 01 action/completion target drift at "
            f"{address:#x}: expected {expected:#x}, got {target:#x}"
        )


def _require_short_target(data: bytes, address: int, opcode: int, expected: int) -> None:
    instruction = _word(data, address)
    if instruction >> 8 != opcode:
        raise ValueError(f"Map 3 Battle 01 action/completion branch opcode drift at {address:#x}")
    displacement = int.from_bytes(bytes((instruction & 0xFF,)), "big", signed=True)
    target = address + 2 + displacement
    if target != expected:
        raise ValueError(
            "Map 3 Battle 01 action/completion branch target drift at "
            f"{address:#x}: expected {expected:#x}, got {target:#x}"
        )


def _require_bsr_target(data: bytes, address: int, expected: int) -> None:
    _require_relative_target(data, address, 0x6100, expected)


def _require_move_immediate(data: bytes, address: int, expected: int, destination: int) -> None:
    if _word(data, address) != 0x31FC or _word(data, address + 2) != expected:
        raise ValueError(f"Map 3 Battle 01 action/completion immediate move drift at {address:#x}")
    if _word(data, address + 4) != destination:
        raise ValueError(
            f"Map 3 Battle 01 action/completion move destination drift at {address:#x}"
        )


def _anchor_projection(h1_binary: bytes, rom: bytes) -> list[dict[str, Any]]:
    anchors: list[dict[str, Any]] = []
    for identifier, address, width, end_address in _anchor_guard_order():
        h1 = h1_binary[address : address + width]
        if len(h1) != width or rom[address : address + width] != h1:
            raise ValueError(f"Map 3 Battle 01 action/completion H1/ROM anchor drift: {identifier}")
        item: dict[str, Any] = {
            "id": identifier,
            "address": address,
            "width": width,
            "sha256": hashlib.sha256(h1).hexdigest().upper(),
        }
        if end_address is not None:
            item["endAddressExclusive"] = end_address
        anchors.append(item)
    if len(anchors) != 26:
        raise ValueError("Map 3 Battle 01 action/completion H1/ROM anchor denominator drift")
    return [
        next(item for item in anchors if item["id"] == identifier) for identifier, *_ in _ANCHORS
    ]


def _anchor_guard_order() -> tuple[tuple[str, int, int, int | None], ...]:
    """Check narrow anchors before their containing ranges for precise mutation diagnostics."""
    return tuple(sorted(_ANCHORS, key=lambda anchor: (anchor[2], anchor[1], anchor[0])))


def _parse_action_completion(h1_binary: bytes) -> dict[str, Any]:
    """Derive the static post-Drop completion topology from guarded H1 use sites."""
    if h1_binary[0x9CD8:0x9CE0] != bytes.fromhex("524D7C0251CFFFD8"):
        raise ValueError("Map 3 Battle 01 action/completion primary loop resume drift")
    _require_bsr_target(h1_binary, 0x9CE0, _FUNCTIONS["battlesceneScript_MakeActorIdle"])
    _require_bsr_target(h1_binary, 0x9CE4, _FUNCTIONS["battlesceneScript_BreakUsedItem"])
    _require_bsr_target(h1_binary, 0x9CF0, _FUNCTIONS["battlesceneScript_ValidateDoubleAttack"])
    if h1_binary[0x9CF4:0x9CF8] != bytes.fromhex("4A2AFFF3"):
        raise ValueError("Map 3 Battle 01 action/completion double decision test drift")
    _require_short_target(h1_binary, 0x9CF8, 0x67, 0x9D3E)
    _require_move_immediate(h1_binary, 0x9CFA, 1, 0xB636)
    for address, target in (
        (0x9D20, "battlesceneScript_SwitchTargets"),
        (0x9D26, "battlesceneScript_DisplayActionMessage"),
        (0x9D2A, "battlesceneScript_PerformAnimation"),
        (0x9D2E, "battlesceneScript_SwitchTargets"),
        (0x9D32, "battlesceneScript_ApplyActionEffect"),
        (0x9D36, "battlesceneScript_DropEnemyItem"),
        (0x9D3A, "battlesceneScript_MakeActorIdle"),
    ):
        _require_bsr_target(
            h1_binary,
            address,
            _CALLEE_ADDRESSES[target],
        )
    _require_bsr_target(h1_binary, 0x9D46, _FUNCTIONS["battlesceneScript_ValidateCounterAttack"])
    if h1_binary[0x9D4A:0x9D4E] != bytes.fromhex("4A2AFFF4"):
        raise ValueError("Map 3 Battle 01 action/completion counter decision test drift")
    _require_short_target(h1_binary, 0x9D4E, 0x67, 0x9D9C)
    _require_move_immediate(h1_binary, 0x9D50, 2, 0xB636)
    for address, target in (
        (0x9D7A, "battlesceneScript_SwitchTargets"),
        (0x9D84, "battlesceneScript_DisplayActionMessage"),
        (0x9D88, "battlesceneScript_PerformAnimation"),
        (0x9D8C, "battlesceneScript_SwitchTargets"),
        (0x9D90, "battlesceneScript_ApplyActionEffect"),
        (0x9D94, "battlesceneScript_DropEnemyItem"),
        (0x9D98, "battlesceneScript_MakeActorIdle"),
    ):
        _require_bsr_target(
            h1_binary,
            address,
            _CALLEE_ADDRESSES[target],
        )
    if h1_binary[0x9DA4:0x9DA8] != bytes.fromhex("4A2AFFF0"):
        raise ValueError("Map 3 Battle 01 action/completion explosion test drift")
    _require_short_target(h1_binary, 0x9DA8, 0x67, 0x9DC4)
    if h1_binary[0x9DAA:0x9DB8] != bytes.fromhex("157C0000FFF036BC000418AAFFEF"):
        raise ValueError("Map 3 Battle 01 action/completion explosion setup drift")
    _require_bsr_target(h1_binary, 0x9DBC, _FUNCTIONS["battlesceneScript_DetermineTargetsByAction"])
    _require_relative_target(h1_binary, 0x9DC0, 0x6000, 0x9C7E)
    if h1_binary[0x9DC4:0x9DCA] != bytes.fromhex("11F8B64FB64E"):
        raise ValueError("Map 3 Battle 01 action/completion end actor-restore drift")
    _require_bsr_target(h1_binary, 0x9DCA, _FUNCTIONS["battlesceneScript_End"])
    if h1_binary[0x9DCE:0x9DD6] != bytes.fromhex("4E5A4CDF7FFF4E75"):
        raise ValueError("Map 3 Battle 01 action/completion write return drift")
    if h1_binary[0xA34E:0xA352] != bytes.fromhex("48E7F080"):
        raise ValueError("Map 3 Battle 01 action/completion End entry drift")
    _require_bsr_target(h1_binary, 0xA35C, _FUNCTIONS["battlesceneScript_SwitchTargets"])
    _require_bsr_target(h1_binary, 0xA3AE, _FUNCTIONS["battlesceneScript_GiveExpAndGold"])
    if h1_binary[0xA3E6:0xA3F4] != bytes.fromhex("3CFC00123CFCFFFF4CDF010F4E75"):
        raise ValueError("Map 3 Battle 01 action/completion End return drift")
    if h1_binary[0xA45E:0xA462] != bytes.fromhex("48E7F080"):
        raise ValueError("Map 3 Battle 01 action/completion double validator entry drift")
    if h1_binary[0xA496:0xA49C] != bytes.fromhex("4CDF010F4E75"):
        raise ValueError("Map 3 Battle 01 action/completion double validator end drift")
    if h1_binary[0xA49C:0xA4A0] != bytes.fromhex("48E7F080"):
        raise ValueError("Map 3 Battle 01 action/completion counter validator entry drift")
    if h1_binary[0xA548:0xA54E] != bytes.fromhex("4CDF010F4E75"):
        raise ValueError("Map 3 Battle 01 action/completion counter validator return drift")
    if h1_binary[0xA702:0xA706] != bytes.fromhex("48E7C000"):
        raise ValueError("Map 3 Battle 01 action/completion switch-targets entry drift")
    if h1_binary[0xA7D0:0xA7D4] != bytes.fromhex("48E74000"):
        raise ValueError("Map 3 Battle 01 action/completion make-idle entry drift")
    if h1_binary[0xBBE6:0xBBEA] != bytes.fromhex("48E7F080"):
        raise ValueError("Map 3 Battle 01 action/completion break-item entry drift")
    if (
        _word(h1_binary, 0x24100) != 0x4EB9
        or _long(h1_binary, 0x24102) != _FUNCTIONS["j_WriteBattlesceneScript"]
    ):
        raise ValueError("Map 3 Battle 01 action/completion write handoff instruction drift")
    if _word(h1_binary, _FUNCTIONS["j_WriteBattlesceneScript"]) != 0x4EFA:
        raise ValueError("Map 3 Battle 01 action/completion write alias opcode drift")
    alias_displacement = int.from_bytes(
        h1_binary[
            _FUNCTIONS["j_WriteBattlesceneScript"] + 2 : _FUNCTIONS["j_WriteBattlesceneScript"] + 4
        ],
        "big",
        signed=True,
    )
    if (
        _FUNCTIONS["j_WriteBattlesceneScript"] + 2 + alias_displacement
        != _FUNCTIONS["WriteBattlesceneScript"]
    ):
        raise ValueError("Map 3 Battle 01 action/completion write alias effective target drift")
    if h1_binary[0x24106:0x2410A] != bytes.fromhex("302EFFFE"):
        raise ValueError("Map 3 Battle 01 action/completion write handoff resume drift")

    return {
        "functionAddresses": _FUNCTIONS,
        "startResumes": {
            "primaryTargetLoop": 0x9CD8,
            "secondAttack": 0x9D3A,
            "counterAttack": 0x9D98,
        },
        "primaryTargetLoop": {
            "resumeAddress": 0x9CD8,
            "orderedSteps": ["targetAdvance", "directionSet", "dbfBackedge"],
            "backedgeAddress": 0x9CDC,
            "backedgeTarget": 0x9CB6,
            "counterRegister": "d7",
        },
        "followupBranches": {
            "itemBreak": {
                "idleCallAddress": 0x9CE0,
                "breakCallAddress": 0x9CE4,
                "sourceRoutes": [
                    "actionType",
                    "equipmentType",
                    "breakableBit",
                    "actorSide",
                    "brokenBit",
                    "randomResult",
                    "breakOrRemove",
                ],
            },
            "doubleAttack": {
                "validatorCallAddress": 0x9CF0,
                "validatorRangeEndExclusive": 0xA49C,
                "decisionRange": [0x9CF4, 0x9CFA],
                "zeroBranchTarget": 0x9D3E,
                "attackType": "SECOND",
                "blockRange": [0x9CFA, 0x9D3E],
                "orderedCalls": [
                    "SwitchTargets",
                    "DisplayActionMessage",
                    "PerformAnimation",
                    "SwitchTargets",
                    "ApplyActionEffect",
                    "DropEnemyItem",
                    "MakeActorIdle",
                ],
            },
            "counterAttack": {
                "validatorCallAddress": 0x9D46,
                "validatorReturnAddress": 0xA54C,
                "decisionRange": [0x9D4A, 0x9D50],
                "zeroBranchTarget": 0x9D9C,
                "attackType": "COUNTER",
                "blockRange": [0x9D50, 0x9D9C],
                "orderedCalls": [
                    "SwitchTargets",
                    "DisplayActionMessage",
                    "PerformAnimation",
                    "SwitchTargets",
                    "ApplyActionEffect",
                    "DropEnemyItem",
                    "MakeActorIdle",
                ],
            },
        },
        "explosionBackedge": {
            "range": [0x9D9C, 0x9DC4],
            "zeroBranchTarget": 0x9DC4,
            "orderedSteps": [
                "clearExplode",
                "setBurstRock",
                "restoreExplodingActor",
                "idleAndEndAnimation",
                "DetermineTargetsByAction",
                "backedge",
            ],
            "backedgeAddress": 0x9DC0,
            "backedgeTarget": 0x9C7E,
        },
        "endSequence": {
            "writeRange": [0x9DC4, 0x9DD6],
            "battlesceneEndRange": [0xA34E, 0xA3F4],
            "orderedWriteSteps": [
                "restoreActorCopy",
                "battlesceneScript_End",
                "stackRelease",
                "return",
            ],
            "orderedEndSteps": [
                "endAnimation",
                "SwitchTargets",
                "rewardGate",
                "GiveExpAndGold",
                "currentHpReplay",
                "hideTextBox",
                "endCommand",
                "return",
            ],
            "returnAddress": 0x9DD4,
            "determineTargetsEntry": 0x9DD6,
            "currentHpReplayEndAddress": 0xA3E6,
        },
        "executeIndividualTurnHandoff": {
            "callAddress": 0x24100,
            "resumeAddress": 0x24106,
            "instructionTarget": "j_WriteBattlesceneScript",
            "instructionTargetAddress": _FUNCTIONS["j_WriteBattlesceneScript"],
            "effectiveTarget": "WriteBattlesceneScript",
            "effectiveTargetAddress": _FUNCTIONS["WriteBattlesceneScript"],
        },
    }


def _retained_r3b(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    fixture = load_json(R3B_FIXTURE)
    if fixture.get("id") != "sf2-map3-battle01-action-effect-static-v1":
        raise ValueError("Map 3 Battle 01 action/completion retained R3b fixture identity drift")
    fresh = build_map3_battle01_action_effect_static(rom_path, upstream_path)
    if fixture != fresh:
        raise ValueError("Map 3 Battle 01 action/completion retained R3b fixture projection drift")
    projection = {
        "fixtureId": fixture["id"],
        "fixtureSha256": hashlib.sha256(R3B_FIXTURE.read_bytes()).hexdigest().upper(),
        "actionEffectStaticSha256": hashlib.sha256(_canonical(fresh)).hexdigest().upper(),
    }
    projection["sha256"] = hashlib.sha256(_canonical(projection)).hexdigest().upper()
    return projection


def _retained_battle_actions(upstream_path: Path) -> dict[str, Any]:
    fixture = load_json(battle_actions.FIXTURE)
    if fixture.get("id") != battle_actions.ID:
        raise ValueError(
            "Map 3 Battle 01 action/completion retained battle-actions fixture identity drift"
        )
    fresh = battle_actions.build_battle_actions_inventory(upstream_path)
    summary = fresh.get("summary")
    if not isinstance(summary, dict) or summary.get("indexedRecordCount") != 47:
        raise ValueError(
            "Map 3 Battle 01 action/completion retained battle-actions indexed record count drift"
        )
    if summary.get("indexedFileCount") != 29:
        raise ValueError(
            "Map 3 Battle 01 action/completion retained battle-actions indexed path count drift"
        )
    required_record_ids = {
        "battle.actions.apply-effect-dispatch",
        "battle.actions.cast-spell",
    }
    missing_record_ids = sorted(required_record_ids - set(fresh.get("indexedRecordIds", [])))
    if missing_record_ids:
        raise ValueError(
            "Map 3 Battle 01 action/completion retained battle-actions indexed record IDs drift: "
            + ", ".join(missing_record_ids)
        )
    engine = fixture["expected"]["actionFacts"]["engine"]
    if engine != fresh["actionFacts"]["engine"]:
        raise ValueError(
            "Map 3 Battle 01 action/completion retained battle-actions engine projection drift"
        )
    projection = {
        "fixtureId": fixture["id"],
        "fixtureSha256": hashlib.sha256(battle_actions.FIXTURE.read_bytes()).hexdigest().upper(),
        "semanticEngineSha256": hashlib.sha256(_canonical(engine)).hexdigest().upper(),
    }
    projection["sha256"] = hashlib.sha256(_canonical(projection)).hexdigest().upper()
    return projection


def _owner_record_ids(index: dict[str, Any]) -> list[str]:
    expected = {
        "battle.actions.engine": "WriteBattlesceneScript",
        "battle.actions.break-used-item": "battlesceneScript_BreakUsedItem",
        "battle.actions.perform-animation": "battlesceneScript_PerformAnimation",
        "battle.actions.display-message": "battlesceneScript_DisplayActionMessage",
        "battle.actions.animate": "battlesceneScript_AnimateAction",
        "battle.followup.validate-double": "battlesceneScript_ValidateDoubleAttack",
        "battle.followup.validate-counter": "battlesceneScript_ValidateCounterAttack",
        "battle.replay.end": "battlesceneScript_End",
        "battle.replay.give-exp-and-gold": "battlesceneScript_GiveExpAndGold",
        "battle.functions.execute-turn": "ExecuteIndividualTurn",
    }
    records = {record["id"]: record for record in index["records"]}
    if tuple(expected) != _OWNER_RECORD_IDS:
        raise ValueError("Map 3 Battle 01 action/completion owner record declaration drift")
    for record_id, symbol in expected.items():
        record = records.get(record_id)
        if record is None or record["symbol"] != symbol:
            raise ValueError(f"Map 3 Battle 01 action/completion owner record drift: {record_id}")
    secondary_labels = {
        "battle.actions.engine": {
            "id": "determine-targets",
            "space": "rom",
            "kind": "observation",
            "value": _FUNCTIONS["battlesceneScript_DetermineTargetsByAction"],
            "symbol": "battlesceneScript_DetermineTargetsByAction",
        },
        "battle.actions.animate": {
            "id": "switch-targets",
            "space": "rom",
            "kind": "observation",
            "value": _FUNCTIONS["battlesceneScript_SwitchTargets"],
            "symbol": "battlesceneScript_SwitchTargets",
        },
    }
    expected_idle_label = {
        "id": "make-actor-idle",
        "space": "rom",
        "kind": "observation",
        "value": _FUNCTIONS["battlesceneScript_MakeActorIdle"],
        "symbol": "battlesceneScript_MakeActorIdle",
    }
    for record_id, expected_label in secondary_labels.items():
        record = records[record_id]
        if not any(address == expected_label for address in record["addresses"]):
            raise ValueError(
                "Map 3 Battle 01 action/completion secondary label drift: "
                f"{expected_label['symbol']}"
            )
    animate_addresses = records["battle.actions.animate"]["addresses"]
    if not any(address == expected_idle_label for address in animate_addresses):
        raise ValueError(
            "Map 3 Battle 01 action/completion secondary label drift: "
            f"{expected_idle_label['symbol']}"
        )
    return list(_OWNER_RECORD_IDS)


def _structural_schema() -> dict[str, Any]:
    schema = load_json(SCHEMA)
    fixture = schema.get("$defs", {}).get("fixture")
    if not isinstance(fixture, dict):
        raise ValueError("Map 3 Battle 01 action/completion fixture schema definition is missing")
    return {"$schema": schema["$schema"], "$ref": "#/$defs/fixture", "$defs": schema["$defs"]}


def _validate_structural_output(value: dict[str, Any]) -> None:
    errors = sorted(
        Draft7Validator(_structural_schema()).iter_errors(value),
        key=lambda error: list(error.absolute_path),
    )
    if errors:
        location = ".".join(str(part) for part in errors[0].absolute_path) or "<root>"
        raise ValueError(
            "Map 3 Battle 01 action/completion structural schema validation failed "
            f"at {location}: {errors[0].message}"
        )


def build_map3_battle01_action_completion_static(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Build the deterministic H2 action-completion spine; no H3 execution is involved."""
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != _ROM_SHA256:
        raise ValueError("Map 3 Battle 01 action/completion canonical ROM SHA-256 drift")
    upstream = upstream_path.resolve(strict=True)
    revision = subprocess.run(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    if revision != _UPSTREAM_COMMIT:
        raise ValueError("Map 3 Battle 01 action/completion upstream revision drift")
    root = _disasm_root(upstream)
    text, source_identities = _read_source_surface(root)
    h1_binary = (upstream / _H1_BINARY).read_bytes()
    rom = rom_path.resolve(strict=True).read_bytes()
    addresses = listing_symbol_addresses((upstream / _LISTING).read_text(encoding="utf-8"))
    if {name: addresses.get(name) for name in _FUNCTIONS} != _FUNCTIONS:
        raise ValueError("Map 3 Battle 01 action/completion H1 symbol projection drift")
    _validate_source_contract(text)
    retained_r3b_before = _retained_r3b(rom_path, upstream_path)
    retained_actions_before = _retained_battle_actions(upstream_path)
    spine = _parse_action_completion(h1_binary)
    retained_r3b_after = _retained_r3b(rom_path, upstream_path)
    retained_actions_after = _retained_battle_actions(upstream_path)
    if (
        retained_r3b_before != retained_r3b_after
        or retained_actions_before != retained_actions_after
    ):
        raise ValueError(
            "Map 3 Battle 01 action/completion pre-construction retained projection drift"
        )
    spine["ownerRecordIds"] = _owner_record_ids(load_json(RESEARCH_INDEX))
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
        "summary": {
            "sourceFiles": 9,
            "h1RomAnchors": 26,
            "indexObjects": 10,
            "indexBindings": 20,
            "battleActionsIndexedRecords": 47,
            "battleActionsIndexedPaths": 29,
            "unknowns": 33,
        },
        "retainedR3b": retained_r3b_after,
        "retainedBattleActions": retained_actions_after,
        "sourceContext": {
            "sourceIdentities": source_identities,
            "h1RomAnchors": _anchor_projection(h1_binary, rom),
        },
        "actionCompletionSpine": spine,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
    }
    _validate_structural_output(output)
    return output


def verify_map3_battle01_action_completion_static(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Validate the checked-in fixture against fresh source/H1/ROM derivation."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    retained_r3b_before = _retained_r3b(rom_path, upstream_path)
    retained_actions_before = _retained_battle_actions(upstream_path)
    output = build_map3_battle01_action_completion_static(rom_path, upstream_path)
    retained_r3b_at_golden = _retained_r3b(rom_path, upstream_path)
    retained_actions_at_golden = _retained_battle_actions(upstream_path)
    if (
        retained_r3b_before != retained_r3b_at_golden
        or retained_actions_before != retained_actions_at_golden
        or output["retainedR3b"] != retained_r3b_at_golden
        or output["retainedBattleActions"] != retained_actions_at_golden
        or fixture["retainedR3b"] != retained_r3b_at_golden
        or fixture["retainedBattleActions"] != retained_actions_at_golden
    ):
        raise ValueError(
            "Map 3 Battle 01 action/completion retained golden-boundary projection drift"
        )
    if fixture != output:
        raise ValueError("Map 3 Battle 01 action/completion complete semantic fixture drift")
    return output
