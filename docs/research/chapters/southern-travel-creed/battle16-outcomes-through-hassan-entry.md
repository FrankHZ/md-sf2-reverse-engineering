# Battle16 outcomes through conditional Hassan entry

Evidence date: 2026-10-13. **Confirmed:** first Battle16 victory sets raft mode
and returns to South Parmecia69 `(29,51)`, Left. Defeat/Egress instead select
Polca13's savepoint, with a conditional raft reset. Numeric event5 saves the
raft tuple before disembark actions and only writes walking mode at the end.
The selected Map69 warp resolves Hassan to9, whose fresh population and init
depend on supplied flags. **Inferred:** these mechanisms support leaving the
Kraken encounter and making landfall near Hassan. **Unknown:** a continuous
natural route, actual caller state and completed presentation/input/persistence.

## Agreement and provenance

Standard rigor under [Issue #824](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/824)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
This owner consumes accepted base `7dc4358280e2ec1b042b39c834dd58f5187c4e13`,
[Battle16 entry](./south-parmecia-through-kraken-battle16-entry.md) and
[raft boarding / South Parmecia entry](./polca-raft-boarding-through-south-parmecia-entry.md).
It explains outcome selection, the complete after scene and numeric type5
consumer, then selected Map69→Map9 entry through conditional control. It does
not prescribe battle/town order or prove a travel path. Combat simulation, AI,
terrain/pathfinding, generalized vehicles, Hassan zone/entity/item/service
dialogue, priest events, Rick/Rohde interactions and later story are excluded.
There is no runtime acquisition, new verification platform or shared-file change.

Reused owners are [battle lifecycle](../../battle/battle-loop.md),
[battle cutscenes](../../battle/battle-cutscenes.md),
[prior outcome consumers](../eastern-parmecia/battle12-outcomes-through-bedoe-entry.md),
[request consumption](../../world/map-event-request-consumption.md),
[common scripting](../../world/common-scripting.md) and [membership](../../party/common-stats.md).
Source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only source checkout is clean at that pin. Retained map-init and
map-script-engine payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.

The unchanged retained pair totals18,285,979 bytes. Existing linear comparisons
were budgeted below512 MiB and one minute; they passed in0.084 seconds. New reads
are limited to the selected records and immediate consumers, with small prose
and receipts, no export/corpus growth or network acquisition of game material.
Widening into runtime, terrain or town interactions requires reassessment.
Fnnn means flag nnn; row and operation indices are zero-based. Coordinates are
operands unless an observation explicitly establishes a reached position.

## Outcome selection and return state

**Confirmed:** `code/gameflow/battle/battleloop_1.asm` calls
`j_ExecuteBattleCutscene_Defeated`, then `ProcessKilledCombatants`, then
`CountRemainingCombatants` after an individual turn. It tests zero allies before
zero enemies. If neither ends the battle, after-turn effects run, followed by
another killed-list/count/defeat-before-victory check; the defeated wrapper is
not called again at that second check. This is caller ordering, not a turn or
damage simulation.

`cutscenes/battleendcutscenesstart.asm` first requires Bowie HP nonzero and
enemy128 HP zero. It then tests F516 (`500+16`). With F516 clear, relative pointer
slot16 in `data/battles/cutscenes/battleendcutscenes.asm` selects `ms_Empty`;
with F516 set, it skips that script. Both continue to the leader tail in
`battleendcutscenesend.asm`. Row16 of
`data/battles/global/enemyleaderpresence.asm` is-1, so the tail appends every
nonzero-HP enemy to the dead-combatant list. The first enemy row of
`data/battles/spritesets/spriteset16.asm` is the Kraken head, establishing which
initialized combatant occupies128; the predicate itself tests HP, not sprite
identity or an enemy name.

`battleloop/processkilledcombatants.asm` consumes that list through death
presentation/stat bookkeeping and marks each listed combatant's X as-1.
`countremainingcombatants.asm` counts nonnegative-X, nonzero-HP allies/enemies;
Bowie HP0 additionally forces the ally count to0. Thus leader defeat can remove
remaining limbs from the count without proving their HP was zero. Simultaneous
zero counts select defeat. Missing/stalled death services are not a completed
victory receipt, and a head dying only during after-turn effects does not cause
an extra defeated-hook call at that same checkpoint.

The actual outcome consumers are `battleloop_2.asm`,
`battlefunctions/battlefunctions_1.asm`, `battleloop/getegresspositionforbattle.asm`
and `code/gameflow/mainloop.asm`:

| Outcome | Ordered effects and returned tuple |
| --- | --- |
| Victory | Heal living/immortal allies; seed parameters2–5 from current map, the first member selected after `UpdateForceAndGetFirstBattlePartyMemberIndex`, its combatant X/Y plus battle-area origin and its mapped facing. Zero parameter1; execute after wrapper; clear F416; set F516; read parameters2–5 into d0–d3; return d4=1. The seed is not necessarily Bowie's position. |
| Defeat | `UpdateBattleUnlockedFlag`; exhaustion text363/music; restore Bowie's HP; halve gold by unsigned right shift; select egress tuple; return d4=-1. Battle4's losable exception does not apply. No new F516 or after scene. |
| Egress / Angel Wing | The action selector recognizes Egress spell or Angel Wing item after masking its entry index. The action consumes MP or removes the used slot, requests text/flash services, calls `UpdateBattleUnlockedFlag`, unwinds the action frame and returns the egress tuple with d4=0. No defeat gold penalty or victory completion. |

`UpdateBattleUnlockedFlag` clears F416 only when F516 was already set. First-loss
or first-Egress leaves that unlock unchanged. Neither it nor the Battle16 after
scene changes the independent introduction/start flag F466. Shared healing
restores living allies and immortal Peter/Lemon to HP/MP maxima, retains existing
STUN/POISON/CURSE bits and does not revive other dead allies or supply membership.

For Battle16, `GetEgressPositionForBattle` explicitly supplies `MAP_POLCA=13`,
ignoring carried `EGRESS_MAP`, before `code/common/maps/egressinit.asm:GetSavepointForMap`.
Normal BattleLoop entry already set F399; the savepoint table therefore supplies
Polca `(18,25)`, Up. If F64 is set, `data/maps/global/raftresetmapcoords.asm` matches
Polca and writes `RAFT_MAP=MAP_OVERWORLD_BEDOE_KINGDOM=72`, X43,Y48. If F64 is
clear, that tuple remains supplied. A direct malformed F399-clear invocation
takes the pre-first-battle room fallback before the raft-reset scan; it is not
the normal Battle16 caller. This slice stops the losing/escape alternative at
the return selection, without following Polca's scene or dialogue.

Normal BattleLoop cleared PLAYER_TYPE to0 on entry. Defeat/Egress do not replace
it with raft mode; their ordinary return is consequently mode0, independently
of the saved raft tuple. This is conditional on the normal entry and no intervening
state mutation, not a new normalization in either outcome handler.

## Complete after scene and fresh South Parmecia return

**Confirmed:** `cutscenes/afterbattlecutscenesstart.asm:ExecuteAfterBattleCutscene`
tests F516 before dispatch. With it clear, after-pointer slot16 selects
`abcs_battle16` at `0x4B1BC`, in
`data/battles/entries/battle16/cs_afterbattle.asm`. Its complete retained program
has three records, source lines4–6:

| Record | Work and immediate consumer |
| --- | --- |
| 0 | `executeSubroutine csub_4B1D0`: `csc0A_executeSubroutine` calls exactly the body at `0x4B1D0`, which writes `PLAYERTYPE_RAFT=2` to PLAYER_TYPE and returns. It does not call neighboring `csub_4B1CA`, which clears mode. |
| 1 | `warp MAP_OVERWORLD_SOUTH_PARMECIA,29,51,LEFT`: enum→69; macro opcode7→`mapscriptengine_2.asm:csc07_warp`. Write pending type1, parameter1=0, then map69,X29,Y51,Left. No map load, actor movement, saved-raft-tuple write or warp-SFX write occurs in this command. |
| 2 | `csc_end`: ordinary script return; the shared terminator conditionally waits for view scrolling when the dialogue-window index is nonzero and clears scroll speed. No text or action program is in this scene. |

The pending request is a mutable word plus parameter bytes, not an atomic queue.
Here `BattleLoop_Victory` consumes its **tuple** after the wrapper, rather than
calling the ordinary `ProcessMapEventType1_Warp` exploration consumer. It returns
`(69,29,51,Left,d4=1)`. `MainLoop` resumes at `alt_MainLoopEntry`, calls SwitchMap,
and goes directly to ExplorationLoop; it does **not** run CheckBattle at that
return. No switch-table source map resolves to69. Map69's battles9/17 are therefore
not immediately scanned merely because this victory returned there.

Both wrapper branches reach `afterbattlecutscenesend.asm:EndAfterBattleCutscene`.
The selected `table_AfterBattleJoins` row16 is0; `j_JoinForce(0)` sets F0, updates
force and conditionally joins Bowie to the active party if its count is below12.
It is not an empty no-op, an ally recruit other than Bowie or a raft write.
The initial return member was selected before this tail.

| Supplied F516 at wrapper entry | Result after ordinary handler completion |
| --- | --- |
| Clear | Run all three records, set mode2, replace the return tuple with69/29/51/Left, then clear F416 and set F516. |
| Set | Skip all three records; retain the seeded map49/variable position/facing and supplied mode (normally0). Still perform the join tail and victory flag writes. There is no repeated raft-mode write or fixed South Parmecia return. |

The repeat Map49 fallback is the accepted predecessor's `ms_Void` exploration
path, not a new Map49 scene. Interruption after the mode write but before warp
can leave mode2 with the old tuple; interruption after warp but before completion
can leave F516 clear. F516 set proves neither that the three records just ran nor
that their map/input services finished. No instruction here saves RAFT_MAP/X/Y.

On first return, supplied map69 causes `explorationfunctions_2.asm:ExplorationLoop`
to clear pending, reduce/clamp the step counter, heal living/immortal allies,
fade/load tiles and rebuild entities. It sets current map69 and current battle255,
clears temporary flags256–383, sets F80, loads the map and runs the selected init.
Map69's `mapsetups/pointertable.asm` uses an empty fixed-entity list and
`ms_map69_InitFunction`, a plain return. Ordinary conditional map music/fade and
`WaitForEvent` follow. This fresh population does not retain the defeated battle
actors or their lookup table.

`followersfunctions_2.asm:sub_44404` sees mode2, makes main0 use the raft sprite,
puts the current exploration-list actors at `(64,64)` with `eas_Standing`, and
declares physical raft31 at `(64,64)`, Left, with lookup63→31. This mode2 branch
does not require F64 or RAFT_MAP=69. It does not update RAFT_MAP/X/Y. Fresh
followers come from current flags; F65 plus overworld69 selects the overworld
follower table. Neither population nor healing establishes a particular roster.
With pending zero, `WaitForEvent` installs the mode2 controller on physical0.
Actual fade/action/update/input completion remains Unknown.

## Numeric type5: tuple save, disembark and restoration

**Confirmed:** the accepted producer is
`entityscriptengine_2.asm:esc08_controlCaravan`, selected for mode2 by
`SetControlledEntityActScript → eas_Caravan`. At this pin
`MAP_EVENT_GETOUTOFCARAVAN=5`; the similarly named GETOUTOFRAFT equals4. Numeric5
dispatches through `ProcessMapEventType5_GetOutOfRaft → j_MapEventType5` (s07 jump
interface) to `code/common/scripting/map/vehiclefunctions.asm:MapEventType5`.

After its input/bounds/current-actor obstruction checks, opcode8 handles masked
`$0400` door, `$1000` warp and `$3C00` movement branches first. Otherwise a raw
layout word unsigned below `$C000` requests5 and skips the movement tail; words
at least `$C000` skip movement without requesting it. No new F64, raft-coordinate,
membership or battle-flag check appears there. The attempted facing can already
have changed. The producer does not advance the destination on the type5 branch.
Layout/occupancy/input and actual travel progress are supplied, not proved by
the selected warp table.

`ProcessMapEvent` clears pending before the consumer. Unlike boarding's
`sub_454AC`, `MapEventType5` has **no alignment/admission rejection**. Its complete
order is:

1. `sub_454E4` reads physical0's **destination**, sign-extends each word and divides
   by `MAP_TILE_SIZE=384`; write current map byte and low quotient bytes to
   RAFT_MAP/X/Y. This saves the departing raft location before the new forward
   movement, not its landing point or physical31's position. It is a RAM tuple
   update, not proof of disk/SRAM persistence.
2. Idle physical0, set `byte_FFAFB0=1` (sprite-link ordering state), run `cs_45470`.
3. Install `byte_45488` on the current EXPLORATION_ENTITIES list; call
   `WaitForHeroAndFollowersStopped`.
4. Call `InitializeFollowerActscripts`; finally write `PLAYERTYPE_BOWIE=0` and return.

`cs_45470` at `0x45470` has four complete retained records, source lines191–194:

| Record | Work and action body |
| --- | --- |
| 0 | Waited logical FOLLOWER_D159 `eas_45360`: clone **physical0**'s position, destination, travel and facing, refresh its own sprite, branch to idle. Normal lookup63→physical31 places the separate raft at the departing position. |
| 1 | `setSprite ALLY_BOWIE,ALLY_BOWIE`: `csc1A_setEntitySprite` resolves logical0, gets Bowie's current ally mapsprite, writes it, waits a VInt and refreshes the sprite. It does not change PLAYER_TYPE yet. |
| 2 | Waited Bowie `eas_4548C`: `ac_moveFacRelPos 0,1`, warp SFX, own destination wait, branch to idle. The movement uses the actor's current position/facing to request one forward tile. It does not clone the raft. |
| 3 | Ordinary `csc_end`, with the shared termination behavior above. |

`sf2cutscenemacros.asm` encodes those action operations as `$09`, `$23` and `$01`;
`entityscriptengine_2.asm:esc09_moveToFacingRelativePosition` uses facing-relative
offsets and enters `loc_55C8` to set destination/travel/velocity. It does not repeat
the controller's layout/occupancy checks. `esc01_waitUntilDestination` waits for
both current/destination differences to vanish. `esc0D_clonePosition` uses a
physical index directly and copies no membership, HP, sprite identity or mapping.
Neither clone nor sprite replacement resets every action/velocity field.

The follower body `byte_45488` first clones physical `ENTITY_RAFT=31`, then falls
through to the same `eas_4548C` forward move/SFX/destination-wait/idle body. Under
consistent mappings, settled departure coordinates and normal facing, Bowie and
listed followers move one tile away while the separate raft remains behind.
That composition is **Inferred** as a successful landfall until those conditions
and services complete. There is no extra wait6 in this exit body, unlike boarding.

Logical FOLLOWER_D159, physical raft31 and logical FOLLOWER_B31 are distinct.
The waited map commands resolve lookup63 or lookup0 and poll that mapped actor's
action pointer for `eas_Idle`; they have no ally-HP guard. `setSprite` likewise
does not skip Bowie at HP0. Missing/aliased mappings can target main0 or another
actor, while follower clones still read physical31. No validity check repairs
the disagreement. The consumer does not heal or revive actors.

`entityfunctions_3.asm:ApplyActscriptToHeroAndFollowers` iterates physical list
bytes to `$FF`. Fresh `InitializeMapEntities` populated this list with followers
starting at physical1 and declared main0 separately, so its name does not prove
main0 is listed. Supplied/current list contents govern. The wait returns at once
for an empty list, otherwise waits on **only the last listed actor's** idle
pointer after VInts. It is not an all-follower completion barrier.

Restoration uses a different authority: `InitializeFollowerActscripts` scans the
current flag table (overworld variant only with F65 and an overworld map) and
writes follow-action pointers into consecutive physical slots from1. It neither
rebuilds the exploration list nor resolves each logical mapping. Stable fresh
flags/population make these authorities agree; changed flags/lists can make them
diverge. Earlier followers may have their in-progress exit actions overwritten
when the last actor finishes. The final mode0 write follows restoration, not a
proof that every actor landed.

Partial/interrupted execution can therefore leave the tuple saved, raft actor
copied, Bowie sprite restored or followers moved while mode remains2. On later
fresh mode0 population, the separate raft is declared from the saved tuple only
if F64 and RAFT_MAP match the map. Boarding still needs the accepted layout/F64
producer and physical destination alignment; neither type5 completion nor the
saved tuple automatically requests reboarding. No Caravan consumer is generalized.

## Selected Map69 to Hassan request

**Confirmed:** `data/maps/entries/map69/6-warp-events.asm` row2 matches `(26,55)`,
is no-scroll, and supplies `MAP_HASSAN → 9`, `(14,1)`, Down. The enum is resolved
before selecting `map09` owners. Earlier rows0/1 match `(28,51)` and `(43,31)`;
later rows3/4 match `(44,7)` and `(41,15)`; rows5–7 match X0, X63 or Y0 with the
other coordinate wildcard. None competes at26,55. `exploration.asm:WarpIfSetAtPoint`
uses first-match priority, interpreting negative row coordinates as wildcards.

Both ordinary walking opcode2 and actual raft opcode8 can call that producer
from the working-layout `$1000` branch after their own movement checks. Opcode8
takes this branch before its type5 test and still follows its movement tail.
The row/producer does not test F516, F518, F64, membership or PLAYER_TYPE. Therefore
disembarking is not an explicit prerequisite of this selected request. This does
not establish that natural water/land geometry lets either supplied state reach
the marker. The victory return at29,51 is not an input/path receipt to26,55.

The producer writes numeric type1, no-scroll parameter1=0, the tuple and warp SFX.
This time ordinary exploration **does** consume it: clear pending, request/clear
warp SFX, idle main0, remove the exploration return frame, sample current player
coordinates, replace nonnegative X/Y, take supplied facing, then return
`(9,14,1,Down,d4=0)` to `MainLoop/@Start`. PLAYER_TYPE and RAFT_MAP/X/Y are unchanged.
A separately supplied nonzero parameter1 would choose the scrolling path, not
this row's ordinary consumer.

`SwitchMap` has no source-map9 entry. The entire ordered
`data/battles/global/battlemapcoords.asm` has no map9 row, so `CheckBattle` returns
no admission for this selected map regardless of supplied unlock flags. A
Hassan visit does not itself enter Battle17/18. Those battles, alternative exits
and any subsequent input are outside this boundary.

## Fresh Hassan population, init and conditional control

**Confirmed:** the supplied-map ExplorationLoop rebuild described above now uses
map9 and the warp coordinates14,1. It preserves transport mode, heals only the
shared eligible allies, clears/rebuilds physical entities and all64 lookup bytes,
clears temporary flags and sets F80. `data/maps/mapsetups.asm` chooses default
`ms_map9` or its sole F518 alternate `ms_map9_flag518`. Their pointer tables differ
only in entity-list pointer and share `ms_map9_InitFunction` at `0x568C4`.
The selector and init re-read current flags; a changing supplied flag need not
represent one coherent snapshot.

Map9 is neither an overworld nor a follower-exclusion map. Current follower flags
use the normal table even with F65 set. Population declares followers, setup
actors, then main0; ally rows reuse an existing nonzero ally lookup instead of
declaring a duplicate. These are flags/mapsprites, not a general party-membership
test. No entry operation joins Rick or Rohde.

The selected files are `map09/mapsetups/s1_entities.asm` (11 setup rows) and
`s1_entities_518.asm` (12). Each has ten non-ally rows mapped consecutively to
logical128–137; ally rows do not consume that sequence. Both contain Rohde11 at
`(16,59)`, Down, `eas_Init`; the alternate additionally contains Rick12 at
`(12,3)`, Right, `eas_Init`. The last non-ally row in each is merchant137 at
`(16,3)`, Down. The alternate also changes a warrior's position/walking range,
a priest's facing/action and a young man's position/walking origin. This small
cast selection establishes init targets, not a town dialogue/service inventory.

The complete init's13 retained operations are three ordered flag/call groups
and return. `mapsetupsfunctions_1.asm:MoveEntityOutOfMap` resolves via
`j_GetEntityIndexForCombatant`, then `j_SetEntityPosition` writes `$7000` to X/Y
**and destination**. It does not call HideEntity, delete a lookup/list entry,
change HP/membership/flags, stop actions or clear travel/velocity.

| Operations | Predicate and target | Fresh-state meaning and supplied-state limit |
| --- | --- | --- |
| 0–3 | If F518 clear, move logical137 out of map. | This is the last merchant row in both casts, not Rick. F518 set leaves it alone. |
| 4–7 | If F65 set, move `ALLY_ROHDE=11` out of map. | F65 is the Caravan-unlock flag, not Rohde's membership flag. With F84 following, the existing follower lookup suppresses the fixed Rohde duplicate; init moves that mapped follower. Otherwise it targets the setup actor. |
| 8–11 | If F12 set, move `ALLY_RICK=12` out of map. | F12 is the joined flag; there is no new JoinForce call. Rick is declared only by the F518 alternate and is absent from the normal follower table. Fresh F518-clear/F12-set state leaves lookup12 zero, so this call addresses physical0. It does not safely ignore the missing actor. |
| 12 | Return. | No dialogue, warp, savepoint selection, new battle unlock/completion or transport write. |

The mapped-index helper subtracts96 for enemy-form bytes then indexes the lookup;
it does not validate actor presence or HP. These consequences explain why supplied
flags, absent actors, moved actors and membership must remain separate. `$7000`
is an internal off-map position, not an observed permanent disappearance; action
state remains live. No natural occurrence of the inconsistent Rick flag case is
claimed.

Transport is independently processed before init. With mode2, `sub_44404` gives
main0 the raft sprite, hides listed followers at64,64 with standing actions and
declares physical31 there, regardless of F64/RAFT_MAP. With mode0, main uses
Bowie's sprite and a separate raft appears only for F64 plus RAFT_MAP=9; a tuple
saved on69 does not satisfy that test. Neither path rewrites the saved tuple or
normalizes mode. An init call can subsequently move a mapped actor as above.

After init, the ordinary palette-dependent music/fade branch and `WaitForEvent`
follow. If no pending event intervenes, control targets physical0 and selects
`eas_ControlledCharacter` for mode0; mode2 selects `eas_Caravan`/opcode8 again.
Mode1 or other supplied values retain the shared selector's own behavior and are
not normalized by this town. A returned init with an off-map main, incomplete
services or inconsistent population is not proof of usable walking control.
Stop before processing any town player action, zone, entity, item or service.

## Reproduction and evidence limits

Load `. ./local/private-inputs.ps1` in the command process, verify the read-only
pin/clean source, then run the existing
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
It compares map-init and map-script-engine fixtures to the unchanged retained
payloads and `_cutscene_facts` to `battle-cutscenes-static-v1.json`. No new ROM
verification/extraction or runtime receipt is claimed.

Direct selections compare all sourceLine/macro/argument records of
`programCorpus.programs[id=abcs_battle16]` (3) and `cs_45470` (4), plus all13
`primarySourceBodies[sourceOwnerSymbol=ms_map9_InitFunction]` operations and its
two route/pointer joins. Read the called subroutine/action bodies separately;
they are not additional top-level scene records. Resolve enum operands before
selecting map, savepoint, reset and battle rows. Check all eight Map69 warp rows,
Battle16's actual leader/defeated/after/join slots, absence of map9 battle/switch
rows, and both setup population lists. The immediate callers, numeric dispatch,
s07 aliases and consumers named above complete the bounded source review.

| Existing fixture under `tests/fixtures/h2/` | Reuse and boundary |
| --- | --- |
| `map-script-engine-static-v1.json`, `map-init-static-v1.json` | Retained comparisons passed; selected7 scene records,13 init operations and both init joins agree with source. Vehicle helper/action bodies were reviewed separately. |
| `battle-cutscenes-static-v1.json`, `battle-loop-static-v1.json`, `battle-control-static-v1.json`, `battle-global-data-static-v1.json`, `battle-routing-data-static-v1.json` | Shared outcome/lifecycle/data owners; cutscene-source facts comparison passed. Direct row16 and savepoint/reset joins do not become a composed victory/escape fixture. |
| `map-event-request-consumption-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json`, `common-stats-static-v1.json` | Shared request, selection, population and membership contracts. No per-Hassan natural-input, transport or town-interaction acceptance is added. |

Discriminating supplied states include F516 already set; Bowie/head simultaneously
dead; head loss only in after-turn effects; F64 clear on escape; destination/current
position disagreement at disembark; empty or changed follower list; missing raft
mapping; earlier followers still moving; F65/F84 changes; F518-clear/F12-set; mode2
at the Hassan warp; nonzero supplied warp parameter1; pending events and stalled
services. These constrain the source-level conclusions, not a new executed matrix.

**Unknown:** natural victory/defeat/Egress timing, travel/landing, actual mode,
flags/HP/membership/mappings/lists/layout/input, all-actor and service completion,
original rendering/audio, usable town input and save/resume/persistence. Existing
aggregate fixtures are not composed or per-case runtime coverage. These limits
remain deferred under [ADR 0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR 0016](../../../decisions/0016-remake-start-evidence-deferral.md), not an automatic
acquisition queue.

Preserve [PR822's accepted checks and inspection/rejected-command history](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/822#issuecomment-6160096072),
[PR818's failed initial destination join](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159788942)
and its [corrected join/final CI](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159852108).
They retain their original statuses; no ritual replay or replacement of failure
evidence follows from this document.

Acceptance is full owned-document/source/provenance/link/private-boundary review,
`git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI.
Documentation-only scope selects no blanket local normal/full/engine/Godot/H3/H4
suite. Exact candidate/check/failure/process state belongs in the Issue/PR handoff;
independent acceptance and integration remain main-gate's.
