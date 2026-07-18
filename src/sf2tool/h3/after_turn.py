from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _verify_upstream
from sf2tool.h3.rng import _rng_step
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/after-turn-status-expiry-v1.json")
SCHEMA = repo_path("schemas/h3-after-turn-status-expiry-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/after_turn_status_expiry_observer.lua")


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


def _verify_source_contract(disasm: Path, case: dict[str, Any]) -> None:
    enums = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    expected_equates = {
        "STATUSEFFECT_CURSE": case["unrelatedStatus"],
        "ITEM_NOTHING": case["itemEntries"][0],
        "STATUSEFFECT_SILENCE": case["silenceMask"],
        "STATUSEFFECTCOUNTER_SILENCE": case["silenceCounterUnit"],
        "STATUSEFFECT_SLOW": case["slowMask"],
        "STATUSEFFECTCOUNTER_SLOW": case["slowCounterUnit"],
        "STATUSEFFECT_ATTACK": case["attackMask"],
        "STATUSEFFECTCOUNTER_ATTACK": case["attackCounterUnit"],
        "STATUSEFFECT_BOOST": case["boostMask"],
        "STATUSEFFECTCOUNTER_BOOST": case["boostCounterUnit"],
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
        raise ValueError("after-turn status-expiry source contract drifted")
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
    if any(item != case["itemEntries"][0] for item in case["itemEntries"]):
        raise ValueError("after-turn fixture must provide four empty item slots")


def _decrement_one(status: int, mask: int, unit: int) -> tuple[int, bool]:
    field = status & mask
    if field != unit:
        raise ValueError("after-turn expiry fixture must supply exactly one counter")
    return (status & ~mask) | (field - unit), True


def _model_expected(fixture: dict[str, Any]) -> dict[str, Any]:
    case = fixture["case"]
    composed = (
        case["unrelatedStatus"]
        | case["silenceCounterUnit"]
        | case["slowCounterUnit"]
        | case["attackCounterUnit"]
        | case["boostCounterUnit"]
    )
    if composed != case["initialStatus"]:
        raise ValueError("after-turn initial status disagrees with its component fields")

    seed, raw_roll = _rng_step(case["seed"], case["silenceCounterUnit"])
    masked_roll = raw_roll & case["silenceMask"]
    if masked_roll != 0:
        raise ValueError("after-turn SILENCE seed does not select expiration")
    after_silence = case["initialStatus"] & ~case["silenceMask"]
    after_slow, slow_expired = _decrement_one(
        after_silence, case["slowMask"], case["slowCounterUnit"]
    )
    after_attack, attack_expired = _decrement_one(
        after_slow, case["attackMask"], case["attackCounterUnit"]
    )
    after_boost, boost_expired = _decrement_one(
        after_attack, case["boostMask"], case["boostCounterUnit"]
    )
    return {
        "rng": {
            "seed": case["seed"],
            "range": case["silenceCounterUnit"],
            "observedSeed": seed,
            "rawRoll": raw_roll,
            "maskedRoll": masked_roll,
        },
        "branches": {
            "silenceExpiredEntries": 1,
            "silenceDecrementEntries": 0,
            "updateStatsEntries": 1,
        },
        "messages": {
            "silence": 1,
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
            "final": after_boost & ~case["unrelatedStatus"],
        },
        "stats": {
            "initialAttack": case["initialCurrentAttack"],
            "initialDefense": case["initialCurrentDefense"],
            "initialAgility": case["initialCurrentAgility"],
            "finalAttack": case["baseAttack"],
            "finalDefense": case["baseDefense"],
            "finalAgility": case["baseAgility"],
        },
    }


def verify_after_turn_status_expiry(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 90
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="after-turn status-expiry fixture")
    verify_runtime_contract(fixture, rom_path)
    harness = load_json(repo_path(fixture["sharedHarnessFixture"]))
    _verify_source_contract(_verify_upstream(upstream_path), fixture["case"])
    modeled = _model_expected(fixture)
    if modeled != fixture["expected"]:
        raise ValueError("after-turn status-expiry golden disagrees with source model")
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {**harness["function"], **fixture["function"]},
            "ram": harness["ram"],
            "harness": harness["harness"],
            "case": fixture["case"],
        },
        output_name="after-turn-status-expiry",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["case"]["id"],
        "battle": fixture["battleId"],
        "combatant": fixture["case"]["combatant"],
        **fixture["expected"],
    }
    if observed != expected:
        raise ValueError(
            "after-turn status-expiry runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "InitialStatus": f"0x{modeled['status']['initial']:04X}",
        "FinalStatus": f"0x{modeled['status']['final']:04X}",
        "ExpiryMessages": sum(modeled["messages"].values()),
        "FinalStats": (
            f"ATT={modeled['stats']['finalAttack']},"
            f"DEF={modeled['stats']['finalDefense']},"
            f"AGI={modeled['stats']['finalAgility']}"
        ),
        "Status": "PASS",
    }
