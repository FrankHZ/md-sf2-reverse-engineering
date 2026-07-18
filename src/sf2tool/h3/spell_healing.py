from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _parse_equates, _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-healing-v1.json")
SCHEMA = repo_path("schemas/h3-spell-healing-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_healing_observer.lua")
BOUNDARY_FIXTURE = repo_path("tests/fixtures/h3/spell-healing-exp-boundaries-v1.json")
BOUNDARY_SCHEMA = repo_path(
    "schemas/h3-spell-healing-exp-boundaries-fixture.schema.json"
)
BOUNDARY_OBSERVER = repo_path(
    "tools/bizhawk/spell_healing_exp_boundaries_observer.lua"
)
AURA_FIXTURE = repo_path("tests/fixtures/h3/spell-aura-targets-v1.json")
AURA_SCHEMA = repo_path("schemas/h3-spell-aura-targets-fixture.schema.json")
AURA_OBSERVER = repo_path("tools/bizhawk/spell_aura_targets_observer.lua")


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


def _verify_boundary_source_contract(fixture: dict[str, Any], disasm: Path) -> None:
    source = (disasm / "code/gameflow/battle/battleactions/earnexp.asm").read_text(
        encoding="utf-8"
    )
    required = (
        "btst    #COMBATANT_BIT_ENEMY,d0",
        "cmpi.b  #CLASS_PRST,d1",
        "cmpi.b  #CLASS_VICR,d1",
        "cmpi.b  #CLASS_MMNK,d1",
        "tst.w   d1",
        "move.w  #HEALING_SPELL_EXP_MAX,d5",
        "mulu.w  d6,d5",
        "divu.w  d1,d5",
        "moveq   #HEALING_SPELL_EXP_MIN,d5",
        "move.w  #HEALING_ACTION_EXP_CAP,((BATTLESCENE_EXP-$1000000)).w",
    )
    if any(fragment not in source for fragment in required):
        raise ValueError("healing EXP boundary source contract drift")

    cast = (
        disasm / "code/gameflow/battle/battleactions/castspell.asm"
    ).read_text(encoding="utf-8")
    full_recovery = (
        "cmpi.b  #255,d6",
        "move.w  d2,d6",
        "bra.s   @CapRecovery    ; if spell power = 255, full recovery",
    )
    if any(fragment not in cast for fragment in full_recovery):
        raise ValueError("healing full-recovery source contract drift")

    spell_defs = (disasm / "data/stats/spells/spelldefs.asm").read_text(encoding="utf-8")
    expected_powers = {0: 15, 128: 30, 192: 255}
    labels = {
        0: r"HEAL\s*;\s*HEAL 1",
        128: r"HEAL\|LV3\s*;\s*HEAL 3",
        192: r"HEAL\|LV4\s*;\s*HEAL 4",
    }
    for spell, expected_power in expected_powers.items():
        block = re.search(
            rf"entry\s+{labels[spell]}(?P<body>.*?)(?=\n\s*entry\s+)",
            spell_defs,
            re.DOTALL,
        )
        power = (
            re.search(r"^\s*power\s+(\d+)\s*$", block.group("body"), re.MULTILINE)
            if block
            else None
        )
        if not power or int(power.group(1)) != expected_power:
            raise ValueError(f"HEAL spell power drift for action entry {spell}")
    if any(expected_powers[case["actionSpell"]] != case["basePower"] for case in fixture["cases"]):
        raise ValueError("healing EXP fixture base power drift")


def _verify_boundary_models(fixture: dict[str, Any], disasm: Path) -> None:
    equates = _parse_equates(disasm)
    healer_classes = {
        equates["CLASS_PRST"],
        equates["CLASS_VICR"],
        equates["CLASS_MMNK"],
    }
    for case in fixture["cases"]:
        if case["class"] != equates[f"CLASS_{case['classCode']}"]:
            raise ValueError(f"healing EXP class identity drift: {case['id']}")
        missing = case["targetMaxHp"] - case["targetCurrentHp"]
        if case["basePower"] == 255:
            pre_cap = missing
        else:
            pre_cap = case["basePower"]
            if case["class"] >= equates["CHAR_CLASS_FIRSTPROMOTED"]:
                pre_cap = (pre_cap * 5) >> 2
        recovery = min(missing, pre_cap)
        raw = (
            (25 * recovery) // case["targetMaxHp"] if case["targetMaxHp"] else 0
        )
        eligible = (
            case["expActor"] < 128
            and case["class"] in healer_classes
            and case["targetMaxHp"] > 0
        )
        computed = max(raw, 10) if eligible else 0
        final = min(case["initialAccumulator"] + computed, 25)
        modeled = {
            "missingHp": missing,
            "preCapPower": pre_cap,
            "recovery": recovery,
            "eligible": eligible,
            "rawHealingExp": raw,
            "computedHealingExp": computed,
            "finalAccumulator": final,
            "capApplied": eligible
            and case["initialAccumulator"] + computed > 25,
        }
        if any(case[field] != value for field, value in modeled.items()):
            raise ValueError(f"healing EXP golden disagrees with model: {case['id']}")


