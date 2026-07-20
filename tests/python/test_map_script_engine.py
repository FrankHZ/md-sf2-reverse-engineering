from sf2tool.h2.map_script_engine import (
    _cursor_flow,
    _emission_rows,
    _logical_source_lines,
    _program_corpus,
    _story_state_facts,
    _substitute_alias_layout,
)


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
