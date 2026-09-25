# Map 3 Controlled-Start Egress Transition

## Scope and result

This owner closes the transition gap between the controlled Map 3 start at
`(56,3)` and the already accepted natural-route interaction with entity 142.
It joins source/H1/ROM and bounded H3 evidence. The first-return comparison below
also records the separately admitted native diagnostic; the two-warp route is unchanged.

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

## First-return comparison boundary

**Confirmed (existing-record readback):** original
`local/issue496/prepared-68/runtime/actual-inputs.jsonl` lines711/773/1015 request
Left30, neutral120, then Right2. Checkpoints line20 records map init return at
frame405/order832 with seed1091; lines22/23 record the later Right read/acceptance
at frame506 with75DA. The final neutral reply at actual-inputs line1014 is frame505
with fading0 and the latest field poll at505. The original collector retained no
first post-fade field poll within that batch.

The accepted remake finite-warp observation
`local/issue534/warp-transition/final-private/observation.json` records init/FadeIn
entry at `warpRecords[56]` (zero-based), tick40/1091, and first logical+actual visible
field return at record82, tick65/F01B. That probe submits two separate Left moves
and stops at visible return, followed by one explicit Wait. Thus its endpoint and
the historical frame506 comparison are different input/consumer boundaries. Equal
init seeds alone do not establish equal NPC action/motion/timer or CPU phase.

