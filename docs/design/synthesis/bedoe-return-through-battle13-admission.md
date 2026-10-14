# Bedoe Return through Battle13 Admission

Returning toward Polca can select a confrontation in which the devils attack two birdmen,
Luke describes his detention, and Gerhalt asks to join Bowie's force. The scene joins Gerhalt
and requests a battle handoff. Travel, scene selection, membership and battle admission have
independent conditions; none alone proves a continuous playable return.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/bedoe-return-through-battle13-admission.md) supports the selected
exits, cast, texts and service order below. **Inferred:** their staging depicts an attack on
Bedoe's soldiers and Gerhalt's intervention to protect Oddler. **Unknown:** natural travel,
supplied state, completed services, rendered action and usable battle input. This guide adds
no remake capability or product decision. Fnnn means flag nnn; coordinates are map tiles.

Read the preceding [king-choice guide](volcanon-return-through-bedoe-king-choice.md) for the
optional acceptance that can supply F799. The [hatchling flight and rescue](bedoe-hatchling-flight-and-rescue.md)
remain separate optional content: neither F723 nor F724 gates these exits or the confrontation.
This reading route imposes no hatchling prerequisite or compulsory order. Stop at the
new-battle start-dispatcher return, immediately before `ActivateEnemies`; combat, outcomes,
the mayor, raft fulfillment and Oddler's departure remain outside scope.

