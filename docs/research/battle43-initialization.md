# Battle43 initialization, Zeon and the complete formation

Evidence date: 2026-10-22. **Confirmed (static):** Battle43 selects
12 formation slots,16 STARTING enemies,one region and two points.
Zeon128 is rebuilt at physical47; neutral Elis uses combat159 backing
but logical alias31. Both before/start hooks are empty. Stop before the
first ActivateEnemies. Natural reach, completed services, original-ROM
agreement and playable remake support remain **Unknown**.

## Agreement, provenance and method

Standard rigor for [Issue #1176](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1176)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053).
Accepted base `15cda51342eb0f196df8ecbead2ae369b4c0d8b7`, tree
`0019af359f27f1e76ea92434ba392b7bca9e86c4`. The outcome is a complete
selected initialization contract for future design/content integration.
Own this document and successor navigation plus the narrowly corrected
scene-Zeon ENTNUM statement in
[Battle42 outcomes and Zeon admission](./battle42-outcomes-and-zeon-admission.md).
No shared owner,engine,extractor,schema,fixture or index changes; no
stacked dependency. Zeon's special allocation and Elis's backing/alias
split require Standard rigor. Activation geometry,queues,first control,
laser targeting/damage/presentation,outcomes and ending are excluded.

Reuse [Battle42 initialization](./battle42-initialization-and-opening.md),
[Battle42 first control](./battle42-activation-and-first-control.md),
[Battle36 initialization](./battle36-initialization-and-opening.md),
[laser owner](./battle36-prism-flower-laser.md),
[spritesets](./battle-spriteset-data.md), [stats](./common-stats.md),
[battle loop](./battle-loop.md), [maps](./common-maps.md) and
[scripting](./common-scripting.md). Selected operands and actual producer/
consumer conditions are rechecked here; those general owners remain authoritative.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` means `code/gameflow/battle/`; `entity/` means
`code/common/scripting/entity/`. sf2const.asm,sf2enums.asm and original
macros own constants and encoding. Ordinals are zero-based; intervals
half-open; packed words/addresses hexadecimal. Fnnn denotes a story flag.
USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source equality or original-runtime observation is claimed.

Method: linear selected source/consumer reads and small supplied-state
arithmetic,seconds per command. Evidence estimate<2MiB,total including
CI/coordination<4MiB; peak-memory target512MiB **unmeasured**. Reassess
material coupling or budget growth. Ignored recipe/receipts belong to
`local/issue1176/`. No static pair read/regeneration,lowering,world/terrain
export,runtime acquisition or new verification framework is needed for
empty hooks. Preserve the existing18,285,979-byte pair and provenance.

## Admission, fresh entry, empty hooks and F88

**Confirmed:** the predecessor owns the conditional after42→ordinary59
setup→warp→MainLoop chain. At the next CheckBattle, the complete45×7-byte
`data/battles/global/battlemapcoords.asm` table tests42 before43.
Its43 row `$7B63..$7B6A` is `59,0,0,28,38,255,255`: map59,crop0,0,
width28,height38,wildcard X/Y. Held442 still selects42 first. With442
clear and443 held,43 is selected. Clear443 skips it. CheckBattle writes
crop bytes before testing543; held543 clears443 but **still returns43
on this call**. Completion does not fall through. MainLoop passes
resolved map59 in d0.w and selected43 in d1.w to j_BattleLoop.
No natural incoming flag or party value is assumed.

`battle/battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`) clears
PLAYER_TYPE.b,sets399,then tests88. Clear88 follows this exact order:
clear SECONDS_COUNTER.l→save incoming d0/d1 words→assign their low bytes
to CURRENT_MAP/CURRENT_BATTLE→SetBaseVIntFunctions→before wrapper→restore
saved words→rewrite both selector bytes→clear flags90–105 inclusive→
HealLivingAndImmortalAllies→InitializeAllAlliesBattlePositions→
InitializeAllEnemiesBattlePositions→ClearAiMemory→clear d0.w→LoadBattle→
start wrapper. The next @Start instruction is the excluded ActivateEnemies.
Restoring selector words does not undo other flags,entities,layout or service
mutations. Subsequent helpers read live selectors at their own calls.

| Complete relative-word table / selected43 entry | Exact target |
| --- | --- |
|beforebattlecutscenes.asm,48 words,base47A88,byte86 at47ADE|ms_Empty minus table base|
|battlestartcutscenes.asm,48 words,base47B2C,byte86 at47B82|ms_Empty minus table base|
|code/common/scripting/map/ms_empty.asm,2 bytes at47A4E|ms_Empty: FFFF, end only|

The before wrapper `$47A50..$47A88` tests450+43=**493**,skips when
held and does not set it. Start `$47AEE..$47B2C` independently tests493,
skips when held and sets493 **before** interpreting the empty body.
Empty ExecuteMapScript still resets dword_FFB1A4.l to
FF9004_LOADING_SPACE,clears text-skip,conditionally waits for view scroll
when dialogue is open,and clears VIEW_SCROLLING_SPEED.w on return.
P2 Start in debug can reassert text-skip after its initial clear.
Skipping a wrapper body bypasses these interpreter effects.

| Supplied state at each named read | Conditional result |
| --- | --- |
|88 clear,493 clear throughout|Empty before,fresh reconstruction/load,set493,empty start.|
|88 clear,493 held throughout|Both bodies skip; fresh healing,positions,stats and load still run.|
|493 changes between wrappers|Each follows its own read; no shared latch snapshot.|
|543 held but443 admits|Admission clears443 yet returns43; fresh definitions still initialize,neutral creation skips.493 is independent.|
|542 held/clear|Controls Map59 layout copy at load,independently of543 neutral gate and493 hooks.|
|Interrupted prefix|493 can be clear after before effects or held before start returns; no atomicity,save or control-ready claim.|

Held88 instead restores SAVED_SECONDS_COUNTER.l,clears88,clears AI
memory and VIEW_TARGET_ENTITY.b,then calls LoadBattle **without clearing
incoming d0**. It branches directly to @IndividualTurns_Loop; the following
branch to @Start is unreachable. Fresh selector assignment,hooks,region
reset,healing,formation/stat initialization and round generation are bypassed.
Retained CURRENT_MAP/BATTLE,crop,combatants,flags,queue,cursor and RNG are
inputs; incoming59 does not install current59/43 on this path.

For intact incoming d0.w=59,LoadBattle preserves that word across
PositionBattleEntities. `battlefunctions_0.asm:GetEntityIndexForCombatant`
tests its low byte:59 is nonnegative,so skips enemy96 subtraction;
FF masking gives offset59 at **FFB17B**,inside64-byte
ENTITY_INDEX_LIST=`FFB140..FFB180`. That byte is enemy alias27/logical155,
not ally59 or an out-of-table read. Fresh43 would mark155 absent,aliasFF;
retained F88 may instead pack an eligible155. Getter replaces d0.b with
alias T; LoadBattle stores T as view target. Negative T uses physical0
for coordinate reads; otherwise T&3F selects a32-byte record without a
49-slot guard. FF/80→0,00→0,30→48,31→49,7F→63 are arithmetic examples,
not observed resume bytes or proof of safe restoration.

ClearAiMemory resets48 last-target bytesFF atFFB1AC..FFB1DC and48 memory
bytes0 atFFB1DC..FFB20C. It does not reset adjacent newly-triggered word
FFB20C..FFB20E,regions or turn buffer. PositionBattleEntities clears
64 aliases and ENTITY_WALKING_PARAMETERS scratch as described below;
F88 bypasses the empty interpreter's separate FFB1A4 reset. These distinct
writes must not be described as a complete saved-state restoration.

## Complete12/16/1/2 spriteset

**Confirmed:** `data/battles/spritesets/spriteset43.asm` occupies
`$1B6A5A..$1B6BBE`,356 bytes: header4,allies144,enemies192,region12,
points4. No terminator follows the counted subsections. Macro declarations
in sf2macros.asm define each12-byte combatant row: identity/X/Y/AI bytes,
placement-item word,primary order/region and secondary order/region bytes,
filler byte and spawn byte. The last pair is read as a word at offset10.
All12 ally rows have HEALER1=0,item NOTHING007F,primary/secondary FF/15,
filler0 and STARTING0. Their identities0–11 are formation ordinals,
**not fixed recruited characters**.

| Ordinal / address | X,Y |
| --- | --- |
|0 /1B6A5E|13,16|
|1 /1B6A6A|14,16|
|2 /1B6A76|12,16|
|3 /1B6A82|13,17|
|4 /1B6A8E|14,17|
|5 /1B6A9A|12,17|
|6 /1B6AA6|11,16|
|7 /1B6AB2|11,17|
|8 /1B6ABE|11,18|
|9 /1B6ACA|13,18|
|10 /1B6AD6|12,18|
|11 /1B6AE2|14,18|

All16 enemies have secondary NONE=FF,secondary region15,filler0,
STARTING0. Thus the following rows plus those common fields specify
all12 bytes. FF is NONE;40/41 mean MOVE_TO point0/1;80/84/87/8A mean
FOLLOW_ENEMY ordinal0/4/7/10. These are orders,not resolved routes.

| Ordinal / logical / address | Kind(ID),X,Y | AI(byte) | Primary order/region; secondary region | Item word |
| --- | --- | --- | --- | --- | --- |
|0 /128 /1B6AEE|ZEON(98),13,6|LEADER(14)|FF/15;15|007F|
|1 /129 /1B6AFA|DEMON_MASTER(75),10,8|ATTACKER3(9)|80/0;15|007F|
|2 /130 /1B6B06|PYROHYDRA(37),16,8|ATTACKER3(9)|80/0;15|007F|
|3 /131 /1B6B12|HORSEMAN(66),8,9|ATTACKER3(9)|84/15;15|007F|
|4 /132 /1B6B1E|ZEON_GUARD(38),7,9|LEADER(14)|FF/15;15|007F|
|5 /133 /1B6B2A|DEMON_MASTER(75),4,10|ATTACKER3(9)|84/15;15|007F|
|6 /134 /1B6B36|PYROHYDRA(37),7,12|ATTACKER3(9)|84/15;15|007F|
|7 /135 /1B6B42|ZEON_GUARD(38),19,7|LEADER(14)|FF/15;15|007F|
|8 /136 /1B6B4E|PYROHYDRA(37),20,8|ATTACKER3(9)|87/15;15|007F|
|9 /137 /1B6B5A|ARCH_DEMON(55),19,9|ATTACKER3(9)|87/15;15|007F|
|10 /138 /1B6B66|DEMON_MASTER(75),16,13|LEADER(14)|40/15;15|007F|
|11 /139 /1B6B72|PYROHYDRA(37),17,14|ATTACKER3(9)|8A/15;15|007F|
|12 /140 /1B6B7E|PYROHYDRA(37),15,21|ATTACKER1(6)|FF/15;15|007F|
|13 /141 /1B6B8A|ARCH_DEMON(55),4,22|ATTACKER1(6)|FF/15;15|007F|
|14 /142 /1B6B96|DEVIL_GRIFFIN(54),19,26|ATTACKER1(6)|FF/15;15|007F|
|15 /143 /1B6BA2|BLUE_SHAMAN_0(102),12,7|HEALER5(4)|41/0;15|6008|

The sole region at1B6BAE is exactly
`4,0;2,4;10,12;16,12;19,4;0,0` (12 bytes). Point0 at1B6BBA is16,13;
point1 at1B6BBC is15,8. Region membership,integer edge geometry,order
selection and first activation are reserved for the next result.

### Definition copies, holdings and refreshed current values

**Confirmed:** `battle/battleloop/initializecombatants.asm` initializes
all32 IDs128–159. Declared128–143 all enter InitializeEnemyStats.
Absent144–159 fail the16-count comparison **before** advancing a0:
activation0000,max/currentHP0,X−1,triggers15/15 from the first Zeon row.
Y,kind,class,MP,items,spells,orders and other fields are not cleared or
copied for these absent slots. Later neutral creation separately changes159.
There are no declared HIDDEN/RESPAWN rows in43.

InitializeEnemyStats reads the kind,checks random upgrade,copies its
56-byte definition in14 longwords,sets HP/MP to maxima,sets X/Y,combines
mover upper nibble with placement AI low nibble,writes kind,triggers,
orders,placement item,activation word,difficulty baseATT,then calls
UpdateCombatantStats.43 is absent from the complete random list
3,8,9,10,14,17,19,21,24,26,35; held543 does not upgrade it.
`table_EnemyDefinitions=$1B1A66`; each address is base+56×kind.
All eight selected definitions have spellPower ENHANCED=99 in the copied
class byte and initialStatus NONE. Unlisted item slots are007F; unlisted
spell slots are3F. LV1 is implicit,LV2/3/4 add40/80/C0 to the spell byte.

| Kind / ID / definition address / sprite | Level,HP,MP,baseATT,DEF,rawAGI,MOV | Mover | Copied items / spells |
| --- | --- | --- | --- |
|ZEON /98 /1B2FD6 /251|99,500,180,102,50,203,0|REGULAR|none /DEMON2|
|DEMON_MASTER /75 /1B2ACE /139|42,73,68,48,50,65,6|MAGE|Mystery Staff100 equipped /FREEZE4|
|PYROHYDRA /37 /1B227E /101|44,80,0,101,53,55,4|STEALTH|none /FLAME3|
|HORSEMAN /66 /1B28D6 /130|41,67,0,59,57,55,7|CENTAUR_ARCHER|Buster Shot52 equipped /none|
|ZEON_GUARD /38 /1B22B6 /102|45,130,150,109,54,64,5|REGULAR|none /DEMON1|
|ARCH_DEMON /55 /1B266E /119|44,99,65,99,46,61,6|FLYING|none /BOLT3|
|DEVIL_GRIFFIN /54 /1B2636 /118|42,89,0,96,47,59,7|FLYING|none /FLAME3|
|BLUE_SHAMAN_0 /102 /1B30B6 /149|42,72,60,64,54,51,5|HEALER|Holy Staff97 equipped,Healing Rain8 /AURA3,BLAST4|

unknownByte is0 for Zeon/Blue Shaman102;otherwise it is the selected
kind ID. The macro adds nine zero bytes after it. HP/maxHP and status/
resistance/activation are words; MP,ATT,DEF,AGI,MOV,prowess,class,level,
X/Y and packed triggers are bytes; four items are words,four spells bytes.
Orders are two bytes. Kind is finally written at combatant offset55.
Definition current numeric fields begin as zero placeholders; the
following table describes the **actual refreshed** fresh result.

| Kind(ID) | Resistance | Prowess critical,double,counter | Definition AI → initial activation | CurrentATT ordinary /SUPER; currentAGI |
| --- | --- | --- | --- | --- |
|ZEON(98)|STATUS_IMMUNITY|CRITICAL125_1IN4,DOUBLE_1IN4,COUNTER_1IN4|1000→1000|127 /158;200|
|DEMON_MASTER(75)|LIGHTNING_MINOR/STATUS_IMMUNITY|CRITICAL150_1IN32,DOUBLE_1IN32,COUNTER_1IN16|2000→2000|99 /114;65|
|PYROHYDRA(37)|ICE_WEAKNESS/FIRE_MAJOR|CRITICAL150_1IN8,DOUBLE_1IN8,COUNTER_1IN8|2000→2000|126 /157;55|
|HORSEMAN(66)|0|CRITICAL150_1IN8,DOUBLE_1IN16,COUNTER_1IN32|2010→2000|110 /128;55|
|ZEON_GUARD(38)|STATUS_MAJOR|CRITICAL_SLOW,DOUBLE_1IN16,COUNTER_1IN16|2000→2000|136 /170;64|
|ARCH_DEMON(55)|LIGHTNING_WEAKNESS/FIRE_MINOR/STATUS_MAJOR|CRITICAL_SLOW,DOUBLE_1IN32,COUNTER_1IN16|2010→2000|123 /153;61|
|DEVIL_GRIFFIN(54)|WIND_WEAKNESS/ICE_WEAKNESS/FIRE_MAJOR/STATUS_IMMUNITY|CRITICAL150_1IN8,DOUBLE_1IN16,COUNTER_1IN8|2010→2000|120 /150;59|
|BLUE_SHAMAN_0(102)|0|CRITICAL_SILENCE,DOUBLE_1IN32,COUNTER_1IN32|3000→3000|109 /129;51|

Initial activation preserves only the definition's upper priority nibble,
then combines rotated/masked filler/spawn; all selected filler/spawn=0.
BIT4 is stripped;1000/2000/3000 is not PRIMARY_ACTIVE bit0. Difficulty's
duplicated conditional applies floor(baseATT×5/4) once for every difficulty,
twice with intermediate truncation for SUPER. Stored base/current ATT are
bytes; mulu/lsr arithmetic uses long intermediates. Equipped ATT is added
later. All selected results fit the caps. HP/MP are maxima; DEF/MOV/
resistance/prowess retain the listed definition values; current status0.

`code/common/stats/updatecombatantstats.asm` copies base current values,
applies status effects even at zero Boost/Slow,then scans four equipped
slots. Zeon's rawAGI203=CB first goes through zero Boost's
IncreaseCurrentAgi→IncreaseAndClamp7Bits: preserve high80 plus low4B,
stillCB. Zero Slow calls DecreaseCurrentAgi→DecreaseAndClampByte with
CHAR_STATCAP_AGI_DECREASING=200,so it becomes **200=C8** even for a zero
decrement. Zeon has no equipment to change it afterward. The resulting
byte has high80 and low48 hexadecimal (decimal72),not raw203/low75.
This does not calculate turn count/order or laser behavior.

Mystery Staff00E4 adds ATT39 and MP_RECOVERY2;Buster Shot00B4 adds37;
Holy Staff00E1 adds29 and HP_RECOVERY3. Recovery entries are no-ops in
the stat-refresh dispatch,not extra initial HP/MP. Already-equipped
copies refresh without a new class-eligibility check; their weapon
presence does not prove class99 may newly equip them. No copied cursed
item occurs here. Weapon/use/drop actions are outside this stop.

BLUE_SHAMAN_0 is definition102,not85: it already contains
`00E1,0008,007F,007F`. Its placement word is
HEALING_RAIN8|USABLE_BY_AI2000|UNUSED_ITEM_DROP4000=**6008**.
InitializeEnemyItems→AddItem finds first empty slot2,masks with807F and
stores0008; return d1 is that masked word,so equipped bit7 is clear and
no EquipItemBySlot path follows. Final inventory is
**00E1,0008,0008,007F**. Both rains are unequipped and skipped by stat
refresh. This is actual native storage,not retained6008 metadata or
only one bottle. Healing Rain has equipFlags NONE,range0–0,price10000,
RARE|CONSUMABLE,use AURA4,three NONE effects. AI consumption and the
separate selected drop transaction are deferred; placement flags do not
prove their future admission or preservation in inventory.

## Combat population, special47 and neutral Elis

**Confirmed:** HealLivingAndImmortalAllies (`$23BFC..$23C58`) visits all30
allies. Living allies and immortal Peter7/Lemon28 even at HP0 heal to
maxHP/maxMP,retain the STUN/POISON/CURSE mask and refresh stats,including
reconstruction of equipment curse. Other dead allies skip refresh.
There is no resetForceBattleStats here; membership,EXP,kills,defeats and
holdings are not generally reset. Bowie's death has no special fresh heal.

InitializeAllAlliesBattlePositions writes X−1 to all30,rebuilds force/
active lists,then reads at most12 active entries in ID order. A dead
entry consumes an iteration without consuming a formation row. Excess
active entries LeaveBattleParty without another list rebuild. X-only
reset does not clear Y or unrelated state. Empty membership retains
the native N−1 underflow boundary; no managed empty-party guard is proved.
Thus12 slots do not promise12 living members or a particular Bowie slot.

`entity/entityfunctions_1.asm:PositionBattleEntities` first calls
ClearEntities:49 physical records0–48 each32 bytes are reset to off-map
7000,7000 with cleared motion/script fields;64 alias bytes are zeroed;
ENTITY_WALKING_PARAMETERS.l becomes FF5600_LOADING_SPACE and sub_19B0
is called. This destroys previous scene population,not combatant state.
It then scans **all30 allies and all32 enemies**,not just declared rows.
The ordinary physical counter starts0 and packs in logical-ID order:

| Producer / eligibility | Allocation and alias result |
| --- | --- |
|Ally0–29: signed Y.b>=0,then signed X.b>=0|No HP,membership or AI test. Declare at current counter,write matching ally alias,increment. Otherwise aliasFF.|
|Enemy128–159: NEUTRAL0008 clear,then signed Y.b>=0 and X.b>=0|No HP,declared-count or PRIMARY_ACTIVE test. Eligible ordinary sprite declares at counter,writes its enemy alias,then increments. Skipped row writesFF.|
|Eligible enemy mapsprite>=240|Save counter,declare physical47 with ENTNUM32,write47 to that enemy alias,restore counter; **no increment**.|
|Neutral pass after enemy scan|Test500+live CURRENT_BATTLE; held skips entire pass. Clear searches table,updates backing159 downward,and allocates from ordinary counter.|

Eligible ordinary coordinates become `(byte&3F)×384` in entity X/Y words.
64 maps to0,127 to63,and128–255 fail the signed admission test. Flying/
hovering movers set layer1;others0. DeclareNewEntity sets position and
destination,clears velocity/travel/acceleration,installs sprite,script,
facing and ENTNUM,sets flags E040,and uses the physical slot for its
animation counter(+1) and action wait timer. Ordinary ENTNUM equals
physical slot; special Zeon ENTNUM32 does **not** equal47 or logical128.
All combat declarations initially use Down3 and eas_Standing. That
script configures motion/obstruction then loops idle;it does not change
facing to a laser direction. Ally sprites use actual class/Rohde membership;
CURRENT_BATTLE43 bypasses the outside-battle dead-ally blue-flame branch.
Enemy sprite lookup uses live enemy kind and table_EnemyMapsprites.

Let A be eligible allies after fresh placement (0≤A≤12 for a valid
bounded party). Fresh Zeon128 has HP500,X13/Y6,neutral clear,kind98→
sprite251,so writes enemy alias0 at **FFB160=47**,physical47 at13,6,
Down/eas_Standing. All15 other declared enemies129–143 pack at
**A..A+14**,so alias for ordinal i=1..15 is A+i−1. The counter ends
A+15. Enemy144–159 are absent,X−1,so enemy aliases16–31 stayFF at this
point. Ally aliases30/31 are still fresh0: neither is a scanned ally.
The aftermath's scene Zeon130/physical47 is a different identity.
Its table initially parks it at62,62;after42 later setPos130 moves it
to13,6/Down. The predecessor's conditional normal43 admission then passes
through ordinary Map59 population rebuilding before fresh combat
reconstruction. Combat Zeon128 at13,6 does not continue scene130;
position differences do not establish the reconstruction.

Zeon Guards132/135 use ordinary sprite102,not special47. Their fresh
physical slots are A+3/A+6 and their declared coordinates7,9/19,7;
both initially face **Down3**. `data/battles/global/laserbattles.asm`
lists battles36,43,0 with a pointer per battle. Its43 table has16 bytes:
`FF,FF,FF,FF,00,FF,FF,02,FF,FF,FF,FF,FF,FF,FF,FF`,where ordinal4
is LASER_RIGHT0 and ordinal7 LASER_LEFT2. PositionBattleEntities,
DeclareNewEntity,eas_Standing and LoadEntityMapsprites do not call
GetLaserFacing. DmaEntityMapsprite uses stored facing and ordinary/special
sprite dispatch,not this laser table. The direct jump-alias call sites
are StartAiControl,ExecuteAiControl and battlescene action construction;
GetLaserFacing also constructs a ray grid. Those are action interfaces,
not initialization. No target scan,damage or visual result is derived here.

### Selected neutral table, addresses and changed states

**Confirmed:** `data/battles/global/battleneutralentities.asm` occupies
448C4..4497A. Its selected43 row at **4496C..44978** is12 bytes:
battle.w43;X11/Y15/facingDown3/spriteELIS204 bytes;eas_LyingLeft.l;
FFFF.w terminator. The final global FFFF is44978..4497A. Battle42's
separate row at44960 uses13,15;the previous aftermath's scene129 also
uses13,15. Neither supplies43's selected11,15.

Clear543 after enemy reconstruction changes backing combat159:
maxHP.w=0,currentHP.w=0,status.w=0,activation.w OR0008,X.b=11,Y.b=15.
It then declares physical **A+15**,Down,sprite204,eas_LyingLeft,layer0,
ENTNUM=A+15. It resets a1 to ENTITY_INDEX_LIST_ENEMIES=FFB160 and
literally writes `move.b d0,-(a1)`: **FFB15F**,offset31/logical
FOLLOWER_B,not enemy159's offset63 atFFB17F. Fresh enemy159's alias
remainsFF,so GetEntityIndexForCombatant(159) does not resolve Elis.
Logical31 resolves her physical slot;alias30 remains0. Fresh159 activation
becomes0008;its kind,class,MP,items,spells and other unstated fields stay
stale. eas_LyingLeft orientsLeft,refreshes,falls through ac_motion OFF
and branches idle;these requests are not initialization of an enemy
kind or proof of completed lying presentation.

Held543 skips neutral state writes,alias31 write and allocation. After
fresh initialization159 remains HP0,X−1,activation0,aliasFF;alias31
stays0 and resolves physical0 through normal logical access. It is not
a validated absent-reference error. F542 has no role in this neutral gate.
For A≤12,with Elis there are A+17 occupied records (maximum29):A
allies,15 ordinary enemies,one neutral and special47. Without Elis,
A+16 (maximum28). The largest ordinary/neutral index is27,and47 remains
inside49 cleared records with no normal collision. LoadEntityMapsprites
scans48 records0–47 (ENTITIES_COUNTER_MINUS_ONE=47),not all49 cleared
records;the fresh selected population lies within that scan. Count,highest slot
and capacity are distinct quantities.

| Changed live input at reconstruction | Actual branch |
| --- | --- |
|A declared enemy HP0 but signed X/Y still nonnegative,neutral clear|Still packs,including Zeon at47. HP0 alone is not the entity predicate.|
|Zeon128 X<0 or Y<0|Alias128 becomesFF;special47 stays cleared unless another eligible special writes it. Ordinary counter is unaffected.|
|Zeon neutral bit held|Enemy scan skips before coordinates/sprite,aliasFF.|
|One ordinary enemy omitted|Later ordinary slots compact downward;Zeon stays47;neutral takes the new counter.|
|An eligible undeclared slot in retained F88 state|May pack despite no spriteset row;fresh32-slot initialization would have made X−1.|
|Retained159 living/positioned,neutral clear,543 held|Ordinary scan may pack it;held543 only skips the later neutral pass.|
|Retained159 neutral held|Enemy scan skips regardless of HP;clear543 can still create selected Elis,held543 leaves no neutral allocation.|
|Retained159 ordinary-packed,then clear543|It can leave an earlier physical record/alias159 and also create selected Elis while overwriting backing159 HP/status/coordinates;no cleanup of that earlier record.|
|More eligible retained allies/enemies or multiple special sprites|Native code has no capacity/collision guard;ordinary counter can reach47/48 or beyond,or another special overwrites47. Fresh A≤12 arithmetic is not a general F88 safety proof.|

These variations keep ally positioning,physical packing,neutral creation
and later living-combatant counting separate. No universal alive/present
predicate describes all of them. No supplied variation is a runtime observation.

## Ordered load, Map59 layout and service boundary

**Confirmed:** `battle/battlefunctions/loadBattle.asm` (`$25610..$256A2`)
saves input combatant word,reads CURRENT_MAP.b,requests fade-out,sets view
targetFF,loads tilesets,waits for fade,clears VInt functions and waits one
VInt. It calls PositionBattleEntities (including the neutral pass),restores
the saved input and resolves its alias,stores view target,and derives
BATTLE_ENTITY_CHOSEN_X/Y.b by signed division of entity coordinates by384.
Fresh d0.w=0 means actual ally0 alias,with negative fallback physical0;
it does not assume Bowie was living or the first eligible member.

It then InitializeSprites(input63,64 sprite records via DBF),sets
d0.w=−1,LoadMap,WaitForVInt,
LoadEntityMapsprites,SetBaseVIntFunctions,LoadBattleTerrainData,
PlayMapMusic,FadeInFromBlack.43 does not take44's timer-window branch.
`data/battles/terrainentries.asm` slot43 selects BattleTerrain43;
no terrain bytes are decoded here. The start wrapper follows only after
LoadBattle returns;its493 write precedes its empty interpreter,then this
slice stops before ActivateEnemies.

LoadMap is the native map reload path,not ordinary exploration's entity/
follower/setup init. Its LoadMapBlocksAndLayout loads blocks,decodes fresh
base working layout,applies held flag-copy rows,chest-open writes and
battle crop,in that order. Map59 `3-flag-events.asm` atB939E..B93A8 has
one eight-byte row and FFFF.w: F542.w;source12,54;size7,4;destination10,3
(bytes;packed words0C36,0704,0A03). Held542 copies28 layout words/56
content bytes in the64-wide working buffer;clear542 omits it. Table
storage10 bytes is not the transferred byte count. The source patch is
outside crop0,0/28×38 and destination inside it. LoadMapArea uses battle
dimensions (end27,37),separately from ordinary Map59 area0,0..26,37,
layer255,zero origins/autoscroll,parallax256,music FINAL_BATTLE.

Battle43's LoadBattle rebuilds the working layout;it does not preserve
all prior scene mutations. F542 copy,543 neutral gate and493 intro latch
are independent reads. LoadBattle does not run ms_map59 default init or
ms_map59_flag543 ending program and does not clear temporary256–383 as
ordinary field entry does. Prior scene population/actions and Elis's old
coordinates are replaced;combat backing fields are changed only by the
named producers. Tactical BattleTerrain43 is independent of the layout
copy;collision,graphics,markers and rendered terrain remain unobserved.
Requested fade,tilesets,sprites,special-sprite loading,music,VInt/DMA and
view services are interfaces. Static call order does not prove their
natural completion or a free-input interval.

## Actual producer, Content, Domain, Application and adapter

**Confirmed about accepted code:** no new lowering is needed for two
Empty hooks,and none ran. The predecessor's **single** after42 diagnostic
still raises `ValueError: special cutscene entity not admitted` on
ce_4F328's sprite251. It retained five instructions and cached eas_Init14,
without a scene-entities instruction,continuation,end or published world.
That partial dictionary is not a successfully imported Zeon scene.
[The accepted producer account](./battle42-outcomes-and-zeon-admission.md#actual-final-producer-and-production-consumption)
owns the retained observation and original214-command corpus distinction.

Actual `prepare` checks canonical identity/source pin,compiles selected
programs and routes,then maps/requested setups,binds materials and writes
the final serialized maps/programs. A selected compilation failure before
the final write cannot be called a published world;earlier other failures
can occur first. Empty43 hooks remove no encounter,definition,map,neutral,
special-sprite or material admission requirement. A table's existence and
a typed primitive's shape do not bind the original contract.

| Actual owner | Present mechanism and selected43 boundary |
| --- | --- |
|[Encounter preparation/routes](../../src/sf2tool/remake_exploration_battles.py), [final producer](../../src/sf2tool/remake_exploration_content.py)|prepare_encounter admits2/3. compile_routes can accept wildcard coordinates and produces before/load/start flags/programs;its shared source-battle-load is FadeOut→BattleLoad→FadeIn→end. This is a typed presentation sequence,not native PositionBattleEntities/neutral/stat fidelity. Selected after42 still refuses when requested.|
|[Encounter reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [definition reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs), [scenario assembly](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Reader pins only1/2/3;assembly also requires1/2/3. Definition metadata retains unknownByte,spell power,raw stats,resistance,prowess,items/spells,mover and AI expression. Assembly additionally restricts REGULAR spell power,selected commandsets and placement item expressions.43 has ENHANCED99,HEALER5,Healing Rain and BIT4 AI expressions outside those bindings. No43 encounter/terrain/definitions package was produced.|
|Same scenario assembly's actual derived fields|It splits raw enemy BaseAgility directly into low7/high80,and treats the admitted Medical Herb placement as item0. Merely widening admission would retain Zeon's unrefreshed low75 instead of72,and would not implement6008→duplicate Rain8. Copied staff/shot refresh,ENHANCED spell effects and full native loadout/current-state rules need bound behavior,not an allowlist edit.|
|[Initialization rules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [activation validation](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs)|Supports roster-only,already-refreshed allies,difficulty0,STARTING formation and initial priority/filler formula with bounded region/deployment checks. Nonzero difficulty and enemy items other than0/127 refuse.43's STARTING declarations fit that spawn mode;equipped staff/shot andRain do not fit the admitted refresh. This is not all30 healing/native stale-slot reconstruction or neutral creation. No region geometry/activation was executed here.|
|[BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Before→initialize→load→start continuations exist;NewBattle with88 refuses suspended-battle-entry. Initialization computes before committing its state/region clear,but field entry can already have published selection/before prefixes;load completion commits intro flag/start before final BattleStartFinished advances. The prior direct Start versus field-publication correction remains binding. No43 entry or complete rollback claim.|
|[SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [allocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [entity actions](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Actual follower-first/fixed population,zero aliases,physical slots and action/sprite waits exist with49-slot checks. This allocator is ordinary scene allocation,not native combat scanning,special47/ENTNUM32,enemyFF aliases,or159 backing→31 neutral binding. Scene support cannot substitute for those distinct battle producers.|
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py), [Content instruction reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Ordinary scene population and typed motion/layout/flag/presentation primitives have consumers;special scene sprites>=240 refuse at compilation. native-call reads as UnsupportedInstruction and refuses when reached. No new scene lowering was run or repaired.|
|[Map preparation](../../src/sf2tool/remake_exploration_maps.py), [working layout](../../remake/src/Sf2.Remake.Domain/Maps/WorkingMapLayout.cs), [map transfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|Typed copy/rebuild/setup mechanisms exist. Default59 outside eventMaps still has the native F542-copy/setup frontier;the accepted conditional shared map-59-setup overwrite by requested543 remains **Inferred**. No default/variant/ending export or published59 package is supplied here;ordinary setup selection is separate from native combat load.|
|[Adapter presentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [session view](../../remake/game/src/Exploration/ExplorationSessionView.cs), [battle viewport](../../remake/game/src/Battles/BattleMapViewport.cs), [capability owner](../../remake/docs/capability-status.md)|BattleLoad invokes the actual prepare-battle callback for ActiveBattle;generic field/battle view and cue support exists. It does not itself execute native special47/Elis alias reconstruction or prove private43 sprite/terrain/material admission,DMA,music,service completion or playability. No adapter observation ran.|

Copied weapon definitions also distinguish inventory from class admission:
Buster Shot52 is SNIP/BRGN/BWNT,range2–3,price6800;Holy Staff97 is VICR,
range1–1,price9000;Mystery Staff100 is WIZ/SORC/VICR,range1–1,price10000.
All three are WEAPON|RARE,use NOTHING. Native refresh of already-equipped
enemies is separate from these class masks and from typed engine metadata.

**Inferred:** later integration can reuse ordered entry,formation/priority,
typed layout/entity actions and presentation primitives after resolving
selected package admission,actual refresh/loadouts,special combat identities,
neutral backing/aliases and materials. No engine/importer repair or new
capability is part of this evidence slice. Laser actions and first control
remain separate outcomes,not hidden acceptance promises here.

## Reproduction, retained failures and stopping boundary

The original candidate's ignored `local/issue1176/review.py` retains the selected source/consumer
snapshots,complete roster/definition/current-value derivations,ordered
call/flag/width guards and direct prose/link/anchor/UTF8/LF/private/owned
diff checks. It reads only metadata for the existing static pair and the
small retained after42 observation;it does not read/rebuild the pair,
re-lower a scene,run the producer or create a maintained checker framework.
Tracked prose and exact Git/source objects remain the durable contract.

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1176/correction_review.py <configured-source-disasm-root> --receipt local/issue1176/correction-reproduction-01.json --committed

git diff --check origin/main
uv run sf2 verify plan --base origin/main --head HEAD
```

Every receipt name must be **fresh and nonexistent**;change the example
after use. The source argument ends in disasm. Original complete direct
receipts and direct-03 snapshots remain anchored to candidate18e19598;
its navigation-only diff assertion is superseded for this correction.
correction_review.py checks only the constant→producer→byte-write and
corrected prose/links/exact two-path delta;it does not rerun full direct
source review,lowering or prior suites.
Load private configuration in the same process for each uv invocation.
Clean committed planner and actual scoped CI selection/results,frozen
head/tree,process state,resources and full command outcomes belong to
the Issue/PR handoff. Normal/full Python,old comparisons,SDK/Godot,
emulator,H3/H4 and screenshots are **SKIPPED** by this docs-only scope.

ADR0004 review covers complete provenance,date,shape,widths,aliases,
branch polarity,mutation order,changed valid states and final producer/
actual consumers. New production parser/schema/fixture/mutation-test/
H3-callback obligations are N/A. One-off source checks do not claim
new maintained parser mutation coverage. Initial path/read/helper failures
and self-review corrections remain in the ignored handoff.

A P2 correction in this slice fixes the predecessor's scene-Zeon
"entity number128" to **ENTNUM32**: mapfunctions.asm passes
ENTITY_ENEMY_START=$20 in d6;DeclareNewEntity stores d6.b at ENTNUM.
Scene logical130,physical47 and ENTNUM32 are distinct. #1174's author
and main-gate direct checks,two independent reviews and CI passed while
missing that numerical prose error. #1176's initial main-gate direct
PASS likewise did not cover this predecessor statement. Those accepted
records remain unchanged;their PASS is not proof of that value.

#1176's original author/main-gate direct checks and CI also passed while
missing its stale scene-coordinate claim. Source review found the two
P2 prose errors;the earlier PASS records remain evidence of their named
checks,not complete claim coverage.

Preserve [the predecessor's failures and Unknowns](./battle42-outcomes-and-zeon-admission.md#reproduction-preserved-failures-and-stopping-boundary):
the single after42 special251 refusal/five-op partial;eas_Init14 cache;
#1172's author/main-gate/CI PASS that missed Sleep/Stun and direct Start/
field-publication differences,then accepted correction;#1170's fresh
receipt reproduction-command correction and old-head CI/watcher evidence;
#1162 high-byte followEntity/BowieHP missed-finding PASS and correction;
geometry/read failures,Map54 final overwrite,Map53 failed import/zero
partial and twelve completed old comparison failures. None is erased,
called interrupted or repeated without invalidation.

**Unknown:** natural incoming party,membership,HP/status/items,flags,
aliases,retained combatants,crop/queue/cursor/death list/RNG;actual
CheckBattle reach,interleaved service/caller mutations and interrupted
prefixes;decoded blocks,markers,collision,terrain and materials;completed
sprite/special-sprite,motion,lying animation,view,fade,text,audio,VInt/DMA
or input services;hardware timing,source-ROM equality,save persistence,
original/remake playability and complete F88 restoration. Supplied static
branches close none of these and create no automatic runtime/repair queue.
Stop before first ActivateEnemies,freeze a clean pushed Draft PR with
actual scoped CI and handoff,then leave independent review/merge to
main-gate. Author does not merge,cleanup,archive or start another Issue.
