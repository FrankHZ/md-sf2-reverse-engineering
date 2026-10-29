# Battle07 Outcomes and the Exploration Handoff

Battle07's first victory leads into a long tower scene before exploration is restored. **Inferred
narrative connection:** the rescue of Elis fails, Bowie acquires Galam's jewel, and the group retreats
as the tower shakes. **Confirmed authored content and static state:** the after program stages that
sequence, writes jewel/follower/earthquake flags and prepares the return to Map58. Those facts do
not independently confirm the characters' explanations or an observed natural playthrough.

Read this after [Battle06 outcomes through Battle07 admission](battle06-outcomes-through-battle07.md).
The accepted [research owner](../../../../research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md) supplies the
selected source composition and provenance. This guide ends at the first conditional exploration
control boundary and Map58's immediate outgoing record. The castle/town evacuation, departure and
Battle08 remain outside scope. It adds no remake capability or product/UI decision.
The [roadmap](../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns the
Standard-rigor agreement for [#719](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/719).

`Fnnn` means flag nnn. Original-source references below use `disasm/` at
[`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The research and fixtures bind the USA retail ROM identity recorded in the
[root baseline](../../../../../README.md#pinned-original-baseline). Command indices are zero-based
map-script records, not frames or source-line counts.

## 1. The Outcome Determines Whether the Long Scene Runs

**Confirmed static rule:** Battle07 uses the ordinary faction-count outcome. Placed, living
combatants count; Bowie's zero HP forces the ally count to zero. Zero allies selects defeat before
zero enemies selects victory. Defeating enemy slot 128 alone does not establish a special Battle07
win: the defeated route is empty, its leader-presence row is zero and the position/HP override
table contains no Battle07 entry. Galam and Dark Smokes as scene actors do not change these rules.
The [controller contract](../../../contracts/battle/battle-control-lifecycle.md#post-action-and-after-turn-order)
and [selected outcome review](../../../../research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order)
own the checks and selected rows.

| Outcome / incoming state | Ordered consequence | Return boundary |
| --- | --- | --- |
| Victory, F507 clear | Heal eligible allies; seed a current-map/position/facing return; execute `abcs_battle07` and the shared join tail; clear F407 and set F507. | The scene replaces the seed with Map58 `(13,35)`, Down, and writes F385/F74/F640 before caller completion. |
| Victory, F507 set | Heal, retain the seed, skip the entire after program, execute the same join tail; clear F407 and set F507. | Ordinary Map58 return uses the first active member's final battle position and entity facing, with origin `(0,0)`. No fresh jewel/follower/earthquake writes or fixed `(13,35)` return follows from this branch. |
| Defeat | Update the battle unlock flag; fixed loss text 363; restore Bowie's maximum HP; halve gold by unsigned floor division; obtain the egress position. | Return result `d4=-1`; no after program/join or new F507/F385/F74/F640 writes. F507 clear leaves F407; F507 set clears it. No Battle04 special-loss path applies. |
| Egress / Angel Wing | Egress charges spell MP and uses fixed text 274; Angel Wing removes the selected item slot and uses text 275. Their shared tail presents the exit, updates the unlock flag and obtains the egress position. | Return result `d4=0`; same conditional F407 handling as defeat, without defeat's gold halving or after/join call. |

The [after-wrapper contract](../../../contracts/battle/battle-cutscene-routing.md#after-battle-route-and-join-tail)
preserves the shared tail even when the scene is skipped. Selected row 7 of the consumed cutscene
join table supplies zero to `JoinForce`, meaning Bowie; it supplies no new story recruit. Do not
substitute the similarly named legacy global table or infer a visible recruitment event from this
helper call.

**Confirmed input-dependent escape example:** Battle07 has no special egress destination. With
the predecessor's unmodified `EGRESS_MAP=16` and F399 set, savepoint selection returns Map16
`(27,7)`, Up. Shrine use or another caller can change that input. An unlisted map gives `(1,1)`,
Up; BattleLoop sets F399, excluding the helper's game-start fallback here. The F64-gated raft reset
is a separate helper side effect. All return destinations pass through `SwitchMap`; no natural
escape/reentry route is proved by this example.

Victory heals living allies and the Peter/Lemon exceptions before the scene. Exploration calls
that heal again on every outcome return: restore HP/MP, skip other dead allies, retain only
already-present stun/poison/curse bits, then rebuild stats. Egress's charged MP and the later heal
are separate stages. Scene falls, shaking and disappearances add no combat HP/death transaction.

### Intro skips preserve different follower inputs

**Confirmed:** F457 gates before/start independently of F507. Executed `bbcs_07` clears F83 before
staging the confrontation; F457-set reentry skips that clear, and F88 suspended entry skips both
intro programs. Neither skip prevents the after scene when F507 is clear. The after program does
not clear F83. The [preceding entry boundary](battle06-outcomes-through-battle07.md#6-battle07-before-clears-royal-following-and-stages-the-confrontation)
therefore remains a conditional input to this return, rather than a guaranteed normalized state.

## 2. The Tower Scene Moves from Confrontation to Retreat

**Confirmed program shape:** `abcs_battle07` (`0x49F7E`), in
`data/battles/entries/battle07/cs_afterbattle.asm`, runs commands 0–396 as one linear program.
It contains no explicit flag branch, subroutine call or intermediate field-control return.
Individual handlers still have liveness checks and service dependencies. Its `loadMapFadeIn`
sets Map58/camera `(8,16)`; `loadMapEntities ce_4A912` then rebuilds the scene population.
Camera coordinates are distinct from Bowie's starting scene position `(13,23)` and eventual
return `(13,35)`.

**Confirmed authored dialogue:** one cursor starting at 2385 supplies ordinary unskipped IDs
2385–2460. These summaries use the accepted text/actor review, without copying a transcript.
Debug/P2-Start skipping can suppress ordinary text and cursor increments; there is no fixed-ID
`txt` command inside this after program. The
[dialogue contract](../../../contracts/interface/dialogue-system.md) owns the command/input boundaries, and the
[scene review](../../../../research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md#long-scene-actors-phases-and-text)
owns phase, speaker and text provenance.

| Scene phase | Authored content and meaningful boundary |
| --- | --- |
| Hostage confrontation — commands 0–89, IDs 2385–2401 | King and minister advance; Galam threatens Elis and effect actors enact an attack, prompting the royals to recoil. Local block copies and actor motion stage the confrontation; threats and reassurance do not independently establish combat damage or survival. |
| Jewel and doorway demonstration — 90–147, IDs 2402–2414 | Astral questions Galam about the tower. Galam displays the jewel in narration; white flashes, quakes, camera motion and working-map copies animate the doorway as effect actors enter. This stage grants no inventory item and proves no persistent doorway collision state. |
| Rising light and rescue attempt — 148–223, IDs 2415–2425 | Galam and Elis rise with the effects. Astral asks for Bowie's acrobatics; Astral and minister help stage Bowie's upward throw, and the camera follows him. Scripted displacement is separate from battle movement legality; waiting and non-waiting actor actions remain distinct. |
| Grasp, flare and fall — 224–305, IDs 2426–2439 | Narration describes Bowie grasping the jewel, its fusion to his neck and the jewels merging. Flashes, shivers, poses and falling motion surround the text. Command 305 sets F385 only after the final merge narration; no ordinary item-slot or ability/stat grant accompanies it. |
| Disappearance and aftermath — 306–337, IDs 2440–2447 | Effect actors mosaic out, Galam and Elis receive disappearance scripts, and sad music is requested. Bowie is restored/restarted and the group discusses the jewels. The apparent failed rescue is an Inferred narrative reading; actor disappearance establishes no death or force join. |
| Renewed quake and retreat — 338–396, IDs 2448–2460 | Quake/boss music calls accompany the decision to leave. Astral and minister stage carrying the king south; the camera follows the king. Astral's final warning precedes the return request, follows, persistent flag writes and script end. The carried retreat provides no intervening exploration control. |

Scene entities 128/129 begin as off-map effects, later become doorway effect sprites and are reused
within this population. Galam 130, Astral 131, Elis 132, king 133 and minister 134 name scene roles;
they do not identify surviving combatants or the next exploration population. Galam/Elis's
`eas_TwirlShrinkDisappear`, Bowie's restored sprite and Astral's restarted action script are actor
operations, not party membership or combat HP rules.

**Unknown:** completed natural waits, simultaneous movement, scroll/fade/audio delivery and total
scene duration. Command ranges are not elapsed frames. The final quake operand alone proves no
visible quake after loading. A supplied-map load rebuilds the working layout, so the scene's block
copies are not evidence of a saved layout; the
[map-load contract](../../../contracts/world/map-exploration.md#load-and-selection-order) retains the
rebuild/preservation distinction.

## 3. The Scene Writes Three Flags with Different Consumers

**Confirmed producer/consumer relations:** narration, current actors and persistent flags have
separate effects. `abcs_battle07` supplies no new recruit, gold transaction, ordinary item-slot
transaction, `setStoryFlag` or Battle08 unlock.

| Producer | Consequence and limit |
| --- | --- |
| F385, command 305 | `BuildMemberStatusWindow` / `@WriteJewelString` first tests F384 for Bowie. With that gate admitted, F385 adds the second jewel icon and plural suffix. F385 set with F384 clear displays neither jewel through this branch. This scene does not set F384 or grant an inventory item. |
| F74, command 394 | The follower table admits three rows: Astral `FOLLOWER_B`, king `FOLLOWER_A`, minister `FOLLOWER_D`. The source comment naming only Astral + King is incomplete. Those rows are selected during exploration reconstruction among any other enabled rows. |
| F640, command 395 | `SwitchMap` consumes the flag-switch table: a later Map3 request becomes Map4, the earthquake-town map. This flag does not substitute Map58. Its later clearing/lifecycle is outside this segment. |

The [persistent-effect review](../../../../research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md#persistent-effects-and-actual-return-consumer)
names the menu, follower and map-switch sources. Generic inventories do not executable-close the
scene-write-to-menu or follower-rebuild composition. The accepted source review supports these
conditional static relations; natural display, follower behavior and persistence remain Unknown.

## 4. The Return Request Precedes Script and Caller Completion

**Confirmed selected order:** the warp command prepares a request and returns to the interpreter.
It does not terminate the scene. The actual path to control is:

1. Command 390, `csc07_warp`, writes event type 1, parameter 1 zero and return parameters
   `(58,13,35,DOWN)`.
2. Commands 391–393 request current-scene Astral → Bowie, king → Astral, minister → king follows
   through `csc2C_followEntity` / `AddFollower` with relationship selector 2. Commands 394/395 set
   F74/F640; command 396 ends the script. The interpreter then performs its conditional scroll
   wait/cleanup, and the wrapper reaches `JoinForce(0)`.
3. `BattleLoop_Victory` clears F407 and sets F507, reads the return parameters and returns `d4=1`.
   F407 is admission state; F507 is completion/after-scene gating. Stopping the explanation at the
   warp would omit the follows, both persistent writes and caller completion.
4. `alt_MainLoopEntry` applies `SwitchMap`, then enters exploration directly. It does not call
   `CheckBattle` again at this outcome return. This consumer bypasses the ordinary exploration
   `ProcessMapEventType1_Warp` route.
5. `ExplorationLoop` immediately clears `MAP_EVENT_TYPE`, heals, fades, assigns the current map
   and `NOT_CURRENTLY_IN_BATTLE`, loads tilesets and initializes entities. Map58 has no setup row:
   `GetCurrentMapSetup` returns `ms_Void`, and the entity-list accessor resolves to its terminator.
   `InitializeMapEntities` clears the old scene population, selects flag-enabled followers and
   declares the main entity at supplied `(13,35)`, Down.
6. Exploration clears temporary flags 256–383, sets F80, loads the map and installs base VInt
   functions. `RunMapSetupInitFunction` skips its call for `ms_Void`; Map58 has no additional init
   scene. F385/F74/F640/F507 survive this temporary clear by lying outside its range.
7. After the conditional music/fade calls, `WaitForEvent` with no pending event follows entity 0
   and installs `SetControlledEntityActScript(0)`. BattleLoop's `PLAYER_TYPE=0` selects
   `eas_ControlledCharacter`. This is the first static control-restoration boundary, conditional
   on the preceding services completing.

The [request consumer](../../../../research/world/map-event-request-consumption.md#confirmed-consumer-topology)
owns the local reset/poll/clear seam; the research owner supplies the selected battle-return
composition. The [setup-data contract](../../../contracts/world/map-setup-data.md#contract-boundary) preserves
the entity-list identity, while the [map contract](../../../contracts/world/map-exploration.md) owns selector
and transition boundaries. Neither an inventory nor the controlled transition observations proves
this whole natural return.

Current-scene follows are not retained entity IDs on the next map. Clearing entities removes their
positions, travel, scripts and mappings, then reconstruction selects follower rows from flags.
This is separate from combat HP and the
[party/script contract](../../../contracts/party/party-roster-state.md#handler-boundary). F74 admits three rows,
not a guaranteed total follower count. An intro-skipped F83 can enable additional king/minister
rows alongside F74; the initializer does not deduplicate them. Other follower flags and conflicting
caller configurations remain explicit inputs, not repaired assumptions.

### F999 changes the effective destination

| Return input | Conditional destination/control boundary |
| --- | --- |
| First victory, F999 clear | Map58 `(13,35)`, Down; F407 clear and F507/F385/F74/F640 set. F83 is clear only if established by the executed intro or caller. Void setup/reconstructed followers precede the control installation above. |
| Repeat victory, F999 clear | Map58 at variable seeded position/facing; same void-setup control path, with jewel/earthquake/follower flags retained as inputs because the after scene was skipped. |
| Map58 requested, F999 set | `SwitchMap` substitutes Map59 with the same coordinates/facing. Default Map59 init with F999 can request another warp when F443 is set; the F543 setup variant enters a different scene. These late-campaign owners are outside scope, so no universal immediate-control claim covers this state. |

## 5. The Next Boundary Is a Separate Field Request

**Confirmed immediate outgoing record:** Map58's flag/step tables contain only terminators. Its
`6-warp-events.asm` record at `0xB8A26` is `(255,37)` → Map60 `(8,52)`, Right, no-scroll;
255 is the X wildcard. This later field request is distinct from the battle's return request.
The next owner is Map60's map-content/warp surface, identified in the
[accepted outgoing boundary](../../../../research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md#persistent-effects-and-actual-return-consumer).

**Unknown:** whether controlled Bowie can walk from `(13,35)` to that marker with the actual
layout, occupancy and followers. Arrival on Map60, castle/town evacuation and Battle08 need their
own accepted evidence before synthesis continues. The independent subsequent research is not a
dependency consumed by this guide.

Continue with [Post-tower return through earthquake-town entry](post-tower-return-through-earthquake-entry.md)
for the accepted conditional tower/castle requests, earthquake-town selection and first/repeat init
through its post-init control/event boundary.

## Evidence Review and Remaining Limits

The pre-synthesis review inspected the accepted #716 research, preceding guide and research,
relevant contracts, selected public fixture payloads, and verifier/schema boundaries below. No
fresh extraction, retained-output comparison, H2 rerun or runtime observation occurred. The research
owner retains the direct pinned-source/text review and reproduction receipts; this guide consumes
that accepted provenance without extending its observation frontier.

| Relation | Reviewed executable surface / reproduction route | Disposition |
| --- | --- | --- |
| Faction checks, healing and outcome writes | [loop fixture](../../../../../tests/fixtures/h2/battle-loop-static-v1.json), ID `sf2-battle-loop-static-v1`, `lifecycleFacts.remainingCombatants/betweenBattleHealing`; [control fixture](../../../../../tests/fixtures/h2/battle-control-static-v1.json), ID `sf2-battle-control-static-v1`, `controlFacts.mainLoop/outcomes`; [loop](../../../../../src/sf2tool/h2/battle_loop.py) and [control](../../../../../src/sf2tool/h2/battle_control.py) verifiers. `uv run sf2 h2 battle-loop`, `battle-control`. | Generic checks/heal/result facts agree; selected Battle07 routing, return seed and resource/egress composition come from research. No natural combat claim. |
| After dispatch/skip, defeated gates and shared join | [cutscene fixture](../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), ID `sf2-battle-cutscenes-static-v1`, `cutsceneFacts.intro/afterBattle/enemyDefeated/leaderDeathPositions`; [verifier](../../../../../src/sf2tool/h2/battle_cutscenes.py), [fixture schema](../../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json). `uv run sf2 h2 battle-cutscenes`. | Preserves shared-tail/guard order. Selected row 7, zero join/leader row and absent position override remain named source-review facts, not quantities established by these generic guards. |
| Scene flags, warp and actor/text boundaries | [script fixture](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json), ID `sf2-map-script-engine-static-v1`, `storyStateCommandFacts`, `transitionCommandFacts`, `dialogueCommandFacts`, `entityPopulationCommandFacts`, `entityGestureRelationshipMotionCommandFacts`; [verifier](../../../../../src/sf2tool/h2/map_script_engine.py). `uv run sf2 h2 map-script-engine`. | `abcs_battle07:305/394/395` flag sites, `:1/390` transitions, `:2` population and `:391–393` follow order agree. Complete phases/text and menu/follower consumers use accepted research; inventories do not prove natural services or end-to-end effects. |
| Reset, substitution, void setup and init | [request fixture](../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), ID `sf2-map-event-request-consumption-static-v1`; [maps fixture](../../../../../tests/fixtures/h2/common-maps-static-v1.json), ID `sf2-common-maps-static-v1`, `mapFacts.mapSwitch/egress`; [setup fixture](../../../../../tests/fixtures/h2/map-setup-static-v1.json), ID `sf2-map-setup-static-v1`, `sourceFacts.selector/dispatch`; [init fixture](../../../../../tests/fixtures/h2/map-init-static-v1.json), ID `sf2-map-init-static-v1`, `primarySourceBodies`; [request](../../../../../src/sf2tool/h2/map_event_request_consumption.py), [maps](../../../../../src/sf2tool/h2/maps.py), [setup](../../../../../src/sf2tool/h2/map_setup.py) and [init](../../../../../src/sf2tool/h2/map_init.py) verifiers. `uv run sf2 h2 map-event-request-consumption`, `common-maps`, `map-setup`, `map-init`. | Local seams agree with the stricter conditional composition. The setup-data contract does not consume selector/dispatch behavior; those fields remain with map exploration. No rail closes the whole scene → caller → void setup → control chain. |

The controller overview's broader before/start grouping does not replace the preceding guide's
precise selected order. The follower comment omits the minister; use the three table rows, and keep
F83/conflicting flags visible. Historical H3 question queues and suggestions to extend existing
owners are Unknown dispositions, not an authorized acquisition queue. These stricter boundaries
agree with the accepted owners; no Layer A correction or new fixture is introduced here.

**Inferred:** the intended narrative connection from the confrontation through failed rescue and
retreat. **Unknown:** natural victory/defeat/escape and incoming savepoint/flags/HP/party/actor state;
completed waits, scrolling, audio, presentation and duration; conflicting followers and reentry;
legal walking, occupancy/collision, onward route, persistence and winnability. Their
[gap owner](../../../../research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md#coverage-gaps-and-reproduction)
retains discriminating controls such as F384-clear/F385-set, F507 skip, F83+F74, and F999 substitution.

Named commands are reproduction routes, not fresh PASS claims. Load the ignored
`local/private-inputs.ps1` in the launching PowerShell process. The research owner's
[reproduction section](../../../../research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md#coverage-gaps-and-reproduction)
links the retained-payload recipe and exact selected source/program/text anchors. Documentation
acceptance uses direct prose/branch/link/private/scope checks, `git diff --check`, a clean committed
verification plan and actual CI, without normal/full/engine/H3/H4/Godot or translation suites.
