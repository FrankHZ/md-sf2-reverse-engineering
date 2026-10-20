# Battle23 outcomes and Willard Burrow return

Evidence date: 2026-10-14. **Confirmed:** Battle23 selects empty defeated and
after scripts, but the leader-cleanup and Bowie-join wrappers still execute.
Victory clears F423, sets F523 and returns a member-derived Map27 position;
a fresh exploration load then selects an empty resident list and a return-only
init. Defeat and Egress instead select a savepoint through EGRESS_MAP.
**Inferred:** victory removes Willard's confrontation population from the burrow.
**Unknown:** natural outcome/traversal, supplied state, completed services,
visible or audible presentation, usable control and persistence.

## Agreement and provenance

Standard rigor under [Issue #937](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The uncertainty is the selected outcome-to-map-return join. Accepted starting
base is `3b8d97758253e1867f05656f98c5862c29bec3c7`, including #932/PR935
and #928/PR931. This is the sole owned document; no shared index, registry,
schema, fixture, tooling, design or remake edit. Accepted #934 design is
disjoint; there is no stacked dependency. The result supports the next English guide.

Consume accepted [Battle23 initialization/roster](./battle23-initialization-and-roster.md),
[Willard Burrow admission](./willard-burrow-through-battle23-entry.md),
[Battle22 outcome consumers](./battle22-outcomes-and-desktop-return.md),
[battle loop](./battle-loop.md), [control/lifecycle contract](../design/contracts/battle-control-lifecycle.md),
[map setup selection](./map-event-flag-route-selection.md) and
[request consumption](./map-event-request-consumption.md). Begin at the
post-action end-check seam. Stop at conditional restored exploration or next
map admission. No turn/AI simulation, new comparator, acquisition, runtime,
engine work, Chinese translation, Map33 scene, Creed choice/recruitment or
later battle is included.

All source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained evidence identifies USA
ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Short battle paths mean `code/gameflow/battle/`; setup filenames mean
`data/maps/entries/map27/mapsetups/`; script consumers mean
`code/common/scripting/map/`. Fnnn means flag nnn; slots are zero-based.
Coordinates and service requests are operands, not observed movement/timing.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair and small
selected fixtures/sources. Linear comparisons are budgeted below one minute
and 512 MiB, plus small prose/receipts; the pair/cutscene comparison passed
in 0.082s. No payload regeneration or environment creation. Reassess material
scope/resource expansion. Aggregate equality does not guard new prose semantics.

## Post-action checks and leader cleanup

**Confirmed:** `battleloop_1.asm:BattleLoop`, at `$23A84..$23BB4`, reaches
`@IsBattleEnd` after ExecuteIndividualTurn and the optional debug kill-all
branch. Its first sequence is ExecuteBattleCutscene_Defeated,
ProcessKilledCombatants, CountRemainingCombatants, then zero-allies defeat
before zero-enemies victory. With both counts nonzero, ProcessAfterTurnEffects
runs, then another killed pass and the same ordered counts. This second
check has no defeated-hook call. No individual turn is simulated here.

The complete selected tables and wrappers resolve as follows:

| Source / selected record | Immediate meaning for Battle23 |
| --- | --- |
| `data/battles/cutscenes/battleendcutscenes.asm`, slot 23 of 48 words, offset 46 at `$47C16` | Relative pointer to ms_Empty. |
| `data/battles/global/enemyleaderpresence.asm`, slot 23 of 45 bytes at `$47CA5` | -1: enable the common surviving-enemy append tail. |
| `data/battles/cutscenes/afterbattlecutscenes.asm`, slot 23 of 48 words, offset 46 at `$47D22` | Relative pointer to ms_Empty. |
| `data/battles/cutscenes/afterbattlejoins.asm`, slot 23 of 52 bytes at `$47D81` | 0: the shared tail calls JoinForce for Bowie. |
| Complete `data/battles/global/afterbattlepositions.asm` | Only Battle05: no Battle23 placement override in ApplyPositionsAfterEnemyLeaderDies. |
| Complete `data/battles/cutscenes/regionactivatedcutscenes.asm` | Only battles32/40: ExecuteBattleRegionCutscene finds no Battle23 row. |

`layout/sf2-07-0x044000-0x064000.asm:43–51` includes the **cutscenes**
join table immediately after the after wrapper; the similarly named global
table is not this consumer's owner. The unused-feature comment does not
cancel the executed JoinForce call. The complete target
`code/common/scripting/map/ms_empty.asm` at `$47A4E..$47A50` is `$FFFF` only.
There is no selected local death dialogue, after scene, warp or new recruit
command. Wrapper and script-engine effects still matter.

`cutscenes/battleendcutscenesstart.asm` first requires Bowie's HP nonzero
and enemy128's HP zero. Either failed predicate restores registers at
`loc_47C88` and returns. The accepted fresh roster in
`data/battles/spritesets/spriteset23.asm:BattleSpriteset23` identifies Willard
as enemy128; the five Rats are 129, 130, 132, 134, 137, and the six Bubbling Oozes
are 131, 133, 135, 136, 138, 139. These identities are interleaved, not contiguous
groups. A resumed or supplied roster is not silently replaced by that fresh
premise.

With both predicates satisfied, F523 clear dispatches ms_Empty; F523 set
skips only the script. Both reach `battleendcutscenesend.asm:loc_47C48`.
The nonzero leader byte causes a scan of all enemy128–159. Every nonzero-HP
entry is appended at DEAD_COMBATANTS_LIST_LENGTH, a following -1 byte is
written, and length increments. This tail does not test placement X, clear
HP, clear the existing list or deduplicate it. Willard's zero-HP slot is not
added by this tail; its ordinary death-list entry, if needed for presentation,
comes from earlier work. Supplied stale/duplicate/invalid lists or mappings
are not repaired by the outcome wrapper.

`battleloop/processkilledcombatants.asm` at `$24518..$24642` returns for
length 0. Otherwise it processes the length-delimited list through spin and
death-sound presentation, then bookkeeping: defeats credited to the listed
dead ally, or enemy kills to BATTLESCENE_FIRST_ALLY, X/Y=-1, status=0, stat recomputation and mapped
entity position `$7000,$7000`. Its complete body does not set listed HP to 0
or clear the list length. Surviving enemies appended by leader cleanup
therefore cease to count through negative X even if HP stays nonzero.
Presentation/VInt/sleep calls must complete before later cleanup/counting.

`battleloop/countremainingcombatants.asm` at `$23C58..$23CBA` counts all 30
ally slots and 32 enemy slots using nonnegative X-byte and nonzero HP-word,
not joined/active flags. Bowie HP0 forces the ally count to 0 regardless of
other survivors. Both end checks prioritize that zero-ally result; simultaneous
loss is defeat. Zero counted enemies also suffices without the leader tail.

With coherent placed party, valid death list/mappings and completed services,
Bowie surviving plus Willard HP0 at the defeated-hook seam removes the other
living enemies and permits victory without individually killing each Rat/Ooze.
A leader death arising only during after-turn effects does not execute that
tail at the second check: remaining counted enemies still matter there.
Bowie alive alone is insufficient if the entire counted ally set is empty.
The debug branch is not evidence of natural victory.

## Victory order and the actual return seed

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` at `$23CBA` performs:

1. Heal eligible allies through `battleloop/heallivingandimmortalallies.asm`:
   all 30 slots, living allies plus Peter7/Lemon28 even when dead; restore
   HP/MP maxima, retain only existing STUN/POISON/CURSE bits, then recompute
   stats. Other dead allies stay dead. Membership is not a healing predicate;
   equipment/stat recomputation still follows the mask. Battle44's timer
   special is absent for23.
2. Set PARAM2=CURRENT_MAP. Call UpdateForceAndGetFirstBattlePartyMemberIndex;
   use that member's combatant X/Y plus BATTLE_AREA_X/Y with byte addition
   for PARAM3/4. Read that member's mapped entity facing for PARAM5; set
   PARAM1=0. No MAP_EVENT_TYPE write occurs here.
3. ExecuteAfterBattleCutscene: F523 clear dispatches ms_Empty; set skips
   dispatch. Both paths execute EndAfterBattleCutscene and JoinForce(0).
4. Clear current-battle unlocked flag400+23=423, then set completed flag523.
   Clear d0–d3 words, read PARAM2–5 bytes into them and return d4=1.

`code/common/scripting/text/textfunctions_1.asm:UpdateForceAndGetFirstBattlePartyMemberIndex`
calls UpdateForce and takes the first BATTLE_PARTY_MEMBERS byte.
`code/common/stats/battleparty.asm:UpdateForce` scans IDs0–29 ascending,
requiring joined and active flags for the active list. This is not a
first-living search. It has no empty-list repair; length 0 does not initialize
that first byte. A supplied first member may be dead/off-map. Normal joined,
active Bowie is first, but that is a state premise, not a hardcoded selector.

`battlefunctions/battlefunctions_0.asm:GetEntityIndexForCombatant` uses the
actual ENTITY_INDEX_LIST lookup; victory shifts the result by
ENTITYDEF_SIZE_BITS=5 and reads ENTITYDEF_OFFSET_FACING. Facing is not a fixed
Right, nor is it derived from movement intent. The accepted Map27 row23 in
`data/battles/global/battlemapcoords.asm` supplies area `(23,14)`, size27x16.
With that current state, local combatant `(x,y)` returns `(x+23,y+14)` modulo 256,
with the actual mapped facing. The old `(0,0,Right)` admission request and
initial formation are not the current victory position. Supplied saved area,
map, membership, position or mapping changes the seed.

`cutscenes/afterbattlecutscenesstart.asm` at `$47CBC` and
`afterbattlecutscenesend.asm:EndAfterBattleCutscene` at `$47D54` restore their
saved registers and return after the common join. JoinForce(0) sets Bowie's
joined flag F0, rebuilds force lists and, if active count is below 12, calls
JoinBattleParty to set F32. A full party is not evicted to make room; that
active-bit write does not immediately rebuild counts again. This is not a
new Creed recruit and cannot repair the member already used for the seed.

`mapscriptengine_2.asm:ExecuteMapScript` saves registers, initializes its work
pointer and clears the text-skip byte (the debug/input check may set it).
The empty target immediately reaches `loc_47234`: when DIALOGUE_WINDOW_INDEX
is nonzero it waits for view scrolling, then clears VIEW_SCROLLING_SPEED,
restores registers and returns. Thus an empty script can still block completion.
An unfinished after-script prevents the join and flag writes; an unfinished
join prevents the later clear/set. Completed prefixes are not rolled back.
The selected empty script supplies no replacement return tuple.

Victory reads the parameter bytes directly, without waiting for a pending
csc07 warp event to be consumed. A competing parameter write before the final
read would alter the result. `code/gameflow/mainloop.asm:alt_MainLoopEntry`
at `$75C4..$75EC` then calls SwitchMap and ExplorationLoop, **bypassing
CheckBattle**. This caller does not branch on d4's outcome sign. No Map27
substitution exists in the complete flag-switched-map table.

## Defeat, Egress and the savepoint selector

**Confirmed:** BattleLoop_Defeat first calls UpdateBattleUnlockedFlag, clears
DIALOGUE_NAME_INDEX_1, requests MUSIC_SAD_THEME_2 and text363 (exhaustion),
closes text, restores Bowie's HP to maximum, halves unsigned gold by a logical
right shift, calls GetEgressPositionForBattle and returns d4=-1. Battle04's
special completion/upgrade branch does not apply. Other dead allies are not
revived by this defeat body; the later exploration entry separately runs the
living/immortal heal. Text/audio requests are not proof of presentation.

In `battlefunctions/battlefunctions_1.asm`, admitted ExecuteBattleaction_Egress
gets spell cost and decreases the acting combatant's MP. Angel Wing instead
removes the selected item slot. Both close battlefield windows and request
their use/cast text; the shared tail requests spell sound/flash, closes text,
updates unlock, unwinds the action frame, obtains Egress coordinates and
returns d4=0. Neither escape body halves gold or performs defeat's Bowie-HP
restoration. Menu availability, spell/item custody and natural admission are
not established by reading these already-entered handlers.

UpdateBattleUnlockedFlag tests F523. If clear it preserves F423; if set it
clears F423. Neither defeat nor escape sets F523 or runs the victory after/join.
The earlier CheckBattle may already have cleared F423 when admitting a
supplied F523-set encounter. These statements describe the remaining writes,
not a guarantee that F423 was set on entry to the outcome.

`battleloop/getegresspositionforbattle.asm` at `$23E50..$23EB0` has special
checks for38/39/16/17/13/31/32; none selects Battle23. Its ordinary arm reads
**EGRESS_MAP as a map selector** and calls GetSavepointForMap. It does not
save or retrieve the party's prior XYZ/facing tuple. The complete consumers
are `code/common/maps/egressinit.asm` at `$75EC..$764E`,
`data/maps/global/savepointmapcoords.asm` and `raftresetmapcoords.asm`.

| Supplied premise | Savepoint result / side effect |
| --- | --- |
| F399 set, EGRESS_MAP26 | Map26 Floor World `(4,36)`, Up1. |
| F399 set, EGRESS_MAP22 | Map22 Desktop `(5,8)`, Up1; not the Battle23 victory seed. |
| F399 set, EGRESS_MAP27 | No Map27 savepoint row: retain map27 with default `(1,1)`, Up1. |
| F399 set, another selector | Use its matching four-byte savepoint row, or retain its map byte with `(1,1)`, Up on no match. |
| F399 clear at a direct supplied call | Initial Map3 `(56,3)`, Down3; early return before raft-reset processing. |
| F399 set/F64 clear | Preserve saved raft fields. |
| F399 set/F64 set, selector26/22/27 | No special raft row: default writes RAFT_MAP69 and RAFT_X/Y=`(42,15)`. This is not boarding. |

BattleLoop itself sets F399 before either fresh/resumed entry; the clear case
is a counterexample for a direct/inconsistent supplied call, not the ordinary
Battle23 route. F64's other raft-reset rows are Hassan, Polca and Bedoe;
they do not alter the named26/22/27 cases.

The accepted [Floor World arrival](./floor-world-first-arrival.md) joins
`data/maps/entries/map26/mapsetups/s6_initfunction.asm`: F779 clear runs
cs_59E04, sets779 after return, then writes EGRESS_MAP26. F779 already set skips that assignment.
Map27's init, confrontation, selected battle hooks and outcomes do not replace
EGRESS_MAP. Thus a completed earlier Floor World assignment, if retained,
yields26 on defeat/Egress; merely entering the burrow or supplying F779 set
does not prove that selector. The return tuple is not automatically Map27.

All three outcomes use MainLoop's alternate entry, so preserved F423 on a
first defeat/Egress does not immediately readmit the battle there. A later
normal MainLoop pass can use it. SwitchMap and destination exploration still
consume the actual supplied map/state. For a returned Map26 or Map22, stop
at that admission and use their accepted owners for initialization; no new
return scene is claimed here.

## Fresh Map27 return and conditional exploration

**Confirmed:** for the explicit normal victory tuple, ExplorationLoop in
`code/gameflow/exploration/explorationfunctions_2.asm` clears MAP_EVENT_TYPE,
adjusts STEP_COUNTER, heals eligible living/immortal allies, requests fade-out
and disables camera following. It sets CURRENT_MAP27 and CURRENT_BATTLE255,
loads tilesets, waits fading, clears VInts, selects/rebuilds entities and loads
mapsprites. It then clears temporary flags256–383 and sets F80. F423/F473/F523/
F781 are outside that reset range. Common work initializes sprite frames,
targets physical0, calls LoadMap, installs base VInts and runs map init.

The LoadMap call has d0=-1 for view selection and d1=27 for a new/full load.
These operands have different roles. `code/common/maps/mapload.asm:LoadMap`
extends the d1 byte; nonnegative selects LoadMapBlocksAndLayout, while negative
reuses the working layout. With CURRENT_BATTLE255 the full-load tail skips
the battle crop. Return therefore loads the full burrow, not the 27x16 battle
rectangle left in the working layout. Map27's complete `3-flag-events.asm`
and `7-chest-items.asm` are each endWord: there is no F523 layout-copy or chest
overlay here. F523's documented local change is population, not an inferred
wall-opening/collision change.

`data/maps/mapsetups.asm:92–94` selects default ms_map27 or F523 alternate
ms_map27_flag523. The complete pointer tables at `$59F42..$59F72` differ only
in entity-list pointer; EntityEvents, ZoneEvents, AreaDescriptions, Section5
and InitFunction are shared. GetCurrentMapSetup in mapsetupsfunctions_1.asm
reselects the setup for each accessor; neither F473 nor F781 selects population.

| F523 at fresh population selection | Complete resident data |
| --- | --- |
| clear | `s1_entities.asm`: RAT79 `(43,20)`, WILLARD154 `(43,21)`, RAT79 `(43,22)`, all Left2/eas_Init. |
| set | `s1_entities_523.asm` at `$59F8A..$59F8C`: msEntitiesEnd only; zero resident rows. |

`data/maps/mapsetupsstorage.asm:380–389` includes the three-row list immediately
before the alternate terminator, so the baseline also terminates there.
Victory's F523 write precedes selection. The old twelve battle enemies do not
become these exploration residents, and defeating Willard does not directly
hide an exploration actor: this explicit reload replaces actors/lookups.

`mapfunctions.asm:InitializeMapEntities` clears entity records/lookups,
allocates followers from physical1, consumes the selected resident list and
finally declares Bowie at physical0 with the return position/facing and
eas_Idle. The supplied main coordinates are multiplied by 384; the six-bit
mask used for resident row coordinates is not a repair of this main tuple.
An empty resident list does not mean no main entity or followers.

The complete follower/mode allocation is retained by
[the accepted population owner](./willard-burrow-through-battle23-entry.md#admission-setup-and-the-complete-population).
Only F67/F68 in the ordinary example allocate Peter7 at physical1 and
B31/Oddler at physical2; force membership alone does not allocate them.
Additional flags shift allocation and can overwrite logical lookups; mode/raft
handling still applies. BattleLoop clears PLAYER_TYPE to 0 and the selected
outcome/init does not change it. Supplied mode changes are separate premises.
There is no new recruitment or guaranteed Peter/B allocation from F523.

For the default resident list, logical128–130 map through lookup32–34, with
Willard at129; battle enemy128 is a different identity. The F523 list leaves
those resident lookups zero after clearing. mapscriptengine_1.asm:GetEntityAddressFromCharacter
therefore resolves an absent resident to physical0, not a safe absent-actor
no-op. No before/after battle actor mapping can be reused as the fresh
exploration mapping. This matters for supplied F781-clear state below.

The complete `s6_initfunction.asm` at `$59FB6..$59FB8` is rts. Its wrapper
RunMapSetupInitFunction selects the pointer, calls it, restores registers and
returns. There is no post-victory dialogue, scripted placement, reward, warp,
EGRESS assignment or F781 repair in this init. Exploration then conditionally
submits map music/fade according to palette comparison and reaches WaitForEvent.
With no pending type, it targets physical0 and installs the current-mode
control action; mode0 selects eas_ControlledCharacter in
`code/common/scripting/entity/entityfunctions_2.asm:SetControlledEntityActScript`.
A pending nonzero event takes precedence and skips controller installation.
A returning init is not proof of completed fade/input services or usable control.

A supplied map255 return/reentry is distinct: ExplorationLoop preserves
CURRENT_MAP/current population, skips fresh tilesets/allocation/temp reset and
CURRENT_BATTLE assignment, updates main and raft/followers, then uses the
current-layout LoadMap arm and init. F523 alone cannot remove previously
allocated residents on that path. It is not the ordinary explicit Map27
victory reload and does not automatically normalize battle state.

## Independent gates and the existing exit records

**Confirmed:** the four gates retain different consumers:

| Flag | Role and outcome boundary |
| --- | --- |
| F423 | Battle23 admission. Victory clears it; defeat/Egress clear it only if F523 is already set. |
| F473 | Before/start intro gate. Start sets it before dispatch; these outcomes/map init do not write it. |
| F523 | Battle completion and fresh Map27 resident selection. Victory sets it; it is not a direct exit predicate. |
| F781 | Exploration Y19 confrontation gate. Its callback writes it after cs_59FB8 returns; outcomes/init do not write it. |

`s3_zoneevents.asm` has one specific row, wildcard X/Y19, plus default return.
Map27_ZoneEvent0 tests only F781. Set returns; clear runs the accepted 41-command
cs_59FB8 and then sets781. That script's tail sets story flag23/F423, requests
Map27 `(0,0,Right)` through csc07 and terminates. The request is pending until
the exploration consumer handles it; F781's caller write precedes that handling.
The full confrontation/action/wait inventory remains in the admission owner.
Type6 first installs the init action on physical0 without waiting and passes
PARAM1/PARAM3 words to RunMapSetupZoneEvent. After the callback returns, that
wrapper closes portrait/text, waits a VInt and waits for physical0 to stop
moving before returning. A pending warp and even F781 set therefore do not
prove that the wrapper returned or that MainLoop consumed the request.

F523 set/F781 clear is not normalized by victory: if the zone is later
invoked, its script uses missing resident lookups against physical0 and may
not complete normally. If its services do complete, it can request another
Battle23 admission. F523 then clears F423 during CheckBattle but does not
cancel that already selected encounter. Conversely, F781 set suppresses the
confrontation even if F473 is clear. None of these flag values proves that a
scene played, an outcome occurred naturally or a save persisted.

`data/maps/entries/map27/6-warp-events.asm`, `$A93F6..$A9438`, has exactly
eight eight-byte records and endWord. Each is warpNoScroll/type0, followed by
a zero padding byte after facing. The full selected table is unchanged:

| Row | Trigger X/Y | Map / destination X/Y | Facing |
| --- | --- | --- | --- |
| 0 | `(2,8)` | FLOOR_WORLD26 `(59,4)` | Down3 |
| 1 | `(11,10)` | CURRENT255 `(30,8)` | Right0 |
| 2 | `(30,8)` | CURRENT255 `(11,10)` | Left2 |
| 3 | `(33,13)` | CURRENT255 `(12,15)` | Left2 |
| 4 | `(12,15)` | CURRENT255 `(33,13)` | Right0 |
| 5 | `(54,2)` | CURRENT255 `(3,3)` | Right0 |
| 6 | `(255,0)` | CREED_MANSION33 `(15,9)` | Right0 |
| 7 | `(21,255)` | CREED_MANSION33 `(15,9)` | Right0 |

`sf2mapmacros.asm:mWarp/warpNoScroll/warpMap/warpDest/warpFacing` supplies
trigger/type/map/coordinates/facing/padding. Rows6/7 mean any X at Y0 and any
Y at X21; first match wins. There is no reverse `(3,3)` row. Facing is an
output, not an approach restriction. Neither the table nor its matcher tests
F423/F473/F523/F781, consumes an item or grants Creed recruitment.

`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint` signed-divides
candidate raw X/Y by 384 and scans the current map's table in order, stopping
at word-1. Negative trigger bytes are wildcards. A match sets event type1,
copies type/map/X/Y and facing/padding to PARAM1–6, and requests SFX_WARP.
The ordinary esc02 caller requires candidate layout class `$1000` under mask
`$3C00`; coordinate equality alone is not a movement or route proof. The zone
uses its separate `$1400` request branch and low-byte X/Y selector, as owned
by the accepted admission trace. Neither table proves a walkable maze solution.

WaitForEvent prioritizes a pending event. ProcessMapEvent clears its sampled
type before dispatch. ProcessMapEventType1_Warp's type0 arm submits/clears
WARP_SFX, makes physical0 idle, unwinds exploration and returns map/coordinates/
facing with d4=0 to **normal MainLoop**, unlike battle's alternate return.
UpdatePlayerPosFromMapEvent first divides physical0's signed raw coordinates
by 384; nonnegative PARAM3/4 then replace only the low byte of those position
words. Abnormal negative/high-byte state is not zero-extended or repaired by
the explicit destination byte. The coherent destination tuples above assume
that inherited limitation is satisfied; wildcard admission does not fix it.

SwitchMap has no26/27/33/255 substitution. The complete battle-coordinate
table has no26 or33 row: ordinary explicit Floor World/Creed requests admit
exploration, stopping here before destination population/init. Map255 resolves
CURRENT_MAP27 for CheckBattle. F423 clear preserves the255 request and admits
current-population exploration; F423 set selects Battle23's wildcard row,
writes area `(23,14)`,27x16 and returns battle23. F523 set clears F423 within
that selection but still admits the battle. F523/F781 are not direct exit
prerequisites; actual layout, action, position and pending state remain needed.

## Reproduction and remaining boundaries

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Verify the pinned checkout's commit and clean status. Reuse the
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init equality, `_verify_map_script_engine_fixture` and `_cutscene_facts`.
Also compare `_build_lifecycle_facts` in `sf2tool.h2.battle_loop` with
`tests/fixtures/h2/battle-loop-static-v1.json` expected.lifecycleFacts and
`_build_control_facts` in `sf2tool.h2.battle_control` with
`tests/fixtures/h2/battle-control-static-v1.json` expected.controlFacts.
All passed against the pinned disasm root; retained pair size/mtime stayed
unchanged. These existing fixtures prove their bounded shared facts, not the
new composed prose or natural execution.

Direct reproduction must additionally:

- Enumerate every row of defeated/after/join/leader tables, select23 and
  resolve the wrapper tails and ms_Empty body; inspect region/position table
  exclusions. Resolve j_ aliases through s02/s05/s07 under
  `code/common/tech/jumpinterfaces/`, including j_GetMapSetupEntities to
  GetMapSetupEntityList rather than a guessed same-name implementation.
- Read the complete post-action count/cleanup, victory/defeat/Egress,
  UpdateForce/JoinForce, first-member/actor lookup and savepoint consumers.
  Trace parameter widths, exact order, branch polarity and d4 returns through
  MainLoop and both exploration load arms, not just table names/comments.
- Join both Map27 pointer tables and actual include order to all resident
  rows/terminator, the full init and shared zone, complete eight warp records,
  matcher/request consumer and complete switch/battle/savepoint tables.
  Compare the confrontation to its accepted retained program and preserve
  the predecessor's low-byte/physical-identity limits.

Perform whole-document/exact-diff, source/provenance/link/private-boundary
checks and `git diff --check`. On a clean committed head run configured
`uv run sf2 verify plan --base origin/main --head HEAD`; record final-head CI
and actual base in the Issue handoff. Documentation policy excludes blanket
local normal/full/engine/Godot/H3/H4 suites solely for this prose. No new
parser, fixture, payload or runtime queue is introduced.

Preserve [#932 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/932#issuecomment-6176988463)
and [PR935 independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/935#issuecomment-6177066791),
including their exact identities, CI38626848120,0.081s pair comparison and
linked #928/#924/predecessor results. Their inspection/path/rejected-write
failures and corrections remain retained; none is an interrupted game suite
or justification to regenerate inputs. This slice's receipts preserve its
own narrowed inspection diagnostics separately.

**Unknown:** natural leader death/all-enemy outcome, exact supplied dead list,
party/HP/placement/actor mappings/flags/current or saved geometry, EGRESS_MAP,
mode/raft, service completion, rendered/audio timing, usable control, exit
reach and persistence. Supplied-state examples discriminate source branches;
they are not executed scenarios. [ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md) retain these
as conditional evidence gaps. Map33 scenes, Creed choices/recruitment and
later battles remain outside this completed static boundary.
