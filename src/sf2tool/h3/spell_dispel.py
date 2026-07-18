from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-dispel-v1.json")
SCHEMA = repo_path("schemas/h3-spell-dispel-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_dispel_observer.lua")


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
    dispel = re.search(
        r"entry\s+DISPEL\s*;\s*DISPEL 1(?P<body>.*?)(?=\n\s*entry\s+)",
        definitions,
        re.DOTALL,
    )
    if not dispel:
        raise ValueError("pinned spell definitions do not contain DISPEL 1")
    body = dispel.group("body")
    if f"mpCost     {case['spellMpCost']}" not in body:
        raise ValueError("DISPEL 1 MP cost disagrees with the fixture")
    if "properties TYPE_SUPPORT|AFFECTEDBYSILENCE" not in body:
        raise ValueError("DISPEL 1 properties disagree with the fixture")

    elements = (disasm / "data/stats/spells/spellelements.asm").read_text(encoding="utf-8")
    if "spellElement STATUS     ; 6: DISPEL" not in elements:
        raise ValueError("DISPEL element disagrees with the fixture")

    enums = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    expected_equates = {
        "SPELL_DISPEL": case["actionSpell"],
        "SPELL_NOTHING": case["spellNothing"],
        "CHANCE_TO_INFLICT_SILENCE": case["baseThreshold"],
        "STATUSEFFECT_SILENCE": case["statusMask"],
        "STATUSEFFECTCOUNTER_SILENCE": case["statusCounterUnit"],
    }
    for name, expected in expected_equates.items():
        if _equate(enums, name) != expected:
            raise ValueError(f"{name} disagrees with the fixture")

    cast = (disasm / "code/gameflow/battle/battleactions/castspell.asm").read_text(
        encoding="utf-8"
    )
    spell_stats = (disasm / "code/common/stats/spellstats.asm").read_text(encoding="utf-8")
    properties = (
        disasm / "code/gameflow/battle/battleactions/initbattlesceneproperties.asm"
    ).read_text(encoding="utf-8")
    engine = (
        disasm / "code/gameflow/battle/battleactions/battleactionsengine_1.asm"
    ).read_text(encoding="utf-8")
    after_turn = (
        disasm / "code/gameflow/battle/battleloop/processafterturneffects.asm"
    ).read_text(encoding="utf-8")
    cast_fragments = (
        "spellEffect_Dispel:",
        "jsr     GetSpellAndNumberOfSpells",
        "moveq   #8,d3",
        "addq.w  #CHANCE_TO_INFLICT_SILENCE,d3",
        "bsr.w   battlesceneScript_DetermineSpellEffectiveness",
        "ori.w   #STATUSEFFECT_SILENCE,d1",
        "executeEnemyReaction #0,#0,d1,#1",
        "bsr.w   battlesceneScript_AddStatusEffectSpellExp",
    )
    spell_count_fragments = (
        "andi.b  #SPELLENTRY_MASK_INDEX,d0",
        "cmpi.b  #SPELL_NOTHING,d0",
        "addq.w  #1,d2",
    )
    silence_gate_fragments = (
        "btst    #SPELLPROPS_BIT_AFFECTEDBYSILENCE,SPELLDEF_OFFSET_PROPS(a0)",
        "andi.w  #STATUSEFFECT_SILENCE,d1",
        "sne     silencedActor(a2)",
    )
    silence_stop_fragments = (
        "tst.b   silencedActor(a2)",
        "displayMessage #MESSAGE_BATTLE_SILENCED",
    )
    if any(fragment not in cast for fragment in cast_fragments):
        raise ValueError("DISPEL source contract drifted")
    if any(fragment not in spell_stats for fragment in spell_count_fragments):
        raise ValueError("combatant spell-count source contract drifted")
    if any(fragment not in properties for fragment in silence_gate_fragments):
        raise ValueError("silenced-caster action gate source contract drifted")
    if any(fragment not in engine for fragment in silence_stop_fragments):
        raise ValueError("silenced-caster action stop source contract drifted")
    if "subi.w  #STATUSEFFECTCOUNTER_SILENCE,d1" not in after_turn:
        raise ValueError("DISPEL duration decrement source contract drifted")


