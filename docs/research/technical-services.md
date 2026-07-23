# Remaining Technical Services

- Status: **Confirmed** for the complete 12-file source boundary, eleven main-ROM representative
  addresses, twenty technical-resource incbin mappings, byte-copy direction, input scan/wait shape,
  SRAM slot/checksum structure, the complete variable-width-font, context-Huffman, and witch-menu
  direct payload/pointer boundaries, the four-stream unused-cloud and two-palette unused-base
  boundaries, the 68000 music-wait command, Z80 driver build chain, and the complete six-entry RNG
  service boundary
- Status: **Inferred** for caller-visible retry distribution and perceived RNG delay
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
is [`input-system.md`](../design/input-system.md). ADR 0005's 2026-07-23 priority decision freezes
raw controller electrical/model/latency exactness after that visible input contract is adequate; it
does not freeze a concrete UI/menu acceptance gap in repeat or wait behavior.

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
[`save-system.md`](../design/save-system.md). Physical-media/failure exactness is priority-frozen by
ADR 0005 once save/load/copy/delete behavior is adequate; a user-visible save-flow acceptance failure
remains a reason to reopen one bounded question.

`PlayMusicAfterCurrentOne` sends the wait-for-current-music command and parameter command, then polls
the mailbox with three-frame sleeps. The Z80 source statically exposes YM2612, PSG, DAC, bank, music,
and SFX machinery, but audible timing and channel output remain outside this static contract and are
priority-frozen by ADR 0005 unless the established sound seam exposes an acceptance gap.

## RNG Service Contract

The two RNG sources form a six-entry static surface: `GenerateRandomNumber` (`0x1600`),
`WaitForRandomValueToMatch` (`0x1628`), `GenerateRandomValueUnsigned` (`0x1652`),
`GenerateRandomOrDebugNumber` (`0x1674`), `GenerateRandomValueSigned` (`0x1AD090`), and
`GenerateRandomNumberUnderD6` (`0x1AD0B4`). Named-function guards and comment-stripping direct
call parsing cover each entry.

**Confirmed:** the main generator updates the 16-bit `RANDOM_SEED` with multiplier 13 and increment
7, preserves `d6`, doubles its range before unsigned multiplication, then uses the upper product
word and halves the result. `GenerateRandomOrDebugNumber` saves `d6`/`d7`; when debug mode is enabled
it checks Right, Up, Left, Down and returns 0, 1, 2, 3 respectively without calling the base generator.
Disabled debug or no direction takes the base-generator fallback. Existing H3 evidence is
`rng-v1.json` (`sf2-rng-generate-random-number-v1`) and `debug-rng-v1.json`
(`sf2-rng-debug-override-v1`).

**Confirmed:** `GenerateRandomValueUnsigned` advances the word at `RANDOM_SEED_COPY` by
`state * 541 + 12345` and returns its masked low byte. The misleadingly named
`GenerateRandomValueSigned` reads that low byte, sign-extends it before an *unsigned* multiply by
the same constants, then masks and stores its low byte. Both bounded helpers return zero for low-byte
ranges 0, 1, and 128..255. For 2..127 they accept unsigned bytes 0..range-1 and retry otherwise.
The `GenerateRandomNumberUnderD6` source comment claims lower bound 2; that claim conflicts with its
actual comparison and is preserved as a discrepancy, not normalized into the contract.

The direct-call inventory contains 131 direct `GenerateRandomNumber` sites, 26 direct
`GenerateRandomOrDebugNumber` sites, and no direct sites for the other five named entries. In
particular, `GenerateRandomNumberUnderD6` has **zero direct target sites**; six sites call the
separate `j_GenerateRandomNumberUnderD6` jump-interface alias in
`code/common/tech/jumpinterfaces/s13_jumpinterface.asm`. This distinguishes source spelling
from runtime reachability. **Unknown:** the retry count/distribution and timing, and whether
`RANDOM_SEED_COPY` is isolated across text, menu, and AI callers. The single grouped H3 matrix is
`random-services-matrix-range-retry-and-seed-copy-isolation`; existing range-two action evidence is
`battle-ai-action-choice-v1.json` (`sf2-battle-ai-action-choice-runtime-v1`). The implementation
boundary is [`randomness.md`](../design/randomness.md).

## Concentrated Runtime Queue

No emulator was launched for this batch. ADR 0005 (priority decision 2026-07-23) freezes raw
controller electrical/latency behavior, SRAM hardware-failure behavior, audio timing/register output,
and VDP/DMA micro-timing after their import and visible contracts are adequate. The active grouped
questions are:

1. controller/input behavior that leaves a concrete UI/menu wait or repeat acceptance ambiguity;
2. SRAM signature/checksum/occupied-flag and save/copy/delete/reload behavior that leaves a concrete
   user-visible save-flow ambiguity;
3. music loop/transition/fade/resume or SFX selection/priority/interruption behavior when the existing
   command/channel seam is insufficient for a remake acceptance contract;
4. RNG low-byte range/retry boundaries and seed-copy isolation across text, menu, and AI callers;
5. asset routing or a user-visible presentation contract for the nominally unused graphics resources.

Reuse the existing VInt/controller, save-state, sound-driver, or graphics seam for one bounded reopen
question. Isolated one-case fixtures and hardware-fidelity expansion are not warranted without ADR
0005's acceptance, provenance/import, explicit-fidelity, or conflicting-evidence trigger.

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
