# Battle23 Outcomes and the Return to Willard Burrow

**Confirmed:** Battle23 can end through leader cleanup or an empty enemy count.
Victory heals eligible allies, marks the battle complete and returns to a
position derived from the current party. A fresh Map27 return rebuilds the
full burrow with no residents. Defeat and admitted Egress/Angel Wing instead
use a savepoint selector, with different costs and recovery effects.
**Inferred:** the changed resident selection removes Willard's confrontation
population. **Unknown:** natural combat/traversal, supplied state, completed
services, presentation, usable control and persistence.

This English guide continues [Battle23 initial formation and suspended entry](battle23-initialization-and-roster.md),
using [corrected accepted outcome research](../../research/battle23-outcomes-and-burrow-return.md)
under [#942](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/942)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
It begins at the post-action end check; it does not simulate the turns between
initialization and that seam. It ends at conditional burrow exploration or
next-map admission, before destination initialization or any Creed scene.
The continuation [Creed's restoration and main-room departure](creed-restoration-and-basement-departure.md)
begins at separately admitted Map33 loading and preserves this guide's exit limits.
Fnnn means flag nnn; slots are zero-based. Examples below discriminate source
branches under supplied premises; they are not executed scenarios.

## What Ends the Battle

**Confirmed:** after an individual turn and the optional debug kill-all branch,
the first end check runs the defeated hook, processes the dead list, counts
remaining combatants, then tests **zero allies before zero enemies**. If both
counts remain nonzero, after-turn effects run, followed by another cleanup
and the same ordered counts. That second check does not call the defeated hook.
The debug branch establishes no natural outcome.

A counted combatant needs nonnegative X and nonzero HP. The count scans all
30 ally and 32 enemy slots, without a joined/active membership test. Bowie's
HP0 forces the ally count to zero even if others survive. Simultaneous empty
counts therefore mean defeat; Bowie alive still needs a nonempty counted
ally set for victory.

### Willard's Death Can Remove the Remaining Enemies

The defeated wrapper requires Bowie alive and enemy 128 HP0. For the accepted
fresh roster, the identities are:

| Type | Logical combatants |
| --- | --- |
| Willard | 128 |
| Five Rats | 129, 130, 132, 134, 137 |
| Six Bubbling Oozes | 131, 133, 135, 136, 138, 139 |

These interleaved identities come from the predecessor's complete twelve-row
[formation table](battle23-initialization-and-roster.md#willard-and-the-complete-enemy-formation).
A supplied/resumed roster must establish its own identity premise.

Battle23's defeated table selects `ms_Empty`. F523 clear dispatches it;
F523 set skips that script. **Both paths still reach the common leader tail**:
the selected leader byte is -1, enabling a scan of enemy 128–159. Every
nonzero-HP entry is appended to the existing dead list, followed by a -1 byte
and an incremented length. This does not test placement, clear HP, reset the
list or deduplicate it. Willard's already zero-HP slot is not appended by
this tail; any ordinary death-list entry for him comes from earlier work.

Cleanup processes the length-delimited list, including spin/death-sound
requests, before bookkeeping: ally defeat or enemy kill credit to
BATTLESCENE_FIRST_ALLY, X/Y=-1, status clear, stat recomputation and mapped
entity placement at raw `(0x7000,0x7000)`. It neither zeroes listed HP nor
clears the list length. Appended survivors cease to count through negative X.
An empty list returns immediately. Stale/duplicate/invalid list entries and
actor mappings are supplied problems, not repairs performed by this wrapper.
All required presentation/VInt/sleep services must return before counting.

Thus Bowie surviving and Willard HP0 at the **first hook seam**, with a
coherent placed party, valid list/mappings and completed services, permits
victory without individually killing every Rat/Ooze. If Willard dies only
in after-turn effects, the second check does not run that leader tail:
remaining counted enemies still prevent victory. Conversely, zero counted
enemies permits victory without using the leader tail, provided allies remain.

There is no local death dialogue or placement override to add: the defeated
and after tables select the terminator-only script, the complete leader-position
table has only Battle05, and the region-script table has only battles 32/40.
The [research table/wrapper join](../../research/battle23-outcomes-and-burrow-return.md#post-action-checks-and-leader-cleanup)
owns the exact selected slots and complete exclusion checks.

## Victory Returns from Current Party State

**Confirmed:** the victory body performs this sequence:

1. Heal all eligible allies across the thirty slots, independently of membership.
   Living allies and Peter (7)/Lemon (28) even when dead receive maximum HP/MP;
   other dead allies stay dead. Retain existing STUN/POISON/CURSE bits, then
   rebuild stats/equipment effects. Battle44's timer exception does not apply.
2. Seed the return: PARAM2=CURRENT_MAP; rebuild force lists; take the first
   BATTLE_PARTY_MEMBERS byte; add that combatant's X/Y to BATTLE_AREA_X/Y into
   PARAM3/4 using byte arithmetic. Read the actual mapped entity facing into
   PARAM5 and set PARAM1=0. This does not write MAP_EVENT_TYPE.
3. Run the after wrapper. F523 clear dispatches `ms_Empty`; set skips it.
   Both reach the common `JoinForce(0)` tail for Bowie.
4. Clear F423, set F523, read PARAM2–5 bytes into cleared d0–d3 words and
   return d4=1.

### Selecting the Member and Coordinates

UpdateForce scans IDs 0–29 ascending and requires joined **and** active flags
for the battle-party list. It does not choose the first living member. Normal
joined, active Bowie is first because he is ID0, not because victory hardcodes
him. A supplied dead/off-map first member can seed the return; a zero-length
list leaves the first byte uninitialized by that rebuild. The later Bowie
join does not repair a seed already taken from that byte.

For current Map27 and the accepted area `(23,14)`, a selected current local
position `(x,y)` yields `(x+23,y+14)` modulo 256, with facing read through
ENTITY_INDEX_LIST from that member's physical entity. For example, a selected
member currently at `(6,4)` facing Left yields Map27 `(29,18)`, Left. Changing
membership, area, position, map or mapping changes the result. Neither the
admission `(0,0,Right)` request nor the initial formation is a victory location.

JoinForce(0) sets Bowie's joined flag F0 and rebuilds force lists. If the active
count is below 12, JoinBattleParty also sets F32; that write does not immediately
rebuild the counts again. A full party is not evicted to make room. This common
tail adds no Creed recruit and does not promise a safe arbitrary empty-party state.

An empty script still initializes work state/text-skip handling and, if a
dialogue window exists, waits for view scrolling before clearing scrolling
speed and returning. An unfinished script prevents the join/flags; an unfinished
join prevents the clear/set. Completed prefixes remain. The empty script
supplies no replacement tuple; a competing parameter write before the final
read can change the returned bytes without waiting for a warp to be consumed.

All three Battle23 outcomes resume MainLoop's **alternate entry**: SwitchMap then
ExplorationLoop, bypassing CheckBattle. That caller does not branch on d4's
sign. No flag-switched Map27 substitution changes this explicit return.
The [victory consumer join](../../research/battle23-outcomes-and-burrow-return.md#victory-order-and-the-actual-return-seed)
retains the exact seed, mapping, wrapper and caller provenance.

## Defeat and Escape Use a Savepoint Selector

**Confirmed:** Battle23 has no special defeat/Egress destination arm.
Already-admitted handlers differ as follows; menu availability or actual
spell/item custody is not established by these handlers alone.

| Outcome | Recovery or cost before return | d4 |
| --- | --- | --- |
| Defeat | Update unlock; clear dialogue name; request sad music/exhaustion text363 and close text; restore Bowie's maximum HP; halve unsigned gold by floor division; obtain Egress position | -1 |
| Egress | Decrease the acting combatant's MP by the spell cost; close battlefield windows; request cast text and shared sound/flash/close; update unlock, unwind action frame and obtain Egress position | 0 |
| Angel Wing | Remove the selected item slot instead of MP cost, then the shared escape tail | 0 |

Escape does not perform defeat's gold loss or Bowie-HP restoration. Defeat's
body does not revive other dead allies; later exploration independently heals
living/immortal allies. None runs the victory after/join or sets F523.
UpdateBattleUnlockedFlag preserves F423 when F523 is clear, and clears F423
when F523 is set. An earlier admission may already have cleared F423 for a
supplied completed encounter. Text/audio requests require their services to
complete; they are not observations of presentation.

GetEgressPositionForBattle reads **EGRESS_MAP as a map selector**, then looks
up a savepoint row. It does not retrieve the party's previous XYZ/facing tuple.

| Supplied state | Selected return or raft effect |
| --- | --- |
| F399 set, selector 26 | Floor World26 `(4,36)`, Up1 |
| F399 set, selector 22 | Desktop22 `(5,8)`, Up1 |
| F399 set, selector 27 | No savepoint row: Map27 default `(1,1)`, Up1 |
| F399 set, other selector | Matching four-byte savepoint row, otherwise that map with default `(1,1)`, Up |
| F399 clear, direct inconsistent call | Initial Map3 `(56,3)`, Down3; early return before raft-reset work |
| F399 set, F64 clear | Preserve saved raft fields |
| F399 set, F64 set, selector 26/22/27 | Default RAFT_MAP69, RAFT_X/Y `(42,15)`; no boarding |

BattleLoop sets F399 before fresh or suspended entry, so its clear example
is a supplied-call counterexample. The named selectors have no special raft
row; other Hassan/Polca/Bedoe rows do not change those examples.

Floor World's completed first-arrival init sets F779 and then EGRESS_MAP26;
F779-set repeats skip that assignment. Burrow init, confrontation and Battle23
hooks/outcomes do not replace it. **If that earlier selector is retained**,
defeat/escape returns to Map26. Entering the burrow, or merely supplying F779 set,
is insufficient to promise Floor World. See the accepted
[Floor World arrival](floor-world-first-arrival.md) and the
[savepoint consumer join](../../research/battle23-outcomes-and-burrow-return.md#defeat-egress-and-the-savepoint-selector).

The alternate MainLoop return bypasses CheckBattle for all three outcomes.
A preserved F423 therefore does not immediately readmit Battle23 there; a later
normal MainLoop pass may use it. Actual returned map/state controls the loader.
For Map26/22 this guide stops at admission, before their destination init.

## A Fresh Burrow Return Replaces the Battlefield

**Confirmed:** the explicit Map27 victory return takes fresh ExplorationLoop
setup: clear event type, adjust the step counter, heal eligible allies, request
fade-out and disable camera following; assign CURRENT_MAP=27/CURRENT_BATTLE=255;
load tilesets, wait fade/VInt work and rebuild entities/mapsprites. Clear
temporary flags 256–383 and set F80. F423/473/523/781 survive that reset range.
Common loading initializes sprites, targets physical 0, loads the map, installs
base VInts and calls map init.

LoadMap receives d0=-1 for view selection but **d1=27 for full loading**.
With CURRENT_BATTLE=255 it skips battle cropping and restores the full burrow
rather than retaining the 27x16 battlefield. Map27 has no flag-event layout
copy or chest overlay: both complete files contain only endWord. F523 changes
resident selection, not a demonstrated wall/collision opening.

| F523 at fresh selection | Resident list |
| --- | --- |
| Clear | Rat79 `(43,20)`, Willard154 `(43,21)`, Rat79 `(43,22)`, all Left2/eas_Init |
| Set | Terminator only: zero residents |

The setup pointer tables share callbacks, zones, descriptions, Section5 and
init; only the resident-list pointer differs. Each accessor reselects setup.
Victory sets F523 before this selection. Actor records/lookups are cleared;
followers allocate from physical 1, residents follow, and Bowie is declared
at physical 0 with the return tuple/eas_Idle. Main coordinates multiply by 384
without the resident rows' six-bit coordinate mask. An empty resident list
still permits the main actor and supplied followers.

Only F67/F68 in the ordinary follower example allocate Peter (7) at physical 1
and B (logical31)/Oddler at physical 2; membership alone does not. Other flags shift
allocation/aliases and mode/raft rules remain applicable. BattleLoop cleared
PLAYER_TYPE to 0; these outcomes/init do not change it. The predecessor's
[complete population owner](../../research/willard-burrow-through-battle23-entry.md#admission-setup-and-the-complete-population)
retains additional allocations and logical/physical/raft aliases.

Battle enemy 128 is Willard, whereas baseline exploration logical 129 is Willard
and 128/130 are Rats. With F523 set, resident lookups 32–34 remain zero after the
clear. An absent-resident script lookup resolves to physical 0, not a safe
no-op. Battle actor mappings cannot be reused for this fresh exploration list.

Map27 init is **rts only**. It adds no dialogue, placement, reward, warp,
EGRESS assignment or F781 repair. After conditional music/fade requests,
WaitForEvent handles pending events first. Only no pending type permits
physical 0's current-mode control action; mode 0 selects eas_ControlledCharacter.
Returning init alone proves no completed services or usable input.

### CURRENT255 Keeps a Different State

A supplied map 255 reentry retains current map/population, skips fresh tilesets,
allocation, temporary reset and CURRENT_BATTLE assignment, updates main and
raft/followers, then reuses the working layout and calls init. F523 alone
cannot remove already allocated residents on this path or normalize retained
battle state. This is distinct from ordinary explicit Map27 victory loading.
The [fresh/retained return join](../../research/battle23-outcomes-and-burrow-return.md#fresh-map27-return-and-conditional-exploration)
owns both complete load arms.

## Existing Exits Remain Conditional Requests

**Confirmed:** four independent flags have different consumers:

| Flag | Consumer and outcome effect |
| --- | --- |
| F423 | Battle admission; victory clears it, defeat/escape clear it only with F523 set |
| F473 | Before/start intro gate; start sets it, outcomes/init do not |
| F523 | Completion and fresh resident selection; victory sets it; no direct exit predicate |
| F781 | Y19 confrontation gate; callback sets it after scene return; outcomes/init do not |

F523 set/F781 clear is not repaired. A later Y19 zone invocation may address
missing residents through physical 0 and fail to complete normally. If services
do complete, its existing scene can set F423 and request Battle23 again; F523
then clears F423 during CheckBattle but does not cancel that selected encounter.
F781 set instead suppresses the confrontation even with F473 clear. Its caller
flag write and pending request precede portrait/text/VInt/physical 0-stop waits;
neither proves wrapper return or request consumption. The complete confrontation
remains in the [admission guide](willard-burrow-through-battle23-entry.md).

The unchanged table contains eight no-scroll/type 0 requests:

| Row | Trigger X/Y | Destination map / X/Y | Facing |
| --- | --- | --- | --- |
| 0 | `(2,8)` | Floor World26 `(59,4)` | Down3 |
| 1 | `(11,10)` | CURRENT255 `(30,8)` | Right0 |
| 2 | `(30,8)` | CURRENT255 `(11,10)` | Left2 |
| 3 | `(33,13)` | CURRENT255 `(12,15)` | Left2 |
| 4 | `(12,15)` | CURRENT255 `(33,13)` | Right0 |
| 5 | `(54,2)` | CURRENT255 `(3,3)` | Right0 |
| 6 | `(255,0)` | Creed Mansion33 `(15,9)` | Right0 |
| 7 | `(21,255)` | Creed Mansion33 `(15,9)` | Right0 |

Each record is eight bytes including zero padding after facing, then endWord
after the table. Negative trigger bytes are wildcards: row 6 means any X at Y0,
row 7 means X21 at any Y, with first match winning. No reverse `(3,3)` row exists.
Facing is an output. The matcher/table tests none of F423/F473/F523/F781, consumes
no item and grants no recruitment.

Ordinary esc02 requires candidate layout class 0x1000 under mask 0x3C00 before
the warp matcher signed-divides raw X/Y by 384 and scans. Coordinate equality
alone proves no walkable route. The separate zone branch uses class 0x1400 and
low-byte selectors. A match requests event type 1, PARAM1–6 and warp sound;
pending consumption and service completion remain necessary.

ProcessMapEvent clears its sampled event type before dispatch. The type 0 warp
consumer submits/clears sound, idles physical 0 and returns d4=0 to **normal
MainLoop**. It derives signed position words from physical 0 and overrides only
the low bytes with nonnegative PARAM3/4. Explicit destination bytes do not
repair abnormal inherited high bytes; coherent tuples above assume that limit.

SwitchMap substitutes none of 26/27/33/255. Explicit 26/33 have no battle row
and admit exploration; stop before their population/init. CURRENT255 resolves
current Map27 for CheckBattle: F423 clear preserves the 255 request for retained
exploration; F423 set selects Battle23 and writes area `(23,14)`,27x16. F523 set
clears F423 during that selection but still admits the battle. These are
conditional consumers, not a maze solution, completed escape or Creed scene.

## Evidence Review and Remaining Limits

**Confirmed provenance:** accepted main `6498fd588d3ed12b9d38392d97778e9e4b90af90`
contains corrected #937/PR939 and #938/PR940. The research pins
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Its [reproduction owner](../../research/battle23-outcomes-and-burrow-return.md#reproduction-and-remaining-boundaries)
retains exact source symbols/addresses, table slots and full consumer joins.
No new observation, extraction or #941 finding is used.

Adversarial review covered the complete corrected research, predecessor
formation/admission and selected control contract, public fixture payloads,
schemas and verifiers. The exact twelve identities, 1/5/6 counts, eight warp
rows and outcome order were compared individually, not inferred from totals.
The selected table joins are slot23 of defeated48, leader45, after48 and join52;
the cutscenes join table, not its similarly named global counterpart, owns Bowie0.

| Executable owner | Claim and limit |
| --- | --- |
| `sf2-battle-loop-static-v1`, tests/fixtures/h2/battle-loop-static-v1.json | expected.lifecycleFacts: 30/32 slots, X/HP count, Bowie override, healing and cleanup. Matching h2-battle-loop-static-fixture schema; battle_loop.py `_build_lifecycle_facts`/verifier. Debug kill-all's list reset/HP clear is not the leader tail. |
| `sf2-battle-control-static-v1`, tests/fixtures/h2/battle-control-static-v1.json | expected.controlFacts: two end checks, outcome order, d4 and gold operation. Matching control fixture schema; battle_control.py `_build_control_facts`/verifier. Ordered source fragments do not prove natural execution or seed correctness. |
| `sf2-battle-cutscenes-static-v1`, tests/fixtures/h2/battle-cutscenes-static-v1.json | expected.cutsceneFacts: HP predicates, completed-script skip, leader append and shared join. Matching cutscenes fixture schema; battle_cutscenes.py `_cutscene_facts`/verifier. Research separately owns exact slot 23 table/body selection. |
| `sf2-map-init-static-v1`, tests/fixtures/h2/map-init-static-v1.json | Setup/init route and embedded-program comparison; matching map-init fixture schema and map_init.py verifier. Return-only init establishes no usable control. |
| `sf2-map-script-engine-static-v1`, tests/fixtures/h2/map-script-engine-static-v1.json | Shared script/request consumers; matching map_script_schema.py fixture schema and map_script_engine.py `_verify_map_script_engine_fixture`. It does not execute this composed outcome or prove pending delivery. |

Schemas are under `schemas/`; verifiers under `src/sf2tool/h2/`. Existing narrow
commands use `uv run sf2 h2` followed by `battle-loop`, `battle-control`,
`battle-cutscenes`, `map-init` or `map-script-engine`. The retained research
comparisons passed at their named boundaries; this design checkout lacks the
pinned upstream/retained pair, so they are not fresh reruns or regenerated data.
Aggregate equality cannot guard the new prose semantics.

The [original #937 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177275252),
[P2 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177302395),
[corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177356550)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177369382)
retain original/corrected results and the superseded contiguous-ID error.
The [#938 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/938#issuecomment-6177356652),
[rebase handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/938#issuecomment-6177400955)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/940#issuecomment-6177419250)
retain initialization results, absent-input diagnostics and predecessor failures.
These records remain authoritative for their exact objects, CI and receipts.

Direct whole-prose/diff, table/order/changed-state, links/anchors, English/private
checks and git diff --check apply, then the configured clean committed planner
and actual final-head CI. No blanket local suites, acquisition or new machinery
are selected solely for this prose.

**Unknown:** natural leader/all-enemy outcomes and exit reach; supplied dead
list, party/HP/placement/mappings/flags/current or saved geometry, EGRESS_MAP,
mode/raft; completed script/load/presentation/input services; visible/audible
timing, usable control and persistence. Empty/full-party and interrupted-prefix
examples retain those limits. [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../decisions/0016-remake-start-evidence-deferral.md) leave no
automatic runtime queue. Destination init, basement/recruitment, Creed scenes
and later battles remain outside this guide and remake acceptance.
