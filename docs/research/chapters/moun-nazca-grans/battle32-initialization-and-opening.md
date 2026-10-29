# Battle32 initialization and opening

Continue with [Battle32 activation, Jaro side change and first-turn dispatch](./battle32-activation-jaro-and-first-turn-dispatch.md).

Evidence date: 2026-10-20. **Confirmed (static):** admitted Map74/Battle32
constructs a temporary scene, then a current-party formation and sixteen
declared enemies. Enemy143 has a joined-list exception that can retain its
old slot. Scene aliases, membership, combatants and physical entities are
separate authorities. Fresh entry stops before ActivateEnemies.
**Unknown:** natural admission, supplied state, completed presentation,
usable control and persistence. No runnable Battle32 claim follows.

## Agreement and provenance

Standard rigor for [Issue #1082](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1082)
under [epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
record the complete opening, declared encounter, fresh initialization and
actual consumer boundaries. Accepted base is
`194032a09e410c010768afffa2b08f8e6313adbb`; the
[predecessor](../northern-travel-mitula/battle31-outcomes-and-battle32-admission.md) owns conditional
admission. Owned writes are this document and its predecessor's navigation
link. No shared-path or stacked dependency; UI692 is independent. Exclude
activation, turn/AI/combat/outcomes, Jaro's later region scene, Moun story,
runtime, extraction, new tool/schema/fixture, remake and translation changes.

Reuse [Battle31 opening](../northern-travel-mitula/battle31-initialization-and-opening.md),
[Battle27 Map74 load/resources](../northern-travel-mitula/battle27-initialization-and-opening.md#load-physical-population-crop-and-view),
[spritesets](../../battle/battle-spriteset-data.md), [global data](../../battle/battle-global-data.md),
[battle loop](../../battle/battle-loop.md), [cutscenes](../../battle/battle-cutscenes.md),
[maps](../../world/common-maps.md) and [scripting](../../world/common-scripting.md).

Original paths below are relative to `disasm/` in clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing provenance identifies USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no fresh ROM/H1 equality is asserted. `sf2const.asm`, `sf2enums.asm`,
`sf2macros.asm`, `sf2mapmacros.asm` and `sf2cutscenemacros.asm` own constants
and widths. Fnnn means flag nnn; ordinals are zero-based, extents half-open.
`battleloop/`, `battlefunctions/` and `cutscenes/` abbreviate children of
`code/gameflow/battle/`; `map/` means `code/common/scripting/map/`.

Minimum evidence is selected source/table/fixture/schema joins and actual
bounded lowering/consumers. Reuse the unchanged18,285,979-byte map-init/script
pair and accepted0.083s provenance at `local/issue1005/pair.txt`. Linear
reads/checks budget less than one minute/512MiB with small ignored receipts
in `local/issue1082/`; no asset decoding or acquisition. Reassess conflicting
evidence or material supporting machinery. ADR0004's width/branch/order/
alias/provenance review applies proportionally; no tests of audit programs.

## Entry, intro latch and fresh versus supplied state

**Confirmed:** the predecessor's admitted row32 is
`74,2,16,32,48,255,255` in `data/battles/global/battlemapcoords.asm`,
at `$7B16`. Earlier held427 selects27 instead. CheckBattle writes crop
`(2,16,32,48)`; held532 clears432 but still admits32 on that invocation.
It does not assign CURRENT_BATTLE. MainLoop replaces d1.w with32 and calls
j_BattleLoop with resolved map74 in d0.w. Ordinary preceding reload supplies
d2.w=sampled Y with low byte61, d3.w=retained facing, d4.w=0; sampled X in
d1 is discarded. These actual words/high bytes are not a scene/formation seed.

`code/gameflow/battle/battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`):

1. Clear PLAYER_TYPE.b; set399; check88.
2. Fresh/clear88: clear seconds, save incoming d0/d1 words, assign their low
   bytes to CURRENT_MAP/CURRENT_BATTLE, set base VInts and execute before.
3. Restore the words and reassign current bytes; clear all region flags90–105,
   independently of the two local regions. HealLivingAndImmortalAllies,
   InitializeAllAlliesBattlePositions, InitializeAllEnemiesBattlePositions,
   ClearAiMemory, in that order.
4. Clear d0.w; LoadBattle for combatant0; ExecuteBattleStartCutscene;
   reach @Start. Stop **before ActivateEnemies**.

Both hook tables have48 relative words. Selected32's byte64 word is
`$47AC8` relative to `$47A88` in beforebattlecutscenes.asm, targeting
bbcs_32 `$4CBE2`; start word `$47B6C` relative to `$47B2C` targets ms_Empty.
The before wrapper checks450+CURRENT_BATTLE=482 and skips if held, without
setting it. Start checks the same flag, **sets it before dispatch**, then
executes the sole FFFF word in `map/ms_empty.asm`. Empty dispatch still clears
text-skip, takes the common end wait when a dialogue window exists, clears
VIEW_SCROLLING_SPEED and restores registers. Held482 skips both bodies but
does not skip fresh healing/placement/load. F482 proves no completed service.

Hooks reread live CURRENT_BATTLE between flag and table selection. Fresh
word restoration repairs the current bytes after before, without rolling
back scene/list/crop mutations. Subsequent roster/terrain/neutral/resource
consumers use their actual selectors; no frozen encounter snapshot exists.
Interrupted before may leave482 clear; interrupted start may leave it held.

Held88 instead restores SAVED_SECONDS_COUNTER, clears88, clears AI memory
and view target, then LoadBattle **without clearing d0**; it jumps directly
to @IndividualTurns_Loop. The following branch to @Start is unreachable.
There is no fresh current-map/battle assignment, scene, heal, region reset,
roster/Jaro initialization or start dispatch. Intact admitted d0.w=74 is
therefore **combatant74**, not fresh0. GetEntityIndexForCombatant treats74
as nonnegative, reads lookup74 without bounding it to0–63, and LoadBattle
uses the returned byte for view selection (negative falls back to physical0,
otherwise masked3F). Its resource selectors/crop and combatants remain supplied.
Stop at the individual-turn handoff; no turn is executed here.

## Temporary cast, follower aliases and complete scene

**Confirmed:** `data/battles/entries/battle32/cs_beforebattle.asm` spans
`$4CBE2..$4CD10`. bbcs_32 ends at ce_4CCC0:38 top-level records/202 encoded
bytes, plus five four-byte inline movement blocks, totaling222 script bytes.
The separate80-byte list is a six-byte main header, nine eight-byte fixed
records and two-byte terminator. It is not spriteset32.

| Record / logical ID | X,Y / facing | Sprite / initial action |
| --- | --- | --- |
|Main /0|15,44 /Up|Actual Bowie ally sprite /eas_Idle|
|Fixed0 /7|16,45 /Up|ALLY_PETER /eas_Init|
|Fixed1 /22|15,46 /Up|ALLY_FRAYJA /eas_Init|
|Fixed2 /128|14,21 /Down|DEATH_ARCHER122 /eas_Init|
|Fixed3 /129|12,21 /Down|NECROMANCER137 /eas_Init|
|Fixed4 /130|13,20 /Down|DEATH_MONK141 /eas_Init|
|Fixed5 /131|16,22 /Down|PEGASUS_KNIGHT112 /eas_Init|
|Fixed6 /132|14,24 /Down|DEVIL_SOLDIER87 /eas_Init|
|Fixed7 /133|16,24 /Down|CERBERUS88 /eas_Init|
|Fixed8 /134|18,23 /Down|NECROMANCER137 /eas_Init|

csc42 reads the main header words, clears/rebuilds entities/lookups,
loads mapsprites and restores display/VInts. `map/mapfunctions.asm` calls
InitializeFollowerEntities before fixed records and physical main0. Map74
is an overworld and is not follower-excluded: held65 starts at the Caravan
prefix (logical30), then falls through the ordinary follower table; clear65
starts at the ordinary table. Reuse the full ordinary list in
[the population owner](../northern-travel-mitula/battle26-return-through-pacalon-entry.md#conditional-map02-population-and-first-eventcontrol-read).
This is source table selection, not a party-size or membership test.

Ordinary F74/F77/F83 can supply30 (King/Old Man/King); F74/F68/F70 can
supply31 (Astral/Oddler/Astral). Later enabled records overwrite each alias
while retaining earlier physical allocations. F67 can preallocate Peter7;
F82 can preallocate Frayja22. A fixed ally with nonzero existing lookup
skips allocation but consumes its action longword. No joined/active check
is made. With F successful follower allocations and P/R indicating fixed
Peter/Frayja allocation, scene128–134 use physical `F+P+R+1` through
`F+P+R+7`, lookup32–38. With only65 held: Caravan1, Peter2, Frayja3,
NPCs4–10;31 remains zero alias/main0. Clear65 with no followers gives
Peter1, Frayja2, NPCs3–9; both30/31 alias main0. These are changed-state
examples, not the actual incoming population.

Fresh PLAYER_TYPE=0 does not clear follower flags. Ally sprites resolve
actual class; live battle32 bypasses outside-battle dead-ally blue flame.
eas_Init establishes speed32/32, acceleration/flags/size/sprite then Idle;
it does not heal or join. A separate F64/RAFT_MAP match may declare physical
raft31/lookup63 after main; that is not FOLLOWER_B31/lookup31. No general
allocation-capacity guard exists in original DeclareNewEntity.

| Top-level indices | Complete ordered work |
| --- | --- |
|0–5|Cursor2845; loadMapFadeIn74, camera10,38; load ce_4CCC0; Init/W Bowie; Init/W Peter; Peter16,45 Up.|
|6–13|Init/W31;31 at14,45 Up; Init/W Frayja; Frayja15,46 Up; Init/W30;30 at15,45 Up; fadeInB; cameraSpeed `$30`.|
|14–18|Install Up3 on Bowie, Peter,31,30 without wait, then Up3 on Frayja/W. Each inline block is moveUp3/endActions.|
|19–22|2845 single text from128; camera10,18 and wait;2846 continued;2847 single.|
|23–30|Face128 Left, Sleep30, Right, Sleep30, Up, Sleep30, Down;2848 single.|
|31–37|Install eas_Jump on129–133 without wait, then134/W; csc_end.|

W requests Idle-pointer completion, not measured motion completion. The
five movement scripts coexist when actors are distinct; waiting for22 is
not an explicit wait for0/7/30/31. Shared aliases can overwrite the same
action pointer. With distinct unchanged actors and all required movements
complete, endpoints are Bowie15,41, Peter16,42,31 at14,42,30 at15,42,
Frayja15,43, all Up. These are scene working-map coordinates. Fresh
CURRENT_BATTLE is already32 when csc37/csc48 loads74: LoadMapBlocksAndLayout
therefore copies the live battle crop to working origin0,0 even for this
before scene. They are not uncropped global Map74 coordinates. Under the
unchanged crop, source-layout counterparts add2,16: Bowie's conditional
endpoint is17,57 and Frayja's17,59. Actor setters themselves add no offset.
NPCs retain declared coordinates,128 returns Down; eas_Jump disables
auto-facing, requests relative Up1/wait5/Down1/wait5/0,0 then reenables it
and reaches Idle. Only134 has the final explicit wait. Interruption, missing
actors, collisions and service scheduling prevent an unconditional endpoint.

### Text, selectors, camera and action consumers

All four comments match keyed gamescript records `0B1D..0B20`. Speaker is
logical128, modifier00 for every record; cursor advances only on actual
unskipped display. Meaning is paraphrased here; no translation is added.

| Text / macro / speaker | Meaning / terminal control |
| --- | --- |
|2845 /S /128|Has heard of the party from Geshp /W1|
|2846 /T /128|Geshp ordered casualties among them /W2|
|2847 /S /128|Threatens to end their resistance /W1|
|2848 /S /128|Orders everyone killed /W1|

csc00/csc02 read a packed word and mask speaker to its low byte. The high
modifier byte supplies portrait flags, not a different actor. Live sprite122
has no row in `data/spritedialogproperties.asm`; the actual
GetEntityPortaitAndSpeechSfx fallback is PORTRAIT_DEFAULT/SFX_DIALOG_BLEEP_6.
Already-open portrait bypasses replacement; speech resolves again. S closes
portrait/text then Sleep10; T retains windows. ExecuteMapScript initially
clears text-skip; later debug/P2 Start can latch it. Skipped text consumes
operands without cursor increment/display/single cleanup. Successful
unskipped display ends cursor2849. No NAME/LEADER token or direct join/flag/
item mutation occurs in these four records or bbcs_32.

In `map/mapscriptengine_1.asm`, csc19 setPos and csc23 setFacing use a
selector byte and AdjustScriptPointerByCharacterAliveStatus: dead ordinary
allies skip four/two operand bytes.30/31 and enemy-bit IDs bypass that HP
test. Thus22/7 setPos can skip while their records/actions still exist;
later healing is not retroactive. csc15 action install reads selector/wait
bytes then pointer longword, installs timer=physical-slot byte and optionally
busy-waits for eas_Idle. csc2D reads selector/wait bytes, clears collision
bits with9F, expands movement bytes into scratch actions and an Idle jump,
then optionally waits. Neither action installer has that alive guard.
GetEntityAddressFromCharacter uses byte selector, enemy-minus96 lookup and
physical-byte shift5; zero aliases main, FF can address ENTITY_DATA+$1FE0.

cameraSpeed emits/consumes a word48, stored in VIEW_SCROLLING_SPEED. Camera
services apply the override before parallax scaling; it is not a duration.
setCamDest detaches following, requests10,18 and waits for view scroll.
Sleep30 emits `$8000|30`; interpreter masks to a byte and skips this sleep
under text-skip, independently of action waits. The common terminator may
wait for the dialogue-related view, clears speed and restores registers;
it does not cancel every installed action or roll back state. No measured
timing, completed fade/text/jump/camera or rendered result is established.

## Formation, all enemy declarations and definition inventory

**Confirmed:** pointer32 selects BattleSpriteset32,
`data/battles/spritesets/spriteset32.asm`, `$1B58FC..$1B5A6A`:366 bytes =
4-byte header +28 twelve-byte combatant rows +2 twelve-byte regions +one
two-byte point. Header12/16/2/1. The aggregate fixture's
`rowContentTracked=false` is not evidence of exact row contents.
Ally rows all declare HEALER1, NOTHING, NONE/15, NONE/15, filler0, STARTING.
They describe positions, not a fixed twelve-person party.

| Slot | X,Y | Slot | X,Y |
| --- | --- | --- | --- |
|0|15,44|6|16,46|
|1|14,45|7|17,46|
|2|15,45|8|17,45|
|3|16,45|9|13,45|
|4|14,46|10|14,44|
|5|15,46|11|16,44|

HealLivingAndImmortalAllies scans30 slots, healing living allies and
Peter7/Lemon28 even at HP0, retaining STUN|POISON|CURSE and updating stats.
Dead Frayja22 remains dead. Ally placement sets every X=-1, calls UpdateForce,
then consumes sorted joined-and-active IDs for at most12 member iterations.
A dead member consumes an iteration but not a formation row; surplus active
members LeaveBattleParty. Unplaced Y/other fields remain. Empty/inconsistent
lists have no general safe-admission guarantee. Neither F82 nor scene22
writes joined22/active54. This scene does not recruit Frayja or Jaro.

All enemies declare item NOTHING, filler0 and STARTING0. The primary/
secondary order and region are independent bytes; local enemy references
are ordinals, not scene actor identities.

| Ordinal / ID | Type | X,Y | Commandset | Primary order / region | Secondary order / region |
| --- | --- | --- | --- | --- | --- |
|0 /128|BOW_MASTER63|14,21|LEADER|MOVE_TO\|0 /15|NONE /15|
|1 /129|NECROMANCER73|12,21|ATTACKER3|FOLLOW_ENEMY\|0 /15|NONE /15|
|2 /130|MASTER_MONK82|13,20|HEALER3|FOLLOW_ENEMY\|0 /15|NONE /15|
|3 /131|PEGASUS_KNIGHT48|16,22|SENTRY|NONE /15|NONE /15|
|4 /132|DEVIL_SOLDIER23|17,33|ATTACKER1|NONE /15|NONE /15|
|5 /133|CERBERUS24|19,33|ATTACKER3|FOLLOW_ENEMY\|4 /15|NONE /15|
|6 /134|PEGASUS_KNIGHT48|20,34|ATTACKER1|NONE /15|NONE /15|
|7 /135|DEVIL_SOLDIER23|9,39|ATTACKER2|NONE /15|NONE /15|
|8 /136|CERBERUS24|10,39|ATTACKER3|FOLLOW_ENEMY\|7 /15|NONE /15|
|9 /137|MASTER_MONK82|6,33|HEALER3|FOLLOW_ENEMY\|9 /0|FOLLOW_ENEMY\|9 /1|
|10 /138|PEGASUS_KNIGHT48|23,26|ATTACKER1|FOLLOW_TARGET\|0 /0|NONE /15|
|11 /139|PEGASUS_KNIGHT48|24,26|ATTACKER2|FOLLOW_TARGET\|0 /0|NONE /15|
|12 /140|DEVIL_SOLDIER23|14,24|ATTACKER1|FOLLOW_TARGET\|0 /0|NONE /15|
|13 /141|CERBERUS24|16,24|ATTACKER3|FOLLOW_ENEMY\|12 /0|NONE /0|
|14 /142|NECROMANCER73|18,23|ATTACKER3|FOLLOW_ENEMY\|12 /0|NONE /0|
|15 /143|JAR99|8,31|ATTACKER1|NONE /0|NONE /1|

InitializeAllEnemiesBattlePositions scans128–159. Declared128–142 and
unskipped143 have spawn nibble0<HIDDEN2 and initialize without an occupancy
test. Enemy definitions are copied in full56 bytes; current HP/MP become
max, X/Y are set, movetype upper nibble/commandset lower nibble are packed,
enemy index/trigger nibbles/orders stored, items processed, activation packed,
difficulty applied and stats recomputed. **NOTHING placement is not empty
inventory:** InitializeEnemyItems returns before AddItem without clearing
the copied definition items. Scene128's Death Archer122 becomes battle
Bow Master127; scene130's Death Monk141 becomes Master Monk146. Scene132–134
also differ from corresponding battle positions/types. Load discards the
temporary population rather than transferring its final positions.

Trigger bytes: `FF FF FF FF FF FF FF FF FF 01 0F 0F 0F 00 00 01`.
Move words: `40FF 80FF 80FF FFFF FFFF 84FF FFFF FFFF 87FF 8989 00FF 00FF 00FF 8CFF 8CFF FFFF`.
NONE=FF, MOVE_TO=40, FOLLOW_ENEMY=80, FOLLOW_TARGET=00. Region15 is not a
third polygon; FOLLOW_TARGET0 is not the MOVE_TO point0. Definition upper
activation nibble is3000 for Master Monks130/137 and2000 for the other
initialized rows. Rotated STARTING0 contributes0 to low0FFF, dropping
definition BIT4/BIT5; active/secondary/neutral/spawn bits are clear.
ClearAiMemory separately writes48 last-target bytesFF and48 memory bytes0.

### Jaro: joined list, empty list and skipped old slot

**Confirmed:** `battleloop/initializecombatants.asm:InitializeEnemyBattlePosition`
checks live battle32 and combatant143 **before** subsection selection or
any clearing/copying. It calls HasJaroJoinedTheForce (`$1B1A28..$1B1A66`),
tests d1.w and skips to @Done when nonzero. The function comment alone
does not establish which list this means:

| Actual reached state | Consumer result / retained boundary |
| --- | --- |
|Nonempty current joined list includes ally Jaro23|j_UpdateForce rebuilds TARGETS_LIST and its word length from joined0–29 flags. Scan finds23; d1=1; enemy143's entire initialization is skipped. Active55 may be clear.|
|Nonempty joined list excludes23, even with active55 held|Scan exhausts the counted prefix, returns0;143 receives definition JAR99 and declared position/orders. Active without joined does not enter the list.|
|Incoming cached lists stale, ordinary nonempty flags|The helper's own UpdateForce overwrites counted prefixes/counts before scanning. It does not trust the supplied count or active list. Uncounted tails remain.|
|No joined members|UpdateForce writes length0 without clearing the list. Subtract1 yields FFFF; no pre-loop empty check exists. The first stale byte can equal23 and return1; otherwise DBF can scan65,536 byte reads, signed d6.w indexing beyond the counted list and wrapping. This does not reliably mean Jaro absent.|
|Skip143 taken|No definition, HP/MP, X/Y, item, enemy index, orders, triggers, activation or difficulty/stat refresh occurs for143. It retains all old slot fields, rather than being cleared or converted to ally23.|
|Absent144–159|The ordinary absent arm writes activation0, max/current HP0, X=-1 and triggerFF from the still-unoffset first enemy row. Y/type/items/orders/other fields remain; these slots are not the skipped143 case.|

`code/common/stats/battleparty.asm:UpdateForce` writes joined prefix to
TARGETS_LIST, active joined prefix to BATTLE_PARTY_MEMBERS, reserve joined
prefix to RESERVE_MEMBERS and three word counts in member-ID order. It does
not clear tails or write membership flags. The Jaro helper runs after ally
placement's earlier rebuild/possible active removals, yet scans all joined
members. No JoinForce occurs here; no new recruitment follows from its name.
A skipped143 with retained nonnegative X/Y and nonneutral activation can
still become a physical enemy even at HP0; retained type can select another
sprite. A retained negative coordinate or neutral bit can suppress it.
Interruption or changed flags/selectors preserves only the reached prefix.
The later region-triggered Jaro program is deliberately outside this slice.

### Definitions, equipment and region declarations

**Confirmed:** `data/stats/enemies/enemydefs.asm` supplies seven definitions,
all ENHANCED, initialStatus NONE, four items/four spells. Listed item is
followed by three NOTHING; spells are padded with NOTHING. These are raw
base values before equipment/difficulty, conditional on initialization.

| Type | Level | Max HP/MP | Base ATT/DEF/AGI/MOV | Movetype | Item | Spells |
| --- | --- | --- | --- | --- | --- | --- |
|Bow Master63|30|46/0|50/53/40/5|ARCHER|ASSAULT_SHELL\|EQUIPPED|none|
|Necromancer73|30|47/42|45/44/51/6|MAGE|FLAIL\|EQUIPPED|FREEZE\|LV3|
|Master Monk82|28|47/39|22/45/31/6|HEALER|BRASS_KNUCKLES\|EQUIPPED|HEAL\|LV4, DISPEL|
|Pegasus Knight48|30|65/0|40/37/47/7|FLYING|CHROME_LANCE\|EQUIPPED|none|
|Devil Soldier23|29|60/16|45/48/43/6|REGULAR|GREAT_AXE\|EQUIPPED|BOOST\|LV2|
|Cerberus24|30|51/0|70/44/45/6|STEALTH|NOTHING|FLAME|
|JAR99|12|59/0|35/44/46/7|FLYING|CHROME_LANCE\|EQUIPPED|none|

Resistance: Bow/Monk0; Necromancer LIGHTNING_MINOR|STATUS_IMMUNITY;
Pegasus/Soldier STATUS_MINOR; Cerberus ICE_WEAKNESS|FIRE_MAJOR; JAR
WIND_MAJOR|LIGHTNING_MAJOR|ICE_MAJOR|FIRE_MAJOR|STATUS_IMMUNITY.
Prowess: Bow/Necromancer/Cerberus/JAR CRITICAL150_1IN32;
Pegasus/Soldier CRITICAL150_1IN8; Monk CRITICAL125_1IN4. Double1IN32
except Cerberus1IN16/Monk1IN8; counter1IN16 except Bow/JAR1IN32.
`enemymapsprites.asm` maps definitions63/73/82/48/23/24/99 to
127/137/146/112/87/88/52 (JARO_SPECIAL). These are not scene aliases.

Item definitions49/90/28/79/39 are Assault Shell/Flail/Brass Knuckles/
Chrome Lance/Great Axe: weapons/useSpell NOTHING, INCREASE_ATT25/19/39/31/28
then two NONE effects; Shell range2–3, others1. Stats apply equipment
without this consumer checking class equippability. The repeated-BEQ
difficulty routine floors baseATT*5/4 at every difficulty, twice on SUPER,
then recomputes stats. Battle32 is absent from all eleven random-upgrade
entries, so unchanged32 preserves selected definition IDs. This is neither
final damage nor AI/spell execution.

Each region is12 bytes: type4, unknown0, four ordered vertices, two trailing
zero bytes. Point0 is two bytes `(14,21)` for MOVE_TO0.

| Region | Ordered vertices |
| --- | --- |
|0|(0,0), (0,34), (31,34), (31,0)|
|1|(1,0), (1,47), (31,17), (31,0)|

No region membership, activation or later narrative is evaluated here.

## LoadBattle, physical actors and selected Map74 resources

**Confirmed:** `battlefunctions/loadBattle.asm` saves view combatant d0,
reads current map byte into cleared d1, fades out/detaches view, loads
tilesets, waits for fade, clears VInts, waits one VInt and calls
PositionBattleEntities. The latter clears49 physical records/lookups and
walking scratch, then scans30 allies and32 enemies. Signed nonnegative
X/Y admit allies; enemies additionally require neutral bit clear. There is
no HP/membership/active test here. Coordinates are masked3F then multiplied
by384, facing Down, action eas_Standing, with flying/hovering layer handling.
Unplaced lookups becomeFF. Selected normal initialized rows have regular
sprites; no special-sprite63 path is selected by those definitions.

With A placed allies and unskipped normal143,128–143 map consecutively to
physical A..A+15. Skipped143's retained fields can change this result;
no fixed actor count is asserted for arbitrary supplied state. Completion532
skips neutral construction, but even clear532 finds no TO_MOUN entry in the
complete `battleneutralentities.asm` table. Frayja22 is therefore not added
as a neutral merely because she appeared in the scene. The original clear/
scan sizes are not an allocation-capacity guard.

LoadBattle restores fresh d0=0, uses Bowie's lookup as VIEW_TARGET_ENTITY,
and samples that physical actor's tile coordinates. Negative lookup falls
back to physical0 for reads without creating Bowie. It calls InitializeSprites,
then LoadMap with **d0=-1, d1=74**: target-based view, full map load rather
than negative-d1 CURRENT reload. F88 uses its supplied map/current battle
and the separate combatant74 operand described above.

Map74's accepted resource owner remains authoritative: palette0, tilesets
101/102/109/110/105; Map74s0_Blocks/Map74s1_Layout; sole area descriptor
`$C365C..$C367A` then FFFF through `$C367C`. Main start0,0/end63,63,
foreground/background starts0,0, parallax256/256 on both, autoscroll0,
layer254, MUSIC_MAIN_THEME1. Flag-copy table is empty. The actual animation
pointer is Map74s9_Animations: `9-animations.asm` declares tileset111,
header word96, then two `(source,count,destination,counter)` word records
`(0,48,$350,20)` and `(48,48,$350,20)`, followed by FFFF. This rechecks
selection without decoding assets/layout or asserting animation timing.

LoadMap selects area using view-target internal coordinates against
bounds*384, falling back to the first descriptor; no crop offset is added.
Base blocks/layout, flag copies/opened chests precede crop copying. Selected32
uses the same `(2,16,32,48)` rectangle as27:1536 words/3072 bytes with64-wide
working rows, origin0,0. Battle layer1 end is11904,18048 (31/47 tiles*384).
Live crop bytes are not repaired. Battle-relative `(x,y)` corresponds to
source layout `(x+2,y+16)`: formation0→17,60, enemy0/point0→16,37,
JAR row→10,47. Scene working coordinates and these battle-relative positions
share the unchanged cropped working map, while source-layout coordinates
add the crop origin. Scene actors are discarded and rebuilt from combatants;
their movement endpoints are not carried into formation. No path, terrain
legality or geometry decoding follows from these relations.

The suffix waits VInt, loads mapsprites, sets base VInts, loads terrain,
requests music and fade-in. Terrain pointer32 in the45-entry table is
**BattleTerrain27**, incbin `data/battles/entries/battle27/terrain.bin`,
decompressed to BATTLE_TERRAIN_ARRAY. There is no separate BattleTerrain32
label in that table: selected32 shares terrain as well as map/crop with27.
MAIN_THEME1 survives the battle music substitutions and submits1. Battle44
timer does not apply. Custom-background slot32 is
OVERWORLD; the30-row enemy-background switch has no32. These selections
prove neither audible/rendered completion nor a public asset binding.

## Actual lowering and current consumer boundaries

**Confirmed about accepted code:**
[OriginalPrograms](../../../../src/sf2tool/remake_exploration_programs.py) lowers
bbcs_32 to59 instructions, `entitiesRunning=true`: scene population at5,
five Init/W motions6/7/9/11/13, fade15, native cameraSpeed16, five Up3
motions17–21, text/camera/facing/waits22–51, jumps52–57 and end58.
Recursive inspection includes all nine scene entities' Init actions and
every nested motion. Unlike Battle31's ac_animSpeedX2 case, **no nested
native-call occurs here**: Init, Up3 and Jump all lower to known actions.
Top-level native16 and missing selected deployment remain separate gaps.

| Actual consumer | Representable behavior / specific boundary |
| --- | --- |
|[ExplorationContentReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Known nested move/flags/speed/acceleration/size/refresh/jump/idle actions are admitted. Native cameraSpeed becomes UnsupportedInstruction and reached `program-opcode` refusal; it does not install native speed48. Position/face forms do not carry original alive-helper checks.|
|[SceneEntities](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapEntityAllocator](../../../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs)|Scene Build/Reload consumes prepared population, suppresses duplicate ally allocation, retains aliases and guards49-slot capacity. Original arbitrary-state capacity/HP/class/alias fidelity is not established. In this actual lowering, population has19 ordinary follower rows and **no65 Caravan prefix**: OriginalPrograms.population reads table_Followers without the original overworld/65 selection. Missing selected conditional population remains explicit.|
|[EntityActionRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs), [ExplorationDispatcher](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationDispatcher.cs), [MapTransfer](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|Motion installation sets cursor0, slot timer, optional9F collision clear and clears follower ownership. Wait=true yields ScriptIdle wait. Enabled services move each slot then execute its actions, including sprite-readiness waits; relative motion has bounds/collision guards. Selected Init can be serviced during earlier waits/fade15 before camera16. No earlier unsupported nested opcode is present; admitted resources/actors/readiness and no earlier failure are still premises.|
|[PrivateBattleEncounterReader.Pin](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [BattleEncounterDefinition](../../../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs), [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Encounter/placements/regions/point/terrain/scene and fresh-before-load-start mechanisms exist. Pin admits1/2/3 and rejects32. No selected live party, seven definitions/items, skipped143/list/slot policy,432/532/482 or world/start/material binding is supplied. F88 NewBattle rejects `suspended-battle-entry`.|
|[MapPartyMembership](../../../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Joined/active/reserve rebuild shapes exist. No selected32 implementation of HasJaroJoinedTheForce's empty-list scan or retained enemy slot follows from those shapes; scene22/30/31 is not membership.|
|[ExplorationViewRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../../../remake/game/src/Battles/BattleMapViewport.cs)|Layer254/zero origins/256 parallax/zero autoscroll passes field profile and fixed board plane-motion guards. Native camera speed remains refused; nonzero admitted ViewSpeed separately hits `field-view-speed`. Board selects area by crop origin, original LoadBattle by view target; missing-area modern null/refusal differs from original first-record fallback.|
|[ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs)|Godot effect/sprite/window consumers exist. Selected Map74/layout/terrain/sprites/portrait/audio/text bindings, enabled motion service completion, concurrency, exact presentation/timing and usable control remain unassessed. No running instance or screenshot was used.|

In particular, ProgramRunner installs Init at6 before its wait; installation
does not itself execute an action. Selected motion waits can advance
EntityActionRunner. Fade15's Script FullFadeWait calls
MapTransfer.FadeTick→ExplorationDispatcher.Service with entityEnabled=null;
selection is explicit override, then Story.EntityServices, then program
EntitiesRunning. Without suppression this permits installed actions before
a successful fade return could reach16. RefreshEntitySprite yields for
actual readiness; action failures preserve the reached snapshot and travel
through FadeServiceFailure. A false service override suppresses that
opportunity. This is a static service path, not an absolute first-error
claim or a Battle32 run. Production consumes prepared Content, not research
JSON/disassembly on demand. Accepted shapes, explicit refusals, missing
content and unassessed fidelity remain distinct.

## Reproduction and retained limits

Load ignored selections in each launching process; reuse the pair unchanged.
The direct review checks the complete38-record/five-block/list/four-text
scene; all28 placement rows/two regions/point; definitions/equipment,
Jaro/UpdateForce/hidden-slot branches; hooks/load/map/terrain and actual
recursive lowering/current consumers. Review every prose phase and changed
state, not just matching a few terms.

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1082/review.py <configured-source-disasm-root> --committed
git diff --check
```

On clean committed HEAD the scope check compares exact owned paths from
accepted base to HEAD and separately requires clean status, including the
navigation-only predecessor comparison. Run
`uv run sf2 verify plan --base origin/main --head HEAD`; retain actual
exact-head scoped CI in the Issue/PR handoff. Documentation scope skips
blanket normal/full Python/.NET/Godot/H3/H4 and tests of audit programs.
No parser/schema/fixture change creates new mutation-test obligations.

Preserve #1080's original author0.302s/CI39488100152 missed inventory claim,
the independent `review-main-gate.log` clean-head AssertionError[] and
accepted focused independent0.107s/CI39489091772 correction. The placement
NOTHING versus retained definition inventory correction is applied above.
Retain #1076's nested-action correction, earlier LEADER/selector-width/HP/
geometry/N==V corrections and their Unknown chain. Twelve completed old
comparison failures remain discovery evidence, not a rerun queue.

**Unknown:** natural route/admission; actual incoming words, flags, crop,
HP/class/equipment/difficulty/membership, cached bytes/old143, aliases,
population/capacity; interleavings and interrupted prefixes; completed
text/action/fade/camera/audio/sprite/terrain services; original presentation,
timing, control and persistence. These Unknowns are not an acquisition queue.
