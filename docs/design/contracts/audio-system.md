# Audio Command and Playback-State Contract

- **Confirmed original structure:** the two 32 KiB Z80 music banks, 64 music-command slots,
  39 unique music targets, 24-byte music headers, ten channel-state roles, 29-macro source
  language, frequency/sample/instrument index domains, 56 embedded SFX commands, and the bounded
  four-command runtime state matrix described below.
- **Inferred original behavior:** audible instrument and envelope meaning, and the player-facing
  meaning suggested by source enum names.
- **Unknown original behavior:** wall-clock tempo, PCM sample rate, exact YM2612/PSG output,
  audible waveform parity, complete loop/fade/resume semantics, and SFX priority/interruption.
- Remake status: implementation-neutral Phase 3 contract; no audio middleware, asset format,
  replacement soundtrack, mixing policy, or hardware-fidelity target has been selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the accepted identity and state boundary between gameplay sound commands and
an audio implementation. It owns:

1. music and SFX command namespaces and their original lookup domains;
2. music-bank, pointer-slot, target, header, and channel-role identities;
3. source-level macro, note, frequency, sample, instrument, and SFX-stream index validity;
4. the bounded live state observed after four representative music commands;
5. a distributable metadata contract that never requires extracted music, PCM, or other sound bytes.

It does not require a remake to emulate the Z80 driver, YM2612, PSG, ROM banking, or original source
macro language. It also does not assign soundtrack meaning to scenes, prove that enum labels match
what a player hears, define mix levels, or close transitions, fades, resume behavior, SFX contention,
presentation timing, or accessibility policy.

The executable evidence owners are:

- `sf2-sound-data-static-v1` in
  [`tests/fixtures/h2/sound-data-static-v1.json`](../../../tests/fixtures/h2/sound-data-static-v1.json);
- `sf2-sound-timing-runtime-v1` in
  [`tests/fixtures/h3/sound-timing-v1.json`](../../../tests/fixtures/h3/sound-timing-v1.json);
