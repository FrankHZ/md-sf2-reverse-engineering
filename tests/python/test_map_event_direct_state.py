from __future__ import annotations

import json
import subprocess
from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.cli import build_parser
from sf2tool.h2.map_event_combatant_state import (
    normalize_map_event_combatant_state_later_owner_index as normalize_later_owner_index,
)
from sf2tool.h2.map_event_direct_state import (
    _direct_access_positions,
    _direct_state_projection,
    _mother_corpus_projection,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json as _load_json
from sf2tool.jsonio import validate_json

ROOT = Path(__file__).resolve().parents[2]
FIXTURE = ROOT / "tests/fixtures/h2/map-event-direct-state-static-v1.json"
SCHEMA = ROOT / "schemas/h2/map-event-direct-state-static-fixture.schema.json"
INDEX = ROOT / "manifests/research-index.json"
INDEX_SCHEMA = ROOT / "schemas/research-index.schema.json"
BASE = "18ee3120d8f67afb44e2da08c045c2a2fa6da88a"
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
EXPECTED_INDEX_BINDINGS = {
    "map.data.ms-map2-entityevents": "ms_map2_EntityEvents",
    "map.data.ms-map3-flag506-entityevents": "ms_map3_flag506_EntityEvents",
    "map.data.ms-map3-flag609-entityevents": "ms_map3_flag609_EntityEvents",
    "map.data.ms-map5-flag530-entityevents": "ms_map5_flag530_EntityEvents",
    "map.data.ms-map5-flag650-entityevents": "ms_map5_flag650_EntityEvents",
    "map.data.ms-map6-flag701-entityevents": "ms_map6_flag701_EntityEvents",
    "map.data.ms-map8-entityevents": "ms_map8_EntityEvents",
    "map.data.ms-map9-entityevents": "ms_map9_EntityEvents",
    "map.data.ms-map10-entityevents": "ms_map10_EntityEvents",
    "map.data.ms-map13-entityevents": "ms_map13_EntityEvents",
    "map.data.ms-map13-flag513-entityevents": "ms_map13_flag513_EntityEvents",
    "map.data.ms-map15-entityevents": "ms_map15_EntityEvents",
    "map.data.ms-map16-entityevents": "ms_map16_EntityEvents",
    "map.data.ms-map16-flag530-entityevents": "ms_map16_flag530_EntityEvents",
    "map.data.ms-map18-entityevents": "ms_map18_EntityEvents",
    "map.data.ms-map19-flag506-entityevents": "ms_map19_flag506_EntityEvents",
    "map.data.ms-map21-flag506-entityevents": "ms_map21_flag506_EntityEvents",
    "map.data.ms-map25-entityevents": "ms_map25_EntityEvents",
    "map.data.ms-map29-entityevents": "ms_map29_EntityEvents",
    "map.data.ms-map31-flag830-entityevents": "ms_map31_flag830_EntityEvents",
    "map.data.ms-map38-entityevents": "ms_map38_EntityEvents",
    "map.data.ms-map40-entityevents": "ms_map40_EntityEvents",
    "map.data.ms-map44-flag507-entityevents": "ms_map44_flag507_EntityEvents",
    "map.data.ms-map63-entityevents": "ms_map63_EntityEvents",
    "map.data.ms-map3-zoneevents": "ms_map3_ZoneEvents",
    "map.data.ms-map16-zoneevents": "ms_map16_ZoneEvents",
    "map.data.ms-map20-flag543-zoneevents": "ms_map20_flag543_ZoneEvents",
    "map.data.ms-map22-zoneevents": "ms_map22_ZoneEvents",
    "map.data.ms-map28-zoneevents": "ms_map28_ZoneEvents",
    "map.data.ms-map66-zoneevents": "ms_map66_ZoneEvents",
    "map.data.ms-map69-zoneevents": "ms_map69_ZoneEvents",
    "map.data.ms-map70-zoneevents": "ms_map70_ZoneEvents",
    "map.data.ms-map72-zoneevents": "ms_map72_ZoneEvents",
    "map.data.ms-map74-zoneevents": "ms_map74_ZoneEvents",
    "map.data.ms-map76-zoneevents": "ms_map76_ZoneEvents",
    "map.data.ms-map77-zoneevents": "ms_map77_ZoneEvents",
    "map.data.ms-map37-section5": "ms_map37_Section5",
    "map.data.ms-map77-section5": "ms_map77_Section5",
}


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


def _without_request_consumption(index: dict[str, object]) -> dict[str, object]:
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

def _without_dialogue_state(index: dict[str, object]) -> dict[str, object]:
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


def _without_predicate_results(index: dict[str, object]) -> dict[str, object]:
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


def test_mother_corpus_projection_is_zero_inclusive_before_direct_state_filtering() -> None:
    categories = {
        "entityEvents": (684, 2624, 183),
        "zoneEvents": (150, 809, 70),
        "itemEvents": (80, 146, 19),
    }
    programs: dict[str, list[dict[str, object]]] = {}
    for category, (program_count, operation_count, raw_count) in categories.items():
        operations = [{"family": "raw-68000-instruction"} for _ in range(raw_count)] + [
            {"family": "event-service-macro"} for _ in range(operation_count - raw_count)
        ]
        programs[category] = [
            {"operations": operations if index == 0 else []} for index in range(program_count)
        ]

    projection = _mother_corpus_projection(
        {
            "entityTargetPrograms": programs["entityEvents"],
            "zoneTargetPrograms": programs["zoneEvents"],
            "itemTargetPrograms": programs["itemEvents"],
        }
    )

    assert projection == {
        "categories": [
            {
                "category": "entityEvents",
                "programContextCount": 684,
                "operationCount": 2624,
                "rawInstructionContextCount": 183,
            },
            {
                "category": "zoneEvents",
                "programContextCount": 150,
                "operationCount": 809,
                "rawInstructionContextCount": 70,
            },
            {
                "category": "itemEvents",
                "programContextCount": 80,
                "operationCount": 146,
                "rawInstructionContextCount": 19,
            },
        ]
    }


def test_mother_corpus_projection_rejects_a_smallest_raw_instruction_mutation() -> None:
    malformed = {
        "entityTargetPrograms": [{"operations": []}] * 684,
        "zoneTargetPrograms": [{"operations": []}] * 150,
        "itemTargetPrograms": [{"operations": []}] * 80,
    }

    try:
        _mother_corpus_projection(malformed)
    except ValueError as error:
        assert "entityEvents denominator drift" in str(error)
    else:
        raise AssertionError("raw-instruction denominator mutation was accepted")


def test_direct_state_projection_has_exact_context_and_physical_denominators() -> None:
    event_direct_state, summary, _source_context, _mother = _direct_state_projection(
        load_map_events_fixture()["expected"],
        upstream_path=Path("local/upstream/SF2DISASM"),
        rom_path=Path("local/roms/sf2-us.bin"),
    )

    assert summary == {
        "sourceIdentityCount": 40,
        "programContextCount": 914,
        "positiveDirectProgramContextCount": 65,
        "zeroDirectProgramContextCount": 849,
        "contextInstructionSiteCount": 127,
        "physicalInstructionSiteCount": 124,
        "contextAccessSiteCount": 152,
        "physicalAccessSiteCount": 148,
        "symbolDefinitionCount": 13,
        "sourceFileCount": 38,
    }
    assert [row["contextAccessSiteCount"] for row in event_direct_state["categorySummaries"]] == [
        104,
        42,
        6,
    ]
    assert event_direct_state == load_json(FIXTURE)["eventDirectState"]


def test_direct_state_source_mutation_fails_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    parent = load_map_events_fixture()["expected"]
    fixture = load_json(FIXTURE)
    first_site = fixture["eventDirectState"]["accessSites"][0]
    source_path = (ROOT / "local/upstream/SF2DISASM/disasm" / first_site["sourcePath"]).resolve()
    original_read_text = Path.read_text

    def drifted_read_text(path: Path, *args: object, **kwargs: object) -> str:
        text = original_read_text(path, *args, **kwargs)
        if path.resolve() != source_path:
            return text
        lines = text.splitlines()
        lines[first_site["sourceLine"] - 1] = "        nop"
        return "\n".join(lines) + "\n"

    monkeypatch.setattr(Path, "read_text", drifted_read_text)
    with pytest.raises(ValueError, match="source mnemonic/operand-order drift"):
        _direct_state_projection(
            parent,
            upstream_path=ROOT / "local/upstream/SF2DISASM",
            rom_path=ROOT / "local/roms/sf2-us.bin",
        )


def test_direct_state_h1_statement_mutation_fails_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    parent = load_map_events_fixture()["expected"]
    first_site = load_json(FIXTURE)["eventDirectState"]["accessSites"][0]
    listing_path = (ROOT / "local/upstream/SF2DISASM/build/sf2build-h1.lst").resolve()
    original_read_text = Path.read_text
    target_prefix = f"{first_site['romPc']:08X} "

    def drifted_read_text(path: Path, *args: object, **kwargs: object) -> str:
        text = original_read_text(path, *args, **kwargs)
        if path.resolve() != listing_path:
            return text
        lines = text.splitlines()
        for index, line in enumerate(lines):
            if line.startswith(target_prefix) and "move.b" in line:
                lines[index] = line.replace("move.b", "move.w", 1)
                return "\n".join(lines) + "\n"
        raise AssertionError("known direct H1 instruction row was not found")

    monkeypatch.setattr(Path, "read_text", drifted_read_text)
    with pytest.raises(ValueError, match="H1 mnemonic/operand-order drift"):
        _direct_state_projection(
            parent,
            upstream_path=ROOT / "local/upstream/SF2DISASM",
            rom_path=ROOT / "local/roms/sf2-us.bin",
        )


def test_direct_state_rom_instruction_byte_mutation_fails_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    parent = load_map_events_fixture()["expected"]
    first_site = load_json(FIXTURE)["eventDirectState"]["accessSites"][0]
    rom_path = (ROOT / "local/roms/sf2-us.bin").resolve()
    original_read_bytes = Path.read_bytes

    def drifted_read_bytes(path: Path) -> bytes:
        data = original_read_bytes(path)
        if path.resolve() != rom_path:
            return data
        drifted = bytearray(data)
        drifted[first_site["romPc"]] ^= 1
        return bytes(drifted)

    monkeypatch.setattr(Path, "read_bytes", drifted_read_bytes)
    with pytest.raises(ValueError, match="H1/ROM instruction-byte drift"):
        _direct_state_projection(
            parent,
            upstream_path=ROOT / "local/upstream/SF2DISASM",
            rom_path=rom_path,
        )


def test_fixture_schema_is_closed_and_rejects_private_or_runtime_fields() -> None:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map-event direct-state fixture")

    for mutator in (
        lambda value: value.__setitem__("privateRomBytes", "00"),
        lambda value: value["eventDirectState"]["accessSites"][0].__setitem__("runtimeValue", 1),
        lambda value: value["eventDirectState"]["sourceFiles"].__setitem__("prose", "dialogue"),
    ):
        broken = deepcopy(fixture)
        mutator(broken)
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(broken, SCHEMA, owner="map-event direct-state fixture")


def test_research_index_adds_exact_direct_state_bindings_without_object_drift() -> None:
    index = _without_request_consumption(
        _without_predicate_results(
            _without_dialogue_state(_without_request_state(load_json(INDEX)))
        )
    )
    normalized = deepcopy(index)
    removed_handoff_records: set[str] = set()
    for record in normalized["records"]:
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
    removed_direct_control_records: set[str] = set()
    for record in normalized["records"]:
        direct_control = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] == "sf2-map-event-direct-control-static-v1"
        ]
        if not direct_control:
            continue
        assert len(direct_control) == 1
        assert direct_control[0] == {
            "level": "H2",
            "fixture": "tests/fixtures/h2/map-event-direct-control-static-v1.json",
            "fixtureId": "sf2-map-event-direct-control-static-v1",
            "verifier": "src/sf2tool/h2/map_event_direct_control.py",
            "bindings": [
                {
                    "addressId": "entry",
                    "fixtureField": (
                        f"eventDirectControl.sourceFiles.{record['symbol']}.tableEntryAddress"
                    ),
                }
            ],
        }
        assert record["documents"].count("docs/research/map-event-direct-control.md") == 1
        assert record["documents"][-1] == "docs/research/map-event-direct-control.md"
        record["evidence"] = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] != "sf2-map-event-direct-control-static-v1"
        ]
        record["documents"].remove("docs/research/map-event-direct-control.md")
        removed_direct_control_records.add(record["id"])
    assert len(removed_direct_control_records) == 53
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
    records = {record["id"]: record for record in normalized["records"]}
    base_records = {record["id"]: record for record in base["records"]}
    assert set(records) == set(base_records)

    direct_evidence = {
        record_id: next(
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] == "sf2-map-event-direct-state-static-v1"
        )
        for record_id, record in records.items()
        if any(
            evidence["fixtureId"] == "sf2-map-event-direct-state-static-v1"
            for evidence in record["evidence"]
        )
    }
    assert set(direct_evidence) == set(EXPECTED_INDEX_BINDINGS)
    for record_id, table_symbol in EXPECTED_INDEX_BINDINGS.items():
        record = records[record_id]
        base_record = base_records[record_id]
        assert record["addresses"] == base_record["addresses"]
        assert record["documents"] == base_record["documents"] + [
            "docs/research/map-event-direct-state.md"
        ]
        assert record["evidence"] == base_record["evidence"] + [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-direct-state-static-v1.json",
                "fixtureId": "sf2-map-event-direct-state-static-v1",
                "verifier": "src/sf2tool/h2/map_event_direct_state.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            f"eventDirectState.sourceFiles.{table_symbol}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
    for record_id in set(records) - set(EXPECTED_INDEX_BINDINGS):
        assert records[record_id] == base_records[record_id]


def test_research_index_schema_allows_only_the_event_direct_state_root() -> None:
    index = load_json(INDEX)
    validate_json(index, INDEX_SCHEMA, owner="map-event direct-state index")

    for fixture_field in (
        "unknownRoot.eventDirectState",
        "sourceContext.eventDirectState.sourceFiles.ms_map2_EntityEvents",
    ):
        broken = deepcopy(index)
        binding = next(
            binding
            for record in broken["records"]
            for evidence in record["evidence"]
            if evidence["fixtureId"] == "sf2-map-event-direct-state-static-v1"
            for binding in evidence["bindings"]
        )
        binding["fixtureField"] = fixture_field
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(broken, INDEX_SCHEMA, owner="map-event direct-state index")


@pytest.mark.parametrize(
    ("operation", "message"),
    [
        (
            {
                "mnemonic": "move",
                "sizeSuffix": ".w",
                "operandTexts": [
                    "d0",
                    "((EVENT_RELATIVE_POSITION-$1000000)).w",
                    "d1",
                ],
            },
            "move operand count drift",
        ),
        (
            {
                "mnemonic": "cmpi",
                "sizeSuffix": ".w",
                "operandTexts": ["((EVENT_RELATIVE_POSITION-$1000000)).w", "#1"],
            },
            "cmpi operand-position drift",
        ),
        (
            {
                "mnemonic": "move",
                "sizeSuffix": ".s",
                "operandTexts": ["d0", "((EVENT_RELATIVE_POSITION-$1000000)).w"],
            },
            "direct instruction width drift",
        ),
    ],
)
def test_direct_state_operand_parser_rejects_near_misses(
    operation: dict[str, object], message: str
) -> None:
    with pytest.raises(ValueError, match=message):
        _direct_access_positions(operation)


def test_direct_state_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-event-direct-state"])
    assert args.h2_command == "map-event-direct-state"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None
