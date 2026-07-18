from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _parse_equates, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/gold-boundaries-v1.json")
SCHEMA = repo_path("schemas/h3-gold-boundaries-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/gold_boundaries_observer.lua")


def _verify_source_contract(fixture: dict[str, Any], disasm: Path) -> None:
    source = (disasm / "code/common/stats/gold.asm").read_text(encoding="utf-8")
    required = (
        "IncreaseGold:",
        "add.l   ((CURRENT_GOLD-$1000000)).w,d1",
        "bcs.s   @CapGoldAmount",
        "cmpi.l  #FORCE_MAX_GOLD,d1",
        "move.l  #FORCE_MAX_GOLD,d1",
        "move.l  d1,((CURRENT_GOLD-$1000000)).w",
    )
    if any(fragment not in source for fragment in required):
        raise ValueError("gold boundary source contract drift")
    cap = _parse_equates(disasm)["FORCE_MAX_GOLD"]
    for case in fixture["cases"]:
        total = case["initialGold"] + case["addedGold"]
        carry = total > 0xFFFFFFFF
        wrapped = total & 0xFFFFFFFF
        final = cap if carry or wrapped > cap else wrapped
        if case["carry"] != carry or case["finalGold"] != final:
            raise ValueError(f"gold boundary golden disagrees with source model: {case['id']}")


def _verify_observation(fixture: dict[str, Any], observed: dict[str, Any]) -> None:
    expected = {
        "battle": fixture["battleId"],
        "cases": [
            {
                "id": case["id"],
                "initialGold": case["initialGold"],
                "addedGold": case["addedGold"],
                "finalGold": case["finalGold"],
            }
            for case in fixture["cases"]
        ],
    }
    if (
        observed.get("system") != "GEN"
        or observed.get("core") != fixture["emulator"]["core"]
        or observed.get("result") != expected
    ):
        raise ValueError("gold boundary runtime matrix mismatch")


def verify_gold_boundaries(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 75
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
        output_name="gold-boundaries",
        timeout_seconds=timeout_seconds,
    )
    _verify_observation(fixture, observed)
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "FinalGold": [case["finalGold"] for case in fixture["cases"]],
        "Status": "PASS",
    }
