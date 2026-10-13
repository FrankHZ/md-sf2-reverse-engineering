# Ribble's Optional May Recruitment through Battle10 Admission

After Ribble's welcome, the party can meet May, hear travel advice and leave town. May's
invitation leads directly into joining and current following; the selected departure and encounter
rules do not require that conversation. The eastward connection then crosses two different map
lifecycles before a zone request can admit Battle10.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/ribble-may-through-battle10-admission.md) supports the interactions,
state changes and selected Map8→69→70 connection below. **Inferred:** the advice and eastward encounter
form a narrative continuation after the welcome. **Unknown:** legal walking, actual actors and party
state, request survival, service completion and a continuously playable route. This guide supports
content/design planning; it does not establish remake readiness or a played walkthrough.

Begin at the [welcome guide's conditional control handoff](battle09-outcomes-through-ribble-welcome.md).
Stop at Battle10's start-dispatcher return, immediately before `ActivateEnemies`. Battle10 combat and
outcomes, East Shrine, the cave and later content require separately accepted owners. Fnnn means flag
nnn; map and battle IDs retain their source numbering.

## 1. Meeting May Is Optional; Her Script Has No Decline Branch

**Confirmed static interaction:** F708 selects Ribble's expanded setup and its May event row. The
authored wandering origin is near `(18,27)`, facing Down, with radius operand 2; it is not an observed
position. Setting F708 alone does not rebuild the current population. The welcome's explicit entity
reload and a later ordinary entry are distinct ways of obtaining a population, as the predecessor
explains.

An ordinary interaction requires an activated current entity, a valid mapping and the event dispatch
path. Current followers and the controlled entity are excluded from the activation scan. May's
character identity 8 is not physical slot 8. The event row's `UP=1` byte controls facing: turn the
actor opposite the incoming player facing before the handler and do not restore her old facing
afterward. It does not require approaching from the north, and row selection matches event identity,
not this facing byte.

`Map8_2C4_EntityEvent12` returns without running the scene when F8 is set. With F8 clear it invokes
`cs_56146`. There is no local Battle09-completion, Peter, party-capacity or travel prerequisite in that
handler. The scene waits for May's initialization and stop-moving actions, faces her toward Bowie's
destination, then requests her invitation textbox. It invokes ordinary `join ALLY_MAY`, including the
join announcement/music/input services, requests her departure encouragement and installs
`addNewFollower ALLY_MAY` before terminating.

The invitation and encouragement are text requests 1347/1348 (keys `0543`/`0544`), not a choice menu.
The accepted script has no yes/no, decline, branch or map request. **Confirmed static consequence:**
recruitment is optional because the selected travel and admission predicates do not test that
interaction; once this F8-clear handler runs, its source order proceeds through joining. Text
confirmation waits do not create a consent branch. On the text-skip path the cursor is not incremented,
so those two resolved messages describe the ordinary non-skipped path.

The event dispatcher temporarily disables entity updates, while the script trap enables them for
execution. Its tail closes windows and restores updates. Exploration can reinstall ordinary control
at a pending-zero `WaitForEvent` read, conditional on the services returning. The waits do not prove
May's presence or HP, natural scene completion or responsive input. A missing character mapping can
resolve physical slot 0; it is not a safe skipped interaction. A forced missing/dead invocation is
not evidence of natural recruitment.

## 2. Joining, Deployment and Following Are Different Results

**Confirmed source composition:** the ordinary `JoinForce` consumer processes May's membership in
this order:

1. Set joined flag F8, then `UpdateForce` builds the joined/active/reserve lists and counts.
2. If the resulting active count is below 12, `JoinBattleParty` sets active flag F40. At 12 or above,
   skip that call; joining still succeeds without forcing another member out.
3. Return without another `UpdateForce`. For initially clear F40, the new active bit is not yet
   reflected in the list/count just built. A later update, including battle placement, rebuilds them.

A full party does not cancel the remaining encouragement or follower command. The capacity branch
does not clear pre-existing F40; an F8-clear/F40-set supplied state is not necessarily inactive.
Joining neither tests HP nor revives May. This script invokes no `joinForceAI` and writes no new
activation/AI bitfield, so it does not establish or clear existing AI control. F8 alone cannot prove
an active, living, player-controlled battle participant.

**Confirmed current following:** `addNewFollower` resolves May's physical entity and targets the
actual last physical follower, or physical entity 0 when the list is empty. The target is therefore
not necessarily Peter or Bowie. `AddFollower` copies a 42-byte looping follower action block, patches
its target and `(-24,0)` offsets, advances walking storage, then appends the physical index unless
already present. A duplicate still allocates and replaces the action block before the duplicate check.
The 42 bytes are an action-block size, not list capacity; this consumer has no capacity guard.

The follow action consumes the target's position, facing and destination through distance and layout
conditions. It is not an immediate teleport or guaranteed visible spacing. A supplied duplicate that
is already the list tail can select itself; natural occurrence and presentation remain **Unknown**.

| Later boundary | What the accepted source establishes |
| --- | --- |
| Recruitment returns in the current population | May can retain the newly installed current following action. Recruitment itself neither reloads entities nor runs Ribble init. |
| Explicit entity reload without map init | Rebuild entities, mappings, walking allocation and flag-selected followers. The persistent follower table has no May row or F8 predicate; it does not recreate this transient relationship. The F8-dependent init hiding is not invoked by this reload. |
| Later ordinary Ribble entry | Rebuild the selected population, then run init. F8 does not remove May's authored row during population selection; init checks F8 and calls `MoveEntityOutOfMap(MAY)` through her current mapping, writing `(0x7000,0x7000)`. The default/F708-clear setup and missing-mapping caveats still apply. |
| Ordinary entry into the overworld | Rebuild the new population and flag-selected followers, rather than carrying May's transient action/list entry. |

There is no persistent May follower flag written by recruitment. F8/F40 lie within the logical save
domain, while the exploration list, walking pointer and dynamic action storage lie outside it. This
storage distinction and the existing bounded join save/load case do not prove May's composed
cross-save presentation, follower capacity or natural reentry behavior. Those remain **Unknown**.

## 3. Travel Advice Supplies Context, Not a Route Gate

**Confirmed static dialogue:** the worker and merchant describe an eastward journey, a northward river
route and a dark cave on the way to Mt. Volcano. Their jumps and facing changes accompany five text
requests starting at 1332 (`0534..0538`), with speakers 135,134,134,135,135. The authored worker is at
`(7,11)` and merchant at `(5,11)`; their event rows carry control byte 7.

Two speakers do not mean two callers. Worker event135 invokes `Map8_2C4_EntityEvent7` and `cs_56172`;
merchant event134 invokes `Map8_2C4_EntityEvent6` and opens the weapon shop. Shop internals are outside
this guide. The directions script sets no recruitment or route flags and requests no map transition.
Neither hearing it nor recruiting May is a tested gate below.

**Inferred narrative meaning:** these conversations give the party a companion opportunity and
context for onward travel. Their wording does not prove map adjacency, navigable paths, river access
or cave entry. The selected map connection comes from the accepted warp/consumer evidence, not from
the dialogue.

## 4. Leaving Town Rebuilds the Party's Exploration Entities

**Confirmed conditional connection:** the selected Ribble exit is Map8's first warp row: wildcard X,
Y31, no scroll, to Map69 at `(44,8)`, Down. Ordinary movement must first encounter the warp layout
attribute; a coordinate match alone does not establish a legal path. The producer writes the complete
type-1 request tuple and warp SFX. If that request survives and the services return, the no-scroll
consumer idles main, exits exploration and returns the tuple to `MainLoop`.

Neither this row nor its consumer tests F8, F708, F709 or the directions conversation. There is no
Map8/69/70 flagswitched substitution. Main checks for battle before ordinary Map69 entry: carried F409
can select Battle09; with F409 clear, F417 can select Battle17. The
[Map69 admission owner](../../research/new-granseal-departure-through-battle09.md#map69-zone-partial-reload-and-battle09-selection)
retains their ordered and completion behavior.

When neither battle qualifies, ordinary supplied-map entry sets the current map/not-in-battle state,
rebuilds entities and flag followers, clears temporary flags 256–383 and sets F80 before map load/init.
F8/F40/F67/F708/F709 and battle flags survive that temporary reset. Map69 has an empty fixed list and
an `rts` init. May's current following is not preserved by this rebuild; her joined membership is a
separate retained fact.

Within Map69, the specific X23/24/25 zones can request Battle17; the default can request Battle09 when
F509 is clear. F509 set does not clear an already-set F409. These inherited interruptions keep this
connection conditional: leaving Ribble does not establish an uninterrupted eastward walk.

## 5. The East Edge Scrolls into Map70 without Battle Admission

**Confirmed static edge:** Map69's selected X63, wildcard-Y row scrolls Right into Map70 at `(1,255)`,
with Right written to the facing parameter. Its nonzero scroll consumer sets `CURRENT_MAP=70`, calls
`ProcessMapTransition`, replaces main X with tile 1 and preserves Y because the destination byte is
negative. It loads the relevant tiles/layout and applies the scrolling transition, then repositions
raft/followers through `DeclareRaftEntity` and returns to the same exploration loop.

This edge invokes neither ordinary entry, entity initialization, map init, `SwitchMap` nor
`CheckBattle`. It does not clear temporary flags or imply ordinary-entry healing. Parameter 5 is
retained but not applied as a new facing assignment here; actual facing depends on the entity/control
path. A pre-existing F410 does not select Battle10 merely because this scroll reached Map70.

Map70 has only its default setup, an empty fixed entity list and an `rts` init. There is no story-flag
setup selection or init-triggered Battle10 scene. That init belongs to a later ordinary-entry path;
the selected scroll does not invoke it.

## 6. A Zone Request and the Battle Selector Decide What Starts

**Confirmed static production:** ordinary zone processing still requires layout attribute `$1400`.
The Map70 table matches X16/17/18/19 with wildcard Y to the specific handler; only a default match
reaches the default handler. A specific helper that declines does not fall through to the default.

| Zone choice | Request behavior |
| --- | --- |
| Specific X16–19 | Ask `CheckRandomBattle` for Battle14. F514 clear requests immediately. When complete, a nonzero step counter declines; otherwise a range-8 zero draw requests, while a nonzero draw establishes a range-4-plus-2 cooldown. A request sets F414, writes `$000100FF`, sets the step counter to 30000 and performs its wait/SFX/flash services. |
| Default | If F510 is set, return without writes. Otherwise set F410 and write `$000100FF`. No recruitment, intro, step-counter or RNG predicate is added. |

The [random-battle owner](../../research/map-event-random-battle-state.md) retains the helper rule;
the [request owner](../../research/map-event-request-consumption.md) retains the mutable pending slot.
The type-6 zone producer places word X in parameters 1/2 and word Y in 3/4. Its consumer initializes
main's actscript and invokes the zone dispatcher, which closes windows and waits for a VInt and
physical entity 0 to stop after the handler. It does not erase a newly written request. This is one
mutable request slot, not a queue.

The long reload write covers type=1, parameter 1=0 (no scroll) and parameter 2=255 (`MAP_CURRENT`);
parameters 3/4/5 survive. The no-scroll consumer samples main's position but replaces each coordinate
with its corresponding nonnegative parameter and always applies parameter 5 as facing. For an intact
ordinary zone lineage with Y in 0–63, the returned tuple is `MAP_CURRENT,(0,Y),retained parameter 5`,
not necessarily main's sampled position/facing. The preceding scroll wrote Right to parameter 5;
only absence of intervening writers preserves it.

After that no-scroll return, main performs `SwitchMap` and ordered `CheckBattle`. Map70's eligible
rows are Battle10 (F410/F510) before Battle14 (F414/F514), both with wildcard trigger coordinates.
Their area rectangles are output placement/view data, not containment tests.

- F410 set selects Battle10 even if F414 is also set or supplied coordinates lie outside its area.
- F410 and F510 both set clears F410 but still returns Battle10 on that same call. Completion does
  not skip immediately to Battle14, and the default's F510-set return does not clear carried F410.
- F410 clear permits Battle14 when F414 is set; with neither unlock, no battle is selected.

Consequently a Battle14 helper request can lead to Battle10 selection when F410 was already set.
Conversely, a specific helper decline does not perform admission just because an unlock exists.
The helper argument, surviving pending request and selector's result are separate decisions.

## 7. Battle10 Has Empty Openings, but Still Has an Intro Gate

**Confirmed new-battle order:** for selected Battle10, `BattleLoop` clears `PLAYER_TYPE`, sets F399 and
checks F88. With F88 clear, it clears elapsed seconds, establishes map/battle/base VInts, calls the
before wrapper, restores map/battle, clears region flags 90–105, heals living/immortal allies,
initializes ally then enemy positions, clears AI memory, loads the battle for combatant 0 and calls
the start wrapper. This guide stops when that dispatcher returns, before `ActivateEnemies`.

The accepted exact row-10 targets in both before/start tables are `ms_Empty`. Both wrappers use F460
(intro base 450 + battle 10): before dispatches only when clear without setting it; start sets it
before dispatch when clear. F460 set skips both scripts, not new-battle initialization. Even an empty
script uses the interpreter terminator, including its conditional scrolling wait and speed clear.
F510 completion is a different predicate.

Battle10's spriteset has twelve ally placement records. That is not twelve required or joined allies.
Placement resets ally X positions, calls `UpdateForce` and traverses the actual active list. Dead
members skip placement; list progress and placement-pointer progress differ, and the remaining-tail
branch can call `LeaveBattleParty`. May's current exploration following does not choose membership or
placement. F8 alone proves neither F40, HP, a slot nor player control.

F88 set takes the alternate resume path: restore saved seconds, clear F88/AI memory and load saved
battle state, bypassing new placement and both intro wrappers. It branches toward individual turns
instead of visiting this start-dispatcher endpoint. It is an explicit alternative, not a passed
instance of this guide's stopping boundary. No activation, turn, combat or outcome is admitted here.

## Evidence Review and Remaining Limits

This guide consumes accepted research from PR #761 and the accepted welcome predecessor from PR #762.
The research binds USA retail to pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`; it owns
exact source/text/warp/table composition and reproduction. No upstream re-investigation, extracted
payload generation or fresh runtime observation is claimed. Direct tracked-fixture and
schema/consumer review found no required Layer A correction; these fixtures remain narrower than a
complete scene or route.

| Reviewed relation | Retained executable/contract boundary |
| --- | --- |
| Interaction, joining and following | [sf2-map-events-static-v1](../../../tests/fixtures/h2/map-events-static-v1.json), [entity-program shard](../../../tests/fixtures/h2/map-events/entity-programs.json) orders 664/669; [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `forceStateCommandFacts` and nested `activePartyCommandFacts`; [party/roster contract](../contracts/party-roster-state.md). Join/addNewFollower source sites use command indices 5/7, distinct from the research's one-based full script records 6/8. These are selected caller/command facts, not the complete invitation or choreography. |
| Capacity, list timing and lifetime | [sf2-common-stats-static-v1](../../../tests/fixtures/h2/common-stats-static-v1.json), [sf2-force-state-active-party-runtime-v1](../../../tests/fixtures/h3/force-state-active-party-v1.json) and [sf2-force-state-roster-death-runtime-v1](../../../tests/fixtures/h3/force-state-roster-death-v1.json); the research's direct JoinForce/follower/rebuild/save-span review. Handler matrices do not establish May's natural actor, capacity or cross-save presentation. |
| Ordinary entry versus scroll | [sf2-map-data-static-v1](../../../tests/fixtures/h2/map-data-static-v1.json), [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json) bodies 12/76/77, [sf2-map-event-request-consumption-static-v1](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json); [map/exploration contract](../contracts/map-exploration.md). Exact selected edges and no-init/rebuild composition remain with research; layout reach and intact request lineage are not proved. |
| Zone requests and row priority | [zone-program shard](../../../tests/fixtures/h2/map-events/zone-programs.json) orders 132/133, [sf2-map-event-random-battle-state-static-v1](../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json), [sf2-common-maps-static-v1](../../../tests/fixtures/h2/common-maps-static-v1.json), [sf2-battle-global-data-static-v1](../../../tests/fixtures/h2/battle-global-data-static-v1.json). Producer/helper/selector facts remain separate, without continuous composition. |
| Empty intro and actual deployment | [sf2-battle-cutscenes-static-v1](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), [sf2-battle-routing-data-static-v1](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), [sf2-battle-spriteset-data-static-v1](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json); [cutscene contract](../contracts/battle-cutscene-routing.md). Routing's `routeTargetsParsed=false` cannot prove exact targets; the accepted research owns the row-10 `ms_Empty` reading. No fixture supplies a played Battle10 opening. |

The [map-script schema composition](../../../src/sf2tool/h2/map_script_schema.py) and
[verifier](../../../src/sf2tool/h2/map_script_engine.py) retain field-closed handler/source-site facts;
[map-event fixture loader](../../../src/sf2tool/h2/map_events_fixture.py) and
[verifier](../../../src/sf2tool/h2/map_events.py) preserve ordered shards and raw operands. The
[map-init schema](../../../schemas/h2-map-init-static-fixture.schema.json) and
[verifier](../../../src/sf2tool/h2/map_init.py) retain exact bodies rather than proving natural callers.
The research's [coverage and reproduction boundary](../../research/ribble-may-through-battle10-admission.md#existing-coverage-reproduction-and-remaining-unknowns)
names each owning narrow H2 command and selected consumer. No blanket suite is selected for this prose.

**Unknown:** legal route/layout and activated actors; current physical mappings, HP, active/AI/follower
state; natural text/input/entity/scroll completion; pending/parameter survival; May's cross-save
presentation and follower capacity; and continuously playable Battle10 entry. Keep these gaps explicit
without turning them into an emulator acquisition queue. Other town content, East Shrine/cave and
Battle10 outcomes remain outside this guide; it selects no new engine or product behavior.
