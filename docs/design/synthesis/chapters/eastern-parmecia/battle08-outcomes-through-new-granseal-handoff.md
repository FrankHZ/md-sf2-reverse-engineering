# Battle08 Outcomes through the New Granseal Castle Handoff

The rescue encounter leads into a worker's recovery, a blocked-tunnel conversation and a return to
the ship. A later scene presents the dismantled ship and the construction of New Granseal before
requesting its castle. **Inferred narrative meaning:** these scenes connect the rescue to the
settlement's founding. **Confirmed static behavior:** the first victory supplies a Map73 return;
only the appropriate Map73 init branch runs the construction scene and requests Map7.
**Unknown:** a naturally completed journey, tunnel access, live actor/follower state, service
completion and survival of the outgoing request.

Read this after [Damaged-ship plank choices through Battle08 admission](damaged-ship-plank-through-battle08.md).
The accepted [research owner](../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md)
supplies the pinned-source, dialogue, action and caller review. This guide supports content/design
planning and stops after the conditional outgoing Map7 admission, **before destination initialization**.
Continue with [New Granseal castle: the year-later address and first control handoff](new-granseal-castle-year-later-control.md)
for destination initialization; later town/campaign content remains separately scoped. It adds
no playable remake capability. The [roadmap](../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)
owns the Standard-rigor agreement for [#739](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/739).

`Fnnn` denotes flag nnn. Source identities use `disasm/` at
[`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Research and fixtures bind the USA retail ROM in the [root baseline](../../../../../README.md#pinned-original-baseline).
Command indices identify zero-based script records, not frames or measured durations. Narrative
summaries add no gameplay predicates.

## 1. Victory, Defeat and Escape Lead to Different Returns

**Confirmed outcome rule:** Battle08 uses the shared battle loop. After actions and turn-end effects,
zero allies takes precedence over zero enemies; Bowie's zero HP supplies the ally-loss override.
Defeating enemy slot 128 alone does not create a special leader-only victory. The selected defeated
route is `ms_Empty`; its wrapper requires Bowie alive, slot 128 dead and F508 clear. Its
leader-presence row is zero, and the leader-position table has no Battle08 override. Thus that
defeated hook does not supply a worker rescue or remove every remaining enemy.

The independent after-battle row selects `abcs_battle08` only while completion flag F508 is clear.
The shared after wrapper still calls `JoinForce(0)` for Bowie, on both executed and skipped paths.
This is the live cutscene join table's row 8, not the similarly named legacy table. It recruits
neither worker nor Kiwi. The [selected-row review](../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#outcomes-rows-and-return-fields)
owns those exact source choices; the shared [cutscene contract](../../../contracts/battle/battle-cutscene-routing.md)
owns the dispatch and join boundary.

| Outcome and supplied state | Confirmed effects | Return supplied to the caller |
| --- | --- | --- |
| Victory with F508 clear | Heal living/immortal allies, seed return fields, run the after scene and shared join, clear F408 and set F508; return d4=1. | The scene replaces the seed with Map73 `(62,49)`, Left. |
| Victory with F508 set | Perform the same heal, seed, shared join and completion writes, but skip the after scene. | Current Map72, the first active party member's final position plus battle-area origin `(1,32)`, and that entity's facing. No fixed Map73 return is supplied. |
| Defeat | Update the unlock flag, show fixed text 363, restore Bowie's max HP and halve gold by unsigned shift; return d4=-1. | The savepoint selected from supplied `EGRESS_MAP`; no after scene, shared after join or new F508/F705 write. |
| Egress / Angel Wing | Charge spell MP / remove the selected item slot; use fixed text 274 / 275, update the unlock flag and return d4=0. | The same stored-map selection, without defeat's gold halving or the after/join path. |

On defeat or escape, `UpdateBattleUnlockedFlag` leaves F408 when F508 is clear and clears F408 when
F508 is set. Neither outcome creates completion. With the predecessor's unchanged `EGRESS_MAP=14`
and F399 set by BattleLoop, the damaged-ship savepoint is Map14 `(45,7)`, Down. Other supplied maps
select their own rows or the `(1,1)`, Up fallback. Shared F64 raft-reset and F399-clear game-start
fallback rules remain with the research owner. These outcome returns use `SwitchMap` and enter
exploration directly; they do not perform another battle-admission scan. Natural escape/defeat
reentry is **Unknown**.

The victory heal and the following exploration heal skip dead non-immortal allies. Their exact
HP/MP and status-mask limits remain with the [shared outcome/heal owner](../../../../research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order).
The later construction reset has a wider scope. F458 controls before/start scenes independently of
F508; skipping an intro does not establish completion. F88 suspended entry restores seconds, clears
F88/AI memory, loads battle and resumes individual turns; it does not by itself suppress a later
F508-clear after scene. Valid suspension history and natural combat remain **Unknown**.

## 2. The Worker Recovers, but the Tunnel Is Still a Dialogue Boundary

**Confirmed scene content:** `abcs_battle08` loads Map72/view `(8,32)`, replaces the scene population
with `ce_4ABBE`, awaits Bowie initialization and stages worker 129 with animation stopped and
motion off. Worker 128 is the guide; 129 is the friend. The list declares Bowie, Sarah, Jaha, Kazin,
Chester and Kiwi, but no fixed Slade record. Those declarations are scene staging, not combat HP
or a roster. Loading the list rebuilds entities/followers; existing ally mappings can take precedence
over fixed placements, and NPC references resolve through `ENTITY_INDEX_LIST` rather than physical
allocation order.

With F6 clear, the scene selects Kiwi's relocation to `(63,63)`, Down; F6 set skips it. This is a
position command, not hiding or leaving the force. Ally handlers guarded by
`AdjustScriptPointerByCharacterAliveStatus` can skip payloads for zero-HP allies below index 30,
so selection does not prove a position write for dead Kiwi. Workers 128/129 bypass that ally check.
The later Kiwi-facing command is present even on the F6-clear branch. Actual mappings and effective
ally mutations remain input-dependent.

The guide approaches and reacts to his friend. The friend receives a posed sprite, then his worker
sprite is restored; later animation and facing actions accompany the conversation. **Inferred:**
the scene depicts regaining consciousness after a falling rock. There is no healing, death,
force-join or item-acquisition operation implementing that narrative.

The friend describes the rock and the blocked tunnel. The camera is directed toward the tunnel area
and back; the guide jumps, laments the inaccessible treasure and proposes returning to the ship.
**Confirmed source content, with a strict limit:** the scene has no rock-removal, block-copy or
tunnel-traversal operation. Dialogue does not prove collision, a walkable path, access or treasure
contents. The [after-scene owner](../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#battle08-after-scene)
binds commands 0–60 and dialogue IDs `09A4..09B2` without turning the dialogue into route evidence.

Named waits also retain their scope. The friend's Right/Left action and the guide's bump begin
without joins; the wait of 120 is not an explicit join. Other worker movements, init actions and
the jump are awaited individually. Neither an awaited actor nor the interpreter's conditional
scroll wait proves that all motion, camera, text, fade or audio services completed.

## 3. The Battle Caller Returns to the Ship before Map73 Selects Its Scene

**Confirmed first-victory order:** the final after-scene `warp` writes event type 1, parameter 1
zero and the Map73 `(62,49)`, Left fields. It does not load Map73 or end the script. After `csc_end`
and interpreter return, the shared join runs; BattleLoop clears F408, sets F508 and reads the
destination fields directly into d0–d3, returning d4=1.

This first return does **not** use `WaitForEvent` or `ProcessMapEventType1_Warp`.
`alt_MainLoopEntry` performs `SwitchMap` and goes straight to exploration, bypassing `CheckBattle`.
Map73 has no substitution row. No player-controlled exploration is implied between the worker
scene and that return. The direct read is confirmed; survival of its fields through intervening
services is **Unknown**.

At entry, exploration clears the old pending word, heals and fades. Explicit Map73 entry sets the
current map and not-in-battle state, obtains setup entities, rebuilds them, loads sprites, clears
temporary flags **256–383**, sets F80, loads the map/base VInts and calls init before installing
control. F408/F458/F508/F700/F705 lie outside that clear range. Map73 has only its default setup;
the fixed entity list is empty, while flag-selected followers can still be populated.

**Confirmed selection:** `ms_map73_InitFunction` checks progress flags, not arrival coordinates,
worker health, F408, F458 or EGRESS_MAP.

| Supplied init state | Selected content and caller write |
| --- | --- |
| F700 clear, any F508/F705 | Run voyage `cs_503A6`, then set F700 and return without testing the later branch. |
| F700 set, F508 clear | Return without either scene. |
| F700 set, F508 set, F705 set | Return without either scene. |
| F700 set, F508 set, F705 clear | Run construction `cs_50806`, then set F705 and return. |

The earlier [voyage](../grans-opening/voyage-through-damaged-ship-control.md) supplies F700 in the intended lineage;
this victory supplies F508. F705 clear is a required input. Direct Map73 init with F508 already set
can select construction without this worker after scene, while F700 clear selects voyage instead.
A repeat victory does not itself supply a Map73 return, so construction cannot be promised after
every win. The [battle-to-init review](../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#battle-caller-to-map73-init)
owns this conditional connection.

## 4. Returning to the Ship Stages the New Town

**Confirmed ordered content:** `cs_50806` has no script-level branch. Once selected, it replaces the
population with `ce_5094A`:
Bowie, two worker references and a ship sprite, with no fixed ally declarations. The new workers
are newly declared references; persistence of identity from the previous scene is unproved.
Follower initialization still applies, including Map73's F65-dependent overworld choice and other
supplied follower flags.

| Phase | What the script directs | Meaning and limit |
| --- | --- | --- |
| Establish the procession | Await Bowie init, guide 128 init twice, then Bowie init again; install worker 129 following Bowie and Bowie following 128, both selector 2. Copy an `11x10` block region from `(42,0)` to `(24,48)`, follow the guide with the camera and fade in. | Duplicate init calls are real. Following is a transient action-script relationship, not force membership or a persistent follower-flag write. |
| Approach the ship | Await four action lists on the guide, then use `setPosFlash` on ship 130 to `(0,0)`, Left. A worker comments that the ship has been dismantled; direct facing and the camera toward the construction area. | The guide's wait does not establish follower collision or final positions. Flashing/relocation is not deletion, hiding or combat death. |
| Present construction | Replace the `11x10` region at `(24,48)` in an ordered sequence using source blocks `(42,0)`, `(53,0)`, `(42,10)` and `(53,10)`, separated by waits. Dialogue discusses the new town and old Granseal. | **Inferred:** staged ship dismantling and town construction. Working-map copies and view updates do not measure elapsed building time or establish a persistent world transaction. |
| Leave the scene | Set neutral camera; await init for 128, 129 and Bowie. For each in that order, set destination `(29,52)`, wait for arrival, then set position/destination `(0,0)`, Down. Wait 60 and show the town-building narration. | Init replaces their current action scripts. Each destination command is a named wait, followed by relocation; this is neither three unjoined departures nor a global completion barrier. |

`followEntity` selector 2 resolves offset `(-24,0)` and installs a follower action script through
`AddFollower`; later `eas_Init` replacements end that script relationship. No survival across a new
map load is established. For the three `setDest` calls, each unchanged entity word has bit 15 clear,
so `csc29_setEntityDest` waits for position to equal destination before returning. Actual successful
movement and other services remain **Unknown**.

`setBlocks` calls `CopyMapBlocks` and marks both view planes for update. `setPosFlash` alternates the
ship's X coordinate while waiting, then assigns its final position and destination. Neither effect
consumes building materials or an inventory item. The [construction owner](../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#map73-later-scene-and-castle-request)
retains exact commands 0–52, copy order and dialogue IDs `08F0..08F3`; this narrative summary does
not replace those source operations.

The scene then requests sound fade-out and calls `resetForceBattleStats`. **Confirmed reset scope:**
`ResetAlliesBattleStats` gives indices **0–29** max HP and max MP without joined, active or alive
filters. It masks existing status to stun/poison/curse, rebuilds stats and subsequently derives curse
from equipment; retained bits are not newly applied ailments. This reset can restore zero-HP allies,
unlike the earlier living/immortal heal. It does not recruit anyone or change party membership.
The [accepted reset owner](../../../../research/chapters/grans-opening/voyage-through-damaged-ship-control.md#force-reset-and-entity-list-semantics)
and [party/script contract](../../../contracts/party/party-roster-state.md#handler-boundary) preserve that distinction.

## 5. The Castle Request Precedes the Caller Completion Flag

**Confirmed outgoing order:** after the reset, construction command 51 requests Map7 `(11,10)`,
Up with event type 1 and parameter 1 zero. Command 52 ends the script. After interpreter return,
the **Map73 init caller sets F705**; the construction script itself does not write it.

Exploration performs its conditional music/fade calls before reaching `WaitForEvent`. If type 1
survives, the nonzero read skips control installation. `ProcessMapEvent` clears the pending word
before dispatch. Its type-1 warp consumer takes the parameter-1-zero branch, handles/clears WARP_SFX,
makes entity 0 idle, unwinds exploration and returns the map/facing and positive X/Y bytes in d0–d3
with d4=0. This ordinary exploration return goes to `MainLoop/@Start`, which performs both
`SwitchMap` and `CheckBattle`.

Map7 is `MAP_NEW_GRANSEAL_CASTLE`. It has no map-substitution or battle-coordinate row, so this
conditional admission retains `(7,11,10,UP)` and selects exploration. **The guide stops before
Map7's ExplorationLoop initialization.** It establishes neither a castle population/init scene nor
player-ready castle control. The [outgoing consumer owner](../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#pending-consumer-and-stopping-boundary)
retains the exact boundary.

There is one shared pending slot, not a queue. An overwrite or clear can prevent castle consumption
even after F705 is set; if pending is zero at the control read, Map73 can install control instead.
Map73 remains current until explicit destination entry, since the request itself does not change
current map.

On the intended completed chain, F408 is clear and F508/F700/F705 are set. F458 is carried from the
ordinary predecessor start, rather than established by suspended entry; `EGRESS_MAP` remains its
supplied value, typically 14. `PLAYER_TYPE` remains 0 from BattleLoop. Other story/follower flags
retain their supplied history. Scene actors, transient follows and rendered/audio completion are
not guaranteed carried state.

## Evidence Review and Remaining Questions

**Confirmed review disposition:** this synthesis compares the accepted research and predecessor
guides with the tracked fixture selections below and their schema/verifier boundaries. The selected
Map73 body agrees with the F700-first selector and post-script F705 write. Shared cutscene facts
agree with completion gating and the after join; handler facts agree with warp production, transient
following, bit-15 destination waits and force reset. No evidence conflict requires a contract change.
Scene-specific text/actions, row-8 choices and the absent Battle08 position override come from the
accepted research's direct pinned-source review. Aggregate fixtures do not independently prove the
whole journey or redistribute the scene transcript.

Fixture links below contain their exact `sf2-…-static-v1` IDs. Narrow command names have prefix
`uv run sf2 h2`; they identify existing owners and **were not rerun for this documentation slice**.

| Claim and accepted owner | Tracked fixture selection / narrow command | Preserved boundary |
| --- | --- | --- |
| [Outcome and return rules](../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#outcomes-rows-and-return-fields) | [sf2-battle-control-static-v1](../../../../../tests/fixtures/h2/battle-control-static-v1.json), [sf2-battle-loop-static-v1](../../../../../tests/fixtures/h2/battle-loop-static-v1.json); `battle-control`, `battle-loop`. | Shared checks, caller flags and healing; actual natural combat and outcome history remain Unknown. |
| [Selected after/defeated/join rows](../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#outcomes-rows-and-return-fields) | [sf2-battle-cutscenes-static-v1](../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json) `expected.cutsceneFacts`; [sf2-battle-global-data-static-v1](../../../../../tests/fixtures/h2/battle-global-data-static-v1.json), [sf2-battle-routing-data-static-v1](../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json); same basename commands. | Shared wrapper facts are fixture-bound; `routeTargetsParsed=false` does not parse row 8. The live join is distinct from the unused legacy join aggregate. |
| [Map73 selection](../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#battle-caller-to-map73-init) | [sf2-map-init-static-v1](../../../../../tests/fixtures/h2/map-init-static-v1.json) `expected.primarySourceBodies[79]`, `ms_map73_InitFunction`, and `cs_50806` call site; [sf2-map-setup-static-v1](../../../../../tests/fixtures/h2/map-setup-static-v1.json), [sf2-map-entities-static-v1](../../../../../tests/fixtures/h2/map-entities-static-v1.json); `map-init`, `map-setup`, `map-entities`. | Static selector/setup identity agrees; empty fixed entries do not exclude followers or prove natural arrival. |
| [After and construction commands](../../../../research/chapters/eastern-parmecia/battle08-outcomes-through-new-granseal-handoff.md#map73-later-scene-and-castle-request) | [sf2-map-script-engine-static-v1](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json) `expected.transitionCommandFacts`, `entityPopulationCommandFacts`, `mapBlockMutationCommandFacts`, `entityPlacementCommandFacts`, `entityGestureRelationshipMotionCommandFacts`, `forceStateCommandFacts.activePartyCommandFacts`; `map-script-engine`. | Handler facts and compact source-order evidence preserve the operations. Research owns the reviewed `abcs_battle08` / `cs_50806` program streams, inline actions, dialogue and lists. |
| [Separate request consumers](../../../../research/world/map-event-request-consumption.md#confirmed-consumer-topology) | [sf2-map-event-request-consumption-static-v1](../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json) `eventRequestConsumption.consumerContexts`; [sf2-common-maps-static-v1](../../../../../tests/fixtures/h2/common-maps-static-v1.json), [sf2-common-scripting-static-v1](../../../../../tests/fixtures/h2/common-scripting-static-v1.json); same basename commands. | BattleLoop's direct field read is distinct from exploration pending consumption. No executable composed natural chain is claimed. |

The corresponding [cutscene verifier](../../../../../src/sf2tool/h2/battle_cutscenes.py) and
[fixture schema](../../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json),
[map-init verifier](../../../../../src/sf2tool/h2/map_init.py) and
[fixture schema](../../../../../schemas/h2-map-init-static-fixture.schema.json),
[script verifier](../../../../../src/sf2tool/h2/map_script_engine.py) and
[schema composition](../../../../../src/sf2tool/h2/map_script_schema.py), and
[request-consumption verifier](../../../../../src/sf2tool/h2/map_event_request_consumption.py) retain the
exact comparison surfaces. The [map contract](../../../contracts/world/map-exploration.md#load-and-selection-order)
preserves path-dependent loads; the [setup contract](../../../contracts/world/map-setup-data.md#contract-boundary)
owns setup identity. These are evidence boundaries, not a new remake architecture or completion claim.

**Unknown, preserved:** natural first/repeat victory, loss/escape and arrival lineage; legal walking,
tunnel collision/access and treasure; live HP, party and actor/follower mappings; successful named
movement and independent camera/text/fade/sprite/audio/timing completion; field/request survival,
control readiness and save/load persistence. Controls that distinguish those seams include
slot-128-only death, zero Bowie HP, F508-set repeat, another EGRESS_MAP, F88 resume, dead/unjoined
Kiwi, conflicting followers, F700-clear voyage, F705-set skip and an overwritten pending request.
They are review counterexamples, not an automatic capture queue.

Expansion would require an accepted owner for the missing relation: starting flags/actors and
outcome, after-return fields and caller writes, selected Map73 init, reset, surviving request and
actual destination consumer. This slice completes neither the campaign nor epic #514. Chinese
synchronization remains deferred under the roadmap; subsequent castle content is outside this guide.
