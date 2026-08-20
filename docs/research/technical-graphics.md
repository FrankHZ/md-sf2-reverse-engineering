# Technical Graphics and Decompression Services

- Status: **Confirmed** for the pinned 11-file layout-owned inventory, H1 entry addresses, the two
  decompression entry contracts, display initialization order, sprite links, palette interpolation,
  special-sprite routing, view parallax gates, flash-script words, and the complete battle-terrain,
  battle-background, battle-sprite, weapon/ground, and portrait Stack-compression corpora, the
  complete ally/enemy battle-sprite animation sequence corpus, plus the
  complete regular map-sprite Basic-compression, special-sprite Stack-compression, and
  special-screen, base/menu UI, battle-effect, and map-tileset Stack-compression corpora, plus the
  complete icon-storage/copy/highlight corpus, map-palette/effective-color-zero boundaries, and
  assembled UI/window layout, spell-pointer, border, direct menu-tile, variable-width-font, and
  witch-menu palette/bubble-animation corpora, plus all uncompressed special-screen palettes/layouts
  and the four-stream unused-cloud/two-palette unused-base payloads
- Status: **Inferred** for visual intent where static state/register routing is clear but no rendered
  frame has been compared
- Status: **Unknown** for remaining embedded compression corpora outside the completed families,
  exact VDP timing, palette
  presentation, portrait/map-sprite animation timing, special-sprite frame output, and whether the
  three regular-map-sprite free-spot IDs, seven incompletely routed special IDs, or two nominally
  unused technical resources can be selected
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

The corresponding sequence corpus is separate from those graphical frames. Its 87 ally and 121
enemy pointers address 208 unique payloads totaling 3,800 bytes. Ally payloads have an eight-byte
header followed by 2-10 eight-byte entries; enemy payloads have a four-byte header followed by 1-4
four-byte entries. The parser validates both size formulas, both pointer tables, both top-level
pointers, every payload, and all 421 entries against source, H1, and ROM.

For allies, entry zero also supplies the optional second idle frame and weapon placement, so the
attack consumer skips it and plays 147 of 234 entries. Enemy attacks play all 187 entries. Across both
sides, frame index 15 means keep the previous battlesprite frame and appears 43 times; seven headers
embed a non-`0xFF` spell-animation index, while all 208 terminate-spell bytes are zero. Ally entries
also retain weapon frame/flip bits, layer 1/2, and signed offsets. These are format and consumer facts,
not rendered timing or placement parity.

Selectors use the combatant's base animation for ordinary attacks, add 40/60 for ally/enemy dodge,
and accept direct special indices from 80/118. Ally regular spear attacks additionally remap KNTE,
PLDN, and PGNT to 80-82. Static parsing does not yet prove the reachable base-index set for every
combatant/weapon combination.

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

The eighth corpus closes every Stack-compressed tile resource consumed by `code/specialscreens`.
Seven call the Stack decoder directly; the speech balloon and Sega logo use the compressed-DMA
wrapper. The nine streams occupy 23,296 compressed bytes and decode to 50,176 bytes. All nine source
ranges, six directly addressable source pointers, and their H1 symbols match the ROM. Title tiles,
title font, and Sega logo have exact fixed transfer sizes; the ending-kiss picture instead feeds its
6,144 decoded bytes to a pixel-fill consumer.

Five other fixed transfers are larger than decoder output: suspend string 448→2,048, ending witch
7,808→16,384, ending jewels 1,856→16,384, witch screen 13,568→16,384, and speech balloon
1,920→2,048 bytes. Their combined 27,648-byte transfer tail is not part of the compressed stream.
Static code proves the transfer boundary but not which staging bytes occupy the tail at runtime, so
tail contents and stability remain **Unknown** rather than being modeled as zero padding.

The ninth corpus closes shared base and menu UI graphics. `tiles_Base` is decoded once for startup
VRAM upload and again as the source for the doubled ending-credits font. Six diamond-menu resources
each decode to 2,304 bytes (two frames of four 288-byte icons), while the yes/no prompt decodes to
1,152 bytes (two frames of two icons). Together the eight streams expand 7,848 compressed bytes to
23,168 bytes. The adjacent uncompressed main-menu payload adds 4,032 bytes: seven fixed 576-byte/
18-tile icon records. Packed table entries reference icons 0-5 in combinations `[5,1,2,4]`,
`[0,1,2,3]`, and `[0,1,2,4]`; icon 6 has no table reference. All nine payloads, nine pointers, the
seven icon boundaries, and the nine-entry menu table match the ROM.

