# Battle23 initialization and initial roster

Evidence date: 2026-10-14. **Confirmed:** fresh Willard/Battle23 entry selects
empty before/start scripts, heals eligible allies, initializes twelve party
placement slots and twelve enemy records, then loads the battle. It declares
one AI region and one point, with no Battle23 neutral-table population.
Suspended entry bypasses that fresh setup and reloads current state.
**Inferred:** the Willard, Rat and Bubbling Ooze formation stages the burrow
encounter. **Unknown:** natural admission, supplied state, completed services,
actual roster/presentation/control and persistence. Declared AI is not an
executed tactic; an empty hook does not mean initialization was skipped.

## Agreement and provenance

Standard rigor under [Issue #932](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/932)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The uncertainty is the complete selected data-to-consumer join and saved-state
contrast; the result supports the next English story/battle guide. Starting
accepted base is `18ed9278764b29950e280d03913a8ae00ed97a03`, including #928/PR931.
This is the sole owned document, without a stacked dependency on #930.
No shared index, registry, schema, fixture, tooling, design or remake changes.

Begin at [Willard Burrow through Battle23 admission](./willard-burrow-through-battle23-entry.md).
Reuse the corrected [Battle22 lifecycle](./battle22-initialization-and-roster.md),
[battle-loop research](../../battle/battle-loop.md), [control/lifecycle contract](../../../design/contracts/battle/battle-control-lifecycle.md),
[cutscene routing](../../battle/battle-cutscenes.md) and [spriteset inventory](../../battle/battle-spriteset-data.md).
The [activation, AQUA2 and first-control continuation](./battle23-initialization-and-first-control.md)
owns the subsequent region/order/round/conditional-dispatch join and precise
production conversion limits. This initial-roster boundary remains unchanged.
Fresh entry stops at `BattleLoop:@Start`, **before ActivateEnemies** (the actual
source label), region activation and initial round creation. Suspended entry
stops at `@IndividualTurns_Loop`, before reading/executing an individual turn.
No damage, turn/AI simulation, leader-death/outcome scripts, postbattle
population, Creed route, runtime acquisition or new verification machinery.

Source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only checkout is clean at that pin. Retained evidence binds
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Fnnn denotes flag nnn; table and row indices are zero-based. Coordinates and
stat fields below are source operands or explicit initializer results, not
observed positions or combat outcomes. No new extraction/playthrough is claimed.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair and small
existing battle inventories/source selections. Linear comparisons are budgeted
below 512 MiB and one minute, plus prose and small receipts; no payload
regeneration, environment creation or capture. The retained pair/shared
cutscene-source comparison passed in 0.081s with size/mtime unchanged.
Reassess before material expansion.

## Admission and ordered fresh entry

**Confirmed:** the predecessor owns the F781-guarded exploration confrontation,
F423 write and requested Map27 `(0,0,Right)` warp. Those are not Battle23
before/start hooks. F423 may already be supplied, permitting direct admission
without playing that scene. F523 selects exploration population; F781 suppresses
the confrontation. Neither substitutes for the battle intro flag F473.

`code/common/maps/getbattle.asm:CheckBattle` and the complete
`data/battles/global/battlemapcoords.asm` select the sole Map27 row 23:
`27,23,14,27,16,255,255`. F423 must be set. Both trigger coordinates are
wildcards; there is no crop-containment test. The selected row writes area
origin `(23,14)` and size 27x16. F523 set clears F423 but still returns 23 for
this call; F523 clear retains it. The incoming BattleLoop pair is map27/battle23.
The predecessor's low-byte coordinate limitation remains: an explicit warp
coordinate replaces only the low byte of the divided position word, and a
wildcard battle trigger does not normalize an abnormal high byte. This
initialization record does not turn a conditional incoming route into natural
traversal or a guaranteed coherent warp.

`code/gameflow/battle/battleloop_1.asm:BattleLoop` at `$23A84..$23BB4` first
clears PLAYER_TYPE and sets F399, then tests F88. With F88 clear:

1. Clear SECONDS_COUNTER. Save incoming d0/d1 words; set CURRENT_MAP=27 and
   CURRENT_BATTLE=23; install base VInts and execute the before wrapper.
2. After its return restore incoming words and reassign current map/battle.
   Clear all region flags 90–105, including F90 for the one declared region.
3. Call HealLivingAndImmortalAllies, InitializeAllAlliesBattlePositions,
   InitializeAllEnemiesBattlePositions and ClearAiMemory, in that order.
4. Clear d0 to 0, call LoadBattle, then execute the start wrapper.
5. Reach `@Start`; stop before its ActivateEnemies call.

Thus fresh healing and roster initialization follow the before wrapper.
Previously current map/battle or exploration positions do not select the fresh
roster after the explicit assignment. PLAYER_TYPE is cleared even for a supplied
raft mode; this does not establish that every retained raft field was erased.
Each later step requires the earlier calls to return; there is no rollback of
completed prefix writes when a later service does not finish.

### Empty hooks still have wrapper effects

**Confirmed:** the complete 48-word pointer tables
`data/battles/cutscenes/beforebattlecutscenes.asm` and
`battlestartcutscenes.asm` both select `ms_Empty` at slot 23, offset 46.
Its entire body in `code/common/scripting/map/ms_empty.asm` is `$FFFF` at
`$47A4E..$47A50`. This is a positive table/body join, not filename inference.

The four actual wrapper files in `code/gameflow/battle/cutscenes/` are
`beforebattlecutscenesstart.asm`, `beforebattlecutscenesend.asm`,
`battlestartcutscenesstart.asm` and `battlestartcutscenesend.asm`:

| Wrapper | F473 clear | F473 set | Return |
| --- | --- | --- | --- |
| ExecuteBeforeBattleCutscene | Add CURRENT_BATTLE to intro base 450, test; save d0/a0, select relative word 23*2 and call ExecuteMapScript; no F473 write | Skip dispatch | Restore d0/a0 on dispatched path, then d1 at loc_47AE8 and rts |
| ExecuteBattleStartCutscene | Same test; **set F473 before dispatch**, save d0/a0 and execute empty script | Skip flag write and dispatch | Restore d0/a0 on dispatched path, then d1 at loc_47B8C and rts |

`mapscriptengine_2.asm:ExecuteMapScript` initializes its work pointer, saves
registers and clears the text-skip byte before reading the terminator (the
shared debug/input check can set that byte again). The `loc_47234` tail waits
for view scrolling if DIALOGUE_WINDOW_INDEX is nonzero, clears
VIEW_SCROLLING_SPEED, restores registers and returns. Empty means no local
text, movement, choice, recruitment or load command; it is not a bare wrapper
rts. A shared wait can prevent completion. F473 is already set during an
unfinished start-script tail, whereas the before wrapper never writes it.
Supplied F473 set skips both scripts while fresh reset/heal/roster/load still
runs. F423/F523/F781 are independent of this gate.

## Complete spriteset and ally formation

**Confirmed:** the complete 45-longword table
`data/battles/spritesets/entries.asm` selects `BattleSpriteset23` from slot 23.
`spriteset23.asm` occupies `$1B4BFA..$1B4D2C`, 306 bytes: four-byte header,
24 twelve-byte combatant records, one twelve-byte region and one two-byte point.
The header is **12 allies, 12 enemies, 1 region, 1 point**.
`getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection` selects by
CURRENT_BATTLE*4, then uses the actual preceding counts and twelve-byte strides.
Offsets from the spriteset start are ally 4, enemy 148, region 292 and point 304.

`sf2macros.asm:allyCombatant/enemyCombatant`, `combatantAiAndItem` and
`combatantBehavior`, with `sf2enums.asm`, define each complete record:

| Offset | Width | Meaning at this source/consumer boundary |
| --- | --- | --- |
| 0 | byte | Ally placement index or ENEMY_ type |
| 1,2 | byte each | Starting X,Y |
| 3 | byte | AICOMMANDSET_ value |
| 4 | word | ITEM_ bitfield |
| 6,7 | byte each | Primary AIORDER_ bitfield and trigger region |
| 8,9 | byte each | Secondary AIORDER_ bitfield and trigger region |
| 10,11 | byte each | Filler and SPAWN_ initialization value; initializer reads the pair as a word |

`defineShorthand` selects the prefixed constant or numeric value;
`defineBitfield` ORs prefixed/numeric terms and emits the requested width.
All twelve ally rows share HEALER1=0, NOTHING=`$007F`, primary NONE=`$FF` /
region 15, secondary NONE=`$FF` / region 15, filler 0 and STARTING=0.
Their index and coordinates, completing every field, are:

| Placement index | X/Y | Placement index | X/Y |
| --- | --- | --- | --- |
| 0 | `(4,5)` | 6 | `(2,6)` |
| 1 | `(4,4)` | 7 | `(4,6)` |
| 2 | `(3,5)` | 8 | `(4,3)` |
| 3 | `(3,4)` | 9 | `(2,7)` |
| 4 | `(3,6)` | 10 | `(3,8)` |
| 5 | `(3,7)` | 11 | `(3,3)` |

The numbers do not prescribe named heroes. In
`code/gameflow/battle/battleloop/initializecombatants.asm`,
InitializeAllAlliesBattlePositions first sets all thirty allies' X to -1.
`j_UpdateForce` resolves to `code/common/stats/battleparty.asm:UpdateForce`,
which rebuilds joined/active lists in ascending ally-ID order. Supplied stale
list order is overwritten. Only X/Y from the next placement row are assigned
to each living listed member; its row index, item and AI fields are not copied.

Healing already ran through all thirty allies, independently of active
membership. `battleloop/heallivingandimmortalallies.asm` restores current
HP/MP maxima for living allies and always for Peter (7)/Lemon (28), masks status to
existing STUN/POISON/CURSE (`$0007`), then calls UpdateCombatantStats. That
shared stat rebuild recomputes current stats/equipment effects and curse;
the intermediate mask is not a guarantee that every final status bit is
unchanged. UpdateCombatantStats excludes incoming CURSE from its next mask,
then equipped cursed ally items can set it again: final CURSE is gear-derived,
not preservation of the incoming bit. Other dead allies are skipped. No class,
inventory or joined flag is replaced by the formation data, and no reserve/follower is auto-enlisted.

A dead listed member consumes a party-loop slot without advancing the placement
pointer. Later living members take earlier unconsumed coordinates. Twelve is
the limit of **processed members**, not twelve guaranteed living actors. Surplus
members have their active flags cleared and X=-1 through LeaveBattleParty;
that helper does not immediately rebuild list/count. Fewer eligible members
produce fewer placements. A supplied zero-length list is not guarded before
the first entry read; this does not promise a safe empty deployment for
arbitrary inconsistent state.

## Twelve enemy records and initialization

**Confirmed:** every enemy row has NOTHING=`$007F`, secondary NONE=`$FF` /
region 15, filler 0 and STARTING=0. Commands resolve to LEADER=14,
ATTACKER1=6, ATTACKER2=7 and ATTACKER3=9. Primary orders resolve to
FOLLOW_TARGET=0, MOVE_TO=`$40`, FOLLOW_ENEMY=`$80`, NONE=`$FF`; the numeric
suffix is ORed into that byte. All differing fields are below; the common
fields above apply to every row without exception.

| Row / combatant | Type (ID) | X/Y | Commandset | Primary order (byte) | Primary region |
| --- | --- | --- | --- | --- | --- |
| 0 /128 | WILLARD (90) | `(18,7)` | LEADER | MOVE_TO\|0 (`$40`) | 15 |
| 1 /129 | RAT (15) | `(20,6)` | ATTACKER3 | FOLLOW_ENEMY\|0 (`$80`) | 15 |
| 2 /130 | RAT (15) | `(20,8)` | ATTACKER3 | FOLLOW_ENEMY\|0 (`$80`) | 15 |
| 3 /131 | BUBBLING_OOZE (16) | `(17,1)` | ATTACKER2 | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 4 /132 | RAT (15) | `(16,2)` | ATTACKER1 | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 5 /133 | BUBBLING_OOZE (16) | `(15,2)` | ATTACKER1 | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 6 /134 | RAT (15) | `(21,12)` | ATTACKER2 | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 7 /135 | BUBBLING_OOZE (16) | `(22,13)` | ATTACKER1 | FOLLOW_TARGET\|0 (`$00`) | 0 |
| 8 /136 | BUBBLING_OOZE (16) | `(20,14)` | ATTACKER2 | NONE (`$FF`) | 0 |
| 9 /137 | RAT (15) | `(10,11)` | ATTACKER1 | NONE (`$FF`) | 15 |
| 10 /138 | BUBBLING_OOZE (16) | `(9,10)` | ATTACKER3 | FOLLOW_ENEMY\|9 (`$89`) | 15 |
| 11 /139 | BUBBLING_OOZE (16) | `(5,12)` | ATTACKER1 | NONE (`$FF`) | 15 |

InitializeAllEnemiesBattlePositions scans combatants 128–159. Rows 0–11 have
initialization word `$0000`; its low nibble STARTING=0 is below HIDDEN=2, so
InitializeEnemyBattlePosition calls InitializeEnemyStats. Slots 140–159 take
the absent path: activation 0, maximum/current HP=0 and X=-1. That path still
reads trigger bytes through the subsection pointer; it does not zero every
field or establish a valid extra row. The Battle32/Jaro exception does not apply.

InitializeEnemyStats resolves type, copies its 56-byte definition (14 longwords),
sets current HP/MP from maxima and X/Y from the row, packs movetype into the
upper nibble and commandset into the lower, then writes enemy type, trigger
regions, move orders and inventory effects. Actual setters in
`code/common/stats/combatantstats_2.asm` pack triggers as primary<<4 | secondary:
rows 3–8 receive `$0F`, all others `$FF`. Orders are primary<<8 | secondary:
row 0 `$40FF`, rows 1/2 `$80FF`, rows 3–7 `$00FF`, rows 8/9/11 `$FFFF`, row 10 `$89FF`.

The activation write retains only the copied definition's `$F000`, rotates
the initialization word by eight, masks it with `$0FFF`, and ORs the two.
WILLARD's PRIORITYMOD_0=0 produces `$0000`; RAT/OOZE's
PRIORITYMOD_2=`$2000` with BIT5=`$20` produces **`$2000`**, discarding BIT5.
All have primary/secondary-active and neutral bits clear and initialization
mode 0; no RESPAWN=1/HIDDEN=2 bits are introduced. This is the write before
ActivateEnemies, not a claim that region 15 or any enemy is already activated.

`battleloop/upgradeenemies.asm:UpgradeRandomBattleEnemies` restores the original
type when IsBattleUpgradable is false. The complete eleven-entry
`data/battles/global/randombattles.asm:list_RandomBattles` excludes
VERSUS_WILLARD (23). F523 set therefore does not upgrade these types. Do not
import another battle's random-upgrade behavior.

### Definitions, items, spells and difficulty

**Confirmed:** complete selected definitions in `data/stats/enemies/enemydefs.asm`
are entries 90,15,16 (source lines 2288–2311,413–436,438–461). They have
initialStatus NONE and four NOTHING item slots: there is no equipped weapon.
All row item words also equal the exact sentinel, so InitializeEnemyItems
returns before AddItem. NOTHING leaves the copied inventory intact; it is not
an inventory-erasure command. The accepted Battle22 non-sentinel/AddItem-mask
correction remains applicable to the shared decoder but is not executed here.

| Definition | HP/MP maxima and fresh current | Base ATT / DEF / raw AGI / MOV | Movetype | Four spell slots |
| --- | --- | --- | --- | --- |
| WILLARD (90) | 50/0 | 50 /36 /165 /6 | STEALTH=3 | NOTHING, NOTHING, NOTHING, NOTHING |
| RAT (15) | 41/0 | 45 /32 /30 /6 | STEALTH=3 | NOTHING, NOTHING, NOTHING, NOTHING |
| BUBBLING_OOZE (16) | 32/0 | 38 /35 /26 /4 | REGULAR=1 | AQUA\|LV2 (`$68`), NOTHING, NOTHING, NOTHING |

SPELL_AQUA=`$28`, LV2=`$40`, NOTHING=`$3F`. The Ooze's declared spell exists
despite zero MP; this boundary does not evaluate its future AI use or cost.
Willard's raw AGI=165 is copied data, not a claim of displayed agility or a
turn-count result. Definitions also supply their level, resistance, prowess
and spell-power fields; their later action/damage consumers are outside scope.

AdjustEnemyBaseAttForDifficulty then UpdateCombatantStats follow the activation
write. The first difficulty comparison has consecutive `beq` branches: SUPER
branches to Continue, while a non-SUPER value falls through the second untaken
`beq` into the same Continue. Thus **all difficulties** get floor(baseATT*5/4);
SUPER=2 gets that operation a second time. Willard/Rat/Ooze become 62/56/47
on NORMAL=0, HARD=1 or OUCH=3, and 77/70/58 on SUPER=2. These are initialized base
ATT values, not damage. UpdateCombatantStats initializes current stats from
base fields and applies status/equipment; these selected definitions have
no initial status or equipped-item effect. It does not turn copied HP/MP,
spells or AI labels into a performed action.

### Region 0 and point 0 are inputs, not fired triggers

**Confirmed:** the entire twelve-byte region record is
`4,0,9,0,9,15,26,15,26,0,0,0`: type 4, second byte 0, vertices
`(9,0),(9,15),(26,15),(26,0)`, trailing0/0. The two-byte point 0 is `(22,3)`.
These are battle-local coordinates. `sf2enums.asm` names type offset 0 and
packed X/Y offsets 2/4/6/8. The selected consumer
`battleloop/triggerregions.asm:IsRegionTriggered` reads those fields; the
non-type 3 arm checks triangles vertices 1/2/4 and 3/2/4. The second/trailing
bytes are not read by that geometry path; no extra semantic name is assigned.

Fresh entry clears F90 before loading; later
TriggerRegionsAndActivateEnemies maps region index 0 to base 90+0 and writes
primary/secondary activation through SetActivationBitfield. This is distinct
from the initializer's packed trigger-region write through SetTriggerRegions.
AI_TRIGGER_REGION_NONE=15 is a sentinel, not fifteen polygons. The source
consumer relation explains the stored 0/15 values without running it or claiming
that crossing/activation occurred. NEWLY_TRIGGERED_BATTLE_REGIONS is separate
state; the fresh F90–105 clear does not by itself assert that every AI scratch
field is reset.

`battlefield/getmoveorderposition.asm:GetMoveOrderPosition` tests MOVE_TO bit 6,
uses the low nibble as a point index, doubles it and reads X/Y; `$40` therefore
selects point 0. Its other arm reads a combatant's position. FOLLOW_ENEMY|9
encodes combatant 137; FOLLOW_TARGET|0 retains target operand 0. These declarations
do not prove any chosen route, reached point, target, attack or tactical decision.
The fresh stop precedes the calls that would activate and schedule them.

## Battle load, entities and coordinate domains

**Confirmed:** `code/gameflow/battle/battlefunctions/loadBattle.asm:LoadBattle`
starts with CURRENT_MAP in d1, fades out, disables view tracking, loads tilesets,
waits for fading, clears VInts, waits one VInt and calls
`code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`.
ClearEntities clears 49 physical records and 64 lookup bytes; PositionBattleEntities
then scans all 30 ally and 32 enemy slots. Nonnegative X/Y admits an actor, with
neutral-marked enemies additionally excluded. There is **no extra HP check**
in this placement pass. Fresh initialization supplies eligible positions;
saved state may not have that relationship.

Ordinary declarations use `(X & $3F)*384`, `(Y & $3F)*384`, Down=3 and eas_Standing.
GetCombatantMapsprite uses GetAllyMapsprite's current class-sensitive mapping
for allies (with its Rohde membership exception), or the stored enemy type
through `data/stats/enemies/enemymapsprites.asm`. Battle mode skips the
exploration-only dead-ally BLUE_FLAME branch. Enemy map sprites are
WILLARD=154, RAT=79 and BUBBLING_OOZE=80, all below special threshold 240.
With N declared allies, fresh enemy row r is physical N+r. If N=12, the twelve
enemies occupy physical 12–23. Skipped ally/enemy lookups are -1.

This rebuild replaces exploration 128–130 and follower/raft mappings rather
than continuing them: fresh combatant 128 is Willard,129/130 are Rats, whereas
exploration 128 was Rat and 129 Willard. Enemy type 90, sprite 154, logical 128 and
physical N are distinct. `battlefunctions_0.asm:GetEntityIndexForCombatant`
subtracts 96 from negative-byte enemy IDs and masks to a byte;128–139 use
lookup 32–43. Generic script A30/B31 and C158/D159 aliases are not newly
deployed characters. Clear/rebuild leaves lookup 30/31 at 0 and absent enemy
lookups 62/63 at -1 here; inherited D/raft63 alias semantics do not preserve the
exploration raft actor or make an invalid lookup safe.

The complete `data/battles/global/battleneutralentities.asm` has eleven groups
and **no VERSUS_WILLARD (23) entry**. PositionBattleEntities checks completed
F523 after the ordinary scans: set skips neutral scanning; clear scans to the
terminal word without a match. Thus neither value adds a fresh neutral or
repurposes combatants 158/159. Do not import Chessboard's Ponpei/Maid, the
exploration residents, or their physical/script mappings. On supplied resume,
a previously neutral-marked enemy remains excluded by the ordinary pass;
absence of a matching neutral group is not a cleanup/repair of its combatant
state. Other supplied nonneutral, nonnegative slots can still declare actors.

### Full-load crop and view selection

LoadBattle restores its supplied combatant d0 (fresh 0), resolves the rebuilt
lookup, sets VIEW_TARGET_ENTITY and reads tile coordinates into
BATTLE_ENTITY_CHOSEN_X/Y. A negative lookup falls back to physical 0 for the
coordinate reads, without placing Bowie or repairing membership. After sprite
initialization it sets **d0=-1** before LoadMap: this chooses target-based view
placement. **d1** selects full load versus reload; the preserved nonnegative
current Map27 selects the full-load arm. This is not the earlier negative-d1
`reloadMap` path that reuses the working layout.

`code/common/maps/mapload.asm:LoadMapBlocksAndLayout` loads the base layout,
applies flag copies/chest overlays, then uses CURRENT_BATTLE!=255 to crop
BATTLE_AREA_X/Y and WIDTH/HEIGHT into working origin 0/0. For the accepted row,
`code/gameflow/exploration/exploration.asm:CopyMapBlocks` copies `(23,14)`,
27x16 using 64-wide word rows. LoadMapArea's battle arm sets layer1 start 0/0
and end `(26,15)*384`; other layer values remain map-area operands.
This geometry does not establish terrain legality, renderer output or a route.

Formation/enemy/region/point coordinates are already battle-local; initializer
helpers do not subtract 23/14. Source-layout correspondence is
`(localX+23,localY+14)`: placement 0 `(4,5)` corresponds to Map27 `(27,19)`,
Willard `(18,7)` to `(41,21)`, and point 0 `(22,3)` to `(45,17)`.
These are crop relations, not observed moves from the incoming warp `(0,0)`.
LoadBattle does not rerun CheckBattle; changed supplied area bytes change the crop.

It then loads entity mapsprites, installs base VInts, loads battle terrain,
submits map music and fades in. Battle23 does not take the Battle44 timer-window
branch. Returning permits the start wrapper and then the fresh stopping boundary.
Required map/sprite/display/fade/VInt services must return; call order alone
proves neither exact visible timing nor usable control.

## Suspended/current-state entry and changed premises

**Confirmed:** after PLAYER_TYPE clear/F399, F88 set restores
SAVED_SECONDS_COUNTER, clears F88, clears AI memory, clears VIEW_TARGET_ENTITY,
calls LoadBattle and branches directly to `@IndividualTurns_Loop`.
The following branch to `@Start` is unreachable. Both fresh and suspended
ClearAiMemory calls reset 48 last-target bytes to `$FF` and 48 memory bytes to 0.
This does not create a fresh turn order or prove an AI decision.

The suspended branch does not assign the incoming map/battle, clear region
flags, heal, rebuild the force, initialize enemies or execute either hook.
It also does not explicitly clear d0 before LoadBattle. Current map/battle,
area, HP/MP/status/items, positions/activation and turn/order are supplied state,
not guaranteed to equal spriteset23. LoadBattle rebuilds physical entities and
reloads actual current map/terrain; its view argument is distinct from that
map selection. A changed CURRENT_BATTLE can select a different battle's data
and neutral population despite incoming 27/23. This document stops before the
individual-turn loop reads the saved order, including its sentinel branch;
it establishes no turn executed, automatic repair or cross-process persistence.

| Changed premise | Consequence within the named boundary |
| --- | --- |
| F423 clear on Map27 | No row 23 admission, regardless of F473/F523/F781/F88 |
| F423 set, F88 clear, F473 clear | Empty before; fresh region/heal/roster/load; start sets F473 before its empty dispatch |
| F473 already set | Skip both hooks; fresh initialization/load still runs; no past played intro proven |
| F523 set with F423 set at selection | Clear F423 but admit this call; no random upgrade, no neutrals; F473 independent |
| F781 changed independently | Earlier exploration scene gate changes; neither battle wrapper replays or checks it |
| Changed membership/stale list order | Ascending joined/active list rebuilt; at most twelve processed members, not fixed heroes |
| Dead non-immortal/fewer/surplus members | Dead members consume capacity without positions; fewer living placements; surplus active bits cleared |
| Changed ally class/items/status | Supplied class/inventory remain inputs; eligible HP/MP healed, status masked then stats/equipment rebuilt |
| Different prior current map/battle, F88 clear | Incoming 27/23 assigned before and after before wrapper; no inherited exploration roster |
| F88 set with altered HP/flags/positions/turn/area | Fresh setup bypassed; actual current state drives load and mapping, with no fresh-health guarantee |
| F88 set with missing/neutral/extra valid-position slots | Entity pass uses position/neutral bits rather than HP or the twelve-row table; no universal fresh actor count |
| Incomplete before/load/start service | Completed prefix effects remain; F473 can already be set during unfinished start; control/presentation unproved |

## Reproduction, retained results and limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands
and verify the clean pinned source. Reuse the existing
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init equality, `_verify_map_script_engine_fixture` and `_cutscene_facts`;
no payload regeneration. Battle23's selected before/start corpus is exactly
the directly read empty terminator, not the preceding exploration scene.

| Existing evidence / direct join | What it establishes and does not establish |
| --- | --- |
| `battle-cutscenes-static-v1.json` and complete before/start tables/wrappers | Shared facts plus selected slot 23 targets, flag polarity/write order and returns |
| `battle-routing-data-static-v1.json` and retained routing inventory | Aggregate topology/source identities; routeTargetsParsed=false does not own individual slot semantics |
| `battle-spriteset-data-static-v1.json` and retained spriteset inventory | Pointer/H1 address/header aggregate provenance; rowContentTracked=false requires all 24 row bodies, macros/constants, region and point to be read directly |
| `battle-loop-static-v1.json`, `battle-control-static-v1.json` | Existing `_build_lifecycle_facts` / `_build_control_facts` match pinned source; direct complete entry/initialization/heal/AI-clear bodies own the selected semantics |
| battleparty, three complete enemy definitions, enemy mapsprites, packed setters and stat/difficulty consumers | Actual membership/capacity, identities, sentinel, current-stat and activation writes; no symbolic AI execution |
| LoadBattle, PositionBattleEntities, GetCombatantMapsprite/GetAllyMapsprite, GetEntityIndexForCombatant and map-load/copy/area helpers | Full-load d1 versus view d0, rebuilt mapping, crop and supplied-state distinctions |
| Complete random/neutral tables and selected jump interfaces | Positive exclusion for 23; `j_` aliases resolve through s02/s07/s13, including GetCombatantEntryAddress_0 -> GetCombatantEntryAddress |

For direct reproduction enumerate every ordered triple of combatant macros
in spriteset23, decode using the named macros/constants, and compare the twelve
formation and twelve enemy rows above, all header/tail bytes and subsection
sizes. Read complete named initializer bodies and selected immediate consumers,
not just inventory totals or upstream comments. No fixture/schema/parser is
added, and aggregate equality does not guard every new prose claim.

Whole-document/diff, source/quantity, link/provenance/private-boundary checks
and `git diff --check` apply. Run the configured clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`; record the plan and
actual final-head CI on Issue932. No local blanket normal/full/engine/Godot/
H3/H4 suite is selected solely for prose.

Preserve [#928 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/928#issuecomment-6176708249)
and [PR931 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/931#issuecomment-6176742983):
head `dc6f02d83778bfe71a40d48a1e5b908819812496`, tree
`298e4be4b35fad2e8ccbd9a9b405b3e069a717ad`, base
`b9405e3d2649af6aee3ace4397ecbfa6faa9121e`, successful CI38624813497,
unchanged pair 0.080s, inspection truncation/path/shape/private-regex/PowerShell
failures and the corrected C158/D159/raft63 alias hypothesis. Keep the accepted
low-byte coordinate, alias, wait, fresh-order, item-mask and neutral-lookup
corrections. Linked [#924 original](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/924#issuecomment-6176361906),
[correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/924#issuecomment-6176448210),
[P2 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/927#issuecomment-6176402766)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/927#issuecomment-6176460244)
retain distinct original/corrected objects, CI38622136093/38622845061 and
all failures; #920 and earlier linked results also remain unchanged.

**Unknown:** natural arrival/admission; supplied flags, HP/class/items, force,
mode/aliases, current/saved area/map/battle, positions and turn state; completed
load/script/fade/VInt services, actual visible/audible roster and presentation,
usable control and persistence. No tactical/AI execution, outcome, postbattle
population or Creed route is established. These limits are not an automatic
runtime queue under [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md).
