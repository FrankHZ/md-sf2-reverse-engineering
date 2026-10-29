# Battle27 initialization and opening

Evidence date: 2026-10-14. **Confirmed:** the fresh Map74/Battle27 handoff
passes an empty before hook, resets region flags, heals eligible allies,
constructs the actual party and sixteen enemy rows, and loads the battle.
Its start wrapper sets F477 before the three-speaker opening. This note stops
at `BattleLoop:@Start`, before `ActivateEnemies`. The F88 continuation instead
loads supplied battle state and stops at `@IndividualTurns_Loop`, before
`ExecuteIndividualTurn`; it does not replay this initialization or opening.
**Inferred:** the source-named TO_TRISTAN encounter confronts the force about
travel restrictions. **Unknown:** natural reach, incoming state, completed
services, presentation, usable control and persistence; no runtime is claimed.

## Agreement and provenance

Standard rigor for [Issue #1033](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1033)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
formation, physical entities, suspended state and the intro flag have distinct
lifetimes. The accepted starting base is
`369757a59864cef7d4c0bf0e40769659fbab219d`, including #1029/PR1032. This
document is the sole tracked owner; no shared fixture, schema, code, index,
design, remake or translation change. Concurrent #1034 and UI692 supply no
evidence. Stop before activation/turn/combat/outcome execution; Battle32,
later story/Frayja, runtime, capture and extraction are excluded.

Start at the accepted [MainLoop admission handoff](./pacalon-departure-to-battle27-admission.md).
Reuse [Battle26 initialization](./battle26-initialization-and-roster.md),
[activation/turn boundary](./battle26-activation-and-first-turn-dispatch.md),
[dialogue consumers](./battle26-admission-and-geshp-confrontation.md),
[battle loop](../../battle/battle-loop.md), [cutscene routing](../../battle/battle-cutscenes.md),
[spritesets](../../battle/battle-spriteset-data.md), [global data](../../battle/battle-global-data.md),
[maps](../../world/common-maps.md) and [script interpreter](../../world/common-scripting.md).
Battle27-specific records and changed-state predicates are checked below;
Battle26's neutral and upgrade results are not transferred to this encounter.

Source paths are relative to `disasm/` at pinned
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained read-only source is clean at that pin. Existing evidence binds
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Constants and encodings belong to `sf2enums.asm`, `sf2const.asm`,
`sf2macros.asm` and `sf2mapmacros.asm`. Fnnn means flag nnn; indices are
zero-based, address extents half-open. Short `battleloop/`, `battlefunctions/`,
`battlefield/` and `cutscenes/` paths are under `code/gameflow/battle/`;
`entity/` and `map/` are under `code/common/scripting/`.

Minimum evidence is the selected pinned source, existing public fixtures and
retained private map-init/script pair. Reuse its 18,285,979 bytes and accepted
0.083s comparison without regeneration. Selected linear reads/comparisons are
budgeted below one minute/512 MiB with small prose/receipts and no acquisition
or API processing cost. Replan contradictory evidence or material supporting
system expansion; no new general simulator, parser or geometry machinery is
needed. Unknowns do not create an observation queue.

## Fresh entry and exact before hook

**Confirmed:** `code/gameflow/mainloop.asm:MainLoop` passes resolved d0=74
and d1=27 to `j_BattleLoop`. Admission has written crop `(2,16,32,48)`;
it has not assigned CURRENT_BATTLE. Selection of27 precedes32 on Map74,
and a held completion527 can clear unlock427 while still admitting27 on
that call. These are predecessor predicates, not additional BattleLoop tests.

`code/gameflow/battle/battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`) orders:

1. Clear PLAYER_TYPE.b, set F399, then check F88. Clear takes fresh entry.
2. Clear SECONDS_COUNTER.l. Save incoming d0/d1 **words**, assign their low
   bytes to CURRENT_MAP/CURRENT_BATTLE, install base VInts, call before.
3. Restore d0/d1 words and reassign current map/battle bytes. Clear all region
   flags90–105, not only this spriteset's four declarations.
4. HealLivingAndImmortalAllies; InitializeAllAlliesBattlePositions;
   InitializeAllEnemiesBattlePositions; ClearAiMemory.
5. Clear d0.w, call LoadBattle for combatant0; ExecuteBattleStartCutscene;
   reach `@Start` and stop before `bsr.w ActivateEnemies`.

