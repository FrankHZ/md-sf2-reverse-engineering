from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.h3.map_interaction_trigger import (
    FIXTURE,
    FIXTURE_SCHEMA,
    OBSERVATION_SCHEMA,
    OBSERVER,
    _callee_use_sites,
    _derived_callee_constants,
    _direct_call_site,
    _section,
    build_map_interaction_trigger_contract,
    derive_case_expectations,
)
from sf2tool.jsonio import load_json, validate_json

ROM = Path("local/roms/sf2-us.bin")
UPSTREAM = Path("local/upstream/SF2DISASM")


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _observation() -> dict[str, object]:
    fixture = _fixture()
    records = []
    for case in fixture["cases"]:
        expected = case["expected"]
        runtime_golden = case["runtimeGolden"]
        assert isinstance(expected, dict)
        assert isinstance(runtime_golden, dict)
        records.append(
            {
                **{name: value for name, value in expected.items() if name != "currentMapSeed"},
                "handlerReturned": True,
                **runtime_golden,
            }
        )
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": records,
    }


def test_map_interaction_trigger_static_contract_and_fixture_are_exact() -> None:
    fixture = _fixture()
    validate_json(fixture, FIXTURE_SCHEMA, owner="map interaction trigger fixture")
    assert fixture["function"] == {
        "entryAddress": 292092,
        "roofHandlerAddress": 288438,
        "stepHandlerAddress": 288582,
        "roofDirectCalleeCallSiteAddress": 288450,
        "stepDirectCalleeCallSiteAddress": 288594,
    }
    static = build_map_interaction_trigger_contract(ROM, UPSTREAM)
    assert static["function"] == {
        "entryAddress": 292092,
        "roofHandlerAddress": 288438,
        "stepHandlerAddress": 288582,
        "roofDirectCalleeCallSiteAddress": 288450,
        "stepDirectCalleeCallSiteAddress": 288594,
        "roofTableScanAddress": 16424,
        "roofSelectedRecordAddress": 16448,
        "stepTableScanAddress": 16000,
        "stepSelectedRecordAddress": 16020,
    }
    assert static["ram"] == {
        "layoutBaseAddress": 16711680,
        "currentMapAddress": 16774929,
        "currentBattleAddress": 16774930,
        "busyWordAddress": 16756546,
        "mapAreaLayerTypeAddress": 16756550,
        "updateToggleBitfieldAddress": 16754733,
    }
    assert static["constants"] == {
        "mapTileSize": 384,
        "coordinateInputShiftBits": 7,
        "coordinateHashMask": 63,
        "mapIndexShiftBits": 2,
        "recordStrideByteCount": 8,
        "recordTerminatorByteCount": 2,
        "layoutRowShiftBits": 6,
        "notCurrentlyInBattleByte": 255,
    }
    assert {
        kind: {key: value for key, value in table.items() if key != "records"}
        for kind, table in static["tables"].items()
    } == {
        "roofEvents": {
            "mapIndex": 2,
            "kind": "roofEvents",
            "symbol": "Map02s5_RoofEvents",
            "address": 616978,
            "recordCount": 9,
            "recordStrideByteCount": 8,
            "terminatorByteCount": 2,
        },
        "stepEvents": {
            "mapIndex": 2,
            "kind": "stepEvents",
            "symbol": "Map02s4_StepEvents",
            "address": 616904,
            "recordCount": 9,
            "recordStrideByteCount": 8,
            "terminatorByteCount": 2,
        },
    }
    assert [table["records"][0] for table in static["tables"].values()] == [
        {
            "trigger": {"x": 9, "y": 13},
            "source": {"x": 255, "y": 255},
            "size": {"width": 9, "height": 7},
            "destination": {"x": 3, "y": 38},
        },
        {
            "trigger": {"x": 9, "y": 13},
            "source": {"x": 48, "y": 0},
            "size": {"width": 1, "height": 1},
            "destination": {"x": 9, "y": 13},
        },
    ]
    assert derive_case_expectations(static, fixture, UPSTREAM) == [
        case["expected"] for case in fixture["cases"]
    ]
    assert fixture["runtimeQuestions"] == [
        "map-interaction-trigger/full-layout-collision-pathfinding-effects",
        "map-interaction-trigger/presentation-audio-timing-hardware-effects",
        "map-interaction-trigger/persistence-story-reachability",
    ]
    assert [case["id"] for case in fixture["cases"]] == [
        "roof-map02-record0-hit",
        "roof-map02-terminator-miss",
        "roof-map02-record0-busy-gate",
        "step-map02-record0-hit",
        "step-map02-terminator-miss",
        "step-map02-record0-battle-gate",
    ]
    assert [case["runtimeGolden"] for case in fixture["cases"]] == [
        {
            "currentMapAfter": 2,
            "matchBoundaryObserved": "selected-record",
            "terminatorBoundaryObserved": False,
            "layoutDestinationMarkerChanged": True,
            "layoutDestinationMarkerMatchesSourceMarker": None,
            "updateToggleBit0Set": True,
            "updateToggleBit1Set": False,
            "busyWordAfter": 1,
            "currentBattleByteAfter": 255,
        },
        {
            "currentMapAfter": 2,
            "matchBoundaryObserved": "terminator",
            "terminatorBoundaryObserved": True,
            "layoutDestinationMarkerChanged": False,
            "layoutDestinationMarkerMatchesSourceMarker": None,
            "updateToggleBit0Set": False,
            "updateToggleBit1Set": False,
            "busyWordAfter": 0,
            "currentBattleByteAfter": 255,
        },
        {
            "currentMapAfter": 2,
            "matchBoundaryObserved": "busy-gate",
            "terminatorBoundaryObserved": False,
            "layoutDestinationMarkerChanged": False,
            "layoutDestinationMarkerMatchesSourceMarker": None,
            "updateToggleBit0Set": False,
            "updateToggleBit1Set": False,
            "busyWordAfter": 1,
            "currentBattleByteAfter": 255,
        },
        {
            "currentMapAfter": 2,
            "matchBoundaryObserved": "selected-record",
            "terminatorBoundaryObserved": False,
            "layoutDestinationMarkerChanged": True,
            "layoutDestinationMarkerMatchesSourceMarker": True,
            "updateToggleBit0Set": False,
            "updateToggleBit1Set": True,
            "busyWordAfter": 0,
            "currentBattleByteAfter": 255,
        },
        {
            "currentMapAfter": 2,
            "matchBoundaryObserved": "terminator",
            "terminatorBoundaryObserved": True,
            "layoutDestinationMarkerChanged": False,
            "layoutDestinationMarkerMatchesSourceMarker": False,
            "updateToggleBit0Set": False,
            "updateToggleBit1Set": False,
            "busyWordAfter": 0,
            "currentBattleByteAfter": 255,
        },
        {
            "currentMapAfter": 2,
            "matchBoundaryObserved": "battle-gate",
            "terminatorBoundaryObserved": False,
            "layoutDestinationMarkerChanged": False,
            "layoutDestinationMarkerMatchesSourceMarker": False,
            "updateToggleBit0Set": False,
            "updateToggleBit1Set": False,
            "busyWordAfter": 0,
            "currentBattleByteAfter": 0,
        },
    ]


