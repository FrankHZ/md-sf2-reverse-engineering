from __future__ import annotations

from copy import deepcopy

import pytest

from sf2tool.h2 import remaining_core
from sf2tool.h2.remaining_core import (
    _debug_direct_call_counts,
    _debug_longword_pointer_counts,
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


def test_debug_schema_definition_is_identical_for_output_and_fixture() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)

    assert output_schema["definitions"]["debugFacts"] == fixture_schema["definitions"]["debugFacts"]


def test_debug_schema_recursively_closes_every_object() -> None:
    definition = load_json(OUTPUT_SCHEMA)["definitions"]["debugFacts"]

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


def test_debug_instruction_parsers_exclude_comments_and_keep_target_identity(tmp_path) -> None:
    source = tmp_path / "debug-references.asm"
    source.write_text(
        """\
                bsr.s   DebugModeSelectHits
label:          jsr.l   (CheatModeConfiguration).l
                bsr.w   DebugModeSelectHits
;               bsr.w   DebugModeActionSelect
                dc.l    DebugModeBattleTest
                dc.b    'jsr.w CheatModeConfiguration'
                dc.l    CheatModeConfiguration trailing
""",
        encoding="utf-8",
    )
    targets = {"DebugModeBattleTest", "CheatModeConfiguration", "DebugModeSelectHits"}

    assert _debug_direct_call_counts(source, targets) == {
        "CheatModeConfiguration": 1,
        "DebugModeSelectHits": 2,
    }
    assert _debug_longword_pointer_counts(source, targets) == {"DebugModeBattleTest": 1}


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
def test_debug_static_contract_is_complete_and_matches_fixture() -> None:
    fixture = load_json(FIXTURE_PATH)
    facts = build_remaining_core_inventory(UPSTREAM)["debugFacts"]

    assert facts == fixture["expected"]["debugFacts"]
    assert facts["functionEntries"] == {
        "DebugModeBattleTest": 30364,
        "LoadAllyStatsDecimalDigits": 30908,
        "LevelUpWholeForce": 31008,
        "GetDecimalDigits": 31024,
        "CheatModeConfiguration": 32314,
        "DebugModeActionSelect": 39578,
        "DebugModeSelectTargetEnemy": 39748,
        "DebugModeSelectHits": 39768,
    }
    assert facts["derived"] == {
        "wholeForceCount": 30,
        "joinedRosterCount": 29,
        "genericListEntryCount": 32,
        "actionCount": 7,
        "actionMaxIndex": 6,
        "enemyTargetCount": 32,
        "magicLevelFirstIndex": 1,
        "magicLevelLastIndex": 4,
    }
    assert len(facts["sourceLabels"]["enumValues"]) == 18
    assert facts["battleTest"]["flow"]["battlePromptRange"] == [0, 49]
    assert facts["battleTest"]["flow"]["shopPromptRange"] == [0, 100]
    assert facts["battleActions"]["targetEnemyPromptRange"] == [128, 159]
    assert facts["battleActions"]["magicSpellPromptRange"] == [0, 42]
    assert facts["battleActions"]["itemPromptRange"] == [0, 127]
    assert facts["externalDirectCallerOccurrences"] == {
        "code/gameflow/battle/battleactions/battleactionsengine_1.asm": {
            "DebugModeActionSelect": 1,
            "DebugModeSelectHits": 1,
        },
        "code/specialscreens/witch/witchstart.asm": {"CheatModeConfiguration": 2},
    }
    assert facts["externalDirectCallSiteCounts"] == {
        "DebugModeBattleTest": 0,
        "LoadAllyStatsDecimalDigits": 0,
        "LevelUpWholeForce": 0,
        "GetDecimalDigits": 0,
        "CheatModeConfiguration": 2,
        "DebugModeActionSelect": 1,
        "DebugModeSelectTargetEnemy": 0,
        "DebugModeSelectHits": 1,
    }
    assert facts["runtimeQuestions"] == [
        "debug-flow-input-chords-menu-selection-and-action-state-matrix"
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


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_debug_schemas_reject_nested_rename_delete_and_value_mutations() -> None:
    fixture = load_json(FIXTURE_PATH)
    renamed_fixture = deepcopy(fixture)
    callers = renamed_fixture["expected"]["debugFacts"]["externalDirectCallerOccurrences"]
    callers["code/specialscreens/witch/renamed.asm"] = callers.pop(
        "code/specialscreens/witch/witchstart.asm"
    )
    with pytest.raises(ValueError, match="debugFacts"):
        validate_json(renamed_fixture, FIXTURE_SCHEMA, owner="renamed debug fixture")

    deleted_fixture = deepcopy(fixture)
    del deleted_fixture["expected"]["debugFacts"]["battleActions"]["hitOverrides"][0]["stackOffset"]
    with pytest.raises(ValueError, match="debugFacts"):
        validate_json(deleted_fixture, FIXTURE_SCHEMA, owner="deleted debug fixture")

    malformed_output = build_remaining_core_inventory(UPSTREAM)
    malformed_output["debugFacts"]["battleActions"]["relativeJumpTargets"] = []
    with pytest.raises(ValueError, match="debugFacts"):
        validate_json(malformed_output, OUTPUT_SCHEMA, owner="mutated debug output")

    extra_fixture = deepcopy(fixture)
    extra_fixture["expected"]["debugFacts"]["battleActions"]["hitOverrides"][0]["extra"] = True
    with pytest.raises(ValueError, match="debugFacts"):
        validate_json(extra_fixture, FIXTURE_SCHEMA, owner="extra debug fixture property")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("source_name", "old", "new"),
    [
        ("battletest.asm", "bsr.w   j_SetBaseDef", "bsr.w   j_SetBaseDefChanged"),
        ("configurationmode.asm", "txt     452", "txt     453"),
        ("debugmodebattleactions.asm", "lsl.w   #6,d3", "lsl.w   #5,d3"),
        (
            "debugmodebattleactions.asm",
            "dc.w @Attack-rjt_DebugModeBattleactions",
            "dc.w @Attack-WrongBase",
        ),
        (
            "debugmodebattleactions.asm",
            (
                "moveq   #ITEMINDEX_MAX,d2\r\n                jsr     j_NumberPrompt\r\n"
                "                move.w  d0,(a0)+"
            ),
            "moveq   #ITEMINDEX_MAX,d2\r\n                jsr     j_NumberPrompt",
        ),
        ("battletest.asm", "bne.w   byte_77DE", "beq.w   byte_77DE"),
    ],
)
def test_debug_source_mutations_fail_guards_or_exact_contract(
    monkeypatch, source_name: str, old: str, new: str
) -> None:
    original_reader = remaining_core.read_upstream_text

    def mutated_reader(path):
        source = original_reader(path)
        if path.name == source_name:
            assert old in source
            return source.replace(old, new, 1)
        return source

    monkeypatch.setattr(remaining_core, "read_upstream_text", mutated_reader)
    with pytest.raises(ValueError):
        output = remaining_core.build_remaining_core_inventory(UPSTREAM)
        validate_json(output, OUTPUT_SCHEMA, owner="mutated debug source output")
