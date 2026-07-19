# Map Content Tables and Binary Payload Parity

- Status: **Confirmed** for all 79 map entries, all 662 source-form content sections, all 154
  private block/layout payloads, record sizes, aggregate record counts, their canonical ROM bytes,
  the deterministic engine-neutral import assembled from them, and the static flag/roof/step/warp
  consumer phases, scan policies, and path-specific working-layout rebuild/preservation rules
- Status: **Unknown** for rendered layout parity and exact VDP animation frame timing
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

- map construction loads blocks, decodes the layout, applies every set flag-copy in source order,
  applies every set chest marker, and finally overlays battle bounds when applicable; overlapping
  flag copies therefore leave the later copy's words;
- area selection consumes the first eight coordinate bytes and skips the remaining 22 bytes per
  rejected record;
- roof-on-load selects the first record whose rectangle contains the controlled entity; step and
  warp consumers select the first coordinate match, with a negative warp coordinate byte acting as
  that axis's wildcard;
- controlled walking checks target markers in the order enter-caravan, enter-raft, door, warp, zone,
  then passability. All use one masked marker value and are mutually exclusive before mutation. A
  door copy is applied first and the target block is re-read, so that same move can subsequently
  observe a newly exposed warp or zone marker;
- flag records are consumed during layout construction, roof records after area selection on map
  load, and step/warp records from controlled movement. They are not four competing callbacks at one
  common dispatch point;
- a nonnegative `LoadMap` argument and every scrolling warp rebuild the target working layout from
  source before replaying persistent flag/chest state. A negative current-map reload skips both
  block and layout decoding and preserves the existing 8 KiB working layout before roof evaluation.
  The explicit `ResetCurrentMap` path first clears those 8 KiB and then deliberately uses that
  preserving reload path;
- step and roof records advance by eight bytes, item records by four, and warp records by the
  eight-byte enum size;
- `VInt_UpdateMapAnimations` consumes the selected map's animation table during vertical interrupt
  handling.

## Private Binary Payloads

Seventy-seven maps own one `0-blocks.bin` and one `1-layout.bin`. Maps 24 and 46 intentionally alias
both payloads from maps 23 and 7 respectively, so all 79 entry records are covered by 77 payload
pairs. The 154 files total 193,678 bytes:

| Payload | Files | Total bytes | Min | Max |
| --- | ---: | ---: | ---: | ---: |
| blocks | 77 | 130,598 | 144 | 4,492 |
| layouts | 77 | 63,080 | 94 | 2,114 |

These files remain ignored private inputs. The generated local report records per-file address, size,
and SHA-256 and proves every byte equals the ROM slice at its H1 label. The tracked fixture retains
only aggregate sizes and parity counts; it does not redistribute hashes or payload content.

The Python decoder now reproduces both 68000 bitstream consumers. The 77 blocksets expand to 19,771
3x3 blocks (177,939 words), from 22 to 666 blocks per payload. Their paired layouts each expand to
exactly 4,096 words (8 KiB), for 315,392 words total. Every decoded layout block index is in range;
the global maximum is 665 against the largest 666-block set.

The block decoder covers all eight command families: repeat, adjacent tile, right/bottom history,
and relative/absolute values with reused or new flags. The layout decoder covers next sequential
block, left/upper run copy, left/upper four-entry MRU history, and fixed-width literal commands. Full
decoded hashes and per-map command counts remain ignored local metadata, not tracked map content.

## Canonical Engine-Neutral Import

`sf2-canonical-map-import-v1` now joins the pointer graph, decoded blocksets/layouts, logical
source-form records, and setup selection graph into one deterministic JSON data contract. It contains
79 map definitions and 1,859 identity-preserving resources. The content side owns 77 blocksets, 77
layouts, five families of 79 event/content tables, 156 item tables, and 32 animation tables. The setup
side adds 64 routes, 126 six-pointer definitions, 125 entity lists, 263 event handlers, 75 area-
description handlers, 90 init functions, 178 standalone labeled programs, and 201 additional
init-source programs. Together they contain 19,771 blocks, 315,392 layout words, and 15,805 logical
records/operations. Every non-null map/setup
reference resolves to a resource.

The import does not inline shared resources into each map. Maps 24 and 46 retain their original
block/layout aliases; 41 animation references resolve to 32 tables; maps 47 and 58 deliberately use
their chest tables for the other-item slot. All 38 null animation pointers stay null. Layout and
block words retain their raw 16-bit values, so unknown flags are not lost through premature naming.

Setup routes preserve source order and the confirmed last-set-flag-wins selector. Fifteen map IDs
without a routing row retain a null setup-route reference. Each setup definition independently
references its entity list, entity/zone/item event handlers, description handler, and init function.
Direct-`rts` handlers remain explicit resources with no invented event records; their runtime
reachability remains an open question.

Each init-function resource retains ordered opcodes/operands, labels, script/direct-call targets,
and resolved local or H1-addressed branch edges. This is syntax and control-flow evidence only: an
opcode name does not by itself prove its persistent story-state or presentation effect.

Standalone programs cover all 8,058 statements and resolve 100 operand references to another of the
178 labels. Twelve of the 75 init `script` targets are owned here; the other 63 resolve through the
201 embedded init-source programs. The import now rejects any unresolved init script target.

The verifier builds the full 12.0 MB output twice and requires byte-identical canonical JSON, then
checks its tracked digest and schema. The full output remains under ignored `local/derived/`; the
tracked fixture stores only geometry, counts, alias facts, and provenance. This closes the data-import
boundary without redistributing the original maps.

## Concentrated Runtime Queue

The ten-case setup-selector H3 matrix now confirms default, missing-map, last-set-flag-wins, and
alias-pointer selection through the natural debug Map Test exploration path. It does not close map
transition or presentation behavior. A following six-case init-dispatch matrix confirms missing-map
skip and exactly one modeled indirect init target for active, scripted, and direct-return setups.
A following nine-case event-dispatch matrix confirms entity, zone, and item first-match selection in
one BizHawk launch without executing the selected scripts. Static parsing now closes the consumer
phases, first-match/all-match policies, overlap direction, movement-marker exclusivity, target check
order, and working-layout rebuild/preservation by load path. Two coherent presentation questions
remain:

1. VInt/VDP frame timing for animation table updates;
2. decoded block/layout rendered-map parity against the VDP presentation path.

Both belong with the graphics/VDP batch; neither justifies a one-map runtime fixture now.

## Reproduction

```powershell
uv run sf2 h2 map-content
uv run sf2 h2 map-layouts
uv run sf2 h2 map-import
uv run sf2 h2 map-data
uv run sf2 h3 map-event-dispatch
uv run sf2 research-index test
```

Detailed outputs are written to ignored `local/derived/map-content-static.json`,
`local/derived/map-layout-decode.json`, and `local/derived/canonical-map-import.json`. The tracked
fixtures contain only compact non-content evidence.
