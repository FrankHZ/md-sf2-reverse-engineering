# Devils Head labyrinth and Battle40 admission

Evidence date: 2026-10-21. **Confirmed (static):** Map41's final warp
enters Map28 at16,30/Up. Map28 has no setup NPC records but retains the
ordinary follower/player initializer. Its default zone callback tests
physical entity0's facing byte: Right0 selects the right-down script;
every nonzero byte selects left-down, with an additional pre-jump wait.
Two step copies and a separately flag-conditioned layout copy do not
constitute a persistent puzzle-completion predicate. The first wildcard
warp requests Map54 at16,41/Up. Battle40 admission requires440;540 clears
440 after the crop writes but still returns40 on that invocation.
Natural traversal, completed services and playable remake support are
**Unknown**.

## Agreement, provenance and method

Standard rigor for [Issue #1156](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1156)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053),
from accepted base `ae3a3d04869651739aba1b74481c4098b5f44022`.
The decision supported is how future content can express labyrinth
state, callback order, map connections and admission boundaries. Own
this document and navigation only in [the predecessor](./battle39-outcomes-and-force-sword-route.md).
No shared registry/index/schema/fixture/tool/engine change or unmerged
dependency. The stopping point is the call into Battle40 opening, or
the no-battle branch into exploration. Battle40's scene, roster,
activation, actions, results and tile-change-producing scenes remain
outside; there is no maze solver, rendered map or runtime acquisition.

Reuse accepted [request production](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md),
[common maps](./common-maps.md), [scripting](./common-scripting.md)
and [Nazca crash handoff](./nazca-ship-shootdown-and-crash-handoff.md).
Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`map/` abbreviates `code/common/scripting/map/`, `entity/` abbreviates
`code/common/scripting/entity/`, `battle/` abbreviates
`code/gameflow/battle/`, and `mapNN/` abbreviates
`data/maps/entries/mapNN/`. Fnnn means flag nnn; ordinals are
zero-based and intervals half-open. sf2const.asm, sf2enums.asm,
sf2mapsetupmacros.asm, sf2cutscenemacros.asm and sf2macros.asm own
addresses, constants and encodings. The USA ROM identity remains
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new source/ROM equality or original-runtime observation is claimed.

Minimum evidence is the complete short Map28 setup/scene and local
tables, Map54 tables, and the necessary shared callers/consumers.
Reuse the18,285,979-byte init/script pair and `local/issue1005/pair.txt`
provenance read-only, without regeneration. One selected compiler
observation answers whether the callback and its two scenes lower;
it does not import a world or decode a route. Work is linear in the
selected files plus one pair read, with fixed small state cases.
Estimated receipts/text below2MiB, direct commands seconds, peak-memory
target512MiB **unmeasured**. No emulator/network asset acquisition or
whole-pair model processing. Reassess a new cross-system mechanism,
large output or acquisition before expansion. Ignored recipes/results
belong to `local/issue1156/`; previous evidence remains intact.

## Map28 entry, setup and complete local tables

**Confirmed:** `data/maps/entries/map41/6-warp-events.asm` row4,
trigger6,7, targets Map28 at16,30/Up with no scroll. Its admission has no battle
coordinate row: `data/battles/global/battlemapcoords.asm` contains45
seven-byte rows, none selecting28. `data/maps/global/flagswitchedmaps.asm`
has no28 or54 switch. A destination row alone proves neither reachable
terrain nor a completed warp service.

`data/maps/mapsetups.asm:MapSetups` has exactly one `msMap 28,ms_map28`
followed immediately by msMapEnd, with no flag variant. The pointer table
at$5F34C..$5F364 has six long pointers in s1–s6 order:

| Section / complete named surface | Exact content |
| --- | --- |
|s1 `$5F364..$5F366`, `ms_map28_Entities`|msEntitiesEnd; zero setup NPC/entity records.|
|s2 `$5F366..$5F36C`, `ms_map28_EntityEvents`|Default relative handler Map28_DefaultEntityEvent; rts.|
|s3 `$5F36C..$5F396`, `ms_map28_ZoneEvents`|Default relative handler Map28_DefaultZoneEvent; complete callback below. No coordinate-specific row.|
|s4 `$5F396..$5F398`, `ms_map28_AreaDescriptions`|rts, no description record/text.|
|s5 `$5F398..$5F3A0`, `ms_map28_Section5`|Default relative handler Map28_DefaultItemEvent0; rts.|
|s6 `$5F3A0..$5F3A2`, `ms_map28_InitFunction`|rts.|
|scripts `$5F3A2..$5F3E8`|cs_5F3A2 and cs_5F3C4; all commands below.|

`ExplorationLoop` in `code/gameflow/exploration/explorationfunctions_2.asm`
sets CURRENT_MAP and CURRENT_BATTLE=NOT_CURRENTLY_IN_BATTLE for a supplied
map, clears VInt functions, obtains the selected entity list, initializes
entities, loads mapsprites, clears setup temporary flags, sets80, then
loads the map and calls its init function after SetBaseVIntFunctions.
`j_GetMapSetupEntities` in `code/common/tech/jumpinterfaces/s07_jumpinterface.asm`
is an alias of GetMapSetupEntityList, not a second population function.

`map/mapfunctions.asm:InitializeMapEntities/loc_440E2` clears entities,
initializes followers first, reads setup records until the first FF
byte, and still declares physical slot0 at the supplied position/facing
with ally0's mapsprite and eas_Idle. `InitializeFollowerEntities` excludes
only Tactical Base and Nazca Ship. Map28 and54 are absent from
`data/maps/global/overworldmaps.asm`, so the ordinary `table_Followers`
and pt_eas_Followers are selected, even with65 held. Each follower's
unlock flag is tested separately;71 controls Lemon's follower declaration.
The complete ordinary table is owned by `data/scripting/entity/followers.asm`;
its current flags/aliases, force membership and active party are separate
state. Empty s1 does not mean an empty rendered map or no followers.

The complete Map28 local record surfaces in `data/maps/entries/map28/`
are:

| File / address | Exact records before endWord |
| --- | --- |
|2-areas `$A9C2E..$A9C4E`|One area: main0,0..31,31; second foreground0,0/background0,32; main parallax256,256, second128,128; both autoscroll0,0; type255; MUSIC_BATTLE_THEME_1.|
|3-flag-events `$A9C4E..$A9C58`|F100: source33,8, size4×6, destination10,16.|
|4-step-events `$A9C58..$A9C6A`|Trigger15,15 then16,15; both source34,0, size2×2, destination15,14.|
|5-roof-events `$A9C6A..$A9C6C`|No records.|
|6-warp-events `$A9C6C..$A9C7E`|Two ordered records in the exit table below.|
|7-chest-items `$A9C7E..$A9C80`|No records.|
|8-other-items `$A9C80..$A9C82`|No records.|

`data/maps/entries.asm:Map28` separately binds the tilesets, blocks,
layout, all these sections and animations. Local event emptiness does
not erase that map-data ownership. The declared visible main area
differs from the64×64 working layout and off-area copy sources. No
binary geometry, marker distribution or collision map is inferred from
the table coordinates alone.

## Default zone callback and the two direction scripts

**Confirmed:** the relevant producer is
`entity/entityscriptengine_2.asm:esc02_controlCharacter/loc_5220..loc_52C0`.
It first checks obstructing entities where flags require it, reads
the candidate working-layout word, and masks3C00. Marker0400 calls
OpenDoor and rereads the word;1000 calls WarpIfSetAtPoint;1400 writes
MAP_EVENT_ZONE_EVENT=6. Its X/Y request words come from the acting
entity's destination plus movement offsets d4/d5 divided by
MAP_TILE_SIZE=384. Candidate coordinates are not a callback-local
position predicate, nor proof that motion into the candidate completed.
Collision/debug handling follows the marker production in this path.

`ProcessMapEvent` clears the event word and dispatches type6 to
ProcessMapEventType6_ZoneEvent. That handler installs init on physical
entity0 via j_ApplyInitActscript, reads the request's X/Y words, and
calls j_RunMapSetupZoneEvent→RunMapSetupZoneEvent. Its setup selector
checks ms_Void first. For Map28 the first FD default row directly
selects the relative callback regardless of those X/Y values. FD is
the table default marker, distinct from working-layout1400.

The complete callback in `map28/mapsetups/s3_zoneevents.asm` is:

1. Trap VINT_FUNCTIONS with VINTS_ACTIVATE and VInt_UpdateEntities.
2. Clear d0.w and call WaitForEntityToStopMoving on physical entity0.
3. `tst.b ((ENTITY_FACING-$1000000)).w`; `bne.s byte_5F38E`.
4. Zero byte: `script cs_5F3A2`, then bra.s return_5F394.
5. Nonzero byte at byte_5F38E: `script cs_5F3C4`.
6. return_5F394: rts.

ENTITY_DATA=$FFA902 and ENTITY_FACING=$FFA912, offset10 hex in
physical slot0. The test is one byte, not a word or a view-indexed
lookup. Canonical Right0 chooses cs_5F3A2; Up1/Left2/Down3 and any
other nonzero byte choose cs_5F3C4. It does not validate facing.
The read occurs after the init installation and callback wait; it is
not a saved copy of producer d6, input direction or candidate coordinates.
WaitForEntityToStopMoving resolves its supplied physical
index, compares X.w/Y.w against XDEST.w/YDEST.w, and repeatedly
WaitForVInt until both differences are zero; it does not wait for
eas_Idle or inspect HP.

| Complete scene command order | cs_5F3A2 | cs_5F3C4 |
| --- | --- | --- |
|Initialize Bowie alias, wait for script idle|setActscriptWait ALLY_BOWIE,eas_Init|Same|
|Additional delay before jump|None|csWait2|
|Install jump and wait for script idle|setActscriptWait ALLY_BOWIE,eas_Jump|Same|
|Delay, falling sound|csWait2; playSound SFX_FALLING|Same|
|Inline action sequence, wait|entityActionsWait ALLY_BOWIE; moveDownRight1; endActions|entityActionsWait ALLY_BOWIE; moveDownLeft1; endActions|
|Finish|csWait2; csc_end|Same|

There are no text commands or flag writes in either scene. `script`
uses Trap6_TriggerAndExecuteMapScript, which activates entity updates
before j_ExecuteMapScript and restores the saved registers on return.
`csc_end` returns the map-script interpreter; the enclosing callback
still has its own rts. After the callback, RunMapSetupZoneEvent closes
the portrait/text window, WaitForVInt, and again waits for physical
entity0 to stop before restoring its caller registers. None of these
waits establishes a wall-clock duration or an input-ready frame here.

The complete eas_Init in `data/scripting/entity/eas_actions.asm` is
accelFactors0,0; setSpeed32,32; acceleration OFF,OFF; deceleration
OFF,OFF; entityObstructable ON; entityUncollidable OFF;
mapUncollidable OFF; autoFacing ON; orientUp; immersed OFF; setSize24;
updateSprite; jump eas_Idle. The complete eas_Jump is autoFacing OFF;
moveRel0,−1; wait5; moveRel0,+1; wait5; moveRel0,0; autoFacing ON;
jump eas_Idle. The switch is **automatic facing**, not motion OFF/ON.
The two fixed waits are not waitDest, and reaching idle is not proof
of a one-tile-up-and-back geometric excursion.

`map/mapscriptengine_1.asm:csc15_setEntityActscript` resolves the
character through GetEntityAddressFromCharacter, writes the slot's
read timer and script pointer, and for Wait spins until that pointer
equals eas_Idle. csc2D_entityActionSequence resolves the same alias,
writes the timer, clears collision bits with9F, builds the inline
script and installs it, then similarly waits for idle. Neither has
AdjustScriptPointerByCharacterAliveStatus; ordinary ally HP0 alone
does not skip either installation. csc14 manual installation also
lacks that guard. The ordinary setFacing/csc23 HP guard and follow's
separate high-byte/Bowie rule must not be generalized to these ops.

GetEntityAddressFromCharacter masks the combatant ID, reads the byte
from ENTITY_INDEX_LIST and scales it by ENTITYDEF_SIZE_BITS. It does
not check absence, HP or mapsprite identity. ALLY_BOWIE0 is an alias
lookup; the wait and facing test use physical0 directly. The producer
uses acting a0; camera/view targeting uses VIEW_TARGET_ENTITY at
$FFA82C. These are distinct interfaces. Ordinary initialization is
evidence for their usual relationship, not permission to assume they
remain identical after arbitrary scene/alias state changes.

The diagonal handlers emit esc04 relative displacement(+1,+1) or
(−1,+1), followed by esc01_waitUntilDestination and a jump to eas_Idle.
`entity/entityscriptengine_2.asm:esc04_moveToRelativeDest` multiplies
by384 and adds the entity's **current** X/Y, checks same destinations,
then enters shared destination installation. It does not use request
candidate X/Y or entry16,30 as an absolute target. The preceding jump
and caller waits matter. A predicted final tile requires the actual
aliased position, service progress, obstruction/destination state and
alignment; a scene command list alone does not prove it. The native
source path is not a0..63 clamping rule.

## Working-layout copies, F100 and ordered exits

**Confirmed:** `code/common/maps/mapload.asm:LoadMapBlocksAndLayout`
loads blocks and decompresses the layout, then scans eight-byte flag
copy records in order. A held flag calls CopyMapBlocks with the packed
source/size/destination words. It subsequently handles chest flags
and, only in battle, crops the battle area. `CopyMapBlocks` in
`code/gameflow/exploration/exploration.asm` addresses64-word rows in
FF0000 working RAM. Thus Map28's held100 applies24 words/48 bytes
from x33..36,y8..13 to x10..13,y16..21 on load; clear100 leaves this
copy unapplied. This is layout data, not a scene or flag grant.

`OpenDoor` in the same file refuses non-exploration CURRENT_BATTLE,
maps candidate pixel coordinates through table_MapOffsetHash, and
scans step rows for exact trigger X/Y. The first match copies and
returns: both selected rows copy4 words/8 bytes from34..35,0..1
to15..16,14..15, play SFX_DOOR_OPEN and request a plane update. It
does not write100 or any completion flag and does not retain a
separate per-door completion register. Marker0400 must cause the
caller to invoke it; merely standing on15,15/16,15 is not sufficient
evidence. Repeated invocation can copy again if the live marker and
caller still admit it. A reload reconstructs the base layout and
applies held flag copies; these step copies are not automatically
replayed from a saved step history.

F100 is the same numeric flag as battle region10, because
BATTLE_REGION_FLAGS_START=90. `battle/battleloop/triggerregions.asm:
TriggerRegionsAndActivateEnemies` can set90+the triggered region index;
this identifies a shared producer, not proof that a preceding battle
actually triggered region10. Fresh `BattleLoop/@Initialize` clears
90..105 inclusively after its before-battle hook. Its F88 suspended
branch bypasses that initialization suffix. In contrast,
ClearMapSetupTempFlags clears256..383; it does not clear100.
Map28's complete setup, callback and scenes neither set nor clear100.
There is no source-backed permanent labyrinth-completion flag here.
An incoming100 may affect load independently of how the labyrinth
was traversed. Natural incoming flag state and save persistence are
**Unknown**.

| Complete ordered warp table | Trigger | Request destination |
| --- | --- | --- |
|Map28 row0|X255 wildcard,Y15|No-scroll Map54,16,41/Up|
|Map28 row1|X255 wildcard,Y31|No-scroll Map41,6,8/Down|
|Map54 row0|X255 wildcard,Y42|No-scroll Map28,16,16/Down|
|Map54 row1|X255 wildcard,Y0|No-scroll Map56,16,48/Up|

`WarpIfSetAtPoint` scans records in authored order, tests the wordFFFF
terminator first, then treats signed-negative X or Y as wildcard.
The selected255 bytes are wildcards; the Y values here are distinct,
so these two rows within each table cannot compete at one coordinate.
First match writes event type1, copies the four type/map/destination
bytes and facing word, sets WARP_SFX and returns. The movement caller
requires marker1000 before calling it. Wildcard X is not a claim that
every tile on the row is a warp tile or passable. The no-scroll form
selects ordinary rebuild transfer through ProcessMapEventType1_Warp,
unwinds to MainLoop and carries requested map/X/Y/facing in d0–d3;
admission runs before the destination exploration setup.

Map41's reciprocal entry is6,7→Map28,16,30/Up; Map28's return lands
at6,8/Down, not the trigger itself. Map54's south return lands at
Map28,16,16/Down, distinct from its row15 trigger. The north endpoint
is only a Map56 interface: `map56/6-warp-events.asm` row0 returns from
wildcardX,Y49 to Map54,16,1/Down. No ascent interior or natural round
trip is claimed. Geometry, marker location, collision and whether
services leave controllable actors at each destination remain
**Unknown**. No layout-decoding observation or screenshot was run.

## Map54 admission, fallback and reentry interface

**Confirmed:** `data/battles/global/battlemapcoords.asm` row40 is
seven bytes at$7B4E: map54,crop0,0,width32,height44,triggerFF,FF.
`MainLoop/@Start` calls SwitchMap then CheckBattle before any
ExplorationLoop. No flag-switched row changes54. CheckBattle at
$799C..$7A36 scans the45 rows in order: map-byte match, unlocked
flag400+index, trigger-X then trigger-Y wildcard/equality checks,
write BATTLE_AREA_X/Y/WIDTH/HEIGHT, test completed500+index, clear
unlocked only if completed is held, and return the selected index.
Map54 occurs only at row40. Its wildcard triggers impose no16,41
position condition. No539,930, sword or Lemon test exists in this seam.

| Supplied selector/flag seam state | Exact branch effects |
| --- | --- |
|Map54,440 clear,540 either|Row40 is skipped before triggers/crop/completion test. With no other54 row, d7=−1; d0 restored. Crop RAM is not written by this row. MainLoop calls exploration.|
|Map54,440 held,540 clear|Write crop0,0,32,44; retain440; return d7=40 and resolved d0=54. MainLoop passes40 in d1 to j_BattleLoop.|
|Map54,440 held,540 held|Write the same crop first; clear440; still return40 this invocation. Completion is not a no-battle return. A later identical check with440 now clear returns−1 unless something rearms it.|
|Different map or failed non-wildcard trigger on another row|Continue scanning. The row40 facts do not authorize admission on another map.|

The success stack pop is `move.w (sp)+,d1` followed by restoration
of saved d1–d6; d0 remains the resolved map. The failure pop restores
d0. If supplied d0.b=MAP_CURRENT=FF, CheckBattle reads CURRENT_MAP;
this convenience is separate from the concrete warp request54.
The crop writes before540 testing are observable intermediate state,
not proof that layout loading has already happened.

Known440 producer: `map59/mapsetups/s6_initfunction.asm:
ms_map59_InitFunction`, after clear999 admits the five Nazca scene
calls, sets999, writes EGRESS_MAP=MAP_DOJO45, then sets436,440,442,443
in that order. Held999 skips this suffix. This is the accepted
[Nazca handoff](./nazca-ship-shootdown-and-crash-handoff.md), not an
extra labyrinth unlock. No natural implication from539/930/sword/
membership to440/540 is asserted. Battle40 completion/result producers
and its F88 behavior remain with the later battle owner.

Map54 has no msMap row in the complete MapSetups table and no local
setup pointer table. This does **not** mean missing map support:
`data/maps/entries.asm:Map54` binds its own map data.
GetCurrentMapSetup scans to the map-table−1 sentinel and returns
ms_Void, a wordFFFF. GetMapSetupEntityList checks that word and
returns the sentinel itself rather than dereferencing six pointers.
InitializeMapEntities still initializes ordinary followers and then
sees FF immediately, ending NPC reads and declaring physical0.
RunMapSetupInitFunction checks the sentinel and skips the indirect
init call. Zone/entity/item/description dispatchers also have their
own void guards. This is a concrete fallback/return path, not an
assumption that a missing setup automatically dereferences safely.

For a supplied no-battle Map54 entry, ExplorationLoop installs
CURRENT_MAP54/CURRENT_BATTLE=FF, initializes those entities at16,41/Up,
clears256..383, sets80, loads Map54, installs base VInts and skips
the void init callback. It then reaches WaitForEvent in its ordinary
exploration loop. Source control flow reaches that loop; usable input,
terrain admission, rendering and natural arrival are **Unknown**.
When a battle is selected, stop at j_BattleLoop and its before-battle
opening interface; do not substitute the fallback population for the
battle's scene population.

The complete Map54 local record surfaces in `data/maps/entries/map54/`
are:

| File / address | Exact records before endWord |
| --- | --- |
|2-areas `$B71B8..$B71D8`|One area: main0,0..31,43; second foreground0,0/background38,30; parallax256,256/128,128; both autoscroll0,0; type255; MUSIC_BATTLE_THEME_1.|
|3-flag-events `$B71D8..$B71EA`|F387 source11,53,size13×8,destination7,18; then F388 source0,52,size10×7,destination13,10.|
|4-step-events `$B71EA..$B71EC`|No records.|
|5-roof-events `$B71EC..$B71EE`|No records.|
|6-warp-events `$B71EE..$B7200`|Both ordered rows listed above.|
|7-chest-items `$B7200..$B7202`|No records.|
|8-other-items `$B7202..$B7204`|No records.|

Held387 reapplies104 words/208 bytes; held388 reapplies70 words/140
bytes, in that order on every LoadMapBlocksAndLayout before any crop.
Their destination rectangles7..19,18..25 and13..22,10..16 do not
overlap; source rectangles are stored off the visible area. They are
not new admission gates. Neither is cleared by ClearMapSetupTempFlags
256..383 or the fresh battle-region90..105 loop. Current incoming
values matter to reentry; tile-change scenes that produce them are
outside this slice. This paragraph describes consumers only, not a
claim about when the battle sets them or persistent save behavior.

## Actual compiler, Content, Domain, Application and adapter boundaries

**Confirmed (code inspection and one lowering only):**
`local/issue1156/observe.py` registers the existing Map28 setup files
and requests Map28_DefaultZoneEvent,cs_5F3A2,cs_5F3C4 once. All three
requests returned without an exception. The complete retained result
has four programs: default callback9 instructions, cs_5F3A2 seven,
cs_5F3C4 eight, return_5F394 one. eas_Init14 and eas_Jump9 actions
contain no native action. Seven callback instructions remain native,
in order: trap,dc.w,dc.l,clr.w,jsr,tst.b,bne.s. The remaining callback
ops call cs_5F3A2 with activateEntities=true and jump to return_5F394;
native bne does not create a typed branch to the left script. That
script is present only because it was requested separately. There
is no supported facing dispatch obtained by acknowledging those
natives. Full native source locations and any exception field are
retained, without retries or compiler changes.

| Actual owner | Expressible shape and present boundary |
| --- | --- |
|[preparation](../../src/sf2tool/remake_exploration_content.py), [maps](../../src/sf2tool/remake_exploration_maps.py), [programs](../../src/sf2tool/remake_exploration_programs.py)|Selected source files/programs, tables, ordered warps, source-zone marker1400, typed motion/wait/sound can be prepared. Event maps expose layout events; unadmitted destinations/setup/scroll and flag-layout branches use explicit native frontiers. Non-event maps without a setup route retain a frontier. Adding54 to eventMaps would require a setup binding that the route-absent path does not provide; it does not synthesize the original ms_Void caller lifecycle. No complete Map28/54 world preparation was run.|
|[ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [PrivateExplorationReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateExplorationReader.cs)|Validate selected maps, routes, sources, action installations and source-zone records. native-call becomes UnsupportedInstruction/UnsupportedEntityAction. Shape admission is not service implementation.|
|[PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [PrivateWorldEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateWorldEncounterReader.cs)|Pinned private encounter cases are1/2/3, otherwise refusal; additional encounter selection is bounded. Battle40 is not admitted by adding a map54 row.|
|[WorkingMapLayout](../../remake/src/Sf2.Remake.Domain/Maps/WorkingMapLayout.cs), [OriginalMapTraversal](../../remake/src/Sf2.Remake.Domain/Maps/OriginalMapTraversal.cs), [MapSetupSelection](../../remake/src/Sf2.Remake.Domain/Maps/MapSetupSelection.cs), [EntityMotion](../../remake/src/Sf2.Remake.Domain/Maps/EntityMotion.cs)|Working-layout block copies, traversal, setup selection and motion primitives exist. They consume admitted layout/state, not these source documents; no labyrinth solution or native callback support follows.|
|[MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs)|Candidate/obstruction, marker match, step copy, ordered first event and source-zone caller lifecycle exist. Doors require0400 and declared rows. This does not link the native default callback or prove original view/physical/alias state.|
|[MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Select admitted battle routes before rebuild where source entry policy applies; explicit completed-battle policy, flag writes, palette/map/setup validation and ordinary transfer exist. They do not execute the assembly table or automatically discover40. Missing binding/refusal remains explicit.|
|[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Native instruction raises program-opcode Unsupported before advancing. Typed script installations, waits and relative motion exist; relative targets outside0..63 raise entity-motion-boundary, without clamping. This finite admitted boundary differs from assuming native16-bit coordinate behavior.|
|[ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [capability owner](../../remake/docs/capability-status.md)|Projected entity/sound/view/presentation consumers exist. No selected labyrinth materials, native services, map54 battle or usable-control observation was run. Rendering code is not evidence about the original.|

**Inferred:** these joins describe a coherent future content contract
from the accepted entrance through conditional Battle40 selection and
the explicit exploration fallback. Existing typed pieces may be reused
by separately scoped implementation; this research does not establish
a runnable labyrinth or Battle40.

## Reproduction, retained failures and stopping point

The ignored direct recipe checks the pinned selected source/table/
caller/alias/branch/width/order relationships, full short scenes,
the one lowering receipt, current consumers, navigation, UTF-8/LF,
links and exact two-path/private publication boundary. It is a compact
transparent check, not a maintained extractor/schema/fixture or tests
of a verification program. Use a new receipt name:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1156/review.py <configured-source-disasm-root> --receipt local/issue1156/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

Applicable ADR0004 source/prose/provenance/date/width/selector/branch/
order/consumer/adversarial checks apply. New maintained parser/schema/
fixture/helper tests and H3 callback acceptance are N/A. Normal verify
is **SKIPPED** by explicit Issue documentation-only scope, not PASS.
No full/old-comparison/SDK/Godot/emulator/H3/H4/screenshot run. Exact
committed planner, actual scoped CI, resources, failures and frozen
head/tree belong in the handoff; independent acceptance remains pending.

Preserve #1154's original direct/main-gate/CI passes that missed the
two P2 prose facts, its focused autoFacing/HP-guard correction and
CI39716514790; #1152 PRIMARY/Mover corrections; earlier no-clamp
correction and aftermath natives; Map53 UnboundLocalError/zero partial
import; twelve completed old comparisons; F88/native/queue/reload/
scroll/natural-time limits. None becomes interrupted, repaired or
rerun because the next accepted commit has a different SHA. The
previous final Project status mismatch was a coordination race,
not a failed evidence gate.

**Unknown:** natural entry/marker/facing/alias/flag/membership state,
collision and decoded geometry, actual jump/diagonal displacement,
service and VInt progress, interrupted/interleaved callbacks, request
headroom, live camera/view/entity state, graphics/text/audio/input/
DMA completion, source/ROM equality beyond accepted evidence, save
persistence and usable control. These are limits, not an acquisition
or repair queue. Stop before Battle40 opening or at the no-battle
exploration branch. Freeze the pushed Draft candidate for main-gate;
no merge, cleanup or next Issue.
