# Battle26 admission and Geshp confrontation

Evidence date: 2026-10-14. **Confirmed:** a later Map72 request can select
Battle26, whose before hook stages the Geshp/Higins confrontation. Admission,
scene actors, dialogue and fresh battle initialization are separate operations.
**Inferred:** this is a candidate continuation after Ketto. **Unknown:** natural
reach, supplied state, completed services, presentation, control and persistence.

## Agreement and provenance

Standard rigor for [Issue #1001](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1001)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
ordered admission, continuation state, scene population and narrative effects
need separate evidence. Accepted starting base is
`780d480ced1d7a652325aebf7166853037c915e5`. This note is the sole owned path;
no shared index, registry, contract, fixture, schema, tooling, design, remake,
glossary or Chinese write. Concurrent #1002 and UI692 are not dependencies.

Consume accepted [Ketto interactions](./ketto-interactions-and-merchant-relocation.md),
[Ketto entry](./battle25-outcomes-through-ketto-entry.md),
[battle loop](../../battle/battle-loop.md), [cutscenes](../../battle/battle-cutscenes.md),
[request writers](../../world/map-event-request-state.md),
[request consumers](../../world/map-event-request-consumption.md),
[scene population](../southern-travel-creed/evil-spirit-shrine-entry-and-scene-admission.md),
[map scripts](../../world/common-scripting.md),
[battle routing](../../../design/contracts/battle/battle-cutscene-routing.md) and
[dialogue](../../../design/contracts/interface/dialogue-system.md).
The endpoint is return from ExecuteBeforeBattleCutscene, before fresh region
reset, healing, formation, LoadBattle and start. Initial combatant/neutral
roster, turns, outcomes and Higins recruitment remain later scope. No runtime,
acquisition, regeneration, new machinery or general effects-engine audit.

Source paths are relative to `disasm/` at
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained read-only checkout is clean at that pin. Existing fixtures and
payloads identify USA ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Constants/RAM belong to `sf2enums.asm`/`sf2const.asm`. Fnnn means flag nnn;
indices are zero-based and extents half-open. Short map/entity scripting
paths below are under `code/common/scripting/`.

Minimum evidence: named source, existing fixtures and the retained
18,285,979-byte map-init/map-script pair. Linear comparison budget is under
one minute/512 MiB, with small prose/receipts. The unchanged-pair and cutscene
comparison took 0.082s. Replan a material contradiction or supporting-tool
expansion; an Unknown does not authorize collection.

## Conditional request and ordered admission

**Confirmed:** Ketto's sole `data/maps/entries/map25/6-warp-events.asm` row
matches X255/Y31 and stores no-scroll Map72, destination(31,27), Down3.
No shopping, F800, victory525 or F426 predicate is in that row. Ordinary
walking production still requires the warp layout kind$1000 and a qualifying
position; a row is not a naturally reachable exit. WarpIfSetAtPoint writes
mutable event type1/PARAM1–5 and WARP_SFX. ProcessMapEvent clears the type
before dispatch, consumes/clears sound, idles physical0 and unwinds exploration.
The no-scroll consumer samples position, substitutes nonnegative destination
low bytes, applies facing and returns d4=0. Nominal(31,27) assumes ordinary
sampled high bytes; replacement/interruption can change the request.

`code/gameflow/mainloop.asm:MainLoop/@Start` calls SwitchMap then CheckBattle;
it sends d7 to BattleLoop as d1 if d7 is not -1. Map72 has no flag-switched
replacement in the accepted switch table. The complete candidates in
`data/battles/global/battlemapcoords.asm` are:

| Battle / row | Unlock / completion | Map; output X,Y,width,height | Trigger X,Y |
| --- | --- | --- | --- |
|8|408 /508|72;1,32,32,30|255,255|
|24|424 /524|72;1,32,32,30|255,255|
|26|426 /526|72;28,0,36,32|255,255|

`code/common/maps/getbattle.asm:CheckBattle` resolves CURRENT255 through
CURRENT_MAP, scans in row order, requires unlock and matching trigger bytes,
then writes BATTLE_AREA_X/Y/WIDTH/HEIGHT. These rectangles are output crops,
not containment gates. A matching held completion clears the selected row's
unlock and **still returns that battle**, without trying the next candidate.
Thus 426 held does not win over earlier408/424; 526 held does not suppress a
currently unlocked26. With no admitted row, MainLoop enters exploration.

Later `map72/mapsetups/s3_zoneevents.asm` rows0–2, X32/33/34 with any Y,
call CheckRandomBattle(26). Ordinary walking zone production requires layout
kind$1400 and writes event6/coordinate words. First matching row wins; a
declined specific callback does not fall through to default. Rows3–8 are
the separate cave callback; default chooses helper8 or24 from F523. Neither
row coordinates nor prior Ketto completion establishes actual traversal.

`map/mapsetupsfunctions_1.asm:CheckRandomBattle` checks526: clear requests
immediately, without a counter test or RNG; set with nonzero STEP_COUNTER
declines; set with zero counter draws range8. Zero requests; nonzero draws
range4+2 and stores2–5 without requesting. Request sets426, writes longword
$000100FF at MAP_EVENT_TYPE (type1, PARAM1=0, PARAM2=CURRENT255), stores
STEP_COUNTER30000, then waits for scroll and requests boost sound/flash.
PARAM3–5 remain supplied. Writes precede service completion. The next scan
can still select8/24 first. F800 and525 neither replace526 nor set426 here.

## Before dispatcher and exact endpoint

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop` clears
PLAYER_TYPE and sets399 before testing88. Fresh88-clear clears seconds,
saves incoming map/battle words, stores CURRENT_MAP72/CURRENT_BATTLE26,
sets base VInts and calls j_ExecuteBeforeBattleCutscene.

| Table under `data/battles/cutscenes/` | Slot26 / byte52 | Effective target |
| --- | --- | --- |
|beforebattlecutscenes.asm, base$47A88|word at$47ABC|bbcs_26 at$4B958|
|battlestartcutscenes.asm, base$47B2C|word at$47B60|ms_Empty at$47A4E..$47A50|

Both are signed relative displacements from their table bases. In
`code/gameflow/battle/cutscenes/beforebattlecutscenesstart.asm`, the wrapper
checks450+26=476 and dispatches only when clear. It does **not** set476.
The end wrapper restores d1 and returns at loc_47AE8. Start's distinct wrapper
sets476 before dispatching its empty script when clear. A held476 skips
before and start scripts, but not the subsequent fresh initialization.

This note stops at before-wrapper return to the instruction following its
call. Only afterward does BattleLoop restore/reassign saved map/battle,
clear region flags90–105, heal living/immortal allies, initialize ally then
enemy battle positions, clear AI, LoadBattle for combatant0 and invoke start.
None of those later steps, a neutral roster, activated enemies or a turn is
established by the scene. Its allocations are not Battle26 combatant records.

With88 set, BattleLoop instead restores saved seconds, clears88/AI, sets view
target0, loads supplied current battle state and jumps to the individual-turn
loop. Incoming72/26 are not assigned by this branch. It bypasses before/start,
fresh healing/formation and region reset; the following jump to @Start is
unreachable. This is not another route through the before-return endpoint.

## Scene loading and logical actors

**Confirmed:** `data/battles/entries/battle26/cs_beforebattle.asm` has
bbcs_26 at$4B958..$4BC5A, followed by ce_4BC5A at$4BC5A..$4BCAA.
Retained `programCorpus.programs[entryLabel=bbcs_26]` covers source lines4–186,
130 top-level commands including termination. Their encoded widths total608
bytes; inline actions/moves contribute162, making770 script bytes. The list
is80 bytes: six-byte main header, nine eight-byte declarations, two-byte$FFFF.
This is not a count of generated action bytes or a combat roster.

`map/mapscriptengine_1.asm:csc37_loadMapAndFadeIn` sets OUT_TO_BLACK fading
state and falls into csc48: view target-1, LoadMapTilesets, wait for fading,
VInt deactivation, LoadMap(72, packed camera2/20 scaled by3), display/VInt
reactivation and one VInt wait. `code/common/maps/mapload.asm:LoadMap` selects
map data, graphics/layout/area and view fields. This is not ExplorationLoop's
population, temporary-flag reset, healing or map-setup init. It does not turn
the camera coordinates into Bowie coordinates or change CURRENT_BATTLE26.
The separately reached fadeInB invokes FadeInFromBlack after scene setup.

csc42_loadMapEntities disables display/VInts, reads main X/Y/facing then
calls InitializeMapEntities and LoadEntityMapsprites, and re-enables them.
`map/mapfunctions.asm` clears actors and the64-byte lookup, declares eligible
followers first from physical1, then the list, then main/Bowie at physical0.
All list actions are eas_Init; fixed ally rows are deduplicated against an
existing nonzero ally lookup. No membership test is required to declare Higins.

Let N be the actual preceding follower count and P=1 when Peter's lookup
was absent after followers, otherwise0. These are conditional allocation
formulas, not a promise of valid supplied follower state:

| Declaration | X,Y / facing | Logical lookup / physical allocation |
| --- | --- | --- |
|main header|7,25 /Right0|Bowie0 /physical0, eas_Idle initially|
|Peter7|6,26 /Right|7 /N+1 if absent; otherwise retained follower, row skipped|
|Higins19|18,14 /Up1|19 /N+P+1; no Higins row in follower table|
|GESHP159|18,11 /Down3|128, offset32 /N+P+2|
|LESSER_DEMON109|17,11 /Down|129, offset33 /N+P+3|
|DARK_BISHOP145|19,11 /Down|130, offset34 /N+P+4|
|LIZARDMAN83|20,10 /Down|131, offset35 /N+P+5|
|EVIL_CLOUD106|63,63 /Down|132, offset36 /N+P+6|
|CENTAUR_MALE233|17,15 /Up|133, offset37 /N+P+7|
|CENTAUR_MALE233|19,15 /Up|134, offset38 /N+P+8|

All seven NPC sprite IDs are30..239, so they take ordinary sequential
allocation, not special physical47. InitializeMapEntities scales coordinates
by384; DeclareNewEntity writes position/destination/facing/sprite/action,
ENTNUM/layer and clears travel/velocity. Ordinary ENTNUM is physical slot,
not logical128..134.
ClearEntities uses$7000 positions as inactive slots; declared63*384=$5E80
is below that sentinel. Neither cloud staging nor later die-to63,63 means
an absent actor. The follower list has its separate byte$FF terminator.

On overworld72, F65 can prepend Caravan/FOLLOWER_A30. The complete selected
table then tests66 Sarah,67 Peter,72 Kazin,73 Slade,74 B/Astral,66 Chester,
68 B/Oddler,75 Luke,69 C/Fairy,70 B/Astral,74 A/King,71 Lemon,74 D/Minister,
76 Zynk,77 A/Old Man,82 Frayja,83 A/King,83 D/Minister and84 Rohde, in order.
Duplicate A/B rows allocate separately and overwrite their logical lookup;
they are not deduplicated. There are only five action pointers in each
follower pointer table and no count guard in InitializeFollowerEntities:
excess simultaneously supplied flags are not a supported capacity guarantee.
With coherent flags65/67/70 only, N=3,P=0: A1, Peter2, B3, Higins4,
NPC128..134 at5..11. That example is not proof these flags were supplied.

Absent A/B lookups remain zero, so commands can alias physical0. Script
lookup masks the logical byte, subtracts96 for high-bit IDs, reads a physical
byte and scales by32; it validates neither missing nor$FF/corrupt mappings.
The script rebuild normally removes earlier Ketto mappings, but retained
followers, duplicate aliases and later mutation still matter. F476-held
skips this entire rebuilding operation; F88 uses its different load path.
With F64 and RAFT_MAP72, sub_44404 can additionally declare raft physical31
using saved coordinates masked$7F. Fresh PLAYER_TYPE0 excludes its mode2
override, but does not clear F64 or the saved raft location.

GetAllyMapsprite uses actual class. Its dead-blue-flame branch requires
CURRENT_BATTLE255; fresh scene26 bypasses it even for zero HP. This differs
from exploration and does not revive anyone. csc19/setPos, csc23/setFacing
and csc1C/stopEntity skip dead actual allies0..29 through
AdjustScriptPointerByCharacterAliveStatus. Followers30/31 and NPCs bypass
that guard. Action installation, shiver, lookup and text do not gain that HP
guard. Dead Peter can therefore receive eas_Init but skip setPos; dead Higins
can skip facing/stop while still receiving shiver and the later inline action.

## Complete ordered program

**Confirmed:** the following ranges exhaust command indices0–129. Tn denotes
the nominal dialogue ID n in the next section; all other names retain their
source macro meaning. Wait suffixes mean interpreter waits as explained below,
not an observed duration or successful presentation.

| Commands | Exact ordered operations, including inline bodies |
| --- | --- |
|0–10|textCursor2596; loadMapFadeIn72,2,20; loadMapEntities ce_4BC5A; setActscriptWait Bowie/eas_Init; Peter/eas_Init; setPos Peter6,26,Right; setActscriptWait B/eas_Init; setPos B6,24,Right; A6,25,Right; fadeInB; cameraSpeed$30.|
|11–17|T2596; setCamDest13,9; T2597–2601.|
|18–26|entityActionsWait133: Up2,Right1,end; facing133 Up; T2602–2604; facing128 Up; T2605; setActscriptWait133/eas_Jump; T2606.|
|27–34|customActscriptWait133: speed48/48,jump Idle,end; entityActions133: Up3,end; csWait5; FX128 MOSAIC_OUT6; waitIdle133; csWait5; setActscript133/eas_2xRightLeft; csWait120.|
|35–47|facing131 Left,133 Down; csWait20; customWait131 speed64/64,jump Idle,end; entityActions131 Left1,end; csWait3; customWait133 speed64/64,jump Idle,end; quake2; wait133 JumpLeft; quake0; wait133 JumpLeft; T2607; wait133 Die.|
|48–56|camera13,13; setPos12818,19,Up; FX128 MOSAIC_IN7; T2608; wait134 Jump; facing Higins Down,134 Down; csWait30; entityActionsWait134 Left1,Down1,end.|
|57–67|T2609–2614; setPos12913,18,Right; customWait129 speed80/80,jump Idle,end; customWait134 speed40/40,jump Idle,end; entityActions134 Down2,end; entityActions129 Right4,end.|
|68–77|csWait20; customWait134 speed64/64,jump Idle,end; quake3; wait134 JumpRight; quake0; wait134 JumpRight; T2615; wait134 Die; facing129 Down; entityActionsWait128 Up2,end.|
|78–91|T2616–2624; FX132 SLIDE_OUT4; flashWhite128,30; camera13,11; setPos13218,12,Down; FX132 SLIDE_IN5.|
|92–101|T2625; entityActionsWait132 Down1,end; FX132 SLIDE_OUT4; T2626; shiver Higins; stopEntity Higins; T2627–2628; stopEntity Higins; customWait Higins: motion OFF,orientLeft,updateSprite,jump Idle,end.|
|102–111|T2629; customWait129 speed32/32,jump Idle,end; setActscript129 Jump; wait128 Jump; camera13,15; entityActionsWait128 Down3,end; csWait5; setActscript128 eas_2xRightLeft; csWait120; entityActionsWait128 Left1,end.|
|112–122|T2630; camera2,20; csWait60; camera13,15; T2631–2634; wait128 DeactivateAutoFacing; customWait128 speed48/48,jump Idle,end; entityActionsWait128 Right2,end.|
|123–129|T2635; facing128 Up; T2636; facing128 Down; setActscript129 Jump; FX128 MOSAIC_OUT6; csc_end.|

There are nine inline custom programs: eight speed programs of12 bytes each
(speed word/paired bytes4, absolute jump6, end marker2) and Higins's18-byte
program (motion4, orientation4, update2, jump6, marker2). Eleven packed move
sequences contain thirteen two-byte moves and eleven two-byte end markers,
48 source bytes. Their headers are already in the608 top-level bytes.
csc2D generates thirteen8-byte `[4,dx,dy,1]` move/destination-wait sequences
plus eleven6-byte jump-to-Idle tails:170 generated bytes, distinct from48.
All inline source markers are$8080; csc_end is$FFFF, not an action marker.

## Motion, effects and waiting consumers

**Confirmed:** ExecuteMapScript initializes the generated-action cursor at
FF9004_LOADING_SPACE. csc14/csc15 install inline/referenced pointers and seed
ACTSCRIPTWAITTIMER with resolved physical ID. Nonzero wait byte$FF polls for
the pointer to equal eas_Idle; zero returns immediately. waitIdle does the
same pointer poll. These are not destination tests and require entity service
progress. csc14 scans through the$8080 marker after any wait.

csc2D clears FLAGS_A bits5/6 with$9F, creates each signed relative move plus
destination wait, installs the stream and optionally polls for Idle. esc04
adds signed tile deltas*384 to actual current position, uses the obstruction
helper whose bit5 gate was just cleared, and derives destination/travel/signed
velocity from stored speed. esc01 waits for position=destination before the
next move/tail. Changing position changes the endpoint. The waited centaur133
approach is nominal(17,15)→(17,13)→(18,13); its later Up3 is asynchronous,
with an explicit waitIdle after Geshp's mosaic. Centaur134's waited approach
is nominal(19,15)→(18,15)→(18,16); subsequent Down2 competes in time with
129's Right4 from(13,18). csWait20 is not an idle barrier before replacing
134's program. Do not force their asynchronous paths to final tiles.

`entity/eas_actions.asm` is under `data/scripting/`, not the code directory.
eas_Init clears acceleration factors, sets speeds32/32, disables acceleration/
deceleration, enables entity obstruction, disables entity/map uncollidable
settings, enables auto-facing, orients Up, clears immersed, sets shared size24,
updates sprite and jumps Idle. Script macro speeds48/64/80/40/32 write actual
paired speed bytes through esc10; they are not distances or frame counts.
The main header initially uses Idle; explicit waited Init then configures it.

eas_Jump disables auto-facing, requests relative(0,-1), waits5, requests(0,1),
waits5, requests(0,0), enables auto-facing, then jumps Idle. JumpLeft uses
(-1,-1)/(-1,1) and waits6/6; JumpRight uses(1,-1)/(1,1), waits6/6. These use
relative destination requests and timer waits, not esc01 completion between
legs: each next leg is relative to the position actually reached. Speed and
update state therefore affect displacement. A waited Jump means its program
reached Idle, not a guaranteed exact two-tile horizontal movement.

eas_2xRightLeft sets Right/Left/Right/Left, updates sprite and waits30 after
each, then Idle. It is a look-around action, not csc27/headshake. The two
csWait120 calls do not join it under arbitrary timer/service state; a later
action can replace it. DeactivateAutoFacing is just OFF then jump Idle; the
following source bytes are unreachable from that entry after the jump.

eas_Die disables animation, cycles facing Left/Up/Right/Down three times
with explicit updates and eleven waits3 (the first Down has no wait), sends
SFX_BATTLEFIELD_DEATH, changes mapsprite to EFFECT1, faces Up/Left/Down with
updates and three waits8, sets position63,63 and jumps Idle. esc0C writes
both position/destination and clears travel. It does not set ally/enemy HP,
status, dead-combatant lists, membership or battle outcome. The two soldiers'
apparent deaths are scene behavior. `VInt_UpdateEntities` skips only X≥$7000
before UpdateEntityData/action dispatch: $5E80 actors still update.

csc19 setPos writes position/destination and facing, then requests a sprite
update; it does not clear velocity/travel or replace the script. csc23 writes
facing/update only. csc1C stopEntity writes ANIMCOUNTER=-1, not a motion stop.
The Higins inline motion OFF likewise disables animation via esc14; orientLeft
changes orientation bits, not facing, then explicitly updates the sprite.
This produces no executable paralysis, possession or enemy allegiance state.

csc32 camera sets view target-1, requests tile destination and waits for view
scroll. cameraSpeed writes VIEW_SCROLLING_SPEED48. Actual quake operands2/3/0
have neither ramp bit set: csc33 directly stores QUAKE_AMPLITUDE, including
the two explicit zero clears. It does not undo an interrupted nonzero prefix.

csc22 mosaic6/7 uses16 iterations of LoadMapsprite, mask transform sub_45E10,
DMA request and three VInt waits, increasing/decreasing the mask. Slide4/5
uses23 crop/transform/DMA iterations with two VInt waits; slide-out additionally
applies the final$FFFF mask and DMA. These selected paths do not delete or
relocate actors, clear action pointers or stop VInt updates. A visually hidden
cloud or Geshp can still have live position/actions; rendered results and
interference by other sprite updates require observation.

flashWhite128,30 shifts30 right2 then DBF: eight iterations set FLAGS_B bit2,
update, wait twice, clear bit2, update, wait twice (32 explicit VInt waits).
Shiver saves shared size and animation counter, uses size21/counter-1, performs
three cycles of FLAGS_B bit3 set/update/Sleep5 then clear/update/Sleep5, and
restores saved size/counter. It has no ally-HP guard. These are effect requests
and bounded source loops, not measured total duration or completed DMA/audio.

## All dialogue and cursor state

**Confirmed:** all41 selected literals were compared with actual four-digit
hex keys in `data/scripting/text/gamescript.txt`, not accepted from comments
alone. With no skip and all calls returning, cursor2596 advances through2636
to2637. G means actor128, H=Higins19, C1=133, C2=134, B=FOLLOWER_B31.
G uses source portrait-control$00; every other speaker uses$C0. `T` means
nextText; `S` means nextSingleText. The table preserves the complete literal
content, controls and speaker order at this bounded scene.

| ID / command | Speaker / form | Literal |
| --- | --- | --- |
|2596 /11|G/S|`Answer me!{N}Where are you going?{W1}`|
|2597 /13|H/S|`Ah...I...{W1}`|
|2598 /14|G/S|`You were going to South{N}Parmecia to get help, right?{W1}`|
|2599 /15|H/S|`I...uh...{W1}`|
|2600 /16|G/T|`Paladins of Pacalon are{N}cowards!  {NAME;19}, don't{N}you have any pride?{W2}`|
|2601 /17|G/S|`Wimp!{W1}`|
|2602 /20|C1/S|`Shut up!  Mr. {NAME;19} is{N}the bravest soldier in the{N}world!{W1}`|
|2603 /21|H/S|`I can defend myself.{W1}`|
|2604 /22|G/S|`Come on, you half-breed!{W1}`|
|2605 /24|G/S|`Are you afraid of me?{N}I'll turn my back to you.{N}Now, run away you cowards!{W1}`|
|2606 /26|C1/S|`You've gone too far!{W1}`|
|2607 /46|C1/S|`Aaaauuuuu!{W1}`|
|2608 /51|G/S|`Pacalon soldiers have no{N}honor.  They attack while{N}my back is turned!{W1}`|
|2609 /57|C2/T|`What?!{W2}`|
|2610 /58|C2/S|`If Mr. {NAME;19} was not{N}ill, you'd be history!{W1}`|
|2611 /59|G/T|`Oh, is {NAME;19} ill?{W1}`|
|2612 /60|G/S|`In that case, you can try{N}and take me.{W1}`|
|2613 /61|C2/S|`I can no longer put up with{N}these insults!{W1}`|
|2614 /62|H/S|`No, no!{W1}`|
|2615 /74|C2/S|`Grruuu....{W1}`|
|2616 /78|G/S|`You look so sad,{N}{NAME;19}....{W1}`|
|2617 /79|H/S|`You're not just a normal{N}devil soldier.{W1}`|
|2618 /80|G/T|`You're right, {NAME;19}.{N}I see why you're the leader{N}of the Pacalon Army.{W2}`|
|2619 /81|G/S|`I'm Geshp!  An adviser for{N}Zeon!{W1}`|
|2620 /82|H/S|`How did you know my name?{W1}`|
|2621 /83|G/S|`I know everything.{W1}`|
|2622 /84|H/T|`Now, tell me the reason why{N}`|
|2623 /85|H/S|`you're keeping me alive.{W1}`|
|2624 /86|G/S|`Ha, ha!  You're smart!{W1}`|
|2625 /92|H/S|`What did you do?{W1}`|
|2626 /95|H/S|`I...can't...!{N}My body won't...{W1}`|
|2627 /98|G/T|`I came here to borrow your{N}body.{W2}`|
|2628 /99|G/S|`Ha, ha!  Welcome our new{N}leader!{W1}`|
|2629 /102|B/S|`Stop!!!{W1}`|
|2630 /112|G/S|`Who said that?{W1}`|
|2631 /116|G/T|`A force from the south?{N}How?{W2}`|
|2632 /117|G/S|`Where did you come from?{W1}`|
|2633 /118|B/S|`We're from Granseal!{W1}`|
|2634 /119|G/S|`But, that's on the other....{W1}`|
|2635 /123|G/S|`I'd better leave...{W2}`|
|2636 /125|G/S|`Devil soldiers!  Stop that{N}force from Granseal!{W1}`|

`text/textfunctions_1.asm:symbol_player` reads the explicit name operand19
and calls GetCombatantName; `{NAME;19}` is the actual ally19 name, not a
lookup of scene physical19 or a guaranteed unchanged English name. Plain
`symbol_name` instead consumes the current dialogue-name-index pointer.
No choice opcode or YesNoPrompt occurs here. Questions, cowardice, illness,
threats, apparent death, possession and the spoken new leader are narrative;
they do not establish executable status, HP, allegiance, join or outcome writes.

`entity/getentityportaitandspeechsfx.asm` resolves current physical mapsprite
through the logical lookup, then `data/spritedialogproperties.asm`. Nominal
Geshp has GESHP portrait/bleep6; HIGINS_PROMO has HIGINS/bleep6; CENTAUR_MALE
has no portrait/bleep6. B can be Astral/bleep6, Oddler/bleep4 or another
supplied alias/sprite. Missing property uses default portrait/bleep6.
csc1D only opens when PORTRAIT_WINDOW_INDEX=0; packed bits15/14 pass the
two portrait controls. Existing portrait state can therefore prevent a new
portrait even while speech resolves again. Class, changed sprite and alias
state matter; logical speaker alone does not prove a rendered portrait.

csc00/csc02 first test SKIP_CUTSCENE_TEXT. Normal paths prepare portrait/
speech, wait for scroll, call DisplayText, then increment the cursor. S also
closes portrait/text and Sleep10; T retains the window. Text2611 is T with
W1, text2622 is T with only a trailing newline, and text2635 is S with W2:
macro choice does not manufacture or replace literal controls.

ExecuteMapScript clears the skip latch at entry. Debug mode plus P2 Start
sets it at the command loop. Skipped S/T consumes the speaker operand but
does not display, increment the cursor, close windows or Sleep10. Negative
csWait words likewise skip their Sleep; ordinary action/effect/camera/load
commands above still execute. Midscene skip leaves the cursor at the completed
prefix, not necessarily2637. It is not a fast-forward that guarantees the same
relative movement or presentation.

## Direct effects, changed states and acceptance

**Confirmed:** the exhaustive local command list contains no set/clear story
flag, JoinForce, HP/status mutation, combatant creation, battle outcome,
inventory/payment, save, map-event request or branch/choice. Shared admission
can set/clear unlocks and BattleLoop sets399; those are separate earlier
effects. Before does not set476 or526. Scene loading and actor/camera/fade/
effect/text fields change as described, with no transaction or rollback.

csc_end conditionally waits for view scrolling if a dialogue window is open,
clears VIEW_SCROLLING_SPEED, restores interpreter registers and returns.
It does not join129's final asynchronous Jump, restore actor speeds, clear
all effects, remove scene actors, grant player control or initialize combat.
The normal quake path already wrote0; an interrupted quake prefix need not.
Geshp's final mosaic is a visual request, not deletion or a battle victory.

| Changed state or interruption | Required result/boundary |
| --- | --- |
|426 absent; earlier408/424 held|26 is not selected; earlier unlocked candidate wins.|
|526 held versus clear|Selector completion cleanup differs from helper RNG/counter gate.|
|Wrong map, replaced request, layout/mode changed|No unconditional Ketto→26 route.|
|476 held|Before scene skipped; fresh initialization remains later.|
|88 set|Supplied continuation bypasses this fresh endpoint and its actor list.|
|Changed follower flags/count/A/B aliases|Actual mapping/action-pointer capacity governs; no fixed cast assumption.|
|Peter absent/present; dead or changed-class allies|Fixed row deduplication, sprite selection and per-command HP guards differ.|
|Retained/corrupt/missing lookup|Zero can alias main; no safe absent-actor no-op.|
|Changed positions/speed/timer or service progress|Relative motion/jump endpoints and async overlap differ; no fixed-duration proof.|
|Cloud63,63 or completed eas_Die|Allocated actor below$7000 continues entity updates.|
|Text skip or existing portrait|Cursor/presentation differ; movement/effects are not all skipped.|
|Interruption after request, load, speed, quake or effect|Already-written prefix remains; no rollback/completed-service assumption.|
|Return from before|No fresh roster, neutral population, start flag, turn, outcome or Higins join established.|

Reproduce from the owning checkout after loading ignored
`local/private-inputs.ps1` in the same process. Verify clean pinned source
HEAD and use the existing [retained-pair recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init equality, `_verify_map_script_engine_fixture` and
`_cutscene_facts`; preserve pair size/mtime without regeneration.

1. Compare complete72 candidates with retained `battle-global-data-static.json`
   facts.battleMaps.rows. `battle-global-data-static-v1.json` retains only
   factSummary, not those private rows. Follow selector/helper/request/MainLoop branches and
   `battle-cutscenes-static-v1.json`/`map-event-random-battle-state-static-v1.json`
   under `tests/fixtures/h2/`. Resolve both slot26 words and476 wrapper order
   from the named source tables: `battle-routing-data-static-v1.json` has
   routeTargetsParsed=false, so its aggregate counts cannot prove either target.
2. Select retained bbcs_26 and compare every sourceLine/macro/argument/width
   against source/macros; review every inline action and all nine entity rows,
   six-byte main header and terminator. Resolve the130-command ranges,608/162
   source widths and170 generated bytes independently; counts alone do not
   prove private detail or consumer semantics.
3. Follow csc37/48/42, population/follower/lookup/class/HP consumers and
   csc14/15/16/19/23/2D plus eas/esc consumers above. Check explicit waits,
   asynchronous replacement, camera/FX/animation-only fields and$7000 update
   gate. Do not replace this review with program-count equality.
4. Read hex keys0A24..0A4C in gamescript.txt; compare all41 literals, speakers,
   S/T macros and controls with this table and source comments. Follow current
   portrait/speech/name and skip consumers, and review every changed-state row.
5. Check complete prose/diff, relative links/anchors, UTF-8/LF, sole-path and
   private boundary; run `git diff --check`. On clean committed head run
   `uv run sf2 verify plan --base origin/main --head HEAD`; record actual
   exact-head CI in the Issue handoff. Docs-only scope skips blanket local
   normal/full/engine/Godot/H3/H4 suites.

**Unknown:** natural route, actual flags/map/request/party/class/HP and actor
state, action/service completion, rendered timing/control and save persistence.
The next stage is fresh initialization, not an automatically observed battle
or Higins recruitment. These limits are not a runtime/acquisition queue.

Preserve [#997 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/997#issuecomment-6181515501)
and [PR1000 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1000#issuecomment-6181536109):
cb3d3f0b/tree42adda37/base10e1ec16, CI38659895203, unchanged-pair0.081s,
corrected actual actor0 waiting versus bit15-set skip, zero-based shop9,
opcode-case assertion failure/correction and diagnostics, plus linked
#993/#989 predecessors and their failures/Unknowns. No original evidence
is rewritten. Retain source/configuration/pair/results; freeze a clean pushed
Draft PR for independent main-gate review. No merge, cleanup, archive or next Issue.
