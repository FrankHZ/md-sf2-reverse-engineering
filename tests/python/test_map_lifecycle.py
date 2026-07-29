from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.h3.map_lifecycle import (
    FIXTURE,
    FIXTURE_SCHEMA,
    OBSERVATION_SCHEMA,
    OBSERVER,
    _derive_case_expectations,
    _listing_reset_tail_address,
    _listing_section_call_sites,
    _map_lifecycle_runtime_equates,
    _pack_load_map_d0_word_at_call,
    _reset_current_map_load_followup,
    _runtime_navigation,
)
from sf2tool.jsonio import load_json, validate_json

ROM = Path("local/roms/sf2-us.bin")
UPSTREAM = Path("local/upstream/SF2DISASM")


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _record(case: dict[str, object]) -> dict[str, object]:
    expected = case["expected"]
    assert isinstance(expected, dict)
    return {
        **expected,
        "handlerReturned": True,
        "currentMapAfter": case["currentMapAfter"],
        **case["runtimeGolden"],
    }


def _observation() -> dict[str, object]:
    fixture = _fixture()
    cases = fixture["cases"]
    assert isinstance(cases, list)
    return {
        "system": "GEN",
        "core": "Genesis Plus GX",
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in cases],
        "records": [_record(case) for case in cases],
    }


def test_map_lifecycle_static_fixture_and_navigation_are_complete() -> None:
    fixture = _fixture()
    validate_json(fixture, FIXTURE_SCHEMA, owner="map lifecycle fixture")
    static = build_map_script_engine_contract(ROM, UPSTREAM)

    assert _derive_case_expectations(static, fixture, UPSTREAM) == [
        case["expected"] for case in fixture["cases"]
    ]
    assert _runtime_navigation(static, fixture, UPSTREAM) == {
        "entryAddress": 292092,
        "handlerAddresses": {
            "resetMap": 288142,
            "loadMapFadeIn": 288154,
            "reloadMap": 288520,
            "mapLoad": 288182,
        },
        "callSitesByMacro": {
            "resetMap": [{"address": 288144, "target": "ResetCurrentMap"}],
            "loadMapFadeIn": [
                {"address": 288192, "target": "LoadMapTilesets"},
                {"address": 288196, "target": "WaitForVInt"},
                {"address": 288236, "target": "LoadMap"},
                {"address": 288242, "target": "EnableDisplayAndInterrupts"},
                {"address": 288254, "target": "WaitForVInt"},
            ],
            "reloadMap": [
                {"address": 288558, "target": "LoadMap"},
                {"address": 288564, "target": "EnableDisplayAndInterrupts"},
                {"address": 288576, "target": "WaitForVInt"},
            ],
            "mapLoad": [
                {"address": 288192, "target": "LoadMapTilesets"},
                {"address": 288196, "target": "WaitForVInt"},
                {"address": 288236, "target": "LoadMap"},
                {"address": 288242, "target": "EnableDisplayAndInterrupts"},
                {"address": 288254, "target": "WaitForVInt"},
            ],
        },
        "resetTailAddress": 15932,
        "layoutMarkers": {
            "layoutClearStartMarkerSeed": 42330,
            "layoutClearEndMarkerSeed": 23130,
            "layoutClearSpanByteCount": 8192,
        },
    }
    assert fixture["ram"] == {
        "currentMapAddress": 16774929,
        "viewTargetEntityAddress": 16754732,
        "fadingSettingAddress": 16768752,
        "viewPlaneAPixelXAddress": 16754704,
        "viewPlaneAPixelYAddress": 16754706,
        "layoutClearStartMarkerAddress": 16711680,
        "layoutClearEndMarkerAddress": 16719870,
    }
    assert fixture["layoutMarkers"] == {
        "layoutClearStartMarkerSeed": 42330,
        "layoutClearEndMarkerSeed": 23130,
        "layoutClearSpanByteCount": 8192,
    }
    assert fixture["runtimeQuestions"] == [
        "map-lifecycle/layout-collision-pathfinding-effects",
        "map-lifecycle/entity-reload-player-placement",
        "map-lifecycle/presentation-fade-hardware-timing",
        "map-lifecycle/story-reachability-persistence",
    ]
    assert fixture["runtimeQuestions"] == load_json(FIXTURE_SCHEMA)["properties"][
        "runtimeQuestions"
    ]["const"]
    assert [case["runtimeGolden"] for case in fixture["cases"]] == [
        {
            "viewPlaneAPixelX": 0,
            "viewPlaneAPixelY": 12288,
            "layoutClearStartMarkerCleared": True,
            "layoutClearStartMarkerReplaced": True,
            "layoutClearEndMarkerCleared": True,
            "layoutClearEndMarkerReplaced": True,
        },
        {
            "viewPlaneAPixelX": 384,
            "viewPlaneAPixelY": 13056,
            "layoutClearStartMarkerCleared": True,
            "layoutClearStartMarkerReplaced": True,
            "layoutClearEndMarkerCleared": True,
            "layoutClearEndMarkerReplaced": True,
        },
        {
            "viewPlaneAPixelX": 1152,
            "viewPlaneAPixelY": 14208,
            "layoutClearStartMarkerCleared": False,
            "layoutClearStartMarkerReplaced": False,
            "layoutClearEndMarkerCleared": False,
            "layoutClearEndMarkerReplaced": False,
        },
        {
            "viewPlaneAPixelX": 2304,
            "viewPlaneAPixelY": 14976,
            "layoutClearStartMarkerCleared": False,
            "layoutClearStartMarkerReplaced": True,
            "layoutClearEndMarkerCleared": True,
            "layoutClearEndMarkerReplaced": True,
        },
        {
            "viewPlaneAPixelX": 2304,
            "viewPlaneAPixelY": 14976,
            "layoutClearStartMarkerCleared": True,
            "layoutClearStartMarkerReplaced": True,
            "layoutClearEndMarkerCleared": True,
            "layoutClearEndMarkerReplaced": True,
        },
    ]


