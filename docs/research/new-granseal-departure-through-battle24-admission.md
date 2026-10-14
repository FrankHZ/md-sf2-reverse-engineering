# New Granseal departure through Battle24 admission

Evidence date: 2026-10-14. **Confirmed:** the selected town south warp enters
Map73, and its east border scrolls into Map72 without fresh battle selection
or population/init. A later default zone callback chooses Battle24 when F523
is set, but a produced request and ordered admission are still necessary.
Fresh Battle24 selects empty before/start hooks and configures twelve ally
placement slots, sixteen enemies, three regions and one point before ordinary
activation. **Inferred:** this formation stages the approach to North Parmecia.
**Unknown:** natural travel, supplied state, completed services, presentation,
playable control and save persistence.

## Agreement and provenance

Standard rigor under [Issue #977](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/977)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
retained border scrolling, request production, ordered admission and initial
combatants have distinct acceptance boundaries. This document alone is owned;
no shared changes or unmerged dependencies. The accepted starting base is
`cdbcccba0f5c33f8feb07313ba43f690c6a81ec7`.

Accepted dependencies are [Janet/departure](./new-granseal-janet-recruitment-and-departure.md),
[inventor/Dynamite](./new-granseal-inventor-and-dynamite-gift.md),
[Astral following](./new-granseal-return-and-astral-following.md),
[Luke/Map72 entry](./polca-departure-through-luke-recruitment.md),
[earlier Map72 admission](./east-shrine-return-through-battle11-admission.md),
[Caravan/movement](./caravan-boarding-and-southwest-parmecia-travel.md),
[request state](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md),
[battle loop](./battle-loop.md), [spriteset data](./battle-spriteset-data.md)
and [shared roster consumers](./battle23-initialization-and-roster.md).
Stop at the fresh start-dispatcher return immediately before ActivateEnemies.
No combat/outcomes/Egress, North Cave blast or missing-item scene audit,
Rohde joining, Battle25, later journey, runtime/acquisition, layout extraction,
new fixture/framework, engine or UI work is included.

Paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Existing fixtures/retained payloads
bind USA retail SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Constants are from `sf2enums.asm`, RAM from `sf2const.asm`; Fnnn means flag nnn.
Rows, battle indices and placement indices are zero-based; intervals are half-open.
Minimum evidence is selected source, public fixtures and the unchanged
18,285,979-byte map-init/script pair. Linear comparison is budgeted below one
minute/512 MiB with small prose/receipts. No regeneration or installation.
Replan material contradiction or expanded machinery; Unknowns are not a runtime queue.

## Selected warp, scroll and population

**Confirmed:** `data/maps/entries/mapNN/6-warp-events.asm` supplies:

| Source / row | Match | Requested tuple |
| --- | --- | --- |
| Map6 /1 | `(255,31)` | no scroll; Map73 `(29,53)`, Down |
| Map73 /1 | `(63,255)` | scroll Right; Map72 `(1,255)`, Right |

Map6 row0 is the castle Y0 warp; row2 `(31,14)` has the same destination as
row1 but is a separate row, not evidence of usable terrain. Map73 row0
`(29,52)` returns to town `(20,30)` Up; row2 `(255,63)` scrolls south to Map69.
At `(63,63)`, east row1 wins over south row2. Negative coordinate bytes are
wildcards in the warp scanner. No selected row tests Janet, joined/active
membership, Cannon/Dynamite, F805/F806, F523 or an earlier conversation.

`entityscriptengine_2.asm:esc02_controlCharacter` needs the selected layout's
masked `$1000` kind to invoke `exploration.asm:WarpIfSetAtPoint`. The scanner
divides supplied internal coordinates by MAP_TILE_SIZE384, scans eight-byte
rows, writes event1, parameters1–4, facing/padding at parameter5 and WARP_SFX.
Coordinate matches do not prove a walk, traversable layout or an intact request.
The shared pending slot permits overwrites; ProcessMapEvent clears its type
before dispatch, not every parameter.

`explorationfunctions_2.asm:ProcessMapEventType1_Warp` with parameter1=0
submits/clears WARP_SFX, idles physical0, unwinds exploration, samples its tile
position and replaces nonnegative requested coordinate **low bytes**. Facing
comes from parameter5 and d4=0. Ordinary in-range sampled high bytes are a
premise, not normalized by the table. MainLoop calls SwitchMap then CheckBattle.
Neither Map73 nor72 has a flag-switched replacement, and no battle-map row is
on73. Thus an intact town request can reach ordinary Map73 exploration.
Neither warp branch clears PLAYER_TYPE.

The east scroll instead sets CURRENT_MAP=72, calls
`mapload.asm:ProcessMapTransition`, updates physical0 current/destination X,
retains Y for `$FF`, samples current facing and calls DeclareRaftEntity. Its
Right facing byte is **not** applied. Transition loads blocks/layout/selected
tilesets and performs the selected overworld scrolling service; it does not
rebuild residents/followers. Requested X replaces only d0's low byte before
word multiplication by384: the nominal X1 result assumes the returned upper
byte is zero. Y's register is explicitly cleared before testing its byte.
This branch returns inside exploration, without MainLoop, CheckBattle,
fresh population, healing, temporary-flag reset or map init. Current actors
and changed follower flags need not agree with a hypothetical fresh Map72.

Both maps have only default `mapsetups.asm` rows; their pointer tables select
empty `s1_entities.asm` lists and their own zone/init tables. Empty residents
still permit main, flag followers and a separate raft. Fresh explicit Map73
entry clears pending, subtracts20000 from STEP_COUNTER as a word and clamps a
negative result to0, heals living/immortal allies, sets current map and
CURRENT_BATTLE255, loads tiles, rebuilds entities, clears temporary F256–383
and sets F80 before load/init. Persistent story/party flags survive.

`InitializeMapEntities` and `InitializeFollowerEntities` construct enabled
followers from physical1 upward. Both maps are overworld maps: F65 selects the
Caravan prefix, followed by the ordinary follower table; otherwise only the
ordinary table applies. Rows include F67/Peter, F75/Luke, F69/Fairy and the
F74/F68/F70 declarations of FOLLOWER_B as Astral/Oddler/Astral. Later enabled
rows can overwrite a logical mapping without removing an earlier physical
actor. Other enabled rows affect order/capacity. No row declares Janet.
Consequently town Janet's current following does not survive this fresh
Map73 rebuild merely because joined18/active50/F806 are set. F805 does not
recreate Astral without the relevant follower flags. Force membership and
map followers are different authorities.

Fresh `followersfunctions_2.asm:sub_44404` uses raft-player mode2 to change
main's sprite, stand followers and declare the separate raft at `(64,64)`.
Otherwise F64 plus CURRENT_MAP=RAFT_MAP permits its stored position, masked
by `$7F`, physical31/lookup offset63, Left. FOLLOWER_B's lookup31 is different.
The scroll/current-map consumer `mapfunctions.asm:DeclareRaftEntity`
returns for mode2; mode1 skips ordinary follower relocation, while other modes
relocate the current exploration follower list before the F64/map raft gate.
Mismatch hides the separate raft; it does not reconstruct followers. These
are retained mode/raft-state consequences, not evidence of boarding/landing.

Map73 init `ms_map73_InitFunction` (`$50342`) has complete history discrimination:
F700 clear calls cs_503A6, then sets700 and returns; with700 set, F508 clear
returns; F508 set/F705 clear calls cs_50806, sets705 and returns; F705 set
returns. The earlier ship/town scenes are alternate supplied histories, not
newly audited here. Map73's sole default zone callback is rts.
The east scroll skips Map72 init. A later ordinary Map72 entry or CURRENT255
no-battle reload can run `ms_map72_InitFunction` (`$4FF2E`): F716 clear or Luke
joined10 returns; otherwise it runs accepted cs_4FF5A. CURRENT255 retains
population/temp flags but still loads/runs init; fresh explicit72 rebuilds
and resets them. No F805/F806/Janet assumption eliminates these alternatives.

## Map72 zone selection and actual request production

**Confirmed:** the complete `map72/mapsetups/s3_zoneevents.asm` table
`$4FE64..$4FF24` selects one callback, with no default fallthrough:

| Ordered rows | Coordinate predicate | Selected callback |
| --- | --- | --- |
| 0–2 | X32,33,34; any Y | Map72_ZoneEvent0 → CheckRandomBattle26 |
| 3–8 | X25,26,27,28,29,30; Y37 | Map72_ZoneEvent3 → next cave boundary |
| 9 / default | `$FD` marker | Map72_DefaultZoneEvent: F523 clear →8; set →24 |

A declined specific callback does not run default afterward. Default is
selected only after a zone event reaches the dispatcher. Walking's masked
`$1400` branch writes event6 and two coordinate words at parameters1/3 before
its raw-layout blocking test. Admitted movement subsequently sets destination,
velocity/travel and decrements STEP_COUNTER only when signed-positive. A
blocked movement tail can therefore leave a zone request without a step.
Mode1 selects eas_Raft/opcode7 (source label esc07_controlRaft), while mode2
selects eas_Caravan/opcode8; these source names differ from the numeric
Caravan1/raft2 mode constants. Neither controller uses this walking
zone producer. Mode0, actual working layout, movement admission and pending
survival remain premises; coordinates alone do not call a default every frame.

`ProcessMapEventType6_ZoneEvent` installs physical0's eas_Init and passes the
two words to RunMapSetupZoneEvent. It compares low coordinate bytes, invokes
the first match/default, closes portrait/text, waits a VInt and waits physical0
position=destination. It does not join all followers. eas_Init does not zero
existing position/destination/travel. WaitForEvent restores camera0 and the
mode-selected controller only if pending is zero.

The cave callback's complete admission branches are a stopping discriminator:
F750 set, F805 clear or F256 set returns immediately. Otherwise it clears256,
looks up Cannon then Dynamite. Missing either dispatches cs_5023E; both missing
branches actually put ITEM_DYNAMITE in DIALOGUE_NAME_INDEX_1. Both present
attempt Cannon/Dynamite removal then dispatch cs_4FFDA and set750 after return.
Returning admitted paths set256. No F806 or Janet test is present. This records
which next boundary is selected, not the item-search/removal algorithm,
missing-item dialogue, blast, Rohde join or their successful completion.
The selected specific row still suppresses default Battle24 when it returns.

`Map72_DefaultZoneEvent` (`$4FF06..$4FF24`) tests523, calls exactly one of
CheckRandomBattle8/24 and returns. F523 is the actual selector; F524 is a
different flag tested inside the selected24 helper. F805,806,474,750 and items
are not extra predicates. The shared actual helper is
`mapsetupsfunctions_1.asm:CheckRandomBattle` (`$47856..$478C6`):

| For selected Battle24 | Helper result |
| --- | --- |
| F524 clear, any STEP_COUNTER | Request immediately, no RNG call |
| F524 set, nonzero counter, including negative | No request or RNG; leave counter |
| F524 set, zero counter, range8 result0 | Request; no second draw |
| F524 set, zero counter, range8 result nonzero | No request; range4 draw+2 becomes counter2–5 |

GenerateRandomNumber updates its 16-bit seed by multiply13/add7 and scales by
the range; these are arithmetic bounds, not measured independent probabilities.
The request path sets424, writes longword `$000100FF` at MAP_EVENT_TYPE,
sets counter30000, waits view scrolling, submits SFX_BOOST and executes the
flash script. Opcode41 reads operand30, shifts it right3 and uses DBF to
make four LaunchFading calls, then DuplicatePalettes. LaunchFading waits via
VInts for the fading setting to clear; request writes precede these services.
No F524 write or direct CheckBattle call occurs here. Battle8 and26 use the
same algorithm with their own completion/unlock flags.

The longword supplies type word1, parameter1=0 and parameter2=CURRENT255.
It leaves parameters3–5 and WARP_SFX intact. In the ordinary just-produced
zone request, parameter3/4 are the high/low bytes of Y and parameter5 is older
state: this is not an implicit all-255 position/facing tuple. A normal small
Y word can therefore yield requested X0, Y=zone Y. Other writers can supply
other residuals. The no-scroll consumer samples main, applies surviving bytes
and returns to MainLoop. The partial request neither guarantees successful
reload nor carries an immutable battle identity.

## Ordered admission and fresh-versus-continued entry

**Confirmed:** `getbattle.asm:CheckBattle` resolves CURRENT255 for the scan,
then tests map, unlock and trigger in source order. Map72 has exactly:

| Battle row | Area output `(X,Y,width,height)` | Trigger | Unlock / completion |
| --- | --- | --- | --- |
| 8 | `(1,32,32,30)` | `(255,255)` |408 /508 |
| 24 | `(1,32,32,30)` | `(255,255)` |424 /524 |
| 26 | `(28,0,36,32)` | `(255,255)` |426 /526 |

First unlocked row wins, irrespective of residual position or rectangle
containment. Row8 can intercept the helper24 request. With408 clear/424 set,
24 wins even if426 is set; with both clear,426 can select26. No unlock means
ordinary exploration, not a mandatory battle. A matched completion flag
clears its unlock **after selection** and still returns that battle on this
call; it does not continue scanning. A supplied424 can admit24 independently
of F523, the default callback, Janet, items or the narrative route. A different
current map can select different rows or none. Clearing424 or overwriting
pending before consumption can break the producer-to-battle connection.

Battle24 outputs Map72 origin `(1,32)`, size32x30. `LoadMapBlocksAndLayout`
uses that crop with CopyMapBlocks; LoadMapArea supplies local bounds. Roster
coordinates below are battle-local, stored directly by SetCombatantX/Y, not
absolute overworld tiles: local `(x,y)` corresponds to Map72 `(x+1,y+32)`.
Neither CheckBattle's wildcard trigger nor the empty scripts place main at
the request coordinates. MainLoop passes selected map72/battle24 to BattleLoop.

`battleloop_1.asm:BattleLoop` first clears PLAYER_TYPE and sets399. With88
clear it clears seconds; saves incoming words, assigns current map/battle,
sets base VInts and invokes before. On return it restores/reassigns map/battle,
clears region flags90–105, heals allies, initializes allies then enemies,
clears AI memory, loads battle for combatant0 and invokes start, in that order.
There is no exploration population/init call between selected battle and roster.
No completed prefix is rolled back if a later wait/service fails.

Both 48-word tables in `data/battles/cutscenes/beforebattlecutscenes.asm` and
`battlestartcutscenes.asm` select **ms_Empty** at slot24/byte offset48.
`code/common/scripting/map/ms_empty.asm` is the single `$FFFF` terminator at `$47A4E..$47A50`.
The actual before/start wrappers test intro flag450+24=474: before dispatches
when clear without setting it; start sets474 **before** dispatch. Set474 skips
both hooks while the fresh heal/placement/load still runs. ExecuteMapScript
clears its skip latch and consumes the terminator; its tail conditionally waits
scrolling if a dialogue window remains, clears VIEW_SCROLLING_SPEED and returns.
There is no Battle24-local dialogue, opening scene, join or scripted movement.

F88 set is a different entry: restore saved seconds, clear88, clear AI memory,
set view target0, LoadBattle using supplied current state, then jump directly
to the individual-turn loop. It skips incoming map/battle assignment, before,
region reset, healing, placement and start; the following @Start branch is
unreachable. The fresh roster below is not asserted for a supplied suspension.

## Complete ally slots and enemy configuration

**Confirmed:** `data/battles/spritesets/entries.asm` slot24 selects
`spriteset24.asm` / BattleSpriteset24 at `$1B4D2C..$1B4EA6`, 378 bytes: header4, 28 records of12,
three regions of12, one point of2. Header counts are12/16/3/1; subsection
byte offsets are4/148/340/376. GetBattleSpritesetSubsection uses CURRENT_BATTLE*4
and preceding counts/strides, not names inferred from file order.
Macros allyCombatant/enemyCombatant, combatantAiAndItem and combatantBehavior
encode type/index,X,Y,AI byte; item word; primary order/region bytes; secondary
order/region bytes; filler/spawn bytes. All rows have filler0 and STARTING0.

All twelve ally rows have HEALER1=0, NOTHING=`$007F`, primary/secondary
NONE=`$FF`, regions15/15. Their complete placement index/local coordinates are:

| Index | X/Y | Index | X/Y |
| --- | --- | --- | --- |
|0|`(4,18)`|6|`(2,19)`|
|1|`(3,17)`|7|`(2,16)`|
|2|`(3,18)`|8|`(3,16)`|
|3|`(2,18)`|9|`(1,16)`|
|4|`(3,19)`|10|`(1,17)`|
|5|`(2,17)`|11|`(1,18)`|

InitializeAllAlliesBattlePositions resets all thirty allies' X=-1, calls
UpdateForce, then reads the rebuilt joined-and-active list in ascending ally-ID
order. The first twelve **processed members**, not twelve guaranteed living
heroes, bound placement. A zero-HP member consumes a loop slot without advancing
the placement pointer; the next living member uses that unconsumed coordinate.
Surplus members LeaveBattleParty (active flag clear, X=-1; cached list/count
not immediately rebuilt). No initial empty-list guard precedes the first read.
Only placement X/Y is copied: the ally row index/item/AI fields do not replace
ally identity, inventory or AI. Janet's recruitment does not guarantee active
membership, health or a particular slot; current map followers do not enlist.

Healing already visited all30 independently of membership: living allies and
Peter7/Lemon28 get HP/MP maxima, status masked to existing STUN/POISON/CURSE,
then UpdateCombatantStats. Other dead allies skip. Equipment/stat rebuilding
can affect the final status; there is no blanket revival or roster replacement.

The complete sixteen enemy records follow. All secondary orders are NONE;
regions are shown primary/secondary. AI values are LEADER14, ATTACKER1/2/3=
6/7/9 and HEALER3=2. Primary MOVE_TO0=`$40`, FOLLOW_ENEMYn=`$80|n`,
FOLLOW_TARGET0=`$00`, NONE=`$FF`. Item words are the **authored source words**.

| Row / combatant | Type (ID) | Local X/Y | AI | Primary order | Regions | Item word |
| --- | --- | --- | --- | --- | --- | --- |
|0 /128|DARK_BISHOP81|`(25,7)`|LEADER|MOVE_TO0|1/15|NOTHING `$007F`|
|1 /129|DARK_SNIPER62|`(25,5)`|ATTACKER3|FOLLOW_ENEMY0|1/15|NOTHING `$007F`|
|2 /130|SKELETON17|`(24,8)`|ATTACKER3|FOLLOW_ENEMY0|1/15|MEDICAL_HERB `$2000`|
|3 /131|LESSER_DEMON45|`(28,10)`|ATTACKER1|NONE|1/15|NOTHING `$007F`|
|4 /132|DARK_SNIPER62|`(18,13)`|ATTACKER1|NONE|1/0|NOTHING `$007F`|
|5 /133|SKELETON17|`(21,14)`|ATTACKER1|NONE|1/0|MEDICAL_HERB `$6000`|
|6 /134|SKELETON17|`(17,18)`|ATTACKER1|NONE|15/15|MEDICAL_HERB `$2000`|
|7 /135|DARK_BISHOP81|`(18,19)`|HEALER3|FOLLOW_ENEMY6|15/15|BLIZZARD `$6011`|
|8 /136|SKELETON17|`(18,20)`|HEALER3|FOLLOW_ENEMY6|15/15|MEDICAL_HERB `$2000`|
|9 /137|LESSER_DEMON45|`(22,25)`|ATTACKER1|NONE|0/1|NOTHING `$007F`|
|10 /138|LESSER_DEMON45|`(21,26)`|ATTACKER1|NONE|0/1|NOTHING `$007F`|
|11 /139|SKELETON17|`(11,21)`|ATTACKER1|NONE|15/15|MEDICAL_HERB `$2000`|
|12 /140|SKELETON17|`(9,25)`|ATTACKER1|NONE|15/15|MEDICAL_HERB `$6000`|
|13 /141|DARK_SNIPER62|`(8,24)`|ATTACKER3|FOLLOW_ENEMY12|15/15|NOTHING `$007F`|
|14 /142|LESSER_DEMON45|`(14,3)`|ATTACKER1|NONE|1/2|NOTHING `$007F`|
|15 /143|LESSER_DEMON45|`(8,8)`|ATTACKER2|FOLLOW_TARGET0|1/2|NOTHING `$007F`|

These combatant IDs follow the actual assignment: InitializeAllEnemiesBattlePositions
scans128–159; InitializeEnemyBattlePosition compares against count16 with bit7
set and indexes each twelve-byte row by the masked combatant index. All sixteen
STARTING0 low nibbles are below HIDDEN2 and call InitializeEnemyStats. Slots144–159
instead get activation0, max/current HP0 and X=-1; their absent path still reads
trigger bytes through the subsection pointer, not a valid extra row. The
Battle32/Jaro exception does not apply. This is not physical map-entity numbering.

InitializeEnemyStats copies the selected 56-byte/14-longword enemy definition,
sets current HP/MP to maxima and row X/Y, packs definition movetype/row AI,
writes enemy index, orders and trigger regions, then initializes items and
activation, adjusts difficulty ATT and calls UpdateCombatantStats. Trigger
storage is primary<<4 | secondary; orders are primary<<8 | secondary.
For STARTING0, activation retains only the definition's `$F000`: initial
DARK_BISHOP gets `$3000`, the other three types `$2000`. Definition BIT4/BIT5
are discarded; active/neutral bits are clear before ordinary activation.

### Fresh definitions and repeat-state distinction

**Confirmed:** with F524 clear, the four complete definitions in
`data/stats/enemies/enemydefs.asm` remain the selected types:

| Type | HP/MP maxima | Base ATT/DEF/AGI/MOV | Movetype | Initial equipped item | Four spell slots |
| --- | --- | --- | --- | --- | --- |
|DARK_BISHOP81|39/38|28/38/27/5|HEALER|FLAIL|AURA, BLAST LV3, NOTHING, NOTHING|
|DARK_SNIPER62|38/0|11/37/35/5|STEALTH_ARCHER|HYPER_CANNON|four NOTHING|
|SKELETON17|30/0|26/40/30/6|REGULAR|BROAD_SWORD|four NOTHING|
|LESSER_DEMON45|40/21|49/37/32/6|FLYING|none|BLAZE LV3, BOOST LV2, NOTHING, NOTHING|

Definitions have initialStatus NONE, three other empty item slots (Demon four),
and retain their level/resistance/prowess/spell-power fields. These are initial
configuration, not damage, spell execution or a claim that zero-MP types cast.
InitializeEnemyItems skips exact NOTHING; otherwise AddItem finds the first
empty slot and masks the incoming word by `$807F`. Thus Skeleton herbs store
`$0000`, Bishop row7's Blizzard stores `$0011`, stripping `$2000` USABLE_BY_AI
and `$4000` UNUSED_ITEM_DROP. The copied equipped weapon remains in slot0;
these additions occupy slot1 for the fresh types. The initializer tests the
post-AddItem word's equipped bit, so none of these additions gets equipped.
Authored item flags do not establish stored AI-use/drop behavior.

The difficulty helper's consecutive beq branches mean all difficulties apply
floor(baseATT*5/4), SUPER2 twice: Bishop35/43, Sniper13/16, Skeleton32/40,
Demon61/76 (ordinary/SUPER). These are initialized **base** ATT values;
UpdateCombatantStats subsequently applies equipped-item/current-stat effects.
Do not report them as final attack or damage; equipment is part of the encounter.

Unlike Battle23,24 is in the eleven-entry list_RandomBattles. The immediate
UpgradeRandomBattleEnemies call first checks list membership then completion524.
F524 clear returns original type without an upgrade draw. F524 set permits the
actual level/class/RNG-dependent upgrade path before copying definitions, even
when supplied424 admitted directly. It uses Bowie's effective level (class
above the last unpromoted adds20), subtracts battle index24 and uses a quotient
by10; nonpositive or zero quotient returns original. Higher values use the
movetype-specific range/exclusion table and RNG, with retry/default branches.
The airborne table has an empty exclusion list; the consumer subtracts1 and
enters its scan without a zero guard. This is not a guaranteed successful or
uniform upgrade. No single repeat enemy definition, stats, equipment or service
completion is asserted. Row coordinates/AI/item inputs remain the selected
spriteset, while copied definition and resulting initialization can differ.

