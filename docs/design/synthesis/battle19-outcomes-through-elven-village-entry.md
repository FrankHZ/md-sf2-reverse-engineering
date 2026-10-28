# Battle19 Outcomes and Independent Elven Village Entry

Battle19 can end without a battle-specific scene and still change party, battle and return state.
A victory returns the party to Southwest Parmecia at a position selected from the current active
roster. A loss or admitted escape uses the supplied savepoint. Elven Village has its own entrance:
the selected Southwest Parmecia warp does not require a Battle19 victory.

**Confirmed:** the accepted [research owner](../../research/battle19-outcomes-through-elven-village-entry.md)
establishes these selected static routes, shared consumers and initial village population.
**Inferred:** they explain the battle-outcome and village-arrival segment. **Unknown:** a natural
journey, actual caller state, completed services, responsive control and persistence. No composed
runtime outcome or mandatory victory-before-entry sequence is established.

Read [Caravan travel](caravan-boarding-and-southwest-parmecia-travel.md) for the preceding separate
walking request/admission, [Battle18 acquisition](battle18-outcomes-and-caravan-acquisition.md) for
F65/F84, and [optional Rick recruitment](hassan-rick-recruitment.md) for current-only following.
This guide ends at immediate outcome returns or conditional Map23 control. Village conversations,
searches, treasure, stairs, Map24/Battle20, later travel and F762's producer remain outside it.
Fnnn means flag nnn; table indices are zero-based. Coordinates are source operands, not observed
positions. Source symbols below retain the accepted research owner's pinned provenance.

## Entering a Battle with Empty Scene Targets

**Confirmed:** Battle19 is BATTLE_OUTSIDE_ELVEN_VILLAGE19 on Southwest Parmecia68. Its sole Map68
battle-coordinate row supplies origin `(5,19)`, size 48x32 and wildcard triggers `(255,255)`.
CheckBattle tests F419 and those trigger bytes; the rectangle is not an additional containment gate.
If F519 is already set, selection clears F419 without canceling that selected entry. Caravan ownership,
Battle18 completion, F762, Sword custody, membership and transport mode add no predicate here.
Walking's separate zone producer and supplied F419 admission remain with the preceding guide.

BattleLoop first sets PLAYER_TYPE=0 and F399, including on a supplied F88 resume. Fresh entry clears
seconds, temporarily selects map68/battle19, installs base VInts and invokes the before wrapper.
It restores incoming map/battle, clears region flags90–105, heals eligible allies, initializes
combatants, clears AI memory, calls LoadBattle with d0=0 and invokes the start wrapper. Initial
activation/region/spawn/turn-order work follows; a usable first turn remains conditional.

All four slot19 pointers in `data/battles/cutscenes/` select `ms_Empty` at `0x47A4E`, containing only
`$FFFF`. They supply no Battle19 dialogue, custom actor movement, warp or story recruitment. Their
wrappers still have separate effects:

| Hook | Selected gate and shared work |
| --- | --- |
| Before battle | F469 set skips the script; clear executes the empty target without setting F469. |
| Battle start | F469 set skips; clear sets F469 before executing the empty target. |
| Enemy defeated | Bowie must be alive and enemy128 dead. F519 clear dispatches the empty target; set skips it. Both eligible branches reach the leader tail. |
| After battle | F519 clear dispatches the empty target; set skips it. Both branches reach EndAfterBattleCutscene and its join table. |

The script reader also retains shared entry/termination work, including skip-state initialization
and conditional scroll waiting/camera-speed clearing. Empty content does not remove those services.
F469 is independent of F519 and is not reset by a loss or repeat.

F88 set instead restores saved seconds, clears F88/AI memory and the view target, calls LoadBattle
and jumps directly into the individual-turn loop. It bypasses incoming map/battle assignment, fresh
healing/placement, before/start and initial round generation, and lacks fresh entry's explicit d0=0.
Saved map, battle, area, roster and turn state must agree; an incoming19 argument alone does not
establish a consistent resumed encounter.

### Initial Roster and Repeated Encounters

**Confirmed:** BattleSpriteset19 at `0x1B46BE` contains 12 ally placements, 15 enemy rows, three AI
regions and one AI point. The first ally placement is battle-local `(40,12)`, or `(45,31)` with the
area origin. It is neither the village entrance nor a fixed victory return.

