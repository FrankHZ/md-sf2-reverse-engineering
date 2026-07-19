# Auxiliary Graphics, Scripting, and Technical Data Inventory

- Status: **Confirmed** for the complete 65-file boundary, 63 layout-owned files and H1 addresses,
  two alternates, file-category counts, private incbin reference counts, sprite-dialogue row shape,
  and the complete battle-background, battle-sprite, and portrait container/decode corpora
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

These belong with the existing UI/VDP and scripting runtime queues rather than separate one-case
launches.

## Reproduction

```powershell
uv run sf2 h2 auxiliary-data
uv run sf2 h2 battle-backgrounds
uv run sf2 h2 battle-sprites
uv run sf2 h2 portraits
uv run sf2 research-index test
```
