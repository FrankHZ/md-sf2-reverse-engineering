# Remaining Technical Services

- Status: **Confirmed** for the complete 12-file source boundary, eleven main-ROM representative
  addresses, twenty technical-resource incbin mappings, byte-copy direction, input scan/wait shape,
  SRAM slot/checksum structure, the complete variable-width-font, context-Huffman, and witch-menu
  direct payload/pointer boundaries, the four-stream unused-cloud and two-palette unused-base
  boundaries, the 68000 music-wait command, Z80 driver build chain, and the complete six-entry RNG
  service boundary, the one-launch range-low-byte retry and controlled source-shaped copy matrix,
  and the fourteen-case in-process SRAM lifecycle matrix
- Status: **Inferred** for caller-visible retry distribution and perceived RNG delay
- Status: **Unknown** for controller hardware edge cases, SRAM persistence/corruption behavior, and
  rendered/audio timing
- Evidence date: 2026-08-03
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
24-frame initial delay and six-frame cadence. The H2 source inventory additionally records the static
shapes of `WaitForPlayerInput`, `WaitForPlayer1NewInput`, the one/three-second waits, and `sub_15A4`,
but does not promote their caller-dependent runtime behavior here. The comment-stripping parser finds
four source-local `WaitForVInt` call sites and eleven external sites across nine callers: one
`UpdatePlayerInputs` site and ten `WaitForPlayerInput` sites; the remaining four entry points have
zero static direct-call sites, which does not establish runtime reachability.

**Confirmed H3, bounded:** `sf2-controller-input-runtime-v1` uses one BizHawk 2.11.1 / Genesis Plus
GX launch and direct original-function seams. Its five `UpdatePlayerInputs` cases prove neutral,
Player 1 Up+B, Player 2 C+Start, simultaneous combined basic buttons, and release across both raw
state bytes for both controller ports. Its three direct `ApplyZ80BusUpdates` input-stage cases prove
new press, release/repress, and continuous held-input suppression through the exact 24-frame threshold
with the six-frame cadence. The observer confirms the direct call/target/return triples and, for the
repeat cases, the original nested `ApplyZ80BusUpdates` call to `UpdatePlayerInputs` at H1
`0x09F6`/`0x09FA`. `CheckSram` return redirection is bootstrap-only. This is direct VInt input-stage
observation, not normal `WaitForVInt` caller progression, UI behavior, or a hardware-latency result.
The temporary work-RAM gate arms one direct call after each host frame boundary and pauses after its
return, so the fixture's real joypad input is visible before every observed original call.
Reproduce with `uv run sf2 h3 controller-input --timeout-seconds 180`.

**Unknown:** runtime `WaitForPlayerInput`, `WaitForPlayer1NewInput`, one/three-second waits, and
`sub_15A4`; controller-model and three-/six-button negotiation; hardware latency; and user-visible
UI/menu timing. The implementation-neutral contract is
[`input-system.md`](../design/contracts/input-system.md).
ADR 0005's 2026-07-23 priority decision freezes raw controller electrical/model/latency exactness
after the visible input contract is adequate; it does not freeze a concrete UI/menu acceptance gap in
repeat or wait behavior.

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

## Confirmed Direct-Service Lifecycle Matrix

**Confirmed:** `sf2-sram-lifecycle-runtime-v1` is one BizHawk 2.11.1 / Genesis Plus GX launch with
fourteen fixture-defined direct-service cases, in this fixture order: signature mismatch init, valid
empty state, valid slot 1, valid slot 2, invalid slot 1, invalid slot 2, save slot 1, save slot 2,
load slot 1, load slot 2, copy 1-to-2, copy 2-to-1, and clear occupied flag for selectors 0 and 1.
The bootstrap captures the original `CheckSram` return only to enter a harness-defined work-RAM
direct-function probe; it neither observes nor promotes Witch-menu behavior.

The matrix checks the full 4,016 logical bytes of every tracked slot with a deterministic compact
span (checksum, mismatch count, boundaries, and sentinels), not only selected samples. It confirms
the signature-mismatch path clears all 8,192 logical locations, writes the source-defined 17-byte
checked signature prefix, and clears flags. It confirms both valid and invalid occupied-bit outcomes,
both save/load selectors, both `CopySave` directions, and both flag-clear selectors. For copy,
runtime callbacks confirm entry into the nested `LoadGame` and `SaveGame` functions. The source
guard establishes `LoadGame`-before-`SaveGame` instruction order, while H1 binds the source-derived
call/return addresses `0x6FDC`/`0x6FDE` and `0x6FE6`/`0x6FE8`. Those addresses provide expected
diagnostic context; the call and return sites are not themselves callback-observed.

Every registered execution callback is wrapped in the observer failure/status contract, reports the
case, phase, role, and expected/actual PC state, and shares a deterministic dispatch list when roles
meet at one physical PC. The final run records zero registered callbacks, zero logical SRAM residue,
an observer-finished status, and no Lua Console error. Reproduce with:

```powershell
uv run sf2 h3 sram-lifecycle --timeout-seconds 180
```

The tracked fixture, schemas, and verifier are
`tests/fixtures/h3/sram-lifecycle-v1.json`, `schemas/h3/sram-lifecycle-*.schema.json`, and
`src/sf2tool/h3/sram_lifecycle.py`. They are tied to the H2 seven-entry source model and the pinned
`SF2DISASM` baseline named in the fixture provenance.

