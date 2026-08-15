"""Deterministic extraction of the original Z80 music/SFX corpus into Standard MIDI files.

This rail re-interprets the canonical ROM byte streams with the exact semantics of the
original Z80 sound driver (``code/common/tech/sound/sounddriver.asm`` at the pinned
SF2DISASM commit) and emits private ``.mid`` files plus a metadata manifest under the
ignored local output directory. It consumes the accepted static facts of the
``sf2-sound-data-static-v1`` contract (two 32 KiB music banks, 64 command slots, 39
unique targets, 24-byte headers, ten channel roles, the 56-entry embedded SFX domain)
and adds the driver-level semantics needed to turn bytes into note events.

Semantics confirmed from the pinned driver source
-----------------------------------------------

- one music tick == one YM2612 Timer B overflow; ``UpdateSound`` decrements every
  channel counter once per overflow;
- channel stream bytes: ``0x00..0x6F``/``0x80..0xEF`` are note (or DAC sample) bytes,
  ``0x70``/``0xF0`` are the wait forms, ``0xF8``-``0xFE`` are two-byte commands, and
  ``0xFF`` + word is end (``00 00``), queued-operation mute (``xx 00`` on YM1-family
  channels), or an absolute Z80-address jump;
- ``0xF8`` loop subcommands by parameter bits 7-5: 0 main-loop start, 1 repeat start,
  2 repeat-section 1, 3 repeat-section 2, 4 repeat-section-3 terminator, 5 loop end
  (``A0`` repeat-end to loop B, ``A1`` main-loop-end to loop A), 6 counted-loop start
  (count = low 5 bits + 1), 7 counted-loop end;
- note pitch: YM index = byte + note-shift, enum = index + 24; PSG index = byte +
  note-shift - ``0x15``, enum = index + 21; noise uses only the low three bits; the
  DAC byte selects sample row = byte, played on channel 6 in DAC mode;
- note length: explicit length byte when bit 7 is set, otherwise the channel's current
  period; key-off occurs when the counter reaches the release value, unless the
  sustain bit (``FCh`` parameter bit 7) holds the key until the next note;
- ``0xFA`` on PSG channels (``ymTimer``) writes a new Timer B value mid-song.

Derived mappings (deliberate engineering choices, not original evidence)
------------------------------------------------------------------------

- MIDI pitch: note enum ``n`` maps to MIDI note ``12 + n`` (C0 -> 12, C4 -> 60);
- tempo: tick seconds = ``(1024 - TimerB) * 144 / 7_670_454`` (generic YM2612 Timer B
  formula on the Mega Drive master clock); microseconds per quarter note = tick
  seconds * 1e6 * ``quarter_ticks`` (default 32 ticks per quarter, ~120 BPM for the
  accepted Timer B range). Wall-clock tempo of the original is formally Unknown;
- GM programs: YM channels 80, PSG tone 81, PSG noise 82, DAC channel 10 with drum
  keys ``35 + sampleRow`` (arbitrary convenience mapping);
- velocity = ``8 + level * 8`` for levels 0..14, default 100 before the first volume
  command;
- loop budget (default 2): a channel stops after ``loop_budget`` loop-back jumps
  (main-loop end or repeat end), which yields three passes of a typical
  repeat-section song body; counted loops are exact;
- SFX wall-clock tempo is Unknown (SFX runs under the concurrent music's Timer B):
  SFX files carry the default 500000 us/quarter (120 BPM) and are flagged in the
  manifest; SFX type-2 channel 6 is parsed as DAC because the driver always takes the
  DAC path for type-2 SFX.

Copyright boundary: the generated MIDI and extracted PCM are private/generated music
payloads. They are written only under the ignored local output directory and are never
tracked. The tracked manifest contains metadata and hashes only.
"""

from __future__ import annotations

import hashlib
import json
from dataclasses import dataclass, field
from pathlib import Path

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.sound_data import (
    DRIVER_ROM_OFFSET,
    ROM_MANIFEST,
    SFX_COMMAND_START,
    SFX_ENTRY_COUNT,
    SFX_POINTER_TABLE_ADDRESS,
    SFX_TYPE_1_SLOTS,
    SFX_TYPE_2_SLOTS,
)
from sf2tool.jsonio import load_json

