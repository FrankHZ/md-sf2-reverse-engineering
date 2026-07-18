from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _parse_equates, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/exp-command-boundaries-v1.json")
SCHEMA = repo_path("schemas/h3-exp-command-boundaries-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/exp_command_boundaries_observer.lua")


def _verify_source_contract(fixture: dict[str, Any], disasm: Path) -> None:
    replay = (
        disasm / "code/gameflow/battle/battlescenes/battlesceneengine_0.asm"
    ).read_text(encoding="utf-8")
    stats = (disasm / "code/common/stats/combatantstats_2.asm").read_text(
        encoding="utf-8"
    )
    helper = (disasm / "code/common/stats/combatantstats_3.asm").read_text(
        encoding="utf-8"
    )
    level_up = (disasm / "code/common/stats/levelup.asm").read_text(encoding="utf-8")
    required = (
        (
            replay,
            "andi.w  #$7FFF,d1",
            "btst    #15,d1",
            "subi.w  #100,d1",
            "jsr     j_LevelUp",
        ),
        (stats, "IncreaseExp:", "move.w  #CHAR_STATCAP_EXP,d6"),
        (helper, "IncreaseAndClampByte:", "move.b  d6,d1"),
        (level_up, "moveq   #CHAR_LEVELCAP_PROMOTED,d2", "moveq   #CHAR_LEVELCAP_BASE,d2"),
    )
    if any(fragment not in source for source, *fragments in required for fragment in fragments):
        raise ValueError("EXP command boundary source contract drift")

    equates = _parse_equates(disasm)
    exp_cap = equates["CHAR_STATCAP_EXP"]
    first_promoted = equates["CHAR_CLASS_FIRSTPROMOTED"]
    for case in fixture["cases"]:
        if case["class"] != equates[f"CLASS_{case['classCode']}"]:
            raise ValueError(f"EXP command class identity drift: {case['id']}")
        after_increase = min(case["initialExp"] + case["commandExp"], exp_cap)
        calls = int(after_increase >= 100)
        final_exp = after_increase - 100 if calls else after_increase
        level_cap = (
            equates["CHAR_LEVELCAP_PROMOTED"]
            if case["class"] >= first_promoted
            else equates["CHAR_LEVELCAP_BASE"]
        )
        final_level = case["level"] + int(calls == 1 and case["level"] < level_cap)
        modeled = {
            "expAfterIncrease": after_increase,
            "levelUpCalls": calls,
            "finalLevel": final_level,
            "finalExp": final_exp,
        }
        if any(case[field] != value for field, value in modeled.items()):
            raise ValueError(f"EXP command golden disagrees with source model: {case['id']}")
        expected_result = final_level if final_level != case["level"] else 255
        if calls and case.get("levelUpResult") != expected_result:
            raise ValueError(f"EXP command LevelUp result drift: {case['id']}")


def _verify_observation(fixture: dict[str, Any], observed: dict[str, Any]) -> None:
    expected_cases = []
    for case in fixture["cases"]:
        expected = {
            "id": case["id"],
            "class": case["class"],
            "initialLevel": case["level"],
            "initialExp": case["initialExp"],
            "commandExp": case["commandExp"],
            "expAfterIncrease": case["expAfterIncrease"],
            "levelUpCalls": case["levelUpCalls"],
            "finalLevel": case["finalLevel"],
            "finalExp": case["finalExp"],
        }
        if "levelUpResult" in case:
            expected["levelUpResult"] = case["levelUpResult"]
        expected_cases.append(expected)
    expected_result = {"battle": fixture["battleId"], "cases": expected_cases}
    if (
        observed.get("system") != "GEN"
        or observed.get("core") != fixture["emulator"]["core"]
        or observed.get("result") != expected_result
    ):
        raise ValueError("EXP command boundary runtime matrix mismatch")


def verify_exp_command_boundaries(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 90
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    verify_runtime_contract(fixture, rom_path)
    _verify_source_contract(fixture, _verify_upstream(upstream_path))
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {**fixture["harness"]["function"], **fixture["function"]},
            "ram": {**fixture["harness"]["ram"], **fixture["ram"]},
            "actor": fixture["actor"],
            "target": fixture["target"],
            "cases": fixture["cases"],
        },
        output_name="exp-command-boundaries",
        timeout_seconds=timeout_seconds,
    )
    _verify_observation(fixture, observed)
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "FinalLevels": [case["finalLevel"] for case in fixture["cases"]],
        "FinalExp": [case["finalExp"] for case in fixture["cases"]],
        "Status": "PASS",
    }
