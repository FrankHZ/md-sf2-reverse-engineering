# Battle15 shrine approach, battle and outcome returns

Evidence date: 2026-10-22. **Confirmed:** Map69 connects to exterior61;
its F77/clear730 zone opens the selected block and unlocks Battle15. Both
Map61 and Ribble8 can request shared Map50, whose wildcard battle row can
intercept either entrance under F415. Fresh battle construction, the entire
before/after programs, conditional combat/control boundaries and three
exploration exits are described below. **Inferred:** the old man's invitation,
shrine battle and failed door attempt form one intended optional excursion.
**Unknown:** naturally traversed connectivity, actual incoming actors/flags,
all tactical decisions, completed presentation/control and save persistence.

## Agreement and provenance

**Standard** rigor for [Issue1200 / Epic1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1200):
one connected static content account supports later design and implementation.
Request, population, battle and caller lifetimes justify explicit boundaries.
Accepted base is `6ea3039fda488f08cb7d1fde3549d00279937793`, with no stacked
dependency. Ownership is this document and Battle15-only references in
[story coverage](./story-content-coverage.md) and [source coverage](./source-coverage.md).
No new parser, fixture, corpus, lowering run, engine implementation or runtime
acquisition is part of this outcome. Complete tactics, tunnel-hub content and
later optional villages are excluded; admissions into adjacent owners remain included.

