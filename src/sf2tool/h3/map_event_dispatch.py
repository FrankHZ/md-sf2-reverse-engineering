from __future__ import annotations

import hashlib
from pathlib import Path
from typing import Any

from sf2tool.h2.map_events import build_map_events_contract
from sf2tool.h3.bizhawk import (
    DERIVED_ROOT,
    bizhawk_contract,
    run_observer,
    verify_runtime_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import mega_drive_checksum

FIXTURE = repo_path("tests/fixtures/h3/map-event-dispatch-v1.json")
SCHEMA = repo_path("schemas/h3-map-event-dispatch-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_event_dispatch_observer.lua")

CATEGORY_FUNCTIONS = {
    "entityEvents": "RunMapSetupEntityEvent",
    "zoneEvents": "RunMapSetupZoneEvent",
    "itemEvents": "RunMapSetupItemEvent",
}


def _instrument_rom(rom_path: Path, fixture: dict[str, Any]) -> Path:
    data = bytearray(rom_path.read_bytes())
    patch = fixture["instrumentation"]
    call_site = patch["callSiteAddress"]
    stub_address = patch["stubAddress"]
    original_call = bytes.fromhex(patch["callSiteOriginalHex"])
    patched_call = bytes.fromhex(patch["callSitePatchedHex"])
    original_stub = bytes.fromhex(patch["stubOriginalHex"])
    stub = bytes.fromhex(patch["stubHex"])
    if data[call_site : call_site + len(original_call)] != original_call:
        raise ValueError("map event dispatch call-site bytes drifted")
    if data[stub_address : stub_address + len(original_stub)] != original_stub:
        raise ValueError("map event dispatch padding bytes drifted")
    expected_call = b"\x4E\xB9" + stub_address.to_bytes(4, "big")
    if patched_call != expected_call or len(stub) != len(original_stub):
        raise ValueError("map event dispatch trampoline shape drifted")
    data[call_site : call_site + len(patched_call)] = patched_call
    data[stub_address : stub_address + len(stub)] = stub
    for target in patch["targetStubs"]:
        address = target["address"]
        original = bytes.fromhex(target["originalHex"])
        if data[address : address + len(original)] != original:
            raise ValueError(f"map event target bytes drifted at 0x{address:X}")
        data[address : address + 2] = b"\x4E\x75"
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "map-event-dispatch.instrumented.bin"
    output.write_bytes(data)
    return output


def verify_map_event_dispatch(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map event dispatch runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_events_contract(rom_path, upstream_path)
    listing = upstream_path.resolve(strict=True) / "build/sf2build-h1.lst"
    addresses = listing_symbol_addresses(listing.read_text(encoding="utf-8"))
    function = {
        "setupAddress": addresses["RunMapSetupInitFunction"],
        "redirectAddress": addresses["loc_4750E"] + 4,
        "entries": {
            category: addresses[symbol]
            for category, symbol in CATEGORY_FUNCTIONS.items()
        },
    }
    if fixture["function"] != function:
        raise ValueError("map event dispatch function/address drift")
    if [row["address"] for row in fixture["instrumentation"]["targetStubs"]] != [
        case["expected"]["selectedTargetAddress"] for case in fixture["cases"]
    ]:
        raise ValueError("map event dispatch target-stub order drift")

    tables = {
        category: {row["symbol"]: row for row in value["tables"]}
        for category, value in static["categories"].items()
    }
    modeled = []
    for case, static_case in zip(fixture["cases"], static["selectionCases"], strict=True):
        table = tables[static_case["category"]][static_case["selectedTable"]]
        expected = {
            "selectedRecordOffset": static_case["selectedRecordAddress"] - table["address"],
            "selectedTargetAddress": static_case["resolvedTargetAddress"],
            "eventFlags": static_case["eventFlags"],
            "maskedItem": (
                static_case["query"]["item"] & 0x7F
                if static_case["category"] == "itemEvents"
                else None
            ),
        }
        for field in ("id", "category", "map", "setFlags", "query"):
            if case[field] != static_case[field]:
                raise ValueError(f"map event runtime case disagrees with H2 model: {case['id']}")
        if case["expected"] != expected:
            raise ValueError(f"map event runtime golden disagrees with H2 model: {case['id']}")
        modeled.append({"id": case["id"], "category": case["category"], **expected})

    instrumented_rom = _instrument_rom(rom_path, fixture)
    _, executable = bizhawk_contract()
    user_db = executable.parent / "gamedb" / "gamedb_user.txt"
    prior_user_db = user_db.read_bytes() if user_db.exists() else None
    md5 = hashlib.md5(instrumented_rom.read_bytes()).hexdigest().upper()
    prior_text = prior_user_db.decode("utf-8") if prior_user_db is not None else ""
    separator = "" if not prior_text or prior_text.endswith("\n") else "\n"
    user_db.write_text(
        f"{prior_text}{separator}{md5}\t\tSF2 H3 instrumented map events\tGEN\n",
        encoding="utf-8",
    )
    try:
        observed = run_observer(
            rom_path=instrumented_rom,
            observer_path=OBSERVER,
            config={
                "fixtureId": fixture["id"],
                "mapTestIndex": fixture["mapTestIndex"],
                "function": fixture["function"],
                "ram": fixture["ram"],
                "instrumentation": fixture["instrumentation"],
                "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))[
                    "harness"
                ],
                "cases": fixture["cases"],
            },
            output_name="map-event-dispatch",
            timeout_seconds=timeout_seconds,
        )
    finally:
        if prior_user_db is None:
            user_db.unlink(missing_ok=True)
        else:
            user_db.write_bytes(prior_user_db)
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "records": modeled,
    }
    if observed != expected:
        raise ValueError(
            "map event dispatch runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(modeled),
        "Categories": len({row["category"] for row in modeled}),
        "BizHawkLaunches": 1,
        "Status": "PASS",
    }
