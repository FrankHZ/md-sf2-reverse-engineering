# Battle18 Outcomes and Caravan Acquisition

A first completion of the Taros battle gives Rohde a scene in which he examines the small Caravan,
appears to enter it, and explains why its size is misleading. Peter challenges his account; Rohde
presents himself as its driver, and narration says he accompanies the force. The actual terminal
changes are F65 set and F84 clear. Battle completion, battle membership, rebuilt exploration actors
and the current transport controller each have their own consumers.

**Confirmed:** the accepted [research owner](../../research/battle18-outcomes-and-caravan-acquisition.md)
establishes the conditional outcome paths, complete first-victory program, selected mutations and
immediate returns. **Inferred:** its dialogue and staging present Rohde as the Caravan's driver and
miniaturization as the explanation for entering it. **Unknown:** natural outcomes, actual supplied
state, rendered disappearance/return, completed services, usable control and persistence.

Read [Taros's warning and Battle18 entry](southwest-parmecia-through-taros-battle18-entry.md) for the
preceding conditional admission, and [Rohde's proposal](hassan-ancestry-and-rohde-caravan-choice.md)
for his separate acceptance/following writes. Neither supplies a natural winning route. This guide
stops at the immediate conditional exploration/control boundary or an unrelated battle admission.
Sword damage, combat simulation, Caravan operation/menus/storage, Rick and later travel are excluded.
The separate [optional Rick recruitment guide](hassan-rick-recruitment.md) explains conditional Hassan
population and current membership/following; it establishes no mandatory shrine-to-Hassan return.
Fnnn means flag nnn; coordinates are map tiles and indices are zero-based. Waits and speeds below
are source operands, not measured time. Dialogue is paraphrased.

## What Ends the Battle

**Confirmed:** Battle18's defeated route is the empty `ms_Empty`, but its leader row is -1 and the
shared leader tail still does work. At the post-turn defeated hook, Bowie must have nonzero HP and
enemy128 (Taros) must have zero HP. F518 clear dispatches the empty script; F518 set skips dispatch.
Both eligible branches append every nonzero-HP enemy among the 32 slots to the killed list, preserving
its trailing -1. The tail does not test placement or directly zero those enemies' HP.

The killed-list consumer then runs death presentation/bookkeeping, writes X/Y=-1, clears status,
rebuilds stats and moves mapped entities off-map. The following count admits only nonnegative-X,
nonzero-HP combatants, and forces the ally count to zero if Bowie has zero HP. Zero allies is tested
before zero enemies. Thus Taros death can remove remaining enemies through cleanup, but does not
override defeat when Bowie also dies. If battle continues, after-turn effects receive a second
killed-list/count/defeat-before-victory checkpoint, without a second defeated-hook call there.
This explains an outcome boundary, not how the Sword damages Taros or which combat trajectory wins.

| Selected outcome | Consequence and return |
| --- | --- |
| Victory with F518 clear | Heal eligible allies; seed return; run the whole acquisition program; run the shared Bowie join; clear F418 and set F518; return d4=1. |
| Victory with F518 set | Heal and seed return; skip the acquisition program; still run the Bowie join; clear F418 and retain/set F518; return d4=1. F65/F84 are left as supplied. |
| Defeat | Update unlock, request exhaustion text/music, restore Bowie's HP, halve gold by unsigned shift and select the supplied savepoint; return d4=-1. Battle4's special loss does not apply. |
| Admitted Egress or Angel Wing | Consume MP or remove the selected item slot, perform the owned text/flash/closure and unlock update, unwind the action frame and select the supplied savepoint; return d4=0. No victory scene/join/completion or defeat gold penalty follows. |

Escape availability, item custody and MP sufficiency are not established by this selected dispatch.
Defeat/Egress leave F518 and F468 unchanged. Their unlock update leaves F418 unchanged when F518 is
clear and clears it when F518 is set. They do not acquire the Caravan.

## Return Position and the Scene's New Cast

**Confirmed:** victory heals living allies and immortal Peter/Lemon, retaining the supported status
bits; other dead allies are not revived. Before the scene, it rebuilds the current flag-selected
active list and seeds the return with CURRENT_MAP, the first member's combatant X/Y plus the battle
area origin using byte arithmetic, and that member's mapped entity facing; parameter1 is zero.
For consistent Battle18 state this is Map12 with origin `(0,0)`. The chosen member need not be Bowie,
the first living ally or an initial placement. Empty/inconsistent lists and lookups gain no repair.

The seed precedes both scene rebuilding and the shared join. No selected scene command or immediate
action body writes this tuple, CURRENT_BATTLE, PLAYER_TYPE or the saved raft tuple. The dramatic
positions below therefore do not become the return position.

The after wrapper tests only F518. Clear loads Map12 with camera `(7,5)` and `ce_4B6BE` at `0x4B6BE`:
main0's three-word header gives `(12,9)`, Up; the sole subsequent row gives Caravan `(12,5)`, Down,
sprite62, `eas_Init`. The header is not an NPC, and this load does not run exploration init.
Actor/lookups are rebuilt, current flag-selected followers precede the fixed row, and main0 is
created separately. This scene contains no Taros row or newly declared combatant.

| Caravan identity | Owning population |
| --- | --- |
| Before-scene logical129 | Earlier confrontation's Caravan, Down; replaced by battle rebuilding. |
| Battle neutral159 | F518-clear combat rebuild's separate Left/HP0/`eas_Motionless` Caravan; not an acquisition result. |
| After-scene logical128 | This new population's sole fixed row, Down; reusing128 does not mutate Taros combatant128. |
| Raw physical1 in the shrink body | Direct ENTITY_DATA operand; allocation decides its actor. It is not a logical128 lookup. |

Map12 uses the normal follower table even when F65 is independently set. F67 declares Peter, F84
Rohde and F68 Oddler/FOLLOWER_B31; other supplied flags retain their ordered effects, including
Astral flags74/70 overwriting FOLLOWER_B. The scene does not declare or check these speakers itself.
With only F67/F68/F84 set, physical1 is Peter,2 FOLLOWER_B,3 Rohde and4 Caravan128. This is conditional
allocation, not a natural-party assertion. Missing mappings default to0 and may alias main; invalidated
255 is different. Neither capacity safety nor conflicting identities are repaired.

## Rohde's Demonstration and Peter's Response

**Confirmed ordered program:** `abcs_battle18` at `0x4B2F2` contains 212 commands, 16 inline movement
bodies, four custom action bodies and 15 text sites. The phases below cover its complete story,
action and camera meaning. A started action installs work without waiting; a waited action polls
only that mapped actor's idle pointer. Neither form establishes all-actor or presentation completion.

### Inspecting the Small Caravan (Commands 0-40)

The program initializes/waits Bowie, Peter, FOLLOWER_B and Rohde; places Peter `(13,9)` Up,
FOLLOWER_B `(14,9)` Up and Rohde `(5,9)` Right; stops Caravan animation, fades in and waits60.
Rohde compliments the named leader's strength (2528). Bowie/Peter/FOLLOWER_B turn Left, wait30;
the camera follows Rohde, wait120; Rohde moves Right6 then Up4 with actor-specific waits, while
the other three turn Up. Rohde then faces Right.

Tracking is disabled and the camera moves to `(7,1)`, followed by wait30. Six positions present
Rohde inspecting different sides: `(12,6)` Up, `(13,5)` Left, `(12,6)` Up, `(11,5)` Right,
`(12,6)` Up, `(13,5)` Left, each followed by wait20. **Inferred:** this staging directs attention
to his inspection rather than establishing a player-controlled approach.

### Entry, Vehicle Motion and Disappearance (Commands 41-83)

Rohde remarks on the small size and attempts entry (2529). Start Left1, wait3, then start
`eas_452BA`, wait3 then60. Caravan animation starts, waits30, stops, waits60, starts and waits120.
It turns Left/Up/Right/Down twice, with wait10 between facings except the final Down, then starts
Jump. After wait10, Rohde is positioned `(63,63)` Down. A waited custom Caravan body sets speed
48/48 and idles; falling SFX is submitted, Caravan starts Down9 and the script waits20.

A waited custom Bowie body sets speed64/64 and idles. After waiting for his auto-facing-off body,
Bowie starts Left1 and faces Right while Peter/FOLLOWER_B face Left. Wait60; camera destination
`(7,4)`; wait Bowie Init. **Inferred:** this sequence stages an apparent disappearance and a moving
Caravan for the witnesses' reaction. The source requests do not prove the rendered effect or that
Rohde's shrinking cloned the scene Caravan; the physical operand is explained below.

### Waiting and Looking for Rohde (Commands 84-139)

Bowie/Peter/FOLLOWER_B face Down. Peter reacts to the disappearance (2530), wait60. Bowie faces
Right and the other two Left; Peter hopes Rohde returns and questions his honesty (2531), wait30.
All three face Down, wait150. Wait Caravan auto-facing off, start Up9, submit falling SFX, wait20;
Caravan/FOLLOWER_B face Left, wait20. Bowie/Peter cycle through Up/Up, Left/Right, Down/Down,
Right/Left twice, with wait10 after each pair.

Wait custom Bowie then Peter bodies: motion OFF, orientLeft, sprite update, jump idle. Stop their
animation, wait30; Caravan faces Down, wait60; stop Caravan animation; wait FOLLOWER_B BumpLeft
twice, wait30. **Inferred:** the looks and bumps punctuate uncertainty while Rohde is away. These
animation changes neither remove combat membership nor establish that earlier travel has finished.

### Returning and Explaining the Ancient Device (Commands 140-170)

Place Rohde `(12,5)` Right, wait `eas_4536C`, then wait3. Wait Bowie Init then Peter Init; face the
three witnesses Up and restart Bowie/Peter animation. Wait Rohde Down3. Rohde denies lying (2532),
moves Left2 with a wait, faces Down and praises the Ancients (2533). Wait Right1; face Up; he explains
miniaturized people and items as the reason the Caravan can be small (2534). Wait Up2; face Down;
he claims he can drive and asks to accompany them as driver (2535).

After wait30, narrator text says the named historian accompanies the force (2536). This is the sole
packed -1/ENTITY_NONE speaker. Peter moves Up1 with a wait and expresses disbelief (2537), moves
Down1 with a wait, then faces Left while Bowie faces Right and FOLLOWER_B Left. Peter proposes
bringing many soldiers (2538); wait Rohde Down1. There is no Yes/No prompt or membership test here.

### Disagreement, Final Entry and Terminal Writes (Commands 171-211)

The three witnesses face Up. Rohde says soldiers miniaturized inside cannot enter battle (2539);
Peter reserves judgment (2540). Bowie faces Right, Peter/FOLLOWER_B Left; Peter expresses dislike
(2541). Wait Rohde Up1, face Down, wait20, wait his Jump twice. The witnesses face Up; Rohde
anticipates adventures with the Caravan (2542), wait30.

Start Rohde Up1, wait3, start `eas_452BA`, wait3 then60. Start Caravan animation, place Rohde
`(63,63)` Down, wait60, then wait Caravan Init. Start Caravan Down9, wait30. Bowie faces Right,
Peter/FOLLOWER_B Left, wait10; all three face Down, wait180. Finally set F65, clear F84 and end.
**Inferred:** the second entry and departing-vehicle staging close Rohde's driver introduction.
The final writes establish only their specific story/population predicates.

All 15 messages use `nextSingleText` and keys `09E0`-`09EE`; actor modifiers are `$0`, apart from
the narrator's `$FF`. LEADER selects the first flag-rebuilt active member's name; NAME;11 selects
Rohde's name. W1 acknowledgements and the W2 inside2534 are text controls, not extra scene choices.
The next Oddler scene is outside this program. Driver, miniaturization and soldiers' battle access
are attributed speech, not verified transport/storage rules or a Rohde combat recruitment.

## Shrinking, Movement and Service Limits

**Confirmed:** both `eas_452BA` installs enter after the preceding `eas_452B2` movement instruction;
they do not issue its move-to-Caravan request. The entered body waits6, enables resizing, submits
warp SFX, refreshes sizes22/20/18/16/14/12 with wait1 between successive sizes, waits for the existing
destination, clones raw physical `ENTITY_CARAVAN=1`, then idles. The clone consumer indexes ENTITY_DATA
directly, copying physical1's current/destination coordinates, travel and facing, then refreshing
Rohde's sprite. Under the F67/F68/F84-only allocation, that source is Peter, while Caravan128 is
physical4. With no admitted followers Caravan can occupy1, but Rohde is absent. Other supplied flags
change allocation. This relationship establishes no observed visual bug, and the symbolic operand
must not be renamed into a logical Caravan target.

The return install enters `eas_4536C` after `byte_45368`'s clone instruction. It requests one tile
relative to Rohde's current facing, enables resizing, submits warp SFX, refreshes sizes12/14/16/18/20/22
with wait1 after each, disables resizing, refreshes size16, waits for destination and idles. Given
the preceding effective `(12,5)`, Right placement it requests `(13,5)`; it is not inherently a downward
exit or a clone of logical128. Neither action body calls the enclosing routines that change transport
mode. A reused vehicle-action label does not establish boarding or disembarking.

Inline moves scale offsets by384 and pass through destination-conflict/travel setup before waiting
for destination equality and idling. Jump requests Up1/wait5, Down1/wait5, then zero offset and restores
auto-facing; BumpLeft uses Left1/Right1/zero with the same waits. Caravan's started Jump can be replaced
by the later custom speed body without a separate completion join. Started Down9/Up9 moves and the
custom witness/Init installs similarly do not prove previous movement finished. Speed48/48 and64/64
are paired X/Y speed bytes, not durations.

`ac_motion OFF` and start/stop-animation write animation state, not vehicle-control state or motion
cancellation. `orientLeft` selects orientation1, distinct from Left facing2. Init restores speed32/32,
acceleration/collision/auto-facing/orientation/immersion and size24 before sprite refresh and idle;
waiting for Init is not waiting for an old destination. `setPos` writes current/destination coordinates
and refreshes without canceling installed action. Rohde's `(63,63)` relocation neither deletes him nor
changes HP/membership; unfinished action can subsequently write his position again.

Logical lookup lacks a presence guard. Placement/facing and start/stop-animation skip zero-HP allies
below30; FOLLOWER_B31 and Caravan128 bypass that ally guard. Action installs/custom bodies do not use
it. Victory healing can make Peter live without declaring him, and does not ensure dead Rohde is
healed. Actor absence, HP and physical allocation are independent dependencies.

Text waits for scrolling, uses mapped portrait/speech properties, advances/closes text and sleeps10;
packed -1 narration bypasses actor portrait selection. Acknowledgements need their own input/VInt/RNG
services. The accepted [text/service observation](../../research/map3-messenger-acceptance.md) does not
observe these 15 messages. Camera destinations disable tracking and wait for scrolling; following
Rohde uses his mapped physical identity, and neutral `$FFFF` disables tracking. `fadeInB` uses the
shared half-in-from-black service. Ordinary waits use low-byte VInt sleeps; debug/player2-Start skip
can omit Sleep and skip text display/cursor advancement. Sound submission, text, fade and camera waits
do not join all actors. End conditionally waits for scrolling with a dialogue window, clears camera
speed and restores registers, without joining outstanding actions. Rendered completion remains Unknown.

## Acquisition Flags and the Shared Bowie Join

**Confirmed:** terminal setF65/clearF84 neither sets F725 nor joins Rohde nor writes PLAYER_TYPE,
F64 or saved RAFT_MAP/X/Y. Clearing F84 changes subsequent population selection; it does not delete
the staged Rohde immediately. The scene still runs if F65 was already set or F84 was absent, provided
F518 is clear. With F518 set, repeat victory skips all scene writes, leaving inconsistent F65-clear
or F84-set state untouched. F468's intro gate does not gate this after scene.

Both wrapper paths reach `EndAfterBattleCutscene`. Slot18's join value0 resolves through `j_JoinForce`
to Bowie's joined F0 write, force-list rebuild and conditional active-party join when the count is
below12. The source table's unused comment does not remove this executed tail. It is Bowie, not Rohde,
and occurs after the return member has been selected. Only then does victory clear F418, set F518,
read the saved return parameters and return d4=1. F468 is retained.

**Inferred story meaning:** Rohde's spoken driver proposal, narrator accompaniment and the replacement
of his ordinary F84 following with F65-selected overworld content describe the acquisition handoff.
**Confirmed mutation boundary:** none alone grants a Rohde combatant, current Caravan mode or an
operational inventory/menu/storage capability.

## Immediate Victory Return: Shrine Control or Warning and Retreat

**Confirmed:** consistent victory returns to Map12 at the pre-scene seeded position/facing, rather
than main's staged `(12,9)` or Caravan's final position. MainLoop resumes at `alt_MainLoopEntry`, then
SwitchMap and ExplorationLoop, without CheckBattle. The complete switched-map table has no source12
row. An independently supplied unlock is not scanned at this direct outcome return; a later ordinary
request has its own admission boundary.

Fresh supplied-map exploration clears pending, reduces the word STEP_COUNTER by20000 with a negative
clamp, heals eligible allies, fades/loads tiles, writes map12/battle255, rebuilds entities, clears
temporary flags256-383, sets F80, loads the map, installs base VInts and runs init. The after-scene
cast and unfinished animation are replaced. Map12's sole fixed row declares Taros128 `(12,8)` Down,
special physical47; no Caravan row survives here. Map12 is not an overworld, so F65 does not add its
Caravan follower. First acquisition's F84 clear omits Rohde; the other supplied follower flags remain.

Init tests F725 before F518. Clear executes the complete accepted warning `cs_57D22`; set skips it.
Only afterward does F518 move Taros off-map. Victory has just set F518, but completion alone does not
suppress the warning, and neither acquisition nor init repairs F725.

| Supplied F725 | Immediate continuation and stop |
| --- | --- |
| Set | Skip warning, move Taros out, perform conditional music/fade. With no new pending event, WaitForEvent follows physical0 and installs current-mode control; normally battle entry left mode0. Usable input is unobserved. |
| Clear | Run the warning before moving Taros out. Its no-scroll request targets Map68 `(55,44)`, Down. After init/music/fade, consume pending without an intervening ordinary-control installation. |

The [complete warning guide](southwest-parmecia-through-taros-battle18-entry.md#warning-and-retreat-request)
explains the assembled approach, repeated reactions, discovery/threat, Peter's request to leave and
retreat actions/dialogue. Its final actor-specific wait does not join all earlier actions. The warp
writes type1/no-scroll parameters but no WARP_SFX; the consumer clears pending, submits/clears the
supplied sound, idles main0 and returns `(68,55,44,Down,d4=0)` to MainLoop's normal battle scan.
There is no guaranteed playable pause in the shrine, and replaced pending state is not an atomic queue.

Map68's sole battle row is19. Supplied F419 admits it; F519 can clear unlock after selection without
canceling that call. Stop at this unrelated admission. Otherwise fresh Map68 exploration has an empty
fixed setup and return-only init. F65 selects the overworld table's Caravan FOLLOWER_A at physical1
before normal followers; later conflicting FOLLOWER_A flags can overwrite its logical mapping.
First acquisition cleared F84; repeated victory retains it as supplied. Absent another pending event,
WaitForEvent reaches the usual conditional control boundary. Declaration does not choose mode1.

Victory and warning preserve F64 and saved raft state. In mode0 a separate physical31/lookup63 raft
is declared only when F64 is set and the saved map matches; mode2 separately changes main's sprite
and hides followers. Neither acquisition nor warning produces mode2. Transport mode, follower
declaration, saved raft position and actual operation remain distinct. No boarding or menu follows here.

## Defeat and Egress: The Supplied Savepoint

**Confirmed:** Battle17 has the hardcoded Hassan case in `GetEgressPositionForBattle`; Battle18 falls
through to EGRESS_MAP and `GetSavepointForMap`. Normal battle entry set F399. A matching savepoint row
supplies X/Y/facing; no match retains the supplied map byte with default `(1,1)`, Up. This is not a
validated arbitrary-map promise. An abnormal F399-clear direct call returns the initial Bowie's-room
tuple before table/raft selection.

F64 clear preserves saved raft coordinates. With F64 set, savepoint selection writes a reset tuple:

| Selected savepoint map | Saved raft tuple |
| --- | --- |
| Hassan9 | Map69 `(28,55)` |
| Polca13 or Bedoe10 | Map72 `(43,48)` |
| Any other table selection | Sentinel default Map69 `(42,15)` |

These are saved-position writes, not boarding, mode normalization or immediate placement on the
return map. For supplied EGRESS_MAP9, the return is Hassan `(8,9)`, Up. Its accepted
[population/init guide](battle16-outcomes-through-hassan-entry.md#hassans-cast-and-conditional-control)
retains default/F518 alternate cast, normal followers, shared init and F65-dependent Rohde movement
out. First loss leaves F518 clear; repeated loss retains it. Neither sets F65 or clears F84. A reset
raft on69 is not placed in town9. After init/services, absent pending work permits conditional control.
No Rick interaction or investigation is implied by this accepted init.

Direct defeat/Egress returns also bypass CheckBattle. Other supplied maps have their own SwitchMap,
setup/init and pending behavior; their continuations are Unknown here, rather than assumed equivalent
to Hassan. Actual EGRESS_MAP remains supplied state, not a forced town or natural loss itinerary.
Inconsistent map255 uses the existing-population branch instead of a fresh rebuild. Stop at the
immediate conditional return/control boundary; no all-map init investigation is admitted.

## Evidence Trace and Review Boundary

**Confirmed provenance:** accepted #844/PR846 binds SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [research reproduction owner](../../research/battle18-outcomes-and-caravan-acquisition.md#reproduction-acceptance-and-deferred-evidence)
retains the actual slot18 pointer/leader/join rows, complete scene/entity header, text keys,
immediate action bodies and caller/allocation/savepoint joins. These accepted joins supply the
selected quantities and behavior below; this synthesis performs no new source investigation.

| Explained boundary | Contract and exact fixture | Limit of that executable owner |
| --- | --- | --- |
| Leader cleanup, healing and outcome | [battle lifecycle](../contracts/battle-control-lifecycle.md), [cutscene routing](../contracts/battle-cutscene-routing.md); `sf2-battle-loop-static-v1` in [loop](../../../tests/fixtures/h2/battle-loop-static-v1.json), `sf2-battle-cutscenes-static-v1` in [hooks](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), `sf2-battle-control-static-v1` in [control](../../../tests/fixtures/h2/battle-control-static-v1.json) | `expected.lifecycleFacts` separates killed-list placement writes from `killRemainingEnemies`' HP writes; `cutsceneFacts` gates leader cleanup and shared after join. Actual slot18 and defeat-before-victory caller joins remain research-owned. No Battle18 turns were observed. |
| Slot18 wrapper and join argument | [cutscene routing](../contracts/battle-cutscene-routing.md); `sf2-battle-routing-data-static-v1` in [routing](../../../tests/fixtures/h2/battle-routing-data-static-v1.json) | `expected.facts.routeTargetsParsed=false`; zero-filled join-table facts do not replace the selected dispatch/caller. Its unused label is not an execution veto. |
| Scene population, actions, text, camera and flags | [map exploration](../contracts/map-exploration.md), [dialogue](../contracts/dialogue-system.md), [roster](../contracts/party-roster-state.md); `sf2-map-script-engine-static-v1` in [script](../../../tests/fixtures/h2/map-script-engine-static-v1.json) | `expected.entityPopulationCommandFacts`, `entityPlacementCommandFacts`, `entityActionBridgeCommandFacts`, `entityLifecyclePresentationCommandFacts`, `entityCloneCommandFacts`, `mapCameraControlCommandFacts`, `dialogueCommandFacts` and `storyStateCommandFacts` retain handler/site/order quantities. These do not execute Rohde's raw-physical shrink body or compose scene completion. |
| Bowie joining versus Rohde following | [membership](../contracts/party-membership-state.md), [roster](../contracts/party-roster-state.md); `sf2-common-stats-static-v1` in [stats](../../../tests/fixtures/h2/common-stats-static-v1.json) | Shared list/join facts retain their own numeric/capacity/normal-caller limits. F0, row18 value0, threshold12 and ordered seed/join consequences use the accepted selected research, not a blanket membership inference. |
| Fresh shrine init and warning | [map exploration](../contracts/map-exploration.md); `sf2-map-init-static-v1` in [init](../../../tests/fixtures/h2/map-init-static-v1.json) | `expected.embeddedPrograms` selects `cs_57D22`; its 67 operation records include inline action tokens, distinct from the research's 51 scene commands. F725-before-F518 and supplied return cast use the accepted init joins. |
| Direct outcome versus ordinary request | [exploration flow](../contracts/exploration-control-flow.md), [entry routing](../contracts/map-entry-routing-state.md); `sf2-gameflow-core-static-v1` in [gameflow](../../../tests/fixtures/h2/gameflow-core-static-v1.json), `sf2-common-maps-static-v1` in [maps](../../../tests/fixtures/h2/common-maps-static-v1.json), `sf2-map-event-request-consumption-static-v1` in [request](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json) | Shared return/event precedence, map selection and `eventRequestConsumption` are distinct from actual Map68 F419 admission, supplied savepoint, actor/controller and init/service state. No composed natural route is supplied. |

Bounded tracked-fixture selections confirm `abcs_battle18`'s population load at command2,
setF65/clearF84 at209/210, and 15 `nextSingleText` sites plus one text cursor. The dialogue group's
commandCount16 is not the whole program's212. Init's inline-token count likewise does not redefine
the scene-command count. The complete movement/custom-body/dialogue explanation consumes the stricter
accepted research program review rather than extrapolating these compact aggregates.

Schema/consumer pairs are [loop schema](../../../schemas/h2-battle-loop-static-fixture.schema.json)/[battle_loop.py](../../../src/sf2tool/h2/battle_loop.py),
[hook schema](../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)/[battle_cutscenes.py](../../../src/sf2tool/h2/battle_cutscenes.py),
[control schema](../../../schemas/h2-battle-control-static-fixture.schema.json)/[battle_control.py](../../../src/sf2tool/h2/battle_control.py),
[routing schema](../../../schemas/h2-battle-routing-data-static-fixture.schema.json)/[battle_routing_data.py](../../../src/sf2tool/h2/battle_routing_data.py),
[script schema](../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../src/sf2tool/h2/map_script_engine.py)
with [component ownership](../../../src/sf2tool/h2/map_script_schema.py),
[stats schema](../../../schemas/h2/common-stats-fixture.schema.json)/[stats.py](../../../src/sf2tool/h2/stats.py),
[init schema](../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../src/sf2tool/h2/map_init.py),
[gameflow schema](../../../schemas/h2-gameflow-core-static-fixture.schema.json)/[gameflow.py](../../../src/sf2tool/h2/gameflow.py),
[maps schema](../../../schemas/h2-common-maps-static-fixture.schema.json)/[maps.py](../../../src/sf2tool/h2/maps.py)
and [request schema](../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../src/sf2tool/h2/map_event_request_consumption.py).
Their existing commands are `uv run sf2 h2` with `battle-loop`, `battle-cutscenes`, `battle-control`,
`battle-routing-data`, `map-script-engine`, `common-stats`, `map-init`, `gameflow-core`, `common-maps`
and `map-event-request-consumption` respectively. Controlled reproduction loads the ignored
worktree configuration in the same process. These commands name ownership, not new runs for prose.

The pre-synthesis review checks accepted research, these contract boundaries and selected fixture/
schema/consumer fields for independent flags, HP/list/lookup dependencies, command versus payload
units, asynchronous overwrite, raw physical identity and return order. No Layer A repair is required:
its stricter selected joins control over compact summaries. Bowie/Taros HP combinations, the second
outcome checkpoint, independent F518/F468/F65/F84/F725, dead/absent Rohde, conflicting followers,
physical1 variants, empty active lists, overwritten/skipped actions, EGRESS_MAP/F399/F64/mode variants
and replaced requests are discriminating review cases, not an executed runtime matrix.

The [PR846 final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/846#issuecomment-6161451537)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/846#issuecomment-6161473505)
preserve its original/rebased identities and planner receipts, retained comparisons, corrected/failed
inspections and final CI. They retain [PR842's final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161224116)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161232331)
with predecessor results. Accepted comparisons are not replayed solely for prose; no fixture,
golden or schema is weakened, and aggregate facts do not supply composed runtime coverage.

**Unknown:** natural reach/outcomes; actual flags, HP, roster/lookups, return seed, savepoint, transport/
raft and pending/input state; all-actor and sprite/text/fade/camera/audio completion; rendered shrinking,
disappearance and return; usable input and save/resume persistence. Non-Hassan supplied-map init
continuations remain Unknown within this boundary. These stay deferred under
[ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) and
[ADR0016](../../decisions/0016-remake-start-evidence-deferral.md), without automatic acquisition or
an operational/menu/storage/Rick/later-travel claim.
