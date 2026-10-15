# Battle27 outcomes through Tristan admission

Evidence date: 2026-10-14. **Confirmed:** Battle27's defeated hook can run
shared leader cleanup, and victory runs an after-battle JoinForce0 tail despite
its empty scripts. Victory returns a member-derived Map74 seed, then constructs
fresh exploration without an intervening battle scan. The separate explicit
Map74 `(52,20)` warp requests Tristan15 `(31,30)`, Up; its later MainLoop
scan admits exploration, not a battle. **Inferred:** these source-named
records connect the encounter to Tristan. **Unknown:** natural outcome or
arrival, actual supplied state, completed services, presentation, usable
control and persistence. A table coordinate proves no walked route.

## Agreement and provenance

Standard rigor for [Issue #1041](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1041)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
terminal predicates, completion/unlock/intro flags, return registers, rebuilt
actors and admission have distinct lifetimes. Accepted integration base is
`65334101a0d8d6f13b7489dd9ac70c66d767038d`, including #1033/PR1036 and
#1037/PR1039. This is the sole tracked owner, with no shared writes or stacked
dependency. Accepted #1038 synthesis supplies no original-game evidence;
UI692 is unconsumed.

Start conditionally after an individual turn returns, beyond the accepted
[first-call boundary](./battle27-activation-and-first-turn-dispatch.md).
Consume [initialization/opening](./battle27-initialization-and-opening.md),
[Pacalon departure/admission](./pacalon-departure-to-battle27-admission.md),
[Battle26 outcomes](./battle26-outcomes-and-higins-recruitment.md) and
[fresh return](./battle26-return-through-pacalon-entry.md), plus the existing
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[lifecycle contract](../design/contracts/battle-control-lifecycle.md),
[party/follower owner](./ribble-may-through-battle10-admission.md),
[common maps](./common-maps.md), [common scripting](./common-scripting.md),
[request state](./map-event-request-state.md) and
[request consumption](./map-event-request-consumption.md).
Stop at MainLoop's resulting Tristan exploration call, before its body/init.
No combat transcript, AI/action/damage execution, general reward/menu system,
decoded collision route, runtime, capture/extraction, RNG seed enumeration,
Tristan dialogue, Battle28/32/later Frayja, design synthesis or remake work.

Source paths below are relative to `disasm/` at pinned
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained source is clean and read-only. Existing evidence binds USA retail
ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Constants/RAM are in `sf2enums.asm`/`sf2const.asm`; Fnnn means flag nnn.
Short `battleloop/`, `battlefunctions/` and `cutscenes/` paths are under
`code/gameflow/battle/`; `map/` and `entity/` under `code/common/scripting/`.
Indices are zero-based; address extents are half-open. These are conditional
source/data facts, not new runtime observations.

Minimum evidence is selected pinned source, existing actual fixture/schema/
consumer joins and retained payloads. Reuse the unchanged 18,285,979-byte
map-init/script pair and accepted 0.083s provenance without regeneration.
Selected linear inspection/comparison is budgeted below one minute/512 MiB,
with small prose/receipts and no acquisition or API/model processing. Replan
contradictions or material supporting-system expansion; no new maintained
machinery is required. Unknowns are not an observation queue.

## Conditional terminal handling

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop` resumes
after ExecuteIndividualTurn, including its optional debug kill-all branch,
then orders ExecuteBattleCutscene_Defeated, ProcessKilledCombatants and
CountRemainingCombatants at `@IsBattleEnd`. Zero allies selects defeat
before zero enemies selects victory. With both counts nonzero it reads the
actual current turn byte/actor, runs ProcessAfterTurnEffects, then repeats
death processing and those ordered counts. That status pass has no second
defeated hook. Only if both tests still fail does CURRENT_BATTLE_TURN.b
advance by2 and dispatch continue. This establishes no actual completed turn.

The selected layout/data records are:

| Record | Battle27 value and use |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`, slot27/byte54, `$47C1E`|Relative `ms_Empty`; defeated wrapper.|
|`data/battles/global/enemyleaderpresence.asm`, slot27, `$47CA9`|Byte-1; admits shared living-enemy append.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`, slot27/byte54, `$47D2A`|Relative `ms_Empty`; after wrapper.|
|`data/battles/cutscenes/afterbattlejoins.asm`, slot27, `$47D85`|Byte0; tail calls JoinForce(Bowie0).|
|`data/battles/global/afterbattlepositions.asm`|Only Battle05 has a record; no27 override.|

`layout/sf2-07-0x044000-0x064000.asm` includes the **cutscenes** join table,
not the similarly named global copy. `map/ms_empty.asm` at `$47A4E` is the
word `$FFFF`. ExecuteMapScript still resets its loading-space pointer and
SKIP_CUTSCENE_TEXT before reading that terminator (the debug input path may
set the skip byte again). The actual termination path in
`map/mapscriptengine_2.asm` still waits for view scroll when dialogue is open
and clears VIEW_SCROLLING_SPEED.w. Empty programs do not remove their callers.

`cutscenes/battleendcutscenesstart.asm` requires Bowie HP.w nonzero and
enemy128 HP.w zero. Clear527 executes the empty script; held527 skips only
that script. Both continue to `battleendcutscenesend.asm:loc_47C48`, whose
nonzero leader-presence byte scans128–159 and appends each nonzero-HP slot
to the supplied DEAD_COMBATANTS_LIST, writes a trailing byte-1 and increments
its word length. It does not check X/Y, activation, membership, neutral state
or declared roster count, clear the list, deduplicate it, or zero HP.

Fresh128 is the declared Lizardman, not a named story antagonist; the
LEADER commandset does not establish this cleanup rule. With Bowie alive,
128's death can queue all other living slots even while their HP is positive.
Changed hidden/absent144–159 with nonzero HP are also appended. The actual
`cutscenes/afterenemyleaderdeathpositions.asm:ApplyPositionsAfterEnemyLeaderDies`
shares the Bowie/128 HP predicates but finds no27 table row, so has no
Battle27 relocation; it is not a return-coordinate override.

`battleloop/processkilledcombatants.asm` consumes the length-delimited list:
spin/sound/wait services, ally defeat or BATTLESCENE_FIRST_ALLY kill increments,
then X/Y=-1, status0, stat recomputation and mapped entity position
`($7000,$7000)`. It neither zeroes HP nor clears the list length nor calls
the item-drop transaction. Missing mappings and stale/duplicate entries
are not repaired by this consumer; interrupted services retain their prefix.

`battleloop/countremainingcombatants.asm` scans30 allies and32 enemies,
counting nonnegative signed X.b and nonzero HP.w. Y, active-party flags,
regions, AI and neutral bits are not predicates. Bowie HP0 forces ally count0.
Thus supplied off-map enemies can yield victory without the leader-death
hook, while simultaneous exhaustion loses. Nonzero high-bit HP also passes
these tests. HP/count termination does not prove death animation completion.

`battleloop/processafterturneffects.asm` clears death-list length on entry
and skips its effects for HP0. Its lethal-poison arm creates a new one-entry
list for the current actor before cleanup; it does not replay the action
list. If128 dies in that pass, only the subsequent death/count tests run
immediately; remaining living enemies are not appended by a second hook.
Shared status effects remain with the battle-loop owner, not a new simulator.
F88 continuation uses actual current selectors, lists, flags and combatants,
not an assumed fresh sixteen-enemy state.

The complete `data/battles/global/enemyitemdrops.asm` has **no Battle27 row**.
The existing [drop consumer](./runtime-rng-and-battle-math.md#confirmed-enemy-item-drop-data-and-core-runtime-branches)
matches actual CURRENT_BATTLE and target before its inventory/flag transfer.
Even a qualifying targetDies/action call cannot match27, so it performs no
Battle27 table drop. Battle26's Black Ring row10 is not inherited. Leader
cleanup, status death and victory do not substitute for action-side callers.
The selected empty after program and victory tail add no reward purse or
Higins scene/recruitment. This is not a claim that ordinary prior combat
earned no gold/experience; those accepted action consumers remain separate.

## Victory seed, shared join and completion

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` first calls
HealLivingAndImmortalAllies. Its all30-slot scan ignores membership: living
allies and immortal Peter7/Lemon28 even at HP0 receive max HP/MP, status
masked to STUN|POISON|CURSE and stat recomputation. Other dead allies stay
dead; the retained status bits are not newly inflicted. Battle44's timer
exception is not selected for27.

It next writes CURRENT_MAP.b to PARAM2 and calls
`text/textfunctions_1.asm:UpdateForceAndGetFirstBattlePartyMemberIndex`.
That rebuilds joined/active/reserve lists in ascending ID via UpdateForce,
then reads the first active byte **without a length or HP check**. Its actual
combatant X/Y plus BATTLE_AREA_X/Y are added as bytes into PARAM3/4.
GetEntityIndexForCombatant masks/remaps the ID and reads the physical lookup;
the result scaled by32 supplies ENTITYDEF facing into PARAM5. PARAM1 becomes0;
MAP_EVENT_TYPE is not written.

With unchanged Map74/crop `(2,16,32,48)`, the seed is map74,
`((x+2) mod256,(y+16) mod256)`, and that mapped entity's facing. It need not
be Bowie, a living/onscreen member or a formation start. An empty active list
exposes its retained first byte. A missing zero mapping selects physical0;
other supplied mapping bytes are not validated before the facing read.
Altered crop/map/coordinates change the seed; no after-scene position repairs it.

`cutscenes/afterbattlecutscenesstart.asm:ExecuteAfterBattleCutscene` checks
completion of the **actual** CURRENT_BATTLE. For27, clear527 runs slot27's
empty script, held527 skips it. Both reach EndAfterBattleCutscene, which
rereads CURRENT_BATTLE and consumes its join-table byte. For unchanged27,
`code/common/stats/battleparty.asm:JoinForce(0)` sets F0, rebuilds force lists,
then sets F32 only if active count is below12. Existing F32 is not cleared
at capacity; a previously inactive Bowie can remain reserved when full.
JoinBattleParty only sets the active flag, so a newly set F32 is not reflected
in these lists until a later UpdateForce. No stats are initialized, revived,
or re-healed, no join presentation/follower is installed by this tail, and
the earlier seed is not recomputed. This shared call occurs on repeat victory
too; the source comment calling the feature unused is not a no-op guarantee.

Only after the wrapper returns does victory reread CURRENT_BATTLE.b,
clear its unlock400+index, then set completion500+index. Unchanged27 clears427
then sets527. Intro477 is left as supplied; nothing here sets428–430,432,
530,811 or a Tristan-arrival flag. Those unlocks may already exist from the
admission-zone callback, but this return does not establish their history.

Finally it clears d0–d3 **words**, copies PARAM2–5 bytes into them, sets
d4=1 with moveq and returns. Upper halves of d0–d3 are not established by
clr.w. The ordinary returned words are74/member-derived X/Y/facing, not a
Tristan entrance. If CURRENT_BATTLE changes between completion test, script
selection, join tail and final flag writes, each reads its actual selector;
there is no snapshot tying them all to27. Changed request bytes are read at
return. Interruption after healing, seed, join, unlock clear or completion
set keeps the reached prefix, with no rollback or completed-return promise.

## Defeat and retreat alternatives

**Confirmed:** the selected shared routes are:

| Route | Ordered effects and return mode |
| --- | --- |
|Defeat|UpdateBattleUnlockedFlag; clear dialogue name1; MUSIC_SAD_THEME_2, text363 and close; Bowie HP=max; unsigned gold shift-right1; GetEgressPositionForBattle; d4=-1. Battle04's losable upgrade tail is excluded for27.|
|Admitted Egress|Deduct selected spell cost from actor MP; close battlefield windows; text274; common cast sound/flash/close; unlock update; unwind action frame; egress helper; d4.w=0.|
|Admitted Angel Wing|Remove selected actor item slot/recompute stats before window close/text275; same common escape tail and d4.w=0. No defeat gold penalty/Bowie HP restoration.|

The two admitted retreat actions escape the individual-turn frame; they do
not need to complete the post-turn terminal pass above. This consumes the
existing action dispatch, not a new spell/item/menu legality audit.
`battlefunctions/battlefunctions_1.asm:UpdateBattleUnlockedFlag` leaves427
alone if527 is clear and clears427 if527 is held. Neither defeat nor retreat
newly sets527, runs the after/join wrapper or changes477. They do not reverse
earlier combat/party/inventory writes. Later exploration healing can restore
eligible MP/HP; the immediate cost is not a promise of eventual values.

`battleloop/getegresspositionforbattle.asm` has special selectors for
13/16/17/31/32 and unlock clears for38/39, **not27**. Battle27 passes
EGRESS_MAP.b to `code/common/maps/egressinit.asm:GetSavepointForMap`, not the
victory seed or saved egress X/Y/facing. With F399 held (set by BattleLoop
on both fresh/resumed entry), the actual table supplies, for example,
Pacalon2 `(41,10)` Up, castle36 `(40,5)` Up, or Tristan15 `(12,16)` Up.
This last is an alternate savepoint seed, not the selected warp `(31,30)`.
Unmatched74 retains the map operand and defaults to `(1,1)` Up; other supplied
maps use their row/fallback. No previous church/save visit is proved.

Matched table fields are written as bytes; default X/Y/facing use moveq.
The helper does not globally clear the incoming map word before copying
EGRESS_MAP.b. Do not impose victory's zero-extended word return on this
path: later SwitchMap compares a word, although CheckBattle and exploration
use low-byte map tests. Direct F399-clear entry returns initial Map3 `(56,3)`
Down before raft reset; it is not the ordinary reached premise.

With399/F64 held the helper also resets raft fields: Polca/Bedoe selects72
`(43,48)`, Hassan selects69 `(28,55)`, otherwise69 `(42,15)`, including the
selected2/36/15/74 cases. Clear64 leaves raft fields unchanged. These writes
prove neither boarding nor eventual exploration. No alternate town's init
or general savepoint tour is included.

## Battle return and fresh Map74 exploration

**Confirmed:** all completed BattleLoop returns reach
`code/gameflow/mainloop.asm:alt_MainLoopEntry`: SwitchMap, then
ExplorationLoop, with **no d4 sign test or intervening CheckBattle**.
SwitchMap's complete table is3/F640→4,71/F530→78,58/F999→59, using word
map identity and first matching set flag. It has no74 substitution.
Retained427/432 therefore do not immediately re-admit combat on return.
Defeat/retreat follow their actual returned map, not an assumed74.

For explicit74, `exploration/explorationfunctions_2.asm:ExplorationLoop`
(under `code/gameflow/`) performs this ordered fresh lifetime:

1. Clear pending event word, leaving adjacent parameter bytes; subtract20000
   from STEP_COUNTER.w, with signed BGE or zero fallback. This is word/CCR
   arithmetic, not an unbounded clamp; ordinary30000 becomes10000.
2. Heal living/immortal allies again, request all-palette fade-out, set view
   target-1 and save the map word. These services must return.
3. Write CURRENT_MAP74 and CURRENT_BATTLE255, load selected tilesets, wait
   for fade and clear VInts. Get the current setup entity list, initialize
   entities/mapsprites, clear temporary flags256–383 and set80.
4. Initialize exploration sprite counters; LoadMap with saved map74 and
   d0=-1/view target0; install base VInts; run selected setup init. Request
   map music/fade-in only if the compared palette entries differ.
5. Set movement sound; enter WaitForEvent. Pending zero installs physical0
   control; pending nonzero bypasses installation and wins over A/C input.

`map/mapfunctions.asm:InitializeMapEntities` multiplies the supplied coordinate
words by384, calls ClearEntities, reconstructs flag followers, consumes the
fixed list and declares main physical0 with Bowie sprite rules and supplied
facing. ClearEntities resets actor records/lookups and walking scratch.
Battle entities/camera crop/temporary scripts are not an exploration roster.
The first active member selected the return coordinates, but main remains
Bowie. Current HP/class/flags feed the actual sprite/declaration consumers;
join membership alone is not a follower declaration.

Map74 is an overworld and is not follower-excluded. F65 selects the Caravan
prefix before ordinary flag followers; the accepted follower table drives
the rest and the rebuilt list is terminated. Raft construction uses its
separate64/map/coordinate/mode gates. BattleLoop cleared PLAYER_TYPE.b and
the selected outcomes do not change it, so the ordinary return retains
numeric0. Exploration itself does not reset it. Changed mode1 selects
`eas_Raft`, other nonzero selects `eas_Caravan` in SetControlledEntityActScript;
preserve these actual branches rather than repair their enum-name mismatch.

The one Map74 setup is unflagged `msMap 74, ms_map74`; retained map-init
routeJoins sourceOrder126 binds pointer table `$5096A` to init `$509D6`.
The six pointers order entities, entity events, zones, descriptions, items,
init. Fixed entities `$50982` contain only msEntitiesEnd; init is RTS.
No527/530 alternate, new NPC/recruitment or automatic zone callback exists
in this fresh init. Shared main/follower/raft construction still applies.

The header `$C362E` selects palette0/tilesets101,102,109,110,105 and
Map74s0_Blocks/Map74s1_Layout. The one area `$C365C..$C367C` is `(0,0)..(63,63)`,
zero other-layer origins, parallax256/256, zero autoscroll, main-layer type254,
MUSIC_MAIN_THEME. Flag/step/roof tables contain only endWord. Explicit loading
reloads blocks/layout and full area metadata; CURRENT_BATTLE255 prevents the
old battle crop from overriding the full rectangle. Crop RAM bytes need not
be zero to be inactive. Empty step records do not disable the movement counter
or layout-kind zone/warp producers. Completed VInt/DMA/fade/control is unobserved.

Story/membership/completion flags outside256–383 survive the temporary clear.
A supplied CURRENT255 request takes the different existing reload branch:
healing/counter/fade/main/raft/load/init still run, but no population rebuild,
CURRENT_BATTLE reset or temporary clear. It retains loaded block/layout data;
stale battle state can affect area selection. A scroll also retains actors
and does no MainLoop battle scan. Neither alternative is silently treated
as the explicit74 victory return.

## Explicit Tristan request and actual admission

**Confirmed:** Map74 warp row2 at `$C3692..$C369A` is encoded as eight bytes
`52,20,0,15,31,30,1,0`: source `(52,20)`, no-scroll0, map15, destination
`(31,30)`, Up1 and padding0. `sf2mapmacros.asm` owns these widths. Earlier
rows at `(3,25)`/`(4,25)` go to Moun and cannot match this source pair.
Later bridge47, south71 and west75 exits remain alternatives, not traversed
routes. No527/477, item, party or dialogue predicate is attached to row2.

Ordinary character `entity/entityscriptengine_2.asm:esc02_controlCharacter`
reaches WarpIfSetAtPoint only on masked layout kind `$1000`, after its
movement/overlap/layout calculations. Its supplied d0/d1 already include
the movement deltas; they are candidate coordinates, not a completed-arrival
sample. Obstruction/debug/flags can still affect movement after event posting.
The row is therefore not evidence of a navigable cell/path or an actual visit.
Other controlled modes have their own callers; do not inherit this walking
admission simply from their enum names.

`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint` sign-extends
coordinate words, divides by384, reads the actual current map's warp table,
tests word `$FFFF` terminator first, then compares low bytes in order.
Any negative source byte is a wildcard (broader than the zone selector's
exact255 rule); selected52/20 are exact. First match writes event word1 at
`$FFA84A`, PARAM1–4 as a longword at `$FFA84C`, PARAM5/padding as a word at
`$FFA850`, then WARP_SFX. No match leaves pending state intact. There is no
pending guard, immutable payload or queue; these are separate ordered writes.

WaitForEvent selects the pending event before A/C. ProcessMapEvent clears
the stored event word before dispatching its read value. Zero PARAM1 takes
ProcessMapEventType1_Warp's no-scroll branch: submit/clear WARP_SFX, idle main,
remove the exploration return address, clear d0–d4 words, read PARAM2 into
d0.b, then call UpdatePlayerPosFromMapEvent. That samples **actual** main
X/Y words using signed division by384 and clears/reads actual facing.
Nonnegative PARAM3/4 bytes replace only the coordinate low bytes; negative
bytes preserve the sampled words. PARAM5 replaces facing; PARAM1 supplies
d4.b. The reads occur around services, not as one snapshot.

For intact row2 and ordinary nonnegative samples, the returned words are
d0=15, d1=31, d2=30, d3=1, d4=0. With changed negative samples, the quotient's
high byte can survive (for example `$FFxx` becomes `$FF1F` for X31); the
producer's coordinates are not reused. Changed negative destination bytes
preserve their sampled word, and changed type/map/facing/pending writes can
select another continuation. This return is a request; it does not itself
place an entity inside Tristan or finish map loading.

Exploration's return reaches MainLoop `@Start`, which calls **SwitchMap then
CheckBattle**. The complete switch table has no15 entry; the complete45-row
BattleMapCoordinates table has no15 row. Thus intact explicit15 yields
d7=-1 and restores the original map operand15; no crop or battle flag write
occurs regardless of427/432,527/532,530 or811. MainLoop takes `@Exploration`
and calls `j_ExplorationLoop` with the returned seed. **This call, before
Tristan's exploration body or init, is the endpoint.** No natural arrival,
initialized population, dialogue, map services or usable Tristan control is
established. Changed supplied coordinates do not create a BattleMap row.

### Alternate pending and zone state

**Confirmed:** fresh Map74 load does not rescan battle tables or invoke its
zone table just because main is at a coordinate. A separately admitted
kind `$1400` producer writes event6 and candidate X/Y words to PARAM1/2 and
PARAM3/4. The current setup's zone selector has `(255,61)→Map74_ZoneEvent0`
and a default RTS; exact255 is wildcard and253 is the default marker.
The accepted [complete callback/consumer](./pacalon-departure-to-battle27-admission.md#callback-order-and-partial-reload-overwrite)
retains these branches:

| State when callback reads flags | Effects |
| --- | --- |
|527 clear|Set427; write reload longword; set428,429,430; return.|
|527 held,811 clear|No new writes.|
|527 held,811 held,532 held|No new writes.|
|527 held,811 held,532 clear|Set432; write reload longword; return.|

The callback does not clear existing unlocks, call CheckRandomBattle or
establish later story history. Its `$000100FF` reload longword writes only
event1/PARAM1=0/PARAM2=255; PARAM3–5 and sound remain supplied. An intact
zone Y word61 reinterprets as X byte0/Y byte61, not two sentinels. Cleanup
then closes windows and waits for VInt/motion before returning; a posted
request does not prove those waits completed. A newer pending event can
replace the selected Tristan request or be overwritten by it.

Only a subsequent consumed no-scroll return reaches MainLoop's battle scan.
For CURRENT255 resolving to74, held427 selects27 before32; otherwise held432
selects32; with neither held, d7=-1. Each admitted row writes crop `(2,16,32,48)`;
held completion clears its unlock but still admits on that call. No-match
restores original255 and takes the distinct CURRENT reload above. These
are alternate handoffs only, not Battle32 execution or a reason to infer
Tristan arrival. An intact explicit15 warp does not resolve CURRENT_MAP74
and is not diverted by these unlocks.

## Changed states, reproduction and limits

| Changed premise | Required distinction |
| --- | --- |
|128 death, off-map enemies, simultaneous exhaustion|HP-only leader append versus X/HP counts; defeat precedence.|
|Post-status leader death, stale/duplicate list, absent slots with HP|No second hook in that pass; no list/HP/mapping repair.|
|527/477 held, actual CURRENT_BATTLE changed|Empty-script gating, shared join and later flags read independently; intro retained.|
|Empty/dead/full party, new F32, missing mapping, changed crop|Seed precedes join; no length/HP guard, facing validation or seed rebuild.|
|Retreat/defeat, EGRESS_MAP/F399/F64 changes|Path-specific costs, savepoint/raft and word/byte contracts; no forced74 return.|
|Explicit74 versus CURRENT/scroll; altered mode/followers|Fresh actors/full area/current-battle reset differ from retained lifetimes.|
|Layout kind/candidate movement versus actual main sample|Producer admission is not arrival; consumption preserves high/sentinel bytes.|
|Flags change, pending slot replacement or interrupted services|Only reached prefix and actual later reads; no atomic request, rollback or assumed completion.|
|15 unchanged versus CURRENT74 alternate|No Tristan battle row;27/32 selection belongs only to the alternate map scan.|

Load ignored `local/private-inputs.ps1` in the launching process. Reuse the
clean pinned source and dependency-valid
[retained-pair recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
without regeneration. Direct review compares selected end/leader/after/join/
position rows and actual layout inclusion; `_build_lifecycle_facts`,
`_build_control_facts` and `_cutscene_facts` against existing fixtures/schemas;
the complete drop table and actual consumer; terminal/status/death/heal/seed/
join/flags/egress/MainLoop order; and Map74 setup/payload/area/warp/zone records
against request, map, party/follower and admission consumers. No aggregate
count substitutes for the selected row/body, and no former runtime fixture
is promoted to a natural Battle27 observation.

Reproduce the bounded ignored review with `uv run --locked python -X utf8
local/issue1041/review.py <configured-source-disasm-root>`. Review all prose,
changed states and cached diff; local links/anchors, UTF-8/LF, sole-path/private
boundary and `git diff --check`. On the clean committed candidate run
`uv run sf2 verify plan --base origin/main --head HEAD`; retain actual scoped
CI for that exact head in the Issue handoff. Documentation scope skips blanket
normal/full/Python/.NET/Godot/H3/H4 suites and tests of review programs.

**Unknown:** natural terminal reach/outcome/walk, actual combatants/party/
stats/lists/mappings/flags/mode, egress/current/request state, candidate layout
admission, completed movement/text/music/fade/DMA/VInt services, timing,
rendering, usable exploration and save persistence. Optional missing compressed
layout remains a no-decoded-route limit, not a ROM blocker or acquisition
authority. No completed suite or failure is reset.

Preserve [#1037 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1037#issuecomment-6183776676)
and [PR1039 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1039#issuecomment-6183789330):
author0.340s/initial0.329s/independent0.332s/CI38676678592, Region1 negative-
projection correction and linked #1033 original/rebased results, schema-path
failure/local-$ref correction and Unknowns remain distinct retained evidence.
Preserve configuration, source/pair and all receipts. Freeze a clean current-
main-based committed/pushed Draft PR, Project Review and exact identity/results
handoff with stopped writer/processes and successful main-gate callback.
No merge, cleanup, archive or next Issue.
