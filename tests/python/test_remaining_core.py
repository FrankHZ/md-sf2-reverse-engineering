from __future__ import annotations

from copy import deepcopy

import pytest

from sf2tool.h2.remaining_core import (
    _window_direct_call_counts,
    _window_doubling_scale,
    _window_longword_pointer_counts,
    build_remaining_core_inventory,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/remaining-core-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-remaining-core-static-fixture.schema.json")
FIXTURE_PATH = repo_path("tests/fixtures/h2/remaining-core-static-v1.json")


def test_window_schema_definition_is_identical_for_output_and_fixture() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)

    assert (
        output_schema["definitions"]["windowFacts"] == fixture_schema["definitions"]["windowFacts"]
    )


def test_window_schema_recursively_closes_every_object() -> None:
    definition = load_json(OUTPUT_SCHEMA)["definitions"]["windowFacts"]

    def assert_closed(value: object) -> None:
        if isinstance(value, dict):
            if value.get("type") == "object":
                assert value.get("additionalProperties") is False
                assert set(value["required"]) == set(value["properties"])
            for child in value.values():
                assert_closed(child)
        elif isinstance(value, list):
            for child in value:
                assert_closed(child)

    assert_closed(definition)


def test_window_direct_call_parser_uses_only_instruction_fields(tmp_path) -> None:
    source = tmp_path / "window-calls.asm"
    source.write_text(
        """\
                bsr.s   CreateWindow
                jsr.w   (MoveWindow).l
label:          bsr.w   DeleteWindow
;               bsr.s   CreateWindow
                dc.l    CreateWindow
                dc.b    'bsr.s CreateWindow'
macro           bsr.s   CreateWindow
                bsr.s   (a0)
""",
        encoding="utf-8",
    )

    assert _window_direct_call_counts(source, {"CreateWindow", "MoveWindow", "DeleteWindow"}) == {
        "CreateWindow": 1,
        "DeleteWindow": 1,
        "MoveWindow": 1,
    }


def test_window_longword_pointer_parser_keeps_target_identity(tmp_path) -> None:
    source = tmp_path / "window-pointers.asm"
    source.write_text(
        """\
                dc.l    VInt_UpdateWindows
label:          dc.l    CreateWindow
;               dc.l    VInt_UpdateWindows
                dc.w    VInt_UpdateWindows
                dc.b    'dc.l VInt_UpdateWindows'
macro           dc.l    VInt_UpdateWindows
                dc.l    VInt_UpdateWindows trailing
""",
        encoding="utf-8",
    )

    assert _window_longword_pointer_counts(source, {"CreateWindow", "VInt_UpdateWindows"}) == {
        "CreateWindow": 1,
        "VInt_UpdateWindows": 1,
    }


def test_window_doubling_scale_derives_from_self_add_operation_count() -> None:
    assert _window_doubling_scale("add.w d0,d0", "single") == 2
    with pytest.raises(ValueError, match="doubling"):
        _window_doubling_scale("add.w d0,d1", "changed")
    with pytest.raises(ValueError, match="doubling"):
        _window_doubling_scale("add.w d0,d0\nadd.w d1,d1", "multiple")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_window_static_contract_is_complete_and_matches_fixture() -> None:
    fixture = load_json(FIXTURE_PATH)
    facts = build_remaining_core_inventory(UPSTREAM)["windowFacts"]

    assert facts == fixture["expected"]["windowFacts"]
    assert facts["derived"] == {
        "windowSlotCount": 8,
        "entrySizeBytes": 16,
        "entryAddressShiftBits": 4,
        "clearLongwordCount": 32,
        "clearSpanBytes": 128,
        "layoutTileWordBytes": 2,
        "mapTileColumns": 32,
        "mapCoordinateXMask": 31,
        "mapRowStrideBytes": 64,
        "coordinateXShiftBits": 8,
        "coordinateYMask": 255,
    }
    assert facts["addressFormulas"] == {
        "windowEntryAddress": "WINDOW_ENTRIES plus slotIndexTimesEntrySizeBytes",
        "windowTileAddress": (
            "layoutAddress plus "
            "(packedCoordinateYTimesWidthPlusPackedCoordinateX)TimesLayoutTileWordBytes"
        ),
        "mapLayoutOffset": (
            "(packedCoordinateYTimesMapTileColumnsPlusPackedCoordinateXModuloMapTileColumns)"
            "TimesLayoutTileWordBytes"
        ),
    }
    assert facts["externalDirectCallSiteCounts"] == {
        "InitializeWindowProperties": 7,
        "CreateWindow": 33,
        "SetWindowDestination": 33,
        "FixWindowsPositions": 1,
        "sub_48BE": 0,
        "CopyPlaneALayoutForWindows": 1,
        "MoveWindowWithSfx": 66,
        "MoveWindow": 13,
        "DeleteWindow": 31,
        "WaitForWindowMovementEnd": 45,
        "VInt_UpdateWindows": 0,
        "sub_4AC8": 0,
        "sub_4B5C": 0,
        "sub_4BEA": 0,
        "GetWindowEntryAddress": 2,
        "GetWindowTileAddress": 30,
    }
    assert facts["externalLongwordPointerOccurrences"] == {
        "code/gameflow/battle/battlescenes/initializebattlescene.asm": {"VInt_UpdateWindows": 1},
        "code/gameflow/battle/battlevints.asm": {"VInt_UpdateWindows": 1},
        "code/gameflow/special/battletest.asm": {"VInt_UpdateWindows": 1},
        "code/gameflow/start/gameinit.asm": {"VInt_UpdateWindows": 1},
        "code/specialscreens/witch/witchfunctions.asm": {"VInt_UpdateWindows": 1},
        "code/specialscreens/witch/witchstart.asm": {"VInt_UpdateWindows": 1},
    }
    assert facts["runtimeQuestions"] == [
        "window-presentation-matrix-animation-hide-fix-scroll-clip-and-dma"
    ]


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_window_schemas_reject_nested_rename_delete_and_value_mutations() -> None:
    fixture = load_json(FIXTURE_PATH)
    renamed_fixture = deepcopy(fixture)
    callers = renamed_fixture["expected"]["windowFacts"]["externalDirectCallerOccurrences"]
    callers["code/common/menus/renamed.asm"] = callers.pop("code/common/menus/itemmenu.asm")
    with pytest.raises(ValueError, match="windowFacts"):
        validate_json(renamed_fixture, FIXTURE_SCHEMA, owner="renamed window fixture")

    deleted_fixture = deepcopy(fixture)
    del deleted_fixture["expected"]["windowFacts"]["entryLayout"]["fields"][0]["role"]
    with pytest.raises(ValueError, match="windowFacts"):
        validate_json(deleted_fixture, FIXTURE_SCHEMA, owner="deleted window fixture")

    malformed_output = build_remaining_core_inventory(UPSTREAM)
    malformed_output["windowFacts"]["externalDirectCallerOccurrences"][
        "code/common/menus/itemmenu.asm"
    ]["GetWindowTileAddress"] = 5
    with pytest.raises(ValueError, match="windowFacts"):
        validate_json(malformed_output, OUTPUT_SCHEMA, owner="mutated window output")
