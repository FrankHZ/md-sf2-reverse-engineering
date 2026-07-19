# Technical Graphics and Decompression Services

- Status: **Confirmed** for the pinned 11-file layout-owned inventory, H1 entry addresses, the two
  decompression entry contracts, display initialization order, sprite links, palette interpolation,
  special-sprite routing, view parallax gates, flash-script words, and the complete battle-terrain,
  battle-background, battle-sprite, weapon/ground, and portrait Stack-compression corpora, plus the
  complete regular map-sprite Basic-compression and special-sprite Stack-compression corpora
- Status: **Inferred** for visual intent where static state/register routing is clear but no rendered
  frame has been compared
- Status: **Unknown** for remaining embedded compression corpora, exact VDP timing, palette
  presentation, portrait/map-sprite animation timing, special-sprite frame output, and whether the
  three regular-map-sprite free-spot IDs or seven incompletely routed special IDs can be selected
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

The Basic format is word-oriented. A 16-bit command bitmap is consumed most-significant bit first:
zero emits the next literal word, while one consumes a copy word. Copy word zero terminates the
stream; otherwise its low five bits encode length as `33 - low5`, and its upper eleven bits encode a
backwards byte offset as `(command & 0xFFE0) >> 4`. Copying may overlap the growing output, so offset
one word is a repeat-last-word operation. The Python decoder rejects odd inputs, invalid backwards
references, missing terminators, unexpected trailing bytes, and output-size drift.

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

The fourth complete corpus joins all 32 ally and 54 enemy battle-sprite containers. A container
starts with animation speed and two status-icon offsets, followed by a relative palette boundary,
one self-relative word per frame, one to four 32-byte palettes, and the Stack streams. Ally
containers provide 153 frames across three to six frames each; enemy containers provide 255 across
two to seven. Every ally frame decodes to 4,608 bytes (12×12 tiles) and every enemy frame to 6,144
bytes (16×12 tiles), totaling 2,271,744 decoded bytes. The rail validates both pointer tables, all 86
payloads, all 167 palettes, and all 408 streams against ROM boundaries without tracking image bytes.

The fifth corpus closes the remaining battle-scene weapon and ground layers. All 23 weapon streams
decode to 8,192 bytes (four 64-tile views) and use 42 contiguous four-byte palette entries. The
30-slot ground table has 27 six-byte palette headers and three aliases matching backgrounds
(21/22→12, 29→13); its relative words select ten shared streams, each decoding to 1,536 bytes (48
tiles). The rail ROM-checks 53 pointers and 102 source payload/header objects and confirms 203,776
decoded bytes without treating tile layout comments as rendered evidence.

The sixth corpus closes the regular map-sprite table. Its 720 pointer slots cover 240 logical IDs
and three directional payloads per ID; 670 source payloads plus 50 pointer aliases satisfy exact
source/H1/ROM parity. Of those payloads, 669 Basic streams consume 225,542 compressed bytes and each
decode to exactly 576 bytes (`0x240`), for 385,344 decoded bytes total. The decoder observes 6,946
command words, 87,031 literal words, and 18,125 copy commands producing 105,641 copied words; maximum
copy distance is 273 words and maximum length is 33 words.

`ChangeEntityMapsprite` and `DmaEntityMapsprite` select one of the three pointers from map-sprite ID
and facing, decode into `FF8002_LOADING_SPACE`, and transfer `0x120` words to the entity's VRAM slot.
IDs 240-255 branch to the special-sprite loader. The remaining source payload `Mapsprite237_0` is only
the word `0xFFFF` and is shared by all nine pointer slots for enum values `MAPSPRITE_FREE_SPOT1` through
`MAPSPRITE_FREE_SPOT3` (IDs 237-239). Because those values are below
`MAPSPRITES_SPECIALS_START = 240`, static consumer shape does not itself prove that the regular Basic
decoder can never receive them. That reachability question remains **Unknown** rather than treating
the sentinel as a valid compressed stream.

The seventh corpus closes `data/graphics/specialsprites`. Ten pointer slots resolve to five initial
containers with five 32-byte palettes; `SpecialSprite_EvilSpiritAlt` contributes a sixth,
animation-only Stack stream. Five streams decode to 2,304 bytes and the Nazca Ship exploration stream
to 5,184 bytes, totaling 16,704 decoded bytes from 6,582 compressed bytes. All ten pointers and all
six contiguous payloads match the ROM; tracked output keeps hashes/statistics rather than palettes or
tile bytes. `AnimateSpecialSprite` reuses the Evil Spirit and Zeon streams after their 32-byte
palettes and selects the separate Evil Spirit alternate stream for its middle mode.

The routing boundary is intentionally asymmetric. `LoadSpecialSprite` converts map-sprite ID to
`255 - ID`; the pointer table has ten indices (0-9), while both load and update dispatch tables have
only nine (0-8). Therefore IDs 247-255 are fully routed, ID 246 has a Kraken pointer but no dispatch
entry, and IDs 240-245 have neither. A complete symbolic scan of the pinned ASM finds actual source
references only for IDs 251-255; IDs 240-250 and regular free-spot IDs 237-239 have none outside their
enum definitions. This is strong source evidence, but dynamic byte writes or encoded script values
remain capable of defeating a name-only scan, so runtime unreachability remains **Unknown**.

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

This batch starts no emulator. The Stack decoder should next expand through remaining embedded
containers such as special screens. Rendered behavior joins the shared presentation matrix: display
initialization, palette interpolation frames, parallax/autoscroll axes, regular/special-sprite
updates, and flash duration can share VDP/RAM observation points. Static symbolic search is now
complete for reserved IDs 237-250; the next reachability step must inspect encoded records and
runtime writes rather than repeating text search.

## Reproduction

```powershell
uv run sf2 h2 tech-graphics
uv run sf2 h2 battle-terrain
uv run sf2 h2 battle-backgrounds
uv run sf2 h2 battle-sprites
uv run sf2 h2 battle-weapon-ground
uv run sf2 h2 portraits
uv run sf2 h2 map-sprites
uv run sf2 h2 special-sprites
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/tech-graphics-static.json` and
`local/derived/battle-terrain-decode.json`, `battle-background-decode.json`, and
`battle-sprite-decode.json`, `battle-weapon-ground-decode.json`, plus
`portrait-graphics-decode.json`, `map-sprite-decode.json`, and `special-sprite-decode.json`.
