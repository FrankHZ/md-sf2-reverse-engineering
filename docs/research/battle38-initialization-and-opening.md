# Battle38 initialization and Geshp opening

Evidence date: 2026-10-21. **Confirmed (static):** selected Map66/Battle38
stages a separate Geshp scene, then reconstructs the party and eleven
STARTING enemies. Nine HIDDEN Burst Rocks have HP0/X−1 before activation;
their declared45 HP is not yet installed. Intro488 can skip both hooks
without skipping fresh initialization. Held88 instead bypasses that prefix
and uses incoming66 to read beyond the64-byte alias table. Stop before
ActivateEnemies; the retained path stops at the individual-turn entrance.
Natural reach, completed services, presentation, control and persistence
remain **Unknown**. Successful offline lowering is not playable38 acceptance.

## Agreement, evidence and budget

Standard rigor for [Issue #1144](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1144)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
continue accepted [Map66/Battle38 admission](./granseal-overworld-return-and-battle38-admission.md)
at base `a270ca71f3ef058122c0355ebd7a14c406d68803` through the complete
opening and fresh initialization, for later content work. Exact ownership
is this new owner and navigation only in the admission owner. No shared
index/registry, parser, schema, fixture, tool, design or engine changes;
no competing writer or stacked dependency. Region evaluation, spawning,
turn generation, Burst Rock actions/explosion and outcomes remain later
scopes. No runtime acquisition or neighbouring story expansion.

Reuse [Battle37 initialization](./battle37-initialization-and-opening.md),
[Battle36 initialization](./battle36-initialization-and-opening.md),
[spritesets](./battle-spriteset-data.md), [global battle data](./battle-global-data.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[scripting](./common-scripting.md), [maps](./common-maps.md) and
[stats](./common-stats.md). Those owners retain common mechanisms;
the selected38 operands, changed valid states and actual consumers are
checked here. Scene/formation declarations are not an immutable party snapshot.

Original paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`map/` and `entity/` abbreviate `code/common/scripting/` children;
`battle/` means `code/gameflow/battle/`. sf2const.asm, sf2enums.asm,
sf2macros.asm and sf2cutscenemacros.asm own layouts/constants/encoding.
Fnnn means flag nnn, ordinals are zero-based, address intervals half-open.
The accepted USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new source/ROM equality or original-runtime observation is claimed.

Minimum evidence is selected source/tables/helpers, actual consumers and
the existing immutable18,285,979-byte init/script pair with
`local/issue1005/pair.txt` provenance. Linear selected reads and one pair
load per command target one minute/512MiB, peak memory unmeasured.
Unique small receipts stay in ignored `local/issue1144/`. One existing
opening lowering resolves typed versus native instructions; it imports
no map/definitions and executes no scene. No acquisition, full-map work,
new verification platform/helper tests, normal/full/old comparison suites,
SDK/Godot/emulator/H3/H4 or screenshots. Reassess a changed prerequisite
or material contradiction before expanding the work.

## Selected caller, fresh prefix and hook latches

**Confirmed:** the predecessor owns CheckBattle's ordered admission.
Row38 at `$7B40..$7B47` is `66,17,0,32,32,255,255`. Only a matching
map/unlock row selected ahead of later rows returns38; older Map66
unlocks can win first. F538 can clear438 while still returning38 on
that scan. The crop is output, not a trigger-containment test. MainLoop
copies returned d7.w to d1.w and calls j_BattleLoop with resolved map66
in d0.w; caller high bytes remain explicit inputs.

`battle/battleloop_1.asm:BattleLoop` clears PLAYER_TYPE.b, sets399 and
tests88. Clear88 clears seconds, saves d0/d1 **words**, installs their
low bytes in CURRENT_MAP/BATTLE, calls SetBaseVIntFunctions and before.
After before it restores those words and rewrites both bytes. It does
not undo reached flags, entities, layout, crop or party effects. It clears
90–105 inclusive, heals living/immortal allies, initializes all ally
positions, all enemy positions and AI memory in that order. Then d0.w=0
selects combatant0 for LoadBattle, followed by the start wrapper. The
next @Start instruction is the excluded first ActivateEnemies call.

| Complete48-word table / selected slot38, byte76 | Target |
| --- | --- |
|`data/battles/cutscenes/beforebattlecutscenes.asm`, base `$47A88`, entry `$47AD4`|bbcs_38 `$4DDA4`|
|`data/battles/cutscenes/battlestartcutscenes.asm`, base `$47B2C`, entry `$47B78`|ms_Empty, sole FFFF word|

`battle/cutscenes/beforebattlecutscenesstart.asm` tests
450+CURRENT_BATTLE=488, skips when held and never sets it.
`battlestartcutscenesstart.asm` independently tests488 and sets it
**before** dispatching the empty start. Empty ExecuteMapScript still
resets dword_FFB1A4 to FF9004_LOADING_SPACE, clears text skip, conditionally
waits for view with an open dialogue window and clears view speed on
return. Skipping a hook bypasses those effects.

| State at the named reads | Boundary |
| --- | --- |
|88 clear,488 clear throughout|Complete before; fresh reconstruction/load; set488 then empty start.|
|88 clear,488 held throughout|Both bodies skip; fresh reconstruction/load still run.|
|538 held while438 admits|Same path;538 and488 are independent.38 remains outside random-upgrade membership.|
|488 changes between wrappers|Each wrapper follows its own read; no immutable shared latch snapshot.|
|Selectors change within services|Helpers reread live selectors; saved-word restoration repairs only map/battle bytes at its named point.|
|Interrupted prefix|Reached mutations can remain; before may have run while488 is clear, or488 may be set before start completes. No save/control guarantee.|

### F88 incoming66: exact out-of-table read

Held88 restores saved seconds, clears88, clears AI memory and
VIEW_TARGET_ENTITY.b, then calls LoadBattle **without clearing d0**.
It branches to @IndividualTurns_Loop; the following branch to @Start is
unreachable. It bypasses current-byte assignment, hooks, region reset,
healing and combatant position/stat initialization. Retained current map,
battle, crop, combatants, activation and turn buffer remain inputs;
incoming66 does not install those selectors.

`battle/battlefunctions/loadBattle.asm` saves d0.w across fade/tilesets/
PositionBattleEntities. GetEntityIndexForCombatant in battlefunctions_0.asm
tests d0.b:66 is nonnegative, so it does not subtract enemy96. The FF
word mask yields offset66 even with nonzero incoming high byte. The read
is `ENTITY_INDEX_LIST($FFB140)+66=$FFB182`, two bytes beyond the end of
`$FFB140..$FFB180`. sf2const.asm names CHEST_CONTENTS at180, with observed
word consumers covering180..182, and BATTLESCENE_MUSIC_INDEX at188;
it assigns no symbol/size ownership to182. It is **not** the chest word,
enemy162, Bowie0, or Battle37's TEMP_ITEM_OR_SPELL low byte at18D.
Selected code has no supported alias ownership or initialization for182;
its value and any intervening service writes remain **Unknown**.

ClearEntities clears64 alias bytes only; PositionBattleEntities rebuilds
ally0–29/enemy32–63 aliases and packs retained eligible combatants, without
repairing offset66. LoadBattle stores fetched byte T as view target.
Negative T uses physical0 for coordinate reads; otherwise T&3F has no
49-record guard. FF/80→0,00→0,30→48,31→49,7F→63 are source arithmetic
examples, not observed bytes. LoadMap later also consumes view-target
state. No safe or valid suspended restoration is established.

ClearAiMemory resets48 last-target bytes toFF and48 memory bytes to0
(`$FFB1AC..$FFB1DC`, `$FFB1DC..$FFB20C`), not the newly-triggered queue
word `$FFB20C..$FFB20E`, region flags or turn buffer. F88 bypasses the
script scratch-pointer reset too. Stop before reading/acting on the
retained individual-turn entry.

## Scene identities, full opening and services

**Confirmed:** `data/battles/entries/battle38/cs_beforebattle.asm`
spans `$4DDA4..$4DE8C`. bbcs_38 has38 top-level commands/188 encoded
bytes plus five interleaved move/end pairs occupying20 bytes, ending at
ce_4DE74. Its24-byte entity table has a six-byte main header,
two eight-byte fixed records and a two-byte terminator. cs_4DDFE is the
interior branch destination after the conditional Zynk block.

| Logical identity | Scene world X,Y / facing | Sprite/action |
| --- | --- | --- |
|main/Bowie0|14,9 /Down|actual ally0/eas_Idle|
|fixed Peter7|13,8 /Down|ALLY_PETER/eas_Init|
|fixed128|13,23 /Up|GESHP159/eas_Init|

loadMapFadeIn66,9,6 enters csc37→csc48: requests out-to-black and
loads map/view services; the separate fadeInB follows later. It does
not call exploration setup init. During this fresh before CURRENT_BATTLE
is already38, so map loading also uses the then-live battle crop/dimensions;
the scene camera operands do not redefine that crop. csc42 invokes
InitializeMapEntities/LoadEntityMapsprites with display/VInt control,
replacing population. ClearEntities resets49 physical records,64 aliases
to0 and walking scratch. Followers allocate before fixed records.

Map66 is explicitly in `data/maps/global/overworldmaps.asm`. Held65
selects table_OverworldFollowers, adding the Caravan65/FOLLOWER_A30
prefix before all19 ordinary rows; clear65 selects the19 only. Complete
ordered rows remain in `data/scripting/entity/followers.asm`. Every held
row allocates, including repeated aliases whose last write wins.
FOLLOWER_A can be Caravan65, King74, Old Man77, then King83;
FOLLOWER_B31 can be Astral74, Oddler68, then Astral70. No producer leaves
fresh alias0, so the reference addresses Bowie. Follower flags do not
require active/joined force membership. F67 Peter can allocate before
the fixed7 record, which then reuses his position/action until explicit
commands. F76 allocates Zynk26 without joining/activating him.

With n allocated followers and p=1 when fixed Peter allocates, Bowie
is physical0 and scene Geshp128 is n+p+1. Follower-script pointer tables
have five entries; arbitrary flag combinations/capacity are not proved
safe. Ally sprites depend on live class/Rohde membership; the outside-
battle HP0 flame branch does not apply while CURRENT_BATTLE=38.

| Index | Complete top-level order (inline payload shown with its owner) |
| --- | --- |
|0|textCursor3003|
|1|loadMapFadeIn66,9,6|
|2|loadMapEntities ce_4DE74|
|3|setActscriptWait Bowie0,eas_Init|
|4|setActscriptWait Peter7,eas_Init|
|5|setPos Peter7,13,8,Down|
|6|setActscriptWait FOLLOWER_B31,eas_Init|
|7|setPos FOLLOWER_B31,15,8,Down|
|8|setActscriptWait FOLLOWER_A30,eas_Init|
|9|setPos FOLLOWER_A30,14,8,Down|
|10|jumpIfFlagClear76,cs_4DDFE (index13)|
|11|setActscriptWait Zynk26,eas_Init|
|12|setPos Zynk26,63,63,Down|
|13|cs_4DDFE: playSound MUSIC_BATTLE_THEME_1|
|14|fadeInB|
|15|cameraSpeed$30|
|16|entityActions Bowie0: moveDown2,endActions|
|17|entityActions Peter7: moveDown2,endActions|
|18|entityActions FOLLOWER_A30: moveDown2,endActions|
|19|entityActionsWait FOLLOWER_B31: moveDown2,endActions|
|20|nextSingleText00,128 →3003|
|21|setCamDest9,19|
|22|nextText00,128 →3004|
|23|nextSingleText00,128 →3005|
|24|entityActionsWait128: moveUp2,endActions|
|25|nextSingleText00,128 →3006|
|26|nextTextC0,FOLLOWER_B31 →3007|
|27|nextSingleTextC0,FOLLOWER_B31 →3008|
|28|nextText00,128 →3009|
|29|nextText00,128 →3010|
|30|nextSingleText00,128 →3011|
|31|setQuake4|
|32|playSound SFX_BOLT_SPELL|
|33|flashScreenWhite30|
|34|playSound SFX_BATTLEFIELD_DEATH|
|35|flashScreenWhite50|
|36|setQuake0|
|37|csc_end|

### HP, action waits, camera and effects

Reuse the [accepted action/HP/alias semantics](./battle36-initialization-and-opening.md#action-hp-alias-and-wait-semantics).
eas_Init clears flagsA bits5/6, **disabling entity/map collision checks**
at the actual consumers; it sets acceleration0, speed32/32, obstruction/
auto-facing, Up, nonimmersed, size24, refreshes and jumps idle.
SetActscriptWait has no ally-HP guard. setPos's
AdjustScriptPointerByCharacterAliveStatus skips its four-byte payload for
HP0 logical allies0–29;30/31 and128+ bypass that test. Dead Peter gets
Init but skips13,8; held76/HP0 Zynk gets Init but skips63,63; clear76
skips both commands. Later fresh immortal healing of Peter is separate.
Both30 and31 explicitly receive Init here. Missing aliases can therefore
reset/reposition Bowie even when his HP0 would have blocked logical0.

csc2D resolves the actual slot, installs its wait timer, clears collision
bits and generates a relative-motion action ending in eas_Idle.
moveDown2/Up2 encode signed relative0,+2/−2; no HP guard is applied.
The first three movements do not wait; the fourth waits **only31's**
resolved pointer to reach idle. It is not an aggregate four-actor barrier.
Aliased slots can replace each other's scripts. Under distinct aliases,
living Peter and completed motion, nominal scene destinations are
Bowie14,11, Peter13,10,30→14,10,31→15,10, then Geshp13,21.
These are conditional scene coordinates, not a runtime observation.

The later combat formation is crop-relative. Its first living row13,11
would be world30,11 under crop17,0; combat Geshp7,27 would be world24,27.
Neither equals scene Bowie's14,11 or Geshp13,21. Scene128 is replaced by
combat128, with the same Geshp sprite159 but a newly built identity/state.
No scene command awards HP/items, recruits an ally or installs an enemy
definition. Zynk63,63 is storage, not a battle placement or deletion.

cameraSpeed writes word48 to VIEW_SCROLLING_SPEED. setCamDest9,19
detaches the view target, requests SetCameraDestination then waits for
view-scroll end; it changes neither combat crop nor entity speed.
setQuake4/0 lacks4000/8000 ramp bits and directly writes QUAKE_AMPLITUDE.w.
The flash handler csc41 takes operand>>3 as DBF counter:30 requests four
LaunchFading calls,50 requests seven, then DuplicatePalettes each time.
LaunchFading uses palette maskF, counter1, FLASH_QUICKLY_2 and waits
through FADING_SETTING clear plus VInt; these are not30/50 literal frames
or measured hardware duration. SFX requests and music interfaces retain
their own asynchronous completion. End conditionally waits for view and
clears view speed; no all-motion/audio/input completion barrier is added.

### Dialogue identity, modifiers and complete tokens

textCursor3003 and nine sequential commands join gamescript.txt
keys0BBB–0BC3 and source comments. S=nextSingleText, C=nextText;
00 has no portrait modifier, C0 requests mirrored/right-side portrait.
Logical31's actual appearance may differ from Astral. Alias-based portrait/
speech resolution uses `data/spritedialogproperties.asm`: GESHP has
portrait GESHP and DIALOG_BLEEP_6. Short authored roles below retain full
token order without publishing the dialogue corpus.

| ID / hex | Mode / modifier / logical speaker | Authored role | Complete tokens |
| --- | --- | --- | --- |
|3003 /0BBB|S /00 /128|Geshp halts the party.|`{W1}`|
|3004 /0BBC|C /00 /128|Geshp acknowledges their arrival.|`{W2}`|
|3005 /0BBD|S /00 /128|Geshp admits underestimating them.|`{N}{W2}`|
|3006 /0BBE|S /00 /128|Geshp stakes his life on stopping them.|`{N}{N}{W1}`|
|3007 /0BBF|C /C0 /31|Follower recognizes his serious intent.|`{N}{N}{W2}`|
|3008 /0BC0|S /C0 /31|Follower cautions the named leader.|`{LEADER}{N}{N}{W1}`|
|3009 /0BC1|C /00 /128|Geshp agrees with Astral.|`{W2}`|
|3010 /0BC2|C /00 /128|Geshp blames them for lost trust.|`{N}{W2}`|
|3011 /0BC3|S /00 /128|Geshp describes his last chance.|`{N}{N}{W1}`|

## Complete selected formation and enemy declarations

**Confirmed:** `data/battles/spritesets/entries.asm` slot38 selects
BattleSpriteset38 at `$1B6250..$1B63FA` (426 bytes). Header12/20/3/1
occupies4 bytes;12 ally rows144 bytes;20 enemy rows240;3 regions36;
one point2. Relative offsets are allies4, enemies148, regions388,
point424. Each entity row is12 bytes; filler0 throughout. These are
capacity/formation declarations, not fixed ally identities or spawned HP.

All ally rows use HEALER1, NOTHING, orders NONE/15 and NONE/15, STARTING:

| Ordinal | X,Y | Ordinal | X,Y | Ordinal | X,Y |
| --- | --- | --- | --- | --- | --- |
|0|13,11|4|14,9|8|12,11|
|1|14,11|5|13,9|9|11,10|
|2|14,10|6|12,9|10|12,10|
|3|13,10|7|11,9|11|11,11|

All enemy placement items are NOTHING except ordinal6. Primary/secondary
orders retain encoded identity; NONE=FF, MOVE_TO|0=40 (point0),
FOLLOW_ENEMY|0/5=80/85 (enemy128/133), FOLLOW_TARGET|0=00. Trigger15
names no declared region here. No activation or move-order action is run.

| Ordinal / combatant | Definition | X,Y | AI | Primary / region | Secondary / region | Mode |
| --- | --- | --- | --- | --- | --- | --- |
|0 /128|GESHP|7,27|LEADER|MOVE_TO\|0 /15|NONE /15|STARTING|
|1 /129|DARK_GUNNER|7,26|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|STARTING|
|2 /130|WHITE_DRAGON|8,26|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|STARTING|
|3 /131|CYCLOPS|8,27|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|STARTING|
|4 /132|MIST_DEMON|12,19|HEALER3|FOLLOW_ENEMY\|5 /15|NONE /15|STARTING|
|5 /133|CHAOS_WIZARD|13,20|SENTRY|NONE /15|NONE /15|STARTING|
|6 /134|EVIL_BISHOP|15,20|HEALER4|FOLLOW_ENEMY\|5 /15|FOLLOW_ENEMY\|0 /15|STARTING|
|7 /135|HELL_HOUND|19,14|ATTACKER1|NONE /15|NONE /15|STARTING|
|8 /136|MINOTAUR|19,15|ATTACKER1|NONE /15|NONE /15|STARTING|
|9 /137|DARK_GUNNER|20,15|ATTACKER1|NONE /15|NONE /15|STARTING|
|10 /138|WHITE_DRAGON|6,17|ATTACKER1|NONE /15|NONE /15|STARTING|
|11 /139|BURST_ROCK|14,15|ATTACKER1|FOLLOW_TARGET\|0 /0|NONE /15|HIDDEN|
|12 /140|BURST_ROCK|10,12|ATTACKER1|FOLLOW_TARGET\|0 /0|NONE /15|HIDDEN|
|13 /141|BURST_ROCK|11,14|ATTACKER1|FOLLOW_TARGET\|0 /0|NONE /15|HIDDEN|
|14 /142|BURST_ROCK|12,16|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|HIDDEN|
|15 /143|BURST_ROCK|15,18|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|HIDDEN|
|16 /144|BURST_ROCK|12,20|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|HIDDEN|
|17 /145|BURST_ROCK|12,22|ATTACKER1|FOLLOW_TARGET\|0 /2|NONE /15|HIDDEN|
|18 /146|BURST_ROCK|9,24|ATTACKER1|FOLLOW_TARGET\|0 /2|NONE /15|HIDDEN|
|19 /147|BURST_ROCK|10,26|ATTACKER1|FOLLOW_TARGET\|0 /2|NONE /15|HIDDEN|

Each region is type4, unused byte0, ordered vertices, then0,0:

| Region | Ordered vertices |
| --- | --- |
|0|0,13;0,31;31,31;31,13|
|1|0,19;0,31;31,31;31,19|
|2|0,24;0,31;31,31;31,24|

Point0 is7,27. These are crop-relative declarations; no region inclusion,
spawn execution, overlap admission or later action is inferred.

### Definitions, class bytes and refreshed current stats

Join `data/stats/enemies/enemydefs.asm` and enemymapsprites.asm. Stats
are `(level,maxHP,maxMP,baseATT,DEF,AGI,MOV)` before difficulty/equipment.
All ten definitions have spellPower ENHANCED=99, initialStatus NONE and
four inventory/spell slots; unlisted slots are NOTHING. The56-byte copy
starts at combatant offset0 via j_GetCombatantEntryAddress_0→
GetCombatantEntryAddress. unknownByte occupies byte0 plus nine zeros;
spellPower occupies offset10, also COMBATANT_OFFSET_CLASS. Thus normally
initialized enemies carry class-byte99, not an ally mage/healer class.
Movement type, placement AI, definition ID and this byte remain distinct.
Hidden rocks have not received that copy at this stop.

| Definition / index / mapsprite | Stats | Move type | Equipped item / spells | Resistance |
| --- | --- | --- | --- | --- |
|GESHP95 /159|70,180,120,78,51,191,6|MAGE|FLAIL /FREEZE4|WIND_MAJOR,LIGHTNING_MAJOR,ICE_MAJOR,FIRE_MAJOR,STATUS_IMMUNITY|
|DARK_GUNNER65 /129|38,60,0,48,56,45,4|BRASS_GUNNER|BUSTER_SHOT /none|LIGHTNING_WEAKNESS,STATUS_MAJOR|
|WHITE_DRAGON51 /115|38,66,0,85,50,53,6|FLYING|none /SNOW1|ICE_MAJOR,FIRE_WEAKNESS|
|CYCLOPS31 /95|36,64,0,88,51,46,5|REGULAR|none /none|0|
|MIST_DEMON50 /114|36,68,28,80,41,51,6|HOVERING|none /MUDDLE2|WIND_WEAKNESS,ICE_MAJOR,STATUS_MINOR|
|CHAOS_WIZARD74 /138|36,53,49,47,48,59,5|MAGE|GREAT_ROD /BLAZE4|0|
|EVIL_BISHOP84 /148|38,64,51,54,48,46,5|HEALER|GREAT_ROD /AURA2,BLAST3|0|
|HELL_HOUND29 /93|35,62,0,84,49,50,6|STEALTH|none /FLAME2|ICE_WEAKNESS,FIRE_MAJOR|
|MINOTAUR30 /94|35,58,0,51,51,44,5|REGULAR|ATLAS_AXE /none|0|
|BURST_ROCK32 /96|38,45,0,0,60,43,4|REGULAR|none /none|0|

Prowess `(critical,double,counter)` is Geshp `(125_1IN4,1IN8,1IN8)`,
Dark Gunner `(150_1IN32,1IN16,1IN32)`, White Dragon
`(150_1IN8,1IN32,1IN16)`, Cyclops `(150_1IN4,1IN16,1IN16)`,
Mist Demon `(SILENCE,1IN32,1IN16)`, Chaos Wizard
`(150_1IN32,1IN32,1IN16)`, Evil Bishop `(150_1IN32,1IN32,1IN32)`,
Hell Hound `(150_1IN32,1IN16,1IN16)`, Minotaur
`(150_1IN8,1IN16,1IN16)`, Burst Rock `(150_1IN32,1IN32,1IN32)`.
unknownByte is0 for Geshp/Evil Bishop and the definition index otherwise.
AI priority is1 for Geshp,3 for Evil Bishop,2 otherwise; BIT4 belongs
to Dark Gunner/White Dragon/Mist Demon/Chaos Wizard, BIT5 to Burst Rock.
The STARTING/filler0 initializer keeps only priority's upper nibble:
Geshp1000, Evil Bishop3000, other STARTING enemies2000 before activation.
It discards BIT4; hidden rocks instead take0200, not their definition2020.

InitializeEnemyStats first calls UpgradeRandomBattleEnemies, copies
56 bytes/14 longwords, fills currentHP/MP from maxima, installs X/Y,
movement+placement AI, definition ID, both triggers/orders, placement
item, activation, difficulty adjustment and current-stat/equipment refresh.
All eleven STARTING rows use this path. The battle32 Jaro exception
does not apply. Initial status remains0; equipped effects below change
ATT only, leaving current DEF/AGI/MOV/resistance/prowess as declared.
Geshp's raw currentAGI is191/$BF here; no initializer masks it to63
or generates a double turn in this slice.

Ordinary difficulty truncates5/4 of baseATT once; SUPER does it twice,
before adding equipped ATT. These four item definitions have three
equip-effect pairs, with NONE0 after the one listed ATT effect:

| Item / index | Range / price | Equip classes | Type / use | ATT effect |
| --- | --- | --- | --- | --- |
|FLAIL90|1–1 /1490|WIZ,SORC,VICR|WEAPON /NOTHING|+19|
|BUSTER_SHOT52|2–3 /6800|SNIP,BRGN,BWNT|WEAPON,RARE /NOTHING|+37|
|GREAT_ROD95|1–1 /7900|WIZ,SORC,VICR|WEAPON,RARE /NOTHING|+28|
|ATLAS_AXE41|1–1 /9600|GLDT,BRN,RDBN|WEAPON,RARE,BREAKABLE /BLAZE3|+35|

Pre-equipped definition words are applied by UpdateCombatantStats without
re-running ally equip-class eligibility. Current ATT after normal refresh:

| Definition | Current ATT ordinary / SUPER |
| --- | --- |
|GESHP|116 /140|
|DARK_GUNNER|97 /112|
|WHITE_DRAGON|106 /132|
|CYCLOPS|110 /137|
|MIST_DEMON|100 /125|
|CHAOS_WIZARD|86 /100|
|EVIL_BISHOP|95 /111|
|HELL_HOUND|105 /131|
|MINOTAUR|98 /113|

No refreshed Burst Rock currentATT/stat result is asserted: its definition
is accounted for, but its initialization copy is deferred.

### Placed Fairy Tear versus copied equipment

Evil Bishop134's placement word is FAIRY_TEAR7|USABLE_BY_AI2000=2007.
InitializeEnemyItems skips NOTHING rather than erasing copied inventory.
Here AddItem finds the first empty slot after copied GREAT_ROD|EQUIPPED
(00DF) and masks with ITEMENTRY_MASK_INDEX_AND_BROKEN_BIT=807F.
It writes0007 in slot1, removes the2000 marker and retains the equipped
rod in slot0; slots2/3 remain007F. AddItem also returns masked d1, so
the following equipped-bit test does not equip the tear. This is not
an AI-use guarantee simply because the placement expression says so.

Fairy Tear's item definition has equipFlags NONE, range0–1, price300,
CONSUMABLE, useSpell G_TEAR2, and three NONE0 equip effects. The item
getter reads stored words without restoring the placement flag. Neither
an item use nor a reward is established at this pre-activation boundary.
No item-use action, drop/outcome or spell-effect simulation was performed.

### Hidden, absent and packed entities before activation

InitializeEnemyBattlePosition takes the HIDDEN branch for declared139–147:
mode2 shifts to activation0200, max/currentHP0, X−1 and declared triggers
0/15,1/15,2/15 in groups of three. It does **not** copy BURST_ROCK stats,
class, items, spells, Y, movement/AI, definition ID or move orders.
Those fields retain earlier state; declaration X/Y are future spawn
operands, not current positions. Dead/offscreen is not permanent removal.

Absent148–159 compare beyond the20-enemy count before advancing a0:
mode0→activation0000, max/currentHP0, X−1; triggers come from the
first selected enemy row (15/15), not nonexistent records. Other fields,
including Y, remain stale. No absent12 or hidden9 entry becomes an
initialized STARTING combatant because an old HP/stat value existed.

Reuse the accepted all30-ally heal/formation mechanism. Living allies
and immortal Peter7/Lemon28 heal regardless of membership; ordinary HP0
allies stay dead/unrefreshed. The retained pre-refresh status mask is
STUN/POISON/CURSE; refresh reconstructs curse from actual equipment,
STUN reduces MOV1/AGI5 with saturation, and poison persists. Actual
class/equipment/HP remain inputs. All30 X fields reset−1 and UpdateForce
rebuilds party lists. At most the first twelve active-list entries in ID
order are examined. Dead members consume an outer iteration without
advancing formation; excess entries LeaveBattleParty without a second
list rebuild. Empty N=0 underflows N−1 with no initial empty-list guard.
Unjoined active flags do not manufacture members or guarantee Bowie0
receives formation13,11.

LoadBattle clears physical/alias state again through PositionBattleEntities.
Packing tests signed X/Y and enemy-neutral bit, **not HP**, masks3F,
scales384 and installs Down/eas_Standing. With A eligible allies, the
eleven STARTING enemies use physicalA..A+10; hidden139–147 and absent
148–159 have aliasFF through X−1 regardless of stale Y. No38 neutral
table row restores scene followers. Follower30/31 aliases remain fresh0.
Scene128 now denotes new combat Geshp128; scene Peter follows actual
party/HP/formation eligibility, rather than the scene table.

LoadBattle requests tilesets/fade, rebuilds entities, selects actual
combatant0 alias or negative fallback0, reads target coordinates/384,
loads map/sprites/terrain/music and fades in. Crop17,0 size32×32 is
applied by LoadMapBlocksAndLayout copying the source rectangle to0,0;
LoadMapArea uses battle dimensions, starts0,0 and ends31×384 each axis.
It does not add17 to stored combatant positions. Selected terrainentries
slot38 is BattleTerrain38, custom background OVERWORLD, enemy-leader
presence−1. No decoded terrain/material or natural camera proof follows.
The empty start/488 latch then completes before the activation stop.

`randombattles.asm` lists exactly3,8,9,10,14,17,19,21,24,26,35.
UpgradeRandomBattleEnemies is called for the eleven normal initializers,
but intact38 fails IsBattleUpgradable and returns the original definition
before DetermineBattleUpgrade, level/category/RNG work. Completion538
or Bowie's level does not turn38 into Battle35. Hidden rocks have not
called that helper at this stop. Changed live selectors are different inputs.

## Actual compiler and production-consumer boundary

**Confirmed about tooling:** the sole selected OriginalPrograms observation
with scene_maps=[66] lowers bbcs_38 to17 instructions and interior
cs_4DDFE to59 (76 total). The first program ends with a jump; it is not
the whole opening. eas_Init has14 instructions; all five inline motion
streams lower without nested native operations. Top-level native calls
are cs_4DDFE[2] cameraSpeed and[54]/[56] flashScreenWhite. No map/
definition export, scene execution, runner or adapter launch occurred.

Lowered population includes only19 ordinary follower rows. On66 clear65
matches native selection, but held65 should include the Caravan prefix;
it is omitted. With no later30 producer the lowered alias0 differs from
native Caravan. SceneEntities.Build has zero aliases, prepared ally
appearances and a49-slot guard; ProgramRunner's position operations do
not reproduce the native logical-ally HP skip. These are concrete
changed-party/flag/HP gaps even before reached cameraSpeed refusal.

| Actual owner / consumer | Shape and selected limitation |
| --- | --- |
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py), [Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Scene-map/entity, flag/text, motion, sound, fade, camera and direct quake shapes exist. Native calls become UnsupportedInstruction and reached program-opcode refusal. Earlier resource/service failures may occur first; later typed operations are not proof of a runnable prefix.|
|[SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [allocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Follower/fixed allocation, zero aliases, Init and relative-motion waits have consumers. Overworld65 prefix, native HP skip, actual appearances/capacity and original service timing remain gaps.|
|[PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [preparation](../../src/sf2tool/remake_exploration_battles.py)|Placement/region/point/terrain structure can retain spawn/item expressions. Pin admits1/2/3 and preparation2/3, neither38; no38 scene/terrain/definitions/world/route/material binding was produced.|
|[PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs), [PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Definition metadata can retain loadouts/class data. Scenario builder restricts battles1/2/3, STARTING, admitted placement item expressions, REGULAR spell power and command sets.38 adds HIDDEN, Fairy Tear, ENHANCED, HEALER3/4 and SENTRY refusals. Successful parsing is not initialized38 content.|
|[BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs)|Requires controlled roster-only/already-refreshed allies, difficulty0 and admitted status/control/capacity. Equipped enemy loadouts refuse enemy-equipment-refresh; SpawnMode2 refuses spawn-mode before initialization. Formula-shaped activation words do not implement native hidden0HP/X−1/stale-field behavior. No general all30/SUPER/interrupted-refresh path.|
|[BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Before→initialize→load→start continuation exists; F88 NewBattle refuses suspended-battle-entry. Initialization computes before publication. BattleStartFinished later invokes BattleAdvancer beyond this slice's stop; it was not executed here.|
|[ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs), [ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Map66 layer254/zero origins/unit parallax/zero autoscroll meets existing view-profile predicates. Nonzero view speed separately refuses field-view-speed. Adapter selects board area by crop origin and implements generic quake/fade/sound/view effects; this does not bind the original flashes or prove VInt/DMA, selected resources, completed presentation or usable input.|

No consumer correction is included. Early Map66/Battle02–03 support is
not a late-story38 acceptance. The predecessor's native reload request
and cross-map scrolling gaps remain; a selected source compilation
bypasses map-event preparation and does not repair them.

## Reproduction, preserved results and Unknowns

The ignored direct recipe checks selected commands/inline payloads/entity
table/tokens, hook tables and fresh/F88 branch order, formation/regions/
point, definitions/items/refresh, hidden/absent fields, actual consumers,
changed states, source pin/pair metadata, scope/navigation/links/UTF8/LF
and private publication. Tracked source and prose own durable findings;
ignored recipes/receipts are coordination state. Load current selections
in the same launching process and use a fresh receipt:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1144/review.py <configured-source-disasm-root> --receipt local/issue1144/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The source argument ends in disasm. The sole lowering is retained in
`local/issue1144/observe-01.json` (0.1169s); review reuses it rather than
repeating compilation for a commit change. Apply ADR0004's relevant
semantic/provenance/polarity/width/order/selector/private checklist.
No maintained parser/schema/fixture/helper test or H3 callback changed.
Normal verify is **SKIPPED** under explicit documentation scope, as are
full/old/runtime gates. Record clean-head planner and actual exact-head
scoped CI in the frozen handoff; a proposed command is not a passing run.

Preserve [Battle37's y64 runner deduction](./battle37-outcomes-and-lemon-aftermath.md#one-lowering-and-actual-current-consumers),
[Map53's failed import/zero partial programs](./roft-to-galam-and-battle36-admission.md#actual-lowering-and-current-consumer-boundary),
the predecessor's sole compiler observation and main-gate receipt, twelve
completed old private comparison failures and earlier alias/collision/
follower/accounting corrections through those linked owners. No repair,
rerun, erasure or relabeling as interrupted occurred here.

**Unknown:** natural admission; actual caller words/selectors/flags/party/
class/HP/MP/status/equipment/followers/aliases/transport; F88 byte182 and
retained crop/combatants/stale fields/queue/turn/RNG state; decoded terrain/
collision and selected materials; interruption/interleaving and completed
motion/view/quake/flash/fade/sprite/text/audio/map/arrival/input services,
original hardware/VInt/DMA timing, usable control and save persistence.
These are not an automatic acquisition queue. Activation/spawn/turn/
Burst Rock explosion/actions and outcomes remain excluded.

Freeze a clean committed/pushed Draft PR with exact head/tree, actual
checks/planner/CI, gaps and retained handoff. Main-gate owns independent
acceptance/merge. Stop writer/processes at handoff; no merge, cleanup,
archive or next slice.
