# Z80 Music-Bank Source and ROM-Parity Inventory

- Status: **Confirmed** for the complete 41-file directory, two bank entry points, include graph,
  bank sizes/hashes/order, pointer/include counts, 37 song ranges/entries, the 29-macro ABI and
  complete source invocation corpus, all 39 music headers, ten-slot channel-role shape and command
  compatibility, and canonical-ROM parity
- Status: **Inferred** for music command playback semantics
- Status: **Unknown** for two grouped runtime questions
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Separate Address Space

`data/sound` contains 41 ASM files and 40,644 lines: two music-bank entry files, two shared macro/enum
files, and 37 song files. `musicbank0.asm` and `musicbank1.asm` are assembled independently for the
Z80 with origin `0x8000`; their 41 include edges reach all 39 shared/song targets and therefore close
the directory at 41/41 H2 inventory.

These sources do not define symbols in the 68000 H1 listing. The ROM layout includes the completed
banks as unlabeled binary payloads, bank 1 first at `0x1F0000` and bank 0 second at `0x1F8000`.
The index therefore keeps H1 as its default and adds one restricted `z80-music-bank` domain. Its 37
song records bind an actual source label to the little-endian bank pointer, Z80 address, and physical
ROM offset; the two bank entry files and two macro/enum sources remain uncredited because they have
no independent entry symbol.

## Static Bank Parity

Both generated banks are exactly 32 KiB and byte-match the canonical ROM slices:

| Bank | ROM offset | Pointer slots | Unique targets | Song includes | SHA-256 |
| --- | ---: | ---: | ---: | ---: | --- |
| 1 | `0x1F0000` | 32 | 10 | 10 | `978575483EC8354379C5099911ADA611BC7BEC96E58E501BF29B71064C72756C` |
| 0 | `0x1F8000` | 32 | 29 | 27 | `EB1A77668279D147FC887CB5E1FBCCA3E037CDC648C63FC78AEBB158E377822D` |

The verifier reads the local generated banks and canonical ROM only for byte parity. It commits no
music bytes, note streams, instruments, PCM, or extracted audio. The generated structured inventory
stays under ignored `local/derived/sound-data-static.json`.

The 37 source headers form contiguous song regions immediately after each 64-byte pointer table:
27 files occupy 32,247 bytes in bank 0 and ten files occupy 32,121 bytes in bank 1, for 64,368
ROM-checked bytes. Each catalog row also records source/payload hashes and checks that its logical
`Music_n` pointer lands inside the owning file range. This handles the combined Music 3/4 and 13/14
sources without pretending the filename necessarily begins at the first label.

## Music Command and Bank Selection

The Z80 `Main` path and both 32-word bank tables close the complete command-to-pointer map. Command
`0` is ignored by `Main_Loop`. Music commands `0x01`–`0x20` set the bank register value to `1`, map
ROM `0x1F8000`, and index bank 0 with `command - 1`; `0x21`–`0x40` set it to `0`, map ROM
`0x1F0000`, and index bank 1 with `command - 33`. Values from `0x41` enter the SFX path after the
earlier special-command checks.

All 64 canonical pointer slots resolve to a source label and begin with the zero music-header marker,
so none takes the driver's nonzero-header redirect to `Load_SFX`. The 38 named nonzero music enums
resolve to their matching slots. Commands 29–32 alias the silent `Music_32` target; commands 42–64
alias `Music_64`. There are 39 unique pointer targets, no target crosses its selected 32 KiB bank,
and no cross-bank fallback edge exists in this path. Bank selection and fallback are therefore closed
statically rather than deferred to an emulator run.

## Music Header Fields

All 39 unique music targets use the same 24-byte header: a zero type marker, a DAC-disable byte, one
reserved Timer A byte, one YM Timer B byte, and ten little-endian channel pointers. The driver reads
the second byte into `MUSIC_DOESNT_USE_SAMPLES`, skips the reserved byte, writes byte 3 to YM register
`0x26`, then initializes ten `0x20`-byte channel-state records from the pointers.

Nineteen headers leave DAC enabled and 20 disable it. The reserved byte is zero in every entry.
Timer B uses 19 distinct source values from `0xBD` through `0xD4`; exact value counts and all entry
rows are part of the canonical fixture. This confirms the stored control fields and driver reads,
but not the wall-clock tempo produced by YM2612 Timer B, which remains in the concentrated timing
matrix.

## Static Note and Frequency Domains

`musicenums.asm` defines 108 semitone names from C0 through B8. The driver owns an 84-entry YM table
for C2–B8 and a 64-entry PSG table for C0–Ds5; their canonical register-word streams are hashed and
the complete indexed rows are generated. The song corpus contains 16,636 YM note calls spanning raw
table indices 1–71, all inside the YM table.