Reuse the [East Shrine old-man/F77/F729 owner](./battle10-outcomes-through-east-shrine-visit.md)
and [Panel/tree/Achilles Sword custody owner](./ribble-tree-opening-and-achilles-sword.md).
The latter's selected chest/custody and tree claims retain their accepted scope.
[Requests](./map-event-request-consumption.md), [map interactions](./map-event-interaction-state.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[spritesets](./battle-spriteset-data.md), [routing](./battle-routing-data.md),
[global battle data](./battle-global-data.md), [scripting](./common-scripting.md)
and [stats](./common-stats.md) own shared mechanisms. The
[Battle28 account](./battle28-bridge-and-outcomes.md) and
[Battle0 escape/Suspend audit](./battle00-bonus-battle-and-reset.md#outcomes-escape-and-the-actual-reset-return)
provide shared-consumer explanations; their selected battle facts are not
Battle15 evidence. This chapter checks the actual selected source and consumers.

Original paths are relative to `disasm/` in read-only
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The registered USA baseline is SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM equivalence or runtime claim is made. `sf2enums.asm`, `sf2const.asm`,
`sf2macros.asm`, `sf2mapmacros.asm` and `sf2cutscenemacros.asm` own encodings.
Fnnn means flag nnn; ordinals are zero-based; address extents are half-open.
Below, `map/` means `code/common/scripting/map/`; battle-relative code paths
mean `code/gameflow/battle/`. Tile coordinates, internal units (384 per tile),
camera operands and source wait counts are distinct quantities.

The retained map-init/map-script-engine pair totals 18,285,979 bytes, with
existing pin/ROM provenance; it is reused without regeneration. Before/after
source text is 1,827/3,722 bytes and spriteset15 text is 6,179 bytes. Finite
source, consumer and prose reads are linear in those inputs. Each direct check
is budgeted below60s/512 MiB, new receipts below4 MiB; peak memory is unmeasured.
Reassess before a material corpus/tool expansion. The local Issue handoff owns
the complete small review recipe and actual results; no old comparator is replayed.

## Map69 approach and complete exterior61 behavior

**Confirmed:** `map69/6-warp-events.asm` row1 requests no-scroll61 `(5,15)` Up
from `(43,31)`. `map61/6-warp-events.asm` row1 returns from `(5,16)` to69
`(43,31)` Down. Neither row tests F77, F729, F730 or an item. The actual
`WarpIfSetAtPoint` requires its layout/movement admission, scans ordered rows
and treats negative source bytes as wildcards. It posts mutable event1 and
payload/sound in separate writes. The request is not a walked-path receipt.

The no-scroll request consumer idles main, discards the exploration return
address and clears the event before dispatch. Nonnegative destination bytes
replace low coordinate bytes of sampled main words; they do not globally
clear supplied high bytes. Ordinary nonnegative samples give the rows above.
`code/gameflow/mainloop.asm:@Start` orders **SwitchMap then CheckBattle**.
SwitchMap has only3/F640→4,71/F530→78,58/F999→59; neither61 nor69 is switched.
The battle-coordinate table has no61 row, so61 enters fresh exploration.
On the reverse request,69 can instead admit its earlier row9/F409 or row17/F417
in table order; these are the accepted [Ribble](./new-granseal-departure-through-battle09.md)
and [Hassan/17](./hassan-departure-and-battle17-outcomes.md) boundaries, not
an unconditional exploration return. Map69's separate walking/random zone
producer does not automatically run just because this warp selected69.

`data/maps/mapsetups.asm` selects `ms_map61`, overridden by F729. Both pointer
tables share entity events, zones, descriptions, item default and init. Only
fixed population changes:

| Live setup | Fixed population after ordinary follower construction |
| --- | --- |
|729 clear|Young-man sprite at `(5,8)` Up, `eas_RightLeftLoop`; logical128.|
|729 held|`ms_map61_flag729_Entities` is terminator-only; no fixed Paseran.|

`data/maps/mapsetupsstorage.asm` includes default `s1_entities.asm` immediately
before `s1_entities_729.asm`; the latter's `msEntitiesEnd` also terminates the
former's single row. Absence of an end macro in the default file does not
mean an unbounded list. `data/maps/entries.asm` explicitly binds Map61's flag,
step, roof, warp, chest and other-item tables. Step/roof/chest/other-item are
terminator-only. Descriptions and init are RTS; item section has only its RTS
default. There is no selected description/item/init scene hiding in another
Map61 setup, and no chest award on the exterior.

`s2_entityevents.asm` has one explicit event:128/Down → `Map61_EntityEvent0`;
default RTS. The interaction dispatcher supplies the active actor and facing,
restores prior facing, closes windows and reactivates updates afterward.
The complete short conversation is:

| Text ID / hex | Condition and public paraphrase | Ordered tokens |
| --- | --- | --- |
|3645 /0E3D|Only clear256: introduces Paseran of Ribble and petroglyph studies.|N,N,W2|
|3646 /0E3E|Always: reports character11's account of a family's ancient tomb.|NAME11,N,N,W2|
|3647 /0E3F|Always: locates the supposed house nearby according to his research.|N,N,W2|
|3648 /0E40|Always: considers an eastern location instead.|W1|

After those calls return, set256 and RTS. No Yes/No, F77 producer, warp,
item transaction or recruitment occurs. Fresh exploration clears temporary
flags256–383 (start256, DBF counter127); repeat talk in the same loaded state
skips only3645. F729's population choice and this temporary flag are independent.
Dialogue is a character's report, not independent proof of tomb location.

The [old-man owner](./battle10-outcomes-through-east-shrine-visit.md#current-following-versus-the-f77-rebuild)
already establishes Map62's transient129→31 follow, subsequent F77, then
caller F729. Here fixed128/Paseran is neither that129 nor follower30. F77
rebuilds the old-man sprite as FOLLOWER_A=30. FOLLOWER_B=31 can be overwritten
in follower-table order by F74/Astral, F68/Oddler, then F70/Astral. A can also
be overwritten by later F83/king; no live identity is guaranteed by a flag
alone. Map61/50 are not overworld Caravan-prepend maps. Under only F67/F68/F77,
Peter7, Oddler31 and old man30 occupy physical1/2/3; main/Bowie is physical0,
and clear729's Paseran128 is physical4. Different enabled rows change slots.
Missing logical aliases resolve cleared lookup0 without a presence guard;
conflicting flags retain last-write lookup and follower-capacity limitations.

## Door zone, opening and partial-state ordering

**Confirmed:** `s3_zoneevents.asm` rows `(4,9)`, `(5,9)`, `(6,9)` all target
`Map61_ZoneEvent0`, followed by default RTS. Actual zone admission requires
the layout-kind `$1400`/event6 path, not merely a map load or coordinate.
The handler tests730 first: held → RTS; otherwise tests77: clear → RTS;
otherwise `script cs_5C6CA`, **then** set730 after return.

The complete `scripts.asm:cs_5C6CA`, `$5C6CA..$5C76A`, is sequential
(29 retained command records including end):

| Phase | Complete ordered work |
| --- | --- |
|Staging|Cursor3642; Init! Peter,A,B; destination Bowie4,9; face Bowie Up; destination Peter6,9; face Peter Up; camera0,5; destination A5,7; destination B5,9; face B Up,A Down.|
|Opening|3642 S/A: takes responsibility; face A Up;3643 S/A: chants; white-flash A40; battlefield-death sound; quake2; copy block `(11,0)` size1×1 to `(5,6)`; wait30; quake0; face A Down;3644 S/A: asks others to enter first.|
|Unlock/follow|setStoryFlag15 (F415); follow Peter→Bowie selector2; B→Peter selector2; A→B selector2; end. Caller then sets730.|

`!` denotes waited action installation; A/B mean logical30/31. All three
texts use control `$00`, `nextSingleText` (S), and only W1. Text keys0E3A–0E3C
were read in `data/scripting/text/gamescript.txt`; wording above is paraphrase.
S closes portrait/text and sleeps10 after display; debug skip can suppress
display/cursor advance, without creating a new dialogue branch.

`setDest` is csc29, **not setPos**: resolve the word entity, install destination
words scaled by384, travel magnitudes and signed velocity32; unless bit15 is
set, wait for that entity's position/destination equality. These operands are
unflagged and waited. It neither teleports nor checks HP nor installs Init.
Thus Bowie gets a destination without an explicit Init here. Action-install
waits instead poll script pointer for `eas_Idle`; neither type is an all-actor
or unobstructed-path guarantee. eas_Init resets speeds32/collision/auto-facing/
size24 and sprite state. Packed setFacing/setPos guard actual allies0–29 by
their own HP; A/B bypass that ally guard. Text/action/white-flash have separate
consumers and no equivalent presence guarantee.

`csc18_flashEntityWhite` shifts40 right2 then DBF:11 cycles/four VInt waits,
44 explicit waits;50 later gives13 cycles/52 waits. It updates the resolved
entity's white bit/sprite, not a global screen flash. Quake2/0 writes amplitude;
scroll finalization consumes it separately. csc34 copies whole working-layout
words and requests plane updates; attributes move with the block. Camera
destinations wait for scroll. Source waits do not prove visible duration or
service completion; debug text-skip can also omit Sleep commands.

Every follow above and after battle encodes **word** operands. csc2C probes
`move.b (a6),d0` before consuming the first word; selected follower words
`$0007/$001F/$001E` all have high byte0, so each checks **Bowie's HP** through
AdjustScriptPointerByCharacterAliveStatus. Dead Bowie skips that follow's six
operand bytes, not later flags/end. Living Bowie does not validate Peter,
A, B or their targets. With the guard allowing it, selector2 resolves offset
`(-24,0)`; `AddFollower` installs copied42-byte following actions, patches
physical target/offsets, advances allocation and appends unless already listed.
Duplicate-list avoidance occurs after allocation/action replacement. No join,
F77 write, capacity proof or wait for settled following occurs in these follows.

| Changed/interrupted state | Static result |
| --- | --- |
|730 held,77 clear or held|No opening/unlock/follow; no repair of415 or blocks.|
|730 clear,77 clear|No work, even if729 or415 is held.|
|730 clear,77 held|Full script; no729/party/item/completion prerequisite. Stale aliases remain unchecked.|
|Interrupted before415|Earlier destinations/flash/quake/block can differ; caller730 remains unwritten.|
|After415, before follows/end/caller|Unlock may survive without following/730; no rollback.|
|Dead Bowie with returning services|Follow calls can skip;415 and caller730 still set.|
|Fresh61 load with730 held|`3-flag-events.asm` reapplies `(11,0)`1×1→`(5,6)` after base layout load, independently of77/415.|
|Supplied730 clear with open working block|Callback repeats; no block-state idempotence check.|

`RunMapSetupZoneEvent` closes windows, waits a VInt and main's stop before
return. `WaitForEvent` with pending0 restores view target0 and installs
controlled actions; mutable pending, player mode and services still matter.
Flag save-domain membership and fresh-load block application are static facts,
not proof of actual save/load, physical passage or usable control.

## Both Map50 admissions and shared storage

**Confirmed:** Map61's first warp `(5,6)` requests50 `(14,22)` Up; Map8's
under-tree row `(255,20)` requests50 `(42,23)` Up. Neither warp row itself
tests730/731/415 or Panel custody. Layout/movement still admits the request;
the accepted [tree owner](./ribble-tree-opening-and-achilles-sword.md) owns
F731 and Panel removal rather than this chapter silently extending it.

CheckBattle's sole50 row is15: `50,0,0,32,24,255,255`. F415 must be held.
Both trigger bytes are wildcard; it **does not test crop containment**.
Thus even42,23 can admit15 under415. A match sets crop0,0/32×24 and D7=15;
held515 clears415 but still admits on this call. Clear415 gives D7=-1 and
restores incoming map50 for exploration. F730, F731 and F465 are not this
predicate. A selected battle is not initialized until MainLoop calls BattleLoop.

The complete map-setup table has no50 entry. The consumer reaches `ms_Void`
and skips setup init; this is not an implicit populated default. Ordinary
shared main/followers and map tables still operate. `entries.asm` binds50's
two areas, flag/step/roof/warp/chest/other-item/animation sections. Area0 is
`(0,0)..(31,23)`, type0, secondary foreground0,24; area1 is
`(33,0)..(54,24)`, type255; both select shrine music, unity parallax and zero
autoscroll. Battle crop copies32×24=768 words/1,536 bytes in64-word layout
rows, with max bounds31×384 by23×384. These are storage/load facts, not proof
that distinct regions connect by walking. The scene camera and terrain
pointer15/`BattleTerrain15` are separate selectors; no terrain/art extraction occurs.

Map50 chest rows are `(2,8),151,POWER_WATER`; `(14,2),152,WOODEN_PANEL`;
`(35,6),147,ACHILLES_SWORD`, then end. Other-item, step and roof tables are
terminators. Panel/Sword eligibility, capacity, inventory search/removal and
Rohde's possession predicate remain with their existing owner. The selected
Power Water chest has a different **battle trap** path below; listing it is
not a second custody proof or an all-map chest replay.

## Fresh battle, hooks and complete before program

**Confirmed:** `battleloop_1.asm` clears PLAYER_TYPE, sets399, tests88.
Fresh clear88 clears seconds, saves incoming map/battle words, assigns50/15
and runs before; restores/reassigns incoming selectors; clears90–105; heals
eligible allies; initializes actual ally/enemy positions; clears AI memory;
LoadBattle for0; start wrapper; then activation/spawn/queue prefix.
Each suffix requires earlier calls to return; crop is not recalculated here.

| Actual table/consumer under `data/battles/` | Selected15 meaning |
| --- | --- |
|`cutscenes/beforebattlecutscenes.asm`|`bbcs_15`; wrapper checks465, does not set it.|
|`cutscenes/battlestartcutscenes.asm`|`ms_Empty`; wrapper checks and sets465 **before** dispatch.|
|`cutscenes/battleendcutscenes.asm`|`ms_Empty`; Bowie/enemy128 HP gates and leader tail remain.|
|`cutscenes/regionactivatedcutscenes.asm`|Only32/0,32/1,40/0,40/1; no15 program.|
|`cutscenes/afterbattlecutscenes.asm`|`abcs_battle15`, checked515.|
|`cutscenes/afterbattlejoins.asm`|Byte15=0; actual shared tail calls JoinForce(Bowie0).|
|`global/enemyleaderpresence.asm`|Byte15=-1: nonzero shared leader-death behavior.|
|`global/afterbattlepositions.asm`, `global/battleneutralentities.asm`|No15 override or neutral group.|

Held465 skips before/start scripts, not fresh roster/healing/load. Empty is
the real `$FFFF` map-script terminator, not an absent wrapper. Script load
resets text-skip/loading state; end can wait for scroll and clears camera speed.

`bbcs_15` is `$4ADA6..$4AE42` (27 retained command records including end),
followed by `ce_4AE42` through `$4AE72`:

| Phase | Complete ordered commands |
| --- | --- |
|Load/stage|Cursor2503; loadMapFadeIn50,9,15; load ce_4AE42; Init! Bowie,A; position A14,19 Up; Init! B; position B15,20 Up; Init! Peter; position Peter13,20 Up; fadeInB; cameraSpeed48.|
|Monsters|2503 S/A; A U1!; camera9,4; wait30; Jump128,129,130,131 un-waited then Jump!132; wait30; camera9,15.|
|Request battle|2504 S/A; face A Down;2505 S/A; end.|

Scene load csc37→48 loads graphics/layout with detached camera at scaled view
coordinates, then reactivates services/waits one VInt; explicit fadeInB is
separate. It does not run ExplorationLoop's temporary reset/healing/setup init.
csc42 reconstructs entities: flag followers first, fixed rows next, main last
at physical0, clearing previous actors/lookups/walking scratch. Main is
`(14,20)` Up. The five fixed NPC rows are logical128 Golem `(14,7)` Down;
129 Gargoyle `(15,6)` Down;130 Master Mage `(13,6)` Down;131 Black Monk
`(16,7)` Down;132 Arrow Launcher `(12,7)` Down, all eas_Init. With N flag
followers they occupy physical N+1..N+5. None is the matching combatant ID
merely because its script alias is128–132. The scene does not declare Peter,
A or B; actual follower flags/mappings are required. Before/after rebuilding
and later battle population are separate mapping lifetimes.

## Formation, definitions and hidden Golem

**Confirmed:** `data/battles/spritesets/entries.asm` selects `BattleSpriteset15`,
`$1B416C..$1B42DA`,366 bytes: header4, allies144, enemies192, regions24,
point2. Header counts12/16/2/1. Twelve-byte formation rows all declare
HEALER1,NOTHING,NONE/15,NONE/15,0/STARTING; only ordinal/X/Y differs:

| Ordinal | X,Y | Ordinal | X,Y |
| --- | --- | --- | --- |
|0|14,20|6|12,21|
|1|13,21|7|16,21|
|2|15,21|8|16,20|
|3|13,20|9|12,20|
|4|15,20|10|14,22|
|5|14,21|11|11,21|

`InitializeAllAlliesBattlePositions` uses actual joined/active lists in ally
ID order and nonzero HP, not these ordinals as fixed named allies. Dead
processed members do not advance the placement pointer; surplus beyond12
leave the party without immediate list refresh. The zero-active-list hazard
is not repaired here. Healing scans all30: living members and immortal
Peter7/Lemon28 at0 heal to maxHP/MP, regardless of membership; other dead
allies stay dead. Status AND7 retains initial STUN/POISON/CURSE, followed by
equipped-curse recomputation. Formation AI/items do not overwrite ally state.

All enemy secondary orders/regions are NONE/15 and filler0. Types below
resolve through `sf2enums.asm` to definitions, not scene sprite IDs:

| Row / combatant | Type / definition | X,Y | Commandset | Primary / region | Spawn / extra item |
| --- | --- | --- | --- | --- | --- |
|0 /128|Arrow Launcher60|14,5|LEADER14|MOVE_TO\|0 /1|STARTING /Nothing|
|1 /129|Golem9|13,4|ATTACKER3=9|FOLLOW_ENEMY\|0 /1|STARTING /Nothing|
|2 /130|Gargoyle43|16,5|ATTACKER3|FOLLOW_ENEMY\|0 /1|STARTING /Nothing|
|3 /131|Master Mage69|5,3|ATTACKER1=6|NONE /0|STARTING /Nothing|
|4 /132|Black Monk78|7,4|HEALER3=2|FOLLOW_ENEMY\|3 /0|STARTING /Nothing|
|5 /133|Golem9|8,9|ATTACKER1|FOLLOW_TARGET\|0 /0|STARTING /Nothing|
|6 /134|Gargoyle43|2,10|ATTACKER1|NONE /0|STARTING /Nothing|
|7 /135|Master Mage69|21,5|ATTACKER1|NONE /0|STARTING /Nothing|
|8 /136|Black Monk78|21,9|HEALER3|FOLLOW_ENEMY\|9 /0|STARTING /Nothing|
|9 /137|Golem9|22,10|ATTACKER1|FOLLOW_TARGET\|0 /0|STARTING /Nothing|
|10 /138|Golem9|21,17|ATTACKER1|NONE /15|STARTING /Nothing|
|11 /139|Golem9|12,12|ATTACKER1|NONE /15|STARTING /Nothing|
|12 /140|Gargoyle43|7,15|ATTACKER1|NONE /15|STARTING /Nothing|
|13 /141|Gargoyle43|26,15|ATTACKER1|NONE /0|STARTING /Nothing|
|14 /142|Arrow Launcher60|21,13|ATTACKER1|NONE /15|STARTING /Nothing|
|15 /143|Golem9|2,8|ATTACKER1|NONE /15|HIDDEN2 /Power Water|

InitializeEnemyBattlePosition initializes rows128–142, but143 gets
activation0200/maxHP0/currentHP0/X=-1 and trigger15/15 without copying Golem
stats/items. Unused144–159 are likewise offscreen/HP0; do not infer their Y
or stale fields were all cleared. No15 neutral group is declared. LoadBattle
reconstructs mapped actors from nonnegative X/Y, so ordinarily there are
fifteen initial enemy actors, not sixteen scene monsters. With L placed
allies these occupy physical L..L+14; actual membership/state determines L.

`InitializeEnemyStats` copies each56-byte definition, fills current HP/MP,
positions/type/commandset/orders/triggers, applies extra item, activation mask,
difficulty and stat refresh. Definitions in `data/stats/enemies/enemydefs.asm`:

| Definition | LV | HP/MP | Base ATT/DEF/AGI/MOV | Movement | Equipment / spells |
| --- | --- | --- | --- | --- | --- |
|Golem9|12|24/0|28/24/19/4|REGULAR|none /none|
|Gargoyle43|15|29/0|31/23/21/6|FLYING|none /none|
|Arrow Launcher60|17|33/0|15/24/23/4|BRASS_GUNNER|Steel Arrow equipped /none|
|Master Mage69|14|28/14|11/18/22/5|MAGE|Iron Rod equipped /Blaze2|
|Black Monk78|14|30/19|27/18/21/6|HEALER|none /Heal2,Blast1|

All use REGULAR spell power, initialStatus NONE, four item/four spell slots;
unlisted slots are Nothing. Definition priority is2 except Monk3; low BIT4/5
are masked away during fresh initialization, giving2000 (Monks3000).
Golem resistance is LIGHTNING_MINOR|ICE_MAJOR|FIRE_WEAKNESS|STATUS_IMMUNITY;
Launcher LIGHTNING_WEAKNESS|STATUS_MINOR; Mage FIRE_MINOR; others0. Golem
critical150 rate1/8, Monk150 rate1/4/double1/16/counter1/16; other critical150
rates1/32, with double/counter1/32 except Monk as stated. These are declarations,
not performed actions. Nothing placement skips item insertion and retains
definition equipment. The actual consecutive BEQs in difficulty adjustment
apply floor(baseATT×5/4) at every difficulty and again at SUPER; equipment
refresh follows. No15 random-upgrade list row exists, so held515 does not
upgrade these definitions. No damage or strategy follows from base stats alone.

### Chest-triggered spawn and Power Water drop

**Confirmed:** `battleloop/populateenemyspawns.asm`'s hidden mode requires
dead currentHP, a held non15 primary/secondary region, and maxHP0. Row143's
15/15 gives neither trigger, so ordinary fresh round-prefix spawning does
not reveal it. Initial target-list length is0; stale target bytes are ignored.

The alternative `battlefunctions/battlefunctions_2.asm:@ExamineContents`
opens the admitted chest first, requests403, rejects Nothing contents, then
computes the facing search coordinate and calls `battlefield/checkfortrappedchest.asm`.
That scans selected placement coordinates; `(2,8)` matches row15, then
requires **entire activation word0200**, both triggers15, and maxHP0. It calls
ResetSpawningEnemyStats and returns143. The caller tests only d0!=-1, **not
carry**, sets TRAPPED_CHEST action/item-or-spell143, requests corrupted-save
music, and SpawnEnemySkipCamera before ending control. This bypasses ordinary
chest AddItem/full-hands logic; F151/open layout is already written.

Reset checks actual row bound144 and all-combatant occupancy, including
dead/self coordinates. Success copies Golem24HP, adds Power Water to its empty
inventory and restores saved activation0200. Failure sets max/currentHP0/X=-1
and carry, but does not change returned143 into-1; the caller can still request
spawn. This is a static failure boundary, not proof of natural failure or an
automatic fix. With unchanged successful143 now maxHP24, the trap predicate
cannot pass again; later ordinary death cleanup does not reset maxHP to0.
Changed activation bits can also defeat the exact0200 predicate. Fresh battle
initialization resets143 independently of already-open F151, which can prevent
another nonempty search. No universal rearm/respawn guarantee follows.

`global/enemyitemdrops.asm` explicitly matches battle15/enemy row15→143/
Power Water/droppedFlag29. `battleactions/dropenemyitem.asm` requires an ally
actor, enemy target, targetDies, matching table row and matching held item.
Power Water is outside the three special random-drop IDs, so no1/32 roll
here. It sets/tests dropped bit29 **before** RemoveItemBySlot and actor receipt;
an already-set bit suppresses delivery. Living actor with capacity receives
the item/messages; dead/full recipient reaches the rare-item test. Power Water
is CONSUMABLE without RARE in `data/stats/items/itemdefs.asm`, so no deals entry
is added and the removed item is not received. Cleanup, leader sweep, poison
or victory is not an alternate call to this action-side transaction. Chest151 and dropped29 are
different flags; hidden declaration, successful spawn and item receipt are
three distinct claims. No new acquisition is observed here.

## Regions, first queue and actual control boundary

**Confirmed:** two12-byte type4 region records have second/trailing bytes0:
region0/F90 vertices `(0,0),(0,14),(31,14),(31,0)`; region1/F91
`(0,0),(0,9),(31,9),(31,0)`. Point0 is `(14,4)`.
`battleloop/triggerregions.asm` tests triangles1/2/4 then3/2/4 using signed
bytes and integer dominant-axis projection/truncation, scanning30 allies for
positive signed HP and nonnegative X/Y, **not membership**. For the declared
formation X11..16/Y20..22, all vertices/edge projections are at or above Y14
or9; neither triangle admits any slot. This uses the integer consumer, not
an assumed floating polygon or terrain path.

Fresh init clears90–105 but not NEWLY_TRIGGERED_BATTLE_REGIONS. ActivateEnemies
scans128–159 and falls through once for160; each eligible X≥0/HP≠0/enemy
entry checks unexamined regions, marking checked bits even on misses. With
unchanged fresh placements F90/F91 remain clear and checked word becomes
incoming OR3. Changed live placed allies, including nonmembers, can trigger;
already checked bits can skip geometry. The later non-swarm AI entry clears
the checked word. The accepted [extra160 RAM-alias boundary](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
remains; no fresh region hit here enables its conditional deals effect.

Rows0–2 use primary1, rows3–9/13 use0, others15; all secondary15. Held primary
sets activation bit0, preserving other bits; a changed held secondary can
set bits0/1. Fresh rows retain2000/3000; hidden143 stays0200. Fifteen initial
enemies are eligible for queue generation regardless of active-region bit.
Their AGIs in row order are23,19,21,22,21,19,21,22,21,19,19,19,21,21,23.

MOVE_TO|0 encodes40, indexing point0 `(14,4)`. FOLLOW_ENEMY|0 encodes80,
targeting combatant128's live coordinates; |3→131 and |9→137. FOLLOW_TARGET|0
encodes00, targeting **Bowie0**, not point0 or enemy128. NONE isFF. Actual
GetMoveOrderPosition selects point by bit6, otherwise encoded combatant;
StartAiControl can fall back to secondary when a followed target HP is0.
These identities do not prove a performed move or target selection.

GenerateBattleTurnOrder resets64 two-byte records toFFFF; offers allies0–29
then enemies128–157, requiring X≥0/HP≠0, not Y/membership/neutral/activation.
Hidden143HP0 is excluded until actually spawned. None of the fifteen base
enemy AGIs doubles. Shared score uses current AGI and advancing RNG, stores
low bytes and signed-sorts with append-order ties; actual allies may add
double records. Therefore fresh records are15+N+T (ordinary12 slots at most39),
not a fixed first actor. Changed roster/stats/capacity retain shared limits.

Cursor0's nonFF reaches ExecuteIndividualTurn; FF repeats prefix. The actual
body clears death-list length, exits HP0, sets actor/camera/cursor state, then
MUDDLE/AI_CONTROLLED, enemy/control-opponent and ally/auto-battle choose AI.
Player path additionally waits view/cursor, builds windows/movement and can
lose action to Sleep/Stun before later dispatch. This is a conditional control
entry, not a healthy named ally, completed UI or first executed action.

Non-swarm AI treats15/15 rows as eligible without region activation; other
listed rows standby/STAY until active bit0. Selected command streams are
LEADER: MOVE_ORDER5,ATTACK2,HEAL3,SUPPORT,MOVE_ORDER2,MOVE3,STAY;
ATTACKER3: MOVE_ORDER3,ATTACK3,HEAL2,SUPPORT,MOVE_ORDER4,MOVE2,STAY;
ATTACKER1: ATTACK1,HEAL1,SUPPORT,MOVE1,STAY;
HEALER3: HEAL2,MOVE_ORDER4,SUPPORT,ATTACK3,MOVE2,STAY.
`global/aicommandsets.asm` and actual StartAiControl/ExecuteAiCommand stop at
the first success; later rounds repeat shared processing. This binds the
decision entry, not every strategy, spell, target, move or action outcome.

Held88 instead restores seconds, clears88/AI memory/view target, LoadBattle
with incoming D0 and jumps to the supplied individual-turn cursor. It does
not assign fresh50/15, heal/place/reset regions/run hooks/generate queue.
Incoming50 indexes lookup50 for the view (enemy146), not Bowie; actual saved
selectors/cursor remain inputs. Physical reconstruction can mutate state.
Valid saved state and naturally completed Suspend/resume are not proved.

## Terminal, victory, retreat and normal Suspend

**Confirmed:** after each returned individual turn, defeated hook → death
processing → counts; zero allies loses before zero enemies wins. If neither,
actual actor's status effects → death processing → ordered counts → cursor+2.
No second defeated hook runs after status effects. Counts require X≥0/HP≠0,
ignore Y/membership/neutral, and BowieHP0 forces ally count0. Simultaneous
exhaustion loses. Debug kill-all is a separate input branch.

The defeated wrapper requires BowieHP≠0/enemy128HP0. Clear515 runs empty15;
held515 skips that script, but both reach leader-presence byte-1. The shared
tail appends all nonzero-HP128–159 to the existing death list, without
coordinates, neutrality, deduplication or resetting length/HP. Here leader128
is Arrow Launcher, not the scene Golem/old man. ProcessKilledCombatants moves
positions offscreen/status0 and records kills/defeats; it neither zeroes HP
nor performs drops. Leader death can therefore clear remaining placed enemies;
all-enemy exhaustion also wins. Hidden143HP0 does not prevent victory, and a
leader sweep is not proof of receiving its Power Water.

Victory orders eligible healing → member-derived return seed → after/join
wrapper → clear415/set515 → D4=1. The seed writes CURRENT_MAP, first actual
active member's X/Y plus crop origin as bytes, mapped facing and PARAM1=0;
there is no empty-list/HP/mapping validation here. Clear515 runs the full
after15; its explicit50 warp replaces the seed with50/27/11/Up and posts
event1. Held515 skips it and retains the member-derived seed. Both then
JoinForce(0): set F0/rebuild force lists, set F32 only if active count<12;
do not clear existing F32 at capacity, heal anew or recompute the seed.
BattleLoop rereads CURRENT_BATTLE for completion writes and PARAM2–5 for
returned D0–D3 words; changed live selectors/payloads need not be atomic.
F465 remains supplied. Interrupted suffixes retain prior writes.

| Route for unchanged15 | Actual boundary |
| --- | --- |
|Defeat|UpdateBattleUnlockedFlag; sad music/text363/close; BowieHP=max; unsigned gold>>1; GetEgressPositionForBattle; D4=-1. No04 losable-battle override.|
|Admitted Egress|Selected learned spell/confirmed target/MP≥8; debit MP, text274, cast/flash/close/unlock and unwind individual-turn frame; egress helper; D4.w=0. No local Silence rejection at the special branch.|
|Admitted Angel Wing|Held selected usable consumable4/confirmed target; remove selected item, common escape presentation/unlock/unwind/helper; D4.w=0. No half-gold charge.|
|Cancel/illegal target/lowMP or earlier Sleep/Stun|No admitted escape; handler entry alone does not establish legality.|
|Normal Suspend|15 passes the battle0 pre-prompt exclusion. Text0/YesNo; negative result returns menu. Nonnegative saves seconds, sets88, calls SaveGame; ordinary fade/sound→WitchSuspend. Debug+Start clears88 and returns menu **after save**. No victory/after/completion/egress return.|

UpdateBattleUnlockedFlag leaves415 supplied with515 clear, clears415 with515
held; defeat/retreat do not newly set515 or change465. Prior items/combat
writes remain. No15 egress special case exists: actual EGRESS_MAP selects
GetSavepointForMap, not the victory seed or stored old X/Y. Under399 held,
Ribble8 gives `(22,22)` Up; other supplied maps use their row/fallback.
Unmatched50/61 retain map with default1,1 Up. Clear399 instead gives3/56/3/Down.
Held64 resets raft fields by the shared map-dependent helper; clear64 keeps
them. No church visit, save durability or eventual town service is implied.

## Complete after program and dialogue

**Confirmed:** `abcs_battle15`, `$4AE72..$4AFB6`, followed by main-only
`ce_4AFB6` through `$4AFBE`, is the whole ordered program
(58 retained command records including end; inline action bodies are separate):

| Phase | Complete ordered commands |
| --- | --- |
|Thanks/invitation|Cursor2506; loadMapFadeIn50,9,10; ce_4AFB6 main14,5 Down; Init! Bowie,Peter,A,B; position A14,19 Up; fadeInB; cameraSpeed48; A U5!;2506 S/A; face A Right;2507 S/A; A R2,U2,R6!; fadeOutB.|
|Door attempt|Position Bowie27,10 Up; Peter28,10 Up; B26,10 Up; A27,7 Up; camera21,5; fadeInB;2508 T/A;2509 S/A; flash A40; wait60; Jump! A;2510 S/A; flash A50; quake1; wait20; quake0; wait90;2511 S/A.|
|Departure|Inline A speed16,16→Idle!; A D1!;2512 S/A; A D2 un-waited; wait15; Bowie D1 and Peter D1 un-waited; B D1!; face Bowie/Peter/B Up; waitIdle A; A L3!; face Bowie/Peter/B Left; A L5!; hide A.|
|Persistent suffix/return|clear77; set732; follow Peter→Bowie selector2; B→Peter selector2; warp explicit50,27,11,Up; end.|

The scene list has no fixed enemy/old-man/Peter/B row: it rebuilds flag
followers and main, then uses their actual aliases. The same ally HP and
word-high-byte follow guards described above apply. Hide moves A to7000,7000,
removes matching lookup aliases/follower-list entry; it is not HP death or
clearing F77. The subsequent clear77 and set732 are separate persistent writes.
Interruption after hide can retain77; after clear77/before732 the old-man
rebuild condition differs without the completed-scene flag. Future population
reads77; no new old-man force member or reward is joined in this script.

No block copy or successful tunnel-opening write appears in after15. The
door remains a dialogue/presentation episode; the F999 block rule below is
independent. An explicit return warp follows both failed attempts and exit
movement, regardless of whether actual allies received all placement writes.
Following is not an all-actor completion barrier.

All text sites were checked against game-script hex keys. `$00` is the
speaker-control operand throughout; A is logical30. T=nextText; S=nextSingleText.
Nominal cursors advance2503→2506 before and2506→2513 after with skip clear:

| ID / hex / form | Speaker | Public paraphrase | Ordered tokens |
| --- | --- | --- | --- |
|2503 /09C7 /S|A|Identifies the hall as leading to an ancient tunnel.|N,N,W1|
|2504 /09C8 /S|A|Notices further monsters.|W2|
|2505 /09C9 /S|A|Calls on the leader to defeat them.|LEADER,W1|
|2506 /09CA /S|A|Thanks the party for the meeting/help.|N,W1|
|2507 /09CB /S|A|Invites them toward the tunnel.|N,W1|
|2508 /09CC /T|A|Questions the closed door.|N,W2|
|2509 /09CD /S|A|Proposes entering anyway.|W1|
|2510 /09CE /S|A|Attempts another try.|W1|
|2511 /09CF /S|A|Attributes the lock to grandson Petro.|N,W1|
|2512 /09D0 /S|A|Says this route cannot take them to Grans.|N,W1|

Those statements identify the intended old-man role; actual mapping still
depends on flags. Petro, the lock's cause and Grans accessibility are spoken
claims, not independently established world lore or a new route guarantee.

## Actual return caller and three exploration exits

**Confirmed:** this battle's caller is MainLoop, unlike Battle0's EndGame
reset caller. After BattleLoop returns, MainLoop calls SwitchMap then
ExplorationLoop **without CheckBattle**. Explicit50 from first victory starts
fresh exploration; held515's unchanged seed50 likewise starts fresh, not
CURRENT255's retained arm. Defeat/escape instead use actual egress results.
No new source-to-next-battle implication follows merely from D4.

Fresh ExplorationLoop heals eligible allies, makes current battle NOT_CURRENTLY_IN_BATTLE,
resets temporary flags, rebuilds followers/main, selects void50 setup and
loads base layout/flag copies. Full load applies `map50/3-flag-events.asm`:
held999 copies `(32,0)`1×1→`(27,6)`; clear999 leaves base layout. F732/515
do not select that block. Retained CURRENT exploration can preserve working
layout instead; no unconditional fresh/retained persistence claim is made.

All three `map50/6-warp-events.asm` records are no-scroll:

| Row / source | Destination request | Actual next admission boundary |
| --- | --- | --- |
|0 /(27,3)|63 `(8,51)` Right|SwitchMap has no63 replacement. Row0/F400 can admit hidden Battle0 regardless of crop trigger coordinates; otherwise fresh63 setup (F29 override) and its selected init. This does not prove walking through the door, F999 state or the tunnel network.|
|1 /(14,23)|61 `(5,7)` Down|No61 battle row; fresh61 setup by729, follower population by77 and open block by730 remain independent.|
|2 /(42,24)|8 `(16,20)` Down|No8 battle row; fresh Ribble default/F708 setup and F731 tree block from the accepted Panel/tree owner.|

Coordinates/facing pass through the same low-byte request rules. Map50's
regions, block changes and these records do not prove continuous walkable
connectivity among the three exits/chests. For63, its init checks29 and can
run the short Claude pose program; no full hub conversation/recruitment or
later village research is undertaken. [Battle0's owner](./battle00-bonus-battle-and-reset.md)
owns the separate F400 admission/reset chapter. Returning61 can later exit69
as above; the chapter stops at these admissions and existing owners.

## Actual producer and remake consumers

**Confirmed by code inspection at the accepted base:** structural expression,
conversion, admission and execution are distinct. No selected15 lowering or
engine run was performed; none was needed to answer these code-bound questions.

| Surface | Actual expression / limitation for this chapter |
| --- | --- |
|H2 producers|`src/sf2tool/h2/map_init.py` and `map_script_engine.py`, plus map setup/event/data, battle spriteset/routing/global/cutscene owners, preserve selected source structure/operands. Retained `programCorpus` physically groups commands to end with inline action bodies separately. Parsing is not natural reach or a playable content package.|
|Python conversion|`src/sf2tool/remake_exploration_programs.py:OriginalPrograms.compile` lowers flags (Story adds400), text, position, actions, block copies, quake, flash, scene loads, hide and queued warp subject to selected scene maps. Unmatched `setDest` has no case and reaches native-call; do not substitute its position primitive. `followEntity` resolves selector offsets but emits no Bowie-HP precheck.|
|Content admission|`ExplorationContentReader.cs` reads typed story operations/native-call into `StoryProgram.cs` records/UnsupportedInstruction. `PrivateBattleScenarioReader.Assemble` admits battle1/2/3 only; requires STARTING, selected item expressions and commandsets (no HEALER3), plus narrower class/derived-state assumptions. Battle15's hidden row/Power Water/Monks do not become supported by generic geometry/order types.|
|Domain|Map/follower motion, flag membership, region geometry, battle initialization/turn/control and source-order AI primitives exist. `BattleInitializationRules.cs` requires controlled roster-only/already-refreshed starts, difficulty0, no auto-battle/opponent control, and narrow enemy loadouts; `Gameplay/Sf2/SourceEnemyAi.cs` admits commandsets6/7/9/14 and narrow no-spell/item categories, excluding these Monks/equipment/spells. `PrivateBattleScenarioReader.Order` distinguishes MOVE_TO point, FOLLOW_ENEMY high-bit ID and FOLLOW_TARGET ally ID; expression is not15 admission. Native trapped-chest/drop/terminal composition still needs supported bindings.|
|Application|`Runtime/Exploration/ProgramRunner.cs` applies FollowEntity by resolving current slots and installing FollowerMotion without checking Bowie's HP. It handles typed scene/map/position/flag/queue operations and returns Unsupported at native instructions. Thus even expressible follows need a source-guard conversion decision before faithful15 use; no blanket rejection of every primitive or claim of current15 execution is made.|
|Battle production|`src/sf2tool/remake_exploration_battles.py:prepare_encounter` permits2/3; `compile_routes` reads the selection's battle IDs and wildcard rows/hooks, separately from that producer restriction and Content/Domain/Application validation. No15 scenario, full after-scene, chest trap, all-AI or playable connection is established by this prose.|

Paths without `src/sf2tool/` in this table are under `remake/src/` in their
named Content/Application projects; Domain rule owners are under
`Sf2.Remake.Domain/`. These are actual inspected consumers, not evidence of
the original and not a fabricated conversion report. The
[four frontiers](./story-content-coverage.md#four-different-frontiers) remain:
this static research chapter, original runtime observations, English design
contracts, and remake playable capability each have their own acceptance.

## Reproduction, review and remaining Unknowns

Load the owning checkout's ignored `local/private-inputs.ps1` in the same
PowerShell process before any uv command. Verify the configured read-only
source pin/clean state; read complete Map61 setup files and their adjacent
include terminator, selected Map69/8/50/61 records, all three scene programs,
full spriteset15 and selected five definitions. Resolve text through hex keys
above. Follow the named request/setup/map-script, initialization/trigger/spawn/
chest/drop, hook/terminal/escape/Suspend/caller and actual production consumers.
Existing fixtures under `tests/fixtures/h2/` include map-data/setup/entities,
map-events entity/zone programs, battle-global/routing/spriteset/cutscene and
map-script-engine static owners; the retained map-init/script payloads add
their accepted program material without a rebuild. Fixtures do not establish
story-runtime observations. The Issue handoff supplies exact selection details
and the complete direct document-check code with a fresh receipt command.

Manual source→consumer→prose review includes changed77/729/730/415/465/515/999,
256 repeat/reentry, dead/missing/overwritten allies/followers, first/repeat/
interrupted outcomes, hidden143 state/occupancy/F151/drop29/capacity, changed
live region/checked word/party/RNG and F88/egress/caller distinctions. It is
not replaced by keyword assertions. Check full three-file diff, links/anchors,
UTF-8/private boundary and unchanged registered ledger counts, then clean
committed `uv run sf2 verify plan --base origin/main --head HEAD` and actual
exact-head scoped CI. Documentation scope selects no normal/full/.NET/Godot/
H3/H4/emulator/screenshots, old comparison or lowering run here.

**Unknown:** natural order and optionality, physical collisions/paths among
storage regions, actual incoming state/aliases/HP/flags/party, complete tactics,
all actor/service/control completion, visible timing and saves. These are
neither passes nor automatic acquisition queues. Battles14/44 and other
optional-content gaps remain; no Epic1053/full-game completion is claimed.
Retain1198's missed high-byte follow gate and focused correction evidence,
1196 runtime-boundary/1194 Suspend corrections, and #1047's twelve unresolved
comparisons without replay or reinterpretation. Their exact results/process
state stay in the local handoff and owning records.
