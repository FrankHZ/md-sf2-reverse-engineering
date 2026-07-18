from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-mp-absorb-v1.json")
SCHEMA = repo_path("schemas/h3-spell-mp-absorb-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_mp_absorb_observer.lua")


def _verify_source_contract(disasm: Path, case: dict[str, Any]) -> None:
    defs = (disasm / "data/stats/spells/spelldefs.asm").read_text(encoding="utf-8")
    match = re.search(
        r"entry\s+SPOIT\s*;\s*SPOIT 1(?P<body>.*?)(?=\n\s*entry\s+)",
        defs,
        re.DOTALL,
    )
    if not match or f"mpCost     {case['spellMpCost']}" not in match.group("body"):
        raise ValueError("SPOIT definition disagrees with the fixture")
    properties = re.search(r"^\s*properties\s+(.+)$", match.group("body"), re.MULTILINE)
    if not properties or properties.group(1).strip() != "TYPE_SPECIAL":
        raise ValueError("SPOIT silence-immunity property disagrees with the fixture")
    cast = (disasm / "code/gameflow/battle/battleactions/castspell.asm").read_text(
        encoding="utf-8"
    )
    required = (
        "spellEffect_AbsorbMp:",
        "moveq   #3,d0",
        "addq.w  #3,d0",
        "move.w  d1,d0           ; clamp random value to target's current MP",
        "executeEnemyReaction #0,d3,d1,#1",
        "executeAllyReaction #0,d2,d1,#2",
        "bsr.w   battlesceneScript_AddStatusEffectSpellExp",
    )
    if any(fragment not in cast for fragment in required):
        raise ValueError("SPOIT source contract drifted")
    stats2 = (disasm / "code/common/stats/combatantstats_2.asm").read_text(
        encoding="utf-8"
    )
    stats3 = (disasm / "code/common/stats/combatantstats_3.asm").read_text(
        encoding="utf-8"
    )
    replay_required = (
        "IncreaseCurrentMp:",
        "move.b  COMBATANT_OFFSET_MP_MAX(a0),d6",
        "bsr.w   IncreaseAndClampByte",
    )
    clamp_required = (
        "IncreaseAndClampByte:",
        "cmp.b   d6,d1",
        "move.b  d6,d1",
        "move.b  d1,(a0,d7.w)",
    )
    if any(fragment not in stats2 for fragment in replay_required) or any(
        fragment not in stats3 for fragment in clamp_required
    ):
        raise ValueError("SPOIT replay MP-clamp source contract drifted")


def _model_expected(fixture: dict[str, Any]) -> dict[str, Any]:
    case = fixture["case"]
    records = []
    accumulated_exp = 0
    award_seed = None
    for target in case["targets"]:
        seed, roll = _rng_step(case["seed"], 3)
        unclamped = roll + 3
        transfer = min(unclamped, target["initialMp"])
        accumulated_exp += 5
        award_seed = seed
        records.append(
            {
                "combatant": target["combatant"],
                "randomRoll": roll,
                "unclampedTransfer": unclamped,
                "targetMp": target["initialMp"],
                "transfer": transfer,
                "accumulatedExp": accumulated_exp,
            }
        )
    halved = accumulated_exp // 2 if fixture["battleId"] == 1 else accumulated_exp
    assert award_seed is not None
    seed = award_seed
    seed, first_roll = _rng_step(seed, 16)
    command_exp = halved + int(first_roll == 0)
    _, second_roll = _rng_step(seed, 16)
    command_exp = max(command_exp - int(second_roll == 0), 1)
    actor_after_cost = case["actorInitialMp"] - case["spellMpCost"]
    actor_mp = actor_after_cost
    ally_reactions = [
        {
            "combatant": case["actor"],
            "mpChange": -case["spellMpCost"],
            "mpBefore": case["actorInitialMp"],
            "mpAfter": actor_after_cost,
        }
    ]
    enemy_reactions = []
    reaction_order = [f"ally:{-case['spellMpCost']}"]
    final_target_mp = []
    for target, record in zip(case["targets"], records, strict=True):
        transfer = record["transfer"]
        enemy_reactions.append(
            {
                "combatant": target["combatant"],
                "mpChange": -transfer,
                "mpBefore": target["initialMp"],
                "mpAfter": target["initialMp"] - transfer,
            }
        )
        reaction_order.append(f"enemy:{-transfer}")
        before = actor_mp
        actor_mp = min(actor_mp + transfer, case["actorMaxMp"])
        ally_reactions.append(
            {
                "combatant": case["actor"],
                "mpChange": transfer,
                "mpBefore": before,
                "mpAfter": actor_mp,
            }
        )
        reaction_order.append(f"ally:{transfer}")
        final_target_mp.append(target["initialMp"] - transfer)
    return {
        "construction": {
            "records": records,
            "accumulatedExp": accumulated_exp,
            "actorMp": case["actorInitialMp"],
            "actorStatus": case["actorInitialStatus"],
            "award": {
                "seed": award_seed,
                "halved": halved,
                "firstRoll": first_roll,
                "secondRoll": second_roll,
                "commandExp": command_exp,
            },
        },
        "replay": {
            "reactionOrder": reaction_order,
            "allyReactions": ally_reactions,
            "enemyReactions": enemy_reactions,
            "expReaction": {
                "commandExp": command_exp,
                "expBefore": case["actorInitialExp"],
                "expAfter": case["actorInitialExp"] + command_exp,
            },
            "finalActorMp": actor_mp,
            "finalActorExp": case["actorInitialExp"] + command_exp,
            "finalActorStatus": case["actorInitialStatus"],
            "finalTargetMp": final_target_mp,
        },
    }


def verify_spell_mp_absorb(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 90
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="SPOIT fixture")
    verify_runtime_contract(fixture, rom_path)
    harness = load_json(repo_path(fixture["sharedHarnessFixture"]))
    status = load_json(repo_path(fixture["sharedStatusFixture"]))
    _verify_source_contract(_verify_upstream(upstream_path), fixture["case"])
    modeled = _model_expected(fixture)
    if modeled != fixture["expected"]:
        raise ValueError("SPOIT golden disagrees with source model")
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {**harness["function"], **status["function"], **fixture["function"]},
            "ram": harness["ram"],
            "harness": harness["harness"],
            "case": fixture["case"],
        },
        output_name="spell-mp-absorb",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["case"]["id"],
        "battle": fixture["battleId"],
        "action": {
            "type": 1,
            "spell": 15,
            "target": fixture["case"]["targets"][0]["combatant"],
        },
        **fixture["expected"],
    }
    if observed != expected:
        raise ValueError(
            "SPOIT runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Spell": "SPOIT",
        "RandomTransfer": modeled["construction"]["records"][0][
            "unclampedTransfer"
        ],
        "Transfers": "/".join(
            str(record["transfer"])
            for record in modeled["construction"]["records"]
        ),
        "PersistentMp": (
            f"actor={modeled['replay']['finalActorMp']},"
            f"targets={modeled['replay']['finalTargetMp']}"
        ),
        "CasterStatus": f"0x{modeled['replay']['finalActorStatus']:04X}",
        "CommandExp": modeled["construction"]["award"]["commandExp"],
        "Status": "PASS",
    }
