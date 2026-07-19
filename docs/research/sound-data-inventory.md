# Z80 Music-Bank Source and ROM-Parity Inventory

- Status: **Confirmed** for the complete 41-file directory, two bank entry points, include graph,
  bank sizes/hashes/order, pointer/include counts, and canonical-ROM byte parity
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
Consequently none of the 41 files receives a false 68000 symbol binding, and strict indexed-file reach
correctly remains unchanged.

## Static Bank Parity

Both generated banks are exactly 32 KiB and byte-match the canonical ROM slices:

| Bank | ROM offset | Pointer slots | Unique targets | Song includes | SHA-256 |
| --- | ---: | ---: | ---: | ---: | --- |
| 1 | `0x1F0000` | 32 | 10 | 10 | `978575483EC8354379C5099911ADA611BC7BEC96E58E501BF29B71064C72756C` |
| 0 | `0x1F8000` | 32 | 29 | 27 | `EB1A77668279D147FC887CB5E1FBCCA3E037CDC648C63FC78AEBB158E377822D` |

The verifier reads the local generated banks and canonical ROM only for byte parity. It commits no
music bytes, note streams, instruments, PCM, or extracted audio. The generated structured inventory
stays under ignored `local/derived/sound-data-static.json`.

## Complete Data-ASM Discovery

With this directory closed, every one of the pinned checkout's 1,690 `disasm/data` ASM files belongs
to a deterministic H2 inventory. This is source-discovery coverage, not semantic completion. The
separate strict H1 index remains 980/1,690 because 662 map bodies are labeled at include sites, one
map storage file is unlabeled, 41 music sources use the Z80 address space, and the remaining explicit
alternates/containers cannot honestly claim built 68000 symbols.

## Concentrated Queue

No emulator was launched. Remaining questions are grouped as music command/channel interpretation,
tempo/loop/instrument timing, and bank-selection/fallback behavior. They should be tested together
through the sound-command boundary after the static command stream model is built.

## Reproduction

```powershell
uv run sf2 h2 sound-data
```
