"""Focused adversarial tests for the Map 21 static transition projection."""

from __future__ import annotations

from copy import deepcopy
from pathlib import Path
from shutil import copy2

import pytest

import sf2tool.h2.map_event_scripted_transition_state as transition_module
from sf2tool.h2.map_event_flag_lifecycle_state import (
    _remove_map_event_flag_lifecycle_state_later_owner_index_delta,
)
from sf2tool.h2.map_event_scripted_transition_state import (
    _PREDECESSOR_INDEX_SHA256,
    FIXTURE,
    ID,
    SCHEMA,
    _project,
    _remove_map_event_scripted_transition_state_later_owner_index_delta,
    _selected_programs,
    _validate_order,
    build_map_event_scripted_transition_state_contract,
    canonical_json_bytes,
    normalize_map_event_scripted_transition_state_later_owner_index,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

ROM = repo_path("local/roms/sf2-us.bin")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
RESEARCH_INDEX = repo_path("manifests/research-index.json")


def _source_surface(tmp_path: Path) -> Path:
    upstream = tmp_path / "SF2DISASM"
    for source_path in transition_module._SOURCE_PATHS:
        destination = upstream / "disasm" / source_path
        destination.parent.mkdir(parents=True, exist_ok=True)
        copy2(UPSTREAM / "disasm" / source_path, destination)
    listing = upstream / "build/sf2build-h1.lst"
    listing.parent.mkdir(parents=True, exist_ok=True)
    copy2(UPSTREAM / "build/sf2build-h1.lst", listing)
    return upstream


def _project_from_source(upstream: Path) -> dict[str, object]:
    return _project(
        load_map_events_fixture()["expected"],
        disasm=upstream / "disasm",
        listing_text=(upstream / "build/sf2build-h1.lst").read_text(encoding="utf-8"),
        rom=ROM.read_bytes(),
    )


def test_complete_static_projection_matches_the_closed_fixture() -> None:
    actual = build_map_event_scripted_transition_state_contract(ROM, UPSTREAM)
    assert actual == load_json(FIXTURE)
    state = actual["scriptedTransitionState"]
    assert state["sourceProgram"] == {
        "canonicalSymbol": "Map21_DefaultZoneEvent",
        "sourcePath": "data/maps/entries/map44/mapsetups/scripts.asm",
        "entrySourceLine": 24,
        "entryAddress": 345526,
        "enclosingScriptEntryAddress": 345464,
        "endAddressExclusive": 345876,
        "terminalAddress": 345874,
        "encodedSpanBytes": 350,
        "referenceCounts": {
            "physicalRecordCount": 1,
            "setupRecordReferenceCount": 4,
            "routeRecordReferenceCount": 4,
        },
        "operationWeightCounts": {
            "uniquePhysicalOperationCount": 87,
            "physicalRecordWeightedOperationCount": 87,
            "setupRecordReferenceWeightedOperationCount": 348,
            "routeRecordReferenceWeightedOperationCount": 348,
        },
    }
    assert state["selectionSummary"] == {
        "motherProgramCount": 914,
        "positiveProgramCount": 1,
        "zeroProgramCount": 913,
        "sourceOperationCount": 87,
        "commandDefinitionCount": 27,
        "payloadContextCount": 4,
        "inheritedPayloadContextCount": 1,
        "retainedHandlerCount": 19,
        "pointerTargetCount": 5,
        "addressAnchorCount": 111,
    }
    assert [row["sourceOrder"] for row in state["operationRows"]] == list(range(87))
    assert sum(row["h1Rom"]["encodedByteLength"] for row in state["operationRows"]) == 350
    assert [row["symbol"] for row in state["pointerTargets"]] == list(
        transition_module._POINTER_TARGETS
    )
    assert len(state["retainedHandlers"]) == 19
    assert {row["handler"] for row in state["retainedHandlers"].values()} == {
        "csc32_setCameraDestInTiles",
        "rjt_cutsceneScriptCommands",
        "csc23_setEntityFacing",
        "csc34_setBlocks",
        "csc00_displaySingleTextbox",
        "csc15_setEntityActscript",
        "csc2D_entityActionSequence",
        "csc05_playSound",
        "csc0A_executeSubroutine",
        "csc07_warp",
        "csc37_loadMapAndFadeIn",
        "csc42_loadMapEntities",
        "csc1A_setEntitySprite",
        "csc39_fadeInFromBlack",
        "csc14_setEntityActscriptManual",
        "rjt_EntityScriptCommands",
        "csc33_setQuakeAmount",
        "csc41_flashScreenWhite",
        "csc10_toggleFlag",
    }
    assert sum(row["inheritedAtProgramEntry"] for row in state["payloadContexts"]) == 1
    assert actual["unknowns"] == {key: "Unknown" for key in transition_module._UNKNOWN_KEYS}


@pytest.mark.parametrize("mutation", ("missing-mother", "missing-positive", "extra-positive"))
def test_selection_denominators_reject_missing_and_extra_contexts(mutation: str) -> None:
    altered = deepcopy(load_map_events_fixture()["expected"])
    if mutation == "missing-mother":
        altered["itemTargetPrograms"] = altered["itemTargetPrograms"][:-1]
    elif mutation == "missing-positive":
        program = next(
            row
            for row in altered["zoneTargetPrograms"]
            if row["canonicalSymbol"] == "Map21_DefaultZoneEvent"
        )
        for operation in program["operations"]:
            operation["family"] = "data-directive"
    else:
        program = altered["entityTargetPrograms"][0]
        program["operations"][0]["family"] = "map-script-macro"
    with pytest.raises(ValueError, match="mother program|selected program"):
        _selected_programs(altered)


def test_source_program_and_macro_definition_mutations_fail_before_fixture(tmp_path: Path) -> None:
    upstream = _source_surface(tmp_path)
    program_source = upstream / "disasm/data/maps/entries/map44/mapsetups/scripts.asm"
    program_source.write_text(
        program_source.read_text(encoding="utf-8").replace("csWait 30", "csWait 31", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="source operation"):
        _project_from_source(upstream)

    upstream = _source_surface(tmp_path / "macro")
    macro_source = upstream / "disasm/sf2cutscenemacros.asm"
    macro_source.write_text(
        macro_source.read_text(encoding="utf-8").replace("dc.b $80", "dc.b $81", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="macro definition emission"):
        _project_from_source(upstream)


def test_h1_rows_and_rom_intervals_are_structural_and_public_safe() -> None:
    fixture = load_json(FIXTURE)
    rows = fixture["scriptedTransitionState"]["operationRows"]
    assert all(set(row["h1Rom"]) == {"encodedByteLength", "h1Sha256", "romSha256"} for row in rows)
    assert all("sourceStatement" not in row and "rawRomBytes" not in row for row in rows)
    assert all(row["address"] >= 345526 for row in rows)
    assert rows[-1]["sourceMacro"] == "csc_end"
    assert rows[-1]["address"] == 345874


def test_h1_and_rom_mutations_change_the_structural_fixture_projection(tmp_path: Path) -> None:
    upstream = _source_surface(tmp_path)
    listing = upstream / "build/sf2build-h1.lst"
    listing.write_text(
        listing.read_text(encoding="utf-8").replace("000545B6 03", "000545B6 04", 1),
        encoding="utf-8",
    )
    assert _project_from_source(upstream) != load_json(FIXTURE)

    rom = bytearray(ROM.read_bytes())
    rom[345526] ^= 1
    assert _project(
        load_map_events_fixture()["expected"],
        disasm=UPSTREAM / "disasm",
        listing_text=(UPSTREAM / "build/sf2build-h1.lst").read_text(encoding="utf-8"),
        rom=bytes(rom),
    ) != load_json(FIXTURE)


def test_schema_and_order_guards_reject_private_runtime_and_order_mutations() -> None:
    fixture = load_json(FIXTURE)
    for mutation in (
        ("runtime", lambda value: value.__setitem__("runtimeTrace", {})),
        (
            "private",
            lambda value: value["scriptedTransitionState"]["operationRows"][0].__setitem__(
                "rawRomBytes", "private"
            ),
        ),
    ):
        altered = deepcopy(fixture)
        mutation[1](altered)
        with pytest.raises(ValueError):
            validate_json(altered, SCHEMA, owner=mutation[0])
    altered = deepcopy(fixture)
    altered["scriptedTransitionState"]["operationRows"][0:2] = list(
        reversed(altered["scriptedTransitionState"]["operationRows"][0:2])
    )
    with pytest.raises(ValueError, match="operation order"):
        _validate_order(altered)
    assert "Weigh anchor" not in FIXTURE.read_text(encoding="utf-8")
    assert "local/" not in FIXTURE.read_text(encoding="utf-8")


def test_id_and_fixture_path_are_stable() -> None:
    assert ID == "sf2-map-event-scripted-transition-state-static-v1"
    assert FIXTURE.name == "map-event-scripted-transition-state-static-v1.json"


def test_latest_index_normalizer_removes_only_this_exact_delta() -> None:
    current = _remove_map_event_flag_lifecycle_state_later_owner_index_delta(
        load_json(RESEARCH_INDEX)
    )
    predecessor = _remove_map_event_scripted_transition_state_later_owner_index_delta(current)
    assert transition_module._sha(canonical_json_bytes(predecessor)) == _PREDECESSOR_INDEX_SHA256
    current_records = {row["id"]: row for row in current["records"]}
    predecessor_records = {row["id"]: row for row in predecessor["records"]}
    changed = {
        record_id
        for record_id, record in current_records.items()
        if record != predecessor_records[record_id]
    }
    assert changed == {
        "map.data.cs-54578",
        "scripting.map.mapscriptengine-1",
        "scripting.map.mapscriptengine-2",
        "map.entity-placement.set-facing",
        "map.block-mutation.set-blocks-handler",
        "map.script-dialogue.next-single-text",
        "map.entity-action-bridge.set-actscript",
        "map.entity-action-bridge.entity-actions",
        "map.script-control-audio.runtime",
        "scripting.map.transition-runtime-boundary",
        "map.entity-population.load-map-entities",
        "map.entity-lifecycle-presentation.set-sprite",
        "map.script-screen-presentation.fade-in",
        "map.entity-action-bridge.custom-actscript",
        "scripting.entity.dispatch-table",
        "map.script-screen-presentation.set-quake",
        "map.script-screen-presentation.flash-white",
    }
    assert normalize_map_event_scripted_transition_state_later_owner_index(current)
