# Battle20 Outcomes and Elric Recruitment

After a first Battle20 victory, Elric introduces himself as an archer, thanks the group for saving
him and offers to help. Peter credits Oddler and welcomes their new friend. The scene explicitly
joins Elric to the force, then briefly makes him follow the group. Returning to ordinary Harpy
Pond exploration rebuilds the actors: Elric's membership remains, but that temporary follower
does not. A repeated victory skips his entire scene and does not repair missing membership.

**Confirmed:** accepted static programs and their consumers establish the conditional order below.
**Inferred:** the dialogue presents a rescue and a voluntary offer of help. **Unknown:** natural
combat/outcome/caller state, completed services, rendered presentation, responsive control and save
persistence. Character statements remain attributed speech rather than independently proved history.

This guide continues [Harpy Pond approach through Battle20 entry](harpy-pond-approach-through-battle20-entry.md)
using accepted [Battle20 outcome/recruitment research](../../research/battle20-outcomes-and-elric-recruitment.md).
It stops at immediate conditional Map51 control or the selected unrelated savepoint continuation.
Later pond departure, F751 passage, Fairy Cave/Creed travel and Battle21 are outside its scope.
Fnnn denotes flag nnn; records and slots are zero-based. Coordinates and waits are source operands,
not observed positions, elapsed time or proof of a naturally reached scenario.

## How the Battle Ends

**Confirmed:** Battle20's defeated hook selects the terminator-only `ms_Empty`, its enemy-leader
row is 0 and the complete region table has no Battle20 scene. The defeated wrapper still checks
Bowie alive, enemy slot128 dead and F520 clear before dispatching its empty program. Its common
tail does not queue the other enemies for death. Killing slot128 alone is therefore no victory
shortcut. An absent region scene does not describe the whole AI activation algorithm.

After an individual turn, BattleLoop runs that defeated hook, processes the killed list and counts
remaining combatants. Zero allies selects defeat **before** zero enemies selects victory. If neither
ends the battle, it processes after-turn effects and repeats the killed-list/count checks in the
same order, without invoking the defeated hook a second time. The counts scan 30 ally and 32 enemy
slots, admitting only nonnegative X and nonzero HP; Bowie's zero HP forces the ally count to zero.
Simultaneous zero counts mean defeat. The separate neutral Elric159 has HP0 and contributes no
living-enemy count merely by having an actor.

Killed-list cleanup performs its bookkeeping/presentation, writes X/Y=-1, clears status, refreshes
stats and moves mapped actors off-map. Battle20 supplies no leader-added victims or special
post-leader position rule; that separate position table selects only Battle05.

| Outcome | Ordered consequence |
| --- | --- |
| First victory, F520 clear | Heal eligible allies; save a return tuple; run Elric's complete scene; join Bowie through the common after tail; clear F420; set F520; return the saved tuple with d4=1. |
| Repeated victory, F520 set | Perform the same healing, seed, common Bowie join and completion writes, but skip the Elric scene, join13 and all its follow calls. |
| Defeat | Update the unlock flag; exhaustion music/text; restore Bowie's HP; halve gold; select the savepoint; return d4=-1. Battle04's special losable branch does not apply. |
| Admitted Egress / Angel Wing | Consume selected spell MP / item slot; text/flash/closure; update unlock; unwind the action frame; select the savepoint; return d4=0. No victory recruitment/completion or defeat gold penalty. |

On loss or escape, the unlock helper preserves F420 while F520 is clear and clears it when F520
is set. Neither path sets F520, resets F470 or joins Elric. The admitted escape consumer proves
neither menu availability, MP sufficiency nor item custody.

## The Return Position Is Saved Before the Scene

**Confirmed:** victory first heals all eligible ally slots: living allies plus Peter7/Lemon28 even
at HP0, restoring HP/MP to maxima and retaining only STUN/POISON/CURSE before stat refresh. It does
not require joined membership. Dead Elric is skipped; this is a lifecycle service, not recruitment.

Next, victory saves CURRENT_MAP and the first current active member's combatant X/Y plus battle-area
offsets using byte arithmetic, together with that member's mapped facing. The helper rebuilds the
force lists and takes their first active entry; it neither searches for a living member nor repairs
an empty/inconsistent entry. Under consistent Battle20 admission, map51 has area offsets `(0,0)`.
The earlier approach warp `(0,0)`, battle starting rows and later scene placements are different
inputs and must not replace this saved tuple.

