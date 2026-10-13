# Earthquake-town exit through docks departure

Evidence date: 2026-10-12. **Confirmed:** Map4's selected east-edge requests enter Map44 at
`(1,25)`, Right. F507 selects its evacuation population and music/fade init. The departure choice
belongs to entity event index 138, `Map44_1FB_EntityEvent7`; acceptance runs `cs_54578`, clears
F74, loads Map66 within the script, then requests Map73 `(26,13)`, Right and clears F640 before
returning. **Inferred:** the dialogue and operations depict evacuation from Granseal and its
destruction as the ship leaves. **Unknown:** natural walking, invocation, service completion,
actor motion and the pending request actually observed at the consumer. Static order is not a
continuous runtime escape or a remake capability.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #724](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/724), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514). Setup,
follower, interaction and request consumers need explicit cross-owner ordering. Sole tracked
ownership is this document; no shared-path writes or stacked dependency. Accepted starting base
is `c2edc03d48f6039e4a36d593b1fec3b07e25da61`, including
[post-tower return through earthquake-town entry](./post-tower-return-through-earthquake-entry.md).
Concurrent design synthesis consumes that accepted predecessor independently.

Start at Map4's three selected outward records. Follow Map44 admission, required same-map
boarding records, actual departure entity mapping, both choice results, the departure script and
its immediate outward-request consumer. Stop before destination setup/init, Battle08 or later
voyage/settlement scenes. Complete traversal/layout proof, optional interiors/NPC inventories,
an exhaustive zone-anomaly investigation, new acquisition/fixtures/parsers/platforms, engine/UI
and Chinese translation are excluded. Acceptance is direct pinned-source/retained-comparison,
prose/reference/link/private/diff checks, a clean committed planner and actual final-head CI.
The clean pushed Draft PR freezes for independent main-gate acceptance and merge.

All original paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only checkout is clean at that pin. Retained `map-init` and `map-script-engine`
payloads bind that commit and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; their existing fixture
comparisons pass. No fresh extraction, ROM verification or runtime observation is claimed.
Addresses are static source/H1 anchors. Script command indices are zero-based retained
`programCorpus.programs` records, not frames. Fnnn denotes flag nnn. Map-local paths abbreviated
as `mapNN/...` below have prefix `data/maps/entries/`.

## Town exit and Map44 admission

**Confirmed:** `map04/6-warp-events.asm` (`0x98A24`) lists `(50,23)`, `(50,24)` and `(50,25)`.
Each has `warpNoScroll`, destination `MAP_GRANSEAL_DOCKS_AND_SHIP` (Map44), `(1,25)`, Right.
The predecessor establishes conditional Map4 entry/init/control, not a legal route across its
changed layout. Its first-victory lineage supplies F507/F74/F640; ordinary before-scene execution
cleared F83. Repeat victory, intro-skipped or externally supplied flags do not acquire that lineage.

The producer is `WarpIfSetAtPoint` in `code/gameflow/exploration/exploration.asm`, reached from
the warp-tile check in `code/common/scripting/entity/entityscriptengine_2.asm:loc_528A`. It matches
coordinates, treating negative trigger bytes as wildcards, writes event type 1 and parameters,
and selects `SFX_WARP`. `WaitForEvent` / `ProcessMapEvent` in `explorationfunctions_2.asm` read
and clear the request. Parameter 1 zero selects `ProcessMapEventType1_Warp`'s no-scroll branch:
consume/clear `WARP_SFX`, idle entity 0, unwind exploration and return d0-d3 with d4 zero.

`code/gameflow/mainloop.asm:MainLoop/@Start` (`0x75C4`) calls `SwitchMap`, then `CheckBattle`,
then exploration if no battle matches. `data/maps/global/flagswitchedmaps.asm` has no Map44
substitution. In `data/battles/global/battlemapcoords.asm`, **no first-column map value is 44**;
44 elsewhere is a dimension, not a Map44 encounter. `CheckBattle` (`0x799C`) therefore selects
no battle for this request. This says nothing about other maps or the natural journey.

