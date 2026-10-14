# Hassan Departure, Battle17 Entry and Conditional Returns

Leaving Hassan can lead directly to a battle or to exploration in South Parmecia. Walking there can
request Battle17, the encounter named `BATTLE_TO_TAROS_SHRINE`, but an eligible Battle9 takes priority.
Winning Battle17 returns the group to a position derived from the current battle party; defeat or
Egress returns to Hassan. These are conditional source paths, rather than a required campaign order
or an observed journey to the shrine.

**Confirmed:** the accepted [research owner](../../research/hassan-departure-and-battle17-outcomes.md)
establishes the departure, request/admission joins, battle entry and immediate outcome consumers
explained here. **Inferred:** they support travel from Hassan toward the shrine. **Unknown:** natural
travel, supplied state, combat results, completed services/presentation, usable input and persistence.
Fnnn means flag nnn; coordinates are map tiles and table indices are zero-based. Source waits,
speeds and counters are parameters, not measured duration or movement.

Read [Battle16 outcomes through Hassan entry](battle16-outcomes-through-hassan-entry.md) for town
population and disembark limits, and [Hassan's conversations](hassan-ancestry-and-rohde-caravan-choice.md)
for Rohde's separate proposal. Neither conversation, the Achilles Sword nor Rohde's following is a
selected Battle17 prerequisite. This guide stops at battle admission/start or the immediate
conditional exploration/control return. Western Map68 travel, Taros/Battle18, Caravan operation,
Rick interactions and combat simulation are outside its scope.

## Leaving Town and Selecting a Battle

**Confirmed departure:** Hassan Map9's first warp row matches any X at Y0 and requests a no-scroll
transition to South Parmecia Map69 at `(26,54)`, facing Down. The other four rows are internal stairs
with different Y values. This row tests no battle/story flag or membership. Walking and actual raft
control can reach the warp producer, but the table alone proves neither a reachable layout marker nor
successful movement to it. The ordinary no-scroll consumer returns the requested tuple with d4=0;
it preserves the supplied transport mode and saved raft tuple.

MainLoop switches the map and scans for a battle **before** entering exploration. Map9/69 have no
selected flag-switched-map override. Only two battle-map rows use Map69, in this order:

| Candidate | Required unlock | Trigger and consequence |
| --- | --- | --- |
| Battle9 | F409 | Both trigger coordinates are wildcard255. It wins whenever eligible, including at the Hassan exit and when F417 is also set. |
| Battle17 | F417 | Both trigger coordinates are wildcard255. It wins when F409 is clear and F417 is set, regardless of the player's X/Y or Rohde's F418. |

The area rectangles supply battle coordinates: Battle9 origin `(0,0)`, size `48x32`; Battle17 origin
`(0,31)`, size `30x32`. They do **not** restrict player-position admission. On an eligible completed
battle, CheckBattle clears its unlock flag but still returns that battle index. F509/F517 therefore
do not suppress an already unlocked candidate. With both unlocks clear, the departure enters Map69
exploration. A Battle9 selection stops this guide at that admission.

Battle17 uses F417 unlock, F467 intro and F517 completion. Rohde's accepted proposal writes F418 for
Battle18, independently of those three flags. F418 alone does not admit17; F467 does not unlock it;
F517 does not establish the current Sword, party or follower state. Numbered battles are not an
enforced sequence at this selector.

## Walking Requests, Repeat Encounters and Actual Raft Control

**Confirmed walking route:** Map69's zone table matches X23, X24 or X25, any Y, and calls
`CheckRandomBattle` with battle17. A trailing default handler is separate. First-match dispatch means
that a matched zone which returns without requesting a battle does not then run the default.

The walking controller produces numeric type6 only through its working-layout `$1400` attribute
branch. It writes word X/Y derived from destination plus attempted tile delta, before the final
layout/debug/obstruction decision. Its successful movement tail decrements STEP_COUNTER only when
the signed word is positive. A listed coordinate is not itself a callback receipt, and the counter
is not a clock or a count of observed completed steps.

Type6 consumption clears pending, idles physical0 and dispatches its two coordinate words. The zone
wrapper closes text/portrait, waits a VInt and waits for physical0's current/destination alignment.
If another event is pending, WaitForEvent returns it; only pending zero reinstalls the current
transport controller. This is one actor's alignment boundary, not an all-actor or rendering barrier.

The helper reads the state supplied **when the callback runs**:

| State for Battle17 | Request/counter result |
| --- | --- |
| F517 clear | Request immediately, without testing the counter or drawing RNG. |
| F517 set, counter nonzero | Return without request or RNG. Negative words also satisfy this test. |
| F517 set, counter0, range8 draw0 | Request. |
| F517 set, counter0, range8 draw1–7 | Do not request; draw range4, add2 and store counter2–5. |

The direct RNG advances a 16-bit seed by `(13*seed+7) mod65536` and scales it to the supplied range.
Seed state, intervening callers and controller progress remain supplied. Range8 is not a measured
independent one-in-eight encounter probability; counter2–5 is not a fixed encounter deadline.

A requesting branch sets F417, writes the partial reload request, stores counter30000, waits for
view scrolling, requests boost SFX and executes the flash script. Operand30 becomes four quick-fade
calls, followed by palette duplication. Script termination conditionally waits for scrolling and
clears scroll speed. The flag/request writes can precede a stalled service; the flash neither loads
the map nor fills the remaining return parameters.

The **default** instead tests F509. If clear, it sets F409 and writes the same partial reload request;
if set, it returns. It performs no random helper, counter write or boost/flash sequence. Neither
callback clears the other battle's unlock, so requesting17 does not guarantee that17 wins the scan.

Actual raft mode2 uses opcode8 (`eas_Caravan`/`esc08_controlCaravan` despite those names). Its door,
warp, movement and numeric type5 landfall branches produce no walking `$1400` zone request and do
not decrement STEP_COUNTER. A zone-like raw tile can take landfall/nonmovement behavior. Sharing
coordinates with a walking zone does not run its callback. The separate
[disembark explanation](battle16-outcomes-through-hassan-entry.md#leaving-the-raft)
retains its own supplied-state and actor-wait boundary.

## What the Reload Actually Carries

**Confirmed byte boundary:** the long `$000100FF` write sets type1, parameter1=0 (no scroll) and
parameter2=255 (current map). It leaves parameter3/4 coordinates, parameter5 facing and WARP_SFX
unchanged. The named reload constant is this same value, not another dispatcher or a complete tuple.

The no-scroll consumer samples physical0's current position, then replaces each coordinate whose
requested byte is nonnegative and always takes parameter5 as facing. In the unchanged walking type6
chain, parameter3/4 retain the high/low bytes of the **zone Y word**. For ordinary Y0–63 these become
requested X0 and that Y, with supplied residual facing. Negative coordinate bytes preserve only
their respective sampled coordinates. The consumer still requests and clears retained WARP_SFX.
This is not a position/facing-preserving reload.

CheckBattle resolves map255 to CURRENT_MAP69. Its wildcard rows make the residual coordinates
irrelevant to candidate selection; fresh combat placement uses the selected spriteset. If neither
row remains eligible by consumption, the map-not-provided exploration branch updates existing
main-entity properties and declares the raft instead of rebuilding a supplied map's population.
That is a conditional fallback, not evidence that an ordinary F417 request loses its flag.

Exploration entry subtracts 20000 from the word counter and clears a negative result. An unchanged
30000 becomes10000 at eventual exploration entry; battle entry itself does not do that reduction.
Low retry counters clamp on exploration restart. Arbitrary word overflow is not a general saturation
rule. Pending state is mutable, so intervening writes remain part of the supplied-state boundary.

## Empty Scene Targets and Substantial Battle Entry

**Confirmed fresh entry:** admitted Battle17 clears PLAYER_TYPE to walking0, sets F399 and tests
saved-entry F88. With F88 clear it clears elapsed seconds, installs map69/battle17 and base VInts,
calls the before wrapper, restores map/battle, clears region flags90–105, heals eligible allies,
rebuilds ally/enemy battle positions, clears AI memory, loads the battle with view target d0=0,
then invokes the start wrapper.

All four actual slot17 targets are `ms_Empty`, the shared `$FFFF` terminator at `0x47A4E`:

| Hook | Work outside the empty target |
| --- | --- |
| Before | F467 set skips; otherwise execute without setting it. |
| Start | F467 set skips; otherwise set F467 **before** execution. |
| Defeated | Script dispatch needs Bowie alive, enemy128 dead and F517 clear; leader-tail behavior is separately selected. |
| After | F517 set skips the script, but both paths run the shared join tail. |

There is no selected Battle17 custom text, warp, actor table, inline action or transport write.
Execution still initializes the script reader/text-skip state and runs ordinary termination/scroll
work. Empty targets do not erase wrapper flags, service dependencies or the surrounding lifecycle.
F467 remains independent and is not reset by loss or repetition.

`BattleSpriteset17` at `0x1B43F2` supplies 12 ally placements, 14 STARTING enemies, two AI regions and
zero AI points. Its first ally placement `(24,23)` corresponds to world `(24,54)` with the area
origin; it is neither the departure `(26,54)` nor a fixed winning return position.

Ally initialization first clears all 30 X values to-1 and rebuilds current joined/active membership.
Dead members consume a party slot without consuming a placement row; members beyond placement
capacity can leave the active party. Twelve placements do not prove twelve living allies or Rohde's
participation. Shared healing restores living allies and immortal Peter/Lemon to HP/MP maxima,
retaining only existing STUN/POISON/CURSE bits; it does not revive other dead members or create
following. Battle membership and exploration-following remain separate authorities.

The 14 starting rows populate enemy128–141; other slots follow hidden/cleared initialization. Row0's
base enemy is SOULSOWER at `(1,10)`. Battle17 belongs to the repeat-upgrade set, and F517 admits the
shared upgrade algorithm, so base rows do not fix post-upgrade identities. Level/random results, AI,
turns, damage and winning trajectories are not evaluated here.

LoadBattle clears physical entities/lookups and declares actors from placed combatants, with the
neutral-enemy exclusion. It replaces the town/follower/raft mapping rather than preserving that cast.
Tiles, sprites, terrain, VInts, music and fades have their ordinary service dependencies. Start
returns before activation, region/spawn work or turn-order generation: no first playable turn is
observed.

**Confirmed saved entry:** F88 set is tested after mode0/F399. It restores saved seconds, clears F88
and AI memory, clears the view target, loads and enters the individual-turn loop directly. It skips
fresh before/start, healing/placement, incoming map/battle assignment and new turn-order generation,
and lacks fresh entry's explicit d0=0 before loading. Saved map, area, roster and turn state must
already agree; departure/reload repairs none of them. This branch proves no durable save/resume.

## End Conditions and Three Different Returns

**Confirmed static end rules:** after an individual turn, the defeated wrapper precedes killed-list
processing and survivor counts. Zero allies is checked before zero enemies; Bowie's HP0 forces zero
allies, and simultaneous zero counts choose defeat. Otherwise after-turn effects run, followed by
another killed-list/count and defeat-before-victory check, without a second defeated-hook call.
Counts require placed X and nonzero HP. Battle17's leader row is0, so enemy128's death does not enqueue
all other enemies for removal. Killing the initial SOULSOWER alone is not a victory condition.
Death services/bookkeeping still have to return; these rules predict no particular combat result.

| Outcome | Selected return and distinct mutations |
| --- | --- |
| Victory | Heal eligible allies, seed the return, run after/join tail, clear F417, set F517; d4=1. |
| Defeat | Update unlock, exhaustion text/music, restore Bowie's HP, halve gold by unsigned shift, select Hassan savepoint; d4=-1. Battle4's special loss does not apply. |
| Egress / Angel Wing | Consume selected spell MP or remove the item slot, request text/flash/closure, update unlock and select the same savepoint; d4=0. No defeat gold penalty or victory script/join/completion write. |

Egress describes an admitted masked action, not evidence of menu availability, current item/spell
custody or completed services. On first defeat/Egress F517 stays clear and F417 is retained; with
F517 already set, unlock updating clears F417. Neither branch resets F467 or writes Rohde's F418.

### First and Repeated Victory

The return seed uses CURRENT_MAP and the **first current active member** returned by rebuilt force
flags, that member's final combatant X/Y plus area origin, and its mapped facing. It is not
unconditionally Bowie or the first living member; inconsistent/empty lists and mappings are not
repaired. Under consistent Battle17 state it returns map69, `(localX,localY+31)` with byte arithmetic,
the supplied mapped facing and d4=1. Final combat position is supplied, not the initial placement.

First victory executes the terminator; repeated victory skips it. Neither changes that seed. Both
run the shared after tail: join row17 is0, so `JoinForce(0)` writes Bowie's joined flag, updates force
and conditionally activates him below 12 active members. It is neither a no-op nor a new Rohde/Rick
recruit. This occurs **after** seed selection and does not replace the return tuple. The victory
caller then clears F417 and sets F517.

There is no shrine warp, fixed western exit or Battle16-style raft-mode write. Mode normally remains0
from entry; saved RAFT_MAP/X/Y, F64, F418 and unrelated F409/F509 are unchanged by this outcome.
MainLoop switches the map and goes directly to exploration without CheckBattle. A supplied F409
therefore does not force an immediate battle on victory return. A later ordinary request can scan.

### Defeat, Egress and the Saved Raft

The selected battle17 egress helper chooses Hassan9, overriding carried EGRESS_MAP. With normal
battle-entry F399, its savepoint is `(8,9)`, facing Up. F64 set also writes the **saved** raft tuple
to map69 `(28,55)`; F64 clear leaves it unchanged. This invokes no boarding/landing scene. An abnormal
F399-clear call takes the earlier Bowie's-room fallback, outside the normal Battle17 chain.

Mode normally remains0 independently of that raft reset; the losing handlers do no separate mode
normalization. F467 remains set if already supplied, so a first-loss retry need not replay intro work.
A later Hassan departure scans again: F409 can win, otherwise retained F417 admits17 immediately.
A cleared F417 needs a new request. This establishes neither a natural retry nor later shrine travel.

## Fresh Exploration and Conditional Control

**Confirmed selected rebuild:** victory supplies map69, defeat/Egress supplies map9, and a departure
without battle supplies map69 at `(26,54)`, Down. Those supplied maps take fresh exploration
population, unlike the map255 fallback. Entry clears pending, reduces/clamps the counter, heals
eligible allies, loads tiles/map, writes CURRENT_BATTLE=255, selects setup, rebuilds entities/lookups,
clears temporary flags256–383, sets F80, runs init and conditionally requests music/fade. Other dead
allies are not all revived. Ordinary departure preserves its incoming mode; it has not run battle's
mode0 write.

Map69 has no setup residents and a plain-return init. Followers still depend on current flags,
with F65 selecting the overworld table. In normal mode0, a separate raft31 is declared only with
F64 set and saved RAFT_MAP69, at the saved coordinates, lookup63→31. Victory does not move/save it.
Independently supplied mode2 makes main0 a raft and hides followers; Battle17's empty after script
does not produce that mode.

Hassan's default/F518 alternate share init and the normal follower table. The
[accepted population/init explanation](battle16-outcomes-through-hassan-entry.md#hassans-cast-and-conditional-control)
owns its 11/12 setup rows and conditional hiding of logical137, Rohde11 and Rick12. Battle17 writes
none of those population flags. An F64-set losing return saves the raft in map69, so no separate
raft is placed in town9; F64 clear also fails its placement gate. Current battle membership does
not guarantee a current or freshly rebuilt exploration-following chain.

Only pending zero in WaitForEvent follows physical0 and installs the current mode's controller.
Service stalls or a new event can prevent that handoff. This stops at conditional control, without
claiming responsive input, all-actor/action completion, rendered/audio results or persistent state.

## Evidence Trace and Review Boundary

**Confirmed provenance:** accepted #836/PR838 research binds SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and the registered USA retail ROM. Its
[reproduction owner](../../research/hassan-departure-and-battle17-outcomes.md#reproduction-and-evidence-limits)
retains exact enum/table/pointer/parameter/caller joins. This synthesis consumes those accepted joins,
without source reinvestigation, payload regeneration or runtime acquisition.

| Explained boundary | Contract and existing fixture trace | Limit |
| --- | --- | --- |
| Ordered admission/savepoint | [map-entry routing](../contracts/map-entry-routing-state.md), [exploration flow](../contracts/exploration-control-flow.md); `sf2-common-maps-static-v1`, `sf2-gameflow-core-static-v1` in [maps](../../../tests/fixtures/h2/common-maps-static-v1.json)/[gameflow](../../../tests/fixtures/h2/gameflow-core-static-v1.json) | Helper/caller rules; selected Map9/69 and Battle9/17 rows are the accepted research join, not a natural route fixture. |
| Walking/default/raft and residual requests | [map exploration](../contracts/map-exploration.md); `sf2-map-init-static-v1`, `sf2-map-event-request-consumption-static-v1` in [init](../../../tests/fixtures/h2/map-init-static-v1.json)/[request](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json); [randomness](../contracts/randomness.md) | Shared setup/request facts do not execute this walking/raft/counter/seed chain or measure probability. |
| Empty hooks and shared tails | [cutscene routing](../contracts/battle-cutscene-routing.md); `sf2-battle-cutscenes-static-v1`, `sf2-battle-routing-data-static-v1` in [hooks](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)/[routing](../../../tests/fixtures/h2/battle-routing-data-static-v1.json) | Shared wrapper facts; actual four empty slot17 pointers, leader0 and join0 are selected research joins. Unused join data is not the executed join table. |
| Population and outcome rules | [battle lifecycle](../contracts/battle-control-lifecycle.md), [encounter definition](../contracts/battle-encounter-definition.md), [roster](../contracts/party-roster-state.md); `sf2-battle-loop-static-v1`, `sf2-battle-control-static-v1`, `sf2-battle-spriteset-data-static-v1`, `sf2-common-stats-static-v1` in [loop](../../../tests/fixtures/h2/battle-loop-static-v1.json)/[control](../../../tests/fixtures/h2/battle-control-static-v1.json)/[spriteset](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)/[stats](../../../tests/fixtures/h2/common-stats-static-v1.json) | Shared entry/healing/membership/count/return facts. Spriteset `rowContentTracked=false`; global header ranges do not independently prove Battle17's 12/14/2/0 or simulate combat/upgrades. |

The existing [routing](../../../schemas/h2-battle-routing-data-static-fixture.schema.json),
[hook](../../../schemas/h2-battle-cutscenes-static-fixture.schema.json),
[loop](../../../schemas/h2-battle-loop-static-fixture.schema.json),
[control](../../../schemas/h2-battle-control-static-fixture.schema.json),
[spriteset](../../../schemas/h2-battle-spriteset-data-static-fixture.schema.json),
[init](../../../schemas/h2-map-init-static-fixture.schema.json) and
[request](../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json) schemas retain
their payload boundaries. The [maps](../../../schemas/h2-common-maps-static-fixture.schema.json),
[gameflow](../../../schemas/h2-gameflow-core-static-fixture.schema.json) and
[stats](../../../schemas/h2/common-stats-fixture.schema.json) schemas own the corresponding shared
selector, outer-loop and membership facts. Their consumers are
[battle_routing_data.py](../../../src/sf2tool/h2/battle_routing_data.py),
[battle_cutscenes.py](../../../src/sf2tool/h2/battle_cutscenes.py),
[battle_loop.py](../../../src/sf2tool/h2/battle_loop.py),
[battle_control.py](../../../src/sf2tool/h2/battle_control.py),
[battle_spriteset_data.py](../../../src/sf2tool/h2/battle_spriteset_data.py),
[map_init.py](../../../src/sf2tool/h2/map_init.py),
[map_event_request_consumption.py](../../../src/sf2tool/h2/map_event_request_consumption.py),
[maps.py](../../../src/sf2tool/h2/maps.py), [gameflow.py](../../../src/sf2tool/h2/gameflow.py) and
[stats.py](../../../src/sf2tool/h2/stats.py).
Owning commands are `uv run sf2 h2 battle-routing-data`, `battle-cutscenes`, `battle-loop`,
`battle-control`, `battle-spriteset-data`, `map-init`, `map-event-request-consumption`, `common-maps`,
`gameflow-core` and `common-stats`. Controlled reproduction loads the worktree's ignored private-input configuration in the same process.

The synthesis review reads the accepted research and shared contract boundaries, selected tracked
intro/after/leader, fresh/saved/outcome, survivor/healing and spriteset-limit facts, plus their schema
and consumer selections. Aggregate facts support those shared rules; they do not independently
compose this route. No Layer A correction is needed to explain the narrower accepted joins.

The [PR838 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/838#issuecomment-6160960756)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/838#issuecomment-6160982826)
preserve original/rebased CI, retained comparisons and corrected/failed inspections, including guessed
scene-path failures and predecessor histories. They are not replayed solely for prose. The
[corrected #831 accounting](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/831#issuecomment-6160802747)
remains initial 13 plus repeat 2 Rohde texts; Battle17's empty scenes add no dialogue attribution.

**Unknown:** natural departure/zone/admission/return reach; actual flags, counter/seed, HP, roster,
mappings, request bytes and input; action/all-actor/service completion, rendering/audio, usable
control, continuous combat-to-exploration travel and save/resume persistence. These remain deferred
under [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) and
[ADR0016](../../decisions/0016-remake-start-evidence-deferral.md). They neither block this static content
explanation nor authorize an acquisition queue, later-story expansion or new remake capability.