- `sf2-gameflow-core-static-v1` in
  [`tests/fixtures/h2/gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json),
  only for the `InitializeZ80` startup handoff.

The principal research owner is the
[Z80 music-bank and runtime-state inventory](../../research/sound-data-inventory.md). The startup
edge is additionally bounded by [gameflow research](../../research/gameflow-core.md). The accepted
[ADR 0005](../../decisions/0005-remake-value-driven-driver-freeze.md) freezes deeper hardware and
driver exactness unless a concrete remake acceptance need reopens one question.

## Pre-Contract Evidence Audit

This synthesis was checked against both dedicated fixtures, their H2/H3 verifiers, the generated
static inventory, the driver command branches, and the source-owned song-bank include graph. Fresh
reproduction passed for both dedicated owners on the evidence date. The audit preserves these
limits:

- 37 indexed `sound.music.*` records identify source-backed song payload ranges; they do not grant
  redistribution rights or make the raw payloads part of the public contract;
- the one shared `gameflow.start.z80-init` record confirms the startup routine and live Z80 state
  addresses, but its H2 binding does not prove a byte-count interpretation or audible startup result;
- `commandSlotCount = 64` is the music lookup width, not a count of unique tracks or named enums;
- 39 unique targets include deliberate aliases, while only 37 source files exist because Music 3/4
  and Music 13/14 share file ranges;
- source enum names and labels are preserved identities, not evidence of scene use, musical title,
  mood, or player-visible meaning;
- emulator checkpoints at frames 1, 10, and 30 are deterministic state samples, not a conversion to
  YM timer frequency, musical beats, seconds, PCM rate, or audible completion;
- the SFX stream parser confirms the currently reachable token and counted-loop shape, not elapsed
  playback length, register output, priority, interruption, or mixing;
- the fixture's `strictIndexExclusion` keeps the two bank entry files and two macro/enum sources out
  of symbol credit because they do not have independent entry symbols.

No contradictory count, range, bank selector, or live checkpoint was found. The original driver
comments occasionally describe comparison branches colloquially; this contract uses the executed
boundaries and verified command ranges rather than expanding those comments.

## Identity Domains

An implementation MUST keep these identities separate:

| Domain | Confirmed original boundary |
| --- | --- |
| gameplay command | raw command value submitted to the sound driver; music occupies `0x01..0x40`, embedded SFX occupies `0x41..0x78`, and special operations are separate driver branches |
| music pointer slot | one of 64 ordered two-byte pointers, selected within one of two banks |
| music target | one of 39 unique header targets reached by those 64 slots; aliases are intentional |
| song source file | one of 37 contiguous payload ranges; a file may contain more than one logical target |
| music header | one 24-byte control record with four leading bytes and ten channel pointers |
| channel slot | one of ten ordered playback roles; pointer reuse does not merge roles |
| source channel label | one of 321 per-file unique pointer targets referenced by 390 header slots |
| sample table row | one of 17 metadata rows; only indices 0 through 5 appear in the accepted music corpus |
| instrument index | a YM or PSG table selection, distinct from audible instrument identity in a modern engine |
| SFX command/header | one of 56 contiguous command/header entries, each with type-specific channel pointers |
| distributable audio asset | a separately licensed or original modern asset; never implied by original ROM/source metadata |

A remake MAY map multiple original commands to one licensed modern asset or use different assets for
aliases. Such a mapping is an explicit product decision. The canonical import must retain original
command, slot, target, and alias identities so compatibility and deliberate deviations remain
reviewable.

## Music Command and Bank Selection

**Confirmed static:** command `0` is ignored by the driver's main loop. Commands `0x01..0x20`
select bank register value `1`, map the bank stored at ROM `0x1F8000`, and index bank 0 with
`command - 1`. Commands `0x21..0x40` select register value `0`, map ROM `0x1F0000`, and index bank 1
with `command - 33`. Values from `0x41` enter the embedded SFX path after the driver's earlier
special-operation comparisons.

Each bank is exactly 32 KiB and byte-matches its canonical ROM slice:

| Logical bank | ROM offset | Pointer slots | Unique targets | Included song files |
| --- | ---: | ---: | ---: | ---: |
| bank 0 | `0x1F8000` | 32 | 29 | 27 |
| bank 1 | `0x1F0000` | 32 | 10 | 10 |

All 64 slots resolve to a source label whose first header byte is zero, so none takes the driver's
nonzero-header redirect into SFX loading. The 38 named nonzero music enums resolve to their matching
slots. Commands 29 through 32 alias `Music_32`; commands 42 through 64 alias `Music_64`. The whole
table contains 39 unique targets and no cross-bank target.

An original-fidelity adapter MUST preserve the raw command-to-slot-to-target relation, including
aliases and bank identity. A modern runtime MAY replace bank loading with an asset lookup, but it
MUST NOT silently collapse command IDs in imported data or infer a track's use from its enum name.

## Music Header and Channel Shape

**Confirmed static:** every unique music target starts with a 24-byte header:

1. a zero type marker;
2. a DAC-disable byte;
3. a reserved Timer A byte;
4. a YM Timer B byte;
5. ten little-endian channel pointers.

Nineteen headers leave DAC enabled and 20 disable it. Every reserved Timer A byte is zero. Timer B
has 19 distinct values from `0xBD` through `0xD4`. These are stored fields and verified driver reads;
they are not a wall-clock tempo contract.

The ten pointer positions retain this order:

| Slots | Original role | Pointer-slot count across 39 targets |
| --- | --- | ---: |
| 0..2 | YM1 channels 1..3 | 117 |
| 3..4 | YM2 channels 4..5 | 78 |
| 5 | YM2 channel 6 / DAC | 39 |
| 6..8 | PSG tone channels | 117 |
| 9 | PSG noise | 39 |

Pointer reuse reduces 390 slots to 321 source labels. Thirty-five labels are shared by PSG tone and
noise slots and contain only channel termination; two silent labels are shared by all roles. These
facts preserve topology. They do not require a modern mixer to expose ten hardware-shaped voices.

## Static Command-Language Boundary

**Confirmed static:** `musicmacros.asm` defines 29 byte-emitting macros and all 29 occur in the song
corpus. Ten express flow. Across 37 files the parser closes 39 entry labels, 321 channel labels,
390 channel pointer slots, 39,290 macro invocations, and 2,347 flow invocations. All invocations are
compatible with the parser role reached from their header slot; the accepted corpus has zero role
violations.

The original grammar is evidence for lossless import and diagnostics, not a required remake
sequencer API. A canonical importer MUST preserve enough structured identity to distinguish:

- source entry and channel labels;
- ordered header roles and target aliases;
- macro name, operands, emitted width, and flow classification when importing original source;
- explicit main, repeat-section, counted-loop, and channel-end forms;
- channel-specific meanings for shared opcode bytes.

A modern authoring format MAY normalize those fields after import, provided a deterministic adapter
can prove the accepted command, role, and control-flow relationships. It MUST NOT assign one global
meaning to bytes such as `FA`, `FC`, `FD`, or `FE`, whose consumer differs by channel family.

## Note, Frequency, Sample, and Instrument Domains

**Confirmed static:** the source defines 108 semitone names from C0 through B8. The driver contains
an 84-entry YM table and a 64-entry PSG table. The 16,636 YM note calls use raw indices 1 through 71;
the 5,205 PSG note calls use effective indices 0 through 48. All are in range. The accepted shift
audit finds no ambiguous or out-of-range PSG use; its three encoded shift arguments retain decoded
note shift zero while selecting separate YM/PSG frequency-shift state.

The 17-row DAC load table stores range metadata. Music contains 1,559 sample calls and uses indices
0 through 5; the other eleven rows have no accepted music-corpus use. The metadata verifier binds
selected ROM bank, range, and payload hash without publishing sample bytes. It does not determine
sample rate or license.

The 922 YM instrument calls use 51 indices within 0 through 63. The 908 packed PSG instrument calls
use seven high-nibble indices and levels 0 through 14. All 6,546 volume calls use levels 0 through
14. These are valid selection domains. Audible timbre, envelope, loudness, and update timing remain
**Inferred** or **Unknown** and MUST NOT be synthesized from index validity alone.

## Embedded SFX Boundary

**Confirmed static:** the 8,192-byte original driver contains a 56-entry SFX pointer table and
payload region. Commands `0x41..0x78` map contiguously to 56 named enums and 56 source headers.
Twenty-eight type-1 headers each provide ten pointers; 28 type-2 headers each provide three, for 364
references to 115 unique targets. The first target byte classifies 66 active references and 298
immediate-inactive references.

The deterministic stream parser walks all 66 active starts. It closes 792 traversed tokens at 786
unique starts and 1,447 unique decoded bytes. All encountered `FF` words are zero, so the accepted
corpus has no absolute redirect edge. Fourteen `F8` tokens form seven matched counted-loop edges
across six streams; the maximum encoded iteration count is 17.

An original-fidelity import MUST retain command ID, header type, ordered channel targets, immediate
inactive entries, token widths, termination, and counted-loop edges. It MUST NOT convert these
static facts into elapsed duration, audible equivalence, priority, interruption, or mix policy.

## Bounded Runtime Playback State

`sf2-sound-timing-runtime-v1` observes four commands in one BizHawk 2.11.1 / Genesis Plus GX launch:

| Case | Command | Static selection | Bounded runtime result |
| --- | ---: | --- | --- |
| bank-0 DAC case | 1 | `Music_1`, bank register 1, DAC enabled | command consumed; eight active and two inactive slots; pointers/counters evolve |
| bank-0 no-DAC case | 8 | `Music_8`, bank register 1, DAC disabled | command consumed; eight active and two inactive slots; pointers/counters evolve |
| bank-1 DAC case | 33 | `Music_33`, bank register 0, DAC enabled | command consumed; eight active and two inactive slots; pointers/counters evolve; pending-sample byte changes before frame 10 |
| silent alias case | 32 | `Music_32`, bank register 1 | command consumed; all ten slots inactive; pointers/counters remain unchanged through frame 30 |

**Confirmed runtime:** at frames 1, 10, and 30, each case preserves the selected bank and header DAC
field; every frame-1 cursor equals its source/ROM-verified header pointer; and the command mailbox is
zero by frame 1. The matrix owns 12 checkpoints and 120 channel snapshots. Its observed fade-counter,
pending-sample, pointer, counter, and inactive values remain exact fixture data.

This confirms command acceptance and bounded state evolution. It does not prove audible output,
normal gameplay reachability, video-frame-to-audio-time conversion, completion, looping, fade
semantics, resume semantics, or behavior outside the sampled window.

## Startup and Caller Boundary

**Confirmed static:** `InitializeZ80` is ordered during system initialization and calls the original
copy helper while loading the separately generated sound-driver binary into Z80 memory. This is the
only claim taken from `sf2-gameflow-core-static-v1` here. Driver byte count, bus timing, reset timing,
electrical behavior, failure behavior, and first audible output are not closed by that function's
fixture binding.

Gameplay callers, map scripts, battle selectors, and presentation systems own their own command
selection and ordering. This contract accepts a raw command at the sound boundary; it does not
associate commands with maps, battles, story beats, menus, or visible actions. In particular, the
map-script sound trap is an adjacent interpreter seam and its research-index records are deliberately
excluded from this contract's association set.

## Fidelity and Modernization Boundary

Original-compatible behavior requires preserving:

- raw command identity and the distinction between music, SFX, and special operations;
- the two-bank slot relation, target aliases, header fields, and ordered channel roles;
- source-valid macro/control-flow and table-index domains when importing original data;
- the accepted SFX header/token topology;
- the four-case runtime checkpoint facts when implementing an original-driver compatibility adapter.

A modern remake may deliberately choose:

- licensed replacement music or newly authored audio;
- streaming files, middleware events, software synthesis, or another playback architecture;
- a smaller or larger voice model than the original ten slots;
- new mixing, ducking, accessibility, pause, resume, and transition policies;
- platform-specific latency and quality targets.

Those choices must be recorded as product or architecture decisions. Publicly available MIDI,
arrangements, recordings, or samples are candidates only after provenance and redistribution rights
are established. Original ROM banks, driver bytes, note streams, instruments, PCM, captured audio,
and derived playable assets remain private/generated inputs and MUST NOT be committed or distributed.

## H4 Acceptance Surface

A remake-side adapter can claim this contract only when automated tests prove:

1. all imported original command IDs retain their music/SFX/special-operation class;
2. all 64 music slots retain selected bank, ordered slot, target identity, and alias relation;
3. all 39 headers retain their four control bytes and ten ordered pointer-role identities;
4. imported macro/control-flow records and note/sample/instrument indices remain within the accepted
   role and table domains;
5. all 56 SFX commands retain type, ordered target topology, inactivity markers, token widths,
   termination, and counted-loop edges;
6. an original-driver compatibility adapter, if provided, matches the four accepted command cases at
   all 12 checkpoints and 120 channel snapshots;
7. public test artifacts contain metadata, hashes, and synthetic inputs only, never extracted or
   captured original audio payloads;
8. intentional replacements or behavior changes are reported separately from original parity.

H4 does not require chip-register, waveform, PCM-rate, or wall-clock timing parity unless a later
decision explicitly reopens that frozen scope.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| two banks, 37 song ranges, 64 command slots, 39 targets, aliases, headers, role topology | **Confirmed static** | `sf2-sound-data-static-v1` ([`sound-data-static-v1.json`](../../../tests/fixtures/h2/sound-data-static-v1.json)) | Scene use, enum meaning, redistribution rights, audible output |
| macro/control-flow corpus and note/frequency/sample/instrument index domains | **Confirmed static** | `sf2-sound-data-static-v1` ([`sound-data-static-v1.json`](../../../tests/fixtures/h2/sound-data-static-v1.json)) | Tempo, sample rate, timbre, envelope, loudness |
| 56 SFX commands, header topology, active-stream tokenization, counted loops | **Confirmed static** | `sf2-sound-data-static-v1` ([`sound-data-static-v1.json`](../../../tests/fixtures/h2/sound-data-static-v1.json)) | Priority, interruption, duration, mixing, audible equivalence |
| four command cases, 12 checkpoints, 120 channel snapshots | **Confirmed bounded runtime** | `sf2-sound-timing-runtime-v1` ([`sound-timing-v1.json`](../../../tests/fixtures/h3/sound-timing-v1.json)) | Natural reachability, completion, loop/fade/resume semantics, wall-clock time |
| startup sound-driver handoff | **Confirmed static** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Byte-count interpretation, bus/reset timing, failures, first audible output |
| audible instrument/envelope meaning | **Inferred** | [sound research](../../research/sound-data-inventory.md) | Requires a bounded reopened question if remake acceptance needs it |
| hardware/waveform fidelity and modern asset policy | **Unknown / deliberate design** | [ADR 0005](../../decisions/0005-remake-value-driven-driver-freeze.md) | Frozen unless a named trigger reopens one bounded question |

## Reproduction

```powershell
uv run sf2 h2 sound-data
uv run sf2 h3 sound-timing --timeout-seconds 180
uv run sf2 design-contracts test
uv run sf2 verify
```
