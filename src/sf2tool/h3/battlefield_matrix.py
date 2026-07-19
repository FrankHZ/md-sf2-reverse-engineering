from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h2.battlefield import build_weighted_movement_model
from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/battlefield-movement-matrix-v1.json")
SCHEMA = repo_path("schemas/h3-battlefield-movement-matrix-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/battlefield_movement_matrix_observer.lua")
GRID_SIZE = 48 * 48


def _case_terrain(case: dict[str, Any]) -> list[int]:
    terrain = [case["terrainDefault"]] * GRID_SIZE
    for range_ in case["terrainRanges"]:
        for offset in range(range_["start"], range_["end"] + 1):
            terrain[offset] = range_["value"]
    for entry in case["terrainEntries"]:
        terrain[entry["offset"]] = entry["value"]
    return terrain


def _model_case(case: dict[str, Any]) -> dict[str, Any]:
    model = build_weighted_movement_model(
        _case_terrain(case),
        case["moveCosts"],
        start_offset=case["startOffset"],
        budget=case["budget"],
    )
    out_of_range_neighbor_calls = sum(
        neighbor < 0 or neighbor >= GRID_SIZE
        for current in model["expansionOrder"]
        for neighbor in (current + 1, current - 1, current - 48, current + 48)
    )
    return {
        "id": case["id"],
        "budget": case["budget"],
        "startOffset": case["startOffset"],
        "reachableCount": model["reachableCount"],
        "maximumCost": model["maximumCost"],
        "expansionOrder": model["expansionOrder"],
        "outOfRangeNeighborCalls": out_of_range_neighbor_calls,
        "probes": [
            {
                "offset": offset,
                "cost": model["reachableCosts"].get(str(offset), -1),
            }
            for offset in case["probeOffsets"]
        ],
    }


def verify_battlefield_movement_matrix(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="battlefield movement matrix fixture")
    verify_runtime_contract(fixture, rom_path)
    disasm = _verify_upstream(upstream_path)
    source = (disasm / "code/gameflow/battle/battlefield/buildmovementarrays.asm").read_text(
        encoding="utf-8"
    )
    for fragment in (
        "BuildMovementArrays:",
        "TestAndMarkNeighborSpace:",
        "andi.w  #BATTLEFIELD_MOVE_BUDGET_MASK,d1",
        "move.w  d5,tempMovableGridArray(a6,d1.w)",
    ):
        if fragment not in source:
            raise ValueError(f"battlefield runtime source contract drift: {fragment}")

    modeled = [_model_case(case) for case in fixture["cases"]]
    for case, expected in zip(fixture["cases"], modeled, strict=True):
        if case["expected"] != expected:
            raise ValueError(f"battlefield movement golden disagrees with model: {case['id']}")

    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "fixtureId": fixture["id"],
            "battleId": fixture["battleId"],
            "function": fixture["function"],
            "ram": fixture["ram"],
            "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
            "cases": fixture["cases"],
        },
        output_name="battlefield-movement-matrix",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "battle": fixture["battleId"],
        "records": modeled,
    }
    if observed != expected:
        raise ValueError(
            "battlefield movement runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(modeled),
        "ReachableCounts": ",".join(str(row["reachableCount"]) for row in modeled),
        "BizHawkLaunches": 1,
        "Status": "PASS",
    }