Ally initialization clears all 30 ally X values and rebuilds current active membership. Dead members
consume a list iteration without advancing the placement row; excess members can leave the active
party. Twelve placements therefore do not prove twelve living participants. Healing restores living
allies and immortal Peter/Lemon to HP/MP maxima, retaining only existing STUN/POISON/CURSE bits.
It does not revive every dead ally or grant membership/following.

| Enemy slots | Base rows, in slot order |
| --- | --- |
| 128–130 | ORC, HIGH_PRIEST, SOULSOWER |
| 131–133 | GARGOYLE, HIGH_PRIEST, ORC |
| 134–136 | DARK_MADAM, SOULSOWER, GARGOYLE |
| 137–139 | ORC, ARROW_LAUNCHER, SOULSOWER |
| 140–142 | SOULSOWER, ARROW_LAUNCHER, GARGOYLE |

All 15 are STARTING rows. Enemy128 has LEADER AI and MOVE_TO point0 `(14,19)`; enemy129 carries
SHINING_BALL|USABLE_BY_AI|UNUSED_ITEM_DROP. These are initial operands, not tactics or an observed
drop/use. The rest of the 32 enemy slots take the hidden/cleared initialization path.

Battle19 is in `randombattles.asm`. With F519 set, DetermineBattleUpgrade admits the existing
movetype/level/class/random algorithm before definition and row copying. It can retain or change a
base identity; F519 alone does not specify the upgraded roster. F469 does not select that upgrade.
No roster is computed and no combat is simulated here.

LoadBattle clears actors/lookups and declares placed combatants. The neutral table has no Battle19
entry: Taros's scene Caravan is not carried into this encounter. Exploration followers and Rick's
current-list extension are likewise not preserved as battle actors. Map/sprite/terrain, VInt,
music/fade and view services still precede conditional battle control.

## Leader Cleanup Before the Outcome Decision

**Confirmed:** after an individual action, BattleLoop runs the defeated hook, processes killed
combatants and counts both sides. Zero allies takes precedence over zero enemies. If neither ends
the battle, after-turn effects receive another killed-list/count pass with the same defeat-first
ordering, without another defeated-hook call. Counts require nonnegative X and nonzero HP; Bowie's
zero HP forces the ally count to zero.

For the eligible Bowie-alive/enemy128-dead hook, `enemyleaderpresence.asm` row19 is -1. Both F519
branches append all nonzero-HP enemies among 32 slots to DEAD_COMBATANTS_LIST with its -1 tail.
That append neither tests placement nor directly zeros their HP. ProcessKilledCombatants performs
death presentation/bookkeeping, sets listed X/Y=-1, clears status, refreshes stats and relocates
mapped actors to raw `(0x7000,0x7000)`. They leave the placed-living count. Enemy128's death is thus
a conditional cleanup trigger; it does not guarantee victory when Bowie also dies. The AI name
LEADER is not the authority for this cleanup; the leader-presence table is.

ApplyPositionsAfterEnemyLeaderDies selects only Battle05. Battle19 receives none of that table's
position/HP writes and has no staged end position to replace its return seed.

## What Each Outcome Preserves or Changes

**Confirmed:** these are ordered selected consumers, with their services still conditional:

| Outcome | Shared result and immediate return |
| --- | --- |
| First victory, F519 clear | Heal eligible allies, seed return, execute empty after target, join Bowie, clear F419/set F519; d4=1. Fresh Map68 return. |
| Repeated victory, F519 set | Same seed, join and completion writes; skip after script; d4=1. Fresh Map68 return. |
| Defeat | Update unlock, exhaustion music/text, restore Bowie's HP, halve gold by unsigned floor division, select savepoint; d4=-1. Battle4's special loss does not apply. |
| Admitted Egress / Angel Wing | Consume MP / remove selected item slot, text/flash/closure, update unlock, unwind action frame, select savepoint; d4=0. No victory join/completion or defeat gold penalty. |

The escape consumer tests action kind and masked spell/item identity. This does not prove menu
availability, sufficient MP or item custody. UpdateBattleUnlockedFlag preserves F419 while F519
is clear, and clears it when F519 is set. Loss/escape neither sets F519 nor resets F469. None of
these selected Battle19 paths grants a new recruit, sets F65, clears F84 or unlocks Battle20.

### Victory Returns from the Current Roster, Before the Join

