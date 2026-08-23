"""Public H2 contract for Battle 01 ApplyActionEffect through DropEnemyItem."""

from __future__ import annotations

import hashlib
import json
import re
import subprocess
from pathlib import Path
from typing import Any

from jsonschema import Draft7Validator

from sf2tool.h2 import battle_actions
from sf2tool.h2.map3_battle01_turn_control import (
    FIXTURE as R3A_FIXTURE,
)
from sf2tool.h2.map3_battle01_turn_control import (
    build_map3_battle01_turn_control_static,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

ID = "sf2-map3-battle01-action-effect-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map3-battle01-action-effect-static-v1.json")
SCHEMA = repo_path("schemas/h2/map3-battle01-action-effect-static-fixture.schema.json")
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
    "code/gameflow/battle/battleactions/attack.asm",
    "code/gameflow/battle/battleactions/castspell.asm",
    "code/gameflow/battle/battleactions/useitem.asm",
    "code/gameflow/battle/battleactions/inflictdamage.asm",
    "code/gameflow/battle/battleactions/displaydeathmessage.asm",
    "code/gameflow/battle/battleactions/dropenemyitem.asm",
)

_FUNCTIONS = {
    "WriteBattlesceneScript": 0x9B92,
    "battlesceneScript_ApplyActionEffect": 0xA3F4,
    "battlesceneScript_Attack": 0xAAB6,
    "battlesceneScript_CastSpell": 0xB0A8,
    "battlesceneScript_UseItem": 0xBBB8,
    "battlesceneScript_InflictDamage": 0xACEA,
    "battlesceneScript_DisplayDeathMessage": 0xB080,
    "battlesceneScript_DropEnemyItem": 0xBD24,
}

_ANCHORS = (
    ("callerContexts.targetLoop.applyCall", 0x9CD0, 4, None),
    ("callerContexts.targetLoop.dropCall", 0x9CD4, 4, None),
    ("callerContexts.secondAttack.applyCall", 0x9D32, 4, None),
    ("callerContexts.secondAttack.dropCall", 0x9D36, 4, None),
    ("callerContexts.counterAttack.applyCall", 0x9D90, 4, None),
    ("callerContexts.counterAttack.dropCall", 0x9D94, 4, None),
    ("actionEffectSpine.applyActionEffectRange", 0xA3F4, 0x6A, 0xA45E),
    ("actionEffectSpine.dispatch.attackCall", 0xA3FE, 4, None),
    ("actionEffectSpine.dispatch.castSpellCall", 0xA40A, 4, None),
    ("actionEffectSpine.dispatch.useItemCall", 0xA416, 4, None),
    ("actionEffectSpine.dispatch.burstRockInflictDamageCall", 0xA426, 4, None),
    ("actionEffectSpine.dispatch.burstRockDisplayDeathCall", 0xA430, 4, None),
    ("actionEffectSpine.dispatch.prismLaserInflictDamageCall", 0xA44A, 4, None),
    ("actionEffectSpine.dispatch.prismLaserDisplayDeathCall", 0xA454, 4, None),
    ("actionEffectSpine.returnConvergence", 0xA458, 6, 0xA45E),
    ("functionAddresses.battlesceneScript_Attack", 0xAAB6, 2, None),
    ("functionAddresses.battlesceneScript_CastSpell", 0xB0A8, 2, None),
    ("functionAddresses.battlesceneScript_UseItem", 0xBBB8, 2, None),
    ("functionAddresses.battlesceneScript_InflictDamage", 0xACEA, 2, None),
    ("functionAddresses.battlesceneScript_DisplayDeathMessage", 0xB080, 2, None),
    ("functionAddresses.battlesceneScript_DropEnemyItem", 0xBD24, 2, None),
)

