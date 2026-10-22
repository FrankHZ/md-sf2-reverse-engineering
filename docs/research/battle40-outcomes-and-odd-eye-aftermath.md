# Battle40 outcomes and Odd Eye's aftermath

Evidence date: 2026-10-21. **Confirmed (static):** defeat has priority.
With Bowie alive and combat Odd Eye128 at HP0, the leader hook can
remove remaining enemies from counting. Fairy Tear and Counter Sword
are independent action-side drops; the sword requires a range32 draw.
Victory seeds a return before the540-guarded Odd Eye scene. Its effects
128–130 and Odd Eye131 are scene aliases. Ordinary exploration rebuilds
entities/layout from the saved return instead of retaining that staging.
Map54 exits lead to Map28 or Map56. Natural completion and playable
remake support remain **Unknown**.

## Agreement and evidence

Standard rigor for [Issue #1162](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1162)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053),
from accepted `b927226c5db8ce38d696e9e34804757848b8f134`. This supplies
outcome, inventory, scene and return contracts for future content
integration. Own this document and successor navigation only in
[Battle40 first control](./battle40-regions-terrain-and-first-control.md).
No shared registry/schema/fixture/extractor/engine changes or stacked
dependency. Stop at Map54 exits and Map56's arrival/setup interface.
Creed, Map56 services, Battle41 admission, action AI/damage and
whole-battle simulation are outside this slice.

Reuse the accepted [opening/initialization](./battle40-initialization-and-opening.md),
[regions/layout](./battle40-regions-terrain-and-first-control.md),
[labyrinth/admission](./devils-head-labyrinth-and-battle40-admission.md),
[Battle39 outcomes](./battle39-outcomes-and-force-sword-route.md),
[Battle38 outcomes](./battle38-outcomes-and-geshp-aftermath.md),
[item transactions](./map-event-item-transactions.md),
[request production](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md),
[maps](./common-maps.md) and [scripting](./common-scripting.md).

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` abbreviates `code/gameflow/battle/`, `map/` abbreviates
`code/common/scripting/map/`. Fnnn means story flag, not a dropped-item
bit. sf2const.asm, sf2enums.asm, sf2macros.asm and sf2cutscenemacros.asm
own constants/encodings. Ordinals are zero-based; intervals half-open.
USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source/ROM equality, asset decode or runtime observation.

Minimum evidence is selected global rows, terminal/drop callers,
complete aftermath and actual production consumers. Reuse the
18,285,979-byte static init/script pair and `local/issue1005/pair.txt`
provenance read-only, loaded once for one selected aftermath lowering.
No opening/region re-lowering, full import, layout/terrain export or
comparison infrastructure. Work is linear in selected source/consumers
with fixed small branch cases. Estimate evidence below2MiB, all local
receipts/coordination below4MiB, seconds per direct command, peak-memory
target512MiB **unmeasured**. No runtime acquisition or model processing
of the full pair. Reassess dynamic acquisition, cross-system machinery
or materially larger output before expanding. Ignored recipes/receipts
are under `local/issue1162/`; older results remain read-only.

## Terminal detection, sweep and priority

**Confirmed:** `battle/battleloop_1.asm:BattleLoop` calls the defeated
hook after the individual turn/debug branch, then ProcessKilledCombatants
and CountRemainingCombatants. Ally count0 branches to defeat before
enemy count0 branches to victory. Only a nonterminal result runs that
actor's after-turn effects, cleanup and the same priority checks.
There is no second defeated hook; only another nonterminal result
advances the byte cursor by2. Admitted retreat unwinds before this suffix.

| Complete table / selected40 row | Exact selector/address |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`,48 relative words at$47BE8|slot40, byte80 at$47C38 → ms_Empty (FFFF).|
|`data/battles/global/enemyleaderpresence.asm`,45 bytes at$47C8E|byte40 at$47CB6 →−1.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`,48 relative words at$47CF4|slot40, byte80 at$47D44 → abcs_battle40.|
|`data/battles/cutscenes/afterbattlejoins.asm`,52 bytes at$47D6A|byte40 at$47D92 →0.|
|`data/battles/global/afterbattlepositions.asm`|Only battle5 has a row; no40 relocation.|

`battle/cutscenes/battleendcutscenesstart.asm` requires Bowie0 HP.w!=0
and enemy128 HP.w=0. spriteset40 identifies combat128 as Odd Eye96,
16,5, first of20 STARTING enemies. Its LEADER14 AI declaration is separate
from the table predicate. Clear540 runs ms_Empty; held540 skips it.
Both reach battleendcutscenesend.asm, reread CURRENT_BATTLE and test
the leader byte. Unchanged40's−1 scans32 IDs128–159, appends every
nonzero-HP ID to the existing death list, writes FF sentinel and
increments its word length. No X/Y, roster, activation, neutral or
deduplication check is made.

`battle/battleloop/processkilledcombatants.asm` (`$24518..$24642`)
performs spin/effect/sound/waits. Listed allies receive their own defeat;
enemies credit BATTLESCENE_FIRST_ALLY, then restore the enemy ID.
It writes X/Y=−1, status0, refreshes stats and parks the aliased entity
at$7000,$7000. It neither writes HP0, calls DropEnemyItem nor clears
list length. Living swept enemies can disappear without a held-item award.

CountRemainingCombatants (`$23C58..$23CBA`) scans30 allies and32 enemies,
requiring signed X.b>=0 and HP.w!=0. Y, membership, activation, neutral and
declared roster size are ignored. Bowie HP0 finally forces ally count0.
ApplyPositionsAfterEnemyLeaderDies has the same Bowie/128 HP gates but
its battle5-only table supplies no40 override.

| Supplied seam state | Consequence, assuming services return |
| --- | --- |
|All20 fresh enemies positioned/living|All20 count, even with PRIMARY clear.|
|Bowie alive,128 HP0, other19 alive|The19 can be swept and fail later X admission; no guaranteed Tear.|
|Same plus a living undeclared148–159 slot|It is swept too, even if already off-map; stale list entries are not deduplicated.|
|128 HP!=0 but X<0, all others fail X/HP|Ordinary victory can occur without the defeated hook.|
|128 living/positioned, all others absent|Still nonterminal; defeating subordinates is insufficient.|
|Bowie HP0 and no enemies, or both counts0|Defeat wins and defeated hook skips. All allies off-map also loses with Bowie alive.|
|128 dies only in after-turn effects|No second sweep that turn; other countable enemies can keep battle running.|
|540 held|Defeated interpreter and later aftermath skip; leader sweep and terminal priority remain.|

These are supplied branch distinctions, not natural reach or a full
turn/round simulation.

## Two conditional inventory transactions

**Confirmed:** `data/battles/global/enemyitemdrops.asm`
(`$BE52..$BECC`) has30 four-byte records plus word terminator.
`enemyEntity` adds128; its ordinal is not a physical entity slot.

| Record/address | Exact selected match | Independent dropped bit |
| --- | --- | --- |
|17 /$BE96|battle40, enemyEntity14=142 Evil Bishop84 at5,23; FAIRY_TEAR7|17 →$FFF7A6 mask02 hex|
|28 /$BEC2|battle40, enemyEntity0=128 Odd Eye96 at16,5; COUNTER_SWORD68|28 →$FFF7A7 mask10 hex|

ENEMY_ITEM_DROPPED_FLAGS starts$FFF7A4, independent of story F17/F28,
440/490/540 and placement UNUSED_ITEM_DROP. Accepted fresh142 inventory
is `[00DF,0007,007F,007F]`: equipped Great Rod95, unequipped Tear7.
Odd Eye128 has equipped Counter Sword00C4, from its enemy definition;
its placement says NOTHING. A declaration is neither held inventory
nor receipt. InitializeEnemyItems/AddItem mask807F strips placement
AI/drop/equipped bits, preserves BROKEN8000 and cannot add a fifth
slot. Copied enemy-definition equipment has a separate refresh path.

`battle/battleactions/battleactionsengine_1.asm` has three direct BSR
sites to `dropenemyitem.asm:battlesceneScript_DropEnemyItem`
(`$BD24..$BE52`). Each reached call requires this sequence:

1. Actor byte enemy bit clear, target byte enemy bit set, targetDies.b!=0;
   live CURRENT_BATTLE.b and full target byte match the selected row.
   HP0 alone and leader cleanup are insufficient.
2. GetItemSlotContainingIndex masks index and scans four actual target
   slots, first match or−1. Equipped/broken copies match. Missing item
   returns before changing dropped flags; no placement-bit predicate.
3. Tear bypasses the special RNG. Counter Sword joins Taros Sword and
   Iron Ball in GenerateRandomOrDebugNumber with range32; only0
   continues. Ordinary output0..31 is not a guaranteed award or measured
   probability. Debug Right forces0, Up1, Left2, Down3, in that test order;
   otherwise generator runs. The following unconditional jump bypasses
   the later repeat-battle chance code, which is unreachable.
4. Then BSET tests/sets17/28. Held bit returns without removal/award;
   Counter Sword can already have drawn before this test. Failed special
   draw leaves bit/item unchanged.
5. First admission removes the target slot, arranges inventory and
   refreshes stats. The allied actor's live HP.w then gates AddItem
   into that actor's four slots. No force-wide/Caravan search or class/
   equip eligibility gate. Bare index is passed: award0007/0044, without
   target equipment/broken bits. Success requests396 then395.
6. Actor HP0 or full inventory instead tests RARE after bit/removal.
   Tear is CONSUMABLE, not RARE: no Deals addition. Sword is WEAPON|RARE:
   AddItemToDeals increments its nibble unless already15; at15 addition
   is lost. Neither fallback requests those pickup messages.

`data/stats/items/itemdefs.asm` defines Tear as no equip classes,
range0–1, price300, use G_TEAR|LV2, three NONE0 effects. Counter Sword is
HERO|BRN|BDBT|RDBN, range1–1, price13000, use NOTHING, ATT+39, counter+1,
then NONE0. Definition/equip capability is separate from receipt.

| Changed state | Result |
| --- | --- |
|Correct ally kill, held target item, clear bit, living actor/room; sword draw0|One bare item can be received;17 and28 remain independent.|
|Sword draw!=0|No bit/removal/award; victory does not retry it.|
|Actor full or HP0 after admission|Tear lost; sword Deals addition below15, otherwise lost.|
|Wrong target/battle, enemy actor, targetDies0, missing actual item|No selected drop; another holder does not satisfy this row.|
|Bit held, target reconstructed with item|No removal/award; sword can still consume its preliminary draw.|
|Odd Eye death sweeps living142|No action-side Tear transaction; sword needs its own actual killing-action call.|
|Interruption after bit/removal/award but before messages/540|Reached mutation can precede scene/completion; rollback/save persistence unproved.|

## Victory seed, defeat and retreat

**Confirmed:** `battle/battleloop_2.asm:BattleLoop_Victory` first
heals living allies and immortal Peter7/Lemon28 even at HP0, to maxima;
preserves STUN/POISON/CURSE and refreshes stats. Other dead allies skip.
It saves CURRENT_MAP.b to PARAM2, rebuilds force/active lists and reads
the first active-list byte. Actual X/Y receive byte additions of
BATTLE_AREA_X/Y into PARAM3/4; that combatant's physical alias supplies
PARAM5 facing. PARAM1=0 is written without posting MAP_EVENT_TYPE.
PARAM1..5 are bytes at$FFA84C..$FFA850. Empty list/stale alias has no
managed fallback in this native selection.

Unchanged40's battlemapcoords row is Map54, crop0,0/32×44. Saved return
is Map54, memberX, memberY, alias facing. The selected member need not be
Bowie, last actor, alive or positioned; it comes from active membership.
Scene Bowie15,10/Up, Odd Eye15,8 and loadMap camera10,4 are not this seed.
Later staging does not rewrite PARAM2–5. Changed crop uses wrapping
byte sums, not clamping.

ExecuteAfterBattleCutscene reads CURRENT_BATTLE/540; clear runs
abcs_battle40, held skips it. Both reach EndAfterBattleCutscene, reread
the selector and pass join byte0 to JoinForce. This rejoins Bowie:
joined0, rebuild lists, active32 only if fewer than12 active. Existing
active32 survives full capacity; a newly written32 is absent from the
just-built snapshot. It recruits neither Lemon nor any follower.

After wrapper return victory rereads CURRENT_BATTLE, clears400+index,
sets500+index: unchanged40 clears440/sets540, leaving490,387/388 and
drop bits unchanged here. Words d0–d3 clear, PARAM2–5 copy into low
bytes, d4=1. Upper longword halves are not established by clr.w.
Independent selector/request reads can differ under mutation; healing,
seed, scene, join and flags are not transactional.

Defeat first UpdateBattleUnlockedFlag: clear440 **only when540 was
already held**. It clears dialogue-name1, requests sad music/text363/
close, restores Bowie HP=max and logically halves gold as a longword.
GetEgressPositionForBattle (`$23E50..$23EB0`) forces clear only for38/39,
and fixed towns for16/17/13/31/32. Selected40 matches neither: live
EGRESS_MAP goes to GetSavepointForMap. Return d4=−1, no aftermath/join/
new540. The losable battle4 exception does not apply.

Admitted Egress/Angel Wing pays MP/removes item first, requests text,
flash/sound, updates unlock, unwinds the action frame, calls that same
egress helper and returns clr.w d4. It bypasses defeat gold/Bowie
recovery and victory completion. Denied retreat is a separate path;
these calls do not prove admission/payment completion.

| Old state/outcome | Selected40 mutation |
| --- | --- |
|540 clear; defeat/admitted retreat|No forced440 clear here; incoming440 retained. No new490/540.|
|540 held; defeat/admitted retreat|UpdateBattleUnlockedFlag clears440;540 stays held.|
|Victory,540 clear|Scene then join0, clear440, set540.|
|Victory,540 held|Skip scene, still join0, clear440, set540; return was seeded first.|

`code/common/maps/egressinit.asm:GetSavepointForMap` (`$75EC..$764E`)
uses the selected map's four-byte savepoint row, default1,1/Up if absent;
pre399 instead returns initial Bowie's room. BattleLoop sets399, but
changed state at the helper can select the other branch. F64 can
reset raft state. EGRESS_MAP is dynamic save/church/new-game state,
not a fixed40 town. Natural savepoint/transfer completion is Unknown.

## Complete aftermath population and layout

**Confirmed:** `data/battles/entries/battle40/cs_afterbattle.asm`
spans `$4EA94..$4ED0E`,634 bytes. Scene prefix through ce_4ECD6 is
578 bytes:108 top-level commands encoding502 bytes plus76 inline-action
bytes. The table is56 bytes:main header6, six8-byte records, terminator2.
cs_4EAEE is the continuation after conditional Zynk, not another narrative.

Complete prefix: textCursor3065; loadMapFadeIn Map54,10,4;
loadMapEntities ce_4ECD6; InitW Bowie; InitW Peter; Peter13,8/Right;
InitW FOLLOWER_B; FOLLOWER_B15,6/Down; InitW Lemon; Lemon17,8/Left;
jump if F76 clear to cs_4EAEE; otherwise InitW Zynk; Zynk63,62/Down.
InitW installs eas_Init and waits for that alias's pointer to equal
eas_Idle; it is not a global completion fence.

Complete eas_Init is accelFactors0,0; speed32,32; acceleration OFF, OFF;
deceleration OFF, OFF; entityObstructable ON; entityUncollidable OFF;
mapUncollidable OFF; autoFacing ON; orientUp; immersed OFF; size24;
updateSprite; jump eas_Idle. It contains neither motion ON nor an
animation-speed reset. Those incoming fields must not be inferred
from the Init name alone.

| Logical identity | Table X, Y/facing | Record |
| --- | --- | --- |
|main/Bowie0|15,10/Up|live ally0, eas_Idle, physical0|
|Peter7|13,8/Right|ALLY_PETER, eas_Init|
|Lemon28|17,8/Left|ALLY_LEMON, eas_Init|
|128|63,63/Down|MAPSPRITE_OBJECT1, eas_Init|
|129|63,63/Down|MAPSPRITE_OBJECT1, eas_Init|
|130|63,63/Down|MAPSPRITE_OBJECT1, eas_Init|
|131|15,8/Down|MAPSPRITE_ODD_EYE, eas_Init|

csc37 requests out-to-black, falls through csc48, detaches view, loads
tilesets, waits for fade, deactivates services, loads Map54 and reenables/
reactivates plus a VInt wait. Its10,4 is camera, not actor/return position.
LoadMapBlocksAndLayout decodes base, applies held387 then388, item changes
and battle crop0,0. This can replace interrupted region animation with
full held-flag templates. csc42 disables display/services around
InitializeMapEntities/LoadEntityMapsprites, then reenables. Static call
order does not prove actual display/asynchronous completion.

ClearEntities clears49 ordinary records and64 aliases; ordinary followers
allocate first. Map54 uses the full19-row ordinary table, not overworld
Caravan prefix. Each held row allocates; later duplicate aliases win.
FOLLOWER_B31 can bind Astral via74, Oddler via68, then Astral via70.
No matching row leaves alias0, so unguarded31 commands can affect Bowie.
Other ordinary followers remain; five follow-script pointers do not
prove arbitrary simultaneous flag combinations safe.

Peter/Lemon's nonzero aliases from67/71 cause their explicit records
to consume tails without redeclaration or applying positions/scripts.
Later InitW/setPos uses the resolved alias. F76's ordinary row supplies
Zynk26; there is no explicit Zynk table record. With n followers and
p/l each1 when Peter/Lemon needs a new record, effects128–130 and Odd
Eye131 occupy physical n+p+l+1 through n+p+l+4. All are regular sprites
below special threshold240. Fresh combat128–131 instead mean Odd Eye,
Demon Master, Evil Bishop, Dark Gunner. Scene rebuild replaces combat
aliases; effects do not damage those combatants or recruit actors.

GetEntityAddressFromCharacter masks logical ID, subtracts96 for enemy-bit
identities, reads ENTITY_INDEX_LIST and uses32-byte physical stride.
Logical alias, physical slot, mapsprite and combat kind are distinct.
No arbitrary stale-alias safety/fallback is established.

| Ordered copy in cs_4EAEE | Transfer |
| --- | --- |
|setBlocks11,6,11,29,44,0|319 live16-bit words/638 bytes, x11..21/y6..34→x44..54/y0..28, saved rectangle.|
|setBlocks0,52,10,7,13,10|70 words/140 bytes, x0..9/y52..58→x13..22/y10..16; F388 load-template operands, unconditional here.|

csc34 calls live CopyMapBlocks at$FF0000,64-word/128-byte stride,
forward row-major reads/writes retaining flags, then requests both view
planes. Neither copy overlaps its own source. Saving precedes the
second copy; no selected command restores it. Total389 transferred
words/778 bytes is not encoded storage or changed visual tiles.
No SetTerrain, battle terrain write or new387/388 write occurs.
Decoded words/collision/render/DMA finish/template equality stay Unknown.

## Complete actions, dialogue and exit chain

**Confirmed:** after copies,131 gets waited inline
`ac_motion OFF; ac_orientLeft; ac_updateSprite; ac_jump eas_Idle`, ac_end
delimiter; then stopEntity131. ac_motion encodes14; esc14 writes animation
counter−1 for OFF, not velocity/destination reset. stopEntity again
writes−1. This scene really contains motion OFF. eas_Jump instead uses
autoFacing OFF, relative0,−1, wait5, relative0,+1, wait5, relative0,0,
autoFacing ON, jump Idle. Those switches are different.

Next are waited eas_DeactivateAutoFacing on128,129,130, in order,
then nonwaited eas_AnimSpeedx2 on128,129,130, in order. Former clears
FLAGS_B bit6 then Idle; latter sets bit4 then Idle. Sprite update adds2
beside its ordinary counter increment with bit4, unless counter−1;
x2 is not a measured timing ratio. Installation does not prove the first
instruction ran before a later service or overwrite.

Then MUSIC_SAD_THEME_1 (16), fadeInB, csWait60, and eighteen consecutive
requested texts3065–3082 with an ordinary non-skipped cursor. Commands00/02
skip display without incrementing that cursor when debug text skipping
is active; a changed skip state need not retain this later ID sequence.
Every token follows source order below; prose is authored
summary, not original dialogue. Single means nextSingleText; raw byte and
speaker are separate from LEADER/NAME's actual text/name context.

| ID | Mode/raw byte/speaker | Complete token sequence | Authored summary |
| --- | --- | --- | --- |
|3065|nextText/00/131|`{LEADER}{N}{W2}`|Acknowledges defeat and Bowie's growth.|
|3066|single/00/131|`{W1}`|Says he expected it.|
|3067|single/C0/Bowie|`{W1}`|Bowie questions him.|
|3068|nextText/00/131|`{N}{N}{W2}`|Recalls learning the group's strength while travelling.|
|3069|single/00/131|`{N}{W1}`|Misses those travels.|
|3070|single/C0/Peter|`{N}{W1}`|Peter addresses Oddler and questions his intent.|
|3071|single/00/131|`{NAME;7}{N}{N}{W1}`|Insists he fought fully and was surpassed.|
|3072|single/C0/Bowie|`{W1}`|Bowie answers.|
|3073|nextText/00/131|`{N}{N}{W2}`|Regrets recovered memories leading to fighting.|
|3074|single/00/131|`{N}{W1}`|Reflects on former devotion to fighting.|
|3075|single/C0/Peter|`{W1}`|Peter asks him to live.|
|3076|nextText/00/131|`{NAME;7}{N}{W2}`|Thanks Peter for what he learned.|
|3077|single/00/131|`{N}{W1}`|Pain precedes farewell.|
|3078|single/00/131|`{N}{N}{W1}`|Hopes for another life without being a devil.|
|3079|single/00/Peter|`{N}{LEADER}{W1}`|Mourns and addresses Bowie/Astral.|
|3080|single/00/Lemon|`{N}{W1}`|Remembers Oddler's spirit.|
|3081|single/00/FOLLOWER_B|`{N}{W1}`|Redirects blame toward Zeon.|
|3082|single/00/FOLLOWER_B|`{N}{N}{W1}`|Urges onward remembrance.|

Between3069/3070 is Peter's waited eas_Jump; between3074/3075 Peter
shiver; between3077/3078 Bowie nonwaited Up1, Peter waited Right1.
csc2D clears wait timer to physical-slot low byte, masks FLAGS_A with9F,
builds moves in shared action scratch and replaces the script pointer.
EndActions appends jump Idle. Named waits fence only that alias; Peter
does not directly wait for Bowie. Installs14/15 replace scripts too,
without csc2D's collision mask. Idle equality does not prove position
or completion of every actor.

After3078, flashWhite131,30 gives `(30>>2)+1=8` iterations, each
white-bit2/update,2 VInt waits, clear/update,2 waits:32 VInt calls.
Every following row does setPos15,8/Down, then a **nonwaited** move stream
ending endActions, then csWait8. There is no entity-idle wait per row.

| Row | Scene alias | Move |
| --- | --- | --- |
|0|128|Right2|
|1|129|DownRight2|
|2|130|Down2|
|3|128|DownRight2|
|4|129|Right2|
|5|130|DownRight2|
|6|128|Down2|
|7|129|DownRight2|
|8|130|Right2|

Each stream replaces that alias's previous script; setPos rewrites
position/destination, not every velocity/flag. The speed stream may have
completed or been overwritten before its flag write. Movement does not
explicitly clear speed bit or reactivate autoFacing. Eight requested
ticks/dialogue delay/nominal endpoints do not prove effect completion.

Then hide128, wait8, hide129, wait8, hide130; flashWhite131,120 gives31
iterations/124 VInt calls; hide131; SOUND_COMMAND_FADE_OUT FD; wait120;
MUSIC_SAD_THEME_3 (15). HideEntity parks physical position/destination
at$7000,$7000, removes every matching alias and its follower-list entry;
it does not assert former actions completed.

Tail: Peter shiver, faceUp, text3079; Lemon faceDown, text3080; FOLLOWER_B
faceUp, text3081, faceDown, text3082. Lemon waited Left1, Down1; Lemon
faceLeft; FOLLOWER_B waited Down2; Peter waited Down1; Peter faceRight;
wait40; Bowie faceRight; wait40; faceLeft; wait40; faceUp; wait30; Bowie nod.
Finally follow Peter→Bowie, FOLLOWER_B→Peter, Lemon→FOLLOWER_B, each
selector2; csc_end.

csc2C doubles selector2 into table_FollowerPositions byte offset4,
yielding signed−24,0. AddFollower copies42 bytes of eas_Follower1,
patches leader physical index at$1E, offsets at$20/$22, replaces script
and inserts physical follower ID if absent. These are fixed-point
entity offsets (one tile384), not two tiles or synchronous relocation.
No wait follows these installations.

csc19 setPos, csc23 facing, csc1C stopEntity and csc2C follow use the
alive-status helper for ordinary allies0–29; HP0 skips the whole command.
Enemy-bit128–131 and nonally31 bypass it. Installs14/15, csc2D movement,
flash, shiver, nod and hide have no such guard. Victory normally revives
Peter/Lemon, not necessarily Zynk. F76-held/HP0 Zynk gets unguarded InitW
but setPos can skip. Changed HP can suppress facing/follow while an
installed action still runs. Missing FOLLOWER_B can redirect unguarded
commands to physical0; F76 alone is no general alias guarantee.

Explicit csWaits are60, nine8, two8,120,40,40,40,30:418 requested Sleep
ticks. ExecuteMapScript can skip encoded waits with SKIP_CUTSCENE_TEXT,
including debug/P2 START. Named waits, flashes, shivers, nod, text/fade/VInt
add separate work. csc_end conditionally waits for scrolling when a
dialogue window is open, clears view speed and restores registers; it
does not drain all actions. No sum proves elapsed hardware time,
final audiovisual state or naturally usable input.

## Ordinary exploration rebuild and exits

**Confirmed:** BattleLoop returns saved PARAM2–5 in d0–d3, d4=1.
`code/gameflow/mainloop.asm` goes to alt_MainLoopEntry, SwitchMap then
ExplorationLoop, bypassing an immediate CheckBattle. ExplorationLoop
clears MAP_EVENT_TYPE.w, adjusts/clamps STEP_COUNTER, heals, fades,
detaches view and sees supplied54 instead of the−1 retain-entities path.

It sets CURRENT_MAP54, CURRENT_BATTLE=FF, loads tilesets, gets setup
entities, reinitializes ordinary entities/followers/sprites, clears
temporary story flags256–383, sets80, initializes exploration sprite
counters, targets physical0, loads map, installs base services and runs
setup init. It clears neither387/388 nor dropped bits. Conditional
palette/music/fade precedes WaitForEvent/player-action. Static calls
to this interface do not prove naturally controlled/rendered arrival.

Accepted Map54 lookup falls back to ms_Void=FFFF, no authored setup
entities/init. Ordinary follower flags rebuild population at the
**saved return**. Explicit Peter/Lemon records, effect/NPC aliases,
follow chain, script scratch and saved638-byte rectangle are not an
exploration snapshot. Fresh layout decodes base, applies held387
(11,53/13×8→7,18) then388 (0,52/10×7→13,10); it does not restore44,0.
Both held rows apply independently. Clear guard omits its row even if
the prior scene partly changed live tiles. The unconditional aftermath
copy does not set388, so clear388 can yield different reloaded layout.
Decoded equality and save-file persistence are not proved.

Held540 skips scene population/copies/actions/text but still takes the
same preseed, join, flags and ordinary rebuild. No40 table overrides the
return position; scene Bowie15,10 is not a hardcoded victory return.

`data/maps/entries/map54/6-warp-events.asm` (`$B71EE..$B7200`)
contains two8-byte rows plus word terminator:

| Trigger | Declared transfer |
| --- | --- |
|X255 wildcard, Y42|No-scroll Map28,16,16/Down|
|X255 wildcard, Y0|No-scroll Map56,16,48/Up|

The movement caller requires the working-layout warp marker1000 before
calling WarpIfSetAtPoint, which checks authored order and wildcards.
Rows do not prove marked or
reachable decoded tiles. Shared request consumption supplies map/X/Y/
facing; these rows add no540/Creed test. Map56's first reverse row is
Y49→Map54,16,1/Down, not proof of an actual return. Stop at Map56's
arrival/setup interface, before its services, Creed or Battle41.

## Actual compiler and runtime boundaries

**Confirmed about current code:** sole selected lowering completed
in0.1186s, no exception;99,351-byte full receipt retains corpus, programs,
actions/error and unchanged pair size/mtime. abcs_battle40 has17 lowered
instructions, cs_4EAEE has173. Prefix has typed scene-map/population/Init/
positions/branch/fallthrough. cs_4EAEE0/1 are native setBlocks;
ProgramRunner refuses reached0 before advancing it. Named speed installs
share eas_AnimSpeedx2, whose first action is native ac_animSpeedX2.
Inline motion OFF lowers to animation counter−1, follow2 to−24,0,
white flashes to32/124 ticks. No scene executed; lowering success is
neither encounter admission nor scheduling/presentation parity.

| Current owner | Expression/support and selected40 gap |
| --- | --- |
|[preparation/routes](../../src/sf2tool/remake_exploration_battles.py),[program compiler](../../src/sf2tool/remake_exploration_programs.py)|Preparation admits2/3; explicit file registration only enabled this observation. Generic routes select after/defeated/join/configured egress, not dynamic native save/church state. No40 lifecycle/drop binding follows.|
|[map compiler](../../src/sf2tool/remake_exploration_maps.py)|Route-absent non-event setup finally becomes end; conditional387/388 wrappers retain native flag-layout-copy, both clear reaches end. eventMaps54 needs unsupplied setup binding. Generic warps require selected destinations; Map56 is not admitted.|
|[PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs),[PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Pin/region admission1/2/3. Predecessor's Fairy Tear placement refresh, HEALER3/5/SENTRY, MAGE/BRASS_GUNNER, ENHANCED/BIT4 and copied-equipment limits remain. No full-import failure ordering or40 drop support claimed.|
|[BattleOutcomeRules](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs),[Sf2OutcomeReturnPolicy](../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs),[BattleOutcome](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs)|Represented positioned/living actors, leader-loss priority, completed-scene skip, join/flags and staged return exist. Active membership uses First/Single and rejects missing position, not unchecked byte/alias selection. Defeat uses configured egress, refuses399/64/640 variants; nonzero return status refuses refresh. No arbitrary RAM slots or interrupted-prefix parity.|
|[MapEntityAllocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs),[SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs),[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs),[EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Follower-first/duplicate-ally allocation, typed motion/facing/follow/animation exist with validation. Native refuses program-opcode/entity-action before that operation advances. Relative motion outside0..63 refuses; native HP/alias/service interleaving and five-script limits are not established.|
|[WorkingMapLayout](../../remake/src/Sf2.Remake.Domain/Maps/WorkingMapLayout.cs),[ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs),[BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs),[capability owner](../../remake/docs/capability-status.md)|Word-copy/generic projections exist; selected native layout/templates/assets, actual40 drops, population and return lifecycle remain unbound/unadmitted. Primitives do not establish display/audio completion or usable40 input.|

**Inferred:** future content can express independent dropped bits,
inventory transactions, completed guard, staged aliases, mutable layout,
action replacement and ordinary reconstruction. Smallest follow-up is
explicit Content/Domain/Application admission for these gaps, including
sword RNG/nonrare-loss; not an engine repair hidden here. Accepted
runnable content through Battle03 entry is separate from original facts.

## Reproduction and retained limits

Ignored direct recipe records selected tables, ordered terminal/drop/
return uses, complete scene/inline source, tokens, actual consumers and
document/private boundaries. It does not reread pair or repeat lowering.
Use fresh output and same-process private configuration:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1162/review.py <configured-source-disasm-root> --receipt local/issue1162/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

ADR0004 review covers provenance, widths, selectors, polarity, mutation/call
order, aliases, changed valid states, consumer boundaries and publication.
New maintained parser/schema/fixture/helper-test and H3 callback items
are N/A. Normal verify **SKIPPED** by explicit documentation-only Issue;
no full/old comparison, SDK/Godot/emulator/H3/H4/screenshots. Frozen
identity, direct receipt, committed planner, scoped CI, local recipe/read
failures, resource totals and process state belong to handoff. Main-gate
independently reads/reproduces/accepts/merges.

Preserve #1160 first direct02 PASS semantic limit, R3 endpoint/hit-table
self-review corrections/local failures, main-gate direct01 PASS0.1425s/
682,198 bytes and CI39723302467 SUCCESS. Preserve #1158 sole opening
lowering/HP/natives, Map54 final-overwrite correction, earlier autoFacing/
no-clamp/PRIMARY/Mover corrections, Map53 UnboundLocalError/zero partial
and twelve completed old comparison failures. None is rerun, erased,
called interrupted or silently upgraded here.

**Unknown:** natural incoming membership/HP/inventory/aliases/flags/RNG,
actual kills/draws/receipt, interleaved services/partial prefixes, decoded
layout/terrain/collision/template equality, source/ROM equality beyond
accepted evidence, actual entity/camera/render/DMA/VInt/text/audio/input
completion, save persistence, natural savepoint, usable Map54/56 arrival
and playable40 support. Not an automatic acquisition/repair queue.
Freeze a clean pushed Draft PR, notify main-gate; no self-merge, cleanup,
archive or next Issue.
