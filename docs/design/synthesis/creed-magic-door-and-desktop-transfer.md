# Creed's Magic Door and the Desktop Transfer

**Confirmed:** a separate Creed Mansion zone event stages a warning, flashes, shrinking actions,
narrated pickup and collection, then requests Desktop Kingdom. **Inferred:** the scene depicts the
party becoming Goliath's collection. The dialogue explains the magic door, but the selected code
does not grant collection membership or persistent gameplay scale. **Unknown:** natural reach,
actual supplied actors/state, completed services, rendered pickup and usable destination control.

This English guide follows [the first Goliath obstruction](creed-mansion-entry-and-first-goliath-scene.md)
and consumes [accepted magic-door/transfer research](../../research/creed-magic-door-and-desktop-transfer.md).
Its Standard agreement is [Issue #899](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/899)
and the [documentation roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
It stops at fresh Map22 exploration entry **before population/init**, or selected Battle22 handoff
**before opening**. Desktop arrival, restoration, later mansion content and Chessboard gameplay are
separate work. No new playthrough, source reinvestigation or remake decision follows from this guide.

## A Separate Door Event with Supplied Actors

**Confirmed:** Map33 zone `(12,14)` selects Zone1 when the current setup supplies that table.
Fnnn means flag nnn. Zone1 checks F783: set returns; clear runs the complete `cs_5AF36` then returns.
It neither checks F774 nor writes F783 or another completion flag. F774 belongs to the earlier
obstruction; its completion is no local prerequisite for this scene. The prior scene does not fall
through into shrinking, and F774 clear/set need not supply equal positions or history.

The baseline setup supplies the zone table. Current setup selection still scans ordered overrides
F523, F783, F784, F786, F22 with last-set precedence. A coordinate or baseline pointer does not prove
natural access or that another selected setup supplies this callback. The zone caller first clears
the old event type, applies the init action to physical actor 0, and dispatches the selected setup's
matching zone row. See [setup selection](../../research/map-event-flag-route-selection.md) and
[request consumption](../../research/map-event-request-consumption.md) for those caller boundaries.

The scene creates no actors. It addresses logical Bowie 0, Peter 7, B 31 and NPC 128 using the
existing lookup table. The [predecessor's population review](creed-mansion-entry-and-first-goliath-scene.md)
owns the six fixed rows and follower allocation. Map33 is not overworld, so F65 supplies no Caravan
prefix. F67 selects Peter; independent F74/F68/F70 rows may allocate duplicate logical B actors,
with later lookup overwrite. Under only F67/F68, Peter/B/128 resolve to physical 1/2/3. Baseline
logical 128 uses a WORKER mapsprite while the text names Goliath; force membership alone does not
establish follower allocation or the supplied mapping.

Missing follower lookups can alias physical 0. Another B row changes action ownership. Action
installation lacks the ally-alive skip used by some placement/facing commands; HP/class can change
the mapped sprite without blocking action installation, and special sprites can skip sprite-change
work. Relative moves start from actual positions. F774 does not repair any of these inputs.

## Warning, Flashes and the Shrinking Sequence

**Confirmed:** the accepted complete scene has 43 commands, indexed 0–42 including its terminator.
The table covers all of them, including all three inline action bodies and the one custom body.
Direction, size, speed and wait numbers below are literal source operands, not measured distances
on screen or elapsed durations.

| Scene phase / commands | Ordered work |
| --- | --- |
| Goliath reacts, 0–4 | Set text cursor 1834. Wait for 128's `eas_Init`, then `eas_Jump`. A waited custom body sets speed `48,48`, jumps to `eas_Idle`, then ends with `ac_end`. A waited inline body moves Down 2, Left 2, then `endActions`. |
| Warning and first flashes, 5–12 | Target the camera at 128; single text 1834 warns against entering. White flash 4, wait 40, white flash 4, wait 30, white flash 4, wait 5. |
| Party reactions, 13–21 | Install Bowie's `eas_2xRightLeft`, wait 5; Peter's `eas_2xUpDown`, wait 5; B's `eas_2xRightLeft`, wait 20. White flash 4, wait 10, white flash 30. These installations do not wait for those actors to become idle. |
| Shrinking and rebuke, 22–26 | Install `eas_TwirlShrinkDisappear` on Bowie, Peter, then B without completion waits; wait 60; single text 1835 rebukes them. |
| Explanation, 27–31 | Wait for 128's `eas_Init`; waited inline Left 3, `endActions`; face 128 Up. Continuing text 1836 explains the door, followed by single text 1837's claimed exception. |
| Narrated collection, 32–36 | Stop 128's animation, wait 20, display single text 1838 without a portrait speaker; restart 128's animation; single text 1839 announces collection. |
| Departure, 37–42 | `stepEvent 12,13`; waited inline Up 2, Left 5, `faceUp 20`, `endActions`; wait for 128's `eas_Jump`; wait 20; `warp MAP_DESKTOP_KINGDOM,0,0,DOWN`; `csc_end`. |

The named action helpers explain the staging without guaranteeing its performance. `eas_Init`
resets acceleration factors to 0/0, speed to 32/32, acceleration/deceleration off, obstructability
on, entity/map uncollidable off, autofacing on, orientation Up, immersion off and size 24, updates
the sprite, then goes idle. It does not reset position. The second Init replaces the earlier speed
48. `eas_Jump` disables autofacing, moves relative `(0,-1)`, waits 5, moves `(0,1)`, waits 5, moves
`(0,0)`, restores autofacing and jumps idle. RightLeft and UpDown perform four named facing changes,
each with a sprite update and wait 30, then idle. Later installs replace the same actor's action
pointer; the look sequences are not first joined to completion.

Waited action forms poll only the selected physical actor's script pointer for `eas_Idle`.
The inline compiler appends a jump to idle; `faceUp 20` means face Up, update sprite, then action
wait 20, not a twenty-tile move. Relative moves scale signed tile deltas by MAP_TILE_SIZE=384
and use destination/conflict handling. Speed `48,48` is an action-engine operand, not a measured
pixel rate. There is no timeout or missing-actor recovery. Installers seed the action wait timer
with the resolved physical index, which the action wait consumer uses; even the first wait is not
an independently measured duration.

The three shrinking installs are concurrent work from the map script's perspective. Its wait 60
does not poll all three for idle. Subsequent waits concern 128 or physical 0's coordinates.
Aliased mappings, replaced/stalled actions and skipped sprite work can therefore defeat a claim
that three distinct actors visibly completed shrinking before the dialogue continues.

## What Goliath Says and What the Commands Change

**Confirmed:** texts 1834–1839 join sequentially to source keys 072A–072F. The complete literals
and controls are preserved here; 1836 continues into 1837 rather than opening a choice.

| Text | Speaker / form | Literal content |
| --- | --- | --- |
| 1834 | 128 / single | `No, don't go in there!{N}That door is...!{W1}` |
| 1835 | 128 / single | `What stupid kids!{W1}` |
| 1836 | 128 / continuing | `Whoever passes through this{N}magic door becomes small.{W2}` |
| 1837 | 128 / single | `That is, anybody who's not{N}protected by Mr. Creed.{W1}` |
| 1838 | none / single | `Goliath picks up{N}{LEADER}.{W1}` |
| 1839 | 128 / single | `Now, you're part of the{N}collection!{W1}` |

Five commands use speaker 128/modifier 0. Portrait and speech SFX resolve from the actual mapped
sprite; modifier 0 sets neither mirrored nor right-side bits and guarantees no displayed portrait.
The pickup line uses `$FF,ENTITY_NONE`, encoded `$FFFF`, bypasses that lookup and sets speech SFX
to zero. `{LEADER}` runs UpdateForce, takes the first BATTLE_PARTY_MEMBERS entry and resolves its
combatant name. It need not display the immutable name Bowie.

`{N}` advances the line. W1/W2 poll the current directional/A/B/C input mask; W2 also runs the
arrow and validation-sound path. Single-text commands close portrait/text and sleep 10 after
DisplayText; the continuing form leaves closure to the next command. Debug mode plus P2 Start
can latch SKIP_CUTSCENE_TEXT and bypass selected text commands and csWait sleeps. The nominal
six-text performance assumes that skip is not latched.

White-flash operands are not frame counts. The selected handler shifts the operand right three
bits and runs a DBF loop: 4 requests one fade launch, 30 requests four. Each launch selects all
palettes (`$F`), counter maximum 1 and FLASH_QUICKLY_2, waits through FADING_SETTING clearing and
another VInt, then restores the saved maximum; the handler finally duplicates palettes. The scene
contains four single-launch calls and one four-launch call. It proves neither a four-frame flash
nor a thirty-frame flash. Completion still depends on those services.

The camera command resolves logical 128 to its physical actor rather than a fixed camera coordinate.
Stop/start write that actor's animation counter -1/0; they do not globally stop/start motion, actions or input.
The pickup text lies between those writes. No selected command attaches Bowie to 128, grants
collection membership, recruits/removes a force member or offers a choice. **Inferred:** warning,
shrinking and dialogue together depict collection. The door's universal effect and Creed's
protection remain Goliath's explanation, not a separately implemented universal door rule.

## Temporary Sprite Work, Parking and Shared Size

**Confirmed:** the complete `eas_TwirlShrinkDisappear` contains 42 operations. It begins with
wait 5 and resize ON, then executes each row as facing change, size write, sprite update, wait 5
(nine groups of four operations):

| Order | Facing | Size operand |
| --- | --- | --- |
| 1 | Down | 22 |
| 2 | Right | 20 |
| 3 | Up | 18 |
| 4 | Left | 16 |
| 5 | Down | 14 |
| 6 | Right | 12 |
| 7 | Up | 10 |
| 8 | Left | 8 |
| 9 | Down | 6 |

Its final four operations are `ac_setPos 63,63`, `ac_resizable OFF`, `ac_setSize 18`,
`ac_jump eas_Idle`. There is no sprite update after the final size write.

The size command writes the **shared SPRITE_SIZE word**, while resize bit 3, facing, current and
destination positions, action pointer and wait timer belong to the physical entity. Sprite update
requests ChangeEntityMapsprite; capacity affects progress, and special mapsprites/entity number 32
skip this work. When the entity's resize bit is set, ResizeSprite consumes the current shared size
and transforms decompressed mapsprite pixels in RAM, including both walking frames. Concurrent
actions share that size scratch word. This establishes sprite work, not a persistent per-character
scale, gameplay-stat change or terrain-size rule.

SetPos scales 63 by 384, writes current and destination X/Y and clears the travel pair. It neither
deallocates the slot nor clears its lookup, changes force membership or issues a warp. Its X remains
below the scheduler's `$7000` empty-slot threshold. The sprite projector sends out-of-view sprites
offscreen; parking at `(63,63)` is distinct from guaranteed visible disappearance under every camera.
No selected scene/action command removes or restores a shadow, and the action name proves no
independent shadow result. Even a completed tail proves only parked temporary actors, resize off
and the shared size write 18. Later destination actors are outside this guide's scope.

## Opening the Live Door and Requesting the Transfer

**Confirmed:** `stepEvent 12,13` scales those coordinates by 384 and calls OpenDoor. With
CURRENT_BATTLE=255, Map33's complete step table selects source `(25,0)`, size 1x1, destination
`(12,13)`. Nonnegative source Y selects a copy of one live layout word at FF0000 with a 64-word
row stride. It plays SFX_DOOR_OPEN and sets the plane-update bit selected by MAP_AREA_LAYER_TYPE.
An in-battle call returns without that work.

This is a live-layout change, with no persistence flag. Map33's complete flag-event table contains
only F22's large replacement, not replay of this copy keyed by F774/F783. The copy guarantees no
rendered opening, collision/passability, traversal or save/load persistence. The final waited move
and Jump follow the copy and must return before the warp command can be reached.

The literal warp encodes map 22, X 0, Y 0, Down=3. Its handler writes MAP_EVENT_TYPE=1 first,
parameter 1=0 and parameters 2–5=`22,0,0,3`; it neither loads the map nor waits for arrival.
Earlier scene mutations are not rolled back on a later failure, and another writer may replace
the shared pending request. The route below assumes that complete tuple survives to consumption,
not an observed atomic transaction.

`csc_end` conditionally waits for view scrolling when a dialogue window exists, clears scrolling
speed and restores registers. Zone1 returns without a flag write. The zone wrapper closes
portrait/text, waits one VInt and waits for **physical 0's current X/Y to equal destination X/Y**.
That is neither a Peter/B completion join nor proof of normal source placement: a completed
shrinking tail already makes both coordinate pairs `(63,63)`.

WaitForEvent sees pending work before reinstalling normal camera target 0 and the current-mode
controller, so a surviving warp returns immediately. ProcessMapEvent clears the event type before
dispatching type 1. Its no-scroll branch consumes/clears WARP_SFX, makes physical 0 idle and unwinds
the exploration return. UpdatePlayerPosFromMapEvent first reads source tile position/facing, but
the explicit nonnegative X/Y bytes override both positions and parameter 5 supplies Down. Parked
source coordinates therefore do not change the requested `(22,0,0,Down)` destination. No usable
source-map control interval is promised between scene return and transfer.

## Desktop Admission Depends on Separate Flags

**Confirmed:** MainLoop returns from exploration through SwitchMap, then CheckBattle. This is not
the alternate post-battle entry that skips CheckBattle. The complete switch table has no Map22
replacement. Its sole battle-coordinate row is zero-based Battle22, tuple `22,9,0,30,30,255,255`:
map 22, area origin `(9,0)`, dimensions 30x30 and two wildcard trigger coordinates.

CheckBattle first matches map, then F422, then the trigger fields. It does not compare the request
against the area rectangle. Requested `(0,0)` is outside that rectangle but passes both wildcard
checks when F422 is set. On selection, it assigns the area **before** checking F522. F522 set clears
F422 afterward but does not cancel this invocation's Battle22 selection. Zone1, the scene and its
shrinking action write neither flag; narration supplies no destination-admission state.

| Supplied F422 | Supplied F522 | Immediate admission result |
| --- | --- | --- |
| Clear | Clear or set | Skip the Map22 row before area assignment or completion check; return d7=-1 and enter fresh exploration with map 22, `(0,0)`, Down. |
| Set | Clear | Assign area `9,0,30,30`, retain F422, return d7=22 and hand off to BattleLoop. |
| Set | Set | Assign the same area, clear F422, still return d7=22 and hand off to BattleLoop. A later check requires its own supplied state. |

Fresh exploration receives explicit map 22 rather than MAP_CURRENT=255, clears pending type and
assigns CURRENT_MAP=22/CURRENT_BATTLE=255 before population. **Stop before GetMapSetupEntities,
InitializeMapEntities and init.** The battle branch stops at the selected BattleLoop handoff
**before opening**. Neither branch establishes a loaded roster, completed arrival, final destination
placement, successful loading or usable control.

Successor identifiers only: baseline Map22 declares 30 entity rows and F522 selects an alternate
entity setup. Both pointer tables reference `ms_map22_InitFunction`; its dispatcher checks F776,
calls `cs_59656` when clear and sets F776 after return. Later F522-set/F778-clear selects
`cs_5996E` then sets F778. Population and those scene bodies belong to separate arrival/post-battle
work. This guide consumes no concurrent arrival research or Battle22 opening/Chessboard content.

## Evidence Review and Changed-State Limits

**Confirmed provenance:** the research is accepted at main
`73366b7a557fe03399a675a4103d89b89c9f9806`, pinned to
[SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [research owner](../../research/creed-magic-door-and-desktop-transfer.md#agreement-and-provenance)
owns exact paths, line ranges and direct-source joins. Its complete selected review establishes local
scene/action/text/consumer boundaries; public aggregates do not prove the whole scene or runtime success.

| Reviewed evidence surface | What it supports and where it stops |
| --- | --- |
| Complete accepted magic-door research and predecessor guide | All 43 commands, three inline/one custom bodies, six text/control joins, complete 42-operation action and immediate consumers, copy/request/admission order. No natural or visible completion claim. |
| `sf2-map-script-engine-static-v1`, [fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json) | programSummary and selected transition, step, camera, action, animation and flash facts. The public fixture contains aggregate/family facts; selected complete joins remain in research, not an assumed public full script/text corpus. |
| `sf2-map-events-static-v1`, [zone shard](../../../tests/fixtures/h2/map-events/zone-programs.json), Map33_ZoneEvent1 | Four operations at source lines 53,54,55,58: F783 check, set-branch return, cs_5AF36 call, return. No F774 prerequisite or local flag write. |
| [Map-init fixture](../../../tests/fixtures/h2/map-init-static-v1.json) and [setup fixture](../../../tests/fixtures/h2/map-setup-static-v1.json) | Shared population/init/selector context; neither aggregate establishes this scene's complete content, supplied cast or destination arrival. |

Reviewed schema selections are the [script-engine fixture](../../../schemas/h2/map-script-engine-fixture.schema.json),
[zone programs](../../../schemas/h2/map-events-zone-programs.schema.json) and their referenced fact shapes.
The [script verifier](../../../src/sf2tool/h2/map_script_engine.py) compares provenance/address and
selected semantic fields; the [event verifier](../../../src/sf2tool/h2/map_events.py) compares complete
assembled fixture semantics. [Map-init](../../../src/sf2tool/h2/map_init.py) and
[setup](../../../src/sf2tool/h2/map_setup.py) own their separate fixture/schema checks. These are existing
verification boundaries, not evidence that every sentence was independently executed. The
[research reproduction route](../../research/creed-magic-door-and-desktop-transfer.md#counterexamples-and-reproduction)
owns retained equality and selected direct-source checks. This synthesis does not rerun/regenerate them.

Preserve [#896's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/896#issuecomment-6173732542),
[rebase handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/896#issuecomment-6173857962)
and [PR897 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/897#issuecomment-6173871815):
distinct original/rebased head/tree/base and CI receipts, unchanged 18,285,979-byte pair/0.079-second
comparison, complete selected joins and inspection failures. Preserve the predecessor's
[#895 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/895#issuecomment-6173738883)
and [PR898 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/898#issuecomment-6173819074),
including its separately owned comparisons, CI and linked earlier failures. Handoffs locate checks;
tracked owners and exact accepted Git objects establish facts. This guide's direct checks, configured
committed planner and actual final-head CI belong to Issue #899.

| Changed supplied state | Static discriminator |
| --- | --- |
| F783 set, selected Zone1 | No new scene/text/copy/warp from the callback; wrapper closure/waits still apply. |
| F783 clear, F774 independently clear/set | Same local selection; prior itinerary, setup and positions may differ. |
| Missing/duplicate mappings or changed HP/class | Different/aliased actors or skipped sprite work; logical names do not guarantee three visible participants. |
| Shrinking incomplete/replaced after wait 60 | Scene may continue without all three idle; 128's action waits and physical 0's coordinate wait test different things. |
| Stalled service or replaced/incomplete pending request | Earlier changes remain; final warp may never be reached or consumed as written. |
| F422/F522 changed | Follow the independent admission table, including outside-area wildcard admission and selection before completion clear. |

These examples are not executed scenarios. **Unknown:** natural trigger reach; actual setup, flags,
party, HP/class, lookups, source position, mode and pending state; service/concurrent-action completion;
rendered shrinking, shadows and pickup; actual destination admission/control and persistence.
They create no automatic runtime queue under [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../decisions/0016-remake-start-evidence-deferral.md), and complete neither the campaign
nor [design epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
