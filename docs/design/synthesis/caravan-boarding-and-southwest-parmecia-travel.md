# Caravan Boarding and Southwest Parmecia Travel

Acquiring the Caravan prepares a new overworld follower; entering it and controlling it are separate
operations. Walking can request a boarding sequence that shrinks Bowie and selected followers, then
changes the controller to mode 1. That controller can move, request a warp or request landing. Landing
grows the walking actors and reinstalls follower actions before returning to mode 0. A later walking
zone can request Battle19; the Caravan controller does not produce that zone event.

**Confirmed:** the accepted [research owner](../../research/caravan-boarding-and-southwest-parmecia-travel.md)
establishes these selected static producers, complete boarding/landing programs and immediate
consumers. **Inferred:** together they explain entering, travelling in and leaving the Caravan.
**Unknown:** a naturally traversed route, actual supplied state, completed presentation/services,
responsive control and save/resume persistence. Source order does not establish composed runtime success.

Read [Battle18 outcomes and acquisition](battle18-outcomes-and-caravan-acquisition.md) for the preceding
F65/F84 changes and conditional shrine return, and [Taros entry](southwest-parmecia-through-taros-battle18-entry.md)
for the independent warning/admission. [Rick recruitment](hassan-rick-recruitment.md) explains optional
current following, not a mandatory detour. The [raft guide](polca-raft-boarding-through-south-parmecia-entry.md)
keeps its different mode and saved-position lifecycle. This guide stops at conditional walking control
after landing or MainLoop's Battle19 handoff, before opening, placement, turns or outcomes. C-button
menu admission ends at the menu call; transactions, storage, village content and later travel are excluded.
Continue with [Battle19 outcomes and independent Elven Village entry](battle19-outcomes-through-elven-village-entry.md)
for the separately accepted outcome/arrival boundary; this guide retains its admission stop.
Fnnn means flag nnn. Tile coordinates and waits are source operands, not observed positions or durations.

## Leaving the Shrine Before Fresh Overworld Population

**Confirmed:** the selected Map12 south-exit row matches any X at Y 31 and requests no-scroll Map68
`(55,44)`, Down. The acquisition scene set F65 and cleared F84, but did not write PLAYER_TYPE or request
boarding. Neither the row nor that scene establishes a walk to the exit or an accessible boarding tile.

`WarpIfSetAtPoint` scans ordered rows using coordinates divided by 384 and negative coordinate bytes
as wildcards. A match writes event 1, its scroll/map/coordinate/facing parameters and WARP_SFX.
`ProcessMapEvent` clears pending before dispatch. The no-scroll consumer submits/clears the sound,
idles physical 0, unwinds exploration, samples current position and substitutes nonnegative requested
coordinates plus requested facing. It returns `(68,55,44,Down,d4=0)` without clearing transport mode.

MainLoop calls SwitchMap and CheckBattle **before** fresh exploration. The complete switched-map table
has no Map68 replacement; the sole Map68 battle-coordinate row is 19, area `(5,19)`, size 48x32, with
wildcard trigger bytes `(255,255)`. F419 must be set, but neither trigger requires containment in that
area. F519 set clears F419 after selection and still admits Battle19 for that call. F65, F518, F725,
Sword custody and transport mode add no admission gate. If F419 was independently supplied, stop at
this BattleLoop handoff rather than claim fresh Caravan population happened first.

Without that admission, supplied-map ExplorationLoop clears pending, subtracts 20000 from the signed
word STEP_COUNTER with a negative-result clamp, heals living/immortal allies, fades/loads, writes
CURRENT_MAP 68/CURRENT_BATTLE 255, rebuilds entities, clears temporary flags 256–383 and sets F80.
Map68 has one setup, an empty fixed-entity list and a return-only init. After conditional music/fade,
pending zero permits WaitForEvent to follow physical 0 and install the supplied PLAYER_TYPE controller.
That installation remains conditional on the preceding services and absence of a new event.

## Population, Logical Names and Physical Slots