### Regions, physical actors and stopping point

**Confirmed:** all three twelve-byte region records have type4, byte1=0,
ordered vertices below and trailing0/0; the single point0 is `(25,7)`:

| Region | Vertices in stored order |
| --- | --- |
|0|`(9,18),(13,29),(31,29),(31,18)`|
|1|`(19,0),(10,18),(31,18),(31,0)`|
|2|`(0,0),(0,10),(10,17),(19,0)`|

These are local coordinates and stored trigger/order inputs. Shared region
and AI consumers distinguish polygon tests, region15 sentinel and point/follow
orders. IsRegionTriggered tests type4 through triangles1/2/4 and3/2/4; the
second/trailing bytes have no geometry meaning assigned here. GetMoveOrderPosition
uses MOVE_TO bit6 and the low nibble to index two-byte point0, while follow
orders read the referenced combatant: FOLLOW_ENEMY0/6/12 refer to128/134/140,
FOLLOW_TARGET0 to operand0. None has been executed at this endpoint.
Clearing F90–105 and storing STARTING0 does not itself claim an enemy is already activated or a tactic fired.

`entityfunctions_1.asm:PositionBattleEntities`, called by LoadBattle, clears old
map entities and rebuilds them. It scans allies0–29, skipping negative X/Y;
regular enemies follow in combatant order, skipping neutral/negative coordinates.
It writes logical-to-physical lookups; normal physical slots are contiguous
across **actually admitted** actors, not fixed128–143. Fresh selected enemy
mapsprites145/126/81/109 are below the special-sprite threshold; the separate
special-slot path is not used for these types. There is no Battle24 entry in
`data/battles/global/battleneutralentities.asm`. Physical actors use local
coordinates masked by `$3F`, scaled384, facing Down and eas_Standing; the
exploration followers and Janet's temporary actor are replaced.

