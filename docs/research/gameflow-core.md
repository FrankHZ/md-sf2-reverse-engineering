# Startup, Main Loop, and Exploration Core

- Status: **Confirmed** for all 13 layout-owned files, representative H1 addresses, cold/system
  initialization order, main battle/exploration routing, six map-event routes, interaction admission,
  item handoff, player-action priority, and map-event-before-action polling/dispatch
- Status: **Inferred** for interrupt-edge event/input perception and transition timing
- Status: **Unknown** for reset/TMSS hardware variations, rejected-region presentation on real
  hardware, and exact exploration/VDP frames
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Complete Source Boundary

This inventory covers the seven files under `code/gameflow/start`, `code/gameflow/mainloop.asm`, and
all five files under `code/gameflow/exploration`. All thirteen are included by the main ROM layout.
Together they contain 3,126 lines, 200 global labels, 81 local labels, and 176 direct 68000 call
sites to 114 unique named targets. Every file has one representative source symbol bound to the H1
assembler listing.

The boundary connects the already-inventoried hardware, map, battle, menu, scripting, stats, and
sound services into the playable top-level flow. It does not claim that all 200 labels are fully
understood.

## Startup Contract

`Start` distinguishes the existing-hardware state before performing its full setup. The cold path
writes 24 initial VDP registers, copies a 38-byte Z80 bootstrap, clears 64 KiB of 68000 RAM, clears
128 CRAM bytes and 80 VSRAM bytes, and silences four PSG channels. It then waits for DMA availability
and enters `InitializeSystem`.

The next statically verified sequence is:

1. `InitializeVdp` (including 19 maintained VDP register values);
2. `InitializeZ80` (copy the separately generated sound-driver binary into Z80 RAM);
3. `InitializeVdpData` (queues, scroll buffers, palettes, sprites, and DMA processing);
4. `InitializeGame`.

Game initialization loads 4,096 base tiles through compression mode 2, applies the region gate,
initializes new-game state, and displays the Sega logo. Start at the logo can skip the intro. After
the intro/title path, one title result resets through the original start vector while the other
enters the witch screen.

`CheckRegion` masks hardware-info bits `$C0` and accepts `$80`. The rejected path renders its warning
and enters an infinite loop. Static source proves this branch shape, not every console revision's
hardware-info value or video result.

## Main and Exploration Contract

`MainLoop` applies flag-driven map switching, checks for a battle, and treats battle index `-1` as
the no-battle sentinel. A real battle calls `BattleLoop`; its return passes through map switching
again before exploration. Exploration returns to the outer loop for warp-style transitions.

`ExplorationLoop` heals eligible allies, establishes or resumes map/entity state, loads map resources,
runs the map setup function, chooses music/fade behavior, and then alternates between map events and
A/C player actions. The six explicit map-event types are:

1. warp;
2. enter caravan;
3. enter raft;
4. leave caravan;
5. leave raft;
6. zone event.

Warp handling includes the source's hard-coded Pacalon completion flag 530 branch. Vehicle and zone
behavior route to their named map/script helpers.

`WaitForEvent` polls `MAP_EVENT_TYPE` before reading A/C input. After it returns, the outer loop also
tests and dispatches the map event before it tests the player-action result. Therefore, when both
values are already visible in one polling iteration, the map event wins. `ProcessMapEvent` clears the
pending event before selecting one of the six handlers; an out-of-range type plays
`SFX_BATTLEFIELD_DEATH` and returns. This is a **Confirmed static branch-order rule**. The exact VInt
edge at which an entity script publishes an event versus the input sample remains a timing question,
not an unknown priority rule.

Player actions test A before C. C can enter debug routes, use the caravan when co-located, activate
an entity event, inspect an area, or fall through to the field menu; A goes to the field-menu path.
Entity activation scans 48 candidates, skips the player and followers, and admits a candidate within
one internal map-tile unit (384 fixed-point units). Desk/counter blocks can extend the inspected
position.

Area inspection distinguishes chest `$1800`, generic `$1C00`, vase `$2C00`, barrel `$3000`, and
bookshelf `$3400` block kinds. Found items go to the player when possible, then another force member;
when every inventory is full, the chest is closed or the non-chest item is restored. Door, roof,
chest, scroll, and plane update code is owned by the same exploration boundary, but its visible frame
sequence is not promoted from static evidence.

## Concentrated Runtime Queue

No emulator was launched for this inventory. Four coherent matrices remain queued:

1. reset, TMSS, Z80 bus, and region-hardware variations;
2. Sega-logo/intro/title skip and debug input timing;
3. map-event publication versus A/C sampling at the VInt edge;
4. scroll, door, roof, warp, and vehicle transition frames.

The two exploration matrices should reuse one prepared map/entity harness rather than start a new
emulator for each action or transition.

## Reproduction

```powershell
uv run sf2 h2 gameflow-core
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/gameflow-core-static.json`.
