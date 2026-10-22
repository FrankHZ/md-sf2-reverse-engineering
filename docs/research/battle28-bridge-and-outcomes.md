# Battle28 bridge, confrontation and outcome returns

Evidence date: 2026-10-22. **Confirmed:** two Map74 requests can admit
Battle28 on Map47 under live F428. Fresh entry runs the bridge rescue setup,
constructs the actual party and sixteen enemies, and reaches conditional
turn/control dispatch. Victory can run the complete Cameela revelation and
return to retained bridge exploration; repeat victory, defeat, retreat and
Suspend have different lifetimes. **Inferred:** this is a branch alongside
the Tristan route. **Unknown:** natural traversal, actual incoming state,
all tactical decisions, completed presentation/control and persistence.

## Agreement, provenance and minimum evidence

**Standard** rigor for [Issue1198 / Epic1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1198):
one connected account of entrance → scene/formation/first control → outcomes
→ complete after-scene → return connections. Different request, actor and
caller lifetimes justify this level. Accepted base is
`40f4a3c6078a49e527b2b391299ee7d597d860ef`; there is no stacked dependency.
This owner and only its Battle28 references in [story coverage](./story-content-coverage.md)
and [source coverage](./source-coverage.md) change. No engine, parser, fixture,
schema, index, extraction, runtime acquisition or all-action simulation.

Reuse the accepted [Map74 producer/admission](./pacalon-departure-to-battle27-admission.md),
[Battle27 return](./battle27-outcomes-through-tristan-admission.md),
[Tristan entry](./tristan-entry-interactions-and-exits.md) and
[Caves35→0/Battle29](./tristan-caves-and-shrine-exterior-admission.md).
The [battle loop](./battle-loop.md), [global tables](./battle-global-data.md),
[spritesets](./battle-spriteset-data.md), [routing](./battle-routing-data.md),
[cutscenes](./battle-cutscenes.md), [maps](./common-maps.md),
[scripting](./common-scripting.md) and [stats](./common-stats.md) own shared
mechanisms. Their named helper observations are not Battle28 observations.
The [Battle0 owner](./battle00-bonus-battle-and-reset.md) supplies the corrected
shared healing/retreat/Suspend distinctions, with its special reset caller
excluded here. PR692 contributes no evidence or implementation dependency.

All original-source paths below are relative to `disasm/` at read-only
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The USA baseline is SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM read/equivalence claim is made. `sf2enums.asm`, `sf2const.asm`,
`sf2macros.asm` and `sf2mapmacros.asm` own constants/encodings. Fnnn denotes
flag nnn; indices are zero-based and address extents half-open. Short
`battleloop/`, `battlefunctions/`, `battlefield/`, `ai/`, `cutscenes/` paths
are under `code/gameflow/battle/`; `map/` and `entity/` under
`code/common/scripting/`.

The new core source is 5,050-byte before, 10,606-byte after and 6,346-byte
spriteset text, about22 KiB. Minimum evidence is those full programs/data,
their direct consumers, selected existing fixtures and the retained
18,285,979-byte map-init/script pair's accepted provenance. No pair rebuild
or old recipe replay is needed. Selected reads/checks are linear in these
bounded files; budget each check below60s/512 MiB, receipts below4 MiB.
Peak memory is unmeasured. Contradictory consumers or material expansion
require reassessment, not automatic lowering/capture. The local handoff
records measurements and the complete one-off review recipe.

## Both entrances, unlock and ordered admission

**Confirmed:** `data/maps/entries/map74/mapsetups/s3_zoneevents.asm` has
`(255,61)→Map74_ZoneEvent0`, then default RTS. The actual zone selector uses
exact255 as wildcard. A separately admitted layout-kind `$1400` event6
supplies candidate X/Y; loading Map74 does not automatically call the zone.
The callback orders these branches:

| Live state | Ordered writes |
| --- | --- |
|527 clear|Set427; post longword reload `$000100FF`; set428,429,430; return.|
|527 held,811 clear; or527/811/532 held|No new writes.|
|527/811 held,532 clear|Set432; post the same reload; return.|

Thus428 may already have been set **before** Battle27. Battle27 victory
does not set it. A completed27 supplied without an earlier callback does
not cause this callback to repair428. Reload writes event1/PARAM1=0/
PARAM2=255, leaving PARAM3–5/sound supplied; the accepted producer owner
retains its partial overwrite and cleanup/wait limits.

`data/maps/entries/map74/6-warp-events.asm` has eight ordered rows; the
two bridge rows are3/4, after two Moun entrances and Tristan row2:

| Source on74 | No-scroll destination | Facing |
| --- | --- | --- |
|50,31|47 `(1,8)`|Right0|
|52,31|47 `(24,30)`|Left2|

No completion, item, party or dialogue predicate is in either row.
`WarpIfSetAtPoint` first tests terminator, then source bytes in order;
any negative source byte is wildcard. Ordinary character control calls it
only through layout kind `$1000` and its movement/obstruction path, using
candidate coordinate words divided by384. It posts mutable event1, payload
and sound in separate writes. This is not evidence of walking either cell.

The no-scroll consumer clears the event before dispatch, idles main,
discards the exploration return address and samples actual main coordinates
and facing. Nonnegative destination bytes replace only coordinate low
bytes; negative destination bytes retain sampled words. For ordinary
nonnegative samples these two requests return words47/1/8/0 or47/24/30/2,
D4=0. Negative sampled high bytes can survive replacements. Pending writes,
interruptions and changed payloads use their actual later values.

`mainloop.asm:@Start` calls SwitchMap then CheckBattle. The complete switch
table3/F640→4,71/F530→78,58/F999→59 has no47. CheckBattle scans the full45-row
`data/battles/global/battlemapcoords.asm` in order. Its **sole47 row is28**:
`47,0,0,26,38,255,255`. Both trigger bytes are wildcard; F428 is required.
On match it writes crop `(0,0,26,38)`. Held528 clears428 but still admits28
on this call. It returns resolved map47/D7=28; MainLoop passes D1=28 to
BattleLoop. It does not yet assign CURRENT_BATTLE or run initialization.

With428 clear no47 row matches: CheckBattle restores the original map
operand and returns D7=-1. Explicit47 starts fresh exploration. A supplied
CURRENT255 resolving to47 is different: on no match the restored operand
is255 and exploration uses its retained arm. Neither478 nor a presumed
27→28 numeric sequence substitutes for428. Tristan's35→0 admission selects29,
not28; no mandatory bridge-before-Tristan/Caves route is established.

The complete `data/maps/mapsetups.asm` has **no Map47 entry**. Actual
`map/mapsetupsfunctions_1.asm:GetCurrentMapSetup` reaches `ms_Void=$FFFF`;
entity-list selection returns the sentinel and init invocation is skipped.
This is absence of a setup, not an unflagged populated default. Shared
main/follower construction and the map-data warp table still operate.

## Fresh initialization, hook routes and scene semantics

**Confirmed:** `battleloop_1.asm:BattleLoop` clears PLAYER_TYPE, sets399,
then tests88. Clear88 resets seconds, saves incoming map/battle words,
writes CURRENT_MAP47/CURRENT_BATTLE28, installs base VInts and calls before.
After it returns, the saved arguments are restored/reassigned; region flags
90–105 clear, eligible allies heal, ally/enemy positions initialize, AI memory
clears, LoadBattle runs for combatant0, then the start wrapper and round prefix.
Crop is not recalculated here. Every suffix requires prior calls to return.

