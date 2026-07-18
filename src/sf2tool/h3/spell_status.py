from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-status-sleep-v1.json")
SCHEMA = repo_path("schemas/h3-spell-status-sleep-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_status_observer.lua")


def _verify_source_contract(disasm: Path, case: dict[str, Any]) -> None:
    spell_defs = (disasm / "data/stats/spells/spelldefs.asm").read_text(encoding="utf-8")
    sleep = re.search(
        r"entry\s+SLEEP\s*;\s*SLEEP 1(?P<body>.*?)(?=\n\s*entry\s+)",
        spell_defs,
        re.DOTALL,
    )
    if not sleep:
        raise ValueError("pinned spell definitions do not contain SLEEP 1")
    cost = re.search(r"^\s*mpCost\s+(\d+)\s*$", sleep.group("body"), re.MULTILINE)
    if not cost or int(cost.group(1)) != case["spellMpCost"]:
        raise ValueError("SLEEP 1 MP cost disagrees with the fixture")
    elements = (disasm / "data/stats/spells/spellelements.asm").read_text(encoding="utf-8")
    if "spellElement STATUS     ; 9: SLEEP" not in elements:
        raise ValueError("SLEEP element disagrees with the fixture")
    cast = (disasm / "code/gameflow/battle/battleactions/castspell.asm").read_text(
        encoding="utf-8"
    )
    exp = (disasm / "code/gameflow/battle/battleactions/earnexp.asm").read_text(
        encoding="utf-8"
    )
    required_cast = (
        "addq.w  #CHANCE_TO_INFLICT_SLEEP,d2",
        "bsr.w   battlesceneScript_DetermineSpellEffectiveness",
        "ori.w   #STATUSEFFECT_SLEEP,d1",
        "bsr.w   battlesceneScript_AddStatusEffectSpellExp",
        "cmp.w   d2,d0",
        "move.b  #-1,dodge(a2)",
    )
    if any(fragment not in cast for fragment in required_cast):
        raise ValueError("SLEEP effectiveness source contract drifted")
    if "moveq   #STATUSEFFECT_SPELL_EXP,d5" not in exp:
        raise ValueError("status-effect EXP source contract drifted")


def _model_expected(fixture: dict[str, Any]) -> dict[str, Any]:
    case = fixture["case"]
    records = []
    accumulated_exp = 0
    final_seed = case["seed"]
    for target in case["targets"]:
        threshold = case["baseThreshold"] + target["setting"]
        final_seed, roll = _rng_step(case["seed"], 8)
        success = roll >= threshold
        if success:
            accumulated_exp = min(accumulated_exp + 5, 49)
        records.append(
            {
                "combatant": target["combatant"],
                "setting": target["setting"],
                "threshold": threshold,
                "roll": roll,
                "success": success,
                "accumulatedExp": accumulated_exp,
                "statusDuringConstruction": 0,
            }
        )
    award_seed = final_seed
    halved = accumulated_exp // 2 if fixture["battleId"] == 1 else accumulated_exp
    seed, first_roll = _rng_step(final_seed, 16)
    randomized = halved + int(first_roll == 0)
    _, second_roll = _rng_step(seed, 16)
    randomized -= int(second_roll == 0)
    command_exp = max(randomized, 1)
    successful = [record for record in records if record["success"]]
    return {
        "construction": {
            "actorMp": case["initialMp"],
            "records": records,
            "award": {
                "accumulatedExp": accumulated_exp,
                "seed": award_seed,
                "halved": halved,
                "firstRoll": first_roll,
                "secondRoll": second_roll,
                "commandExp": command_exp,
            },
        },
        "replay": {
            "allyReaction": {
                "mpChange": -case["spellMpCost"],
                "mpBefore": case["initialMp"],
                "mpAfter": case["initialMp"] - case["spellMpCost"],
            },
            "enemyReactions": [
                {
                    "combatant": record["combatant"],
                    "statusBefore": 0,
                    "statusAfter": case["statusMask"],
                }
                for record in successful
            ],
            "expReaction": {
                "commandExp": command_exp,
                "expBefore": case["actorInitialExp"],
                "expAfter": case["actorInitialExp"] + command_exp,
            },
            "finalActorMp": case["initialMp"] - case["spellMpCost"],
            "finalActorExp": case["actorInitialExp"] + command_exp,
            "finalTargetStatus": [
                case["statusMask"] if record["success"] else 0 for record in records
            ],
        },
    }


def verify_spell_status(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 75
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="spell status fixture")
    verify_runtime_contract(fixture, rom_path)
    shared = load_json(repo_path(fixture["sharedHarnessFixture"]))
    disasm = _verify_upstream(upstream_path)
    _verify_source_contract(disasm, fixture["case"])
    modeled = _model_expected(fixture)
    if fixture["expected"] != modeled:
        raise ValueError("spell status golden disagrees with source model")
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {**shared["function"], **fixture["function"]},
            "ram": shared["ram"],
            "harness": shared["harness"],
            "case": fixture["case"],
        },
        output_name="spell-status-sleep",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["case"]["id"],
        "battle": fixture["battleId"],
        "action": {
            "type": fixture["case"]["actionType"],
            "spell": fixture["case"]["actionSpell"],
            "targetCount": len(fixture["case"]["targets"]),
        },
        **fixture["expected"],
    }
    if observed != expected:
        raise ValueError(
            "spell status runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Engine": f"BizHawk {fixture['emulator']['version']} / {fixture['emulator']['core']}",
        "Battle": fixture["battleId"],
        "Spell": "SLEEP 1",
        "Thresholds": [record["threshold"] for record in modeled["construction"]["records"]],
        "Rolls": [record["roll"] for record in modeled["construction"]["records"]],
        "Success": [record["success"] for record in modeled["construction"]["records"]],
        "AccumulatedExp": modeled["construction"]["award"]["accumulatedExp"],
        "CommandExp": modeled["construction"]["award"]["commandExp"],
        "PersistentStatus": modeled["replay"]["finalTargetStatus"],
        "Status": "PASS",
    }