def _spell_count(entries: list[int], spell_nothing: int) -> int:
    return sum((entry & 0x3F) != spell_nothing for entry in entries)


def _model_expected(fixture: dict[str, Any]) -> dict[str, Any]:
    case = fixture["case"]
    accumulated_exp = 0
    seed = case["seed"]
    records: list[dict[str, Any]] = []
    for target in case["targets"]:
        spell_count = _spell_count(target["spellEntries"], case["spellNothing"])
        threshold = (
            case["immunityThreshold"]
            if spell_count == 0
            else case["baseThreshold"] + target["setting"]
        )
        seed, roll = _rng_step(case["seed"], 8)
        success = roll >= threshold
        if success:
            accumulated_exp = min(accumulated_exp + 5, 49)
        records.append(
            {
                "combatant": target["combatant"],
                "setting": target["setting"],
                "spellCount": spell_count,
                "threshold": threshold,
                "roll": roll,
                "success": success,
                "reactionStatus": case["statusMask"] if success else 0,
                "accumulatedExp": accumulated_exp,
                "statusAfterConstruction": target["initialStatus"],
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

    successful = [
        (target, record)
        for target, record in zip(case["targets"], records, strict=True)
        if record["success"]
    ]
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
                f"enemy:{target['combatant']}:{case['statusMask']}"
                for target, _ in successful
            ],
            "allyReaction": {
                "combatant": case["actor"],
                "mpChange": -case["spellMpCost"],
                "statusCommand": 0,
                "mpBefore": case["actorInitialMp"],
                "mpAfter": case["actorInitialMp"] - case["spellMpCost"],
            },
            "enemyReactions": [
                {
                    "combatant": target["combatant"],
                    "statusCommand": case["statusMask"],
                    "statusBefore": target["initialStatus"],
                    "statusAfter": target["initialStatus"] | case["statusMask"],
                }
                for target, _ in successful
            ],
            "expReaction": {
                "commandExp": command_exp,
                "expBefore": case["actorInitialExp"],
                "expAfter": case["actorInitialExp"] + command_exp,
            },
            "finalActorMp": case["actorInitialMp"] - case["spellMpCost"],
            "finalActorExp": case["actorInitialExp"] + command_exp,
            "finalTargetStatus": [
                target["initialStatus"] | case["statusMask"]
                if record["success"]
                else target["initialStatus"]
                for target, record in zip(case["targets"], records, strict=True)
            ],
        },
    }


def verify_spell_dispel(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 90
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="DISPEL fixture")
    verify_runtime_contract(fixture, rom_path)
    harness = load_json(repo_path(fixture["sharedHarnessFixture"]))
    status = load_json(repo_path(fixture["sharedStatusFixture"]))
    healing = load_json(repo_path(fixture["sharedHealingFixture"]))
    _verify_source_contract(_verify_upstream(upstream_path), fixture["case"])
    modeled = _model_expected(fixture)
    if modeled != fixture["expected"]:
        raise ValueError("DISPEL golden disagrees with source model")
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
        output_name="spell-dispel",
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
            "DISPEL runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Spell": "DISPEL 1",
        "SpellCounts": ",".join(
            str(record["spellCount"]) for record in modeled["construction"]["records"]
        ),
        "Thresholds": ",".join(
            str(record["threshold"]) for record in modeled["construction"]["records"]
        ),
        "Results": ",".join(
            "success" if record["success"] else "failure"
            for record in modeled["construction"]["records"]
        ),
        "Recast": "0x0100->0x0300",
        "CommandExp": modeled["construction"]["award"]["commandExp"],
        "Status": "PASS",
    }
