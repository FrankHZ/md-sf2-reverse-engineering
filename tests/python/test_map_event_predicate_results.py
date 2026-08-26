from __future__ import annotations

import hashlib
import json
import subprocess
from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.cli import build_parser
from sf2tool.h2 import map_event_predicate_results as predicate_module
from sf2tool.h2.map_event_direct_control import FIXTURE as DIRECT_CONTROL_FIXTURE
from sf2tool.h2.map_event_direct_state import FIXTURE as DIRECT_STATE_FIXTURE
from sf2tool.h2.map_event_predicate_results import (
    FIXTURE,
    ID,
    SCHEMA,
    _predicate_projection,
    _producer,
    _validate_contract_order,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.research_index import verify_index

ROOT = Path(__file__).resolve().parents[2]
ROM = ROOT / "local/roms/sf2-us.bin"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"
INDEX = ROOT / "manifests/research-index.json"
INDEX_SCHEMA = ROOT / "schemas/research-index.schema.json"
BASE = "02301229ef25a7f0a366f843c76042aceaf0524f"
DOCUMENT = "docs/research/map-event-predicate-results.md"
VERIFIER = "src/sf2tool/h2/map_event_predicate_results.py"
EXPECTED_INDEX_BINDINGS = {
    "map.data.ms-map10-flag722-entityevents": "ms_map10_flag722_EntityEvents",
    "map.data.ms-map11-entityevents": "ms_map11_EntityEvents",
    "map.data.ms-map20-flag543-zoneevents": "ms_map20_flag543_ZoneEvents",
    "map.data.ms-map22-section5": "ms_map22_Section5",
    "map.data.ms-map25-entityevents": "ms_map25_EntityEvents",
    "map.data.ms-map28-zoneevents": "ms_map28_ZoneEvents",
    "map.data.ms-map3-flag506-entityevents": "ms_map3_flag506_EntityEvents",
    "map.data.ms-map3-flag543-entityevents": "ms_map3_flag543_EntityEvents",
    "map.data.ms-map31-flag830-entityevents": "ms_map31_flag830_EntityEvents",
    "map.data.ms-map44-flag507-entityevents": "ms_map44_flag507_EntityEvents",
    "map.data.ms-map6-flag701-entityevents": "ms_map6_flag701_EntityEvents",
    "map.data.ms-map63-entityevents": "ms_map63_EntityEvents",
    "map.data.ms-map67-zoneevents": "ms_map67_ZoneEvents",
    "map.data.ms-map72-zoneevents": "ms_map72_ZoneEvents",
    "map.data.ms-map9-entityevents": "ms_map9_EntityEvents",
}


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _projection(
    parent: dict[str, object] | None = None,
    *,
    direct_state: dict[str, object] | None = None,
    direct_control: dict[str, object] | None = None,
) -> tuple[dict[str, object], dict[str, object], dict[str, object]]:
    return _predicate_projection(
        load_map_events_fixture()["expected"] if parent is None else parent,
        direct_state=load_json(DIRECT_STATE_FIXTURE) if direct_state is None else direct_state,
        direct_control=load_json(DIRECT_CONTROL_FIXTURE)
        if direct_control is None
        else direct_control,
        upstream_path=UPSTREAM,
        rom_path=ROM,
    )


def test_predicate_result_projection_has_exact_denominators_and_fixture() -> None:
    predicates, summary, source_context = _projection()

    assert summary == {
        "programContextCount": 914,
        "operationCount": 3579,
        "conditionalContextCount": 340,
        "conditionalPhysicalCount": 336,
        "directFlagContextExclusionCount": 316,
        "directFlagPhysicalExclusionCount": 314,
        "contextPairCount": 24,
        "physicalPairCount": 22,
        "positiveProgramContextCount": 19,
        "zeroProgramContextCount": 895,
        "sourceFileCount": 15,
        "sourceIdentityCount": 24,
        "physicalCallerAnchorCount": 59,
        "entrySeamCount": 8,
    }
    assert predicates == _fixture()["eventPredicateResults"]
    assert source_context == _fixture()["sourceContext"]
    assert [row["contextPairCount"] for row in predicates["categorySummaries"]] == [18, 5, 1]
    assert [row["physicalPairCount"] for row in predicates["categorySummaries"]] == [16, 5, 1]
    assert [
        (row["symbol"], row["contextPairCount"], row["physicalPairCount"])
        for row in predicates["resultOriginCohorts"]
    ] == [
        ("j_YesNoPrompt", 8, 8),
        ("j_GetItemInventoryLocation", 7, 6),
        ("EVENT_RELATIVE_POSITION", 5, 5),
        ("ReceiveMandatoryItem", 2, 1),
        ("j_GetCurrentHp", 1, 1),
        ("ENTITY_FACING", 1, 1),
    ]


def test_source_opcode_and_branch_polarity_mutations_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    pair = _fixture()["eventPredicateResults"]["pairs"][0]
    source_path = (UPSTREAM / "disasm" / pair["sourcePath"]).resolve()
    original_read_text = Path.read_text

    for role, replacement in (
        ("producer", "        nop"),
        ("branch", "        beq.s Map10_2D2_EntityEvent19_1"),
    ):

        def drifted_read_text(
            path: Path,
            *args: object,
            role: str = role,
            replacement: str = replacement,
            **kwargs: object,
        ) -> str:
            text = original_read_text(path, *args, **kwargs)
            if path.resolve() != source_path:
                return text
            lines = text.splitlines()
            lines[pair[role]["sourceLine"] - 1] = replacement
            return "\n".join(lines) + "\n"

        monkeypatch.setattr(Path, "read_text", drifted_read_text)
        with pytest.raises(ValueError, match="source opcode/operand drift"):
            _projection()
        monkeypatch.setattr(Path, "read_text", original_read_text)


def test_direct_state_operand_and_result_origin_target_mutations_fail() -> None:
    direct_state = deepcopy(load_json(DIRECT_STATE_FIXTURE))
    state_site = next(
        row
        for row in direct_state["eventDirectState"]["accessSites"]
        if row["symbol"] == "EVENT_RELATIVE_POSITION"
    )
    state_site["symbol"] = "ENTITY_FACING"
    with pytest.raises(ValueError, match="retained direct-state operand drift"):
        _projection(direct_state=direct_state)

    direct_control = deepcopy(load_json(DIRECT_CONTROL_FIXTURE))
    transfer = next(
        row
        for row in direct_control["eventDirectControl"]["transferSites"]
        if row["instructionTargetSymbol"] == "j_YesNoPrompt"
    )
    transfer["effectiveTargetSymbol"] = "GetCurrentHp"
    with pytest.raises(ValueError, match="result-origin effective target drift"):
        _projection(direct_control=direct_control)


def test_predicate_producer_parser_rejects_near_misses() -> None:
    base = {
        "sourceOrder": 1,
        "sourceLine": 1,
        "address": 2,
        "sourceMnemonic": "cmpi.b",
    }
    for operation, message in (
        (
            {**base, "mnemonic": "cmpi", "sizeSuffix": ".l", "operandTexts": ["#-1", "d0"]},
            "cmpi predicate shape drift",
        ),
        (
            {**base, "mnemonic": "tst", "sizeSuffix": ".b", "operandTexts": ["d0", "d1"]},
            "tst predicate shape drift",
        ),
        (
            {**base, "mnemonic": "btst", "sizeSuffix": ".b", "operandTexts": ["#0", "d0"]},
            "btst predicate shape drift",
        ),
        (
            {**base, "mnemonic": "bsr", "sizeSuffix": None, "operandTexts": ["j_YesNoPrompt"]},
            "unclassified predicate producer",
        ),
    ):
        with pytest.raises(ValueError, match=message):
            _producer(operation)


def test_retained_owner_fixture_guard_rejects_fresh_owner_drift(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    map_events = load_map_events_fixture()["expected"]
    direct_state = deepcopy(load_json(DIRECT_STATE_FIXTURE))
    direct_state["summary"]["sourceFileCount"] = -1
    monkeypatch.setattr(
        predicate_module, "build_map_events_contract", lambda _rom, _upstream: map_events
    )
    monkeypatch.setattr(
        predicate_module,
        "build_map_event_direct_state_contract",
        lambda _rom, _upstream: direct_state,
    )
    original_load_json = predicate_module.load_json

    def retained_load_json(path: Path) -> dict[str, object]:
        value = original_load_json(path)
        if path == predicate_module.MAP_EVENTS_MANIFEST:
            value = deepcopy(value)
            value["outputSha256"] = (
                hashlib.sha256(predicate_module._map_events_canonical_bytes(map_events))
                .hexdigest()
                .upper()
            )
        return value

    monkeypatch.setattr(predicate_module, "load_json", retained_load_json)
    with pytest.raises(ValueError, match="retained direct-state fixture drift"):
        predicate_module._fresh_retained_owners(ROM, UPSTREAM)


def test_fixture_schema_is_recursively_closed_ordered_and_public() -> None:
    fixture = _fixture()
    schema = load_json(SCHEMA)
    validate_json(fixture, SCHEMA, owner="map-event predicate-results fixture")
    _validate_contract_order(fixture)
    assert fixture["unknowns"] == {
        key: "Unknown"
        for key in (
            "naturalProgramReachability",
            "callerEntryRegisterAndState",
            "actualYesNoPromptResult",
            "actualInventoryLocationResult",
            "actualMandatoryItemResult",
            "actualCurrentHpResult",
            "actualEventRelativePosition",
            "actualEntityFacing",
            "actualCcrAndPredicateEvaluation",
            "actualBranchSelection",
            "successorExecutionAndSideEffects",
            "tailAndReturnState",
            "crossMapStateLifetime",
            "saveLoadPersistence",
            "inputUiDialogueAudioTimingAndStoryMeaning",
        )
    }
    object_defs = [value for value in schema["$defs"].values() if value.get("type") == "object"]
    assert all(value.get("additionalProperties") is False for value in object_defs)
    public = json.dumps(fixture, sort_keys=True).lower()
    for forbidden in ("local/", "rawbytes", "private", "runtimevalue", "calleeimplementation"):
        assert forbidden not in public

    for mutator in (
        lambda value: value.__setitem__("privateRomBytes", "00"),
        lambda value: value["eventPredicateResults"]["pairs"][0]["branch"].__setitem__(
            "runtimeSelection", True
        ),
        lambda value: value["eventPredicateResults"]["pairs"][0]["producer"].__setitem__(
            "rawBytes", "00"
        ),
        lambda value: value["eventPredicateResults"]["sourceFiles"].__setitem__("prose", {}),
    ):
        broken = deepcopy(fixture)
        mutator(broken)
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(broken, SCHEMA, owner="map-event predicate-results fixture")

    reordered = deepcopy(fixture)
    reordered["eventPredicateResults"]["pairOrder"][:2] = reversed(
        reordered["eventPredicateResults"]["pairOrder"][:2]
    )
    with pytest.raises(ValueError, match="pair order drift"):
        _validate_contract_order(reordered)


def test_research_index_delta_is_exact_15_binding_append_without_object_or_design_drift() -> None:
    index = load_json(INDEX)
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
    assert changed == set(EXPECTED_INDEX_BINDINGS)
    for record_id, symbol in EXPECTED_INDEX_BINDINGS.items():
        record, previous = records[record_id], base_records[record_id]
        assert record["symbol"] == symbol
        assert record["addresses"] == previous["addresses"]
        assert record["documents"] == previous["documents"] + [DOCUMENT]
        assert record["evidence"] == previous["evidence"] + [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-predicate-results-static-v1.json",
                "fixtureId": ID,
                "verifier": VERIFIER,
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            f"eventPredicateResults.sourceFiles.{symbol}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
    for record_id in set(records) - set(EXPECTED_INDEX_BINDINGS):
        assert records[record_id] == base_records[record_id]
    assert verify_index(UPSTREAM) == {
        "Index": "manifests/research-index.json",
        "Records": 1625,
        "Confirmed": 1625,
        "H2Fixtures": 89,
        "H3Fixtures": 94,
        "H3FixtureFiles": 94,
        "AddressBindings": 2927,
        "IndexedCodeFiles": 381,
        "IndexedDataFiles": 1017,
        "H1ListingRecords": 1588,
        "AlternateListingRecords": 37,
        "Z80MusicBankRecords": 37,
        "ResearchDocuments": 51,
        "DesignContracts": 68,
        "UpstreamSourcesChecked": True,
        "H1ListingChecked": True,
        "Status": "PASS",
    }


def test_research_index_schema_allows_only_exact_predicate_result_bindings() -> None:
    index = load_json(INDEX)
    validate_json(index, INDEX_SCHEMA, owner="map-event predicate-results index")
    bindings = [
        binding
        for record in index["records"]
        for evidence in record["evidence"]
        if evidence["fixtureId"] == ID
        for binding in evidence["bindings"]
    ]
    assert len(bindings) == 15
    assert {binding["fixtureField"] for binding in bindings} == {
        f"eventPredicateResults.sourceFiles.{symbol}.tableEntryAddress"
        for symbol in EXPECTED_INDEX_BINDINGS.values()
    }
    for fixture_field in (
        "unknownRoot.eventPredicateResults",
        "eventPredicateResults.sourceFiles.ms_map9_EntityEvents.unknown",
        "sourceContext.eventPredicateResults.sourceFiles.ms_map9_EntityEvents.tableEntryAddress",
    ):
        broken = deepcopy(index)
        next(
            binding
            for record in broken["records"]
            for evidence in record["evidence"]
            if evidence["fixtureId"] == ID
            for binding in evidence["bindings"]
        )["fixtureField"] = fixture_field
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(broken, INDEX_SCHEMA, owner="map-event predicate-results index")


def test_predicate_results_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-event-predicate-results"])
    assert args.h2_command == "map-event-predicate-results"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None
