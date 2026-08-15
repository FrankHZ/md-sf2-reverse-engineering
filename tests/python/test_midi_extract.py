"""Synthetic unit tests for the MIDI extraction rail (no original payloads)."""

from __future__ import annotations

import mido
import pytest

from sf2tool.midi_extract import (
    BANK_ORIGIN,
    DRIVER_ROM_OFFSET,
    SFX_POINTER_TABLE_ADDRESS,
    ChannelResult,
    ExtractionOptions,
    MemoryWindow,
    _timer_b_us_per_qn,
    _vlq,
    extract_music,
    extract_sfx,
    interpret_channel,
    write_smf,
)


def _mem(payload: bytes, origin: int = BANK_ORIGIN) -> MemoryWindow:
    return MemoryWindow(payload, origin, "test")


def _stream(*values: int) -> bytes:
    return bytes(values)


def _opts(budget: int = 2) -> ExtractionOptions:
    return ExtractionOptions(loop_budget=budget)


def test_note_enum_maps_to_midi():
    from sf2tool.midi_extract import _NOTE_NAMES

    assert len(_NOTE_NAMES) == 108
    assert _NOTE_NAMES[0] == "C0"
    assert _NOTE_NAMES[48] == "C4"
    assert _NOTE_NAMES[107] == "B8"


def test_timer_b_tempo_formula():
    us = _timer_b_us_per_qn(0xC2, 30)
    assert us == 500000
    assert _timer_b_us_per_qn(0xD4, 30) == 500000
    assert _timer_b_us_per_qn(0xC2, 32) == 533333
    with pytest.raises(ValueError):
        _timer_b_us_per_qn(0, 30)


def test_wait_note_and_end_stream():
    stream = _stream(
        0xF0, 0x15,  # waitL 21 -> period 21, tick 21
        0xFE, 0x0D,  # inst 13
        0xFD, 0x0C,  # vol 12
        0xB2, 0x1E,  # noteL D6,30 (enum 74 -> MIDI 86), period 30
        0x32,  # bare note D6, period 30 reused
        0x70,  # wait, one period
        0xFF, 0x00, 0x00,  # channel end
    )
    result = interpret_channel(_mem(stream), BANK_ORIGIN, "ym1", 0, "ym1-1", _opts())
    assert result.stop_reason == "channel-end"
    assert result.rest_count == 2
    notes = result.notes
    assert len(notes) == 2
    assert notes[0][:3] == (21, 86, 104)
    assert notes[0][3] == 51
    assert notes[1][:3] == (51, 86, 104)
    assert notes[1][3] == 81
    assert result.volume_events == [(21, 12)]
    assert result.program_events == [(21, 80)]


def test_counted_loop():
    stream = _stream(
        0xF0, 0x10,  # period 16
        0xF8, 0xC1,  # counted loop, 2 iterations
        0xB3, 0x06,  # noteL Ds6,6
        0xF8, 0xE0,  # loop end
        0xFF, 0x00, 0x00,
    )
    result = interpret_channel(_mem(stream), BANK_ORIGIN, "ym1", 0, "ym1-1", _opts())
    assert len(result.notes) == 2
    assert [note[0] for note in result.notes] == [16, 22]


def test_repeat_sections_three_passes():
    stream = _stream(
        0xF0, 0x10,  # period 16
        0xF8, 0x20,  # repeatStart
        0xA6, 0x0A,  # noteL, 10
        0xF8, 0x40,  # repeatSection1Start
        0xA8, 0x0A,
        0xF8, 0xA0,  # repeatEnd (after ending 1)
        0xF8, 0x60,  # repeatSection2Start
        0xAA, 0x0A,
        0xF8, 0xA0,  # repeatEnd (after ending 2)
        0xF8, 0x80,  # repeatSection3Start (terminator)
        0xAC, 0x0A,
        0xF8, 0xA0,  # repeatEnd (after ending 3)
        0xFF, 0x00, 0x00,
    )
    result = interpret_channel(_mem(stream), BANK_ORIGIN, "ym1", 0, "ym1-1", _opts())
    assert result.stop_reason == "loop-budget"
    assert len(result.notes) == 6
    assert [note[1] for note in result.notes] == [
        12 + 62,
        12 + 64,
        12 + 62,
        12 + 66,
        12 + 62,
        12 + 68,
    ]