| Selected record | Slot28 and actual wrapper |
| --- | --- |
|`cutscenes/beforebattlecutscenes.asm`|48-word table, byte56 at `$47AC0`: `bbcs_28`; checked478, no flag set by before.|
|`cutscenes/battlestartcutscenes.asm`|48-word table, byte56 at `$47B64`: `ms_Empty`; wrapper checks then **sets478 before** dispatch.|
|`cutscenes/battleendcutscenes.asm`|48-word table, byte56 at `$47C20`: `ms_Empty`; Bowie/enemy128 HP gates and shared leader tail remain.|
|`cutscenes/regionactivatedcutscenes.asm`|Only32/0,32/1,40/0,40/1; no28 dispatch or played-flag write.|
|`cutscenes/afterbattlecutscenes.asm`|48-word table, byte56 at `$47D2C`: `abcs_battle28`, checked528.|
|`cutscenes/afterbattlejoins.asm`|Byte28 at `$47D86` is0; layout includes this cutscenes table, and shared tail calls JoinForce(0).|
|`global/enemyleaderpresence.asm`|Byte28 at `$47CAA` is-1, not zero; enables shared living-enemy append.|
|`global/afterbattlepositions.asm`|Only05 override; no28 relocation.|

Paths in this table are under `data/battles/`. Empty means `map/ms_empty.asm`'s
`$FFFF` script, not a missing caller. ExecuteMapScript resets loading pointer
and text-skip latch; its terminator can wait for scroll and clears camera
speed. Held478 skips before and start scenes, but not fresh healing/roster/load.
An interrupted start can leave478 set despite unfinished work.

In-script `loadMapFadeIn47,x,y` invokes csc37→csc48: out-to-black state,
detached camera, tilesets/fade wait/VInt changes, LoadMap with packed camera
coordinates scaled by3, then reactivation/one VInt wait. The later fadeInB
is separate. These are view coordinates, not main placement. Explicit map
load sets CURRENT_MAP47 and selects graphics/layout/area; CURRENT_BATTLE
remains28 and the supplied battle crop applies. It does not execute the
ExplorationLoop's healing, temporary-flag reset or setup init.

csc42 `loadMapEntities` disables display/VInts, reads the main header,
calls InitializeMapEntities/LoadEntityMapsprites and re-enables them. It
clears49 actor records,64 lookup bytes and walking scratch, declares
flag followers first, fixed rows next, main/Bowie physical0 last. Fixed
ally rows deduplicate against a nonzero ally lookup; NPC sprite rows allocate
sequentially. Positions/destinations use384 units per tile. All below-listed
NPC sprites are ordinary30..239, not special physical47.

FOLLOWER_B=31 is a logical alias. The complete follower table can write it
for F74/Astral209, F68/Oddler213, then F70/Astral209; later matches overwrite lookup
without deduplicating actors. The accepted [Astral-return owner](./new-granseal-return-and-astral-following.md)
establishes F70's story source. **Inferred story identity:** B here is Astral,
also explicitly addressed in the after-dialogue. Actual identity still
depends on those flags. Map47 is neither follower-excluded nor an overworld,
so F65 does not prepend Caravan; the scripts declare Caravan separately.
Excess conflicting flags have no supported five-pointer follower capacity
guarantee. Missing B maps to cleared physical0; corrupt mappings are unchecked.

Let N be actually declared flag followers and P=1 if Peter is absent from
their lookup, otherwise0. Conditional scene allocation is:

| Scene / declaration | Coordinates/facing | Logical alias / physical slot |
| --- | --- | --- |
|Before `ce_4C03C` main|1,8 Right|Bowie0 /0|
|Peter row|0,9 Right|7 /N+1 if absent, otherwise row skipped|
|Caravan62|0,8 Right|128 /N+P+1|
|POSE3=176 woman|12,24 Up|129 /N+P+2|
|Four WORM84 rows|13,13 Left;12,17 Up;11,22 Down;11,25 Right|130–133 /N+P+3 throughN+P+6|
|After `ce_4C3B4` main|12,28 Down|Bowie0 /0|
|POSE3 woman|12,31 Up|128 /N+1|
|Peter row|13,28 Down|7 /N+2 if absent, otherwise skipped|
|Caravan62|12,27 Down|129 /N+P+2|
|EFFECT3=178|63,63 Left|130 /N+P+3|

The before list is64 bytes (6+7×8+2); after list40 (6+4×8+2).
Scene129 woman becomes scene128 after; neither is combatant129. Scene128
Caravan becomes scene129 after. NPC ENTNUM is physical slot, not script ID.
63×384=`$5E80` is below inactive `$7000`, so staging there is not removal.