`ExplorationLoop` (`0x257C0`) clears `MAP_EVENT_TYPE`, heals living/immortal allies and fades out.
For explicit Map44, it writes `CURRENT_MAP` and the not-in-battle value, loads tilesets, gets the
selected population, calls `InitializeMapEntities`, loads sprites, clears setup temp flags and
sets F80. It then loads the map, installs base VInts and calls `RunMapSetupInitFunction`.
Map44's `3-flag-events.asm` and `4-step-events.asm` contain only terminators; neither table
supplies an authored copy from which to infer a boarding route.

## Setup, followers and conditional control

**Confirmed:** `data/maps/mapsetups.asm` lists Map44 default, then F609, F506, F507.
`code/common/scripting/map/mapsetupsfunctions_1.asm:GetCurrentMapSetup` keeps scanning and
replaces the setup pointer for each set flag. Thus F507 wins even with F609/F506 set; without
F507, F506 wins over F609, otherwise F609 or default applies. Flag descriptions add no predicate.
Each selected service obtains its setup through this consumer.

`map44/mapsetups/pointertable_507.asm` selects `ms_map44_flag507_Entities`, its entity events,
the shared zone/descriptions/item tables and `ms_map44_flag507_InitFunction` (`0x54568`). The
actual init file is **`s6_initfunction_507.asm`**. It calls `cs_54570` (`0x54570`), whose commands
0-2 are `playSound MUSIC_BOSS_ATTACK`, `fadeInB`, end. It falls through to the `rts` also named
`ms_map44_flag609_InitFunction`; default/F609/F506 pointers enter directly at that return.
There is no F507 init branch marking first completion, placing actors or producing a warp.
Reentry with F507 still set repeats this init. `fadeInB` calls `FadeInFromBlack`; exploration
conditionally performs its own map-music/fade path based on palette comparison. Source order
does not prove the final audible or visible result.

`code/common/scripting/map/mapfunctions.asm:InitializeMapEntities` clears current entities,
initializes followers, declares the selected fixed population and finally entity 0 at the supplied
position/facing. `followersfunctions_1.asm:InitializeFollowerEntities` and
`data/scripting/entity/followers.asm` bind F74 to Astral (`FOLLOWER_B`), King Granseal
(`FOLLOWER_A`) and the Minister (`FOLLOWER_D`). Map44 is not one of the two no-follower maps
and is not an overworld selection for the F65 alternate list. These are current entity mappings,
not force membership or new companion-join operations.

F74 absent leaves this particular group undeclared. Conflicting flags can declare additional
actors and overwrite a repeated follower reference with a later mapping: F68/F70 also use B,
F77 uses A, and F83 uses A/D after F74. First-victory F83-clear state matters, but does not prove
every other follower flag clear. The scene never checks or repairs these conflicts. F507 alone
does not establish its required speakers.

After init, `WaitForEvent` (`0x2591C`) installs control on entity 0 only when the pending-event
word reads zero. `SetControlledEntityActScript` in `entityfunctions_2.asm` selects ordinary
movement for `PLAYER_TYPE=0`; arbitrary raft/caravan state remains distinct. A pending event
skips that installation and returns to `ProcessMapEvent`. Entry's earlier clear and the absence
of an explicit init producer do not prove the later runtime read is zero.

## Boarding records and actual entity interaction

**Confirmed:** the selected records in `map44/6-warp-events.asm` (`0xB2412`) are:

| Trigger | Requested destination `(map,x,y,facing)` |
| --- | --- |
| `(12,11)`, `(13,11)`, `(14,11)` | Respectively `(MAP_CURRENT,52,8,UP)`, `(MAP_CURRENT,53,8,UP)`, `(MAP_CURRENT,54,8,UP)` |
| `(52,9)`, `(53,9)`, `(54,9)` | Respectively `(MAP_CURRENT,12,11,DOWN)`, `(MAP_CURRENT,13,11,DOWN)`, `(MAP_CURRENT,14,11,DOWN)` |
| `(0,255)` | `(MAP_GRANSEAL,49,24,LEFT)`; 255 is the wildcard Y |