def test_load_map_d0_word_derivation_truncates_each_guarded_word_use_site() -> None:
    static = build_map_script_engine_contract(ROM, UPSTREAM)
    handler = next(
        row
        for row in static["mapLifecycleCommandFacts"]["handlers"]
        if row["macro"] == "mapLoad"
    )
    assert _pack_load_map_d0_word_at_call(
        macro="mapLoad", handler=handler, operand_words=[3, 0x101, 5]
    ) == (783, 3, 3)

    changed = deepcopy(handler)
    changed["sectionGuard"]["operandPackUseSites"]["shiftUseSite"]["instruction"] = (
        "lsl.l #BYTE_SHIFT_COUNT,d0"
    )
    with pytest.raises(ValueError, match="D0 shift width drift"):
        _pack_load_map_d0_word_at_call(
            macro="mapLoad", handler=changed, operand_words=[3, 0x101, 5]
        )


@pytest.mark.parametrize(
    ("name", "mutation"),
    [
        (
            "missing-runtime-golden-field",
            lambda value: value["cases"][0]["runtimeGolden"].pop(
                "layoutClearEndMarkerReplaced"
            ),
        ),
        (
            "renamed-runtime-golden-field",
            lambda value: value["cases"][0]["runtimeGolden"].update(
                {
                    "layout_clear_end_marker_replaced": value["cases"][0][
                        "runtimeGolden"
                    ].pop(
                        "layoutClearEndMarkerReplaced"
                    )
                }
            ),
        ),
        (
            "extra-runtime-golden-field",
            lambda value: value["cases"][0]["runtimeGolden"].update(
                {"unexpected": True}
            ),
        ),
        ("reordered-cases", lambda value: value["cases"].reverse()),
        (
            "runtime-golden-word-boundary",
            lambda value: value["cases"][1]["runtimeGolden"].__setitem__(
                "viewPlaneAPixelX", 65536
            ),
        ),
        ("reordered-runtime-question-queue", lambda value: value["runtimeQuestions"].reverse()),
    ],
)
def test_map_lifecycle_fixture_schema_rejects_nested_mutations(
    name: str, mutation: object
) -> None:
    fixture = deepcopy(_fixture())
    mutation(fixture)
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(fixture, FIXTURE_SCHEMA, owner=name)


@pytest.mark.parametrize(
    ("name", "mutation"),
    [
        (
            "missing-observed-marker-field",
            lambda value: value["records"][0].pop("layoutClearStartMarkerCleared"),
        ),
        (
            "renamed-observed-marker-field",
            lambda value: value["records"][0].update(
                {
                    "layout_clear_start_marker_cleared": value["records"][0].pop(
                        "layoutClearStartMarkerCleared"
                    )
                }
            ),
        ),
        (
            "extra-observed-marker-field",
            lambda value: value["records"][0].update({"unexpected": True}),
        ),
        ("reordered-record-order", lambda value: value["recordOrder"].reverse()),
        (
            "camera-word-boundary",
            lambda value: value["records"][0].__setitem__("viewPlaneAPixelX", 65536),
        ),
    ],
)
def test_map_lifecycle_observation_schema_rejects_nested_mutations(
    name: str, mutation: object
) -> None:
    observation = _observation()
    validate_json(observation, OBSERVATION_SCHEMA, owner="map lifecycle observation")
    mutation(observation)
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(observation, OBSERVATION_SCHEMA, owner=name)


