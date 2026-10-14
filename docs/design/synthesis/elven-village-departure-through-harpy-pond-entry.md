# Elven Village Departure Through Harpy Pond Entry

Leaving Elven Village can return the party to Southwest Parmecia. A separately selected west-border
scroll carries the current party into Devil's Tail, where Oddler can interrupt movement because he
hears somebody calling for help. That scene requests Harpy Pond; a separate entrance also requests
the pond. If ordinary entry does not admit Battle20 first, the pond's initial scene identifies an
elven boy in trouble and proposes helping him.

**Confirmed:** these are selected source programs and conditional request/population consumers.
**Inferred:** the two scenes introduce a rescue. **Unknown:** a natural village-to-pond itinerary,
actual caller state, completed presentation and usable control. The dialogue introduces help; neither
scene rescues or recruits Elric, opens a passage or unlocks a battle.

This guide consumes accepted [departure and entry research](../../research/elven-village-departure-through-harpy-pond-entry.md)
and follows the optional [village conversations and local exploration](elven-village-conversations-and-local-exploration.md).
The selected exit/request programs impose no village-activity prerequisite. The scope stops at conditional pond control, another pending
or admitted continuation, or Battle20 admission **before its opening**. Pond approach/exit-warning,
battle content/outcomes, Elric rescue/recruitment, F751 passage, Fairy Cave/Creed and Battle21 content
remain outside it. Fnnn denotes flag nnn; rows and command records are zero-based.

## Selected Village Exits

**Confirmed:** Map23's ordered warp table begins with three exits. Each requests a no-scroll return
to Map68, Southwest Parmecia, at `(48,31)`, facing Down:

| Row | Matching village tile | Selection consequence |
| --- | --- | --- |
| 0 | Any X, Y 1 | North exit; wins at a corner also matching a later row. |
| 1 | X 0, any Y | West exit when row 0 did not match. |
| 2 | Any X, Y 30 | South exit when earlier rows did not match. |

The internal stairs retain their local owner. The east exit to Map24 is excluded. WarpIfSetAtPoint
scans in source order after dividing supplied internal coordinates by 384; negative coordinate bytes
are wildcards. It writes event 1, parameters 1–6 and WARP_SFX. Movement/layout admission remains
separate: the table does not prove that walking or a vehicle can naturally reach an exit.

The ordinary no-scroll consumer clears pending before dispatch, submits and clears the current
WARP_SFX, idles physical actor 0 and unwinds exploration. It samples the current tuple, replaces
nonnegative requested coordinates and returns `(68,48,31,Down,d4=0)`. It preserves PLAYER_TYPE.
MainLoop runs SwitchMap and CheckBattle before fresh exploration; no flag-switched replacement
applies to Maps68, 67 or 51.

### Battle Admission Precedes Fresh Population

The selected battle rows distinguish admission flags from completion flags:

| Map / sole battle row | Supplied unlock | Completion | Trigger | Area origin and size |
| --- | --- | --- | --- | --- |
| 68 / 19 | F419 | F519 | Any X/Y | `(5,19)`, 48x32 |
| 67 / 21 | F421 | F521 | Any X/Y | `(21,0)`, 32x42 |
| 51 / 20 | F420 | F520 | Any X/Y | `(0,0)`, 18x26 |

CheckBattle uses the unlock and trigger bytes; the area rectangle is an output, not a containment
test. After selecting a completed row, it clears that row's unlock without canceling the current
selection. Thus supplied F419 admits Battle19 on village departure even if F519 is set. Stop on that
unrelated admission. The selected exits produce none of these flags.

With F419 clear, fresh Map68 exploration clears pending, subtracts 20000 from STEP_COUNTER with a
signed-word negative-result clamp to zero, heals living/immortal allies, fades, loads tilesets and
writes CURRENT_MAP=68/CURRENT_BATTLE=255. It rebuilds population, clears temporary flags 256–383,
sets F80, loads the map and invokes its return-only init. Map68 has no fixed actor rows.

