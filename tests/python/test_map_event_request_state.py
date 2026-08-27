"""Focused H2 contract tests for map-event request-state flow."""

from __future__ import annotations

import json
import re
import subprocess
from collections.abc import Iterator
from contextlib import contextmanager
from copy import deepcopy
from pathlib import Path
from shutil import copy2

import pytest

import sf2tool.h2.map_event_request_state as request_state_module
from sf2tool.cli import build_parser
from sf2tool.h2.map_event_combatant_state import (
    normalize_map_event_combatant_state_later_owner_index as normalize_later_owner_index,
)
from sf2tool.h2.map_event_direct_state import FIXTURE as DIRECT_STATE_FIXTURE
from sf2tool.h2.map_event_request_state import (
    FIXTURE,
    ID,
    SCHEMA,
    _fresh_retained_request_state_owners,
    _reaching_definitions,
    _selected_programs,
    _selected_write_rows,
)
from sf2tool.jsonio import load_json as _load_json
from sf2tool.jsonio import validate_json
from sf2tool.research_index import verify_index

ROOT = Path(__file__).resolve().parents[2]
INDEX = ROOT / "manifests/research-index.json"
INDEX_SCHEMA = ROOT / "schemas/research-index.schema.json"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"
ROM = ROOT / "local/roms/sf2-us.bin"
BASE = "0fe7e33b96f39761198404de60d3f0fd5456c426"
DOCUMENT = "docs/research/map-event-request-state.md"
VERIFIER = "src/sf2tool/h2/map_event_request_state.py"


def load_json(path):
    value = _load_json(path)
    return normalize_later_owner_index(value) if path == INDEX else value
EXPECTED_INDEX_BINDINGS = {
    "map.data.ms-map10-entityevents": "ms_map10_EntityEvents",
    "map.data.ms-map13-entityevents": "ms_map13_EntityEvents",
    "map.data.ms-map13-flag513-entityevents": "ms_map13_flag513_EntityEvents",
    "map.data.ms-map15-entityevents": "ms_map15_EntityEvents",
    "map.data.ms-map16-entityevents": "ms_map16_EntityEvents",
    "map.data.ms-map16-flag530-entityevents": "ms_map16_flag530_EntityEvents",
    "map.data.ms-map16-zoneevents": "ms_map16_ZoneEvents",
    "map.data.ms-map2-entityevents": "ms_map2_EntityEvents",
    "map.data.ms-map22-zoneevents": "ms_map22_ZoneEvents",
    "map.data.ms-map25-entityevents": "ms_map25_EntityEvents",
    "map.data.ms-map29-entityevents": "ms_map29_EntityEvents",
    "map.data.ms-map3-flag609-entityevents": "ms_map3_flag609_EntityEvents",
    "map.data.ms-map31-flag830-entityevents": "ms_map31_flag830_EntityEvents",
    "map.data.ms-map38-entityevents": "ms_map38_EntityEvents",
    "map.data.ms-map5-flag530-entityevents": "ms_map5_flag530_EntityEvents",
    "map.data.ms-map6-flag701-entityevents": "ms_map6_flag701_EntityEvents",
    "map.data.ms-map66-zoneevents": "ms_map66_ZoneEvents",
    "map.data.ms-map69-zoneevents": "ms_map69_ZoneEvents",
    "map.data.ms-map70-zoneevents": "ms_map70_ZoneEvents",
    "map.data.ms-map74-zoneevents": "ms_map74_ZoneEvents",
    "map.data.ms-map76-zoneevents": "ms_map76_ZoneEvents",
    "map.data.ms-map77-zoneevents": "ms_map77_ZoneEvents",
    "map.data.ms-map8-entityevents": "ms_map8_EntityEvents",
    "map.data.ms-map9-entityevents": "ms_map9_EntityEvents",
}


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _mutable_request_state_inputs(tmp_path: Path, fixture: dict[str, object]) -> tuple[Path, Path]:
    """Copy only the selected public source/H1/ROM inputs for mutation guards."""
    upstream = tmp_path / "SF2DISASM"
    source_root = UPSTREAM / "disasm"
    for identity in fixture["sourceContext"]["sourceIdentities"]:
        source_path = Path(identity["path"])
        destination = upstream / "disasm" / source_path
        destination.parent.mkdir(parents=True, exist_ok=True)
        copy2(source_root / source_path, destination)
    h1_destination = upstream / "build/sf2build-h1.lst"
    h1_destination.parent.mkdir(parents=True, exist_ok=True)
    copy2(UPSTREAM / "build/sf2build-h1.lst", h1_destination)
    rom = tmp_path / "sf2-us.bin"
    copy2(ROM, rom)
    return upstream, rom


