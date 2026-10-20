# Battle30 initialization and opening

Evidence date: 2026-10-20. **Confirmed (static):** fresh Map1/Battle30 entry
can run Zalbard's `bbcs_30` before healing and building the battle roster.
The temporary scene, twelve formation slots, sixteen enemy declarations and
intro latch have separate lifetimes. Fifteen enemies receive full fresh
initialization; the last Harpy Queen is hidden. The empty start script still
has wrapper effects. Stop at `BattleLoop:@Start` before ActivateEnemies;
F88 instead reloads supplied state and reaches the individual-turn loop.
**Inferred:** the scene introduces Zalbard, then turns the follower's insult
into a threat and lightning display. **Unknown:** natural reach, actual
incoming state, completed services, presentation, usable control and persistence.

## Agreement and provenance

Standard rigor for [Issue #1064](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1064)
under data [epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
the outcome is a source-backed encounter inventory and actual consumer-gap
survey. Accepted dependency is #1062/PR1063 at
`7f841ed153461c1c4e175f2f99bbd7d97da35ae2`. This document and one navigation
link in the [predecessor](./battle29-outcomes-and-shrine-exterior-route.md)
are the exact owned paths. No shared index, roadmap, schema, fixture, tool,
remake, translation or design-guide change. Activation, spawning, turn
generation/execution, AI decisions, combat, outcomes, Map1 exploration/Taya
initialization and later scenes are excluded.

Reuse [Battle29 initialization](./battle29-initialization-and-opening.md),
[Battle27 initialization](./battle27-initialization-and-opening.md),
[Battle26 roster consumers](./battle26-initialization-and-roster.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[spritesets](./battle-spriteset-data.md), [global data](./battle-global-data.md),
[maps](./common-maps.md) and [script consumers](./common-scripting.md).
Shared rules are checked at the actual Battle30 selectors below; Battle29's
cast, hidden-state result and Map0 plane rejection are not transferred.

All source paths below are relative to `disasm/` in clean pinned
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing evidence binds USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
this slice does not rebuild or newly observe it. Constants and encoding are
owned by `sf2const.asm`, `sf2enums.asm`, `sf2macros.asm`, `sf2mapmacros.asm`
and `sf2cutscenemacros.asm`. Fnnn denotes flag nnn; indices are zero-based,
intervals half-open. Short `battleloop/`, `battlefunctions/`, `cutscenes/`
paths are under `code/gameflow/battle/`; `entity/` and `map/` are under
`code/common/scripting/`.

Minimum evidence is selected source, existing fixtures/schemas/consumers and
the retained 18,285,979-byte private map-init/script pair (accepted 0.083s
provenance). Selected reads and comparisons are linear, budgeted below one
minute/512 MiB with small receipts; no acquisition, binary map/texture
extraction, new material package or API processing. Replan contradictory
evidence or material supporting-system growth. Existing evidence and failures
remain read-only; generated review receipts stay in this worktree's ignored
`local/issue1064/`.

## Entry and hook lifetimes

**Confirmed:** the [accepted admission](./battle29-outcomes-and-shrine-exterior-route.md#four-warp-requests-and-actual-destination-selectors)
selects Map1/Battle30 only with F430 held. Held F530 clears430 but still
admits30 during that scan. Map0's F801/F840 door effects do not set430.
`code/gameflow/mainloop.asm:MainLoop` passes d0=1/d1=30 to j_BattleLoop,
with crop `(0,0,24,34)` and trigger bytes255,255 from global row30. Neither
that row nor the before scene establishes a naturally traversed entrance.

`battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`) orders fresh entry:

1. Clear PLAYER_TYPE.b; set399; check88. Clear88 selects initialization.
2. Clear SECONDS_COUNTER.l, save incoming d0/d1 **words**, assign their low
   bytes to CURRENT_MAP/CURRENT_BATTLE, install base VInts, call before wrapper.
3. Restore saved words and reassign current map/battle bytes. Clear all
   region flags90–105, independently of the four declared local polygons.
4. HealLivingAndImmortalAllies; InitializeAllAlliesBattlePositions;
   InitializeAllEnemiesBattlePositions; ClearAiMemory.
5. Clear d0.w, LoadBattle for combatant0, ExecuteBattleStartCutscene, reach
   @Start. This document stops before ActivateEnemies.

The two complete hook tables each have48 relative **word** slots.
`data/battles/cutscenes/beforebattlecutscenes.asm` slot30/byte60 at `$47AC4`
uses base `$47A88` and target bbcs_30 (`$4C5E6`).
`battlestartcutscenes.asm` slot30 at `$47B68`, base `$47B2C`, targets
ms_Empty (`map/ms_empty.asm`, `$47A4E..$47A50`, sole word `$FFFF`).

`cutscenes/beforebattlecutscenesstart.asm` checks CURRENT_BATTLE+450=480
and skips if held; it does **not** set the latch. The start wrapper checks
the same flag and sets it **before** dispatch, including empty dispatch.
Both save d1 and, when dispatching, d0/a0; ExecuteMapScript preserves its
interpreter registers. Empty dispatch still clears text-skip and reaches
the shared terminator tail. F480 is not proof of a completed scene/service.

Each wrapper rereads current battle for table selection after checking its
flag; roster, neutral, terrain and music consumers also read live selectors.
If those change between reads, a checked flag and selected data can differ.
Fresh argument restoration repairs current map/battle bytes after before,
without undoing crop or other state mutations. Held480 skips both scripts
but still runs the fresh reset/heal/build/load chain. An interruption in
before can leave480 clear after a partial scene; an interruption after the
start latch write can leave480 set before its suffix returns.

## Temporary scene and logical identities

**Confirmed:** `data/battles/entries/battle30/cs_beforebattle.asm` spans
`$4C5E6..$4C6A8`. bbcs_30 ends at ce_4C690:29 top-level commands/140 bytes
in the retained corpus, plus seven inline movement blocks/30 bytes
(eight move commands and seven terminators), total170 bytes. Its24-byte
entity list is a six-byte main header, two eight-byte fixed records and a
two-byte end marker. It is not a battle spriteset or a fixed party preset.

| Record | Position / facing | Sprite operand / initial action |
| --- | --- | --- |
|Main|11,26 /Up|Bowie's actual ally sprite /eas_Idle|
|Fixed0|11,28 /Up|ALLY_PETER7 /eas_Init|
|Fixed1|13,4 /Down|MAPSPRITE_ZALBARD155 /eas_Init|

csc42_loadMapEntities disables VInts/display, reads the header **words**,
calls InitializeMapEntities, loads mapsprites, and restores display/services.
`map/mapfunctions.asm` clears physical records/mappings, allocates ordinary
flag-selected followers **first**, then fixed records, then main physical0.
Map1 is neither overworld nor a follower-excluded map. The
[shared follower rules](./battle26-return-through-pacalon-entry.md#conditional-map02-population-and-first-eventcontrol-read)
apply: F67 can allocate Peter before fixed0; the fixed ally record skips
allocation if Peter's lookup is already nonzero. F74/F70 can supply Astral
to FOLLOWER_B31; F68 can supply Oddler earlier, with later enabled aliases
overwriting31. F64/RAFT_MAP and actual mode can also cause the separate
**physical** raft31 allocation at lookup63, not logical FOLLOWER_B31/lookup31.
No local script sets those membership flags or unconditionally creates Astral.

With F ordinary follower allocations, Zalbard logical128 (lookup32) uses
physical F+1 if Peter was already allocated, otherwise F+2. Main remains0.
Absent31 retains its cleared zero lookup and can address main; an Oddler
or another aliased31 is not repaired by dialogue referring to Astral.
DeclareNewEntity has no general capacity check, so arbitrary populations
and inconsistent supplied mappings are not established safe.

Scene128 is a mapsprite actor. Fresh roster128 later receives enemy
definition91 at11,7; it is not the scene actor at13,4 with transferred stats.
LoadBattle clears/rebuilds mappings and physical entities. Peter's scene
presence is independent of his live battle membership and placement slot.
There is no Taya record or recruitment in this list or bbcs_30.

## Complete opening, text and effects

**Confirmed:** the full top-level stream has no flag/choice/join, damage,
inventory or pending-map-event producer. Its ordered phases account for all
29 commands and every inline block:

1. Cursor2760; loadMapFadeIn Map1 at camera request7,21; load ce_4C690.
   Install/wait eas_Init on Bowie, Peter and31; set Peter11,28/Up and
   31 at11,27/Up. Submit BATTLE_THEME_1; fadeInB.
2. Start Bowie Up2 and31 Up2 without sequence wait; Peter Up2 with wait.
   Zalbard greets them. Install/wait Peter eas_Jump;31 Right1/Up1 with wait,
   nod31, then31's insult using packed portrait modifier `$C0`.
3. setCamDest7,0 detaches following and waits for scroll;128 Left2 with
   wait; setCameraEntity128 maps the logical ID to its physical slot;
   128 Down2 with wait. Two angry lines: nextText then nextSingleText.
4. 128 Down1 with wait, final threat, submit SFX_INTRO_LIGHTNING,
   flashScreenWhite40, csc_end. No explicit camera-target reset follows;
   the later LoadBattle replaces that view state.

Under distinct unchanged actors and completed movement, Bowie ends11,24,
Peter11,26,31 at12,24 and Zalbard11,7. Those are conditional path results,
not proof of completion or an initial battle party. Peter's jump and the
head nod are action services; their wait is not a measured duration.

Gamescript keys `0AC8..0ACC` are decimal2760–2764. This table summarizes
meaning; `data/scripting/text/gamescript.txt` owns the original strings.

| ID | Macro / packed speaker word | Meaning / final control |
| --- | --- | --- |
|2760|nextSingleText /`$0080`|Zalbard welcomes them /W1|
|2761|nextSingleText /`$C01F`|31 calls him ugly /W1|
|2762|nextText /`$0080`|Zalbard objects to the insult /W2|
|2763|nextSingleText /`$0080`|Zalbard threatens death /W1|
|2764|nextSingleText /`$0080`|Ask whether they are ready to die /W1|

`sf2cutscenemacros.asm` emits modifier byte then selector byte. csc1D reads
the word: bit15 supplies OpenPortraitWindow's side argument and bit14 its
mirror argument; csc00/csc02 and GetEntityPortaitAndSpeechSfx mask to the
logical low byte for the actual actor. `$C01F` therefore selects31 with
both bits set (right side/mirrored), not entity192 or unconditional Astral.
`entity/getentityportaitandspeechsfx.asm` resolves the current physical
mapsprite via `data/spritedialogproperties.asm`: Zalbard155 -> portrait43,
bleep7. Actual Astral209 -> portrait31/bleep6; another live31 resolves its
own row, with default portrait=-1/bleep6 if absent from the table.

csc1D consumes its operand but does not open/replace an already open
portrait. Speech resolves anew. csc00 and csc02 first test text-skip, wait
for view, display and increment the cursor. Single-text then closes portrait
and text and calls Sleep10; nextText retains windows without that suffix.
The second angry line can therefore reuse2762's open portrait. Successful
unskipped completion ends cursor2765. Debug/P2-Start may latch text-skip
after ExecuteMapScript clears it at entry; skipped text does not increment
or execute its suffix. W1/W2 do not establish a new physical button press.

`map/mapscriptengine_1.asm:csc19` uses
AdjustScriptPointerByCharacterAliveStatus: dead ordinary allies skip setPos;
enemies and follower31 bypass this HP guard. Action installation, movement,
nod, text and camera each use their own consumer, not a universal alive test.
GetEntityAddressFromCharacter masks the ID, subtracts96 for enemy lookup,
then shifts the unsigned lookup by5 without a missing-sentinel check.
Zero can alias main; FF can address ENTITY_DATA+$1FE0. A missing actor is
not a safely omitted action. Healing occurs later and is not retroactive.

csc15/csc2D install scripts and wait for the script pointer to become
eas_Idle when requested. Entity actions change destinations and use VInt
movement; starting three Up2 streams does not itself prove synchronized
arrival. csc32 detaches then calls SetCameraDestination/WaitForViewScrollEnd;
csc24 stores the mapped view target without waiting for subsequent following.
`data/scripting/entity/eas_actions.asm:eas_Init` sets speed32,32, zero
acceleration factors, disables acceleration/deceleration, enables automatic
facing, orients Up, selects size24, refreshes the sprite and jumps eas_Idle
alongside its collision/obstruction flags. eas_Jump disables automatic facing,
requests relative0,-1, waits5, relative0,1, waits5, relative0,0, then restores
automatic facing and jumps Idle. csc26 nod sets animation counterFF, Sleep10,
loads/crops/DMAs the mapsprite, Sleep20, refreshes, Sleep10, then clears the
counter. These are source service operands, not measured motion/frame totals.

csc41_flashScreenWhite reads40, shifts right3 to DBF counter5 and calls
LaunchFading **six times**, with d0=$F/d1=1/d2=FLASH_QUICKLY_2, then
DuplicatePalettes.40 is the script operand, not40 flashes or a measured
40-frame total. The preceding csc05 sound request is a separate service;
LaunchFading waits on enabled VInts until FADING_SETTING clears, then waits
once more before returning. Completion/audibility is unobserved. No entityFlashWhite or permanent sprite
transformation occurs here. Interruptions preserve only reached prefix
effects; neither screen flash submission nor F480 proves a completed display.

## Formation, enemy rows and initialization

**Confirmed:** `data/battles/spritesets/entries.asm` slot30 selects
BattleSpriteset30, source `spriteset30.asm`, `$1B55FC..$1B5782`:390 bytes =
4-byte header +28 twelve-byte combatant records +4 twelve-byte regions
+one two-byte point. Header is12 allies/16 enemies/4 regions/1 point.
The existing fixture's `rowContentTracked=false` means its aggregate shape
does not substitute for the complete source rows below.

All ally rows have HEALER1, NOTHING, primary NONE/15, secondary NONE/15,
filler0 and STARTING. Their identity bytes0–11 are placement declarations,
not a named twelve-member cast consumed by the fresh placement routine.

| Slot | X,Y | Slot | X,Y |
| --- | --- | --- | --- |
|0|11,28|6|12,30|
|1|11,29|7|11,31|
|2|10,29|8|9,30|
|3|12,29|9|13,30|
|4|11,30|10|12,31|
|5|10,30|11|10,31|

HealLivingAndImmortalAllies scans30 allies, restores living allies plus
Peter7/Lemon28 even if dead, HP/MP to maxima, retains only
STUN|POISON|CURSE (`$0007`), then updates stats.
`battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions`
first sets all30 X=-1 and UpdateForce rebuilds sorted current membership.
It walks at most12 member iterations; a dead member consumes an iteration
but does not advance the formation pointer. A later living member can use
that earlier slot; surplus active members leave the battle party. Empty or
inconsistent membership is not proven safe by the ordinary loop assumptions.
Unplaced Y and other state are not all cleared. There is no fixed party.

All enemy secondary orders are NONE; all filler bytes0. Rows0–14 are
STARTING with NOTHING. Row15 alone has HIDDEN and
MITHRIL|UNUSED_ITEM_DROP. The secondary **region** differs on row2.
Combatant ID is128+ordinal; FOLLOW_ENEMY operands use local ordinals.

| Ordinal / ID | Type | X,Y | Commandset | Primary order / region | Secondary region |
| --- | --- | --- | --- | --- | --- |
|0 /128|ZALBARD91|11,7|LEADER|MOVE_TO\|0 /3|15|
|1 /129|MASTER_MONK82|12,6|HEALER3|FOLLOW_ENEMY\|0 /3|15|
|2 /130|DARK_KNIGHT21|12,8|ATTACKER3|FOLLOW_ENEMY\|0 /3|3|
|3 /131|GARGOYLE43|8,7|ATTACKER3|FOLLOW_ENEMY\|4 /3|15|
|4 /132|DARK_KNIGHT21|9,10|ATTACKER3|FOLLOW_ENEMY\|5 /3|15|
|5 /133|ORC_LORD22|7,12|SENTRY|NONE /15|15|
|6 /134|HARPY_QUEEN47|11,11|ATTACKER3|MOVE_TO\|0 /15|15|
|7 /135|WIZARD72|17,10|ATTACKER3|FOLLOW_ENEMY\|8 /15|15|
|8 /136|DARK_KNIGHT21|16,12|SENTRY|NONE /15|15|
|9 /137|HARPY_QUEEN47|2,14|ATTACKER1|FOLLOW_TARGET\|0 /1|15|
|10 /138|WYVERN46|18,16|ATTACKER1|FOLLOW_TARGET\|0 /0|15|
|11 /139|DARK_KNIGHT21|7,18|SENTRY|NONE /15|15|
|12 /140|DARK_KNIGHT21|17,21|SENTRY|NONE /15|15|
|13 /141|ORC_LORD22|5,22|SENTRY|NONE /15|15|
|14 /142|WIZARD72|16,21|ATTACKER3|FOLLOW_ENEMY\|12 /15|15|
|15 /143|HARPY_QUEEN47|19,15|ATTACKER1|NONE /15|15|

The fresh enemy loop scans128–159. Battle32/Jaro's index15 exception does
not apply to30. Rows0–14's spawn nibble0 is below HIDDEN2, so they call
InitializeEnemyStats with no starting-occupancy check. Row15 instead writes
activation `$0200`, max/current HP0, X=-1 and trigger FF. It does **not**
load Harpy Queen stats, set its Y/type/orders/item, or fully clear the old
record. Missing144–159 get activation0, max/current HP0, X=-1, and trigger
3F from the still-unoffset first enemy row (not FF from row15). Later hidden
spawn admission/initialization and item delivery are outside this boundary.

InitializeEnemyStats copies the56-byte definition, restores current HP/MP,
places X/Y, packs movetype upper nibble with commandset lower nibble, stores
enemy index, packs trigger nibbles and order bytes, and initializes items.
NOTHING returns before AddItem, leaving definition equipment intact.
Activation retains definition `$F000`, combines the rotated spawn word
masked by `$0FFF`, dropping lower definition BIT4/BIT5. Rows0–14 therefore
start at `$1000` for Zalbard, `$3000` for Monk, `$2000` for the others;
active/secondary/neutral/spawn-mode bits are clear. These are construction
results, not activation or AI execution.

Declared trigger bytes, in row order, are
`3F 3F 33 3F 3F FF FF FF FF 1F 0F FF FF FF FF FF`;
move words are `40FF 80FF 80FF 84FF 85FF FFFF 40FF 88FF FFFF 00FF
00FF FFFF FFFF FFFF 8CFF FFFF`. The last row's orders remain declarations
at this fresh endpoint. NONE=$FF, MOVE_TO=$40, FOLLOW_ENEMY=$80,
FOLLOW_TARGET=0. Point0 is not a fixed target for FOLLOW_TARGET.
ClearAiMemory separately writes48 last-target bytesFF and48 memory bytes0;
it does not construct a turn order.

### Definitions, equipment and region data

**Confirmed:** `data/stats/enemies/enemydefs.asm` supplies these eight
types. All status NONE; all have four item/spell slots. The listed item is
followed by three NOTHING slots, spells by NOTHING padding. Spell power is
ENHANCED except Gargoyle REGULAR. AGI below is the raw definition byte.

| Type | Level | Max HP/MP | Base ATT/DEF/AGI/MOV | Movetype | Item | Spells |
| --- | --- | --- | --- | --- | --- | --- |
|Zalbard91|40|80/65|73/50/180/5|MAGE|NOTHING|BOLT\|LV2|
|Master Monk82|28|47/39|22/45/31/6|HEALER|BRASS_KNUCKLES\|EQUIPPED|HEAL\|LV4, DISPEL|
|Dark Knight21|27|50/0|36/48/40/7|CENTAUR|CHROME_LANCE\|EQUIPPED|none|
|Gargoyle43|15|29/0|31/23/21/6|FLYING|NOTHING|none|
|Orc Lord22|27|46/13|40/44/37/5|REGULAR|HEAVY_LANCE\|EQUIPPED|DISPEL|
|Harpy Queen47|27|53/20|65/39/43/7|FLYING|NOTHING|FREEZE\|LV2|
|Wizard72|26|37/37|32/40/44/5|MAGE|FLAIL\|EQUIPPED|BLAZE\|LV3|
|Wyvern46|26|46/0|58/39/36/7|FLYING|NOTHING|FLAME|

Zalbard resistance is WIND_MINOR|LIGHTNING_MINOR|FIRE_MAJOR|STATUS_IMMUNITY;
Harpy Queen WIND_WEAKNESS|FIRE_WEAKNESS; Wizard FIRE_MINOR; Wyvern
ICE_WEAKNESS|FIRE_MAJOR; others0. Prowess: Zalbard CRITICAL125_1IN4 /
DOUBLE_1IN8 /COUNTER_1IN4; Monk CRITICAL125_1IN4 /DOUBLE_1IN8 /
COUNTER_1IN16; Knight CRITICAL150_1IN4; Orc CRITICAL150_1IN8; Gargoyle
and Wizard CRITICAL150_1IN32; Harpy Queen CRITICAL_SILENCE; Wyvern
CRITICAL_POISON. The latter six all DOUBLE_1IN32, with COUNTER_1IN16
except Gargoyle/Wizard1IN32. No spell or prowess execution is claimed.

The repeated-BEQ difficulty routine applies floor(baseATT*5/4) in every
difficulty and repeats it for SUPER before UpdateCombatantStats. Definition
ATT values are not final attack. `data/stats/items/itemdefs.asm` entries28,
77,79,90 select Brass Knuckles, Heavy Lance, Chrome Lance and Flail,
respectively; all are range1 weapons with useSpell NOTHING and one
INCREASE_ATT effect (39,23,31,19 respectively), then two NONE effects.
UpdateCombatantStats initializes current stats, applies status and equipped
item effects without a class-equippability check in this consumer; its enemy
branch clears the ally curse test. EQUIPPED survives definition copy.
Mithril123 and UNUSED_ITEM_DROP in row15 are not a delivered item/drop at this endpoint.
Battle30 is absent from all eleven random-battle entries: no random enemy
upgrade. Zalbard's LEADER commandset, raw AGI180, priority `$1000`, resistance,
prowess and leader-present metadata are source-specific attributes, not
proof of a particular turn frequency, boss AI action or victory condition.

Each region is12 bytes: type4, unknown0, four ordered vertices, two trailing
zeros. The point is two bytes. Region declarations are not activation.

| Region | Ordered vertices |
| --- | --- |
|0|(0,0), (0,21), (23,30), (23,0)|
|1|(0,0), (0,29), (23,20), (23,0)|
|2|(0,0), (0,18), (23,18), (23,0)|
|3|(0,0), (0,16), (23,16), (23,0)|

Point0 is `(11,13)` for the MOVE_TO0 declarations; region15 is not a fifth
local polygon. Activation/membership and move-order execution are deferred.

## LoadBattle and Map1 resources

**Confirmed:** `battlefunctions/loadBattle.asm` selects live CURRENT_MAP,
fades out, detaches the view, loads tilesets, waits, clears VInts and calls
PositionBattleEntities. That clears49 physical records and mappings, scans
30 allies then32 enemies by nonnegative X/Y (also non-neutral for enemies),
without its own HP test. Position uses `(byte & $3F)*384`, facing Down,
eas_Standing and layer1 for FLYING/HOVERING.

`data/stats/enemies/enemymapsprites.asm` maps selected types to155,146,85,
107,86,111,136,110 respectively in the definition table's order above.
All are below special240. With N allocated allies, fresh rows0–14 use
physical N..N+14, with aerial rows3/6/9/10; at N=12 they are12–26.
Hidden143 and absent144–159 are skipped (lookupFF), so at most27 ordinary
fresh actors. Battle30 has no neutral group: clear530 scans without a match,
held530 skips the scan. No Astral scene follower persists as a battle neutral.

LoadBattle restores d0=0 and resolves Bowie's lookup for VIEW_TARGET_ENTITY
and chosen X/Y. A negative lookup falls back to physical0 for position reads
and masks `$3F`; it does not create Bowie. It calls LoadMap with
**d0=-1/d1=1**, a full explicit map load with target-based camera, not
LoadMap's negative-d1 CURRENT reload. Sprites, base VInts, terrain, music
and fade-in follow; Battle44's timer is irrelevant. Supplied state may select
different data; LoadBattle does not rerun admission or fresh construction.

Map01 header `$95E9C` selects palette0 and tilesets `[5,255,40,41,42]`;
255 is an absent signed-byte slot. `data/maps/entries.asm:Map01` selects
Map01s0_Blocks/0-blocks.bin, Map01s1_Layout/1-layout.bin, and the area,
flag, step, roof, warp, chest, other-item and animation tables under map01.
Resource loading does not execute Map1 exploration setup/init/Taya events.

The three30-byte area records start `$95ECA`, `$95EE8`, `$95F06`; endWord
ends at `$95F26`. All have foreground/background origins0,0; main/second
parallax256,256; both autoscroll0,0; layer255/nonzero; MUSIC_SHRINE.

| Area | Inclusive source bounds |
| --- | --- |
|0|0,0..23,33|
|1|25,0..50,20|
|2|25,22..50,42|

`code/common/maps/mapload.asm:LoadMap` selects the first area containing
the explicit camera or current target using inclusive signed comparisons.
If no area matches, its terminator branch reloads **the first record**.
The before scene's7,21 request and ordinary fresh Bowie placement select
area0. Changed/supplied target coordinates can select another area or fallback;
there is no unconditional single-area Map1 rule.
LoadMapArea in battle overrides only main bounds to0,0 and
`(23*384,33*384)=(8832,12672)`, retaining the selected other fields.
Layer255 makes the original camera use plane A as main (nonzero branch in
camerafunctions.asm); it is not the absent tileset255 sentinel. Equal
origins/parallax and zero autoscroll do not have Map0's parallax16 restriction.

LoadMapBlocksAndLayout rebuilds base blocks/layout, applies flag copies and
opened-chest edits, then CopyMapBlocks crops the battle into64-wide working
rows. At unchanged `(0,0,24,34)`, this is816 words/1632 bytes, not compact
24-wide storage. All local placements/regions/point are in that battle frame;
zero origin makes their unchanged source coordinates coincide.
Map1's two F430 flag-copy records copy1x1 from2,12 to2,13 and20,12 to20,13.
Held530 admission already clears430, so these writes can differ even though
the same call admits Battle30. The source comment calls them pointless;
their actual equality is not asserted without inspecting layout data.
Opened chest flags are also inputs; no chest interaction is executed here.

Map01 animation selects tileset46, copy64 tiles, entries `(32,16,$370,20)`
and `(48,16,$370,20)`. No new binary decode is needed to retain that declaration.
`data/battles/terrainentries.asm` slot30 selects BattleTerrain30 /incbin
`data/battles/entries/battle30/terrain.bin`; LoadBattleTerrainData decompresses
to BATTLE_TERRAIN_ARRAY. Existing evidence owns the48x48 logical terrain,
distinct from24x34 crop and64-wide map layout. MUSIC_SHRINE is35 and is
**retained** by PlayMapMusic even in battle: its replacement comparisons are
NOTHING0/TOWN8/MITULA14 -> theme3 and MITULA_SHRINE40/CASTLE38 -> theme1.
Consequently LoadBattle requests35 after the before script's explicit
BATTLE_THEME_1(34) and lightning SFX. Do not transfer Map0's40 ->34 result
to this map. Service order is not audible timing.
Global tables additionally select MITULA_SHRINE_INTERIOR background20,
enemy-switch byte0, leader-present=-1, and no halved-EXP membership.
No battle-scene, leader-death, reward or outcome consumer is entered here.

## F88 and changed-state boundary

**Confirmed:** held88 still clears PLAYER_TYPE and sets399, restores saved
seconds, clears88/AI memory/view target, calls LoadBattle without clearing
d0, then branches directly to @IndividualTurns_Loop. The following bra @Start
is unreachable. No fresh assignment, before/start, healing, region reset or
combatant construction occurs. Current selectors, crop, stats and turn state
are supplied inputs; physical entities/display are rebuilt. Flag traps and
ClearAiMemory preserve d0: admitted incoming d0=1 selects **combatant1** for
the view, not fresh d0=0/Bowie. A different supplied register/map selection
can differ. No natural suspend caller or safely arbitrary state is established.
Stop before turn selection/execution. The next turn byte can beFF and branch
to @Start; that does not establish an already generated new round.

| Changed premise | Consequence to retain |
| --- | --- |
|88 held/clear|Supplied state versus saved-word fresh chain; both rebuild physical entities, with different incoming view combatant.|
|480 held/clear or changed between hooks|Before/start skip separately; fresh construction remains; start sets latch before empty dispatch.|
|430/530|Admission unlock/clear differs from480; flag-copy writes can differ, with no30 upgrades or neutral group.|
|Current selectors/crop change between consumers|Hooks, rows, terrain/music/area use their actual reads; restored words repair only fresh current bytes.|
|Follower flags/mode/raft/mappings change|Peter duplicate suppression,31 alias/absence and Zalbard physical slot change.|
|Dead Bowie/Peter before healing|setPos can skip; action/text/camera rules differ; later healing does not repair an earlier interrupted scene.|
|Dead/fewer/surplus/empty active members|Sorted membership and advancing formation pointer determine placements; no fixed twelve-person cast or arbitrary-state safety.|
|Definition/HP/status/class/equipment/difficulty|Healing, raw AGI, derived stats and sprites follow actual inputs; row15's partial clear is not a new definition copy.|
|Positions/neutral/lookup/target/area change|Physical allocation, view fallback and selected area can differ from ordinary crop; no HP check in placement.|
|Portrait already open, text-skip or held input|Open-window bypass, cursor and wait behavior differ without suppressing all movement/effects.|
|Interruption at text/action/load/flag/flash|Only reached prefix exists; F480 and declarations do not establish completion or persistence.|

## Existing representations and actual consumer gaps

**Confirmed about the accepted repository:** evidence, prepared Content and
executing Application/Godot are separate. The
[capability owner](../../remake/docs/capability-status.md) controls runtime
claims. No Map1/Battle30 package is prepared or executed here.

| Surface | Actual representation / consumer | Boundary |
| --- | --- | --- |
|Research|Spriteset/global fixtures and schemas retain summary/address facts; retained map-script corpus has bbcs_30's29 top-level commands and source references.|Complete rows, inline movement and scene list still require pinned source. Summary equality is not exact-row or playable-content acceptance.|
|Encounter|[BattleEncounterDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs) expresses placements, expressions, regions/point, area/scene/terrain; [PrivateBattleEncounterReader.Pin](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) admits1/2/3.|30 is rejected by current private admission. Missing30 enemy/start/live-party/policy/material bindings remain; a source row is not initialized runtime state.|
|Fresh flow|[BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) routes intro/before/load/start and rebuilds membership through BattleTurnFlow.|Useful existing mechanism; correct430/530/480 route and changed-party definitions are unbound. F88 under NewBattle rejects `suspended-battle-entry`; general save/suspend is Unsupported.|
|Scene/text/motion|[Preparer](../../src/sf2tool/remake_exploration_programs.py) lowers scene-map/loadMapEntities, setPos, action streams/waits, nod, text with modifier flags and camera following. [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), EntityActionRunner and [ExplorationPortraitRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationPortraitRunner.cs) consume prepared state.|Mechanisms exist; full30 selection/resources/follower aliases and dead/missing/interrupted semantics are unbound or unassessed. The `$C0` operand is expressible, not universally Unsupported text.|
|Final screen flash|The preparer has entityFlashWhite and white fade cases, but no flashScreenWhite lowering; its fallback emits native-call, and [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) maps that to UnsupportedInstruction.|The actual40-operand/six-call screen-flash sequence needs a consumer binding. Existing [Godot effects](../../remake/game/src/Exploration/ExplorationPresentation.cs) and ProgramRunner FlashWhite do not prove this source command is consumed. Lightning/audio material and exact timing also remain unassessed.|
|Map planes/areas|Preparer preserves multiple view areas. [ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs) accepts layer255 with zero foreground origin, all256 parallax/zero autoscroll. [BattleMapViewport.BuildTerrain](../../remake/game/src/Battles/BattleMapViewport.cs) admits fixed1:1 planes and uses area ordinal/overlay offsets.|All three declared Map1 profiles satisfy these plane guards; there is no Map0-style `field-view-profile` or `battlefield-map-plane-motion-unsupported` rejection from these declarations. Materials/crop/animation/flag-copy and precise presentation remain missing binding or unassessed.|
|Area fallback|Original LoadMap falls back to first area when none contains target; [OriginalMapTraversal.SelectActiveArea](../../remake/src/Sf2.Remake.Domain/Maps/OriginalMapTraversal.cs) returns null, ExplorationViewRunner throws `field-view-area`, and BattleMapViewport throws `battlefield-map-area-unavailable` for its crop-origin selection.|Known changed-state mechanism gap, separate from supported ordinary area0 selection and missing Map1 content. Current battlefield selection uses region origin, not the source view target.|

Production consumes prepared Content, not research JSON/disassembly on demand.
No engine, preparer, Godot or emulator run supports a readiness claim in this
slice. Supported mechanisms do not establish exact30 parity; absent bindings
do not make every action/text/effect universally Unsupported.

## Reproduction and remaining boundaries

Load the current ignored `local/private-inputs.ps1` in each launching process.
Reuse the retained pair and existing provenance unchanged. The ignored
bounded review checks the pin, complete28 rows/four regions/point, hook
tables/wrappers,29 commands plus inline/list bytes, five gamescript records
and packed portrait/flash consumers, selected definitions and actual
initialization/LoadBattle/Map1/Content/Application/Godot relationships:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1064/review.py <configured-source-disasm-root>
git diff --check
```

Review every prose phase and changed-state row; check UTF-8/LF, relative links
and anchors, exact two-path/navigation-only diff and private boundary. On clean
committed HEAD run `uv run sf2 verify plan --base origin/main --head HEAD`;
record actual exact-head scoped CI in the Issue/PR handoff. This documentation
scope skips blanket normal/full Python/.NET/Godot/H3/H4 and tests of review
programs. Retain #1062's original/P2 mapping correction, original0.229s and
correction0.125s independent receipts, CI/diagnostics, #1060/#1058/#1056 and
earlier failure/Unknown chain; twelve completed unrelated remake comparisons
remain failed evidence, not this slice's automatic repair or rerun queue.

**Unknown:** natural route/admission, incoming register words/current map and
battle/flags/membership/HP/items/mode/crop/turn state, actual mappings/capacity,
working layout and service interleavings, completed text/action/camera/fade/
screen flash/audio/animation, rendering/timing, usable control and persistence.
Activation/AI/turn/combat/outcomes and Map1 exploration/Taya/later scenes stay
outside this boundary. These Unknowns are not an automatic acquisition queue.