def test_reset_current_map_parser_guards_loop_span_and_tail(tmp_path: Path) -> None:
    source_path = tmp_path / "disasm/code/gameflow/exploration/exploration.asm"
    source_path.parent.mkdir(parents=True)
    source = """\
ResetCurrentMap:
    lea     (FF0000_RAM_START).l,a2
    move.w  #MAP_LAYOUT_LONGS_COUNTER,d7
@Clear_Loop:
    clr.l   (a2)+
    dbf     d7,@Clear_Loop
    clr.w   d0
    moveq   #-1,d1
    bra.w   LoadMap
; End of function ResetCurrentMap
"""
    source_path.write_text(source, encoding="utf-8")

    assert _reset_current_map_load_followup(tmp_path) == {
        "sourcePath": "code/gameflow/exploration/exploration.asm",
        "layoutStartInstruction": "lea (FF0000_RAM_START).l,a2",
        "layoutCounterInstruction": "move.w #MAP_LAYOUT_LONGS_COUNTER,d7",
        "layoutClearInstruction": "clr.l (a2)+",
        "layoutLoopInstruction": "dbf d7,@Clear_Loop",
        "layoutClearUnitByteCount": 4,
        "clearInstruction": "clr.w d0",
        "selectorInstruction": "moveq #-1,d1",
        "transferInstruction": "bra.w LoadMap",
        "loadMapD0WordAtTransfer": 0,
        "loadMapD1WordAtTransfer": 65535,
    }
    with pytest.raises(ValueError, match="layout clear width"):
        source_path.write_text(source.replace("clr.l", "clr.w"), encoding="utf-8")
        _reset_current_map_load_followup(tmp_path)
    with pytest.raises(ValueError, match="load order"):
        source_path.write_text(
            source.replace("clr.w   d0\n    moveq", "moveq   #-1,d1\n    clr.w   d0\n    moveq"),
            encoding="utf-8",
        )
        _reset_current_map_load_followup(tmp_path)


def test_runtime_equate_parser_keeps_decimal_loop_count_and_hex_ram_addresses(
    tmp_path: Path,
) -> None:
    disasm = tmp_path / "disasm"
    disasm.mkdir()
    (disasm / "sf2const.asm").write_text(
        """\
FF0000_RAM_START: equ $FF0000
VIEW_PLANE_A_PIXEL_X: equ $FFA810
VIEW_PLANE_A_PIXEL_Y: equ $FFA812
CURRENT_MAP: equ $FFF711
VIEW_TARGET_ENTITY: equ $FFA82C
FADING_SETTING: equ $FFDEF0
""",
        encoding="utf-8",
    )
    (disasm / "sf2enums.asm").write_text(
        "MAP_LAYOUT_LONGS_COUNTER: equ 2047\n", encoding="utf-8"
    )
    assert _map_lifecycle_runtime_equates(tmp_path) == {
        "FF0000_RAM_START": 16711680,
        "VIEW_PLANE_A_PIXEL_X": 16754704,
        "VIEW_PLANE_A_PIXEL_Y": 16754706,
        "MAP_LAYOUT_LONGS_COUNTER": 2047,
        "CURRENT_MAP": 16774929,
        "VIEW_TARGET_ENTITY": 16754732,
        "FADING_SETTING": 16768752,
    }


def test_h1_call_site_and_reset_tail_parsers_reject_smallest_listing_mutations() -> None:
    listing = """\
000465B6 csc48_loadMap:
000465C0 4EB9 jsr (LoadMapTilesets).w
000465C4 4EB9 jsr (WaitForVInt).w
000465EC 4EB9 jsr (LoadMap).w
000465F2 4EB9 jsr (EnableDisplayAndInterrupts).w
000465FE 4EB9 jsr (WaitForVInt).w
00046600 ; End of function csc48_loadMap
00003E20 ResetCurrentMap:
00003E3C 6000 bra.w LoadMap
00003E40 ; End of function ResetCurrentMap
"""
    targets = [
        "LoadMapTilesets",
        "WaitForVInt",
        "LoadMap",
        "EnableDisplayAndInterrupts",
        "WaitForVInt",
    ]
    assert _listing_section_call_sites(listing, "csc48_loadMap", targets) == [
        {"address": 288192, "target": "LoadMapTilesets"},
        {"address": 288196, "target": "WaitForVInt"},
        {"address": 288236, "target": "LoadMap"},
        {"address": 288242, "target": "EnableDisplayAndInterrupts"},
        {"address": 288254, "target": "WaitForVInt"},
    ]
    assert _listing_reset_tail_address(listing) == 15932
    with pytest.raises(ValueError, match="target/order"):
        _listing_section_call_sites(
            listing.replace("jsr (LoadMap).w", "jsr (LoadMapTilesets).w"),
            "csc48_loadMap",
            targets,
        )
    with pytest.raises(ValueError, match="transfer use-site"):
        _listing_reset_tail_address(listing.replace("bra.w LoadMap", "bsr.w LoadMap"))


def test_map_lifecycle_lua_observes_markers_only_after_handler_return() -> None:
    source = OBSERVER.read_text(encoding="utf-8")
    assert source.count("memorysavestate.savecorestate") == 1
    assert "config.function" not in source
    assert 'config["function"]' in source
    assert "memory.write_u16_be(config.ram.layoutClearStartMarkerAddress" in source
    assert "config.layoutMarkers.layoutClearStartMarkerSeed" in source
    assert "viewPlaneAPixelX=memory.read_u16_be" in source
    assert "layoutClearStartMarkerCleared=start_cleared" in source
    assert "timeout:frame-budget-exhausted" in source
    assert "client.exitCode(exit_code)" in source
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