BANK_ROM_OFFSETS = {"bank0": 0x1F8000, "bank1": 0x1F0000}
BANK_ORIGIN = 0x8000
BANK_SIZE = 0x8000
DRIVER_SIZE = 0x2000
MCLK_HZ = 7_670_454
DEFAULT_QUARTER_TICKS = 32
DEFAULT_LOOP_BUDGET = 2
DEFAULT_TEMPO_US_PER_QN = 500_000
MAX_PARSE_STEPS = 2_000_000
MAX_EVENTS = 400_000

YM_FAMILIES = frozenset({"ym1", "ym2", "ym2-note"})
PSG_FAMILIES = frozenset({"psg-tone", "psg-noise"})
PROGRAM_FOR = {"ym": 80, "psg": 81, "noise": 82}

MUSIC_SLOT_NAMES = (
    "ym1-1",
    "ym1-2",
    "ym1-3",
    "ym2-4",
    "ym2-5",
    "ym2-6-dac",
    "psg-tone-1",
    "psg-tone-2",
    "psg-tone-3",
    "psg-noise",
)

_NOTE_NAMES = [
    f"{name}{octave}"
    for octave in range(9)
    for name in ("C", "Cs", "D", "Ds", "E", "F", "Fs", "G", "Gs", "A", "As", "B")
]
if len(_NOTE_NAMES) != 108:
    raise AssertionError("note name table drift")


@dataclass(frozen=True)
class ExtractionOptions:
    loop_budget: int = DEFAULT_LOOP_BUDGET
    quarter_ticks: int = DEFAULT_QUARTER_TICKS
    sfx_tempo_us_per_qn: int = DEFAULT_TEMPO_US_PER_QN
    max_parse_steps: int = MAX_PARSE_STEPS
    max_events: int = MAX_EVENTS


@dataclass
class ChannelResult:
    slot_index: int
    slot_name: str
    family: str
    midi_channel: int
    notes: list[tuple[int, int, int, int | None]] = field(default_factory=list)
    volume_events: list[tuple[int, int]] = field(default_factory=list)
    program_events: list[tuple[int, int]] = field(default_factory=list)
    tempo_events: list[tuple[int, int]] = field(default_factory=list)
    rest_count: int = 0
    parse_steps: int = 0
    stop_reason: str = "unresolved"
    clamped_out_of_range_notes: int = 0


class MemoryWindow:
    """Z80-addressable byte window (bank or driver binary) with bounds checking."""

    def __init__(self, payload: bytes, z80_origin: int, label: str) -> None:
        self._payload = payload
        self._origin = z80_origin
        self._label = label

    def byte(self, address: int) -> int:
        offset = address - self._origin
        if not 0 <= offset < len(self._payload):
            raise ValueError(
                f"byte read {address:04X} outside {self._label} window "
                f"({self._origin:04X}..{self._origin + len(self._payload) - 1:04X})"
            )
        return self._payload[offset]

    def word_le(self, address: int) -> int:
        return self.byte(address) | (self.byte(address + 1) << 8)


def _decode_note_shift(value: int) -> int:
    masked = value & 0x8F
    if masked & 0x80:
        masked |= 0xF0
    return masked - 0x100 if masked & 0x80 else masked


def _timer_b_us_per_qn(timer_b: int, quarter_ticks: int) -> int:
    if not 0 < timer_b < 0x100:
        raise ValueError(f"Timer B value out of range: {timer_b:02X}")
    tick_seconds = (1024 - timer_b) * 144 / MCLK_HZ
    return round(tick_seconds * 1_000_000 * quarter_ticks)


def _velocity(level: int | None) -> int:
    return 8 + level * 8 if level is not None else 100


def _slot_midi_channel(slot_index: int, family: str) -> int:
    if family == "dac":
        return 10
    if family == "psg-noise":
        return 11
    if slot_index <= 5:
        return 1 + slot_index
    return slot_index + 1


def _scan_forward(mem: MemoryWindow, pos: int, marker: int) -> int | None:
    """Reproduce the driver's repeat-section skip scan (slide by one byte)."""
    while True:
        byte = mem.byte(pos)
        if byte == 0xFF:
            return None
        if byte == 0xF8 and mem.byte(pos + 1) == marker:
            return pos
        pos += 1


