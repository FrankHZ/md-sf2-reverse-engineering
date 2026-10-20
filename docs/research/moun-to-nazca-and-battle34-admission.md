# Moun to the Nazca region and Battle34 admission

Continue with [Battle34 initialization and the Cameela/Geshp opening](./battle34-initialization-and-opening.md).

Evidence date: 2026-10-20. **Confirmed (static):** Moun's three surface
returns explicitly request Map74. That request can select Battle27 before
Battle32; exploration is conditional. The west-edge scroll into Map75 loads
new blocks/layout, including held-F999 copies, while retaining the field
population and skipping setup init and battle selection. All four Nazca
entrances request Map55 `(30,30)` Up. Held434 selects Battle34 even with534
held; clear434 selects exploration. **Unknown:** natural traversal, actual
incoming state, completed transition/presentation and usable control. These
are admission contracts, not a playable travel route.

## Agreement and evidence route

Standard rigor for [Issue #1098](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1098)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053).
Mutable request bytes, scroll versus fresh-load lifetimes, ordered battle
selection and independent flag producers justify a connected state account.
Accepted base is `7edce5331d4fe069532e10379c4562d819266302`. Own this document
and navigation only in the accepted
[Moun town owner](./moun-town-painter-and-golem-arm.md); no shared index,
schema, fixture, design or implementation changes and no stacked dependency.
The outcome supports later route/content integration without prescribing it.

Reuse the [Map74/Pacalon departure owner](./pacalon-departure-to-battle27-admission.md),
[Frayja producer owner](./pacalon-return-frayja-and-battle31-admission.md),
[Moun underground owner](./moun-underground-gyan-and-zynk.md),
[request production](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md),
[common maps](./common-maps.md) and [common scripting](./common-scripting.md).
The accepted owners retain their full catalogues; this slice directly checks
the selected rows and immediate consumers. Stop at MainLoop's Map55
exploration selection or Battle34 admission branch, **before** Map55 init,
zones/ship scripts, NewBattle, Battle34 opening/initialization/activation,
combat or outcomes. Any admitted27/32 is likewise an outbound dependency.
Dojo45 and Roft-region76 are exit handoffs only.

Original paths below are relative to `disasm/` at clean read-only
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Constants come from `sf2enums.asm`/`sf2const.asm`; Fnnn denotes flag nnn,
ordinals are zero-based and address intervals half-open. `mapNN/` means
`data/maps/entries/mapNN/`, and `map/` in script paths abbreviates
`code/common/scripting/map/`. The registered USA ROM identity remains SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1 equality or runtime observation is claimed.

Reuse the unchanged 18,285,979-byte init/script pair with
`local/issue1005/pair.txt` provenance. Selected source/table/consumer reads
are linear, budgeted below one minute/512MiB per audit with small independent
ignored `local/issue1098/` receipts. No pair regeneration, layout extraction,
new environment/platform, runtime/SDK/Godot/emulator, screenshot, old
comparison or broad suite. A material contradiction or supporting-tool
expansion requires reassessment; Unknowns alone do not authorize acquisition.

## Moun departure and the conditional Map74 load

**Confirmed:** `map31/6-warp-events.asm` `$AB6C4..$AB6EE` contains the two
underground rows followed by these three surface rows. Their order and
eight-byte shape are checked against `sf2mapmacros.asm`:

| Row / address | Source X,Y | Request map / X,Y / facing | Mode |
| --- | --- | --- | --- |
|2 /$AB6D4|0,255|74 /2,25 /Down3|No-scroll0|
|3 /$AB6DC|31,255|74 /5,25 /Down3|No-scroll0|
|4 /$AB6E4|255,38|74 /5,25 /Down3|No-scroll0|

The ordinary producer is `entityscriptengine_2.asm:esc02_controlCharacter`
under `code/common/scripting/entity/`. Movement/obstruction and loaded
layout-kind admission surround its `$1000` call to
`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint`. That helper
sign-extends the supplied candidate coordinate words, divides by384, checks
theFFFF terminator before matching, and treats **any negative source byte**
as wildcard. It compares other source low bytes, selecting the first row.
Thus255 means a wildcard here, not destination inheritance. These source
conditions do not prove any boundary tile has the required layout kind or
is naturally walkable. Raft/Caravan have separate control callers.

The first match writes event **word**1, then the type/map/destination-X/Y
**longword** at PARAM1..4, then facing plus zero padding as a **word** at
PARAM5, then WARP_SFX. This is one replaceable slot, not a queue or atomic
transaction. No match leaves it unchanged. WaitForEvent prioritizes pending
events over A/C and skips control-script installation for nonzero pending.
ProcessMapEvent clears the stored event word before dispatching its read type.

`explorationfunctions_2.asm:ProcessMapEventType1_Warp` sees PARAM1=0,
submits/clears WARP_SFX, idles physical0 and removes the exploration return
address. It clears d0–d4 low words, reads PARAM2=74, then samples actual main
X/Y by signed division and actual facing via UpdatePlayerPosFromMapEvent.
Nonnegative PARAM3/4 replace only the sampled coordinate **low bytes**;
PARAM5 supplies facing, PARAM1 supplies d4. With ordinary small sampled
coordinates the outgoing seeds are74 `(2,25)` or `(5,25)` Down. Changed
sampled high bytes survive; this is not copying the trigger coordinate or
moving the actor at request production. Sound/idle/sampling and subsequent
payload reads permit changed/interrupted requests to have different results.

At `code/gameflow/mainloop.asm:MainLoop/@Start`, SwitchMap precedes
CheckBattle. `code/common/maps/mapinit_0.asm:SwitchMap` scans ordered
six-byte source-map/flag/replacement rows. The complete
`data/maps/global/flagswitchedmaps.asm` contains only3/F640→4,
71/F530→78 and58/F999→59. None substitutes31/74/75/55. Its58/F999 row does
not replace Map75; that map's F999 layout work is separate.

`code/common/maps/getbattle.asm:CheckBattle` saves d1–d6/a0 and the original
map operand. CURRENT255 resolves from live CURRENT_MAP; explicit74 stays74.
It scans all45 seven-byte rows of
`data/battles/global/battlemapcoords.asm` `$7A36..$7B71`. The only74 rows are:

| Battle / address | Crop X,Y,width,height | Trigger X,Y | Unlock / completed |
| --- | --- | --- | --- |
|27 /$7AF3|2,16,32,48|255,255|427 /527|
|32 /$7B16|2,16,32,48|255,255|432 /532|

Map equality, held unlock400+index, then trigger low-byte equality or exact
FF wildcard are the admission predicates. Crop containment is not tested.
On the first match it writes the four crop bytes, checks completion100 above
unlock and, if held, clears **that unlock**; it still returns the same battle
on this call. Therefore held427 wins over432 regardless of527/532. With427
clear, held432 selects32 even with532 held. Both clear returns d7=−1,
restores original d0 and writes no crop. No success or failure writes
CURRENT_BATTLE. MainLoop passes selected d7 in d1 to j_BattleLoop; stop there.

The existing [Map74 zone](./pacalon-departure-to-battle27-admission.md#callback-order-and-partial-reload-overwrite)
is a separate producer, not an automatic part of arriving or scrolling.
At supplied zone Y61:527 clear sets427, posts longword000100FF then sets
428/429/430; otherwise811 held/532 clear sets432 then posts that reload;
other branches do no writes. Retained427 can still outrank a newly set432.
This does not write434, and no zone has to run before the west warp.

Only after the explicit74 scan returns no battle does ExplorationLoop's
fresh-map path run: clear pending; subtract20000 from STEP_COUNTER with
negative clamp; heal living/immortal allies; request/wait for fade;
write CURRENT_MAP74 and CURRENT_BATTLE255; load tilesets; select setup and
reconstruct entities/mappings/followers/walking scratch; load sprites;
clear temporary256–383 and set80. Then sprite counters, full LoadMap,
base VInts and selected init run, followed by conditional music/fade and
the event/control loop. The [Map74 owner](./pacalon-departure-to-battle27-admission.md#map74-setup-resources-and-zone-admission)
retains its single unflagged setup, empty fixed list, RTS init, profile and
remaining exits. Empty fixed list does not mean no main/followers/raft.
PLAYER_TYPE and supplied membership/HP/classes/follower/Caravan state retain
their independent meanings; a map request does not normalize them.

## West scroll to Map75 and reverse scroll

**Confirmed:** `map74/6-warp-events.asm` row7 `$C36BA..$C36C2` is source
`(0,255)`, scroll Left, destination75 `(62,255)`, recorded facing Left2
and zero padding. `warpScroll LEFT` emits `$10+2=$12`. It is reached only
after the actual warp caller admits the loaded cell. Earlier rows include
two identical south-scroll rows; the first wins there. The west row does
not imply a route from either Moun seed through intervening terrain.

The nonzero-PARAM1 branch of ProcessMapEventType1_Warp:

1. Idles physical0, reads PARAM2 and writes CURRENT_MAP75. Its special
   71/F530→78 substitution does not affect intact75; it does not call
   SwitchMap.
2. Calls `code/common/maps/mapload.asm:ProcessMapTransition`. This requests
   selected tileset1/3/4 transfers, loads destination blocks/layout, applies
   flag copies and existing item/crop processing, reads/scales the first
   area rectangle and dispatches ApplyOverworldMapTransition from the then
   current PARAM1 masked with3. Intact12 selects Left; zero is Right in
   this inner direction table, not a fresh no-scroll redispatch.
3. Reads PARAM3 as a **byte into returned d0**, without first clearing d0.w.
   Nonnegative62 multiplies the resulting whole word by384 and writes its
   low word to main X/XDEST. Precisely, with return word R the source is
   `((R & FF00) | 3E) * 384 mod 65536`; a clean high byte yields62 tiles.
   Do not silently discard a supplied high byte. Negative X instead skips
   both writes. It then **clears d0.w**, reads PARAM4, and negativeFF skips
   Y/YDEST independently, preserving their actual post-transition words.
4. Samples actual main coordinates/facing and calls DeclareRaftEntity, then
   returns to the existing exploration loop. PARAM5 is not applied. Left is
   the transition direction/request facing; it is not an enforced main
   facing. The retained Y is consumption-time state, not a cached trigger Y.

The Left transition updates plane X positions/counters, rebuilds plane
layout columns and uses sub_29A2 to adjust VDP sprite projections; its
DMA/VInt calls do not prove rendered completion or wall time. It does not
translate every entity's world coordinates into the destination map.
DeclareRaftEntity supplies the separate positional effects: numeric mode2
returns immediately; mode1 skips follower relocation; other modes move the
currently listed physical followers to the sampled main coordinates and
destinations. Scripts/aliases/list order stay live. F64 and RAFT_MAP/X/Y own
physical raft31 declaration/removal; clear64 skips that repair. Logical
FOLLOWER_B31 is not physical raft31. The accepted owner retains the shared
follower flags, HP/class and alias rules.

There is no MainLoop return, CheckBattle, healing, counter subtraction,
temporary clear, CURRENT_BATTLE reset, setup population rebuild or init call
in this scroll. It also leaves WARP_SFX for a later consumer. Main's idle
action and the explicit follower/raft mutations mean population retention
does not mean all actor fields are unchanged. GetCurrentMapSetup can select
Map75 callbacks afterward without rebuilding its declared population.

This is a new blocks/layout load, **not** LoadMap's CURRENT reload path.
ProcessMapTransition does not call LoadMapArea, so full area music/layer/
parallax/autoscroll/animation and roof initialization are not refreshed by
this route, despite reading its rectangle. The declared75 and74 profiles
happen to match in several fields; that does not supply a missing call or
repair stale metadata. LoadMapBlocksAndLayout still uses actual
CURRENT_BATTLE: ordinary255 avoids crop; stale non255 crops with the supplied
BATTLE_AREA values after flag/item work. No implicit battle32/34 crop follows
from the destination name.

Map75 row6 `$C4914..$C491C` is source `(63,255)`, scroll Right `$10`, map74
`(1,255)`, recorded facing Right0. It takes the same retained-population
branch, Right transition, byte-X/whole-word multiplication and inherited Y
rules. It does not scan74 for27/32 or run74 init. A later explicit74 or
CURRENT reload can do other work; that is a distinct request. Reversing this
edge is not an inverse reconstruction of the old actors/layout lifetime.

## Map75 declarations and the F999 layout lifetime

**Confirmed:** `data/maps/entries.asm` binds Map75s0_Blocks and
Map75s1_Layout plus every table below. All selected records are retained:

| Source / interval | Complete declaration |
| --- | --- |
|`00-tilesets.asm` `$C4880..$C4886`|Palette0; tilesets101,102,109,110,105.|
|`2-areas.asm` `$C48AE..$C48CE`|One area0,0 through63,63; secondary foreground/background0,0; both parallax256,256; both autoscroll0,0; main type254; MUSIC_MAIN_THEME; terminator.|
|`3-flag-events.asm` `$C48CE..$C48E0`|F999 copy56,59 size4×5→12,17; F999 copy60,59 size4×5→44,15; terminator.|
|`4-step-events.asm` `$C48E0..$C48E2`|Terminator only.|
|`5-roof-events.asm` `$C48E2..$C48E4`|Terminator only.|
|`6-warp-events.asm` `$C48E4..$C491E`|Seven ordered eight-byte rows, below; terminator.|
|`7-chest-items.asm` `$C491E..$C4920`|Terminator only.|
|`8-other-items.asm` `$C4920..$C4922`|Terminator only.|
|`9-animations.asm` `$C4F2E..$C4F44`|mapAnimation111,96; mapAnimEntry0,48,$350,20; mapAnimEntry48,48,$350,20; terminator. These are raw word operands, not measured animation timing.|

`data/maps/mapsetups.asm` selects only ms_map75, with no flag variant.
The retained init route joins sourceOrder127, pointer$509D8 to init$50A08.
The six-long pointer table `$509D8..$509F0` orders entities, entity events,
zones, descriptions, item events, init. Physical source placement differs
from pointer order:

| Pointer offset / target | Interval / full selected content |
| --- | --- |
|0 /ms_map75_Entities|`$509F0..$509F2`: msEntitiesEnd.|
|4 /ms_map75_EntityEvents|`$509F8..$509FE`: default→Map75_DefaultEntityEvent, RTS.|
|8 /ms_map75_ZoneEvents|`$509F2..$509F8`: default→Map75_DefaultZoneEvent, RTS.|
|12 /ms_map75_AreaDescriptions|`$509FE..$50A00`: RTS.|
|16 /ms_map75_Section5|`$50A00..$50A08`: default→Map75_DefaultItemEvent0, RTS.|
|20 /ms_map75_InitFunction|`$50A08..$50A0A`: RTS.|

There is no selected resident, explicit zone, description/item script or
local434/999 write. Default RTS callbacks are still selected by their real
wrappers; absence of content rows does not suppress control/layout producers.
No local script lowering is needed to establish these bodies. A hypothetical
fresh exploration75 load would rebuild ordinary main/followers/raft and run
this RTS init; the west scroll does neither. No battle-map row selects75.

LoadMapBlocksAndLayout reads each flag **word**, checksFFFF first, calls
j_CheckFlag, and only for held999 supplies packed source/size/destination
words to CopyMapBlocks; it advances eight bytes and tests the second row
independently. Each rectangle is20 block words, not20 ROM bytes. Normal
stable999 applies both copies in source order. A flag change/interruption
between checks can leave a prefix or differing row selection. Changing999
while loaded does not retroactively copy or undo blocks. Reloading blocks/
layout, including scrolling back into75, decompresses the base and samples
flags again; a LoadMap CURRENT reload bypasses this work and retains the
modified layout. Item table work follows the copies, then optional stale
battle crop. The separate global58/F999 map substitution has no part here.

The flag's source comment names a later ship shootdown; this slice establishes
only the two conditional copies, not that scene, flag producer, visible
geography, collision consequences or natural access to an entrance. Decoding
the compressed layout and proving actual walking are outside this scope.

## Four Nazca entrances and the Map55 branch

**Confirmed:** Map75's complete warp table is:

| Row / address | Source X,Y | Mode | Destination / X,Y / recorded facing |
| --- | --- | --- | --- |
|0 /$C48E4|47,16|No-scroll0|55 /30,30 /Up1|
|1 /$C48EC|46,17|No-scroll0|55 /30,30 /Up1|
|2 /$C48F4|46,18|No-scroll0|55 /30,30 /Up1|
|3 /$C48FC|45,19|No-scroll0|55 /30,30 /Up1|
|4 /$C4904|2,27|No-scroll0|Dojo45 /13,43 /Up1|
|5 /$C490C|0,255|Scroll Left12|Roft-region76 /62,255 /Left2|
|6 /$C4914|63,255|Scroll Right10|North Parmecia74 /1,255 /Right0|

The mode numbers12/10 in the last rows are hexadecimal. All seven rows have
zero facing padding. Four coordinate rows are alternatives, not four
successive movements or four independent route types. F999/434/534 do not
filter this table. Layout/collision/mode, actual producer reach and first
matching row remain separate conditions. Dojo/Roft are outbound dependencies;
their interiors, recruitment and later travel are not investigated here.

Each intact first-four request follows the no-scroll consumer already
described: event1/zero mode, explicit55, destination low bytes30/30 and
Up1, with sampled coordinate high bytes retained. SwitchMap leaves55 alone.
CheckBattle's **only**55 row is index34/BATTLE_VERSUS_CAMEELA at
`$7B24..$7B2B`: crop `(0,0,32,32)`, trigger `(255,255)`, unlock434 and
completion534. Other held unlocks cannot select their different-map rows.
Neither entering30,30 nor being inside the32×32 crop is a predicate; even
changed supplied coordinate words do not defeat these wildcard triggers.

| Actual state at the55 scan | Ordered result / stopping boundary |
| --- | --- |
|434 clear, either534|No matching battle; d7=−1, restore explicit d0=55, no crop write. MainLoop chooses j_ExplorationLoop. Stop before its Map55 load/setup/init.|
|434 held,534 clear|Write crop0,0,32,32; d0=55, d7=34; preserve434. MainLoop moves34 to d1 and calls j_BattleLoop. Stop before that consumer.|
|434 held,534 held|Write the same crop, clear434, still return34 and select the battle call on this scan.534 is preserved. A later independent55 scan without another unlock then has no match.|

CheckBattle does not set CURRENT_MAP55 or CURRENT_BATTLE34, does not write
534, heal/rebuild actors, run Map55 scripts or establish NewBattle state.
The source branch selects a downstream consumer; no Battle34 opening or
exploration state is delivered by these rows alone. `alt_MainLoopEntry`
after an actual battle bypasses CheckBattle and has a different boundary;
it is not this outgoing warp's ordinary @Start route.

## Where434 comes from, and independent changed state

**Confirmed:** the accepted
[Frayja producer](./pacalon-return-frayja-and-battle31-admission.md#independent-state-lifetimes-and-interrupted-prefixes)
is directly rechecked in `map36/mapsetups/s2_entityevents_530.asm` and
`scripts.asm:cs_5DA28`. Map36_212_EntityEvent0 returns immediately with811
held; otherwise it calls cs_5DA28, then sets811, then82. The scene's tail
follows Frayja22→FOLLOWER_B31 with offset2, then setStoryFlag31,33,34 in that
order, then end. `map/mapscriptengine_2.asm:csc13_setStoryFlag` reads a
**word**, adds BATTLE_UNLOCKED_FLAGS_START400 and calls j_SetFlag. Thus the
reached prefix writes431,433,434, not31/33/34, completion531/533/534 or party
membership. Shared flags are masked by1023; these indices do not wrap.

The Frayja owner retains the caller, face/text/list-rebuild/follower/HP/alias
details. In particular follow's high-byte Bowie-HP guard can skip following
while later unlocks still run.434 is before script return and callback811/82,
so interrupted scenes may hold434 without those later flags; held811 does
not repair a supplied missing434. Neither533,830 nor76 produces or implies
434 here. Moun population restoration, Zynk following and Battle33 completion
are separate authorities. No selected75 callback sets any battle unlock.

| Changed state / interruption | Required interpretation |
| --- | --- |
|427/432 or527/532 change before the explicit74 scan|Use the values at each check;27 precedes32, precompleted still admits once while clearing unlock. Do not promise exploration74.|
|West or reverse scroll with held27/32/34 unlocks|No battle scan in the scroll branch. Later independently posted no-scroll requests have their own admission.|
|PARAM1 changes after entering the scroll branch|ProcessMapTransition rereads direction bits; no immutable request or automatic redispatch.|
|Map/coordinates/facing/pending event replaced|Use the consumed fields and live sample at each read. A later writer can post a new event after the pre-dispatch clear; earlier actor/map/layout effects are not rolled back.|
|Population/HP/classes/follower flags/mode change|Scroll retains aliases/list/scripts; fresh load reconstructs. Explicit DeclareRaftEntity effects do not imply successful walking or a full rebuild.|
|999 changes while loaded or between copy checks|No continuous layout refresh; copies occur at actual block-load checks. CURRENT reload retains layout; a new block load resamples.|
|CURRENT_BATTLE is stale on scroll|Existing crop and later area rules can differ; no reset to255 or inferred34.|
|434 is clear with533/830/76 held, or811 already held|No inferred unlock or repair. Intact55 request selects exploration at this boundary.|
|434/534 change before/during55 scan|Unlock and completion are separate reads. A precompleted match admits34 and clears434; a change after a read does not retroactively change it.|
|Interruption after crop or unlock clear|Reached mutations remain even if the battle consumer never starts. No completion/persistence guarantee.|

## Relation to current remake consumers

**Confirmed (current code, separate from original evidence):** the existing
[map preparation](../../src/sf2tool/remake_exploration_maps.py),
[route preparation](../../src/sf2tool/remake_exploration_battles.py) and
[Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)
can represent selected maps/events/profiles/ordered battle routes in a
prepared provenance-bound world. No such Map74/75/55 world or new session is
prepared or executed here; production does not read this prose or the retained
research pair as a runtime content source.

| Concern / actual consumer | Existing mechanism and remaining boundary |
| --- | --- |
|Warp forms: map preparation; [ExplorationDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/ExplorationDefinition.cs)|Every source row is retained. Only selected-destination, zero-scroll, non-retaining coordinates below64 become ordinary warps; others become reached warp-frontier programs.74→75,75→74 and75→76 scrolling remain frontiers even if both maps are selected. Four75→55 rows can be ordinary rebuild warps only with the selected destination and actual bindings.|
|Natural reach: [MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs), [OriginalMapTraversal](../../remake/src/Sf2.Remake.Domain/Maps/OriginalMapTraversal.cs)|Candidate/obstruction/door-copy/layout marker and first event matching exist. Coordinates alone do not import collision data, original control mode, physical alias or mutable request lifetime; current prepared events are not the original RAM slot.|
|Scroll versus fresh load: [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|Modes are Rebuild and same-map Preserve; Preserve rejects another map. Ordinary transfers have fade/load services; LoadScene retains actors but is a separate script context. None is an admitted horizontal overworld-scroll implementation. Preserve/fade/scene-load cannot silently stand in for this transition.|
|Setup/population/layout: MapTransfer.Build, Content reader, [WorkingMapLayout](../../remake/src/Sf2.Remake.Domain/Maps/WorkingMapLayout.cs)|Preparation's eventMaps path imports ordered flag copies; Build applies held flag copies before entry-position admission and population/roof work. Without eventMaps selected flag copies retain an explicit native frontier. This can represent both F999 rectangles but does not reproduce their per-row live rereads during a scroll, loaded metadata lifetime, stale battle crop or retained actors. Setup is selected at load; original later wrappers reselect live setup.|
|Ordered admission: [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [SF2 story policy](../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2StoryPolicy.cs)|Map routes preserve supplied array order, unlock/completed flags and before/start/load programs. The compiler iterates selection.battles order; it does not independently restore original27-before32 ordering. SF2 policy enters a completed battle only with PrivateDefinitions present, and Select clears its unlock; authored policy differs. NewBattle/flag88 has a separate suspended-entry refusal. Original admission equivalence therefore needs that admitted context and the correct ordered content. No prepared34 encounter/roster/crop/route is established by this research.|
|World and flag bindings: route preparation, [PrivateExplorationReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateExplorationReader.cs)|compile_routes admits only wildcard-triggered selected battles, emits400/500/450+index bindings and requires selected before/start program compilation. It is not a way to establish34 support without investigating those excluded downstream programs. Actual434 history and ordered74 routes must be carried into the prepared world, not inferred from a narrative endpoint.|
|Presentation: [Godot exploration presentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Consumes actual layouts, visuals, entity and logical-view state. Its rendering resources/services do not prove original plane-column/DMA/VInt scroll behavior, selected animation111, inherited area metadata, completed travel or usable control. No adapter/runtime observation is claimed.|

No selected Map75 script needs an in-memory lowering probe: all selected
callbacks/init are directly checked RTS. Forcing compilation of Battle34
hooks would cross this slice's stopping boundary. **Inferred:** prepared map,
event, flag-copy and ordered-route shapes can support later bounded content
integration after the declared scroll, state and downstream gaps are resolved.
This is not acceptance of that future implementation.

## Reproduction and remaining Unknowns

After loading the current ignored configuration in the same process:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1098/review.py <configured-source-disasm-root> --committed
git diff --check origin/main HEAD
uv run sf2 verify plan --base origin/main --head HEAD
```

The ignored direct recipe checks selected source/macro/constant identities,
all route rows, complete75 declarations/setup, request widths/order/branch
polarity, both scroll directions, flag-copy/crop lifetime, full battle table,
Frayja434 producer and actual current consumers. It records distinct receipts
without regenerating the pair. Review changed-state cases above, complete
two-path diff, navigation-only predecessor, UTF8/LF, relative links/anchors,
tables and private boundary. Apply ADR0004 adversarial review proportionally;
no maintained parser/schema/fixture/test is added. Record clean committed
base/head/tree/planner and actual exact-head scoped CI in the PR/handoff.
Normal/full suites remain NOT RUN under explicit documentation-only scope.

Preserve1096 diagnostics, including its own word/FFFF shop draft/audit error
and withdrawn misattribution: accepted Tristan length-prefixed prose was
correct. Preserve1094/1092/1088/1090 corrections and all twelve old completed
comparison failures in their existing owners/receipts. They are not new
failures, interrupted work or a queue to rerun. New checks do not replace them.

**Unknown:** naturally reached Moun exits, west/reverse scroll and all four
Nazca entrances; selected collision/layout cells; actual coordinates/high
bytes/facing, flags, HP/classes/membership/followers/Caravan/raft, aliases,
population, player mode, pending request and battle/area state; transition,
DMA/sprite/audio/animation/camera/fade/input completion, timing, interruption,
usable control and saved persistence. Map55 load/init/zone/ship programs and
Battle34 NewBattle/opening/initialization/activation/combat/outcomes remain
outbound dependencies, as do Dojo/Roft interiors. None is automatically a
runtime acquisition queue. Done is a clean committed/pushed frozen Draft PR,
scoped evidence/results and successful report to independent main-gate, then
stopped writer/process; acceptance and merge remain there.