Only then does the after wrapper test F520. The scene loads Map51 and places its main actor at
`(6,10)`, Up, but writes no return parameters, warp, CURRENT_BATTLE clear or mode change. Its load
does not enter ExplorationLoop. Completion writes and the tuple return wait for full script return.

## Elric's Complete Recruitment Scene

**Confirmed:** `abcs_battle20` at `0x4B6CE` contains 36 top-level records and one complete inline
move. The following account includes every selected record and dialogue key, paraphrased from the
[research-owned program and literal-text joins](../../research/battle20-outcomes-and-elric-recruitment.md#complete-selected-elric-program-and-replacement-population).
B means logical FOLLOWER_B31. Its identity depends on supplied population; the script does not
independently check that it depicts Oddler.

| Records | Dialogue and action order |
| --- | --- |
| 0–2 | Set cursor2566; load/fade Map51 with camera `(2,6)`; replace population with ce_4B780: main `(6,10)`, Up, and fixed Elric13 `(6,8)`, Down, Init. |
| 3–8 | Install and wait for Bowie, Peter and B Init; place Peter `(5,10)`, Up and B `(7,10)`, Up; fade in from black. |
| 9–12 | Elric introduces himself as an archer and says he did not know the pond was dangerous (2566), then thanks them for saving him (2567). Peter credits Oddler (2568), saying he heard the cry and brought them (2569). |
| 13–19 | Peter faces Right; wait5; start B's Right/Left/Right/Left action; wait120; B faces Up and modestly dismisses the thanks (2570); Peter faces Up. |
| 20–24 | Elric says an elf repays a favor (2571) and offers to help (2572). Peter faces Right, Bowie Left, and Peter tells the leader they have another friend (2573). |
| 25–29 | Elric moves Down1 and waits for his action to idle; he thanks them (2574). Bowie faces Up and nods; the explicit join13 service announces Elric and his current class (generic text446). |
| 30–35 | Peter faces Up and proposes seeing Mr. Creed (2575). Peter follows Bowie, B follows Peter, Elric follows B, all relative2; end. |

The map load disables entity camera following and performs tileset/fade/VInt/map work; `(2,6)` is
a camera operand, not Bowie's tile. Population loading then clears the old entities, lookups and
walking allocation, declares flag-selected followers, the fixed row and Bowie, and loads mapsprites.
It uses the scene table rather than Map51's default/F520 rows. With only the ordinary F67/F68
followers, Peter→physical1, B/Oddler→2, Elric→3 and Bowie→0. Other supplied follower flags can change
indices or overwrite B's binding; a fixed ally row is skipped if its logical lookup is populated.

This clears the old neutral159 and the battle population. No conversion or HP/status copy from
neutral159 to ally13 occurs. The parked default-map monsters are also absent from the scene table.
Declaring the scene's Elric requires no joined-flag test. CURRENT_BATTLE stays20, so the ally-sprite
consumer does not take its exploration HP-zero blue-flame branch; current class still matters.

### Dialogue and action completion

**Confirmed:** Elric's lines use modifier `$0`; Peter and B use `$C0` (mirrored/right portrait bits).
Portrait/speech resolve through the current mapped mapsprite. Text2566 uses `{NAME;13}` and two
newlines; 2567/2569/2571/2573 include one newline. Text2566/2568/2571 end in `{W2}`; the other
selected scene lines use `{W1}`. Text2573 uses `{LEADER}`, resolving the first active member's name
after rebuilding the lists, rather than an unconditional Bowie literal. Generic446 uses name,
class and newline, without a literal W1/W2; its separate music/input service follows.

The generic join announcement does not consume the scene cursor: 2575 is still the next scene
text after 2574. There is no YesNoPrompt, refusal branch or choice result. The nod and later input
acknowledgement are presentation actions, not a decision to accept Elric. Peter's attribution to
Oddler and proposal to visit Creed establish dialogue content, not a natural route or future travel.

Init replaces action/timer and can wait for exactly idle. It sets speed32/32, zero acceleration,
collision/obstructability/autofacing, Up facing, no immersion and size24 before refreshing/idling.
The scene explicitly waits Bowie/Peter/B Init, but has no separate such wait for Elric's table Init.
B's alternating action refreshes and waits30 at each of four facings; wait120 is a Sleep request,
not an action-pointer barrier. Bowie/Peter position/facing operations skip their own HP-zero state;
B31 bypasses that ally HP gate.

Elric's inline Down1 has no HP gate. It clears action flags5/6 and installs relative movement,
destination wait and final idle. Under consistent population, completed Init/movement and no other
displacement, `(6,8)` becomes `(6,9)`. The cleared conflict flag bypasses the destination-conflict
scan; the command neither dispatches an ordinary walking zone/warp nor proves elapsed duration.
Its wait joins only Elric's idle action. Bowie's nod stops animation, sleeps10/20/10 around
sprite work/refresh, then restores animation, without checking HP or consent.

Text waits for scrolling and uses the current actor; single-text commands also close windows and
sleep10. Script entry clears debug text skip, but debug/player2 Start can suppress scene text,
cursor increments and csWait Sleep. Generic join follows its own unconditional service path.
Cleared missing lookups can alias physical0; 255 is not a validated safe absence sentinel.
Actions, dialogue and nod do not inherit the position/facing HP test. Missing Peter/B, dead actors
or unfinished services can therefore differ from the ordinary cast without being repaired here.

## Joining the Force and Following Are Different

**Confirmed:** explicit join13 clears speech SFX, waits for scrolling and selects normal join music.
JoinForce sets F13, rebuilds joined/active/reserve lists in ascending ally order0–29, then checks
active count against12. If below12, JoinBattleParty sets Elric's active flag F45; that call does not
immediately rebuild the lists again. Later UpdateForce consumers reflect the activation.

With a full active party and F45 clear, Elric joins the reserves without displacing anyone. An
already joined reserve can activate if room exists. Supplied F45 remains set even at/above capacity;
this helper does not repair an inconsistent roster. Repeated join appends no duplicate member.
Supplying F13 before a first scene does not skip its dialogue, music or join service.

Neither JoinForce nor the join opcode heals HP/MP, status, class, equipment or combatant position.
Dead Elric can join without revival. After mutation, the opcode reads his current class, displays446,
requests previous music and waits for current masked input, then closes text and sleeps10.
`FadeOut_WaitForP1Input` is the helper's name: it performs music/input work, with no screen fade.
Music polling sleeps3 between checks. Any nonzero masked current direction/A/B/C/Start satisfies
input; no new press/release edge or yes/no answer is required. The special Sarah/Chester case128
and text447 are not selected. If these services stop, membership already changed; no rollback
guarantees later text, following, common join or completion flags.

All three follow commands check the high byte of their **word-encoded** follower ID. IDs7/31/13
have high byte0, so each checks **Bowie's HP**, not its named follower's HP. HP-zero Bowie skips
the entire command. Otherwise they map the current actors and install Peter→Bowie, B→Peter and
Elric→B in that order, using relative2's `(-24,0)` follower operand.

AddFollower copies a 42-byte follower action with physical target/offset and adds the physical
follower to the current exploration list only if absent. In the ordinary cast, Peter/B are already
listed and Elric's physical3 is appended. It writes no membership, active or persistent follower
selection flag. The action disables obstructability, enables uncollidable bits and follows/waits
for destinations; the follow consumer still checks layout and separation. It retains destination
within maximum-axis separation `$1E0`, otherwise tries the target-facing offset, zero-lateral
fallback and then target destination if blocked. Relative2 promises no world-coordinate endpoint.
The scene does not wait for these new follower actions to settle.

Scene end conditionally waits for scrolling if the dialogue-window index is nonzero, clears camera
scrolling speed and returns. It is no all-actor completion barrier and contains no camera-follow
restoration after the load. Earlier writes alone do not prove full program return.

## Fresh Pond Return Removes the Temporary Elric Actor

**Confirmed:** after full scene return or repeat bypass, the common after tail uses actual
cutscene join-table row20=0 and joins Bowie. This sets F0 and rebuilds the lists, now reflecting
Elric's F45 if set; Bowie activates only if the resulting active count is below12. It has no
generic join announcement/music and does not revise the previously saved return seed.

Victory next clears F420, sets F520, reads the saved map/X/Y/facing and returns d4=1. F470 remains
supplied. MainLoop runs SwitchMap then ExplorationLoop **without CheckBattle**. Map51 has no
flag-switched substitution, so its tuple is preserved. Normal saved map51 selects fresh population,
even though the scene just loaded the same numeric map; it is not the map255 preservation path.

Fresh exploration clears pending, subtracts20000 from the step counter with a negative clamp,
heals eligible allies again, fades, sets CURRENT_MAP51/CURRENT_BATTLE255, selects setup and builds
entities from the saved tuple. It loads sprites, clears temporary flags256–383, sets F80, loads
map/view, installs base VInts and calls init. Membership F13/F45, F470/F520 and follower flags lie
outside that temporary reset.

F520 selects Map51's empty fixed-entity list. Fresh initialization clears scene lookups/actions and
rebuilds following from the persistent follower-selection table, which has **no Elric row**.
Consequently Elric's temporary actor, action and current-list entry disappear while joined/active
flags remain. Active force membership does not spawn every ally as an exploration follower.
Peter/F67 and B/Oddler/F68 are rebuilt if selected; other flags retain their ordering and possible
B overwrite. F65 does not add the Caravan here because Map51 is not an overworld map.

| Supplied state | Immediate boundary |
| --- | --- |
| F470 set, first victory completed | Init returns without a pond scene; saved pre-scene position/facing survive setup subject to ordinary services. Elric remains a force member, with no persistent exploration follower row. |
| F470 clear, first or repeat victory | Shared init still runs the earlier help prompt despite F520-empty fixed population. F470 is neither repaired nor gated by F520. Its placements can supersede the saved tuple. |
| F520 set before victory | The whole Elric program is skipped. Common Bowie join/fresh setup still run; supplied F13 is retained, missing F13 is not restored. An empty repeat is no recruitment proof. |
| Full party, already joined reserve or supplied active flag | The join rules above determine membership/activation; all remain separate from temporary following and its removal. |
| Dead/missing/mismapped actors, empty/inconsistent lists or partial services | HP gates, lookup aliasing and incomplete waits still apply. Earlier writes do not establish natural reach, full return or usable input. |

The [accepted pond-init owner](elven-village-departure-through-harpy-pond-entry.md#the-complete-initial-prompt)
supplies the complete F470-clear continuation: initialize B/Peter; place Bowie `(11,22)`, B `(10,22)`
and Peter `(9,22)`, Up; fade; B moves Up2, waits/looks/waits, faces Up and speaks2550; Peter moves
Up2/speaks2551; camera `(2,2)`/wait50; Elric2552 and Peter2553–2554; camera follows Bowie; Peter
follows Bowie and B follows Peter; end. Under consistent actors/completed moves, B/Peter reach
`(10,20)`/`(9,20)` before following. No new Elric actor, membership or F470 write occurs; missing
Elric lookup0 can resolve his help line through Bowie. This supplied state is not a guaranteed
post-rescue presentation or evidence of its natural reach.

Normal battle entry set mode0 and F399, neither changed by this scene. Victory leaves F64 and
saved raft coordinates unchanged; a selected matching raft can be separately declared, without
boarding. Independently supplied mode2 takes the existing raft-population path. Later control
selects mode0 walking, mode1 eas_Raft (actual Caravan), or other-mode eas_Caravan (actual raft),
without validating arbitrary modes. After init/music/fade, pending zero lets WaitForEvent follow
physical0 and install that controller; a pending request takes precedence. This is **conditional
Map51 control**, not an observed playable pause. Stop before later departure producers.

## Defeat and Escape Stop at Their Selected Return

**Confirmed:** Battle20 has no special Egress-position case. With normal F399 set, the helper uses
supplied EGRESS_MAP and its matching savepoint row; no match retains that map with `(1,1)`, Up.
An abnormal direct F399-clear call instead returns initial Map3 `(56,3)`, Down before savepoint or
raft scanning. These are supplied-state exceptions, not a proved loss itinerary.

F64 clear preserves saved raft position. F64 set resets it by the selected savepoint: Hassan9 to
Map69 `(28,55)`, Polca13/Bedoe10 to Map72 `(43,48)`, otherwise default Map69 `(42,15)`. No reset
boards or changes mode. For supplied EGRESS_MAP23, return is Elven Village `(61,4)`, Up with the
default raft reset if enabled; this proves no last-savepoint history. Its unrelated content is
outside this guide.

SwitchMap precedes exploration without battle admission. Matching substitutions are Map3→4/F640,
Map71→78/F530 and Map58→59/F999, preserving coordinates/facing. Supplied map255 takes the existing
population path, updates main properties/raft and loads/init without fresh setup or temporary-flag
clear; it is not ordinary Map51 victory. Destination init or pending requests can further change
the continuation. No universal responsive-control claim follows.

## Evidence Trace and Review Boundary

**Confirmed provenance:** the accepted research binds SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Its [reproduction owner](../../research/battle20-outcomes-and-elric-recruitment.md#reproduction-and-remaining-limits)
retains exact source paths, symbols, source-line joins and consumer review. Selected originals are
`disasm/data/battles/entries/battle20/cs_afterbattle.asm` lines4–44 (program/ce_4B780), text2566–2575/446,
actual cutscene-table slot20 and leader row20. The included cutscene join table supplies Bowie0;
the similarly named global table's unused-feature comment does not erase this executed join.
This guide consumes accepted evidence without new source investigation, runtime or regeneration.

| Explanation | Contract / executable owner | Claim limit |
| --- | --- | --- |
| Defeated/region/after selection and common join | [cutscene routing](../contracts/battle-cutscene-routing.md); [cutscene fixture](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), sf2-battle-cutscenes-static-v1/expected.cutsceneFacts | Shared wrapper guards are public. Exact slot20, absent region20, leader0 and actual included join0 are accepted research joins, not complete public table proof. |
| Count checks, cleanup, healing, seed and outcomes | [battle lifecycle](../contracts/battle-control-lifecycle.md); [loop fixture](../../../tests/fixtures/h2/battle-loop-static-v1.json), sf2-battle-loop-static-v1/expected.lifecycleFacts | Fixture binds counts/cleanup/healing context. Selected branch/seed/escape/savepoint order is research-owned; no new combat simulation or natural outcome is claimed. |
| Full scene, dialogue, join and following | [dialogue](../contracts/dialogue-system.md), [party/roster](../contracts/party-roster-state.md), [map exploration](../contracts/map-exploration.md); [engine fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), sf2-map-script-engine-static-v1/expected command facts; [stats fixture](../../../tests/fixtures/h2/common-stats-static-v1.json), sf2-common-stats-static-v1/expected.statsFacts.party | Public summaries/handler facts do not reproduce complete choreography or literal dialogue. Accepted retained programCorpus.programs abcs_battle20 and direct-source joins supply all36 records, inline move, cast, text, Bowie-HP follow gate and immediate services. |
| Replacement population, fresh setup/init and conditional control | [setup](../contracts/map-setup-data.md), [entities](../contracts/map-entity-data.md), [exploration flow](../contracts/exploration-control-flow.md); [init fixture](../../../tests/fixtures/h2/map-init-static-v1.json), sf2-map-init-static-v1/expected | Public init has shared dispatcher, selected source bodies and embedded programs; accepted map-init/map-script comparisons and direct selected consumers supply F520-empty population/no-Elric persistent row. No universal actor/control rule follows. |

The selected engine fields are dialogueCommandFacts, forceStateCommandFacts, entityPopulationCommandFacts,
mapLifecycleCommandFacts, entityPlacementCommandFacts, entityActionBridgeCommandFacts,
entityGestureRelationshipMotionCommandFacts, mapCameraControlCommandFacts and scriptControlCommandFacts.
Schema/verifier routes are [cutscene schema](../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)/[battle_cutscenes.py](../../../src/sf2tool/h2/battle_cutscenes.py),
[loop schema](../../../schemas/h2-battle-loop-static-fixture.schema.json)/[battle_loop.py](../../../src/sf2tool/h2/battle_loop.py),
[engine schema](../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../src/sf2tool/h2/map_script_engine.py),
[stats schema](../../../schemas/h2/common-stats-fixture.schema.json)/[stats.py](../../../src/sf2tool/h2/stats.py)
and [init schema](../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../src/sf2tool/h2/map_init.py).
Existing `uv run sf2 h2` routes battle-cutscenes, battle-loop, map-script-engine, common-stats and
map-init locate these owners; no prose-only regeneration or blanket suite is selected.
Load ignored private-input configuration in the launching process for controlled commands.

Pre-synthesis review inspected accepted research, the shared contracts and bounded fixture/schema/
verifier surfaces above. Public quantities and handler boundaries agree with the stricter account.
Complete selected programs/tables/text and their consumer semantics rely on accepted research and
its retained/direct-source review, rather than a new independent reproduction. Shared contract
Unknowns remain open beyond these selected static joins; accepted remake music/control policies
in map-exploration do not establish original Battle20 timing or natural continuity.

Preserve [PR874's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/872#issuecomment-6171350805)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/874#issuecomment-6171383624),
including unchanged comparisons, final-head CI, resolved inspection failures and linked predecessor
receipts/failures. No completed failure is hidden by a new prose check.

**Unknown:** natural combat/outcome/route; actual flags, HP, classes, membership, lookups, mode, raft
and savepoint; completed text/action/camera/fade/music/input services; rendered scene, usable control
and save persistence. Independent F420/F470/F520, full/over-capacity/empty parties, repeated/supplied
membership, dead/missing actors, debug skip and partial services constrain this static account;
they are not executed scenarios. Under [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../decisions/0016-remake-start-evidence-deferral.md), they create no automatic runtime
queue. This guide establishes neither a composed runtime route nor campaign/design-epic completion.
