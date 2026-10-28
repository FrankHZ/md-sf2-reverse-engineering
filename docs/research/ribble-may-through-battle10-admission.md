# Ribble's optional May recruitment through Battle10 admission

Evidence date: 2026-10-13. **Confirmed:** after the accepted Ribble mayor caller sets
F708, May has a flag-gated interaction. F8 clear runs a linear recruitment script;
the invitation is not a yes/no choice. Joining, active-party membership, AI control
and the current follower entity are separate state changes. Ribble's south warp and
one Map69 east scrolling edge provide a conditional Map8→69→70 connection. Map70's
default zone can request Battle10 admission, but the request is partial and ordered
battle selection remains authoritative. **Inferred:** this is a narrative continuation
toward the east. **Unknown:** natural traversal, actual actor/state availability,
service completion and continuous playability.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #760](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/760), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Interaction, transient entity state and deferred map/battle admission need explicit
cross-owner boundaries. Sole tracked owner: this document. Starting accepted base:
`493e674f1733d01690f740a8f02e13ee226bfd6e`; no stacked dependency or shared-path write.
Start after the [Ribble welcome](./battle09-outcomes-through-ribble-welcome.md)
mayor caller/control boundary. Select only May, the directly relevant directions
conversation, Map8 south exit, Map69 east edge and Battle10 opening. Stop at the
start-dispatcher return before `ActivateEnemies`. Combat/outcomes, East Shrine,
cave/Battle11, other routes, unrelated town content, translation, design, engine/UI
and runtime acquisition are excluded. This is original-game research, not remake acceptance.

Source paths are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM`, pinned `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind the pin and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Their fixture comparisons and the existing cutscene-source comparison pass; no new
extraction or observation is claimed. Fnnn denotes flag nnn; battle IDs are zero-based.
Map8 setup files are under `data/maps/entries/map08/mapsetups/`. Script record numbers
below are one-based indices in retained `programCorpus.programs[].commands`, including
sleep and terminator records. Map-event `programOrder` and init `sourceOrder` remain zero-based.

Acceptance is bounded source/row/handler/list/fixture review, retained comparisons,
document/link/private/diff checks, clean committed planner and actual final-head CI.
The reused payloads total 18,285,979 bytes: one process, linear selected reads, an
estimated peak below 512 MiB and one-minute comparison budget; only prose and small
ignored receipts are produced. No comparator, fixture or capture platform is needed.
Reassess before expanding routes, data or semantics. Unknowns are not an acquisition queue.

## May's interaction and its caller

