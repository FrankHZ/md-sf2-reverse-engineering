from sf2tool.h2.map_scripts import _script_programs


def test_script_programs_preserve_labels_and_symbol_edges() -> None:
    definitions = {"cs_Start": "scripts.asm", "cs_End": "scripts.asm"}
    programs = _script_programs(
        "scripts.asm",
        """
cs_Start: jumpIfFlagSet 42,cs_End
          nextText 0,ALLY_BOWIE
cs_End:   csc_end
""",
        definitions,
        {"cs_Start": 0x100, "cs_End": 0x108},
    )

    assert [row["id"] for row in programs] == ["cs_Start", "cs_End"]
    assert programs[0]["operations"][0]["targetSymbols"] == ["cs_End"]
    assert programs[0]["operations"][0]["targetAddresses"] == [0x108]
    assert programs[1]["operations"] == [
        {
            "index": 0,
            "opcode": "csc_end",
            "operandText": "",
            "targetSymbols": [],
            "targetAddresses": [],
        }
    ]