Population allocates flag-selected followers from physical 1 and main actor 0 separately; main 0
does not enter the fresh EXPLORATION_ENTITIES follower list. On overworld Map68, F65 adds Caravan
and binds FOLLOWER_A before the normal table. With only F65/F67/F68 selected, Caravan, Peter and
Oddler/FOLLOWER_B occupy physical 1/2/3. This tests flags, not HP or force membership. Later
F74/F77/F83 rows can overwrite A's lookup; F70 can overwrite B's. F65 clear neither supplies Caravan
nor repairs a supplied mode 1.

Fresh mode 2 gives main 0 the raft sprite, parks the current follower list at `(64,64)` with standing
actions and declares separate raft 31 there. Other modes declare raft 31 only for F64 and a matching
saved RAFT_MAP. Mode, saved raft coordinates and membership remain supplied; mode 1 does not replay
boarding. Pending zero may permit its controller, without proving responsive input or westward travel.

## An Independent West Scroll Retains the Current Party

**Confirmed:** Map68 warp row 4 matches X 0/any Y and requests a Left scroll into Map67, Devil's Tail,
with destination X 62, Y 255 and facing operand Left. Earlier row 3 matches any X/Y 0 and leads north
to Map73, so it wins at `(0,0)`. Rows 0–2 are earlier interior entrances. The west row is selected
only when none of those rows matches.

This consumer stays inside the existing ExplorationLoop. It idles main 0, writes CURRENT_MAP=67 and
calls ProcessMapTransition to load tilesets/blocks/layout and perform the plane/sprite scroll using
the low direction bits. Its special Map71/F530 substitution is not selected here. After transition,
main 0's current and destination X become `62*384`; negative Y 255 preserves Y. The consumer does
not apply the warp row's facing byte, so Left is not an unconditional resulting facing.

Scrolling does not unwind to MainLoop or run SwitchMap/CheckBattle. It does not heal, reduce the step
counter, replace CURRENT_BATTLE, rebuild population, clear temporary flags or run Map67 init. Actors,
logical mappings, follower list and mode survive. It also bypasses ordinary WARP_SFX submission/clear.

DeclareRaftEntity then uses the sampled main tuple. Mode 2 returns immediately; mode 1 skips current
follower relocation; other modes move every existing follower-list entry to the sampled coordinates
without rebuilding that list. For non-2 modes, F64 clear returns. A saved raft map matching 67 declares
raft 31 at masked saved coordinates; mismatch clears lookup 63 and parks raft 31 at raw `$70007000`.
These calls write neither saved RAFT_MAP/X/Y nor PLAYER_TYPE. A current-only follower survives even
without a fresh setup row, and a newly supplied follower flag does not create an actor during scroll.

Supplied F421 therefore causes no immediate Battle21 scan. A separate ordinary return/reload to 67
would scan row 21 and could admit it, even with F521 set; stop there. F419 belongs to Map68 and cannot
stand in for F421. Fresh Map67, if independently selected, has an empty fixed list and return-only
init; those facts do not turn this scroll into fresh entry or establish later battle content.

## Oddler's Call-for-Help Scene

**Confirmed:** walking's `$1400` layout branch writes event 6 with word X/Y from destination plus
candidate movement divided by 384. Its `$1000` warp branch is separate; the Caravan controller has
no corresponding zone producer. The zone consumer initializes physical 0 and submits those words
to RunMapSetupZoneEvent. That wrapper resolves the current setup and scans ordered rows with 255
wildcards and `$FD` default. After the callback it closes portrait/text, waits one VInt and waits for
main 0's current position to equal its destination. Natural layout/producer selection remains Unknown.

| Map67 zone selection | Callback and bounded result |
| --- | --- |
| Row 0, `(57,25)` | Map67_ZoneEvent0 belongs to the separate F751/F13/Elric-HP passage path; excluded. |
| Rows 1–4, `(56,26)`, `(57,26)`, `(58,24)`, `(58,25)` | Map67_ZoneEvent1 returns with F520 set; with F520 clear it executes cs_4FB64. It tests neither F470 nor F420. |
| Default, other coordinates | Map67_DefaultZoneEvent supplies BATTLE_DEVIL_TAIL (21) to CheckRandomBattle. |

