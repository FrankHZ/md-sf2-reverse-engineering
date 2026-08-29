"""Focused adversarial tests for static map-event item transactions."""

from __future__ import annotations

import json
import runpy
from collections.abc import Iterator
from contextlib import contextmanager
from copy import deepcopy
from pathlib import Path
from shutil import copy2

import pytest

import sf2tool.h2.map_event_item_transactions as transactions_module
from sf2tool.h2.map_event_combatant_state import (
    _remove_map_event_combatant_state_later_owner_index_delta,
)
from sf2tool.h2.map_event_cross_program_flag_state import (
    _remove_map_event_cross_program_flag_state_later_owner_index_delta,
)
from sf2tool.h2.map_event_flag_lifecycle_state import (
    _remove_map_event_flag_lifecycle_state_later_owner_index_delta,
)
from sf2tool.h2.map_event_flag_route_selection import (
    _remove_map_event_flag_route_selection_later_owner_index_delta,
)
from sf2tool.h2.map_event_interaction_state import (
    normalize_interaction_state_later_owner_index,
)
from sf2tool.h2.map_event_item_transactions import (
    FIXTURE,
    ID,
    SCHEMA,
    _remove_map_event_item_transactions_index_delta,
    build_map_event_item_transactions_contract,
    normalize_map_event_item_transactions_later_owner_index,
    verify_map_event_item_transactions_contract,
)
from sf2tool.h2.map_event_random_battle_state import (
    _remove_map_event_random_battle_state_later_owner_index_delta,
)
from sf2tool.h2.map_event_scripted_transition_state import (
    _remove_map_event_scripted_transition_state_later_owner_index_delta,
)
from sf2tool.h2.map_event_tactical_base_quote_state import (
    _remove_map_event_tactical_base_quote_state_later_owner_index_delta,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json


def _remove_cross_program_flag_lifecycle_deltas(index):
    return _remove_map_event_flag_lifecycle_state_later_owner_index_delta(
        _remove_map_event_cross_program_flag_state_later_owner_index_delta(
            _remove_map_event_flag_route_selection_later_owner_index_delta(index)
        )
    )


ROOT = Path(__file__).resolve().parents[2]
ROM = ROOT / "local/roms/sf2-us.bin"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"
INDEX = ROOT / "manifests/research-index.json"


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _build(rom: Path = ROM, upstream: Path = UPSTREAM) -> dict[str, object]:
    return build_map_event_item_transactions_contract(
        rom,
        upstream,
        map_events_override=load_map_events_fixture()["expected"],
    )


def _mutable_inputs(
    tmp_path: Path, *, include_listing: bool = True, include_rom: bool = True
) -> tuple[Path, Path]:
    """Copy exactly the guarded 16-source/H1/ROM surface for adversarial mutation."""
    upstream = tmp_path / "SF2DISASM"
    for source_path in transactions_module._SOURCE_PATHS:
        destination = upstream / "disasm" / source_path
        destination.parent.mkdir(parents=True, exist_ok=True)
        copy2(UPSTREAM / "disasm" / source_path, destination)
    listing = upstream / "build/sf2build-h1.lst"
    listing.parent.mkdir(parents=True, exist_ok=True)
    if include_listing:
        copy2(UPSTREAM / "build/sf2build-h1.lst", listing)
    else:
        listing.write_text("", encoding="utf-8")
    rom = tmp_path / "sf2-us.bin"
    if include_rom:
        copy2(ROM, rom)
    else:
        rom.write_bytes(b"")
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


def _operation_for_control_kind(kind: str) -> tuple[str, int]:
    fixture = _fixture()
    transactions = fixture["eventItemTransactions"]
    for context in transactions["programContexts"].values():
        for operation in context["operations"]:
            if operation["controlFlowKind"] == kind:
                source_path = transactions["sourceFiles"][context["sourceFileId"]]["sourcePath"]
                return source_path, operation["sourceLine"]
    raise AssertionError(f"missing control-flow kind: {kind}")


def _operation_source(address: int) -> tuple[str, int]:
    transactions = _fixture()["eventItemTransactions"]
    for context in transactions["programContexts"].values():
        for operation in context["operations"]:
            if operation["address"] == address:
                return (
                    transactions["sourceFiles"][context["sourceFileId"]]["sourcePath"],
                    operation["sourceLine"],
                )
    raise AssertionError(f"missing source operation: {address:#x}")


@pytest.fixture(scope="module")
def h1_rows() -> dict[int, tuple[bytes, str]]:
    listing = (UPSTREAM / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    return transactions_module._h1_instruction_rows(listing)


@pytest.fixture
def cached_h1_rows(monkeypatch: pytest.MonkeyPatch, h1_rows: dict[int, tuple[bytes, str]]) -> None:
    monkeypatch.setattr(transactions_module, "_h1_instruction_rows", lambda _text: h1_rows)


def test_projection_is_closed_exact_and_bounded_to_static_choreography() -> None:
    fixture = _fixture()
    validate_json(fixture, SCHEMA, owner="map-event item transactions fixture")
    rebuilt = _build()
    validate_json(rebuilt, SCHEMA, owner="map-event item transactions rebuilt contract")
    assert rebuilt == fixture
    assert list(fixture) == [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "retainedOwners",
        "sourceContext",
        "eventItemTransactions",
        "unknowns",
        "summary",
    ]
    assert fixture["id"] == ID
    assert fixture["summary"] == {
        "sourceIdentityCount": 16,
        "programContextCount": 8,
        "physicalProgramCount": 7,
        "contextOperationCount": 190,
        "physicalOperationCount": 150,
        "contextLabelCount": 42,
        "physicalLabelCount": 34,
        "contextEncodedByteCount": 708,
        "physicalEncodedByteCount": 558,
        "eventServiceMacroPhysicalOperationCount": 53,
        "rawInstructionPhysicalOperationCount": 37,
        "rawControlPhysicalOperationCount": 60,
        "ordinaryPhysicalControlCount": 90,
        "conditionalPhysicalControlCount": 20,
        "unconditionalPhysicalControlCount": 12,
        "directCallPhysicalControlCount": 20,
        "returnPhysicalControlCount": 8,
        "contextServiceCallCount": 15,
        "physicalServiceCallCount": 13,
        "contextPredicateCount": 9,
        "physicalPredicateCount": 7,
        "transactionChainCount": 8,
        "d6WriteCount": 5,
        "anchorCount": 167,
    }
    calls = fixture["eventItemTransactions"]["serviceCalls"].values()
    assert {(call["item"]["itemSymbol"], call["item"]["itemValue"]) for call in calls} == {
        ("ITEM_ACHILLES_SWORD", 0x3D),
        ("ITEM_WOODEN_PANEL", 0x70),
        ("ITEM_CANNON", 0x72),
        ("ITEM_DYNAMITE", 0x74),
        ("ITEM_ARM_OF_GOLEM", 0x75),
        ("ITEM_COTTON_BALLOON", 0x7D),
    }
    assert set(fixture["unknowns"].values()) == {"Unknown"}


@pytest.mark.parametrize(
    "mutator",
    (
        lambda value: value.__setitem__("runtimeObservation", {}),
        lambda value: value["sourceContext"].__setitem__("rawRomBytes", "private"),
        lambda value: value["eventItemTransactions"]["serviceCalls"].popitem(),
        lambda value: value["eventItemTransactions"]["programContexts"]["map6-entity-event13"][
            "operations"
        ][0].__setitem__("decodedDialogue", "private"),
        lambda value: value["unknowns"].__setitem__("actualInventoryContents", "Confirmed"),
    ),
)
def test_schema_rejects_private_payload_and_closed_shape_drift(mutator: object) -> None:
    broken = deepcopy(_fixture())
    mutator(broken)
    with pytest.raises(ValueError, match="schema validation"):
        validate_json(broken, SCHEMA, owner="map-event item transactions fixture")


@pytest.mark.parametrize(
    "control_kind",
    (
        "ordinary",
        "conditional-branch",
        "unconditional-branch",
        "direct-call",
        "return",
    ),
)
def test_source_operation_guards_reject_every_control_class(
    tmp_path: Path, control_kind: str, cached_h1_rows: None
) -> None:
    upstream, rom = _mutable_inputs(tmp_path, include_listing=False)
    source_path, source_line = _operation_for_control_kind(control_kind)
    with (
        _replaced_line(upstream / "disasm" / source_path, source_line, "nop"),
        pytest.raises(ValueError, match="source operation drift"),
    ):
        _build(rom, upstream)


@pytest.mark.parametrize(
    ("symbol", "expected_value", "mutation"),
    (
        ("ITEM_ACHILLES_SWORD", 0x3D, "wrong-value"),
        ("ITEM_ACHILLES_SWORD", 0x3D, "missing"),
        ("ITEM_ACHILLES_SWORD", 0x3D, "renamed"),
        ("ITEM_ACHILLES_SWORD", 0x3D, "duplicate"),
        ("ITEM_WOODEN_PANEL", 0x70, "wrong-value"),
        ("ITEM_WOODEN_PANEL", 0x70, "missing"),
        ("ITEM_WOODEN_PANEL", 0x70, "renamed"),
        ("ITEM_WOODEN_PANEL", 0x70, "duplicate"),
        ("ITEM_CANNON", 0x72, "wrong-value"),
        ("ITEM_CANNON", 0x72, "missing"),
        ("ITEM_CANNON", 0x72, "renamed"),
        ("ITEM_CANNON", 0x72, "duplicate"),
        ("ITEM_DYNAMITE", 0x74, "wrong-value"),
        ("ITEM_DYNAMITE", 0x74, "missing"),
        ("ITEM_DYNAMITE", 0x74, "renamed"),
        ("ITEM_DYNAMITE", 0x74, "duplicate"),
        ("ITEM_ARM_OF_GOLEM", 0x75, "wrong-value"),
        ("ITEM_ARM_OF_GOLEM", 0x75, "missing"),
        ("ITEM_ARM_OF_GOLEM", 0x75, "renamed"),
        ("ITEM_ARM_OF_GOLEM", 0x75, "duplicate"),
        ("ITEM_COTTON_BALLOON", 0x7D, "wrong-value"),
        ("ITEM_COTTON_BALLOON", 0x7D, "missing"),
        ("ITEM_COTTON_BALLOON", 0x7D, "renamed"),
        ("ITEM_COTTON_BALLOON", 0x7D, "duplicate"),
    ),
)
def test_item_enum_parser_rejects_each_used_identity_value_and_multiplicity(
    symbol: str, expected_value: int, mutation: str
) -> None:
    lines = (UPSTREAM / "disasm/sf2enums.asm").read_text(encoding="utf-8").splitlines()
    source_line = next(
        index for index, line in enumerate(lines) if line.lstrip().startswith(f"{symbol}:")
    )
    broken = list(lines)
    if mutation == "wrong-value":
        broken[source_line] = broken[source_line].replace(
            f"${expected_value:02X}", f"${(expected_value + 1):02X}"
        )
    elif mutation == "missing":
        broken.pop(source_line)
    elif mutation == "renamed":
        broken[source_line] = broken[source_line].replace(symbol, f"{symbol}_RENAMED", 1)
    else:
        broken.insert(source_line + 1, broken[source_line])
    with pytest.raises(ValueError, match="item enum drift"):
        transactions_module._parse_item_enums(broken)


@pytest.mark.parametrize("predicate_kind", ("inventory-location", "mandatory-receive"))
def test_predicate_source_guards_reject_sentinel_and_bit0(
    tmp_path: Path, predicate_kind: str, cached_h1_rows: None
) -> None:
    fixture = _fixture()
    predicate = next(
        row
        for row in fixture["eventItemTransactions"]["resultPredicates"].values()
        if row["predicateKind"] == predicate_kind
    )
    source_path, source_line = _operation_source(predicate["producerAddress"])
    upstream, rom = _mutable_inputs(tmp_path, include_listing=False)
    path = upstream / "disasm" / source_path
    original = path.read_text(encoding="utf-8").splitlines()[source_line - 1]
    token, replacement = ("#-1", "#0") if predicate_kind == "inventory-location" else ("#0", "#1")
    assert token in original
    with (
        _replaced_line(path, source_line, original.replace(token, replacement, 1)),
        pytest.raises(ValueError, match="source operation drift"),
    ):
        _build(rom, upstream)


@pytest.mark.parametrize("field", ("branchMnemonic", "targetAddress", "fallthroughAddress"))
def test_predicate_branch_relation_guards_reject_opcode_polarity_target_and_fallthrough(
    field: str,
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    cached_h1_rows: None,
) -> None:
    fixture = _fixture()
    predicate_id = "map72-zone-event3:GetItemInventoryLocation:04FEB0:predicate"
    predicate = fixture["eventItemTransactions"]["resultPredicates"][predicate_id]
    if field == "branchMnemonic":
        broken = deepcopy(fixture)
        broken["eventItemTransactions"]["resultPredicates"][predicate_id][field] = "bne"
        wrong_fixture = tmp_path / "wrong-branch-fixture.json"
        wrong_fixture.write_text(json.dumps(broken), encoding="utf-8")
        monkeypatch.setattr(transactions_module, "FIXTURE", wrong_fixture)
        monkeypatch.setattr(
            transactions_module,
            "build_map_events_contract",
            lambda _rom, _upstream: load_map_events_fixture()["expected"],
        )
        monkeypatch.setattr(
            transactions_module,
            "_retained_owners",
            lambda _map_events, *, check_manifest: _fixture()["retainedOwners"],
        )
        with pytest.raises(ValueError, match="complete semantic fixture drift"):
            verify_map_event_item_transactions_contract(ROM, UPSTREAM)
        return
    original = load_map_events_fixture()["expected"]
    broken = deepcopy(original)
    map72 = next(
        program
        for program in broken["zoneTargetPrograms"]
        if program["canonicalSymbol"] == "Map72_ZoneEvent3"
    )
    branch = next(
        row for row in map72["operations"] if row["address"] == predicate["branchAddress"]
    )
    if field == "targetAddress":
        branch["target"]["effectiveTargetAddress"] += 2
    else:
        next_row = next(
            row for row in map72["operations"] if row["address"] == predicate["fallthroughAddress"]
        )
        next_row["address"] += 2
    with pytest.raises(ValueError, match="retained map-events fixture drift"):
        build_map_event_item_transactions_contract(ROM, UPSTREAM, map_events_override=broken)


def test_branch_opcode_and_target_source_mutations_are_rejected_before_fixture(
    tmp_path: Path, cached_h1_rows: None
) -> None:
    fixture = _fixture()
    predicate = fixture["eventItemTransactions"]["resultPredicates"][
        "map72-zone-event3:GetItemInventoryLocation:04FEB0:predicate"
    ]
    source_path, source_line = _operation_source(predicate["branchAddress"])
    for replacement in ("bne.s   loc_4FEF4", "beq.s   loc_4FEE6"):
        upstream, rom = _mutable_inputs(tmp_path, include_listing=False)
        with (
            _replaced_line(upstream / "disasm" / source_path, source_line, replacement),
            pytest.raises(ValueError, match="source operation drift"),
        ):
            _build(rom, upstream)


@pytest.mark.parametrize("d6_address", (366122, 379462, 379468, 353016, 353038))
def test_each_item_event_d6_write_is_source_guarded(
    tmp_path: Path, d6_address: int, cached_h1_rows: None
) -> None:
    source_path, source_line = _operation_source(d6_address)
    upstream, rom = _mutable_inputs(tmp_path, include_listing=False)
    with (
        _replaced_line(upstream / "disasm" / source_path, source_line, "nop"),
        pytest.raises(ValueError, match="source operation drift"),
    ):
        _build(rom, upstream)


@pytest.mark.parametrize(
    ("source_path", "source_line", "replacement", "error"),
    (
        (
            "code/common/menus/main/mainactions.asm",
            253,
            "jsr j_ReturnFromMapSetup",
            "FieldMenu source seam drift",
        ),
        (
            "code/common/menus/main/mainactions.asm",
            254,
            "tst.w d5",
            "FieldMenu source seam drift",
        ),
        (
            "code/common/tech/jumpinterfaces/s07_jumpinterface.asm",
            348,
            "j_RunMapSetupItemEventBroken:",
            "source label drift",
        ),
        (
            "code/common/scripting/map/mapsetupsfunctions_1.asm",
            90,
            "RunMapSetupItemEventBroken:",
            "source label drift",
        ),
    ),
)
def test_field_menu_map_setup_and_interface_seams_are_source_guarded(
    tmp_path: Path,
    source_path: str,
    source_line: int,
    replacement: str,
    error: str,
    cached_h1_rows: None,
) -> None:
    upstream, rom = _mutable_inputs(tmp_path)
    with (
        _replaced_line(upstream / "disasm" / source_path, source_line, replacement),
        pytest.raises(ValueError, match=error),
    ):
        _build(rom, upstream)


def test_table_anchor_and_independent_h1_rom_identities_are_falsifiable(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    h1_rows: dict[int, tuple[bytes, str]],
) -> None:
    fixture = _fixture()
    table = fixture["sourceContext"]["eventTableAnchors"][0]
    upstream, rom = _mutable_inputs(tmp_path)
    with (
        _replaced_line(
            upstream / "disasm" / table["sourcePath"], table["sourceLine"], "msDefaultEntityEvent 0"
        ),
        pytest.raises(ValueError, match="seam source drift"),
    ):
        monkeypatch.setattr(transactions_module, "_h1_instruction_rows", lambda _text: h1_rows)
        _build(rom, upstream)

    def changed_h1(_text: str) -> dict[int, tuple[bytes, str]]:
        changed = dict(h1_rows)
        encoded, statement = changed[136490]
        changed[136490] = (bytes([encoded[0] ^ 1]) + encoded[1:], statement)
        return changed

    monkeypatch.setattr(transactions_module, "_h1_instruction_rows", changed_h1)
    changed_h1_contract = _build()
    assert (
        changed_h1_contract["sourceContext"]["handoffAnchors"][0]["h1EncodedSha256"]
        != fixture["sourceContext"]["handoffAnchors"][0]["h1EncodedSha256"]
    )

    source_path = "code/common/menus/main/mainactions.asm"
    source_text = {
        source_path: (UPSTREAM / "disasm" / source_path).read_text(encoding="utf-8").splitlines()
    }
    original_rom = ROM.read_bytes()
    original_anchor = transactions_module._source_seam_anchor(
        address=136496,
        role="field-menu-result-test",
        source_path=source_path,
        source_line=254,
        source_statement="tst.w d6",
        source_text=source_text,
        h1_rows={136496: h1_rows[136496]},
        rom=original_rom,
        extra={},
    )
    changed_rom = bytearray(original_rom)
    changed_rom[136496] ^= 1
    changed_rom_anchor = transactions_module._source_seam_anchor(
        address=136496,
        role="field-menu-result-test",
        source_path=source_path,
        source_line=254,
        source_statement="tst.w d6",
        source_text=source_text,
        h1_rows={136496: h1_rows[136496]},
        rom=bytes(changed_rom),
        extra={},
    )
    assert (
        original_anchor["romInstructionSha256"]
        == fixture["sourceContext"]["handoffAnchors"][3]["romInstructionSha256"]
    )
    assert changed_rom_anchor["romInstructionSha256"] != original_anchor["romInstructionSha256"]


def test_full_rom_identity_rejects_mutation_outside_all_anchor_spans(tmp_path: Path) -> None:
    fixture = _fixture()
    anchors = [
        anchor
        for group in (
            "physicalOperationAnchors",
            "eventTableAnchors",
            "serviceSeamAnchors",
            "handoffAnchors",
        )
        for anchor in fixture["sourceContext"][group]
    ]
    outside_anchor_address = 0
    assert all(
        not (
            anchor["address"]
            <= outside_anchor_address
            < anchor["address"] + anchor["romEncodedByteCount"]
        )
        for anchor in anchors
    )
    changed_rom = bytearray(ROM.read_bytes())
    changed_rom[outside_anchor_address] ^= 1
    mutated_rom = tmp_path / "mutated-rom.bin"
    mutated_rom.write_bytes(changed_rom)
    with pytest.raises(ValueError, match="map-event item transactions ROM identity drift"):
        _build(mutated_rom, UPSTREAM)


def test_retained_map_event_guard_rejects_table_order_and_shared_tail_mutation() -> None:
    original = load_map_events_fixture()["expected"]
    reordered = deepcopy(original)
    map72 = next(
        program
        for program in reordered["zoneTargetPrograms"]
        if program["canonicalSymbol"] == "Map72_ZoneEvent3"
    )
    map72["operations"][10], map72["operations"][11] = (
        map72["operations"][11],
        map72["operations"][10],
    )
    with pytest.raises(ValueError, match="retained map-events fixture drift"):
        build_map_event_item_transactions_contract(ROM, UPSTREAM, map_events_override=reordered)

    broken_tail = deepcopy(original)
    map6 = next(
        program
        for program in broken_tail["entityTargetPrograms"]
        if program["canonicalSymbol"] == "Map6_EntityEvent13"
    )
    default_label = next(
        label for label in map6["labels"] if label["symbol"] == "Map6_DefaultEntityEvent"
    )
    default_label["address"] = map6["entryAddress"]
    with pytest.raises(ValueError, match="retained map-events fixture drift"):
        build_map_event_item_transactions_contract(ROM, UPSTREAM, map_events_override=broken_tail)


@pytest.mark.parametrize("mutation", ("missing", "extra", "duplicate", "wrong"))
def test_retained_source_call_inventory_rejects_each_service_call_drift(mutation: str) -> None:
    original = load_map_events_fixture()["expected"]
    broken = deepcopy(original)
    map72 = next(
        program
        for program in broken["zoneTargetPrograms"]
        if program["canonicalSymbol"] == "Map72_ZoneEvent3"
    )
    calls = [
        row
        for row in map72["operations"]
        if row["target"] is not None
        and row["target"]["effectiveTargetSymbol"]
        in {"GetItemInventoryLocation", "RemoveItemFromInventory"}
    ]
    assert len(calls) == 4
    if mutation == "missing":
        map72["operations"].remove(calls[0])
    elif mutation == "extra":
        duplicate = deepcopy(calls[0])
        duplicate["address"] += 1
        map72["operations"].append(duplicate)
    elif mutation == "duplicate":
        calls[2]["target"] = deepcopy(calls[0]["target"])
    else:
        calls[0]["target"]["effectiveTargetSymbol"] = "RemoveItemBySlot"
    with pytest.raises(ValueError, match="retained map-events fixture drift"):
        build_map_event_item_transactions_contract(ROM, UPSTREAM, map_events_override=broken)


def test_map72_lookup_removal_order_is_retained_before_fixture_comparison() -> None:
    original = load_map_events_fixture()["expected"]
    broken = deepcopy(original)
    map72 = next(
        program
        for program in broken["zoneTargetPrograms"]
        if program["canonicalSymbol"] == "Map72_ZoneEvent3"
    )
    calls = [
        row
        for row in map72["operations"]
        if row["target"] is not None
        and row["target"]["effectiveTargetSymbol"]
        in {"GetItemInventoryLocation", "RemoveItemFromInventory"}
    ]
    for first, second in zip(calls, calls[1:], strict=False):
        first["target"], second["target"] = second["target"], first["target"]
    with pytest.raises(ValueError, match="retained map-events fixture drift"):
        build_map_event_item_transactions_contract(ROM, UPSTREAM, map_events_override=broken)


def test_h1_anchor_and_alias_seam_guards_reject_missing_or_wrong_entry(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    with pytest.raises(ValueError, match="seam H1 row missing"):
        transactions_module._source_seam_anchor(
            address=1,
            role="service-instruction-target",
            source_path="seam.asm",
            source_line=1,
            source_statement="entry:",
            source_text={"seam.asm": ["entry:"]},
            h1_rows={},
            rom=b"\x00",
            extra={"serviceEntryId": "GetItemInventoryLocation"},
        )
    with pytest.raises(ValueError, match="seam ROM boundary drift"):
        transactions_module._source_seam_anchor(
            address=1,
            role="service-instruction-target",
            source_path="seam.asm",
            source_line=1,
            source_statement="entry:",
            source_text={"seam.asm": ["entry:"]},
            h1_rows={1: (b"\x00\x01", "entry")},
            rom=b"\x00",
            extra={"serviceEntryId": "GetItemInventoryLocation"},
        )

    wrong_targets = deepcopy(transactions_module._SERVICE_TARGETS)
    wrong_targets["GetItemInventoryLocation"]["entryAddress"] += 2
    with monkeypatch.context() as patch:
        patch.setattr(transactions_module, "_SERVICE_TARGETS", wrong_targets)
        with pytest.raises(ValueError, match="service alias/effective entry drift"):
            transactions_module._validate_service_seam_identities()


def test_map_setup_retained_owner_identity_and_path_are_exact(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    fixture = _fixture()
    assert transactions_module._MAP_SETUP_RETAINED_FIXTURE == (
        "sf2-map-setup-static-v1",
        "tests/fixtures/h2/map-setup-static-v1.json",
    )
    assert fixture["retainedOwners"]["mapSetup"] == {
        "fixtureId": "sf2-map-setup-static-v1",
        "fixtureSha256": transactions_module._fixture_digest(
            "tests/fixtures/h2/map-setup-static-v1.json"
        ),
    }
    broken = tuple(
        (name, "sf2-map-events-static-v1", path) if name == "mapSetup" else (name, fixture_id, path)
        for name, fixture_id, path in transactions_module._RETAINED_FIXTURES
    )
    monkeypatch.setattr(transactions_module, "_RETAINED_FIXTURES", broken)
    with pytest.raises(ValueError, match="retained mapSetup owner drift"):
        _build()


def test_strict_later_owner_normalizer_proves_only_the_declared_delta() -> None:
    index = _remove_map_event_combatant_state_later_owner_index_delta(
        _remove_map_event_random_battle_state_later_owner_index_delta(
            _remove_map_event_tactical_base_quote_state_later_owner_index_delta(
                _remove_map_event_scripted_transition_state_later_owner_index_delta(
                    _remove_cross_program_flag_lifecycle_deltas(load_json(INDEX))
                )
            )
        )
    )
    prior = _remove_map_event_item_transactions_index_delta(index)
    assert normalize_map_event_item_transactions_later_owner_index(index) == (
        normalize_interaction_state_later_owner_index(prior)
    )

    for mutator in (
        lambda value: value["records"][0]["documents"].append(
            "docs/research/map-event-item-transactions.md"
        ),
        lambda value: next(
            record for record in value["records"] if record["id"] == "stats.item-stats"
        )["addresses"].__setitem__(
            0,
            {
                "id": "get-item-inventory-location",
                "space": "rom",
                "kind": "observation",
                "value": 37191,
            },
        ),
    ):
        broken = deepcopy(index)
        mutator(broken)
        with pytest.raises(ValueError, match="map-event item transactions later-owner"):
            _remove_map_event_item_transactions_index_delta(broken)


@pytest.mark.parametrize(
    "relative_path",
    (
        "tests/python/test_field_item_effects.py",
        "tests/python/test_field_menu_control.py",
        "tests/python/test_field_search_control.py",
        "tests/python/test_common_stats.py",
        "tests/python/test_map3_battle01_action_completion.py",
        "tests/python/test_map3_battle01_victory_return.py",
        "tests/python/test_map_event_combatant_state.py",
        "tests/python/test_map_event_dialogue_state.py",
        "tests/python/test_map_event_direct_control.py",
        "tests/python/test_map_event_direct_handoff.py",
        "tests/python/test_map_event_direct_state.py",
        "tests/python/test_map_event_interaction_state.py",
        "tests/python/test_map_event_predicate_results.py",
        "tests/python/test_map_event_request_consumption.py",
        "tests/python/test_map_event_request_state.py",
        "tests/python/test_map_event_scripted_transition_state.py",
        "src/sf2tool/h2/map3_battle01_victory_return.py",
    ),
)
def test_all_sibling_compatibility_paths_accept_the_exact_later_owner_index(
    relative_path: str,
) -> None:
    namespace = runpy.run_path(str(ROOT / relative_path))
    index = load_json(INDEX)
    if relative_path.endswith("map3_battle01_victory_return.py"):
        if relative_path.startswith("tests/"):
            normalized = namespace["_normalize_later_owner_index"](index)
        else:
            normalized = namespace["_normalize_request_consumption_later_owner_index"](index)
    elif relative_path.endswith("test_common_stats.py") or relative_path.endswith(
        "test_map3_battle01_action_completion.py"
    ):
        normalized = namespace["normalize_later_owner_index"](index)
    elif relative_path.endswith("test_map_event_combatant_state.py"):
        normalized = namespace["normalize_map_event_combatant_state_later_owner_index"](
            namespace["_remove_map_event_random_battle_state_later_owner_index_delta"](
                namespace["_remove_map_event_tactical_base_quote_state_later_owner_index_delta"](
                    namespace["_remove_map_event_scripted_transition_state_later_owner_index_delta"](
                        namespace[
                            "_remove_cross_program_flag_lifecycle_deltas"
                        ](index)
                    )
                )
            )
        )
    elif relative_path.endswith("test_map_event_scripted_transition_state.py"):
        normalized = namespace["normalize_map_event_scripted_transition_state_later_owner_index"](
            namespace["_remove_cross_program_flag_lifecycle_deltas"](
                index
            )
        )
    elif relative_path.endswith("test_map_event_interaction_state.py"):
        normalized = namespace["_normalize_interaction_predecessor_index"](index)
    else:
        normalized = namespace["load_json"](INDEX)
    assert len(normalized["records"]) == 1625
