# Battle40 initialization and opening

Evidence date: 2026-10-21. **Confirmed (static):** the accepted Map54
admission selects Battle40. This owner covers its complete before-battle
scene and fresh initialization, stopping before the first region activation.
Natural arrival, completed services and playable remake support are **Unknown**.

## Agreement, provenance and method

Standard rigor for [Issue #1158](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1158)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053),
from accepted base `e7dffc99379bb974e248e5b7ed1e32b1ed935462`.
Own this document and navigation only in
[the admission owner](./devils-head-labyrinth-and-battle40-admission.md).
The decision supported is future content's scene/state/initialization contract.
No registry, schema, fixture, extractor, engine or global routing change.
Region evaluation, turn generation, AI actions, the two region tile-change
scenes, battle outcomes and afterbattle remain separate work.

Reuse [Battle39 initialization](./battle39-opening-and-first-control.md),
[Battle38 initialization](./battle38-initialization-and-opening.md),
[spritesets](./battle-spriteset-data.md), [battle loop](./battle-loop.md),
[scripting](./common-scripting.md), [maps](./common-maps.md) and
[stats](./common-stats.md). Selected40 operands are checked here; prior
battle constants, addresses and no-region behavior are not substituted.

Original paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` means `code/gameflow/battle/`; `map/` and `entity/` mean
children of `code/common/scripting/`. sf2const.asm, sf2enums.asm,
sf2macros.asm and sf2cutscenemacros.asm own constants and encoding.
Fnnn is flag nnn; ordinals are zero-based; address intervals half-open.
The accepted USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source/ROM equality or original-runtime observation is claimed.

Minimum evidence is the complete selected scene, placement/region/point
tables and necessary shared callers/consumers. Reuse the immutable
18,285,979-byte init/script pair and `local/issue1005/pair.txt` provenance;
do not regenerate extraction or import layouts/assets. Work is linear
in selected source/consumer text plus one pair read. One selected
lowering answers which scene commands remain native. Receipts and
recipes belong to ignored `local/issue1158/`; estimate below4MiB
including coordination metadata (selected evidence alone below2MiB),
seconds per direct command, peak-memory target512MiB **unmeasured**.
No SDK, Godot, emulator, H3/H4, screenshot, old comparison or normal/full
suite. Reassess before expanding acquisition or cross-system machinery.

## Admission, hooks and the distinct F88 branch

**Confirmed:** accepted CheckBattle row40 at `$7B4E..$7B55` is
`54,0,0,32,44,255,255`. F440 must be held; F540 is tested after crop
writes and can clear440 while still returning40 that call. MainLoop
passes the resolved map54 in d0.w and selected40 in d1.w to j_BattleLoop.
The preceding owner retains all flag/admission/no-battle variants.
Its ms_Void exploration population is not this battle's scene.

`battle/battleloop_1.asm:BattleLoop` at `$23A84..$23BB4` clears
PLAYER_TYPE.b, sets399, then reads88. Clear88 resets seconds, saves
d0/d1 words, installs their low bytes as CURRENT_MAP/CURRENT_BATTLE,
sets base VInt functions and calls the before wrapper. On return it
restores those words and rewrites both low bytes, clears90..105
inclusive, heals living/immortal allies, initializes ally positions,
initializes enemies, clears AI memory, clears d0.w, calls LoadBattle,
then the start wrapper. The next instruction is @Start's ActivateEnemies;
this document stops **before** executing it.

| Complete48-word hook table / selected offset80 | Selected target |
| --- | --- |
|`data/battles/cutscenes/beforebattlecutscenes.asm`, base `$47A88`, slot `$47AD8`|bbcs_40 at `$4E4DC`|
|`data/battles/cutscenes/battlestartcutscenes.asm`, base `$47B2C`, slot `$47B7C`|ms_Empty, the single FFFF word|

`beforebattlecutscenesstart.asm` reads live CURRENT_BATTLE, tests
450+40=490 and skips when held; it never sets490. The start wrapper
reads the live selector and490 again, sets490 **before** dispatching
the empty start. Empty ExecuteMapScript still resets dword_FFB1A4
to FF9004_LOADING_SPACE, clears SKIP_CUTSCENE_TEXT, conditionally
waits for view scrolling when the dialogue window is open, then
clears view speed. Skipping the wrapper body bypasses those effects.

| State supplied at the actual reads | Source result |
| --- | --- |
|88 clear,490 clear throughout|Full scene, fresh reconstruction/load, set490, empty start.|
|88 clear,490 held throughout|Skip both hook bodies; fresh reconstruction/load still runs.|
|540 held while440 admits|Completion540 and intro490 are independent; no new hook bypass.|
|490 changes between wrappers|Each wrapper obeys its own read, without a shared immutable snapshot.|
|Selectors mutate during a service|Helpers consume their live selectors. The saved-word restoration repairs only the two named bytes afterward.|
|Interrupted prefix|Scene effects can exist while490 is clear;490 can be held before start returns. Neither proves a completed opening.|

Held88 restores saved seconds, clears88, clears AI memory/view target,
then calls LoadBattle with incoming d0 unchanged and jumps directly to
@IndividualTurns_Loop. The following bra @Start is unreachable. It
bypasses selector assignment, both hooks, healing, initial positions/
definitions, region clearing and generation. Retained selectors, crop,
combatants, flags, queue/cursor and RNG remain inputs; map54/battle40
are not installed by this branch merely because d0/d1 carry them.

For concrete incoming d0.w=54, LoadBattle saves54, rebuilds battle
entities using the **retained** CURRENT_MAP/BATTLE/combatants, restores54
and calls GetEntityIndexForCombatant. Its nonnegative low byte avoids
the96 subtraction; COMBATANT_MASK_ALL=FF leaves offset54. The byte read
is `$FFB140+54=$FFB176`, inside the64-byte table ending `$FFB180`,
at enemy alias22 (logical150), **not** ally54 or the prior map66
out-of-table read. PositionBattleEntities has just rebuilt that alias
from retained enemy150's signed X/Y and neutral bit, without an HP test.
Under an otherwise ordinary freshly initialized40 state,150 is absent
and the byte isFF. Changed retained position/activation can produce a
physical slot instead; this is not a saved-byte observation.

The fetched byte T becomes VIEW_TARGET_ENTITY. Negative T makes the
coordinate read use physical0; otherwise LoadBattle masks3F and scales
ENTITYDEF_SIZE_BITS. Arbitrary retained state is not validated against
the49 ordinary records. ClearAiMemory writes48 last-target bytesFF
and48 memory bytes0, not region flags, the checked word or the turn
queue. No suspended-state validity, chosen coordinates or control is
established by this branch analysis.

## Scene identities and population

**Confirmed:** `data/battles/entries/battle40/cs_beforebattle.asm`
spans `$4E4DC..$4E8A8`,972 bytes. bbcs_40 has175 top-level commands
encoding848 bytes;17 inline move streams contain21 move commands
and17 endActions words,76 bytes. The scene prefix therefore occupies
924 bytes through ce_4E878. Its48-byte table is main header6,
five records×8, terminator2. cs_4E544 is the flag-clear continuation,
top-level index15, not a second independent narrative scene.

| Logical identity | Table X,Y / facing | Record source |
| --- | --- | --- |
|main/Bowie0|16,39 /Up|live ally0 /eas_Idle|
|Peter7|15,40 /Up|ALLY_PETER /eas_Init|
|Lemon28|15,41 /Up|ALLY_LEMON /eas_Init|
|128|16,41 /Up|MAPSPRITE_CARAVAN62 /eas_Init|
|129|16,4 /Down|MAPSPRITE_ODD_EYE160 /eas_Init|
|130|63,63 /Up|MAPSPRITE_EFFECT3 178 /eas_Init|

csc42 calls InitializeMapEntities and LoadEntityMapsprites with
display/VInt services. ClearEntities clears49 ordinary physical
records and64 aliases. InitializeFollowerEntities runs first; Map54
is not an overworld, Tactical Base or Nazca Ship map, so the complete
ordinary19-row table is considered and the overworld65 Caravan prefix
is excluded. Each held row allocates; later duplicate aliases win.
FOLLOWER_B31 may bind Astral via74, Oddler via68, then Astral via70.
No matching row leaves its fresh alias0, so an unguarded operation on31
can affect Bowie. Incoming flags and live mapsprites matter to dialogue.

Peter and Lemon have explicit ally records: an existing nonzero alias
from67/71 causes InitializeMapEntities to consume that record's tail
without redeclaring or applying its position/script. Otherwise the
explicit record allocates that ally even when its follower flag is
clear. The subsequent waited Init and setPos commands act on their
aliases. These declarations neither recruit nor choose the battle party.
F76 supplies Zynk26 only through the ordinary follower table; the
conditional commands later initialize and park that alias. Other
ordinary followers remain present; five follow-script pointers do not
prove arbitrary simultaneous flag combinations safe.

With n allocated followers, let p/l be1 when Peter/Lemon require a new
record, otherwise0. Caravan128, Odd Eye129 and effect130 occupy physical
n+p+l+1 through n+p+l+3, while Bowie remains0. Their sprites are below
MAPSPRITES_SPECIALS_START240 and use regular allocation. Physical slots,
logical aliases, current mapsprite and combat kind are different
identities. In particular scene128 is Caravan, scene129 is Odd Eye;
fresh combat128 will be Odd Eye96,129 Demon Master75,130 Evil Bishop84.
LoadBattle replaces the scene and aliases instead of carrying these
scripts/positions into combat.

## Complete opening command order

Indices below are the175 top-level commands. B/P/L/F/Z mean Bowie0,
Peter7,Lemon28,FOLLOWER_B31,Zynk26; numbers128–130 are scene aliases.
Move streams always endActions. W means the command's named wait;
its absence means installation returns without waiting for that actor.
All29 text requests are expanded separately below.

| Index | Exact ordered requests |
| --- | --- |
|0–2|textCursor3036; loadMapFadeIn54,11,34; loadMapEntities ce_4E878|
|3–11|InitW B; InitW P; setPos P15,40/Up; InitW F; setPos F16,40/Up; InitW L; setPos L15,41/Up; InitW128; setPos12816,41/Up|
|12–14|jumpIfFlagClear76→cs_4E544/index15; otherwise InitW Z; setPos Z63,62/Down|
|15–19|cs_4E544: copy11,6/11×29→44,0; copy32,0/11×29→11,6; MUSIC_STOP; fadeInB; cameraSpeed0030|
|20–24|Install Up3 on B,P,F,128 in that order; install Up3W on L.|
|25–27|Text3036; Jump B without wait; JumpW P.|
|28–33|Install B Up1; P Up2; F Up1; L Up1,Left1,Up1;128 Up1,Right1,Up1W; csWait30.|
|34–46|setCamDest11,2; MUSIC_BATTLE_THEME_3; texts3037–3047 in order.|
|47–52|setFacing129 Left;3048; setFacing129 Down;3049;3050;129 Down1W.|
|53–62|texts3051–3060 in order.|
|63–70|entityFlashWhite129,30; tintMap; csWait20; flickerOnce; csWait10; tintMap; csWait20; flickerOnce.|
|71–72|Install130 eas_AnimSpeedx2 without wait; immediately install130 eas_DeactivateAutoFacing with wait.|
|73–91|The four effect passes and first hide/delay in the layout table below.|
|92–157|All33 subsequent tile copies and their individual delays below.|
|158–160|Restore44,0/11×29→11,6;3061; setCamDest11,31.|
|161–164|setFacing P Right; setFacing B Left;3062; F Left1W.|
|165–168|setFacing F Up; setFacing L Right;3063; F Right1W.|
|169–174|setFacing F Up; setFacing B Down;3064; install P eas_Jump without wait; nod B; csc_end.|

### Temporary working-layout edits and effect passes

`map/mapscriptengine_1.asm:csc34_setBlocks` reads three packed words,
calls CopyMapBlocks and sets plane-update bits0/1. The macro encodes
source X/Y, width/height and destination X/Y as bytes. CopyMapBlocks
uses64-word working-layout rows at FF0000, not the32-wide visible crop.
Each11×29 copy moves319 words/638 bytes. First save x11..21,y6..34
to x44..54,y0..28; replace it from x32..42,y0..28; finally restore the
saved rectangle. The three rectangles do not overlap. No graphics or
terrain meaning of the source tiles is inferred without decoding.

Every small copy below is source6,15,size1×1 (one word/two bytes).
Coordinates in comma lists are in execution order. Delays apply after
**each** listed copy; they are not one delay for the whole row.

| Index | Effect130 and copy destination / following delay |
| --- | --- |
|73–77|setPos16,8/Up; Up2W; copy16,6; setPos63,63/Up; csWait30.|
|78–82|setPos15,8/Up; Up2W; copy15,6; setPos63,63/Up; csWait20.|
|83–86|setPos17,9/Up; Up2W; copy17,7; csWait10 (no intervening parking).|
|87–91|setPos16,9/Up; Up2W; copy16,7; hide130; csWait5.|
|92–95|y7: x15,14; csWait5 each.|
|96–107|y8: x18,17,16,15,14,13; csWait4 each.|
|108–123|y9: x19,18,17,16,15,14,13,12; csWait3 each.|
|124–139|y10: x19,18,17,16,15,14,13,12; csWait2 each.|
|140–157|y11: x21,19,18,17,16,15,14,13,12; csWait1 each. There is no x20 copy.|

There are37 single-tile copies,40 setBlocks commands including save/
replace/restore. None writes387/388 or a battle-region flag. All37
destinations lie inside the saved rectangle, so the final restoration
replaces their working words with the saved values. This is temporary
scene mutation, distinct from the later two flag-conditioned changes.
LoadBattle subsequently reloads map data and reapplies incoming387/388
copies before cropping. Fresh battle90..105 clearing does not clear
387/388; ClearMapSetupTempFlags256..383 is an exploration helper, not
part of this fresh battle suffix. No permanent opening layout grant
or new saved flag is established.

### HP, alias, overwrite and service semantics

**Confirmed:** check the actual handlers, rather than applying one HP
rule to the scene. csc15/setActscript, csc2D/entityActions, csc18/flash,
csc2E/hide and csc26/nod do not call
AdjustScriptPointerByCharacterAliveStatus. Ordinary setPos/csc19 and
setFacing/csc23 do: allies0..29 with currentHP0 skip their four-byte
or two-byte operand payload respectively.30/31 and enemy-bit identities
bypass this helper. F76 does not bypass Zynk's HP0 setPos skip; its
Init still installs. Peter/Lemon's later immortal healing cannot
retroactively undo a scene skip. No scene command tests their party
membership. GetEntityAddressFromCharacter reads an alias without an
absence/HP/mapsprite validation; flags/HP/aliases are separate inputs.

All six selected InitW installations (B,P,F,L,128 and conditional Z)
replace that slot's follower/old script. eas_Init sets acceleration0,
speed32/32, acceleration/deceleration OFF, obstructable ON, entity/map
uncollidable OFF, autoFacing ON, Up, nonimmersed, size24, refresh,
then idle. csc15 writes the resolved physical slot byte to the read
timer and installs one script pointer; the Wait loop tests that pointer
against eas_Idle. csc2D writes the same slot timer and also clears
collision bits with9F and installs the inline script; its move stream
requests relative destinations and waits for destination before the
next move. Neither Wait form is a global parallel-action barrier.

eas_Jump is autoFacing OFF; relative0,−1; wait5; relative0,+1;
wait5; relative0,0; autoFacing ON; idle. It does not disable motion.
esc04 computes each destination from the then-current X/Y and does not
itself wait. Net symbolic offsets therefore do not prove an exact
return coordinate. Waiting only for P after the first B/P jump, L after
the initial five moves, or128 after the second group proves only the
named script-idle condition. Later installs can replace unfinished
scripts. Closing with Peter's nonwaited Jump and Bowie's nod is not a
join on Peter. Aliases that happen to share a slot can also replace
one another's script.

eas_AnimSpeedx2 sets the animation-speed flag then jumps idle;
eas_DeactivateAutoFacing turns automatic facing OFF then jumps idle.
They are successive writes to130's **same** script pointer, with no
wait between the first installation and its replacement. Whether the
first executes before an entity service interrupt is **Unknown**;
the second's wait does not prove that the first flag was ever applied.
Parking at63,63 is a coordinate write, distinct from HideEntity:
hide writes X/Y and destinations7000 hex, changes every matching alias
toFF and removes the follower-list occurrence. No later130 use occurs.

loadMapFadeIn54,11,34 requests fade/map/view services through csc37→48;
it does not execute exploration setup. CURRENT_BATTLE is already40,
so live crop0,0,32×44 applies; camera11,34 does not change that crop.
Later fadeInB is separate. csc45 writes view speed0030; csc32 detaches
the view target and waits for scrolling at11,2 then11,31. These waits
do not settle unrelated entity actions. PlaySound sends the requested
music command, without proving audible completion.

csc18 takes30, shifts right2 and runs DBF inclusively:8 flash cycles,
four WaitForVInt calls each,32 calls total, with sprite updates and
flagsB bit2 set/clear. It is not30 frames of measured white output.
tintMap launches palette mask1/period6/HALF_OUT_TO_BLACK; flickerOnce
uses mask1/period6/FLICKER_ONCE. LaunchFading waits on FADING_SETTING
via VInt, waits once more, restores the previous counter maximum and
palette mask1111. csWait uses the interpreter's low-byte Sleep request,
skipped under its debug text-skip state. Nod requests10/20/10 sleeps
plus mapsprite/DMA work, with no ally-HP guard. None supplies a measured
wall-clock duration, DMA/display completion or final input-ready frame.

### All29 texts, modifiers and complete tokens

textCursor3036 joins gamescript.txt keys0BDC–0BF8. S is nextSingleText,
C is nextText.129 always has modifierC0; all other speakers00. The
high bits become bit15/14 in the packed speaker word: csc1D passes
−1/−1 portrait parameters for C0,0/0 for00 when opening a new window.
They do not suppress speech or test HP. C retains windows; S closes
portrait/text and requests Sleep10. Debug skip changes text/cursor and
delay behavior. No yes/no branch or actual jewel/item transfer occurs.
The roles below are authored summaries, not the original dialogue.

| ID / hex | Mode / speaker | Authored role | Complete ordered tokens |
| --- | --- | --- | --- |
|3036 /0BDC|S /129|Odd Eye greets the party.|`{LEADER}{N}{W1}`|
|3037 /0BDD|S /P|Peter reacts to the figure.|`{W1}`|
|3038 /0BDE|S /129|Odd Eye recalls travelling with them.|`{N}{LEADER}{NAME;7}{W1}`|
|3039 /0BDF|S /P|Peter connects Oddler with Odd Eye.|`{N}{W1}`|
|3040 /0BE0|S /129|Odd Eye confirms his identity.|`{W1}`|
|3041 /0BE1|S /F|The follower identifies the attacker of Bedoe.|`{N}{W1}`|
|3042 /0BE2|S /129|Odd Eye addresses Astral.|`{W1}`|
|3043 /0BE3|S /F|The follower questions that recognition.|`{W1}`|
|3044 /0BE4|S /129|Odd Eye recalls Bowie's accounts.|`{LEADER}{N}{W1}`|
|3045 /0BE5|S /P|Peter challenges his purpose.|`{W1}`|
|3046 /0BE6|S /129|Odd Eye professes sadness.|`{N}{W1}`|
|3047 /0BE7|S /P|Peter rejects the sentiment.|`{N}{W1}`|
|3048 /0BE8|S /129|Odd Eye admits his malice.|`{N}{W1}`|
|3049 /0BE9|S /129|Odd Eye addresses Red Baron.|`{N}{W1}`|
|3050 /0BEA|S /L|Lemon reacts.|`{W1}`|
|3051 /0BEB|C /129|Odd Eye introduces his ultimatum.|`{N}{W2}`|
|3052 /0BEC|C /129|He demands deliberation.|`{N}{W2}`|
|3053 /0BED|S /129|He demands surrender of jewel and equipment.|`{N}{N}{W1}`|
|3054 /0BEE|S /P|Peter considers avoiding battle.|`{N}{W1}`|
|3055 /0BEF|S /F|The follower rejects surrender.|`{N}{N}{W1}`|
|3056 /0BF0|S /129|Odd Eye emphasizes his strength.|`{N}{N}{W1}`|
|3057 /0BF1|S /B|Bowie hesitates.|`{W1}`|
|3058 /0BF2|S /P|Peter asks for a decision.|`{W1}`|
|3059 /0BF3|S /F|The follower calls for resistance.|`{W1}`|
|3060 /0BF4|S /129|Odd Eye announces the battlefield change.|`{N}{W1}`|
|3061 /0BF5|S /129|Odd Eye threatens Bowie after the effect.|`{LEADER}{N}{W1}`|
|3062 /0BF6|S /P|Peter questions fighting his former companion.|`{LEADER}{N}{W1}`|
|3063 /0BF7|S /F|The follower rebukes Peter.|`{W1}`|
|3064 /0BF8|S /F|The follower urges Bowie onward.|`{LEADER}{W1}`|

`entity/getentityportaitandspeechsfx.asm` resolves the actual aliased
mapsprite into `data/spritedialogproperties.asm`. ODD_EYE has portrait
ODD_EYE and DIALOG_BLEEP_6; Peter's live class sprite and Lemon's
live appearance select their corresponding rows. F's identity remains
conditional on the follower alias; calling it Astral in the authored
role does not force that mapsprite. Source text identity is not proof
of a naturally displayed portrait, voice or completed text wait.

## Exact12/20/4/1 spriteset

**Confirmed:** `data/battles/spritesets/entries.asm` slot40 selects
BattleSpriteset40 at `$1B6538..$1B66EE`,438 bytes. spriteset40.asm is
header4 +12×12 formations +20×12 enemies +4×12 regions +point2.
Relative offsets are allies4,enemies148,regions388,point436,end438.
All rows have filler0 and STARTING0; ally rows use HEALER1/NOTHING,
NONE15/NONE15. Formations select positions, not fixed ally identities.

| Ordinal | X,Y | Ordinal | X,Y | Ordinal | X,Y |
| --- | --- | --- | --- | --- | --- |
|0|15,35|4|15,37|8|14,36|
|1|16,35|5|16,37|9|17,36|
|2|15,36|6|15,38|10|14,37|
|3|16,36|7|16,38|11|17,37|

Enemy table cells give primary order/trigger then secondary order/
trigger. N=NONE FF; M0=MOVE_TO|0=40 hex; E#=FOLLOW_ENEMY|#=80|#;
T0=FOLLOW_TARGET|0=00. AI commandsets are LEADER14,ATTACKER1/2/3=6/7/9,
SENTRY8,HEALER3/5=2/4. No selected row is initially PRIMARY_ACTIVE1,
SECONDARY_ACTIVE2, AI_CONTROLLED4, NEUTRAL8, HIDDEN200 or RESPAWN100.

| Ordinal / logical | Kind | X,Y | AI | Primary / secondary | Placement |
| --- | --- | --- | --- | --- | --- |
|0 /128|ODD_EYE|16,5|LEADER|M0/2;N/15|NOTHING|
|1 /129|DEMON_MASTER|15,5|ATTACKER3|E0/2;N/15|NOTHING|
|2 /130|EVIL_BISHOP|16,3|HEALER5|E0/2;N/15|NOTHING|
|3 /131|DARK_GUNNER|15,8|ATTACKER3|E5/1;N/1|NOTHING|
|4 /132|HYDRA|13,9|ATTACKER3|E5/1;N/1|NOTHING|
|5 /133|CHAOS_WARRIOR|16,9|SENTRY|N/1;N/15|NOTHING|
|6 /134|DEMON|18,3|SENTRY|N/2;N/15|NOTHING|
|7 /135|WHITE_DRAGON|12,4|SENTRY|N/2;N/15|NOTHING|
|8 /136|DEMON|5,27|ATTACKER2|N/0;N/15|NOTHING|
|9 /137|DARK_GUNNER|8,14|ATTACKER3|E10/3;N/3|NOTHING|
|10 /138|CHAOS_WIZARD|10,12|ATTACKER3|E11/3;N/3|NOTHING|
|11 /139|CHAOS_WARRIOR|12,13|ATTACKER1|N/3;N/15|NOTHING|
|12 /140|CYCLOPS|12,22|ATTACKER1|N/0;N/15|NOTHING|
|13 /141|CHAOS_WIZARD|9,22|ATTACKER3|E15/0;N/0|NOTHING|
|14 /142|EVIL_BISHOP|5,23|HEALER3|E15/0;N/0|FAIRY_TEAR\|USABLE_BY_AI\|UNUSED_ITEM_DROP|
|15 /143|CYCLOPS|24,17|ATTACKER1|T0/0;N/15|NOTHING|
|16 /144|CHAOS_WARRIOR|18,21|ATTACKER1|T0/0;N/15|NOTHING|
|17 /145|CYCLOPS|22,22|ATTACKER1|T0/0;N/15|NOTHING|
|18 /146|HYDRA|15,17|SENTRY|N/15;N/15|NOTHING|
|19 /147|WHITE_DRAGON|21,28|ATTACKER1|N/15;N/15|NOTHING|

| Region ordinal | Complete12-byte declaration: type,byte1,ordered vertices,last bytes |
| --- | --- |
|0|4,0;(0,24),(0,32),(31,32),(31,24);0,0|
|1|4,0;(0,0),(0,23),(31,23),(31,0);0,0|
|2|4,0;(0,0),(0,16),(31,16),(31,0);0,0|
|3|4,0;(0,0),(0,22),(17,22),(27,0);0,0|

Point0 is16,5. Coordinates fit32×44; that does not prove traversability
or exact region-edge inclusion. `regionactivatedcutscenes.asm` contains
the two necessary later interfaces: battle40/region0/F387→rbcs_battle40_1,
then battle40/region1/F388→rbcs_battle40_2. Their bodies, trigger
evaluation, timing, first activated enemy and turn queue remain outside.
At the fresh endpoint flags90..105 are cleared, while incoming387/388
and the separate checked/new-region storage are not thereby cleared.

## Fresh stats, holdings and reconstruction

`battleloop/initializecombatants.asm:InitializeEnemyStats` copies each
selected56-byte definition in14 longwords, sets currentHP/MP to maxima,
sets X/Y, movement/AI byte, kind, both triggers/orders, placement item,
activation word, difficulty ATT and current-stat refresh, in that order.
UpgradeRandomBattleEnemies returns40's original kind:40 is absent from
the random-battle list; completion540 does not admit it to that list.
Battle32's Jaro exception is inapplicable. STARTING does not run the
hidden-spawn occupancy helper.

All ten selected kinds have ENHANCED99 in combatant class byte and
initialStatus NONE. Four item/spell slots exist; unlisted slots are
NOTHING. These are selected dependencies, not all-game definitions.

| Kind / ID / mapsprite | Level,HP,MP,baseATT,DEF,rawAGI,MOV | Mover | Copied equipment / spells |
| --- | --- | --- | --- |
|ODD_EYE96 /160|80,200,160,64,54,195,6|MAGE|Counter Sword /ODDEYE1|
|DEMON_MASTER75 /139|42,73,68,48,50,65,6|MAGE|Mystery Staff /FREEZE4|
|EVIL_BISHOP84 /148|38,64,51,54,48,46,5|HEALER|Great Rod /AURA2,BLAST3|
|DARK_GUNNER65 /129|38,60,0,48,56,45,4|BRASS_GUNNER|Buster Shot /none|
|HYDRA33 /97|39,70,0,96,62,50,4|STEALTH|none /none|
|CHAOS_WARRIOR34 /98|40,76,23,92,50,54,5|REGULAR|none /ATTACK1|
|DEMON52 /116|40,75,38,90,50,55,6|FLYING|none /BOLT2|
|WHITE_DRAGON51 /115|38,66,0,85,50,53,6|FLYING|none /SNOW1|
|CHAOS_WIZARD74 /138|36,53,49,47,48,59,5|MAGE|Great Rod /BLAZE4|
|CYCLOPS31 /95|36,64,0,88,51,46,5|REGULAR|none /none|

| Kind | Resistance | Base prowess: critical,double,counter | Copied AI→initial activation |
| --- | --- | --- | --- |
|Odd Eye|WIND/LIGHTNING/ICE/FIRE_MINOR,STATUS_IMMUNITY|SLEEP,1IN8,1IN4|1000→1000|
|Demon Master|LIGHTNING_MINOR,STATUS_IMMUNITY|150_1IN32,1IN32,1IN16|2000→2000|
|Evil Bishop|0|150_1IN32,1IN32,1IN32|3000→3000|
|Dark Gunner|LIGHTNING_WEAKNESS,STATUS_MAJOR|150_1IN32,1IN16,1IN32|2010→2000|
|Hydra|ICE_WEAKNESS|150_1IN32,1IN8,1IN8|2020→2000|
|Chaos Warrior|STATUS_MAJOR|STUN,1IN16,1IN16|2000→2000|
|Demon|LIGHTNING_WEAKNESS,FIRE_MINOR,STATUS_MAJOR|MAGIC_DRAIN,1IN32,1IN16|2010→2000|
|White Dragon|ICE_MAJOR,FIRE_WEAKNESS|150_1IN8,1IN32,1IN16|2010→2000|
|Chaos Wizard|0|150_1IN32,1IN32,1IN16|2010→2000|
|Cyclops|0|150_1IN4,1IN16,1IN16|2000→2000|

AI values above are hex words. The initializer preserves only the
upper priority nibble and combines the rotated/masked placement word;
STARTING/filler0 supplies0. It strips definition BIT4/5. Thus128 has
1000,130/142 have3000, the other17 have2000. Priority1000 is not
PRIMARY_ACTIVE bit0. unknownByte is0 for Odd Eye/Evil Bishop, otherwise
the kind ID. Current status stays0. InitializeCurrentStats copies
rawAGI195=C3 for Odd Eye; its high bit survives the zero status/equipment
AGI path, with numerical low7 bits67. This is an initialized byte,
not a claim about queue entries or double-action timing.

### Fairy Tear14 and actual copied equipment

EQUIPPED0080,USABLE_BY_AI2000,UNUSED_ITEM_DROP4000,BROKEN8000,
index007F and AddItem mask807F come from sf2enums.asm.
InitializeEnemyItems skips only full-word NOTHING007F; otherwise
j_AddItem searches four slots for index127, masks d1 with807F,
writes the first empty slot and returns d2=0. The caller tests the
**returned d1** equipment bit, not the original placement word.

For ordinal14/logical142,6007 becomes0007: its holdings are
`00DF,0007,007F,007F`, equipped Great Rod95 plus unequipped Fairy Tear7.
No AI2000/drop4000 bit survives and no equipment attempt occurs.
Ordinal2/logical130 has `00DF,007F,007F,007F`, without a tear.
Other first slots:128 Counter Sword68→00C4;129 Mystery Staff100→00E4;
131/137 Buster Shot52→00B4;138/141 Great Rod95→00DF; remaining kinds
have four007F. NOTHING placements retain those copied items.

Fairy Tear is range0–1,price300,CONSUMABLE,use G_TEAR2,no equip classes
or equip effects. Its held word proves neither AI use nor a reward.
`data/battles/global/enemyitemdrops.asm` independently declares
battle40/enemyEntity14/FAIRY_TEAR/droppedFlag17. That is a later
conditional drop interface; this slice does not execute or prove it.
A supplied full inventory makes AddItem return failure before masking;
it cannot add a fifth slot. BROKEN survives807F. Neither changed state
describes the fresh copied Evil Bishop inventory.

Copied EQUIPPED words are already present before placement addition.
UpdateCombatantStats scans them without rerunning equip-class eligibility.
Great Rod adds ATT28, Buster Shot37, Counter Sword39 plus counter+1,
Mystery Staff39 plus MP_RECOVERY2. Counter increase caps at encoding3,
so Odd Eye's existing1IN4 stays1IN4. MP_RECOVERY is a no-op entry in
the stat-refresh function table; it does not increase initialized MP
past160/68 or establish a future recovery tick. Their separate item
definitions own range/classes/type/use: Great Rod95 WIZ/SORC/VICR,
1–1/7900; Buster Shot52 SNIP/BRGN/BWNT,2–3/6800; Counter Sword68
HERO/BRN/BDBT/RDBN,1–1/13000; Mystery Staff100 WIZ/SORC/VICR,
1–1/10000. All four are WEAPON|RARE, use NOTHING. No class99 equip
attempt or curse application is needed for these copied holdings.

Difficulty's duplicated conditional means all difficulties apply
floor(baseATT×5/4) once; SUPER repeats the truncating multiplication.
Equipped ATT is added afterward. No selected item/status changes DEF,
MOV,resistance or AGI from the displayed values.

| Kind | Current ATT ordinary / SUPER | Current rawAGI |
| --- | --- | --- |
|Odd Eye|119 /139|195|
|Demon Master|99 /114|65|
|Evil Bishop|95 /111|46|
|Dark Gunner|97 /112|45|
|Hydra|120 /150|50|
|Chaos Warrior|115 /143|54|
|Demon|112 /140|55|
|White Dragon|106 /132|53|
|Chaos Wizard|86 /100|59|
|Cyclops|110 /137|46|

### Allies, absent slots, battle entities and endpoint

HealLivingAndImmortalAllies visits all30 allies: living allies plus
Peter7/Lemon28 regain maximumHP/MP regardless of membership. Other
HP0 allies skip refresh. STUN/POISON/CURSE are retained for refresh,
which rebuilds curse from equipment; STUN's MOV1/AGI5 decreases saturate.
No resetForceBattleStats command is present, so kills/defeats/EXP,
items and membership are not reset by the scene. PLAYER_TYPE and
seconds have their distinct prefix writes; no general statistics
reset should be inferred from fresh placement.

InitializeAllAlliesBattlePositions resets all30 X bytes−1, rebuilds
force lists, then considers at most12 active entries in ID order.
Dead members consume an outer iteration without consuming a formation
row; excess active entries LeaveBattleParty without a second rebuild.
The existing empty-membership N−1 underflow boundary remains. Main
scene Bowie and the twelve row ordinals do not fix which allies occupy
them. Y and unrelated state are not cleared by an X-only reset.

All32 enemy slots128..159 are considered.148..159 fail the20-enemy
count check before advancing a0: activation0000,max/currentHP0,X−1,
triggers2/15 read from the first Odd Eye row. Y,kind,class,items,spells,
orders and movement/AI can remain stale; changing only HP/X does not
install an absent definition. Selected128..147 all receive full copies.

LoadBattle at `$25610..$256A2` calls PositionBattleEntities, which
clears scene population/aliases and packs allies then enemies using
signed X/Y and enemy neutral bit, **not HP**. With A eligible allies,
20 unchanged enemies occupy physical A..A+19; A≤12 gives maximum31,
below49. No selected mapsprite uses the special240 path; no40 neutral
override is declared.148..159 aliases becomeFF through X−1;30/31
remain fresh0. Scene follower positions and the130 hide do not carry
into this new combat population.

Fresh caller d0 is explicitly0, so LoadBattle uses combatant0's
actual alias for chosen coordinates (negative alias falls back to
physical0), then requests sprites/map, VInt, mapsprites, base services,
BattleTerrain40, map music and fade. Its selected global dependencies
are `terrainentries.asm` slot40, `custombackgrounds.asm` VERSUS_ODD_EYE,
and `enemyleaderpresence.asm` byte−1. Working crop0,0,32×44 remains;
scene16,4 and combat16,5 have distinct roles even without a crop offset.
Terrain/materials were not decoded. Returning through the empty start
reaches @Start **before ActivateEnemies**. No region trigger, spawn,
queue generation, first control or Odd Eye action is claimed here.

## Actual compiler and production consumers

**Confirmed about the tool:** sole `observe.py` lowering returned
error=None in0.1216s. Full retained programs are bbcs_40:19 instructions
and cs_4E544:301; actions Init14,Jump9,AnimSpeedx2 3,DeactivateAutoFacing3.
There are45 program natives:40 setBlocks,2 tintMap,2 flickerOnce and
1 cameraSpeed. eas_AnimSpeedx2 has one native ac_animSpeedX2;
the other actions have none. All results, locations and error fields
remain in `local/issue1158/observe-01.json`, without retry or execution.

The selected lowering emits bare position/face instructions, with no
branch on the native ally-HP predicate. ProgramRunner's position/facing
cases and alias resolution do not supply that HP guard. Thus a bound
Peter/Lemon/Zynk at HP0 can differ from the original setPos skip;
typed shape alone does not preserve this changed-state behavior.

After the typed population/position prefix and either F76 branch,
cs_4E544 instruction0 is native setBlocks. If earlier map/material/
alias/services admit, ProgramRunner refuses it before music/dialogue.
Acknowledging a native does not supply its operands, effect or lifecycle.
The later action-native would independently refuse if serviced before
its replacement; absence of a compiler exception is not runnable support.

| Actual owner | Present support / selected40 boundary |
| --- | --- |
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py), [preparation](../../src/sf2tool/remake_exploration_battles.py)|Typed scene load/population, branch, scripts/moves/waits, text, sound, flash, hide and nod can be represented. This observation registers the selected source explicitly; it is not automatic world binding. Encounter preparation admits2/3, not40. Native layout/tint/flicker/camera-speed/action-speed remain.|
|[map compiler](../../src/sf2tool/remake_exploration_maps.py)|Route-absent non-event Map54 setup is finally overwritten to a single end; held387/388 encounter native flag-layout copies, both clear reaches end. eventMaps54 still needs an unsupplied setup binding. The accepted ms_Void exploration fallback supplies neither this scene nor Battle40. Final assignments, not the earlier setup frontier alone, determine output.|
|[PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Pinned encounter and region-program admission are1/2/3. Further selected refusals include Fairy Tear placement-item-refresh; HEALER3/5 and SENTRY source-commandset; ENHANCED enemy-derived-state; BIT4 source-ai-word; MAGE and BRASS_GUNNER source-mover. REGULAR/HEALER/STEALTH/FLYING Mover cases exist. These are separate guards, not a prediction of one full-import failure order.|
|[BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [BattleTerrainRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTerrainRules.cs)|Priority/mode initialization and validated formation/region/point shapes exist. Controlled roster/already-refreshed/difficulty0 policies do not implement the native all30 refresh or arbitrary suspended state. Selected copied equipment/tear refuse enemy-equipment-refresh. No40 terrain or complete loadout is bound.|
|[MapEntityAllocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Follower-first allocation, duplicate-ally reuse and alias initialization have consumers, with49-slot validation. Native-call becomes UnsupportedInstruction/UnsupportedEntityAction. Population shape does not establish live class/HP/alias flags, original five-script capacity or fulfilled mapsprite services.|
|[BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Before/load/start continuation and typed actions exist; F88 refuses suspended-battle-entry. Native program/action raises program-opcode/entity-action before advancing that instruction. Relative destinations outside0..63 refuse entity-motion-boundary without clamping. Whole initialization validates before publishing, unlike interrupted native prefixes. No running40 scene was observed.|
|[WorkingMapLayout](../../remake/src/Sf2.Remake.Domain/Maps/WorkingMapLayout.cs), [ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs), [capability owner](../../remake/docs/capability-status.md)|Block-copy and display/audio/camera/entity primitives exist, but these setBlocks commands are still native and unlinked. Generic projection does not bind40 assets, complete original services or prove natural presentation, usable input or persistence.|

**Inferred:** a future content contract can keep temporary scene layout,
scene alias population, fresh combat data and later region changes
separate while reusing typed primitives. This research supplies the
accepted-source relationships; it does not admit or implement Battle40.

## Reproduction, preserved evidence and open boundary

The compact ignored direct recipe checks the pinned complete scene,
inline moves/table/branch/order, all29 token sequences, all12/20/4/1
records and selected definitions/items, actual HP/service/caller/alias
uses, current consumers and complete one-shot lowering result. It also
checks the navigation-only predecessor, date, UTF8/LF, links, exact
owned diff and private/text publication boundary. It never compiles
the opening again, imports a world or writes a maintained checker/test.
Use fresh receipt names and same-process private configuration:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1158/review.py <configured-source-disasm-root> --receipt local/issue1158/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

Applicable ADR0004 provenance/date/width/branch/selector/order/alias/
consumer/adversarial checks apply. New parser/schema/fixture/helper
tests and H3 callback checks are N/A. Normal verify is **SKIPPED** by
the explicit documentation-only Issue scope, not PASS. Exact committed
planner, actual scoped CI, resources, failures and clean frozen Git
identities belong to the handoff; main-gate independently accepts.

Preserve #1156's direct/main-gate/CI passes that missed its final
Map54 producer overwrite, its focused correction/main-gate PASS1964
and CI39719021665; #1154's autoFacing/HP corrections and prior missed
passes; #1152's PRIMARY bit0/Mover corrections; prior no-clamp and
aftermath-native results; Map53 UnboundLocalError/zero partial;
twelve completed old comparison failures and F88/natural-service limits.
No prior run becomes interrupted, repaired or rerun because main moved.

**Unknown:** natural incoming flags/membership/HP/aliases, arrival and
terrain, actual concurrent action progress and first speed-script service,
VInt/fade/view/music/text/input/DMA completion, interleaved or interrupted
prefixes, source/ROM equality beyond accepted evidence, final displayed
positions, persistence and usable control. These are limits, not automatic
acquisition/repair work. Stop before first region activation; leave a
clean pushed Draft PR for independent review, without merge/cleanup or
starting another Issue.
