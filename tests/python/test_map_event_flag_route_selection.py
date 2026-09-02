"""Focused static tests for map-event flag route-selection topology."""

from __future__ import annotations

import re
from copy import deepcopy
from pathlib import Path
from shutil import copy2

import pytest

import sf2tool.h2.map_event_flag_route_selection as route_module
from sf2tool.h2.map_event_cross_program_flag_state import _sha, canonical_json_bytes
from sf2tool.h2.map_event_flag_route_selection import (
    _EXPECTED_TOPOLOGY,
    _RETAINED_OWNER_EXPECTED,
    FIXTURE,
    ID,
    RESEARCH_INDEX,
    SCHEMA,
    _remove_map_event_flag_route_selection_later_owner_index_delta,
    _validate_order,
    build_map_event_flag_route_selection_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import (
    _normalize_current_index_to_owner_state,
    normalize_current_index_to_owner_predecessor,
)

ROM = repo_path("local/roms/sf2-us.bin")
UPSTREAM = repo_path("local/upstream/SF2DISASM")


def _source_surface(tmp_path: Path, *, copy_rom: bool = False) -> tuple[Path, Path]:
    fixture = load_json(FIXTURE)
    upstream = tmp_path / "SF2DISASM"
    for identity in fixture["sourceContext"]["sourceIdentities"]:
        source = UPSTREAM / "disasm" / identity["path"]
        target = upstream / "disasm" / identity["path"]
        target.parent.mkdir(parents=True, exist_ok=True)
        copy2(source, target)
    listing = upstream / "build/sf2build-h1.lst"
    listing.parent.mkdir(parents=True, exist_ok=True)
    copy2(UPSTREAM / "build/sf2build-h1.lst", listing)
    if not copy_rom:
        return upstream, ROM
    rom = tmp_path / "sf2-us.bin"
    copy2(ROM, rom)
    return upstream, rom


def _flip_h1_byte(listing: Path, address: int) -> None:
    lines = listing.read_text(encoding="utf-8").splitlines(keepends=True)
    pattern = re.compile(rf"^({address:08X}\s+)([0-9A-Fa-f])")
    for index, line in enumerate(lines):
        match = pattern.match(line)
        if match is None:
            continue
        byte_index = len(match.group(1))
        original = line[byte_index]
        replacement = "0" if original != "0" else "1"
        lines[index] = line[:byte_index] + replacement + line[byte_index + 1 :]
        listing.write_text("".join(lines), encoding="utf-8")
        return
    raise AssertionError(f"missing H1 data row at {address:#x}")


def test_complete_projection_has_closed_public_denominators() -> None:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map-event flag route selection fixture")
    _validate_order(fixture)
    assert build_map_event_flag_route_selection_contract(ROM, UPSTREAM) == fixture
    facts = fixture["flagRouteSelection"]
    assert fixture["summary"] == {
        "sourceIdentityCount": 192,
        "crossProgramCandidateCount": 720,
        "matchingEventRecordCount": 284,
        "matchingEventTableCount": 96,
        "selectedPointerTableCount": 91,
        "physicalCategoryPointerEntryCount": 139,
        "selectedSelectorRowCount": 94,
        "routeMapCount": 51,
        "selectorWriterRelationCount": 15,
        "selectorWriterFlagCount": 11,
        "selectorWriterProgramCount": 11,
        "physicalAnchorPcCount": 1321,
        "physicalAnchorByteCount": 4862,
    }
    assert facts["retainedIdentities"] == {
        "entityEvents": 292378,
        "zoneEvents": 292122,
        "itemEvents": 292230,
        "trapEntryAddress": 5888,
        "selectorEntryAddress": 292766,
        "mapSetupsEntryAddress": 325346,
    }
    assert {
        row["classification"]: row["candidateCount"]
        for row in facts["topologyCategoryTotals"]["classificationTotals"]
    } == _EXPECTED_TOPOLOGY
    assert len(facts["programRouteContexts"]) == 195
    assert len(facts["classifiedCandidates"]) == 720
    assert len(facts["selectorWriterRelations"]) == 15
    assert (
        len(
            {
                (row["flagNumber"], row["writerRouteContextOrder"])
                for row in facts["selectorWriterRelations"]
            }
        )
        == 11
    )
    assert len(facts["physicalCoverage"]["physicalAnchors"]) == 1321


def test_new_anchor_h1_bytes_and_source_rom_mutations_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    upstream, _rom = _source_surface(tmp_path)
    cross, routing, map_setup, owners = route_module._load_owners(ROM, UPSTREAM)
    monkeypatch.setattr(
        route_module,
        "_load_owners",
        lambda _rom_path, _upstream_path: (cross, routing, map_setup, owners),
    )
    fixture = load_json(FIXTURE)
    anchors = fixture["flagRouteSelection"]["physicalCoverage"]["physicalAnchors"]
    listing = upstream / "build/sf2build-h1.lst"
    original_listing = listing.read_text(encoding="utf-8")
    for cohort, error in (
        ("eventRecord", "event-record H1 byte drift"),
        ("categoryPointer", "category-pointer H1 byte drift"),
        ("routeSelector", "selector H1 byte drift"),
    ):
        address = next(row["address"] for row in anchors if row["cohort"] == cohort)
        _flip_h1_byte(listing, address)
        with pytest.raises(ValueError, match=error):
            build_map_event_flag_route_selection_contract(ROM, upstream)
        listing.write_text(original_listing, encoding="utf-8")

    map_setups = upstream / "disasm/data/maps/mapsetups.asm"
    map_setups.write_text(
        map_setups.read_text(encoding="utf-8").replace("msMap 4", "msFlag 4", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="selector map drift"):
        build_map_event_flag_route_selection_contract(ROM, upstream)

    rom_upstream, mutated_rom = _source_surface(tmp_path / "rom", copy_rom=True)
    payload = bytearray(mutated_rom.read_bytes())
    payload[next(row["address"] for row in anchors if row["cohort"] == "eventRecord")] ^= 1
    mutated_rom.write_bytes(payload)
    with pytest.raises(ValueError, match="ROM identity drift"):
        build_map_event_flag_route_selection_contract(mutated_rom, rom_upstream)


def test_order_and_candidate_topology_mutations_fail_closed() -> None:
    fixture = load_json(FIXTURE)
    missing = deepcopy(fixture)
    missing["flagRouteSelection"]["classifiedCandidates"].pop()
    with pytest.raises(ValueError, match="candidate order"):
        _validate_order(missing)

    wrong_total = deepcopy(fixture)
    wrong_total["flagRouteSelection"]["topologyCategoryTotals"]["classificationTotals"][0][
        "candidateCount"
    ] += 1
    with pytest.raises(ValueError, match="classification total"):
        _validate_order(wrong_total)

    wrong_unknown = deepcopy(fixture)
    wrong_unknown["unknowns"]["actualSelectedEventRecord"] = "Confirmed"
    with pytest.raises(ValueError, match="Unknown queue"):
        _validate_order(wrong_unknown)


def test_schema_is_recursively_closed_and_public_safe() -> None:
    fixture = load_json(FIXTURE)
    for mutate in (
        lambda value: value.__setitem__("runtime", {}),
        lambda value: value["sourceContext"]["sourceIdentities"][0].__setitem__(
            "privatePath", "local/roms/sf2-us.bin"
        ),
        lambda value: value["flagRouteSelection"]["programRouteContexts"][0].__setitem__(
            "extra", True
        ),
        lambda value: value["flagRouteSelection"]["physicalCoverage"]["physicalAnchors"][
            0
        ].__setitem__("rawRomBytes", "private"),
    ):
        altered = deepcopy(fixture)
        mutate(altered)
        with pytest.raises(ValueError):
            validate_json(altered, SCHEMA, owner="map-event flag route selection closure mutation")
    assert "local/" not in FIXTURE.read_text(encoding="utf-8")
    assert fixture["id"] == ID


def test_retained_owner_receipts_and_index_delta_are_exact(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    _cross, _routing, _map_setup, owners = route_module._load_owners(ROM, UPSTREAM)
    assert owners == _RETAINED_OWNER_EXPECTED
    altered_receipts = deepcopy(_RETAINED_OWNER_EXPECTED)
    altered_receipts["mapSetup"]["semanticSha256"] = "0" * 64
    monkeypatch.setattr(route_module, "_RETAINED_OWNER_EXPECTED", altered_receipts)
    with pytest.raises(ValueError, match="retained owner identity/hash"):
        route_module._load_owners(ROM, UPSTREAM)

    raw_index = load_json(RESEARCH_INDEX)
    current = _normalize_current_index_to_owner_state(raw_index, owner_id=ID)
    predecessor = _remove_map_event_flag_route_selection_later_owner_index_delta(current)
    assert _sha(canonical_json_bytes(predecessor)) == (
        "4F729D50C06D63484565A0DABF15A98F3B092896C7FAF9455DAB884A537DD3FE"
    )
    current_records = {row["id"]: row for row in current["records"]}
    predecessor_records = {row["id"]: row for row in predecessor["records"]}
    changed = {
        record_id
        for record_id, record in current_records.items()
        if record != predecessor_records[record_id]
    }
    assert changed == {
        "map.setup.entity-event",
        "map.setup.zone-event",
        "map.setup.item-event",
        "tech.interrupts.trap-flags",
        "map.setup.selector",
        "map.data.mapsetups",
    }
    assert sum(len(row["addresses"]) for row in current["records"]) == sum(
        len(row["addresses"]) for row in predecessor["records"]
    )
    assert sum(len(row.get("designContracts", [])) for row in current["records"]) == sum(
        len(row.get("designContracts", [])) for row in predecessor["records"]
    )
    assert (
        sum(len(row["evidence"]) for row in current["records"])
        - sum(len(row["evidence"]) for row in predecessor["records"])
        == 6
    )
    assert (
        sum(len(evidence["bindings"]) for row in current["records"] for evidence in row["evidence"])
        - sum(
            len(evidence["bindings"])
            for row in predecessor["records"]
            for evidence in row["evidence"]
        )
        == 6
    )
    assert (
        sum(len(row["documents"]) for row in current["records"])
        - sum(len(row["documents"]) for row in predecessor["records"])
        == 6
    )
    assert normalize_current_index_to_owner_predecessor(raw_index, owner_id=ID) == predecessor

    for mutate in (
        lambda value: next(row for row in value["records"] if row["id"] == "map.setup.selector")[
            "documents"
        ].append("docs/research/map-event-flag-route-selection.md"),
        lambda value: next(
            evidence
            for row in value["records"]
            for evidence in row["evidence"]
            if evidence.get("fixtureId") == ID
        )["bindings"].append(
            {
                "addressId": "entry",
                "fixtureField": "flagRouteSelection.retainedIdentities.selectorEntryAddress",
            }
        ),
        lambda value: value["records"].append(deepcopy(value["records"][0])),
    ):
        altered = deepcopy(current)
        mutate(altered)
        with pytest.raises(ValueError, match="index"):
            _remove_map_event_flag_route_selection_later_owner_index_delta(altered)
