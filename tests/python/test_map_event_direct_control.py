from __future__ import annotations

import json
import subprocess
from collections import Counter
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.cli import build_parser
from sf2tool.h2.map_event_direct_control import (
    _UNKNOWN_KEYS,
    _direct_control_projection,
    _mother_corpus_projection,
)
from sf2tool.h2.map_event_item_transactions import (
    normalize_map_event_item_transactions_later_owner_index as normalize_later_owner_index,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json as _load_json
from sf2tool.jsonio import validate_json
from sf2tool.research_index import verify_index

ROOT = Path(__file__).resolve().parents[2]
UPSTREAM = ROOT / "local/upstream/SF2DISASM"
ROM = ROOT / "local/roms/sf2-us.bin"
FIXTURE = ROOT / "tests/fixtures/h2/map-event-direct-control-static-v1.json"
SCHEMA = ROOT / "schemas/h2/map-event-direct-control-static-fixture.schema.json"
INDEX = ROOT / "manifests/research-index.json"
INDEX_SCHEMA = ROOT / "schemas/research-index.schema.json"
BASE = "76895279f133c2d49ba10a71196d85429378fd6d"
FIXTURE_ID = "sf2-map-event-direct-control-static-v1"
DOCUMENT = "docs/research/map-event-direct-control.md"
VERIFIER = "src/sf2tool/h2/map_event_direct_control.py"
HANDOFF_FIXTURE_ID = "sf2-map-event-direct-handoff-static-v1"
HANDOFF_DOCUMENT = "docs/research/map-event-direct-handoff.md"
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
    return _direct_control_projection(parent, upstream_path=UPSTREAM, rom_path=ROM)


def _first_site(category: str) -> dict[str, Any]:
    return next(
        site
        for site in _fixture()["eventDirectControl"]["transferSites"]
        if site["category"] == category
    )


def _program(parent: dict[str, Any], site: dict[str, Any]) -> dict[str, Any]:
    field = {
        "entityEvents": "entityTargetPrograms",
        "zoneEvents": "zoneTargetPrograms",
        "itemEvents": "itemTargetPrograms",
    }[site["category"]]
    return next(
        program for program in parent[field] if program["canonicalSymbol"] == site["programSymbol"]
    )


def _assert_recursively_closed(schema: dict[str, Any], node: dict[str, Any]) -> None:
    if "$ref" in node:
        reference = node["$ref"]
        assert reference.startswith("#/$defs/")
        _assert_recursively_closed(schema, schema["$defs"][reference.rsplit("/", 1)[-1]])
    if node.get("type") == "object":
        assert node["additionalProperties"] is False
        for child in node.get("properties", {}).values():
            _assert_recursively_closed(schema, child)
    if node.get("type") == "array" and isinstance(node.get("items"), dict):
        _assert_recursively_closed(schema, node["items"])


def test_direct_control_projection_is_complete_zero_inclusive_and_matches_fixture() -> None:
    fixture = _fixture()
    direct_control, summary, source_context = _projection(load_map_events_fixture()["expected"])

    assert summary == {
        "programContextCount": 914,
        "positiveProgramContextCount": 154,
        "zeroProgramContextCount": 760,
        "contextTransferSiteCount": 205,
        "physicalTransferSiteCount": 201,
        "directCallContextSiteCount": 143,
        "directJumpContextSiteCount": 62,
        "callerTableSourceCount": 53,
        "effectiveTargetIdentityCount": 35,
        "aliasJoinCount": 15,
        "ownerSourceIdentityCount": 81,
        "h1RomAnchorCount": 251,
        "callContinuationCount": 143,
        "tailTransferCount": 62,
    }
    assert direct_control == fixture["eventDirectControl"]
    assert source_context == fixture["sourceContext"]
    assert len(direct_control["programContexts"]) == 914
    assert sum(row["transferSiteCount"] == 0 for row in direct_control["programContexts"]) == 760
    assert Counter(row["kind"] for row in direct_control["callContinuations"]) == {
        "ordinary": 72,
        "return": 57,
        "direct-call": 6,
        "unconditional-branch": 6,
        "conditional-branch": 1,
        "direct-jump": 1,
    }
    assert {row["symbol"] for row in direct_control["effectiveTargets"]} == {
        "BlacksmithMenu",
        "CaravanMenu",
        "ChangeEntityFacing",
        "CheckRandomBattle",
        "ChurchMenu",
        "ClosePortraitEyes",
        "ClosePortraitWindow",
        "DisplayCurrentPortrait",
        "DisplayTacticalBaseQuote",
        "DisplayText",
        "ExecuteMapScript",
        "GenerateRandomNumber",
        "GetCurrentHp",
        "GetEntityPortaitAndSpeechSfx",
        "GetItemInventoryLocation",
        "GetMaxHp",
        "GetMaxMp",
        "GetRhodeFacing",
        "MakeEntityWalk",
        "MoveEntityOutOfMap",
        "NameAlly",
        "PlayEndingCredits",
        "PlayIntroOrEndCutscene",
        "ReceiveMandatoryItem",
        "RemoveItemBySlot",
        "RemoveItemFromInventory",
        "SetCurrentHp",
        "SetCurrentMp",
        "ShopMenu",
        "Sleep",
        "WaitForEntityToStopMoving",
        "WaitForViewScrollEnd",
        "WitchEnd",
        "YesNoPrompt",
        "sub_5A278",
    }


def test_mother_corpus_projection_rejects_a_program_or_operation_denominator_mutation() -> None:
    parent = deepcopy(load_map_events_fixture()["expected"])
    parent["entityTargetPrograms"] = parent["entityTargetPrograms"][:-1]
    with pytest.raises(ValueError, match="entityEvents denominator drift"):
        _mother_corpus_projection(parent)

    parent = deepcopy(load_map_events_fixture()["expected"])
    parent["zoneTargetPrograms"][0]["operations"] = []
    with pytest.raises(ValueError, match="zoneEvents denominator drift"):
        _mother_corpus_projection(parent)


@pytest.mark.parametrize("category", ["entityEvents", "zoneEvents", "itemEvents"])
def test_transfer_source_mutation_fails_for_each_event_category(
    category: str, monkeypatch: pytest.MonkeyPatch
) -> None:
    site = _first_site(category)
    source_path = (UPSTREAM / "disasm" / site["sourcePath"]).resolve()
    original_read_text = Path.read_text

    def drifted_read_text(path: Path, *args: object, **kwargs: object) -> str:
        text = original_read_text(path, *args, **kwargs)
        if path.resolve() != source_path:
            return text
        lines = text.splitlines()
        lines[site["sourceLine"] - 1] = "        nop"
        return "\n".join(lines) + "\n"

    monkeypatch.setattr(Path, "read_text", drifted_read_text)
    with pytest.raises(ValueError, match="source mnemonic/operand-order drift"):
        _projection(load_map_events_fixture()["expected"])


@pytest.mark.parametrize("category", ["entityEvents", "zoneEvents", "itemEvents"])
def test_transfer_h1_statement_mutation_fails_for_each_event_category(
    category: str, monkeypatch: pytest.MonkeyPatch
) -> None:
    site = _first_site(category)
    listing_path = (UPSTREAM / "build/sf2build-h1.lst").resolve()
    original_read_text = Path.read_text
    prefix = f"{site['romPc']:08X} "

    def drifted_read_text(path: Path, *args: object, **kwargs: object) -> str:
        text = original_read_text(path, *args, **kwargs)
        if path.resolve() != listing_path:
            return text
        lines = text.splitlines()
        for index, line in enumerate(lines):
            if line.startswith(prefix):
                lines[index] = line.replace(site["sourceMnemonic"], "nop", 1)
                return "\n".join(lines) + "\n"
        raise AssertionError("known transfer H1 row was not found")

    monkeypatch.setattr(Path, "read_text", drifted_read_text)
    with pytest.raises(ValueError, match="H1 mnemonic/operand-order drift"):
        _projection(load_map_events_fixture()["expected"])


@pytest.mark.parametrize("category", ["entityEvents", "zoneEvents", "itemEvents"])
def test_transfer_rom_mutation_fails_for_each_event_category(
    category: str, monkeypatch: pytest.MonkeyPatch
) -> None:
    site = _first_site(category)
    rom_path = ROM.resolve()
    original_read_bytes = Path.read_bytes

    def drifted_read_bytes(path: Path) -> bytes:
        data = original_read_bytes(path)
        if path.resolve() != rom_path:
            return data
        changed = bytearray(data)
        changed[site["romPc"]] ^= 1
        return bytes(changed)

    monkeypatch.setattr(Path, "read_bytes", drifted_read_bytes)
    with pytest.raises(ValueError, match="H1/ROM instruction-byte drift"):
        _projection(load_map_events_fixture()["expected"])


def test_alias_instruction_and_effective_target_mutations_fail_before_fixture_comparison() -> None:
    parent = load_map_events_fixture()["expected"]
    first = _first_site("entityEvents")
    for target_field in ("instructionTargetAddress", "effectiveTargetAddress"):
        drifted = deepcopy(parent)
        operation = next(
            item
            for item in _program(drifted, first)["operations"]
            if item["address"] == first["romPc"]
        )
        operation["target"][target_field] += 2
        with pytest.raises(ValueError, match="(?:missing H1 instruction|H1/ROM target drift)"):
            _projection(drifted)


def test_alias_definition_label_and_callee_entry_anchor_mutations_fail(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    fixture = _fixture()["eventDirectControl"]
    alias = fixture["aliasJoins"][0]
    alias_source = (UPSTREAM / "disasm" / alias["sourcePath"]).resolve()
    original_read_text = Path.read_text

    def drifted_alias(path: Path, *args: object, **kwargs: object) -> str:
        text = original_read_text(path, *args, **kwargs)
        if path.resolve() != alias_source:
            return text
        lines = text.splitlines()
        lines[alias["sourceLine"] - 1] = "j_not_the_retained_alias:"
        return "\n".join(lines) + "\n"

    monkeypatch.setattr(Path, "read_text", drifted_alias)
    with pytest.raises(ValueError, match="callee entry label drift"):
        _projection(load_map_events_fixture()["expected"])

    monkeypatch.undo()
    target = fixture["effectiveTargets"][0]
    target_source = (UPSTREAM / "disasm" / target["sourcePath"]).resolve()

    def drifted_target(path: Path, *args: object, **kwargs: object) -> str:
        text = original_read_text(path, *args, **kwargs)
        if path.resolve() != target_source:
            return text
        lines = text.splitlines()
        lines[target["sourceLine"] - 1] = "not_the_retained_callee:"
        return "\n".join(lines) + "\n"

    monkeypatch.setattr(Path, "read_text", drifted_target)
    with pytest.raises(ValueError, match="callee entry label drift"):
        _projection(load_map_events_fixture()["expected"])


def test_callee_first_instruction_and_shared_map6_contexts_are_guarded(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    fixture = _fixture()["eventDirectControl"]
    target = fixture["effectiveTargets"][0]
    rom_path = ROM.resolve()
    original_read_bytes = Path.read_bytes

    def drifted_read_bytes(path: Path) -> bytes:
        data = original_read_bytes(path)
        if path.resolve() != rom_path:
            return data
        changed = bytearray(data)
        changed[target["entryAddress"]] ^= 1
        return bytes(changed)

    monkeypatch.setattr(Path, "read_bytes", drifted_read_bytes)
    with pytest.raises(ValueError, match="H1/ROM instruction-byte drift"):
        _projection(load_map_events_fixture()["expected"])

    shared = [row for row in fixture["physicalSites"] if row["contextCount"] == 2]
    assert [row["romPc"] for row in shared] == [347214, 347244, 347258, 347306]
    sites = fixture["transferSites"]
    for row in shared:
        contexts = [sites[index]["programSymbol"] for index in row["contextSiteOrders"]]
        assert contexts == ["Map6_EntityEvent13", "Map6_DefaultEntityEvent"]


def test_continuation_and_tail_lexical_mutations_and_returning_tail_classification_fail() -> None:
    fixture = _fixture()["eventDirectControl"]
    parent = deepcopy(load_map_events_fixture()["expected"])
    call = fixture["callContinuations"][0]
    site = fixture["transferSites"][call["siteOrder"]]
    program = _program(parent, site)
    next_operation = program["operations"][
        next(
            index
            for index, item in enumerate(program["operations"])
            if item["address"] == site["romPc"]
        )
        + 1
    ]
    next_operation["mnemonic"] = "bra"
    with pytest.raises(ValueError, match="call continuation denominator drift"):
        _projection(parent)

    parent = deepcopy(load_map_events_fixture()["expected"])
    site = fixture["transferSites"][0]
    operation = next(
        item for item in _program(parent, site)["operations"] if item["address"] == site["romPc"]
    )
    operation["mnemonic"] = "jmp"
    with pytest.raises(ValueError, match="source/H1/ROM denominator drift"):
        _projection(parent)


def test_fixture_schema_is_recursively_closed_public_and_exact() -> None:
    fixture = _fixture()
    schema = load_json(SCHEMA)
    validate_json(fixture, SCHEMA, owner="map-event direct-control fixture")
    _assert_recursively_closed(schema, schema)
    assert list(fixture) == [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "scope",
        "sourceContext",
        "retainedMapEvents",
        "eventDirectControl",
        "unknowns",
        "summary",
    ]
    assert set(fixture["eventDirectControl"]) == {
        "sourceFiles",
        "programContexts",
        "transferSites",
        "physicalSites",
        "aliasJoins",
        "effectiveTargets",
        "callContinuations",
        "tailTransfers",
        "ownerJoins",
    }
    assert fixture["unknowns"] == {key: "Unknown" for key in _UNKNOWN_KEYS}
    public = json.dumps(fixture, sort_keys=True).lower()
    for forbidden in ("local/", "capture", "savestate", "movie", "bizhawk", "runtimevalue"):
        assert forbidden not in public
    for mutator in (
        lambda value: value.__setitem__("privateRomBytes", "00"),
        lambda value: value["eventDirectControl"]["transferSites"][0].__setitem__(
            "runtimeValue", 1
        ),
        lambda value: value["sourceContext"].__setitem__("eventDirectControl", {}),
    ):
        broken = deepcopy(fixture)
        mutator(broken)
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(broken, SCHEMA, owner="map-event direct-control fixture")


def test_research_index_delta_is_exact_53_object_append_without_record_or_address_drift() -> None:
    index = _without_request_consumption(
        _without_predicate_results(
            _without_dialogue_state(_without_request_state(load_json(INDEX)))
        )
    )
    removed_handoff_records: set[str] = set()
    for record in index["records"]:
        handoff = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] == HANDOFF_FIXTURE_ID
        ]
        if not handoff:
            continue
        assert handoff == [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-direct-handoff-static-v1.json",
                "fixtureId": HANDOFF_FIXTURE_ID,
                "verifier": "src/sf2tool/h2/map_event_direct_handoff.py",
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
        assert record["documents"].count(HANDOFF_DOCUMENT) == 1
        assert record["documents"][-1] == HANDOFF_DOCUMENT
        record["evidence"] = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] != HANDOFF_FIXTURE_ID
        ]
        record["documents"].remove(HANDOFF_DOCUMENT)
        removed_handoff_records.add(record["id"])
    assert len(removed_handoff_records) == 53
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
    table_symbols = set(_fixture()["eventDirectControl"]["sourceFiles"])
    owner_records = {
        record_id: record["symbol"]
        for record_id, record in records.items()
        if record["symbol"] in table_symbols
    }
    assert len(owner_records) == 53
    assert set(owner_records.values()) == table_symbols
    changed_records = {
        record_id for record_id in records if records[record_id] != base_records[record_id]
    }
    assert changed_records == set(owner_records)
    for record_id, table_symbol in owner_records.items():
        assert records[record_id]["addresses"] == base_records[record_id]["addresses"]
        assert records[record_id]["documents"] == base_records[record_id]["documents"] + [DOCUMENT]
        assert records[record_id]["evidence"] == base_records[record_id]["evidence"] + [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-direct-control-static-v1.json",
                "fixtureId": FIXTURE_ID,
                "verifier": VERIFIER,
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            f"eventDirectControl.sourceFiles.{table_symbol}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
    assert verify_index(UPSTREAM) == {
        "Index": "manifests/research-index.json",
        "Records": 1625,
        "Confirmed": 1625,
        "H2Fixtures": 94,
        "H3Fixtures": 94,
        "H3FixtureFiles": 94,
        "AddressBindings": 3007,
        "IndexedCodeFiles": 381,
        "IndexedDataFiles": 1017,
        "H1ListingRecords": 1588,
        "AlternateListingRecords": 37,
        "Z80MusicBankRecords": 37,
        "ResearchDocuments": 56,
        "DesignContracts": 68,
        "UpstreamSourcesChecked": True,
        "H1ListingChecked": True,
        "Status": "PASS",
    }


def test_research_index_schema_allows_exact_direct_control_root_and_rejects_near_misses() -> None:
    index = load_json(INDEX)
    validate_json(index, INDEX_SCHEMA, owner="map-event direct-control index")
    bindings = [
        binding
        for record in index["records"]
        for evidence in record["evidence"]
        if evidence["fixtureId"] == FIXTURE_ID
        for binding in evidence["bindings"]
    ]
    assert len(bindings) == 53
    assert all(
        binding["fixtureField"].startswith("eventDirectControl.sourceFiles.")
        and binding["fixtureField"].endswith(".tableEntryAddress")
        for binding in bindings
    )
    for fixture_field in (
        "unknownRoot.eventDirectControl",
        "sourceContext.eventDirectControl.sourceFiles.ms_map10_EntityEvents",
    ):
        broken = deepcopy(index)
        binding = next(
            binding
            for record in broken["records"]
            for evidence in record["evidence"]
            if evidence["fixtureId"] == FIXTURE_ID
            for binding in evidence["bindings"]
        )
        binding["fixtureField"] = fixture_field
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(broken, INDEX_SCHEMA, owner="map-event direct-control index")


def test_direct_control_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-event-direct-control"])
    assert args.h2_command == "map-event-direct-control"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None
