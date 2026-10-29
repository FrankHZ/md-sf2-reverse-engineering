# Polca raft boarding through South Parmecia entry

Evidence date: 2026-10-13. **Confirmed:** ordinary movement can request type3
boarding; its consumer separately checks alignment with the physical raft before
running the boarding actions and finally writing PLAYER_TYPE=2. The installed
controller is named `eas_Caravan` in this pin, despite consuming raft-mode state.
The two selected southern Map72 exits have different lifecycles: no-scroll returns
to main-loop battle admission; scroll changes maps inside exploration without that
scan or fresh setup/init. **Inferred:** these operations implement boarding and
water travel. **Unknown:** natural boarding/traversal, actual flags, actor mappings,
completion, presentation, responsive input and persistence.

## Agreement and provenance

Standard rigor under [Issue #816](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/816)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
producer eligibility, actor completion and map admission must not be collapsed.
This is the sole tracked owner. Accepted base is
`c8a20bec656d15351e507fdf1fa49cf9d24392ee`; dependencies are accepted
[Luke recruitment](../eastern-parmecia/polca-departure-through-luke-recruitment.md),
[raft grant](../eastern-parmecia/battle13-outcomes-through-polca-raft-grant.md),
[request state](../../world/map-event-request-state.md) and
[request consumption](../../world/map-event-request-consumption.md). No stack or shared edits.

Scope begins at Luke's conditional Map72 control boundary and ends at either the
type5 disembark request, an admitted battle before initialization/combat, or
conditional Map69 exploration control. No continuous journey, terrain/pathfinding,
vehicle inventory, full disembark, Caravan behavior, downstream zone/Creed/Kraken
story, new verification machinery, engine change or runtime acquisition. The
remaining content question is what a separately admitted Map69 interaction or
zone does after this boundary; this document does not select or authorize it.

All source paths below are relative to `disasm/` in
[pinned SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Existing retained map-init and
map-script-engine payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The unchanged pair totals 18,285,979 bytes. Existing fixture/source comparisons
passed in 0.083 seconds. Linear selected comparisons were budgeted below 512 MiB
peak and one minute, plus small prose/receipts; no new export corpus. Bounded
reads include the necessary controller, transition and list consumers, rather
than decoding terrain. Reassess before widening that boundary. Fnnn means flag
nnn; table rows and program operations are zero-based.

## Movement request and the separate alignment gate

**Confirmed:** `code/common/scripting/entity/entityscriptengine_2.asm`,
`esc02_controlCharacter` through `loc_531E`, is the ordinary producer. The installed
`eas_ControlledCharacter` configures speed32 and loops map-block-copy check,
character control and destination wait. Direction inputs are considered Up,
Down, Left, Right against the current layer1 bounds; later eligible inputs can
overwrite facing/axis values. The character path has its own bound/input selection
and horizontal slope adjustment; a narrative instruction is not an input receipt.

Before the event tests, bit5 of the controlled actor's FLAGS_A enables a scan of
32 physical slots. An actor with bit7 set can block the proposed point when both
axis distances are below `$100`, testing current positions and destinations.
Then the working-layout word is masked with `$3C00`. At `loc_5256`, masked `$3C00`,
CURRENT_BATTLE=`NOT_CURRENTLY_IN_BATTLE=$FF`, and F64 set write
`MAP_EVENT_GETINTORAFT=3` to the pending word and branch to the sprite-update tail.
This branch does not advance the player's destination/travel or decrement the
step counter. The attempted direction can update facing. It precedes the later
raw-layout `$C000` blocking test. An obstructing actor can prevent reaching it;
`eas_Standing` clears the separate raft's obstructable bit when that action runs.

There is no F716/F718/F10/F75/F416, RAFT_MAP, membership, HP or direct raft-coordinate
test at that producer. F64 by itself cannot supply a valid actor/alignment. Luke's
scene sets no boarding event; his Down1/hide and the raft-grant tuple Map72
`(43,48)` do not establish player contact. Ordinary door (`$0400`), warp (`$1000`)
and zone (`$1400`) alternatives remain separate layout branches.

`code/gameflow/exploration/explorationfunctions_2.asm:WaitForEvent` polls pending;
`ProcessMapEvent` clears it **before** selecting type3 and calling
`ProcessMapEventType3_GetIntoRaft → j_MapEventType3 → MapEventType3` in
`code/common/scripting/map/vehiclefunctions.asm`. Pending is a mutable slot, not a
queued transaction; an intervening write/service state is not proved here. The
aliases resolve in `code/common/tech/jumpinterfaces/s07_jumpinterface.asm`.

`MapEventType3` first calls `sub_454AC`. It reads physical main0's **destination**
X/Y and facing, adds the corresponding one-tile offset from
`table_4549C/table_4549E` (Right `384,0`; Up `0,-384`; Left `-384,0`; Down `0,384`),
then sums the word absolute differences from physical31's destination. Offsets
`ENTITYDEF_ENTITY32_XDEST=$3EC` and YDEST=`$3EE` are slot31 with 32-byte entities.
Nonzero returns immediately; zero admits boarding. This tests facing-adjacent
destinations, not current-position overlap, sprite identity or a logical follower.
F64/CURRENT_BATTLE/RAFT_MAP are not rechecked by this consumer. Supplied malformed
coordinates/mappings and actual action progress remain outside a natural-contact
claim. On rejection the consumed event stays cleared and mode is not changed.

## Complete boarding and actor identity

**Confirmed:** the admitted order is physical0 idle → clear `byte_FFAFB0` → run
`cs_453F2` → install `byte_45434` on the exploration list → wait on the list helper
→ write `PLAYERTYPE_RAFT=2` → return. The byte cleared before the script belongs to
the sprite-link ordering flags consumed by `entityscriptengine_1.asm:sub_4E24`;
it is not an HP, membership or transport-mode write. `MakeEntityIdle` only installs
the idle pointer; it does not clear existing travel/velocity fields.

The complete `cs_453F2` at `0x453F2`, source lines142–145, has four records:

| Record | Ordered command and immediate effect |
| --- | --- |
| 0 | Waited Bowie `eas_4540C`: request movement to physical `ENTITY_RAFT=31` at relative distance0; wait6; request warp SFX; wait until own position equals destination; clone physical31's position, destination, travel and facing; branch to `eas_Idle`. |
| 1 | `setPos FOLLOWER_D,64,64,DOWN`: resolve logical159 through mapping offset63 and set that actor's position/destination to internal `$6000,$6000`, facing Down, with sprite refresh. |
| 2 | Waited Bowie `eas_45426`: set `MAPSPRITE_RAFT`, turn resizable off, request sprite update, branch to idle. |
| 3 | `csc_end`: ordinary terminator/caller return, including the accepted conditional view-scroll wait and scroll-speed clear. |

`sf2enums.asm`, `sf2cutscenemacros.asm`, `mapscriptengine_1.asm:csc15_setEntityActscript`,
`csc19_setEntityPosAndFacing` and `GetEntityAddressFromCharacter` define these
operands. The waited script commands resolve Bowie through lookup0 and wait for
that mapped actor's action pointer to equal `eas_Idle`; they have no HP guard.
FOLLOWER_D159 takes the non-ally branch of the position command's alive-status
helper, so zero Bowie/Peter/Luke HP does not skip that raft relocation. No command
here heals, joins, hides via `HideEntity`, clears follower flags or changes maps.

The raft's physical31 is distinct from logical FOLLOWER_B31 (Oddler or another
flag-selected actor). FOLLOWER_D159 subtracts96 to lookup63; normal raft declaration
writes physical31 there. Its position command neither clears this mapping nor
removes an exploration-list entry. Conversely, `ac_moveEntFacRelPos` and
`ac_clonePos` use **physical** indices directly (`esc0E` and `esc0D`); they do not
resolve FOLLOWER_D. Missing/aliased lookup63 can therefore move main0 or another
actor while subsequent clones still target physical31. An invalid mapping is not
validated as a live actor. Fresh normal mappings justify the intended effects,
not every supplied-state case.

After the four-record script, `byte_45434` requests movement to physical0 at
relative distance0, then branches into `byte_45414`: wait6, warp SFX, own destination
wait, clone physical31, idle. In normal state that physical raft is now at `(64,64)`;
listed actors first converge on main and then copy the off-map raft state. They
keep their own sprite identities. There is no transformation of every follower
into a raft, membership change or per-follower HP gate.

The helper names must not replace their actual list semantics:
`entityfunctions_3.asm:ApplyActscriptToHeroAndFollowers` iterates physical bytes
from `EXPLORATION_ENTITIES` until `$FF`, installing the pointer with
`SetEntityActscript`. Fresh `mapfunctions.asm:InitializeMapEntities` passes that
list's **start** to `InitializeFollowerEntities`, which appends admitted followers
starting at physical1, then terminates the list after declaring main0 separately.
It does not append main0. `FOLLOWERS_LIST` is one byte later, not an independently
rebuilt list. Thus the normal fresh list acted on here consists of followers,
despite the helper's “HeroAndFollowers” name. Current/supplied list contents govern
other cases; it does not reconstruct them from flags or logical names.

`WaitForHeroAndFollowersStopped → WaitForPartyEntitiesIdle` returns immediately for
an empty list. Otherwise it waits a VInt, scans to the terminator, and polls **only
the last listed physical actor's** pointer after VInts until it equals `eas_Idle`.
It does not independently join every actor. The earlier two Bowie waits and this
last-actor wait are separate completion predicates. List changes, malformed lists,
missing actors and service stalls are not normalized. The final mode write is
ordered after these returns, not atomic with sprite/position changes: interruption
can leave raft presentation or relocated followers while PLAYER_TYPE is still0.
RAFT_MAP/X/Y remain supplied; no boarding-time persistence write occurs.

## Installed raft controller and stopping alternatives

**Confirmed:** with pending zero, the next `WaitForEvent` targets physical0 and
calls `entityfunctions_2.asm:SetControlledEntityActScript(0)`. The exact selector is:

| PLAYER_TYPE | Installed source label | Macro / numeric dispatch |
| --- | --- | --- |
| 0 | `eas_ControlledCharacter` | `ac_controlCharacter` → opcode2 |
| 1 (`PLAYERTYPE_CARAVAN`) | `eas_Raft` | `ac_controlRaft` → opcode7 |
| 2 (`PLAYERTYPE_RAFT`), or other nonzero/non1 supplied value | `eas_Caravan` | `ac_controlCaravan` → opcode8 |

For this slice the actual consumer is **`esc08_controlCaravan`**, source
lines1065–1217, not the similarly named opcode7. Preserve the pin's names:
`MAP_EVENT_GETOUTOFCARAVAN` equals **5**, dispatched to `MapEventType5` (raft exit),
whereas `MAP_EVENT_GETOUTOFRAFT` equals4. The symbolic naming disagreement is not
permission to swap the controller or infer a different numeric request.

`data/scripting/entity/eas_main.asm:eas_Caravan` sets speed32, acceleration/deceleration
parameters and FLAGS_A bits7/6/5 via opcodes `$18/$19/$1A`, then loops block-copy check,
opcode8 and destination wait. The “Uncollidable” macro names do not suppress the
consumer's bit5 collision branch. `esc40_checkMapBlockCopy` uses the current working
layout's `$0800/$0C00` copy markers when not fading; this is not a terrain decode.

Opcode8 samples `PLAYER_1_INPUT`, considers Up/Down/Left/Right in that order, and
checks current position against unshrunk layer1 bounds. Eligible axes can combine;
later direction checks set final facing. When no direction is eligible, the script
advances without requesting movement. Bit5 enables a 32-slot bit7-obstructable scan using
**current positions only**, with both axis differences below `$100`; unlike ordinary
movement it does not check the other actors' destinations. A blocked attempt goes
to sprite update without movement. After that scan:

| Working-layout test, in order | Result |
| --- | --- |
| Masked `$0400` | Request `OpenDoor`, then reread/mask the layout word. |
| Masked `$1000` | Call `WarpIfSetAtPoint`, then take the movement tail whether or not a row matched. |
| Masked `$3C00` | Take movement tail. |
| Otherwise raw word unsigned below `$C000` | Write numeric type5; skip movement tail. **Alternative stopping boundary.** |
| Otherwise raw word at least `$C000` | Skip movement without a disembark request. |

The movement tail writes velocity from selected speed components, sets 384 travel
for each nonzero velocity, and adds the selected tile offsets to destination; then
sprite update runs. There is no new F64/current-battle/unlock test in these opcode8
branches, no type6 zone producer here and no step-counter decrement. Complete
`MapEventType5`, later walking, arbitrary travel and Caravan operation are excluded.
Layout words, map-block copies, actor occupancy, state and input must be supplied
to establish a path; rows or southward dialogue cannot do so.

## Selected Map72 exits and producer compatibility

**Confirmed:** `data/maps/entries/map72/6-warp-events.asm` contains ten ordered
8-byte rows. The selected rows and possible competing edge matches are:

`sf2enums.asm` resolves the row operands before selection of numeric map owners.
South Parmecia is69; `MAP_OVERWORLD_SOUTHEAST_PARMECIA=70` is a different map.

| Row | Match `(X,Y)` | Source destination symbol → map | Request |
| --- | --- | --- | --- |
| 6 | `(40,62)` | `MAP_OVERWORLD_SOUTH_PARMECIA` → 69 | No-scroll, `(41,16)`, Down. |
| 7 | `(0,255)` | `MAP_OVERWORLD_NEW_GRANSEAL_KINGDOM` → 73 | Scroll Left; precedes the south wildcard. |
| 8 | `(63,255)` | `MAP_OVERWORLD_PACALON_KINGDOM_DROUGHT` → 71 | Scroll Right; precedes the south wildcard. |
| 9 | `(255,63)` | `MAP_OVERWORLD_SOUTH_PARMECIA` → 69 | Scroll Down, `(255,1)`, Down. |

Rows0–5 are specific cave/Ketto/Polca entrances and do not match either selected
point. Negative coordinate bytes are wildcards in
`exploration.asm:WarpIfSetAtPoint`; first match wins. Rows6 and9 do not overlap.
At `(0,63)` row7 wins; at `(63,63)` row8 wins. Other X on Y63 can select row9,
provided the movement producer actually reaches the `$1000` layout call.
The routine converts proposed internal coordinates by signed division by384,
writes type1 and its tuple plus warp SFX; it does not test raft mode or story flags.

Both maps' `2-areas.asm` declare layer1 bounds `(0,0)..(63,63)`. Ordinary character
control shrinks bounds by one tile in its `d7==0` branch; the vehicle controller
does not. Do not equate loop index `d7` with physical0 by name: it is the remaining
entity-scan counter in `VInt_UpdateEntities`. Regardless, row6 and the Y63 edge are
compatible with opcode8's unshrunk bounds, subject to layout/collision/input. This
is a producer compatibility statement, not proof of a natural water route or of
the state needed to approach `(40,62)` or Y63.

## No-scroll admission versus scrolling continuation

**Confirmed:** `ProcessMapEvent` clears pending before type1. With parameter1=0,
`ProcessMapEventType1_Warp` requests/clears warp SFX, idles physical0, unwinds the
exploration return frame, samples main's current coordinates/facing, replaces
nonnegative X/Y and takes the tuple's facing. Row6 therefore returns Map69
`(41,16)`, Down to `mainloop.asm:MainLoop/@Start`. It preserves PLAYER_TYPE and
RAFT_MAP/X/Y. `SwitchMap` has no Map69 replacement in `flagswitchedmaps.asm`.
Then `CheckBattle` scans the actual Map69 rows in `battlemapcoords.asm`:

| Battle row | Area output `(X,Y,width,height)` | Trigger | Unlock / completion |
| --- | --- | --- | --- |
| 9 | `(0,0,48,32)` | `(255,255)` | F409 / F509 |
| 17 | `(0,31,30,32)` | `(255,255)` | F417 / F517 |

Map, unlock, then trigger are tested. The first unlocked matching row wins;
F409 takes priority over F417. Area rectangles are **outputs**, not containment,
so `(41,16)` does not exclude either row. Set completion clears the selected
row's unlock but **still admits that battle on this call**, without falling
through to the next row. With both unlocks clear there is ordinary exploration.
F416 from Luke unlocks Battle16, whose map is49, not either Map69 row; it alone
does not prove either selected battle or any immediate admission. Supplied
unlock/completion state is not repaired from story progress. Stop before `j_BattleLoop` initialization or
combat. Battle return's separate `alt_MainLoopEntry` is not this original entry.

Row9 has nonzero parameter1 (scroll Down). Its consumer idles main, writes
CURRENT_MAP=69, and calls `mapload.asm:ProcessMapTransition`; the special Map71/F530
replacement does not apply. That service loads selected tiles, blocks and working
layout with flag-dependent copies, and dispatches Down through
`ApplyOverworldMapTransition`. The Down routine scrolls/redraws with DMA/VInt waits.
It then returns to the type1 handler, which leaves negative X unchanged and sets
main's Y position/destination to384 (tile1). It samples current position/facing,
calls `DeclareRaftEntity`, and returns **inside the same exploration loop**.

This scroll branch does not consume parameter5 to set facing, unwind to MainLoop,
call SwitchMap/CheckBattle, rebuild entities, clear setup temporary flags, heal,
run Map69 init, or request a fresh map-music/fade-in lifecycle. The row carries
Down, but the retained actor's facing is the consumer result. It also leaves
PLAYER_TYPE and RAFT_MAP/X/Y unchanged. For raft mode2, `DeclareRaftEntity` returns
immediately, retaining current mappings/list/hidden actors. Thus F409/F417 do not
interrupt this selected scroll call; applying the no-scroll battle table here
would invent a call. Subsequent events are outside this slice. Services and
intervening mutable parameter writes remain completion/state limits for both forms.

## Ordinary Map69 population and conditional control

**Confirmed:** only the no-scroll path with no admitted battle reaches fresh
`ExplorationLoop(69,41,16,Down)` here. It clears pending, adjusts step count,
heals living/immortal allies, fades out, sets Map69 and not-in-battle, loads tiles,
initializes actors/mapsprites, clears temporary F256–383 and sets F80 before
map loading, base VInts and init. Map69 has a default-only entry in `mapsetups.asm`;
its pointer table selects an empty `s1_entities.asm` and
`s6_initfunction.asm:ms_map69_InitFunction` at `0x4FDE2`, a sole `rts`. There is no
fresh Map69 story script or F416-dependent init on this path.

Map69 is an overworld map and not follower-excluded. The accepted flag-based
follower construction applies again: F65 can prefix caravan; F67 admits Peter;
F68 maps FOLLOWER_B to Oddler, subject to the other rows that use that same logical
lookup; F75 can reconstruct Luke despite his prior current-actor hide. Membership
alone is not follower admission. Supplied flags, HP-dependent ally sprites and
mapping order remain the [predecessor's population boundary](../eastern-parmecia/polca-departure-through-luke-recruitment.md#fresh-population-raft-declaration-and-init-guard).
There is no fixed NPC row to repair an absent follower.

`followersfunctions_2.asm:sub_44404` sees mode2, sets physical0's sprite to raft,
then `MakeFollowersStand` moves each **listed** physical actor to internal
`$6000,$6000` and installs `eas_Standing`. With the fresh list above, main0 stays
at `(41,16)`; followers stand off-map. It declares separate physical31 at `(64,64)`,
Left, raft sprite, standing, and sets lookup63=31. This mode2 branch needs neither
F64 nor a matching RAFT_MAP. It does not rewrite the saved raft tuple.

For supplied non-raft mode, the ordinary F64-and-CURRENT_MAP==RAFT_MAP gate instead
controls separate raft declaration. A still-stored Map72 tuple does not declare
it on Map69. Fresh declarations and scroll's no-rebuild `DeclareRaftEntity` are not
interchangeable: outside raft mode the latter can relocate listed followers, and
with F64 set/map mismatch clears lookup63 and moves physical31 to `$7000,$7000`.
These supplied alternatives are not a claim that boarding secretly disembarks.

After the no-op init, only a base/current palette inequality triggers
`PlayMapMusic` and `FadeInFromBlack`; there is no unconditional second music request.
At the next `WaitForEvent`, pending zero retargets physical0 and installs the
PLAYER_TYPE-selected script. For preserved mode2 this is the opcode8 controller
above. Scroll continuation reaches the same conditional control setup without
fresh population/init. Nonzero pending defers control setup to event processing.
**Stop here:** available code paths do not establish rendered arrival or responsive
input, and no downstream zone, Kraken/Creed event, battle init or combat is entered.

## Reproduction and coverage limits

Load `. ./local/private-inputs.ps1` in the command's process; verify upstream pin
and clean status. Run the existing
[retained payload/cutscene comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
without rewriting inputs. Select `programCorpus.programs` entry `cs_453F2`, compare
all four sourceLine/macro/argument records to `vehiclefunctions.asm:142–145`, then
read the named adjacent actor bodies and wrappers separately. Select map-init
`primarySourceBodies` by the resolved destination's path
`data/maps/entries/map69/mapsetups/s6_initfunction.asm`: sourceOrder76,
`ms_map69_InitFunction`, address `0x4FDE2`, sole `rts`.

For the destination check, parse each selected `warpMap` operand, resolve it from
pinned `sf2enums.asm`, and use that numeric value to select `msMap 69, ms_map69`,
the corresponding pointer/entity/area/init files, and the battle-table rows whose
map byte equals69. Compare the retained init's path, owner symbol, address and
operation to that selected source. Compare both corner-row symbols as well.
Checking a symbolic row and an independently assumed numeric owner's records is
insufficient, even when their bodies happen to match. No dialogue/transcript or
new composed candidate is needed.

| Existing fixture surface under `tests/fixtures/h2/` | Relation reused and limit |
| --- | --- |
| `map-script-engine-static-v1.json` and retained corpus | Four boarding records and established map-script handlers. Does not prove natural type3 production, alignment, all-list completion or player control. New immediate action/list/controller consumers above are direct pinned-source evidence. |
| `map-event-request-state-static-v1.json`, `map-event-request-consumption-static-v1.json` | Existing request/raft state and pending-consumer topology. Their bounded joins do not compose boarding, these two exits and arrival. |
| `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json` | Default/empty population and sole init return at their aggregate boundaries; selected records and actual follower/raft consumers were read directly. Empty fixed rows do not mean no actors. |
| `battle-routing-data-static-v1.json` | Aggregate routing facts; exact Map69 row9/17 values and `CheckBattle` order were read directly. A successful fixture comparison is not per-case or composed route coverage. |

Discriminating supplied cases are misaligned physical raft despite F64; lookup63
not mapping physical31; empty or differently ordered follower list; earlier
listed actor still busy when the last becomes idle; presentation changed before
mode commit; opcode7 selected by an incorrect label-based reading; edge corners
matching rows7/8 before9; F409 preceding F417; completed-but-still-unlocked battle;
and the same F417 state taking admission on row6 but no scan on row9.

**Unknown:** natural reach from Luke's exit to a boarding marker and either south
exit; actual layout/actor/flag/HP/list state; which pending write survives; action,
VInt/DMA/sound/sprite/transition completion; all followers finishing; visible
boarding/arrival and input response; saved raft tuple and transport persistence.
These remain deferred under [ADR 0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR 0016](../../../decisions/0016-remake-start-evidence-deferral.md), not an automatic
acquisition queue. Prior successful comparisons/CI and resolved inspection,
quoting/path/propagation failures remain preserved in
[PR814's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/814#issuecomment-6159534926)
and its linked original record.

Acceptance uses direct source/provenance/link/private/whole-diff checks,
`git diff --check`, the clean committed dependency planner and actual final-head
CI. Documentation-only scope does not select local normal/full/engine/Godot/H3/H4
suites. Current check results, exact Git identity and any inspection failures belong
in the Issue/PR handoff; independent acceptance and merge remain main-gate's.
