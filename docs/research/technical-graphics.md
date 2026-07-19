# Technical Graphics and Decompression Services

- Status: **Confirmed** for the pinned 11-file layout-owned inventory, H1 entry addresses, the two
  decompression entry contracts, display initialization order, sprite links, palette interpolation,
  special-sprite routing, view parallax gates, and flash-script words
- Status: **Inferred** for visual intent where static state/register routing is clear but no rendered
  frame has been compared
- Status: **Unknown** for decompressor corpus parity, exact VDP timing, palette presentation, and
  special-sprite frame output
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
with words 4 through 15. This proves the calling convention and core history shape, but not yet that a
project-owned decoder reproduces every upstream compressed asset. That belongs in a later corpus-wide
source/output parity rail rather than eleven separate emulator cases.

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

This batch starts no emulator. Decompression should next be validated statically against a broad,
hash-only corpus of existing compressed inputs and outputs. Rendered behavior joins the shared
presentation matrix: display initialization, palette interpolation frames, parallax/autoscroll axes,
special-sprite updates, and flash duration can share VDP/RAM observation points.

## Reproduction

```powershell
uv run sf2 h2 tech-graphics
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/tech-graphics-static.json`.