`cutscenes/beforebattlecutscenesstart.asm:ExecuteBeforeBattleCutscene` reads
CURRENT_BATTLE, adds450 and checks F477. Held skips dispatch. Clear selects
slot27/byte54 of the complete48-word `data/battles/cutscenes/beforebattlecutscenes.asm`
table: word at `$47ABE`, relative to base `$47A88`, selects **ms_Empty**.
Its body in `map/ms_empty.asm` is only `$FFFF` (`$47A4E..$47A50`). Before
does not set477. It saves/restores d1 and, on dispatch, d0/a0. Even empty
dispatch enters ExecuteMapScript, clears the text-skip latch and executes its
shared terminator tail; return is conditional on shared services completing.

The restored arguments repair a stale CURRENT_BATTLE for fresh entry, but
do not rewrite crop bytes or rerun admission. Changed incoming d0/d1 select
different actual records; the numeric27 claims require that selection at
each consumer. Later consumers independently reread CURRENT_BATTLE. F477
held skips both hooks but not fresh reset/healing/roster/load. Every suffix
depends on preceding calls returning; interruption preserves completed writes.

## Actual party and all twelve placement slots

**Confirmed:** `battleloop/heallivingandimmortalallies.asm` scans all30 allies,
independently of joined/active membership. Living allies, and always Peter7
and Lemon28, receive maximum HP/MP as current values; other dead allies are
skipped. The status mask retains STUN|POISON|CURSE (`$0007`), then
`code/common/stats/updatecombatantstats.asm:UpdateCombatantStats` rebuilds
current stats, applies statuses/equipment and recomputes curse. Thus the
intermediate mask is not a final curse promise. No recruitment, promotion,
inventory replacement or active-party selection follows from healing.
HP fields are words; MP and position fields are bytes.

`battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions` first
sets X=-1 for every ally, then calls UpdateForce. The actual
`code/common/stats/battleparty.asm` rebuilds joined/active/reserve lists in
ascending ally-ID order from membership flags. A stale supplied list is not
authoritative. Formation ordinals are not ally identities. Each living active
member gets the next row's X/Y; that row's AI/item fields do not replace the
ally's own state.

The header is **12 ally slots, 16 enemy rows, 4 regions, 1 point**. Every
ally row has HEALER1=0, NOTHING=`$007F`, NONE=`$FF`/region15 in both orders,
filler0 and STARTING=0. All twelve varying positions are:

| Ordinal | X/Y | Ordinal | X/Y |
| --- | --- | --- | --- |
|0|`(14,44)`|6|`(16,46)`|
|1|`(15,44)`|7|`(17,46)`|
|2|`(14,45)`|8|`(16,45)`|
|3|`(15,45)`|9|`(17,45)`|
|4|`(14,46)`|10|`(16,44)`|
|5|`(15,46)`|11|`(17,44)`|

A dead member consumes one processed-member slot but does not advance the
placement pointer; later living members use earlier unused positions. Twelve
limits processed members, not guaranteed visible actors. Surplus active
members go through LeaveBattleParty (active bit cleared and X=-1), without
an immediate list/count rebuild. Fewer eligible members produce fewer actors.
The zero-member count is decremented before the first member read without
an empty-list guard; arbitrary inconsistent empty state is not established
safe. Changed HP/class/status/equipment and membership alter eligibility,
stats and sprite choice. Neither a fixed named cast nor Bowie presence is
proved by twelve formation records; Higins19 participates only if his actual
membership/HP permits it.

## Complete enemy declarations and initialization

**Confirmed:** `data/battles/spritesets/entries.asm` has45 longword pointers;
slot27/byte108 selects `spriteset27.asm:BattleSpriteset27` at
`$1B5180..$1B5306`, **390 bytes**. Header4 + ally144 + enemy192 + region48 +
point2 gives relative subsection offsets0/4/148/340/388. The actual
`getbattlespritesetsubsection.asm` consumer uses CURRENT_BATTLE*4 and preceding
header counts, not fixed27 counts for every battle.

Macros `allyCombatant`/`enemyCombatant`, `combatantAiAndItem` and
`combatantBehavior` encode twelve-byte rows: type/ordinal at0, X/Y at1/2,
commandset at3, item word at4, primary order/region at6/7, secondary at8/9,
filler/spawn at10/11. Initialization reads the latter pair as a word.
All sixteen enemy items are NOTHING; all filler/spawn pairs are0/STARTING,
so every initialization word is `$0000`. HEALER4/5 are3/4,
ATTACKER1/2/3 are6/7/9, LEADER14. MOVE_TO=`$40`, FOLLOW_ENEMY=`$80`,
NONE=`$FF`; numeric operands are ORed into the order byte.

