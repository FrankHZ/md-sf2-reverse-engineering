# New Granseal castle: year-later address through the first control read

Evidence date: 2026-10-13. **Confirmed:** Map7 init with F707 clear sets
`EGRESS_MAP=6`, runs `cs_55832`, then sets F707 and F411 after script return.
The scene loads Map7, stages the address, and explicitly reloads `ms_map7_Entities`
while retaining the main entity's current position/facing. It requests no warp or battle.
With carried player type 0 and no intervening pending-event write, exploration installs
ordinary control on entity 0.
**Inferred:** this presents a year-later assembly and dismissal. **Unknown:** natural
arrival, completed movement/services, rendering and actual usable control. This is a
conditional source chain, not a continuous runtime observation or remake capability.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #740](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/740), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Setup priority, scene population replacement and caller/control ordering require explicit
state boundaries. Sole tracked owner: this document. Accepted starting base:
`cd8adfaa64657a7bf29fab5d405f01ce5ffc16ed`, including
[the Battle08/castle handoff](./battle08-outcomes-through-new-granseal-handoff.md)
from [PR #738](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/738).
No shared writer paths or unmerged dependency; concurrent synthesis uses accepted evidence.

Start before destination `ExplorationLoop` initialization with Map7 `(11,10)`, Up.
Resolve setup/init, F707 branches, the address, population reload and caller writes;
stop at the first post-init pending/control read. Later Peter/kids/castle-audience hooks,
town exits, recruitment and Battle09/Battle11 execution are excluded, as are new fixtures,
parsers, comparison platforms, acquisition, engine/UI changes and Chinese translation.
Acceptance uses direct pinned source/text/list/fixture review, retained comparisons,
complete document/reference/link/private/diff checks, a clean committed planner and actual
final-head CI. The clean pushed Draft PR freezes for independent main-gate acceptance.

Original paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM`, pinned `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only checkout is clean at that pin. Retained `map-init` and
`map-script-engine` payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Their fixture comparisons pass; no fresh ROM extraction or runtime gate is claimed.
Fnnn denotes flag nnn. Command indices are zero-based retained
`programCorpus.programs` records, not frames or inline action indices. Map7 filenames
use `data/maps/entries/map07/mapsetups/`; bare scripting filenames use
`code/common/scripting/map/`. Addresses are static source anchors.

## Entry, setup priority and init selection

**Confirmed:** the predecessor's ordinary exploration consumer hands the tuple to
`MainLoop`; Map7 has neither a flag-substitution entry nor a battle-coordinate row.
That accepted admission has already selected exploration. It does not establish natural
arrival or reset unrelated story/follower flags. On the intended completed lineage F408 is
clear, F508/F700/F705 are set, and `PLAYER_TYPE=0`; F458 is carried from the ordinary
Battle08 start. F707 clear and the early-history setup flags are separate assumptions.

`code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop` (`0x257C0`)
clears `MAP_EVENT_TYPE`, adjusts the step counter, heals living/immortal allies and fades.
For this explicit map index it sets `CURRENT_MAP=7` and `CURRENT_BATTLE` to not-in-battle,
loads tilesets, selects the entity list, initializes entities and loads their sprites.
It clears temporary flags 256-383, sets F80, loads the map and installs base VInts, then
calls `RunMapSetupInitFunction` before installing player control. F411/F707 and the
setup flags below are outside that temporary-clear range. The living/immortal heal has
its [accepted HP/status limits](../grans-opening/battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order);
it is not another unconditional full-force reset.

`data/maps/mapsetups.asm` lists Map7 default, F701, F702 and F805 in that order.
`mapsetupsfunctions_1.asm:GetCurrentMapSetup` replaces its selected pointer on each set
flag, so the last applicable row wins. `GetMapSetupEntityList` selects the population;
`RunMapSetupInitFunction` separately resolves the selected setup's init pointer.

| Supplied flags | Selected setup / initial ordinary population |
| --- | --- |
| F805 set, any F701/F702 | `ms_map7_flag805` / `ms_map7_flag805_Entities` (11 NPC declarations, no Astral declaration). |
| F805 clear, F702 set | `ms_map7` / `ms_map7_Entities` (12 NPC declarations). F702 restores the default even if F701 is set. |
| F805/F702 clear, F701 set | `ms_map7_flag701` / `ms_map7_flag701_Entities` (17 declarations: five allies and 12 NPCs). |
| All three clear | `ms_map7` / `ms_map7_Entities`. This is the intended early-history population, not a condition imposed by init. |

The three `pointertable*.asm` files all point to the **same** `ms_map7_InitFunction`
(`0x55816`) in `s6_initfunction.asm`. Thus later supplied setup history does not by itself
suppress this scene. The shared zone table identifies `(9..13,8)` hooks and a default
handler; these are separate later event entrypoints, not calls made by this init.
Their bodies and subsequent content are outside this investigation.

The retained `map-init` primary source body 11 has these seven ordered operations:
check F707; branch-if-set to return; assign `EGRESS_MAP=MAP_NEW_GRANSEAL` (6);
invoke `cs_55832`; set F707; set F411; return. `sf2enums.asm` distinguishes town Map6
from castle Map7. There is no coordinate, Battle08 flag, HP or other setup-flag test here.

| F707 at init | Source result |
| --- | --- |
| Clear | Assign egress **before** the scene. Only after normal script return, set F707 then F411. Supplied F411 set does not skip the scene. |
| Set | Return immediately: preserve supplied egress and F411, retain the initially selected population; no address, scene relocation or final explicit reload. F707 set/F411 clear does not repair F411. |

The source comment labels F411 as Battle11 unlocked. This flag write neither performs
`CheckBattle` nor produces a battle request. No immediate Battle11 admission follows
from a comment, and no downstream battle route is investigated here.

## Address actors and ordered phases

**Confirmed:** `cs_55832` (`0x55832`), `s6_initfunction.asm` lines 23-137, contains
78 commands (0-77), with no script-level branch. `ce_559AE` begins at line 138:

| Reference | Scene declaration (tile position, facing) |
| --- | --- |
| Main / Bowie | `(11,7)`, Up |
| 128 minister; 129 King Granseal | `(12,5)`, Down; `(11,4)`, Down |
| 130, 131, 132 guards | `(10,7)`, `(10,8)`, `(10,9)`, all Up |
| 133, 134, 135, 136 soldiers | `(10,10)`, `(12,8)`, `(12,9)`, `(12,10)`, all Up |
| 137, 138 maids | `(8,7)`, Right; `(14,7)`, Left |
| 139 Astral | `(10,5)`, Down |
| Jaha, Sarah, Kazin, Chester | `(12,7)`, `(11,8)`, `(11,9)`, `(11,10)`, all Up |

All 16 non-main records use `eas_Init`. These are script references through
`ENTITY_INDEX_LIST`, not guaranteed physical slot numbers or evidence of force membership.
`mapfunctions.asm:InitializeMapEntities` clears/rebuilds entities, declares flag-selected
followers first, then consumes the supplied list. An already mapped ally skips its fixed
record. NPC references are mapped independently of physical allocation order. The main
entity is declared at the supplied main coordinates with `eas_Idle`. Follower history is
preserved as an input; loading the list is not a recruit, heal or persistent follower write.

| Commands | Complete ordered phase |
| --- | --- |
| 0-7 | Cursor 1290; `mapLoad` Map7, camera `(6,2)`; load `ce_559AE`; await Bowie's `eas_Init`; `resetMap`; slow fade-in; `reloadMap (6,2)`; narrator's year-later text. |
| 8-16 | Cursor 1136; stop 129's animation; `loadMapFadeIn` Map7 at `(6,2)`; request castle music; fade-in; wait 40; await minister Left 1; face him Down and nod. |
| 17-27 | Minister's five text commands address rebuilding, avoiding accidents and the soldiers' contribution. Wait 30; face Up; call the king; wait 60; call again; wait 20. |
| 28-39 | Start king animation; minister's two commands ask him to thank the people; king replies that he has nothing to say. Stop king animation; minister's two replies; minister faces Left, Astral Right; invite Astral, nod Astral, then his reply. |
| 40-47 | Wait 20; await minister Right 1; face him Left; wait 25; await Astral Right 1; face Astral and minister Down; wait 40. These are sequential named waits. |
| 48-59 | Astral's first three commands discuss the year, construction and lack of trading; minister nods; Astral proposes alliances. Astral faces Right, minister Left; minister thanks him; wait 20; both face Down; minister dismisses the gathering. |
| 60-66 | Start 130 Down 1/Left 5; 131 Left 5; 132 Down 1/Left 2; 133 Down 4; 134 Right 5; 135 Down 1/Right 2, all **unjoined**. Then start and await 136 Down 4. |
| 67-72 | Start Astral Left 1 unjoined; await Jaha Down 1; start Jaha Right 6, Sarah Right 6, Kazin Up 1/Right 6 unjoined; start and await Chester Up 2/Right 6. |
| 73-77 | Face Astral, 132 and 135 Down; `reloadEntities ms_map7_Entities`; end. There is no intervening global action-completion barrier. |

The 21 dialogue operands match `data/scripting/text/gamescript.txt`: key `050A` (1290),
then `0470..0483` (1136-1155). **Inferred:** this is a year-later assembly, with the king
reluctant to speak, Astral proposing trade/alliances and the minister dismissing everyone.
The narration and speech do not measure elapsed emulator/game-clock time or establish a
new trading mechanic. Animation start/stop writes an animation counter; it does not change
HP, death or force membership. No inventory, alliance, recruit or time-advance command is
present in this program.

### Map loading and action semantics

**Confirmed:** the relevant shared handlers separate presentation from world/control state:

- `mapscriptengine_1.asm:csc48_loadMap` loads tilesets, waits on fading, packs the two
  camera operands and calls `code/common/maps/mapload.asm:LoadMap`. The positive map index
  takes `LoadMap`'s new-map branch even when it equals the current map: it writes Map7,
  reloads palette/blocks/layout, and finishes map display setup. It does not call Map7 init,
  rebuild the population, clear story flags or install control. `(6,2)` is camera input,
  not Bowie's destination.
- `csc36_resetMap` calls `code/gameflow/exploration/exploration.asm:ResetCurrentMap`,
  which clears working layout storage, seeds blank tile data and tail-branches to `LoadMap`
  with d1=-1. `csc46_reloadMap` also supplies d1=-1, plus the camera operands. These reload
  the current map's display/area state rather than restarting exploration or resetting
  persistent progression. `csc37_loadMapAndFadeIn` first seeds fade-out state and falls
  through to `csc48`; command 12's separate fade-in remains meaningful. Visible timing is
  not established by these calls.
- `csc42_loadMapEntities` consumes the scene list's main coordinates, initializes the
  population and calls `LoadEntityMapsprites`. By contrast `csc44_reloadEntities` reads
  the current main entity's X/Y divided by `MAP_TILE_SIZE` and facing, then initializes
  the **explicitly supplied** list. It does not select the current setup again and has no
  direct `LoadEntityMapsprites` call. Neither handler is a force-membership or HP reset.
- `sf2cutscenemacros.asm` encodes `entityActions` with control byte 0 and
  `entityActionsWait` with `$FF`. `csc2D_entityActionSequence` builds a per-entity action
  script ending at `eas_Idle`; a nonzero control byte waits for that named entity's script
  pointer to become idle. Waiting for 136, Jaha or Chester does not explicitly join earlier
  actors. `csc15_setEntityActscript` uses the same named-idle predicate for Bowie's init.
  Dead allies are **not** filtered by these two handlers. Do not generalize the
  `AdjustScriptPointerByCharacterAliveStatus` guard used by position/facing/animation
  commands to this action family; both action handlers resolve the current mapping directly.

Actual completion, obstruction and scheduling remain Unknown. In particular, an existing
ally follower mapping can change which entity and starting position an action uses. A
later population reload replaces the entity/action records rather than proving that all
unjoined dispersal finished. The maids receive no dispersal commands. There is no basis
for publishing final per-actor movement endpoints as observed facts.

## Reload, caller flags and first control read

**Confirmed:** the ordinary `s1_entities.asm:ms_map7_Entities` (`0x553C6`) declares
minister `(12,5)` and king `(11,4)`, both Down; guards `(3,7)`, `(4,14)`, `(8,10)`,
Down; soldiers `(9,20)`, `(19,9)`, `(14,10)`, `(13,20)`, Down; maids `(8,7)`, Right
and `(14,7)`, Left; Astral `(10,5)`, Down. The guards at `(3,7)` / `(4,14)` and soldier
at `(19,9)` have walking declarations with range operands 2/1/1 (`sf2mapsetupmacros.asm`); the soldiers at `(9,20)` and
`(13,20)` use opposite horizontal loops. The other seven records use `eas_InitSlow`.
There are no fixed ally records here, but flag-selected followers are still initialized.

Command 76 explicitly chooses this list even if an unusual supplied F701/F805 history
selected another setup at entry. It does not clear those flags or replace the setup's
entity-event pointers. Thus the reloaded population and later selected event setup must
not be silently assumed to match under arbitrary history; those later interactions are
outside this endpoint. F707-set entry instead retains its selected initial list.

On the intended normally completing F707-clear path, main was declared at `(11,7)`, Up;
its awaited `eas_Init` configures motion/collision/appearance and returns to idle, and the
scene has no later main movement/facing command. `csc44` therefore preserves `(11,7)`, Up
under that mapping/service assumption. This is a source-derived result, not an observed
leader position. F707-set skips that declaration: its initial leader tuple remains
`(11,10)`, Up under the same no-intervening-change assumption.

The end marker returns through `mapscriptengine_2.asm:ExecuteMapScript`'s normal tail
(conditional view-scroll wait, clear scrolling speed, restore registers). **The init
caller then sets F707 followed by F411**, and returns. The script itself writes neither
flag. There is no second exploration initialization, heal, map-admission check, or
pending-word clear at that return. Egress remains town Map6 on the executed branch;
current map remains castle Map7. The predecessor's full-force reset is not repeated here,
and force membership, unrelated persistent flags and supplied player type are carried.

Exploration conditionally calls map music/fade based on the palette comparison, calls
`SetMoveSfx`, then enters `WaitForEvent` (`0x2591C`). Its **first** pending-word read is
the stopping boundary:

| Pending/control condition | Immediate source behavior |
| --- | --- |
| `MAP_EVENT_TYPE=0` | Set view target 0; call `SetControlledEntityActScript(0)`. With carried `PLAYER_TYPE=0`, `code/common/scripting/entity/entityfunctions_2.asm` installs `eas_ControlledCharacter`. The following loop reads pending again and checks A/C input. |
| Nonzero | Skip control installation; if still nonzero at the secondary read, return that event to the exploration dispatcher. This slot is not a queue. No particular event/consumer is attributed to this scene. |

There is no explicit pending producer in `cs_55832` or its init caller. Entry cleared the
old word; absent an intervening write, zero selects the first row. Supplied nonzero
`PLAYER_TYPE` takes the shared vehicle-control branch instead; the scene does not normalize
it. F707/F411 are already set on the executed path before either pending branch. A service
stall before script return would instead leave those caller writes unreached, despite the
earlier egress assignment. Pending survival/overwrite and usable control are unobserved;
no specific produced event requires expanding this slice to an immediate consumer.

## Coverage, gaps and reproduction

Fixture names below are under `tests/fixtures/h2/`; command names have prefix `uv run sf2 h2`.
These are existing coverage boundaries, not claims that whole H2/H3 rails ran for this prose.

| Relation / owner | Covered boundary and remaining composition gap |
| --- | --- |
| Setup order, entity declarations and init | `map-setup-static-v1.json` (`map7-alias-restores-default`, `map7-final-variant-wins`), `map-entities-static-v1.json`, `map-init-static-v1.json` body 11; [map inventory](../../world/map-data-inventory.md). Commands: `map-setup`, `map-entities`, `map-init`. **Covered:** selectors, source/list consumers and init body. Direct reads resolve all three pointer tables and ordinary/scene lists. Controls: F701/F702/F805 priority; F707 set with F411 clear and arbitrary egress. |
| Scene, text, map loads, action bridge and population reload | `map-script-engine-static-v1.json`, program `cs_55832`; [common scripting](../../world/common-scripting.md). Command: `map-script-engine`. **Covered:** ordered program and transition/lifecycle, action-bridge and population handler facts. Retained comparison passes; text and inline actions/lists were checked directly. Controls: explicit default reload despite variant setup, already-mapped or dead allies, waited versus unjoined actions. Historical synthetic observations of these families are not a natural run of this scene and were not replayed. |
| Init return to pending/control branch | `map-event-request-consumption-static-v1.json`, `common-maps-static-v1.json`, `common-scripting-static-v1.json`; [request consumption](../../world/map-event-request-consumption.md). Commands use those basenames. **Covered:** separate shared consumers. Missing executable composition: supplied setup/history → F707 selection → same-map scene/population → caller F707/F411 → first pending read/control installation. **Reuse/extend** existing owners only if that composition is later required; no new fixture/platform is justified here. |
| Minimum possible composition and runtime limits | Public-safe facts would bind source/caller IDs, input flags, initial/final list pointers, leader mapping/tuple, egress/current map, caller writes and pending/control branch. Counterexamples: F707-set skip, F805 with F707 clear, mapped/dead ally, unjoined actor still active, pending overwrite, nonzero player type, or script not returning. **Unknown:** natural arrival, completed actions/services, display/audio/text timing and actual usable control. No automatic acquisition queue. |

Bounded review reuses two payloads totaling 18,285,979 bytes (about 18.3 MB), read-only.
One process and linear comparisons, estimated under 512 MiB peak and a one-minute comparison
budget, suffice. Outputs are prose and small ignored review/planner receipts; no full
transcript, layout, capture, tool installation or new environment. Replan before new
acquisition or research beyond the first post-init control read.

Load `. ./local/private-inputs.ps1` in the owning checkout. With the locally selected
read-only `$upstream`, check `git -C $upstream rev-parse HEAD` and
`git -C $upstream status --short`. Run the accepted
[retained-payload comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary);
its map-init/map-script-engine comparisons and ancillary cutscene-source comparison pass.
The cutscene comparison is not independent evidence for this castle scene. Select
`cs_55832` from retained `programCorpus.programs`: compare all 78 `sourceLine`, `macro`
and `arguments` records to the named pinned file, and read its inline actions/list. Verify
text keys `050A`, `0470..0483`, init body 11's seven operations, Map7 setup row order and
all pointer tables, `ms_map7_Entities`, and the named handler/caller sections above.
No parser/schema/fixture contract or evidence counter changes.

Record direct prose/source/reference/link/private checks, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, actual final-head CI and command
failures in the Issue/PR handoff. Normal/full/engine/H3/H4/Godot suites are excluded by
this documentation-only agreement. Preserve prior results and inputs; freeze the clean
pushed Draft PR. No self-merge, cleanup or next-Issue selection is included.
