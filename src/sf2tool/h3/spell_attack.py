from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-attack-v1.json")
SCHEMA = repo_path("schemas/h3-spell-attack-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_attack_observer.lua")


def _equate(source: str, name: str) -> int:
    match = re.search(
        rf"^{re.escape(name)}:\s+equ\s+\$([0-9A-F]+)\s*$", source, re.MULTILINE
    )
    if not match:
        raise ValueError(f"missing pinned equate: {name}")
    return int(match.group(1), 16)


def _verify_source_contract(disasm: Path, case: dict[str, Any]) -> None:
    definitions = (disasm / "data/stats/spells/spelldefs.asm").read_text(
        encoding="utf-8"
    )
    attack = re.search(
        r"entry\s+ATTACK\s*;\s*ATTACK 1(?P<body>.*?)(?=\n\s*entry\s+)",
        definitions,
        re.DOTALL,
    )
    if not attack or f"mpCost     {case['spellMpCost']}" not in attack.group("body"):
        raise ValueError("ATTACK 1 definition disagrees with the fixture")

    enums = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    if _equate(enums, "STATUSEFFECT_ATTACK") != case["statusMask"]:
        raise ValueError("ATTACK status mask disagrees with the fixture")
    if _equate(enums, "STATUSEFFECTCOUNTER_ATTACK") != case["statusCounterUnit"]:
        raise ValueError("ATTACK counter unit disagrees with the fixture")

    cast = (disasm / "code/gameflow/battle/battleactions/castspell.asm").read_text(
        encoding="utf-8"
    )
    stats = (disasm / "code/common/stats/updatecombatantstats.asm").read_text(
        encoding="utf-8"
    )
    required_cast = (
        "spellEffect_Attack:",
        "ori.w   #STATUSEFFECT_ATTACK,d1",
        "andi.w  #STATUSEFFECT_ATTACK,d3",
        "moveq   #8,d2",
        "executeAllyReaction #0,#0,d1,#2",
        "bsr.w   battlesceneScript_AddStatusEffectSpellExp",
        "jsr     GetBaseAtt",
        "mulu.w  #3,d1",
        "lsr.l   #3,d1",
    )
    required_stats = (
        "andi.w  #STATUSEFFECT_ATTACK,d2",
        "rol.w   #2,d2",
        "bsr.w   IncreaseCurrentAtt",
    )
    if any(fragment not in cast for fragment in required_cast):
        raise ValueError("ATTACK spell source contract drift")
    if any(fragment not in stats for fragment in required_stats):
        raise ValueError("ATTACK stat-refresh source contract drift")


def _model_expected(fixture: dict[str, Any]) -> list[dict[str, Any]]:
    case = fixture["case"]
    accumulator = 0
    records: list[dict[str, Any]] = []
    for target in case["targets"]:
        set_status = target["initialStatus"] | case["statusMask"]
        reapplication = bool(target["initialStatus"] & case["statusMask"])
        threshold = case["recastThreshold"] if reapplication else 0
        roll = -1
        success = True
        reaction_status = set_status
        bonus = (target["baseAtt"] * 3) // 8
        if reapplication:
            _, roll = _rng_step(case["seed"], 8)
            success = roll >= threshold
            if not success:
                reaction_status = 0
                bonus = 0
        if success:
            accumulator = min(accumulator + 5, 49)
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
                "accumulatedExp": accumulator,
                "attackBonus": bonus,
                "statusAfterConstruction": set_status,
                "currentAttAfterConstruction": target["initialCurrentAtt"],
            }
        )
    return records


def verify_spell_attack(
    rom_path: Path,
    upstream_path: Path,
    *,
    timeout_seconds: int = 90,
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="ATTACK spell fixture")
    verify_runtime_contract(fixture, rom_path)
    shared = load_json(repo_path(fixture["sharedHarnessFixture"]))
    _verify_source_contract(_verify_upstream(upstream_path), fixture["case"])
    modeled = _model_expected(fixture)
    if modeled != fixture["expected"]:
        raise ValueError("ATTACK golden disagrees with source model")
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {**shared["function"], **fixture["function"]},
            "ram": {**shared["ram"], **fixture["ram"]},
            "harness": shared["harness"],
            "battleId": fixture["battleId"],
            "case": fixture["case"],
        },
        output_name="spell-attack",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["case"]["id"],
        "battle": fixture["battleId"],
        "records": fixture["expected"],
    }
    if observed != expected:
        raise ValueError(
            "ATTACK spell runtime mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "FreshStatus": f"0x{modeled[0]['setStatus']:04X}",
        "FreshAttackBonus": modeled[0]["attackBonus"],
        "Recast": f"roll {modeled[1]['roll']} < threshold {modeled[1]['threshold']}",
        "ConstructionCurrentAtt": [
            record["currentAttAfterConstruction"] for record in modeled
        ],
        "AccumulatedExp": modeled[-1]["accumulatedExp"],
        "Status": "PASS",
    }