| Row / combatant | Type (ID) | X/Y | Commandset | Primary / region | Secondary / region |
| --- | --- | --- | --- | --- | --- |
|0 /128|LIZARDMAN19|`(15,19)`|LEADER|MOVE_TO\|0 /3|NONE /3|
|1 /129|WYVERN46|`(17,17)`|ATTACKER3|FOLLOW_ENEMY\|0 /3|NONE /3|
|2 /130|DARK_BISHOP81|`(13,19)`|HEALER5|FOLLOW_ENEMY\|0 /3|NONE /3|
|3 /131|DARK_SOLDIER18|`(19,33)`|ATTACKER3|FOLLOW_ENEMY\|9 /15|NONE /15|
|4 /132|DARK_SOLDIER18|`(8,34)`|ATTACKER1|NONE /0|NONE /15|
|5 /133|DARK_SOLDIER18|`(17,38)`|ATTACKER1|NONE /15|NONE /15|
|6 /134|WYVERN46|`(24,25)`|ATTACKER2|NONE /2|NONE /15|
|7 /135|DARK_SNIPER62|`(24,28)`|ATTACKER3|FOLLOW_ENEMY\|8 /2|NONE /2|
|8 /136|LIZARDMAN19|`(25,29)`|ATTACKER1|NONE /2|NONE /15|
|9 /137|DARK_SNIPER62|`(18,36)`|ATTACKER3|FOLLOW_ENEMY\|5 /15|NONE /15|
|10 /138|WIZARD72|`(7,33)`|ATTACKER3|FOLLOW_ENEMY\|4 /0|NONE /0|
|11 /139|DARK_SNIPER62|`(9,32)`|ATTACKER3|FOLLOW_ENEMY\|4 /0|NONE /0|
|12 /140|DARK_BISHOP81|`(7,25)`|HEALER4|FOLLOW_ENEMY\|10 /0|FOLLOW_ENEMY\|0 /15|
|13 /141|DARK_BISHOP81|`(16,27)`|HEALER4|FOLLOW_ENEMY\|14 /1|FOLLOW_ENEMY\|0 /15|
|14 /142|LIZARDMAN19|`(15,28)`|ATTACKER1|NONE /1|NONE /15|
|15 /143|DARK_SNIPER62|`(16,21)`|ATTACKER3|FOLLOW_ENEMY\|0 /2|NONE /2|

InitializeAllEnemiesBattlePositions scans128–159. All selected rows have
spawn nibble0 below HIDDEN2 and call InitializeEnemyStats. Missing144–159
receive activation0, max/current HP0 and X=-1, not a full clear. Their trigger
read uses the unchanged enemy-subsection pointer (row0 here), so their trigger
byte becomes `$33`; Y/MP/items and other fields are not universally reset.
The Battle32/Jaro slot15 exception is conditional on battle32 and does not
skip Battle27's row15. No starting-position occupancy check is called by this
fresh initializer; ResetSpawningEnemyStats is a separate later consumer.

Battle27/TO_TRISTAN is absent from the complete eleven-entry random-battle
list in `data/battles/global/randombattles.asm`. UpgradeRandomBattleEnemies
therefore exits through IsBattleUpgradable's rejection before completion/level/
RNG upgrade selection. Held527 does not upgrade these definitions. Battle27
is also absent from the complete `battleneutralentities.asm` table: clear527
finds no neutral group, held527 skips scanning; neither creates neutral Higins.

InitializeEnemyStats copies each56-byte definition as14 longwords, sets
current HP/MP from maxima, writes X/Y and stored type, packs movetype into
the upper nibble and row commandset into the lower nibble. SetTriggerRegions
stores primary<<4 OR secondary's low nibble; SetMoveOrders stores primary<<8
OR secondary byte. Complete row-order results are:

| Rows | Trigger bytes | Move-order words |
| --- | --- | --- |
|0–3|`33 33 33 FF`|`40FF 80FF 80FF 89FF`|
|4–7|`0F FF 2F 22`|`FFFF FFFF FFFF 88FF`|
|8–11|`2F FF 00 00`|`FFFF 85FF 84FF 84FF`|
|12–15|`0F 1F 1F 22`|`8A80 8E80 FFFF 80FF`|

