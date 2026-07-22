# Window-System Contract

- **Confirmed original behavior:** the fixed-slot layout allocation, packed-coordinate addressing,
  source-level movement state, map-layout composition, and VInt/DMA call order described here.
- **Inferred original behavior:** caller-specific admission, return, and user-facing meaning.
- **Unknown original behavior:** rendered frames, clipping and scrolling perception, queue completion,
  capacity, and hardware timing.
- Evidence date: 2026-07-21
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- Traceability: `sf2-remaining-core-static-v1` in
  `tests/fixtures/h2/remaining-core-static-v1.json`; `sf2-ui-layout-static-v1` in
  `tests/fixtures/h2/ui-layout-static-v1.json`; `sf2-tech-interrupts-static-v1` in
  `tests/fixtures/h2/tech-interrupts-static-v1.json`; `src/sf2tool/h2/remaining_core.py`; and
  `docs/research/remaining-core.md`.

## Confirmed Static Contract

The original has eight fixed 16-byte entries. An entry holds one layout address/active-test longword,
two packed-size bytes, six packed position/origin/destination bytes, two animation-control bytes, and
two source-unlabeled bytes. A layout allocation consumes `width * height * 2` bytes; that is a layout
word formula, not a renderer-buffer or capacity guarantee. The allocation cursor advances on successful
creation and is recomputed from the highest remaining layout address after deletion.

Coordinates are packed with X in the high byte and Y in the low byte. Entry lookup uses a slot stride
of 16; tile lookup uses `layout + (Y * width + X) * 2`. Composition uses a 32-tile, 64-byte map-layout
row stride and source checks protect its `<32` X and `<28` Y boundary. A remake should keep coordinate
packing and layout-word addressing as an import/fidelity boundary, while choosing its own internal data
representation where parity is not required.

Movement keeps packed origin, destination, length, and counter state. Normal movement resets the
counter after writing origin/destination/length; Special Turbo forces length one. Each VInt restores
moving windows before interpolating and recomposing them, tracks moving slots in a bitfield, applies
the hide/fix paths, and conditionally submits the combined Plane-A layout before enabling DMA queue
processing. The exact visible result, completion frame, and queue behavior are intentionally outside
this contract.

## Remake Boundary

A remake can expose allocation, movement, composition, and deferred presentation as separate services.
It should retain the confirmed ordering where original fidelity is wanted, preserve the source-unlabeled
bytes as opaque compatibility state until runtime work explains them, and make clipping, scroll camera,
buffer capacity, DMA emulation, audio feedback, and presentation timing explicit implementation choices.