def _boundary_observed_case(case: dict[str, Any]) -> dict[str, Any]:
    fields = (
        "id",
        "class",
        "expActor",
        "actionSpell",
        "targetMaxHp",
        "targetCurrentHp",
        "preCapPower",
        "recovery",
        "initialAccumulator",
        "finalAccumulator",
    )
    return {field: case[field] for field in fields}


def verify_spell_healing_exp(
    rom_path: Path,
    upstream_path: Path,
    *,
    timeout_seconds: int = 90,
) -> dict[str, Any]:
    fixture = load_json(BOUNDARY_FIXTURE)
    validate_json(fixture, BOUNDARY_SCHEMA, owner="healing EXP boundary fixture")
    verify_runtime_contract(fixture, rom_path)
    healing_shared = load_json(repo_path(fixture["sharedHarnessFixture"]))
    shared = load_json(repo_path(healing_shared["sharedHarnessFixture"]))
    disasm = _verify_upstream(upstream_path)
    _verify_boundary_source_contract(fixture, disasm)
    _verify_boundary_models(fixture, disasm)
    observed = run_observer(
        rom_path=rom_path,
        observer_path=BOUNDARY_OBSERVER,
        config={
            "function": {
                **shared["function"],
                **healing_shared["function"],
                **fixture["function"],
            },
            "ram": {**shared["ram"], **fixture["ram"]},
            "harness": shared["harness"],
            "battleId": fixture["battleId"],
            "setup": fixture["caseSetup"],
            "cases": fixture["cases"],
        },
        output_name="spell-healing-exp-boundaries",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "battle": fixture["battleId"],
        "cases": [_boundary_observed_case(case) for case in fixture["cases"]],
    }
    if (
        observed.get("system") != "GEN"
        or observed.get("core") != fixture["emulator"]["core"]
        or observed.get("result") != expected
    ):
        raise ValueError(
            "healing EXP boundary runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "Eligible": sum(case["eligible"] for case in fixture["cases"]),
        "Skipped": sum(not case["eligible"] for case in fixture["cases"]),
        "FullRecovery": sum(case["basePower"] == 255 for case in fixture["cases"]),
        "Caps": sum(case["capApplied"] for case in fixture["cases"]),
        "Awards": [case["finalAccumulator"] for case in fixture["cases"]],
        "Status": "PASS",
    }


def _verify_aura_source_contract(fixture: dict[str, Any], disasm: Path) -> None:
    target_source = (
        disasm / "code/gameflow/battle/battlefield/populatetargetslist.asm"
    ).read_text(encoding="utf-8")
    target_fragments = (
        "bsr.w   BuildTargetsArrayWithTeammatesOfTarget",
        "cmpi.b  #SPELL_AURA|SPELL_LV4,d1",
        "move.b  SPELLDEF_OFFSET_RADIUS(a0),d2",
        "addq.b  #1,d2",
        "bsr.w   ApplyRelativeCoordinatesListToGrid",
        "bsr.w   PopulateTargetsListWithAllAllies",
        "jsr     GetCombatantX",
        "jsr     GetCurrentHp",
    )
    if any(fragment not in target_source for fragment in target_fragments):
        raise ValueError("AURA target-population source contract drift")

    sort_source = (
        disasm / "code/gameflow/battle/battleactions/sorttargets.asm"
    ).read_text(encoding="utf-8")
    if "Sort targets by combatant index" not in sort_source:
        raise ValueError("AURA target-sort source contract drift")

    spell_defs = (disasm / "data/stats/spells/spelldefs.asm").read_text(
        encoding="utf-8"
    )
    source_levels = {1: "AURA", 2: r"AURA\|LV2", 4: r"AURA\|LV4"}
    for case in fixture["cases"]:
        block = re.search(
            rf"entry\s+{source_levels[case['level']]}\s*;\s*AURA {case['level']}"
            r"(?P<body>.*?)(?=\n\s*entry\s+)",
            spell_defs,
            re.DOTALL,
        )
        if not block:
            raise ValueError(f"pinned spell definitions omit {case['id']}")
        body = block.group("body")
        radius = re.search(r"^\s*radius\s+(\d+)\s*$", body, re.MULTILINE)
        power = re.search(r"^\s*power\s+(\d+)\s*$", body, re.MULTILINE)
        if (
            not radius
            or not power
            or int(radius.group(1)) != case["radius"]
            or int(power.group(1)) != case["power"]
        ):
            raise ValueError(f"AURA definition drift: {case['id']}")


