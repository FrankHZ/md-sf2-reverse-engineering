# Polca's Fairy Interaction through Battle12 Admission

Polca offers an optional encounter with a turtle asking to be turned over. Bowie and Peter receive
staged movement and pushing requests; the turtle's last speech describes a test and departure as
the script requests a fairy sprite and mosaic effects. Leaving town to the east is independent of
that interaction. On the ordinary Mount Volcano entry branch, a later zone can stage Peter observing
four searching devils, their account of battle marks and rocks, and their confrontation with the
visitors. The scene then requests a same-map return that can admit Battle12.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/polca-fairy-through-battle12-admission.md) supports the interactions,
connections and ordered handoffs below. **Inferred:** they form a candidate content continuation
from [Polca's first Oddler arrival](battle11-outcomes-through-polca-arrival.md).
**Unknown:** natural travel, supplied flags and actors, completed services, rendered transformation
and playable battle entry. Neither helping the turtle nor witnessing Oddler's arrival is a required
predicate of the selected east exit. This guide adds no remake capability or product decision.

Stop at the new-battle start-dispatcher return immediately before `ActivateEnemies`. Combat,
outcomes, `cs_5C622`, Bedoe/Volcanon scenes and successor findings are outside this guide. Fnnn means
flag nnn; map and battle numbers retain their source meanings.

## Helping the Turtle Is an Optional Interaction

**Confirmed routing:** in Polca's default and F710 populations, regular NPC lookup 130 is the
kappa/turtle at `(1,11)`, Down, using `eas_InitSlow`. Gerhalt's ally row does not consume a regular
NPC index. The [preceding population guide](battle11-outcomes-through-polca-arrival.md#the-town-population-before-the-incident)
retains the last-set setup priority F513 > F799 > F710 > default and Peter's follower-selected
presence. These conditions determine what the interaction can address; an event label alone does
not establish an available turtle.

Ordinary entity activation is reached through C after the debug/caravan alternatives, while A opens
the menu. Activation uses a nonnegative view target and a point offset in its facing direction,
continues across counter blocks, and scans physical entities excluding the controlled entity and
followers. A candidate must be within Manhattan distance less than one tile, 384 internal units,
from that point. The selected physical slot is reverse-mapped through `ENTITY_INDEX_LIST` before
the setup dispatcher matches its entity index. No natural walk to a usable activation point has
been demonstrated here.

Ordinary entity-event row 3 selects `Map13_EntityEvent3` for lookup 130. Its operand spelled `RIGHT`
is numerically 0 and is stored as **event flags**, not a facing admission predicate. The dispatcher
matches the entity index; flags bits 0 and 1 would request temporary facing toward the player and
restoration afterward. Both are clear, so this row supplies neither an automatic turn/restore nor
a requirement that the player face Right.

The caller has two branches:

| Supplied F711 | Handler behavior |
| --- | --- |
| Clear | Run `cs_58512`; only after it returns, set F711 and return. |
| Set | Request text 1425 (`0591`) about ancient magic tunnels and return. |

The second branch does not establish that the departed actor remains available for another
conversation. F711-set init skips the turtle's staging but does not itself hide 130. The separate
first Oddler caller, after rebuilding residents, hides 130 when F711 is set or stages him again
when clear. Those are different consumers of the flag.

Supplied populations also change the meaning of lookup 130. F799 keeps the ordinary event table
but makes 130 a minotaur. F513 uses another event table whose row 2 routes 130 with flags 0 to the
same handler; its kappa starts at `(1,13)`. These are selection alternatives, not evidence of a
sensible turtle scene under every flag combination. A missing mapping can resolve physical main
slot 0 rather than safely skipping an operation.

F711 is not an exclusive rescue-history bit. The accepted research records another producer:
`Map11_ZoneEvent0`, with F722 clear, calls its separate scene and then sets F722 and F711. Only that
flag relationship is consumed here; the other scene's content is excluded. F711 alone therefore
proves neither that Bowie helped this turtle nor that actor 130 is present.

## The Test and the Fairy's Departure

**Confirmed ordered content:** `cs_58512` requests four texts from actor 130, IDs 1435–1438
(`059B..059E`). Their paraphrased content and staging are:

1. The turtle asks for help turning over (1435). Its obstruction is disabled and its action is
   waited on; camera tracking becomes neutral. Bowie and Peter receive initialization actions,
   Peter is requested at `(2,12)` and faced Left, and Bowie receives the first waited bump.
2. The turtle asks for a harder push (1436). Bowie receives a nod and two more waited bumps.
   The turtle then offers encouragement (1437). The bump action uses small relative motion and
   waits; its units are separate from the scene's tile-step movement commands.
3. Bowie is requested Down1, given a waited speed action, then requested Up1. Quake 2 and reset 0
   bracket a sleep. Actor130 receives `MOSAIC_OUT`, a non-waited action writing Up orientation and
   `MAPSPRITE_FAIRY=219`, then `MOSAIC_IN`. After another sleep and Down-facing request, its final
   text describes the test and departure (1438).
4. Actor130 receives `eas_InitSlow`, auto-facing is disabled, and Up10 is waited on. Bowie is reset
   and Peter is instructed to follow him before the script ends. No join, population reload, story
   flag or warp command occurs inside this scene.

This is a paraphrase of the four text requests, not a transcript. The fairy sprite write belongs
to an inline entity action; it is not a recruitment or the map-script `setSprite` command. Up10
requests movement, rather than deletion or guaranteed off-map placement. The caller's later F711
write is distinct from all of these requests.

**Confirmed service limits:** the seven sleeps total 200 requested frames, not scene duration.
Waited actions poll for `eas_Idle` and have no general missing-actor or HP skip. `setDest` scales tiles
by 384 and waits for motion here; it does not instantly place Peter. Facing uses a word-HP guard.
Quake writes amplitude. The mosaic selectors enter sixteen-iteration source loops that call sprite
loading, processing, DMA and VInt services. Neither that structure nor the interposed non-waited
sprite action proves a visible transformation or its timing.

The [existing controlled effect evidence](../../research/common-scripting.md#confirmed-map-script-entity-presentation-fx-command-family)
confirms selector entry/loop/return chronology only at its named seam. Resolver, loading, crop/DMA
and other services are shimmed there; their original bodies do not execute in that matrix. It does
not certify this Polca scene's rendering or completion.

The final `followEntity` encodes Peter as word `$0007`. Its handler probes that word's high byte,
so the word-HP check is **Bowie0's**, not Peter7's. Bowie at zero HP skips the operands. With Bowie
alive and valid mappings, selector 2 adds Peter following Bowie at `(-24,0)`, independently of
Peter's HP. Missing Peter can resolve to main. This writes no F7, F39, F67 or force-AI membership
and grants no player control over Peter.

The relation and fairy sprite belong to the current entity state. After the script returns and
its caller sets F711, pending zero permits the shared event wait to restore camera target 0 and
controlled action; another pending request wins that poll. A later ordinary entry clears and
rebuilds entities/followers from persistent flags. F67 can independently rebuild Peter, but that
is not preservation of this transient follow relation or sprite. The scene has no explicit reload
of the kind used by the earlier Oddler event.

## Leaving Polca for Mount Volcano

**Confirmed selected connection:** Map13's east warp row 3, X31/wildcard Y, requests Map52
`(1,28)`, Right, without scrolling. Map52 is Mount Volcano. The row tests none of F710, F711,
Battle11 completion or Peter membership. At `(31,31)`, the earlier wildcard-X/Y31 south exit wins;
the east row is not the selected result at that overlap.

Movement must encounter the layout's `$1000` attribute before the warp producer scans ordered
rows. The consumer clears pending type before dispatch, performs sound and motion/return services,
and supplies the mutable map/X/Y/facing tuple to `MainLoop` for `SwitchMap` and `CheckBattle`.
There is no flag-switched substitute for Map52. A matching coordinate or destination row proves
neither collision-free travel nor that the parameters survive intervening services/writers.
The [request owner](../../research/map-event-request-consumption.md) retains that boundary.

Map52's battle row 12 has area `(0,0,32,32)`, wildcard trigger `(255,255)`, unlock F412 and
completion F512. The selector tests the trigger, not containment within the output area. The intact
east-exit request therefore has these alternatives:

| Supplied state | Immediate selection |
| --- | --- |
| F412 clear, either F512 | Ordinary Map52 entry; no Battle12 admission on this call. |
| F412 set, F512 clear | Battle12 immediately, before ordinary map population/init and the searching scene. |
| F412 set, F512 set | Clear F412 and still select Battle12 on this same call. |

F713, F710 and F711 are not predicates of that admission. A supplied unlock can bypass the scene;
completion does not veto the same-call selection. The candidate content below describes the
ordinary-entry branch, rather than all possible Battle12 entries.

Ordinary entry clears pending, adjusts the step counter, heals living/immortal allies, sets the
current map outside battle, rebuilds entities/mapsprites, clears temporary flags 256–383, sets F80,
and runs map load/init. Default Map52 supplies four exploration actors, all Down with `eas_Init`:
Death Archer128 at `(21,7)`, Hobgoblin129 at `(18,9)`, Witch130 at `(23,8)` and Evil Cloud131 at
`(21,15)`. These are the scene cast, not the battle roster.

F512 selects an **empty** fixed population while retaining the same event/zone/init pointers.
With F512 set and F713 clear, the scene caller can remain selectable despite missing all four
lookups. Peter has no fixed row; F67 can supply him independently of joined/active flags, HP and
force AI. Other followers change physical order. Map52 admits followers but is not overworld,
so F65 does not supply a caravan prefix. A dead-ally sprite alternative repairs neither mapping
nor membership. These supplied-state risks are not additional admission checks.

Init is another independent interruption. F714 clear returns; set falls through to `sub_5C4DC`,
which tests F712, invokes the excluded later scene only when clear, then sets F712. The primary
map-init fixture body contains only the initial two instructions; the accepted research's helper
composition supplies the rest. Arrival coordinates and F412-clear alone do not establish that this
interruption is absent.

## Observing the Devils and Being Confronted

**Confirmed selection:** points `(3,23)` and `(4,23)` target `Map52_ZoneEvent0`; the default only
returns. Zone production requires `$1400`. Arrival at `(1,28)` does not automatically run the scene.
F713 set returns; clear runs `cs_5C4EE`, then sets F713 after script return. The caller checks neither
F512, actor availability nor turtle/Oddler history.

The script requests thirteen texts, IDs 1397–1409 (`0575..0581`): two Peter, three Evil Cloud131,
two Hobgoblin129 and six Death Archer128. Their paraphrased sequence is:

1. Bowie and Peter receive initialization and destination requests to `(5,23)` and `(4,22)`;
   Peter moves Right1. He asks who the strangers are (1397). The camera is requested at `(16,4)`
   while Hobgoblin129 and Witch130 receive repeated, non-waited oscillation actions. Peter describes
   them as searching (1398).
2. Evil Cloud131 moves Up6 and reports finding no target under the cliff (1399). Hobgoblin129
   moves up/right; Death Archer128 is faced Left. The hobgoblin describes battle marks (1400) and
   asks about birdmen (1401).
3. Death Archer128 receives a nod and responds about the birdmen and rocks (1402–1403). More
   oscillation and sleeps follow. Its next two texts attribute the rocks to Volcanon and continue
   that account (1404–1405). These four responses report the devils' explanation; they do not
   independently prove the cause of earlier events.
4. Evil Cloud131 moves down, receives a waited speed action and approaches down/left. It asks
   who is there (1406). The camera shifts toward Bowie/Peter, their facings change, and the camera
   returns toward the devils. Evil Cloud131 resets and moves Up1 before identifying the visitors
   as Polca children (1407).
5. Death Archer128 moves down/left/down, disparages Polca (1408), and is faced Up along with 131.
   Its last text threatens the visitors (1409). The script then writes the battle unlock and
   requests a same-map warp before terminating.

These are paraphrases, not transcripts or proof that all actors reached their requested positions.
The scene makes no join, follow, population-reload or force-AI change. Witch130 participates in
motion staging without a text request. The nine sleeps total 215 requested frames; camera,
movement and text services add waits. Camera destinations wait for scrolling, text/debug paths can
skip work, and the terminator conditionally waits for scrolling and resets its speed. Actual cast,
HP, motion, view, dialogue, sound and service completion remain **Unknown**.

## The Request Becomes Battle Admission in Stages

**Confirmed order:** the final `setStoryFlag 12` adds the battle-unlock base 400 and sets **F412**,
not literal F12. The next command writes pending type 1 and Map52 `(0,0)`, Right, with no scroll.
It does not immediately switch map or set `WARP_SFX`; consumption uses the carried sound value.
Only after the script terminates does its caller set **F713**.

The zone dispatcher then closes portrait/text, waits a VInt and waits for main to stop. With type 1
intact, the exploration event poll skips restoring controlled input. `ProcessMapEvent` clears the
type, consumes the request and returns to `MainLoop`, which switches map and checks battle again.
Thus the order is **F412 → pending warp → caller F713 → request consumption → battle selection**.
It is not an atomic scene-to-battle switch: an early service stall can prevent the writes, while a
later stall can leave some or all written without consumption. Intervening writers can change the
mutable request. F713-set/F412-clear does not repair itself by revisiting the zone, because the
caller skips the scene.

The intact `(0,0)` request qualifies for Battle12 on F412 using the wildcard trigger. F512 retains
its same-call clear-unlock behavior. This branch enters `BattleLoop` rather than running ordinary
Map52 init/population again. The exploration actors' positions are not battle placement.

## The Empty Opening Still Has a Battle Lifecycle

**Confirmed new-battle handoff:** `BattleLoop` clears player type, sets F399 and tests F88. A supplied
F88 takes the resumed-battle branch: restore saved seconds, clear F88/AI memory, load saved battle
state and jump to individual turns, bypassing fresh placement and both intro wrappers. That branch
is outside this guide's new-battle endpoint.

With F88 clear, the loop zeroes seconds, stores map/battle and installs base VInts before the
before-battle wrapper. Battle12's before and start rows both select `ms_Empty`. Shared intro flag
F462 clear allows before dispatch without setting it; start sets F462 before dispatch. F462 set
skips both scripts, but not the intervening initialization. Empty scripts still perform their
terminator's conditional view-scroll wait and scrolling-speed reset.

Between before-return and start dispatch, the loop restores map/battle, clears region flags 90–105,
heals living/immortal allies, initializes ally then enemy positions, clears AI memory and loads the
battle for combatant 0. Actual ally placement consumes the active list after `UpdateForce`, tests
word HP and skips dead members; list and placement-pointer advancement differ, and the remaining
tail can remove members from the battle party. The spriteset declares twelve ally placement rows
and twelve enemies. It guarantees neither twelve deployed allies nor reuse of the four scene actors.
Peter's F7/F39, HP and force AI remain separate from F67 story following and script participation.
This content recruits him nowhere and grants no player control over him.

The battle loader fades out, loads tilesets and waits, replaces field entities with battle entities,
selects the mapped view target, initializes sprites, loads map/mapsprites, restores base VInts,
loads terrain, plays map music and requests fade-in. The start wrapper then returns.
**Stop immediately before `BattleLoop:@Start` calls `ActivateEnemies`.** Enemy activation, turns,
combat, outcomes, completed presentation and usable battle input are not accepted by this guide.

## Evidence Review and Remaining Limits

This guide consumes research accepted in [PR #778](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/778)
and the preceding English guide from [PR #777](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/777),
without a stacked dependency or successor consumption. Research owns the pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, USA retail identity, exact source/text/row composition
and [reproduction boundary](../../research/polca-fairy-through-battle12-admission.md#reproduction-and-review-boundary).
Bounded review uses accepted prose, selected tracked fixtures and their contracts/schema/verifiers;
no optional payload generation/copy, source reinvestigation or runtime observation is required.

| Reviewed relation | Existing evidence and its limit |
| --- | --- |
| Interaction and caller | [sf2-map-events-static-v1:routing-setup](../../../tests/fixtures/h2/map-events/routing-setup.json), [entity-programs](../../../tests/fixtures/h2/map-events/entity-programs.json) program41 `Map13_EntityEvent3`, and [map exploration contract](../contracts/map-exploration.md). Preserve event flags0, actual lookup and script-return-before-F711. Selection evidence does not prove natural talkability. |
| Scene commands and following | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), [party/roster contract](../contracts/party-roster-state.md), [sf2-map-entity-gesture-relationship-motion-runtime-v1](../../../tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json) and [sf2-map-script-entity-presentation-fx-runtime-v1](../../../tests/fixtures/h3/map-script-entity-presentation-fx-v1.json). Public family-site/handler facts are not a complete scene transcript; controlled HP/effect seams do not certify this composed scene or rendered fairy. |
| Departure and ordinary entry | [sf2-map-data-static-v1](../../../tests/fixtures/h2/map-data-static-v1.json), [sf2-map-setup-static-v1](../../../tests/fixtures/h2/map-setup-static-v1.json) and [sf2-map-entities-static-v1](../../../tests/fixtures/h2/map-entities-static-v1.json). Research supplies exact warp/population joins; inventories do not prove natural travel or the current cast. |
| Init and scene request | [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json) body65 and [sf2-map-events-static-v1:zone-programs](../../../tests/fixtures/h2/map-events/zone-programs.json) program97 `Map52_ZoneEvent0`. The two-instruction primary init body does not contain the helper fallthrough; research owns that composition. Ordered zone operations establish script then caller F713, not request consumption. |
| Battle selection and start | [sf2-battle-global-data-static-v1](../../../tests/fixtures/h2/battle-global-data-static-v1.json), [sf2-battle-routing-data-static-v1](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), [sf2-battle-cutscenes-static-v1](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), [sf2-battle-spriteset-data-static-v1](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json) and [battle lifecycle contract](../contracts/battle-control-lifecycle.md). Aggregate routing retains `routeTargetsParsed=false`; research supplies exact row12/empty targets and placement composition. No playable Battle12 observation is implied. |

The [event fixture loader](../../../src/sf2tool/h2/map_events_fixture.py) and
[shard schema](../../../schemas/h2/map-events-fixture-shard.schema.json) preserve shard identity and
ordered program operands; the [map-init verifier](../../../src/sf2tool/h2/map_init.py) and
[schema](../../../schemas/h2-map-init-static-fixture.schema.json) preserve delimited bodies, not a
whole-event runtime trace. The [script schema composition](../../../src/sf2tool/h2/map_script_schema.py)
and [verifier](../../../src/sf2tool/h2/map_script_engine.py) retain handler and family-site boundaries.
Narrow owners include `uv run sf2 h2 map-events`, `map-init`, `map-script-engine`, `map-data`,
`map-setup`, `map-entities`, `battle-global-data`, `battle-routing-data`, `battle-cutscenes` and
`battle-spriteset-data`. These are reproduction routes, not claims of fresh extraction or execution
in this prose-only slice. No Layer A correction is needed for the bounded explanation.

**Unknown:** natural interaction and collision-free travel from Polca through the mountain zone;
actual flags, actors, mappings, HP, party and AI; interruption-free entry and request survival;
action/text/camera/pending/audio/fade completion; quake/mosaic/fairy appearance and timing; and
continuous playable Battle12 entry. These remain limits, not an acquisition queue. The guide ends
before enemy activation and completes neither the campaign nor the wider design epic.