The default's shared step/random/completion mechanism may write a reload request. It is not an
immediate battle scan on every scroll or callback. Its battle content and the passage continuation
are excluded; stop when an unrelated request takes ownership.

### The Complete Scene, in Order

`cs_4FB64` (`0x4FB64`) has 39 top-level records. B below means FOLLOWER_B (31), A means FOLLOWER_A
(30). The script uses current lookups rather than searching joined members or verifying its cast.
In the unchanged fresh-Map68 F65/F67/F68 example, B/A/Peter/Bowie resolve to physical 3/1/2/0.
Changing flags after population does not replace those lookups. Oddler/Peter dialogue names do not
repair absent or conflicting actors.

| Records | Ordered action and dialogue |
| --- | --- |
| 0–5 | Set cursor 2543; wait for B Init, A Init, Peter Init and B Jump; B single text 2543 `$09EF`, modifier 0, says “Wait!” `{W1}`. |
| 6–10 | Move B to `(60,27)` and wait; csWait 5; install B 2xRightLeft without waiting for idle; csWait 120; move camera to `(53,24)` and wait for scrolling. |
| 11–18 | Move Peter to `(59,27)` and wait, face Right; Peter single 2544 `$09F0`, modifier `$C0`, asks what is wrong with Oddler `{W1}`. Move Bowie to `(60,26)` and wait, face Down; B faces Up and single 2545 `$09F1`, modifier 0, addresses `{LEADER}...{NAME;7}` and asks whether they can hear `{N}`/`{W1}`. Peter single 2546 `$09F2`, modifier `$C0`, asks what `{W1}`. |
| 19–24 | csWait 5; install Peter 2xRightLeft; csWait 120; Peter faces Down and single 2547 `$09F3`, modifier `$C0`, says he hears nothing `{W1}`. B inline moveDown 1, endActions, waits for idle. |
| 25–29 | csWait 5; install B 2xRightLeft; csWait 120; B nextText 2548 `$09F4`, modifier 0, says somebody cries for help `{W2}`; B single 2549 `$09F5`, modifier 0, locates it “over here” `{W1}`. |
| 30–38 | Move B to `(57,25)` and wait, place at `(63,63)` Down; csWait 40; move Peter to `(57,25)` and wait, place at `(63,63)` Down; move Bowie to `(57,25)` and wait, place at `(63,63)` Down; request Map51 `(11,22)` Up; end. |

Dialogue descriptions paraphrase accepted text annotations; quoted fragments, IDs, substitutions and
controls retain their literal identity. Coordinates and wait values are source operands, not observed
positions, elapsed durations or rendered scenes. Placement at `(63,63)` retains mappings/list entries;
it is not HideEntity. The scene sets/clears no F520/F470/F420/F751/F13/follower flag or PLAYER_TYPE,
creates no Elric and joins nobody. It has no local completion flag preventing another F520-clear call.

### What the Actions and Waits Actually Consume

Init installs speed 32/32, zero acceleration factors, disabled acceleration/deceleration, enabled
obstruction/collision/autofacing, Up orientation, disabled immersion and size 24; it refreshes and
jumps idle. It does not allocate a fresh actor. Jump disables autofacing, moves relatively `(0,-1)`,
waits 5, moves `(0,1)`, waits 5, moves `(0,0)`, restores autofacing and jumps idle. 2xRightLeft refreshes
Right/Left/Right/Left, waiting 30 after each, then jumps idle. A separate csWait 120 is a sleep request,
not proof that the action completed.

`csc15` installs action/timer through the mapped actor, optionally waiting for the exact eas_Idle
pointer, with no HP/presence gate. `csc29` setDest similarly has no HP gate; it writes tile-scaled
destinations, absolute travel and signed velocity 32. The selected operands lack the high-bit no-wait
modifier, so their waits poll current/destination equality through VInts. This bypasses walking's
terrain/collision producer.

