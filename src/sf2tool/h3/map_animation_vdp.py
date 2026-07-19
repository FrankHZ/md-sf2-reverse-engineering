from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h2.map_import import build_canonical_map_import
from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses

FIXTURE = repo_path("tests/fixtures/h3/map-animation-vdp-v1.json")
SCHEMA = repo_path("schemas/h3-map-animation-vdp-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_animation_vdp_observer.lua")

RAM = {
    "currentMapAddress": 0xFFF711,
    "animationCacheAddress": 0xFF9B04,
    "animationCacheCapacityBytes": 96 * 32,
    "animationDataAddress": 0xFFA86A,
    "animationCounterAddress": 0xFFA86E,
    "animationMapAddress": 0xFFA870,
    "dmaQueueBaseAddress": 0xFFD550,
    "dmaQueueSizeAddress": 0xFFDE96,
    "dmaQueuePointerAddress": 0xFFDED4,
}


def _modeled_case(case: dict[str, Any], table: dict[str, Any]) -> dict[str, Any]:
    records = table["records"]
    entries = records["entries"]
    selected_index = 0 if case["startAtTerminator"] else case["entryIndex"]
    selected = entries[selected_index]
    data_address_before = (
        table["address"] + 4 + len(entries) * 8
        if case["startAtTerminator"]
        else table["address"] + 4 + selected_index * 8
    )
    data_address_after_submit = table["address"] + 4 + (selected_index + 1) * 8
    if selected["counter"] == 1:
        next_entry = entries[selected_index + 1]
        counter_after_transfer = next_entry["counter"]
        data_address_after_transfer = table["address"] + 4 + (selected_index + 2) * 8
        queue_size_after_transfer = 1
    else:
        counter_after_transfer = selected["counter"] - 1
        data_address_after_transfer = data_address_after_submit
        queue_size_after_transfer = 0
    return {
        "dataAddressBefore": data_address_before,
        "dataAddressAfterSubmit": data_address_after_submit,
        "dataAddressAfterTransfer": data_address_after_transfer,
        "sourceByteOffset": selected["replacementStartTile"] * 32,
        "byteCount": selected["tileCount"] * 32,
        "targetByteAddress": selected["targetStartTile"] * 32,
        "counterAfterSubmit": selected["counter"],
        "counterAfterTransfer": counter_after_transfer,
        "queueContributionAfterSubmit": 1,
        "queueContributionAfterTransfer": queue_size_after_transfer,
    }


def verify_map_animation_vdp(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map animation VDP runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_canonical_map_import(rom_path, upstream_path)
    listing = upstream_path.resolve(strict=True) / "build/sf2build-h1.lst"
    addresses = listing_symbol_addresses(listing.read_text(encoding="utf-8"))
    function = {
        "animationAddress": addresses["VInt_UpdateMapAnimations"],
        "processDmaQueueAddress": addresses["ProcessDmaQueue"],
        "waitForEventAddress": addresses["WaitForEvent"],
    }
    if fixture["function"] != function or fixture["ram"] != RAM:
        raise ValueError("map animation VDP function/RAM contract drift")

    tables = {row["id"]: row for row in static["resources"]["animationTables"]}
    modeled = []
    for case in fixture["cases"]:
        table = tables[case["table"]]
        expected = _modeled_case(case, table)
        if case["expected"] != expected:
            raise ValueError(f"map animation runtime golden disagrees with H2: {case['id']}")
        if case["map"] != int(case["table"][3:5]):
            raise ValueError(f"map animation case/table identity drift: {case['id']}")
        modeled.append(case)

    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "fixtureId": fixture["id"],
            "mapTestIndex": fixture["mapTestIndex"],
            "function": fixture["function"],
            "ram": fixture["ram"],
            "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
            "cases": modeled,
        },
        output_name="map-animation-vdp",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "records": [
            {
                "id": case["id"],
                "map": case["map"],
                "targetWasSentinelBeforeSubmit": True,
                "targetWasSentinelAfterSubmit": True,
                "targetMatchedSourceAfterTransfer": True,
                "dataAddressAfterSubmit": case["expected"]["dataAddressAfterSubmit"],
                "dataAddressAfterTransfer": case["expected"]["dataAddressAfterTransfer"],
                "counterAfterSubmit": case["expected"]["counterAfterSubmit"],
                "counterAfterTransfer": case["expected"]["counterAfterTransfer"],
                "queueContributionAfterSubmit": case["expected"][
                    "queueContributionAfterSubmit"
                ],
                "queueContributionAfterTransfer": case["expected"][
                    "queueContributionAfterTransfer"
                ],
            }
            for case in modeled
        ],
    }
    if observed != expected:
        raise ValueError(
            "map animation VDP runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(modeled),
        "TransferredBytes": sum(case["expected"]["byteCount"] for case in modeled),
        "TerminatorWrapCases": sum(case["startAtTerminator"] for case in modeled),
        "BizHawkLaunches": 1,
        "Status": "PASS",
    }