def test_main_loop_budget():
    stream = _stream(
        0xF8, 0x00,  # mainLoopStart
        0xF0, 0x10,
        0xB2, 0x1E,  # noteL D6,30
        0xF8, 0xA1,  # mainLoopEnd
        0xFF, 0x00, 0x00,
    )
    result = interpret_channel(_mem(stream), BANK_ORIGIN, "ym1", 0, "ym1-1", _opts(budget=2))
    assert result.stop_reason == "loop-budget"
    assert len(result.notes) == 3


def test_absolute_jump():
    jump_target = BANK_ORIGIN + 4
    stream = _stream(
        0x70,
        0xFF, jump_target & 0xFF, (jump_target >> 8) & 0xFF,
        0xB2, 0x1E,
        0xFF, 0x00, 0x00,
    )
    result = interpret_channel(_mem(stream), BANK_ORIGIN, "ym1", 0, "ym1-1", _opts())
    assert len(result.notes) == 1
    assert result.stop_reason == "channel-end"


def test_sustain_holds_until_next_note():
    stream = _stream(
        0xFC, 0x80,  # sustain
        0xF0, 0x10,
        0xB2, 0x0A,  # noteL, held
        0xB4, 0x0A,  # noteL, closes the held note
        0xFF, 0x00, 0x00,
    )
    result = interpret_channel(_mem(stream), BANK_ORIGIN, "ym1", 0, "ym1-1", _opts())
    assert len(result.notes) == 2
    assert result.notes[0][:4] == (16, 86, 100, 26)
    assert result.notes[1][:4] == (26, 88, 100, 36)


def test_psg_note_mapping():
    stream = _stream(
        0xF0, 0x10,
        0x15, 0x0A,  # psgNote C0 (enum 21), length 10
        0xFF, 0x00, 0x00,
    )
    result = interpret_channel(_mem(stream), BANK_ORIGIN, "psg-tone", 6, "psg-tone-1", _opts())
    assert result.notes[0][1] == 12 + 21


def test_ym_out_of_range_note_raises():
    stream = _stream(
        0xF0, 0x10,
        0x54, 0x0A,  # raw 84 -> enum 108 -> out of range
        0xFF, 0x00, 0x00,
    )
    with pytest.raises(ValueError, match="out of range"):
        interpret_channel(_mem(stream), BANK_ORIGIN, "ym1", 0, "ym1-1", _opts())


def test_smf_writer_roundtrip(tmp_path):
    channel = ChannelResult(
        slot_index=0,
        slot_name="ym1-1",
        family="ym1",
        midi_channel=1,
        notes=[(0, 60, 100, 10), (10, 62, 80, 20)],
        program_events=[(0, 80)],
        volume_events=[(0, 12)],
    )
    out = tmp_path / "t.mid"
    write_smf("Test", [(0, 500000)], [channel], 32, out)
    data = out.read_bytes()
    assert data[:14] == b"MThd\x00\x00\x00\x06\x00\x01\x00\x02\x00\x20"
    assert data[14:18] == b"MTrk"
    assert b"\xff\x51\x03" in data
    assert b"\x00\xc1\x50" in data
    assert b"\x00\xb1\x07\x68" in data
    assert b"\x00\x91\x3c\x64" in data
    assert b"\x0a\x91\x3e\x50" in data
    assert b"\x00\x81\x3c\x00" in data
    assert b"\x0a\x81\x3e\x00" in data
    assert data.endswith(b"\x00\xff\x2f\x00")
    assert data.count(b"\x00\xff\x2f\x00") == 2


def test_velocity_clamped_to_127():
    from sf2tool.midi_extract import _velocity

    assert _velocity(None) == 100
    assert _velocity(14) == 120
    assert _velocity(15) == 127


