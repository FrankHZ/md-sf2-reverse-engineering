# Harpy Pond Approach Through Battle20 Entry

At Harpy Pond, Elric thanks the arriving group for helping him. A hidden monster interrupts,
claims that the pond traps its food and threatens to eat the elf. Peter reacts, the camera turns
toward the monsters, and the confrontation ends by unlocking Battle20 and requesting a transition.
A separate warning can make Bowie step back from two southern cells while the confrontation flag
is still clear. The subsequent fresh battle replaces the exploration actors with combatants;
Elric's separate neutral pond actor does not establish force membership.

**Confirmed:** the selected static programs, dialogue and conditional consumers establish the
sequence below. **Inferred:** the encounter frames a fight over a trapped elf. **Unknown:** natural
access, actual caller state, completed services, rendered presentation, usable input and persistence.
Statements made by the characters remain attributed speech. Neither the confrontation nor battle
entry rescues or recruits Elric.

This guide follows [Elven Village departure through Harpy Pond entry](elven-village-departure-through-harpy-pond-entry.md)
and consumes accepted [approach and Battle20-entry research](../../../../research/chapters/southern-travel-creed/harpy-pond-approach-through-battle20-entry.md).
Its fresh-entry stop is BattleLoop's `@Start`, **before ActivateEnemies**. Suspended entry stops
before `@IndividualTurns_Loop`. Unrelated requests stop at their own continuation boundary.
Activation, turns, tactics, outcomes, rescue/recruitment, F751 passage, Fairy Cave/Creed, Battle21
and terrain reconstruction remain outside this guide. Fnnn denotes flag nnn; rows and records are
zero-based. Wait counts and positions below are command inputs, not measured time or observed motion.

[Battle20 outcomes and Elric recruitment](battle20-outcomes-and-elric-recruitment.md) continues
separately through the selected aftermath and conditional exploration return.

## Which Pond Event Is Selected?

**Confirmed:** Map51's default and F520 alternate setups share the same zone callbacks and init.
F520 changes fixed population, not zone availability. Walking's admitted zone-event path publishes
event 6 with destination-derived tile coordinates; its warp-check path is a separate producer.
Layout and earlier movement checks determine which path runs. The zone table alone proves no
natural route or unavoidable barrier.

| Supplied zone coordinate | Gate and selected consequence |
| --- | --- |
| `(9,6)`, `(9,7)`, `(9,8)`, `(9,9)` | F470 set returns. F470 clear runs the confrontation `cs_5C312`, then sets F470 after script return. |
| `(9,24)`, `(10,24)` | F470 set returns. F470 clear runs the separate warning `cs_5C3AA`; it leaves F470 unchanged. |
| Other coordinates | Return-only default. The entity-event callback is also return-only. |

Neither selected zone tests F420, F520, membership, F13 or HP. ProcessMapEvent clears the pending
request before dispatch and initializes physical actor 0. The zone wrapper then selects the
current setup and first matching row. On return it closes portrait/text, waits one VInt and waits
for physical 0 to reach its destination. That final wait does not join every actor's action.

The prior guide owns initial pond population and its prompt. Fresh default population puts logical
Elric 13 at `(6,6)` with `eas_WhirlsInWater`; a Harpy and two Soulsower sprites occupy parked
`(63,63)` positions as logical 128–130. Their physical indices depend on supplied followers.
F520-set fresh setup omits those four fixed actors but can retain flag-selected followers. An ally
row can be skipped if its lookup is already populated; later follower flags can change bindings.
Changing a flag after population does not itself create or remove an actor.

The confrontation uses these **current logical lookups** without rebuilding them. B means
FOLLOWER_B 31, whose identity is supplied by the current population; it is not independently
validated as Oddler. Peter's presence and Elric's force membership are likewise not prerequisites
checked by the scene.

## The Confrontation, in Order

