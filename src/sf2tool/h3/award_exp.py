from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _parse_equates, _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/award-exp-randomization-v1.json")
SCHEMA = repo_path("schemas/h3-award-exp-randomization-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/award_exp_randomization_observer.lua")


def _verify_source_contract(fixture: dict[str, Any], disasm: Path) -> None:
    source = (
        disasm / "code/gameflow/battle/battleactions/giveexpandgold.asm"
    ).read_text(encoding="utf-8")
    required_fragments = (
        "battlesceneScript_GiveExpAndGold:",
        "lsr.w   #1,d1",
        "move.w  #16,d0",
        "addq.w  #1,d1",
        "subq.w  #1,d1",
        "moveq   #1,d1",
        "giveEXP d1",
    )
    if any(fragment not in source for fragment in required_fragments):
        raise ValueError("award EXP randomization source contract drift")

    equates = _parse_equates(disasm)
    halved_table = (
        disasm / "data/battles/global/halvedexpearnedbattles.asm"
    ).read_text(encoding="utf-8")
    if "battle INSIDE_ANCIENT_TOWER" not in halved_table:
        raise ValueError("Battle 01 EXP-halving table drift")
    if fixture["battleId"] != equates["BATTLE_INSIDE_ANCIENT_TOWER"]:
        raise ValueError("award EXP battle identity drift")

    for case in fixture["cases"]:
        next_seed, first = _rng_step(case["seed"], 16)
        _, second = _rng_step(next_seed, 16)
        halved = case["accumulatedExp"] // 2
        command = max(halved + int(first == 0) - int(second == 0), 1)
        modeled = {
            "halvedExp": halved,
            "firstRoll": first,
            "secondRoll": second,
            "commandExp": command,
        }
        if any(case[field] != value for field, value in modeled.items()):
            raise ValueError(f"award EXP golden disagrees with source model: {case['id']}")


def _verify_observation(fixture: dict[str, Any], observed: dict[str, Any]) -> None:
    expected = {
        "battle": fixture["battleId"],
        "cases": [
            {
                "id": case["id"],
                "accumulatedExp": case["accumulatedExp"],
                "halvedExp": case["halvedExp"],
                "firstRoll": case["firstRoll"],
                "secondRoll": case["secondRoll"],
                "commandExp": case["commandExp"],
            }
            for case in fixture["cases"]
        ],
    }
    if (
        observed.get("system") != "GEN"
        or observed.get("core") != fixture["emulator"]["core"]
        or observed.get("result") != expected
    ):
        raise ValueError("award EXP randomization runtime matrix mismatch")


def verify_award_exp_randomization(
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
            "cases": fixture["cases"],
        },
        output_name="award-exp-randomization",
        timeout_seconds=timeout_seconds,
    )
    _verify_observation(fixture, observed)
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "Rolls": [f"{case['firstRoll']}/{case['secondRoll']}" for case in fixture["cases"]],
        "Awards": [case["commandExp"] for case in fixture["cases"]],
        "Status": "PASS",
    }
