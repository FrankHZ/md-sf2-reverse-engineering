# Pacalon departure to Battle27 admission

Evidence date: 2026-10-14. **Confirmed:** castle departure requests town
entry; town departure rebuilds exploration on Map71 or Map78. Their north
scroll loads Map74 while retaining the exploration population. A separately
admitted Map74 zone event can set Battle27's unlock and post a reload request.
Only the subsequent MainLoop battle scan admits a battle, with27 before32.
Neither the scroll nor the callback initializes combat. **Unknown:** natural
reach, actual supplied state, completed transition/rendering, usable control
and persistence.

## Agreement and provenance

Standard rigor for [Issue #1029](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1029)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Mutable requests, retained actors and conditional admission require one
connected state account. Accepted starting base is
`1d62ba1cd233577ddda8275d70ff1b8f9a41572a`. This is the sole tracked write;
there is no shared write or stacked dependency. Parallel synthesis and UI692
supply no original-game evidence.

Consume the accepted [castle return](./pacalon-castle-entry-and-first-visit.md#ordered-stairs-return-request-and-lifecycle),
[town outgoing request](./pacalon-interactions-and-castle-handoff.md#ordered-exits-and-outgoing-mainloop-request),
[entry/lifecycle](./battle26-return-through-pacalon-entry.md),
[request state](../../world/map-event-request-state.md),
[request consumption](../../world/map-event-request-consumption.md),
[common maps](../../world/common-maps.md), [common scripting](../../world/common-scripting.md) and
[random-battle distinction](../../world/map-event-random-battle-state.md).
Stop before BattleLoop initialization for both27 and32. Battle31 is an
excluded town-admission diversion. F530-set castle dialogue/`cs_5DA28`,
Battle27 execution, Battle28–34 progression, services, combat, layout decoding,
runtime acquisition, Chinese synthesis and engine implementation are deferred.

Source paths are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The selected read-only source is clean at that pin. The accepted USA ROM is
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
`sf2enums.asm`/`sf2const.asm` own constants; Fnnn denotes game flag nnn.
Rows are zero-based; address ranges are half-open. Short `map/` and `entity/`
paths below are under `code/common/scripting/`.

Minimum evidence is selected pinned source, existing fixture/schema/consumer
joins and the dependency-valid retained map-init/script pair:18,285,979 bytes
and accepted0.083s comparison, reused without regeneration. Linear selected
reads are budgeted below one minute/512 MiB with small ignored receipts.
No new maintained parser, simulator or acquisition is needed. Replan a
contradiction or supporting-system expansion, not an arbitrary prose limit.

## Castle return, town admission and fresh overworld entry

**Confirmed:** Map36's first warp row is no-scroll source `(255,31)` to
Map02 `(255,1)`, Down3. The accepted consumer returns to MainLoop with explicit
map2, sampled main X retained, Y's low byte replaced with1 and facing3.
Sentinel255 preserves the consumption-time coordinate word, not a saved
trigger coordinate. Nonnegative replacements preserve its high byte. An
interrupted or replaced request must use the actual later bytes.

`code/gameflow/mainloop.asm:MainLoop/@Start` calls SwitchMap then CheckBattle.
SwitchMap has no Map02 replacement. The sole Map02 battle candidate is31:
crop `(0,0,48,32)`, wildcard triggers, unlock431. Clear431 reaches exploration;
held431 admits31 even when531 is held (in that case clearing431 first).
Neither530 nor king dialogue, F810, collection or services is an admission
prerequisite. Stop the Battle31 alternative at its handoff.

With no battle, explicit2 follows
`code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop`:
clear pending event word, subtract20000 from STEP_COUNTER with its signed
word branch/zero fallback, heal living/immortal allies, request fade-out and
set view target-1. It writes CURRENT_MAP2/CURRENT_BATTLE255, loads tilesets,
waits for fade, clears VInts, selects setup entities and rebuilds entities/
mapsprites, clears temporary flags256–383 and sets80. It then initializes
sprite counters, LoadMap, base VInts and the selected init, conditionally
requests music/fade-in, and enters WaitForEvent. This is conditional control
entry, not an observation of usable control or completed display.

F530 currently clear/set selects `ms_map2`/`ms_map2_flag530`; both select
`ms_map2_InitFunction` at$5EAA6, which is RTS. The accepted entry owner owns
their different fixed populations. Fresh ClearEntities resets actor records,
lookups and walking scratch; flag-driven followers are reconstructed, not
carried from the castle. Town2 is not an overworld, so F65 adds no Caravan
prefix there. Reached main declaration uses supplied coordinates/facing and
Bowie's sprite rules; PLAYER_TYPE itself is not reset by this entry. Story,
membership and inventory flags outside the temporary range survive.

Map02 warp row3 at$96A74 requests no-scroll71 `(22,16)`, Down3, from source
`(255,31)`. The three earlier rows request the castle; their precedence and
layout admission remain with the accepted outgoing owner. This south row
has no dialogue, inventory, F810 or king-visit test. Its no-scroll consumer
returns the explicit71 request to MainLoop, not directly to exploration.

SwitchMap's ordered six-byte table includes71/F530→78; it has no reverse
78→71 rule. Thus clear530 retains71 and set530 selects78. Neither map occurs
in any of the45 battle-map rows. CheckBattle returns d7=-1, so explicit71/78
performs the same fresh exploration lifetime, resetting CURRENT_BATTLE255
and discarding the town population. Both are overworlds: F65 can add the
Caravan prefix before ordinary flag followers. Current flags/membership/
HP/class/mode govern the existing declaration consumers; town actors and
castle Frayja displacement are not retained. No new ally joins here.

Neither71 nor78 has a MapSetups record. GetCurrentMapSetup returns `ms_Void`
($FFFF); fixed-list selection gets the sentinel and init invocation is skipped.
Shared main/follower/raft construction still runs. This is setup absence,
not a default populated setup. Both maps use palette0/tilesets101–105,
area `(0,0)..(63,63)`, type254, main-theme music, parallax256/256, zero
autoscroll and zero other-layer origins. Their block/layout resources differ;
identical warp/area declarations do not prove identical walkable terrain.

The rebuild does not reset numeric PLAYER_TYPE. WaitForEvent installs physical0
control only when the pending event word is zero: numeric0 chooses character,
1 chooses `eas_Raft`, other nonzero chooses `eas_Caravan` in
`entityfunctions_2.asm:SetControlledEntityActScript`. These actual branches
must not be silently repaired to agree with the enum names. Pending nonzero
bypasses that installation and wins over the A/C poll.

## Ordered north scroll and retained state

**Confirmed:** `data/maps/entries/map71/6-warp-events.asm` and Map78's equivalent
each contain five eight-byte rows plus endWord. Their first two are the
Pacalon no-scroll entrances. Row2, at$C06A4/$C69F4 respectively, is:

| Source X,Y | Type | Destination map | Destination X,Y | Recorded facing |
| --- | --- | --- | --- | --- |
|255,0|scroll UP = $11|NORTH_PARMECIA74|255,62|Up1 plus zero padding|

Row3 is south scroll to70 `(255,1)` and row4 west scroll to72 `(62,255)`.
These remain side-exit boundaries. First-match order is significant even
though row2's Y0 differs from the earlier town rows' Y15.

`entityscriptengine_2.asm:esc02_controlCharacter` checks the loaded layout's
masked kind$1000 before `WarpIfSetAtPoint` in
`code/gameflow/exploration/exploration.asm`. Movement/obstruction admission
precedes or surrounds that call; a row alone is not a traversable edge.
The warp producer consumes supplied d0/d1 coordinate words (the character
caller has added its movement deltas), sign-extends and divides by384, checks
the$FFFF terminator first, then compares source low bytes. Those candidate
coordinates are distinct from the later consumer's actual-position sample.
Any negative source byte is wildcard here. This is broader than the zone
selector's exact$FF wildcard below. First match posts event word1 at$FFA84A,
type/map/X/Y as a longword at PARAM1..4 ($FFA84C..$FFA850), facing/padding
as a word at$FFA850..$FFA852, and WARP_SFX. No-match leaves pending state
unchanged. These are ordered writes to one mutable slot, not an event queue.
Raft/Caravan use separate warp callers; their control path cannot be inferred
from the ordinary character's route.

ProcessMapEvent clears the stored event word before dispatching its read d0.
`ProcessMapEventType1_Warp` sees nonzero PARAM1 and:

1. Makes physical0 idle (replaces its action pointer), reads PARAM2 and writes
   CURRENT_MAP74. Its special71/F530→78 substitution is irrelevant to an
   intact74 operand; it does not call the generic SwitchMap table.
2. Calls `code/common/maps/mapload.asm:ProcessMapTransition`. That consumer
   requests selected tileset transfers, loads Map74 blocks/layout and reads/
   scales the first area rectangle. `ApplyOverworldMapTransition` reads the
   current PARAM1, masks direction with3 and dispatches Up for$11. The Up
   routine updates plane counters/layout and entity sprite projection via
   sub_29A2, queues DMA and waits for VInts. Those calls are not a measured
   rendering/timing result.
3. Reads PARAM3 as a byte into post-transition d0. Negative$FF skips both
   main X and XDEST writes, preserving their actual values. A changed
   nonnegative byte uses the whole resulting word in multiplication by384;
   its inherited high byte is not implicitly cleared. It then **clears d0.w**,
   reads PARAM4 and, for62, writes62*384 to main Y/YDEST. A negative altered
   Y byte would preserve Y/YDEST independently.
4. Samples actual main X/Y by signed division and actual facing, calls
   DeclareRaftEntity, and returns to the existing exploration loop.

PARAM5 is not applied by this scrolling branch. Up is the record's direction
and facing request, but the actor's actual facing is retained. It also does
not submit/clear WARP_SFX via the no-scroll branch, heal, subtract20000,
clear temporary flags, reset CURRENT_BATTLE, rebuild mappings/fixed entities/
walking scratch, run map setup init, enter MainLoop or scan for a battle.
Map74's setup becomes available to later event selection without being an
entry initializer. Newly changed follower flags do not automatically rebuild
the retained list. Main's action pointer was changed; population retention
does not mean all actor fields are unchanged.

DeclareRaftEntity returns immediately for numeric mode2; mode1 skips follower
relocation. Other modes move each physical ID in the current exploration
follower list to the sampled main coordinates, including its destinations.
Their scripts and mappings remain live. The raft gate requires64; matching
RAFT_MAP declares physical31 from masked raft coordinate bytes, Left/raft
standing semantics, while mismatch clears its lookup and moves it off-map.
Clear64 skips that repair. Logical follower B31 is not physical raft31.

The scroll does not call LoadMapArea: selected area/music/layer metadata and
temporary flags remain from the existing exploration lifetime, apart from
explicit transition mutations. The declared rectangle is consumed without a
full area refresh. LoadMapBlocksAndLayout uses actual CURRENT_BATTLE; ordinary
255 avoids the battle crop, but stale non255 can apply its supplied crop.
Neither metadata nor the source warp proves a continuous decoded path from
town to this edge, to Y61, or to Tristan.

## Map74 setup, resources and zone admission

**Confirmed:** Map74 has one unflagged setup selection expressed as
`msMap 74, ms_map74` in `data/maps/mapsetups.asm`; no530/527 alternative.
Retained `map-init-static.json:routeJoins` sourceOrder126 binds `ms_map74`
at$5096A to `ms_map74_InitFunction` at$509D6. Its six-pointer table orders
entities, entity events, zones, descriptions, item events, init. The fixed
list at$50982 is only `msEntitiesEnd`; init at$509D6..$509D8 is RTS.
These do not erase retained followers or create NPCs during a scroll.
GetCurrentMapSetup reads actual CURRENT_MAP at each call; an altered map
changes event selection even if74 was requested earlier.

The header at$C362E uses palette0 and tilesets101,102,109,110,105.
`data/maps/entries.asm` selects Map74s0_Blocks/Map74s1_Layout. Its one area
at$C365C..$C367C has `(0,0)..(63,63)`, zero other-layer origins, both-layer
parallax256/256, zero autoscroll, main-layer type254 and MUSIC_MAIN_THEME.
Flag/step/roof tables at$C367C/$C367E/$C3680 contain only endWord. They supply
no additional event records; this does not disable the separate entity
layout-kind producer, STEP_COUNTER or setup zone table. On scrolling, the
area rectangle is read but full metadata/roof handling is not run. An explicit
exploration load and a CURRENT reload have their own consumers below.

Map74's exits are boundaries only: rows0/1 request Moun31 at `(1,12)` Right /
`(30,37)` Up; row2 Tristan15 `(31,30)` Up; rows3/4 bridge47 `(1,8)` Right /
`(24,30)` Left. Rows5/6 duplicate south scroll to71 `(255,1)`, Down, from
`(255,63)`; the earlier duplicate wins. Row7 west scroll requests75 `(62,255)`,
Left, from `(0,255)`. F530 can substitute78 for the south71 at consumption.
None of these records establishes arrival or authorizes a destination visit.

The actual ordinary zone producer is the$1400 branch of
`entityscriptengine_2.asm:esc02_controlCharacter`, reached after candidate
movement/overlap and layout lookup. It writes event word6, then the signed
tile quotient of XDEST+d4 as a **word** to PARAM1/2, then YDEST+d5 as a word
to PARAM3/4. d4/d5 are the selected movement deltas, so these are candidate
destination coordinates, not a snapshot of current X/Y. The type write is
first, with no pending guard or atomic payload. PARAM5 and WARP_SFX are
untouched. The subsequent collision/debug/flagsA-bit6 branch can still skip
the movement writes; posting is not evidence that the actor reached the cell.
The continuing movement path sets velocities/travel/destinations and decrements
a positive STEP_COUNTER. Map74's callback does not perform that decrement.

After WaitForEvent selects6, ProcessMapEvent clears the stored type.
`ProcessMapEventType6_ZoneEvent` installs `eas_Init` on physical0, reads
PARAM1/2 and PARAM3/4 as X/Y words, then calls RunMapSetupZoneEvent. The
wrapper resolves the current setup and its zone pointer; it scans four-byte
records, recognizes default marker$FD before coordinate matching, treats
exact$FF as wildcard, compares low bytes otherwise, and adds the signed
relative target word to the table base for the first match.

`map74/mapsetups/s3_zoneevents.asm`, $50984..$509C6, contains row0
`(255,61) → Map74_ZoneEvent0`, then default → Map74_DefaultZoneEvent/RTS.
Thus any supplied X and Y low byte61 selects row0; other Y uses default.
This does not prove every actual X at61 is a zone-kind tile. Missing setup
skips selection. A changed CURRENT_MAP, overwritten coordinates, high bytes,
alternate mode or an externally posted event can select a different path.

After a selected callback returns, the wrapper closes portrait/text, waits
one VInt and waits for physical0's actual X/Y to equal its destinations,
then restores registers and returns. It does not clear a newly posted
pending request. Missing setup bypasses callback/window cleanup but still
does the common VInt/motion wait. Neither a callback return nor the request
write proves this wait completed or usable control resumed.

## Callback order and partial reload overwrite

**Confirmed:** the complete Map74_ZoneEvent0 local branch is:

| State read when reached | Ordered effects |
| --- | --- |
|527 clear|Set427; longword reload write; set428; set429; set430; return.|
|527 set,811 clear|Return without these writes.|
|527 set,811 set,532 set|Return without these writes.|
|527 set,811 set,532 clear|Set432; longword reload write; return.|

The first branch does not consult811/532, and none clears427/432 or writes
527/530/532. Existing unlocks survive the no-write branches. Repeating with527
clear repeats all writes even if427 was already held. F428–430 are unlock
flags, not completed battles. F811's source comment describes Frayja asking
to go to Moun; reading that flag proves neither the deferred castle scene nor
its natural history, rain restoration or later outcomes. F530 is not tested.

`move.l #MAP_EVENT_RELOADMAP,((MAP_EVENT_TYPE-$1000000)).w` uses constant
$000100FF. On this big-endian target the four written bytes are:

| Address / field | Result |
| --- | --- |
|$FFA84A..$FFA84C / event word|$0001 = warp|
|$FFA84C / PARAM1|0 = no-scroll|
|$FFA84D / PARAM2|255 = CURRENT|

It does **not** write PARAM3/4 at$FFA84E/$FFA84F, PARAM5 at$FFA850, the next
padding byte, or WARP_SFX. With the intact ordinary zone producer and Y=61,
PARAM3/4 still contain the word$003D: the later warp interprets them as
destination **X byte0 / Y byte61**, not X/Y sentinels. PARAM5 may still be
the north warp's Up1 if no intervening writer replaced it. That conditional
example is not a freshly specified facing. Altered high Y bytes can change
the later X byte independently of the zone selector's low-byte61 match.

The request replaces any current slot contents; it is not appended. Flags
and request are not one transaction: interruption after427 but before reload
can leave an unlock without this request; interruption after reload but
before428/429/430 can leave only a prefix of those writes. The432 branch has
the analogous flag-before-request boundary. Cleanup/wait can be interrupted
after the entire callback. An intervening writer can replace type or payload
before WaitForEvent/dispatch; no rollback or immutable copy is supplied.

There is no call to CheckRandomBattle, no random draw, STEP_COUNTER reset to
30000, view-scroll wait, boost sound or flash in this callback. Its deterministic
flag branch is distinct from that helper's accepted counter/RNG request path.
Ordinary movement's counter decrement and later exploration subtraction remain
separate consumers, not evidence of a random Battle27 admission rule.

## Reload consumption and ordered battle handoff

**Confirmed:** after callback/cleanup return, ExplorationLoop sets movement
sound and reenters WaitForEvent. An intact new event1 bypasses control-script
installation and wins over A/C input. ProcessMapEvent clears the stored type,
then the zero PARAM1 warp branch submits/clears the current WARP_SFX, idles
main and removes the exploration return address to return to MainLoop.
It clears low words d0–d4, reads PARAM2=255, then samples actual main coordinate
words/facing through UpdatePlayerPosFromMapEvent. Nonnegative PARAM3/4 bytes
replace only the corresponding coordinate **low byte**; negative bytes preserve
the sampled word. PARAM5 replaces facing and PARAM1 supplies d4=0.

For ordinary small coordinates and the untouched zone Y word$003D, the outgoing
seed is CURRENT255 `(0,61)` with retained PARAM5. This is a register request;
it does not itself move the actor to X0. Changed sampled high bytes survive
replacement; altered negative bytes preserve the corresponding sampled word.
The warp reads payload fields at different times around sound/idle/sampling,
so interrupted/replaced payloads must be evaluated as consumed.

MainLoop's ordinary @Start now calls SwitchMap, which has no255 replacement,
then `code/common/maps/getbattle.asm:CheckBattle` ($799C..$7A36). CheckBattle
saves the original map word and coordinate registers, resolves CURRENT255
from actual CURRENT_MAP, and scans all45 seven-byte BattleMapCoordinates
records in source order. Exactly two select74:

| Battle / table row | Crop X,Y,width,height | Trigger X,Y | Unlock / completion |
| --- | --- | --- | --- |
|27 /$7AF3|2,16,32,48|255,255|427 /527|
|32 /$7B16|2,16,32,48|255,255|432 /532|

Map-byte equality and held unlock are required; each trigger matches either
exact low byte or exact255 wildcard. Both rows here are fully wildcard, so
neither the candidate Y61, sampled coordinates, outgoing X0 nor being inside
the crop is required by this scan. The crop is output written only after
admission, not a containment predicate. Other maps' rows are skipped even
when their unlocks are set. F428–430 do not identify Map74 candidates.

| State at the actual Map74 scan | Result |
| --- | --- |
|427 set, either432|Select27 first; write crop; held527 clears427, but still admits27 on this call.|
|427 clear,432 set|Select32; write crop; held532 clears432, but still admits32 on this call.|
|427 clear,432 clear|No match; d7=-1; no crop write; restore original map operand255.|

Completion is checked **after** selection/crop writes and is not a rejection
predicate. Consequently the527-held/811-set/532-clear callback can set432
yet still admit27 if427 was already held. The no-write callback branches do
not themselves cause a scan; an independently pending/no-scroll request can
still do so using retained unlocks. A changed CURRENT_MAP redirects CURRENT
resolution entirely. CheckBattle does not reset stale CURRENT_BATTLE or
write it to27/32. Success retains resolved d0=74 and returns d7=27/32;
MainLoop moves d7 to d1 and calls j_BattleLoop. This document stops at that
call, before initialization, roster, opening, turn dispatch or outcome.

If no battle matches, MainLoop enters ExplorationLoop with255. Unlike fresh
explicit74 entry, that CURRENT branch clears pending, adjusts the counter,
heals/fades and updates main facing/sprite and nonnegative-low-byte coordinates
without rebuilding actors/mappings/walking scratch, resetting CURRENT_BATTLE
or clearing temporary flags. UpdateMainEntityProperties multiplies whole
coordinate words by384 when their low byte is nonnegative. It calls
DeclareRaftEntity, initializes sprite counters and LoadMap for the current
map, sets base VInts and invokes Map74's RTS init if74 is still selected.

LoadMap sign-extends255 and takes its CURRENT reload branch: it bypasses
new palette/tileset and block/layout loading, retaining those loaded resources,
but refreshes selected area/view/display/roof handling. Thus this fallback
can refresh Map74 metadata that the earlier scroll did not fully initialize;
its empty roof table adds no record. Stale non255 CURRENT_BATTLE also changes
LoadMapArea's rectangle to the supplied battle dimensions; a CURRENT reload
does not repair that state. Conditional music/fade and the pending
event/control gate still apply. A substituted explicit map instead takes
fresh construction and its own admission path. No-match is not an unconditional
promise of control, and a pending replacement can interrupt the fallback.

## Changed-state review and reproduction

| Changed state or boundary | Required distinction |
| --- | --- |
|530 changes before/after town71 selection|SwitchMap samples it for71→78; later changes do not retroactively reload the map. North74 is not substituted.|
|431/531 at castle south return|Town exploration is conditional; admitted31 is outside this slice.|
|Actor/follower/mode/mapping changes|Explicit town/overworld rebuild differs from scroll/CURRENT retention; main action and follower position can change without a rebuild.|
|Layout kind, candidate movement or zone coordinates differ|Warp$1000, zone$1400 and low-byte row selection are separate from collision/natural reach.|
|527/811/532 changes between reads|Actual branch order governs; unlocks are not cleared by no-write branches.|
|427/432 both held, completion held, or neither held|First27 precedence; completion clears unlock but admits now; no-match restores255.|
|428–430 partly written|Preserve reached prefix; no implied later completion or automatic Map74 battle.|
|Stale CURRENT_BATTLE or changed CURRENT_MAP|Scroll crop and CURRENT scan use actual state; callbacks do not repair it.|
|Pending replacement, high/sentinel bytes or stale facing/sound|Four-byte reload overwrite retains adjacent bytes; selector coordinates and consumption-time position differ.|
|Interrupted callback/cleanup/transition|Reached writes remain; callback return, MainLoop handoff, rendering and usable control are different boundaries.|

Load ignored `local/private-inputs.ps1` in the launching process. Use the
verified retained source selection and
[retained-pair recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
without regeneration or pair size/mtime changes. Direct review compares:

1. Both identical five-row north-source warp tables, Map74's eight side-exit
   rows, accepted castle/town seeds, full45-row battle table and SwitchMap
   against actual request, scroll, MainLoop and CheckBattle consumers.
2. Map74 pointer/route/init/fixed-list and selected map metadata against
   retained map-init joins/body and existing common-map records. Compare
   `map-events/routing-setup.json`'s zone table/consumer facts and
   `map-events/zone-programs.json`'s complete Map74_ZoneEvent0 operations to
   actual source; inspect the existing fixture schemas, not only counts.
3. The two longword writes and their existing
   `map-event-request-state-static-v1.json` records, current request consumer
   accesses/control shapes, zone producer/selector/wrapper and exact byte
   reinterpretation. Evaluate every callback branch and27/32/no-match ordering;
   no generic simulator or tests of the review program are introduced.
4. The complete prose/diff and changed states, local links/anchors, UTF-8/LF,
   sole-path/private boundary and `git diff --check`. On the clean committed
   head run `uv run sf2 verify plan --base origin/main --head HEAD`; record
   the planner and actual exact-head scoped CI in the Issue handoff.

Documentation scope skips blanket normal/full/Python/.NET/Godot/H3/H4 suites.
**Inferred:** the north request plus the source-named BATTLE_TO_TRISTAN27
represents the route toward Tristan; it is not evidence of a naturally
completed walk. **Unknown:** supplied state/flags, layout-cell reach,
request timing/replacement, completed movement/cleanup/rendering, usable
control and save/load persistence. Optional compressed-layout absence remains
a no-decoded-route limit, not acquisition authority or a ROM blocker. No new
runtime is launched and these Unknowns are not an automatic observation queue.

Preserve [#1025 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1025#issuecomment-6183176440),
[final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1025#issuecomment-6183213238)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1028#issuecomment-6183221581),
including their linked #1021/#1017 failures, Unknowns and receipts. Original
and rebased results remain distinct; final direct0.146s, independent0.144s and
CI38672496689 are retained history, not new checks. Freeze a clean pushed
Draft PR at current accepted main, Project Review handoff and successful
main-gate callback. No merge, cleanup, archive, next Issue or environment change.
