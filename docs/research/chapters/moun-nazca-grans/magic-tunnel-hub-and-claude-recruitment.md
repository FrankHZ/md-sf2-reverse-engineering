# Magic Tunnel Hub, Pipe transitions and Claude recruitment

Evidence date: 2026-10-22. **Confirmed (static):** Map63 connects Achilles
Shrines50 and Granseal Kingdom66 through four ordinary warps, two block-copy
toggle entries and two scripted Pipe64 journeys. Both Claude talk and explicit
Arm use can remove a force-held Arm before awakening; membership, active-party
admission and immediate following are distinct. **Inferred:** these declarations
describe an optional tunnel/recruitment excursion. **Unknown:** a naturally
walked itinerary, actual incoming state, completed presentation/control and save
durability. This is the selected static chapter, not a playable or all-recruits claim.

## Agreement, provenance and reproduction

**Standard** rigor for [Issue1208 / Epic1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1208):
one connected optional-content account for later design and implementation.
Coupled map, inventory, scene and party lifetimes require explicit caller and
changed-state review. Accepted base is
`b8ca89ce5529d1757f527849aa62ecede0cec045`; no stacked dependency.
Ownership is this file, bounded Arm/Claude/tunnel references in
[story coverage](../../story-content-coverage.md) and a short frontier in
[source coverage](../../source-coverage.md). No code/schema/fixture, global index,
engine, acquisition, lowering or runtime work belongs to this outcome.

Original paths below are relative to `disasm/` at read-only
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Registered USA ROM SHA-256 is
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This adds no source/ROM equivalence claim. `sf2enums.asm`, `sf2const.asm`,
`sf2macros.asm`, `sf2mapsetupmacros.asm` and `sf2cutscenemacros.asm` own constants,
RAM and encodings. Fnnn means flag nnn; coordinates/IDs are decimal, addresses
hexadecimal, intervals half-open. `map/` means `code/common/scripting/map/`;
local Map63 paths mean `data/maps/entries/map63/`, callbacks under `mapsetups/`.

