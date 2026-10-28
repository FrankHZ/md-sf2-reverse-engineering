# Polca fairy interaction through Battle12 admission

Evidence date: 2026-10-13. **Confirmed:** Polca's optional turtle interaction requests
four texts and an actor transformation, then its caller sets F711. The selected east
exit independently requests Mount Volcano, Map52 `(1,28)`, Right. Its searching-devils
zone script sets Battle12's unlock F412 and requests Map52 `(0,0)`, Right; the caller
sets F713 before that pending request is consumed. Battle12's before and start rows
are both empty. **Inferred:** these source connections form a candidate continuation
from Polca toward the mountain battle. **Unknown:** natural travel, supplied actors
and flags, completed services, visible effects and playable entry. Neither the turtle
interaction nor the first Oddler scene is a required predicate of the selected exit.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #776](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/776), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Interaction routing, asynchronous entity services, pending requests and battle admission
cross existing owners and require explicit boundaries. This document is the primary
tracked owner; the only additional change corrects the reload coordinate divisor in
`battle11-outcomes-through-polca-arrival.md`. No shared index, registry, schema,
fixture, parser, design, engine, UI or translation edit. Consume accepted [Polca arrival](./battle11-outcomes-through-polca-arrival.md),
[request consumption](./map-event-request-consumption.md),
[common scripting](./common-scripting.md), [battle cutscenes](./battle-cutscenes.md)
and [battle loop](./battle-loop.md). Starting accepted base is
`3d6944a7c3717eac8d7c3466278750cc83f0d8fb`; no stacked dependency.

Scope is `Map13_EntityEvent3` / `cs_58512`, the selected east exit, Map52 ordinary-entry
and interruption discriminators, `Map52_ZoneEvent0` / `cs_5C4EE`, and Battle12 admission
through the start-dispatcher return immediately before `ActivateEnemies`. Other Polca
conversations/populations are selection discriminators only. Combat, outcomes,
Battle12 end/after scripts, `cs_5C622`, Bedoe entry/content, the Volcanon scene, other
routes, layout/path extraction, runtime acquisition and new comparison machinery are
excluded. No inseparable scope expansion is required. Unknowns do not authorize work.

Source paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM`, pinned `master` revision `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind this revision and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Their existing fixture comparisons and the cutscene-source comparison pass; no fresh
ROM extraction or scene observation is claimed. Fnnn means flag nnn. Table rows,
map-init `sourceOrder` and event `programOrder` are zero-based; script record numbers
are one-based positions in retained `programCorpus.programs[].commands`.

Reuse the retained 18,285,979-byte pair: one process, linear selected reads/comparisons,
estimated peak below 512 MiB and comparisons below one minute, plus small prose and
receipts. Comparisons completed in 0.091 seconds. The selected Map13 `scripts.asm`
segment is lines 132–185, **54 lines**, ending before `cs_585FE`; Map52's is lines 4–81,
**78 lines**, ending before `cs_5C622`. Inline action payloads are separate from script
records. Reassess before expanding these bounds; no new payload or per-scene fixture
is needed. Direct document/source/fixture checks, the clean committed planner and
actual final-head CI complete the docs-only acceptance profile.

## Optional interaction and actual entity-event routing

