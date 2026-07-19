# Auxiliary Graphics, Scripting, and Technical Data Inventory

- Status: **Confirmed** for the complete 65-file boundary, 63 layout-owned files and H1 addresses,
  two alternates, file-category counts, private incbin reference counts, sprite-dialogue row shape,
  and the complete battle-background, battle-sprite, weapon/ground, and portrait container/decode
  corpora plus the complete regular/special map-sprite and special-screen pointer/decode corpora
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

The sprite-dialogue table has 119 aligned `mapsprite`, `portrait`, and `speechSfx` rows. This confirms
the table shape, not the presentation behavior or the meaning of individual entries. Likewise, the
scripting inventory proves build ownership and symbol placement without redistributing text banks,
staff strings, cutscene content, or entity-action bodies. Full detail remains under ignored
`local/derived/auxiliary-data-static.json`.

## Concentrated Queue

No emulator was launched. Remaining questions are grouped as:

1. window-layout and VDP presentation behavior;
2. map/battle sprite animation frame timing;
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
uv run sf2 h2 battle-weapon-ground
uv run sf2 h2 portraits
uv run sf2 h2 map-sprites
uv run sf2 h2 special-sprites
uv run sf2 h2 special-screen-graphics
uv run sf2 research-index test
```