`csc19` setPos and `csc23` facing use AdjustScriptPointerByCharacterAliveStatus: ally IDs 0–29 with
HP 0 skip those commands, whereas A/B IDs 30/31 bypass that HP test. Placement updates current and
destination coordinates/facing and refreshes; it does not delete an action, list entry or membership.
Peter/Bowie HP 0 can therefore skip placement/facing while their action, destination and text records
still run.

`csc2D` compiles the selected inline move into relative-destination opcode 4, wait-destination opcode
1 and a final idle jump. entityActionsWait clears FLAGS_A bits 5/6 and waits for idle, without an HP
gate. Opcode 4 adds signed tile displacement to current position; clearing bit 5 disables its
destination-conflict check. It bypasses walking layout checks and uses current action speeds.
Actual completion still needs entity processing; the caller validates no presence.

GetEntityAddressFromCharacter masks the logical ID and indexes the current lookup without a membership
or presence check. ClearEntities leaves unassigned entries 0, so absent A/B/Peter/Elric may alias
main 0 rather than skip safely. Lookup 255 is likewise not a safe absence test here. Earlier writes
can survive unfinished waits/services: no transaction or automatic cast repair is provided. These
are conditional source limits, not observed failure scenarios or proof that dead actors reach this scene.

`csc32` disables entity camera following, sets the camera tile destination and waits for scrolling.
`csc24` restores a mapped camera entity without HP validation or its own scroll wait. csWait sleeps
with its low byte; debug P2 Start can set SKIP_CUTSCENE_TEXT and bypass these waits/text. Script entry
clears skip state; end conditionally waits for view scrolling when dialogue state requires it and
clears camera scroll speed.

The cursor selects the next text ID; nextText and single forms consume/increment it. Single also
closes portrait/text and sleeps 10; nextText leaves that closure for continuation. `$C0` combines
the mirrored/right-side portrait modifier bits. Portrait/speech lookup uses the mapped mapsprite.
`{N}`, `{W1}`, `{W2}` and substitutions remain literal source controls, without observed cadence or
service completion. Neither selected scene contains YesNoPrompt or a choice result.

## Two Pond Requests, With Admission Before Init

**Confirmed:** the scene's `csc07_warp` writes pending 1, no-scroll 0, Map51, X 11, Y 22 and Up.
It does not move immediately, end the script or write WARP_SFX. After script and zone-wrapper return,
ordinary consumption uses the request and supplied sound state; another pending write can replace it.

Separately, Map67 warp row 3 at `(57,25)` requests Map51 `(10,24)` Up through WarpIfSetAtPoint,
including WARP_SFX. Earlier rows `(26,22)`, `(51,22)`, `(57,23)` and the later X63 border do not
compete there. The direct warp and excluded ZoneEvent0 share a coordinate but use distinct layout/
producer selection; neither implies the other, a passage opening or a call-for-help prerequisite.

Both ordinary returns reach MainLoop admission. Supplied F420 selects Battle20 at either coordinate
before pond population/init. There is no F470/F13/HP/membership prerequisite. F520 set clears F420
after selection but still admits 20. Stop before BattleLoop/opening; the exploration prompt has not
run. These scenes/exits do not set F420, and this guide does not invoke later approach content to obtain it.

## F520 Selects Population; F470 Independently Selects the Prompt

**Confirmed:** with no admitted battle, fresh Map51 exploration applies the same healing, counter,
temporary-flag, F80 and map-loading sequence, with CURRENT_MAP=51/CURRENT_BATTLE=255. Default ms_map51
(`0x5C27C`) and the sole F520 alternative ms_map51_flag520 (`0x5C294`) differ only in entity pointer.
Their actual six-slot tables share entity-event, zone, description, item and ms_map51_InitFunction
pointers. An alternate filename does not establish a separate init; pointer binding executes no zones.