InitializeEnemyItems sees the exact NOTHING sentinel and returns before
AddItem/equipping; copied definition weapons remain intact, with no Black
Ring row override. Activation retains only definition `$F000`, ORed with
the byte-rotated initialization word masked `$0FFF`. Thus Bishop rows2/12/13
have `$3000`, all others `$2000`. Definition BIT4/BIT5 are discarded; primary/
secondary-active, neutral and spawn-mode bits are clear. These are initial
storage values, not activated enemies. Difficulty and UpdateCombatantStats
follow. ClearAiMemory separately writes48 last-target bytes `$FF` and48
memory bytes0; it does not generate a turn order.

### Definitions, weapons, spells and stats

**Confirmed:** `data/stats/enemies/enemydefs.asm` selects the following six
complete types. Each has ENHANCED spell power, initialStatus NONE, four item
slots and four spell slots. The listed first item is followed by three
NOTHING slots; listed spells start at slot0, all remaining slots NOTHING.

| Type | Level | Max/fresh HP, MP | Base ATT/DEF/AGI/MOV | Movetype | First item | Spells |
| --- | --- | --- | --- | --- | --- | --- |
|Soldier18|23|35,17|31/46/34/4|REGULAR1|LARGE_AXE\|EQUIPPED|ATTACK|
|Lizardman19|24|40,0|34/45/38/6|REGULAR1|LARGE_AXE\|EQUIPPED|none|
|Wyvern46|26|46,0|58/39/36/7|FLYING5|NOTHING|FLAME|
|Sniper62|24|38,0|11/37/35/5|STEALTH_ARCHER10|HYPER_CANNON\|EQUIPPED|none|
|Wizard72|26|37,37|32/40/44/5|MAGE11|FLAIL\|EQUIPPED|BLAZE\|LV3|
|Bishop81|23|39,38|28/38/27/5|HEALER12|FLAIL\|EQUIPPED|AURA, BLAST\|LV3|

Resistance is0 for Soldier/Sniper/Bishop, ICE_WEAKNESS for Lizardman,
ICE_WEAKNESS|FIRE_MAJOR for Wyvern and FIRE_MINOR for Wizard. Prowess is
CRITICAL150_1IN32|DOUBLE_1IN32|COUNTER_1IN16 for Soldier; Lizardman instead
uses critical1in8, Wyvern CRITICAL_POISON, Sniper double1in16, and Wizard/
Bishop counter1in32. Priority is PRIORITYMOD_2 except Bishop's3; Wyvern also
declares BIT4|BIT5, Sniper/Wizard BIT4 before the initialization mask.

AdjustEnemyBaseAttForDifficulty has consecutive `beq` instructions after
the first SUPER comparison: SUPER takes Continue, every other value falls
through the second untaken branch to Continue. All difficulties therefore
apply floor(baseATT*5/4); SUPER2 applies it twice. Current stats copy the
adjusted base, then equipped effects apply. `data/stats/items/itemdefs.asm`
gives LARGE_AXE38 +25 ATT, HYPER_CANNON53 +40, FLAIL90 +19. These definitions
have no initial status altering DEF/AGI/MOV, so those values stay as above.

| Type | NORMAL/HARD/OUCH base/current ATT | SUPER base/current ATT |
| --- | --- | --- |
|Soldier|38/63|47/72|
|Lizardman|42/67|52/77|
|Wyvern|72/72|90/90|
|Sniper|13/53|16/56|
|Wizard|40/59|50/69|
|Bishop|35/54|43/62|

These are initializer results under the selected definitions/difficulty,
not damage, hit-rate, spell-use or future tactics claims. In particular,
Wyvern's FLAME declaration and zero MP do not establish a performed action.

## Four regions and one point

**Confirmed:** every region is twelve bytes: type4, second byte0, four
packed X/Y pairs, trailing0/0. The complete varying geometry is:

| Index / flag | Vertices1,2,3,4 |
| --- | --- |
|0 /90|`(3,0),(3,47),(31,22),(31,0)`|
|1 /91|`(0,0),(0,37),(31,37),(31,0)`|
|2 /92|`(0,0),(0,18),(28,47),(28,0)`|
|3 /93|`(0,0),(0,29),(31,29),(31,0)`|

Point0 is the sole two-byte `(15,23)` record. All coordinates are battle-local
tile units. `battleloop/triggerregions.asm:IsRegionTriggered` uses type and
vertex words at2/4/6/8. Type4 selects triangles1/2/4 and3/2/4; second/trailing
bytes are not geometry operands. Fresh reset clears90–105. The later region
consumer maps index to90+index and separately changes activation; no such
evaluation is executed here. Region15 is the no-trigger sentinel, not a
fifteenth polygon, and the initial clear flags do not prove a later trigger.