def interpret_channel(
    mem: MemoryWindow,
    start_address: int,
    family: str,
    slot_index: int,
    slot_name: str,
    options: ExtractionOptions,
) -> ChannelResult:
    """Interpret one channel byte stream with the driver semantics."""
    result = ChannelResult(
        slot_index=slot_index,
        slot_name=slot_name,
        family=family,
        midi_channel=_slot_midi_channel(slot_index, family),
    )
    pos = start_address
    tick = 0
    period = 0
    release = 0
    sustain = False
    note_shift = 0
    level: int | None = None
    loop_a: int | None = None
    loop_b: int | None = None
    counted_pos: int | None = None
    counted = 0
    r1 = False
    r2 = False
    loopbacks = 0
    steps = 0
    ended = False

    def note_enum(raw: int) -> int:
        if family in YM_FAMILIES:
            return raw + 24 + note_shift
        return raw + note_shift

    while not ended:
        steps += 1
        result.parse_steps = steps
        if steps > options.max_parse_steps:
            raise ValueError(
                f"channel {slot_name} parse-step limit exceeded at {pos:04X} "
                f"(unbounded loop without flow commands?)"
            )
        if len(result.notes) + len(result.volume_events) > options.max_events:
            raise ValueError(f"channel {slot_name} event limit exceeded")
        byte = mem.byte(pos)
        if byte & 0xF8 == 0xF8:
            if byte == 0xFF:
                word = mem.word_le(pos + 1)
                if word == 0:
                    ended = True
                    result.stop_reason = "channel-end"
                    break
                if word & 0xFF00 == 0:
                    ended = True
                    result.stop_reason = "channel-end-queued-operation"
                    break
                pos = word
                continue
            param = mem.byte(pos + 1)
            pos += 2
            if byte == 0xF8:
                sub = (param >> 5) & 7
                if sub == 0:
                    loop_a = pos
                elif sub == 1:
                    loop_b = pos
                    r1 = False
                    r2 = False
                elif sub == 2:
                    if not r1:
                        r1 = True
                    else:
                        target = _scan_forward(mem, pos, 0x60)
                        if target is None:
                            ended = True
                            result.stop_reason = "channel-end"
                            break
                        pos = target
                elif sub == 3:
                    if not r2:
                        r2 = True
                    else:
                        target = _scan_forward(mem, pos, 0x80)
                        if target is None:
                            ended = True
                            result.stop_reason = "channel-end"
                            break
                        pos = target
                elif sub == 4:
                    pass
                elif sub == 5:
                    target = loop_a if param & 1 else loop_b
                    if target is None:
                        raise ValueError(
                            f"unmatched loop end (param {param:02X}) in {slot_name}"
                        )
                    if loopbacks >= options.loop_budget:
                        ended = True
                        result.stop_reason = "loop-budget"
                        break
                    loopbacks += 1
                    pos = target
                elif sub == 6:
                    counted_pos = pos
                    counted = (param & 0x1F) + 1
                elif sub == 7:
                    counted -= 1
                    if counted > 0 and counted_pos is not None:
                        pos = counted_pos
            elif byte == 0xF9:
                note_shift = _decode_note_shift(param)
            elif byte == 0xFA:
                if family in PSG_FAMILIES:
                    result.tempo_events.append(
                        (tick, _timer_b_us_per_qn(param, options.quarter_ticks))
                    )
            elif byte == 0xFB:
                pass
            elif byte == 0xFC:
                if family in YM_FAMILIES:
                    if param == 0xFF or param >= 0x81:
                        pass
                    else:
                        release = param & 0x7F
                        sustain = (param & 0x80) != 0
                else:
                    release = param & 0x7F
                    sustain = (param & 0x80) != 0
            elif byte == 0xFD:
                if family in YM_FAMILIES:
                    level = param & 0x0F
                    result.volume_events.append((tick, level))
                else:
                    level = param & 0x0F
                    result.volume_events.append((tick, level))
                    result.program_events.append((tick, PROGRAM_FOR["psg"]))
            elif byte == 0xFE:
                if family in YM_FAMILIES:
                    result.program_events.append((tick, PROGRAM_FOR["ym"]))
            continue
        if byte & 0x7F == 0x70:
            if byte == 0xF0:
                period = mem.byte(pos + 1)
                pos += 2
            else:
                pos += 1
            tick += period
            result.rest_count += 1
            continue
        raw = byte & 0x7F
        if family == "dac":
            pitch = 35 + raw
        elif family == "psg-noise":
            pitch = 60 + (raw & 7)
        else:
            pitch = 12 + note_enum(raw)
            if not 12 <= pitch <= 119:
                raise ValueError(
                    f"note enum {note_enum(raw)} out of range at {pos:04X} "
                    f"in {slot_name}"
                )
        if byte & 0x80:
            period = mem.byte(pos + 1)
            pos += 2
        else:
            pos += 1
        velocity = _velocity(level)
        for index, (_, _, _, off_tick) in enumerate(result.notes):
            if off_tick is None:
                result.notes[index] = (result.notes[index][0], result.notes[index][1],
                                       result.notes[index][2], tick)
        off_tick = (
            tick + period - release if not sustain and release < period else None
        )
        result.notes.append((tick, pitch, velocity, off_tick))
        tick += period

    for index, (on_tick, pitch, velocity, _) in enumerate(result.notes):
        if result.notes[index][3] is None:
            result.notes[index] = (on_tick, pitch, velocity, tick)
    return result


