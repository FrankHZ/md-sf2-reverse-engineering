# ROM Header, Window Engine, and Special Debug Flows

- Status: **Confirmed** for the final five primary layout files outside earlier subsystem inventories,
  their representative H1 addresses, header/vector shape, complete window-engine source/control-flow
  contract, battle-test roster, configuration toggles, and debug action routes
- Status: **Inferred** for caller-dependent window and debug UI meaning
- Status: **Unknown** for rendered window/DMA frames, clipping/scrolling perception, queue timing or
  capacity, hardware behavior, and simultaneous debug input presentation
- Evidence date: 2026-07-21
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Boundary and Coverage Meaning

This batch closes the last five main-layout files that can be honestly connected to a named 68000
symbol in the H1 listing:

- `code/romheader.asm`;
- `code/common/windows/windowengine.asm`;
- `code/gameflow/special/battletest.asm`;
- `code/gameflow/special/configurationmode.asm`;
- `code/gameflow/special/debugmodebattleactions.asm`.

Together they contain 1,210 lines, 69 global labels, 31 local labels, and 107 direct call sites. The
remaining six files outside strict symbol reach are not forgotten work: three unassembled alternate
item sources, one overlapping member-list alternate, one unlabeled scripting blob, and the separately
assembled Z80 sound-driver source. Each exception is already represented by its owning H2 inventory.

## ROM Header

The source defines 64 vector entries before the console header. HInt is level 4, VInt is level 6,
and named traps cover trap 0 through trap 9. The machine-checked header facts include product code
`GM MK-1315 -00`, checksum `$8921`, ROM end `$1FFFFF`, SRAM range `$200001..$203FFF`, and region `U`.
These agree with the independent H0 ROM identity rail.

## Window Engine

**Confirmed.** The 644-line `windowengine.asm` source contains 16 stable global entries at H1
addresses `0x47C6..0x4C44`: initialization, allocation/destination/fix/move/delete/wait helpers,
the VInt updater, three private composition helpers, and the two address helpers. The machine-readable
contract records all entry addresses, six internal direct target identities/site counts, 32 external
direct-caller files with each target's site count, and the six separate `dc.l VInt_UpdateWindows`
references. The latter are pointer data rather than direct calls; a zero direct-caller total does not
establish that an entry is unreachable.

**Confirmed.** `sf2const.asm` and `sf2enums.asm` are parsed once for the entry/table addresses,
counter/stride values, field offsets, and packing masks. The counter `WINDOW_ENTRIES_COUNTER=7` derives
eight slots; `WINDOW_ENTRY_SIZE=$10` derives the 16-byte entry stride; and
`WINDOW_ENTRIES_LONGWORD_COUNTER=$1F` derives 32 longword clears / a 128-byte reset span, independently
matching eight entries. Each entry uses bytes 0..3 as the layout-address/active-test longword, width and
height at bytes 4 and 5, packed position/origin/destination coordinates at bytes 6..11, animation
length/counter at bytes 12 and 13, and source-unlabeled bytes 14 and 15. The upstream
`WINDOWDEF_OFFSET_*` labels are retained in the contract; the two unlabeled bytes are deliberately not
given a lifecycle name.

**Confirmed.** `CreateWindow` scans all slots in ascending order, returns `-1` when none has a zero
low-word active test, conditionally copies Plane A only when the layout-end pointer is at the base,
then writes the layout address, packed size, three packed positions, initial length/counter bytes, and
cleared unlabeled bytes. Its layout-end change is `width * height * 2`; the two is the separately
recorded layout-tile word width, not an allocation capacity claim. `DeleteWindow` clears the selected
layout address first, then recomputes the end from the greatest remaining layout address and its
width/height, or resets it to the base when none remains.

**Confirmed.** Packed coordinates use the upper byte for X and lower byte for Y. `GetWindowEntryAddress`
calculates `WINDOW_ENTRIES + slot * 16`; `GetWindowTileAddress` calculates
`layout + (Y * width + X) * 2`. The map helper separately derives a 32-tile/64-byte row stride and has
source checks for the `<32` horizontal and `<28` vertical composition boundary. The special black-bar
scroll adjustment exists in `sub_4BEA`; its visible effect is not promoted beyond the source operation.

**Confirmed.** `SetWindowDestination` acts only on an active non-moving entry, resolves packed
`$8080` to the current position, and writes origin, destination, then length 256. `MoveWindow` resolves
the same sentinel, writes origin/destination/length and clears the counter; Special Turbo forces its
length input to one. `MoveWindowWithSfx` contains `sndCom SFX_MENU_SWITCH` and has no return before the
following `MoveWindow` entry, so its source fall-through order is guarded. `WaitForWindowMovementEnd`
waits for VInt before testing the moving-bitfield and repeats while any bit is set.

**Confirmed.** `VInt_UpdateWindows` returns when the layout end is still the base. Otherwise it clears
the moving bitfield, first restores each moving entry through `sub_4B5C`, then increments its counter
and uses signed integer multiply/divide interpolation before packed-coordinate recomposition. With
hiding clear it composes through `sub_4AC8`; its byte-14 and byte-15 paths respectively compose or
restore once and clear that byte. The hide/fix branches copy Plane A before marking fixed, or call
`FixWindowsPositions` when showing. When the update toggle is set, source order is an
`ApplyVIntVramDma` call with arguments `$C000`, `$400`, and `2`, then
`EnableDmaQueueProcessing`, then clearing the toggle. Those are source arguments, not a claimed byte
count, queue capacity, completion frame, or hardware transfer result.

