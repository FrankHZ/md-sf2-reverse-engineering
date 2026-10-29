# Battle26 initialization and initial roster

Evidence date: 2026-10-14. **Confirmed:** after the before wrapper returns,
fresh Battle26 entry resets regions, heals eligible allies, initializes the
actual party and fifteen enemy rows, replaces scene entities, and loads the
battle before its empty start hook. With completion526 clear, loading also
creates a neutral Higins actor. That actor is neither the earlier scene actor
nor proof that ally19 joined. Stop immediately before the first ActivateEnemies.
**Inferred:** the declared formation stages the encounter outside Ketto.
**Unknown:** natural reach, supplied state, completed services, presentation,
control and persistence; no activation, turn, outcome or recruitment is observed.

## Agreement and provenance

Standard rigor for [Issue #1005](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1005)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Scene population, force membership, enemy initialization and neutral loading
have distinct lifecycles. Accepted starting base is
`a8f4cf764bce9da9bc253fd3c516b3e1c3bd50ac`, including #1001/PR1004.
This document is the sole tracked owner; no shared index, registry, contract,
fixture, schema, tooling, design, remake, glossary or Chinese changes.
Parallel #1006 and UI692 are independent and supply no evidence.

Resume at [Battle26 before-wrapper return](./battle26-admission-and-geshp-confrontation.md#before-dispatcher-and-exact-endpoint).
Reuse [Battle23 roster consumers](../southern-travel-creed/battle23-initialization-and-roster.md),
[Battle22 item and neutral lookup corrections](../southern-travel-creed/battle22-initialization-and-roster.md),
[Battle25 initialization](./north-cliff-entry-through-battle25-admission.md),
[battle loop](../../battle/battle-loop.md), [cutscenes](../../battle/battle-cutscenes.md),
[spriteset inventory](../../battle/battle-spriteset-data.md),
[global battle data](../../battle/battle-global-data.md) and [common maps](../../world/common-maps.md).
The endpoint is BattleLoop's `@Start`, before its `bsr.w ActivateEnemies`.
F88-supplied loading is a contrasting branch, not another route through that
fresh endpoint. No activation execution, turn simulation, outcome, afterbattle
dialogue, Higins recruitment, later exploration, runtime, acquisition or
extraction regeneration is included.

Source paths below are relative to `disasm/` at
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained read-only checkout is clean at that pin. Existing evidence binds
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Constants, RAM and encodings belong to `sf2enums.asm`, `sf2const.asm`,
`sf2macros.asm` and `sf2mapmacros.asm`. Fnnn means flag nnn; indices are
zero-based and extents half-open. Coordinates and stats are source operands
or conditional initializer results, not observed gameplay. Short `battleloop/`,
`battlefunctions/`, `battlefield/` and `cutscenes/` paths are under
`code/gameflow/battle/`; `entity/` is under `code/common/scripting/` and
`exploration/` under `code/gameflow/`.

Minimum evidence is named source, existing public fixtures and retained private
payloads. Reuse the 18,285,979-byte map-init/map-script pair and small selected
battle records. Linear comparison budget is under one minute/512 MiB, plus
small prose and receipts; the unchanged pair comparison took 0.083s. No new
maintained machinery or collection is needed. Replan contradictory evidence
or material expansion. Unknowns do not authorize a runtime queue.

## Return from before and ordered fresh initialization

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) has already cleared PLAYER_TYPE, set F399 and taken the
F88-clear branch. It cleared SECONDS_COUNTER, saved incoming map/battle words,
assigned CURRENT_MAP72/CURRENT_BATTLE26, installed base VInts and invoked
ExecuteBeforeBattleCutscene. The accepted predecessor owns that scene or its
F476-held skip; neither path sets F476 in the before wrapper.

After return, the exact ordered suffix is:

1. Restore saved d0/d1 words and reassign their low bytes to current map/battle.
2. Clear every region flag90–105, including the three declared regions' F90–92.
3. HealLivingAndImmortalAllies; InitializeAllAlliesBattlePositions;
   InitializeAllEnemiesBattlePositions; ClearAiMemory.
4. Clear d0.w to zero and call LoadBattle for combatant0.
5. ExecuteBattleStartCutscene and reach `@Start`; stop before ActivateEnemies.

F426 is the admission unlock and F526 the completion flag. Neither is the
intro gate F476. The predecessor's ordered Map72 selector can admit26 only
after earlier408/424 candidates fail; a selected held526 clears426 but still
admits that call. Initialization does not repeat this selection or repair
supplied battle-area bytes. Reassigning map/battle after before does not
reassign the crop. F476 held skips the two hooks but still runs this fresh
suffix. Every suffix step depends on earlier services returning; completed
prefix writes are not rolled back on interruption.

### Actual allies, healing and formation

**Confirmed:** `battleloop/heallivingandimmortalallies.asm` iterates all thirty
allies, independently of membership. Living allies and always Peter7/Lemon28
receive maximum HP/MP as current values. Other dead allies are skipped.
It masks status to STUN|POISON|CURSE (`$0007`) and calls
`code/common/stats/updatecombatantstats.asm:UpdateCombatantStats`.
That rebuild initializes current stats, applies status/equipment and recomputes
curse; the intermediate mask is not a promise of final curse retention.
It does not join, activate, promote or replace inventory. HP fields/setters
are words; MP and X/Y are bytes, even where call operands use d1.w.

`code/gameflow/battle/battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions`
sets X=-1 for all thirty slots, then calls UpdateForce. The actual
`code/common/stats/battleparty.asm` rebuilds joined/active/reserve lists in
ascending ally-ID order from flags. A stale supplied list is not authoritative.
Only each living active member's next row X/Y is copied; the row index is a
placement ordinal, and its AI/item fields do not overwrite that ally's state.

The complete `BattleSpriteset26` header is **12 allies, 15 enemies, 3 regions,
1 point**. All twelve ally rows encode HEALER1=0, NOTHING=`$007F`, primary
NONE=`$FF`/region15, secondary NONE=`$FF`/region15, filler0, STARTING=0.
The complete varying fields are:

| Placement ordinal | X/Y | Placement ordinal | X/Y |
| --- | --- | --- | --- |
|0|`(7,25)`|6|`(4,26)`|
|1|`(6,25)`|7|`(6,27)`|
|2|`(5,25)`|8|`(5,27)`|
|3|`(4,25)`|9|`(4,27)`|
|4|`(6,26)`|10|`(7,26)`|
|5|`(5,26)`|11|`(7,27)`|

A dead listed member consumes a processed-member slot but does not advance
the placement pointer. Later living members use earlier unconsumed positions.
Twelve limits processed members, not guaranteed living actors. Surplus active
members pass through LeaveBattleParty, clearing their active bit and X; that
helper does not rebuild the list/count immediately. Fewer eligible members
yield fewer actors. A zero-member count is decremented before the first read
without an empty-list guard, so arbitrary inconsistent empty state is not
established safe. Changed classes, equipment and HP affect healing, stats,
eligibility and sprite choice; no fixed named twelve-character party follows
from the placement table. Supplied joined/active Higins19 would be handled as
an ally independently of the neutral load below; the load itself never joins him.

## Complete spriteset and fifteen enemy rows

**Confirmed:** the 45-longword `data/battles/spritesets/entries.asm` table
selects BattleSpriteset26 at slot26, byte104. `spriteset26.asm` spans
`$1B5012..$1B5180`, **366 bytes**: header4, allies144, enemies180, regions36,
point2. `code/gameflow/battle/getbattlespritesetsubsection.asm` uses
CURRENT_BATTLE*4 and actual preceding header counts. Relative subsection
offsets are sizes0, allies4, enemies148, regions328 and points364.

`allyCombatant`/`enemyCombatant`, `combatantAiAndItem` and
`combatantBehavior` emit each twelve-byte row:

| Offset | Width | Source meaning |
| --- | --- | --- |
|0|byte|placement ordinal or enemy type|
|1,2|byte each|starting X,Y|
|3|byte|AI commandset|
|4|word|item bitfield|
|6,7|byte each|primary order and trigger region|
|8,9|byte each|secondary order and trigger region|
|10,11|byte each|filler and spawn value, read together as initialization word|

All enemy rows have filler0 and STARTING=0, hence initialization word `$0000`.
All items are NOTHING except row2's BLACK_RING|EQUIPPED|USABLE_BY_AI|
UNUSED_ITEM_DROP=`$6098`. Commandsets HEALER3/HEALER4/ATTACKER1/ATTACKER3/LEADER
are 2/3/6/9/14. Orders encode FOLLOW_TARGET=0, MOVE_TO=`$40`,
FOLLOW_ENEMY=`$80`, NONE=`$FF`, ORed with their numeric operand.

| Row / combatant | Type (ID) | X/Y | Commandset | Primary order / region | Secondary order / region |
| --- | --- | --- | --- | --- | --- |
|0 /128|LIZARDMAN19|`(22,7)`|LEADER|MOVE_TO\|0 /15|NONE /15|
|1 /129|WIZARD72|`(21,8)`|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|
|2 /130|DARK_BISHOP81|`(24,7)`|HEALER3|FOLLOW_ENEMY\|0 /15|NONE /15|
|3 /131|LESSER_DEMON45|`(17,3)`|HEALER3|FOLLOW_ENEMY\|0 /1|NONE /1|
|4 /132|DARK_SOLDIER18|`(22,9)`|ATTACKER1|NONE /1|NONE /15|
|5 /133|DARK_SNIPER62|`(23,11)`|ATTACKER1|NONE /1|NONE /15|
|6 /134|DARK_SNIPER62|`(15,12)`|ATTACKER3|FOLLOW_ENEMY\|7 /2|NONE /2|
|7 /135|DARK_SOLDIER18|`(17,13)`|ATTACKER1|NONE /2|NONE /15|
|8 /136|LESSER_DEMON45|`(4,6)`|ATTACKER1|NONE /2|NONE /15|
|9 /137|DARK_SOLDIER18|`(24,17)`|ATTACKER3|FOLLOW_ENEMY\|10 /0|NONE /0|
|10 /138|DARK_SOLDIER18|`(23,18)`|ATTACKER1|NONE /0|NONE /15|
|11 /139|LESSER_DEMON45|`(24,27)`|ATTACKER1|NONE /0|NONE /15|
|12 /140|LESSER_DEMON45|`(9,19)`|ATTACKER1|NONE /15|NONE /15|
|13 /141|DARK_BISHOP81|`(13,20)`|HEALER4|FOLLOW_ENEMY\|14 /15|FOLLOW_ENEMY\|12 /15|
|14 /142|DARK_SNIPER62|`(14,23)`|ATTACKER1|NONE /15|NONE /15|

InitializeAllEnemiesBattlePositions scans128–159. Rows0–14 have spawn low
nibble0 below HIDDEN2 and call InitializeEnemyStats. Absent143–159 receive
activation0, max/current HP0 and X=-1, not a full combatant clear. The absent
branch also reads trigger bytes from the unchanged enemy-subsection pointer;
here row0 supplies15/15. It does not establish extra rows or clear all Y/MP/
inventory fields. The Battle32/Jaro exception does not apply. Neutral loading
later repurposes159 when526 is clear.

InitializeEnemyStats resolves the type (including the upgrade decision below),
copies its56-byte definition as14 longwords, sets current HP/MP from maxima,
writes row X/Y, packs movetype into the high nibble and commandset into the
low nibble, stores enemy type, triggers, move orders and item effects.
`code/common/stats/combatantstats_2.asm:SetTriggerRegions` stores primary<<4
OR secondary's low nibble as one byte; SetMoveOrders stores primary<<8 OR
secondary as a word. In row order the exact packed results are:

| Rows | Trigger bytes | Move-order words |
| --- | --- | --- |
|0–4|`FF FF FF 11 1F`|`40FF 80FF 80FF 80FF FFFF`|
|5–9|`1F 22 2F 2F 00`|`FFFF 87FF FFFF FFFF 8AFF`|
|10–14|`0F 0F FF FF FF`|`FFFF FFFF FFFF 8E8C FFFF`|

After items, the activation write keeps only the definition's `$F000`, rotates
the initialization word by8, masks it with `$0FFF` and ORs the two. With the
unupgraded definitions this yields `$3000` for DARK_BISHOP rows2/13 and `$2000`
for every other row. Definition BIT4/BIT5 are discarded; primary-active,
secondary-active, neutral and spawn-mode bits are clear. This is initialized
activation storage, not proof of an activated enemy. Difficulty adjustment
and UpdateCombatantStats then run.

### Selected definitions, equipment and initialized stats

**Confirmed:** `data/stats/enemies/enemydefs.asm` entries18,19,45,62,72,81
all have ENHANCED spell power and initialStatus NONE. Each has four item and
four spell slots. The table gives the first item; all remaining definition
item slots are NOTHING. The listed spells fill slots from zero, with every
unlisted slot NOTHING. These are the **F526-clear, unupgraded** values.

| Type | Level | HP/MP maxima and fresh current | Base ATT/DEF/AGI/MOV | Movetype | First item | Spells |
| --- | --- | --- | --- | --- | --- | --- |
|DARK_SOLDIER18|23|35/17|31/46/34/4|REGULAR1|LARGE_AXE\|EQUIPPED|ATTACK|
|LIZARDMAN19|24|40/0|34/45/38/6|REGULAR1|LARGE_AXE\|EQUIPPED|none|
|LESSER_DEMON45|22|40/21|49/37/32/6|FLYING5|NOTHING|BLAZE\|LV3, BOOST\|LV2|
|DARK_SNIPER62|24|38/0|11/37/35/5|STEALTH_ARCHER10|HYPER_CANNON\|EQUIPPED|none|
|WIZARD72|26|37/37|32/40/44/5|MAGE11|FLAIL\|EQUIPPED|BLAZE\|LV3|
|DARK_BISHOP81|23|39/38|28/38/27/5|HEALER12|FLAIL\|EQUIPPED|AURA, BLAST\|LV3|

Lizardman resistance is ICE_WEAKNESS, Lesser Demon STATUS_MINOR, Wizard
FIRE_MINOR; the other three are0. Prowess is CRITICAL150_1IN32|
DOUBLE_1IN32|COUNTER_1IN16 for Soldier/Demon, with Lizardman critical1in8,
Sniper double1in16, and Wizard/Bishop counter1in32 instead. Definitions supply
PRIORITYMOD_2 except Bishop's PRIORITYMOD_3; Demon also has BIT4|BIT5,
Sniper/Wizard BIT4. These are copied fields, not evaluated damage or tactics.

`AdjustEnemyBaseAttForDifficulty` has two consecutive `beq` instructions after
the first SUPER comparison. SUPER takes Continue, and all other values fall
through the second untaken branch into Continue too. All difficulties therefore
apply floor(baseATT*5/4); SUPER2 applies it twice. Then current stats copy base
values and equipped effects run. `data/stats/items/itemdefs.asm` gives
LARGE_AXE38 +25 ATT, HYPER_CANNON53 +40 and FLAIL90 +19. These definitions have
no initial status effect altering DEF/AGI/MOV; their current values equal the
table. Exact ATT results are:

| Type | Adjusted base ATT, NORMAL/HARD/OUCH | Current ATT with copied equipment | SUPER base / current ATT |
| --- | --- | --- | --- |
|Soldier|38|63|47 /72|
|Lizardman|42|67|52 /77|
|Demon|61|61|76 /76|
|Sniper|13|53|16 /56|
|Wizard|40|59|50 /69|
|Bishop, both rows|35|54|43 /62|

For row2, the copied Flail is slot0 (`$00DA`); slot1 is empty.
`InitializeEnemyItems` calls `code/common/stats/itemstats.asm:AddItem` because
`$6098` differs from NOTHING. AddItem masks the successful value **and d1**
with `$807F`, inserting `$0018` (Black Ring24) in slot1. Back in the caller,
the equipped-bit test now fails, so EquipItemBySlot is not called. Final items
are `$00DA,$0018,$007F,$007F`; the ring is unequipped and has neither encoded
AI-use nor drop bit. Its declared +10 ATT/useSpell BLAZE2 does not contribute
to the initialized ATT above. No later use, drop or reward is established.
For all other row items, the exact NOTHING sentinel returns without AddItem,
leaving copied equipment intact. A changed full definition inventory would
take AddItem's failure path, which the caller does not check; do not generalize
the successful-insertion result to arbitrary modified definitions.

### Completion-held upgrade alternative

**Confirmed:** unlike Battle23/25, OUTSIDE_KETTO26 is in the complete eleven-entry
`data/battles/global/randombattles.asm` list. In
`battleloop/upgradeenemies.asm`, IsBattleUpgradable succeeds; DetermineBattleUpgrade
tests526. Clear returns each declared type unchanged. Held allows
UpgradeRandomBattleEnemies before the definition copy, independently of476.
Thus the fixed definition/current-stat table above is not an unconditional
repeat-battle roster.

The selected categories are melee for Soldier/Lizardman, airborne for Demon,
ranged for Sniper, mage for Wizard and healer for Bishop. The shared helper
uses Bowie0's level, adds20 for class>11, subtracts battle26, and requires a
positive difference of at least10 before upgrade range/RNG selection. Smaller
differences return the original type. Range parameters are5/2/2/1/2; selected
bounds/exclusions and retry/fallback branches then govern the returned type.
The airborne exclusion list is empty; its consumer decrements the zero count
and enters the scan without a zero-length guard. No safe guaranteed upgraded
Demon result is inferred from the category name or high level. This existing
shared boundary remains with the global-data owner's upgrade Unknowns.

The returned definition supplies stats, inventory, movetype, sprite identity
and priority; row coordinates, commands, regions, orders and item operand
remain the selected Battle26 data. Difficulty still runs after that copy.
F526 also independently suppresses neutral loading. Supplied level/class/RNG
and shared-service completion prevent a fixed upgraded cast claim; this slice
does not simulate or audit the general upgrade/combat system.

## Three regions and one point

**Confirmed:** all records are twelve bytes: type, second byte0, four packed
X/Y pairs, trailing0/0. The complete region values are:

| Index / flag | Type | Vertices1,2,3,4 | Remaining bytes |
| --- | --- | --- | --- |
|0 /90|4|`(21,0),(12,31),(35,31),(35,0)`|second0; trailing0,0|
|1 /91|3|`(4,0),(34,31),(34,0),(0,0)`|second0; trailing0,0|
|2 /92|4|`(0,0),(0,21),(35,21),(35,0)`|second0; trailing0,0|

Point0 is the complete two-byte `(23,7)` record. All coordinates are battle-local
tile units. `battleloop/triggerregions.asm:IsRegionTriggered` reads type at0
and packed vertex words at2/4/6/8. Type3 uses vertices1/2/3; the other arm uses
triangles1/2/4 and3/2/4. Region1's fourth pair and the second/trailing bytes
are not geometry operands on that selected path. Fresh reset clears90–105;
later TriggerRegionsAndActivateEnemies maps indices to90+index and separately
writes activation bits. Region15 is the no-trigger sentinel, not a fifteenth
polygon. Declared polygons and packed triggers are not already-fired events.

`battlefield/getmoveorderposition.asm:GetMoveOrderPosition` tests MOVE_TO bit6;
the low nibble times2 selects a point. Row0's `$40` references point0. Its
other arm reads the referenced combatant's X/Y. FOLLOW_ENEMY operands0/7/10/
14/12 encode combatants128/135/138/142/140. These are configured references,
not chosen targets or performed movement. No first activation/turn is consumed.

## Battle load and replacement of physical entities

**Confirmed:** `battlefunctions/loadBattle.asm:LoadBattle` preserves its d0
argument, takes CURRENT_MAP72 in d1, requests fade-out, disables view tracking,
loads map tilesets, waits for fade completion, clears VInts, waits one VInt
and calls `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`.
ClearEntities resets49 physical records (32 bytes each, X/Y and destinations
`$7000`, action/travel state cleared) and64 lookup bytes, then resets walking
allocation state. It does not preserve the earlier ce_4BC5A scene actors,
their actions, follower aliases or cinematic role numbers.

The ordinary scans cover all30 ally and32 enemy slots. Nonnegative signed-byte
X/Y permits declaration; enemy neutral bit `$0008` excludes that slot. There
is no additional HP predicate here. Fresh initialization creates the relevant
HP/position relation; supplied F88 state need not. Coordinates become
`(X & $3F)*384,(Y & $3F)*384`, with Down3 and eas_Standing. Flying/hovering
movetype selects aerial layer1, otherwise0. DeclareNewEntity writes position,
destination, facing, physical entity number, layer, sprite and action pointer,
clears velocity/travel/acceleration, installs flags `$7040` and initializes
animation/wait counters from the physical index. Installing a pointer is not
proof of its completed actions.

`entity/getcombatantmapsprite.asm` uses GetAllyMapsprite for supplied ally
class (including the Rohde membership exception); battle mode does not take
the exploration dead-ally BLUE_FLAME arm. For enemies it uses the stored type
through `data/stats/enemies/enemymapsprites.asm`. Unupgraded sprites are
Soldier82, Lizardman83, Demon109, Sniper126, Wizard136 and Bishop145, all below
special threshold240. They take sequential allocation. With N declared allies,
row r maps combatant128+r to physical N+r; with N=12, enemies occupy12–26.
Scene NPC128 was Geshp; fresh combatant128 is Lizardman. Those are different
lifecycles, not Geshp transformed or defeated.

Skipped ally/enemy lookup bytes are -1. `battlefunctions/battlefunctions_0.asm:GetEntityIndexForCombatant`
subtracts96 from negative-byte enemy IDs and uses the low byte as lookup index:
128–159 map to32–63. Actual storage is64 lookup bytes, with30/31 distinct from
enemy entries. ClearEntities's49 reset records and the fixed 30/32 scan bounds
are not a capacity check: DeclareNewEntity has no allocation-bound guard.
Ordinary fresh entry uses at most12+15+1=28 actors; arbitrary supplied resume
positions can exceed that ordinary bound, so no safe full62-slot population
is asserted. Missing lookups, changed types/special sprites or changed slots
must retain the actual branch/sentinel behavior.

### Neutral Higins: slot159, script B31, physical N+15

**Confirmed:** after the ordinary scans, PositionBattleEntities checks
500+CURRENT_BATTLE, hence526. Held skips neutral scanning. Clear searches the
complete `data/battles/global/battleneutralentities.asm` and selects its sole
OUTSIDE_KETTO row: `(18,14)`, Down3, HIGINS_PROMO48, eas_LyingLeft. The group
has battle word26, one eight-byte declaration and a terminating `$FFFF` word;
the whole table ends with a further terminator. No second Battle26 neutral
or earlier scene actor is selected.

The consumer starts descending combatant state at159. It writes max/current
HP0 and status0, ORs NEUTRAL `$0008` into activation, and writes X18/Y14.
Fresh absent-slot initialization makes activation exactly `$0008`. It does
not heal ally19, set joined/active flags, copy Higins's ally stats or establish
allegiance from his name/sprite. Other slot159 fields are not all rewritten.
It declares the next physical actor with neutral layer0 and the table's sprite,
facing and action; with the unupgraded fresh enemies that is N+15, or27 for
twelve placed allies.

The lookup write is literally `move.b d0,-(a1)` after resetting a1 to
ENTITY_INDEX_LIST_ENEMIES=`$FFB160`. It writes `$FFB15F`, offset31 from
ENTITY_INDEX_LIST=`$FFB140`: **script B31**, not enemy offset63. The ordinary
scan left combatant159's lookup63 at -1. GetEntityIndexForCombatant(159) does
not recover this neutral actor. A30 remains the clear0 byte; B31 is overwritten
only by the neutral declaration. Generic C158/D159 enemy aliases do not become
new allies. Missing/held526 cases do not preserve the earlier scene Higins19 actor.

`data/scripting/entity/eas_battleneutralentities.asm:eas_LyingLeft` installs
orientLeft, updateSprite, falls through eas_Motionless's motion OFF and branches
to eas_Idle. The initial declaration facing remains Down; the left orientation
command concerns sprite orientation, not a new initial coordinate/facing row.
LoadBattle later reinstalls VInts, so asynchronous action processing can progress
before the start wrapper; this static endpoint establishes installation and
script intent, not the precise reached action/frame or a rendered lying pose.

With F526 held, fresh159 remains absent (HP0/X=-1/activation0), B31 remains0,
and no neutral actor is declared. On F88 load, neutral-marked enemies are skipped
by the ordinary pass, then clear526 can rewrite159 again. A supplied nonneutral
159 with valid positions can first allocate an ordinary actor and later have
its state repurposed by this neutral pass; the routine does not retroactively
remove that declaration. No universal one-to-one mapping/repair is promised
for inconsistent supplied slots.

## Map72 crop, descriptor, terrain, music and view

**Confirmed:** the accepted `data/battles/global/battlemapcoords.asm` row26 is
`72,28,0,36,32,255,255`. The 36x32 crop at source-layout `(28,0)` is distinct
from the full map descriptor and from trigger wildcards. LoadBattle does not
rerun CheckBattle. The restored map/battle selection alone cannot repair
changed BATTLE_AREA_X/Y/WIDTH/HEIGHT.

After rebuilding entities, LoadBattle restores d0=0, looks up combatant0,
writes VIEW_TARGET_ENTITY and samples the physical actor's tile coordinates
into BATTLE_ENTITY_CHOSEN_X/Y. Negative lookup uses physical0 for coordinate
reads without creating Bowie or repairing membership. It initializes sprites,
sets **d0=-1** for target-based view placement and calls LoadMap with the
preserved nonnegative **d1=72**, selecting full load rather than negative-d1
reload. These two arguments have different purposes.

`data/maps/entries/map72/00-tilesets.asm` selects palette0 and tilesets
106/102/107/108/105. The sole30-byte `2-areas.asm` descriptor at
`$C1BD0..$C1BEE` has main start0/0, end63/63, foreground/background starts0/0,
both parallax pairs256/256, four autoscroll bytes0, layer type254 and music
MUSIC_MAIN_THEME1; `$FFFF` ends the list at `$C1BEE..$C1BF0`.
LoadMap's descriptor test uses the view target's internal coordinates against
bounds scaled384, without adding crop28/0; fallback returns the first descriptor.
There is only this one Map72 descriptor to select.

`code/common/maps/mapload.asm:LoadMapBlocksAndLayout` loads the base blocks/
layout, applies flag copies and opened-chest overlays, then copies the supplied
battle rectangle to working origin0/0. `exploration/exploration.asm:CopyMapBlocks`
uses64-wide word rows: here1152 words/2304 bytes copied, not a new36-wide
storage stride. LoadMapArea's battle arm sets layer1 origin0/0 and end
`(35*384,31*384)=(13440,11904)`. Remaining layer/parallax/autoscroll/type/music
fields still come from the descriptor. This is not terrain legality or
rendered-map evidence. Formation/enemy/region/point/neutral coordinates are
already local; no initializer subtracts28. Source correspondence is
`(localX+28,localY)`: placement0→`(35,25)`, Lizardman→`(50,7)`, point0→`(51,7)`,
neutral Higins→`(46,14)`. These are crop relations, not observed walking paths.

The load then waits a VInt, loads entity mapsprites, installs base VInts,
calls `battleloop/loadbattleterraindata.asm:LoadBattleTerrainData`, requests
map music and fade-in. Terrain lookup uses CURRENT_BATTLE*4 in the45-longword
`data/battles/terrainentries.asm` table, slot26 BattleTerrain26, whose incbin
is `data/battles/entries/battle26/terrain.bin`; LoadStackCompressedData writes
BATTLE_TERRAIN_ARRAY. This selects the existing private terrain; no tile corpus
or new terrain/movement comparison is published here.

`code/gameflow/battle/battlemusic.asm:PlayMapMusic` begins with descriptor
music1. Its battle substitutions concern NOTHING/TOWN/MITULA/MITULA_SHRINE/
CASTLE; MAIN_THEME matches none, so it submits1 through the sound command.
Do not infer a battle-theme substitution merely because this is battle26.
Battle44's timer-window branch does not apply. Requested fade/music/map/sprite
work and service order establish neither exact visible/audible timing nor
usable player control.

## Empty start hook, supplied continuation and changed premises

**Confirmed:** the complete48-entry relative-word table in
`data/battles/cutscenes/battlestartcutscenes.asm` selects slot26 at byte52,
word `$47B60`, relative to base `$47B2C`: target ms_Empty. Its complete
`code/common/scripting/map/ms_empty.asm` body is `$FFFF` at `$47A4E..$47A50`.
`cutscenes/battlestartcutscenesstart.asm` checks450+26=476, and if clear,
**sets476 before saving d0/a0 and dispatching ExecuteMapScript**. It restores
d0/a0, then the end wrapper at loc_47B8C restores d1 and returns. Held476 skips
flag mutation and dispatch, not the prior healing/roster/load.

Empty dispatch still enters the shared interpreter, initializes its work pointer,
saves registers and clears text skip. The terminator tail conditionally waits
for view scrolling if a dialogue window is present, clears VIEW_SCROLLING_SPEED,
restores registers and returns. There is no local text/movement/choice/join/load
command. A stalled shared tail can leave476 already set without wrapper return.
Only a completed return reaches this note's fresh pre-ActivateEnemies endpoint.

With88 held, BattleLoop clears PLAYER_TYPE/sets399, restores saved seconds,
clears88, clears AI memory, clears VIEW_TARGET_ENTITY and calls LoadBattle
without explicitly clearing d0. It branches to `@IndividualTurns_Loop`;
the subsequent branch to `@Start` is unreachable. It does not assign incoming
72/26, reset regions, heal/rebuild the force, initialize enemies or run either
hook. Both ClearAiMemory calls write48 last-target bytes `$FF` and48 memory
bytes0. Current map/battle/crop, HP/status/items, positions, activation and
turn state remain supplied inputs, subject to LoadBattle's entity rebuild and
neutral writes. This is not byte-for-byte preservation or fresh-state repair.
Stop this contrast before reading/executing the individual-turn loop.

| Changed premise | Required distinction |
| --- | --- |
|426 absent or earlier408/424 admitted|No unconditional Battle26 initialization from a Ketto exit.|
|476 clear versus held|Before/start dispatch differs; fresh reset/heal/roster/load still follows before return.|
|526 clear versus held|Fixed base definitions plus neutral versus upgrade eligibility and neutral suppression; independent of476.|
|Changed joined/active flags, dead/fewer/surplus members|Actual sorted list, processed-member capacity and HP checks govern positions; no fixed named cast.|
|Changed class/items/status or HP maxima|Healing/stat/sprite consumers use supplied state; no default inventory/class replacement.|
|Changed battle area or actor positions|Actual crop/view/mapping differs; load does not rerun admission or normalize all supplied fields.|
|Missing Bowie or missing lookup|Negative lookup falls back to physical0 for view reads; membership is not repaired.|
|Changed slot159/neutral bit, completion or F88|Ordinary scan and later neutral rewrite differ; combatant159 and script B31 are distinct lookups.|
|F88 set with altered current map/battle/turn|Incoming72/26 ignored by fresh assignment; no fresh endpoint, health or first-round guarantee.|
|Interruption during healing, load, fade, neutral declaration or start tail|Completed prefix writes remain;476 may be held before start returns; no transactional completion.|

The next coherent boundary starts at `BattleLoop:@Start`: ActivateEnemies,
ExecuteBattleRegionCutscene, spawning-target handling and GenerateBattleTurnOrder
up to the first individual-turn dispatch. Those are named future source owners,
not execution in this slice. No activated cast, first actor, battle result or
Higins join follows from the initial load.

## Reproduction and review boundary

Load ignored `local/private-inputs.ps1` in the same process as controlled
commands and verify clean pinned source. Reuse the existing
[retained-pair recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init equality, `_verify_map_script_engine_fixture` and `_cutscene_facts`,
preserving size/mtime and avoiding regeneration. The slice receipt uses
`uv run --locked python -X utf8 local/issue989-pair.py <configured-source-root>`.

| Evidence / direct join | Scope of proof |
| --- | --- |
|`battle-spriteset-data-static-v1.json`, retained `battle-spriteset-data-static.json`, complete spriteset26 and macros/constants|Pointer/H1 address/header/source-identity facts; `rowContentTracked=false` means all27 row bodies, regions and point require direct source review. Aggregate equality is not private row detail.|
|`battle-global-data-static-v1.json` and retained global payload|Public `factSummary` differs from private `facts`; the latter owns battle-map row26 and neutral/random-list membership, but not the neutral row's position/sprite/action. Read that complete row directly from source and join its consumer.|
|`battle-loop-static-v1.json`, `battle-control-static-v1.json`|Existing `_build_lifecycle_facts`/`_build_control_facts` compared with pinned source; read complete selected suffix/heal/force/enemy/AI-clear bodies for the new prose.|
|`battle-cutscenes-static-v1.json`, `battle-routing-data-static-v1.json` and actual tables/wrappers|Shared facts and source identity; `routeTargetsParsed=false` cannot prove selected slot26. Direct table/empty-body join and476 write order do.|
|Six definitions, item definitions/AddItem/stat setters, upgrade and neutral tables|Conditional type/stats/equipment/masks and membership distinction; no executed AI, drop or recruitment claim.|
|LoadBattle, PositionBattleEntities, lookup/sprite/action consumers, Map72 descriptor, crop/music/terrain consumers|Physical replacement, B31 versus159, scalar units and actual selected load; no rendered/service-completion claim.|

For direct reproduction enumerate all ordered macro triples, compare every
field with the tables above and resolve the366-byte extent/offsets through
actual macro widths. Check every selected definition and successful item
insertion path, packed writes, upgrade guards, neutral source row/consumer,
all start table entries and shared function aliases (s02/s07/s13 jump
interfaces). Review changed-state rows as well as the ordinary entry.
The ignored `local/issue1005/review.py` receipt records these selected joins;
it is not a new maintained parser or a substitute for full prose review.

Check the complete document/diff, relative links/anchors, UTF-8/LF, sole-path
and private boundaries and `git diff --check`. On clean committed head run
`uv run sf2 verify plan --base origin/main --head HEAD`; record its actual
selection and exact-head CI in the Issue handoff. The documentation-only
agreement skips blanket local normal/full/engine/Godot/H3/H4 suites.

Preserve [#1001 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1001#issuecomment-6181719337),
[ignored-only correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1001#issuecomment-6181773472)
and [PR1004 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1004#issuecomment-6181778859):
head1ac04fb9/tree dab707d3/base af894656, CI38661497541 and original38661384831,
unchanged-pair0.082/0.080s, factSummary/private-row correction, HTTPS false-positive
correction and removal of the unjustified ignored Markdown line cap. Linked
#997/#993/#989 failures, completed results and Unknowns remain unchanged.
This slice's inspection path/shape diagnostics and any corrected checks remain
in its handoff; they do not reset earlier completed verification.

**Unknown:** natural arrival; actual flags, membership, HP/class/items, level/RNG,
current/saved map/battle/crop/positions/turn state; completed load/action/fade/
VInt/interpreter services, live presentation/control and save persistence.
No activation, turn, outcome or Higins recruitment is established. Retain source,
inputs, pair, environment and receipts; freeze a clean pushed Draft PR for
independent review. No merge, cleanup, archive or next Issue.