`battlefield/getmoveorderposition.asm:GetMoveOrderPosition` tests MOVE_TO
bit6; low nibble*2 indexes a point. Row0's `$40` refers to point0. The other
arm reads a combatant's X/Y: FOLLOW_ENEMY0/9/8/5/4/10/14 encodes
128/137/136/133/132/138/142. These are stored references, not selected
targets, paths or performed movement. No decoded collision/layout route or
initial visibility/activation conclusion follows from the polygon/row counts.

## Load, physical population, crop and view

**Confirmed:** `battlefunctions/loadBattle.asm:LoadBattle` preserves d0,
selects CURRENT_MAP74 in d1, requests fade-out, sets VIEW_TARGET_ENTITY=-1,
loads tilesets, waits for fade completion, clears VInts, waits one VInt and
calls `entity/entityfunctions_1.asm:PositionBattleEntities`.

ClearEntities resets49 physical32-byte records (positions/destinations
`$7000`, action/travel state cleared), all64 lookup bytes, and walking
allocation state. Existing overworld actors, aliases and action scripts are
not retained. PositionBattleEntities scans30 ally and32 enemy slots;
nonnegative signed-byte X/Y permits declaration, except neutral-marked enemies
are skipped. There is **no HP check** in that physical scan. Fresh initialization
provides the ordinary HP/position relation; arbitrary supplied resume state
need not. Positions become `(X & $3F)*384,(Y & $3F)*384`, facing Down3,
action eas_Standing, aerial layer1 for flying/hovering, otherwise0.
DeclareNewEntity writes position/destination/facing/entity/layer/sprite/action,
clears velocity/travel/acceleration, installs flags `$7040`, and initializes
animation/wait counters from the physical index. Action-pointer installation
does not prove a completed pose or movement.

`entity/getcombatantmapsprite.asm` selects allies by actual class (including
the Rohde membership exception); battle mode does not select exploration's
dead-ally BLUE_FLAME branch. Enemies use stored type via
`data/stats/enemies/enemymapsprites.asm`: Soldier82, Lizardman83, Wyvern110,
Sniper126, Wizard136, Bishop145. All are below special threshold240 and use
sequential allocation. With N declared allies, row r maps128+r to physical
N+r; with N=12 enemies occupy12–27. Wyvern rows1/6 get aerial layer1.
No neutral declaration follows for27, so ordinary fresh population is at most28.

Skipped slots get lookup -1; ally-extra offsets30/31 retain ClearEntities's0
because no neutral group overwrites them. GetEntityIndexForCombatant subtracts96
from enemy-byte IDs, mapping128–159 to lookup32–63. The49-record clear and
30/32 scan bounds are not an allocation-capacity guard: DeclareNewEntity
has none. Arbitrary supplied positions can exceed the ordinary bound.

LoadBattle restores d0=0, writes Bowie's lookup to VIEW_TARGET_ENTITY and
samples tile X/Y into BATTLE_ENTITY_CHOSEN_X/Y. A negative lookup falls back
to physical0 for these reads without creating Bowie or repairing membership.
It initializes sprites and calls LoadMap with **d0=-1** for target-based view
placement but preserved **d1=74**, selecting full map load. This differs
from LoadMap's negative-d1 CURRENT reload arm.

`data/maps/entries/map74/00-tilesets.asm` selects palette0 and tilesets
101/102/109/110/105. The sole30-byte descriptor in `2-areas.asm` is
`$C365C..$C367A`, followed by `$FFFF` through `$C367C`: main start0/0,
end63/63, foreground/background starts0/0, both parallax pairs256/256,
four autoscroll bytes0, layer type254, MUSIC_MAIN_THEME1. Descriptor selection
tests view-target internal coordinates against bounds*384 without adding
the crop; fallback is the first descriptor, here the only one.

`code/common/maps/mapload.asm:LoadMapBlocksAndLayout` loads base blocks/layout,
applies flag copies and opened-chest overlays, then copies the supplied battle
rectangle to working origin0/0. Accepted battle-map row27 is
`74,2,16,32,48,255,255`. `code/gameflow/exploration/exploration.asm:CopyMapBlocks`
uses64-wide word rows:1536 words/3072 bytes copied, not32-wide storage.
LoadMapArea's battle arm sets layer1 origin0/0 and end
`(31*384,47*384)=(11904,18048)`; other layer/music fields still come from
the descriptor. Load does not repair changed BATTLE_AREA_X/Y/WIDTH/HEIGHT.
Formation/enemy/region/point coordinates are already local: source-layout
correspondence is `(localX+2,localY+16)`. Placement0→`(16,60)`, row0→`(17,35)`,
row5→`(19,54)`, point0→`(17,39)`. These are crop relations, not walked routes.

