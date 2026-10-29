# Earthquake-town Exit through Docks Departure

Leaving the earthquake town leads toward a harbor whose population is selected by the aftermath
flags. **Inferred narrative connection:** the royal group prepares to evacuate, chooses to depart,
and watches Granseal's destruction as the ship leaves. **Confirmed static content:** the selected
town exits request Map44; a specific old-man entity offers the departure choice; Yes runs an ordered
scene that clears follower/earthquake flags and produces a Map73 request. **Unknown:** legal walking,
natural interaction, completed scene services and the request actually read by the outward consumer.

Read this after [Post-tower return through earthquake-town entry](post-tower-return-through-earthquake-entry.md).
The accepted [research owner](../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md) supplies
provenance, selected records, dialogue review and consumer order. This guide supports content/design
planning and stops at the conditional Map73 request handoff to the main loop. It adds no remake
capability. The [roadmap](../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns
the Standard-rigor agreement for [#727](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/727).

`Fnnn` means flag nnn. Original-source references use `disasm/` at
[`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The research and fixtures bind the USA retail ROM in the
[root baseline](../../../../../README.md#pinned-original-baseline). Command indices below are zero-based
script records, not frames or measured durations. Dialogue is summarized from accepted research.

## 1. The East Exit Requests the Changed Harbor

**Confirmed selected connection:** Map4's `(50,23)`, `(50,24)` and `(50,25)` east-edge records each
request Map44, `MAP_GRANSEAL_DOCKS_AND_SHIP`, at `(1,25)`, Right, without scrolling. These records
continue the predecessor's conditional town-control boundary; they do not prove a route across the
changed town layout. The ordinary first-victory lineage supplies F507/F74/F640 and clears F83 before
the battle. Repeat victory, skipped intro and externally supplied flags retain their own caller limits.

The field request unwinds exploration, then `MainLoop/@Start` calls `SwitchMap` before `CheckBattle`.
Map44 has no substitution row and no encounter row naming 44 in the first-column map field. It
therefore passes this selected request to exploration. A 44 elsewhere in the encounter table is a
dimension, not another encounter predicate. This local result does not establish the natural journey.

On explicit Map44 entry, exploration clears the pending event, heals living/immortal allies, loads
selected content and initializes a fresh population before setup init. Map44's flag/step-event tables
contain only terminators; they provide no authored layout-copy evidence for a boarding path.
The [entry review](../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md#town-exit-and-map44-admission)
owns the producer, no-scroll consumer and admission order.

**Confirmed setup priority:** Map44's setup rows are default, F609, F506, F507. The selector scans all
flag rows and keeps the last set one, so F507 wins even with F609/F506 set. Without F507, F506 wins
over F609; otherwise F609 or default applies. F507 is a content selector, not proof that the evacuation
or its prerequisites completed.

Its init calls `cs_54570`: request boss-attack music, `fadeInB`, end. It falls through to the return
also named `ms_map44_flag609_InitFunction`; default/F609/F506 enter that return directly. The source
owner is `s6_initfunction_507.asm`. There is no first-entry latch, actor placement or warp in this
short init. F507 reentry repeats it. Exploration's own music/fade path remains palette-conditional;
the calls do not establish the final audible or visible result.

F74 enables Astral (`FOLLOWER_B`), King Granseal (`FOLLOWER_A`) and the Minister (`FOLLOWER_D`) during
entity reconstruction. Map44 permits these flag-selected followers. F74 absent omits this group;
F68/F70 can also use B, F77 uses A, and F83 uses A/D after F74. Additional declarations can overwrite
an addressed follower mapping. First-victory F83-clear state matters without proving every other flag
clear, and this scene repairs no absent/conflicting mapping. Followers are current actors, not new
force recruits; F507 alone does not establish the speakers the scene expects.

After init, `WaitForEvent` installs entity 0 control only if the pending word reads zero. Ordinary
`PLAYER_TYPE=0` selects character movement; arbitrary raft/caravan input is distinct. A pending event
bypasses control installation. The earlier clear and lack of an explicit init producer do not prove
that later read zero. These [setup/follower/control boundaries](../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md#setup-followers-and-conditional-control)
remain conditions on the content explanation.

## 2. Boarding Preserves Actors; Leaving and Reentering Rebuilds Them

**Confirmed authored boarding records:** three no-scroll portals connect the dock markers to ship
markers, and three return portals connect them back:

| Trigger positions | Requested positions and facing |
| --- | --- |
| `(12,11)`, `(13,11)`, `(14,11)` | Respectively `MAP_CURRENT` at `(52,8)`, `(53,8)`, `(54,8)`, Up |
| `(52,9)`, `(53,9)`, `(54,9)` | Respectively `MAP_CURRENT` at `(12,11)`, `(13,11)`, `(14,11)`, Down |
| West edge `(0,255)` | Map3 `(49,24)`, Left; Y=255 is a wildcard |

`MAP_CURRENT=255` selects the same-map path. Encounter checking uses `CURRENT_MAP`, so the Map44
absence still applies. Exploration updates Bowie's position/facing and calls `DeclareRaftEntity`,
which can reposition followers and conditionally declare a raft. It skips fresh setup-population
construction and temporary-flag clearing, while still loading current map properties and running
setup init. Thus boarding retains the population and repeats F507's music/fade request; it does not
promise identical actor coordinates.

Leaving west and returning via an explicit Map44 request reconstructs the selected population.
While F640 is set, the west request for Map3 becomes Map4; with F640 clear it remains Map3. Those
return-init details stay with the predecessor. Same-map boarding is not an explicit Map44 reentry.

**Unknown traversal and occupancy:** the F507 population includes fixed actors at all three dock
trigger coordinates. Their overlap is a concrete reason not to infer free passage from warp records.
Collision, follower state, walking and service completion remain unproved. The
[boarding/interaction review](../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md#boarding-records-and-actual-entity-interaction)
owns these records and their separate consumers.

## 3. The Departure Choice Belongs to the Old Man

**Confirmed interlocutor:** the eleventh fixed NPC record in the F507 population is an old-man sprite
at `(10,14)`, Right, `eas_Init`, mapped to event index **138**. Worker 137 at `(20,12)` is the preceding
record, and guards 139/140 follow. `Map44_1FB_EntityEvent7` is the handler for index 138; its suffix is
neither NPC slot 7 nor event index 7, and it is not bound to the worker.

The event table's `UP` byte has value 1 and is interpreted as behavior bits: turn toward the player
before the handler, with no bit-1 facing restoration afterward. It imposes no requirement to stand
south or face Up. The ordinary C-button path still needs `GetActivatedEntity` to select an actor from
facing/position, counter-block and follower-exclusion inputs, then `GetEntityEventIndex` supplies the
table index. The consumer suspends entity-update VInt processing around the handler and later closes
windows/reactivates updates. A record mapping proves neither natural activation nor wait completion.

The handler greets the royal group (1008), selects follower B's portrait/speech for the question
(1009), then calls `j_YesNoPrompt`, whose effective target is `YesNoPrompt`. Nearby evacuation text
reports damage, urgency and crowding; it is not a formal capacity rule. The handler adds no F74,
F640, inventory or party-readiness predicate.

| Choice | Confirmed input/result and local consequence |
| --- | --- |
| Yes | Initially selected result 0; Left selects it, A/C confirms. Zero runs `cs_54578`, then reaches the common handler return. |
| No or cancel | Right selects No; A/C confirms, or B produces the same -1 result. Nonzero displays 1010 asking for a prompt return, then reaches the common return without the departure script or its F74/F640 clears. |

No and cancellation are indistinguishable at this return. Normal cleanup returns through
`ProcessPlayerAction`; control resumes only if the next pending-event read is zero. Declining supplies
no guarantee of immediate input readiness, and acceptance supplies no natural readiness check on the
followers. The [choice review](../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md#departure-choice-and-ordered-script)
retains the prompt and both branches.

## 4. Acceptance Stages Departure in Distinct Phases

**Confirmed ordered content:** `cs_54578` progresses through the following authored phases.
**Inferred narrative meaning:** the group boards, discusses leaving for the mainland and departs as
the kingdom is destroyed. The ordered commands do not establish a completed runtime performance.

| Phase | Content and state consequence |
| --- | --- |
| Give the order and move aboard (0–8) | Astral agrees; actor 138 nods/faces Down and gives the order. Bowie is sent to `(11,13)` with a wait, guards 139/140 start unjoined northward action lists, and Bowie's inline movement list is awaited. The embedded `Map21_DefaultZoneEvent` label is action data, not a scene transfer to Map21. |
| Change the dock scene (9–30) | Face Bowie and B/A/D Down; target the camera at `(10,11)`. Eight `3x1` block strips copy from x15 to x12, y22 down through y15, with waits between and afterward. These mutate working map blocks and mark both view planes for update; they are not progression flags. |
| Discuss the destination (31–46) | Camera `(13,12)`; the Minister asks where they are going, Astral proposes the mainland/Parmecia, and the King objects because of Elis. Astral urges escape. D's two jumps are awaited; B's later movement is unjoined. Camera `(11,12)` precedes the last line. Dialogue supplies the proposal, not proof of arrival. |
| Leave the royal-following state (47–50) | Clear F74, camera `(11,13)`, then call `csub_54714` and wait 120. The subroutine decrements layer-1 autoscroll X 16 times and increments layer-2 X 24 times, each with `Sleep(4)`. These iteration counts do not establish measured displacement or presentation duration. |
| Replace the map and actors (51–57) | Load Map66 with view operands `(28,26)` inside the script; rebuild actors from `ce_54736`; await Bowie initialization, replace his sprite with ship, fade in, await the speed/idle action, then start an unjoined ship path. |
| Depict destruction and produce the request (58–76) | Waits accompany quake values 3, 5, 3, 1, 0, lightning SFX, two white flashes and a `10x15` block copy from `(0,49)` to `(28,23)`. Finally request Map73 `(26,13)`, Right, then clear F640 and end. No explicit join waits for the ship path before that request; no further dialogue or explicit music command appears. |

**Confirmed flag/actor distinction:** clearing F74 at command 47 changes later follower declarations;
it does not remove already allocated royal actors there. Command 52 explicitly rebuilds entities
through `InitializeMapEntities` using `ce_54736`: main entity `(34,33)`, Right, with no fixed NPCs.
F74 is then clear for follower processing. Other enabled flags can still declare followers, so the
empty fixed list does not prove Bowie is the only entity. Replacing his sprite with ship neither
changes force membership nor itself selects a vehicle `PLAYER_TYPE`.

**Confirmed map-load distinction:** `loadMapFadeIn` at 51 initiates fade-out, loads tilesets, waits for
fading, loads Map66 and restores interrupt/display processing. `LoadMap` writes `CURRENT_MAP=66`;
`(28,26)` are view operands, while the entity table supplies Bowie's `(34,33)` separately. This
in-script load bypasses the main loop's substitution/encounter check and does not run Map66 setup
init. The later `fadeInB` explicitly requests fade-in. F640 remains set through this Map66 sequence
until command 75. Final ship position, concurrent motion, quake/flash/block appearance, audio and
wait completion remain **Unknown**.

## 5. The Final Warp Remains Pending until Its Consumer Runs

**Confirmed producer:** command 74's `csc07_warp` writes event type 1, parameter 1 zero and the bytes
`(73,26,13,RIGHT)`. It returns to the interpreter, loading no Map73 and exiting no handler. Unlike the
ordinary tile-warp producer, it assigns no `WARP_SFX`. The interpreter then clears F640 at command 75
before ending. Script-end cleanup conditionally waits for view scroll if a dialogue window remains,
clears view scrolling speed and restores registers. The handler and shared event cleanup return
through `ProcessPlayerAction` with in-script `CURRENT_MAP=66` and the produced Map73 request.

**Confirmed conditional consumer:** if that request survives to `WaitForEvent`, the nonzero word
bypasses control installation. `ProcessMapEvent` clears the request and its no-scroll warp consumer
consumes/clears existing `WARP_SFX`, idles current entity 0, unwinds exploration and hands the main
loop `d0=73, d1=26, d2=13, d3=Right, d4=0`.

At this static handoff, F74/F640 were explicitly cleared, F507 has no clear in the departure, and the
script's current actor is the ship sprite on Map66. No ordinary Map66 player-control frame between
scene and outward consumption is established or required by this path. Actual pending value,
intervening producers and completed services remain **Unknown**. The
[return/consumer owner](../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md#return-pending-request-and-stopping-boundary)
retains this limit.

This is the stopping boundary. `MAP_OVERWORLD_NEW_GRANSEAL_KINGDOM` names outgoing Map73; destination
setup/init, natural arrival, voyage/settlement and Battle08 are outside this guide. A hypothetical
Map44 reentry could still select F507 while F74 is now clear, unless another owner changes those
flags. There is no completed-departure latch here making repeat interaction coherent. This is a
counterexample to treating setup selection as full scene state, not a naturally reachable return claim.

Continue with [Voyage and landing through damaged-ship control](voyage-through-damaged-ship-control.md)
for the destination's first/repeat selection, voyage, landing and conditional Map14 control/event read.
That separate segment preserves this guide's request-survival and natural-route limits.

## A Separate Zone-table Anomaly Limits Route Claims

**Confirmed static anomaly:** all four Map44 setups share `ms_map44_ZoneEvents`. Its `$FD00` default
and relative expression `byte_54868+4-ms_map44_ZoneEvents` select **`0x5486C`**, not the adjacent
`Map44_DefaultZoneEvent` return at `0x5445C`. The target is an action-script pointer field within a
Map6 fixed-entity record. The zone consumer adds the displacement and calls that address; the
mismatch is established by table/consumer data independently of the source's bug comment.

The existing zone fixture preserves `raw-map44-zone-default-expression-boundary` as an exclusion,
not an admitted zone-program body. **Unknown:** whether a natural zone producer reaches it along this
route and what execution there would do. Do not infer a normal no-op or crash, or reinterpret the
unrelated entity list as a scene. This zone consumer is distinct from the ordinary entity-event
interaction with the departure old man. The
[anomaly owner](../../../../research/chapters/grans-opening/earthquake-town-through-docks-departure.md#selected-zone-table-anomaly)
retains the bounded investigation.

## Evidence Review and Remaining Limits

The pre-synthesis review read accepted #724, its predecessor, the preceding guide, relevant contracts
and selected tracked fixture payloads with their schema/verifier boundaries. Research owns the
pinned-source/dialogue review and retained comparison results. This guide adds no source investigation,
extraction, fixture implementation, H2/H3 rerun or runtime observation. Commands below identify the
existing reproduction owners, not checks claimed as newly executed.

| Relation | Reviewed executable owner and reproduction route | Review disposition |
| --- | --- | --- |
| Requests, substitution and encounter admission | [map-content fixture](../../../../../tests/fixtures/h2/map-content-static-v1.json), `sf2-map-content-static-v1`; [common-maps](../../../../../tests/fixtures/h2/common-maps-static-v1.json), `sf2-common-maps-static-v1`; [battle-global](../../../../../tests/fixtures/h2/battle-global-data-static-v1.json), `sf2-battle-global-data-static-v1`. Commands `uv run sf2 h2 map-content`, `common-maps`, `battle-global-data`. | Wildcards, current-map reload and separate map/encounter fields agree with the bounded research review. Selected Map4/Map44 records use that accepted owner; aggregate fixtures do not prove walking or occupancy. |
| Setup, followers and init | [setup fixture](../../../../../tests/fixtures/h2/map-setup-static-v1.json), `sf2-map-setup-static-v1`; [entities](../../../../../tests/fixtures/h2/map-entities-static-v1.json), `sf2-map-entities-static-v1`; [init](../../../../../tests/fixtures/h2/map-init-static-v1.json), `sf2-map-init-static-v1`, primary body 61; [init verifier](../../../../../src/sf2tool/h2/map_init.py) and [schema](../../../../../schemas/h2-map-init-static-fixture.schema.json). Commands `map-setup`, `map-entities`, `map-init` under `uv run sf2 h2`. | Last-set selector and Map44 init invocation/return agree. Selected follower rows and explicit versus same-map consumers use research; generic entity-record facts do not establish natural population or completed fade. |
| Entity 138, behavior bits and choice | [routing](../../../../../tests/fixtures/h2/map-events/routing-setup.json) and [entity programs](../../../../../tests/fixtures/h2/map-events/entity-programs.json), program 529, under `sf2-map-events-static-v1`; [shard/schema owner](../../../../../src/sf2tool/h2/map_events_fixture.py); [menus](../../../../../tests/fixtures/h2/common-menus-static-v1.json), `sf2-common-menus-static-v1`, `yesNoPrompt`; [menu verifier](../../../../../src/sf2tool/h2/menus.py). Commands `uv run sf2 h2 map-events`, `common-menus`. | Table flags are behavior bits; handler alias, branch and shared return agree. Yes=0, No/B=-1; no natural activation, follower readiness or next control read is supplied. |
| Map/actor/flag/presentation ordering | [script fixture](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json), `sf2-map-script-engine-static-v1`; [verifier](../../../../../src/sf2tool/h2/map_script_engine.py), [schema owner](../../../../../src/sf2tool/h2/map_script_schema.py). Command `uv run sf2 h2 map-script-engine`. | Selected `cs_54578` sites retain clearF at 47/75, map load 51, actor load 52, warp 74 and block copies. Placement/action/presentation site keys preserve local command identity; complete phase/inline-action meaning uses accepted research, not a new scene observation. |
| Event clear, poll and dispatch | [request fixture](../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `sf2-map-event-request-consumption-static-v1`; [verifier](../../../../../src/sf2tool/h2/map_event_request_consumption.py), [schema](../../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json). Command `uv run sf2 h2 map-event-request-consumption`. | Entry reset, WaitForEvent reads and ProcessMapEvent clear/dispatch remain separate. Actual consumer entry/read and normal-story reachability are explicitly Unknown; no composed request-survival claim is added. |
| Zone anomaly | [zone programs](../../../../../tests/fixtures/h2/map-events/zone-programs.json), `sf2-map-events-static-v1`, exclusion `raw-map44-zone-default-expression-boundary`; routing shard above. Command `uv run sf2 h2 map-events`. | Target 346220 (`0x5486C`) and base H1 address 346216 are preserved outside admitted programs. Natural reach and runtime result stay Unknown. |

The [map contract](../../../contracts/world/map-exploration.md#load-and-selection-order) owns selection and load
order; the [setup-data contract](../../../contracts/world/map-setup-data.md#contract-boundary) owns static pointer
identity without broader population claims. The [party/script contract](../../../contracts/party/party-roster-state.md#handler-boundary)
keeps actor/follower operations distinct from recruitment, combat death and persistence. These
contracts do not become another departure evidence ledger.

**Unknown, preserved:** natural outcome/caller state; legal town/harbor walking and changed-layout
reentry; occupancy and follower conflicts at real interaction; party/HP/actor state; wait, motion,
audio/fade/flash/quake/render completion and timing; actual pending-request survival/overwrite; and
save/load persistence. A future composed observation must name its entry, caller flags/player type,
selected setup/entity, choice, post-script maps/flags and first consumer. Decline, F507 absent,
conflicting followers and pending overwrite are useful controls, not an automatic runtime queue.
Original evidence, inferred story meaning and remake choices remain separate. This segment completes
neither the campaign nor the design epic.