def test_h2_handler_caller_breakdown_remains_the_static_invocation_authority() -> None:
    facts = build_map_script_engine_contract(ROM, UPSTREAM)["mapInteractionTriggerCommandFacts"]
    assert facts["callerBreakdown"] == {
        "callerHandlers": [
            {
                "handler": "csc43_RoofEvent",
                "instructionTargetSiteCounts": {
                    "PerformMapBlockCopyScript": 1,
                    "OpenDoor": 0,
                },
                "effectiveTargetSiteCounts": {
                    "PerformMapBlockCopyScript": 1,
                    "OpenDoor": 0,
                },
            },
            {
                "handler": "csc47_StepEvent",
                "instructionTargetSiteCounts": {
                    "PerformMapBlockCopyScript": 0,
                    "OpenDoor": 1,
                },
                "effectiveTargetSiteCounts": {
                    "PerformMapBlockCopyScript": 0,
                    "OpenDoor": 1,
                },
            },
        ],
        "targetResolutions": [
            {
                "instructionTarget": "PerformMapBlockCopyScript",
                "effectiveTarget": "PerformMapBlockCopyScript",
                "aliasSourcePath": None,
                "effectiveTargetScope": "external",
            },
            {
                "instructionTarget": "OpenDoor",
                "effectiveTarget": "OpenDoor",
                "aliasSourcePath": None,
                "effectiveTargetScope": "external",
            },
        ],
        "instructionTargetTotals": {"PerformMapBlockCopyScript": 1, "OpenDoor": 1},
        "effectiveTargetTotals": {"PerformMapBlockCopyScript": 1, "OpenDoor": 1},
        "internalEffectiveTargetTotals": {"PerformMapBlockCopyScript": 0, "OpenDoor": 0},
        "externalEffectiveTargetTotals": {"PerformMapBlockCopyScript": 1, "OpenDoor": 1},
    }