def _music_families(dac_enabled: bool) -> tuple[str, ...]:
    roles = ("ym1", "ym1", "ym1", "ym2", "ym2")
    return roles + (("dac",) if dac_enabled else ("ym2-note",)) + (
        "psg-tone",
        "psg-tone",
        "psg-tone",
        "psg-noise",
    )


@dataclass
class MusicTrack:
    command_ids: list[int]
    bank: str
    target_z80: int
    header: dict[str, int]
    channels: list[ChannelResult] = field(default_factory=list)
    tempo_events: list[tuple[int, int]] = field(default_factory=list)
    silent: bool = False


def _header_dict(header_bytes: bytes) -> dict[str, int]:
    return {
        "typeMarker": header_bytes[0],
        "dacDisabled": header_bytes[1],
        "reservedTimerA": header_bytes[2],
        "timerB": header_bytes[3],
    }


def _extract_music_target(
    mem: MemoryWindow, payload: bytes, target_z80: int, header: dict[str, int], options
) -> list[ChannelResult]:
    header_offset = target_z80 - BANK_ORIGIN
    pointers = [
        int.from_bytes(payload[header_offset + 4 + index * 2 : header_offset + 6 + index * 2],
                       "little")
        for index in range(10)
    ]
    families = _music_families(header["dacDisabled"] == 0)
    channels = []
    for slot_index, (pointer, family) in enumerate(zip(pointers, families, strict=True)):
        if mem.byte(pointer) == 0xFF:
            continue
        channels.append(
            interpret_channel(
                mem,
                pointer,
                family,
                slot_index,
                MUSIC_SLOT_NAMES[slot_index],
                options,
            )
        )
    return channels


