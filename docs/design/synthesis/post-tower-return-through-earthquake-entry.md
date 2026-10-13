# Post-tower Return through Earthquake-town Entry

The tower aftermath leaves a changed Granseal ahead of the party. **Inferred narrative connection:**
the group returns through the tower and castle into the town's earthquake evacuation. **Confirmed
static content:** selected exits lead toward a town request, the earthquake flag changes its effective
map, and the town has distinct first and repeat entry programs. **Unknown:** a legal continuous walk,
naturally completed scene services and the actual pending-event value when initialization returns.

Read this after [Battle07 outcomes and the exploration handoff](battle07-outcomes-and-aftermath.md).
The accepted [research owner](../../research/post-tower-return-through-earthquake-entry.md) supplies
provenance, exact records and consumer order. This guide supports content/design planning and stops
at Map4's post-init control/event check. It adds no remake capability. The
[roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns the Standard-rigor
agreement for [#723](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/723).

`Fnnn` means flag nnn. Original-source references use `disasm/` at
[`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The research and fixtures bind the USA retail ROM in the
[root baseline](../../../README.md#pinned-original-baseline). Command indices in references are
zero-based script records, not elapsed frames. Dialogue below is summarized from the accepted review.

## 1. Leaving the Tower Is a Series of Conditional Field Requests

**Confirmed starting boundary:** completed first victory returns to Map58 `(13,35)`, Down, with
F74/F640 set by the after scene and F407 clear/F507 set by the battle caller. F999 must be clear to
retain Map58. The ordinary before scene clears F83; intro-skipped callers can retain it. Repeat
victory does not freshly establish the fixed position or follower/earthquake flags. The
[preceding return boundary](battle07-outcomes-and-aftermath.md#4-the-return-request-precedes-script-and-caller-completion)
continues to own those alternatives and the natural-combat/service limits.

**Confirmed selected connections:** Map58's outgoing marker requests Map60, the stairway; a selected
Map60 exit requests Map57, the tower entrance. The onward records request Map40, the exterior, then
castle Map21 **3F**, Map20 **1F**, Map19 **2F**, and finally Map3 `(29,2)`, Down. Each named record
uses a no-scroll warp. The [selected request review](../../research/post-tower-return-through-earthquake-entry.md#starting-state-and-outgoing-requests)
retains every trigger/destination tuple, including wildcard coordinates. These records describe
possible requests at separate consumers; they do not supply the walking path between them.

Ordinary field warps unwind exploration through its request consumer. At the main loop, `SwitchMap`
runs before `CheckBattle`, and exploration initializes only if no battle is selected. This differs
from the predecessor's direct battle-outcome return, which bypasses that encounter check.

**Confirmed interception:** Map57 can select Battle01 when F401 is set, regardless of coordinates.
F501 set clears F401 only after selection, so it does not prevent that encounter on the current call.
F401 clear is needed for this request to pass directly to exploration; F507 alone supplies no such
permission. No battle-coordinate row names the other selected maps in this chain, but that local
absence does not prove an encounter-free journey.

**Unknown walking:** Map60 has other exits and same-map portals; its arrival is not proof of reaching
the selected onward marker. Map57's F506 block copy overlaps the arrival area, and other enabled
copies, collision, occupancy and followers can affect traversal. Reverse exits or block-copy records
cannot substitute for reachability evidence. A Map58 detour has separate F407/F999 conditions.

## 2. The Castle Reflects the Aftermath without Replaying Earlier Scenes

**Confirmed setup rule:** the last set flag in source order wins setup selection. Under the
predecessor's F506/F507 state, with later F543/F982 overrides absent where applicable, the selected
content is:

| Place | Selected content and its meaning |
| --- | --- |
| Map60 stairway and Map57 entrance | Neither has a setup row. If no encounter intercepts the request, `ms_Void` supplies no fixed actors or setup init; generic flag-selected followers still initialize. |
| Map40 exterior | F507 restores the default setup after the F506 variant. Its fixed entity table is empty, so the earlier Lemon/guard population is not selected. |
| Map21 castle 3F and Map20 castle 1F | F506 routes select their later init targets, bypassing the older guard-position/bedroom programs. Their selected entity/zone events are default-only; a shared implementation filename does not select an older conversation. |
| Map19 castle 2F | The F507 setup declares one Astral sprite, reference 128, initially at `(63,63)`, Down. This setup actor is distinct from the flag-selected `FOLLOWER_B`. Its default entity event returns; two zone dialogues remain available at their own predicates. |

The four exterior/castle init functions check F507 and call the same short presentation program:
request boss-attack music, run `fadeInB`, then end. It writes no progression flag, warp, join or
follower row. This is an authored presentation request, not confirmation that its services finished.
Battle07 victory does not clear later setup flags, so this table cannot describe every arbitrary
reentry state. The [castle setup review](../../research/post-tower-return-through-earthquake-entry.md#selected-castle-setups-and-events)
owns the selected pointers and overrides.

### Astral's two warnings have independent repeat controls

**Confirmed authored dialogue and guards:** both Map19 zone programs address setup actor 128.

| Marker | First invocation with its flag clear | Repeat behavior |
| --- | --- | --- |
| `(16,5)`, F622 | Astral warns about the quake and urges escape; the handler sets F622 after the script returns. | F622 set suppresses this dialogue. |
| `(25,13)` or `(25,14)`, F623 | Astral declares the group safe; the handler sets F623 after the script returns. It does not test F622. | F623 set suppresses this dialogue independently. |

These lines do not change followers, F74/F83/F640, combat HP, inventory or force membership. The south
warp, map substitution and town init do not read F622/F623, so neither dialogue is an explicit
progression prerequisite at those consumers. **Unknown:** whether a legal walk must cross a marker.
Absence of a flag prerequisite does not establish physically optional content. Astral's reassurance
also supplies no independent proof of every character's survival.

## 3. The Earthquake Flag Selects the Town before Its Actors Are Rebuilt

**Confirmed map selection:** the castle's south exit requests Map3 `(29,2)`, Down. With F640 set,
`SwitchMap` substitutes Map4 while retaining those coordinates and facing. F640 clear retains Map3;
F507/F622/F623/F624 are not predicates for this substitution. A direct Map4 request needs no such
substitution. Map4 always selects `ms_map4`; its first/repeat decision happens inside initialization.

**Confirmed population order:** exploration clears the pending event on entry, heals living/immortal
allies, fades, assigns the current map and non-battle state, then loads tilesets and initializes
entities. Initialization clears the previous population, declares flag-selected followers, reads
fixed setup actors, and declares Bowie at the supplied entry position with an idle script. Map4's
fixed actors are guards 128/129 at `(28,7)` / `(29,7)`, Up. These are new-map declarations, not old
scene IDs surviving the journey.

F74 enables Astral `FOLLOWER_B`, king `FOLLOWER_A` and minister `FOLLOWER_D`. Selected content does
not clear F74. Other enabled rows remain inputs: intro-skipped F83 can add royal rows, and conflicting
`FOLLOWER_B` declarations can change the mapping addressed by the script. The initializer promises
neither deduplication nor a fixed three-follower total; town init does not repair absent/conflicting
followers. Following is separate from force recruitment or combat HP.

Exploration then clears temporary flags 256–383, sets F80, loads the map and installs base VInt
functions before setup init. F74/F507/F622/F623/F624/F640 survive this temporary clear. Flag-selected
layout copies run during load, before the init script. The
[entry/population review](../../research/post-tower-return-through-earthquake-entry.md#effective-map4-entry-and-population)
and [map-load contract](../contracts/map-exploration.md#load-and-selection-order) own this distinction.

## 4. First Entry Stages the Guards and the Widening Cracks

**Confirmed first-entry order:** F624 clear selects the full town scene. It begins by placing living
Bowie at `(29,4)`, Down, and follower B/A/D at `(28,3)` / `(29,3)` / `(30,3)`, Down. These are scene
placements that replace arrival positions, rather than new warp destinations. The placement handler
sets position, destination and facing; it skips dead force members and depends on initialized
mappings for non-force followers.

The scene proceeds in four readable phases:

| Phase | Authored content and state boundary |
| --- | --- |
| Arrival amid the quake | Quake amount 1, fade-in and waits stage the party outside. Completion of presentation services remains unobserved. |
| The guards explain the danger | Guards greet the king/Astral. Astral moves down one tile and asks why they remain; they report chasms and people aboard a ship. He urges departure, and a guard calls for help. Different guard speeds precede non-waiting movement: 128 down, 129 right. Their report is dialogue, not an observation of the docks. |
| The cracks widen around the guards | Rumble, quake amount 3 and waits accompany three successive `11x7` working-layout copies into `(24,3)`. Guard 128 receives the falling action, then a wait of 5 precedes assigning it to guard 129 with an explicit action-script Wait. |
| Astral points toward the harbor | Rumble and three `5x4` copies into `(27,0)` complete the second change. Quake returns to 1; Astral faces Right and directs Bowie via the east side. `FOLLOWER_B` receives `eas_Follower1`, then the script ends. Only after it returns does the init caller set F624. |

**Confirmed actor boundary:** the falling action moves a guard, requests falling audio, turns/shrinks
its sprite, waits for destination, parks it at `(0,0)` and returns to idle. Guard 129 is explicitly
awaited; there is no equivalent next-command join for 128. These operations write no combat HP/death
or force membership. **Inferred narrative reading:** the scene depicts guards falling into the
cracks. **Unknown:** concurrent motion, wait termination, exact final actor state, perceived deaths
and completed audiovisual delivery. A narrated or animated fall is not a battle casualty rule.

The six town text commands consume ordinary unskipped IDs 715–720; the castle dialogues use 713/714.
The [first/repeat research review](../../research/post-tower-return-through-earthquake-entry.md#first-and-repeat-initialization)
retains text provenance, source positions and command order without a transcript.

## 5. Repeat Entry Reconstructs the Changed Layout and Skips the Scene

**Confirmed repeat branch:** F624 set selects a short program that parks both guards at `(0,0)`,
Right, then ends. Before that init runs, the map load has already applied the F624-selected final
`5x4` and `11x7` copies at the destinations described above. Persistent flag-selected reconstruction
on entry differs from the first scene's animated working-layout changes; neither proves a legal
route through the resulting map or cross-process save persistence.

The repeat program does not reposition Bowie or followers, display text, copy blocks, set quake or
request another event. For the named south-exit request, Bowie therefore retains `(29,2)`, Down
before control; another supplied entry retains its own tuple. No quake command here proves zero
amplitude: the caller's quake state and visible presentation remain separate. F624 set selects the
branch without proving this execution previously completed the first scene.

## 6. Control Returns Only If the Post-init Event Read Allows It

**Confirmed conditional handoff:** neither town init program explicitly produces another warp or
zone event. After init returns, exploration conditionally handles music/fade and enters
`WaitForEvent`. If `MAP_EVENT_TYPE` is zero, it follows entity 0 and installs its controlled action
script. The predecessor's ordinary `PLAYER_TYPE=0` selects `eas_ControlledCharacter`; arbitrary
raft/caravan caller input is not silently normalized. If an event is pending, control installation
is skipped and the event passes to `ProcessMapEvent`.

**Unknown:** the actual pending value at that read and natural input readiness. Clearing the event
on entry does not prove it remains zero after services and scripts. The separate static consumers
do not executable-close this complete caller composition. This conditional read is the stopping
boundary, rather than a claimed continuous playable return.

Map4's default zone and entity handlers simply return. The generic zone-event consumer still closes
text/portrait windows, waits a VInt and waits for the main entity to stop. The next outward owner is
[Map4's warp table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map04/6-warp-events.asm),
identified in the [accepted handoff](../../research/post-tower-return-through-earthquake-entry.md#immediate-controlevent-boundary-and-next-owner).
Its east-edge records point toward a docks request. Consuming that request, reaching the docks,
boarding/departure, Battle08 and later destination content require their own accepted owners and
remain outside this guide.

## Evidence Review and Remaining Limits

The pre-synthesis review read accepted #720 and its predecessors, the preceding guide, relevant
contracts, selected tracked fixture payloads and schema/verifier boundaries. The research owner
retains pinned-source/text review and completed comparison receipts. This guide performs no fresh
source investigation, extraction, H2/H3 run or runtime observation. A local attempt to read the old
derived script payload found it absent; the tracked fixtures were reviewed instead. No retained
comparison is reported as rerun here, and the absence does not invalidate accepted research.

| Relation | Reviewed executable owner and reproduction route | Review disposition |
| --- | --- | --- |
| Field requests, substitution and interception | [map-content fixture](../../../tests/fixtures/h2/map-content-static-v1.json), `sf2-map-content-static-v1`; [common-maps fixture](../../../tests/fixtures/h2/common-maps-static-v1.json), `sf2-common-maps-static-v1`; [battle-global fixture](../../../tests/fixtures/h2/battle-global-data-static-v1.json), `sf2-battle-global-data-static-v1`. Commands `uv run sf2 h2 map-content`, `common-maps`, `battle-global-data`. | Separate table/consumer scope agrees with the accepted selected records. Exact route composition uses research; no walking or encounter-free journey is established. |
| Setup, population and init branch | [setup fixture](../../../tests/fixtures/h2/map-setup-static-v1.json), `sf2-map-setup-static-v1`; [entities fixture](../../../tests/fixtures/h2/map-entities-static-v1.json), `sf2-map-entities-static-v1`; [init fixture](../../../tests/fixtures/h2/map-init-static-v1.json), `sf2-map-init-static-v1`. Commands `map-setup`, `map-entities`, `map-init` under `uv run sf2 h2`; [init verifier](../../../src/sf2tool/h2/map_init.py) and [schema](../../../schemas/h2-map-init-static-fixture.schema.json). | Selected `primarySourceBodies` preserve Map19's F507 check and Map4's F624 read → first script → caller flag write versus repeat script. Selector/actor identities remain separate from natural reachability. |
| Castle dialogue and town default events | [zone programs](../../../tests/fixtures/h2/map-events/zone-programs.json), orders 33/34/93, and [entity programs](../../../tests/fixtures/h2/map-events/entity-programs.json), order 568, under `sf2-map-events-static-v1`; [verifier](../../../src/sf2tool/h2/map_events.py), [shard/schema owner](../../../src/sf2tool/h2/map_events_fixture.py). Command `uv run sf2 h2 map-events`. | F622/F623 each follow their own script return, with no F622 prerequisite on F623; Map4 handlers return. Marker invocation is not proved. |
| Scene commands, actor waits and copies | [script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `sf2-map-script-engine-static-v1`; [verifier](../../../src/sf2tool/h2/map_script_engine.py), [schema owner](../../../src/sf2tool/h2/map_script_schema.py). Command `uv run sf2 h2 map-script-engine`. | Placement/action/presentation site keys and block-copy facts preserve selected programs and the six first-scene copies. `cs_51750` contains no story-flag command; F624 belongs to init. Full dialogue/scene order uses accepted research, not a new observation. |
| Event reset/poll/consume and control read | [request fixture](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `sf2-map-event-request-consumption-static-v1`; [verifier](../../../src/sf2tool/h2/map_event_request_consumption.py), [schema](../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json). Command `uv run sf2 h2 map-event-request-consumption`. | Named accesses and local consumers agree; the fixture explicitly retains actual consumer entry/read and normal-story reachability as Unknown. No natural post-init value is asserted. |

The [map contract](../contracts/map-exploration.md) owns setup selection and load/copy order; the
[setup-data contract](../contracts/map-setup-data.md#contract-boundary) owns static pointer/entity-list
identity, excluding broader population claims. The
[party/script contract](../contracts/party-roster-state.md#handler-boundary) keeps actor/follower
operations separate from recruitment, combat death and persistence. The accepted research supplies
selected follower rows and scene consumers rather than those contracts becoming a second route ledger.

**Unknown, preserved from the predecessors:** natural battle outcomes and caller state; legal
walking and changed-layout reentry; party/HP/actor/follower state at natural consumers; service,
audio/fade/motion completion and timing; actual post-init pending state; and save/load persistence.
These gaps neither erase supported static content nor authorize an emulator queue. Future composed
claims must reuse the owning request/init/script/follower surfaces and name the missing relation.
Original static content and inferred story meaning remain separate from remake implementation and
future product decisions; this segment completes neither the campaign nor the design epic.
