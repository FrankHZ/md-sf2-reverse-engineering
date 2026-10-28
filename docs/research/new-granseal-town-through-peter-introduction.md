# New Granseal town through Peter's introduction

Evidence date: 2026-10-13. **Confirmed:** the castle's conditional south-exit row
requests town Map6 `(11,1)`, Down. Town zones `(10,9)` / `(11,9)` run `cs_54E86`
when F706 is clear; `(19,23)` independently runs `cs_54F26` when F701 is clear.
Each caller sets its own flag only after script return. Neither event reloads the
population, recruits Peter, nor requests a map change. **Inferred:** these are the
sailor/guard introduction and Peter being harassed by children before the minister
invites him to the castle. **Unknown:** natural walking between these boundaries,
completed actor/service work and actual usable control. This is conditional static
research, not a continuous playthrough or an implemented remake capability.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #744](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/744), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Independent triggers, population selection and pending/control ordering need explicit
state boundaries. Sole tracked owner: this document. Accepted base:
`f9bc2159239864f0df8dcaae340e56e53e2893c9`, including
[the castle year-later control boundary](./new-granseal-castle-year-later-control.md)
from [PR #741](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/741).
No stacked dependency or shared-path write; concurrent synthesis consumes accepted evidence.

Start at that castle control boundary, resolve the conditional south exit, town setup/init
and the two early zone programs, and stop at the first pending/control read after
`cs_54F26` and caller F701. The later castle audience, town services/entity conversations,
Creed/Janet bodies, overworld and Battle09/Battle11 are excluded. No new parser, fixture,
platform, acquisition, engine/UI capability or Chinese translation. Acceptance is direct
pinned source/text/list/fixture review, retained comparisons, complete document/link/private/
diff checks, a clean committed planner and actual final-head CI. Independent main-gate
acceptance and merge follow the clean pushed Draft PR; the writer freezes there.

Original paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM`, pinned `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only checkout is clean at that pin. Retained `map-init` and
`map-script-engine` payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Their fixture comparisons pass; no new extraction or runtime observation is claimed.
Fnnn means flag nnn. Command indices are zero-based retained `programCorpus.programs`
records, not frames or inline action indices. Map6 setup files are under
`data/maps/entries/map06/mapsetups/`; bare map-handler filenames are under
`code/common/scripting/map/`. Addresses identify static source anchors.

## Conditional castle exit and town admission

**Confirmed:** the predecessor's normally completing F707-clear scene leaves Map7,
source-derived main `(11,7)`, Up, F707/F411 set and `EGRESS_MAP=6`. Its F707-set
alternative preserves supplied egress/F411 and the entry tuple under the stated mapping
assumptions. Carry these distinctions and `PLAYER_TYPE=0`; no new town gate is inferred
from F707, F411 or egress. They are not tested by the exit row or the two early events.

`data/maps/entries/map07/6-warp-events.asm` (`0x9AFF6`) has a headquarters row and
the south row `mWarp 255,23`, `warpNoScroll`, Map6, destination `(11,1)`, Down.
`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint`
compares the candidate tile to the current map's warp rows: the negative X byte is a
wildcard, Y must equal 23. This is not a statement that every X on row 23 is walkable.
The entity movement consumer in `code/common/scripting/entity/entityscriptengine_2.asm`
(`loc_528A`) calls that function only for the selected layout attribute `$1000` after
its earlier movement/collision work. No layout was decoded and no walk from castle
control to that tile was observed.

On a matching row the writer sets word `MAP_EVENT_TYPE=MAP_EVENT_WARP` (1), copies
row type/map/X/Y/facing into the request parameters, and selects `SFX_WARP`.
`code/gameflow/exploration/explorationfunctions_2.asm:WaitForEvent` returns a pending
request to `ProcessMapEvent`, which clears the word before dispatch. The no-scroll
`ProcessMapEventType1_Warp` branch plays/clears the warp SFX, idles main, removes the
exploration return frame and returns Map6 `(11,1)`, Down to `MainLoop`. Its explicit
nonnegative coordinates replace the fallback current position. This requires the request
to survive until consumption; it is a mutable slot, not a queue.

The ordinary `MainLoop` admission runs `SwitchMap` then `CheckBattle`. Map6 has no
row in `data/maps/global/flagswitchedmaps.asm` or `data/battles/global/battlemapcoords.asm`;
thus this tuple stays Map6 and selects exploration, independently of carried F411.
This does not examine any future battle route. `ExplorationLoop` (`0x257C0`) clears
pending, applies the accepted living/immortal heal, sets current map 6 and not-in-battle,
loads tilesets, selects/initializes the entity list and sprites, clears temporary flags
256-383, sets F80, loads the map and runs init before the first control read. This is not
a full-force reset; the predecessor's [HP/status limits](./battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order)
apply. Persistent story flags and force/follower history remain supplied inputs.

## Setup, ordinary population and init

**Confirmed:** `data/maps/mapsetups.asm` lists Map6 default, F701 and F702 in that
order. `mapsetupsfunctions_1.asm:GetCurrentMapSetup` retains the last applicable
pointer. F702 wins over F701. All three `pointertable*.asm` files select the same
`ms_map6_ZoneEvents` and `ms_map6_InitFunction`; their entity lists differ.

| Supplied setup flags | Selected setup / population |
| --- | --- |
| F702 set, any F701 | `ms_map6_flag702` / `ms_map6_flag702_Entities`: 14 NPC declarations, no fixed Peter or minister/escort records. Later-history control only; no later event body is researched. |
| F702 clear, F701 set | `ms_map6_flag701` / `ms_map6_flag701_Entities`: 14 NPC declarations, no fixed Peter or minister/escort records. |
| Both clear | `ms_map6` / `ms_map6_Entities`: Peter plus 19 NPC declarations. This is the intended early-history scene population, not a prerequisite enforced by the zone caller. |

The default `s1_entities.asm` (`0x547F8`) gives the following script references. Positions
are declaration operands, not observed endpoints or physical entity slots:

| Reference | Default declaration: tile, facing, role from sprite |
| --- | --- |
| `ALLY_PETER` (ally ID 7) | `(18,25)`, Up; `eas_RightLeftLoop`. |
| 128, 129 | Merchant `(20,23)`, Down; walking girl `(18,8)`, Down, walking range operand 1. |
| 130, 131, 132 | Woman `(16,23)`, Down; woman `(15,25)`, Right; priest `(15,24)`, Right. |
| 133, 134, 135 | Merchant `(18,23)`, Down; old man `(17,23)`, Down; worker `(14,26)`, Right. |
| 136, 137, 140 | Boys `(16,25)`, Right; `(21,25)`, Left; `(19,28)`, Up. |
| 138, 139 | Worker `(10,12)`, Right; guard `(11,12)`, Left; both `eas_Init`. The caller comment/narrative describes sailor/soldier, not distinct physical slot identities. |
| 141, 142, 143 | Dwarf `(14,27)`, Right; two old men `(63,63)`, Right. |
| 144, 145, 146 | Minister, guard, worker at `(63,63)`, Down, all `eas_Init`. |

The ordinary fixed NPCs other than 138/139 and 142-146 use `eas_InitSlow`.
`mapfunctions.asm:InitializeMapEntities` clears/rebuilds records, initializes flag-selected
followers first, then consumes the list. An already mapped ally skips its fixed declaration;
NPC references map separately from physical allocation. Main uses the admitted tuple and
`eas_Idle`. No force membership follows from Peter having a map entity. A supplied Peter
follower mapping can replace the assumed scene starting position.

`ms_map6_InitFunction` (`0x54E54`), retained `map-init` primary source body 10, has eight
operations and no flag write. F802 takes priority over F706:

| Supplied init flags | Program and exact operations |
| --- | --- |
| F802 set | Run `cs_54E70`: set reference 132 to `(11,18)`, Down; end. Return without testing F706. |
| F802 clear, F706 set | Run `cs_54E78`: set 138 to `(63,63)`, Down, then 139 to `(15,23)`, Down; end. |
| F802/F706 clear | Return without a script. |

These operations use whichever mapping the selected population supplied; init does not
require F701/F702 clear or reset either flag. `setPos (63,63)` is a relocation, distinct
from the `hide` handler's mapping/follower removal. The F706 re-entry guard tuple `(15,23)`, Down
also differs from the first scene's conditional movement result `(14,25)`, Right. No init branch assigns
egress, changes map, recruits, heals beyond exploration entry, or starts either long scene.

## Independent zone dispatch and sailor/guard scene

**Confirmed:** an ordinary movement zone request is not proven by a table row alone.
`entityscriptengine_2.asm:loc_5294` checks layout attribute `$1400`, writes event type 6
and word X/Y parameters derived from destination plus movement displacement divided by
`MAP_TILE_SIZE`. `ProcessMapEvent` clears pending before its type-6 handler, which applies
init actscript to main and passes those coordinates to `RunMapSetupZoneEvent`. That
function resolves the setup anew and compares the zone rows, with wildcard support.
The exact rows in `s3_zoneevents.asm` (`0x54CD0`) are:

| Coordinates / caller | Predicate and bounded result |
| --- | --- |
| `(10,9)` or `(11,9)`, `Map6_ZoneEvent0` | F706 clear: run `cs_54E86`, set F706 after return, branch directly to return. It does not fall through to the later-history test on this invocation. |
| Same rows, F706 set | Skip `cs_54E86`. F802 clear returns; F802 set/F803 clear selects the excluded later greeting; F803 set returns. No later body is consumed here. |
| `(19,23)`, `Map6_ZoneEvent2` | F701 clear: run `cs_54F26`, then set F701. F701 set: return. **No F706, F707, F411, F702, F802, membership or HP predicate.** |
| `(19,29)` / `(20,29)` and default | Separate later Janet/default dispatch, excluded. These rows are not calls within either early scene. |

The common zone pointers mean unusual F702-set/F701-clear history can dispatch the Peter
program with an unsuitable population. F701-set/F706-clear can likewise dispatch the
first scene using the variant population. These are counterexamples to assuming story
order or actor availability, not evidence that such histories occur naturally.

**Confirmed:** `scripts.asm:cs_54E86` (`0x54E86`), lines 4-50, contains 35 commands
(0-34), no script branch, and dialogue IDs 1066-1073 (`042A..0431`):

| Commands | Ordered source phase |
| --- | --- |
| 0-10 | Set text cursor; track 139. Guard/worker exchange about a large bird and crowd, with waits 30/50 and guard facing Down then Left. Guard says he will look and asks the other to tell the king. |
| 11-24 | Await 139 Down 3/Right 4/Down 8; wait 20; await jump; wait 20; face Right; wait 20; await jump; face Down; waits 30 then 5; start unjoined `eas_2xRightLeft`; wait 60; await Left 1/Down 2; face Right. |
| 25-30 | Set Bowie `(10,9)`, Down; track 138; await 138 Up 1; three text commands tell the leader about the animal. |
| 31-34 | Track Bowie; await 138 Right 1/Up 7; hide 138; end. Caller then sets F706. |

With the default mapping, living Bowie and completing services, the main relocation is
`(10,9)`, Down, and guard movement operands from `(11,12)` sum to `(14,25)`, Right. These are
conditional source results, not measured endpoints. The no-wait action at command 21
is not independently joined merely because 60 time units pass. On script return the
zone wrapper closes portrait/text, waits for VInt and main to stop moving, then exploration
returns to its pending/control read. The second zone requires another dispatch; there
is no direct call from this first program/caller to the Peter program.

## Peter, children and minister: ordered program

**Confirmed:** `scripts.asm:cs_54F26` (`0x54F26`), lines 51-279, contains 149 commands
(0-148), no script branch, and dialogue IDs 1074-1098 (`0432..044A`). The caller predicate
and supplied mapping remain essential; sequential script records do not prove actor/service completion.

| Commands | Ordered source phase |
| --- | --- |
| 0-10 | Cursor 1074; awaited inline scripts give Peter and boys 136/137/140 speed operands `(48,48)` then jump to idle. Await Peter's auto-facing disable; track 136, face Left, his first remark, face Right and wait 30. |
| 11-34 | Start 136 Right 1 unjoined, wait 8, await Peter Right 1, wait 30, boy's missed attempt. Track 137 and his reply; await Peter `eas_Init`, then reapply speed `(48,48)`. Start 137 Left 1 unjoined, wait 8, await Peter Down 1; two remarks. Track 140; his turn; start 140 Up 1 unjoined, wait 8, await Peter Left 1/DownLeft 1; face 136 Right and 137 Left; 140 remarks on speed. |
| 35-56 | Track Peter; face Up, wait 30, Right, wait 30; Peter protests. Start 136/137 jumps unjoined, await 140 jump; face boys and wait 30. Three replies establish the bird speaks and objects to treatment. Await 137 Down 1/Left 1; 136 retorts; wait 5; start Peter `eas_2xRightLeft` unjoined; Peter's two replies. |
| 57-72 | Face boys, 137 reply, then Peter Right and boys toward him. Narrator describes impending hitting. Start 136 Down 1 and 137/140 Left 1 unjoined; wait 5; await Peter `eas_Init` then `eas_JumpLeft`; 144 speaks. No damage/HP instruction accompanies the narrative threat. |
| 73-94 | Quake operand 3; wait 40; face Peter, boys, Bowie and crowd 128/130-135/139/141 Up; quake 0. Set 144 `(16,17)`, 145 `(15,16)`, 146 `(16,16)`, Down; track 144; wait 60. |
| 95-108 | Start 144/145 Down 3 unjoined; await 146 Down 3/Right 1 and face Down. Minister rebukes boys. Start 136/137 Up 2/Right 2 unjoined; await 140 same path; face boys Up. Camera destination `(12,21)`; await 144 Down 2, then 130 Down 1/Right 1. |
| 109-129 | Face boys, Bowie and 128 Left; 130-134 Down; 135/139/141 Right. Await 144 Down 4. Seven dialogue commands: apology, welcome/minister introduction, new settlement, Peter's replies and invitation to meet the king. |
| 130-141 | Wait 40; track Peter; start 144 Up 8 unjoined; await Peter Up 8. Face escorts inward; wait 30; neutral camera. Start 144, Peter and 145 Left 1/Up 5 unjoined; await only 146 Left 1/Up 2/Left 1/Up 3. |
| 142-148 | Hide 144, 145, 146, then Peter; await 130 Left 1; face 130 Down; end. Caller then sets F701. |

**Inferred:** the dialogue and movement stage the minister leading Peter toward the castle.
**Unknown:** completed travel to the castle. There is no warp, map load, population reload,
join, force reset or persistent follower-flag write in either early program. Do not turn
an invitation, unjoined movement or hiding into arrival, death or recruitment.

## Handler effects and post-event boundary

**Confirmed:** `sf2cutscenemacros.asm` and `mapscriptengine_1.asm` distinguish these effects:

- `csc14_setEntityActscriptManual`, `csc15_setEntityActscript` and
  `csc2D_entityActionSequence` use the current character-to-entity mapping. A wait joins
  that named entity's `eas_Idle` pointer, not all earlier actors. Inline speed `(48,48)`
  is an action operand, not an observed velocity/timing. `eas_Init` restores speed
  `(32,32)`, auto-facing and collision/motion defaults; the second initialization before
  `eas_JumpLeft` has no subsequent speed-48 restoration. These handlers do not apply an
  ally-HP skip.
- `csc19_setEntityPosAndFacing` sets current and destination X/Y plus facing;
  `csc23_setEntityFacing` sets facing. Both call
  `AdjustScriptPointerByCharacterAliveStatus`: a zero-current-HP ally reference skips
  that command. Thus supplied dead Bowie/Peter is a control case; do not extrapolate
  ordinary actor poses or confuse that guard with recruitment. NPC references bypass it.
- `csc24_setCameraTargetEntity` maps a nonnegative character reference; negative
  `CAMERA_NEUTRAL=$FFFF` writes the neutral target. `csc32_setCameraDestInTiles` makes
  the target neutral, requests the destination and waits for scroll completion. End
  of script has the normal conditional view-scroll wait and clears scroll speed.
- `csc2E_hideEntity` calls `entityfunctions_2.asm:HideEntity`: set X/Y and destinations
  to `$7000`, replace matching `ENTITY_INDEX_LIST` mappings with -1, and remove a
  matching entry from the current `EXPLORATION_ENTITIES` follower list. It does not
  clear force membership, HP or persistent follower-selection flags, nor join the
  hidden entity's action. A supplied Peter follower may therefore be removed from the
  current follow chain without a persistent dismissal. It is distinct from init's
  position `(63,63)` and is not a death operation.

At the second program's normal return, current map is still 6. Bowie has no position
command in this program; his explicit facing sequence is Up then Left, subject to the
HP/mapping guard. The event coordinates `(19,23)` are dispatch parameters, not proof of
his actual tile at the stopping point. No natural leader position is claimed. The source
leaves the current actors/actions plus explicit hiding; **setting F701 does not immediately
replace that population**. A future entry selects the F701 list (unless F702 wins) and
runs init again. Under ordinary early history, the F701 list's boys are `(19,24)` Left,
`(20,24)` Left and `(20,25)` Down, worker 138 `(63,63)` Down and guard 139 `(13,24)` Up;
F706-set init subsequently overrides 138/139, with guard `(15,23)` Down. These are re-entry
operands, not retroactive endpoints of this scene.

After `csc_end`, caller `Map6_ZoneEvent2` sets F701, then returns through
`RunMapSetupZoneEvent`'s portrait close, text clear, VInt wait and main-stop wait. The
exploration type-6 handler returns, then `loc_2586A` calls `SetMoveSfx` and `WaitForEvent`
(`0x2591C`). This first pending read is the stopping boundary:

| Pending/control condition | Immediate source result |
| --- | --- |
| `MAP_EVENT_TYPE=0` | Set view target to physical entity 0 and install its controlled actscript. With carried `PLAYER_TYPE=0`, use `eas_ControlledCharacter`; the following loop reads pending again and tests A/C. |
| Nonzero | Skip installation. If pending remains nonzero at the secondary read, return that event for dispatch. No specific producer or future event is attributed to this scene. |

The pre-dispatch clear occurred before the scene, and neither program nor its caller
produces a pending request. Absent an intervening write, zero selects ordinary control;
this is not a runtime observation. There is no second exploration initialization, heal
or pending clear between caller F701 and that read. Carried F707/F411/egress remain;
F706 changes only if its separate event ran, and F701 changes only after Peter's script
returns. A service stall can leave caller flags unreached; a supplied nonzero player type
selects the shared vehicle-control branch. Actual control and save persistence are Unknown.

## Coverage, counterexamples and reproduction

Fixtures are under `tests/fixtures/h2/`; command names have prefix `uv run sf2 h2`.
These are existing owner boundaries, not claims that complete H2/H3 rails ran here.

| Relation / owner | Existing coverage and missing composition |
| --- | --- |
| Warp/admission, setup and population | `map-data-static-v1.json`, `map-content-static-v1.json`, `gameflow-static-v1.json`, `common-maps-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json`; [map inventory](./map-data-inventory.md). Commands `map-data`, `map-content`, `gameflow`, `common-maps`, `map-setup`, `map-entities`. Direct pinned reads bind the Map7 row, no Map6 substitution/battle row, Map6 setup order, all three pointers/lists. Table membership is not collision/layout or natural-walk evidence. |
| Init and early zone predicates | `map-init-static-v1.json` body 10; `map-events-static-v1.json` and the predicate/flag-route owners. Commands `map-init`, `map-events`, `map-event-predicate-results`, `map-event-flag-route-selection`. Covered shared selectors/dispatch and retained init operations; direct source resolves F706/F701 caller order. Controls: F802 priority, F706/F701 repeat skips, F701 clear without F706, F702 with incomplete early history. |
| Two scripts, inline actions, text and actor handlers | `map-script-engine-static-v1.json`, programs `cs_54E86` / `cs_54F26`; [common scripting](./common-scripting.md). Command `map-script-engine`. Retained comparison plus direct command/text/inline-action review. Controls: supplied follower mapping, dead ally command skip, missing actor in later population, named waits versus unjoined actions, hide versus persistent dismissal. |
| Request return / control | `map-event-request-consumption-static-v1.json`, `common-scripting-static-v1.json`; [request consumer](./map-event-request-consumption.md). Covered shared consumers. Missing executable composition: selected town population and init → separate zone request/predicate → scene return → caller flag → wrapper cleanup → first pending/control read. |

A minimum future composition would bind source/caller IDs, setup flags/list/mappings,
request coordinates, caller writes, current-map/leader state, population changes and
pending/control reads. Counterexamples must include F701-clear/F706-clear independent
second dispatch, F702-selected missing actors, unjoined action still active at hiding,
script not returning, pending overwrite and nonzero player type. Reuse/extend the owning
fixtures if that question becomes necessary; no new comparison platform is justified now.
Natural walking, actor/service completion, text/audio/render timing, actual control and
save persistence remain Unknown, not an automatic emulator-work queue.

The two reused payloads total 18,285,979 bytes (about 18.3 MB). One process, linear selected
reads/comparisons, estimated under 512 MiB and a one-minute comparison budget suffice.
Outputs are this prose and small ignored review/planner receipts. No transcript, layout,
capture, installation or new environment. Replan before leaving these two early events.

Load `. ./local/private-inputs.ps1` in the owning checkout. With the selected read-only
`$upstream`, check `git -C $upstream rev-parse HEAD` and `git -C $upstream status --short`.
Run the accepted [retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init/map-script-engine comparisons and its ancillary cutscene-source comparison pass;
the latter is not independent evidence for these town events. Select the two programs
from retained `programCorpus.programs`; compare all 184 `sourceLine`, `macro`, `arguments`
records against the pinned file, then read the inline actions. Check their text keys against
`data/scripting/text/gamescript.txt`, init body 10's eight operations, all three Map6 setup
pointers/lists, the Map7 warp row, zone callers and named handlers above. No fixture/schema,
parser or evidence counter changes.

Record direct prose/source/reference/link/private checks, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, actual final-head CI and command
failures in the Issue/PR handoff. No normal/full/engine/H3/H4/Godot suite is selected by this
documentation-only agreement. Preserve prior results/inputs, freeze the clean pushed Draft
PR and leave acceptance/merge to main-gate. No cleanup or next-Issue selection is included.