_SELECTOR_USE_SITES = (
    (0xA3F8, "BATTLEACTION_ATTACK", "Attack"),
    (0xA404, "BATTLEACTION_CAST_SPELL", "CastSpell"),
    (0xA410, "BATTLEACTION_USE_ITEM", "UseItem"),
    (0xA41C, "BATTLEACTION_BURST_ROCK", "BurstRock"),
    (0xA436, "BATTLEACTION_MUDDLED", "Muddled"),
    (0xA440, "BATTLEACTION_PRISM_LASER", "PrismLaser"),
)
_POWER_USE_SITES = (
    (0xA422, "BATTLEACTION_BURST_ROCK_POWER"),
    (0xA446, "BATTLEACTION_PRISM_LASER_POWER"),
)
_DISPATCH_CALL_SITES = (
    (0xA3FE, "battlesceneScript_Attack"),
    (0xA40A, "battlesceneScript_CastSpell"),
    (0xA416, "battlesceneScript_UseItem"),
    (0xA426, "battlesceneScript_InflictDamage"),
    (0xA430, "battlesceneScript_DisplayDeathMessage"),
    (0xA44A, "battlesceneScript_InflictDamage"),
    (0xA454, "battlesceneScript_DisplayDeathMessage"),
)
_CALLER_CONTEXTS = (
    ("targetLoop", 0x9CD0, 0x9CD4, 0x9CD8),
    ("secondAttack", 0x9D32, 0x9D36, 0x9D3A),
    ("counterAttack", 0x9D90, 0x9D94, 0x9D98),
)
_OWNER_RECORD_IDS = (
    "battle.actions.engine",
    "battle.actions.apply-effect-dispatch",
    "battle.actions.attack",
    "battle.actions.cast-spell",
    "battle.actions.use-item",
    "battle.damage.inflict",
    "battle.actions.display-death",
    "battle.reward.drop-enemy-item",
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


def _require_order(source: str, expected: tuple[str, ...], context: str) -> None:
    lines = _normalized(source).splitlines()
    cursor = 0
    for fragment in expected:
        for found in range(cursor, len(lines)):
            if lines[found] == fragment:
                break
        else:
            raise ValueError(
                f"Map 3 Battle 01 action/effect {context} source-use drift: {fragment}"
            )
        cursor = found + 1


def _read_source_surface(root: Path) -> tuple[dict[str, str], list[dict[str, str]]]:
    text: dict[str, str] = {}
    identities: list[dict[str, str]] = []
    for relative in _SOURCE_SURFACE:
        path = root / relative
        if not path.is_file():
            raise ValueError(f"Map 3 Battle 01 action/effect source is missing: {relative}")
        data = path.read_bytes()
        identities.append({"path": relative, "sha256": hashlib.sha256(data).hexdigest().upper()})
        text[relative] = data.decode("utf-8").replace("\r\n", "\n")
    if len(identities) != 8:
        raise ValueError("Map 3 Battle 01 action/effect source denominator drift")
    return text, identities


def _validate_source_contract(text: dict[str, str]) -> dict[str, Any]:
    """Guard only the bounded static ApplyActionEffect and caller topology."""
    engine = text["code/gameflow/battle/battleactions/battleactionsengine_1.asm"]
    effects = text["code/gameflow/battle/battleactions/battleactionsengine_2.asm"]
    attack = text["code/gameflow/battle/battleactions/attack.asm"]
    cast_spell = text["code/gameflow/battle/battleactions/castspell.asm"]
    use_item = text["code/gameflow/battle/battleactions/useitem.asm"]
    inflict = text["code/gameflow/battle/battleactions/inflictdamage.asm"]
    death = text["code/gameflow/battle/battleactions/displaydeathmessage.asm"]
    drop = text["code/gameflow/battle/battleactions/dropenemyitem.asm"]

    if effects.splitlines()[138] != "battlesceneScript_ApplyActionEffect:":
        raise ValueError("Map 3 Battle 01 action/effect ApplyActionEffect source label/line drift")
    if cast_spell.splitlines()[12] != "battlesceneScript_CastSpell:":
        raise ValueError("Map 3 Battle 01 action/effect CastSpell source label/line drift")

    _require_order(
        effects,
        (
            "battlesceneScript_ApplyActionEffect:",
            "movem.l d0-d3/a0,-(sp)",
            "cmpi.w #BATTLEACTION_ATTACK,(a3)",
            "bne.s @IsCastSpell",
            "bsr.w battlesceneScript_Attack",
            "bra.s @Done",
            "@IsCastSpell:",
            "cmpi.w #BATTLEACTION_CAST_SPELL,(a3)",
            "bne.s @IsUseItem",
            "bsr.w battlesceneScript_CastSpell",
            "bra.s @Done",
            "@IsUseItem:",
            "cmpi.w #BATTLEACTION_USE_ITEM,(a3)",
            "bne.s @IsBurstRock",
            "bsr.w battlesceneScript_UseItem",
            "bra.s @Done",
            "@IsBurstRock:",
            "cmpi.w #BATTLEACTION_BURST_ROCK,(a3)",
            "bne.s @IsMuddled",
            "move.w #BATTLEACTION_BURST_ROCK_POWER,d6",
            "bsr.w battlesceneScript_InflictDamage",
            "tst.b targetDies(a2)",
            "beq.s @Goto_Done",
            "bsr.w battlesceneScript_DisplayDeathMessage",
            "@Goto_Done:",
            "bra.s @Done",
            "@IsMuddled:",
            "cmpi.w #BATTLEACTION_MUDDLED,(a3)",
            "bne.w @IsPrismLaser",
            "bra.s @Done",
            "@IsPrismLaser:",
            "cmpi.w #BATTLEACTION_PRISM_LASER,(a3)",
            "bne.s @Done",
            "move.w #BATTLEACTION_PRISM_LASER_POWER,d6",
            "bsr.w battlesceneScript_InflictDamage",
            "tst.b targetDies(a2)",
            "beq.s @Done",
            "bsr.w battlesceneScript_DisplayDeathMessage",
            "@Done:",
            "movem.l (sp)+,d0-d3/a0",
            "rts",
        ),
        "ApplyActionEffect dispatcher",
    )
    _require_order(
        engine,
        (
            "@ApplyActionOnTargets_Loop:",
            "bsr.w battlesceneScript_ApplyActionEffect",
            "bsr.w battlesceneScript_DropEnemyItem",
            "addq.w #1,a5",
        ),
        "primary Apply-to-Drop caller",
    )
    _require_order(
        engine,
        (
            "move.w #BATTLEACTION_ATTACKTYPE_SECOND,((BATTLESCENE_ATTACK_TYPE-$1000000)).w",
            "bsr.w battlesceneScript_ApplyActionEffect",
            "bsr.w battlesceneScript_DropEnemyItem",
            "bsr.w battlesceneScript_MakeActorIdle",
        ),
        "second Apply-to-Drop caller",
    )
    _require_order(
        engine,
        (
            "move.w #BATTLEACTION_ATTACKTYPE_COUNTER,((BATTLESCENE_ATTACK_TYPE-$1000000)).w",
            "bsr.w battlesceneScript_ApplyActionEffect",
            "bsr.w battlesceneScript_DropEnemyItem",
            "bsr.w battlesceneScript_MakeActorIdle",
        ),
        "counter Apply-to-Drop caller",
    )
    for source, label in (
        (attack, "battlesceneScript_Attack:"),
        (cast_spell, "battlesceneScript_CastSpell:"),
        (use_item, "battlesceneScript_UseItem:"),
        (inflict, "battlesceneScript_InflictDamage:"),
        (death, "battlesceneScript_DisplayDeathMessage:"),
        (drop, "battlesceneScript_DropEnemyItem:"),
    ):
        _require_order(source, (label,), f"owner entry {label}")
    return {"sourceContract": "confirmed"}


def _anchor_projection(h1_binary: bytes, rom: bytes) -> list[dict[str, Any]]:
    anchors: list[dict[str, Any]] = []
    for identifier, address, width, end_address in _ANCHORS:
        h1 = h1_binary[address : address + width]
        if len(h1) != width or rom[address : address + width] != h1:
            raise ValueError(f"Map 3 Battle 01 action/effect H1/ROM anchor drift: {identifier}")
        item: dict[str, Any] = {
            "id": identifier,
            "address": address,
            "width": width,
            "sha256": hashlib.sha256(h1).hexdigest().upper(),
        }
        if end_address is not None:
            item["endAddressExclusive"] = end_address
        anchors.append(item)
    if len(anchors) != 21:
        raise ValueError("Map 3 Battle 01 action/effect H1/ROM anchor denominator drift")
    return anchors


def _word(data: bytes, address: int) -> int:
    value = data[address : address + 2]
    if len(value) != 2:
        raise ValueError(f"Map 3 Battle 01 action/effect H1 word is truncated at {address:#x}")
    return int.from_bytes(value, "big")


def _require_cmpi_selector(h1_binary: bytes, address: int) -> int:
    if _word(h1_binary, address) != 0x0C53:
        raise ValueError(f"Map 3 Battle 01 action/effect selector opcode drift at {address:#x}")
    return _word(h1_binary, address + 2)


def _require_move_power(h1_binary: bytes, address: int) -> int:
    if _word(h1_binary, address) != 0x3C3C:
        raise ValueError(f"Map 3 Battle 01 action/effect power opcode drift at {address:#x}")
    return _word(h1_binary, address + 2)


def _require_bsr_target(h1_binary: bytes, address: int, expected: int) -> None:
    if _word(h1_binary, address) != 0x6100:
        raise ValueError(f"Map 3 Battle 01 action/effect call opcode drift at {address:#x}")
    displacement = int.from_bytes(h1_binary[address + 2 : address + 4], "big", signed=True)
    target = address + 2 + displacement
    if target != expected:
        raise ValueError(
            "Map 3 Battle 01 action/effect call target drift at "
            f"{address:#x}: expected {expected:#x}, got {target:#x}"
        )


def _parse_h1_dispatch(h1_binary: bytes) -> tuple[list[dict[str, Any]], dict[str, int]]:
    selectors = [
        (source_selector, action, _require_cmpi_selector(h1_binary, address))
        for address, source_selector, action in _SELECTOR_USE_SITES
    ]
    if [selector for _source, _action, selector in selectors] != [0, 1, 2, 4, 5, 6]:
        raise ValueError("Map 3 Battle 01 action/effect selector values drift")
    powers = {
        source_constant: _require_move_power(h1_binary, address)
        for address, source_constant in _POWER_USE_SITES
    }
    if powers != {
        "BATTLEACTION_BURST_ROCK_POWER": 18,
        "BATTLEACTION_PRISM_LASER_POWER": 16,
    }:
        raise ValueError("Map 3 Battle 01 action/effect power values drift")
    for address, symbol in _DISPATCH_CALL_SITES:
        _require_bsr_target(h1_binary, address, _FUNCTIONS[symbol])
    if h1_binary[0xA42A:0xA42E] != bytes.fromhex("4A2AFFFC"):
        raise ValueError("Map 3 Battle 01 action/effect Burst Rock targetDies test drift")
    if h1_binary[0xA42E:0xA430] != bytes.fromhex("6704"):
        raise ValueError("Map 3 Battle 01 action/effect Burst Rock targetDies polarity drift")
    if h1_binary[0xA44E:0xA452] != bytes.fromhex("4A2AFFFC"):
        raise ValueError("Map 3 Battle 01 action/effect Prism Laser targetDies test drift")
    if h1_binary[0xA452:0xA454] != bytes.fromhex("6704"):
        raise ValueError("Map 3 Battle 01 action/effect Prism Laser targetDies polarity drift")
    if h1_binary[0xA458:0xA45E] != bytes.fromhex("4CDF010F4E75"):
        raise ValueError("Map 3 Battle 01 action/effect return convergence drift")

    by_action = {
        action: (source_selector, selector) for source_selector, action, selector in selectors
    }
    dispatch = [
        {
            "selector": by_action["Attack"][1],
            "sourceSelector": by_action["Attack"][0],
            "action": "Attack",
            "power": None,
            "primaryCall": "battlesceneScript_Attack",
            "targetDiesFalse": None,
            "targetDiesTrue": None,
        },
        {
            "selector": by_action["CastSpell"][1],
            "sourceSelector": by_action["CastSpell"][0],
            "action": "CastSpell",
            "power": None,
            "primaryCall": "battlesceneScript_CastSpell",
            "targetDiesFalse": None,
            "targetDiesTrue": None,
        },
        {
            "selector": by_action["UseItem"][1],
            "sourceSelector": by_action["UseItem"][0],
            "action": "UseItem",
            "power": None,
            "primaryCall": "battlesceneScript_UseItem",
            "targetDiesFalse": None,
            "targetDiesTrue": None,
        },
        {
            "selector": by_action["BurstRock"][1],
            "sourceSelector": by_action["BurstRock"][0],
            "action": "BurstRock",
            "power": powers["BATTLEACTION_BURST_ROCK_POWER"],
            "primaryCall": "battlesceneScript_InflictDamage",
            "targetDiesFalse": "Done",
            "targetDiesTrue": "battlesceneScript_DisplayDeathMessage",
        },
        {
            "selector": by_action["Muddled"][1],
            "sourceSelector": by_action["Muddled"][0],
            "action": "Muddled",
            "power": None,
            "primaryCall": None,
            "targetDiesFalse": None,
            "targetDiesTrue": None,
        },
        {
            "selector": by_action["PrismLaser"][1],
            "sourceSelector": by_action["PrismLaser"][0],
            "action": "PrismLaser",
            "power": powers["BATTLEACTION_PRISM_LASER_POWER"],
            "primaryCall": "battlesceneScript_InflictDamage",
            "targetDiesFalse": "Done",
            "targetDiesTrue": "battlesceneScript_DisplayDeathMessage",
        },
        {
            "selector": "default",
            "sourceSelector": None,
            "action": "Done",
            "power": None,
            "primaryCall": None,
            "targetDiesFalse": None,
            "targetDiesTrue": None,
        },
    ]
    return dispatch, powers


def _parse_caller_contexts(h1_binary: bytes) -> list[dict[str, Any]]:
    contexts: list[dict[str, Any]] = []
    for identifier, apply_call, drop_call, resume in _CALLER_CONTEXTS:
        _require_bsr_target(
            h1_binary, apply_call, _FUNCTIONS["battlesceneScript_ApplyActionEffect"]
        )
        _require_bsr_target(h1_binary, drop_call, _FUNCTIONS["battlesceneScript_DropEnemyItem"])
        if resume != drop_call + 4:
            raise ValueError(f"Map 3 Battle 01 action/effect resume width drift: {identifier}")
        contexts.append(
            {
                "id": identifier,
                "applyCallAddress": apply_call,
                "dropCallAddress": drop_call,
                "resumeAddress": resume,
            }
        )
    return contexts


def _retained_r3a(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    fixture = load_json(R3A_FIXTURE)
    if fixture.get("id") != "sf2-map3-battle01-turn-control-static-v1":
        raise ValueError("Map 3 Battle 01 action/effect retained R3a fixture identity drift")
    fresh = build_map3_battle01_turn_control_static(rom_path, upstream_path)
    if fixture != fresh:
        raise ValueError("Map 3 Battle 01 action/effect retained R3a fixture projection drift")
    projection = {
        "fixtureId": fixture["id"],
        "fixtureSha256": hashlib.sha256(R3A_FIXTURE.read_bytes()).hexdigest().upper(),
        "turnControlStaticSha256": hashlib.sha256(_canonical(fresh)).hexdigest().upper(),
    }
    projection["sha256"] = hashlib.sha256(_canonical(projection)).hexdigest().upper()
    return projection


def _retained_battle_actions(upstream_path: Path) -> dict[str, Any]:
    fixture = load_json(battle_actions.FIXTURE)
    if fixture.get("id") != battle_actions.ID:
        raise ValueError(
            "Map 3 Battle 01 action/effect retained battle-actions fixture identity drift"
        )
    fresh = battle_actions.build_battle_actions_inventory(upstream_path)
    summary = fresh.get("summary")
    if not isinstance(summary, dict) or summary.get("indexedRecordCount") != 47:
        raise ValueError(
            "Map 3 Battle 01 action/effect retained battle-actions indexed record count drift"
        )
    if summary.get("indexedFileCount") != 29:
        raise ValueError(
            "Map 3 Battle 01 action/effect retained battle-actions indexed path count drift"
        )
    required_record_ids = {
        "battle.actions.apply-effect-dispatch",
        "battle.actions.cast-spell",
    }
    if not required_record_ids <= set(fresh.get("indexedRecordIds", [])):
        raise ValueError(
            "Map 3 Battle 01 action/effect retained battle-actions indexed record IDs drift"
        )
    engine = fixture["expected"]["actionFacts"]["engine"]
    if engine != fresh["actionFacts"]["engine"]:
        raise ValueError(
            "Map 3 Battle 01 action/effect retained battle-actions engine projection drift"
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
        "battle.actions.engine": (
            "WriteBattlesceneScript",
            "code/gameflow/battle/battleactions/battleactionsengine_1.asm",
        ),
        "battle.actions.apply-effect-dispatch": (
            "battlesceneScript_ApplyActionEffect",
            "code/gameflow/battle/battleactions/battleactionsengine_2.asm",
        ),
        "battle.actions.attack": (
            "battlesceneScript_Attack",
            "code/gameflow/battle/battleactions/attack.asm",
        ),
        "battle.actions.cast-spell": (
            "battlesceneScript_CastSpell",
            "code/gameflow/battle/battleactions/castspell.asm",
        ),
        "battle.actions.use-item": (
            "battlesceneScript_UseItem",
            "code/gameflow/battle/battleactions/useitem.asm",
        ),
        "battle.damage.inflict": (
            "battlesceneScript_InflictDamage",
            "code/gameflow/battle/battleactions/inflictdamage.asm",
        ),
        "battle.actions.display-death": (
            "battlesceneScript_DisplayDeathMessage",
            "code/gameflow/battle/battleactions/displaydeathmessage.asm",
        ),
        "battle.reward.drop-enemy-item": (
            "battlesceneScript_DropEnemyItem",
            "code/gameflow/battle/battleactions/dropenemyitem.asm",
        ),
    }
    records = {record["id"]: record for record in index["records"]}
    if tuple(expected) != _OWNER_RECORD_IDS:
        raise ValueError("Map 3 Battle 01 action/effect owner record declaration drift")
    for record_id, (symbol, source_path) in expected.items():
        record = records.get(record_id)
        if record is None or (record["symbol"], record["sourcePath"]) != (symbol, source_path):
            raise ValueError(f"Map 3 Battle 01 action/effect owner record drift: {record_id}")
        entry = next((address for address in record["addresses"] if address["id"] == "entry"), None)
        if entry is None or entry["value"] != _FUNCTIONS[symbol]:
            raise ValueError(f"Map 3 Battle 01 action/effect owner entry drift: {record_id}")
    return list(_OWNER_RECORD_IDS)


def _structural_schema() -> dict[str, Any]:
    schema = load_json(SCHEMA)
    fixture = schema.get("$defs", {}).get("fixture")
    if not isinstance(fixture, dict):
        raise ValueError("Map 3 Battle 01 action/effect fixture schema definition is missing")
    return {"$schema": schema["$schema"], "$ref": "#/$defs/fixture", "$defs": schema["$defs"]}


def _validate_structural_output(value: dict[str, Any]) -> None:
    errors = sorted(
        Draft7Validator(_structural_schema()).iter_errors(value),
        key=lambda error: list(error.absolute_path),
    )
    if errors:
        location = ".".join(str(part) for part in errors[0].absolute_path) or "<root>"
        raise ValueError(
            "Map 3 Battle 01 action/effect structural schema validation failed "
            f"at {location}: {errors[0].message}"
        )


def build_map3_battle01_action_effect_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the deterministic H2 ApplyActionEffect spine; no H3 execution is involved."""
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != _ROM_SHA256:
        raise ValueError("Map 3 Battle 01 action/effect canonical ROM SHA-256 drift")
    upstream = upstream_path.resolve(strict=True)
    revision = subprocess.run(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    if revision != _UPSTREAM_COMMIT:
        raise ValueError("Map 3 Battle 01 action/effect upstream revision drift")
    root = _disasm_root(upstream)
    text, source_identities = _read_source_surface(root)
    h1_binary = (upstream / _H1_BINARY).read_bytes()
    rom = rom_path.resolve(strict=True).read_bytes()
    addresses = listing_symbol_addresses((upstream / _LISTING).read_text(encoding="utf-8"))
    if {name: addresses.get(name) for name in _FUNCTIONS} != _FUNCTIONS:
        raise ValueError("Map 3 Battle 01 action/effect H1 symbol projection drift")
    _validate_source_contract(text)
    dispatch, _powers = _parse_h1_dispatch(h1_binary)
    caller_contexts = _parse_caller_contexts(h1_binary)
    retained_r3a = _retained_r3a(rom_path, upstream_path)
    retained_battle_actions = _retained_battle_actions(upstream_path)
    owner_record_ids = _owner_record_ids(load_json(RESEARCH_INDEX))
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
            "sourceFiles": 8,
            "h1RomAnchors": 21,
            "callerContexts": 3,
            "actionSelectors": 7,
            "indexObjects": 8,
            "indexBindings": 8,
            "battleActionsIndexedRecords": 47,
            "battleActionsIndexedPaths": 29,
            "unknowns": 23,
        },
        "retainedR3a": retained_r3a,
        "retainedBattleActions": retained_battle_actions,
        "sourceContext": {
            "sourceIdentities": source_identities,
            "h1RomAnchors": _anchor_projection(h1_binary, rom),
        },
        "actionEffectSpine": {
            "functionAddresses": _FUNCTIONS,
            "dispatch": dispatch,
            "callerContexts": caller_contexts,
            "rewardConvergence": {
                "dropOwnerRecordId": "battle.reward.drop-enemy-item",
                "resumeAddresses": [context["resumeAddress"] for context in caller_contexts],
                "boundary": "DropEnemyItemReturn",
            },
            "ownerRecordIds": owner_record_ids,
        },
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
    }
    _validate_structural_output(output)
    return output


def verify_map3_battle01_action_effect_static(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Validate the checked-in fixture against fresh source/H1/ROM derivation."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    retained_r3a_before = _retained_r3a(rom_path, upstream_path)
    retained_actions_before = _retained_battle_actions(upstream_path)
    output = build_map3_battle01_action_effect_static(rom_path, upstream_path)
    retained_r3a_at_golden = _retained_r3a(rom_path, upstream_path)
    retained_actions_at_golden = _retained_battle_actions(upstream_path)
    if (
        retained_r3a_before != retained_r3a_at_golden
        or retained_actions_before != retained_actions_at_golden
        or output["retainedR3a"] != retained_r3a_at_golden
        or output["retainedBattleActions"] != retained_actions_at_golden
        or fixture["retainedR3a"] != retained_r3a_at_golden
        or fixture["retainedBattleActions"] != retained_actions_at_golden
    ):
        raise ValueError("Map 3 Battle 01 action/effect retained golden-boundary projection drift")
    if fixture != output:
        raise ValueError("Map 3 Battle 01 action/effect complete semantic fixture drift")
    return output