**Confirmed:** fresh initialization clears entities/lookups, allocates flag-selected followers from
physical 1, processes fixed rows, declares main 0 separately and terminates EXPLORATION_ENTITIES. Main 0
is not appended to that fresh list. Because Map68 is an overworld, F65 set selects the overworld table:
its first row is Caravan mapsprite 62, logical FOLLOWER_A 30, allocated at physical 1; normal followers follow.
F65 clear instead selects normal followers. Allocation does not test HP.

F65 and transport mode have different consumers. F65 set/mode 0 creates a separate Caravan follower
while retaining walking. F65 clear does not repair an independently supplied mode 1. Fresh mode 1 does
not replay boarding; initialization's special transport handling is for mode 2/raft. First acquisition
cleared Rohde's F84, whereas repeat victory preserves its supplied value. Peter F67, Oddler F68 and
all other follower flags remain independent inputs.

Logical lookup and raw physical operands must stay separate. Later F74/F77/F83 rows can overwrite
FOLLOWER_A's lookup while the Caravan remains physical 1. With only F65/F67/F68, the fresh list is
`[1,2,3,-1]`: Caravan, Peter, FOLLOWER_B. FOLLOWERS_LIST begins one byte after EXPLORATION_ENTITIES,
so its suffix is `[2,3,-1]`; with only F65 it is immediately empty. This skips the first byte, rather
than searching for a logical Caravan. F65-clear or changed flags, missing mappings, excess followers
and a current list extended by Rick need not behave like the fresh example. Fresh tables contain no
Rick row. No selected operation repairs capacity or actor identity.

## Which Numeric Controller and Event Actually Runs

**Confirmed:** the accepted enum/macro/dispatcher/action joins resolve these names at the pinned source:

| Numeric value | Upstream name | Actual consumer |
| --- | --- | --- |
| PLAYER_TYPE 0 | PLAYERTYPE_BOWIE | eas_ControlledCharacter, ac_controlCharacter, opcode 2: walking. |
| PLAYER_TYPE 1 | PLAYERTYPE_CARAVAN | eas_Raft, ac_controlRaft, opcode 7: the mode explained here. |
| PLAYER_TYPE 2 | PLAYERTYPE_RAFT | eas_Caravan, ac_controlCaravan, opcode 8: actual raft. Other values except 0/1 also select this branch. |
| Event 2 | MAP_EVENT_GETINTOCARAVAN | ProcessMapEventType2_GetIntoCaravan → j_MapEventType2 → MapEventType2. |
| Event 4 | MAP_EVENT_GETOUTOFRAFT | ProcessMapEventType4_GetOutOfCaravan → j_MapEventType4 → MapEventType4: actual Caravan landing. |
| Event 5 | MAP_EVENT_GETOUTOFCARAVAN | ProcessMapEventType5_GetOutOfRaft → MapEventType5: actual raft exit. |

Event 1 is warp, event 3 actual raft boarding and event 6 zone dispatch. The GETOUT and vehicle-action
labels disagree with numeric routing; preserve both names and the executed body. `$100FF`, the
source MAP_EVENT_RELOADMAP constant, is a packed longword write, not another dispatcher case.

### Walking Requests Boarding; C Requests a Menu

**Confirmed:** walking opcode 2 uses CURRENT_PLAYER_INPUT for physical 0 with layer 1 bounds reduced
by one tile; other slots use PLAYER_1_INPUT and unshrunk bounds. It considers Up/Down/Left/Right in
order, allowing later eligible inputs to replace axis/facing choices; a horizontal slope branch can
adjust Y. Without an eligible direction, the script advances without movement. With FLAGS_A bit 5 set,
obstruction scans 32 bit 7 actors using both current and destination positions; both axis distances must be below
`$100`. A block skips layout/movement handling but can still reach sprite/facing update.

At the boarding branch, masked working-layout bits `$3C00` equal `$3800`, CURRENT_BATTLE 255 and F65
set write **only pending event word 2**. The branch precedes raw `$C000` blocking and skips destination,
velocity, travel and STEP_COUNTER work. Facing can update only through the sprite/auto-facing/capacity
conditions. It does not test Caravan coordinates, mode, F518/F725, HP, membership or logical mapping.
An obstructing actor can prevent this branch; the Caravan follower action clears its obstruction bit
when that action executes. The later F64/`$3C00` raft branch is separate.