def _mutable_source_path(upstream: Path, source_path: str) -> Path:
    return upstream / "disasm" / source_path


@contextmanager
def _replaced_line(path: Path, line_number: int, replacement: str) -> Iterator[None]:
    """Restore a byte-exact temporary input after one source/H1 mutation."""
    original = path.read_bytes()
    lines = original.decode("utf-8").splitlines(keepends=True)
    assert 1 <= line_number <= len(lines)
    ending = "\r\n" if lines[line_number - 1].endswith("\r\n") else "\n"
    lines[line_number - 1] = f"{replacement.rstrip(chr(13) + chr(10))}{ending}"
    path.write_bytes("".join(lines).encode("utf-8"))
    try:
        yield
    finally:
        path.write_bytes(original)


@contextmanager
def _replaced_text(path: Path, expected: str, replacement: str) -> Iterator[None]:
    original = path.read_bytes()
    text = original.decode("utf-8")
    assert text.count(expected) == 1
    path.write_bytes(text.replace(expected, replacement, 1).encode("utf-8"))
    try:
        yield
    finally:
        path.write_bytes(original)


@contextmanager
def _appended_comment(path: Path, comment: str) -> Iterator[None]:
    original = path.read_bytes()
    path.write_bytes(original + b"\n" + comment.encode("utf-8") + b"\n")
    try:
        yield
    finally:
        path.write_bytes(original)


def _source_line(path: Path, line_number: int) -> str:
    lines = path.read_text(encoding="utf-8").splitlines()
    assert 1 <= line_number <= len(lines)
    return lines[line_number - 1]


def _build_from_mutable_inputs(
    monkeypatch: pytest.MonkeyPatch,
    retained: dict[str, object],
    dialogue: dict[str, object],
    retained_owners: dict[str, object],
    *,
    upstream: Path,
    rom: Path,
) -> dict[str, object]:
    """Run the public production builder, deliberately before fixture comparison."""
    monkeypatch.setattr(
        request_state_module,
        "_fresh_retained_request_state_owners",
        lambda _rom_path, _upstream_path: (retained, dialogue, retained_owners),
    )
    return request_state_module.build_map_event_request_state_contract(rom, upstream)


def test_request_state_fixture_is_closed_public_and_exact() -> None:
    fixture = _fixture()
    validate_json(fixture, SCHEMA, owner="map-event request-state fixture")
    assert set(fixture) == {
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "sourceContext",
        "retainedOwners",
        "eventRequestState",
        "unknowns",
        "summary",
    }
    assert fixture["id"] == ID
    assert fixture["summary"] == {
        "motherProgramContextCount": 914,
        "motherOperationCount": 3579,
        "positiveProgramContextCount": 39,
        "zeroProgramContextCount": 875,
        "entityProgramContextCount": 30,
        "zoneProgramContextCount": 9,
        "sourceFileCount": 24,
        "sourceIdentityCount": 27,
        "symbolDefinitionCount": 6,
        "contextOperationCount": 262,
        "physicalOperationCount": 262,
        "contextLabelCount": 82,
        "physicalLabelCount": 82,
        "ordinaryOperationCount": 139,
        "conditionalBranchCount": 34,
        "unconditionalBranchCount": 21,
        "directCallCount": 29,
        "directJumpCount": 3,
        "returnCount": 36,
        "writeDefinitionSiteCount": 45,
        "currentShopIndexWriteCount": 32,
        "mapEventTypeWriteCount": 8,
        "egressMapWriteCount": 2,
        "raftMapWriteCount": 1,
        "raftXWriteCount": 1,
        "raftYWriteCount": 1,
        "uniqueSourceOperandCount": 37,
        "enumSourceOperandCount": 35,
        "numericSourceOperandCount": 2,
        "shopMenuTransferSiteCount": 31,
        "shopMenuReturningCallCount": 28,
        "shopMenuTailJumpCount": 3,
        "returnStateSiteCount": 36,
        "handoffStateSiteCount": 67,
        "handoffStateRelationCount": 69,
        "h1RomAnchorCount": 262,
    }
    state = fixture["eventRequestState"]
    assert set(state) == {
        "symbolDefinitions",
        "symbolDefinitionOrder",
        "programFlows",
        "programFlowOrder",
        "writeDefinitionSites",
        "writeDefinitionSiteOrder",
        "handoffStateSites",
        "handoffStateSiteOrder",
        "sourceFiles",
        "sourceFileOrder",
        "digests",
    }
    assert set(fixture["retainedOwners"]) == {
        "mapEvents",
        "directState",
        "directControl",
        "directHandoff",
        "predicateResults",
        "dialogueState",
    }
    assert set(fixture["unknowns"]) == {
        "normalStoryProgramReachability",
        "selectedControlFlowPath",
        "callerEntryState",
        "actualRequestWriteOrder",
        "actualDefinitionAtHandoff",
        "actualShopSelection",
        "actualShopMenuEntryAndOutcome",
        "actualEgressDestination",
        "actualRaftDestinationAndCoordinates",
        "actualMapEventReloadRequestConsumption",
        "actualProgramReturnState",
        "crossMapStateLifetime",
        "saveLoadPersistence",
        "inputUiMapTransitionAudioTimingAndStoryMeaning",
    }
    assert set(fixture["unknowns"].values()) == {"Unknown"}


