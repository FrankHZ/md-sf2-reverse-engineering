from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _verify_upstream
from sf2tool.h3.rng import _rng_step
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/after-turn-status-lifecycle-v1.json")
SCHEMA = repo_path("schemas/h3-after-turn-status-lifecycle-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/after_turn_status_lifecycle_observer.lua")


def _equate(source: str, name: str) -> int:
    match = re.search(
        rf"^{re.escape(name)}:\s+equ\s+(\$[0-9A-F]+|[0-9]+)\s*$",
        source,
        re.MULTILINE,
    )
    if not match:
        raise ValueError(f"missing pinned equate: {name}")
    value = match.group(1)
    return int(value[1:], 16) if value.startswith("$") else int(value)


def _verify_source_contract(disasm: Path, constants: dict[str, Any]) -> None:
    enums = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    expected_equates = {
        "STATUSEFFECT_CURSE": constants["unrelatedStatus"],
        "ITEM_NOTHING": constants["itemEntries"][0],
        "STATUSEFFECT_SILENCE": constants["silenceMask"],
        "STATUSEFFECTCOUNTER_SILENCE": constants["silenceCounterUnit"],
        "STATUSEFFECT_SLOW": constants["slowMask"],
        "STATUSEFFECTCOUNTER_SLOW": constants["slowCounterUnit"],
        "STATUSEFFECT_ATTACK": constants["attackMask"],
        "STATUSEFFECTCOUNTER_ATTACK": constants["attackCounterUnit"],
        "STATUSEFFECT_BOOST": constants["boostMask"],
        "STATUSEFFECTCOUNTER_BOOST": constants["boostCounterUnit"],
    }
    for name, expected in expected_equates.items():
        if _equate(enums, name) != expected:
            raise ValueError(f"pinned {name} disagrees with the fixture")

    source = (
        disasm / "code/gameflow/battle/battleloop/processafterturneffects.asm"
    ).read_text(encoding="utf-8")
    fragments = (
        "ProcessAfterTurnEffects:",
        "move.w  d1,d6",
        "jsr     (GenerateRandomNumber).w",
        "andi.w  #STATUSEFFECT_SILENCE,d7",
        "subi.w  #STATUSEFFECTCOUNTER_SILENCE,d1",
        'txt     351             ; "{CLEAR}{SPELL} expired.',
        "subi.w  #STATUSEFFECTCOUNTER_SLOW,d1",
        'txt     349             ; "{CLEAR}{SPELL} expired.',
        "subi.w  #STATUSEFFECTCOUNTER_ATTACK,d1",
        'txt     350             ; "{CLEAR}{SPELL} expired.',
        "subi.w  #STATUSEFFECTCOUNTER_BOOST,d1",
        'txt     348             ; "{CLEAR}{SPELL} expired.',
        "jsr     j_UpdateCombatantStats",
    )
    if any(fragment not in source for fragment in fragments):
        raise ValueError("after-turn status-lifecycle source contract drifted")
    stats_source = (
        disasm / "code/common/stats/updatecombatantstats.asm"
    ).read_text(encoding="utf-8")
    stats_fragments = (
        "andi.w  #STATUSEFFECT_STUN|STATUSEFFECT_POISON|STATUSEFFECT_MUDDLE2|"
        "STATUSEFFECT_MUDDLE|STATUSEFFECT_SLEEP|STATUSEFFECT_SILENCE|"
        "STATUSEFFECT_SLOW|STATUSEFFECT_BOOST|STATUSEFFECT_ATTACK,d3",
        "cmpi.w  #ITEM_NOTHING,d1",
        "ori.w   #STATUSEFFECT_CURSE,d3",
    )
    if any(fragment not in stats_source for fragment in stats_fragments):
        raise ValueError("after-turn final stat/status refresh source contract drifted")
    if any(item != constants["itemEntries"][0] for item in constants["itemEntries"]):
        raise ValueError("after-turn fixture must provide four empty item slots")


def _decrement_field(status: int, mask: int, unit: int) -> tuple[int, bool]:
    field = status & mask
    if field < unit or field % unit:
        raise ValueError("after-turn fixture provides an invalid packed counter")
    updated = field - unit
    return (status & ~mask) | updated, updated == 0


def _expected_current_stats(
    constants: dict[str, Any], case: dict[str, Any], status: int
) -> tuple[int, int, int]:
    attack_setting = (status & constants["attackMask"]) // constants["attackCounterUnit"]
    boost_setting = (status & constants["boostMask"]) // constants["boostCounterUnit"]
    slow_setting = (status & constants["slowMask"]) // constants["slowCounterUnit"]
    attack = case["baseAttack"] + case["baseAttack"] * attack_setting // 8
    defense = (
        case["baseDefense"]
        + case["baseDefense"] * boost_setting // 8
        - case["baseDefense"] * slow_setting // 8
    )
    agility = (
        case["baseAgility"]
        + case["baseAgility"] * boost_setting // 8
        - case["baseAgility"] * slow_setting // 8
    )
    return attack, defense, agility


