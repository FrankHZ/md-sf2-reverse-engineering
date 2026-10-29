# Battle16 Outcomes, Disembark and Conditional Hassan Entry

Defeating the Kraken can return the party to South Parmecia in raft mode. Losing or escaping
instead selects Polca's savepoint. Leaving the raft is a separate event that saves its departing
location, stages a forward step and restores following before returning to walking mode. A selected
South Parmecia exit then requests Hassan, whose cast and init depend on current flags. Neither that
exit nor town entry establishes that everyone landed or that walking control is usable.

**Confirmed static content and control flow:** the accepted
[research owner](../../../../research/chapters/southern-travel-creed/battle16-outcomes-through-hassan-entry.md) supports the outcome,
disembark and entry mechanisms below. **Inferred:** consistent actors and completed movement can
compose a successful landfall after the encounter. **Unknown:** natural combat/travel, actual caller
state, completed services, original presentation, usable input and persistence. Fnnn means flag nnn;
coordinates are map-tile operands unless stated otherwise, and record indices are zero-based.

Read [the Kraken reveal and Battle16 entry](south-parmecia-through-kraken-battle16-entry.md) for the
preceding admission/start boundary and [raft boarding](polca-raft-boarding-through-south-parmecia-entry.md)
for the separate boarding mechanism. This guide stops at fresh Hassan init and the first conditional
control handoff, before town actions, zones, entities, items, service dialogue, priest events,
Rick/Rohde interaction or later story. It adds no continuous playthrough or remake capability.