Handlers retain [the accepted action distinction](./battle26-admission-and-geshp-confrontation.md#complete-ordered-program):
setActscript/custom/action-sequence waits poll for `eas_Idle`, not a tile
endpoint; un-waited installation overlaps later commands and can be replaced.
Packed moves become signed relative destination requests plus destination
waits. csWait is Sleep, not an implicit join. eas_Init configures speeds32,
collision/auto-facing/size24/orientation and refresh; Jump uses relative
requests/timers with auto-facing temporarily off. stopEntity sets animation
counter-1; startEntity sets1. `ac_motion OFF` disables animation, not travel;
orientDown changes FLAGS_B orientation bits, **not FACING**; refresh requests
sprite update separately. setFacing writes FACING/update; setPos writes
position/destination/facing, without resetting travel/script. For actual
allies0–29, setPos/facing/stop have HP guards; follower/NPC IDs
bypass them, while action installation/text/shiver have different gates.
Dead Peter/Bowie therefore need not receive every apparent placement/pose
operation. followEntity has the distinct high-byte precheck described below.

Shiver saves/restores size/counter around three flash/size cycles. Quake2/0
directly writes amplitude; camera destinations wait for scroll, cameraSpeed
`$30` writes48, camera-entity changes target. Entity white-flash40 uses the
handler's shifted-count/DBF loop:11 iterations/four VInt waits each,44 explicit
waits. Screen-flash20 instead shifts by3 then DBF, submitting three
FLASH_QUICKLY_2 fading services and DuplicatePalettes. These are source
effects/waits, not measured presentation or timing.

## Complete before program and dialogue

**Confirmed:** `data/battles/entries/battle28/cs_beforebattle.asm:bbcs_28`
is `$4BE8C..$4C03C`, followed by the above list through `$4C07C`.
This ordered account includes its full command stream. B denotes logical31;
R/U/L/D denote signed tile movement; `!` means waited action installation
or sequence. Text IDs below are nominal with skip clear and calls returning.

| Phase | Ordered commands and text boundary |
| --- | --- |
|Load/approach|Cursor2661; loadMapFadeIn47,0,4; entities ce_4C03C; Init! Bowie,Peter; Peter→0,9 Right; Init! B; B→0,7 Right; stop129; music BATTLE_THEME_3; fadeInB; cameraSpeed48. Unwaited R3 Bowie,B,Peter; waited128 R3; wait40.|
|Bridge discovery|Jump! B; B R2,D1!; face B Right;2661; camera7,11; shiver130/scream/wait30; shiver131/scream/wait40; camera0,6; Peter D1,R1!;2662,2663.|
|Rescue staging|Unwaited Jump Bowie,B; Jump! Peter; Bowie D1,R1,D1!; wait5; Bowie look Right/Left twice; wait5; Peter look Up/Down twice; wait100. Jump! B; inline B speed48,48→Idle!; B D2 unwaited; wait10; stop Bowie; inline Bowie speed64,64→Idle!; quake2/hit sound; Bowie D1!; face Bowie Up; inline motion OFF/orientDown/update/Idle!; waitIdle B; face B Right; quake0.|
|Appeal/start handoff|2664,2665; face Peter Right; shiver Bowie; camera7,21;2666–2668; camera0,6; face B Down; Jump! B; wait30; Init! Bowie; start Bowie; headshake Bowie;2669; Jump Peter unwaited; nod Bowie; end.|

For example the initial asynchronous R3 group does not imply all four actors
finished together; only128 is joined at that point. Later B's D2 is explicitly
joined after Bowie's pose installation. Script return is before the fresh
battle population replaces these actors. There is no setBlocks/collision
mutation, countdown variable, reward or item transaction in this program.
Dialogue about eating/falling and quake/hit requests do not establish a
physical collapsing bridge mechanic or timed win condition.

All literals were checked against hexadecimal keys in
`data/scripting/text/gamescript.txt`; only public paraphrases are given.
S=nextSingleText (close portrait/text and Sleep10 after successful display),
T=nextText (continued textbox). Both use/advance CUTSCENE_DIALOG_INDEX only
on the non-skip path; cursor is2661→2670 here and2670→2718 after. Debug text
skip does not skip the surrounding scene actions. `N`, `W1`, `W2`, `LEADER`,
`NAME7` below mean the original ordered line-break/wait/name tokens; they
do not reproduce the dialogue. Speaker control is `$00` throughout before.

| ID / form | Speaker | Public meaning | Ordered tokens |
| --- | --- | --- | --- |
|2661 S|B|Calls attention to the scene.|W1|
|2662 S|Peter|Says monsters are consuming the bridge.|N,W1|
|2663 S|129|Calls for rescue.|W1|
|2664 T|B|Points toward the victim.|W2|
|2665 S|B|Identifies a stranded woman.|N,W1|
|2666 S|Peter|Urges departure before the bridge falls.|N,N,W1|
|2667 T|129|Reports an injured ankle and requests help.|N,W2|
|2668 S|129|Says she cannot rise.|W1|
|2669 S|B|Urges the leader to defeat the monsters quickly.|LEADER,N,W1|

## Formation, enemies and actual battle population

**Confirmed:** `data/battles/spritesets/entries.asm` slot28 selects
`BattleSpriteset28`, `$1B5306..$1B5480`,378 bytes: header4, allies144,
enemies192, regions36, point2. Relative subsection offsets are0/4/148/340/376.
The header is12 ally placements,16 enemy rows,3 regions,1 point. Twelve-byte
placement rows store identity/X/Y/commandset, item word, two order/region
pairs, filler/spawn. Ally rows all declare HEALER1,NOTHING,
NONE/15,NONE/15,0/STARTING. Only these coordinates vary:

| Ordinal | X,Y | Ordinal | X,Y |
| --- | --- | --- | --- |
|0|5,6|6|5,7|
|1|4,8|7|2,7|
|2|4,7|8|5,8|
|3|3,7|9|2,8|
|4|3,8|10|6,7|
|5|4,6|11|6,6|

`InitializeAllAlliesBattlePositions` sets all30 X=-1, rebuilds force lists
from joined/active flags in ascending ID, then positions actual nonzero-HP
active members. A dead member consumes a processed-member slot without
advancing the placement pointer; surplus beyond12 leave the battle party,
without immediate list refresh. Zero active count lacks an empty-list guard.
These ordinals are not fixed named allies or an instruction to recruit12.
Ally placement AI/item fields do not replace the member's own state.

Healing scans all30 regardless of membership: living allies plus immortal
Peter7/Lemon28 at HP0 receive maxHP/MP. Other dead allies stay dead. Status
AND7 retains STUN/POISON/CURSE initially, then UpdateCombatantStats recomputes
CURSE from equipped cursed items; old CURSE without such equipment can clear.
Class, equipment, membership and HP still affect sprites/placement/control.

All sixteen enemy rows have item NOTHING, filler0/STARTING0, and secondary
NONE/15. The complete primary data is:

| Row / ID | Type / definition ID | X,Y | Commandset / value | Primary / region |
| --- | --- | --- | --- | --- |
|0 /128|WIZARD72|11,24|LEADER14|MOVE_TO\|0 /15|
|1 /129|MASTER_MONK82|12,26|HEALER3=2|MOVE_TO\|0 /15|
|2 /130|DARK_SOLDIER18|13,23|ATTACKER3=9|MOVE_TO\|0 /15|
|3 /131|LESSER_DEMON45|14,1|ATTACKER1=6|NONE /15|
|4 /132|LIZARDMAN19|11,7|SENTRY8|NONE /15|
|5 /133|DARK_SNIPER62|12,7|SENTRY8|NONE /15|
|6 /134|LESSER_DEMON45|2,16|ATTACKER1|NONE /15|
|7 /135|WYVERN46|5,18|ATTACKER1|NONE /15|
|8 /136|WYVERN46|6,17|ATTACKER1|FOLLOW_TARGET\|0 /0|
|9 /137|WORM20|13,13|SENTRY|NONE /15|
|10 /138|WORM20|12,17|SENTRY|NONE /15|
|11 /139|WORM20|11,22|SENTRY|NONE /15|
|12 /140|WORM20|11,25|ATTACKER1|NONE /2|
|13 /141|LESSER_DEMON45|17,12|ATTACKER1|NONE /0|
|14 /142|WYVERN46|19,20|ATTACKER1|NONE /1|
|15 /143|LESSER_DEMON45|6,26|ATTACKER1|NONE /1|

`InitializeEnemyBattlePosition` scans128–159; real rows' spawn nibble0
initializes stats. Missing144–159 get activation0/maxHP0/HP0/X=-1, not a full
clear; their trigger read uses the unadvanced row0 pointer here, yieldingFF.
The32/Jaro143 exception does not apply. STARTING has no occupancy check at
this initialization; later spawn reset is a distinct consumer.

Each definition in `data/stats/enemies/enemydefs.asm` is56 bytes copied as14
longwords. Initial currentHP/MP=maxima; position/type/movetype-commandset,
trigger nibbles and order words are then assigned. Row0–2 order words are
40FF, row8 is00FF, all othersFFFF. Trigger bytes areFF except rows8/13=0F,
12=2F and14/15=1F. InitializeEnemyItems sees NOTHING=`$007F` and skips
AddItem/equipping; it **retains copied definition equipment**.

Initialization retains definition activation `$F000`, ORs the rotated
initialization word masked `$0FFF`, then adjusts difficulty and refreshes
stats. Fresh Monk129 has3000; every other declared enemy2000. Lower
definition BIT4/BIT5 disappear; primary/secondary-active, neutral and spawn
bits start clear. The complete random-battle list has no28, so
IsBattleUpgradable rejects before completion/level/RNG upgrade selection.
Held528 does not upgrade these definitions. ClearAiMemory resets48 target
bytes toFF and48 memory bytes to0, not the region checked word or turn queue.

All eight types use ENHANCED spell power, initialStatus NONE, four item and
four spell slots. The named first item is followed by three NOTHING slots;
spells shown occupy the first slots, the rest NOTHING:

| Type | LV | HP/MP | Base ATT/DEF/AGI/MOV | Move type | First item | Spells |
| --- | --- | --- | --- | --- | --- | --- |
|Wizard72|26|37/37|32/40/44/5|MAGE11|FLAIL equipped|BLAZE3|
|Master Monk82|28|47/39|22/45/31/6|HEALER12|BRASS_KNUCKLES equipped|HEAL4,DISPEL1|
|Dark Soldier18|23|35/17|31/46/34/4|REGULAR1|LARGE_AXE equipped|ATTACK1|
|Lesser Demon45|22|40/21|49/37/32/6|FLYING5|NOTHING|BLAZE3,BOOST2|
|Lizardman19|24|40/0|34/45/38/6|REGULAR1|LARGE_AXE equipped|none|
|Dark Sniper62|24|38/0|11/37/35/5|STEALTH_ARCHER10|HYPER_CANNON equipped|none|
|Wyvern46|26|46/0|58/39/36/7|FLYING5|NOTHING|FLAME1|
|Worm20|25|45/0|60/40/34/5|STEALTH3|NOTHING|none|

Resistance is FIRE_MINOR for Wizard, STATUS_MINOR for Lesser Demon,
ICE_WEAKNESS for Lizardman, ICE_WEAKNESS|FIRE_MAJOR for Wyvern; others0.
Prowess normally CRITICAL150_1IN32/DOUBLE_1IN32/COUNTER_1IN16; Wizard has
counter1/32, Lizardman critical1/8, Sniper double1/16, Wyvern/Worm poison
critical. Monk has CRITICAL125_1IN4/DOUBLE_1IN8/COUNTER_1IN16. Priority modifier
is2 except Monk3; Demon/Wyvern declare BIT4|BIT5, Sniper/Wizard BIT4 before
the mask. These are declarations, not performed poison/critical/spell actions.

The actual consecutive BEQs in AdjustEnemyBaseAttForDifficulty apply
floor(baseATT×5/4) at **all** difficulties, twice at SUPER2. Itemdefs give
FLAIL +19, BRASS_KNUCKLES +39, LARGE_AXE +25, HYPER_CANNON +40. Resulting
base/current ATT for NORMAL/HARD/OUCH versus SUPER is respectively:
Wizard40/59→50/69; Monk27/66→33/72; Soldier38/63→47/72;
Demon61/61→76/76; Lizard42/67→52/77; Sniper13/53→16/56;
Wyvern72/72→90/90; Worm75/75→93/93. Other listed fresh stats stay unchanged
under these clear-status/equipment inputs; these are not damage outcomes.

### Neutral woman, resources and rebuild boundaries

**Confirmed:** LoadBattle calls PositionBattleEntities, clearing the scene
actors/lookups and declaring nonnegative-X/Y allies/enemies, without an HP
test in that physical scan. Neutral-marked enemies skip its ordinary pass.
With L ordinarily placed allies, the sixteen fresh enemies map to physical
L..L+15, facing Down/eas_Standing; physical count depends on actual state.
Definition mapsprites are136,146,82,109,83,126,110,84 respectively for the
eight table types above. No scene actor identity survives this reconstruction.

After that pass, clear528 selects the sole28 group in
`data/battles/global/battleneutralentities.asm`: `(12,24)`, Up,POSE3,
`eas_Motionless`. The consumer starts at combatant159, writes max/currentHP0,
status0,X12/Y24 and ORs NEUTRAL8. Fresh159's activation becomes0008.
It declares the next physical actor L+16 (28 if L=12), layer0. It writes
`move.b d0,-(a1)` with a1 reset to enemy lookup base: this is lookup offset31,
**FOLLOWER_B**, not enemy159's offset63. Ordinary159 lookup remainsFF.
Thus the battle's B alias is the neutral woman, not the pre-scene Astral;
GetEntityIndexForCombatant(159) does not recover her. No joined/active flag,
reward or rescue-success state is set. HP0 excludes her from counts/queue.
Motionless disables animation and enters Idle, without a new FACING write.

Held528 skips this neutral pass: fresh159 stays HP0/X=-1/activation0 and
B lookup0. F88 loads can skip an already neutral ordinary159 then rewrite
it when528 clear, or declare a supplied nonneutral159 before repurposing
its state. This is not a general mapping repair or byte-preserving restore.

Map47 header `$B3824` selects palette0/tilesets0,24,25,26,57 and its own
blocks/layout. The sole area `$B3852..$B3872` has rectangle0,0..25,37,
secondary foreground0,0/background0,38, parallax256/256 and128/128,
zero autoscroll, type255 and BATTLE_THEME_3. Flag/step/roof/chest tables
are terminators. No collapse copy is declared in them.

LoadBattle selects map47, rebuilds physical actors, chooses the supplied
combatant's view (fresh0/Bowie; missing/negative mapping has fallback limits),
then full LoadMap, sprites, base VInts, terrain, music and fade-in. Crop0,0
copies26×38=988 words/1,976 bytes in64-word layout rows, and battle bounds
become25×384 by37×384. Battle/local/source coordinates coincide here because
crop origin is0,0; camera coordinates and entity internal units remain distinct.
Terrain pointer slot28 selects `BattleTerrain28`, incbin
`data/battles/entries/battle28/terrain.bin`, decompressed to the shared48×48
array. This note neither extracts terrain nor proves a legal walked route.

`data/stats/enemies/enemybattlesprites.asm` selects WIZARD38/0,MONK41/2,DWARF5/1,
GARGOYLE24/1,LIZARDMAN12/0,HUNTER_GOBLIN32/1,WYVERN26/0,WORM13/1 in that same type order.
These are battle-sprite/palette indices, separate from mapsprites and enemy
IDs. Scene YOUNG_WOMAN197, EFFECT2=175, CAMEELA156 and EFFECT3=178 are sprite
requests, not imported/licensed remake assets. No binary art is published.

## Regions, first dispatch and control inputs

**Confirmed:** three12-byte type4 records have second/trailing bytes0 and
the following four packed vertices; point0 is `(12,24)`:

| Region / flag | Vertices1,2,3,4 |
| --- | --- |
|0 /90|0,19;0,37;25,37;25,2|
|1 /91|0,25;0,37;25,37;25,11|
|2 /92|0,14;0,37;25,37;25,14|

The actual `triggerregions.asm` consumer tries triangles1/2/4 then3/2/4,
using signed-byte positions, integer multiply/divide truncated toward zero,
dominant axis and endpoint-B projections. It scans all30 allies with positive
signed HP and nonnegative signed X/Y; membership is not tested. Each of the
twelve formation positions is outside all three. For region0 triangle1 the
second edge4→1 projects Y=`19+trunc(-17X/25)` (15..18 at X2..6), above
every formation Y6..8 on the opposite side from vertex2. Triangle2 shares
the4→2 edge: its projection at Y6..8 is X20..22 while formation X2..6
lies opposite vertex3. Region1's corresponding first-edge barrier is
`25+trunc(-14X/25)` (22..24), and its second triangle rejects on the same
diagonal separation; region2's horizontal Y14 excludes both triangles.
No earlier edge gives collinear acceptance for these points. These are
integer-consumer results, not a floating polygon/pathfinding claim.

At `BattleLoop:@Start`, ActivateEnemies scans128–159 then falls through once
more for160. Update requires X≥0/HP≠0/enemy byte; Y/membership/neutral are
not its gates. TriggerRegionsAndActivateEnemies checks each unexamined region,
sets90+index on a hit and marks the bit checked even on a miss. Fresh init
cleared90–105, **not NEWLY_TRIGGERED_BATTLE_REGIONS**. With unchanged fresh
placements all three flags remain clear; the checked word becomes incoming
OR7. Changed placed nonmembers can trigger, but already checked bits skip
geometry. The later non-swarm AI entry resets that word; it is not an initial
reset before this prefix. Slot160's accepted deals/RAM-alias hazard remains
with [the shared owner](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias);
no fresh region flag here enables its conditional deals write.

Both trigger nibbles15 cause no activation write. Rows8/13 use90,12 uses92,
14/15 use91, all secondary15. A held primary sets bit0, preserving other
bits, including supplied bit1; a separately changed secondary can set bits0/1.
Fresh words therefore remain3000 for Monk and2000 otherwise. No28 region
cutscene runs. Debug diagnostics may call text463 before spawn handling.
PopulateTargetsListWithSpawningEnemies finds all fresh spawn modes0 and
writes length0; it does not consume stale list bytes or call spawn animation.

Changed hidden2/respawn1/respawn3 modes require the shared HP/trigger/maxHP
tests; triggers can write active bits before reset rejection. Row-bound144
and all-combatant occupancy, including dead/self, govern ResetSpawningEnemyStats.
Failure writes HP/maxHP0/X=-1; success reinitializes row stats/items/position
then restores activation before append. IDs144–159 have no valid row. These
changed-state possibilities are not fresh bridge respawns or a timed event.

MOVE_TO bit6 indexes the sole point: rows128–130 reference12,24.
FOLLOW_TARGET\|0 is byte0, **combatant Bowie0's live X/Y**, not point0 or
enemy128. `GetMoveOrderPosition` branches on bit6; the non-point arm reads
the encoded combatant. StartAiControl can replace a follow order with the
secondary when that target HP is zero. Order bytes are not trigger flags or
proof that movement happened.

GenerateBattleTurnOrder clears64 two-byte records toFFFF, offers allies0–29
then enemies128–157 (not158/159), admitting nonnegative X and nonzero HP.
Y/membership/neutral/activation are not queue predicates. All sixteen fresh
enemies qualify with AGI from the definitions:44,31,34,32,38,35,32,36,36,
34,34,34,34,32,36,32. Each has one record; none has rawAGI≥128. With N
eligible placed allies and T doubles, records=16+N+T≤40 for ordinary12 slots.
Actual changed party/stats can invalidate that bound; no append guard exists.

For A=rawAGI&127, score=`A+R(A>>3)-R(A>>3)+R(3)-1`; doubles add
B=floor(A×5/6), score=`B+R(B>>3)-R(B>>3)`. Each draw advances
seed=(seed×13+7)&65535, including range0, and returns floor(seed×range/65536).
Scores store low bytes. The64-buffer signed-byte descending sort uses62
passes/63 adjacent comparisons, preserving append order on ties. Fresh
positive enemy scores precede padding; 48 enemy draws follow actual ally
draws. **No fixed first actor** follows without party, current AGI and RNG.
Changed wrapped scores/padding/capacity retain the shared limitations.

The cursor clears to0; a non-FF actor reaches ExecuteIndividualTurn,
otherwise prefix repeats. A call is not an executed action. Its actual body
clears death-list length; HP0 exits; live state sets actor/target coordinates,
camera and cursor. MUDDLE/AI_CONTROLLED, enemy/control-opponent toggle and
ally/auto-battle toggle decide StartAiControl. The later player-control path
waits for cursor/view, opens windows/builds movement, then Sleep/Stun can
skip action before the second AI/player decision. No healthy named ally,
usable player turn or UI completion is assumed.

Non-swarm AI has all15/15 rows immediately eligible for its activated path;
region-gated rows8/12–15 take standby/STAY until bit0 is held. Actual command
streams in `global/aicommandsets.asm`, stopping at the first successful
ExecuteAiCommand result, are:

| This roster's set | Ordered candidates / pathfinding mode |
| --- | --- |
|LEADER14 (same pointer as13)|MOVE_ORDER5,ATTACK2,HEAL3,SUPPORT,MOVE_ORDER2,MOVE3,STAY /block-and-carve|
|HEALER3=2|HEAL2,MOVE_ORDER4,SUPPORT,ATTACK3,MOVE2,STAY /block-nonmovable|
|ATTACKER3=9|MOVE_ORDER3,ATTACK3,HEAL2,SUPPORT,MOVE_ORDER4,MOVE2,STAY /block-nonmovable|
|ATTACKER1=6|ATTACK1,HEAL1,SUPPORT,MOVE1,STAY /regular|
|SENTRY8|ATTACK1,HEAL1,SUPPORT,STAY /regular|

This binds the first decision entry, not every candidate's tactics, spell
selection, target or resulting move. Subsequent rounds reuse the same prefix
and shared battle/action owners. Full bridge AI/spell/strategy behavior remains
**Unknown** at this account; no turn-by-turn simulation is invented.

Held88 restores saved seconds, clears88/AI memory/view target, calls LoadBattle
with incoming D0 and jumps directly to the supplied individual-turn cursor.
It does not assign incoming47/28 to current selectors, heal/place/reset
regions, run hooks or generate a first queue. Incoming47 indexes lookup47
(enemy143) for LoadBattle view, not Bowie. A supplied FF cursor enters the
round prefix; non-FF reaches its actor call without fresh alignment/range
validation. Neutral loading/physical reconstruction can still mutate state.
Valid suspension/restoration and actual incoming saved fields are unproved.

## Terminal handling, victory and alternatives

**Confirmed:** after an individual turn returns, `battleloop_1.asm` orders
defeated hook, ProcessKilledCombatants, CountRemainingCombatants. Zero ally
count loses **before** zero enemy count wins. If neither, it runs the actual
actor's after-turn effects, death processing and the same ordered counts;
only then advances cursor by2. There is no second defeated hook in that
status pass. Post-status128 death therefore does not immediately append
living enemies through another hook. Debug kill-all is a separate input path.

The defeated wrapper requires BowieHP≠0 and enemy128HP=0. Clear528 runs the
empty script; held528 skips only that script. Both then reach the nonzero
leader-presence tail: scan128–159, append each nonzero-HP ID to the existing
length-delimited death list, terminate withFF and increment length. It does
not test coordinates/neutral/activation/roster bounds, deduplicate/reset the
list or zero HP. Here128 is **Wizard**, not Cameela or a Worm. Thus leader
death can clear other living enemy positions via this tail; all-enemy
exhaustion through counts can also win. The05 relocation override is absent.

ProcessKilledCombatants performs spin/sound/wait and kill/defeat bookkeeping,
sets X/Y=-1,status0, refreshes stats and moves mapped entities to7000,7000.
It neither zeroes HP nor clears list length nor calls the drop transaction;
stale/duplicate entries/missing aliases retain their hazards. Counts scan30
allies/32 enemies for X signed≥0 and HP≠0, ignoring Y/membership/neutral;
BowieHP0 forces ally count0. Fresh neutral159HP0 never counts. Simultaneous
exhaustion loses. The after-turn routine clears list length and can create
a new lethal-poison entry; retained earlier action lists are not replayed.

The complete `data/battles/global/enemyitemdrops.asm` has **no28 row**.
The actual action-side [drop consumer](./runtime-rng-and-battle-math.md#confirmed-enemy-item-drop-data-and-core-runtime-branches)
cannot match28; cleanup/victory is not an alternate drop caller. The after
script adds no inventory reward or Jewel transfer. Ordinary experience/gold
from admitted combat remains with its actual shared action consumers.

Victory orders: eligible-allies healing/refresh → member-derived return seed
→ after wrapper/shared JoinForce tail → clear428/set528 → return D4=1.
The seed writes current map to PARAM2, rebuilds actual force lists and reads
the first active byte without length/HP validation, adds crop origin to that
member's X/Y as bytes, samples mapped entity FACING into PARAM5 and writes
PARAM1=0. With unchanged47/crop0 this is47/memberX/memberY/facing; absent
mapping or empty/dead/full party cannot be replaced by a fixed Bowie seed.

Clear528 runs the complete after28 below; its warp replaces that seed with
255/16/30/Right and posts event1. Held528 skips it, so the member seed47
survives. Both reach cutscenes/afterbattlejoins byte0 and JoinForce(Bowie0),
setting F0/rebuilding force lists, then setting F32 only when active count
is below12. Existing F32 is not cleared at capacity; the newly set active
bit is not immediately rebuilt into the lists. No revive/stat initialization,
follower installation or seed recomputation occurs in this tail.

After returns, BattleLoop independently rereads CURRENT_BATTLE to clear
400+index/set500+index. For unchanged28 these are428/528;478 stays supplied.
It clears D0–D3 **words**, reads PARAM2–5 bytes into them and returns D4=1.
Upper halves are not globally cleared. Changed selectors/parameters between
wrapper/scene/join/flags/return are live reads, not one atomic snapshot;
an interrupted prefix does not roll back healing, alias, join or flag writes.

| Route for unchanged28 | Immediate effects / return |
| --- | --- |
|Defeat|UpdateBattleUnlockedFlag; sad music/text363/close; BowieHP=max; unsigned gold>>1; actual egress helper; D4=-1. No04 losable upgrade.|
|Admitted Egress|Deduct selected spell MP; close windows/text274; common cast sound/flash/close/unlock; unwind individual-turn frame; helper; D4.w=0.|
|Admitted Angel Wing|Remove selected item slot/refresh stats before close/text275; same escape tail/D4.w=0. No defeat gold penalty or forced Bowie revival.|
|Ordinary Suspend|Menu option reaches prompt because CURRENT_BATTLE28≠0; decline returns menu. Accept saves seconds, sets88, SaveGame(current slot), then normal fade/WitchSuspend. Debug+Start instead clears88/returns menu after saving.|

The [shared eligibility audit](./battle00-bonus-battle-and-reset.md#outcomes-escape-and-the-actual-reset-return)
is read with the actual28 menu path: learned selected Egress, target
confirmation and MP≥8; Wing requires held selected usable consumable4 and
confirmed target. Cancel/no target/lowMP does not admit escape. Earlier
Sleep/Stun can prevent action; the menu has no local Silence rejection before
the special Egress branch. Handler entry itself does not repeat those legality
checks. Escape branches discard the individual-turn return frame and bypass
ordinary terminal tests, returning to MainLoop's BattleLoop caller. It does
not run the after/join wrapper. Battle0's pre-prompt Suspend rejection and
EndGame reset suffix do **not** apply to28.

UpdateBattleUnlockedFlag leaves428 as supplied with528 clear, clears428
with528 held. Defeat/retreat do not newly set528 or change478. They retain
prior combat/party/item writes; later exploration may heal eligible resources.
GetEgressPositionForBattle has no28 special map and reads EGRESS_MAP, not the
victory seed or saved X/Y. F399 is ordinarily held from BattleLoop. Its actual
savepoint table gives Tristan15 `(12,16)` Up, Pacalon2 `(41,10)` Up, castle36
`(40,5)` Up; unmatched47/74 retain map and default1,1/Up. Other supplied maps
use their row/fallback. Matched bytes do not globally clear the incoming map
word; SwitchMap's word compare and later low-byte tests retain that distinction.
Changed clear399 gives3/56/3/Down. Held64 also resets raft fields (Polca/Bedoe
→72/43/48; Hassan→69/28/55; otherwise69/42/15); clear64 preserves them.
No actual church visit/save, eventual town initialization or persistence follows.

## Complete after program and dialogue

**Confirmed:** `data/battles/entries/battle28/cs_afterbattle.asm:abcs_battle28`
occupies `$4C07C..$4C3B4`, with list through `$4C3DC`. This is the full
ordered scene; `!` retains the waited distinction, B=31 and C=128:

| Phase | Ordered commands and dialogue |
| --- | --- |
|Rescue setup|Cursor2670; scene47/camera7,25; ce_4C3B4; Init! Bowie/Peter; Peter13,28 Down; Init! B; B11,28 Down; stop128/130; priority B0,Peter0,128FFFF; fadeInB; wait57; Peter Left;2670–2672. Priority normalizes to byte0/0/1, not numeric depthFFFF.|
|Help offered|Bowie D1!; B D1,R1 unwaited; wait15; JumpRight! Bowie; waitIdle B; B Down;2673; B D1!; B Up;2674–2676; sprite128 YOUNG_WOMAN;128 U2 unwaited; wait14; StopMoving!128 (relative0,0→Idle, replaces prior stream); Peter L1,D1!;2677–2678.|
|Departure stalls|Bowie R1 unwaited;129 R1,D2!; camera10,25; Bowie R2,D1,R2 unwaited;129 R3,D1,R1 unwaited; Peter R4,D1!;2679; Bowie/Peter Left;2680; wait60;2681–2682; Jump! Peter;2683–2684. No wait joins those two later un-waited paths.|
|Revelation|camera8,25; entity-flash128/40; screen-flash20;128→11,30 Right; EFFECT2; Up/wait8,Left/wait8,Down/wait8; CAMEELA sprite/Right/start128; Jump Bowie unwaited/Jump! Peter; Peter L1 unwaited; Bowie D1,L3!;2685–2689; B Left;2690–2698; Jump!128;2699–2700; entity-flash B40; Jump!128;2701.|
|Projectile/dodge|DeactivateAutoFacing!128;128 L1!;130→12,30 Left; inline130 speed56/56→Idle!; DeactivateAutoFacing!130; inline128 speed48/48→Idle!;130 L8 unwaited; jump sound/wait10;128 U1! thenD1!; wait30; hide130;2702–2705.|
|Cameela withdraws|Init!128; cameraSpeed48; inline128 speed48/48→Idle!; camera follows128; wait40;128 U10!; wait40; neutral camera; camera8,26; Bowie/Peter Up; B U2!;2706;128 Left; camera7,16;128 L1!;128 Down;2707; JumpLeft!128; TwirlShrinkDisappear!128.|
|Explanation/return|camera8,26; B Down;2708; B R1,D2!; B Right/Peter Left;2709–2717; follow129→Bowie slot2; B→129 slot1; Peter→129 slot3; warp CURRENT,16,30,Right; end.|

The after list rebuild restores B through the live follower table, independent
of the neutral woman's previous B lookup. C's pose→woman→effect→Cameela are
sprite changes to the same scene actor, not combatant conversion or recruitment.
EFFECT3's L8 is a visual projectile request; hide writes position/destination
7000,7000, replaces matching lookup aliases withFF and removes a matching
exploration-follower list entry. It is not a combat damage/death transaction. Auto-facing OFF is separate
from motion; later Init re-enables it. TwirlShrinkDisappear waits/cycles
facing and sizes22 down to6, moves128 to63,63, clears resize and restores18
before Idle. It does not set combat HP, story flags or save state. Quoted
lore/threats are characters' statements, not independently confirmed Zeon
orders, conspiracies, evil aura, hostage state or inventory ownership.

The full48 dialogue calls use `$00` except the explicitly marked `$C0`
rows below. `$C0` is portrait-control input, not a new entity
ID/allegiance. S/T and token notation retain the before section's meaning:

| ID / form | Speaker / control | Public meaning | Ordered tokens |
| --- | --- | --- | --- |
|2670 S|Peter|Relief after the danger.|W1|
|2671 T|C|Thanks the party.|W2|
|2672 S|C|Requests help standing.|N,W1|
|2673 S|B|Volunteers to help instead.|W1|
|2674 T|B|Offers accompaniment.|W2|
|2675 S|B|Proposes resting at nearby Tristan.|N,N,W1|
|2676 S|C|Thanks him.|W1|
|2677 T|Peter|Comments on B's attentiveness.|N,W2|
|2678 S|Peter|Proposes heading for Tristan.|N,W1|
|2679 S|B|Calls to leader and Peter.|LEADER,NAME7,W1|
|2680 S|Peter|Questions the hesitation.|N,W1|
|2681 S|B|Hesitates despite agreeing.|W1|
|2682 S|C|Objects to going immediately.|N,W1|
|2683 S|Peter|Asks why.|W1|
|2684 S|C|Attributes the objection to herself.|W1|
|2685 T|Peter|Asks her identity.|W2|
|2686 S|Peter|Suspects a devil.|W1|
|2687 S|C/C0|Introduces Cameela as a Devil Army leader.|N,N,W1|
|2688 S|B|Asks her purpose.|W1|
|2689 S|C/C0|Demands the leader's Jewel of Evil.|LEADER,N,W1|
|2690 T|B|Claims Zeon directed them to the Ancient Tower.|N,N,W2|
|2691 S|B|Refuses to hand over the jewel to her.|N,N,W1|
|2692 S|C/C0|Begins disputing those orders.|W1|
|2693 S|B|Threatens consequences from Zeon.|N,W1|
|2694 T|C/C0|Expects forgiveness after obtaining it.|N,W2|
|2695 T|C/C0|Offers to spare Astral for the jewel.|N,N,W2|
|2696 S|C/C0|Threatens the older man.|N,W1|
|2697 T|B|Claims the jewel cannot be removed from the leader.|N,LEADER,W2|
|2698 S|B|Challenges her ability to kill him.|N,W1|
|2699 S|C/C0|Affirms the threat.|W1|
|2700 S|B|Defies her.|W1|
|2701 S|C/C0|Reacts to B's flash.|W1|
|2702 S|B|Comments on her evading his fireball.|N,W1|
|2703 T|C/C0|Acknowledges the wizard's strength.|W2|
|2704 T|C/C0|Admits the jewel would be difficult to obtain.|N,N,W2|
|2705 S|C/C0|Leaves it with the leader for now.|N,LEADER,N,W1|
|2706 S|B|Calls her back.|W1|
|2707 S|C|Warns about Geshp and Zalbard.|N,W1|
|2708 S|B|Notes her departure and proposes Tristan.|N,LEADER,N,W1|
|2709 S|Peter|Requests an explanation.|N,W1|
|2710 T|B|Questions Peter's understanding.|W2|
|2711 T|B|Attributes the setup to Cameela.|N,W2|
|2712 S|B|Calls it a trap for the Jewel of Evil.|N,W1|
|2713 S|Peter|Infers B knew her nature beforehand.|N,N,W1|
|2714 T|B|Confirms his suspicion.|W2|
|2715 T|B|Claims her aura was obvious and recalls her words.|N,N,W2|
|2716 T|B|Infers the devils act independently of Zeon.|N,W2|
|2717 S|B|Urges caution about the danger.|N,W1|

There is no Yes/No, inventory Jewel removal/award, damage, setStoryFlag,
new Tristan unlock or after-script SaveGame. All57 before/after nominal
texts and their ordered tokens are included above; service/skip state can
prevent actual display/cursor advance. The spoken itinerary does not itself
warp to Tristan.

## Actual return consumption and the two exits

**Confirmed:** `sf2cutscenemacros.asm:followEntity` emits opcode word2C and
three operand words: follower, leader and relative slot. Before resolving
the full follower word, `mapscriptengine_1.asm:csc2C_followEntity` reads only
`move.b (a6),d0`, its **high byte**, and calls
AdjustScriptPointerByCharacterAliveStatus with D7=6. All three after28
followers129/31/7 encode a high byte0, so **each call tests live Bowie HP**.
HP0 makes the helper advance A6 past the six operand bytes and discard
the handler's helper-return address; AddFollower is skipped and map-script
execution continues. Nonzero Bowie HP permits full-word alias resolution
and AddFollower with offsets2=(-24,0),1=(0,-24),3=(0,24): Caravan129 follows
Bowie, B follows Caravan, Peter follows Caravan. Peter HP0 alone does not
skip his follow call. Each call samples Bowie independently; a change to0
between calls preserves earlier installed relationships and skips the reached
later ones. These are current entity actions/relationships, not future
flag-driven population changes. `csc07_warp` writes event word1,
PARAM1=0,PARAM2=255,PARAM3=16,PARAM4=30,PARAM5=Right separately; it does not
load another map or unwind the battle call.

BattleLoop's victory suffix reads those bytes directly and returns words
255/16/30/0,D4=1. It has not called ProcessMapEvent. All completed28 battle
returns reach MainLoop's `alt_MainLoopEntry`: SwitchMap then ExplorationLoop,
**without CheckBattle or a D4-sign test**. SwitchMap does not resolve255 or
substitute47. This is why the posted event is not a second queued transfer:
ExplorationLoop first clears MAP_EVENT_TYPE, then consumes the returned seed.

After clearing the pending event, ExplorationLoop subtracts20000 from
STEP_COUNTER with its signed-word fallback, heals eligible allies and fades
out. The CURRENT255 arm waits for fade, updates
main position/destination to16,30 and facingRight. With ordinary PLAYER_TYPE0,
DeclareRaftEntity first also places the flag-built EXPLORATION_ENTITIES at
those coordinates (before its F64/map raft tests); it does not promise all
scene positions remain intact. It then declares raft under its gates.
It retains CURRENT_MAP47, **CURRENT_BATTLE28**, existing scene
actors/aliases/follow programs, temporary flags and working blocks/layout.
It does not reconstruct the default population or reset current battle255.
LoadMap with negative map reloads current area/view data and retains working
layout. The battle crop remains applicable because current battle is still28.
Map47's void setup skips init. Music/fade-in are conditional on palette
comparison; WaitForEvent installs physical0 control only with no pending
event, replacing main's action. Following NPCs and services can still progress.
This establishes conditional input entry, not usable/presented natural control.

Held528 victory instead returns explicit47/member coordinates/facing. That
arm writes CURRENT_BATTLE255, reloads full map blocks/layout, rebuilds main
and live flag followers, clears temporary256–383, sets80, and skips absent
47 init. There are no fixed NPCs: the woman/Cameela/projectile and explicit
scene Caravan disappear; F65 alone adds no Caravan on non-overworld47.
Main remains Bowie even if another active member supplied the seed. Defeat/
retreat use their actual explicit savepoint seed; never assume47/Tristan.

| Supplied case | Observable static distinction |
| --- | --- |
|428 held,478 clear,88 clear|Admit28; before scene, then fresh roster/load; start sets478 with empty script.|
|428 held,478 held,88 clear|Admit and initialize; skip before/start; clear528 can still load neutral woman.|
|428/528 held|Admit this time while clearing428; no enemy upgrade; skip neutral/after; shared join/flags still run.|
|88 held|Supplied selectors/queue branch; no incoming47/28 repair, fresh healing or hooks.|
|Clear528 victory after completes|Warp bytes select retained CURRENT47/28 at16,30; scene actors retained, with follow relationships only where each call's Bowie-HP precheck admitted installation.|
|Held528 victory|Member seed selects fresh explicit47/255 population; no Cameela scene.|
|Dead/fewer/full/empty party; changed HP/mapping|Placement, seed, count/control, follow and join behavior differ; no fixed12 or guaranteed Bowie mapping.|
|Bowie HP changes between final follow calls|Each high-byte0 precheck samples Bowie again; HP0 skips that call without undoing earlier installations or stopping the scene. Peter HP0 alone is not this predicate.|
|Changed current map/crop/savepoint/parameters|Each later consumer reads live values; no repaired coordinates/caller/forced town.|
|Interrupted text/action/fade/join/flags or pending overwrite|Only reached prefix is established; no rollback/completed suffix promise.|

`data/maps/entries/map47/6-warp-events.asm` has exactly two rows, in this
order, both no-scroll:

| Source47 | Explicit destination74 | Facing |
| --- | --- | --- |
|25,negative wildcard Y|53,31|Down3|
|0,negative wildcard Y|49,31|Down3|

They correspond to the far/near sides beside74's52,31 and50,31 entrances;
they do not promise a reversible one-step route. The same layout-kind/
candidate-coordinate/no-scroll/actual-sample rules apply. No528 gate in the
warp rows prevents either exit before combat when428 is clear. On consumed
explicit74 return, MainLoop scans battles: held427 selects27 before held432
selects32, both crop2,16,32,48; completion can clear the selected unlock while
still admitting. With neither, explicit74 builds fresh overworld exploration
with empty fixed list and RTS init, current battle255, flag followers and
possible F65 Caravan prefix. No automatic zone61 callback runs on that load.

The separate74 `(52,20)` no-scroll row requests Tristan15 `(31,30)` Up.
There is no15 switch/battle row, so an intact request admits exploration;
the [accepted Tristan owner](./tristan-entry-interactions-and-exits.md) owns
its body/interactions. After-dialogue directions do not post this request,
prove navigation from an exit, or make bridge victory its tested prerequisite.
Stop at these traced47/74/15 admission connections; do not replay Tristan,
35→0/29 or extend into later chapters.

## Actual production consumers and remaining boundaries

**Confirmed by current code inspection:** `src/sf2tool/remake_exploration_battles.py:prepare_encounter`
admits only2/3. `PrivateBattleScenarioReader.Assemble` admits1/2/3 at the
region-program boundary even though28 has no region script; its commandset
conversion accepts0/6/7/9/14, not this Monk's2 or SENTRY8. Mover conversion
also lacks MAGE/STEALTH_ARCHER. This is no ready Battle28 import. Source
formation capacity and live force assignment need conversion rather than
assuming row ordinal is the fixed ally identity in a controlled package.

| Surface | Actual capacity / Battle28 gap |
| --- | --- |
|Program producer/Content types|`remake_exploration_programs.py` maps selected scene-map/population, position/facing, animation, priority-as-bool, quake, camera, flash/presentation, follow, inline actions and queued warp. Reader constructs their typed instructions. Map47, all sprite resources, scene aliases, HP gates, text/name/portrait controls and actual caller lifecycle need admitted conversion and validation. Types do not prove this scene executes.|
|Entity orientation/refresh|Producer explicitly lowers ac_orient to FLAGS_B mask3 and ac_updateSprite to refresh-sprite, separately from face; current code agrees with source distinction. Size/resize/actions have explicit representations or native boundaries; do not equate declaration to exact original presentation.|
|Follow precheck|Producer's followEntity branch emits follower/leader IDs and table-derived offsets without the original high-byte Bowie-HP precheck. Content reads a typed FollowEntity; ProgramRunner resolves aliases then calls FollowerMotion.Install, which installs the relationship/motion settings without an HP test. These consumers do not implement the source's per-call skip/continue distinction. No engine correction or lowering is included here.|
|Native/Unsupported|Unmatched source operations emit native-call; action lowering has UnsupportedEntityAction. Reader maps native-call to UnsupportedInstruction; ProgramRunner rejects before advancing/committing that instruction while retaining earlier prefixes. Scene maps are selection-gated. This account has run **no** lowering, so it does not invent a fresh per-command native/compile result or label an unexecuted suffix accepted.|
|Battle Domain/Application|Current typed actors, orders (including FOLLOW_TARGET0), regions, agility and bounded outcome primitives can express parts. They do not bypass producer/reader admission, supply all HEAL4/DISPEL/ATTACK/BOOST/FLAME AI decisions, implement this SENTRY/Monk repertoire or authorize leader cleanup. ReadOutcome explicitly notes leader cleanup/nonempty defeated programs need their supported seam; empty script alone does not implement the shared28 leader tail.|
|Outcome/map caller|After/join/return primitives and retained/rebuilt map operations need the actual victory PARAM overwrite, CURRENT retaining CURRENT_BATTLE28 and scene aliases, repeat seed, live savepoint/retreat and ordinary Suspend semantics. ProgramRunner/MapTransfer's finite accepted early scenarios are not proof of this new late caller composition.|
|Presentation/content/trust|No47 material package/asset import, bridge actor binding, Godot state/input observation, natural original run or hardware/persistence claim exists here. Public builds still require authored/licensed content. No allowlist/engine/parser repair is in scope.|

Production paths above are under `remake/src/Sf2.Remake.Content/Scenarios/`,
`Sf2.Remake.Application/Runtime/Exploration/`, and the Domain battle owners.
The [coverage consumer distinction](./story-content-coverage.md#cross-chapter-consumption-gaps)
and [capability owner](../../remake/docs/capability-status.md) retain actual
early acceptance. Static28 research moves neither English design nor remake
readiness. Full original move/spell choices, natural incoming flags/HP/party/
RNG, legal collision routes, service completion, rendered/audible timing,
usable control and durable save behavior remain **Unknown**.

## Reproduction and review boundary

Load ignored `local/private-inputs.ps1` in the same process as every uv command.
Use the configured read-only pin and existing source/data/consumer owners.
The ignored `local/issue1198/review.py` and semantic review in its handoff
provide complete dependencies and fresh-receipt commands. Directly compare
all formation/enemy/region/point records, hook/neutral/resource rows, both
full programs and57 text tokens with their actual handler/caller use sites;
review changed valid states, not just a fixed endpoint or keyword presence.
These finite checks are not a maintained parser or all-game simulator.

Check full prose/links/anchors, strict UTF-8/LF, exact three-path scope,
unchanged fixture/index counter table, private boundary and cached diff.
On clean committed HEAD run `uv run sf2 verify plan --base origin/main --head HEAD`
and record actual exact-head scoped CI. Explicit docs scope runs no normal/
full/.NET/Godot/H3/H4/emulator/screenshots, old comparison, acquisition,
lowering, capture, extraction or pair rebuild. No verification-tool tests.

Retain1196's original5.043489s PASS omission and corrected main0.277633s PASS,
full original/correction handoffs, PR504's accepted original victory/stable
return and PR526's later ordinary Down input/effect,1194/earlier completed
failures/partial/native/Unknown records and #1047's twelve unresolved
comparisons. They are distinct evidence, not new passes or a rerun queue.
This account closes28's declared **static content boundary** only;14/15/44,
optional content, all-AI and natural continuity gaps remain with coverage.
