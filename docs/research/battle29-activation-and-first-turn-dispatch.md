# Battle29 activation and first-turn dispatch

Continue with [Battle29 outcomes and shrine exterior route](./battle29-outcomes-and-shrine-exterior-route.md).

Evidence date: 2026-10-20. **Confirmed:** all twelve declared Battle29
formation positions miss all four regions under the actual integer consumer.
The fifteen initialized enemies still qualify for turn records: checked
regions, persistent flags, activation, spawning and turn admission are distinct.
Follow round preparation from `BattleLoop:@Start` to the first
`ExecuteIndividualTurn` call; stop before its body. **Inferred:** this
formation stages contact before entry into the declared regions. **Unknown:**
natural reach, actual incoming state/RNG, first actor, completed services,
presentation, usable control and persistence.

## Agreement and provenance

Standard rigor for [Issue #1060](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1060)
under [epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053).
Accepted starting base is `adc476ff314d8be37bf2aced2f3f60479c060174`, including
#1058/PR1059. This document and one navigation link from the
[initialization/opening owner](./battle29-initialization-and-opening.md) are
the exact owned paths. No shared index, schema, fixture, tool, engine, design
or translation changes. No AI/action body, combat, outcomes, Map0 post-battle
scene, runtime, extraction, seed enumeration or runtime-readiness claim.

Fresh entry starts only after the predecessor's LoadBattle/start wrapper
returns. F479 is set before the empty start dispatch; it does not certify
completed opening services. F88 is a separate supplied-state boundary.
Reuse [Battle27's shared consumer organization](./battle27-activation-and-first-turn-dispatch.md),
[Battle26's activation/spawn/turn derivations](./battle26-activation-and-first-turn-dispatch.md),
[battle loop](./battle-loop.md), [placement](./battle01-placement.md),
[RNG/turn evidence](./runtime-rng-and-battle-math.md), [cutscenes](./battle-cutscenes.md),
[lifecycle](../design/contracts/battle-control-lifecycle.md) and
[routing](../design/contracts/battle-cutscene-routing.md). Their fixtures prove
their named seams, not natural Battle29 traversal; Battle29 rows/geometry
are independently checked below.

Paths are relative to `disasm/` at clean, read-only pinned
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing evidence binds USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
this slice does not rebuild or newly observe it. Constants/widths come from
`sf2enums.asm`, `sf2const.asm` and actual macro/consumer operands. Fnnn denotes
flag nnn, indices are zero-based, address intervals half-open. Short
`battleloop/` and `cutscenes/` paths are under `code/gameflow/battle/`.

Minimum evidence is selected source, existing fixtures/consumers, and 12x4
bounded integer calculations. Reuse unchanged 18,285,979-byte init/script pair
and accepted 0.083s provenance without extraction. Linear selected reads/joins
plus 48 region checks are budgeted below one minute/512 MiB, with small
receipts and no acquisition/API cost. Contradictory evidence or material
supporting-system expansion requires reassessment, not a new platform.

## Ordered prefix and supplied entry

**Confirmed:** `battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`) orders:

1. ActivateEnemies, including its extra fall-through helper call for160.
2. `j_ExecuteBattleRegionCutscene`.
3. Test DEBUG_MODE_TOGGLE.b. Zero skips PrintAllActivatedDefCons; nonzero
   invokes its sixteen flag checks90–105, requesting text463 for held flags
   with DIALOGUE_NUMBER.l set to region index, then closing text.
4. `j_PopulateTargetsListWithSpawningEnemies`; read TARGETS_LIST_LENGTH.w.
   Zero skips spawning. Otherwise decrement count, traverse TARGETS_LIST
   bytes in stored order, clear d0.w before each ID read and call
   SpawnEnemyWithCamera via DBF.
5. GenerateBattleTurnOrder, then `@IndividualTurns_Loop`.
6. Clear d0.w, read CURRENT_BATTLE_TURN.b as a **byte offset**, point a0 to
   BATTLE_TURN_ORDER, read selected combatant byte into d0.b. `$FF` returns
   to `@Start`; otherwise `bsr.w ExecuteIndividualTurn` is the endpoint.
   No HP, score, membership, activation or outcome check intervenes. d0.w is
   the zero-extended ID; its upper word is not cleared. No body/return is claimed.

Every preceding called service must return. A missing region-table match
does not erase activation, debug, spawn-list, RNG or turn-buffer effects.

The [F88 entry](./battle29-initialization-and-opening.md#f88-and-changed-state-cases)
restores saved seconds, clears88/AI memory/view target, calls LoadBattle
with incoming d0 (0 on the named Map0 entry), and branches directly to
`@IndividualTurns_Loop`; the following branch to `@Start` is unreachable.
It initially bypasses this preparation and consumes supplied offset/buffer
without alignment/range validation. A non-FF byte reaches the call even if
fresh admission would reject its ID. A sentinel enters `@Start` with actual
current battle/flags/positions/HP/modes/AGI/RNG. Incoming map0/battle29 words
do not repair current selectors on F88; LoadBattle rebuilds physical entities,
so this is neither byte-for-byte preservation nor safe arbitrary state.

## Activation and the complete formation/region join

**Confirmed:** `battleloop/activateenemies.asm` (`$2550C..$25544`) scans
128–159 using start128/counter31, preserving its counter around each helper.
UpdateEnemyActivationBitfield requires nonnegative signed X.b, nonzero
current HP.w, and negative d0.b. It does not test Y, membership, neutral bit,
orders or commandset. High-bit HP passes the nonzero test. Fresh128–142 pass;
absent143–159 have X=-1/HP0 and fail. Battle29 has neither a neutral group
nor a random-enemy upgrade, including with completion529 held.

`battleloop/triggerregions.asm:TriggerRegionsAndActivateEnemies` preserves
d0–a6 and selects the actual current battle's region subsection. Count0
returns before activation writes; Battle29 count4 scans0–3. Held bits in
NEWLY_TRIGGERED_BATTLE_REGIONS.w (`$FFB20C`) skip geometry. For each unchecked
region, IsRegionTriggered returning nonzero d1.b sets F90+index; the helper
then sets its checked bit even on a miss. It clears neither checked word nor
persistent flags. Fresh initialization cleared F90–105, **not** the checked
word. The later `ai/startaicontrol.asm:@NonSwarmAi` zero write is outside this
boundary and cannot establish its incoming value.

`data/battles/spritesets/spriteset29.asm` declares four12-byte type4 regions:

| Region / flag | Vertices1,2,3,4 |
| --- | --- |
|0 /90|`(0,0),(4,37),(31,13),(31,0)`|
|1 /91|`(0,0),(0,22),(14,22),(31,9)`|
|2 /92|`(0,0),(0,25),(28,13),(28,0)`|
|3 /93|`(0,0),(0,14),(31,36),(31,0)`|

IsRegionTriggered reads vertex words at2/4/6/8, tries triangle1/2/4, then
3/2/4 only if the first finds no eligible ally. AreAnyAlliesInTriangle scans
all30 slots0–29, requiring **positive signed** HP.w and nonnegative signed
X.b/Y.b, without joined/active membership tests. IsAllyInTriangle returns0
inside; the scan returns-1 on a match/0 on a miss, copied to d1.w. Living
placed nonmembers can trigger; dead, negative-HP or negative-coordinate slots
cannot. Enemy/neutral slots are not in this ally scan.

The actual edge consumer sign-extends bytes, chooses the larger absolute
delta (horizontal on equality), performs signed multiply/divide truncated
toward zero and adds the result to **endpoint B**. IsAllyInTriangle tests
edges1->2,3->1,3->2 in order: result1 immediately accepts; otherwise compare
against the opposite vertex, with bit0-set exceptions following the actual
branches. Negative projections use `move #1,ccr` then flag synthesis, returning2;
the source comment's3 is not substituted for those instructions. All selected
coordinates are nonnegative and triangles nondegenerate. Index guard
`cmp.b d1,d7`/`ble` is not strict index<count; normal iteration supplies0–3.
`$FF` has a separate false return; trigger15 is a sentinel, not polygon lookup.

Every declared formation ordinal was checked against all four regions:

| Ordinal | X/Y | Region0 | Region1 | Region2 | Region3 |
| --- | --- | --- | --- | --- | --- |
|0|`(14,30)`|outside|outside|outside|outside|
|1|`(15,30)`|outside|outside|outside|outside|
|2|`(13,31)`|outside|outside|outside|outside|
|3|`(16,31)`|outside|outside|outside|outside|
|4|`(13,32)`|outside|outside|outside|outside|
|5|`(16,32)`|outside|outside|outside|outside|
|6|`(14,32)`|outside|outside|outside|outside|
|7|`(15,32)`|outside|outside|outside|outside|
|8|`(14,31)`|outside|outside|outside|outside|
|9|`(15,31)`|outside|outside|outside|outside|
|10|`(14,33)`|outside|outside|outside|outside|
|11|`(15,33)`|outside|outside|outside|outside|

Let trunc mean signed truncation toward zero. Reproduce each rejection with
these B-anchored projections; earlier edges produce no immediate collinear
acceptance for these twelve points:

| Region | Triangle1/2/4 rejection | Triangle3/2/4 rejection |
| --- | --- | --- |
|0|edge4->2 X=`4+trunc(-27*(Y-37)/37)` < X; vertex1 on other side|edge3->2 Y=`37+trunc(-24*(X-4)/27)` < Y; vertex4 on other side|
|1|edge4->2 Y=`22+trunc(-13*X/31)` < Y; vertex1 on other side|edge3->2 horizontal Y22 < Y; vertex4 on other side|
|2|edge4->2 Y=`25+trunc(-25*X/28)` < Y; vertex1 on other side|edge3->2 Y=`25+trunc(-12*X/28)` < Y; vertex4 on other side|
|3|edge4->2 Y=`14+trunc(-14*X/31)` < Y; vertex1 on other side|edge3->2 Y=`14+trunc(22*X/31)` < Y; vertex4 on other side|

These are conditional formation results, not twelve named living members.
Unchanged fresh placement puts fewer/dead members in a subset and other ally
slots at X=-1. Thus F90–93 remain clear regardless of incoming checked bits;
F94–105 also remain clear. The first eligible enemy makes checked word
`incoming OR $000F`, not necessarily exactly15. Changed live coordinates,
flags, current battle or interruption can invalidate this conclusion. Held
checked bits suppress a rescan even after a position changes.

## All fifteen activation rows and extra helper invocation

**Confirmed:** GetTriggerRegions splits the stored byte into primary high
and secondary low nibbles. Both15 returns without activation write. A held
non15 primary wins and sets bit0; only missing/clear primary permits a held
non15 secondary to set bits0/1. Misses preserve the word. Primary preserves
previous bit1 and unrelated bits. Initialization leaves both active bits clear.

All rows below have item NOTHING, filler0, STARTING0, secondary order NONE
and secondary trigger15. Primary order and trigger are separate operands.
Words are hexadecimal; commandsets/orders alone prove no tactic or control.

| Row / ID | Type | X,Y | Commandset | Primary order | Triggers P/S | Fresh word | Held relevant flag |
| --- | --- | --- | --- | --- | --- | --- | --- |
|0 /128|Dark Knight|14,13|LEADER|MOVE_TO\|0|15/15|`2000`|no activation write|
|1 /129|Orc Lord|15,13|ATTACKER3|MOVE_TO\|1|15/15|`2000`|no activation write|
|2 /130|Master Monk|14,12|HEALER3|FOLLOW_ENEMY\|0|15/15|`3000`|no activation write|
|3 /131|Worm|11,14|ATTACKER1|FOLLOW_TARGET\|0|1/15|`2000`|F91 -> `2001`|
|4 /132|Wizard|15,14|ATTACKER1|FOLLOW_TARGET\|0|1/15|`2000`|F91 -> `2001`|
|5 /133|Wyvern|6,16|ATTACKER1|FOLLOW_TARGET\|0|2/15|`2000`|F92 -> `2001`|
|6 /134|Wyvern|25,17|ATTACKER1|NONE|3/15|`2000`|F93 -> `2001`|
|7 /135|Orc Lord|18,19|ATTACKER1|FOLLOW_TARGET\|0|3/15|`2000`|F93 -> `2001`|
|8 /136|Master Monk|19,19|HEALER3|FOLLOW_ENEMY\|7|3/15|`3000`|F93 -> `3001`|
|9 /137|Wizard|11,21|ATTACKER3|FOLLOW_ENEMY\|11|0/15|`2000`|F90 -> `2001`|
|10 /138|Master Monk|11,22|HEALER3|FOLLOW_ENEMY\|9|0/15|`3000`|F90 -> `3001`|
|11 /139|Worm|10,22|ATTACKER1|FOLLOW_TARGET\|0|0/15|`2000`|F90 -> `2001`|
|12 /140|Worm|19,27|ATTACKER1|NONE|15/15|`2000`|no activation write|
|13 /141|Worm|20,27|ATTACKER1|NONE|15/15|`2000`|no activation write|
|14 /142|Wyvern|6,29|ATTACKER1|NONE|15/15|`2000`|no activation write|

MOVE_TO0/1 reference the two declared AI points `(14,13)/(15,13)`, not
regions0/1. FOLLOW_ENEMY0/7/11/9 are order payloads, not alternate trigger
regions. Row6 has order NONE but trigger3. No unchanged row can take a
secondary-only activation path; changed trigger bytes can. With fresh flags
and unchanged formation every selected word remains its fresh value.
Missing143–159 retain triggerFF/activation0/HP0/X=-1 from the predecessor's
selective initializer, not a full record clear. No neutral filter protects a
changed slot; actual X/HP and the helper's signed ID gate do.

After32 DBF iterations d0.w=160/d7.w=`$FFFF`. There is no RTS/branch/data
before UpdateEnemyActivationBitfield, so it runs once more and its RTS
returns to BattleLoop. The verified
[address/deals join](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
applies: GetCombatantEntryAddress rejects **above**160 (`bhi`), admits160,
and maps `(160-96)*56+$FFE800` to CURRENT_GOLD `$FFF600`.

Pseudo-X `$FFF62E` is packed quantities84/85; pseudo-HP `$FFF60E` holds
ring20–23 quantities; triggers `$FFF636` are quantities100/101; activation
word `$FFF634` holds quantities96–99. With nonnegative pseudo-X, nonzero
pseudo-HP and held non15 trigger flag90–104, the setter can OR bit0 or
bits0/1 into Goddess Staff99's count nibble, preserving other bits. This
comes from actual stat getters/setter/addressing and packed-deals consumer,
not the BUG comment. It is no unconditional addition, saturating increment
or shop/persistence observation. Fresh Battle29 sets no region flags, so
even eligible pseudo-fields cause no deals write. Changed allies/flags/deals
can change this; earlier enemy flag writes are visible to the extra call.

## Region hook and actual spawn admission

**Confirmed:** complete `data/battles/cutscenes/regionactivatedcutscenes.asm`
contains four8-byte records, then a word terminator:

| Battle | Region | Played flag | Script |
| --- | --- | --- | --- |
|32|0|386|rbcs_battle32|
|32|1|386|rbcs_battle32|
|40|0|387|rbcs_battle40_1|
|40|1|388|rbcs_battle40_2|

`cutscenes/regionactivatedcutscenes.asm:ExecuteBattleRegionCutscene`
advances8, tests terminator then compares CURRENT_BATTLE.b. With29 throughout,
it skips all rows and returns without checking played/region flags, setting
played flags or dispatching MAPSCRIPT. Changed current32/40 is another
premise: matching rows check played flag then region, set played flag **before**
script dispatch, and return after that script. An interrupted script can leave
its flag held. Optional debug diagnostics still follow the no-match return.

`battleloop/populateenemyspawns.asm` scans128–159 and masks activation with
`$0F00`. Zero skips HP/region/position/reset/item calls. All fifteen STARTING
rows and seventeen absent slots have masked0; activation only changes bits0/1.
TARGETS_LIST_LENGTH.w becomes0, stale list bytes remain unconsumed, and
SpawnEnemyWithCamera is skipped. No fresh spawn animation, item or camera
effect follows from these declarations.

Changed mode `$0200` requires HP0 and held non15 primary/secondary trigger,
then max HP0; `$0100` requires HP0 without a region; `$0300` requires HP0
and the trigger without an additional max-HP test. Other nonzero modes append
nothing. The dead/trigger helper can set active bits before max-HP/reset
rejection. See the actual [spawn/reset consumer](./battle26-activation-and-first-turn-dispatch.md#spawn-list-and-changed-state-conditions).

ResetSpawningEnemyStats uses Battle29's count15 OR `$80` as bound143:
143–159 cannot reset from real rows. For a real row, starting X/Y are compared
with all30 allies and32 enemies, without HP/neutral/self exclusion. A dead
actor left at its own start can block itself. Invalid/occupied reset writes
max/current HP0, X=-1 and carry, with no append. Success reinitializes actual
row stats/position/items, then restores saved full activation and returns
clear carry via the setter. No Battle29 upgrade; all row items NOTHING skip
AddItem, leaving copied definition equipment. No spawn-specific item award.

Successful slots append once in ascending ID order, word length/byte IDs.
Later comparisons use returned full d1 rather than remasking; hidden/respawn
fall-through does not duplicate appends. A nonempty list invokes entity spawn,
camera/cursor waits, twelve orientation/Sleep3 iterations, SFX_SPAWN and
text397/close, preserving outer d7/a0. Services must return before generation;
their appearance/timing/completion is unobserved here.

## First-round records, order and dispatch

**Confirmed:** `battleloop/turnorderfunctions.asm:GenerateBattleTurnOrder`
(`$25544`, helper `$255A4`) clears64 two-byte records at `$FFF71A` to `$FFFF`.
It offers allies0–29 then enemies128–157: BATTLE_ENEMY_ENTITIES_COUNTER29,
not COMBATANT_ENEMIES_COUNTER31. Changed158/159 still are not offered.
The helper tests nonnegative signed X.b then nonzero HP.w, with no Y,
membership, neutral, activation, region, commandset or order test. Fresh
143–157 fail X; all fifteen128–142 qualify despite clear activation bits.

Actual selected definitions, clear initial status and equipment/stat refresh
give this complete current-stat join (difficulty affects ATT, not these AGIs):
`code/common/stats/updatecombatantstats.asm:InitializeCurrentStats` copies
base AGI to current AGI before status/equipment effects. The four equipped
definitions in `data/stats/items/itemdefs.asm` (28/77/79/90: Brass Knuckles,
Heavy Lance, Chrome Lance, Flail) have only INCREASE_ATT and two NONE effects;
they do not change this AGI join. Source status NONE supplies no AGI modifier.

| ID | Type | Current HP | Current AGI | A>>3 | Records |
| --- | --- | --- | --- | --- | --- |
|128|Dark Knight|50|40|5|1|
|129|Orc Lord|46|37|4|1|
|130|Master Monk|47|31|3|1|
|131|Worm|45|34|4|1|
|132|Wizard|37|44|5|1|
|133|Wyvern|46|36|4|1|
|134|Wyvern|46|36|4|1|
|135|Orc Lord|46|37|4|1|
|136|Master Monk|47|31|3|1|
|137|Wizard|37|44|5|1|
|138|Master Monk|47|31|3|1|
|139|Worm|45|34|4|1|
|140|Worm|45|34|4|1|
|141|Worm|45|34|4|1|
|142|Wyvern|46|36|4|1|

Reuse the verified [RNG/score/signed-sort derivation](./battle26-activation-and-first-turn-dispatch.md#turn-buffer-randomized-agility-and-first-dispatch).
Current AGI is an unsigned byte; A=rawAGI&127. First score is
`A+R(A>>3)-R(A>>3)+R(3)-1`. rawAGI>=128 adds an immediate second record with
B=floor(A*5/6), score `B+R(B>>3)-R(B>>3)`. Store low score byte after word
arithmetic. Direct RNG advances `(seed*13+7)&65535` on every call, including
range0; for these ranges R(n)=floor(seed*n/65536). Singles consume3 sequential
draws/doubles5/rejects0. Draws are coupled; no seed or first actor is selected.

All fifteen fresh enemies contribute one positive score below128 for all
permitted draws:45 enemy draws after actual ally draws. With N eligible
initialized allies and T at rawAGI>=128, records=`15+N+T`, at most39 for the
accepted twelve-position formation. Fewer/dead members reduce N. Changed
membership/placement/AGI cannot inherit that bound; all30 ally slots are offered.

Sorting uses62 passes of63 adjacent comparisons across all64 records.
`cmp.b`/signed `ble` swaps only if right score is strictly greater; ties
preserve append order, including ally-before-enemy and each immediate second
entry. Padding255 is -1; wrapped135 is -121. Existing boundary fixture's
rawAGI128 scores0/255 and rawAGI127 score135 are seam examples, not Battle29
state. There is no append-capacity guard; changed many/doubled candidates
can exceed64, and62 passes do not prove complete sorting for arbitrary64
entries. Fresh maximum39 fits and positive enemy scores establish a
non-sentinel front within these passes, conditional on earlier service returns.

CURRENT_BATTLE_TURN.b (`$FFF79A`, immediately after128 buffer bytes) is then
cleared. The prefix reads offset0 and reaches the first call; actual actor
depends on party/current AGIs and sequential RNG. Empty admitted sets leave
`$FF` first and repeat preparation rather than declaring an outcome. Changed
scores below-1 may also sort behind padding. F88 supplied offset can be odd
or out of buffer, without validation before its byte read.

## Actual representations and consumer limits

**Confirmed about the accepted repository:** research data, prepared Content
and executing Application/Godot are separate. The
[capability owner](../../remake/docs/capability-status.md) controls runtime claims.

| Category | Existing actual consumer / support | Missing binding, Unsupported or unassessed behavior |
| --- | --- | --- |
|Research|Spriteset summary fixture/schema retains `rowContentTracked=false`; full source supplies exact29 rows. Shared loop/control/cutscene fixtures and RNG/region/turn boundary fixtures retain named consumer/seam evidence.|Neither summary equality nor another battle's observed fixture establishes natural29 state or a playable package.|
|Content|[EncounterDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs) expresses placements/orders/regions/points; [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) parses them.|Actual Pin admits1/2/3, rejects29.29 encounter/start/party/enemy/policy/material bindings are missing; declarations alone do not initialize actors.|
|Round rules|[BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) consumes regions/activation, admits RegionProgram.None and spawn0; [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) calls it before [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), which retains RNG draws,64 slots and signed62x63 sort.|Activation uses cross-product inclusion and a fresh local tested mask, not the source's truncated projection/supplied checked-word semantics. It iterates admitted actors, with no slot160/deals alias. Selected fresh all-miss deductions do not establish general geometry/changed-state parity. Nonzero spawn modes/region programs explicitly reject; no29 execution claim.|
|Application|[BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) publishes region/round/RNG observations and advances its own session queue; [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) has fresh hooks and membership initialization.|Reusable mechanisms exist.29 binding and full changed-state behavior remain unassessed; F88 rejects under NewBattle and save/suspend is Unsupported. Session advancement is not original supplied-byte dispatch evidence.|
|Godot/Map0|[ExplorationViewRunner.Validate](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs) and [BattleMapViewport.BuildTerrain](../../remake/game/src/Battles/BattleMapViewport.cs) enforce actual plane profiles.|Map0 ParallaxBX=16 / AutoscrollBY=1 remains **Unsupported** in both: `field-view-profile` / `battlefield-map-plane-motion-unsupported`. Resource binding alone cannot admit it. Materials/layout/animation and exact presentation remain separate binding/unassessed work; existing mosaic/white services do not prove this full sequence.|

Production consumes prepared Content, not research JSON/disassembly on demand.
No engine, adapter, Godot or emulator was launched for this source survey.

## Changed states, reproduction and stopping boundary

| Changed premise | Required distinction |
| --- | --- |
|F479/opening incomplete|Flag-before-empty-start is not completion evidence; fresh endpoint requires returned wrappers.|
|F88/current map/battle/turn differs|Initial supplied dispatch bypasses preparation; sentinel re-enters with actual current selectors/state.|
|Party/member/HP/position differs|Formation ordinal is not identity; geometry uses positive signed HP/Y, activation/turn use nonzero HP and no Y.|
|Checked word/positions changed|Held bits suppress geometry, including earlier misses; clearing F90–105 did not clear checked word.|
|Persistent flags/active bits changed|Primary precedence/sentinel15 and preserved bit1 apply; no prefix reset.|
|Order/trigger changed|MOVE_TO/FOLLOW payloads are distinct from trigger nibbles; row6 NONE still has region3.|
|Absent/neutral/high-bit-HP slot changed|Activation/spawn/turn bounds and predicates differ; no universal neutral or membership filter.|
|Deals/pseudo-fields changed|Extra160 can write count bits only through the actual X/HP/trigger gates.|
|Current battle/debug changed|Matching table scripts or diagnostic services may intervene before spawning/generation.|
|Mode/max HP/occupancy changed|Active bits can precede reset rejection; rejected reset writes HP0/X=-1; success resets actual row before append.|
|Current AGI/RNG/party changed|Extra records, wrapping, finite-buffer writes and first actor need actual inputs.|
|Interrupted prefix|Reached checked/flag/activation/deals/list/stats/seed/buffer writes persist without rollback; preparation is not atomic.|

Load ignored `local/private-inputs.ps1` in each launching process. Reproduce
bounded direct review with `uv run --locked python -X utf8
local/issue1060/review.py <configured-source-disasm-root>`. It joins complete29
rows/definitions and all12x4 integer deductions to actual activation, aliases,
region wrapper, spawn/reset, turn/RNG, selected fixtures/schemas and current
Content/Application/Godot predicates. Review full prose/changed states,
UTF-8/LF, links/anchors, exact two-path/navigation-only diff, private boundary
and `git diff --check`. On clean committed HEAD use `uv run sf2 verify plan
--base origin/main --head HEAD`; retain exact-head scoped CI in the handoff.
Documentation scope skips blanket normal/full Python/.NET/Godot/H3/H4 and
tests of review programs. No new maintained verifier or acquisition.

Preserve [#1058/PR1059](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1059)
original TypeError/P2 correction, source/independent0.218s/correction0.023s
receipts and both CI results; retain #1056 and predecessor failures/Unknowns
and the unchanged pair. Twelve old remake comparison failures remain completed
outside this slice. This slice's actual results/diagnostics belong in its
Issue/PR handoff. Freeze a clean current-main-based pushed Draft PR, Project
Review / Verify, exact identities/checks and successful main-gate callback;
no merge, cleanup, archive or next Issue.

**Unknown:** natural reach, actual party/class/items/HP/flags/checked word/
deals/current/saved state/seed, service returns, timing/rendering/audio,
usable control, persistence and actual first actor. No AI/action body, resolved
turn, outcome or post-battle scene is established; these are not an automatic
runtime work queue.
