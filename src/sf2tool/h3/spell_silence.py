from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-silence-gate-v1.json")
SCHEMA = repo_path("schemas/h3-spell-silence-gate-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_silence_gate_observer.lua")


def _verify_source_contract(disasm: Path, case: dict[str, Any]) -> None:
    definitions = (disasm / "data/stats/spells/spelldefs.asm").read_text(encoding="utf-8")
    blaze = re.search(
        r"entry\s+BLAZE\s*;\s*BLAZE 1(?P<body>.*?)(?=\n\s*entry\s+)",
        definitions,
        re.DOTALL,
    )
    if not blaze:
        raise ValueError("pinned spell definitions do not contain BLAZE 1")
    body = blaze.group("body")
    if f"mpCost     {case['spellMpCost']}" not in body:
        raise ValueError("BLAZE 1 MP cost disagrees with the fixture")
    if "properties TYPE_ATTACK|AFFECTEDBYSILENCE" not in body:
        raise ValueError("BLAZE 1 silence property disagrees with the fixture")

    properties = (
        disasm / "code/gameflow/battle/battleactions/initbattlesceneproperties.asm"
    ).read_text(encoding="utf-8")
    engine1 = (
        disasm / "code/gameflow/battle/battleactions/battleactionsengine_1.asm"
    ).read_text(encoding="utf-8")
    engine2 = (
        disasm / "code/gameflow/battle/battleactions/battleactionsengine_2.asm"
    ).read_text(encoding="utf-8")
    cast = (disasm / "code/gameflow/battle/battleactions/castspell.asm").read_text(
        encoding="utf-8"
    )
    animation = (
        disasm / "code/gameflow/battle/battleactions/createbattlesceneanimation.asm"
    ).read_text(encoding="utf-8")
    property_fragments = (
        "btst    #SPELLPROPS_BIT_AFFECTEDBYSILENCE,SPELLDEF_OFFSET_PROPS(a0)",
        "andi.w  #STATUSEFFECT_SILENCE,d1",
        "sne     silencedActor(a2)",
    )
    block_fragments = (
        "bsr.w   battlesceneScript_PerformAnimation",
        "tst.b   silencedActor(a2)",
        "displayMessage #MESSAGE_BATTLE_SILENCED",
        "bsr.w   battlesceneScript_ApplyActionEffect",
    )
    if any(fragment not in properties for fragment in property_fragments):
        raise ValueError("silence property gate source contract drifted")
    if any(fragment not in engine1 for fragment in block_fragments):
        raise ValueError("silenced action branch source contract drifted")
    cost_fragments = (
        "; Decrease caster's MP",
        "jsr     GetSpellCost",
        "executeAllyReaction #0,d2,d1,#0",
    )
    if any(fragment not in animation for fragment in cost_fragments):
        raise ValueError("spell-cost-before-silence source contract drifted")
    if "tst.b   silencedActor(a2)" not in engine2:
        raise ValueError("silenced EXP suppression source contract drifted")
    if "spellEffect_Blaze:" not in cast:
        raise ValueError("BLAZE effect source contract drifted")


def _model_expected(fixture: dict[str, Any]) -> dict[str, Any]:
    case = fixture["case"]
    blocked = bool(case["actorInitialStatus"] & case["silenceMask"])
    if not blocked:
        raise ValueError("silence fixture does not provide a silenced caster")
    return {
        "construction": {
            "silencedFlagAfterProperties": 255,
            "silencedFlagAtDecision": 255,
            "silencedMessageCommands": 1,
            "notSilencedEntries": 0,
            "costMpChange": -case["spellMpCost"],
            "costStatusCommand": case["actorInitialStatus"],
            "applyActionEffectCalls": 0,
            "blazeEffectCalls": 0,
            "expSilencedFlag": 255,
            "accumulatedExp": 0,
            "actorMp": case["actorInitialMp"],
            "actorStatus": case["actorInitialStatus"],
            "targetHp": case["targetInitialHp"],
            "targetMp": case["targetInitialMp"],
            "targetStatus": case["targetInitialStatus"],
        },
        "replay": {
            "allyReactionCalls": 1,
            "allyReaction": {
                "combatant": case["actor"],
                "mpChange": -case["spellMpCost"],
                "statusCommand": case["actorInitialStatus"],
                "mpBefore": case["actorInitialMp"],
                "mpAfter": case["actorInitialMp"] - case["spellMpCost"],
            },
            "enemyReactionCalls": 0,
            "expReactionCalls": 0,
            "finalActorMp": case["actorInitialMp"] - case["spellMpCost"],
            "finalActorExp": case["actorInitialExp"],
            "finalActorStatus": case["actorInitialStatus"],
            "finalTargetHp": case["targetInitialHp"],
            "finalTargetMp": case["targetInitialMp"],
            "finalTargetStatus": case["targetInitialStatus"],
        },
    }


def verify_spell_silence_gate(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 90
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="silenced-caster fixture")
    verify_runtime_contract(fixture, rom_path)
    harness = load_json(repo_path(fixture["sharedHarnessFixture"]))
    _verify_source_contract(_verify_upstream(upstream_path), fixture["case"])
    modeled = _model_expected(fixture)
    if modeled != fixture["expected"]:
        raise ValueError("silenced-caster golden disagrees with source model")
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {**harness["function"], **fixture["function"]},
            "ram": harness["ram"],
            "harness": harness["harness"],
            "case": fixture["case"],
        },
        output_name="spell-silence-gate",
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
            "target": fixture["case"]["target"],
        },
        **fixture["expected"],
    }
    if observed != expected:
        raise ValueError(
            "silenced-caster runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Spell": "BLAZE 1",
        "SilencedFlag": modeled["construction"]["silencedFlagAtDecision"],
        "EffectCalls": modeled["construction"]["blazeEffectCalls"],
        "Mp": modeled["replay"]["finalActorMp"],
        "Exp": modeled["replay"]["finalActorExp"],
        "TargetHp": modeled["replay"]["finalTargetHp"],
        "Status": "PASS",
    }
