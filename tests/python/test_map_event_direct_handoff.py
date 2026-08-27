from __future__ import annotations

import hashlib
import json
import subprocess
from collections import Counter
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

import sf2tool.h2.map_event_direct_handoff as handoff_module
from sf2tool.cli import build_parser
from sf2tool.h2.map_event_combatant_state import (
    normalize_map_event_combatant_state_later_owner_index as normalize_later_owner_index,
)
from sf2tool.h2.map_event_direct_handoff import (
    _UNKNOWN_KEYS,
    _handoff_projection,
    _mother_corpus_projection,
    _validate_contract_order,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json as _load_json
from sf2tool.jsonio import validate_json
from sf2tool.research_index import verify_index

ROOT = Path(__file__).resolve().parents[2]
UPSTREAM = ROOT / "local/upstream/SF2DISASM"
ROM = ROOT / "local/roms/sf2-us.bin"
FIXTURE = ROOT / "tests/fixtures/h2/map-event-direct-handoff-static-v1.json"
SCHEMA = ROOT / "schemas/h2/map-event-direct-handoff-static-fixture.schema.json"
INDEX = ROOT / "manifests/research-index.json"
INDEX_SCHEMA = ROOT / "schemas/research-index.schema.json"
BASE = "2c567569412d9ef74582cda7f13156b33653f648"
FIXTURE_ID = "sf2-map-event-direct-handoff-static-v1"
DOCUMENT = "docs/research/map-event-direct-handoff.md"
VERIFIER = "src/sf2tool/h2/map_event_direct_handoff.py"
PREDICATE_FIXTURE_ID = "sf2-map-event-predicate-results-static-v1"
PREDICATE_DOCUMENT = "docs/research/map-event-predicate-results.md"
_DIALOGUE_STATE_FIXTURE_ID = "sf2-map-event-dialogue-state-static-v1"
_DIALOGUE_STATE_DOCUMENT = "docs/research/map-event-dialogue-state.md"
_REQUEST_STATE_FIXTURE_ID = "sf2-map-event-request-state-static-v1"
_REQUEST_STATE_DOCUMENT = "docs/research/map-event-request-state.md"


def load_json(path):
    value = _load_json(path)
    return normalize_later_owner_index(value) if path == INDEX else value
_DIALOGUE_STATE_OWNER_IDS = {
    "map.data.ms-map3-flag506-entityevents",
    "map.data.ms-map3-zoneevents",
    "map.data.ms-map5-flag530-entityevents",
    "map.data.ms-map5-flag650-entityevents",
    "map.data.ms-map6-flag701-entityevents",
    "map.data.ms-map16-flag530-entityevents",
    "map.data.ms-map18-entityevents",
    "map.data.ms-map19-flag506-entityevents",
    "map.data.ms-map20-flag543-zoneevents",
    "map.data.ms-map21-flag506-entityevents",
    "map.data.ms-map25-entityevents",
    "map.data.ms-map37-section5",
    "map.data.ms-map40-entityevents",
    "map.data.ms-map44-flag507-entityevents",
    "map.data.ms-map63-entityevents",
    "map.data.ms-map72-zoneevents",
    "map.data.ms-map77-section5",
}


def _fixture() -> dict[str, Any]:
    return load_json(FIXTURE)


def _without_request_state(index):
    normalized = deepcopy(index)
    removed: set[str] = set()
    for record in normalized["records"]:
        evidence = [
            item for item in record["evidence"] if item["fixtureId"] == _REQUEST_STATE_FIXTURE_ID
        ]
        if not evidence:
            continue
        assert evidence == [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-request-state-static-v1.json",
                "fixtureId": _REQUEST_STATE_FIXTURE_ID,
                "verifier": "src/sf2tool/h2/map_event_request_state.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            "eventRequestState.sourceFiles."
                            f"{record['symbol']}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
        assert record["documents"][-1] == _REQUEST_STATE_DOCUMENT
        record["evidence"].remove(evidence[0])
        record["documents"].pop()
        removed.add(record["id"])
    assert len(removed) == 24
    return normalized


def _without_request_consumption(index: dict[str, Any]) -> dict[str, Any]:
    for record in index["records"]:
        evidence = [
            item
            for item in record["evidence"]
            if item["fixtureId"] == "sf2-map-event-request-consumption-static-v1"
        ]
        if not evidence:
            continue
        assert len(evidence) == 1
        assert record["documents"].count("docs/research/map-event-request-consumption.md") == 1
        record["evidence"] = [item for item in record["evidence"] if item not in evidence]
        record["documents"].remove("docs/research/map-event-request-consumption.md")
        record["addresses"] = [
            address
            for address in record["addresses"]
            if address["id"]
            not in {
                "get-shop-inventory-address",
                "process-map-event",
                "declare-raft-entity",
                "raft-refresh",
            }
        ]
    return index

def _without_dialogue_state(index: dict[str, Any]) -> dict[str, Any]:
    normalized = deepcopy(index)
    removed: set[str] = set()
    for record in normalized["records"]:
        evidence = [
            item for item in record["evidence"] if item["fixtureId"] == _DIALOGUE_STATE_FIXTURE_ID
        ]
        if not evidence:
            continue
        assert record["id"] in _DIALOGUE_STATE_OWNER_IDS
        assert evidence == [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-dialogue-state-static-v1.json",
                "fixtureId": _DIALOGUE_STATE_FIXTURE_ID,
                "verifier": "src/sf2tool/h2/map_event_dialogue_state.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            "eventDialogueState.sourceFiles."
                            f"{record['symbol']}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
        assert record["documents"][-1] == _DIALOGUE_STATE_DOCUMENT
        record["evidence"].remove(evidence[0])
        record["documents"].pop()
        removed.add(record["id"])
    assert removed == _DIALOGUE_STATE_OWNER_IDS
    return normalized


def _without_predicate_results(index: dict[str, Any]) -> dict[str, Any]:
    normalized = deepcopy(index)
    removed: set[str] = set()
    for record in normalized["records"]:
        evidence = [
            item for item in record["evidence"] if item["fixtureId"] == PREDICATE_FIXTURE_ID
        ]
        if not evidence:
            continue
        assert evidence == [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-predicate-results-static-v1.json",
                "fixtureId": PREDICATE_FIXTURE_ID,
                "verifier": "src/sf2tool/h2/map_event_predicate_results.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            "eventPredicateResults.sourceFiles."
                            f"{record['symbol']}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
        assert record["documents"][-1] == PREDICATE_DOCUMENT
        record["evidence"].remove(evidence[0])
        record["documents"].pop()
        removed.add(record["id"])
    assert len(removed) == 15
    return normalized


def _projection(parent: dict[str, Any]) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any]]:
    return _handoff_projection(
        parent,
        load_json(ROOT / "tests/fixtures/h2/map-event-direct-state-static-v1.json"),
        load_json(ROOT / "tests/fixtures/h2/map-event-direct-control-static-v1.json"),
        upstream_path=UPSTREAM,
        rom_path=ROM,
    )


def _program(parent: dict[str, Any], handoff: dict[str, Any]) -> dict[str, Any]:
    context = _fixture()["eventDirectHandoff"]["programContexts"][handoff["programContextId"]]
    field = {
        "entityEvents": "entityTargetPrograms",
        "zoneEvents": "zoneTargetPrograms",
        "itemEvents": "itemTargetPrograms",
    }[context["category"]]
    return next(
        program
        for program in parent[field]
        if program["canonicalSymbol"] == context["programSymbol"]
        and program["entryAddress"] == context["programEntryAddress"]
    )


def _operation(parent: dict[str, Any], handoff: dict[str, Any], address: int) -> dict[str, Any]:
    return next(
        item for item in _program(parent, handoff)["operations"] if item["address"] == address
    )


def _first_setup() -> tuple[dict[str, Any], dict[str, Any], dict[str, Any]]:
    fixture = _fixture()["eventDirectHandoff"]
    handoff = next(row for row in fixture["transferHandoffs"].values() if row["setupOperationIds"])
    setup = fixture["setupOperations"][handoff["setupOperationIds"][0]]
    physical = fixture["physicalOperations"][setup["physicalOperationId"]]
    return handoff, setup, physical


def test_handoff_projection_has_exact_static_denominators_and_fixture() -> None:
    fixture = _fixture()
    handoff, summary, source_context = _projection(load_map_events_fixture()["expected"])

    assert summary == {
        "sourceIdentityCount": 54,
        "programContextCount": 914,
        "contextTransferSiteCount": 205,
        "setupEmptyTransferCount": 56,
        "setupOneOperationTransferCount": 118,
        "setupTwoOperationTransferCount": 29,
        "setupFourOperationTransferCount": 2,
        "nonemptySetupTransferCount": 149,
        "contextSetupOperationCount": 184,
        "physicalSetupOperationCount": 177,
        "contextCallContinuationCount": 143,
        "physicalCallContinuationCount": 139,
        "contextOperationCount": 327,
        "physicalOperationCount": 299,
        "contextualPhysicalOverlapCount": 17,
        "symbolicImmediateIdentityCount": 78,
        "symbolicImmediateUseCount": 120,
    }
    assert handoff == fixture["eventDirectHandoff"]
    assert source_context == fixture["sourceContext"]
    retained = fixture["retainedOwners"]
    assert (
        retained["mapEvents"]["outputSha256"]
        == load_json(ROOT / "manifests/extractions/map-events-static.json")["outputSha256"]
    )
    for owner, path in (
        ("eventDirectState", ROOT / "tests/fixtures/h2/map-event-direct-state-static-v1.json"),
        ("eventDirectControl", ROOT / "tests/fixtures/h2/map-event-direct-control-static-v1.json"),
    ):
        assert (
            retained[owner]["outputSha256"]
            == hashlib.sha256(
                json.dumps(
                    load_json(path), ensure_ascii=False, sort_keys=True, separators=(",", ":")
                ).encode("utf-8")
                + b"\n"
            )
            .hexdigest()
            .upper()
        )
    assert Counter(
        len(row["setupOperationIds"]) for row in handoff["transferHandoffs"].values()
    ) == {
        0: 56,
        1: 118,
        2: 29,
        4: 2,
    }
    assert Counter(row["kind"] for row in handoff["callContinuations"].values()) == {
        "ordinary": 72,
        "return": 57,
        "direct-call": 6,
        "unconditional-branch": 6,
        "conditional-branch": 1,
        "direct-jump": 1,
    }
    assert sum(len(row["uses"]) for row in handoff["symbolicImmediates"].values()) == 120


def test_mother_corpus_denominator_mutation_fails_before_handoff_fixture_comparison() -> None:
    parent = deepcopy(load_map_events_fixture()["expected"])
    parent["itemTargetPrograms"] = parent["itemTargetPrograms"][:-1]
    with pytest.raises(ValueError, match="itemEvents denominator drift"):
        _mother_corpus_projection(parent)


@pytest.mark.parametrize(
    ("builder_name", "fixture_path", "expected_message"),
    (
        (
            "build_map_events_contract",
            ROOT / "tests/fixtures/h2/map-events-static-v1.json",
            "retained map-events projection drift",
        ),
        (
            "build_map_event_direct_state_contract",
            ROOT / "tests/fixtures/h2/map-event-direct-state-static-v1.json",
            "retained direct-state projection drift",
        ),
        (
            "build_map_event_direct_control_contract",
            ROOT / "tests/fixtures/h2/map-event-direct-control-static-v1.json",
            "retained direct-control projection drift",
        ),
    ),
)
def test_retained_owner_projection_drift_fails_before_handoff_derivation(
    monkeypatch: pytest.MonkeyPatch,
    builder_name: str,
    fixture_path: Path,
    expected_message: str,
) -> None:
    map_events = load_map_events_fixture()["expected"]
    direct_state = load_json(ROOT / "tests/fixtures/h2/map-event-direct-state-static-v1.json")
    direct_control = load_json(ROOT / "tests/fixtures/h2/map-event-direct-control-static-v1.json")
    replacement = {
        "build_map_events_contract": map_events,
        "build_map_event_direct_state_contract": direct_state,
        "build_map_event_direct_control_contract": direct_control,
    }
    drifted = deepcopy(
        map_events if builder_name == "build_map_events_contract" else load_json(fixture_path)
    )
    drifted["summary"][next(iter(drifted["summary"]))] = -1
    replacement[builder_name] = drifted

    for name, value in replacement.items():
        monkeypatch.setattr(handoff_module, name, lambda _rom, _upstream, value=value: value)
    original_load_json = handoff_module.load_json

    def retained_load_json(path: Path) -> dict[str, Any]:
        value = original_load_json(path)
        if path == handoff_module.MAP_EVENTS_MANIFEST:
            value = deepcopy(value)
            value["outputSha256"] = (
                hashlib.sha256(handoff_module._map_events_canonical_bytes(map_events))
                .hexdigest()
                .upper()
            )
        return value

    monkeypatch.setattr(handoff_module, "load_json", retained_load_json)
    with pytest.raises(ValueError, match=expected_message):
        handoff_module._fresh_retained_owners(ROM, UPSTREAM)


@pytest.mark.parametrize("mutation", ("opcode", "size", "operands"))
def test_setup_opcode_size_and_operand_mutations_fail_before_fixture_comparison(
    mutation: str,
) -> None:
    parent = deepcopy(load_map_events_fixture()["expected"])
    handoff, _setup, physical = _first_setup()
    operation = _operation(parent, handoff, physical["romPc"])
    if mutation == "opcode":
        operation["sourceMnemonic"] = "nop.b"
        operation["mnemonic"] = "nop"
        expected = "H1 opcode/operand/order drift"
    elif mutation == "size":
        operation["sizeSuffix"] = ".w"
        expected = "source mnemonic/size drift"
    else:
        assert len(operation["operandTexts"]) == 2
        operation["operandTexts"].reverse()
        expected = "H1 opcode/operand/order drift"
    with pytest.raises(ValueError, match=expected):
        _projection(parent)


def test_missing_and_extra_immediate_setup_groups_fail_before_fixture_comparison() -> None:
    fixture = _fixture()["eventDirectHandoff"]
    parent = deepcopy(load_map_events_fixture()["expected"])
    one = next(
        row for row in fixture["transferHandoffs"].values() if len(row["setupOperationIds"]) == 1
    )
    setup = fixture["setupOperations"][one["setupOperationIds"][0]]
    missing = _operation(
        parent, one, fixture["physicalOperations"][setup["physicalOperationId"]]["romPc"]
    )
    missing["family"] = "event-service-macro"
    with pytest.raises(ValueError, match="setup distribution drift"):
        _projection(parent)

    parent = deepcopy(load_map_events_fixture()["expected"])
    for zero in fixture["transferHandoffs"].values():
        if zero["setupOperationIds"]:
            continue
        program = _program(parent, zero)
        transfer_order = zero["directControlTransferSiteOrder"]
        transfer = next(
            row
            for row in load_json(
                ROOT / "tests/fixtures/h2/map-event-direct-control-static-v1.json"
            )["eventDirectControl"]["transferSites"]
            if row["siteOrder"] == transfer_order
        )
        index = next(
            index
            for index, item in enumerate(program["operations"])
            if item["address"] == transfer["romPc"]
        )
        if index and program["operations"][index - 1]["family"] == "raw-68000-control-flow":
            program["operations"][index - 1]["family"] = "raw-68000-instruction"
            break
    else:
        raise AssertionError(
            "a zero-setup transfer with an immediately preceding raw control flow was not found"
        )
    with pytest.raises(ValueError, match="setup distribution drift"):
        _projection(parent)


@pytest.mark.parametrize("mutation", ("h1", "rom", "enum", "continuation"))
def test_h1_rom_enum_and_continuation_mutations_fail_before_fixture_comparison(
    mutation: str,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    handoff, _setup, physical = _first_setup()
    original_read_text = Path.read_text
    if mutation == "h1":
        listing_path = (UPSTREAM / "build/sf2build-h1.lst").resolve()
        prefix = f"{physical['romPc']:08X} "

        def h1_drift(path: Path, *args: object, **kwargs: object) -> str:
            text = original_read_text(path, *args, **kwargs)
            if path.resolve() != listing_path:
                return text
            lines = text.splitlines()
            for index, line in enumerate(lines):
                if line.startswith(prefix) and physical["sourceMnemonic"] in line:
                    lines[index] = line.replace(physical["sourceMnemonic"], "nop", 1)
                    return "\n".join(lines) + "\n"
            raise AssertionError("known H1 setup row was not found")

        monkeypatch.setattr(Path, "read_text", h1_drift)
        expected = "H1 opcode/operand/order drift"
        parent = load_map_events_fixture()["expected"]
    elif mutation == "rom":
        rom_path = ROM.resolve()
        original_read_bytes = Path.read_bytes

        def rom_drift(path: Path) -> bytes:
            data = original_read_bytes(path)
            if path.resolve() != rom_path:
                return data
            changed = bytearray(data)
            changed[physical["romPc"]] ^= 1
            return bytes(changed)

        monkeypatch.setattr(Path, "read_bytes", rom_drift)
        expected = "H1/ROM instruction-byte drift"
        parent = load_map_events_fixture()["expected"]
    elif mutation == "enum":
        enums_path = (UPSTREAM / "disasm/sf2enums.asm").resolve()

        def enum_drift(path: Path, *args: object, **kwargs: object) -> str:
            text = original_read_text(path, *args, **kwargs)
            if path.resolve() == enums_path:
                return text.replace("SHOP_ITEM_BEDOE: equ 20", "SHOP_ITEM_BEDOE: equ 21", 1)
            return text

        monkeypatch.setattr(Path, "read_text", enum_drift)
        expected = "authoritative enum value drift"
        parent = load_map_events_fixture()["expected"]
    else:
        parent = deepcopy(load_map_events_fixture()["expected"])
        continuation = next(iter(_fixture()["eventDirectHandoff"]["callContinuations"].values()))
        transfer = _fixture()["eventDirectHandoff"]["transferHandoffs"][
            continuation["transferHandoffId"]
        ]
        program = _program(parent, transfer)
        direct = load_json(ROOT / "tests/fixtures/h2/map-event-direct-control-static-v1.json")[
            "eventDirectControl"
        ]["transferSites"][continuation["directControlTransferSiteOrder"]]
        index = next(
            index
            for index, item in enumerate(program["operations"])
            if item["address"] == direct["romPc"]
        )
        program["operations"][index + 1]["family"] = "raw-68000-instruction"
        expected = "continuation kind distribution drift"

    with pytest.raises(ValueError, match=expected):
        _projection(parent)


def test_continuation_operand_branch_and_first_consumer_order_mutations_fail() -> None:
    fixture = _fixture()["eventDirectHandoff"]
    direct_control = load_json(ROOT / "tests/fixtures/h2/map-event-direct-control-static-v1.json")[
        "eventDirectControl"
    ]

    parent = deepcopy(load_map_events_fixture()["expected"])
    direct_call = next(
        row for row in fixture["callContinuations"].values() if row["kind"] == "direct-call"
    )
    transfer = fixture["transferHandoffs"][direct_call["transferHandoffId"]]
    program = _program(parent, transfer)
    call = direct_control["transferSites"][direct_call["directControlTransferSiteOrder"]]
    index = next(
        index
        for index, operation in enumerate(program["operations"])
        if operation["address"] == call["romPc"]
    )
    program["operations"][index + 1]["operandTexts"] = ["NotTheRetainedCallee"]
    with pytest.raises(ValueError, match="H1 opcode/operand/order drift"):
        _projection(parent)

    parent = deepcopy(load_map_events_fixture()["expected"])
    conditional = next(
        row for row in fixture["callContinuations"].values() if row["kind"] == "conditional-branch"
    )
    transfer = fixture["transferHandoffs"][conditional["transferHandoffId"]]
    program = _program(parent, transfer)
    call = direct_control["transferSites"][conditional["directControlTransferSiteOrder"]]
    index = next(
        index
        for index, operation in enumerate(program["operations"])
        if operation["address"] == call["romPc"]
    )
    program["operations"][index + 1]["target"]["instructionTargetAddress"] += 2
    with pytest.raises(ValueError, match="H1/ROM instruction-byte drift"):
        _projection(parent)

    parent = deepcopy(load_map_events_fixture()["expected"])
    continuation = next(iter(fixture["callContinuations"].values()))
    transfer = fixture["transferHandoffs"][continuation["transferHandoffId"]]
    program = _program(parent, transfer)
    call = direct_control["transferSites"][continuation["directControlTransferSiteOrder"]]
    index = next(
        index
        for index, operation in enumerate(program["operations"])
        if operation["address"] == call["romPc"]
    )
    program["operations"][index + 1]["sourceOrder"] += 1
    with pytest.raises(ValueError, match="program source order drift"):
        _projection(parent)


def test_fixture_schema_is_recursively_closed_ordered_and_public() -> None:
    fixture = _fixture()
    schema = load_json(SCHEMA)
    validate_json(fixture, SCHEMA, owner="map-event direct-handoff fixture")
    _validate_contract_order(fixture, schema)
    assert list(fixture) == [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "scope",
        "sourceContext",
        "retainedOwners",
        "eventDirectHandoff",
        "unknowns",
        "summary",
    ]
    assert list(fixture["eventDirectHandoff"]) == [
        "sourceFileOrder",
        "sourceFiles",
        "programContextOrder",
        "programContexts",
        "transferHandoffOrder",
        "transferHandoffs",
        "setupOperationOrder",
        "setupOperations",
        "callContinuationOrder",
        "callContinuations",
        "physicalOperationOrder",
        "physicalOperations",
        "symbolicImmediateOrder",
        "symbolicImmediates",
        "digests",
    ]
    assert fixture["unknowns"] == {key: "Unknown" for key in _UNKNOWN_KEYS}
    reusable_objects = [
        definition for definition in schema["$defs"].values() if definition.get("type") == "object"
    ]
    assert 1 + len(reusable_objects) + 7 == 36
    assert all(definition.get("additionalProperties") is False for definition in reusable_objects)
    handoff_schema = schema["$defs"]["eventDirectHandoff"]["properties"]
    record_maps = (
        ("sourceFiles", "sourceFileOrder", "sourceFile", 53),
        ("programContexts", "programContextOrder", "programContext", 914),
        ("transferHandoffs", "transferHandoffOrder", "transferHandoff", 205),
        ("setupOperations", "setupOperationOrder", "setupOperation", 184),
        ("callContinuations", "callContinuationOrder", "callContinuation", 143),
        ("physicalOperations", "physicalOperationOrder", "physicalOperation", 299),
        ("symbolicImmediates", "symbolicImmediateOrder", "symbolicImmediate", 78),
    )
    for record_field, order_field, item_definition, count in record_maps:
        record_schema = handoff_schema[record_field]
        assert "properties" not in record_schema
        assert "required" not in record_schema
        assert record_schema["propertyNames"]["enum"] == list(
            fixture["eventDirectHandoff"][record_field]
        )
        assert record_schema["minProperties"] == record_schema["maxProperties"] == count
        assert record_schema["additionalProperties"] == {"$ref": f"#/$defs/{item_definition}"}
        assert handoff_schema[order_field] == {
            "type": "array",
            "const": fixture["eventDirectHandoff"][order_field],
        }
    public = json.dumps(fixture, sort_keys=True).lower()
    for forbidden in (
        "local/",
        "bizhawk",
        "capture",
        "rawbytes",
        "runtimevalue",
        "calleeimplementation",
    ):
        assert forbidden not in public

    for mutator in (
        lambda value: value.__setitem__("privateRomBytes", "00"),
        lambda value: value["eventDirectHandoff"]["physicalOperations"].__setitem__("prose", {}),
        lambda value: value["eventDirectHandoff"]["physicalOperations"][
            value["eventDirectHandoff"]["physicalOperationOrder"][0]
        ].__setitem__("rawBytes", "00"),
    ):
        broken = deepcopy(fixture)
        mutator(broken)
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(broken, SCHEMA, owner="map-event direct-handoff fixture")

    for record_field, order_field, _item_definition, _count in record_maps:
        record_id = fixture["eventDirectHandoff"][order_field][0]
        missing = deepcopy(fixture)
        missing["eventDirectHandoff"][record_field].pop(record_id)
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(missing, SCHEMA, owner="map-event direct-handoff fixture")

        unknown = deepcopy(fixture)
        unknown_records = unknown["eventDirectHandoff"][record_field]
        record = unknown_records.pop(record_id)
        unknown_records["unknown:record"] = record
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(unknown, SCHEMA, owner="map-event direct-handoff fixture")

        duplicate = deepcopy(fixture)
        duplicate_order = duplicate["eventDirectHandoff"][order_field]
        duplicate_order[1] = duplicate_order[0]
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(duplicate, SCHEMA, owner="map-event direct-handoff fixture")

        reordered_order = deepcopy(fixture)
        order = reordered_order["eventDirectHandoff"][order_field]
        order[0], order[1] = order[1], order[0]
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(reordered_order, SCHEMA, owner="map-event direct-handoff fixture")

    reordered = deepcopy(fixture)
    first = reordered["eventDirectHandoff"]["physicalOperationOrder"][0]
    value = reordered["eventDirectHandoff"]["physicalOperations"].pop(first)
    reordered["eventDirectHandoff"]["physicalOperations"][first] = value
    with pytest.raises(ValueError, match="record order drift"):
        _validate_contract_order(reordered, schema)


def test_research_index_delta_is_exact_53_binding_append_without_record_or_address_drift() -> None:
    index = _without_request_consumption(
        _without_predicate_results(
            _without_dialogue_state(_without_request_state(load_json(INDEX)))
        )
    )
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
    symbols = set(_fixture()["eventDirectHandoff"]["sourceFiles"])
    changed = {record_id for record_id in records if records[record_id] != base_records[record_id]}
    owners = {record_id for record_id, record in records.items() if record["symbol"] in symbols}
    assert changed == owners and len(owners) == 53
    for record_id in owners:
        record, previous = records[record_id], base_records[record_id]
        assert record["addresses"] == previous["addresses"]
        assert record["documents"] == previous["documents"] + [DOCUMENT]
        assert record["evidence"] == previous["evidence"] + [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-direct-handoff-static-v1.json",
                "fixtureId": FIXTURE_ID,
                "verifier": VERIFIER,
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            f"eventDirectHandoff.sourceFiles.{record['symbol']}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
    normalized = deepcopy(index)
    removed_handoff_records: set[str] = set()
    for record in normalized["records"]:
        handoff_evidence = [
            evidence for evidence in record["evidence"] if evidence["fixtureId"] == FIXTURE_ID
        ]
        if record["id"] not in owners:
            assert handoff_evidence == []
            assert DOCUMENT not in record["documents"]
            continue
        assert len(handoff_evidence) == 1
        assert record["documents"].count(DOCUMENT) == 1
        assert record["documents"][-1] == DOCUMENT
        record["evidence"].remove(handoff_evidence[0])
        record["documents"].pop()
        removed_handoff_records.add(record["id"])
    assert removed_handoff_records == owners
    assert normalized == base
    assert json.dumps(normalized, ensure_ascii=False, indent=2) == json.dumps(
        base, ensure_ascii=False, indent=2
    )
    assert verify_index(UPSTREAM) == {
        "Index": "manifests/research-index.json",
        "Records": 1626,
        "Confirmed": 1626,
        "H2Fixtures": 95,
        "H3Fixtures": 94,
        "H3FixtureFiles": 94,
        "AddressBindings": 3019,
        "IndexedCodeFiles": 381,
        "IndexedDataFiles": 1017,
        "H1ListingRecords": 1589,
        "AlternateListingRecords": 37,
        "Z80MusicBankRecords": 37,
        "ResearchDocuments": 57,
        "DesignContracts": 68,
        "UpstreamSourcesChecked": True,
        "H1ListingChecked": True,
        "Status": "PASS",
    }


def test_research_index_schema_allows_handoff_root_and_rejects_near_misses() -> None:
    index = load_json(INDEX)
    validate_json(index, INDEX_SCHEMA, owner="map-event direct-handoff index")
    bindings = [
        binding
        for record in index["records"]
        for evidence in record["evidence"]
        if evidence["fixtureId"] == FIXTURE_ID
        for binding in evidence["bindings"]
    ]
    assert len(bindings) == 53
    assert all(
        binding["fixtureField"].startswith("eventDirectHandoff.sourceFiles.")
        and binding["fixtureField"].endswith(".tableEntryAddress")
        for binding in bindings
    )
    for field in (
        "unknownRoot.eventDirectHandoff",
        "sourceContext.eventDirectHandoff.sourceFiles.ms_map10_EntityEvents",
    ):
        broken = deepcopy(index)
        next(
            binding
            for record in broken["records"]
            for evidence in record["evidence"]
            if evidence["fixtureId"] == FIXTURE_ID
            for binding in evidence["bindings"]
        )["fixtureField"] = field
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(broken, INDEX_SCHEMA, owner="map-event direct-handoff index")


def test_handoff_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-event-direct-handoff"])
    assert args.h2_command == "map-event-direct-handoff"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None
