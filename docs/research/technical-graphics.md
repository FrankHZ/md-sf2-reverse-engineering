# Technical Graphics and Decompression Services

- Status: **Confirmed** for the pinned 11-file layout-owned inventory, H1 entry addresses, the two
  decompression entry contracts, display initialization order, sprite links, palette interpolation,
  special-sprite routing, view parallax gates, flash-script words, and the complete battle-terrain,
  battle-background, and portrait Stack-compression corpora
- Status: **Inferred** for visual intent where static state/register routing is clear but no rendered
  frame has been compared
- Status: **Unknown** for remaining Basic and embedded Stack-compression corpora, exact VDP timing,
  palette presentation, portrait animation timing, and special-sprite frame output
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Complete Static Boundary

All 11 files under `code/common/tech/graphics` are directly included by the pinned layout and bind to
representative H1 symbols. The boundary contains 2,137 lines, 209 global labels, 34 direct calls, and
three indirect calls. It includes basic/stack decompression, display setup and scrolling helpers,
sprite initialization, palette transitions, special sprites, the white-flash script, and explicitly
unused display/graphics helpers.

## Decompression Contract

`LoadBasicCompressedData` and `LoadStackCompressedData` both accept source in `a0`, destination in
`a1`, and return bytes written in `d0`. The stack decoder reserves a 32-byte history area and seeds it
with words 4 through 15 while keeping the four hottest values 0 through 3 in registers.

The maintained Python decoder now models the full bitstream grammar rather than only this calling
convention. Each command group expands four variable-length command nibbles into sixteen literal/copy
bits. A literal word takes four nibbles from a sixteen-value move-to-front history. A section copy uses
an eleven-bit backwards word offset; its length starts at two words, adds two per `00`, optionally adds
one for `01`, and a zero offset terminates the stream.

The first complete corpus is all 43 `data/battles/entries/battle*/terrain.bin` payloads. Their 16,466
compressed bytes deterministically decode to 99,072 bytes: one 48×48 grid for each unique payload. Every
decoded byte is one of terrain types 0 through 8 or the obstructed value `0xFF`. The rail validates all
45 pointer-table entries and every compressed payload against the original ROM; battles 4 and 32
retain their source aliases to payloads 3 and 27. Only decoded hashes, counts, and codec statistics are
tracked; the private grids stay under `local/derived/`.

The second complete corpus is the 30-slot battle-background table backed by 27 unique containers.
Each six-byte header contains three relative offsets: tileset 1 begins at byte 38, tileset 2 is
relative to the word at byte 2, and the 32-byte palette begins at byte 6. The loader decodes both
Stack streams into consecutive 6,144-byte VRAM staging ranges. All 54 streams reach that exact output
size, producing 331,776 decoded bytes from 163,742 compressed bytes. Slots 21 and 22 alias payload 12;
slot 29 aliases payload 13. The rail ROM-checks all 30 pointers and 27 payloads and tracks only palette
and decoded hashes, offsets, counts, and codec statistics. At load time the destination palette's
first word is cleared and the remaining fifteen words are copied from container palette bytes 2-31.

The third complete corpus is the 56-slot portrait table backed by 52 unique containers. The loader
reads a word count plus four-byte entries for eyes, repeats that structure for mouths, consumes a
32-byte palette, and passes the remaining stream to the Stack decoder. Every container decodes to
2,048 bytes, for 106,496 bytes total. The corpus contains 261 eye entries and 218 mouth entries, all
using coordinates 0-7; portrait 35 aliases payload 33 and slots 53-55 alias payload 52. Pointer and
payload bytes are ROM-checked, while tracked output retains only metadata/palette/decoded hashes and
aggregate codec facts.

## Display, Sprite, and Palette State

`InitializeDisplay` first deactivates contextual VInt functions, waits for VInt, disables display and
interrupts, clears sprites, configures H32/V32 non-interlaced planes and scroll tables, then loads a
black screen, sprite masks, and the base UI palette. `InitializeSprites` uses a `dbf` counter, writes
sequential sprite links, and clears the final link.

Palette transitions start with a 32-frame timer. The current timer divided by four selects blend
weights whose total is eight; every update queues CRAM DMA. At completion, a flag can promote the
backup palette into the new base.

## Special Sprites and View Routing

Special sprites have nine dispatch slots; slot 2 is the exploration-specific path and the remaining
slots use battle handling. Initial loads use immediate VRAM DMA, while animation refresh uses the VInt
DMA queue. Palette 4 is loaded before dispatch.

View destinations multiply each plane/axis by its own parallax factor. An enabled autoscroll axis
keeps its current position; otherwise the calculated position becomes the destination. The flash
screen script is the fixed word sequence `0x41, 0x1E, 0xFFFF`.

## Concentrated Verification Queue

This batch starts no emulator. The same decoder should next expand through the structured Stack
containers for battle sprites and special screens, while a separate Basic decoder owns map sprites.
Rendered behavior joins the shared presentation matrix: display
initialization, palette interpolation frames, parallax/autoscroll axes, special-sprite updates, and
flash duration can share VDP/RAM observation points.

## Reproduction

```powershell
uv run sf2 h2 tech-graphics
uv run sf2 h2 battle-terrain
uv run sf2 h2 battle-backgrounds
uv run sf2 h2 portraits
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/tech-graphics-static.json` and
`local/derived/battle-terrain-decode.json`, `battle-background-decode.json`, and
`portrait-graphics-decode.json`.