**Confirmed:** BattleLoop_Victory rebuilds the active list and seeds the return from its first
current member: CURRENT_MAP, that combatant's X/Y plus the battle-area origin using byte arithmetic,
and its mapped entity's facing. Parameter1 is 0. This member need not be Bowie or the first living
ally, and the position need not remain an initial placement. Inconsistent membership/lookups have
no established repair. Consistent Battle19 state gives Map68 with local X+5/Y+19 and sampled facing.

The empty after target preserves that tuple. Both first/repeated paths reach EndAfterBattleCutscene;
the actually consumed `data/battles/cutscenes/afterbattlejoins.asm` row19 is 0. The s07 include uses
this table, rather than the similarly named legacy global file. Its `j_JoinForce`/JoinForce(0) call
sets Bowie's joined F0, rebuilds force lists and activates him if active count is below 12. This runs
after return-member selection. It is not a village recruit, and an unused-feature comment does not
erase the call. Victory then clears F419, sets F519 and reads the tuple; F469 remains set/supplied.

Direct outcome return uses SwitchMap → ExplorationLoop without CheckBattle. No source68 switched-map
row substitutes another map. Fresh exploration clears pending, subtracts 20000 from the STEP_COUNTER
word with a negative-result clamp, heals eligible allies, fades/loads, sets CURRENT_MAP=68 and
CURRENT_BATTLE=255, rebuilds actors, clears temporary flags 256–383, sets F80 and runs init.
It is neither preserved battle population nor an immediate repeat scan.

Map68 has one setup, an empty fixed entity list and return-only init. On this overworld F65 set
selects the table with Caravan/FOLLOWER_A first at physical 1, followed by admitted normal followers;
F65 clear selects only normal followers. Peter/F67, Oddler/F68, Rohde/F84 and other flags remain
supplied; victory does not repair them. Conflicting later FOLLOWER_A rows can overwrite its lookup.
Normal battle entry already selected walking mode 0, so F65 does not automatically board the Caravan.
F64 and saved raft coordinates survive; a separate raft can be declared when its saved map matches68.

After init/music/fade, pending zero allows following main 0 and installing current-mode control,
normally walking 0. A new zone request or village warp needs its own producer/admission. The victory
return itself does not enter the village. Stop at conditional Map68 control.

### Loss or Escape Uses the Supplied Savepoint

**Confirmed:** Battle19 has no special egress return. With normal BattleLoop's F399 set,
GetSavepointForMap uses EGRESS_MAP and its selected savepoint X/Y/facing. A missing row keeps that
map with `(1,1)`, Up. An abnormal F399-clear direct helper call returns the initial Bowie's-room
tuple before savepoint/raft scanning; it does not validate arbitrary continuation.

F64 clear preserves saved RAFT_MAP/X/Y. F64 set resets them by the selected savepoint map:
Hassan9 → Map69 `(28,55)`; Polca13 or Bedoe10 → Map72 `(43,48)`; every other selection uses the
sentinel default Map69 `(42,15)`. These saved writes neither board the raft nor change the controller.
The Battle19 outcome consumer does not normalize mode again; normal entry already set 0.

For supplied EGRESS_MAP=23, the return is Elven Village `(61,4)`, Up. It is distinct from the
independent entrance `(22,29)`, Right. F64 set resets the saved raft on69 and thus does not place it
in23. Direct outcome entry again bypasses CheckBattle and uses fresh Map23 population/init below at
the savepoint tuple. This proves neither where the player last saved nor any church/NPC activation.

Other savepoints retain their own SwitchMap/setup/init continuation, **Unknown** here. They need
not share Map23's return-only init or yield a usable pause. Supplied map255 instead uses the existing
population branch, updating main properties/raft and loading/init without fresh actors or temporary
flag clearing. No natural loss itinerary or all-map continuation is supplied.

## The Village Entrance Is Independent of Winning

**Confirmed:** Map68's selected warp row2 matches `(48,30)` and requests no-scroll Map23 `(22,29)`,
Right. Earlier points `(55,43)` and `(57,24)` and later border rows do not compete at that point.
WarpIfSetAtPoint divides supplied coordinates by 384, scans ordered rows with negative-byte
wildcards, and writes event1, parameters and WARP_SFX. It tests no F519/F419/F65/F762 or membership.
Walking/vehicle consumers reach this helper through their `$1000` layout branch after movement
checks; the match does not prove terrain reach, transport eligibility or a walk from the victory seed.

ProcessMapEvent clears pending before dispatch. The no-scroll consumer submits/clears WARP_SFX,
idles main 0, removes the exploration return frame, samples position and applies nonnegative requested
coordinates/facing. It returns `(23,22,29,Right,d4=0)` without healing, rebuilding actors, changing
PLAYER_TYPE or saving raft position. MainLoop then uses normal SwitchMap → CheckBattle admission.

