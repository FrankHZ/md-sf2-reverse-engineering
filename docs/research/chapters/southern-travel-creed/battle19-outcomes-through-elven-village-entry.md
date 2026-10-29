# Battle19 outcomes through Elven Village entry

Evidence date: 2026-10-14. **Confirmed:** Battle19's four scene pointers select
`ms_Empty`, but its leader cleanup, outcome mutations and shared Bowie join
still execute at their respective gates. Victory returns to Southwest Parmecia;
defeat/admitted escape use the supplied savepoint. Independently, Map68's
`(48,30)` entrance selects Elven Village23 without testing Battle19 completion.
**Inferred:** this is the named village-arrival segment. **Unknown:** natural
travel/outcomes, actual supplied state, completed services, usable control and
persistence. No mandatory victory-before-entry ordering is established.

## Agreement and provenance

Standard rigor under [Issue #856](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/856)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Accepted starting base: `b49b787198077c420124024ac621ef1daa3789d4`.
This document owns admitted Battle19 entry/outcomes and independent Map23
entrance through immediate population/init/conditional control. There are no
shared edits or stacked dependencies. Stop at immediate outcome return,
conditional Map23 control, or an unrelated battle admission. Exclude combat
simulation, terrain/path reconstruction, village NPC/zone/description/item/
treasure activation, stairs, Map24 travel, Battle20, later journeys and runtime
acquisition. F762 is a supplied later-story content selector only; its producer
and fairy/Creed events are outside this slice. Significant new coupling requires
replanning rather than extending that boundary.

Reuse accepted [Caravan travel and Battle19 admission](./caravan-boarding-and-southwest-parmecia-travel.md),
[Battle18 outcomes](./battle18-outcomes-and-caravan-acquisition.md),
[Battle17 shared entry/outcome consumers](./hassan-departure-and-battle17-outcomes.md),
[battle lifecycle](../../battle/battle-loop.md), [cutscene wrappers](../../battle/battle-cutscenes.md),
[membership](../../party/common-stats.md), [map requests](../../world/map-event-request-consumption.md)
and [Rick's current following](./hassan-rick-recruitment.md).
All source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained payloads bind this source
and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Battle files with shortened paths are under `code/gameflow/battle/`; map and
follower consumers are under `code/common/scripting/map/`, entity consumers
under `code/common/scripting/entity/`, and exploration consumers under
`code/gameflow/exploration/`. Fnnn means flag nnn; table/row indices are zero-based. Coordinates and waits are
source operands, not observed positions or durations.

The existing map-init/map-script-engine pair totals 18,285,979 bytes. Linear
retained comparisons plus bounded selected source/fixture reads were budgeted
below 512 MiB and one minute for comparisons, with small prose/receipts and no
regeneration, new environment, runtime or verification machinery. Comparisons
passed in 0.085 seconds. No game-material upload is needed; reassess before
material resource/scope growth.

## Admitted Battle19 and selected entry graph

**Confirmed:** `sf2enums.asm` resolves BATTLE_OUTSIDE_ELVEN_VILLAGE19,
MAP_OVERWORLD_SOUTHWEST_PARMECIA68, MAP_ELVEN_VILLAGE23 and RIGHT0.
Battle flag bases400/450/500 select F419/F469/F519. The accepted admission owner
traces walking's Map68 default-zone callback and the packed reload request;
Caravan mode1 has no equivalent zone producer. A supplied F419 can independently
admit the battle at MainLoop's next ordinary map scan.

`data/battles/global/battlemapcoords.asm` row19 is the only Map68 row: origin
`(5,19)`, size48x32, trigger `(255,255)`. `code/common/maps/getbattle.asm:CheckBattle`
tests the unlock and trigger bytes, not containment in that rectangle. With
F419 set, the wildcard triggers admit19 regardless of the returned coordinates.
F519 set clears F419 **after selection without canceling this entry**. There is
no F65, F518, F762, sword, membership or vehicle-mode predicate here.

`code/gameflow/battle/battleloop_1.asm:BattleLoop` clears PLAYER_TYPE to0 and
sets F399 before testing F88. Fresh entry clears seconds, writes map68/battle19,
sets base VInts and invokes the before wrapper. It restores the incoming map/
battle, clears region flags90–105, heals living/immortal allies, initializes
ally/enemy positions, clears AI memory, calls LoadBattle with d0=0 and invokes
the start wrapper. These are ordered source calls, not a completed playable turn.

Select slot19 from the actual tables in `data/battles/cutscenes/`:

| Table | Actual target and wrapper under `code/gameflow/battle/cutscenes/` |
| --- | --- |
| `beforebattlecutscenes.asm` | `ms_Empty`; before wrapper skips when F469 set, otherwise executes without setting F469. |
| `battlestartcutscenes.asm` | `ms_Empty`; start wrapper skips when F469 set, otherwise sets F469 before execution. |
| `battleendcutscenes.asm` | `ms_Empty`; defeated wrapper requires Bowie alive and enemy128 dead; F519 clear dispatches, set skips to its shared leader tail. |
| `afterbattlecutscenes.asm` | `ms_Empty`; after wrapper dispatches when F519 clear, skips when set; both paths reach EndAfterBattleCutscene. |

`code/common/scripting/map/ms_empty.asm` at `0x47A4E` contains only `$FFFF`.
The map-script reader still performs shared entry/termination work, including
skip-state initialization and conditional view-scroll waiting/camera-speed
clear. These targets provide no Battle19 dialogue, warp, custom scene actors,
inline movement or story recruitment. Empty targets do not erase wrapper work,
F469, service waits, or the join/leader tails. F469 is independent of completion
and is not reset by a loss or repeat.

F88 set instead restores saved seconds, clears F88 and AI memory, clears the
view target, calls LoadBattle and branches directly to the individual-turn loop.
It bypasses incoming map/battle assignment, fresh healing/placement, before/start
and initial round generation. It also lacks fresh entry's explicit d0=0 before
LoadBattle. Saved map/battle/area/roster/turn state must agree; an incoming19
argument alone does not establish a consistent resumed Battle19.

### Actual initial roster and repeat boundary

Initialization clarification date: 2026-10-23. The
[complete initialization and first-control account](./battle19-initialization-and-first-control.md)
now owns the full definitions, F519 selection, activation/orders, Shining Ball
custody and conditional first control. The following roster is entry context;
it does not supersede that account's computed consumer boundaries.

**Confirmed:** `getbattlespritesetsubsection.asm` selects slot19 of
`data/battles/spritesets/entries.asm`: BattleSpriteset19 at `0x1B46BE`.
The complete `spriteset19.asm` has 12 ally rows, 15 enemy rows, three AI regions
and one AI point. Ally placement order is `(40,12)`, `(40,13)`, `(40,11)`,
`(41,11)`, `(41,12)`, `(41,13)`, `(42,13)`, `(42,12)`, `(43,13)`, `(43,12)`,
`(42,11)`, `(40,10)`. These are battle-local positions. For example the first
placement plus area origin is `(45,31)`, not the village entrance or a fixed
victory return.

`battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions` first
clears all30 ally X values and rebuilds current membership. Active-list identities
receive positions; dead members consume an iteration/list slot without advancing
the placement row. Remaining members beyond capacity can leave the active party.
Twelve placements do not establish twelve living participants. Shared healing
restores living allies and immortal Peter/Lemon to HP/MP maxima. Its intermediate
status mask retains STUN/POISON/CURSE; the subsequent UpdateCombatantStats
preserves STUN/POISON and derives final CURSE from equipped cursed items.
Old CURSE alone does not survive final refresh. This does not revive every dead
ally or grant membership/following.

| Enemy slots | Base type and local starting positions, in slot order |
| --- | --- |
| 128–130 | ORC `(14,19)`; HIGH_PRIEST `(15,18)`; SOULSOWER `(16,21)` |
| 131–133 | GARGOYLE `(13,18)`; HIGH_PRIEST `(16,12)`; ORC `(17,13)` |
| 134–136 | DARK_MADAM `(26,14)`; SOULSOWER `(24,16)`; GARGOYLE `(26,21)` |
| 137–139 | ORC `(23,27)`; ARROW_LAUNCHER `(30,13)`; SOULSOWER `(32,15)` |
| 140–142 | SOULSOWER `(38,21)`; ARROW_LAUNCHER `(40,22)`; GARGOYLE `(38,3)` |

All15 rows are STARTING, not hidden/respawn rows. Slot128's source AI is LEADER,
with MOVE_TO point0 `(14,19)`; slot129 carries
SHINING_BALL|USABLE_BY_AI|UNUSED_ITEM_DROP. These are initialization operands,
not a simulated tactic, reward or item-use outcome. The remaining slots in the
32-enemy initialization follow its hidden/cleared path. The three region records
and one point are retained inputs; no activation, turn or damage simulation follows.

`InitializeEnemyStats` calls `battleloop/upgradeenemies.asm:UpgradeRandomBattleEnemies`
before copying enemy definitions and applying row coordinates/AI/items.
`data/battles/global/randombattles.asm` includes19; DetermineBattleUpgrade admits
the upgrade branch when F519 is set. Its existing movetype/level/class/random
algorithm may retain or change a base identity. F519 alone does not identify a
computed upgraded roster, nor does F469 control this decision. No random roster
is calculated here.

`battlefunctions/loadBattle.asm:LoadBattle` uses
`code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities` to
clear actors/lookups and declare placed combatants, excluding neutral-marked
enemies from its regular enemy loop. The complete
`data/battles/global/battleneutralentities.asm` has no Battle19 entry; there is
no Taros-scene Caravan neutral to carry into this battle. Exploration followers
and any current-only Rick/list extension are not preserved as battle actors.
LoadBattle then selects the view target, loads map/sprites/terrain, installs
VInts and requests music/fade. Fresh start proceeds to activation/region/spawn/
turn-order work after its empty start wrapper. Service completion and a usable
first turn remain Unknown.

## End checks and first/repeated outcomes

**Confirmed:** BattleLoop calls ExecuteBattleCutscene_Defeated after an individual
turn, then ProcessKilledCombatants and CountRemainingCombatants. Zero allies
wins the branch order over zero enemies. If neither ends battle, after-turn
effects are followed by another killed-list pass and the same defeat-before-
victory checks, without another defeated-hook call at that second checkpoint.
The counter requires nonnegative X and nonzero HP and forces allies to zero
when Bowie's HP is zero.

The defeated wrapper's Bowie-alive/enemy128-dead gate precedes both dispatch
and cleanup. `data/battles/global/enemyleaderpresence.asm` row19 is -1, so both
eligible F519 branches reach `battleendcutscenesend.asm:loc_47C48` and append
all nonzero-HP enemies among32 slots to DEAD_COMBATANTS_LIST with its -1 tail.
This does not test placement or directly zero their HP. ProcessKilledCombatants
performs death presentation/bookkeeping, writes listed combatant X/Y=-1, clears
status, refreshes stats and relocates mapped actors to raw `(0x7000,0x7000)`.
They then disappear from the placed-living count. Slot128 death is thus a
conditional leader-cleanup trigger, not unconditional victory if Bowie also dies.
The LEADER AI name is not the authority for this rule; the leader-presence row is.

`ApplyPositionsAfterEnemyLeaderDies` and `data/battles/global/afterbattlepositions.asm`
select only Battle05; Battle19 reaches the terminator without their position/HP
writes. There is no Battle19 staged end position to substitute for the return seed.

| Outcome | Ordered selected result |
| --- | --- |
| First victory, F519 clear | Heal eligible allies, seed return, execute empty after target, shared Bowie join, clear F419/set F519, return d4=1. |
| Repeated victory, F519 set | Same seed/join/completion writes; skip the after script, return d4=1. |
| Defeat | Update unlock, exhaustion music/text, restore Bowie's HP, halve gold by unsigned shift, choose savepoint, return d4=-1. Battle4's special losable branch does not apply. |
| Admitted Egress / Angel Wing | Consume MP / remove selected item slot, text/flash/closure, update unlock, unwind action frame, choose savepoint, return d4=0. No victory join/completion or defeat gold penalty. |

`battlefunctions/executeindividualturn.asm` checks the action kind and masked
spell/item identity before branching to `battlefunctions/battlefunctions_1.asm`.
This establishes the admitted consumer, not menu availability, sufficient MP or
item custody. UpdateBattleUnlockedFlag preserves F419 if F519 clear, otherwise
clears it. Neither defeat nor escape sets F519 or resets F469. No Battle19 path
here sets F65, clears F84, grants a new recruit, or unlocks Battle20.

### Victory seed and fresh Southwest Parmecia return

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` seeds parameter2 from
CURRENT_MAP, parameters3/4 from the first current active member's combatant X/Y
plus BATTLE_AREA_X/Y using byte arithmetic, parameter5 from that member's mapped
entity facing, and parameter1=0. UpdateForceAndGetFirstBattlePartyMemberIndex
rebuilds the flag-selected list before choosing its first entry. The seed is not
necessarily Bowie, the first living ally, an initial placement, or `(48,30)`.
Empty/inconsistent membership or lookups have no established repair here.

For consistent Battle19 state this is Map68 with local X+5/Y+19 and the sampled
facing. The empty after script preserves the tuple. Both F519 branches then use
`cutscenes/afterbattlecutscenesend.asm:EndAfterBattleCutscene`, whose consumed
`data/battles/cutscenes/afterbattlejoins.asm` row19 is0. The s07 layout includes
that table, not the similarly named legacy global file. Its call through
`j_JoinForce` to `code/common/stats/battleparty.asm:JoinForce(0)` sets Bowie's
joined F0, rebuilds force lists and activates him if active count is below12.
The source's unused-feature comment does not remove the call. It occurs after
return-member selection, not as a new village recruit. Only then does victory
clear F419, set F519 and read the seeded tuple for return; F469 remains set/supplied.

`code/gameflow/mainloop.asm:alt_MainLoopEntry` calls SwitchMap then ExplorationLoop
**without CheckBattle**. The complete flag-switched table has no source68 row.
Fresh supplied-map exploration clears pending, subtracts20000 from STEP_COUNTER
as a word with a negative-result clamp, heals eligible allies, fades/loads, writes
CURRENT_MAP=68/CURRENT_BATTLE=255, rebuilds population, clears temporary flags
256–383, sets F80 and runs init. This is not a preserved battle population or
an immediate repeat-battle scan.

Map68 has one setup, an empty fixed entity list and return-only init. F65 set
selects the overworld follower table: Caravan/FOLLOWER_A first at physical1,
then admitted normal followers. F65 clear uses only normal followers. Current
flags select Peter/F67, Oddler/F68, Rohde/F84 and the other rows; Battle19 victory
does not repair any of them. Later conflicting FOLLOWER_A rows can overwrite
its lookup. F65 supplies a follower independently of PLAYER_TYPE; normal battle
entry already set mode0, so victory does not automatically board the Caravan.
F64 and saved raft coordinates are preserved. The shared fresh population tail
can declare a separate raft if F64 set and saved map matches68.

After init and conditional music/fade, pending zero permits WaitForEvent to
follow main0 and install current-mode control, normally walking0. A later zone
request or warp has its own producer/admission; this return does not take the
player to the village. Stop at conditional Map68 control.

### Defeat or escape: supplied savepoint and raft effects

**Confirmed:** `battleloop/getegresspositionforbattle.asm` has no Battle19
special return. It falls through to EGRESS_MAP and
`code/common/maps/egressinit.asm:GetSavepointForMap`. Normal BattleLoop has set
F399. The selected map's row in `data/maps/global/savepointmapcoords.asm` supplies
X/Y/facing; no match retains that map with default `(1,1)`, Up. An abnormal direct
F399-clear helper call returns the initial Bowie's-room tuple before savepoint
or raft scanning. Do not turn this fallback into validated arbitrary-map behavior.

With F64 clear, saved RAFT_MAP/X/Y remain unchanged. With F64 set,
`raftresetmapcoords.asm` chooses Hassan9 → Map69 `(28,55)`, Polca13/Bedoe10 →
Map72 `(43,48)`, or sentinel default → Map69 `(42,15)` for every other selected
savepoint map. These are saved-position writes, not boarding or controller changes.
The Battle19 consumer does not normalize mode again; normal battle entry already
did so, and no selected scene changes it.

For supplied EGRESS_MAP23 the savepoint is Elven Village `(61,4)`, Up, distinct
from the independent entrance `(22,29)`, Right. F64 set uses the default raft
reset on69, so it does not place that saved raft in village23. Direct outcomes
again bypass CheckBattle and use the fresh Map23 population/init below, with
the savepoint tuple. This conditional example does not establish where the
player last saved or activate the village's church/NPC behavior.

For any other supplied savepoint, SwitchMap and its selected map's setup/init
remain conditional. Their nonselected continuations are Unknown here; do not
assume every map has Map23's return-only init or a usable pause. A supplied map255
uses ExplorationLoop's existing-population branch (main-property/raft update,
load/init) instead of fresh setup population or temporary-flag clearing. No
all-map investigation or natural loss itinerary is implied.

## Independent Map68 entrance to Map23

**Confirmed:** `data/maps/entries/map68/6-warp-events.asm` row2 matches `(48,30)`
and supplies no scroll, MAP_ELVEN_VILLAGE23, destination `(22,29)`, RIGHT0.
Earlier rows `(55,43)` and `(57,24)` do not match that point. The later border
rows are any-X/Y0, X0/any-Y and X63/any-Y, so none competes there.
`exploration.asm:WarpIfSetAtPoint` divides supplied coordinates by384, scans in
order with negative-byte wildcards, and writes event1, parameters1–4, facing/
padding at parameter5 and WARP_SFX. It checks no F519/F419/F65/F762 or membership.
The accepted walking/vehicle consumers reach this helper through their `$1000`
layout branch after their movement checks; the table match does not prove
terrain reach, transport eligibility, input or a route from the victory seed.

`explorationfunctions_2.asm:ProcessMapEvent` clears pending before dispatch.
The no-scroll warp consumer submits/clears WARP_SFX, idles main0, removes the
exploration return frame, samples its current position, applies nonnegative
requested coordinates and requested facing, and returns `(23,22,29,Right,d4=0)`.
It does not itself heal, rebuild actors, change PLAYER_TYPE or save raft position.
This is MainLoop's normal SwitchMap → CheckBattle route, unlike direct outcomes.

The complete `flagswitchedmaps.asm` has no source23 substitution, and the complete
45-row battle-map table has **no Map23 row**. Row19 belongs to68; row44 belongs
to24, not23. F419/F519 or another battle's unlock therefore cannot select a battle
on this unchanged23 tuple. F762 changes the setup pointer, not the map or battle
scan. No winning19 prerequisite is established. This does not prove that a
natural pre-victory walk reaches the entrance. Replaced pending/map state is a
separate supplied condition, not an atomic guaranteed transition.

## Fresh Map23 population, init and conditional control

**Confirmed:** the supplied-map ExplorationLoop follows the fresh sequence above,
now writing CURRENT_MAP23/CURRENT_BATTLE255. `data/maps/mapsetups.asm` has default
ms_map23 and exactly one flag alternative, F762 → ms_map23_flag762.
`mapsetupsfunctions_1.asm:GetCurrentMapSetup` starts with the default and replaces
it for a set matching flag. F519 is not a selector. Both pointer tables in
`data/maps/entries/map23/mapsetups/` have six entries: different entity and entity-
event pointers, shared zone/description/item pointers and the same init pointer.
Those event pointers are content bindings; entering the map does not execute them.

### Complete selected initial actor rows

**Confirmed:** the default pointer at `0x59AC0` selects `s1_entities.asm`
(`0x59AF0`); F762 alternate at `0x59AD8` selects `s1_entities_762.asm` (`0x59B22`).
Each list ends at msEntitiesEnd. These are ordinary non-ally mapsprites, with
sequential logical NPC identities and physical slots after admitted followers:

| Setup / logical ID | Initial tile, facing | Mapsprite and initial action data |
| --- | --- | --- |
| Default128 | `(18,9)`, Down | MAN194; walking origin `(18,9)`, range1. |
| Default129 | `(17,11)`, Right | DWARF226; eas_Init. |
| Default130 | `(18,11)`, Left | OLD_MAN198; eas_Init. |
| Default131 | `(54,3)`, Down | WORKER202; eas_Init. |
| Default132 | `(8,12)`, Right | ELF_FEMALE224; walking origin `(8,12)`, range1. |
| Default133 | `(61,3)`, Down | PRIEST192; eas_Init. |
| F762 alternate128 | `(18,9)`, Down | MAN194; walking origin `(18,9)`, range1. |
| F762 alternate129 | `(56,6)`, Down | WORKER202; walking origin `(56,6)`, range1. |
| F762 alternate130 | `(18,11)`, Up | OLD_MAN198; eas_Init. |
| F762 alternate131 | `(61,3)`, Down | PRIEST192; eas_Init. |

`sf2mapsetupmacros.asm:msWalkingEntity` encodes `$FF` plus origin X/Y/range in
the action field. `entityfunctions_1.asm:DeclareNewEntity` resolves that negative
field through SetWalkingActscript, copying the50-byte eas_Walking template and
patching its three operands. Fixed rows install eas_Init. Declaration sets current/
destination coordinates, zeroes velocities/travel and installs an action; these
are initial values, not a frozen pose. The template/actions can subsequently
execute. No random-walk trajectory or NPC event is evaluated here.

`mapfunctions.asm:InitializeMapEntities` clears physical actors and lookups,
allocates admitted followers starting at1, then these six/four NPCs. It declares
Bowie/main0 separately at the incoming tuple using GetAllyMapsprite, with eas_Idle,
and terminates EXPLORATION_ENTITIES. Only followers were appended to that fresh
list; neither main0 nor these ordinary NPC rows are appended. If n followers
were allocated, the NPCs begin at n+1. Their logical128 onward identities are
not their physical slot numbers. Current-only Rick is not a fresh follower row.

Map23 is neither an excluded-follower map nor an overworld. Therefore
`followersfunctions_1.asm:InitializeFollowerEntities` uses the normal table
regardless of F65. The complete order in `data/scripting/entity/followers.asm` is:
F66 Sarah; F67 Peter; F72 Kazin; F73 Slade; F74 Astral/FOLLOWER_B; F66 Chester;
F68 Oddler/FOLLOWER_B; F75 Luke; F69 fairy/FOLLOWER_C; F70 Astral/FOLLOWER_B;
F74 king/FOLLOWER_A; F71 Lemon; F74 minister/FOLLOWER_D; F76 Zynk;
F77 old man/FOLLOWER_A; F82 Frayja; F83 king/FOLLOWER_A;
F83 minister/FOLLOWER_D; F84 Rohde. Each row tests its flag, without an HP or
force-membership admission test. Ally sprite selection has separate class/HP/
Rohde-membership rules; outside battle a dead ally selects blue flame.

No Caravan follower is added solely by F65 on23. F65 is not cleared, nor are
F84/other follower flags repaired. Conflicting rows can overwrite a logical lookup
while leaving earlier physical actors present; excessive supplied followers
have no capacity repair established here. With only F67/F68 among these flags,
Peter is physical1 and FOLLOWER_B physical2, fresh list `[1,2,-1]`; default NPCs
are3–8, alternate NPCs3–6. This is a supplied-state example, not a natural roster.
A supplied F69 follower is not evidence that F762 produced that flag in this slice.

### Transport, temporary flags and control

**Confirmed:** the no-scroll entrance, setup selection and shared Map23 init do
not clear PLAYER_TYPE. Main0 initially uses Bowie's class/HP sprite. The fresh
population tail `followersfunctions_2.asm:sub_44404` handles actual raft mode2
specially: give main0 the raft sprite, move the current follower list to raw
`(0x6000,0x6000)` with standing actions, and declare separate physical31/lookup63
raft at `(64,64)`. It does not rewrite saved RAFT_MAP/X/Y. With mode other than2,
F64 plus matching saved map23 declares that separate raft at saved coordinates;
otherwise this fresh tail adds none. It does not replay Caravan boarding or
restore a Caravan sprite merely because mode1 was supplied. NPCs are not in the
follower list hidden by the mode2 branch.

After entity/sprite population, ClearMapSetupTempFlags clears256–383; its caller
then sets F80. F762, F519, F65/F64 and follower flags lie outside that reset range.
LoadMap/base VInts precede RunMapSetupInitFunction, which resolves the current
setup again. Both choices call `ms_map23_InitFunction` at `0x59C04`, whose complete
body is `rts`. It supplies no scene, request, actor move, mode change, recruitment
or entrance relocation. The later-story alternate's producer remains excluded.

After conditional map music/fade, WaitForEvent checks pending before installing
control. Pending zero follows physical0 and invokes
`entityfunctions_2.asm:SetControlledEntityActScript`: mode0 installs
`eas_ControlledCharacter`/opcode2, mode1 installs `eas_Raft`/opcode7 (actual
Caravan), and mode2 installs `eas_Caravan`/opcode8 (actual raft). The fallback for
other nonzero/non1 values also selects the last script; no validity repair is
claimed. Source vehicle names do not override the numeric consumer mapping.

For normal battle outcomes mode0 comes from BattleLoop entry; the independent
entrance retains whatever mode reached its consumer. Population alone therefore
does not establish walking, a boarded Caravan, a valid raft state or usable
input. Any intervening pending request wins before ordinary controller install.
Services and asynchronous initial actor actions remain conditional. Stop here,
before any village interaction, exit/stair, Map24 or later battle content.

## Reproduction and remaining limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Verify source pin/clean status and use the existing
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init payload/fixture equality, `_verify_map_script_engine_fixture`, and
source `_cutscene_facts` against `battle-cutscenes-static-v1.json`. All passed
unchanged; these are retained comparisons, not a new composed runtime receipt.

| Evidence owner | Selected direct join and limit |
| --- | --- |
| Retained `local/derived/map-init-static.json` | primarySourceBodies sourceOrder38 is Map23's single rts; routeJoins59/60 and pointerTableJoins62/63 bind default/F762 to that same init. Join their paths/symbols/operations to the pinned source. |
| `battle-cutscenes-static-v1.json` | Complete shared wrapper comparison; direct slot19 pointer/leader/join reads supply the selected content. Resolve the s07 layout's actual join-table include. |
| `battle-routing-data-static-v1.json`, `battle-spriteset-data-static-v1.json` | Aggregate context only: routeTargetsParsed/rowContentTracked limits remain. Read complete selected tables and spriteset19, not guessed per-battle scene files or a claimed fixture roster guard. |
| `battle-loop-static-v1.json`, `battle-control-static-v1.json`, `common-stats-static-v1.json` | Reuse accepted initialization/outcome/membership algorithms; direct Battle19 branch selection adds no combat simulation or random upgraded roster. |
| `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-event-request-consumption-static-v1.json` | Shared selection/population/request context; direct Map68 warp, complete competing tables, both Map23 entity lists/macros and immediate consumers establish this bounded entrance. |

Review full selected bodies, stripping comments for opcode/operand comparisons.
Resolve sf2enums constants, table index19, actual include/alias targets, F469/F519
branch polarity, leader-before-count/defeat-before-victory order, pre-join seed,
savepoint/raft selectors and fresh-versus-existing population. Supplied-state
counterexamples include F519-clear village entry, completed19 with F419 set,
Bowie/enemy128 both dead, F88 resume, changed active membership, F65 independent
of mode, F762 alternate, F64/saved map mismatches and replaced pending state.
They constrain static review; they are not executed game scenarios.

**Unknown:** naturally reachable entrance/terrain/route, winning trajectory,
actual flags/HP/roster/mappings/random seed/savepoint/transport, service and
initial-action completion, rendered presentation, responsive input and persistence.
Nonselected savepoint init remains Unknown; F762 does not authorize its later-story
producer. These gaps remain deferred under
[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md).

Preserve [PR854 final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/854#issuecomment-6161892874)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/854#issuecomment-6161933230),
including their original/rebased checks, corrected inspections and linked
PR849/846/842 results. Lookup misses, legacy duplicate files and truncated reads
are not absence evidence; locate and reread the actual owners. Current inspection
corrections and exact candidate/check/CI/process state belong in this Issue/PR
handoff. Acceptance is complete owned-prose/source/provenance/link/private review,
`git diff --check`, clean committed configured
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI.
No blanket local normal/full/engine/Godot/H3/H4 suite is selected solely for prose.
Leave a clean frozen Draft PR for independent main-gate review and integration.
