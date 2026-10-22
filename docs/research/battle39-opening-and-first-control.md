# Battle39 opening, initialization and first control

Evidence date: 2026-10-21. **Confirmed (static):** Battle39 stages five
non-allies, then replaces the scene with the actual party and fourteen
STARTING enemies. It has no AI regions or normal hidden-spawn lifecycle.
Evil Ring and Holy Thunder enter inventory without their placement
equipment/AI/drop bits. Intro489 controls the two hooks; F88 follows a
different retained-state entry. First control follows current AGI and
sequential RNG, then live HP/status/control checks. Hydra kind33 uses
ordinary LEADER AI, not Burst Rock kind32. Natural reach, completed
services, presentation and usable input remain **Unknown**.

## Agreement, evidence and method

Standard rigor for [Issue #1152](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1152)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
explain one coherent start-of-battle contract for later content work,
from accepted [Map66→39 admission](./battle38-outcomes-and-geshp-aftermath.md#exploration-rebuild-and-battle39-admission-boundary)
at base `e10de85af56a1a7c30dfbb4183215a3777a9a5f3` through the opening,
fresh initialization, generation and ordinary command/control interfaces.
Exact ownership is this document and navigation only in that predecessor.
No shared paths, competing writer, stacked dependency, schema, fixture,
parser, engine or design changes. Individual command execution, movement
decisions, damage, outcomes and the next map are outside this boundary.

Reuse [Battle38 initialization](./battle38-initialization-and-opening.md),
[activation and first dispatch](./battle38-activation-and-first-turn-dispatch.md),
[spritesets](./battle-spriteset-data.md), [global data](./battle-global-data.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[scripting](./common-scripting.md), [maps](./common-maps.md) and
[stats](./common-stats.md) for common mechanisms. Selected39 operands and
changed valid states are checked here; nonexistent regions need no
geometry probes. Formation rows do not fix the party's identities.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` abbreviates `code/gameflow/battle/`; `map/` and `entity/`
abbreviate children of `code/common/scripting/`. Constants, layouts and
macro encoding belong to sf2const.asm, sf2enums.asm, sf2macros.asm and
sf2cutscenemacros.asm. Fnnn means flag nnn; ordinals are zero-based and
address intervals half-open. The accepted USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This slice establishes no new source/ROM equality or runtime observation.

The bounded inputs are selected linear source/helper reads, actual
consumers, and the existing immutable18,285,979-byte init/script pair
with `local/issue1005/pair.txt` provenance. One existing-compiler opening
lowering records all programs, actions and native instructions. It
imports no map or definitions and runs no scene. Each command targets
under one minute/512MiB; time and sizes are retained, peak memory is
unmeasured. Unique ignored receipts belong to `local/issue1152/`.
No acquisition, full import/export, normal/full/old comparison suite,
SDK/Godot/emulator/H3/H4, screenshot or verification-tool tests. Reassess
before materially expanding inputs, processing or publication.

## Admission, fresh entry and F88

**Confirmed:** the predecessor owns the y28 zone callback and ordered
CheckBattle scan. Selected row39 is `66,17,20,23,32,255,255`, at
`$7B47..$7B4E`. Unlock439 must win after earlier Map66 rows2/3/4/6/38;
the crop is not an additional trigger-containment test. Held539 can
clear439 while still selecting39 on that scan. MainLoop passes map66
in d0.w and returned battle39 in d1.w to j_BattleLoop. High bytes and
live selectors remain caller inputs; the route does not prove arrival.

`battle/battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`) clears
PLAYER_TYPE.b, sets399 and checks88. Clear88 resets seconds, saves
d0/d1 words, installs their low map/battle bytes, installs base VInt
functions and calls the before wrapper. It restores those words and
rewrites both bytes, clears flags90–105 inclusive, heals living/immortal
allies, initializes all ally positions, initializes all enemy positions,
clears AI memory, clears d0.w and calls LoadBattle. The start wrapper
returns to @Start. Reached flags, entities, crop and service mutations
are not rolled back by restoring two words.

| Complete48-word table / selected39 offset78 | Target |
| --- | --- |
|`data/battles/cutscenes/beforebattlecutscenes.asm`, base `$47A88`, slot `$47AD6`|bbcs_39 at `$4E3D2`|
|`data/battles/cutscenes/battlestartcutscenes.asm`, base `$47B2C`, slot `$47B7A`|ms_Empty, sole FFFF word|

`cutscenes/beforebattlecutscenesstart.asm` tests450+39=489 and skips
when held, without setting it. `battlestartcutscenesstart.asm` tests
489 again and sets it **before** dispatching the empty start. Empty
ExecuteMapScript still resets dword_FFB1A4 to FF9004_LOADING_SPACE,
clears text skip, conditionally waits for the view when dialogue is
open, and clears view speed on return. Skipping bypasses those effects.

| Supplied state at the actual reads | Reached boundary |
| --- | --- |
|88 clear,489 clear throughout|Full before, fresh reconstruction/load, set489 then empty start.|
|88 clear,489 held throughout|Both hook bodies skip; fresh reconstruction/load still run.|
|539 held while439 admits|Completion and intro489 are independent; ordinary fresh prefix still applies.|
|489 changes between wrappers|Each wrapper obeys its own read; no immutable shared snapshot.|
|Selector changes within services|Helpers use live selectors; the saved-word restoration repairs only its named map/battle bytes.|
|Interrupted prefix|Before can have run with489 still clear;489 can be set before start returns. Neither flag proves completion or usable control.|

Held88 restores saved seconds, clears88, clears AI memory/view target
and calls LoadBattle without clearing incoming d0, then jumps directly
to @IndividualTurns_Loop. The following branch to @Start is unreachable.
It bypasses selector assignment, hooks, healing, region clearing,
initial positions/stats and first generation. Retained map/battle/crop,
combatants, region flags/checked word, queue/cursor and RNG remain inputs.

Reuse the accepted [incoming66 derivation](./battle38-initialization-and-opening.md#f88-incoming66-exact-out-of-table-read):
GetEntityIndexForCombatant reads alias `$FFB140+66=$FFB182`, beyond
the64-byte table ending180. It is not a named supported suspended-state
field, combatant162 or the chest word180..182. PositionBattleEntities
rebuilds normal aliases without repairing this offset. Fetched T becomes
view target; negative T selects physical0 for coordinate reads, otherwise
T&3F can exceed49 physical records. T=30/31 hex selects48/49; these are
conditional arithmetic, not observed bytes or a valid suspended battle.
ClearAiMemory resets48 last-target bytes toFF and48 memory bytes to0,
not the newly-triggered word, region flags or queue. A retained odd
cursor reads a score as an actor; offsets128–255 exceed the128-byte
buffer. Retained actorFF later returns to @Start with then-current state.

## Scene identities and complete opening

**Confirmed:** `data/battles/entries/battle39/cs_beforebattle.asm`
spans `$4E3D2..$4E4DC`,266 bytes. The37 top-level commands occupy190
encoded bytes; four interleaved move/end pairs plus one custom action
payload occupy28 more. The48-byte ce_4E4AC table is main header6,
five eight-byte records40 and terminator2. Interior labels cs_4E40C
and cs_4E41A are the two conditional continuation points.

| Logical identity | Scene requested X,Y / facing | Sprite/action |
| --- | --- | --- |
|main/Bowie0|8,9 /Down|live ally0/eas_Idle|
|128|8,20 /Down|HYDRA97/eas_Init|
|129|7,23 /Up|CHAOS_WIZARD138/eas_Init|
|130|9,23 /Up|EVIL_BISHOP148/eas_Init|
|131|6,23 /Up|DARK_GUNNER129/eas_Init|
|132|10,23 /Up|CYCLOPS95/eas_Init|

There is **no fixed Peter record**. loadMapFadeIn66,3,5 enters
csc37→csc48 and requests fade/map/view services; fadeInB is later.
It does not run exploration setup init. CURRENT_BATTLE is already39,
so LoadMapBlocksAndLayout/LoadMapArea retain the live battle crop17,20,
size23×32. They copy that rectangle to working origin0,0 and set layer1
ends22×384,31×384. Camera3,5 or3,18 and main8,9 do not replace the crop.
Scene coordinates are not combat placement coordinates plus this crop.

csc42 calls InitializeMapEntities/LoadEntityMapsprites with display/VInt
services. ClearEntities resets49 physical records and64 aliases; followers
allocate before fixed non-allies. Map66 is an overworld map. Held65
selects the Caravan65/FOLLOWER_A30 prefix followed by all19 ordinary
rows; clear65 selects the19 only. `data/scripting/entity/followers.asm`
owns their complete order. Every held row allocates, including repeated
aliases; later writes win. FOLLOWER_A may be Caravan65, King74,
Old Man77, then King83; FOLLOWER_B31 may be Astral74, Oddler68, then
Astral70. Without a producer a fresh zero alias addresses Bowie.

F67 alone supplies Peter7; F76 supplies Zynk26 and F71 supplies Lemon28
without recruiting or adding them to the battle party. Flag combinations,
not active membership, select followers. If n followers allocate, the
five fixed scene non-allies occupy physical n+1..n+5; Bowie is0. The
follower script pointer tables have five entries; arbitrary flag
combinations are not proved safe merely because n+6 fits49 records.
Ally sprites depend on live class/Rohde membership. The out-of-battle
HP0 flame branch does not apply while CURRENT_BATTLE=39.

| Index | Complete top-level order, including owned inline payload |
| --- | --- |
|0|textCursor3029|
|1|loadMapFadeIn66,3,5|
|2|loadMapEntities ce_4E4AC|
|3|setActscriptWait Bowie0,eas_Init|
|4|setPos Peter7,9,8,Down|
|5|setPos FOLLOWER_B31,7,8,Down|
|6|setPos FOLLOWER_A30,8,8,Down|
|7|jumpIfFlagClear76,cs_4E40C (index9)|
|8|setPos Zynk26,8,7,Down|
|9|cs_4E40C: jumpIfFlagClear71,cs_4E41A (index11)|
|10|setPos Lemon28,63,62,Down|
|11|cs_4E41A: fadeInB|
|12|cameraSpeed$30|
|13|setCamDest3,18|
|14|nextText00,128 →3029|
|15|nextSingleText00,128 →3030|
|16|entityActionsWait128: moveDown1,endActions|
|17|nextText00,128 →3031|
|18|nextSingleText00,128 →3032|
|19|setActscript129,eas_Jump|
|20|setActscript130,eas_Jump|
|21|setActscript131,eas_Jump|
|22|setActscriptWait132,eas_Jump|
|23|csWait50|
|24|entityActionsWait131: moveUp1,endActions|
|25|nextSingleText00,131 →3033|
|26|entityActionsWait132: moveUp1,endActions|
|27|setFacing128,Up|
|28|setCamDest3,5|
|29|csWait60|
|30|setCamDest3,18|
|31|nextSingleText00,131 →3034|
|32|customActscriptWait128: ac_setSpeed24,24; ac_jump eas_Idle; ac_end|
|33|entityActionsWait128: moveDown3,endActions|
|34|setFacing128,Up|
|35|nextSingleText00,128 →3035|
|36|csc_end|

### HP skips, aliases, motion and service completion

Reuse the accepted [action/HP/alias handlers](./battle36-initialization-and-opening.md#action-hp-alias-and-wait-semantics),
checked at csc19/csc14/csc15/csc2D and entityscriptengine_2.asm.
eas_Init sets acceleration0, speed32/32, obstruction/auto-facing, Up,
nonimmersed, size24, refresh and idle; clearing flagsA5/6 disables the
actual entity/map collision checks. Only Bowie explicitly gets this
waited Init. Fixed non-allies get Init from their records. Followers
retain their installed follow scripts when setPos changes position,
destination and facing; setPos does not cancel those scripts.

csc19's four-byte HP skip applies only to logical allies0–29. HP0
Peter skips9,8; HP0 Zynk/Lemon skip their selected positions even when
their flags are held. Both30/31 bypass HP checking. Missing7 with
nonzero Peter HP therefore repositions Bowie's aliased slot, while HP0
Peter skips it; missing30/31 always can reposition Bowie. setActscriptWait
Bowie has no HP gate. Later immortal healing of Peter/Lemon is separate.
Neither Zynk8,7 nor Lemon63,62 is recruitment, durable parking or a
battle placement. The four independent76/71 combinations reach both,
only Zynk, only Lemon or neither conditional setPos; HP still gates each.

csc15 and csc14 install their resolved slot timer/script and wait only
when requested until that pointer equals eas_Idle. eas_Jump disables
auto-facing, requests relative(0,−1), waits5, requests(0,+1), waits5,
requests(0,0), reenables auto-facing and jumps idle. esc04 computes each
destination from the **then-current** position and does not itself wait
for arrival. The signed wait counter and asynchronous movement mean the
net symbolic offsets do not prove an exact returned Y or complete motion.
The first three jump installations do not wait; the fourth waits only132,
not a four-entity barrier. csWait50 is a later requested delay.

csc2D clears collision bits, generates relative movement and waits for
the named slot's idle pointer. Hydra's waited Down1 then Down3 requests
lead from8,20 to8,24 if those motions complete; its last speed is24/24,
facing Up. It matches the later Hydra's numerical8,24 but does not carry
scene state into combat. The separate131/132 Up1 requests apply to their
then-current positions after the jump scripts; no exact endpoint is
asserted from adding signed script offsets alone. The custom128 stream
only installs speed24 and jumps idle; it has no move or HP/stat mutation.

cameraSpeed writes48 to VIEW_SCROLLING_SPEED.w. Each setCamDest detaches
the target, requests SetCameraDestination and waits for scroll end;
3,18→3,5→3,18 does not move the party or change crop. csWait50/60 and
the seven text requests are source interfaces, not measured hardware
duration. End conditionally waits for the view and clears view speed;
it adds no global follower/motion/audio/input completion guarantee.
No explicit music, quake, flash, reward, recruit or flag write occurs
inside bbcs_39. LoadBattle later requests ordinary map music separately.

### Seven text requests and speaker properties

The cursor joins gamescript.txt keys0BD5–0BDB in order. C=nextText,
S=nextSingleText; every modifier is00. Short authored roles retain full
token sequences without copying the original dialogue corpus:

| ID / hex | Mode / logical speaker | Authored role | Complete tokens |
| --- | --- | --- | --- |
|3029 /0BD5|C /128|Hydra reports Geshp's loss.|`{N}{W2}`|
|3030 /0BD6|S /128|Hydra identifies the remaining greater devil.|`{N}{W1}`|
|3031 /0BD7|C /128|Hydra sees an opportunity for promotion.|`{N}{W2}`|
|3032 /0BD8|S /128|Hydra links the jewel to that promotion.|`{N}{W1}`|
|3033 /0BD9|S /131|Dark Gunner points out the approaching party.|`{W1}`|
|3034 /0BDA|S /131|Dark Gunner announces their approach.|`{W1}`|
|3035 /0BDB|S /128|Hydra welcomes the party with a threat.|`{N}{W1}`|

`entity/getentityportaitandspeechsfx.asm:GetEntityPortaitAndSpeechSfx`
uses the resolved entity's actual mapsprite. Neither HYDRA97 nor
DARK_GUNNER129 has a row in `data/spritedialogproperties.asm`; the
fallback is portraitFFFF and SFX_DIALOG_BLEEP_6=74. Logical speaker128
does not mean a Hydra portrait exists. Text roles describe the supplied
scene, not a natural display observation.

## Exact encounter declarations and fresh reconstruction

**Confirmed:** `data/battles/spritesets/entries.asm` slot39 selects
BattleSpriteset39 at `$1B63FA..$1B6538`,318 bytes. spriteset39.asm
declares12/14/0/1: header4, formations144, enemies168, regions0, point2.
Relative offsets are formations4, enemies148, regions/point316, end318.
Each entity row is12 bytes. All filler values are0 and modes STARTING.

All twelve ally rows use HEALER1, NOTHING, NONE/15 and NONE/15:

| Ordinal | X,Y | Ordinal | X,Y | Ordinal | X,Y |
| --- | --- | --- | --- | --- | --- |
|0|8,9|4|6,8|8|7,7|
|1|7,9|5|7,8|9|8,7|
|2|6,9|6|5,7|10|8,8|
|3|5,8|7|6,7|11|5,9|

Every enemy has primary/secondary trigger15/15, secondary order NONE.
Point0 is8,24. The placement item column retains declaration syntax;
the next section derives stored holdings.

| Ordinal / ID | Definition | X,Y | AI | Primary order | Placement item |
| --- | --- | --- | --- | --- | --- |
|0 /128|HYDRA|8,24|LEADER|MOVE_TO\|0|NOTHING|
|1 /129|CHAOS_WIZARD|7,23|ATTACKER3|FOLLOW_ENEMY\|0|NOTHING|
|2 /130|EVIL_BISHOP|9,23|HEALER5|FOLLOW_ENEMY\|0|EVIL_RING\|EQUIPPED\|USABLE_BY_AI\|UNUSED_ITEM_DROP|
|3 /131|DARK_GUNNER|6,22|SENTRY|NONE|NOTHING|
|4 /132|CYCLOPS|10,22|SENTRY|NONE|NOTHING|
|5 /133|WHITE_DRAGON|2,29|ATTACKER1|NONE|NOTHING|
|6 /134|WHITE_DRAGON|9,28|ATTACKER1|NONE|NOTHING|
|7 /135|HELL_HOUND|7,19|ATTACKER3|FOLLOW_ENEMY\|8|NOTHING|
|8 /136|MINOTAUR|8,19|SENTRY|FOLLOW_TARGET\|0|HOLY_THUNDER\|USABLE_BY_AI\|UNUSED_ITEM_DROP|
|9 /137|DARK_GUNNER|11,18|SENTRY|FOLLOW_TARGET\|0|NOTHING|
|10 /138|DARK_GUNNER|11,15|ATTACKER1|NONE|NOTHING|
|11 /139|MINOTAUR|11,2|ATTACKER3|FOLLOW_ENEMY\|12|NOTHING|
|12 /140|CYCLOPS|12,2|ATTACKER1|NONE|NOTHING|
|13 /141|WHITE_DRAGON|17,9|ATTACKER1|NONE|NOTHING|

All selected coordinates fit23×32. That arithmetic does not prove
terrain legality or completed placement. Fresh STARTING initialization
does not run the hidden-spawn occupancy helper. The battle32/Jaro
exception is inapplicable. `randombattles.asm` lists3,8,9,10,14,17,19,
21,24,26,35; selected39 returns its definition unchanged from
UpgradeRandomBattleEnemies before level/category/RNG upgrade work.
Completion539 does not turn39 into an upgradable encounter.

`battleloop/initializecombatants.asm:InitializeEnemyStats` copies each
selected56-byte definition in14 longwords, fills currentHP/MP from
maxima, writes X/Y, movement+placement AI, kind ID, both triggers/orders,
placement item, activation, difficulty adjustment and current-stat
refresh, in that order. All fourteen selected enemies receive this copy.
Definition spellPower ENHANCED99 occupies combatant class offset10;
it is not an ally WIZ/VICR class. Initial status is NONE throughout.
Unlisted item/spell slots below are NOTHING; each has four slots.

| Kind / ID / sprite | Level,HP,MP,baseATT,DEF,AGI,MOV | Mover | Copied equipment / spells | Resistance |
| --- | --- | --- | --- | --- |
|HYDRA33 /97|39,70,0,96,62,50,4|STEALTH|none /none|ICE_WEAKNESS|
|CHAOS_WIZARD74 /138|36,53,49,47,48,59,5|MAGE|GREAT_ROD /BLAZE4|0|
|EVIL_BISHOP84 /148|38,64,51,54,48,46,5|HEALER|GREAT_ROD /AURA2,BLAST3|0|
|DARK_GUNNER65 /129|38,60,0,48,56,45,4|BRASS_GUNNER|BUSTER_SHOT /none|LIGHTNING_WEAKNESS,STATUS_MAJOR|
|CYCLOPS31 /95|36,64,0,88,51,46,5|REGULAR|none /none|0|
|WHITE_DRAGON51 /115|38,66,0,85,50,53,6|FLYING|none /SNOW1|ICE_MAJOR,FIRE_WEAKNESS|
|HELL_HOUND29 /93|35,62,0,84,49,50,6|STEALTH|none /FLAME2|ICE_WEAKNESS,FIRE_MAJOR|
|MINOTAUR30 /94|35,58,0,51,51,44,5|REGULAR|ATLAS_AXE /none|0|

Prowess `(critical,double,counter)` is Hydra `(150_1IN32,1IN8,1IN8)`,
Chaos Wizard `(150_1IN32,1IN32,1IN16)`, Evil Bishop
`(150_1IN32,1IN32,1IN32)`, Dark Gunner `(150_1IN32,1IN16,1IN32)`,
Cyclops `(150_1IN4,1IN16,1IN16)`, White Dragon
`(150_1IN8,1IN32,1IN16)`, Hell Hound `(150_1IN32,1IN16,1IN16)`,
Minotaur `(150_1IN8,1IN16,1IN16)`. unknownByte is0 for Evil Bishop,
otherwise the kind ID. AI priority is3 for Evil Bishop,2 otherwise;
BIT4 is present in Chaos Wizard/Dark Gunner/White Dragon definitions,
BIT5 in Hydra. STARTING/filler0 preserves only the upper priority nibble:
130 gets3000, the other thirteen get2000. BIT4/5 do not survive this
initializer. Activation priority and the separate AI commandset byte
do not set the currentAGI high bit.

### Placement bits, real holdings, equipment and current stats

**Confirmed:** sf2enums.asm defines EQUIPPED0080, USABLE_BY_AI2000,
UNUSED_ITEM_DROP4000, BROKEN8000, index mask007F and AddItem mask807F.
`InitializeEnemyItems` first compares the full placement word to
NOTHING007F, calls j_AddItem, then tests the **returned d1** equipped
bit. AddItem scans four two-byte slots for index127; at the first empty
slot it masks d1 with807F, writes it and returns d2=0. It strips the
equipment/AI/drop bits both in inventory and d1. It does not restore the
placement expression after returning.

| ID | Declaration word | Four stored words after selected initialization |
| --- | --- | --- |
|130|EVIL_RING25\|0080\|2000\|4000=`6099`|`00DF,0019,007F,007F` (equipped Great Rod95; unequipped Evil Ring25)|
|136|HOLY_THUNDER18\|2000\|4000=`6012`|`00A9,0012,007F,007F` (equipped Atlas Axe41; unequipped Holy Thunder18)|

All other selected holdings are their copied four-slot definitions:
Chaos Wizard uses00DF, Dark Gunners00B4, Minotaur139 uses00A9;
enemies with no item have four007F words. Remaining slots are007F.
NOTHING placement skips AddItem, retaining copied equipment.

The stripped0080 means130 never enters InitializeEnemyItems' matching-
slot/EquipItemBySlot branch. Evil Ring's +15 is **not applied**; status
remains0. The selected path does not test its equip class. If equipped
through another path, EquipItemBySlot masks index, tests class+1 against
the32-bit item equip flags using the register-count LSR, and refreshes
stats even on refusal. Native class99 gives count100 mod64=36, shifting
all32 bits out and failing eligibility. A changed matching WIZ/SORC/VICR
class can return2 for a cursed ring and set0080. Do not substitute that
counterfactual for selected class99 or its already-stripped bit.

Copied definition equipment is different: UpdateCombatantStats applies
each already-equipped non-NOTHING word without rerunning class eligibility.
It initializes current stats, applies preserved status effects, scans all
four item words and reconstructs curse. ApplyItemOnStats clears the curse
test for enemies, so even a supplied equipped Evil Ring on an enemy adds
ATT15 without introducing CURSE through this helper; allied cursed
equipment can introduce it. No selected ring curse/AI use is established.

| Item / ID | Range / price | Equip classes | Type / use | Equip effect (then two NONE0 pairs) |
| --- | --- | --- | --- | --- |
|Evil Ring25|0–0 /5000|WIZ,SORC,VICR|RING,RARE,CURSED,BREAKABLE /BOLT2|ATT+15|
|Holy Thunder18|0–1 /1500|NONE|RARE,CONSUMABLE /THUNDR1|NONE0|
|Great Rod95|1–1 /7900|WIZ,SORC,VICR|WEAPON,RARE /NOTHING|ATT+28|
|Buster Shot52|2–3 /6800|SNIP,BRGN,BWNT|WEAPON,RARE /NOTHING|ATT+37|
|Atlas Axe41|1–1 /9600|GLDT,BRN,RDBN|WEAPON,RARE,BREAKABLE /BLAZE3|ATT+35|

A changed prefilled four-slot inventory makes AddItem return d2=1
without masking d1; InitializeEnemyItems does not test that result
before its equip-bit/matching-byte scan. Thus that supplied full-ring
case can search an existing low-byte match and attempt equipment; it
does not add a fifth item or guarantee success. A supplied BROKEN bit
survives807F. These boundary cases explain the actual helper, not fresh
selected39 inventories, which each have three empty slots after copy.

Difficulty applies floor(baseATT×5/4) once; SUPER applies the same
truncating multiplication again. Copied equipped ATT is added afterward.

| Kind | Current ATT ordinary / SUPER | Current raw AGI |
| --- | --- | --- |
|HYDRA|120 /150|50|
|CHAOS_WIZARD|86 /100|59|
|EVIL_BISHOP|95 /111|46|
|DARK_GUNNER|97 /112|45|
|CYCLOPS|110 /137|46|
|WHITE_DRAGON|106 /132|53|
|HELL_HOUND|105 /131|50|
|MINOTAUR|98 /113|44|

No selected effect changes AGI/DEF/MOV/resistance/prowess from the shown
definition values. CurrentHP/MP are maxima, status0. The held ring is
not110/126 ATT; Hydra's prowess double chance is not a second queue
entry. Item getters do not recreate2000/4000. Actual AI item selection
and separate conditional drop records/actions remain outside this start
boundary; holding an item is neither proof of AI use nor a victory award.

### Allies, absent enemies and battle entities

Reuse the accepted all30-ally heal/formation mechanism: living allies
and immortal Peter7/Lemon28 heal regardless of membership; ordinary HP0
allies remain dead/unrefreshed. Retained STUN/POISON/CURSE is passed to
refresh, which reconstructs curse from equipment; STUN reduces MOV1/
AGI5 with saturation. All30 X bytes reset−1. UpdateForce rebuilds lists;
at most the first twelve active entries in ID order are examined. Dead
members consume an outer iteration without advancing formation; excess
entries LeaveBattleParty without a second rebuild. Empty membership has
the existing N−1 underflow boundary. No rule fixes Bowie at row0 merely
because its scene mainEntity was Bowie.

Absent142–159 fail the14-enemy count check before a0 is advanced:
activation0000, max/currentHP0, X−1, triggers15/15 read from first Hydra
row. Y, kind, class, items, spells, mover/AI and orders can remain stale.
There are no selected HIDDEN/RESPAWN rows. Changing an absent HP/X alone
does not install a definition.142–157 can pass the turn scan's gates
under changed state;158/159 are never offered to that scan.

LoadBattle calls PositionBattleEntities, replacing scene population and
aliases. Packing tests signed X/Y and enemy-neutral bit, **not HP**,
masks3F, scales384 and installs Down/eas_Standing. With A eligible allies,
fourteen unchanged enemies pack into physical A..A+13; A≤12 gives a
maximum physical25. Absent142–159 mapFF through X−1. There is no39
neutral-position override.30/31 remain fresh zero aliases; scene
followers/Zynk/Lemon are not preserved by their earlier positions.
Combat128 is a newly copied Hydra33, not the old speed24/Up scene128.

LoadBattle uses combatant0's actual alias (negative fallback physical0)
for chosen coordinates, then requests map/sprites/terrain/music/fade
services and returns. `terrainentries.asm` slot39 is BattleTerrain39;
custom background is OVERWORLD and leader-presence byte−1. This names
the selected dependencies without decoding terrain/materials or entering
the later leader-defeat/outcome behavior. Working crop17,20 remains
23×32; stored combat coordinates are not shifted by17/20. For example,
combat8,24 corresponds to source-map25,44, while the scene8,24 is its
own working-coordinate request. No natural spatial equality follows.

## Zero regions, unchanged activation and no normal spawn list

**Confirmed:** fresh entry clears F90–105, but neither ClearAiMemory
nor GenerateBattleTurnOrder clears NEWLY_TRIGGERED_BATTLE_REGIONS.w
at `$FFB20C`. `battleloop/activateenemies.asm` scans128–159, rejecting
signed-negative X or zero HP, then falls through once for raw160.
For a passing candidate, `triggerregions.asm:TriggerRegionsAndActivateEnemies`
obtains subsection region count0 and branches straight to @Done.
It never calls IsRegionTriggered/AreAnyAlliesInTriangle, tests/sets
region flags or the checked word, reads assigned triggers, or writes
activation. A supplied retained checkedA55A and held F90 therefore stay
unchanged at this zero-count seam. No geometric case is needed.

All fourteen selected15/15 words remain2000 except130=3000; they do
not acquire primary-active bit0 (mask0001) simply because all enemies can act.
Even a supplied changed non15 trigger/held flag cannot activate through
this function while the region count stays0. Ordinary AI's separate
15/15 bypass admits the unchanged rows later. Non15 with primary bit0
clear instead reaches standby; a retained secondary bit alone is not
the required primary-active bit.

The fall-through raw160 is still a credible read seam: GetCombatantEntryAddress
admits160 and aliases CURRENT_GOLD at `$FFF600`; pseudo-X=`$FFF62E`
(Deals84/85), pseudo-HP=`$FFF60E` (20–23), pseudo-trigger=`$FFF636`
(100/101), pseudo-activation=`$FFF634` (96–99). It first needs a
nonnegative pseudo-X and nonzero pseudo-HP. **Selected39 then returns
on count0 before trigger/activation access**, even with held flags;
the prior nonzero-region Goddess Staff mutation is not selected here.
With a different nonzero-region selector the accepted conditional
[raw160 mechanism](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
still applies. This is not a valid extra combatant or item delivery.

ExecuteBattleRegionCutscene scans32/0/F386,32/1/F386,40/0/F387,
40/1/F388 then terminatorFFFF. Live39 misses before region/played
checks: no selected program. Debug PrintAllActivatedDefCons requests
none after unchanged fresh clearing; supplied held flags can still
produce diagnostic463, independent of zero-count activation.

PopulateTargetsListWithSpawningEnemies scans exactly128–159, masks
initialization bits and writes rebuilt TARGETS_LIST_LENGTH.w. All
selected/absent initialization modes are0, so unchanged list length0
and no SpawnEnemyWithCamera/reset/occupancy/appearance/RNG call follows.
Retained target-list payload bytes need not be cleared to get length0.
Supplied nonzero initialization bits on stale/changed rows are different
inputs; no normal hidden list is inferred from definition BIT5, which
was already discarded. Raw160 is not in this scan.

## Current AGI, sequential RNG and first queued actor

**Confirmed:** @Start orders activation, region program, optional debug,
spawn list/each appearance, then GenerateBattleTurnOrder. With unchanged39
there is no spawn appearance. The generator (`$25544..$25610`) clears
64 two-byte records at `$FFF71A` toFFFF; it offers allies0–29 then
enemies128–157 ascending. Signed-negative X or HP.w=0 rejects. Y,
party membership, neutrality, active bit, sleep/stun and signed-positive
HP are not generation gates. CurrentAGI is read at each offered actor.
HP8000 and Y−1 alone pass; rejected candidates consume no draws.

For A=currentAGI&127, first score is
`A+R(A>>3)−R(A>>3)+R(3)−1`, where each R is the next direct
GenerateRandomNumber call. RawAGI≥128 appends a second record immediately:
B=floor(A×5/6), score=`B+R(B>>3)−R(B>>3)`. Store the low score byte
after word arithmetic. For these small ranges the direct generator is
`seed=(seed×13+7)&65535`, return floor(seed×range/65536); range0 still
updates. It bypasses direction/debug overrides and the thinking RNG copy.

All fourteen selected raw AGIs are below128, so enemies alone yield
14 records/42 sequential draws. With N eligible normally placed allies
and T high-bit allies, totals are `14+N+T` records and `42+3N+2T`
draws; at N=T=12,38 records/102 draws. For arbitrary changed candidates
C/high-bit count D, use C+D and3C+2D. Definition prowess, priority and
LEADER commandset do not change those formulas. Status/equipment refresh
can change current numerical AGI or its high-bit coupling before a later
generation; no immutable definition snapshot is assumed.

| Kind / selected IDs | Current raw AGI | First draw range | Conservative score envelope |
| --- | --- | --- | --- |
|HYDRA /128|50|6|44..56|
|CHAOS_WIZARD /129|59|7|52..66|
|EVIL_BISHOP /130; CYCLOPS /132,140|46|5|41..51|
|DARK_GUNNER /131,137,138|45|5|40..50|
|WHITE_DRAGON /133,134,141|53|6|47..59|
|HELL_HOUND /135|50|6|44..56|
|MINOTAUR /136,139|44|5|39..49|

The envelopes are algebraic bounds, not independent attainable extrema
or probabilities. Supplied incoming seed/state examples:

| Generation seam | Appended or first sorted actor:score | Seed after |
| --- | --- | --- |
|Unchanged fourteen enemies, seed0|first129:59,14 records/42 draws|7850|
|Same, seed1|first129:57,14/42|49395|
|Same, seed65535|first129:59,14/42|31841|
|Ally0 raw80 before all enemies, seed0|first0:79,15/45|11763|
|Hydra alone/raw50, seed0|128:49|1281|
|Hydra alone/raw178, seed0|128:49;128:41 (five draws)|19979|
|Hydra HP0 or XFF, seed0|none/no draws|0|
|Hydra YFF/HP8000/raw50, seed0|128:49|1281|
|Ally0 raw52 then Hydra raw50, seed0|0:51 then128:51, tie order retained|63126|
|Ally0 alone/raw0, seed0|0:255 (word−1)|1281|

These are generation seams, not seeds preserved across preceding
services or natural first-control observations. The preceding ally
changes later enemies' draws. A LEADER14 label does not fix the first
actor, and the enemy-only supplied examples do not include a living
party or prove an actual battle entry.

Sorting uses62 passes of63 adjacent **signed score-byte** comparisons,
swapping only when right>left. Equal scores retain append order: allies
before enemies, lower IDs first, an immediate first/second entry keeps
its order on ties. FFFF padding score−1 can precede wrapped negative
scores; actor0/score255 is not actorFF. Native writes have no capacity
guard; arbitrary30+30 high-bit candidates can exceed64 records.62 passes
are not proof of a fully sorted arbitrary64-entry buffer. The bounded
unchanged38-record maximum avoids that overflow claim.

Generation clears CURRENT_BATTLE_TURN.b at `$FFF79A`, after the128-byte
buffer. Fresh dispatch reads actor byte at offset0. ActorFF repeats
@Start. Non-FF calls ExecuteIndividualTurn, which rereads current HP;
entries are opportunities, not reserved completed actions. The cursor
advances by2 only after returning through the downstream terminal/death/
after-turn interfaces. Those bodies and whole rounds remain outside.

## First living/control and ordinary command entrance

**Confirmed:** ExecuteIndividualTurn (`$23EB0..$24242`) sets the
battlefield frame counter, clears speech SFX, allocates its frame,
masks actor and clears the dead-list length.39 misses the Taros/Bowie
F112 branch. HP0 returns @Done without advancing the cursor itself;
the caller's later checks precede its increment. Nonzero HP publishes
actor/target X/Y, clears cursor radius, resolves alias/view target and
requests the cursor destination. It does not repair position/alias state.

MUDDLE30 or AI_CONTROLLED0004 first chooses StartAiControl. Otherwise
an enemy uses AI with CONTROL_OPPONENT_TOGGLE=0, player path with nonzero;
an ally uses AI when AUTO_BATTLE_TOGGLE is nonzero. AI preparation returns
to the same @PlayerControl setup: cursor/view waits, targeting reset,
moving-entity ID, mini-status/land-effect windows, movement range/blocks
and hidden cursor. That label does not prove human control. Only then
SLEEP C0 and STUN bit0 select @NoAction. Status/activation/toggles are
read again before ProcessBattleEntityControlPlayerInput or ExecuteAiControl;
intervening changes can alter dispatch. Stop at these interfaces, before
input/action execution and no-action cleanup. Required-return waits and
windows do not prove completed services or usable input.

`ai/startaicontrol.asm:StartAiControl` (`$DEFC..$E1AC`) sends ally AI
directly to ATTACKER1, bypassing the enemy checked reset. Selected enemy
sets are not SWARM15: @NonSwarmAi clears the checked word, reads
trigger nibbles, and15/15 reaches activated handling without consulting
active bits. A changed non15 row needs primary-active bit0; otherwise
standby/STAY. This later reset, not count0 activation, explains why a
retained checked word can become0 before the first AI command.

Special-kind comparisons are PRISM_FLOWER93, ZEON_GUARD38 and
BURST_ROCK32. Hydra33 misses all three and continues ordinary handling;
the adjacent enum and BIT5 do not select ProcessExploderAi. All selected
kinds use the ordinary path here.

Before pathfinding, non-NONE/nonpoint primary orders read the full
order as a combatant ID and test its live HP. Zero HP promotes secondary
to primary and sets secondaryFF, without chaining another promotion in
that invocation. No position/party/region test replaces the HP check.

| Selected primary byte | Actual target / selected callers | HP0 promotion |
| --- | --- | --- |
|MOVE_TO0=`40`|point0=8,24, Hydra128|No followed-HP check.|
|FOLLOW_ENEMY0=`80`|combat128 Hydra,129/130|NONE/FF becomes primary.|
|FOLLOW_ENEMY8=`88`|combat136 Minotaur,135|NONE/FF becomes primary.|
|FOLLOW_ENEMY12=`8C`|combat140 Cyclops,139|NONE/FF becomes primary.|
|FOLLOW_TARGET0=`00`|combat0 Bowie,136/137|NONE/FF becomes primary.|
|NONE=`FF`|131/132/133/134/138/140/141|No follow lookup.|

These are combatants, not physical/scene aliases. A living Bowie at X−1
still passes this HP precheck; later order commands have separate
dependencies. SENTRY's promotion can occur even though its command
list contains no MOVE_ORDER command. The prepared list comes from the
current dedicated commandset byte, not the activation priority/filler.

| Set / number | Pathfinding mode | Ordered command list |
| --- | --- | --- |
|LEADER14 (pointer aliases CRITICAL13)|BLOCK_AND_CARVE2|MOVE_ORDER5,ATTACK2,HEAL3,SUPPORT,MOVE_ORDER2,MOVE3,STAY|
|HEALER5 /4|BLOCK_AND_CARVE2|HEAL3,MOVE_ORDER5,SUPPORT,ATTACK2,MOVE3,STAY|
|ATTACKER1 /6|REGULAR0|ATTACK1,HEAL1,SUPPORT,MOVE1,STAY|
|ATTACKER3 /9|BLOCK_NON_MOVABLE1|MOVE_ORDER3,ATTACK3,HEAL2,SUPPORT,MOVE_ORDER4,MOVE2,STAY|
|SENTRY /8|REGULAR0|ATTACK1,HEAL1,SUPPORT,STAY|

`data/battles/global/aicommandsets.asm` owns pointers/count-prefixed
lists/modes. Preparation calls BlockNonMovableSpacesAroundDestination
or BlockAndCarveAroundDestination for nonzero modes. The list dispatcher
passes each byte to ExecuteAiCommand (`ai/executeaicommand.asm`,
`$E294..$E3EE`), stops on returned d1.b=0 and continues on nonzero.
MOVE_ORDER5 uses point target/mode2, MOVE_ORDER2 follow2/mode2,
MOVE_ORDER3 follow1/mode1 and MOVE_ORDER4 point/mode1. These interfaces
name possible work; no chosen attack/heal/support/item/spell/path is
asserted. In particular Hydra has no spell or item, while HEALER5's
list does not establish Evil Ring use or a first AURA cast.

## Existing lowering and actual production consumers

**Confirmed about the tool:** the sole selected lowering completed with
error=None in0.1368s. Full programs are bbcs_39:13 instructions,
cs_4E40C:3, cs_4E41A:58; named actions eas_Init:14 and eas_Jump:9.
It retains the five scene records, nineteen ordinary follower rows,
both branches, seven text requests, four move payloads and the speed24
custom stream. cs_4E41A instruction1 is native-call cameraSpeed;
there are no action natives. Complete JSON and summary are retained,
not substituted/retried or executed. If prior map/population/services
succeed, that instruction is an unbound runner boundary before the
dialogue. Error=None means lowering completed, not a runnable opening.

**Confirmed about accepted code:** production consumes prepared Content.
The [capability owner](../../remake/docs/capability-status.md) defines
supported playable scope. Direct source inspection gives these selected
boundaries, without an SDK/adapter launch or speculative error ordering:

| Actual consumer | Represented or consumed behavior / selected39 gap |
| --- | --- |
|[prepare_encounter](../../src/sf2tool/remake_exploration_battles.py), [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Shapes include12 formations/14 placements/zero regions/one point and23×32 crop. Preparation admits2/3; pinned reader admits1/2/3 and rejects39 with “The selected encounter is not admitted.” Coordinates fitting the rectangle establish neither terrain nor content/material binding.|
|[PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|region-program-table allows1/2/3 only. STARTING shape is compatible; selected Evil Ring/Holy Thunder expressions refuse placement-item-refresh. HEALER5/SENTRY refuse source-commandset; ENHANCED refuses enemy-derived-state; selected BIT4 refuses source-ai-word. REGULAR/HEALER/CENTAUR/HOVERING/STEALTH/FLYING have Mover cases; BRASS_GUNNER (Dark Gunner131/137/138) and MAGE refuse source-mover. Order parsing represents40/80/88/8C/00/FF but does not itself execute them.|
|[BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [BattleTerrainRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTerrainRules.cs)|InitialActivationWord preserves upper priority and mode/filler; source loadout has four slots. Controlled roster/already-refreshed/difficulty0 policies do not reproduce all30 native slots or general ally refresh. Enemy items other than0/127 refuse enemy-equipment-refresh, including copied rods/axes/guns and the held ring/thunder. Stealth terrain costs exist; they do not supply selected39 terrain.|
|[BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs)|BeforeRound explicitly returns unchanged on Regions.Count0 after validation.15/15 rows need no region IDs; missing non15 region refuses missing-activation-region. Fresh managed initialization clears region state; native checked word can be retained until enemy AI. No raw160/Deals alias exists in the validated actor roster.|
|[BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs), [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [BattleRandom](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleRandom.cs)|Sequential draws, masked numerical basis plus ExtraRoundAction,64 slots, signed62×63 sort, sentinel and dead skip are represented. Flow reads current actor.Agility and definition.ExtraRoundAction; arbitrary native raw-AGI refresh coupling is unproved. Managed capacity/placement/resource guards and entry cursor differ from unchecked native memory. No actual39 seed/queue/cursor is bound.|
|[RuleCompositions](../../remake/src/Sf2.Remake.Application/Gameplay/RuleCompositions.cs), [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs)|source-orders binds actual decision rules. Enemy checked reset/15-bypass/primary gate and dead-follow promotion exist. Commandset guard permits6/7/9/14, rejects4/8. Hydra's empty loadout could pass source-action-categories in a suitably bound authored state; equipped items or spells refuse it for other selected rows. Follow00/80/88/8C needs explicit SourceCombatants bindings or source-follow-binding. MOVE_ORDER/mode support is restricted and no selected39 decision is run. Absence of special-kind32 dispatch is not a Hydra33 exploder gap.|
|[BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs), [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs)|Before/load/start continuation and generation/control observations exist. F88 refuses suspended-battle-entry. Whole initialization validates before publishing; native interrupted prefixes are not transactional. Managed outcome check precedes generation, and sleeping-action-skipped precedes AI preparation, unlike native order. Controlled player admission also bounds X/Y and policies/toggles; no complete native mutable dispatch is claimed.|
|[ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Native-call deserializes as UnsupportedInstruction; ProgramRunner throws program-opcode for cameraSpeed. Typed action flags/speed/waits/moves/sprite refresh have consumers; no action-native refusal was observed here. SceneEntities uses supplied ordinary followers and guarded49-slot allocation; the lowering omits overworld Caravan65. Alias/class/HP-skip/crop services require their actual bindings. Relative motion rejects out-of-range destinations with entity-motion-boundary before starting; it does not clamp them.|
|[BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs), [ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Generic board/cursor/entities/camera/text/audio projections exist. They do not consume the unbound cameraSpeed instruction or establish39 map/materials, missing portraits/default voice, VInt/DMA/timing, service completion, usable input or save persistence.|

The separate leader-defeat-program restriction tests dead actors'
Physical.Leader; it is not a refusal of LEADER14. This reader sets enemy
physical leader:false. No missing consumer, import admission or engine
behavior is repaired in this documentation slice. Shape, admission,
execution and original-game evidence remain separate judgments.

## Reproduction, preserved evidence and open boundary

The ignored selected review checks the clean pinned source, table slots,
all37 commands/inline operands/48-byte scene table, seven complete token
sequences, all12/14/0/1 declarations, definitions/items/current-stat
derivations, zero-region early return and unchanged-state variants,
absent/raw160 gates, sequential RNG/queue deltas, control/order/mode
interfaces and actual consumers. It reads the sole lowering result;
it never invokes the compiler again. Its direct document checks cover
exact two-path ownership, navigation-only predecessor, UTF8/LF, links,
fences, dates and private/text publication boundary.

After current ignored configuration in the same PowerShell process,
use unused receipt names:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1152/review.py <configured-source-disasm-root> --receipt local/issue1152/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The source root contains sf2enums.asm, code/ and data/. Applicable
ADR0004 provenance/selector/width/branch/order/date/consumer/adversarial
checks apply; maintained parser/schema/fixture/test/H3 changes are N/A.
The committed planner reports selection, not permission to enlarge this
documentation scope. Normal verify is **SKIPPED** under the explicit
Issue boundary, not PASS; full/old comparisons and SDK/runtime remain
excluded. Retain actual exact-head scoped CI and independent main-gate
source/consumer/publication review before integration.

Preserve #1150's original direct/CI record with its P2 wording defect,
the bfc29ef focused correction and independent correction acceptance;
out-of-range movement throws rather than clamps. Preserve its eleven
aftermath native gaps, #1144's sole opening lowering, earlier direct/
main-gate receipts, Map53 import UnboundLocalError/zero partial programs,
twelve completed old comparison failures, Battle37 y64 refusal, Map66
reload/cross-map-scroll and existing consumer Unknowns. No prior failure
is relabeled interrupted, erased, repaired or rerun automatically.

**Inferred:** this source structure supplies a coherent future content
contract with ordinary Hydra AI and no selected hidden lifecycle. It
does not establish that the current remake can play39 or that a natural
party/seed reaches any particular first actor.

**Unknown:** natural entry/opening/control reach; live all30 ally and32
enemy slots, HP/status/class/equipment/positions/aliases/membership,
flags/checked/Deals and F88 caller182/queue/cursor/RNG; source/ROM equality
beyond accepted evidence; decoded terrain/collision/materials; actual
jump/follower interleaving and interrupted prefixes; completed map,
sprite/view/cursor/window/text/voice/audio/DMA/VInt services, hardware
timing, usable input and persistence. Individual AI command choices,
movement/actions/damage/item use/conditional drops/outcomes/next map
remain outside this slice. These are not passed gates or an automatic
runtime acquisition queue.