def extract_music(
    rom: bytes, options: ExtractionOptions, *, expect_target_count: int | None = 39
) -> list[MusicTrack]:
    banks = {
        name: rom[offset : offset + BANK_SIZE]
        for name, offset in BANK_ROM_OFFSETS.items()
    }
    tracks: list[MusicTrack] = []
    seen_targets: dict[tuple[str, int], MusicTrack] = {}
    for command_id in range(1, 65):
        bank = "bank0" if command_id <= 32 else "bank1"
        payload = banks[bank]
        slot_index = (command_id - 1) % 32
        target_z80 = int.from_bytes(payload[slot_index * 2 : slot_index * 2 + 2], "little")
        if not BANK_ORIGIN + 64 <= target_z80 < BANK_ORIGIN + BANK_SIZE:
            raise ValueError(f"music command {command_id} target outside bank: {target_z80:04X}")
        header_offset = target_z80 - BANK_ORIGIN
        if header_offset + 24 > len(payload):
            raise ValueError(f"music command {command_id} header truncated")
        header = _header_dict(payload[header_offset : header_offset + 4])
        if header["typeMarker"] != 0 or header["reservedTimerA"] != 0:
            raise ValueError(f"music command {command_id} header fields drift")
        existing = seen_targets.get((bank, target_z80))
        if existing is not None:
            existing.command_ids.append(command_id)
            continue
        mem = MemoryWindow(payload, BANK_ORIGIN, bank)
        channels = _extract_music_target(mem, payload, target_z80, header, options)
        silent = not channels
        track = MusicTrack(
            command_ids=[command_id],
            bank=bank,
            target_z80=target_z80,
            header=header,
            channels=channels,
            silent=silent,
        )
        track.tempo_events = [
            (0, _timer_b_us_per_qn(header["timerB"], options.quarter_ticks))
        ]
        for channel in channels:
            track.tempo_events.extend(channel.tempo_events)
        track.tempo_events.sort()
        seen_targets[(bank, target_z80)] = track
        tracks.append(track)
    if expect_target_count is not None and len(tracks) != expect_target_count:
        raise ValueError(f"unique music target count drift: {len(tracks)}")
    return tracks


def _sfx_families(sfx_type: int) -> tuple[str, ...]:
    if sfx_type == 1:
        return (
            "ym1",
            "ym1",
            "ym1",
            "ym2",
            "ym2",
            "dac",
            "psg-tone",
            "psg-tone",
            "psg-tone",
            "psg-noise",
        )
    return ("ym2", "ym2", "dac")


@dataclass
class SfxTrack:
    command_id: int
    sfx_index: int
    sfx_type: int
    header_z80: int
    channels: list[ChannelResult] = field(default_factory=list)
    silent: bool = False


def extract_sfx(rom: bytes, options: ExtractionOptions) -> list[SfxTrack]:
    if len(rom) < DRIVER_ROM_OFFSET + DRIVER_SIZE:
        raise ValueError("ROM too small for the sound driver slice")
    driver = rom[DRIVER_ROM_OFFSET : DRIVER_ROM_OFFSET + DRIVER_SIZE]
    mem = MemoryWindow(driver, 0, "sound driver")
    tracks: list[SfxTrack] = []
    for slot_index in range(SFX_ENTRY_COUNT):
        table_offset = SFX_POINTER_TABLE_ADDRESS + slot_index * 2
        header_z80 = int.from_bytes(driver[table_offset : table_offset + 2], "little")
        sfx_type = driver[header_z80]
        if sfx_type not in (1, 2):
            raise ValueError(f"SFX {slot_index:02X} header type drift: {sfx_type}")
        names = SFX_TYPE_1_SLOTS if sfx_type == 1 else SFX_TYPE_2_SLOTS
        pointers = [
            int.from_bytes(
                driver[header_z80 + 1 + index * 2 : header_z80 + 3 + index * 2], "little"
            )
            for index in range(len(names))
        ]
        families = _sfx_families(sfx_type)
        channels = []
        for channel_index, (pointer, family) in enumerate(zip(pointers, families, strict=True)):
            if mem.byte(pointer) == 0xFF:
                continue
            slot_name = names[channel_index]
            result = interpret_channel(
                mem, pointer, family, channel_index, slot_name, options
            )
            channels.append(result)
        tracks.append(
            SfxTrack(
                command_id=SFX_COMMAND_START + slot_index,
                sfx_index=slot_index + 1,
                sfx_type=sfx_type,
                header_z80=header_z80,
                channels=channels,
                silent=not channels,
            )
        )
    return tracks


def _vlq(value: int) -> bytes:
    chunks = [value & 0x7F]
    value >>= 7
    while value:
        chunks.append(0x80 | (value & 0x7F))
        value >>= 7
    return bytes(reversed(chunks))


def _meta_event(event_type: int, data: bytes) -> bytes:
    return b"\xFF" + bytes((event_type,)) + _vlq(len(data)) + data


def _end_of_track() -> bytes:
    return _meta_event(0x2F, b"")


def _tempo_meta(us_per_qn: int) -> bytes:
    return _meta_event(0x51, us_per_qn.to_bytes(3, "big"))


