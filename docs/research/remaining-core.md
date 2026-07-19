# ROM Header, Window Engine, and Special Debug Flows

- Status: **Confirmed** for the final five primary layout files outside earlier subsystem inventories,
  their representative H1 addresses, header/vector shape, window-slot/animation structure, battle-test
  roster, configuration toggles, and debug action routes
- Status: **Inferred** for window-motion perception and debug UI timing
- Status: **Unknown** for rendered window/DMA frames and simultaneous debug input presentation
- Evidence date: 2026-07-19
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

The window engine owns eight 16-byte window entries. `CreateWindow` returns `-1` when no slot is free
and advances a shared tile-layout end pointer for successful allocations. Deleting a window recomputes
that end pointer from the remaining layouts.

Movement state uses a per-slot bitfield and integer linear interpolation between packed X/Y origins
and destinations. Special Turbo forces animation length to one. `VInt_UpdateWindows` owns movement,
hide/fix state, map-layout composition, and DMA queue updates; window tile lookup converts packed
coordinates into the per-window layout address. Exact clipping, scrolling interaction, and frame
parity remain runtime presentation questions.

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

No emulator was launched for this inventory. Two grouped questions remain:

1. window animation, hide/fix, scrolling, clipping, and DMA frames;
2. configuration/debug input chords and menu presentation.

The debug-only paths are preservation evidence, not remake requirements unless a later design
decision explicitly retains developer tools.

## Reproduction

```powershell
uv run sf2 h2 remaining-core
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/remaining-core-static.json`.