The ordinary C path instead waits for idle/motion/view conditions, retains debug alternatives and
checks **physical 1's** Caravan sprite plus equality of physical 0/1 destination coordinates before
the shrink/menu call sequence ending at `j_CaravanMenu`. That is a different admission from event 2;
a call is not evidence of a menu selection, storage/party operation or completed transaction.

## Boarding: Shrink First, Change Mode Last

**Confirmed:** MapEventType2 idles physical 0, executes the complete `cs_45284`, applies `byte_45316`
to FOLLOWERS_LIST, waits through WaitForFollowersStopped and only then writes PLAYER_TYPE 1.
It has no raft-style alignment gate or recheck of F65/battle state. It creates no actor, joins/heals
nobody and saves no vehicle position. MakeEntityIdle installs an idle pointer without clearing motion.

| cs_45284 record | Ordered action and completion predicate |
| --- | --- |
| 0 | Wait FOLLOWER_A on eas_452A4: request one tile from raw physical 0's current position in physical 0's facing, wait own destination, then idle. |
| 1 | Wait Bowie on eas_452B2: request raw physical 1's current position at distance 0, then enter eas_452BA: wait 6, resize ON, warp SFX, sizes 22/20/18/16/14/12 with refreshes and wait 1 between successive sizes, wait own destination, clone physical 1, idle. |
| 2 | Set logical FOLLOWER_A to `(64,64)`, Down: current/destination become `$6000,$6000`; facing/sprite update while lookup/list remain. |
| 3 | Wait Bowie on eas_45308: Caravan mapsprite 62, resize OFF, refresh, idle. |
| 4 | End: normal script return, including conditional view-scroll wait and camera-speed clear. |

The waited action command resolves a logical actor and polls its idle pointer without HP/presence
validation. FOLLOWER_A 30 lies outside the ally 0–29 alive-skip range of placement. Cleared missing
lookups can alias main 0; invalid 255 is not a safe absence sentinel. Nothing verifies that the mapped
FOLLOWER_A is physical 1 or has the Caravan sprite.

The relative-move action reads its raw physical target's **current position/facing**, not logical
lookup or destination, then sets destination/travel/signed velocity using existing speeds. It bypasses
walking collision/layout tests. Clone copies physical position, destination, travel and facing and
refreshes; it does not copy velocity, sprite, membership or controller. With conflicting FOLLOWER_A,
the first move and off-map relocation can affect another actor while Bowie still clones physical 1.
This static alternative proves neither successful presentation nor a naturally observed defect.

The suffix action first moves each listed follower toward raw physical 0 at distance 0, then enters
eas_452BA after its physical 1 move instruction. It still ends by cloning **physical 1**. In the fresh
consistent case, that slot has just been relocated off-map; followers approach main, then copy that
Caravan state while keeping their sprite identities. The current physical list has no HP filter.

ApplyActscriptToFollowers installs one pointer per suffix byte through 255. WaitForFollowersStopped
uses the same suffix: empty returns immediately; otherwise it waits a VInt, finds the **last listed
actor** and polls only that actor's idle pointer after VInts. It does not join all actors. The three
mapped-actor waits above and this last-actor wait are distinct. SPRITE_SIZE is shared word state,
while resize is a per-actor bit; these requests do not prove simultaneous visible animation.
Zero speeds, missing services, malformed lists or a partial prefix may prevent the final mode write.

Only after the returns does mode 1 become state. Returning to ExplorationLoop with pending zero lets
WaitForEvent install its controller on physical 0; an intervening event takes precedence. There is
no map reload or atomic rollback. F65, follower flags, membership and saved RAFT_MAP/X/Y are unchanged.

## Mode 1 Travel and the Decision to Land

