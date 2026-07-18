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


def _model_expected(fixture: dict[str, Any]) -> dict[str, Any]:
    case = fixture["case"]
    records = []
    for target in case["targets"]:
        adjusted = case["spellPower"]
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

        seed, critical_roll = _rng_step(case["seed"], case["criticalRange"])
        pre_variance = post_resistance + (quarter if critical_roll == 0 else 0)
        variance_range = (pre_variance >> 3) + 1
        seed, first = _rng_step(seed, variance_range)
        _, second = _rng_step(seed, variance_range)
        final_damage = max(pre_variance - first - second, 1)
        records.append(
            {
                "combatant": target["combatant"],
                "setting": setting,
                "adjustedPower": adjusted,
                "quarterPower": quarter,
                "postResistance": post_resistance,
                "criticalRoll": critical_roll,
                "preVariance": pre_variance,
                "varianceRange": variance_range,
                "varianceRolls": [first, second],
                "finalDamage": final_damage,
                "temporaryHp": 100 - final_damage,
                "restoredHp": 100,
            }
        )
    return {
        "resistanceCalls": len(case["targets"]),
        "actorMpAfterConstruction": case["initialMp"],
        "records": records,
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
        "ResistanceSettings": [record["setting"] for record in fixture["expected"]["records"]],
        "PreVarianceDamage": [
            record["preVariance"] for record in fixture["expected"]["records"]
        ],
        "FinalDamage": [record["finalDamage"] for record in fixture["expected"]["records"]],
        "Status": "PASS",
    }