All seven are no-scroll. `MAP_CURRENT=255` is a sentinel, not an explicit Map44 reload.
`CheckBattle` resolves it to `CURRENT_MAP` for its scan, so Map44 still has no matching encounter.
`ExplorationLoop`'s no-map-index branch updates the main entity's supplied position/facing and
calls `DeclareRaftEntity`; it skips fresh setup-population construction and temp-flag clearing.
That helper can reposition existing followers and conditionally declare a raft; retention does
not mean identical actor coordinates. It still reloads current map properties and runs setup init.
Thus same-map boarding retains the
current actor population and repeats the F507 music/fade init. By contrast, leaving west and
returning on an explicit Map44 request reconstructs that population. The west request becomes
Map4 while F640 is set; with F640 clear it remains Map3. Those return-init details belong to the
predecessor; they are not explored further here.

`map44/mapsetups/s1_entities_507.asm` (`0x543EE`) has 13 fixed NPC records. The records at
`(12,11)`, `(13,11)`, `(14,11)` coincide with boarding triggers, a concrete reason not to infer
free traversal from warp records. The event required here is the **eleventh** fixed record:
old-man sprite at `(10,14)`, Right, `eas_Init`, mapped to event index **138**. The preceding
record is worker 137 at `(20,12)`; the following records are guards 139/140.
`Map44_1FB_EntityEvent7` is not NPC slot 7 or event index 7, nor is it bound to worker 137.

`InitializeMapEntities` appends ordinary NPC mappings beginning at `NEXT_ENTITYDEF` in
`ENTITY_INDEX_LIST`. `GetEntityEventIndex` in
`code/gameflow/battle/battlefunctions/battlefunctions_0.asm` converts that mapping range to
indices starting at `$80`. `s2_entityevents_507.asm` binds 138 to the departure handler.
`RunMapSetupEntityEvent` matches the index, not an approach direction; the table's `UP` byte
is value 1 (`sf2enums.asm`); `sf2mapsetupmacros.asm:msEntityEvent` emits it unchanged. It is
interpreted as behavior bits (turn toward the player before the handler, no bit-1
facing restoration afterward), not a requirement to stand south or face Up.

The ordinary C-button path is `explorationvints.asm:ProcessPlayerAction`: idle/wait for the
main entity and view, use `GetActivatedEntity`, convert the entity to its event index, then
call `j_RunMapSetupEntityEvent`. `GetActivatedEntity` uses facing/position, counter-block and
follower exclusions; the record alone does not prove selection. The setup event consumer
selects portrait/speech, suspends entity-update VInt processing around the handler, then closes
portrait/text windows and reactivates entity updates. Natural invocation and all waits remain
Unknown. Nearby evacuation text establishes damage, urgency and crowding (1001-1007), not a
formal capacity or readiness predicate.

## Departure choice and ordered script

**Confirmed:** `Map44_1FB_EntityEvent7` (`0x54510`), lines 200-223 of
`s2_entityevents_507.asm`, is program 529 in `map-events/entity-programs.json`. It displays 1008,
closes text, copies current speech SFX, resolves `FOLLOWER_B`'s portrait/speech, displays 1009
and calls `j_YesNoPrompt` (effective target `YesNoPrompt`, `0x15284`). The dialogue greets the
royal group and asks Bowie whether they can depart. No F74, F640, inventory or party-readiness
check is added by this handler.

`code/common/menus/yesnoprompt.asm` initializes Yes (0), selects Yes/No with Left/Right,
confirms with A/C and maps B to No (-1). After closing its window, its final sign extension
sets the condition used by the handler's `bne.s byte_5454A`. Nonzero displays 1010 asking for a
prompt return, then returns at `0x5454E`; it does not run the departure script or clear F74/F640.
Normal event cleanup returns through `ProcessPlayerAction`; control resumes only if the next
pending-event read is zero. No and cancellation are indistinguishable at this return.
Zero runs `cs_54578` and branches to the same return.