The menu table is heterogeneous by design. Its first three longwords have bit 31 set and pack four
indices into the uncompressed `tiles_MainMenu`; the other six point to pointer words for the Stack
resources. The consumer clears and branches on bit 31 before choosing `LoadMainMenuIcon` or the
Stack decoder, so the tracked contract preserves the two formats instead of treating the packed
values as malformed addresses. This closes the last section-6 technical incbin that previously had
only routing evidence; icon 6 runtime reach and final presentation remain Unknown.

The tenth corpus closes battle-scene effects. Twenty-three spell containers each carry a decoded
byte-count word, three palette colors, and one Stack stream; every decoded size matches its header.
Four invocation containers contribute 15 logical frames and two streams per frame. Status animation
adds one stream and battle-scene transitions add two, producing 56 streams total. Their 46,364
compressed bytes decode to 200,992 bytes, with all 30 resource containers, four top-level pointers,
and three pointer tables matching source, H1, and ROM.

Every invocation stream decodes to 4,096 bytes, while both consumer paths transfer 4,608 bytes. The
30 transfers therefore expose a consistent 512-byte tail each, or 15,360 bytes in aggregate. As with
the five special-screen over-transfers, this is a confirmed transfer boundary but not evidence that
the tail is zeroed, stable, or invisible.

The eleventh corpus closes the full 115-entry map-tileset table. Every Stack payload decodes to
4,096 bytes, totaling 471,040 decoded bytes from 198,514 compressed bytes. The rail checks all 115
table longwords, all payload ranges, the top-level pointer, all 79 six-byte palette/tileset map
headers, and all 32 animation headers against H1 and ROM.

The 79 maps provide 395 ordinary tileset slots: 326 real references and 69 `255` sentinels, covering
100 unique indices. Animation headers add 32 references across 15 unique indices. Combined static
usage reaches 114 of 115 resources; only `MapTileset029` has no reference in either complete source
surface. This proves static absence, not runtime unreachability through dynamic or encoded writes.

The twelfth graphics corpus closes `pt_MapPalettes`. Its sixteen pointers select sixteen 32-byte
payloads, each holding sixteen big-endian Genesis color words. All 512 payload bytes, the 64-byte
pointer table, the top-level pointer, and the palette byte in every one of the 79 map headers match
source, H1, and ROM. All 256 color words obey mask `0x0EEE`; the source corpus contains 69 unique
values. Every palette index 0-15 is referenced by at least one map (index 0 appears 47 times; all
others appear one to six times).

`LoadMap` copies all 32 source bytes into `PALETTE_1_BASE` and then clears its first word. Fifteen of
the sixteen source palettes have a nonzero first word, but all sixteen effective palettes therefore
use zero for color 0. This is a consumer-visible transformation, so canonical remake data must retain
the source palette and the effective color-zero rule separately. Static parity does not yet prove the
rendered fade, transition, or final per-map presentation.

The thirteenth graphics corpus closes the uncompressed icon block. The source tree contains 167
192-byte payloads, but `entries.asm` assembles exactly 163 into one contiguous 31,296-byte range:
127 `ItemIcon` entries, 30 `SpellIcon` entries, and six `OtherIcon` entries. All 163 payloads and the
`p_Icons` base pointer match ROM. The source-only exceptions are `item/icon127.bin` and
`spell/icon016.bin` through `icon018.bin`; they are not silently credited as original-build assets.

Storage is arithmetic rather than a pointer table: index multiplied by 192 is added to `p_Icons`.
Slots 127 and 128 are `ICON_NOTHING` and `ICON_UNARMED`; slot 129 has no enum name or symbolic code
reference. Slots 146-148 are Jewel of Light, Jewel of Evil, and the cracks overlay. Because spell
icons start at 130, those same physical slots also equal spell indices 16-18, whose available payloads
are not assembled. Static shape proves the collision, not whether generic spell-icon callers can
receive those three IDs.

`LoadIcon` and the shop loader copy 48 longwords (192 bytes) and force four corner nibbles to color
15. `LoadHighlightableIcon` instead emits the 192-byte source followed by a second 192-byte frame made
with bitwise `source AND tiles_IconHighlight`; the mask itself is ROM-checked. Rendered palette,
highlight timing, and DMA ordering remain presentation questions rather than inferred pixel parity.

The fourteenth graphics corpus closes the uncompressed UI layout surface used by the vanilla build.
Nineteen ASM owners expand to 5,116 source bytes, including 27 leaf tilemaps with 2,394 Genesis VDP
words, a 16-entry spell-level pointer table with ten unique targets, four 48-byte diamond-border
variants, and the 72-byte alphabet highlight. Three adjacent direct `incbin` resources add 498 bytes
for price tags and shop highlighting, bringing the unique tracked layout/asset boundary to 5,614
bytes. Every local label, pointer, macro-expanded word, byte directive, and payload matches H1 and ROM.