The suffix waits a VInt, loads entity mapsprites, installs base VInts, calls
LoadBattleTerrainData, requests map music and fade-in. Terrain selects
CURRENT_BATTLE*4 in the45-pointer `data/battles/terrainentries.asm`: slot27
BattleTerrain27, incbin `data/battles/entries/battle27/terrain.bin`, decompressed
to BATTLE_TERRAIN_ARRAY. This does not publish a terrain corpus or prove
movement legality. PlayMapMusic's battle substitutions do not match
MAIN_THEME1, so it submits1. Battle44's timer branch does not apply.
Service order is not visible/audible timing or usable control.

## Intro flag and complete three-speaker opening

**Confirmed:** the48-word `data/battles/cutscenes/battlestartcutscenes.asm`
table selects slot27/byte54, word `$47B62` relative to base `$47B2C`, target
`bscs_battle27`. `cutscenes/battlestartcutscenesstart.asm` checks450 plus the
current battle, hence477, and **sets477 before saving d0/a0 and dispatching**
ExecuteMapScript. Held477 skips the dispatch; after successful return the
wrapper restores d0/a0, and its end restores d1. The flag read and the later
table selector independently read CURRENT_BATTLE: a changed value between
them can select another program without retroactively changing which flag
was checked/set. This note's program requires27 at that selection.

`data/battles/entries/battle27/cs_battlestart.asm` spans `$4BE6A..$4BE8C`,
34 bytes. Its complete nine-command stream is:

| Index / address | Command and operands |
| --- | --- |
|0 /`$4BE6A`|textCursor2658|
|1 /`$4BE6E`|cameraSpeed `$30`|
|2 /`$4BE72`|setCameraEntity133|
|3 /`$4BE76`|nextSingleText `$00`,133|
|4 /`$4BE7A`|setCameraEntity132|
|5 /`$4BE7E`|nextSingleText `$00`,132|
|6 /`$4BE82`|setCameraEntity128|
|7 /`$4BE86`|nextSingleText `$00`,128|
|8 /`$4BE8A`|csc_end `$FFFF`|

The authoritative `data/scripting/text/gamescript.txt` entries are hex keys
0A62–0A64, decimal2658–2660. These are the full literals, including the
source spelling **to bad**; each has newline `{N}` and wait `{W1}`, with
no name, number, item or other variable substitution:

| Text | Logical speaker / fresh identity | Literal |
| --- | --- | --- |
|2658|133 = row5 Dark Soldier at `(17,38)`|`Where did you come from?{N}What are you doing here?{W1}`|
|2659|132 = row4 Dark Soldier at `(8,34)`|`Don't you know about the{N}restrictions?{W1}`|
|2660|128 = row0 Lizardman at `(15,19)`|`Even if you didn't know,{N}that's just to bad.{W1}`|

These IDs identify combatant lookups, not physical slots133/132/128 or fixed
story NPC names. Fresh mappings use lookup37/36/32, physical N+5/N+4/N;
for twelve allies they are17/16/12. The actual mapsprites are82/82/83.
`entity/getentityportaitandspeechsfx.asm:GetEntityPortaitAndSpeechSfx` resolves
the logical lookup and current physical sprite, then scans the complete
`data/spritedialogproperties.asm`. Neither DARK_SOLDIER nor LIZARDMAN has a
row; fallback is PORTRAIT_DEFAULT=`$FFFF` (no portrait) and bleep6=74.
Changed/aliased sprites can resolve differently. No helper here checks
speaker HP. A missing lookup is not a safely skipped line: camera writes
lookup byte `$FF`, whereas GetEntityAddressFromCharacter shifts the unsigned
lookup word by5 without a sentinel guard (for these operands `$00FF`→`$1FE0`
offset into ENTITY_DATA). Do not infer a valid actor/portrait from that read.

`map/mapscriptengine_1.asm:csc24_setCameraTargetEntity` maps positive-word
enemy IDs through the enemy-minus96 index, then writes the physical byte to
VIEW_TARGET_ENTITY. `csc45_cameraSpeed` writes `$30`=48 as a **word** to
VIEW_SCROLLING_SPEED. These are camera requests, not48 frames, pixels or
proof that the view reaches a speaker. csc04_setTextIndex writes2658 to
CUTSCENE_DIALOG_INDEX. The source actor positions and text order do not
establish natural visibility or completed scrolling.