@pytest.mark.parametrize(
    "mutator",
    (
        lambda value: value.__setitem__("privateRomBytes", "00"),
        lambda value: value.__setitem__("schemaVersionRenamed", value.pop("schemaVersion")),
        lambda value: value["eventRequestState"]["programFlows"][0].__setitem__(
            "decodedStory", "private"
        ),
        lambda value: value["eventRequestState"].__setitem__("runtimeTrace", []),
        lambda value: value["eventRequestState"]["symbolDefinitionOrder"].reverse(),
        lambda value: value["eventRequestState"]["handoffStateSiteOrder"].reverse(),
        lambda value: value["eventRequestState"]["sourceFiles"].pop("ms_map2_EntityEvents"),
    ),
)
def test_request_state_schema_rejects_private_shape_and_order_drift(mutator: object) -> None:
    broken = deepcopy(_fixture())
    mutator(broken)
    with pytest.raises(ValueError, match="schema validation"):
        validate_json(broken, SCHEMA, owner="map-event request-state fixture")


def test_selected_write_rows_reject_missing_and_wrong_class_counts() -> None:
    direct_state = load_json(DIRECT_STATE_FIXTURE)["eventDirectState"]
    assert len(_selected_write_rows(direct_state)) == 45

    missing = deepcopy(direct_state)
    missing["accessSites"] = missing["accessSites"][1:]
    with pytest.raises(ValueError, match="write-source denominator"):
        _selected_write_rows(missing)

    changed = deepcopy(direct_state)
    changed["accessSites"][0]["symbol"] = "CURRENT_PORTRAIT"
    with pytest.raises(ValueError, match="write-source denominator"):
        _selected_write_rows(changed)