def test_smf_mido_message_level_roundtrip(tmp_path):
    channel = ChannelResult(
        slot_index=0,
        slot_name="ym1-1",
        family="ym1",
        midi_channel=1,
        notes=[(0, 60, 100, 10), (10, 62, 80, 20)],
        program_events=[(0, 80)],
        volume_events=[(0, 12), (10, 10)],
    )
    out = tmp_path / "t.mid"
    write_smf("Test", [(0, 498620), (100, 500000)], [channel], 32, out)
    mf = mido.MidiFile(str(out))
    assert mf.type == 1
    assert mf.ticks_per_beat == 32
    assert len(mf.tracks) == 2

    tempo_track = mf.tracks[0]
    assert tempo_track.name == "Test"
    tick = 0
    tempo_events = []
    for msg in tempo_track:
        tick += msg.time
        tempo_events.append((tick, msg))
    assert [(t, msg.type) for t, msg in tempo_events] == [
        (0, "track_name"),
        (0, "set_tempo"),
        (100, "set_tempo"),
        (100, "end_of_track"),
    ]
    assert tempo_events[1][1].tempo == 498620
    assert tempo_events[2][1].tempo == 500000

    channel_track = mf.tracks[1]
    assert channel_track.name == "Test ym1-1"
    tick = 0
    messages = []
    for msg in channel_track:
        tick += msg.time
        messages.append((tick, msg))
    assert [(t, msg.type) for t, msg in messages] == [
        (0, "track_name"),
        (0, "program_change"),
        (0, "control_change"),
        (0, "note_on"),
        (10, "control_change"),
        (10, "note_on"),
        (10, "note_off"),
        (20, "note_off"),
        (20, "end_of_track"),
    ]
    assert messages[1][1].program == 80
    assert messages[2][1].control == 7 and messages[2][1].value == 104
    assert messages[3][1].note == 60 and messages[3][1].velocity == 100
    assert messages[4][1].control == 7 and messages[4][1].value == 88
    assert messages[5][1].note == 62 and messages[5][1].velocity == 80
    assert messages[6][1].note == 60
    assert messages[7][1].note == 62


def test_mido_parses_synthetic_song(tmp_path):
    rom_bytes = _synthetic_rom()
    tracks = extract_music(rom_bytes, _opts(), expect_target_count=None)
    song = tracks[0]
    assert song.command_ids == list(range(1, 33))
    out = tmp_path / "s.mid"
    write_smf(
        f"Music_{song.command_ids[0]}",
        song.tempo_events,
        song.channels,
        32,
        out,
    )
    mf = mido.MidiFile(str(out))
    assert len(mf.tracks) == 2
    tempo_track = mf.tracks[0]
    tempo_messages = [msg for msg in tempo_track if msg.type == "set_tempo"]
    assert len(tempo_messages) == 1
    assert tempo_messages[0].tempo == 500000
    channel_track = mf.tracks[1]
    assert channel_track.name == "Music_1 ym1-1"
    tick = 0
    messages = []
    for msg in channel_track:
        tick += msg.time
        messages.append((tick, msg))
    assert [(t, msg.type) for t, msg in messages] == [
        (0, "track_name"),
        (16, "note_on"),
        (46, "note_off"),
        (46, "end_of_track"),
    ]
    assert messages[1][1].note == 86
    assert messages[1][1].velocity == 100

