"""Focused H2 contract tests for map-event request-consumption access shape."""

from __future__ import annotations

import hashlib
import json
import subprocess
from collections.abc import Iterator
from contextlib import contextmanager
from copy import deepcopy
from pathlib import Path
from shutil import copy2

import pytest

import sf2tool.h2.map_event_request_consumption as consumption_module
from sf2tool.h2.map_event_request_consumption import (
    _CONTEXTS,
    FIXTURE,
    ID,
    REQUEST_STATE_FIXTURE,
    SCHEMA,
    _projection,
    build_map_event_request_consumption_contract,
    verify_map_event_request_consumption_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.research_index import verify_index

ROOT = Path(__file__).resolve().parents[2]
BASE = "7577d0cb617d5880c7666899b44ed24fa3e58120"
DOCUMENT = "docs/research/map-event-request-consumption.md"
INDEX = ROOT / "manifests/research-index.json"
INDEX_SCHEMA = ROOT / "schemas/research-index.schema.json"
ROM = ROOT / "local/roms/sf2-us.bin"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"
CONSUMER_CONTEXT_FIELD = "eventRequestConsumption.consumerContexts"


def _observation(address_id: str, value: int) -> dict[str, object]:
    return {"id": address_id, "space": "rom", "kind": "observation", "value": value}


def _context_binding(address_id: str, context_key: str) -> dict[str, str]:
    return {
        "addressId": address_id,
        "fixtureField": f"{CONSUMER_CONTEXT_FIELD}.{context_key}.entryAddress",
    }


EXPECTED_INDEX_DELTA = {
    "menus.shop-actions": {
        "addresses": [_observation("get-shop-inventory-address", 133202)],
        "bindings": [_context_binding("get-shop-inventory-address", "getShopInventoryAddress")],
    },
    "gameflow.exploration.loop": {
        "addresses": [_observation("process-map-event", 153930)],
        "bindings": [
            _context_binding("entry", "explorationLoop"),
            _context_binding("wait-for-event", "waitForEvent"),
            _context_binding("process-map-event", "processMapEvent"),
        ],
    },
    "menus.field-main": {
        "addresses": [],
        "bindings": [_context_binding("entry", "fieldMenu")],
    },
    "battle.loop.egress-position": {
        "addresses": [],
        "bindings": [_context_binding("entry", "getEgressPositionForBattle")],
    },
    "scripting.map.mapfunctions": {
        "addresses": [_observation("declare-raft-entity", 278954)],
        "bindings": [_context_binding("declare-raft-entity", "declareRaftEntity")],
    },
    "scripting.map.followersfunctions-2": {
        "addresses": [_observation("raft-refresh", 279556)],
        "bindings": [_context_binding("raft-refresh", "raftRefresh")],
    },
}


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _mutable_inputs(tmp_path: Path) -> tuple[Path, Path]:
    """Copy only this slice's seven-source/H1/ROM input surface for adversaries."""
    upstream = tmp_path / "SF2DISASM"
    source_root = UPSTREAM / "disasm"
    for source_path in consumption_module._SOURCE_PATHS:
        destination = upstream / "disasm" / source_path
        destination.parent.mkdir(parents=True, exist_ok=True)
        copy2(source_root / source_path, destination)
    listing = upstream / "build/sf2build-h1.lst"
    listing.parent.mkdir(parents=True, exist_ok=True)
    copy2(UPSTREAM / "build/sf2build-h1.lst", listing)
    rom = tmp_path / "sf2-us.bin"
    copy2(ROM, rom)
    return upstream, rom


@contextmanager
def _replaced_line(path: Path, line_number: int, replacement: str) -> Iterator[None]:
    original = path.read_bytes()
    lines = original.decode("utf-8").splitlines(keepends=True)
    ending = "\r\n" if lines[line_number - 1].endswith("\r\n") else "\n"
    lines[line_number - 1] = f"{replacement.rstrip(chr(13) + chr(10))}{ending}"
    path.write_bytes("".join(lines).encode("utf-8"))
    try:
        yield
    finally:
        path.write_bytes(original)


def test_request_consumption_production_projection_is_closed_and_exact() -> None:
    """Rebuild the complete bounded source/H1/ROM projection before golden compare."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map-event request-consumption fixture")
    rebuilt = build_map_event_request_consumption_contract(ROM, UPSTREAM)
    validate_json(rebuilt, SCHEMA, owner="map-event request-consumption rebuilt contract")

    assert rebuilt == fixture
    assert set(fixture) == {
        "schemaVersion",
        "id",
        "system",
        "romSha256",
        "upstream",
        "retainedOwners",
        "sourceContext",
        "eventRequestConsumption",
        "unknowns",
        "summary",
    }
    assert fixture["id"] == ID
    assert set(fixture["eventRequestConsumption"]) == {
        "symbolDefinitions",
        "accessSites",
        "accessOrder",
        "consumerContexts",
        "symbolContextRelations",
        "roleCounts",
    }
    assert fixture["summary"] == {
        "retainedPositiveProgramContextCount": 39,
        "retainedZeroProgramContextCount": 875,
        "retainedContextOperationCount": 262,
        "retainedWriteDefinitionSiteCount": 45,
        "retainedHandoffStateSiteCount": 67,
        "retainedHandoffStateRelationCount": 69,
        "sourceFileCount": 7,
        "consumerContextCount": 8,
        "symbolDefinitionCount": 6,
        "lifecycleAccessCount": 13,
        "symbolContextRelationCount": 12,
        "contextRoleCount": 21,
        "physicalAnchorCount": 18,
    }


@pytest.mark.parametrize(
    "mutator",
    (
        lambda value: value.__setitem__("sourceProse", "private"),
        lambda value: value["eventRequestConsumption"].__setitem__("rawSource", "private"),
        lambda value: value["eventRequestConsumption"].__setitem__("accessOrder", []),
        lambda value: value["eventRequestConsumption"]["consumerContexts"].pop("fieldMenu"),
        lambda value: value["sourceContext"]["anchors"].append({}),
        lambda value: value["unknowns"].__setitem__("actualConsumerReadValue", "Confirmed"),
    ),
)
def test_request_consumption_schema_rejects_private_payload_and_shape_drift(
    mutator: object,
) -> None:
    broken = deepcopy(_fixture())
    mutator(broken)
    with pytest.raises(ValueError, match="schema validation"):
        validate_json(broken, SCHEMA, owner="map-event request-consumption fixture")


def test_request_consumption_source_h1_rom_and_retained_owner_guards_fail_closed(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    """Exercise all selected access/control/anchor guards before golden comparison."""
    fixture = _fixture()
    upstream, rom = _mutable_inputs(tmp_path)
    source_root = upstream / "disasm"

    # Every selected consumer label, access, local branch, and handoff is a
    # source-identity guard.  A nearby comment would not match an exact line.
    for spec in _CONTEXTS:
        path = source_root / spec["sourcePath"]
        with (
            _replaced_line(path, spec["entrySourceLine"], "MutatedEntry:"),
            pytest.raises(ValueError, match="source label drift"),
        ):
            _projection(rom, upstream)
        for access in spec["accesses"]:
            with (
                _replaced_line(path, access["sourceLine"], "nop"),
                pytest.raises(ValueError, match="source mnemonic/operand-order drift"),
            ):
                _projection(rom, upstream)
            original_rows = consumption_module._h1_instruction_rows
            with monkeypatch.context() as patch:

                def wrong_h1(
                    text: str,
                    *,
                    row_pc: int = access["romPc"],
                    rows_parser=original_rows,
                ):
                    rows = rows_parser(text)
                    raw, statement = rows[row_pc]
                    rows[row_pc] = (raw, "nop")
                    return rows

                patch.setattr(consumption_module, "_h1_instruction_rows", wrong_h1)
                with pytest.raises(ValueError, match="H1 source join drift"):
                    _projection(rom, upstream)
        for control in spec["controls"]:
            with (
                _replaced_line(path, control["sourceLine"], "nop"),
                pytest.raises(ValueError, match="source mnemonic/operand-order drift"),
            ):
                _projection(rom, upstream)

    # All 18 physical H1/ROM anchors fail independently, including the three
    # PCs shared by an entry role and a lifecycle access role.
    anchors = fixture["sourceContext"]["anchors"]
    assert len(anchors) == len({anchor["romPc"] for anchor in anchors}) == 18
    original_rows = consumption_module._h1_instruction_rows
    for anchor in anchors:
        with monkeypatch.context() as patch:

            def changed_h1(text: str, *, row_pc: int = anchor["romPc"]):
                rows = original_rows(text)
                raw, statement = rows[row_pc]
                rows[row_pc] = (bytes([raw[0] ^ 1]) + raw[1:], statement)
                return rows

            patch.setattr(consumption_module, "_h1_instruction_rows", changed_h1)
            with pytest.raises(ValueError, match="H1/ROM anchor drift"):
                _projection(rom, upstream)

        original_rom = rom.read_bytes()
        changed_rom = bytearray(original_rom)
        changed_rom[anchor["romPc"]] ^= 1
        rom.write_bytes(changed_rom)
        try:
            with monkeypatch.context() as patch:
                patch.setattr(
                    consumption_module,
                    "_ROM_SHA256",
                    hashlib.sha256(changed_rom).hexdigest().upper(),
                )
                with pytest.raises(ValueError, match="H1/ROM anchor drift"):
                    _projection(rom, upstream)
        finally:
            rom.write_bytes(original_rom)

    # An unrelated source-byte addition changes the closed identity through the
    # public verifier rather than being mistaken for a selected instruction.
    source = source_root / "sf2const.asm"
    original = source.read_bytes()
    source.write_bytes(original + b"\n; near-miss consumer guard\n")
    try:
        with monkeypatch.context() as patch:
            patch.setattr(
                consumption_module,
                "_fresh_retained_owners",
                lambda _rom, _upstream: fixture["retainedOwners"],
            )
            with pytest.raises(ValueError, match="fixture drift"):
                verify_map_event_request_consumption_contract(rom, upstream)
    finally:
        source.write_bytes(original)

    retained = load_json(REQUEST_STATE_FIXTURE)
    retained["summary"]["positiveProgramContextCount"] += 1
    monkeypatch.setattr(
        consumption_module,
        "build_map_event_request_state_contract",
        lambda _rom, _upstream: retained,
    )
    with pytest.raises(ValueError, match="retained request-state projection drift"):
        consumption_module._fresh_retained_owners(ROM, UPSTREAM)


def test_request_consumption_index_delta_is_exact_without_object_drift() -> None:
    index = load_json(INDEX)
    validate_json(index, INDEX_SCHEMA, owner="map-event request-consumption index")
    base = json.loads(
        subprocess.run(
            ["git", "show", f"{BASE}:manifests/research-index.json"],
            cwd=ROOT,
            check=True,
            capture_output=True,
            text=True,
            encoding="utf-8",
        ).stdout
    )
    records = {record["id"]: record for record in index["records"]}
    base_records = {record["id"]: record for record in base["records"]}
    assert set(records) == set(base_records)
    changed = {record_id for record_id in records if records[record_id] != base_records[record_id]}
    assert changed == set(EXPECTED_INDEX_DELTA)
    assert len(records) == len(base_records) == 1625
    assert (
        sum(len(record["addresses"]) for record in records.values())
        == sum(len(record["addresses"]) for record in base_records.values()) + 4
    )
    assert sum(len(record.get("designContracts", [])) for record in records.values()) == sum(
        len(record.get("designContracts", [])) for record in base_records.values()
    )

    for record_id, expected in EXPECTED_INDEX_DELTA.items():
        record, previous = records[record_id], base_records[record_id]
        assert record["addresses"] == previous["addresses"] + expected["addresses"]
        assert record["documents"] == previous["documents"] + [DOCUMENT]
        assert record["evidence"] == previous["evidence"] + [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-request-consumption-static-v1.json",
                "fixtureId": ID,
                "verifier": "src/sf2tool/h2/map_event_request_consumption.py",
                "bindings": expected["bindings"],
            }
        ]
    for record_id in set(records) - set(EXPECTED_INDEX_DELTA):
        assert records[record_id] == base_records[record_id]

    bindings = [
        binding
        for record in records.values()
        for evidence in record["evidence"]
        if evidence["fixtureId"] == ID
        for binding in evidence["bindings"]
    ]
    assert len(bindings) == 8
    assert {binding["fixtureField"] for binding in bindings} == {
        binding["fixtureField"]
        for expected in EXPECTED_INDEX_DELTA.values()
        for binding in expected["bindings"]
    }
    broken = deepcopy(index)
    next(
        binding
        for record in broken["records"]
        for evidence in record["evidence"]
        if evidence["fixtureId"] == ID
        for binding in evidence["bindings"]
    )["fixtureField"] = f"sourceContext.{CONSUMER_CONTEXT_FIELD}.fieldMenu.entryAddress"
    with pytest.raises(ValueError, match="schema validation"):
        validate_json(broken, INDEX_SCHEMA, owner="map-event request-consumption index")
    assert verify_index(UPSTREAM) == {
        "Index": "manifests/research-index.json",
        "Records": 1625,
        "Confirmed": 1625,
        "H2Fixtures": 92,
        "H3Fixtures": 94,
        "H3FixtureFiles": 94,
        "AddressBindings": 2976,
        "IndexedCodeFiles": 381,
        "IndexedDataFiles": 1017,
        "H1ListingRecords": 1588,
        "AlternateListingRecords": 37,
        "Z80MusicBankRecords": 37,
        "ResearchDocuments": 54,
        "DesignContracts": 68,
        "UpstreamSourcesChecked": True,
        "H1ListingChecked": True,
        "Status": "PASS",
    }
