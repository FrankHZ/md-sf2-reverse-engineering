from sf2tool.h3.sound_timing import RAM, _header_pointers


def test_music_header_pointer_decode_is_little_endian() -> None:
    payload = bytearray(0x100)
    payload[0x20:0x38] = bytes.fromhex(
        "000100C0"
        "0180028003800480058006800780088009800A80"
    )

    assert _header_pointers(bytes(payload), 0x8020) == [
        0x8001,
        0x8002,
        0x8003,
        0x8004,
        0x8005,
        0x8006,
        0x8007,
        0x8008,
        0x8009,
        0x800A,
    ]


def test_sound_channel_ram_shape_is_ten_fixed_records() -> None:
    assert RAM["channelBaseAddress"] == 0x1380
    assert RAM["channelCount"] == 10
    assert RAM["channelRecordSize"] == 0x20
    assert RAM["channelBaseAddress"] + RAM["channelCount"] * RAM["channelRecordSize"] == 0x14C0