LoadBattle selects combatant0's mapped view target, initializes sprites,
loads map/mapsprites and terrain, restores base VInts, requests map music and
fade-in, then returns. An absent combatant0 is not repaired into a valid roster
by this load. The Battle44 timer branch does not apply. The empty start wrapper
then returns. **Stop immediately before BattleLoop:@Start calls ActivateEnemies**;
no turn generation, actions, victory/defeat/Egress or playable acceptance is claimed.

## Changed-state review and reproduction

| Changed state / interrupted prefix | Consequence |
| --- | --- |
|No Janet/items/805/806, but route and later battle premises hold|Selected route/default24 still admits; those facts are not prerequisites.|
|F700 clear or700/508 set with705 clear|Map73 alternate-history init can interrupt before selected east travel.|
|East scroll with408/424/426 set|Scroll itself does not select any battle; a later no-scroll request can.|
|Specific26/cave row declines|No default24 fallthrough, even with523 set.|
|523 clear/524 clear|Default chooses8, not24; independent supplied424 remains distinct.|
|524 set, counter negative/nonzero|Helper24 declines; walking only decrements a positive word.|
|424 produced while408 already set|Next Map72 CheckBattle selects8;424 can remain for later.|
|424/524 both set,408 clear|Select24 and clear424 on this call; completed state also enables upgrade logic.|
|All Map72 unlocks clear|Current-map reload retains actors/temp flags but runs init;716/10 can select Luke.|
|474 set with88 clear|Skip empty hooks; fresh healing/roster/load still runs.|
|88 set|Current suspended state wins; no fresh roster guarantee or start-dispatcher endpoint.|
|Dead/reserve Janet, altered active list, >12 active|Placement follows rebuilt membership/HP and twelve processed-member limit, not scene order.|
|Request/flag write completes but flash/wrapper does not|Pending and424 may exist without MainLoop admission; no transaction/rollback.|
|Start sets474 but termination wait stalls|Flag set does not prove completed load/control or persisted encounter.|

