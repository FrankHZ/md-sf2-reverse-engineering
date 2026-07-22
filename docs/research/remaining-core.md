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
The engine-neutral static boundary is in [`window-system.md`](../design/window-system.md).

## Development and Debug Flows

The battle-test path joins the 29 non-Bowie allies, operates on the full 30-member roster, sets
Bowie's selected test stats to 99, and exposes battle indexes through 49 and shop indexes through
100. It connects battle, church, shop, field, caravan, members-list, and whole-force level-up tools.

Configuration mode owns four toggles: Special Turbo, Control Opponent, Auto Battle, and Game
Completed. Sound-test routing requires Start+Up and the completed bit, but the US sound-test target
is the return-only stub documented in the special-screen inventory.

The debug battle-action table has seven routes: Attack, Magic, Item, End Turn, Burst Rock, Muddle,
and Prism Laser. A separate helper can force four battle-scene outcomes: dodge, critical, double,
and counter.

## Concentrated Runtime Queue

No emulator was launched for this inventory. The window queue is one future generated presentation
matrix: allocation/failure and delete-end-pointer boundaries; ordinary and Special-Turbo movement;
hide/fix transitions; composition/clipping/scroll offsets; and the VInt/DMA queue before/after frames.
It must share setup and VInt/VDP observation points rather than add one-case fixtures. The separate
configuration/debug input and menu-presentation queue remains unchanged.

The debug-only paths are preservation evidence, not remake requirements unless a later design
decision explicitly retains developer tools.

## Reproduction

```powershell
uv run sf2 h2 remaining-core
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/remaining-core-static.json`.