@pytest.mark.parametrize(
    ("name", "mutation"),
    [
        (
            "missing-nested-state-field",
            lambda value: value["cases"][0]["initialState"].pop("busyWord"),
        ),
        (
            "missing-nested-function-field",
            lambda value: value["function"].pop("roofDirectCalleeCallSiteAddress"),
        ),
        (
            "renamed-nested-marker-field",
            lambda value: value["cases"][3]["markerSeeds"].update(
                {
                    "layout_source_word_seed": value["cases"][3]["markerSeeds"].pop(
                        "layoutSourceWordSeed"
                    )
                }
            ),
        ),
        (
            "extra-nested-expected-field",
            lambda value: value["cases"][0]["expected"].update({"unexpected": 1}),
        ),
        (
            "missing-runtime-golden-field",
            lambda value: value["cases"][0]["runtimeGolden"].pop("layoutDestinationMarkerChanged"),
        ),
        ("reordered-cases", lambda value: value["cases"].reverse()),
        (
            "word-boundary",
            lambda value: value["cases"][0]["initialState"].__setitem__("busyWord", 65536),
        ),
        (
            "record-index-boundary",
            lambda value: value["cases"][0]["table"].__setitem__("recordIndex", 9),
        ),
    ],
)
def test_map_interaction_trigger_fixture_schema_rejects_full_object_mutations(
    name: str, mutation: object
) -> None:
    del name
    mutated = deepcopy(_fixture())
    assert callable(mutation)
    mutation(mutated)
    with pytest.raises(ValueError, match="fixture failed schema validation"):
        validate_json(mutated, FIXTURE_SCHEMA, owner="map interaction trigger fixture")


@pytest.mark.parametrize(
    ("name", "mutation"),
    [
        (
            "missing-nested-hash-field",
            lambda value: value["records"][0]["hashedTriggerTile"].pop("x"),
        ),
        (
            "renamed-record-field",
            lambda value: value["records"][0].update(
                {"handler_address": value["records"][0].pop("handlerAddress")}
            ),
        ),
        (
            "extra-nested-hash-field",
            lambda value: value["records"][0]["hashedTriggerTile"].update({"unexpected": 1}),
        ),
        ("reordered-observations", lambda value: value["records"].reverse()),
        (
            "direct-call-word-boundary",
            lambda value: value["records"][0].__setitem__("calleeD0WordAtDirectCall", 65536),
        ),
    ],
)
def test_map_interaction_trigger_observation_schema_rejects_full_object_mutations(
    name: str, mutation: object
) -> None:
    del name
    mutated = _observation()
    assert callable(mutation)
    mutation(mutated)
    with pytest.raises(ValueError, match="observation failed schema validation"):
        validate_json(mutated, OBSERVATION_SCHEMA, owner="map interaction trigger observation")


