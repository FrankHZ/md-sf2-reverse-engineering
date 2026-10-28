# New Granseal Town: Peter's First Introduction

After the castle's year-later address, the next town scenes draw attention to a large bird and a
crowd. Children chase and threaten Peter, discover that he can speak, and receive a rebuke from the
minister. The minister apologizes, welcomes Peter to the new settlement and invites him to meet the
king. **Inferred narrative meaning:** an unfamiliar visitor moves from being a spectacle to being
received as a guest. **Confirmed static behavior:** two independent zone callers stage these scenes
and mark their own completion after script return. **Unknown:** natural travel, completed actor and
presentation work, and actual usable control.

Read this after [New Granseal castle: the year-later address and first control handoff](new-granseal-castle-year-later-control.md).
The accepted [research owner](../../research/new-granseal-town-through-peter-introduction.md) supplies
the source, dialogue, population and handler review. This guide stops at the first pending/control
read after Peter's town event and caller F701. The castle audience, later town services, overworld
and battles require separate accepted owners. It adds no remake capability. The
[roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns the Standard-rigor
agreement for [#747](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/747).

`Fnnn` denotes flag nnn. Source identities use `disasm/` at
[`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Research and fixtures bind the USA retail ROM in the [root baseline](../../../README.md#pinned-original-baseline).
Script command indices identify zero-based source records, not frames or measured durations.

## 1. Leaving the Castle Requests a Separate Town Visit

**Confirmed conditional connection:** castle Map7's south-exit row matches Y=23 with wildcard X
and requests town Map6 `(11,1)`, Down, using a no-scroll warp. The movement consumer tests this row
only on the selected warp layout attribute after earlier movement/collision work. Wildcard X does
not make every tile on that row walkable. Neither the row nor the predecessor's control boundary
proves a natural walk to the exit.

The predecessor's normally completing first address leaves F707/F411 set and `EGRESS_MAP=6`;
repeat castle entry preserves supplied egress/F411. The south exit tests none of these values.
Setting egress alone never moves the player into town. A matching movement request must survive
in the single mutable pending slot until exploration consumes it. The consumer clears the pending
word before dispatch, idles main and returns the explicit town tuple to `MainLoop`.

Map6 has no flag-substitution or battle-coordinate row, so this admission keeps Map6 and selects
exploration, independently of carried F411. Exploration clears pending, heals living/immortal
allies under the [shared HP/status limits](../../research/battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order),
sets map 6 and not-in-battle state, selects/rebuilds entities and loads sprites, clears temporary
flags 256–383, sets F80, loads the map and runs init before the first control read. Persistent story
and force/follower history remain supplied inputs; this is not a full-force reset.

## 2. Town History Selects the Cast; Init Adjusts It Separately

**Confirmed setup priority:** Map6 scans default, F701 and F702 in order. The last applicable row
wins, so F702 takes priority over F701. All three setups share the same zone table and init function,
while their populations differ.

| Supplied history | Selected population |
| --- | --- |
| F701/F702 clear | Default `ms_map6_Entities`: Peter plus 19 NPC declarations, including the children, sailor/guard and minister's group. |
| F702 clear, F701 set | `ms_map6_flag701_Entities`: 14 NPC declarations, without fixed Peter or minister/escort records. |
| F702 set, any F701 | `ms_map6_flag702_Entities`: 14 NPC declarations, also without those fixed records; later-history selection only. |

The default list places `ALLY_PETER` (ally ID 7) at `(18,25)`, Up, with a horizontal-loop action.
Boys 136/137/140 surround that area at `(16,25)`, `(21,25)` and `(19,28)`. Reference 138 has a worker
sprite at `(10,12)`, Right; 139 has a guard sprite at `(11,12)`, Left. The sailor/soldier description
comes from the caller comment and narrative, not a separate physical-slot identity. Minister 144,
guard 145 and worker 146 begin at `(63,63)`, Down, ready for explicit scene placement. The other
nearby townspeople provide the crowd. The [population owner](../../research/new-granseal-town-through-peter-introduction.md#setup-ordinary-population-and-init)
retains the full declaration list.

These references resolve through the current character-to-entity mapping. Initialization creates
flag-selected followers first; an already mapped ally skips its fixed declaration. Thus a supplied
Peter follower can change the entity and starting position used by this scene. NPC references map
separately from physical allocation. A fixed Peter entity is not evidence of force membership.

**Confirmed init priority:** `ms_map6_InitFunction` tests F802 before F706 and writes no flags.
It uses the mappings provided by whichever population was selected:

| Init history | Adjustment before control |
| --- | --- |
| F802 set | Place priest reference 132 at `(11,18)`, Down, then return without testing F706. |
| F802 clear, F706 set | Place 138 at `(63,63)`, Down, and 139 at `(15,23)`, Down. |
| F802/F706 clear | Return without a script. |

This init does not start either long scene, change egress, recruit anyone or reset F701/F702.
Positioning an actor at `(63,63)` retains its mapping; it is distinct from the explicit hiding
operation later in the scenes. The guard's re-entry placement also differs from his first-event
movement result.

## 3. The Sailor and Guard Introduce the Disturbance

**Confirmed independent dispatch:** zone coordinates `(10,9)` or `(11,9)` select
`Map6_ZoneEvent0`. With F706 clear it runs `cs_54E86`, sets F706 only after normal return, then
returns directly. With F706 set it skips that introduction. A separate later-history greeting can
be selected by F802/F803 on the repeat path; its body is outside this guide. The first execution
does not fall through into that greeting.

The guard and sailor discuss a large bird and a gathering crowd. The guard decides to investigate
and asks the other to inform the king. His individually awaited movements take him toward the
crowd, with jumps and facing changes interspersed with waits. The script also starts a horizontal
action without joining it. Later, it explicitly places living Bowie at `(10,9)`, Down, brings
138 a step upward and delivers the report about the animal. It then awaits 138's departure path
and hides him before returning to the caller.

Under default mappings and completing services, the guard's movement operands from `(11,12)` sum
to `(14,25)`, Right. This is a **Confirmed conditional source result**, not an observed endpoint.
It is not the re-entry init tuple `(15,23)`, Down. Bowie's position command has an ally-HP guard:
zero-current-HP Bowie skips it. Waiting for an action joins only that named entity's idle script;
a timed pause is not an independent join of an earlier unawaited action.

After return and caller F706, the zone wrapper closes portrait/text, waits for VInt and main to
stop moving, and returns to exploration's pending/control read. The program does not call Peter's
scene. Reaching its separate zone requires another dispatch; natural movement between the two
zones remains **Unknown**.

## 4. Peter's Reception Has Its Own Trigger

**Confirmed predicate:** zone `(19,23)` selects `Map6_ZoneEvent2`. F701 clear runs `cs_54F26`;
F701 set skips it. There is **no F706 prerequisite**, nor a test of F707, F411, F702, F802,
membership or HP. The coordinate is a dispatch parameter, not proof of Bowie's actual tile.

This separation matters to content planning. F701-clear/F706-clear can select Peter's event without
having run the introduction. F702-set/F701-clear can select it with a later population lacking
its fixed actors; F701-set/F706-clear can still select the first scene with the variant population.
Those are supplied-state counterexamples to assuming enforced story order or actor availability.
They do not establish naturally reachable save histories.

### The children discover that Peter can speak

The opening gives Peter and boys 136/137/140 inline speed operands `(48,48)` and disables Peter's
auto-facing. The boys take turns approaching; Peter steps away from their attempts. Their movements
start without joins, while Peter's responses are individually awaited. A boy remarks on his speed.
Peter turns and protests, and the boys react to the discovery that he can speak and objects to their
treatment.

The exchanges escalate into a narrated threat of hitting him. The boys start further unjoined
approaches; Peter receives an awaited init and jump-left action, then the minister speaks.
**Confirmed distinction:** that threat has no accompanying damage or HP instruction. Speed operands
are not measured velocity. Peter's earlier init is followed by speed-48 restoration; the init before
the jump is not, so the opening speed is not a permanent property of Peter or the entire scene.

### The minister changes how the town receives him

The program requests a quake, turns the crowd toward the interruption, stops the quake and explicitly
places minister 144 and escorts 145/146 above the gathering. The minister and guard begin unjoined
steps while only worker 146's path is awaited. The minister rebukes the children; their retreat also
starts boys 136/137 without joins and awaits only 140. The camera shifts toward the exchange, the
minister advances, and woman 130 receives her own awaited movement.

The dialogue then gives the apology, welcome and minister's introduction, explains the new
settlement, includes Peter's responses and invites him to meet the king. **Inferred:** his speaking
and the minister's intervention turn a hostile spectacle into a formal welcome. The ordered source
commands confirm the staged exchange, but do not establish completed camera, quake, text, sound or
motion services and their timing.

### Departure staging does not establish castle arrival

The minister starts an unjoined Up-eight path while Peter's corresponding path is awaited. The
escorts turn inward, the camera returns to neutral, and the final departure starts the minister,
Peter and guard without joins, awaiting only worker 146's distinct path. The script then hides
144, 145, 146 and Peter, awaits woman 130's return step, faces her Down and ends.

**Inferred:** the invitation and movement stage Peter following the minister toward the castle.
**Unknown:** completed castle travel. Neither early scene contains a warp, map load, population
reload, recruitment, force reset or persistent follower-selection write. The castle audience is a
separate outcome beyond this endpoint.

## 5. Hiding Ends the Staging before the Caller Returns to Control

**Confirmed handler distinction:** action/init handlers use the current mapping and do not skip
a dead ally. Their waits test the named actor's idle script, not every earlier actor. Position and
facing handlers instead skip zero-current-HP ally references; NPC references bypass that guard.
Peter's supplied mapping and HP can therefore change poses without suppressing the zone caller.

`HideEntity` relocates current and destination X/Y to `$7000`, removes matching index mappings and
removes a matching entry from the current exploration follower list. It does not change HP, clear
force membership or clear persistent follower-selection flags, and it does not join the hidden
actor's action. A supplied Peter follower may leave the **current** chain through hiding; that is
not persistent dismissal, death or recruitment. Nor does the final awaited escort certify that
the three unjoined departures completed before hiding.

At normal script return, current map remains 6. Peter's program gives Bowie no position command;
his explicit facing commands are Up then Left, subject to the HP/mapping guard. No natural leader
endpoint is claimed. The caller sets F701 only now. **Setting F701 does not immediately replace
the population:** the current actors/actions and explicit hiding remain. Future entry selects
F701's list unless F702 wins, then reruns init. That list's guard declaration `(13,24)`, Up, and
F706-set init's override `(15,23)`, Down, are re-entry operands, not retroactive scene results.

After caller F701, `RunMapSetupZoneEvent` closes portrait/text, waits for VInt and main to stop,
and returns through the type-6 handler. Exploration sets movement sound and calls `WaitForEvent`.
The **first post-event pending read** is this guide's stopping boundary:

| State at that read | Immediate static consequence |
| --- | --- |
| `MAP_EVENT_TYPE=0`, carried `PLAYER_TYPE=0` | Set view target to physical entity 0 and install `eas_ControlledCharacter`; the following loop reads pending again and tests A/C. |
| Zero pending, supplied nonzero player type | Select the shared vehicle-control branch. The scene does not normalize player type. |
| Nonzero pending | Skip control installation; if still nonzero at the secondary read, return the event for dispatch. No particular producer or next event is attributed to these scenes. |

Pending was cleared before zone dispatch, and neither early program nor caller produces a request.
Without an intervening write, zero selects ordinary control. This is a conditional static result,
not an observation of usable input. There is no second exploration init, heal or pending clear
between caller F701 and that read. F707/F411/egress carry through; F706 changes only through its
separate event. A script/service stall can leave its caller flag unreached.

## Evidence Review and Remaining Questions

**Confirmed review disposition:** the accepted research, predecessor, shared contracts and tracked
fixture selections agree on the conditional chain. Init body 10 preserves F802-before-F706 priority;
the zone-program shard preserves the independent F706/F701 tests and post-return writes. Script
witnesses separate named waits, unjoined actions, placement guards and hiding. No conflict requires
an owning research or contract correction. Town-specific cast, dialogue, inline actions, exact setup
rows and `HideEntity`'s helper effects rely on the accepted research's pinned-source review; compact
fixtures do not reproduce the entire scene or establish natural play.

The reviewed fixtures have the exact IDs below. Narrow commands use prefix `uv run sf2 h2` and name
existing executable owners; **they were not rerun for this documentation slice**. Scene-specific
explanation consumes accepted research; no new source investigation or runtime observation is claimed.

| Relation | Tracked selection / narrow command | Limit |
| --- | --- | --- |
| Setup, actors and init | [sf2-map-setup-static-v1](../../../tests/fixtures/h2/map-setup-static-v1.json), `expected.sourceFacts` / `selectionCases`; [sf2-map-entities-static-v1](../../../tests/fixtures/h2/map-entities-static-v1.json), `expected.consumerFacts`; [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), `expected.primarySourceBodies[10]`; `map-setup`, `map-entities`, `map-init`. | Shared selection/mapping facts and the eight-operation Map6 init. Selection cases do not provide a dedicated Map6 history witness. |
| Independent zone callers | [sf2-map-events-static-v1](../../../tests/fixtures/h2/map-events-static-v1.json), [zone-program shard](../../../tests/fixtures/h2/map-events/zone-programs.json), `expected.zoneTargetPrograms` `Map6_ZoneEvent0` / `Map6_ZoneEvent2`; `map-events`. | Exact caller operations, not collision/layout or natural zone reach. |
| Actions, poses and hiding | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `expected.entityActionBridgeCommandFacts`, `entityPlacementCommandFacts`, `entityLifecyclePresentationCommandFacts`; `map-script-engine`. | Source-site keys include `cs_54E86:21:setActscript`, `cs_54E86:33:hide`, `cs_54F26:138:entityActions` through `141:entityActionsWait`, and hides 142–145. Handler guards do not prove completed departures. |
| Pending and control | [sf2-map-event-request-consumption-static-v1](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `eventRequestConsumption.consumerContexts.waitForEvent` (`wait-for-event`); [sf2-common-maps-static-v1](../../../tests/fixtures/h2/common-maps-static-v1.json), [sf2-common-scripting-static-v1](../../../tests/fixtures/h2/common-scripting-static-v1.json); same basename commands. | Shared consumers, without an executable composition from town admission through actual usable control. |

The [map-init verifier](../../../src/sf2tool/h2/map_init.py) and
[fixture schema](../../../schemas/h2-map-init-static-fixture.schema.json),
[script verifier](../../../src/sf2tool/h2/map_script_engine.py) and
[schema composition](../../../src/sf2tool/h2/map_script_schema.py), and
[request-consumption verifier](../../../src/sf2tool/h2/map_event_request_consumption.py) retain the
comparison boundaries. The [setup contract](../contracts/map-setup-data.md#contract-boundary) and
[map exploration contract](../contracts/map-exploration.md#load-and-selection-order) govern shared
entry semantics. The [research reproduction owner](../../research/new-granseal-town-through-peter-introduction.md#coverage-counterexamples-and-reproduction)
retains source anchors and the missing composition; no new comparator is needed for this guide.

**Unknown, preserved:** natural exit/zone travel and reachable setup/follower history; actual mappings,
HP, obstruction and action completion; display/camera/text/audio timing; pending survival or overwrite,
player-type history, actual usable control and save persistence. Independent second dispatch without
F706, F702-selected missing actors, a dead or already mapped ally, an unfinished action at hiding,
a service stall and a nonzero pending/player-type state constrain the interpretation. They do not
create an automatic runtime-acquisition queue.

This guide completes neither campaign chronology nor epic #514. Chinese synchronization remains
deferred under the roadmap; later audiences and routes require their own accepted evidence.

Continue with [Peter's castle audience and the return to town](peter-castle-audience-through-town-return.md)
for the separate audience predicate, Peter's joining/following, the narrated gift and the conditional
town re-entry pending/control boundary.
