# Auxiliary Graphics, Scripting, and Technical Data Inventory

- Status: **Confirmed** for the complete 65-file boundary, 63 layout-owned files and H1 addresses,
  two alternates, file-category counts, private incbin reference counts, sprite-dialogue row shape,
  and the complete battle-background, battle-sprite, battle-sprite-animation, weapon/ground, and
  portrait container/decode corpora plus the complete regular/special map-sprite, special-screen, and base/menu UI
  pointer/decode corpora, plus the complete spell/invocation/status/transition graphics corpus
  and complete map-tileset decode/usage, map-palette/header-usage, and icon-storage corpora
- Status: **Inferred** for presentation timing and scripting consumers
- Status: **Unknown** for four grouped runtime questions
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Build Boundary

This batch covers all 44 ASM files under `data/graphics`, all 15 under `data/scripting`, all five
under `data/tech`, and the root `data/spritedialogproperties.asm`. The 65 files contain 11,234 lines
and 1,826 global labels. The original layout owns 63 files, each with a representative H1 symbol and
address.

Two graphics files are explicit exceptions:

- `data/graphics/tech/windowborder/entries.asm` is an unassembled compressed-window-border aggregate
  with no H1 symbol;
- `data/graphics/tech/windowlayouts/fighterministatuswindowlayout.asm` is an unassembled alternate
  whose `layout_MiniStatusWindow` symbol is owned in H1 by the built mini-status layout file.

Both are H2-hashed but receive no borrowed address. This yields 65/65 inventory and 63/65 strict
indexed-file reach.

## Static Shape and Copyright Boundary

The sources reference 1,495 unique private binary payloads across battle graphics, map tiles and
palettes, sprites, portraits, icons, menu/window layouts, and special screens. The verifier records
only target-path counts and source hashes. Separate battle-background and portrait rails read 27 and
52 local payloads to validate structure and ROM parity, but commit only counts and hashes—never
palettes or decoded tile data.

The 30-entry battle-background pointer table has 27 unique containers and three aliases: slots 21
and 22 reuse payload 12, while slot 29 reuses payload 13. Each container holds a 32-byte palette and
two Stack-compressed tile streams addressed through three relative-offset words. All 54 streams
decode to exactly 6,144 bytes, for 331,776 bytes total, and all table/payload bytes match the ROM.

The 56-entry portrait pointer table has 52 unique containers and four aliases. Each container holds
counted four-byte eye entries, counted four-byte mouth entries, one 32-byte palette, and a
Stack-compressed tile stream. All 52 streams decode to exactly 2,048 bytes; the corpus contains 261
eye entries and 218 mouth entries with coordinates limited to the 8×8 portrait tile grid. The four
aliases are portrait 35→33 and portraits 53-55→52.

The battle-sprite tables contain 32 ally and 54 enemy containers without pointer aliases. Header
offsets deterministically separate animation speed, two status-icon bytes, a 2-7-entry frame-offset
array, and 1-4 palettes. The 86 payloads contain 167 palettes and 408 Stack-compressed frames; all
153 ally frames decode to 4,608 bytes and all 255 enemy frames to 6,144 bytes. Pointer tables,
payloads, header boundaries, and stream output sizes are ROM-checked; image and palette bytes remain
private.

The two battle-sprite animation tables add 87 ally and 121 enemy payloads. Their 832 pointer-table
bytes and 3,800 payload bytes match ROM and parse into 421 frame entries. Ally sequences contain
eight-byte weapon-aware entries and reserve entry zero as idle frame two; enemy sequences use
four-byte entries and play all of them. The tracked contract keeps field values, counts, addresses,
and hashes while the full original sequence bytes remain private.

The weapon/ground rail covers both remaining battle-layer families. It validates 23 weapon pointers
and Stack streams, 42 contiguous four-byte weapon palettes, 30 ground pointers, 27 ground palette/
relative-pointer headers, and ten shared ground streams. Weapon outputs are fixed at 8,192 bytes and
ground outputs at 1,536; slots 21/22 reuse ground 12 and slot 29 reuses ground 13. The 53 pointer
entries and 102 source objects match ROM bytes, while the 203,776 decoded bytes remain local.

The regular map-sprite table contains 720 pointers: 240 logical IDs with three directional payloads
each. It resolves to 670 source payloads and 50 aliases. The Basic decoder expands 669 payloads to a
fixed 576 bytes each (385,344 bytes total), with all 720 pointers and all 670 payload byte ranges
checked against the ROM. The sole non-stream payload, `Mapsprite237_0`, is exactly `0xFFFF` and backs
the nine slots for reserved IDs 237-239. Those IDs remain below the special-sprite cutoff at 240, so
their static unreachability from the regular loader is an explicit runtime/data-flow question rather
than an assumed property. Decoded sprites and compressed source bytes remain private; the tracked
contract contains only hashes, counts, codec statistics, aliases, addresses, and the sentinel shape.