def test_map_interaction_trigger_observation_schema_accepts_complete_skeleton_shape() -> None:
    validate_json(_observation(), OBSERVATION_SCHEMA, owner="map interaction trigger observation")


def test_callee_parser_rejects_guard_and_mutation_source_drift() -> None:
    source = (UPSTREAM / "disasm/code/gameflow/exploration/exploration.asm").read_text(
        encoding="utf-8"
    )
    use_sites = _callee_use_sites(source)
    derived = _derived_callee_constants(use_sites)
    assert derived["values"] == {
        "coordinateInputShiftBits": 7,
        "coordinateHashMask": 63,
        "recordStrideByteCount": 8,
        "recordTerminatorByteCount": 2,
        "layoutRowShiftBits": 6,
    }
    assert {
        kind: {
            name: [(row["opcode"], row["operand"]) for row in rows] for name, rows in groups.items()
        }
        for kind, groups in use_sites.items()
    } == {
        "step": {
            "battleGate": [
                ("cmpi.b", "#not_currently_in_battle,((current_battle-$1000000)).w"),
                ("bne.w", "@return"),
            ],
            "selector": [
                ("lsr.w", "#7,d0"),
                ("lsr.w", "#7,d1"),
                ("lea", "table_mapoffsethash(pc),a2"),
                ("add.w", "d0,d0"),
                ("move.b", "(a2,d0.w),d0"),
                ("andi.w", "#$3f,d0"),
                ("add.w", "d1,d1"),
                ("move.b", "(a2,d1.w),d1"),
                ("andi.w", "#$3f,d1"),
                ("move.b", "((current_map-$1000000)).w,d7"),
                ("lsl.w", "#index_shift_count,d7"),
                ("movea.l", "mapdata_offset_event_step(a2),a2"),
            ],
            "match": [
                ("tst.b", "(a2)"),
                ("bmi.w", "@done"),
                ("cmp.b", "(a2),d0"),
                ("bne.w", "@nextevent"),
                ("cmp.b", "1(a2),d1"),
                ("bne.w", "@nextevent"),
                ("tst.w", "(a2)+"),
                ("addq.l", "#8,a2"),
                ("addq.w", "#1,d2"),
                ("bra.w", "@main_loop"),
            ],
            "mutation": [
                ("lsl.w", "#6,d3"),
                ("add.w", "d3,d2"),
                ("add.w", "d2,d2"),
                ("lsl.w", "#6,d1"),
                ("add.w", "d1,d0"),
                ("add.w", "d0,d0"),
                ("tst.w", "d1"),
                ("blt.s", "loc_3eec"),
                ("move.w", "(a2,d0.w),(a2,d2.w)"),
                ("clr.w", "(a2,d2.w)"),
            ],
            "updateToggle": [
                ("tst.b", "((map_area_layer_type-$1000000)).w"),
                ("beq.s", "@updateplaneb"),
                ("bset", "#0,((view_plane_update_toggle_bitfield-$1000000)).w"),
                ("bra.s", "@done"),
                ("bset", "#1,((view_plane_update_toggle_bitfield-$1000000)).w"),
            ],
        },
        "roof": {
            "busyGate": [
                ("tst.w", "((word_ffaf42-$1000000)).w"),
                ("bne.w", "loc_40e6"),
            ],
            "selector": [
                ("lsr.w", "#7,d0"),
                ("lsr.w", "#7,d1"),
                ("lea", "table_mapoffsethash(pc),a3"),
                ("add.w", "d0,d0"),
                ("move.b", "(a3,d0.w),d0"),
                ("andi.w", "#$3f,d0"),
                ("add.w", "d1,d1"),
                ("move.b", "(a3,d1.w),d1"),
                ("andi.w", "#$3f,d1"),
                ("move.b", "((current_map-$1000000)).w,d7"),
                ("lsl.w", "#index_shift_count,d7"),
                ("movea.l", "mapdata_offset_event_roof(a2),a2"),
            ],
            "match": [
                ("tst.b", "(a2)"),
                ("bmi.w", "loc_40e6"),
                ("cmp.b", "(a2),d0"),
                ("bne.w", "loc_40ea"),
                ("cmp.b", "1(a2),d1"),
                ("bne.w", "loc_40ea"),
                ("move.w", "d2,((word_ffaf42-$1000000)).w"),
                ("tst.w", "(a2)+"),
                ("addq.l", "#8,a2"),
                ("addq.w", "#1,d2"),
                ("bra.w", "loc_4028"),
            ],
            "mutation": [
                ("lsl.w", "#6,d3"),
                ("add.w", "d3,d2"),
                ("add.w", "d2,d2"),
                ("lsl.w", "#6,d1"),
                ("add.w", "d1,d0"),
                ("add.w", "d0,d0"),
                ("tst.w", "d1"),
                ("blt.s", "loc_40ba"),
                ("move.w", "(a2,d0.w),(a2,d2.w)"),
                ("clr.w", "(a2,d2.w)"),
                ("bset", "#0,((view_plane_update_toggle_bitfield-$1000000)).w"),
            ],
        },
    }
    with pytest.raises(ValueError, match="OpenDoor battle gate"):
        _callee_use_sites(source.replace("bne.w   @Return", "beq.w   @Return", 1))
    step_source_offset = source.index("OpenDoor:")
    mutated_step_source = source[:step_source_offset] + source[step_source_offset:].replace(
        "bset    #1,((VIEW_PLANE_UPDATE_TOGGLE_BITFIELD-$1000000)).w",
        "bset    #2,((VIEW_PLANE_UPDATE_TOGGLE_BITFIELD-$1000000)).w",
        1,
    )
    with pytest.raises(ValueError, match="update-toggle branch"):
        _callee_use_sites(mutated_step_source)
    with pytest.raises(ValueError, match="record stride"):
        _callee_use_sites(source.replace("addq.l  #8,a2", "addq.l  #10,a2", 1))
    with pytest.raises(ValueError, match="coordinate and step-table selector"):
        _callee_use_sites(source.replace("andi.w  #$3F,d0", "andi.w  #$3E,d0", 1))


