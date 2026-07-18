from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _parse_equates, _rng_step, _verify_upstream
from sf2tool.h3.kill_exp import _kill_exp
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-damage-exp-v1.json")
SCHEMA = repo_path("schemas/h3-spell-damage-exp-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_damage_exp_observer.lua")


def _verify_source_contract(fixture: dict[str, Any], disasm: Path) -> None:
    source = (disasm / "code/gameflow/battle/battleactions/earnexp.asm").read_text(
        encoding="utf-8"
    )
    required_fragments = (
        "battlesceneScript_CalculateDamageExp:",
        "bsr.w   battlesceneScript_GetKillExp",
        "mulu.w  d6,d5",
        "divu.w  d1,d5",
        "battlesceneScript_AddExpAndGoldForKill:",
        "battlesceneScript_AddExpAndApplyPerActionCap:",
        "cmpi.w  #PER_ACTION_EXP_CAP,((BATTLESCENE_EXP-$1000000)).w",
        "move.w  #PER_ACTION_EXP_CAP,((BATTLESCENE_EXP-$1000000)).w",
    )
    if any(fragment not in source for fragment in required_fragments):
        raise ValueError("spell damage EXP source contract drift")

    inflict = (
        disasm / "code/gameflow/battle/battleactions/inflictdamage.asm"
    ).read_text(encoding="utf-8")
    required_inflict = (
        "jsr     battlesceneScript_CalculateDamageExp",
        "jsr     DecreaseCurrentHp",
        "bsr.w   battlesceneScript_AddExpAndGoldForKill",
    )
    if any(fragment not in inflict for fragment in required_inflict):
        raise ValueError("damage-to-kill EXP call order drift")

    setup = fixture["caseSetup"]
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
    if not power or int(power.group(1)) != setup["spellPower"]:
        raise ValueError("BLAZE 2 power disagrees with spell EXP fixture")
    if not cost or int(cost.group(1)) != setup["spellMpCost"]:
        raise ValueError("BLAZE 2 MP cost disagrees with spell EXP fixture")

    halved_table = (
        disasm / "data/battles/global/halvedexpearnedbattles.asm"
    ).read_text(encoding="utf-8")
    if "battle INSIDE_ANCIENT_TOWER" not in halved_table:
        raise ValueError("Battle 01 EXP-halving table drift")


def _verify_models(fixture: dict[str, Any], disasm: Path) -> None:
    equates = _parse_equates(disasm)
    first_promoted = equates["CHAR_CLASS_FIRSTPROMOTED"]
    extra_level = equates["CHAR_CLASS_EXTRALEVEL"]
    if fixture["battleId"] != equates["BATTLE_INSIDE_ANCIENT_TOWER"]:
        raise ValueError("spell damage EXP Battle 01 identity drift")
    if any(
        case["awardBattle"] == equates["BATTLE_INSIDE_ANCIENT_TOWER"]
        for case in fixture["cases"]
        if case["id"] == "nonbattle-table-miss"
    ):
        raise ValueError("non-battle award case unexpectedly selects the halved battle")

    for case in fixture["cases"]:
        if case["class"] != equates[f"CLASS_{case['classCode']}"]:
            raise ValueError(f"spell EXP class identity drift: {case['id']}")
        effective = case["actorLevel"]
        if case["class"] >= first_promoted:
            effective += extra_level
        difference = effective - case["targetLevel"]
        bracket = _kill_exp(difference)
        scaled = (bracket * case["finalDamage"]) // case["targetMaxHp"]
        after_damage = min(case["initialAccumulatedExp"] + scaled, 49)
        lethal = case["finalDamage"] >= case["targetCurrentHp"]
        after_kill = min(after_damage + bracket, 49) if lethal else after_damage
        halved = after_kill // 2 if case["awardBattle"] == fixture["battleId"] else after_kill
        next_seed, first = _rng_step(fixture["caseSetup"]["awardSeed"], 16)
        _, second = _rng_step(next_seed, 16)
        command = max(halved + int(first == 0) - int(second == 0), 1)
        modeled = {
            "effectiveActorLevel": effective,
            "levelDifference": difference,
            "levelBracketExp": bracket,
            "afterDamageExp": after_damage,
            "killApplied": lethal,
            "afterKillExp": after_kill,
            "halvedExp": halved,
            "firstRoll": first,
            "secondRoll": second,
            "commandExp": command,
        }
        if any(case[field] != value for field, value in modeled.items()):
            raise ValueError(f"spell damage EXP golden disagrees with model: {case['id']}")


def _expected_case(case: dict[str, Any]) -> dict[str, Any]:
    fields = (
        "id",
        "class",
        "actorLevel",
        "targetLevel",
        "targetMaxHp",
        "targetCurrentHp",
        "finalDamage",
        "levelBracketExp",
        "initialAccumulatedExp",
        "afterDamageExp",
        "killApplied",
        "afterKillExp",
        "awardBattle",
        "halvedExp",
        "firstRoll",
        "secondRoll",
        "commandExp",
    )
    return {field: case[field] for field in fields}


def _verify_observation(fixture: dict[str, Any], observed: dict[str, Any]) -> None:
    setup = fixture["caseSetup"]
    expected = {
        "battle": fixture["battleId"],
        "action": {
            "type": setup["actionType"],
            "spell": setup["actionSpell"],
            "baseSpell": setup["baseSpell"],
        },
        "cases": [_expected_case(case) for case in fixture["cases"]],
    }
    if (
        observed.get("system") != "GEN"
        or observed.get("core") != fixture["emulator"]["core"]
        or observed.get("result") != expected
    ):
        raise ValueError(
            "spell damage EXP runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )


def verify_spell_damage_exp(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 90
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    verify_runtime_contract(fixture, rom_path)
    shared = load_json(repo_path(fixture["sharedHarnessFixture"]))
    disasm = _verify_upstream(upstream_path)
    _verify_source_contract(fixture, disasm)
    _verify_models(fixture, disasm)
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {**shared["function"], **fixture["function"]},
            "ram": {**shared["ram"], **fixture["ram"]},
            "harness": shared["harness"],
            "battleId": fixture["battleId"],
            "setup": fixture["caseSetup"],
            "cases": fixture["cases"],
        },
        output_name="spell-damage-exp",
        timeout_seconds=timeout_seconds,
    )
    _verify_observation(fixture, observed)
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "LevelDifferences": [case["levelDifference"] for case in fixture["cases"][:8]],
        "DamageExp": [case["afterDamageExp"] for case in fixture["cases"][:8]],
        "Caps": sum(case["afterKillExp"] == 49 for case in fixture["cases"]),
        "KillBonuses": sum(case["killApplied"] for case in fixture["cases"]),
        "NonHalvedAwards": sum(
            case["awardBattle"] != fixture["battleId"] for case in fixture["cases"]
        ),
        "Status": "PASS",
    }