def _aura_model_case(
    fixture: dict[str, Any], case: dict[str, Any]
) -> dict[str, Any]:
    allies = {ally["combatant"]: ally for ally in fixture["setup"]["allies"]}
    if case["allAllies"]:
        targets = sorted(
            combatant
            for combatant, ally in allies.items()
            if ally["currentHp"] > 0 and ally["x"] != 0xFF
        )
    else:
        center = allies[fixture["setup"]["target"]]
        targets = sorted(
            combatant
            for combatant, ally in allies.items()
            if ally["currentHp"] > 0
            and ally["x"] < 48
            and ally["y"] < 48
            and abs(ally["x"] - center["x"]) + abs(ally["y"] - center["y"])
            <= case["radius"]
        )

    accumulator = 0
    effects: list[dict[str, Any]] = []
    for target in targets:
        ally = allies[target]
        missing = ally["maxHp"] - ally["currentHp"]
        pre_cap = missing if case["power"] == 255 else case["power"]
        recovery = min(missing, pre_cap)
        contribution = max((25 * recovery) // ally["maxHp"], 10)
        initial = accumulator
        accumulator = min(accumulator + contribution, 25)
        effects.append(
            {
                "target": target,
                "maxHp": ally["maxHp"],
                "currentHp": ally["currentHp"],
                "missingHp": missing,
                "preCapPower": pre_cap,
                "recovery": recovery,
                "initialAccumulator": initial,
                "finalAccumulator": accumulator,
            }
        )
    return {"targets": targets, "effects": effects}


def _verify_aura_models(fixture: dict[str, Any]) -> None:
    for case in fixture["cases"]:
        modeled = _aura_model_case(fixture, case)
        if case["targets"] != modeled["targets"] or case["effects"] != modeled["effects"]:
            raise ValueError(f"AURA golden disagrees with source model: {case['id']}")


def _aura_observed_case(case: dict[str, Any]) -> dict[str, Any]:
    effect_fields = (
        "target",
        "maxHp",
        "currentHp",
        "preCapPower",
        "recovery",
        "initialAccumulator",
        "finalAccumulator",
    )
    return {
        "id": case["id"],
        "actionSpell": case["actionSpell"],
        "targets": case["targets"],
        "effects": [
            {field: effect[field] for field in effect_fields}
            for effect in case["effects"]
        ],
    }


def verify_spell_aura(
    rom_path: Path,
    upstream_path: Path,
    *,
    timeout_seconds: int = 90,
) -> dict[str, Any]:
    fixture = load_json(AURA_FIXTURE)
    validate_json(fixture, AURA_SCHEMA, owner="AURA target geometry fixture")
    verify_runtime_contract(fixture, rom_path)
    healing_shared = load_json(repo_path(fixture["sharedHarnessFixture"]))
    shared = load_json(repo_path(healing_shared["sharedHarnessFixture"]))
    disasm = _verify_upstream(upstream_path)
    _verify_aura_source_contract(fixture, disasm)
    _verify_aura_models(fixture)
    observed = run_observer(
        rom_path=rom_path,
        observer_path=AURA_OBSERVER,
        config={
            "function": {
                **shared["function"],
                **healing_shared["function"],
                **fixture["function"],
            },
            "ram": {**shared["ram"], **fixture["ram"]},
            "harness": shared["harness"],
            "battleId": fixture["battleId"],
            "setup": fixture["setup"],
            "cases": fixture["cases"],
        },
        output_name="spell-aura-targets",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "battle": fixture["battleId"],
        "cases": [_aura_observed_case(case) for case in fixture["cases"]],
    }
    if (
        observed.get("system") != "GEN"
        or observed.get("core") != fixture["emulator"]["core"]
        or observed.get("result") != expected
    ):
        raise ValueError(
            "AURA target geometry runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "Targets": [len(case["targets"]) for case in fixture["cases"]],
        "FinalExp": [case["effects"][-1]["finalAccumulator"] for case in fixture["cases"]],
        "AllAlliesTargets": fixture["cases"][-1]["targets"],
        "Status": "PASS",
    }