The tilemaps span palettes 1-4 and retain priority, mirror, and flip attributes; the parser records
shapes and hashes rather than redistributing raw layouts. The unused window-border aggregate and
fighter mini-status alternate are absent from the original section layouts and stay excluded rather
than inheriting canonical addresses. Runtime tile overwrites, DMA, palette, window motion, and final
composition remain presentation questions.

The fifteenth graphics corpus closes the 2,560-byte variable-width dialogue font. It contains 80
fixed 32-byte records with one width header and fifteen 12-pixel rows. One glyph is blank; the corpus
has 1,633 set pixels, zero padding violations, widths 3-9, and five glyphs whose set pixels extend
beyond their computed advance. The 4-byte pointer and 256-byte ASCII conversion table bring the
source/H1/ROM parity boundary to 2,820 bytes.

The ASCII path reaches 78 glyph IDs and maps 145 byte values to glyph 1. IDs 70 and 71 are absent,
but Huffman-decoded symbols bypass this table, so static ASCII absence is not a reachability claim.
Glyph overlap, palette selection, one-versus-two-load rendering, typewriter timing, DMA, and final
frames remain one shared text-presentation matrix.

The sixteenth graphics corpus closes the two direct witch-menu resources beside that font. The
32-byte choice palette is copied as one 16-color CRAM palette. The 960-byte bubble table divides into
four options × three distinct frames × forty words, and the two longword pointers make 1,000 checked
source bytes. After the consumer's `-$5D00` adjustment, all 480 words are priority palette-2 tiles;
240 mirror, 240 flip, and 60 unique tile indices span 1024-1083.

The selected timer maps states 1-20 to frame phases 0→1→2→1, while unselected options remain on frame
zero. Static source proves this phase table and the four source/destination offsets, not exact CRAM,
redraw, window-motion, or visible timing; those remain in the shared witch presentation matrix.

The seventeenth graphics corpus closes the uncompressed presentation payloads across title, witch,
ending witch, ending jewels, ending kiss, and suspend screens. Seven palettes contribute 480 bytes/
240 color words; five layouts contribute 8,352 bytes/4,176 words. All twelve resource addresses and
8,832 bytes match H1 and ROM. Title A/B additionally prove their `vdpTile` ASM expansion equals the
two upstream binary mirrors. Runtime palette order, fades, layout changes, scroll, and composition
remain grouped presentation questions.

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

## Nominally Unused Technical Assets

The two named but nominally unused technical resources now have their own byte-level boundary. The
5,694-byte `tiles_UnusedCloud` container has exactly four even-offset candidates that each decode as
one 8,192-byte Stack stream: 32,768 decoded bytes, or 4 × 256 32-byte tiles. All four decoded hashes
are distinct, and their stored spans retain 158 bits beyond the logical terminators. The 64-byte
`palette_UnusedBase` payload is two valid 16-color Mega Drive palettes that differ only at color
indices 1 and 5; its longword pointer also matches ROM.

After comments are removed, the cloud symbol occurs only at its definition, the palette symbol only
at its definition and pointer, and the pointer only at its definition. This confirms there is no
symbolic ASM consumer, not that a raw address, computed pointer, or debug-only path is impossible.
The names/comments likewise do not independently confirm clouds, animation order, or rendered use.

## Direct Consumer Denominator

The complete pinned `disasm/code` tree contains 46 direct named compression calls in 23 files: 35
to `LoadStackCompressedData`, four to `LoadBasicCompressedData`, and seven to
`ApplyImmediateVramDmaOnCompressedTiles`. A deterministic source rail assigns every call to one of
twelve completed corpus or wrapper owners; the unowned count is zero. This is a direct named-call
denominator, not proof that dynamic indirect calls or self-modifying targets cannot exist.

## Renderer Mapping (private extraction tooling)

Status: **Confirmed** only for the source/ROM-bound storage shapes and transformations already
owned by the named H2 rails, plus the project-owned synthetic renderer tests. The RGB channel
assignment, composed appearance, and original-game screenshot equivalence remain **Inferred** or
**Unknown** as identified below: the `sf2 texture` rail emits PNGs but does not execute an emulator,
capture a frame, or compare a color histogram. No prior informal screenshot inspection is promoted
to reproducible evidence. These mappings guide private extraction tooling; they are not new
data-contract claims.

