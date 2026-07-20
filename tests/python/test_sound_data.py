from sf2tool.h2.sound_data import (
    _fixed_opcode_families,
    _music_bank_selection_contract,
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


def test_music_bank_selection_maps_all_command_slots() -> None:
    bank0_source = "\n".join(["dw Music_1"] * 32)
    bank1_source = "\n".join(["dw Music_33"] * 32)
    sources = {
        "data/sound/musicbank0/musicbank0.asm": bank0_source,
        "data/sound/musicbank0/music01.asm": "Music_1:\n",
        "data/sound/musicbank1/musicbank1.asm": bank1_source,
        "data/sound/musicbank1/music33.asm": "Music_33:\n",
    }
    bank_payload = bytearray(0x8000)
    for index in range(32):
        bank_payload[index * 2 : index * 2 + 2] = (0x8040).to_bytes(2, "little")
    enum_source = (
        "; enum Music\nMUSIC_NOTHING: equ 0\nMUSIC_MAIN_THEME: equ 1\n"
        "MUSIC_BATTLE_THEME_3: equ 33\n; enum Sfx\n"
    )

    contract = _music_bank_selection_contract(
        sources, {"bank0": bytes(bank_payload), "bank1": bytes(bank_payload)}, enum_source
    )

    assert contract["summary"] == {
        "commandSlotCount": 64,
        "namedMusicCommandCount": 2,
        "unnamedCommandSlotCount": 62,
        "uniquePointerTargetCount": 2,
        "zeroHeaderMarkerSlotCount": 64,
        "sfxRedirectSlotCount": 0,
        "crossBankFallbackEdgeCount": 0,
    }
    assert contract["slots"][0]["bankRegisterValue"] == 1
    assert contract["slots"][0]["enumName"] == "MUSIC_MAIN_THEME"
    assert contract["slots"][-1]["bankRegisterValue"] == 0
    assert contract["slots"][-1]["commandId"] == 64
