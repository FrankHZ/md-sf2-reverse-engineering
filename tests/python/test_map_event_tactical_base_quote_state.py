"""Focused adversarial tests for the static tactical-base quote contract."""

from __future__ import annotations

import hashlib
from copy import deepcopy
from pathlib import Path
from shutil import copy2

import pytest

import sf2tool.h2.map_event_tactical_base_quote_state as tactical_module
from sf2tool.h2.map_event_combatant_state import canonical_json_bytes
from sf2tool.h2.map_event_tactical_base_quote_state import (
    _PREDECESSOR_INDEX_SHA256,
    FIXTURE,
    ID,
    SCHEMA,
    _remove_map_event_tactical_base_quote_state_later_owner_index_delta,
    build_map_event_tactical_base_quote_state_contract,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import (
    _normalize_current_index_to_owner_state,
    normalize_current_index_to_owner_predecessor,
)

ROM = repo_path("local/roms/sf2-us.bin")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
INDEX = repo_path("manifests/research-index.json")
DOCUMENT = "docs/research/map-event-tactical-base-quote-state.md"
_CHANGED_IDS = {
    "map.data.ms-map37-entityevents",
    "map.data.ms-map46-entityevents",
    "scripting.map.headquartersfunctions",
    "tech.interfaces.jump-s03b",
    "menus.name-under-portrait",
    "tech.interfaces.jump-s02",
    "stats.combatant-getters",
    "stats.flags",
    "scripting.text.textfunctions-1",
}


_ADDRESS_DELTA = (
    ("tech.interfaces.jump-s03b", "open-name-under-portrait", 65708),
    ("tech.interfaces.jump-s03b", "close-name-under-portrait", 65712),
    ("menus.name-under-portrait", "close-entry", 92720),
)
_STATE_ROOT = "tacticalBaseQuoteState"
_BINDING_DELTA = (
    (
        "map.data.ms-map37-entityevents",
        "entry",
        f"{_STATE_ROOT}.sourceFiles.map37.tableEntryAddress",
    ),
    (
        "map.data.ms-map46-entityevents",
        "entry",
        f"{_STATE_ROOT}.sourceFiles.map46.tableEntryAddress",
    ),
    (
        "scripting.map.headquartersfunctions",
        "entry",
        f"{_STATE_ROOT}.functionFlow.entryAddress",
    ),
    (
        "tech.interfaces.jump-s03b",
        "open-name-under-portrait",
        f"{_STATE_ROOT}.serviceEntries.j_OpenNameUnderPortraitWindow.instructionTargetAddress",
    ),
    (
        "tech.interfaces.jump-s03b",
        "close-name-under-portrait",
        f"{_STATE_ROOT}.serviceEntries.j_CloseNameUnderPortraitWindow.instructionTargetAddress",
    ),
    (
        "menus.name-under-portrait",
        "entry",
        f"{_STATE_ROOT}.serviceEntries.j_OpenNameUnderPortraitWindow.effectiveTargetAddress",
    ),
    (
        "menus.name-under-portrait",
        "close-entry",
        f"{_STATE_ROOT}.serviceEntries.j_CloseNameUnderPortraitWindow.effectiveTargetAddress",
    ),
    (
        "tech.interfaces.jump-s02",
        "get-current-hp",
        f"{_STATE_ROOT}.serviceEntries.j_GetCurrentHp.instructionTargetAddress",
    ),
    (
        "tech.interfaces.jump-s02",
        "check-flag",
        f"{_STATE_ROOT}.serviceEntries.j_CheckFlag.instructionTargetAddress",
    ),
    (
        "stats.combatant-getters",
        "get-current-hp",
        f"{_STATE_ROOT}.serviceEntries.j_GetCurrentHp.effectiveTargetAddress",
    ),
    (
        "stats.flags",
        "entry",
        f"{_STATE_ROOT}.serviceEntries.j_CheckFlag.effectiveTargetAddress",
    ),
    (
        "scripting.text.textfunctions-1",
        "entry",
        f"{_STATE_ROOT}.serviceEntries.DisplayText.effectiveTargetAddress",
    ),
)


def _copied_surface(tmp_path: Path, *, copy_rom: bool = False) -> tuple[Path, Path]:
    upstream = tmp_path / "SF2DISASM"
    for source_path in tactical_module._SOURCE_PATHS:
        destination = upstream / "disasm" / source_path
        destination.parent.mkdir(parents=True, exist_ok=True)
        copy2(UPSTREAM / "disasm" / source_path, destination)
    listing = upstream / "build" / "sf2build-h1.lst"
    listing.parent.mkdir(parents=True, exist_ok=True)
    copy2(UPSTREAM / "build/sf2build-h1.lst", listing)
    rom = ROM
    if copy_rom:
        rom = tmp_path / "sf2-us.bin"
        copy2(ROM, rom)
    return upstream, rom


def test_complete_static_contract_matches_golden() -> None:
    actual = build_map_event_tactical_base_quote_state_contract(ROM, UPSTREAM)
    assert actual == load_json(FIXTURE)
    facts = actual["tacticalBaseQuoteState"]
    assert actual["summary"] == {
        "sourceIdentityCount": 10,
        "motherProgramContextCount": 914,
        "positiveProgramContextCount": 54,
        "zeroProgramContextCount": 860,
        "physicalProgramCount": 54,
        "map37CallerContextCount": 25,
        "map46CallerContextCount": 29,
        "callerInstructionRowCount": 108,
        "functionInstructionRowCount": 16,
        "sourceOperationCount": 124,
        "h1InstructionRowCount": 124,
        "ownedByteCount": 490,
        "retainedServiceJoinCount": 9,
        "anchorCount": 133,
    }
    contexts = facts["programContexts"]
    assert [contexts[key]["programSymbol"] for key in facts["programContextOrder"][:25]] == [
        *(f"Map37_EntityEvent{number}" for number in range(24)),
        "Map37_EntityEvent25",
    ]
    assert [contexts[key]["programSymbol"] for key in facts["programContextOrder"][25:]] == [
        f"Map46_EntityEvent{number}" for number in range(29)
    ]
    assert facts["programContextOrder"] == facts["physicalProgramOrder"]
    assert len(facts["physicalPrograms"]) == 54
    assert all(len(row["operations"]) == 2 for row in contexts.values())
    assert sum(len(row["operations"]) for row in contexts.values()) == 108
    caller_anchors = actual["sourceContext"]["callerAnchors"]
    assert len(caller_anchors) == 108
    assert (
        sum(
            anchor["controlFlowKind"] == "direct-jump"
            and anchor["expectedTargetAddress"] == 0x4790E
            for anchor in caller_anchors
        )
        == 54
    )
    assert len(actual["sourceContext"]["functionAnchors"]) == 16
    assert len(actual["sourceContext"]["retainedServiceAnchors"]) == 9
    assert all("sourceStatement" not in anchor for anchor in caller_anchors)
    assert facts["allySelectors"]["values"] == list(range(1, 30))
    assert facts["quoteLineDomain"]["uniqueLineIdCount"] == 59


def test_mother_corpus_mutation_is_rejected() -> None:
    altered = deepcopy(load_map_events_fixture()["expected"])
    altered["entityTargetPrograms"] = altered["entityTargetPrograms"][:-1]
    with pytest.raises(ValueError, match="mother corpus"):
        build_map_event_tactical_base_quote_state_contract(
            ROM, UPSTREAM, map_events_override=altered
        )


@pytest.mark.parametrize(
    ("path", "old", "new", "message"),
    [
        (tactical_module._MAP37_PATH, "#ALLY_SARAH,d0", "#ALLY_CHESTER,d0", "caller source"),
        (
            tactical_module._MAP46_PATH,
            "jmp     DisplayTacticalBaseQuote",
            "jsr     DisplayTacticalBaseQuote",
            "caller source",
        ),
        (tactical_module._FUNCTION_PATH, "tst.w   d1", "tst.b   d1", "function source"),
        (tactical_module._FUNCTION_PATH, "addi.w  #$DC3,d0", "addi.w  #$DE1,d0", "function source"),
    ],
)
def test_source_mutations_are_rejected(
    tmp_path: Path, path: str, old: str, new: str, message: str
) -> None:
    upstream, _ = _copied_surface(tmp_path)
    target = upstream / "disasm" / path
    target.write_text(target.read_text(encoding="utf-8").replace(old, new, 1), encoding="utf-8")
    with pytest.raises(ValueError, match=message):
        build_map_event_tactical_base_quote_state_contract(ROM, upstream)


def test_h1_and_rom_anchor_mutations_are_rejected(tmp_path: Path) -> None:
    upstream, rom = _copied_surface(tmp_path, copy_rom=True)
    listing = upstream / "build/sf2build-h1.lst"
    listing.write_text(
        listing.read_text(encoding="utf-8").replace("04791A 4A41", "04791A 4A40", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="H1/ROM anchor"):
        build_map_event_tactical_base_quote_state_contract(rom, upstream)
    rom_bytes = bytearray(ROM.read_bytes())
    rom_bytes[0x4791A] ^= 1
    rom.write_bytes(rom_bytes)
    with pytest.raises(ValueError, match="ROM identity"):
        build_map_event_tactical_base_quote_state_contract(rom, UPSTREAM)


@pytest.mark.parametrize(
    ("role", "address", "h1_old", "h1_new", "rom_old", "rom_new"),
    [
        (
            "caller-tail-jump",
            0x5F8E2,
            "0005F8E2 4EF9 0004 790E",
            "0005F8E2 4EF9 0004 790C",
            b"\x4e\xf9\x00\x04\x79\x0e",
            b"\x4e\xf9\x00\x04\x79\x0c",
        ),
        (
            "callee-alias-call",
            0x4790E,
            "0004790E 4EB9 0001 00AC",
            "0004790E 4EB9 0001 00AE",
            b"\x4e\xb9\x00\x01\x00\xac",
            b"\x4e\xb9\x00\x01\x00\xae",
        ),
        (
            "display-text-edge",
            0x4793C,
            "0004793C 4EB8 6260",
            "0004793C 4EB8 6262",
            b"\x4e\xb8\x62\x60",
            b"\x4e\xb8\x62\x62",
        ),
        ("pc-relative-service-stub", 0x100AC, None, None, b"\x4e\xfa\x69\x00", b"\x4e\xfa\x69\x02"),
    ],
)
def test_effective_target_classes_reject_before_fixture_comparison(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    role: str,
    address: int,
    h1_old: str | None,
    h1_new: str | None,
    rom_old: bytes,
    rom_new: bytes,
) -> None:
    upstream, rom = _copied_surface(tmp_path, copy_rom=True)
    listing = upstream / "build/sf2build-h1.lst"
    if h1_old is not None and h1_new is not None:
        text = listing.read_text(encoding="utf-8")
        assert text.count(h1_old) >= 1
        listing.write_text(text.replace(h1_old, h1_new, 1), encoding="utf-8")
    rom_bytes = bytearray(rom.read_bytes())
    assert rom_bytes[address : address + len(rom_old)] == rom_old
    rom_bytes[address : address + len(rom_new)] = rom_new
    rom.write_bytes(rom_bytes)
    monkeypatch.setattr(
        tactical_module, "_ROM_SHA256", hashlib.sha256(rom_bytes).hexdigest().upper()
    )
    with pytest.raises(ValueError, match="effective target"):
        build_map_event_tactical_base_quote_state_contract(rom, upstream)


@pytest.mark.parametrize(
    ("role", "anchor_group", "anchor_index"),
    [
        ("caller-setup", "callerAnchors", 0),
        ("caller-tail", "callerAnchors", 1),
        ("function-fallthrough", "functionAnchors", 2),
        ("function-branch", "functionAnchors", 3),
        ("service-jump-interface", "retainedServiceAnchors", 0),
        ("service-effective-entry", "retainedServiceAnchors", 1),
    ],
)
def test_representative_anchor_roles_reject_rom_mutation_before_fixture_comparison(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    role: str,
    anchor_group: str,
    anchor_index: int,
) -> None:
    upstream, rom = _copied_surface(tmp_path, copy_rom=True)
    baseline = build_map_event_tactical_base_quote_state_contract(rom, upstream)
    address = baseline["sourceContext"][anchor_group][anchor_index]["address"]
    rom_bytes = bytearray(rom.read_bytes())
    rom_bytes[address] ^= 1
    rom.write_bytes(rom_bytes)
    monkeypatch.setattr(
        tactical_module, "_ROM_SHA256", hashlib.sha256(rom_bytes).hexdigest().upper()
    )
    with pytest.raises(ValueError, match="H1/ROM anchor|effective target"):
        build_map_event_tactical_base_quote_state_contract(rom, upstream)


@pytest.mark.parametrize(
    ("old", "new", "message"),
    [
        ("ALLY_SARAH: equ 1", "ALLY_SARAH: equ ALLY_CHESTER", "ally alias"),
        (
            "ALLY_SARAH: equ 1\nALLY_CHESTER: equ 2",
            "ALLY_SARAH: equ 2\nALLY_CHESTER: equ 1",
            "ally enumeration",
        ),
    ],
)
def test_ally_selector_equate_alias_and_value_swaps_fail_source_first(
    tmp_path: Path, old: str, new: str, message: str
) -> None:
    upstream, _ = _copied_surface(tmp_path)
    path = upstream / "disasm/sf2enums.asm"
    text = path.read_text(encoding="utf-8")
    assert old in text
    path.write_text(text.replace(old, new, 1), encoding="utf-8")
    with pytest.raises(ValueError, match=message):
        build_map_event_tactical_base_quote_state_contract(ROM, upstream)


def test_source_and_listing_identities_hash_raw_bytes() -> None:
    baseline = build_map_event_tactical_base_quote_state_contract(ROM, UPSTREAM)
    source_identity = next(
        row
        for row in baseline["sourceContext"]["sourceIdentities"]
        if row["path"] == tactical_module._MAP37_PATH
    )
    assert (
        source_identity["sha256"]
        == hashlib.sha256((UPSTREAM / "disasm" / tactical_module._MAP37_PATH).read_bytes())
        .hexdigest()
        .upper()
    )
    assert (
        baseline["sourceContext"]["h1Listing"]["sha256"]
        == hashlib.sha256((UPSTREAM / "build/sf2build-h1.lst").read_bytes()).hexdigest().upper()
    )


def test_source_and_listing_identity_hashes_preserve_raw_newlines(tmp_path: Path) -> None:
    baseline = build_map_event_tactical_base_quote_state_contract(ROM, UPSTREAM)
    source_upstream, _ = _copied_surface(tmp_path / "source")
    source = source_upstream / "disasm" / tactical_module._MAP37_PATH
    source.write_bytes(source.read_bytes() + b"\n")
    source_changed = build_map_event_tactical_base_quote_state_contract(ROM, source_upstream)
    assert (
        source_changed["sourceContext"]["sourceIdentities"]
        != baseline["sourceContext"]["sourceIdentities"]
    )
    listing_upstream, _ = _copied_surface(tmp_path / "listing")
    listing = listing_upstream / "build/sf2build-h1.lst"
    listing.write_bytes(listing.read_bytes() + b"\n")
    listing_changed = build_map_event_tactical_base_quote_state_contract(ROM, listing_upstream)
    assert listing_changed["sourceContext"]["h1Listing"] != baseline["sourceContext"]["h1Listing"]


def test_schema_is_closed_and_rejects_private_payloads() -> None:
    fixture = load_json(FIXTURE)
    root_mutation = deepcopy(fixture)
    root_mutation["runtimeGolden"] = True
    with pytest.raises(ValueError):
        validate_json(root_mutation, SCHEMA, owner="tactical quote root mutation")
    nested_mutation = deepcopy(fixture)
    nested_mutation["sourceContext"]["callerAnchors"][0]["rawRomBytes"] = "private"
    with pytest.raises(ValueError):
        validate_json(nested_mutation, SCHEMA, owner="tactical quote private mutation")
    structural_mutation = deepcopy(fixture)
    structural_mutation["sourceContext"]["callerAnchors"][0]["sourceStatement"] = "private prose"
    with pytest.raises(ValueError):
        validate_json(structural_mutation, SCHEMA, owner="tactical quote structural mutation")
    order_mutation = deepcopy(fixture)
    order_mutation["tacticalBaseQuoteState"]["programContextOrder"][0:2] = [
        "map37-event1",
        "map37-event0",
    ]
    with pytest.raises(ValueError):
        validate_json(order_mutation, SCHEMA, owner="tactical quote order mutation")
    source_order_mutation = deepcopy(fixture)
    source_order_mutation["sourceContext"]["sourceIdentities"][0:2] = list(
        reversed(source_order_mutation["sourceContext"]["sourceIdentities"][0:2])
    )
    with pytest.raises(ValueError):
        validate_json(source_order_mutation, SCHEMA, owner="tactical quote source identity order")
    anchor_order_mutation = deepcopy(fixture)
    anchor_order_mutation["sourceContext"]["callerAnchors"][0:2] = list(
        reversed(anchor_order_mutation["sourceContext"]["callerAnchors"][0:2])
    )
    with pytest.raises(ValueError, match="callerAnchors order"):
        tactical_module._validate_order(anchor_order_mutation)
    text_mutation = deepcopy(fixture)
    text_mutation["tacticalBaseQuoteState"]["quoteLineDomain"]["decodedText"] = "not public"
    with pytest.raises(ValueError):
        validate_json(text_mutation, SCHEMA, owner="tactical quote decoded text mutation")


def test_public_id_and_fixture_path_are_stable() -> None:
    assert ID == "sf2-map-event-tactical-base-quote-state-static-v1"
    assert Path(FIXTURE).name == "map-event-tactical-base-quote-state-static-v1.json"


def _record(index: dict[str, object], record_id: str) -> dict[str, object]:
    return next(row for row in index["records"] if row["id"] == record_id)


def _totals(index: dict[str, object]) -> dict[str, int]:
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


def test_later_owner_index_delta_is_exact_and_delegates() -> None:
    current = load_json(INDEX)
    tactical_current = _normalize_current_index_to_owner_state(current, owner_id=ID)
    predecessor = _remove_map_event_tactical_base_quote_state_later_owner_index_delta(
        tactical_current
    )
    assert hashlib.sha256(canonical_json_bytes(predecessor)).hexdigest().upper() == (
        _PREDECESSOR_INDEX_SHA256
    )
    current_by_id = {row["id"]: row for row in tactical_current["records"]}
    predecessor_by_id = {row["id"]: row for row in predecessor["records"]}
    assert set(current_by_id) == set(predecessor_by_id)
    assert {
        record_id
        for record_id in current_by_id
        if current_by_id[record_id] != predecessor_by_id[record_id]
    } == _CHANGED_IDS
    current_totals = _totals(tactical_current)
    predecessor_totals = _totals(predecessor)
    assert {key: current_totals[key] - predecessor_totals[key] for key in current_totals} == {
        "records": 0,
        "addresses": 3,
        "h2Evidence": 9,
        "bindings": 12,
        "documents": 9,
        "designContracts": 0,
    }
    assert normalize_current_index_to_owner_predecessor(current, owner_id=ID) == predecessor


@pytest.mark.parametrize("record_id,address_id,value", _ADDRESS_DELTA)
@pytest.mark.parametrize("mutation", ("missing", "extra", "wrong"))
def test_later_owner_normalizer_rejects_every_address_delta_mutation(
    record_id: str, address_id: str, value: int, mutation: str
) -> None:
    altered = _normalize_current_index_to_owner_state(load_json(INDEX), owner_id=ID)
    addresses = _record(altered, record_id)["addresses"]
    address = next(row for row in addresses if row["id"] == address_id and row["value"] == value)
    if mutation == "missing":
        addresses.remove(address)
    elif mutation == "extra":
        addresses.append({"id": "unexpected", "space": "rom", "kind": "observation", "value": 0})
    else:
        address["value"] += 2
    with pytest.raises(ValueError, match="address|predecessor"):
        _remove_map_event_tactical_base_quote_state_later_owner_index_delta(altered)


@pytest.mark.parametrize("record_id,address_id,fixture_field", _BINDING_DELTA)
@pytest.mark.parametrize("mutation", ("missing", "extra", "wrong"))
def test_later_owner_normalizer_rejects_every_binding_delta_mutation(
    record_id: str, address_id: str, fixture_field: str, mutation: str
) -> None:
    altered = _normalize_current_index_to_owner_state(load_json(INDEX), owner_id=ID)
    evidence = next(
        row for row in _record(altered, record_id)["evidence"] if row.get("fixtureId") == ID
    )
    binding = next(
        row
        for row in evidence["bindings"]
        if (row["addressId"], row["fixtureField"]) == (address_id, fixture_field)
    )
    if mutation == "missing":
        evidence["bindings"].remove(binding)
    elif mutation == "extra":
        evidence["bindings"].append(
            {"addressId": "unexpected", "fixtureField": "tacticalBaseQuoteState.unexpected"}
        )
    else:
        binding["fixtureField"] = "tacticalBaseQuoteState.unexpected"
    with pytest.raises(ValueError, match="record fields"):
        _remove_map_event_tactical_base_quote_state_later_owner_index_delta(altered)


def test_later_owner_normalizer_rejects_document_and_unrelated_drift() -> None:
    altered = _normalize_current_index_to_owner_state(load_json(INDEX), owner_id=ID)
    _record(altered, "stats.flags")["documents"].remove(DOCUMENT)
    with pytest.raises(ValueError, match="record fields drift"):
        _remove_map_event_tactical_base_quote_state_later_owner_index_delta(altered)
    unrelated = _normalize_current_index_to_owner_state(load_json(INDEX), owner_id=ID)
    _record(unrelated, "stats.flags")["symbol"] = "Unexpected"
    with pytest.raises(ValueError, match="predecessor index drift"):
        _remove_map_event_tactical_base_quote_state_later_owner_index_delta(unrelated)
