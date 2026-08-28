"""Focused tests for the static map-event random-battle contract."""

from __future__ import annotations

import hashlib
from copy import deepcopy
from pathlib import Path
from shutil import copy2

import pytest

import sf2tool.h2.map_event_random_battle_state as random_battle_module
from sf2tool.h2.map_event_combatant_state import canonical_json_bytes
from sf2tool.h2.map_event_random_battle_state import (
    FIXTURE,
    ID,
    SCHEMA,
    _remove_map_event_random_battle_state_later_owner_index_delta,
    build_map_event_random_battle_state_contract,
    normalize_map_event_random_battle_state_later_owner_index,
)
from sf2tool.h2.map_event_tactical_base_quote_state import (
    _remove_map_event_tactical_base_quote_state_later_owner_index_delta,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

ROM = repo_path("local/roms/sf2-us.bin")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
_INDEX = repo_path("manifests/research-index.json")
_DOCUMENT = "docs/research/map-event-random-battle-state.md"
_OWNER_ID = "map.setup.check-random-battle"
_EXPECTED_CHANGED_IDS = {
    _OWNER_ID,
    "map.data.ms-map66-zoneevents",
    "map.data.ms-map67-zoneevents",
    "map.data.ms-map68-zoneevents",
    "map.data.ms-map69-zoneevents",
    "map.data.ms-map70-zoneevents",
    "map.data.ms-map72-zoneevents",
    "tech.interfaces.jump-s02",
    "stats.flags",
    "rng.generate-random-number",
    "map.camera-control.wait-for-view-scroll-end",
    "tech.graphics.flash-white",
}
_ADDRESS_DELTA = (
    (_OWNER_ID, "entry", "rom", "symbol", 0x47856),
    (_OWNER_ID, "completion-flag-gate", "rom", "observation", 0x47860),
    (_OWNER_ID, "request-write", "rom", "observation", 0x478A0),
    ("tech.interfaces.jump-s02", "check-flag", "rom", "observation", 0x8264),
    ("tech.interfaces.jump-s02", "set-flag", "rom", "observation", 0x8268),
    ("stats.flags", "set-flag", "rom", "observation", 0x98C4),
)
_BINDING_DELTA = (
    (_OWNER_ID, "entry", "eventRandomBattleState.functionAddresses.entryAddress"),
    (
        _OWNER_ID,
        "completion-flag-gate",
        "eventRandomBattleState.completionFlagGate.callAddress",
    ),
    (
        _OWNER_ID,
        "request-write",
        "eventRandomBattleState.requestWriteSequence.setFlagCallAddress",
    ),
    (
        "map.data.ms-map66-zoneevents",
        "entry",
        "eventRandomBattleState.callerSites.map66-default-battle3.tableEntryAddress",
    ),
    (
        "map.data.ms-map67-zoneevents",
        "entry",
        "eventRandomBattleState.callerSites.map67-default-battle21.tableEntryAddress",
    ),
    (
        "map.data.ms-map68-zoneevents",
        "entry",
        "eventRandomBattleState.callerSites.map68-default-battle19.tableEntryAddress",
    ),
    (
        "map.data.ms-map69-zoneevents",
        "entry",
        "eventRandomBattleState.callerSites.map69-event0-battle17.tableEntryAddress",
    ),
    (
        "map.data.ms-map70-zoneevents",
        "entry",
        "eventRandomBattleState.callerSites.map70-event0-battle14.tableEntryAddress",
    ),
    (
        "map.data.ms-map72-zoneevents",
        "entry",
        "eventRandomBattleState.callerSites.map72-event0-battle26.tableEntryAddress",
    ),
    (
        "tech.interfaces.jump-s02",
        "check-flag",
        "eventRandomBattleState.serviceEntries.j_CheckFlag.instructionTargetAddress",
    ),
    (
        "tech.interfaces.jump-s02",
        "set-flag",
        "eventRandomBattleState.serviceEntries.j_SetFlag.instructionTargetAddress",
    ),
    (
        "stats.flags",
        "entry",
        "eventRandomBattleState.serviceEntries.j_CheckFlag.effectiveTargetAddress",
    ),
    (
        "stats.flags",
        "set-flag",
        "eventRandomBattleState.serviceEntries.j_SetFlag.effectiveTargetAddress",
    ),
    (
        "rng.generate-random-number",
        "entry",
        "eventRandomBattleState.serviceEntries.GenerateRandomNumber.effectiveTargetAddress",
    ),
    (
        "map.camera-control.wait-for-view-scroll-end",
        "entry",
        "eventRandomBattleState.serviceEntries.WaitForViewScrollEnd.effectiveTargetAddress",
    ),
    (
        "tech.graphics.flash-white",
        "entry",
        "eventRandomBattleState.serviceEntries.ExecuteFlashScreenScript.effectiveTargetAddress",
    ),
)


def _record(index: dict[str, object], record_id: str) -> dict[str, object]:
    return next(row for row in index["records"] if row["id"] == record_id)


def _fixture_evidence(record: dict[str, object]) -> dict[str, object]:
    return next(row for row in record["evidence"] if row.get("fixtureId") == ID)


def _index_totals(index: dict[str, object]) -> dict[str, int]:
    records = index["records"]
    return {
        "records": len(records),
        "addresses": sum(len(row.get("addresses", [])) for row in records),
        "h2Evidence": sum(
            evidence["level"] == "H2" for row in records for evidence in row.get("evidence", [])
        ),
        "bindings": sum(
            len(evidence.get("bindings", []))
            for row in records
            for evidence in row.get("evidence", [])
        ),
        "documents": sum(len(row.get("documents", [])) for row in records),
        "designContracts": sum(len(row.get("designContracts", [])) for row in records),
    }


def _copied_source_surface(tmp_path: Path) -> Path:
    upstream = tmp_path / "SF2DISASM"
    for source_path in random_battle_module._SOURCE_PATHS:
        destination = upstream / "disasm" / source_path
        destination.parent.mkdir(parents=True, exist_ok=True)
        copy2(UPSTREAM / "disasm" / source_path, destination)
    listing = upstream / "build/sf2build-h1.lst"
    listing.parent.mkdir(parents=True, exist_ok=True)
    copy2(UPSTREAM / "build/sf2build-h1.lst", listing)
    return upstream


def test_complete_static_contract_matches_golden() -> None:
    actual = build_map_event_random_battle_state_contract(ROM, UPSTREAM)
    assert actual == load_json(FIXTURE)
    facts = actual["eventRandomBattleState"]
    assert facts["functionAddresses"]["sourceStatementCount"] == 34
    assert facts["functionAddresses"]["h1InstructionRowCount"] == 35
    assert [facts["callerSites"][key]["call"]["address"] for key in facts["callerSites"]] == [
        0x4FAD8,
        0x4FB5C,
        0x4FD74,
        0x4FDB6,
        0x4FE16,
        0x4FE90,
        0x4FF10,
        0x4FF1C,
    ]
    assert (
        facts["callerSites"]["map72-default-north-cliff-battle8"]["continuation"]["address"]
        == 0x4FF16
    )
    assert (
        facts["callerSites"]["map72-default-north-parmecia-battle24"]["continuation"]["address"]
        == 0x4FF22
    )
    assert actual["summary"]["anchorCount"] == 66


def test_mother_corpus_mutation_is_rejected() -> None:
    fixture = load_map_events_fixture()["expected"]
    altered = deepcopy(fixture)
    altered["zoneTargetPrograms"] = altered["zoneTargetPrograms"][:-1]
    with pytest.raises(ValueError, match="mother corpus"):
        build_map_event_random_battle_state_contract(ROM, UPSTREAM, map_events_override=altered)


def test_function_source_body_mutation_is_rejected(tmp_path: Path) -> None:
    upstream = _copied_source_surface(tmp_path)
    function_path = upstream / "disasm" / random_battle_module._FUNCTION_PATH
    original = function_path.read_text(encoding="utf-8")
    function_path.write_text(
        original.replace("moveq   #8,d6", "moveq   #7,d6", 1), encoding="utf-8"
    )
    with pytest.raises(ValueError, match="function source body drift"):
        build_map_event_random_battle_state_contract(ROM, upstream)


def test_schema_rejects_unknown_root_and_nested_fields() -> None:
    fixture = load_json(FIXTURE)
    root_mutation = deepcopy(fixture)
    root_mutation["extra"] = True
    with pytest.raises(ValueError):
        validate_json(root_mutation, SCHEMA, owner="root mutation")
    nested_mutation = deepcopy(fixture)
    nested_mutation["eventRandomBattleState"]["callerSites"]["map66-default-battle3"]["extra"] = (
        True
    )
    with pytest.raises(ValueError):
        validate_json(nested_mutation, SCHEMA, owner="nested mutation")
    missing_mutation = deepcopy(fixture)
    del missing_mutation["eventRandomBattleState"]["randomCadence"]["secondRange"]
    with pytest.raises(ValueError):
        validate_json(missing_mutation, SCHEMA, owner="missing nested mutation")


def test_later_owner_index_delta_is_exact() -> None:
    index = _remove_map_event_tactical_base_quote_state_later_owner_index_delta(load_json(_INDEX))
    predecessor = _remove_map_event_random_battle_state_later_owner_index_delta(index)
    assert hashlib.sha256(canonical_json_bytes(predecessor)).hexdigest().upper() == (
        random_battle_module._PREDECESSOR_INDEX_SHA256
    )
    current_by_id = {row["id"]: row for row in index["records"]}
    predecessor_by_id = {row["id"]: row for row in predecessor["records"]}
    assert set(current_by_id) - set(predecessor_by_id) == {_OWNER_ID}
    assert set(predecessor_by_id) - set(current_by_id) == set()
    assert {
        record_id
        for record_id in current_by_id
        if current_by_id.get(record_id) != predecessor_by_id.get(record_id)
    } == _EXPECTED_CHANGED_IDS
    current_totals = _index_totals(index)
    predecessor_totals = _index_totals(predecessor)
    assert {key: current_totals[key] - predecessor_totals[key] for key in current_totals} == {
        "records": 1,
        "addresses": 6,
        "h2Evidence": 12,
        "bindings": 16,
        "documents": 12,
        "designContracts": 0,
    }
    actual_addresses = {
        (record_id, address["id"], address["space"], address["kind"], address["value"])
        for record_id, record in current_by_id.items()
        for address in record.get("addresses", [])
        if address not in predecessor_by_id.get(record_id, {}).get("addresses", [])
    }
    assert actual_addresses == set(_ADDRESS_DELTA)
    actual_bindings = {
        (record_id, binding["addressId"], binding["fixtureField"])
        for record_id in _EXPECTED_CHANGED_IDS
        for record in [current_by_id[record_id]]
        for binding in _fixture_evidence(record).get("bindings", [])
    }
    assert actual_bindings == set(_BINDING_DELTA)


@pytest.mark.parametrize("record_id,address_id,space,kind,value", _ADDRESS_DELTA)
@pytest.mark.parametrize("mutation", ("missing", "extra", "wrong"))
def test_later_owner_normalizer_rejects_each_address_delta_mutation(
    record_id: str,
    address_id: str,
    space: str,
    kind: str,
    value: int,
    mutation: str,
) -> None:
    altered = deepcopy(
        _remove_map_event_tactical_base_quote_state_later_owner_index_delta(load_json(_INDEX))
    )
    record = _record(altered, record_id)
    address = next(
        row
        for row in record["addresses"]
        if (row["id"], row["space"], row["kind"], row["value"]) == (address_id, space, kind, value)
    )
    if mutation == "missing":
        record["addresses"].remove(address)
    elif mutation == "extra":
        record["addresses"].append(
            {"id": "unexpected", "space": "rom", "kind": "observation", "value": 0}
        )
    else:
        address["value"] += 2
    with pytest.raises(ValueError):
        normalize_map_event_random_battle_state_later_owner_index(altered)


@pytest.mark.parametrize("record_id,address_id,fixture_field", _BINDING_DELTA)
@pytest.mark.parametrize("mutation", ("missing", "extra", "wrong"))
def test_later_owner_normalizer_rejects_each_binding_delta_mutation(
    record_id: str,
    address_id: str,
    fixture_field: str,
    mutation: str,
) -> None:
    altered = deepcopy(
        _remove_map_event_tactical_base_quote_state_later_owner_index_delta(load_json(_INDEX))
    )
    bindings = _fixture_evidence(_record(altered, record_id))["bindings"]
    binding = next(
        row
        for row in bindings
        if (row["addressId"], row["fixtureField"]) == (address_id, fixture_field)
    )
    if mutation == "missing":
        bindings.remove(binding)
    elif mutation == "extra":
        bindings.append(
            {
                "addressId": "unexpected",
                "fixtureField": "eventRandomBattleState.unexpected",
            }
        )
    else:
        binding["fixtureField"] = "eventRandomBattleState.unexpected"
    with pytest.raises(ValueError):
        normalize_map_event_random_battle_state_later_owner_index(altered)


def test_later_owner_normalizer_rejects_missing_exact_delta() -> None:
    index = _remove_map_event_tactical_base_quote_state_later_owner_index_delta(load_json(_INDEX))
    altered = deepcopy(index)
    record = next(row for row in altered["records"] if row["id"] == "stats.flags")
    record["documents"].remove(_DOCUMENT)
    with pytest.raises(ValueError, match="record fields drift"):
        normalize_map_event_random_battle_state_later_owner_index(altered)
    owner_mutation = deepcopy(index)
    owner = next(row for row in owner_mutation["records"] if row["id"] == _OWNER_ID)
    owner["addresses"][0]["value"] += 2
    with pytest.raises(ValueError, match="new index record drift"):
        normalize_map_event_random_battle_state_later_owner_index(owner_mutation)


def test_public_ids_are_stable() -> None:
    assert ID == "sf2-map-event-random-battle-state-static-v1"
    assert Path(FIXTURE).name == "map-event-random-battle-state-static-v1.json"
