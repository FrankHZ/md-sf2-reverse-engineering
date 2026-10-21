# Nazca ship shootdown and crash handoff

Evidence date: 2026-10-20. **Confirmed (static):** Map59's default
999-clear initializer calls five consecutive scenes, stages the enemy
conference and Prism Flowers, switches to the flying ship, then requests
warp37,8,12/Down. Only after all five scenes return does it set999,
EGRESS_MAP45 and436/440/442/443. Consumption of that request subsequently
selects fresh Map37's999 setup; after crew initialization and the cleared256
test, it first dispatches cs_5FABC. **Stop before that scene's body.**
This is source control flow, not natural completion or playable content.

## Agreement, provenance and bounded method

Standard rigor for [Issue #1110](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1110)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
organize the complete continuous attack/crash passage for content and
engine decisions. Accepted base is
`a67a59acddbb9692dc2c1dbd3dbb204a836f0ceb`. Dependencies are accepted
[voyage and Map59 admission](./nazca-voyage-and-map59-arrival.md) and
[boarding/crew/item admission](./nazca-ship-boarding-and-departure-admission.md).
Own this document and navigation only in the voyage owner; no shared
index, registry, schema, fixture, tool or engine edit and no stacked work.

Include cs_5EB44→cs_5EBFC→cs_5ED06→cs_5EDB8→cs_5EF60, all their stage
lists, nested actions, selected helpers and actual consumers, and the
request/initializer tail through fresh Map37's first dispatch. Exclude
Map59's alternate ending, returned cs_5EB26, Battle36/40/42/43 bodies,
post-crash cs_5FABC body, ship exploration/dialogue/exit and later towns.
Changed-state selections stop at those named boundaries. No flight or
laser simulator, new acquisition, comparison platform or runtime is needed.

All original paths below are relative to `disasm/` at clean, read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
USA ROM identity remains SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/build/runtime equality is asserted. Constants resolve from
sf2enums.asm/sf2const.asm. `map/` and `entity/` abbreviate
`code/common/scripting/`; `exploration/` is `code/gameflow/exploration/`.
IDs are decimal; intervals are half-open. Texts below are authored short
summaries plus IDs/tokens, not original dialogue. Raw text/palette data
remain ignored/private.

The main source owner is `data/maps/entries/map59/mapsetups/s6_initfunction.asm`:
five scenes `$5EB44..$5F14C`, crash helpers/palette/tables through `$5F34C`.
Reuse the immutable18,285,979-byte init/script pair in `local/derived/`
and its `local/issue1005/pair.txt`0.083s provenance without regeneration.
The direct audit reads selected source/consumer records linearly, loads
that pair once, and lowers selected programs in memory. Budget one
minute/512MiB per audit, small uniquely named ignored receipts under
`local/issue1110/`; reassess material expansion. No binary layout decode,
SDK, Godot, emulator, H3/H4 or screenshots.

## Five scenes in source order

**Confirmed:** the predecessor supplies the admission conditions:
Map59 no admitted battle,543 clear,999 clear. The five bodies themselves
have no story-flag conditional branch. Every later row requires preceding
waits/calls to return. The notation below preserves order: P=Peter7,
Z=Zynk26, B=FOLLOWER_B31; I(actor)=setActscriptWait actor,eas_Init;
Tn=the exact text command in the text table; EA/EAwait are nonwaiting/
waiting entityActions, each ending endActions. A comma-separated actor
list means consecutive commands in that order, not simultaneous calls.
An inline custom speed action means ac_setSpeed(x,y), ac_jump eas_Idle,
ac_end, installed with customActscriptWait.

| Scene / passage | Complete ordered commands |
| --- | --- |
|cs_5EB44, conference staging|textCursor3699; setCameraEntity CAMERA_NEUTRAL; reloadMap8,2; loadMapEntities ce_5F19E; I(0); csWait60; MUSIC_WITCH; csWait30; setActscript128,eas_Transparent (nonwaiting); fadeInFromBlackHalf.|
|Conference|T3699..3711; shiver129 twice; T3712..3714; animEntityFX129,MOSAIC_OUT; T3715; setPos130,9,9,Down; animEntityFX130,MOSAIC_IN; EAwait130:Right4,Up1; T3716; nod130; T3717..3722; csc_end.|
|cs_5EBFC, preparations|loadMapFadeIn53,8,14; textCursor3723; loadMapEntities ce_5F1DE; I(0); stopEntity130,131,132,133; MUSIC_BATTLE_THEME_1; fadeInB; I(130),I(134),I(135),I(136); custom speed4,4 on130,134,135,136 in that order.|
|Preparations continued|EA130:Right3; EA134:Right3; EA135:Right3; EA136:Right3; setFacing128 Left; csWait30; setFacing128 Right; csWait30; shiver128; T3723; setCamDest10,14; setPos129,15,16,Down; animEntityFX129,MOSAIC_IN; T3724..3726; nod129; setCameraEntity129; EAwait129:Right2,Up2; setFacing129 Right; T3727..3728; nod129 twice; csc_end.|
|cs_5ED06, first flight|textCursor3729; loadMapFadeIn73,0,0; loadMapEntities ce_5F1BE; I(0); executeSubroutine sub_5F31E; MUSIC_MITULA_SHRINE; setBlocks0,10,22,10,42,10; fadeInB; executeSubroutine sub_5FD92; T3729..3731.|
|Ground cutaway|loadMapFadeIn53,10,13; loadMapEntities ce_5F23E; I(0); MUSIC_BATTLE_THEME_1; stopEntity128,130,131,132; fadeInB; T3732.|
|Second flight|loadMapFadeIn73,0,0; loadMapEntities ce_5F1BE; I(0); executeSubroutine sub_5F31E; MUSIC_MITULA_SHRINE; setBlocks0,10,22,10,42,10; fadeInB; executeSubroutine sub_5FD92; T3733..3736; csc_end.|
|cs_5EDB8, firing stage|textCursor3737; loadMapFadeIn53,10,13; loadMapEntities ce_5F28E; I(0); MUSIC_ENEMY_ATTACK; setActscript eas_5EF46 on133..140 in ascending order, all nonwaiting; stopEntity141..144 ascending; fadeInB; setFacing129 Left; T3737; EAwait129:Right1; T3738.|
|First volley|SFX_BATTLEFIELD_DEATH; setPos133,12,16,Left; csWait30; setFacing133 Up; SFX_PRISM_LASER_CUTSCENE_FIRING; setPos133,13,15,Up; EA133:UpRight10; flashScreenWhite2; setPos134,13,15,Up; EA134:UpRight10; csWait20.|
|Second volley|SFX_BATTLEFIELD_DEATH; setPos135,13,17,Left; csWait30; setFacing135 Up; firing SFX; setPos135,14,16,Up; EA135:UpRight10; flashScreenWhite2; setPos136,14,16,Up; EA136:UpRight10.|
|Third volley|SFX_BATTLEFIELD_DEATH; setPos137,14,19,Left; csWait30; setFacing137 Up; firing SFX; setPos137,15,18,Up; EA137:UpRight10; flashScreenWhite2; setPos138,15,18,Up; EA138:UpRight10; csWait10.|
|Fourth volley|SFX_BATTLEFIELD_DEATH; setPos139,15,20,Left; csWait30; setFacing139 Up; firing SFX; setPos139,16,19,Up; EA139:UpRight10; flashScreenWhite2; setPos140,16,19,Up; EA140:UpRight10.|
|Fifth volley, reusing139/140|SFX_BATTLEFIELD_DEATH; setPos139,12,16,Left; csWait30; setFacing139 Up; firing SFX; setPos139,13,15,Up; EA139:UpRight10; flashScreenWhite2; setPos140,13,15,Up; EAwait140:UpRight10; csc_end.|
|cs_5EF60, interior rebuild|textCursor3739; loadMapFadeIn37,4,6; loadEntitiesFromMapSetup9,11,Down; executeSubroutine InitializeNazcaShipForceMembers; setPosZ,8,10,Right; executeSubroutine sub_5F32E; I(P),I(B),I(Z); setPos128,9,12,Up; setPosP,10,12,Up; setPosZ,10,11,Left; fadeInB; shiver128; T3739..3743.|
|First hit and reaction|SFX_BATTLEFIELD_DEATH; setQuake3; flashScreenWhite20; setQuake1; csWait10; setQuake0; EAwaitP:Right1,jumpRight1,jumpLeft1,jumpRight1,jumpLeft1; setFacingP Left; T3744; setFacing0 Right; setFacingZ Down; T3745..3746.|
|Second and third hits|death SFX; quake3; flash20; quake1; wait10; quake0; wait10; death SFX; quake3; flash20; quake1; wait10; T3747..3748; EAwait128:Right1; setFacing128 Up; shiver128; T3749..3750.|
|Fourth and fifth hits|death SFX; quake3; flash20; quake1; wait10; death SFX; quake3; flash20; quake1; wait10; setActscriptWaitP,eas_Jump twice; T3751..3753.|
|Sixth and seventh hits|death SFX; quake3; flash20; quake1; wait10; T3754..3755; death SFX; quake3; flash20; quake1; wait10; quake3; executeSubroutine sub_5F338; fadeOutB; quake0.|
|Exterior descent|mapLoad75,20,14; loadMapEntities ce_5F1CE; I(0); setCameraEntity128; custom speed16,16 on128; cameraSpeed `$10`; EA128:Left12; executeSubroutine csub_5F326; fadeInB; executeSubroutine sub_5FD92; csWait150; SOUND_COMMAND_FADE_OUT; executeSubroutine sub_5F16C.|
|Crash and handoff|quake1; SFX_INTRO_LIGHTNING; flashScreenWhite60; waitIdle128; setBlocks56,59,4,5,12,17; csWait120; quake0; executeSubroutine csub_5F14C; csWait120; warp37,8,12,Down; csc_end.|

The selected source has45/66/35/97/119 normalized macro records respectively,
including nested actions and delimiters. These are not opcode byte sizes,
timing samples or counts of gameplay actions. Eight effect actors support
five paired volleys;139/140 are reused before their previous motion is
explicitly waited upon. Only the final140 action is waiting. No bullet hit,
damage calculation, HP decrement or battle simulation occurs in these
scene commands. A death-named SFX is not a kill or HP write.

## Stage populations and live identity

**Confirmed:** csc42_loadMapEntities reads mainEntity's three words,
clears/rebuilds entities and loads sprites. `map/mapfunctions.asm` creates
flag-selected followers first, then reads fixed records to cscEntitiesEnd,
then creates Bowie0 and conditionally handles the raft. A special mapsprite
at/above240 uses physical47, appends its non-ally alias, and does not
advance the ordinary slot counter. Thus Zeon251 and ship253 each occupy47
in their separate stages. Their layer seed is0 after declaration, regardless
of the preceding stage's layer. No table spawn recruits a party member.

For each list below all action pointers are eas_Init. Rows are in source
declaration order, with logical selector, X,Y/facing and source sprite.
The mainEntity row seeds the player independently of those records.

| List / mainEntity | Complete entity sequence |
| --- | --- |
|ce_5F19E /0,0,Up|128:13,4,Up/ZEON;129:13,8,Up/GESHP;130:13,25,Up/ODD_EYE; end.|
|ce_5F1BE /63,63,Up|128:5,5,Up/NAZCA_SHIP; end.|
|ce_5F1CE /63,63,Up|128:25,19,Up/NAZCA_SHIP; end.|
|ce_5F1DE /0,0,Up|128:11,19,Up/EXECUTIONER;129:63,63,Right/GESHP;130:9,16,Right/PRISM_FLOWER;131:13,17,Up/PRISM_FLOWER;132:14,19,Left/PRISM_FLOWER;133:15,20,Up/PRISM_FLOWER;134:8,16,Right/DRAGONEWT;135:10,16,Left/DRAGONEWT;136:11,16,Left/DRAGONEWT;137:15,19,Down/CHAOS_WIZARD;138:13,19,Right/CHAOS_WIZARD; end.|
|ce_5F23E /0,0,Up|128:12,16,Right/PRISM_FLOWER;129:16,18,Right/GESHP;130:13,17,Right/PRISM_FLOWER;131:14,19,Right/PRISM_FLOWER;132:15,20,Right/PRISM_FLOWER;133:11,16,Right/DRAGONEWT;134:12,17,Right/CHAOS_WIZARD;135:13,19,Right/DRAGONEWT;136:14,20,Right/CHAOS_WIZARD; end.|
|ce_5F28E /0,0,Up|128:11,16,Right/DRAGONEWT;129:16,18,Right/GESHP;130:12,17,Right/CHAOS_WIZARD;131:13,19,Right/DRAGONEWT;132:14,20,Right/CHAOS_WIZARD;133..140:each0,0,Left/EFFECT5 (eight distinct records);141:12,16,Right/PRISM_FLOWER;142:13,17,Right/PRISM_FLOWER;143:14,19,Right/PRISM_FLOWER;144:15,20,Right/PRISM_FLOWER; end.|

For N successfully declared ordinary followers, ordinary stages' selector
128+k maps to physicalN+1+k. In ce_5F19E,128 maps47,129 mapsN+1 and130
mapsN+2 instead. Followers can themselves overwrite one another's aliases;
they do not consume fixed non-ally alias128. Coordinate63,63 is a source
offstage placement, not a HideEntity operation, collision proof or despawn.
Actors130..133 in ce_5F1DE and141..144 in ce_5F28E receive stopEntity:
the actual csc1C writes animation counter255, not motion cancellation or
an action-pointer idle wait.

Map73 and75 are overworld maps;59 and53 use the ordinary follower table.
For65 held on an overworld, the Caravan65/FOLLOWER_A30 prefix continues
directly into table_Followers through its final tableEnd.w. It does not
terminate at the table_Followers label. With65 clear or a non-overworld,
only that prefix is skipped. Ordinary flags in exact order are
66/Sarah,67/Peter,72/Kazin,73/Slade,74/B/Astral,66/Chester,
68/B/Oddler,75/Luke,69/C/Fairy,70/B/Astral,74/A/King,
71/Lemon,74/D/Minister,76/Zynk,77/A/OldMan,82/Frayja,
83/A/King,83/D/Minister,84/Rohde. Each held row allocates another
physical actor; later assignments to the same selector win. No membership
or HP guard in that walk converts follower flags into joined status.
Capacity/action-pointer safety for arbitrary combinations is not proven.

On73 with only65/70/67/76 held, declaration order gives Caravan1,
Peter2,Astral3,Zynk4, so3729 uses26→4,3730 uses7→2 and3731 uses31→3.
With only65/67/76 held,31 remains cleared alias0 and those31 lines resolve
Bowie. With74/68/70 held,31 ends on the later70 Astral while earlier
Astral/Oddler actors remain allocated. Dialogue intent never fixes31 to
Astral. Toggles76/836 alone cannot establish70. Logical7/26 can likewise
be absent or overwritten; source GetEntityAddressFromCharacter does not
reject a cleared alias.

On37, InitializeFollowerEntities explicitly returns without ordinary
followers. With999 clear in cs_5EF60's interior reload, reuse the
predecessor's complete31-record first-visit crew table: ally1..29 in
identity order, then128/Astral at16,12/Up and129/Caravan at23,12/Up.
Ally physical slots1..29 align with their IDs;128→30,129→31. The code
then explicitly calls InitializeNazcaShipForceMembers: for each1..29,
clear joined flag hides that physical slot at63,63, faces Down and installs
eas_Idle, regardless of HP or active-party status. This differs from
loadEntitiesFromMapSetup alone. FOLLOWER_B31 is still a cleared alias0,
so I(B) here initializes Bowie, not128/Astral or physical31/Caravan.

setPosZ,8,10/Right precedes the later10,11/Left position; the second
wins only if executed. setPos/setFacing HP-skip zero-HP allies below30,
while the selected setActscript, sequence, text and waitIdle commands do
not. An unjoined living Zynk may be brought back from63,63 by setPos;
a dead Zynk skips it. Rebuilding or showing him is not recruitment.
The conditional raft helper can overwrite physical31 while129 still
aliases31; player type,64 and raft map remain explicit preconditions.

## Actions, maps and native service semantics

**Confirmed:** source eas_5EF46 is exactly ac_animSpeedX2 ON,
ac_transparency ON, ac_entityUncollidable OFF, ac_autoFacing OFF,
ac_setSpeed64,64, ac_jump eas_Idle. It does not fall into cs_5EF60:
ac_jump terminates its action path. `entity/entityscriptengine_2.asm`
sets FLAGS_B 2X_ANIMATION_SPEED and BLINKING for the first two operations,
clears FLAGS_A bit5 for uncollidable OFF, clears auto-facing and stores
speed64 per axis. Transparency names a blinking flag, not alpha blending;
uncollidable OFF does not disable all collision. Subsequent entityActions
also masks FLAGS_A with `$9F`, clearing entity/map uncollidable bits5/6.
The eight installations are nonwaiting; service completion before a later
replacement is not proven solely by the intervening fade/dialogue.

Shared eas_Init/eas_Jump and sequence/manual installation semantics are
owned by the voyage document. Here the speed4 preparations and speed16
ship motion replace the speed32 initialization. Nonwaiting sequences can
overlap and be replaced. jumpRight/jumpLeft use the shared jump actions;
Peter's later two eas_Jump calls each wait for pointer-idle. The final
waitIdle128 polls ACTSCRIPTADDR==eas_Idle in a tight loop; it is neither
WaitForEntityToStopMoving's coordinate poll nor a guarantee of visible
landing. VInt/action progression is required and there is no timeout.
setPos/facing request sprite work, while stopEntity only stops animation.

csc37_loadMapAndFadeIn configures an out-to-black fade and **falls through**
to csc48_loadMap; it does not finish with an automatic fade-in. The explicit
fadeInB later in each scene is separate. csc48 waits for fading, loads
tilesets/map/area under VInt control, updates CURRENT_MAP and waits one
VInt; it does not invoke fresh exploration, map setup init, CheckBattle
or recruitment. csc46_reloadMap calls LoadMap with−1 to reload the current
map/area at its camera seed, leaving CURRENT_MAP and actor allocation
intact until the following loadMapEntities. Its first use remains on59.
The later mapLoad75 is another scene load, not a warp to a battle.

Map53 declares area0,0..27,31, both foreground/background origins0,0,
parallax256/256, scroll0,0/0,0, type255. Map73 has first area0,0..10,9,
second foreground0,0/background0,10, scroll0,0/1,0, type255; its second
area0,0..63,63 has both origins0,0, zero scroll and type254. The explicit
0,0 camera selects the first area under LoadMap's ordered bounds checks.
Both have parallax256/256. Reuse the predecessor's Map37/75 area records.
Each LoadMapArea overwrites all four scroll bytes; native seeds below
apply after that load, not to a permanently shared travel velocity.

Both73 setBlocks calls copy working-layout0,10..21,19 to42,10..63,19:
22×10 words=440 bytes per call. Final75 copy56,59..59,63 to12,17..15,21
is4×5 words=40 bytes. csc34 passes packed operands to CopyMapBlocks and
requests both view planes; its source is the current64-word-row RAM
layout. Binary payloads are absent in the registered source checkout;
actual block values, collision/markers and visible terrain remain Unknown.

| Native helper / complete effective source path | Effect and waits |
| --- | --- |
|sub_5F31E→csub_5F326|Write layer2 X scroll byte−16 (240); nop; **fall through** to write special layer24; rts. A label is not a return. Called twice on73.|
|csub_5F326 directly|Write special layer24; rts. Does not change scroll; selected after75 movement installation.|
|sub_5F32E|Write layer2 X scroll byte−8 (248); nop; rts.|
|sub_5F338→loc_5F33A|d7=15;16 iterations decrementing layer2 X byte, nop, d0=4/Sleep, dbf; rts. From248 ends232 (−24);64 requested VInt waits if Sleep returns.|
|sub_5F16C→loc_5F16E|d7=23;24 iterations decrementing special layer byte, d0=2/Sleep, dbf; rts. From explicit24 ends0;48 requested VInt waits.|
|csub_5F14C|Copy palette_NazcaShip to base palette4 using32 bytes; a0=base palette1; clear FADING_TIMER_BYTE; call UpdateBasePalettesAndBackupCurrent; rts.|

The arithmetic is byte modulo256. For changed seed b the two ramps end
`(b−16)&255` and`(b−24)&255`; explicit writes240/248/24 replace their
incoming byte rather than subtracting from it. The source scroll consumer
has special0 and128 branches; other bytes sign-extend. A ramp crossing
those values need not behave like a uniform signed velocity. The75 area
reload resets scroll to0 before csub_5F326 sets layer24. The special253
projection consumes the zero-extended layer as position offsets across
its18 sprite records, as described in the voyage owner; it is not an
altitude or physics model. Here the layer ramp lowers24 to0 if uninterrupted.

Reuse the voyage's full sub_5FD92/CopyBytes/palette-service facts: each
selected call writes byte1 to FADING_TIMER_BYTE and copies32 bytes (16
words) to base palette4`[$FFD0E0,$FFD100)`, then schedules the base-bank
update and backs up current128 bytes`[$FFD000,$FFD080)` to
`[$FFDF2A,$FFDFAA)`. csub_5F14C uses a different source palette in that
same destination and clears the low timer byte instead. The shared helper
writes byte32 at FADING_TIMER_WORD `$FFDFAA`, distinct from low byte
`$FFDFAB`; hence the selected pair becomes32/0 rather than32/1. No raw
palette values are public and no immediate CRAM/DMA completion is claimed.

csc41_flashScreenWhite shifts its word operand right3 and uses dbf:
operands2/20/60 request1/3/8 LaunchFading calls, then DuplicatePalettes.
They are not literal elapsed2/20/60-frame observations. The five volleys
use2; the seven interior hit groups use20; the exterior crash uses60.
Quake3/1/0 have no ramp bits and directly set amplitude. cameraSpeed16
writes VIEW_SCROLLING_SPEED as a word; actor speed16,16 is a separate
motion field. Text, camera, portrait, mosaic, sound, fades, palette and
movement all require their actual services. No wait proves another actor's
completion, and an earlier stalled wait prevents publication of the warp.

## Selected text commands

**Confirmed:** cursors3699/3723/3729/3737/3739 produce57 commands through
3755 under normal non-debug text progression. `S` means nextSingleText
(close portrait/text and Sleep10 after display); `N` means nextText (keep
windows open). Both advance the cursor once; `$80` is right-side portrait,
`$00` is ordinary side. Each command resolves the **live** selector for
portrait/speech. P2 debug Start skipping can consume the speaker without
display/cursor advance; the table does not assert debug/input state.
All summaries are deliberately short paraphrases.

| ID | Mode / modifier / selector | Summary | Ordered tokens |
| --- | --- | --- | --- |
|3699|S/00/128|approach question|`["{LEADER}","{N}","{W1}"]`|
|3700|S/80/129|aircraft report|`["{N}","{N}","{W1}"]`|
|3701|S/00/128|power motive|`["{N}","{W1}"]`|
|3702|N/80/129|opponent strength|`["{N}","{W2}"]`|
|3703|S/80/129|proposed plan|`["{N}","{W1}"]`|
|3704|S/00/128|invites explanation|`["{W1}"]`|
|3705|N/80/129|shootdown proposal|`["{N}","{N}"]`|
|3706|S/80/129|location continuation|`["{W1}"]`|
|3707|S/00/128|approval|`["{W1}"]`|
|3708|N/80/129|further request|`["{W2}"]`|
|3709|S/80/129|requests Odd Eye|`["{W1}"]`|
|3710|N/00/128|prior injury|`["{N}","{W2}"]`|
|3711|S/00/128|substitute assignment|`["{W1}"]`|
|3712|S/80/129|gratitude|`["{W1}"]`|
|3713|S/00/128|departure order|`["{N}","{W1}"]`|
|3714|S/80/129|acknowledgement|`["{W1}"]`|
|3715|S/00/130|criticizes Geshp|`["{N}","{W1}"]`|
|3716|S/00/128|asks agreement|`["{W1}"]`|
|3717|N/00/130|addresses Zeon|`["{W2}"]`|
|3718|S/00/130|doubts interception|`["{N}","{LEADER}","{W1}"]`|
|3719|S/00/128|questions doubt|`["{W1}"]`|
|3720|N/00/130|hidden ability concern|`["{N}","{LEADER}","{W2}"]`|
|3721|S/00/130|caution|`["{N}","{W1}"]`|
|3722|S/00/128|anticipates spectacle|`["{N}","{W1}"]`|
|3723|S/00/128|preparation order|`["{N}","{N}","{W1}"]`|
|3724|S/80/129|readiness question|`["{W1}"]`|
|3725|S/00/128|readiness answer|`["{W1}"]`|
|3726|S/80/129|approval|`["{W1}"]`|
|3727|N/80/129|taunt|`["{W2}"]`|
|3728|S/80/129|weapon threat|`["{N}","{W1}"]`|
|3729|S/80/26|island sighting|`["{W1}"]`|
|3730|S/80/7|excitement|`["{W1}"]`|
|3731|S/00/31|homecoming reaction|`["{W1}"]`|
|3732|S/80/129|ship sighting/order|`["{N}","{W1}"]`|
|3733|S/00/31|distant flash|`["{N}","{W1}"]`|
|3734|S/80/7|dismisses concern|`["{N}","{N}","{W1}"]`|
|3735|N/00/31|grumbling|`["{W2}"]`|
|3736|S/00/31|addresses Peter|`["{NAME;7}","{W1}"]`|
|3737|S/80/129|attack signal|`["{W1}"]`|
|3738|S/80/129|firing order|`["{W1}"]`|
|3739|S/00/128|repeats observation|`["{W1}"]`|
|3740|S/80/7|disagreement|`["{W1}"]`|
|3741|S/00/128|concern|`["{W1}"]`|
|3742|S/80/26|incoming warning|`["{LEADER}","{N}","{N}","{W1}"]`|
|3743|S/00/128|asks identification|`["{W1}"]`|
|3744|S/80/7|impact reaction|`["{N}","{W1}"]`|
|3745|N/80/26|ground attack|`["{W2}"]`|
|3746|S/80/26|control failure|`["{W1}"]`|
|3747|N/80/26|engine damage report|`["{N}","{W2}"]`|
|3748|S/80/26|crash warning|`["{W1}"]`|
|3749|N/00/128|alarm|`["{W2}"]`|
|3750|S/80/26|identifies trap|`["{W1}"]`|
|3751|N/80/7|fear|`["{W2}"]`|
|3752|S/80/7|appeal|`["{W1}"]`|
|3753|S/00/128|landing request|`["{NAME;26}","{N}","{N}","{W1}"]`|
|3754|S/80/26|uncertainty|`["{W2}"]`|
|3755|S/80/26|attempt commitment|`["{W1}"]`|

128/129/130 mean Zeon/Geshp/Odd Eye only in ce_5F19E; in the preparation
scene128 is Executioner,129 Geshp, and in the firing stage128 is Dragonewt,
129 Geshp. Interior128 is the fixed Astral, independent of FOLLOWER_B.
The spoken damage/crash assertions do not establish an engine HP or
damage state. Split rows3705/3706,3745/3746 and3754/3755 remain separate
commands with separate token sequences; no inferred punctuation/token is
added to a continuation.

## Request publication, initializer return and fresh Map37

**Confirmed:** csc07_warp writes type1 word, parameter1=0, then bytes
37,8,12,Down3. It overwrites any pending request without a queue/conflict
guard. CURRENT_MAP remains75 at publication. csc_end returns from the
fifth script to the initializer; only then does the initializer set999,
write EGRESS_MAP=45, set436,440,442,443 in that order and rts. No444,
completion536/540/542/543,836 or76 write occurs in this selected path.
If a scene never returns, none of that tail has completed; even a published
request is not evidence of consumed transfer or completed flag persistence.

RunMapSetupInitFunction returns to ExplorationLoop; only later does
WaitForEvent see the request and ProcessMapEvent clear its type word before
type1/parameter0 unwinds exploration with explicit map/position/facing.
The existing [request producer](./map-event-request-state.md) and
[consumer](./map-event-request-consumption.md) own that mechanism.
MainLoop runs SwitchMap/CheckBattle before fresh exploration. The complete
flag-switched table has no37 source row, and the45-row battle map table
has no37 row. Newly held436/440/442/443 therefore do not start a battle
inside37; scene53 was not a Battle36 admission either. Their later-map
meaning is outside this slice.

Fresh positive37 entry clears the request word, adjusts STEP_COUNTER,
heals living allies plus immortal Peter/Lemon, sets CURRENT_MAP37 and
CURRENT_BATTLE−1, selects the live setup and rebuilds its actors at
Bowie8,12/Down, loads sprites, clears temp256..383, sets80, loads map/area,
installs base VInt services, then invokes the live setup init. Healing
restores HP/MP and keeps only stun/poison/curse status bits for those it
heals; other zero-HP allies stay dead. It does not depend on membership
in this routine. Consequently this entry can alter resources even though
the attack script has no combat damage operation.

`data/maps/mapsetups.asm` chooses ms_map37_flag999 with999 held. Its six
pointers select alternate entities/entity events and shared zone events,
descriptions/item section, plus ms_map37_flag999_InitFunction. Pointer
selection rechecks live flags, not an atomic snapshot. The alternate
31 fixed records below all use eas_Init, in ally-ID order followed by the
two non-allies; ordinary followers remain suppressed. The first-visit table
used during cs_5EF60 is separately retained in the predecessor.

| Physical / logical | Alternate fixed position/facing |
| --- | --- |
|1 Sarah|15,5,Down|
|2 Chester|16,6,Down|
|3 Jaha|18,6,Down|
|4 Kazin|17,8,Down|
|5 Slade|19,7,Down|
|6 Kiwi|20,6,Down|
|7 Peter|12,12,Up|
|8 May|21,6,Down|
|9 Gerhalt|23,6,Down|
|10 Luke|19,9,Down|
|11 Rohde|21,12,Up|
|12 Rick|21,8,Down|
|13 Elric|22,8,Down|
|14 Eric|23,9,Down|
|15 Karna|25,9,Down|
|16 Randolf|18,11,Up|
|17 Tyrin|19,12,Up|
|18 Janet|16,13,Up|
|19 Higins|17,14,Up|
|20 Skreech|18,16,Up|
|21 Taya|20,14,Up|
|22 Frayja|21,14,Up|
|23 Jaro|21,16,Up|
|24 Gyan|23,14,Up|
|25 Sheela|63,14,Up|
|26 Zynk|10,10,Down|
|27 Chaz|63,10,Down|
|28 Lemon|63,10,Down|
|29 Claude|23,12,Up|
|30 /128 Astral|16,12,Up|
|31 /129 Caravan|25,11,Up|

The selected alternate init first calls InitializeNazcaShipForceMembers,
so unjoined1..29 are hidden again. It then reads256. Fresh entry has
cleared256, so it dispatches cs_5FABC. **Nothing in that body is admitted
here.** The following setFlg256 is after the script returns and has not
executed at this stopping point. It is not a persistent first-crash latch:
a later fresh entry clears temp flags again. A retained/reentrant init
with256 set returns without this scene. If999 was externally cleared,
the default initializer still initializes crew and reads836: held836
dispatches cs_5FA9C, clear836 returns. Stop at that dispatch/return as an
alternate boundary, not the default crash conclusion.

Across the selected scenes/helpers/tail there is no item removal/addition,
Orb-slot write, join/leave operation, battle completion or ally-HP damage.
Subject to calls returning and no external mutation, Sky Orb custody and
membership remain unchanged from admitted entry. The actual persistent
flag additions are999/436/440/442/443; EGRESS_MAP changes to45. Temporary
entry clearing/80 and resource healing are distinct effects. No claim of
actual saving or later natural battle completion follows.

## Changed-state checks and actual current consumers

These are small source-derived examples, not emulator observations:

| Changed state | Selected consequence |
| --- | --- |
|999 held before Map59 init|Five scenes skipped;443 clear returns,443 held at that later read selects cs_5EB26. Stable443 held earlier would admit Battle43 first, as the predecessor explains.|
|999 externally held at interior reload|Alternate crew table selected early; later actor requests still apply their opcode/HP/alias rules. Does not recruit anyone.|
|999 held, fresh37, incoming256 set|Fresh temp clear resets256; alternate init dispatches cs_5FABC. Retained init with256 still set returns instead.|
|999 clear at37 init|Default crew helper;836 chooses cs_5FA9C versus return. No crash-body inference.|
|65/70/67/76 versus65/67/76 on73|31 resolves Astral3 versus cleared0; same dialogue ID can select a different portrait/speech.|
|74/68/70 held|Later70 wins31; earlier physical actors remain. No table termination at label or automatic despawn.|
|Joined26 clear, HP26 positive/zero|Crew helper hides26; later setPos can restore living26, skips dead26. Actions/text are not skipped for that reason.|
|HP7 zero during scene versus fresh37|Selected action/text still run during scene; fresh entry heals immortal7 before allocation/init. Ordinary dead26 is not revived there.|
|Scroll/layer seed0,1,128,255|Explicit helpers replace bytes; decrement loops wrap modulo256 and may cross0/128 consumer cases. Layer1−24 wraps233; layer24−24 ends0.|
|Old request or blocked wait|Warp overwrites an old request only when reached; blocked earlier work gives no later writes. If published then interrupted before initializer tail, flags are not assumed written.|

**Confirmed (current code):** actual OriginalPrograms invocations in
`src/sf2tool/remake_exploration_programs.py` register the entire selected
Map59 file, Map37 script file and headquarters helper file, including all
nonlocal helper/loop targets. No truncation/frontier or injected successful
action cache is used. Each root compile has its own instance so a prior
failed partial cannot become a cached success.

| Selected root | Actual compiler result |
| --- | --- |
|cs_5EB44|ValueError special cutscene entity not admitted at ce_5F19E's Zeon251. Four-instruction partial only; not a successful scene.|
|cs_5EBFC|Successful complete74-instruction lowering ending end-map-script, no native-call. This is representation, not a bound/running Map53 scene.|
|cs_5ED06|Same special-entity rejection at ce_5F1BE's ship253; five-instruction partial, not success.|
|cs_5EDB8|ValueError selected source label is missing:eas_5EF46; eight-instruction partial. Complete Map59 registration includes this label, but action_stream looks only in shared eas_actions.asm. This is the current local-action resolution gap, not an omitted import.|
|cs_5EF60|Same special-entity rejection at later ce_5F1CE's ship253;159-instruction partial plus selected helper outputs. Earlier native operations remain unsupported; no successful full crash program.|
|sub_5F31E|Successful two native-call records (move.b,nop); current compiler stops at terminal native and omits the real csub_5F326 fallthrough.|
|csub_5F326 /sub_5F32E|Successful2/3 instructions: native move.b,end /native move.b,native nop,end.|
|sub_5F338 /loc_5F33A|Root jump to fully registered loop; loop native subq.b,native nop,native jsr,native dbf,end (5). No actual loop/Sleep execution.|
|sub_5F16C /loc_5F16E|Root jump to registered loop; native subq.b,native jsr,native dbf,end (4).|
|csub_5F14C /sub_5FD92|Each7 instructions: six native-call records then end; immediate moveq tracking emits no loop/palette operation.|
|InitializeNazcaShipForceMembers|19 instructions including18 native calls and end; successful emission does not implement its local branches/physical writes.|

Separately feeding the six actual eas_5EF46 action rows to the existing
action mapper preserves animSpeedX2/transparency as native-call,
emits flagsA mask32/value0, flagsB mask64/value0 and speed64,64;
ac_jump alone is native in that mapper (action_stream normally owns the
idle-jump special case). This diagnostic is not a repaired full script.
Shared eas_Transparent likewise retains unsupported native transparency.
No artificial missing-local-target initializer probe is promoted to a
compiler defect; Map37 init selection is checked directly against source.

Content's `Scenarios/ExplorationContentReader.cs` maps native-call to
UnsupportedInstruction/UnsupportedEntityAction. Application's
`Runtime/Exploration/ProgramRunner.cs` and EntityActionRunner stop with
program-opcode/entity-action respectively. Typed ordinary scene entities,
motions, text, quake, reload/scene maps and queue-warp exist; position/facing
do not automatically acquire the original HP-skip/sprite semantics.
LoadSceneMap requires a bound map; its text-aware BindScene requires closed
windows, a settled black display and a valid base palette. ReloadScene has
its own window/palette checks. Successful lowering does not prove those
runtime preconditions or reader admission of an assembled package.

`Domain/Maps/MapEntityAllocation.cs` allocates non-allies generically, not
special sprites into47. SceneEntities consumes its flag-selected population;
the imported population still lacks37's explicit follower exclusion.
mapLoad, loadEntitiesFromMapSetup, setBlocks, flashScreenWhite, cameraSpeed
and the selected native scroll/palette/crew operations are not implemented
by their default native-call emission. Godot's ExplorationPresentation
handles its current bound camera/quake/fade/sprite/audio cues; it does not
establish the18-piece ship, blinking/laser/palette service parity here.
PrivateBattleEncounterReader still binds only encounters1/2/3. No current
selected package binds this five-scene sequence or fresh37 crash handoff.
Minimum follow-up gaps are concrete content binding, special/local-action
allocation/resolution, native services and guard parity, not a new simulator.
This research does not dispatch those implementations.

## Verification, preservation and Unknowns

Reproduction after same-process ignored input configuration load:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1110/review.py <registered-source-root> --committed
uv run sf2 verify plan --base origin/main --head HEAD
```

The direct recipe checks selected complete source command/action/table
order, helper effective paths/widths, initializer/request joins, changed
states, current compiler output/failure and actual consumers,57 text
IDs/modes/speakers/tokens, two-path/navigation-only scope, relative links,
UTF-8/LF and private boundary. It preserves pair size/mtime and never
executes game services. Exact clean-head checks, planner selection and
actual scoped CI are recorded separately in the ignored handoff/PR.
Planner suggestions are not executed passes. No normal/full, old comparison,
SDK, Godot, emulator/H3/H4, screenshots or new fixture/schema/parser.

Preserve #1108's original7ee6a237/treeccb2227d author0.262s and
CI39520102343 which missed the continuous follower-table P2; corrected
d1405151 author0.240s/main0.242s/CI39521090928 was accepted into this base.
No old audit is rerun. Preserve #1106's local eas_5E2C4/@Return failures,
missed operation-label checks and corrected24-end/26-jump27/27-end with
conditional caller cleanup. Preserve #1104 count-attribution, #1100
public-full-text and other named historical diagnostics through the
predecessors/handoffs, including twelve completed old comparison failures.
None is reset, renamed interrupted or made an automatic work queue.

**Unknown:** natural entry and continuous reach; actual flag/membership/
HP/inventory/alias/follower/raft/player/debug/input state; pending requests
and reentrancy; binary map blocks/collision/markers; actual completed
VInt/action/motion/sprite/DMA/palette/fade/camera/text/portrait/mosaic/audio
services, rendered appearance and timings; usable control, saving and
persistence. Post-crash scene/exploration, later battles and alternate
ending remain outside this acceptance. No static result or partial compile
establishes a playable ship or complete supported subsystem.

Freeze clean committed/pushed Draft PR, exact head/tree/base, selected
checks/actual CI, failures/Unknowns and stopped process state; Project
Review / Verify and required main-gate notification precede task completion.
Independent acceptance/merge stays with main-gate. No next Issue, merge,
cleanup or archive.
