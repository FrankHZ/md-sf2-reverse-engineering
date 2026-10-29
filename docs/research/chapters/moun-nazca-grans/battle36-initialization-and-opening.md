# Battle36 initialization and the Prism Flowers opening

Continue with [Battle36 activation and first-turn dispatch](./battle36-activation-and-first-turn-dispatch.md).

Evidence date: 2026-10-20. **Confirmed (static):** admitted Map53/Battle36
can stage Geshp, a Caravan and four flowers as temporary scene actors, then
replace them with the live party and twenty STARTING enemies, including
eight Prism Flowers. Battle36 is absent from the random-battle upgrade
list. Its completed flag536 therefore does not upgrade those definitions.
Fresh entry stops before the first ActivateEnemies; F88 stops at the direct
individual-turn entrance. Natural reach, incoming state, completed services,
presentation, usable control and persistence remain **Unknown**. Current
offline lowering is not playable Battle36 acceptance.

## Agreement and evidence route

Standard rigor for [Issue #1128](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1128)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect the complete opening, formation, definitions and initialization for
later content integration and a separate activation/turn investigation.
Accepted base `1815626e6768ba4fcce2b8ffeb3293aef9c24d73` includes the
[Roft/Galam admission owner](./roft-to-galam-and-battle36-admission.md).
Own this document and navigation only there; no shared index, registry,
schema, fixture, engine, design or tooling mutation, or stacked dependency.
Activation geometry, turn order/RNG, laser targeting/damage/visual algorithms,
combat, outcomes/drop delivery, Battle37 body and town content are excluded.

Reuse accepted [Battle33 initialization](./battle33-initialization-and-opening.md),
[Battle35 initialization](./battle35-initialization-and-opening.md),
[battle loop](../../battle/battle-loop.md), [spritesets](../../battle/battle-spriteset-data.md),
[global data](../../battle/battle-global-data.md), [cutscenes](../../battle/battle-cutscenes.md),
[cutscene data](../../battle/battle-cutscene-data.md), [maps](../../world/common-maps.md) and
[scripting](../../world/common-scripting.md). Shared owners remain authoritative;
selected36 operands, order, polarity and changed-state cases are checked here.

Original paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`sf2enums.asm`, `sf2const.asm` and the original macros own constants and
encoding. `map/` and `entity/` below mean `code/common/scripting/` children;
`battleloop/` means `code/gameflow/battle/battleloop/`. Fnnn means flag nnn;
ordinals are zero-based and intervals half-open. The registered USA ROM
SHA-256 remains `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source equality, terrain decoding or runtime observation is claimed.

Minimum sufficient evidence is the complete selected scene/list/text join,
formation/definitions/helpers and actual current lowering/consumers. Reuse
the immutable18,285,979-byte init/script pair with `local/issue1005/pair.txt`
provenance, and the registered12,000,595-byte canonical map export checked
against `manifests/extractions/canonical-map-import.json`. Selected linear
reads and one pair/canonical load per direct command have a one minute/512MiB
budget; peak memory is unmeasured. Unique small receipts stay in ignored
`local/issue1128`. No re-export, new fixture/parser/platform/tool tests,
normal/full/old suite, SDK, Godot, emulator, H3/H4 or screenshots. Reassess
material contradictions or expansion before increasing this work.

## Admission, first/repeat entry and F88

**Confirmed:** the predecessor owns CheckBattle row36 `$7B32..$7B39`,
`53,0,0,28,32,255,255`. F436 admits36; held536 clears436 while still
admitting it on that invocation. Neither486 nor possession of Zynk26 is an
admission predicate. CheckBattle publishes crop0,0 size28×32, not
CURRENT_BATTLE. `code/gameflow/mainloop.asm:MainLoop` moves selected d7.w
to d1.w and calls j_BattleLoop with map53 in d0.w. Crop, entrance/camera
coordinates and formation positions are separate operands.

`code/gameflow/battle/battleloop_1.asm:BattleLoop` clears PLAYER_TYPE.b,
sets399, then tests88. Clear88 clears seconds, saves incoming d0/d1
**words**, assigns their low bytes to CURRENT_MAP/CURRENT_BATTLE, sets
base VInts and runs before. It restores those words and rewrites the two
bytes after before, without rolling back reached flags, crop, layout,
party lists or actors. It clears flags90–105 inclusive, heals living and
immortal allies, initializes all ally positions, all enemy positions and
AI memory, in that order. It clears d0.w, calls LoadBattle for combatant0,
then the start wrapper. The next @Start instruction is the excluded
ActivateEnemies call.

The two48-entry relative-word tables select slot36/byte72:
before `$47AD0` relative to `$47A88` → bbcs_36 `$4D88A`;
start `$47B74` relative to `$47B2C` → ms_Empty (sole FFFF word).
`ExecuteBeforeBattleCutscene` tests450+CURRENT_BATTLE=486 and skips when
held, without setting it. `ExecuteBattleStartCutscene` independently
tests486, sets it **before** dispatch, and runs that empty script.
ExecuteMapScript still resets its inline-action scratch pointer to
FF9004_LOADING_SPACE, clears text skip, conditionally waits for view when
dialogue is open and clears VIEW_SCROLLING_SPEED on return.

| State at the relevant read | Result within this slice |
| --- | --- |
|88 clear;486 clear throughout|Complete before, fresh reconstruction/load, set486 and empty start.|
|88 clear;486 held throughout|Skip both hook bodies; fresh heal/positions/load still run.|
|536 held while436 admits|Same fresh path;486 independently gates opening;36 still fails upgrade-list membership.|
|486 changes between wrappers|Each wrapper follows its own read; no shared immutable latch snapshot.|
|CURRENT_BATTLE changes inside a service seam|Each selector reads live state; saved-word restoration repairs only current map/battle bytes at its named point.|
|Interrupted prefix|Reached mutations survive;486 can remain clear after before work or held before start dispatch completes. Neither proves control or completed presentation.|

Held88 instead restores saved seconds, clears88, clears AI memory and
VIEW_TARGET_ENTITY.b, then calls LoadBattle **without clearing d0**. It
branches directly to @IndividualTurns_Loop; the following branch to @Start
is unreachable. Fresh current-byte assignment, both hooks, region reset,
healing and combatant position/stat initialization are bypassed. Retained
CURRENT_MAP/BATTLE, crop, coordinates/activation and turn buffer remain inputs.

With intact incoming map53, `GetEntityIndexForCombatant` takes the nonnegative
branch and masks with FF: it reads alias offset53 at `$FFB175` in the valid
64-byte `$FFB140..$FFB180` table. This is the offset rebuilt for enemy149
(149−96=53), **not Bowie0, combatant53, or Battle35's scratch offset76**.
PositionBattleEntities clears/rebuilds all enemy aliases128–159 from retained
signed X/Y and neutral status. If149 is absent/off-map/neutral it writes FF
there; a retained eligible149 gets its newly packed physical slot. It does
not limit this F88 reconstruction to spriteset36's twenty declarations.
Fresh initialization would set149 X−1, but F88 bypasses it.

LoadBattle stores the fetched byte T as view target. Negative T falls back
to physical0 for coordinate reads; otherwise it masks3F, with no49-record
guard. Thus FF/80→0,00→0,30→48,31→49 and7F→63 are source arithmetic cases,
not observed values. It later uses the view target again in LoadMap.
ClearAiMemory resets48 last-target bytes to FF and48 memory bytes to0
(`$FFB1AC..$FFB1DC`, `$FFB1DC..$FFB20C`), not the adjacent newly-triggered
queue word `$FFB20C..$FFB20E`, region flags90–105 or turn buffer. F88 also
bypasses ExecuteMapScript's scratch-pointer reset. No successful restoration,
safe caller state or individual-turn execution is established.

## Temporary population and complete opening

**Confirmed:** `data/battles/entries/battle36/cs_beforebattle.asm`
`$4D88A..$4D9A4` contains39 top-level commands/202 encoded bytes plus
four4-byte inline action streams. They reach ce_4D964 at `$4D964`;
the64-byte list is a six-byte main header, seven eight-byte records and
two-byte terminator. Counts are storage/record quantities, not elapsed time.

| Record / logical identity | X,Y / facing | Sprite / action |
| --- | --- | --- |
|main /Bowie0|19,1 /Down|actual ally0 /eas_Idle|
|0 /Peter7|18,0 /Down|ALLY_PETER /eas_Init|
|1 /128|16,16 /Up|GESHP159 /eas_Init|
|2 /129|19,0 /Down|CARAVAN62 /eas_Init|
|3 /130|12,16 /Right|PRISM_FLOWER157 /eas_Init|
|4 /131|13,17 /Up|PRISM_FLOWER157 /eas_Init|
|5 /132|14,19 /Left|PRISM_FLOWER157 /eas_Init|
|6 /133|15,20 /Up|PRISM_FLOWER157 /eas_Init|

csc42 disables display/services, consumes the main header, calls
InitializeMapEntities/LoadEntityMapsprites, then reenables them. ClearEntities
resets49 physical records,64 alias bytes to0 and walking scratch. It does not
make every logical reference a separately allocated entity. Ordinary followers
allocate first, independently of joined/active membership. Map53 is absent
from the complete overworld list, so even held65 does not choose the Caravan
overworld prefix. Scene Caravan129 is an explicit fixed actor, distinct from
FOLLOWER_A30 and any transport state. Fresh exploration's ms_Void setup does
not supply this scene list; loadMapEntities explicitly replaces population.

F67 can allocate Peter7 before the fixed list; that duplicate ally row then
skips allocation, retaining its follower position/action until later commands.
F76 allocates Zynk26 as a follower; it does not join or activate26. FOLLOWER_B31
can be Astral under74, Oddler under68, then Astral under70, with the last alias
write winning while earlier physical allocations remain. No held producer
leaves31 at **alias0**, which can direct its Init, setPos, move, speech and
facing at Bowie. It is not an unresolved-reference refusal. F76 clear similarly
leaves26 at0, but the selected branch skips its commands. Arbitrary aliases,
finite follower-script pointers and physical capacity are not guaranteed safe.

Let n be allocated followers, p=1 if fixed Peter allocates, else0. Geshp128
is physical n+p+1, Caravan129 follows, then four flowers. Bowie is physical0.
Actual ally mapsprites depend on live class/Rohde membership; while currently
in battle36, dead allies do not take the outside-battle blue-flame path.
No party/HP/membership mutation occurs in this scene.

The complete top-level order follows; all four inline streams are
`moveDown4;endActions` (two-byte movement plus two-byte terminator).

| Index | Command / boundary |
| --- | --- |
|0|textCursor2946|
|1|loadMapFadeIn53,15,0|
|2|loadMapEntities ce_4D964|
|3|setActscriptWait Bowie0,eas_Init|
|4|setActscriptWait Peter7,eas_Init|
|5|setPos Peter7,18,0,Down|
|6|setActscriptWait FOLLOWER_B31,eas_Init|
|7|setPos FOLLOWER_B31,20,0,Down|
|8|jumpIfFlagClear76,cs_4D8D6 (index11)|
|9|setActscriptWait Zynk26,eas_Init|
|10|setPos Zynk26,63,62,Down|
|11|cs_4D8D6: playSound MUSIC_BATTLE_THEME_1|
|12|fadeInB|
|13|cameraSpeed$30|
|14|entityActions Bowie0:moveDown4,endActions|
|15|entityActions Peter7:moveDown4,endActions|
|16|entityActions FOLLOWER_B31:moveDown4,endActions|
|17|entityActionsWait129:moveDown4,endActions|
|18|nextSingleText00,128 →2946|
|19|setCamDest10,14|
|20|nextText00,128 →2947|
|21|nextText00,128 →2948|
|22|nextSingleText00,128 →2949|
|23|setActscript130,eas_AnimSpeedx2|
|24|setActscript131,eas_AnimSpeedx2|
|25|setActscript132,eas_AnimSpeedx2|
|26|setActscript133,eas_AnimSpeedx2|
|27|csWait40|
|28|setCamDest14,2|
|29|setFacing Peter7,Right|
|30|nextSingleText00,Peter7 →2950|
|31|setFacing FOLLOWER_B31,Left|
|32|nextSingleText00,FOLLOWER_B31 →2951|
|33|setFacing FOLLOWER_B31,Down|
|34|setFacing Bowie0,Up|
|35|nextSingleText00,FOLLOWER_B31 →2952|
|36|nod Bowie0|
|37|setFacing Bowie0,Down|
|38|csc_end|

### Action, HP, alias and wait semantics

`map/mapscriptengine_1.asm:csc15_setEntityActscript` resolves the alias,
sets its wait timer to the physical slot byte, installs the pointer, and
for Wait spins until that pointer equals eas_Idle. It has no alive guard.
`eas_Init` sets acceleration0, speeds32/32, disables acceleration and
deceleration, enables obstruction/auto-facing, clears flagsA bits5/6,
disabling entity/map collision checks at their respective consumers,
orients Up, clears immersed, sets global sprite size24,
refreshes sprite and jumps idle. Requested Down facing after Init is separate.

setPos and setFacing use AdjustScriptPointerByCharacterAliveStatus: only
ally IDs0–29 at HP0 skip their four-/two-byte payload;31 and128+ bypass
that HP test. Init and inline moves still run for dead Peter/Bowie, and nod
also has no HP guard. Scene HP precedes fresh healing: dead Peter's setPos
can skip here even though later initialization heals him. F76 held/HP0
Zynk still gets Init but skips its63,62 position; F76 clear skips both.
Those coordinates do not delete an actor or set combatant positions.

csc2D resolves each actual alias, sets the slot timer and applies AND9F to
flagsA, **clearing bits5/6**. The movement builder writes relative opcode4,
X0/Y+4 and wait1, followed by a jump to eas_Idle, advancing its shared scratch
pointer. The three non-Wait installations permit concurrent entity service;
only129 is explicitly waited on. The wait tests script-pointer identity,
not an aggregate all-four destination barrier. If31 aliases0, index16
replaces Bowie's index14 script; alias installation and interleaving can
change results. With distinct stable aliases and completed requests the
nominal endpoints are Bowie19,5; Peter18,4; follower20,4; Caravan19,4.
These are conditional arithmetic endpoints, not natural reach or duration.

`entity/entityscriptengine_2.asm:HasSameDestinationAsOtherEntity` tests bit5
of the **current moving entity at a0**. Clear returns no obstruction without
scanning; set permits scanning other entity records through a2. The scanned
entity's bit5 does not decide whether this scan runs or blocks movement.
esc04 retries a blocked move; esc01 checks remaining
travel and yields while nonzero. Bits0/2/1/3 respectively govern X acceleration,
X deceleration, Y acceleration, Y deceleration; bits5/6 must not be inverted
or described as motion bits. Ordinary controlled/random movement tests bit6
for map collision, a different call path from these relative scene moves.
No tile passability or schedule is proved by the four operands.

cameraSpeed writes word$30=48 to VIEW_SCROLLING_SPEED, not entity speed or
a40-tick wait. Both csc32 camera destinations set view target−1, call
SetCameraDestination and wait for scrolling completion. `eas_AnimSpeedx2`
sets flagsB bit4 via esc1E, then jumps idle; four non-Wait installs do not
move or attack the flowers. csWait40 is one Sleep request. nod freezes the
animation counter at FF, sleeps10, loads/transfers a nod sprite, sleeps20,
refreshes, sleeps10, then clears the counter; this is not a measured40-frame
presentation guarantee. Text/view/sprite/DMA/fade/entity services can affect
ordering and interruption. The scene itself neither activates combatants
nor changes scene facings to laser-table facings.

### Dialogue identity and complete tokens

textCursor2946 and seven sequential commands join gamescript.txt keys
0B82–0B88 and the source comments. S=nextSingleText; C=nextText. All modifiers
are00, without right-side/mirror bits. Prose below is authored short paraphrase.

| ID / mode / modifier / speaker | Paraphrase | Ordered tokens |
| --- | --- | --- |
|2946 /S /00 /128|Geshp remarks on the early arrival.|`["{N}", "{W2}"]`|
|2947 /C /00 /128|Geshp insults the Granseal party.|`["{W2}"]`|
|2948 /C /00 /128|Geshp says preparations are complete.|`["{N}", "{N}", "{W2}"]`|
|2949 /S /00 /128|Geshp announces the demonstration.|`["{W1}"]`|
|2950 /S /00 /Peter7|Peter asks Astral about Geshp's smile.|`["{W1}"]`|
|2951 /S /00 /FOLLOWER_B31|The follower explains the threat.|`["{N}", "{W1}"]`|
|2952 /S /00 /FOLLOWER_B31|The follower warns the leader.|`["{LEADER}", "{W1}"]`|

Geshp159 binds portrait45/bleep6 via spritedialogproperties.asm; Peter and
FOLLOWER_B use resolved appearance. The text naming Astral does not enforce
that31 is Astral. csc1D retains an already open portrait; otherwise it resolves
entity appearance. Single text waits for view, increments cursor, closes
portrait/text and sleeps10; continued text retains the window. W1/W2/N are
distinct controls. LEADER2952 calls UpdateForce and reads the first active-list
byte, not fixed Bowie. List prefixes rebuild in ID order while tails remain;
empty/changed lists or skipped text can differ. No choice/recruit/item token
occurs. **Inferred:** the scene introduces a prepared defensive threat;
it does not prove successful laser fire or a naturally reached battle.

## Map, full formation and definitions

**Confirmed:** the predecessor owns Map53's absent setup/ms_Void, resource
tables and two return warps. Its sole area is0,0..27,31; layer255, zero
secondary origins/autoscroll, both parallax256/256, default BATTLE_THEME_3.
Palette3/tilesets0,89,255,255,57 and chest24,27/F192/VALKYRIE remain declarations.
The opening explicitly loads53 with camera15,0, then explicitly supplies
ce_4D964; no missing map setup is invented. LoadMap reloads base blocks/layout,
flag/chest processing and crop0,0 size28×32 into working origin0,0:896 words/
1,792 copied bytes with64-word source stride. The rest is not cleared;
actors/cameras use working coordinates, not a coordinate validation rule.

LoadBattle fades, loads tilesets for live CURRENT_MAP, waits, clears services,
rebuilds physical actors and selects view from its supplied combatant (fresh0;
F88 alias53). It initializes sprites and calls LoadMap with d0=−1; **d1.b is
the map selector**, preserved as53 on the intact path. This reloads base and
crop, distinct from negative-d1 CURRENT partial reload. Area selection uses
the actual view/fallback coordinates, not crop origin. It then loads entity
sprites, base VInts, terrain, map music and fade-in. The scene's BATTLE_THEME_1
request and later area BATTLE_THEME_3 selection are separate. Terrainentries
slot36→BattleTerrain36→battle36/terrain.bin is a binding, not decoded terrain.
Global36 selects SWAMP and no neutral row. The placement's LEADER AI is
independent of the global leader-presence byte FF. FF is nonzero:
`code/gameflow/battle/cutscenes/battleendcutscenesend.asm:loc_47C48` tests
table_EnemyLeaderPresentFlags with tst.b and skips its post-leader-death
append path only for zero. Thus36 enables that path when its caller reaches
this chunk; full defeat/settlement behavior remains deferred. Drop table has
enemyEntity0/BATTLE_SWORD/F25; no outcome or award execution is covered here.

### Complete spriteset36

`data/battles/spritesets/spriteset36.asm` spans `$1B5F14..$1B60BE`
(426 bytes): four header bytes12/20/3/1,144 ally bytes,240 enemy bytes,
36 region bytes and two point bytes. Relative sections are0/4/148/388/424/426;
enemy rows start `$1B5FA8`, regions `$1B6098`, point `$1B60BC`.
Each entity record is12 bytes: index/definition0, X1, Y2, AI3, item word4–5,
primary order6/region7, secondary order8/region9, filler10/spawn11.
The last pair is consumed as a word then rotated/masked for activation.
The accepted inventory explicitly has rowContentTracked=false; these rows
are direct source checks, not attributed to a nonexistent row fixture.

All twelve ally records use HEALER1, NOTHING, NONE/15,NONE/15, filler0,
STARTING. Their identity bytes are formation slots, not actual party IDs:

| Slot | X,Y | Slot | X,Y | Slot | X,Y |
| --- | --- | --- | --- | --- | --- |
|0|19,5|4|20,4|8|16,3|
|1|19,4|5|17,3|9|21,4|
|2|20,3|6|18,4|10|21,5|
|3|18,3|7|19,3|11|17,4|

All20 enemy rows use item NOTHING, filler0, spawn STARTING. ID=128+ordinal.
Both orders/triggers are retained, including raw144|0:

| Ordinal / ID | Definition | X,Y | AI | Primary / region | Secondary / region |
| --- | --- | --- | --- | --- | --- |
|0 /128|EXECUTIONER|8,27|LEADER|MOVE_TO\|0 /0|NONE /15|
|1 /129|PRISM_FLOWER|11,7|HEALER1|NONE /15|NONE /15|
|2 /130|PRISM_FLOWER|6,11|HEALER1|NONE /15|NONE /15|
|3 /131|PRISM_FLOWER|12,16|HEALER1|NONE /15|NONE /15|
|4 /132|PRISM_FLOWER|13,17|HEALER1|NONE /15|NONE /15|
|5 /133|PRISM_FLOWER|14,19|HEALER1|NONE /15|NONE /15|
|6 /134|PRISM_FLOWER|15,20|HEALER1|NONE /15|NONE /15|
|7 /135|PRISM_FLOWER|18,23|HEALER1|NONE /15|NONE /15|
|8 /136|PRISM_FLOWER|7,26|HEALER1|NONE /15|NONE /15|
|9 /137|CHAOS_WIZARD|7,27|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|
|10 /138|SHAMAN|9,28|HEALER5|FOLLOW_ENEMY\|0 /15|NONE /15|
|11 /139|DRAGONEWT|14,10|SENTRY|NONE /15|NONE /15|
|12 /140|GRIFFIN|14,12|SENTRY|NONE /15|NONE /15|
|13 /141|BOW_RIDER|16,12|SENTRY|NONE /15|NONE /15|
|14 /142|PURPLE_WORM|12,15|ATTACKER3|FOLLOW_ENEMY\|15 /0|FOLLOW_ENEMY\|15 /2|
|15 /143|BOW_RIDER|9,14|ATTACKER1|FOLLOW_TARGET\|0 /0|FOLLOW_TARGET\|0 /2|
|16 /144|MIST_DEMON|17,20|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|
|17 /145|BOW_RIDER|18,21|ATTACKER3|144\|0 /1|NONE /15|
|18 /146|BOW_RIDER|6,21|SENTRY|NONE /15|NONE /15|
|19 /147|MIST_DEMON|8,22|SENTRY|NONE /15|NONE /15|

`sf2macros.asm:combatantBehavior` emits primary bytes directly:
MOVE_TO|0=$40, FOLLOW_ENEMY|0=$80, FOLLOW_ENEMY|15=$8F,
FOLLOW_TARGET|0=$00,144|0=$90, NONE=$FF. Preserve$90; it is not$80 or
FOLLOW_ENEMY|0. `ai/startaicontrol.asm`/`ai/command/moveorder.asm` distinguish
FF then bit6; a nonpoint order passes the **whole byte** to GetCurrentHp.
`battlefield/getmoveorderposition.asm` likewise uses whole d0 for combatant
X/Y unless bit6 selects a point (then masks the low nibble). Thus$90 takes
the combatant144 path, unlike$80's128; no target-selection/AI execution is
proved here. Point and trigger interfaces are recorded without geometry.

Each region is type4, unused byte0, four ordered vertices and trailing0,0:

| Region | Ordered vertices |
| --- | --- |
|0|0,0;0,31;27,21;6,0|
|1|0,10;0,31;27,31;27,10|
|2|0,22;0,31;27,31;27,22|

Point0 is8,27. Sloping region0 is retained verbatim. Region/point presence
does not prove inclusion, activation, movement or turn order.

### Selected definitions, current stats and inventories

Join enemydefs.asm and enemymapsprites.asm under data/stats/enemies/.
Stats below are `(level,maxHP,maxMP,baseATT,DEF,AGI,MOV)` before difficulty
and equipment refresh. All nine definitions have ENHANCED spell power,
initialStatus NONE, four item/spell slots; unlisted slots are NOTHING.

| Definition / index / mapsprite | Stats | Move type | Equipped item / spells | Resistance |
| --- | --- | --- | --- | --- |
|EXECUTIONER28 /92|33,64,0,46,46,45,6|REGULAR|BATTLE_SWORD /none|WIND_MINOR,LIGHTNING_MINOR,STATUS_MINOR|
|PRISM_FLOWER93 /157|40,62,0,16,55,44,0|REGULAR|none /LASER|ICE_MAJOR,FIRE_WEAKNESS,STATUS_IMMUNITY|
|CHAOS_WIZARD74 /138|36,53,49,47,48,59,5|MAGE|GREAT_ROD /BLAZE4|0|
|SHAMAN83 /147|32,56,46,45,42,40,5|HEALER|WISH_STAFF /AURA2,BLAST3|0|
|DRAGONEWT26 /90|32,60,0,44,42,46,6|REGULAR|HEAT_AXE /none|0|
|GRIFFIN49 /113|32,60,0,77,45,48,7|FLYING|none /none|WIND_WEAKNESS,LIGHTNING_WEAKNESS,FIRE_WEAKNESS|
|BOW_RIDER64 /128|33,61,0,44,55,44,7|CENTAUR_ARCHER|HYPER_CANNON /none|0|
|PURPLE_WORM27 /91|32,58,0,78,46,43,5|STEALTH|none /none|0|
|MIST_DEMON50 /114|36,68,28,80,41,51,6|HOVERING|none /MUDDLE2|WIND_WEAKNESS,ICE_MAJOR,STATUS_MINOR|

Prowess triples `(critical,double,counter)` are Executioner
`(SILENCE,1IN16,1IN16)`, Prism Flower `(150_1IN32,1IN32,1IN32)`,
Chaos Wizard `(150_1IN32,1IN32,1IN16)`, Shaman `(MAGIC_DRAIN,1IN32,1IN32)`,
Dragonewt/Griffin `(150_1IN8,1IN16,1IN16)`, Bow Rider `(150_1IN8,1IN32,1IN32)`,
Purple Worm `(POISON,1IN16,1IN16)`, Mist Demon `(SILENCE,1IN32,1IN16)`.
unknownByte is the definition index except Shaman/Prism Flower0.
AI priority is1 for flowers,3 for Shaman,2 otherwise; extra BIT4 for
Chaos Wizard/Griffin/Bow Rider/Mist Demon, BIT5 for Dragonewt/Purple Worm,
none for Executioner/flowers/Shaman. STARTING initialization preserves the
upper priority nibble and replaces lower activation bits with filler/spawn:
flowers1000, Shaman3000, all other selected enemies2000, before activation.

The five equipped items have only an ATT equip effect: Battle Sword66 +35
(range1), Heat Axe40 +32 (range1, BLAZE2 use), Hyper Cannon53 +40 (range2–3),
Wish Staff94 +26 (range1, ATTACK use), Great Rod95 +28 (range1).
All are WEAPON/RARE; Heat Axe/Wish Staff also BREAKABLE. Placement NOTHING
skips additions in InitializeEnemyItems; it does **not empty copied equipment**.

InitializeEnemyStats copies56 bytes/14 longwords, fills currentHP/MP from
maxima, installs X/Y, movement type plus placement AI, definition index,
both orders/triggers/items and activation, adjusts baseATT for difficulty,
then UpdateCombatantStats. Ordinary difficulty applies truncated5/4 once;
SUPER applies it twice. `code/common/stats/updatecombatantstats.asm` resets
current stats from base, applies status effects, then equipped effects in
four-slot order. These enemy definitions have no initial status and none of
their five weapons changes DEF/AGI/MOV; those current values match the table.
All eight flowers therefore start HP62/MP0/AGI44/MOV0, status0 and current
ATT20 ordinarily or25 SUPER; no AGI high-bit/double-turn claim follows.

| Definition | Current ATT ordinary / SUPER after equipped effect |
| --- | --- |
|EXECUTIONER|92 /106|
|PRISM_FLOWER|20 /25|
|CHAOS_WIZARD|86 /100|
|SHAMAN|82 /96|
|DRAGONEWT|87 /100|
|GRIFFIN|96 /120|
|BOW_RIDER|95 /108|
|PURPLE_WORM|97 /121|
|MIST_DEMON|100 /125|

HealLivingAndImmortalAllies visits all30 allies independently of membership:
living allies plus Peter7/Lemon28 at HP0 get maxHP/MP, retain STUN/POISON/CURSE
at the pre-refresh mask, then refresh current stats/equipment. UpdateCombatantStats
rebuilds curse from equipped cursed items rather than preserving a stale curse
bit; STUN reduces current MOV1 and AGI5 through the owning saturating helpers,
while poison persists without a stat decrement here. Dead ordinary allies
stay dead/unrefreshed. Actual ally class/base stats/equipment and values remain
inputs, not invented from a scene identity or formation slot.

InitializeAllAlliesBattlePositions resets all30 X to−1, calls UpdateForce,
then consumes the active prefix in ID order. Living members consume successive
formation rows; dead members spend an outer iteration without advancing the
formation pointer. Only the first12 list entries are examined; excess entries
LeaveBattleParty (active flag cleared, X−1), without rebuilding list tails/counts
afterward. At N=0 there is no entry precheck: N−1 underflows and an out-of-list
entry can be read. Unjoined-but-active flags do not add a member to the prefix.
This is not a guarantee of twelve living allies or Bowie's placement.

All32 enemy IDs128–159 are visited. The Jaro exception is only battle32.
All twenty declared36 rows take normal initialization; there are no HIDDEN
rows. Absent148–159 take mode0/activation0, max/currentHP0 and X−1, without
refreshing other definition/current fields. Their trigger read uses the first
enemy pointer (0/15), not nonexistent records. ClearAiMemory then applies
the bounded reset described above, without clearing the newly-triggered word.

PositionBattleEntities resets physical/alias state again, packs eligible
combatants in ID order from signed X/Y and enemy-neutral bit, **not HP**,
masks coordinates3F, scales384 and installs Down/eas_Standing. Skipped
combatant aliases become FF; untouched follower aliases30/31 remain0.
With A eligible allies, twenty selected enemies occupy physical A..A+19.
Scene128 now denotes Executioner128 and scene129 becomes flower129;
scene130–133 likewise resolve through the rebuilt combatant population.
In particular scene flowers130/131/132/133 at
12,16/13,17/14,19/15,20 become combatant131/132/133/134 at those coordinates.
There is no Geshp or Caravan combatant declaration. F536 skips neutral
allocation when held; clear536 finds no36 neutral row either.

`randombattles.asm` contains exactly3,8,9,10,14,17,19,21,24,26,35.
UpgradeRandomBattleEnemies is still called for each normally initialized
enemy, but IsBattleUpgradable fails for36 and returns the original definition
**before** DetermineBattleUpgrade, level/category/RNG logic. F536 and Bowie's
level therefore do not produce Battle35's upgrades or airborne exclusion scan
on an intact36 selector. A changed live selector is a different input.

### Eight laser-facing declarations and consumer boundary

`data/battles/global/laserbattles.asm` length-prefixes battles36,43,0 and
selects the corresponding pointer in the same order. Battle36's first table
has24 bytes. GetLaserFacing in `code/gameflow/battle/getlaserfacing.asm`
selects live CURRENT_BATTLE, indexes the chosen table by combatant low byte
masked COMBATANT_MASK_INDEX_AND_SORT_BIT (7F), and treats FF as LASER_NONE.
It does not read the scene facing or physical entity number for that lookup.

| Enemy ordinal / combatant | X,Y | Table direction / value |
| --- | --- | --- |
|1 /129|11,7|Down /3|
|2 /130|6,11|Right /0|
|3 /131|12,16|Right /0|
|4 /132|13,17|Up /1|
|5 /133|14,19|Left /2|
|6 /134|15,20|Up /1|
|7 /135|18,23|Left /2|
|8 /136|7,26|Up /1|

Ordinal0 and9–23 are FF, including all other declared enemy slots9–19 and
four beyond the declared20. Table length24 is not32 or a roster count.
The helper's later target/ray algorithm, attack damage, visual effects and
turn invocation are outside this slice. Initial Down physical facing does
not overwrite this independent data; matching four scene facings at selected
coordinates does not bind scene IDs130–133 to combatant IDs130–133.

## Actual offline lowering and current consumers

**Confirmed about current code:** OriginalPrograms with registered canonical,
scene_maps=[53], and complete registration of cs_beforebattle.asm compiles
bbcs_36 and interior successor cs_4D8D6 as15 and59 instructions (74 total).
The first ends with an explicit jump; its15 instructions are not the whole
opening. All seven scene entities and four inline streams lower. The only
top-level native is cs_4D8D6 index2 cameraSpeed. Each of its four flower
motion instructions also contains a native ac_animSpeedX2 action before
jump/idle. Neither is silently accepted as implemented animation.

Lowered population has19 ordinary followers. For intact53 this matches the
native table choice, including65 held: the Battle35 missing-overworld-prefix
case is not selected. Actual SceneEntities.Build allocates and initializes
fresh zero aliases; missing31/26 can alias physical0. Removed-reference
tombstones are a distinct current-state boundary. It also uses start-class
prepared ally appearance rather than the original live-class calculation.
ProgramRunner's setPos/face operations have no native ally-HP skip; changed
dead-Peter/Zynk/Bowie inputs therefore differ even if the scene compiles.

| Actual owner | Expressible shape / refusal, difference or missing binding |
| --- | --- |
|[OriginalPrograms](../../../../src/sf2tool/remake_exploration_programs.py), [ExplorationContentReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Scene-map/entities, flags, text, camera targets, motion and gestures have shapes. cameraSpeed becomes UnsupportedInstruction and reached `program-opcode` refusal. A reached nested native becomes UnsupportedEntityAction and `entity-action` refusal in EntityActionRunner. Earlier service/resource failures can occur first.|
|[SceneEntities](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapEntityAllocator](../../../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [EntityActionRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Follower/fixed allocation, aliases, Init, relative moves, slot timers, AND9F and idle waits have consumers with49-slot capacity guards. Native arbitrary alias/HP/class/service behavior is not established. Fresh zero aliases must retain the accepted alias0 interpretation.|
|[ExplorationTextRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs), [MapPartyMembership](../../../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Token/portrait/window and party-list shapes exist. Selected seven text/material bindings and empty/stale-list behavior are not proved by lowering. FOLLOWER_B speech depends on actual appearance.|
|[PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Placement/region/point/terrain and before→init→load→start shapes exist. Pin admits1/2/3 and rejects36; selected36 content/definitions/world/route/load/start bindings are absent. F88 NewBattle refuses `suspended-battle-entry`.|
|[BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs)|Requires controlled roster-only/already-refreshed allies, difficulty0, admitted status/control and formation capacity. Five equipped enemy loadouts refuse `enemy-equipment-refresh`; placement NOTHING cannot erase them. Does not reproduce all30 nonroster allies, stale/empty prefixes or interrupted native initialization; computes before publication.|
|[Map preparation](../../../../src/sf2tool/remake_exploration_maps.py), [ExplorationViewRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../../../remake/game/src/Battles/BattleMapViewport.cs)|53's layer255/zero origins/unit parallax/zero autoscroll meets existing profile predicates; nonzero view speed separately refuses `field-view-speed`. Current board selection uses crop origin while original LoadBattle uses actual view/fallback. Prior base53 conversion supplies no encounter binding.|
|[ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs)|Sprite/text/audio/fade/camera/gesture cues have consumers. This does not prove original AnimSpeedx2, selected resources, DMA/service timing, complete presentation or usable input.|

The predecessor already observed actual event-enabled53 conversion throwing
UnboundLocalError for unassigned setup, with zero partial programs. That
completed result is inherited; it is not rerun, repaired or converted into
success here. Its native item/flash/copy/reload limits remain. A direct
scene compilation bypasses that failed map-event preparation and cannot
close it. ProgramRunner's BattleStartFinished invokes BattleAdvancer;
that consumer lies beyond the original stop before ActivateEnemies and
was not executed. Battle01–03 support does not establish36 playability.

## Reproduction, preserved results and Unknowns

The ignored direct recipe joins the complete39-command corpus, four inline
streams,64-byte entity list, seven text modes/speakers/tokens, selected hooks,
F88 alias,32 placement records, three regions/point, nine definitions/five
items, current stats, laser table and actual connected compiler/consumer code.
It checks changed-state boundaries, two-path/nav-only scope, private/UTF-8/LF
and local links. After loading current private configuration in the same
PowerShell process, use an unused receipt name:

```powershell
uv run --locked python -X utf8 local/issue1128/review.py <disasm-root> --canonical <registered-canonical-json> --receipt local/issue1128/direct-reproduction-01.json --committed
uv run sf2 verify plan --base origin/main --head HEAD
```

The source argument contains sf2enums.asm, code/ and data/. Inspect clean
committed planner and actual exact-head scoped CI. ADR0004's applicable
adversarial checklist is applied; parser/schema/fixture/H3 obligations are
N/A because none are added or changed. Suggested normal verification is
SKIPPED under the explicit documentation scope, not a passed execution.

Preserve [Roft](./roft-town-interactions-and-zynk-recruitment.md)'s original
b571a496 audit/CI39537957812 that missed alias0 and accepted d5bffb02
focused/CI39538519434 correction; [Nazca/Roft](./nazca-to-roft-and-battle35-admission.md)'s
collision correction; [Dojo](./dojo-exploration-and-sheela-recruitment.md)'s
follow2=(−24,0)/local-action refusal; [post-crash](./post-crash-ship-and-nazca-overworld-return.md)'s
complete-import/native/reload limitations; the [Battle34 outcomes](./battle34-outcomes-and-sky-orb-handoff.md)
owner's statistics correction; and the twelve completed old-comparison
failures with their existing owners. No old audit/suite is repeated or
relabeled interrupted, and no selected36 success closes those boundaries.

**Unknown:** natural entry/order and actual caller words/flags, membership,
HP/MP/status/class/equipment, aliases/followers/transport and F88 retained
combatants/crop/scratch/queue/turn state; decoded geometry/collision/terrain
and resource/material bindings; interruption/interleaving and complete
motion/view/text/sprite/audio/fade/nod services; VInt/DMA/rendering timing,
usable input and save/load persistence. None is an automatic acquisition
queue. Activation, individual turns, laser algorithms, combat and outcomes
remain outside this slice.
