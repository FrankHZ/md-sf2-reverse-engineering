# East Shrine return through Battle11 admission

Evidence date: 2026-10-13. **Confirmed:** the selected source connection is optional
Map62 south exit → Map70 west scroll → Map69 fixed northern warp → Map72 cave warp →
Map48 `(15,26)`. With F411 set, that last request selects Battle11. Its before scene
explicitly loads a population, requests seven dialogue texts and repositions plane B;
its start row is empty. **Inferred:** this is a candidate continuation from the accepted
Battle10/optional shrine frontier toward the cave encounter. **Unknown:** natural
navigation, supplied story/party state, completed services and darkness presentation.
The shrine is not a required visit, and this document stops before `ActivateEnemies`.

## Agreement and provenance

This is the Standard-rigor scope agreement for
[Issue #768](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/768), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Route requests, flag admission, prebattle population and plane services cross existing
owners and need explicit boundaries. The sole tracked owner is this document; no
shared index, registry, schema, fixture, parser, design, engine or UI writes are needed.
Consume accepted [Battle10 admission](./ribble-may-through-battle10-admission.md) and
[Battle10 outcomes / optional East Shrine visit](./battle10-outcomes-through-east-shrine-visit.md).
The starting accepted base is `f03773938fd9d5d35e80f20d0c4ddca68463a35c`; no stacked
dependency. Start at the predecessor's final conditional shrine control or Battle10's
direct Map70 return. Select only the connection above and the new-battle opening through
the start-dispatcher return. Enemy activation, turns, combat/outcomes, cave crossing,
Battle25 content, Polca/Bedoe scenes, alternative route inventories, layout extraction,
new runtime acquisition and comparison infrastructure are excluded.

Source paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM`, pinned `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. The retained map-init/map-script-engine
payloads bind that revision and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Their existing fixture comparisons and the existing cutscene-source comparison pass;
this is not a fresh ROM extraction or runtime observation. Fnnn means flag nnn;
battle indices and warp row indices are zero-based. Script record numbers below are
one-based positions in `programCorpus.programs[].commands`; inline action payloads
and entity-list records are separate. Map-init `sourceOrder` is zero-based.

Bounded source/row/list/handler/fixture review is the acceptance method. Reuse the
18,285,979-byte retained pair, one process with linear selected reads/comparisons,
estimated peak below 512 MiB and comparison time below one minute, plus small prose
and receipts. Actual comparison time was below one second. The before-script file has
52 physical lines and its helper 27, smaller than the planning estimate. No new payload,
parser or scene fixture is justified. Reassess before broadening these boundaries;
Unknowns do not authorize acquisition. Direct document/link/private/diff checks, the
clean committed planner and actual final-head CI complete the docs-only profile.

## Selected return and overworld connection

**Confirmed:** `sf2enums.asm` resolves the route identities to East Shrine 62,
Southeast Parmecia 70, South Parmecia 69, Bedoe Kingdom overworld 72 and Dark Caves 48.
The following rows are in `data/maps/entries/mapNN/6-warp-events.asm`:

| Source and row | Matching point / transition | Destination tuple |
| --- | --- | --- |
| Map62 row 0, optional predecessor | X255 wildcard, Y19; no scroll | Map70 `(43,41)`, Down |
| Map70 row 1 | X0, Y255 wildcard; scroll Left | Map69 `(62,255)`, row facing Left; destination Y is retained |
| Map69 row 4 | `(41,15)`; no scroll | Map72 `(40,61)`, Up |
| Map72 row 3 | `(58,49)`; no scroll | Map48 `(15,26)`, Up |

The ordinary movement producer in `entityscriptengine_2.asm` requires layout attribute
`$1000` before calling `exploration.asm:WarpIfSetAtPoint`. That function scans the
eight-byte rows, uses negative coordinate bytes as wildcards, writes event type 1,
the row's parameter tuple and warp SFX. Coordinates alone do not establish reachable
warp tiles. The [request owner](./map-event-request-consumption.md) retains the mutable
slot, intervening-writer and service-completion limits.

`explorationfunctions_2.asm:ProcessMapEventType1_Warp` takes two distinct branches:

- No scroll: request SFX, idle main, unwind the exploration return frame, sample
  main's position, replace each coordinate with its nonnegative parameter byte and
  return map/X/Y/facing to `MainLoop`. All selected fixed destinations replace both
  coordinates. Main calls `SwitchMap`, then `CheckBattle`, before ordinary exploration.
- Left scroll: idle main, set `CURRENT_MAP=69`, call `ProcessMapTransition`, write
  main X/destination X to tile 62 and retain Y/destination Y because `$FF` is negative.
  Reposition followers/raft through `DeclareRaftEntity`, then return to the same
  exploration loop. It does not invoke ordinary entity initialization, map init,
  `SwitchMap` or `CheckBattle`, or clear temporary flags/heal by ordinary entry.
  Parameter 5 contains Left but this branch does not assign it to main's facing.

None of these rows checks F77, F728/F729, May recruitment, Peter membership, Battle10
completion or hearing the directions dialogue. `flagswitchedmaps.asm` has no replacement
for 48/62/69/70/72. The other northern scroll rows are discriminators only: Map70's
north scroll requests the drought overworld, not Map72; Map69's north scroll requests
Map72 `(retained X,62)` and bypasses the ordinary-entry boundary of the selected fixed
`(41,15)` row. They cannot be substituted while retaining this analysis.

The predecessor's immediate Battle10 victory return uses `alt_MainLoopEntry`, which
switches map but does not check battles again before exploration. It returns Map70
at the first battle-party member's battle-derived position/facing, not shrine exit
coordinates. It therefore supplies a separate starting point when the optional visit
is omitted. By contrast, the shrine exit is a new ordinary request: supplied F410 can
select Battle10, or F414 can select Battle14 if F410 is clear, before Map70 entry.
Completion clears a matched unlock but still selects that battle on the same call.

If admitted to ordinary Map70, shared entry clears pending, adjusts the step counter,
heals living/immortal allies, sets current map/not-in-battle, rebuilds entities, clears
temporary flags 256–383 and sets F80 before map load/init. Its fixed list is empty and
init is `rts` (retained body 77). Map69 has the same empty-list/return-only setup
(body 76), but the selected scroll does not invoke it. Persistent story/battle/party
flags are not cleared by the temporary reset. Restored input requires completed entry
services and pending zero, not merely arrival at a source-code label.

### Interruptions before the cave request

**Confirmed:** the inherited Map70 zones still distinguish specific X16–19 → Battle14
helper from the F510-clear default → F410/partial reload. Specific matches do not fall
through on a declined helper. Map69's X23–25 helper can request Battle17 and its
F509-clear default Battle09. On a later Map69 admission, F409/row 9 precedes F417/row17;
the west scroll itself performs no such admission. Their
[accepted request lineage](./new-granseal-departure-through-battle09.md#map69-zone-partial-reload-and-battle09-selection)
and [Map70 owner](./ribble-may-through-battle10-admission.md#map70-zone-requests-and-ordered-battle-admission)
remain authoritative, including partial `$000100FF` writes and surviving parameters.

The fixed Map69→72 request does run `CheckBattle`. Map72 rows 8,24,26 in
`battlemapcoords.asm` have wildcard triggers and ordered unlocks F408,F424,F426;
the first unlocked row wins, even outside its output area. Completion flags
F508/F524/F526 clear the selected unlock without declining that same call.
Absent a matching unlock, ordinary Map72 entry rebuilds its empty fixed list and
flag followers, clears temporary flags and calls `ms_map72_InitFunction` (`0x4FF2E`,
retained body 78). It returns when F716 is clear or F10 (Luke joined) is set; otherwise
it calls `cs_4FF5A`. That later scene is an explicit interruption, not audited here.
Neither the selected tuple nor Battle10/shrine completion proves those flags.

Map72's `s3_zoneevents.asm` additionally routes X32–34 (wildcard Y) to
`CheckRandomBattle(BATTLE_OUTSIDE_KETTO=26)`. The default calls Battle8
(`BATTLE_NORTH_CLIFF`) with F523 clear, or Battle24 (`BATTLE_TO_NORTH_PARMECIA`) with
F523 set. The existing helper requests immediately if its completion flag is clear;
otherwise it requires zero step counter and a zero range-8 draw, with the declined
draw setting a range-4-plus-2 cooldown. Request, selected battle and uninterrupted
travel are distinct. Specific `(25..30,37)` rows instead select `Map72_ZoneEvent3`;
its later-story scene body is outside this slice. All these ordinary zone producers
require `$1400`; a specific-row return does not fall through to the default.

## Cave admission, F411 and the no-battle alternative

**Confirmed:** the accepted [castle-init owner](./peter-castle-audience-through-town-return.md)
and `data/maps/entries/map07/mapsetups/s6_initfunction.asm:ms_map7_InitFunction`
(`0x55816`, retained body 11) establish the F411 producer. With F707 clear it writes
`EGRESS_MAP=6`, runs `cs_55832`, then sets F707 and F411. With F707 set it returns;
it does not repair a supplied F707-set/F411-clear state. This provenance is independent
of F510, F77, F728/F729, F8/F40 and Peter's F7/F39/F67/AI state. None is an additional
predicate in the selected cave warp or Battle11 admission.

`code/common/maps/getbattle.asm:CheckBattle` scans map/unlock/trigger in table order.
Its comparison uses the low coordinate bytes; `$FF` triggers are wildcard only where
actually declared. The only Map48 rows in `data/battles/global/battlemapcoords.asm` are:

| Row | Area output `(X,Y,width,height)` | Required trigger | Unlock / completion |
| --- | --- | --- | --- |
| 11, `BATTLE_CAVE_OF_DARKNESS` | `(0,0,27,32)` | `(15,26)` | F411 / F511 |
| 25 | `(28,0,32,32)` | `(55,29)` | F425 / F525 |

The rectangle is an output, not a containment predicate. The selected requested point
is `(15,26)`, not the before-scene main position `(15,24)` or view origin `(8,19)`.
For that intact request: F411 set/F511 clear selects 11; both set clear F411 and still
select 11 on this call; F411 clear selects neither row, even if F425 is set. At
`(55,29)`, row11 fails its trigger and row25 can qualify with F425. At the other cave
destinations `(52,12)` and `(3,11)`, neither qualifies; no arbitrary point inside either
area is enough. Row25 content and those routes are excluded. F461 controls the intro,
not admission; F511 completion does not imply F461 in arbitrary supplied states.

Inventory confirms Map48 has no independent `mapsetups` directory and no `msMap 48`
row in `data/maps/mapsetups.asm`. This means no setup, not a conjectured empty init
file. `mapsetupsfunctions_1.asm:GetCurrentMapSetup` returns `ms_Void` (`$FFFF`) when
the map-table sentinel is reached. `GetMapSetupEntityList` returns that sentinel
unchanged; its alias is `j_GetMapSetupEntities`. `InitializeMapEntities` still clears
entities and rebuilds flag followers first, then sees the list's `$FF` and declares
main without fixed rows. `RunMapSetupInitFunction` sees `$FFFF` and skips the call.
The zone dispatcher also skips its handler/close-window branch for this sentinel,
but still waits one VInt and waits for physical entity 0 to stop.

Thus a no-battle supplied Map48 request takes ordinary exploration with the shared
load/heal/temp-reset lifecycle, main and applicable persistent followers, but no
map-setup init or fixed NPC population. The battle-before script is not reached merely
because the cave map loaded. Pending zero after services permits control; collision,
visibility and eventual walking through the cave remain Unknown. No layout is decoded.

## New-battle wrappers and explicit prebattle population

**Confirmed:** `MainLoop` passes selected row11 to `BattleLoop` in
`code/gameflow/battle/battleloop_1.asm`. It clears `PLAYER_TYPE`, sets F399 and checks
F88. For F88 clear it clears seconds, stores current map/battle, sets base VInts and
calls the before wrapper. Exact row11 of `beforebattlecutscenes.asm` is `bbcs_11`;
row11 of `battlestartcutscenes.asm` is `ms_Empty`. The wrappers use base450 + 11 = F461:
before dispatches only when clear without setting it; start sets it before dispatch
when clear. F461 set skips both scripts but not new-battle initialization. F88 set
instead restores saved seconds, clears F88/AI memory and loads the saved battle state,
bypassing both wrappers and new placement; it branches to individual turns and is not
an instance of this slice's start-return endpoint. See [battle cutscenes](./battle-cutscenes.md).

`data/battles/entries/battle11/cs_beforebattle.asm:bbcs_11` (`0x4ABFE`) first requests
Map48 at view origin `(8,19)` through `loadMapFadeIn`. The actual handler
`mapscriptengine_1.asm:csc37_loadMapAndFadeIn` starts fade-out state then falls into
`csc48_loadMap`: view target -1, tileset load, fading wait, packed origin and map load,
display/interrupt and VInt services. The macro name alone is not proof of completed
fade-in. Record4 explicitly calls `loadMapEntities ce_4ACA0`; it does not use Map48's
missing normal setup. `csc42_loadMapEntities` reads the main triple and rebuilds
entities/mapsprites under display/VInt control, without map init or temporary reset.

`ce_4ACA0` (source lines47–52) has one `mainEntity` triple, four entity rows and the
entity-list terminator. This is not a battle roster or six map-script commands:

| Authored entry | Lookup/population consequence |
| --- | --- |
| Main `(15,24)`, Up | Physical main slot0; subsequent record5 installs `eas_Init` and waits for idle. |
| Peter `(15,25)`, Up, `eas_Init` | Ally identity7. With no earlier mapping, declare from this row even if unjoined/inactive/dead. If the follower pass already mapped Peter (F67), skip this row including its position/action pointer: the rebuilt follower is retained instead. |
| Hobgoblin `(15,3)`, Down | First regular NPC, character/event128. |
| Vampire bat `(16,4)`, Down | Second regular NPC, 129. |
| Witch `(14,4)`, Down | Third regular NPC, 130. |

`mapfunctions.asm:InitializeMapEntities` calls `ClearEntities` and
`InitializeFollowerEntities` before the explicit rows. Map48 is neither an excluded
follower map nor overworld. F65 therefore does not add the overworld caravan prefix;
enabled `data/scripting/entity/followers.asm` rows are declared in source order.
F67 maps Peter independently of joined/active/HP status; F77 maps the old man as
`FOLLOWER_A=30`. Other supplied follower flags can add actors or overwrite aliases.
These are lookup identities, not fixed physical slots. The earlier shrine command's
transient `FOLLOWER_B=31` relation is not carried through the rebuild. May has no
persistent follower row; F8/F40 alone cannot place her in this prebattle list.

`GetAllyMapsprite` tests word HP for the out-of-battle blue-flame alternative only when
`CURRENT_BATTLE=NOT_CURRENTLY_IN_BATTLE`. On this new-battle path current battle is11
before the scene, so that alternative is bypassed even for zero HP. The explicit Peter
row/follower and non-blue-flame sprite selection do not establish living or joined Peter. Healing
and actual battle placement occur after this scene. The inherited
[word-operand/high-byte follower probe correction](./battle10-outcomes-through-east-shrine-visit.md#current-following-versus-the-f77-rebuild)
still applies to the predecessor; there is no `followEntity`, join, active-party or
force-AI command in `bbcs_11`.

## Opening records, guards and services

**Confirmed:** retained `bbcs_11` and source lines4–46 agree on 34 records: 29 opcode
commands, four `csWait` records and one terminator. Seven `nextSingleText` calls use
2483–2489 (`09B3..09B9`) with ordinary cursor behavior; four are speaker128 and three
Peter. `data/scripting/text/gamescript.txt` supplies the authored text, not just comments.

| Records | Ordered requests |
| --- | --- |
| 1–9 | Cursor2483; camera Bowie; Map48/view `(8,19)` load; explicit population; Bowie init wait; camera Bowie again; `csub_458E`; `fadeInB`; sound fade-out command. |
| 10–17 | Wait for Peter Left1 then Up1; Peter describes the route through the cave. Face Peter Right and Bowie Left; Peter mentions a hobgoblin. Sleep20; speaker128 responds; request `MUSIC_ENEMY_ATTACK`. |
| 18–27 | Wait for Peter Up1; sleep5; start Bowie's `eas_2xRightLeft` without wait; sleep5; start Peter's `eas_2xUpDown` without wait; sleep120; face Bowie Up; wait for Peter Up1; Peter asks who spoke; 128 identifies himself. |
| 28–34 | Camera speed `$20`; camera entity128; host's welcome text; wait for128 Down1; threat text; sound fade-out command; terminate. |

Four waits encode 20,5,5,120 frames (150 total requested by explicit sleeps), not a
scene-duration measurement. Text services add their own scrolling/window waits and
ten-frame sleeps. The debug skip path can omit text/cursor advance and explicit
sleeps, so text IDs/counts describe authored requests with ordinary behavior.
There is no scene choice, conditional jump, story write, warp or combat decision.

The actual handlers prevent a blanket "dead actors are skipped" statement:

- `csc23_setEntityFacing` probes the first byte of its byte-pair operands: Peter7 or
  Bowie0 here. `AdjustScriptPointerByCharacterAliveStatus` skips the two operands only
  for an ally below30 with **word** HP zero (`tst.w d1`). Enemy IDs/non-allies bypass
  this check. HP256 is nonzero; this is not a low-byte HP test or the predecessor's
  word-operand/high-byte character-ID pitfall.
- `csc15_setEntityActscript` and `csc2D_entityActionSequence` have no such HP guard.
  They install the action through the mapping; wait variants poll for `eas_Idle`.
  The latter builds the inline actions in dynamic storage. The two oscillation
  commands themselves do not wait for completion.
- `csc24_setCameraTargetEntity` maps the word character operand and writes the view
  target byte; it does not wait for scrolling or check HP. `cameraSpeed` writes the
  scrolling-speed word. Text waits for scrolling and invokes portrait/speech/text
  services; it does not establish the speaking actor's membership or position.

`GetEntityAddressFromCharacter` has no missing-entity guard: a cleared mapping resolves
slot0 rather than a safe skip. Explicit loading supplies the stated mappings under its
consumer conditions, but a name in a later request is not independent proof of presence.
Relative movement requests do not imply absolute final positions, especially when the
Peter row was skipped in favor of a follower. The interpreter's terminator conditionally
waits for view scrolling based on the dialogue-window index, then clears scrolling speed.
See the [existing script/service owner](./common-scripting.md); no composed observation
of these waits or this story scene is claimed.

## Plane-B helper and darkness evidence boundary

**Confirmed:** record7 calls `code/common/scripting/map/bbcs_11_function.asm:csub_458E`
(`0x458E..0x45C2`) through `csc0A_executeSubroutine`, after the second camera-Bowie
request. It calls `exploration.asm:sub_38C0`, then performs these word operations:

1. Write returned D0 to `VIEW_PLANE_B_PIXEL_X` and its destination; set X counter to
   `(-(D0 logical-right-shift 4)) & 255`.
2. Write returned D1 to `VIEW_PLANE_B_PIXEL_Y` and its destination; set Y counter to
   `((D1 logical-right-shift 4) + 8) & 255`.
3. Set bit1 of `VIEW_PLANE_UPDATE_TOGGLE_BITFIELD`, then return.

The shifts use `NIBBLE_SHIFT_COUNT=4`, masks `BYTE_MASK=255`. These are source-named
coordinate words and ring counters, not measured on-screen pixel locations. The
helper does not load entities, change party/story flags, set a visibility radius or
wait for hardware rendering.

`sub_38C0` preserves D2/D3/A0. With a nonnegative view target it reads that physical
entity's X/Y words. For layer type0 it subtracts plane-B position from the entity and
uses layer2-start words; for nonzero layer type it subtracts plane-A position and uses
background-start words. Per axis, its result is `(start << 7) + 3456 - relativeEntity`,
with word arithmetic. With a negative target it instead returns plane-A coordinates
for type0 or plane-B coordinates for nonzero type. Record6 has just remapped Bowie;
neither a fixed camera origin nor a fixed helper output may replace these inputs.

The selected Map48 area descriptor in `2-areas.asm` has main bounds `(0,0)..(26,31)`,
layer type255, background start `(28,33)`, parallax256 on both axes/layers and
second-layer autoscroll bytes128/128. These are small descriptor facts, not an extracted
layout. The existing `LoadMap` plane initialization also uses `sub_38C0` and equivalent
counter construction; this helper refreshes plane B after the explicit population and
camera selection. `animations.asm:VInt_UpdateMapPlanes` consumes/clears bit1 and calls
`exploration.asm:UpdateVdpPlaneB`, which reads these positions/counters, rebuilds its
plane layout and calls `ApplyVIntVramDma` toward VDP plane B at `$E000`.

**Inferred:** the helper aligns the cave's secondary plane relative to the chosen
actor for the darkness-related presentation. **Unknown:** the resulting visible
mask, lit region, occlusion, palette appearance and frame/scanline timing on natural
entry. The later `fadeInB`, movement/camera changes and `LoadBattle` add services and
reload state; an isolated helper write cannot prove their composed presentation or
persistence. [Common maps](./common-maps.md) and [gameflow](./gameflow-core.md) already
own these consumers and retain their presentation limits. No renderer or comparator
is introduced to promote this inference.

## After before-return: actual battle placement and stop

**Confirmed:** after the before wrapper returns, `BattleLoop` restores map/battle,
clears region flags90–105, heals living/immortal allies, initializes ally then enemy
positions, clears AI memory, loads battle for combatant0 and calls the start wrapper.
This order matters: prebattle scripted actors are not the final combatant population.

`battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions` first resets
ally X, runs `UpdateForce` and traverses the actual active list, testing word HP and
skipping dead members. List and placement-pointer progress differ; the remaining-tail
branch can call `LeaveBattleParty`. `spriteset11.asm` (`0x1B3BF2`) has twelve authored
ally placement rows and ten enemy rows, not twelve guaranteed deployed allies or the
three-enemy scene roster. May's F8, F40, HP and AI state remain separate; Peter's
prebattle entity similarly proves neither F7/F39 nor player control. Old-man following
does not create a combatant. Enemy initialization consumes the battle spriteset, not
`ce_4ACA0`; activation and AI/turn behavior are outside the endpoint.

`battlefunctions/loadBattle.asm:LoadBattle` fades out, loads tilesets and waits,
clears VInts, calls `PositionBattleEntities` (which clears the old entities), selects
the requested combatant's mapped view target, initializes sprites, loads map and
mapsprites, restores base VInts, loads terrain, plays map music and requests fade-in.
The Battle44 timer branch does not apply. It does not preserve the prebattle population
or establish that the helper's exact plane words survive. The start wrapper then sets
F461 if clear and dispatches `ms_Empty`, including normal script termination services.
**Stop at its return immediately before `BattleLoop:@Start` calls `ActivateEnemies`.**
This is a source control-flow boundary, not an observed playable turn or an accepted
combat/outcome contract. [Battle loop](./battle-loop.md) and
[spriteset data](./battle-spriteset-data.md) retain the shared consumers.

## Existing evidence, reproduction and Unknowns

Fixture paths below are under `tests/fixtures/h2/`. Naming an existing static command
identifies its owner, not a fresh extraction; command prefix is `uv run sf2 h2`.

| Relation | Reused/direct evidence | Discriminating limits |
| --- | --- | --- |
| Selected warp/scroll/request | `map-data-static-v1.json`, `map-event-request-consumption-static-v1.json`; selected rows and movement/type1/main handlers; `map-data`, `map-event-request-consumption`. | `$1000`/path, request overwrites, retained Y/facing; shrine optional; scroll versus fixed northern warp. |
| Setup/init/interruption | `map-setup-static-v1.json`, `map-init-static-v1.json` bodies11/76/77/78, `map-events/zone-programs.json` named Map70/69/72 handlers, `map-event-random-battle-state-static-v1.json`; sentinel consumers. | Carried battle flags, Map72 F716/F10 selector, zone-specific versus default, Map48 no setup rather than invented init. |
| Admission/wrappers | `common-maps-static-v1.json`, `battle-global-data-static-v1.json`, `battle-cutscenes-static-v1.json`, `battle-routing-data-static-v1.json`; exact Map48 rows and row11 targets. | F411 absent; F411/F511 both set; `(15,26)` versus `(55,29)`/other points; F461 skip and F88 resume. Routing summary alone does not resolve targets. |
| `bbcs_11`/population/guards | `map-script-engine-static-v1.json` program `bbcs_11`, source `ce_4ACA0`, macros, loading/mapping/HP handlers and follower table; `map-script-engine`. | F67-clear explicit Peter versus F67-set row skip; F77 aliases/other supplied followers; unjoined/inactive/dead actors; zero mapping; word HP0 versus256; debug skips. |
| Plane helper/load/start | Direct `csub_458E`, `sub_38C0`, selected area descriptor, map VInt/plane consumers and `LoadBattle`; existing `common-maps`, `battle-cutscenes`, `battle-spriteset-data` owners/fixtures. | Negative/nonnegative camera and layer-type alternatives; no composed darkness or hardware timing observation; later battle population replaces the scene. |

From the owning checkout, load `. ./local/private-inputs.ps1` in the same command
process and select the registered read-only `$upstream`. Verify its Git HEAD and clean
status against the pin, inventory Map48's files, then run the existing
[retained payload/cutscene comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Select `bbcs_11` from `programCorpus.programs` and compare all 34 records' source lines,
macro names and operands; separately read the inline movements and six-line population.
Select the named map-init bodies/zone labels and the ordered battle/warp rows, then
read their actual consumers above. Resolve text by four-digit keys `09B3..09B9` in
`data/scripting/text/gamescript.txt`. No transcript, private absolute path or extracted
layout needs publication.

**Unknown:** natural collision-free route and exact actor/flag/HP/party/AI state;
interruption-free traversal; waits, request survival, speech/input/camera/fade completion;
composed darkness and hardware timing; and continuous playable Battle11 entry.
Existing independent handler observations do not close these story relations. They
remain explicit limits, not prerequisites that silently launch a runtime queue.

Record direct checks, completed failures, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI in the
Issue/PR handoff. No normal/full/engine/H3/H4/Godot suite is selected solely for this
prose change. Leave the sole-path clean pushed Draft PR in Project Review / Verify,
freeze the writer and deliver the authorized completion callback for independent
review; no self-merge, cleanup or next-slice selection.