The PSG parser subtracts `0x15` after applying channel-state byte `$1C`, so the 5,205 PSG note calls
span base indices 0–48—not 21–69. A conservative channel CFG covers main, counted, and repeat-loop
back-edges. All 218 `shifting` calls use only `0x00`, `0x10`, or `0x20`; `LoadNoteShift` decodes all
three to note shift zero. The resulting PSG indices therefore remain 0–48 with no ambiguous or
out-of-range invocation. Bits 4–6 instead set frequency-shift byte `$1D`: the three arguments produce
YM shifts 0/2/4 and PSG shifts 0/1/2 after PSG's additional right shift.

## DAC Sample Load Table

The driver's `t_SAMPLE_LOAD_DATA` has 17 eight-byte entries. Each row stores a PCM frame-period byte,
two zero/reserved bytes, a bank selector, a little-endian length, and a little-endian Z80-window
pointer. All derived ranges stay inside the selected `0x1E0000`/`0x1E8000` 32 KiB ROM bank; the
inventory records only metadata and payload hashes, never PCM bytes.

The 1,559 `sample`/`sampleL` calls in music use only indices 0–5, with counts 88, 33, 360, 572, 402,
and 104. The remaining eleven rows are outside the music corpus and may serve SFX or alternate rates.
Index bounds and ROM ranges are confirmed statically; translating the frame-period byte into audible
sample rate remains part of the shared timing observation.

## Static Command Corpus

`musicmacros.asm` defines 29 byte-emitting macros; every definition occurs in the song corpus and ten
encode loop/section/channel flow. Across 37 files, the parser owns 39 song-entry labels, 321 channel
labels, 390 channel pointer slots resolving to 321 per-file unique targets, and 39,290 macro
invocations. The 2,347 flow invocations comprise main loops, repeat sections, counted loops, and 139
channel terminators. Each macro records its parameters, exact emitted-byte expressions and width;
each song records entry/channel labels, pointer counts, directives and per-macro use counts.

The adjacent Z80 driver now closes the static consumer side too. Five YM/PSG parsers call one shared
`ParseLoopCommand`; fixed opcode families cover `70`, `F0`, and `F8`–`FF`, while six note/sample
macros compute their first byte from arguments. `FA`, `FC`, `FD`, and `FE` are explicitly
channel-specific: for example, `FA` selects stereo on YM but Timer B on PSG, and `FD` means volume on
YM but instrument on PSG. `FF 0000` terminates all channels; YM1 alone treats a nonzero low byte with
zero high byte as a queued operation, while other nonzero words are absolute Z80 jumps.

The nine `F8` parameter forms are also tied to exact channel-state offsets: main/repeat starts,
three repeat sections, repeat/main endings, and counted-loop start/end. A counted start stores
`low5 + 1`; the ending decrements offset `$19` and jumps through `$17-$18` until zero.

## Channel-Role Compatibility Audit

Every one of the 39 logical song entries has exactly ten pointers. Their order is confirmed against
the music pass in `UpdateSound`: three YM1 channels, two ordinary YM2 channels, the YM2 channel 6 / DAC
slot, three PSG tone channels, and one PSG noise channel. Across all entries this produces 117 YM1,
78 YM2, 39 YM2/DAC, 117 PSG-tone, and 39 PSG-noise pointer slots.

Pointer reuse reduces the 390 slots to 321 source labels. Of these, 108 are YM1-only, 72 YM2-only,
36 YM2/DAC-only, and 68 PSG-tone-only. Another 35 labels are shared by the PSG tone/noise slots and
contain only `channel_end`; the two all-role labels are the silent Music 32/64 bodies. Thus no active
noise sequence appears in the original song-source corpus, even though the noise parser supports
notes, instruments, release, waits, loops, and termination.

The verifier checks all 39,290 macro calls against the parser-supported roles. YM note/instrument/
volume/stereo macros remain on YM-capable slots, sample macros occur only on the channel-6/DAC slot,
PSG note/instrument macros remain on PSG slots, and `ymTimer` occurs only on PSG tone. Shared waits,
loops, release, and termination are accepted on every parser that implements them. The pinned corpus
has zero incompatible role uses. This is a static source/dispatch guarantee; it does not establish
audible timing, channel state, or hardware output.

This closes source grammar, dispatch, loop state, table selection, and effective note-index bounds—not
playback timing. DAC sample rate, live channel-state evolution, tempo, instruments, and scheduling
remain `Inferred` or `Unknown` until the coherent runtime matrix is observed.

## Complete Data-ASM Discovery

With this directory closed, every one of the pinned checkout's 1,690 `disasm/data` ASM files belongs
to a deterministic H2 inventory. This is source-discovery coverage, not semantic completion. The
domain-aware index is now 1,017/1,690: 980 files use H1 and 37 song files use the Z80 bank domain.
The remaining gap includes 662 map bodies labeled only at include sites, one unlabeled map storage
file, the four symbol-less music support/entry sources, and explicit alternates/containers.

## Concentrated Queue

No emulator was launched. Remaining questions are now grouped as DAC frame-period/sample rate plus
live channel state, and Timer-B/loop/instrument timing. They should be tested together through the
sound-command boundary rather than one launch per opcode.

## Reproduction

```powershell
uv run sf2 h2 sound-data
```
