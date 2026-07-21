# Remaining Technical Services

- Status: **Confirmed** for the complete 12-file source boundary, eleven main-ROM representative
  addresses, twenty technical-resource incbin mappings, byte-copy direction, input scan/wait shape,
  SRAM slot/checksum structure, the complete variable-width-font, context-Huffman, and witch-menu
  direct payload/pointer boundaries, the four-stream unused-cloud and two-palette unused-base
  boundaries, the 68000 music-wait command, and the Z80 driver build chain
- Status: **Inferred** for caller-visible thinking-RNG distribution and perceived delay
- Status: **Unknown** for controller hardware edge cases, SRAM persistence/corruption behavior, and
  rendered/audio timing
- Evidence date: 2026-07-20
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Complete Boundary

This batch closes the twelve `code/common/tech` files not owned by the interrupt, graphics,
jump-interface, or pointer inventories. They contain 6,986 source lines and 550 global labels:

- five layout-owned resource-incbin files;
- `bytecopy.asm`, `input.asm`, `randomnumbergenerator.asm`, and `thinkingairng.asm`;
- the 68000 `sound/music.asm` bridge and standalone Z80 `sound/sounddriver.asm`;
- `sram/sramfunctions.asm`.

Eleven files are included directly by the main 68000 layout. The exception is `sounddriver.asm`: the
pinned build script assembles it separately with ASW into an 8,192-byte `$0000..$1FFF` binary, then
the main layout includes that binary at `SoundDriver` (`0x1EC000`). Its SHA-256 is recorded in the
canonical inventory. The source is an IDA-era single-byte export with C1 bytes in comments, so the
parser uses strict UTF-8 first and a byte-preserving Latin-1 fallback; source hashes always use the
unaltered bytes.

The research index binds the eleven main-layout files. The standalone Z80 source is intentionally
kept as an H2 auxiliary-build fact rather than pretending its `init` label exists in the 68000 H1
listing.

## Static Contracts

The five incbin files contain exactly twenty named entries across sections 3, 6, and 17. Their
canonical mapping covers UI/font/title resources, Huffman text trees, witch-screen resources, and
base tiles. A deterministic ownership audit now requires all twenty entries to appear in one of
eight deeper H2 fixtures and verifies each fixture/verifier/command exists; the unowned count is
zero. Extracted copyrighted resource bytes remain local, and rendered meaning stays with the
corresponding presentation subsystem.

That audit exposed and then closed the sole remaining shallow entry, uncompressed
`tiles_MainMenu`: 4,032 bytes divide into seven 576-byte/18-tile icons, its pointer and payload match
ROM, and the packed menu table selects icon IDs 0-5 while leaving ID 6 without a static table
reference. The other nineteen entries were already owned by the Huffman, font, screen, witch, UI,
icon, or nominally-unused resource contracts.

The text-Huffman rail closes the tree entries beyond routing. It checks the 510-byte offset table and
1,952-byte tree payload against H1 and ROM, reconstructs all 86 defined trees and 1,536 symbol/code
leaves, proves that their packed spans cover the payload exactly, and proves the context graph is
closed and wholly reachable from initial symbol 254. The adjacent note's 256-entry statement is kept
as a documented mismatch: the actual table has 255 entries, with 169 `$FFFF` sentinels.

The two remaining nominally unused resource entries are also closed beyond routing. The 5,694-byte
cloud-named container has four unique Stack streams, each decoding to 8,192 bytes/256 tiles. The
64-byte base payload is two valid 16-color palettes differing at two indices, and its four-byte
pointer matches H1 and ROM. Static token scanning finds no symbolic consumer for either resource or
the palette pointer; raw/computed/debug reach and rendered meaning remain explicitly unknown.

The variable-width-font rail now closes the font entry beyond routing: its 2,560-byte payload, the
four-byte pointer, 256-byte ASCII conversion table, 80 record boundaries, width/padding shape, and
three text consumer entry points all match H1 and ROM. Per-glyph output retains counts and hashes,
not original pixels; presentation semantics remain owned by the graphics/text documents.

The same section-6 owner also supplies the witch menu's 32-byte choice palette and 960-byte bubble
table. Their two pointers, resource adjacency, consumer addresses, and 1,000 total bytes match H1 and
ROM. The owning special-screen/graphics documents retain the palette, frame-grid, and timer semantics.

`CopyBytes` compares destination and source. It copies backward when the destination address is
higher and forward otherwise, preserving overlapping moves in the normal memmove cases.