The complete switched-map table has no source23 row, and the complete 45-row battle map table has no
Map23 row. Battle19 is on68; row44 is on24. F419/F519 or another unlock cannot select a battle on the
unchanged23 tuple. F762 changes setup selection, not this map/battle scan. Entry therefore has no
victory19 prerequisite, but a natural pre-victory walk remains **Unknown**. A replaced pending/map
request is a separate supplied condition; this is not an atomic guaranteed transition.

## What Fresh Elven Village Arrival Initializes

**Confirmed:** fresh exploration writes CURRENT_MAP=23/CURRENT_BATTLE=255 and uses the shared sequence
described above. Map23 has default ms_map23 and exactly one alternate, F762 → ms_map23_flag762.
F519 does not select population. Both six-entry pointer tables share zone/description/item and init
pointers, while selecting different entity/entity-event pointers. Arrival binds those events;
it does not execute them. F762 is a supplied later-story selector, not permission to explain its producer.

### Residents, Followers and Actor Identities

The default entity list at `0x59AF0` and alternate at `0x59B22` terminate at msEntitiesEnd:

| Setup / logical NPC | Initial tile, facing | Mapsprite / initial action |
| --- | --- | --- |
| Default128 | `(18,9)`, Down | MAN194; walk origin `(18,9)`, range1. |
| Default129 | `(17,11)`, Right | DWARF226; eas_Init. |
| Default130 | `(18,11)`, Left | OLD_MAN198; eas_Init. |
| Default131 | `(54,3)`, Down | WORKER202; eas_Init. |
| Default132 | `(8,12)`, Right | ELF_FEMALE224; walk origin `(8,12)`, range1. |
| Default133 | `(61,3)`, Down | PRIEST192; eas_Init. |
| Alternate128 | `(18,9)`, Down | MAN194; walk origin `(18,9)`, range1. |
| Alternate129 | `(56,6)`, Down | WORKER202; walk origin `(56,6)`, range1. |
| Alternate130 | `(18,11)`, Up | OLD_MAN198; eas_Init. |
| Alternate131 | `(61,3)`, Down | PRIEST192; eas_Init. |

msWalkingEntity encodes `$FF` and origin/range operands. DeclareNewEntity resolves that field into
the 50-byte eas_Walking template with those three patches; fixed rows install eas_Init. Coordinates,
destinations and zero velocities/travel are initial values, not frozen poses or completed movement.
No NPC interaction or random-walk trajectory is evaluated.

InitializeMapEntities clears physical actors/lookups, allocates followers from physical 1, then the
six/four NPCs, and declares Bowie/main 0 separately at the incoming tuple with eas_Idle and his
class/HP sprite. Only followers enter fresh EXPLORATION_ENTITIES; main 0/NPCs do not. With n followers,
NPCs start at n+1: logical128 onward are not physical slot numbers. Rick has no fresh follower row.

Map23 is neither an excluded-follower map nor an overworld, so it uses the normal follower table
regardless of F65. Its flag/row order is F66 Sarah; F67 Peter; F72 Kazin; F73 Slade; F74 Astral;
F66 Chester; F68 Oddler; F75 Luke; F69 fairy; F70 Astral; F74 king; F71 Lemon; F74 minister;
F76 Zynk; F77 old man; F82 Frayja; F83 king; F83 minister; F84 Rohde. Admission tests each flag,
without HP or force-membership gates. Sprite selection separately considers class/HP and Rohde
membership; dead allies outside battle select blue flame.

F65 alone adds no Caravan follower here and is not cleared. Other follower flags remain supplied;
conflicting logical rows can overwrite lookups while earlier physical actors remain. Capacity repair
for excessive supplied rows is not established. With only F67/F68, Peter is physical 1 and Oddler's
FOLLOWER_B physical 2, list `[1,2,-1]`; default NPCs occupy 3–8, alternate 3–6. This is a supplied-state
example, not a natural roster. F69 supplied does not show that F762 produced it.

### Transport, Return-Only Init and Conditional Control