**Confirmed:** mode 1 installs eas_Raft: FLAGS_A bits 7/6/5, speed 32, acceleration factors 0/0 and
acceleration/deceleration operands -2/ON, then map-block-copy check/opcode 7/destination-wait loop.
The upstream “Uncollidable” macro names do not disable the bit 5 obstruction branch. Block-copy handling
recognizes current-layout `$0800/$0C00` markers while not fading; it supplies no reconstructed terrain.

Opcode 7 samples PLAYER_1_INPUT with unshrunk layer 1 bounds, checking Up/Down/Left/Right in order.
Eligible axes can combine; later checks select facing. Without an eligible direction, opcode 7 yields without movement. Its
32-slot obstruction scan uses bit 7 actors' **current positions only**, with both distances below
`$100`; unlike walking, it does not test their destinations. A block can reach sprite update without
new movement. If unobstructed, the controller evaluates the following ordered layout decisions:

| Test | Consequence |
| --- | --- |
| Masked `$0400` | Call OpenDoor, then reread and mask the layout word. |
| Masked `$1000` | Call WarpIfSetAtPoint, then movement tail whether a row matched or not. |
| Masked `$3800` | Movement tail. |
| Otherwise raw word unsigned below `$C000` | Write event 4, skip the movement tail. |
| Otherwise raw word at least `$C000` | Skip movement without requesting landing. |

OpenDoor returns during battle. Otherwise a matching step-event row copies/clears working-layout
words, submits door SFX and marks the view plane. Without a match, the marker does not open anything.
The controller's reread preserves this distinction. A warp match has its separate event consumer;
Map68 town rows `(57,24)`/`(48,30)` and north/west/east borders are alternatives, not an itinerary.

The movement tail sets velocities and travel 384 on nonzero axes, adds tile offsets to destination
and requests conditional sprite/facing update. Landing skips that tail, retaining destination while
possibly turning toward the attempted step. Opcode 7 has no direct F65/unlock/current-battle gate,
walking slope adjustment, zone6 producer, MOVE_SFX or STEP_COUNTER decrement. Opcode 8 instead
recognizes `$3C00` and requests 5 for actual raft exit. These decisions establish neither a reachable
river crossing nor a natural landing square.

## Landing: Current List Growth and Flag-Driven Reinstallation

**Confirmed:** event 4 is cleared before MapEventType4 idles physical 0 and executes `cs_45348`.
It then applies `byte_45368` to FOLLOWERS_LIST, waits through the same last-actor helper, runs
InitializeFollowerActscripts and **only then** writes PLAYER_TYPE 0. It neither saves raft coordinates
nor checks alignment, landing terrain, F65 or HP again.

| cs_45348 record | Ordered work |
| --- | --- |
| 0 | Wait FOLLOWER_A on eas_45360: clone raw physical 0's position/destination/travel/facing, refresh, idle. |
| 1 | setSprite Bowie,Bowie: resolve mapped Bowie, obtain his current ally mapsprite, write it, wait a VInt and refresh. |
| 2 | Wait Bowie on eas_4536C: request one tile in his own current facing, resize ON, warp SFX, sizes 12/14/16/18/20/22 with refresh/wait 1 after each, resize OFF, size 16/refresh, wait own destination, idle. |
| 3 | End and ordinary script return. |

The sprite command has no alive-skip guard. Its second Bowie is an ally identity, not literal sprite0:
class affects the mapsprite and outside-battle HP 0 selects blue flame. It neither heals nor proves
membership. Growth is relative to current position/facing, not always Down, collision-checked movement
or an implicit clone. The suffix action first clones raw physical 1, then enters that growth body.

With consistent mappings, record 0 put the separate Caravan at main's landing origin/facing. Bowie
and the suffix actors move one tile forward from there; Caravan stays until later following actions
move it. Conflicting FOLLOWER_A does not redirect the suffix's physical 1 clone. The current list
selects who grows; no entry or lookup is removed or rebuilt.

