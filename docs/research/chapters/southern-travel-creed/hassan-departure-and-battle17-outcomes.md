# Hassan departure and Battle17 outcomes

Evidence date: 2026-10-13. **Confirmed:** Hassan's north exit requests South
Parmecia69 `(26,54)`, Down. Battle17 admission depends on the ordered battle-map
scan and F417, not Rohde's F418. Walking zone events can set F417 and request
another scan; actual raft control does not produce those zone events. Battle17
has empty before/start/defeated/after script targets, but substantial shared
initialization and outcome work. Victory returns to the current South Parmecia
battle position; defeat/Egress select Hassan's savepoint and conditionally reset
the saved raft tuple. **Inferred:** these mechanisms support the journey from
Hassan toward the shrine. **Unknown:** natural travel, supplied state, combat
trajectory, completed presentation/input and persistence.

## Agreement and provenance

Standard rigor under [Issue #836](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/836)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Accepted planning base is `76245c67cf4af9265fd67a78874942bb7368de1b`.
This document solely owns the selected departure, zone/request/admission join,
fresh/saved entry distinctions and conditional Battle17 outcome returns. There
are no shared-path writes or stacked dependencies. Combat, AI and damage
simulation, terrain/path reconstruction, western Map68 travel, Map12/Taros
scene/Battle18, Caravan operation and Rick interactions are excluded. Stop at
the resulting exploration/control or next conditional admission boundary.

Accepted dependencies are [Hassan entry](./battle16-outcomes-through-hassan-entry.md),
[Rohde's choice](./hassan-ancestry-and-rohde-caravan-choice.md),
[South Parmecia](./south-parmecia-through-kraken-battle16-entry.md),
[battle lifecycle](../../battle/battle-loop.md), [cutscene routing](../../battle/battle-cutscenes.md),
[request consumption](../../world/map-event-request-consumption.md),
[common scripting](../../world/common-scripting.md) and [membership](../../party/common-stats.md).
Source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind the pin and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Fnnn denotes flag nnn; table indices are zero-based. Coordinates, counters and
waits are source operands, not measured motion or elapsed-time observations.

The existing pair is 18,285,979 bytes; its linear comparisons were budgeted below
512 MiB and one minute and passed in 0.086 seconds. Selected source/fixture
reads and small prose/receipts require no payload rebuild, acquisition, new
environment or verification platform. No game-material network transfer occurs.
Reassess before runtime, corpus growth or a material scope/resource expansion.

## Departure and ordered admission

**Confirmed:** resolve `sf2enums.asm` first: `MAP_HASSAN=9`,
`MAP_OVERWORLD_SOUTH_PARMECIA=69`, `BATTLE_TO_TAROS_SHRINE=17`,
`MAP_CURRENT=255`, `DOWN=3`, `UP=1`, `PLAYERTYPE_BOWIE=0` and
`PLAYERTYPE_RAFT=2`. The flag bases are unlock400, intro450 and completed500,
so Battle17 uses F417/F467/F517. Rohde's accepted script writes F418 for
Battle18; no selected Battle17 caller tests that flag, the Achilles Sword or
Rohde's follower/membership state. Numbered battle order is not a prerequisite.

`data/maps/entries/map09/6-warp-events.asm` has five rows. Row0 matches any X
and Y0 and supplies no-scroll Map69, X26,Y54,Down. Rows1–4 are internal stairs
at `(6,16)`, `(3,57)`, `(11,26)`, `(14,57)` and cannot compete at Y0.
`exploration.asm:WarpIfSetAtPoint` scans in order and accepts the first match;
negative row coordinates are wildcards. The row does not test F417/F418/F517,
transport mode, membership or a prior town interaction.

Walking `entityscriptengine_2.asm:esc02_controlCharacter` reaches the producer
from a working-layout `$1000` branch after its movement checks. Actual raft
`esc08_controlCaravan` can also call it. The misleading action names are resolved
by `entityfunctions_2.asm:SetControlledEntityActScript`: mode2 uses
`eas_Caravan`/opcode8, while mode0 uses `eas_ControlledCharacter`/opcode2.
A table row does not establish the layout marker, occupancy, input or ability
to reach Y0 in either mode. Ordinary departure here means the no-scroll request,
not an assertion that every caller was walking.

The producer writes type1, parameter1=0, the map/position/facing tuple and warp
SFX. `explorationfunctions_2.asm:ProcessMapEvent` clears the pending word before
`ProcessMapEventType1_Warp`. Its no-scroll branch requests/clears warp SFX,
idles main0, removes the exploration return frame, samples physical0's current
position, replaces nonnegative requested X/Y and takes requested facing. It
returns `(69,26,54,Down,d4=0)` to `MainLoop/@Start`; it does not normalize
PLAYER_TYPE or RAFT_MAP/X/Y. The scrolling branch requires a different supplied
parameter1 and is not this row's ordinary consumer.

`code/gameflow/mainloop.asm` calls SwitchMap and then CheckBattle. The complete
`data/maps/global/flagswitchedmaps.asm` has no source-map69 or source-map9 row.
`code/common/maps/getbattle.asm:CheckBattle` scans the ordered table below,
checks the unlock flag and only the two trigger bytes, then selects the first
eligible row. Area width/height/origin do **not** test whether the player is
inside that rectangle.

| Battle-map row | Map | Area origin / size | Trigger X/Y | Admission consequence |
| --- | --- | --- | --- | --- |
| 9 | 69 | `(0,0)` / `48x32` | `255,255` | F409 set wins before row17, even at the Hassan exit and even with F509 set. |
| 17 | 69 | `(0,31)` / `30x32` | `255,255` | Selects if F409 clear and F417 set, regardless of X/Y, F418 or F467. |

These are the only map69 rows in `data/battles/global/battlemapcoords.asm`.
For an eligible row, CheckBattle writes its area globals, checks completion and
clears that row's unlock if completion was already set; it **still returns the
battle index**. Thus F517 does not suppress a supplied unlocked Battle17.
With neither F409 nor F417 set, departure enters ordinary Map69 exploration.
This immediate scan can admit a battle before any Map69 zone handler has run.
Selection of row9 stops this slice at that admission; its encounter is not replayed.

## Walking zones, random inputs and pending reload

**Confirmed:** Map69's setup has no alternate flag row. Its
`mapsetups/s3_zoneevents.asm` rows0–2 match X23/24/25 respectively, any Y, and
call `Map69_ZoneEvent0`, which supplies battle17 to `CheckRandomBattle`.
The trailing default calls `Map69_DefaultZoneEvent`. `RunMapSetupZoneEvent`
in `mapsetupsfunctions_1.asm` uses first-match order and byte comparisons;
`$FF` is wildcard and `$FD` selects the default. A matched callback returning
without a request does not fall through to the default.

Walking opcode2 tests the destination layout's masked `$1400` attribute and
writes numeric type6 plus **word** X at parameter1 and word Y at parameter3.
Those coordinates are the actor's destination plus the attempted tile delta,
divided by `MAP_TILE_SIZE=384`. The request write occurs before the final raw
layout/debug/obstruction branch and before destination/velocity updates. The
successful movement tail decrements STEP_COUNTER only when its signed word is
positive. This is a controller-tail count, not a time interval or proof that a
step actually completed. Counter state is sampled later by the callback.

Type6 consumption clears pending, applies the init action to physical0, reads
the two parameter words and invokes the setup handler. `RunMapSetupZoneEvent`
then closes portrait/text, waits a VInt and waits for physical0's current and
destination X/Y to agree. This is not an all-actor, action-pointer or renderer
join. When a handler has written another pending event, WaitForEvent returns
that event without reinstalling ordinary control; otherwise it installs the
current mode's controller. Pending state is mutable, not an atomic queue.

Actual raft opcode8 has door, warp, `$3C00` movement and numeric type5 landfall
branches. It has no `$1400` zone producer or STEP_COUNTER decrement. A zone-like
raw tile can therefore take its landfall/nonmovement path; the walking callback
cannot be assumed merely from sharing map coordinates. The existing
[disembark owner](./battle16-outcomes-through-hassan-entry.md#numeric-type5-tuple-save-disembark-and-restoration)
retains that conditional transition; no new vehicle mechanism is inferred here.

`mapsetupsfunctions_1.asm:CheckRandomBattle` has this exact decision order for17:

| Supplied state at helper entry | Work |
| --- | --- |
| F517 clear | Request unconditionally; do not test the counter or draw RNG. |
| F517 set, STEP_COUNTER nonzero | Return without request or draw. The check is zero/nonzero, so a negative word also suppresses it. |
| F517 set, counter0, range8 RNG result0 | Request. |
| F517 set, counter0, range8 result nonzero | No request; draw range4, add2 and store the low word in STEP_COUNTER. |

`code/common/tech/randomnumbergenerator.asm:GenerateRandomNumber` updates the
16-bit seed as `(13*seed+7) mod 65536` and returns the scaled result for d6=8 or4.
The observed instruction sequence gives values0–7 or0–3; the unsuccessful
repeat branch therefore stores2–5. It is the direct RNG, not the debug-choice
wrapper. Seed, intervening callers and controller progress remain supplied;
this is not a measured independent one-in-eight encounter probability or a
promise of an encounter after a fixed number of steps.

A requesting branch sets F417, writes long `$000100FF` at MAP_EVENT_TYPE,
writes counter30000, waits for view scrolling, submits SFX_BOOST and calls
`ExecuteFlashScreenScript`. The latter's actual body is opcode `$41`, operand30,
then `$FFFF`. `mapscriptengine_1.asm:csc41_flashScreenWhite` shifts30 right3,
uses DBF for four `LaunchFading` calls with `FLASH_QUICKLY_2`, then duplicates
palettes. Script termination conditionally waits for scrolling when the dialogue
window index is nonzero, clears scrolling speed and returns. Neither the flash
body nor those waits load the map or change the request tuple. F417 and pending
can be written before a stalled flash/scroll service returns.

The default callback instead tests F509. If clear it sets F409 and writes the
same long reload request; if set it returns without a request. It does not
write STEP_COUNTER, call CheckRandomBattle or perform the boost/flash sequence.
This can cause row9 to win even when F417 was already supplied. Neither callback
clears the other battle's unlock flag.

### Reload bytes and the next scan

**Confirmed:** `sf2const.asm` places MAP_EVENT_TYPE at `$FFA84A`, parameter1
at `$FFA84C`, parameter2 at `$FFA84D`, parameter3 at `$FFA84E`, parameter4 at
`$FFA84F` and parameter5 at `$FFA850`. The long `$000100FF` write therefore
sets type1, no-scroll parameter1=0 and map parameter2=255; it leaves parameters3–5
and WARP_SFX untouched. `MAP_EVENT_RELOADMAP` is that same numeric constant.
It is not a separate dispatcher or a newly populated return tuple.

The ordinary no-scroll consumer samples current physical0 position, but then
replaces each coordinate whose requested byte is nonnegative and always takes
parameter5 as facing. In the unmodified walking type6 chain, parameter3/4 still
hold the high/low bytes of the **zone Y word**. For ordinary Y0–63 this requests
X0 and Y equal to that zone Y; parameter5 remains supplied from earlier state.
A negative byte instead preserves the corresponding sampled coordinate. Thus
this source does not promise a position/facing-preserving reload. No new warp
SFX was supplied, although the consumer still submits and clears the retained
WARP_SFX value.

With map255, CheckBattle resolves CURRENT_MAP to69 before selection. Row9/17's
wildcard triggers make those residual coordinates irrelevant to **which battle**
is admitted; fresh combat positions come from the selected spriteset. If no row
is eligible by consumption time, d0 returns as255, and ExplorationLoop's
map-not-provided branch updates existing main-entity properties and declares the
raft rather than performing the supplied-map population rebuild. This distinct
conditional fallback is not a claim that the normal F417 request loses its flag.

No helper here reads F418 or guarantees its newly requested battle wins the
scan. Supplied flags, partial requests and intervening writes remain explicit.
ExplorationLoop subtracts20000 from STEP_COUNTER as a word and clears a negative
result. In the ordinary unchanged30000 case the eventual exploration entry leaves
10000; battle entry itself does not perform this reduction. Low retry counters
are clamped on an exploration restart. Arbitrary word overflow is not a general
saturating-counter contract.

## Selected before/start graph and battle population

**Confirmed:** for admitted17, fresh BattleLoop receives map69 and battle17,
clears PLAYER_TYPE to0, sets F399 and tests saved-entry F88. With F88 clear it
clears elapsed seconds, sets CURRENT_MAP/BATTLE, installs base VInts and invokes
the before wrapper. It restores map/battle afterward, clears region flags90–105,
heals living/immortal allies, initializes both rosters, clears AI memory, calls
LoadBattle with d0=0 and then invokes the start wrapper.

All four slot17 entries in `data/battles/cutscenes/` were selected by the resolved
battle index, not by guessed per-battle filenames:

| Table file | Actual target | Wrapper effect |
| --- | --- | --- |
| `beforebattlecutscenes.asm` | `ms_Empty` | F467 set skips; otherwise execute without setting it. |
| `battlestartcutscenes.asm` | `ms_Empty` | F467 set skips; otherwise set F467 **before** execution. |
| `battleendcutscenes.asm` | `ms_Empty` | Requires Bowie alive, enemy128 dead and F517 clear for dispatch; shared leader tail still has its own row. |
| `afterbattlecutscenes.asm` | `ms_Empty` | F517 set skips script, but both paths reach the join tail. |

`code/common/scripting/map/ms_empty.asm` at `0x47A4E` contains the shared `$FFFF`
terminator. ExecuteMapScript still initializes its reader/text-skip state and
uses the ordinary termination/scroll behavior. There is no selected Battle17
scene warp, text, custom actor table, inline action body or transport write.
An empty target does not make its wrapper, flag writes or service dependencies
empty. F467 is independent of F417/F517; a repeat or loss does not reset it.

`getbattlespritesetsubsection.asm` reads slot17 of
`data/battles/spritesets/entries.asm`, selecting `BattleSpriteset17` at `0x1B43F2`.
`spriteset17.asm` has 12 ally placement rows, 14 enemy rows, two AI regions and
zero AI points. The first placement is local `(24,23)`; area origin `(0,31)`
makes its world position `(24,54)` when consumed for a return. It is not the
Hassan exit `(26,54)` or the reload's residual X0.

`battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions` clears
all30 ally X values to-1, rebuilds membership from current joined/active flags
and uses BATTLE_PARTY_MEMBERS identities. Dead members consume a list/loop slot
without consuming a placement row; remaining members beyond capacity can leave
the active party. Twelve rows do not establish twelve living allies or Rohde's
participation. Shared healing restores living allies plus immortal Peter/Lemon
to HP/MP maxima, retaining only existing STUN/POISON/CURSE bits. It does not
revive other dead allies or supply membership/following.

All14 enemy rows are STARTING and supply slots128–141; the remaining slots in
the32-slot initialization follow the hidden/cleared path. Row0's base enemy is
SOULSOWER at `(1,10)`, not a special leader. The rows are initialization inputs,
not a claim of fixed post-upgrade identities: `InitializeEnemyStats` calls
`upgradeenemies.asm:UpgradeRandomBattleEnemies`; `randombattles.asm` includes17,
and DetermineBattleUpgrade admits upgrades when F517 is set. The shared upgrade
algorithm's level/random results are not evaluated here. AI regions/behaviors
are retained input, not simulated or proof of a turn/winning trajectory.

`battlefunctions/loadBattle.asm:LoadBattle` fades out, loads tiles, clears VInts,
then calls `entityfunctions_1.asm:PositionBattleEntities`. That rebuild clears
physical entities/lookups and declares actors from nonnegative placed combatant
coordinates, with an additional neutral-enemy exclusion. It does not preserve
Hassan's fixed cast, exploration followers or earlier raft lookup. LoadBattle
then selects d0's mapped view target, initializes sprites, loads map/mapsprites,
installs base VInts, loads terrain, requests map music and fade-in. The start
wrapper returns to `@Start` before activation, region/spawn work or turn order.
No first playable turn or completed service is claimed.

F88 set takes a different path **after** mode0 and F399: restore saved seconds,
clear F88 and AI memory, clear view target, LoadBattle and branch directly to
the individual-turn loop. It bypasses before/start, fresh healing/placement,
map/battle assignment from the incoming tuple and new turn-order generation.
It also lacks fresh entry's explicit d0=0 before LoadBattle. Saved map/battle,
area, roster and turn state must already agree; departure/reload is no repair
mechanism. The later branch to `@Start` on that path is unreachable.

## End conditions and first/repeated outcome returns

**Confirmed:** `battleloop_1.asm` calls the defeated wrapper after an individual
turn, then processes killed combatants and counts survivors. Zero allies is
tested before zero enemies. If neither ends battle, after-turn effects run and
another killed-list/count/defeat-before-victory check follows, without a second
defeated-hook call at that checkpoint. CountRemainingCombatants counts
nonnegative-X, nonzero-HP entries; Bowie's HP0 forces zero allies. Simultaneous
zero counts therefore choose defeat.

For Battle17 the defeated wrapper's HP gate can dispatch `ms_Empty` when F517
is clear, or skip it when set. Row17 of
`data/battles/global/enemyleaderpresence.asm` is **0**. Its shared tail therefore
does not append all remaining enemies on enemy128's death. Killing the initial
SOULSOWER alone does not establish victory. The killed-list consumer still
performs death services/bookkeeping and removes listed actors from placement;
none of this predicts a combat result or proves all services completed.

| Outcome consumer | Selected effects and return |
| --- | --- |
| `battleloop_2.asm:BattleLoop_Victory` | Heal eligible allies; seed map/position/facing parameters; execute after wrapper/join tail; clear F417, set F517; return tuple with d4=1. |
| `BattleLoop_Defeat` | UpdateBattleUnlockedFlag; exhaustion text/music; restore Bowie's HP; halve gold by unsigned shift; select Hassan savepoint; return d4=-1. The special losable Battle4 branch does not apply. |
| `battlefunctions_1.asm` Egress/Angel Wing | Consume spell MP or remove the selected item slot, request text/flash/closure; update unlock; unwind action frame; select the same savepoint; return d4=0. No defeat gold penalty or victory script/join/completion write. |

The Egress/Angel Wing branch is selected by the masked action identity in
`battlefunctions/executeindividualturn.asm`. This describes an admitted action,
not proof of menu availability, actual spell/item custody or successful service
completion. UpdateBattleUnlockedFlag clears F417 only if F517 was already set;
on a first defeat/Egress it leaves F417 unchanged. Neither losing branch sets
F517, resets F467 or writes F418.

### Victory

**Confirmed:** the seed is CURRENT_MAP and the first current member returned by
`UpdateForceAndGetFirstBattlePartyMemberIndex` in `textfunctions_1.asm`, followed
by that member's combatant X/Y plus area origin and mapped entity facing. This
is the first flag-rebuilt active member, not an unconditional Bowie/first living
member. Empty/inconsistent lists or mappings are not repaired here. The selected
member's final combat position is supplied, not the initial placement.

First victory (F517 clear) executes the terminator; repeated victory (F517 set)
skips it. Neither overwrites the seed. Under consistent selected Battle17 state,
both return map69, `(localX,localY+31)` with byte arithmetic, the mapped facing,
and d4=1. There is no fixed western-edge exit, shrine warp or Battle16-style
raft-mode write. PLAYER_TYPE remains the supplied mode, normally0 from entry;
RAFT_MAP/X/Y, F64, F418 and unrelated F409/F509 remain unchanged by this outcome.

Both paths execute `afterbattlecutscenesend.asm:EndAfterBattleCutscene`.
Row17 of `data/battles/global/afterbattlejoins.asm` is0, so `j_JoinForce(0)`
sets Bowie's joined flag F0, updates force and conditionally activates him when
the active count is below12. It is not a no-op or a Rohde/Rick recruit. This
happens **after** selection of the return member and does not replace its tuple.
Only then does the victory caller clear F417 and set F517.

MainLoop resumes at `alt_MainLoopEntry`, calls SwitchMap and goes directly to
ExplorationLoop, without CheckBattle. F409 or any other supplied unlock does
not cause an immediate battle scan merely because victory returns to69.
A later ordinary request can reach `@Start`; the same row priority applies.

### Defeat and Egress

**Confirmed:** `battleloop/getegresspositionforbattle.asm` specifically maps
battle17 to `MAP_HASSAN=9`, overriding carried EGRESS_MAP. Normal BattleLoop
already set F399. `code/common/maps/egressinit.asm:GetSavepointForMap` then
uses Hassan's `data/maps/global/savepointmapcoords.asm` row `(8,9)`, Up.
With F64 set, `raftresetmapcoords.asm`'s Hassan row writes saved raft map69,
X28,Y55. With F64 clear the saved tuple is unchanged. No landing/boarding script
is invoked. A direct abnormal F399-clear invocation takes the early Bowie's-room
fallback before the raft-reset scan; it is not the normal Battle17 call chain.

Both returns normally retain mode0 from battle entry, independently of whether
the raft tuple was reset. There is no separate losing-handler mode normalization.
First defeat/Egress leaves F517 clear and the current F417 intact; on repeated
loss/escape F517 stays set and F417 is cleared (the admission scan may already
have cleared it). F467 is retained, so a first-loss retry need not replay intro
work. Departure from Hassan later re-enters the same ordered scan: F409 can win;
otherwise a retained F417 admits17 immediately, while a cleared F417 needs a
new request. No natural retry path or subsequent shrine encounter is asserted.

## Immediate exploration rebuild and conditional control

**Confirmed:** the selected victory map69 and losing/escape map9 are supplied
map indices, so ExplorationLoop uses its fresh-population branch. It clears
pending, reduces/clamps the counter as above, heals eligible allies, fades/loads
tiles, writes CURRENT_MAP and CURRENT_BATTLE=255, obtains the current setup and
rebuilds entities/lookups, clears temporary flags256–383, sets F80, loads map,
runs init, conditionally requests music/fade and reaches WaitForEvent.
Death in defeat does not cause every dead ally to revive during this rebuild.
An ordinary Hassan departure with neither battle row admitted uses the same
supplied-map69 rebuild at `(26,54)`, Down, while preserving its entering transport
mode; it has not passed through BattleLoop's mode0 write.

Map69's setup entity list is empty and `s6_initfunction.asm:ms_map69_InitFunction`
is a plain return. Followers still come from current flags; F65 on this overworld
selects the overworld follower table. Fresh main0 is separate from the follower
list. In normal mode0, `followersfunctions_2.asm:sub_44404` declares separate
raft31 only if F64 is set and RAFT_MAP matches69; it uses the saved X/Y and maps
lookup63 to31. Victory does not save/move that tuple. Mode2, if independently
supplied, instead makes main0 a raft and hides followers; it is not produced by
the Battle17 after script.

Hassan uses default `ms_map9` or its F518 alternate, both sharing
`ms_map9_InitFunction`. The accepted [Hassan population/init owner](./battle16-outcomes-through-hassan-entry.md#fresh-hassan-population-init-and-conditional-control)
remains authoritative: normal follower table, 11 or12 setup rows, conditional
movement out of map for logical137 when F518 clear, Rohde11 when F65 set and
Rick12 when his joined flag is set. These selected init effects are not new
interactions. The Battle17 outcome sets none of those flags. A normal F64-set
loss resets RAFT_MAP to69, so the separate saved raft is not placed in town9.
F64-clear preserves its supplied tuple but also fails the placement flag gate.

With no pending event, WaitForEvent follows physical0 and installs the current
mode's controller. This is conditional control handoff, not observed usable
input, a completed all-actor action join, a natural move to an exit or a
save/resume guarantee. The story scope stops here for each outcome.

## Reproduction and evidence limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Verify the read-only source pin and clean status, then run the existing
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init payload comparison, `_verify_map_script_engine_fixture` and source
`_cutscene_facts` against `battle-cutscenes-static-v1.json`. All passed without
rewriting the retained inputs. No ROM extraction or runtime run is claimed.

Direct review joins the enum to Map9 row0, resolves map69 before selecting its
setup and both battle-map rows, and uses battle17 to select the four pointer
slots, leader/join rows, spriteset pointer/header and egress special case.
Read CheckRandomBattle, the zone/default caller, RNG, flash body, RAM parameter
layout and ordinary request consumer together. Review the selected battle entry,
roster/membership, outcome and exploration consumers above, including the
s05/s07/s13 jump-interface aliases; `j_JoinForce` resolves through s02.

| Existing fixture under `tests/fixtures/h2/` | Reuse boundary |
| --- | --- |
| `map-init-static-v1.json`, `map-script-engine-static-v1.json` | Existing retained comparisons pass. They do not become a natural Hassan-to-battle trace. |
| `battle-cutscenes-static-v1.json` | Existing source-facts comparison passes; direct pointer selection establishes the actual four empty targets. |
| `battle-routing-data-static-v1.json`, `map-event-request-consumption-static-v1.json` | Shared routing/request evidence; selected rows, byte effects and priority are direct joins here, not new executable composed-route coverage. |
| `battle-loop-static-v1.json`, `battle-control-static-v1.json`, `battle-spriteset-data-static-v1.json`, `common-stats-static-v1.json` | Reused lifecycle/population/membership owners; no combat, all-state or outcome simulation is added. |

Discriminating supplied states include both F409/F417 set; F417 with F517 set;
F418 without F417; F467 set on first completion; F88 set; walking versus raft;
F517 with zero, positive or negative counter; zero versus nonzero range8 draw;
residual request coordinates/facing; no eligible row at consumption; missing or
inconsistent active member/mapping; F64 clear or set; and interrupted flash,
scroll, death or map services. These constrain the prose and source review,
not a newly executed runtime matrix.

**Unknown:** natural zone/departure/return reach, actual flags/counter/seed,
HP/roster/mapping/request state, action and all-actor/service completion,
rendering/audio timing, usable input, continuous combat-to-exploration travel
and save/resume persistence. They remain deferred under
[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md), with no automatic
acquisition queue. A static empty scene does not close those limits.

The [accepted #832 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/833#issuecomment-6160733380)
and [#828 final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/828#issuecomment-6160569868)
preserve predecessor results, corrected inspections and linked original failures.
The main-gate's failed guessed Battle17 scene reads are not absence evidence;
the pointer-table joins above supply the positive evidence. Current inspection
failures and exact check/Git/CI identities belong in this Issue/PR handoff.

Acceptance is entire owned-document/source/provenance/link/private-boundary
review, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI.
No local blanket normal/full/engine/Godot/H3/H4 suite is selected for prose.
A clean pushed Draft PR is frozen for independent main-gate review and merge.