The actual `code/common/maps/camerafunctions.asm:VInt_UpdateViewData`
reads the physical target's X/Y unless its signed byte is negative, applies
view thresholds/map bounds, and requests destinations through SetViewDestination.
A nonzero VIEW_SCROLLING_SPEED overrides the default step; the selected
zero-autoscroll/256-parallax fields scale that value by256 then shift8 for
the plane scroll-speed words. WaitForViewScrollEnd repeatedly checks
IsMapScrollingToViewTarget across VInts. These consumers establish how the
requests are used, not that a particular update or wait has completed.

`map/mapscriptengine_2.asm:csc00_displaySingleTextbox` first tests
SKIP_CUTSCENE_TEXT. Normally it calls csc1D_showPortrait before resolving
speech again, writes CURRENT_SPEECH_SFX, waits for scroll, calls DisplayText,
increments the cursor, closes portrait/text and calls Sleep with immediate10.
The packed speaker words `$0085/$0084/$0080` are not the `$FFFF` no-speaker
sentinel. Their portrait-control bits15/14 are both0; csc1D only opens if
PORTRAIT_WINDOW_INDEX=0 and returned portrait is not-1. Existing portrait
state can bypass opening even when speech resolves anew. Normal completion
requests2658/2659/2660 in order and leaves cursor2661; no choice or yes/no
branch follows from their questions. `{W1}` is the literal text wait control;
Sleep10 is a separate source call, neither is a measured rendered duration.
`text/textfunctions_1.asm:symbol_wait1` saves/clears typewriting state, loops
through RNG-seed-copy work and WaitForVInt, and accepts the current input mask
UP/DOWN/LEFT/RIGHT/B/C/A before restoring typewriting. It does not test a
new press or Start in that mask. Held accepted input and supplied input state
therefore matter; the literal does not guarantee a fresh confirmation press.

ExecuteMapScript clears skip at entry; debug mode plus P2 Start can latch
it at the command loop. A skipped single-text command consumes its operand
but does not display, increment, close or Sleep10. Camera commands still
execute, so a midscene skip leaves a prefix cursor rather than necessarily2661.
The terminator conditionally waits for scroll if a dialogue window is open,
clears VIEW_SCROLLING_SPEED and restores interpreter registers. It does not
restore the view target to Bowie; after all three camera commands the target
remains the row0 mapping, subject to supplied-state/service changes.

There is no local creation, movement, damage, join, inventory, outcome,
story-flag or map-request command in the nine commands. F477 is a wrapper
write, not a scene completion flag. Interruption after that write can leave
the opening incomplete but suppressed on a later fresh dispatch. A successful
wrapper return reaches the fresh pre-ActivateEnemies endpoint, not a player
turn, an activated cast or a battle result.

## F88 continuation and changed-state boundary

**Confirmed:** F88 held still clears PLAYER_TYPE and sets399, then restores
SAVED_SECONDS_COUNTER.l, clears88, clears AI memory, clears VIEW_TARGET_ENTITY
and calls LoadBattle **without explicitly clearing d0**. It branches directly
to `@IndividualTurns_Loop`; the following `bra @Start` is unreachable.
Incoming74/27 are not assigned to current map/battle, no region reset/healing/
party/enemy initialization occurs and neither hook runs, even with477 clear.
Current map/battle/crop/HP/items/positions/activation and turn state are
supplied inputs; LoadBattle rebuilds physical entities and uses its actual
d0 combatant argument and current selectors. This is neither byte-for-byte
preservation nor fresh-state repair.

At this exact handoff, the flag traps in
`code/common/tech/interrupts/trap1-4_flags.asm`, their `gameflags.asm`
helpers and ClearAiMemory preserve d0. Thus forcing F88 here carries
incoming low byte74 into GetEntityIndexForCombatant: it reads lookup offset74,
outside the64-byte list, rather than Bowie0. Changed incoming d0 changes
that lookup; LoadBattle's negative-result fallback does not validate its
index first. This supplied-state alternative is not proof that natural
suspend/resume enters through this same admission with these registers.

Stop at that label before individual-turn execution. For boundary meaning,
the next source instructions read CURRENT_BATTLE_TURN.b as a byte offset
into BATTLE_TURN_ORDER and compare the selected combatant byte to `$FF`:
that sentinel branches to `@Start`; otherwise ExecuteIndividualTurn follows.
Reaching the label does not guarantee a valid turn or an immediate actor.
No round reset or turn-order resampling is caused by LoadBattle itself.