Neither entrance nor setup/init clears PLAYER_TYPE. Fresh population's special mode 2 handling gives
main 0 the raft sprite, moves current listed followers to raw `(0x6000,0x6000)` with standing actions
and declares separate physical 31/lookup 63 raft at `(64,64)`, without rewriting saved RAFT_MAP/X/Y.
NPCs are not in that hidden list. For other modes, F64 plus saved map23 declares a separate raft at
saved coordinates; otherwise none is added by that tail. Mode1 alone does not replay Caravan boarding
or restore its sprite.

After population, temporary flags 256–383 are cleared and F80 set. F762/F519/F65/F64 and follower flags
survive this reset. LoadMap/base VInts precede a fresh setup resolution for init. Both selections use
ms_map23_InitFunction at `0x59C04`, containing only `rts`: no scene, request, movement, relocation,
mode change or recruitment is supplied there.

After conditional music/fade, WaitForEvent checks pending before installing control. Pending zero
follows physical 0 and selects the numeric mode consumer: 0 → eas_ControlledCharacter/opcode2;
1 → eas_Raft/opcode7, actually Caravan; 2 → eas_Caravan/opcode8, actually raft. Other nonzero/non1
values fall through to the last script without an established validity repair. Source vehicle labels
do not override that mapping.

Normal battle outcomes arrive with mode 0 from BattleLoop. Independent entrance retains its supplied
mode. Population therefore does not by itself establish walking, boarded Caravan, valid raft state
or usable input. A pending request wins before controller installation; services and asynchronous
initial actions remain conditional. Stop at this boundary before village interaction or departure.

For optional local content after admitted arrival, see
[Elven Village conversations and local exploration](elven-village-conversations-and-local-exploration.md).
That guide owns dialogue, search and internal stairs; this guide retains its entry/control stop.

## Evidence Trace and Review Limits

**Confirmed provenance:** accepted #856/PR857 binds SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [research reproduction owner](../../research/battle19-outcomes-through-elven-village-entry.md#reproduction-and-remaining-limits)
retains source/table/include joins and unchanged map-init/script/cutscene comparisons. This guide
consumes accepted evidence; it performs no source reinvestigation or new acquisition.

| Explained boundary | Contract and exact fixture owner | Claim limit |
| --- | --- | --- |
| Empty targets, wrapper gates, leader/join tails | [cutscene routing](../contracts/battle-cutscene-routing.md); `sf2-battle-cutscenes-static-v1` in [wrappers](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), expected.cutsceneFacts; `sf2-battle-routing-data-static-v1` in [routing](../../../tests/fixtures/h2/battle-routing-data-static-v1.json) | routeTargetsParsed=false; aggregate join labels/occupancy do not close the actual s07 include or selected row19. Accepted research supplies the four pointers, leader -1 and consumed join0. |
| Fresh/resume, counts, outcomes and membership | [battle lifecycle](../contracts/battle-control-lifecycle.md), [party membership](../contracts/party-membership-state.md); `sf2-battle-loop-static-v1` in [loop](../../../tests/fixtures/h2/battle-loop-static-v1.json), expected.lifecycleFacts; `sf2-battle-control-static-v1` in [control](../../../tests/fixtures/h2/battle-control-static-v1.json), expected.controlFacts; `sf2-common-stats-static-v1` in [stats](../../../tests/fixtures/h2/common-stats-static-v1.json), expected.statsFacts.party | Shared algorithms are not a played Battle19, consistent resume, actual seed/roster or menu admission. Selected return-before-join and escape/savepoint branches use accepted research. |
| Selected initial battle rows/repeat | [encounter definition](../contracts/battle-encounter-definition.md); `sf2-battle-spriteset-data-static-v1` in [spritesets](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), expected.facts | rowContentTracked=false. Counts do not serialize the complete selected spriteset19 or calculate upgraded enemies; those static selections belong to the research owner. |
| Entrance, competing map/battle rows, savepoint and control | [entry routing](../contracts/map-entry-routing-state.md), [exploration flow](../contracts/exploration-control-flow.md), [map exploration](../contracts/map-exploration.md); `sf2-common-maps-static-v1` in [maps](../../../tests/fixtures/h2/common-maps-static-v1.json), expected.mapFacts; `sf2-gameflow-core-static-v1` in [flow](../../../tests/fixtures/h2/gameflow-core-static-v1.json); `sf2-map-event-request-consumption-static-v1` in [request consumption](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), eventRequestConsumption | Shared scan/poll/clear shapes do not serialize the selected private tables or prove a producer-to-consumer runtime route. No Map23 battle row and the two distinct arrival tuples use accepted research. |
| Default/F762 population and init | [setup](../contracts/map-setup-data.md), [entities](../contracts/map-entity-data.md); `sf2-map-setup-static-v1` in [selection](../../../tests/fixtures/h2/map-setup-static-v1.json), `sf2-map-entities-static-v1` in [actors](../../../tests/fixtures/h2/map-entities-static-v1.json), `sf2-map-init-static-v1` in [init](../../../tests/fixtures/h2/map-init-static-v1.json) | Init expected.primarySourceBodies sourceOrder38 is one rts; expected.routeJoins sourceOrder59/60 and expected.pointerTableJoins sourceOrder62/63 share it. Shared macro/population facts do not prove actual flags, completed initial actions or village events. Ten selected NPC rows and immediate follower/mode consumers use accepted research. |