Reproduce from the owning checkout with ignored `local/private-inputs.ps1`
loaded in the same process. Use the registered read-only upstream; verify its
HEAD and clean status. Reuse the [retained-pair recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
(map-init equality, _verify_map_script_engine_fixture and _cutscene_facts),
with size/mtime unchanged. Do not regenerate extraction or replay runtime.

1. Join complete selected warp rows, default setup pointers/empty lists and
   both init bodies against source and retained map-init. Preserve Map73's
   full700/508/705 ordering and Map72's716/10 branch. Existing map-data/setup,
   map-events/routing-setup and zone-programs fixtures own their structural facts.
2. Join every Map72 zone row and all three callback bodies, Map73's return,
   and the random-battle caller/body fixture. Read actual walking/mode/warp,
   zone wrapper, pending consumption and CheckBattle bodies; retain partial
   widths, residual parameters, first-match priority and completed-clear order.
3. Join selected Map72 battle rows and both slot24 cutscene targets to the
   empty body/actual wrappers. Join spriteset slot24, header, every ally/enemy
   record, region/point, macro layout and four fresh enemy definitions. Existing
   battle-spriteset/routing/cutscene fixtures locate these sources; aggregate
   counts alone do not prove the detailed roster or effective targets.
4. Read the actual subsection, healing/membership/placement, enemy assignment,
   upgrade, stats/item setters, physical population and LoadBattle consumers.
   Compare fresh/completed/suspended, clear/held/mismatched flags, mode,
   dead/reserve/capacity and interrupted-prefix states above. No new tests of
   verification tools or tracked comparison infrastructure are needed.
5. Check full prose/diff, local links, UTF-8, sole-path/private boundary and
   git diff --check. On the clean committed head run
   `uv run sf2 verify plan --base origin/main --head HEAD`; record actual head CI
   and exact identities in the Issue handoff. Documentation-only acceptance
   does not launch blanket normal/full/engine/Godot/H3/H4 suites.

**Unknown:** natural route/layout/event production; supplied flags, mode, party,
HP/classes/levels/RNG, mappings and valid capacity; request survival and completed
scroll/follower/flash/fade/DMA/music/input services; actual repeat upgraded roster,
visible initial encounter, usable control and save/load persistence. These do
not weaken the static conditional findings or authorize acquisition.

Preserve [#973 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/973#issuecomment-6180052225)
and [PR975 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/975#issuecomment-6180065642):
head3e907b8f/tree5bc75a39/base36b0026b, CI38648632852, distinct author0.093s
and main-gate0.096s pair receipts and the two corrected direct-check failures.
Their linked #969/#965 results, Battle19/32-byte-stride and #954/#962 corrections,
earlier failures and Unknowns remain retained. The #977 preflight's nonexistent
`data/maps/battlecoords.asm` was resolved to the actual
`data/battles/global/battlemapcoords.asm`; narrowed reads superseded truncated
inspection output, not completed evidence. No suite failure is reset/reclassified.
Leave a clean pushed Draft PR, frozen writer/completed processes and Project
Review / Verify for independent main-gate integration; no merge or cleanup.
