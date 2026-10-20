# Mitula Shrine exploration and Taya recruitment

Evidence date: 2026-10-20. **Confirmed (static):** Map1 exploration rebuilds
from the actual battle-return seed. Eight lore interactions, two conditional
Taya-scene entrances, four chest declarations and six warp rows have distinct
consumers. F841 marks scene return; F21 marks force membership; F53 marks
active membership; the temporary follower list is a fourth authority.
**Inferred:** the blank actors present the shrine's legends and the statue
reveals Taya, who proposes Nazca. **Unknown:** natural interaction/collection/
travel reach, supplied state, completed presentation/control and persistence.
This supplies research data, not a runnable Map1 or engine-ready claim.

## Agreement and provenance

Standard rigor for [Issue #1070](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1070)
under [epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
produce traceable exploration, lore, recruitment, item and transition data for
later design/import. Accepted base is `d2796a28a72550823fd5a068ad56846af514c7c6`
(#1068/PR1069). Ownership is this document and one navigation link from the
[Battle30 outcome owner](./battle30-outcomes-and-mitula-scene.md). No shared
index, schema, fixture, tool, remake, design-guide or translation change.

Start at that owner's returned ExplorationLoop call. Reuse the
[opening's three Map1 resource profiles](./battle30-initialization-and-opening.md),
[activation/hidden-state boundary](./battle30-activation-and-first-turn-dispatch.md),
[Battle29 exterior destination owner](./battle29-outcomes-and-shrine-exterior-route.md),
[ordinary population/return rules](./battle26-return-through-pacalon-entry.md),
[common maps](./common-maps.md), [common scripting](./common-scripting.md),
[request state](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md) and
[item transaction seams](./map-event-item-transactions.md).
Stop at each destination's admission/next setup boundary. Exterior/cave scenes,
Nazca/ship, the next encounter body, new collection and engine work are excluded.

Source paths are relative to `disasm/` at clean read-only pinned
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing provenance binds USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1/runtime equality is claimed. `sf2enums.asm`/`sf2const.asm`
own constants/RAM; `sf2mapmacros.asm`, `sf2mapsetupmacros.asm`,
`sf2cutscenemacros.asm` and `sf2macros.asm` own encoding. Fnnn means flag nnn;
indices are zero-based, extents half-open. Short `map/` and `entity/` paths
mean `code/common/scripting/`; exploration files are in
`code/gameflow/exploration/`.

Minimum evidence is complete selected tables/scene/text and actual dispatcher/
service/preparer/Content/Application/Godot joins, with existing fixtures and
schemas. Reuse the unchanged18,285,979-byte init/script pair and accepted0.083s
provenance in `local/issue1005/pair.txt`. Linear reads/checks are budgeted below
one minute/512MiB with small receipts in ignored `local/issue1070/`. No binary
map extraction, exporter, new fixture/schema or API processing. Reassess a
contradiction or material supporting-tool growth; ADR0004 applies proportionally.

## Returned seed, fresh entry and first event/control read

**Confirmed:** `code/gameflow/mainloop.asm:alt_MainLoopEntry` calls SwitchMap
then ExplorationLoop without an intervening battle scan. Intact Battle30
victory supplies Map1 and the pre-scene first-active-member-derived coordinate
bytes/facing, not the scene's Bowie `(10,13)`. The preceding owner retains
empty-list, FF mapping, changed selector and defeat/egress qualifications.
SwitchMap does not switch1 under530. This entry does not require21 or841.

`explorationfunctions_2.asm:ExplorationLoop` orders:

1. Clear MAP_EVENT_TYPE.w. Subtract20000 from STEP_COUNTER.w and clear it
   when the signed word result is negative; this is word arithmetic, not an
   unbounded mathematical clamp for arbitrary supplied values.
2. Heal living/immortal allies under the accepted30-slot rules; fade out;
   detach view target. Save incoming map word. With low byte other than FF,
   assign CURRENT_MAP.b and CURRENT_BATTLE=255 before loading tilesets.
3. Preserve coordinate/facing words across tilesets, wait for fade, clear
   VInts, resolve setup entities and InitializeMapEntities. Load mapsprites,
   clear temp flags256–383 and set80. Story841,21,53 and chest flags are not
   in that temp range.
4. Initialize sprite frame counter; pop saved map into d1; set d0=-1 and
   view target0; LoadMap; restore base VInts; resolve and run setup init.
5. Palette comparison conditionally requests map music/fade. WaitForEvent
   reads MAP_EVENT_TYPE at `$2591C`. Zero installs control for physical0:
   numeric PLAYER_TYPE0 selects character,1 raft, other nonzero caravan.
   Pending nonzero bypasses installation. The polling loop rereads event
   before masking CURRENT_PLAYER_INPUT for A/C.

The returned d4=1 is not an ExplorationLoop battle-admission test. Fresh
InitializeMapEntities clears prior scene physical records, lookup bytes and
walking-script scratch, builds ordinary followers/fixed actors and main0 from
the supplied coordinate/facing **words**. Main uses Bowie's actual ally sprite,
not the seed member's identity. The accepted raft/mode tail can change main
and ordinary follower handling; no new vehicle boarding is implied.

The FF-map entry instead retains CURRENT_MAP/CURRENT_BATTLE and population,
updates nonnegative coordinate bytes/main facing, and declares raft as
applicable. It skips tilesets, rebuild and temp reset, then still LoadMap/init.
Same-map room warps below use this branch. Explicit re-entry1 rebuilds even
if current map was already1. List resolution and later init/event resolution
are independent live CURRENT_MAP reads; changing selectors between them does
not repair population. Reaching a poll or installing control proves no usable
input opportunity, completed fade, collision path or persistence.

## Map1 resources and working layout

**Confirmed:** `data/maps/entries.asm:Map01` and `map01/00-tilesets.asm`
select palette0, tilesets `[5,255,40,41,42]`, own blocks/layout and all tables
below. Signed tileset255 is absent. Reuse the original resource profile;
no extracted binary content is published. Complete area table
`map01/2-areas.asm`, `$95ECA..$95F26`, contains three30-byte records/endWord:

| Area / record address | Inclusive main bounds |
| --- | --- |
|0 / `$95ECA`|`(0,0)..(23,33)`|
|1 / `$95EE8`|`(25,0)..(50,20)`|
|2 / `$95F06`|`(25,22)..(50,42)`|

Every record has secondary foreground/background origin `(0,0)`, both
parallaxes `(256,256)`, both autoscroll `(0,0)`, layer255 and MUSIC_SHRINE.
LoadMap selects from actual view-target coordinates using inclusive signed
comparisons; absence of a containing area falls back to the first record.
Fresh exploration's battle255 removes the battle main-bounds override and
battle overlay. Old crop RAM need not be cleared. Expected intact main seed
can select area0; changed seeds can select another area or fallback.
Layer255 is the nonzero Plane A selector, not the absent-tileset sentinel.

| Complete table / extent | Records and consumer effect |
| --- | --- |
|Flags `3-flag-events.asm`, `$95F26..$95F38`|Two8-byte F430 records: source `(2,12)`, size1x1, destination `(2,13)`; then source `(20,12)`, size1x1, destination `(20,13)`; endWord.|
|Steps `4-step-events.asm`, `$95F38..$95F4A`|Trigger `(2,14)` copies source `(0,34)`,1x1 to `(2,14)`; trigger `(20,14)` copies the same source1x1 to `(20,14)`; endWord.|
|Roofs `5-roof-events.asm`, `$95F4A..$95F4C`|endWord only.|
|Warps `6-warp-events.asm`, `$95F4C..$95F7E`|Six8-byte records/endWord; complete destinations below.|
|Chest `7-chest-items.asm`, `$95F7E..$95F90`|Four4-byte records/endWord; complete inventory below.|
|Other items `8-other-items.asm`, `$95F90..$95F92`|endWord only.|
|Animation `9-animations.asm`, `$968C2..$968D8`|mapAnimation46,64; entries `(32,16,$370,20)` and `(48,16,$370,20)`; endWord.|

`code/common/maps/mapload.asm:LoadMapBlocksAndLayout` on explicit load
rebuilds decoded blocks/layout, applies held flag copies, then held chest
markers `$D802`; battle255 skips battle crop copying. Victory cleared430,
so unchanged post-victory load skips both flag copies. Held/inconsistent430
applies both even if530 also held; no530 check occurs here. The source comment
calls them pointless, but only the copy instruction/coordinates are confirmed;
no new binary equality/visual effect is asserted.

The ordinary character action stream checks map block copies and the movement
consumer opens a door-kind `$0400` candidate before rereading its marker.
`exploration.asm:OpenDoor` selects the step row by trigger coordinates and
copies the declared rectangle into the working layout; it does not set a
story flag. Warp matching still requires the post-copy `$1000` layout kind.
Tables alone prove neither door traversal nor a resulting warp marker.
CURRENT LoadMap retains the mutated layout; an explicit load rebuilds it from
source plus flags/chest state. Empty roofs/other-items do not suppress
layout-kind production, generic search or setup descriptions.

## Complete setup and actor identity

**Confirmed:** `data/maps/mapsetups.asm` has Map1→ms_map1 with no flag
alternative. Six-pointer table `$5E62A..$5E642` selects all six owners under
`map01/mapsetups/`. Complete fixed list `s1_entities.asm`, `$5E642..$5E694`,
is ten8-byte records/endWord. Logical IDs come from the actual consumer,
not from record ordinal:

| Record / logical ID | X,Y / facing | Mapsprite operand / initial action |
| --- | --- | --- |
|0 / Taya21|63,63 /Down|ALLY_TAYA21 → actual class/HP sprite; eas_Init|
|1 /128|41,8 /Down|BLANK60; eas_Init|
|2 /129|43,8 /Down|BLANK60; eas_Init|
|3 /130|45,10 /Left|BLANK60; eas_Init|
|4 /131|45,13 /Left|BLANK60; eas_Init|
|5 /132|43,15 /Up|BLANK60; eas_Init|
|6 /133|41,15 /Up|BLANK60; eas_Init|
|7 /134|39,13 /Right|BLANK60; eas_Init|
|8 /135|39,10 /Right|BLANK60; eas_Init|
|9 /136|44,26 /Down|OBJECT4=181; eas_InitFixedSprite|

`map/mapfunctions.asm:InitializeMapEntities` allocates ordinary flag
followers first. Map1 is not an excluded/overworld map: the complete ordinary
follower table applies, independent of member21. There is no ordinary Taya
row. With F allocated followers and unchanged cleared lookup21, Taya is
physical F+1;128–135 are F+2 through F+9;136 is F+10. Main stays0.
A previously mapped ally record is skipped/deduplicated; each ordinary
nonally consumes a new physical slot and the next logical nonally mapping.
There is no general capacity check proving arbitrary follower populations safe.

F67 can supply Peter7. F74 initially supplies Astral31, F68 later overwrites31
with Oddler, and F70 later overwrites31 with Astral; all admitted rows still
consume slots. F74/F77/F83 can similarly overwrite FOLLOWER_A30. Missing31
has cleared lookup0 and resolves main; it is not guaranteed Astral. The
separate physical raft31/lookup63 under mode/64/RAFT_MAP is not logical31.
Fresh entry discards the prior battle-after-scene follower installation.

Blank mapsprite60 does not remove the record, its mapping, action stream or
interaction route. `GetActivatedEntity` searches physical actors near the tile
ahead, skips controlled/follower actors and can traverse counter-kind blocks;
it does not reject an actor because its sprite is named BLANK. Actual rendering,
collision, selection and natural reach of each lore actor remain Unknown.
Fixed facing, action orientation and live dialogue-facing bits are separate.

`ms_map1_InitFunction`, `$5E760..$5E772`, checks F21: clear returns; held
calls MoveEntityOutOfMap136. That resolves the live mapping and SetEntityPosition
`$7000,$7000`; it does not clear mapping, delete the record, move Taya into
the party, set841 or rebuild member/follower lists. Scene replacement instead
moves136 to `(63,62)`. Both mechanisms are distinct from hiding/unmapping.

## All interaction and description routes

**Confirmed:** `s2_entityevents.asm`, `$5E694..$5E720`, has nine4-byte rows
for128–136 in that order, then FD/0 default. All nine second operands are
UP=1. `RunMapSetupEntityEvent` treats this byte as **flags**, not an Up-facing
requirement: bit0 turns the actor opposite the player's facing after VInt;
bit1 would restore prior facing, but is clear. It selects by actual logical
ID, opens portrait/speech from the mapped sprite, suppresses entity updates
for the callback, closes windows and restores entity updates afterward.
The default is rts. None of these lore rows checks530/841/21.

Ordinary C handling in `explorationvints.asm:ProcessPlayerAction` first calls
GetActivatedEntity, then GetEntityEventIndex and the entity dispatcher on a
hit. The reverse mapper scans lookup bytes in order, converting indices32+
to logical128+ and taking the first matching physical ID; a no-match returns0.
Only no activated entity proceeds to CheckArea with d6=1. Thus intact blank
actors map to128–135, while aliases/duplicate physical mappings can select
another route. The `(44,26)` description is a separate search fallback, not
an automatic second call after interacting with136.

| ID / function | Text IDs / terminal controls | Paraphrased lore |
| --- | --- | --- |
|128 /Map1_EntityEvent0|3665 W2;3666 W1|Arc Valley gathers evil; its ruler is king of devils.|
|129 /Map1_EntityEvent1|3667 W2;3668 W1|Dark Sol, Dark Dragon and Zeon fought for it; battle made a storm.|
|130 /Map1_EntityEvent2|3669 W2;3670 W1|Zeon won; jealous rivals sealed his power in the Jewel of Evil.|
|131 /Map1_EntityEvent3|3671 W2;3672 W1|His fall made a crater; his moan broke mountains and debris buried it.|
|132 /Map1_EntityEvent4|3673 W2;3674 W1|God of Wisdom made the Jewel of Light; it chose a brave man and holy sword.|
|133 /Map1_EntityEvent5|3675 W2;3676 W1|That man fought the other two kings; the sword's holy power defeated them.|
|134 /Map1_EntityEvent6|3677 W2;3678 W1|Arc Valley sealed beneath Ancient Tower; do not open its door.|
|135 /Map1_EntityEvent7|3679 W2;3680 W1|Light chooses another if Zeon wakes; attack the devil's mouth with holy sword.|
|136 /Map1_EntityEvent8|Conditional cs_5E772|Check841; held returns, clear calls scene then sets841.|
|default /Map1_DefaultEntityEvent|None|rts.|

`txt` directly supplies the indicated gamescript ID. It is not a cutscene
cursor operation. These dialogues narrate original lore; they do not grant
jewels/sword, set travel flags, identify the next encounter or validate lore
as an independently observed historical event.

Complete zone owner `s3_zoneevents.asm`, `$5E720..$5E726`, has only default
Map1_DefaultZoneEvent/rts. Item owner `s5_itemevents.asm`, `$5E758..$5E760`,
has only default Map1_DefaultItemEvent0/rts; it does not replace generic
item-use/search behavior. `RunMapSetupItemEvent` masks item identity and
starts d6=0; this default makes no new item-specific transaction.

`s4_descriptions.asm`, `$5E726..$5E758`, sets d3=`$1044`=4164 then calls
DisplayAreaDescription on the complete two-record table at byte_5E736:

| Search coordinate | Encoded record / actual consumer |
| --- | --- |
|11,3|msDesc investigation9 /description offset0: text423+9=432 (statue investigation, W2/CLEAR), then4164+0=4164 (beautiful Mitula statue, W1).|
|44,26|msDescFunction relative to byte_5E736→Map1_DescFunc0, identical841/scene/set841 sequence to entity136.|

Both records encode byte2=0, so the consumer's nonzero-byte2/d6 restriction
does not veto them. Byte3 distinguishes text0 versus function1; FD00 ends
the scan. The function path preserves d6, then shares window close and d7=-1
handled return, including when841 was already held. A no-match returns d7=0.
CheckArea reaches this table only after its layout-kind item/chest alternatives;
coordinate equality alone is not a proven approach or selected callback.

## Taya scene, text and actual command consumers

**Confirmed:** both callback bodies check841 **before** script, then set841
**after** return. Neither checks21,530 or53. Complete `scripts.asm:cs_5E772`,
`$5E772..$5E86C`, has52 top-level commands/230 encoded bytes plus five inline
move/end blocks/20 bytes =250 bytes through csc_end. There is no actor list,
choice, conditional branch, explicit setF, item grant or warp in this scene.
The only force join is21; window/ceremony service text446 is independent
of its3649–3664 cursor progression.

Complete phases cover all commands0–51 and every inline body:

1. **Reveal (0–18):** cursor3649; wait30; face Bowie Up, awaited setDest
   to44,27. Install/wait Init Peter and31. flashScreenWhite30, wait30,
   flashScreenWhite30; set136 to63,62/Down and Taya21 to44,26/Down; stop21;
   wait30; flashScreenWhite30; wait30. Awaited setDest Peter43,27 and31
   45,27; face both Up. No script loads/rebuilds actors here.
2. **Identify (19–29):** two Taya texts; start21; Peter Up1/wait, Right;
   question; Taya Left, recognition; Peter question; shiver21, Taya text.
3. **Explain (30–44):** Bowie Down1/wait;31 Left1/wait then Up; Bowie
   Right1/wait then Up1/wait and Up-facing.31 asks; Taya Down-facing;
   introduction,31 response and four further Taya texts.
4. **Join and proposed route (45–51):** join21; two more Taya texts;
   follow Peter→Bowie/2; follow31→Peter/2; addNewFollower21; csc_end.

All16 scene text commands are nextSingleText (S); cursor advances only on
normal display. Source comments and `data/scripting/text/gamescript.txt`
agree individually:

| Text / command | Live speaker / modifier | Paraphrase and terminal control |
| --- | --- | --- |
|3649 /19|Taya21 /80|Feels power returning her to human; W2.|
|3650 /20|Taya21 /80|Evil gone; asks if they chased it away; W1.|
|3651 /24|Peter7 /00|Asks LEADER who she is; W1.|
|3652 /26|Taya21 /80|Recognizes LEADER/from Granseal; W1.|
|3653 /27|Peter7 /00|Asks her identity; W1.|
|3654 /29|Taya21 /80|Asks how long she was stone; W1.|
|3655 /36|31 /00|Asks how she knows LEADER; W1.|
|3656 /38|Taya21 /80|Apology; W2.|
|3657 /39|Taya21 /80|NAME21, sorceress; Mitula told her; W1.|
|3658 /40|31 /00|Surprise; W1.|
|3659 /41|Taya21 /80|Mitula worried about them; W2.|
|3660 /42|Taya21 /80|Fated to fight Zeon; W2.|
|3661 /43|Taya21 /80|Volcanon's order prevents Mitula helping; W2.|
|3662 /44|Taya21 /80|She will help for Mitula; W1.|
|3663 /46|Taya21 /80|Proposes ancient ship to Grans; W2.|
|3664 /47|Taya21 /80|Suggests Nazca; W1.|

Packed Taya text operand is `$8015`: csc1D tests bit15 for right side and
bit14 for mirror;80 is not logical entity128 or a blanket unsupported mode.
Portrait/speech resolves the **live mapped mapsprite**, including class/HP;
Taya's name substitution21 does not guarantee a specific portrait or actor.
An existing portrait can short-circuit opening. nextSingleText waits for
scrolling, displays/increments cursor, closes windows and sleeps10; W2
versus W1 remains an independent text control. Debug P2 Start skipping can
consume text operands without display/cursor advance and skip csWait; it
is not wholesale action/effect skipping. No measured dwell/audio/complete
presentation is claimed.

`map/mapscriptengine_1.asm`/`_2.asm`, actual entity engines and eas_actions
join the commands rather than their names alone:

- setFacing/setPos read byte actor IDs before the ally-HP helper; stopEntity
  reads its word. Own ally0–29 HP0 can skip those commands.31 and136 bypass
  that gate. Referenced init/action/text/start/wait consumers are separate;
  no universal alive check is implied. eas_Init and eas_InitFixedSprite
  manipulate entity action/orientation/motion fields, not member stats.
- All three setDest IDs have bit15 clear. csc29 sets scaled destinations,
  distances/velocities and **waits for movement to stop**. Init waits and
  each of the five inline moves wait for eas_Idle. Missing31 resolves main0;
  absent Peter7 likewise need not be a safely omitted action. Actual aliases,
  collisions and service progress can alter or prevent the commanded path.
- Each flashScreenWhite30 shifts30 right3 to DBF counter3: four calls to
  LaunchFading with palette maskF, period1, FLASH_QUICKLY_2, then
  DuplicatePalettes. Three commands request twelve such calls, not thirty
  flashes/frames each. LaunchFading waits on its enabled service and another
  VInt. No measured duration, permanent transformation or completed display
  is proven. stop/start write animation countersFF/0; shiver temporarily
  toggles sprite fields and restores them when it returns.
- followEntity macro emits follower/leader/offset **words**. csc2C first
  peeks the follower's high byte before its HP helper.0007 and001F both
  expose00; GetCurrentHp's byte mask therefore checks **Bowie0** for both,
  not Peter/31. Bowie HP0 skips six operand bytes and unwinds the command;
  Peter HP0 alone does not suppress Peter-follow. Each admitted command
  then reads full IDs, resolves current physical slots and AddFollower with
  table offset2=`(-24,0)`. The two HP reads can differ if live state changes.
- addNewFollower21 (csc56) reads the full word with **no** corresponding HP
  precheck. It resolves21, scans the current EXPLORATION_ENTITIES list to
  its sentinel, chooses the last physical follower or0, then AddFollower
  with `(-24,0)`. It does not mean follow31 unconditionally.

AddFollower installs a42-byte mutable eas_Follower1 copy, advances walking
scratch, patches physical leader/offsets and appends the follower only if
its physical ID is absent. Reinstalling an existing follower still consumes
scratch/replaces its script; list deduplication is not allocation deduplication.
The two followEntity commands do not reorder existing list entries. Thus
Taya follows the actual list tail, which can be another ordinary follower;
if Peter/31 alias0 or each other, the chain can differ or be self-referential.
No arbitrary population/scratch/mapping safety is established.

With distinct intact aliases, admitted positioning/movement and completed
waits, commanded pre-follow positions are Bowie45,27/Up, Peter43,26/Right,
31 at44,27/Up, Taya44,26/Down and136 at63,62/Down. Following can then move
them; these are conditional command projections, not a frozen final state,
collision proof or next map seed. csc_end restores interpreter state and
view-scroll service fields under its shared tail; it does not restore the
statue, actors, follower scripts or membership. Interruptions are prefix
mutations without rollback.

## Membership, ceremony and changed flags

**Confirmed:** `mapscriptengine_2.asm:csc08_joinForce` waits for view scroll,
reads0015, clears bit15 (clear selects MUSIC_JOIN), then JoinForce21. The
special128 Sarah/Chester case is not selected. `code/common/stats/battleparty.asm`
sets joined flag21, UpdateForce rebuilds all30 joined/active/reserve lists,
then active count<12 calls JoinBattleParty21 to set flag32+21=53. At capacity
it does not clear a preheld53. The rebuild happens **before** that conditional
active write: a newly active21 need not yet appear in the counted active list.
Repeated joins preserve live state and may change active53 when capacity permits.

No InitializeAllyStats, ResetAlliesBattleStats, heal, item initialization or
battle placement is called by this join. Taya's stats/class/items are supplied
state; calling21 is not proof that its initial stats are natural or initialized
at this scene. The distinct new-game/stat initialization owner remains outside
this local join. After membership mutation, csc08 reads actual class/name,
displays service text446, waits through the sound/input fade service, closes
text and sleeps10. Ceremony completion is not membership atomicity and does
not consume the scene cursor. No natural automatic battle-party inclusion is
proved merely by the dialogue or off-map Taya actor.

| Supplied flags / interruption | Conditional result |
| --- | --- |
|841 clear,21 clear|Init retains136; either admitted callback can run scene/join/follow and then set841. Capacity determines53, not narrative.|
|841 held,21 clear|Both callbacks skip scene; init retains136. Neither repairs missing membership.|
|841 clear,21 held|Init moves136 off map, but an admitted `(44,26)` description still runs scene; join is repeated. No21 guard in either callback.|
|841 held,21 held|Init removes statue position; both callbacks return; fresh Taya stays at63,63 and is not an ordinary follower.|
|53 held without21|UpdateForce excludes21 until joined; join can retain53 even at capacity.|
|Interrupt before join / after join / before841|Different actor/membership/ceremony/follower prefixes;841 can remain clear after21 was set, allowing replay if reached.|
|Interrupt after841|Callback latch does not prove save persistence, final field control or correct aliases.|
|Fresh explicit entry after join|Clears temporary scene following; no Taya row in ordinary follower table. Membership21/53 and841 remain independent.|
|CURRENT room warp|Population, Taya follower scripts, layout and temp flags are retained; init is rerun and can move136 from63,62 to7000,7000.|

## Four chests and actual inventory transaction

**Confirmed:** exact `map01/7-chest-items.asm` rows are:

| Coordinate | Flag | Item constant / ID |
| --- | --- | --- |
|3,15|180|CRITICAL_SWORD /65 (`$41`)|
|19,15|204|MITHRIL /123 (`$7B`)|
|32,34|181|POWER_WATER /9|
|34,34|182|HEALING_SEED /1|

`CheckArea` searches ahead by working layout kind; chest `$1800` precedes
vase/barrel/bookshelf/area-item and description fallback. GetChestItem/GetItem
selects CURRENT_MAP table, compares coordinate bytes, returns flag/item and
masked64x64 layout offset. CURRENT_BATTLE255 avoids battle coordinate adjustment.
CheckChestItem also requires live chest-kind. OpenChest on a matched row
substitutes NOTHING if its flag is held, then sets the flag, writes open
block `$D802` and marks Plane A update for layer255, **before** award continuation.
A held flag yields opened/empty messaging and no item award.

`explorationfunctions_1.asm:loc_239C8` is that continuation. These four IDs
are ordinary items below the gold-chest threshold. It names the found item,
checks Bowie's four slots, AddItem0 when available, otherwise UpdateForce and
scans the counted other-force-member prefix for a free slot. Success requests
item music and received/passed text; it does not imply Bowie received it.
When everyone's hands are full it displays417, calls CloseChest and
RefillNonChestItem at the candidate coordinate, clearing matching flags and
restoring the closed chest layout. The empty other-item table makes that
second refill a no-match here. Inventory, list contents and service completion
are inputs, not an assumed empty-inventory happy path.

The normal full-capacity branch repairs the earlier opening; interruption
between opening, award or repair can leave a different prefix. These chest
flags/items are unrelated to Battle30 enemy143's droppedFlag27 or the Orb
narrative. No actual collection, sale/deals behavior, save/load persistence
or collision reach is claimed by this inventory.

## Six warps and destination admission

**Confirmed:** all six rows encode warpNoScroll=0, with facing byte and a
trailing zero byte. MAP_CURRENT=255 is a **target sentinel**, distinct from
a trigger-coordinate wildcard and from CURRENT_BATTLE255:

| Ordered row / trigger | Target / destination / facing |
| --- | --- |
|0 /2,14|CURRENT /48,30 /Up|
|1 /20,14|CURRENT /27,8 /Up|
|2 /48,31|CURRENT /2,15 /Down|
|3 /27,9|CURRENT /20,15 /Down|
|4 /10,32|Map0 /14,12 /Down|
|5 /255,32|Map0 /15,12 /Down|

`WarpIfSetAtPoint` is reached from the ordinary candidate movement only on
post-door-copy layout kind `$1000`; it divides supplied pixel coordinates
by384, scans in source order and treats **any negative** trigger byte as
wildcard. Whole-word FFFF terminates before matching; row5 FF20 is a real
wildcard-X row, not the terminator. On Y32, row4 wins for X10, otherwise
row5 can match every X allowed to reach this producer, not only X11.
It writes MAP_EVENT_TYPE.w=1, copies type/map/X/Y into PARAM1–4 and facing/
zero into PARAM5–6, then WARP_SFX. A declaration does not post itself.

WaitForEvent observes the request before input. ProcessMapEvent clears type
before dispatch. Type1 with PARAM1=0 consumes WARP_SFX, idles main, unwinds
ExplorationLoop's call frame, clears return words, reads target map and the
actual main position, replaces X/Y only for nonnegative destination bytes,
and returns facing/d4=0. All declared destination coordinates are nonnegative.
Any overwritten pending bytes or interrupted prefix can change this result.
The scroll branch and its71/F530 substitution are not selected by these rows.

MainLoop re-enters its ordinary SwitchMap→CheckBattle chain on this return.
CheckBattle accepts low-byte mapFF as current and scans unlock/coordinates;
its selected result precedes the next exploration init:

- CURRENT rows preserve effective Map1. The sole Map1 battle row is30,
  wildcard trigger255,255/crop `(0,0,24,34)`, requiring430. Clear430 reaches
  ExplorationLoop withFF and the room destination, retaining population/
  layout/temp flags and rerunning Map1 init. Held430 admits30; held530 clears
  430 but still admits that same scan. Room warps are not unconditional
  exploration transfers in inconsistent/rearmed state.
- South rows produce explicit Map0 `(14,12)` or `(15,12)` Down. SwitchMap
  does not change0. Map0's row29 requires429 with wildcard trigger; held529
  clears429 but still admits29 in that scan. With429 clear, explicit Map0
  exploration rebuilds population/layout/temp flags and selects its live
  setup/init as owned by the accepted exterior document. Neither row sets
  429,430,841 or a Nazca/ship request.

This stops at those destination admission/next setup boundaries. It does not
rerun the exterior zone scene, caves, next battle, or a route to Nazca.
Taya's3663–3664 text is a suggestion with no map request/ship acquisition.

## Actual representations and consumer gaps

**Confirmed about the accepted repository:** expressibility, explicit refusal,
missing binding and unassessed fidelity have separate meanings. The
[capability owner](../../remake/docs/capability-status.md) controls actual
runtime claims. No Map1/Taya package, new media or runtime execution is added.

| Surface | Actual consumer join | Classification / smallest remaining boundary |
| --- | --- | --- |
|Evidence|Retained map-init/script corpus, map-content/setup/event/request/item fixtures and schemas; pinned full tables/scene/text above.|Expresses static facts. Existing item-transaction fixture owns caller seams, not proof of these chest grants; direct service read supplies this conditional algorithm.|
|Population and event defaults|[compile_maps](../../src/sf2tool/remake_exploration_maps.py) preserves fixed actors, ordered interactions/defaults, flag/step/roof tables for selected eventMaps; [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs) and [MapEntityAllocation](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs) allocate live population.|Mechanisms expressible; missing Map1 selection/materials/state. Other selected-map paths deliberately emit setup/input/flag frontiers. Blank/ally identity must use actual bindings, not another map's package.|
|Init removal|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py) lowers MoveEntityOutOfMap to `hide` with removeAliases=false; ProgramRunner calls ExplorationState.Hide.|Partial correspondence: keeps aliases and moves to7000,7000, but also sets Visible=false and clears Actions/Follower. Original MoveEntityOutOfMap is a position service; this extra behavior needs assessment. No bound/reached Map1 init is claimed.|
|Scene actions/text|OriginalPrograms lowers Init/inline moves/waits, facing/position/animation, shiver, portrait80, cursor/text and join/follow; Content reads immutable instructions and ProgramRunner/EntityActionRunner consume them.|Expressible subset. `setDest`, `flashScreenWhite` and `addNewFollower` have no dedicated lowering: become native-call→UnsupportedInstruction; ProgramRunner stops Unsupported when reached. These are explicit service gaps, not silently completed presentation.|
|Follow HP/membership|Preparer follow lowering emits entity/leader offsets; [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs) plus ProgramRunner JoinPartyMember implement joined/active/list ordering.|Known follow lowering does not encode original high-byte Bowie-HP precheck. csc56's last-list-tail/scratch behavior also needs support. Join resources require partyFlags and an admitted unique member deployment/start binding when encounter starts exist; no21 binding is established. Those guards are separate from original no-stat-reset semantics.|
|Search and items|Preparer does not compile chest/description/item-event tables into a reached generic search transaction; capability owner lists item acquisition/area descriptions/setup-event selection Unsupported after legacy retirement.|Missing present consumer/binding for four chest transactions and description `(44,26)` route; generic inventory or text instructions do not prove this path. Entity136 route alone cannot close description acceptance.|
|Warp requests|compile_maps retains rows/order; admits simple destination-in-selection no-scroll rows, otherwise warp-frontier; CURRENT resolves same map with preserve load mode. Content/MapTransfer/MapEventDispatcher consume definitions, pending state, marker and ordered match.|Expressible simple warps with explicit guards; Map1/0 admission, state/party/door/layout/battle bindings are missing. Arbitrary negative wildcard semantics beyond declaredFF require assessment; no source request timing equivalence claimed.|
|Areas/visuals|[ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs) and [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs) admit all three declared fixed1:1 profiles.|No Map0-style plane-profile refusal from these declarations. Missing Map1 materials/animation/text/portrait/audio bindings; actual Godot presentation unassessed. Original first-area fallback versus Domain null/field-view-area remains a changed-seed gap.|
|Battle admission|[PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) admits only1/2/3.|Explicit refusal for29/30, independent of expressible exploration warps. No next encounter body or natural runtime claim.|

Required future joins, if implemented, are source-backed Map1 setup/material
selection; both Taya entrances; full scene services and live aliases/HP;
member21 resources without assuming a stat reset; transient follower lifetime;
actual inventory capacity/repair; ordered marker/request/transfer/admission.
They are bounded gaps for later scoping, not automatic acquisition or engine work.

## Changed-state review and reproduction

The tables above preserve independent changes to21/53/841/430/530/429/529,
map/setup/view selectors, returned seed, population/alias/HP/mode, layout,
chest flags/inventory/capacity, pending bytes, interrupts and service completion.
No unsupported capability is hidden by a natural-story assumption.

Load ignored configuration in the same launching process. Direct review uses
selected fixture schemas and the unchanged retained corpus; compares every
Map1 setup/resource row,52 top-level commands/five inline blocks, all32 lore/
scene text IDs plus432/4164/446 service references, actual dispatcher and
consumer boundaries, then checks prose tables/projections, links, UTF-8/LF,
private boundary, exact two-path ownership and navigation-only predecessor:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1070/review.py <configured-source-disasm-root>
git diff --check
```

On clean committed HEAD run `uv run sf2 verify plan --base origin/main --head HEAD`;
record actual exact-head scoped CI in the Issue/PR handoff. Documentation scope
skips blanket normal/full Python/.NET/Godot/H3/H4 and tests of review programs.
Preserve #1068 original ef9942dd/author0.758s/main-gate0.761s/CI39460245865 and
its followEntity operand-flow deficiency; correction4d200508/focused main-gate
0.044s/CI39461500875 remains accepted evidence. #1066 original/projection
correction and earlier mapping diagnostics/failures/Unknowns remain with their
owners. The twelve completed old remake comparison failures remain outside
this queue; no corpus regeneration or slow-suite rerun is authorized here.

**Unknown:** naturally supplied flags/return seed/mappings/stats/items/followers,
reach of blank actors/statue/description/chests/door/warp markers, unobstructed
movements and services, alias/capacity safety, text/audio/flash/animation/DMA/
camera completion and timing, actual usable control/collection/party inclusion/
onward travel and save/load persistence. Static completion is not runtime
closure; explicit Unsupported and missing binding do not erase expressible
mechanisms, and a source suggestion is not a travel mutation.
