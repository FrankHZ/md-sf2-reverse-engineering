from sf2tool.h2.battle_functions import (
    _branch_targets,
    _control_statements,
    _function_segments,
)


def test_player_control_segments_join_main_body_and_three_chunks() -> None:
    name = "ProcessBattleEntityControlPlayerInput"
    source = f"""
{name}:
    moveq #0,d0
    ; End of function {name}
; START OF FUNCTION CHUNK FOR {name}
    moveq #1,d0
; END OF FUNCTION CHUNK FOR {name}
; START OF FUNCTION CHUNK FOR {name}
    moveq #2,d0
; END OF FUNCTION CHUNK FOR {name}
; START OF FUNCTION CHUNK FOR {name}
    moveq #3,d0
; END OF FUNCTION CHUNK FOR {name}
"""

    actual = _function_segments(source, name)

    assert [row["kind"] for row in actual] == ["function", "chunk", "chunk", "chunk"]
    assert [row["body"].strip() for row in actual] == [
        "moveq #0,d0",
        "moveq #1,d0",
        "moveq #2,d0",
        "moveq #3,d0",
    ]


def test_control_statements_remove_labels_and_preserve_branch_targets() -> None:
    statements = _control_statements(
        "module\n@Start: moveq #0,d0\n  bne.s @Done\n  dbf d7,@Start\n@Done: rts\nmodend\n"
    )

    assert statements == ["moveq #0,d0", "bne.s @Done", "dbf d7,@Start", "rts"]
    assert _branch_targets(statements) == ["@Done", "@Start"]
