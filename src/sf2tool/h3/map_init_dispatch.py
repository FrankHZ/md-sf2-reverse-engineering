from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h2.map_init import build_map_init_contract
from sf2tool.h2.map_setup import build_map_setup_contract
from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/map-init-dispatch-v1.json")
SCHEMA = repo_path("schemas/h3-map-init-dispatch-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_init_dispatch_observer.lua")


def _selected_setup(setup: dict[str, Any], map_index: int, flags: set[int]) -> str | None:
    route = next((row for row in setup["routes"] if row["map"] == map_index), None)
    if route is None:
        return None
    selected = route["defaultPointer"]
    for variant in route["flagVariants"]:
        if variant["flag"] in flags:
            selected = variant["pointer"]
    return selected


def verify_map_init_dispatch(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map init dispatch runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    setup = build_map_setup_contract(rom_path, upstream_path)
    init = build_map_init_contract(rom_path, upstream_path)
    tables = {row["symbol"]: row for row in setup["pointerTables"]}
    functions = {row["symbol"]: row for row in init["sourceFiles"]}
    modeled = []
    for case in fixture["cases"]:
        setup_symbol = _selected_setup(setup, case["map"], set(case["setFlags"]))
        target = tables[setup_symbol]["targets"]["initFunction"] if setup_symbol else None
        function = functions[target["symbol"]] if target else None
        expected = {
            "setupSymbol": setup_symbol,
            "calledSymbol": target["symbol"] if target else None,
            "calledAddress": target["address"] if target else None,
            "targetKind": (
                "skipped"
                if target is None
                else "direct-return"
                if function["directReturnStub"]
                else "active"
            ),
        }
        if case["expected"] != expected:
            raise ValueError(f"map init runtime golden disagrees with H2 model: {case['id']}")
        modeled.append(
            {
                "id": case["id"],
                "map": case["map"],
                "callCount": 0 if target is None else 1,
                "calledAddress": target["address"] if target else None,
            }
        )

    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "fixtureId": fixture["id"],
            "mapTestIndex": fixture["mapTestIndex"],
            "function": fixture["function"],
            "ram": fixture["ram"],
            "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
            "cases": fixture["cases"],
        },
        output_name="map-init-dispatch",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "records": modeled,
    }
    if observed != expected:
        raise ValueError(
            "map init dispatch runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(modeled),
        "CalledTargets": len(
            {row["calledAddress"] for row in modeled if row["calledAddress"] is not None}
        ),
        "SkippedCalls": sum(row["calledAddress"] is None for row in modeled),
        "BizHawkLaunches": 1,
        "Status": "PASS",
    }
