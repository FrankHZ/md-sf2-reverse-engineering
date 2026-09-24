# Map 3 Controlled-Start Egress Transition

## Scope and result

This owner closes the transition gap between the controlled Map 3 start at
`(56,3)` and the already accepted natural-route interaction with entity 142.
It joins existing source/H1/ROM and bounded H3 evidence; it does not add a new
runtime observation.

- Canonical private input: US ROM SHA-256
  `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source baseline: `SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`.
- Retained runtime owners:
  [`map3-admitted-start-v1.json`](../../tests/fixtures/h3/map3-admitted-start-v1.json)
  and
  [`map3-battle01-natural-route-v1.json`](../../tests/fixtures/h3/map3-battle01-natural-route-v1.json).
- Retained static owners:
  [`map-content-static-v1.json`](../../tests/fixtures/h2/map-content-static-v1.json)
  and
  [`map3-entity142-interactable-reference-static-v1.json`](../../tests/fixtures/h2/map3-entity142-interactable-reference-static-v1.json).

**Confirmed:** the required boundary is a two-warp, same-map chain. The first
warp moves the player from Map 3 area ordinal 2 into area ordinal 1; after the
accepted route through the house and school, the second moves the player from
area ordinal 1 into area ordinal 3, which contains the entity 142 interaction
position. Neither warp selects a different Map 3 setup variant.

## Stable records and area relation

Map 3's warp table is `data/maps/entries/map03/6-warp-events.asm`, ROM
`0x978F0..0x9793A`. Each row is eight bytes in the macro-defined order
`trigger X, trigger Y, type, target map, target X, target Y, facing, padding`.
The two forward records are:

| Role | Stable row identity | ROM address and bytes | Trigger target | Destination |
| --- | --- | --- | --- | --- |
| Bowie house stairs down | zero-based 8 / one-based 9 | `0x97930`, `36 03 00 FF 03 03 00 00` | `(54,3)` | current Map 3 `(3,3)`, `RIGHT` (`0`) |
| School stairs down | zero-based 5 / one-based 6 | `0x97918`, `2E 07 00 FF 3B 0C 02 00` | `(46,7)` | current Map 3 `(59,12)`, `LEFT` (`2`) |

The `type` byte is `0` (`warpNoScroll`) and the raw destination-map byte is
`0xFF` (`MAP_CURRENT`) in both records. The table is first-coordinate-match;
the trigger coordinate is the controlled entity's candidate target tile, not
a requirement that the entity first settle on that tile.

The stable consumer chain is `WarpIfSetAtPoint` (`0x42DC`) →
`WaitForEvent` (`0x2591C`) → `ProcessMapEvent` (`0x2594A`) →
`ProcessMapEventType1_Warp` (`0x25978`) →
`UpdatePlayerPosFromMapEvent` (`0x25A2A`) → `MainLoop` (`0x75C4`) →
`ExplorationLoop` (`0x257C0`) → `LoadMap` (`0x2A8C`) / `LoadMapArea`
(`0x2DEC`). The first landing additionally reaches `ToggleRoofOnMapLoad`
(`0x3F2C`). These are H1-bound program identities; the table row, rather than
an inferred scene name, supplies each transition's operands.

Map 3's three 30-byte area rows are
`data/maps/entries/map03/2-areas.asm`, ROM `0x977FE..0x9785A`:

| One-based ordinal | ROM address | Inclusive main-layer bounds | Second foreground/background start | Relevant point |
| --- | --- | --- | --- | --- |
| 1 | `0x977FE` | `(0,0)..(50,31)` | `(0,32)` / `(0,0)` | house landing `(3,3)` and school stair `(46,7)` |
| 2 | `0x9781C` | `(51,0)..(61,9)` | `(0,0)` / `(0,0)` | controlled start `(56,3)` and pre-warp `(55,3)` |
| 3 | `0x9783A` | `(51,10)..(61,19)` | `(0,0)` / `(0,0)` | stair landing `(59,12)`, stand `(55,17)`, entity 142 `(54,17)` |

`LoadMap` scans these rows in source order and selects the first inclusive
bounds containing the player/load coordinate. Thus the coordinate changes,
not a flag-switched layout definition, account for the observed area changes.
Area ordinal 1 is the sole row here with a nonzero second-foreground start.

## First warp: controlled pocket to area 1

The accepted start reaches the first `WaitForEvent` at Map 3 `(56,3)`, facing
`DOWN` (`3`), with the default setup `ms_map3` (`0x50AE8`) and
`ms_map3_InitFunction` (`0x51382`). Selector flags 609, 506, and 543 are clear.

The observed and source-joined chronology is:

1. `Left` moves the player to `(55,3)` and leaves facing `LEFT`.
2. A second `Left` targets `(54,3)`. The target block's warp marker is tested
   before passability; `WarpIfSetAtPoint` chooses warp row 8 and writes map
   event type 1 with parameters `0, 0xFF, 3, 3, RIGHT`.
3. `WaitForEvent` returns the request. `ProcessMapEvent` clears the request and
   dispatches `ProcessMapEventType1_Warp`.
4. Because parameter 1 is zero, the handler idles the controlled entity,
   unwinds the exploration return, and returns the raw current-map sentinel,
   destination `(3,3)`, facing `RIGHT`, and `D4=0` to `MainLoop`.
5. `ExplorationLoop` interprets the low `0xFF` map byte as the current-map
   reload path, updates the player, and calls `LoadMap`. That path preserves
   the already decoded working blocks/layout, selects area ordinal 1 from the
   destination coordinate, runs `ToggleRoofOnMapLoad`, reruns the selected
   Map 3 init, and reaches the next `WaitForEvent`.

**Confirmed (bounded H3):** the natural-route trace records
`map-event:warp:map3-bowie-house-exit`, a new `exploration:3`,
`map-init:ms_map3_InitFunction`, and `route:post-warp-wait-for-event`, in that
order. It records the landing `(3,3)`, working-layout byte offset `390`, and
layout word `20533`; the next `Right` follows the slope to `(4,4)`.

This transition is primarily a relocation plus area reselection. It is not a
setup-variant selection and it does not rebuild the base layout. There is one
separate, deterministic post-reload layout effect: at `(3,3)`, area ordinal
1's `(0,32)` second-layer origin makes the first containing roof record the
Bowie-house `slbc 4,8` record. Its source `(255,255)`, size `7x8`, and
destination `(2,32)` make `PerformMapBlockCopyScript` save and clear that
second-layer rectangle. This mutates the preserved working layout as
roof/presentation state; it is not the mechanism that changes the player's
coordinate or area.

## Entity service before the warp caller

**Confirmed (static source):** at the pinned revision above,
`disasm/code/common/scripting/entity/entityscriptengine_2.asm:VInt_UpdateEntities`
calls `UpdateEntityData` before dispatching that physical slot's action script.
`esc02_controlCharacter` checks other entities' current/reserved positions before
door handling and `WarpIfSetAtPoint`. A reached warp writes the event before
candidate passability is checked. If travel is allowed, the same action installs
velocity/travel/destination; otherwise the event can still be reached without
travel. Its tail calls `UpdateEntitySprite` and `esc_goToNextEntity`. The remaining
old-map slots therefore finish their motion/action work before this VInt returns.
A warp does not replace that population halfway through the producing pass.

`disasm/code/gameflow/exploration/explorationfunctions_2.asm:WaitForEvent`
checks `MAP_EVENT_TYPE` before A/C input. `ProcessMapEvent` clears the event and
dispatches `ProcessMapEventType1_Warp`; the type-zero path calls `MakeEntityIdle`
and unwinds to the main loop. `MakeEntityIdle` in
`disasm/code/common/scripting/entity/entityfunctions_2.asm` changes the action
pointer, without clearing velocity/travel. These are caller and service-order
facts, not a count of every enabled interrupt through the later transition.

Reproduce the source readback from the verified pinned SF2DISASM checkout:

```powershell
git show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/common/scripting/entity/entityscriptengine_2.asm
git show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/exploration/explorationfunctions_2.asm
git show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/common/scripting/entity/entityfunctions_2.asm
```

The existing `map-content-static-v1.json` owns the `VInt_UpdateEntities` identity;
`map3-battle01-natural-route-v1.json` binds `esc02_controlCharacter` and
`WaitForEvent` within its retained observation. No new runtime observation is
claimed for the service-order readback.

**Unknown:** the complete ordinary-transition opportunity schedule still includes
fade-out with the old population, gated display/map loading, base VInt service
re-enablement, map init and conditional fade-in/control return. Their source
owners are `ExplorationLoop`, `FadeOutToBlackAll` and `WaitForFadeToFinish` in the
exploration file above, `mapload.asm:LoadMap`, `displayinit.asm:InitializeDisplay`,
`battlevints.asm:SetBaseVIntFunctions`, and `fadingcommands.asm:ExecuteFading`.
Interruptions between those CPU phases are not enumerated by the explicit waits.
The producing pass alone cannot establish a fixed transition tick budget or the
post-warp seed. The [remake verification owner](../../remake/docs/development-and-verification.md#ordinary-field-action-and-warp-service)
separates implemented finite services from that remaining CPU-phase gap.

## Finite full-black helpers and visible return

**Confirmed (pinned source):** `ExplorationLoop` calls `FadeOutToBlackAll`,
waits for that fade, takes the Preserve/Rebuild load branch, installs
`SetBaseVIntFunctions`, runs the selected map init, then compares the **LONG**
at `PALETTE_1_BASE_02` (`FFD084`) with `PALETTE_1_CURRENT_02` (`FFD004`).
The comparison includes palette 1 colors **2 and 3**. Inequality calls
`PlayMapMusic` and synchronous `FadeInFromBlack`; equality skips both.
This is not an unconditional fade-in or a comparison of only color 2.

`FadeOutToBlackAll` resets the pointer, timer and full palette mask and copies
live `FADING_COUNTER_MAX` (`FFDEF2`) into the countdown. It deactivates scrolling,
not the entity service. `WaitForFadeToFinish` adds no final service after the
setting becomes zero. `fadingcommands.asm:ExecuteFading` performs the same
initialization but waits **one additional VInt** after fade termination.
`applyfadingeffectandz80busupdate.asm:ApplyFadingEffect` decrements the countdown;
at zero it reloads the period and consumes a table entry. The full-black tables
in `data/tech/fadingdata.asm` contain seven color entries and an `80` terminator:
`FA FB FC FD FE FF 00 80` for in and `FF FE FD FC FB FA F9 80` for out.
`ApplyCurrentColorFadingValue` derives each channel from BASE plus twice the
signed entry, clamped to the channel range. The terminator changes no color.

For a stable positive byte period `p`, the fade therefore consumes `8*p`
**enabled fade services**; ExecuteFading adds its separate final service.
`vint.asm` orders fade before context slots, including the terminal cycle.
Period zero wraps the byte countdown to 256; the bounded remake explicitly
rejects it. This finite helper rule does not count interruptions during CPU work.

`displayinit.asm:InitializeDisplay` deactivates context slots before its VInt
wait. `mapload.asm:LoadMap` also waits after enabling the display, before the
caller reinstalls base context functions. Those two explicit gated services
update no entities. Preserve retains live BASE and the old physical population;
Rebuild selects the map palette for BASE and initializes entities, clears its
specified temporary flags and sets flag 80. CURRENT remains black until a
supported palette operation changes it. The source has additional CPU phases
before/after these operations whose interruption counts are **Unknown**.

### Selected R1 period and initial display binding

**Confirmed (bounded source plus retained RAM join):** the selected R1 ancestry
uses period **3** at the first warp. The retained research-worktree record
`local/issue496/prepared-68/runtime/continuation.json` contains
`original.ramHex`, a 65,536-byte 68K RAM readback. Offset `DEF2` is 3;
`DEF0` is 0; `F711` is map 3. It is neutral completed emulator frame 8605,
checkpoint `map3-zone-messenger`, ordinal 1, with no active script/helper.
It is a later saved observation, **not a direct first-warp sample**.
The 2,626 retained checkpoints begin at `r1:first-wait-before-restoration`,
include the first warp at frame 368 and end at segment save; their maps remain 3.

The writer audit at the pinned commit is bounded and reproducible:

| Source below `disasm/code/` | Period write |
| --- | --- |
| `gameflow/start/systeminit.asm`, `gameflow/start/gameintro.asm`, `specialscreens/witchend/witchend.asm` | Set 3 in reset/intro/end contexts |
| `common/menus/endingkiss.asm` | Set 5 in ending context |
| `gameflow/battle/battlescenes/battlesceneengine_0.asm` | Temporary 1, restore predecessor |
| `common/scripting/map/mapscriptengine_1.asm:csc3B/csc3C` | Temporary 6, restore predecessor |
| Same file, `LaunchFading` | Supplied temporary period, restore predecessor |

The selected R1-to-messenger suffix enters none of the permanent writer contexts.
Its ordinary fades and default Map 3 init do not write the period; synchronous
map helpers restore temporary changes. Numeric `FFDEF2` aliases and wider fading
stores add no writer (`FADING_TIMER_WORD` is `FFDFAA`, not the period).
`tools/bizhawk/map3_messenger_acceptance_observer.lua:original_state` reads every
68K RAM offset in order, and `save_segment` writes the record before stop/cleanup.
R1 patch/scratch/time restoration does not touch DEF2, and this segment was not
resumed. Thus the later byte and intervening writer invariant join back to R1
and the first warp. **Inferred:** cold boot is its likely origin. **Unknown:**
arbitrary other starts. Production consumes an explicit period; it reads no
checkpoint and has no default 3.

The initial palette/visibility has a separate proof. Map 3 `00-tilesets.asm`
selects palette 0; `mappalette00.bin` words at byte offsets 4 and 6 are nonzero.
The admitted R1 prefix reaches `ExplorationLoop`'s first `WaitForEvent` after
full FadeOut, new-map BASE load, default init (flags 1/602/603 only hide/position
entities), and the conditional synchronous FadeIn. CURRENT zero cannot equal
this nonzero BASE, so that prefix returns with BASE restored. The later saved
RAM's BASE/CURRENT pair equality corroborates this state; it does not substitute
for the initial-prefix proof. This binds the selected visible R1 entry only.

Read-only reproduction after selecting the retained record in `$continuationPath`
and the clean pinned source checkout in `$upstream`:

```powershell
$record = Get-Content -LiteralPath $continuationPath -Raw | ConvertFrom-Json
$record.original.ramHex.Length / 2 # 65536
[Convert]::ToInt32($record.original.ramHex.Substring(0xDEF2 * 2, 2), 16) # 3
$record.checkpoint.name # map3-zone-messenger
& rg -n 'FADING_COUNTER_MAX|FFDEF2|FADING_TIMER_WORD' (Join-Path $upstream 'disasm')
& git -C $upstream show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/common/tech/interrupts/fadingcommands.asm
```

**Confirmed (bounded implementation contract):** logical palette state and actual
host delivery are separate. The source pair comparison selects the helper;
pair equality alone cannot establish RGBA host visibility. The supported equal
branch is an onLoad synchronous full-black FadeIn that has completed both its
logical services and actual visible delivery. Equal-but-black returns are
explicitly Unsupported. White, partial, tint and asynchronous map-load composition
remain Unsupported in this transition context. No implicit white restore is
licensed by this source evidence. The [verification owner](../../remake/docs/development-and-verification.md#ordinary-warp-visible-return)
records engine and real host acceptance without claiming hardware timing or H4
closure. Original-emulator evidence was read, not recaptured.

## Required bridge through area 1

The accepted natural route supplies the bridge between the two warps:

1. The forced slope step `(3,3) -> (4,4)` admits `Map3_ZoneEvent6`. With flag
   601 clear it runs `cs_5145C`, performs the bounded entity-128 interception,
   hands that entity to ambient walking, and sets flag 601. It does not
   relocate the player or select a setup/layout variant.
2. The Bowie-house door at `(4,8)` and school door at `(41,13)` are step-event
   block copies. They mutate their door cells; neither is a warp.
3. Entity Sarah initially blocks the school route at `(42,8)`. Interaction
   from `(42,9)`, facing `UP`, reaches `Map3_EntityEvent0`; with flags 603,
   602, and temporary 256 clear, it runs `cs_513D6`, moves Sarah to `(41,7)`,
   and sets temporary flag 256.
4. Movement can then reach `(45,7)` and target the school warp at `(46,7)`.

Flags 601 and 256 describe mandatory events on this accepted route, but they
do not guard either warp record. The only setup-selection predicates remain
flags 609, 506, and 543; none is set by this bridge.

## Zone 601 interception lifecycle

The default Map 3 zone table row is zero-based 6 / one-based 7 at ROM
`0x50D64`, with bytes `04 04 00 F8`. Its `(4,4)` key and table-relative
`0x00F8` target select `Map3_ZoneEvent6` at `0x50E44`. On the retained route,
the post-warp player is at `(3,3)` and the next `Right` supplies `(4,4)` as
the raw target in `MAP_EVENT_PARAM_1` and `MAP_EVENT_PARAM_3`. The caller chain
is `ProcessMapEventType6_ZoneEvent` (`0x25A7C`) ->
`RunMapSetupZoneEvent` (`0x4751A`) -> the matching row ->
`Map3_ZoneEvent6`. The first caller applies `eas_Init` to controlled entity 0;
the table consumer compares the raw target coordinates and calls the row
target synchronously.

The interception actor is Map 3 entity-source row zero-based 2 / one-based 3
at `0x50B40`, bytes `05 06 00 C3 00 04 61 02`: the woman at `(5,6)`, facing
`RIGHT`, initialized by `eas_InitSlow`. In this accepted opening,
`InitializeMapEntities` assigns Sarah and Chester's preceding ally rows to
physical slots 1 and 2 and this first non-ally row to physical slot 3. Its
logical ID is 128 (`0x80`): `GetEntityIndexForCombatant` subtracts the enemy
index difference `0x60`, then `ENTITY_INDEX_LIST[0x20]` resolves to slot 3.
The current-map house warp does not rebuild entities. `eas_InitSlow` reaches
its idle loop without changing the source position, so this accepted static
state remains `(5,6)` before Zone 6.

With flag 601 clear, the exact order is:

1. `cs_5145C` assigns `eas_Init` to logical entity 128 and waits until the
   physical entity returns to `eas_Idle`.
2. A waiting entity-action sequence executes `moveUp 2`, which installs the
   relative destination `(5,4)` and waits for arrival, followed by
   `faceLeft 20`, which sets facing `LEFT` and carries the encoded wait operand
   20. The cutscene script does not advance until the sequence returns to
   `eas_Idle`.
3. The script presents text IDs 510 and 511 with entity 128, then resets the
   cursor and presents single-text ID 483. Each map-script text command calls
   `DisplayText` synchronously; all three calls return before the next entity
   assignment or any flag write. This establishes script gating, not exact
   rendered-text timing.
4. The script assigns `eas_Init` to entity 128 once more, waits for idle, and
   returns to the zone handler. At this boundary the deterministic cutscene
   displacement is `(5,6) -> (5,4)` and the facing is `LEFT`.
5. The handler calls `MakeEntityWalk` (`0x47808`) with raw selector `0x80` and
   operands `(5,6,1)`. After resolving slot 3, `SetWalkingActscript`
   (`0x44CD0`) copies `eas_Walking` and replaces its center-X, center-Y, and
   range operands with those values. This is an immediate behavior handoff,
   not a synchronous command to walk back to `(5,6)`: after its initial wait,
   the entity repeatedly chooses a random one-tile move subject to the
   center/range bounds, collision, and entity-destination checks.
6. Only after that handoff does the handler set flag 601 and return.
   `RunMapSetupZoneEvent` closes the presentation, waits one VInt, and calls
   `WaitForEntityToStopMoving` (`0x44DA4`) with `D0=0`; it waits for the
   controlled player, not slot 3. Ambient entity-128 walking may therefore
   continue after route control resumes.

Flag 601 is read once before all effects, written once after all blocking
movement and dialogue plus the nonblocking walking handoff, and is not cleared
anywhere in the pinned source. Re-entering `(4,4)` with it set returns directly
without replaying the script or replacing the walking behavior. It is outside
the temporary range cleared on a true new-map load, so both current-map and
new-map exploration entries retain it until a broader game-state reset. The
accepted H3 trace confirms the first natural Zone 6 and `cs_5145C` entries and
later route progress; the exact subsequent random choices and terminal
position of entity 128 are deliberately **Unknown**. The mandatory route fact
is the synchronous Zone 6 lifecycle and return, not a later warp predicate:
neither accepted warp row reads flag 601.

## Sarah interaction and temporary flag 256

Sarah's entity-source row is zero-based 0 / one-based 1 at `0x50B30`, bytes
`2A 08 03 01 00 04 60 CE`, placing logical ally 1 at `(42,8)`, facing `DOWN`,
in physical slot 1. Her entity-event row is zero-based 0 / one-based 1 at
`0x50F10`, bytes `01 03 00 44`; it selects logical ally 1,
`Map3_EntityEvent0` at `0x50F54`, and event-facing control `DOWN` (`3`).

The retained input is `C` at player `(42,9)`, facing `UP`. The accepted H3
chronology is `ProcessPlayerAction` -> `GetActivatedEntity` ->
`RunMapSetupEntityEvent` (`0x4761A`) -> `Map3_EntityEvent0` -> `cs_513D6`.
The event dispatcher resolves `ENTITY_INDEX_LIST[1]` to physical slot 1.
Because both low bits of the row's facing-control byte are set, it first turns
Sarah opposite the player's `UP` facing (`DOWN`) and, after the handler
returns, restores her original activated-entity facing (`DOWN`).

With flags 603, 602, and 256 initially clear, the handler order is exact:

1. read 603 and take its clear branch;
2. read 602 and take its clear branch;
3. read 256 and, because it is clear, present text ID 512;
4. present text IDs 480 and 481;
5. read 256 a second time and take its clear branch;
6. run `cs_513D6`, whose waiting action sequence moves Sarah left one tile
   `(42,8) -> (41,8)`, then up one tile `(41,8) -> (41,7)`, waiting for each
   destination and for the sequence to return to idle; the last move leaves
   the action-script facing `UP`;
7. after the script returns, set temporary flag 256 and return; then the event
   dispatcher restores Sarah's facing to `DOWN`, closes the portrait/text
   presentation, and reactivates entity updates.

Thus all three text commands gate Sarah's movement, movement completion gates
the flag-256 write, and the dispatcher cleanup/facing restoration gates return
to exploration. The accepted H3 observer sees the complete action/event/script
chronology and, when flag 256 marks the waypoint complete, reads physical slot
1 at `(41,7)`. It does not provide a rendered-text, per-frame movement, or
post-cleanup facing observation; those ordering and facing facts are static
source/H1/ROM results.

While flags 603 and 602 remain clear, a same-load re-interaction with flag 256
set skips text 512, still presents 480 and 481, then skips `cs_513D6` and the
redundant flag write. Flag 256 has no local clear in this handler. The
current-map `0xFF` stair warps take `ExplorationLoop`'s map-index-not-provided
branch, preserving live entities and skipping `ClearMapSetupTempFlags`; the
retained H3 endpoint confirms flag 256 remains set across the accepted
same-map warp chain. By contrast, a map-index-provided exploration entry
reinitializes entities and then `ClearMapSetupTempFlags` clears all 128 flags
from 256 through 383 before the selected map init runs. A map-index-provided
entry that selects this same default Map 3 entity setup would therefore place
Sarah back at `(42,8)` and clear 256; no bounded H3 re-entry case has observed
the replay. If 602 is set, Sarah instead uses text 502; if 603 is set, the
handler reads follower flag 66 and may run `cs_513E2`. Those later story
branches are outside this route-lifecycle slice.

## Minimum faithful runtime state and deferred presentation

A truthful implementation of this bridge needs the candidate-target event
ordering, default Map 3 setup, flags 601/603/602/256 and the temporary-flag
reset boundary, logical actor identity, live actor position/facing/behavior,
and blocking text/action/script order. It must preserve current-map entity and
temporary-flag state across the two `MAP_CURRENT` warps. It need not reproduce
the original physical slot numbers internally, but it must not confuse raw
logical selector `0x80` with physical slot 3, or Sarah's logical ID 1 with a
newly allocated NPC ID.

The exact ambient random-walk choices, interpolation and frame counts,
textbox rendering and advance timing, portraits, camera, audio, alternate
Map 3 setup variants, downstream 602/603 branches, save/power-cycle
persistence, and global shortest-route uniqueness remain deferred. None is
promoted by the retained natural-route H3 observation.

## Second warp: area 1 to the entity 142 region

Targeting `(46,7)` follows the same request and no-scroll handler chronology,
using warp row 5. The destination is current Map 3 `(59,12)`, facing `LEFT`.
The current-map reload again preserves the working base layout, selects area
ordinal 3, retains temporary flag 256 because this path skips the new-map
entity/temp-flag initialization block, and reruns the same default
`ms_map3_InitFunction`. At this point flags 1, 602, and 603 are clear, so that
init requests no script; no Map 3 roof row contains `(59,12)`, so this landing
does not add a roof block-copy effect.

**Confirmed (bounded H3):** the retained trace records
`map-event:warp:map3-school-stairs-down`, `exploration:3`, the same Map 3 init,
and the next exploration wait. From `(59,12)`, `Left` follows the slope to the
area-3 zone at `(58,13)`, after which accepted controller input reaches
`(55,17)`, faces `LEFT`, and dispatches logical entity 142 at `(54,17)` to
`Map3_EntityEvent15`. The independent entity-142 H2 owner binds that logical
entity to source record 17 / physical slot 17 under this accepted route.

The first warp is therefore sufficient to escape the controlled pocket and
activate area ordinal 1. It is not sufficient to reach entity 142's disjoint
area ordinal 3; the school warp is the second required transition in the
accepted chain.

## Evidence labels and retained Unknowns

| Claim | Classification | Owner |
| --- | --- | --- |
| record widths/order, first-match scan, target-marker-before-passability order, current-map reload policy, area scan, and roof-on-load scan | **Confirmed static source/H1/ROM** | `sf2-map-content-static-v1` |
| `(56,3)`, facing 3, Map 3/default setup/init, clear selector flags, first wait | **Confirmed bounded H3** | `sf2-map3-admitted-start-runtime-v1` |
| Zone 601 and Sarah record identities, logical-to-physical actor resolution, exact handler/script/action/text order, flag writes, facing restoration, and temporary-flag clear range | **Confirmed static source/H1/ROM** | pinned `SF2DISASM` source plus the ROM rows named above |
| both warp requests, reload/init/wait chronology, Zone 6 and Sarah event/script entry order, Sarah slot-1 waypoint `(41,7)`, flag-256 same-map continuity, area-3 route, and entity 142 dispatch | **Confirmed bounded H3** | `sf2-map3-battle01-natural-route-runtime-v1` |
| entity 142 source/event identity and physical-slot relation | **Confirmed static under the accepted route state** | `sf2-map3-entity142-interactable-reference-static-v1` |
| globally shortest input route or uniqueness under every possible entity/flag state | **Unknown** | not required by this bounded accepted chain |
| exact fade frames, roof appearance, camera transition frames, audio timing, and other rendered presentation | **Unknown** | future admitted presentation evidence |
| behavior when selector flags 609, 506, or 543 choose another Map 3 setup | **Unknown for this route** | outside the controlled default-setup boundary |

Reproduce the static owners with `uv run sf2 h2 map-content` and
`uv run sf2 h2 map3-entity142-interactable-reference`. The retained runtime
owners are reproduced by `uv run sf2 h3 map3-admitted-start --timeout-seconds
180` and `uv run sf2 h3 map3-battle01-natural-route --timeout-seconds 180`.
This slice reuses their accepted observations and does not claim a new H3 run.
