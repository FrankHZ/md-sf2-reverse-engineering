"""Focused adversarial tests for map-event interaction-state static H2."""

from __future__ import annotations

import hashlib
from collections.abc import Iterator
from contextlib import contextmanager
from copy import deepcopy
from pathlib import Path
from shutil import copy2

import pytest

import sf2tool.h2.map_event_interaction_state as interaction_module
from sf2tool.h2.map_event_interaction_state import (
    _FUNCTION_SPECS,
    _SEAM_SPECS,
    FIXTURE,
    ID,
    SCHEMA,
    _interaction_projection,
    build_map_event_interaction_state_contract,
    normalize_interaction_state_later_owner_index,
    verify_map_event_interaction_state_contract,
)
from sf2tool.h2.map_event_item_transactions import (
    _remove_map_event_item_transactions_index_delta,
)
from sf2tool.jsonio import load_json, validate_json

ROOT = Path(__file__).resolve().parents[2]
ROM = ROOT / "local/roms/sf2-us.bin"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _mutable_inputs(tmp_path: Path) -> tuple[Path, Path]:
    """Copy only the 12-source/H1/ROM surface needed for mutation guards."""
    upstream = tmp_path / "SF2DISASM"
    for source_path in interaction_module._SOURCE_PATHS:
        destination = upstream / "disasm" / source_path
        destination.parent.mkdir(parents=True, exist_ok=True)
        copy2(UPSTREAM / "disasm" / source_path, destination)
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


def test_interaction_state_projection_is_closed_and_exact() -> None:
    fixture = _fixture()
    validate_json(fixture, SCHEMA, owner="map-event interaction-state fixture")
    rebuilt = build_map_event_interaction_state_contract(ROM, UPSTREAM)
    validate_json(rebuilt, SCHEMA, owner="map-event interaction-state rebuilt contract")
    assert rebuilt == fixture
    assert set(fixture) == {
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "sourceContext",
        "retainedOwners",
        "interactionState",
        "unknowns",
        "summary",
    }
    assert fixture["id"] == ID
    state = fixture["interactionState"]
    assert set(state) == {
        "symbolDefinitions",
        "functionRanges",
        "dispatchContexts",
        "jumpInterfaces",
        "producerWrites",
        "consumerReads",
        "predicateJoins",
        "multiplexedRoles",
        "anchorOrder",
        "digests",
    }
    assert fixture["summary"] == {
        "sourceIdentityCount": 12,
        "functionRangeCount": 7,
        "seamGroupCount": 7,
        "sourceOperationCount": 230,
        "h1RomByteCount": 792,
        "symbolDefinitionCount": 2,
        "producerWriteCount": 2,
        "consumerReadCount": 8,
        "predicateJoinCount": 6,
        "jumpInterfaceCount": 4,
    }
    assert [row["sourceOperationCount"] for row in state["functionRanges"]] == [
        57,
        15,
        9,
        39,
        59,
        10,
        6,
    ]
    assert [row["h1RomByteLength"] for row in state["functionRanges"]] == [
        170,
        44,
        36,
        148,
        194,
        36,
        24,
    ]
    assert sum(len(group["reads"]) for group in state["consumerReads"].values()) == 8
    assert len(state["predicateJoins"]) == 6


@pytest.mark.parametrize(
    "mutator",
    (
        lambda value: value.__setitem__("sourceProse", "private"),
        lambda value: value["sourceContext"].__setitem__("rawH1Bytes", "private"),
        lambda value: value["interactionState"].__setitem__("entityAcquisition", {}),
        lambda value: value["interactionState"]["functionRanges"][0].__setitem__(
            "rawRom", "private"
        ),
        lambda value: value["interactionState"]["consumerReads"]["map6"]["reads"].append({}),
        lambda value: value["unknowns"].__setitem__("entityFacingValue", "Confirmed"),
    ),
)
def test_interaction_state_schema_rejects_private_payload_and_nested_shape_drift(
    mutator: object,
) -> None:
    broken = deepcopy(_fixture())
    mutator(broken)
    with pytest.raises(ValueError, match="schema validation"):
        validate_json(broken, SCHEMA, owner="map-event interaction-state fixture")


