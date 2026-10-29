# Granseal overworld return and Battle38 admission

Continue with [Battle38 initialization and Geshp opening](./battle38-initialization-and-opening.md).

Evidence date: 2026-10-21. **Confirmed (static):** Map77's south scroll
loads Map66 while retaining the live population and exploration call;
it does not scan for battles. A subsequently invoked Map66 y9/y10 zone
callback needs F537 held and F538 clear to set F438 and post a reload.
That request can reach a separate ordered CheckBattle scan. Four older
Map66 rows precede38; any earlier matching unlock wins, including a
completed row on that scan. Row38's area17,0,32,32 is an output crop,
not a containment predicate. Existing early Map66 support does not bind
late-story content or Battle38. Natural travel, completed services,
usable control and persistence remain **Unknown**.

## Agreement, evidence and budget

Standard rigor for [Issue #1142](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1142)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
join accepted [Battle37 aftermath](./battle37-outcomes-and-lemon-aftermath.md)
at base `cd0cacfb7484018f62bdcb27b3f7efc141514314` through the
Map77/66 connection, complete local Map66 setup/content interfaces,
zone/request lifetime and independent Battle38 admission. This new owner
and navigation only in the aftermath owner are the exact tracked scope.
No shared index/registry, parser, schema, fixture, design or engine edit;
no competing writer or stacked dependency. Stop at MainLoop's selected
38 call interface, before its opening/initialization. Dwarven Village29/30,
blacksmith services, neighbouring town/revisit bodies, later Lemon
recruitment and Battle39 progression remain deferred branches.

Reuse [early Map66/Battle02→03](../grans-opening/battle02-to-battle03-content-route.md),
[regional scroll and Map77](./roft-to-galam-and-battle36-admission.md),
[retained-scroll services](./moun-to-nazca-and-battle34-admission.md),
[map setup/scripting](../../world/common-scripting.md), [map content](../../world/map-content.md),
[request production](../../world/map-event-request-state.md),
[request consumption](../../world/map-event-request-consumption.md),
[random-battle state](../../world/map-event-random-battle-state.md) and
[common maps](../../world/common-maps.md). Those owners retain common mechanisms;
this document checks the selected operands, caller lifetimes, changed
states and actual production consumers. Early chronology is not reused
as a late-story state snapshot.

Original paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`map/` and `entity/` abbreviate `code/common/scripting/` children;
`exploration/` means `code/gameflow/exploration/`. sf2const.asm,
sf2enums.asm, sf2mapmacros.asm and sf2mapsetupmacros.asm own byte layout,
constants and table encoding. Fnnn means flag nnn, ordinals are zero-based,
address ranges half-open. The accepted USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new source/ROM equality or runtime observation is claimed.

Minimum evidence is selected source/setup/tables/helpers, actual consumers,
and the existing immutable18,285,979-byte init/script pair with
`local/issue1005/pair.txt` provenance. Direct reads and one pair load are
linear in bounded inputs. Small unique receipts belong in `local/issue1142/`;
each command targets one minute/512MiB, peak memory unmeasured. One existing
compiler observation resolves whether the explicit reload callbacks have
typed consumers, separately from its special random-battle lowering.
It does not import a map, compile battle hooks or run a program. No new
acquisition/platform/helper tests, full-map reprocessing, normal/full/old
comparison suites, SDK/Godot/emulator/H3/H4 or screenshots. Reassess changed
prerequisites or material contradictions before expanding work.

## South scroll, reverse edge and live map context

**Confirmed:** complete Map77 warp table row3 at `$C5D90..$C5D98`
is source255,63 → Down-scroll$13 /Map66 /255,1 /Down3.
Map66 row9 at `$BBDC6..$BBDCE` is source255,0 → Up-scroll$11
/Map77 /255,62 /Up1. Map77's earlier wildcard-X rows at Y33/34 do not
match Y63; at63,63 row3 precedes the east-scroll row4. Map66's nine
earlier rows have other Y values, so the north edge is not shadowed
at Y0. These declarations prove neither walkability nor natural reach.

The actual movement caller is
`entity/entityscriptengine_2.asm:esc02_controlCharacter`. After its
candidate/entity-obstruction checks, a masked working-layout marker$1000
allows `exploration/exploration.asm:WarpIfSetAtPoint`. Signed pixel
coordinates divide by384. The first matching eight-byte row wins;
signed-negative source coordinate bytes are wildcards. The producer
writes TYPE.w=1, PARAM1..4 as a longword, PARAM5 plus padding as a
word, and WARP_SFX.w. Neither edge has a story/party/HP/Lemon predicate.
A coordinate tuple without its marker/caller is insufficient.

`explorationfunctions_2.asm:WaitForEvent` gives pending requests priority
over A/C. ProcessMapEvent clears TYPE.w before dispatch. With the intact
nonzero PARAM1, ProcessMapEventType1_Warp idles physical0, changes
CURRENT_MAP.b and calls `code/common/maps/mapload.asm:ProcessMapTransition`.
The map71/F530 exception does not apply. Destination66 and77 both declare
palette0/tilesets101..105. Transition reloads tilesets1/3/4 (101/103/104),
blocks and working layout, applies held flag copies/chest blocks, and
would apply an existing battle crop if CURRENT_BATTLE were notFF.
It scales destination's first area and selects direction from the
then-live PARAM1&3 through plane/VInt/DMA scrolling helpers.

With intact packets, destination X=255 skips both X/XDEST writes:
each retains its post-transition value, not necessarily the same value.
Y explicitly clears its word, then sets both Y/YDEST to1×384=384
southward, or62×384=23808 northward. The shared explicit-X high-byte
hazard does not apply to these wildcard-X edges. PARAM5 is not applied
as facing in the scroll branch; posting Down3/Up1 does not turn the actor.
Idle replaces an action pointer without clearing travel/velocities.
The helper later samples actual signed coordinates/facing and calls
DeclareRaftEntity, whose mode2 early return, mode1 follower bypass,
other-mode follower relocation and independent F64/raft state are
retained by the accepted scroll owner. Later control-script installation
and execution remain separate steps.

The caller remains inside ExplorationLoop: no stack unwind, SwitchMap,
CheckBattle, fresh population/aliases/follower-list rebuild, mapsprite
loading, temp256..383 clear, F80 initialization, setup init, healing or
step-counter entry adjustment. LoadMapArea's fresh music/palette/full
area/parallax/autoscroll/animation/roof work is not executed. No scroll
SFX consumption/clear is added. Population/action/temporary/resource/
CURRENT_BATTLE state is retained subject to the named helper mutations;
empty destination NPC data does not remove carried actors. Subsequent
GetCurrentMapSetup reads CURRENT_MAP66, selecting66 callbacks even with
population carried from77. There is no cached Map77 callback authority.

Contrast a no-scroll request: its handler consumes/clears WARP_SFX, idles0,
unwinds exploration and samples signed actual position/384. Nonnegative
destination bytes replace only the corresponding low coordinate byte;
negative bytes retain that axis. Facing comes from PARAM5. MainLoop runs
SwitchMap then CheckBattle. The complete switched-map table has only
3/F640→4,71/F530→78,58/F999→59, no66/77 override. If no battle is selected,
an explicit map entry rebuilds population and loads sprites/temp/F80;
MAP_CURRENT entry retains population/temp/working layout but still performs
common exploration heal/step and area/roof/init services. Neither is
the regional scroll lifetime. The preceding Battle37 return enters
exploration after SwitchMap without a new CheckBattle, as its owner records.

## Complete Map66 local surfaces and immediate exits

**Confirmed:** `data/maps/mapsetups.asm` has one `msMap66,ms_map66`
selection and no conditional setup rows before its end marker.
GetCurrentMapSetup uses the live current-map byte and last held override
when overrides exist; none exists for66. All six pointers at
`data/maps/entries/map66/mapsetups/pointertable.asm` (`$4FA70..$4FA88`)
are accounted for:

| Surface | Complete selected content |
| --- | --- |
|s1_entities `$4FA88..$4FA8A`|Only msEntitiesEnd: zero fixed NPCs. Ordinary leader/followers and retained population are independent.|
|s2_entityevents `$4FA8A..$4FA90`|One default → Map66_DefaultEntityEvent, RTS.|
|s3_zoneevents `$4FA90..$4FAE0`|Y9,10 → Map66_ZoneEvent0; Y28 → Map66_ZoneEvent2; then default. All explicit X bytes255. Callback bodies below.|
|s4_descriptions `$4FAE0..$4FAE2`|ms_map66_AreaDescriptions is RTS.|
|s5_itemevents `$4FAE2..$4FAEA`|One default → Map66_DefaultItemEvent0, RTS; no explicit item-use callback.|
|s6_initfunction `$4FAEA..$4FAEC`|ms_map66_InitFunction is RTS.|

No local dialogue is requested by these empty surfaces or explicit zone
bodies. Default random-battle presentation is a shared helper. Empty
description/item callbacks do not remove generic Search/ground-item,
portrait/window/arrival-wait services in their callers.

| Complete data file under map66 | Content interface, not a natural state |
| --- | --- |
|2-areas `$BBD48..$BBD68`|One area0,0..63,63; secondary foreground/background0,0; both parallax256,256; both autoscroll0,0; type254; MUSIC_MAIN_THEME; terminator.|
|3-flag-events `$BBD68..$BBD7A`|F508: copy source0,49 size10×15 to28,23. F611: source59,4 size1×1 to31,7. Then terminator.|
|4-step-events /5-roof-events `$BBD7A..$BBD7E`|Each only endWord; no declared step-copy or roof rows.|
|7-chest-items `$BBDD0..$BBDD2`|Only endWord; no chest records.|
|8-other-items `$BBDD2..$BBDD8`|One mapItem30,44 /flag213 /MITHRIL, then endWord. This is neither a chest nor a zone reward.|

LoadMapBlocksAndLayout tests each copy flag and copies source words in
table order. Both copies are considered on scrolling loads as well as
fresh loads; setting a flag alone is not a live recopy call. F508/F611
are independent of537/538/438, and are not assigned assumed values here.
Their working words can affect marker/collision interpretation. The chest
postpass has no66 rows; the ground-item declaration alone proves neither
award nor a reachable Search target. No decoded marker/terrain/collision
join or walking-route computation was performed.

All ten outgoing eight-byte rows in `6-warp-events.asm`:

| Ordinal / source X,Y | Connection / destination interface |
| --- | --- |
|0 /40,7|No-scroll0 → MAGIC_TUNNEL_HUB,37,57,Up1.|
|1 /6,5|No-scroll0 → HAWEL_HOUSE,6,11,Up1.|
|2 /30,32|No-scroll0 → GRANSEAL,1,27,Right0.|
|3 /19,23|No-scroll0 → YEEL,21,42,Up1.|
|4 /18,22|No-scroll0 → YEEL,7,1,Down3.|
|5 /31,8|No-scroll0 → GALAM_DRAWBRIDGE,9,28,Up1.|
|6 /32,5|No-scroll0 → GALAM_CASTLE,45,1,Down3.|
|7 /35,28|No-scroll0 → DEVILS_HEAD,6,14,Up1.|
|8 /33,43|No-scroll0 → DEVILS_HEAD,23,14,Up1.|
|9 /255,0|Up-scroll$11 → OVERWORLD_GALAM_KINGDOM77,255,62,Up1.|

Destination names are sf2enums MAP_ symbols without the prefix. These
rows have no537/538/party predicate. Zone row Y28 and warp35,28 do not
both fire merely because coordinates overlap: the movement marker's
masked value chooses$1400 zone or$1000 warp. Neighbouring interiors,
their setup switching and return bodies are not entered by this slice.

## Invoked zones, reload bytes and competing default producer

**Confirmed:** esc02 first checks entity obstruction. After a$0400 door
operation it rereads the working marker. Caravan$3800 and raft$3C00
branches have separate mode/flag gates; warp$1000 and zone$1400 are
distinct. At loc_5294 a$1400 marker posts TYPE.w=6, candidate X.w at
PARAM1 and candidate Y.w at PARAM3, before later tile-collision/motion
admission. Thus a reached marker need not imply completed movement.
ProcessMapEventType6_ZoneEvent applies Init to0, reads those two words,
then calls RunMapSetupZoneEvent. It does not directly scan battles.

That dispatcher rereads current setup, compares byte X/Y in table order
(only FF is its wildcard), and calls the first match or default. Callback
return does not try the next row. It closes portrait/text and requests
VInt/arrival wait. The explicit y9/y10 callback is therefore not followed
by CheckRandomBattle when its flag gate fails.

| Invoked callback / state | Writes on that invocation |
| --- | --- |
|Y9 or10;537 clear, any538|Immediate RTS. Does not clear a preheld438 or another pending request.|
|Y9 or10;537 held,538 held|Immediate RTS; preheld438 remains.|
|Y9 or10;537 held,538 clear|set438, then long MAP_EVENT_RELOADMAP at TYPE; repeated invocation can repost even when438 was already held.|
|Y28;538 clear or539 held|Immediate RTS, leaving preheld439 alone.|
|Y28;538 held,539 clear|set439, same long reload; no438 write and no Battle39 body entered.|
|Other coordinates at an invoked zone|Default loads battle3 and calls CheckRandomBattle; no537/538/438 gate.|

MAP_EVENT_TYPE is `$FFA84A`; PARAM1..5 are consecutive bytes
`$FFA84C..$FFA850`. Big-endian long$000100FF writes TYPE.w=1,
PARAM1=0, PARAM2=FF/MAP_CURRENT only. It neither writes PARAM3/4/5
nor WARP_SFX. For the uninterrupted movement→type6→explicit callback
path, PARAM3/4 still contain the candidate Y word: Y9 gives00,09,
Y10 gives00,0A, Y28 gives00,1C. The no-scroll consumer therefore
overrides low X with0 and low Y with9/10/28; it does **not** snapshot
candidate X as the later entry X. High coordinate bytes still come
from signed actual-position division. PARAM5 is retained request facing,
not necessarily current facing; if the south-scroll packet remains its
last writer it is Down3 even though that scroll never applied it.
WARP_SFX similarly retains earlier state until no-scroll consumption.
A direct callback call or intervening writer can supply other bytes.

After callback/waits return, the poll consumes type1, unwinds exploration
and reaches MainLoop's SwitchMap→CheckBattle. There is no fresh66 init
or population rebuild before that scan. If no row matches, CURRENT
exploration performs its own later entry work. Request state and live map,
flags, selectors, actor positions and services remain mutable; a posted
request is not an atomic promise of the next selected battle.

The complete CheckRandomBattle body in
`map/mapsetupsfunctions_1.asm:$47856..$478C6` is already owned by the
early route. Clear503 requests without step/RNG tests. Held503 with
nonzero STEP_COUNTER makes no request; held503/step0 draws bound8:
zero requests, nonzero draws bound4 and stores that result+2, no request.
A request sets403, long$000100FF, step30000, then requests view wait,
boost sound and flash. Its d1/d6/d7 are restored, not a returned success
code. No branch sets438 or clears competing unlocks. The same retained
PARAM3..5 rule applies. A default-selected late visit can thus produce
an older battle request; a held503 does not permanently disable it.

## Independent ordered CheckBattle and stopping interface

**Confirmed:** `code/common/maps/getbattle.asm:CheckBattle` (`$799C`)
scans all45 seven-byte rows in `data/battles/global/battlemapcoords.asm`
(`$7A36..$7B71`), ascending index. Matching uses the map byte, held
400+index, and trigger X/Y bytes (FF wildcard). No area containment,
537 prerequisite, party/HP, layout marker or numeric-story-order check
is added here. All and only Map66 rows are:

| Order / battle / row address | Output X,Y,width,height | Trigger X,Y / unlock / completed |
| --- | --- | --- |
|0 /2 /`$7A44`|16,19,22,18|255,255 /402 /502|
|1 /3 /`$7A4B`|0,0,22,30|255,255 /403 /503|
|2 /4 /`$7A52`|0,0,22,30|255,255 /404 /504|
|3 /6 /`$7A60`|21,2,20,32|255,255 /406 /506|
|4 /38 /`$7B40`|17,0,32,32|255,255 /438 /538|
|5 /39 /`$7B47`|17,20,23,32|255,255 /439 /539|

The first match writes four BATTLE_AREA bytes, checks500+index and,
if held, clears that row's unlock **while still returning that index**.
Completion does not continue the scan on the same call. MAP_CURRENT
resolves through live CURRENT_MAP; success leaves resolved d0=66 and
returns d7=index. d1–d6/a0 are restored. No match restores supplied d0,
returns d7=-1 and leaves prior area bytes untouched. The routine itself
does not write CURRENT_BATTLE, run an opening or initialize combatants.

| Supplied state at this scan | Result |
| --- | --- |
|Map66, only438 held,538 clear|38; crop17,0,32,32;438 retained. F537 is not consulted.|
|Same,538 held|38 on this call,438 cleared. A second unchanged scan no longer selects38.|
|Map66,438 absent; no other matching unlock|−1, even with537 held; prior crop remains.|
|Map66,402 and438 held;502 held|2 wins,402 clears,438 remains. Another scan may then reach38.|
|Map66,403/503 and438 held;402 absent|3 wins and403 clears. Default request can create this competing403.|
|Map66,404 or406 held; earlier unlocks absent|4 or6 wins before38, respectively.|
|Map66,438 and439 held; earlier unlocks absent|38 wins before39. With438 absent,439 can select39; stop at that interface.|
|Only438 held; supplied0,9;250,250; or17,32|All select38 despite being outside its17≤X<49,0≤Y<32 crop. Triggers are wildcard.|
|MAP_CURRENT/current66 versus current77; only438 held|38 versus−1. Explicit66 can select38 even if current-map byte differs.|

MainLoop compares d7 to−1 and, on success, copies it to d1 and calls
j_BattleLoop. This is the selected38 stopping boundary. F88's suspended
versus ordinary battle path, load/crop application,38 roster/opening/
activation/actions/outcomes and any39 progression are not established
by this returned index. Changed coordinates do not create a38 trigger
failure, but changed map/flags/prior rows can change the result.

## Existing compiler and actual consumer limits

**Confirmed about tooling:** one unchanged OriginalPrograms observation
registered the six Map66 defining files and compiled only the selected
zone and empty local callbacks in0.0036s. Zone0/2 each have four root
instructions: two typed branch-flag tests, set438/439, then native-call
move.l. Each has its one-instruction return continuation. The default
has typed source-random-battle3 then end; four empty handlers/init each
lower to end. No map, route, encounter or host was imported/executed.
The default's source-random-battle-presentation target is not emitted by
this isolated observation; compile_routes normally supplies it. Bare
lowering is not a self-contained validated Content package.

| Actual owner / consumer | Representable boundary and selected gap |
| --- | --- |
|[Map importer](../../../../src/sf2tool/remake_exploration_maps.py)|For event-enabled66, code can retain ordered source-zone rows/default, Init actions, empty fixed NPCs, population, two flag copies, empty doors/roofs and RTS init. Structural no-scroll rows require selected destinations; retained/scroll edges become warp-frontier programs. No item-use/chest/ground-item award binding is supplied here. This is a direct code deduction, not a new66 import receipt.|
|[Compiler](../../../../src/sf2tool/remake_exploration_programs.py), [Content reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Native-call becomes UnsupportedInstruction and stops at program-opcode. Clear537 or held538 returns; admitted Zone0 can publish438 before stopping at missing reload write. Zone2 analogously publishes439. No guaranteed rollback or successful admission follows.|
|[MapEventDispatcher](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs), [MapTransfer](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|Supplied marker and ordered event matching, block copies, arrival/window waits and fresh/same-map transfer exist. Cross-map Preserve refuses preserve-other-map; same-map Preserve and ReloadScene are not native regional scrolling or the mutable$000100FF packet. Existing retained actor/resource/service lifetime is not proven for77→66.|
|[SourceRandomBattleRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SourceRandomBattleRunner.cs)|Default3 has actual completion/step/RNG/unlock/request consumers and a bound presentation path when prepared. Consume selects the first bound route using a boolean request; it does not reproduce mutable native parameter bytes, coordinate overrides or the explicit Zone0/2 reload.|
|[Route preparation](../../../../src/sf2tool/remake_exploration_battles.py), [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Ordered routes/unlock/completion policy and crop origin are expressible. Preparation rejects nonwildcard triggers and compiles explicitly selected before/start hooks; no38 hooks were compiled here. Runtime selects the first bound route and SourceStory policy decides completed admission; no automatic complete45-row native table or38 binding follows.|
|[PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [capability owner](../../../../remake/docs/capability-status.md)|Pinned reader admits1/2/3; encounter preparation admits only2/3. Neither admits38. Existing early66/02–03 support supplies no selected38 terrain/definitions/material/world/route/scene or late-story state/presentation acceptance.|

No consumer correction is included. Representable flags/tables and a
successful compiler return do not establish a playable Granseal return.

## Reproduction, preservation and Unknowns

The ignored direct recipe checks selected complete tables, shared caller/
handler ordering and widths, changed-state examples, actual consumers,
source pin/input metadata, two-path/navigation-only scope, links/UTF8/LF
and private publication. Tracked sources and prose own durable findings;
ignored scripts/receipts are coordination state. Load current selections
in the same launching PowerShell process and use a unique receipt:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1142/review.py <configured-source-disasm-root> --receipt local/issue1142/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The source argument ends in disasm. The single compiler observation is
retained at `local/issue1142/observe-01.json`; do not repeat it merely
for a new commit identity. Apply ADR0004's semantic/provenance/polarity/
width/order/selector/private checklist. No maintained parser/schema/
fixture/helper tests or H3 callbacks changed. Normal `uv run sf2 verify`
is **SKIPPED** under explicit documentation scope; full/old/runtime gates
remain excluded. Record clean-head planner and actual exact-head CI in
the frozen handoff; a proposed command is not an executed pass.

Preserve [Battle37's sole lowering and y64 runner guard deduction](./battle37-outcomes-and-lemon-aftermath.md#one-lowering-and-actual-current-consumers),
[Map53's event-enabled import UnboundLocalError/zero partial programs](./roft-to-galam-and-battle36-admission.md#actual-lowering-and-current-consumer-boundary),
and the linked twelve completed old private comparison failures,
corrected alias/collision/follower/accounting evidence and prior receipts.
They were neither repaired, rerun, erased nor described as interrupted.

**Unknown:** naturally reached Map77 exit/Map66 markers, local ground
item or neighbouring entrances; actual flags508/611/213/537/538/438 and
older unlocks, pending requests/parameters/SFX, selectors, party/HP/class/
aliases/followers/transport/RNG/steps/position/facing; working layout,
collision and materials; interleaving and completed scroll/plane/DMA/
sprite/view/window/text/audio/flash/map/init/arrival/input services,
original VInt/hardware timing, usable control and save persistence.
No original-runtime observation, route proof, late-story38 content binding
or engine-ready claim is made. Unknowns do not authorize acquisition.

Freeze a clean committed/pushed Draft PR with exact head/tree, actual
direct checks/planner/CI, residual gaps and retained ignored handoff.
Main-gate owns independent acceptance/merge. Stop writer/processes at
handoff; no merge, cleanup/archive or next slice.