def test_analyze_tick_rate():
    from sf2tool.midi_extract import analyze_tick_rate

    counters_a = [[0] * 10 for _ in range(31)]
    counters_b = [[0] * 10 for _ in range(31)]
    for frame in range(31):
        counters_a[frame][0] = 30 - frame
        counters_b[frame][3] = max(20 - frame, 0)
    observed = {
        "frames": 31,
        "records": [
            {"command": 1, "counters": counters_a},
            {"command": 33, "counters": counters_b},
        ],
    }
    summary = analyze_tick_rate(observed)
    assert summary["records"][0]["channelIndex"] == 0
    assert summary["records"][0]["decrementCount"] == 30
    assert summary["records"][0]["averageTicksPerFrame"] == 1.0
    assert summary["records"][0]["activeTicksPerFrame"] == 1.0
    assert summary["records"][1]["channelIndex"] == 3
    assert summary["records"][1]["decrementCount"] == 20
    assert summary["records"][1]["decrementFrames"] == 20
    assert summary["records"][1]["averageTicksPerFrame"] == round(20 / 30, 4)
    assert summary["records"][1]["activeTicksPerFrame"] == 1.0


def test_vlq():
    assert _vlq(0) == b"\x00"
    assert _vlq(0x7F) == b"\x7f"
    assert _vlq(0x80) == b"\x81\x00"
    assert _vlq(0x2000) == b"\xc0\x00"


def _synthetic_rom() -> bytes:
    """Build a full 2 MiB ROM with one valid song target per bank."""
    from sf2tool.midi_extract import BANK_ROM_OFFSETS

    rom = bytearray(0x200000)
    stream = bytes(_stream(0xF0, 0x10, 0xB2, 0x1E, 0xFF, 0x00, 0x00))
    for bank_name, rom_offset in BANK_ROM_OFFSETS.items():
        del bank_name
        bank = memoryview(rom)[rom_offset : rom_offset + 0x8000]
        header_offset = 0x40
        stream_offset = 0x80
        bank[header_offset : header_offset + 4] = bytes([0, 0, 0, 0xC2])
        bank[stream_offset : stream_offset + len(stream)] = stream
        target = BANK_ORIGIN + header_offset
        for slot in range(32):
            bank[slot * 2 : slot * 2 + 2] = target.to_bytes(2, "little")
        stream_target = BANK_ORIGIN + stream_offset
        silent_target = BANK_ORIGIN + 0x100
        bank[0x100] = 0xFF
        for index in range(10):
            pointer = stream_target if index == 0 else silent_target
            bank[header_offset + 4 + index * 2 : header_offset + 6 + index * 2] = (
                pointer.to_bytes(2, "little")
            )
    return bytes(rom)


def test_extraction_determinism():
    rom_bytes = _synthetic_rom()
    tracks_a = extract_music(rom_bytes, _opts(), expect_target_count=None)
    tracks_b = extract_music(rom_bytes, _opts(), expect_target_count=None)
    assert len(tracks_a) == len(tracks_b) == 2
    assert len({track.header["timerB"] for track in tracks_a}) == 1
    assert [n for c in tracks_a[0].channels for n in c.notes] == [
        n for c in tracks_b[0].channels for n in c.notes
    ]
    assert tracks_a[0].header["timerB"] == 0xC2


def test_sfx_type2_dac_channel():
    driver = bytearray(0x2000)
    stream_offset = 0x1700
    silent_offset = 0x1800
    stream = bytes(_stream(0xF0, 0x10, 0x00, 0x06, 0xFF, 0x00, 0x00))
    driver[stream_offset : stream_offset + len(stream)] = stream
    driver[silent_offset] = 0xFF
    header_addr = 0x162D
    driver[header_addr] = 2
    driver[header_addr + 1 : header_addr + 3] = stream_offset.to_bytes(2, "little")
    driver[header_addr + 3 : header_addr + 7] = silent_offset.to_bytes(2, "little") * 2
    for slot in range(56):
        offset = SFX_POINTER_TABLE_ADDRESS + slot * 2
        driver[offset : offset + 2] = header_addr.to_bytes(2, "little")
    rom = bytearray(0x200000)
    rom[DRIVER_ROM_OFFSET : DRIVER_ROM_OFFSET + 0x2000] = driver
    tracks = extract_sfx(bytes(rom), _opts())
    assert len(tracks) == 56
    first = tracks[0]
    assert first.sfx_type == 2
    assert len(first.channels) == 1
    assert first.channels[0].family == "ym2"
    assert first.channels[0].notes[0][1] == 36