**Confirmed (admitted first-return observation):** the
[native result and reproducible readback](map3-messenger-acceptance.md#first-warp-field-return-native-result-issue-534)
now retain the first original post-warp/init fade-clear poll at observer432 /
emulator431, PC `0x2593C`, order980, seedF01B. Completed432 still hasF01B; the
later completed505 endpoint has75DA. The R1 source state and complete inherited
entity/status/index record matched prepared68 before input. The seed difference at
the historical later boundary can therefore occur during subsequent neutral field
waiting; the new first-return seed equals the accepted remake visible-return seed.
This does not establish equality of entity or CPU phase.

In the historical comparison of original first-return physical slots0..19 with
PR564 `warpRecords[82]` at tick65, the source-equivalent fields `x`, `y`, `targetX`,
`targetY`, facing low two bits, sprite, speedX, flagsA and flagsB differ only as follows:

| Physical slot | Field | Original first callback | Remake visible return |
| --- | --- | --- | --- |
| 0 (player) | flagsA | `EF` | `E0` |
| 5 | Y fixed-point | 5397 | 5386 |
| 8 | Y fixed-point | 3117 | 3100 |

These are raw differences; flagsA is not masked away. Other listed fields match
across those20 slots. This comparison is limited to fields actually present in
both observations. Source `disasm/sf2enums.asm:1200..1233` at the pinned revision
owns the32-byte record: signed16 X/Y at0/2, velocities4/6, travel8/10,
destinations12/14, facing16, sprite19, action pointer20..23, acceleration24/25,
speed26/27, flags28/29, animation counter30 and script wait31.
The original slot5 has velocity(0,6), travel(0,384), action pointer `0xFF5628`,
animation6/wait0; slot8 has(0,9), (0,384), `0xFF568C`, animation9/wait0.
All48 original records and64 index bytes are retained at first callback and endpoint.
The historical PR564 receipt omits velocities/travel, speedY, animation and wait
counters. The [phase observation below](#first-warp-entity-phase-readback) supplies
these fields. An engine action cursor remains an instruction index, not an original
action address.

**Unknown at that result boundary:** why the available NPC positions and player flags differ, full
motion/action/timer/CPU phase agreement, subsequent eligible entity-service count,
and any earlier input/transition interruption difference. The73 later completed
frames from432 to505 are not evidence of73 or74 entity services, and callback432
is distinct from completed432. No per-VInt entity-service count was captured.
The120 neutral frames include transition work and later field waiting; they do not
establish120 `WaitAtInput` operations. No historical seed, tick65, neutral count or
inverted RNG sequence is an engine rule or replacement golden. No production rule
is changed by this result. The later phase readback resolves part of the missing
projection; #534/#437 and whole-route/H4 acceptance remain open.

## First-warp entity phase readback

**Confirmed (actual remake observation plus retained original readback):** the
observation-only adapter change on accepted base
`5232a1762efcdba384bb4dcf735dd13fa634573d` serializes existing authoritative Motion
fields without changing their values or service order. Private normal/reduced
runs in `local/issue534/warp-phase-observation/{normal,reduced}/observation.json`
use the existing first-warp probe, owned Godot 4.7.2 project/installation, accepted
world/party/assets and unchanged PR564 display binding. Both exit 0, pass with no
failures/unavailable, and preserve identical 97 ordered events and selected gameplay
fields at all 85 result records. Entry, load and visible-return state are read from
the running instance. The [verification owner](../../remake/docs/development-and-verification.md#first-warp-motion-phase-observation)
owns the launch/readback commands; no original emulator was launched.

The comparison reads accepted PR567 `checkpoints.jsonl:2` for inherited R1 entities
and `observer.observed.json#/diagnostic/firstReturn` for the first fade-clear callback.
The latter is observer 432/emulator 431, not its completed-frame state or endpoint505.
Each source record uses the pinned `sf2enums.asm` offsets above. X/Y, destinations and
velocities are signed 16-bit; travel is decoded as unsigned 16-bit; speed,
acceleration, layer, flags, facing, animation and wait are individual bytes.
The engine widens speeds to ushort; this capture's values remain byte-representable.
Word arithmetic wraps in `UpdateEntityData`; acceleration is zero-extended before
word addition/subtraction. The source animation reset uses signed byte comparison
with30; `esc00_wait` also uses a byte comparison. This readback makes no assertion
about unobserved high-bit timer values. No flags or velocity differences are masked.

**Confirmed earliest available boundary:** at R1, physical slots 5, 6 and 8 match
all 19 common fields (position/destination, velocity/travel, speeds/accelerations,
facing/layer/sprite, flags, animation and wait). Other slots already differ: the
player has speed0/0 versus32/32, animation26 versus1 and wait1 versus0. Idle
slots1/2/4/7/9..19 have flagsA80 versusE0; most also have wait1 versus their allocated
slot number. Slot3 has speed16/16, acceleration1/1, flagsA8F, wait1 versus32/32,
0/0, E0,3. These are entry differences, not evidence of a fade-produced discrepancy.
`map3-opening-start.json` binds continuation phases for the three walking slots;
`SceneEntities.Build` initializes the others with allocation defaults. This boundary
is therefore only a partial original entity-phase binding.

At first visible return, the following additional fields refine the PR567 result
(original/remake notation; coordinates remain fixed-point):

| Slot | Differing phase fields | Matching relevant fields |
| --- | --- | --- |
| 0 | flagsA EF/E0; animation22/25; wait1/0 | XY/destination, velocities/travel, speed32/32, acceleration0/0 |
| 3 | animation29/4; wait1/0 | position, speed16/16, acceleration1/1, flagsA8F |
| 5 | Y5397/5386; Y velocity6/4 | travelY384, acceleration1/1, flagsAEF, animation6, wait0 |
| 6 | wait7/5 | XY/destination, velocity(4,0), travel(0,0), flagsAEF, animation7 |
| 8 | Y3117/3100; Y velocity9/7 | travelY384, acceleration1/1, flagsAEF, animation9, wait0 |
| 1/2/4/7/9..19 | wait1/0 | all other common fields |

There are25 differing slot/field pairs at return across the19-field/20-slot comparison.
The normal run's zero-based result30 starts FadeOut at tick14/C632; result57 completes
load/onLoad and starts FadeIn at tick40/1091; result83 is logical+actual visible return
at tick65/F01B. The next explicit input sample is tick66/F01B. These record indices
are receipt locations, not engine boundaries or timing rules. The original capture
has no full entity array at pre-warp or init-return, so the first **observed** new
walking-slot discrepancy lies between R1 and first return; its earliest CPU/service
point remains **Unknown**. The related velocity and timer deltas do not authorize
adding two entity services or converting original frames into engine ticks.

### Idle, controlled-character and motion ordering audit

**Confirmed (pinned source and the PR568 engine structure):**

- `entityfunctions_2.asm:MakeEntityIdle` (`0x44BEC`) only writes action pointer
  `eas_Idle` (`0x451FC`). It changes no flags, velocity, travel or timer.
  `data/scripting/entity/eas_main.asm:eas_Idle` is `ac_wait 1` plus a branch back.
  `esc00_wait` increments/yields or clears the timer and continues; the branch
  returns to that wait. Null engine Actions is not this raw script phase.
- `eas_ControlledCharacter` (`0x44E3E`) sets speed32/32, acceleration factors0/0
  and enables both acceleration/deceleration axes. `esc12`/`esc13` set flagsA
  bits0..3; with the existing high bits this explains EF. Zero acceleration factors
  make those bits inert for this player's current trajectory, but they remain real
  state. `MakeEntityIdle` does not explain or clear the low nibble.
- `SceneEntities.Build` starts player flagsA atE0. `MapEventDispatcher.Move`
  changes facing and installs movement; it does not execute that source controlled
  setup. `MapTransfer.BeginWarp`/Preserve retain Motion and clear Actions. The
  first-return source pointer is `0x44E3E`, while EF already remains from earlier
  control. Copying EF at the return boundary would bypass the missing setup rule.
- `src/sf2tool/remake_exploration_content.py:action_stream` explicitly truncates
  `ac_jump eas_Idle`; `ExplorationContentReader.Actions` appends StopEntityActions,
  and `EntityActionRunner` retires Actions. This accounts for an absent ongoing
  idle timer in that projection; it cannot explain the active walking slots' gap.
- `VInt_UpdateEntities` calls `UpdateEntityData` then that slot's action script
  before the next physical slot. `EntityActionRunner.Tick` uses the same relative
  order. Movement installs velocity/travel after that slot's movement pass; wait
  completion can execute subsequent script commands in the same service. The
  observed signed velocities and travel widths agree with their source encoding.

These findings identify actual phase-model differences, not a missing serializer
alone. They do not establish a wrong NPC acceleration formula or identify a missing
mandatory warp service. **Unknown:** the complete interruption schedule, walking
slot divergence point, and equivalence of raw action pointers to engine continuations.
Production correction requires its own scope and behavior acceptance; no seed,
coordinate, timer, flags or original input was repaired in this observation slice.

### Ordinary source-population control handoff

**Confirmed (pinned source):** `disasm/code/gameflow/exploration/explorationfunctions_2.asm:WaitForEvent`
calls `SetControlledEntityActScript` only when no map event remains, before the field
input poll at `0x2593C`. In `disasm/code/common/scripting/entity/entityfunctions_2.asm`,
that setter installs `eas_ControlledCharacter` for ordinary on-foot control. Its
prelude in `disasm/data/scripting/entity/eas_main.asm` sets speed32/32, acceleration0/0,
entity/map collision and both acceleration/deceleration axes. The corresponding
`esc12`/`esc13` and `esc18`/`esc19`/`esc1A` setters in `entityscriptengine_2.asm`
establish flagsA `(prior & 0x10) | 0xEF` and clear the script timer. The pointer
handoff itself does not integrate or clear physical motion.

The remake applies this rule to source populations at successful ordinary field
return: player Actions, Follower and WaitingForMotion are superseded, while Motion
is retained. The next existing player service integrates movement, establishes the
controlled settings, then handles input before later physical slots. Neutral field
services maintain the same idempotent settings. Supported field-owned continuations
do not independently author them; program/dialogue/init/load/fade ownership excludes
this setup. Authored populations retain their configured behavior. This is a logical
control invariant, not a claim that the original reruns the prelude every VInt.

Move preview uses the same controlled collision policy without publishing settings
or advancing time. A blocked move commits only facing; a pending move or rejected
warp binding preserves Motion until the actual service. The behavior tests and
[verification receipt](../../remake/docs/development-and-verification.md#ordinary-source-population-control-handoff)
cover custom player slots, nondefault settings, retained travel, ordered NPC/RNG work,
player-only retirement, other owners and preview admission.

**Confirmed (one actual normal first-warp observation):** compared with PR568,
the initial sample is unchanged. At first-move completion, visible return and next
explicit Wait, the only entity-field change is player flagsA E0 to EF. Ordered
gameplay events, ticks, RNG and other selected state match; raw sprite-ready delivery
and receipt grouping differ and remain retained. Visible return is still tick65/F01B.
The original first-return comparison now has24 differing slot/field pairs: the
historical table above loses only player flagsA. NPC Y/velocity/timer differences,
player/slot3 animation, idle timers and the partial initial binding remain unresolved.
No new original observation or entity service is added.

**Confirmed separate gaps excluded from the control-handoff correction:** direct `MakeEntityIdle`
and `SetWalkingActscript` preserve the incoming timer, while `esc34_jump` clears it
before entering idle and the `eas_Walking` prelude begins with wait30. Map3 Zone6's
`cs_5145C` ends with init then `MakeEntityWalk`, so idle timer carryover can affect
that leading wait. `mapscriptengine_1.asm` callers `csc2D`/`csc14`/`csc15` initialize
the timer from the resolved physical slot; `csc2D` also clears flagsA bits5/6. The
common StartEntityMotion path at that boundary did not distinguish these caller semantics.
Simply adding a literal idle loop would keep an active action owner busy and
block field input. The bounded idle/caller correction below addresses this consumer.
**Unknown:**
the active-NPC divergence point and full CPU/service phase agreement remain as above;
this control correction does not close JOIN, HEAL, H4 or the whole route.

### Source idle completion and caller installation

**Confirmed (pinned source):** `GetEntityAddressFromCharacter` in
`code/common/scripting/map/mapscriptengine_1.asm` resolves the logical selector through
the identity table and returns the physical slot in d0. `csc2D_entityActionSequence`
writes that slot to the wait timer and applies flagsA `& 0x9F`; `csc14_setEntityActscriptManual`
and `csc15_setEntityActscript` write the same slot timer without that flags change.
Their synchronous waits compare the script pointer with `eas_Idle`, not the physical
destination. `eas_Init` has no wait-destination or motion-cancellation operation, so
script-idle completion can precede physical arrival. The aliases are defined in
`sf2cutscenemacros.asm:csc14/csc15/csc2D`.

`esc34_jump` clears the timer through `esc_clearTimerGoToNextCommand` and dispatches
the idle wait in the same entity service. `eas_Idle` is wait1/branch; for ordinary
nonnegative timer values one service leaves timer1. Direct `MakeEntityIdle` and
`SetWalkingActscript` installations preserve the incoming timer until service.
These are distinct transitions, not a universal timer0/1 normalization. The source
wait uses signed byte comparison; full high-bit wait fidelity remains outside this
bounded leading-wait consumer claim.

The source-backed content now distinguishes Preserve, SlotTimer and
SlotTimerClearCollision installation. Source jump-to-idle and csc2D's implicit tail
use a jump followed by a terminal idle action; authored Stop, Unsupported and plain
stream exhaustion do not become source idle. The terminal uses the existing action
cursor and wait timer, yields in the existing service and contributes no busy action
ownership. Physical movement still contributes Busy. A source cutscene wait explicitly
requires script idle; ordinary field movement still requires arrival. The accepted
ordinary-control setup supersedes the player's terminal action and remains unchanged.

The real consumer is `Map3_ZoneEvent6` → `cs_5145C` → final `eas_Init` →
`MakeEntityWalk(128,5,6,1)` → leading wait30. Other valid callers include
`cs_5149A`'s custom speed48 script followed by entityActions, and the leading eaWait
sequences in Map19 `scripts.asm:147` and Map20 `scripts_1.asm:96`. Those latter source
blocks demonstrate slot-dependent waits; they are not claimed naturally reached by
the selected observation. Slot3/7 and alias variations are exercised by engine behavior
tests rather than baking selector128 or one route's timing into execution.

**Confirmed (bounded actual remake observation):** the existing ordinary-input probe
executes only Left/Left/Right and three real dialogue confirmations. At Zone6 field
return, flag601 is set, the player is at(4,4) with usable control and flagsAEF, and
entity128/slot3 is at(5,4) in walking cursor0/wait30 with timer1. Its Y velocity-32 and
zero remaining Y travel are retained. The run exits0 without session failures or
Godot errors; tick142/seed75DA are observed receipt values, not expected quotas or
an original-phase equivalence claim. The [verification owner](../../remake/docs/development-and-verification.md#source-idle-and-caller-verification)
records the fresh content, three-way producer comparison and exact readback.

**Unknown:** this does not identify the earlier first-warp active-NPC divergence point,
repair the partial initial binding or establish CPU interruption/service equivalence.
Direct warp-player idle counters without a later timer consumer, raw pointer equality,
unimplemented native/waitIdle producers and idle animation parity are not added scope.
All prior failures, nonruns and original acquisition totals remain unchanged.

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
