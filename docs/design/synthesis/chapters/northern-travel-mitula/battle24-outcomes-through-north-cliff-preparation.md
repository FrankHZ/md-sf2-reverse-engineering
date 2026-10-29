# Battle24 Outcomes through North Cliff Preparation

Battle24 ends according to the combatants still counted on the field. Defeating
the first Dark Bishop does not automatically win it. Victory returns the party
to exploration at a position derived from current party state; defeat and
escape use a different destination selector. The North Cliff cave warning is
a later, separately admitted encounter with current inventory and current
actors, rather than an automatic victory scene.

**Confirmed static:** either missing-item branch names Dynamite, including
the branch where Cannon is absent. It stages the same reprimand and then
sets a temporary encounter latch. It consumes neither item and performs no
automatic town trip, choice prompt or recruitment. **Inferred:** with the
usual distinct cast, the movement and gestures stage an adviser reprimanding
the party before cave preparation. **Unknown:** natural travel and outcome,
supplied state, completed services, visible settling, usable control and
save/load persistence. This guide ends at the missing-item scene's local
retry/control boundary. Actual removal, the blast, Rohde, the cave interior
and Battle25 have separate scope.

[North Cliff opening and Rohde recruitment](north-cliff-opening-and-rohde-recruitment.md)
continues the both-item path through actual removal, the full blast/join scene,
local return and fresh-map consequences. It identifies the next entrance warp
only, retaining the cave-interior/Battle25 boundary.

## Evidence and Reading Boundary

