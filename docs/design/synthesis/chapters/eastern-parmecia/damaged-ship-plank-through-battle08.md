# Damaged-ship Plank Choices through Battle08 Admission

Bowie is asked to bring a plank above deck, where a worker requests help for a missing friend at
North Cliff. **Inferred narrative connection:** carrying and handing over the timber leads into a
rescue journey and an encounter. **Confirmed static content:** the first choice can return without
a request; the above-deck choice repeats until Yes. That scene sets F408 and requests Map72, whose
first eligible encounter is Battle08. **Unknown:** a naturally completed route, live actors/followers,
request survival and completed movement, text, camera, fade or audio services.

Read this after [Voyage and landing through damaged-ship control](../grans-opening/voyage-through-damaged-ship-control.md).
The accepted [research owner](../../../../research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md) supplies the
pinned-source, text, inline-action and caller review. This guide supports content/design planning
and stops at the Battle08 start dispatcher's return, immediately before `ActivateEnemies`. Combat,
outcomes and town construction belong to later segments. It adds no remake capability. The
[roadmap](../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns the Standard-rigor
agreement for [#735](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/735).

Continue with [Battle08 outcomes through the New Granseal castle handoff](battle08-outcomes-through-new-granseal-handoff.md)
for the conditional worker aftermath, construction scene and outgoing castle request.

`Fnnn` means flag nnn. Source references use `disasm/` at
[`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Research and fixtures bind the USA retail ROM in the [root baseline](../../../../../README.md#pinned-original-baseline).
Command indices are zero-based script records, not frames or measured durations. Dialogue summaries
and narrative interpretation supply no additional gameplay predicates.

## 1. Responding to the Plank Request

**Confirmed starting boundary:** the predecessor returns from Map73 init with Map14 current and
its scripted `ce_5079E` population. It did not run Map14 init. Bowie is authored at `(40,8)`, Left;
objects 131–133 are at `(44,6)`, `(45,6)`, `(46,6)`, and Jaha has been hidden. Actual arrival positions,
F6, follower mappings, player type and the first pending/control read retain the predecessor's limits.
This is the starting scene context, rather than an ordinary Map14 setup population.

A movement destination whose working-map event bits masked by `$3C00` equal `$1400` produces zone
event type 6 and destination tile coordinates. That producer neither checks F257 nor proves that
Bowie can legally reach `(45,5)`: collision, occupancy, changed blocks and controlled-entity updates
remain inputs. `ProcessMapEvent` clears the pending word before dispatch; the zone selector chooses
the first matching coordinates, allowing 255 wildcards, or its default.

With F458 clear, Map14's default zone table selects `Map14_ZoneEvent0` at `(45,5)`. It runs
`cs_58E5C` (0–15), then sets F257. Neither the producer, selector, handler nor script uses F257 as a
repeat guard. F458 instead selects another zone table, whose nondefault `(255,15)` row is separate
content; the same `(45,5)` lookup does not offer this plank choice under that setup.

The text asks whether Bowie will bring the plank up. `yesNo` overwrites F89 on every invocation:
Yes/return 0 sets it; No or B cancel clears it. The menu begins on Yes, Left/Right choose Yes/No,
A/C confirm, and B returns No. A previously supplied F89 cannot answer the new prompt.

| Response | Confirmed ordered result |
| --- | --- |
| No or B cancel | F89 clear jumps to the end. No scripted movement or warp follows. The caller still sets F257. Its zone tail closes portrait/text, waits a VInt and waits for entity 0 to stop; later zero pending can reinstall control, with actual readiness Unknown. |
| Yes | Hide text; await Bowie Down 1; wait 40; separately await disabling auto-facing on 131, 132 and 133. Start Bowie and objects 131/132 Down 3 unjoined; await 133 Down 3, then request explicit Map14 `(13,11)`, Down. |

**Inferred staging:** the object group depicts the plank being moved. No item acquisition or
inventory-carrying state follows from those movements. Awaiting object 133 does not independently
prove completion of the other three actors. Disabling auto-facing jumps to idle; unreachable later action
instructions add no acceleration effect.

The warp writes a shared pending type-1 slot; it does not immediately load Map14. Script return,
caller F257 write and zone-tail services precede a later pending read/consumer. Another writer can
overwrite the request during those services. If it survives, the no-scroll consumer returns its tuple
to MainLoop. The [plank-event review](../../../../research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md#starting-population-and-plank-event)
owns this order and the distinction between optional response and observed control restoration.

## 2. Above-deck Entry Depends on Setup and Init State

**Confirmed entry distinction:** the accepted response requests explicit map 14, even though it is
already current. Map14 has no substitution or encounter row. Explicit exploration entry rebuilds
the selected entities, loads sprites, clears setup flags 256–383, including F257, and sets F80 before
loading the map and running setup init. The default fixed list is empty, but follower initialization
still occurs. No player-control handoff intervenes before init.

Same-map stairs instead use `MAP_CURRENT`: deck triggers `(12,11)`–`(14,11)` lead to below-deck
`(44,8)`–`(46,8)`, Up; below-deck triggers `(44,9)`–`(46,9)` lead to deck `(12,12)`–`(14,12)`, Down.
CheckBattle scans current Map14 but preserves the sentinel when no battle matches. This entry branch
updates the main entity/raft without rebuilding setup entities or clearing temporary flags.
**Both entry branches still run setup init.** A spawn coordinate also appearing in the warp table
does not itself invoke a second warp.

| State at init selection | Confirmed consequence |
| --- | --- |
| F458 clear, F703 clear | Default init runs `cs_58FA4`, then sets F703 and writes `EGRESS_MAP=14`. There is no F257, carried-item or entry-coordinate test. |
| F458 clear, F703 set | Return before the scene and both caller writes. Explicit entry retains the empty fixed setup plus admitted followers; it does not restore the plank objects. |
| F458 set, either F703 | Alternate init only returns. Its explicit-entry fixed population is a priest and worker, plus admitted followers; it replaces the first-handoff content. |

A same-map stair or direct entry with F458/F703 clear can therefore select the above-deck scene
without a plank-acceptance test. This is a conditional counterexample to a mandatory inventory task,
not evidence of a legal walking shortcut or naturally attainable arbitrary flags. F703's source
comment also refers to Ribble: setting it is not unique proof of this handover. The
[entry/init review](../../../../research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md#explicit-map14-entry-versus-stairs-and-repeat-entry)
owns these alternatives.

## 3. Timber Handover Becomes a Rescue Request

**Confirmed scene content:** `cs_58FA4`, in Map14 `mapsetups/scripts_2.asm`, reloads the map with
view `(8,7)`, then loads `ce_59270`. Bowie is authored at `(13,11)`, Down. Merchant 128, young
man 129, man 130 and worker 131 now accompany objects **132–134**; the old interior object range
was 131–133. Logical references use the rebuilt entity-index mapping, not allocation order.

List loading clears entities, initializes followers and declares fixed records/main. An ally already
mapped as a follower is skipped in the fixed list. Sarah, Chester, Jaha, Kazin, Slade and Kiwi have
scene declarations, but flags/player type still affect their actual admission. This script has no
F6 test: it declares and directs Kiwi even when F6 is clear. Those commands neither recruit him nor
establish battle eligibility.

| Player-facing phase | Confirmed source order and interpretation boundary |
| --- | --- |
| Prepare the deck (0–9) | Await Bowie init; assign priority to Bowie, man 130 and middle object 133; fade in. Start young man 129's independent custom walk/wait action without joining it. |
| Pass over timber (10–43) | Merchant asks for help; man 130 responds and approaches. Move objects 132/133 Down 1 unjoined and await 134; show handover narration. Move man and 132/133 Down 7 unjoined and await 134; hide man and all three objects, then reposition merchant. |
| Hear the rescue request (44–68) | Merchant laments Granseal; worker 131 calls for help. Await his speed-48 setup and Up-6 move, then initialization back to speed 32. Worker gestures and describes the missing friend at North Cliff; merchant reacts and addresses Bowie. |
| Accept the task (69–75) | Reset cursor 2275, ask Bowie to investigate, prompt Yes/No. Yes branches onward; No/cancel displays two replies and returns to this prompt. Every retry resets cursor and overwrites F89. There is no refusal return to control. |

**Inferred narrative meaning:** timber is handed over and the worker asks Bowie to accompany him to
rescue a friend. Unlike the first plank choice, this second refusal is a dialogue loop. The actual
operations include no item-give/remove, force-join/leave or HP assignment. Hiding man/objects removes
scene entities and mappings; it transfers no inventory or combatant. Animation stop/start does not
change health, and priority writes store Boolean 0/1, not a signed height or observed overlap.

The program makes no block-copy commands. Its reload updates map/view through LoadMap; it does not
re-enter admission or run another setup init. Earlier below-deck layout cells were not inventory.
Text, fade, camera motion and the unjoined young man's action retain separate completion dependencies.
The [complete above-deck review](../../../../research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md#above-deck-script-and-actor-boundary)
owns the commands, cursor resets, inline actions and reference changes.

## 4. Departing Produces the Encounter Request before Caller Writes

**Confirmed ordered departure:** after Yes, the merchant praises Bowie and assigns the worker as
guide. Merchant/worker move and speak; worker and Bowie start Down 4 without joins while Jaha is
awaited Up 5. Jaha calls out and Sarah comments on his wet appearance (76–94). Neither appearance nor
dialogue supplies an HP change or proves what happened to him outside this scene.

The script next places Kazin, Chester, Slade, Sarah and Kiwi along X13 at Y10/9/8/7/6, Down. It
starts their separate paths, awaits Kiwi's Down-7 action, then assigns facing (95–112). Sarah, Kazin
and the worker discuss accompanying Bowie. After a wait and neutral camera, worker, Bowie, Sarah,
Chester, Jaha, Kazin and Slade start Down 7 unjoined; Kiwi's Down 7 is awaited (113–126). The final
wait for Kiwi is not a barrier proving that every actor or camera service completed.

The outward state order is precise:

1. Command 127, `setStoryFlag 8`, adds 400 and sets **F408**. It does not set F8, F458 or F508.
2. Command 128 writes pending Map72 `(1,1)`, Down; command 129 ends the script.
3. The still-running Map14 init caller sets **F703**, then writes **EGRESS_MAP=14**.
4. A later nonzero pending read skips control installation. If this request survives, its type-1
   consumer returns the tuple to MainLoop for admission.

F703 and EGRESS_MAP are written after request production, before its pending read/consumption.
EGRESS_MAP stores a destination input; it establishes no executed escape or future battle outcome.
A cleared/overwritten pending slot breaks this conditional connection. The independent Map14 exit
at `(255,20)` requests Map72 `(1,1)`, Up without a story-flag write; it can also admit Battle08 if
F408 is already set. Neither route proves natural walking or a particular flag history.

## 5. Map72 Selects the First Eligible Battle

**Confirmed admission:** Map72 has no substitution row. `CheckBattle` scans ascending battle
indices. Row 8 names Map72 with wildcard trigger `(255,255)` and output battle area `(1,32,32,30)`.
Earlier rows 0–7 name other maps, so F400–F407 do not intercept this request. Later Map72 rows 24/26
cannot precede eligible row 8. With F408 set, `(1,1)` selects Battle08 even though it is outside the
area rectangle: the rectangle is output geometry, not a containment test. Facing is not a predicate.

| Relevant input | Confirmed result in this call |
| --- | --- |
| F408 set, F508 clear | Select Battle08. |
| F408 set, F508 set | Select Battle08 and clear F408 in the same call; completion does not reject this already selected encounter. |
| F408 clear | Skip row 8; a later unlocked matching row can win, otherwise return no battle. |

F257, F703 and F458 are not CheckBattle predicates. A flag-producing script alone does not prove that
MainLoop consumed its request. The
[admission/before-start review](../../../../research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md#map72-admission-and-battle08-beforestart)
owns both the exact row and caller boundary.

## 6. The Opening Scene Is Separate from the Combat Roster

MainLoop passes battle index 8 to BattleLoop, which clears player type and sets F399.
**Confirmed ordinary path, F88 clear:** clear seconds, save incoming map/battle, write current
Map72/Battle08, install base VInts and run the before dispatcher. F458 clear selects `bbcs_08`;
F458 set skips it. The before dispatcher does **not** set F458 on return.

The before scene rebuilds from `ce_4AA14` after loading Map72/view `(1,46)`. Its main placement is
`(4,50)`, Right, with fixed Sarah/Jaha/Kazin/Chester/Kiwi declarations. Slade has no fixed record;
any follower mapping is a separate input. Worker 128 is the guide; worker 129 is the friend. Witch
130 and creature sprites 131–133 form another scene group. This list describes staging, not a
combat roster or guaranteed live cohort.

| Opening phase | Confirmed content and limits |
| --- | --- |
| Prepare the friend (0–9) | Await Bowie init; stop worker 129's animation and await motion-off/Left orientation/sprite update. F6 clear relocates Kiwi to `(63,63)`, Down; F6 set skips that command. Both request Main Theme/fade-in. Relocation is not hide or a force-leave operation. |
| Discover trouble (10–17) | Guide approaches and points out the friend; camera targets 129, dialogue describes his position, then Enemy Attack music and another camera destination are requested. |
| Stage the attackers (18–30) | Place goblin/green ooze/hunter goblin 131–133, then witch 130, with intervening waits. Text operands 25–28 all name `$C0,130`, including the question about hurting the worker; narrative intuition must not reassign that speaker. Face witch Up and issue attack dialogue. |
| End the introduction (31–34) | Start jump actions for 131/132 unjoined and await 133's jump, then return. Each action disables auto-facing, requests Y -1/+1 with waits, restores auto-facing and returns to idle. |

**Inferred staging:** the friend appears prone and hostile creatures confront the group. Motion and
animation suppression assign no worker HP and prove no death. The Map72 load inside this scene
invokes neither MainLoop admission nor Map72 setup init; the named waits/music/fade/camera requests
are not independently observed presentation completion.

After the before dispatcher returns, ordinary BattleLoop restores/writes its saved map/battle,
clears region flags 90–105, heals living/immortal allies, initializes ally/enemy battle positions,
clears AI memory and calls LoadBattle. Combat placement uses the updated battle party, Battle08
spriteset and ally HP checks. It does not promote the scene NPCs or every declared ally into combat.
LoadBattle positions entities, loads map/sprites/terrain, installs VInts, requests map music and fades
in; runtime completion remains Unknown here.

The start dispatcher then checks the same F458. If clear, it **sets F458 before** invoking Battle08's
start-table target, `ms_Empty`; if set, it skips that target. Thus the first ordinary entry has a
before scene and an empty start script, while F458-set reentry skips both scripts but still performs
ordinary battle initialization/load. Neither dispatcher recruits anyone. F458 also selects Map14's
alternate setup on a later lookup.

**Confirmed alternate caller:** F88 set takes the suspended-battle path: restore seconds, clear F88,
clear AI memory, reset camera target and LoadBattle, then jump directly to the individual-turn loop.
It bypasses before/start dispatch and ordinary position initialization. It neither proves a valid
supplied suspended state nor establishes that every BattleLoop entry sets F458. No combat is consumed
here. The ordinary-path stopping boundary is the start dispatcher's return **before ActivateEnemies**.

## Evidence Review and Remaining Limits

This synthesis reviewed accepted #732 and its voyage predecessor, the relevant contracts, selected
tracked fixtures and their verifier/schema boundaries. Research owns the complete pinned-source,
text, inline-list/action review and retained comparisons. No source investigation, extraction,
comparison rerun or runtime observation is added. The narrow commands below identify reproduction
owners, rather than checks newly executed for this guide.

| Relation | Reviewed executable owner and reproduction route | Boundary |
| --- | --- | --- |
| Plank selector and F257 | [zone shard](../../../../../tests/fixtures/h2/map-events/zone-programs.json), `sf2-map-events-static-v1`, program `Map14_ZoneEvent0:364112`; [shard/schema owner](../../../../../src/sf2tool/h2/map_events_fixture.py); [setup fixture](../../../../../tests/fixtures/h2/map-setup-static-v1.json), `sf2-map-setup-static-v1`. Commands `uv run sf2 h2 map-events`, `map-setup`. | The handler contains script → F257 → return, without a local guard. Accepted research owns the required producer/menu/selector review and F458 counterexample; no natural reachable-tile/repeat observation follows. |
| Entry and pending consumer | [common-maps](../../../../../tests/fixtures/h2/common-maps-static-v1.json), `sf2-common-maps-static-v1`; [map-content](../../../../../tests/fixtures/h2/map-content-static-v1.json), `sf2-map-content-static-v1`; [request consumption](../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `sf2-map-event-request-consumption-static-v1`. Commands with those basenames under `uv run sf2 h2`. | Separate source tables, writers and consumers agree with explicit/sentinel entry; request survival, walking and a zero control-read value remain Unknown. |
| Init and script effects | [init fixture](../../../../../tests/fixtures/h2/map-init-static-v1.json), `sf2-map-init-static-v1`, primary bodies 19/20; [verifier](../../../../../src/sf2tool/h2/map_init.py), [schema](../../../../../schemas/h2-map-init-static-fixture.schema.json); [script fixture](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json), `sf2-map-script-engine-static-v1`; [verifier](../../../../../src/sf2tool/h2/map_script_engine.py), [schema owner](../../../../../src/sf2tool/h2/map_script_schema.py). Commands `map-init`, `map-script-engine` under `uv run sf2 h2`. | Body 19 preserves F703 test → cs_58FA4 → F703 → EGRESS_MAP; body 20 only returns. Public command facts retain cs_58E5C warp 14, cs_58FA4 reload/list 1/2 and warp 128, bbcs_08 load/list 1/2. Full branches, text and inline actions use accepted research, not a public complete transcript. |
| Scene actors versus combat | [map-entities](../../../../../tests/fixtures/h2/map-entities-static-v1.json), `sf2-map-entities-static-v1`; [battle-loop](../../../../../tests/fixtures/h2/battle-loop-static-v1.json), `sf2-battle-loop-static-v1`; [spritesets](../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), `sf2-battle-spriteset-data-static-v1`. Commands `map-entities`, `battle-loop`, `battle-spriteset-data` under `uv run sf2 h2`. | Generic entity/roster facts preserve distinct authorities; exact lists and follower/F6 counterexamples use the research review. Authored casts do not prove actual mappings, joins, health or combat eligibility. |
| Battle08 before/start | [global data](../../../../../tests/fixtures/h2/battle-global-data-static-v1.json), `sf2-battle-global-data-static-v1`; [cutscenes](../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), `sf2-battle-cutscenes-static-v1`; [cutscene data](../../../../../tests/fixtures/h2/battle-cutscene-data-static-v1.json), `sf2-battle-cutscene-data-static-v1`; [routing](../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json), `sf2-battle-routing-data-static-v1`. Commands with those basenames under `uv run sf2 h2`. | Shared F458 before/start rules agree. Aggregates declare `storyContentParsed=false` / `routeTargetsParsed=false`; exact row 8, bbcs_08 and ms_Empty targets belong to accepted research's direct source review, not parsed routing summary claims. |

The [map contract](../../../contracts/world/map-exploration.md#load-and-selection-order) preserves path-dependent
loads; its conceptual exploration init step is not an init call made by every LoadMap service.
The [setup-data contract](../../../contracts/world/map-setup-data.md#contract-boundary) preserves static selection
identity, and the [party/script contract](../../../contracts/party/party-roster-state.md#handler-boundary) keeps
actor operations separate from recruitment and combatant state. No contract rewrite follows here.

**Unknown, preserved:** natural event/arrival lineage and legal walking; actual party, actor/follower
mappings and positions; pending survival/overwrite and control readiness; independent motion, scroll,
text, fade, sprite overlap, audio and timing completion; and save/load persistence. A future composed
observation would need the starting population/flags/player type, both choice results, explicit or
sentinel entry, init selection, ordered request/caller writes and actual admission/start consumer.
F257 already set, No/cancel, F703/F458 alternatives, F6 clear, conflicting followers, overwritten
pending, F408 clear, F508 set and F88 resume distinguish the relevant seams. These are controls, not
an automatic runtime queue. This bounded guide completes neither the campaign nor the design epic.