The direct-call parser (comments excluded) finds seven external call sites in three source files:
church `SaveGame`, battle suspend `SaveGame`, and witch `CheckSram`/`SaveGame`/`LoadGame`/
`CopySave`/`ClearSaveSlotFlag`, one site each. **Inferred:** these callers intend their respective
save UI/lifecycle operations; their full caller-state outcomes are not promoted solely from the
direct-call inventory. **Unknown:** cross-process or durable-media persistence, power loss between
data/checksum/flag writes, torn-write recovery, corruption beyond this checksum, SRAM bus/bank/cycle
behavior, emulator storage behavior, normal story/church/battle caller persistence, and the resulting
player-visible timing. The matrix deliberately does not reopen ADR 0005 hardware exactness. The
remake-facing extraction is
[`save-system.md`](../design/contracts/save-system.md). Physical-media/failure exactness is priority-frozen by
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
`GenerateRandomValueSigned` has the H2 source shape “read one byte at the `RANDOM_SEED_COPY` base
address, sign-extend before an *unsigned* multiply by the same constants, mask the computed result
to one byte, and write one byte back at that same base address.” Both bounded helpers return zero for
range low-byte values 0, 1, and 128..255. For 2..127 they accept unsigned bytes 0..range-1 and retry otherwise.
The `GenerateRandomNumberUnderD6` source comment claims lower bound 2; that claim conflicts with its
actual comparison and is preserved as a discrepancy, not normalized into the contract. One BizHawk
2.11.1 / Genesis Plus GX natural-start probe uses the accepted debug Battle Test route only as setup,
then redirects the original `GenerateBattleTurnOrder` return through a work-RAM probe. That probe
executes original `WaitForRandomValueToMatch`, `j_GenerateRandomNumberUnderD6`, its effective target,
and the base generator against ten generated cases. It observes entry, helper-generator return, helper
return, and the exact `move.b d7,RANDOM_SEED_COPY` shape; it establishes no Battle Test behavior. The
range low-byte values 0, 1, and 255 return zero after one generated value. Unsigned range two retries
`194, 51, 0`; thinking range two reaches zero after 57 generated values from this exact seed. The H3
word result resolves the source-shape lane: on the 68000 big-endian word at `$FFDFB0`, the base-address
byte is the word high byte. The original bounded helpers return `d7=0` with helper-return seed-copy
states `$53C2` and `$985D`; the controlled `move.b d7,RANDOM_SEED_COPY` probe instruction then changes
those states to `$00C2` and `$005D`. It is not helper-local behavior. The source-context text and
diamond shapes similarly take `$ABCD → $ECCD` and `$5A33 → $6833`, preserving the low byte. Both helper
paths leave `RANDOM_SEED` unchanged. The complete observed matrix is
`tests/fixtures/h3/random-services-v1.json` (`sf2-random-services-matrix-runtime-v1`).

The direct-call inventory contains 131 direct `GenerateRandomNumber` sites, 26 direct
`GenerateRandomOrDebugNumber` sites, and no direct sites for the other five named entries. In
particular, `GenerateRandomNumberUnderD6` has **zero direct target sites**; six sites call the
separate `j_GenerateRandomNumberUnderD6` jump-interface alias in
`code/common/tech/jumpinterfaces/s13_jumpinterface.asm`. This distinguishes source spelling
from runtime reachability. The text `symbol_wait1` and diamond-menu direct source sites are represented
only as source-context copy-shape rows: both set range 256, call the base generator, then use the
same byte-store encoding. **Unknown:** their full caller loops, VInt/input/menu/text timing, and
cross-caller seed-copy lifetime/isolation. The grouped H3 queue remains
`random-services-matrix-range-retry-and-seed-copy-isolation` for that caller-context boundary; existing range-two action evidence is
`battle-ai-action-choice-v1.json` (`sf2-battle-ai-action-choice-runtime-v1`). The implementation
boundary is [`randomness.md`](../design/contracts/randomness.md).

## Concentrated Runtime Queue

This batch's one-launch random-services H3 probe closes only the helper-local range/retry, alias, and
controlled source-shaped copy boundary. It does not execute text/menu/AI caller contexts or establish
their timing or shared seed-copy lifetime. ADR 0005 (priority decision 2026-07-23) freezes raw controller
electrical/latency behavior, SRAM hardware-failure behavior, audio timing/register output, and VDP/DMA
micro-timing after their import and visible contracts are adequate. The remaining grouped questions are:

1. controller/input behavior that leaves a concrete UI/menu wait or repeat acceptance ambiguity;
2. SRAM durable-media, interrupted-write, or normal player-caller behavior that leaves a concrete
   user-visible save-flow ambiguity; the direct in-process service lifecycle is now observed;
3. music loop/transition/fade/resume or SFX selection/priority/interruption behavior when the existing
   command/channel seam is insufficient for a remake acceptance contract;
4. RNG text/menu/AI caller-context execution and seed-copy lifetime/isolation (the helper-local
   range/retry and base-address byte-lane facts are now observed);
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
uv run sf2 h3 random-services --timeout-seconds 180
uv run sf2 h3 sram-lifecycle --timeout-seconds 180
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/tech-services-static.json` and
`local/derived/unused-technical-assets-static.json`.
