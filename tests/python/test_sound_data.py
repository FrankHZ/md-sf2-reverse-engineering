from sf2tool.h2.sound_data import _parse_music_macros, _song_command_row


def test_music_macro_abi_and_song_invocations_are_parsed() -> None:
    macros = _parse_music_macros(
        "note macro arg0\n db arg0-24\n endm\n"
        "channel_end macro\n db 0FFh\n db 0\n db 0\n endm\n"
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
    row = _song_command_row(
        "music01.asm",
        "Music_1: db 0\n dw Music_1_Channel_0\n"
        "Music_1_Channel_0: note C4\n channel_end\n",
        {"note", "channel_end"},
    )
    assert row["entryLabels"] == ["Music_1"]
    assert row["channelLabels"] == ["Music_1_Channel_0"]
    assert row["macroInvocationCount"] == 2
    assert row["macroInvocations"] == {"channel_end": 1, "note": 1}
