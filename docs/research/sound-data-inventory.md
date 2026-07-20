# Z80 Music-Bank Source and ROM-Parity Inventory

- Status: **Confirmed** for the complete 41-file directory, two bank entry points, include graph,
  bank sizes/hashes/order, pointer/include counts, 37 song ranges/entries, the 29-macro ABI and
  complete source invocation corpus, and canonical-ROM parity
- Status: **Inferred** for music command semantics and bank selection
- Status: **Unknown** for three grouped runtime questions
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

## Static Command Corpus

`musicmacros.asm` defines 29 byte-emitting macros; every definition occurs in the song corpus and ten
encode loop/section/channel flow. Across 37 files, the parser owns 39 song-entry labels, 321 channel
labels, 390 channel pointer slots resolving to 321 per-file unique targets, and 39,290 macro
invocations. The 2,347 flow invocations comprise main loops, repeat sections, counted loops, and 139
channel terminators. Each macro records its parameters, exact emitted-byte expressions and width;
each song records entry/channel labels, pointer counts, directives and per-macro use counts.

This closes source grammar and command inventory, not playback meaning. Note/sample pitch decoding,
loop execution, tempo, instruments, and channel scheduling remain `Inferred` or `Unknown` until the
Z80 driver consumer is modeled and the coherent runtime matrix is observed.

## Complete Data-ASM Discovery

With this directory closed, every one of the pinned checkout's 1,690 `disasm/data` ASM files belongs
to a deterministic H2 inventory. This is source-discovery coverage, not semantic completion. The
domain-aware index is now 1,017/1,690: 980 files use H1 and 37 song files use the Z80 bank domain.
The remaining gap includes 662 map bodies labeled only at include sites, one unlabeled map storage
file, the four symbol-less music support/entry sources, and explicit alternates/containers.

## Concentrated Queue

No emulator was launched. Remaining questions are grouped as music command/channel interpretation,
tempo/loop/instrument timing, and bank-selection/fallback behavior. They should be tested together
through the sound-command boundary after the static command stream model is built.

## Reproduction

```powershell
uv run sf2 h2 sound-data
```
