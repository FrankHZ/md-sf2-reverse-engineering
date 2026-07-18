from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-damage-resistance-v1.json")
SCHEMA = repo_path("schemas/h3-spell-damage-resistance-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_damage_resistance_observer.lua")
SUMMON_FIXTURE = repo_path("tests/fixtures/h3/spell-summon-division-v1.json")
SUMMON_SCHEMA = repo_path("schemas/h3-spell-summon-division-fixture.schema.json")


def _verify_source_contract(disasm: Path, case: dict[str, Any]) -> None:
    spell_defs = (disasm / "data/stats/spells/spelldefs.asm").read_text(encoding="utf-8")
    blaze_2 = re.search(
        r"entry\s+BLAZE\|LV2\b(?P<body>.*?)(?=\n\s*entry\s+)",
        spell_defs,
        re.DOTALL,
    )
    if not blaze_2:
        raise ValueError("pinned spell definitions do not contain BLAZE 2")
    power = re.search(r"^\s*power\s+(\d+)\s*$", blaze_2.group("body"), re.MULTILINE)
    cost = re.search(r"^\s*mpCost\s+(\d+)\s*$", blaze_2.group("body"), re.MULTILINE)
    if not power or int(power.group(1)) != case["spellPower"]:
        raise ValueError("BLAZE 2 power disagrees with the fixture")
    if not cost or int(cost.group(1)) != case["spellMpCost"]:
        raise ValueError("BLAZE 2 MP cost disagrees with the fixture")

    elements = (disasm / "data/stats/spells/spellelements.asm").read_text(encoding="utf-8")
    blaze_element = re.search(r"spellElement\s+(\w+)\s*;\s*11:\s*BLAZE", elements)
    if not blaze_element or blaze_element.group(1) != "FIRE":
        raise ValueError("BLAZE element disagrees with the fixture")

    calculation = (
        disasm / "code/gameflow/battle/battleactions/calculatespelldamage.asm"
    ).read_text(encoding="utf-8")
    required_fragments = (
        "sub.w   d1,d6           ; -25% damage if target has minor resistance",
        "lsr.w   #1,d6           ; -50% damage if target has major resistance",
        "add.w   d1,d6           ; +25% damage if target is weak",
        "add.w   d1,d6           ; +25% damage if successful critical hit",
    )
    if any(fragment not in calculation for fragment in required_fragments):
        raise ValueError("spell-damage arithmetic source contract drifted")


def _verify_summon_source_contract(disasm: Path, case: dict[str, Any]) -> None:
    spell_defs = (disasm / "data/stats/spells/spelldefs.asm").read_text(encoding="utf-8")
    dao_1 = re.search(
        r"entry\s+DAO\s*;\s*DAO 1(?P<body>.*?)(?=\n\s*entry\s+)",
        spell_defs,
        re.DOTALL,
    )
    if not dao_1:
        raise ValueError("pinned spell definitions do not contain DAO 1")
    power = re.search(r"^\s*power\s+(\d+)\s*$", dao_1.group("body"), re.MULTILINE)
    cost = re.search(r"^\s*mpCost\s+(\d+)\s*$", dao_1.group("body"), re.MULTILINE)
    if not power or int(power.group(1)) != case["spellPower"]:
        raise ValueError("DAO 1 power disagrees with the fixture")
    if not cost or int(cost.group(1)) != case["spellMpCost"]:
        raise ValueError("DAO 1 MP cost disagrees with the fixture")
    calculation = (
        disasm / "code/gameflow/battle/battleactions/calculatespelldamage.asm"
    ).read_text(encoding="utf-8")
    required_fragments = (
        "cmpi.w  #SPELL_DAO,d1",
        "mulu.w  #5,d6",
        "divu.w  d0,d6",
    )
    if any(fragment not in calculation for fragment in required_fragments):
        raise ValueError("summon spell-power source contract drifted")


