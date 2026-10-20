# Battle29 initialization and opening

Evidence date: 2026-10-20. **Confirmed:** Map0/Battle29 fresh entry can run
`bbcs_29` before healing and constructing the battle party. Its priest/devil
scene is temporary: LoadBattle replaces it with the selected formation and
fifteen enemy declarations. The start hook is empty but its wrapper sets
F479. Stop at `BattleLoop:@Start` before ActivateEnemies. F88 instead reloads
supplied state and reaches the individual-turn loop without either hook.
**Inferred:** the scene portrays a blocked shrine entrance and a priest
revealing a devil occupation. **Unknown:** natural reach, actual incoming
state, completed services, presentation, control and persistence.

## Agreement and provenance

Standard rigor for [Issue #1058](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1058)
under data [epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
scene entities, party membership, combatants, intro flags and supplied suspend
state have different lifetimes. Accepted base is
`f4346dfa31ee275b1e41474bb1d8521fcacafed3`, including #1056/PR1057.
This document and one predecessor navigation link are the exact owned paths.
No shared index, schema, fixture, tool, remake, design or translation changes.
Activation, turn generation/execution, combat, outcomes, Map0 post-battle
exploration/Zalbard door scene and shrine interior are excluded.

Start with the accepted [Map0 admission](./tristan-caves-and-shrine-exterior-admission.md#conditional-consumption-current-lifecycle-and-destination-admission).
Reuse [Battle27 initialization](./battle27-initialization-and-opening.md),
[Battle26 roster consumers](./battle26-initialization-and-roster.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[spritesets](./battle-spriteset-data.md), [global data](./battle-global-data.md),
[maps](./common-maps.md) and [script consumers](./common-scripting.md).
Their common rules are checked at Battle29's actual selectors below; a
different encounter's scene, neutral cast or upgrades are not transferred.

Paths below are relative to `disasm/` in clean pinned
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing evidence binds USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
this slice does not rebuild or newly observe it. Constants/widths are owned by
`sf2enums.asm`, `sf2const.asm`, `sf2macros.asm`, `sf2mapmacros.asm` and
`sf2cutscenemacros.asm`. Fnnn denotes flag nnn; indices are zero-based and
address intervals half-open. Short `battleloop/`, `battlefunctions/` and
`cutscenes/` paths are under `code/gameflow/battle/`; `entity/` and `map/`
are under `code/common/scripting/`.

Minimum evidence is selected source, existing fixture/schema relationships
and the unchanged 18,285,979-byte private map-init/script pair (accepted
0.083s provenance). Linear selected reads/comparisons are budgeted below one
minute/512 MiB, small receipts, no acquisition/API cost or new material
package. Replan contradictory evidence or material supporting-system growth;
no binary map decode, parser infrastructure or runtime is needed.

## Entry, before and start wrappers

**Confirmed:** `code/gameflow/mainloop.asm:MainLoop` passes d0=0/d1=29
to j_BattleLoop after CheckBattle selects row29. Admission requires429,
writes crop `(0,0,32,38)`, and, if529 is held, clears429 while still admitting
29 in that call. Battle28 belongs to Map47, not this entrance.

`battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`) orders:

1. Clear PLAYER_TYPE.b, set399, check88. Clear chooses fresh entry.
2. Clear SECONDS_COUNTER.l; save incoming d0/d1 **words**; assign low bytes
   to CURRENT_MAP/CURRENT_BATTLE; install base VInts; call before wrapper.
3. Restore saved words and reassign current map/battle bytes; clear all
   region flags90–105, independently of the four declared local regions.
4. HealLivingAndImmortalAllies; InitializeAllAlliesBattlePositions;
   InitializeAllEnemiesBattlePositions; ClearAiMemory.
5. Clear d0.w; LoadBattle for combatant0; ExecuteBattleStartCutscene;
   reach @Start and stop before ActivateEnemies.

Both complete hook tables have48 relative **word** slots. Slot29/byte58 in
`data/battles/cutscenes/beforebattlecutscenes.asm` is `$47AC2`, relative to
base `$47A88`, target bbcs_29. The same slot in `battlestartcutscenes.asm`
is `$47B66`, base `$47B2C`, target ms_Empty. The latter body in
`map/ms_empty.asm` is only `$FFFF` (`$47A4E..$47A50`).

`cutscenes/beforebattlecutscenesstart.asm` checks CURRENT_BATTLE+450 =479,
skips on held, and does **not** set it. `battlestartcutscenesstart.asm`
checks the same flag and sets it **before** dispatch, even to ms_Empty.
Both save d1, and on dispatch save/restore d0/a0; ExecuteMapScript preserves
its interpreter registers. Empty dispatch still clears text-skip and runs
the shared terminator tail. F479 is an admission latch, not proof that any
scene/service completed. Interruption in before can leave479 clear; after
the start flag write it can leave479 set with an incomplete suffix.

Each wrapper reads current battle again for table selection after checking
the flag. Later roster/terrain/neutral consumers likewise use their own live
reads. A change between reads can select different data without changing the
already checked flag. Restored arguments repair fresh current bytes, not
crop/state changed during the before scene. Held479 skips hooks but still
runs fresh reset/heal/initialization/load. Every suffix requires earlier
services to return; the tables are not atomic scene transactions.

## Temporary scene population and mapping requirements

**Confirmed:** `data/battles/entries/battle29/cs_beforebattle.asm` spans
`$4C3DC..$4C5E6`. bbcs_29's map-script body ends at ce_4C5AE. The retained
corpus has85 top-level commands (376 bytes); nine inline movement blocks
add42 bytes and four inline action blocks add48, giving466 bytes through
`$4C5AE`. Do not add top-level widths alone and call that the whole body.
The entity list is56 bytes: six-byte main header, six eight-byte fixed rows,
two-byte end marker. It is not a battle spriteset or a named party preset.

| List record | Position / facing | Sprite operand / initial action |
| --- | --- | --- |
|Main header|15,33 /Up|Bowie's actual ally sprite; ordinary main eas_Idle|
|Fixed0|16,34 /Up|ALLY_PETER7 /eas_Init|
|Fixed1|15,30 /Down|PRIEST192 /eas_Init|
|Fixed2|63,63 /Down|DEATH_MONK141 /eas_Init|
|Fixed3|63,63 /Down|ORC_LORD86 /eas_Init|
|Fixed4|63,63 /Down|WIZARD136 /eas_Init|
|Fixed5|63,63 /Down|WORM84 /eas_Init|

csc42_loadMapEntities disables VInts/display, reads header **words**, calls
InitializeMapEntities, loads mapsprites and reactivates display/services.
That consumer clears physical state/mappings, allocates flag-selected
ordinary followers **first**, then fixed records, then main physical0.
Map0 is neither follower-excluded map nor overworld. The complete ordinary
[follower rules](./battle26-return-through-pacalon-entry.md#conditional-map02-population-and-first-eventcontrol-read)
apply: F67 can supply Peter before fixed0; the fixed ally row is skipped if
his lookup byte is already nonzero. F74 and F70 supply Astral to FOLLOWER_B31;
F68 supplies Oddler earlier than F70, so later enabled aliases overwrite31.
No local command enables these flags or creates Astral unconditionally.
F64/RAFT_MAP can additionally affect raft31 construction; PLAYER_TYPE was
cleared at entry but this consumer still samples actual live state.

With F ordinary follower allocations, priest/devil logical128–132 map to
consecutive physical slots F+1..F+5 if Peter was allocated as follower, or
F+2..F+6 if fixed0 allocates him. They occupy lookup32–36. Main is physical0;
Peter and FOLLOWER_B are separate aliases. Missing31 retains the clear-time
zero alias and can address main, rather than a missing Astral. Different
enabled followers, alias collisions and capacity change these relationships.
DeclareNewEntity has no general capacity guard; arbitrary supplied-state
populations are not proven safe by this ordinary scene.

The six non-main records are not six enemies. Scene128 begins as priest,
then receives EFFECT2 and DARK_KNIGHT sprite changes without changing an
enemy stat record. Scene129 is Death Monk, not roster129 Orc Lord; scene130
is Orc Lord, not roster130 Master Monk. Scene131/132 are Wizard/Worm, while
roster131/132 are Worm/Wizard. LoadBattle clears and rebuilds these physical
entities/mappings later. No identity/HP/item transfer from the scene is implied.

## Ordered story, text and action consumption

**Confirmed:** the complete top-level bbcs_29 map-script stream has no flag, branch, choice,
join, damage, inventory or pending-map-event producer. Its ordered phases are:

1. Cursor2718; direct loadMapFadeIn Map0 at camera request10,28; load the
   entity list. Install/wait eas_Init on Bowie, Peter and31, setting Peter
   to16,34/Up and31 to14,34/Up. Submit MITULA_SHRINE music, fadeInB,
   camera speed `$30` (word48). Move each up1; only31's sequence waits.
2. Priest refuses entry; Peter asks Astral;31 hesitates. Move31 up1/wait,
   face Right and ask Bowie. Bowie faces Left and answers. Peter moves
   up1/wait, faces Left and insists. Bowie faces Right;31 nods, agrees
   about storytellers/legends, then urges a break for the entrance.
3. Camera10,26. Inline speed48,48 then jump eas_Idle/wait for Bowie,31,
   Peter. Start31 Up3; Bowie Left1/Up2; Peter Left2/Up1, all without sequence
   wait; csWait10. Priest speed64,64/idle, then Up2/Left1 without sequence
   wait; waitIdle128, face Down, waitIdle Peter; two warning/revelation texts.
4. Submit SOUND_COMMAND_FADE_OUT. Three successive mosaic-out / camera /
   position-Down / mosaic-in / csWait40 groups for128: camera12,19 and
   position16,23; camera8,15 and12,18; camera9,9 and14,13.
5. entityFlashWhite128,30; submit BATTLE_THEME_1; stop128 animation;
   sprite EFFECT2(175), face Up/wait8, Left/wait8, Down/wait8; sprite
   DARK_KNIGHT85, start animation. Set129 to14,12,130 to15,13,131 to15,14,
   132 to11,14, all Down, each followed by wait30. Final priest/devil line,
   then31's two lines; csc_end.

The thirteen ordered text selections are authoritative gamescript keys
0A9E–0AAA, decimal2718–2730. The table gives meaning, not substitute English
localization. `{LEADER}` reads the current leader name; `{N}` is newline.

| ID | Logical speaker | Meaning / final control |
| --- | --- | --- |
|2718|128 /Priest|Cannot enter Mitula Shrine now /W1|
|2719|Peter7|Ask Sir Astral what to do /W1|
|2720|FOLLOWER_B31|Hesitation /W1|
|2721|31|Ask `{LEADER}`'s opinion /W1|
|2722|Bowie0|Uncertain answer /W1|
|2723|Peter7|Tell `{LEADER}` the answer is obvious /W1|
|2724|31|Agree; seek storytellers and legends /W2, nextText|
|2725|31|Make a break for it /W1|
|2726|128 /Priest|Halt; forbid entering the holy shrine /W2, nextText|
|2727|128 /Priest|Reveal the truth /W1|
|2728|128 /Dark Knight sprite|Devils occupy the shrine; go away /W1|
|2729|31|Devils again /W2, nextText|
|2730|31|Order `{LEADER}` to attack /W1|

`entity/getentityportaitandspeechsfx.asm` resolves logical lookup then the
**current physical sprite** against `data/spritedialogproperties.asm`.
Priest192 selects portrait PRIEST34 and bleep5; DARK_KNIGHT has no row and
falls back to PORTRAIT_DEFAULT=$FFFF/no portrait and bleep6. Bowie base/promo
select corresponding portraits and bleep5; Peter base uses portrait7/bleep4,
promo portrait48/bleep5. Actual Astral209 gives portrait31/bleep6. An Oddler,
main alias or other live sprite at31 selects its own entry. The dialogue
calling an actor Astral does not establish that mapping.

csc00 single-text and csc02 nextText both check skip, resolve portrait/speech,
wait for view, display and increment cursor. Single-text then closes portrait
and text and calls Sleep10; nextText retains windows and has no such close/
Sleep suffix. Speaker packed high bits15/14 are zero here; an already-open
portrait bypasses opening, while speech resolves anew. No ordinary helper
skips text on speaker HP. Successful unskipped sequence ends cursor2731.
W1/W2 controls belong to shared text services, not measured timing or proof
of a new button press. Debug/P2-Start can latch text-skip after interpreter
entry clears it: skipped texts do not increment or perform their suffix;
negative csWait commands also skip Sleep while other scene commands run.

csc19 positions and csc1B/1C start/stop first use
AdjustScriptPointerByCharacterAliveStatus: dead ordinary allies skip that
operation; enemies and follower31 bypass that HP guard. Actions and facing
have their own consumers and are not all protected by this guard. Logical
lookups are mutable: GetEntityAddressFromCharacter masks the ID, maps enemy
bytes by subtracting96, then shifts the unsigned lookup by5 with no missing
sentinel guard. Cleared aliases can select physical0; `$FF` can produce
offset `$1FE0` into ENTITY_DATA. Neither is a safely omitted actor.

The action consumers csc14/15/16/2D install actual scripts and test pointer
equality to eas_Idle for waits, not arrival from source comments. csc2D builds
relative motion and clears collision bits; its non-wait variants allow
overlap. ac_setSpeed writes component bytes, not tile counts or durations.
csc32 sets view target=-1, requests tile camera destination and waits for
scroll completion; csc45 supplies word speed. csc22 modes6/7 use the mosaic
mask/DMA loop, not entity creation/deletion. csc18 duration30 shifts right2
then DBF gives eight four-VInt flash cycles (32 explicit waits); no measured
wall time follows. stop/start set animation counter -1/0, not remove/create.
The final terminator conditionally waits if a dialogue window remains, clears
scrolling speed and restores registers; it does not restore Bowie tracking.
These source effects require their services and actors to exist and complete.

## Formation, complete roster and initialization

**Confirmed:** slot29 in the45-longword spriteset pointer table selects
BattleSpriteset29 at `$1B5480..$1B55FC`,380 bytes. Header is12 allies,
15 enemies,4 regions,2 points. Relative offsets are header0, allies4,
enemies148, regions328, points376. Twelve-byte combatant records encode
type/ordinal, X,Y, commandset, item word, primary order/region, secondary
order/region and filler/spawn bytes. All27 rows have item NOTHING=$007F,
filler0/STARTING0; all ally rows use HEALER1, NONE/15 twice.

| Ally ordinal | X,Y | Ally ordinal | X,Y |
| --- | --- | --- | --- |
|0|14,30|6|14,32|
|1|15,30|7|15,32|
|2|13,31|8|14,31|
|3|16,31|9|15,31|
|4|13,32|10|14,33|
|5|16,32|11|15,33|

HealLivingAndImmortalAllies scans30 allies, heals living plus Peter7/Lemon28
to maximum HP/MP, masks status to STUN|POISON|CURSE=$0007 then rebuilds stats
and equipment/curse. It does not recruit or choose a fixed cast.
InitializeAllAlliesBattlePositions resets all ally X=-1, calls UpdateForce
to rebuild ascending membership lists, then positions living active members.
Dead members consume a processed-member slot but not a formation row; surplus
beyond twelve LeaveBattleParty without an immediate count/list rebuild.
The count is decremented before first member access without an empty-list
guard. Slot count is neither twelve named allies nor safe arbitrary empty
state; class/HP/membership/items/status alter later stats/sprites/placement.

| Row / combatant | Type / ID | X,Y | Commandset | Primary / region | Secondary / region |
| --- | --- | --- | --- | --- | --- |
|0 /128|DARK_KNIGHT21|14,13|LEADER|MOVE_TO\|0 /15|NONE /15|
|1 /129|ORC_LORD22|15,13|ATTACKER3|MOVE_TO\|1 /15|NONE /15|
|2 /130|MASTER_MONK82|14,12|HEALER3|FOLLOW_ENEMY\|0 /15|NONE /15|
|3 /131|WORM20|11,14|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|
|4 /132|WIZARD72|15,14|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|
|5 /133|WYVERN46|6,16|ATTACKER1|FOLLOW_TARGET\|0 /2|NONE /15|
|6 /134|WYVERN46|25,17|ATTACKER1|NONE /3|NONE /15|
|7 /135|ORC_LORD22|18,19|ATTACKER1|FOLLOW_TARGET\|0 /3|NONE /15|
|8 /136|MASTER_MONK82|19,19|HEALER3|FOLLOW_ENEMY\|7 /3|NONE /15|
|9 /137|WIZARD72|11,21|ATTACKER3|FOLLOW_ENEMY\|11 /0|NONE /15|
|10 /138|MASTER_MONK82|11,22|HEALER3|FOLLOW_ENEMY\|9 /0|NONE /15|
|11 /139|WORM20|10,22|ATTACKER1|FOLLOW_TARGET\|0 /0|NONE /15|
|12 /140|WORM20|19,27|ATTACKER1|NONE /15|NONE /15|
|13 /141|WORM20|20,27|ATTACKER1|NONE /15|NONE /15|
|14 /142|WYVERN46|6,29|ATTACKER1|NONE /15|NONE /15|

Initialization scans128–159. All selected spawn words are0 (<HIDDEN2), so
all15 call InitializeEnemyStats; Battle32/Jaro's slot15 exception does not
apply. Missing143–159 get activation0, HP0 and X=-1, with trigger FF from
the unchanged row0 subsection pointer, not a complete field clear.
No starting-occupancy check occurs here. Battle29 is absent from the complete
eleven random-battle entries and neutral table: no random upgrade, and
neither clear529 (no group) nor held529 (skip scan) adds a neutral actor.

InitializeEnemyStats copies each56-byte definition, restores current HP/MP,
sets position/type, combines movetype upper nibble with row commandset lower
nibble, packs regions and orders. Trigger row sequence is
`FF FF FF 1F 1F 2F 3F 3F 3F 0F 0F 0F FF FF FF`;
move words are `40FF 41FF 80FF 00FF 00FF 00FF FFFF 00FF 87FF 8BFF
89FF 00FF FFFF FFFF FFFF`. NONE=$FF, MOVE_TO=$40,
FOLLOW_ENEMY=$80, FOLLOW_TARGET=0. These are declarations, not proof of
activation or an executed AI decision. NOTHING returns before AddItem;
definition weapons persist. Activation keeps definition `$F000`, combines
rotated spawn word under `$0FFF`: monks get `$3000`, others `$2000`, with
definition BIT4/BIT5 removed and no active/neutral/spawn-mode bit. Difficulty
ATT adjustment and UpdateCombatantStats follow. ClearAiMemory separately
writes48 last targets=$FF and48 memory bytes0, not a turn order.

### Selected definitions and region/point data

**Confirmed:** `data/stats/enemies/enemydefs.asm` supplies all six types.
Each has ENHANCED spell power, status NONE, four item/spell slots; listed
first item has three NOTHING followers, listed spells have NOTHING padding.

| Type | Level | Max HP/MP | Base ATT/DEF/AGI/MOV | Movetype | First item | Spells |
| --- | --- | --- | --- | --- | --- | --- |
|Worm20|25|45/0|60/40/34/5|STEALTH|NOTHING|none|
|Dark Knight21|27|50/0|36/48/40/7|CENTAUR|CHROME_LANCE\|EQUIPPED|none|
|Orc Lord22|27|46/13|40/44/37/5|REGULAR|HEAVY_LANCE\|EQUIPPED|DISPEL|
|Wyvern46|26|46/0|58/39/36/7|FLYING|NOTHING|FLAME|
|Wizard72|26|37/37|32/40/44/5|MAGE|FLAIL\|EQUIPPED|BLAZE\|LV3|
|Master Monk82|28|47/39|22/45/31/6|HEALER|BRASS_KNUCKLES\|EQUIPPED|HEAL\|LV4, DISPEL|

Wyvern resistance is ICE_WEAKNESS|FIRE_MAJOR; Wizard FIRE_MINOR; others0.
Prowess: Worm/Wyvern CRITICAL_POISON, Knight CRITICAL150_1IN4, Orc
CRITICAL150_1IN8, Wizard CRITICAL150_1IN32, Monk CRITICAL125_1IN4.
All use DOUBLE_1IN32 except Monk1IN8; counters1IN16 except Wizard1IN32.
The original difficulty routine's repeated BEQ makes all difficulties apply
floor(baseATT*5/4), with SUPER applying it again before equipment/stat refresh;
base values above are not final attack. This reuses the
[shared source distinction](./battle27-initialization-and-opening.md#definitions-weapons-spells-and-stats).

Every region is12 bytes, type4/unknown0, four ordered vertices, two trailing
zero bytes. Geometry membership/activation is deferred.

| Region | Ordered vertices |
| --- | --- |
|0|(0,0), (4,37), (31,13), (31,0)|
|1|(0,0), (0,22), (14,22), (31,9)|
|2|(0,0), (0,25), (28,13), (28,0)|
|3|(0,0), (0,14), (31,36), (31,0)|

Points0/1 are `(14,13)` /`(15,13)`; MOVE_TO operands0/1 reference those
indices. FOLLOW_ENEMY uses local enemy ordinal; FOLLOW_TARGET0 denotes live
target state, not fixed Bowie. Region15 is not a fifth local polygon.

## LoadBattle, map resources and replacement lifetime

**Confirmed:** `battlefunctions/loadBattle.asm` selects live CURRENT_MAP0,
fades out, detaches view, loads tilesets, waits, clears VInts, then calls
PositionBattleEntities. That clears49 physical records and mappings, scans
30 allies then32 enemies by nonnegative X/Y (and non-neutral for enemies),
without its own HP test. Coordinates are `(byte & $3F)*384`, facing Down,
eas_Standing, flying/hovering layer1. Fresh initialization normally supplies
eligible placements; F88/arbitrary supplied positions need not.

Stored enemy type selects mapsprite84/85/86/110/136/146 for Worm/Knight/Orc/
Wyvern/Wizard/Master Monk. All are below special240; with N allocated allies,
enemy row r uses physical N+r. At N=12, rows0–14 occupy12–26, with Wyvern
rows5/6/14 aerial. At most27 ordinary fresh actors, no Astral neutral group;
skipped combatants get lookup=-1. This rebuild does not preserve the scene's
logical128–132 physical identities, speeds, effects or positions.

LoadBattle restores d0=0 and maps Bowie for VIEW_TARGET_ENTITY and chosen
tile X/Y. A negative lookup falls back to physical0 for position reads without
creating Bowie. It calls LoadMap with **d0=-1/d1=0**: target-based view and a
full explicit map load, not LoadMap's negative-d1 CURRENT reload. Sprites,
base VInts, terrain, music and fade-in follow; Battle44 timer does not apply.

Map00 header at `$94CC6` selects palette0, tilesets `[5,255,40,41,42]`;
255 is an absent signed-byte slot. Its resource longs select map00 blocks,
layout, area, flag/step/roof/warp/chest/other tables and animation, rather than
the town/caves resources. Sole area `$94CF4..$94D12` plus endWord through
`$94D14`: bounds0,0..31,37; foreground0,0; background32,0;
parallax256,256 /16,256; autoscroll0,0 /0,1; layer255/nonzero;
MUSIC_MITULA_SHRINE. View selection uses actual target coordinates and first
area fallback. Battle area consumption overrides main origin/end to0,0 and
`(31*384,37*384)=(11904,14208)`, retaining other area fields.

LoadMapBlocksAndLayout rebuilds base blocks/layout, applies flag copies and
opened chests, then copies the supplied battle crop into64-wide working rows.
For admitted0,0,32,38 it copies1216 words/2432 bytes, not a compact32-wide
layout. Local formation/enemy/region/point coordinates equal source X/Y only
under that unchanged zero-origin crop. F840's existing2x3 block copy from
0,38 to14,10 is applied if held; no door scene or natural flag acquisition is
entered here. Map animation selects tileset46/two16-tile entries at `$170`,
delay20; its visible result is unobserved. No binary map payload is exported.

`data/battles/terrainentries.asm` slot29 selects BattleTerrain29 /incbin
`data/battles/entries/battle29/terrain.bin`; LoadBattleTerrainData decompresses
to BATTLE_TERRAIN_ARRAY. Existing terrain evidence is a48x48 logical grid,
distinct from32x38 crop and64-wide map storage. PlayMapMusic replaces shrine
music with BATTLE_THEME_1 when current battle is not255. The before scene
explicitly submits shrine music and later Battle Theme1 itself; declaration
and service order do not prove audible/VDP timing, navigation or usable input.

Selected global declarations additionally name custom background
MITULA_SHRINE_EXTERIOR19 (enemy-switch byte0), leader-present=-1 and no
halved-EXP membership. These are encounter metadata from `custombackgrounds.asm`,
`backgroundenemyswitch.asm`, `enemyleaderpresence.asm` and the experience
table, not execution of a battle scene, leader-death or reward consumer.

## F88 and changed-state cases

**Confirmed:** held88 still clears PLAYER_TYPE and sets399, restores saved
seconds, clears88/AI memory/view target, calls LoadBattle without clearing
d0, then branches directly to @IndividualTurns_Loop (following bra @Start
is unreachable). No fresh assignment, before/start, healing, region reset
or combatant construction. Supplied current map/battle/crop/stats/turn order
remain inputs, while physical entities/display are rebuilt. Flag traps and
ClearAiMemory preserve d0: this admitted d0=0 would select Bowie, unlike a
different incoming map/register. No natural suspend caller is established.
Stop before turn selection/execution; the next turn byte can be `$FF` and
branch to @Start, which does not make this an already generated new round.

| Changed premise | Required distinction |
| --- | --- |
|88 held/clear|Supplied selectors and turn state versus saved-word fresh construction; both rebuild physical population.|
|479 held/clear or changed between hooks|Skip scene/start separately; fresh construction still happens; start sets latch before empty dispatch.|
|CURRENT_MAP/BATTLE changes between consumers|Scene load, follower routing, spriteset, flag, terrain/music and neutral selection use actual reads; fresh restores only saved map/battle bytes after before.|
|429/529 held|Admission clearing429 is not479;529 supplies neither upgrades nor neutrals for29.|
|Follower flags/mode/raft/mappings differ|Peter duplicate suppression,31 aliases/absence and non-ally slot placement differ; dialogue names do not repair them.|
|Dead Bowie/Peter before healing|Selected position/start/stop commands skip, other actions/text have separate guards; later healing is not retroactive.|
|Dead/fewer/surplus/empty active party|Sorted live membership and placement-pointer behavior determine actors; no fixed twelve-person cast or safe inconsistent empty state.|
|HP/class/status/equipment/difficulty change|Healing, stat refresh, sprite selection and enemy initialized values follow actual inputs.|
|Crop/position/type/neutral/lookup changes|Load does not rerun admission; allocation and view fallback can differ from ordinary bounds.|
|Portrait state/debug text-skip/held input changes|Window/cursor/wait outcomes differ; movement/effects may continue.|
|Interruption during load/action/text/flag writes|Only reached prefix effects exist; neither479 nor table row proves scene completion.|

## Existing representations and actual consumer gaps

**Confirmed about the accepted repository:** source evidence, prepared Content
and executing Application/Godot are separate. The
[capability owner](../../remake/docs/capability-status.md) controls runtime claims.
No Battle29/Map0 package is prepared by this slice.

| Category | Accepted representation / actual consumer | Gap or unassessed behavior |
| --- | --- | --- |
|Evidence|`battle-spriteset-data-static-v1.json` and its fixture schema retain aggregate/header/address facts (`rowContentTracked=false`); private full source rows remain required. Global-data fixture/schema retain table summaries. Loop/control/cutscene/routing fixtures cover actual shared consumers.|Summary equality does not establish exact29 rows. Retained map-script corpus supplies bbcs_29's top-level macros/operands and source references; source supplies the complete inline blocks and entity list. Neither is production runtime data.|
|Encounter data|[BattleEncounterDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs) and [Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) express placements, item/order expressions, regions/points, area/scene/terrain.|The shared shape is expressible, but this reader's actual Pin admits only1/2/3 and rejects29. An encounter source document has no enemy baseline, live party or initialized-state guarantee. Missing29 definitions/start/content/material binding remains work; raw rows are not playable actors.|
|Fresh hooks/party|ExplorationBattleRoute carries intro/before/load/start programs; [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) skips before by intro, sets it before start, rebuilds membership/initializes through BattleTurnFlow.|Useful existing mechanism.29 route, encounter, policies and full changed-party inputs are not bound; current supported encounters do not establish these six enemy types/commandsets. F88 explicitly rejects under NewBattle; save/suspend persistence is Unsupported.|
|Scene population/motion/text|[Preparer](../../src/sf2tool/remake_exploration_programs.py) supports scene-map/loadMapEntities, source action streams including speed/idle jumps, positions/facing/sprite, stop/start, camera/text/window and effects; [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs) and EntityActionRunner consume prepared definitions.|Already expressible subsets require actual follower flags/aliases, sprites/portraits/text/font/audio and29 program selection. Complete dead/missing/aliased/interrupted source semantics are not established by compiler support.|
|Map/effect presentation|Content view/visual definitions express parallax, autoscroll, layers and resources. [ExplorationViewRunner.Validate](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs) admits only128/256 parallax and zero autoscroll; [BattleMapViewport.BuildTerrain](../../remake/game/src/Battles/BattleMapViewport.cs) requires fixed1:1 planes and zero autoscroll. Godot [ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs) has accepted bounded mosaic and white services.|Map0's ParallaxBX=16 / AutoscrollBY=1 profile is currently **Unsupported** in both consumers: `field-view-profile` and `battlefield-map-plane-motion-unsupported`, respectively. Resource binding alone cannot admit it. Map0 materials, animation, F840/layout and battle crop still require binding/review; the exact transformation/motion sequence and hardware presentation/parity remain unassessed separately from the supported effect mechanisms.|
|Completion/policy|BattleEntry consults SourceStory completed-battle policy and clears unlock before permitted completed entry.|Bind correct original admission policy for529; unrelated modern-repeat behavior is not original evidence. Activation, general AI/commandsets, outcomes and natural route remain outside this survey.|

Production consumes prepared Content, not research JSON/disassembly on demand.
No engine, preparer, Godot or emulator was launched for this consumer survey.

## Reproduction and remaining boundaries

Load ignored `local/private-inputs.ps1` in the launching process. The retained
pair recipe and prior provenance remain unchanged. The ignored one-off direct
review checks the clean pinned source, all27 rows/four regions/two points,
complete hook tables,85-command body plus inline/list bytes, thirteen text
records, selected definitions and actual initialization/load/script consumers,
existing fixture/schema joins and Content/Application/Godot limits:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1058/review.py <configured-source-disasm-root>
git diff --check
```

Review complete prose and every changed-state case; check UTF-8/LF, links,
exact two-path/navigation-only diff and private boundary. On clean committed
HEAD run `uv run sf2 verify plan --base origin/main --head HEAD`; preserve
actual exact-head scoped CI in the Issue/PR handoff. Documentation scope skips
blanket normal/full Python/.NET/Godot/H3/H4 and tests of review programs.
Retain #1056 handoff/0.201s independent receipt, #1054 and predecessor
failure/Unknown chain; twelve unrelated remake comparison failures are not
rerun or relabeled. No original observation is replaced by static review.

**Unknown:** natural route, actual incoming words/map/battle/flags/membership/
HP/mode/crop/turn state, live mappings/capacity/working layout/interleavings,
completed text/action/camera/fade/audio/animation services, rendering and
timing, usable control and save/load persistence. Deferred activation/turn/
combat/outcome and post-battle scenes do not become a runtime work queue.