The following is the ordered static script, not a frame schedule. Rows refer to
`map44/mapsetups/scripts.asm` and retained command indices; dialogue 1013-1020 is summarized
rather than reproduced. Inline action bodies must also be read at the cited source commands.

| Commands | Operations and boundary |
| --- | --- |
| 0-8 | Set text cursor 1013; Astral agrees; nod/face actor 138 Down and give the departure order. `setDest` Bowie to `(11,13)` waits through `csc29_setEntityDest` for this unflagged reference. Start guard 139 and 140 action lists without joins; then run Bowie's action list with Wait. Guard lists move north from their own starting positions; Bowie's inline list is Right 4, Down 1, Right 1, Right 1, Right 2. The intervening label `Map21_DefaultZoneEvent` is embedded in that action data, not a transfer to Map21. |
| 9-30 | Wait 30; face Bowie and B/A/D Down; camera target `(10,11)`. Copy eight `3x1` block strips from x15 to x12, y22 down through y15 in order. Wait 4 between strips and 15 after the last. `csc34_setBlocks` calls `CopyMapBlocks` and marks both view planes for update; these are mutable map blocks, not story flags. |
| 31-46 | Camera `(13,12)`; the Minister asks the destination, Astral proposes the mainland/Parmecia, the King objects because of Elis, and Astral insists on leaving danger. D performs two awaited jumps; B's later Up 1 / Left 2 action list is unjoined. Camera changes to `(11,12)` before the last line. |
| 47-50 | Clear F74, camera `(11,13)`, call `csub_54714` (`0x54714`), then wait 120. The subroutine decrements layer-1 autoscroll X 16 times, then increments layer-2 autoscroll X 24 times, each with `Sleep(4)`; these counts follow DBF counters `$F` and `$17`. They do not establish measured pixel displacement or elapsed presentation. |
| 51-57 | `loadMapFadeIn MAP_OVERWORLD_GRANSEAL_KINGDOM,28,26` (Map66); `loadMapEntities ce_54736`; awaited Bowie `eas_Init`; replace Bowie's current sprite with ship; `fadeInB`; awaited custom action sets speed `(20,20)` then jumps to idle. Start an unjoined ship/Bowie path Right 1, Up 2, Right 1, Up 1, Right 1, Up 2, Right 3. |
| 58-73 | Wait 60; quake 3; wait 60; quake 5; wait 30; lightning SFX; white flash 30; copy `(0,49)` size `10x15` to `(28,23)`; white flash 30; wait 60; quake 3; wait 60; quake 1; wait 90; quake 0; wait 120. No further dialogue or explicit music command occurs in this script. |
| 74-76 | Write `warp MAP_OVERWORLD_NEW_GRANSEAL_KINGDOM,26,13,RIGHT` (Map73), **then clear F640**, then end. There is no branch back to the choice or explicit wait joining the ship path before the final request. |

`clearF` uses `csc10_toggleFlag`'s zero operand to call `ClearFlag`. Clearing F74 changes later
follower declarations; it does not remove the already allocated royal actors at command 47.
The explicit `loadMapEntities` at 52 rebuilds actors through `InitializeMapEntities` using
`ce_54736` (`0x54736`): main entity `(34,33)`, Right, with an empty fixed-NPC list. F74 is now
clear when the follower table is processed. Other still-set follower flags can still declare
actors; an empty fixed list is not proof that Bowie is the only live entity. Changing Bowie's
sprite to ship does not itself change force membership or set `PLAYER_TYPE` to a vehicle mode.

`csc37_loadMapAndFadeIn` falls through `csc48_loadMap` in `mapscriptengine_1.asm`: initiate
fade-out, load tilesets, wait for fading, load the requested map with its view operands, restore
interrupt/display processing. `code/common/maps/mapload.asm:LoadMap` writes `CURRENT_MAP=66`.
The `(28,26)` operands are not Bowie's placement; `ce_54736` supplies `(34,33)` separately.
This in-script load does **not** go through MainLoop's substitution/encounter check or run Map66
setup init. The later explicit `fadeInB` calls the fade-in service. F640 remains set during this
Map66 sequence until command 75. Concurrent motion, wait termination, final ship position,
screen shake, copied-block appearance and audio completion remain runtime Unknowns.

