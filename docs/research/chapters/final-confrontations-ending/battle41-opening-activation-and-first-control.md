# Battle41 opening, activation and first control

Continue with [Battle41 outcomes and Ancient Tower stairway](./battle41-outcomes-and-ancient-tower-stairway.md).

Evidence date: 2026-10-22. **Confirmed (static):** Outside Ancient Tower
has a short Reaper opening, twelve party formation rows, twenty STARTING
enemies, five rectangular regions and one AI point. Seventeen enemies
use unconditional15/15 AI; the last three use0/4,2/3 and1/15. All twelve
formation points miss every region. Reaper's rawAGI57 has one queue
record; no fixed natural first actor follows. Natural entry, completed
presentation and usable remake control remain **Unknown**.

## Agreement, provenance and method

Standard rigor for [Issue #1166](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1166)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053),
from accepted base `1633716d6c73369f6162c8d65ff1fdd8db3c9d09`,
tree `395446ec036ade54754843200dda72179aa5516e`.
Continue [Map56/Creed and Battle41 admission](./ancient-tower-ascent-creed-and-battle41-admission.md)
through the complete opening, initialization, first activation and first
player/AI dispatch interfaces. Own this document and predecessor navigation
only. The result supports future content contracts. Individual action
algorithms, damage, drops, outcomes, afterbattle and next maps are separate
work; no index, schema, fixture, extractor, engine or shared registry changes.

Reuse [Battle39 opening/first control](./battle39-opening-and-first-control.md),
[Battle40 initialization](./battle40-initialization-and-opening.md),
[Battle40 activation/queue](./battle40-regions-terrain-and-first-control.md),
[spritesets](../../battle/battle-spriteset-data.md), [battle loop](../../battle/battle-loop.md),
[scripting](../../world/common-scripting.md), [maps](../../world/common-maps.md) and
[stats](../../party/common-stats.md). Selected41 operands and actual consumers are
checked here; earlier scenes and encounters are not substituted.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` means `code/gameflow/battle/`; `map/` and `entity/` mean
children of `code/common/scripting/`. sf2const.asm, sf2enums.asm,
sf2macros.asm and sf2cutscenemacros.asm own constants/encodings.
Fnnn denotes flag nnn; ordinals are zero-based; intervals are half-open;
activation/order/item words are hexadecimal unless stated otherwise.
USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source/ROM equality or original-runtime observation is claimed.

Minimum evidence is the complete local scene/placement/region data and
necessary shared callers and production consumers. Reuse the immutable
18,285,979-byte init/script pair and `local/issue1005/pair.txt` provenance;
do not regenerate it, decode layouts/terrain or import the world. Linear
selected-source/consumer reads plus one pair read and bounded integer
geometry checks suffice. Only one explicit opening lowering is run.
Ignored recipes/receipts belong to `local/issue1166/`; estimated selected
evidence below2MiB, total with CI/coordination below4MiB, seconds per
direct check, peak-memory target512MiB **unmeasured**. Reuse source snapshots
by direct equality on subsequent checks to avoid duplicate payloads.
Reassess before expanding acquisition or adding cross-system machinery.

## Admission, two hooks and F88

**Confirmed:** the predecessor owns Map56's441/541/Y24 predicate and
Creed's903/904 lifecycle. Selected coordinate row41 at `$7B55..$7B5C`
is `56,0,0,32,36,255,24`: map, crop X/Y/W/H, trigger X/Y bytes.
Held441 and Y24 admit any supplied X; a physical route still needs its
marker/collision/warp callers. Crop writes precede541; held541 clears441
and still returns41 on that call.903/904 and540 are not direct predicates.
MainLoop passes d0.w map56 and d1.w battle41 to j_BattleLoop.

`battle/battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`) clears
PLAYER_TYPE.b, sets399, then checks88. Clear88 resets seconds, saves
d0/d1 words, installs their low map/battle bytes, installs base VInt
functions, and calls the before wrapper. Returning restores the saved
words and rewrites both selector bytes. It then clears90..105 inclusive,
heals living/immortal allies, initializes ally positions, initializes
enemies, clears AI memory, clears d0.w, calls LoadBattle, and calls the
start wrapper. First activation follows the start wrapper, not the scene.

| Complete48-word table / selected byte offset82 | Selected target |
| --- | --- |
|`data/battles/cutscenes/beforebattlecutscenes.asm`, base `$47A88`, slot `$47ADA`|bbcs_41 at `$4ED0E`|
|`data/battles/cutscenes/battlestartcutscenes.asm`, base `$47B2C`, slot `$47B7E`|ms_Empty, single FFFF word|

The before wrapper reads live CURRENT_BATTLE and450+41=491, skipping
its body when held; it never sets491. The start wrapper reads those
values again and sets491 **before** dispatching the empty script.
An executed empty ExecuteMapScript still resets dword_FFB1A4 to
FF9004_LOADING_SPACE, clears SKIP_CUTSCENE_TEXT, conditionally waits for
view scrolling if a dialogue window is open, and clears view speed.
Skipping the wrapper bypasses those interpreter effects.

| State supplied at the reads | Consequence |
| --- | --- |
|88 clear,491 clear throughout|Opening, fresh reconstruction/load, set491, empty start, then activation.|
|88 clear,491 held throughout|Skip both hook bodies; fresh reconstruction/load still occurs.|
|541 held while441 admits|Completion and intro491 are independent; no extra hook bypass.|
|491 changes between wrappers|Each obeys its own read; no shared snapshot.|
|Selectors change during a service|Helpers use live selectors. Saved-word restoration repairs only the named selector bytes afterward.|
|Interrupted prefix|Scene effects can exist with491 clear;491 can be held before start returns. Neither proves opening completion.|

Fresh battle initialization does not call ClearMapSetupTempFlags256..383,
reset party membership or generally clear saved story flags.441/541,
76,903/904, region flags90..105,491, AI memory and checked storage have
different owners/lifetimes. There is no scene resetForceBattleStats or
join/leave/revive command. PLAYER_TYPE and seconds have only the stated
prefix writes. No new save transaction is established.

Held88 instead restores saved seconds, clears88 and AI memory, clears
view target, calls LoadBattle with **incoming d0 unchanged**, and jumps
directly to the retained individual-turn loop. The following bra @Start
is unreachable. It bypasses selector installation, both hooks, healing,
initial positions/definitions, region clearing, activation and generation.
Retained map/battle/crop, combatants, queue/cursor, flags and RNG remain
inputs; carrying56/41 in d0/d1 does not install them on this branch.

For supplied d0.w=56, GetEntityIndexForCombatant sees nonnegative low
byte56, applies maskFF and reads `$FFB140+56=$FFB178`, inside the64-byte
alias table ending `$FFB180`: enemy alias24/logical152. This is not
ally56 or a scene alias. PositionBattleEntities has just rebuilt it
using retained combatant152's signed X/Y and neutral bit, without HP.
An otherwise unchanged fresh41 state leaves152 absent and its aliasFF;
changed retained state can supply a slot. LoadBattle stores fetched T
as view target; signed-negative T uses physical0 for chosen coordinates,
otherwise masks3F and scales by ENTITYDEF_SIZE_BITS. This is conditional
address arithmetic, not an observed suspend restore. A retained odd
cursor reads a score as actor; offsets128..255 exceed the128-byte queue.
An encountered FF can subsequently reach @Start with then-current state.

## Complete opening and scene population

**Confirmed:** `data/battles/entries/battle41/cs_beforebattle.asm`
spans `$4ED0E..$4ED8E`,128 encoded bytes. Its20 top-level commands
occupy100 bytes, inline moveDown2/endActions4 more; scene code therefore
ends at ce_4ED76 after104 bytes. The table occupies24 bytes: three main
words6, two records×8, FFFF terminator2. cs_4ED48 at top-level index9
is a continuation inside the same scene, not an independent story event.

| Index | Exact ordered request |
| --- | --- |
|0|textCursor3083|
|1|loadMapFadeIn56,12,21|
|2|loadMapEntities ce_4ED76|
|3|setActscriptWait Bowie0,eas_Init|
|4|setPos Peter7,16,24/Right|
|5|setPos FOLLOWER_B31,15,24/Right|
|6|setPos Lemon28,14,24/Right|
|7|jumpIfFlagClear76→cs_4ED48/index9|
|8|Only held76: setPos Zynk26,13,24/Right|
|9–10|cs_4ED48: MUSIC_BATTLE_THEME_3; fadeInB|
|11–14|single3083 speaker128; setFacing Bowie Up; setCamDest12,4; single3084 speaker128|
|15|entityActionsWait128: moveDown2; endActions|
|16–19|continued3085; continued3086; single3087, all speaker128; csc_end|

The main table header is Bowie17,24/Right with eas_Idle. Record0 is
Peter16,24/Right with live ally mapsprite/eas_Init; record1 is
Reaper16,6/Down, mapsprite99/eas_Init. `mainEntity` uses three words;
`entity` uses X/Y/facing/sprite bytes and a long script pointer. This
table neither defines Lemon/Zynk nor selects the actual battle party.

csc42 deactivates VInt services, disables display/interrupts, reads the
table header, calls InitializeMapEntities and LoadEntityMapsprites,
then reenables display/interrupts and services. ClearEntities clears49
ordinary physical records and64 aliases. Ordinary followers allocate
**before** the explicit records. Map56 is not an overworld, Tactical Base
or Nazca Ship map; the19-row ordinary follower table applies, not its
Caravan65 overworld prefix. Every held row allocates; duplicate logical
aliases resolve to the last allocation. FOLLOWER_B31 can mean Astral
through74, then Oddler through68, then Astral through70. No matching
row leaves its fresh alias0. Narrative labels do not fix live identity.

Let n be allocated followers and p=1 if Peter needs a new explicit
record, otherwise0. Peter's nonzero alias from67 makes the explicit
record consume its script tail without resetting position/script; when
absent, that record allocates him even with67 clear or HP0. Reaper's
ordinary sprite allocates physical n+p+1 and scene logical128; Bowie is
physical0. F71 alone supplies Lemon; F76 alone supplies Zynk through the
ordinary table. Neither has a fallback explicit record. The small table
does not prove arbitrary simultaneous follower flags safe against the
existing five follow-script pointers.

### Operand, HP, alias and asynchronous semantics

`loadMapFadeIn` is opcode37 hex: three **words** map56/view12,21,
not a warp or exploration setup. csc37 starts out-to-black then falls
through csc48: detach view, request tilesets, wait VInt/fade, deactivate
services, call LoadMap with encoded view coordinates, reenable services,
wait one VInt. CURRENT_BATTLE is already41, so crop0,0/32×36 applies.
LoadMapEntities is a separate population replacement. The later fadeInB
is its own full-black fade service, not redundant proof the map appeared.

Only Bowie's eas_Init is explicitly installed **and waited**. csc15
reads entity/wait **bytes** plus a long pointer, resolves the alias,
writes physical-slot byte to the read timer, installs the script, and
waits for that pointer to equal eas_Idle. It has no HP guard. eas_Init
sets acceleration0, speeds32/32, acceleration/deceleration OFF,
obstructable ON, entity/map uncollidable OFF, autoFacing ON, facingUp,
nonimmersed, size24, refresh, then idle. Thus the header's Right is not
an invariant after Init. This wait is only for Bowie's current slot.
Peter/Reaper's declared Init and ordinary followers' follow scripts
are not a globally joined group and can interleave with later commands.

setPos/csc19 encodes entity/X/Y/facing **bytes** after its opcode word.
It peeks the actual entity byte and passes skip length4 to
AdjustScriptPointerByCharacterAliveStatus. Allies0..29 with currentHP0
skip the complete four-byte payload;30/31 and enemy-bit identities
bypass that test. A nonzero HP word, including signed8000, does not skip.
It resolves the alias, multiplies coordinate bytes by384, writes current
X/Y **and** XDEST/YDEST words, writes facing and updates sprite. It does
not install idle, cancel a follow script, recruit or revive anyone.

setFacing/csc23 likewise peeks a **byte** entity, uses skip length2,
then consumes entity/facing bytes and refreshes the sprite. Only Bowie
uses it here. It is distinct from followEntity's word/high-byte guard
and faceEntity's coordinate-facing implementation in earlier owners.

| Supplied scene state | Actual effect |
| --- | --- |
|Peter HP0,67 either state|His explicit/follower entity can exist, but setPos is skipped; later immortal healing cannot retroactively apply it.|
|Lemon HP0|setPos skips even if71 allocated him.|
|Lemon HP nonzero,71 clear|No explicit Lemon record; fresh alias28=0, so his setPos moves Bowie to14,24/Right.|
|FOLLOWER_B absent|31 bypasses ally HP test; alias0 makes its setPos move Bowie to15,24/Right, subject to later commands.|
|Multiple FOLLOWER_B rows held|Last alias wins; earlier physical followers still exist.|
|76 clear at branch|Skip only Zynk's setPos; it neither clears76 nor removes an already allocated entity.|
|76 held and Zynk HP0|Allocation is independent of HP, but setPos skips.|
|76 changes between population and branch|Allocation and command read their own live flags. A newly held76 with no alias and nonzero Zynk HP can target physical0; a newly clear76 can leave an allocated Zynk unpositioned by this command.|
|Bowie HP0|Init still installs/waits; later setFacing skips. Other missing aliases can still have moved his slot.|

The scene never sets or clears76. It has no HP test on Reaper's text or
motion. csc32 reads camera X/Y **words**, detaches view, calls
SetCameraDestination and WaitForViewScrollEnd for12,4. This is a view
request, not an actor coordinate or barrier for follower actions.

entityActionsWait/csc2D encodes entity/wait bytes. It writes slot timer,
clears collision bits with9F and generates one script at the current
dword_FFB1A4 buffer. moveDown's direction/count bytes become relative
0,+2 and wait-for-destination action; endActions adds a jump to eas_Idle.
The handler waits on **that slot's script pointer**, without an ally-HP
precheck. With unchanged Reaper current position16,6 it requests16,8;
the relative destination uses current X/Y when the action executes.
This is distinct from the combat placement16,8 and does not carry the
scene object into battle. Neither requested destination nor source wait
proves natural arrival, velocity/timing, display/DMA or all actor completion.

### Five text requests

All modifiers are00 and all speakers are logical128. gamescript.txt
keys0C0B–0C0F supply the following complete ordered control tokens.
Only IDs/tokens and authored summaries are published, not original dialogue.

| ID | Mode | Tokens | Authored summary |
| --- | --- | --- | --- |
|3083|single|`{N} {W1}`|Reaper challenges the arriving party and recognizes its origin.|
|3084|single|`{W2}`|He questions what happened to Odd Eye.|
|3085|continued|`{N} {W2}`|He reacts to Odd Eye's defeat.|
|3086|continued|`{N} {W2}`|He resolves to block the party himself.|
|3087|single|`{W1}`|He threatens an immediate fight.|

csc00 closes portrait/text after display and requests Sleep10; csc02
continues without that close/sleep. Debug text skip can omit display
and cursor advancement. The speech lookup consumes the aliased live
mapsprite; Reaper99 has no selected row in spritedialogproperties.asm
and uses PORTRAIT_DEFAULT/SFX_DIALOG_BLEEP_6 fallback. csc_end conditionally
waits for scrolling when the dialogue window is open and clears view
speed. Music, fades, text and VInt calls establish requests/ordering,
not audible/rendered completion or an input-ready frame.

## Complete12/20/5/1 spriteset and fresh initialization

**Confirmed:** `data/battles/spritesets/entries.asm` slot41 selects
BattleSpriteset41 (`spriteset41.asm`) at `$1B66EE..$1B68B0`,450 bytes:
header4 +12×12 ally rows +20×12 enemy rows +5×12 regions +point2.
Relative offsets are4,148,388,448,end450. Each entity row encodes
identity/X/Y/commandset bytes, item word, primary-order/region and
secondary-order/region bytes, filler byte and spawn byte. All filler
bytes0 and STARTING0; all placement items NOTHING007F. Ally rows use
HEALER1 and NONE15/NONE15, not a selected permanent ally identity.

| Ordinal | X,Y | Ordinal | X,Y | Ordinal | X,Y |
| --- | --- | --- | --- | --- | --- |
|0|16,26|4|17,24|8|14,25|
|1|17,26|5|17,25|9|14,26|
|2|15,25|6|15,26|10|16,25|
|3|16,24|7|15,24|11|14,24|

N=NONE FF, M0=MOVE_TO0=40, E#=FOLLOW_ENEMY#=80 OR #.
AI commandsets: LEADER14,HEALER5=4,ATTACKER1=6,ATTACKER3=9,SENTRY8.
Both orders/triggers are shown even where the secondary order is NONE.

| Ordinal / logical | Kind | X,Y | AI | Primary/region; secondary/region |
| --- | --- | --- | --- | --- |
|0 /128|REAPER|16,8|LEADER|M0/15;N/15|
|1 /129|BLUE_SHAMAN|15,5|HEALER5|E0/15;N/15|
|2 /130|DEMON_MASTER|17,6|ATTACKER3|E0/15;N/15|
|3 /131|HYDRA|23,6|ATTACKER3|E4/15;N/15|
|4 /132|CHAOS_WARRIOR|24,5|SENTRY|N/15;N/15|
|5 /133|DEMON|18,12|SENTRY|N/15;N/15|
|6 /134|CHAOS_WARRIOR|18,13|SENTRY|N/15;N/15|
|7 /135|DEMON_MASTER|20,13|ATTACKER3|E5/15;N/15|
|8 /136|HYDRA|13,12|SENTRY|N/15;N/15|
|9 /137|CHAOS_WARRIOR|13,13|SENTRY|N/15;N/15|
|10 /138|DEMON_MASTER|9,12|ATTACKER3|E11/15;N/15|
|11 /139|DEMON|7,11|SENTRY|N/15;N/15|
|12 /140|CHAOS_WARRIOR|15,19|ATTACKER3|E13/15;N/15|
|13 /141|HYDRA|18,18|SENTRY|N/15;N/15|
|14 /142|HYDRA|18,19|SENTRY|N/15;N/15|
|15 /143|HYDRA|22,18|ATTACKER3|`144\|0`=90/15;N/15|
|16 /144|DEMON|23,17|SENTRY|N/15;N/15|
|17 /145|DEMON|3,18|ATTACKER1|N/0;N/4|
|18 /146|CHAOS_DRAGON|28,9|ATTACKER1|N/2;N/3|
|19 /147|CHAOS_DRAGON|5,3|ATTACKER1|N/1;N/15|

Point0 is17,7; all placements/vertices/point fit crop32×36 without
proving traversability. MOVE_TO40 has bit6 set: GetMoveOrderPosition
selects AI_POINTS, masks low nibble0, doubles it and reads this point.
FOLLOW_ENEMY80/84/85/8B/8D directly names combatants128/132/133/139/141.
The literal decimal144 OR0 is90 hex, with bit7 set and bit6 clear:
it follows combatant144/enemy16 at23,17, not point0, enemy0 or a new
behavior class. Stat getters and GetMoveOrderPosition use that complete
combatant byte; they do not mask it to its low nibble on the follow path.
In activated StartAiControl, a zero-HP follow target promotes secondary
orderFF into primary and sets secondaryFF. A nonzero HP target retains
the order. MOVE_TO skips that HP test. These are preparation interfaces,
not a concrete movement/attack decision or a guarantee the followed
target has a valid position/alias. Scene128 and combat128 are rebuilt
objects; the shared number does not preserve scene script/state.

### Selected definitions, current stats and equipment

`battle/battleloop/initializecombatants.asm:InitializeEnemyStats` copies
the selected56-byte definition in14 longwords; sets currentHP/MP from
maxima, X/Y, movement/commandset byte, kind, both trigger nibbles/orders,
placement item, activation word, difficulty ATT, then current-stat refresh.
UpgradeRandomBattleEnemies returns the original kind:41 is absent from
the eleven-entry random-battle list. Battle32/Jaro's exception does not
apply. Source names ENHANCED99 in the copied class/spellPower byte;
all seven kinds have initialStatus NONE. Four item and four spell slots
exist; unlisted slots are NOTHING.

| Kind / ID / mapsprite | Level,HP,MP,baseATT,DEF,rawAGI,MOV | Mover | Copied equipment / spells |
| --- | --- | --- | --- |
|REAPER35 /99|41,82,31,60,49,57,6|REGULAR|Battle Sword /MUDDLE2|
|BLUE_SHAMAN85 /149|42,72,60,64,54,51,5|HEALER|Holy Staff /AURA3,BLAST4|
|DEMON_MASTER75 /139|42,73,68,48,50,65,6|MAGE|Mystery Staff /FREEZE4|
|HYDRA33 /97|39,70,0,96,62,50,4|STEALTH|none /none|
|CHAOS_WARRIOR34 /98|40,76,23,92,50,54,5|REGULAR|none /ATTACK1|
|DEMON52 /116|40,75,38,90,50,55,6|FLYING|none /BOLT2|
|CHAOS_DRAGON53 /117|41,83,0,93,48,57,6|FLYING|none /SNOW2|

| Kind | Resistance | Base prowess: critical,double,counter | Copied AI→initial activation |
| --- | --- | --- | --- |
|Reaper|STATUS_MAJOR|MUDDLE,1IN16,1IN16|2000→2000|
|Blue Shaman|0|SILENCE,1IN32,1IN32|3000→3000|
|Demon Master|LIGHTNING_MINOR,STATUS_IMMUNITY|150_1IN32,1IN32,1IN16|2000→2000|
|Hydra|ICE_WEAKNESS|150_1IN32,1IN8,1IN8|2020→2000|
|Chaos Warrior|STATUS_MAJOR|STUN,1IN16,1IN16|2000→2000|
|Demon|LIGHTNING_WEAKNESS,FIRE_MINOR,STATUS_MAJOR|MAGIC_DRAIN,1IN32,1IN16|2010→2000|
|Chaos Dragon|LIGHTNING_WEAKNESS|150_1IN4,1IN32,1IN16|2010→2000|

The initializer retains only the definition's upper priority nibble and
combines rotated/masked filler/spawn word0000. BIT4/5 are stripped;
129 has3000, the other19 have2000. No selected PRIMARY1,SECONDARY2,
AI_CONTROLLED4,NEUTRAL8,HIDDEN0200 or RESPAWN0100 is installed. Priority
2000/3000 is not activation. unknownByte is0 for Blue Shaman and otherwise
the selected kind ID. Current status remains0; no selected AGI has bit7.

NOTHING placement skips AddItem entirely and retains copied equipped
holdings: Reaper first slot00C2 (Battle Sword66), Blue Shaman00E1 (Holy
Staff97), Demon Masters00E4 (Mystery Staff100), others007F in all four
slots. Remaining equipment slots are007F. No placement AI/drop flag,
equip attempt, new item, class-compatibility check or curse application
occurs. UpdateCombatantStats scans already EQUIPPED0080 holdings.
Battle Sword adds ATT35; Holy Staff ATT29 plus HP_RECOVERY3;
Mystery Staff ATT39 plus MP_RECOVERY2. Recovery entries are no-ops in
this stat-refresh dispatch, not an initial HP/MP increase or observed
future recovery tick. Item definitions retain their own equip/range/use
rules: Battle Sword HERO/BRN/BDBT/RDBN,1–1/9200; Holy Staff VICR,1–1/9000;
Mystery Staff WIZ/SORC/VICR,1–1/10000. All WEAPON|RARE/use NOTHING.

The duplicated difficulty conditional applies floor(baseATT×5/4) once
at all difficulties; SUPER repeats that truncating multiplication.
Equipment is added afterward. Zero status and these items leave selected
DEF/MOV/resistance/rawAGI unchanged.

| Kind | Current ATT ordinary / SUPER | Current rawAGI |
| --- | --- | --- |
|Reaper|110 /128|57|
|Blue Shaman|109 /129|51|
|Demon Master|99 /114|65|
|Hydra|120 /150|50|
|Chaos Warrior|115 /143|54|
|Demon|112 /140|55|
|Chaos Dragon|116 /145|57|

HealLivingAndImmortalAllies visits all30 allies. Living allies plus
Peter7/Lemon28 regain maxima regardless of membership; other HP0 allies
skip. STUN/POISON/CURSE remain for refresh, which rebuilds curse from
equipment. STUN's MOV1/AGI5 decreases saturate. This occurs after the
scene, so it does not repair earlier scene HP skips.
InitializeAllAlliesBattlePositions resets all30 X bytes to−1, rebuilds
force lists, then considers at most12 active entries in ID order.
Dead members consume an outer iteration without consuming a formation
row; excess entries LeaveBattleParty without another force rebuild.
The established empty-membership N−1 underflow boundary remains. Y and
unrelated state are not cleared by the X reset. Formation ordinals and
scene actors do not fix the actual participating ally identities.

All32 enemy slots128..159 are considered. Selected128..147 copy full
definitions; absent148..159 fail the count check **before** a0 advances:
activation0000,max/currentHP0,X−1, triggers15/15 from the first Reaper
row. Y/kind/items/spells/orders/commandset can remain stale. Changing
only X/HP does not install an absent definition or supported loadout.

LoadBattle (`$25610..$256A2`) rebuilds population/aliases from combatants,
packing allies then enemies by signed X/Y and enemy neutral bit, without
an HP gate. With A eligible allies, unchanged enemies occupy physical
A..A+19; A≤12 gives maximum31, below49 ordinary slots. No selected
mapsprite uses the special240 path, and the neutral-entity table has no41
override.148..159 aliases becomeFF through X−1;30/31 remain fresh0.
Scene followers/scripts/positions are replaced, not battle attachments.

Fresh d0.w=0 chooses combatant0's alias/view/chosen coordinates, with
negative alias falling back to physical0. LoadBattle then requests sprites,
map, VInt, mapsprites, base services, terrain, map music and fade. Its
global dependencies are terrainentries.asm slot41, custombackgrounds.asm
TOWER_EXTERIOR, enemyleaderpresence.asm byte−1;41 is absent from the
halved-EXP table. Leader presence is a separate battle rule, not the
LEADER commandset or an extra PRIMARY bit.

The64×64 working-layout words at FF0000 are distinct from the48×48/
2304-byte BATTLE_TERRAIN_ARRAY at FF5F00. LoadMap's new-map path decodes
base blocks/layout, applies flag/item copies, then crop→0,0; Map56's
flag-copy table is just FFFF. LoadBattleTerrainData separately selects
and decompresses BattleTerrain41. This opening has no setBlocks or
SetTerrain, and the region dispatcher has only32/40 rows. No41-specific
start/region layout or terrain rewrite follows from these dispatchers.
The defeated-enemy table's41 slot `$47C3A` is also ms_Empty; that table
selection is not an outcome/afterbattle contract. It does not assert that
all later battle services leave terrain untouched: ordinary AI pathfinding
can use temporary obstruction bits. No layout/terrain payload was decoded.

## Five rectangles, cache lifetime and activation

**Confirmed:** regions begin at relative388 / `$1B6872`, occupy60 bytes
through `$1B68AE`; point0 occupies the final two bytes. Each region is
type4, unused0, four X/Y byte pairs in exact order, trailing0,0.

| Region / flag | Complete raw12 bytes | Inclusive native rectangle |
| --- | --- | --- |
|0 /90|`4,0,0,0,0,22,19,22,19,0,0,0`|x0..19,y0..22|
|1 /91|`4,0,0,0,0,17,18,17,18,0,0,0`|x0..18,y0..17|
|2 /92|`4,0,12,0,12,11,31,11,31,0,0,0`|x12..31,y0..11|
|3 /93|`4,0,26,12,26,19,31,19,31,12,0,0`|x26..31,y12..19|
|4 /94|`4,0,0,24,0,28,5,28,5,24,0,0`|x0..5,y24..28|

`battle/battleloop/triggerregions.asm` (`$1AC9FC..$1ACF30`) tests
triangles1/2/4 then3/2/4. Each considers all30 ally slots, requiring
signed HP.w>0 and signed X.b/Y.b≥0; joined/active membership, enemies,
terrain and collision are not predicates. Thus an eligible nonmember can
trigger; HP0/8000..FFFF or coordinate128..255 excludes that ally, while
another eligible ally can still hit. Coordinates are crop-relative,
without adding scene/world origin. The edge helper sign-extends bytes,
chooses horizontal on equal absolute deltas, uses MULS/DIVS truncating
toward zero and projects from endpoint B. Bounded collinearity accepts.
The existing arithmetic was checked at all128×128 nonnegative signed-byte
coordinate pairs for **each selected rectangle**: zero mismatches against
the inclusive bounds, with460/342/240/48/30 hits. This finite, selected
geometry check does not establish arbitrary quadrilateral parity or reach.
Region indexFF rejects; the native signed cmp/ble admits equality with
count, so ordinary0..4 iteration must not be generalized to safe index5.

ActivateEnemies (`$2550C..$25544`) scans128..159, then falls through once
for160. Its guard is signed X.b≥0, HP.w **nonzero**, negative d0.b;
it does not inspect Y, membership, neutral or activation. HP8000 passes
this guard and queue admission but fails ally geometry. Entity packing
requires nonnegative X/Y and nonneutral enemies but no HP test. These
different gates cannot be replaced with one generic alive/present test.

For the first passing enemy, each clear bit in
NEWLY_TRIGGERED_BATTLE_REGIONS.w (`$FFB20C`) permits geometry. A hit sets
90+region; both hit and miss then set its checked bit. Subsequent enemies
reuse flags and checked bits. This word records tested geometry, not newly
activated regions. ClearAiMemory writes48 FF last-target bytes and48 zero
memory bytes, ending immediately before it; fresh90..105 clearing does
not clear it. Enemy StartAiControl's @NonSwarmAi writes word0 before
testing15/15 or PRIMARY. Selected AI has no SWARM15; ally AI skips this
enemy branch. A nominal round boundary or player move alone is not a reset.

All12 formation points lie at x14..17,y24..26, outside every region.
With only those eligible positions, clear90..105 and incoming checked C,
the first passing enemy leaves C OR001F and sets no selected flag.
Fresh slots ensure an ordinary passing enemy; a changed state with none
passing (including pseudo160) never reaches geometry/activation. With a
passing enemy but no valid allies, misses still mark checked bits and do
not erase held flags. A later move inside a checked region is suppressed
until its bit is cleared, independently of whether its flag is already set.

| Supplied eligible ally point, with corresponding unchecked bits | Regions that can be set |
| --- | --- |
|19,22 /20,22|0 /none|
|18,17|0,1|
|12,11|0,1,2|
|31,11 /31,12|2 /3|
|26,19 /25,19|3 /none|
|5,28 /6,28|4 /none|
|0,23 /0,24|none /4|

GetTriggerRegions splits the combatant's byte into high primary and low
secondary nibbles. Both15 preserves activation. Held primary wins and
ORs1; only its miss permits held secondary to OR3. Misses never clear
either bit; an existing SECONDARY survives a primary hit.

| Selected row / ID | Primary/secondary region | Fresh word / changed held-flag result |
| --- | --- | --- |
|0..16 /128..144|15/15|129 remains3000, others2000; no activation write, later AI bypasses PRIMARY.|
|17 /145|0/4|2000;90→2001; only94→2003; both→2001.|
|18 /146|2/3|2000;92→2001; only93→2003; both→2001.|
|19 /147|1/15|2000;91→2001; otherwise unchanged.|

For arbitrary existing W the corresponding results are W OR1/W OR3/W,
including already-held secondary, other priority/AI/neutral/spawn bits.
The direct receipt enumerates all32 selected region-flag combinations
for these three assignments plus15/15, at seven representative existing
words (896 cases). Flags changed between reads remain live; these cases
are static projections, not observed atomic snapshots. A new primary hit
after a secondary hit preserves both bits; clearing flags alone does not
deactivate the combatant. Held flags still activate with checked bits set.

The [accepted extra-slot derivation](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
applies unchanged: GetCombatantEntryAddress accepts160 and subtracts96,
yielding storage64×56+FFE800=FFF600/CURRENT_GOLD. Pseudo-X FFF62E
overlaps deals84/85, pseudo-HP FFF60E deals20..23, pseudo-triggers FFF636
deals100/101, pseudo-activation FFF634 deals96..99. Nonnegative pseudo-X,
nonzero pseudo-HP and held non15 region flags can OR1/3 into Goddess
Staff99 count bits. Formation-only fresh41 sets no region flag and hence
does not produce that write; other valid supplied flag/deals states can.
This is a storage alias, not safe actor160, item delivery or persistence.

## Round prefix, spawning and first control

**Confirmed:** after LoadBattle/empty start, BattleLoop @Start orders:
ActivateEnemies → ExecuteBattleRegionCutscene → optional debug
PrintAllActivatedDefCons → PopulateTargetsListWithSpawningEnemies →
any ascending SpawnEnemyWithCamera calls → GenerateBattleTurnOrder →
individual-turn dispatch. The complete region table contains only32/40;
selected41 therefore executes no region scene regardless of90..94.
There is no hidden41 region program or per-movement scene callback.

The spawn collector masks activation0F00. All selected STARTING words,
including OR1/3, mask0 and skip reset/append; freshly absent slots do too.
It rebuilds length0, so no selected appearance request. Activation and
queue admission are not spawning. Changed0100/0200/0300 states use the
separate dead/trigger/maxHP/occupancy reset rules retained in
[Battle38 activation](../moun-nazca-grans/battle38-activation-and-first-turn-dispatch.md);
this scene does not install those modes. No new appearance is inferred
from activating145/146/147.

### Queue admission, arithmetic and sorting

`battle/battleloop/turnorderfunctions.asm` (`$25544..$25610`) clears64
two-byte records at FFF71A toFFFF. It offers allies0..29, then enemies
128..157, ascending. Only signed-negative X or zero HP skips; Y,
membership, neutral, PRIMARY, sleep/stun and signed-positive HP are not
queue gates. All20 unchanged enemies enter, including the three inactive
ones.148..157 normally skip;158..160 are never offered.

Current rawAGI is live state, not necessarily the definition. With
A=raw AND127 and R(n) the next direct GenerateRandomNumber draw:

- First score is `A + R(A>>3) - R(A>>3) + R(3) - 1`.
- Raw≥128 appends a second record immediately: B=floor(5A/6), score
  `B + R(B>>3) - R(B>>3)`.
- Word arithmetic stores only the low score byte. Skips draw0,
  single candidates3, two-record candidates5 times.

The direct word generator advances seed=(seed×13+7) AND65535 and returns
floor(seed×n/65536) for these ranges; range0 still advances. It bypasses
debug/direction overrides and the thinking RNG copy. Earlier eligible
allies and high bits affect all later draws. Prowess DOUBLE is a different
within-action mechanic and does not append these queue records.

| Kind | Raw / numerical AGI | First draw range | Algebraic first-score envelope |
| --- | --- | --- | --- |
|Reaper,Chaos Dragon|57 /57|7|50..64|
|Blue Shaman|51 /51|6|45..57|
|Demon Master|65 /65|8|57..73|
|Hydra|50 /50|6|44..56|
|Chaos Warrior|54 /54|6|48..60|
|Demon|55 /55|6|49..61|

All selected fresh rawAGI bytes are below128:20 enemy records and60
draws. With N eligible normally placed allies and T high-bit allies,
totals are20+N+T records and60+3N+2T draws; N≤12 gives at most44/120.
These are bounded counts/envelopes, not probabilities, jointly attainable
extrema or a concrete seed/first actor. Other valid live states use C
eligible candidates and D high-bit candidates: C+D and3C+2D. The original
has no arbitrary-state capacity check; sixty high-bit candidates could
write120 records into64 slots. Production validation rejects overflow.

Sorting has62 outer passes and63 adjacent comparisons per pass, signed
score bytes descending. Swap only if next is signed-greater; ties retain
insertion order (allies before enemies, lower IDs, first before second).
FFFF sentinel score−1 participates. Wrapped negative scores can remain
behind a sentinel and never execute; admission is not execution. Cursor
CURRENT_BATTLE_TURN.b resets0 and advances2 after an ordinary completed
turn. Reading actorFF returns to @Start; no immutable first-hand selection
or safe arbitrary retained cursor follows.

### Live dispatch branches

ExecuteIndividualTurn (`$23EB0..$24242`) checks HP again: zero skips.
It sets actor/target X/Y, alias/view target and cursor destination, then
uses live MUDDLE, AI_CONTROLLED bit2, enemy/control-opponent toggle and
ally auto-battle toggle to select StartAiControl preparation. Ordinary
allies otherwise take the player path; controlled opponents can too.
After preparation come cursor/view waits, targeting reset, mini-status/
land-effect windows, movement grid/pulsating blocks and hidden cursor.
SLEEP/STUN then skip action; otherwise predicates are reread to select
ExecuteAiControl or ProcessBattleEntityControlPlayerInput. Sleeping AI
can therefore already have prepared. PRIMARY/AGI alone never grant input.

Ordinary enemy @NonSwarmAi clears checked storage.15/15 then bypasses
PRIMARY; otherwise clear PRIMARY invokes DetermineAiStandbyMovement and
installs STAY. Standby can have a movement interface; it is not absent
from the queue. With only fresh formation positions, rows145..147 take
this inactive path when reached unchanged; rows128..144 use the ordinary
activated-command interface despite their clear PRIMARY bit. Reaper35,
Hydra33 and the other selected kinds are neither Prism Flower, Zeon Guard
nor Burst Rock32. No selected special AI branch substitutes for the
ordinary commandset/pathfinding interfaces. Stop before concrete move,
spell/attack choice, damage, whole-round progress or outcomes. Natural
first actor, completed waits and usable input remain unobserved.

## Actual compiler, importer and production consumers

**Confirmed about current code:** the sole explicit-file OriginalPrograms
observation completed in0.1219s, no errors, retaining30,407 bytes with
all entries/programs/actions/source corpus. bbcs_41 has13 instructions,
cs_4ED48 has31; no native instruction occurs in either. eas_Init has14
actions, no native; the inline Reaper motion has move0,+2, jump-to-idle,
idle. Both conditional entry and final unconditional fallthrough target
cs-4ed48 instruction0; its final instruction is end-map-script. All output
instructions were inspected. These are compiler facts, not scene execution
or a full Map56/41 import. The original corpus records one20-command
scene with local continuation, while lowering splits it into two programs.

| Actual owner | Existing expression and selected41 gap |
| --- | --- |
|[content preparation](../../../../src/sf2tool/remake_exploration_content.py), [canonical importer](../../../../src/sf2tool/h2/map_import.py), [map compiler](../../../../src/sf2tool/remake_exploration_maps.py)|Registered canonical identity and source callbacks/setup metadata can be transported. Real Map56 has a setup route, one NPC, RTS init, areas and fourteen warps. Outside eventMaps, map-56-setup stays native and NPC input stays native: the final overwrite replaces only route-absent setups; Map56's RTS is not the four-operation guard/script shape. Explicit eventMaps56 can bind RTS/end, population, callbacks/actions, but predecessor faceEntity/Church frontiers remain. A standalone opening lowering neither passes this production path nor supplies actual Map56 materials. No map compile/import or prior observation was rerun.|
|[program compiler](../../../../src/sf2tool/remake_exploration_programs.py), [Content reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Scene-map/population, motion/flags/text/camera/presentation all have typed forms for this output. Bare position/face carry no native ally-HP branch; runner cases do not add it. Bound HP0 Peter/Lemon/Zynk or Bowie-facing can therefore differ. Typed presentation requests require admitted resources/actual completion, not compiler acknowledgement. Native-call elsewhere becomes UnsupportedInstruction and refuses before advancement.|
|[allocation](../../../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [SceneEntities](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs)|Follower-first allocation, duplicate-alias replacement, Peter reuse, scene rebuild, fresh zero aliases and sprite waits exist. These mechanisms do not select the incoming flags/HP/class sprites, validate every simultaneous follower combination or implement the missing scene HP guards.|
|[battle preparation/routes](../../../../src/sf2tool/remake_exploration_battles.py), [encounter reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Preparation admits2/3. Route compilation rejects non-FF coordinate triggers **before** hook binding, so41's Y24 is rejected. Reader's private identity pins admit1/2/3 only. Generic unlock/completion/intro and load interfaces do not bind this coordinate predicate, F88 retained state or actual41 terrain/roster.|
|[scenario reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Region-program admission1/2/3; HEALER5 and SENTRY commandsets, ENHANCED99, BIT4 and MAGE mover are selected gaps. STARTING/NOTHING and rectangular record transport exist. Order parser supports MOVE_TO/FOLLOW_ENEMY and plain decimal bytes, but literal `144\|0` returns unresolved null because prefix144 has no branch; do not silently turn it into NONE. Formation-as-capacity and copied equipment/difficulty behavior need the actual admitted path; no full import or first-rejection ordering is claimed.|
|[activation](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [turn rules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [BattleTurnFlow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs)|Sticky OR1/3, four-vertex shapes, sequential RNG, extra-action encoding and stable signed sorting/sentinels exist; GenerateRound activates before RNG. Domain resets local tested0 each round and considers represented living/positioned actors, unlike retained native checked storage/all30/raw signed slots/pseudo160. Cross products are a general geometry difference; these five rectangles agree at the checked integer points. Capacity validation refuses unsafe overflow. Nonzero spawn/program modes refuse before RNG/publication; selected41 itself has no region scene.|
|[SourceEnemyAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [control rules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs), [BattleAdvancer](../../../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs)|Represented tested reset, unconditional15/PRIMARY, standby and followed-target promotion exist. Active commandsets are limited6/7/9/14; SENTRY8/HEALER5=4 and the selected spells/equipped loadouts exceed support. Literal90 needs a valid combatant144 binding, not a point. Application advances admitted rounds/control and preserves committed state on refusal; it does not prove this original partial-prefix/toggle/status parity.|
|[adapter presentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs), [capability owner](../../../../remake/docs/capability-status.md)|Generic scene/text/map and battle projections consume admitted content. No playable56/41, original arrival, audiovisual completion or usable first control was observed. The accepted through-Battle03 runnable boundary is separate.|

**Inferred:** future explicit content work can reuse typed population,
motion, rectangles, sticky activation and queue/control interfaces after
resolving admission, HP/alias semantics, order expression and selected
definitions/AI/materials. This is a gap boundary, not an implementation
task silently added here or an assertion Battle41 is engine-ready.

## Reproduction, preserved failures and stopping boundary

Ignored `local/issue1166/review.py` retains full selected local data,
source/consumer snapshots, ordered operand/branch/call guards, text tokens,
geometry and changed activation states, lowering final-output checks,
relative links, date/private/owned-diff checks. Its first source-guard
attempt failed by stopping at InitializeMapEntities' stub end marker;
the corrected recipe checks the actual loc_440E2 function chunk. Preserve
that completed failure and all read/path lookup failures in the private
handoff; none is a source failure or interrupted run. The one-shot lowering
recipe/result is `observe.py`/`observe-01.json`; do not repeat an unchanged
observation. Source snapshots can be reused only after direct equality.
Use fresh receipt names and load private selections in the launching process:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1166/review.py <configured-source-disasm-root> --receipt local/issue1166/direct-reproduction-01.json --committed --snapshots local/issue1166/direct-02.json
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

ADR0004 adversarial review covers provenance, complete inventory, operand
widths/dispatch, state changes, aliases, initialization/activation/queue
order, consumer/final producer and public/private boundary. Maintained
parser/schema/fixture/tool-test and H3 callback requirements are N/A.
Normal/full verify, old comparisons, SDK/Godot/emulator/H3/H4/screenshots
are **SKIPPED** under explicit documentation-only scope. Committed planner,
actual scoped CI, resource totals, frozen identity and process completion
belong to the Issue/PR handoff, with independent main-gate acceptance.

Preserve #1162's followEntity high-byte/Bowie-HP missed-finding PASS and
accepted correction, including original author/direct-main-gate/CI39725455473
passes and corrected narrow/CI39726160754 results. Preserve #1160's R3
geometry self-review/local failures and direct02 limit, Map54's final
producer overwrite correction, Map53 UnboundLocalError with zero partial
output, and twelve completed old comparison failures. Their owners and
handoffs retain detail. None is repaired, rerun, erased, relabeled
interrupted or upgraded by this slice; they are not a new repair queue.

**Unknown:** natural marker/warp/interaction/admission reach; incoming
flags, membership, HP, statuses, aliases, checked word and RNG; interleaved
or interrupted services; decoded Map56 layout/BattleTerrain41 and collision;
source/ROM equality beyond accepted evidence; actual first actor, camera,
text/portrait/fade/music/render/DMA/VInt/input completion; hardware timing,
save/load persistence and playable56/41. These are not automatic runtime
work. Freeze a clean pushed Draft PR and notify main-gate; no self-merge,
cleanup, archive or next Issue follows.
