# Battle34 initialization and the Cameela/Geshp opening

Evidence date: 2026-10-20. **Confirmed (static):** admitted Map55/Battle34
can run a temporary Cameela/Geshp scene, then rebuild combatants and map
entities from the current party and spriteset34. F76 independently gates
two Zynk actions; it does not recruit him. The fixed scene Caravan is a
separate actor. Fresh entry ends here immediately before ActivateEnemies.
The F88 route instead reaches the individual-turn loop without fresh init.
**Unknown:** natural entry, incoming state, completed services, presentation,
usable control and persistence. This is not a runnable Battle34 claim.

## Agreement and evidence route

Standard rigor for [Issue #1100](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1100)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect the complete opening, roster, selected definitions and fresh-load
boundary for later content integration. Mutable flags, aliases, party lists
and separate scene/battle lifetimes require this connected account. Accepted
base is `64cc90763cab31c76a48c26aea4bc82dec14a087`, including the
[Moun/Nazca admission owner](./moun-to-nazca-and-battle34-admission.md).
Own this document and a navigation-only link there. No shared registry,
fixture, schema, design or engine changes; no stacked dependency or competing
research writer. Map55 exploration/ship scripts, region geometry/activation,
spawn execution, turn/RNG generation, individual turns, combat and outcomes
are excluded. Unknowns do not authorize acquisition.

Reuse the shared [battle loop](./battle-loop.md),
[spriteset](./battle-spriteset-data.md), [global data](./battle-global-data.md),
[cutscene](./battle-cutscenes.md), [cutscene-data](./battle-cutscene-data.md),
[map](./common-maps.md) and [scripting](./common-scripting.md) owners.
[Battle33 initialization](./battle33-initialization-and-opening.md) owns
the accepted shared initialization account; all selected34 rows, hooks,
scene actions and differences below are checked directly against source.
[Moun underground](./moun-underground-gyan-and-zynk.md) owns F76 production.

Original paths below are relative to `disasm/` at clean read-only
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Constants/widths come from `sf2enums.asm`, `sf2const.asm` and the cutscene,
entity and battle macros. Fnnn means flag nnn; ordinals are zero-based;
intervals are half-open. `map/` and `entity/` abbreviate
`code/common/scripting/map/` and `code/common/scripting/entity/`.
The registered USA ROM identity remains SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1 equality or runtime observation is asserted.

Minimum evidence is the complete selected scene/inline/list/text/table join,
its immediate native consumers, shared init and actual current consumers.
Reuse the unchanged 18,285,979-byte init/script pair with
`local/issue1005/pair.txt` provenance. Bounded linear reads, one pair load and
three selected in-memory lowered programs are budgeted below one minute and
512MiB per audit; receipts stay in ignored `local/issue1100/`. No pair
regeneration, decoding, new extraction platform, environment, runtime, SDK,
Godot, emulator, screenshots, old comparisons or broad suites. Reassess a
material contradiction or supporting-tool expansion before expanding scope.

## Incoming state, fresh entry and F88

**Confirmed:** the predecessor's ordered CheckBattle scan selects row34 at
`$7B24`: `55,0,0,32,32,255,255`. Held434 admits34 and writes crop0,0 size32×32.
Held534 also clears434 but still admits34 on that invocation. This does not
write CURRENT_BATTLE. `code/gameflow/mainloop.asm:MainLoop` moves selected
d7.w into d1.w and calls j_BattleLoop with map55 in d0.w. For an intact
Nazca entrance the other request seeds are30,30 Up, but the X word in d1 is
replaced by34. Sampled high bytes and later mutations retain their own
meaning; request coordinates are neither scene nor formation placement.

`code/gameflow/battle/battleloop_1.asm:BattleLoop` first clears PLAYER_TYPE.b,
sets399 and tests88. With88 clear it:

1. Clears seconds; saves incoming d0/d1 **words**; assigns their low bytes to
   CURRENT_MAP/CURRENT_BATTLE; sets base VInts; executes before.
2. Restores those words and reassigns the two current bytes. This repairs
   those bytes after the scene, not crop, layout, flags, lists or actors.
3. Clears region flags90–105 inclusive; heals living/immortal allies;
   initializes all ally positions, all enemy positions and AI memory, in order.
4. Clears d0.w and calls LoadBattle for combatant0, then the start wrapper.
   Stop at `@Start`, **before its ActivateEnemies call**.

Both hook tables contain48 relative words. Slot34/byte68 is `$47ACC`
relative to `$47A88` → bbcs_34 `$4D342`; start slot `$47B70` relative to
`$47B2C` → ms_Empty. Before tests450+34=F484 and skips when held, without
setting it. Start tests484, **sets it before dispatch**, then executes the
FFFF-only ms_Empty. Even empty ExecuteMapScript resets text-skip, conditionally
waits for view when dialogue is open, and clears scrolling speed. Stable held484
skips both bodies, but fresh healing/placement/load still occur. Interrupted
before may leave484 clear; interrupted start may leave it held. F484 does
not certify dialogue, a completed load or usable input. Both wrappers reread
CURRENT_BATTLE for flag and table selection; these are live selectors.
Changing484 between wrappers can skip only one body.

With88 held, BattleLoop restores saved seconds, clears88, clears AI memory
and the view-target byte, then calls LoadBattle **without clearing d0**.
It branches directly to `@IndividualTurns_Loop`; the subsequent branch to
`@Start` is unreachable. There is no fresh current-map/battle assignment,
before/start, healing, region reset or combatant placement. Intact incoming
d0.w=55 is used as a **combatant lookup operand**, not map55 or fresh0.
`battlefunctions/battlefunctions_0.asm:GetEntityIndexForCombatant` treats55
as nonnegative and reads alias offset55. PositionBattleEntities has just
rebuilt that offset as the alias for enemy151 (151−96=55), from retained
combatant coordinates/neutral state. It is not unused alias31 as in the
Battle33 entry. A skipped151 writesFF; an included151 supplies its packed
physical index. LoadBattle records that byte as view target; a negative byte
falls back to physical0 for coordinate reads, otherwise masks3F. Supplied
CURRENT_MAP/BATTLE, crop, roster and turn buffer still govern this load.
Stop at the direct individual-turn handoff; no turn or activation is claimed.

## Temporary population and the two F76 reads

**Confirmed:** `data/battles/entries/battle34/cs_beforebattle.asm`
`$4D342..$4D4B6` contains62 top-level records/292 encoded bytes. Seven
inline movement streams add28 bytes and one custom action adds12, reaching
ce_4D48E at `$4D48E`. Its40-byte list is a six-byte main header, four
eight-byte records and a two-byte terminator. These are storage counts,
not executed instruction or frame counts; interior labels do not add bytes.

| List record | Position / facing | Sprite / action | Identity consequence |
| --- | --- | --- | --- |
|mainEntity|29,29 Left|current ally0; idle at declaration|physical0|
|0|30,28 Left|ALLY_PETER7 /eas_Init|logical ally7, not a joined/HP condition|
|1|11,10 Down|CAMEELA156 /eas_Init|logical non-ally128|
|2|11,6 Down|GESHP159 /eas_Init|logical non-ally129|
|3|30,29 Left|CARAVAN62 /eas_Init|logical non-ally130|

csc42 disables services/display, consumes the main header and calls
InitializeMapEntities, then loads entity mapsprites and re-enables services.
That function clears49 physical records and64 alias bytes, resets walking
scratch, declares held-flag followers first, processes fixed records, and
declares main0 last. A fixed ally row is skipped if its alias is already
nonzero; a regular non-ally appends to aliases128 onward independently of
the follower count. Let K be the number of allocated followers and P=1 if
the fixed Peter row is newly allocated, otherwise0: ordinary Cameela,
Geshp and Caravan occupy physical `1+K+P`, `2+K+P`, `3+K+P`. Thus128/129/130
are logical references, not physical slot numbers or proof of battle slots.
Coordinates are masked3F and scaled384 internally.

Map55 is absent from `data/maps/global/overworldmaps.asm` and is neither the
Tactical Base nor Map37 Nazca Ship early-return map. InitializeFollowerEntities
therefore uses the ordinary19-row follower table even with65 held.
Its selected row76→Zynk26 uses his current ally appearance, independent of
joined26, active58 and HP; follower74/68/70 can successively overwrite
FOLLOWER_B31, and other duplicate rows can allocate multiple actors while
leaving only the last alias. The intact list bounds K+P at19: Peter's fixed
row adds only when his follower row is absent. Total population is at most23,
within the49 physical records; source allocation itself
has no capacity guard. No assumption of an ordinary party or unique alias
repairs alias conflicts. The accepted [follower owner](./moun-underground-gyan-and-zynk.md)
retains the full shared table and script/list behavior.

The fixed Caravan62 at alias130 is declared regardless of65, PLAYER_TYPE,
CARAVAN_MAP/X/Y or overworld population. BattleLoop has already cleared
PLAYER_TYPE. The scene never calls the ordinary overworld Caravan declaration
to create this actor and does not update its parked state. A retained/changed
vehicle state is not evidence for this fixed row's presence or position.

Two **independent** `jumpIfFlagClear 76` instructions surround different work:
ordinal8 skips only awaited Zynk Init plus setPos31,29 Left to cs_4D38E;
ordinal18 skips only his moveLeft3 to cs_4D3C6. Earlier follower allocation
sampled76 separately. A changed76 can therefore allocate without reposition,
reposition without later movement, or reach actions through an absent alias.
Neither test asks membership, active-party size or HP. setPos skips a dead
ally0–29 via AdjustScriptPointerByCharacterAliveStatus; script installations
and entityActions do not use that HP guard. Peter's setPos has the same
guard. FOLLOWER_B31 and non-allies bypass it. A zero/missing alias can target
physical0, and an overwritten alias targets its actual last writer. Later
healing does not retroactively correct these scene operations. No join,
joinBattleParty, recruitment ceremony or forced party replacement occurs.

## Complete opening order and native service meaning

**Confirmed:** ordinals below refer to the62-record corpus in the single
source file, with all inline/custom streams included explicitly:

| Ordinals / label | Ordered work |
| --- | --- |
|0–7 bbcs_34|textCursor2895; loadMapFadeIn55 camera21,22; list reload; awaited Init for Bowie, Peter; Peter setPos30,28 Left; awaited FOLLOWER_B Init; setPos30,30 Left.|
|8–10|First clear76 branch; otherwise awaited Zynk Init and setPos31,29 Left.|
|11–14 cs_4D38E|Geshp MOSAIC_OUT; MUSIC_BATTLE_THEME_1; fadeInB; cameraSpeed48.|
|15–20|Nonawaited moveLeft3 for Bowie, Peter,130; second clear76 branch; otherwise Zynk moveLeft3; then awaited FOLLOWER_B moveLeft3.|
|21–27 cs_4D3C6|2895; awaited Peter eas_Jump; camera7,5;2896,2897; camera7,4; Geshp MOSAIC_IN.|
|28–35|2898;128 Up;2899,2900,2901; awaited128 moveUp1;2902,2903.|
|36–41|Awaited custom128 `ac_setSpeed48,48; ac_jump eas_Idle; ac_end`; nonawaited128 moveUp4; sleep10; Geshp MOSAIC_OUT; waitIdle128;128 Down.|
|42–47|sleep30; sleep5; nonawaited128 eas_2xRightLeft; sleep120;128 Down;2904.|
|48–55|tintMap;2905,2906; flickerOnce; camera7,3; sleep60; awaited128 eas_Jump;128 Right.|
|56–61|2907,2908; camera21,22; FOLLOWER_B2909,2910; csc_end.|

Each moveLeft/Up stream terminates with endActions; the engine translates
relative displacement into an installed entity script ending at eas_Idle.
Waiting tests the actual action-pointer idle state, not merely elapsed time.
Up to four nonawaited movements can overlap service work; waiting for FOLLOWER_B
does not independently join all actors. With intact live aliases and
completed moves Bowie ends26,29; Peter27,28; Caravan27,29; FOLLOWER_B27,30;
participating Zynk28,29. Cameela goes11,10→11,9→11,5. These are conditional
endpoints, not runtime observations; changed/dead/missing aliases may differ.

`data/scripting/entity/eas_actions.asm` owns all named streams. eas_Init
zeros acceleration factors, sets speed32/32, clears acceleration/deceleration,
enables obstruction/auto-facing, clears entity/map uncollidability and
immersion, selects orientUp/size24, refreshes sprite and jumps idle.
eas_Jump disables auto-facing, installs relative0,−1, waits5, relative0,+1,
waits5, relative0,0, restores auto-facing and jumps idle. eas_2xRightLeft
sets Right/refresh/wait30, Left/refresh/wait30, then repeats that pair and
jumps idle. Sleep120 can interleave those services; it is not a proof that
four sprite changes or the idle transition completed. The custom stream
changes **entity speed** to48/48 and waits idle; cameraSpeed48 separately
writes VIEW_SCROLLING_SPEED.w. They do not share a speed authority.

csc32's setCamDest detaches VIEW_TARGET_ENTITY, calls SetCameraDestination
with tile operands and waits for view-scroll end. The scene's five camera
requests are7,5;7,4;7,3;21,22 plus its initial load21,22. csc16 waitIdle128
reads the current alias and spins on the exact eas_Idle pointer. Missing
services or changed scripts can prevent completion; it does not remove128.
csc15/custom csc14 initialize the timer from the resolved physical byte;
csc2D also masks flagsA with9F before installing movement. None makes a
transaction across all actors. Reached prefixes survive interrupted waits.

csc22 maps MOSAIC_OUT6 and MOSAIC_IN7 to the dedicated native loop, without
the ally-HP skip. The out seed is1, the in seed7FFF;16 iterations each
load/crop via sub_45E10/DMA the current mapsprite and call WaitForVInt three
times, shifting the mask in opposite directions. This does not delete Geshp
or clear alias129: his2905/2906 dialogue still occurs after mosaic-out.
Actual pixel appearance and duration remain Unknown.

csc3D tintMap calls LaunchFading with `(palette bitfield1,counter6,setting9
HALF_OUT_TO_BLACK)`; csc3E flickerOnce uses `(1,6,10 FLICKER_ONCE)`.
LaunchFading clears timer/pointer, installs the fields, waits until the
setting byte clears, waits another VInt, restores the previous max counter
and sets palette bitfield15. These are palette-service operations, not a
flag mutation, Geshp deletion or a second mosaic. fadeInB invokes the full
black fade service. The five sleeps10/30/5/120/60 are script delay operands;
P2 Start plus debug can suppress sleep/text work through SKIP_CUTSCENE_TEXT.
Static call order does not prove real time, completed fades/audio or control.

## Complete dialogue and token binding

**Confirmed:** textCursor2895 plus16 sequential text commands joins exactly
to `data/scripting/text/gamescript.txt` hexadecimal keys0B4F–0B5E and all
source comments. `S` is nextSingleText; `C` is nextText. Modifiers are00 for
128/FOLLOWER_B andC0 for129 (right-side plus mirrored portrait bits).

| ID / mode / speaker | Exact text and tokens |
| --- | --- |
|2895 /S /128|`Hello, Granseal soldiers.{W1}`|
|2896 /C /128|`Remember?  I promised that I{N}would see you again.{W2}`|
|2897 /S /128|`Come on.  Let's party!{W1}`|
|2898 /S /129|`Stop!  You need my war plan!{W1}`|
|2899 /S /128|`You again, Geshp?  Why don't{N}you leave me alone?{W1}`|
|2900 /C /129|`Zeon left me in charge.{W2}`|
|2901 /S /129|`It doesn't matter if you are{N}killed or not.{W1}`|
|2902 /C /128|`Then kill me, if you've{N}got the guts to do it!{W2}`|
|2903 /S /128|`I know.  You want me to be{N}die!{W1}`|
|2904 /S /128|`Geshp, you coward!{N}I'll kill you first!{W1}`|
|2905 /C /129|`I have no time to waste.{N}I must go.{W2}`|
|2906 /S /129|`Silly Cameela.{N}You can't survive without my{N}brains.{W1}`|
|2907 /C /128|`{LEADER}, did you hear that?{N}He said you would win!{W2}`|
|2908 /S /128|`He must be a bad{N}fortuneteller.{W1}`|
|2909 /C /FOLLOWER_B31|`We can get a ship and return{N}to Grans if we defeat her.{W2}`|
|2910 /S /FOLLOWER_B31|`{LEADER}, get her!{W1}`|

`data/spritedialogproperties.asm` binds Cameela156 to portrait44/bleep3 and
Geshp159 to portrait45/bleep6. The actual alias mapsprite supplies identity;
FOLLOWER_B is not intrinsically Astral. csc1D leaves an already open portrait
unchanged, otherwise reads the portrait modifier bits and current entity
appearance. Single text waits view, displays, increments cursor, closes
portrait/text and sleeps10; continued text retains the window. Text and
token waits are separate from entity/view/fade completion. `{LEADER}` at2907
and2910 calls UpdateForce then reads the first active-list byte. That
rebuilds ID-sorted joined/active/reserve prefixes and counts but does not
clear tails or validate a nonempty list. Empty or changed membership can
therefore expose stale byte state; LEADER is not fixed to Bowie. `{N}`,
`{W1}` and `{W2}` remain distinct controls. Source spelling `be{N}die` is
retained. No question, item token, reward or membership write is present.
**Inferred:** the scene frames a quarrel and a ship objective; dialogue
alone does not prove a ship unlock, successful fight or completed departure.

## Map, crop, terrain and fresh reconstruction

**Confirmed:** `data/maps/entries.asm:Map55` binds Map55s0_Blocks and
Map55s1_Layout; animation pointerFFFFFFFF means no selected animation payload.
`map55/00-tilesets.asm` `$B78B8..$B78BE` is palette3, tilesets20/255/255/255/255.
The sole area `$B78E6..$B7906` declares0,0..31,31, foreground/background0,0,
both parallax256/256, both autoscroll0, layer255 and MUSIC_BATTLE_THEME_1=34.
Flag-copy `$B7906..$B7908` and chest lists are terminator-only. No34-specific
block-copy command is present in this opening.

loadMapFadeIn sets black-fade state then csc48 loads explicit55 tilesets,
waits fade, disables services, packs camera21,22 as `(21<<8|22)*3` and calls
LoadMap. Explicit55 loads palette/blocks/layout, flag/chest work, then crops
using the live BATTLE_AREA words while CURRENT_BATTLE is non255. Intact
crop0,0 size32×32 copies1,024 words/2,048 bytes into the origin of the64-wide
working layout; the remainder is not cleared. No cell values or terrain
walkability are decoded here. The camera seed is an origin, not player
position. LoadMapArea selects by that origin, then the battle dimensions
override main bounds to local0,0..31,31 =11904/11904 internal endpoints.

Later LoadBattle uses current map55, fades out, loads tilesets, waits,
clears services and calls PositionBattleEntities. It resolves the supplied
combatant lookup for view coordinates, initializes sprites, then calls
LoadMap with **d0=−1** for view-target selection. This is not necessarily a
CURRENT-map reload: LoadMap's map selector is **d1.b**, left as explicit55
by the tileset loader, so this path reloads base blocks/layout and crop.
A changed selector takes its actual branch. It loads entity sprites, base
VInts, terrain, music and fade-in, then returns. Area selection uses actual
view-target/fallback coordinates and falls back to the first area when none
matches. This differs from choosing the area by crop origin.

`data/battles/terrainentries.asm` slot34 selects BattleTerrain34 and
`data/battles/entries/battle34/terrain.bin`; LoadBattleTerrainData indexes by
live CURRENT_BATTLE and decompresses to BATTLE_TERRAIN_ARRAY. This is a
binding/consumer observation, not a decoded terrain or ROM-equality claim.
Global row34 has custom background NAZCA, enemy-leader-presence byteFF;
only INSIDE_ANCIENT_TOWER is in the halved-EXP list. No34 neutral-entity or
random-upgrade battle row exists. PlayMapMusic's named substitutions do
not change this area's BATTLE_THEME_1. Leader/EXP/outcome execution is deferred.

### Complete spriteset34 declarations

`data/battles/spritesets/spriteset34.asm` `$1B5BCC..$1B5D76` is426 bytes:
four count bytes;29 twelve-byte combatant rows; six twelve-byte region
records; one two-byte point. Counts are12/17/6/1. Formation ordinals are
positions, not ally IDs. All12 ally rows declare HEALER1, NOTHING,
`NONE,15,NONE,15,0,STARTING`:

| Formation | X,Y | Formation | X,Y |
| --- | --- | --- | --- |
|0|28,29|6|27,30|
|1|29,29|7|27,29|
|2|29,30|8|27,28|
|3|30,29|9|28,28|
|4|30,30|10|29,28|
|5|28,30|11|30,28|

All17 enemy rows declare item NOTHING, filler0 and STARTING; orders below
retain both independently supplied trigger nibbles. Physical packing happens
later; combatant ID is128+ordinal.

| Ordinal / ID | Definition | X,Y | AI | Primary / region | Secondary / region |
| --- | --- | --- | --- | --- | --- |
|0 /128|CAMEELA|11,5|LEADER|MOVE_TO\|0 /4|MOVE_TO\|0 /5|
|1 /129|NECROMANCER|10,4|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|
|2 /130|SHAMAN|12,4|HEALER5|FOLLOW_ENEMY\|0 /15|NONE /15|
|3 /131|CERBERUS|18,17|ATTACKER3|FOLLOW_ENEMY\|4 /15|NONE /15|
|4 /132|PEGASUS_KNIGHT|17,17|ATTACKER2|NONE /15|NONE /15|
|5 /133|BOW_MASTER|17,18|ATTACKER3|FOLLOW_ENEMY\|4 /15|NONE /15|
|6 /134|MUD_MAN|16,10|ATTACKER3|FOLLOW_ENEMY\|7 /0|FOLLOW_ENEMY\|7 /3|
|7 /135|DRAGONEWT|14,14|ATTACKER2|FOLLOW_TARGET\|0 /0|FOLLOW_TARGET\|0 /3|
|8 /136|MUD_MAN|10,16|ATTACKER3|FOLLOW_ENEMY\|7 /0|FOLLOW_ENEMY\|7 /3|
|9 /137|MUD_MAN|17,6|ATTACKER1|FOLLOW_TARGET\|0 /1|FOLLOW_TARGET\|0 /3|
|10 /138|CERBERUS|17,8|ATTACKER3|FOLLOW_ENEMY\|9 /1|FOLLOW_ENEMY\|9 /3|
|11 /139|PEGASUS_KNIGHT|12,14|ATTACKER3|FOLLOW_ENEMY\|10 /1|FOLLOW_ENEMY\|10 /3|
|12 /140|BOW_MASTER|14,12|ATTACKER2|FOLLOW_TARGET\|0 /1|FOLLOW_TARGET\|0 /3|
|13 /141|DRAGONEWT|6,17|ATTACKER3|FOLLOW_ENEMY\|12 /1|FOLLOW_ENEMY\|12 /3|
|14 /142|PEGASUS_KNIGHT|4,17|ATTACKER3|FOLLOW_ENEMY\|13 /1|FOLLOW_ENEMY\|13 /3|
|15 /143|DRAGONEWT|17,4|SENTRY|NONE /15|NONE /15|
|16 /144|DRAGONEWT|8,17|ATTACKER2|FOLLOW_TARGET\|0 /2|NONE /15|

Every region has unused second byte0 and two trailing zero bytes. All four
coordinate pairs, including region3's fourth stored pair, are retained:

| Region / type | Point0 | Point1 | Point2 | Point3 |
| --- | --- | --- | --- | --- |
|0 /4|31,0|0,31|15,31|31,15|
|1 /4|31,0|0,31|7,31|31,7|
|2 /4|0,0|0,31|9,31|25,0|
|3 /3|0,0|0,31|31,0|0,0|
|4 /4|0,0|0,19|8,19|8,0|
|5 /4|8,0|8,19|22,5|22,0|

Point0 is7,7. These are declarations only; no polygon, activation, spawn,
AI movement or turn legality is inferred from them.

### Selected definitions, items and initialization effects

`data/stats/enemies/enemydefs.asm` and `enemymapsprites.asm` join the eight
selected definitions below. Stats are raw `(level,maxHP,maxMP,ATT,DEF,AGI,MOV)`
before difficulty/equipment/status refresh; raw AGI186 is not a claim of
effective turn speed. All have ENHANCED power, initialStatus NONE and four
item/spell slots; unused slots are NOTHING.

| Definition / index / mapsprite | Raw stats | Move type | Equipped item / spells | Resistance |
| --- | --- | --- | --- | --- |
|CAMEELA92 /156|50,99,0,42,52,186,6|REGULAR|IRON_BALL /none|WIND_MINOR,LIGHTNING_MINOR,ICE_MINOR,FIRE_MINOR,STATUS_IMMUNITY|
|NECROMANCER73 /137|30,47,42,45,44,51,6|MAGE|FLAIL /FREEZE3|LIGHTNING_MINOR,STATUS_IMMUNITY|
|SHAMAN83 /147|32,56,46,45,42,40,5|HEALER|WISH_STAFF /AURA2,BLAST3|0|
|CERBERUS24 /88|30,51,0,70,44,45,6|STEALTH|none /FLAME|ICE_WEAKNESS,FIRE_MAJOR|
|PEGASUS_KNIGHT48 /112|30,65,0,40,37,47,7|FLYING|CHROME_LANCE /none|STATUS_MINOR|
|BOW_MASTER63 /127|30,46,0,50,53,40,5|ARCHER|ASSAULT_SHELL /none|0|
|MUD_MAN25 /89|31,55,0,76,51,41,4|REGULAR|none /none|LIGHTNING_MINOR,ICE_MAJOR,FIRE_WEAKNESS,STATUS_IMMUNITY|
|DRAGONEWT26 /90|32,60,0,44,42,46,6|REGULAR|HEAT_AXE /none|0|

Prowess is Cameela CRITICAL125_1IN4/DOUBLE_1IN4/COUNTER_1IN4; Necromancer
CRITICAL150_1IN32/DOUBLE_1IN32/COUNTER_1IN16; Shaman
CRITICAL_MAGIC_DRAIN/DOUBLE_1IN32/COUNTER_1IN32; Cerberus
CRITICAL150_1IN32/DOUBLE_1IN16/COUNTER_1IN16; Pegasus Knight
CRITICAL150_1IN8/DOUBLE_1IN32/COUNTER_1IN16; Bow Master
CRITICAL150_1IN32/DOUBLE_1IN32/COUNTER_1IN32; Mud Man and Dragonewt
CRITICAL150_1IN8/DOUBLE_1IN16/COUNTER_1IN16. UnknownByte is0 for Cameela
and Shaman, otherwise their definition index. Priority is1 for Cameela,
3 for Shaman,2 otherwise; definition AI extras are BIT4 for Necromancer,
Pegasus/Bow, BIT5 for Cerberus/Mud/Dragonewt, none for Cameela/Shaman.

`data/stats/items/itemdefs.asm` gives Iron Ball102 ATT+44, WEAPON/RARE/CURSED/
BREAKABLE, BLAZE3 use; Heat Axe40 +32, WEAPON/RARE/BREAKABLE, BLAZE2 use;
Flail90 +19, WEAPON, no use spell; Wish Staff94 +26, WEAPON/RARE/BREAKABLE,
ATTACK use; Chrome Lance79 +31, WEAPON, no use spell; Assault Shell49 +25,
WEAPON, no use spell. Assault Shell range is2–3; the other five are1–1.
Placement NOTHING preserves the copied definition inventory, not an empty
loadout. InitialStatus NONE precedes UpdateCombatantStats/equipment; it
does not establish that refresh leaves no curse/status or stat effect.

Fresh healing visits all30 allies independently of membership: Peter7 and
Lemon28 even at HP0; other living allies regain maxHP/MP and retain only
STUN/POISON/CURSE before refresh; other dead allies stay dead. Ally placement
sets all30 X=−1 without clearing Y, rebuilds sorted joined/active lists and
considers at most12 list entries. A dead member consumes a loop iteration
but no formation row; excess members lose active status/X. With zero count,
the initial decremented count still permits a stale first-byte read; there
is no safe-empty guard. Dead, unjoined, full, oversized and stale parties
therefore cannot be replaced by an assumed12-alive formation. F76 alone
does not put Zynk into this list or into any fixed formation slot.

Enemy initialization visits32 slots128–159. The Jaro143 skip requires battle32
and does not apply to34. Each declared STARTING enemy gets its56-byte
definition (14 longwords), HP/MP maxima, placement, move type/AI command,
enemy index, both trigger nibbles and move-order bytes; there is no selected34
random upgrade. Definition priority survives in the upper nibble; lower bits
are replaced from the initialization word, so Cameela starts1000, Shaman3000
and the others2000 before activation. Trigger bytes are45 for row0,FF for
1–5/15,03 for6–8,13 for9–14 and2F for16. Orders use MOVE_TO0=40,
FOLLOW_TARGET0=00, FOLLOW_ENEMYk=80|k and NONE=FF, primary shifted8 OR secondary.
The difficulty helper's consecutive BEQs apply floor(5*ATT/4) at every
difficulty and repeat for SUPER, before the separate equipment refresh.
Cameela42 becomes52 (SUPER65), not her final equipped ATT.

Absent145–159 take the hidden branch with d1=0: activation/maxHP/currentHP0,
X=−1, no Y/enemy-index/inventory refresh. Their trigger reads use the first
enemy pointer (4/5), not15 absent records. ClearAiMemory writes exactly48
last-target bytesFF and48 memory bytes0, offsets0–47, with subq/BNE, not DBF.
The ranges `$FFB1AC..$FFB1DC` and `$FFB1DC..$FFB20C` exclude the checked
NEWLY_TRIGGERED_BATTLE_REGIONS **word** `$FFB20C..$FFB20E`. Neither byte of
that word is cleared here. Region flags90–105 are a separate operation.

PositionBattleEntities clears physical/alias state again, then packs allies
and enemies in combatant-ID order from signed X/Y bytes and enemy neutral
bit, **not HP**. Coordinates are masked3F, facingDown, eas_Standing. With A
placed allies the intact17 enemies occupy physical A..A+16. Scene Geshp129
becomes combatant Necromancer129's alias and scene Caravan130 becomes
Shaman130's alias; no Geshp/Caravan battle combatant is declared. Held534
skips neutral allocation; clear534 finds no34 row anyway. Scene endpoints,
follower population and scripts are not retained as the battle roster.

## Actual lowering and present consumer boundaries

**Confirmed about current code:** production consumes prepared Content,
not disassembly/research JSON on demand. A selected OriginalPrograms probe
with scene_maps=[55] registers the complete source once and compiles bbcs_34.
It follows both interior successors: bbcs_34 has15 instructions,
cs_4D38E10 and cs_4D3C6 108 (133 total). These are three connected programs,
not a15-instruction scene. Branches explicitly target the next program;
text cursor and service state must survive those transitions.

All nested streams are inspected: four list Init streams, four awaited Init
installations (including conditional Zynk), seven movements, two eas_Jump,
the custom speed/idle and eas_2xRightLeft. None contains native-call.
The nineteen prepared ordinary follower rows have no65 prefix, agreeing
with Map55's non-overworld choice. Known lowered effect cues are two
mosaic-outs/one mosaic-in; custom speed and all named actions are represented.
The first explicit native refusal is cs_4D38E instruction3 cameraSpeed;
later cs_4D3C6 natives are58 waitIdle,71 tintMap and81 flickerOnce. Earlier
map/resource, palette/fade, sprite service or alias failures can precede it;
no successful execution through that point is claimed.

| Actual owner | Expressible shape / selected refused or unbound boundary |
| --- | --- |
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py), [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Scene/branch/text/action shapes exist. native-call becomes UnsupportedInstruction and reached `program-opcode` refusal; nested native forms become UnsupportedEntityAction, though none is selected here. No cameraSpeed/waitIdle/tint/flicker implementation follows from their preserved source records.|
|[SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapEntityAllocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Follower/fixed allocation, aliases, Init, movements, jumps and sprite waits have consumers with49-slot guards. Position/face shapes do not encode the original ally-HP skip. Selected Map55 package, arbitrary alias/duplicate/capacity fidelity and interleavings remain unassessed.|
|[ExplorationTextRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs), [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|LEADER rebuild and explicit text/portrait/wait controls exist; selected sixteen texts, actual empty/stale-list behavior and appearance/material bindings are not established by lowering. No Zynk membership write should be manufactured from76.|
|[PrivateBattleEncounterReader.Pin](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Shared placement/region/point/terrain and before→init→load→start routes exist, but Pin admits1/2/3 and rejects34. F88 NewBattle refuses `suspended-battle-entry`. Selected34 declarations/definitions/party/flags484/434/534/world/start/load/resource bindings are absent from this evidence.|
|[BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs)|Requires controlled provenance, roster-only/already-refreshed policy, difficulty0, supported control and status; equipped enemy loadouts refuse `enemy-equipment-refresh`. Iron Ball and the five other weapons cannot be admitted by pretending NOTHING emptied inventory. Active count must fit formation, unlike the original stale/oversize path. It computes initialization before publishing; it does not reproduce arbitrary interrupted native prefixes or all30 non-roster allies.|
|[Map preparation](../../src/sf2tool/remake_exploration_maps.py), [ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs)|Map55 layer255/zero origins/unit parallax/zero autoscroll is representable and passes the field/board profile guards. Board selects area using crop origin; original load uses camera/view-target fallback. Native speed is refused and nonzero admitted ViewSpeed independently refuses `field-view-speed`. Representable metadata does not bind layout/crop/terrain.|
|[ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Mosaic, sprite, text, sound and black-fade cue consumers exist. Their host animation is not proof of the original16-mask/48-VInt mosaic or LaunchFading semantics; tint/flicker never reach those services through this lowering. Selected resource bindings, completed services, exact presentation/timing and usable control remain Unknown.|

The bounded next integration need is selected content/material/definition
binding plus explicitly admitted semantics for these four native operations,
party/equipment and changed-state boundaries. No engine work or new platform
is authorized by this research. Fresh original stop is before ActivateEnemies;
ProgramRunner's BattleStartFinished subsequently invokes BattleAdvancer, a
different current consumer boundary that is not exercised here.

## Verification and remaining Unknowns

Direct audit checks pin/clean upstream, unchanged pair metadata/provenance,
the complete62-record source/corpus join and all eight inline streams,
40-byte list,16 texts/tokens, actual helpers, all29 combatant/six region/one
point declarations, eight definitions/six items, selected map/terrain/global
and hook joins, shared fresh/F88/placement/clearing and recursive lowering
against actual consumers. ADR0004's adversarial checklist is applied to this
documentation scope: no parser/schema/fixture/test is added. The48-byte
AI-clear extent and adjacent checked word are explicitly retained.

Reproduction uses the pinned clone, the retained init/script pair and the
owning source symbols/paths above. For the lowering, from the repository root
after loading current private selections, run the existing compiler in memory:

```python
from sf2tool.remake_exploration_programs import OriginalPrograms
# upstream is the selected read-only clone root, above disasm/.
p = OriginalPrograms({"resources": {"standaloneScriptPrograms": [],
    "initSourcePrograms": []}}, upstream, scene_maps=[55])
p.register_file("disasm/data/battles/entries/battle34/cs_beforebattle.asm")
p.compile("bbcs_34")
print({name: len(value["instructions"]) for name, value in p.programs.items()})
```

Check the exact two-path diff, navigation-only predecessor addition, UTF-8/LF,
links, tables and private boundary; then on the clean committed head run
`uv run sf2 verify plan --base origin/main --head HEAD` and record the actual
exact-head scoped CI. Documentation policy excludes normal/full Python,
.NET/Godot/H3 suites and old comparisons; a planner suggestion does not make
an unrun suite pass. Ignored audit/lowering/receipt/handoff files are review
coordination, not a new maintained extractor or second source of truth.

**Unknown:** natural Nazca traversal/admission and incoming flags/words/party;
actual follower/alias population and independently changed76; completed
movement/view/text/fade/sprite/audio services and interrupted prefixes;
full equipment/status and nonordinary roster behavior in a future consumer;
decoded layout/terrain/materials; actual resumed151 alias/view state; usable
control and persistence. Region activation, spawning, turn/RNG, combat,
outcomes and Map55 exploration/ship behavior remain explicitly outside this
slice, not passed checks or an automatic runtime queue.