| Changed premise | Required distinction |
| --- | --- |
|Different incoming map/battle or stale current battle|Fresh assigns/restores incoming bytes; F88 uses supplied current selectors.|
|F477 clear/held, or changes between hooks|Empty before and actual start sample separately; held skips hooks but not fresh construction.|
|CURRENT_BATTLE changes after flag read or before another consumer|Flag identity, table selection, spriteset, terrain and neutral/random decisions use their own actual reads.|
|F527 clear/held|No27 upgrade or neutral group in either case; not the477 intro gate.|
|Changed membership, dead/fewer/surplus/empty party|Actual sorted list and HP/pointer behavior govern formation; no fixed twelve names or safe inconsistent empty state.|
|Changed HP/class/status/items/difficulty|Ally healing/stats/sprite and enemy initializer values follow actual inputs.|
|Changed crop/position/type/neutral bit or F88|Load does not rerun admission or guarantee ordinary population/capacity.|
|Missing Bowie or speaker lookup; dead/aliased speaker|View fallback, camera byte and unguarded dialogue lookup differ; no automatic HP-based dialogue skip.|
|Existing portrait or midscene debug skip|Portrait opening/cursor/waits differ while camera commands remain.|
|Interruption during before/heal/load/start or flag-before-scene|Reached prefix writes persist;477 held is not proof of a completed opening.|
|Supplied current turn/order byte on F88|Continuation label can next branch to activation on `$FF`; no claimed fresh round or individual-turn result.|

Later ActivateEnemies, region cutscene, spawning, turn generation, individual
turns, AI/action/combat, outcomes and exploration remain unsupported by this
slice. Counts and initial positive HP do not establish activation, visibility,
reachability or natural participation of all sixteen enemies.

## Reproduction and retained limits

Load ignored `local/private-inputs.ps1` in the launching process and verify
the retained source selection/pin. Reuse the
[retained-pair recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
without regeneration or size/mtime changes. Direct review must join:

1. Full spriteset27 macros/constants, all28 combatant rows, four regions and
   point; both48-entry hook tables, actual empty/34-byte bodies, six enemy
   definitions, weapons, sprite list, complete random and neutral lists.
2. Existing `battle-spriteset-data-static-v1.json`/retained payload pointer,
   address/header facts (`rowContentTracked=false` does not prove row bodies),
   `battle-global-data-static-v1.json` summary versus private full facts,
   `battle-loop-static-v1.json` lifecycle and `battle-control-static-v1.json`
   control consumers. Inspect selected schema shapes; do not equate counts
   with exact content or public summaries with private row detail.
3. Retained map-init/script provenance, exact bscs_battle27 command operands/
   widths/addresses and dialogue/camera/interpreter consumers; existing
   `battle-cutscenes-static-v1.json` and `battle-routing-data-static-v1.json`
   dispatch facts; gamescript0A62–0A64 literals, portrait/speech fallback and
   changed-state behavior. Read actual party/heal/enemy/physical/crop/view
   consumers, not just aggregate fixture equality.
4. Full prose/diff, every changed-state row, local links/anchors, UTF-8/LF,
   sole-path/private boundary and `git diff --check`. On a clean committed
   head run `uv run sf2 verify plan --base origin/main --head HEAD`; retain
   the plan and actual exact-head scoped CI in the Issue handoff. The ignored
   direct receipt uses `uv run --locked python -X utf8 local/issue1033/review.py
   <configured-source-disasm-root>`; it is review, not a maintained test suite.

The documentation agreement skips blanket normal/full/Python/.NET/Godot/H3/H4
suites and tests of review tools. **Unknown:** natural route and supplied
state/flags, completed service/camera/text/action timing, actual presentation,
usable control and persistence. Optional compressed-layout absence remains
a no-decoded-route limit, not a ROM blocker or acquisition authority.

Preserve [#1029 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1029#issuecomment-6183401642)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1032#issuecomment-6183422393):
final direct0.143s, independent0.136s and CI38673862670 remain distinct from
original results/CI38673722838 and the corrected KeyError. Their linked
#1025/#1021/#1017 failures, receipts and Unknowns remain preserved; no completed
suite is reset. Freeze a clean current-main-based pushed Draft PR, Project
Review handoff and successful main-gate callback; no merge, cleanup, archive,
environment creation or next Issue.
