from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _parse_equates, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/kill-exp-level-difference-v1.json")
SCHEMA = repo_path("schemas/h3-kill-exp-level-difference-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/kill_exp_level_difference_observer.lua")


def _kill_exp(level_difference: int) -> int:
    if level_difference < 3:
        return 50
    if level_difference == 3:
        return 40
    if level_difference == 4:
        return 30
    if level_difference == 5:
        return 20
    if level_difference == 6:
        return 10
    return 0


def _verify_source_contract(fixture: dict[str, Any], disasm: Path) -> None:
    source = (disasm / "code/gameflow/battle/battleactions/earnexp.asm").read_text(
        encoding="utf-8"
    )
    required_fragments = (
        "battlesceneScript_GetKillExp:",
        "cmpi.b  #CHAR_CLASS_FIRSTPROMOTED,d3",
        "addi.w  #CHAR_CLASS_EXTRALEVEL,d1",
        "moveq   #50,d5",
        "moveq   #40,d5",
        "moveq   #30,d5",
        "moveq   #20,d5",
        "moveq   #10,d5",
        "moveq   #0,d5",
    )
    if any(fragment not in source for fragment in required_fragments):
        raise ValueError("kill EXP source contract drift")

    equates = _parse_equates(disasm)
    first_promoted = equates["CHAR_CLASS_FIRSTPROMOTED"]
    extra_level = equates["CHAR_CLASS_EXTRALEVEL"]
    for case in fixture["cases"]:
        if case["class"] != equates[f"CLASS_{case['classCode']}"]:
            raise ValueError(f"kill EXP class identity drift: {case['id']}")
        effective = case["actorLevel"]
        if case["class"] >= first_promoted:
            effective += extra_level
        difference = effective - case["targetLevel"]
        modeled = {
            "effectiveActorLevel": effective,
            "levelDifference": difference,
            "killExp": _kill_exp(difference),
        }
        if any(case[field] != value for field, value in modeled.items()):
            raise ValueError(f"kill EXP golden disagrees with source model: {case['id']}")


def _verify_observation(fixture: dict[str, Any], observed: dict[str, Any]) -> None:
    expected_cases = [
        {
            "id": case["id"],
            "actor": case["actor"],
            "target": case["target"],
            "class": case["class"],
            "actorLevel": case["actorLevel"],
            "targetLevel": case["targetLevel"],
            "killExp": case["killExp"],
        }
        for case in fixture["cases"]
    ]
    expected = {"battle": fixture["battleId"], "cases": expected_cases}
    if (
        observed.get("system") != "GEN"
        or observed.get("core") != fixture["emulator"]["core"]
        or observed.get("result") != expected
    ):
        raise ValueError("kill EXP runtime matrix mismatch")


def verify_kill_exp_level_differences(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 60
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
            "cases": fixture["cases"],
        },
        output_name="kill-exp-level-differences",
        timeout_seconds=timeout_seconds,
    )
    _verify_observation(fixture, observed)
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "LevelDifferences": [case["levelDifference"] for case in fixture["cases"]],
        "KillExp": [case["killExp"] for case in fixture["cases"]],
        "Status": "PASS",
    }
