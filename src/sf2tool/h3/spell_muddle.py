from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-muddle-v1.json")
SCHEMA = repo_path("schemas/h3-spell-muddle-fixture.schema.json")


def _equate(source: str, name: str) -> int:
    match = re.search(
        rf"^{re.escape(name)}:\s+equ\s+(\$?)([0-9A-F]+)(?:\s|$)",
        source,
        re.MULTILINE,
    )
    if not match:
        raise ValueError(f"missing pinned equate: {name}")
    return int(match.group(2), 16 if match.group(1) else 10)


def _verify_source_contract(disasm: Path, case: dict[str, Any]) -> None:
    definitions = (disasm / "data/stats/spells/spelldefs.asm").read_text(
        encoding="utf-8"
    )
    muddle2 = re.search(
        r"entry\s+MUDDLE\|LV2\s*;\s*MUDDLE 2(?P<body>.*?)(?=\n\s*entry\s+)",
        definitions,
        re.DOTALL,
    )
    if not muddle2 or f"mpCost     {case['spellMpCost']}" not in muddle2.group("body"):
        raise ValueError("MUDDLE 2 definition disagrees with the fixture")

    enums = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    expected_equates = {
        "SPELL_MUDDLE": case["baseSpell"],
        "SPELL_LV2": case["actionSpell"] - case["baseSpell"],
        "CHANCE_TO_INFLICT_MUDDLE2": case["baseThreshold"],
        "STATUSEFFECT_MUDDLE": case["statusMask"] & ~8,
        "STATUSEFFECT_MUDDLE2": 8,
        "STATUSEFFECTCOUNTER_MUDDLE": case["statusCounterUnit"],
    }
    for name, expected in expected_equates.items():
        if _equate(enums, name) != expected:
            raise ValueError(f"{name} disagrees with the fixture")

    cast = (disasm / "code/gameflow/battle/battleactions/castspell.asm").read_text(
        encoding="utf-8"
    )
    required = (
        "spellEffect_Muddle:",
        "addq.w  #CHANCE_TO_INFLICT_MUDDLE2,d2",
        "bsr.w   battlesceneScript_DetermineSpellEffectiveness",
        "ori.w   #STATUSEFFECT_MUDDLE2,d1",
        "ori.w   #STATUSEFFECT_MUDDLE,d1",
        "executeEnemyReaction #0,#0,d1,#1",
        "bsr.w   battlesceneScript_AddStatusEffectSpellExp",
    )
    if any(fragment not in cast for fragment in required):
        raise ValueError("MUDDLE 2 source contract drifted")


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
    command_exp = max(randomized - int(second_roll == 0), 1)
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


def verify_spell_muddle(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 75
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="MUDDLE fixture")
    verify_runtime_contract(fixture, rom_path)
    shared = load_json(repo_path(fixture["sharedHarnessFixture"]))
    _verify_source_contract(_verify_upstream(upstream_path), fixture["case"])
    modeled = _model_expected(fixture)
    if fixture["expected"] != modeled:
        raise ValueError("MUDDLE golden disagrees with source model")

    function = {**shared["function"], **fixture["function"]}
    function["sleepEffectEntryAddress"] = fixture["function"][
        "muddleEffectEntryAddress"
    ]
    observed = run_observer(
        rom_path=rom_path,
        observer_path=repo_path(fixture["sharedObserver"]),
        config={
            "function": function,
            "ram": shared["ram"],
            "harness": shared["harness"],
            "case": fixture["case"],
        },
        output_name="spell-muddle",
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
            "MUDDLE runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Spell": "MUDDLE 2",
        "Thresholds": ",".join(
            str(record["threshold"]) for record in modeled["construction"]["records"]
        ),
        "Results": ",".join(
            "success" if record["success"] else "failure"
            for record in modeled["construction"]["records"]
        ),
        "StatusMask": f"0x{fixture['case']['statusMask']:04X}",
        "CommandExp": modeled["construction"]["award"]["commandExp"],
        "Status": "PASS",
    }