**Confirmed:** `cs_5C312` contains 29 top-level records and one complete inline move. Under the
ordinary mapped cast, the source orders the following content and actions. Dialogue is paraphrased
from keys 2555–2564; exact keys, modifiers and controls remain with the
[complete research program](../../../../research/chapters/southern-travel-creed/harpy-pond-approach-through-battle20-entry.md#complete-approach-program).

| Records | Content and action sequence |
| --- | --- |
| 0–5 | Set text cursor 2555. Replace Peter's action with Init and wait for idle, then do the same for B. Position 128 at `(7,16)`, 129 at `(6,17)` and 130 at `(8,17)`, all Up. These writes reveal/relocate existing actors; they allocate none. |
| 6–10 | Elric asks whether the group came to help, thanks them (2555), and says monsters might otherwise have eaten him (2556). Monster 128 protests that its dinner is escaping (2557). Start B's Jump, then start Peter's Jump and wait for Peter. |
| 11–15 | Wait 5; start Peter's Right/Left/Right/Left action; wait 120. Peter asks who spoke (2558). Request camera destination `(3,12)` and wait for scrolling. |
| 16–20 | Monster 128 explains its food trap (2559) and claims anyone caught there (2560). Elric asks about the missing people (2561); the monster replies that they were in its pond (2562). Move 128 Up two tiles with an inline destination/idle wait. |
| 21–25 | Monster 128 claims Elric as its dinner (2563), faces Down, and asks the others about ketchup (2564). Start 129's Jump, then start 130's Jump and wait for 130. |
| 26–28 | `setStoryFlag 20` writes F420. Publish the no-scroll Map51 `(0,0)`, Right warp; end the script. Only after script return does the caller set F470. |

With consistent mappings and completed movement, 128 goes from `(7,16)` to `(7,14)` before facing
Down. No command removes Elric from the pond, joins him, grants an item or writes F13, F520 or F751.
The monsters' claims about victims and Elric's words about escape do not independently prove those
events happened. There is no choice prompt or consent branch.

Text 2555 and 2559 use continuing nextText with `{N}`/`{W2}`. The remaining lines use single text
with `{W1}`; 2556, 2560, 2561, 2563 and 2564 also contain newlines. Monster 128's `$C0` modifier
selects mirrored/right portrait bits, while Elric and Peter use `$0`. Portrait/speech properties
come from the currently mapped mapsprite. Continuing text leaves closure to continuation; single
text closes portrait/text and requests Sleep 10 after display. These controls are not additional
choices or a measurement of how long the conversation takes.

### What the Waits and Actor Writes Establish

**Confirmed:** Init replaces the selected action, sets speed 32/32 and zero acceleration, enables
obstruction/collision and autofacing, orients Up, disables immersion, sets size 24, refreshes and
idles. Thus the approach replaces the prior prompt's following actions for Peter and B. Jump
briefly disables autofacing, requests relative vertical motion and two waits of 5, restores
autofacing and idles. Peter's four-facing action refreshes and waits 30 after each facing.

A wait modifier polls for the exact `eas_Idle` pointer of its named actor. Waiting for Peter's
Jump does not independently wait for B; waiting for 130 does not join 129. The separate wait 120
does not join Peter's facing action. Later writes can replace unfinished actions. The inline
Up move constructs relative destination movement, waits for destination equality and finally
idles. It bypasses ordinary walking's zone/warp producer; crossing a listed cell does not itself
submit another zone event.

Position and facing commands have a live-ally HP helper, but IDs 128–130 bypass it. These selected
NPC writes therefore have no HP gate. Action and dialogue commands do not suppress their work
because Peter or Elric has HP 0. They do not validate membership or safely reject a missing actor:
unassigned logical lookups can alias physical 0, and 255 is not a safe absence sentinel for this
consumer. F520-set/F470-clear supplied state can consequently select this same script with missing
or independently supplied actors. The source does not repair that state or reinterpret it as a
post-battle scene. These are static counterexamples, not observed gameplay failures.

The camera command disables entity following and waits for a tile destination; this scene does
not restore a camera entity before publishing its warp. Debug mode with player-2 Start can enable
cutscene-text skip, bypassing display/cursor advancement and Sleep for csWait. End conditionally
waits for scrolling if the dialogue-window index is nonzero, clears camera speed and returns.
Action, text, camera and wrapper services still have to return for later work to occur. Earlier
writes survive partial execution; there is no rollback or guaranteed all-actor completion.

## The Southern Warning and the Independent Exit

**Confirmed:** the five-record `cs_5C3AA` sets cursor 2565, displays Peter's single line urging
`{LEADER}` to save the elf (`{N}`/`{W1}`), nods Bowie, moves Bowie Up one tile with an inline idle
wait, then ends. It writes no flag, warp, mode or membership state. F470 is its only caller gate;
while clear, a later separately admitted invocation can repeat it.

Bowie's nod has no HP or consent test. It stops animation, requests Sleep 10, transforms/uploads
the mapsprite, requests Sleep 20, refreshes, requests Sleep 10 and restores animation. The inline
move also has no HP gate. With ordinary Bowie-to-physical-0 mapping and completed services, it
uses the earlier event Init speed and moves Up from Bowie's then-current position. The supplied
Y24 zone coordinate does not prove an actual starting Y24 or a final absolute Y23. `{LEADER}`
is resolved from the first current battle-party member after UpdateForce, not an unconditional
literal Bowie name.

The separate external warp table selects **any X at Y25**, requesting no-scroll Map67 `(57,26)`,
Down. WarpIfSetAtPoint writes WARP_SFX and does not test F470. That return can scan Map67's unrelated
row 21/F421/F521 admission; this guide stops at that continuation. The two warning cells do not
establish a universal exit lock, all traversable routes or natural movement to the external warp.

Direct Map67 arrival requests Map51 `(10,24)`, Up; the earlier call-for-help requests `(11,22)`,
Up. Initial setup/init and a subsequently admitted walking zone are different operations. Arrival
at the direct tuple does not itself trigger the Y24 warning. The F470-clear initial prompt can
move the cast before later walking. Neither entry tuple proves that the warning ran.

## From the Confrontation Request to Battle20

**Confirmed:** the ordering preserves three separate changes:

1. The script's `setStoryFlag 20` adds unlock base 400 and sets **F420**.
2. Its warp publishes pending event 1, no-scroll parameter 0, Map51/X0/Y0/Right. It does not load
   the map, place combatants, leave the script immediately or write WARP_SFX.
3. After script return the zone callback sets **F470**, then the wrapper services finish.

If the request remains pending, exploration bypasses ordinary controller installation.
ProcessMapEvent clears pending; the no-scroll path consumes/clears the currently supplied WARP_SFX,
idles physical 0, unwinds to MainLoop and returns the tuple. MainLoop performs SwitchMap, then
CheckBattle, before fresh ExplorationLoop. Map51 has no flag-switched-map substitution.

Map51's sole battle-coordinate row is **row 20**, origin `(0,0)`, size 18x26, trigger 255/255.
CheckBattle requires F420 and treats both trigger bytes as wildcards; it tests neither rectangle
containment nor F470, membership or HP. After writing the battle area, it checks F520. If F520
is set, it clears F420 **after selection without canceling this admission**. MainLoop then passes
Map51/Battle20 to BattleLoop. The warp tuple is not an ally placement or battle-facing instruction.

| Supplied state or interruption | Conditional consequence |
| --- | --- |
| Complete F470-clear confrontation, retained request, F520 clear | F420 and then F470 are set. Ordinary consumption admits Battle20 before fresh pond setup/init. No intervening usable exploration pause is established. |
| Same completion with F520 set | Admission still succeeds and clears F420. Fresh battle omits neutral Elric but still selects the normal enemy rows; earlier missing exploration actors are not repaired. |
| F470 set, F420 clear | Both pond zones return without writes. No unlock is repaired. A later ordinary scan can enter fresh exploration under its supplied F520 population and F470 init gates. |
| F420 independently set, F470 either value | An ordinary Map51 scan can admit Battle20 directly at either initial tuple or another coordinate, without the confrontation. |
| F420 cleared before the scan | The retained warp alone does not admit Battle20. If fresh exploration is selected, its heal/setup/temporary-clear/init/controller order belongs to the prior guide; surviving F470 skips the prompt. |
| Pending replaced, or script/wrapper not returned | The replacement consumer or partial execution controls continuation. Earlier flag writes are not rolled back. F420 and pending may already be written before F470. Stop unrelated admission/request work. |

F470, F420 and F520 are independent supplied flags; the source does not normalize them into a
single story phase. Ordinary fresh exploration is bypassed by successful battle admission, not
inserted between that no-scroll return and BattleLoop.

## Empty Before/Start Targets and Fresh or Suspended Entry

**Confirmed:** actual before-battle and battle-start slot 20 both select `ms_Empty`, the `$FFFF`
terminator at `0x47A4E`. The wrappers share intro flag **F470**, also used by the confrontation.
Before checks without setting; start checks and, when clear, sets F470 before dispatch.

A completed approach therefore skips both selected scripts. Supplied direct F420/F470-clear
admission executes the empty before target and later sets F470 before executing the empty start
target. It does not replay the confrontation, reveal the old NPCs or run pond init. Empty targets
still use the script engine's entry/end services; absence of scene records does not prove immediate
presentation or responsive input.

BattleLoop first normalizes PLAYER_TYPE to 0 and sets F399, then tests suspension F88. Fresh
F88-clear entry clears elapsed seconds, saves incoming map/battle, assigns current Map51/Battle20,
installs base VInts and invokes before. It then restores incoming map/battle, clears region flags
90–105, heals eligible allies, initializes ally/enemy positions, clears AI memory, calls LoadBattle with
fresh d0=0 selection, and invokes start. The stop is its return to `@Start`, **before ActivateEnemies**.

Healing restores HP/MP maxima for living allies and immortal Peter 7 / Lemon 28, retains only existing
STUN/POISON/CURSE bits and updates stats. It does not revive other dead allies or join anyone.
Later battle healing cannot establish that an earlier pond scene had valid actors or HP. The mode
clear occurs in BattleLoop, not in the warp; it proves no saved raft-coordinate write.

F88-set entry instead restores saved seconds, clears F88 and AI memory, clears the view target,
calls LoadBattle and branches directly to `@IndividualTurns_Loop`. It skips incoming map/battle
assignment, before/start, fresh healing/roster initialization and new turn-order generation; it
does not explicitly clear d0 before LoadBattle. Saved map, battle, area, combatants and turn state
must already agree. Incoming Battle20 does not repair them. Stop before individual turns; the
following source branch to `@Start` is unreachable on this suspended path.

## The Battle Cast Is Rebuilt

**Confirmed:** BattleSpriteset20 contains twelve ally placement rows, twelve enemy rows and zero
AI regions/points. Full positions, row values and consumers belong to the
[selected roster research](../../../../research/chapters/southern-travel-creed/harpy-pond-approach-through-battle20-entry.md#complete-selected-spriteset-and-roster-consumers).
This guide does not maintain a second low-level roster table.

Ally rows are positions for the current joined/active battle-party list, not twelve named allies
or a new recruitment list. Initialization clears X for all 30 allies, calls UpdateForce and
consumes that party and the row X/Y values. Dead members consume a party/loop slot without advancing
the placement row; remaining members beyond capacity can leave the active party. No guaranteed
living twelve-person force, Peter, Oddler or Elric membership follows. The first party entry is
read before exhausted count is tested, so an empty/inconsistent list is not guaranteed safe.
Row HEALER1/NOTHING and NONE/15 operands do not install new ally AI or items.

The twelve enemy rows select **five Harpies, two Dark Madams, two High Priests, two Orcs and one
Arrow Launcher**, at combatant slots 128–139. The commands differ: SWARM on Harpy rows 0/6/11,
ATTACKER1 on rows 3/5/8/9/10, ATTACKER3 on rows 1/4, and HEALER3 on rows 2/7. Primary FOLLOW_ENEMY
operands on rows 1/2/4/7 encode enemy rows 3/1/5/8; other primary orders are NONE. All have primary
region 15, secondary NONE/15, filler 0, STARTING and row item NOTHING. These are installed inputs,
not observed following, attacking, healing or activation; zero region records do not close the
excluded activation algorithm.

Enemy initialization scans all 32 slots 128–159. The twelve STARTING rows copy their selected enemy
definitions, fill HP/MP, write row position/type/orders/triggers, combine movement type with
commandset, handle row items/activation bits, apply shared difficulty and update stats. Remaining
slots initially take the absent/hidden path (HP 0, X=-1). NOTHING omits an added row item, not
necessarily definition inventory, equipment or spells. No derived stats or effectiveness follow
from this guide. Battle20 is excluded from the actual random-upgrade list; F520-set admission
does not promote its enemy types through that mechanism.

LoadBattle requests fade/tileset/VInt work, then clears entity data/lookups and creates actors
from eligible rebuilt combatants with nonnegative X/Y, excluding neutral-marked enemies from the
ordinary pass. Ordinary actors use standing actions and Down facing. Exploration physical indices
and actions are not carried into those combat slots: old exploration logical 128–130 mappings
do not turn into battle enemy types. In particular, battle 129/130 are Dark Madam/High Priest.

### Elric's Separate Neutral Actor

**Confirmed:** after ordinary placement, the neutral tail checks F500+CURRENT_BATTLE. With
**F520 clear**, it declares a new ELRIC_BASE actor at `(6,6)`, initially Down, using
`eas_WhirlsInWater`. It writes battle lookup **159**, zero maximum/current HP and status, neutral
activation bit and position. This is distinct from exploration ally lookup 13 and force membership.
Its action enables immersion/double animation speed and loops refreshed Right/Up/Left/Down facings
with wait 10 each. Initial Down is not a fixed final facing or an observed drowning animation.

F520 set skips this tail, including after CheckBattle cleared F420 but retained admission. It does
not remove the twelve enemy rows. Neutral HP 0 does not use ordinary dead-ally omission. Neither
this declaration nor the approach joins Elric. Independently supplied Elric membership can coexist
with ordinary ally placement and the separate neutral; the source does not reconcile them here.

LoadBattle then selects the d0 entity (fresh supplies combatant 0), initializes sprites, loads
map/mapsprites, installs base VInts, loads terrain, requests music/fade-in and returns to the start
wrapper. These are named service dependencies, not terrain reconstruction or an observed playable
turn. This pre-activation stop establishes no later roster snapshot, playable turn or rescue.

## Evidence Trace and Review Boundary

**Confirmed provenance:** accepted research binds SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Its [reproduction owner](../../../../research/chapters/southern-travel-creed/harpy-pond-approach-through-battle20-entry.md#reproduction-and-remaining-limits)
retains exact source paths, addresses, full programs, literal texts and immediate consumers.
This synthesis adds no raw-source investigation, payload regeneration or runtime observation.

| Explanation | Contract and executable owner | Bound retained in this guide |
| --- | --- | --- |
| Selected zones and population | [map exploration](../../../contracts/world/map-exploration.md), [setup](../../../contracts/world/map-setup-data.md), [entities](../../../contracts/world/map-entity-data.md); [zone shard](../../../../../tests/fixtures/h2/map-events/zone-programs.json) sf2-map-events-static-v1:zone-programs, expected.zoneTargetPrograms orders 94–96; [init fixture](../../../../../tests/fixtures/h2/map-init-static-v1.json) sf2-map-init-static-v1 | Public zone bodies bind 5/4/1 operations. Initial population/init remains with the preceding guide and accepted research. Neither proves natural reach. |
| Full confrontation/warning and immediate command semantics | [dialogue](../../../contracts/interface/dialogue-system.md), [party/roster](../../../contracts/party/party-roster-state.md); [map-script fixture](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json) sf2-map-script-engine-static-v1, expected.dialogueCommandFacts, storyStateCommandFacts, transitionCommandFacts, entityPlacementCommandFacts, entityActionBridgeCommandFacts, entityGestureRelationshipMotionCommandFacts, mapCameraControlCommandFacts and scriptControlCommandFacts | Public summaries do not contain complete scene programs. Accepted research's retained programCorpus.programs cs_5C312/cs_5C3AA supplies all 29/5 records and two inline moves; selected literal text/HP/lookup/action joins are research-owned. |
| Pending request, ordinary admission and no fresh exploration | [exploration flow](../../../contracts/world/exploration-control-flow.md), [entry routing](../../../contracts/world/map-entry-routing-state.md); [request writes](../../../../../tests/fixtures/h2/map-event-request-state-static-v1.json) sf2-map-event-request-state-static-v1/eventRequestState, [consumption](../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json) sf2-map-event-request-consumption-static-v1/eventRequestConsumption; [common maps](../../../../../tests/fixtures/h2/common-maps-static-v1.json) sf2-common-maps-static-v1/expected.mapFacts | Shared fixtures guard helpers and context; accepted research owns selected Map51 row 20, missing switch substitution and actual ordered producer/consumer joins. |
| Before/start and fresh/saved lifecycle | [cutscene routing](../../../contracts/battle/battle-cutscene-routing.md), [battle lifecycle](../../../contracts/battle/battle-control-lifecycle.md); [cutscene fixture](../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json) sf2-battle-cutscenes-static-v1/expected.cutsceneFacts.intro, [loop fixture](../../../../../tests/fixtures/h2/battle-loop-static-v1.json) sf2-battle-loop-static-v1/expected.lifecycleFacts | Exact slot 20 empty targets and branch-specific lifecycle are selected research joins. [routing fixture](../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json) sf2-battle-routing-data-static-v1 has routeTargetsParsed=false, not a complete public slot-to-program proof. |
| Roster, upgrade exclusion and neutral 159 | [encounter definition](../../../contracts/battle/battle-encounter-definition.md), [battle lifecycle](../../../contracts/battle/battle-control-lifecycle.md); [spriteset fixture](../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json) sf2-battle-spriteset-data-static-v1/expected.facts | rowContentTracked=false: aggregates do not guard Battle20's full rows. Accepted research owns the selected spriteset, neutral/upgrade lists and actual consumers; no tactics/control outcome is imported. |

Schema/verifier routes are [zone schema](../../../../../schemas/h2/map-events-zone-programs.schema.json)/[map_events_fixture.py](../../../../../src/sf2tool/h2/map_events_fixture.py),
[engine schema](../../../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../../../src/sf2tool/h2/map_script_engine.py),
[init schema](../../../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../../../src/sf2tool/h2/map_init.py),
[request-write schema](../../../../../schemas/h2/map-event-request-state-static-fixture.schema.json)/[map_event_request_state.py](../../../../../src/sf2tool/h2/map_event_request_state.py),
[request-consumption schema](../../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../../../src/sf2tool/h2/map_event_request_consumption.py),
[map schema](../../../../../schemas/h2-common-maps-static-fixture.schema.json)/[maps.py](../../../../../src/sf2tool/h2/maps.py),
[cutscene schema](../../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)/[battle_cutscenes.py](../../../../../src/sf2tool/h2/battle_cutscenes.py),
[loop schema](../../../../../schemas/h2-battle-loop-static-fixture.schema.json)/[battle_loop.py](../../../../../src/sf2tool/h2/battle_loop.py),
[routing schema](../../../../../schemas/h2-battle-routing-data-static-fixture.schema.json)/[battle_routing_data.py](../../../../../src/sf2tool/h2/battle_routing_data.py)
and [spriteset schema](../../../../../schemas/h2-battle-spriteset-data-static-fixture.schema.json)/[battle_spriteset_data.py](../../../../../src/sf2tool/h2/battle_spriteset_data.py).
Existing `uv run sf2 h2` routes include map-events, map-script-engine, map-init, map-event-request-state,
map-event-request-consumption, common-maps, battle-cutscenes, battle-loop, battle-routing-data and
battle-spriteset-data. They locate evidence and select no prose-only regeneration or blanket suite.
Load ignored worktree private-input configuration in the same process as controlled commands.

Review uses the accepted research, shared contracts, bounded public fixture sections and schema/
consumer boundaries above. Complete programs, selected table rows and literal text are consumed
through accepted research and its receipts, not independently reproduced here. Preserve
[PR870 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/868#issuecomment-6170807905),
[rebased handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/868#issuecomment-6170840743)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/870#issuecomment-6170954167),
including unchanged comparisons, original/rebased CI, inspection corrections and linked predecessor
failures. Shared contract Unknowns are not globally closed by a content-specific static join.

Review states include independent F470/F420/F520, direct admission, missing/dead/mismapped actors,
changed followers, empty/over-capacity party, F88 saved state, debug text skip, partial services
and overwritten pending requests. They constrain this account and are not executed scenarios.
**Unknown:** natural terrain/access/producer selection, actual flags/HP/class/membership/lookups/mode,
completed action/text/camera/fade services, rendered scene, usable control and persistence. They
remain deferred under [ADR0014](../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../../decisions/0016-remake-start-evidence-deferral.md), not an automatic runtime queue.
This guide establishes neither a composed runtime route nor campaign/design-epic completion.