- Map 4bpp tiles are 32-byte 8x8 tiles with two pixels per byte, left pixel in the high nibble.
  Decoded tileset streams (4,096 bytes) hold 128 tiles each.
- Map palettes (`mappaletteNN.bin`, and `basepalette.bin` "Palette for UI and mapsprites") use the
  accepted `0x0EEE` word mask. The tool maps the low bit group (bits 1-3) to red, the middle group
  (bits 5-7) to green, and the high group (bits 9-11) to blue; final on-screen channel/color parity
  is **Inferred** pending a reproducible original-game observation. `mapload.asm` clears effective
  palette index 0 (`clr.w (PALETTE_1_BASE).l`). The renderer therefore makes palette *index* 0
  transparent while preserving an RGB-black value at every nonzero index as opaque.
- Battle-background palette words are confirmed to stay inside the `0x0EEE` mask. The tool applies
  the same candidate channel mapping as map palettes; final composed color parity is **Unknown**.
- Special-sprite palettes are each a 32-byte header copied verbatim to `PALETTE_4_BASE` by
  `LoadSpecialSprite` (`specialsprites.asm`); the words do not all fit the `0x0EEE` mask (e.g.
  `0x558` in `taros.bin`), so the tooling decodes them with a candidate 6/5/5 channel layout
  (6-bit blue bits 0-5, 5-bit green bits 6-10, 5-bit red bits 11-15). This decode is
  **Unknown/unconfirmed** and not consumed by this phase.
- Map blocks are 3x3 tiles (24x24 pixels); the tile word stores the VRAM tile number plus `0x100`
  with flags `0x8000` (priority), `0x1000` (vertical flip), `0x800` (horizontal mirror). The five
  map tileset slots map via `tileIndex // 128` (VRAM bases `$2000`, `$3000`, `$4000`, `$5000`,
  `$6000` per `mapload.asm`).
- Map layout is a 64x64 block grid; area bounds come from `2-areas.asm` (`mainLayerStart/End`,
  `scndLayerFgndStart`, `scndLayerBgndStart`). Only the main layer is rendered; the
  second/background layer's exploration-mode palette source remains **Unknown**.
