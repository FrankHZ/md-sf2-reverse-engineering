# North Cliff entry through Battle25 admission

Evidence date: 2026-10-14. **Confirmed:** Map72's opened-entrance warp
requests Map48 `(55,29)`, Up. An intact ordinary request selects Battle25
only with F425 set; completion525 clears that unlock after selection rather
than declining the same entry. Fresh entry uses empty before/start scripts,
twelve ally placement slots, fifteen enemy records and three regions.
**Inferred:** this stages the North Cave encounter after the accepted opening.
**Unknown:** natural traversal, supplied state, completed load/presentation,
usable control and save persistence. Stop at the fresh start-dispatcher return
immediately before `ActivateEnemies`.

## Agreement and provenance

Standard rigor for [Issue #989](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/989)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
opening, request production, ordered battle admission, map loading and combatant
initialization have distinct conditions. Accepted starting base is
`1238d4d5165cb99d124651520bdf0cf87cc0014b`. Owned paths are this document and the
one-line secondary-autoscroll field-width correction in the linked Battle11 owner;
no shared indexes/registries, schemas/fixtures, tooling, design, remake/UI,
glossary or Chinese changes. #986 synthesis and UI692 are independent.
No stacked dependency or unmerged conclusion is consumed.

Accepted owners are [North Cliff opening/Rohde](./north-cliff-opening-and-rohde-recruitment.md),
[Battle24 outcomes/preparation](./battle24-outcomes-through-north-cliff-preparation.md),
[Battle24 admission](./new-granseal-departure-through-battle24-admission.md),
[earlier Dark Caves/Battle11 admission](./east-shrine-return-through-battle11-admission.md),
[request consumption](./map-event-request-consumption.md),
[battle loop](./battle-loop.md), [battle cutscenes](./battle-cutscenes.md),
[spriteset data](./battle-spriteset-data.md),
[shared roster consumers](./battle23-initialization-and-roster.md) and
[common maps](./common-maps.md). Scope is Map72 entry into Map48 through fresh
Battle25 initialization; no turns/combat, victory/defeat/Egress, later cave
crossing/Ketto, all-map items, runtime/acquisition, extraction regeneration,
installation, new comparison machinery or production implementation.

Paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained read-only checkout is clean at that pin. Existing fixtures and
retained payloads identify USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Constants belong to `sf2enums.asm`, RAM to `sf2const.asm`, encodings to
`sf2mapmacros.asm` and `sf2macros.asm`. Fnnn means flag nnn; table/roster indices
are zero-based and extents half-open. A source-named AI role is configuration,
not an observed decision.

Minimum evidence is selected source, existing fixtures and the unchanged
18,285,979-byte map-init/map-script pair. The read-only linear comparison is
budgeted below one minute/512 MiB with small prose/receipts; it took 0.107s.
No new maintained checker or input acquisition is needed. Replan material
contradictions or supporting-tool expansion. Remaining Unknowns do not launch
a runtime queue. Direct source/fixture/consumer and changed-state review,
document/private checks, committed planner and actual exact-head CI are the
documentation-only acceptance profile.

## Opened entrance, mutable request and ordered admission

**Confirmed:** `sf2enums.asm` resolves `MAP_OVERWORLD_BEDOE_KINGDOM=72`,
`MAP_DARK_CAVES=48`, `BATTLE_NORTH_CAVE=25`, `BATTLE_CAVE_OF_DARKNESS=11`
and `UP=1`. `data/maps/entries/map72/6-warp-events.asm` row0 is the eight-byte
record at `$C1BFE`: `(27,33)`, no scroll, Map48, destination `(55,29)`, Up.
The accepted scene's unlock425, live layout copy and later caller750 are
separate effects. Fresh Map72 uses F750 to copy the opened layout; neither
live opening nor reloaded opening proves a successful walk to this warp.
The row itself contains no F750, F425, F524, Rohde membership, item, HP or
class predicate. A supplied clear750/set425 state is not repaired or rejected
by the row; physical access remains a separate question.

`code/common/scripting/entity/entityscriptengine_2.asm:esc02_controlCharacter`
requires the layout attribute's masked `$1000` kind before calling
`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint`. The scanner
divides supplied internal X/Y by `MAP_TILE_SIZE=384`, scans rows in order,
uses negative source coordinate bytes as wildcards, and writes event type1,
parameters1-4 as a longword, facing/padding as a word and WARP_SFX. Row0's
coordinates are exact. Source coordinates alone do not prove layout reach.
The pending slot is mutable; another producer can replace the request.
`ProcessMapEvent` clears its type before dispatch, not all parameters.

`code/gameflow/exploration/explorationfunctions_2.asm:ProcessMapEventType1_Warp` with parameter1=0
submits/clears WARP_SFX, idles physical0, unwinds the exploration frame,
clears d0-d4 words, then samples main's position with
`UpdatePlayerPosFromMapEvent`. It replaces each nonnegative destination
coordinate's **low byte**, applies the facing byte and returns d4=0 to
`MainLoop`. Thus the nominal words `(55,29)` assume ordinary sampled high
bytes; the row does not normalize arbitrary supplied upper bytes. Admission
compares low bytes, whereas later exploration placement multiplies words.
The warp does not clear PLAYER_TYPE. A fresh BattleLoop does; a no-battle
exploration entry retains the supplied player-mode premise.

`code/gameflow/mainloop.asm` calls `SwitchMap` then `CheckBattle`.
`data/maps/global/flagswitchedmaps.asm` has no replacement for48 or72.
`code/common/maps/getbattle.asm:CheckBattle` resolves CURRENT255 for the scan
and tests map, unlock and trigger in ascending battle order. The complete
Map48 candidate set in `data/battles/global/battlemapcoords.asm` is:

| Row / symbol | Area output X/Y/width/height | Exact trigger X/Y | Unlock / completion |
| --- | --- | --- | --- |
|11 / CAVE_OF_DARKNESS|`(0,0,27,32)`|`(15,26)`|411 /511|
|25 / NORTH_CAVE|`(28,0,32,32)`|`(55,29)`|425 /525|

The rectangle is output geometry, not a containment predicate. At `(55,29)`,
row11 fails its trigger even with411 set; row25 succeeds if425 is set.
At `(15,26)`, row11 can win and row25 cannot. At `(52,12)`, `(3,11)` or any
other nonmatching point, neither row qualifies merely because it lies inside
a cave rectangle. Both unlocks clear gives ordinary exploration. A changed
effective map can select different rows or none; a nominal Map48 name does
not guarantee Battle25 or even the first battle encountered during travel.

On a match, CheckBattle stores all four area bytes before checking completion.
With425/525 both set it clears425 **and still returns25 on that same call**;
it does not continue scanning. With425 clear,525 alone selects nothing.
Holding425 with525 clear permits another supplied admission; clearing425
before consumption removes it. No intro475, party/HP/class, RNG, scene-completion
or F750 check is added here. Interrupted prefixes retain whatever writes
were reached. MainLoop changes d1 to the selected battle index and calls
BattleLoop; it does not first run Map48 exploration init/population.

The other Map72 cave rows discriminate requests without expanding travel:
row1 `(25,28)` requests48 `(52,12)` Up; row3 `(58,49)` requests48 `(15,26)` Up;
row4 `(56,45)` requests48 `(3,11)` Up. These are distinct stored routes.
Their existence does not establish free passage between the two cave areas.

## No-battle Map48 and immediate return routes

**Confirmed:** `data/maps/mapsetups.asm` has no Map48 row. The actual fallback
is in `code/common/scripting/map/mapsetupsfunctions_1.asm:GetCurrentMapSetup`:
on the table sentinel it returns `ms_Void`, a `$FFFF` word. The entity-list
accessor returns that sentinel unchanged; `j_GetMapSetupEntities` resolves
to `GetMapSetupEntityList`. `mapfunctions.asm:InitializeMapEntities` still
clears the old entities and builds persistent flag followers before the list
read. Its first byte `$FF` skips fixed NPC rows; it then declares main0 from
the supplied coordinates/facing. `RunMapSetupInitFunction` sees `$FFFF` and
skips its function call. Missing setup does not mean missing shared behavior.

Explicit ordinary Map48 entry clears pending, subtracts20000 from STEP_COUNTER
with signed clamp, heals living/immortal allies, sets CURRENT_MAP48 and
CURRENT_BATTLE255, loads tiles, rebuilds entities/mapsprites, clears temporary
F256-383 and sets80 before LoadMap and the sentinel init. Persistent story,
party and battle flags survive. Map48 is neither overworld nor an excluded
follower map: enabled follower rows are rebuilt in source order, with no
overworld Caravan prefix from65. Peter67, Astral70 and Rohde84 remain their
own conditions. Rohde's join11/active43 is not84, and the previous live follow
chain is not preserved as a fixed population. Supplied alias flags/HP/class
can alter mapping/sprites. The map-before Battle11 scene is not called merely
because48 loads, and Battle25 has no separate scene population.

A CURRENT255 request takes the existing partial-entry branch: healing still
precedes it, but it retains current map/battle, updates main properties and
raft, and omits the explicit population/temp reset. A scroll request stays
inside exploration and bypasses MainLoop/CheckBattle and ordinary entry.
A direct script LoadMap is likewise not an ordinary warp/admission. None
can substitute for the selected explicit no-scroll request while preserving
its lifecycle. Pending zero and completed services are needed for usable
control; the static return label alone is insufficient.

`data/maps/entries/map48/6-warp-events.asm` contains four no-scroll return
rows, all facing Down and requesting Map72:

| Row | Match in Map48 | Requested Map72 X/Y |
| --- | --- | --- |
|0|`(55,30)`|`(27,34)`|
|1|`(52,13)`|`(25,29)`|
|2|`(15,27)`|`(58,50)`|
|3|`(3,12)`|`(56,46)`|

Row0 is the immediate stored return beside the selected entry; it is not
proof of a reachable escape tile or a Battle25 Egress route. Ordinary return
would again run CheckBattle on72, whose wildcard rows8/24/26 test408/424/426
in order, with corresponding completion behavior, before fresh exploration.
No later trip or outcome is claimed. Map48's flag-copy table is only endWord;
its optional item content is outside this note.

## Fresh initialization and the two empty hooks

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop` clears
PLAYER_TYPE and sets399 before testing88. With88 clear it clears seconds,
saves incoming map/battle words, stores current map48/battle25, sets base
VInts and invokes before. After return it restores/reassigns map/battle,
clears region flags90-105, heals allies, initializes ally then enemy positions,
clears AI memory, loads battle for combatant0 and invokes start, in that order.
The supplied exploration `(55,29)` and Up do not become a forced battle-main
position/facing. The roster and physical-entity consumers below choose those.
No completed prefix is rolled back after an interrupted service.

Both48-word tables in `data/battles/cutscenes/beforebattlecutscenes.asm` and
`battlestartcutscenes.asm` select `ms_Empty` at slot25, byte offset50.
The pointer words are at `$47ABA` and `$47B5E`; they resolve to
`code/common/scripting/map/ms_empty.asm`, the single `$FFFF` terminator at
`$47A4E..$47A50`. This is actual target resolution, not an inference from a
missing `battle25` directory or a routing table's structural count.

The corresponding wrappers under `code/gameflow/battle/cutscenes/` test
`BATTLE_INTRO_CUTSCENE_FLAGS_START450 + 25 = 475`. Before dispatches only
when clear without setting475; start sets475 **before** dispatch when clear.
Set475 skips both hooks but does not skip fresh healing/placement/load.
ExecuteMapScript clears its skip latch, consumes the terminator, conditionally
waits for view scrolling if a dialogue window remains, then clears scrolling
speed and returns. There is no Battle25-local dialogue, camera scene, explicit
entity list, join, movement or Battle11 `csub_458E` call in either hook.
The shared loader still configures the secondary layer below.

With88 set, BattleLoop restores saved seconds, clears88, clears AI memory,
sets view target0, calls LoadBattle using supplied current state and jumps
to the individual-turn loop. Incoming48/25 do not replace that saved current
state on this branch. Before/start, region reset, healing and new placement
are skipped; the following jump to @Start is unreachable. This continuation
is not the fresh start-return endpoint or evidence of the roster below.

## Complete initial combatants

**Confirmed:** `data/battles/spritesets/entries.asm` slot25 selects
`BattleSpriteset25` in `spriteset25.asm`, `$1B4EA6..$1B5012` (364 bytes).
Header12/15/3/0 and subsection offsets4/148/328/364 follow from four header
bytes,27 twelve-byte combatant records,3 twelve-byte regions and zero points.
`GetBattleSpritesetSubsection` uses CURRENT_BATTLE*4 and preceding counts
and strides. Macros encode type/index, X, Y, AI, item word, primary order,
primary region, secondary order, secondary region, filler and spawn.
All27 records have filler0 and `STARTING=0`; no hidden/spawning row changes
the initial identity assignment in this spriteset.

All twelve ally rows have HEALER1=0, NOTHING=`$007F`, both orders NONE=`$FF`
and regions15/15. Their complete placement-index/local-coordinate list is:

| Index | X/Y | Index | X/Y |
| --- | --- | --- | --- |
|0|`(25,26)`|6|`(27,25)`|
|1|`(26,26)`|7|`(26,25)`|
|2|`(27,26)`|8|`(28,26)`|
|3|`(26,27)`|9|`(28,25)`|
|4|`(27,27)`|10|`(27,28)`|
|5|`(28,27)`|11|`(28,24)`|

`code/gameflow/battle/battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions`
resets all30 ally X=-1, calls UpdateForce and traverses the actual rebuilt
joined-and-active list in ascending ally-ID order. At most twelve processed
members bound this pass, not twelve guaranteed healthy actors. Zero word HP
consumes a member/loop slot without advancing the coordinate pointer; the
next living member takes that unconsumed placement. Surplus members leave
the active party, setting X=-1 without immediately rebuilding the cached
list/count. An empty list has no initial guard before its first read.
Only placement X/Y is copied; the row's index/AI/item fields do not overwrite
the member's identity, inventory or AI.

The preceding `HealLivingAndImmortalAllies` visits all30 independent of
membership: living allies and Peter7/Lemon28 get max HP/MP, status masked to
existing STUN/POISON/CURSE, then UpdateCombatantStats. Other dead allies skip;
this is not blanket revival or a new force. Rohde11 joining, active43, health,
class/equipment and placement are separate. Neither his map actor nor the
cannon scene guarantees a battle slot, transfers an item here, or overrides
party capacity. A living joined/active Bowie0 normally takes placement0;
that premise is explicit when discussing the camera below.

The complete enemy records follow. All secondary orders are NONE. AI numeric
values are ATTACKER1=6, ATTACKER3=9, HEALER3=2; FOLLOW_ENEMYn=`$80|n`.
Regions are primary/secondary. Items below are authored source words.

| Row / combatant ID | Type (ID) | Local X/Y | AI | Primary order | Regions | Item word |
| --- | --- | --- | --- | --- | --- | --- |
|0 /128|DARK_SOLDIER18|`(24,7)`|ATTACKER1|NONE|0/15|NOTHING `$007F`|
|1 /129|DARK_BISHOP81|`(21,10)`|HEALER3|FOLLOW_ENEMY0|0/15|NOTHING `$007F`|
|2 /130|LESSER_DEMON45|`(19,9)`|HEALER3|FOLLOW_ENEMY0|0/15|NOTHING `$007F`|
|3 /131|DARK_BISHOP81|`(6,6)`|HEALER3|FOLLOW_ENEMY4|1/1|NOTHING `$007F`|
|4 /132|DARK_SNIPER62|`(7,9)`|ATTACKER3|FOLLOW_ENEMY5|1/1|NOTHING `$007F`|
|5 /133|SKELETON17|`(5,10)`|ATTACKER1|NONE|1/15|MEDICAL_HERB `$6000`|
|6 /134|LESSER_DEMON45|`(14,12)`|ATTACKER1|NONE|1/15|NOTHING `$007F`|
|7 /135|SKELETON17|`(10,16)`|ATTACKER1|NONE|2/15|MEDICAL_HERB `$2000`|
|8 /136|DARK_SNIPER62|`(8,17)`|ATTACKER1|NONE|2/15|NOTHING `$007F`|
|9 /137|LESSER_DEMON45|`(6,28)`|ATTACKER1|NONE|2/15|NOTHING `$007F`|
|10 /138|DARK_SNIPER62|`(13,28)`|ATTACKER3|FOLLOW_ENEMY11|15/15|NOTHING `$007F`|
|11 /139|SKELETON17|`(14,27)`|ATTACKER1|NONE|15/15|MEDICAL_HERB `$2000`|
|12 /140|SKELETON17|`(15,19)`|ATTACKER1|NONE|15/15|MEDICAL_HERB `$2000`|
|13 /141|DARK_SNIPER62|`(18,21)`|ATTACKER3|FOLLOW_ENEMY14|15/15|NOTHING `$007F`|
|14 /142|SKELETON17|`(20,22)`|ATTACKER1|NONE|15/15|MEDICAL_HERB `$6000`|

These IDs are verified from assignment, not presumed from display order.
InitializeAllEnemiesBattlePositions iterates128-159. Its per-enemy function
compares the combatant byte with count15|$80, masks the admitted index and
multiplies by12 to address the row. STARTING0 is below HIDDEN2, so all fifteen
call InitializeEnemyStats. Slots143-159 instead get activation0, max/current
HP0 and X=-1; that absent path still reads trigger bytes from the subsection
pointer, not a legitimate sixteenth row. Battle32's Jaro exception does not
apply. IDs128-142 are combatant IDs, not physical map-entity slots.

InitializeEnemyStats copies the selected56-byte/14-longword definition, sets
current HP/MP to maxima and row X/Y, combines definition movetype and row AI,
sets enemy index, trigger regions and move orders, adds the row item, sets
activation, adjusts difficulty base ATT, then updates current stats. Regions
pack primary<<4|secondary; orders pack primary<<8|secondary. STARTING0 retains
only the definition's `$F000` activation bits: Bishop `$3000`, all four other
types `$2000`. Definition BIT4/BIT5 are discarded; active/neutral bits are
clear at this endpoint. No activation or tactic is established yet.

### Definitions, equipment and altered difficulty

**Confirmed:** `data/stats/enemies/enemydefs.asm` supplies the following
selected definitions, independent of525 on this battle:

| Type | Level | Max HP/MP | Base ATT/DEF/AGI/MOV | Movetype | Equipped slot0 | Four spell slots |
| --- | --- | --- | --- | --- | --- | --- |
|DARK_SOLDIER18|23|35/17|31/46/34/4|REGULAR|LARGE_AXE|ATTACK, NOTHING, NOTHING, NOTHING|
|DARK_BISHOP81|23|39/38|28/38/27/5|HEALER|FLAIL|AURA, BLAST LV3, NOTHING, NOTHING|
|LESSER_DEMON45|22|40/21|49/37/32/6|FLYING|NOTHING|BLAZE LV3, BOOST LV2, NOTHING, NOTHING|
|DARK_SNIPER62|24|38/0|11/37/35/5|STEALTH_ARCHER|HYPER_CANNON|four NOTHING|
|SKELETON17|21|30/0|26/40/30/6|REGULAR|BROAD_SWORD|four NOTHING|

All have spellPower ENHANCED, initialStatus NONE, and three remaining NOTHING
item slots (Demon all four). Definition resistance is0 for Soldier/Bishop/
Sniper, STATUS_MINOR for Demon, and WIND_WEAKNESS|LIGHTNING_MINOR|ICE_MAJOR|
FIRE_WEAKNESS|STATUS_IMMUNITY for Skeleton. Prowess is CRITICAL150_1IN32|
DOUBLE_1IN32|COUNTER_1IN16 for Soldier/Demon; Bishop changes counter to1IN32;
Sniper changes double to1IN16; Skeleton changes critical to1IN8. These fields
are copied configuration, not damage, accuracy or spell-execution claims.

InitializeEnemyItems skips exact NOTHING. For the five Skeleton herbs,
`code/common/stats/itemstats.asm:AddItem` finds the first empty slot and masks the input
by `$807F`, stripping `$2000` USABLE_BY_AI and `$4000` UNUSED_ITEM_DROP.
Both authored `$2000` and `$6000` become stored `$0000` in slot1 after the
copied equipped sword. The initializer tests the post-AddItem equipped bit;
these herbs are not equipped. Source item flags do not prove stored AI use
or drops. No Bishop row adds the Battle24 Blizzard item.

`AdjustEnemyBaseAttForDifficulty` has consecutive beq branches: every
difficulty applies floor(baseATT*5/4), and SUPER2 applies it again. Initialized
base ATT ordinary/SUPER is Soldier38/47, Bishop35/43, Demon61/76, Sniper13/16,
Skeleton32/40. UpdateCombatantStats then incorporates equipment and other
current-stat rules. These are not final ATT or damage values.

`UpgradeRandomBattleEnemies` calls IsBattleUpgradable before any completion,
class/level or upgrade RNG calculation. The eleven-entry `list_RandomBattles`
in `data/battles/global/randombattles.asm` excludes NORTH_CAVE25, so it returns
the original enemy type. Supplied525, a high/promoted Bowie or changed RNG
does not enable this upgrade on25. This differs from Battle24 and is not a
claim that the entire later battle is RNG-free. F88 continuation does not
initialize these definitions at all.

### Regions and physical population

**Confirmed:** all three region records have type4, byte1=0 and trailing0/0.
Their complete vertices, in stored order, are:

| Region | Local vertices |
| --- | --- |
|0|`(0,0),(0,14),(31,14),(31,0)`|
|1|`(0,0),(0,31),(7,31),(31,7)`|
|2|`(0,0),(0,31),(18,31),(18,0)`|

There are zero AI points and no MOVE_TO point order. The shared type4
IsRegionTriggered tests triangles1/2/4 and3/2/4; region15 is a sentinel,
not a fourth polygon. Follow orders reference combatants128,132,133,139,142
for operands0,4,5,11,14 respectively. They are stored intentions, not actions
performed before ActivateEnemies; clearing90-105 alone fires no tactic.

`code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities` clears exploration actors,
scans allies0-29 with nonnegative X/Y, then enemies in combatant order,
skipping neutral/negative entries and writing fresh logical-to-physical
lookups. The five enemy mapsprites82/145/109/126/81 are below the special-sprite
threshold. Thus normal physical indices follow the actually admitted allies
and enemies; they are not fixed128-142. There is no NORTH_CAVE neutral-table
entry. Coordinates are masked by$3F and scaled384, with facing Down and
`eas_Standing`. Rohde's prior live follow chain is replaced. LoadBattle uses
combatant0's mapping for the camera; a missing0 is not repaired into a valid
party (negative target falls back to physical0 for coordinate sampling).

## Actual map crop, area selection and secondary layer

**Confirmed:** `code/common/maps/mapload.asm:LoadMapBlocksAndLayout` loads
Map48 blocks/layout, applies its flag-copy/collected-item processing, then
uses BATTLE_AREA_X/Y and WIDTH/HEIGHT to CopyMapBlocks into origin0 for a
battle. Here that is source `(28,0)`, size32x32. Roster X/Y is stored directly
as battle-local coordinates: `(x,y)` corresponds to original Map48 `(x+28,y)`.
Bowie's ordinary local `(25,26)` therefore corresponds to `(53,26)`, not
warp destination `(55,29)`. No crop rectangle is an admission trigger.

The two30-byte descriptors in `data/maps/entries/map48/2-areas.asm` are:

| Descriptor | Main start/end | Foreground start | Background start | Both parallax pairs | Main / secondary autoscroll bytes | Layer / music |
| --- | --- | --- | --- | --- | --- | --- |
|0|`(0,0)..(26,31)`|`(0,0)`|`(28,33)`|256/256|0/0;128/128|255 / BATTLE_THEME_1|
|1|`(28,0)..(59,31)`|`(0,0)`|`(28,33)`|256/256|0/0;128/128|255 / BATTLE_THEME_1|

The autoscroll operands are **bytes**, as encoded by sf2mapmacros and consumed
at RAM `$FFA846..$FFA849`; they are not word velocities. Type255 is nonzero,
selecting the map-on-foreground branch. No new cave-specific renderer or
layout extraction is needed to establish these fields.

`code/gameflow/battle/battlefunctions/loadBattle.asm:LoadBattle` fades out,
loads map tilesets, waits, clears VInts, calls
PositionBattleEntities, selects combatant0's view target, initializes sprites
and calls LoadMap with d0=-1. LoadMap selects a descriptor by the target's
current internal coordinates (or physical0 when the target is negative),
comparing against descriptor bounds scaled384. It does **not** add crop X28
to this local target for selection. With ordinary Bowie at local `(25,26)`,
descriptor0 matches, despite this being Battle25. With a target at local X28
and in-range Y, descriptor1 can match; X27 misses both and reaches the
sentinel's fallback to descriptor0. Arbitrary supplied positions must retain
this selection logic. By contrast, no-battle main `(55,29)` selects descriptor1
with world coordinates and uncut layout.

LoadMapArea overrides main bounds for any battle to local0..31 on both axes
from width/height, while still consuming the selected descriptor's remaining
fields. Both descriptors have identical remaining fields, so the selection
distinction above does not change Battle25's secondary-layer parameters.
The foreground/background starts are multiplied by3 into RAM words; the
later plane initialization shifts by7. Parallax applies multiply then >>8.
`LoadMap` calls `exploration.asm:sub_38C0` and, because secondary autoscroll
bytes are nonzero, uses its results for plane-B X/Y. For nonnegative target
and layer255, each word result is `(backgroundStartRAM << 7) + 3456 -
(entityCoordinate - planeACoordinate)`. A negative target instead returns
current plane-B coordinates. The alternate type0 path uses plane B relative
position and layer2-start words; it is not the selected descriptor here.

LoadMap builds plane ring counters as `(- (X >> 4)) & 255` and
`((Y >> 4) + 8) & 255`, enables display/interrupts, updates H/V scroll data,
window/roof state, waits one VInt and calls UpdateVdpPlaneA/B. Plane B builds
its layout and requests VInt VRAM DMA to `$E000`. The ordinary
VInt_UpdateScrollingData consumer treats byte128 specially: it recomputes
sub_38C0, writes plane-B destination and speed16 per axis, and sets the
corresponding scrolling bit; it does not interpret128 as an ordinary signed
constant velocity. `animations.asm:VInt_UpdateMapPlanes` consumes update
bits to refresh planes. No Battle11 helper invocation is necessary for this
shared load path, nor is its explicit scene evidence a Battle25 observation.

LoadBattle then loads mapsprites, restores base VInts, loads battle terrain,
requests map music and fade-in and returns (Battle44's timer branch does not
apply). The empty start wrapper sets475 if needed and returns.
**Stop immediately before `BattleLoop:@Start` calls `ActivateEnemies`.**

**Inferred:** this secondary plane configuration supports the cave darkness
presentation relative to the chosen actor. **Unknown:** composed visible mask,
lit radius, occlusion/palette, camera settling, DMA/frame timing and usable
control. Static loader requests and an empty script return do not prove that
rendering or fade-in completed. No turn/combat/outcome is included.

## Changed-state review and reproduction

| Supplied change or interrupted prefix | Conditional consequence |
| --- | --- |
|750 clear/set independently of425|Warp row has no added guard; actual layout access and battle unlock remain separate.|
|Request overwritten or425 cleared before admission|Original producer no longer guarantees Battle25; inspect consumed tuple/state.|
|48 `(55,29)`,411 and425 set|11 fails exact trigger;25 qualifies.|
|48 `(15,26)`,411 set|11 qualifies; Battle25 roster/configuration is not substituted.|
|48 `(52,12)` / `(3,11)` / other point|Neither exact cave trigger matches; area containment is insufficient.|
|425 clear,525 set|No25; no unlock repair from completion.|
|425 and525 set|Clear425 after selection and enter25 once; original definitions still used.|
|475 already set|Skip empty hooks, retain fresh initialization; no before-scene population.|
|88 set with changed incoming map/trigger|Load supplied saved current state; bypass fresh chain and start-return endpoint.|
|Dead/reserve/unjoined Rohde or full/altered active list|Join/follower facts do not guarantee placement; healing/list/HP rules decide.|
|Dead non-immortal / Peter or Lemon dead|Other dead remain unplaced; immortals are healed before placement.|
|No active members / absent combatant0|No initial empty-list guard or valid-camera repair is inferred.|
|Bowie level/class/RNG changed, battle still25|No random enemy upgrade; ally stats/sprites may differ.|
|SUPER versus other difficulty|Two versus one baseATT scaling passes; equipment/current stats still apply.|
|Camera local25 /27 /28 with valid Y|Descriptor0 /fallback0 /descriptor1; battle bounds still32x32.|
|CURRENT255 / scroll / explicit48 / direct LoadMap|Different admission, population and flag-reset lifetimes; do not conflate.|
|Interrupted heal/placement/load/start or pending/service completion|Retain reached mutations; no atomic rollback, playable or saved-state claim.|

Fixture paths below are under `tests/fixtures/h2/`. Existing owning commands
(`uv run sf2 h2 <name>`) identify evidence boundaries; they are not claimed
as fresh extraction/runtime runs for this note.

| Relation | Existing evidence / selected direct source |
| --- | --- |
|Warp/admission|`map-data-static-v1.json`, `map-event-request-consumption-static-v1.json`, `common-maps-static-v1.json`, `battle-global-data-static-v1.json`; exact Map72/48 warp rows, all Map48 battle rows, producer, no-scroll/MainLoop/CheckBattle bodies.|
|No-battle fallback|`map-setup-static-v1.json`, `map-init-static-v1.json`; complete MapSetups membership and sentinel/accessor/init/population/follower consumers, explicit versus CURRENT entry.|
|Before/start|`battle-cutscenes-static-v1.json`, `battle-routing-data-static-v1.json`, `map-script-engine-static-v1.json`; actual slot25 targets, wrapper order and ms_Empty interpreter return. Structural routing counts alone do not resolve a target.|
|Initial roster|`battle-spriteset-data-static-v1.json`, retained spriteset facts, `battle-loop-static-v1.json`; exact spriteset25 source records, pointer/subsection macros, assignment/heal/item/difficulty/upgrade/physical-entity consumers and all five enemy definitions. Counts alone do not prove these rows.|
|Map/render configuration|`common-maps-static-v1.json`, `map-data-static-v1.json`; both Map48 descriptors, byte macros, LoadBattle/LoadMap/LoadMapArea/crop, sub_38C0 and plane/VInt consumers. No layout or rendered observation is published.|

From the owning checkout load ignored `local/private-inputs.ps1` in the same
process as each Python/planner command. `$upstream` denotes the verified
clean read-only pinned source. Reuse the
[retained-pair recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, _verify_map_script_engine_fixture and actual _cutscene_facts,
with unchanged pair size/mtime. No extraction regeneration.

1. Verify pin/clean source and select the exact rows/labels above. Resolve
   symbols through sf2enums, encoding through the named macros, and actual
   aliases/consumers. Compare the complete candidate order and no-map-setup
   sentinel rather than inventing a map48 init or battle25 scene file.
2. Resolve both slot25 words and compare wrapper facts to the existing fixture.
   Read fresh and88 branches through the endpoint. Compare all27 complete
   combatant records, all3 region records, zero points and364-byte extent;
   resolve enemy IDs from actual assignment and all selected definition fields.
3. Review the immediate no-battle return alternatives and changed-state table.
   Follow the selected map crop separately from descriptor selection, byte128
   secondary-layer behavior and final load/start requests. Preserve service,
   state and presentation Unknowns rather than escalating to runtime.
4. Check complete prose/diff, relative links/anchors, UTF-8/LF, owned-path/private
   boundary and `git diff --check`. On a clean committed head run
   `uv run sf2 verify plan --base origin/main --head HEAD`; record selected
   partitions and actual final-head CI in the Issue/PR handoff. No blanket
   local normal/full/engine/Godot/H3/H4 suite is selected solely for this note.

**Unknown:** natural collision-free entrance/return reach and supplied
flags/request/party/HP/class/equipment/mode/mappings; execution/completion of
load, sound, text/window tail, fade, VInt/DMA and scrolling services; rendered
darkness, usable control, uninterrupted progression and save persistence.
Supplied suspension state is not validated by a fresh initialization trace.
These limits remain explicit and do not prevent the bounded static result.

Retain [#985 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/985#issuecomment-6180719770)
and [PR987 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/987#issuecomment-6180735932):
02991c0d/tree8c4eca33/basee96f6faa, CI38653869752(scope15s/research33s),
0.082s unchanged-pair receipt,114 commands/612 bytes/150 generated action
bytes, LF-only failure/correction, path/field diagnostics and hidden-X VInt
correction. Preserve their linked #981/#977 failures, results and Unknowns.
The earlier Battle11 owner described128/128 autoscroll as words. Direct
sf2mapmacros dc.b encoding, adjacent RAM bytes and byte consumer operations
establish bytes. The [consolidated PR992 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/992#issuecomment-6181013516)
expanded Issue989 ownership for that one-line correction; both owners now
say bytes, while this paragraph preserves the original discrepancy and its
source-backed correction. The ignored no-Map48 check also now compares each
msMap row's first operand, so a row such as `48,ms_map48` cannot evade it;
the earlier whole-argument comparison was insufficient despite the manually
confirmed absence. Discovery guesses were resolved to actual source
paths; no absent guessed path is treated as missing behavior.

Freeze the clean pushed Draft PR with Project Review / Verify and completed
local processes for independent main-gate review. Preserve environment,
source/pair and receipts. No merge, cleanup, archive or next Issue.