def _text_meta(text: str) -> bytes:
    return _meta_event(0x03, text.encode("ascii", "replace"))


def _channel_track_bytes(
    name: str,
    midi_channel: int,
    program_events: list[tuple[int, int]],
    volume_events: list[tuple[int, int]],
    notes: list[tuple[int, int, int, int | None]],
) -> bytes:
    events: list[tuple[int, int, bytes]] = []
    for tick, program in program_events:
        events.append((tick, 0, bytes((0xC0 | midi_channel, program))))
    for tick, level in volume_events:
        events.append((tick, 1, bytes((0xB0 | midi_channel, 7, _velocity(level)))))
    for on_tick, pitch, velocity, off_tick in notes:
        events.append((on_tick, 2, bytes((0x90 | midi_channel, pitch, velocity))))
        off = on_tick if off_tick is None else off_tick
        events.append((off, 3, bytes((0x80 | midi_channel, pitch, 0))))
    events.sort(key=lambda row: (row[0], row[1]))
    running_tick = 0
    body = b""
    for tick, _order, payload in events:
        body += _vlq(tick - running_tick) + payload
        running_tick = tick
    return _vlq(0) + _text_meta(name) + body + _end_of_track()


def _tempo_track_bytes(name: str, tempo_events: list[tuple[int, int]]) -> bytes:
    merged: list[tuple[int, int]] = []
    for tick, us_per_qn in tempo_events:
        if merged and merged[-1][1] == us_per_qn:
            continue
        merged.append((tick, us_per_qn))
    running_tick = 0
    body = b""
    for tick, us_per_qn in merged:
        body += _vlq(tick - running_tick) + _tempo_meta(us_per_qn)
        running_tick = tick
    return _vlq(0) + _text_meta(name) + body + _end_of_track()


def write_smf(
    name: str,
    tempo_events: list[tuple[int, int]],
    channels: list[ChannelResult],
    division: int,
    out_path: Path,
) -> None:
    tracks = [_tempo_track_bytes(name, tempo_events)]
    for channel in channels:
        track = _channel_track_bytes(
            f"{name} {channel.slot_name}",
            channel.midi_channel,
            channel.program_events,
            channel.volume_events,
            channel.notes,
        )
        tracks.append(track)
    if len(tracks) == 1:
        return
    chunks = b"".join(
        b"MTrk" + len(track).to_bytes(4, "big") + track for track in tracks
    )
    payload = b"MThd" + (6).to_bytes(4, "big") + (1).to_bytes(2, "big") + len(
        tracks
    ).to_bytes(2, "big") + division.to_bytes(2, "big") + chunks
    out_path.write_bytes(payload)


def _sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest().upper()


