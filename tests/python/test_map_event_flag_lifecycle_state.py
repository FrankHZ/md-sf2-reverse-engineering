"""Focused adversarial tests for same-program map-event flag lifecycle facts."""

from __future__ import annotations

import hashlib
from copy import deepcopy
from pathlib import Path
from shutil import copy2

import pytest

import sf2tool.h2.map_event_flag_lifecycle_state as lifecycle_module
from sf2tool.h2.map_event_cross_program_flag_state import (
    _remove_map_event_cross_program_flag_state_later_owner_index_delta,
)
from sf2tool.h2.map_event_flag_lifecycle_state import (
    _PREDECESSOR_INDEX_SHA256,
    FIXTURE,
    ID,
    SCHEMA,
    _project,
    _remove_map_event_flag_lifecycle_state_later_owner_index_delta,
    _validate_order,
    canonical_json_bytes,
    normalize_map_event_flag_lifecycle_state_later_owner_index,
)
from sf2tool.h2.map_event_flag_route_selection import (
    _remove_map_event_flag_route_selection_later_owner_index_delta,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

ROM = repo_path("local/roms/sf2-us.bin")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
INDEX = repo_path("manifests/research-index.json")
INDEX_SCHEMA = repo_path("schemas/research-index.schema.json")


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _project_fixture(
    *,
    map_events: dict[str, object] | None = None,
    rom_path: Path = ROM,
    upstream_path: Path = UPSTREAM,
) -> dict[str, object]:
    fixture = _fixture()
    return _project(
        deepcopy(load_map_events_fixture()["expected"] if map_events is None else map_events),
        deepcopy(fixture["retainedOwners"]),
        rom_path,
        upstream_path,
    )


def _source_surface(tmp_path: Path, *, copy_rom: bool = False) -> tuple[Path, Path]:
    """Copy exactly the selected public source/H1 surface for mutation guards."""
    upstream = tmp_path / "SF2DISASM"
    for source in _fixture()["sourceContext"]["sourceIdentities"]:
        path = source["path"]
        destination = upstream / "disasm" / path
        destination.parent.mkdir(parents=True, exist_ok=True)
        copy2(UPSTREAM / "disasm" / path, destination)
    listing = upstream / "build/sf2build-h1.lst"
    listing.parent.mkdir(parents=True, exist_ok=True)
    copy2(UPSTREAM / "build/sf2build-h1.lst", listing)
    rom = ROM
    if copy_rom:
        rom = tmp_path / "sf2-us.bin"
        copy2(ROM, rom)
    return upstream, rom


def _program_with_single_relation() -> tuple[dict[str, object], dict[str, object]]:
    fixture = _fixture()
    relation = next(
        row
        for row in fixture["flagLifecycleState"]["lifecycleRelations"]
        if len(row["accesses"]) == 2
        and sum(
            other["category"] == row["category"]
            and other["programSymbol"] == row["programSymbol"]
            and other["programEntryAddress"] == row["programEntryAddress"]
            for other in fixture["flagLifecycleState"]["lifecycleRelations"]
        )
        == 1
    )
    program = next(
        program
        for program in load_map_events_fixture()["expected"][
            lifecycle_module._PROGRAM_FIELDS[relation["category"]]
        ]
        if program["canonicalSymbol"] == relation["programSymbol"]
        and program["entryAddress"] == relation["programEntryAddress"]
    )
    return relation, program


def _altered_program_map_events(mutator: object) -> dict[str, object]:
    relation, selected_program = _program_with_single_relation()
    altered = deepcopy(load_map_events_fixture()["expected"])
    program = next(
        row
        for row in altered[lifecycle_module._PROGRAM_FIELDS[relation["category"]]]
        if row["canonicalSymbol"] == selected_program["canonicalSymbol"]
        and row["entryAddress"] == selected_program["entryAddress"]
    )
    mutator(program, relation)
    return altered


def test_complete_projection_is_closed_and_has_all_exact_denominators() -> None:
    fixture = _fixture()
    validate_json(fixture, SCHEMA, owner="map-event flag lifecycle fixture")
    _validate_order(fixture)
    assert _project_fixture() == fixture
    state = fixture["flagLifecycleState"]
    assert state["selectionSummary"]["categoryRelationCounts"] == {
        "entityEvents": 65,
        "zoneEvents": 62,
        "itemEvents": 4,
    }
    assert state["selectionSummary"]["accessSequenceCounts"] == [
        {"accessKinds": ["read", "set"], "relationCount": 121},
        {"accessKinds": ["read", "set", "read", "set"], "relationCount": 3},
        {"accessKinds": ["read", "clear"], "relationCount": 2},
        {"accessKinds": ["read", "read", "set"], "relationCount": 1},
        {"accessKinds": ["set", "read"], "relationCount": 1},
        {"accessKinds": ["read", "clear", "clear"], "relationCount": 1},
        {"accessKinds": ["read", "set", "clear"], "relationCount": 1},
        {"accessKinds": ["read", "clear", "set"], "relationCount": 1},
    ]
    assert fixture["summary"] == {
        "motherProgramContextCount": 914,
        "motherOperationCount": 3579,
        "positiveProgramContextCount": 117,
        "zeroProgramContextCount": 797,
        "lifecycleRelationCount": 131,
        "numericFlagCount": 82,
        "relationLocalAccessCount": 272,
        "readAccessCount": 135,
        "setAccessCount": 131,
        "clearAccessCount": 6,
        "selectedProgramAccessCount": 348,
        "sourceFileCount": 67,
        "contextualOperationCount": 1177,
        "physicalOperationCount": 1137,
        "contextualLabelCount": 339,
        "contextualEncodedByteCount": 4216,
        "physicalUniqueByteCount": 4066,
        "intervalCount": 79,
        "overlapByteCount": 150,
    }
    assert len(state["sourceFiles"]) == 67
    assert len(state["programFlows"]) == 117
    assert len(state["lifecycleRelations"]) == 131
    assert len(state["flagTotals"]) == 82
    assert state["intervalCoverage"]["intervals"][-1]["endAddressExclusive"] > 0
    assert fixture["unknowns"] == {key: "Unknown" for key in lifecycle_module._UNKNOWN_KEYS}


def test_selection_rejects_missing_mother_and_direct_read_mutation_pairs() -> None:
    missing_mother = deepcopy(load_map_events_fixture()["expected"])
    missing_mother["itemTargetPrograms"] = missing_mother["itemTargetPrograms"][:-1]
    with pytest.raises(ValueError, match="mother corpus"):
        _project_fixture(map_events=missing_mother)

    without_mutation = _altered_program_map_events(
        lambda program, _relation: [
            operation.__setitem__("sourceMnemonic", "notFlagMutation")
            for operation in program["operations"]
            if operation["sourceMnemonic"] in {"setFlg", "clrFlg"}
        ]
    )
    with pytest.raises(ValueError, match="selection denominator"):
        _project_fixture(map_events=without_mutation)

    without_read = _altered_program_map_events(
        lambda program, _relation: [
            operation.__setitem__("sourceMnemonic", "notFlagRead")
            for operation in program["operations"]
            if operation["sourceMnemonic"] == "chkFlg"
        ]
    )
    with pytest.raises(ValueError, match="selection denominator"):
        _project_fixture(map_events=without_read)


def test_access_operand_kind_order_and_immediate_branch_mutations_fail() -> None:
    wrong_operand = _altered_program_map_events(
        lambda program, relation: next(
            operation
            for operation in program["operations"]
            if operation["sourceMnemonic"] in {"setFlg", "clrFlg"}
            and operation["operandTexts"] == [str(relation["flagNumber"])]
        )["operandTexts"].__setitem__(0, "2047")
    )
    with pytest.raises(ValueError, match="selection denominator|source opcode/operand"):
        _project_fixture(map_events=wrong_operand)

    set_to_clear = _altered_program_map_events(
        lambda program, relation: next(
            operation
            for operation in program["operations"]
            if operation["sourceMnemonic"] == "setFlg"
            and operation["operandTexts"] == [str(relation["flagNumber"])]
        ).__setitem__("sourceMnemonic", "clrFlg")
    )
    with pytest.raises(ValueError, match="access sequence|source opcode/operand"):
        _project_fixture(map_events=set_to_clear)

    def reorder_access_source_orders(
        program: dict[str, object], relation: dict[str, object]
    ) -> None:
        read = next(
            operation
            for operation in program["operations"]
            if operation["sourceMnemonic"] == "chkFlg"
            and operation["operandTexts"] == [str(relation["flagNumber"])]
        )
        mutation = next(
            operation
            for operation in program["operations"]
            if operation["sourceMnemonic"] in {"setFlg", "clrFlg"}
            and operation["operandTexts"] == [str(relation["flagNumber"])]
        )
        read["sourceOrder"], mutation["sourceOrder"] = (
            mutation["sourceOrder"],
            read["sourceOrder"],
        )

    reordered = _altered_program_map_events(reorder_access_source_orders)
    with pytest.raises(ValueError, match="access source-order"):
        _project_fixture(map_events=reordered)

    def duplicate_lifecycle_access(program: dict[str, object], relation: dict[str, object]) -> None:
        duplicate = next(
            operation
            for operation in program["operations"]
            if operation["sourceMnemonic"] not in {"chkFlg", "setFlg", "clrFlg"}
            and operation["controlFlowKind"] == "ordinary"
        )
        duplicate["sourceMnemonic"] = "setFlg"
        duplicate["operandTexts"] = [str(relation["flagNumber"])]

    duplicated = _altered_program_map_events(duplicate_lifecycle_access)
    with pytest.raises(ValueError, match="source opcode/operand"):
        _project_fixture(map_events=duplicated)

    wrong_branch = _altered_program_map_events(
        lambda program, _relation: next(
            operation
            for operation in program["operations"]
            if operation["controlFlowKind"] == "conditional-branch"
        ).__setitem__("mnemonic", "bcs")
    )
    with pytest.raises(ValueError, match="immediate branch"):
        _project_fixture(map_events=wrong_branch)


def test_source_macro_and_complete_program_body_guards_fail_before_fixture(tmp_path: Path) -> None:
    upstream, _rom = _source_surface(tmp_path)
    macro = upstream / "disasm/sf2macros.asm"
    macro.write_text(
        macro.read_text(encoding="utf-8").replace("trap #SET_FLAG", "trap #CLEAR_FLAG", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="service definition emission"):
        _project_fixture(upstream_path=upstream)

    upstream, _rom = _source_surface(tmp_path / "program")
    relation = _fixture()["flagLifecycleState"]["lifecycleRelations"][0]
    source = upstream / "disasm" / relation["sourcePath"]
    read = relation["accesses"][0]
    lines = source.read_text(encoding="utf-8").splitlines(keepends=True)
    assert lifecycle_module._normalise_statement(lines[read["sourceLine"] - 1]) == (
        f"chkFlg {relation['flagNumber']}"
    )
    lines[read["sourceLine"] - 1] = lines[read["sourceLine"] - 1].replace(
        str(relation["flagNumber"]), str(relation["flagNumber"] + 1), 1
    )
    source.write_text("".join(lines), encoding="utf-8")
    with pytest.raises(ValueError, match="source opcode/operand"):
        _project_fixture(upstream_path=upstream)


def test_h1_rom_and_complete_alias_accounting_guards_fail_before_fixture(tmp_path: Path) -> None:
    fixture = _fixture()
    address = fixture["flagLifecycleState"]["intervalCoverage"]["physicalOperations"][0]["address"]
    upstream, _rom = _source_surface(tmp_path)
    listing = upstream / "build/sf2build-h1.lst"
    original = listing.read_text(encoding="utf-8")
    encoded = lifecycle_module._h1_instruction_rows(original)[address][0].hex().upper()
    h1_line = next(
        line for line in original.splitlines() if line.split()[:2] == [f"{address:08X}", encoded]
    )
    old_byte = h1_line.split()[1][:2]
    new_byte = f"{int(old_byte, 16) ^ 1:02X}"
    listing.write_text(
        original.replace(h1_line, h1_line.replace(old_byte, new_byte, 1), 1), encoding="utf-8"
    )
    with pytest.raises(ValueError, match="H1/ROM byte"):
        _project_fixture(upstream_path=upstream)

    upstream, rom = _source_surface(tmp_path / "rom", copy_rom=True)
    rom_bytes = bytearray(rom.read_bytes())
    rom_bytes[address] ^= 1
    rom.write_bytes(rom_bytes)
    mutated_sha256 = hashlib.sha256(rom_bytes).hexdigest().upper()
    mutated_map_events = deepcopy(load_map_events_fixture()["expected"])
    mutated_map_events["romSha256"] = mutated_sha256
    with pytest.MonkeyPatch.context() as monkeypatch:
        monkeypatch.setattr(lifecycle_module, "_ROM_SHA256", mutated_sha256)
        with pytest.raises(ValueError, match="H1/ROM byte"):
            _project_fixture(
                map_events=mutated_map_events,
                rom_path=rom,
                upstream_path=upstream,
            )

    alias_mutation = deepcopy(fixture)
    physical = next(
        row
        for row in alias_mutation["flagLifecycleState"]["intervalCoverage"]["physicalOperations"]
        if len(row["contextOrders"]) > 1
    )
    physical["contextOrders"] = physical["contextOrders"][:1]
    with pytest.raises(ValueError, match="physical/context alias"):
        _validate_order(alias_mutation)


def test_schema_is_recursively_closed_and_order_checks_are_exact() -> None:
    fixture = _fixture()
    for mutation in (
        lambda value: value.__setitem__("runtime", {}),
        lambda value: value["flagLifecycleState"]["lifecycleRelations"][0].__setitem__(
            "rawRomBytes", "private"
        ),
        lambda value: value["flagLifecycleState"]["serviceDefinitions"]["chkFlg"].__setitem__(
            "extra", True
        ),
    ):
        altered = deepcopy(fixture)
        mutation(altered)
        with pytest.raises(ValueError):
            validate_json(altered, SCHEMA, owner="closure mutation")

    altered = deepcopy(fixture)
    altered["flagLifecycleState"]["programFlows"][:2] = list(
        reversed(altered["flagLifecycleState"]["programFlows"][:2])
    )
    with pytest.raises(ValueError, match="program flow order"):
        _validate_order(altered)

    duplicated_access = deepcopy(fixture)
    accesses = duplicated_access["flagLifecycleState"]["lifecycleRelations"][0]["accesses"]
    extra = deepcopy(accesses[-1])
    extra["accessOrder"] = len(accesses)
    extra["sourceOrder"] += 1
    accesses.append(extra)
    with pytest.raises(ValueError, match="access sequence"):
        _validate_order(duplicated_access)
    assert "local/" not in FIXTURE.read_text(encoding="utf-8")
    assert "rawRomBytes" not in FIXTURE.read_text(encoding="utf-8")


def test_retained_owners_are_fresh_builds_and_hash_locked(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    maps = lifecycle_module.build_map_events_contract(ROM, UPSTREAM)
    outputs = {
        "state": load_json(repo_path("tests/fixtures/h2/map-event-direct-state-static-v1.json")),
        "control": load_json(
            repo_path("tests/fixtures/h2/map-event-direct-control-static-v1.json")
        ),
        "handoff": load_json(
            repo_path("tests/fixtures/h2/map-event-direct-handoff-static-v1.json")
        ),
        "predicate": load_json(
            repo_path("tests/fixtures/h2/map-event-predicate-results-static-v1.json")
        ),
    }
    calls: list[str] = []
    monkeypatch.setattr(lifecycle_module, "build_map_events_contract", lambda *_args: maps)
    for attribute, output_name in (
        ("build_map_event_direct_state_contract", "state"),
        ("build_map_event_direct_control_contract", "control"),
        ("build_map_event_direct_handoff_contract", "handoff"),
        ("build_map_event_predicate_results_contract", "predicate"),
    ):
        monkeypatch.setattr(
            lifecycle_module,
            attribute,
            lambda *_args, output=outputs[output_name], name=output_name: (
                calls.append(name) or output
            ),
        )
    _maps, owners = lifecycle_module._fresh_retained_owners(ROM, UPSTREAM)
    assert calls == ["state", "control", "handoff", "predicate"]
    assert owners == lifecycle_module._RETAINED_OWNER_EXPECTED
    broken = deepcopy(outputs["state"])
    broken["summary"]["contextInstructionSiteCount"] += 1
    monkeypatch.setattr(
        lifecycle_module, "build_map_event_direct_state_contract", lambda *_args: broken
    )
    with pytest.raises(ValueError, match="retained direct-state fixture drift"):
        lifecycle_module._fresh_retained_owners(ROM, UPSTREAM)


def test_index_delta_is_exact_and_rejects_stale_or_extra_associations() -> None:
    current = load_json(INDEX)
    lifecycle_current = _remove_map_event_cross_program_flag_state_later_owner_index_delta(
        _remove_map_event_flag_route_selection_later_owner_index_delta(current)
    )
    predecessor = _remove_map_event_flag_lifecycle_state_later_owner_index_delta(lifecycle_current)
    assert lifecycle_module._sha(canonical_json_bytes(predecessor)) == _PREDECESSOR_INDEX_SHA256
    current_by_id = {row["id"]: row for row in lifecycle_current["records"]}
    previous_by_id = {row["id"]: row for row in predecessor["records"]}
    changed = {
        record_id
        for record_id, record in current_by_id.items()
        if record != previous_by_id[record_id]
    }
    assert changed == {
        "map.setup.entity-event",
        "map.setup.zone-event",
        "map.setup.item-event",
        "tech.interrupts.trap-flags",
    }

    def index_totals(value: dict[str, object]) -> dict[str, int]:
        return {
            "records": len(value["records"]),
            "addresses": sum(len(row["addresses"]) for row in value["records"]),
            "h2Evidence": sum(
                evidence["level"] == "H2"
                for row in value["records"]
                for evidence in row["evidence"]
            ),
            "bindings": sum(
                len(evidence["bindings"])
                for row in value["records"]
                for evidence in row["evidence"]
            ),
            "documentRefs": sum(len(row["documents"]) for row in value["records"]),
            "documents": len(
                {document for row in value["records"] for document in row["documents"]}
            ),
            "designContracts": len(
                {
                    contract
                    for row in value["records"]
                    for contract in row.get("designContracts", [])
                }
            ),
        }

    current_totals = index_totals(lifecycle_current)
    previous_totals = index_totals(predecessor)
    assert {name: current_totals[name] - previous_totals[name] for name in current_totals} == {
        "records": 0,
        "addresses": 0,
        "h2Evidence": 4,
        "bindings": 4,
        "documentRefs": 4,
        "documents": 1,
        "designContracts": 0,
    }
    assert current_totals["records"] == 1627
    assert current_totals["bindings"] == 3071
    assert current_totals["documents"] == 61
    assert current_totals["designContracts"] == 68
    assert normalize_map_event_flag_lifecycle_state_later_owner_index(lifecycle_current)

    stale = deepcopy(lifecycle_current)
    next(record for record in stale["records"] if record["id"] == "map.setup.entity-event")[
        "documents"
    ].append("docs/research/map-event-flag-lifecycle-state.md")
    with pytest.raises(ValueError, match="index delta"):
        _remove_map_event_flag_lifecycle_state_later_owner_index_delta(stale)


def test_research_index_schema_admits_only_the_four_new_fixture_fields() -> None:
    index = load_json(INDEX)
    validate_json(index, INDEX_SCHEMA, owner="map-event flag lifecycle research index")
    fixture_id = "sf2-map-event-flag-lifecycle-state-static-v1"
    fields = {
        binding["fixtureField"]
        for record in index["records"]
        for evidence in record["evidence"]
        if evidence["fixtureId"] == fixture_id
        for binding in evidence["bindings"]
    }
    assert fields == {
        "flagLifecycleState.dispatchEntries.entityEvent",
        "flagLifecycleState.dispatchEntries.zoneEvent",
        "flagLifecycleState.dispatchEntries.itemEvent",
        "flagLifecycleState.serviceDefinitions.chkFlg.trapEntryAddress",
    }
    for field in (
        "flagLifecycleState.dispatchEntries.unknown",
        "sourceContext.flagLifecycleState.dispatchEntries.entityEvent",
    ):
        altered = deepcopy(index)
        evidence = next(
            evidence
            for record in altered["records"]
            for evidence in record["evidence"]
            if evidence["fixtureId"] == fixture_id
        )
        evidence["bindings"][0]["fixtureField"] = field
        with pytest.raises(ValueError):
            validate_json(altered, INDEX_SCHEMA, owner="fixture-field near miss")


def test_id_and_public_fixture_path_are_stable() -> None:
    assert ID == "sf2-map-event-flag-lifecycle-state-static-v1"
    assert FIXTURE.name == "map-event-flag-lifecycle-state-static-v1.json"
