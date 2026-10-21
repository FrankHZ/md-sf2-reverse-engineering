# Battle37 initialization and the Red Baron opening

Evidence date: 2026-10-21. **Confirmed (static):** admitted Map77/Battle37
can stage Geshp128 and Red Baron129, then replace that population with
the live party and eighteen STARTING enemies led by Red Baron128.
Fresh entry reconstructs combatants even when intro487 skips the scene.
Battle37 is absent from the random-upgrade list. Held88 bypasses fresh
initialization and, with incoming map77, reads a scratch byte beyond the
alias table for its load target. Stop before the first ActivateEnemies;
F88 stops at the individual-turn entrance. Natural reach, completed
services, presentation, usable control and persistence remain **Unknown**.
Offline lowering is not playable Battle37 acceptance.

## Agreement, provenance and method

Standard rigor for [Issue #1136](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1136)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect accepted Galam return and independent37 admission to the complete
opening, formation, definitions and initialization for later content work.
Accepted base `a7aa9f27917c453ef926c418140c249b6ef7124f` includes
[Battle36 outcomes](./battle36-outcomes-and-galam-return.md) and the
[Map77 admission owner](./roft-to-galam-and-battle36-admission.md).
Own this document and navigation only in the outcomes owner; no shared
index, registry, schema, fixture, design, engine or tool changes, competing
writer or stacked dependency. Region geometry/activation, turn/RNG
generation, individual Red Baron/Desoul rules, combat, outcomes/after37
and Galam/Dwarven Village content are excluded.

Reuse [Battle36 initialization](./battle36-initialization-and-opening.md),
[Battle35 initialization](./battle35-initialization-and-opening.md),
[spritesets](./battle-spriteset-data.md), [global data](./battle-global-data.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[cutscene data](./battle-cutscene-data.md), [maps](./common-maps.md),
[scripting](./common-scripting.md) and
[battle lifecycle](../design/contracts/battle-control-lifecycle.md).
Shared mechanics retain their owners; selected37 operands and changed-state
boundaries are checked here. Tables describe declarations, not immutable
legal gameplay states or an actual roster snapshot.

Original paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`map/` and `entity/` abbreviate `code/common/scripting/` children;
`battle/` means `code/gameflow/battle/`. sf2const.asm, sf2enums.asm and
original macros own constants, widths and encodings. Fnnn means story
flag nnn; ordinals are zero-based; intervals half-open. USA ROM identity
remains SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source equality, decoded terrain or runtime observation is claimed.

Minimum sufficient evidence is selected source/tables/helpers, the accepted
immutable18,285,979-byte init/script pair with `local/issue1005/pair.txt`
provenance, one existing-compiler lowering and actual consumers. Complete
selected source registration requires no canonical map import/export.
Linear reads and one pair load have a one minute/512MiB budget per direct
command; peak memory is unmeasured. Small unique receipts stay in ignored
`local/issue1136/`. No acquisition, new parser/platform/fixture, tool tests,
normal/full/old comparisons, SDK/Godot/emulator/H3/H4 or screenshots.
Reassess changed prerequisites or material contradictions before expansion.

## Admission, fresh/repeat prefix and independent latches

**Confirmed:** the predecessor owns Map77's invoked default-zone callback:
clear537 sets437 and posts longword RELOADMAP; held537 returns without
clearing preheld437. The callback does not initialize battle37.
CheckBattle's seven-byte row37 at `$7B39..$7B40` is
`77,16,32,48,32,255,255`. A CURRENT scan with437 admits37; held537 clears437
while admitting it on that invocation. No36-victory, party or487 predicate
is added. CheckBattle publishes crop16,32 size48×32, not CURRENT_BATTLE.
`code/gameflow/mainloop.asm:MainLoop` passes selected d7.w→d1.w and
map77 in d0.w to j_BattleLoop. Caller high bytes remain explicit inputs.

`battle/battleloop_1.asm:BattleLoop` clears PLAYER_TYPE.b, sets399 and
tests88. Clear88 clears seconds, saves incoming d0/d1 **words**, assigns
their low bytes to CURRENT_MAP/BATTLE and calls SetBaseVIntFunctions then
the before wrapper. It restores the two words and rewrites the two bytes
after before, without undoing reached flags, crop, layout, entities or
party effects. It then clears90–105 inclusive, heals living/immortal
allies, initializes all ally positions, all enemy positions and AI memory,
in that order. It clears d0.w, calls LoadBattle for combatant0 and calls
the start wrapper. Its next @Start instruction is the excluded ActivateEnemies.

| Complete table / selected entry | Result |
| --- | --- |
|`data/battles/cutscenes/beforebattlecutscenes.asm`,48 relative words, base `$47A88`|slot37/byte74 at `$47AD2` → bbcs_37 `$4DA66`.|
|`data/battles/cutscenes/battlestartcutscenes.asm`,48 relative words, base `$47B2C`|slot37/byte74 at `$47B76` → ms_Empty, sole FFFF word.|

`battle/cutscenes/beforebattlecutscenesstart.asm:ExecuteBeforeBattleCutscene`
tests450+CURRENT_BATTLE=487 and skips when held; it does **not** set487.
`battlestartcutscenesstart.asm:ExecuteBattleStartCutscene` independently
tests487, sets it **before** dispatch and executes the empty script.
Even empty ExecuteMapScript resets dword_FFB1A4 to FF9004_LOADING_SPACE,
clears text skip, conditionally waits for view when dialogue is open and
clears VIEW_SCROLLING_SPEED on return. Skipping a hook bypasses those effects.

| State at the named read | Selected boundary |
| --- | --- |
|88 clear;487 clear throughout|Complete before, fresh reconstruction/load, set487 then empty start.|
|88 clear;487 held throughout|Both hook bodies skip; fresh heal/positions/load still run.|
|537 held while437 admits|Same fresh path;487 independently gates scene.37 still fails random-upgrade membership.|
|487 changes between wrappers|Each follows its own read; no shared immutable latch snapshot.|
|Selector changes inside a service seam|Each table/helper rereads live state. Saved-word restoration repairs only current map/battle bytes at its named point.|
|Interrupted prefix|Reached mutations persist.487 may remain clear after before work, or held before start dispatch completes. No control/presentation/save guarantee follows.|

### F88: incoming77 selects scratch, not a combatant alias

Held88 restores saved seconds, clears88, clears AI memory and
VIEW_TARGET_ENTITY.b, then calls LoadBattle **without clearing d0**.
It branches directly to @IndividualTurns_Loop; the following @Start branch
is unreachable. Fresh current-byte assignment, both hooks,90–105 reset,
healing and combatant position/stat initialization are bypassed. Retained
CURRENT_MAP/BATTLE, crop, combatants/activation and turn buffer remain inputs;
incoming77 does not install them.

`battle/battlefunctions/loadBattle.asm:LoadBattle` saves d0.w across fade,
tilesets and PositionBattleEntities. `battlefunctions_0.asm:GetEntityIndexForCombatant`
tests d0.b:77 is nonnegative, so there is no enemy subtraction. Its FF word
mask yields offset77 even with a nonzero incoming high byte. The byte read
is `ENTITY_INDEX_LIST($FFB140)+77=$FFB18D`, the **low byte of the word
TEMP_ITEM_OR_SPELL** at `$FFB18C`. This lies beyond the64-byte alias table
`$FFB140..$FFB180`; it is not enemy173, Bowie0 or Battle36's valid offset53.
ClearEntities clears only that64-byte table; PositionBattleEntities
rebuilds ally0–29/enemy32–63 aliases, not offset77. The selected prefix
does not initialize that scratch byte for this purpose. Its value at the
read, including intervening service effects, is **Unknown**.

LoadBattle saves fetched byte T as view target. Negative T uses physical0
for coordinate reads; otherwise T&3F is used without a49-record guard.
FF/80→0,00→0,30→48,31→49 and7F→63 are source arithmetic examples,
not observed values. PositionBattleEntities packs retained eligible slots
regardless of the eighteen-row declaration. The later LoadMap also consumes
view-target state. No valid/safe suspended restoration is established.

ClearAiMemory resets48 last-target bytes to FF and48 memory bytes to0
(`$FFB1AC..$FFB1DC`, `$FFB1DC..$FFB20C`), not the newly-triggered queue
word `$FFB20C..$FFB20E`, region flags or turn buffer. F88 also bypasses
ExecuteMapScript's scratch-pointer reset. Stop at the individual-turn
entrance, before reading/acting on its retained turn entry.

## Scene population, coordinates and complete opening

**Confirmed:** `data/battles/entries/battle37/cs_beforebattle.asm`
spans `$4DA66..$4DB94`:52 top-level commands/270 encoded bytes, then
ce_4DB74's32 bytes (six-byte main header, three eight-byte records,
two-byte terminator). There are no inline action streams. cs_4DAB8 is
an interior branch target after the conditional Zynk block, not another
independent opening.

| Record / logical identity | World-map X,Y / facing | Sprite / action |
| --- | --- | --- |
|main /Bowie0|55,36 /Left|actual ally0 /eas_Idle|
|0 /Peter7|56,37 /Left|ALLY_PETER /eas_Init|
|1 /128|63,63 /Up|GESHP159 /eas_Init|
|2 /129|35,60 /Up|RED_BARON158 /eas_Init|

Scene positions are world-map operands, distinct from camera requests
50,33 →40,44 →30,54 →40,44 →50,33 and from crop-relative combat positions.
For unchanged crop16,32, scene Red Baron35,60 equals combatant128's later
19,28+crop. Scene Bowie55,36 differs from first living formation39,6+crop
=55,38; membership/HP determine whether Bowie receives that row at all.
Geshp's63,63 initial storage is not a battle declaration or an off-map
deletion. His later45,47 position is likewise separate from combatants.

loadMapFadeIn77,50,33 enters csc37 then csc48: it requests out-to-black,
loads scene map/view services without exploration init/population, and
returns before the separate fadeInB command. csc42_loadMapEntities invokes
InitializeMapEntities/LoadEntityMapsprites with display/VInt control,
replacing population. ClearEntities resets49 physical records,64 aliases
to0 and walking scratch. Followers allocate before fixed records,
independently of joined/active force membership.

Unlike53,77 is in the complete `data/maps/global/overworldmaps.asm` list.
Held65 therefore selects table_OverworldFollowers, which starts with the
Caravan65/FOLLOWER_A30 prefix and falls through all19 ordinary follower
rows; clear65 selects the ordinary19 only. The full ordered rows are in
`data/scripting/entity/followers.asm`; repeated aliases retain every
physical allocation while the last write wins. FOLLOWER_A can be Caravan
under65, King under74, Old Man under77, then King under83. FOLLOWER_B31
can be Astral74, Oddler68, then Astral70. No producer leaves its fresh
alias0, directing that logical reference to Bowie rather than refusing it.

F67 may allocate Peter7 first; the duplicate fixed ally record then retains
that follower's position/action until explicit commands. F76 allocates
Zynk26 as a follower, without joining/activating26. Let n be allocated
followers and p=1 if fixed Peter allocates, else0: Bowie is physical0,
Geshp128 is n+p+1 and scene Red Baron129 is n+p+2. Follower-script pointer
tables have five entries; arbitrary flag combinations/capacity are not
proved safe. Appearance uses actual ally class/Rohde membership; while
currently in battle37, dead allies do not take the outside-battle flame
path. No scene command joins the force, adds HP or initializes combatants.

| Index | Complete ordered command / boundary |
| --- | --- |
|0|textCursor2956|
|1|loadMapFadeIn77,50,33|
|2|loadMapEntities ce_4DB74|
|3|setActscriptWait Bowie0,eas_Init|
|4|setActscriptWait Peter7,eas_Init|
|5|setPos Peter7,56,37,Left|
|6|setActscriptWait FOLLOWER_B31,eas_Init|
|7|setPos FOLLOWER_B31,56,35,Left|
|8|setPos FOLLOWER_A30,56,36,Left|
|9|jumpIfFlagClear76,cs_4DAB8 (index12)|
|10|setActscriptWait Zynk26,eas_Init|
|11|setPos Zynk26,63,62,Down|
|12|cs_4DAB8: fadeInB|
|13|nextSingleText00,128 →2956|
|14|setActscript Bowie0,eas_Jump|
|15|setActscriptWait Peter7,eas_Jump|
|16|setFacing Bowie0,Down|
|17|setFacing Peter7,Down|
|18|setFacing FOLLOWER_B31,Down|
|19|cameraSpeed$30|
|20|nextSingleTextC0,FOLLOWER_B31 →2957|
|21|setCamDest40,44|
|22|animEntityFX128,MOSAIC_OUT|
|23|setPos128,45,47,Up|
|24|animEntityFX128,MOSAIC_IN|
|25|nextSingleText00,128 →2958|
|26|nextSingleTextC0,Peter7 →2959|
|27|nextText00,128 →2960|
|28|nextSingleText00,128 →2961|
|29|setFacing128,Down|
|30|nextSingleText00,128 →2962|
|31|setCamDest30,54|
|32|setActscriptWait129,eas_Jump|
|33|setActscriptWait129,eas_Jump|
|34|nextSingleText00,129 →2963|
|35|nextText00,128 →2964|
|36|nextText00,128 →2965|
|37|nextText00,128 →2966|
|38|nextSingleText00,128 →2967|
|39|setCamDest40,44|
|40|setFacing128,Up|
|41|nextText00,128 →2968|
|42|nextSingleText00,128 →2969|
|43|animEntityFX128,MOSAIC_OUT|
|44|setCamDest50,33|
|45|setFacing Peter7,Up|
|46|nextSingleTextC0,Peter7 →2970|
|47|setFacing FOLLOWER_B31,Down|
|48|setFacing Bowie0,Right|
|49|nextSingleTextC0,FOLLOWER_B31 →2971|
|50|nod Bowie0|
|51|csc_end|

### Actions, HP, aliases and service order

Reuse the accepted [action, HP, alias and wait semantics](./battle36-initialization-and-opening.md#action-hp-alias-and-wait-semantics).
In particular eas_Init clears flagsA bits5/6, **disabling entity/map
collision checks** at their actual consumers; do not infer the opposite
from uncollidable macro names. It sets acceleration0, speed32/32,
obstruction/auto-facing, Up, nonimmersed, sprite size24, refreshes and
jumps idle. csc15 resolves aliases, installs the physical-slot wait timer
and pointer, and Wait spins until eas_Idle. It has no ally-HP guard.

setPos/setFacing use AdjustScriptPointerByCharacterAliveStatus: HP0
ally IDs0–29 skip their four-/two-byte payload, while30/31 and128+ bypass
that test. Thus dead Peter gets Init/jump but skips position/facing; later
fresh healing is a separate step. Held76/HP0 Zynk gets Init but skips63,62;
clear76 skips both. setPos FOLLOWER_A has **no preceding Init**. Its alias
may target a still-following actor or Bowie; prior action/service state
is not canceled by describing a new position. Missing31's Init can reset
Bowie, and its later position/facing can address him despite Bowie's HP0.
No65 or follower-identity predicate protects either setPos.

`data/scripting/entity/eas_actions.asm:eas_Jump` disables auto-facing,
moves relative0,−1, waits5, moves relative0,+1, waits5, issues relative0,0,
reenables auto-facing and jumps idle. Bowie's non-Wait jump is followed
by Peter's Wait; that waits Peter's actual script pointer, not an aggregate
both-actors barrier. Red Baron129 has two successive Wait jumps. These
are scene action requests with a nominal return to the starting tile;
they do not execute a combat attack or prove a completed destination.

cameraSpeed writes word$30=48 to VIEW_SCROLLING_SPEED. Each setCamDest
detaches the view target and waits for SetCameraDestination/scroll end;
these are view requests, not combat crop changes or entity speeds.
csc22 resolves Geshp's actual alias: mosaic-out6, setPos45,47,Up,
mosaic-in7 occur in that order, then final mosaic-out6. The mosaic loop
loads/transforms/transfers sprites with VInt waits; it does not remove
the alias, set HP0 or create a combatant. nod has no HP guard: it sets
animation counterFF, sleeps10, transfers the nod sprite, sleeps20,
refreshes, sleeps10 and clears the counter. These operands are not
measured elapsed presentation. The scene has no explicit csWait or
playSound command; map loading/music and other services retain their
own effects. Interruption/interleaving can leave a reached prefix.

### Dialogue identity, modifiers and full tokens

textCursor2956 plus sixteen sequential commands joins gamescript.txt keys
0B8C–0B9B and the source comments. S=nextSingleText, C=nextText;00 has no
portrait modifier, C0 requests mirrored/right-side portrait. Portrait and
speech lookup use actual aliased appearance, so follower identity and
Red Baron158's no-portrait mapping must not be replaced by a name guess.
`data/spritedialogproperties.asm` supplies GESHP/GESHP/DIALOG_BLEEP_6
and RED_BARON/NONE/DIALOG_BLEEP_6 to GetEntityPortaitAndSpeechSfx;
csc1D skips opening a portrait when the returned portrait is−1.
Prose below is short authored paraphrase; braces retain full token order.

| ID / hex | Mode / modifier / logical speaker | Authored purpose | Complete tokens |
| --- | --- | --- | --- |
|2956 /0B8C|S /00 /128|Geshp threatens revenge for the flowers.|`{N}{W1}`|
|2957 /0B8D|S /C0 /31|Follower recognizes Geshp.|`{W1}`|
|2958 /0B8E|S /00 /128|Geshp laughs.|`{W1}`|
|2959 /0B8F|S /C0 /7|Peter suspects another trap.|`{N}{W1}`|
|2960 /0B90|C /00 /128|Geshp feigns innocence.|`{W2}`|
|2961 /0B91|S /00 /128|Geshp announces a companion.|`{N}{W1}`|
|2962 /0B92|S /00 /128|Geshp calls Red Baron.|`{W1}`|
|2963 /0B93|S /00 /129|Red Baron growls.|`{W1}`|
|2964 /0B94|C /00 /128|Geshp locates him outside the gate.|`{N}{W2}`|
|2965 /0B95|C /00 /128|Geshp makes him the obstacle to advance.|`{N}{W2}`|
|2966 /0B96|C /00 /128|Geshp points to the bloodied armor.|`{N}{W2}`|
|2967 /0B97|S /00 /128|Geshp describes prior victims.|`{W1}`|
|2968 /0B98|C /00 /128|Geshp turns the threat toward the party.|`{W2}`|
|2969 /0B99|S /00 /128|Geshp predicts their defeat.|`{W1}`|
|2970 /0B9A|S /C0 /7|Peter notes nearby Galam and the obstacle.|`{N}{W1}`|
|2971 /0B9B|S /C0 /31|Follower urges advance and asks the leader.|`{N}{LEADER}{W1}`|

Single commands close/clear their textbox/portrait after display; C
commands retain the continuing window. W1/W2 and LEADER remain actual
text-service/input/name dependencies, not rendered English guarantees.

## Complete selected formation, roster and definitions

**Confirmed:** `data/battles/spritesets/entries.asm` slot37 points to
BattleSpriteset37; `data/battles/terrainentries.asm` slot37 points to
BattleTerrain37→battle37/terrain.bin. This is a binding, not terrain decoding.
Global custom background37 is OVERWORLD. No37 neutral row exists;
held537 skips the neutral scan, clear537 reaches it and finds none.
The leader-presence byte37 is FF/nonzero; its later death/outcome consumer
is outside this initialization slice. Placement LEADER AI is independent
of that global byte and of scene identity129.

### Complete spriteset37

`data/battles/spritesets/spriteset37.asm` spans `$1B60BE..$1B6250`,
402 storage bytes: four header bytes12/18/3/1,144 ally bytes,216 enemy
bytes,36 region bytes and two point bytes. Relative sections are
0/4/148/364/400/402. Enemy rows start `$1B6152`, regions `$1B622A`,
point `$1B624E`. Each entity record is12 bytes:
definition-or-slot0,X1,Y2,AI3,item word4–5,primary order6/region7,
secondary order8/region9,filler10/spawn11. The last pair is consumed as
a word then rotated/masked for activation. The accepted inventory has
rowContentTracked=false; these complete rows are direct source checks,
not a nonexistent exact-row public fixture.

All twelve ally records use HEALER1, NOTHING, NONE/15,NONE/15, filler0,
STARTING. Their identity bytes are formation slots, not party IDs:

| Slot | X,Y | Slot | X,Y | Slot | X,Y |
| --- | --- | --- | --- | --- | --- |
|0|39,6|4|41,6|8|41,4|
|1|40,6|5|39,4|9|42,6|
|2|39,5|6|41,5|10|42,4|
|3|40,5|7|40,4|11|42,5|

All18 enemy rows have placement item NOTHING, filler0 and STARTING.
Positions/regions/points below are crop-relative; combatantID=128+ordinal.

| Ordinal / ID | Definition | X,Y | AI | Primary / region | Secondary / region |
| --- | --- | --- | --- | --- | --- |
|0 /128|RED_BARON|19,28|LEADER|MOVE_TO\|0 /15|NONE /15|
|1 /129|EVIL_BISHOP|17,28|HEALER5|FOLLOW_ENEMY\|0 /15|NONE /15|
|2 /130|CHAOS_WIZARD|18,27|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|
|3 /131|MINOTAUR|20,24|SENTRY|NONE /15|NONE /15|
|4 /132|BOW_RIDER|20,25|ATTACKER3|FOLLOW_ENEMY\|3 /15|NONE /15|
|5 /133|HELL_HOUND|23,27|SENTRY|NONE /15|NONE /15|
|6 /134|CHAOS_WIZARD|24,18|ATTACKER1|FOLLOW_TARGET\|0 /2|NONE /15|
|7 /135|EVIL_BISHOP|25,20|HEALER4|FOLLOW_ENEMY\|6 /2|FOLLOW_ENEMY\|0 /15|
|8 /136|HELL_HOUND|26,20|ATTACKER3|FOLLOW_ENEMY\|6 /2|NONE /15|
|9 /137|MIST_DEMON|36,29|ATTACKER2|FOLLOW_TARGET\|0 /1|NONE /15|
|10 /138|HELL_HOUND|38,23|ATTACKER3|FOLLOW_ENEMY\|11 /1|NONE /15|
|11 /139|MINOTAUR|39,22|ATTACKER2|FOLLOW_TARGET\|0 /1|NONE /15|
|12 /140|MIST_DEMON|23,2|ATTACKER2|NONE /15|NONE /15|
|13 /141|EXECUTIONER|31,9|ATTACKER2|NONE /15|NONE /15|
|14 /142|BOW_RIDER|32,10|ATTACKER3|FOLLOW_ENEMY\|13 /15|NONE /15|
|15 /143|BOW_RIDER|31,13|ATTACKER1|NONE /15|NONE /15|
|16 /144|HELL_HOUND|32,15|ATTACKER3|FOLLOW_ENEMY\|15 /15|NONE /15|
|17 /145|BOW_RIDER|33,15|ATTACKER3|144\|0 /15|NONE /15|

`sf2macros.asm:combatantBehavior` emits the full order byte:
MOVE_TO|0=$40, FOLLOW_ENEMY|0/3/6/11/13/15=$80/83/86/8B/8D/8F,
FOLLOW_TARGET|0=$00, raw144|0=$90, NONE=$FF. Preserve$90: it takes
combatant144 rather than128 at the existing nonpoint HP/position consumers
in `battle/ai/startaicontrol.asm`, `ai/command/moveorder.asm` and
`battlefield/getmoveorderposition.asm`. Bit6 instead chooses an AI point
and masks its low nibble. Red Baron's$40 therefore names point0, not an
ally/combatant64. Trigger15 is the absent-region value; no region15 exists
in these three declarations. Actual AI selection/activation is not run.

Each region is type4, unused byte0, four ordered vertices, trailing0,0:

| Region | Ordered vertices |
| --- | --- |
|0|0,15;0,31;30,31;30,15|
|1|0,12;0,31;47,31;47,12|
|2|0,0;0,31;47,31;18,0|

Point0 is19,28 (world35,60 under unchanged crop). The sloping region2
is retained verbatim. No inclusion, activation or first-turn result is
inferred from the table alone.

### Definitions, equipment, status and fresh initialization

Join `data/stats/enemies/enemydefs.asm` and enemymapsprites.asm. Stats are
`(level,maxHP,maxMP,baseATT,DEF,AGI,MOV)` before difficulty/equipment.
All eight definitions have ENHANCED spell power, initialStatus NONE and
four item/spell slots; unlisted slots are NOTHING.

| Definition / index / mapsprite | Stats | Move type | Equipped item / spells | Resistance |
| --- | --- | --- | --- | --- |
|RED_BARON94 /158|60,150,0,42,59,190,6|REGULAR|DARK_SWORD /none|WIND_MINOR,LIGHTNING_MINOR,ICE_MINOR,FIRE_MINOR,STATUS_IMMUNITY|
|EVIL_BISHOP84 /148|38,64,51,54,48,46,5|HEALER|GREAT_ROD /AURA2,BLAST3|0|
|CHAOS_WIZARD74 /138|36,53,49,47,48,59,5|MAGE|GREAT_ROD /BLAZE4|0|
|MINOTAUR30 /94|35,58,0,51,51,44,5|REGULAR|ATLAS_AXE /none|0|
|BOW_RIDER64 /128|33,61,0,44,55,44,7|CENTAUR_ARCHER|HYPER_CANNON /none|0|
|HELL_HOUND29 /93|35,62,0,84,49,50,6|STEALTH|none /FLAME2|ICE_WEAKNESS,FIRE_MAJOR|
|MIST_DEMON50 /114|36,68,28,80,41,51,6|HOVERING|none /MUDDLE2|WIND_WEAKNESS,ICE_MAJOR,STATUS_MINOR|
|EXECUTIONER28 /92|33,64,0,46,46,45,6|REGULAR|BATTLE_SWORD /none|WIND_MINOR,LIGHTNING_MINOR,STATUS_MINOR|

Prowess `(critical,double,counter)` is Red Baron `(125_1IN4,1IN4,1IN4)`,
Evil Bishop `(150_1IN32,1IN32,1IN32)`, Chaos Wizard
`(150_1IN32,1IN32,1IN16)`, Minotaur `(150_1IN8,1IN16,1IN16)`,
Bow Rider `(150_1IN8,1IN32,1IN32)`, Hell Hound
`(150_1IN32,1IN16,1IN16)`, Mist Demon `(SILENCE,1IN32,1IN16)` and
Executioner `(SILENCE,1IN16,1IN16)`. unknownByte is0 for Red Baron/Evil
Bishop and the definition index for the others. AI priority is1 for Red
Baron,3 for Evil Bishop,2 otherwise; extra BIT4 exists only on Chaos
Wizard/Bow Rider/Mist Demon. STARTING/filler0 initialization preserves
only the priority upper nibble, producing1000/3000/2000 respectively,
before activation. Placement AI/movement type and these words are distinct.

InitializeEnemyStats calls UpgradeRandomBattleEnemies before copying
56 bytes/14 longwords. It fills currentHP/MP from maxima, installs X/Y,
movement+placement AI, definition ID, both triggers/orders, placement
items and activation, adjusts baseATT for difficulty, then refreshes
current stats/equipment. Placement NOTHING skips additions; it does
**not erase copied equipment**. All18 rows initialize normally; the Jaro
exception is battle32 only. Absent146–159 take mode0/activation0,
max/currentHP0 and X−1 without refreshing other stale fields. Their
trigger read uses the first enemy pointer (15/15), not nonexistent rows.

The five equipped item definitions are in `data/stats/items/itemdefs.asm`:

| Item / index | Range / price | Type / use | Ordered equip effects |
| --- | --- | --- | --- |
|DARK_SWORD70|1–1 /17000|WEAPON,RARE,CURSED,BREAKABLE /DESOUL1|ATT+50,DEF−5,NONE0|
|GREAT_ROD95|1–1 /7900|WEAPON,RARE /NOTHING|ATT+28,NONE0,NONE0|
|ATLAS_AXE41|1–1 /9600|WEAPON,RARE,BREAKABLE /BLAZE3|ATT+35,NONE0,NONE0|
|HYPER_CANNON53|2–3 /8700|WEAPON,RARE /NOTHING|ATT+40,NONE0,NONE0|
|BATTLE_SWORD66|1–1 /9200|WEAPON,RARE /NOTHING|ATT+35,NONE0,NONE0|

`code/common/stats/updatecombatantstats.asm` rebuilds current stats,
applies status effects, then equipped effects in four-slot order.
ApplyItemOnStats's enemy branch clears d2 and preserves that zero CCR
instead of testing CURSED, so Red Baron's equipped cursed sword does
not set CURSE here. His status remains0, currentDEF54, currentAGI raw190
($BE), currentMOV6. No initializer masks AGI to62; the downstream high-bit
turn rule is excluded. DESOUL is item-use metadata, not a learned spell
in his four empty slots, and no individual action rule is established.

Ordinary difficulty applies truncated5/4 to baseATT once; SUPER applies
it twice, before equipment. Current DEF/AGI/MOV match the definition except
Red Baron's DEF−5; none of these weapons changes AGI/MOV or prowess.

| Definition | Current ATT ordinary / SUPER |
| --- | --- |
|RED_BARON|102 /115|
|EVIL_BISHOP|95 /111|
|CHAOS_WIZARD|86 /100|
|MINOTAUR|98 /113|
|BOW_RIDER|95 /108|
|HELL_HOUND|105 /131|
|MIST_DEMON|100 /125|
|EXECUTIONER|92 /106|

Reuse the [common ally heal/formation/packing owner](./battle36-initialization-and-opening.md#selected-definitions-current-stats-and-inventories):
HealLivingAndImmortalAllies visits all30 allies regardless of membership,
healing living members and Peter7/Lemon28 even at HP0, retaining the
STUN/POISON/CURSE pre-refresh mask. Refresh reconstructs curse from actual
cursed equipment; STUN reduces MOV1/AGI5 through saturating helpers;
poison persists. Dead ordinary allies stay dead/unrefreshed. Actual stats,
class, equipment and HP remain inputs.

InitializeAllAlliesBattlePositions resets all30 X−1 and rebuilds force
lists, then examines at most the first twelve active-list entries in ID
order. Living members consume formation rows; dead members consume an
outer iteration without advancing the formation pointer. Excess entries
LeaveBattleParty, without another list rebuild. Empty N=0 has no entry
precheck: N−1 underflows before a possible out-of-list read. Unjoined
active flags do not create prefix members. This is not twelve living
allies or a guarantee that Bowie occupies39,6.

LoadBattle/PositionBattleEntities clears physical/alias state again,
packs eligible combatants in ID order from signed X/Y and enemy-neutral
bit, **not HP**, masks coordinates3F, scales384 and installs Down/eas_Standing.
With A eligible allies the eighteen selected enemies occupy physical
A..A+17. Skipped combatant aliases become FF; follower30/31 aliases
remain fresh0. Scene128 now denotes Red Baron128; scene129 becomes Evil
Bishop129. No Geshp, Caravan or exploration follower becomes a combatant
from the scene table. Neutral handling above does not restore them.
LoadBattle loads map/sprites/terrain/music/fade using actual view-target
coordinates/fallback, not the scene's camera50,33. ClearAiMemory's limited
reset and the independent empty start complete before the activation stop.

`randombattles.asm` contains exactly3,8,9,10,14,17,19,21,24,26,35.
UpgradeRandomBattleEnemies is called per normal enemy, but intact37 fails
IsBattleUpgradable and returns the original definition **before**
DetermineBattleUpgrade, level/category/RNG work. Held537 and Bowie's level
therefore do not produce Battle35 upgrades. Changed live selectors are
different inputs, not an immutable selected37 guarantee.

## Actual lowering and production-consumer boundary

**Confirmed about current code:** one OriginalPrograms compilation with
scene_maps=[77] and complete source registration lowers bbcs_37 to16
instructions and interior cs_4DAB8 to109 (125 total). The first ends in
an explicit jump and is not the complete opening. All three fixed entities
and eas_Init14/eas_Jump9 action instructions lower; there are no nested
native actions. The sole top-level native is cs_4DAB8[12] cameraSpeed.
No canonical map import, scene execution, runner or Godot launch occurred.

Lowered population contains only19 ordinary followers. Clear65 matches
the native table choice; held65 on77 should include the Caravan prefix,
which this compiler omits. With no other30 producer this yields alias0
instead of Caravan; later74/77/83 producers can instead overwrite30.
Successful lowering therefore does not establish correct follower identity.
Fresh SceneEntities.Build initializes zero aliases (distinct from removed
reference tombstones), uses prepared start-class appearances and has a49-slot
capacity guard. ProgramRunner position/facing operations do not implement
the native ally-HP skip. Those differences remain for changed party/flag/HP
states even before any reached cameraSpeed refusal.

| Actual consumer owner | Expressible shape / actual limit or missing selected binding |
| --- | --- |
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py), [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Scene-map/entity, text, flag branch, camera, motion and gesture shapes exist. cameraSpeed lowers to UnsupportedInstruction and reached `program-opcode` refusal. Earlier resource/service failures can occur first; no whole37 execution is claimed.|
|[SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapEntityAllocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Follower/fixed allocation, zero aliases, Init/jump/idle waits have consumers. Missing overworld65 prefix, prepared appearance and native HP/alias/capacity/service differences remain explicit. No31 producer means alias0, not unresolved reference.|
|[ExplorationTextRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs), [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Token/window/portrait and party-list shapes exist; selected16 material/name/speech bindings and empty/stale native-list behavior are not proved by lowering.|
|[PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Placement/region/point/terrain and before→init→load→start shapes exist. Pin admits1/2/3 and rejects37.37 encounter/definitions/world/route/load/start bindings are absent; F88 NewBattle refuses `suspended-battle-entry`.|
|[BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs)|Requires controlled roster-only/already-refreshed allies, difficulty0, admitted status/control and formation capacity. Selected five equipped loadouts refuse `enemy-equipment-refresh`; placement NOTHING cannot erase them. No general all30/stale-list/interrupted refresh or SUPER path; state computes before publication.|
|[Map preparation](../../src/sf2tool/remake_exploration_maps.py), [ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs)|Map77's area has layer254, zero origins, unit parallax and zero autoscroll, meeting the existing view-profile predicates. Nonzero view speed separately refuses `field-view-speed`. Board area selection uses crop origin, whereas native LoadBattle uses actual target/fallback. No new77 conversion/resource/37 encounter binding is supplied.|
|[ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Mosaic, fade, sprite, text, camera and nod consumers exist, including nod logical/actual completion. Generic effects do not prove original VInt/DMA timing, selected materials, completed presentation or usable input.|

The [Map77/53 predecessor](./roft-to-galam-and-battle36-admission.md#actual-lowering-and-current-consumer-boundary)
already records the default-zone native reload-request gap and actual
event-enabled Map53 UnboundLocalError with zero partial programs. Preserve
that completed failure; this selected source compilation bypasses map-event
preparation and does not repair or close it. Battle01–03 support cannot
establish37 playability. ProgramRunner's BattleStartFinished calls
BattleAdvancer beyond this original stop; it was not executed here.

## Reproduction, preserved results and Unknowns

The ignored probe and direct recipe retain selected source identities,
complete52 commands/three records/sixteen token arrays, both hook tables,
fresh/F88 helpers, all30 placement rows/three regions/point/eight definitions/
five items and actual consumers. Fixed changed-state cases cover latches,
HP/followers, raw orders, difficulty and F88 byte arithmetic. Direct scope,
navigation-only, UTF-8/LF, local links and private-boundary checks apply.
Load current private configuration in the same PowerShell process and use
an unused receipt name:

```powershell
uv run --locked python -X utf8 local/issue1136/probe.py <disasm-root> --receipt local/issue1136/probe-reproduction-01.json
uv run --locked python -X utf8 local/issue1136/review.py <disasm-root> --receipt local/issue1136/direct-reproduction-01.json --committed
uv run sf2 verify plan --base origin/main --head HEAD
```

The source argument contains sf2enums.asm, code/ and data/. One lowering
completed in0.1145s; the review recipe checks that receipt rather than
repeating compilation. Inspect clean committed planner and actual exact-head
scoped CI. ADR0004's applicable adversarial checklist applies; new parser,
schema, fixture, tool-test and H3 callback duties are N/A. Suggested normal
verify is SKIPPED under explicit docs-only scope, not an executed pass.

Preserve Battle36 outcomes' original d9fa79a4 direct/CI success that missed
collision polarity and accepted45f9c5f5 focused correction (author0.2060s,
main0.2124s, CI39547744333); its0.1193s lowering is not rerun. Preserve
Battle36 laser/activation results and their hand-calculation/regex diagnostics,
initialization's three accepted prose corrections, the Map53 import failure,
and the twelve completed old-comparison failures with their named owners.
Earlier Roft alias0, Nazca collision, Dojo follow/action, post-crash import
and Battle34 statistics boundaries remain through those linked owners.
No completed failure is repeated, relabeled interrupted or promoted to PASS.

**Unknown:** natural entry/order; actual caller words, flags, membership,
HP/MP/status/class/equipment, aliases/followers/transport; F88 scratch,
retained selectors/crop/combatants/queue/turn/RNG state; decoded terrain,
geometry/collision and selected resource bindings; interruption/interleaving,
complete motion/view/text/sprite/audio/fade/nod services, hardware VInt/DMA
timing, usable control and save/load persistence. None is an automatic
runtime queue. Region activation, first turn, individual Red Baron/Desoul
combat and outcomes remain outside this slice.
