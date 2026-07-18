from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-healing-v1.json")
SCHEMA = repo_path("schemas/h3-spell-healing-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_healing_observer.lua")


def _verify_source_contract(disasm: Path, case: dict[str, Any]) -> None:
    spell_defs = (disasm / "data/stats/spells/spelldefs.asm").read_text(encoding="utf-8")
    heal_1 = re.search(
        r"entry\s+HEAL\s*;\s*HEAL 1(?P<body>.*?)(?=\n\s*entry\s+)",
        spell_defs,
        re.DOTALL,
    )
    if not heal_1:
        raise ValueError("pinned spell definitions do not contain HEAL 1")
    power = re.search(r"^\s*power\s+(\d+)\s*$", heal_1.group("body"), re.MULTILINE)
    cost = re.search(r"^\s*mpCost\s+(\d+)\s*$", heal_1.group("body"), re.MULTILINE)
    if not power or int(power.group(1)) != case["spellPower"]:
        raise ValueError("HEAL 1 power disagrees with the fixture")
    if not cost or int(cost.group(1)) != case["spellMpCost"]:
        raise ValueError("HEAL 1 MP cost disagrees with the fixture")

    cast_spell = (
        disasm / "code/gameflow/battle/battleactions/castspell.asm"
    ).read_text(encoding="utf-8")
    exp_source = (
        disasm / "code/gameflow/battle/battleactions/earnexp.asm"
    ).read_text(encoding="utf-8")
    cast_fragments = (
        "move.b  SPELLDEF_OFFSET_POWER(a0),d6",
        "bsr.w   AdjustSpellPower",
        "move.w  d2,d6",
        "bsr.w   battlesceneScript_CalculateHealingExp",
    )
    exp_fragments = (
        "cmpi.b  #CLASS_PRST,d1",
        "move.w  #HEALING_SPELL_EXP_MAX,d5",
        "mulu.w  d6,d5",
        "moveq   #HEALING_SPELL_EXP_MIN,d5",
        "cmpi.w  #HEALING_ACTION_EXP_CAP,((BATTLESCENE_EXP-$1000000)).w",
    )
    if any(fragment not in cast_spell for fragment in cast_fragments):
        raise ValueError("healing spell source contract drifted")
    if any(fragment not in exp_source for fragment in exp_fragments):
        raise ValueError("healing EXP source contract drifted")


def _model_expected(fixture: dict[str, Any]) -> dict[str, Any]:
    case = fixture["case"]
    missing_hp = case["actorMaxHp"] - case["actorInitialHp"]
    adjusted_power = case["spellPower"]
    if case["healerClass"] >= 12:
        adjusted_power = (adjusted_power * 5) >> 2
    recovery = min(missing_hp, adjusted_power)
    healing_exp = max((25 * recovery) // case["actorMaxHp"], 10)
    accumulated_exp = min(healing_exp, 25)
    halved = (
        accumulated_exp // 2
        if fixture["battleId"] == 1 and not case["targetSameSide"]
        else accumulated_exp
    )
    seed, first_roll = _rng_step(case["seed"], 16)
    randomized = halved + int(first_roll == 0)
    _, second_roll = _rng_step(seed, 16)
    randomized -= int(second_roll == 0)
    command_exp = max(randomized, 1)
    return {
        "construction": {
            "missingHp": missing_hp,
            "basePower": case["spellPower"],
            "adjustedPower": adjusted_power,
            "cappedRecovery": recovery,
            "accumulatedExp": accumulated_exp,
            "targetSameSide": case["targetSameSide"],
            "actorHp": case["actorInitialHp"],
            "actorMp": case["actorInitialMp"],
            "award": {
                "seed": case["seed"],
                "halved": halved,
                "firstRoll": first_roll,
                "secondRoll": second_roll,
                "commandExp": command_exp,
            },
        },
        "replay": {
            "allyReactions": [
                {
                    "hpChange": 0,
                    "mpChange": -case["spellMpCost"],
                    "hpBefore": case["actorInitialHp"],
                    "hpAfter": case["actorInitialHp"],
                    "mpBefore": case["actorInitialMp"],
                    "mpAfter": case["actorInitialMp"] - case["spellMpCost"],
                },
                {
                    "hpChange": recovery,
                    "mpChange": 0,
                    "hpBefore": case["actorInitialHp"],
                    "hpAfter": case["actorInitialHp"] + recovery,
                    "mpBefore": case["actorInitialMp"] - case["spellMpCost"],
                    "mpAfter": case["actorInitialMp"] - case["spellMpCost"],
                },
            ],
            "expReaction": {
                "commandExp": command_exp,
                "expBefore": case["actorInitialExp"],
                "expAfter": case["actorInitialExp"] + command_exp,
            },
            "finalActorHp": case["actorInitialHp"] + recovery,
            "finalActorMp": case["actorInitialMp"] - case["spellMpCost"],
            "finalActorExp": case["actorInitialExp"] + command_exp,
        },
    }


def verify_spell_healing(
    rom_path: Path,
    upstream_path: Path,
    *,
    timeout_seconds: int = 75,
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="spell healing fixture")
    verify_runtime_contract(fixture, rom_path)
    shared = load_json(repo_path(fixture["sharedHarnessFixture"]))
    disasm = _verify_upstream(upstream_path)
    _verify_source_contract(disasm, fixture["case"])
    modeled = _model_expected(fixture)
    if fixture["expected"] != modeled:
        raise ValueError("spell healing golden disagrees with source model")

    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {**shared["function"], **fixture["function"]},
            "ram": shared["ram"],
            "harness": shared["harness"],
            "case": fixture["case"],
        },
        output_name="spell-healing",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["case"]["id"],
        "battle": fixture["battleId"],
        "action": {"type": fixture["case"]["actionType"], "spell": 0, "target": 0},
        **fixture["expected"],
    }
    if observed != expected:
        raise ValueError(
            "spell healing runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Engine": f"BizHawk {fixture['emulator']['version']} / {fixture['emulator']['core']}",
        "Battle": fixture["battleId"],
        "Spell": "HEAL 1",
        "Recovery": (
            f"{modeled['construction']['basePower']}"
            f"->{modeled['construction']['cappedRecovery']}"
        ),
        "AccumulatedExp": modeled["construction"]["accumulatedExp"],
        "CommandExp": modeled["construction"]["award"]["commandExp"],
        "PersistentHp": modeled["replay"]["finalActorHp"],
        "PersistentMp": modeled["replay"]["finalActorMp"],
        "PersistentExp": modeled["replay"]["finalActorExp"],
        "Status": "PASS",
    }