`input.asm` is a 158-line, six-entry H1 surface: `UpdatePlayerInputs` (`0x150E`),
`WaitForPlayerInput` (`0x1576`), `WaitForPlayer1NewInput` (`0x1586`), `sub_15A4` (`0x15A4`),
`WaitForInputFor1Second` (`0x15D8`), and `WaitForInputFor3Seconds` (`0x15F4`). **Confirmed:**
`UpdatePlayerInputs` samples `DATA1` then `DATA2` at a two-byte stride, produces two inverted composed
state bytes per port, and stores contiguous Player 1 then Player 2 raw state. Each composition writes
TH low then high, shifts/masks `$C0`, combines a `$3F` read, and inverts before storing. The VInt repeat
stage is owned by the technical-interrupt rail: it transforms raw input into current/last input with a
24-frame initial delay and six-frame cadence. `WaitForPlayerInput` uses current input; the Player 1
new-input helper requires release then press; one/three-second raw Player 1 waits have 60/180 VInt
upper bounds and early exit. `sub_15A4` has a scratch-mask overlap counter with threshold 10. The
comment-stripping parser finds four source-local `WaitForVInt` call sites and eleven external sites
across nine callers: one `UpdatePlayerInputs` site and ten `WaitForPlayerInput` sites; the remaining
four entry points have zero static direct-call sites, which does not establish runtime reachability.
**Unknown:** hardware latency, controller-model
and three-/six-button behavior, and player-visible repeat timing. The implementation-neutral contract
is [`input-system.md`](../design/input-system.md).

## SRAM Save-System Contract

`code/common/tech/sram/sramfunctions.asm` is a 229-line, seven-entry service surface: `CheckSram`
(`0x6EA6`), `SaveGame` (`0x6F6A`), `LoadGame` (`0x6FAC`), `CopySave` (`0x6FDA`),
`ClearSaveSlotFlag` (`0x6FEC`), `CopyBytesToSram` (`0x7004`), and `CopyBytesFromSram` (`0x701C`).
The maintained H2 model parses all corresponding H1 entries plus the SRAM constants from
`sf2const.asm`/`sf2enums.asm`; named-function source guards prove each promoted semantic rather than
relying on the file's presence alone.

**Confirmed:** SRAM has two logical slots. Selector zero chooses slot 1; a nonzero selector chooses
slot 2. Each writes 4,016 logical bytes as 4,016 physical storage bytes; advancing the SRAM address
by two for every copied byte reserves an 8,032-byte address interval per slot, rather than writing
8,032 storage bytes. `CheckSram` checks its signature, slot 2, then slot 1. The two occupied bits
are 0 and 1 in `SAVE_FLAGS`. An occupied slot is
copied through the checksum reader and compared with its selected checksum byte: valid returns 1,
empty returns 0, invalid returns -1 and clears that occupied bit. A failed signature performs the
8,192-logical-byte clear, writes the signature, then clears save flags.

**Confirmed:** both copy helpers clear the word accumulator and perform byte addition, so their
observable checksum is the eight-bit accumulator low byte. Writing stores each source byte then
adds it before advancing the SRAM destination by two; reading copies the interleaved source byte,
adds it, then advances that source by two. `SaveGame` copies combatant data, writes the checksum
byte, then sets its occupied bit. `LoadGame` copies the selected slot to combatant data without a
local checksum comparison. `CopySave` loads then saves to the opposite selector, while
`ClearSaveSlotFlag` only clears the selected occupied bit.

The direct-call parser (comments excluded) finds seven external call sites in three source files:
church `SaveGame`, battle suspend `SaveGame`, and witch `CheckSram`/`SaveGame`/`LoadGame`/
`CopySave`/`ClearSaveSlotFlag`, one site each. **Inferred:** these callers intend their respective
save UI/lifecycle operations; their full caller-state outcomes are not promoted solely from the
direct-call inventory. **Unknown:** physical-media persistence, power loss between data/checksum/flag
writes, corruption beyond this checksum, emulator storage behavior, and the resulting player-visible
timing. The remake-facing extraction is
[`save-system.md`](../design/save-system.md).

`PlayMusicAfterCurrentOne` sends the wait-for-current-music command and parameter command, then polls
the mailbox with three-frame sleeps. The Z80 source statically exposes YM2612, PSG, DAC, bank, music,
and SFX machinery, but audible timing and channel output remain outside this static contract.

The thinking-AI RNG advances the low-byte state with multiplier 541 and increment 12345. The base
68000 RNG file was already covered by dedicated H3 fixtures. For the thinking variant, caller-visible
distribution and variable delay remain inferred until tested as a matrix.

## Concentrated Runtime Queue

No emulator was launched for this batch. Five questions are retained for later grouped runs:

1. one controller/input matrix: raw state A/B to last/current, new press and release/repress, held
   24/6 repeat boundaries, 60/180 early exit/timeout, and controller-model/latency edges;
2. SRAM signature initialization/full clear, valid/invalid checksum and occupied flags, save/copy/
   delete/reload persistence ordering, and partial-write/power-loss boundaries in one corruption
   matrix;
3. 68000-to-Z80 mailbox, channel routing, and audio timing;
4. thinking-RNG caller distribution and delay boundaries.
5. raw/computed reach plus frame/palette/VDP behavior for the nominally unused graphics resources.

The controller/input cases should share VInt and controller setup, SRAM cases should share one
initialized save-state matrix, and audio cases should share one sound-driver instrumentation launch.
Isolated one-case fixtures are not warranted by the current evidence.

## Reproduction

```powershell
uv run sf2 h2 tech-services
uv run sf2 h2 variable-width-font
uv run sf2 h2 text-huffman
uv run sf2 h2 unused-tech-assets
uv run sf2 h2 witch-menu-graphics
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/tech-services-static.json` and
`local/derived/unused-technical-assets-static.json`.