## Return, pending request and stopping boundary

**Confirmed:** `mapscriptengine_2.asm:csc07_warp` writes type 1, parameter 1 zero and four
request bytes `(73,26,13,RIGHT)`. It returns to the script interpreter; it neither loads Map73
nor exits the entity handler. Unlike `WarpIfSetAtPoint`, it does not assign `WARP_SFX`.
The interpreter therefore executes clearF640 before `csc_end`. `ExecuteMapScript`'s end tail
conditionally waits for view scroll when a dialogue window remains, clears view scrolling speed
and restores registers. The entity handler returns, its shared cleanup closes windows/reactivates
entity updates, and `ProcessPlayerAction` returns to the existing exploration loop, now with the
in-script `CURRENT_MAP=66` and a produced Map73 request.

If that request survives to `WaitForEvent`, the nonzero word skips control installation;
`ProcessMapEvent` clears it and `ProcessMapEventType1_Warp` consumes the no-scroll tuple. It
consumes/clears the existing `WARP_SFX`, idles current entity 0, removes the exploration return
address and returns d0=73, d1=26, d2=13, d3=Right, d4=0 to `MainLoop/@Start`. At that handoff,
F74/F640 have been explicitly cleared, F507 has no clear in this departure, and the script's
current actor is the ship sprite on Map66. No ordinary player-control frame on Map66 between
script and outward consumption is established or required by this static path. The pending value,
possible intervening producers and actual service completion remain unobserved.

This is the stopping boundary. `MAP_OVERWORLD_NEW_GRANSEAL_KINGDOM` is the outgoing Map73
identity; its setup/init, natural arrival and subsequent voyage/Battle08 are not investigated.
A hypothetical later Map44 reentry still selects F507 unless another owner changes it, but F74
would now be clear: this script has no completed-departure latch that makes repeat interaction
coherent. That is a counterexample to treating a setup flag as a complete scene-state proof,
not a claim that such a return is naturally reachable.

## Selected zone-table anomaly

**Confirmed:** the same `ms_map44_ZoneEvents` pointer is used by all four setups.
`map44/mapsetups/s3_zoneevents.asm` (`0x54458`) contains `$FD00` followed by
`byte_54868+4-ms_map44_ZoneEvents`. `RunMapSetupZoneEvent` recognizes the `$FD` default,
adds the signed relative displacement to the table base and executes `jsr (a0)`. Its selected
target is **`0x5486C`**, not the adjacent `Map44_DefaultZoneEvent` `rts` at `0x5445C`.
`byte_54868` is a fixed-entity record in `map06/mapsetups/s1_entities.asm`; +4 is its action-script
pointer field. A source comment says bug, but the table and consumer establish the mismatch
independently of that comment.

The existing `map-events/zone-programs.json:zoneTargetProgramExclusions` already records
`raw-map44-zone-default-expression-boundary`, target 346220, base H1 address 346216, with one
physical record and four setup/route references. It is deliberately not an admitted zone-program
body. **Unknown:** whether a natural zone producer reaches this table on the selected walking
route and what runtime execution there would do. Do not describe Map44 zone dispatch as a normal
no-op, infer a crash, disassemble the unrelated list as a new scene, or extend this slice into
an exhaustive anomaly fix. The ordinary entity-event departure path is a distinct consumer.

## Coverage, gaps and reproduction

Fixture paths below are under `tests/fixtures/h2/`; commands have prefix `uv run sf2 h2`.
Coverage identifies existing admitted boundaries, not a fresh full-rail execution.

