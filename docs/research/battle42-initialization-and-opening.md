# Battle42 initialization and opening

Evidence date: 2026-10-22. **Confirmed (static):** the Map58/F999
replacement can admit Battle42 before Battle43. Its complete Galam
opening precedes fresh 12-ally/19-enemy initialization. Four HIDDEN
declarations have HP0/X−1 at this boundary; Elis is reconstructed as a
neutral, independently of scene129 and combat129. Stop immediately
before the first ActivateEnemies call. Natural arrival, completed
presentation and playable remake support remain **Unknown**.

Continue with [Battle42 activation and first control](./battle42-activation-and-first-control.md)
for the four integer regions, hidden spawning, neutral/queue interaction
and the first action-selection boundary.

## Agreement, provenance and method

Standard rigor for [Issue #1170](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1170)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053),
from accepted base `78d663f6fab6e4e2dc8c2cbc9aed6300fd6241a3`, tree
`5bd829731d6585f5d713743c26ab9cab4e228a4a`. The decision supported is
future content's opening, identity/state and initialization contract.
Own this document and successor navigation only in
[Battle41 outcomes/stairway](./battle41-outcomes-and-ancient-tower-stairway.md).
No shared index/schema/fixture/extractor/engine change or stacked dependency.
The longer branched scene, asynchronous actions, hidden inputs and sloped
regions justify separating initialization from activation. Region intersection,
hidden activation/spawning, queue/turn generation, first control, Battle42
outcomes, shipdown and Map59 ending content are outside this agreement.

Reuse [Battle40 initialization](./battle40-initialization-and-opening.md),
[Battle38 hidden initialization](./battle38-initialization-and-opening.md),
[spritesets](./battle-spriteset-data.md), [battle loop](./battle-loop.md),
[scripting](./common-scripting.md), [maps](./common-maps.md) and
[stats](./common-stats.md). Selected42 operands and changed states are
checked here; general mechanisms remain with those owners.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` means `code/gameflow/battle/`; `map/` and `entity/` mean
children of `code/common/scripting/`. sf2const.asm, sf2enums.asm,
sf2macros.asm, sf2mapmacros.asm and sf2cutscenemacros.asm own constants
and encoding. Fnnn denotes a story flag. Ordinals are zero-based,
address intervals half-open, addresses and packed words hexadecimal.
USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source/ROM equality or original-runtime observation is claimed.

Minimum evidence is the complete selected scene/formation and necessary
shared callers/actual consumers. One read of the unchanged18,285,979-byte
static init/script pair, with `local/issue1005/pair.txt` provenance,
supports one selected lowering of42 before and its local continuation.
No pair regeneration, real layout/terrain decode, full-world import,
asset export or old41/Map56 observation. Work is linear in selected
text/table size, seconds per direct check, selected receipts below2MiB
and total including coordination/CI below4MiB; peak-memory target512MiB
**unmeasured**. Ignored recipes/results belong to `local/issue1170/`.
Reassess before runtime, cross-system machinery or material budget growth.

## Receiving Map59 and the two different entry paths

**Confirmed:** the predecessor owns Map60's declared58,13,36/Up warp.
MainLoop runs SwitchMap before CheckBattle: held999 replaces58 with59
while preserving coordinates/facing. Clear999 leaves the old7 receiver.
`data/battles/global/battlemapcoords.asm` rows42 then43 are both
`59,0,0,28,38,255,255`;42's row is `$7B5C..$7B63`. In
`code/common/maps/getbattle.asm`, live442 admits42 before443 can admit43.
The crop0,0/28×38 is written before testing542; held542 clears442
but still returns42 **on this call**. Clear442 permits the later43 row;
neither held unlock returns−1. Completion, intro and unlock are separate.
No flags' natural arrival values are inferred from the story.

MainLoop puts selected42 in d1.w and passes resolved map59 in d0.w
to j_BattleLoop. BattleLoop's direct before call does **not** execute
ordinary exploration's setup init. By contrast, the no-encounter path
rebuilds ordinary Map59, chooses setup from `data/maps/mapsetups.asm`,
then runs that setup's init after ordinary population/layout/services:

| Actual setup / source entry | Receiving behavior, bounded at its calls |
| --- | --- |
|Default ms_map59, pointer table `$5EAA8..$5EAC0`; entities `$5EAC0..$5EAC2`|Empty fixed table; ordinary followers/Bowie still rebuild. Init `$5EAD8`: clear999 calls cs_5EB44,cs_5EBFC,cs_5ED06,cs_5EDB8,cs_5EF60 in order, then sets999, EGRESS_MAP=DOJO,436,440,442,443. These earlier-scene bodies are excluded.|
|Same default,999 held|Test443: clear returns; held calls cs_5EB26, which calls csub_5EB34 (32 long clears at PALETTE_1_BASE), requests explicit59,13,34/Up and ends. This is not Battle42 before or evidence of the warp's completed return.|
|Held543 selects ms_map59_flag543, pointer table `$6153E..$61556`|Eleven fixed rows `$61556..$615B0`: four parked effects/objects, Elis, Peter, Lemon, Astral, King Galam, parked Mitula and Woman. Init `$615D8` rereads543: clear returns, held calls cs_615E6. Stop at that ending-scene interface; its body is excluded.|

Selection and init each read live flags. Changing543 between them can
select the ending population but skip its scene; changing999/443 chooses
its own default branch. Neither setup is interchangeable with ce_4EEDC,
and Battle42 before neither calls these init functions nor sets999/442/443.
The global F542 layout copy below is independent of setup543.

## Fresh initialization, hooks and suspended entry

**Confirmed:** `battle/battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`)
clears PLAYER_TYPE.b, sets399 and tests88. Clear88 clears seconds,
saves incoming d0/d1 **words**, writes their low bytes to CURRENT_MAP/
CURRENT_BATTLE, installs base VInt services and calls before. After
return, it restores the saved words and rewrites those selector bytes;
it does not undo scene entity/layout/flag/service mutations. It clears
region flags90–105 inclusive, heals living/immortal allies, initializes
all ally positions, all enemy positions and AI memory, in that order.
Then d0.w=0, LoadBattle, start wrapper, and the excluded ActivateEnemies.

| Complete48-relative-word hook table / selected byte84 | Target |
| --- | --- |
|beforebattlecutscenes.asm base `$47A88`, slot `$47ADC`|bbcs_42 `$4ED8E`|
|battlestartcutscenes.asm base `$47B2C`, slot `$47B80`|ms_Empty, sole FFFF word|

The before wrapper tests450+42=492, skips when held and never sets it.
The start wrapper tests492 independently and sets it **before** the
empty script. Empty ExecuteMapScript still resets dword_FFB1A4 to
FF9004_LOADING_SPACE, clears text-skip, conditionally waits for scrolling
when a dialogue window is open and clears view speed. Debug P2 Start
can reassert text-skip after its initial clear. Skipping a wrapper body
bypasses those interpreter effects.

| Supplied state at the named reads | Result, if called services return |
| --- | --- |
|88 clear,492 clear throughout|Complete before, fresh reconstruction/load, set492, empty start.|
|88 clear,492 held throughout|Both hook bodies skip; fresh reconstruction/load remains.|
|542 held while442 admits|Clear442 on admission; intro492 remains independently tested. F542 copy and neutral omission apply at their own reads.|
|492 changes between wrappers|Each wrapper obeys its read; no snapshot or completion latch across both.|
|Selector changes within services|Helpers use live selectors; saved-word restoration repairs only the named current bytes afterward.|
|Interrupted prefix|Scene effects may exist with492 clear, or492 held before start returns. No atomicity, save or control-ready claim.|

Held88 restores saved seconds, clears88, clears AI memory and view
target, calls LoadBattle with **unchanged incoming d0**, then branches
to @IndividualTurns_Loop. The following branch to @Start is unreachable.
It bypasses selector assignment, both hooks, region reset, healing,
formation/stat initialization and generation. Retained selectors,
combatants/crop/flags/queue/cursor/RNG remain inputs, not installed59/42.

For incoming d0.w=59, LoadBattle saves59 across PositionBattleEntities.
GetEntityIndexForCombatant in battlefunctions_0.asm tests the low byte:
59 is nonnegative, so no enemy96 subtraction. FF masking yields offset59,
`$FFB140+59=$FFB17B`, inside the64-byte alias table ending `$FFB180`.
It is enemy alias27/logical155, **not ally59 or an out-of-table read**.
Fresh42 marks155 absent with X−1, so the rebuilt byte isFF; changed
retained position/neutral state can populate it. Packing has no HP test.
The fetched byte T is stored as view target; negative T uses physical0
for coordinates, otherwise T&3F selects a record without a49-slot guard.
This arithmetic is not an observed saved byte or safe suspend proof.
ClearAiMemory writes48 last-target bytesFF and48 memory bytes0
(`$FFB1AC..$FFB20C`), not the newly-triggered word at20C..20E,
region flags or turn buffer. Stop before consuming the retained turn.

## Scene population and changed-state identity

**Confirmed:** `data/battles/entries/battle42/cs_beforebattle.asm`
occupies `$4ED8E..$4EF04`. Its61 top-level commands occupy288 bytes;
seven move/end payloads add28 and one custom action/delimiter adds18,
giving334 bytes up to ce_4EEDC. The40-byte table then ends at4EF04:
six-byte main header, four eight-byte fixed records, two-byte terminator.

| Encoded address / width | Complete entity table |
| --- | --- |
|4EEDC /6|mainEntity13,35,Up → Bowie0, actual ally sprite, eas_Idle|
|4EEE2 /8|12,36,Up, ALLY_PETER7,eas_Init|
|4EEEA /8|13,36,Up, ALLY_LEMON28,eas_Init|
|4EEF2 /8|13,18,Down, EVIL_GALAM164,eas_Init → scene128|
|4EEFA /8|13,15,Down, ELIS204,eas_Init → scene129|
|4EF02 /2|FFFF terminator|

mainEntity fields are three words; fixed fields are X/Y/facing/sprite
bytes plus script long. loadMapFadeIn59,8,29 requests fade/map/view
services through csc37→csc48; it does not run exploration setup.
CURRENT_BATTLE is already42, so then-live battle crop/dimensions apply.
csc42 replaces population via InitializeMapEntities/LoadEntityMapsprites.
ClearEntities resets49 physical records,64 aliases to0 and walking
scratch. Followers allocate first; fixed ally records reuse an existing
nonzero alias and skip that record's position/script installation.

Map59 is absent from the complete13-entry overworld map table, so even
held65 selects the **19 ordinary follower rows**, not the Caravan prefix.
Their complete ordering is owned by `data/scripting/entity/followers.asm`.
Every held row allocates, including repeated logical aliases whose last
write wins; force membership is not its predicate. FOLLOWER_B31 can
resolve to Astral/F74, Oddler/F68 or Astral/F70 in table order; without
any producer its fresh0 alias addresses Bowie. FOLLOWER_A30 can also
be populated, but this scene issues no explicit30 command. Other held
followers retain their own scripts unless a later alias targets them.
F67 Peter and F71 Lemon can allocate before their fixed records, which
then reuse them; otherwise the fixed records allocate them regardless
of joined/active membership. Ally sprites use live class/Rohde selection;
outside-battle HP0 flame selection does not apply with CURRENT_BATTLE42.

With n followers and p/l equal1 when fixed Peter/Lemon newly allocate,
Bowie is physical0, Galam n+p+l+1 and Elis n+p+l+2. Actual scripts,
positions, follower pointer-table capacity and arbitrary combined flags
are not inferred safe. Physical slots and combatant IDs are distinct.

| Changed state | Selected command effect |
| --- | --- |
|F76 clear|Skip both Zynk Init and63,62/Down. No conditional scene Zynk command is reached.|
|F76 held|Ordinary row allocates Zynk26; explicit Init replaces its follower script, then setPos requests63,62/Down if HP!=0. It does not join/activate him.|
|Peter/Lemon HP0|Init still installs; setPos skips its four-byte payload. Later fresh immortal healing does not retroactively reposition the scene.|
|Bowie HP0|Main declaration still populates physical0; later Init/actions have no logical-ally HP guard. BattleLoop heals only later.|
|31 absent, alias0|Explicit Init/position/jump/actions/shiver can operate Bowie; logical31 bypasses the0–29 HP guard even if Bowie HP0.|
|Peter/Lemon already followers|Fixed rows reuse their prior position/script; explicit Init/position then apply with their separate HP rules.|
|Two logical IDs resolve to one physical slot|Later installations replace that one pointer; a wait belongs only to that resolved slot.|

No opening command grants items/HP, recruits, clears/sets76 or resets
force battle statistics. Scene128 Galam and129 Elis are not initialized
combat128/129; those later denote Galam97 and Blue Shaman85. The final
neutral Elis uses combat159's backing state, script alias31 and a newly
declared physical entity; those three identities must stay distinct.

## Complete opening order, encodings and text requests

B/P/F/L/Z below mean Bowie0/Peter7/FOLLOWER_B31/Lemon28/Zynk26.
I/W denote setActscript/setActscriptWait; A/AW denote entityActions/
entityActionsWait. Text S/T denote nextSingleText/nextText. Addresses
include inline storage; widths in the command column exclude payloads,
which are separately named. cs_4EDE8 is the local target at index13.

| Index / address | Command (bytes) and owned inline payload |
| --- | --- |
|0 /4ED8E|textCursor 3088 (4)|
|1 /4ED92|loadMapFadeIn MAP_ZEON_BATTLEFIELD,8,29 (8)|
|2 /4ED9A|loadMapEntities ce_4EEDC (6)|
|3 /4EDA0|setActscriptWait ALLY_BOWIE,eas_Init (8)|
|4 /4EDA8|setActscriptWait ALLY_PETER,eas_Init (8)|
|5 /4EDB0|setPos ALLY_PETER,12,36,UP (6)|
|6 /4EDB6|setActscriptWait FOLLOWER_B,eas_Init (8)|
|7 /4EDBE|setPos FOLLOWER_B,14,36,UP (6)|
|8 /4EDC4|setActscriptWait ALLY_LEMON,eas_Init (8)|
|9 /4EDCC|setPos ALLY_LEMON,13,36,UP (6)|
|10 /4EDD2|jumpIfFlagClear 76,cs_4EDE8 (8)|
|11 /4EDDA|setActscriptWait ALLY_ZYNK,eas_Init (8)|
|12 /4EDE2|setPos ALLY_ZYNK,63,62,DOWN (6)|
|13 /4EDE8|cs_4EDE8: stopEntity 129 (4)|
|14 /4EDEC|customActscriptWait 129; inline@4EDF0: OFF/orientLeft/refresh/jumpIdle/ac_end (18) (4)|
|15 /4EE02|playSound MUSIC_WITCH (4)|
|16 /4EE06|fadeInFromBlackHalf (2)|
|17 /4EE08|cameraSpeed $30 (4)|
|18 /4EE0C|csWait 60 (2)|
|19 /4EE0E|entityActionsWait ALLY_PETER; inline@4EE12: moveUp2,endActions (4) (4)|
|20 /4EE16|nextSingleText $0,ALLY_PETER (4)|
|21 /4EE1A|nextSingleText $C0,128 (4)|
|22 /4EE1E|setActscript ALLY_BOWIE,eas_Jump (8)|
|23 /4EE26|setActscript ALLY_PETER,eas_Jump (8)|
|24 /4EE2E|setActscript FOLLOWER_B,eas_Jump (8)|
|25 /4EE36|setActscriptWait ALLY_LEMON,eas_Jump (8)|
|26 /4EE3E|entityActionsWait FOLLOWER_B; inline@4EE42: moveUp3,endActions (4) (4)|
|27 /4EE46|nextSingleText $0,FOLLOWER_B (4)|
|28 /4EE4A|nextSingleText $C0,128 (4)|
|29 /4EE4E|fadeInB (2)|
|30 /4EE50|nextSingleText $C0,128 (4)|
|31 /4EE54|entityActions ALLY_BOWIE; inline@4EE58: moveUp2,endActions (4) (4)|
|32 /4EE5C|entityActionsWait ALLY_LEMON; inline@4EE60: moveUp2,endActions (4) (4)|
|33 /4EE64|shiver FOLLOWER_B (4)|
|34 /4EE68|nextSingleText $0,FOLLOWER_B (4)|
|35 /4EE6C|entityActions ALLY_BOWIE; inline@4EE70: moveLeft1,endActions (4) (4)|
|36 /4EE74|entityActionsWait ALLY_LEMON; inline@4EE78: moveUp1,endActions (4) (4)|
|37 /4EE7C|setFacing ALLY_BOWIE,UP (4)|
|38 /4EE80|entityActionsWait ALLY_LEMON; inline@4EE84: moveUp1,endActions (4) (4)|
|39 /4EE88|nextSingleText $0,ALLY_LEMON (4)|
|40 /4EE8C|setCamDest 8,13 (6)|
|41 /4EE92|nextSingleText $C0,128 (4)|
|42 /4EE96|nextSingleText $0,FOLLOWER_B (4)|
|43 /4EE9A|nextText $C0,128 (4)|
|44 /4EE9E|nextSingleText $C0,128 (4)|
|45 /4EEA2|nextSingleText $0,ALLY_PETER (4)|
|46 /4EEA6|setFacing 128,UP (4)|
|47 /4EEAA|csWait 40 (2)|
|48 /4EEAC|setFacing 128,DOWN (4)|
|49 /4EEB0|nextSingleText $C0,128 (4)|
|50 /4EEB4|nextText $0,FOLLOWER_B (4)|
|51 /4EEB8|nextSingleText $0,FOLLOWER_B (4)|
|52 /4EEBC|nextText $C0,128 (4)|
|53 /4EEC0|nextText $C0,128 (4)|
|54 /4EEC4|nextSingleText $C0,128 (4)|
|55 /4EEC8|setQuake 3 (4)|
|56 /4EECC|playSound SFX_INTRO_LIGHTNING (4)|
|57 /4EED0|flashScreenWhite 30 (4)|
|58 /4EED4|csWait 20 (2)|
|59 /4EED6|setQuake 0 (4)|
|60 /4EEDA|csc_end (2)|

The seven inline move streams encode direction.b,count.b followed by
8080.w; each is four bytes. The custom stream at4EDF0 encodes
ac_motion OFF (4 bytes), ac_orientLeft (4), ac_updateSprite (2),
ac_jump eas_Idle (6), ac_end8080 (2). Its18 bytes end at4EE02;
the jump, rather than interpreting8080 as an action, reaches idle.
All commands have opcode.w except csWait's word containing the low-byte
sleep amount; branch flag is word plus absolute target long; I/W have
entity/control bytes plus script long; A/AW and custom have entity/control
bytes; positions have entity/X/Y/facing bytes; text has modifier/entity
bytes. setCamDest has two word operands. Text cursor is a word.

### Actual action and presentation boundaries

**Confirmed:** csc1C stopEntity takes entity **word129**, applies its
logical-ID HP predicate, resolves the slot and writes ANIMCOUNTER.b=FF.
129 bypasses the ally0–29 HP guard. It neither clears velocities nor
waits for an idle script. The following csc14 custom command takes
entity.b129/control.bFF, resolves that slot and writes its physical slot
byte into ACTSCRIPTWAITTIMER, then installs the inline pointer. It waits
only for that pointer to equal eas_Idle and scans words through8080.
esc14 for ac_motion OFF also writes ANIMCOUNTER=FF; esc1B orientLeft
sets the orientation bits, refresh requests sprite work, then jump idle.
This is animation-counter control, not zero movement or autoFacing OFF.

eas_Init sets acceleration factors0, speeds32/32, acceleration and
deceleration OFF, obstruction ON, clears entity/map collision-check
bits5/6, autoFacing ON, Up, nonimmersed, size24, refresh and idle.
csc15 I/W installs pointer and physical-slot timer without HP guard.
csc19 setPos uses the actual byte operand and skips its four-byte
payload only for HP0 logical allies0–29;31/128/129 bypass that test.
csc23 setFacing likewise takes the entity byte, skips its two-byte
payload for HP0 Bowie0, while128's Up/Down writes bypass that ally guard.
No absent-alias validation precedes GetEntityAddressFromCharacter.

eas_Jump at45E44 is autoFacing OFF; relative0,−1; wait5;
relative0,+1; wait5; relative0,0; autoFacing ON; jump idle.
It never calls ac_motion OFF. Relative destinations use the then-current
position, and these ac_moveRel operations do not wait for destination.
Lemon's W at index25 is not a barrier for Bowie's/Peter's/31's jumps;
the subsequent31 motion can replace an unfinished31 jump. Net symbolic
offsets alone do not prove return coordinates.

csc2D A/AW installs a slot timer, clears collision bits with9F,
generates relative move/wait-destination code and a jump idle. Each AW
waits only its slot pointer. Under distinct aliases, living explicit
actors and completed requested moves, nominal ordinary-move endpoints
are P12,34, F14,33, B12,33, L13,32; the intervening jumps/asynchronous
execution are additional conditions, so these are not observed finals.
csc2A shiver31 has **no ally-HP guard**: save sprite size/counter,
set size21/counterFF, toggle flagsB bit3 three times with Sleep5/5,
refresh sprites, restore size/counter. It does not stop other scripts.

csc4A half fade calls LaunchFading with palette maskF, period6,
HALF_IN_FROM_BLACK14; later fadeInB requests the separate full black
fade-in. cameraSpeed writes VIEW_SCROLLING_SPEED.w=0030. setCamDest8,13
detaches the view target, requests the destination and waits for view
scrolling, without changing crop/entity speed or joining actor motions.
setQuake3/0 directly writes QUAKE_AMPLITUDE.w (no ramp bits).
flashScreenWhite30 uses csc41's operand>>3 DBF counter: four
LaunchFading calls with maskF/period1/FLASH_QUICKLY_2, then
DuplicatePalettes once. LaunchFading waits for FADING_SETTING clear
through VInt, waits once more and restores the prior counter maximum
and palette mask1111. Thirty is not thirty measured white frames.
csWait60/40/20 uses low-byte Sleep, skipped under debug text-skip.
Music/SFX requests do not prove audio completion. Script end's conditional
view wait and speed clear supply no global motion/presentation/input join.

### All18 text IDs, modifiers and control tokens

**Confirmed:** cursor3088 advances through3105. Only IDs, authored tokens
and project-authored summaries are public. C0 means portrait mirrored40
and right-side80;00 is the ordinary modifier. S closes the single text
request and sleeps10 after display/close; T continues. Both wait for view
scroll before DisplayText. These IDs describe normal unskipped execution:
debug text-skip advances the operand without displaying or incrementing
the cursor. Actual rendering/input/timing is not observed.

| ID | Request / speaker | Complete ordered tokens | Author summary |
| --- | --- | --- | --- |
|3088|S00/P|N,W1|Peter asks about the darkness and company.|
|3089|SC0/128|W1|Galam welcomes the group.|
|3090|S00/F|W1|Follower recognizes Galam.|
|3091|SC0/128|N,W1|Galam offers light.|
|3092|SC0/128|W1|Galam asks about visibility.|
|3093|S00/F|W1|Follower addresses Galam.|
|3094|S00/L|W1|Lemon notices Elis.|
|3095|SC0/128|N,NAME;28,N,W1|Galam greets Astral and names member28.|
|3096|S00/F|W1|Follower asks for Zeon.|
|3097|TC0/128|W2|Galam describes incomplete revival.|
|3098|SC0/128|N,W1|Galam explains his welcome.|
|3099|S00/P|N,W1|Peter questions Elis's stillness.|
|3100|SC0/128|N,N,W1|Galam describes the intended sacrifice.|
|3101|T00/F|W2|Follower reacts.|
|3102|S00/F|N,W1|Follower cites the jewel bargain.|
|3103|TC0/128|W2|Galam disavows the promise.|
|3104|TC0/128|N,N,W2|Galam objects to the party's victories.|
|3105|SC0/128|N,W1|Galam threatens to prevent further progress.|

## Complete12/19/4/1 initialization inputs

**Confirmed:** `data/battles/spritesets/spriteset42.asm` is
`$1B68B0..$1B6A5A` (426 bytes). Header bytes12,19,4,1 occupy4;
12 ally records144,19 enemy records228,4 region records48 and one
two-byte point complete the span, with no additional terminator.
Each12-byte combatant row is identity/X/Y/AI bytes, placement-item word,
primary order/region and secondary order/region bytes, then filler/spawn
word. Formation ordinal is not a fixed party member. All12 ally rows
have HEALER1=0, NOTHING007F, FF/15,FF/15, filler0/STARTING0.

| Ordinal / address | X,Y |
| --- | --- |
|0 /1B68B4|13,32|
|1 /1B68C0|14,33|
|2 /1B68CC|13,33|
|3 /1B68D8|12,33|
|4 /1B68E4|11,33|
|5 /1B68F0|14,32|
|6 /1B68FC|12,32|
|7 /1B6908|11,32|
|8 /1B6914|14,34|
|9 /1B6920|13,34|
|10 /1B692C|12,34|
|11 /1B6938|11,34|

Every enemy placement item is NOTHING007F, filler0, secondary order
NONE=FF. MOVE_TO0=40, FOLLOW_ENEMY0=80, FOLLOW_ENEMY12=8C and
FOLLOW_ENEMY15=8F are **byte orders**, not resolved movement paths.
S=STARTING0 and H=HIDDEN2. AI values are placement commandset bytes.

| Ordinal / logical / address | Kind(ID),X,Y | AI(byte) | Primary order/region; secondary region | Spawn |
| --- | --- | --- | --- | --- |
|0 /128 /1B6944|GALAM(97),13,18|LEADER(14)|40/0;1|S|
|1 /129 /1B6950|BLUE_SHAMAN(85),12,17|HEALER5(4)|80/0;1|S|
|2 /130 /1B695C|DEMON_MASTER(75),14,17|ATTACKER3(9)|80/0;1|S|
|3 /131 /1B6968|DEVIL_GRIFFIN(54),9,13|ATTACKER1(6)|FF/15;15|S|
|4 /132 /1B6974|DEVIL_GRIFFIN(54),17,17|SENTRY(8)|FF/3;15|H|
|5 /133 /1B6980|REAPER(35),16,18|HEALER1(0)|FF/3;15|H|
|6 /134 /1B698C|EVIL_BEAST(36),9,17|SENTRY(8)|FF/2;15|H|
|7 /135 /1B6998|HORSEMAN(66),8,18|SENTRY(8)|FF/2;15|H|
|8 /136 /1B69A4|REAPER(35),10,20|SENTRY(8)|FF/15;15|S|
|9 /137 /1B69B0|EVIL_BEAST(36),12,21|SENTRY(8)|FF/15;15|S|
|10 /138 /1B69BC|DEMON_MASTER(75),3,19|ATTACKER3(9)|8C/0;15|S|
|11 /139 /1B69C8|EVIL_BEAST(36),6,20|ATTACKER3(9)|8C/0;15|S|
|12 /140 /1B69D4|CHAOS_WARRIOR(34),4,21|ATTACKER1(6)|FF/0;15|S|
|13 /141 /1B69E0|HORSEMAN(66),19,19|ATTACKER3(9)|8F/1;15|S|
|14 /142 /1B69EC|REAPER(35),18,20|ATTACKER3(9)|8F/1;15|S|
|15 /143 /1B69F8|CHAOS_DRAGON(53),21,21|ATTACKER1(6)|FF/1;15|S|
|16 /144 /1B6A04|REAPER(35),11,26|ATTACKER1(6)|FF/15;15|S|
|17 /145 /1B6A10|CHAOS_WARRIOR(34),13,26|ATTACKER1(6)|FF/15;15|S|
|18 /146 /1B6A1C|HORSEMAN(66),12,27|ATTACKER1(6)|FF/15;15|S|

Regions are raw12-byte records: kind4,padding0,four ordered X/Y pairs,
trailing0,0. Preserve their sloped edges and order; no intersection,
inclusion or activation result is calculated here.

| Region / address | Complete12 bytes, grouped only for reading |
| --- | --- |
|0 /1B6A28|4,0;0,0;0,31;25,22;25,0;0,0|
|1 /1B6A34|4,0;0,0;0,21;25,32;25,0;0,0|
|2 /1B6A40|4,0;7,15;7,18;11,20;18,15;0,0|
|3 /1B6A4C|4,0;11,15;11,20;18,18;18,15;0,0|
|Point0 /1B6A58 /2 bytes|13,18|

### Definition copies, equipment and current stats

**Confirmed:** InitializeEnemyStats in initializecombatants.asm copies
56-byte enemydefs records in14 longwords, sets HP/MP to maxima, X/Y,
movement/placement AI, kind, triggers/orders, placement item, activation
word, difficulty ATT and UpdateCombatantStats in that order. Base
`table_EnemyDefinitions=$1B1A66`, record address `base+56×kind`;
selected identities are below. All nine have ENHANCED99 class and
initialStatus NONE. This full definition path is reached for15 STARTING
enemies; the four HIDDEN definitions remain **future inputs**, not copied
current state at this stop.42 is absent from the complete random-upgrade
list3,8,9,10,14,17,19,21,24,26,35; completion542 does not admit it.

| Kind / ID / sprite | Level,HP,MP,baseATT,DEF,rawAGI,MOV | Mover | First copied item / spells (remaining slots NOTHING) |
| --- | --- | --- | --- |
|Galam97 /164|99,240,180,49,50,208,6|REGULAR|Demon Rod101 equipped /DEMON1|
|Blue Shaman85 /149|42,72,60,64,54,51,5|HEALER|Holy Staff97 equipped /AURA3,BLAST4|
|Demon Master75 /139|42,73,68,48,50,65,6|MAGE|Mystery Staff100 equipped /FREEZE4|
|Devil Griffin54 /118|42,89,0,96,47,59,7|FLYING|none /FLAME3|
|Reaper35 /99|41,82,31,60,49,57,6|REGULAR|Battle Sword66 equipped /MUDDLE2|
|Evil Beast36 /100|42,84,0,98,51,60,6|REGULAR|none /none|
|Horseman66 /130|41,67,0,59,57,55,7|CENTAUR_ARCHER|Buster Shot52 equipped /none|
|Chaos Warrior34 /98|40,76,23,92,50,54,5|REGULAR|none /ATTACK1|
|Chaos Dragon53 /117|41,83,0,93,48,57,6|FLYING|none /SNOW2|

| Kind | Resistance | Prowess critical,double,counter | Definition AI → STARTING activation |
| --- | --- | --- | --- |
|Galam|WIND/LIGHTNING/ICE/FIRE_MAJOR,STATUS_IMMUNITY|125_1IN4,1IN8,1IN4|1000→1000|
|Blue Shaman|0|SILENCE,1IN32,1IN32|3000→3000|
|Demon Master|LIGHTNING_MINOR,STATUS_IMMUNITY|150_1IN32,1IN32,1IN16|2000→2000|
|Devil Griffin|WIND_WEAKNESS,ICE_WEAKNESS,FIRE_MAJOR,STATUS_IMMUNITY|150_1IN8,1IN16,1IN8|2010→2000|
|Reaper|STATUS_MAJOR|MUDDLE,1IN16,1IN16|2000→2000|
|Evil Beast|0|SLEEP,1IN16,1IN16|2000→2000|
|Horseman|0|150_1IN8,1IN16,1IN32|2010→2000|
|Chaos Warrior|STATUS_MAJOR|STUN,1IN16,1IN16|2000→2000|
|Chaos Dragon|LIGHTNING_WEAKNESS|150_1IN4,1IN32,1IN16|2010→2000|

Activation preserves the definition's upper priority nibble, strips BIT4,
and combines rotated/masked filler/spawn. STARTING0/filler0 adds0;
priority1000 is not PRIMARY_ACTIVE bit0. unknownByte is0 for Galam/
Blue Shaman, otherwise the selected kind ID. Copied equipped words
are Galam00E5, Blue Shaman00E1, Demon Master00E4, Reaper00C2,
Horseman00B4; remaining slots007F. All NOTHING placements skip
InitializeEnemyItems' AddItem path, preserving these copies.

UpdateCombatantStats refreshes already-equipped items without a new
equip-class check. Demon Rod adds ATT50 and decreases AGI10; Holy Staff
adds29 plus HP_RECOVERY3; Mystery Staff adds39 plus MP_RECOVERY2;
Battle Sword adds35 and Buster Shot37. Recovery entries are no-ops
in the stat-refresh table, not extra initial HP/MP. Demon Rod's CURSED
type does **not** curse enemy Galam: ApplyItemOnStats' enemy branch
returns its cleared test result. All selected current statuses remain0.
Item101 is WIZ/SORC/VICR,range1–1,price12500,WEAPON|RARE|CURSED|
BREAKABLE,use SPOIT; the copied enemy item does not prove future use,
breakage, drop or class99 eligibility. Other equipment definitions remain
with [Battle40](./battle40-initialization-and-opening.md#fresh-stats-holdings-and-reconstruction)
and [Battle41 staff](./battle41-outcomes-and-ancient-tower-stairway.md#holy-staff-transaction-and-limits).

Difficulty's duplicated conditional applies floor(baseATT×5/4) once
for every difficulty, twice with intermediate truncation for SUPER;
equipped ATT is added after. Galam's AGI requires the full refresh path:
InitializeCurrentStats copies D0(208); zero Boost's IncreaseCurrentAgi
uses IncreaseAndClamp7Bits and preserves high80 plus low50 (decimal80).
Zero Slow still
calls DecreaseCurrentAgi→DecreaseAndClampByte, whose unsigned upper
cap CHAR_STATCAP_AGI_DECREASING=200 changes208→200 even for decrement0.
The later rod decrement10 changes200→190=BE, high bit80 and low62.
It is neither unchanged208 nor a direct208−10 result198. This is a
static current-byte derivation; no queue size/order or double-action
timing is claimed. TWO_TURN_THRESHOLD80/TURN_AGILITY_MASK7F are future
turn-generation inputs only.

| Kind | Refreshed currentATT ordinary /SUPER | Current rawAGI |
| --- | --- | --- |
|Galam|111 /126|190 (BE; low62, high80)|
|Blue Shaman|109 /129|51|
|Demon Master|99 /114|65|
|Devil Griffin|120 /150|59|
|Reaper|110 /128|57|
|Evil Beast|122 /152|60|
|Horseman|110 /128|55|
|Chaos Warrior|115 /143|54|
|Chaos Dragon|116 /145|57|

DEF/MOV/resistance/prowess otherwise retain the selected values. Hidden
rows are not credited with these refreshed values before spawning.

## Fresh population, neutral Elis and map rebuild

**Confirmed:** HealLivingAndImmortalAllies visits all30 allies. Living
allies plus immortal Peter7/Lemon28 heal to maxima regardless of party
membership; other dead allies skip refresh. STUN/POISON/CURSE survive
the native heal mask, and refresh reconstructs equipment curse. There
is no scene resetForceBattleStats; EXP/kills/defeats/items/membership
are not generally reset.

InitializeAllAlliesBattlePositions first writes X−1 to all30, rebuilds
force lists, then visits at most12 active entries in ID order. A dead
member consumes an iteration without consuming a formation row; excess
active entries LeaveBattleParty without a second list rebuild. Empty
membership retains the existing N−1 underflow boundary. Y and unrelated
fields are not cleared by the X-only reset. Formation rows therefore
do not promise Bowie/Peter/Lemon or twelve living members.

InitializeAllEnemiesBattlePositions visits all32 IDs128–159. Declared
HIDDEN ordinals4–7/logical132–135 read mode2, set activation0200,
maxHP/currentHP0, X−1 and their own triggers3/15,3/15,2/15,2/15.
No Y/kind/class/items/spells/MP/orders/mover/priority definition is copied;
these may be stale. Initial zero HP is not a defeated initialized
Devil Griffin/Reaper/Evil Beast/Horseman. Their declarations are inputs
to later activation/spawn owners, which are excluded here.

Absent147–159 fail the19-count comparison **before** advancing the
enemy pointer: activation0000, max/currentHP0, X−1, triggers0/1
from the first Galam row. Other fields remain stale. Neutral processing
later overrides159 when542 is clear; it is a second producer.

PositionBattleEntities clears scene population/aliases, packs all allies
then enemies by signed nonnegative X/Y and enemy neutral bit, **without
HP/membership/activation-primary tests**. It masks coordinates3F and
sets eas_Standing/Down for ordinary combat sprites. With A eligible
allies, the15 STARTING enemies occupy A..A+14;132–135 and147–159
initially get aliasFF. FOLLOWER_A/B aliases30/31 remain fresh0.
No selected combat sprite uses the special240 path. Scene Galam/Peter/
Lemon/followers and their scripts/positions are not retained.

The live completed flag542 then gates the neutral table. Clear542
selects `data/battles/global/battleneutralentities.asm`'s battle42 record
at `$44960..$4496C`:
battle.w42, X13/Y15/facingDown/spriteELIS204 bytes, eas_LyingLeft.l,
FFFF.w terminator (12 bytes including battle and terminator). It changes
combat159 max/currentHP0,status0, activation OR NEUTRAL0008,
X13/Y15, then declares physical A+15. After resetting a1 to
ENTITY_INDEX_LIST_ENEMIES=$FFB160, its literal predecrement alias write
targets$FFB15F, offset31/script FOLLOWER_B, **not offset63/enemy159**.
The enemy scan left159's offset63 atFF, and that remainsFF;
GetEntityIndexForCombatant(159) therefore does not recover Elis.
Fresh159 activation becomes0008; alias30 stays0. Other159 fields
remain stale. eas_LyingLeft orientsLeft, refreshes,
falls through ac_motion OFF and branches idle; it does not initialize
an enemy definition. Held542 skips all of this neutral mutation and
allocation;159 stays HP0/X−1/activation0, alias31 stays0, even though
the fresh before can still display scene Elis.
At A≤12, maximum physical count28 with Elis,27 without, below49.
Changed selectors or retained F88 states have their own eligibility.

LoadBattle (`$25610..$256A2`) fades/loads tilesets, rebuilds entities,
uses fresh d0.w=0 to select combatant0's actual alias (negative fallback
physical0), derives chosen coordinates by384, initializes sprites,
loads map, VInt/mapsprites, base services, BattleTerrain42, map music
and fade-in. `terrainentries.asm` slot42 selects BattleTerrain42;
`custombackgrounds.asm` selects VERSUS_GALAM and leader-presence byte
is−1. Battle44 timer exception does not apply.

`code/common/maps/mapload.asm:LoadMapBlocksAndLayout` loads blocks,
decodes base working layout, applies flag copies, then chest-open writes
and battle crop. Map59 `3-flag-events.asm` `$B939E..$B93A8` contains
one eight-byte row and FFFF.w: F542.w, source12,54, size7,4,
destination10,3 (bytes; words0C36,0704,0A03). Held542 copies28
layout words/56 content bytes in the64-wide working buffer; clear542
omits it. Ten bytes of table storage are not56 transferred bytes.
Battle crop0,0/28×38 copies the working rectangle to0,0; the source
patch lies outside the crop and destination inside it. BattleTerrain42
is independently loaded tactical data; this layout copy neither proves
a tactical terrain mutation nor collision/visual geometry.

Before's loadMap59 already uses then-live battle42 crop; final LoadBattle
loads the base working layout again and reapplies the live542 copy,
rather than preserving a scene's mutable layout. Scene camera8,29 and
later8,13 are distinct from battle crop and combat coordinates. Map59's
single area `$B937E..$B939E` is0,0..26,37, layer255, zero second-layer
origins/autoscroll, both parallax256, MUSIC_FINAL_BATTLE. Battle mode
LoadMapArea instead uses width/height28×38 (end27,37); no actual terrain,
layout, material, view/display or input observation is added.

After LoadBattle the independent492 start latch and empty script complete.
The next instruction is ActivateEnemies. No R0–R3 truth, hidden spawn,
activation result, queue, turn count or first control has been calculated.

## Selected lowering and actual production consumers

**Confirmed about tooling:** the sole observation uses scene_maps=[59]
and registers42's before source. bbcs_42 lowers to17 instructions and
ends with a jump to cs_4EDE8's127:144 total. It is not just the first
program. Shared eas_Init lowers14 instructions and eas_Jump9. All seven
inline motion streams and the custom Elis stream are retained. The
only top-level native calls are cs_4EDE8[3] fadeInFromBlackHalf,
[4] cameraSpeed and[123] flashScreenWhite; no nested native action.
No importer/world/terrain/definition export, runner or adapter executed.
`local/issue1170/observe-01.json` retains every entry, action, native
operation, error(null), source dependency and corpus relationship.

The lowered scene population contains exactly19 ordinary follower rows,
matching Map59's non-overworld selection. Fixed Peter/Lemon reuse and
zero aliases have actual allocation consumers. Logical-ally HP0 still
differs: native setPos skips Peter/Lemon/Zynk, while the generated
position op and ProgramRunner's SetEntityPosition have no HP guard.
stopEntity129's typed animation and custom OFF/orient/refresh/jump are
representable; this does not bypass the later native half-fade refusal.

| Actual owner / boundary | Present mechanism and selected limitation |
| --- | --- |
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py), [Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Scene-map/entity, branch, text/token, animation, motion, sound, full fade, quake and camera shapes exist. Native-call reads as UnsupportedInstruction; reached opcode refuses. Earlier missing resource/service can stop first. No accepted144-op execution or completed prefix.|
|[SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [allocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Follower/fixed allocation, zero aliases, physical-slot timers, Init/Jump, flags/animation-counter, refresh and waits have consumers.49-slot guard and prepared appearances differ from unguarded native service/caller states; HP-skip and original asynchronous timings remain gaps.|
|[Map preparation](../../src/sf2tool/remake_exploration_maps.py), [final producer](../../src/sf2tool/remake_exploration_content.py)|Default59's real setup is route-present, not ms_Void. Outside eventMaps it retains map59-setup frontier and a542 guarded native flag-layout-copy onLoad; its long default init cannot match the supported four-op guard/script/return lowering.543 variant is structurally that four-op shape, but selected variant/eventMaps binding and later scene compilation are separate; no variant preparation was run. It must not be described as default59 or as an absent-route overwrite to end.|
|Same producers, ordinary versus battle entry|eventMaps registers setup sources and compiles chosen init; selected setups prepare additional variant entities/events/onLoad. Direct before registration bypasses that work and does not establish a59 exploration package,58→59 switching, F542 copy, material binding or Battle42 entry. No synthetic setup/lower run was needed to establish the actual producer conditions.|
|[Encounter reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [encounter preparation](../../src/sf2tool/remake_exploration_battles.py)|Pinned reader admits1/2/3, preparation2/3, neither42. Coordinate admission, selected private sources/materials/definitions/routes are not produced by a scene lowering.|
|[Definition reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs), [scenario assembly](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Metadata can retain items/spells/class, but assembly restricts1/2/3, STARTING and REGULAR spell power;42 adds ENHANCED, HIDDEN, HEALER5/SENTRY and BIT4 AI gaps. Galam's copied rod/stat refresh and neutral159 need their own bound behavior.|
|[Initialization](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [activation validation](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs)|Requires roster-only/already-refreshed allies,difficulty0 and bounded controls/status/capacity. Equipped enemy refresh refuses; SpawnMode2 refuses before initialization. Formula-shaped activation is not native hidden HP0/X−1/stale fields, all30 healing or neutral creation.|
|[BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Before→initialize→load→start continuation exists; F88 NewBattle refuses suspended-battle-entry. Initialization publishes computed state; BattleStartFinished later advances beyond this slice's stop. No42 entry was executed.|
|[View runner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [battle viewport](../../remake/game/src/Battles/BattleMapViewport.cs), [presentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Map59 layer255/unit parallax/zero origins/autoscroll meets existing view-profile predicates. Nonzero field speed separately refuses field-view-speed. Adapter has generic fades/sound/quake/view effects, not selected half fade/white flash or proven resources, VInt/DMA, completed presentation or usable control.|

These are present support boundaries and source-level gaps, not a claim
that original operations are already bound or playable. No consumer fix
or new package is included.

**Inferred:** the variant's final producer has a conditional shared-ID effect.
prepare_exploration first compiles the default map, then requested setups
using the **same** compiler. frontier uses setdefault, and the four-op
optimization assigns `compiler.programs["map-59-setup"]["instructions"]`
without a setup-specific ID. If requested543 compilation reaches that
assignment, it replaces that shared program with the543 guard/call/end
body. Outside eventMaps the earlier default onLoad still points through
map59-flag-layout to that same ID; it does not retain an independent
default init merely because the default map row was built first. An
earlier compile/refusal can prevent publication. With eventMaps, onLoad
instead names the chosen init directly. This is a source-backed producer
deduction, not a run or published variant package; it does not authorize
an ending-scene import to close the gap.

## Reproduction, retained failures and open boundary

The ignored direct recipe checks selected source/consumer snapshots,
whole command/table order, inline payloads, branch polarity, operand
widths, aliases/HP, hidden/absent/neutral producers, current-stat arithmetic,
final producer conditions, source pin, pair metadata, links/UTF8/LF,
private publication and exact owned diff. No maintained checker framework,
parser/schema/fixture/helper test or H3 callback changed. Tracked prose
and named source/Git objects own durable findings; local receipts are
coordination and reproduction aids.

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1170/review.py <configured-source-disasm-root> --receipt local/issue1170/direct-reproduction-01.json --committed --snapshots local/issue1170/direct-01.json
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

Each run must choose a receipt path that does not yet exist; change the
example name after using it and never overwrite historical results.
The snapshots option reuses the retained direct-01 source snapshots.
The source argument ends in disasm. The sole selected observation took
0.1266s and is reused, not repeated for a commit change. Apply ADR0004's
relevant complete-data/provenance/date/polarity/width/order/selector/
actual-consumer/private checklist. Normal/full, old comparison, SDK,
Godot, emulator/H3/H4 and screenshots are **SKIPPED** by explicit
documentation-only scope. Clean committed planner and actual scoped CI
belong in the frozen handoff; proposed commands are not passed checks.

Preserve [Battle41's retained failures](./battle41-outcomes-and-ancient-tower-stairway.md#reproduction-preserved-results-and-stop),
[Battle40 geometry](./battle40-regions-terrain-and-first-control.md),
[Map54 final overwrite](./devils-head-labyrinth-and-battle40-admission.md),
[Map53 failed import/zero partial](./roft-to-galam-and-battle36-admission.md#actual-lowering-and-current-consumer-boundary),
the twelve completed old comparison failures, #1162 followEntity high-byte/
BowieHP missed-finding PASS and accepted correction, and #1166/#1168
recipe/read failures with their exact prior receipts. Nothing is rerun,
erased, called interrupted, upgraded to PASS or silently repaired here.
This task's read/lookup failures and self-review corrections remain in
its ignored handoff alongside completed results.

**Unknown:** natural admission and actual incoming words/selectors/flags,
party/class/HP/status/items/followers/aliases/transport; F88 retained
state, queue/cursor/RNG and saved-byte contents; interleaving/interruption
and completion of entity/view/sprite/text/audio/quake/fade/flash/map/input
services, hardware/VInt/DMA, source-ROM equality, decoded geometry/
collision/tactical terrain/materials, save persistence and original/remake
playability. These are not an automatic runtime or repair queue.

Freeze a clean committed/pushed Draft PR with exact head/tree, completed
direct checks/planner/actual CI, process terminal state and handoff. Main-gate
retains independent review/merge. Stop before first activation and at the
frozen author delivery; no merge, cleanup, archive or next Issue.
