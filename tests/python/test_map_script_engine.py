from collections import Counter
from copy import deepcopy
from hashlib import sha256
from json import dumps
from pathlib import Path

import pytest
from jsonschema import Draft7Validator, FormatChecker

from sf2tool.h2 import map_script_engine
from sf2tool.h2.map_script_engine import (
    DIALOGUE_HANDLER_BY_MACRO,
    DIALOGUE_MACROS,
    DIALOGUE_MODIFIER_MACROS,
    ENTITY_DIALOGUE_CONSUMER,
    ENTITY_DIALOGUE_CONSUMER_PATH,
    PORTRAIT_HANDLER,
    _active_party_section_guard,
    _cursor_flow,
    _dialogue_handler_facts,
    _direct_call_sites,
    _emission_rows,
    _entity_action_bridge_branch_target_record,
    _entity_action_bridge_cursor_use_site,
    _entity_action_bridge_inline_payload,
    _entity_action_bridge_macro_facts,
    _entity_action_bridge_payload_invocation,
    _entity_action_bridge_section_guard,
    _entity_clone_cursor_read_use_site,
    _entity_clone_field_read_use_site,
    _entity_clone_field_write_use_site,
    _entity_clone_macro_annotations,
    _entity_clone_section_guard,
    _entity_dialogue_consumer_facts,
    _entity_gesture_relationship_motion_branch_target_record,
    _entity_gesture_relationship_motion_cursor_read_use_site,
    _entity_gesture_relationship_motion_macro_annotations,
    _entity_gesture_relationship_motion_section_guard,
    _entity_lifecycle_presentation_branch_target_record,
    _entity_lifecycle_presentation_cursor_read_use_site,
    _entity_lifecycle_presentation_macro_annotations,
    _entity_lifecycle_presentation_section_guard,
    _entity_placement_branch_target_record,
    _entity_placement_cursor_read_use_site,
    _entity_placement_macro_annotations,
    _entity_placement_section_guard,
    _entity_placement_update_entity_sprite_wrapper_use_site,
    _entity_population_macro_annotations,
    _entity_population_read_use_site,
    _entity_population_section_guard,
    _entity_presentation_fx_direct_calls,
    _entity_presentation_fx_function_chunk_target_record,
    _entity_presentation_fx_macro_annotations,
    _entity_presentation_fx_section_guard,
    _force_state_aliases,
    _force_state_direct_calls,
    _force_state_program_facts,
    _force_state_section_guard,
    _logical_source_lines,
    _map_block_macro_operand_fields,
    _map_block_mutation_section_guard,
    _map_camera_control_branch_target_record,
    _map_camera_control_cursor_read_use_site,
    _map_camera_control_cursor_write_use_site,
    _map_camera_control_macro_annotations,
    _map_camera_control_section_guard,
    _map_lifecycle_macro_annotations,
    _map_lifecycle_read_use_site,
    _map_lifecycle_section_guard,
    _map_script_ui_primary_macro_annotations,
    _map_script_ui_primary_portrait_helper_join,
    _map_script_ui_primary_section_guard,
    _modifier_source_labels,
    _program_corpus,
    _screen_presentation_branch_target_record,
    _screen_presentation_cursor_read_use_site,
    _screen_presentation_direct_calls,
    _screen_presentation_macro_annotations,
    _screen_presentation_section_guard,
    _statements,
    _story_state_corpus_order_facts,
    _story_state_facts,
    _story_state_section_guard,
    _substitute_alias_layout,
    build_map_script_engine_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path


def test_emission_rows_preserve_shorthand_width_and_stream_offset() -> None:
    rows = _emission_rows(
        """
        dc.w $22
        dc.w \\1
        defineShorthand.w ENTITY_TRANSITION_,\\2
        """
    )

    assert [row["streamOffset"] for row in rows] == [0, 2, 4]
    assert [row["widthBytes"] for row in rows] == [2, 2, 2]
    assert rows[2]["encoding"] == "shorthand:ENTITY_TRANSITION_"
    assert rows[2]["parameterOrdinals"] == [2]


def test_alias_layout_substitutes_constants_without_losing_physical_fields() -> None:
    layout = _emission_rows("dc.w \\1\ndc.w \\2")
    actual = _substitute_alias_layout(layout, ["\\1", "$FFFF"])

    assert [row["expression"] for row in actual] == ["\\1", "$FFFF"]
    assert [row["parameterOrdinals"] for row in actual] == [[1], []]


def test_cursor_flow_distinguishes_jump_and_inline_program_shapes() -> None:
    assert _cursor_flow("csc0B_jump", ["movea.l (a6),a6"]) == "absolute-jump"
    assert (
        _cursor_flow(
            "csc0C_jumpIfFlagSet",
            ["movea.l (a6),a6", "addq.w #4,a6"],
        )
        == "conditional-absolute-jump"
    )
    assert (
        _cursor_flow(
            "csc14_setEntityActscriptManual",
            [
                "move.l a6,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)",
                "cmpi.w #$8080,(a6)+",
            ],
        )
        == "inline-action-program"
    )


def test_logical_source_lines_join_ampersand_continuations() -> None:
    assert _logical_source_lines("  setBlocks 1,2,&\n    3,4,5,6\n") == [
        (1, "  setBlocks 1,2, 3,4,5,6")
    ]


def test_force_state_call_parser_ignores_near_misses_and_accepts_size_suffixes() -> None:
    calls = _force_state_direct_calls(
        [
            "jsr.w j_JoinForce",
            "bsr.l GetCurrentHp",
            "move.w j_JoinForce,d0",
            "j_JoinForce:",
            "jsr (a0)",
            "; jsr.w j_JoinForce",
            "jsr j_JoinForce ; comment is already stripped by handler parsing",
        ]
    )

    assert calls == [
        {"opcode": "jsr", "instructionTarget": "j_JoinForce"},
        {"opcode": "bsr", "instructionTarget": "GetCurrentHp"},
        {"opcode": "jsr", "instructionTarget": "j_JoinForce"},
    ]


def test_map_camera_control_parsers_preserve_operand_comments_and_cursor_widths(
    tmp_path: Path,
) -> None:
    (tmp_path / "sf2cutscenemacros.asm").write_text(
        """
setCameraEntity: macro
    dc.w $24
    dc.w \\1 ; target entity
    endm
setCamDest: macro
    dc.w $32
    dc.w \\1 ; X (left border)
    dc.w \\2 ; Y (top border)
    endm
cameraSpeed: macro
    dc.w $45
    dc.w \\1 ; ($8-, $10-, $20-, $28-, $30-, $38-, $40-)
    endm
""",
        encoding="utf-8",
    )

    assert _map_camera_control_macro_annotations(tmp_path) == {
        "setCameraEntity": [
            {
                "parameterOrdinal": 1,
                "sourceComment": "target entity",
                "streamOffset": 2,
                "widthBytes": 2,
            }
        ],
        "setCamDest": [
            {
                "parameterOrdinal": 1,
                "sourceComment": "X (left border)",
                "streamOffset": 2,
                "widthBytes": 2,
            },
            {
                "parameterOrdinal": 2,
                "sourceComment": "Y (top border)",
                "streamOffset": 4,
                "widthBytes": 2,
            },
        ],
        "cameraSpeed": [
            {
                "parameterOrdinal": 1,
                "sourceComment": "($8-, $10-, $20-, $28-, $30-, $38-, $40-)",
                "streamOffset": 2,
                "widthBytes": 2,
            }
        ],
    }
    assert _map_camera_control_cursor_read_use_site("move.b (a6)+,d7") == {
        "sourceRegister": "a6",
        "destinationRegister": "d7",
        "transferredByteCount": 1,
        "cursorAdvanceByteCount": 1,
        "instruction": "move.b (a6)+,d7",
    }
    assert _map_camera_control_cursor_read_use_site("move.l (a6)+,d0")[
        "transferredByteCount"
    ] == 4
    assert _map_camera_control_cursor_write_use_site(
        "move.w (a6)+,((VIEW_SCROLLING_SPEED-$1000000)).w"
    ) == {
        "sourceRegister": "a6",
        "destinationOperand": "((VIEW_SCROLLING_SPEED-$1000000)).w",
        "transferredByteCount": 2,
        "cursorAdvanceByteCount": 2,
        "instruction": "move.w (a6)+,((VIEW_SCROLLING_SPEED-$1000000)).w",
    }

    (tmp_path / "sf2cutscenemacros.asm").write_text(
        (tmp_path / "sf2cutscenemacros.asm")
        .read_text(encoding="utf-8")
        .replace("dc.w \\2 ; Y (top border)", "dc.w \\1 ; Y (top border)"),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="operand ordinal"):
        _map_camera_control_macro_annotations(tmp_path)
    for near_miss in (
        "move.w (a6),d0",
        "move.w (a5)+,d0",
        "move.w (a6)+,a0",
        "move.w (a6)+,VIEW_SCROLLING_SPEED",
    ):
        with pytest.raises(ValueError, match="cursor-read"):
            _map_camera_control_cursor_read_use_site(near_miss)


def test_map_entity_action_bridge_payload_invocation_rejects_non_instructions() -> None:
    payload_macros = {"ac_setSpeed", "ac_end", "moveDown", "endActions"}
    assert _entity_action_bridge_payload_invocation(
        " ac_setSpeed 48,48 ; source payload", payload_macros
    ) == ("ac_setSpeed", ["48", "48"])
    assert _entity_action_bridge_payload_invocation("endActions", payload_macros) == (
        "endActions",
        [],
    )
    for near_miss in (
        "label_ac_setSpeed:",
        "; ac_setSpeed 48,48",
        "dc.w ac_setSpeed",
        "not_ac_setSpeed 48,48",
        "ac_setSpeed 48,,48",
    ):
        if near_miss == "ac_setSpeed 48,,48":
            with pytest.raises(ValueError, match="operand is empty"):
                _entity_action_bridge_payload_invocation(near_miss, payload_macros)
        else:
            assert _entity_action_bridge_payload_invocation(near_miss, payload_macros) is None


def test_map_entity_placement_cursor_read_parser_preserves_advance_and_sizes() -> None:
    assert _entity_placement_cursor_read_use_site("move.b (a6),d0") == {
        "sourceRegister": "a6",
        "destinationOperand": "d0",
        "transferredByteCount": 1,
        "cursorAdvanceByteCount": 0,
        "instruction": "move.b (a6),d0",
    }
    assert _entity_placement_cursor_read_use_site("move.w (a6)+,d2") == {
        "sourceRegister": "a6",
        "destinationOperand": "d2",
        "transferredByteCount": 2,
        "cursorAdvanceByteCount": 2,
        "instruction": "move.w (a6)+,d2",
    }
    assert _entity_placement_cursor_read_use_site("move.l (a6)+,d7")[
        "transferredByteCount"
    ] == 4
    for near_miss in (
        "move.w (a6),a0",
        "move.w (a5)+,d0",
        "move.w (a6)+,d0 ; comment",
        "label: move.w (a6)+,d0",
        "; move.w (a6)+,d0",
    ):
        with pytest.raises(ValueError, match="entity-placement cursor-read"):
            _entity_placement_cursor_read_use_site(near_miss)


def test_map_camera_control_section_guard_rejects_operand_branch_and_call_order() -> None:
    equates = {"ENTITY_ENEMY_INDEX_DIFFERENCE": 128, "BYTE_MASK": 255}
    entity_statements = [
        "lea ((ENTITY_INDEX_LIST-$1000000)).w,a5",
        "move.w (a6)+,d0",
        "bmi.w loc_46C52",
        "tst.b d0",
        "bpl.s @Ally",
        "subi.b #ENTITY_ENEMY_INDEX_DIFFERENCE,d0",
        "andi.w #BYTE_MASK,d0",
        "move.b (a5,d0.w),d0",
        "move.b d0,((VIEW_TARGET_ENTITY-$1000000)).w",
        "nop",
        "rts",
    ]
    assert _map_camera_control_section_guard(
        "setCameraEntity", entity_statements, equates
    )["branchRecords"] == [
        {
            "testInstruction": "move.w (a6)+,d0",
            "branchInstruction": "bmi.w loc_46C52",
            "branchTargetLabel": "loc_46C52",
        },
        {
            "testInstruction": "tst.b d0",
            "branchInstruction": "bpl.s @Ally",
            "branchTargetLabel": "@Ally",
        },
    ]
    destination_statements = [
        "move.b #-1,((VIEW_TARGET_ENTITY-$1000000)).w",
        "nop",
        "move.w (a6)+,d2",
        "move.w (a6)+,d3",
        "jsr j_SetCameraDestination",
        "jsr (WaitForViewScrollEnd).w",
        "rts",
    ]
    assert _map_camera_control_section_guard(
        "setCamDest", destination_statements, equates
    )["directCallOrder"] == [
        "jsr j_SetCameraDestination",
        "jsr (WaitForViewScrollEnd).w",
    ]
    assert _map_camera_control_section_guard(
        "cameraSpeed",
        [
            "move.w (a6)+,((VIEW_SCROLLING_SPEED-$1000000)).w",
            "nop",
            "rts",
        ],
        equates,
    )["scriptCursorWriteUseSites"][0]["cursorAdvanceByteCount"] == 2

    with pytest.raises(ValueError, match="csc24_setCameraTargetEntity statement is missing"):
        _map_camera_control_section_guard(
            "setCameraEntity",
            [
                statement.replace(
                    "#ENTITY_ENEMY_INDEX_DIFFERENCE", "#ENTITY_ENEMY_INDEX_DIFFERENCE+1"
                )
                for statement in entity_statements
            ],
            equates,
        )
    with pytest.raises(ValueError, match="csc24_setCameraTargetEntity statement is missing"):
        _map_camera_control_section_guard(
            "setCameraEntity",
            [statement.replace("bpl.s @Ally", "bmi.s @Ally") for statement in entity_statements],
            equates,
        )
    swapped_calls = list(destination_statements)
    swapped_calls[4], swapped_calls[5] = swapped_calls[5], swapped_calls[4]
    with pytest.raises(ValueError, match="csc32_setCameraDestInTiles statement is missing"):
        _map_camera_control_section_guard("setCamDest", swapped_calls, equates)


def test_map_camera_control_branch_targets_require_local_labels_and_target_instructions() -> None:
    ordered = [
        "lea ((ENTITY_INDEX_LIST-$1000000)).w,a5",
        "move.w (a6)+,d0",
        "bmi.w loc_46C52",
        "tst.b d0",
        "bpl.s @Ally",
        "subi.b #ENTITY_ENEMY_INDEX_DIFFERENCE,d0",
        "andi.w #BYTE_MASK,d0",
        "move.b (a5,d0.w),d0",
        "move.b d0,((VIEW_TARGET_ENTITY-$1000000)).w",
        "nop",
        "rts",
    ]
    source = """
    lea ((ENTITY_INDEX_LIST-$1000000)).w,a5
    move.w (a6)+,d0
    bmi.w loc_46C52
    tst.b d0
    bpl.s @Ally
    subi.b #ENTITY_ENEMY_INDEX_DIFFERENCE,d0
@Ally:
    andi.w #BYTE_MASK,d0
    move.b (a5,d0.w),d0
loc_46C52:
    move.b d0,((VIEW_TARGET_ENTITY-$1000000)).w
    nop
    rts
"""
    assert _map_camera_control_branch_target_record(
        source, "bmi.w loc_46C52", ordered
    ) == {
        "targetLabel": "loc_46C52",
        "targetInstruction": "move.b d0,((VIEW_TARGET_ENTITY-$1000000)).w",
        "targetStatementIndex": 8,
    }
    assert _map_camera_control_branch_target_record(source, "bpl.s @Ally", ordered) == {
        "targetLabel": "@Ally",
        "targetInstruction": "andi.w #BYTE_MASK,d0",
        "targetStatementIndex": 6,
    }
    with pytest.raises(ValueError, match="branch target label is missing"):
        _map_camera_control_branch_target_record(
            source.replace("loc_46C52:", "loc_46C53:"), "bmi.w loc_46C52", ordered
        )
    with pytest.raises(ValueError, match="branch target instruction drift"):
        _map_camera_control_branch_target_record(
            source.replace("andi.w #BYTE_MASK,d0", "andi.w #BYTE_MASK,d1"),
            "bpl.s @Ally",
            ordered,
        )


def test_map_camera_control_branch_label_owner_fails_during_contract_construction(
    monkeypatch,
) -> None:
    original_source = map_script_engine._map_camera_control_named_section_source

    def changed_branch_owner(disasm, source_path, name):
        source = original_source(disasm, source_path, name)
        if name == "csc24_setCameraTargetEntity":
            return source.replace("loc_46C52:", "loc_46C53:")
        return source

    monkeypatch.setattr(
        map_script_engine, "_map_camera_control_named_section_source", changed_branch_owner
    )
    with pytest.raises(ValueError, match="branch target label is missing"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_force_state_program_totals_keep_zero_rows_and_exact_site_order() -> None:
    corpus = {
        "summary": {"programCount": 2},
        "programs": [
            {
                "id": "first",
                "commands": [
                    {"index": 0, "sourceLine": 10, "macro": "join", "arguments": ["1"]},
                    {"index": 1, "sourceLine": 11, "macro": "wait", "arguments": []},
                ],
            },
            {"id": "second", "commands": []},
        ],
    }

    sites, totals = _force_state_program_facts(corpus)

    assert sites == [
        {
            "programId": "first",
            "commands": [
                {"commandIndex": 0, "sourceLine": 10, "macro": "join", "arguments": ["1"]}
            ],
        }
    ]
    assert totals == [
        {
            "programId": "first",
            "commandCount": 1,
            "macroCounts": {
                "join": 1,
                "jumpIfDefeatedByLastAttack": 0,
                "jumpIfDead": 0,
                "allyDefeated": 0,
                "updateDefeatedAllies": 0,
                "reviveAlly": 0,
            },
        },
        {
            "programId": "second",
            "commandCount": 0,
            "macroCounts": {
                "join": 0,
                "jumpIfDefeatedByLastAttack": 0,
                "jumpIfDead": 0,
                "allyDefeated": 0,
                "updateDefeatedAllies": 0,
                "reviveAlly": 0,
            },
        },
    ]


def test_active_party_program_totals_keep_zero_rows_and_exact_site_order() -> None:
    corpus = {
        "summary": {"programCount": 2},
        "programs": [
            {
                "id": "first",
                "commands": [
                    {
                        "index": 0,
                        "sourceLine": 10,
                        "macro": "joinForceAI",
                        "arguments": ["1", "-1"],
                    },
                    {"index": 1, "sourceLine": 11, "macro": "wait", "arguments": []},
                    {
                        "index": 2,
                        "sourceLine": 12,
                        "macro": "joinForceAIs",
                        "arguments": ["1", "-1"],
                    },
                ],
            },
            {"id": "second", "commands": []},
        ],
    }

    sites, totals = _force_state_program_facts(
        corpus, macro_names=map_script_engine.ACTIVE_PARTY_MACRO_NAMES
    )

    assert sites == [
        {
            "programId": "first",
            "commands": [
                {
                    "commandIndex": 0,
                    "sourceLine": 10,
                    "macro": "joinForceAI",
                    "arguments": ["1", "-1"],
                }
            ],
        }
    ]
    assert totals == [
        {
            "programId": "first",
            "commandCount": 1,
            "macroCounts": {
                "joinBatParty": 0,
                "joinForceAI": 1,
                "resetForceBattleStats": 0,
                "addNewFollower": 0,
            },
        },
        {
            "programId": "second",
            "commandCount": 0,
            "macroCounts": {
                "joinBatParty": 0,
                "joinForceAI": 0,
                "resetForceBattleStats": 0,
                "addNewFollower": 0,
            },
        },
    ]


def test_story_state_program_totals_keep_aliases_and_zero_primary_carrier() -> None:
    corpus = {
        "summary": {"programCount": 2},
        "programs": [
            {
                "id": "first",
                "commands": [
                    {
                        "index": 0,
                        "sourceLine": 10,
                        "macro": "jumpIfFlagSet",
                        "arguments": ["6", "target"],
                    },
                    {
                        "index": 1,
                        "sourceLine": 11,
                        "macro": "setF",
                        "arguments": ["71"],
                    },
                    {
                        "index": 2,
                        "sourceLine": 12,
                        "macro": "clearF",
                        "arguments": ["76"],
                    },
                    {"index": 3, "sourceLine": 13, "macro": "menu", "arguments": []},
                ],
            },
            {"id": "second", "commands": []},
        ],
    }

    sites, totals = _force_state_program_facts(
        corpus, macro_names=map_script_engine.STORY_STATE_MACRO_NAMES
    )

    assert sites == [
        {
            "programId": "first",
            "commands": [
                {
                    "commandIndex": 0,
                    "sourceLine": 10,
                    "macro": "jumpIfFlagSet",
                    "arguments": ["6", "target"],
                },
                {
                    "commandIndex": 1,
                    "sourceLine": 11,
                    "macro": "setF",
                    "arguments": ["71"],
                },
                {
                    "commandIndex": 2,
                    "sourceLine": 12,
                    "macro": "clearF",
                    "arguments": ["76"],
                },
            ],
        }
    ]
    assert totals == [
        {
            "programId": "first",
            "commandCount": 3,
            "macroCounts": {
                "jumpIfFlagSet": 1,
                "jumpIfFlagClear": 0,
                "csc10": 0,
                "setF": 1,
                "clearF": 1,
                "yesNo": 0,
                "setStoryFlag": 0,
            },
        },
        {
            "programId": "second",
            "commandCount": 0,
            "macroCounts": {
                "jumpIfFlagSet": 0,
                "jumpIfFlagClear": 0,
                "csc10": 0,
                "setF": 0,
                "clearF": 0,
                "yesNo": 0,
                "setStoryFlag": 0,
            },
        },
    ]


def test_map_block_mutation_program_totals_keep_both_source_forms() -> None:
    corpus = {
        "summary": {"programCount": 2},
        "programs": [
            {
                "id": "first",
                "commands": [
                    {
                        "index": 0,
                        "sourceLine": 10,
                        "macro": "setBlocks",
                        "arguments": ["1", "2", "3", "4", "5", "6"],
                    },
                    {
                        "index": 1,
                        "sourceLine": 11,
                        "macro": "setBlocksVar",
                        "arguments": ["7", "8", "9", "10", "11", "12"],
                    },
                    {"index": 2, "sourceLine": 12, "macro": "setQuake", "arguments": []},
                ],
            },
            {"id": "second", "commands": []},
        ],
    }

    sites, totals = _force_state_program_facts(
        corpus, macro_names=map_script_engine.MAP_BLOCK_MUTATION_MACRO_NAMES
    )

    assert sites == [
        {
            "programId": "first",
            "commands": [
                {
                    "commandIndex": 0,
                    "sourceLine": 10,
                    "macro": "setBlocks",
                    "arguments": ["1", "2", "3", "4", "5", "6"],
                },
                {
                    "commandIndex": 1,
                    "sourceLine": 11,
                    "macro": "setBlocksVar",
                    "arguments": ["7", "8", "9", "10", "11", "12"],
                },
            ],
        }
    ]
    assert totals == [
        {
            "programId": "first",
            "commandCount": 2,
            "macroCounts": {"setBlocks": 1, "setBlocksVar": 1},
        },
        {
            "programId": "second",
            "commandCount": 0,
            "macroCounts": {"setBlocks": 0, "setBlocksVar": 0},
        },
    ]


def test_map_block_mutation_operand_field_parser_rejects_ordinal_drift(tmp_path) -> None:
    macro_path = tmp_path / "sf2cutscenemacros.asm"
    macro_path.write_text(
        """
setBlocks: macro
    dc.w $34
    dc.b \\1 ; source x
    dc.b \\2 ; source y
    endm
setBlocksVar: macro
    dc.w $35
    dc.b \\1 ; source x
    dc.b \\2 ; source y
    endm
""",
        encoding="utf-8",
    )
    assert _map_block_macro_operand_fields(tmp_path) == {
        "setBlocks": [
            {"parameterOrdinal": 1, "sourceLabel": "source x", "streamOffset": 2, "widthBytes": 1},
            {"parameterOrdinal": 2, "sourceLabel": "source y", "streamOffset": 3, "widthBytes": 1},
        ],
        "setBlocksVar": [
            {"parameterOrdinal": 1, "sourceLabel": "source x", "streamOffset": 2, "widthBytes": 1},
            {"parameterOrdinal": 2, "sourceLabel": "source y", "streamOffset": 3, "widthBytes": 1},
        ],
    }
    original_text = macro_path.read_text(encoding="utf-8")
    uncommented_text = original_text.replace("; source y", "", 1)
    macro_path.write_text(uncommented_text, encoding="utf-8")
    with pytest.raises(ValueError, match="comment coverage drift"):
        _map_block_macro_operand_fields(tmp_path)
    drifted_text = original_text.replace(
        "dc.b \\2 ; source y", "dc.b \\3 ; source y", 1
    )
    macro_path.write_text(
        drifted_text,
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="operand ordinal drift"):
        _map_block_macro_operand_fields(tmp_path)


def test_map_block_mutation_section_guard_rejects_bit_order_and_extra_statement() -> None:
    statements = [
        "move.w (a6)+,d0",
        "move.w (a6)+,d1",
        "move.w (a6)+,d2",
        "jsr (CopyMapBlocks).w",
        "bset #0,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l",
        "bset #1,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l",
        "rts",
    ]
    actual = _map_block_mutation_section_guard("csc34_setBlocks", statements)
    assert actual["postCallBitSetUseSites"] == [
        {
            "bitIndex": 0,
            "sourceTarget": "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD",
            "instruction": "bset #0,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l",
        },
        {
            "bitIndex": 1,
            "sourceTarget": "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD",
            "instruction": "bset #1,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l",
        },
    ]
    statements[4], statements[5] = statements[5], statements[4]
    with pytest.raises(ValueError, match="bit-set order drift"):
        _map_block_mutation_section_guard("csc34_setBlocks", statements)
    statements[4], statements[5] = statements[5], statements[4]
    statements.insert(-1, "nop")
    with pytest.raises(ValueError, match="statement coverage drift"):
        _map_block_mutation_section_guard("csc34_setBlocks", statements)


def test_entity_population_macro_annotations_preserve_blank_source_comments(tmp_path) -> None:
    macro_path = tmp_path / "sf2cutscenemacros.asm"
    macro_path.write_text(
        """
newEntity: macro
    dc.w $2B
    dc.w \\1 ; entity number
    dc.b \\2 ; X
    endm
loadMapEntities: macro
    dc.w $42
    dc.l \\1 ; address of entity table
    endm
reloadEntities: macro
    dc.w $44
    dc.l \\1 ; address of entity table
    endm
loadEntitiesFromMapSetup: macro
    dc.w $49
    dc.w \\1 ;
    dc.w \\2 ;
    dc.w \\3 ;
    endm
""",
        encoding="utf-8",
    )
    actual = _entity_population_macro_annotations(tmp_path)
    assert actual["newEntity"] == [
        {
            "parameterOrdinal": 1,
            "sourceComment": "entity number",
            "streamOffset": 2,
            "widthBytes": 2,
        },
        {
            "parameterOrdinal": 2,
            "sourceComment": "X",
            "streamOffset": 4,
            "widthBytes": 1,
        },
    ]
    assert [row["sourceComment"] for row in actual["loadEntitiesFromMapSetup"]] == [
        "",
        "",
        "",
    ]
    source = macro_path.read_text(encoding="utf-8")
    drifted = source.replace("dc.w \\2 ;\n", "dc.w \\4 ;\n", 1)
    macro_path.write_text(drifted, encoding="utf-8")
    with pytest.raises(ValueError, match="operand ordinal"):
        _entity_population_macro_annotations(tmp_path)

    missing_comment = source.replace(
        "dc.w \\1 ; entity number", "dc.w \\1", 1
    )
    macro_path.write_text(missing_comment, encoding="utf-8")
    with pytest.raises(ValueError, match="comment is missing"):
        _entity_population_macro_annotations(tmp_path)


def test_entity_clone_macro_annotations_preserve_source_comments(tmp_path) -> None:
    macro_path = tmp_path / "sf2cutscenemacros.asm"
    macro_path.write_text(
        """
cloneEntity: macro
    dc.w $25
    dc.w \\1 ; copied entity
    dc.w \\2 ; entity clone
    endm
""",
        encoding="utf-8",
    )
    actual = _entity_clone_macro_annotations(tmp_path)
    assert actual == {
        "cloneEntity": [
            {
                "parameterOrdinal": 1,
                "sourceComment": "copied entity",
                "streamOffset": 2,
                "widthBytes": 2,
            },
            {
                "parameterOrdinal": 2,
                "sourceComment": "entity clone",
                "streamOffset": 4,
                "widthBytes": 2,
            },
        ]
    }
    source = macro_path.read_text(encoding="utf-8")
    macro_path.write_text(
        source.replace("dc.w \\2 ; entity clone", "dc.w \\3 ; entity clone"),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="operand ordinal"):
        _entity_clone_macro_annotations(tmp_path)
    macro_path.write_text(
        source.replace("dc.w \\1 ; copied entity", "dc.w \\1"), encoding="utf-8"
    )
    with pytest.raises(ValueError, match="comment is missing"):
        _entity_clone_macro_annotations(tmp_path)


def test_entity_population_pointer_read_parser_accepts_sizes_and_rejects_near_misses() -> None:
    assert [_entity_population_read_use_site(instruction) for instruction in (
        "move.b (a6)+,d1",
        "move.w (a0)+,d2",
        "move.l (a6)+,d3",
        "movea.l (a6)+,a0",
    )] == [
        {
            "sourceRegister": "a6",
            "destinationRegister": "d1",
            "transferredByteCount": 1,
            "instruction": "move.b (a6)+,d1",
        },
        {
            "sourceRegister": "a0",
            "destinationRegister": "d2",
            "transferredByteCount": 2,
            "instruction": "move.w (a0)+,d2",
        },
        {
            "sourceRegister": "a6",
            "destinationRegister": "d3",
            "transferredByteCount": 4,
            "instruction": "move.l (a6)+,d3",
        },
        {
            "sourceRegister": "a6",
            "destinationRegister": "a0",
            "transferredByteCount": 4,
            "instruction": "movea.l (a6)+,a0",
        },
    ]
    for near_miss in (
        "moveq #0,d1",
        "read: move.w (a6)+,d1",
        "; move.w (a6)+,d1",
        "move.w (a6)+,d1 ; source pointer read",
        "move.w (a6),d1",
        "move.w (a1)+,d1",
        "move.w (a6)+,a0",
    ):
        with pytest.raises(ValueError, match="pointer-read use shape"):
            _entity_population_read_use_site(near_miss)


def test_entity_population_section_guard_rejects_vint_order_and_extra_statement() -> None:
    statements = [
        "trap #VINT_FUNCTIONS",
        "dc.w VINTS_DEACTIVATE",
        "dc.l 0",
        "jsr (DisableDisplayAndInterrupts).w",
        "movea.l (a6)+,a0",
        "move.w (a0)+,d1",
        "move.w (a0)+,d2",
        "move.w (a0)+,d3",
        "jsr InitializeMapEntities",
        "jsr (LoadEntityMapsprites).w",
        "jsr (EnableDisplayAndInterrupts).w",
        "trap #VINT_FUNCTIONS",
        "dc.w VINTS_ACTIVATE",
        "dc.l 0",
        "rts",
    ]
    actual = _entity_population_section_guard(
        "loadMapEntities",
        statements,
        {"VINTS_DEACTIVATE": 3, "VINTS_ACTIVATE": 4},
        {"eas_Init": 0},
    )
    assert actual["vintControlRecords"][1]["operationValue"] == 4
    statements[12] = "dc.w VINTS_DEACTIVATE"
    with pytest.raises(ValueError, match="VINTS_ACTIVATE"):
        _entity_population_section_guard(
            "loadMapEntities",
            statements,
            {"VINTS_DEACTIVATE": 3, "VINTS_ACTIVATE": 4},
            {"eas_Init": 0},
        )
    statements[12] = "dc.w VINTS_ACTIVATE"
    statements.insert(-1, "nop")
    with pytest.raises(ValueError, match="statement coverage drift"):
        _entity_population_section_guard(
            "loadMapEntities",
            statements,
            {"VINTS_DEACTIVATE": 3, "VINTS_ACTIVATE": 4},
            {"eas_Init": 0},
        )


def test_map_lifecycle_macro_annotations_preserve_source_comments(tmp_path) -> None:
    macro_path = tmp_path / "sf2cutscenemacros.asm"
    macro_path.write_text(
        """
resetMap: macro
    dc.w $36
    endm
loadMapFadeIn: macro
    dc.w $37
    dc.w \\1 ; map
    dc.w \\2 ; camera X
    dc.w \\3 ; camera Y
    endm
reloadMap: macro
    dc.w $46
    dc.w \\1 ; camera X
    dc.w \\2 ; camera Y
    endm
mapLoad: macro
    dc.w $48
    dc.w \\1 ; map
    dc.w \\2 ; camera X
    dc.w \\3 ; camera Y
    endm
""",
        encoding="utf-8",
    )
    actual = _map_lifecycle_macro_annotations(tmp_path)
    assert actual["resetMap"] == []
    assert actual["loadMapFadeIn"] == [
        {"parameterOrdinal": 1, "sourceComment": "map", "streamOffset": 2, "widthBytes": 2},
        {
            "parameterOrdinal": 2,
            "sourceComment": "camera X",
            "streamOffset": 4,
            "widthBytes": 2,
        },
        {
            "parameterOrdinal": 3,
            "sourceComment": "camera Y",
            "streamOffset": 6,
            "widthBytes": 2,
        },
    ]
    source = macro_path.read_text(encoding="utf-8")
    macro_path.write_text(
        source.replace("dc.w \\2 ; camera X", "dc.w \\3 ; camera X", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="operand ordinal"):
        _map_lifecycle_macro_annotations(tmp_path)
    macro_path.write_text(source.replace("dc.w \\1 ; map", "dc.w \\1", 1), encoding="utf-8")
    with pytest.raises(ValueError, match="comment is missing"):
        _map_lifecycle_macro_annotations(tmp_path)


def test_map_lifecycle_cursor_read_parser_accepts_sizes_and_rejects_near_misses() -> None:
    assert [_map_lifecycle_read_use_site(instruction) for instruction in (
        "move.b (a6)+,d1",
        "move.w (a0),d2",
        "move.l (a6)+,d3",
    )] == [
        {
            "sourceRegister": "a6",
            "destinationRegister": "d1",
            "transferredByteCount": 1,
            "cursorAdvanceByteCount": 1,
            "instruction": "move.b (a6)+,d1",
        },
        {
            "sourceRegister": "a0",
            "destinationRegister": "d2",
            "transferredByteCount": 2,
            "cursorAdvanceByteCount": 0,
            "instruction": "move.w (a0),d2",
        },
        {
            "sourceRegister": "a6",
            "destinationRegister": "d3",
            "transferredByteCount": 4,
            "cursorAdvanceByteCount": 4,
            "instruction": "move.l (a6)+,d3",
        },
    ]
    for near_miss in (
        "moveq #0,d1",
        "read: move.w (a6)+,d1",
        "; move.w (a6)+,d1",
        "move.w (a6)+,d1 ; cursor read",
        "move.w (a1)+,d1",
        "move.w (a6)+,a0",
    ):
        with pytest.raises(ValueError, match="map-lifecycle read use shape"):
            _map_lifecycle_read_use_site(near_miss)


def test_map_lifecycle_section_guard_rejects_branch_polarity_and_call_order() -> None:
    statements = [
        "move.b #-1,((VIEW_TARGET_ENTITY-$1000000)).w",
        "nop",
        "move.w (a6),d1",
        "jsr (LoadMapTilesets).w",
        "jsr (WaitForVInt).w",
        "tst.b ((FADING_SETTING-$1000000)).w",
        "bne.s loc_465C4",
        "trap #VINT_FUNCTIONS",
        "dc.w VINTS_DEACTIVATE",
        "dc.l 0",
        "clr.l d0",
        "move.w (a6)+,d1",
        "move.w (a6)+,d0",
        "lsl.w #BYTE_SHIFT_COUNT,d0",
        "move.w (a6)+,d2",
        "andi.w #BYTE_MASK,d2",
        "or.w d2,d0",
        "mulu.w #3,d0",
        "move.l a6,-(sp)",
        "jsr (LoadMap).w",
        "movea.l (sp)+,a6",
        "jsr (EnableDisplayAndInterrupts).w",
        "trap #VINT_FUNCTIONS",
        "dc.w VINTS_ACTIVATE",
        "dc.l 0",
        "jsr (WaitForVInt).w",
        "rts",
    ]
    annotations = [
        {"parameterOrdinal": 1, "sourceComment": "map", "streamOffset": 2, "widthBytes": 2},
        {"parameterOrdinal": 2, "sourceComment": "camera X", "streamOffset": 4, "widthBytes": 2},
        {"parameterOrdinal": 3, "sourceComment": "camera Y", "streamOffset": 6, "widthBytes": 2},
    ]
    equates = {"BYTE_SHIFT_COUNT": 8, "BYTE_MASK": 255, "VINTS_DEACTIVATE": 3, "VINTS_ACTIVATE": 4}
    actual = _map_lifecycle_section_guard("mapLoad", statements, annotations, equates)
    assert actual["branchRecords"] == [
        {
            "testInstruction": "tst.b ((FADING_SETTING-$1000000)).w",
            "branchInstruction": "bne.s loc_465C4",
            "fallthroughInstruction": "trap #VINT_FUNCTIONS",
        }
    ]
    statements[6] = "beq.s loc_465C4"
    with pytest.raises(ValueError, match="csc48_loadMap statement is missing"):
        _map_lifecycle_section_guard("mapLoad", statements, annotations, equates)
    statements[6] = "bne.s loc_465C4"
    statements[19], statements[21] = statements[21], statements[19]
    with pytest.raises(ValueError, match="csc48_loadMap statement is missing"):
        _map_lifecycle_section_guard("mapLoad", statements, annotations, equates)
def test_story_state_corpus_order_facts_bind_order_and_canonical_content() -> None:
    source_sites = [
        {
            "programId": "first",
            "commands": [
                {
                    "commandIndex": 3,
                    "macro": "setF",
                    "storyStateReference": {"field": "directWrites", "entryIndex": 2},
                }
            ],
        }
    ]
    program_totals = [
        {"programId": "first", "commandCount": 1, "macroCounts": {}},
        {"programId": "second", "commandCount": 0, "macroCounts": {}},
    ]

    actual = _story_state_corpus_order_facts(source_sites, program_totals)

    assert actual["sourceSiteOrderKeys"] == ["first:3:setF:directWrites:2"]
    assert actual["programTotalOrderKeys"] == ["first", "second"]
    assert len(actual["sourceSitesSha256"]) == 64
    assert len(actual["programTotalsSha256"]) == 64
    changed = deepcopy(source_sites)
    changed[0]["commands"][0]["commandIndex"] = 4
    assert _story_state_corpus_order_facts(changed, program_totals)[
        "sourceSitesSha256"
    ] != actual["sourceSitesSha256"]
    reordered = list(reversed(program_totals))
    assert _story_state_corpus_order_facts(source_sites, reordered)[
        "programTotalOrderKeys"
    ] == ["second", "first"]


def test_force_state_alias_parser_requires_named_jump_and_accepts_size_suffix(
    tmp_path: Path,
) -> None:
    interface = tmp_path / "code/common/tech/jumpinterfaces"
    interface.mkdir(parents=True)
    alias_path = interface / "s02_jumpinterface.asm"
    alias_path.write_text(
        "; j_Target in a comment is not an alias definition\n"
        "j_Target:\n"
        "    jmp.w Target(pc) ; legal instruction-size suffix\n",
        encoding="utf-8",
    )
    addresses = {"j_Target": 0, "Target": 4}
    rom = b"\x4e\xfa\x00\x02" + b"\x00" * 4

    assert _force_state_aliases(tmp_path, {"j_Target"}, addresses, rom) == {
        "j_Target": {
            "effectiveTarget": "Target",
            "sourcePath": "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
        }
    }

    alias_path.write_text("j_Target:\n    jsr Target(pc)\n", encoding="utf-8")
    with pytest.raises(ValueError, match="alias instruction drift"):
        _force_state_aliases(tmp_path, {"j_Target"}, addresses, rom)


def test_program_corpus_owns_anonymous_and_jump_terminated_programs(tmp_path) -> None:
    (tmp_path / "code").mkdir()
    (tmp_path / "data").mkdir()
    (tmp_path / "sf2enums.asm").write_text(
        "FLAG_INDEX_YES_NO_PROMPT: equ 89\n"
        "BATTLE_UNLOCKED_FLAGS_START: equ 400\n",
        encoding="utf-8",
    )
    (tmp_path / "code" / "one.asm").write_text(
        "entry:\n  csc_end\n\n  csc_end\n", encoding="utf-8"
    )
    (tmp_path / "data" / "two.asm").write_text(
        "tail:\n  jump entry\n", encoding="utf-8"
    )
    contracts = {
        "csc_end": {"kind": "terminator", "opcode": None, "encodedBytes": 2},
        "jump": {"kind": "command", "opcode": 11, "encodedBytes": 6},
    }

    actual = _program_corpus(
        tmp_path,
        ["code/one.asm", "data/two.asm"],
        contracts,
        {"entry": 0x100, "tail": 0x200},
    )

    assert actual["summary"]["programCount"] == 3
    assert actual["summary"]["anonymousProgramCount"] == 1
    assert actual["summary"]["absoluteJumpTerminatedProgramCount"] == 1
    assert actual["transferCounts"] == {"absolute-jump:cross-program": 1}
    assert actual["referenceSummary"]["referencedProgramCount"] == 1
    assert actual["referenceSummary"]["unreferencedProgramCount"] == 2
    assert actual["referenceSummary"]["referencedLabelCount"] == 1
    assert actual["referenceSummary"]["unreferencedLabelCount"] == 1


def test_story_state_facts_resolve_prompt_and_battle_flag_domains(tmp_path) -> None:
    (tmp_path / "sf2enums.asm").write_text(
        "FLAG_INDEX_YES_NO_PROMPT: equ 89\n"
        "BATTLE_UNLOCKED_FLAGS_START: equ 400\n",
        encoding="utf-8",
    )
    programs = [
        {
            "id": "scene",
            "commands": [
                {
                    "index": 0,
                    "macro": "jumpIfFlagSet",
                    "arguments": ["89", "target"],
                    "targetSymbol": "target",
                },
                {"index": 1, "macro": "yesNo", "arguments": []},
                {"index": 2, "macro": "setF", "arguments": ["70"]},
                {"index": 3, "macro": "clearF", "arguments": ["71"]},
                {"index": 4, "macro": "setStoryFlag", "arguments": ["4"]},
            ],
        }
    ]

    actual = _story_state_facts(tmp_path, programs)

    assert actual["summary"]["conditionalReadCount"] == 1
    assert actual["summary"]["uniqueWriteFlagCount"] == 4
    assert actual["constants"] == {
        "yesNoPromptFlag": 89,
        "battleUnlockedFlagsStart": 400,
    }
    assert actual["readWriteOverlapFlags"] == [89]
    assert actual["battleUnlockFlags"] == [404]


def _named_handler(name: str, statements: list[str]) -> str:
    return "\n".join(
        [
            f"{name}:",
            *[f"    {statement}" for statement in statements],
            f"; End of function {name}",
            "",
        ]
    )


def _synthetic_dialogue_handler_inputs(tmp_path: Path):
    map_path = tmp_path / "code/common/scripting/map"
    map_path.mkdir(parents=True)
    second_path = map_path / "mapscriptengine_2.asm"
    first_path = map_path / "mapscriptengine_1.asm"
    bodies: dict[str, list[str]] = {}

    for macro in DIALOGUE_MACROS[:4]:
        is_single = macro.startswith("nextSingle")
        has_skip_guard = macro in {"nextSingleText", "nextText"}
        has_vars = macro.endswith("Var")
        statements = []
        if has_skip_guard:
            statements.extend(["tst.b ((SKIP_CUTSCENE_TEXT-$1000000)).w", "bne.s @skip"])
        statements.extend(
            [
                "cmpi.w #-1,(a6)",
                "beq.s @noPortrait",
                "bsr.w csc1D_showPortrait",
                "bsr.w GetEntityPortaitAndSpeechSfx",
            ]
        )
        if has_vars:
            statements.extend(
                [
                    "move.w (a6)+,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
                    "move.w (a6)+,((DIALOGUE_NAME_INDEX_2-$1000000)).w",
                ]
            )
        statements.extend(
            [
                "jsr (DisplayText).l",
                "addq.w #1,((CUTSCENE_DIALOG_INDEX-$1000000)).w",
            ]
        )
        if is_single:
            statements.extend(
                ["jsr j_ClosePortraitWindow", "clsTxt", "moveq #10,d0", "jsr (Sleep).w"]
            )
        statements.append("rts")
        bodies[DIALOGUE_HANDLER_BY_MACRO[macro]] = statements

    bodies[DIALOGUE_HANDLER_BY_MACRO["textCursor"]] = [
        "move.w (a6)+,((CUTSCENE_DIALOG_INDEX-$1000000)).w",
        "rts",
    ]
    bodies[DIALOGUE_HANDLER_BY_MACRO["hideText"]] = [
        "jsr j_ClosePortraitWindow",
        "clsTxt",
        "rts",
    ]
    second_path.write_text(
        "".join(_named_handler(name, statements) for name, statements in bodies.items()),
        encoding="utf-8",
    )
    portrait_statements = [
        "move.w (a6)+,d0",
        "moveq #0,d3",
        "btst #$F,d0",
        "beq.s @rightDone",
        "moveq #-1,d3",
        "moveq #0,d4",
        "btst #$E,d0",
        "beq.s @mirrorDone",
        "moveq #-1,d4",
        "bsr.w GetEntityPortaitAndSpeechSfx",
        "rts",
    ]
    first_path.write_text(
        _named_handler("csc1D_showPortrait", portrait_statements), encoding="utf-8"
    )

    widths = [4, 6, 4, 8, 4, 2]
    targets = ["csc_doNothing"] * 10
    handlers = []
    opcodes_by_macro = {
        "nextSingleText": 0,
        "nextSingleTextVar": 1,
        "nextText": 2,
        "nextTextVar": 3,
        "textCursor": 4,
        "hideText": 9,
    }
    for address, (macro, width) in enumerate(
        zip(DIALOGUE_MACROS, widths, strict=True), start=100
    ):
        name = DIALOGUE_HANDLER_BY_MACRO[macro]
        targets[opcodes_by_macro[macro]] = name
        handlers.append(
            {
                "name": name,
                "opcodes": [next(index for index, target in enumerate(targets) if target == name)],
                "encodedCommandBytes": width,
                "sourcePath": "code/common/scripting/map/mapscriptengine_2.asm",
                "statementCount": len(_statements("\n".join(bodies[name]))),
                "address": address,
            }
        )
    handlers.append(
        {
            "name": "csc1D_showPortrait",
            "opcodes": [29],
            "encodedCommandBytes": 4,
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "statementCount": len(_statements("\n".join(portrait_statements))),
            "address": 99,
        }
    )
    macros = {
        name: {
            "kind": "command",
            "aliasOf": None,
            "opcode": index,
            "encodedBytes": widths[position],
        }
        for position, (name, index) in enumerate(
            zip(DIALOGUE_MACROS, opcodes_by_macro.values(), strict=True)
        )
    }
    return macros, targets, handlers


def _modifier_entity_pairs() -> Counter[tuple[int, int]]:
    return Counter({(0, 1): 1, (128, 128): 1, (192, 128): 1, (255, 255): 1})


def _synthetic_entity_dialogue_consumer() -> dict[str, str]:
    return {
        "function": ENTITY_DIALOGUE_CONSUMER,
        "sourcePath": ENTITY_DIALOGUE_CONSUMER_PATH.as_posix(),
    }


def test_dialogue_handler_guards_reject_use_site_order_and_operand_mutations(tmp_path) -> None:
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(tmp_path)
    facts, portrait, callers = _dialogue_handler_facts(
        tmp_path,
        macros,
        targets,
        handlers,
        _modifier_entity_pairs(),
        _synthetic_entity_dialogue_consumer(),
    )

    assert [row["macro"] for row in facts] == list(DIALOGUE_MACROS)
    assert portrait["handlerTestedModifierByteMask"] == 192
    assert portrait["modifierBitTests"] == [
        {"bit": 15, "destination": "d3"},
        {"bit": 14, "destination": "d4"},
    ]
    targets_by_handler = {
        "csc00_displaySingleTextbox": (1, 1),
        "csc01_displaySingleTextboxWithVars": (1, 1),
        "csc02_displayTextbox": (1, 1),
        "csc03_displayTextboxWithVars": (1, 1),
        "csc04_setTextIndex": (0, 0),
        "csc09_hideDialogueAndPortraitWindows": (0, 0),
        PORTRAIT_HANDLER: (0, 1),
    }
    assert callers == {
        "callerHandlers": [
            {
                "handler": handler,
                "sourcePath": (
                    "code/common/scripting/map/mapscriptengine_1.asm"
                    if handler == PORTRAIT_HANDLER
                    else "code/common/scripting/map/mapscriptengine_2.asm"
                ),
                "instructionTargetSiteCounts": {
                    PORTRAIT_HANDLER: portrait_count,
                    ENTITY_DIALOGUE_CONSUMER: consumer_count,
                },
                "effectiveTargetSiteCounts": {
                    PORTRAIT_HANDLER: portrait_count,
                    ENTITY_DIALOGUE_CONSUMER: consumer_count,
                },
            }
            for handler, (portrait_count, consumer_count) in targets_by_handler.items()
        ],
        "targetResolutions": [
            {
                "instructionTarget": PORTRAIT_HANDLER,
                "effectiveTarget": PORTRAIT_HANDLER,
                "effectiveTargetSourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
                "effectiveTargetScope": "internal",
            },
            {
                "instructionTarget": ENTITY_DIALOGUE_CONSUMER,
                "effectiveTarget": ENTITY_DIALOGUE_CONSUMER,
                "effectiveTargetSourcePath": ENTITY_DIALOGUE_CONSUMER_PATH.as_posix(),
                "effectiveTargetScope": "external",
            },
        ],
        "instructionTargetTotals": {PORTRAIT_HANDLER: 4, ENTITY_DIALOGUE_CONSUMER: 5},
        "effectiveTargetTotals": {PORTRAIT_HANDLER: 4, ENTITY_DIALOGUE_CONSUMER: 5},
        "internalInstructionTargetTotals": {PORTRAIT_HANDLER: 4, ENTITY_DIALOGUE_CONSUMER: 0},
        "externalInstructionTargetTotals": {PORTRAIT_HANDLER: 0, ENTITY_DIALOGUE_CONSUMER: 5},
        "internalEffectiveTargetTotals": {PORTRAIT_HANDLER: 4, ENTITY_DIALOGUE_CONSUMER: 0},
        "externalEffectiveTargetTotals": {PORTRAIT_HANDLER: 0, ENTITY_DIALOGUE_CONSUMER: 5},
    }

    bad_macros = deepcopy(macros)
    bad_macros["nextText"]["opcode"] = 1
    with pytest.raises(ValueError, match="dispatcher target"):
        _dialogue_handler_facts(
            tmp_path,
            bad_macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )

    engine = tmp_path / "code/common/scripting/map/mapscriptengine_2.asm"
    engine.write_text(
        engine.read_text(encoding="utf-8").replace(
            "addq.w #1,((CUTSCENE_DIALOG_INDEX-$1000000)).w",
            "addq.w #2,((CUTSCENE_DIALOG_INDEX-$1000000)).w",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="addq"):
        _dialogue_handler_facts(
            tmp_path,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )


def test_dialogue_handler_guards_reject_call_order_mutation(tmp_path) -> None:
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(tmp_path)
    engine = tmp_path / "code/common/scripting/map/mapscriptengine_2.asm"
    engine.write_text(
        engine.read_text(encoding="utf-8").replace(
            "bsr.w GetEntityPortaitAndSpeechSfx\n    jsr (DisplayText).l",
            "jsr (DisplayText).l\n    bsr.w GetEntityPortaitAndSpeechSfx",
            1,
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="DisplayText"):
        _dialogue_handler_facts(
            tmp_path,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )

    target_root = tmp_path / "target"
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(target_root)
    engine = target_root / "code/common/scripting/map/mapscriptengine_2.asm"
    engine.write_text(
        engine.read_text(encoding="utf-8").replace(
            "bsr.w csc1D_showPortrait", "bsr.w csc1C_otherPortrait", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="helper call count"):
        _dialogue_handler_facts(
            target_root,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )


def test_dialogue_handler_guards_reject_sentinel_skip_bit_and_name_mutations(tmp_path) -> None:
    sentinel_root = tmp_path / "sentinel"
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(sentinel_root)
    second = sentinel_root / "code/common/scripting/map/mapscriptengine_2.asm"
    second.write_text(
        second.read_text(encoding="utf-8").replace("cmpi.w #-1,(a6)", "cmpi.w #0,(a6)", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="disagree"):
        _dialogue_handler_facts(
            sentinel_root,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )

    skip_root = tmp_path / "skip"
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(skip_root)
    second = skip_root / "code/common/scripting/map/mapscriptengine_2.asm"
    second.write_text(
        second.read_text(encoding="utf-8").replace("bne.s @skip", "beq.s @skip", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="bne"):
        _dialogue_handler_facts(
            skip_root,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )

    bit_root = tmp_path / "bit"
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(bit_root)
    first = bit_root / "code/common/scripting/map/mapscriptengine_1.asm"
    first.write_text(
        first.read_text(encoding="utf-8").replace("btst #$F,d0", "btst #$D,d0", 1),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="handler-tested modifier"):
        _dialogue_handler_facts(
            bit_root,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )

    unsupported_root = tmp_path / "unsupported"
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(unsupported_root)
    unsupported_pairs = _modifier_entity_pairs()
    unsupported_pairs[(1, 1)] = 1
    with pytest.raises(ValueError, match="handler-tested modifier"):
        _dialogue_handler_facts(
            unsupported_root,
            macros,
            targets,
            handlers,
            unsupported_pairs,
            _synthetic_entity_dialogue_consumer(),
        )

    name_root = tmp_path / "name"
    macros, targets, handlers = _synthetic_dialogue_handler_inputs(name_root)
    second = name_root / "code/common/scripting/map/mapscriptengine_2.asm"
    second.write_text(
        second.read_text(encoding="utf-8").replace(
            "DIALOGUE_NAME_INDEX_2", "DIALOGUE_NAME_INDEX_3", 1
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="name-word"):
        _dialogue_handler_facts(
            name_root,
            macros,
            targets,
            handlers,
            _modifier_entity_pairs(),
            _synthetic_entity_dialogue_consumer(),
        )


def test_entity_dialogue_consumer_guard_rejects_mask_mutation(tmp_path) -> None:
    path = tmp_path / "code/common/scripting/entity"
    path.mkdir(parents=True)
    source_path = path / "getentityportaitandspeechsfx.asm"
    source_path.write_text(
        _named_handler(
            "GetEntityPortaitAndSpeechSfx",
            [
                "andi.w #COMBATANT_MASK_ALL,d0",
                "bsr.w GetEntityAddressFromCharacter",
                "move.b ENTITYDEF_OFFSET_MAPSPRITE(a5),d0",
                "rts",
            ],
        ),
        encoding="utf-8",
    )
    constants = {"COMBATANT_MASK_ALL": 255, "COMBATANT_MASK_INDEX": 63}
    actual = _entity_dialogue_consumer_facts(
        tmp_path, constants, {"GetEntityPortaitAndSpeechSfx": 0x45638}
    )
    assert actual["lowDomainMask"] == {"constant": "COMBATANT_MASK_ALL", "value": 255}

    source_path.write_text(
        source_path.read_text(encoding="utf-8").replace(
            "COMBATANT_MASK_ALL", "COMBATANT_MASK_INDEX"
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="low-domain mask"):
        _entity_dialogue_consumer_facts(
            tmp_path, constants, {"GetEntityPortaitAndSpeechSfx": 0x45638}
        )


def test_dialogue_text_cursor_rejects_source_line_domain_boundary(monkeypatch) -> None:
    original = map_script_engine.build_text_line_domain_contract

    def narrowed_domain(*args, **kwargs):
        value = original(*args, **kwargs)
        value["gamescriptFacts"]["lastLineId"] = 4232
        return value

    monkeypatch.setattr(map_script_engine, "build_text_line_domain_contract", narrowed_domain)
    with pytest.raises(ValueError, match="outside the source text-line domain"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_dialogue_modifier_labels_and_call_parser_reject_near_misses(tmp_path) -> None:
    (tmp_path / "sf2cutscenemacros.asm").write_text(
        "".join(
            f"{macro}: macro\n"
            "    dc.b \\1 ; portrait modifier "
            "($0-none, $40-mirrored, $80-display on right, $FF-undisplayed)\n"
            "    endm\n"
            for macro in DIALOGUE_MODIFIER_MACROS
        ),
        encoding="utf-8",
    )
    labels = _modifier_source_labels(
        tmp_path,
        [{"bit": 15, "destination": "d3"}, {"bit": 14, "destination": "d4"}],
        0xFFFF,
    )
    assert labels[1] == {
        "modifierByteValue": 64,
        "sourceLabel": "mirrored",
        "handlerWordBit": 14,
    }
    source = _statements(
        "; bsr.w GetEntityPortaitAndSpeechSfx\n"
        "GetEntityPortaitAndSpeechSfx:\n"
        "bsr.s GetEntityPortaitAndSpeechSfx ; legal short suffix\n"
        "jsr (GetEntityPortaitAndSpeechSfx).w\n"
        "move.w #GetEntityPortaitAndSpeechSfx,d0\n"
    )
    assert _direct_call_sites(source, "GetEntityPortaitAndSpeechSfx") == [0, 1]

    macro_path = tmp_path / "sf2cutscenemacros.asm"
    macro_path.write_text(
        macro_path.read_text(encoding="utf-8").replace("$40-mirrored", "$20-mirrored"),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="handler bit test"):
        _modifier_source_labels(
            tmp_path,
            [{"bit": 15, "destination": "d3"}, {"bit": 14, "destination": "d4"}],
            0xFFFF,
        )


@pytest.fixture(scope="module")
def map_script_engine_output() -> dict:
    return build_map_script_engine_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )


def test_map_camera_control_contract_matches_complete_golden_fixture(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["mapCameraControlCommandFacts"]
    assert actual == fixture["expected"]["mapCameraControlCommandFacts"]
    assert [
        (
            row["name"],
            row["opcode"],
            row["encodedBytes"],
            row["operandBytes"],
            row["sourceCommandCount"],
        )
        for row in actual["macros"]
    ] == [
        ("setCameraEntity", 36, 4, 2, 125),
        ("setCamDest", 50, 6, 4, 247),
        ("cameraSpeed", 69, 4, 2, 43),
    ]
    assert len(actual["sourceSites"]) == 123
    assert len(actual["sourceSiteOrderKeys"]) == 415
    assert actual["sourceSitesSha256"] == (
        "C285A849AEB914FCCBF0E52D33D84936260120F4AC50DC4D27C2A070031C211A"
    )
    assert len(actual["programTotals"]) == 304
    assert actual["programTotalsSha256"] == (
        "E3F10FDFC69E617255D52DF4ED4FF12B42E8DA496C59DE1D948227D9EBB50EA9"
    )
    assert [
        (
            row["macro"],
            row["handler"],
            row["address"],
            row["opcode"],
            row["sourceCommandCount"],
            row["statementCount"],
        )
        for row in actual["handlers"]
    ] == [
        ("setCameraEntity", "csc24_setCameraTargetEntity", 289848, 36, 125, 11),
        ("setCamDest", "csc32_setCameraDestInTiles", 288006, 50, 247, 7),
        ("cameraSpeed", "csc45_cameraSpeed", 288512, 69, 43, 3),
    ]
    assert actual["handlers"][0]["sectionGuard"]["branchRecords"] == [
        {
            "testInstruction": "move.w (a6)+,d0",
            "branchInstruction": "bmi.w loc_46C52",
            "branchTargetLabel": "loc_46C52",
            "branchTarget": {
                "targetLabel": "loc_46C52",
                "targetInstruction": "move.b d0,((VIEW_TARGET_ENTITY-$1000000)).w",
                "targetStatementIndex": 8,
            },
        },
        {
            "testInstruction": "tst.b d0",
            "branchInstruction": "bpl.s @Ally",
            "branchTargetLabel": "@Ally",
            "branchTarget": {
                "targetLabel": "@Ally",
                "targetInstruction": "andi.w #BYTE_MASK,d0",
                "targetStatementIndex": 6,
            },
        },
    ]
    assert actual["handlers"][2]["sectionGuard"]["scriptCursorWriteUseSites"] == [
        {
            "sourceRegister": "a6",
            "destinationOperand": "((VIEW_SCROLLING_SPEED-$1000000)).w",
            "transferredByteCount": 2,
            "cursorAdvanceByteCount": 2,
            "instruction": "move.w (a6)+,((VIEW_SCROLLING_SPEED-$1000000)).w",
        }
    ]
    assert actual["cameraDestinationServiceFacts"]["sectionGuard"][
        "tileSizeUseSites"
    ] == [
        {
            "symbol": "MAP_TILE_SIZE",
            "value": 384,
            "instruction": "mulu.w #MAP_TILE_SIZE,d2",
        },
        {
            "symbol": "MAP_TILE_SIZE",
            "value": 384,
            "instruction": "mulu.w #MAP_TILE_SIZE,d3",
        },
    ]
    assert actual["callerBreakdown"] == {
        "callerHandlers": [
            {
                "handler": "csc24_setCameraTargetEntity",
                "instructionTargetSiteCounts": {
                    "j_SetCameraDestination": 0,
                    "WaitForViewScrollEnd": 0,
                },
                "effectiveTargetSiteCounts": {
                    "SetCameraDestination": 0,
                    "WaitForViewScrollEnd": 0,
                },
            },
            {
                "handler": "csc32_setCameraDestInTiles",
                "instructionTargetSiteCounts": {
                    "j_SetCameraDestination": 1,
                    "WaitForViewScrollEnd": 1,
                },
                "effectiveTargetSiteCounts": {
                    "SetCameraDestination": 1,
                    "WaitForViewScrollEnd": 1,
                },
            },
            {
                "handler": "csc45_cameraSpeed",
                "instructionTargetSiteCounts": {
                    "j_SetCameraDestination": 0,
                    "WaitForViewScrollEnd": 0,
                },
                "effectiveTargetSiteCounts": {
                    "SetCameraDestination": 0,
                    "WaitForViewScrollEnd": 0,
                },
            },
        ],
        "targetResolutions": [
            {
                "instructionTarget": "j_SetCameraDestination",
                "effectiveTarget": "SetCameraDestination",
                "aliasSourcePath": "code/common/tech/jumpinterfaces/s05_jumpinterface.asm",
                "effectiveTargetScope": "external",
            },
            {
                "instructionTarget": "WaitForViewScrollEnd",
                "effectiveTarget": "WaitForViewScrollEnd",
                "aliasSourcePath": None,
                "effectiveTargetScope": "external",
            },
        ],
        "instructionTargetTotals": {
            "j_SetCameraDestination": 1,
            "WaitForViewScrollEnd": 1,
        },
        "effectiveTargetTotals": {
            "SetCameraDestination": 1,
            "WaitForViewScrollEnd": 1,
        },
        "internalInstructionTargetTotals": {
            "j_SetCameraDestination": 0,
            "WaitForViewScrollEnd": 0,
        },
        "externalInstructionTargetTotals": {
            "j_SetCameraDestination": 1,
            "WaitForViewScrollEnd": 1,
        },
        "internalEffectiveTargetTotals": {
            "SetCameraDestination": 0,
            "WaitForViewScrollEnd": 0,
        },
        "externalEffectiveTargetTotals": {
            "SetCameraDestination": 1,
            "WaitForViewScrollEnd": 1,
        },
    }
    assert actual["sourceIdentityJoins"]["sourceOwners"][-1] == {
        "sourcePath": "code/common/tech/graphics/display.asm",
        "sourceSha256": "8567D93AFEBB8AE628907271EDD3FFD3598B32E4BCB20C0100CAC9F170DF9C21",
        "symbols": ["SetViewDestination"],
    }
    assert actual["runtimeQuestions"] == [
        "map-script-camera-control/normal-story-reachability",
        "map-script-camera-control/vdp-player-visible-behavior",
    ]


def test_map_camera_control_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    output_schema = repo_path("schemas/map-script-engine-static.schema.json")
    fixture_schema = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    validate_json(map_script_engine_output, output_schema, owner="map-camera-control output")
    validate_json(fixture, fixture_schema, owner="map-camera-control fixture")

    missing = deepcopy(map_script_engine_output)
    del missing["mapCameraControlCommandFacts"]["handlers"][0]["sectionGuard"][
        "branchRecords"
    ][0]["branchTarget"]["targetStatementIndex"]
    with pytest.raises(ValueError, match="targetStatementIndex"):
        validate_json(missing, output_schema, owner="map-camera-control output missing field")

    renamed = deepcopy(map_script_engine_output)
    operand = renamed["mapCameraControlCommandFacts"]["sourceSites"][0]["commands"][0][
        "operandValues"
    ][0]
    operand["label"] = operand.pop("sourceComment")
    with pytest.raises(ValueError, match="sourceComment"):
        validate_json(renamed, output_schema, owner="map-camera-control output renamed field")

    extra = deepcopy(map_script_engine_output)
    extra["mapCameraControlCommandFacts"]["sourceIdentityJoins"]["sourceOwners"][0][
        "extra"
    ] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(extra, output_schema, owner="map-camera-control output extra field")

    reordered_source = deepcopy(map_script_engine_output)
    source_order = reordered_source["mapCameraControlCommandFacts"]["sourceSiteOrderKeys"]
    source_order[0], source_order[1] = source_order[1], source_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(reordered_source, output_schema, owner="map-camera-control source order")

    reordered_programs = deepcopy(map_script_engine_output)
    program_order = reordered_programs["mapCameraControlCommandFacts"]["programTotalOrderKeys"]
    program_order[0], program_order[1] = program_order[1], program_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(reordered_programs, output_schema, owner="map-camera-control program order")

    boundary = deepcopy(map_script_engine_output)
    boundary["mapCameraControlCommandFacts"]["cameraDestinationServiceFacts"][
        "sectionGuard"
    ]["tileSizeUseSites"][0]["value"] = 385
    with pytest.raises(ValueError, match="was expected"):
        validate_json(boundary, output_schema, owner="map-camera-control output boundary")

    fixture_missing = deepcopy(fixture)
    del fixture_missing["expected"]["mapCameraControlCommandFacts"]["handlers"][0][
        "sectionGuard"
    ]["branchRecords"][0]["branchTarget"]["targetStatementIndex"]
    with pytest.raises(ValueError, match="targetStatementIndex"):
        validate_json(fixture_missing, fixture_schema, owner="map-camera-control fixture missing")

    fixture_renamed = deepcopy(fixture)
    operand = fixture_renamed["expected"]["mapCameraControlCommandFacts"]["sourceSites"][0][
        "commands"
    ][0]["operandValues"][0]
    operand["label"] = operand.pop("sourceComment")
    with pytest.raises(ValueError, match="sourceComment"):
        validate_json(fixture_renamed, fixture_schema, owner="map-camera-control fixture renamed")

    fixture_extra = deepcopy(fixture)
    fixture_extra["expected"]["mapCameraControlCommandFacts"]["sourceIdentityJoins"][
        "sourceOwners"
    ][0]["extra"] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(fixture_extra, fixture_schema, owner="map-camera-control fixture extra")

    fixture_reordered = deepcopy(fixture)
    source_order = fixture_reordered["expected"]["mapCameraControlCommandFacts"][
        "sourceSiteOrderKeys"
    ]
    source_order[0], source_order[1] = source_order[1], source_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(fixture_reordered, fixture_schema, owner="map-camera-control fixture order")

    fixture_boundary = deepcopy(fixture)
    fixture_boundary["expected"]["mapCameraControlCommandFacts"][
        "cameraDestinationServiceFacts"
    ]["sectionGuard"]["tileSizeUseSites"][0]["value"] = 385
    with pytest.raises(ValueError, match="was expected"):
        validate_json(fixture_boundary, fixture_schema, owner="map-camera-control fixture boundary")


def test_map_camera_control_schema_exact_blocks_keep_large_corpora_compact() -> None:
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for path in schema_paths:
        schema = load_json(path)
        contract = schema["properties"].get("mapCameraControlCommandFacts")
        if contract is None:
            contract = schema["properties"]["expected"]["properties"][
                "mapCameraControlCommandFacts"
            ]
        exact = contract["allOf"][1]
        assert "sourceSites" not in exact["properties"]
        assert "programTotals" not in exact["properties"]
        definitions = {
            name: value
            for name, value in schema["definitions"].items()
            if name.startswith("mapCameraControl")
        }

        def assert_closed_objects(value):
            if isinstance(value, dict):
                if value.get("type") == "object":
                    assert value.get("additionalProperties") is False
                for child in value.values():
                    assert_closed_objects(child)
            elif isinstance(value, list):
                for child in value:
                    assert_closed_objects(child)

        for value in definitions.values():
            assert_closed_objects(value)


def test_dialogue_contract_matches_complete_golden_fixture(map_script_engine_output: dict) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["dialogueCommandFacts"]

    assert actual == fixture["expected"]["dialogueCommandFacts"]
    assert len(actual["programTotals"]) == 304
    assert sum(
        len(row["commandIndexes"]) for row in actual["sourceSiteReferences"]
    ) == 2883


def test_dialogue_schemas_reject_missing_extra_reordered_and_boundary_content(
    map_script_engine_output: dict,
) -> None:
    output_schema = repo_path("schemas/map-script-engine-static.schema.json")
    fixture_schema = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    validate_json(map_script_engine_output, output_schema, owner="dialogue output")
    validate_json(fixture, fixture_schema, owner="dialogue fixture")

    missing = deepcopy(map_script_engine_output)
    del missing["dialogueCommandFacts"]["macros"][0]["operandLayout"][0]["widthBytes"]
    with pytest.raises(ValueError, match="widthBytes"):
        validate_json(missing, output_schema, owner="dialogue output missing field")

    renamed = deepcopy(map_script_engine_output)
    operand = renamed["dialogueCommandFacts"]["macros"][0]["operandLayout"][0]
    operand["widthByte"] = operand.pop("widthBytes")
    with pytest.raises(ValueError, match="widthBytes"):
        validate_json(renamed, output_schema, owner="dialogue output renamed field")

    extra = deepcopy(map_script_engine_output)
    extra["dialogueCommandFacts"]["entityDialogueConsumer"]["lowDomainMask"]["extra"] = 1
    with pytest.raises(ValueError, match="extra"):
        validate_json(extra, output_schema, owner="dialogue output extra field")

    reordered = deepcopy(map_script_engine_output)
    references = reordered["dialogueCommandFacts"]["sourceSiteReferences"]
    references[0], references[1] = references[1], references[0]
    with pytest.raises(ValueError, match="const"):
        validate_json(reordered, output_schema, owner="dialogue output reordered sites")

    missing_zero_caller = deepcopy(map_script_engine_output)
    del missing_zero_caller["dialogueCommandFacts"]["callerBreakdown"]["callerHandlers"][4]
    with pytest.raises(ValueError, match="const"):
        validate_json(
            missing_zero_caller, output_schema, owner="dialogue output missing zero caller"
        )

    extra_caller_target = deepcopy(map_script_engine_output)
    extra_caller_target["dialogueCommandFacts"]["callerBreakdown"]["callerHandlers"][4][
        "instructionTargetSiteCounts"
    ]["csc1C_otherPortrait"] = 0
    with pytest.raises(ValueError, match="csc1C_otherPortrait"):
        validate_json(
            extra_caller_target, output_schema, owner="dialogue output extra caller target"
        )

    reordered_callers = deepcopy(map_script_engine_output)
    caller_rows = reordered_callers["dialogueCommandFacts"]["callerBreakdown"][
        "callerHandlers"
    ]
    caller_rows[4], caller_rows[5] = caller_rows[5], caller_rows[4]
    with pytest.raises(ValueError, match="const"):
        validate_json(reordered_callers, output_schema, owner="dialogue output reordered callers")

    boundary = deepcopy(fixture)
    bounds = boundary["expected"]["dialogueCommandFacts"]["operandFacts"][
        "textCursorValueBounds"
    ]
    bounds["maximum"] += 1
    with pytest.raises(ValueError, match="const"):
        validate_json(boundary, fixture_schema, owner="dialogue fixture boundary")


def test_transition_contract_and_closed_schema_mutations(map_script_engine_output: dict) -> None:
    output_schema = repo_path("schemas/map-script-engine-static.schema.json")
    fixture_schema = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["transitionCommandFacts"]
    assert actual == fixture["expected"]["transitionCommandFacts"]
    assert actual["handlers"] == [
        {
            "macro": "warp",
            "handler": "csc07_warp",
            "address": 291714,
            "opcode": 7,
            "cursorReadWidths": [1, 1, 1, 1],
            "mapEventTypeValue": 1,
            "mapEventClearByteValue": 0,
            "d1Immediate": None,
            "packedCoordinateMultiplier": None,
            "directServiceCalls": [],
            "fallsThroughTo": None,
        },
        {
            "macro": "resetMap",
            "handler": "csc36_resetMap",
            "address": 288142,
            "opcode": 54,
            "cursorReadWidths": [],
            "mapEventTypeValue": None,
            "mapEventClearByteValue": None,
            "d1Immediate": None,
            "packedCoordinateMultiplier": None,
            "directServiceCalls": ["ResetCurrentMap"],
            "fallsThroughTo": None,
        },
        {
            "macro": "loadMapFadeIn",
            "handler": "csc37_loadMapAndFadeIn",
            "address": 288154,
            "opcode": 55,
            "cursorReadWidths": [],
            "mapEventTypeValue": None,
            "mapEventClearByteValue": None,
            "d1Immediate": None,
            "packedCoordinateMultiplier": None,
            "directServiceCalls": [],
            "fallsThroughTo": "csc48_loadMap",
        },
        {
            "macro": "reloadMap",
            "handler": "csc46_reloadMap",
            "address": 288520,
            "opcode": 70,
            "cursorReadWidths": [2, 2],
            "mapEventTypeValue": None,
            "mapEventClearByteValue": None,
            "d1Immediate": -1,
            "packedCoordinateMultiplier": 3,
            "directServiceCalls": ["LoadMap", "EnableDisplayAndInterrupts"],
            "fallsThroughTo": None,
        },
        {
            "macro": "mapLoad",
            "handler": "csc48_loadMap",
            "address": 288182,
            "opcode": 72,
            "cursorReadWidths": [2, 2, 2],
            "mapEventTypeValue": None,
            "mapEventClearByteValue": None,
            "d1Immediate": None,
            "packedCoordinateMultiplier": 3,
            "directServiceCalls": [
                "LoadMapTilesets",
                "LoadMap",
                "EnableDisplayAndInterrupts",
            ],
            "fallsThroughTo": None,
        },
    ]
    assert actual["callerBreakdown"] == {
        "callerHandlers": [
            {
                "handler": "csc07_warp",
                "instructionTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 0,
                    "LoadMap": 0,
                    "EnableDisplayAndInterrupts": 0,
                },
                "effectiveTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 0,
                    "LoadMap": 0,
                    "EnableDisplayAndInterrupts": 0,
                },
            },
            {
                "handler": "csc36_resetMap",
                "instructionTargetSiteCounts": {
                    "ResetCurrentMap": 1,
                    "LoadMapTilesets": 0,
                    "LoadMap": 0,
                    "EnableDisplayAndInterrupts": 0,
                },
                "effectiveTargetSiteCounts": {
                    "ResetCurrentMap": 1,
                    "LoadMapTilesets": 0,
                    "LoadMap": 0,
                    "EnableDisplayAndInterrupts": 0,
                },
            },
            {
                "handler": "csc37_loadMapAndFadeIn",
                "instructionTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 0,
                    "LoadMap": 0,
                    "EnableDisplayAndInterrupts": 0,
                },
                "effectiveTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 0,
                    "LoadMap": 0,
                    "EnableDisplayAndInterrupts": 0,
                },
            },
            {
                "handler": "csc46_reloadMap",
                "instructionTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 0,
                    "LoadMap": 1,
                    "EnableDisplayAndInterrupts": 1,
                },
                "effectiveTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 0,
                    "LoadMap": 1,
                    "EnableDisplayAndInterrupts": 1,
                },
            },
            {
                "handler": "csc48_loadMap",
                "instructionTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 1,
                    "LoadMap": 1,
                    "EnableDisplayAndInterrupts": 1,
                },
                "effectiveTargetSiteCounts": {
                    "ResetCurrentMap": 0,
                    "LoadMapTilesets": 1,
                    "LoadMap": 1,
                    "EnableDisplayAndInterrupts": 1,
                },
            },
        ],
        "targetResolutions": [
            {
                "instructionTarget": "ResetCurrentMap",
                "effectiveTarget": "ResetCurrentMap",
                "effectiveTargetScope": "external",
            },
            {
                "instructionTarget": "LoadMapTilesets",
                "effectiveTarget": "LoadMapTilesets",
                "effectiveTargetScope": "external",
            },
            {
                "instructionTarget": "LoadMap",
                "effectiveTarget": "LoadMap",
                "effectiveTargetScope": "external",
            },
            {
                "instructionTarget": "EnableDisplayAndInterrupts",
                "effectiveTarget": "EnableDisplayAndInterrupts",
                "effectiveTargetScope": "external",
            },
        ],
        "instructionTargetTotals": {
            "ResetCurrentMap": 1,
            "LoadMapTilesets": 1,
            "LoadMap": 2,
            "EnableDisplayAndInterrupts": 2,
        },
        "effectiveTargetTotals": {
            "ResetCurrentMap": 1,
            "LoadMapTilesets": 1,
            "LoadMap": 2,
            "EnableDisplayAndInterrupts": 2,
        },
        "internalEffectiveTargetTotals": {
            "ResetCurrentMap": 0,
            "LoadMapTilesets": 0,
            "LoadMap": 0,
            "EnableDisplayAndInterrupts": 0,
        },
        "externalEffectiveTargetTotals": {
            "ResetCurrentMap": 1,
            "LoadMapTilesets": 1,
            "LoadMap": 2,
            "EnableDisplayAndInterrupts": 2,
        },
    }
    assert actual["runtimeQuestions"] == [
        "map-script-transition-presentation-matrix"
    ]
    assert actual["canonicalMapDomain"] == {
        "contractId": "sf2-map-content-static-v1",
        "mapCount": 79,
        "mapIds": list(range(79)),
        "sourceMapCurrentValue": 255,
    }
    assert [row["sourceCommandCount"] for row in actual["macros"]] == [38, 7, 60, 24, 17]
    assert len(actual["programTotals"]) == 304
    assert actual["canonicalMapDomain"]["mapCount"] == 79
    validate_json(fixture, fixture_schema, owner="transition fixture")

    missing = deepcopy(map_script_engine_output)
    del missing["transitionCommandFacts"]["sourceSites"][0]["commands"][0][
        "coordinateXValue"
    ]
    with pytest.raises(ValueError, match="coordinateXValue"):
        validate_json(missing, output_schema, owner="transition output missing field")

    renamed = deepcopy(map_script_engine_output)
    command = renamed["transitionCommandFacts"]["sourceSites"][0]["commands"][0]
    command["coordinateX"] = command.pop("coordinateXValue")
    with pytest.raises(ValueError, match="coordinateXValue"):
        validate_json(renamed, output_schema, owner="transition output renamed field")

    extra = deepcopy(map_script_engine_output)
    extra["transitionCommandFacts"]["callerBreakdown"]["callerHandlers"][0][
        "instructionTargetSiteCounts"
    ]["OtherTarget"] = 0
    with pytest.raises(ValueError, match="OtherTarget"):
        validate_json(extra, output_schema, owner="transition output extra target")

    reordered = deepcopy(map_script_engine_output)
    totals = reordered["transitionCommandFacts"]["programTotals"]
    totals[0], totals[1] = totals[1], totals[0]
    with pytest.raises(ValueError):
        validate_json(reordered, output_schema, owner="transition output reordered totals")

    out_of_bounds = deepcopy(map_script_engine_output)
    out_of_bounds["transitionCommandFacts"]["sourceSites"][0]["commands"][0][
        "destinationMapValue"
    ] = 79
    with pytest.raises(ValueError):
        validate_json(out_of_bounds, output_schema, owner="transition output map boundary")

    fixture_missing = deepcopy(fixture)
    del fixture_missing["expected"]["transitionCommandFacts"]["handlers"][0][
        "mapEventTypeValue"
    ]
    with pytest.raises(ValueError):
        validate_json(fixture_missing, fixture_schema, owner="transition fixture missing field")


def test_transition_guards_reject_mutated_source_operand_and_use_site(monkeypatch) -> None:
    original_program_corpus = map_script_engine._program_corpus

    def invalid_map_operand(*args, **kwargs):
        corpus = original_program_corpus(*args, **kwargs)
        for program in corpus["programs"]:
            for command in program["commands"]:
                if command["macro"] == "warp":
                    command["arguments"][0] = "79"
                    return corpus
        raise AssertionError("expected a warp source use site")

    monkeypatch.setattr(map_script_engine, "_program_corpus", invalid_map_operand)
    with pytest.raises(ValueError, match="outside the canonical map domain"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_transition_guards_reject_mutated_service_use_site(monkeypatch) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_scale(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc48_loadMap":
            return [statement.replace("mulu.w #3,d0", "mulu.w #2,d0") for statement in statements]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_scale)
    with pytest.raises(ValueError, match="coordinate selector scale disagreement"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_force_state_contract_matches_complete_golden_and_zero_inclusive_maps(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["forceStateCommandFacts"]

    assert actual == fixture["expected"]["forceStateCommandFacts"]
    assert [row["sourceCommandCount"] for row in actual["macros"]] == [34, 0, 0, 5, 1, 3]
    assert len(actual["programTotals"]) == 304
    assert actual["callerBreakdown"]["effectiveTargetTotals"] == {
        "FadeOut_WaitForP1Input": 1,
        "GetClass": 1,
        "GetCombatantX": 1,
        "GetCurrentHp": 1,
        "JoinForce": 3,
        "Sleep": 1,
        "WaitForViewScrollEnd": 1,
    }
    assert actual["callerBreakdown"]["internalEffectiveTargetTotals"] == {
        target: 0
        for target in actual["callerBreakdown"]["effectiveTargetTotals"]
    }
    scopes = {
        row["effectiveTarget"]: row["effectiveTargetScope"]
        for row in actual["callerBreakdown"]["targetResolutions"]
    }
    assert scopes == {
        "FadeOut_WaitForP1Input": "external",
        "GetClass": "external",
        "GetCombatantX": "external",
        "GetCurrentHp": "external",
        "JoinForce": "external",
        "Sleep": "external",
        "WaitForViewScrollEnd": "external",
    }
    assert actual["callerBreakdown"]["internalEffectiveTargetTotals"] == {
        target: actual["callerBreakdown"]["effectiveTargetTotals"][target]
        if scopes[target] == "internal"
        else 0
        for target in actual["callerBreakdown"]["effectiveTargetTotals"]
    }
    assert actual["callerBreakdown"]["externalEffectiveTargetTotals"] == {
        target: actual["callerBreakdown"]["effectiveTargetTotals"][target]
        if scopes[target] == "external"
        else 0
        for target in actual["callerBreakdown"]["effectiveTargetTotals"]
    }
    assert actual["commonStatsIdentity"] == {
        "contractId": "sf2-common-stats-static-v1",
        "upstreamCommit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
        "sourcePath": "code/common/stats/battleparty.asm",
        "sourceSha256": "670A25075D807BA60B0AA3C6D158DDF80E5248264753361DBC495F7655ED8B37",
        "services": ["JoinForce", "UpdateForce"],
    }
    assert actual["runtimeQuestions"] == [
        "force-state/roster-death-persistence-visible-outcomes"
    ]
    active = actual["activePartyCommandFacts"]
    assert active == fixture["expected"]["forceStateCommandFacts"]["activePartyCommandFacts"]
    assert [row["sourceCommandCount"] for row in active["macros"]] == [1, 4, 5, 19]
    assert len(active["programTotals"]) == 304
    assert active["callerBreakdown"]["effectiveTargetTotals"] == {
        "AddFollower": 1,
        "GetActivationBitfield": 1,
        "GetCurrentHp": 1,
        "GetEntityAddressFromCharacter": 1,
        "IsInBattleParty": 1,
        "JoinBattleParty": 1,
        "JoinForce": 1,
        "LeaveBattleParty": 1,
        "ResetAlliesBattleStats": 1,
        "SetActivationBitfield": 1,
        "UpdateForce": 1,
    }
    assert active["runtimeQuestions"] == [
        "force-state/active-party-ai-follower-runtime-matrix"
    ]
    assert active["handlers"][0]["sectionGuard"]["mutationCallOrder"] == [
        "move.w #-1,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
        "move.w d0,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
        "jsr j_LeaveBattleParty",
        "jsr j_JoinBattleParty",
    ]


def test_force_state_schemas_reject_nested_mutations_and_boundary_content(
    map_script_engine_output: dict,
) -> None:
    output_schema = repo_path("schemas/map-script-engine-static.schema.json")
    fixture_schema = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    validate_json(map_script_engine_output, output_schema, owner="force-state output")
    validate_json(fixture, fixture_schema, owner="force-state fixture")

    missing = deepcopy(map_script_engine_output)
    del missing["forceStateCommandFacts"]["handlers"][0]["sectionGuard"]["branchRecords"][0][
        "branchInstruction"
    ]
    with pytest.raises(ValueError, match="branchInstruction"):
        validate_json(missing, output_schema, owner="force-state output missing field")

    renamed = deepcopy(map_script_engine_output)
    branch = renamed["forceStateCommandFacts"]["handlers"][0]["sectionGuard"]["branchRecords"][0]
    branch["branch"] = branch.pop("branchInstruction")
    with pytest.raises(ValueError, match="branchInstruction"):
        validate_json(renamed, output_schema, owner="force-state output renamed field")

    extra = deepcopy(map_script_engine_output)
    extra["forceStateCommandFacts"]["handlers"][0]["sectionGuard"]["branchRecords"][0][
        "extra"
    ] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(extra, output_schema, owner="force-state output extra field")

    reordered = deepcopy(map_script_engine_output)
    totals = reordered["forceStateCommandFacts"]["programTotals"]
    totals[0], totals[1] = totals[1], totals[0]
    with pytest.raises(ValueError, match="const"):
        validate_json(reordered, output_schema, owner="force-state output reordered totals")

    out_of_bounds = deepcopy(map_script_engine_output)
    out_of_bounds["forceStateCommandFacts"]["macros"][0]["encodedBytes"] = 3
    with pytest.raises(ValueError, match="const"):
        validate_json(out_of_bounds, output_schema, owner="force-state output boundary")

    wrong_scope = deepcopy(map_script_engine_output)
    wrong_scope["forceStateCommandFacts"]["callerBreakdown"]["targetResolutions"][0][
        "effectiveTargetScope"
    ] = "internal"
    with pytest.raises(ValueError, match="const"):
        validate_json(wrong_scope, output_schema, owner="force-state output wrong scope")

    extra_effective_target = deepcopy(map_script_engine_output)
    extra_effective_target["forceStateCommandFacts"]["callerBreakdown"][
        "effectiveTargetTotals"
    ]["OtherTarget"] = 0
    with pytest.raises(ValueError, match="OtherTarget"):
        validate_json(
            extra_effective_target, output_schema, owner="force-state output extra target"
        )

    fixture_missing = deepcopy(fixture)
    del fixture_missing["expected"]["forceStateCommandFacts"]["handlers"][4]["sectionGuard"]
    with pytest.raises(ValueError, match="sectionGuard"):
        validate_json(fixture_missing, fixture_schema, owner="force-state fixture missing field")

    active_missing = deepcopy(map_script_engine_output)
    del active_missing["forceStateCommandFacts"]["activePartyCommandFacts"]["handlers"][0][
        "sectionGuard"
    ]["sourceLiteralUses"][0]["instruction"]
    with pytest.raises(ValueError, match="instruction"):
        validate_json(active_missing, output_schema, owner="active-party output missing field")

    active_renamed = deepcopy(map_script_engine_output)
    literal = active_renamed["forceStateCommandFacts"]["activePartyCommandFacts"][
        "handlers"
    ][0]["sectionGuard"]["sourceLiteralUses"][0]
    literal["sourceInstruction"] = literal.pop("instruction")
    with pytest.raises(ValueError, match="instruction"):
        validate_json(active_renamed, output_schema, owner="active-party output renamed field")

    active_extra = deepcopy(map_script_engine_output)
    active_extra["forceStateCommandFacts"]["activePartyCommandFacts"]["sourceIdentityJoins"][
        "followerOwner"
    ]["extra"] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(active_extra, output_schema, owner="active-party output extra field")

    active_owner_missing = deepcopy(map_script_engine_output)
    del active_owner_missing["forceStateCommandFacts"]["activePartyCommandFacts"][
        "sourceIdentityJoins"
    ]["battleStatsOwner"]
    with pytest.raises(ValueError, match="battleStatsOwner"):
        validate_json(
            active_owner_missing,
            output_schema,
            owner="active-party output missing battle-stats owner",
        )

    active_reordered = deepcopy(map_script_engine_output)
    active_macros = active_reordered["forceStateCommandFacts"]["activePartyCommandFacts"][
        "macros"
    ]
    active_macros[0], active_macros[1] = active_macros[1], active_macros[0]
    with pytest.raises(ValueError, match="const"):
        validate_json(active_reordered, output_schema, owner="active-party output reordered macros")

    active_out_of_bounds = deepcopy(map_script_engine_output)
    active_out_of_bounds["forceStateCommandFacts"]["activePartyCommandFacts"]["handlers"][
        1
    ]["sectionGuard"]["sourceConstantUses"][0]["value"] = 5
    with pytest.raises(ValueError, match="const"):
        validate_json(
            active_out_of_bounds,
            output_schema,
            owner="active-party output source constant boundary",
        )

    fixture_active_missing = deepcopy(fixture)
    del fixture_active_missing["expected"]["forceStateCommandFacts"]["activePartyCommandFacts"][
        "handlers"
    ][3]["sectionGuard"]["sourceLiteralUses"]
    with pytest.raises(ValueError, match="sourceLiteralUses"):
        validate_json(
            fixture_active_missing,
            fixture_schema,
            owner="active-party fixture missing field",
        )


def test_force_state_section_guards_reject_mutated_branch_operands_before_fixture(
    monkeypatch,
) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_use_site(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc20_updateDefeatedAllies":
            return [statement.replace("cmpi.w #-1,d1", "cmpi.w #0,d1") for statement in statements]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_use_site)
    with pytest.raises(ValueError, match="comparison operand drift"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_force_state_section_guard_rejects_branch_polarity_mutation() -> None:
    statements = [
        "move.w (a6)+,d0",
        "jsr j_GetCurrentHp",
        "tst.w d1",
        "bne.w alive",
        "movea.l (a6),a6",
        "bra.s return",
        "addq.w #4,a6",
        "rts",
    ]
    assert _force_state_section_guard("jumpIfDead", statements, {})["branchRecords"][0] == {
        "testInstruction": "tst.w d1",
        "branchInstruction": "bne.w alive",
        "fallthroughInstruction": "movea.l (a6),a6",
        "branchTargetInstruction": "addq.w #4,a6",
    }
    statements[3] = "beq.w alive"
    with pytest.raises(ValueError, match="csc0F_jumpIfCharacterDead statement is missing"):
        _force_state_section_guard("jumpIfDead", statements, {})


@pytest.mark.parametrize(
    ("handler_name", "original", "replacement", "error"),
    [
        (
            "csc51_joinBattleParty",
            "move.w #-1,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
            "move.w #0,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
            "initialization source use drift",
        ),
        (
            "csc54_joinForceAi",
            "ori.w #AIBITFIELD_AI_CONTROLLED,d1",
            "ori.w #AIBITFIELD_NEUTRAL,d1",
            "csc54_joinForceAi statement is missing",
        ),
        (
            "csc55_resetCharacterBattleStats",
            "jsr ResetAlliesBattleStats",
            "jsr ResetAlliesBattleStatsLater",
            "csc55_resetCharacterBattleStats statement is missing",
        ),
        (
            "csc56_addFollower",
            "jsr AddFollower",
            "jsr AddFollowerLater",
            "csc56_addFollower statement is missing",
        ),
    ],
)
def test_active_party_section_guards_reject_use_site_mutations_before_fixture(
    monkeypatch, handler_name: str, original: str, replacement: str, error: str
) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_use_site(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == handler_name:
            return [statement.replace(original, replacement) for statement in statements]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_use_site)
    with pytest.raises(ValueError, match=error):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_active_party_section_guard_rejects_follower_sentinel_and_call_order() -> None:
    statements = [
        "move.w (a6)+,d0",
        "bsr.w GetEntityAddressFromCharacter",
        "moveq #0,d1",
        "lea ((EXPLORATION_ENTITIES-$1000000)).w,a0",
        "cmpi.b #-1,(a0)",
        "beq.w @break",
        "move.b (a0)+,d1",
        "bra.s @loop",
        "move.w #$FFE8,d2",
        "move.w #0,d3",
        "jsr AddFollower",
        "rts",
    ]
    guard = _active_party_section_guard("addNewFollower", statements, {})
    assert guard["sourceLiteralUses"] == [
        {"value": -1, "instruction": "cmpi.b #-1,(a0)"},
        {"value": 65512, "instruction": "move.w #$FFE8,d2"},
        {"value": 0, "instruction": "move.w #0,d3"},
    ]
    statements[4] = "cmpi.b #0,(a0)"
    with pytest.raises(ValueError, match="follower sentinel"):
        _active_party_section_guard("addNewFollower", statements, {})
    statements[4] = "cmpi.b #-1,(a0)"
    statements[10] = "jsr AddFollowerLater"
    with pytest.raises(ValueError, match="csc56_addFollower statement is missing"):
        _active_party_section_guard("addNewFollower", statements, {})


def test_story_state_contract_matches_complete_golden_and_caller_identities(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["storyStateCommandFacts"]

    assert actual == fixture["expected"]["storyStateCommandFacts"]
    assert [
        (
            row["name"],
            row["opcode"],
            row["encodedBytes"],
            row["aliasOf"],
            row["handler"],
            row["sourceCommandCount"],
        )
        for row in actual["macros"]
    ] == [
        ("jumpIfFlagSet", 12, 8, None, "csc0C_jumpIfFlagSet", 24),
        ("jumpIfFlagClear", 13, 8, None, "csc0D_jumpIfFlagClear", 27),
        ("csc10", 16, 6, None, "csc10_toggleFlag", 0),
        ("setF", 16, 6, "csc10", "csc10_toggleFlag", 37),
        ("clearF", 16, 6, "csc10", "csc10_toggleFlag", 16),
        ("yesNo", 17, 2, None, "csc11_promptYesNoForStoryFlow", 22),
        ("setStoryFlag", 19, 4, None, "csc13_setStoryFlag", 20),
    ]
    assert actual["macros"][3]["operandLayout"][1] == {
        "streamOffset": 4,
        "widthBytes": 2,
        "expression": "$FFFF",
        "parameterOrdinals": [],
        "encoding": "direct",
    }
    assert actual["macros"][4]["operandLayout"][1] == {
        "streamOffset": 4,
        "widthBytes": 2,
        "expression": "0",
        "parameterOrdinals": [],
        "encoding": "direct",
    }
    assert sum(row["sourceCommandCount"] for row in actual["macros"]) == 146
    assert len(actual["programTotals"]) == 304
    assert actual["programCorpusReferences"] == [
        {"field": "conditionalReads", "entryCount": 51},
        {"field": "directWrites", "entryCount": 53},
        {"field": "yesNoPromptWrites", "entryCount": 22},
        {"field": "battleUnlockWrites", "entryCount": 20},
    ]
    assert [
        (
            row["handler"],
            row["address"],
            row["opcode"],
            row["sourceCommandCount"],
            row["cursorReadWidths"],
        )
        for row in actual["handlers"]
    ] == [
        ("csc0C_jumpIfFlagSet", 291864, 12, 24, [2, 4]),
        ("csc0D_jumpIfFlagClear", 291884, 13, 27, [2, 4]),
        ("csc10_toggleFlag", 291962, 16, 53, [2, 2]),
        ("csc11_promptYesNoForStoryFlow", 291984, 17, 22, []),
        ("csc13_setStoryFlag", 292064, 19, 20, [2]),
    ]
    assert actual["handlers"][0]["sectionGuard"]["branchRecords"] == [
        {
            "testInstruction": "jsr j_CheckFlag",
            "branchInstruction": "beq.w loc_47428",
            "fallthroughInstruction": "movea.l (a6),a6",
            "branchTargetInstruction": "addq.w #4,a6",
        }
    ]
    assert actual["handlers"][1]["sectionGuard"]["branchRecords"][0][
        "branchInstruction"
    ] == "bne.w loc_4743C"
    assert actual["handlers"][2]["sectionGuard"]["mutationCallOrder"] == [
        "jsr j_ClearFlag",
        "jsr j_SetFlag",
    ]
    assert actual["handlers"][3]["sectionGuard"] == {
        "orderedInstructions": [
            "move.l a6,-(sp)",
            "jsr j_YesNoPrompt",
            "movea.l (sp)+,a6",
            "moveq #FLAG_INDEX_YES_NO_PROMPT,d1",
            "tst.w d0",
            "bne.s loc_474A8",
            "jsr j_SetFlag",
            "bra.s loc_474AE",
            "jsr j_ClearFlag",
            "moveq #10,d0",
            "jsr (Sleep).w",
            "rts",
        ],
        "branchRecords": [
            {
                "testInstruction": "tst.w d0",
                "branchInstruction": "bne.s loc_474A8",
                "fallthroughInstruction": "jsr j_SetFlag",
                "branchTargetInstruction": "jsr j_ClearFlag",
            }
        ],
        "sourceConstantUses": [
            {
                "constant": "FLAG_INDEX_YES_NO_PROMPT",
                "value": 89,
                "instruction": "moveq #FLAG_INDEX_YES_NO_PROMPT,d1",
            }
        ],
        "sourceLiteralUses": [{"value": 10, "instruction": "moveq #10,d0"}],
        "mutationCallOrder": [
            "move.l a6,-(sp)",
            "jsr j_YesNoPrompt",
            "movea.l (sp)+,a6",
            "jsr j_SetFlag",
            "jsr j_ClearFlag",
            "moveq #10,d0",
            "jsr (Sleep).w",
        ],
    }
    assert actual["handlers"][4]["sectionGuard"]["sourceConstantUses"] == [
        {
            "constant": "BATTLE_UNLOCKED_FLAGS_START",
            "value": 400,
            "instruction": "addi.w #BATTLE_UNLOCKED_FLAGS_START,d1",
        }
    ]
    assert actual["callerBreakdown"]["instructionTargetTotals"] == {
        "Sleep": 1,
        "j_CheckFlag": 2,
        "j_ClearFlag": 2,
        "j_SetFlag": 3,
        "j_YesNoPrompt": 1,
    }
    assert actual["callerBreakdown"]["effectiveTargetTotals"] == {
        "CheckFlag": 2,
        "ClearFlag": 2,
        "SetFlag": 3,
        "Sleep": 1,
        "YesNoPrompt": 1,
    }
    assert actual["callerBreakdown"]["internalEffectiveTargetTotals"] == {
        "CheckFlag": 0,
        "ClearFlag": 0,
        "SetFlag": 0,
        "Sleep": 0,
        "YesNoPrompt": 0,
    }
    assert actual["callerBreakdown"]["externalEffectiveTargetTotals"] == actual[
        "callerBreakdown"
    ]["effectiveTargetTotals"]
    assert actual["callerBreakdown"]["targetResolutions"] == [
        {
            "instructionTarget": "Sleep",
            "effectiveTarget": "Sleep",
            "aliasSourcePath": None,
            "effectiveTargetScope": "external",
        },
        {
            "instructionTarget": "j_CheckFlag",
            "effectiveTarget": "CheckFlag",
            "aliasSourcePath": "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
            "effectiveTargetScope": "external",
        },
        {
            "instructionTarget": "j_ClearFlag",
            "effectiveTarget": "ClearFlag",
            "aliasSourcePath": "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
            "effectiveTargetScope": "external",
        },
        {
            "instructionTarget": "j_SetFlag",
            "effectiveTarget": "SetFlag",
            "aliasSourcePath": "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
            "effectiveTargetScope": "external",
        },
        {
            "instructionTarget": "j_YesNoPrompt",
            "effectiveTarget": "YesNoPrompt",
            "aliasSourcePath": "code/common/tech/jumpinterfaces/s03_jumpinterface_1.asm",
            "effectiveTargetScope": "external",
        },
    ]
    assert actual["sourceIdentityJoins"] == {
        "commonStatsFlags": {
            "contractId": "sf2-common-stats-static-v1",
            "upstreamCommit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
            "sourcePath": "code/common/stats/gameflags.asm",
            "sourceSha256": "1D9BA2EAD0CEA13718D20B0E96D86FD0AC01730E1C6C07A15F9E3EF875A45DD9",
            "symbols": ["CheckFlag", "SetFlag", "ClearFlag"],
        },
        "yesNoOwner": {
            "sourcePath": "code/common/menus/yesnoprompt.asm",
            "sourceSha256": "CF54DD1628DB83CA94F4AACA9E854A8356BB2658A5396A32950F5F31219518CA",
            "symbols": ["YesNoPrompt"],
        },
    }
    assert actual["runtimeQuestions"] == [
        "story-state/branch-prompt-persistence-matrix"
    ]


def test_story_state_section_guard_rejects_branch_polarity() -> None:
    statements = [
        "move.w (a6)+,d1",
        "jsr j_CheckFlag",
        "beq.w skip",
        "movea.l (a6),a6",
        "bra.s return",
        "addq.w #4,a6",
        "rts",
    ]
    assert _story_state_section_guard("csc0C_jumpIfFlagSet", statements, {})[
        "branchRecords"
    ] == [
        {
            "testInstruction": "jsr j_CheckFlag",
            "branchInstruction": "beq.w skip",
            "fallthroughInstruction": "movea.l (a6),a6",
            "branchTargetInstruction": "addq.w #4,a6",
        }
    ]
    statements[2] = "bne.w skip"
    with pytest.raises(ValueError, match="csc0C_jumpIfFlagSet statement is missing"):
        _story_state_section_guard("csc0C_jumpIfFlagSet", statements, {})


def test_story_state_conditional_skip_width_is_derived_from_macro_layout(monkeypatch) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_skip_width(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc0C_jumpIfFlagSet":
            return [statement.replace("addq.w #4,a6", "addq.w #2,a6") for statement in statements]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_skip_width)
    with pytest.raises(ValueError, match="conditional target skip width drift"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_story_state_source_use_site_mutation_fails_before_fixture(monkeypatch) -> None:
    original_program_corpus = map_script_engine._program_corpus

    def changed_branch_polarity(*args, **kwargs):
        corpus = original_program_corpus(*args, **kwargs)
        for program in corpus["programs"]:
            for command in program["commands"]:
                if command["macro"] == "jumpIfFlagSet":
                    command["macro"] = "jumpIfFlagClear"
                    return corpus
        raise AssertionError("expected a jumpIfFlagSet source use site")

    monkeypatch.setattr(map_script_engine, "_program_corpus", changed_branch_polarity)
    with pytest.raises(ValueError, match="conditional read use-site drift"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_story_state_corpus_order_keys_reject_source_reordering_before_fixture(
    monkeypatch,
) -> None:
    original_program_corpus = map_script_engine._program_corpus

    def reordered_story_programs(*args, **kwargs):
        corpus = original_program_corpus(*args, **kwargs)
        story_programs = [
            index
            for index, program in enumerate(corpus["programs"])
            if any(
                command["macro"] in map_script_engine.STORY_STATE_MACRO_NAMES
                for command in program["commands"]
            )
        ]
        first, second = story_programs[:2]
        corpus["programs"][first], corpus["programs"][second] = (
            corpus["programs"][second],
            corpus["programs"][first],
        )
        return corpus

    monkeypatch.setattr(map_script_engine, "_program_corpus", reordered_story_programs)
    output = build_map_script_engine_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            output,
            repo_path("schemas/map-script-engine-static.schema.json"),
            owner="story-state source-order output",
        )


def test_story_state_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    output_schema = repo_path("schemas/map-script-engine-static.schema.json")
    fixture_schema = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    validate_json(map_script_engine_output, output_schema, owner="story-state output")
    validate_json(fixture, fixture_schema, owner="story-state fixture")

    missing = deepcopy(map_script_engine_output)
    del missing["storyStateCommandFacts"]["sourceSites"][0]["commands"][0][
        "storyStateReference"
    ]["entryIndex"]
    with pytest.raises(ValueError, match="entryIndex"):
        validate_json(missing, output_schema, owner="story-state output missing field")

    renamed = deepcopy(map_script_engine_output)
    reference = renamed["storyStateCommandFacts"]["sourceSites"][0]["commands"][0][
        "storyStateReference"
    ]
    reference["index"] = reference.pop("entryIndex")
    with pytest.raises(ValueError, match="entryIndex"):
        validate_json(renamed, output_schema, owner="story-state output renamed field")

    extra = deepcopy(map_script_engine_output)
    extra["storyStateCommandFacts"]["sourceIdentityJoins"]["yesNoOwner"]["extra"] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(extra, output_schema, owner="story-state output extra field")

    reordered = deepcopy(map_script_engine_output)
    macros = reordered["storyStateCommandFacts"]["macros"]
    macros[0], macros[1] = macros[1], macros[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(reordered, output_schema, owner="story-state output reordered macros")

    reordered_source_order = deepcopy(map_script_engine_output)
    keys = reordered_source_order["storyStateCommandFacts"]["sourceSiteOrderKeys"]
    keys[0], keys[1] = keys[1], keys[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            reordered_source_order,
            output_schema,
            owner="story-state output reordered source-site keys",
        )

    reordered_program_order = deepcopy(map_script_engine_output)
    keys = reordered_program_order["storyStateCommandFacts"]["programTotalOrderKeys"]
    keys[0], keys[1] = keys[1], keys[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            reordered_program_order,
            output_schema,
            owner="story-state output reordered program-total keys",
        )

    out_of_bounds = deepcopy(map_script_engine_output)
    out_of_bounds["storyStateCommandFacts"]["handlers"][3]["sectionGuard"][
        "sourceConstantUses"
    ][0]["value"] = 90
    with pytest.raises(ValueError, match="was expected"):
        validate_json(out_of_bounds, output_schema, owner="story-state output boundary")

    fixture_missing = deepcopy(fixture)
    del fixture_missing["expected"]["storyStateCommandFacts"]["handlers"][4][
        "sectionGuard"
    ]["sourceConstantUses"]
    with pytest.raises(ValueError, match="sourceConstantUses"):
        validate_json(fixture_missing, fixture_schema, owner="story-state fixture missing field")


@pytest.mark.parametrize(
    ("handler_name", "original", "replacement", "error"),
    [
        (
            "csc0C_jumpIfFlagSet",
            "beq.w loc_47428",
            "bne.w loc_47428",
            "csc0C_jumpIfFlagSet statement is missing",
        ),
        (
            "csc0D_jumpIfFlagClear",
            "bne.w loc_4743C",
            "beq.w loc_4743C",
            "csc0D_jumpIfFlagClear statement is missing",
        ),
        (
            "csc10_toggleFlag",
            "jsr j_ClearFlag",
            "jsr j_ClearFlagLater",
            "csc10_toggleFlag statement is missing",
        ),
        (
            "csc11_promptYesNoForStoryFlow",
            "moveq #FLAG_INDEX_YES_NO_PROMPT,d1",
            "moveq #FLAG_INDEX_YES_NO_PROMPT_ALT,d1",
            "csc11_promptYesNoForStoryFlow statement is missing",
        ),
        (
            "csc13_setStoryFlag",
            "addi.w #BATTLE_UNLOCKED_FLAGS_START,d1",
            "addi.w #BATTLE_UNLOCKED_FLAGS_START_ALT,d1",
            "csc13_setStoryFlag statement is missing",
        ),
    ],
)
def test_story_state_section_guards_reject_use_site_mutations_before_fixture(
    monkeypatch, handler_name: str, original: str, replacement: str, error: str
) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_use_site(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == handler_name:
            return [statement.replace(original, replacement) for statement in statements]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_use_site)
    with pytest.raises(ValueError, match=error):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_map_block_mutation_contract_matches_complete_golden_and_helper_provenance(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["mapBlockMutationCommandFacts"]

    assert actual == fixture["expected"]["mapBlockMutationCommandFacts"]
    assert [
        (
            row["name"],
            row["opcode"],
            row["encodedBytes"],
            row["operandBytes"],
            row["handler"],
            row["sourceCommandCount"],
        )
        for row in actual["macros"]
    ] == [
        ("setBlocks", 52, 8, 6, "csc34_setBlocks", 201),
        ("setBlocksVar", 53, 8, 6, "csc35_setBlocksVar", 7),
    ]
    assert len(actual["sourceSiteOrderKeys"]) == 208
    assert len(actual["programTotalOrderKeys"]) == 304
    assert actual["operandValueBounds"] == [
        {
            "parameterOrdinal": 1,
            "sourceLabel": "source x",
            "minimumValue": 0,
            "minimumSourceSiteKey": "abcs_battle07:119:setBlocks",
            "maximumValue": 63,
            "maximumSourceSiteKey": "cs_528D4:15:setBlocks",
            "sourceValueCount": 208,
        },
        {
            "parameterOrdinal": 2,
            "sourceLabel": "source y",
            "minimumValue": 0,
            "minimumSourceSiteKey": "bbcs_16:58:setBlocks",
            "maximumValue": 63,
            "maximumSourceSiteKey": "cs_503A6:148:setBlocks",
            "sourceValueCount": 208,
        },
        {
            "parameterOrdinal": 3,
            "sourceLabel": "width",
            "minimumValue": 1,
            "minimumSourceSiteKey": "IntroCutscene1:133:setBlocks",
            "maximumValue": 22,
            "maximumSourceSiteKey": "cs_5ED06:6:setBlocks",
            "sourceValueCount": 208,
        },
        {
            "parameterOrdinal": 4,
            "sourceLabel": "height",
            "minimumValue": 1,
            "minimumSourceSiteKey": "IntroCutscene1:133:setBlocks",
            "maximumValue": 29,
            "maximumSourceSiteKey": "bbcs_40:16:setBlocks",
            "sourceValueCount": 208,
        },
        {
            "parameterOrdinal": 5,
            "sourceLabel": "destination x",
            "minimumValue": 0,
            "minimumSourceSiteKey": "bbcs_16:5:setBlocks",
            "maximumValue": 63,
            "maximumSourceSiteKey": "cs_503A6:124:setBlocks",
            "sourceValueCount": 208,
        },
        {
            "parameterOrdinal": 6,
            "sourceLabel": "destination y",
            "minimumValue": 0,
            "minimumSourceSiteKey": "abcs_battle40:13:setBlocks",
            "maximumValue": 63,
            "maximumSourceSiteKey": "cs_503A6:124:setBlocks",
            "sourceValueCount": 208,
        },
    ]
    assert actual["inputWordGroups"] == [
        {
            "handlerRegister": "d0",
            "sourceParameterOrdinals": [1, 2],
            "sourceLabels": ["source x", "source y"],
            "streamOffset": 2,
            "transferredByteCount": 2,
            "cursorReadInstruction": "move.w (a6)+,d0",
        },
        {
            "handlerRegister": "d1",
            "sourceParameterOrdinals": [3, 4],
            "sourceLabels": ["width", "height"],
            "streamOffset": 4,
            "transferredByteCount": 2,
            "cursorReadInstruction": "move.w (a6)+,d1",
        },
        {
            "handlerRegister": "d2",
            "sourceParameterOrdinals": [5, 6],
            "sourceLabels": ["destination x", "destination y"],
            "streamOffset": 6,
            "transferredByteCount": 2,
            "cursorReadInstruction": "move.w (a6)+,d2",
        },
    ]
    assert [
        (row["handler"], row["address"], row["opcode"], row["sourceCommandCount"])
        for row in actual["handlers"]
    ] == [
        ("csc34_setBlocks", 288102, 52, 201),
        ("csc35_setBlocksVar", 288130, 53, 7),
    ]
    assert actual["handlers"][0]["sectionGuard"]["postCallBitSetUseSites"] == [
        {
            "bitIndex": 0,
            "sourceTarget": "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD",
            "instruction": "bset #0,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l",
        },
        {
            "bitIndex": 1,
            "sourceTarget": "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD",
            "instruction": "bset #1,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l",
        },
    ]
    assert actual["handlers"][1]["sectionGuard"]["postCallBitSetUseSites"] == []
    assert actual["copyMapBlocksHelperFacts"]["derivedAddressStride"] == {
        "addressRowShiftBits": 6,
        "wordCopyByteStride": 2,
        "rowByteStride": 128,
    }
    assert actual["copyMapBlocksHelperFacts"]["inputByteShiftConstantUses"] == [
        {
            "constant": "BYTE_SHIFT_COUNT",
            "value": 8,
            "instruction": "lsr.w #BYTE_SHIFT_COUNT,d6",
        },
        {
            "constant": "BYTE_SHIFT_COUNT",
            "value": 8,
            "instruction": "lsr.w #BYTE_SHIFT_COUNT,d2",
        },
        {
            "constant": "BYTE_SHIFT_COUNT",
            "value": 8,
            "instruction": "lsr.w #BYTE_SHIFT_COUNT,d0",
        },
    ]
    assert actual["callerBreakdown"] == {
        "callerHandlers": [
            {
                "handler": "csc34_setBlocks",
                "instructionTargetSiteCounts": {"CopyMapBlocks": 1},
                "effectiveTargetSiteCounts": {"CopyMapBlocks": 1},
            },
            {
                "handler": "csc35_setBlocksVar",
                "instructionTargetSiteCounts": {"CopyMapBlocks": 1},
                "effectiveTargetSiteCounts": {"CopyMapBlocks": 1},
            },
        ],
        "targetResolutions": [
            {
                "instructionTarget": "CopyMapBlocks",
                "effectiveTarget": "CopyMapBlocks",
                "aliasSourcePath": None,
                "effectiveTargetScope": "external",
            }
        ],
        "instructionTargetTotals": {"CopyMapBlocks": 2},
        "effectiveTargetTotals": {"CopyMapBlocks": 2},
        "internalEffectiveTargetTotals": {"CopyMapBlocks": 0},
        "externalEffectiveTargetTotals": {"CopyMapBlocks": 2},
    }
    assert actual["sourceIdentityJoins"] == {
        "copyMapBlocksOwner": {
            "sourcePath": "code/gameflow/exploration/exploration.asm",
            "sourceSha256": "C38279815C832B5D65B443092048BB92E19FAEE47B81734A3EF0D16AA0E445A0",
            "symbols": ["CopyMapBlocks"],
        }
    }
    assert actual["runtimeQuestions"] == ["map-block-mutation/runtime-effects-matrix"]


def test_map_block_mutation_guards_reject_source_mutations_before_fixture(monkeypatch) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_use_site(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc34_setBlocks":
            return [
                statement.replace("bset #1,", "bset #2,")
                for statement in statements
            ]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_use_site)
    with pytest.raises(ValueError, match="bit-set order drift"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_map_block_mutation_handler_order_fails_before_fixture(monkeypatch) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_call_order(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc34_setBlocks":
            statements[3], statements[4] = statements[4], statements[3]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_call_order)
    with pytest.raises(ValueError, match="csc34_setBlocks statement is missing"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_map_block_mutation_copy_stride_use_site_fails_before_fixture(monkeypatch) -> None:
    original_section = map_script_engine._map_block_named_section_statements

    def changed_row_stride(disasm, source_path, name):
        statements = original_section(disasm, source_path, name)
        if name == "CopyMapBlocks":
            return [statement.replace("addi.w #128", "addi.w #64") for statement in statements]
        return statements

    monkeypatch.setattr(
        map_script_engine, "_map_block_named_section_statements", changed_row_stride
    )
    with pytest.raises(ValueError, match="row-stride relationship drift"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_map_block_mutation_corpus_order_schema_rejects_source_reordering(
    monkeypatch,
) -> None:
    original_program_corpus = map_script_engine._program_corpus

    def reordered_block_mutation_programs(*args, **kwargs):
        corpus = original_program_corpus(*args, **kwargs)
        matching_indexes = [
            index
            for index, program in enumerate(corpus["programs"])
            if any(
                command["macro"] in map_script_engine.MAP_BLOCK_MUTATION_MACRO_NAMES
                for command in program["commands"]
            )
        ]
        first, second = matching_indexes[:2]
        corpus["programs"][first], corpus["programs"][second] = (
            corpus["programs"][second],
            corpus["programs"][first],
        )
        return corpus

    monkeypatch.setattr(map_script_engine, "_program_corpus", reordered_block_mutation_programs)
    output = build_map_script_engine_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            output,
            repo_path("schemas/map-script-engine-static.schema.json"),
            owner="map-block mutation source-order output",
        )


def test_map_block_mutation_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    output_schema = repo_path("schemas/map-script-engine-static.schema.json")
    fixture_schema = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    validate_json(map_script_engine_output, output_schema, owner="map-block mutation output")
    validate_json(fixture, fixture_schema, owner="map-block mutation fixture")

    missing = deepcopy(map_script_engine_output)
    del missing["mapBlockMutationCommandFacts"]["sourceSites"][0]["commands"][0][
        "operandValues"
    ][0]["value"]
    with pytest.raises(ValueError, match="value"):
        validate_json(missing, output_schema, owner="map-block mutation output missing field")

    renamed = deepcopy(map_script_engine_output)
    operand = renamed["mapBlockMutationCommandFacts"]["sourceSites"][0]["commands"][0][
        "operandValues"
    ][0]
    operand["label"] = operand.pop("sourceLabel")
    with pytest.raises(ValueError, match="sourceLabel"):
        validate_json(renamed, output_schema, owner="map-block mutation output renamed field")

    extra = deepcopy(map_script_engine_output)
    extra["mapBlockMutationCommandFacts"]["copyMapBlocksHelperFacts"]["innerLoop"][
        "extra"
    ] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(extra, output_schema, owner="map-block mutation output extra field")

    reordered_source = deepcopy(map_script_engine_output)
    source_order = reordered_source["mapBlockMutationCommandFacts"]["sourceSiteOrderKeys"]
    source_order[0], source_order[1] = source_order[1], source_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            reordered_source,
            output_schema,
            owner="map-block mutation output reordered source keys",
        )

    reordered_programs = deepcopy(map_script_engine_output)
    program_order = reordered_programs["mapBlockMutationCommandFacts"]["programTotalOrderKeys"]
    program_order[0], program_order[1] = program_order[1], program_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            reordered_programs,
            output_schema,
            owner="map-block mutation output reordered program keys",
        )

    out_of_boundary = deepcopy(map_script_engine_output)
    out_of_boundary["mapBlockMutationCommandFacts"]["copyMapBlocksHelperFacts"][
        "derivedAddressStride"
    ]["rowByteStride"] = 127
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            out_of_boundary, output_schema, owner="map-block mutation output boundary"
        )

    fixture_missing = deepcopy(fixture)
    del fixture_missing["expected"]["mapBlockMutationCommandFacts"]["handlers"][0][
        "sectionGuard"
    ]["cursorReadUseSites"][0]["handlerRegister"]
    with pytest.raises(ValueError, match="handlerRegister"):
        validate_json(
            fixture_missing, fixture_schema, owner="map-block mutation fixture missing field"
        )


def test_map_block_mutation_schema_exact_blocks_keep_large_corpora_compact() -> None:
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for path in schema_paths:
        schema = load_json(path)
        contract = schema["properties"].get("mapBlockMutationCommandFacts")
        if contract is None:
            contract = schema["properties"]["expected"]["properties"][
                "mapBlockMutationCommandFacts"
            ]
        exact = contract["allOf"][1]
        assert "sourceSites" not in exact["properties"]
        assert "programTotals" not in exact["properties"]
        facts = schema["definitions"]["mapBlockMutationCommandFacts"]
        assert facts["additionalProperties"] is False
        assert {"sourceSites", "programTotals"} <= set(facts["required"])


def test_entity_population_contract_matches_complete_golden_fixture(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["entityPopulationCommandFacts"]
    assert actual == fixture["expected"]["entityPopulationCommandFacts"]
    assert [
        (
            macro["name"],
            macro["opcode"],
            macro["encodedBytes"],
            macro["operandBytes"],
            macro["sourceCommandCount"],
        )
        for macro in actual["macros"]
    ] == [
        ("newEntity", 43, 8, 6, 18),
        ("loadMapEntities", 66, 6, 4, 69),
        ("reloadEntities", 68, 6, 4, 2),
        ("loadEntitiesFromMapSetup", 73, 8, 6, 7),
    ]
    assert actual["macros"][0]["sourceOperandAnnotations"] == [
        {
            "parameterOrdinal": 1,
            "sourceComment": "entity number",
            "streamOffset": 2,
            "widthBytes": 2,
        },
        {"parameterOrdinal": 2, "sourceComment": "X", "streamOffset": 4, "widthBytes": 1},
        {"parameterOrdinal": 3, "sourceComment": "Y", "streamOffset": 5, "widthBytes": 1},
        {"parameterOrdinal": 4, "sourceComment": "facing", "streamOffset": 6, "widthBytes": 1},
        {"parameterOrdinal": 5, "sourceComment": "mapsprite", "streamOffset": 7, "widthBytes": 1},
    ]
    assert [row["sourceComment"] for row in actual["macros"][3]["sourceOperandAnnotations"]] == [
        "",
        "",
        "",
    ]
    assert len(actual["sourceSites"]) == 78
    assert (
        actual["sourceSitesSha256"]
        == "BE26AD2D93D08929FC28BD451629EC8B275ED3832E24A4D732F033408A0785FD"
    )
    assert len(actual["programTotals"]) == 304
    assert (
        actual["programTotalsSha256"]
        == "45DAE48D41348AE403864F15E2FAD1C30E17637CD3037C03A41BC8105A124F65"
    )
    assert [
        (
            handler["macro"],
            handler["handler"],
            handler["address"],
            handler["opcode"],
            handler["sourceCommandCount"],
            handler["statementCount"],
        )
        for handler in actual["handlers"]
    ] == [
        ("newEntity", "csc2B_initializeNewEntity", 290360, 43, 18, 12),
        ("loadMapEntities", "csc42_loadMapEntities", 288394, 66, 69, 15),
        ("reloadEntities", "csc44_reloadEntities", 288456, 68, 2, 19),
        ("loadEntitiesFromMapSetup", "csc49_loadEntitiesFromMapSetup", 288600, 73, 7, 15),
    ]
    assert actual["handlers"][0]["sectionGuard"]["scriptCursorReadUseSites"] == [
        {
            "sourceRegister": "a6",
            "destinationRegister": "d0",
            "transferredByteCount": 2,
            "instruction": "move.w (a6)+,d0",
        },
        {
            "sourceRegister": "a6",
            "destinationRegister": "d1",
            "transferredByteCount": 1,
            "instruction": "move.b (a6)+,d1",
        },
        {
            "sourceRegister": "a6",
            "destinationRegister": "d2",
            "transferredByteCount": 1,
            "instruction": "move.b (a6)+,d2",
        },
        {
            "sourceRegister": "a6",
            "destinationRegister": "d3",
            "transferredByteCount": 1,
            "instruction": "move.b (a6)+,d3",
        },
        {
            "sourceRegister": "a6",
            "destinationRegister": "d4",
            "transferredByteCount": 1,
            "instruction": "move.b (a6)+,d4",
        },
    ]
    assert actual["handlers"][1]["sectionGuard"]["pointerReadUseSites"] == [
        {
            "sourceRegister": "a0",
            "destinationRegister": register,
            "transferredByteCount": 2,
            "instruction": f"move.w (a0)+,{register}",
        }
        for register in ("d1", "d2", "d3")
    ]
    assert actual["handlers"][2]["sectionGuard"]["sourceConstantUses"] == [
        {"symbol": "MAP_TILE_SIZE", "value": 384, "instruction": "divu.w #MAP_TILE_SIZE,d1"},
        {
            "symbol": "ENTITYDEF_OFFSET_Y",
            "value": 2,
            "instruction": "move.w ENTITYDEF_OFFSET_Y(a5),d2",
        },
        {"symbol": "MAP_TILE_SIZE", "value": 384, "instruction": "divu.w #MAP_TILE_SIZE,d2"},
        {
            "symbol": "ENTITYDEF_OFFSET_FACING",
            "value": 16,
            "instruction": "move.b ENTITYDEF_OFFSET_FACING(a5),d3",
        },
    ]
    assert actual["handlers"][3]["sectionGuard"]["directCallOrder"] == [
        "jsr (DisableDisplayAndInterrupts).w",
        "jsr GetMapSetupEntityList",
        "jsr j_InitializeMapEntities",
        "jsr (LoadEntityMapsprites).w",
        "jsr (EnableDisplayAndInterrupts).w",
    ]
    assert actual["callerBreakdown"]["instructionTargetTotals"] == {
        "DisableDisplayAndInterrupts": 2,
        "EnableDisplayAndInterrupts": 2,
        "GetEntityAddressFromCharacter": 1,
        "GetMapSetupEntityList": 1,
        "InitializeMapEntities": 2,
        "InitializeNewEntity": 1,
        "LoadEntityMapsprites": 2,
        "j_InitializeMapEntities": 1,
    }
    assert actual["callerBreakdown"]["effectiveTargetTotals"] == {
        "DisableDisplayAndInterrupts": 2,
        "EnableDisplayAndInterrupts": 2,
        "GetEntityAddressFromCharacter": 1,
        "GetMapSetupEntityList": 1,
        "InitializeMapEntities": 3,
        "InitializeNewEntity": 1,
        "LoadEntityMapsprites": 2,
    }
    assert actual["callerBreakdown"]["internalEffectiveTargetTotals"] == {
        target: 0 for target in actual["callerBreakdown"]["effectiveTargetTotals"]
    }
    assert (
        actual["callerBreakdown"]["externalEffectiveTargetTotals"]
        == actual["callerBreakdown"]["effectiveTargetTotals"]
    )
    assert actual["callerBreakdown"]["targetResolutions"][-1] == {
        "instructionTarget": "j_InitializeMapEntities",
        "effectiveTarget": "InitializeMapEntities",
        "aliasSourcePath": "code/common/tech/jumpinterfaces/s07_jumpinterface.asm",
        "effectiveTargetScope": "external",
    }
    assert actual["sourceIdentityJoins"]["entityActionInitializer"] == {
        "sourcePath": "data/scripting/entity/eas_actions.asm",
        "sourceSha256": "8C4312D69370D882C32A61276D94D744C3252FD6C32EB9351932F17EE39178F0",
        "symbol": "eas_Init",
        "address": 286926,
        "relatedContractId": "sf2-entity-action-scripts-static-v1",
    }
    assert actual["runtimeQuestions"] == ["entity-population-reload/runtime-effects-matrix"]


def test_entity_population_guards_reject_use_site_and_call_order_mutations_before_fixture(
    monkeypatch,
) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_use_site(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc44_reloadEntities":
            return [
                statement.replace("divu.w #MAP_TILE_SIZE,d2", "divs.w #MAP_TILE_SIZE,d2")
                for statement in statements
            ]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_use_site)
    with pytest.raises(ValueError, match="MAP_TILE_SIZE"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_entity_population_alias_target_mutation_fails_before_fixture(monkeypatch) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_alias_target(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc49_loadEntitiesFromMapSetup":
            return [
                statement.replace("jsr j_InitializeMapEntities", "jsr InitializeMapEntities")
                for statement in statements
            ]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_alias_target)
    with pytest.raises(ValueError, match="j_InitializeMapEntities"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_entity_population_handler_order_fails_before_fixture(monkeypatch) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_call_order(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc42_loadMapEntities":
            statements[3], statements[4] = statements[4], statements[3]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_call_order)
    with pytest.raises(ValueError, match="csc42_loadMapEntities statement is missing"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_entity_population_corpus_order_schema_rejects_source_reordering(monkeypatch) -> None:
    original_program_corpus = map_script_engine._program_corpus

    def reordered_entity_population_programs(*args, **kwargs):
        corpus = original_program_corpus(*args, **kwargs)
        matching_indexes = [
            index
            for index, program in enumerate(corpus["programs"])
            if any(
                command["macro"] in map_script_engine.ENTITY_POPULATION_MACRO_NAMES
                for command in program["commands"]
            )
        ]
        first, second = matching_indexes[:2]
        corpus["programs"][first], corpus["programs"][second] = (
            corpus["programs"][second],
            corpus["programs"][first],
        )
        return corpus

    monkeypatch.setattr(map_script_engine, "_program_corpus", reordered_entity_population_programs)
    output = build_map_script_engine_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            output,
            repo_path("schemas/map-script-engine-static.schema.json"),
            owner="entity-population source-order output",
        )


def test_entity_population_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    output_schema = repo_path("schemas/map-script-engine-static.schema.json")
    fixture_schema = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    validate_json(map_script_engine_output, output_schema, owner="entity-population output")
    validate_json(fixture, fixture_schema, owner="entity-population fixture")

    missing = deepcopy(map_script_engine_output)
    del missing["entityPopulationCommandFacts"]["handlers"][0]["sectionGuard"][
        "scriptCursorReadUseSites"
    ][0]["destinationRegister"]
    with pytest.raises(ValueError, match="destinationRegister"):
        validate_json(missing, output_schema, owner="entity-population output missing field")

    renamed = deepcopy(map_script_engine_output)
    annotation = renamed["entityPopulationCommandFacts"]["macros"][0]["sourceOperandAnnotations"][0]
    annotation["label"] = annotation.pop("sourceComment")
    with pytest.raises(ValueError, match="sourceComment"):
        validate_json(renamed, output_schema, owner="entity-population output renamed field")

    extra = deepcopy(map_script_engine_output)
    extra["entityPopulationCommandFacts"]["sourceIdentityJoins"]["calleeOwners"][0]["extra"] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(extra, output_schema, owner="entity-population output extra field")

    reordered_source = deepcopy(map_script_engine_output)
    source_order = reordered_source["entityPopulationCommandFacts"]["sourceSiteOrderKeys"]
    source_order[0], source_order[1] = source_order[1], source_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            reordered_source,
            output_schema,
            owner="entity-population output reordered source keys",
        )

    reordered_programs = deepcopy(map_script_engine_output)
    program_order = reordered_programs["entityPopulationCommandFacts"]["programTotalOrderKeys"]
    program_order[0], program_order[1] = program_order[1], program_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            reordered_programs,
            output_schema,
            owner="entity-population output reordered program keys",
        )

    out_of_boundary = deepcopy(map_script_engine_output)
    out_of_boundary["entityPopulationCommandFacts"]["handlers"][2]["sectionGuard"][
        "sourceConstantUses"
    ][0]["value"] = 383
    with pytest.raises(ValueError, match="was expected"):
        validate_json(out_of_boundary, output_schema, owner="entity-population output boundary")

    fixture_missing = deepcopy(fixture)
    del fixture_missing["expected"]["entityPopulationCommandFacts"]["handlers"][1]["sectionGuard"][
        "vintControlRecords"
    ][0]["operationValue"]
    with pytest.raises(ValueError, match="operationValue"):
        validate_json(
            fixture_missing, fixture_schema, owner="entity-population fixture missing field"
        )

    fixture_renamed = deepcopy(fixture)
    annotation = fixture_renamed["expected"]["entityPopulationCommandFacts"]["macros"][0][
        "sourceOperandAnnotations"
    ][0]
    annotation["label"] = annotation.pop("sourceComment")
    with pytest.raises(ValueError, match="sourceComment"):
        validate_json(
            fixture_renamed, fixture_schema, owner="entity-population fixture renamed field"
        )

    fixture_extra = deepcopy(fixture)
    fixture_extra["expected"]["entityPopulationCommandFacts"]["sourceIdentityJoins"][
        "calleeOwners"
    ][0]["extra"] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(fixture_extra, fixture_schema, owner="entity-population fixture extra field")

    fixture_reordered = deepcopy(fixture)
    source_order = fixture_reordered["expected"]["entityPopulationCommandFacts"][
        "sourceSiteOrderKeys"
    ]
    source_order[0], source_order[1] = source_order[1], source_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            fixture_reordered,
            fixture_schema,
            owner="entity-population fixture reordered source keys",
        )

    fixture_out_of_boundary = deepcopy(fixture)
    fixture_out_of_boundary["expected"]["entityPopulationCommandFacts"]["handlers"][2][
        "sectionGuard"
    ]["sourceConstantUses"][0]["value"] = 383
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            fixture_out_of_boundary,
            fixture_schema,
            owner="entity-population fixture boundary",
        )


def test_entity_population_schema_exact_blocks_keep_large_corpora_compact() -> None:
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for path in schema_paths:
        schema = load_json(path)
        contract = schema["properties"].get("entityPopulationCommandFacts")
        if contract is None:
            contract = schema["properties"]["expected"]["properties"][
                "entityPopulationCommandFacts"
            ]
        exact = contract["allOf"][1]
        assert "sourceSites" not in exact["properties"]
        assert "programTotals" not in exact["properties"]
        facts = schema["definitions"]["entityPopulationCommandFacts"]
        assert facts["additionalProperties"] is False
        assert {"sourceSites", "programTotals"} <= set(facts["required"])


def test_entity_clone_contract_matches_complete_golden_fixture(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["entityCloneCommandFacts"]
    expected = fixture["expected"]["entityCloneCommandFacts"]
    assert {key: actual[key] for key in expected} == expected
    assert actual["macros"] == [
        {
            "name": "cloneEntity",
            "opcode": 37,
            "encodedBytes": 6,
            "operandBytes": 4,
            "operandLayout": [
                {
                    "streamOffset": 2,
                    "widthBytes": 2,
                    "expression": "\\1",
                    "parameterOrdinals": [1],
                    "encoding": "direct",
                },
                {
                    "streamOffset": 4,
                    "widthBytes": 2,
                    "expression": "\\2",
                    "parameterOrdinals": [2],
                    "encoding": "direct",
                },
            ],
            "parameterOrdinals": [1, 2],
            "handler": "csc25_cloneEntity",
            "sourceOperandAnnotations": [
                {
                    "parameterOrdinal": 1,
                    "sourceComment": "copied entity",
                    "streamOffset": 2,
                    "widthBytes": 2,
                },
                {
                    "parameterOrdinal": 2,
                    "sourceComment": "entity clone",
                    "streamOffset": 4,
                    "widthBytes": 2,
                },
            ],
            "sourceCommandCount": 9,
        }
    ]
    assert [
        (
            site["programId"],
            command["commandIndex"],
            command["sourceLine"],
            command["arguments"],
            [value["resolvedValue"] for value in command["operandValues"]],
            command["sourceOrderKey"],
        )
        for site in actual["sourceSites"]
        for command in site["commands"]
    ] == [
        ("bbcs_16", 7, 11, ["129", "130"], [129, 130], "bbcs_16:7:cloneEntity"),
        ("bbcs_16", 8, 12, ["131", "132"], [131, 132], "bbcs_16:8:cloneEntity"),
        ("bbcs_16", 9, 13, ["131", "133"], [131, 133], "bbcs_16:9:cloneEntity"),
        ("bbcs_16", 10, 14, ["131", "134"], [131, 134], "bbcs_16:10:cloneEntity"),
        ("bbcs_16", 11, 15, ["131", "135"], [131, 135], "bbcs_16:11:cloneEntity"),
        ("bbcs_16", 12, 16, ["131", "136"], [131, 136], "bbcs_16:12:cloneEntity"),
        ("bbcs_16", 13, 17, ["131", "137"], [131, 137], "bbcs_16:13:cloneEntity"),
        ("bbcs_16", 14, 18, ["131", "138"], [131, 138], "bbcs_16:14:cloneEntity"),
        ("IntroCutscene2", 4, 8, ["132", "131"], [132, 131], "IntroCutscene2:4:cloneEntity"),
    ]
    assert actual["sourceSitesSha256"] == (
        "867E601D639D063120D3A3A5C7B5CE52664A59A1A6D2CC397C8861A896F042A2"
    )
    assert len(actual["programTotals"]) == 304
    assert actual["programTotalsSha256"] == (
        "36F45DF30945F8AA1883D1982702DE9A7290D4C0E797F52923C90471E85ECE70"
    )
    assert [
        (row["programId"], row["commandCount"], row["macroCounts"])
        for row in actual["programTotals"]
        if row["commandCount"]
    ] == [
        ("bbcs_16", 8, {"cloneEntity": 8}),
        ("IntroCutscene2", 1, {"cloneEntity": 1}),
    ]
    assert all(
        row["macroCounts"] == {"cloneEntity": row["commandCount"]}
        for row in actual["programTotals"]
    )
    assert actual["handlers"] == [
        {
            "macro": "cloneEntity",
            "handler": "csc25_cloneEntity",
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "address": 289882,
            "opcode": 37,
            "sourceCommandCount": 9,
            "operandAnnotations": actual["macros"][0]["sourceOperandAnnotations"],
            "statementCount": 7,
            "sectionGuard": {
                "orderedInstructions": [
                    "move.w (a6)+,d0",
                    "bsr.w GetEntityAddressFromCharacter",
                    "move.b ENTITYDEF_OFFSET_ENTNUM(a5),d1",
                    "move.w (a6)+,d0",
                    "bsr.w GetEntityAddressFromCharacter",
                    "move.b d1,ENTITYDEF_OFFSET_ENTNUM(a5)",
                    "rts",
                ],
                "scriptCursorReadUseSites": [
                    {
                        "sourceRegister": "a6",
                        "destinationRegister": "d0",
                        "transferredByteCount": 2,
                        "cursorAdvanceByteCount": 2,
                        "instruction": "move.w (a6)+,d0",
                    },
                    {
                        "sourceRegister": "a6",
                        "destinationRegister": "d0",
                        "transferredByteCount": 2,
                        "cursorAdvanceByteCount": 2,
                        "instruction": "move.w (a6)+,d0",
                    },
                ],
                "entityLookupSequence": [
                    {
                        "role": "source",
                        "cursorReadInstruction": "move.w (a6)+,d0",
                        "lookupCallInstruction": "bsr.w GetEntityAddressFromCharacter",
                        "resultAddressRegister": "a5",
                    },
                    {
                        "role": "destination",
                        "cursorReadInstruction": "move.w (a6)+,d0",
                        "lookupCallInstruction": "bsr.w GetEntityAddressFromCharacter",
                        "resultAddressRegister": "a5",
                    },
                ],
                "entnumFieldTransfer": {
                    "sourceFieldRead": {
                        "baseRegister": "a5",
                        "offsetSymbol": "ENTITYDEF_OFFSET_ENTNUM",
                        "offsetValue": 18,
                        "destinationRegister": "d1",
                        "transferredByteCount": 1,
                        "instruction": "move.b ENTITYDEF_OFFSET_ENTNUM(a5),d1",
                    },
                    "destinationFieldWrite": {
                        "sourceRegister": "d1",
                        "baseRegister": "a5",
                        "offsetSymbol": "ENTITYDEF_OFFSET_ENTNUM",
                        "offsetValue": 18,
                        "transferredByteCount": 1,
                        "instruction": "move.b d1,ENTITYDEF_OFFSET_ENTNUM(a5)",
                    },
                    "derivedTransferByteCount": 1,
                },
                "loopRecords": [],
                "directCallOrder": [
                    "bsr.w GetEntityAddressFromCharacter",
                    "bsr.w GetEntityAddressFromCharacter",
                ],
                "returnInstruction": "rts",
            },
            "directCalls": [
                {"opcode": "bsr", "instructionTarget": "GetEntityAddressFromCharacter"},
                {"opcode": "bsr", "instructionTarget": "GetEntityAddressFromCharacter"},
            ],
        }
    ]
    assert actual["callerBreakdown"] == {
        "callerHandlers": [
            {
                "handler": "csc25_cloneEntity",
                "instructionTargetSiteCounts": {"GetEntityAddressFromCharacter": 2},
                "effectiveTargetSiteCounts": {"GetEntityAddressFromCharacter": 2},
            }
        ],
        "targetResolutions": [
            {
                "instructionTarget": "GetEntityAddressFromCharacter",
                "effectiveTarget": "GetEntityAddressFromCharacter",
                "aliasSourcePath": None,
                "effectiveTargetScope": "external",
            }
        ],
        "instructionTargetTotals": {"GetEntityAddressFromCharacter": 2},
        "effectiveTargetTotals": {"GetEntityAddressFromCharacter": 2},
        "internalInstructionTargetTotals": {"GetEntityAddressFromCharacter": 0},
        "externalInstructionTargetTotals": {"GetEntityAddressFromCharacter": 2},
        "internalEffectiveTargetTotals": {"GetEntityAddressFromCharacter": 0},
        "externalEffectiveTargetTotals": {"GetEntityAddressFromCharacter": 2},
    }
    assert actual["sourceIdentityJoins"] == {
        "handlerSource": {
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "sourceSha256": "17F52906D05B933F318D509204460743591BA9F802D21B121D37217F156F83BF",
            "symbol": "csc25_cloneEntity",
        },
        "entityAddressLookupOwner": {
            "sourceFactPath": "entityPopulationCommandFacts.sourceIdentityJoins.calleeOwners",
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "sourceSha256": "17F52906D05B933F318D509204460743591BA9F802D21B121D37217F156F83BF",
            "symbol": "GetEntityAddressFromCharacter",
        },
    }
    assert actual["runtimeQuestions"] == ["map-script-entity-clone/runtime-effects-matrix"]


def test_entity_clone_use_site_and_call_order_guards_fail_before_fixture(monkeypatch) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_field_width(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc25_cloneEntity":
            return [
                statement.replace(
                    "move.b d1,ENTITYDEF_OFFSET_ENTNUM(a5)",
                    "move.w d1,ENTITYDEF_OFFSET_ENTNUM(a5)",
                )
                for statement in statements
            ]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_field_width)
    with pytest.raises(ValueError, match="csc25_cloneEntity statement is missing"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_entity_clone_order_guard_rejects_reordered_lookup_before_fixture(monkeypatch) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def reordered_lookup(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc25_cloneEntity":
            statements[2], statements[3] = statements[3], statements[2]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", reordered_lookup)
    with pytest.raises(ValueError, match="csc25_cloneEntity statement is missing"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_entity_clone_use_site_parsers_accept_suffixes_and_reject_near_misses() -> None:
    assert [_entity_clone_cursor_read_use_site(instruction) for instruction in (
        "move.b (a6)+,d0",
        "move.w (a6)+,d1",
        "move.l (a6)+,d7",
    )] == [
        {
            "sourceRegister": "a6",
            "destinationRegister": register,
            "transferredByteCount": width,
            "cursorAdvanceByteCount": width,
            "instruction": instruction,
        }
        for instruction, register, width in (
            ("move.b (a6)+,d0", "d0", 1),
            ("move.w (a6)+,d1", "d1", 2),
            ("move.l (a6)+,d7", "d7", 4),
        )
    ]
    equates = {"ENTITYDEF_OFFSET_ENTNUM": 18}
    assert _entity_clone_field_read_use_site(
        "move.b ENTITYDEF_OFFSET_ENTNUM(a5),d1", equates
    )["transferredByteCount"] == 1
    assert _entity_clone_field_read_use_site(
        "move.w ENTITYDEF_OFFSET_ENTNUM(a5),d1", equates
    )["transferredByteCount"] == 2
    assert _entity_clone_field_write_use_site(
        "move.l d1,ENTITYDEF_OFFSET_ENTNUM(a5)", equates
    )["transferredByteCount"] == 4
    for parser, instruction in (
        (_entity_clone_cursor_read_use_site, "move.w (a6)+,d0 ; comment"),
        (_entity_clone_cursor_read_use_site, "label: move.w (a6)+,d0"),
        (_entity_clone_cursor_read_use_site, "; move.w (a6)+,d0"),
        (_entity_clone_field_read_use_site, "move.b ENTITYDEF_OFFSET_ENTNUM(a4),d1"),
        (_entity_clone_field_write_use_site, "move.b d1,ENTITYDEF_OFFSET_ENTNUM(a5) ; comment"),
    ):
        with pytest.raises(ValueError, match="entity-clone"):
            if parser is _entity_clone_cursor_read_use_site:
                parser(instruction)
            else:
                parser(instruction, equates)


def test_entity_clone_section_guard_rejects_extra_statement_and_field_relationship_drift() -> None:
    statements = [
        "move.w (a6)+,d0",
        "bsr.w GetEntityAddressFromCharacter",
        "move.b ENTITYDEF_OFFSET_ENTNUM(a5),d1",
        "move.w (a6)+,d0",
        "bsr.w GetEntityAddressFromCharacter",
        "move.b d1,ENTITYDEF_OFFSET_ENTNUM(a5)",
        "rts",
    ]
    actual = _entity_clone_section_guard(statements, {"ENTITYDEF_OFFSET_ENTNUM": 18})
    assert actual["entnumFieldTransfer"]["derivedTransferByteCount"] == 1
    assert actual["loopRecords"] == []
    statements[5] = "move.b d1,ENTITYDEF_OFFSET_MAPSPRITE(a5)"
    with pytest.raises(ValueError, match="csc25_cloneEntity statement is missing"):
        _entity_clone_section_guard(statements, {"ENTITYDEF_OFFSET_ENTNUM": 18})
    statements[5] = "move.b d1,ENTITYDEF_OFFSET_ENTNUM(a5)"
    statements.insert(-1, "nop")
    with pytest.raises(ValueError, match="statement coverage drift"):
        _entity_clone_section_guard(statements, {"ENTITYDEF_OFFSET_ENTNUM": 18})


def test_entity_clone_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    output_schema = repo_path("schemas/map-script-engine-static.schema.json")
    fixture_schema = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    validate_json(map_script_engine_output, output_schema, owner="entity-clone output")
    validate_json(fixture, fixture_schema, owner="entity-clone fixture")

    missing = deepcopy(map_script_engine_output)
    del missing["entityCloneCommandFacts"]["handlers"][0]["sectionGuard"][
        "entnumFieldTransfer"
    ]["sourceFieldRead"]["destinationRegister"]
    with pytest.raises(ValueError, match="destinationRegister"):
        validate_json(missing, output_schema, owner="entity-clone output missing field")

    renamed = deepcopy(map_script_engine_output)
    read = renamed["entityCloneCommandFacts"]["handlers"][0]["sectionGuard"][
        "entnumFieldTransfer"
    ]["sourceFieldRead"]
    read["register"] = read.pop("destinationRegister")
    with pytest.raises(ValueError, match="destinationRegister"):
        validate_json(renamed, output_schema, owner="entity-clone output renamed field")

    extra = deepcopy(map_script_engine_output)
    extra["entityCloneCommandFacts"]["handlers"][0]["sectionGuard"][
        "entityLookupSequence"
    ][0]["extra"] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(extra, output_schema, owner="entity-clone output extra field")

    reordered = deepcopy(map_script_engine_output)
    source_order = reordered["entityCloneCommandFacts"]["sourceSiteOrderKeys"]
    source_order[0], source_order[1] = source_order[1], source_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(reordered, output_schema, owner="entity-clone output reordered keys")

    out_of_boundary = deepcopy(map_script_engine_output)
    out_of_boundary["entityCloneCommandFacts"]["handlers"][0]["sectionGuard"][
        "entnumFieldTransfer"
    ]["derivedTransferByteCount"] = 2
    with pytest.raises(ValueError, match="was expected"):
        validate_json(out_of_boundary, output_schema, owner="entity-clone output boundary")

    fixture_missing = deepcopy(fixture)
    del fixture_missing["expected"]["entityCloneCommandFacts"]["handlers"][0][
        "sectionGuard"
    ]["entnumFieldTransfer"]["sourceFieldRead"]["destinationRegister"]
    with pytest.raises(ValueError, match="destinationRegister"):
        validate_json(fixture_missing, fixture_schema, owner="entity-clone fixture missing")

    fixture_extra = deepcopy(fixture)
    fixture_extra["expected"]["entityCloneCommandFacts"]["sourceIdentityJoins"][
        "handlerSource"
    ]["extra"] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(fixture_extra, fixture_schema, owner="entity-clone fixture extra")

    fixture_reordered = deepcopy(fixture)
    source_order = fixture_reordered["expected"]["entityCloneCommandFacts"][
        "sourceSiteOrderKeys"
    ]
    source_order[0], source_order[1] = source_order[1], source_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(fixture_reordered, fixture_schema, owner="entity-clone fixture order")

    fixture_out_of_boundary = deepcopy(fixture)
    fixture_out_of_boundary["expected"]["entityCloneCommandFacts"]["handlers"][0][
        "sectionGuard"
    ]["entnumFieldTransfer"]["derivedTransferByteCount"] = 2
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            fixture_out_of_boundary,
            fixture_schema,
            owner="entity-clone fixture boundary",
        )


def test_entity_clone_schema_exact_blocks_keep_raw_corpora_out_of_fixture() -> None:
    output_schema = load_json(repo_path("schemas/map-script-engine-static.schema.json"))
    fixture_schema = load_json(repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"))
    output_contract = output_schema["properties"]["entityCloneCommandFacts"]
    fixture_contract = fixture_schema["properties"]["expected"]["properties"][
        "entityCloneCommandFacts"
    ]
    assert {"sourceSites", "programTotals"} <= set(
        output_schema["definitions"]["entityCloneCommandFacts"]["required"]
    )
    assert {"sourceSites", "programTotals"}.isdisjoint(
        fixture_schema["definitions"]["entityCloneCommandFactsFixture"]["required"]
    )
    for contract in (output_contract, fixture_contract):
        exact = contract["allOf"][1]
        assert {"sourceSites", "programTotals"}.isdisjoint(exact["properties"])


def test_map_lifecycle_contract_matches_complete_golden_fixture(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["mapLifecycleCommandFacts"]
    assert actual == fixture["expected"]["mapLifecycleCommandFacts"]
    assert [
        (
            macro["name"],
            macro["opcode"],
            macro["encodedBytes"],
            macro["operandBytes"],
            macro["sourceCommandCount"],
        )
        for macro in actual["macros"]
    ] == [
        ("resetMap", 54, 2, 0, 7),
        ("loadMapFadeIn", 55, 8, 6, 60),
        ("reloadMap", 70, 6, 4, 24),
        ("mapLoad", 72, 8, 6, 17),
    ]
    assert actual["macros"][1]["sourceOperandAnnotations"] == [
        {"parameterOrdinal": 1, "sourceComment": "map", "streamOffset": 2, "widthBytes": 2},
        {
            "parameterOrdinal": 2,
            "sourceComment": "camera X",
            "streamOffset": 4,
            "widthBytes": 2,
        },
        {
            "parameterOrdinal": 3,
            "sourceComment": "camera Y",
            "streamOffset": 6,
            "widthBytes": 2,
        },
    ]
    assert actual["macros"][2]["sourceOperandAnnotations"] == [
        {
            "parameterOrdinal": 1,
            "sourceComment": "camera X",
            "streamOffset": 2,
            "widthBytes": 2,
        },
        {
            "parameterOrdinal": 2,
            "sourceComment": "camera Y",
            "streamOffset": 4,
            "widthBytes": 2,
        },
    ]
    assert len(actual["sourceSites"]) == 81
    assert (
        actual["sourceSitesSha256"]
        == "4F07DC2BD06A9E326A61CF43867FCBD2027BC35E76FAD9EEE09B622E11DC13A8"
    )
    assert len(actual["programTotals"]) == 304
    assert (
        actual["programTotalsSha256"]
        == "E553A857B356B1B334DE3C68DAD17D966C46C6788EADB62EAFD6A966D6EBEB84"
    )
    assert [
        (
            handler["macro"],
            handler["handler"],
            handler["address"],
            handler["opcode"],
            handler["sourceCommandCount"],
            handler["statementCount"],
        )
        for handler in actual["handlers"]
    ] == [
        ("resetMap", "csc36_resetMap", 288142, 54, 7, 4),
        ("loadMapFadeIn", "csc37_loadMapAndFadeIn", 288154, 55, 60, 5),
        ("reloadMap", "csc46_reloadMap", 288520, 70, 24, 22),
        ("mapLoad", "csc48_loadMap", 288182, 72, 17, 27),
    ]
    assert actual["handlers"][1]["continuation"] == {
        "handler": "csc48_loadMap",
        "address": 288182,
        "sectionGuard": actual["handlers"][3]["sectionGuard"],
        "directCalls": actual["handlers"][3]["directCalls"],
    }
    assert actual["handlers"][2]["sectionGuard"]["sourceD1SelectorUseSite"] == {
        "literalValue": -1,
        "instruction": "moveq #-1,d1",
    }
    assert actual["handlers"][3]["sectionGuard"]["mapProbeUseSite"] == {
        "sourceRegister": "a6",
        "destinationRegister": "d1",
        "transferredByteCount": 2,
        "cursorAdvanceByteCount": 0,
        "instruction": "move.w (a6),d1",
    }
    assert actual["handlers"][3]["sectionGuard"]["branchRecords"] == [
        {
            "testInstruction": "tst.b ((FADING_SETTING-$1000000)).w",
            "branchInstruction": "bne.s loc_465C4",
            "fallthroughInstruction": "trap #VINT_FUNCTIONS",
            "branchTarget": {
                "targetLabel": "loc_465C4",
                "targetInstruction": "jsr (WaitForVInt).w",
                "targetStatementIndex": 4,
            },
        }
    ]
    assert actual["handlers"][3]["sectionGuard"]["operandPackUseSites"] == {
        "parameterOrdinals": [2, 3],
        "sourceComments": ["camera X", "camera Y"],
        "shiftUseSite": {
            "symbol": "BYTE_SHIFT_COUNT",
            "value": 8,
            "instruction": "lsl.w #BYTE_SHIFT_COUNT,d0",
        },
        "maskUseSite": {
            "symbol": "BYTE_MASK",
            "value": 255,
            "instruction": "andi.w #BYTE_MASK,d2",
        },
        "mergeInstruction": "or.w d2,d0",
        "multiplierUseSite": {"value": 3, "instruction": "mulu.w #3,d0"},
    }
    assert actual["handlers"][3]["sectionGuard"]["directCallOrder"] == [
        "jsr (LoadMapTilesets).w",
        "jsr (WaitForVInt).w",
        "jsr (LoadMap).w",
        "jsr (EnableDisplayAndInterrupts).w",
        "jsr (WaitForVInt).w",
    ]
    assert actual["callerBreakdown"]["instructionTargetTotals"] == {
        "EnableDisplayAndInterrupts": 2,
        "LoadMap": 2,
        "LoadMapTilesets": 1,
        "ResetCurrentMap": 1,
        "WaitForVInt": 3,
    }
    assert actual["callerBreakdown"]["callerHandlers"] == [
        {
            "handler": "csc36_resetMap",
            "instructionTargetSiteCounts": {
                "EnableDisplayAndInterrupts": 0,
                "LoadMap": 0,
                "LoadMapTilesets": 0,
                "ResetCurrentMap": 1,
                "WaitForVInt": 0,
            },
            "effectiveTargetSiteCounts": {
                "EnableDisplayAndInterrupts": 0,
                "LoadMap": 0,
                "LoadMapTilesets": 0,
                "ResetCurrentMap": 1,
                "WaitForVInt": 0,
            },
        },
        {
            "handler": "csc37_loadMapAndFadeIn",
            "instructionTargetSiteCounts": {
                "EnableDisplayAndInterrupts": 0,
                "LoadMap": 0,
                "LoadMapTilesets": 0,
                "ResetCurrentMap": 0,
                "WaitForVInt": 0,
            },
            "effectiveTargetSiteCounts": {
                "EnableDisplayAndInterrupts": 0,
                "LoadMap": 0,
                "LoadMapTilesets": 0,
                "ResetCurrentMap": 0,
                "WaitForVInt": 0,
            },
        },
        {
            "handler": "csc46_reloadMap",
            "instructionTargetSiteCounts": {
                "EnableDisplayAndInterrupts": 1,
                "LoadMap": 1,
                "LoadMapTilesets": 0,
                "ResetCurrentMap": 0,
                "WaitForVInt": 1,
            },
            "effectiveTargetSiteCounts": {
                "EnableDisplayAndInterrupts": 1,
                "LoadMap": 1,
                "LoadMapTilesets": 0,
                "ResetCurrentMap": 0,
                "WaitForVInt": 1,
            },
        },
        {
            "handler": "csc48_loadMap",
            "instructionTargetSiteCounts": {
                "EnableDisplayAndInterrupts": 1,
                "LoadMap": 1,
                "LoadMapTilesets": 1,
                "ResetCurrentMap": 0,
                "WaitForVInt": 2,
            },
            "effectiveTargetSiteCounts": {
                "EnableDisplayAndInterrupts": 1,
                "LoadMap": 1,
                "LoadMapTilesets": 1,
                "ResetCurrentMap": 0,
                "WaitForVInt": 2,
            },
        },
    ]
    assert actual["callerBreakdown"]["targetResolutions"] == [
        {
            "instructionTarget": target,
            "effectiveTarget": target,
            "aliasSourcePath": None,
            "effectiveTargetScope": "external",
        }
        for target in (
            "EnableDisplayAndInterrupts",
            "LoadMap",
            "LoadMapTilesets",
            "ResetCurrentMap",
            "WaitForVInt",
        )
    ]
    assert actual["callerBreakdown"]["effectiveTargetTotals"] == actual[
        "callerBreakdown"
    ]["instructionTargetTotals"]
    assert actual["callerBreakdown"]["internalEffectiveTargetTotals"] == {
        target: 0 for target in actual["callerBreakdown"]["effectiveTargetTotals"]
    }
    assert actual["callerBreakdown"]["externalEffectiveTargetTotals"] == actual[
        "callerBreakdown"
    ]["effectiveTargetTotals"]
    assert actual["sourceIdentityJoins"]["handlerSource"] == {
        "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
        "sourceSha256": "17F52906D05B933F318D509204460743591BA9F802D21B121D37217F156F83BF",
        "symbols": [
            "csc36_resetMap",
            "csc37_loadMapAndFadeIn",
            "csc46_reloadMap",
            "csc48_loadMap",
        ],
    }
    assert actual["runtimeQuestions"] == [
        "map-lifecycle/layout-collision-pathfinding-effects",
        "map-lifecycle/entity-reload-player-placement",
        "map-lifecycle/presentation-fade-hardware-timing",
        "map-lifecycle/story-reachability-persistence",
    ]


def _canonical_digest(value: object) -> str:
    encoded = dumps(value, ensure_ascii=False, separators=(",", ":"), sort_keys=True)
    return sha256(encoded.encode("utf-8")).hexdigest()


def test_h3_handoffs_change_only_runtime_question_queues() -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    expected = deepcopy(fixture["expected"])
    assert expected.pop("runtimeQuestions") == [
        "caller-dependent-story-branch-reachability-and-persistence",
        "entity-camera-text-wait-and-transition-frame-timing",
        "palette-fade-and-vdp-visible-presentation",
        "force-state/roster-death-persistence-visible-outcomes",
        "force-state/active-party-ai-follower-runtime-matrix",
        "story-state/branch-prompt-persistence-matrix",
        "map-block-mutation/runtime-effects-matrix",
        "entity-population-reload/runtime-effects-matrix",
        "map-lifecycle/layout-collision-pathfinding-effects",
        "map-lifecycle/entity-reload-player-placement",
        "map-lifecycle/presentation-fade-hardware-timing",
        "map-lifecycle/story-reachability-persistence",
        "map-interaction-trigger/full-layout-collision-pathfinding-effects",
        "map-interaction-trigger/presentation-audio-timing-hardware-effects",
        "map-interaction-trigger/persistence-story-reachability",
        "map-script-camera-control/normal-story-reachability",
        "map-script-camera-control/vdp-player-visible-behavior",
        "map-script-entity-placement/normal-story-reachability",
        "map-script-entity-placement/full-animation-visibility-presentation",
        "map-script-entity-placement/collision-pathfinding-persistence",
        "map-script-entity-action-bridge/normal-story-reachability",
        "map-script-entity-action-bridge/full-action-motion-collision-effects",
        "map-script-entity-action-bridge/presentation-timing-persistence",
        "map-script-entity-lifecycle-presentation/normal-story-reachability",
        "map-script-entity-lifecycle-presentation/full-entity-state-callback-effects",
        "map-script-entity-lifecycle-presentation/player-visible-presentation-timing-collision-persistence",
        "map-script-entity-gesture-relationship-motion/normal-story-reachability",
        "map-script-entity-gesture-relationship-motion/full-entity-state-callback-effects",
        "map-script-entity-gesture-relationship-motion/player-visible-presentation-timing-collision-persistence",
        "map-script-screen-presentation/runtime-effects-matrix",
        "map-script-entity-presentation-fx/runtime-effects-matrix",
        "map-script-ui-command/runtime-effects-matrix",
        "map-script-entity-clone/runtime-effects-matrix",
    ]
    assert expected["mapLifecycleCommandFacts"].pop("runtimeQuestions") == [
        "map-lifecycle/layout-collision-pathfinding-effects",
        "map-lifecycle/entity-reload-player-placement",
        "map-lifecycle/presentation-fade-hardware-timing",
        "map-lifecycle/story-reachability-persistence",
    ]
    assert expected["entityPlacementCommandFacts"].pop("runtimeQuestions") == [
        "map-script-entity-placement/normal-story-reachability",
        "map-script-entity-placement/full-animation-visibility-presentation",
        "map-script-entity-placement/collision-pathfinding-persistence",
    ]
    assert expected["entityActionBridgeCommandFacts"].pop("runtimeQuestions") == [
        "map-script-entity-action-bridge/normal-story-reachability",
        "map-script-entity-action-bridge/full-action-motion-collision-effects",
        "map-script-entity-action-bridge/presentation-timing-persistence",
    ]
    assert expected["entityLifecyclePresentationCommandFacts"].pop("runtimeQuestions") == [
        "map-script-entity-lifecycle-presentation/normal-story-reachability",
        "map-script-entity-lifecycle-presentation/full-entity-state-callback-effects",
        "map-script-entity-lifecycle-presentation/player-visible-presentation-timing-collision-persistence",
    ]
    assert expected["entityGestureRelationshipMotionCommandFacts"].pop(
        "runtimeQuestions"
    ) == [
        "map-script-entity-gesture-relationship-motion/normal-story-reachability",
        "map-script-entity-gesture-relationship-motion/full-entity-state-callback-effects",
        "map-script-entity-gesture-relationship-motion/player-visible-presentation-timing-collision-persistence",
    ]
    # H3 queues are explicitly excluded so the canonical static digest remains stable.
    assert _canonical_digest(expected) == (
        "5b10957b26fb21f0bc5722fd5f010a3f561cecb00268e73fc37940f9568e1142"
    )

    output_schema = deepcopy(load_json(repo_path("schemas/map-script-engine-static.schema.json")))
    del output_schema["properties"]["runtimeQuestions"]
    del output_schema["properties"]["mapLifecycleCommandFacts"]["allOf"][1]["properties"][
        "runtimeQuestions"
    ]
    del output_schema["properties"]["entityPlacementCommandFacts"]["allOf"][1]["properties"][
        "runtimeQuestions"
    ]
    del output_schema["properties"]["entityActionBridgeCommandFacts"]["allOf"][1][
        "properties"
    ]["runtimeQuestions"]
    del output_schema["properties"]["entityLifecyclePresentationCommandFacts"]["allOf"][1][
        "properties"
    ]["runtimeQuestions"]
    del output_schema["properties"]["entityGestureRelationshipMotionCommandFacts"]["allOf"][1][
        "properties"
    ]["runtimeQuestions"]
    del output_schema["definitions"]["entityGestureRelationshipMotionCommandFacts"][
        "properties"
    ]["runtimeQuestions"]
    output_schema["definitions"]["entityGestureRelationshipMotionCommandFacts"][
        "required"
    ].remove("runtimeQuestions")
    assert _canonical_digest(output_schema) == (
        "0656244f672bdbaceae9cdccf65e2431cde64335f1f2b9b63a297760ae4a7ad7"
    )

    fixture_schema = deepcopy(
        load_json(repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"))
    )
    del fixture_schema["properties"]["expected"]["properties"]["runtimeQuestions"]
    del fixture_schema["properties"]["expected"]["properties"]["mapLifecycleCommandFacts"][
        "allOf"
    ][1]["properties"]["runtimeQuestions"]
    del fixture_schema["properties"]["expected"]["properties"]["entityPlacementCommandFacts"][
        "allOf"
    ][1]["properties"]["runtimeQuestions"]
    del fixture_schema["properties"]["expected"]["properties"][
        "entityActionBridgeCommandFacts"
    ]["allOf"][1]["const"]["runtimeQuestions"]
    del fixture_schema["properties"]["expected"]["properties"][
        "entityLifecyclePresentationCommandFacts"
    ]["allOf"][1]["const"]["runtimeQuestions"]
    del fixture_schema["properties"]["expected"]["properties"][
        "entityGestureRelationshipMotionCommandFacts"
    ]["allOf"][1]["const"]["runtimeQuestions"]
    del fixture_schema["definitions"]["entityGestureRelationshipMotionFixtureCommandFacts"][
        "properties"
    ]["runtimeQuestions"]
    fixture_schema["definitions"]["entityGestureRelationshipMotionFixtureCommandFacts"][
        "required"
    ].remove("runtimeQuestions")
    assert _canonical_digest(fixture_schema) == (
        "583c8b51021b070f50739e246c758b7072c5e847e99e1755d20ebde11b2d6739"
    )


def test_map_lifecycle_opcode_operand_order_and_polarity_guards_fail_before_fixture(
    monkeypatch,
) -> None:
    original_macros = map_script_engine._map_macro_contracts

    def changed_opcode(disasm):
        macros = original_macros(disasm)
        macros["resetMap"]["opcode"] = 56
        return macros

    monkeypatch.setattr(map_script_engine, "_map_macro_contracts", changed_opcode)
    with pytest.raises(ValueError, match="dispatcher target drift"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_map_lifecycle_operand_use_site_guard_fails_before_fixture(monkeypatch) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_operand(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc46_reloadMap":
            return [
                statement.replace("mulu.w #3,d0", "mulu.w #4,d0")
                for statement in statements
            ]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_operand)
    monkeypatch.setattr(map_script_engine, "_transition_command_facts", lambda *args, **kwargs: {})
    with pytest.raises(ValueError, match="shared operand-pack use-site drift"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_map_lifecycle_call_order_and_branch_polarity_guards_fail_before_fixture(
    monkeypatch,
) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_branch(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc48_loadMap":
            return [
                statement.replace("bne.s loc_465C4", "beq.s loc_465C4")
                for statement in statements
            ]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_branch)
    with pytest.raises(ValueError, match="csc48_loadMap statement is missing"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_map_lifecycle_branch_target_and_label_guards_fail_before_fixture(
    monkeypatch,
) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_branch_target(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc48_loadMap":
            return [
                statement.replace("bne.s loc_465C4", "bne.s loc_465C5")
                for statement in statements
            ]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_branch_target)
    with pytest.raises(ValueError, match="branch target label drift"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_map_lifecycle_branch_target_label_owner_guard_fails_before_fixture(
    monkeypatch,
) -> None:
    original_source = map_script_engine._map_lifecycle_named_section_source

    def changed_target_label(disasm, handler):
        source = original_source(disasm, handler)
        if handler["name"] == "csc48_loadMap":
            return source.replace("loc_465C4:", "loc_465C5:")
        return source

    monkeypatch.setattr(
        map_script_engine, "_map_lifecycle_named_section_source", changed_target_label
    )
    with pytest.raises(ValueError, match="branch target label is missing"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_map_lifecycle_call_order_guard_fails_before_fixture(monkeypatch) -> None:
    original_statements = map_script_engine._stable_handler_statements

    def changed_call_order(disasm, handler):
        statements = original_statements(disasm, handler)
        if handler["name"] == "csc48_loadMap":
            statements[19], statements[21] = statements[21], statements[19]
        return statements

    monkeypatch.setattr(map_script_engine, "_stable_handler_statements", changed_call_order)
    with pytest.raises(ValueError, match="csc48_loadMap statement is missing"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )


def test_map_lifecycle_corpus_order_schema_rejects_source_reordering(monkeypatch) -> None:
    original_program_corpus = map_script_engine._program_corpus

    def reordered_lifecycle_programs(*args, **kwargs):
        corpus = original_program_corpus(*args, **kwargs)
        matching_indexes = [
            index
            for index, program in enumerate(corpus["programs"])
            if any(
                command["macro"] in map_script_engine.MAP_LIFECYCLE_MACRO_NAMES
                for command in program["commands"]
            )
        ]
        first, second = matching_indexes[:2]
        corpus["programs"][first], corpus["programs"][second] = (
            corpus["programs"][second],
            corpus["programs"][first],
        )
        return corpus

    monkeypatch.setattr(map_script_engine, "_program_corpus", reordered_lifecycle_programs)
    output = build_map_script_engine_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            output,
            repo_path("schemas/map-script-engine-static.schema.json"),
            owner="map-lifecycle source-order output",
        )


def test_map_lifecycle_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    output_schema = repo_path("schemas/map-script-engine-static.schema.json")
    fixture_schema = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    validate_json(map_script_engine_output, output_schema, owner="map-lifecycle output")
    validate_json(fixture, fixture_schema, owner="map-lifecycle fixture")

    missing = deepcopy(map_script_engine_output)
    del missing["mapLifecycleCommandFacts"]["handlers"][3]["sectionGuard"][
        "mapProbeUseSite"
    ]["destinationRegister"]
    with pytest.raises(ValueError, match="destinationRegister"):
        validate_json(missing, output_schema, owner="map-lifecycle output missing field")

    renamed = deepcopy(map_script_engine_output)
    annotation = renamed["mapLifecycleCommandFacts"]["macros"][1][
        "sourceOperandAnnotations"
    ][0]
    annotation["label"] = annotation.pop("sourceComment")
    with pytest.raises(ValueError, match="sourceComment"):
        validate_json(renamed, output_schema, owner="map-lifecycle output renamed field")

    extra = deepcopy(map_script_engine_output)
    extra["mapLifecycleCommandFacts"]["sourceIdentityJoins"]["calleeOwners"][0][
        "extra"
    ] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(extra, output_schema, owner="map-lifecycle output extra field")

    reordered_source = deepcopy(map_script_engine_output)
    source_order = reordered_source["mapLifecycleCommandFacts"]["sourceSiteOrderKeys"]
    source_order[0], source_order[1] = source_order[1], source_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            reordered_source,
            output_schema,
            owner="map-lifecycle output reordered source keys",
        )

    reordered_programs = deepcopy(map_script_engine_output)
    program_order = reordered_programs["mapLifecycleCommandFacts"]["programTotalOrderKeys"]
    program_order[0], program_order[1] = program_order[1], program_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            reordered_programs,
            output_schema,
            owner="map-lifecycle output reordered program keys",
        )

    out_of_boundary = deepcopy(map_script_engine_output)
    out_of_boundary["mapLifecycleCommandFacts"]["handlers"][3]["sectionGuard"][
        "operandPackUseSites"
    ]["multiplierUseSite"]["value"] = 4
    with pytest.raises(ValueError, match="was expected"):
        validate_json(out_of_boundary, output_schema, owner="map-lifecycle output boundary")

    fixture_missing = deepcopy(fixture)
    del fixture_missing["expected"]["mapLifecycleCommandFacts"]["handlers"][3][
        "sectionGuard"
    ]["mapProbeUseSite"]["destinationRegister"]
    with pytest.raises(ValueError, match="destinationRegister"):
        validate_json(fixture_missing, fixture_schema, owner="map-lifecycle fixture missing field")

    fixture_renamed = deepcopy(fixture)
    annotation = fixture_renamed["expected"]["mapLifecycleCommandFacts"]["macros"][1][
        "sourceOperandAnnotations"
    ][0]
    annotation["label"] = annotation.pop("sourceComment")
    with pytest.raises(ValueError, match="sourceComment"):
        validate_json(fixture_renamed, fixture_schema, owner="map-lifecycle fixture renamed field")

    fixture_extra = deepcopy(fixture)
    fixture_extra["expected"]["mapLifecycleCommandFacts"]["sourceIdentityJoins"][
        "calleeOwners"
    ][0]["extra"] = True
    with pytest.raises(ValueError, match="extra"):
        validate_json(fixture_extra, fixture_schema, owner="map-lifecycle fixture extra field")

    fixture_reordered = deepcopy(fixture)
    source_order = fixture_reordered["expected"]["mapLifecycleCommandFacts"][
        "sourceSiteOrderKeys"
    ]
    source_order[0], source_order[1] = source_order[1], source_order[0]
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            fixture_reordered,
            fixture_schema,
            owner="map-lifecycle fixture reordered source keys",
        )

    fixture_out_of_boundary = deepcopy(fixture)
    fixture_out_of_boundary["expected"]["mapLifecycleCommandFacts"]["handlers"][3][
        "sectionGuard"
    ]["operandPackUseSites"]["multiplierUseSite"]["value"] = 4
    with pytest.raises(ValueError, match="was expected"):
        validate_json(
            fixture_out_of_boundary,
            fixture_schema,
            owner="map-lifecycle fixture boundary",
        )


def test_map_lifecycle_schema_exact_blocks_keep_large_corpora_compact() -> None:
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for path in schema_paths:
        schema = load_json(path)
        contract = schema["properties"].get("mapLifecycleCommandFacts")
        if contract is None:
            contract = schema["properties"]["expected"]["properties"][
                "mapLifecycleCommandFacts"
            ]
        exact = contract["allOf"][1]
        assert "sourceSites" not in exact["properties"]
        assert "programTotals" not in exact["properties"]
        facts = schema["definitions"]["mapLifecycleCommandFacts"]
        assert facts["additionalProperties"] is False
        assert {"sourceSites", "programTotals"} <= set(facts["required"])
        lifecycle_definitions = {
            name: value
            for name, value in schema["definitions"].items()
            if name.startswith("mapLifecycle")
        }

        def assert_closed_objects(value):
            if isinstance(value, dict):
                if value.get("type") == "object":
                    assert value.get("additionalProperties") is False
                for child in value.values():
                    assert_closed_objects(child)
            elif isinstance(value, list):
                for child in value:
                    assert_closed_objects(child)

        for definition in lifecycle_definitions.values():
            assert_closed_objects(definition)


def test_map_interaction_trigger_contract_matches_complete_golden_fixture(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["mapInteractionTriggerCommandFacts"]
    assert actual == fixture["expected"]["mapInteractionTriggerCommandFacts"]
    assert [
        (
            macro["name"],
            macro["opcode"],
            macro["encodedBytes"],
            macro["operandBytes"],
            macro["sourceCommandCount"],
        )
        for macro in actual["macros"]
    ] == [("roofEvent", 67, 6, 4, 2), ("stepEvent", 71, 6, 4, 6)]
    assert [
        macro["sourceOperandAnnotations"] for macro in actual["macros"]
    ] == [
        [
            {
                "parameterOrdinal": 1,
                "sourceComment": "trigger X",
                "streamOffset": 2,
                "widthBytes": 2,
            },
            {
                "parameterOrdinal": 2,
                "sourceComment": "trigger Y",
                "streamOffset": 4,
                "widthBytes": 2,
            },
        ],
        [
            {
                "parameterOrdinal": 1,
                "sourceComment": "trigger X",
                "streamOffset": 2,
                "widthBytes": 2,
            },
            {
                "parameterOrdinal": 2,
                "sourceComment": "trigger Y",
                "streamOffset": 4,
                "widthBytes": 2,
            },
        ],
    ]
    assert actual["sourceSiteOrderKeys"] == [
        "cs_62D0E:15:roofEvent",
        "cs_5AC58:5:stepEvent",
        "cs_5AC58:20:stepEvent",
        "cs_5AF36:37:stepEvent",
        "cs_5B016:15:stepEvent",
        "cs_540C0:23:roofEvent",
        "cs_540C0:24:stepEvent",
        "cs_540C0:25:stepEvent",
    ]
    assert (
        actual["sourceSitesSha256"]
        == "525013B1AD4B1796BBBD398C063A3F7AF5DDD72D3B062888B1F1E7A26ECF58AB"
    )
    assert len(actual["programTotals"]) == 304
    assert (
        actual["programTotalsSha256"]
        == "D82DA66400E77E7881A4482AFF5A7541E64DA60E57068306B70102606E72F1E8"
    )
    assert [
        (
            handler["macro"],
            handler["handler"],
            handler["address"],
            handler["opcode"],
            handler["sourceCommandCount"],
            handler["statementCount"],
        )
        for handler in actual["handlers"]
    ] == [
        ("roofEvent", "csc43_RoofEvent", 288438, 67, 2, 6),
        ("stepEvent", "csc47_StepEvent", 288582, 71, 6, 6),
    ]
    for handler, target in zip(
        actual["handlers"], ("PerformMapBlockCopyScript", "OpenDoor"), strict=True
    ):
        assert handler["sectionGuard"] == {
            "orderedInstructions": [
                "move.w (a6)+,d0",
                "move.w (a6)+,d1",
                "mulu.w #MAP_TILE_SIZE,d0",
                "mulu.w #MAP_TILE_SIZE,d1",
                f"jsr ({target}).w",
                "rts",
            ],
            "scriptCursorReadUseSites": [
                {
                    "sourceRegister": "a6",
                    "destinationRegister": register,
                    "transferredByteCount": 2,
                    "cursorAdvanceByteCount": 2,
                    "instruction": f"move.w (a6)+,{register}",
                }
                for register in ("d0", "d1")
            ],
            "tileSizeUseSites": [
                {
                    "symbol": "MAP_TILE_SIZE",
                    "value": 384,
                    "destinationRegister": register,
                    "instruction": f"mulu.w #MAP_TILE_SIZE,{register}",
                }
                for register in ("d0", "d1")
            ],
            "directCallOrder": [f"jsr ({target}).w"],
            "returnInstruction": "rts",
        }
        assert handler["directCalls"] == [
            {"opcode": "jsr", "instructionTarget": target}
        ]
    assert actual["callerBreakdown"] == {
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
                "instructionTarget": target,
                "effectiveTarget": target,
                "aliasSourcePath": None,
                "effectiveTargetScope": "external",
            }
            for target in ("PerformMapBlockCopyScript", "OpenDoor")
        ],
        "instructionTargetTotals": {"PerformMapBlockCopyScript": 1, "OpenDoor": 1},
        "effectiveTargetTotals": {"PerformMapBlockCopyScript": 1, "OpenDoor": 1},
        "internalEffectiveTargetTotals": {"PerformMapBlockCopyScript": 0, "OpenDoor": 0},
        "externalEffectiveTargetTotals": {"PerformMapBlockCopyScript": 1, "OpenDoor": 1},
    }
    assert actual["sourceIdentityJoins"] == {
        "handlerSource": {
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "sourceSha256": "17F52906D05B933F318D509204460743591BA9F802D21B121D37217F156F83BF",
            "symbols": ["csc43_RoofEvent", "csc47_StepEvent"],
        },
        "calleeOwner": {
            "sourcePath": "code/gameflow/exploration/exploration.asm",
            "sourceSha256": "C38279815C832B5D65B443092048BB92E19FAEE47B81734A3EF0D16AA0E445A0",
            "symbols": ["PerformMapBlockCopyScript", "OpenDoor"],
            "relatedContractId": None,
        },
        "eventTableBoundary": {
            "mapContentContractId": "sf2-map-content-static-v1",
            "canonicalMapImportContractId": "sf2-canonical-map-import-v1",
            "mapContentSectionCounts": {"stepEvents": 79, "roofEvents": 79},
            "mapContentRecordCounts": {"stepEvents": 94, "roofEvents": 114},
            "canonicalResourceCounts": {"stepEventTables": 79, "roofEventTables": 79},
            "canonicalRecordCounts": {"stepEvents": 94, "roofEvents": 114},
        },
    }
    assert actual["runtimeQuestions"] == [
        "map-interaction-trigger/full-layout-collision-pathfinding-effects",
        "map-interaction-trigger/presentation-audio-timing-hardware-effects",
        "map-interaction-trigger/persistence-story-reachability",
    ]


def test_map_interaction_trigger_named_section_guards_reject_operand_and_order_drift() -> None:
    disasm = repo_path("local/upstream/SF2DISASM/disasm")
    equates = map_script_engine._source_equates(disasm)
    handler_profile = {
        "csc43_RoofEvent": {
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "statementCount": 6,
        },
        "csc47_StepEvent": {
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "statementCount": 6,
        },
    }
    statements_by_handler = {
        handler: map_script_engine._stable_handler_statements(
            disasm, {"name": handler, **profile}
        )
        for handler, profile in handler_profile.items()
    }

    roof_statements = statements_by_handler["csc43_RoofEvent"]
    assert map_script_engine._map_interaction_trigger_section_guard(
        "roofEvent", "csc43_RoofEvent", roof_statements, equates
    )["directCallOrder"] == ["jsr (PerformMapBlockCopyScript).w"]
    changed_operand = [
        statement.replace("mulu.w #MAP_TILE_SIZE,d1", "mulu.w #384,d1")
        for statement in roof_statements
    ]
    with pytest.raises(ValueError, match="csc43_RoofEvent statement is missing"):
        map_script_engine._map_interaction_trigger_section_guard(
            "roofEvent", "csc43_RoofEvent", changed_operand, equates
        )

    step_statements = statements_by_handler["csc47_StepEvent"]
    changed_call_order = step_statements.copy()
    changed_call_order[3], changed_call_order[4] = (
        changed_call_order[4],
        changed_call_order[3],
    )
    with pytest.raises(ValueError, match="csc47_StepEvent statement is missing"):
        map_script_engine._map_interaction_trigger_section_guard(
            "stepEvent", "csc47_StepEvent", changed_call_order, equates
        )


def test_map_interaction_trigger_bounded_construction_rejects_dispatcher_drift_before_fixture(
) -> None:
    """Keep one construction-level guard without building the full 304-program contract."""
    disasm = repo_path("local/upstream/SF2DISASM/disasm")
    macros = map_script_engine._map_macro_contracts(disasm)
    dispatch_source = map_script_engine.read_upstream_text(
        disasm / map_script_engine.DISPATCH_SOURCE
    )
    handler_profiles = {
        "csc43_RoofEvent": {
            "opcodes": [67],
            "encodedCommandBytes": 6,
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "statementCount": 6,
            "address": 288438,
        },
        "csc47_StepEvent": {
            "opcodes": [71],
            "encodedCommandBytes": 6,
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "statementCount": 6,
            "address": 288582,
        },
    }
    program_corpus = {
        "programs": [
            {
                "id": "bounded_interaction_trigger_guard",
                "commands": [
                    {
                        "index": 0,
                        "sourceLine": 1,
                        "macro": "roofEvent",
                        "arguments": ["0", "0"],
                    }
                ],
            }
        ],
        "summary": {"programCount": 1},
    }
    macros["roofEvent"]["opcode"] = 68
    with pytest.raises(ValueError, match="dispatcher target drift: roofEvent"):
        map_script_engine._map_interaction_trigger_command_facts(
            disasm,
            map_script_engine._source_equates(disasm),
            macros,
            map_script_engine._dispatch_targets(dispatch_source),
            [
                {"name": handler, **profile}
                for handler, profile in handler_profiles.items()
            ],
            program_corpus,
            {},
            b"",
            repo_path("local/roms/sf2-us.bin"),
            repo_path("local/upstream/SF2DISASM"),
        )


def test_map_interaction_trigger_parsers_reject_near_misses_and_preserve_comments(
    monkeypatch,
) -> None:
    assert map_script_engine._map_interaction_trigger_read_use_site("move.w (a6)+,d0") == {
        "sourceRegister": "a6",
        "destinationRegister": "d0",
        "transferredByteCount": 2,
        "cursorAdvanceByteCount": 2,
        "instruction": "move.w (a6)+,d0",
    }
    assert map_script_engine._map_interaction_trigger_read_use_site("move.w (a6)+,d1")[
        "destinationRegister"
    ] == "d1"
    for near_miss in (
        "move.b (a6)+,d0",
        "move.l (a6)+,d0",
        "move.w (a6)+,d2",
        "label_move.w (a6)+,d0",
        "; move.w (a6)+,d0",
        "move.w (a6)+,d0 ; comment",
    ):
        with pytest.raises(ValueError, match="cursor-read use-site drift"):
            map_script_engine._map_interaction_trigger_read_use_site(near_miss)
    assert map_script_engine._force_state_direct_calls(
        ["; jsr (OpenDoor).w", "move.w (a6)+,d0"]
    ) == []

    original_read = map_script_engine.read_upstream_text

    def missing_operand_comment(path):
        source = original_read(path)
        if path.name == "sf2cutscenemacros.asm":
            return source.replace("dc.w \\1 ; trigger X", "dc.w \\1", 1)
        return source

    monkeypatch.setattr(map_script_engine, "read_upstream_text", missing_operand_comment)
    with pytest.raises(ValueError, match="operand comment is missing: roofEvent"):
        map_script_engine._map_interaction_trigger_macro_annotations(
            repo_path("local/upstream/SF2DISASM/disasm")
        )


def test_map_interaction_trigger_schemas_reject_nested_mutations_and_exact_order() -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    facts = fixture["expected"]["mapInteractionTriggerCommandFacts"]
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )

    for path in schema_paths:
        schema = load_json(path)
        property_schema = schema["properties"].get("mapInteractionTriggerCommandFacts")
        if property_schema is None:
            property_schema = schema["properties"]["expected"]["properties"][
                "mapInteractionTriggerCommandFacts"
            ]
        exact_properties = property_schema["allOf"][1]["properties"]

        def definition_validator(
            name: str, *, schema: dict = schema
        ) -> Draft7Validator:
            return Draft7Validator(
                {
                    "$schema": schema["$schema"],
                    "definitions": {name: schema["definitions"][name]},
                    "$ref": f"#/definitions/{name}",
                },
                format_checker=FormatChecker(),
            )

        def rejects(validator: Draft7Validator, instance: object) -> None:
            assert next(validator.iter_errors(instance), None) is not None

        tile_validator = definition_validator("mapInteractionTriggerTileSizeUse")
        annotation_validator = definition_validator("mapInteractionTriggerOperandAnnotation")
        owner_validator = definition_validator("mapInteractionTriggerCalleeOwner")
        source_order_validator = Draft7Validator(exact_properties["sourceSiteOrderKeys"])
        handler_validator = Draft7Validator(exact_properties["handlers"])

        missing = deepcopy(facts["handlers"][0]["sectionGuard"]["tileSizeUseSites"][0])
        del missing["destinationRegister"]
        rejects(tile_validator, missing)

        renamed = deepcopy(facts["macros"][0]["sourceOperandAnnotations"][0])
        renamed["label"] = renamed.pop("sourceComment")
        rejects(annotation_validator, renamed)

        extra = deepcopy(facts["sourceIdentityJoins"]["calleeOwner"])
        extra["extra"] = True
        rejects(owner_validator, extra)

        reordered = facts["sourceSiteOrderKeys"].copy()
        reordered[0], reordered[1] = reordered[1], reordered[0]
        rejects(source_order_validator, reordered)

        out_of_boundary = deepcopy(facts["handlers"])
        out_of_boundary[0]["sectionGuard"]["tileSizeUseSites"][0]["value"] = 383
        rejects(handler_validator, out_of_boundary)


def test_map_interaction_trigger_schema_exact_blocks_keep_large_corpora_compact() -> None:
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for path in schema_paths:
        schema = load_json(path)
        contract = schema["properties"].get("mapInteractionTriggerCommandFacts")
        if contract is None:
            contract = schema["properties"]["expected"]["properties"][
                "mapInteractionTriggerCommandFacts"
            ]
        exact = contract["allOf"][1]
        assert "sourceSites" not in exact["properties"]
        assert "programTotals" not in exact["properties"]
        assert {
            "sourceSiteOrderKeys",
            "sourceSitesSha256",
            "programTotalOrderKeys",
            "programTotalsSha256",
        } <= set(exact["properties"])
        facts = schema["definitions"]["mapInteractionTriggerCommandFacts"]
        assert facts["additionalProperties"] is False
        assert {"sourceSites", "programTotals"} <= set(facts["required"])
        interaction_definitions = {
            name: value
            for name, value in schema["definitions"].items()
            if name.startswith("mapInteractionTrigger")
        }

        def assert_closed_objects(value):
            if isinstance(value, dict):
                if value.get("type") == "object":
                    assert value.get("additionalProperties") is False
                for child in value.values():
                    assert_closed_objects(child)
            elif isinstance(value, list):
                for child in value:
                    assert_closed_objects(child)

        for definition in interaction_definitions.values():
            assert_closed_objects(definition)


def test_map_entity_placement_contract_matches_complete_golden_fixture(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["entityPlacementCommandFacts"]
    assert fixture["expected"]["entityPlacementCommandFacts"] == {
        key: actual[key] for key in fixture["expected"]["entityPlacementCommandFacts"]
    }
    assert [
        (
            row["name"],
            row["opcode"],
            row["encodedBytes"],
            row["operandBytes"],
            row["sourceCommandCount"],
        )
        for row in actual["macros"]
    ] == [
        ("setPos", 25, 6, 4, 608),
        ("setPosFlash", 23, 6, 4, 2),
        ("setFacing", 35, 4, 2, 1579),
        ("setDest", 41, 8, 6, 99),
    ]
    assert [row["sourceOperandAnnotations"] for row in actual["macros"]] == [
        [
            {
                "parameterOrdinal": ordinal,
                "sourceComment": comment,
                "streamOffset": ordinal + 1,
                "widthBytes": 1,
            }
            for ordinal, comment in enumerate(("entity to act", "X", "Y", "facing"), 1)
        ],
        [
            {
                "parameterOrdinal": ordinal,
                "sourceComment": comment,
                "streamOffset": ordinal + 1,
                "widthBytes": 1,
            }
            for ordinal, comment in enumerate(("entity to act", "X", "Y", "facing"), 1)
        ],
        [
            {
                "parameterOrdinal": 1,
                "sourceComment": "entity to act",
                "streamOffset": 2,
                "widthBytes": 1,
            },
            {
                "parameterOrdinal": 2,
                "sourceComment": "facing",
                "streamOffset": 3,
                "widthBytes": 1,
            },
        ],
        [
            {
                "parameterOrdinal": ordinal,
                "sourceComment": comment,
                "streamOffset": ordinal * 2,
                "widthBytes": 2,
            }
            for ordinal, comment in enumerate(("entity to act", "X", "Y"), 1)
        ],
    ]
    assert len(actual["sourceSites"]) == 204
    assert len(actual["sourceSiteOrderKeys"]) == 2288
    assert actual["sourceSitesSha256"] == (
        "C451E4B4F2B154D9B01F7321E288D1E9DEC16A656E55730826C9E1800BE64734"
    )
    assert len(actual["programTotals"]) == 304
    assert actual["programTotalsSha256"] == (
        "5AE7802BB7D93463304AE491B89F136C763AF0E3BAF1EC85877F68E24867B388"
    )
    assert [
        (
            row["macro"],
            row["handler"],
            row["address"],
            row["opcode"],
            row["sourceCommandCount"],
            row["statementCount"],
        )
        for row in actual["handlers"]
    ] == [
        ("setPos", "csc19_setEntityPosAndFacing", 289298, 25, 608, 18),
        ("setPosFlash", "csc17_setEntityPosAndFacingWithFlash", 289196, 23, 2, 17),
        ("setFacing", "csc23_setEntityFacing", 289824, 35, 1579, 8),
        ("setDest", "csc29_setEntityDest", 290200, 41, 99, 29),
    ]
    assert actual["handlers"][0]["sectionGuard"]["aliveStatusCursorAdjustment"] == {
        "selectorPreReadInstruction": "move.b (a6),d0",
        "adjustmentLiteralInstruction": "moveq #4,d7",
        "adjustmentLiteralText": "4",
        "adjustmentLiteralValue": 4,
        "callInstruction": "bsr.w AdjustScriptPointerByCharacterAliveStatus",
    }
    assert actual["handlers"][1]["sectionGuard"]["sharedTail"] == {
        "targetHandler": "csc19_setEntityPosAndFacing",
        "targetFirstInstruction": "move.b (a6),d0",
        "cursorReadUseSites": actual["handlers"][0]["sectionGuard"][
            "scriptCursorReadUseSites"
        ],
    }
    assert actual["handlers"][3]["sectionGuard"]["branchRecords"] == [
        {
            "branchInstruction": "bpl.s loc_46DC4",
            "branchTarget": {
                "targetLabel": "loc_46DC4",
                "targetInstruction": "move.w d1,ENTITYDEF_OFFSET_XTRAVEL(a5)",
                "targetStatementIndex": 16,
            },
        },
        {
            "branchInstruction": "bpl.s loc_46DDA",
            "branchTarget": {
                "targetLabel": "loc_46DDA",
                "targetInstruction": "move.w d2,ENTITYDEF_OFFSET_YTRAVEL(a5)",
                "targetStatementIndex": 23,
            },
        },
        {
            "branchInstruction": "bne.s return_46DEC",
            "branchTarget": {
                "targetLabel": "return_46DEC",
                "targetInstruction": "rts",
                "targetStatementIndex": 28,
            },
        },
    ]
    assert actual["callerBreakdown"]["instructionTargetTotals"] == {
        "AdjustScriptPointerByCharacterAliveStatus": 2,
        "GetEntityAddressFromCharacter": 4,
        "UpdateEntitySprite_0": 2,
        "WaitForVInt": 2,
        "Sleep": 1,
        "WaitForEntityToStopMoving": 1,
    }
    assert actual["callerBreakdown"]["effectiveTargetTotals"] == actual[
        "callerBreakdown"
    ]["instructionTargetTotals"]
    for field in ("internalInstructionTargetTotals", "internalEffectiveTargetTotals"):
        assert actual["callerBreakdown"][field] == {
            target: 0
            for target in actual["callerBreakdown"]["instructionTargetTotals"]
        }
    assert actual["sourceIdentityJoins"]["entityActionStaticContractJoin"] == {
        "fixturePath": "tests/fixtures/h2/entity-action-scripts-static-v1.json",
        "fixtureId": "sf2-entity-action-scripts-static-v1",
        "upstreamCommit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
        "independentlyParsedFunctions": [
            {"symbol": "UpdateEntityData", "address": 23916},
            {"symbol": "ChangeEntityMapsprite", "address": 24744},
        ],
        "wrapperInstruction": "jsr (ChangeEntityMapsprite).w",
    }
    assert actual["runtimeQuestions"] == [
        "map-script-entity-placement/normal-story-reachability",
        "map-script-entity-placement/full-animation-visibility-presentation",
        "map-script-entity-placement/collision-pathfinding-persistence",
    ]


def test_map_entity_placement_source_guards_reject_use_site_order_and_label_drift() -> None:
    disasm = repo_path("local/upstream/SF2DISASM/disasm")
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    facts = fixture["expected"]["entityPlacementCommandFacts"]
    equates = map_script_engine._source_equates(disasm)
    assert _entity_placement_macro_annotations(disasm)["setPos"] == facts["macros"][0][
        "sourceOperandAnnotations"
    ]
    handler = facts["handlers"][0]
    statements = map_script_engine._stable_handler_statements(
        disasm,
        {
            "name": handler["handler"],
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "statementCount": handler["statementCount"],
        },
    )
    assert _entity_placement_section_guard("setPos", statements, equates)[
        "directCallOrder"
    ] == [
        "bsr.w AdjustScriptPointerByCharacterAliveStatus",
        "bsr.w GetEntityAddressFromCharacter",
        "bsr.w UpdateEntitySprite_0",
    ]
    with pytest.raises(ValueError, match="csc19_setEntityPosAndFacing statement is missing"):
        _entity_placement_section_guard(
            "setPos",
            [
                statement.replace("moveq #4,d7", "moveq #5,d7")
                for statement in statements
            ],
            equates,
        )
    alive_adjustment_reordered = statements.copy()
    alive_adjustment_reordered[0], alive_adjustment_reordered[1] = (
        alive_adjustment_reordered[1],
        alive_adjustment_reordered[0],
    )
    with pytest.raises(ValueError, match="csc19_setEntityPosAndFacing statement is missing"):
        _entity_placement_section_guard("setPos", alive_adjustment_reordered, equates)
    with pytest.raises(ValueError, match="csc19_setEntityPosAndFacing statement is missing"):
        _entity_placement_section_guard(
            "setPos",
            [
                statement.replace("bsr.w UpdateEntitySprite_0", "bsr.w UpdateEntityData")
                for statement in statements
            ],
            equates,
        )
    with pytest.raises(ValueError, match="csc19_setEntityPosAndFacing statement is missing"):
        _entity_placement_section_guard(
            "setPos",
            [
                statement.replace("mulu.w #MAP_TILE_SIZE,d0", "mulu.w #384,d0")
                for statement in statements
            ],
            equates,
        )
    flash = facts["handlers"][1]
    flash_statements = map_script_engine._stable_handler_statements(
        disasm,
        {
            "name": flash["handler"],
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "statementCount": flash["statementCount"],
        },
    )
    with pytest.raises(
        ValueError, match="csc17_setEntityPosAndFacingWithFlash statement is missing"
    ):
        _entity_placement_section_guard(
            "setPosFlash",
            [
                statement.replace(
                    "bra.w csc19_setEntityPosAndFacing", "bra.w csc23_setEntityFacing"
                )
                for statement in flash_statements
            ],
            equates,
        )
    destination = facts["handlers"][3]
    destination_statements = map_script_engine._stable_handler_statements(
        disasm,
        {
            "name": destination["handler"],
            "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
            "statementCount": destination["statementCount"],
        },
    )
    with pytest.raises(ValueError, match="csc29_setEntityDest statement is missing"):
        _entity_placement_section_guard(
            "setDest",
            [
                statement.replace("bpl.s loc_46DC4", "bne.s loc_46DC4")
                for statement in destination_statements
            ],
            equates,
        )
    section_source = map_script_engine._map_camera_control_named_section_source(
        disasm,
        "code/common/scripting/map/mapscriptengine_1.asm",
        "csc29_setEntityDest",
    )
    with pytest.raises(ValueError, match="branch target label is missing"):
        _entity_placement_branch_target_record(
            section_source.replace("loc_46DC4:", "loc_46DC5:"),
            "bpl.s loc_46DC4",
            "move.w d1,ENTITYDEF_OFFSET_XTRAVEL(a5)",
            destination_statements,
        )
    wrapper_source = map_script_engine._map_camera_control_named_section_source(
        disasm,
        "code/common/scripting/map/mapscriptengine_1.asm",
        "UpdateEntitySprite_0",
    )
    assert _entity_placement_update_entity_sprite_wrapper_use_site(wrapper_source) == {
        "instruction": facts["sourceIdentityJoins"]["entityActionStaticContractJoin"][
            "wrapperInstruction"
        ]
    }
    wrapper_statements = _statements(wrapper_source)
    target_drift = [
        statement.replace("(ChangeEntityMapsprite).w", "(UpdateEntityData).w")
        for statement in wrapper_statements
    ]
    opcode_drift = [
        statement.replace("jsr (ChangeEntityMapsprite).w", "bsr (ChangeEntityMapsprite).w")
        for statement in wrapper_statements
    ]
    order_drift = wrapper_statements.copy()
    order_drift[2], order_drift[3] = order_drift[3], order_drift[2]
    for mutation in (target_drift, opcode_drift, order_drift):
        with pytest.raises(ValueError, match="UpdateEntitySprite_0 statement is missing"):
            _entity_placement_update_entity_sprite_wrapper_use_site("\n".join(mutation))


def test_map_entity_placement_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    sources = (
        map_script_engine_output,
        fixture,
    )
    for schema_path, source in zip(schema_paths, sources, strict=True):
        schema = schema_path
        validate_json(source, schema, owner="entity-placement baseline")

        missing = deepcopy(source)
        target = (
            missing["entityPlacementCommandFacts"]
            if source is map_script_engine_output
            else missing["expected"]["entityPlacementCommandFacts"]
        )
        del target["handlers"][3]["sectionGuard"]["branchRecords"][0]["branchTarget"][
            "targetStatementIndex"
        ]
        with pytest.raises(ValueError, match="targetStatementIndex"):
            validate_json(missing, schema, owner="entity-placement missing nested field")

        renamed = deepcopy(source)
        target = (
            renamed["entityPlacementCommandFacts"]
            if source is map_script_engine_output
            else renamed["expected"]["entityPlacementCommandFacts"]
        )
        operand = target["macros"][0]["sourceOperandAnnotations"][0]
        operand["label"] = operand.pop("sourceComment")
        with pytest.raises(ValueError, match="sourceComment"):
            validate_json(renamed, schema, owner="entity-placement renamed nested field")

        extra = deepcopy(source)
        target = (
            extra["entityPlacementCommandFacts"]
            if source is map_script_engine_output
            else extra["expected"]["entityPlacementCommandFacts"]
        )
        target["handlers"][1]["sectionGuard"]["sharedTail"]["extra"] = True
        with pytest.raises(ValueError, match="extra"):
            validate_json(extra, schema, owner="entity-placement extra nested field")

        reordered = deepcopy(source)
        target = (
            reordered["entityPlacementCommandFacts"]
            if source is map_script_engine_output
            else reordered["expected"]["entityPlacementCommandFacts"]
        )
        source_order = target["sourceSiteOrderKeys"]
        source_order[0], source_order[1] = source_order[1], source_order[0]
        with pytest.raises(ValueError, match="was expected"):
            validate_json(reordered, schema, owner="entity-placement exact source order")

        boundary = deepcopy(source)
        target = (
            boundary["entityPlacementCommandFacts"]
            if source is map_script_engine_output
            else boundary["expected"]["entityPlacementCommandFacts"]
        )
        target["handlers"][0]["sectionGuard"]["aliveStatusCursorAdjustment"][
            "adjustmentLiteralValue"
        ] = 5
        with pytest.raises(ValueError, match="was expected"):
            validate_json(boundary, schema, owner="entity-placement exact boundary")


def test_map_entity_placement_schema_exact_blocks_keep_large_corpora_compact() -> None:
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for path in schema_paths:
        schema = load_json(path)
        contract = schema["properties"].get("entityPlacementCommandFacts")
        if contract is None:
            contract = schema["properties"]["expected"]["properties"][
                "entityPlacementCommandFacts"
            ]
        exact = contract["allOf"][1]
        assert "sourceSites" not in exact["properties"]
        assert "programTotals" not in exact["properties"]
        facts = schema["definitions"]["entityPlacementCommandFacts"]
        assert facts["additionalProperties"] is False
        assert {"sourceSites", "programTotals"} <= set(facts["required"])

        def assert_closed_objects(value):
            if isinstance(value, dict):
                if value.get("type") == "object":
                    assert value.get("additionalProperties") is False
                for child in value.values():
                    assert_closed_objects(child)
            elif isinstance(value, list):
                for child in value:
                    assert_closed_objects(child)

        for name, definition in schema["definitions"].items():
            if name.startswith("entityPlacement"):
                assert_closed_objects(definition)


def test_map_entity_action_bridge_contract_matches_complete_golden_fixture(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["entityActionBridgeCommandFacts"]
    assert fixture["expected"]["entityActionBridgeCommandFacts"] == {
        key: actual[key] for key in fixture["expected"]["entityActionBridgeCommandFacts"]
    }
    assert [
        (
            row["name"],
            row["opcode"],
            row["primaryEncodedCommandByteCount"],
            row["primaryOperandByteCount"],
            row["sourceControlField"]["value"],
            row["sourceCommandCount"],
        )
        for row in actual["macros"]
    ] == [
        ("setActscriptWait", 21, 8, 6, 255, 1015),
        ("setActscript", 21, 8, 6, 0, 436),
        ("customActscriptWait", 20, 4, 2, 255, 359),
        ("customActscript", 20, 4, 2, 0, 2),
        ("entityActionsWait", 45, 4, 2, 255, 957),
        ("entityActions", 45, 4, 2, 0, 487),
    ]
    assert len(actual["sourceSites"]) == 196
    assert len(actual["sourceSiteOrderKeys"]) == 3256
    assert actual["sourceSitesSha256"] == (
        "3FCEFC418031DE5457EE1F47972A3EC2CA95645E45779BB41979D287EAF92BED"
    )
    assert len(actual["programTotals"]) == 304
    assert actual["programTotalsSha256"] == (
        "C22323C27AFC8BD2F6DFAFA721F26F152582A7AF0D9A23B11CDCBCE2DF5D648F"
    )
    assert [
        (
            row["handler"],
            row["address"],
            row["opcode"],
            row["macros"],
            row["sourceCommandCounts"],
            row["statementCount"],
        )
        for row in actual["handlers"]
    ] == [
        (
            "csc15_setEntityActscript",
            289144,
            21,
            ["setActscriptWait", "setActscript"],
            {"setActscriptWait": 1015, "setActscript": 436},
            10,
        ),
        (
            "csc14_setEntityActscriptManual",
            289104,
            20,
            ["customActscriptWait", "customActscript"],
            {"customActscriptWait": 359, "customActscript": 2},
            12,
        ),
        (
            "csc2D_entityActionSequence",
            288738,
            45,
            ["entityActionsWait", "entityActions"],
            {"entityActionsWait": 957, "entityActions": 487},
            18,
        ),
    ]
    source_commands = [
        command for site in actual["sourceSites"] for command in site["commands"]
    ]
    assert {
        kind: sum(command["payloadKind"] == kind for command in source_commands)
        for kind in ("none", "ac-macro-stream", "entity-action-byte-stream")
    } == {"none": 1451, "ac-macro-stream": 361, "entity-action-byte-stream": 1444}
    assert sum(
        command["payload"]["commandEncodedByteCount"] for command in source_commands
    ) == 8048
    assert all(
        command["payloadScanTransferByteCount"] == 2
        and command["payloadScanIterationCount"]
        * command["payloadScanTransferByteCount"]
        == command["payload"]["commandEncodedByteCount"]
        and command["payloadCommandCursorAdvanceByteCount"]
        == command["payload"]["commandEncodedByteCount"]
        and command["terminatorCursorAdvanceByteCount"] == 2
        and command["scriptCursorAdvanceByteCount"]
        == command["primaryOperandCursorAdvanceByteCount"]
        + command["payloadCommandCursorAdvanceByteCount"]
        + command["terminatorCursorAdvanceByteCount"]
        for command in source_commands
        if command["payloadKind"] == "ac-macro-stream"
    )
    assert all(
        command["payloadScanTransferByteCount"] == 0
        and command["payloadScanIterationCount"] == 0
        and command["payloadCommandCursorAdvanceByteCount"]
        == command["payload"]["commandEncodedByteCount"]
        and command["terminatorCursorAdvanceByteCount"] == 2
        for command in source_commands
        if command["payloadKind"] == "entity-action-byte-stream"
    )
    assert actual["callerBreakdown"]["instructionTargetTotals"] == {
        "GetEntityAddressFromCharacter": 3,
        "rjt_EntityMoveCommands": 1,
    }
    assert actual["callerBreakdown"]["effectiveTargetTotals"] == actual[
        "callerBreakdown"
    ]["instructionTargetTotals"]
    assert actual["handlers"][2]["sectionGuard"]["tailTransferTarget"] == {
        "targetLabel": "loc_467FC",
        "targetInstruction": "move.b (a6)+,d1",
    }
    for field in ("internalInstructionTargetTotals", "internalEffectiveTargetTotals"):
        assert actual["callerBreakdown"][field] == {
            "GetEntityAddressFromCharacter": 0,
            "rjt_EntityMoveCommands": 0,
        }
    assert actual["sourceIdentityJoins"]["entityActionStaticContract"] == {
        "fixturePath": "tests/fixtures/h2/entity-action-scripts-static-v1.json",
        "fixtureId": "sf2-entity-action-scripts-static-v1",
        "upstreamCommit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
        "inlineTerminatorMacro": "ac_end",
        "inlineTerminatorWord": 32896,
    }
    assert actual["runtimeQuestions"] == [
        "map-script-entity-action-bridge/normal-story-reachability",
        "map-script-entity-action-bridge/full-action-motion-collision-effects",
        "map-script-entity-action-bridge/presentation-timing-persistence",
    ]


def test_map_entity_action_bridge_source_guards_reject_local_drift(
    map_script_engine_output: dict, monkeypatch: pytest.MonkeyPatch
) -> None:
    disasm = repo_path("local/upstream/SF2DISASM/disasm")
    equates = map_script_engine._source_equates(disasm)
    handlers = {
        row["name"]: row
        for row in map_script_engine_output["handlers"]
        if row["name"]
        in {
            "csc14_setEntityActscriptManual",
            "csc15_setEntityActscript",
            "csc2D_entityActionSequence",
        }
    }
    csc14 = map_script_engine._stable_handler_statements(
        disasm, handlers["csc14_setEntityActscriptManual"]
    )
    csc15 = map_script_engine._stable_handler_statements(
        disasm, handlers["csc15_setEntityActscript"]
    )
    csc2d = map_script_engine._stable_handler_statements(
        disasm, handlers["csc2D_entityActionSequence"]
    )
    terminal = _statements(map_script_engine._entity_action_bridge_csc2d_terminal_source(disasm))
    assert _entity_action_bridge_section_guard(
        "csc14_setEntityActscriptManual", csc14, equates
    )["cursorAdvanceProfile"] == {
        "primaryOperandCursorAdvanceByteCount": 2,
        "payloadCommandReadByteCount": 0,
        "payloadScanTransferByteCount": 2,
        "terminatorCursorAdvanceByteCount": 2,
    }
    assert _entity_action_bridge_section_guard(
        "csc15_setEntityActscript", csc15, equates
    )["cursorAdvanceProfile"]["primaryOperandCursorAdvanceByteCount"] == 6
    assert _entity_action_bridge_section_guard(
        "csc2D_entityActionSequence", csc2d, equates, terminal
    )["cursorAdvanceProfile"]["payloadCommandReadByteCount"] == 2
    mutations = (
        ("csc14_setEntityActscriptManual", csc14, None, "cmpi.w #$8080", "cmpi.w #$8081"),
        ("csc14_setEntityActscriptManual", csc14, None, "cmpi.w #$8080", "cmpi.b #$80"),
        ("csc14_setEntityActscriptManual", csc14, None, "beq.w loc_46970", "bne.w loc_46970"),
        ("csc15_setEntityActscript", csc15, None, "move.l (a6)+", "move.w (a6)+"),
        (
            "csc15_setEntityActscript",
            csc15,
            None,
            "bsr.w GetEntityAddressFromCharacter",
            "bsr.w UpdateEntitySprite_0",
        ),
        ("csc2D_entityActionSequence", csc2d, terminal, "bmi.w loc_46928", "bpl.w loc_46928"),
        (
            "csc2D_entityActionSequence",
            csc2d,
            terminal,
            "rjt_EntityMoveCommands",
            "rjt_EntityMoveCommandsDrift",
        ),
        ("csc2D_entityActionSequence", csc2d, terminal, "bra.s loc_467FC", "bne.s loc_467FC"),
    )
    for handler_name, statements, terminal_statements, original, replacement in mutations:
        with pytest.raises(ValueError, match="statement is missing"):
            _entity_action_bridge_section_guard(
                handler_name,
                [statement.replace(original, replacement) for statement in statements],
                equates,
                terminal_statements,
            )
    reordered = csc15.copy()
    reordered[0], reordered[1] = reordered[1], reordered[0]
    with pytest.raises(ValueError, match="statement is missing"):
        _entity_action_bridge_section_guard(
            "csc15_setEntityActscript", reordered, equates
        )
    with pytest.raises(ValueError, match="terminal chunk statement is missing"):
        _entity_action_bridge_section_guard(
            "csc2D_entityActionSequence",
            csc2d,
            equates,
            [statement.replace("addq.l #1,a6", "addq.l #2,a6") for statement in terminal],
        )
    source = map_script_engine.read_upstream_text(
        disasm / "code/common/scripting/map/mapscriptengine_1.asm"
    )
    with pytest.raises(ValueError, match="branch target label is missing"):
        _entity_action_bridge_branch_target_record(
            source.replace("loc_46928:", "loc_46929:"),
            "bmi.w loc_46928",
            "move.w #$34,(a0)+",
        )
    assert _entity_action_bridge_branch_target_record(
        source,
        "bra.s loc_467FC",
        "move.b (a6)+,d1",
    ) == {"targetLabel": "loc_467FC", "targetInstruction": "move.b (a6)+,d1"}
    with pytest.raises(ValueError, match="branch target label is missing"):
        _entity_action_bridge_branch_target_record(
            source.replace("loc_467FC:", "loc_467FD:"),
            "bra.s loc_467FC",
            "move.b (a6)+,d1",
        )
    with pytest.raises(ValueError, match="branch target instruction drift"):
        _entity_action_bridge_branch_target_record(
            source.replace("move.b  (a6)+,d1", "move.b  (a6)+,d3", 1),
            "bra.s loc_467FC",
            "move.b (a6)+,d1",
        )
    catalog, command_names, _ = map_script_engine._entity_action_bridge_payload_macro_catalog(
        disasm
    )
    payload_lines = [(1, "customActscriptWait 128"), (2, "ac_setSpeed 48,48"), (3, "ac_end")]
    assert _entity_action_bridge_inline_payload(
        payload_lines,
        opener_line=1,
        payload_macro_names=command_names,
        terminator="ac_end",
        catalog=catalog,
    )["terminatorWord"] == 0x8080
    with pytest.raises(ValueError, match="inline payload instruction is missing"):
        _entity_action_bridge_inline_payload(
            payload_lines[:-1] + [(3, "ac_stop")],
            opener_line=1,
            payload_macro_names=command_names,
            terminator="ac_end",
            catalog=catalog,
        )
    original_reader = map_script_engine.read_upstream_text

    def altered_reader(path: Path) -> str:
        source_text = original_reader(path)
        if path.name == "sf2cutscenemacros.asm":
            return source_text.replace("csc15 \\1,$FF,\\2", "csc15 \\1,1,\\2", 1)
        return source_text

    monkeypatch.setattr(map_script_engine, "read_upstream_text", altered_reader)
    with pytest.raises(ValueError, match="primary emission ABI drift"):
        _entity_action_bridge_macro_facts(disasm, map_script_engine_output["macroContracts"])

    def opcode_altered_reader(path: Path) -> str:
        source_text = original_reader(path)
        if path.name == "sf2cutscenemacros.asm":
            return source_text.replace("dc.w $15", "dc.w $16", 1)
        return source_text

    monkeypatch.setattr(map_script_engine, "read_upstream_text", opcode_altered_reader)
    with pytest.raises(ValueError, match="primary emission ABI drift"):
        _entity_action_bridge_macro_facts(disasm, map_script_engine_output["macroContracts"])

    def misaligned_payload_reader(path: Path) -> str:
        source_text = original_reader(path)
        if path.name == "sf2cutscenemacros.asm":
            return source_text.replace("dc.b \\1 ; X speed", "dc.w \\1 ; X speed", 1)
        return source_text

    bridge_macros = {
        row["name"]: row
        for row in map_script_engine_output["entityActionBridgeCommandFacts"]["macros"]
    }
    cursor_profiles = {
        row["handler"]: row["sectionGuard"]["cursorAdvanceProfile"]
        for row in map_script_engine_output["entityActionBridgeCommandFacts"]["handlers"]
    }
    monkeypatch.setattr(map_script_engine, "read_upstream_text", misaligned_payload_reader)
    with pytest.raises(ValueError, match="source payload drift"):
        map_script_engine._entity_action_bridge_program_facts(
            disasm,
            map_script_engine_output["programCorpus"],
            bridge_macros,
            cursor_profiles,
            equates,
        )


def test_map_entity_action_bridge_cursor_parser_handles_sizes_and_near_misses() -> None:
    assert _entity_action_bridge_cursor_use_site("move.b (a6)+,d0")[
        "cursorAdvanceByteCount"
    ] == 1
    assert _entity_action_bridge_cursor_use_site("move.w (a6)+,d2")[
        "transferredByteCount"
    ] == 2
    assert _entity_action_bridge_cursor_use_site(
        "move.l (a6)+,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)"
    )["cursorAdvanceByteCount"] == 4
    assert _entity_action_bridge_cursor_use_site("cmpi.w #$8080,(a6)+")[
        "cursorAdvanceByteCount"
    ] == 2
    assert _entity_action_bridge_cursor_use_site("addq.l #1,a6")["cursorAdvanceByteCount"] == 1
    for near_miss in (
        "label: move.b (a6)+,d0",
        "; move.b (a6)+,d0",
        "move.b (a6),d0",
        "move.q (a6)+,d0",
    ):
        with pytest.raises(ValueError, match="cursor use is not recognized"):
            _entity_action_bridge_cursor_use_site(near_miss)


def test_map_entity_action_bridge_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    sources = (map_script_engine_output, fixture)
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for source, schema_path in zip(sources, schema_paths, strict=True):
        validate_json(source, schema_path, owner="entity-action bridge baseline")
        target_path = (
            ("entityActionBridgeCommandFacts",)
            if source is map_script_engine_output
            else ("expected", "entityActionBridgeCommandFacts")
        )

        def target_for(value: dict, target_path: tuple[str, ...] = target_path) -> dict:
            target = value
            for key in target_path:
                target = target[key]
            return target

        missing = deepcopy(source)
        del target_for(missing)["macros"][0]["sourceSelectorField"]["streamOffset"]
        with pytest.raises(ValueError, match="streamOffset"):
            validate_json(missing, schema_path, owner="entity-action bridge missing nested field")

        renamed = deepcopy(source)
        field = target_for(renamed)["handlers"][0]["directCalls"][0]
        field["target"] = field.pop("instructionTarget")
        with pytest.raises(ValueError, match="instructionTarget"):
            validate_json(renamed, schema_path, owner="entity-action bridge renamed nested field")

        extra = deepcopy(source)
        target_for(extra)["handlers"][2]["sectionGuard"]["cursorAdvanceProfile"]["extra"] = 1
        with pytest.raises(ValueError, match="extra"):
            validate_json(extra, schema_path, owner="entity-action bridge extra nested field")

        reordered = deepcopy(source)
        order = target_for(reordered)["sourceSiteOrderKeys"]
        order[0], order[1] = order[1], order[0]
        with pytest.raises(ValueError, match="was expected"):
            validate_json(reordered, schema_path, owner="entity-action bridge exact source order")

        boundary = deepcopy(source)
        target_for(boundary)["macros"][0]["sourceControlField"]["value"] = 1
        with pytest.raises(ValueError, match="was expected"):
            validate_json(boundary, schema_path, owner="entity-action bridge exact boundary")


def test_map_entity_action_bridge_schema_exact_blocks_keep_large_corpora_compact() -> None:
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for path in schema_paths:
        schema = load_json(path)
        contract = schema["properties"].get("entityActionBridgeCommandFacts")
        if contract is None:
            contract = schema["properties"]["expected"]["properties"][
                "entityActionBridgeCommandFacts"
            ]
        exact = contract["allOf"][1]
        exact_value = exact.get("const", exact.get("properties", {}))
        assert "sourceSites" not in exact_value
        assert "programTotals" not in exact_value
        definition_name = (
            "entityActionBridgeCommandFacts"
            if "entityActionBridgeCommandFacts" in schema["definitions"]
            else "entityActionBridgeFixtureCommandFacts"
        )
        facts = schema["definitions"][definition_name]
        assert facts["additionalProperties"] is False
        if definition_name == "entityActionBridgeCommandFacts":
            assert {"sourceSites", "programTotals"} <= set(facts["required"])
            assert schema["definitions"]["entityActionBridgeSourceSite"][
                "additionalProperties"
            ] is False
            assert schema["definitions"]["entityActionBridgeProgramTotal"][
                "additionalProperties"
            ] is False

        def assert_closed_objects(value):
            if isinstance(value, dict):
                if value.get("type") == "object":
                    assert value.get("additionalProperties") is False
                for child in value.values():
                    assert_closed_objects(child)
            elif isinstance(value, list):
                for child in value:
                    assert_closed_objects(child)

        for name, definition in schema["definitions"].items():
            if name.startswith("entityActionBridge"):
                assert_closed_objects(definition)


def test_map_entity_lifecycle_presentation_contract_matches_complete_golden_fixture(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["entityLifecyclePresentationCommandFacts"]
    assert fixture["expected"]["entityLifecyclePresentationCommandFacts"] == {
        key: actual[key]
        for key in fixture["expected"]["entityLifecyclePresentationCommandFacts"]
    }
    assert [
        (
            row["name"],
            row["opcode"],
            row["encodedBytes"],
            row["operandBytes"],
            row["sourceCommandCount"],
            row["handler"],
        )
        for row in actual["macros"]
    ] == [
        ("hide", 46, 4, 2, 141, "csc2E_hideEntity"),
        ("startEntity", 27, 4, 2, 70, "csc1B_startEntityAnim"),
        ("stopEntity", 28, 4, 2, 107, "csc1C_stopEntityAnim"),
        ("waitIdle", 22, 4, 2, 30, "csc16_waitUntilEntityIdle"),
        ("setSprite", 26, 6, 4, 56, "csc1A_setEntitySprite"),
        ("setPriority", 83, 6, 4, 51, "csc53_setPriority"),
        ("removeShadow", 48, 4, 2, 5, "csc30_removeEntityShadow"),
        ("setSize", 80, 6, 4, 4, "csc50_setEntitySize"),
    ]
    assert sum(row["sourceCommandCount"] for row in actual["macros"]) == 464
    assert len(actual["sourceSites"]) == 105
    assert len(actual["sourceSiteOrderKeys"]) == 464
    assert actual["sourceSitesSha256"] == (
        "152416D18046AC324FCF0EBA3F148B82D723FAF03705698B58565F17935E88AD"
    )
    assert len(actual["programTotals"]) == 304
    assert actual["programTotalsSha256"] == (
        "0ADCBF8A1207FD628CBC63B8BCD028F9426D585E97F29271FB0A23904F05EA3C"
    )
    assert [
        (
            row["macro"],
            row["handler"],
            row["address"],
            row["opcode"],
            row["sourceCommandCount"],
            row["statementCount"],
        )
        for row in actual["handlers"]
    ] == [
        ("hide", "csc2E_hideEntity", 290458, 46, 141, 4),
        ("startEntity", "csc1B_startEntityAnim", 289388, 27, 70, 7),
        ("stopEntity", "csc1C_stopEntityAnim", 289410, 28, 107, 7),
        ("waitIdle", "csc16_waitUntilEntityIdle", 289178, 22, 30, 5),
        ("setSprite", "csc1A_setEntitySprite", 289352, 26, 56, 11),
        ("setPriority", "csc53_setPriority", 290750, 83, 51, 10),
        ("removeShadow", "csc30_removeEntityShadow", 290496, 48, 5, 8),
        ("setSize", "csc50_setEntitySize", 290528, 80, 4, 9),
    ]
    assert actual["handlers"][1]["sectionGuard"]["aliveStatusPointerAdjustment"] == {
        "selectorPreReadUseSite": {
            "sourceRegister": "a6",
            "destinationOperand": "d0",
            "transferredByteCount": 2,
            "cursorAdvanceByteCount": 0,
            "instruction": "move.w (a6),d0",
        },
        "adjustmentLiteralInstruction": "moveq #2,d7",
        "adjustmentLiteralText": "2",
        "adjustmentLiteralValue": 2,
        "callInstruction": "bsr.w AdjustScriptPointerByCharacterAliveStatus",
    }
    assert actual["handlers"][7]["sectionGuard"]["bitMutationUseSites"] == [
        {
            "sourceOperand": "ENTITYDEF_OFFSET_FLAGS_B(a5)",
            "operation": "or-immediate",
            "immediateText": "%1000",
            "immediateValue": 8,
            "bitIndices": [3],
            "instruction": "ori.b #%1000,ENTITYDEF_OFFSET_FLAGS_B(a5)",
        }
    ]
    assert actual["callerBreakdown"]["instructionTargetTotals"] == {
        "GetEntityAddressFromCharacter": 8,
        "HideEntity": 1,
        "AdjustScriptPointerByCharacterAliveStatus": 2,
        "GetAllyMapsprite": 1,
        "WaitForVInt": 3,
        "UpdateEntitySprite_0": 2,
        "LoadMapsprite": 1,
        "sub_45A8C": 1,
        "DmaMapsprite": 1,
    }
    assert actual["callerBreakdown"]["effectiveTargetTotals"] == actual[
        "callerBreakdown"
    ]["instructionTargetTotals"]
    for field in ("internalInstructionTargetTotals", "internalEffectiveTargetTotals"):
        assert actual["callerBreakdown"][field] == {
            target: 0 for target in actual["callerBreakdown"]["instructionTargetTotals"]
        }
    assert actual["sourceIdentityJoins"]["entityActionStaticContract"] == {
        "fixturePath": "tests/fixtures/h2/entity-action-scripts-static-v1.json",
        "fixtureId": "sf2-entity-action-scripts-static-v1",
        "upstreamCommit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
        "independentlyParsedFunctions": [
            {"symbol": "UpdateEntityData", "address": 23916},
            {"symbol": "ChangeEntityMapsprite", "address": 24744},
        ],
        "wrapperInstruction": "jsr (ChangeEntityMapsprite).w",
    }
    assert actual["sourceIdentityJoins"]["mapSpriteAssignmentStaticContract"]["fixtureId"] == (
        "sf2-map-sprite-assignments-static-v1"
    )
    assert actual["sourceIdentityJoins"]["spriteDialogueStaticContract"]["fixtureId"] == (
        "sf2-sprite-dialogue-static-v1"
    )
    assert actual["runtimeQuestions"] == [
        "map-script-entity-lifecycle-presentation/normal-story-reachability",
        "map-script-entity-lifecycle-presentation/full-entity-state-callback-effects",
        "map-script-entity-lifecycle-presentation/player-visible-presentation-timing-collision-persistence",
    ]


def test_map_entity_lifecycle_presentation_source_guards_reject_local_drift(
    map_script_engine_output: dict, monkeypatch: pytest.MonkeyPatch
) -> None:
    disasm = repo_path("local/upstream/SF2DISASM/disasm")
    equates = map_script_engine._source_equates(disasm)
    facts = map_script_engine_output["entityLifecyclePresentationCommandFacts"]
    annotations = _entity_lifecycle_presentation_macro_annotations(disasm)
    assert annotations["setSize"] == facts["macros"][-1]["sourceOperandAnnotations"]
    source_handlers = {
        row["name"]: row for row in map_script_engine_output["handlers"]
    }
    guarded_handlers = {row["macro"]: row for row in facts["handlers"]}
    for macro, handler in guarded_handlers.items():
        statements = map_script_engine._stable_handler_statements(
            disasm, source_handlers[handler["handler"]]
        )
        guard = _entity_lifecycle_presentation_section_guard(macro, statements, equates)
        assert guard["orderedInstructions"] == handler["sectionGuard"]["orderedInstructions"]
        assert guard["directCallOrder"] == handler["sectionGuard"]["directCallOrder"]
        assert sum(
            row["cursorAdvanceByteCount"] for row in guard["scriptCursorReadUseSites"]
        ) == next(row["operandBytes"] for row in facts["macros"] if row["name"] == macro)
    mutations = (
        ("hide", "jsr HideEntity", "jsr UpdateEntityData"),
        ("startEntity", "moveq #2,d7", "moveq #3,d7"),
        ("stopEntity", "move.b #-1", "move.b #0"),
        ("waitIdle", "bne.s loc_469A0", "beq.s loc_469A0"),
        (
            "setSprite",
            "cmpi.w #COMBATANT_ALLIES_NUMBER,d0",
            "cmpi.w #COMBATANT_ENEMIES_NUMBER,d0",
        ),
        ("setPriority", "bne.s loc_46FD4", "beq.s loc_46FD4"),
        ("removeShadow", "bsr.w DmaMapsprite", "bsr.w LoadMapsprite"),
        ("setSize", "ori.b #%1000", "ori.b #%0100"),
    )
    for macro, original, replacement in mutations:
        handler = guarded_handlers[macro]
        statements = map_script_engine._stable_handler_statements(
            disasm, source_handlers[handler["handler"]]
        )
        with pytest.raises(ValueError, match="statement is missing"):
            _entity_lifecycle_presentation_section_guard(
                macro,
                [statement.replace(original, replacement) for statement in statements],
                equates,
            )
    reordered = map_script_engine._stable_handler_statements(
        disasm, source_handlers["csc1B_startEntityAnim"]
    )
    reordered[0], reordered[1] = reordered[1], reordered[0]
    with pytest.raises(ValueError, match="statement is missing"):
        _entity_lifecycle_presentation_section_guard("startEntity", reordered, equates)
    section_source = map_script_engine._map_camera_control_named_section_source(
        disasm,
        "code/common/scripting/map/mapscriptengine_1.asm",
        "csc1A_setEntitySprite",
    )
    set_sprite = guarded_handlers["setSprite"]
    with pytest.raises(ValueError, match="branch target label is missing"):
        _entity_lifecycle_presentation_branch_target_record(
            section_source.replace("@NotAlly:", "@NotAllies:"),
            "bcc.s @NotAlly",
            "move.b d0,ENTITYDEF_OFFSET_MAPSPRITE(a5)",
            set_sprite["sectionGuard"]["orderedInstructions"],
        )
    original_reader = map_script_engine.read_upstream_text

    def annotation_altered_reader(path: Path) -> str:
        source = original_reader(path)
        if path.name == "sf2cutscenemacros.asm":
            return source.replace("dc.w \\1 ; entity to act", "dc.w \\1", 1)
        return source

    monkeypatch.setattr(map_script_engine, "read_upstream_text", annotation_altered_reader)
    with pytest.raises(ValueError, match="operand annotation drift"):
        _entity_lifecycle_presentation_macro_annotations(disasm)


def test_map_entity_lifecycle_presentation_cursor_parser_handles_comments_sizes_and_near_misses(
) -> None:
    assert _entity_lifecycle_presentation_cursor_read_use_site(
        "move.b (a6),d0 ; selector"
    ) == {
        "sourceRegister": "a6",
        "destinationOperand": "d0",
        "transferredByteCount": 1,
        "cursorAdvanceByteCount": 0,
        "instruction": "move.b (a6),d0",
    }
    assert _entity_lifecycle_presentation_cursor_read_use_site("move.w (a6)+,d2")[
        "cursorAdvanceByteCount"
    ] == 2
    assert _entity_lifecycle_presentation_cursor_read_use_site("move.l (a6)+,d7")[
        "transferredByteCount"
    ] == 4
    for near_miss in (
        "label: move.w (a6)+,d2",
        "; move.w (a6)+,d2",
        "move.q (a6)+,d2",
        "move.w d2,(a6)+",
        "move.w target(a6),d2",
    ):
        with pytest.raises(ValueError, match="cursor-read use-site drift"):
            _entity_lifecycle_presentation_cursor_read_use_site(near_miss)


def test_map_entity_lifecycle_presentation_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    sources = (map_script_engine_output, fixture)
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for source, schema_path in zip(sources, schema_paths, strict=True):
        validate_json(source, schema_path, owner="entity-lifecycle presentation baseline")
        target_path = (
            ("entityLifecyclePresentationCommandFacts",)
            if source is map_script_engine_output
            else ("expected", "entityLifecyclePresentationCommandFacts")
        )

        def target_for(value: dict, target_path: tuple[str, ...] = target_path) -> dict:
            target = value
            for key in target_path:
                target = target[key]
            return target

        missing = deepcopy(source)
        del target_for(missing)["macros"][0]["sourceOperandAnnotations"][0][
            "sourceComment"
        ]
        with pytest.raises(ValueError, match="sourceComment"):
            validate_json(missing, schema_path, owner="entity-lifecycle missing nested field")

        renamed = deepcopy(source)
        operand = target_for(renamed)["handlers"][4]["sectionGuard"]["stateWrites"][0]
        operand["target"] = operand.pop("sourceOperand")
        with pytest.raises(ValueError, match="sourceOperand"):
            validate_json(renamed, schema_path, owner="entity-lifecycle renamed nested field")

        extra = deepcopy(source)
        target_for(extra)["handlers"][7]["sectionGuard"]["bitMutationUseSites"][0][
            "extra"
        ] = True
        with pytest.raises(ValueError, match="extra"):
            validate_json(extra, schema_path, owner="entity-lifecycle extra nested field")

        reordered = deepcopy(source)
        order = target_for(reordered)["sourceSiteOrderKeys"]
        order[0], order[1] = order[1], order[0]
        with pytest.raises(ValueError, match="was expected"):
            validate_json(reordered, schema_path, owner="entity-lifecycle exact source order")

        boundary = deepcopy(source)
        target_for(boundary)["handlers"][1]["sectionGuard"][
            "aliveStatusPointerAdjustment"
        ]["adjustmentLiteralValue"] = 3
        with pytest.raises(ValueError, match="was expected"):
            validate_json(boundary, schema_path, owner="entity-lifecycle exact boundary")


def test_map_entity_lifecycle_presentation_schema_compacts_raw_corpora_and_closes_shapes() -> None:
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for path in schema_paths:
        schema = load_json(path)
        contract = schema["properties"].get("entityLifecyclePresentationCommandFacts")
        if contract is None:
            contract = schema["properties"]["expected"]["properties"][
                "entityLifecyclePresentationCommandFacts"
            ]
        exact_block = contract["allOf"][1]
        exact = (
            exact_block["const"]
            if "const" in exact_block
            else exact_block["properties"]
        )
        assert {"sourceSites", "programTotals"}.isdisjoint(exact)
        assert {"sourceSiteOrderKeys", "programTotalOrderKeys"} <= set(exact)
        definition_name = (
            "entityLifecyclePresentationCommandFacts"
            if "entityLifecyclePresentationCommandFacts" in schema["definitions"]
            else "entityLifecyclePresentationFixtureCommandFacts"
        )
        facts = schema["definitions"][definition_name]
        assert facts["additionalProperties"] is False
        if definition_name == "entityLifecyclePresentationCommandFacts":
            assert {"sourceSites", "programTotals"} <= set(facts["required"])
            source_sites = facts["properties"]["sourceSites"]
            program_totals = facts["properties"]["programTotals"]
            assert source_sites == {
                "type": "array",
                "minItems": 105,
                "maxItems": 105,
                "items": {"$ref": "#/definitions/entityLifecyclePresentationSourceSite"},
            }
            assert program_totals == {
                "type": "array",
                "minItems": 304,
                "maxItems": 304,
                "items": {"$ref": "#/definitions/entityLifecyclePresentationProgramTotal"},
            }
            for name in (
                "entityLifecyclePresentationCommand",
                "entityLifecyclePresentationSourceSite",
                "entityLifecyclePresentationProgramTotal",
            ):
                item = schema["definitions"][name]
                assert item["additionalProperties"] is False
                assert "prefixItems" not in item
                assert "const" not in item
        else:
            assert {"sourceSites", "programTotals"}.isdisjoint(facts["required"])

        def assert_closed_objects(value):
            if isinstance(value, dict):
                if value.get("type") == "object":
                    assert value.get("additionalProperties") is False
                for child in value.values():
                    assert_closed_objects(child)
            elif isinstance(value, list):
                for child in value:
                    assert_closed_objects(child)

        assert_closed_objects(facts)


def test_map_ui_command_macro_annotations_preserve_byte_operands_and_empty_comment(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    disasm = repo_path("local/upstream/SF2DISASM/disasm")
    annotations = _map_script_ui_primary_macro_annotations(disasm)
    assert annotations == {
        "showPortrait": [
            {
                "parameterOrdinal": 1,
                "sourceComment": (
                    "portrait modifier ($0-none, $40-mirrored, $80-display on right, "
                    "$FF-undisplayed)"
                ),
                "streamOffset": 2,
                "widthBytes": 1,
                "encoding": "direct",
            },
            {
                "parameterOrdinal": 2,
                "sourceComment": "entity",
                "streamOffset": 3,
                "widthBytes": 1,
                "encoding": "direct",
            },
        ],
        "hidePortrait": [],
        "menu": [
            {
                "parameterOrdinal": 1,
                "sourceComment": "",
                "streamOffset": 2,
                "widthBytes": 2,
                "encoding": "direct",
            }
        ],
    }
    original_reader = map_script_engine.read_upstream_text

    def annotation_altered_reader(path: Path) -> str:
        source = original_reader(path)
        if path.name == "sf2cutscenemacros.asm":
            prefix, marker, show_and_after = source.partition("showPortrait: macro")
            return prefix + marker + show_and_after.replace("dc.b \\2 ; entity", "dc.b \\2", 1)
        return source

    monkeypatch.setattr(map_script_engine, "read_upstream_text", annotation_altered_reader)
    with pytest.raises(ValueError, match="operand comment is missing"):
        _map_script_ui_primary_macro_annotations(disasm)


def test_map_ui_command_boundary_matches_complete_golden_fixture(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["mapScriptUiPrimaryCommandFacts"]
    assert fixture["expected"]["mapScriptUiPrimaryCommandFacts"] == {
        key: actual[key]
        for key in fixture["expected"]["mapScriptUiPrimaryCommandFacts"]
    }
    assert [
        (
            row["name"],
            row["opcode"],
            row["encodedBytes"],
            row["operandBytes"],
            row["sourceCommandCount"],
            row["handler"],
        )
        for row in actual["macros"]
    ] == [
        ("showPortrait", 29, 4, 2, 4, "csc1D_showPortrait"),
        ("hidePortrait", 30, 2, 0, 1, "csc1E_hidePortrait"),
        ("menu", 18, 4, 2, 0, "csc12_executeContextMenu"),
    ]
    assert (len(actual["sourceSites"]), len(actual["sourceSiteOrderKeys"])) == (4, 5)
    assert actual["sourceSitesSha256"] == (
        "FDF32E72E55D28E7EBC57BB5963658F6A4B10DE7C1920A2A69F75D1A90D4CC4A"
    )
    assert (len(actual["programTotals"]), len(actual["programTotalOrderKeys"])) == (304, 304)
    assert actual["programTotalsSha256"] == (
        "63EBE7909405F52FAD4D9C4E24050213E107CD9ABCBF7A709A4A7AA9F4F5EA1D"
    )
    assert [
        (row["macro"], row["handler"], row["sourcePath"], row["address"], row["statementCount"])
        for row in actual["handlers"]
    ] == [
        (
            "showPortrait",
            "csc1D_showPortrait",
            "code/common/scripting/map/mapscriptengine_1.asm",
            289432,
            20,
        ),
        (
            "hidePortrait",
            "csc1E_hidePortrait",
            "code/common/scripting/map/mapscriptengine_1.asm",
            289490,
            3,
        ),
        (
            "menu",
            "csc12_executeContextMenu",
            "code/common/scripting/map/mapscriptengine_2.asm",
            292022,
            13,
        ),
    ]
    assert actual["handlers"][2]["sectionGuard"]["stackPointerTransferInstructions"] == [
        "move.l a6,-(sp)",
        "movea.l (sp)+,a6",
    ]
    assert actual["portraitHelperJoin"] == {
        "sourceFactPath": "dialogueCommandFacts.portraitHelper",
        "macro": "showPortrait",
        "handler": "csc1D_showPortrait",
        "handlerAddress": 289432,
        "sourcePath": "code/common/scripting/map/mapscriptengine_1.asm",
        "macroOperandWidths": [1, 1],
        "macroOperandByteCount": 2,
        "handlerModifierEntityWordRead": "move.w (a6)+,d0",
        "handlerTestedModifierByteMask": 192,
        "modifierBitTests": [
            {"bit": 15, "destination": "d3"},
            {"bit": 14, "destination": "d4"},
        ],
    }
    instruction_totals = {
        "WaitForViewScrollEnd": 2,
        "GetEntityPortaitAndSpeechSfx": 1,
        "j_OpenPortraitWindow": 1,
        "j_ClosePortraitWindow": 1,
        "j_ChurchMenu": 1,
        "j_ShopMenu": 1,
        "j_BlacksmithMenu": 1,
    }
    assert actual["callerBreakdown"]["instructionTargetTotals"] == instruction_totals
    assert actual["callerBreakdown"]["externalInstructionTargetTotals"] == instruction_totals
    assert actual["callerBreakdown"]["internalInstructionTargetTotals"] == {
        target: 0 for target in instruction_totals
    }
    assert actual["runtimeQuestions"] == ["map-script-ui-command/runtime-effects-matrix"]


def test_map_ui_command_boundary_source_guards_and_portrait_join_reject_drift(
    map_script_engine_output: dict,
) -> None:
    disasm = repo_path("local/upstream/SF2DISASM/disasm")
    equates = map_script_engine._source_equates(disasm)
    facts = map_script_engine_output["mapScriptUiPrimaryCommandFacts"]
    source_handlers = {row["name"]: row for row in map_script_engine_output["handlers"]}
    for handler in facts["handlers"]:
        statements = map_script_engine._stable_handler_statements(
            disasm, source_handlers[handler["handler"]]
        )
        guard = _map_script_ui_primary_section_guard(
            handler["macro"], statements, equates
        )
        for field in (
            "orderedInstructions",
            "scriptCursorReadUseSites",
            "sourceImmediateUseSites",
            "sourceOperandInstructions",
            "stackPointerTransferInstructions",
            "directCallOrder",
            "returnInstruction",
        ):
            assert guard[field] == handler["sectionGuard"][field]
    guarded_handlers = {row["macro"]: row for row in facts["handlers"]}
    for macro, original, replacement in (
        ("showPortrait", "btst #$F,d0", "btst #$D,d0"),
        ("hidePortrait", "jsr j_ClosePortraitWindow", "jsr j_OpenPortraitWindow"),
        ("menu", "cmpi.w #2,d0", "cmpi.w #3,d0"),
    ):
        handler = guarded_handlers[macro]
        statements = map_script_engine._stable_handler_statements(
            disasm, source_handlers[handler["handler"]]
        )
        with pytest.raises(ValueError, match="statement is missing"):
            _map_script_ui_primary_section_guard(
                macro,
                [statement.replace(original, replacement) for statement in statements],
                equates,
            )
    portrait_helper = deepcopy(map_script_engine_output["dialogueCommandFacts"]["portraitHelper"])
    portrait_helper["modifierEntityWordRead"] = "move.b (a6)+,d0"
    with pytest.raises(ValueError, match="portrait-helper provenance join drift"):
        _map_script_ui_primary_portrait_helper_join(
            map_script_engine_output["macroContracts"]["showPortrait"], portrait_helper
        )


def test_map_ui_command_boundary_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    sources = (map_script_engine_output, fixture)
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for source, schema_path in zip(sources, schema_paths, strict=True):
        validate_json(source, schema_path, owner="map UI command boundary baseline")
        target_path = (
            ("mapScriptUiPrimaryCommandFacts",)
            if source is map_script_engine_output
            else ("expected", "mapScriptUiPrimaryCommandFacts")
        )

        def target_for(value: dict, target_path: tuple[str, ...] = target_path) -> dict:
            target = value
            for key in target_path:
                target = target[key]
            return target

        missing = deepcopy(source)
        del target_for(missing)["macros"][0]["sourceOperandAnnotations"][0]["encoding"]
        with pytest.raises(ValueError, match="encoding"):
            validate_json(missing, schema_path, owner="map UI missing nested")

        renamed = deepcopy(source)
        handler = target_for(renamed)["handlers"][2]
        handler["path"] = handler.pop("sourcePath")
        with pytest.raises(ValueError, match="sourcePath"):
            validate_json(renamed, schema_path, owner="map UI renamed nested")

        extra = deepcopy(source)
        target_for(extra)["portraitHelperJoin"]["modifierBitTests"][0]["extra"] = True
        with pytest.raises(ValueError, match="extra"):
            validate_json(extra, schema_path, owner="map UI extra nested")

        reordered = deepcopy(source)
        order = target_for(reordered)["sourceSiteOrderKeys"]
        order[0], order[1] = order[1], order[0]
        with pytest.raises(ValueError, match="was expected"):
            validate_json(reordered, schema_path, owner="map UI exact source order")

        boundary = deepcopy(source)
        target_for(boundary)["macros"][2]["sourceCommandCount"] = 1
        with pytest.raises(ValueError, match="was expected"):
            validate_json(boundary, schema_path, owner="map UI zero-use boundary")


def test_map_ui_command_boundary_schema_compacts_raw_corpora_and_closes_shapes() -> None:
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for path in schema_paths:
        schema = load_json(path)
        contract = schema["properties"].get("mapScriptUiPrimaryCommandFacts")
        if contract is None:
            contract = schema["properties"]["expected"]["properties"][
                "mapScriptUiPrimaryCommandFacts"
            ]
        exact = contract["allOf"][1]["properties"]
        assert {"sourceSites", "programTotals"}.isdisjoint(exact)
        assert {"sourceSiteOrderKeys", "programTotalOrderKeys"} <= set(exact)
        definition_name = (
            "mapScriptUiPrimaryCommandFacts"
            if "mapScriptUiPrimaryCommandFacts" in schema["definitions"]
            else "mapScriptUiPrimaryFixtureCommandFacts"
        )
        facts = schema["definitions"][definition_name]
        assert facts["additionalProperties"] is False
        if definition_name == "mapScriptUiPrimaryCommandFacts":
            assert facts["properties"]["sourceSites"] == {
                "type": "array",
                "minItems": 4,
                "maxItems": 4,
                "items": {"$ref": "#/definitions/mapScriptUiPrimarySourceSite"},
            }
            assert facts["properties"]["programTotals"] == {
                "type": "array",
                "minItems": 304,
                "maxItems": 304,
                "items": {"$ref": "#/definitions/mapScriptUiPrimaryProgramTotal"},
            }
            command = schema["definitions"]["mapScriptUiPrimaryCommand"]
            operand_value = command["properties"]["operandValues"]["items"]
            assert operand_value["additionalProperties"] is False
            assert set(operand_value["required"]) >= {"encoding", "resolution", "resolvedValue"}
            assert operand_value["properties"]["resolvedValue"] == {
                "type": ["integer", "null"]
            }
        else:
            assert {"sourceSites", "programTotals"}.isdisjoint(facts["required"])

        def assert_closed_objects(value):
            if isinstance(value, dict):
                if value.get("type") == "object":
                    assert value.get("additionalProperties") is False
                for child in value.values():
                    assert_closed_objects(child)
            elif isinstance(value, list):
                for child in value:
                    assert_closed_objects(child)

        assert_closed_objects(facts)


def test_map_entity_gesture_relationship_motion_contract_matches_complete_golden_fixture(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["entityGestureRelationshipMotionCommandFacts"]
    assert fixture["expected"]["entityGestureRelationshipMotionCommandFacts"] == {
        key: actual[key]
        for key in fixture["expected"]["entityGestureRelationshipMotionCommandFacts"]
    }
    assert [
        (
            row["name"],
            row["opcode"],
            row["encodedBytes"],
            row["operandBytes"],
            row["sourceCommandCount"],
            row["handler"],
        )
        for row in actual["macros"]
    ] == [
        ("shiver", 42, 4, 2, 191, "csc2A_entityShiver"),
        ("nod", 38, 4, 2, 169, "csc26_entityNodHead"),
        ("followEntity", 44, 8, 6, 160, "csc2C_followEntity"),
        ("faceEntity", 82, 6, 4, 15, "csc52_faceEntity"),
        ("moveNextToPlayer", 40, 6, 4, 7, "csc28_moveEntityNextToPlayer"),
        ("fly", 47, 6, 4, 2, "csc2F_fly"),
        ("moveEntityAboveAnother", 49, 6, 4, 1, "csc31_moveEntityAboveEntity"),
    ]
    assert sum(row["sourceCommandCount"] for row in actual["macros"]) == 545
    assert len(actual["sourceSites"]) == 133
    assert len(actual["sourceSiteOrderKeys"]) == 545
    assert actual["sourceSitesSha256"] == (
        "A8EAB146BD07272B5D63DD1ADE4FF4BCF941B0D169E9FEDB92B0F70DE55DE022"
    )
    assert len(actual["programTotals"]) == 304
    assert actual["programTotalsSha256"] == (
        "62D7A6F5A4A7FF8ABA021555F3FF3BAD8B96F6F5A67910FEF257FC7E76CDAFB8"
    )
    assert [
        (
            row["macro"],
            row["handler"],
            row["address"],
            row["opcode"],
            row["sourceCommandCount"],
            row["statementCount"],
        )
        for row in actual["handlers"]
    ] == [
        ("shiver", "csc2A_entityShiver", 290286, 42, 191, 19),
        ("nod", "csc26_entityNodHead", 289904, 38, 169, 18),
        ("followEntity", "csc2C_followEntity", 290392, 44, 160, 19),
        ("faceEntity", "csc52_faceEntity", 290648, 82, 15, 33),
        ("moveNextToPlayer", "csc28_moveEntityNextToPlayer", 290064, 40, 7, 44),
        ("fly", "csc2F_fly", 290472, 47, 2, 8),
        (
            "moveEntityAboveAnother",
            "csc31_moveEntityAboveEntity",
            290864,
            49,
            1,
            9,
        ),
    ]
    assert actual["handlers"][0]["sectionGuard"]["bitMutationUseSites"] == [
        {
            "operation": "or-immediate",
            "immediateText": "%1000",
            "immediateValue": 8,
            "instruction": "ori.b #%1000,ENTITYDEF_OFFSET_FLAGS_B(a5)",
        },
        {
            "operation": "and-immediate",
            "immediateText": "%11110111",
            "immediateValue": 247,
            "instruction": "andi.b #%11110111,ENTITYDEF_OFFSET_FLAGS_B(a5)",
        },
    ]
    assert actual["handlers"][2]["sectionGuard"]["aliveStatusPointerAdjustment"] == {
        "selectorPreReadUseSite": {
            "sourceRegister": "a6",
            "destinationOperand": "d0",
            "transferredByteCount": 1,
            "cursorAdvanceByteCount": 0,
            "instruction": "move.b (a6),d0",
        },
        "adjustmentLiteralInstruction": "moveq #6,d7",
        "adjustmentLiteralText": "6",
        "adjustmentLiteralValue": 6,
        "callInstruction": "bsr.w AdjustScriptPointerByCharacterAliveStatus",
    }
    assert actual["handlers"][4]["sectionGuard"]["sourceConstantUseSites"] == [
        {
            "symbol": "MAP_TILE_SIZE",
            "value": 384,
            "instruction": instruction,
        }
        for instruction in (
            "addi.w #MAP_TILE_SIZE,d1",
            "subi.w #MAP_TILE_SIZE,d2",
            "subi.w #MAP_TILE_SIZE,d1",
            "addi.w #MAP_TILE_SIZE,d2",
        )
    ] + [
        {"symbol": "UP", "value": 1, "instruction": "cmpi.b #UP,d3"},
        {"symbol": "LEFT", "value": 2, "instruction": "cmpi.b #LEFT,d3"},
        {
            "symbol": "DIRECTION_MASK",
            "value": 3,
            "instruction": "andi.b #DIRECTION_MASK,d3",
        },
    ]
    assert actual["callerBreakdown"]["instructionTargetTotals"] == {
        "GetEntityAddressFromCharacter": 11,
        "UpdateEntitySprite_0": 5,
        "Sleep": 5,
        "LoadMapsprite": 1,
        "sub_45D70": 1,
        "DmaMapsprite": 1,
        "AdjustScriptPointerByCharacterAliveStatus": 1,
        "AddFollower": 2,
        "WaitForVInt": 1,
        "WaitForEntityToStopMoving": 2,
    }
    assert actual["callerBreakdown"]["effectiveTargetTotals"] == actual[
        "callerBreakdown"
    ]["instructionTargetTotals"]
    for field in (
        "internalInstructionTargetTotals",
        "internalEffectiveTargetTotals",
    ):
        assert actual["callerBreakdown"][field] == {
            target: 0 for target in actual["callerBreakdown"]["instructionTargetTotals"]
        }
    assert actual["runtimeQuestions"] == [
        "map-script-entity-gesture-relationship-motion/normal-story-reachability",
        "map-script-entity-gesture-relationship-motion/full-entity-state-callback-effects",
        "map-script-entity-gesture-relationship-motion/player-visible-presentation-timing-collision-persistence",
    ]


def test_map_entity_gesture_relationship_motion_source_guards_reject_local_drift(
    map_script_engine_output: dict, monkeypatch: pytest.MonkeyPatch
) -> None:
    disasm = repo_path("local/upstream/SF2DISASM/disasm")
    equates = map_script_engine._source_equates(disasm)
    facts = map_script_engine_output["entityGestureRelationshipMotionCommandFacts"]
    annotations = _entity_gesture_relationship_motion_macro_annotations(disasm)
    assert annotations["moveEntityAboveAnother"] == facts["macros"][-1][
        "sourceOperandAnnotations"
    ]
    source_handlers = {row["name"]: row for row in map_script_engine_output["handlers"]}
    guarded_handlers = {row["macro"]: row for row in facts["handlers"]}
    for macro, handler in guarded_handlers.items():
        statements = map_script_engine._stable_handler_statements(
            disasm, source_handlers[handler["handler"]]
        )
        guard = _entity_gesture_relationship_motion_section_guard(
            macro, statements, equates
        )
        assert guard["orderedInstructions"] == handler["sectionGuard"][
            "orderedInstructions"
        ]
        for field in (
            "scriptCursorReadUseSites",
            "aliveStatusPointerAdjustment",
            "sourceConstantUseSites",
            "sourceOperandInstructions",
            "sourceImmediateInstructions",
            "bitMutationUseSites",
            "directCallOrder",
            "returnInstruction",
        ):
            assert guard[field] == handler["sectionGuard"][field]
        section_source = map_script_engine._map_camera_control_named_section_source(
            disasm,
            "code/common/scripting/map/mapscriptengine_1.asm",
            handler["handler"],
        )
        assert [
            {
                "branchInstruction": row["branchInstruction"],
                "branchTarget": _entity_gesture_relationship_motion_branch_target_record(
                    section_source,
                    row["branchInstruction"],
                    row["expectedTargetInstruction"],
                    guard["orderedInstructions"],
                ),
            }
            for row in guard["branchRecords"]
        ] == handler["sectionGuard"]["branchRecords"]
        assert [
            {
                "loopInstruction": row["loopInstruction"],
                "loopTarget": {
                    "counterRegister": "d7",
                    "loopInstruction": row["loopInstruction"],
                    **_entity_gesture_relationship_motion_branch_target_record(
                        section_source,
                        f"bra.s {row['loopInstruction'].split(',', 1)[1]}",
                        row["expectedTargetInstruction"],
                        guard["orderedInstructions"],
                    ),
                },
            }
            for row in guard["loopRecords"]
        ] == handler["sectionGuard"]["loopRecords"]
        assert sum(
            row["cursorAdvanceByteCount"] for row in guard["scriptCursorReadUseSites"]
        ) == next(row["operandBytes"] for row in facts["macros"] if row["name"] == macro)
    mutations = (
        ("shiver", "ori.b #%1000", "ori.b #%0100"),
        ("nod", "dbf d7,loc_46C8A", "dbf d7,loc_46C8C"),
        ("followEntity", "moveq #6,d7", "moveq #5,d7"),
        ("faceEntity", "bcs.s @Face_Up", "bcc.s @Face_Up"),
        ("moveNextToPlayer", "addi.w #MAP_TILE_SIZE,d1", "addi.w #MAP_TILE_SIZE,d2"),
        ("fly", "bne.s loc_46EB8", "beq.s loc_46EB8"),
        ("moveEntityAboveAnother", "jsr AddFollower", "jsr Sleep"),
    )
    for macro, original, replacement in mutations:
        handler = guarded_handlers[macro]
        statements = map_script_engine._stable_handler_statements(
            disasm, source_handlers[handler["handler"]]
        )
        with pytest.raises(ValueError, match="statement is missing"):
            _entity_gesture_relationship_motion_section_guard(
                macro,
                [statement.replace(original, replacement) for statement in statements],
                equates,
            )
    reordered = map_script_engine._stable_handler_statements(
        disasm, source_handlers["csc2A_entityShiver"]
    )
    reordered[7], reordered[8] = reordered[8], reordered[7]
    with pytest.raises(ValueError, match="statement is missing"):
        _entity_gesture_relationship_motion_section_guard("shiver", reordered, equates)
    section_source = map_script_engine._map_camera_control_named_section_source(
        disasm,
        "code/common/scripting/map/mapscriptengine_1.asm",
        "csc52_faceEntity",
    )
    with pytest.raises(ValueError, match="branch target label is missing"):
        _entity_gesture_relationship_motion_branch_target_record(
            section_source.replace("@Face_Up:", "@FaceUp:"),
            "bcs.s @Face_Up",
            "move.b #UP,ENTITYDEF_OFFSET_FACING(a5)",
            guarded_handlers["faceEntity"]["sectionGuard"]["orderedInstructions"],
        )
    original_reader = map_script_engine.read_upstream_text

    def annotation_altered_reader(path: Path) -> str:
        source = original_reader(path)
        if path.name == "sf2cutscenemacros.asm":
            prefix, marker, shiver_and_after = source.partition("shiver: macro")
            return prefix + marker + shiver_and_after.replace(
                "dc.w \\1 ; entity to act", "dc.w \\1", 1
            )
        return source

    monkeypatch.setattr(map_script_engine, "read_upstream_text", annotation_altered_reader)
    with pytest.raises(ValueError, match="operand comment is missing"):
        _entity_gesture_relationship_motion_macro_annotations(disasm)


def test_map_entity_gesture_cursor_parser_handles_comments_sizes_and_near_misses(
) -> None:
    assert _entity_gesture_relationship_motion_cursor_read_use_site(
        "move.b (a6),d0 ; selector"
    ) == {
        "sourceRegister": "a6",
        "destinationOperand": "d0",
        "transferredByteCount": 1,
        "cursorAdvanceByteCount": 0,
        "instruction": "move.b (a6),d0",
    }
    assert _entity_gesture_relationship_motion_cursor_read_use_site("move.w (a6)+,d2")[
        "cursorAdvanceByteCount"
    ] == 2
    assert _entity_gesture_relationship_motion_cursor_read_use_site("move.l (a6)+,d7")[
        "transferredByteCount"
    ] == 4
    for near_miss in (
        "label: move.w (a6)+,d2",
        "; move.w (a6)+,d2",
        "move.q (a6)+,d2",
        "move.w d2,(a6)+",
        "move.w target(a6),d2",
    ):
        with pytest.raises(ValueError, match="cursor-read use-site drift"):
            _entity_gesture_relationship_motion_cursor_read_use_site(near_miss)


def test_map_entity_gesture_relationship_motion_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    sources = (map_script_engine_output, fixture)
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for source, schema_path in zip(sources, schema_paths, strict=True):
        validate_json(source, schema_path, owner="entity gesture relationship motion baseline")
        target_path = (
            ("entityGestureRelationshipMotionCommandFacts",)
            if source is map_script_engine_output
            else ("expected", "entityGestureRelationshipMotionCommandFacts")
        )

        def target_for(value: dict, target_path: tuple[str, ...] = target_path) -> dict:
            target = value
            for key in target_path:
                target = target[key]
            return target

        missing = deepcopy(source)
        del target_for(missing)["macros"][0]["sourceOperandAnnotations"][0][
            "sourceComment"
        ]
        with pytest.raises(ValueError, match="sourceComment"):
            validate_json(missing, schema_path, owner="gesture missing nested field")

        renamed = deepcopy(source)
        direct_call = target_for(renamed)["handlers"][0]["directCalls"][0]
        direct_call["target"] = direct_call.pop("instructionTarget")
        with pytest.raises(ValueError, match="instructionTarget"):
            validate_json(renamed, schema_path, owner="gesture renamed nested field")

        extra = deepcopy(source)
        target_for(extra)["handlers"][0]["sectionGuard"]["bitMutationUseSites"][0][
            "extra"
        ] = True
        with pytest.raises(ValueError, match="extra"):
            validate_json(extra, schema_path, owner="gesture extra nested field")

        reordered = deepcopy(source)
        order = target_for(reordered)["sourceSiteOrderKeys"]
        order[0], order[1] = order[1], order[0]
        with pytest.raises(ValueError, match="was expected"):
            validate_json(reordered, schema_path, owner="gesture exact source order")

        boundary = deepcopy(source)
        target_for(boundary)["macros"][0]["sourceCommandCount"] = 192
        with pytest.raises(ValueError, match="was expected"):
            validate_json(boundary, schema_path, owner="gesture exact boundary")


def test_map_entity_gesture_relationship_motion_schema_compacts_raw_corpora_and_closes_shapes(
) -> None:
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for path in schema_paths:
        schema = load_json(path)
        contract = schema["properties"].get("entityGestureRelationshipMotionCommandFacts")
        if contract is None:
            contract = schema["properties"]["expected"]["properties"][
                "entityGestureRelationshipMotionCommandFacts"
            ]
        exact_block = contract["allOf"][1]
        exact = (
            exact_block["const"]
            if "const" in exact_block
            else exact_block["properties"]
        )
        assert {"sourceSites", "programTotals"}.isdisjoint(exact)
        assert {"sourceSiteOrderKeys", "programTotalOrderKeys"} <= set(exact)
        definition_name = (
            "entityGestureRelationshipMotionCommandFacts"
            if "entityGestureRelationshipMotionCommandFacts" in schema["definitions"]
            else "entityGestureRelationshipMotionFixtureCommandFacts"
        )
        facts = schema["definitions"][definition_name]
        assert facts["additionalProperties"] is False
        if definition_name == "entityGestureRelationshipMotionCommandFacts":
            assert {"sourceSites", "programTotals"} <= set(facts["required"])
            assert facts["properties"]["sourceSites"] == {
                "type": "array",
                "minItems": 133,
                "maxItems": 133,
                "items": {
                    "$ref": "#/definitions/entityGestureRelationshipMotionSourceSite"
                },
            }
            assert facts["properties"]["programTotals"] == {
                "type": "array",
                "minItems": 304,
                "maxItems": 304,
                "items": {
                    "$ref": "#/definitions/entityGestureRelationshipMotionProgramTotal"
                },
            }
            for name in (
                "entityGestureRelationshipMotionCommand",
                "entityGestureRelationshipMotionSourceSite",
                "entityGestureRelationshipMotionProgramTotal",
            ):
                item = schema["definitions"][name]
                assert item["additionalProperties"] is False
                assert "prefixItems" not in item
                assert "const" not in item
        else:
            assert {"sourceSites", "programTotals"}.isdisjoint(facts["required"])

        def assert_closed_objects(value):
            if isinstance(value, dict):
                if value.get("type") == "object":
                    assert value.get("additionalProperties") is False
                for child in value.values():
                    assert_closed_objects(child)
            elif isinstance(value, list):
                for child in value:
                    assert_closed_objects(child)

        assert_closed_objects(facts)


def test_map_screen_presentation_macro_annotations_preserve_only_source_operands(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    disasm = repo_path("local/upstream/SF2DISASM/disasm")
    annotations = _screen_presentation_macro_annotations(disasm)
    assert annotations["setQuake"] == [
        {
            "parameterOrdinal": 1,
            "sourceComment": "? ($4000-, $8000-",
            "streamOffset": 2,
            "widthBytes": 2,
        }
    ]
    assert annotations["flashScreenWhite"] == [
        {
            "parameterOrdinal": 1,
            "sourceComment": "duration",
            "streamOffset": 2,
            "widthBytes": 2,
        }
    ]
    assert all(
        annotations[macro] == []
        for macro in (
            "fadeInB",
            "fadeOutB",
            "slowFadeInB",
            "slowFadeOutB",
            "tintMap",
            "flickerOnce",
            "mapFadeOutToWhite",
            "mapFadeInFromWhite",
            "fadeInFromBlackHalf",
            "fadeOutToBlackHalf",
        )
    )
    original_reader = map_script_engine.read_upstream_text

    def annotation_altered_reader(path: Path) -> str:
        source = original_reader(path)
        if path.name == "sf2cutscenemacros.asm":
            prefix, marker, quake_and_after = source.partition("setQuake: macro")
            return prefix + marker + quake_and_after.replace(
                "dc.w \\1 ; ? ($4000-, $8000-", "dc.w \\1", 1
            )
        return source

    monkeypatch.setattr(map_script_engine, "read_upstream_text", annotation_altered_reader)
    with pytest.raises(ValueError, match="operand comment is missing"):
        _screen_presentation_macro_annotations(disasm)


def test_map_screen_presentation_direct_call_parser_preserves_pc_relative_form() -> None:
    assert _screen_presentation_direct_calls(
        [
            "jsr LaunchFading(pc) ; service boundary",
            "jsr (Sleep).w",
            "bsr.l NamedService",
        ]
    ) == [
        {
            "opcode": "jsr",
            "instructionTarget": "LaunchFading",
            "addressingForm": "pc-relative",
        },
        {
            "opcode": "jsr",
            "instructionTarget": "Sleep",
            "addressingForm": "direct",
        },
        {
            "opcode": "bsr",
            "instructionTarget": "NamedService",
            "addressingForm": "direct",
        },
    ]
    assert _screen_presentation_direct_calls(
        [
            "label: jsr LaunchFading(pc)",
            "; jsr LaunchFading(pc)",
            "move.w LaunchFading(pc),d0",
            "jsr a0",
            "jsr LaunchFading(pc),d0",
        ]
    ) == []


def test_map_screen_presentation_contract_matches_complete_golden_fixture(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["screenPresentationCommandFacts"]
    assert fixture["expected"]["screenPresentationCommandFacts"] == {
        key: actual[key]
        for key in fixture["expected"]["screenPresentationCommandFacts"]
    }
    assert [
        (
            row["name"],
            row["opcode"],
            row["encodedBytes"],
            row["operandBytes"],
            row["sourceCommandCount"],
            row["handler"],
        )
        for row in actual["macros"]
    ] == [
        ("setQuake", 51, 4, 2, 194, "csc33_setQuakeAmount"),
        ("fadeInB", 57, 2, 0, 98, "csc39_fadeInFromBlack"),
        ("fadeOutB", 58, 2, 0, 10, "csc3A_fadeOutToBlack"),
        ("slowFadeInB", 59, 2, 0, 1, "csc3B_slowFadeInFromBlack"),
        ("slowFadeOutB", 60, 2, 0, 0, "csc3C_slowFadeOutToBlack"),
        ("tintMap", 61, 2, 0, 11, "csc3D_tintMap"),
        ("flickerOnce", 62, 2, 0, 5, "csc3E_FlickerOnce"),
        ("mapFadeOutToWhite", 63, 2, 0, 15, "csc3F_fadeMapOutToWhite"),
        ("mapFadeInFromWhite", 64, 2, 0, 15, "csc40_fadeMapInFromWhite"),
        ("flashScreenWhite", 65, 4, 2, 96, "csc41_flashScreenWhite"),
        ("fadeInFromBlackHalf", 74, 2, 0, 8, "csc4A_fadeInFromBlackHalf"),
        ("fadeOutToBlackHalf", 75, 2, 0, 6, "csc4B_fadeOutToBlackHalf"),
    ]
    assert sum(row["sourceCommandCount"] for row in actual["macros"]) == 459
    assert len(actual["sourceSites"]) == 115
    assert len(actual["sourceSiteOrderKeys"]) == 459
    assert actual["sourceSitesSha256"] == (
        "EE24CB393511FD9640AC96E427815CBC1851B2A6384A9D045FE74CC7E28F0948"
    )
    assert len(actual["programTotals"]) == 304
    assert actual["programTotalsSha256"] == (
        "DB8AFFDF9AE1FE4B119CF916EB1F9792A383F5BD7FE6B7F95B7FD7CBE8F3107F"
    )
    assert [
        (
            row["macro"],
            row["handler"],
            row["address"],
            row["statementCount"],
        )
        for row in actual["handlers"]
    ] == [
        ("setQuake", "csc33_setQuakeAmount", 288030, 23),
        ("fadeInB", "csc39_fadeInFromBlack", 288260, 2),
        ("fadeOutB", "csc3A_fadeOutToBlack", 288266, 2),
        ("slowFadeInB", "csc3B_slowFadeInFromBlack", 288272, 5),
        ("slowFadeOutB", "csc3C_slowFadeOutToBlack", 288292, 5),
        ("tintMap", "csc3D_tintMap", 288312, 6),
        ("flickerOnce", "csc3E_FlickerOnce", 288326, 6),
        ("mapFadeOutToWhite", "csc3F_fadeMapOutToWhite", 288340, 6),
        ("mapFadeInFromWhite", "csc40_fadeMapInFromWhite", 288354, 6),
        ("flashScreenWhite", "csc41_flashScreenWhite", 288368, 10),
        ("fadeInFromBlackHalf", "csc4A_fadeInFromBlackHalf", 288648, 6),
        ("fadeOutToBlackHalf", "csc4B_fadeOutToBlackHalf", 288662, 6),
    ]
    assert actual["handlers"][0]["sectionGuard"]["sourceImmediateUseSites"] == [
        {
            "instruction": instruction,
            "rawValue": raw_value,
            "resolvedValue": value,
            "resolution": "literal",
        }
        for instruction, raw_value, value in (
            ("andi.w #$3FFF,d0", "$3FFF", 16383),
            ("subq.w #1,d7", "1", 1),
            ("btst #$F,d3", "$F", 15),
            ("moveq #0,d1", "0", 0),
            ("move.w #1,d2", "1", 1),
            ("btst #$E,d3", "$E", 14),
            ("move.w #-1,d2", "-1", -1),
            ("move.w #$28,d0", "$28", 40),
        )
    ]
    assert actual["handlers"][9]["sectionGuard"]["loopRecords"] == [
        {
            "loopInstruction": "dbf d7,loc_4667A",
            "loopTarget": {
                "counterRegister": "d7",
                "loopInstruction": "dbf d7,loc_4667A",
                "targetLabel": "loc_4667A",
                "targetInstruction": "jsr LaunchFading(pc)",
                "targetStatementIndex": 5,
            },
        }
    ]
    assert actual["handlers"][5]["directCalls"] == [
        {
            "opcode": "jsr",
            "instructionTarget": "LaunchFading",
            "addressingForm": "pc-relative",
        }
    ]
    assert actual["callerBreakdown"]["instructionTargetTotals"] == {
        "Sleep": 1,
        "FadeInFromBlack": 2,
        "FadeOutToBlack": 2,
        "LaunchFading": 7,
        "DuplicatePalettes": 1,
    }
    assert actual["callerBreakdown"]["effectiveTargetTotals"] == actual[
        "callerBreakdown"
    ]["instructionTargetTotals"]
    for field in ("internalInstructionTargetTotals", "internalEffectiveTargetTotals"):
        assert actual["callerBreakdown"][field] == {
            target: 0 for target in actual["callerBreakdown"]["instructionTargetTotals"]
        }
    assert actual["runtimeQuestions"] == [
        "map-script-screen-presentation/runtime-effects-matrix"
    ]


def test_map_screen_presentation_source_guards_reject_local_drift(
    map_script_engine_output: dict,
) -> None:
    disasm = repo_path("local/upstream/SF2DISASM/disasm")
    equates = map_script_engine._source_equates(disasm)
    facts = map_script_engine_output["screenPresentationCommandFacts"]
    source_handlers = {row["name"]: row for row in map_script_engine_output["handlers"]}
    guarded_handlers = {row["macro"]: row for row in facts["handlers"]}
    for macro, handler in guarded_handlers.items():
        statements = map_script_engine._stable_handler_statements(
            disasm, source_handlers[handler["handler"]]
        )
        guard = _screen_presentation_section_guard(macro, statements, equates)
        assert guard["orderedInstructions"] == handler["sectionGuard"]["orderedInstructions"]
        for field in (
            "scriptCursorReadUseSites",
            "sourceImmediateUseSites",
            "sourceOperandInstructions",
            "directCallOrder",
            "returnInstruction",
        ):
            assert guard[field] == handler["sectionGuard"][field]
        section_source = map_script_engine._map_camera_control_named_section_source(
            disasm,
            "code/common/scripting/map/mapscriptengine_1.asm",
            handler["handler"],
        )
        assert [
            {
                "branchInstruction": row["branchInstruction"],
                "branchTarget": _screen_presentation_branch_target_record(
                    section_source,
                    row["branchInstruction"],
                    row["expectedTargetInstruction"],
                    guard["orderedInstructions"],
                ),
            }
            for row in guard["branchRecords"]
        ] == handler["sectionGuard"]["branchRecords"]
        assert [
            {
                "loopInstruction": row["loopInstruction"],
                "loopTarget": {
                    "counterRegister": "d7",
                    "loopInstruction": row["loopInstruction"],
                    **_screen_presentation_branch_target_record(
                        section_source,
                        f"bra.s {row['loopInstruction'].split(',', 1)[1]}",
                        row["expectedTargetInstruction"],
                        guard["orderedInstructions"],
                    ),
                },
            }
            for row in guard["loopRecords"]
        ] == handler["sectionGuard"]["loopRecords"]
    mutations = (
        ("setQuake", "andi.w #$3FFF,d0", "andi.w #$7FFF,d0"),
        ("fadeInB", "jsr (FadeInFromBlack).w", "jsr (FadeOutToBlack).w"),
        ("fadeOutB", "jsr (FadeOutToBlack).w", "jsr (FadeInFromBlack).w"),
        ("slowFadeInB", "move.b #6", "move.b #5"),
        ("slowFadeOutB", "move.b #6", "move.b #5"),
        ("tintMap", "#HALF_OUT_TO_BLACK", "#FLICKER_ONCE"),
        ("flickerOnce", "#FLICKER_ONCE", "#HALF_OUT_TO_BLACK"),
        ("mapFadeOutToWhite", "#OUT_TO_WHITE", "#IN_FROM_WHITE"),
        ("mapFadeInFromWhite", "#IN_FROM_WHITE", "#OUT_TO_WHITE"),
        ("flashScreenWhite", "lsr.w #3,d7", "lsr.w #2,d7"),
        ("fadeInFromBlackHalf", "#HALF_IN_FROM_BLACK", "#OUT_TO_BLACK_2"),
        ("fadeOutToBlackHalf", "#OUT_TO_BLACK_2", "#HALF_IN_FROM_BLACK"),
    )
    for macro, original, replacement in mutations:
        handler = guarded_handlers[macro]
        statements = map_script_engine._stable_handler_statements(
            disasm, source_handlers[handler["handler"]]
        )
        with pytest.raises(ValueError, match="statement is missing"):
            _screen_presentation_section_guard(
                macro,
                [statement.replace(original, replacement) for statement in statements],
                equates,
            )
    section_source = map_script_engine._map_camera_control_named_section_source(
        disasm,
        "code/common/scripting/map/mapscriptengine_1.asm",
        "csc33_setQuakeAmount",
    )
    with pytest.raises(ValueError, match="branch target label is missing"):
        _screen_presentation_branch_target_record(
            section_source.replace("loc_46546:", "loc_46547:"),
            "beq.s loc_46546",
            "move.w d0,(QUAKE_AMPLITUDE).l",
            guarded_handlers["setQuake"]["sectionGuard"]["orderedInstructions"],
        )


def test_map_screen_presentation_cursor_parser_handles_comments_sizes_and_near_misses() -> None:
    assert _screen_presentation_cursor_read_use_site("move.b (a6),d0 ; selector") == {
        "sourceRegister": "a6",
        "destinationOperand": "d0",
        "transferredByteCount": 1,
        "cursorAdvanceByteCount": 0,
        "instruction": "move.b (a6),d0",
    }
    assert _screen_presentation_cursor_read_use_site("move.w (a6)+,d2")[
        "cursorAdvanceByteCount"
    ] == 2
    assert _screen_presentation_cursor_read_use_site("move.l (a6)+,d7")[
        "transferredByteCount"
    ] == 4
    for near_miss in (
        "label: move.w (a6)+,d2",
        "; move.w (a6)+,d2",
        "move.q (a6)+,d2",
        "move.w d2,(a6)+",
        "move.w target(a6),d2",
    ):
        with pytest.raises(ValueError, match="cursor-read use-site drift"):
            _screen_presentation_cursor_read_use_site(near_miss)


def test_map_screen_presentation_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    sources = (map_script_engine_output, fixture)
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for source, schema_path in zip(sources, schema_paths, strict=True):
        validate_json(source, schema_path, owner="screen presentation baseline")
        target_path = (
            ("screenPresentationCommandFacts",)
            if source is map_script_engine_output
            else ("expected", "screenPresentationCommandFacts")
        )

        def target_for(value: dict, target_path: tuple[str, ...] = target_path) -> dict:
            target = value
            for key in target_path:
                target = target[key]
            return target

        missing = deepcopy(source)
        del target_for(missing)["macros"][0]["sourceOperandAnnotations"][0][
            "sourceComment"
        ]
        with pytest.raises(ValueError, match="sourceComment"):
            validate_json(missing, schema_path, owner="screen presentation missing nested")

        renamed = deepcopy(source)
        direct_call = target_for(renamed)["handlers"][5]["directCalls"][0]
        direct_call["addressing"] = direct_call.pop("addressingForm")
        with pytest.raises(ValueError, match="addressingForm"):
            validate_json(renamed, schema_path, owner="screen presentation renamed nested")

        extra = deepcopy(source)
        target_for(extra)["handlers"][0]["sectionGuard"]["sourceImmediateUseSites"][0][
            "extra"
        ] = True
        with pytest.raises(ValueError, match="extra"):
            validate_json(extra, schema_path, owner="screen presentation extra nested")

        reordered = deepcopy(source)
        order = target_for(reordered)["sourceSiteOrderKeys"]
        order[0], order[1] = order[1], order[0]
        with pytest.raises(ValueError, match="was expected"):
            validate_json(reordered, schema_path, owner="screen presentation exact source order")

        boundary = deepcopy(source)
        target_for(boundary)["macros"][0]["sourceCommandCount"] = 195
        with pytest.raises(ValueError, match="was expected"):
            validate_json(boundary, schema_path, owner="screen presentation exact boundary")


def test_map_screen_presentation_schema_compacts_raw_corpora_and_closes_shapes() -> None:
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for path in schema_paths:
        schema = load_json(path)
        contract = schema["properties"].get("screenPresentationCommandFacts")
        if contract is None:
            contract = schema["properties"]["expected"]["properties"][
                "screenPresentationCommandFacts"
            ]
        exact_block = contract["allOf"][1]
        exact = (
            exact_block["const"]
            if "const" in exact_block
            else exact_block["properties"]
        )
        assert {"sourceSites", "programTotals"}.isdisjoint(exact)
        assert {"sourceSiteOrderKeys", "programTotalOrderKeys"} <= set(exact)
        definition_name = (
            "screenPresentationCommandFacts"
            if "screenPresentationCommandFacts" in schema["definitions"]
            else "screenPresentationFixtureCommandFacts"
        )
        facts = schema["definitions"][definition_name]
        assert facts["additionalProperties"] is False
        if definition_name == "screenPresentationCommandFacts":
            assert {"sourceSites", "programTotals"} <= set(facts["required"])
            assert facts["properties"]["sourceSites"] == {
                "type": "array",
                "minItems": 115,
                "maxItems": 115,
                "items": {"$ref": "#/definitions/screenPresentationSourceSite"},
            }
            assert facts["properties"]["programTotals"] == {
                "type": "array",
                "minItems": 304,
                "maxItems": 304,
                "items": {"$ref": "#/definitions/screenPresentationProgramTotal"},
            }
            for name in (
                "screenPresentationCommand",
                "screenPresentationSourceSite",
                "screenPresentationProgramTotal",
            ):
                item = schema["definitions"][name]
                assert item["additionalProperties"] is False
                assert "prefixItems" not in item
                assert "const" not in item
        else:
            assert {"sourceSites", "programTotals"}.isdisjoint(facts["required"])

        def assert_closed_objects(value):
            if isinstance(value, dict):
                if value.get("type") == "object":
                    assert value.get("additionalProperties") is False
                for child in value.values():
                    assert_closed_objects(child)
            elif isinstance(value, list):
                for child in value:
                    assert_closed_objects(child)

        assert_closed_objects(facts)


def test_map_entity_presentation_fx_macro_annotations_preserve_shorthand_encoding(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    disasm = repo_path("local/upstream/SF2DISASM/disasm")
    annotations = _entity_presentation_fx_macro_annotations(disasm)
    assert annotations["animEntityFX"] == [
        {
            "parameterOrdinal": 1,
            "sourceComment": "entity to act",
            "streamOffset": 2,
            "widthBytes": 2,
            "encoding": "direct",
        },
        {
            "parameterOrdinal": 2,
            "sourceComment": "transition type",
            "streamOffset": 4,
            "widthBytes": 2,
            "encoding": "shorthand:ENTITY_TRANSITION_",
        },
    ]
    assert annotations["headshake"] == [
        {
            "parameterOrdinal": 1,
            "sourceComment": "entity to act",
            "streamOffset": 2,
            "widthBytes": 2,
            "encoding": "direct",
        }
    ]
    assert annotations["entityFlashWhite"][1]["sourceComment"] == "duration"
    original_reader = map_script_engine.read_upstream_text

    def annotation_altered_reader(path: Path) -> str:
        source = original_reader(path)
        if path.name == "sf2cutscenemacros.asm":
            prefix, marker, fx_and_after = source.partition("animEntityFX: macro")
            return prefix + marker + fx_and_after.replace(
                "defineShorthand.w ENTITY_TRANSITION_,\\2 ; transition type",
                "defineShorthand.w ENTITY_TRANSITION_,\\2",
                1,
            )
        return source

    monkeypatch.setattr(map_script_engine, "read_upstream_text", annotation_altered_reader)
    with pytest.raises(ValueError, match="operand comment is missing"):
        _entity_presentation_fx_macro_annotations(disasm)


def test_map_entity_presentation_fx_contract_matches_complete_golden_fixture(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    actual = map_script_engine_output["entityPresentationFxCommandFacts"]
    assert fixture["expected"]["entityPresentationFxCommandFacts"] == {
        key: actual[key]
        for key in fixture["expected"]["entityPresentationFxCommandFacts"]
    }
    assert [
        (
            row["name"],
            row["opcode"],
            row["encodedBytes"],
            row["operandBytes"],
            row["sourceCommandCount"],
            row["handler"],
        )
        for row in actual["macros"]
    ] == [
        ("animEntityFX", 34, 6, 4, 66, "csc22_animateEntityFadeInOrOut"),
        ("headshake", 39, 4, 2, 63, "csc27_entityShakeHead"),
        ("entityFlashWhite", 24, 6, 4, 48, "csc18_flashEntityWhite"),
    ]
    assert sum(row["sourceCommandCount"] for row in actual["macros"]) == 177
    assert (len(actual["sourceSites"]), len(actual["sourceSiteOrderKeys"])) == (61, 177)
    assert actual["sourceSitesSha256"] == (
        "A5A1424438C21C3A3B7602F8537851AD559F1193E72B5D998AF184BED04B4738"
    )
    assert (len(actual["programTotals"]), len(actual["programTotalOrderKeys"])) == (304, 304)
    assert actual["programTotalsSha256"] == (
        "921183412DB9E4E0BE1CAE4960A9702CD410BB85886CD92C967EED89AAE2CDB0"
    )
    assert [
        (row["macro"], row["handler"], row["address"], row["statementCount"])
        for row in actual["handlers"]
    ] == [
        ("animEntityFX", "csc22_animateEntityFadeInOrOut", 289602, 31),
        ("headshake", "csc27_entityShakeHead", 289972, 22),
        ("entityFlashWhite", "csc18_flashEntityWhite", 289246, 14),
    ]
    assert actual["handlers"][0]["sectionGuard"]["branchRecords"] == [
        {
            "branchInstruction": "beq.w loc_46BE2",
            "branchTarget": {
                "targetLabel": "loc_46BE2",
                "targetInstruction": "tst.w d1",
                "targetStatementIndex": 0,
                "targetSectionAnchor": "loc_46BE2",
            },
        },
        {
            "branchInstruction": "beq.w loc_46BE2",
            "branchTarget": {
                "targetLabel": "loc_46BE2",
                "targetInstruction": "tst.w d1",
                "targetStatementIndex": 0,
                "targetSectionAnchor": "loc_46BE2",
            },
        },
        {
            "branchInstruction": "bne.s @Return",
            "branchTarget": {
                "targetLabel": "@Return",
                "targetInstruction": "rts",
                "targetStatementIndex": 30,
                "targetSectionAnchor": "csc22_animateEntityFadeInOrOut",
            },
        },
    ]
    assert actual["handlers"][2]["sectionGuard"]["loopRecords"] == [
        {
            "loopInstruction": "dbf d7,loc_469E8",
            "loopTarget": {
                "counterRegister": "d7",
                "loopInstruction": "dbf d7,loc_469E8",
                "targetLabel": "loc_469E8",
                "targetInstruction": "ori.b #%100,ENTITYDEF_OFFSET_FLAGS_B(a5)",
                "targetStatementIndex": 4,
                "targetSectionAnchor": "csc18_flashEntityWhite",
            },
        }
    ]
    target_totals = {
        "GetEntityAddressFromCharacter": 3,
        "LoadMapsprite": 4,
        "ApplySpriteCropEffect": 1,
        "DmaMapsprite": 4,
        "WaitForVInt": 12,
        "sub_45E10": 1,
        "sub_45D1C": 1,
        "UpdateEntitySprite_0": 4,
        "sub_45D46": 1,
    }
    assert actual["callerBreakdown"]["instructionTargetTotals"] == target_totals
    assert actual["callerBreakdown"]["effectiveTargetTotals"] == target_totals
    assert actual["callerBreakdown"]["internalInstructionTargetTotals"] == {
        target: 0 for target in target_totals
    }
    assert actual["callerBreakdown"]["externalEffectiveTargetTotals"] == target_totals
    assert actual["runtimeQuestions"] == [
        "map-script-entity-presentation-fx/runtime-effects-matrix"
    ]


def test_map_entity_presentation_fx_source_guards_reject_local_drift(
    map_script_engine_output: dict,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    disasm = repo_path("local/upstream/SF2DISASM/disasm")
    equates = map_script_engine._source_equates(disasm)
    facts = map_script_engine_output["entityPresentationFxCommandFacts"]
    source_handlers = {row["name"]: row for row in map_script_engine_output["handlers"]}
    for handler in facts["handlers"]:
        statements = map_script_engine._stable_handler_statements(
            disasm, source_handlers[handler["handler"]]
        )
        guard = _entity_presentation_fx_section_guard(
            handler["macro"], statements, equates
        )
        for field in (
            "orderedInstructions",
            "scriptCursorReadUseSites",
            "sourceImmediateUseSites",
            "sourceOperandInstructions",
            "directCallOrder",
            "returnInstruction",
        ):
            assert guard[field] == handler["sectionGuard"][field]
    mutations = (
        ("animEntityFX", "lsl.w #3,d0", "lsl.w #2,d0"),
        ("headshake", "moveq #6,d7", "moveq #5,d7"),
        ("entityFlashWhite", "lsr.w #2,d7", "lsr.w #3,d7"),
    )
    guarded_handlers = {row["macro"]: row for row in facts["handlers"]}
    for macro, original, replacement in mutations:
        handler = guarded_handlers[macro]
        statements = map_script_engine._stable_handler_statements(
            disasm, source_handlers[handler["handler"]]
        )
        with pytest.raises(ValueError, match="statement is missing"):
            _entity_presentation_fx_section_guard(
                macro,
                [statement.replace(original, replacement) for statement in statements],
                equates,
            )
    original_reader = map_script_engine.read_upstream_text

    def chunk_mutating_reader(path: Path) -> str:
        source = original_reader(path)
        if path.name == "mapscriptengine_1.asm":
            return source.replace("tst.w   d1              ; manage param 6/7", "tst.w   d2", 1)
        return source

    monkeypatch.setattr(map_script_engine, "read_upstream_text", chunk_mutating_reader)
    with pytest.raises(ValueError, match="function-chunk target instruction drift"):
        _entity_presentation_fx_function_chunk_target_record(
            disasm,
            "csc22_animateEntityFadeInOrOut",
            "beq.w loc_46BE2",
            "tst.w d1",
        )


def test_map_entity_presentation_fx_direct_call_parser_and_resolution_guard(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    assert _entity_presentation_fx_direct_calls(
        [
            "bsr.w NamedService ; comment",
            "jsr (OtherService).l",
            "jsr ThirdService(pc)",
        ]
    ) == [
        {"opcode": "bsr", "instructionTarget": "NamedService", "addressingForm": "direct"},
        {"opcode": "jsr", "instructionTarget": "OtherService", "addressingForm": "direct"},
        {"opcode": "jsr", "instructionTarget": "ThirdService", "addressingForm": "pc-relative"},
    ]
    assert _entity_presentation_fx_direct_calls(
        [
            "label: bsr.w NamedService",
            "; jsr OtherService",
            "move.w NamedService,d0",
            "jsr a0",
            "jsr NamedService(pc),d0",
        ]
    ) == []
    original_resolver = map_script_engine._screen_presentation_resolve_operand

    def malformed_resolution(value: str, equates: dict[str, int]) -> dict:
        if value == "MOSAIC_OUT":
            return {"rawValue": value, "resolvedValue": None, "resolution": "literal"}
        return original_resolver(value, equates)

    monkeypatch.setattr(
        map_script_engine, "_screen_presentation_resolve_operand", malformed_resolution
    )
    with pytest.raises(ValueError, match="operand resolution/value drift"):
        build_map_script_engine_contract(
            repo_path("local/roms/sf2-us.bin"),
            repo_path("local/upstream/SF2DISASM"),
        )


def test_map_entity_presentation_fx_schemas_reject_nested_mutations_and_exact_order(
    map_script_engine_output: dict,
) -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))
    sources = (map_script_engine_output, fixture)
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for source, schema_path in zip(sources, schema_paths, strict=True):
        validate_json(source, schema_path, owner="entity presentation FX baseline")
        target_path = (
            ("entityPresentationFxCommandFacts",)
            if source is map_script_engine_output
            else ("expected", "entityPresentationFxCommandFacts")
        )

        def target_for(value: dict, target_path: tuple[str, ...] = target_path) -> dict:
            target = value
            for key in target_path:
                target = target[key]
            return target

        missing = deepcopy(source)
        del target_for(missing)["macros"][0]["sourceOperandAnnotations"][0]["encoding"]
        with pytest.raises(ValueError, match="encoding"):
            validate_json(missing, schema_path, owner="entity presentation FX missing nested")

        renamed = deepcopy(source)
        branch = target_for(renamed)["handlers"][0]["sectionGuard"]["branchRecords"][0][
            "branchTarget"
        ]
        branch["targetSection"] = branch.pop("targetSectionAnchor")
        with pytest.raises(ValueError, match="targetSectionAnchor"):
            validate_json(renamed, schema_path, owner="entity presentation FX renamed nested")

        extra = deepcopy(source)
        target_for(extra)["handlers"][0]["directCalls"][0]["extra"] = True
        with pytest.raises(ValueError, match="extra"):
            validate_json(extra, schema_path, owner="entity presentation FX extra nested")

        reordered = deepcopy(source)
        order = target_for(reordered)["sourceSiteOrderKeys"]
        order[0], order[1] = order[1], order[0]
        with pytest.raises(ValueError, match="was expected"):
            validate_json(reordered, schema_path, owner="entity presentation FX exact source order")

        boundary = deepcopy(source)
        target_for(boundary)["macros"][2]["sourceCommandCount"] = 49
        with pytest.raises(ValueError, match="was expected"):
            validate_json(boundary, schema_path, owner="entity presentation FX exact boundary")


def test_map_entity_presentation_fx_schema_compacts_raw_corpora_and_closes_shapes() -> None:
    schema_paths = (
        repo_path("schemas/map-script-engine-static.schema.json"),
        repo_path("schemas/h2-map-script-engine-static-fixture.schema.json"),
    )
    for path in schema_paths:
        schema = load_json(path)
        contract = schema["properties"].get("entityPresentationFxCommandFacts")
        if contract is None:
            contract = schema["properties"]["expected"]["properties"][
                "entityPresentationFxCommandFacts"
            ]
        exact = contract["allOf"][1]["properties"]
        assert {"sourceSites", "programTotals"}.isdisjoint(exact)
        assert {"sourceSiteOrderKeys", "programTotalOrderKeys"} <= set(exact)
        definition_name = (
            "entityPresentationFxCommandFacts"
            if "entityPresentationFxCommandFacts" in schema["definitions"]
            else "entityPresentationFxFixtureCommandFacts"
        )
        facts = schema["definitions"][definition_name]
        assert facts["additionalProperties"] is False
        if definition_name == "entityPresentationFxCommandFacts":
            assert facts["properties"]["sourceSites"] == {
                "type": "array",
                "minItems": 61,
                "maxItems": 61,
                "items": {"$ref": "#/definitions/entityPresentationFxSourceSite"},
            }
            assert facts["properties"]["programTotals"] == {
                "type": "array",
                "minItems": 304,
                "maxItems": 304,
                "items": {"$ref": "#/definitions/entityPresentationFxProgramTotal"},
            }
            command = schema["definitions"]["entityPresentationFxCommand"]
            operand_value = command["properties"]["operandValues"]["items"]
            assert operand_value["additionalProperties"] is False
            assert set(operand_value["required"]) >= {"encoding", "resolution", "resolvedValue"}
            assert operand_value["properties"]["resolvedValue"] == {
                "type": ["integer", "null"]
            }
        else:
            assert {"sourceSites", "programTotals"}.isdisjoint(facts["required"])

        def assert_closed_objects(value):
            if isinstance(value, dict):
                if value.get("type") == "object":
                    assert value.get("additionalProperties") is False
                for child in value.values():
                    assert_closed_objects(child)
            elif isinstance(value, list):
                for child in value:
                    assert_closed_objects(child)

        assert_closed_objects(facts)