def test_source_h1_rom_range_and_operation_guards_fail_before_fixture(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    upstream, rom = _mutable_inputs(tmp_path)
    source_root = upstream / "disasm"

    for spec in (*_FUNCTION_SPECS, *_SEAM_SPECS):
        source_path = source_root / spec["sourcePath"]
        source_lines = source_path.read_text(encoding="utf-8").splitlines()
        first_line = next(
            line_number
            for line_number in spec["sourceLines"]
            if interaction_module._source_statement(source_lines[line_number - 1]) is not None
        )
        with (
            _replaced_line(source_path, first_line, "nop"),
            pytest.raises(ValueError, match="source/H1 operation-order drift"),
        ):
            _interaction_projection(rom, upstream)

    original_parser = interaction_module._parse_h1
    with monkeypatch.context() as patch:

        def changed_h1(text: str):
            rows, labels = original_parser(text)
            raw, statement = rows[0x4761E]
            rows[0x4761E] = (raw, "move.b d1,((EVENT_RELATIVE_POSITION-$1000000)).w")
            return rows, labels

        patch.setattr(interaction_module, "_parse_h1", changed_h1)
        with pytest.raises(ValueError, match="source/H1 operation-order drift"):
            _interaction_projection(rom, upstream)

    original_rom = rom.read_bytes()
    changed_rom = bytearray(original_rom)
    changed_rom[0x549D6] ^= 1
    rom.write_bytes(changed_rom)
    try:
        with monkeypatch.context() as patch:
            patch.setattr(
                interaction_module, "_ROM_SHA256", hashlib.sha256(changed_rom).hexdigest().upper()
            )
            with pytest.raises(ValueError, match="H1/ROM byte drift"):
                _interaction_projection(rom, upstream)
    finally:
        rom.write_bytes(original_rom)

    with monkeypatch.context() as patch:
        patch.setattr(interaction_module, "_FUNCTION_SPECS", _FUNCTION_SPECS[:-1])
        with pytest.raises(ValueError, match="function operation denominator drift"):
            _interaction_projection(rom, upstream)
    with monkeypatch.context() as patch:
        patch.setattr(interaction_module, "_SEAM_SPECS", (*_SEAM_SPECS, _SEAM_SPECS[-1]))
        with pytest.raises(ValueError, match="seam operation denominator drift"):
            _interaction_projection(rom, upstream)
    with monkeypatch.context() as patch:
        patch.setattr(
            interaction_module,
            "_FUNCTION_SPECS",
            (_FUNCTION_SPECS[1], _FUNCTION_SPECS[0], *_FUNCTION_SPECS[2:]),
        )
        with pytest.raises(ValueError, match="function operation denominator drift"):
            _interaction_projection(rom, upstream)


@pytest.mark.parametrize(
    ("path", "line", "replacement", "error"),
    (
        (
            "code/common/scripting/map/mapsetupsfunctions_1.asm",
            95,
            "                move.b  d1,((EVENT_RELATIVE_POSITION-$1000000)).w",
            "source/H1 operation-order drift",
        ),
        (
            "code/common/scripting/map/mapsetupsfunctions_1.asm",
            165,
            "                move.w  d2,((EVENT_RELATIVE_POSITION-$1000000)).w",
            "source/H1 operation-order drift",
        ),
        (
            "code/common/tech/jumpinterfaces/s07_jumpinterface.asm",
            340,
            "                jmp     RunMapSetupItemEvent(pc)",
            "source/H1 operation-order drift",
        ),
        (
            "code/gameflow/exploration/explorationvints.asm",
            86,
            "                bsr.w   GetActivatedEntity",
            "source/H1 operation-order drift",
        ),
        (
            "code/common/menus/main/mainactions.asm",
            253,
            "                jsr     j_RunMapSetupEntityEvent",
            "source/H1 operation-order drift",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_2.asm",
            394,
            "                move.w  ((MAP_EVENT_PARAM_1-$1000000)).w,d2",
            "source/H1 operation-order drift",
        ),
        (
            "data/maps/entries/map06/mapsetups/s2_entityevents_701.asm",
            36,
            "                cmpi.b  #2,((EVENT_RELATIVE_POSITION-$1000000)).w",
            "source/H1 operation-order drift",
        ),
        (
            "data/maps/entries/map06/mapsetups/s2_entityevents_701.asm",
            37,
            "                beq.s   byte_549EC",
            "source/H1 operation-order drift",
        ),
        (
            "data/maps/entries/map09/mapsetups/s2_entityevents.asm",
            106,
            "                addq.w  #1,d1",
            "source/H1 operation-order drift",
        ),
        (
            "data/maps/entries/map09/mapsetups/s2_entityevents.asm",
            107,
            "                andi.w  #DIRECTION_MASK,d2",
            "source/H1 operation-order drift",
        ),
        (
            "data/maps/entries/map28/mapsetups/s3_zoneevents.asm",
            16,
            "                nop",
            "source/H1 operation-order drift",
        ),
        (
            "data/maps/entries/map28/mapsetups/s3_zoneevents.asm",
            18,
            "                beq.s   byte_5F38E",
            "source/H1 operation-order drift",
        ),
    ),
)
def test_writer_reader_alias_dispatch_and_predicate_near_misses_fail_closed(
    tmp_path: Path, path: str, line: int, replacement: str, error: str
) -> None:
    upstream, rom = _mutable_inputs(tmp_path)
    with (
        _replaced_line(upstream / "disasm" / path, line, replacement),
        pytest.raises(ValueError, match=error),
    ):
        _interaction_projection(rom, upstream)


def test_static_roles_preserve_two_event_relative_position_inputs() -> None:
    fixture = _fixture()
    roles = fixture["interactionState"]["multiplexedRoles"]
    assert roles[:2] == [
        {
            "id": "event-relative-position/entity-event-d2",
            "symbol": "EVENT_RELATIVE_POSITION",
            "producerWriteId": "entity-event-d2",
            "dispatchContextId": "entityDispatch",
            "inputRegister": "d2",
            "inputRole": "player-facing",
        },
        {
            "id": "event-relative-position/item-event-d2",
            "symbol": "EVENT_RELATIVE_POSITION",
            "producerWriteId": "item-event-d2",
            "dispatchContextId": "itemInvocation",
            "inputRegister": "d2",
            "inputRole": "player-y-tile",
        },
    ]
    assert roles[2]["boundedWriterCount"] == 0


def test_retained_owner_guard_fails_before_output_construction(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    import sf2tool.h2.map_events as map_events

    monkeypatch.setattr(
        map_events, "build_map_events_contract", lambda _rom, _upstream: {"drift": True}
    )
    with pytest.raises(ValueError, match="retained owner drift: mapEvents"):
        interaction_module._fresh_retained_owners(ROM, UPSTREAM)


def test_map_setup_retained_projection_uses_its_wrapper_and_canonical_guard(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    import sf2tool.h2.map_setup as map_setup

    accepted = map_setup.build_map_setup_contract(ROM, UPSTREAM)
    retained = interaction_module._fresh_map_setup_retained_owner(ROM, UPSTREAM)
    assert retained["fixtureId"] == "sf2-map-setup-static-v1"

    projected_drift = deepcopy(accepted)
    projected_drift["summary"]["mapRowCount"] += 1
    monkeypatch.setattr(
        map_setup,
        "build_map_setup_contract",
        lambda _rom, _upstream: projected_drift,
    )
    with pytest.raises(ValueError, match="retained owner drift: mapSetup summary"):
        interaction_module._fresh_map_setup_retained_owner(ROM, UPSTREAM)

    canonical_drift = deepcopy(accepted)
    canonical_drift["mapOrder"][0] = 32
    monkeypatch.setattr(
        map_setup,
        "build_map_setup_contract",
        lambda _rom, _upstream: canonical_drift,
    )
    with pytest.raises(ValueError, match="retained owner drift: mapSetup canonical output"):
        interaction_module._fresh_map_setup_retained_owner(ROM, UPSTREAM)


@pytest.mark.parametrize(
    ("owner", "error"),
    (
        ("gameflowCore", "retained owner drift: gameflowCore startupFacts"),
        ("commonMenus", "common menus model drift"),
        ("battleFunctions", "retained owner drift: battleFunctions indexedRecordIds"),
        ("techInterfaces", "retained owner drift: techInterfaces indexedRecordIds"),
    ),
)
def test_projection_wrappers_reject_a_mutated_projected_field_before_output_construction(
    owner: str, error: str, monkeypatch: pytest.MonkeyPatch
) -> None:
    monkeypatch.setattr(interaction_module, "validate_json", lambda *_args, **_kwargs: None)

    if owner == "gameflowCore":
        import sf2tool.h2.gameflow as gameflow

        fixture = load_json(gameflow.FIXTURE)
        manifest = load_json(gameflow.MANIFEST)
        output = {
            "upstream": {"commit": fixture["upstreamCommit"]},
            "summary": manifest["summary"],
            "representativeAddresses": fixture["function"],
            **fixture["expected"],
        }
        output["startupFacts"] = {"drift": True}
        monkeypatch.setattr(gameflow, "build_gameflow_inventory", lambda _upstream: output)
        helper = interaction_module._fresh_gameflow_core_retained_owner
    elif owner == "commonMenus":
        import sf2tool.h2.menus as menus

        fixture = load_json(menus.FIXTURE)
        manifest = load_json(menus.MANIFEST)
        output = {
            "upstream": {"commit": fixture["upstreamCommit"]},
            "summary": manifest["summary"],
            "representativeAddresses": fixture["function"],
            **fixture["expected"],
        }
        output["menuFacts"] = {"drift": True}
        monkeypatch.setattr(menus, "build_menu_inventory", lambda _upstream: output)
        helper = interaction_module._fresh_common_menus_retained_owner
    elif owner == "battleFunctions":
        import sf2tool.h2.battle_functions as battle_functions

        fixture = load_json(battle_functions.FIXTURE)
        manifest = load_json(battle_functions.MANIFEST)
        output = {
            "function": fixture["function"],
            "summary": manifest["summary"],
            **fixture["expected"],
        }
        output["indexedRecordIds"] = ["drift"]
        monkeypatch.setattr(
            battle_functions,
            "build_battle_functions_inventory",
            lambda _upstream: output,
        )
        monkeypatch.setattr(battle_functions, "_verify_indexed_record_join", lambda *_args: None)
        monkeypatch.setattr(battle_functions, "_verify_fixture_provenance", lambda *_args: None)
        helper = interaction_module._fresh_battle_functions_retained_owner
    else:
        import sf2tool.h2.interfaces as interfaces

        fixture = load_json(interfaces.FIXTURE)
        manifest = load_json(interfaces.MANIFEST)
        output = {
            "representativeAddresses": fixture["function"],
            "summary": manifest["summary"],
            **fixture["expected"],
        }
        output["indexedRecordIds"] = ["drift"]
        monkeypatch.setattr(interfaces, "build_interface_inventory", lambda _upstream: output)
        monkeypatch.setattr(interfaces, "_verify_indexed_record_join", lambda _output: None)
        monkeypatch.setattr(interfaces, "_verify_fixture_provenance", lambda *_args: None)
        helper = interaction_module._fresh_tech_interfaces_retained_owner

    with pytest.raises(ValueError, match=error):
        helper(UPSTREAM)


def test_verifier_writes_only_after_source_h1_rom_and_fixture_checks(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    fixture = _fixture()
    output = tmp_path / "result.json"
    monkeypatch.setattr(
        interaction_module,
        "_fresh_retained_owners",
        lambda _rom, _upstream: fixture["retainedOwners"],
    )
    with pytest.raises(ValueError, match="ROM identity drift"), monkeypatch.context() as patch:
        patch.setattr(interaction_module, "_ROM_SHA256", "0" * 64)
        verify_map_event_interaction_state_contract(ROM, UPSTREAM, output_path=output)
    assert not output.exists()


def test_index_binds_the_actual_field_menu_item_call_not_legacy_observation() -> None:
    index = load_json(ROOT / "manifests/research-index.json")
    record = next(item for item in index["records"] if item["id"] == "menus.field-main")
    addresses = {item["id"]: item["value"] for item in record["addresses"]}
    assert addresses["run-map-setup-item-event-call"] == 0x2152A
    assert addresses["field-item-use-call"] == 136572
    evidence = next(
        item
        for item in record["evidence"]
        if item["fixtureId"] == "sf2-map-event-interaction-state-static-v1"
    )
    assert evidence["bindings"] == [
        {
            "addressId": "run-map-setup-item-event-call",
            "fixtureField": (
                "interactionState.dispatchContexts.itemInvocation.runMapSetupItemEventCallAddress"
            ),
        }
    ]


def test_later_owner_normalizer_reconstructs_the_exact_closed_index_delta() -> None:
    index = load_json(ROOT / "manifests/research-index.json")
    prior_index = _remove_map_event_item_transactions_index_delta(index)
    normalized = normalize_interaction_state_later_owner_index(prior_index)

    candidate_records = {
        record["id"]: record
        for record in prior_index["records"]
        if any(evidence["fixtureId"] == ID for evidence in record["evidence"])
    }
    normalized_records = {record["id"]: record for record in normalized["records"]}
    assert set(candidate_records) == set(interaction_module._INDEX_DELTA)
    assert len(candidate_records) == 13
    assert (
        sum(len(addresses) for addresses, _bindings in interaction_module._INDEX_DELTA.values())
        == 9
    )
    assert (
        sum(len(bindings) for _addresses, bindings in interaction_module._INDEX_DELTA.values())
        == 17
    )
    assert index != normalized
    assert {record["id"] for record in index["records"]} == set(normalized_records)
    assert (
        sum(
            len(record["addresses"]) - len(normalized_records[record_id]["addresses"])
            for record_id, record in candidate_records.items()
        )
        == 9
    )
    assert (
        sum(
            len(record["evidence"]) - len(normalized_records[record_id]["evidence"])
            for record_id, record in candidate_records.items()
        )
        == 13
    )
    assert (
        sum(
            len(record["documents"]) - len(normalized_records[record_id]["documents"])
            for record_id, record in candidate_records.items()
        )
        == 13
    )
    assert all(
        record.get("designContracts", [])
        == normalized_records[record_id].get("designContracts", [])
        for record_id, record in candidate_records.items()
    )
    assert all(
        evidence["fixtureId"] != ID
        for record in normalized["records"]
        for evidence in record["evidence"]
    )


def test_map28_table_entry_cannot_substitute_for_wait_predicate_seam_start() -> None:
    state, shared = _interaction_projection(ROM, UPSTREAM)
    seam = next(
        item
        for item in shared["sourceContext"]["seamRanges"]
        if item["id"] == "map28-facing-wait-predicate"
    )
    assert seam["startAddress"] == 0x5F37A
    assert state["consumerReads"]["map28"]["tableEntryAddress"] == 0x5F36C
    assert seam["startAddress"] != state["consumerReads"]["map28"]["tableEntryAddress"]