def test_production_source_h1_rom_and_reaching_mutations_fail_pre_fixture(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    """Exercise real selected inputs through the public builder, not the golden verifier.

    The retained owners are freshly reconstructed once from the pinned inputs;
    each adversary then changes only a mutable copy of the narrow request-state
    source/H1/ROM surface.  ``build_map_event_request_state_contract`` has no
    fixture comparison, so every asserted failure is a construction guard.
    """
    fixture = _fixture()
    retained, dialogue, retained_owners = _fresh_retained_request_state_owners(ROM, UPSTREAM)
    mutable_upstream, mutable_rom = _mutable_request_state_inputs(tmp_path, fixture)

    def build(retained_input: dict[str, object] = retained) -> dict[str, object]:
        return _build_from_mutable_inputs(
            monkeypatch,
            retained_input,
            dialogue,
            retained_owners,
            upstream=mutable_upstream,
            rom=mutable_rom,
        )

    baseline = build()
    assert baseline == fixture
    state = baseline["eventRequestState"]
    write_sites = state["writeDefinitionSites"]
    assert len(write_sites) == 45
    assert baseline["summary"]["positiveProgramContextCount"] == 39
    assert baseline["summary"]["contextOperationCount"] == 262
    writes_by_symbol = {row["symbol"]: row for row in write_sites}
    assert set(writes_by_symbol) == {
        "CURRENT_SHOP_INDEX",
        "MAP_EVENT_TYPE",
        "EGRESS_MAP",
        "RAFT_MAP",
        "RAFT_X",
        "RAFT_Y",
    }

    # Every write class is covered by a real source value-identity mutation.
    # The shared source-statement guard has already walked all 45 sites; this
    # representative loop catches one smallest writable use site per class.
    for _symbol, write in writes_by_symbol.items():
        source_path = _mutable_source_path(mutable_upstream, write["sourcePath"])
        source_line = _source_line(source_path, write["sourceLine"])
        value_token = write["valueToken"]
        assert value_token is not None and value_token in source_line
        replacement = "#1" if value_token != "#1" else "#0"
        with (
            _replaced_line(
                source_path,
                write["sourceLine"],
                source_line.replace(value_token, replacement, 1),
            ),
            pytest.raises(ValueError, match="source mnemonic/operand-order drift"),
        ):
            build()

    representative = write_sites[0]
    representative_path = _mutable_source_path(mutable_upstream, representative["sourcePath"])
    representative_line = _source_line(representative_path, representative["sourceLine"])
    opcode = f"{representative['mnemonic']}.{representative['width']}"
    assert opcode in representative_line
    with (
        _replaced_line(
            representative_path,
            representative["sourceLine"],
            representative_line.replace(opcode, "moveq", 1),
        ),
        pytest.raises(ValueError, match="source mnemonic/operand-order drift"),
    ):
        build()
    alternate_width = "w" if representative["width"] != "w" else "b"
    with (
        _replaced_line(
            representative_path,
            representative["sourceLine"],
            representative_line.replace(
                opcode, f"{representative['mnemonic']}.{alternate_width}", 1
            ),
        ),
        pytest.raises(ValueError, match="source mnemonic/operand-order drift"),
    ):
        build()
    first_operand, second_operand = representative["operandTexts"]
    swapped_line = (
        representative_line.replace(first_operand, "__FIRST_OPERAND__", 1)
        .replace(second_operand, first_operand, 1)
        .replace("__FIRST_OPERAND__", second_operand, 1)
    )
    assert swapped_line != representative_line
    with (
        _replaced_line(representative_path, representative["sourceLine"], swapped_line),
        pytest.raises(ValueError, match="source mnemonic/operand-order drift"),
    ):
        build()

    branch_flow = next(
        flow
        for flow in state["programFlows"]
        if any(
            operation["controlFlowKind"] == "conditional-branch" for operation in flow["operations"]
        )
    )
    branch = next(
        operation
        for operation in branch_flow["operations"]
        if operation["controlFlowKind"] == "conditional-branch"
    )
    branch_path = _mutable_source_path(mutable_upstream, branch_flow["sourcePath"])
    branch_line = _source_line(branch_path, branch["sourceLine"])
    branch_mnemonic = branch["sourceMnemonic"]
    assert branch_mnemonic in branch_line
    opposite_polarity = ("beq" if branch_mnemonic.startswith("bne") else "bne") + branch_mnemonic[
        3:
    ]
    with (
        _replaced_line(
            branch_path,
            branch["sourceLine"],
            branch_line.replace(branch_mnemonic, opposite_polarity, 1),
        ),
        pytest.raises(ValueError, match="source mnemonic/operand-order drift"),
    ):
        build()

    shop = next(row for row in state["handoffStateSites"] if row["kind"] == "shop-menu-transfer")
    shop_path = _mutable_source_path(mutable_upstream, shop["sourcePath"])
    shop_line = _source_line(shop_path, shop["sourceLine"])
    assert "j_ShopMenu" in shop_line
    with (
        _replaced_line(
            shop_path,
            shop["sourceLine"],
            shop_line.replace("j_ShopMenu", "ShopMenu", 1),
        ),
        pytest.raises(ValueError, match="source mnemonic/operand-order drift"),
    ):
        build()

    source_file = next(iter(state["sourceFiles"].values()))
    table_path = _mutable_source_path(mutable_upstream, source_file["sourcePath"])
    with (
        _replaced_text(
            table_path,
            f"{source_file['tableSymbol']}:",
            f"{source_file['tableSymbol']}_MUTATED:",
        ),
        pytest.raises(ValueError, match="table source owner drift"),
    ):
        build()
    label_program_field = (
        "entityTargetPrograms"
        if branch_flow["category"] == "entityEvents"
        else "zoneTargetPrograms"
    )
    source_program_for_label = next(
        program
        for program in retained["mapEvents"][label_program_field]
        if program["canonicalSymbol"] == branch_flow["programSymbol"]
        and program["entryAddress"] == branch_flow["programEntryAddress"]
    )
    program_label = source_program_for_label["labels"][0]
    label_line = _source_line(branch_path, program_label["sourceLine"])
    assert program_label["symbol"] in label_line
    with (
        _replaced_line(
            branch_path,
            program_label["sourceLine"],
            label_line.replace(program_label["symbol"], f"{program_label['symbol']}_MUTATED", 1),
        ),
        pytest.raises(ValueError, match="label source drift"),
    ):
        build()
    with _appended_comment(representative_path, f"; near-miss {representative_line.lstrip()}"):
        commented = build()
    assert commented["summary"] == baseline["summary"]
    assert commented["eventRequestState"]["digests"] == state["digests"]

    anchors = baseline["sourceContext"]["anchors"]
    assert len(anchors) == len({row["romPc"] for row in anchors}) == 262
    assert baseline["sourceContext"]["h1Listing"] == fixture["sourceContext"]["h1Listing"]
    assert baseline["romSha256"] == fixture["romSha256"]
    anchor = next(row for row in anchors if row["romPc"] == representative["romPc"])
    assert anchor["h1InstructionSha256"] and anchor["romInstructionSha256"]
    h1_path = mutable_upstream / "build/sf2build-h1.lst"
    h1_lines = h1_path.read_text(encoding="utf-8").splitlines()
    h1_candidates = [
        line_number
        for line_number, line in enumerate(h1_lines, start=1)
        if line.startswith(f"{representative['romPc']:08X} ")
        and re.search(r"\b[0-9A-F]{4}\b", line[8:])
    ]
    assert len(h1_candidates) == 1
    h1_line = h1_lines[h1_candidates[0] - 1]
    h1_word = re.search(r"\b[0-9A-F]{4}\b", h1_line[8:])
    assert h1_word is not None
    h1_bytes = h1_word.group()
    changed_h1_bytes = ("0" if h1_bytes[0] != "0" else "1") + h1_bytes[1:]
    with (
        _replaced_line(
            h1_path,
            h1_candidates[0],
            h1_line.replace(h1_bytes, changed_h1_bytes, 1),
        ),
        pytest.raises(ValueError, match="H1/ROM relocation drift"),
    ):
        build()
    original_rom = mutable_rom.read_bytes()
    changed_rom = bytearray(original_rom)
    changed_rom[representative["romPc"]] ^= 1
    mutable_rom.write_bytes(changed_rom)
    try:
        with pytest.raises(ValueError, match="H1/ROM relocation drift"):
            build()
    finally:
        mutable_rom.write_bytes(original_rom)

    # This changes the real source-derived selected CFG, not the fixture or the
    # synthetic graph below.  The source/H1/ROM assertions remain valid, then
    # the caller-local reaching/handoff projection rejects the changed branch.
    selected = _selected_programs(
        retained["mapEvents"], _selected_write_rows(retained["eventDirectState"])
    )
    category, source_program = next(
        (category, program)
        for category, program in selected
        if any(
            operation["controlFlowKind"] == "conditional-branch"
            for operation in program["operations"]
        )
    )
    source_branch = next(
        operation
        for operation in source_program["operations"]
        if operation["controlFlowKind"] == "conditional-branch"
    )
    mutated_retained = deepcopy(retained)
    program_field = "entityTargetPrograms" if category == "entityEvents" else "zoneTargetPrograms"
    mutated_program = next(
        program
        for program in mutated_retained["mapEvents"][program_field]
        if program["canonicalSymbol"] == source_program["canonicalSymbol"]
        and program["entryAddress"] == source_program["entryAddress"]
    )
    mutated_branch = next(
        operation
        for operation in mutated_program["operations"]
        if operation["address"] == source_branch["address"]
    )
    mutated_branch["controlFlowKind"] = "ordinary"
    with pytest.raises(ValueError, match="selected CFG has an unreachable operation"):
        build(mutated_retained)


def test_reaching_definitions_keeps_branch_merges_kills_and_partial_tuples() -> None:
    program = {"canonicalSymbol": "Synthetic", "operations": [{}, {}, {}, {}, {}, {}]}
    definitions = {
        0: {"CURRENT_SHOP_INDEX": "write:000010:CURRENT_SHOP_INDEX"},
        1: {"MAP_EVENT_TYPE": "write:000012:MAP_EVENT_TYPE"},
        2: {"CURRENT_SHOP_INDEX": "write:000014:CURRENT_SHOP_INDEX"},
        3: {},
        4: {"EGRESS_MAP": "write:000018:EGRESS_MAP"},
        5: {},
    }
    successors = {0: [1, 2], 1: [3], 2: [3], 3: [4], 4: [5], 5: []}
    may, must = _reaching_definitions(program, definitions, successors)
    assert may[3]["CURRENT_SHOP_INDEX"] == {
        "write:000010:CURRENT_SHOP_INDEX",
        "write:000014:CURRENT_SHOP_INDEX",
    }
    assert must[3]["CURRENT_SHOP_INDEX"] == set()
    assert may[3]["MAP_EVENT_TYPE"] == {"write:000012:MAP_EVENT_TYPE"}
    assert must[3]["MAP_EVENT_TYPE"] == set()
    assert may[5]["EGRESS_MAP"] == {"write:000018:EGRESS_MAP"}
    assert must[5]["EGRESS_MAP"] == {"write:000018:EGRESS_MAP"}
    assert may[5]["RAFT_MAP"] == must[5]["RAFT_MAP"] == set()


def test_handoffs_keep_shop_alias_and_do_not_enter_callees() -> None:
    state = _fixture()["eventRequestState"]
    handoffs = state["handoffStateSites"]
    shops = [row for row in handoffs if row["kind"] == "shop-menu-transfer"]
    returns = [row for row in handoffs if row["kind"] == "program-return"]
    assert len(shops) == 31
    assert len(returns) == 36
    assert {row["instructionTargetSymbol"] for row in shops} == {"j_ShopMenu"}
    assert {row["effectiveTargetSymbol"] for row in shops} == {"ShopMenu"}
    assert {row["transferKind"] for row in shops} == {"direct-call", "direct-jump"}
    assert all(row["instructionTargetSymbol"] is None for row in returns)
    assert (
        sum(bool(state_row["mayDefinitionIds"]) for row in handoffs for state_row in row["state"])
        == 69
    )


def test_request_state_cli_registration() -> None:
    args = build_parser().parse_args(["h2", "map-event-request-state"])
    assert args.h2_command == "map-event-request-state"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


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

def test_request_state_index_delta_is_exact_24_binding_append_without_object_drift() -> None:
    index = _without_request_consumption(load_json(INDEX))
    validate_json(index, INDEX_SCHEMA, owner="map-event request-state index")
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
    assert len(records) == len(base_records) == 1625
    assert sum(len(record["addresses"]) for record in records.values()) == sum(
        len(record["addresses"]) for record in base_records.values()
    )
    assert sum(len(record.get("designContracts", [])) for record in records.values()) == sum(
        len(record.get("designContracts", [])) for record in base_records.values()
    )
    for record_id, symbol in EXPECTED_INDEX_BINDINGS.items():
        record, previous = records[record_id], base_records[record_id]
        assert record["symbol"] == symbol
        assert record["addresses"] == previous["addresses"]
        assert record["documents"] == previous["documents"] + [DOCUMENT]
        assert record["evidence"] == previous["evidence"] + [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-request-state-static-v1.json",
                "fixtureId": ID,
                "verifier": VERIFIER,
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            f"eventRequestState.sourceFiles.{symbol}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
    for record_id in set(records) - set(EXPECTED_INDEX_BINDINGS):
        assert records[record_id] == base_records[record_id]
    bindings = [
        binding
        for record in records.values()
        for evidence in record["evidence"]
        if evidence["fixtureId"] == ID
        for binding in evidence["bindings"]
    ]
    assert len(bindings) == 24
    assert {binding["fixtureField"] for binding in bindings} == {
        f"eventRequestState.sourceFiles.{symbol}.tableEntryAddress"
        for symbol in EXPECTED_INDEX_BINDINGS.values()
    }
    for field in (
        "eventRequestState.sourceFiles.ms_map2_Foo.tableEntryAddress",
        "eventRequestState.sourceFiles.ms_map2_EntityEvents.notAnEntryAddress",
        "sourceContext.eventRequestState.sourceFiles.ms_map2_EntityEvents.tableEntryAddress",
    ):
        broken = deepcopy(index)
        next(
            binding
            for record in broken["records"]
            for evidence in record["evidence"]
            if evidence["fixtureId"] == ID
            for binding in evidence["bindings"]
        )["fixtureField"] = field
        with pytest.raises(ValueError, match="schema validation"):
            validate_json(broken, INDEX_SCHEMA, owner="map-event request-state index")
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
