# Tristan Caves and shrine exterior admission

Evidence date: 2026-10-20. **Confirmed (static):** explicit Map35 entry uses
the global void setup while still constructing ordinary exploration. Its map
data declares three chest items, no other items, and twelve ordered no-scroll
warps: four to Tristan15, six CURRENT reloads and two to shrine exterior0.
CURRENT preserves actors and working layout through a different entry arm.
Map0 can select Battle29 under live F429; it is not Battle28. **Inferred:**
source names identify these as Tristan Caves and Mitula shrine exterior.
**Unknown:** natural transit, collision routes, successful services/control,
presentation/timing and persistence. No remake readiness is established.

Continue the admitted encounter in [Battle29 initialization and opening](./battle29-initialization-and-opening.md).

## Agreement and provenance

Standard rigor for [Issue #1056](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1056)
under [data epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
organize one transit map and its conditional destination admission. Explicit
entry and reload have different lifetimes, so complete selectors and changed
state matter. Accepted dependency is [Tristan entry, interactions and exits](./tristan-entry-interactions-and-exits.md),
#1054/PR1055 on base `da7b48843f6554f48dc5893d99791749b1dd8598`.
This document owns the findings; the predecessor gains one navigation link.
No index, schema, fixture, tool, remake, Chinese or Layer B change is required.

Begin at the four admitted explicit35 ExplorationLoop seeds below. Inventory
Map35 resources and actual consumers; end outbound selection immediately before
the destination ExplorationLoop/BattleLoop body. The CURRENT35 entry arm is
examined to explain the internal lifecycle. Map0 initialization, narrative,
later battle execution, collision search, runtime collection and asset extraction
are excluded. UI692 is unrelated/unconsumed. The consumer survey describes
accepted code, never evidence about the original.

Source paths are relative to `disasm/` at pinned
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Retained source is read-only and clean. Existing provenance binds USA retail
ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1/runtime equality is claimed. `sf2enums.asm`/`sf2const.asm` own
constants/RAM. Fnnn means flag nnn; rows are zero-based; ranges are half-open.
Short `map/`/`entity/` paths mean `code/common/scripting/`.

Reuse the unchanged 18,285,979-byte map-init/script pair and accepted provenance.
Its `routeJoins` has no Map35 row and its `pointerTableJoins` no ms_Void table;
do not synthesize six pointers or regenerate missing map-specific init records.
The existing setup fixture does express the missing-map result ms_Void (its
selected example is Map32), and the canonical import represents absent setup
routes separately. Selected source/fixture checks are linear in bounded tables
and bodies, below one minute/512 MiB, with small local receipts and no acquisition.
Contradictory semantics or material scope growth require reassessment.

## Explicit entry, void setup and ordinary population

**Confirmed:** the predecessor's four town exits admit explicit35, d4=0,
facing Up1, with nominal coordinate bytes `(15,30)`, `(16,30)`, `(24,15)`,
`(4,14)`. No-scroll consumption retains high coordinate-word bytes; these
are not guaranteed normalized coordinates or walked endpoints.

`code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop` clears
MAP_EVENT_TYPE.w, subtracts 20000 from STEP_COUNTER.w (clearing a negative
result), heals living/immortal allies, requests full fade-out and selects view
-1. Its explicit-map arm writes CURRENT_MAP.b=35 and CURRENT_BATTLE.b=255,
loads tilesets, waits for fade, clears VInts, selects setup entities, calls
InitializeMapEntities then LoadEntityMapsprites, clears F256–383 and sets F80.
Sprite counters, view0, LoadMap with saved35/view-coordinate -1, base VInts,
RunMapSetupInitFunction, conditional music/fade-in, SetMoveSfx and WaitForEvent
follow. Entry is not an atomic completed visit; a stopped prefix has only its
reached effects.

The [shared entry owner](./battle26-return-through-pacalon-entry.md) retains
healing details: Peter7/Lemon28 always and other allies with nonzero HP receive
max HP/MP, retain only already-held STUN/POISON/CURSE and recompute stats.
Other dead allies are not universally revived. Persistent chest flags177/178/203,
F429/F529/F530, membership/follower flags, PLAYER_TYPE and pending parameters
are not cleared by ClearMapSetupTempFlags; pending **type** was reset earlier.

The complete `data/maps/mapsetups.asm` has no msMap35 row; there is no Map35
mapsetups directory. `map/mapsetupsfunctions_1.asm:GetCurrentMapSetup` rereads
CURRENT_MAP.b, scans the global table through its $FFFF terminator and returns
`ms_Void: dc.w $FFFF`. For present maps it loads the default, scans all flag
rows through $FFFD, and the last set flag replaces the candidate. On35 no
flag combination manufactures a setup. Every reached helper tests `(a0).w`
before reading offsets; ms_Void is one sentinel word, not an RTS init or a
six-pointer table.

Changing CURRENT_MAP between allocation and a later helper selects that live
map's setup against the existing actors; it does not itself rebuild them.
Changing follower flags after allocation likewise does not reallocate actors.

| Helper | Void guard and remaining local work |
| --- | --- |
|GetMapSetupEntityList, reached through j_GetMapSetupEntities in s07_jumpinterface.asm|Returns a0 still pointing to $FFFF; it does not dereference an entity pointer.|
|RunMapSetupInitFunction|Skips the offset20 load/call; restores registers and returns. The ExplorationLoop caller still did all preceding work.|
|RunMapSetupZoneEvent|Skips table scan, callback and window closure; still waits one VInt, then WaitForEntityToStopMoving with main0. ProcessMapEventType6 first applies main's init action and samples pending coordinate words.|
|RunMapSetupItemEvent|Before selection clears speech SFX.w, masks item d4.w, stores d2.b to EVENT_RELATIVE_POSITION and sets d6=0. Void skips callback/window closure; still waits a VInt and main motion.|
|RunMapSetupEntityEvent|Stores d2.b to EVENT_RELATIVE_POSITION before selection; void skips table, portrait, facing, callback, text closure and VInt suppression/restoration.|
|RunMapSetupAreaDescription|Clears d7, skips offset12 call and tests d7=0 for the caller's no-description result.|

`map/mapfunctions.asm:InitializeMapEntities` branches into loc_440E2. It
multiplies supplied X/Y words by384, ClearEntities resets actors/mappings/
walking scratch, then InitializeFollowerEntities allocates ordinary flag
followers before examining the returned entity-list byte. $FF immediately
ends fixed rows: **zero Map35 fixed NPC/ally declarations**, not zero actors.
Main physical0 is then declared with Bowie's current sprite and eas_Idle;
the follower list ends with $FF, and sub_44404 handles raft construction.
Map35 is neither follower-excluded map nor an overworld; F65 does not choose
the overworld/Caravan follower prefix. The [follower owner](./battle26-return-through-pacalon-entry.md#conditional-map02-population-and-first-eventcontrol-read)
retains complete ordered ordinary flags, alias overwrites and capacity limits.
No local town population/conversation is implied by those live followers.

Explicit construction with PLAYER_TYPE=2 replaces main's sprite, stands/hides
followers and declares raft31 at `(64,64)`; other modes use F64 plus matching
RAFT_MAP and saved raft coordinates. Supplied mode is not normalized by void
setup. WaitForEvent first reads pending type: zero installs control/view0;
its control selector uses numeric0 for character,1 for raft and other nonzero
for Caravan. That numeric dispatch differs from enum-valued raft construction.
An event written after entry reset can bypass control installation. Service
completion and actual usable input remain Unknown.

## Map resources, layer255 and pickups

**Confirmed:** `data/maps/entries.asm:Map35` has a six-byte header at
`$ADA66..$ADA6C`, followed by ten long resource slots through `$ADA94`:
blocks, layout, areas, flag/step/roof/warp events, chest/other items and absent
animation `$FFFFFFFF`. The binary blockset/layout references are Map35's own;
no decoded payload is copied here. `00-tilesets.asm` declares palette4 and
tilesets `[8,255,255,255,42]`. LoadMapTilesets and explicit LoadMap use signed
byte tests to skip absent slots2–4, not tileset255 loads.

The sole area `2-areas.asm`, `$ADA94..$ADAB4`, declares main bounds
`(0,0)..(63,35)`, both foreground/background origins `(0,0)`, both parallaxes
256/256, both autoscrolls0, layer type255 and MUSIC_BATTLE_THEME_1. LoadMap
selects by actual viewed-actor words with inclusive bounds, falling back to
the first area when none matches. Music naming does not admit a battle.

LoadMapArea copies layer255 as a **byte**. VInt_UpdateViewData and
VInt_UpdateSprites use `tst.b`/nonzero to choose Plane A coordinates;
UpdateVdpPlaneA uses nonzero for palette mask `$2000`, while Plane B clears
that mask. OpenChest uses nonzero to set plane-update bit0. Thus255 is a
nonzero main-foreground selector at these consumers, not a missing area,
layer index255 or an absent map. Zero offsets do not change that branch.
Exact VDP output, priority and timing are not observed here.

| Resource | Address interval / complete records |
| --- | --- |
|Flag events|`$ADAB4..$ADAB6`: endWord only, zero persistent block copies.|
|Step events|`$ADAB6..$ADAB8`: endWord only, zero door block copies.|
|Roof events|`$ADAB8..$ADABA`: endWord only, zero roof copies.|
|Warps|`$ADABA..$ADB1C`: twelve eight-byte rows plus two-byte terminator; complete order below.|
|Chest items|`$ADB1C..$ADB2A`: three four-byte rows plus two-byte terminator.|
|Other items|`$ADB2A..$ADB2C`: endWord only, zero non-chest item rows.|
|Animation|Absent header pointer, not an invented empty animation program.|

| Chest row / target X,Y | Acquisition flag | ITEM_ identity / value |
| --- | --- | --- |
|0 /3,4|177|PROTECT_MILK /10|
|1 /39,26|178|HEALING_SEED /1|
|2 /20,5|203|HEALING_SEED /1|

These rows are independent of setup scripts. Reuse [search/description priority](../../../design/contracts/world/map-area-description-routing.md)
and [item transaction boundaries](../../world/map-event-item-transactions.md).
CheckArea in explorationfunctions_0.asm uses the tile ahead and actual layout
kind: chest `$1800` precedes vase/barrel/bookshelf/area-item branches, which
precede setup descriptions. Void descriptions do not disable shared searching
or generic fallback text. No Map35 conversations, setup zone/item callbacks
or description rows exist; do not invent them from the cave name.

`exploration.asm:GetChestItem/GetItem` selects CURRENT_MAP's chest pointer,
compares coordinate **bytes**, stops at negative source X and returns the
flag/item plus a masked64x64 layout offset. In normal35 exploration there is
no battle-coordinate adjustment. OpenChest, on a table hit, checks the flag,
returns ITEM_NOTHING if already set, sets it, writes the open block and marks
Plane A for update. Acquisition/inventory/text work remains the caller's
separate continuation; flag/block effects precede its successful completion.
CheckChestItem additionally checks the live chest layout kind. A table row
alone proves neither that CheckArea selects it nor a usable approach.

Explicit LoadMapBlocksAndLayout decodes source blocks/layout, applies all set
flag copies (none here), then all set chest markers: held177/178/203 write
`$D802` at their listed coordinates. CURRENT does not rerun that pass. A changed
chest flag can therefore disagree with the retained working block until an
actual opening or rebuilding call; neither rows nor flags alone reconstruct
the live layout. Interrupted opening/grant prefixes and persistence remain
separate. No private layout/collision search was performed.

## Twelve directed warp requests

**Confirmed:** each row is warpNoScroll0. Source X,Y below are candidate
activation coordinates; destination X,Y/facing are requested bytes, not proof
of standing there. Right0 and Down3 are facing enums.

| Row / source X,Y | Requested map | Destination X,Y / facing |
| --- | --- | --- |
|0 /15,31|Tristan15|18,6 /Down|
|1 /16,31|Tristan15|19,6 /Down|
|2 /24,16|Tristan15|4,10 /Down|
|3 /4,15|Tristan15|7,3 /Down|
|4 /2,11|CURRENT255|2,24 /Right|
|5 /2,24|CURRENT255|2,11 /Right|
|6 /26,26|CURRENT255|58,12 /Right|
|7 /58,12|CURRENT255|26,26 /Right|
|8 /35,14|CURRENT255|39,30 /Down|
|9 /42,30|CURRENT255|24,9 /Down|
|10 /15,22|Mitula shrine exterior0|14,34 /Down|
|11 /16,22|Mitula shrine exterior0|15,34 /Down|

Rows4/5 and6/7 are reciprocal coordinate requests. Row8 does not land on row9's
source, and row9 does not return to row8. Town seeds are adjacent to the four
return triggers, but this proves no traversable connection, inverse edge,
collision-free route or automatic shrine reach.

The [map exploration contract](../../../design/contracts/world/map-exploration.md) owns
shared markers/block-copy meanings. The selected controlled-movement consumers
in entityscriptengine_2.asm test layout kind `$1000` after Caravan/raft/door
handling and before zone/passability. WarpIfSetAtPoint in exploration.asm
signed-divides supplied position words by384 and selects the **live** map's
warp table. It scans in order: word `$FFFF` ends the table, negative source
coordinate bytes wildcard that axis, otherwise low coordinate bytes must match.
Every local source coordinate is nonnegative/exact. A matching coordinate
without the reached marker/caller does not itself request a warp.

On the first match it writes MAP_EVENT_TYPE.w=1 **before** copying the four
type/map/X/Y parameter bytes, then copies facing plus the row's padding byte
as a word, then writes WARP_SFX. It replaces one mutable pending slot, not a
queue; no-match leaves any prior slot unchanged. Interruption between writes
can expose a prefix with old parameters. Another producer, changed current map
or changed actor/slot before consumption changes the result; table facts are
not a frozen route snapshot.

## Conditional consumption, CURRENT lifecycle and destination admission

**Confirmed:** [request consumption](../../world/map-event-request-consumption.md) owns
entry reset/poll/pre-dispatch clear. ProcessMapEvent clears type before its
six-way dispatch. For unchanged no-scroll parameter0, ProcessMapEventType1_Warp
plays/clears WARP_SFX, idles main and unwinds one return frame to MainLoop.
It clears d0–d4 words, reads destination map.b, then UpdatePlayerPosFromMapEvent
samples main's **current signed position words**, divided by384. Nonnegative
destination bytes overwrite only the low d1/d2 bytes; negative bytes retain
sampled words. Every local destination byte is nonnegative, yet preserved high
bytes can survive. Facing is a byte and d4=0. If pending type/parameters change,
the selected event/scroll arm may differ; the table does not force this arm.

MainLoop returns to @Start: SwitchMap then CheckBattle. It does not use the
battle-return alt_MainLoopEntry that bypasses CheckBattle. The complete
flag-switched table has exactly the Granseal/F640, drought-overworld/F530,
underground-room/F999 substitutions; none matches explicit35/15/0 or the
CURRENT argument255. SwitchMap compares supplied **words** and does not resolve
CURRENT; CheckBattle then resolves a low map byte255 from live CURRENT_MAP.
Thus a changed current map on a CURRENT request can change battle selection
without first applying that map's flag-switch row.

CheckBattle scans all45 ordered rows, requires F400+row, and matches trigger
bytes with $FF wildcards. It writes crop bytes before checking F500+row.
Completed matches clear their unlock and **still return that battle in this
call**, rather than continuing the scan. No match returns d7=-1, restores
the original map argument and leaves prior battle-area bytes untouched.
On a CURRENT match d0 retains the resolved map.
The [routing contract](../../../design/contracts/world/map-entry-routing-state.md) retains
the helper-local boundary; MainLoop here supplies the actual next call.

| Intact request / supplied live state | Selector result and stopping call |
| --- | --- |
|Explicit15 or35, arbitrary battle flags|No battle-map row for either map; d7=-1, call j_ExplorationLoop with explicit map. Stop before its body (except the owned35 entry analysis above).|
|CURRENT255 with CURRENT_MAP=35|No battle row; original argument255 restored, d7=-1, call j_ExplorationLoop through its reload arm.|
|Explicit0 with F429 clear, regardless of F529|Only matching map row is29 and it is locked; d7=-1, call j_ExplorationLoop with0.|
|Explicit0 with F429 set and F529 clear|Select29, write crop `(0,0,32,38)`, call j_BattleLoop with d0=0/d1=29.|
|Explicit0 with F429 and F529 set|Write same crop, clear429, still call j_BattleLoop with0/29. A later scan differs after that clear.|
|CURRENT255 with changed CURRENT_MAP/flags/coordinates|Resolve actual current map, scan full ordered table under actual unlocks/triggers/completion flags; no-match retains255, match uses resolved map. No unconditional35/exploration result.|

Row28 is map47/Pangoat Valley Bridge; row29 is map0/
BATTLE_OUTSIDE_MITULA_SHRINE. Other-map unlocks, F530 or battle-themed music
cannot turn Map0 into Battle28. No Map0 init, story flag enabling429, scene,
roster or battle-body execution is inferred or entered here.

For the admitted CURRENT35 reload, ExplorationLoop still resets pending type,
adjusts step counter, heals, fades and temporarily detaches view. It **skips**
CURRENT_MAP/CURRENT_BATTLE assignment, tileset loading, VInts-clear, setup
entity selection/allocation, mapsprite load, temp-flag clear and F80 set.
After fade it calls UpdateMainEntityProperties and DeclareRaftEntity instead.
Main X/Y use signed-low-byte guards then full-word multiplication; facing,
sprite/properties/action are updated. DeclareRaftEntity returns immediately
for mode2, skips follower reposition for mode1, otherwise repositions retained
followers to supplied coordinates; F64/RAFT_MAP then declare or hide raft as
appropriate. Retained actors/mappings are not an immutable actor snapshot.

LoadMap receives255, sign-extends its low byte and takes the current-map path:
InitializeDisplay/scroll reset, fresh area lookup/load, view/window/plane work
and roof evaluation still run, but palette/tileset transfer and source block/
layout reconstruction are skipped. Map35 has no roof rows or animation pointer.
Base VInts, guarded void init, palette-dependent music/fade and WaitForEvent
still follow. CURRENT_BATTLE and temp flags remain supplied live state; if the
former changed from255, LoadMapArea takes battle dimensions even on this path.
An explicit35 request would reconstruct actors/layout and reset battle state
even though the map number is unchanged. Explicit15/0 likewise choose fresh
entry only if the preceding selector chose exploration; their bodies are not
part of this slice. No service completion/control/persistence is claimed.

## Existing representations and consumer gaps

**Confirmed about the accepted repository:** evidence shapes, prepared Content
and running consumers are separate. No Map35/0 package or observation is
prepared here. The [capability owner](../../../../remake/docs/capability-status.md)
remains authoritative for runtime scope.

| Category | Existing representation / actual consumer | Bounded gap or unassessed behavior |
| --- | --- | --- |
|Void/default setup|`map-setup-static-v1.json`/`h2-map-setup-static-fixture.schema.json` preserve missingMapResult and guarded helpers; canonical importer [map_import.py](../../../../src/sf2tool/h2/map_import.py) emits absent setupRoute via `.get`, separate from definitions. The retained init pair has no35 route/pointer row.|No new format needed. `Setup=null`, empty Entities and null OnLoad in [ExplorationDefinition](../../../../remake/src/Sf2.Remake.Application/Content/Scenarios/ExplorationDefinition.cs) can express a subset, but do not reproduce caller effects automatically.|
|Resources/events/items|`map-content-static-v1.json`/`h2-map-content-static-fixture.schema.json` bind corpus/consumer facts; `canonical-map-import-v1.json` and [canonical schema](../../../../schemas/canonical-map-import.schema.json) cover typed resource categories, absent routes/animations and raw flags. These public fixtures summarize the corpus, not a public decoded35 package.|Private Map35 layout/block/tileset/palette material and explicit destination bindings remain unprepared; source rows above were checked directly.|
|Layer255/area|ExplorationViewArea retains byte layer; [ExplorationViewRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs) accepts255 with zero foreground origin,256 parallax and no autoscroll, matching this area. [Content reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) reads prepared view data.|Logical subset is expressible. Its Initialize requires a containing area rather than original first-area fallback. Godot [ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs) selects camera A for nonzero but `hasForeground` uses origin/parallax differences: equal Map35 planes omit A draw passes while background passes remain. Plane/palette/priority binding needs review; actual visual consequences and parity are unassessed.|
|Fresh entry/population/init|ExplorationPopulation, SceneEntities and MapTransfer.Build can build main/followers; EntryFlags and nullable OnLoad express useful subsets. [Existing preparer](../../../../src/sf2tool/remake_exploration_maps.py) emits null setup for an absent route and its final pass replaces that setup frontier with `end`.|Missing-route init is already expressible. Ordinary population/entry flags are emitted only when a population is selected (the no-route path requires outcomeEgressMap); eventMaps expects a real setup/entities/init. No Map35 package or mode/raft/changed-state/service binding is supplied. The ending init program alone does not supply caller effects.|
|CURRENT reload|Preparer maps target255 to selected map with loadMode preserve; reader accepts preserve/rebuild. [MapTransfer.Apply](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs) preserve keeps working layout/actors and moves main, while rebuild applies entry flags and constructs population.|Expressible mode distinction. Preserve requires same-map, active-area/passable destination and selected setup; it does not implement the original full-word/high-byte domain or these DeclareRaftEntity follower/raft effects. It resolves255 during preparation, unlike original live-current resolution. Other heal/fade/music/control services require bounded binding review.|
|Explicit destinations/battle admission|Prepared Warp events and MapTransfer/BattleEntry use maps and ordered battle routes; [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) consults completed-battle policy.|15/0/29 maps/routes/encounter bindings are absent from this work. Original completion-clears-unlock-but-still-admits behavior needs the correct policy; no completed-state readiness is implied. Original word/byte selectors and changed pending slot are not modeled merely by a destination event.|
|Search/chest outcomes|Canonical item tables and original GetItem/OpenChest facts remain evidence. General flags/text/layout can express pieces.|Current ExplorationEventKind has Step/Interact/Warp/SourceZone, no chest/search dispatch; item acquisition/area descriptions remain Unsupported. Required marker/flag/block/grant continuation and inventory/presentation support cannot be replaced with a flag-only event.|

The preparer consumes a provenance-bound canonical input and selected sources;
the [private Content reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateExplorationReader.cs)
consumes prepared world/start documents. Production does not read research
fixtures/disassembly on demand. Godot presents the admitted Application state
and prepared resources; it is not original-game evidence. No preparer, engine,
Godot or emulator was launched to make the survey.

## Reproduction and remaining boundaries

Load `local/private-inputs.ps1` in the same launching process. The ignored
one-off `local/issue1056/review.py` recipe compares all Map35 rows/header,
complete selectors, sentinel/helper guards, caller/lifecycle order, selected
search/request controls, existing fixture/schema relations and concrete
consumer limits. It checks UTF-8/LF, local links, exact owned paths and private
boundaries; it adds no maintained verifier or extraction layer.

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1056/review.py <configured-source-disasm-root>
git diff --check
```

Review complete prose against changed current-map/flags/mode/position/pending/
layout states and interrupted prefixes, then on a clean committed head run
`uv run sf2 verify plan --base origin/main --head HEAD`. Retain actual exact-head
scoped CI in the Issue/PR handoff. Documentation scope skips blanket normal/full
Python, .NET, Godot, H3/H4 suites and tests of review programs. Preserve #1054's
receipts/regex correction and predecessor failure chain, unchanged pair, and
twelve unrelated remake comparison failures; none is rerun or relabeled.

**Unknown:** actual entry words, flags/mode/current map/battle, live pending
slot and interleaving, actors/mappings/capacity, working blocks, natural
reach/approach/collision, completed search/grant/fade/wait/control services,
hardware presentation/timing, save-load persistence, and unassessed consumer
behavior. Destination scene and battle bodies remain outside this boundary.