Schema/consumer pairs are [cutscene schema](../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)/[battle_cutscenes.py](../../../src/sf2tool/h2/battle_cutscenes.py),
[routing schema](../../../schemas/h2-battle-routing-data-static-fixture.schema.json)/[battle_routing_data.py](../../../src/sf2tool/h2/battle_routing_data.py),
[loop schema](../../../schemas/h2-battle-loop-static-fixture.schema.json)/[battle_loop.py](../../../src/sf2tool/h2/battle_loop.py),
[control schema](../../../schemas/h2-battle-control-static-fixture.schema.json)/[battle_control.py](../../../src/sf2tool/h2/battle_control.py),
[stats schema](../../../schemas/h2/common-stats-fixture.schema.json)/[stats.py](../../../src/sf2tool/h2/stats.py),
[spriteset schema](../../../schemas/h2-battle-spriteset-data-static-fixture.schema.json)/[battle_spriteset_data.py](../../../src/sf2tool/h2/battle_spriteset_data.py),
[maps schema](../../../schemas/h2-common-maps-static-fixture.schema.json)/[maps.py](../../../src/sf2tool/h2/maps.py),
[flow schema](../../../schemas/h2-gameflow-core-static-fixture.schema.json)/[gameflow.py](../../../src/sf2tool/h2/gameflow.py),
[request schema](../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../src/sf2tool/h2/map_event_request_consumption.py),
[setup schema](../../../schemas/h2-map-setup-static-fixture.schema.json)/[map_setup.py](../../../src/sf2tool/h2/map_setup.py),
[entity schema](../../../schemas/h2-map-entities-static-fixture.schema.json)/[map_entities.py](../../../src/sf2tool/h2/map_entities.py),
and [init schema](../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../src/sf2tool/h2/map_init.py).
Existing reproduction commands are `uv run sf2 h2` with `battle-cutscenes`, `battle-routing-data`,
`battle-loop`, `battle-control`, `common-stats`, `battle-spriteset-data`, `common-maps`, `gameflow-core`,
`map-event-request-consumption`, `map-setup`, `map-entities` and `map-init`. Load ignored worktree
configuration in the same process before controlled reproduction. These identify owners, not new
prose-only runs; the retained map-script-engine comparison stays with the research recipe.

Pre-synthesis review joined accepted research, the stricter contracts, selected fixture fields and
schema/consumer boundaries. It distinguishes public aggregate structure from selected content and
runtime completion. Generic older Unknown queues do not invalidate later accepted static joins or
authorize acquisition. Counterexamples include F519-clear entrance, completed19 with F419 supplied,
both Bowie/enemy128 dead, F88 resume, changed membership before join, F65/mode mismatches, F762 alternate,
current-only Rick, conflicting follower lookups, F64/saved-map mismatch and replaced pending state.
These constrain static explanation; they are not executed scenarios. No Layer A correction is needed.

Preserve [PR857 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/856#issuecomment-6162129601)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/857#issuecomment-6162154889),
including retained comparisons, corrected inspections and exact CI. Preserve
[PR858 updated handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/858#issuecomment-6162196284)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/858#issuecomment-6162210098),
with their linked original/predecessor failures. Accepted comparisons need no ritual replay for prose;
fixtures/schema/goldens remain unchanged and do not become composed runtime observations.

**Unknown:** naturally reachable entrance/terrain/route or winning trajectory; actual flags, HP,
roster, mappings, random seed, savepoint and transport; nonselected savepoint init; service/initial-action
completion, rendered presentation, responsive input and persistence. F762's producer and later content
remain excluded. These gaps stay deferred under [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../decisions/0016-remake-start-evidence-deferral.md). This guide chooses no remake rule
and consumes no unaccepted Issue860 finding.
