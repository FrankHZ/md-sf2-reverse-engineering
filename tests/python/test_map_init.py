from sf2tool.h2.map_init import _operation_rows


def test_operation_rows_preserve_labels_and_resolve_local_branches() -> None:
    operations = _operation_rows(
        """
@Again:
    chkFlg 42
    beq.s @Again
    script cs_Test
    rts
"""
    )

    assert operations[0]["labels"] == ["@Again"]
    assert operations[0]["opcode"] == "chkFlg"
    assert operations[0]["operandText"] == "42"
    assert operations[1]["branchTargetSymbol"] == "@Again"
    assert operations[1]["localBranchTargetIndex"] == 0
    assert operations[2]["opcode"] == "script"
    assert operations[2]["operandText"] == "cs_Test"
