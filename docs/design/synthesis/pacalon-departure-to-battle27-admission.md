# Pacalon departure: the north scroll and conditional Battle27 admission

Leaving Pacalon does not immediately start the battle on the way to Tristan.
**Confirmed:** the castle's south request first allows a town battle check;
the town's south request then constructs a fresh overworld exploration.
Scrolling north loads Map74 while keeping that exploration's actors and
scripts. A separately admitted zone callback can unlock a battle and request
a reload. Only the later MainLoop scan selects battle27 or32.

This guide continues [Pacalon castle: entry and the first drought-state visit](pacalon-castle-entry-and-first-visit.md).
It stops at the call to BattleLoop, **before initialization or execution**.
**Inferred:** the north request and source name BATTLE_TO_TRISTAN27 describe
travel toward Tristan. They do not prove a naturally completed walk.
**Unknown:** supplied state, decoded route, completed movement/cleanup/display,
usable control and persistence. No observed or remake frontier advances.

## Evidence and scope

The complete accepted [departure research](../../research/pacalon-departure-to-battle27-admission.md)
is the **Confirmed static evidence owner**, accepted through
[#1029 / PR1032](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1032)
at `369757a59864cef7d4c0bf0e40769659fbab219d`. The accepted castle guide and
its [research owner](../../research/pacalon-castle-entry-and-first-visit.md),
[town outgoing requests](../../research/pacalon-interactions-and-castle-handoff.md#ordered-exits-and-outgoing-mainloop-request),
[entry/lifecycle](../../research/battle26-return-through-pacalon-entry.md),
[request writes](../../research/map-event-request-state.md),
[request consumption](../../research/map-event-request-consumption.md),
[common maps](../../research/common-maps.md) and
[random-battle owner](../../research/map-event-random-battle-state.md)
retain their own evidence boundaries. The
[map/exploration contract](../contracts/map-exploration.md) separates static
records, controlled observations and natural progression.

Source paths are relative to `disasm/` at SF2DISASM `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; research retains the USA retail
ROM identity and source/address joins. Fnnn denotes flag nnn; rows are
zero-based. Source/pair are unavailable in this design checkout. The
18,285,979-byte retained pair's accepted0.083s comparison is provenance only,
without source copying, acquisition or a fresh private comparison here.

Standard rigor for [#1034](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1034)
keeps fresh construction, retained scrolling, mutable event bytes and ordered
admission separate. The five owned Markdown paths explain accepted evidence;
they change no research, contracts, fixtures, schemas or engine. Battle31 is
only a town-admission alternative. Later Frayja/cs_5DA28 history, Battle27
initialization, later battles, services, runtime and Chinese synchronization
remain outside scope. Unmerged Battle27-opening research and UI692 supply
no evidence to this guide.

## Back through town, then a fresh overworld

**Confirmed:** castle Map36 row0 requests no-scroll Map02 from `(255,31)`,
destination `(255,1)`, Down3. The handler samples physical0's current signed
coordinate words divided by384. Negative destination bytes preserve whole
sampled words; nonnegative bytes replace only their low byte. Thus X is
consumption-time X, not a saved trigger position; a nonzero sampled Y high
byte survives replacement with1. The outgoing facing is3 and d4 is0.
Pending replacement changes the actual request that MainLoop receives.

MainLoop calls SwitchMap then CheckBattle. There is no Map02 substitution.
Its sole battle candidate is31: wildcard triggers, crop `(0,0,48,32)`,
unlock431/completion531. Clear431 allows exploration. Held431 admits31;
held531 clears431 but **still admits on that call**. Stop that alternative
before BattleLoop. Neither F530, the king's words, F810, pickups nor services
is a town-admission prerequisite.

If exploration is reached, explicit2 begins a fresh lifetime. ExplorationLoop
clears pending type, performs signed-word STEP_COUNTER subtraction20000 with
its conditional zero fallback, heals eligible allies, requests fade-out and
sets view target-1. It writes CURRENT_MAP2/CURRENT_BATTLE255, loads tilesets,
waits for fade, clears VInts, selects/rebuilds actors and mapsprites, clears
temporary256–383 and sets80. Sprite counters, LoadMap, base VInts and current
init follow, then conditional music/fade and WaitForEvent. Healing covers
living allies plus always Peter7/Lemon28; it is not a recruitment operation.

Current530 selects `ms_map2` or `ms_map2_flag530`, with different fixed casts
but the same RTS init. ClearEntities discards castle actors, mappings and
walking scratch; flags reconstruct followers. Town2 has no F65 Caravan
prefix because it is not overworld. Main uses the supplied coordinates/
facing and Bowie's actual HP/class sprite rules. PLAYER_TYPE is not reset.
Non-temporary story/membership/item flags survive; the castle's current
Frayja displacement is not carried as an actor record into this rebuild.

Town row3 requests no-scroll71 `(22,16)`, Down3, from `(255,31)` after the
three castle rows. Its caller still needs applicable layout/movement state.
The row tests no king visit, F810, dialogue or inventory. MainLoop's
SwitchMap substitutes71→78 when530 is held; clear530 retains71. There is no
reverse78→71 rule. Neither map has a battle-coordinate row, so intact71/78
reaches another fresh exploration lifetime and discards the town population.

Both are overworlds: F65 can prepend Caravan before ordinary enabled
followers. Current membership, HP/class, flags and mode govern the existing
declaration consumers; no ally joins here. Neither map has a MapSetups row:
GetCurrentMapSetup returns `ms_Void`/$FFFF, fixed-list selection finds the
sentinel and init is skipped. Shared main/follower/raft construction still
runs. Absence of a setup is not a populated default setup.

Both headers select palette0/tilesets101–105, area `(0,0)..(63,63)`, layer
type254, main-theme music, parallax256/256, zero autoscroll and other-layer
origins. Different block/layout resources prevent inferring identical terrain.
At WaitForEvent, zero pending type installs physical0 control: numeric mode0
chooses character,1 raft, other nonzero Caravan. Those consumer branches
govern even if enum names suggest another reading. Nonzero pending bypasses
installation and wins over A/C. Construction is not observed usable control.

## The north scroll keeps the exploration population

**Confirmed:** Map71 and78 each have five ordered eight-byte warp rows.
After two town entrances, row2 is `(255,0)` → Map74 `(255,62)`, scroll
UP/$11, recorded facing Up1 plus padding. Row3 goes south70; row4 west72.
First match matters. These records do not decode a walkable north edge.

Ordinary `esc02_controlCharacter` uses layout kind$1000 before
WarpIfSetAtPoint. Its supplied coordinate words include movement deltas;
the producer sign-extends/divides by384 and compares source low bytes after
checking the$FFFF terminator. Any negative source byte is a warp wildcard.
It writes event word1, the type/map/X/Y longword, facing/padding word, then
WARP_SFX. No match leaves pending state untouched. Raft/Caravan have other
callers; ordinary character control does not prove their route.

ProcessMapEvent clears the stored type before dispatch. For nonzero PARAM1,
the scroll handler idles physical0, writes CURRENT_MAP from PARAM2 and calls
ProcessMapTransition. Intact74 has no71/F530 substitution. Transition loads
selected tilesets/blocks/layout, reads/scales the first area rectangle and
uses current PARAM1's direction bits to perform the Up transition, including
projection/plane/DMA/VInt work. This is source order, not measured rendering.

The following coordinate updates use mutable PARAM3/4. Negative X byte255
skips both X/XDEST writes; a changed nonnegative X byte multiplies the whole
resulting word by384 without implicitly clearing its inherited high byte.
The handler then clears d0.w before reading Y: intact62 writes Y/YDEST62*384;
a negative changed Y byte skips that axis independently. It samples actual
main position/facing, calls DeclareRaftEntity and returns to exploration.
Recorded PARAM5/Up1 is **not applied**. Actual facing remains current.

The scroll neither calls MainLoop/CheckBattle nor starts fresh exploration.
It does not heal, subtract20000, clear temporary flags, replace CURRENT_BATTLE,
rebuild actors/mappings/walking scratch or invoke Map74 init. WARP_SFX is not
submitted/cleared through the no-scroll branch. New follower flags do not
automatically reconstruct the retained list, although main's action pointer
has changed to idle.

DeclareRaftEntity returns for numeric mode2; mode1 skips follower relocation;
other modes move current listed followers' actual and destination coordinates
to the sampled main tile. Their scripts/mappings persist. F64 gates raft
work: matching RAFT_MAP declares physical31 from masked stored coordinates;
mismatch clears lookup and moves it off-map. Clear64 skips that repair.
Logical follower B31 and physical raft31 are different identities.

The scroll does not call LoadMapArea. It retains selected area/music/layer
metadata and temporary flags apart from explicit transition mutations.
LoadMapBlocksAndLayout still reads actual CURRENT_BATTLE: ordinary255 avoids
crop; stale non255 can apply its supplied crop. Population retention is not
proof that every actor field or loaded resource is unchanged.

## A candidate zone, not a proven arrival

**Confirmed:** Map74 has one unflagged `ms_map74` setup, no530/527 variant.
Its fixed list is empty and its init is RTS. Neither runs as a scroll entry
initializer. Later event selection resolves actual CURRENT_MAP/setup anew.
The header selects palette0, tilesets101/102/109/110/105, one0..63 area,
layer254/main-theme, parallax256/256 and zero origins/autoscroll. Flag/step/
roof tables contain only endWord. They do not disable the separate zone
producer or ordinary movement counter.

Map74's exits are side boundaries: two Moun31 requests, Tristan15, two
bridge47 requests, two duplicate south scrolls71 `(255,1)` and west scroll75
`(62,255)`. Earlier duplicate wins; south71 can become78 at consumption.
None proves arrival, a usable passage or a destination visit.

The ordinary zone producer is `esc02_controlCharacter`'s **layout-kind$1400**
branch, after candidate movement/overlap and layout lookup. It writes event
word6 first, then signed tile quotient `(XDEST+d4)/384` as a word in PARAM1/2
and `(YDEST+d5)/384` as a word in PARAM3/4. d4/d5 are selected movement
deltas. These are candidate destinations, not sampled actual coordinates.
There is no pending guard or atomic payload; PARAM5 and WARP_SFX remain.
Later collision/debug/flagsA-bit6 logic can still skip movement writes.
The continuing movement path sets velocities/travel/destinations and
decrements positive STEP_COUNTER. Posting the event proves no arrival.

Type6 dispatch installs eas_Init on physical0 and reads both parameter words
as X/Y. RunMapSetupZoneEvent resolves the current zone table, checks default
marker$FD first, accepts exact$FF wildcard or matching low coordinate byte,
and adds the first match's signed relative target to the table base. This
wildcard is narrower than the warp producer's any-negative-byte rule.
Map74 row0 `(255,61)` selects Map74_ZoneEvent0 for any supplied X and Y low
byte61. Otherwise the default returns. It does not prove that every X at61
is a zone-kind cell, or that there is a traversable path from the north edge.

After callback return the wrapper closes portrait/text, waits one VInt and
waits for physical0's actual coordinates to equal its destinations before
restoring registers. It does not clear a new pending event. Missing setup
bypasses selection/window cleanup but still takes the common VInt/motion wait.
Changed map, mode, candidate bytes or external event writes change selection;
returning from the callback alone proves neither cleanup nor control.

## What the callback changes, and what its reload retains

**Confirmed:** all Map74_ZoneEvent0 branches are:

| Flags when read | Ordered effects |
| --- | --- |
|527 clear|Set427 → reload longword → set428 → set429 → set430 → return.|
|527 held,811 clear|Return with none of these writes.|
|527 held,811 held,532 held|Return with none of these writes.|
|527 held,811 held,532 clear|Set432 → reload longword → return.|

The first branch never consults811/532. No branch clears427/432 or writes
527/530/532. Existing unlocks survive no-write paths; clear527 repeats its
writes even if427 is already held. F428–430 are unlocks, not completions.
F811's source comment names Frayja asking to go to Moun, but its read proves
no deferred scene/history, restored rain or outcome. F530 is not tested.

The reload constant$000100FF is a **four-byte write** at MAP_EVENT_TYPE:

| Field | Bytes after write |
| --- | --- |
|Event word, $FFA84A..$FFA84B|$0001: warp|
|PARAM1, $FFA84C|$00: no-scroll|
|PARAM2, $FFA84D|$FF: CURRENT|
|PARAM3/4, $FFA84E/$FFA84F|Unchanged zone Y word, ordinarily$003D|
|PARAM5, padding and WARP_SFX|Unchanged|

The retained Y word$003D is later read as **warp X byte0 / Y byte61**,
not two fresh sentinel coordinates. Changed high Y bytes change that X byte
while low-byte61 still matches the zone. Facing might remain the north
record's Up1 if no writer replaced it; sound is also stale state, not a new
callback instruction. The event slot is mutable and can replace another
request; it is not an immutable queued journey.

Interruption after427 but before the reload leaves an unlock without this
request. After reload but before428–430 it leaves a prefix of the flag
writes. The432 branch likewise sets its flag before posting. Cleanup can
interrupt after every callback write; later writers can replace type or
parameters independently. There is no rollback or atomic flag/request commit.

This callback makes no CheckRandomBattle call or RNG draw, no counter reset
to30000, view-scroll wait, boost sound or flash. The generic helper has its
own completion/counter/RNG path and request sequence. Ordinary movement's
counter decrement and later exploration subtraction are different consumers;
they do not make this deterministic callback a random encounter rule.

## Reload consumption and the ordered battle decision

**Confirmed:** on callback/wrapper return exploration sets movement sound
and reenters WaitForEvent. Intact event1 bypasses control installation and
wins over A/C. Dispatch clears stored type; zero PARAM1 submits/clears
current WARP_SFX, idles main and unwinds exploration to MainLoop. It clears
d0–d4 words, reads map255 and samples actual main coordinate words/facing.
Nonnegative PARAM3/4 bytes replace coordinate low bytes, negative bytes
preserve sampled words; PARAM5 replaces facing and PARAM1 supplies d4=0.
The fields are read at different times, so replacement must be evaluated as
consumed rather than reconstructed from the earlier producer.

For ordinary small sampled coordinates and unchanged$003D, the outgoing
register seed is CURRENT255 `(0,61)` with retained PARAM5. The request itself
does not move the actor to X0. Supplied high sampled coordinate bytes survive;
altered negative destination bytes preserve their corresponding whole word.

MainLoop calls SwitchMap (no255 replacement), then CheckBattle. It saves the
original map/coordinate registers, resolves CURRENT from actual CURRENT_MAP
and scans45 seven-byte records in source order. Exactly two use Map74:

| Battle | Crop X,Y,width,height | Trigger X,Y | Unlock / completion |
| --- | --- | --- | --- |
|27, earlier|2,16,32,48|255,255|427 /527|
|32, later|2,16,32,48|255,255|432 /532|

Map equality and held unlock admit a row; triggers use exact-byte or255
wildcard matching. Both are fully wildcard. Candidate Y61, outgoing X0 and
containment in the crop are **not admission predicates**. Crop is output
after selection. Other maps' unlocks, including428–430, supply no Map74 row.

| State at actual scan | Result |
| --- | --- |
|427 held, either432|27 wins; write crop; held527 clears427 but still admits27 now.|
|427 clear,432 held|32 wins; write crop; held532 clears432 but still admits32 now.|
|Both clear|d7=-1; no crop write; restore original map operand255.|

Completion is not a rejection gate. Even the callback that sets432 can lead
to27 when427 was already held. No-write callback paths do not themselves
post a scan, but an independent no-scroll request can scan retained unlocks.
Changed CURRENT_MAP redirects resolution. CheckBattle does not set/reset
CURRENT_BATTLE. Success returns resolved map74 and d7=27/32; MainLoop moves
d7 to d1 and calls j_BattleLoop. **This is the stopping boundary.** Roster,
opening scripts, initialization, turns and outcomes are separate work.

If no row matches, CURRENT255 reaches retained exploration: pending clear,
counter adjustment, eligible heal/fade, main facing/sprite/coordinate updates,
DeclareRaftEntity, sprite counters, LoadMap, base VInts and current init.
Whole coordinate words multiply by384 when their low byte is nonnegative.
Actors/mappings/walking scratch, temporary flags and CURRENT_BATTLE persist.
Map74 init is RTS if74 is still current; altered map/state follows its owner.

LoadMap's CURRENT branch bypasses new palette/tilesets and block/layout
loading, keeping loaded resources, but refreshes area/view/display/roof
handling. It can refresh Map74 metadata left from the earlier scroll.
Stale non255 CURRENT_BATTLE can make LoadMapArea use supplied battle
dimensions; CURRENT does not repair it. Conditional music/fade and the
pending/control gate still apply. Replacement by an explicit map instead
takes that map's admission/fresh lifetime. No-match alone does not establish
usable control.

## Changed states and retained evidence review

| Changed premise or interruption | Required reading |
| --- | --- |
|530 changes before/after selection|71→78 samples current530; no retroactive reload or north74 substitution.|
|431/531 at castle return|Town exploration is conditional; Battle31 handoff stays separate.|
|Actors/followers/mode/mappings/HP/class change|Fresh rebuild differs from scroll/CURRENT retention; relocation and idle writes still change actors.|
|Layout/candidate/deltas/collision differ|Warp$1000, zone$1400, table match and actual arrival are separate.|
|527/811/532 change between reads|Actual branch order wins; no-write branches retain existing unlocks.|
|427/432 both/neither held; completion held|27 precedence, clear-unlock-but-admit-now, or retained CURRENT fallback.|
|428–430 only partly reached|Preserve the written prefix; infer no completion or new Map74 candidate.|
|CURRENT_MAP/CURRENT_BATTLE stale or altered|Actual map resolves the scan; stale battle can affect scroll crop and fallback area.|
|High/sentinel bytes, facing/sound or pending replacement|Candidate zone words become differently interpreted warp bytes; consumption samples actual position.|
|Callback/cleanup/transition/control interrupted|Reached mutations persist; returned script, completed wait and usable control are different claims.|

The pre-synthesis review consumed complete accepted #1029 and the castle
guide, the necessary entry/request/common-map/random-helper owners and the
actual public payloads, schemas and verifier consumers below. No blocking
owner disagreement was found. A future contradiction returns to research;
Layer B does not silently repair it.

| Public fixture / consumer | Reviewed join and limit |
| --- | --- |
|`sf2-map-init-static-v1`; [fixture](../../../tests/fixtures/h2/map-init-static-v1.json), [consumer](../../../src/sf2tool/h2/map_init.py)|Actual Map74 route126, pointer and RTS body; absent71/78 routes. These are not observed scroll-entry execution.|
|`sf2-map-events-static-v1:routing-setup` / `:zone-programs`; [routing](../../../tests/fixtures/h2/map-events/routing-setup.json), [programs](../../../tests/fixtures/h2/map-events/zone-programs.json), [loader](../../../src/sf2tool/h2/map_events_fixture.py), [verifier](../../../src/sf2tool/h2/map_events.py)|Map74 zone/default target, first-match facts, all15 callback operations/branches and both reload writes. Schema-valid structural records prove no natural producer-to-consumer path.|
|`sf2-map-event-request-state-static-v1`; [fixture](../../../tests/fixtures/h2/map-event-request-state-static-v1.json), [consumer](../../../src/sf2tool/h2/map_event_request_state.py)|Both Map74 longword definitions and caller-local control/reaching handoffs. Callee algorithms and runtime consumption remain outside this compact relation.|
|`sf2-map-event-request-consumption-static-v1`; [fixture](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), [consumer](../../../src/sf2tool/h2/map_event_request_consumption.py)|Actual exploration reset, WaitForEvent reads, pre-dispatch clear/branches and raft accesses. Bounded contexts do not enter the full warp/scroll/map callees.|
|`sf2-common-maps-static-v1`; [fixture](../../../tests/fixtures/h2/common-maps-static-v1.json), [consumer](../../../src/sf2tool/h2/maps.py)|SwitchMap, ordered trigger/completion and resource-load facts. Full selected coordinate/scroll/zone producer and fallback joins rely on accepted #1029, not newly inspected private source here.|
|`sf2-map-event-random-battle-state-static-v1`; [fixture](../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json), [consumer](../../../src/sf2tool/h2/map_event_random_battle_state.py)|Actual seven selected programs/eight helper callers exclude Map74; guarded counter/RNG/request body differs from its deterministic callback. No natural random encounter is observed.|

Owning commands are `uv run sf2 h2 map-init`, `map-events`,
`map-event-request-state`, `map-event-request-consumption`, `common-maps`
and `map-event-random-battle-state`. Public surfaces are inspected directly;
private-source extraction/runtime commands are not rerun. Compact fixture
counts do not replace the accepted connected source review or decode terrain.

[#1029 final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1029#issuecomment-6183401642)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1032#issuecomment-6183422393)
retain final direct0.143s/independent0.136s and CI38673862670, separately from
original0.132s/0.134s, CI38673722838 and the corrected KeyError('path')
inspection diagnostic. The research document was byte-identical across its
synthesis-only rebase. The accepted0.083s pair result is reused, not reset.

[#1030 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1030#issuecomment-6183352710)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1031#issuecomment-6183367265)
retain direct3.075s/independent3.229s and CI38673538882, with separate
checker import/field/link diagnostics. The predecessor guide preserves
linked #1025/#1026 and earlier results, corrections, failures and Unknowns.

This Issue's handoff records complete prose/diff/changed-state review,
actual public-schema/consumer joins, exact five paths, navigation/links/
anchors/UTF-8/private checks, git diff --check, configured clean-head planner
and actual exact-head scoped CI. Documentation scope skips blanket normal/
full/Python/.NET/Godot/H3/H4/translation suites. Optional compressed-layout
absence remains a no-decoded-route limit, not acquisition authority or a ROM
blocker. Natural reach, supplied flags/state, interruption/timing, completed
movement/cleanup/rendering, usable control and save/load persistence stay
**Unknown**, without creating a runtime queue.
