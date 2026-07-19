from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h2.map_setup import build_map_setup_contract
from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses

FIXTURE = repo_path("tests/fixtures/h3/map-setup-selection-v1.json")
SCHEMA = repo_path("schemas/h3-map-setup-selection-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_setup_selection_observer.lua")


def verify_map_setup_selection(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map setup selection runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    setup = build_map_setup_contract(rom_path, upstream_path)
    listing = upstream_path.resolve(strict=True) / "build/sf2build-h1.lst"
    addresses = listing_symbol_addresses(listing.read_text(encoding="utf-8"))
    navigation = {
        "mapPromptReturnAddress": 0x715E,
        "flagPromptReturnAddress": 0x7178,
        "explorationLoopAddress": addresses["ExplorationLoop"],
        "getMapSetupEntityListAddress": addresses["GetMapSetupEntityList"],
        "runMapSetupInitFunctionAddress": addresses["RunMapSetupInitFunction"],
    }
    modeled = []
    for case, static_case in zip(fixture["cases"], setup["selectionCases"], strict=True):
        expected = {
            "id": static_case["id"],
            "map": static_case["map"],
            "selectedSymbol": static_case["selectedPointer"],
            "selectedAddress": addresses[static_case["selectedPointer"]],
        }
        if (
            case["id"] != static_case["id"]
            or case["map"] != static_case["map"]
            or case["setFlags"] != static_case["setFlags"]
            or case["expected"] != expected
        ):
            raise ValueError(f"map setup runtime golden disagrees with H2 model: {case['id']}")
        modeled.append(expected)

    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "fixtureId": fixture["id"],
            "mapTestIndex": fixture["mapTestIndex"],
            "function": fixture["function"],
            "ram": fixture["ram"],
            "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
            "navigation": navigation,
            "cases": fixture["cases"],
        },
        output_name="map-setup-selection",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "records": [
            {
                "id": row["id"],
                "map": row["map"],
                "selectedAddress": row["selectedAddress"],
            }
            for row in modeled
        ],
    }
    if observed != expected:
        raise ValueError(
            "map setup selection runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(modeled),
        "SelectedPointers": len({row["selectedAddress"] for row in modeled}),
        "BizHawkLaunches": 1,
        "Status": "PASS",
    }
