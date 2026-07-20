from sf2tool.h2.sound_data import (
    _fixed_opcode_families,
    _parse_music_macros,
    _song_command_row,
)


def test_music_macro_abi_and_song_invocations_are_parsed() -> None:
    macros = _parse_music_macros(
        "note macro arg0\n db arg0-24\n endm\nchannel_end macro\n db 0FFh\n db 0\n db 0\n endm\n"
    )

    assert macros == [
        {
            "name": "note",
            "parameters": ["arg0"],
            "emittedByteCount": 1,
            "byteExpressions": ["arg0-24"],
            "flowControl": False,
        },
        {
            "name": "channel_end",
            "parameters": [],
            "emittedByteCount": 3,
            "byteExpressions": ["0FFh", "0", "0"],
            "flowControl": True,
        },
    ]
    source = (
        "Music_1: db 0\n"
        + "dw Music_1_Channel_0\n" * 10
        + "Music_1_Channel_0: note C4\n channel_end\n"
    )
    row = _song_command_row(
        "music01.asm",
        source,
        {"note", "channel_end"},
    )
    assert row["entryLabels"] == ["Music_1"]
    assert row["channelLabels"] == ["Music_1_Channel_0"]
    assert row["channelPointerCount"] == 10
    assert row["uniqueChannelPointerCount"] == 1
    assert row["macroInvocationCount"] == 2
    assert row["macroInvocations"] == {"channel_end": 1, "note": 1}
    assert row["entryPointers"] == [
        {"entryLabel": "Music_1", "targets": ["Music_1_Channel_0"] * 10}
    ]
    assert row["channels"] == [
        {
            "label": "Music_1_Channel_0",
            "roles": ["dac", "psg-noise", "psg-tone", "ym1", "ym2"],
            "macroInvocationCount": 2,
            "macroInvocations": {"channel_end": 1, "note": 1},
        }
    ]
    assert _fixed_opcode_families(macros) == (
        {"FF": ["channel_end"]},
        ["note"],
    )


def test_music_entry_requires_exact_channel_pointer_shape() -> None:
    source = "Music_1: db 0\n" + "dw Music_1_Channel_0\n" * 9 + "Music_1_Channel_0: channel_end\n"

    try:
        _song_command_row("music01.asm", source, {"channel_end"})
    except ValueError as error:
        assert str(error) == (
            "music entry does not have ten channel pointers: music01.asm::Music_1"
        )
    else:
        raise AssertionError("nine-channel music entry unexpectedly parsed")
