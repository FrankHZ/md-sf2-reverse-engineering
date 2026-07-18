from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-desoul-v1.json")
SCHEMA = repo_path("schemas/h3-spell-desoul-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_desoul_observer.lua")


def _verify_source_contract(disasm: Path, case: dict[str, Any]) -> None:
    defs = (disasm / "data/stats/spells/spelldefs.asm").read_text(encoding="utf-8")
    match = re.search(
        r"entry\s+DESOUL\s*;\s*DESOUL 1(?P<body>.*?)(?=\n\s*entry\s+)",
        defs,
        re.DOTALL,
    )
    if not match or f"mpCost     {case['spellMpCost']}" not in match.group("body"):
        raise ValueError("DESOUL 1 definition disagrees with the fixture")
    cast = (disasm / "code/gameflow/battle/battleactions/castspell.asm").read_text(
        encoding="utf-8"
    )
    earn = (disasm / "code/gameflow/battle/battleactions/earnexp.asm").read_text(
        encoding="utf-8"
    )
    gold = (disasm / "data/stats/enemies/enemygold.asm").read_text(encoding="utf-8")
    required = (
        "addq.w  #CHANCE_TO_INFLICT_DESOUL,d2",
        "executeEnemyReaction #$8000,#0,d1,#1",
        "bsr.w   battlesceneScript_AddExpAndGoldForKill",
        "move.b  #-1,targetDies(a2)",
    )
    if any(fragment not in cast for fragment in required):
        raise ValueError("DESOUL source contract drifted")
    if "bsr.w   battlesceneScript_GetKillExp" not in earn or "table_EnemyGold" not in earn:
        raise ValueError("DESOUL reward source contract drifted")
    first_gold = re.search(r"table_EnemyGold:dc\.w\s+(\d+)", gold)
    if not first_gold or int(first_gold.group(1)) != case["targetGold"]:
        raise ValueError("enemy index 0 gold disagrees with the fixture")


def _model_expected(fixture: dict[str, Any]) -> dict[str, Any]:
    case = fixture["case"]
    threshold = case["baseThreshold"] + case["targetSetting"]
    seed, roll = _rng_step(case["seed"], 8)
    success = roll >= threshold
    if not success:
        raise ValueError("DESOUL success fixture seed no longer succeeds")
    accumulated_exp = 49
    halved = accumulated_exp // 2 if fixture["battleId"] == 1 else accumulated_exp
    award_seed = seed
    seed, first_roll = _rng_step(seed, 16)
    command_exp = halved + int(first_roll == 0)
    _, second_roll = _rng_step(seed, 16)
    command_exp = max(command_exp - int(second_roll == 0), 1)
    return {
        "construction": {
            "setting": case["targetSetting"],
            "threshold": threshold,
            "roll": roll,
            "success": True,
            "instantDeathCommand": 0x8000,
            "accumulatedExp": accumulated_exp,
            "accumulatedGold": case["targetGold"],
            "targetDiesFlag": 255,
            "targetHp": case["targetInitialHp"],
            "actorMp": case["initialMp"],
            "award": {
                "seed": award_seed,
                "halved": halved,
                "firstRoll": first_roll,
                "secondRoll": second_roll,
                "commandExp": command_exp,
                "commandGold": case["targetGold"],
            },
        },
        "replay": {
            "allyReaction": {
                "mpChange": -case["spellMpCost"],
                "mpBefore": case["initialMp"],
                "mpAfter": case["initialMp"] - case["spellMpCost"],
            },
            "enemyReaction": {
                "hpChange": -0x8000,
                "hpBefore": case["targetInitialHp"],
                "hpAfter": 0,
            },
            "expReaction": {
                "commandExp": command_exp,
                "expBefore": case["actorInitialExp"],
                "expAfter": case["actorInitialExp"] + command_exp,
            },
            "goldReaction": {
                "commandGold": case["targetGold"],
                "goldBefore": 0,
                "goldAfter": case["targetGold"],
            },
            "finalActorMp": case["initialMp"] - case["spellMpCost"],
            "finalActorExp": case["actorInitialExp"] + command_exp,
            "finalTargetHp": 0,
            "finalGold": case["targetGold"],
        },
    }


def verify_spell_desoul(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 90
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="DESOUL fixture")
    verify_runtime_contract(fixture, rom_path)
    harness = load_json(repo_path(fixture["sharedHarnessFixture"]))
    status = load_json(repo_path(fixture["sharedStatusFixture"]))
    _verify_source_contract(_verify_upstream(upstream_path), fixture["case"])
    modeled = _model_expected(fixture)
    if modeled != fixture["expected"]:
        raise ValueError("DESOUL golden disagrees with source model")
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {**harness["function"], **status["function"], **fixture["function"]},
            "ram": {**harness["ram"], **fixture["ram"]},
            "harness": harness["harness"],
            "case": fixture["case"],
        },
        output_name="spell-desoul",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["case"]["id"],
        "battle": fixture["battleId"],
        "action": {"type": 1, "spell": 8, "target": 128},
        **fixture["expected"],
    }
    if observed != expected:
        raise ValueError(
            "DESOUL runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Spell": "DESOUL 1",
        "Roll": modeled["construction"]["roll"],
        "KillExp": modeled["construction"]["accumulatedExp"],
        "CommandExp": modeled["construction"]["award"]["commandExp"],
        "Gold": modeled["replay"]["finalGold"],
        "PersistentHp": modeled["replay"]["finalTargetHp"],
        "Status": "PASS",
    }