**Confirmed:** the [accepted population/init owner](./battle11-outcomes-through-polca-arrival.md#ordinary-polca-population-and-init)
establishes the last-set setup priority F513 > F799 > F710 > default. Actual files
`pointertable_709.asm` and `s1_entities_709.asm` contain **flag710** symbols. In default
and F710 populations, regular NPC lookup 130 is the kappa at `(1,11)`, Down,
`eas_InitSlow`; the separate Gerhalt ally row does not consume a regular NPC index.
Physical slots depend on the follower prefix. F711-clear init invokes `cs_58310`,
which stages 130's action/orientation. F711-set init only skips that staging; it does
not itself hide 130. The first Oddler zone's separate post-reload caller hides rebuilt
130 when F711 is set, or stages him again when clear, before setting F710.

`data/maps/entries/map13/mapsetups/s2_entityevents.asm` row 3 routes lookup 130 to
`Map13_EntityEvent3` (`0x58078`, retained entity program 41). Its second operand is
spelled `RIGHT`, numerically **0**, and stored as event **flags**. The actual
`mapsetupsfunctions_1.asm:RunMapSetupEntityEvent` matches the entity index only; it
does not compare player facing against that byte. Bit 0 requests temporary facing
toward the player and bit 1 restores the saved facing afterward. Both are clear here.
Thus the table spelling imposes neither a Right-facing activation condition nor an
automatic turn/restore for this event.

The normal `explorationvints.asm:ProcessPlayerAction` path first idles main and waits
for motion/view scrolling. A opens the menu; C reaches entity activation after the
debug/caravan alternatives. `explorationfunctions_0.asm:GetActivatedEntity` requires
a nonnegative view target, offsets a point in its facing direction, continues across
`$2800` counter blocks, and scans physical entities excluding the controlled entity
and followers. A candidate needs Manhattan distance less than `MAP_TILE_SIZE=384`
from that point. `GetEntityEventIndex` reverse-maps the physical slot through
`ENTITY_INDEX_LIST`; only then does the setup dispatcher select the event. This
proves conditional activation mechanics, not a natural path or usable interaction
from every player position. The dispatcher sets portrait/speech state, invokes the
handler, closes windows and reenables entity updates on return.

The seven retained operations of `Map13_EntityEvent3` have two branches:

- F711 clear: execute `cs_58512`, then set F711, then return. The script must return
  before that flag write; a stalled action/text/effect service does not complete it.
- F711 set: request text 1425 (`0591`, about ancient magic tunnels), then return.
  This is a conditional handler branch, not evidence that departed actor 130 remains
  available to talk to immediately after the scene.

F799 still selects the ordinary event table, but its lookup 130 is a minotaur. F513
selects another event table whose row 2 also routes 130 with flags 0 to this same
handler; its kappa row is `(1,13)`. These are supplied-state discriminators, not a
full later-town audit or proof that the turtle scene is sensible under arbitrary
flag combinations. Missing mappings are not safely skipped: the script resolver
`GetEntityAddressFromCharacter` can resolve an untouched zero mapping to physical 0.
F711 alone therefore proves neither prior rescue nor actor identity/availability.

A second producer is `data/maps/entries/map11/mapsetups/s3_zoneevents.asm:Map11_ZoneEvent0`:
with F722 clear it calls `cs_57AAA`, then sets F722 and F711. Only this relationship is
included; the Volcanon scene is outside scope. It explains why F711 is not an
exclusive rescue-history bit.

## Turtle scene records and service limits

**Confirmed:** `cs_58512` at `0x58512` has **38 records: 30 opcode commands, seven
sleeps and one terminator**. Cursor 1435 yields four `nextSingleText` requests, all
from actor 130, IDs 1435–1438 (`059B..059E`). The direct source and retained corpus
agree on each record's source line, macro and operands.

| Records | Ordered requests and inline actions |
| --- | --- |
| 1–8 | Install/wait actor 130's obstruction-off then idle action; neutral camera; cursor 1435; request help turning over; reset Bowie and Peter with `eas_Init`; move Peter's destination to `(2,12)` and wait; face Peter Left. |
| 9–18 | Sleep 30; Bowie `eas_BumpUp` with wait; sleep 40; harder-push text 1436; sleep 10; Bowie nod; two more waited bumps; sleep 20; encouragement text 1437. |
| 19–26 | Disable Bowie's auto-facing; wait Down 1; sleep 30; install/wait speed `(48,48)` then idle; wait Up 1; quake 2, sleep 30, quake 0. |
| 27–32 | Actor130 `MOSAIC_OUT`; install a non-waited Up-orientation / fairy-sprite / idle action; `MOSAIC_IN`; sleep 40; face 130 Down; text 1438 describes the test and departure. |
| 33–38 | Reset 130 with `eas_InitSlow`, disable auto-facing, wait Up 10; reset Bowie with `eas_Init`; attempt Peter→Bowie following with selector 2; end. |

The seven sleeps sum to **200 requested frames**, not measured scene duration.
`eas_BumpUp` in `data/scripting/entity/eas_actions.asm` disables auto-facing, requests
relative `(0,-1)`, waits5, `(0,1)`, waits5, `(0,0)`, reenables auto-facing and jumps
idle. These action units must not be substituted for the scene's tile-step macros.
The first obstruction action clears flagsA bit 7 through `esc18_setEntityObstructable`;
`eas_InitSlow` later reenables it. `esc1B_setEntityOrientation` changes orientation
bits, while `esc17_setSpriteNumber` writes `MAPSPRITE_FAIRY=219` to the mapsprite byte.
That inline sprite write is not the map-script `setSprite` command or a recruitment.

`mapscriptengine_1.asm:csc33_setQuakeAmount` with unflagged operands 2/0 writes
`QUAKE_AMPLITUDE`. `csc22_animateEntityFadeInOrOut` resolves the actor, consumes the
word selector, and sends `MOSAIC_OUT=6` / `MOSAIC_IN=7` to `loc_46BE2`, respectively
with D1=0/-1 and initial D0=1/`$7FFF`. Each branch has sixteen source loop iterations,
calling `LoadMapsprite`, `sub_45E10`, `DmaMapsprite`, and three `WaitForVInt` services
per iteration, then advancing D0 by add/add-one or right-shift. This describes
requests/control flow, not a verified visible mosaic or transformation timing.
The non-waited action between effects leaves its scheduling relative to those
services unobserved here.

The [existing effect evidence](./common-scripting.md#confirmed-map-script-entity-presentation-fx-command-family)
confirms selector 6/7 entry/loop/return chronology at a controlled seam. Its resolver,
loading, crop/DMA and other named service entries are shims; their original bodies
do not execute in that matrix. It cannot certify this scene's visual effect or
service completion. The [screen owner](./common-scripting.md#confirmed-map-script-screenmap-presentation-command-family)
and [gesture owner](./common-scripting.md#confirmed-map-script-entity-gesturerelationshipmotion-command-family)
retain their own limits. No runtime rerun is part of this slice.

`customActscriptWait`, `setActscriptWait` and waited entity sequences install actions
and poll for `eas_Idle`; they have no general HP or missing-actor skip. `setDest`
scales tiles by 384, writes destination/travel/velocity and waits for motion for these
unflagged operands; it is not an instantaneous position assignment. `setFacing`
uses its byte character's word-HP guard. `followEntity`, however, probes the high
byte of the first **word** operand: Peter's `$0007` probes Bowie0, not Peter7.
With Bowie's word HP zero it skips the six operand bytes. Otherwise it resolves
Peter/Bowie and calls `AddFollower` with selector 2's `(-24,0)` offset. It sets no
F7, F39, F67 or force-AI membership. Missing Peter can resolve to main; a dead Peter
with living Bowie is not excluded by this particular probe.

This relation is current entity/follower state. Unlike the first Oddler scene,
`cs_58512` has no `reloadEntities`, `loadMapEntities`, join, flag or warp command.
Up 10 is a movement request, not actor deletion or guaranteed off-map placement.
After the caller sets F711, pending zero allows the shared `WaitForEvent` path to
restore camera target 0 and controlled action. Another pending request instead
wins that poll. A later ordinary map entry clears/rebuilds entities and followers
from persistent flags; it does not preserve this transient relation or fairy sprite.
F67 can independently rebuild Peter. Later Polca init/setup and the separate
Oddler caller retain the distinct F711 behavior described above.

## East exit and immediate Map52 admission

**Confirmed:** `sf2enums.asm` defines `MAP_POLCA=13`, `MAP_MOUNT_VOLCANO=52` and
`BATTLE_MOUNT_VOLCANO=12`. Row3 in `data/maps/entries/map13/6-warp-events.asm` is
X31 / wildcardY, no scroll, Map52 `(1,28)`, Right. Earlier rows win on overlap;
at `(31,31)` the preceding wildcardX/Y31 south exit matches first. The selected
east row itself tests none of F710, F711, F511, Peter membership or rescue history.

The ordinary movement producer in `entityscriptengine_2.asm` requires layout
attribute `$1000`; `exploration.asm:WarpIfSetAtPoint` then scans eight-byte rows and
writes event type 1, parameters and warp SFX. Coordinates alone do not prove a
reachable warp tile. `ProcessMapEvent` clears the pending type before dispatch;
`ProcessMapEventType1_Warp` runs the SFX service, idles main, unwinds exploration,
samples/replaces coordinates and returns the tuple to `MainLoop`, which calls
`SwitchMap` then `CheckBattle`. There is no flag-switched replacement for Map52.
Mutable parameters and intervening services/writers remain the
[request owner's](./map-event-request-consumption.md) boundary.

The sole Map52 battle row is row 12 in `data/battles/global/battlemapcoords.asm`:
area `(0,0,32,32)`, trigger `(255,255)`, unlock F412 / completion F512. `CheckBattle`
uses the wildcard trigger, not area containment. Thus the intact `(1,28)` request
has these supplied-state alternatives:

| F412 | F512 | Immediate result |
| --- | --- | --- |
| Clear | Either | No battle; ordinary Map52 entry. |
| Set | Clear | Select Battle12 immediately, before ordinary population/init or searching-devils scene. |
| Set | Set | Clear F412 and still select Battle12 on this same call. Completion does not decline this admission. |

Neither F713 nor F710/F711/F511 is a predicate here. A supplied unlock can bypass
the searching scene; no claim that every admitted battle follows that scene.

Ordinary entry clears pending, adjusts the step counter, heals living/immortal allies,
sets current map/not-in-battle, rebuilds entities/mapsprites, clears temporary
flags 256–383, sets F80 and runs map load/init. `data/maps/mapsetups.asm` selects
`ms_map52`, with F512 overriding it to `ms_map52_flag512`. Both pointer tables share
the entity-event, zone and init pointers. Their fixed populations differ:

| Default lookup | Authored origin / identity, all Down and `eas_Init` |
| --- | --- |
| 128 | `(21,7)`, Death Archer |
| 129 | `(18,9)`, Hobgoblin |
| 130 | `(23,8)`, Witch |
| 131 | `(21,15)`, Evil Cloud |

F512's list is empty. These four authored enemies are exploration actors, not the
battle roster. Map52 is neither a follower-excluded map nor overworld; F65 does not
supply the caravan prefix. The shared follower pass can supply Peter7 through F67,
independently of F7/F39, HP or force AI, and other enabled follower rows affect
physical order. No fixed Peter row repairs F67-clear absence. Ordinary sprite
selection can use the dead-ally blue-flame alternative; that does not repair mapping
or establish membership. F512-set/F713-clear can leave all four scene lookups missing,
yet the common zone pointer still offers the scene. Such inconsistent supplied states
are not silently promoted to a natural route or protected by invented predicates.

`ms_map52_InitFunction` at `0x5C4D6` (retained primary body 65) tests F714. Clear returns;
set falls through to `sub_5C4DC`, which tests F712, calls later `cs_5C622` only if clear,
then sets F712. The primary fixture body ends after its first two instructions;
the separately delimited helper must be read to understand the fallthrough.
This is an explicit possible interruption, not an audited postbattle scene. Neither
selected arrival coordinates nor F412-clear establishes its absence.

## Searching-devils scene and pending handoff

**Confirmed:** `data/maps/entries/map52/mapsetups/s3_zoneevents.asm` has point rows
`(3,23)` and `(4,23)`, both targeting `Map52_ZoneEvent0` (`0x5C4B6`, zone program 97),
then a return-only default. Ordinary zone production requires `$1400`; town departure
or arrival at `(1,28)` does not automatically invoke these points. F713 set returns.
Clear loads `cs_5C4EE` into A0, calls `ExecuteMapScript`, then sets F713 and returns.
It checks neither F512 nor actor availability nor the turtle/Oddler flags.

`cs_5C4EE` at `0x5C4EE` has **57 records: 47 opcode commands, nine sleeps and one
terminator**. Cursor 1397 yields thirteen text requests, nine `nextSingleText` and
four `nextText`, IDs 1397–1409 (`0575..0581`); Peter speaks twice, 131 three times,
129 twice and 128 six times. The texts describe a search, battle marks, birdmen and
Volcanon, then discovery of the visitors and a threat. They are dialogue assertions,
not independently established causes of prior events.

| Records | Ordered requests and inline actions |
| --- | --- |
| 1–8 | Cursor 1397, camera speed `$30`; reset Bowie/Peter; move destinations to Bowie `(5,23)`, Peter `(4,22)` with waits; wait Peter Right 1; Peter asks who the strangers are (1397). |
| 9–18 | Camera `(16,4)`; sleep 5, install 129 Right/Left oscillation; sleep 5, install 130 same; Peter describes searching (1398); sleep 5/129 oscillation, sleep 5/130 oscillation. Oscillation installations do not wait. |
| 19–28 | Wait 131 Up 6; sleep 40; 131 reports no target under the cliff (1399); wait 129 Up 2/Right 1; face 128 Left; 129 texts 1400/1401 describe marks and ask about birdmen; nod 128; 128 texts 1402/1403 respond and mention rocks. |
| 29–38 | Sleep 5; install 129 Up/Down oscillation; sleep 60; 128 text 1404 attributes the rocks to Volcanon; face 129 Right; 128 text 1405 continues; wait 131 Down 1; sleep 40; install/wait 131 speed `(48,48)` then idle; wait 131 Down 2/Left 1. |
| 39–48 | 131 asks who is there (1406); face 131 Down; camera `(0,17)`; face Bowie Up and Peter Down; sleep 50; camera `(16,7)`; reset 131, disable auto-facing, wait 131 Up 1. |
| 49–57 | 131 calls them Polca children (1407); wait 128 Down 2/Left 2/Down 1; 128 disparages Polca (1408); face 128 and 131 Up; 128 threatens them (1409); `setStoryFlag 12`; warp Map52 `(0,0)`, Right; end. |

The nine sleeps total **215 requested frames**; movement, camera and text services
add waits. `cameraSpeed` writes `VIEW_SCROLLING_SPEED`; each `setCamDest` sets neutral
view target, calls `SetCameraDestination` and waits for scrolling. Ordinary script
termination conditionally waits for scrolling when a dialogue window remains, then
zeroes scrolling speed. Text/debug sleep skips and the action/HP/missing-lookup
boundaries above apply. No join, following command, population reload or force-AI
change is present in this program.

Record 55 `mapscriptengine_2.asm:csc13_setStoryFlag` adds
`BATTLE_UNLOCKED_FLAGS_START=400` to operand 12, setting **F412**, not literal F12.
Record 56 `csc07_warp` writes pending type 1, parameter 1=0, then map52/X0/Y0/Right to
parameters 2–5. It does not immediately switch map, launch battle or set `WARP_SFX`.
That service uses the carried SFX value. Record 57 must terminate and return before
the caller writes **F713**; the script does not set F713 itself.

`RunMapSetupZoneEvent` then closes portrait/text, waits a VInt and waits for main
to stop. Only after the caller and these services return does exploration poll the
new pending warp. With type 1 intact, `WaitForEvent` skips restoring controlled input;
`ProcessMapEvent` clears the type and consumes the no-scroll tuple. It returns to
`MainLoop` for `SwitchMap` and `CheckBattle` again. An earlier service stall can prevent
F412 or the warp write; a later stall can leave F412, the request or F713 already
written without consumption. No atomic transaction, immutable request snapshot or
unconditional completion is proved. F713-set/F412-clear also does not repair itself
by revisiting the zone, because that caller skips the script.

## Re-admission, actual battle placement and stop

**Confirmed:** the intact Map52 `(0,0)`, Right request qualifies for row 12 on the
newly set F412, regardless of the output rectangle. F512 still has the same-call
clear-unlock behavior above. This re-entry takes `BattleLoop`, not ordinary Map52
init/population again; the four scene actors and their final positions are not the
battle placement contract.

`battleloop_1.asm:BattleLoop` clears `PLAYER_TYPE`, sets F399 and tests F88. F88 set
restores saved seconds, clears F88/AI memory and loads the saved battle, then jumps
to individual turns; it bypasses new placement and both wrappers and is outside this
new-battle endpoint. F88 clear zeroes seconds, stores map/battle and installs base
VInts before `ExecuteBeforeBattleCutscene`.

Exact row 12 in both `data/battles/cutscenes/beforebattlecutscenes.asm` and
`battlestartcutscenes.asm` targets `ms_Empty`. The shared wrappers use
`BATTLE_INTRO_CUTSCENE_FLAGS_START + 12 = 462`. F462 clear allows before dispatch
without setting it; start sets F462 before dispatch. F462 set skips both scripts,
not the intervening initialization. An empty script still reaches the interpreter's
conditional view-scroll wait and scrolling-speed reset.

After before-return, the loop restores map/battle, clears region flags 90–105, heals
living/immortal allies, initializes ally then enemy positions, clears AI memory,
loads battle for combatant 0 and calls the start wrapper. `InitializeAllAlliesBattlePositions`
resets positions and consumes the actual active list after `UpdateForce`, tests word
HP and skips dead members; list and placement-pointer advancement differ and the
remaining-tail branch can call `LeaveBattleParty`. `data/battles/spritesets/spriteset12.asm`
(`0x1B3D30`) declares twelve ally placement rows and twelve enemies. This is neither
twelve guaranteed deployed allies nor the four exploration actors. Peter's F7, F39,
HP and force AI remain separate from the F67 story follower and scripted participation.
The present scripts do not recruit him or confer player control.

`battlefunctions/loadBattle.asm:LoadBattle` fades out, loads tilesets and waits,
clears VInts, calls `PositionBattleEntities` to replace old entities, selects the
mapped view target, initializes sprites, loads map/mapsprites, restores base VInts,
loads terrain, plays map music and requests fade-in. See the shared
[battle placement owner](./battle-spriteset-data.md). The start wrapper then returns.
**Stop immediately before `BattleLoop:@Start` calls `ActivateEnemies`.** No activation,
turn, combat, outcome, postbattle scene or playable-control acceptance is claimed.

## Reproduction and review boundary

Load `. ./local/private-inputs.ps1` in the owning checkout's command process and
verify the read-only upstream pin and clean status. Run the existing
[retained payload/cutscene comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Select `entryLabel` values `cs_58512` and `cs_5C4EE` from `programCorpus.programs`;
compare every `sourceLine`, macro and argument list with the two bounded source
segments, inspecting inline actions separately. Select text keys `0591`, `059B..059E`
and `0575..0581` from `data/scripting/text/gamescript.txt`; no full transcript export.

| Relation | Existing fixture / exact direct check | Preserved alternatives |
| --- | --- | --- |
| Interaction | `map-events/routing-setup.json` ordinary row 3 and F513 row 2; `map-events/entity-programs.json` program 41; activation, reverse lookup and event-flag consumers. | Flags0 versus facing requirement; changed setup/actor identity; absent mapping; F711-set text versus actual talkability. |
| Scene programs/services | `map-script-engine-static-v1.json`; all 95 selected records; inline action bodies; existing `h3/map-script-entity-presentation-fx-v1.json` and `h3/map-entity-gesture-relationship-motion-v1.json`. | Word-operand/high-byte guard, missing Peter, waits/stalls, transient relation, shimmed effects and unknown presentation. |
| Map connection/setup | `map-data-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json`; Map13 warp row 3 and earlier south row; Map52 pointer/list rows. | Attribute gate, row priority, carried pending, F412 immediate admission, F512 empty population and independent F67. |
| Init/zone/request | `map-init-static-v1.json` body 65 plus direct `sub_5C4DC`; `map-events/zone-programs.json` program 97; named zone/request/main consumers. | F714/F712 interruption, F713 skip, F412 before pending before caller F713, service stall and mutable slot. |
| Battle start | `battle-global-data-static-v1.json`, `battle-routing-data-static-v1.json`, `battle-cutscenes-static-v1.json`, `battle-spriteset-data-static-v1.json`; exact row 12 and direct wrappers/placement/load consumers. | Wildcard versus area; F412/F512 same-call admission, F462 skip, F88 resume, actual party/HP/AI versus exploration cast. |

Static fixture names are under `tests/fixtures/h2/`; explicit `h3/` paths are under
`tests/fixtures/`. Existing controlled observations are reused only within their
published boundaries. No new parser, comparator, fixture, capture or runtime launch.

**Unknown:** natural interaction and walking/collision from Polca through the zone;
actual flags, actor mappings, HP, party and AI state; interruption-free entry;
quake/mosaic/fairy presentation and timing; full action, text, camera, fading, pending
and audio service completion; and continuous playable Battle12 entry. These do not
block the declared static result and do not become an acquisition queue.

Record direct checks/failures, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI in
the Issue/PR handoff. No local normal/full/engine/H3/H4/Godot suite solely for prose.
Leave a clean pushed Draft PR and Project Review / Verify; freeze the writer and
send the authorized completion callback. Independent main-gate owns acceptance and
merge. No cleanup or next slice is authorized here.