Continue with [Hassan ancestry and Rohde's Caravan choice](hassan-ancestry-and-rohde-caravan-choice.md)
for the independent priest zones and Rohde talk/selected-item paths. That guide distinguishes
initial/repeat choices, current following and conditional menu/control return, without assuming
usable walking at this entry or proceeding to sword acquisition, Rick, departure, Taros or Caravan use.

## How the Encounter Ends

**Confirmed caller order:** after an individual turn, BattleLoop invokes the defeated hook,
processes the dead-combatant list and counts remaining allies/enemies. Zero allies wins priority
over zero enemies. If neither ends combat, after-turn effects run and the list/count/defeat-first
check repeats, without another defeated-hook call at that checkpoint.

For Battle16, the hook requires Bowie alive and enemy128 at zero HP. The first initialized enemy
row is the Kraken head; the predicate tests that slot's HP, not its name or sprite. F516 clear
selects an empty defeated script; F516 set skips it. Both reach the leader tail. Battle16's leader
row is -1, so that tail queues every remaining nonzero-HP enemy for death processing.

Death processing performs presentation/stat bookkeeping and marks listed combatants' X=-1.
The count requires nonnegative X and nonzero HP; Bowie HP0 forces zero allies. Thus head defeat
can remove surviving limbs from the count without making their HP zero. Simultaneous zero counts
select defeat. A head dying only in after-turn effects does not receive an extra hook call there;
stalled death services do not establish a completed victory. The
[outcome owner](../../../../research/chapters/southern-travel-creed/battle16-outcomes-through-hassan-entry.md#outcome-selection-and-return-state)
retains the exact caller, leader and count joins.

| Selected outcome | State and return consequence |
| --- | --- |
| Victory | Heal eligible allies; seed a return tuple from the current map and the first selected battle-party member's position plus battle-area origin and mapped facing. That member need not be Bowie. Zero parameter1, run the after wrapper, clear F416 and set F516, then read return parameters2–5 and return result1. |
| Defeat | Update the unlock flag, request exhaustion text363/music, restore Bowie's HP, halve gold by unsigned right shift and select the egress tuple; return result-1. Battle4's special loss does not apply. No after scene or new F516. |
| Egress / Angel Wing | Consume the selected spell's MP or used item slot, request text/flash services, update the unlock flag and unwind the action frame to the egress tuple; return result0. No defeat gold penalty or victory completion. |

Unlock updating clears F416 only if F516 was already set; first loss/escape leaves the unlock
unchanged. **F416 unlock, F516 completion and F466 introduction/start remain distinct.** No outcome
here changes F466. Shared healing restores living allies and immortal Peter/Lemon to HP/MP maxima,
retains existing STUN/POISON/CURSE bits and revives neither other dead allies nor missing members.

Defeat/Egress explicitly selects **Polca13**, ignoring carried EGRESS_MAP. Normal BattleLoop entry
set F399, so the savepoint is `(18,25)`, Up. With F64 set, the Polca reset row writes the saved raft
tuple to **Map72 `(43,48)`**; with F64 clear it leaves that tuple supplied. A malformed F399-clear
direct call takes the earlier room fallback before the reset scan, rather than this normal return.
The normal battle entry also cleared PLAYER_TYPE to0. Defeat/Egress do not write raft mode, so
mode0 remains conditional on that caller and no intervening mutation, independently of the saved
raft location. This alternative stops at return selection, before Polca's content.

## First Victory and the Return to the River

**Confirmed complete scene:** with F516 clear at after-wrapper entry, `abcs_battle16` at `0x4B1BC`
has exactly three records. With F516 set, the wrapper skips all three.

| Record | Immediate effect |
| --- | --- |
| 0: `executeSubroutine csub_4B1D0` | Write PLAYER_TYPE=2, the raft mode. This exact body differs from neighboring `csub_4B1CA`, which clears mode. |
| 1: `warp MAP_OVERWORLD_SOUTH_PARMECIA,29,51,LEFT` | Resolve the map to69; write pending type1, parameter1=0 and the map/X/Y/facing tuple. It loads no map, moves no actor, saves no raft tuple and requests no warp sound. |
| 2: `csc_end` | Return through the ordinary terminator's conditional view-scroll wait and scroll-speed clear. There is no dialogue or actor action program in this scene. |

Here **BattleLoop reads the tuple directly after the wrapper**. It does not run exploration's
ordinary type1 warp consumer. First victory returns `(69,29,51,Left,result1)` through
`alt_MainLoopEntry`, requests SwitchMap and enters ExplorationLoop **without CheckBattle**.
There is no map-switch replacement for69, and its battles9/17 are not immediately scanned merely
because victory returned there. Pending plus parameter bytes are mutable slots, not an atomic queue.

Both first and repeated victory reach the shared join tail. After-join row16 calls `JoinForce(0)`:
set Bowie's F0, update force and conditionally add him to the active party below capacity12.
This is not an empty tail or a new companion recruitment. The return seed was selected before it.

| Supplied F516 at the wrapper | Return and transport |
| --- | --- |
| Clear | Run the three records, set mode2 and replace the seed with Map69 `(29,51)`, Left; then clear F416/set F516. |
| Set | Retain seeded Map49, variable coordinates/facing and supplied mode, normally0. Still run the join tail and victory flag writes. There is no repeated fixed river return or raft-mode write. |

Repeat Map49 uses the predecessor's `ms_Void` exploration fallback, not a new map scene. Interrupting
between mode and tuple writes can leave mode2 with the old destination; interrupting after the warp
can leave F516 clear. Completion does not prove that the scene just ran or that its services finished.
No after-scene instruction writes RAFT_MAP/X/Y, and no completed presentation proves saved persistence.

On fresh Map69 return, exploration clears pending, reduces/clamps the step counter, heals eligible
allies, loads map/tiles and rebuilds entities. It selects current map69/battle255, clears temporary
flags256–383 and sets F80. Map69 has no fixed setup actors and its init simply returns.
Battle actors/lookups are replaced. Mode2 gives main0 the raft sprite, moves listed exploration
actors to `(64,64)` with standing actions and declares separate physical raft31 there, Left,
with lookup63→31. This branch needs neither F64 nor RAFT_MAP=69 and saves no tuple.

Current follower flags supply the new cast; F65 plus overworld69 selects the overworld table.
No cast or heal proves a particular active roster. Conditional music/fade and `WaitForEvent` follow;
with pending zero, physical0 receives the mode2 controller. The
[return owner](../../../../research/chapters/southern-travel-creed/battle16-outcomes-through-hassan-entry.md#complete-after-scene-and-fresh-south-parmecia-return)
retains the full scene and population boundaries; actual update/input completion remains Unknown.

## Leaving the Raft

**Confirmed producer:** mode2 selects `eas_Caravan` and actual entity opcode8 despite those source
names. After input, bounds and current-actor obstruction checks, its door/warp/movement marker
branches take priority. Otherwise a raw working-layout word unsigned below `$C000` requests
**numeric5** without the movement tail; words at least `$C000` do not request it. Attempted facing
can already have changed, but the type5 branch does not advance the destination. F64, raft
coordinates, membership and battle flags are not additional producer tests.

At the pinned source, `MAP_EVENT_GETOUTOFCARAVAN=5` and `MAP_EVENT_GETOUTOFRAFT=4`. The actual
numeric5 route is `ProcessMapEventType5_GetOutOfRaft → j_MapEventType5 → MapEventType5`.
Exploration clears pending first. Unlike boarding, this consumer has no alignment/admission rejection.
Its [complete ordering owner](../../../../research/chapters/southern-travel-creed/battle16-outcomes-through-hassan-entry.md#numeric-type5-tuple-save-disembark-and-restoration)
supports the following sequence:

1. Save current map and **physical0's destination** in RAFT_MAP/X/Y. Each destination word is
   sign-extended, divided by384 internal units per tile, then stored as a low quotient byte.
   This is the departing raft location, before the forward move, rather than its landing point
   or physical31's position. It is a RAM update, not a disk/SRAM persistence receipt.
2. Idle physical0, set sprite-link ordering byte_FFAFB0=1, and run all four records of `cs_45470`.
3. Install the follower exit action on the current EXPLORATION_ENTITIES list and call its stop wait.
4. Restore flag-selected follower action pointers, then finally write PLAYER_TYPE=0 and return.

| `cs_45470` record | Actor, movement and wait |
| --- | --- |
| 0 | Waited logical FOLLOWER_D159 action clones **physical0** position, destination, travel and facing, refreshes its own sprite and idles. Normal lookup63→31 puts the separate raft at departure. |
| 1 | Resolve logical Bowie0, select his current ally mapsprite, write it, wait a VInt and refresh. PLAYER_TYPE is still2. |
| 2 | Waited Bowie action requests one tile forward relative to current position/facing, warp sound, its own destination wait and idle. It does not clone the raft. |
| 3 | Ordinary script terminator, with the conditional scroll wait/speed clear described above. |

The follower action first clones **physical raft31**, then uses the same forward-move/sound/own
destination-wait/idle body. Those scripted moves do not repeat layout/occupancy checks, and there
is no extra boarding-style wait6. With consistent mappings, settled departure coordinates, normal
facing and completed services, Bowie/followers stepping away while the raft remains behind is
**Inferred successful landfall**, rather than a demonstrated natural landing.

Logical FOLLOWER_D159, physical raft31 and logical FOLLOWER_B31 are different identities. Map-command
waits resolve the target lookup and poll that actor's idle action pointer; they have no ally-HP
guard. Bowie's sprite replacement also does not skip HP0. Missing or aliased lookups can target
main0 or another actor while follower clones still read physical31. Cloning/sprite replacement
does not repair mappings, membership or HP, nor reset every action/velocity field. Disembark heals
or revives nobody.

The exploration list contains physical indices ending in `$FF`. Fresh population declares main0
separately, so the helper name does not prove that main is listed. An empty list returns immediately;
otherwise **the stop wait checks only the last listed actor**, not everyone. Restoration then scans
current follower flags and writes follow pointers into consecutive physical slots from1. It does
not rebuild the list or resolve each logical lookup. Stable flags/population can make these
authorities agree; changed flags/lists can diverge, and earlier actors may have their still-running
exit actions overwritten. The final mode0 write proves neither all-actor landing nor usable control.

Partial execution can save the tuple, copy the raft, restore Bowie's sprite or move followers while
mode remains2. A later fresh mode0 population declares the separate raft only if F64 is set and
the saved raft map matches the current map. Reboarding still requires the separate accepted
layout/F64 producer and physical destination alignment. Neither disembark nor the saved tuple
automatically boards again; this guide does not generalize other vehicle consumers.

## The Selected Hassan Exit

**Confirmed request:** Map69 row2 matches `(26,55)` and resolves `MAP_HASSAN` to **Map9**,
requesting no-scroll `(14,1)`, Down. None of the other seven rows competes at that point;
the matcher uses first-match priority and negative-coordinate wildcards.

Ordinary walking opcode2 and actual raft opcode8 can both call the working-layout `$1000` warp
producer after their own movement checks. Opcode8 handles this before testing type5 and continues
its movement tail. The row/producer tests neither completion/F518, F64, membership nor transport
mode. **Disembark is not an explicit prerequisite of this request.** Natural water/land geometry
and reachability remain Unknown; Map69 return `(29,51)` is not a travel receipt to `(26,55)`.

This request includes warp sound and **does use ordinary exploration type1 consumption**: clear
pending, request/clear sound, idle main0, unwind exploration, sample player coordinates, replace
nonnegative X/Y and use supplied facing. It returns `(9,14,1,Down,result0)` to `MainLoop/@Start`.
It preserves PLAYER_TYPE and RAFT_MAP/X/Y. A separately supplied nonzero parameter1 would take
scrolling instead of this row's no-scroll route.

Map9 has no switch-table replacement and **no battle-map row**. CheckBattle therefore admits no
battle here, regardless of supplied unlock flags. Hassan entry does not itself start Battle17/18.
The [request owner](../../../../research/chapters/southern-travel-creed/battle16-outcomes-through-hassan-entry.md#selected-map69-to-hassan-request)
retains the complete row comparison and symbol-to-number join.

## Hassan's Cast and Conditional Control

**Confirmed fresh lifecycle:** exploration now uses Map9 and `(14,1)`, heals eligible allies,
rebuilds physical entities/all64 lookup bytes, clears temporary flags and sets F80, preserving
transport mode. F518 chooses the default `ms_map9` or alternate `ms_map9_flag518`; their entity
lists differ, but both use `ms_map9_InitFunction` at `0x568C4`. Selection and init re-read flags,
so changing supplied state need not form one coherent snapshot.

Hassan uses the normal follower table even with F65, because it is not an overworld or excluded
map. Flags/mapsprites select followers; this is not a general party-membership test. Population
declares followers, setup actors, then main0, reusing an existing nonzero ally lookup instead of
duplicating it. No entry operation joins Rick or Rohde.

Both setup lists have ten non-ally rows mapped consecutively to logical128–137 and a Rohde11 row
at `(16,59)`, Down. Default has11 setup rows; F518 alternate has12, adding Rick12 at `(12,3)`,
Right. Merchant137 is last in both, at `(16,3)`, Down. The alternate also changes a warrior's
walking placement, priest facing/action and a young man's walking origin. This cast explanation
identifies init targets, rather than supplying a dialogue/service inventory.

The complete thirteen-operation init has three ordered conditional movement groups and return:

| Operations | Condition and effect | Actor/membership limit |
| --- | --- | --- |
| 0–3 | F518 clear: move logical137 out of map. | The target is the merchant, not Rick. F518 set leaves it alone. |
| 4–7 | F65 set: move mapped Rohde11 out of map. | F65 is Caravan unlock, not his membership flag. F84 following can supply the reused follower lookup, which is then moved instead of the fixed setup actor. |
| 8–11 | F12 set: move mapped Rick12 out of map. | F12 is his joined flag. Rick is absent from the normal follower table and declared only in the F518 alternate. Fresh F518-clear/F12-set leaves lookup12 zero, so the call targets **physical main0**, rather than safely skipping Rick. No natural occurrence of this inconsistent state is claimed. |
| 12 | Return. | No dialogue, warp, savepoint selection, battle flag or transport write. |

`MoveEntityOutOfMap` resolves the lookup and writes `$7000` to position **and destination**.
It does not delete/hide the actor, remove a lookup/list entry, alter HP/membership/flags, stop
actions or clear travel/velocity. The helper validates neither presence nor HP. This internal
off-map placement is not an observed permanent disappearance.

Transport population precedes init. Mode2 again gives main0 the raft sprite, places listed
followers at `(64,64)` standing and declares physical31 there regardless of F64/saved map. Mode0
uses Bowie's sprite and declares a separate raft only with F64 plus RAFT_MAP=9; a tuple saved on69
does not qualify. Neither path changes the saved tuple or normalizes mode. Init may then move a
mapped actor, including main0 in the supplied Rick case.

After init, conditional music/fade and `WaitForEvent` follow. With no pending event, physical0 gets
walking control in mode0 or the opcode8 controller in mode2. Other supplied modes retain the shared
selector's behavior. Returned init, an off-map main or incomplete services cannot establish usable
walking. The [population/control owner](../../../../research/chapters/southern-travel-creed/battle16-outcomes-through-hassan-entry.md#fresh-hassan-population-init-and-conditional-control)
retains exact operations and actor rules. This guide stops before any town input or interaction.

## Evidence Trace and Remaining Questions

**Confirmed provenance:** accepted #824/PR826 research binds SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and the registered USA retail ROM. Its
[reproduction owner](../../../../research/chapters/southern-travel-creed/battle16-outcomes-through-hassan-entry.md#reproduction-and-evidence-limits)
names source bodies, selected records and the unchanged retained comparison recipe. This guide
consumes that accepted review without source reinvestigation, payload rebuilding or runtime acquisition.

| Boundary | Existing contract and exact fixture trace | Coverage limit |
| --- | --- | --- |
| Outcome, leader and after/join wrappers | [battle lifecycle](../../../contracts/battle/battle-control-lifecycle.md), [cutscene routing](../../../contracts/battle/battle-cutscene-routing.md); `sf2-battle-loop-static-v1`, `sf2-battle-control-static-v1`, `sf2-battle-cutscenes-static-v1`, `sf2-battle-global-data-static-v1`, `sf2-battle-routing-data-static-v1` | Shared lifecycle/data facts; selected row16, head identity, Polca savepoint/reset and direct return composition are the research owner's joins, not a composed outcome fixture. |
| After scene and disembark | [map exploration](../../../contracts/world/map-exploration.md); `sf2-map-script-engine-static-v1` | Tracked command facts select `abcs_battle16`/`cs_45470`; they do not contain the full programCorpus. Research checked all3/4 retained records and separate action/subroutine consumers. This is no natural landing or all-actor receipt. |
| Warp, cast/init and following | [exploration flow](../../../contracts/world/exploration-control-flow.md), [entry routing](../../../contracts/world/map-entry-routing-state.md), [roster](../../../contracts/party/party-roster-state.md); `sf2-map-event-request-consumption-static-v1`, `sf2-map-setup-static-v1`, `sf2-map-entities-static-v1`, `sf2-map-init-static-v1`, `sf2-common-stats-static-v1` | Tracked init has all13 operations and both route/pointer joins. Hassan row/enum, setup actor lists and missing-Rick mapping consequence remain accepted research joins, not per-case natural-input coverage. |

The IDs name matching files under `tests/fixtures/h2/`, without the `sf2-` prefix. Owning commands
include `uv run sf2 h2 battle-loop`, `battle-cutscenes`, `map-script-engine`, `map-init`, `map-setup`
and `common-stats`. The [map-script verifier](../../../../../src/sf2tool/h2/map_script_engine.py) and its
[schema composition](../../../../../src/sf2tool/h2/map_script_schema.py) compare the command-fact projection,
while the [map-init verifier](../../../../../src/sf2tool/h2/map_init.py) and
[fixture schema](../../../../../schemas/h2-map-init-static-fixture.schema.json) retain complete operations
and joins. The [cutscene verifier](../../../../../src/sf2tool/h2/battle_cutscenes.py) owns shared wrapper facts.
This prose review inspected bounded tracked command sites, all13 init operations,
both joins and shared outcome/request facts, plus their schema/verifier boundaries. No aggregate
fixture was promoted into a composed natural journey. Any controlled reproduction loads the owning
worktree's ignored private-input configuration in the same process.

The [PR826 final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/826#issuecomment-6160352020)
preserves completed comparisons, final CI and the earlier corrected enemy-macro assertion/inspection
failures through its [full handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/826#issuecomment-6160316137).
[PR822](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/822#issuecomment-6160096072) retains
its checks, inspection/commentary corrections and rejected-command history;
[PR818's initial destination failure](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159788942)
and [corrected final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159852108)
retain their actual statuses. None is replayed or replaced by this documentation slice.

**Unknown:** natural outcome/travel/landing and caller mode, flags, HP, membership, mappings, lists,
layout/occupancy/input; all-actor/service completion, original rendering/audio, usable Hassan input
and save/resume/persistence. These are deferred limits under
[ADR 0014](../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) and
[ADR 0016](../../../../decisions/0016-remake-start-evidence-deferral.md), not an automatic acquisition queue.
