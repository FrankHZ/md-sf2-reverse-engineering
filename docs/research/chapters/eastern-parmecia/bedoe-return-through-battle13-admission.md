# Bedoe return through Battle13 admission

Evidence date: 2026-10-13. **Confirmed:** the selected Bedoe south exit requests
Map52, whose west exit requests Polca. Polca's X24 zone selects `cs_585FE` only with
F710 and F799 set and F715 clear. The scene stages a confrontation, joins Gerhalt,
sets Battle13 unlock F413 and queues a Map13 warp; its caller sets F715 after the
script returns. Battle13 admission independently tests F413, and both before/start
rows select `ms_Empty`. **Inferred:** the staging and attributed texts depict devils
attacking birdmen and detaining Luke. **Unknown:** natural travel, supplied state,
actual actors, completed services, rendered action and playable battle entry.

## Agreement, provenance and resources

Standard rigor under [Issue #800](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/800)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
travel, setup selection, asynchronous actors, joining and battle admission need
separate acceptance boundaries. This is the sole tracked owner, with no shared-path
changes or stacked dependency. Consumed accepted main is
`177dd62d91833624ddffbf82881c567721a2ba41`. Dependencies are the accepted
[king choice](./volcanon-return-through-bedoe-king-choice.md),
[shrine audience and membership consumers](./bedoe-to-volcanon-audience.md),
[Polca arrival/setup](./battle11-outcomes-through-polca-arrival.md),
[Map52 and Battle12 admission](./polca-fairy-through-battle12-admission.md),
[request consumption](../../world/map-event-request-consumption.md),
[common scripting](../../world/common-scripting.md), [battle cutscenes](../../battle/battle-cutscenes.md)
and [battle placement](../../battle/battle-spriteset-data.md).

Scope is the selected two exits, their competing battle/init branches, Map13
population and X24 caller, complete `cs_585FE`, relevant services, and Battle13's
new-battle start-dispatcher return. Stop before `ActivateEnemies`, first turn or
`cs_58856`. Exclude battle outcomes, mayor/raft/Oddler departure, later travel or
recruitment, floor-path inventories, optional Bedoe scene reinvestigation, runtime
acquisition and implementation. F723/F724 are not prerequisites here. No inseparable
scope expansion is needed; Unknowns do not authorize acquisition.

Source paths below are relative to `disasm/` in
[pinned SF2DISASM `master` revision `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Existing payload/fixture and cutscene-source comparisons pass. This is static
evidence, not a new composed observation. Fnnn means flag nnn. Table rows and
fixture program orders are zero-based; scene records are one-based positions in
`programCorpus.programs[].commands`, including sleeps and the terminator.

The selected `data/maps/entries/map13/mapsetups/scripts.asm` segment is exactly
186–351, **166 source lines**, before `cs_58856` at 352. Reuse the unchanged
18,285,979-byte pair in one process with linear selected reads/comparisons,
estimated below 512 MiB peak and one minute of comparisons, and small prose/receipts.
Existing comparisons took 0.081 seconds; selected direct checks took 0.063 seconds.
All 100 scene records match source-line macros/operands; 21 inline movement bodies
and six custom bodies are reviewed separately. The actual before/start row 13
selection is checked, rather than inherited from Battle12. No new parser,
comparator, per-scene fixture, export or capture is needed. Replan before material
expansion. Local acceptance is direct source/fixture/document review and the clean
committed planner, with final-head CI and independent main-gate review at handoff.

## Selected exits and competing admission

**Confirmed:** `data/maps/entries/mapNN/6-warp-events.asm` supplies these requests:

| Source / row | Match, no-scroll request | Destination |
| --- | --- | --- |
| Map10 / 34 | wildcard X, Y31, Bedoe south exit | Map52 `(28,7)`, Down |
| Map52 / 1 | X0, wildcard Y, west exit | Map13 `(30,6)`, Left |

Map10's earlier rows do not match Y31; its following shrine row is `(45,50)`.
Map52's earlier row 0 is `(28,6)` back to Bedoe `(12,30)`, Up, not the west exit.
These comparisons establish selected table matches, not routes between coordinates.
There is no F799/F723/F724 check on either exit. No floor traversal is inferred.

Ordinary movement in `entityscriptengine_2.asm` requires layout attribute `$1000`
before `WarpIfSetAtPoint` scans eight-byte rows and writes pending type 1, its tuple
and warp SFX. `explorationfunctions_2.asm:ProcessMapEventType1_Warp` consumes the
no-scroll request through the sound service, idles main, unwinds exploration and
samples/replaces coordinates. `MainLoop` then calls `SwitchMap` and `CheckBattle`.
`data/maps/global/flagswitchedmaps.asm` has no Map52 or Map13 replacement. The pending
slot remains mutable across services; these declarations prove neither production
from a natural walk nor uninterrupted consumption.

The sole Map52 battle row is 12; the sole Map13 row is 13. Both have wildcard
trigger `(255,255)` and output area `(0,0,32,32)` in
`data/battles/global/battlemapcoords.asm`. `code/common/maps/getbattle.asm:CheckBattle`
tests map, unlock and trigger, then writes the area. It does not test whether the
requested point is inside that rectangle.

| Requested map | Unlock clear | Unlock set, completion clear | Both set |
| --- | --- | --- | --- |
| Map52 `(28,7)` | Ordinary entry, independent of F512 | F412 selects Battle12 immediately | F512 clears F412, **still selecting Battle12 on this call** |
| Map13 `(30,6)` | Ordinary entry, independent of F513 | F413 selects Battle13 immediately | F513 clears F413, **still selecting Battle13 on this call** |

Thus supplied battle unlocks can interrupt either ordinary arrival. F715, the king
choice, hatchling flags and Gerhalt membership are not battle-admission predicates.
Battle13 can be admitted without the confrontation. The earlier Battle12 outcome
does not establish the supplied F413/F513 values.

Absent Map52 battle admission, ordinary supplied-map entry clears pending, adjusts
the step counter, heals living/immortal allies, stores map/not-in-battle, rebuilds
entities/mapsprites, clears temporary F256–383, sets F80 and runs map load/init.
Map52 setup has its F512 override; its default has four fixed NPCs, while F512's
fixed list is empty. Shared follower construction remains independent. Its common
`s6_initfunction.asm:ms_map52_InitFunction` at `0x5C4D6` tests F714: clear returns;
set falls through to `sub_5C4DC`, which calls `cs_5C622` only when F712 is clear,
then sets F712. The primary retained init body 65 contains only the first test/branch;
the helper is essential to the interruption boundary. No new audit of that scene
or of Battle12 outcomes is made. Arrival at `(28,7)` does not establish its absence.

## Polca setup, init and actual actors

**Confirmed:** ordinary Map13 entry uses the same initialization services.
`data/maps/mapsetups.asm` and `GetCurrentMapSetup` select the last set override:
**F513 > F799 > F710 > default**. Filename suffix 709 contains flag 710 symbols.
All four setups share the zone/init handlers; F513 also changes entity events.
Flag writes alone neither rebuild actors nor invoke a zone.

For F799 set and F513 clear, `s1_entities_799.asm` declares two ally rows followed
by five regular rows. `mapfunctions.asm:InitializeMapEntities` clears state, builds
followers, then fixed entities; regular mapping starts at 128 and is separate from
ally mapping and physical slot allocation. With **only F67** among follower flags,
the resulting declarations are:

| Source row | Lookup | Physical slot in this bounded example | Origin, identity and action |
| --- | --- | --- | --- |
| Separate main | Bowie0 | 0 | Supplied arrival `(30,6)`, Left, idle |
| Follower prefix | Peter7 | 1 | F67 follower, independent of force membership |
| 0 | Luke10 | 2 | `(18,15)`, Right, `eas_Init` |
| 1 | Gerhalt9 | 3 | `(11,9)`, Down, `eas_Init` |
| 2 | 128 | 4 | `(22,15)`, Left, Dark Soldier, `eas_Init` |
| 3 | 129 | 5 | `(22,14)`, Left, Minotaur, `eas_Init` |
| 4 | 130 | 6 | `(22,16)`, Left, Minotaur, `eas_Init` |
| 5 | 131 | 7 | `(18,14)`, Right, Birdman, `eas_Init` |
| 6 | 132 | 8 | `(18,16)`, Right, Birdman, `eas_Init` |

Polca is neither follower-excluded nor overworld; F65 does not add the caravan
prefix. `followersfunctions_1.asm` and `data/scripting/entity/followers.asm` use
flags, not membership/active-party/HP/AI eligibility. F75 can supply Luke before
the fixed list, causing its Luke row to be skipped because his mapping is already
nonzero. That Luke retains the follower declaration, not the fixed `(18,15)` origin.
F74 contributes three non-ally rows: Astral under 31, king under 30, minister under 159;
they affect allocation and can share lookup identities with other supplied rows.
F74 does not supply Gerhalt. No follower table row supplies ally 9. With no follower
flags, Luke/Gerhalt are physical 1/2 and NPC128–132 are 3–7. F67-clear construction
has no fixed Peter row to repair his absence. Do not apply the example's physical
indices to another supplied follower combination.

`GetEntityAddressFromCharacter` (`mapscriptengine_1.asm:1841–1860`) masks the byte,
subtracts `$60` for NPC128+, reads the mapping and scales its physical index by 32.
Untouched zero mappings resolve physical 0, not a safe skip; `$FF` is likewise not
validated as an actor by this routine. `setPos`/`setFacing` skip allies below 30 only
when their **word** HP is zero; HP 256 is nonzero. NPCs bypass that guard. Action,
camera and text requests do not thereby acquire an HP or presence guard. Dead-ally
sprite selection can use a blue flame; that proves neither revival nor membership.

F513 overrides F799 with a regular town population: 128/129 are centaurs, 130 is
the kappa, 131/132 merchants, and no fixed Luke or Gerhalt row exists. With F799/F513 clear,
the default and F710 populations also differ from the F799 cast. Since the zone checks no F513 or
actor availability, supplied flags can admit the scene against a mismatched cast.
Names in dialogue do not validate physical mappings, combatant identity or a route.

Common Map13 init at `0x582E0` (retained body 18) independently tests F711, F716 and
F718 in that order. F711-clear calls `cs_58310` to face 130 Down, wait for its inline
motion-off/Right-orientation/sprite-update/idle action, and reset its animation
counter. In the F799 population that lookup is a **minotaur**, not a turtle.
F716-set asks lookup 137 to face Down; F718-set moves 140 to `$7000,$7000` without
deleting it. Neither 137 nor 140 has a fixed row in the F799 list; a zero mapping can
therefore affect main. The shrine owner's F711 write is a possible prior history,
not an additional predicate inferred from F799. Init effects and services precede
the later zone and remain supplied-state qualifications.

## X24 caller and scene admission

**Confirmed:** `s3_zoneevents.asm` lists X24/wildcard Y first, then wildcard X/Y11,
then wildcard X/Y30, then default. At `(24,11)` or `(24,30)`, the X24 row wins; a
handler return does not fall through. Ordinary movement produces a zone request
only with `$1400`. Entry at `(30,6)` does not automatically execute X24.

`Map13_ZoneEvent0` at `0x5823A` is retained zone program 9, with 18 operations:

| Supplied flags | Selected result |
| --- | --- |
| F710 clear | Accepted first-Oddler branch; its F711-dependent postprocessing then F710 write and return. No fallthrough to this confrontation in the same call. |
| F710 set, F799 clear | Return. |
| F710/F799 set, F715 set | Return. |
| F710/F799 set, F715 clear | Execute `cs_585FE`, then set F715 and return. |

The return-specific branch tests no F723/F724, F413/F513, F9/F41, follower flag,
HP, AI or actor availability. F715 is written after script return, not at scene
entry. F715-set/F413-clear does not repair itself by repeating the zone.

## Complete confrontation records and texts

**Confirmed:** `cs_585FE` at `0x585FE` has **100 records: 87 commands, 12 sleeps and
one terminator**. Cursor 1468 yields 19 texts, 16 `nextSingleText` and three `nextText`,
IDs 1468–1486, keys `05BC..05CE` in `data/scripting/text/gamescript.txt`. The separate
join service requests 446 (`01BE`) without advancing this scene cursor. Dialogue
comments and all 19 text keys agree; the table paraphrases their attributed claims.
Wait means the named action's idle-pointer wait, not a measured visible outcome.

| Records | Ordered requests and inline bodies |
| --- | --- |
| 1–9 | Sound fade-out command; sequential waited custom bodies for 129,130,131,132, each setting speed `(48,48)` then jumping to idle; waited auto-facing disable for 131 then 132; cursor 1468; waited Peter `eas_Init`. |
| 10–20 | 128 insults Bedoe soldiers (1468); camera `(14,10)`; enemy-attack music; Luke responds (1469), waits Right1; start 129 Left3 and wait 130 Left3; start 129 then 130 Right3; start 131 Left2 and wait 132 Left2. |
| 21–31 | Start 131 `eas_Die`, wait 132 `eas_Die`; face 129/130/Luke Left; sleep 40; Luke laments his comrades (1470); sleep 20; wait 128 Left1; face Luke Right;128 calls him captive and threatens him (1471). |
| 32–42 | Sleep 5, start 128 Up/Down oscillation, sleep 20; face 128 Left; `nextText` 1472 warns against resistance; face 128 Left again; single 1473 demands the injured boy; start 129 Left1 and wait 130 Left1; sleep 20; Gerhalt objects (1474). |
| 43–55 | Sleep 5/start 128 Up/Down; sleep 5/start 129 Right/Left; sleep 5/start 130 Right/Left; face Luke Up; sleep 40; camera `(6,6)`; Gerhalt offers to fight and guard Oddler (1475); track Gerhalt; set Bowie `(20,9)`, Peter `(19,8)`, both Down. |
| 56–68 | Wait Gerhalt Down3/Right6; wait `eas_BumpRight`; sleep 20; wait Right1; face Gerhalt Up; camera `(14,8)`; Gerhalt calls out (1476); face 128/129/130 Up; sleep 40; face Gerhalt Right; wait Luke Up1. |
| 69–79 | Luke recognizes leader/Peter (1477); wait Peter Down1; Peter asks why Luke is here (1478, portrait operand `$C0`); Luke's `nextText` 1479 says he came for a raft, single 1480 alleges the devils control Polca and deceived him; wait Gerhalt Right1, face Down; Gerhalt apologizes to Luke (1481); track Gerhalt; wait Up1; Gerhalt says Polcans will not fight and asks to join (1482). |
| 80–87 | `join Gerhalt`; track Luke; Luke approves (1483); face Gerhalt Down and nod him; wait Luke Up1; Luke asks not to be forgotten (1484); wait 129 Up1/Left1. |
| 88–95 | Wait Luke auto-facing off; waited custom speed `(48,48)`/idle; start Luke Left3; wait 129 Right1/Down1; replace Luke action with waited motion-off/Left-orientation/sprite-update/idle body; face 129 Left; sleep 40; wait 128 Left1/Up1. |
| 96–100 | 128's `nextText` 1485 challenges continued resistance; single 1486 threatens death; `setStoryFlag 13`; warp Map13 `(0,0)`, Right; end. |

Speakers are 128 six times, Luke seven, Gerhalt five and Peter once. Apart from
Peter's `$C0`, all text portrait operands are `$0`. Oddler is named in Gerhalt's
claim; no Oddler lookup, placement or joining command occurs. Luke's raft account
does not grant a raft or prove previous events. Luke's plea is not recruitment:
there is no `join Luke`, F10 or F42 write in this scene/caller. The 12 sleeps total
**260 requested frames**, excluding movement, text, camera and join waits; debug
text/sleep skipping and supplied services prevent a wall-clock claim.

## Action, death presentation and interruption boundaries

**Confirmed:** `mapscriptengine_1.asm:csc14_setEntityActscriptManual` installs an
inline pointer, `csc15_setEntityActscript` a named pointer, and
`csc2D_entityActionSequence` builds dynamic action storage. Their wait forms poll
for the selected entity's pointer to equal `eas_Idle`; nonwaited forms permit other
entities to proceed concurrently. None uses the position/facing HP guard. A wait
on 130 does not prove 129 finished, and a wait on 132 does not explicitly wait 131.
Pointer replacement is not a queue that preserves the replaced sequence's tail.

The speed bytes 48 are internal movement parameters, not tiles per frame.
`MAP_TILE_SIZE=384` is in 1/16-pixel coordinates, corresponding to 24 pixels;
`entityscriptengine_2.asm:esc04_moveToRelativeDest` scales tile operands, while
movement/update consumers handle the actual destination/velocity progression.
Layout, collision, VInts and dynamic state remain relevant. The four initial
speed bodies change speed, not position or collision policy. Peter's `eas_Init`
resets speed 32, acceleration/deceleration, obstruction/collision, auto-facing,
orientation and sprite update; it does not join, revive or declare a missing Peter.
Gerhalt's `eas_BumpRight` disables auto-facing, requests relative `(1,0)`, waits 5,
requests `(-1,0)`, waits 5, requests `(0,0)`, reenables auto-facing and jumps idle.
This service ordering does not independently prove the rendered bump.

`data/scripting/entity/eas_actions.asm:eas_Die` turns animation motion off, cycles
facing and sprite updates, requests `SFX_BATTLEFIELD_DEATH`, changes the sprite to
`MAPSPRITE_EFFECT1`, cycles effect poses, then sets position/destination to `(63,63)`
and jumps idle. `esc0C_setPosition` writes both coordinates and destinations and
clears travel. It does not remove the lookup mapping or call a combatant HP/death
consumer. Thus 131/132 receive a scripted death presentation and off-map placement;
this is not evidence of combat damage, kill processing, reward or battle victory.
The final wait observes 132's action pointer only. Rendering, audio and simultaneous
progress of 131 are Unknown at this composition.

Luke's record 90 Left3 request is nonwaited. The only intervening movement wait is
for 129 Right1/Down1; record 92 replaces Luke's action without a Luke destination
wait. Its body has no `ac_moveRel 0,0`, position reset or travel/velocity clearing.
`sf2cutscenemacros.asm:ac_motion OFF` emits opcode `$14`; the consumer
`esc14_setAnimationCounter` writes `-1` to the animation counter. Orientation writes
flagsB direction bits, distinct from facing. `ac_updateSprite` invokes the sprite
service, then the body jumps idle. The action installer itself changes pointer and
timer, not destination/velocity. `UpdateEntityData` continues to integrate position
toward the existing destination independently of that animation counter; `eas_Idle`
only waits/branches. Therefore the interruption neither proves a complete Left3
nor freezes Luke at a known coordinate. Auto-facing stays disabled and speed 48 is
retained until a later resetting service. There is no following `startEntity Luke`
here. Any eventual pose/motion and sprite-service completion remain Unknown.

`setCamDest` clears entity tracking, requests a destination and waits for scrolling.
`setCameraEntity` writes the mapped target without that wait. Here the last target
is Luke; this does not guarantee a valid/live Luke. Text services wait for scrolling
and call portrait/speech/window consumers; single-text requests also close windows
and sleep 10. Nod suspends animation, requests the gesture and sprite restoration with waits,
then resets the animation counter to zero. Sound requests use the
sound command path, without establishing audible playback. The scene terminator
conditionally waits for view scrolling when a dialogue window remains, then clears
scroll speed; it is not an all-actor completion barrier. Accepted controlled handler
evidence in [common scripting](../../world/common-scripting.md) retains its original scope;
no new natural scene observation is claimed.

## Gerhalt membership, capacity and cached lists

**Confirmed:** record 80 encodes `join ALLY_GERHALT` as opcode `$08`, word 9.
`mapscriptengine_2.asm:csc08_joinForce` at `0x47398` clears speech SFX, waits for
scrolling, reads/clears operand bit 15 and selects ordinary `MUSIC_JOIN`. Word 9
does not take the special 128 paired-join branch. Through `s02_jumpinterface.asm`,
`j_JoinForce` resolves to `code/common/stats/battleparty.asm:JoinForce`.
The service supplies class/name indices to text 446 (`01BE`), waits through
fade/input, closes text and sleeps 10. This is separate from cursor 1468–1486 and
is not suppressed merely because Gerhalt was already joined. No naming prompt is
encoded in this scene.

`JoinForce` sets **F9**, calls `UpdateForce`, compares its active count against
`FORCE_MAX_SIZE=12`, then calls `JoinBattleParty` only below 12. That helper sets
**F41=32+9** without rebuilding lists. `UpdateForce` enumerates joined allies into
force/active/reserve lists using their active flags; its snapshot precedes any new
F41 mutation.

| Supplied state at joining | Result |
| --- | --- |
| F41 clear, active count below 12 after F9 is set | F9/F41 set; cached active/reserve lists still reflect F41-clear until a later `UpdateForce`. |
| F41 clear, active count at least 12 | F9 set, F41 stays clear; Gerhalt remains reserve in that snapshot. No replacement member is chosen. |
| F41 already set | It remains set even if full/overfull; the rebuild includes Gerhalt after F9 is set. No normalization or eviction. |

These helpers do not test HP, heal/revive, create a map actor, change follower flags
or write force AI. Joining Gerhalt does not convert his fixed field actor into a
follower. There is no `joinForceAI`, `followEntity` or `addFollower` command in
`cs_585FE`; F67/F74/F75 and the supplied activation word are not changed by joining.
The subsequent battle initialization refreshes lists and tests life/placement
separately. Gerhalt's scene presence and F9/F41 alone prove neither a deployed unit
nor a player-controlled turn. Peter and Luke retain their own independent
membership/active/HP/AI conditions.

## Pending warp, caller return and Battle13 setup

**Confirmed:** record 98 `csc13_setStoryFlag` adds
`BATTLE_UNLOCKED_FLAGS_START=400` to 13, setting **F413**, not literal F13.
Record 99 `csc07_warp` writes pending type 1, parameter 1=0 and parameters 2–5=
Map13/X0/Y0/Right. It does not switch map or start battle immediately and does not
write `WARP_SFX`; the eventual consumer uses the carried value. The script then
terminates. Only after `ExecuteMapScript` returns does the caller set **F715**.

`RunMapSetupZoneEvent` closes portrait/text, waits a VInt and waits for physical 0
to stop. On return, pending type 1 makes `WaitForEvent` skip its normal camera 0 and
controlled-action restoration. `ProcessMapEvent` clears pending type and consumes
the no-scroll tuple through its services, returning to `MainLoop` for `SwitchMap`
and `CheckBattle`. Thus the order is **F413 → pending warp → script return → F715
→ wrapper return → pending consumption → battle selection**, conditional on each
service and the still-intact request. A stall can leave earlier writes committed
without later progress; there is no atomic transaction or immutable snapshot.
F715-set/F413-clear is not a replay-repair contract.

The intact Map13 `(0,0)` tuple qualifies for row 13, `BATTLE_POLCA_VILLAGE`, on F413.
F513-set still clears F413 while selecting that same call. Selection enters
`BattleLoop`, **not** the ordinary Map13 population/init path again. Scene lookups,
their final destinations, hidden birdmen and Luke's interrupted action do not
specify the battle population.

`code/gameflow/battle/battleloop_1.asm:BattleLoop` clears `PLAYER_TYPE`, sets F399
and tests F88. F88-set restores saved seconds, clears F88/AI memory, loads the
saved battle and branches straight to individual turns; it bypasses new placement
and both wrappers. That supplied-state branch is outside this new-battle endpoint.
F88-clear zeroes seconds, stores current map/battle and installs base VInts before
the before wrapper. Both actual row 13 targets in
`data/battles/cutscenes/beforebattlecutscenes.asm` and `battlestartcutscenes.asm`
are **`ms_Empty`**, not `cs_585FE` or a Battle13-specific intro.

The two wrappers use **F463=450+13**. Before checks without setting; start checks
and sets it before dispatch. F463-set skips both scripts, not the initialization
between them. F463-clear dispatches the empty interpreter program, whose terminator
still has its conditional scroll wait and scroll-speed reset. Empty scripts do
not imply an empty service flow.

After before-return, the new-battle path restores current map/battle, clears region
flags 90–105, heals living/immortal allies, initializes ally then enemy positions,
clears AI memory, loads battle for combatant 0 and calls the start wrapper.
`battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions` first
resets ally X positions, calls `UpdateForce`, then consumes the actual active list.
Zero **word** HP skips placement; list advancement and placement-pointer advancement
differ, and a remaining-tail branch can call `LeaveBattleParty`. This refresh is
where a newly set F41 becomes reflected in those cached lists; it does not promise
Gerhalt a particular ordinal or coordinate.

Direct `data/battles/spritesets/spriteset13.asm` at `0x1B3E86` declares 12 ally
placements, 16 enemies, three AI regions and one AI point. Ally rows 0–11 contain
`(22,9),(23,9),(24,8),(23,8),(22,8),(21,9),(21,8),(23,7),(22,7),(20,8),(20,9),(24,7)`.
These rows are placement capacity, not named fixed allies or a guarantee of 12
living deployed members. Enemy initialization consumes Battle13's actual entries
and initialization types, not the five exploration NPCs. The general spriteset
fixture explicitly does **not** track row content; these selected counts/coordinates
are source evidence, not a claim of per-row fixture coverage. AI declarations and
memory clearing are not AI execution or a guarantee of player control.

`battlefunctions/loadBattle.asm:LoadBattle` fades out, loads map tilesets and waits,
clears VInts, calls `PositionBattleEntities` to clear/redeclare actual battle
entities and mappings, selects the combatant view target, initializes sprites,
loads map/mapsprites, restores base VInts, loads terrain, plays map music and
requests fade-in. The entity builder uses positioned combatants and skips invalid
coordinates; enemy neutral state is also tested. It does not preserve field
actions/followers or the F799 exploration cast. The start wrapper then returns.
**Stop immediately before `BattleLoop:@Start` calls `ActivateEnemies`.** No enemy
activation, first turn, combat, outcome, later raft scene or usable-input acceptance
is asserted.

## Reproduction and evidence limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands;
select the retained read-only upstream and verify the exact pin/clean status.
Run the existing [retained payload and cutscene comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Select `entryLabel=cs_585FE` from `programCorpus.programs`; compare all 100 records'
`sourceLine`, macro and arguments against lines 186–351, inspecting the 27 inline
bodies separately. Select text keys `05BC..05CE` and `01BE`, without exporting a
transcript. Inspect all 18 operations of `map-events/zone-programs.json` program 9,
retained init body 18/body 65 and the separately delimited Map52 helper.

| Relation | Existing owners and direct selected checks | Boundary |
| --- | --- | --- |
| Travel/admission | `map-data-static-v1.json`, `battle-global-data-static-v1.json`; exact Map10 row 34/Map52 row 1, battle rows 12/13, `SwitchMap`/`CheckBattle` and request consumers. | Attribute gate, row order, wildcard versus output area, mutable pending and completion clear-on-selected-call. |
| Setup/actors/init | `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json`; actual Map13 pointer/population rows, follower table and initialization/lookup consumers. | F513 priority, F75 fixed-row skip, F74 allocation, absent Peter/mappings, life/member/AI distinction and init effects on alternate actors. |
| Caller/scene/services | `map-events/zone-programs.json`, retained map-script-engine corpus and existing `map-script-engine-static-v1.json`; all records/inline bodies/text IDs and named handlers. | F710/F799/F715 order, action concurrency, scripted death versus combat, interrupted Luke animation versus position, separate join text and service completion. |
| Membership | `csc08_joinForce`, jump-interface alias, `battleparty.asm` and later `InitializeAllAlliesBattlePositions`. | F9/F41, capacity, pre-active-write cached lists, no follower/AI write or guaranteed placement. |
| Start handoff | `battle-routing-data-static-v1.json`, `battle-cutscenes-static-v1.json`, `battle-spriteset-data-static-v1.json`; independently read exact row 13 targets, spriteset13 and shared wrappers/loop/load. | Empty-script service path, F463/F88 alternatives, actual party and reinitialization, stop before activation. |

Fixture paths above are relative to `tests/fixtures/h2/`. Existing aggregate facts
do not substitute for exact source rows or a composed runtime observation.

**Unknown:** natural Bedoe-to-Polca walking/collision, interruption-free arrival,
supplied flags/actors/mappings/HP/party/AI, concurrent action progress and exact
positions, text/camera/sprite/fading/audio/pending completion, rendered birdmen
deaths or Luke's pose, responsive control and continuous Battle13 playability.
Static source and accepted shared consumers establish only the named boundaries.
These gaps do not block the declared static outcome or become an acquisition queue.

Record direct checks/failures, the clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI in
the Issue/PR handoff. Preserve predecessor comparisons/failures recorded in
[PR798's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/798#issuecomment-6158652959),
without rerunning its old scope as a ritual. No local normal/full/engine/H3/H4/Godot
suite solely for prose. Leave a clean pushed Draft PR, Project Review / Verify,
frozen writer and successful authorized completion callback. Main-gate owns
independent review/merge; no cleanup or next-Issue selection is authorized here.