| Default fixed row / identity | Initial tile and facing | Mapsprite / action |
| --- | --- | --- |
| 0 / Elric, ally 13 | `(6,6)`, Down | GetAllyMapsprite(13); eas_WhirlsInWater |
| 1 / NPC128 | `(63,63)`, Up | HARPY 108; eas_Init |
| 2 / NPC129 | `(63,63)`, Up | SOULSOWER 75; eas_Init |
| 3 / NPC130 | `(63,63)`, Up | SOULSOWER 75; eas_Init |

Default s1_entities (`0x5C2AC`) has these four rows; alternate s1_entities_520 (`0x5C2CE`) immediately
terminates. Elric uses ally lookup 13, not NPC128. Population skips an ally row whose lookup is already
nonzero instead of duplicating it. The normal follower table has no Elric row; with only F67/F68,
Peter/B occupy physical 1/2, default Elric 3 and NPCs 4–6, while the alternate has none of those four
fixed actors. Main 0 is separate. Fixed actors are not appended to the fresh follower list, and
placing Elric sets neither joined F13 nor active membership.

Map51 uses the normal follower table, even with F65 set; it is neither overworld nor an excluded-
follower map, so F65 alone adds no Caravan. GetAllyMapsprite consumes class/HP; HP 0 outside battle
selects blue flame rather than using membership as a general gate. WhirlsInWater enables immersion
and doubled animation speed, cycles Right/Up/Left/Down with refresh/wait 10 after each and loops.
This is an initial action, not an observed drowning/rescue animation.

Mode 2 keeps the fresh raft/parked-follower tail; mode 1 is not normalized or reboarded. Clearing
temporary 256–383 preserves supplied F420/F470/F520, F13 and follower flags. All four combinations
below reach init only when F420 did not already admit battle:

| F520 | F470 | Population and init consequence |
| --- | --- | --- |
| Clear | Clear | Default fixed rows; shared initial prompt requested. |
| Clear | Set | Default fixed rows; prompt skipped. |
| Set | Clear | Empty fixed list; the same prompt still requested, without fixed Elric repair. |
| Set | Set | Empty fixed list; prompt skipped. |

### The Complete Initial Prompt

ms_map51_InitFunction (`0x5C3CA`) returns with F470 set; clear runs cs_5C3D8 (`0x5C3D8`) then
returns. It does not test F520/presence or set F470/F420. Skipping preserves the incoming `(10,24)`
or `(11,22)` main tuple, subject to shared setup. The selected scene contains 24 top-level records:

| Records | Ordered action and dialogue |
| --- | --- |
| 0–6 | Cursor 2550; wait for B Init and Peter Init; place Bowie `(11,22)`, B `(10,22)`, Peter `(9,22)`, all Up; fadeInB calls FadeInFromBlack. |
| 7–12 | B inline moveUp 2/endActions, wait for idle; csWait 5; install B 2xRightLeft; csWait 120; B faces Up; B single 2550 `$09F6`, modifier 0, identifies the direction `{N}`/`{W1}`. |
| 13–17 | Peter inline moveUp 2/endActions, wait for idle; Peter single 2551 `$09F7`, modifier 0, points “over there” `{W1}`; camera destination `(2,2)` with scroll wait; csWait 50; Elric single 2552 `$09F8`, modifier `$C0`, calls “Help me!” `{W1}`. |
| 18–23 | Peter nextText 2553 `$09F9`, modifier 0, identifies an elven boy stuck in the pond `{N}`/`{W2}`; Peter single 2554 `$09FA`, modifier 0, says he is tired and proposes pulling him out `{N}`/`{W1}`; camera follows Bowie; Peter follows Bowie, relative 2; B follows Peter, relative 2; end. |

With consistent live mappings/HP and completed actions, placement/inline operands put Bowie at
`(11,22)`, B at `(10,20)` and Peter at `(9,20)` before the new following runs. This is a conditional
derivation, not a fixed control snapshot: following, skipped placements and partial execution can
change it. F520-set/F470-clear still calls the same script with no fixed Elric; his text lookup is
neither suppressed nor repaired.