The [Moun Arm owner](./moun-town-painter-and-golem-arm.md#outgoing-requests-and-stopping-boundary)
stops before this journey. Its F834 appearance/F835 award and custom Search
remain acquisition evidence; neither F835 nor a Moun visit is checked locally.
The [Battle15 owner](../southern-travel-creed/battle15-shrine-approach-and-outcomes.md) and
[Battle0 owner](../final-confrontations-ending/battle00-bonus-battle-and-reset.md) retain their battles and
outcomes. Reuse [requests](../../world/map-event-request-consumption.md),
[search](../../world/field-search-control.md), [item effects](../../world/field-item-effects.md),
[map content](../../world/map-content.md), [stats](../../party/common-stats.md) and
[scripting](../../world/common-scripting.md) for shared evidence. Actual selected callers
and included bodies, not similarly named alternate files, support this account.

Finite input is all eighteen Map63 files (18,229 bytes), all eight Map64 files
(1,519 bytes), selected adjacent/shared consumers and the retained18,285,979-byte
map-init/map-script-engine pair. Its provenance remains `local/issue1005/pair.txt`;
no regeneration or simulated route is needed. Linear source/prose checks budget
under60s/512MiB per check and under4MiB of new ignored `local/issue1208/` receipts;
peak memory is unmeasured. No new environment, download, corpus or API batch.
Reassess a material contradiction or expanded supporting tool before scaling.

Direct source review is reproducible from the configured pinned root:

```powershell
# $disasm is the configured read-only pinned disasm directory.
git -C (Split-Path -LiteralPath $disasm) rev-parse HEAD
git -C (Split-Path -LiteralPath $disasm) status --short
Get-ChildItem -LiteralPath (Join-Path $disasm 'data/maps/entries/map63') -Recurse -File |
    ForEach-Object { $_.Name; Get-Content -LiteralPath $_.FullName -Encoding utf8 }
Get-ChildItem -LiteralPath (Join-Path $disasm 'data/maps/entries/map64') -Recurse -File |
    ForEach-Object { $_.Name; Get-Content -LiteralPath $_.FullName -Encoding utf8 }
```

Then follow the named include/table/consumer sections below. The local Issue
handoff contains the full one-off document/scope recipe and dependencies,
clean committed planner and actual scoped CI. Mechanical checks do not certify
source semantics. No normal/full, SDK, engine, Godot, H3/H4, emulator, screenshot,
lowering or historical comparison rerun is admitted here.

## Entrances, four exits and battle admission

**Confirmed:** `map50/6-warp-events.asm` row0 is `(27,3)` →63 `(8,51)` Right;
`map66/6-warp-events.asm` row0 is `(40,7)` →63 `(37,57)` Up. All four63 rows
in `6-warp-events.asm` (`$BB040..$BB062`, including terminator) are:

| Source X,Y | Destination map / X,Y / facing |
| --- | --- |
|37,58|66 OVERWORLD_GRANSEAL_KINGDOM /40,8 /Down3|
|17,2|CURRENT255 /37,40 /Down3|
|37,40|CURRENT255 /17,2 /Left2|
|8,51|50 ACHILLES_SHRINES /27,3 /Down3|

All six selected inbound/outbound declarations are no-scroll mode0, padding0.
No row tests Arm possession, F29, F835 or F256. The CURRENT rows are reciprocal;
63→50 arrives on50's return trigger and50→63 on63's return trigger. This does
not prove an automatic bounce: layout admission, movement/input and pending
events still matter. The66 entrance and return differ by Y7/Y8.

`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint` divides sampled
coordinates by384, scans ordered eight-byte rows until FFFF.W and treats
negative trigger bytes as wildcards; selected rows are exact. On match it
writes event1, then payload/facing, then WARP_SFX. Walking's layout/movement
gates admit `$1000` warp checks and `$1400` zone checks; rows alone prove
neither reach nor walkability. No decoded collision/terrain or continuous
path between these points, the bridge, Claude and Moun is claimed.

`explorationfunctions_2.asm:ProcessMapEvent` clears the request before its
six-way dispatch. No-scroll type1 plays/clears WARP_SFX, idles main, discards
the exploration return address and samples physical main0 using
UpdatePlayerPosFromMapEvent. Nonnegative destination bytes replace low bytes
of those sampled coordinate words; supplied high bytes are not globally cleared.
Ordinary nonnegative tile samples produce the table positions. MainLoop's
`@Start` calls **SwitchMap then CheckBattle**. Only3/F640→4,71/F530→78 and
58/F999→59 are flag substitutions; none changes50,63 or66.

CheckBattle scans battle order, requires each row's unlocked flag and tests
its trigger coordinates, including wildcardFF. A selected completed flag
clears that unlocked flag but does not reject the match:

| Requested map | Immediate admission boundary |
| --- | --- |
|63, including CURRENT resolved from63|Row0: crop0,12 size32×36, triggerFF,FF, requires400. Clear400 gives no63 battle; held400 can admit0 regardless of500. Held500 clears400 after selecting0.|
|50|Row15: crop0,0 size32×24, triggerFF,FF, requires415. Clear415 admits exploration; held415 selects15 even with515, clearing415 in that completed case. Accepted Battle15 owner retains the battle.|
|66|Rows2,3,4,6 have wildcard triggers and F402/F403/F404/F406. First unlocked match wins; crop rectangles do not restrict wildcard triggers. Clear all four admits exploration.|

Ordinary63 warps do not set400. EndGame separately clears entities, sets400,
supplies63, calls CheckBattle, then BSRs BattleLoop and resets through the start
vector after normal returns. That reset contract cannot be transferred to an
ordinary MainLoop entrance because the same row is selected. Stop at accepted
battle owners if intercepted. Map50/66 exploration beyond these exact seams,
including later Yeel/Chaz/Lemon and other encounters, remains outside this slice.

## Complete profiles and live setup selection

**Confirmed:** `data/maps/entries.asm` binds each header to its own blocks/layout
incbins and all seven area/event/item pointers, then FFFFFFFF.L. Private
`0-blocks.bin`/`1-layout.bin` bindings are resources, not public assets or proof
of geometry. Both headers use palette6 and tilesets17,255,34,35,36; slot2's255
is the absent-tileset sentinel. Every area has type255 and initial0,0 autoscroll
on both layers:

| Map / area | Main start→end | Secondary foreground / background | Main / secondary parallax | Music |
| --- | --- | --- | --- | --- |
|63 /0|32,36→42,58|0,0 /0,0|256,256 /256,256|SHRINE|
|63 /1|0,0→31,11|0,0 /32,0|256,256 /128,128|SHRINE|
|63 /2|0,12→31,63|0,0 /32,0|256,256 /128,128|SHRINE|
|64 /0|0,0→10,63|0,0 /11,0|256,256 /256,256|NOTHING|

Map63 header is `$BAFB0..$BAFB6`; areas `$BAFDE..$BB03A` include three records
and endWord. Flag/step/roof sections `$BB03A..$BB040` are each terminator-only.
Chest `$BB062..$BB064` and other-item `$BB064..$BB066` are also endWord only.
`s4_descriptions.asm` `$5CA24..$5CA26` is RTS, with no local description table.
Empty ordinary pickup tables do not exclude the explicit scripted Arm use.

Map64 header is `$BB6B2..$BB6B8`, area `$BB6E0..$BB700`. Flag, step, roof,
warp and chest sections are successive terminator-only words `$BB700..$BB70A`.
Other-items `$BB70A..$BB70E` is endWord then RTS; that RTS is not a setup/init
callback. The complete MapSetups scan has no64 row. GetCurrentMapSetup reaches
the final terminator and returns `ms_Void: FFFF.W`. GetMapSetupEntityList
leaves that pointer unchanged; InitializeMapEntities readsFF and ends its
fixed list. Other setup wrappers skip on that sentinel. No local64 setup
directory, NPC/init, event or description is invented.

Map63 default ms_map63 is overridden only by live F29 to ms_map63_flag29.
The six-long tables are `$5C968..$5C980` and `$5C950..$5C968`. Only offset0
(entities) differs; offsets4/8/12/16/20 share entity events, zones, descriptions,
item events and init. GetCurrentMapSetup reselects from CURRENT_MAP/live flags
on each call; a changed29 does not rebuild actors already allocated.

| Setup | Fixed declarations and shared init |
| --- | --- |
|29 clear|One row: Claude29 /21,25 /Down3 /eas_Init (`$5C980..$5C988`). Immediately included s1_entities_29 supplies msEntitiesEnd at `$5C988..$5C98A`; default is not unterminated. Init calls cs_5CA5E: setSprite29,POSE3=176; setFacing29,Down; stopEntity29; end.|
|29 held|Terminator-only fixed list. Shared init returns without the pose scene.|

InitializeMapEntities clears records/64 alias bytes, builds flag-selected
ordinary followers first, eligible fixed rows afterward, then main0 and the
follower terminator. Claude29 is a logical ally identity, not physical slot29
or nonally128. A nonzero existing ally alias suppresses a duplicate fixed row;
ordinary follower declarations contain no29. GetAllyMapsprite's class/HP
selection differs from explicit POSE3. Physical slots depend on followers;
missing/overwritten aliases are not repaired by these callbacks.

eas_Init sets factors0,0, speed32,32, acceleration/deceleration OFF,
obstructable ON, map/entity uncollidable OFF, autoFacing ON, orientUp,
immersed OFF, size24 and refresh, then eas_Idle. setFacing also refreshes.
stopEntity writes ANIMCOUNTER byteFF; startEntity writes0. Neither allocates,
hides or stops an entire action stream. start/stop use their word actor's
current-HP guard; packed setFacing uses its actor byte. HP0 can skip these
operations. setSprite itself resolves ally sprite selectors below30 and
waits/refreshes, without creating an actor or healing Claude.

## Bridge toggle and layout lifetime

**Confirmed:** s3_zoneevents has four ordered rows:15,55→Event0;13,42→Event0;
10,21→Event2;8,7→Event3; default RTS. RunMapSetupZoneEvent matches one row/default,
runs it, closes windows, waits VInt/main stop and restores registers. Ordinary
control posts event6/coordinates; ProcessMapEventType6 then installs main
eas_Init before that dispatch. It does not run all matching callbacks.
Both toggle coordinates share F256:

| State / coordinate | Complete callback order |
| --- | --- |
|256 clear at either toggle|script cs_5CA6E returns →set256→RTS|
|256 held at either toggle|script cs_5CB34 returns →clear256→RTS|
|10,21|script cs_5CC26→RTS; no256 write|
|8,7|script cs_5CCF6→RTS; no256 write|
|Other admitted coordinate|Default RTS; no toggle/pipe|

cs_5CA6E copies working source16,54 size1×1 to all destinations below, in order.
Each setBlocks is followed by SFX_DOOR_OPEN_HIGH_PITCH and csWait; end follows:

| Ordinal | Destination | Wait |
| --- | --- | --- |
|0|13,43|20|
|1|13,44|18|
|2|13,45|16|
|3|13,46|14|
|4|13,47|12|
|5|13,48|10|
|6|14,48|8|
|7|15,48|6|
|8|15,49|5|
|9|15,50|4|
|10|15,51|3|
|11|15,52|2|
|12|15,53|1|
|13|15,54|1|

cs_5CB34 has seven triples then end. Each first setBlocksVar copies16,53
size1×1 to its destination, then setBlocks performs the second copy, then waits.
There is no playSound in this program:

| Ordinal | Var destination | setBlocks source→destination, size1×1 | Wait |
| --- | --- | --- | --- |
|0|15,54|12,43→13,43|30|
|1|15,53|16,53→13,44|25|
|2|15,52|16,53→13,45|20|
|3|15,51|16,53→13,46|15|
|4|15,50|16,53→13,47|10|
|5|15,49|16,53→13,48|5|
|6|15,48|16,53→14,48|1|

csc34/csc35 read three packed words and call CopyMapBlocks. The helper copies
live64-wide layout words at FF0000, two bytes per cell/128 per row. Sources are
current cells, not immutable resource reads. csc34 then sets plane-update bits0
and1; csc35 does not. This does not prove rendered/collision geometry or that
the second program geometrically inverts the first.

Fresh explicit exploration reconstructs population, clears256–383 and sets80,
then explicit LoadMap rebuilds blocks/layout and runs init. CURRENT exploration
preserves population/temp flags, updates main and reloads properties without
LoadMapBlocksAndLayout, then still runs init. CURRENT warps can retain working
copies/256; explicit63 begins with base layout/clear256. Supplied flag/layout
mismatches are not validated or repaired. Interrupted copies can leave256
unchanged because its write is after scene return.

Script mapLoad/csc48 detaches viewFF, selects tilesets, waits until fade clears,
deactivates VInts, packs camera operands with multiplier3, calls LoadMap,
enables display/VInts and waits once. Explicit64/63 replaces layout/properties;
it does not enter MainLoop/CheckBattle, heal, clear temps, set80, run init or
reconstruct actors. csc49 separately gets the live setup, initializes entities
at supplied player position/facing, loads sprites and restores display/VInts.
Pipe round trips thus reset63 layout while retaining256: held256 can survive
with base geometry. No flag-event restoration or reconciliation occurs.

## Both Pipe64 scene directions

**Confirmed:** Event2 calls cs_5CC26, Event3 cs_5CCF6. Complete common order and
differences follow; Bowie is logical0:

| Phase | cs_5CC26 from10,21 | cs_5CCF6 from8,7 |
| --- | --- | --- |
|Departure|entityFlashWhite Bowie,50; setActscript Bowie,eas_RotateRightHighSpeed; wait30; native FadeOutToWhite|Same|
|Pipe load|mapLoad64,0,0; loadEntitiesFromMapSetup5,4,Up|Same|
|Initial scroll|native csub_5CDDC writes byte−87|native sub_5CDE4 writes byte+87|
|Pipe presentation|setActscript Bowie,eas_RotateRightHighSpeed; native FadeInFromWhite; wait30|Same|
|Acceleration|Ten native sub_5CDEC calls subtract4; wait20 after calls1–9,30 after call10|Ten native sub_5CDF2 calls add4; same waits|
|Before return|native FadeOutToWhite; wait40; native sub_5CDF8 clears scroll byte; wait2|Same|
|Hub load|mapLoad63,3,3; loadEntitiesFromMapSetup8,7,Up|mapLoad63,5,17; loadEntitiesFromMapSetup10,21,Up|
|Arrival|setActscript Bowie,eas_RotateRightHighSpeed; native FadeInFromWhite; wait30; entityFlashWhite Bowie,50; setActscriptWait Bowie,eas_Init; startEntity Bowie|Same|
|Claude|end, without pose reapplication|jumpIfFlagSet29→cs_5CDDA/end; otherwise setSprite29,POSE3; setFacing29,Down; stopEntity29; end|

Returns name the other zone's coordinate; that does not prove immediate repeat
travel without a new admitted producer. Rotation disables motion/autoFacing,
cycles Left/Up/Right/Down with refresh/wait1 each, then loops. Waited eas_Init
and counter0 start are separate restoration operations. Waits are engine counts,
not measured seconds or animation heights.

Native stores/arithmetic are **byte** operations despite `.w` absolute addressing.
Hexadecimal bytes in the negative direction start A9 then A5,A1,9D,99,95,91,8D,
89,85,81 (−87 through−127); positive starts57 then5B,5F,63,67,6B,6F,73,77,7B,7F
(+87 through+127).
Arithmetic is modulo256, not saturation; these ten updates do not cross signed
overflow. `exploration.asm:VInt_UpdateScrollingData` treats0 as no scroll, `$80` as
a special branch, otherwise sign-extends the byte, adds to plane-B Y and uses
absolute magnitude as speed. Neither sequence reaches `$80`; final clr returns0.
The actual native fades in `code/common/tech/interrupts/fadingcommands.asm`
write IN_FROM_WHITE/OUT_TO_WHITE and enter ExecuteFading, which initializes
timer/pointer/counter/palette mask, waits until FADING_SETTING clears and waits
one more VInt. The source loop is not observed speed, cadence, completion or pixels.

Each csc49 discards old actors/aliases/immediate follower scripts, then creates
flag-selected followers, fixed rows and main. Pipe64's void fixed list still
permits ordinary followers. Returned63 with clear29 creates normal Claude;
only cs_5CCF6 reapplies POSE3/stop because neither load runs init. cs_5CC26
leaves normal ally sprite/eas_Init. With29 held both lists omit the fixed row.
Claude is absent from table_Followers: join29 does not reconstruct him as an
ordinary follower. CURRENT, fresh and script loads are distinct lifetimes.

## Talk and explicit item use: custody and result

**Confirmed:** s2_entityevents selects ally29 with packed event byte RIGHT=0,
then default RTS. Its bits0/1 are both clear: neither player-facing Right nor
automatic turn/restore is required. Ordinary C uses GetActivatedEntity then
GetEntityEventIndex. The shared wrapper reselects live setup, binds actual
portrait/speech, suppresses entity VInt updates around the callback, then
closes windows/reactivates updates. The real48-slot activation, physical alias
scan and follower exclusions remain with the [interaction owner](../../world/map-event-interaction-state.md); wrong
alias, missing actor or being a follower can change admission.

| Callback state | Complete local behavior |
| --- | --- |
|EntityEvent0,29 held|Default RTS; no inventory search/scene/repair.|
|29 clear, no Arm found|GetItemInventoryLocation→−1; copy current speech to SPEECH_SFX_COPY; select/display Claude portrait/speech; txt4177 then4178; clsTxt; default RTS. No local item/flag change.|
|29 clear, Arm found|D0=ARM_OF_GOLEM117; RemoveItemFromInventory; script cs_5CBB4; default RTS. No removal-result check.|
|ItemEvent0,29 clear|RemoveItemFromInventory(117); script cs_5CBB4; D6.W=−1; branch to default RTS. No separate presence/result check.|
|ItemEvent0,29 held|D6.W=0; default RTS, without removal/scene.|
|Unmatched item table|Default RTS preserves dispatcher D6=0.|

s5_itemevents has one six-byte row: **standing** tile21,26, facing Up1,
item117, then default. GetPlayerEntityPosition samples physical main X/Y and
facing; this is not Search's faced tile21,25. RunMapSetupItemEvent masks D4
with7F, sets D6=0, scans X/Y/facing/item and runs the first match/default.
D5 carries selected slot but this handler does not use it. It preserves
D0–D5/D7–A1, leaving D6 as handled result, closes windows and waits VInt/main stop.

The real FieldMenu Item→Use in `code/common/menus/main/mainactions.asm` first
populates current force members and selects member/slot/item; cancel avoids
this callback. Arm117 is outside IsItemUsableOnField's nine ordinary IDs, so
it reaches map events rather than UseItemOnField. It supplies player position,
selected item/slot; nonzero D6 exits main. Zero displays73 (selected member
used item) then422 (no effect), closes and returns to the submenu. There is
no second selected-slot removal on the handled map-event path. Ordinary effects'
separate removal and Caravan effects are not Claude's consumer. A direct caller
could supply a matching item without owning it; table matching is not custody
validation.

### Party-wide search and physical removal

**Confirmed:** use the included `code/common/stats/iteminventory.asm` and
`itemstats.asm`, not obsolete alternate sources. GetItemInventoryLocation
takes D1.W, calls UpdateForce and scans TARGETS_LIST in ascending joined-member
order0..29. UpdateForce tests joined0..29 and includes reserves/dead members;
there is no active/HP filter. Battle capacity is a separate authority.

For each member, GetItemBySlotAndHeldItemsNumber counts non-NOTHING entries
across four words after masking index7F. If count>0, the search checks slots
count−1 down to0, masks each word with7F and compares to requested117. Duplicates
therefore choose the earliest joined holder and highest matching slot in that
counted prefix. Equipped/broken/high bits do not prevent a match. Packed
inventories are fully scanned; supplied holes can hide a high occupied slot
because count is not highest occupied slot+1. Empty members skip, and empty
index127 cannot match117. Arbitrary inventories are not normalized.

The force loop starts at joinedCount−1 without prechecking zero; a supplied
zero-joined force enters unchecked DBF scanning, not a safe empty-result
contract. No natural zero-member state is asserted. Normal exhaustion of a
nonempty force returns D0.W/D1.W=−1. A match yields holder in D0's low byte
and slot in D1.W, not a unique narrative quest owner.

RemoveItemFromInventory saves D1/D2, copies requested D0→D1 and searches again.
Absent returns−1; found calls RemoveItemBySlot with that holder/slot. That
helper tests masked127, otherwise shifts subsequent words left and writes
NOTHING in the last slot; D2=0 on removal,3 on nothing. It tail-calls
UpdateCombatantStats. The outer returns0 for success or−1 for absent/nothing,
restoring D1/D2. Both Claude callers ignore its D0 and run the scene after
the call. The talk path's first search is not a transaction spanning the second
search/removal and every later scene service.

With duplicate Arms, menu-selected holder/slot can differ from the actual
party-wide first-holder/highest-prefix-slot removed. One found entry is removed
per call; there is no all-Arms sweep, grant/refund, F835 prerequisite, owner
identity, rollback or success reconciliation. Held835 without custody gives
absent speech; custody with clear835 can recruit. A supplied matched item event
without joined-held Arm still enters the scene because failure is unchecked;
this does not mean the ordinary menu fabricates items.

## Awakening, membership and immediate following

**Confirmed:** cs_5CBB4 (`$5CBB4..$5CC26`) has30 commands including end:

| Ordinal | Operation and public paraphrase |
| --- | --- |
|0–1|textCursor4179; nextSingleText FF,ENTITY_NONE: leader puts Arm onto golem.|
|2–4|setSprite29,29 (resolve ally mapsprite); startEntity29 (counter0 under HP guard); nextSingleText0,29: awakening exclamation4180.|
|5–10|setActscriptWait29,eas_Jump; setQuake2; playSound DOOR_OPEN_HIGH_PITCH; wait20; setQuake0; wait40.|
|11–17|setFacing29 Left; wait20; Up; wait20; Right; wait20; Down.|
|18–23|setActscriptWait29,eas_Jump; setQuake2; same sound; wait20; setQuake0; wait30.|
|24–26|nextText0,29:4181 power restored; nextText0,29:4182 thanks/ability to move; nextSingleText0,29:4183 offers to join/protect.|
|27–29|join29; addNewFollower29; csc_end.|

No choice, F835, refund, duplicate-item reward or active-party guarantee occurs.
textCursor advances at the five text consumers; join's fixed446 does not consume
it. Actual keyed text, not comments, gives:

| Decimal / hex key | Complete ordered controls/substitutions |
| --- | --- |
|4177 /1051|N,W2|
|4178 /1052|N,W1|
|4179 /1053|LEADER,N,W1|
|4180 /1054|W1|
|4181 /1055|N,W1|
|4182 /1056|N,N,W1|
|4183 /1057|N,W1|
|446 /01BE|NAME,CLASS,N; no inline W token|
|73 /0049;422 /01A6|73: NAME,N,ITEM,W2;422: D1|

W1/W2, breaks, FF narration/speaker flags, nextSingleText's own window/wait work
and callback cleanup are distinct. eas_Jump disables autoFacing, moveRel0,−1;
wait5; moveRel0,+1; wait5; moveRel0,0; autoFacing ON; jump eas_Idle. These are
action operands, not measured height or tile-path assertions. csc15 installs
the pointer and physical-slot wait timer, then waits for eas_Idle for nonzero
wait byte; it has no HP skip. setFacing/start/stop have their stated HP guards.
None allocates a missing actor or guarantees valid alias recovery.

csc08_joinForce clears current speech, waits view scrolling, reads29 with
bit15 clear, requests MUSIC_JOIN and calls JoinForce(29), then GetClass,
NAME/CLASS446, FadeOut_WaitForP1Input, close text, wait10, return. Special128
Sarah/Chester and sad-join paths are unselected. JoinForce sets joined29,
calls UpdateForce, and only when its counted active prefix is below12 calls
JoinBattleParty to set active61. It does not clear preheld61 at capacity and
does not rebuild the lists after that conditional flag write. Full active
party can gain joined/reserve Claude; supplied active61 keeps its consequences.
Join neither heals nor replaces inventory/stats/class, nor guarantees deployment.

csc56_addFollower reads29, resolves its **existing physical actor**, scans
EXPLORATION_ENTITIES toFF retaining last follower or0 if empty, then calls
AddFollower with D2.W=FFE8 (−24), D3.W=0. AddFollower first installs a fresh
42-byte eas_Follower1 copy from the walking buffer, patches leader/X/Y at
offsets `$1E/$20/$22` and advances buffer42; only then checks list membership.
Existing entry avoids duplicate append but still replaces the script; absent
entry appends before a newFF. If the target is already the last follower,
csc56's selected leader can be itself; duplicate suppression does not fix that
relationship. There is no capacity/missing-actor guard.
Full/malformed lists or exhausted buffer have no safe rejection guarantee;
48/49-record storage conventions do not establish supported follower capacity.
Installation alone does not prove movement.

The copied template sets speed32,32/factors0, acceleration/deceleration OFF,
obstructable OFF, map/entity uncollidable ON, then follows, waits destination
and loops. csc56 has no HP guard. Distinguish csc2C/followEntity: its initial
**nonadvancing byte read** from a word actor passes the high byte to the HP
helper; ordinary00xx tests ally0 HP, not automatically actorxx. That shared
quirk must not be attributed to addNewFollower or silently repaired here.

## Revisit, interruption and alternate state

**Confirmed conditional consequences:** no transaction/save guarantee is added:

| State or stopping prefix | Later local consequence |
| --- | --- |
|Arm removed; interrupted before join|Arm can be gone while29 clear. Later talk uses remaining custody; no refund.|
|Sprite/start/jump/quake prefix only|Actor/action/quake state can differ with29 clear; clear29 does not prove pristine statue. No rollback.|
|join commits; join text/music/wait or follow incomplete|29 may be held without following. Both local guards skip recruitment, without rejoin/follow repair.|
|29 held in live loaded population|Selector changes leave actor present until rebuild; guard/default avoids consuming another Arm.|
|Fresh explicit63,29 held|Empty fixed list/no pose init; ordinary follower table has no29. Immediate scene following is not persistent recruitment-following.|
|CURRENT warp while Claude follows|Population/list/scripts retained apart from ordinary main/property/init effects; held29 init does nothing.|
|Script pipe rebuild after join|Immediate Claude following discarded; held29 fixed list empty. Membership/active flags remain independent.|
|Clear29 with active61, HP0, duplicate Arms, reserve/dead holders or changed class|Use actual membership/custody/HP/alias consumers; no artificial first visit, one Arm or fixed roster prerequisite.|
|256/layout disagree after interrupted toggle or pipe|Next toggle tests256 alone; no layout-derived repair.|

All four exits remain table entries in both29 setups; reach, pending requests,
terrain and battle interception still apply. No save-range inference is needed:
local code calls no SaveGame/LoadGame. Existing stats/SRAM evidence retains its
bounded in-process seam. Physical durability, restored population and naturally
revisited inputs remain **Unknown**.

## Actual H2, Python, Content, Domain and Application consumers

**Confirmed by source inspection:** the retained pair holds parsed init/scripts
and provenance. [Map init H2](../../../../src/sf2tool/h2/map_init.py) reads selected
init/embedded programs, pointer/route joins and standalone targets;
[map scripts](../../../../src/sf2tool/h2/map_scripts.py) records complete standalone
programs; [map-script engine](../../../../src/sf2tool/h2/map_script_engine.py) records
macro/opcode/handler facts. These are evidence, not a natural route or executable
Hub. No parser, pair rebuild or selected lowering was run.

The [coordinator](../../../../src/sf2tool/remake_exploration_content.py) and
[map preparer](../../../../src/sf2tool/remake_exploration_maps.py) consume verified
canonical export/source into prepared content. Runtime Content reads prepared
packages, not fixtures/disassembly on demand. [Current capability](../../../../remake/docs/capability-status.md#milestone-and-deferred-surfaces)
retains unsupported item acquisition/area-description/setup selection and
save/load boundaries; opening behavior does not establish late-map admission.

| Actual owner | Reusable expression / selected boundary |
| --- | --- |
|[OriginalPrograms](../../../../src/sf2tool/remake_exploration_programs.py)|Flag branches/calls/text/sprites/facing, counter0/255, actions, quake, sound and join have typed cases. **setBlocks, setBlocksVar, mapLoad, loadEntitiesFromMapSetup and addNewFollower have no matching cases and reach native-call fallthrough.** loadMapFadeIn/loadMapEntities/followEntity are separate supported names, not aliases for missing cases.|
|OriginalPrograms native subroutines|executeSubroutine recursively compiles the target. Five autoscroll bodies use untyped move.b/subq.b/addq.b/clr.b stores. Recognizing a call does not support its body. FadeOutToWhite/FadeInFromWhite also require real recursively reached instructions/services; no successful lowering or universal earliest pipe error is claimed.|
|[Join producer](../../../../src/sf2tool/remake_exploration_joins.py)|join29 emits camera wait, JOIN sound, join-party, fixed446/nameMember29, sound wait/previous music, input wait/close/ticks. Existing service policy is not original timing or Claude resource acceptance.|
|Inventory/item callbacks|GetItemInventoryLocation/RemoveItemFromInventory jsr and compare/result routing are not an implemented exploration item transaction. Entity/zone conversion does not import s5/D6/menu-selected-member routing. Typed join does not remove Arm.|
|[Content reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [StoryProgram](../../../../remake/src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs)|Motion/scene-map/scene-entities/animation/quake/join/follow shapes exist. native-call becomes UnsupportedInstruction; native action becomes UnsupportedEntityAction. Types do not supply conversion/alias/HP/capacity semantics.|
|[ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Native raises program-opcode before advancing that instruction, retaining completed prefixes; unsupported action rejects. SetMapEntityAnimation writes counter without source HP precheck. Facing/sprite/actions require actual actors/resources/completion. Typed suffixes cannot bypass native inventory/block/scene work.|
|[MapPartyMembership](../../../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Rebuild/Join preserve joined/active/reserve, capacity and pre-active list timing. ProgramRunner.JoinResources also requires unique admitted actor/start when encounter starts exist and adds initial resources only once. No Claude binding, Arm custody or deployment is established.|
|[MapTransfer](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [MapEventDispatcher](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs)|Prepared population, preserve/rebuild and scene loads exist for admitted content. Preparation/load-selected setup differs from original live29 reselection. Selected63/64 resources, void64, rebuilt followers and asymmetric pose lifetime need bindings.|
|Map preparer / [domain copy reducers](../../../../remake/src/Sf2.Remake.Domain/Maps/MapBlockCopyAction.cs)|Static flag/door/roof copies use layoutEvents/reducers. That does **not** implement scripted csc34/csc35 working-copy/update-toggle programs. Empty63 static copy tables do not make its native bridge supported.|

Review compiler register/branch handling and native fallthrough at each actual
operation, including labels after scene returns; no fabricated lowered counts
or query results are supplied. Future integration needs selected trusted
materials/text/sprites/aliases, caller routing, inventory effects, native
scene/copy/autoscroll support and reached state/control observations. Moun
acquisition bindings, tunnel resources/destinations and this static recruitment
finding remain separate. None alone grants production/design acceptance.

## Acceptance and remaining Unknowns

Author review covers every local declaration/selected shared body against
consumers/prose, changed29/61/256/835, HP/classes, reserve/dead holders,
duplicate/flag-bearing/holey inventory, zero joined input, missing aliases,
preexisting/full follower lists, interrupted scenes and all three load lifetimes.
All seven Issue groups apply; a join label or mechanical PASS is insufficient.
Local handoff retains exact head/tree/base, full diff, checks/CI, corrections
and stopped-process state.

**Unknown:** natural entrance/zone/talk/menu/warp reach and continuity from
Moun; supplied flags/party/custody/aliases/layout/request/player mode; completed
scroll/fade/quake/jump/follow/text/input/music/VInt services; visual meaning,
collision/walkability, usable return control and save durability. Existing
bounded helper H3 does not establish this chapter. These questions are not
an acquisition queue under [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md).

Other Yeel/Chaz/Lemon entrances and late revisits remain with the story frontier.
No all-recruits, Epic1053, full-game or runtime/engine completion is claimed.
Stop at clean committed/pushed frozen Draft PR, actual scoped CI and successful
main-gate notification for independent review/integration. No merge, next
dispatch, Epic closure, archive or cleanup belongs to this slice.
