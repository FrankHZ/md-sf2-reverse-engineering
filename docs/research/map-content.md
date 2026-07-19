# Map Content Tables and Binary Payload Parity

- Status: **Confirmed** for all 79 map entries, all 662 source-form content sections, all 154
  private block/layout payloads, record sizes, aggregate record counts, and their canonical ROM bytes
- Status: **Inferred** for transition-time ordering and persistence across flag/step/roof/warp consumers
- Status: **Unknown** for rendered block/layout parity and exact VDP animation frame timing
- Evidence date: 2026-07-19
- ROM SHA-256: `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Complete Static Boundary

`pt_MapData` at ROM `0x94B8A` contains 79 big-endian pointers. Each target begins with six inline
bytes (palette and five tileset indices) followed by ten longword slots, so a map entry is 46 bytes:

| Offset | Slot |
| ---: | --- |
| `0` | palette and five tilesets (6 bytes) |
| `6` | compressed block payload |
| `10` | compressed layout payload |
| `14` | area descriptions |
| `18` | flag-triggered block copies |
| `22` | step-triggered block copies |
| `26` | roof/layer-2 block copies |
| `30` | warps |
| `34` | chest items |
| `38` | other searchable items |
| `42` | tile animations |

The verifier independently encodes the pointer table and every 46-byte map entry from source and H1
symbols before comparing them with the ROM. All 79 entries pass. Thirty-eight optional slots contain
`$FFFFFFFF`; these are retained as null pointers rather than invented empty tables.

The pinned `sf2enums.asm` declares `MAPDATA_OFFSET_LAYOUT` as `8`, but the actual longword begins at
offset `10`. No assembled code references the enum. `LoadMapBlocksAndLayout` consumes the blocks and
layout pointers sequentially from offsets 6 and 10, and the reconstructed entry bytes confirm that
layout. This is a **Confirmed upstream constant defect**, not a reason to encode the wrong offset in
the remake contract.

## Source-Form Sections

The 662 include-site-labeled ASM sections are no longer inventory-only. The Python rail interprets
the pinned `sf2mapmacros.asm`, resolves enum shorthands, re-encodes every value in big-endian order,
and compares all 12,576 resulting bytes with the canonical ROM.

| Section | Files | Logical records |
| --- | ---: | ---: |
| palette/tileset bytes | 79 | 474 bytes |
| areas | 79 | 135 |
| flag block copies | 79 | 71 |
| step block copies | 79 | 94 |
| roof/layer-2 copies | 79 | 114 |
| warps | 79 | 409 |
| chest items | 79 | 50 |
| other items | 77 | 46 |
| animations | 32 | 108 entries in 32 tables |

An area record is 30 bytes. Flag, step, roof, warp, and animation-entry records are eight bytes;
items are four bytes; animation headers are four bytes. Variable tables use a two-byte `$FFFF`
terminator. Three other-item sections include an assembled `rts` after their terminator (maps 5, 10,
and 64); those six code bytes are part of source/ROM parity but are unreachable to the item scanner,
which stops when the first coordinate byte is negative.

Static consumers confirm the important ownership rules:

- flag events are applied while `LoadMapBlocksAndLayout` constructs the working layout;
- area selection consumes the first eight coordinate bytes and skips the remaining 22 bytes per
  rejected record;
- step and roof records advance by eight bytes, item records by four, and warp records by the
  eight-byte enum size;
- `VInt_UpdateMapAnimations` consumes the selected map's animation table during vertical interrupt
  handling.

## Private Binary Payloads

Maps 0-76 each own one `0-blocks.bin` and one `1-layout.bin`; maps 77-78 use null pointers. The 154
files total 193,678 bytes:

| Payload | Files | Total bytes | Min | Max |
| --- | ---: | ---: | ---: | ---: |
| blocks | 77 | 130,598 | 144 | 4,492 |
| layouts | 77 | 63,080 | 94 | 2,114 |

These files remain ignored private inputs. The generated local report records per-file address, size,
and SHA-256 and proves every byte equals the ROM slice at its H1 label. The tracked fixture retains
only aggregate sizes and parity counts; it does not redistribute hashes or payload content.

`LoadMapLayoutData` deterministically expands a payload to the 8 KiB working layout using literal,
copy-from-up, copy-from-left, and history-stack commands. The consumer and input byte identity are
confirmed, but a canonical Python decoder and rendered tile comparison are still separate work. They
remain one grouped question rather than 77 emulator launches.

## Concentrated Runtime Queue

No emulator was launched for this batch. Static evidence leaves three coherent later matrices:

1. transition event precedence and state persistence across flag, step, roof, and warp processing;
2. VInt/VDP frame timing for animation table updates;
3. decoded block/layout and rendered-map parity after a Python-owned decompressor exists.

The first matrix should share one map-transition observation seam. The latter two belong with the
graphics/VDP batch; neither justifies a one-map runtime fixture now.

## Reproduction

```powershell
uv run sf2 h2 map-content
uv run sf2 h2 map-data
uv run sf2 research-index test
```

The detailed output is written to ignored `local/derived/map-content-static.json`. The tracked
fixture is `tests/fixtures/h2/map-content-static-v1.json`.