No prompt command rescues/joins Elric, moves his fixed row out of the pond, reveals the parked NPCs,
unlocks Battle20, sets F470/F520/F751, changes mode or writes saved raft position. With F470 still
clear, re-entry can request the prompt again. Calling for help is not completed rescue.

### Following Uses the Actual First-Byte HP Test

`csc2C_followEntity` tests the first byte of its word-encoded follower ID through the alive helper.
Both selected IDs 7 and 31 have high byte 0, so **Bowie's HP** gates both whole follow commands,
not Peter's or B's. Bowie HP 0 skips each. Otherwise the command maps follower/target, obtains
relative 2 from table_FollowerPositions as `(-24,0)` and calls AddFollower.

AddFollower installs a 42-byte eas_Follower1 copy with physical target/offset and appends the follower
to EXPLORATION_ENTITIES only if absent. It writes no membership or follower flag. The template uses
speed 32/32, zero acceleration, disabled acceleration/deceleration/obstructability, map/entity
uncollidable bits and a follow/wait-destination loop.

Actual esc03_follow still checks layout. At maximum current-axis separation at most `$1E0`, it keeps
the existing destination. Otherwise it derives a facing-relative offset from the target destination,
tries the zero-lateral fallback (the same offset for these commands), then the target destination
if layout remains blocked. Travel/velocity use current follower speeds. The map command does not
wait for this new action to settle. Relative 2 is not a guaranteed world-coordinate endpoint;
missing mappings may retarget main 0 without validation or story-state repair.

## Conditional Control Is the Stopping Boundary

After init, the shared loop conditionally submits music/fade. Pending zero lets WaitForEvent follow
physical 0 and install the supplied mode's controller:

| PLAYER_TYPE | Source action / opcode | Actual transport handling |
| --- | --- | --- |
| 0 | eas_ControlledCharacter / 2 | Walking |
| 1 | eas_Raft / 7 | Caravan |
| 2 | eas_Caravan / 8 | Raft |

Other non-0/non-1 values take the last branch without validity repair. The numeric consumer controls
interpretation despite the source labels. Neither scene changes this policy. A pending request
precedes controller installation; unfinished action/text/fade/camera services do not establish
usable control. Stop before the pond's approach/exit-warning callbacks or any battle opening.

## Evidence Trace and Review Boundary