InitializeFollowerActscripts uses a **different authority**: choose the normal or F65/overworld
pointer table, scan current flag rows and write consecutive physical slots from 1. It does not walk
FOLLOWERS_LIST, resolve logical lookup, declare/move actors or wait for newly installed actions.
Changed flags or a current-only Rick can disagree with the list that just grew. This is action
reinstallation, not list reconstruction or identity normalization.

Mode 0 plus return and pending zero permits ordinary walking on physical 0. Earlier sprites/positions
can have changed while a stalled prefix leaves mode 1 intact. Landing itself produces no zone request,
STEP_COUNTER decrement or battle selection; the next eligible walking attempt is separate.

## Walking's Separate Battle19 Request

**Confirmed:** walking opcode 2's later masked `$1400` branch writes event 6 and zone words
`(XDEST + selected X offset)/384` at parameter 1 and `(YDEST + selected Y offset)/384` at parameter 3.
This precedes raw-layout blocking. An admitted movement tail submits MOVE_SFX and decrements the signed
STEP_COUNTER only if positive; a blocked tail can leave a zone request without movement/decrement.
Boarding's earlier branch bypasses both. Opcode 7 has no equivalent producer, although separately
supplied or replaced pending state remains possible.

The event 6 consumer installs physical 0's eas_Init and passes the two words to RunMapSetupZoneEvent.
Map68's sole zone row is default marker `$FD` → Map68_DefaultZoneEvent: it bypasses coordinate matching
**once entered**, rather than running every frame or on every tile. Its complete three-operation
callback loads battle 19, calls CheckRandomBattle and returns. The wrapper closes portrait/text,
waits a VInt and waits physical 0 current position equal destination. Init resets motion configuration
and sprite settings, then idles; it does not zero position/destination or existing travel.

| Supplied callback state | CheckRandomBattle consequence |
| --- | --- |
| F519 clear | Request immediately, regardless of counter; no RNG. |
| F519 set, counter nonzero | No request/RNG; helper leaves counter unchanged, including negative values. |
| F519 set, counter 0, first RNG range 8 returns 0 | Request; no second RNG. |
| F519 set, counter 0, first RNG nonzero | No request; second RNG range 4 plus 2 becomes counter 2–5. |

GenerateRandomNumber updates the 16-bit seed with multiply 13/add 7 and scales to the range. This is
arithmetic, not a measured encounter probability or an assumption of independent/uniform seeds.
The request path sets F419, writes longword `$000100FF` at MAP_EVENT_TYPE, writes counter 30000,
waits view scrolling, submits SFX_BOOST and runs the flash script. It does not set F519 or call
CheckBattle/BattleLoop. Flash opcode `$41` operand 30 shifts by 3 and its DBF loop calls LaunchFading
four times, then DuplicatePalettes; fading waits use VInts. Sound/flash requests remain service-dependent.

### Packed Reload Changes Coordinates Through Residual Parameters

**Confirmed:** MAP_EVENT_TYPE is the word at `$FFA84A`; parameter bytes1–5 follow at `$FFA84C..$FFA850`.
The longword writes event 1, parameter 1=0/no scroll and parameter 2=255/MAP_CURRENT. It does **not**
initialize parameters 3/4/5 or WARP_SFX. One replaceable pending slot is not a queue or atomic admission.

In the unmodified selected walking-zone chain with ordinary nonnegative byte coordinates, the zone
Y word remains at parameters 3/4: byte 3=0 and byte 4=Y. The no-scroll consumer therefore substitutes
**X 0 and zone Y**, with residual parameter 5 facing, and submits/clears residual WARP_SFX. This is not
a position-preserving reload. Other supplied bytes or intervening writes change the tuple; negative
coordinate bytes instead retain the sampled current axis. The old zone X word is wholly overwritten.

After callback/wrapper return, pending takes precedence over walking-control installation. Event 1's
consumer clears it, idles main 0, unwinds exploration and returns map 255 with those coordinates/facing.
SwitchMap has no 255 row; CheckBattle resolves it through CURRENT_MAP 68. With F419 still set, row19's
wildcards select Battle19 and write area `(5,19)`, size 48x32, independent of residual X/Y. F519 set
clears F419 without canceling this selected call; clear leaves F419 set. No transport/F65/HP/Sword
predicate is added. Stop at MainLoop's selected BattleLoop handoff, before Battle19 content/outcomes.

