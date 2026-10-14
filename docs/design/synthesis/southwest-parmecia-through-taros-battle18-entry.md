# Southwest Parmecia, Taros's Warning and Conditional Battle18 Entry

The selected western exit from South Parmecia scrolls into Southwest Parmecia while exploration
continues. The shrine entrance takes a different route: it returns to MainLoop, where an unlocked
Battle18 is selected before the shrine's exploration warning can run. Without that admission, Rohde's
acceptance flag decides whether the group hears Taros's warning and requests a retreat. Fresh
Battle18 has its own confrontation, followed by a separate rebuild of combat actors.

**Confirmed:** the accepted [research owner](../../research/southwest-parmecia-through-taros-battle18-entry.md)
establishes these conditional transfers, the complete warning and before scene, and the fresh/saved
entry boundaries. **Inferred:** the dialogue presents Taros as an obstacle to obtaining the Caravan.
**Unknown:** a natural journey, actual supplied state, completed services, presentation, usable input
and persistence. Fnnn means flag nnn; coordinates are map tiles and table indices are zero-based.
Wait counts, camera speeds and movement operands describe source requests, not measured durations.

Read [Hassan departure and Battle17 returns](hassan-departure-and-battle17-outcomes.md) for the
preceding selected region, [Rohde's proposal](hassan-ancestry-and-rohde-caravan-choice.md) for his
independent acceptance/following writes, and [the Achilles Sword puzzle](ribble-tree-opening-and-achilles-sword.md)
for conditional item custody. These links do not establish a required chronological journey.
This guide stops at conditional exploration return, unrelated battle admission or Battle18's
pre-activation start handoff. Battle18 outcomes, Caravan acquisition/operation, Rick and later travel
require separate owners; no combat or playable turn is observed here.

## Two Transfers with Different Consequences

**Confirmed selected rows:** Map69 row5 matches X0 with any Y and requests a Left scroll to Map68,
X62/Y255, facing Left. Earlier rows do not match X0; at `(0,0)` this row wins before the later
any-X/Y0 row. Map68 row0 instead matches `(55,43)` and requests Taros Shrine Map12 at `(12,30)`, Up,
with no scrolling. First-match table selection establishes neither traversable terrain nor a
continuous walk between these points.

The warp producer scans supplied positions in 384-unit tiles, allowing negative coordinate bytes
as wildcards. It writes the pending type1 request, parameters and warp sound. It tests no Sword,
Battle17 completion, F725, F418 or membership. The immediate walking/vehicle callers still require
their own movement/layout conditions. Actual mode1 is Caravan and mode2 is raft, despite the upstream
action names `eas_Raft`/opcode7 and `eas_Caravan`/opcode8 respectively. This does not prove access in
any mode.

| Consumer | What the selected transfer does | Where control can continue |
| --- | --- | --- |
| Map69 → Map68 scroll | Clears pending, idles main0, changes CURRENT_MAP to 68, loads selected tiles/layout and requests the Left plane/sprite transition. After that service, main0's current/destination X becomes 62; negative Y255 preserves its supplied Y. | Returns inside the existing ExplorationLoop. There is no MainLoop battle scan, fresh population, Map68 init, healing or temporary-flag clear. CURRENT_BATTLE is not rewritten here. |
| Map68 → Map12 no-scroll | Clears pending, submits/clears the supplied warp sound, idles main0, removes the exploration return frame and returns `(12,12,30,Up,d4=0)`. Nonnegative requested coordinates replace the sampled position. | MainLoop switches the map, scans for battle, then chooses battle or exploration. |

The scroll consumer does not assign facing from its requested-facing parameter; current/controller
facing remains supplied. Sprite-position adjustment during the scroll is distinct from entity-data
coordinates. PLAYER_TYPE and the saved RAFT_MAP/X/Y tuple remain unchanged on both transfers.

After the scroll, `DeclareRaftEntity` uses the sampled player state. Mode0 collapses listed followers'
current/destination coordinates to that position; mode1 skips this loop; mode2 returns immediately.
For the applicable non-raft modes, F64 and matching saved raft map can place separate physical31,
lookup63, at its saved position. A mismatch takes removal; F64 clear skips placement/removal. This
neither moves the saved raft to Map68 nor creates a follower list. Only absent another pending event
does WaitForEvent follow main0 and install the current controller. F419 alone does not admit
Battle19 during this in-place scroll; later event production is a separate boundary.

## Battle Selection Comes Before the Shrine Warning

**Confirmed:** the complete flag-switched-map table has no source12/68/69 override. The only Map12
battle row is 18, `BATTLE_VERSUS_TAROS`, with area origin `(0,0)`, size `26x32` and wildcard255/255
trigger coordinates. CheckBattle requires F418. The rectangle supplies battle geometry; entry Y30
and player containment are not admission tests. On selection it writes the area fields. F518 set
clears F418 but still returns 18 for that call.

| Flags supplied at the Map12 scan | Result |
| --- | --- |
| F418 set; F725 either | Admit Battle18, without Map12 exploration init or its warning first. F518 can clear unlock after selection without canceling this admission. |
| F418 clear; F725 clear | Enter exploration; init runs the warning, then checks F518. |
| F418 clear; F725 set | Enter exploration; init skips the warning, then checks F518. Control return remains conditional. |

F468, F725, Battle17 completion, Sword custody, Rohde's following and membership are not Battle18
admission checks. Rohde's accepted Yes path writes F725/F84/F418 together, but these consumers do not
repair independently supplied flags. F725 is acceptance, F418 is unlock, F468 is the shared intro
gate and F518 is completion; none can stand in for another.

## The Exploration Cast and the Complete Warning

**Confirmed population:** supplied-map exploration clears pending, reduces STEP_COUNTER by 20000
with a signed-word negative clamp, heals eligible allies, fades out and writes map12/battle255.
It loads tiles, selects the sole Map12 setup, rebuilds entities/lookups, clears temporary flags 256–383,
sets F80, loads the map, installs base VInts and runs init. Healing covers living allies and immortal
Peter/Lemon; it does not revive every dead ally or establish membership.

Map12's one fixed setup row places Taros at `(12,8)`, Down, special mapsprite255, `eas_Init`.
Logical128 maps to special physical47 without consuming an ordinary slot. This is a scene actor,
not combatant128's statistics. Main0 is separately created at the requested position/facing. No
fixed Peter, Rohde, FOLLOWER_B or Caravan row is present.

The normal follower table applies even with F65 set. F67 supplies Peter, F84 supplies Rohde, and F68
supplies Oddler at FOLLOWER_B31. Other supplied flags 74/70 can write Astral rows to that same logical
identity; table order decides conflicting lookup writes. F725 does not populate these actors.
Exploration following and joined/active battle membership remain separate. Mode2 makes main0 a raft
and hides followers; other modes retain the F64/matching-saved-raft placement rule. Init acquires or
normalizes no vehicle.

Init at `0x57D04` first tests F725. If clear, it runs `cs_57D22`; only after return does it test F518
and, when set, move logical128 out of map. That movement changes current/destination coordinates to
raw `$7000`; it clears neither lookup nor membership nor HP and does not report a defeat.
F725-clear/F518-set therefore still requests the warning/retreat before hiding Taros. Conversely,
F725-set/F518-clear can leave him present without warning or battle. Init writes none of these four
story/battle flags.

### Warning and Retreat Request

**Confirmed complete warning:** `cs_57D22` at `0x57D22` has 51 commands and begins at text 3197.
The stages below preserve the complete ordered action/dialogue meaning; a waited action joins only
its named actor's idle pointer. Started actions can overlap or be replaced before finishing.

| Stage | Staging, dialogue and handoff |
| --- | --- |
| Assemble and approach | Set camera speed `$30`; initialize/wait Bowie, Peter and FOLLOWER_B in that order. Place them Up at `(12,31)`, `(13,31)`, `(11,31)` and fade in. Start Bowie/Peter Up5 and wait FOLLOWER_B Up5. Taros orders them to stop (3197); wait 10. |
| React | Start Bowie/Peter jumps and wait FOLLOWER_B's jump, then repeat that sequence. Wait 30, then 5. Start Bowie's Right/Left look twice, wait 5; Peter's Up/Down look twice, wait 5; FOLLOWER_B's Right/Left look twice; wait 80. |
| Discover the speaker | Wait Peter Up1; face Bowie/FOLLOWER_B Up. Peter asks who spoke (3198). Move the camera to `(7,5)`; Peter remarks on the figure's size (3199). Wait 30; Taros orders them away and threatens attack (3200). |
| Decide to leave | Move the camera to `(7,21)`; wait Peter Down1. Face Bowie/FOLLOWER_B Right and Peter Left. Peter reacts to the threat and urges the named leader to leave (3201), then says they do not want the fight (3202). The first message continues with `nextText`; the second closes through `nextSingleText`. |
| Retreat | Wait 20; nod Bowie. Peter urges haste (3203). Start Bowie/Peter Down5 and wait FOLLOWER_B Down5. Queue no-scroll Map68 `(55,44)`, Down, then end. |

There is no Yes/No prompt, Rohde speech, inventory test or flag write. A supplied Rohde follower
can still exist; silence in this program does not delete him. Bowie's nod is a scripted gesture,
not recorded player consent. The leader substitution reads the first current active battle-party
member's name after UpdateForce; it is not hardcoded to Bowie or a validated empty-list fallback.

### Consuming the Retreat

The script warp writes the pending request without immediately loading Map68, exiting the script
or writing WARP_SFX. Ordinary termination returns to init's F518 check, then exploration's
conditional palette/music/fade work. There is no all-actor/main-position barrier at init return.
With the retreat still pending, WaitForEvent returns it before reinstalling ordinary control;
the no-scroll consumer submits/clears whatever warp sound is supplied and returns
`(68,55,44,Down,d4=0)` to MainLoop. No playable shrine pause is guaranteed between warning and retreat.
Another pending write can replace the request; this is not an atomic queue.

The only Map68 battle row is 19, area `(5,19)`, size `48x32`, wildcard255/255. Supplied F419 admits it;
F519 can clear its unlock after selection. This guide stops at that unrelated admission. Otherwise
fresh Map68 exploration rebuilds population, clears temporary flags and runs its return-only init.
Its setup has no fixed entities or alternate flag row; followers use current flags, with F65 choosing
the overworld table, and the mode/F64/saved-raft rules still apply. The warning writes none of those
states. With no later pending event, ordinary following/controller installation can occur. Natural
return, completed services and usable input remain Unknown.

## Battle18 Has a Separate Before Scene

**Confirmed entry:** BattleLoop first writes PLAYER_TYPE=0 and F399, then tests F88. Fresh entry
clears seconds, sets map12/battle18 and installs base VInts before the before wrapper. Slot18 selects
`bbcs_18` at `0x4B1D8`; the start slot selects `ms_Empty` at `0x47A4E`. The before wrapper tests F468
without setting it. The start wrapper, if F468 is clear, sets it before executing the empty terminator.
F468-set fresh entry skips both scripts and still rebuilds combatants. F725 and F518 are not intro
guards, and Map12's exploration init/warning is not invoked by this scene load.

F88-set entry instead restores saved seconds, clears F88 and AI memory, clears the view target,
loads saved battle state and branches directly to the individual-turn loop. It bypasses incoming
map/battle assignment, before/start, fresh healing/placement and new turn-order generation, and lacks
fresh entry's explicit d0=0 before LoadBattle. Saved map, area, roster and turn state must already
agree; this is not a repaired Taros opening or proof of durable save/resume. This guide stops at
that branch.

### Scene Population

`loadMapFadeIn Map12,7,22` starts/waits fade-out, loads tiles/map and restores display/VInt services.
The visible fade-in is a later explicit command. `ce_4B2DA` begins with a **three-word main header**:
main0 `(12,32)`, Up. Its remaining two rows are Taros `(12,8)`, Down, special sprite255 and Caravan
`(12,5)`, Down, sprite62, both `eas_Init`. The header is not a third NPC. Taros maps to physical47;
Caravan takes the next ordinary physical slot and lookup129, depending on earlier follower count.
Caravan129 is neither FOLLOWER_A nor a combatant nor the transport controller.

Loading this population clears old exploration actors and rebuilds Map12's normal flag-selected
followers. The scene table does not supply Peter, FOLLOWER_B or Rohde. F84-clear/F418-set does not
synthesize Rohde, and speech cannot repair a missing mapping. The staged vehicle grants no F65,
transport mode, inventory or membership; no scene command checks or consumes the Achilles Sword.

### Complete Confrontation

**Confirmed:** `bbcs_18` has 51 commands and begins at text 2520. Every inline action body contains
one move followed by `endActions`; there is no additional start dialogue hidden behind the empty
start target. All eight messages use `nextSingleText`.

| Stage | Staging, dialogue and handoff |
| --- | --- |
| Assemble | Load Map12/camera `(7,22)` and the scene population. Initialize/wait Bowie, Peter, FOLLOWER_B and Rohde in that order. Position Peter `(13,33)`, FOLLOWER_B `(12,33)`, Rohde `(11,33)`, all Up. Stop Caravan129 animation, request shrine music, fade in, set camera speed `$40`. |
| Approach and react | Start Bowie/Peter/Rohde Up6 and wait FOLLOWER_B Up6. Taros orders a stop (2520); wait 5. Start Bowie's Right/Left look twice, wait 5; Peter's Right/Left look twice, wait 5; Rohde's Up/Down look twice, wait 5; FOLLOWER_B's Right/Left look twice; wait 30. |
| Identify Taros | Wait Peter Up1; he asks who the large figure is (2521). Wait Rohde Up1; Rohde asks whether Peter recognizes him (2522). Face Bowie/Peter/FOLLOWER_B Up; wait 30; track actor128 with the camera. Rohde identifies Taros as the ancient shrine's guardian (2523); Taros repeats the warning and threatens attack (2524). |
| Point to the Caravan | Set camera destination `(7,22)`; face Bowie Right, Peter Left and Rohde Right. Peter asks what to do (2525). Face Bowie Left; Rohde points to the Caravan behind Taros and insists on obtaining it (2526). |
| End the exchange | Face Bowie/Peter/Rohde Up. Taros announces that time is up and battle begins (2527); end. There is no warp or story-flag write. |

These are attributed statements: Rohde's account does not independently prove Taros's history,
his demand does not obtain a vehicle, and Taros's announcement does not complete battle services.
Neither the cast nor its dialogue establishes a required party composition or Sword damage effect.

## Actors and Services Must Still Satisfy Their Own Conditions

**Confirmed shared limits:** cleared logical lookups default to physical0. An absent Peter, Rohde or
FOLLOWER_B can therefore alias main0; invalidated255 is a different invalid state. Conflicting
follower writes or excessive supplied lists gain no presence/capacity repair from these scripts.
`setPos`/`setFacing` skip a zero-HP packed ally below 30, while FOLLOWER_B31 and Taros128 bypass that
guard. Action installation and nod lack an equivalent HP/presence guard. Exploration heals before
the warning; fresh battle heals after the before scene. Later healing cannot establish earlier
placement of a dead ally. Caravan's animation-counter -1 write is not a general AI/vehicle stop.

`eas_Init` selects speed 32, zero acceleration factors, disables acceleration/deceleration, sets
obstruction/collision, auto-facing, Up orientation, immersion off and size 24, refreshes the sprite
and idles. Jump disables auto-facing, requests `(0,-1)`, waits 5, requests `(0,1)`, waits 5, then
requests `(0,0)`, restores auto-facing and idles. Look actions request four alternating facings with
wait 30 after each. Inline moves scale their relative displacement by 384 from the current position,
check destination conflicts and install travel/velocity; destination equality precedes idle.
Actor-specific waits do not join all concurrently started moves, jumps or looks. Later writes can
replace unfinished actions, including the warning's unjoined Bowie/Peter work.

The warning's Taros modifier is `$0`, Peter's `$80`; battle Peter uses `$C0`, Taros/Rohde `$0`.
These select portrait-side/mirroring bits, with an existing-window guard. Mapped sprites select
Taros's portrait/special bleep, Peter's base/promoted variant and Rohde's special-sprite properties.
`nextSingleText` resolves speaker services, waits for scrolling, displays/advances text, closes
portrait/text and sleeps 10; `nextText` lacks that close/sleep tail. Text controls N, LEADER and W1/W2
are line/substitution/acknowledgement controls, not extra scene commands. W1/W2 poll directional/A/B/C
input with VInts while updating RNG/seed copy; W2 also services the arrow and validation sound.
Typewriting0 is not ordinary-control readiness. Neither dialogue nor sound is observed here.

Camera destinations disable entity tracking and wait for scrolling; camera-follow128 uses its mapped
physical target, with following text's scroll wait. `fadeInB` requests half-in-from-black over all
palettes with factor 6 and waits through VInts. Music submission does not wait for an entire track.
Ordinary `csWait` uses its low byte as a VInt count. Debug mode plus player2 Start can set script skip
after entry clears it; skipped text handlers consume arguments without display/cursor increment,
and skipped waits omit Sleep. End conditionally waits for scrolling when the dialogue-window index
is nonzero and clears camera speed. Nod has its own animation/sprite/DMA and 10/20/10 sleeps, without
consent or HP checks. None of these seams supplies an all-actor, rendering, audio or usable-input
completion guarantee.

## Rebuilt Combatants and the Empty Start

**Confirmed fresh order:** after the before wrapper returns, BattleLoop restores map12/battle18,
clears region flags 90–105, heals eligible allies, initializes ally/enemy positions, clears AI memory,
loads battle with d0=0 and invokes start. Healing restores HP/MP maxima for living allies and immortal
Peter/Lemon, retaining existing STUN/POISON/CURSE bits; other dead allies are not revived.

`BattleSpriteset18` at `0x1B4546` supplies 12 ally placements, 14 enemies, five AI regions and zero AI
points. The first placement is `(12,27)` at area origin `(0,0)`, distinct from shrine request
`(12,30)` and scene main header `(12,32)`. Ally initialization clears all 30 X values and rebuilds the
current joined/active party list. Dead members consume list/loop slots without advancing placement
rows; excess members can leave the active party. Twelve rows do not force twelve living allies,
Peter, Rohde or Oddler into combat. The first list entry is read before the zero-member exit check;
empty/inconsistent supplied lists are not guaranteed safe or repaired.

All 14 enemy rows are STARTING, slots128–141, with TAROS at `(12,8)` in row0; other slots follow their
hidden/cleared path. Battle18 is absent from the random-battle upgrade table, even with F518 supplied
set. Enemy identity and AI data establish neither activation nor turns, damage or a winning route.

LoadBattle fades out, loads tiles and clears VInts. PositionBattleEntities clears entities/lookups
and declares placed combatants, excluding negative coordinates and neutral enemies. The old scene
and exploration follower cast are replaced. View target, sprite/map resources, VInts, terrain,
music and fade-in have their separate consumers. Start sets F468 and executes the empty terminator
only when that flag is clear. Fresh return reaches `@Start` **before** ActivateEnemies, region/spawn
processing and turn generation. This is a pre-activation handoff, not a first observed playable turn.

| F468 at fresh entry | F518 at population rebuild | Before scene and neutral Caravan |
| --- | --- | --- |
| Clear | Clear | Stage scene Caravan129; later replace it with neutral159. Start sets F468. |
| Clear | Set | Stage scene Caravan129; rebuild skips the neutral tail. Start sets F468. |
| Set | Clear | Skip before/start scripts; rebuild still adds neutral159. |
| Set | Set | Skip before/start scripts and the neutral tail; combat rebuild still runs. |

The completion-gated neutral record places a **new** Caravan at `(12,5)`, **Left**, logical159,
with max/current HP0, status0 and the neutral activation bit. It declares/maps a new physical actor
with `eas_Motionless` (motion-off then idle). F518 set skips this tail. Scene129's Down facing and
animation-stop state do not carry over: lookup129 now belongs to an enemy row. Taros is likewise
rebuilt from his combatant row despite repeated coordinates. Neutral HP0 is a declaration input,
not a vehicle defeat/acquisition result. Neither Caravan sprite is a usable transport grant,
Rohde join, inventory change or Sword-effect test.

## Evidence Trace and Review Boundary

**Confirmed provenance:** accepted #840/PR842 binds SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [reproduction owner](../../research/southwest-parmecia-through-taros-battle18-entry.md#reproduction-acceptance-and-deferred-evidence)
retains enum/warp/init/pointer/header/command/actor/roster/neutral joins. This guide consumes accepted
joins, without source reinvestigation, payload regeneration or runtime acquisition.

| Explained boundary | Contract and exact existing fixture | Review limit |
| --- | --- | --- |
| MainLoop and battle selection | [exploration flow](../contracts/exploration-control-flow.md), [map-entry routing](../contracts/map-entry-routing-state.md); `sf2-gameflow-core-static-v1` in [gameflow](../../../tests/fixtures/h2/gameflow-core-static-v1.json), `sf2-common-maps-static-v1` in [maps](../../../tests/fixtures/h2/common-maps-static-v1.json) | Shared call order and unlock/wildcard/completion facts; selected Map12/68/69 rows come from accepted research. |
| Setup, warning and pending requests | [map exploration](../contracts/map-exploration.md); `sf2-map-init-static-v1` in [init](../../../tests/fixtures/h2/map-init-static-v1.json), `sf2-map-event-request-consumption-static-v1` in [request](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json) | Init's `expected.embeddedPrograms` selects `cs_57D22`; shared pending facts do not compose a natural scroll/retreat. |
| Population, actions, camera and dialogue | [roster](../contracts/party-roster-state.md), [dialogue](../contracts/dialogue-system.md), [map exploration](../contracts/map-exploration.md); `sf2-map-script-engine-static-v1` in [script](../../../tests/fixtures/h2/map-script-engine-static-v1.json) | `expected.entityPopulationCommandFacts`, `entityPlacementCommandFacts`, `entityActionBridgeCommandFacts`, `mapCameraControlCommandFacts` and `dialogueCommandFacts` own handler/source joins, not completed Taros services. |
| Before/start and fresh/saved boundary | [cutscene routing](../contracts/battle-cutscene-routing.md), [battle lifecycle](../contracts/battle-control-lifecycle.md); `sf2-battle-cutscenes-static-v1` in [hooks](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), `sf2-battle-control-static-v1` in [control](../../../tests/fixtures/h2/battle-control-static-v1.json), `sf2-battle-loop-static-v1` in [loop](../../../tests/fixtures/h2/battle-loop-static-v1.json) | Shared intro/F88/healing facts; exact before → rebuild → start order and slot18 targets use the stricter accepted research joins. |
| Combat/neutral identity and membership | [encounter definition](../contracts/battle-encounter-definition.md), [roster](../contracts/party-roster-state.md); `sf2-battle-routing-data-static-v1` in [routing](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), `sf2-battle-spriteset-data-static-v1` in [spriteset](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), `sf2-common-stats-static-v1` in [stats](../../../tests/fixtures/h2/common-stats-static-v1.json) | `routeTargetsParsed=false` and `rowContentTracked=false`; aggregate counts do not establish slot18 rows, Caravan159 or combat results. Those selected joins remain research-owned. |

The corresponding schema/consumer pairs are [gameflow schema](../../../schemas/h2-gameflow-core-static-fixture.schema.json)/[gameflow.py](../../../src/sf2tool/h2/gameflow.py),
[maps schema](../../../schemas/h2-common-maps-static-fixture.schema.json)/[maps.py](../../../src/sf2tool/h2/maps.py),
[init schema](../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../src/sf2tool/h2/map_init.py),
[request schema](../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../src/sf2tool/h2/map_event_request_consumption.py),
[composed script schema](../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../src/sf2tool/h2/map_script_engine.py)
with [component ownership](../../../src/sf2tool/h2/map_script_schema.py),
[hook schema](../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)/[battle_cutscenes.py](../../../src/sf2tool/h2/battle_cutscenes.py),
[control schema](../../../schemas/h2-battle-control-static-fixture.schema.json)/[battle_control.py](../../../src/sf2tool/h2/battle_control.py),
[loop schema](../../../schemas/h2-battle-loop-static-fixture.schema.json)/[battle_loop.py](../../../src/sf2tool/h2/battle_loop.py),
[routing schema](../../../schemas/h2-battle-routing-data-static-fixture.schema.json)/[battle_routing_data.py](../../../src/sf2tool/h2/battle_routing_data.py),
[spriteset schema](../../../schemas/h2-battle-spriteset-data-static-fixture.schema.json)/[battle_spriteset_data.py](../../../src/sf2tool/h2/battle_spriteset_data.py)
and [stats schema](../../../schemas/h2/common-stats-fixture.schema.json)/[stats.py](../../../src/sf2tool/h2/stats.py).
Their owning commands are `uv run sf2 h2` with `gameflow-core`, `common-maps`, `map-init`,
`map-event-request-consumption`, `map-script-engine`, `battle-cutscenes`, `battle-control`,
`battle-loop`, `battle-routing-data`, `battle-spriteset-data` and `common-stats` respectively.
Controlled reproduction loads the ignored worktree input configuration in the same process.

The synthesis review checks accepted research, shared contracts and bounded tracked-fixture,
schema/consumer selections for flag independence, actor/list/HP limits, service order and aggregate
overclaims. The lifecycle contract's compact new-entry summary is not an ordered before/start
schedule; its [before-body ownership](../contracts/map-exploration.md#bound-before-body-and-battle-loader-ownership)
and accepted research supply the precise ordering used here. No Layer A repair is required for
this narrower explanation. Independently supplied F418/F725/F468/F518, F88, F419, follower conflicts,
missing lookups/dead allies, different party lists, modes/raft state and replaced/stalled requests
are discriminating review cases, not an executed runtime matrix.

The [PR842 final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161224116),
[original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161137887)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161232331)
preserve original/rebased CI, retained comparisons, corrected/failed inspections and linked
predecessor results. Failed guessed paths or oversized outputs are not absence evidence. Accepted
results are not replayed solely for prose; aggregate fixtures do not become composed runtime coverage.

**Unknown:** natural travel/admission/retreat; actual flags, HP, lists, mappings, transport/raft and
pending/input state; completed actions/all-actor/services, rendered camera/text/audio, usable control
and save/resume persistence. These remain deferred under
[ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) and
[ADR0016](../../decisions/0016-remake-start-evidence-deferral.md). They do not authorize acquisition,
later-story investigation or a new remake capability.