def run_midi_extraction(
    rom_path: Path,
    upstream_path: Path,
    *,
    out_dir: Path,
    loop_budget: int,
    quarter_ticks: int,
    skip_sfx: bool,
) -> dict[str, object]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, _toolchain = _resolve_upstream(upstream_path)
    del disasm
    expected_rom = load_json(ROM_MANIFEST)
    rom = rom_path.read_bytes()
    if len(rom) != expected_rom["sizeBytes"] or _sha256(rom) != expected_rom["hashes"]["sha256"]:
        raise ValueError("MIDI extraction ROM identity drift")

    options = ExtractionOptions(
        loop_budget=loop_budget, quarter_ticks=quarter_ticks
    )
    out_dir.mkdir(parents=True, exist_ok=True)
    music_dir = out_dir / "music"
    sfx_dir = out_dir / "sfx"
    music_dir.mkdir(parents=True, exist_ok=True)
    sfx_dir.mkdir(parents=True, exist_ok=True)

    files: list[dict[str, object]] = []
    notes: list[str] = []
    music_tracks = extract_music(rom, options)
    for track in music_tracks:
        name = f"Music_{track.command_ids[0]}"
        if track.silent:
            notes.append(f"{name}: silent target, no MIDI written")
            continue
        rel = music_dir / f"{name}.mid"
        write_smf(
            name,
            track.tempo_events,
            track.channels,
            options.quarter_ticks,
            rel,
        )
        files.append(
            {
                "path": f"music/{name}.mid",
                "sha256": _sha256(rel.read_bytes()),
                "sizeBytes": rel.stat().st_size,
                "kind": "music",
                "id": name,
                "commandIds": track.command_ids,
                "bank": track.bank,
                "targetZ80Address": f"{track.target_z80:04X}",
                "timerB": f"{track.header['timerB']:02X}",
                "dacEnabled": track.header["dacDisabled"] == 0,
                "channelCount": len(track.channels),
                "noteCount": sum(len(channel.notes) for channel in track.channels),
                "restCount": sum(channel.rest_count for channel in track.channels),
                "tempoEventCount": len(track.tempo_events),
            }
        )

    sfx_tracks: list[SfxTrack] = []
    if not skip_sfx:
        sfx_tracks = extract_sfx(rom, options)
        for track in sfx_tracks:
            name = f"sfx_{track.sfx_index:02X}"
            if track.silent:
                notes.append(f"{name}: no active channels, no MIDI written")
                continue
            rel = sfx_dir / f"{name}.mid"
            tempo_events = [(0, options.sfx_tempo_us_per_qn)]
            write_smf(name, tempo_events, track.channels, options.quarter_ticks, rel)
            files.append(
                {
                    "path": f"sfx/{name}.mid",
                    "sha256": _sha256(rel.read_bytes()),
                    "sizeBytes": rel.stat().st_size,
                    "kind": "sfx",
                    "id": name,
                    "commandId": track.command_id,
                    "sfxType": track.sfx_type,
                    "channelCount": len(track.channels),
                    "noteCount": sum(len(channel.notes) for channel in track.channels),
                    "restCount": sum(channel.rest_count for channel in track.channels),
                    "tempoNotOriginal": True,
                }
            )
        notes.append(
            "SFX wall-clock tempo is Unknown (SFX runs under concurrent music Timer B); "
            "SFX files use the default 500000 us/quarter and SFX type-2 channel 6 is parsed as DAC"
        )

    manifest = {
        "schemaVersion": 1,
        "tool": "sf2 midi extract",
        "romId": expected_rom["id"],
        "romSha256": expected_rom["hashes"]["sha256"],
        "upstream": {"repository": "ShiningForceCentral/SF2DISASM", "commit": commit},
        "options": {
            "loopBudget": loop_budget,
            "quarterTicks": quarter_ticks,
            "sfxTempoUsPerQuarter": options.sfx_tempo_us_per_qn,
        },
        "mappings": {
            "pitch": "note enum n maps to MIDI note 12+n; YM enum = raw+24+shift, "
            "PSG enum = raw+shift; noise = 60+(raw&7)",
            "tempo": "tick = (1024-TimerB)*144/7670454 s; us/quarter = tick*1e6*quarterTicks",
            "programs": {"ym": 80, "psgTone": 81, "psgNoise": 82, "dac": "channel 10, keys 35+row"},
            "velocity": "8+level*8 for levels 0..14, default 100",
            "noteOff": "key off at period-release ticks; sustain (FC bit 7) holds until next note",
        },
        "notes": notes,
        "summary": {
            "musicTargetCount": len(music_tracks),
            "musicSilentTargetCount": sum(track.silent for track in music_tracks),
            "musicFileCount": sum(row["kind"] == "music" for row in files),
            "sfxCommandCount": len(sfx_tracks),
            "sfxSilentCount": sum(track.silent for track in sfx_tracks),
            "sfxFileCount": sum(row["kind"] == "sfx" for row in files),
            "totalFileCount": len(files),
            "totalNoteCount": sum(int(row["noteCount"]) for row in files),
        },
        "files": files,
    }
    manifest_path = out_dir / "manifest.json"
    manifest_json = json.dumps(manifest, ensure_ascii=False, indent=2) + "\n"
    manifest_path.write_text(manifest_json, encoding="utf-8")
    return {
        "Music": len(music_tracks),
        "MusicFiles": manifest["summary"]["musicFileCount"],
        "Sfx": len(sfx_tracks),
        "SfxFiles": manifest["summary"]["sfxFileCount"],
        "Files": len(files),
        "Notes": len(notes),
        "Output": str(out_dir),
        "Manifest": _sha256(manifest_path.read_bytes()),
        "Status": "PASS",
    }