**Confirmed:** `data/maps/mapsetups.asm` selects `ms_map8_flag708` when F708 is set;
otherwise `ms_map8` supplies only the original two NPC event rows and default return.
The selected `pointertable_708.asm` points to `ms_map8_flag708_EntityEvents` in
`s2_entityevents_518.asm`. The filename does not encode an F518 condition.
Its thirteenth explicit row is `ALLY_MAY,UP,Map8_2C4_EntityEvent12` (source line 17).
The population table `s1_entities_708.asm` places May in a random-walk row centered
at `(18,27)`, Down, radius operand 2. This is an authored origin, not an observed location.
The [predecessor](./battle09-outcomes-through-ribble-welcome.md#map8-setup-actors-and-independent-flag-lifetimes)
owns follower-first population, ally/event-ID mapping and the mayor's explicit reload.
F708 lookup does not itself reload the current entities.

The successful entity path in `code/gameflow/exploration/explorationvints.asm`
calls `GetActivatedEntity`, then `GetEntityEventIndex`, then `j_RunMapSetupEntityEvent`.
Activation depends on the actual forward position/layout and current entities; the
scan excludes the controlled entity and current followers. The
[interaction owner](./map-event-interaction-state.md) retains physical-to-event identity
and the returned actor/player facing values. Character 8 is May's lookup identity,
not physical entity slot 8.

`code/common/scripting/map/mapsetupsfunctions_1.asm:RunMapSetupEntityEvent` matches the
event ID, not the row's facing value. The second byte is consumed as control bits:
`UP=1` sets bit 0, so the actor is turned opposite the incoming player facing before
the handler; bit 1 is clear, so the actor's old facing is not restored afterward.
This is not a requirement to approach May from the north. The dispatcher stores the
incoming player facing in `EVENT_RELATIVE_POSITION`, handles portrait/speech state,
disables entity updates around the direct handler and restores updates at its tail.
The `script` macro invokes `trap6_mapscript.asm:Trap6_TriggerAndExecuteMapScript`, which
activates entity updates before `j_ExecuteMapScript`. The event tail closes windows;
the exploration loop can reinstall control at `WaitForEvent` if pending is zero.
That caller return does not prove every entity action or service completed naturally.

`s2_entityevents.asm:190..210` is the complete bounded recruitment surface:
`Map8_2C4_EntityEvent12` checks F8 and branches to the empty default return when set.
Otherwise it runs `cs_56146` (`0x56146`), then returns. There is no F509, capacity,
Peter, route or choice test in this event body.

| Script records | Ordered behavior |
| --- | --- |
| 1–3 | Install/wait for May's `eas_Init`, then `eas_StopMoving`, then `faceEntity MAY,BOWIE`. The face handler uses destination coordinates, favoring vertical facing on equal absolute distances. |
| 4–5 | Set text cursor 1347 and request one May textbox. `gamescript.txt` key `0543` expresses her wish to adventure and invitation to join. |
| 6 | `join ALLY_MAY` invokes the ordinary join handler, including its music, join announcement and input-wait services. |
| 7–9 | Request the next May textbox (1348 / `0544`, departure encouragement), `addNewFollower ALLY_MAY`, then terminate. |

The corpus has eight commands and one terminator, with no choice, branch, map request,
`joinForceAI` or explicit follower-flag write. Text sequence assumes the ordinary
non-skipped text path; `csc00_displaySingleTextbox`'s skip path does not increment the
cursor. Invitation wording and text confirmation waits do not create a consent branch.
Recruitment is optional because this interaction is not required by the selected travel
or encounter predicates, not because this script offers a decline response.

## Membership, active capacity, AI and current following

**Confirmed:** the [common-scripting owner](./common-scripting.md) retains the join and
active-party/AI/follower handler families and their bounded Map Test observations.
For this source composition, `mapscriptengine_2.asm:csc08_joinForce` resolves
`j_JoinForce` through the S02 jump interface to `code/common/stats/battleparty.asm:JoinForce`.
With `ALLY_MAY=8`, joined flags starting at 0, active flags starting at 32 and
`FORCE_MAX_SIZE=12` (`sf2enums.asm`), its order is:

1. Set F8, then `UpdateForce` materializes joined/active/reserve lists and counts.
2. If the resulting active count is below 12, `JoinBattleParty` sets F40. At 12 or
   above it skips that call: membership still joined, with no forced replacement.
3. Return without another `UpdateForce`. With initially clear F40, a successful
   activation is not yet reflected in the just-built active list/count; a later
   update, including battle placement initialization, rebuilds them.

A full party does not cancel the remaining text or follower command. Pre-existing
F40 is not cleared by the capacity branch; a supplied F8-clear/F40-set state must not
be described as newly inactive. None of these join/follower handlers writes May's
activation bitfield or invokes `joinForceAI`; existing AI state is not established or
cleared by this recruitment. Joining does not test HP or revive May. Party membership,
living deployment and visible following must therefore remain distinct.

`mapscriptengine_1.asm:csc56_addFollower` (`0x47008`) is the actual consumer of
`addNewFollower`, not the consumer of `followEntity`. It resolves May through
`GetEntityAddressFromCharacter`, scans the sentinel-terminated `EXPLORATION_ENTITIES`
list and selects the last physical follower as target, or physical entity 0 if empty.
It passes `(target,-24,0)` to `code/common/scripting/entity/entityfunctions_2.asm:AddFollower`.
The latter installs a newly copied 42-byte `eas_Follower1` action block on the resolved
entity, advances `ENTITY_WALKING_PARAMETERS` by 42, patches three words at offsets
`0x1E/0x20/0x22`, then appends the physical entity index unless already present.
A duplicate still allocates/replaces the block before the duplicate-list check;
there is no capacity guard in this bounded consumer. The 42 bytes describe the copied
action block, not a follower-list capacity.

`data/scripting/entity/eas_main.asm:eas_Follower1` consumes those words through
`ac_follow`, waits for destination and loops. `entityscriptengine_2.asm:esc03_follow`
uses a physical target entity, its position/facing/destination and the two offsets;
it includes a distance gate and layout-dependent destination fallbacks. Thus this is
a current following relation, not an immediate teleport or a fixed visible spacing
guarantee. With other followers, the target is the actual list tail, not necessarily
Peter or Bowie. The already-following case can even select itself if it is the tail;
normal occurrence and visible behavior of that supplied state remain Unknown.

There is no HP guard in `csc15_setEntityActscript`, `csc52_faceEntity`, `csc08` or
`csc56`. The first two waits poll until the installed actscript reaches `eas_Idle`;
`eas_StopMoving` requests zero relative movement then jumps there. They do not prove
a living or present May. `GetEntityAddressFromCharacter` uses the mapping byte without
an absent-entity check; a zero mapping resolves slot 0. Missing mappings must not be
reported as safe skips, and a forced direct invocation with dead/missing May is not
proof of a natural recruitment. F8 already set returns before all nine script records.

**Confirmed static lifetime:** `EXPLORATION_ENTITIES` (`0xFFAF22`), walking pointer
(`0xFFB1A0`) and dynamic action storage starting at `0xFF5600` are outside the logical
SaveGame/LoadGame domain
`0xFFE800..0xFFF7AF` retained by the common-scripting persistence owner. F8/F40 are
game flags within that domain. The accepted join save/load observation is a bounded
handler case, not a May story/save lifecycle observation.
`mapfunctions.asm:InitializeMapEntities` calls `ClearEntities`, rebuilds flag-selected
followers and the fixed entity list, and writes the new follower sentinel. Clear resets
entity mappings and walking allocation. `data/scripting/entity/followers.asm` has no
May row and does not turn F8 into a follower predicate. Consequently ordinary supplied-map
entry does not recreate May's transient following from this command.

In Ribble, F8 does not remove the fixed authored May row at population time.
`s6_initfunction.asm:ms_map8_InitFunction` (retained body 12, `0x563A2`) subsequently
checks F8 and calls `MoveEntityOutOfMap(MAY)` to write `(0x7000,0x7000)` through the
current mapping. Recruitment itself performs neither this init nor a population reload:
the current May can follow until a later relevant consumer. An explicit entity reload
rebuilds the list without running init; a later ordinary Map8 entry rebuilds and then
runs init. The predecessor's F708-clear/default-population and missing-mapping caveats
still apply. Cross-save/reload presentation and follower capacity remain Unknown.

## Directions are context, not a travel gate

**Confirmed:** `cs_56172` (`0x56172`; source lines 211–224) has twelve commands,
one 57-frame sleep record and a terminator. Text cursor 1332 supplies five text requests
(keys `0534..0538`) with speakers 135,134,134,135,135. The actors jump and face each
other, then Down while describing an eastward journey, a northward river route and
a dark cave on the way to Mt. Volcano. This is dialogue meaning, not a proof of map edges.

The two participating actors are the worker at `(7,11)` (event 135) and merchant at
`(5,11)` (event 134) in `s1_entities_708.asm`. Their event rows both carry control byte 7.
The exact caller distinction matters: only event 135 → `Map8_2C4_EntityEvent7` calls
`cs_56172`; event 134 → `Map8_2C4_EntityEvent6` selects the weapon shop and jumps to
`j_ShopMenu`. There are two speakers but not two direction-script callers. Shop internals
are excluded. This script sets no route/recruitment flags and requests no map transition;
neither hearing it nor recruiting May is a tested gate in the selected route below.

## Selected Map8→69→70 connection

**Confirmed:** the only selected Map8 exit is the first row in
`data/maps/entries/map08/6-warp-events.asm`: wildcard X, Y31, no scroll,
Map69 (`MAP_OVERWORLD_SOUTH_PARMECIA`), `(44,8)`, Down. Ordinary movement must encounter
the warp layout attribute before `exploration.asm:WarpIfSetAtPoint` matches this row.
It writes type 1, the complete parameter tuple and warp SFX. The no-scroll consumer
in `explorationfunctions_2.asm` idles main, exits the exploration frame and returns
the tuple to `MainLoop`. Surviving request state and successful services are conditions,
not a claim of collision-free access to any X on that row.

Neither the row nor that consumer checks F8, F708, F709 or the directions conversation.
`data/maps/global/flagswitchedmaps.asm` contains no Map8/69/70 substitution.
Main calls `CheckBattle` before ordinary Map69 entry, so carried F409 can select
Battle09, or F417 can select Battle17 if F409 is clear. The
[accepted Map69 owner](./new-granseal-departure-through-battle09.md#map69-zone-partial-reload-and-battle09-selection)
owns their ordered/completion behavior. If neither qualifies, supplied-map exploration
sets current map/not-in-battle, rebuilds entities and flag followers, clears temporary
flags 256–383 and sets F80 before map load/init. Persistent F8/F40/F67/F708/F709 and
battle flags are not cleared by that temporary reset. Map69's fixed list is empty and
its init is `rts`; May's transient follower is not carried through this rebuild.

Map69 movement remains conditional: X23/24/25 zone rows call the Battle17 helper;
the default requests Battle09 only with F509 clear. F509 set does not clear an
already-set F409. These inherited interruptions preclude an unconditional walk claim.
No Battle09/17 content is re-audited here.

The selected next row is `data/maps/entries/map69/6-warp-events.asm`'s X63, wildcard Y,
scroll Right → Map70 (`MAP_OVERWORLD_SOUTHEAST_PARMECIA`), `(1,255)`, Right.
Its type-1 consumer takes the nonzero parameter-1 branch: set `CURRENT_MAP=70`, call
`ProcessMapTransition`, replace main X with tile 1 and preserve Y because destination
Y is negative. `code/common/maps/mapload.asm:ProcessMapTransition` loads the relevant
tiles/layout and applies the scrolling transition. It does not call ordinary entry,
`InitializeMapEntities`, `RunMapSetupInitFunction`, `SwitchMap` or `CheckBattle`.
The caller repositions raft/followers through `DeclareRaftEntity` and returns to the
same exploration loop; it does not consume parameter 5 as a new facing assignment.
The row writes Right to that retained parameter byte, while actual facing remains
dependent on the entity/control path. No temp-flag reset or ordinary-entry healing is
implied by this scrolling edge.

Map70 has only the default setup, an empty fixed list and `ms_map70_InitFunction`
at `0x4FE42` (retained body 77), just `rts`. There is no story-flag setup discriminator
or init-triggered Battle10 script to add. This init is relevant to a later ordinary
entry, not invoked merely by the selected scroll. In particular a pre-existing F410
does not trigger a battle on the scroll itself.

## Map70 zone requests and ordered battle admission

**Confirmed:** `data/maps/entries/map70/mapsetups/s3_zoneevents.asm` (`0x4FDFE`)
matches X16/17/18/19, wildcard Y, to `Map70_ZoneEvent0`; only the default reaches
`Map70_DefaultZoneEvent`. Specific matches do not fall through if their helper declines.
The ordinary zone producer still requires layout attribute `$1400`, not just a coordinate.

| Selected handler | Producer behavior |
| --- | --- |
| Specific X16–19 | `CheckRandomBattle(BATTLE_SOUTHEAST_DESERT=14)`: F514 clear requests immediately; if complete, nonzero step counter declines, otherwise RNG range 8 yielding zero requests. A nonzero draw takes the range-4 draw plus 2 cooldown. A request sets F414, writes long `$100FF`, sets step counter 30000, then waits/plays boost SFX/flashes. |
| Default | F510 set returns without writes. F510 clear sets F410 then writes long `MAP_EVENT_RELOADMAP=$100FF`. No recruitment, intro-flag, step-counter or RNG test. |

The [random-battle owner](./map-event-random-battle-state.md) owns the helper gates;
the [request owner](./map-event-request-consumption.md) and
[accepted partial-reload lineage](./new-granseal-departure-through-battle09.md#map69-zone-partial-reload-and-battle09-selection)
own consumption. The ordinary type-6 producer (`entityscriptengine_2.asm:loc_5294`)
puts word X in parameters 1/2 and word Y in 3/4. Its consumer initializes main's
actscript and invokes `RunMapSetupZoneEvent`, which closes windows, waits a VInt and
waits for physical entity 0 to stop after the handler. It does not clear a newly
written request. This slot is mutable, not a queue.

The long `$000100FF` write at `MAP_EVENT_TYPE=0xFFA84A` covers type word 1,
parameter 1=0 (no scroll), parameter 2=255 (`MAP_CURRENT`). Parameters 3/4/5 at
`0xFFA84E/4F/50` survive. The type-1 consumer samples main X/Y, overrides either
with its corresponding nonnegative parameter byte and always uses parameter 5 as
facing. For an intact ordinary zone lineage with Y in 0–63, parameters 3/4=`00,Y`:
the returned tuple is `MAP_CURRENT,(0,Y),retained parameter 5`. It is not a guaranteed
reload at main's sampled position/facing. The selected preceding scroll wrote Right
to parameter 5, but only absence of intervening writers preserves that value.

After this no-scroll return, main performs `SwitchMap` then
`code/common/maps/getbattle.asm:CheckBattle`. For Map70 the only competing rows in
`data/battles/global/battlemapcoords.asm` are:

| Battle row | Area output `(X,Y,width,height)` | Trigger X/Y | Unlock / completion |
| --- | --- | --- | --- |
| 10, `BATTLE_TO_THE_EAST` | `(0,0,32,32)` | `(255,255)` | F410 / F510 |
| 14, `BATTLE_SOUTHEAST_DESERT` | `(15,26,32,32)` | `(255,255)` | F414 / F514 |

The function checks map, unlock and trigger bytes in row order; area bounds are
outputs, not containment tests. Thus F410 set selects 10 even when F414 is set or
the supplied coordinates lie outside row 10's area. F410/F510 both set clears F410
but still returns row 10 on that same call: completion does not skip to row 14.
F410 clear permits row 14 if F414 is set; neither unlock gives no battle. The default's
F510-set return does not clear an existing F410. A Battle14 helper request can therefore
select Battle10 when F410 was already set. Conversely, a specific-row non-request does
not perform admission merely because an unlock flag exists. Helper argument, pending
request and selected battle are three separate boundaries.

## Battle10 opening and stopping boundary

**Confirmed:** main passes selected index 10 to
`code/gameflow/battle/battleloop_1.asm:BattleLoop`. It clears `PLAYER_TYPE`, sets F399
and checks F88. With F88 clear, it clears elapsed seconds, establishes map/battle/base
VInts, calls the before wrapper, restores map/battle, clears region flags 90–105,
heals living/immortal allies, initializes ally then enemy positions, clears AI memory,
loads the battle for combatant 0 and calls the start wrapper. Stop at that return,
immediately before `ActivateEnemies`; no combat, activation, turn or outcome is admitted.

Row 10 in both `data/battles/cutscenes/beforebattlecutscenes.asm` and
`battlestartcutscenes.asm` resolves to `ms_Empty`. The wrappers in
`code/gameflow/battle/cutscenes/beforebattlecutscenesstart.asm` and
`battlestartcutscenesstart.asm` use intro base 450 + 10 = F460. Before dispatches only
if clear without setting it; start sets it before dispatch if clear. F460 already
set skips both scripts but not new-battle initialization. Empty scripts still execute
the shared interpreter terminator, with its conditional scrolling wait and speed clear.
F510 completion is a separate predicate. The
[battle-cutscene owner](./battle-cutscenes.md) owns wrapper/tail semantics.

`data/battles/spritesets/spriteset10.asm` (`0x1B3AA8`) declares twelve ally placement
records, not twelve required or joined allies. `battleloop/initializecombatants.asm`
resets ally X positions, calls `UpdateForce` and traverses the actual active list.
Dead members skip placement; list progress and placement-pointer progress are distinct,
and the source's remaining-tail branch can call `LeaveBattleParty`. May's F8 alone does
not establish F40, HP, a deployment slot or player control. The current exploration
follower list does not select battle membership. The existing
[battle loop](./battle-loop.md) and [spriteset owner](./battle-spriteset-data.md) own
those consumers; no roster or battle AI audit is added here.

F88 set takes the alternate resume path: restore saved seconds, clear F88/AI memory
and load saved battle state, bypassing new placement and both intro wrappers on this
entry. It branches to individual turns rather than visiting the named start-dispatcher
return; this is a discriminating alternative, not a passed instance of this endpoint.
The selected route and empty intros do not establish natural admission or service completion.

## Existing coverage, reproduction and remaining Unknowns

Static fixture names below are relative to `tests/fixtures/h2/`; runtime fixtures use
explicit `h3/` paths beneath `tests/fixtures/`. Named static commands have
prefix `uv run sf2 h2`; listing a command identifies its owner, not a fresh execution.

| Relation | Retained evidence and direct review | Missing composition / discriminating controls |
| --- | --- | --- |
| F708 actor/event and May script | `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-events/entity-programs.json` orders 664/669, `map-script-engine-static-v1.json` programs `cs_56146`/`cs_56172`; commands `map-setup`, `map-entities`, `map-events`, `map-script-engine`. | Actual activation/facing/mapping and waits; F708 clear, F8 set, absent/dead mapping, skipped text and two speakers versus one directions caller. |
| Join, capacity, AI and follower | `common-stats-static-v1.json`, map-script `forceStateCommandFacts`, `h3/force-state-roster-death-v1.json`, `h3/force-state-active-party-v1.json`; common-scripting owner. Direct `JoinForce`, `AddFollower`, `esc03_follow`, rebuild/table/save-span review. | May's composed story/save lifetime; active count 12, stale list after F40 write, pre-existing F40/AI, empty/other/duplicate followers, reload without init versus ordinary init. No new H3 run. |
| Map8→69→70 request and setup | `map-data-static-v1.json`, `map-init-static-v1.json` bodies 12/76/77, `map-event-request-consumption-static-v1.json`; commands `map-data`, `map-init`, `map-event-request-consumption`. Direct selected warp/scroll/main consumers. | Legal path/layout, request survival, carried F409/F417, ordinary rebuild versus scroll, retained Y and facing, current/absent May mapping. |
| Map70 producer→battle selection | `map-events/zone-programs.json` orders 132/133, `map-event-random-battle-state-static-v1.json`, `common-maps-static-v1.json`, `battle-global-data-static-v1.json`; commands `map-events`, `map-event-random-battle-state`, `common-maps`, `battle-global-data`. | Executable continuous composition absent; F510-clear default, F510/F410 both set, specific-row decline, F410/F414 competition, overwritten pending or parameter bytes, outside-area coordinates. |
| Battle10 before/start/placement | `battle-cutscenes-static-v1.json`, `battle-routing-data-static-v1.json`, `battle-spriteset-data-static-v1.json`; commands `battle-cutscenes`, `battle-routing-data`, `battle-spriteset-data`. Exact row targets read directly, not inferred from routing's `routeTargetsParsed=false` summary. | Actual entry, living/active list and service completion; F460 skip, F88 alternate and unjoined/inactive/dead May. No combat/outcome claim. |

Load `. ./local/private-inputs.ps1` in the owning checkout and select the registered
read-only `$upstream`; check its `git rev-parse HEAD` and `git status --short` against
the pin above. Run the existing
[retained payload/cutscene comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Select `cs_56146`/`cs_56172` by `entryLabel`; compare each record's `sourceLine`, macro
and operands with the pinned source. Read the exact event/zone orders and init bodies
above, then the named list, handler, warp and battle rows together. Resolve text by
four-digit hex keys in `data/scripting/text/gamescript.txt`; do not infer branches
from dialogue. Reproduction needs no full transcript, new fixture or source extraction.

**Unknown:** collision/layout and natural route reach; actual activated actors,
physical mappings/HP/party/AI/follower state; entity, text, input, scrolling and request
completion; cross-save follower presentation/capacity; and playable continuous entry.
Existing handler observations and separate fixtures do not close these relations.
Keep them explicit without promoting them to an emulator queue.

Record direct content/source/link/private/diff checks, completed failures, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI in the
Issue/PR handoff. No normal/full/engine/H3/H4/Godot suite is selected solely for this
prose change. Preserve retained evidence and leave a clean pushed Draft PR, Project
Review / Verify and successful authorized completion callback for independent review;
no self-merge, cleanup or next-slice selection.