The accepted Layer A owner is
[Battle24 outcomes through North Cliff preparation](../../../../research/chapters/northern-travel-mitula/battle24-outcomes-through-north-cliff-preparation.md),
accepted through [#981 / PR984](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/984).
It pins USA retail SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
and [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6).
Source paths below are relative to `disasm/`; Fnnn denotes flag nnn. The
roadmap owns the [Standard scope agreement](../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
This explanation adds no original-game finding or remake behavior.

The [admission guide](new-granseal-departure-through-battle24-admission.md)
explains the preceding request, ordered selection and fresh formation.
[Battle23 outcomes](../southern-travel-creed/battle23-outcomes-and-burrow-return.md) and the accepted
[battle loop](../../../../research/battle/battle-loop.md),
[cutscene wrappers](../../../../research/battle/battle-cutscenes.md),
[inventory search](../../../../research/chapters/northern-travel-mitula/new-granseal-inventor-and-dynamite-gift.md),
[request state](../../../../research/world/map-event-request-state.md) and
[request consumption](../../../../research/world/map-event-request-consumption.md)
owners provide shared mechanisms. The
[encounter-definition](../../../contracts/battle/battle-encounter-definition.md) and
[cutscene-routing](../../../contracts/battle/battle-cutscene-routing.md) contracts preserve
their structural limits. The deeper selected facts below come from #981.

## Winning Is a Counting Rule

**Confirmed static:** Battle24's leader-presence byte is zero. Dark Bishop128's
`LEADER` AI is a separate operand. Its death can reach the defeated wrapper
when Bowie is alive; with F524 clear that wrapper executes `ms_Empty`, while
set524 skips dispatch. Both paths reach the zero-leader tail and append no
living enemies to the dead list. Bishop135 supplies no special hook selector.
Killing128 therefore leaves the other fifteen enemies to their actual state.

`battleloop_1.asm:@IsBattleEnd` first runs the defeated hook, processes the
current dead list and counts remaining combatants. It checks zero allies
before zero enemies. If both sides remain, it applies after-turn effects,
processes deaths again and repeats those checks without another defeated hook.

The counter scans all30 ally and32 enemy slots. Nonnegative X and a nonzero
HP word count; joined/active membership and enemy type are not its predicates.
Bowie HP0 forces the ally count to zero, so defeat wins a simultaneous loss.
With Bowie alive and positive counted allies, zero counted enemies suffices
for victory. Bowie alive alone does not repair zero placed allies. Supplied
off-map or nonmember combatants can change these counts.

Dead-list processing is not a fresh HP scan. It uses the current length and
entries, presents each death, then performs bookkeeping, sets X/Y=-1 and
status0, recomputes stats and moves its mapped entity to `$7000,$7000`.
It neither writes HP0 nor clears the list length. Coherent entries/mappings
and completed sound, spin and VInt services remain premises; stale or duplicate
entries are not repaired here.

Fresh formation and completed/suspended state remain distinct. F524 can alter
fresh enemy definitions through the accepted upgrade path. F88 suspension
uses supplied current state instead of promising the original sixteen-enemy
formation. Intro474, unlock424 and completion524 have different consumers.

## Three Outcomes, Two Destination Mechanisms

### Victory uses current party placement

**Confirmed static:** `BattleLoop_Victory` performs this ordered work:

1. Heal all30 living allies and Peter7/Lemon28 even when dead: HP/MP maxima,
   existing status masked to STUN/POISON/CURSE, then recomputed stats/equipment
   effects. Membership is not the healing predicate; other dead allies remain
   dead. Battle44's timer special does not apply.
2. Seed return parameters from current state. PARAM2 takes CURRENT_MAP;
   `UpdateForceAndGetFirstBattlePartyMemberIndex` rebuilds membership and
   selects the first joined-and-active ID in ascending order, not the first
   living member. Its X/Y plus BATTLE_AREA_X/Y become byte PARAM3/4, and its
   actual ENTITY_INDEX_LIST mapping supplies facing in PARAM5. PARAM1=0;
   this seed does not write MAP_EVENT_TYPE.
3. Execute the after wrapper. Clear524 dispatches the empty after script;
   set524 skips it. Both reach `EndAfterBattleCutscene` and `JoinForce(0)`.
   Bowie becomes joined and, below twelve active members, active32 is set.
   A full party evicts nobody. This is not Rohde recruitment, and the join
   occurs too late to repair the already selected return seed.
4. Clear424, set524, zero d0–d3 words, load PARAM2–5 bytes into them and return
   d4=1. Intro474 is unchanged.

For the selected Map72 battle area `(1,32)`, local `(x,y)` gives return
`(x+1,y+32)` modulo256 with current mapped facing. This is neither the initial
placement nor a fixed cave tile. Dead/off-map first members, a changed area,
map or mapping, and intervening parameter writes can alter it. An empty active
list leaves a stale first-list byte; there is no empty-list or facing repair.

Both end/after table slot24 targets are `ms_Empty`, the word `$FFFF` at
`$47A4E..$47A50`. Empty scripts still have wrappers. The layout selects the
cutscenes after-join table, whose slot24 is0, rather than its similarly named
global counterpart. No Battle24 placement override exists. A terminator can
still wait for scrolling when dialogue is open and clears VIEW_SCROLLING_SPEED.
An unfinished after script prevents the later join/flags; an unfinished join
prevents the clear/set. Earlier effects are not rolled back.

### Defeat and escape use EGRESS_MAP

**Confirmed static:** defeat updates unlock, clears DIALOGUE_NAME_INDEX_1,
requests sad music and text363, closes text, restores Bowie's maximum HP,
halves unsigned gold and obtains Egress coordinates before returning d4=-1.
Battle04's special does not apply. Admitted Egress deducts spell MP; Angel
Wing removes its selected inventory slot. Their shared text/sound/flash tail
updates unlock, unwinds the action frame, obtains coordinates and returns
d4=0. Escape does not halve gold or perform defeat's Bowie restoration.
These entered handlers do not establish menu availability or item/spell custody.

The unlock helper preserves424 when524 is clear and clears424 when524 is set.
Defeat/escape do not set524 or run victory's after/join, and neither changes474.
Completed admission may already have cleared424.

`GetEgressPositionForBattle` has no Battle24 special. It passes **EGRESS_MAP**,
not the previous position/facing, to `egressinit.asm`. With399 set, supplied6
selects New Granseal `(11,22)`, Up. Supplied72 has no savepoint row and retains
Map72 with default `(1,1)`, Up. Other selectors use their own row or default;
the warning text does not select New Granseal. A direct399-clear call instead
returns initial Map3 `(56,3)`, Down before raft reset; BattleLoop sets399 at
fresh or resumed entry.

With399/F64 set, raft reset uses the savepoint: Polca/Bedoe select Map72
`(43,48)`, Hassan Map69 `(28,55)`, otherwise Map69 `(42,15)`, including
supplied6/72. F64 clear preserves raft fields. This is saved raft positioning,
not boarding.

## Exploration Returns before Another Battle Scan

**Confirmed static:** every outcome reaches `alt_MainLoopEntry`, then
SwitchMap and ExplorationLoop, without an immediate CheckBattle or a branch
on d4's sign. Map72 has no flag-switched replacement. Victory does not invoke
the cave callback or immediately repeat Battle24.

Ordinary explicit72 return clears pending, reduces word STEP_COUNTER by20000
and clamps negative to0, heals eligible allies, fades out and sets map72 /
CURRENT_BATTLE255. It loads tiles and rebuilds entities/mapsprites, clears
temporary F256–383 and sets80 before map load, base VInts and init. Persistent
story/party flags, including424/474/524, survive. BattleLoop cleared PLAYER_TYPE;
the empty hooks do not replace it. Ordinary mode0 and supplied/direct modes1/2
must remain distinct.

Map72's only setup has no fixed residents. Fresh initialization clears battle
actors/mappings, declares enabled flag followers from physical1 and main at0.
Empty residents therefore do not mean an empty cast. Mode2 changes main's
sprite, stands followers and declares a separate raft at `(64,64)`; otherwise
F64 plus matching RAFT_MAP can declare the stored masked raft position at
physical31, lookup63, Left. FOLLOWER_B's logical lookup31 is separate.

Map72 init has six operations: return if716 is clear, return if Luke10 is
joined, otherwise run the accepted Luke scene and return. Items,750/805/524
do not replace those guards, and init does not invoke the missing-item scene.
Conditional music/fade-in precedes WaitForEvent. Successful loading/init is
not itself usable-control evidence.

A CURRENT255 no-battle reload retains actors and temporary flags while still
loading/running init. Its raft consumer returns for mode2; mode1 skips ordinary
follower relocation, while other modes relocate the current list before the
F64/map gate. Border scrolling retains actors and skips init/temp reset.
F88 suspension is instead a battle-entry history; its supplied current state
must be followed through the outcome.

Later zone events still have their own production and admission conditions.
The consumer clears pending, installs `eas_Init` on physical0 and compares
coordinate low bytes against the first matching row:

| Ordered rows | Predicate | Callback |
| --- | --- | --- |
| 0–2 | X32,33,34; any Y | helper26 |
| 3–8 | X25–30 respectively; Y37 | cave callback |
| default9 | `$FD` after no specific match | F523 clear: helper8; set: helper24 |

Declining a specific26/cave callback never falls through to default24.
Walking needs actual masked layout kind`$1400`; pending can be written before
a later blocking check. Coordinates do not establish an admitted step/event.
Mode1 uses `eas_Raft`/opcode7 and mode2 `eas_Caravan`/opcode8, neither this
walking producer; a supplied event can still reach the zone.

Helper24 requests immediately with524 clear. With524 set, any nonzero counter
declines; counter0 draws range8, zero requests, and nonzero draws range4+2 for
counter2–5. A request sets424, writes partial longword type1/CURRENT255 and
counter30000 before flash services, leaving residual position/facing fields.
A later no-scroll return reaches ordered Map72 admission8/24/26: first unlocked
row wins, without crop containment. Completion clears the selected unlock
after selection, without continuing the scan. F408 can intercept a24 request;
F426 can remain for later. Postbattle bypass does not abolish these later paths.

## The Cave Checks What the Party Carries Now

**Confirmed static:** `Map72_ZoneEvent3` first returns for750 set, then805
clear, then256 set. On admission it clears256 and searches Cannon, then
Dynamite, comparing each result's full word against `$FFFF`. No524/474/806,
actual Astral, mode, HP/class or party-capacity guard substitutes for these
tests. If Cannon is absent, Dynamite is not searched at all.

The accepted inventory predicate rebuilds joined IDs in ascending order,
including reserve and dead members. For each carrier it counts non-NOTHING
entries across four slots, then searches from count-1 down to0, comparing
entry masked by`$7F` with the unmasked requested word. The highest examined
matching slot of the first matching member wins. A gap can hide slot3 when
count<4; unjoined carriers are excluded. The returned member byte does not
normalize the rest of d0; exhaustion writes d0.w/d1.w=-1. Empty joined lists
lack a safe initial loop guard. Gift-history761/804 and follower identity do
not establish present inventory.

| Current lookup result | Admitted branch |
| --- | --- |
| Cannon absent, whether Dynamite is present or absent | Set item-name INDEX_1 to Dynamite `$74`; run `cs_5023E`; then set256. |
| Cannon found, Dynamite absent | The same name, scene and later256; Cannon is not consumed. |
| Both found | Cannon removal call, Dynamite removal call, `cs_4FFDA`, set750, then256: a separate next boundary. |

The missing paths only search inventory; neither consumes an item or recruits
anyone. Both-present call order does not prove removal success: the caller
checks no removal return. This guide stops before that callee audit and the
blast/Rohde scene. Set750 follows that scene's return; missing paths never
set750. Early guards leave state unchanged and do not select default battle.

## Who Receives the Reprimand

**Confirmed static:** the scene uses logical A=FOLLOWER_A30,
B=FOLLOWER_B31, P=Peter7 and Bowie0. These are lookups, not physical slots or
guaranteed identities. It creates no actors and repairs no absent/duplicate
mapping. F805 does not require the cast flags65/67/70.

On fresh mode0 Map72 with only65/67/70 enabled, A maps physical1 Caravan,
P2 Peter and B3 Astral, with main0. Without65, A retains lookup0 and aliases
main, while P1/B2 remain. Without relevant declarations all three can alias0.
Later74/77/83 A declarations can replace Caravan with King/Old Man/King;
the last enabled B row among74/68/70 selects Astral/Oddler/Astral. Declarations
append physical actors to the exploration list, and later lookup overwrites
do not remove earlier actors. A hidden`$FF` mapping is not a safe absence
sentinel for the shared lookup/scaling routine.

Fresh sprite selection can produce Blue Flame for an out-of-battle HP0 ally;
otherwise Bowie's HERO/SDMN and Peter's PHNX/PHNK classes select their table
sprites. Other supplied classes follow the shared algorithm. F67 declares
Peter independently of membership/HP, though ordinary outcome healing restores
immortal Peter. CURRENT255 may retain changed sprites/mappings instead of
these examples. Thus the following choreography always acts on actual mappings.

## The Missing-Item Scene, in Order

**Confirmed static:** `cs_5023E` contains40 map-script records plus two inline
action records,212 encoded bytes in total. The source sequence and the generated
movement program are separate representations.

### Approach, gestures and the first warning

The script sets text cursor3357, installs Init and waits on P, B, then A,
and moves the camera to `(21,33)`. Init sets acceleration factors0, speed32,
obstruction/collision and auto-facing, faces Up, disables immersion, requests
size24/sprite refresh and jumps Idle. It does not reset every destination or
travel field and has no HP guard.

Destinations then request A`(27,39)`, B`(28,37)`, P`(25,37)` and Bowie`(26,37)`.
The first three use bit15 to skip their position wait; Bowie's command joins
his actual position to destination with VInt retries. These destinations scale
by384 and install travel/velocity magnitude32. Subsequent waitIdle A/B/P calls
poll script Idle, not all arrivals. Aliases can overwrite prior destinations,
so even distinct-actor Idle checks do not prove a settled assembled cast.

Bowie, P and B face Up. After50 ticks Bowie faces Right; after30 B faces Left.
Bowie nods, B shivers, P faces Right and a20-tick wait precedes text3357.
Facing skips dead Bowie/Peter but A/B bypass that HP test. Nod is unguarded:
counter-1, Sleep10, sprite/DMA transformation, Sleep20, refresh, Sleep10,
counter0. Shiver saves global size/counter, uses size21/counter-1 and three
resize on/off cycles with Sleep5 per half, then restores the saved values.
It leaves resize cleared, rather than restoring an arbitrary former flag.

Text3357 asks why the party forgot the named item and addresses LEADER.
Both missing branches supply **Dynamite**, even when Cannon alone is absent.
ITEM resolves INDEX_1 through GetItemName. LEADER separately rebuilds force
and names the first active member, not necessarily mapped physical Bowie;
changed or empty lists affect that substitution independently of B.

Context`$001F` selects B's current sprite for portrait/speech: Astral209 gives
Astral/bleep6, Oddler213 gives Oddler/bleep4, otherwise the actual table row or
default portrait/bleep6 applies. An already open portrait can bypass opening,
but speech lookup still occurs. Non-skipped text waits scrolling, displays the
cursor, increments it, closes portrait/text and sleeps10.

### Shrinking, a step and two timed jumps

The script stops Bowie's animation, requests sizes22,20,18 with10-tick waits
between, then gives B a compiled one-tile left movement and waits for Idle.
Stop/start change animation counter-1/0, not movement enablement, and their
word-actor guard skips dead Bowie. Size commands are unguarded: each temporarily
changes global size, sets the actor resize flag, refreshes/waits a VInt, then
restores global size. They do not store permanent actor height18.

The inline `moveLeft1`/`endActions` source is4 bytes. Its compiler emits a
relative opcode4 move`(-1,0)`, opcode1 arrival wait and a jump to Idle:
14 generated bytes, starting from the map-script scratch reset. It masks
B's FLAGS_A with`$9F`, bypassing the shared other-entity obstruction check
while bit5 stays clear. The move adds`(-384,0)` to current position; the nominal
`(28,37)`→`(27,37)` depends on the earlier staging having arrived. Waiting on
this generated program's Idle still depends on movement/action services.

Two waited `eas_Jump` installations follow. Each disables auto-facing, requests
relative`(0,-1)`, waits5, relative`(0,1)`, waits5, relative`(0,0)`, re-enables
auto-facing and jumps Idle. These also use opcode4, but fixed delays replace
the compiled step's inserted arrival wait. Each offset uses then-current
position; the last zero move does not restore a saved starting point. Idle
does not prove a rendered jump or exact position restoration.

Text3358 then calls the party foolish and tells it to go back and bring the
item. There is no YesNoPrompt, warp, reload or transaction. Its normal W1
accepts held direction/A/B/C after a VInt wait, not Start; the preceding W2
uses continuation/input and validation sound. N denotes newline.

ExecuteMapScript clears the text-skip latch. Debug-enabled P2 Start can set it:
skipped text neither displays nor increments the cursor, and csWait sleeps
skip. Movement, nod/shiver's internal sleeps, size changes, action waits and
following still execute. Skip is not cancellation or instantaneous completion.
No explicit music/fade/flash command belongs to this missing-item program.

### Restoration and a branching following chain

Bowie receives Init and startEntity, requesting size24/refresh and restored
animation. The final follow commands request A→Bowie at slot2 offset`(-24,0)`,
P→A at slot1`(0,-24)` and B→A at slot3`(0,24)`. This is a branching chain,
not three direct followers of Bowie.

The follow handler reads the **high byte** of its follower word for the alive
guard. Words`$001E/$0007/$001F` therefore all test Bowie0, not Peter or the
mapped follower. With Bowie dead they skip installation after earlier
unguarded scene changes. With Bowie alive the actual mappings govern every
target, including aliases/self-follow and overwrites.

Each admitted AddFollower copies42 bytes and installs its script before
deduplicating the exploration-list append. Three commands allocate126 bytes
without a capacity guard or settling wait; unrelated followers remain. A
repeated direct scene allocates again even for an existing listed actor.
Fresh ClearEntities resets the walking buffer; resetting map-script scratch
does not reclaim it. These lifetimes explain changed/repeated behavior without
assuming unlimited follower capacity.

## Local Return and Retry

**Confirmed static:** the terminator conditionally waits scrolling if dialogue
is open and clears VIEW_SCROLLING_SPEED. Only after scene return does the zone
caller set256. It closes portrait/text, waits a VInt and joins physical0's
position, not the entire cast. WaitForEvent restores camera0 and the selected
controller only when pending is zero. Pending overwrites, mode/mapping and
unfinished services can change continuation. This local return is not a
universal playable-control endpoint.

F256 is temporary suppression, distinct from cave-cleared750. Walking away and
back, or acquiring an item without fresh entry, does not alone clear it.
CURRENT255 no-battle reload retains256. Fresh explicit entry clears256–383;
with750 clear/805 set it can retry current inventory. Set750 suppresses the
callback even after that temporary reset.

A direct `cs_5023E` call bypasses the zone guards and writes neither256/750
nor the item-name word; ITEM can inherit another index. Interruption before
the caller's set256 leaves partial actor effects with no latch write. Neither
this scene nor the outcome wrappers roll completed prefixes back.

| Changed state | Consequence to preserve |
| --- | --- |
| Bishop128 dead; another counted enemy survives | No leader cleanup; battle can continue. |
| All enemies uncounted, Bowie HP0 or no counted allies | Defeat has priority. |
| Completed524 or suspended88 | Wrapper/upgrade or supplied-state rules replace fresh assumptions. |
| Different first active member, area or mapping | Victory return seed changes; later JoinForce0 cannot repair it. |
| 750 set,805 clear or256 set | Ordered guard return; no search, scene or default fallthrough. |
| Gapped, dead/reserve or unjoined carrier | Actual count-limited joined search decides item presence. |
| Missing/conflicting A/B/P mappings or changed class/HP | Actual actors, guards, portrait and following differ. |
| Peter dead, Bowie alive | Peter facing can skip while final follow admission still passes. |
| Bowie dead in direct scene | Facing/animation/follows can skip; unguarded destinations/gestures/size/actions remain. |
| Repeated direct call or manually cleared256 | No scene-owned guard; walking-buffer copies accumulate. |
| Debug skip or unfinished service | Preserve reached effects without text, arrival, later flags or control guarantees. |
| Fresh entry versus current reload/scroll | Population/temp reset differs from retained cast/latch. |

**Unknown:** natural battle/travel/layout/event production; supplied flags,
inventory/gaps, membership, HP/class/mode/mappings and buffer capacity; completed
input/text/sound/palette/DMA/action/follower/scroll services; visible cast
settling, usable control and save/load persistence. These do not authorize
runtime work or expand this guide into the both-item scene.

## Evidence Review and Retained Results

The complete accepted #981 owner was reviewed against the admission predecessor
and selected public fixture/schema/consumer surfaces. The
[map-events fixture](../../../../../tests/fixtures/h2/map-events-static-v1.json)
and its loader retain the full Map72_ZoneEvent3 operation/branch order, lookup
targets, six cave-row references and first-match routing. The
[map-init fixture](../../../../../tests/fixtures/h2/map-init-static-v1.json)
retains Map72's six-operation716/Luke guard and script target. The
[random-battle fixture](../../../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json)
retains helper/request ordering. The
[cutscene fixture](../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)
retains wrapper guards/shared join; the
[routing fixture](../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)
has `routeTargetsParsed=false`, and the
[spriteset fixture](../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)
has `rowContentTracked=false`. Aggregate counts do not prove slot24 targets,
private roster rows or the detailed missing-item scene.

The [map-script fixture](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)
and its verifier retain command-family/handler facts and corpus summaries,
rather than publishing this private detailed program. Source/generated action
distinctions, actual follow guards, full text/scene and consumer joins above
consume accepted #981's selected review, not an inference from aggregate
schema success. No selected contradiction was found; material contradictions
return to research rather than being repaired in Layer B.

Owning commands remain `uv run sf2 h2 map-events`, `map-init`,
`map-event-random-battle-state`, `battle-cutscenes`, `battle-routing-data`,
`battle-spriteset-data` and `map-script-engine` under that prefix, plus Layer A's
retained-pair recipe. Source/pair are absent here. Selected public schema and
consumer checks are not a fresh source/pair comparison; no acquisition,
regeneration or new environment is part of this synthesis.

[#981 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/981#issuecomment-6180545109)
and [PR984 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/984#issuecomment-6180565663)
retain final `e53e678a`, tree `81d1f6ef`, base `1a4d0092` and original
`a21d8fe9` with unchanged blob; CI run38652506094 (scope15s/research33s),
author0.081s/independent0.082s unchanged 18,285,979-byte pair comparisons and
40+2 records/212 bytes. Literal
terminator, zone-key, jump-opcode and obstruction corrections remain with
those receipts. The preflight's guessed Battle23 owner was resolved to the
linked actual owner; narrowed reads supersede truncated inspection output.

[#982 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/982#issuecomment-6180471079)
and [PR983 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/983#issuecomment-6180498860)
retain `2d3c9072`, tree `f6137ec1`, base `5d707f60`, CI38652007944 (scope20s),521links
and corrected case/source-size scratch assertions. The admission guide's
[retained review](new-granseal-departure-through-battle24-admission.md#evidence-review-and-retained-results)
preserves #977's Map68 P2 correction, original/intermediate/final CI and both
distinct0.080s pair receipts, plus linked #978/#973/#969/#965/#954/#962 failures,
corrections and Unknowns. Completed failures and suites are not reset or
repeated to obtain replacement receipts.

The #986 handoff records direct prose/diff/navigation/links/anchors, UTF-8,
private/exact-ownership and `git diff --check`, same-process configured clean
committed planning and actual exact-head CI. Documentation-only scope uses
direct checks without blanket Python/.NET/Godot/H3/H4/translation suites.
Independent acceptance and integration remain with main-gate.
