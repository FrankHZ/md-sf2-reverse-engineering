"""Focused adversarial tests for cross-program direct map-event flag candidates."""

from __future__ import annotations

from copy import deepcopy
from pathlib import Path
from shutil import copy2

import pytest

import sf2tool.h2.map_event_cross_program_flag_state as cross_module
from sf2tool.h2.map_event_cross_program_flag_state import (
    _PREDECESSOR_INDEX_SHA256,
    _RETAINED_OWNER_EXPECTED,
    FIXTURE,
    ID,
    RESEARCH_INDEX,
    SCHEMA,
    _guarded_retained_owners,
    _project,
    _remove_map_event_cross_program_flag_state_later_owner_index_delta,
    _sha,
    _validate_order,
    canonical_json_bytes,
    normalize_map_event_cross_program_flag_state_later_owner_index,
)
from sf2tool.h2.map_event_flag_route_selection import (
    _remove_map_event_flag_route_selection_later_owner_index_delta,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

ROM = repo_path("local/roms/sf2-us.bin")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
INDEX_SCHEMA = repo_path("schemas/research-index.schema.json")


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _project_fixture(
    *,
    map_events: dict[str, object] | None = None,
    lifecycle: dict[str, object] | None = None,
    rom_path: Path = ROM,
    upstream_path: Path = UPSTREAM,
) -> dict[str, object]:
    fixture = _fixture()
    return _project(
        deepcopy(load_map_events_fixture()["expected"] if map_events is None else map_events),
        deepcopy(
            load_json(repo_path("tests/fixtures/h2/map-event-flag-lifecycle-state-static-v1.json"))
            if lifecycle is None
            else lifecycle
        ),
        deepcopy(fixture["retainedOwners"]),
        rom_path,
        upstream_path,
    )


def _source_surface(tmp_path: Path, *, copy_rom: bool = False) -> tuple[Path, Path]:
    """Copy only the assigned macro/map-event source surface plus H1/ROM."""
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


def test_complete_projection_is_closed_and_has_exact_static_denominators() -> None:
    fixture = _fixture()
    validate_json(fixture, SCHEMA, owner="map-event cross-program flag-state fixture")
    _validate_order(fixture)
    assert _project_fixture() == fixture
    facts = fixture["crossProgramFlagState"]
    assert fixture["summary"] == {
        "motherProgramContextCount": 914,
        "motherOperationCount": 3579,
        "sourceIdentityCount": 91,
        "mapEventSourceFileCount": 90,
        "contextualAccessSiteCount": 493,
        "physicalAccessSiteCount": 490,
        "contextualReadAccessSiteCount": 316,
        "physicalReadAccessSiteCount": 314,
        "contextualSetAccessSiteCount": 169,
        "physicalSetAccessSiteCount": 168,
        "contextualClearAccessSiteCount": 8,
        "physicalClearAccessSiteCount": 8,
        "contextualImmediateReadConsumerCount": 316,
        "physicalImmediateReadConsumerCount": 314,
        "contextualAnchorPcCount": 809,
        "physicalAnchorPcCount": 804,
        "contextualAnchorEncodedByteCount": 2610,
        "physicalAnchorByteCount": 2592,
        "overlapAnchorByteCount": 18,
        "programContextCount": 195,
        "readerProgramContextCount": 190,
        "writerProgramContextCount": 135,
        "numericFlagCount": 151,
        "readFlagCount": 128,
        "writtenFlagCount": 114,
        "readWriteOverlapFlagCount": 91,
        "readOnlyFlagCount": 37,
        "writeOnlyFlagCount": 23,
        "sameProgramLifecycleFlagCount": 82,
        "sameProgramLifecycleRelationCount": 131,
        "sameProgramOnlyFlagCount": 42,
        "bothSameAndCrossProgramFlagCount": 40,
        "crossProgramOnlyFlagCount": 9,
        "crossProgramFlagCount": 49,
        "crossProgramCandidateCount": 720,
    }
    assert facts["categoryRoles"] == {
        "entityEvents": 292378,
        "zoneEvents": 292122,
        "itemEvents": 292230,
    }
    assert facts["serviceJoin"] == {"trapEntryAddress": 5888}
    assert facts["categoryPairTotals"] == [
        {"writerCategory": "entityEvents", "readerCategory": "entityEvents", "candidateCount": 374},
        {"writerCategory": "entityEvents", "readerCategory": "itemEvents", "candidateCount": 2},
        {"writerCategory": "entityEvents", "readerCategory": "zoneEvents", "candidateCount": 123},
        {"writerCategory": "itemEvents", "readerCategory": "entityEvents", "candidateCount": 2},
        {"writerCategory": "itemEvents", "readerCategory": "zoneEvents", "candidateCount": 1},
        {"writerCategory": "zoneEvents", "readerCategory": "entityEvents", "candidateCount": 182},
        {"writerCategory": "zoneEvents", "readerCategory": "zoneEvents", "candidateCount": 36},
    ]
    assert len(facts["programDomain"]) == 914
    assert len(facts["readerAccessSites"]) == 316
    assert len(facts["writerAccessSites"]) == 177
    assert len(facts["readerCohorts"]) == 310
    assert len(facts["writerCohorts"]) == 171
    assert len(facts["crossProgramCandidates"]) == 720
    assert len(facts["physicalContextCoverage"]["physicalAnchors"]) == 804
    assert fixture["unknowns"] == {key: "Unknown" for key in cross_module._UNKNOWN_KEYS}


def test_mother_access_consumer_and_cross_program_derivations_reject_mutation() -> None:
    missing_mother = deepcopy(load_map_events_fixture()["expected"])
    missing_mother["itemTargetPrograms"] = missing_mother["itemTargetPrograms"][:-1]
    with pytest.raises(ValueError, match="mother corpus"):
        _project_fixture(map_events=missing_mother)

    wrong_direct_access = deepcopy(load_map_events_fixture()["expected"])
    wrong_direct_access["directFlagAccessSites"][0]["sourceMacro"] = "setFlg"
    with pytest.raises(ValueError, match="direct access identity"):
        _project_fixture(map_events=wrong_direct_access)

    wrong_consumer = deepcopy(load_map_events_fixture()["expected"])
    reader = next(
        row for row in wrong_consumer["directFlagAccessSites"] if row["accessKind"] == "read"
    )
    reader["conditionConsumer"]["branchPolarity"] = "equal"
    with pytest.raises(ValueError, match="consumer polarity/target"):
        _project_fixture(map_events=wrong_consumer)

    wrong_lifecycle = deepcopy(
        load_json(repo_path("tests/fixtures/h2/map-event-flag-lifecycle-state-static-v1.json"))
    )
    wrong_lifecycle["flagLifecycleState"]["lifecycleRelations"] = wrong_lifecycle[
        "flagLifecycleState"
    ]["lifecycleRelations"][:-1]
    with pytest.raises(ValueError, match="lifecycle join"):
        _project_fixture(lifecycle=wrong_lifecycle)


def test_source_h1_rom_and_macro_mutations_fail_before_fixture_comparison(tmp_path: Path) -> None:
    upstream, _rom = _source_surface(tmp_path)
    macro = upstream / "disasm/sf2macros.asm"
    macro.write_text(
        macro.read_text(encoding="utf-8").replace("trap #SET_FLAG", "trap #CLEAR_FLAG", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="macro emission"):
        _project_fixture(upstream_path=upstream)

    upstream, _rom = _source_surface(tmp_path / "source")
    access = _fixture()["crossProgramFlagState"]["readerAccessSites"][0]
    source = upstream / "disasm" / access["sourcePath"]
    lines = source.read_text(encoding="utf-8").splitlines(keepends=True)
    lines[access["sourceLine"] - 1] = lines[access["sourceLine"] - 1].replace(
        str(access["flagNumber"]), str(access["flagNumber"] + 1), 1
    )
    source.write_text("".join(lines), encoding="utf-8")
    with pytest.raises(ValueError, match="source opcode/operand"):
        _project_fixture(upstream_path=upstream)

    upstream, _rom = _source_surface(tmp_path / "h1")
    address = _fixture()["crossProgramFlagState"]["physicalContextCoverage"]["physicalAnchors"][0][
        "address"
    ]
    listing = upstream / "build/sf2build-h1.lst"
    original = listing.read_text(encoding="utf-8")
    encoded = cross_module._h1_instruction_rows(original)[address][0].hex().upper()
    h1_line = next(
        line for line in original.splitlines() if line.split()[:2] == [f"{address:08X}", encoded]
    )
    old_byte = h1_line.split()[1][:2]
    listing.write_text(
        original.replace(h1_line, h1_line.replace(old_byte, f"{int(old_byte, 16) ^ 1:02X}", 1), 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="H1/ROM byte"):
        _project_fixture(upstream_path=upstream)

    upstream, rom = _source_surface(tmp_path / "rom", copy_rom=True)
    rom_bytes = bytearray(rom.read_bytes())
    rom_bytes[address] ^= 1
    rom.write_bytes(rom_bytes)
    with pytest.raises(ValueError, match="ROM identity"):
        _project_fixture(rom_path=rom, upstream_path=upstream)


def test_candidate_self_edge_reversal_membership_and_cohort_changes_fail_closed() -> None:
    fixture = _fixture()
    facts = fixture["crossProgramFlagState"]

    self_edge = deepcopy(fixture)
    candidate = self_edge["crossProgramFlagState"]["crossProgramCandidates"][0]
    candidate["readerCohortOrder"] = candidate["writerCohortOrder"]
    with pytest.raises(ValueError, match="candidate membership/self-edge"):
        _validate_order(self_edge)

    reversed_edge = deepcopy(fixture)
    candidate = next(
        row
        for row in reversed_edge["crossProgramFlagState"]["crossProgramCandidates"]
        if row["readerCohortOrder"] < len(reversed_edge["crossProgramFlagState"]["writerCohorts"])
    )
    candidate["writerCohortOrder"], candidate["readerCohortOrder"] = (
        candidate["readerCohortOrder"],
        candidate["writerCohortOrder"],
    )
    with pytest.raises(ValueError, match="candidate membership/self-edge"):
        _validate_order(reversed_edge)

    missing_candidate = deepcopy(fixture)
    missing_candidate["crossProgramFlagState"]["crossProgramCandidates"].pop()
    with pytest.raises(ValueError, match="candidate order"):
        _validate_order(missing_candidate)

    wrong_pair_total = deepcopy(fixture)
    wrong_pair_total["crossProgramFlagState"]["categoryPairTotals"][0]["candidateCount"] += 1
    with pytest.raises(ValueError, match="category-pair"):
        _validate_order(wrong_pair_total)

    wrong_alias = deepcopy(fixture)
    anchor = next(
        row
        for row in wrong_alias["crossProgramFlagState"]["physicalContextCoverage"][
            "physicalAnchors"
        ]
        if len(row["contextRoleKeys"]) > 1
    )
    anchor["contextRoleKeys"] = anchor["contextRoleKeys"][:1]
    with pytest.raises(ValueError, match="physical/context coverage"):
        _validate_order(wrong_alias)

    assert facts["crossProgramCandidates"][0]["writerCohortOrder"] < len(facts["writerCohorts"])


def test_schema_is_recursively_closed_and_public_safe() -> None:
    fixture = _fixture()
    for mutate in (
        lambda value: value.__setitem__("runtime", {}),
        lambda value: value["crossProgramFlagState"]["readerAccessSites"][0].__setitem__(
            "rawRomBytes", "private"
        ),
        lambda value: value["crossProgramFlagState"]["physicalContextCoverage"]["physicalAnchors"][
            0
        ].__setitem__("extra", True),
        lambda value: value["sourceContext"]["sourceIdentities"][0].__setitem__(
            "privatePath", "local/roms/sf2-us.bin"
        ),
    ):
        altered = deepcopy(fixture)
        mutate(altered)
        with pytest.raises(ValueError):
            validate_json(altered, SCHEMA, owner="cross-program closure mutation")
    assert "local/" not in FIXTURE.read_text(encoding="utf-8")
    assert "rawRomBytes" not in FIXTURE.read_text(encoding="utf-8")


def test_retained_owners_are_hash_locked_before_projection(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    _map_events, _lifecycle, owners = _guarded_retained_owners(ROM, UPSTREAM)
    assert owners == _RETAINED_OWNER_EXPECTED
    monkeypatch.setattr(cross_module, "_fixture_sha256", lambda _path: "0" * 64)
    with pytest.raises(ValueError, match="retained owner identity/hash"):
        _guarded_retained_owners(ROM, UPSTREAM)


def test_index_delta_is_exact_and_chains_predecessor_normalization() -> None:
    current = load_json(RESEARCH_INDEX)
    cross_current = _remove_map_event_flag_route_selection_later_owner_index_delta(current)
    predecessor = _remove_map_event_cross_program_flag_state_later_owner_index_delta(cross_current)
    assert _sha(canonical_json_bytes(predecessor)) == _PREDECESSOR_INDEX_SHA256
    current_by_id = {row["id"]: row for row in cross_current["records"]}
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
    assert normalize_map_event_cross_program_flag_state_later_owner_index(cross_current)

    stale = deepcopy(cross_current)
    next(row for row in stale["records"] if row["id"] == "map.setup.entity-event")[
        "documents"
    ].append("docs/research/map-event-cross-program-flag-state.md")
    with pytest.raises(ValueError, match="index delta"):
        _remove_map_event_cross_program_flag_state_later_owner_index_delta(stale)


def test_research_index_schema_admits_only_the_four_cross_program_fixture_fields() -> None:
    index = load_json(RESEARCH_INDEX)
    validate_json(index, INDEX_SCHEMA, owner="cross-program research index")
    fields = {
        binding["fixtureField"]
        for record in index["records"]
        for evidence in record["evidence"]
        if evidence["fixtureId"] == ID
        for binding in evidence["bindings"]
    }
    assert fields == {
        "crossProgramFlagState.categoryRoles.entityEvents",
        "crossProgramFlagState.categoryRoles.zoneEvents",
        "crossProgramFlagState.categoryRoles.itemEvents",
        "crossProgramFlagState.serviceJoin.trapEntryAddress",
    }
    for field in (
        "crossProgramFlagState.categoryRoles.unknown",
        "sourceContext.crossProgramFlagState.categoryRoles.entityEvents",
    ):
        altered = deepcopy(index)
        evidence = next(
            evidence
            for record in altered["records"]
            for evidence in record["evidence"]
            if evidence["fixtureId"] == ID
        )
        evidence["bindings"][0]["fixtureField"] = field
        with pytest.raises(ValueError):
            validate_json(altered, INDEX_SCHEMA, owner="cross-program fixture-field near miss")


def test_id_and_public_fixture_path_are_stable() -> None:
    assert ID == "sf2-map-event-cross-program-flag-state-static-v1"
    assert FIXTURE.name == "map-event-cross-program-flag-state-static-v1.json"
