from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-slow-v1.json")
SCHEMA = repo_path("schemas/h3-spell-slow-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_slow_observer.lua")


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
    definitions = (disasm / "data/stats/spells/spelldefs.asm").read_text(encoding="utf-8")
    slow = re.search(
        r"entry\s+SLOW\s*;\s*SLOW 1(?P<body>.*?)(?=\n\s*entry\s+)",
        definitions,
        re.DOTALL,
    )
    if not slow or f"mpCost     {case['spellMpCost']}" not in slow.group("body"):
        raise ValueError("SLOW 1 definition disagrees with the fixture")

    enums = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    expected_equates = {
        "STATUSEFFECT_SLOW": case["statusMask"],
        "STATUSEFFECTCOUNTER_SLOW": case["statusCounterUnit"],
        "CHANCE_TO_INFLICT_SLOW": case["nonzeroResistanceOffset"],
    }
    for name, expected in expected_equates.items():
        if _equate(enums, name) != expected:
            raise ValueError(f"{name} disagrees with the fixture")

    cast = (disasm / "code/gameflow/battle/battleactions/castspell.asm").read_text(
        encoding="utf-8"
    )
    stats = (disasm / "code/common/stats/updatecombatantstats.asm").read_text(
        encoding="utf-8"
    )
    after_turn = (
        disasm / "code/gameflow/battle/battleloop/processafterturneffects.asm"
    ).read_text(encoding="utf-8")
    cast_fragments = (
        "spellEffect_Slow:",
        "tst.w   d2",
        "addq.w  #CHANCE_TO_INFLICT_SLOW,d2",
        "bsr.w   battlesceneScript_DetermineSpellEffectiveness",
        "ori.w   #STATUSEFFECT_SLOW,d1",
        "jsr     SetStatusEffects",
        "executeEnemyReaction #0,#0,d1,#1",
        "bsr.w   battlesceneScript_AddStatusEffectSpellExp",
        "mulu.w  #3,d1",
        "lsr.l   #3,d1",
    )
    stats_fragments = (
        "andi.w  #STATUSEFFECT_SLOW,d2",
        "rol.w   #6,d2",
        "bsr.w   DecreaseCurrentDef",
        "bsr.w   DecreaseCurrentAgi",
    )
    if any(fragment not in cast for fragment in cast_fragments):
        raise ValueError("SLOW source contract drifted")
    if any(fragment not in stats for fragment in stats_fragments):
        raise ValueError("SLOW stat-refresh source contract drifted")
    if "subi.w  #STATUSEFFECTCOUNTER_SLOW,d1" not in after_turn:
        raise ValueError("SLOW duration decrement source contract drifted")