**Inferred.** The external callers show extensive menu, text, map, battle, and witch use, but do not
by themselves prove admission conditions, caller-visible result meanings, or user-visible movement.
The engine-neutral static boundary is in [`window-system.md`](../design/contracts/window-system.md).

## Development and Debug Flows

**Confirmed.** The three debug sources contain eight H1-bound global entries: the battle-test entry
and its three local helpers, configuration entry, battle-action selector, target prompt, and hit-choice
helper. The H2 fixture records their exact H1 addresses; `sf2const.asm` and `sf2enums.asm` are parsed
once for nine RAM labels and 18 enum values. The contract keeps the physical RAM addresses, prompt
bounds, roster counts, loop counters, and per-record byte stride separate.

**Confirmed.** `DebugModeBattleTest` first writes the debug and Special Turbo toggles, then calls
`j_JoinForce` for the exact ordered 29-label non-Bowie roster. It next applies source value 99 through
eight named Bowie stat setters, registers `VInt_UpdateWindows` as a VInt add-pointer, writes a
30-length generic-list declaration alongside its exact stored 0..31 byte sequence, and calls
`CheatModeConfiguration`. The guarded flow sends a negative battle prompt result to the member/level-up
loop; otherwise its zero-to-49 battle selection, optional zero-to-one cutscene flag path, follower flag,
seven-byte map-coordinate stride, and battle → church → zero-to-100 shop → field → caravan call order
are source facts. The members result first tests byte `d0` and branches back on nonzero; its only
fallthrough therefore has zero and then takes the following `bpl` to whole-force level-up. The source
still contains a church-call block between those branches, but it is statically unreachable under that
preceding `tst`/`bne`/`bpl` sequence. Its runtime meaning is **Unknown** rather than a claimed negative
route. Its stat-display helper loops all 30 ally slots, stores six
packed-decimal words at offsets 0, 2, 4, 6, 8, and 10 of each 16-byte record, and refreshes current HP
and MP after their maximum getters.

**Confirmed.** `CheatModeConfiguration` first requires Start. Its next Up-bit/completed-bit `bne.w`
conditionally transfers directly to `j_SoundTest` without pushing a return address; this three-file
slice does not claim the target implementation or sound-test presentation. The existing special-screen
inventory separately records that the US target implementation is an `rts` stub. Without that edge it requires the
configuration toggle, then presents text IDs 450, 451, 452, and 455 in order. A zero response writes
`-1` to the three named toggle bytes; the fourth response sets or clears save-flag bit 7.

**Confirmed.** The action selector admits values zero through six, returns on `-1`, stores the selected
word, then uses the exact seven-entry relative table in order: Attack, Magic, Item, EndTurn, BurstRock,
Muddle, PrismLaser. Attack and Magic/Item target calls stay distinct from the table dispatch. Magic
combines its one-to-four level selection with a six-bit shift before its zero-to-42 spell selection;
Item uses zero-to-127 and then zero-to-three prompts; target selection is 128 through 159; PrismLaser
writes the source-labelled battle value. The hit helper executes four prompt/test/`seq` triples in
source order to the separately recorded stack aliases `debugDodge`, `debugCritical`, `debugDouble`, and
`debugCounter` at offsets -23 through -20. These are source writes, not a runtime claim that every
caller exposes the debug UI.

**Confirmed.** Comment-stripped instruction parsing finds two external caller files: one battle-actions
file has one site each for `DebugModeActionSelect` and `DebugModeSelectHits`, and the witch start file
has two `CheatModeConfiguration` sites. Direct-call zeros are retained for the other five targets and do
not assert unreachability; no external `dc.l` pointer occurrence is found in this bounded scan.

**Inferred.** The labels and route structure strongly suggest developer tooling, but caller admission,
prompt cancellation semantics beyond the checked branches, and player-visible results remain unobserved.

## Concentrated Runtime Queue

No emulator was launched for this inventory. The window queue is one future generated presentation
matrix: allocation/failure and delete-end-pointer boundaries; ordinary and Special-Turbo movement;
hide/fix transitions; composition/clipping/scroll offsets; and the VInt/DMA queue before/after frames.
It must share setup and VInt/VDP observation points rather than add one-case fixtures. The separate
debug-flow queue is `debug-flow-input-chords-menu-selection-and-action-state-matrix`: one grouped
matrix for Start/Up/completed-bit admission, configuration responses, action selection/cancel, and
the four stack-alias writes.

The debug-only paths are preservation evidence, not remake requirements unless a later design decision
explicitly retains developer tools.

## Reproduction

```powershell
uv run sf2 h2 remaining-core
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/remaining-core-static.json`.
The H2 command validates `schemas/remaining-core-static.schema.json` and
`schemas/h2-remaining-core-static-fixture.schema.json`, fixture ID
`sf2-remaining-core-static-v1`, pinned upstream commit, H1 entries, and canonical output hash.