A replaced request or cleared F419 makes admission conditional. A no-battle map 255 return instead
uses existing-population exploration: update main properties/raft declaration, load/init, without
fresh setup entities or temporary-flag clearing. It is not fresh Map68 population. Independently
supplied F419 can also admit Battle19 on a later no-scroll return while mode 1 is supplied, including
the initial shrine exit; absence of a zone producer does not prohibit every battle entry.

## Evidence Trace and Review Boundary

**Confirmed provenance:** accepted #852/PR854 binds SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [research reproduction owner](../../research/caravan-boarding-and-southwest-parmecia-travel.md#reproduction-and-remaining-limits)
retains complete vehiclefunctions/event 2/event 4/action tails, opcodes 2/7, lookup/list/follower and
no-scroll/zone/random-battle/flash/MainLoop/CheckBattle joins. This synthesis consumes that accepted
review; it does not reinvestigate source or infer a complete action from a compact fixture total.

| Explained boundary | Contract and exact executable owner | What the fixture does not establish |
| --- | --- | --- |
| Fresh population/setup/init | [map exploration](../contracts/map-exploration.md), [setup](../contracts/map-setup-data.md), [entities](../contracts/map-entity-data.md); `sf2-map-init-static-v1` in [init](../../../tests/fixtures/h2/map-init-static-v1.json), `sf2-map-setup-static-v1` in [setup](../../../tests/fixtures/h2/map-setup-static-v1.json), `sf2-map-entities-static-v1` in [entities](../../../tests/fixtures/h2/map-entities-static-v1.json) | Shared population/selector shapes are not a natural Map68 arrival or actual supplied flag/lookup/list state. The selected table/return joins use accepted research. |
| Boarding/landing and action commands | [map exploration](../contracts/map-exploration.md); `sf2-map-script-engine-static-v1` in [script](../../../tests/fixtures/h2/map-script-engine-static-v1.json), especially entityActionBridgeCommandFacts/entityCloneCommandFacts and placement/lifecycle facts | Public summaries/order/hash/shared handlers do not serialize the full programCorpus. The retained cs_45284 records0–4 and cs_45348 records0–3 are nine map commands; immediate action bodies are separate accepted joins. No composed vehicle runtime is supplied. |
| Map68 default callback | [map exploration](../contracts/map-exploration.md); `sf2-map-events-static-v1`, [index](../../../tests/fixtures/h2/map-events-static-v1.json) and [zone shard](../../../tests/fixtures/h2/map-events/zone-programs.json), expected.zoneTargetPrograms canonicalSymbol Map68_DefaultZoneEvent | Exactly three operations at programOrder 125, plus the accepted default row, do not establish walking terrain or every-frame callback execution. |
| First/repeat random request | [map exploration](../contracts/map-exploration.md); `sf2-map-event-random-battle-state-static-v1` in [random state](../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json), eventRandomBattleState.callerSites.map68-default-battle19 and requestWriteSequence | Caller 19 and 34 source statements/35 H1 rows retain local structure, not probability, completed flash, residual byte values or downstream runtime admission. |
| Request replacement/consumption | [map exploration](../contracts/map-exploration.md), [exploration flow](../contracts/exploration-control-flow.md); `sf2-map-event-request-state-static-v1` in [writes](../../../tests/fixtures/h2/map-event-request-state-static-v1.json), `sf2-map-event-request-consumption-static-v1` in [consumption](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json) | eventRequestState writer/reaching-definition and eventRequestConsumption poll/clear/dispatch facts are not actual read-time state or a producer-to-consumer runtime receipt. Packed bytes use the accepted direct consumer join. |
| MainLoop admission | [exploration flow](../contracts/exploration-control-flow.md), [entry routing](../contracts/map-entry-routing-state.md); `sf2-gameflow-core-static-v1` in [gameflow](../../../tests/fixtures/h2/gameflow-core-static-v1.json), `sf2-common-maps-static-v1` in [maps](../../../tests/fixtures/h2/common-maps-static-v1.json) | Shared call/selection/area-write/completion order alone supplies neither the private row19 nor a started battle. The selected row/caller join comes from accepted research and stops at handoff. |