def _model_case(constants: dict[str, Any], case: dict[str, Any]) -> dict[str, Any]:
    initial_stats = _expected_current_stats(constants, case, case["initialStatus"])
    provided_stats = (
        case["initialCurrentAttack"],
        case["initialCurrentDefense"],
        case["initialCurrentAgility"],
    )
    if initial_stats != provided_stats:
        raise ValueError(f"initial derived stats disagree with packed status: {case['id']}")

    silence_field = case["initialStatus"] & constants["silenceMask"]
    if not silence_field:
        raise ValueError("after-turn lifecycle case lacks a SILENCE counter")
    seed, raw_roll = _rng_step(case["seed"], silence_field)
    masked_roll = raw_roll & constants["silenceMask"]
    silence_expired = masked_roll == 0
    if silence_expired:
        after_silence = case["initialStatus"] & ~constants["silenceMask"]
    else:
        after_silence, _ = _decrement_field(
            case["initialStatus"],
            constants["silenceMask"],
            constants["silenceCounterUnit"],
        )
    after_slow, slow_expired = _decrement_field(
        after_silence, constants["slowMask"], constants["slowCounterUnit"]
    )
    after_attack, attack_expired = _decrement_field(
        after_slow, constants["attackMask"], constants["attackCounterUnit"]
    )
    after_boost, boost_expired = _decrement_field(
        after_attack, constants["boostMask"], constants["boostCounterUnit"]
    )
    normalized_status = after_boost & ~constants["unrelatedStatus"]
    final_attack, final_defense, final_agility = _expected_current_stats(
        constants, case, normalized_status
    )
    return {
        "id": case["id"],
        "combatant": case["combatant"],
        "rng": {
            "seed": case["seed"],
            "range": silence_field,
            "observedSeed": seed,
            "rawRoll": raw_roll,
            "maskedRoll": masked_roll,
        },
        "branches": {
            "silenceExpiredEntries": int(silence_expired),
            "silenceDecrementEntries": int(not silence_expired),
            "updateStatsEntries": 1,
        },
        "messages": {
            "silence": int(silence_expired),
            "slow": int(slow_expired),
            "attack": int(attack_expired),
            "boost": int(boost_expired),
        },
        "status": {
            "initial": case["initialStatus"],
            "afterSilence": after_silence,
            "afterSlow": after_slow,
            "afterAttack": after_attack,
            "afterBoost": after_boost,
            "final": normalized_status,
        },
        "stats": {
            "initialAttack": case["initialCurrentAttack"],
            "initialDefense": case["initialCurrentDefense"],
            "initialAgility": case["initialCurrentAgility"],
            "finalAttack": final_attack,
            "finalDefense": final_defense,
            "finalAgility": final_agility,
        },
    }


def _model_expected(fixture: dict[str, Any]) -> dict[str, Any]:
    constants = fixture["constants"]
    return {"records": [_model_case(constants, case) for case in fixture["cases"]]}


def verify_after_turn_status_lifecycle(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 150
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="after-turn status-lifecycle fixture")
    verify_runtime_contract(fixture, rom_path)
    harness = load_json(repo_path(fixture["sharedHarnessFixture"]))
    _verify_source_contract(_verify_upstream(upstream_path), fixture["constants"])
    modeled = _model_expected(fixture)
    if modeled != fixture["expected"]:
        raise ValueError("after-turn status-lifecycle golden disagrees with source model")
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "fixtureId": fixture["id"],
            "function": {**harness["function"], **fixture["function"]},
            "ram": harness["ram"],
            "harness": harness["harness"],
            "action": fixture["action"],
            "constants": fixture["constants"],
            "cases": fixture["cases"],
        },
        output_name="after-turn-status-lifecycle",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "battle": fixture["battleId"],
        **fixture["expected"],
    }
    if observed != expected:
        raise ValueError(
            "after-turn status-lifecycle runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    records = modeled["records"]
    expired = sum(record["branches"]["silenceExpiredEntries"] for record in records)
    continued = sum(record["branches"]["silenceDecrementEntries"] for record in records)
    return {
        "Fixture": fixture["id"],
        "Cases": len(records),
        "Expiry": f"0x{records[0]['status']['initial']:04X}->0x{records[0]['status']['final']:04X}",
        "Continuation": (
            f"0x{records[1]['status']['initial']:04X}"
            f"->0x{records[1]['status']['final']:04X}"
        ),
        "SilenceBranches": f"expire={expired},continue={continued}",
        "Messages": sum(sum(record["messages"].values()) for record in records),
        "Status": "PASS",
    }