def _model_expected(fixture: dict[str, Any]) -> dict[str, Any]:
    case = fixture["case"]
    accumulated_exp = 0
    seed = case["seed"]
    records: list[dict[str, Any]] = []
    for target in case["targets"]:
        threshold = (
            0
            if target["setting"] == 0
            else target["setting"] + case["nonzeroResistanceOffset"]
        )
        seed, roll = _rng_step(case["seed"], 8)
        success = roll >= threshold
        status = case["statusMask"] if success else 0
        agi_penalty = (target["baseAgi"] * 3) // 8 if success else 0
        def_penalty = (target["baseDef"] * 3) // 8 if success else 0
        if success:
            accumulated_exp = min(accumulated_exp + 5, 49)
        records.append(
            {
                "combatant": target["combatant"],
                "setting": target["setting"],
                "threshold": threshold,
                "roll": roll,
                "success": success,
                "setStatus": status,
                "reactionStatus": status,
                "accumulatedExp": accumulated_exp,
                "agiPenalty": agi_penalty,
                "defPenalty": def_penalty,
                "statusAfterConstruction": status,
                "currentDefAfterConstruction": target["baseDef"],
                "currentAgiAfterConstruction": target["baseAgi"],
            }
        )

    award_seed = seed
    halved = (
        accumulated_exp // 2
        if fixture["battleId"] == 1 and not case["targetSameSide"]
        else accumulated_exp
    )
    seed, first_roll = _rng_step(seed, 16)
    command_exp = halved + int(first_roll == 0)
    _, second_roll = _rng_step(seed, 16)
    command_exp = max(command_exp - int(second_roll == 0), 1)

    successful = [record for record in records if record["success"]]
    enemy_reactions = []
    final_targets = []
    for target, record in zip(case["targets"], records, strict=True):
        if record["success"]:
            enemy_reactions.append(
                {
                    "combatant": target["combatant"],
                    "statusCommand": case["statusMask"],
                    "statusBefore": case["statusMask"],
                    "statusAfter": case["statusMask"],
                    "defBefore": target["baseDef"],
                    "defAfter": target["baseDef"] - record["defPenalty"],
                    "agiBefore": target["baseAgi"],
                    "agiAfter": target["baseAgi"] - record["agiPenalty"],
                }
            )
        final_targets.append(
            {
                "combatant": target["combatant"],
                "status": case["statusMask"] if record["success"] else 0,
                "currentDef": target["baseDef"] - record["defPenalty"],
                "currentAgi": target["baseAgi"] - record["agiPenalty"],
            }
        )
    return {
        "construction": {
            "actorMp": case["actorInitialMp"],
            "records": records,
            "targetSameSide": case["targetSameSide"],
            "award": {
                "seed": award_seed,
                "halved": halved,
                "firstRoll": first_roll,
                "secondRoll": second_roll,
                "commandExp": command_exp,
            },
        },
        "replay": {
            "reactionOrder": [f"ally:{-case['spellMpCost']}:0"]
            + [
                f"enemy:{record['combatant']}:{case['statusMask']}"
                for record in successful
            ],
            "allyReaction": {
                "combatant": case["actor"],
                "mpChange": -case["spellMpCost"],
                "statusCommand": 0,
                "mpBefore": case["actorInitialMp"],
                "mpAfter": case["actorInitialMp"] - case["spellMpCost"],
            },
            "enemyReactions": enemy_reactions,
            "expReaction": {
                "commandExp": command_exp,
                "expBefore": case["actorInitialExp"],
                "expAfter": case["actorInitialExp"] + command_exp,
            },
            "finalActorMp": case["actorInitialMp"] - case["spellMpCost"],
            "finalActorExp": case["actorInitialExp"] + command_exp,
            "finalTargets": final_targets,
        },
    }


def verify_spell_slow(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 90
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="SLOW fixture")
    verify_runtime_contract(fixture, rom_path)
    harness = load_json(repo_path(fixture["sharedHarnessFixture"]))
    status = load_json(repo_path(fixture["sharedStatusFixture"]))
    healing = load_json(repo_path(fixture["sharedHealingFixture"]))
    _verify_source_contract(_verify_upstream(upstream_path), fixture["case"])
    modeled = _model_expected(fixture)
    if modeled != fixture["expected"]:
        raise ValueError("SLOW golden disagrees with source model")
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {
                **harness["function"],
                **status["function"],
                **healing["function"],
                **fixture["function"],
            },
            "ram": harness["ram"],
            "harness": harness["harness"],
            "case": fixture["case"],
        },
        output_name="spell-slow",
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
            "target": fixture["case"]["targets"][0]["combatant"],
        },
        **fixture["expected"],
    }
    if observed != expected:
        raise ValueError(
            "SLOW runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Spell": "SLOW 1",
        "Thresholds": ",".join(
            str(record["threshold"]) for record in modeled["construction"]["records"]
        ),
        "Results": ",".join(
            "success" if record["success"] else "failure"
            for record in modeled["construction"]["records"]
        ),
        "Penalties": "DEF-15,AGI-8",
        "CommandExp": modeled["construction"]["award"]["commandExp"],
        "Status": "PASS",
    }