Schema/consumer pairs are [init schema](../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../src/sf2tool/h2/map_init.py),
[setup schema](../../../schemas/h2-map-setup-static-fixture.schema.json)/[map_setup.py](../../../src/sf2tool/h2/map_setup.py),
[entity schema](../../../schemas/h2-map-entities-static-fixture.schema.json)/[map_entities.py](../../../src/sf2tool/h2/map_entities.py),
[script schema](../../../schemas/h2/map-script-engine-fixture.schema.json) with [entity component](../../../schemas/h2/map-script-engine-entity.schema.json)/[map_script_engine.py](../../../src/sf2tool/h2/map_script_engine.py)
and [schema ownership](../../../src/sf2tool/h2/map_script_schema.py),
[zone schema](../../../schemas/h2/map-events-zone-programs.schema.json)/[map_events.py](../../../src/sf2tool/h2/map_events.py)
and [shard consumer](../../../src/sf2tool/h2/map_events_fixture.py),
[random schema](../../../schemas/h2/map-event-random-battle-state-static-fixture.schema.json)/[map_event_random_battle_state.py](../../../src/sf2tool/h2/map_event_random_battle_state.py),
[write schema](../../../schemas/h2/map-event-request-state-static-fixture.schema.json)/[map_event_request_state.py](../../../src/sf2tool/h2/map_event_request_state.py),
[consumption schema](../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../src/sf2tool/h2/map_event_request_consumption.py),
[gameflow schema](../../../schemas/h2-gameflow-core-static-fixture.schema.json)/[gameflow.py](../../../src/sf2tool/h2/gameflow.py)
and [maps schema](../../../schemas/h2-common-maps-static-fixture.schema.json)/[maps.py](../../../src/sf2tool/h2/maps.py).
Existing reproduction commands are `uv run sf2 h2` with `map-init`, `map-setup`, `map-entities`,
`map-script-engine`, `map-events`, `map-event-random-battle-state`, `map-event-request-state`,
`map-event-request-consumption`, `gameflow-core` and `common-maps`. Load ignored worktree configuration
in the same process before controlled reproduction. These identify owners, not new prose-only runs.

Pre-synthesis review checks accepted research against the stricter contract/fixture boundaries,
including summary versus retained corpus, source versus H1 units, mode/action names, physical/logical
selectors, current-list waits versus flag-row reinstallation, partial services and pending precedence.
F65/mode mismatches, F74/F77/F83 lookup overwrites, HP 0/missing actors, current-only Rick/empty suffix,
zero speeds, changed flags, residual parameters, F419/F519/counter/seed variants and replaced events
are discriminating static cases, not an executed runtime matrix. No Layer A repair is required.
Older generic runtime-question queues do not erase these accepted selected static consumer joins
and do not become automatic acquisition work.

Preserve [PR854 original evidence](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/854#issuecomment-6161835568),
[rebased final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/854#issuecomment-6161892874)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/854#issuecomment-6161933230):
retained 18,285,979-byte comparisons, selected 9/3/34 program/callback/helper joins, corrected/failed
inspections, original/rebased identities and actual CI. They retain linked PR849/846/842 histories.
Accepted comparisons are not replayed solely for prose; no fixture/schema/golden is weakened or
aggregate fact promoted to composed runtime observation.

**Unknown:** natural arrival/tile/door/warp/landing reach and actual input; supplied flags/counter/seed,
HP/class, actor mappings/list and residual parameters; completed motion/resize/sprite/sound/fade/view
services; responsive walking/vehicle control and persistence. These stay deferred under
[ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) and
[ADR0016](../../decisions/0016-remake-start-evidence-deferral.md). This guide selects no remake rule,
new verification machinery or automatic runtime work, and consumes no unaccepted successor finding.