def test_instruction_parser_handles_comments_and_legal_size_suffixes() -> None:
    source = """Example:
    move.w  d0,d1 ; an operand comment must not change the instruction
    bra.s   @Loop
    rts
    ; End of function Example
"""
    assert _section(source, "Example") == [
        ("move.w", "d0,d1", 2),
        ("bra.s", "@loop", 3),
        ("rts", "", 4),
    ]
    near_miss = source.replace("move.w", "move.wx", 1)
    with pytest.raises(ValueError, match="cannot parse"):
        _section(near_miss, "Example")


def test_h1_direct_call_parser_ignores_comments_and_rejects_near_miss_targets() -> None:
    listing = """00000000 csc43_RoofEvent:
00000010 4EB8 3FEA                                  jsr     (PerformMapBlockCopyScript).w
00000016                                             ; jsr (PerformMapBlockCopyScript).w
00000018 ; End of function csc43_RoofEvent
"""
    assert _direct_call_site(listing, "csc43_RoofEvent", "PerformMapBlockCopyScript") == 16
    with pytest.raises(ValueError, match="direct call-site drift"):
        _direct_call_site(listing, "csc43_RoofEvent", "PerformMapBlockCopyScripts")


def test_map_interaction_trigger_lua_syntax_preflight() -> None:
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