**Confirmed provenance:** accepted research binds SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Its [reproduction owner](../../research/elven-village-departure-through-harpy-pond-entry.md#reproduction-and-remaining-limits)
retains the exact selected source paths, addresses, programs and immediate consumer joins. Map table
paths begin at `data/maps/entries/` in the pinned `disasm/`. This synthesis performs no new source
investigation, full-program comparison, H1/ROM acquisition or composed runtime observation.

| Explanation | Contract and executable evidence | Deliberate limit |
| --- | --- | --- |
| Ordered exits, scroll versus ordinary return, admission | [exploration flow](../contracts/exploration-control-flow.md), [entry routing](../contracts/map-entry-routing-state.md), [map exploration](../contracts/map-exploration.md); [request writes](../../../tests/fixtures/h2/map-event-request-state-static-v1.json) eventRequestState and [consumption](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json) eventRequestConsumption; [battle routing](../../../tests/fixtures/h2/battle-routing-data-static-v1.json) expected.facts | Shared helper facts/aggregates are not complete guards for the selected warp/battle rows. Accepted research owns complete-table and actual-consumer joins. |
| Map67 callback gate/default | [zone shard](../../../tests/fixtures/h2/map-events/zone-programs.json), expected.zoneTargetPrograms programOrder 122–124 | 123 binds F520/read/branch/script/return, 124 the shared random helper. 122 is excluded passage context; no activation is inferred. |
| Both complete scene programs and actions/text | [map-script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), expected.dialogueCommandFacts, transitionCommandFacts, scriptControlCommandFacts, entityPlacementCommandFacts, entityActionBridgeCommandFacts, entityGestureRelationshipMotionCommandFacts and mapCameraControlCommandFacts; [dialogue](../contracts/dialogue-system.md) | Public summaries do not contain a complete selected-program proof. Accepted retained programCorpus.programs IDs cs_4FB64/cs_5C3D8 bind all 39/24 records and three inline moves; research owns the literal text annotations and external actions. |
| F520 population, shared F470 init and current following | [setup](../contracts/map-setup-data.md), [entities](../contracts/map-entity-data.md), [map exploration](../contracts/map-exploration.md); [setup fixture](../../../tests/fixtures/h2/map-setup-static-v1.json), [entity fixture](../../../tests/fixtures/h2/map-entities-static-v1.json), [init fixture](../../../tests/fixtures/h2/map-init-static-v1.json) | Init primarySourceBodies 64/74, routeJoins 101/102/120 and pointerTableJoins 101/102/116 bind shared init/Map67 return. Actual four/empty rows, first-byte HP and asynchronous follow come from accepted selected research, not aggregate actor totals or story names. |

Schema/consumer owners are [request-write schema](../../../schemas/h2/map-event-request-state-static-fixture.schema.json)/[map_event_request_state.py](../../../src/sf2tool/h2/map_event_request_state.py),
[request-consumption schema](../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../src/sf2tool/h2/map_event_request_consumption.py),
[battle-routing schema](../../../schemas/h2-battle-routing-data-static-fixture.schema.json)/[battle_routing_data.py](../../../src/sf2tool/h2/battle_routing_data.py),
[zone schema](../../../schemas/h2/map-events-zone-programs.schema.json)/[map_events_fixture.py](../../../src/sf2tool/h2/map_events_fixture.py) and [map_events.py](../../../src/sf2tool/h2/map_events.py),
[engine schema](../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../src/sf2tool/h2/map_script_engine.py),
[setup schema](../../../schemas/h2-map-setup-static-fixture.schema.json)/[map_setup.py](../../../src/sf2tool/h2/map_setup.py),
[entity schema](../../../schemas/h2-map-entities-static-fixture.schema.json)/[map_entities.py](../../../src/sf2tool/h2/map_entities.py),
and [init schema](../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../src/sf2tool/h2/map_init.py).
Existing reproduction uses `uv run sf2 h2` with `map-event-request-state`, `map-event-request-consumption`,
`battle-routing-data`, `map-events`, `map-script-engine`, `map-setup`, `map-entities` and `map-init`,
loading the ignored worktree configuration in the same process. These routes locate evidence;
they select no prose-only regeneration, runtime or blanket suite.

Review counterexamples include corner precedence, F419 on exit, F421 during scroll versus ordinary
return, F420 before init, all four F520/F470 combinations, absent/conflicting mappings, HP 0,
mode 0/1/2, saved-raft mismatch, debug text skip and replaced pending requests. They constrain static
claims; they are not executed scenarios. The narrower shared contracts retain their runtime/service
Unknowns; accepted content-specific research supplies selected joins without expanding those contracts.

Preserve [PR866 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/864#issuecomment-6170376311),
[updated handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/864#issuecomment-6170461179)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/866#issuecomment-6170476818),
including the unchanged retained pair, original/rebased CI, complete joins and corrected inspections.
Preserve [PR865 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/863#issuecomment-6170364766)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/865#issuecomment-6170393336),
with their linked predecessor evidence/failures. No completed result, golden, fixture or private
payload is replaced or replayed to refresh this prose.

**Unknown:** natural access/terrain/route, supplied flags/HP/mappings/membership/mode/raft, producer
selection, completed actions/text/fade/camera/services, presentation, responsive control and
persistence. These remain deferred under [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../decisions/0016-remake-start-evidence-deferral.md), not an automatic runtime queue.
This segment establishes neither a composed runtime route nor campaign/design-epic completion.