The special-sprite family adds ten pointers, five palette-bearing initial containers, and one
animation-only stream. The six Stack streams decode to 16,704 bytes in total and occupy one
contiguous 6,742-byte ROM range. Pointer index is `255 - mapSpriteId`: ten pointer entries exist, but
the load/update dispatch tables each expose only indices 0-8. Thus 247-255 are fully routed, 246 is
pointer-only, and 240-245 are unbacked. Symbolic source references use only 251-255. This contract
records the asymmetry without claiming that name absence proves runtime unreachability.

The nine Stack-compressed special-screen tile resources are also complete. Their 23,296 compressed
bytes decode to 50,176 bytes, with nine source ranges and six direct pointers checked against H1 and
ROM. Three fixed transfers exactly match output; five exceed it by 27,648 aggregate bytes, and the
ending-kiss picture uses a pixel-fill consumer instead. Tail contents remain a runtime question;
the tracked contract does not invent zero padding or retain any tile bytes.

The base/menu UI family adds eight Stack streams: one 8,192-byte base output shared by startup and
ending credits, six 2,304-byte diamond-menu outputs, and one 1,152-byte yes/no output. All 23,168
decoded bytes, eight source pointers, and the nine-entry heterogeneous menu table have ROM parity.
The table's first three high-bit values select uncompressed main-menu icon combinations; its last
six are compressed-resource pointer indirections.

Battle effects add 23 spell containers, four invocation containers with 15 frames and 30 streams,
one status-animation stream, and two battle-transition streams. All 56 streams decode to 200,992
bytes; 30 resource containers, four top-level pointers, and three pointer tables match the ROM. Each
invocation stream produces 4,096 bytes but its consumer transfers 4,608, leaving a 512-byte tail per
stream whose runtime contents are not inferred from static shape.

The map-tileset table adds 115 Stack payloads. Every stream decodes to 4,096 bytes; all 115 pointers
and payloads match ROM for 471,040 decoded bytes total. The same rail validates 79 map headers with
395 slots and 32 animation headers. Ordinary and animation references jointly use 114 indices;
`MapTileset029` is the sole statically unreferenced resource and is not declared runtime-unreachable.

The map-palette table adds sixteen 32-byte payloads and sixteen pointer entries. The payloads contain
256 mask-valid Genesis color words (69 unique source values), and all sixteen palette indices appear
across the 79 map headers. Source/H1/ROM parity covers the top-level pointer, table, payloads, and map
header bytes. Fifteen source palettes begin with a nonzero word, but `LoadMap` clears color 0 after
copying; every effective map palette therefore begins with zero. The verifier tracks only hashes,
counts, usage, and this consumer rule—not palette bytes.

The icon source directory has 167 fixed 192-byte payloads, while the original layout assembles 163
contiguously after `p_Icons`: 127 item, 30 spell, and six special/other entries. Every assembled byte
and the 192-byte highlight mask match ROM. The four files absent from the build are item 127 and
spell 16-18; the built slots at those logical positions instead encode nothing and three named
special icons. Tracked output retains paths, counts, addresses, and hashes, never pixel bytes.

The sprite-dialogue table has 119 aligned `mapsprite`, `portrait`, and `speechSfx` rows. This confirms
the table shape, not the presentation behavior or the meaning of individual entries. Likewise, the
scripting inventory proves build ownership and symbol placement without redistributing text banks,
staff strings, cutscene content, or entity-action bodies. Full detail remains under ignored
`local/derived/auxiliary-data-static.json`.

## Concentrated Queue

No emulator was launched. Remaining questions are grouped as:

1. window-layout and VDP presentation behavior;
2. map palette/fade behavior and map/battle sprite animation frame timing;
3. entity-action and global-cutscene dispatch effects;
4. configuration, debug, fading, and spell-animation data consumers.

The complete symbolic source scan found no uses of regular IDs 237-239 or special IDs 240-250.
Encoded records and runtime sprite-ID writes are the remaining reachability surface; if static data
decoding cannot close it, all reserved IDs join one entity-sprite runtime matrix rather than separate
emulator launches.

These belong with the existing UI/VDP and scripting runtime queues rather than separate one-case
launches.

## Reproduction

```powershell
uv run sf2 h2 auxiliary-data
uv run sf2 h2 battle-backgrounds
uv run sf2 h2 battle-sprites
uv run sf2 h2 battle-sprite-animations
uv run sf2 h2 battle-weapon-ground
uv run sf2 h2 portraits
uv run sf2 h2 map-sprites
uv run sf2 h2 special-sprites
uv run sf2 h2 special-screen-graphics
uv run sf2 h2 ui-graphics
uv run sf2 h2 icon-graphics
uv run sf2 h2 battle-effect-graphics
uv run sf2 h2 map-tilesets
uv run sf2 h2 map-palettes
uv run sf2 research-index test
```
