# Battle27: activation and the first turn handoff

The force can be outside every declared region while every placed enemy is
eligible for a turn. **Confirmed:** after completed fresh Battle27 initialization
and opening, none of the twelve formation positions triggers any of the four
regions. All sixteen initialized enemies nevertheless qualify for turn records.
Region activation, spawning and turn admission ask different questions.

This guide continues [the initial formation and restrictions confrontation](battle27-initialization-and-opening.md)
from `BattleLoop:@Start`, after the opening wrapper returns. It stops at the
first `ExecuteIndividualTurn` call, **before its body**. The conclusion requires
unchanged fresh state and all intervening services returning. **Inferred:**
the formation stages contact before movement into the declared regions.
**Unknown:** natural reach, actual party/state/RNG, first actor, completed
presentation, usable control and persistence. No observed or remake frontier advances.

## From the returned opening to a turn call

**Confirmed:** the accepted [ordered prefix](../../../../research/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md#ordered-prefix-and-supplied-continuation)
performs these steps:

1. Activate enemies, including the extra fall-through helper call for slot 160.
2. Run the current-battle region-cutscene wrapper.
3. If the debug byte is nonzero, request diagnostics for held flags 90–105:
   text 463 with the region index in DIALOGUE_NUMBER, then close text.
   Zero skips those requests. No displayed result is observed.
4. Build the spawning-enemy list. Zero length skips spawning; otherwise consume
   its stored byte IDs in order and call SpawnEnemyWithCamera for each.
5. Generate the turn-order buffer and clear the current-turn byte.
6. At `@IndividualTurns_Loop`, read that byte as an **offset**, then read a
   combatant byte from BATTLE_TURN_ORDER. `$FF` returns to `@Start`; another
   value reaches the call that ends this guide.

No HP, score, membership, activation or outcome test intervenes between that
byte read and the call. d0.w is cleared before reading the offset and ID, so
the ID is zero-extended within the word; upper d0 bits are not cleared. a0
addresses the buffer. Reaching the call establishes no executed turn or control owner.

The predecessor's F477 write occurs **before** its scene. A held flag or an
interrupted scene is not evidence of a completed opening. Start this explanation
only after the relevant wrapper returns; the current selector and missing
physical-mapping limits remain those of the predecessor.

## Outside the regions, with their checks remembered

**Confirmed:** activation scans enemy slots 128–159, accepting nonnegative
signed-byte X, nonzero current HP word and an enemy-marked ID. It does not
test Y, membership, neutral state or move orders. All fresh rows 128–143 pass;
absent 144–159 fail X/HP. A high-bit HP word still passes the nonzero test.

For current Battle27, the trigger helper scans declared regions 0–3. A changed
selector with zero declared regions returns before activation writes.
It first tests each bit of NEWLY_TRIGGERED_BATTLE_REGIONS, the **checked word**.
A held bit skips geometry. Otherwise it evaluates the region, sets persistent
F90+index only on a hit, and sets the checked bit even on a miss. Neither this
prefix nor the helper clears that word or the persistent flags. Fresh entry
cleared F90–105, **not** the checked word. A later AI-side clear lies beyond
this guide and supplies no premise about its incoming value.

The [complete geometry, twelve-position results and rejection arithmetic](../../../../research/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md#integer-region-join)
are the authority for all four type-4 regions. They use triangles 1/2/4, then
3/2/4 if needed. The actual edge consumer uses signed coordinate bytes,
the larger absolute delta (horizontal on equality), signed multiplication/
division with truncation toward zero, and projection **from endpoint B**.
Its edge order, immediate result-1 acceptance, opposite-vertex comparisons
and bit-0 exceptions matter; a generic floating-point polygon model is insufficient.
The Region1 rejection includes a negative X projection and the actual CCR/
SynthesizeFlag result 2 versus vertex1's 0. Source comments cannot replace it.

Every formation ordinal at local X14–17/Y44–46 is outside **each** region.
The complete owner gives each ordinal and all four results, rather than
inferring rejection from a bounding rectangle. Geometry offers all 30 ally
slots, requiring **positive signed** HP and nonnegative signed X **and Y**.
It does not check joined/active membership; a changed living placed nonmember
can trigger a region. High-bit HP, HP 0 or negative coordinates fail this scan.
Enemy/neutral slots are not offered. These are different tests from activation
and turn admission's nonzero HP and no Y test. The region index guard is not
a general strict index<count validation; normal iteration supplies 0–3.
Trigger 15 is a sentinel, not a fifth polygon.

Fewer/dead members leave only a subset of the initialized positions, with
other allies at X=-1. Under unchanged fresh premises F90–93 therefore stay
clear, regardless of incoming checked bits; F94–105 also remain clear.
The first passing enemy leaves the checked word equal to **incoming OR `$000F`**,
not necessarily exactly 15. These bits record checks, not successful triggers.
Changing positions later does not rescan a region whose bit is already held.

## What activation can change on the sixteen enemies

**Confirmed:** the [complete sixteen-row trigger/activation table](../../../../research/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md#all-sixteen-row-effects-and-the-extra-call)
joins every row to its stored primary/secondary nibbles and resulting word.
All fresh words remain unchanged: Bishop rows 2/12/13 keep `$3000`, the others
`$2000`, with primary/secondary-active bits clear. No region flag was set.

The table's changed-flag consequences can be read in these groups, covering
all sixteen rows (combatant ID = 128 + row):

| Relevant flag | Rows whose primary trigger uses it | Word change when that flag is held |
| --- | --- | --- |
|F93 / region 3|0,1,2|0/1: `$2000 -> $2001`; Bishop 2: `$3000 -> $3001`|
|F90 / region 0|4,10,11,12|4/10/11: `$2000 -> $2001`; Bishop 12: `$3000 -> $3001`|
|F92 / region 2|6,7,8,15|`$2000 -> $2001`|
|F91 / region 1|13,14|Bishop 13: `$3000 -> $3001`; row 14: `$2000 -> $2001`|
|Neither trigger (15/15)|3,5,9|No activation write|

A held non-15 primary wins and sets bit 0. Only an absent/clear primary lets
a held non-15 secondary set bits 0/1. Misses preserve the word. Primary
selection preserves other bits, including a previously held bit 1; it does
not reset a supplied secondary-active state.

Rows 0/1/2,7,10/11 and 15 have equal primary/secondary triggers and choose
primary when their flag is held. All remaining secondary triggers are 15.
Thus **no unchanged row has a secondary-only activation path**. Bishop12/13
store primary FOLLOW_ENEMY10/14 and secondary FOLLOW_ENEMY0, but those are
move orders; their secondary **trigger remains 15**, not region 0. Changed
trigger bytes can admit a secondary path. None of these names or bits proves
a tactic, target, movement, action or player/AI ownership.

### The extra slot is a conditional deals alias

After the 32-slot loop, d0.w=160 and d7.w=`$FFFF`. No return, branch or data
separates ActivateEnemies from its helper, so it runs once more for 160.
The accepted [address and packed-deals join](../../../../research/chapters/northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
shows the address helper rejects IDs **above** 160, admitting 160 itself:
`(160-96)*56 + $FFE800 = $FFF600`, CURRENT_GOLD beside DEALS_ITEMS.

Pseudo-X at `$FFF62E` is quantities 84/85; pseudo-HP at `$FFF60E` is ring 20–23
quantities; triggers at `$FFF636` are quantities 100/101; activation at
`$FFF634` is quantities 96–99. Nonnegative pseudo-X, nonzero pseudo-HP and a
held non-15 trigger flag 90–104 can OR bit 0, or bits 0/1 on secondary selection,
into Goddess Staff 99's count nibble, preserving other bits. Already held bits
need not change its count. This is no unconditional item addition, saturating
increment or demonstrated shop visit/persistence.

Fresh Battle27's clear flags prevent this write even if the pseudo-X/HP gates
pass. Changed allies/flags/deals can alter that conclusion; earlier per-enemy
writes are visible to the extra call. This guide consumes the accepted alias,
without extending it into a shop audit.

## No Battle27 region scene or fresh spawn request

**Confirmed:** the [complete region-hook table and actual wrapper](../../../../research/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md#region-hook-and-spawn-admission)
contain four records: battle 32 regions 0/1 share played flag 386; battle 40 region 0
uses 387 and region 1 uses 388. With CURRENT_BATTLE=27 throughout, the wrapper
skips every record and returns before checking played/region flags, setting
a played flag or dispatching MAPSCRIPT. This conclusion uses the complete
table, not a missing guessed Battle27 file. A changed current battle can match
a record; then the played flag is written **before** the script. Interruption
can leave it set before completion. Debug diagnostics follow the wrapper.

Spawn admission scans all 32 enemy slots and masks activation with `$0F00`.
All sixteen fresh rows are STARTING 0; absent slots also have mode 0. Region
activation changes only bits 0/1 and cannot turn mode 0 into a spawning mode.
Zero skips HP/region/position/reset/item calls. TARGETS_LIST_LENGTH becomes
zero; stale list bytes remain unconsumed. SpawnEnemyWithCamera is skipped,
so fresh preparation establishes no spawn animation, camera, item or text effect.

Changed modes use the accepted [spawn/reset consumers](../../../../research/chapters/northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#spawn-list-and-changed-state-conditions),
with Battle27's real row count 16:

| Masked mode | Admission before row reset |
| --- | --- |
|`$0200` hidden|Current HP 0 and a held non-15 trigger; then max HP must be 0.|
|`$0100` respawn|Current HP 0; no region requirement.|
|`$0300` both|Current HP 0 and a held non-15 trigger; no additional max-HP test.|
|Other nonzero modes|No matching append arm.|

The dead/trigger helper can set activation bits **before** a max-HP or reset
rejection. Reset uses bound `16 OR $80 = 144`, rejecting IDs 144–159 despite
their participation in the scan. A real row's starting X/Y is compared with
all 30 allies and 32 enemies, without HP, neutral or self exclusion. A dead actor
at its own start can block itself. Invalid row or occupied start writes
max/current HP 0 and X=-1, returns carry, and appends no ID.

Success initializes the real row's stats, position and items, then restores
its saved **full** activation word and returns clear carry through the setter
path. Battle27 has no upgrade; all row items are NOTHING, leaving copied
definition equipment rather than making a row-specific AddItem award.
Each success appends one byte ID in ascending scan order to the word-length
list. Later comparisons use returned full d1 without remasking; source
fall-through does not duplicate a hidden/respawn append.

A nonempty list is consumed only after construction, in stored order. Each
SpawnEnemyWithCamera call preserves outer traversal registers and requests
entity/camera/wait work, twelve orientation/Sleep3 iterations, sound, text 397
and closing. Services must return before turn generation. This order proves
no observed appearance, timing, completed presentation or AI action.

## Sixteen eligible enemies, with the first actor still unknown

**Confirmed:** the [complete current-stat table and constructor](../../../../research/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md#turn-eligibility-records-and-call-boundary)
join all sixteen enemies to current HP/AGI after definitions, equipment and
clear initial status. Lizardmen use AGI 38, Wyverns 36, Bishops 27, Soldiers 34,
Snipers 35 and Wizard 44. Their actual HP is positive; each supplies one record.
Difficulty affects ATT rather than this AGI join, and F527 does not upgrade 27.
Clear region-active bits do **not** exclude these turns. Names or AGI alone
cannot identify the first actor.

The constructor first clears 64 two-byte records at `$FFF71A` to `$FFFF`.
It offers ally slots 0–29, then enemy slots 128–157; **158/159 are excluded**
even if changed into living placed actors. Admission tests nonnegative
signed X and nonzero HP, with no Y, membership, neutral, region-active or
commandset test. Fresh 144–157 fail X. Supplied living placed nonmembers and
high-bit HP words can pass these predicates.

The accepted [score, sequential RNG and sorting derivation](../../../../research/chapters/northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#turn-buffer-randomized-agility-and-first-dispatch)
applies to these callers. For unsigned current rawAGI, let `A=rawAGI&127`
and R(n) be each successive direct RNG call:

- First score: `A + R(A>>3) - R(A>>3) + R(3) - 1`.
- For rawAGI>=128, immediately append the same actor again with
  `B=floor(A*5/6)` and score `B + R(B>>3) - R(B>>3)`.
- Store each low byte after word arithmetic. Singles consume 3 draws,
  doubles 5 and rejects 0, in ally-then-enemy slot order.

Every call advances `(seed*13+7)&65535`, including range 0; these ranges
return `floor(seed*n/65536)`. The turn caller uses the direct helper, not
the debug-direction override. Draws are coupled in sequence; no seed is
selected, enumerated or simulated for Battle27 here.

All sixteen fresh enemies have positive scores for every permitted draw,
using **48 enemy draws after the actual ally draws**. With N eligible
initialized allies and T doubled allies, total records are `16+N+T <= 40`
for the accepted twelve-position formation. Fewer/dead members reduce N.
Changed membership/positions/stats cannot inherit this bound without review.

Sorting covers the entire 64-record buffer: 62 passes of 63 adjacent comparisons,
signed-byte descending score, swapping only when the right score is strictly
greater. Ties preserve append order, including allies before enemies; a doubled
actor's second record was appended immediately after its first. Padding 255 is
signed -1, while wrapped 135 is -121. The accepted rawAGI 128 score 0/255 and
rawAGI 127 score 135 cases are fixture boundaries, not Battle27 observations.

There is no append-capacity guard. Changed many/doubled candidates can write
beyond 64 records, and 62 passes do not prove full sorting of arbitrary 64 entries.
The fresh maximum 40 fits, and its positive enemy scores bring a non-sentinel
record to the front within those passes. CURRENT_BATTLE_TURN, the byte just
after the 128-byte buffer at `$FFF79A`, is cleared. Offset 0 then reaches the
first call if prior services return. The actual actor still depends on the
party, current AGIs and incoming sequential RNG.

An empty admitted set leaves `$FF` first and repeats preparation; it does not
declare victory/defeat. In changed state, scores below -1 may sort behind padding.

## Supplied continuation and interrupted preparation

**Confirmed:** [F88 continuation](battle27-initialization-and-opening.md#f88-continuation-uses-supplied-battle-state)
restores seconds, clears 88/AI memory and view target, loads the battlefield,
then branches directly to `@IndividualTurns_Loop`. The following branch to
`@Start` is unreachable. Initially it bypasses **all** preparation above and
reads supplied turn/order bytes without alignment or range validation. A
non-FF byte reaches the call even if that ID would fail fresh admission;
the offset may be odd or outside the buffer. A sentinel instead enters
`@Start` with actual selectors and supplied flags/positions/HP/modes/AGI/RNG.
Incoming 74/27 does not repair current selectors. The predecessor's conditional
d0=74 out-of-list view lookup and missing physical mappings remain limits;
loading is no safe-state validation or byte-for-byte preservation promise.

| Changed premise | Boundary that still applies |
| --- | --- |
|F477 held or opening interrupted|A flag written before the scene does not prove a returned opening.|
|F88/current selectors/turn offset differs|Initial supplied dispatch and sentinel reentry differ; no automatic fresh repair.|
|Fewer/dead/changed/nonmember allies|Formation ordinal is not identity; geometry and turn HP/Y predicates differ.|
|Checked bits held or later coordinates changed|A remembered miss skips geometry; fresh persistent-flag reset does not clear checks.|
|Region flags/activation words/trigger bytes held or changed|Primary precedence, sentinel 15 and retained bit 1 remain; no per-round reset here.|
|Bishop12/13 orders changed or reinterpreted|Move-order FOLLOW_ENEMY0 does not make secondary trigger 15 into region 0.|
|Absent/neutral/high-bit-HP slot changed|Activation, geometry, spawn and turn bounds/predicates differ; no general neutral exclusion.|
|Deals fields changed|Slot 160's X/HP/trigger gates condition any packed-count write.|
|Debug byte nonzero or current battle changed|Diagnostics or a matching flag-before-script region hook can intervene.|
|Spawn mode/occupancy changed|Activation can precede rejection; reset can change HP/X or rebuild stats before append/services.|
|AGI/RNG/party changed|Record count, wrapped signed scores, buffer writes and first actor require actual inputs.|
|Interruption anywhere in preparation|Earlier checks/flags/activation/deals/list/stats/seed/buffer writes are not rolled back.|

Preparation is not atomic. **Unknown:** natural reach, actual party/class/HP/
items/flags/deals/checked word/current/saved state/seed, first actor, completed
services, timing, rendering/audio, usable control and persistence. No
ExecuteIndividualTurn body, AI/action choice, movement/damage, completed turn,
outcome, Battle32 or later story is established. Optional absent compressed
layout remains a no-decoded-route limit; regions prove no collision route
and authorize no acquisition.

## Evidence review and retained results

The full accepted [Battle27 activation research](../../../../research/chapters/northern-travel-mitula/battle27-activation-and-first-turn-dispatch.md)
is the **Confirmed static evidence owner**, accepted as
[#1037 / PR1039](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1039)
in `6334e76cf59e7f04ac871115d91a994f0c581e67`. Its pinned SF2DISASM revision is
`c834c652b6862bc5679fd7f69a38a7093206efc6`, bound to the USA retail ROM identity.
The [accepted opening guide](battle27-initialization-and-opening.md#evidence-review-and-retained-results)
and its Layer A owner supply the completed-fresh-entry premise. Addresses,
source paths, full tables, integer arithmetic and provenance remain owned there.

The pre-synthesis review read the full accepted evidence and changed-state
rows, the predecessor, the [lifecycle contract](../../../contracts/battle/battle-control-lifecycle.md),
[cutscene-routing contract](../../../contracts/battle/battle-cutscene-routing.md), selected
[placement/activation](../../../../research/chapters/grans-opening/battle01-placement.md) and
[RNG/turn evidence](../../../../research/battle/runtime-rng-and-battle-math.md#confirmed-statically-and-at-runtime-turn-order-score).
The contracts' compact “living/placed”
wording and older broad Unknown queues do not replace the deeper accepted
HP/X/Y/bounds/reset predicates. Prior Battle01 runtime and its geometry model
prove their named seams, not a general integer-edge model or natural Battle27
traversal. No blocking conflict was found; a future contradiction returns to
research rather than being silently repaired in Layer B.

| Public fixture ID / consumer | Reviewed join and limit |
| --- | --- |
|`sf2-battle-spriteset-data-static-v1`; [fixture](../../../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), [consumer](../../../../../src/sf2tool/h2/battle_spriteset_data.py)|27's 390-byte extent; `rowContentTracked=false` cannot prove full rows or geometry.|
|`sf2-battle-loop-static-v1`; [fixture](../../../../../tests/fixtures/h2/battle-loop-static-v1.json), [consumer](../../../../../src/sf2tool/h2/battle_loop.py)|Roster/memory/initializer source guards; no complete activation, spawn or turn simulation.|
|`sf2-battle-control-static-v1`; [fixture](../../../../../tests/fixtures/h2/battle-control-static-v1.json), [consumer](../../../../../src/sf2tool/h2/battle_control.py)|Actual fresh/resume/round order, reset 90–105 and sentinel 255; no checked-word-zero or safe supplied-offset guarantee.|
|`sf2-battle-cutscenes-static-v1`; [fixture](../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), [consumer](../../../../../src/sf2tool/h2/battle_cutscenes.py)|Region 8-byte/terminator/order/flag-before-script guards, distinct from completed dispatch.|
|`sf2-battle-routing-data-static-v1`; [fixture](../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json), [consumer](../../../../../src/sf2tool/h2/battle_routing_data.py)|Four region records and terminator, `routeTargetsParsed=false`; full 27 exclusion uses accepted complete table.|
|`sf2-battle01-region-activation-v1`; [fixture](../../../../../tests/fixtures/h3/battle01-region-activation-v1.json), [consumer](../../../../../scripts/Test-H3Battle01RegionActivationFixture.ps1)|Baseline checked 7/clear flags versus controlled hit; only Battle01's declared points and seam.|
|`sf2-battle01-secondary-activation-v1`; [fixture](../../../../../tests/fixtures/h3/battle01-secondary-activation-v1.json), [consumer](../../../../../scripts/Test-H3Battle01SecondaryActivationFixture.ps1)|Controlled trigger `$F2` and `$2060 -> $2063`; no natural secondary path for unchanged 27 rows.|
|`sf2-battle01-turn-order-v1`; [fixture](../../../../../tests/fixtures/h3/battle01-turn-order-v1.json), [consumer](../../../../../scripts/Test-H3Battle01TurnOrderFixture.ps1)|Seed `$1234`, nine records and tie order; no fixed Battle27 first actor.|
|`sf2-turn-order-boundaries-v1`; [fixture](../../../../../tests/fixtures/h3/turn-order-boundaries-v1.json), [consumer](../../../../../scripts/Test-H3TurnOrderBoundariesFixture.ps1)|Four controlled mutations, eight records, signed 135 and 0/255 scores; no full-capacity safety claim.|
|`sf2-rng-generate-random-number-v1`; [fixture](../../../../../tests/fixtures/h3/rng-v1.json), [consumer](../../../../../src/sf2tool/h3/rng.py)|Seven range 128 cases, seed/return/register comparison and direct RNG model; no Battle27 seed observation.|

Owning H2 commands are `uv run sf2 h2 battle-spriteset-data`, `battle-loop`,
`battle-control`, `battle-cutscenes` and `battle-routing-data`; runtime commands
remain with the linked consumers and `uv run sf2 h3 rng`. This slice inspected
actual payloads, their schemas and selected consumer branches without launching
those observations. Source and the retained 18,285,979-byte pair are unavailable
in this design environment: accepted 0.083s provenance only, no acquisition,
copying, regeneration or fresh private comparison.

[#1037 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1037#issuecomment-6183776676)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1039#issuecomment-6183789330)
retain author 0.340s, initial 0.329s, independent 0.332s and CI38676678592,
including the Region1 negative-projection/CCR prose correction. The
[#1038 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1038#issuecomment-6183815772)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1040#issuecomment-6183838094)
retain final author 0.389s/independent 0.393s/CI38677018042 and distinct earlier
heads/checks/CI/diagnostics. Their linked #1033 original/rebased results,
schema-path/local-$ref failures and earlier predecessor Unknowns remain
preserved. No completed suite or failure record is reset.

Standard rigor and the [current agreement](../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)
keep these state authorities separate for [#1042](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1042).
The handoff records complete prose/diff/changed-state review, direct public
fixture/schema/consumer checks, five-path/navigation/UTF-8/link/private checks,
clean committed planner and exact-head actual scoped CI. Documentation scope
skips blanket normal/full/Python/.NET/Godot/H3/H4/translation suites. Outcome
research #1041 and UI692 remain offlimits and unconsumed. No runtime queue,
engine/platform change, translation or later-story scope is added.
