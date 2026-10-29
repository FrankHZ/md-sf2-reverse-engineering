# Battle35 initialization and the Geshp opening

Continue with [Battle35 activation and first-turn dispatch](./battle35-activation-and-first-turn-dispatch.md).

Evidence date: 2026-10-20. **Confirmed (static):** an admitted Map76/Battle35
fresh entry can stage Geshp's confrontation, then replace the scene population
with the current party and spriteset35. Geshp is a temporary scene actor,
not a declared Battle35 combatant. Five Mud Men are declared HIDDEN; thirteen
enemies are initialized normally before activation. TO_ROFT is also in the
random-battle upgrade list, so a completed-battle re-entry cannot be described
by the first-entry definitions alone. **Stop before ActivateEnemies.** F88
instead goes directly to the individual-turn entrance, with an out-of-alias-table
view lookup described below. Natural reach, actual incoming state, completed
services, presentation, usable control and persistence remain **Unknown**.

## Agreement and evidence route

Standard rigor for [Issue #1118](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1118)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect the complete opening, selected formation/definitions and first/repeat/
suspended initialization for subsequent content integration. Accepted base
`be9f005e8b18563ddd34b1d15023eb6bd24bc534` includes the
[Nazca/Roft admission owner](./nazca-to-roft-and-battle35-admission.md).
Own this document and a navigation-only addition there; no shared index,
registry, schema, fixture, engine or design change, stacked dependency or
competing research writer. Roft visitation is neither assumed nor excluded.
Activation geometry, spawning execution, turn order/RNG, individual actions,
combat, outcomes and Roft interactions are outside this slice.

Reuse the accepted [Battle33 shared initialization](./battle33-initialization-and-opening.md),
[battle loop](../../battle/battle-loop.md), [spriteset](../../battle/battle-spriteset-data.md),
[global data](../../battle/battle-global-data.md), [cutscene](../../battle/battle-cutscenes.md),
[cutscene data](../../battle/battle-cutscene-data.md), [map](../../world/common-maps.md) and
[scripting](../../world/common-scripting.md) owners. Only the selected35 source/use
relationships and changed-state boundaries are checked again here.

Original paths below are relative to `disasm/` in clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`sf2enums.asm`, `sf2const.asm` and the original macros own constants and
widths. `map/` and `entity/` abbreviate `code/common/scripting/`.
Fnnn means flag nnn; ordinals are zero-based; intervals are half-open.
The registered USA ROM identity remains SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source equality, decoding or runtime observation is asserted.

Minimum sufficient evidence is the complete selected stream/list/text join,
small declaration tables, their immediate consumers and actual current
lowering. Reuse the unchanged18,285,979-byte init/script pair and
`local/issue1005/pair.txt` provenance. The existing12,000,595-byte canonical
map export is read-only and checked against its registered manifest before
the selected lowering. Linear selected reads, one pair/canonical load and
two connected in-memory programs have a one minute/512MiB budget per direct
audit; peak memory is unmeasured. Small unique receipts stay in ignored
`local/issue1118/`. No extraction/regeneration, new platform, environment,
SDK, Godot, emulator, screenshot, old comparison or broad suite. Reassess a
material contradiction or expansion before increasing the work.

## Admission, fresh hooks and the retained F88 route

**Confirmed:** the predecessor ends at the ordered CheckBattle selection:
row35 `$7B2B..$7B32` is `76,16,12,48,32,255,255`. F435 admits35;
held535 clears435 but still admits it on that invocation. A later scan with
435 clear does not. This is distinct from Map76's earlier west-scroll
population, conditional zone reload and independent town38 warp. Neither
Roft greeting flags nor text order is a Battle35 prerequisite. CheckBattle
writes crop16,12 size48×32, not CURRENT_BATTLE. `code/gameflow/mainloop.asm`
MainLoop moves d7.w to d1.w and calls j_BattleLoop with map76 in d0.w.
Reload destination coordinates are not formation or scene positions.

`code/gameflow/battle/battleloop_1.asm:BattleLoop` clears PLAYER_TYPE.b,
sets399, then tests88. With88 clear it clears seconds, saves incoming d0/d1
**words**, writes their low bytes to CURRENT_MAP/CURRENT_BATTLE, installs
base VInts and executes before. After return it restores those words and
rewrites the two current bytes. It does not thereby restore crop, flags,
layout, party lists, aliases or actors. It then clears region flags90–105
inclusive, heals living/immortal allies, initializes all ally positions,
all enemy positions and AI memory, clears d0.w, loads battle for combatant0,
and executes the start wrapper. The next instruction at `@Start` is the
excluded ActivateEnemies call.

Both hook tables have48 relative-word entries. Slot35/byte70 at `$47ACE`
relative to `$47A88` selects bbcs_35 `$4D6CE`; start slot `$47B72` relative
to `$47B2C` selects ms_Empty (only FFFF). The before wrapper tests
450+35=F485 and skips when held; it does not set485. Start tests485,
**sets it before dispatch**, then executes the empty script. Even empty
ExecuteMapScript clears text skip, conditionally waits for the view when
dialogue is open, and clears scrolling speed on return.

| State at the relevant read | Consequence |
| --- | --- |
|88 clear,485 clear throughout|Before body, fresh reconstruction/load, then set485 and empty start.|
|88 clear,485 held throughout|Both hook bodies skipped; healing/placement/load still run.|
|535 held with435 admitted|Same fresh path;485 independently gates scene; enemy upgrade check can pass.|
|485 changes between wrappers|Only the body whose own read permits it runs.|
|CURRENT_BATTLE changes inside a wrapper/service seam|Each wrapper rereads it for flag/table; placement and terrain use their own live reads. Restoring saved words repairs only the two bytes at its named point.|
|Interrupted before/start|Reached prefix survives; before can leave485 clear, start can leave it held before any completed scene/load/control claim.|

### F88 does not select a valid map76 alias

With88 held, BattleLoop restores saved seconds, clears88, clears AI memory
and VIEW_TARGET_ENTITY.b, then calls LoadBattle **without clearing d0**.
It branches directly to `@IndividualTurns_Loop`; the following branch to
`@Start` is unreachable. Before/start, fresh current-byte assignment,
region reset, healing and combatant placement are bypassed. Existing
CURRENT_MAP/BATTLE, crop, combatant coordinates/activation and turn buffer
remain inputs; the incoming map word does not install them.

`battlefunctions/loadBattle.asm` saves that word across tilesets/fade and
PositionBattleEntities. `battlefunctions/battlefunctions_0.asm:GetEntityIndexForCombatant`
tests d0.b; intact76 is nonnegative, so it performs no enemy subtraction.
`andi.w #COMBATANT_MASK_ALL` uses maskFF, yielding offset76 even with a
nonzero incoming high byte. Its byte read is therefore
`ENTITY_INDEX_LIST($FFB140)+76=$FFB18C`, the **high byte of the word
TEMP_ITEM_OR_SPELL**, beyond the64-byte alias interval `$FFB140..$FFB180`.
This is not enemy172's alias, an unused alias, or Battle34's valid offset55.
ClearEntities clears only those64 bytes; PositionBattleEntities rebuilds
ally offsets0–29 and enemy32–63, not76. The selected direct prefix supplies
no initialization contract for this scratch byte; other services/caller
state cannot be assumed safe.

LoadBattle writes the fetched byte T to VIEW_TARGET_ENTITY. If T has bit7
set it uses physical0 for coordinates; otherwise it masks T with3F. Thus
T=$00 and$80 both read physical0, T=$30 reads physical48, T=$31 reads49 outside
the49-record population, and T=$7F reads63. The selected X/Y reads use
`ENTITY_DATA + 32*(T&63)` on the nonnegative branch, without a49-record
guard. Later LoadMap with d0=-1 repeats the view-byte sign/fallback/mask
logic. These are source-arithmetic cases, not measured scratch values or
valid restored state. Even an in-range slot can be cleared/unpopulated.
The retained handoff is only the direct individual-turn entrance; no turn,
activation, successful restore or usable control is claimed.

## Temporary population and scene identities

**Confirmed:** `data/battles/entries/battle35/cs_beforebattle.asm`
`$4D6CE..$4D88A` contains67 top-level records/346 encoded bytes.
Two inline movement streams add10 bytes, reaching ce_4D832 at `$4D832`.
Its88 bytes are a six-byte main header, ten eight-byte records and a
two-byte terminator. These are storage sizes, not service/frame counts.
The complete list is:

| Record / logical identity | Position / facing | Sprite / action |
| --- | --- | --- |
|mainEntity /Bowie0|43,9 Left|current ally0 /idle at declaration|
|0 /Peter7|44,10 Left|ALLY_PETER /eas_Init|
|1 /128|27,16 Up|GESHP159 /eas_Init|
|2 /129|26,21 Up|MUD_MAN89 /eas_Init|
|3 /130|29,21 Up|MUD_MAN89 /eas_Init|
|4 /131|31,24 Up|MUD_MAN89 /eas_Init|
|5 /132|28,28 Up|MUD_MAN89 /eas_Init|
|6 /133|24,24 Up|DRAGONEWT90 /eas_Init|
|7 /134|24,25 Up|PURPLE_WORM91 /eas_Init|
|8 /135|24,26 Up|GRIFFIN113 /eas_Init|
|9 /136|24,27 Up|MIST_DEMON114 /eas_Init|

csc42 disables services/display, consumes the main header, reconstructs
entities, loads mapsprites and re-enables services. InitializeMapEntities
clears49 physical records and64 alias bytes, resets walking scratch,
allocates followers first, then fixed rows and finally main physical0.
A fixed ally row is skipped when its existing alias is nonzero; regular
non-allies append to aliases128 onward. For K allocated followers and
P=1 only when fixed Peter needs allocation, Geshp128 is physical `1+K+P`,
and129–136 are the next eight physical records. Logical128 is not physical128
or a battle-combatant identity.

Unlike Map55, Map76 **is** in `data/maps/global/overworldmaps.asm`.
InitializeFollowerEntities chooses table_OverworldFollowers when65 is held:
the65/FOLLOWER_A/Caravan62 prefix falls through all19 ordinary rows.
Otherwise only the19 rows are eligible. The complete shared ordinary table
is retained by the [follower owner](./moun-underground-gyan-and-zynk.md).
Its76→Zynk26 row reads appearance independently of joined26, active58 or HP.
Repeated logical references can allocate multiple physical actors and leave
the last alias: FOLLOWER_B31 can be overwritten by74/Astral,68/Oddler,
then70/Astral; FOLLOWER_A30 by65/Caravan,74/King,77/Old Man,83/King.
These overwrites retain earlier physical actors. With intact tables K+P≤20,
so this list uses at most30 physical records, within49; the original allocator
itself has no capacity check. FOLLOWER_A is not intrinsically Caravan,
FOLLOWER_B is not intrinsically Astral, and the list declares no fixed Caravan.

Ordinal10 independently samples76: clear jumps to cs_4D728 and skips only
Zynk's awaited Init and setPos63,62 Down; held executes both. Earlier
population selection sampled76 separately. A changed flag, alias or HP can
therefore produce a different result. Zynk63,62 is a scene coordinate,
outside the48×32 battle crop, not a combat deployment or recruitment.
The scene makes no joined/active/Force membership write and does not
recruit Peter, Zynk or either pseudo follower.

SetPos/csc19 and setFacing/csc23 use
AdjustScriptPointerByCharacterAliveStatus: HP0 skips the operand payload
only for real allies0–29; pseudo30/31 and non-allies bypass it. Awaited or
nonawaited csc15 script installation and csc2D movement have no such HP guard.
Thus dead Peter can be initialized/jump/move even when his setPos skips;
dead Zynk can receive Init but skip63,62. Fresh immortal healing happens
**after** this scene. A missing pseudo alias starts at0 and can address
main physical0; changed/tombstoned aliases have their actual lookup behavior.
No logical-name or normal-party assumption repairs these cases.

## Complete opening and service boundaries

**Confirmed:** the complete top-level sequence below retains every command;
the cs_4D728 label starts at ordinal13. `Init!`/`Jump!` denote awaited named
scripts; no exclamation means installation without an idle join.

| Ordinals | Ordered operations |
| --- | --- |
|0–5|textCursor2934; loadMapFadeIn76 camera37,5; load ce_4D832; Bowie Init!; Peter Init!; Peter setPos44,10 Left.|
|6–12|FOLLOWER_B Init!, setPos44,8 Left; FOLLOWER_A Init!, setPos44,9 Left; clear76→cs_4D728, otherwise Zynk Init!, setPos63,62 Down.|
|13–19|fadeInB; cameraSpeed48;2934; Bowie Jump; Peter Jump; FOLLOWER_B Jump!;2935.|
|20–26|camera22,11;128 Right;2936;2937;2938;2939;2940.|
|27–33|sleep40;2941;128 MOSAIC_OUT; camera22,20;128 setPos28,24 Down;128 MOSAIC_IN; sleep40.|
|34–40|129 Down;130 Down;131 Left;133 Right;134 Right;135 Right;136 Right. No setFacing132 here.|
|41–49|sleep20;128 Left; sleep30;128 Up; sleep30;128 Right; sleep30;128 Down;2942.|
|50–58|Install eas_Jump on129,130,131,132,133,134,135,136 in order;128 MOSAIC_OUT.|
|59–66|sleep50; camera37,5; Peter awaited actions(moveDown1,moveLeft1,endActions);2943; FOLLOWER_B awaited actions(moveLeft1,endActions);2944;2945; csc_end.|

`data/scripting/entity/eas_actions.asm:eas_Init` clears acceleration factors,
sets speed32/32, clears acceleration/deceleration, sets independent obstruction
bit7, clears FLAGS_A bits5/6, enables auto-facing, clears orientation/immersion,
sets sprite size24, refreshes and jumps idle. Macro names do not reverse the
actual consumer polarity: bits5/6 clear **skip the corresponding entity/map
collision checks**. `HasSameDestinationAsOtherEntity`'s bit5-clear branch
returns no obstruction, so esc04's BNE is not taken; bit6-clear branches
bypass controlled/random map checks. Obstruction bit7 is a separate property.

csc15 writes the resolved physical byte to its wait timer and installs the
pointer; its waited form spins until exact eas_Idle. It does not itself
clear bits5/6. csc2D explicitly ANDs FLAGS_A with9F, emits relative esc04 plus
esc01 destination waits for each directional step, and terminates at idle.
Here Peter gets Down1 then Left1; FOLLOWER_B gets Left1. Relative movement
uses loc_55C8 destination/travel/velocity setup; UpdateEntityData integrates/
clamps it, without another bit5/6 gate. Completion still needs actual
entity-service progress and stable state/scripts/aliases. Obstacles alone
do not explain a stalled stream with those bits clear. With intact aliases,
successful prior placement and completed movements Peter ends43,11 Left
and FOLLOWER_B43,8 Left; these are
conditional coordinate endpoints, not measured completion. Bowie remains43,9
apart from the jump motion. Missing pseudo aliases can make these actors overlap.

eas_Jump disables auto-facing, moves relative0,−1, waits5, relative0,+1,
waits5, relative0,0, restores auto-facing and jumps idle. The three early
jumps are joined only through FOLLOWER_B; its idle does not independently
join Bowie/Peter. The eight later jumps are not individually awaited.
Sleep operands40,40,20,30,30,30,50 do not prove those services completed.
P2 Start plus debug can suppress sleep/text commands; it does not undo
reached scene/entity writes. cameraSpeed writes VIEW_SCROLLING_SPEED.w;
setCamDest detaches VIEW_TARGET_ENTITY, requests the named destination and
waits for view-scroll end. Entity speed32 and camera speed48 are separate.

csc22's MOSAIC_OUT6/MOSAIC_IN7 selects the native16-iteration loop:
out seed1/in seed7FFF, opposite mask shifts, current mapsprite crop/DMA
and three WaitForVInt calls per iteration. No ally-HP skip or alias removal
occurs. Geshp128 moves27,16→28,24 between out/in and receives a final out;
the script neither deletes him nor kills a battle unit. fadeInB uses the
black-fade service. Reached command order is not completed pixel/audio/text/
camera service timing, and no new native helper is hidden in this stream.

### Dialogue identity and complete token order

textCursor2934 plus twelve sequential text commands joins
`data/scripting/text/gamescript.txt` keys0B76–0B81 and the source comments.
S is nextSingleText, C is nextText. Each row gives its complete modifier,
logical speaker and ordered tokens; prose is a short authored paraphrase.

| ID / mode / modifier / speaker | Paraphrase | Ordered tokens |
| --- | --- | --- |
|2934 /S /00 /128|Geshp reacts to the party's survival.|`["{W1}"]`|
|2935 /S /C0 /FOLLOWER_B31|The follower recognizes Geshp.|`["{W1}"]`|
|2936 /S /00 /128|Geshp remarks on their persistence.|`["{N}", "{W1}"]`|
|2937 /C /C0 /Peter7|Peter refuses to stop.|`["{W2}"]`|
|2938 /S /C0 /Peter7|Peter vows to defeat the enemy.|`["{N}", "{W1}"]`|
|2939 /C /C0 /FOLLOWER_B31|The follower urges the leader to kill Geshp.|`["{LEADER}", "{W2}"]`|
|2940 /S /C0 /FOLLOWER_B31|The follower demands immediate action.|`["{W1}"]`|
|2941 /S /00 /128|Geshp announces his departure.|`["{N}", "{W1}"]`|
|2942 /S /00 /128|Geshp orders his soldiers to block the party.|`["{W1}"]`|
|2943 /S /00 /Peter7|Peter remarks on the escape.|`["{W1}"]`|
|2944 /C /00 /FOLLOWER_B31|The follower comments on Geshp's evasiveness.|`["{N}", "{W2}"]`|
|2945 /S /00 /FOLLOWER_B31|The follower urges pursuit.|`["{W1}"]`|

`data/spritedialogproperties.asm` binds Geshp159→portrait45/bleep6;
Peter and FOLLOWER_B use their actual resolved appearance. ModifierC0 carries
right-side and mirrored-portrait bits;00 carries neither. csc1D retains an
already open portrait, otherwise derives the portrait/bleep from the entity.
Single text waits for view, displays, increments cursor, closes portrait/text
and sleeps10; continued text retains its window. W1/W2/N remain distinct
controls. The sole LEADER2939 calls UpdateForce and reads the first active-list
byte, not a fixed Bowie identity. UpdateForce rebuilds joined/active/reserve
prefixes in member-ID order without clearing tails or enforcing nonempty
membership. Empty or changed lists can expose stale byte state; text skipping
also skips the token's rebuild. No choice, item/reward token or recruitment
operation is present. **Inferred:** the scene frames interception and pursuit;
it proves neither successful escape/pursuit nor a naturally reached route.

## Map, crop, terrain and fresh reconstruction

**Confirmed:** `data/maps/entries.asm:Map76` binds Map76s0_Blocks,
Map76s1_Layout and animation `$C5D06..$C5D1C`. The predecessor owns the full
regional declarations. Selected palette0/tilesets101,102,103,104,105 and
sole area0,0..63,63 have zero secondary origins, both parallax256/256,
both autoscroll0, layer254, MUSIC_MAIN_THEME1. Flag/chest tables contain
only terminators. No additional scene block copy changes the selected layout.

loadMapFadeIn sets black-fade state then csc48 loads explicit76 tilesets,
waits fade, disables services, packs camera37,5 as `(37<<8|5)*3` and calls
LoadMap. Explicit76 loads palette/blocks/layout and flag/chest work, then
copies live crop16,12 size48×32 to working origin0,0 in the64-wide layout.
That is1,536 words/3,072 copied bytes; the remainder is not cleared. A local
copied coordinate `(x,y)` corresponds to base `(x+16,y+12)`, not the same
source coordinate. Source and destination overlap, but the forward copy's
destination precedes its source. Actor and camera operands are used in the
working coordinate space; they are not automatically reduced by16,12.
LoadMapArea overrides main bounds to local0,0..47,31, internal ends18048/11904,
while the secondary plane origins remain independent.

Later LoadBattle fades out, loads tilesets for live CURRENT_MAP, waits,
clears services, reconstructs battle entities and chooses view coordinates
from its supplied combatant (fresh0; F88 uses the earlier operand). It
initializes sprites and calls LoadMap with d0=-1 for view-target selection.
The **map selector is d1.b**, preserved as explicit76 by the tileset loader
on the intact route. Thus it reloads base blocks/layout and crop, rather
than taking the negative-d1 current-map partial-reload branch. Area selection
uses actual view-target/fallback coordinates, with first-area fallback;
it is not selection by crop origin. Entity mapsprites, base VInts, terrain,
music and fade-in follow. PlayMapMusic's substitutions do not replace this
area's MAIN_THEME. Music selection does not prove audible completion.

`data/battles/terrainentries.asm` slot35→BattleTerrain35→
`data/battles/entries/battle35/terrain.bin`; LoadBattleTerrainData selects
live CURRENT_BATTLE and decompresses into BATTLE_TERRAIN_ARRAY. This is a
binding observation, not a decoded terrain/collision claim. Global row35
selects custom background OVERWORLD, enemy-leader-presence byteFF, and no
halved-EXP row (the sole row is INSIDE_ANCIENT_TOWER). No35 neutral-entity
row exists. Outcomes/background drawing/EXP execution remain excluded.

### Complete spriteset35 declaration

`data/battles/spritesets/spriteset35.asm` `$1B5D76..$1B5F14` is414 bytes:
four count bytes12/18/4/1, thirty twelve-byte combatant records, four
twelve-byte regions and one two-byte point. All12 ally rows have HEALER1,
NOTHING, `NONE,15,NONE,15,0,STARTING`; ordinals are formation positions,
not ally identities.

| Formation | X,Y | Formation | X,Y | Formation | X,Y |
| --- | --- | --- | --- | --- | --- |
|0|45,8|4|45,6|8|43,8|
|1|45,7|5|44,6|9|42,6|
|2|44,7|6|43,6|10|42,7|
|3|43,7|7|44,8|11|42,8|

All18 enemy rows declare item NOTHING and filler0. The full rows retain
both orders/triggers even when one has NONE or15. ID=128+ordinal.

| Ordinal / ID | Definition | X,Y | AI | Primary / region | Secondary / region | Spawn |
| --- | --- | --- | --- | --- | --- | --- |
|0 /128|BOW_RIDER|23,25|LEADER|MOVE_TO\|0 /0|NONE /15|STARTING|
|1 /129|CHAOS_WIZARD|22,24|ATTACKER3|FOLLOW_ENEMY\|0 /0|NONE /15|STARTING|
|2 /130|SHAMAN|23,27|HEALER5|FOLLOW_ENEMY\|0 /0|NONE /15|STARTING|
|3 /131|DRAGONEWT|24,24|ATTACKER2|FOLLOW_TARGET\|0 /15|NONE /15|STARTING|
|4 /132|PURPLE_WORM|24,27|ATTACKER3|FOLLOW_ENEMY\|3 /15|NONE /15|STARTING|
|5 /133|DRAGONEWT|23,26|ATTACKER3|FOLLOW_ENEMY\|7 /15|NONE /15|STARTING|
|6 /134|PURPLE_WORM|24,25|ATTACKER3|FOLLOW_ENEMY\|7 /15|NONE /15|STARTING|
|7 /135|GRIFFIN|23,23|ATTACKER1|FOLLOW_TARGET\|0 /15|FOLLOW_TARGET\|0 /15|STARTING|
|8 /136|DRAGONEWT|22,25|ATTACKER3|NONE /1|NONE /2|STARTING|
|9 /137|MIST_DEMON|22,23|HEALER3|FOLLOW_ENEMY\|8 /1|FOLLOW_ENEMY\|8 /2|STARTING|
|10 /138|GRIFFIN|22,26|ATTACKER1|FOLLOW_TARGET\|0 /1|FOLLOW_TARGET\|0 /2|STARTING|
|11 /139|MIST_DEMON|23,24|HEALER3|FOLLOW_ENEMY\|10 /1|FOLLOW_ENEMY\|10 /2|STARTING|
|12 /140|PURPLE_WORM|24,26|ATTACKER3|FOLLOW_ENEMY\|10 /1|FOLLOW_ENEMY\|10 /2|STARTING|
|13 /141|MUD_MAN|26,21|ATTACKER1|FOLLOW_TARGET\|0 /3|NONE /15|HIDDEN|
|14 /142|MUD_MAN|29,21|ATTACKER2|FOLLOW_TARGET\|0 /3|NONE /15|HIDDEN|
|15 /143|MUD_MAN|31,24|ATTACKER2|FOLLOW_TARGET\|0 /3|NONE /15|HIDDEN|
|16 /144|MUD_MAN|28,28|ATTACKER1|FOLLOW_TARGET\|0 /3|NONE /15|HIDDEN|
|17 /145|MUD_MAN|21,29|ATTACKER1|FOLLOW_TARGET\|0 /3|NONE /15|HIDDEN|

Every region is type4, unused byte0, four ordered vertices and trailing0,0:

| Region | Ordered vertices |
| --- | --- |
|0|20,20;20,31;31,31;31,20|
|1|0,0;0,31;21,31;21,0|
|2|21,0;22,31;47,31;47,25|
|3|19,21;19,28;29,28;29,21|

Point0 is23,25. These are declarations only; the sloping edge of region2
is retained verbatim. No polygon inclusion, activation or AI movement is
inferred. The accepted spriteset inventory registers this file/range and
headers, explicitly `rowContentTracked=false`; these complete rows are
checked directly against source, not attributed to a nonexistent row fixture.

### Selected definitions and inventories

Join `data/stats/enemies/enemydefs.asm` with `enemymapsprites.asm`.
Stats are `(level,maxHP,maxMP,baseATT,DEF,AGI,MOV)` before difficulty,
equipment and status refresh. All eight definitions use ENHANCED spell
power, initialStatus NONE, four item/spell slots; unspecified slots are NOTHING.

| Definition / index / mapsprite | Stats | Move type | Equipped item / spells | Resistance |
| --- | --- | --- | --- | --- |
|BOW_RIDER64 /128|33,61,0,44,55,44,7|CENTAUR_ARCHER|HYPER_CANNON /none|0|
|CHAOS_WIZARD74 /138|36,53,49,47,48,59,5|MAGE|GREAT_ROD /BLAZE4|0|
|SHAMAN83 /147|32,56,46,45,42,40,5|HEALER|WISH_STAFF /AURA2,BLAST3|0|
|DRAGONEWT26 /90|32,60,0,44,42,46,6|REGULAR|HEAT_AXE /none|0|
|PURPLE_WORM27 /91|32,58,0,78,46,43,5|STEALTH|none /none|0|
|GRIFFIN49 /113|32,60,0,77,45,48,7|FLYING|none /none|WIND_WEAKNESS,LIGHTNING_WEAKNESS,FIRE_WEAKNESS|
|MIST_DEMON50 /114|36,68,28,80,41,51,6|HOVERING|none /MUDDLE2|WIND_WEAKNESS,ICE_MAJOR,STATUS_MINOR|
|MUD_MAN25 /89|31,55,0,76,51,41,4|REGULAR|none /none|LIGHTNING_MINOR,ICE_MAJOR,FIRE_WEAKNESS,STATUS_IMMUNITY|

Prowess triples `(critical,double,counter)` are Bow Rider `(150_1IN8,1IN32,1IN32)`,
Chaos Wizard `(150_1IN32,1IN32,1IN16)`, Shaman `(MAGIC_DRAIN,1IN32,1IN32)`,
Dragonewt/Griffin/Mud Man `(150_1IN8,1IN16,1IN16)`, Purple Worm
`(POISON,1IN16,1IN16)`, Mist Demon `(SILENCE,1IN32,1IN16)`.
Definition AI priority is3 for Shaman,2 for the others; extras are BIT4 for
Bow Rider/Chaos Wizard/Griffin/Mist Demon, BIT5 for Dragonewt/Purple Worm/
Mud Man, none for Shaman. unknownByte equals the definition index except
Shaman0. Initialization preserves upper priority and replaces the lower
activation portion with the placement word; base-definition STARTING entries
produce2000 or3000 before activation.

`data/stats/items/itemdefs.asm`: Heat Axe40 gives ATT+32, range1,
WEAPON/RARE/BREAKABLE, BLAZE2 use; Hyper Cannon53 ATT+40, range2–3,
WEAPON/RARE, no use spell; Wish Staff94 ATT+26, range1,
WEAPON/RARE/BREAKABLE, ATTACK use; Great Rod95 ATT+28, range1,
WEAPON/RARE, no use spell. All are equipped in the copied definitions.
Placement NOTHING skips InitializeEnemyItems additions; it **does not empty
the copied inventory**. No drop/reward execution is claimed here.

### Placement, mutable lists and AI reset

HealLivingAndImmortalAllies visits all30 allies independently of membership:
living allies plus Peter7/Lemon28 even at HP0 get maxHP/MP and retain only
STUN/POISON/CURSE before stats refresh; other dead allies remain dead.
InitializeAllAlliesBattlePositions first resets all30 X to−1, rebuilds
UpdateForce lists, then consumes the active list. Living entries consume
successive formation rows; dead entries advance the member pointer/counter
without advancing the formation pointer, while still spending an iteration
of the twelve-entry outer budget. Excess list entries are removed with
LeaveBattleParty (active flag cleared, X−1); list tails/counts are not
rebuilt after those removals. Unjoined-but-active flags alone do not put
members in UpdateForce's active prefix.

For N=1..12, the first N members are examined in ID order; dead ordinary
members remain off-map and survivors pack early formation rows. At N>12,
only the first12 list entries are examined, even when dead members leave
formation gaps; later entries leave the battle party. At N=0 there is no
entry precheck: subtract1 underflows, entry0 is consumed before the counter
can terminate, so stale/out-of-list reads are possible. Do not invent a
safe empty-party result. A full ordinary party, missing Bowie, dead Peter
before scene healing, or changed flags/lists therefore have different
scene, cached-list and final placement consequences.

InitializeAllEnemiesBattlePositions visits IDs128–159. Its Jaro exception
applies only to battle32, not35. STARTING128–140 call InitializeEnemyStats:
select/possibly upgrade the definition, copy56 bytes (14 longwords), fill
HP/MP, place X/Y, combine definition movement type with placement AI set,
write enemy index/orders/triggers/items, preserve priority and apply spawn/
filler, adjust base ATT for difficulty, then UpdateCombatantStats. The
source difficulty code multiplies baseATT by5/4 (truncated) once for all
ordinary difficulty values, twice for SUPER; this is not an equipment bonus.

HIDDEN141–145 take the hidden branch with mode2→activation0200, max/current
HP0, X−1 and triggers3/15. They do **not** copy Mud Man stats/inventory,
refresh Y/enemy index, or call the upgrader here. Scene Mud Men129–132
are separate actors, not evidence these five combatants have spawned.
Absent146–159 take the same branch with mode0→activation0, HP0/X−1;
their trigger read uses the first enemy pointer (0/15), not absent records.
ClearAiMemory writes48 last-target bytesFF and48 memory bytes0, exactly
`$FFB1AC..$FFB1DC` and `$FFB1DC..$FFB20C`. It does not clear the adjacent
NEWLY_TRIGGERED_BATTLE_REGIONS word `$FFB20C..$FFB20E`; flags90–105 are separate.

PositionBattleEntities clears physical/alias state again and packs retained
combatants in ID order by signed X/Y and enemy neutral bit, **not HP**.
Coordinates mask3F, scale384 and faceDown with eas_Standing. With A placed
allies and stable first-entry declarations, thirteen enemies occupy physical
A..A+12. Geshp128's former logical reference now denotes Bow Rider128;
the scene Mud Men129–132 become Chaos Wizard/Shaman/Dragonewt/Purple Worm
references. There is no Geshp combatant. Held535 skips neutral allocation;
clear535 still finds no35 neutral row. Scene endpoints/followers/scripts
are replaced rather than retained as combatants. InitializeEnemyList is a
separate debug caller, not part of this fresh prefix; do not confuse its
definition-ID list with combatant/alias arrays.

### TO_ROFT and completed-battle upgrades

`data/battles/global/randombattles.asm` has the complete length-prefixed
eleven-battle list, in order:
TO_HAWEL_HOUSE3, NORTH_CLIFF8, TO_RIBBLE9, TO_THE_EAST10,
SOUTHEAST_DESERT14, TO_TAROS_SHRINE17, OUTSIDE_ELVEN_VILLAGE19,
DEVIL_TAIL21, TO_NORTH_PARMECIA24, OUTSIDE_KETTO26, TO_ROFT35.
It does not mean that admission performs a random draw.

`battleloop/upgradeenemies.asm:UpgradeRandomBattleEnemies` is actually called
inside each normally initialized enemy's InitializeEnemyStats. IsBattleUpgradable
checks list membership; DetermineBattleUpgrade separately reads500+battle,
here535. Clear535 returns the original definition without level/RNG upgrade.
Held535 admits the upgrade calculation for the thirteen STARTING enemies,
even when485 suppresses the opening. Hidden/absent entries have not reached
that call in this slice. Each call samples live state; there is no immutable
whole-roster upgrade snapshot.

The complete category table is:

| Definition movement category | Range parameter / inclusive definition bounds | Excluded IDs |
| --- | --- | --- |
|REGULAR,CENTAUR,STEALTH,BRASS_GUNNER|5 /OOZE..REAPER|KRAKEN_LEG,PAWN,KNIGHT,RAT,BURST_ROCK,ZEON_GUARD|
|FLYING,HOVERING|2 /HUGE_BAT..DEMON|empty length0|
|ARCHER,CENTAUR_ARCHER,STEALTH_ARCHER|2 /GALAM_ARCHER..DARK_GUNNER|KRAKEN_ARM,ROOK|
|MAGE|1 /GALAM_MAGE..CHAOS_WIZARD|QUEEN|
|HEALER|2 /DARK_CLERIC..EVIL_BISHOP|BISHOP|

The routine uses **Bowie0's** level, adding CHAR_CLASS_EXTRALEVEL20 for a
class above the last nonpromoted class, then subtracts live battle35.
A nonpositive difference retries once and returns the original; positive
differences below10 also retain it. For ordinary effective level E, the
positive multiplier is `floor((E−35)/10)` (source retains its low byte).
The range5 category uses `5*m−5` as candidate offset; the other categories
use m. It adds a bounded RNG result using range parameter+1, checks the
category bounds and excluded IDs, and has a shared one-retry fallback.
This is a definition selector before copy/appearance/inventory, not an HP
percentage boost or always-upgrade promise. No seed/draw or turn RNG is run.

For example, clear535 keeps Bow Rider64 regardless of level; held535/E44
also keeps it. Held535/E45 yields ranged candidates starting65: a permitted
candidate65/Dark Gunner changes the copied definition, mapsprite and loadout;
out-of-range/retry failure can retain64. Chaos Wizard74 is already the mage
upper bound and cannot accept a higher candidate. These arithmetic examples
do not establish actual caller level, random result or execution completion.

The airborne exclusion length0 is a real boundary: the source subtracts1
and enters the exclusion-read body without a zero-length guard, permitting
an out-of-list DBF scan with counterFFFF. It must not be modeled as an empty
set that simply accepts the candidate, nor used to guarantee a safe/fixed
Griffin/Mist Demon upgrade. No additional bytes or natural result are inferred
from source table shape; that reached behavior remains Unknown here.

## Actual lowering and present consumers

**Confirmed about current code:** production consumes prepared Content;
research/disassembly is not an on-demand engine input. Direct OriginalPrograms
with the registered canonical input and scene_maps=[76], followed by complete
registration of cs_beforebattle.asm, compiles bbcs_35 and its interior
successor cs_4D728. Counts are17 and108 instructions (125 total); the first
program's final explicit jump preserves the connected scene. All definitions,
fallthrough/branch continuation and named eas_Init/eas_Jump are registered;
both inline action arrays are inspected. No nested native-call is selected.
The sole top-level native is cs_4D728 instruction1 cameraSpeed. The first
program's17 instructions are not the whole opening.

The resulting scene-entities instruction has Peter plus logical128–136,
the exact list positions/facing and actions. **A selected semantic mismatch
is observed:** OriginalPrograms.population always emits the19 ordinary
followers, omitting the65/Caravan prefix and the native overworld choice.
scene_maps=[76] does not change that method. With65 held and later
FOLLOWER_A-producing flags clear, native76 provides Caravan30 while the
lowered population leaves30 at its initial alias0; its unconditional Init/
setPos can then address a different actor. Later overwrites can hide the
logical alias difference while physical population/order still differs.
Complete source registration does not repair this omission, and no data
or cache is altered to conceal it.

| Actual owner | Expressible shape / refusal, difference or missing binding |
| --- | --- |
|[OriginalPrograms](../../../../src/sf2tool/remake_exploration_programs.py), [ExplorationContentReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Branches, text, scene entities, motion and effects lower. cameraSpeed becomes native-call→UnsupportedInstruction→reached `program-opcode` refusal. Earlier resource/service/alias failures may occur first. No execution through that point is claimed.|
|[SceneEntities](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapEntityAllocator](../../../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [EntityActionRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Follower/fixed population, aliases, Init/moves/jumps and sprite waits have consumers with49-slot guards. The selected missing65 prefix is upstream in preparation. Position/facing shapes omit the original ally-HP skip; arbitrary changed-alias/service fidelity is not established.|
|[ExplorationTextRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs), [MapPartyMembership](../../../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Token/portrait/wait and membership-list shapes exist. Selected twelve material/text bindings and stale/empty-list behavior are not proved by compilation. FOLLOWER_B speech must use actual appearance;76 is not a membership grant.|
|[PrivateBattleEncounterReader.Pin](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Placement/region/point/terrain and before→init→load→start shapes exist, but Pin admits1/2/3 and rejects35. F88 NewBattle refuses `suspended-battle-entry`; it does not reproduce scratch-offset76. Selected35 definitions, terrain/materials, party/flags435/485/535 and world/route/load/start bindings are absent.|
|[BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs)|Controlled roster-only/already-refreshed input, difficulty0, admitted status/control and formation capacity are required. Equipped enemy loadouts refuse `enemy-equipment-refresh`; placement NOTHING cannot remove the four weapons to evade it. General completed-battle definition upgrades/airborne underflow and all30 nonroster allies are not reproduced. It computes before publication, unlike arbitrary interrupted native prefixes.|
|[Map preparation](../../../../src/sf2tool/remake_exploration_maps.py), [ExplorationViewRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../../../remake/game/src/Battles/BattleMapViewport.cs)|Map76 layer254, zero origins/unit parallax/zero autoscroll satisfies existing profile predicates. Board area selection uses crop origin; original LoadBattle uses actual view target/fallback. Nonzero admitted view speed separately refuses `field-view-speed`. Prior selected map76 conversion is not a Battle35 route or decoded terrain/resource binding.|
|[ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs)|Mosaic/sprite/text/audio/black-fade cue consumers exist. Their animations do not prove original16-mask/service timing. Complete selected resources, actual presentation and usable input remain Unknown.|

Existing Battle01–03 support does not establish Battle35 playability. The
bounded integration needs are selected content/definition/material/route
binding, an admitted camera-speed operation, the overworld-follower selection,
and explicit decisions on party/equipment/upgrade/suspended-entry boundaries.
This research authorizes no implementation repair or new platform.
ProgramRunner's BattleStartFinished invokes BattleAdvancer after start;
that current consumer boundary is not exercised and is beyond this slice's
original stop before ActivateEnemies.

## Reproduction, preserved failures and Unknowns

Direct checks join the complete67-command corpus, both inline streams,
88-byte list, twelve text IDs/modes/speakers/tokens, hooks485, live selectors,
all30 formation/enemy rows, four regions/one point, eight base definitions/
four items, global/terrain/map facts and actual connected lowering/consumers.
Use the pinned source plus unchanged pair/canonical selections above;
receipts are review coordination, not a maintained extractor or evidence owner.
The ignored direct recipe is `local/issue1118/review.py`; after loading
current private selections in the same PowerShell process, invoke:

```powershell
uv run --locked python -X utf8 local/issue1118/review.py <disasm-root> --canonical <registered-canonical-json> --receipt local/issue1118/direct-reproduction-01.json --committed
uv run sf2 verify plan --base origin/main --head HEAD
```

Choose an unused receipt path. The first argument contains sf2enums.asm,
code/ and data/. Inspect exact two-path/navigation-only/private/UTF-8/LF/link
checks, clean committed planner and actual exact-head scoped CI. ADR0004's
applicable adversarial checklist is used; parser/schema/fixture/H3 obligations
are N/A because none are added or changed. Normal/full Python, SDK/Godot/H3/H4
and old comparisons are excluded by this documentation-only scope; suggested
planner commands are not executed passes.

Preserve the predecessor's original e26968a9 author0.500s and CI39529528549
PASS that missed the collision polarity error; c3f0c96a corrected AND9F
semantics and the Dojo wording with focused checks/CI39530664106. The original
whole direct recipe was not rerun. [Dojo](./dojo-exploration-and-sheela-recruitment.md)
retains Lemon28 and follow offset2=(−24,0) corrections, eas_5FFC4/5FFC8 and
compile_maps45 refusals. [Post-crash](./post-crash-ship-and-nazca-overworld-return.md)
retains complete-import @Return, native crew/RNG/mapLoad and ReloadScene
working-layout limits. Their other named predecessor failures remain with
their owners. Twelve completed old-comparison failures remain completed
discovery evidence, not interrupted work or a rerun queue. Selected35 lowering
does not close any of those distinct boundaries.

**Unknown:** natural zone/edge/admission/town order and actual caller words;
flags/HP/classes/status/items/party/followers/aliases and F88 scratch/restored
turn state; reached airborne out-of-list behavior; decoded layout/terrain/
collision and selected resource bindings; request interference, interruption,
motion/view/text/fade/sprite/audio completion and service scheduling;
VInt/DMA/rendering/presentation timing, usable control and save/load persistence.
No Unknown is an automatic acquisition queue. Activation, hidden spawning,
turn generation/actions, outcomes and Roft interaction remain excluded.