Continue with [Battle13 outcomes through Polca's raft grant](battle13-outcomes-through-polca-raft-grant.md)
for first/repeat outcomes, Luke's aftermath, fresh town return and the optional mayor bargain.
It separates following from recruitment/health, and the ordered raft configuration from
current-map presence, river access or boarding, stopping at conditional caller/control return.

## Returning toward Polca

**Confirmed selected requests:** Bedoe's south exit and the intervening west exit have these
ordered table matches. They describe requests, not the walking paths between them.

| Selected exit | No-scroll request |
| --- | --- |
| Bedoe/Map10, any X at Y31; warp row 34 | Map52 `(28,7)`, Down. Earlier rows do not match Y31; the following shrine row is separate. |
| Map52, X0 at any Y; warp row 1 | Polca/Map13 `(30,6)`, Left. Earlier row 0 at `(28,6)` returns to Bedoe instead. |

Neither exit tests F799 or the hatchling flags. Ordinary movement needs layout attribute
`$1000` to produce a warp. The producer writes pending type 1, destination and warp sound;
the no-scroll consumer uses sound services, idles main, unwinds exploration and supplies
coordinates to `MainLoop`. `SwitchMap` has no Map52/Map13 substitution here, then `CheckBattle`
runs. Pending state can change across services; a table match proves neither natural access
nor an uninterrupted request.

Battle selection precedes ordinary town entry. Both maps have one battle row, wildcard
trigger `(255,255)` and output area `(0,0,32,32)`. The output rectangle is not a point-admission
check: the selector tests map, unlock and trigger, then writes that area.

| Supplied destination state | Selection on this call |
| --- | --- |
| Map52, F412 clear | Ordinary exploration, independently of completion F512. |
| Map52, F412 set | Battle12; F512-set clears F412 but still selects Battle12 on this call. |
| Map13, F413 clear | Ordinary exploration, independently of completion F513. |
| Map13, F413 set | Battle13; F513-set clears F413 but still selects Battle13 on this call. |

The king choice, F715, Gerhalt membership and hatchling flags are not battle-admission tests.
A supplied F413 can select Battle13 without this confrontation. Earlier Battle12 completion
does not establish the supplied F413/F513 values.

Without battle admission, ordinary entry clears pending, adjusts the step counter, heals
living/immortal allies, stores map/not-in-battle, rebuilds entities/mapsprites, clears temporary
F256–383, sets F80 and runs map load/init. Map52's F512 override has no fixed NPCs instead of
the default's four; shared followers are separate. Its init tests F714, then the fall-through helper tests
F712: F714-set/F712-clear calls the accepted Peter response `cs_5C622`, then sets F712.
Arrival at `(28,7)` does not exclude that intervening content. This guide does not reinvestigate
it or treat arrival as a guaranteed immediate westward departure.

## Polca's Population Is Selected before the Zone

**Confirmed setup priority:** last-set override wins: **F513 > F799 > F710 > default**.
All four setups share zone/init handlers; F513 also changes entity events. The source filename
suffix 709 uses flag 710 symbols. Writing a flag neither rebuilds actors nor invokes the zone.

With F799 set and F513 clear, the fixed population declares Luke and Gerhalt followed by five
regular NPCs. Followers are built first. Lookup identities differ from physical slots:

| Lookup | Declared role and fixed origin |
| --- | --- |
| Bowie / 0 | Separate main actor at supplied arrival `(30,6)`, Left. |
| Peter / 7 | Supplied by follower F67; no fixed Peter row. |
| Luke / 10 | Fixed `(18,15)`, Right, `eas_Init`, unless already mapped by follower F75. |
| Gerhalt / 9 | Fixed `(11,9)`, Down, `eas_Init`. |
| 128 | Dark Soldier at `(22,15)`, Left, `eas_Init`. |
| 129 / 130 | Minotaurs at `(22,14)` / `(22,16)`, Left, `eas_Init`. |
| 131 / 132 | Birdmen at `(18,14)` / `(18,16)`, Right, `eas_Init`. |

With only F67 among follower flags, physical slots are Bowie 0, Peter 1, Luke 2, Gerhalt 3 and
NPCs128–132 in slots 4–8. With no follower flags, Luke/Gerhalt occupy slots 1/2 and the NPCs slots 3–7.
Other follower combinations change allocation. Polca admits followers but is not overworld;
F65 does not prepend a caravan here.

F75 supplies Luke before the fixed list, so his fixed row is skipped and he retains the follower
placement/action. **F74 supplies Astral / 31, king / 30 and minister / 159; it does not supply Gerhalt.**
Those non-ally rows affect allocation and can share supplied lookup identities. No follower row
supplies ally 9. F67-clear leaves Peter without a fixed declaration. Following tests flags rather
than force membership, deployment, HP or AI; the scene's names cannot establish any of those.

F513 overrides the confrontation population even with F799 set: regular lookups 128/129 are
centaurs, 130 the kappa and 131/132 merchants, with no fixed Luke/Gerhalt. Default/F710 populations
also differ. The zone does not test F513 or actor availability, so supplied state can select the
scene against a mismatched cast.

Lookup resolution masks the byte, translates NPC128+ and reads a physical-index mapping. A
cleared zero mapping resolves physical 0; `$FF` is not validated as an actor. `setPos`/`setFacing`
skip allies below 30 only when **word HP is zero**, so HP 256 is nonzero; NPCs bypass that guard.
Action, camera and text requests do not thereby gain a presence/HP test. A dead ally's possible
blue-flame sprite is neither revival nor membership.

Common init independently tests F711, F716 and F718, in that order. F711-clear runs `cs_58310`
to face 130 Down and wait its animation-off/Right-orientation/sprite-update/idle body, then reset
animation. In the F799 cast, 130 is a minotaur. F716-set faces 137 Down; F718-set moves 140 to
internal `$7000,$7000` without deleting it. Neither has a fixed F799 row, so a zero mapping can
affect main. Prior shrine F711 is possible history, not a predicate inferred from F799.
These init effects and services precede the later zone.

## Selecting the Confrontation

**Confirmed ordered zone selection:** X24/wildcard Y is first, ahead of wildcard X/Y11,
wildcard X/Y30 and default. X24 wins even at `(24,11)` or `(24,30)`; a handler return does not
fall through to another row. Ordinary movement needs `$1400` to produce the zone request.
Entry at `(30,6)` does not automatically execute it.

| `Map13_ZoneEvent0` supplied state | Caller result |
| --- | --- |
| F710 clear | Select the earlier Oddler-arrival branch, perform its F711-dependent postprocessing, set F710 and return. No confrontation in this call. |
| F710 set, F799 clear | Return. |
| F710/F799 set, F715 set | Return. |
| F710/F799 set, F715 clear | Execute `cs_585FE`, then set F715 after script return. |

This branch tests no F723/F724, F413/F513, F9/F41, follower flag, HP, AI or actor presence.
F715 is not written at scene entry. A supplied F715-set/F413-clear state suppresses the scene;
this caller contains no replay or repair contract.

## The Confrontation and Its Attributed Speech

**Confirmed requested sequence:** `cs_585FE` has 100 records: 87 commands, 12 sleeps and the
terminator. It begins with sound fade-out, sequential waited speed 48/idle bodies for 129–132,
waited auto-facing disable for 131/132, cursor 1468 and a waited Peter `eas_Init`. Camera movement
and enemy-attack music accompany the first exchange. The cursor requests all 19 texts 1468–1486:
16 `nextSingleText` and three `nextText` (1472,1479,1485). The table paraphrases attributed speech;
it neither reproduces a transcript nor validates the speaker's mapped identity.

| Text | Speaker lookup / attribution | Paraphrased content |
| --- | --- | --- |
| 1468 | 128 / Dark Soldier | Insults Bedoe's soldiers. |
| 1469 | Luke / 10 | Responds to the insult. |
| 1470 | Luke / 10 | Laments his comrades. |
| 1471 | 128 / Dark Soldier | Calls Luke a captive and threatens him. |
| 1472 | 128 / Dark Soldier | Warns against resistance. |
| 1473 | 128 / Dark Soldier | Demands the injured boy. |
| 1474 | Gerhalt / 9 | Objects. |
| 1475 | Gerhalt / 9 | Offers to fight and protect Oddler. |
| 1476 | Gerhalt / 9 | Calls out. |
| 1477 | Luke / 10 | Recognizes the leader and Peter. |
| 1478 | Peter / 7 | Asks why Luke is here. |
| 1479 | Luke / 10 | Says he came for a raft. |
| 1480 | Luke / 10 | Claims the devils control Polca and deceived him. |
| 1481 | Gerhalt / 9 | Apologizes to Luke. |
| 1482 | Gerhalt / 9 | Says Polcans will not fight and asks to join. |
| 1483 | Luke / 10 | Approves Gerhalt's decision. |
| 1484 | Luke / 10 | Asks not to be forgotten. |
| 1485 | 128 / Dark Soldier | Challenges continued resistance. |
| 1486 | 128 / Dark Soldier | Threatens death. |

Peter's request uses portrait operand `$C0`; the others use `$0`. These six requests by 128,
seven by Luke, five by Gerhalt and one by Peter are separate from join text 446 (`01BE`), which
announces Gerhalt joining using supplied class/name indices. Oddler is named in speech; no
Oddler lookup/placement/join command occurs. Luke's raft account grants no raft and establishes
no previous delivery. His plea is not recruitment: no `join Luke`, F10 or F42 write occurs.

After Luke's first response, he waits Right1. The minotaurs receive Left3, then Right3 actions;
129's first move is nonwaited, while 130's is waited. The birdmen receive Left2 in the same paired
start/wait pattern, then scripted `eas_Die` (start 131, wait 132). The minotaurs and Luke face Left;
Luke laments the birdmen, then 128 approaches Left1 and threatens the captive. The threat and boy
demand are interleaved with sleeps, 128's Up/Down oscillation and the minotaurs' approach.

Gerhalt objects; further enemy oscillations and Luke facing Up precede camera `(6,6)` and
Gerhalt's offer to guard Oddler. The camera tracks Gerhalt, while Bowie is placed `(20,9)` and
Peter `(19,8)`, both Down, subject to their HP guards. Gerhalt waits Down3/Right6, `eas_BumpRight`,
then Right1. He faces Up, the camera moves to `(14,8)`, and he calls out. The enemies face Up;
after Gerhalt faces Right, Luke waits Up1 and recognizes Bowie/Peter. Peter waits Down1 before
asking his question. Luke's raft/deception account follows. Gerhalt waits Right1, faces Down,
apologizes, then is tracked and waits Up1 before asking to join.

Joining precedes the camera tracking Luke and Luke's approval. Gerhalt faces Down and nods;
Luke waits Up1 and asks not to be forgotten. Minotaur 129 waits Up1/Left1. Luke's auto-facing is
then disabled and his speed changed to 48 with a waited idle body. His Left3 starts without a
wait; 129 waits Right1/Down1, then Luke's action is replaced by a waited animation-off/
Left-orientation/sprite-update/idle body. After 129 faces Left and a sleep 40, 128 waits Left1/Up1
and delivers the final two threats. Only then does the script set the battle unlock, queue the
warp and end.

**Confirmed action boundaries:** action installers replace pointers rather than queueing the
old tail. Wait forms poll the named entity's action pointer for `eas_Idle`. Waiting 130 does not
prove 129 finished; waiting 132 does not explicitly wait 131. Sleeps total 260 requested frames,
excluding movement/text/camera/join waits and debug skipping; this is not wall-clock duration.
Speed 48 is an internal movement parameter, not tiles per frame. Relative tile operands scale
by 384 internal units (24 pixels); layout, collision and updates still control actual movement.
Peter's `eas_Init` resets speed 32, acceleration/deceleration, obstruction/collision, auto-facing,
orientation and sprite-update state without joining, reviving or declaring a missing actor.
Gerhalt's bump requests relative `(1,0)`, waits 5,
`(-1,0)`, waits 5, `(0,0)`, reenables auto-facing and returns idle; its rendered effect is Unknown.

`eas_Die` turns animation motion off, cycles facing/sprite updates, requests battlefield-death
sound and `MAPSPRITE_EFFECT1`, then writes position/destination `(63,63)` and clears travel before
idle. It neither removes mappings nor invokes combat HP/death consumers. The birdmen therefore
receive scripted death presentation and off-map placement, not proved combat damage, rewards,
kill processing or victory. Rendering/audio and 131's concurrent progress remain **Unknown**.

Luke's replacement does not wait his destination or clear position, destination, velocity or
travel. Motion OFF writes animation counter `-1`; orientation is distinct from facing.
Entity position updates can continue toward the previous destination independently of animation.
Thus **neither completed Left3 nor a freeze at a known coordinate is proved**. Auto-facing remains
disabled and speed 48 retained until a later reset. No `startEntity Luke` follows here.

Camera destinations clear tracking and wait scrolling; tracking an actor does not itself wait.
The final tracked lookup is Luke, without presence/life validation. Text, portrait, speech,
window, nod, sprite and sound services retain their own waits and completion limits. The
terminator's conditional scrolling wait and speed reset are not an all-actor completion barrier.

## Gerhalt Joins, but Deployment Is Separate

**Confirmed membership order:** opcode `$08`, word 9 selects ordinary join music and Gerhalt's
`JoinForce` service through `j_JoinForce`; it does not take the paired-join 128 branch. The service
sets **F9**, rebuilds force/active/reserve lists with `UpdateForce`, then compares active count
with 12. Below 12, `JoinBattleParty` sets **F41=32+9** without rebuilding those cached lists.
The separate text 446 service waits through fade/input, closes text and sleeps 10; it does not
advance cursor 1468–1486 or disappear merely because Gerhalt was already joined. No naming prompt
is encoded here.

| Supplied party state | Join consequence |
| --- | --- |
| F41 clear; active count below 12 after F9 set | F9/F41 set, but cached lists still reflect F41-clear until another `UpdateForce`. |
| F41 clear; active count at least 12 | F9 set, F41 stays clear; Gerhalt remains reserve in the rebuilt snapshot, with no replacement chosen. |
| F41 already set | It stays set, including full/overfull states; the rebuild includes Gerhalt after F9 set, without eviction or normalization. |

These helpers neither test HP nor heal/revive, create a field actor, change following or write AI.
This scene has no `joinForceAI`, `followEntity` or `addFollower`; F67/F74/F75 and supplied force
AI remain unchanged. Gerhalt's fixed field presence, F9 membership, F41 active selection, cached
lists, health and actual placement differ. Peter and Luke retain their own independent states.
No player-controlled turn or particular Gerhalt battle coordinate is promised.

## Requesting and Initializing Battle13

**Confirmed handoff order:** `setStoryFlag 13` adds 400 and sets **F413**, not F13. The warp writes
pending type 1, parameter 1=0 and Map13/X0/Y0/Right; it does not switch map, start battle or write
`WARP_SFX`. Its consumer uses the carried sound value. Script return precedes caller **F715**.
The zone wrapper then closes portrait/text, waits a VInt and waits for physical 0 to stop.

The conditional chain is **F413 → pending warp → script return → F715 → wrapper return →
pending consumption → battle selection**. Pending makes `WaitForEvent` bypass normal camera 0/
controlled-action restoration; consumption clears pending and unwinds to `MainLoop`. Each service
and the request's survival remain conditions. A stall can retain earlier writes without later
completion. Flags are not receipts, and this sequence is not an atomic transaction or repair rule.

Intact Map13 `(0,0)` selects row 13, `BATTLE_POLCA_VILLAGE`, on F413, with output area
`(0,0,32,32)`. F513-set clears F413 while selecting that call. `BattleLoop` follows rather than
ordinary Map13 population/init. Hidden birdmen, field destinations and Luke's interrupted action
do not define the battle population.

`BattleLoop` clears `PLAYER_TYPE`, sets F399 and tests F88. **F88-set** restores saved seconds,
clears F88/AI memory, loads the saved battle and goes directly to individual turns, bypassing
new placement and both wrappers; that alternative is outside this new-battle endpoint.
**F88-clear** zeroes seconds, stores current map/battle and installs base VInts before the
before wrapper. Actual row 13 before/start targets are both **`ms_Empty`**.

The wrappers share **F463=450+13**. Before tests without setting it; start tests and sets it
before dispatch. F463-set skips both scripts, not intervening initialization. F463-clear executes
the empty program, whose terminator still has conditional scrolling wait and speed reset.
There is no Battle13 intro script here, but an empty target does not imply an empty service path.

Between wrappers, initialization restores current map/battle, clears region flags 90–105, heals
living/immortal allies, initializes ally then enemy positions, clears AI memory and loads battle
for combatant 0. Ally placement first resets X and calls `UpdateForce`, refreshing any new F41
in cached lists. It consumes the actual active list and skips zero **word** HP. List advancement
and placement-pointer advancement differ; a remaining-tail branch can call `LeaveBattleParty`.
The resulting deployment is not fixed by scene order.

Accepted selected source declares 12 ally placements, 16 enemies, three AI regions and one AI point
in `spriteset13`. Those 12 rows are capacity, not named allies or 12 guaranteed living members.
Enemy initialization uses the actual battle entries/types rather than the five exploration NPCs.
`LoadBattle` fades/loads resources, waits, replaces entities and mappings from positioned
combatants, initializes view/sprites/terrain/music and requests fade-in. Invalid coordinates
and enemy neutral state affect declarations. Field actions/followers and the F799 cast are not
preserved as battle state; AI declarations/memory clearing prove no executed AI or player control.
The start wrapper returns. **Stop before `BattleLoop:@Start` calls `ActivateEnemies`.**

## Evidence Review and Remaining Limits

This guide consumes research accepted in
[PR #802](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/802), with preceding English
king-choice and optional-hatchling guides, from accepted main only. Research owns USA retail ROM
identity, pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, scene `0x585FE`, zone
`0x5823A`, init `0x582E0` and selected source/service composition. Its
[reproduction boundary](../../research/bedoe-return-through-battle13-admission.md#reproduction-and-evidence-limits)
retains source/retained-payload comparisons. This slice reviews accepted prose and bounded
tracked-fixture selections without source reinvestigation, derived payload copying/generation
or new observation.

| Reviewed boundary | Existing fixture / contract / consumer | Disposition |
| --- | --- | --- |
| Caller | [sf2-map-events-static-v1:zone-programs](../../../tests/fixtures/h2/map-events/zone-programs.json), program 9; [zone schema](../../../schemas/h2/map-events-zone-programs.schema.json), [fixture loader](../../../src/sf2tool/h2/map_events_fixture.py). | All 18 operations preserve first-arrival return and F710/F799/F715 order, with scene before F715. No optional-scene prerequisite. |
| Init and population | [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), bodies 18/65; [init verifier](../../../src/sf2tool/h2/map_init.py); [map exploration](../contracts/map-exploration.md). | Body 18's 13 operations independently test F711/F716/F718. Body 65's two-operation boundary ends before the Map52 helper; accepted selected research supplies that fall-through. Setup/entity inventories do not contain this complete cast composition. |
| Dialogue, actors and joining | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json); [dialogue](../contracts/dialogue-system.md), [party/roster](../contracts/party-roster-state.md); [map-script verifier](../../../src/sf2tool/h2/map_script_engine.py). | Family facts own handlers, not this complete 100-record scene/text corpus. Accepted research owns all 19 texts, join 446, action interruption and F9/F41/cache composition; shared runtime seams are not natural scene proof. |
| Battle handoff | [sf2-battle-routing-data-static-v1](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), [sf2-battle-cutscenes-static-v1](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), [battle routing contract](../contracts/battle-cutscene-routing.md); [cutscene verifier](../../../src/sf2tool/h2/battle_cutscenes.py). | Route fixture explicitly leaves exact targets unparsed; wrapper facts own shared flag order. Actual row 13 empty targets come from accepted selected research, independently of Battle12. |
| Battle population | [sf2-battle-spriteset-data-static-v1](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json); [placement research](../../research/battle-spriteset-data.md). | `rowContentTracked=false`: selected row 13 counts are accepted source evidence, not per-row fixture coverage or Gerhalt placement proof. |

The [init schema](../../../schemas/h2-map-init-static-fixture.schema.json) and verifier retain
complete body/provenance equality; [map-script schema composition](../../../src/sf2tool/h2/map_script_schema.py)
keeps the command families separate. Existing focused checks live in `tests/python/test_map_events.py`,
`test_map_init.py` and `test_map_script_engine.py`; this documentation scope does not rerun them.

Existing narrow evidence routes are `uv run sf2 h2 map-events`, `map-init`, `map-script-engine`,
`map-data`, `map-setup`, `map-entities`, `battle-routing-data`, `battle-cutscenes` and
`battle-spriteset-data`; naming them is not a new execution claim. Selected checks preserve the
stricter executable boundary rather than expanding aggregate inventories into route observations.

**Unknown:** natural walking/collision and uninterrupted arrival; actual flags, actors, mappings,
HP, party/AI and positions; concurrent action progress; camera/text/sprite/fade/audio/pending
completion; rendered birdmen deaths or Luke pose/motion; responsive control, continuous Battle13
playability and wider persistence. These questions do not create an acquisition queue. Retained
predecessor comparisons/failures remain with the research handoff; this prose slice does not rerun
them or claim a new runtime pass. It completes neither the campaign nor the design epic.
