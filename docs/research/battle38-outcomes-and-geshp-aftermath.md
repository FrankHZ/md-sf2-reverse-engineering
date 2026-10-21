# Battle38 outcomes, Geshp aftermath and exploration return

Evidence date: 2026-10-21. **Confirmed (static):** with Bowie alive,
combat Geshp128's death can queue all remaining living enemy slots for
removal. Both terminal checks give defeat precedence. Buster Shot is a
conditional action-side drop from enemy137, not a victory award. Clear538
runs the complete aftermath with scene Geshp136, eight effects128–135
and off-screen speaker137. It preserves the pre-scene return seed;
exploration rebuilds Map66 at the selected active member's battle position
plus crop17,0 and its aliased facing. The y28 callback can then unlock439,
subject to its own invocation and flags. Natural reach, completed services,
usable control and persistence remain **Unknown**. One lowering retains
unsupported program and action instructions; it is not runnable acceptance.

## Agreement, provenance and method

Standard rigor for [Issue #1150](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1150)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect accepted [initialization](./battle38-initialization-and-opening.md),
[activation](./battle38-activation-and-first-turn-dispatch.md) and
[Burst Rock actions](./battle38-burst-rock-explosion.md) at accepted base
`9a315fa3d11400f236fa5bf9bb8463eced46ea8f` to outcomes, the full Geshp
scene and exploration return for future content/design consumers. Own this
new document and navigation only in the Burst Rock owner. No shared paths,
index/registry/schema/fixture/tool/design/engine changes, competing writer
or unmerged dependency. Stop at Battle39 admission; its initialization,
actions and later story, optional towns/blacksmith/Lemon recruitment and
whole-battle simulation remain outside.

Reuse [Battle37 outcomes](./battle37-outcomes-and-lemon-aftermath.md),
[Battle36 aftermath](./battle36-outcomes-and-galam-return.md),
[shared drop/return services](./battle35-outcomes-and-roft-return.md),
[corrected death accounting](./battle34-outcomes-and-sky-orb-handoff.md),
[Map66 routes and admission](./granseal-overworld-return-and-battle38-admission.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[scripting](./common-scripting.md), [maps](./common-maps.md),
[request state](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md),
[lifecycle](../design/contracts/battle-control-lifecycle.md) and
[cutscene routing](../design/contracts/battle-cutscene-routing.md).
Their shared mechanisms remain authoritative; selected38 operands,
complete source joins and actual consumers are checked here.

Original paths are relative to clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` means `code/gameflow/battle/`; `map/` and `entity/` abbreviate
children of `code/common/scripting/`. sf2const.asm, sf2enums.asm,
sf2macros.asm and sf2cutscenemacros.asm own constants, widths and encodings.
Fnnn means story flag nnn; ordinals are zero-based, intervals half-open.
USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source/ROM equality, decoded terrain or original-runtime observation
is claimed.

Minimum evidence is selected tables/scene/helpers, the immutable
18,285,979-byte init/script pair with `local/issue1005/pair.txt` provenance,
one existing-compiler lowering and actual Content/Domain/Application/
runner/adapter reads. Source reads and one pair load are linear in these
bounded inputs; expected inputs are under20MB, receipts under1MB, no
network acquisition or model processing of the full map pair. Each command
targets one minute/512MiB; actual time/size is retained, peak memory
unmeasured. Small ignored receipts stay in `local/issue1150/`. No full
export/import, new parser/platform/helper tests, normal/full/old comparison
suites, SDK/Godot/emulator/H3/H4 or screenshots. Reassess material input,
cost or semantic expansion before proceeding.

## Terminal priority, selected hooks and leader cleanup

**Confirmed:** `battle/battleloop_1.asm:BattleLoop` returns from
ExecuteIndividualTurn, optionally runs debug kill-all, then calls
ExecuteBattleCutscene_Defeated, ProcessKilledCombatants and
CountRemainingCombatants. Ally count0 chooses defeat before enemy count0
chooses victory. A nonterminal result processes the actual turn-buffer
actor's after-turn effects, cleans deaths and counts again with the same
priority. There is no second defeated hook. Only a second nonterminal
result advances the byte turn cursor by2. An admitted retreat unwinds
before this suffix.

| Complete table / selected38 use | Selected operand |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`:48 relative words, base `$47BE8`|slot38/byte76 at `$47C34` → ms_Empty, sole FFFF word.|
|`data/battles/global/enemyleaderpresence.asm`:45 bytes, base `$47C8E`|byte38 at `$47CB4` → -1, nonzero leader cleanup.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`:48 relative words, base `$47CF4`|slot38/byte76 at `$47D40` → abcs_battle38 `$4DE8C`.|
|`data/battles/cutscenes/afterbattlejoins.asm`:52 bytes, base `$47D6A`|byte38 at `$47D90` →0, passed to JoinForce.|
|`data/battles/global/afterbattlepositions.asm`|Only battle5 has a row; no38 post-leader position override.|

`battle/cutscenes/battleendcutscenesstart.asm` requires Bowie0 HP.w!=0
and enemy128 HP.w=0. At this seam128 is **combat Geshp**, kind95 from
spriteset38's first enemy record, not an effect or scene136. Clear538
interprets ms_Empty; held538 skips that interpreter call. Both reach
`battleendcutscenesend.asm:loc_47C48`, reread CURRENT_BATTLE.b and use
its leader byte. Unchanged38 scans all32 enemy IDs128–159 and appends
every nonzero-HP ID to the existing death list, increments its word length
and writes FF sentinel. It neither clears that list nor tests X/Y,
neutral/activation state or membership in the twenty declared records.

ProcessKilledCombatants requests death spins/sound/effects/waits. A listed
ally increments its own defeats; only the enemy branch temporarily uses
BATTLESCENE_FIRST_ALLY for kills, then restores the enemy ID. The listed
combatant receives X/Y=-1, status0, stat refresh, and physical position
$7000,$7000 through its alias. It does not set HP0, clear list length or
call DropEnemyItem. Living enemies from leader cleanup can leave the
count through X without an action kill/drop. Stale/duplicate lists and
FIRST_ALLY/physical aliases remain caller inputs.

CountRemainingCombatants visits all30 allies and32 enemies, requiring
signed X.b>=0 and HP.w!=0. It ignores Y, joined/active membership,
neutral/activation bits and declared roster size; nonzero high-bit HP
counts too. Bowie HP0 finally forces ally count0. The separate
ApplyPositionsAfterEnemyLeaderDies uses Bowie/enemy128 gates and a word
battle lookup; selected38 misses its sole battle5 row and repositions
nobody. This listener does not replace terminal detection.

| Supplied state at these seams | Consequence |
| --- | --- |
|Unchanged eleven STARTING enemies, nine unspawned rocks HP0/X−1|Eleven count; hidden declarations alone do not keep the battle alive.|
|A rock was actually reset/spawned, has HP45 and nonnegative X|It counts even with activation bit clear; appearance completion is a different boundary.|
|Bowie alive,128 HP0, ten other STARTING enemies alive and rocks still HP0|Sweep can append the ten; it skips the nine zeroHP rocks.|
|Same with a living hidden slot or other unused enemy slot|That slot is swept too, independent of roster/neutral/activation state.|
|128 alive/positioned, all others gone|128 keeps enemy count nonzero; AI label LEADER is not the count test.|
|128 alive but all enemies fail X/HP count|Victory can occur without the defeated hook's HP0 gate.|
|Bowie HP0, another living ally, no enemies / both counts0|Defeat wins; wrapper skips. All allies off-map also loses with Bowie alive.|
|128 becomes HP0 only in after-turn effects|No second defeated hook; other positioned living enemies can keep battle running.|
|538 already held|Empty hook skips, but gated sweep and both count priorities remain.|

Death/action playback, cleanup and terminal return are separate, ordered
prefixes. The prior Burst Rock owner retains construction/restoration and
reactive actor identity. Nothing here turns a pending reaction, emitted
death request or F88-resumed state into a completed natural battle.

## Buster Shot drop and independent item state

**Confirmed:** `data/battles/global/enemyitemdrops.asm` contains30
four-byte records plus word terminator (`$BE52..$BECC`). Record14 at
`$BE8A` is `VERSUS_GESHP / enemyEntity9 / BUSTER_SHOT / droppedFlag14`.
`sf2macros.asm:enemyEntity` emits9+128=**137**. This is combat Dark
Gunner at20,15, ordinal9, not combat Geshp128, the other Dark Gunner129
at7,26, or the later off-screen **scene** speaker137.

`data/stats/enemies/enemydefs.asm` kind65 DARK_GUNNER supplies
BUSTER_SHOT52|EQUIPPED plus three NOTHING slots. Both declared gunners
can have that weapon; only137 matches this drop row. Placement NOTHING
adds no item and does not erase its definition weapon. Evil Bishop134's
placement FAIRY_TEAR7|USABLE_BY_AI is separate; there is no38 Fairy Tear
drop row. The Fairy Tear record later in this table belongs to battle40,
enemy ordinal14/flag17. Equipment/placement inventory is not entitlement.

At each of the three direct action-engine calls to
`battleactions/dropenemyitem.asm:battlesceneScript_DropEnemyItem`:

1. Actor byte at a4 must have enemy bit clear, target byte at a5 enemy
   bit set, and targetDies.b nonzero. The live battle byte and full target
   byte must match the ordered row. Reactive rock actors are enemies and
   return early; their kills are not additional drops for the original ally.
2. GetItemSlotContainingIndex masks indices and scans four **actual**
   target slots, returning first match or−1. Matching broken/equipped
   entries qualify, a missing52 does not. No party-wide inventory search
   substitutes for this target.
3. Item52 is not Taros Sword, Iron Ball or Counter Sword, so it bypasses the
   special range32 RNG draw. The later IsBattleUpgradable block is
   unreachable and supplies no repeat-battle chance.
4. BSET tests/sets dropped bit14 at
   `$FFF7A4+floor(14/8)=$FFF7A5`, mask40 hexadecimal. A previously held
   bit returns before removal/award. It is independent of story F14,
   unlock438, intro488 and completion538.
5. First admission removes the matched target slot, arranges inventory
   and refreshes target stats. The current allied actor's HP.w is then
   checked. HP0 takes the rare fallback; otherwise AddItem attempts that
   actor's four slots. No joined/active, equipment-class or Caravan-room
   predicate is added; it does not search other force members.
6. Success passes bare index52 to AddItem, yielding an unequipped weapon,
   and requests dropped396 then received395. The target's equipped/broken
   word is not transferred. Actor full/HP0 instead attempts RARE Deals,
   without those two success messages.

Buster Shot52's definition is SNIP|BRGN|BWNT, range2..3, price6800,
WEAPON|RARE, useSpell NOTHING, effects INCREASE_ATT37/NONE0/NONE0.
AddItemToDeals uses one nibble/item and adds only below15; a full15
discards the addition. Victory, leader sweep and aftermath have no
independent DropEnemyItem call or guaranteed award.

| Changed supplied state / interrupted prefix | Result |
| --- | --- |
|Ally kills137 with52, clear bit14, living actor has room|One bare52 can reach that actor if preceding services return.|
|Same but actor full or HP0|Rare fallback attempts Deals; count15 can lose the addition after bit/removal.|
|137 lacks52, or target129 has52|No selected row+slot match; no new bit14 or selected award.|
|Enemy actor kills137, or targetDies0|Early return; death/victory alone does not retry the drop.|
|Bit14 already held, even with a fresh52 copy|No removal or new award at this seam.538/488 changes do not reset it here.|
|Leader sweep removes living137|No action-side drop; victory does not repair that omission.|
|Interrupt after BSET / removal / award before text or538|Reached item/bit state can precede story completion; no rollback or save persistence is established.|

## Victory return seed, repeat skip, defeat and retreat

**Confirmed:** `battle/battleloop_2.asm:BattleLoop_Victory` first calls
HealLivingAndImmortalAllies. It visits30 slots: living allies and Peter7/
Lemon28 even at HP0 receive maxHP/maxMP, retain STUN/POISON/CURSE and
refresh stats; other dead allies are skipped. Immortal maxHP0 can remain0.
Battle44's timer exception is not38.

Before the after wrapper, CURRENT_MAP.b is saved to MAP_EVENT_PARAM_2.
UpdateForceAndGetFirstBattlePartyMemberIndex rebuilds lists and reads the
**first active-list byte**; its actual combatant X/Y receive byte additions
of BATTLE_AREA_X/Y into PARAM3/4. Its physical alias supplies facing to
PARAM5; PARAM1=0 is written without posting event TYPE. This member need
not be Bowie or the last actor, alive/positioned, or even a valid nonempty
list member. The shared owner retains unchecked stale-list and alias
semantics: alias0 can read physical0, FF can read facing beyond the table,
without LoadBattle's fallback/3F mask.

Unchanged map66/crop17,0 saves66 and byte-wrapped memberX+17,memberY.
Supplied19,28→36,28;250,250→11,250. These are arithmetic seam cases,
not observed endings. It does not save camera3,22, mainEntity8,23,
Geshp's ending position or a church point.

`battle/cutscenes/afterbattlecutscenesstart.asm` tests live
500+CURRENT_BATTLE: clear538 selects abcs_battle38, held538 skips the
**entire** scene. Both reach EndAfterBattleCutscene, reread the selector
and call JoinForce with byte0. It sets Bowie's joined0, rebuilds lists,
then sets active32 only below twelve active members; a preexisting32
survives capacity, a newly set32 is absent from the just-built snapshot.
It joins neither Zynk26 nor any scene Geshp/follower and runs no join scene.

After wrapper return victory rereads CURRENT_BATTLE, clears400+index
then sets500+index: unchanged38 clears438 and sets538. It does not
write intro488, next unlock439/completion539, follower76, or drop bit14.
Clear d0–d3 **words**, copy PARAM2–5 bytes, return d4=1; upper longword
halves are not established by clr.w. PARAM1=0 differs from d4=1.
Selector reads for hook/skip/table/join/clear/set are independent; an
intervening selector or request mutation can change later results.
Healing/seed/scene/join/clear/set are nontransactional prefixes.538 alone
does not prove finished text, motion, drop possession or usable control.

Defeat calls UpdateBattleUnlockedFlag, which clears438 only if538 is
already held, then clears dialogue name1, requests sad music/txt363/close,
restores Bowie HP=max and halves current gold by logical longword shift.
It selects egress, returns d4=-1, and does not run aftermath/join/victory
completion. Battle4's losable case is not38. **Selected38 has an additional
unconditional438 clear in GetEgressPositionForBattle**, so defeat clears
438 even when538 is clear, provided that later helper is reached.

Reuse [admitted Egress/Angel Wing resource and stack-unwind semantics](./battle35-outcomes-and-roft-return.md#defeat-egress-and-angel-wing-actual-destination-state):
MP or item removal/refresh precedes text/sound/flash. Admitted retreat
updates unlock, unwinds the action frame, selects egress and returns
clr.w d4, bypassing the terminal suffix, defeat gold recovery and after/
completion tail. Selected38's egress helper likewise clears438; it newly
sets neither538 nor488. Denied retreat is not this return path.

`battle/battleloop/getegresspositionforbattle.asm` checks38 then39 for
their unlock clears, but38 has no fixed-town destination case. After
other battle16/17/13/31/32 checks it reads live EGRESS_MAP.b and calls
`code/common/maps/egressinit.asm:GetSavepointForMap`. Held399 scans23
rows:66 has no row, retaining66 with1,1/Up; supplied Granseal3 selects
32,13/Up; clear399 selects initial3/56,3/Down. Held64 additionally resets
raft state through the accepted table. An ordinary BattleLoop entry sets
399, but this direct helper's changed flag cases remain distinct. No
actual last save, EGRESS_MAP or visited church is inferred.

## Complete Geshp scene and identities

**Confirmed:** `data/battles/entries/battle38/cs_afterbattle.asm` spans
`$4DE8C..$4E3D2`. abcs_battle38 starts at$4DE8C; its only flag branch
joins cs_4DF00 at$4DF00. ce_4E372 at$4E372 contains a six-byte main
header, eleven eight-byte records and word terminator,96 bytes total:

| Record order / resolved reference | Complete declaration |
| --- | --- |
|Main / Bowie0 physical0|mainEntity8,23,Down.|
|0 / Peter7|5,25,Right,ALLY_PETER,eas_Init.|
|1–8 /128–135|Each63,63,MAPSPRITE_EFFECT3=178,eas_Init; facings alternate Right/Left starting Right.|
|9 /136|6,27,Down,MAPSPRITE_GESHP=159,eas_Init.|
|10 /137|63,63,Down,MAPSPRITE_FILLER4=162,eas_Init.|

Here B=Bowie0, P=Peter7, A=FOLLOWER_A30, S=FOLLOWER_B31 and G=136.
S and A are references resolved through current follower flags, not
unconditional Astral/Caravan identities. `single` means nextSingleText;
`next` means nextText. Dialogue defaults flags0 except C0 where shown.
Init/NoFace/Stop/Jump/RL/Anim2/JumpL/JumpR/Die below name eas_Init,
eas_DeactivateAutoFacing/eas_StopMoving/eas_Jump/eas_2xRightLeft/
eas_AnimSpeedx2/eas_JumpLeft/eas_JumpRight/eas_Die. `wait-script`
means setActscriptWait; `script` means nonwaiting setActscript. A listed
movement stream ends endActions and waits unless explicitly marked async.

| Ordered block | Complete source sequence |
| --- | --- |
|Entry/population|textCursor3012; loadMapFadeIn66,3,22; loadMapEntities ce_4E372; wait-script B Init, P Init; setPos P5,25,Right; wait-script S Init; setPos S8,28,Up; wait-script A Init; setPos A9,24,Down.|
|Lying and sole branch|stopEntity G; customWait G [motion OFF,orientLeft,updateSprite,jump Idle,end delimiter]; jumpIfFlagClear76→cs_4DF00. Held76: wait-script Zynk26 Init; setPos26,63,62,Down; fall through.|
|Reveal|wait-script128..135 NoFace in ascending order; priority B0,GFFFF; fadeInB; wait60; shiver G; single3012 G; wait60; customWait G [motion OFF,orientUp,updateSprite,jump Idle,end]; startEntity G; facing G Up; single3013 G.|
|Attempted escape|customWait G [speed8,8,jump Idle,end]; moves G Right1,Up1,Left1,Up2,Right1,Down1,Right1,Up1; **async** moves G Up1; wait12; wait-script G Stop; quake1; stopEntity G; setPos G8,24,Left.|
|Tumble|customWait G [motion OFF,orientLeft,updateSprite,jump Idle,end]; wait-script G NoFace; customWait G [speed40,40,jump Idle,end]; quake0; move G Down1; facing Down; move Down1; facing Right; move Down1; facing Up; wait40; shiver G; wait60.|
|Panic|customWait G [motion OFF,orientUp,updateSprite,jump Idle,end]; facing G Down; wait40; headshake G; startEntity G; wait10; wait-script G Jump, then NoFace; single3014 G; move G Up2; single3015 G.|
|Mercy|move S Up1; single3016 S/C0; single3017 G; move P Right1; single3018 P/C0; facing G Left; single3019 G; wait-script G Init; facing G Up; cameraEntity G; wait60.|
|Party watches|move B Right1; facing B Down; wait60; cameraSpeed8; customWait G [speed8,8,jump Idle,end]; move G Up1; facing P Up,B Left; move G Up2; facing B Up; move G Up2; wait40; facing G Down; single3020 G; move G Up3.|
|Interruption|flashScreenWhite30; sound SFX_BATTLEFIELD_DEATH; quake3; wait50; wait5; **script** G RL; wait80; quake0; wait40; facing G Down; tintMap; single3021 speaker137/C0.|
|Judgment|customWait G [speed48,48,jump Idle,end]; wait-script G Jump twice; wait30; wait-script G NoFace; move G Up1; single3022 speaker137/C0; headshake G; next3023 speaker137/C0; single3024 speaker137/C0; single3025 G; priority G0.|
|Eight placements|setPos128,8,13,Right; wait10;129,10,14,Left; wait5; **script** G RL; wait10;130,11,16,Right; wait10;131,10,18,Left; wait10;132,8,19,Right; wait10;133,6,18,Left; wait10;134,5,16,Right; wait10;135,6,14,Left; wait40.|
|Attack preparation|flashScreenWhite20; tintMap; facing G Down; quake3; customWait each128..135 in order [speed80,80,jump Idle,end].|
|First strike|sound SFX_BOLT_SPELL; move128 Down3; facing G Left; sprite G OBJECT3=180; **script** G Anim2; **script**128 JumpL.|
|Second strike|sound BOLT; move129 DownLeft2; **script**129 JumpR; hide128.|
|Third strike|sound BOLT; move130 Left3; **script**130 JumpL; hide129.|
|Fourth strike|sound BOLT; move131 UpLeft2; **script**131 JumpR; hide130.|
|Fifth strike|sound BOLT; move132 Up3; **script**132 JumpL; hide131.|
|Sixth strike|sound BOLT; move133 UpRight2; **script**133 JumpR; hide132.|
|Seventh strike|sound BOLT; move134 Right3; **script**134 JumpL; hide133.|
|Eighth strike|sound BOLT; move135 DownRight2; **script**135 JumpR; hide134; wait8; hide135.|
|Geshp removal request|quake0; cameraEntity NEUTRAL; wait40; sprite G GESHP159; wait-script G Die; flickerOnce; cameraSpeed30 hex; cameraDest3,20 (waits).|
|Closing|move S Up3; single3026 S; moves P Right1,Up2; single3027 P; single3028 S; follow A→B position2; P→A position1; S→A position3; csc_end.|

No warp, reloadMap/resetMap, reset-return, joinForce/joinBattleParty,
flag write, item delivery or start-battle command appears. The only story
test is76. The defeat/retreat path never selects this scene; held538
skips all of it, including population and final follows.

### Allocation, followers and speech

`map/mapscriptengine_1.asm:csc37/csc48` requests black fade, detaches
view, loads map66/tilesets with packed camera3,22 and VInt/display
services. LoadMap writes CURRENT_MAP66, retains CURRENT_BATTLE38 and
does not execute fresh exploration setup/init or CheckBattle. csc42
separately clears/rebuilds entities and loads their mapsprites before
reactivating display/VInts. The saved PARAM2–5 are not written by those
helpers. Camera coordinates and mainEntity are distinct operands.

LoadMapBlocksAndLayout applies held layout copies/chest markers then,
because CURRENT_BATTLE remains38, copies crop17,0 size32×32 to working
origin0,0. LoadMapArea's battle branch sets layer1 bounds0..31 on both
axes rather than the ordinary66 area's0..63. The declared scene positions
and camera are in that retained battle view; they are not full-map return
coordinates. Fresh exploration later sets battleFF and rebuilds the full
Map66 layout/area. Current selectors/crop and completed load services are
premises, not values established by the scene's map number alone.

`map/mapfunctions.asm:loc_440E2` creates followers first, explicit
records next, main physical0 last. Existing nonzero ally alias7 suppresses
the explicit Peter record; its following setPos repairs position only
when that command's HP check permits it. Non-ally explicit records assign
sequential references128–137 independent of physical-slot offsets from
followers. Effects178, Geshp159 and filler162 are regular non-ally sprite
operands, not special-sprite aliases or combatant reinitializations.
Combat Geshp128/kind95 and scene136/sprite159 share an authored character
role but not a combatant/HP identity. Scene128 is an effect, scene137 is
the off-screen voice; neither inherits the earlier combat slot's role.

`data/scripting/entity/followers.asm` and
`map/followersfunctions_1.asm:InitializeFollowerEntities` resolve S31
through74/68/70 and A30 through74/77/83 in table order. On overworld66,
held65 prepends Caravan/A30 before ordinary rows; later A aliases can
overwrite it while earlier physical entities remain. F71 independently
declares Lemon28; F76 declares Zynk26, independent of joined26/active58
or HP. No76 branch creates recruitment. With76 clear, its extra Init/
parking commands skip; with76 held but Zynk HP0, Init is unguarded while
setPos can skip. Other follower flags and transport state remain inputs.
Missing aliases initialize0; unguarded commands can then act on physical0.

GetEntityAddressFromCharacter masks word identity toFF, subtracts96 for
enemy-bit IDs and reads the physical alias without zero/FF/capacity
validation. setPos/setFacing use byte identity for the ordinary-ally HP
precheck; start/stop use word identity. G136 and effects128–135 bypass
that check; ordinary Peter/Zynk/Bowie commands can skip at HP0; pseudo
A30/S31 bypass it. Custom/action scripts, priority, shiver, sprite and
camera calls have their own unchecked alias reads. Commands do not
establish identity by matching the displayed sprite.

`entity/getentityportaitandspeechsfx.asm` resolves the current aliased
mapsprite through `data/spritedialogproperties.asm`.159 explicitly maps
to GESHP portrait45 and DIALOG_BLEEP_6.162/FILLER4 has **no row**, so
unchanged137 uses PORTRAIT_DEFAULT=FFFF and DIALOG_BLEEP_6=74, not an
implicit Zeon portrait or demon-breath voice. **Inferred:**137 represents
Zeon's off-screen judgment from authored3021–3024 and the closing
attribution to Zeon. This does not change137 into combat Zeon. C0 retains right/mirrored portrait
modifiers, not an unconditional portrait resource binding.

### Movement, camera and presentation services

stop/start set ANIMCOUNTER.b toFF/0; they do not halt travel, hide, kill
or change combatant HP. Inline motion OFF also writes animation counterFF;
orientLeft/Up changes orientation bits then explicitly refreshes. csc14/
csc15 installs action pointer and physical-slot byte wait timer; waiting
spins until the pointer equals eas_Idle. ac_end is the inline delimiter,
not the jump-to-idle behavior. csc2D stores the same timer, ANDs flagsA
with9F (clearing5/6), expands signed byte step operands and appends Idle;
wait variants spin for that pointer, async variants return immediately.
The async Up1 followed by wait12 and Stop does not prove one full tile.
The later setPos8,24 expressly replaces that uncertain position/destination.

`data/scripting/entity/eas_actions.asm` defines Init as accel0, speed32,
accel/decel OFF, entityObstructable ON, entityUncollidable OFF,
mapUncollidable OFF, autoFacing ON, orientUp, immersed OFF, size24,
refresh, jump Idle. Actual esc19/1A handlers clear flagsA6/5 for OFF;
the clear bit5 bypasses HasSameDestinationAsOtherEntity's entity scan.
The script naming does not add ordinary route/terrain admission. esc04
multiplies signed relative deltas by384 and adds current word position,
without a0..63 clamp; it still depends on obstruction/travel services.
Speed8/40/48/80 are stored word operands, not measured durations. NoFace
clears auto-facing bit6 in flagsB. Stop requests relative0,0 then Idle;
it is separate from stopEntity's animation counter.

Jump disables auto-facing, requests0,−1/wait5 then0,+1/wait5,0,0,
reenables auto-facing and idles. JumpL/JumpR use−1,−1 then−1,+1 or
+1,−1 then+1,+1 with waits6. These are velocity/travel requests around
waits, not guaranteed completed tile displacements. RL refreshes Right,
Left,Right,Left with wait30 after each. Nonwaiting RL/Anim2/JumpL/JumpR
may overlap later calls or be replaced. Anim2 sets flagsB bit4; it does
not double a script's wait count. Fresh Init later resets selected movement
properties, not every global or animation flag.

With declared aliases and completed waited movement, the explicit reset
G8,24 plus Down3→8,27, Up2→8,25, then Up1+2+2+3+1→8,16.
All eight waited effect approaches converge on8,16 from the placements
in the table. These conditional arithmetic joins explain staging; they
are not observations of movement or a general scene simulator. Prior
async travel/RL/jumps/interleaving can change actual state. B's only
waited move takes8,23→9,23, and S8,28→8,27→8,24; neither overwrites
the saved battle return seed.

csc53 priority0 writes byte_FFAFB0[slot]=0; any nonzero word, including
FFFF, writes1. Entity sprite-link construction visits nonzero entries
first. G changesFFFF→0; B is set0. This is sprite ordering, not tile
priority or a story flag. csc24 camera target resolves G's physical alias;
NEUTRAL writesFF. csc45 directly writes VIEW_SCROLLING_SPEED.w with8
then30 hex. csc32 detaches the target, sets tile destination3,20 and
waits for scroll end. ExecuteMapScript's return conditionally waits for
view when dialogue is open, then clears scrolling speed. No camera path,
rendered framing or frame timing is established by these requests.

csc33 operands1/3/0 directly set QUAKE_AMPLITUDE.w; neither high-bit
ramp mode is selected. Shiver saves counter/global size, temporarily
uses FF/21 and toggles flagsB3 with refresh/Sleep5 on/off three times,
restoring counter/size but leaving bit3 clear. Headshake's separate
csc27 sets counterFF and loops seven times through two mapsprite/DMA
transform phases, each with two VInt waits before normal refresh and
one after, then writes counter0. Its42 requested VInts are not a measured
duration or the shiver flag sequence. Neither call is damage or death.

csc41 flash30 computes `(30>>3)+1=4` LaunchFading calls; flash20 computes
3, not the same four-pulse command. Each uses palette maskF, period1,
FLASH_QUICKLY_2 and blocking fade/VInt service, then DuplicatePalettes.
csc3D tint uses mask1/period6/HALF_OUT_TO_BLACK; csc3E flicker uses
mask1/period6/FLICKER_ONCE. LaunchFading restores the previous period and
maskF after completion. Repeated tint calls and flicker are distinct
operations; they are not generic fade-to-black or proof of final palette.
SFX_BATTLEFIELD_DEATH before judgment and eight BOLT requests accompany
the effect sequence; Die requests another death sound later. No actual
audio completion, DMA, palette outcome or total duration is claimed.

Hide resolves each effect's physical slot and writes position/destination
$7000,$7000, replaces matching alias bytes withFF, and removes it from
the follower list if present. It does not clear its action pointer/travel,
combatant HP, drop state or story flags; later installed actions are not
transactionally canceled. After restoring sprite159, **eas_Die** sets
animation counterFF, cycles Left/Up/Right/Down three times with the source's
individual waits3 (the first Down has no wait), requests death sound,
sets EFFECT1 sprite63, faces Up/Left/Down with refresh and wait8 after
each, sets position63,63 and jumps Idle. It is not HideEntity, a combat
kill, EXP/drop, or alias removal; no battle HP is changed by this scene.

The final csc2C commands use **word** follower/leader/place operands but
precheck `move.b (a6),d0`, the word's **high byte**. For all three selected
small IDs this is0: the native HP-skip is Bowie's, not Peter's or A/S's.
Then aliases resolve and place2/1/3 yields pixel offsets(−24,0)/(0,−24)/
(0,24). AddFollower copies42 bytes of its template, stores physical
leader/offset words and adds the physical follower to the list only if
absent. Thus A follows B, P follows A, S follows A; no final Zynk follow
is installed here. These are temporary scene actions before fresh
exploration population, not new joined/active/follower flags.

## Text requests and complete control tokens

**Confirmed:** `data/scripting/text/gamescript.txt` supplies17 successive
requests3012–3028. Only authored roles and complete ordered tokens are
reproduced; no dialogue corpus is copied. IDs assume unchanged unskipped
cursor flow. csc00 single closes portrait/text and requests Sleep10;
csc02 next leaves them open. Text skip can bypass display and cursor
increment. Presentation, acknowledgment and input ownership remain separate.

| ID / speaker / authored role | Complete ordered tokens |
| --- | --- |
|3012 G / failed traps|`["{W1}"]`|
|3013 G / exhaustion and intended escape|`["{N}","{W1}"]`|
|3014 G / panic|`["{W1}"]`|
|3015 G / plea for release|`["{W1}"]`|
|3016 S/C0 / mercy and lost powers|`["{N}","{N}","{W1}"]`|
|3017 G / acceptance|`["{W1}"]`|
|3018 P/C0 / permission to leave|`["{W1}"]`|
|3019 G / gratitude|`["{W1}"]`|
|3020 G / parting assurance|`["{W1}"]`|
|3021 speaker137/C0 / interrupts departure|`["{W1}"]`|
|3022 speaker137/C0 / recalls vow|`["{W1}"]`|
|3023 speaker137/C0 / failed jewel mission, continued text|`["{N}","{LEADER}","{W2}"]`|
|3024 speaker137/C0 / death sentence|`["{N}","{W1}"]`|
|3025 G / final plea|`["{W1}"]`|
|3026 S / reaction to Zeon's cruelty|`["{N}","{W1}"]`|
|3027 P / risk of losing|`["{N}","{W1}"]`|
|3028 S / resolve|`["{W1}"]`|
|396 / target dropped item|`["{NAME}","{N}","{ITEM}","{D1}"]`|
|395 / actor received item|`["{NAME}","{N}","{ITEM}"]`|
|363 / defeat exhaustion|`["{LEADER}","{W1}"]`|
|274 / admitted Egress spell/level|`["{NAME}","{N}","{SPELL}","{#}"]`|
|275 / admitted retreat item|`["{NAME}","{N}","{ITEM}"]`|

LEADER rebuilds active members and need not name hardcoded Bowie.
Authored claims about powers, mercy and death do not implement those
mechanics. Voice identity, current sprite/portrait lookup and combatant
identity remain separate even when names or reference bytes coincide.

## Exploration rebuild and Battle39 admission boundary

**Confirmed:** the complete scene and its selected load/population/action
helpers do not write MAP_EVENT_PARAM2–5. No warp/reset-return command
replaces the victory seed. After scene/skip/join/438/538, d0–d3 return
those saved bytes, not the scene's last actors/camera. Shared request
state can be changed by an intervening writer; this is the uninterrupted
source path, not an immutable return object.

`code/gameflow/mainloop.asm:MainLoop` returns from BattleLoop, runs
SwitchMap then enters ExplorationLoop **without another CheckBattle**.
The switched-map table has no66 row. d4=1/-1/0 distinguishes victory/
defeat/retreat; this exploration entry does not select population from d4.
`exploration/explorationfunctions_2.asm:ExplorationLoop` clears TYPE.w,
adjusts/clamps step counter, heals living/immortal allies again and fades
out. Destination66 is nonFF: set CURRENT_MAP66, CURRENT_BATTLEFF, obtain
setup, clear/rebuild population at returned d1–d3, load sprites, clear
setup-temp256..383, set80. Load map/view, install base VInts, run setup
init, conditionally request music/fade and reach event/input dispatch.
No scene effect, speaker137 or G136 survives ordinary fresh population.
Followers are rebuilt from retained flags; the three final scene follows
do not substitute for that population. The alternative destinationFF
keeps population/update-main/raft semantics; unchanged victory returns66.

The [accepted Map66 owner](./granseal-overworld-return-and-battle38-admission.md#complete-map66-local-surfaces-and-immediate-exits)
retains the single setup, zero fixed NPCs, RTS entity/description/item/init
callbacks,64×64 area/MUSIC_MAIN_THEME, F508/F611 block copies, no step/
roof/chest rows, ground Mithril flag213 at30,44 and all ten exits. Neither
538 nor Geshp's scene adds a layout-copy row or removes a warp. Copy flags,
marker/collision data, actual party/followers/transport and traversal
remain independent; no town or other optional branch is re-audited here.
Immediate nearby interfaces include Yeel19,23/18,22 and Devils Head35,28;
coordinate proximity is not a route or automatic exit.

`map66/mapsetups/s3_zoneevents.asm:Map66_ZoneEvent0` at y9/10 requires
537 held and538 clear before setting438/posting reload. With538 held it
returns without clearing a preheld438. **Map66_ZoneEvent2** at y28
requires538 held and539 clear before setting439/posting reload. It
does not require438 clear, set539 or initialize39. Failure of either flag
gate preserves preheld439/pending bytes. Invoked unmatched coordinates
use the separate battle3 random producer; story order does not suppress it.

The [accepted invocation/packet owner](./granseal-overworld-return-and-battle38-admission.md#invoked-zones-reload-bytes-and-competing-default-producer)
retains entity marker$1400→type6 versus warp$1000 dispatch and callback
waits. Zone coordinates alone do not invoke a callback. For uninterrupted
type6 at y28, long MAP_EVENT_RELOADMAP=$000100FF writes TYPE.w=1,
PARAM1=0,PARAM2=FF, retaining candidate-Y word PARAM3/4=00,1C and
prior PARAM5/WARP_SFX. The later no-scroll return overrides low X with0,
low Y with28, not the candidate X, and consumes retained facing/SFX.
Actual signed coordinate high bytes and any intervening writers remain
inputs. A posted reload is not proof of completed movement or entry.

After that return MainLoop runs SwitchMap→CheckBattle. The complete
45-row ordered table `data/battles/global/battlemapcoords.asm` has Map66
rows2/3/4/6/38/39; earlier eligible rows can win. Row39 at
`$7B47..$7B4E` is `66,17,20,23,32,255,255`, unlock439/completion539.
It requires map/unlock/trigger match, not a fresh538 test or area
containment. Held539 clears439 **while still admitting39 on that scan**.
Normal victory cleared438; a changed preheld438 can still admit earlier38,
even held538 (clear438 on that scan). Ordinary exploration return alone
does not perform this scan. Stop at the declared39 admission interface;
no39 initialization, actions or later story is inspected or executed.

## One lowering and actual consumers

**Confirmed about tooling:** unchanged
[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py)
registered selected source and compiled abcs_battle38 once in0.1365s,
error=None, retaining19 instructions in abcs_battle38 and267 in cs_4DF00.
The nine referenced named actions have lengths Init14, NoFace3, Stop3,
Jump9, RL14, Anim2=3, JumpL9, JumpR9, Die50. Inline scripts and movement
streams are retained in their owning instructions. Complete results are
in ignored `local/issue1150/observe-01.json`; no lowering/import/runtime
retry was performed to manufacture a green result.

The full result contains **seven program native-call occurrences and
four action native-call occurrences**, not a fully typed scene:

| Location in cs_4DF00 / action stream (zero-based) | Retained refusal |
| --- | --- |
|105,240|cameraSpeed8 /30 hex.|
|122,186|flashScreenWhite30 /20.|
|132,187|tintMap twice.|
|239|flickerOnce.|
|eas_AnimSpeedx2[0]|ac_animSpeedX2 ON.|
|eas_Die[36],[37],[47]|ac_soundCommand death /ac_setSprite EFFECT1 /ac_setPos63,63.|

The compiler does have a separate fixed flash cue in battle-route
preparation; it does not lower these two source opcodes or express their
different4/3 pulse counts. No patched compiler/substitute cue was used.
There are17 show-text requests, eight effect hides, the76 branch and three
typed final follows. This receipt proves complete lowering/refusal shape,
not Content admission, original service completion or runner execution.

**Confirmed about current production, by direct read:** prepared Content
is the production input. [Capability status](../../remake/docs/capability-status.md)
owns runnable claims. The accepted selected38 pins/HIDDEN/reset/terrain/
material/control/Burst Rock gaps remain open.

| Actual consumer | Representation, admission, execution or unbound boundary |
| --- | --- |
|[Preparation](../../src/sf2tool/remake_exploration_battles.py), [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Outcome hooks/join0/battleOrigin/configured egress have shapes. Pins admit1/2/3, not38. Private outcome admission requires EnemyLeaderPresent=false plus End/Return-only defeated programs:38's empty hook satisfies the latter, nonzero leader byte fails the former. Native calls can deserialize as explicit Unsupported instructions, not executable services. No admitted38 world/after resources result.|
|[BattleOutcomeRules](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs), [BattleDeathBatch](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleDeathBatch.cs), [BattleActionCommitter](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs)|Defeat priority and own-dead-ally/enemy-FIRST_ALLY accounting exist. Batch only admits distinct positiveHP→0 reactions and excludes living/already-off-map entries; it cannot execute arbitrary native death lists or living-enemy leader sweep. No unconditional Buster Shot outcome reward is provided.|
|[PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs), [PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [BattleRewards](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleRewards.cs), [BattleSceneContinuation](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs)|Item definition/index/equipment metadata and source loadouts are expressible. Actual placement admission permits NOTHING or the named Medical Herb expression, so selected Fairy Tear placement refuses; selected38 already lacks pins. Reviewed action rewards/scene phases credit EXP/growth/gold, with no native137/52/drop-bit14/Deals producer. General inventory capacity does not bind this conditional drop.|
|[BattleOutcome](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs), [Sf2OutcomeReturnPolicy](../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs), [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Pre-scene return anchor, completed skip and join→unlock→complete exist. Active.First(), bound positioned member and validated coordinates differ from native unchecked list/alias/byte wrapping. Nonzero status refuses return-status-refresh; egress is configured, with absent399 or held64/640 refused. The unconditional selected38 egress-clear path is not supplied by the generic completed-only Finish condition. Policy validation precedes published tail, unlike native interrupted prefixes.|
|[Compiler](../../src/sf2tool/remake_exploration_programs.py), [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapEntityAllocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs)|Eleven explicit records, sequential non-ally identities and ally/follower duplicate allocation are representable. Population uses ordinary followers, omitting65's overworld Caravan prefix; appearance is prepared start-class data rather than live class/battle/HP lookup. Capacity checks differ from native unchecked aliases. Complete scene import/material binding is unobserved.|
|[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Typed positions/motion/priority/counter/sprite/follow/quake/camera/branch/text have consumers; source HP prechecks, including csc2C's high-byte Bowie gate, are not reproduced by those typed operations. UnsupportedInstruction throws program-opcode; UnsupportedEntityAction throws entity-action, preserving typed failures. If earlier inputs/services succeed, first unbound program operation is cs_4DF00[105] cameraSpeed. Later action refusals are not made reachable by successful lowering. Relative movement additionally clamps0..63; source has no such general clamp.|
|[MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs)|Scene map load keeps population until explicit reload/sprite readiness and requires black/palette/text context; return consumes OutcomeReturn. Existing Map66 request/reload/scroll gaps remain. No selected538/y28→439 route or natural return is bound by this receipt.|
|[ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs)|Generic gesture/sprite/fade/quake/text projections exist, with modern shiver timing, sprite ordering separate from tile priority and a fixed white-four-pulses cue. They do not consume the seven Unsupported scene operations or four Unsupported actions. No selected38 visuals/audio/palette/view/portrait/input/persistence observation is claimed.|

Successful early typed operations do not fill later missing behavior.
Selected38 needs admitted content/outcome/leader cleanup, conditional drop,
native presentation/action support and exact alias/HP/return behavior plus
completed control/service evidence before a playable claim. These findings
authorize no engine repair, new platform or automatic runtime acquisition.

## Reproduction, retained failures and Unknowns

The sole lowering receipt retains all programs/actions/refusals and pair
metadata unchanged. The ignored direct review recipe checks selected source
pin/cleanliness, complete selected tables/scene/tokens, source helper order/
width/polarity, bounded terminal/item/flag/identity/return cases, actual
consumers, two-path navigation-only scope, UTF8/LF/links/fences and private
publication boundary. It is a local review receipt, not maintained tooling
or evidence of natural reach. Use an unused receipt destination:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1150/review.py <configured-source-disasm-root> --receipt local/issue1150/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The source argument contains sf2enums.asm/code/data. Apply ADR0004's
applicable evidence/date/provenance/selector/order/width/consumer/adversarial
checks; maintained parser/schema/fixture/helper tests and H3 callbacks are
N/A. Normal `uv run sf2 verify` is **SKIPPED** by explicit Issue/docs
scope; full/old/SDK/runtime gates remain excluded. Actual committed planner
and exact-head scoped CI belong in the frozen handoff, not inferred from
suggested commands or error=None.

Preserve #1148 direct/main-gate/CI and earlier accepted #1146/#1144/#1142/
#1140 results, Map53 event import UnboundLocalError/zero partial programs,
twelve completed old private comparison failures, Battle37 y64 runner
refusal, Map66 reload/cross-map-scroll and previous consumer gaps. The new
eleven native-call occurrences are retained findings, not a repaired pass.
No prior failure is erased, called interrupted, automatically fixed or
rerun. Scratch inspection/review failures and actual command resources
remain in the ignored handoff.

Freeze a clean committed/pushed Draft PR with base/head/tree, actual checks/
CI, failures/Unknowns and ignored handoff. Main-gate independently accepts
and integrates. Stop writer/processes at handoff; no merge, cleanup,
archive or next Issue follows.

**Unknown:** natural38 outcomes/retreat/after/zone/warp reach; live selectors,
F88/538/539/399/transport/follower flags, all combatant slots/HP/MP/maxima/
status/class/inventory/drop/Deals/gold/death-list/FIRST_ALLY/seeds, first
active member/aliases/positions/facing/EGRESS_MAP; interrupted/interleaved
requests and command-buffer headroom; completed death/action/travel/view/
text/portrait/window/audio/palette/flash/DMA/map/setup/input services,
original VInt/hardware timing, usable exploration and save persistence.
Source/ROM equality beyond accepted evidence, selected terrain/material
bindings and malformed states remain open. These are explicit boundaries,
not passed gates or an acquisition queue; Battle39 body remains outside.
