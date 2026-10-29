# Battle07 outcomes and the after-scene exploration handoff

Evidence date: 2026-10-12. **Confirmed:** first victory runs `abcs_battle07`, then returns to
Map58 `(13,35)`, Down when F999 is clear. The script requests that return **before** installing
three current-entity follows and setting F74/F640; the caller subsequently clears F407 and sets
F507. Map58 has no setup entry, so the generic exploration initialization reaches player control
without another map-init scene. This is a conditional static handoff, not an observed natural play.
**Inferred:** the scene depicts the failed rescue of Elis, Bowie's acquisition of Galam's jewel,
and an urgent retreat from the tower. **Unknown:** natural combat, completed scene services,
presentation timing and the walk onward. The evacuation and Battle08 are outside this slice.

## Agreement and evidence

This is the Standard-rigor agreement for
[Issue #716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/716), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514). The long scene
crosses battle, script, map and follower consumers; explicit order and conditional state control the
scope. Sole tracked ownership is this document. Accepted starting base is
`f325b10ee5943029c476cd9ca3912dc089f192c0`, including
[the Battle06-to-Battle07 research owner](./battle06-to-battle07-content-route.md) from
[PR #714](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/714). Concurrent design synthesis
has separate ownership and is not a stacked dependency.

Scope is Battle07 defeated/outcome/after/join behavior through the first post-scene exploration
handoff, with the immediate outgoing event identified for the next slice. Exclude the complete
castle/town/docks evacuation, ship departure, next-battle admission, later campaign, map-layout
acquisition, new fixtures/parsers/platforms, engine/UI, emulator work and Chinese translation.
Acceptance is direct source/fixture/verifier and prose/reference/link/private/diff review, clean
committed planner and actual final-head CI under this documentation-only agreement. A clean pushed
Draft PR freezes for independent main-gate acceptance; it does not accept a remake capability.

All original paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only checkout is clean at that pin. Retained `map-script-engine` and `map-init`
payloads bind this source and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; their existing fixture
comparisons pass here. The cutscene source facts were also compared directly to their fixture.
No new ROM verification, extraction or runtime observation is claimed. Addresses name static
source/H1 anchors, not new executed observations.

`Fnnn` denotes flag nnn. `sf2enums.asm` names battle 7 `BATTLE_VERSUS_DARK_SMOKES`, Map58
`MAP_ANCIENT_TOWER_UNDERGROUND_ROOM`, Map59 `MAP_ZEON_BATTLEFIELD` and Map60
`MAP_ANCIENT_TOWER_STAIRWAY`. Battle filenames below live under `code/gameflow/battle/`;
map-script handler filenames live under `code/common/scripting/map/`. Scene command indices are
zero-based retained `programCorpus.programs` indices; source line ranges include embedded action
instructions and must not be mistaken for command counts.

## Outcome branches and caller order

**Confirmed:** `battleloop_1.asm:BattleLoop` (`0x23A84`) clears `PLAYER_TYPE` and sets F399 even
on suspended entry. After an individual turn it runs the defeated wrapper, processes the dead list
and counts combatants; it processes turn-end effects/deaths and counts again.
`battleloop/countremainingcombatants.asm:CountRemainingCombatants` (`0x23C58`) counts nonnegative-X,
nonzero-HP entries, then forces zero allies if Bowie has zero HP. Zero allies selects defeat before
zero enemies selects victory. Battle07 is not a special leader-only win.

| Consumer / selected row | Battle07 consequence |
| --- | --- |
| `cutscenes/battleendcutscenesstart.asm:ExecuteBattleCutscene_Defeated` and `battleendcutscenesend.asm` (`0x47B92..0x47C8E`) | Requires Bowie alive and enemy slot 128 dead. With F507 clear, row 7 of `data/battles/cutscenes/battleendcutscenes.asm` is `ms_Empty`; F507 set skips the script. Row 7 of `data/battles/global/enemyleaderpresence.asm` is zero, so the shared tail does not append all surviving enemies on that death. |
| `cutscenes/afterenemyleaderdeathpositions.asm:ApplyPositionsAfterEnemyLeaderDies` (`0x47D9E`) | `data/battles/global/afterbattlepositions.asm` contains only Battle05. Battle07 reaches the terminator without its position/HP rewrite. Scene actors named Galam and Dark Smokes do not replace these predicates. |
| `cutscenes/afterbattlecutscenesstart.asm:ExecuteAfterBattleCutscene` (`0x47CBC`) | F507 clear selects row 7 at `0x47D02` of `data/battles/cutscenes/afterbattlecutscenes.asm`: `abcs_battle07` (`0x49F7E`). F507 set bypasses the entire scene. |
| `cutscenes/afterbattlecutscenesend.asm:EndAfterBattleCutscene` (`0x47D54`) | Both after-script and skipped paths read row 7 at `0x47D71` of `data/battles/cutscenes/afterbattlejoins.asm` and call `JoinForce(0)` (Bowie). This is the consumed table, not the similarly named legacy global table. No new story recruit is supplied by that zero. |

| Outcome / input | Ordered writes and resources | Return boundary |
| --- | --- | --- |
| Victory, F507 clear | `battleloop_2.asm:BattleLoop_Victory` (`0x23CBA`) heals living/immortal allies; seeds event parameters 2–5 from current map, first active party member's final position plus battle-area origin, and entity facing; zeros parameter 1; executes the complete after script and shared join; clears F407, sets F507; reads parameters 2–5 into d0–d3, returns d4=1. | Scene replaces the seed with Map58 `(13,35)`, Down. F385, F74 and F640 have been set before return. Destination selection and control are detailed below. |
| Victory, F507 already set | Same seed, heal, shared join, F407 clear and F507 set; no after-scene command runs. | Ordinary Battle07 seed is Map58, first active member's final battle coordinates (origin `(0,0)`), and that entity's facing. No fixed `(13,35)` return or fresh F385/F74/F640 writes may be inferred. Existing values remain inputs. |
| Defeat | `BattleLoop_Defeat` calls `UpdateBattleUnlockedFlag`, displays fixed text 363, restores Bowie's maximum HP, halves gold by unsigned shift, calls `GetEgressPositionForBattle`, returns d4=-1. | No after/join call or new F507/F385/F74/F640. F507 clear leaves F407; F507 set clears F407. No Battle04 loss/upgrade exception applies. |
| Egress / Angel Wing | `battlefunctions/battlefunctions_1.asm:ExecuteBattleaction_Egress` charges spell MP and uses fixed text 274; `ExecuteBattleaction_AngelWing` removes the selected item slot and uses fixed text 275. Their shared tail calls the presentation helpers, updates the unlock flag, gets the egress position and returns d4=0. | Same conditional F407 handling and savepoint selection as defeat; no defeat gold halving or after/join call. The charged MP is distinct from the subsequent exploration heal. |

`battleloop/getegresspositionforbattle.asm:GetEgressPositionForBattle` (`0x23E50`) has no Battle07
special case: it reads `EGRESS_MAP`. `code/common/maps/egressinit.asm:GetSavepointForMap` (`0x75EC`)
uses `data/maps/global/savepointmapcoords.asm`. With the predecessor's unmodified `EGRESS_MAP=16`
and F399 set, the result is Map16 `(27,7)`, Up. That is an input-dependent example, not a universal
Battle07 destination: shrine use or another caller selection can change it. An unlisted map gives
`(1,1)`, Up; F399 clear takes the game-start fallback, although BattleLoop sets it here. The helper
also has an F64-gated raft-reset side effect, not a new Battle07 resource grant. All returned maps
pass through `SwitchMap`; no natural defeat/escape reentry path is established.

`battleloop/heallivingandimmortalallies.asm` (`0x23BFC`) restores HP/MP for living allies and Peter/
Lemon, skips other dead allies, retains only already-present stun/poison/curse bits, and rebuilds
stats. Victory calls it before the scene; `ExplorationLoop` calls it again on all outcome returns.
Actor falls, disappearance effects and dialogue do not add a combat HP/death transaction.

**Confirmed:** F457 gates the before/start programs, independently of F507. The predecessor owns
`bbcs_07`, whose command 1 clears F83 before staging the scene; start row 7 is empty and the wrapper
sets F457. F457 already set skips that F83 clear; F88 suspended entry skips both intro programs.
Neither skip suppresses the after scene when F507 is clear. The after script does not clear F83,
so arbitrary intro-skipped caller state must not be normalized to the ordinary follower state.

## Long scene: actors, phases and text

**Confirmed:** `data/battles/entries/battle07/cs_afterbattle.asm:abcs_battle07` occupies source
lines 4–644, commands 0–396. It has one linear script through `csc_end`, no explicit flag branch,
subroutine call or intermediate field-control return. Individual command handlers still have their
own entity/liveness and service behavior. `loadMapFadeIn` command 1 supplies Map58 and camera
`(8,16)`; these camera coordinates are not a player destination. Command 2 loads `ce_4A912`
(`0x4A912`), rebuilding the scene's entity population. IDs are local to this population:

| Scene reference | Declared role and relevant mutation |
| --- | --- |
| Bowie / entity 0 | Starts `(13,23)`, Up; is stopped, oriented, moved, given pose sprites, then restored to the Bowie sprite and initialized/restarted. Scripted displacement is not a legal battle movement or HP rule. |
| 128 / 129 | Initially off-map effect sprites. They are placed together at `(13,16)`, moved apart/down, then parked `(63,63)`. Later they become `MAPSPRITE_OBJECT3` at `(12,5)` / `(13,5)` with mosaic effects and scripted upward/downward motion. These are reused effect actors, not proof that combat enemies survived. |
| 130 / 131 / 132 | Evil Galam, Astral and Elis. Galam/Elis rise with the effects and later receive `eas_TwirlShrinkDisappear`; Astral is later reinitialized and participates in carrying the king away. No `JoinForce`, combatant HP or item-slot write is made by those actor operations. |
| 133 / 134 | King Granseal and minister. They move into the confrontation, recoil, assist the throw and leave in the retreat. Their scene follows are distinct from the flag-selected followers rebuilt on return. |

The single `textCursor 2385` feeds 70 `nextSingleText` and 6 `nextText` commands: ordinary unskipped
IDs 2385–2460 (`0x951..0x99C`). Every selected source comment was compared with the corresponding
hexadecimal key in `data/scripting/text/gamescript.txt`. No fixed-ID `txt` command occurs inside
this after program. `mapscriptengine_2.asm:csc04_setTextIndex` and the display handlers own cursor
writes/increments and debug skipping. The table summarizes content rather than publishing a
transcript; actor operands identify the script speaker, not independent truth of the statement.

| Phase / source lines / command boundary | Text IDs | Authored sequence and material boundary |
| --- | --- | --- |
| Staging and hostage confrontation, 4–168; commands 0–89 | 2385–2401 | Load the room/cast; copy `(7,23)` size `1x2` to `(13,22)`, `(13,24)`, `(13,29)`; stop/orient Galam and Astral, then release Galam. King/minister advance, Elis is blocked, and effect actors enact the warning attack. King/minister recoil and restart. Galam's threat and the king's reassurance do not establish damage or survival outside these actor calls. |
| Jewel/door demonstration, 169–241; commands 90–147 | 2402–2414 | Astral is released to ask about the tower. Galam displays the jewel in narration; white flashes, quake commands, camera `(8,3)` and three `3x3` copies from `(0,39)`, `(0,43)`, `(0,47)` to `(12,4)` animate the doorway. Effect actors change sprite and enter. These are working-map copies and presentation calls, not an inventory transfer or persistent collision proof. |
| Rising light and rescue attempt, 242–407; commands 148–223 | 2415–2425 | Effects, Galam and Elis rise. Astral asks for Bowie's acrobatics; Astral/minister and Bowie move back and forward, and Bowie is thrown upward. Camera targeting shifts to Bowie; source action waits and non-waiting actions remain distinct. |
| Grasp, flare and fall, 408–517; commands 224–305 | 2426–2439 | Narration says Bowie grasps the jewel, it fuses to his neck and the two jewels merge. Camera switches, shivers, flashes, sprite changes and falling motion surround those lines. Only after the final merge narration does command 305 set F385. There is no ordinary inventory-slot add/remove or new ability/stat write here. |
| Disappearance and aftermath, 518–558; commands 306–337 | 2440–2447 | The effect actors mosaic out; Galam and Elis receive disappearance scripts. Astral/minister recover ordinary action scripts, sad music is requested, Bowie is restored/restarted and the group discusses the jewels. Their disappearance and the rescue failure remain scene content, not confirmed death or a new force membership. |
| Renewed quake, retreat and final request, 559–644; commands 338–396 | 2448–2460 | Quake and boss music calls precede the decision to leave. Astral/minister stage carrying the king south; the camera follows the king. Astral's final warning precedes the warp request, three follows, F74, F640 and end. There is no exploration interval during the carried retreat. |

The command ranges above name macro records, not elapsed frames. The last `setQuake` operand is 1;
that alone does not prove the visible quake after map loading. Wait completion, scroll/fade/audio
completion, simultaneous actor motion and total duration remain **Unknown** at natural runtime.
The new-map branch of `code/common/maps/mapload.asm:LoadMap` calls `LoadMapBlocksAndLayout` again
on return; the scene's six copies must not be treated as a saved layout merely because they ran.

## Persistent effects and actual return consumer

**Confirmed:** the following writer/consumer relations distinguish narration, scene entities and
persistent state. There is no new story recruit, gold transaction, ordinary item-slot transaction,
`setStoryFlag`, or Battle08 unlock in `abcs_battle07`.

| Producer | Material consumer and limit |
| --- | --- |
| F385, command 305 / line 517 | `code/common/menus/memberscreen.asm:BuildMemberStatusWindow`, local `@WriteJewelString`: for Bowie, F384 first gates the jewel display; F385 then adds the second jewel icon and plural suffix. F385 alone with F384 clear does not display either jewel through this branch. The scene does not set F384 or add an inventory item. |
| Commands 391–393 / lines 639–641: `followEntity 131,ALLY_BOWIE,2`, `133,131,2`, `134,133,2` | `mapscriptengine_1.asm:csc2C_followEntity` (`0x46E58`) resolves entities and calls `AddFollower` using relationship selector 2. These update the current scene's Astral → Bowie, king → Astral, minister → king relations. They do not by themselves define the next map's entity list or a force join. |
| F74, command 394 / line 642 | `followersfunctions_1.asm:InitializeFollowerEntities` (`0x44298`) consumes `data/scripting/entity/followers.asm:table_Followers` when rebuilding Map58. Three F74 rows declare Astral (`FOLLOWER_B`), king (`FOLLOWER_A`) and minister (`FOLLOWER_D`), in table order among any other enabled rows. The comment mentioning only Astral + King is incomplete. |
| F640, command 395 / line 643 | `code/common/maps/mapinit_0.asm:SwitchMap` (`0x7956`) consumes `data/maps/global/flagswitchedmaps.asm`: a request for Map3 becomes Map4, the Granseal earthquake map. It does not replace Map58. Clearing this flag at the docks is outside this slice; the source's question-mark comment is not accepted lifecycle evidence. |
| F407 clear / F507 set after the wrapper | Battle completion and encounter admission remain separate. F507 skips future after-program dispatch; F407 is the Battle07 admission flag. Neither is used to choose a Map58 setup because that map has no setup row. |

For the completed first-victory script, the return sequence is:

1. Command 390 / line 638 calls `csc07_warp` (`mapscriptengine_2.asm`): write event type 1,
   parameter 1 zero and parameters 2–5 = `(58,13,35,DOWN)`, then return to the script interpreter.
   `ExecuteMapScript` continues its dispatch loop after each command; the warp does not unwind it.
2. Commands 391–395 execute the follows and F74/F640 writes. Command 396 ends the program.
   The interpreter's `loc_47234` tail conditionally waits for view scrolling, clears view-scroll
   speed and restores registers. The after wrapper then executes the shared `JoinForce(0)` tail.
3. `BattleLoop_Victory` clears F407 and sets F507, reads the request parameters into d0–d3 and
   returns d4=1. This consumer bypasses `ProcessMapEventType1_Warp`; it is not the ordinary
   exploration poll consuming the pending warp. Treating line 638 as the last script operation
   would omit both follower persistence and the earthquake-map switch flag.
4. `code/gameflow/mainloop.asm:alt_MainLoopEntry` (`0x75E0`) runs `SwitchMap`, then directly
   enters exploration. It does **not** run `CheckBattle` again at this outcome return.
5. For effective Map58, `code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop`
   (`0x257C0`) clears `MAP_EVENT_TYPE` immediately, heals, fades, assigns current map and
   `NOT_CURRENTLY_IN_BATTLE`, then loads tilesets and initializes entities. In
   `mapsetupsfunctions_1.asm`, `GetCurrentMapSetup` (`0x4779E`) returns `ms_Void` because
   `data/maps/mapsetups.asm:MapSetups` has no Map58 row. `GetMapSetupEntityList` returns that
   terminator; `j_GetMapSetupEntities` resolves to it through
   `code/common/tech/jumpinterfaces/s07_jumpinterface.asm`. `mapfunctions.asm:InitializeMapEntities`
   (`0x440CC`) clears the old scene entities, initializes flag-selected followers and declares
   the main entity at the supplied `(13,35)`, Down. Rebuilt followers, not the three old scene
   entity IDs, survive this handoff.
6. Exploration clears temporary flags 256–383, sets F80, loads the map, installs base VInt
   functions and invokes `RunMapSetupInitFunction` (`0x474FC`, `mapsetupsfunctions_1.asm`).
   Its `ms_Void` check skips the init-function call. There is no hidden Map58 init scene to extend
   the after program. F385, F74, F640 and F507 are outside the temporary clear range.
7. After the conditional music/fade calls, `WaitForEvent` (`0x2591C`) with no pending event
   follows entity 0 and calls `SetControlledEntityActScript(0)`.
   `code/common/scripting/entity/entityfunctions_2.asm` selects `eas_ControlledCharacter` for
   `PLAYER_TYPE=0`, as set by BattleLoop. This is the first static control-restoration boundary.

`ClearEntities` in `code/common/scripting/entity/entityfunctions_1.asm` clears positions, travel,
actscript state and entity-index mappings. It is not proof of canonical combatant HP changes.
Other follower flags remain inputs: in particular, an intro-skipped F83 can enable additional
king/minister rows alongside F74, and the initializer does not deduplicate those rows. Do not
promise a fixed total follower count or correct arbitrary conflicting caller configurations.

| Return condition | Effective destination / first-control claim |
| --- | --- |
| First victory; F999 clear | Map58 `(13,35)`, Down; F407 clear, F507/F385/F74/F640 set; F83 clear only if established by the executed intro or caller. Void setup, F74-selected followers, then the control boundary above. Natural service completion remains Unknown. |
| Repeat victory; F999 clear | Map58 at the seeded variable position/facing. Same void setup/control path, but follower and jewel/earthquake flags are retained inputs because the after script did not run. |
| Requested Map58; F999 set | `SwitchMap` substitutes Map59 without changing the coordinates/facing. Map59 selects `ms_map59` or, with F543 set, `ms_map59_flag543`. Default init with F999 set can request another Map59 warp when F443 is set (`cs_5EB26`); the F543 variant enters `cs_615E6`. These are late-campaign owners under `data/maps/entries/map59/mapsetups/`, outside this story slice. No universal immediate player-control claim covers this alternate state. |

**Confirmed immediate outgoing record:** Map58's `3-flag-events.asm` and `4-step-events.asm`
contain only terminators. `6-warp-events.asm` (`0xB8A26`) contains the no-scroll marker
`(255,37)` → Map60 `(8,52)`, Right; 255 is the X wildcard. This is a later field request,
not the battle's return warp. The next owner is Map60's map-content/warp surface and its subsequent
evacuation route. Whether the player can walk from `(13,35)` to that marker with the selected
layout/followers, and what happens after arrival on Map60, remain outside this acceptance boundary.

## Coverage, gaps and reproduction

Fixture names below are under `tests/fixtures/h2/`; command names have prefix `uv run sf2 h2`.
Covered means the named source/handler/table boundary, not a fresh execution of every rail.

| Relation / owning evidence | Disposition and command | Missing relation or discriminating control |
| --- | --- | --- |
| Count/outcome/heal, defeated/after routing and shared join | **Covered:** `battle-control-static-v1.json`, `battle-loop-static-v1.json`, `battle-cutscenes-static-v1.json`, `battle-global-data-static-v1.json`; [battle loop](../../battle/battle-loop.md), [cutscene routing](../../battle/battle-cutscenes.md). Commands `battle-control`, `battle-loop`, `battle-cutscenes`, `battle-global-data`. | Fresh `_cutscene_facts` equals the fixture. Selected row 7 and no Battle07 position override were read directly. Controls: living enemies after slot-128 death; zero allies and enemies favors defeat; F507 skips scene but not join. |
| Full selected program, text, entity and map commands | **Covered:** `map-script-engine-static-v1.json`; [common scripting](../../world/common-scripting.md). Command `map-script-engine`; direct `gamescript.txt` comparison. | Retained fixture comparison passes; commands 0–396 retain exact order. Controls: camera `(8,16)` versus player `(13,35)`; effect actors versus battle roster; F385 write after narration versus item transfer; warp before follows/flags. No scene-specific new fixture is justified. |
| F385 member-screen display and F74 follower declarations | **Reuse/extend existing owners** if executable composition is later needed: common-menu/common-scripting inventories plus source call sites above. Commands `common-menus`, `common-scripting`, `map-script-engine`. | Separate inventories do not assert scene write → conditional menu/follower rebuild. Minimum public-safe output: producer command, input flags, selected consumer branch/row identities. Controls: F384 clear with F385 set; F83+F74 versus cleared F83; extra enabled follower rows. Existing source reads suffice for this static prose; no new command/platform. |
| Warp → continued script → victory fields → SwitchMap → void init/control | **Reuse/extend existing owners; bounded H2 candidate only if a later contract requires an executable composed predicate.** `map-event-request-consumption-static-v1.json`, `map-setup-static-v1.json`, `map-init-static-v1.json`, common-maps and map-script-engine; [request consumer](../../world/map-event-request-consumption.md), [map inventory](../../world/map-data-inventory.md). Commands `map-event-request-consumption`, `map-setup`, `map-init`, `common-maps`, `map-script-engine`. | Missing executable relation is the complete ordered producer/caller/reset/setup composition, not any single known handler. Current rails do not prove this end-to-end return. Minimum output: input F507/F999/F83, producer/consumer IDs, request tuple, final flags, selected setup and control-or-pending-event result. Controls: F507 set retains seed; stopping at warp misses F74/F640; F999 selects Map59; an init-written event prevents immediate controlled-script installation. Extend existing owner(s), not a per-battle fixture. |
| Natural victory/escape, scene completion, field walk and reentry | **Deferred runtime Unknown:** [battle loop](../../battle/battle-loop.md), [common scripting](../../world/common-scripting.md), [map content](../../world/map-content.md). | Static command order cannot prove service timing, HP at a natural caller, collision reachability, actor overlap or completed presentation. No acquisition or emulator queue is authorized. |

This review used bounded source reads and small in-memory selections. The two retained payloads
sum to 18,285,979 bytes (about 18.3 MB); they were read, not rewritten. Source work was linear in
the selected files/program, with no layout decode, full transcript/roster publication, capture,
installation or new environment. A materially broader service or traversal closure needs a new
bounded agreement; preserve its Unknown rather than expanding this slice.

Load the owning ignored `local/private-inputs.ps1` in the command's PowerShell process. Reproduce
the retained payload and fresh cutscene-fact comparisons with the exact recipe in the
[predecessor's reproduction boundary](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Then select `programCorpus.programs` entry `id == "abcs_battle07"` from
`local/derived/map-script-engine-static.json`. Its `sourcePath`, `sourceLine`, `index`, `macro`
and `arguments` reproduce every phase and tail anchor above. Starting at its `textCursor`, increment
once per `nextText`/`nextSingleText` under ordinary unskipped execution and compare all 76 comments
to `gamescript.txt` hexadecimal IDs 0951–099C. The selected source tables/functions above reproduce
return, setup, follower and menu predicates without extracting new data.

Record prose/source-reference/link/private checks, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, actual final-head CI and completed failures
in the Issue/PR handoff. Normal/full/engine/H3/H4/Godot suites are not selected for this document-only
scope. Preserve retained inputs/results, freeze the clean pushed Draft PR and leave integration to
main-gate. The next independent slice starts from the Map58-to-Map60 outgoing owner; it is not
implicitly authorized to claim Battle08 admission.
