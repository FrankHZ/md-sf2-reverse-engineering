from sf2tool.h2.map_script_engine import (
    _cursor_flow,
    _emission_rows,
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
