from sf2tool.h2.sound_data import (
    _fixed_opcode_families,
    _music_bank_selection_contract,
    _music_frequency_contract,
    _music_instrument_contract,
    _music_sample_contract,
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
        "Music_1: db 0\n db 1\n db 0\n db 200\n"
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
        {
            "entryLabel": "Music_1",
            "header": {
                "typeMarker": 0,
                "dacDisabled": True,
                "reservedTimerA": 0,
                "timerB": 200,
            },
            "targets": ["Music_1_Channel_0"] * 10,
        }
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


def test_music_frequency_contract_parses_overlapping_ym_table() -> None:
    note_names = [
        f"{name}{octave}"
        for octave in range(9)
        for name in ("C", "Cs", "D", "Ds", "E", "F", "Fs", "G", "Gs", "A", "As", "B")
    ]
    enum_source = "\n".join(f"{name}: equ {index}" for index, name in enumerate(note_names))
    driver_source = (
        "db 01h\n"
        "t_YM_FREQUENCIES: db 02h\n"
        + "dw 0100h\n" * 83
        + "t_PSG_FREQUENCIES: dw 0200h\n"
        + "dw 0200h\n" * 63
        + "t_YM_LEVELS: db 0\n"
    )
    sources = {
        "data/sound/musicenums.asm": enum_source,
        "data/sound/musicbank0/music01.asm": (
            "Music_1_Channel_0:\nnote C2\npsgNote A1\nchannel_end\n"
        ),
    }

    contract = _music_frequency_contract(sources, driver_source)

    assert contract["ym"]["entries"][0] == {
        "index": 0,
        "note": "C2",
        "registerValue": 0x0201,
    }
    assert contract["ym"]["summary"]["rawIndexOutsideTableInvocationCount"] == 0
    assert contract["psg"]["summary"]["rawIndexOutsideTableInvocationCount"] == 0
    assert contract["psg"]["shiftAudit"]["summary"]["outOfRangeInvocationCount"] == 0
    assert contract["macroInvocationCounts"] == {"note": 1, "psgNote": 1}


def test_music_sample_contract_maps_table_and_song_uses() -> None:
    driver_source = (
        "t_SAMPLE_LOAD_DATA: db 1,0,0,0,1,0,0,80h\n"
        + "db 1,0,0,0,1,0,0,80h\n" * 16
        + "pt_SFX: dw 0\n"
    )
    sources = {"data/sound/musicbank0/music01.asm": "sample 0\nsampleL 1,4\n"}

    contract = _music_sample_contract(sources, driver_source, bytes(0x200000))

    assert contract["summary"]["tableEntryCount"] == 17
    assert contract["summary"]["musicInvocationCount"] == 2
    assert contract["musicInvocationCounts"] == [
        {"sampleIndex": 0, "invocationCount": 1},
        {"sampleIndex": 1, "invocationCount": 1},
    ]
    assert contract["entries"][0]["romOffset"] == 0x1E0000


def test_music_instrument_contract_splits_psg_nibbles() -> None:
    driver_source = (
        "t_YM_LEVELS: db 0\n"
        + "db 0\n" * 15
        + "t_SLOTS_PER_ALGO: db 8\n"
        + "db 8\n" * 7
        + "pt_PITCH_EFFECTS: dw 0\n"
        + "pt_PSG_INSTRUMENTS: dw byte_12D2\n"
        + "dw byte_12D2\n" * 15
        + "byte_12D2: db 80h\n"
    )
    sources = {"data/sound/musicbank0/music01.asm": "inst 0\nvol 0\npsgInst 0A5h\n"}

    contract = _music_instrument_contract(sources, driver_source, bytes(0x200000))

    assert contract["summary"]["ymInvocationCount"] == 1
    assert contract["psg"]["instrumentInvocationCounts"] == [
        {"instrumentIndex": 10, "invocationCount": 1}
    ]
    assert contract["psg"]["levelInvocationCounts"] == [{"level": 5, "invocationCount": 1}]
