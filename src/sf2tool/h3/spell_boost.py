from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-boost-v1.json")
SCHEMA = repo_path("schemas/h3-spell-boost-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_boost_observer.lua")


def _equate(source: str, name: str) -> int:
    match = re.search(rf"^{re.escape(name)}:\s+equ\s+\$([0-9A-F]+)\s*$", source, re.MULTILINE)
    if not match:
        raise ValueError(f"missing pinned equate: {name}")
    return int(match.group(1), 16)


def _verify_source_contract(disasm: Path, case: dict[str, Any]) -> None:
    definitions = (disasm / "data/stats/spells/spelldefs.asm").read_text(encoding="utf-8")
    boost = re.search(
        r"entry\s+BOOST\s*;\s*BOOST 1(?P<body>.*?)(?=\n\s*entry\s+)",
        definitions,
        re.DOTALL,
    )
    if not boost or f"mpCost     {case['spellMpCost']}" not in boost.group("body"):
        raise ValueError("BOOST 1 definition disagrees with the fixture")

    enums = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    if _equate(enums, "STATUSEFFECT_BOOST") != case["statusMask"]:
        raise ValueError("BOOST status mask disagrees with the fixture")
    if _equate(enums, "STATUSEFFECTCOUNTER_BOOST") != case["statusCounterUnit"]:
        raise ValueError("BOOST counter unit disagrees with the fixture")

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
        "spellEffect_Boost:",
        "ori.w   #STATUSEFFECT_BOOST,d1",
        "jsr     SetStatusEffects",
        "andi.w  #STATUSEFFECT_BOOST,d3",
        "moveq   #8,d2",
        "bsr.w   battlesceneScript_DetermineSpellEffectiveness",
        "executeAllyReaction #0,#0,d1,#2",
        "bsr.w   battlesceneScript_AddStatusEffectSpellExp",
        "mulu.w  #3,d1",
        "lsr.l   #3,d1",
    )
    stats_fragments = (
        "andi.w  #STATUSEFFECT_BOOST,d2",
        "rol.w   #4,d2",
        "bsr.w   IncreaseCurrentDef",
        "bsr.w   IncreaseCurrentAgi",
    )
    if any(fragment not in cast for fragment in cast_fragments):
        raise ValueError("BOOST source contract drifted")
    if any(fragment not in stats for fragment in stats_fragments):
        raise ValueError("BOOST stat-refresh source contract drifted")
    if "subi.w  #STATUSEFFECTCOUNTER_BOOST,d1" not in after_turn:
        raise ValueError("BOOST duration decrement source contract drifted")


def _model_expected(fixture: dict[str, Any]) -> dict[str, Any]:
    case = fixture["case"]
    accumulated_exp = 0
    seed = case["seed"]
    records: list[dict[str, Any]] = []
    for target in case["targets"]:
        set_status = target["initialStatus"] | case["statusMask"]
        reapplication = bool(target["initialStatus"] & case["statusMask"])
        threshold = case["recastThreshold"] if reapplication else 0
        roll = -1
        success = True
        reaction_status = set_status
        agi_bonus = (target["baseAgi"] * 3) // 8
        def_bonus = (target["baseDef"] * 3) // 8
        if reapplication:
            seed, roll = _rng_step(case["seed"], 8)
            success = roll >= threshold
            if not success:
                reaction_status = 0
                agi_bonus = 0
                def_bonus = 0
        if success:
            accumulated_exp = min(accumulated_exp + 5, 49)
        records.append(
            {
                "combatant": target["combatant"],
                "initialStatus": target["initialStatus"],
                "setStatus": set_status,
                "reapplication": reapplication,
                "threshold": threshold,
                "roll": roll,
                "success": success,
                "reactionStatus": reaction_status,
                "accumulatedExp": accumulated_exp,
                "agiBonus": agi_bonus,
                "defBonus": def_bonus,
                "statusAfterConstruction": set_status,
                "currentDefAfterConstruction": target["initialCurrentDef"],
                "currentAgiAfterConstruction": target["initialCurrentAgi"],
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

    actor_target = case["targets"][0]
    actor_after_mp = case["actorInitialMp"] - case["spellMpCost"]
    actor_def_after = actor_target["baseDef"] + (actor_target["baseDef"] * 3) // 8
    actor_agi_after = actor_target["baseAgi"] + (actor_target["baseAgi"] * 3) // 8
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
            "reactionOrder": [
                f"ally:{-case['spellMpCost']}:0",
                f"ally:0:{case['statusMask']}",
            ],
            "allyReactions": [
                {
                    "combatant": case["actor"],
                    "mpChange": -case["spellMpCost"],
                    "statusCommand": 0,
                    "mpBefore": case["actorInitialMp"],
                    "mpAfter": actor_after_mp,
                    "statusBefore": case["statusMask"],
                    "statusAfter": 0,
                    "defBefore": actor_target["initialCurrentDef"],
                    "defAfter": actor_target["baseDef"],
                    "agiBefore": actor_target["initialCurrentAgi"],
                    "agiAfter": actor_target["baseAgi"],
                },
                {
                    "combatant": case["actor"],
                    "mpChange": 0,
                    "statusCommand": case["statusMask"],
                    "mpBefore": actor_after_mp,
                    "mpAfter": actor_after_mp,
                    "statusBefore": 0,
                    "statusAfter": case["statusMask"],
                    "defBefore": actor_target["baseDef"],
                    "defAfter": actor_def_after,
                    "agiBefore": actor_target["baseAgi"],
                    "agiAfter": actor_agi_after,
                },
            ],
            "expReaction": {
                "commandExp": command_exp,
                "expBefore": case["actorInitialExp"],
                "expAfter": case["actorInitialExp"] + command_exp,
            },
            "finalActorMp": actor_after_mp,
            "finalActorExp": case["actorInitialExp"] + command_exp,
            "finalTargets": [
                {
                    "combatant": actor_target["combatant"],
                    "status": case["statusMask"],
                    "currentDef": actor_def_after,
                    "currentAgi": actor_agi_after,
                },
                {
                    "combatant": case["targets"][1]["combatant"],
                    "status": case["statusMask"],
                    "currentDef": case["targets"][1]["initialCurrentDef"],
                    "currentAgi": case["targets"][1]["initialCurrentAgi"],
                },
            ],
        },
    }


def verify_spell_boost(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 90
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="BOOST fixture")
    verify_runtime_contract(fixture, rom_path)
    harness = load_json(repo_path(fixture["sharedHarnessFixture"]))
    status = load_json(repo_path(fixture["sharedStatusFixture"]))
    healing = load_json(repo_path(fixture["sharedHealingFixture"]))
    _verify_source_contract(_verify_upstream(upstream_path), fixture["case"])
    modeled = _model_expected(fixture)
    if modeled != fixture["expected"]:
        raise ValueError("BOOST golden disagrees with source model")
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
        output_name="spell-boost",
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
            "target": fixture["case"]["actor"],
        },
        **fixture["expected"],
    }
    if observed != expected:
        raise ValueError(
            "BOOST runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Spell": "BOOST 1",
        "FreshStatus": f"0x{fixture['case']['statusMask']:04X}",
        "FreshBonuses": (
            f"DEF+{modeled['construction']['records'][0]['defBonus']},"
            f"AGI+{modeled['construction']['records'][0]['agiBonus']}"
        ),
        "Recast": "roll 7 < threshold 8; status refreshed without stat recompute",
        "CommandExp": modeled["construction"]["award"]["commandExp"],
        "Status": "PASS",
    }
