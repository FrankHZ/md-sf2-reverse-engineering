from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import (
    _model_level_case,
    _parse_ally_codes,
    _parse_ally_starts,
    _parse_equates,
    _parse_growth_curves,
    _rng_step,
    _verify_upstream,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/battle-exp-level-up-v1.json")
SCHEMA = repo_path("schemas/h3-battle-exp-level-up-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/battle_exp_level_up_observer.lua")


def _snapshot(
    values: dict[str, int],
    *,
    current_hp: int,
    current_mp: int,
    current_attack: int,
    current_defense: int,
    current_agility: int,
    exp: int,
) -> dict[str, int]:
    return {
        "class": values["class"],
        "level": values["level"],
        "maxHp": values["hp"],
        "currentHp": current_hp,
        "maxMp": values["mp"],
        "currentMp": current_mp,
        "baseAttack": values["attack"],
        "currentAttack": current_attack,
        "baseDefense": values["defense"],
        "currentDefense": current_defense,
        "baseAgility": values["agility"],
        "currentAgility": current_agility,
        "exp": exp,
    }


def _model_fixture(fixture: dict[str, Any], disasm: Path) -> dict[str, Any]:
    case = fixture["case"]
    award_model = case["awardModel"]
    equates = _parse_equates(disasm)
    halved_table = (
        disasm / "data/battles/global/halvedexpearnedbattles.asm"
    ).read_text(encoding="utf-8")
    halved_battles = {
        equates[f"BATTLE_{name}"]
        for name in re.findall(r"^\s*battle\s+([A-Z0-9_]+)", halved_table, re.MULTILINE)
    }
    if award_model["halvedBattle"] != (fixture["battleId"] in halved_battles):
        raise ValueError("battle EXP halving golden disagrees with pinned battle table")
    seed, first = _rng_step(case["awardSeed"], award_model["randomRange"])
    _, second = _rng_step(seed, award_model["randomRange"])
    if (first, second) != (award_model["firstRoll"], award_model["secondRoll"]):
        raise ValueError("battle EXP RNG golden disagrees with independent model")
    award = award_model["accumulatedExp"]
    if award_model["halvedBattle"]:
        award //= 2
    award += int(first == 0) - int(second == 0)
    award = max(award, 1)

    input_ = case["input"]
    increased = min(input_["exp"] + award, equates["CHAR_STATCAP_EXP"])
    if increased < 100:
        raise ValueError("battle EXP fixture does not reach the level-up threshold")
    remaining = increased - 100

    level_case = {
        "id": case["id"],
        "ally": case["actor"],
        "allyCode": case["allyCode"],
        "classCode": case["classCode"],
        "seed": case["levelUpSeed"],
    }
    level = _model_level_case(
        level_case,
        disasm=disasm,
        curves=_parse_growth_curves(disasm),
        equates=equates,
        starts=_parse_ally_starts(disasm),
        ally_codes=_parse_ally_codes(disasm),
    )
    source_before = level["before"]
    input_source_values = {
        "class": input_["class"],
        "level": input_["level"],
        "hp": input_["maxHp"],
        "mp": input_["maxMp"],
        "attack": input_["baseAttack"],
        "defense": input_["baseDefense"],
        "agility": input_["baseAgility"],
        "exp": 0,
    }
    if input_source_values != source_before or input_["items"] != [127] * 4:
        raise ValueError("battle EXP setup disagrees with Bowie's source-modeled SDMN baseline")

    before = _snapshot(
        source_before,
        current_hp=input_["currentHp"],
        current_mp=input_["currentMp"],
        current_attack=input_["battleAttack"],
        current_defense=input_["currentDefense"],
        current_agility=input_["battleAgility"],
        exp=remaining,
    )
    source_after = level["after"]
    after = _snapshot(
        source_after,
        current_hp=input_["currentHp"],
        current_mp=input_["currentMp"],
        current_attack=source_after["attack"],
        current_defense=source_after["defense"],
        current_agility=source_after["agility"],
        exp=remaining,
    )
    return {
        "award": {
            "commandExp": award,
            "expBefore": input_["exp"],
            "expAfterIncrease": increased,
            "expAfterThreshold": remaining,
        },
        "levelUp": {
            "calls": 1,
            "seed": case["levelUpSeed"],
            "observedSeed": level["expectedSeed"],
            "before": before,
            "after": after,
            "arguments": level["arguments"],
        },
        "final": after,
    }


def _verify_observation(fixture: dict[str, Any], observed: dict[str, Any]) -> None:
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["case"]["id"],
        "battle": fixture["battleId"],
        **fixture["expected"],
    }
    if observed != expected:
        raise ValueError("battle EXP-to-level-up runtime observation mismatch")


def verify_battle_exp_level_up(
    rom_path: Path,
    upstream_path: Path,
    *,
    timeout_seconds: int = 75,
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="battle EXP level-up fixture")
    verify_runtime_contract(fixture, rom_path)
    disasm = _verify_upstream(upstream_path)
    modeled = _model_fixture(fixture, disasm)
    if fixture["expected"] != modeled:
        raise ValueError("battle EXP level-up golden disagrees with source model")

    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {**fixture["harness"]["function"], **fixture["function"]},
            "ram": {**fixture["harness"]["ram"], **fixture["ram"]},
            "case": fixture["case"],
        },
        output_name="battle-exp-level-up",
        timeout_seconds=timeout_seconds,
    )
    _verify_observation(fixture, observed)
    expected = fixture["expected"]
    return {
        "Fixture": fixture["id"],
        "Engine": f"BizHawk {fixture['emulator']['version']} / {fixture['emulator']['core']}",
        "Battle": fixture["battleId"],
        "Award": expected["award"]["commandExp"],
        "Exp": (
            f"{expected['award']['expBefore']}->{expected['award']['expAfterIncrease']}"
            f"->{expected['award']['expAfterThreshold']}"
        ),
        "Level": f"{expected['levelUp']['before']['level']}->{expected['final']['level']}",
        "Arguments": expected["levelUp"]["arguments"],
        "Status": "PASS",
    }