- The second layer is not a separate area record or layout stream: its content lives **in the
  same 64x64 layout** at the plane-view offset given by `scndLayerFgndStart` minus
  `scndLayerBgndStart`. `SetViewDestination` (`display.asm`) scrolls Plane A at the camera plus
  the foreground start and Plane B at the camera plus the background start (parallax 256 = 1x);
  Plane A renders on top, so layout block `(x + fg - bg, y + fg - bg)` appears over main-layer
  block `(x, y)`. Map 3 area 0 has fg `(0,32)`/bg `(0,0)`, so its overlay content (roofs and the
  cell's iron bars) occupies layout rows 32..63 and is displayed over main rows 0..31. Layout
  words carry a block-section flag (`00`/`01`/`100`/`101`/`11` prefixes in
  `ReadMapLayoutBarrelForBlockFlags`, `mapload.asm`) whose   `0xC000` bit selects the plane
  palette in `UpdateVdpPlane`; static rendering draws both sections with the map palette. Roof/layer-2
  `slbc` records (`map-content.md`: 79 tables, 114 records) copy/clear blocks into this overlay
  region at runtime. Some overlay content is stored **outside the areas** in the record's
  source rectangle (map 3: the creature-building facade with its iron bars at `(51,20)`,
  9x7 blocks) and is copied to the display destination `(22,51)` by
  `PerformMapBlockCopyScript` (`$0800` show = snapshot-then-copy source to destination,
  `$0C00` hide = restore the saved rectangle; `map-block-copy-lifecycle` H3 fixture); the
  static extraction applies the copy records so the bars appear over the middle-lower room.
  **Confirmed** (static; overlay palette source still **Unknown**).
- Map sprites decode to `0x240` bytes = two 3x3-tile frames (24x24 pixels each) stored as
  contiguous 32-byte tiles; the 3x3 frame grid is column-major (tile 0 top-left, 1 middle-left,
  2 bottom-left). The three streams per sprite index are the three facing directions.
- Portraits decode to 2,048 bytes = 64 tiles on an 8x8 grid (64x64 pixels) with a per-portrait
  32-byte palette; the leading eye/mouth animation-entry tables are metadata only.
- Icons are 192 bytes = six tiles in a 2x3 grid (16x24 pixels); map sprites and icons share the
  `basepalette.bin` palette.
- The variable-width font stores 80 glyphs of 32 bytes (width header, then 15 rows of 12 usable
  pixels in `(left << 4) | (right >> 4)`, bit 11 = column 0).
- Battle backgrounds are two 6,144-byte Stack tilesets plus a 32-byte palette. The diagnostic
  composed sheet applies the 384-entry `layout_BattlesceneBackground` table (VRAM tile numbers
  928-1311, 32 columns); original rendered-screen equivalence remains **Unknown**.
- Special sprites decode (Stack) to 72 tiles for the battle class and 162 for
  `SpecialSprite_NazcaShip`; `SpecialSprite_EvilSpiritAlt` is animation-only. The tooling emits
  the raw tile-pool sheet plus candidate composed layouts (3x3, 3x6, 4x4, 6x6) for later
  inspection, but the exact frame assembly (frame size, tile order, linked-sprite arrangement)
  is **Unknown/unconfirmed**: no candidate is accepted as an original-game composition, and this
  phase does not need special-sprite assembly, so it is deferred. The animation-only stream has
  no owned palette; its all-zero diagnostic candidate renders palette index 0 transparent and
  nonzero indices opaque black rather than treating every RGB-black entry as transparent.

All binary inputs consumed by the texture commands are ignored upstream split payloads. Before
rendering, the tool resolves each payload's owning H1 symbol and requires the complete bytes to equal
the exact range in the hash-verified USA ROM; accepted unused-cloud stream hashes add a second
owner-fixture check. A clean upstream Git checkout alone is not treated as payload provenance.
Texture commands fail closed when their owned output directories or manifests already exist,
preventing a fresh manifest from silently inheriting stale products. The unused-assets output
identifies all four accepted Stack streams explicitly, renders each under both base palettes, and
counts the two palette strips separately from stream coverage.

## Concentrated Verification Queue

This batch starts no emulator. All current direct named decoder consumers now have corpus owners.
ADR 0005's 2026-07-23 priority decision freezes decompressor/copy-loop micro-implementation and
VDP/DMA cycle-accuracy work once those import contracts are adequate; an indirect table alone is not a
reason to pursue decoder internals. Reopen one bounded question only for an import-format or
asset/provenance gap, conflicting evidence, an explicit hardware-fidelity target, or a concrete
user-visible acceptance failure.

Rendered behavior remains eligible when a map, UI, menu, or other visible acceptance contract needs
it. Display initialization, palette interpolation, parallax/autoscroll, regular/special-sprite
updates, special-screen transfer tails, and flash duration can share existing VDP/RAM observation
points, but do not expand that work into cycle counting by default. Static reachability is complete for
reserved IDs 237-250: symbolic references, all 980 initial map-entity records, 81 later script
assignments, ally/enemy derivation, five actual writers, and all 20 property-update callers exclude
them from the original built domains. Only deliberately malformed script/RAM injection and a concrete
visible acceptance gap remain optional runtime research.

## Reproduction

```powershell
uv run sf2 h2 tech-graphics
uv run sf2 h2 battle-terrain
uv run sf2 h2 battle-backgrounds
uv run sf2 h2 battle-sprites
uv run sf2 h2 battle-sprite-animations
uv run sf2 h2 battle-weapon-ground
uv run sf2 h2 portraits
uv run sf2 h2 map-sprites
uv run sf2 h2 special-sprites
uv run sf2 h2 special-screen-graphics
uv run sf2 h2 special-screen-presentation
uv run sf2 h2 ui-graphics
uv run sf2 h2 icon-graphics
uv run sf2 h2 ui-layouts
uv run sf2 h2 variable-width-font
uv run sf2 h2 unused-tech-assets
uv run sf2 h2 witch-menu-graphics
uv run sf2 h2 battle-effect-graphics
uv run sf2 h2 map-tilesets
uv run sf2 h2 map-palettes
uv run sf2 h2 compression-consumers
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/tech-graphics-static.json` and
`local/derived/battle-terrain-decode.json`, `battle-background-decode.json`, and
`battle-sprite-decode.json`, `battle-sprite-animation-static.json`,
`battle-weapon-ground-decode.json`, plus
`portrait-graphics-decode.json`, `map-sprite-decode.json`, `special-sprite-decode.json`, and
`special-screen-graphics-decode.json`, `special-screen-presentation-static.json`,
`ui-graphics-decode.json`, `icon-graphics-static.json`, and
`ui-layout-static.json`, `variable-width-font-static.json`, `witch-menu-graphics-static.json`,
`unused-technical-assets-static.json`,
`battle-effect-graphics-decode.json`, plus `map-tileset-decode.json` and
`map-palette-static.json`.
The consumer map stays under ignored `local/derived/compression-consumers-static.json`.