| Claim / owner | Existing coverage and direct selection | Missing relation / disposition |
| --- | --- | --- |
| Town exit, boarding and request consumption | `map-content-static-v1.json`, `common-maps-static-v1.json`, `battle-global-data-static-v1.json`, `map-event-request-consumption-static-v1.json`; commands `map-content`, `common-maps`, `battle-global-data`, `map-event-request-consumption`; [map content](./map-content.md), [request consumer](./map-event-request-consumption.md). | Separate records/consumers do not prove traversal or pending-state survival. Reuse these owners if bounded composition is later required. Controls: F640 clear, explicit map versus MAP_CURRENT, alternate parameter 1. |
| Setup, population, init and followers | `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json`, `map-script-engine-static-v1.json`; commands `map-setup`, `map-entities`, `map-init`, `map-script-engine`; [map inventory](./map-data-inventory.md), [common scripting](./common-scripting.md). Retained init comparison and primary body 61 pass. | F507 absent selects another setup; F74 absent or conflicting F68/F70/F77/F83 changes mappings. Same-map and explicit reentry differ. Natural follower state/control read remains Unknown. |
| Entity 138, prompt and both returns | `map-events/routing-setup.json`, `map-events/entity-programs.json` program 529 and effective YesNoPrompt alias; `common-menus-static-v1.json`; commands `map-events`, `common-menus`; [common menus](./common-menus.md). | Existing event/mapping/menu owners suffice. Decline/B returns without departure; direct invocation with wrong follower state is not ordinary narrative execution. No new choice fixture is needed. |
| Departure commands and handlers | `map-script-engine-static-v1.json`, retained `cs_54570` commands 0-2 and `cs_54578` 0-76; direct source for inline actions, `csub_54714`, `ce_54736`, `LoadMap` and caller cleanup. Retained script comparison passes. | Static command/handler coverage does not join natural completed motion, rendering or audio. No new platform or scene replay is justified. |
| Zone pointer selection | `map-events/routing-setup.json`, `map-events/zone-programs.json` raw-expression exclusion; command `map-events`. | Preserve exact target and unproven reach/runtime result. This is already represented, not a new general bug investigation. |
| F74 clear → Map66 actor rebuild → Map73 request → F640 clear → consumer | Reuse/extend `map-script-engine`, `map-init` and `map-event-request-consumption` only if executable composition becomes necessary. | Concrete missing relation: caller-state/pending-request survival at the first consumer after this handler. Minimal public-safe output would name caller, input flags/player type, selected setup/entity, choice, current/requested maps, post-script flags and consumer tuple. Controls: decline, F507 absent, conflicting followers, pending overwrite. No standalone new H2 command is warranted. |
| Walking, presentation and natural services | **Deferred runtime Unknown**, retaining predecessor movement, pending-event, follower and service limitations. | A future observation needs its own named entry and consumer; these gaps are not an automatic emulator-work queue. |

Bounded source reads and in-memory selections reuse two JSON payloads totaling 18,285,979 bytes
(about 18.3 MB), without rewriting them. Each comparison is linear in its retained input;
the planning budget is one process, under 512 MiB peak memory and one minute per comparison
(memory is an estimate, not a measured peak). New output is this prose and small
local review/planner receipts; no network acquisition, model/API processing batch, layout dump,
transcript export or emulator is involved. Replan if the investigation would leave this departure
event or require new bulk input. Existing evidence and failure records remain intact.

Load `. ./local/private-inputs.ps1` in the owning checkout and use the predecessor's exact
[retained-comparison command](./post-tower-return-through-earthquake-entry.md#reproduction-and-review).
It compares all map-init expected data and invokes the existing map-script-engine fixture verifier;
both pass here. Verify the locally selected read-only upstream with
`git -C $upstream rev-parse HEAD` and `git -C $upstream status --short`. Read the named
paths/labels and selected fixture objects; compare every selected program command's source line,
macro and operands, plus its inline action bodies. Dialogue keys `03F0..03F2` and `03F5..03FC`
in `data/scripting/text/gamescript.txt` bind the summarized choice/departure text. `$upstream`
denotes a local selection, never a published machine path.

Record direct prose/reference/link/private checks, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, actual final-head CI and completed failures
in the Issue/PR handoff. Documentation-only scope selects no local normal/full/engine/H3/H4/Godot
suite. Preserve ignored inputs/results, leave a clean pushed Draft PR, stop the writer and notify
main-gate; no self-merge, cleanup or next-Issue selection is part of this delivery.