def _model_expected(fixture: dict[str, Any]) -> dict[str, Any]:
    case = fixture["case"]
    records = []
    for target in case["targets"]:
        pre_division = case["spellPower"]
        if target["casterClass"] >= 12:
            pre_division = (pre_division * 5) >> 2
        adjusted = (
            pre_division // len(case["targets"])
            if case["divideByTargetCount"]
            else pre_division
        )
        quarter = adjusted >> 2
        setting = target["setting"]
        if setting == 1:
            post_resistance = adjusted - quarter
        elif setting == 2:
            post_resistance = adjusted >> 1
        elif setting == 3:
            post_resistance = adjusted + quarter
        else:
            post_resistance = adjusted

        seed, critical_roll = _rng_step(target["seed"], case["criticalRange"])
        pre_variance = post_resistance + (quarter if critical_roll == 0 else 0)
        variance_range = (pre_variance >> 3) + 1
        seed, first = _rng_step(seed, variance_range)
        _, second = _rng_step(seed, variance_range)
        final_damage = max(pre_variance - first - second, 1)
        records.append(
            {
                "combatant": target["combatant"],
                "setting": setting,
                "casterClass": target["casterClass"],
                "preDivisionPower": pre_division,
                "adjustedPower": adjusted,
                "quarterPower": quarter,
                "postResistance": post_resistance,
                "criticalRoll": critical_roll,
                "criticalFlag": 255 if critical_roll == 0 else 0,
                "preVariance": pre_variance,
                "varianceRange": variance_range,
                "varianceRolls": [first, second],
                "finalDamage": final_damage,
                "temporaryHp": 100 - final_damage,
                "restoredHp": 100,
            }
        )
    enemy_reactions = [
        {
            "combatant": record["combatant"],
            "hpChange": -record["finalDamage"],
            "hpBefore": record["restoredHp"],
            "hpAfter": record["restoredHp"] - record["finalDamage"],
        }
        for record in records
    ]
    return {
        "construction": {
            "resistanceCalls": len(case["targets"]),
            "divisionCalls": (
                len(case["targets"]) if case["divideByTargetCount"] else 0
            ),
            "actorMp": case["initialMp"],
            "records": records,
        },
        "replay": {
            "allyReactions": [
                {
                    "combatant": case["actor"],
                    "hpChange": 0,
                    "mpChange": -case["spellMpCost"],
                    "mpBefore": case["initialMp"],
                    "mpAfter": case["initialMp"] - case["spellMpCost"],
                }
            ],
            "enemyReactions": enemy_reactions,
            "finalActorMp": case["initialMp"] - case["spellMpCost"],
            "finalTargetHp": [reaction["hpAfter"] for reaction in enemy_reactions],
        },
    }


def _verify_observation(fixture: dict[str, Any], observed: dict[str, Any]) -> None:
    case = fixture["case"]
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": case["id"],
        "battle": fixture["battleId"],
        "action": {
            "type": case["actionType"],
            "spell": case["actionSpell"],
            "baseSpell": case["baseSpell"],
            "targetCount": len(case["targets"]),
        },
        **fixture["expected"],
    }
    if observed != expected:
        raise ValueError(
            "spell damage/resistance runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )


def verify_spell_damage(
    rom_path: Path,
    upstream_path: Path,
    *,
    timeout_seconds: int = 75,
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="spell damage/resistance fixture")
    verify_runtime_contract(fixture, rom_path)
    disasm = _verify_upstream(upstream_path)
    _verify_source_contract(disasm, fixture["case"])
    modeled = _model_expected(fixture)
    if fixture["expected"] != modeled:
        raise ValueError("spell damage/resistance golden disagrees with source model")

    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": fixture["function"],
            "ram": fixture["ram"],
            "harness": fixture["harness"],
            "case": fixture["case"],
        },
        output_name="spell-damage-resistance",
        timeout_seconds=timeout_seconds,
    )
    _verify_observation(fixture, observed)
    return {
        "Fixture": fixture["id"],
        "Engine": f"BizHawk {fixture['emulator']['version']} / {fixture['emulator']['core']}",
        "Battle": fixture["battleId"],
        "Spell": "BLAZE 2",
        "ResistanceSettings": [
            record["setting"] for record in fixture["expected"]["construction"]["records"]
        ],
        "PreVarianceDamage": [
            record["preVariance"]
            for record in fixture["expected"]["construction"]["records"]
        ],
        "FinalDamage": [
            record["finalDamage"]
            for record in fixture["expected"]["construction"]["records"]
        ],
        "PersistentHp": fixture["expected"]["replay"]["finalTargetHp"],
        "PersistentMp": fixture["expected"]["replay"]["finalActorMp"],
        "Status": "PASS",
    }


def verify_spell_summon(
    rom_path: Path,
    upstream_path: Path,
    *,
    timeout_seconds: int = 75,
) -> dict[str, Any]:
    fixture = load_json(SUMMON_FIXTURE)
    validate_json(fixture, SUMMON_SCHEMA, owner="summon spell division fixture")
    verify_runtime_contract(fixture, rom_path)
    shared = load_json(repo_path(fixture["sharedHarnessFixture"]))
    disasm = _verify_upstream(upstream_path)
    _verify_summon_source_contract(disasm, fixture["case"])
    modeled = _model_expected(fixture)
    if fixture["expected"] != modeled:
        raise ValueError("summon spell division golden disagrees with source model")

    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {**shared["function"], **fixture["function"]},
            "ram": shared["ram"],
            "harness": shared["harness"],
            "case": fixture["case"],
        },
        output_name="spell-summon-division",
        timeout_seconds=timeout_seconds,
    )
    _verify_observation(fixture, observed)
    records = fixture["expected"]["construction"]["records"]
    return {
        "Fixture": fixture["id"],
        "Engine": f"BizHawk {fixture['emulator']['version']} / {fixture['emulator']['core']}",
        "Battle": fixture["battleId"],
        "Spell": "DAO 1",
        "Targets": len(records),
        "Power": f"{records[0]['preDivisionPower']}->{records[0]['adjustedPower']}",
        "DivisionCalls": fixture["expected"]["construction"]["divisionCalls"],
        "PersistentHp": fixture["expected"]["replay"]["finalTargetHp"],
        "PersistentMp": fixture["expected"]["replay"]["finalActorMp"],
        "Status": "PASS",
    }
