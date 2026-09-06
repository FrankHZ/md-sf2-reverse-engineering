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

Game initialization expands the base-tile Stack stream to 8,192 bytes and uploads 4,096 words through
compression mode 2, applies the region gate,
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

The bounded [field-search-control.md](field-search-control.md) owner retains the exact static
`CheckArea` caller polarity, faced-tile derivation, dispatch order, content classification, and
fallback/rollback control. This core document retains the broader exploration inventory only.

## Warp Facing and Return Source Guard

Evidence date: 2026-09-05. **Confirmed (source structure only):** the existing
`uv run sf2 h2 gameflow-core` command now calls `_guard_warp_facing_handoff` in
[`gameflow.py`](../../src/sf2tool/h2/gameflow.py) during inventory construction, before fixture
comparison. It uses the same pinned source revision stated above and the existing source/H1
inventory owners; it does not add an instruction-byte parity or runtime observation claim.

The bounded source chain is:

- `code/gameflow/exploration/exploration.asm`, `WarpIfSetAtPoint`: the selected row writes
  `MAP_EVENT_WARP` as a word, the type/map/coordinate fields as a longword starting at
  `MAP_EVENT_PARAM_1`, and the facing field as a word at `MAP_EVENT_PARAM_5`. The guard retains the
  exact/wildcard scan branches, their label destinations, write order, and saved-register return.
- `code/gameflow/exploration/explorationfunctions_2.asm`, `ProcessMapEvent`: the cleared request
  and first decrement lead to a conditional **branch**, not a subroutine call, to
  `ProcessMapEventType1_Warp`.
- In that handler, a **byte** test of `MAP_EVENT_PARAM_1` separates the zero and nonzero paths.
  The zero path balances its sound-command word save/restore, calls `j_MakeEntityIdle`, and discards
  one stack longword with `movem.l (sp)+,d0`. After `UpdatePlayerPosFromMapEvent` and the coordinate
  branches, it reads `MAP_EVENT_PARAM_5` as a **byte** into D3, then reads parameter 1 into D4 and
  reaches its first RTS with no intervening call. This is the source's facing-parameter return path.
- The nonzero path instead calls `ProcessMapTransition`, performs its coordinate branches, then
  calls `UpdatePlayerPosFromMapEvent` and `j_DeclareRaftEntity` before its own RTS. It contains no
  `MAP_EVENT_PARAM_5` access or extra return-address pop. The helper reads the entity's X/Y words,
  uses the source `MAP_TILE_SIZE` divisions, clears D3, reads `ENTITYDEF_OFFSET_FACING` as a byte,
  restores A0, and returns. In particular, this does **not** claim that the later raft call preserves
  D3, that the transition preserves the prior entity facing, or that either callee's effects occurred.
- The exact `ExplorationLoop` event-dispatch block calls `ProcessMapEvent` with BSR and otherwise
  branches back to its event loop. `code/gameflow/mainloop.asm`, `MainLoop`, calls
  `j_ExplorationLoop` with JSR and follows that call with its outer-loop backedge. Together with the
  handler's tail dispatch and explicit longword pop, these are static caller/return relationships,
  not observed stack contents or an executed return to MainLoop.

The guard extracts each exact named function through the existing function-block parser, excludes
instruction-like comments, rejects duplicate boundaries/labels, and compares ordered source
statements rather than searching for fragments anywhere in a file. Within the producer/handler it
retains every control/stack operation, predicate, direct D3 access, and warp-field access; remaining
arithmetic/data moves are not newly modeled algorithms. Unknown macros and inline machine-code
directives are rejected. The helper and direct caller blocks are checked completely. Whitespace,
inline comments, and equivalent short/word branch suffixes are accepted; data-access and stack widths
remain exact. No whole-game caller inventory or general control-flow framework is introduced.

[`test_gameflow.py`](../../tests/python/test_gameflow.py) supplies the source-parser boundary tests,
legal-suffix positive cases, and scoped mutations of producer/consumer fields and widths, branch
polarity/targets/ownership, D3 clobbers, stack operations, and RTS/call positions. The initial
`test_warp_facing_wrong_parameter_fails_during_inventory_construction` completed with
`DID NOT RAISE` when parameter 5 was replaced by parameter 4; the added guard rejects that mutation
during construction. The fixture, schemas, manifest, output shape/values, index, and counters are
unchanged.

The first direct-consumer `uv run sf2 h2 map-event-request-consumption` attempt completed with
exit 1 because the new worktree lacked `build/sf2build-h1.bin`; the interaction-state consumer had
not started. The independent input copy was then supplied and matched the canonical ROM identity.
This is an input-preparation correction, not evidence of a guard failure or a new H1 rebuild.

**Unknown:** actual caller state and D3 at runtime returns, natural reachability, downstream init and
final entity facing, callee effects, timing, presentation, and persistence. This source-only guard
does not reopen any completed route investigation or authorize an H3 launch. A frame-end debugger
query would not establish the instruction-time state at these return boundaries.

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
uv run sf2 h2 ui-graphics
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/gameflow-core-static.json`.
