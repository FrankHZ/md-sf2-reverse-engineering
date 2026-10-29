# Battle31 activation and first-turn dispatch

Continue with [Battle31 outcomes and Battle32 admission](./battle31-outcomes-and-battle32-admission.md).

Evidence date: 2026-10-20. **Confirmed (static):** all twelve declared
formation positions miss all four Battle31 regions under the actual integer
consumer. With unchanged fresh construction, flags90–105 remain clear,
activation words stay at their initialized values, and the spawn list is
empty. All fifteen initialized enemies nevertheless enter turn generation;
none has a second-turn AGI bit. Stop at the first ExecuteIndividualTurn
call, before its body. **Unknown:** natural reach, actual incoming party,
state/RNG, first actor, completed services, presentation, control and persistence.

## Agreement, sources and method

Standard rigor for [Issue #1078](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1078)
under [epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
deliver the rules needed for later Battle31 content integration, separating
geometry, checked bits, persistent flags, activation, spawning and turn
eligibility. Accepted dependency is [initialization/opening](./battle31-initialization-and-opening.md),
#1076/PR1077, base `a69f089fdfb75ba77dd67ebf4b30cdd1f92e0732`.
Owned writes are this document and one predecessor navigation link. No
shared-path or stacked dependency; UI692 is independent. No engine,
schema/fixture/tool/extraction, runtime acquisition, seed search,
AI/action/combat/outcome, later story, design-guide or translation work.

Reuse [Battle30](./battle30-activation-and-first-turn-dispatch.md),
[Battle29](./battle29-activation-and-first-turn-dispatch.md),
[Battle27](./battle27-activation-and-first-turn-dispatch.md),
[Battle26 shared derivations](./battle26-activation-and-first-turn-dispatch.md),
[battle loop](../../battle/battle-loop.md), [spritesets](../../battle/battle-spriteset-data.md),
[RNG/stat/turn evidence](../../battle/runtime-rng-and-battle-math.md),
[lifecycle](../../../design/contracts/battle/battle-control-lifecycle.md) and
[routing](../../../design/contracts/battle/battle-cutscene-routing.md). Their fixtures
prove their named seams, not a Battle31 runtime observation.

Original paths below are relative to `disasm/` at clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing evidence identifies USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no ROM is newly run or rebuilt. `sf2const.asm`/`sf2enums.asm` own constants
and RAM; Fnnn means flag nnn, ordinals are zero-based, intervals half-open.
`battleloop/` and `cutscenes/` abbreviate `code/gameflow/battle/` children.

Minimum evidence is selected source, existing fixture/schema/consumer joins,
48 formation/region results and focused changed states. Reuse the unchanged
18,285,979-byte init/script pair and accepted0.083s provenance in
`local/issue1005/pair.txt`. Linear bounded reads/joins, including this small
finite geometry calculation, are budgeted below one minute/512MiB; new small
ignored receipts use `local/issue1078/`. No corpus acquisition, extraction,
binary decoding or model/API processing. Replan contradictory evidence or
material supporting machinery before expansion.

## Prefix order and supplied-state boundary

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) reaches @Start after fresh LoadBattle and start-wrapper
return. F481 is set before the empty start script; it proves no completed
opening. The ordered prefix is:

1. ActivateEnemies, including its extra fall-through invocation160.
2. j_ExecuteBattleRegionCutscene.
3. If DEBUG_MODE_TOGGLE.b is nonzero, PrintAllActivatedDefCons.
4. j_PopulateTargetsListWithSpawningEnemies; read TARGETS_LIST_LENGTH.w.
   Zero skips spawning. Otherwise decrement the DBF counter, read byte IDs
   in TARGETS_LIST order with d0.w cleared each time, call SpawnEnemyWithCamera.
5. GenerateBattleTurnOrder.
6. @IndividualTurns_Loop clears d0.w, reads CURRENT_BATTLE_TURN.b as a
   **byte offset** into BATTLE_TURN_ORDER, replaces d0.b with that byte.
   FF returns to @Start; another byte reaches ExecuteIndividualTurn. Stop here.

No prefix transaction or rollback exists. Interrupted services retain only
reached checked/flag/activation/deals/list/stat/seed/buffer writes. Debug
output checks all sixteen flags90–105; a held flag writes its region index
to DIALOGUE_NUMBER.l and requests text463, then the caller closes text.
Fresh clear flags request no such region text. No timing or display was observed.

F88's separate supplied-state path restores seconds, clears88/AI memory/view
target, calls LoadBattle and branches straight to @IndividualTurns_Loop;
the following bra @Start is unreachable. It bypasses this preparation and
does not clear the supplied turn offset. Odd offsets can read score bytes;
128–255 read beyond the128-byte buffer. Supplied FF can re-enter @Start
using actual current selectors/state. This is no arbitrary-state safety,
natural suspend or cross-process persistence claim.

## Four regions and all48 formation results

**Confirmed:** `battleloop/activateenemies.asm` (`$2550C..$25544`) offers
128–159. UpdateEnemyActivationBitfield rejects negative signed X.b, zero
HP.w, then nonnegative d0.b. It has no Y, membership, neutral, order or
commandset test. All fifteen initialized enemies pass, including inactive
ones; absent143–159 fail X. A high-bit HP word is nonzero for this guard.

`battleloop/triggerregions.asm:TriggerRegionsAndActivateEnemies`
(`$1AC9FC..$1ACF30` file) saves the combatant, selects live CURRENT_BATTLE's
region subsection and returns before scanning/activation for count0.
Battle31 supplies4. Each unset NEWLY_TRIGGERED_BATTLE_REGIONS.w
(`$FFB20C`) bit admits geometry; a hit sets F90+index. Both hit and miss
set the checked bit. Held bits suppress scans; this prefix clears neither
the checked word nor persistent flags. Fresh initialization cleared F90–105.
The later AI checked-word clear is outside this stopping boundary.

All four12-byte records have type4, unknown0 and two trailing zero bytes:

| Region / flag | Ordered vertices1,2,3,4 |
| --- | --- |
|0 /90|`(29,0),(29,31),(47,31),(47,0)`|
|1 /91|`(0,7),(0,31),(47,31),(47,7)`|
|2 /92|`(0,14),(0,31),(47,31),(47,14)`|
|3 /93|`(0,11),(0,31),(47,31),(47,11)`|

IsRegionTriggered tries triangle1/2/4, then3/2/4 if needed.
AreAnyAlliesInTriangle offers slots0–29 with **positive signed HP.w** and
nonnegative signed X.b/Y.b, independently of membership. IsAllyInTriangle
returns0 inside; the wrapper translates a hit to d6/d1=-1. Living placed
nonmembers can trigger; negative-HP, dead or offscreen allies cannot.

Reuse the [corrected integer relationship](./battle30-activation-and-first-turn-dispatch.md#geometry-checked-bits-and-persistent-flags):
signed coordinate bytes, larger absolute delta (horizontal on equality),
signed multiply/divide with truncation toward zero, projection added to
endpoint B. Triangle edges are1->2,3->1,3->2. A bounded collinear return1
immediately accepts; otherwise actual side/bit0 branches compare the point
with the opposite vertex. Negative projection synthesizes2, despite the
comment's3. Do not substitute visual containment or cross products.
Index validation is `cmp.b d1,d7`/`ble`, not strict index<count; FF returns
false separately. The normal loop offers only0–3; trigger15 is a sentinel.

| Formation ordinal | X/Y | Region0 | Region1 | Region2 | Region3 |
| --- | --- | --- | --- | --- | --- |
|0|`(21,4)`|outside|outside|outside|outside|
|1|`(21,3)`|outside|outside|outside|outside|
|2|`(22,3)`|outside|outside|outside|outside|
|3|`(20,3)`|outside|outside|outside|outside|
|4|`(22,4)`|outside|outside|outside|outside|
|5|`(23,3)`|outside|outside|outside|outside|
|6|`(20,4)`|outside|outside|outside|outside|
|7|`(19,3)`|outside|outside|outside|outside|
|8|`(21,2)`|outside|outside|outside|outside|
|9|`(23,4)`|outside|outside|outside|outside|
|10|`(20,2)`|outside|outside|outside|outside|
|11|`(22,2)`|outside|outside|outside|outside|

The direct calculation checks both triangles for each row. Let trunc mean
signed truncation toward zero; selected coordinates are nonnegative and
arithmetic fits. Actual rejecting edges and projections are:

| Region | Triangle1/2/4 first rejection | Triangle3/2/4 first rejection |
| --- | --- | --- |
|0|1->2: vertical Xproj29, above every selected X|4->2: vertical `29+trunc(-18*(Y-31)/31)`, above selected X|
|1|4->1: horizontal Yproj7, above selected Y|4->2: horizontal `31+trunc(-24*X/47)`, above selected Y|
|2|4->1: horizontal Yproj14, above selected Y|4->2: horizontal `31+trunc(-17*X/47)`, above selected Y|
|3|4->1: horizontal Yproj11, above selected Y|4->2: horizontal `31+trunc(-20*X/47)`, above selected Y|

Each point differs from the corresponding opposite-vertex side; earlier
edges do not immediately accept. At `(21,4)`, region0's second projection
is44, and region1's is21. Thus the conclusion follows from the consumer,
not rectangle appearance alone. Formation ordinals are not fixed identities:
fewer/dead members occupy a subset, others have X=-1. Under unchanged fresh
placement F90–93 stay clear regardless of incoming checked bits; F94–105
also stay clear. The first passing enemy yields `incoming OR $000F`, not
necessarily exactly15.

Changed living positions are distinct: `(29,4)` hits only0, `(21,7)` only1,
`(21,11)` hits1/3, `(21,14)` hits1/2/3, `(30,20)` hits all four. Neighbour
checks `(28,4)`, `(21,6)`, `(21,10)`, `(21,13)` retain the relevant threshold
distinctions. A checked miss still prevents a rescan after movement; held
flags may activate enemies even when geometry is skipped or now misses.

## All enemy activation rows and the extra invocation

**Confirmed:** `data/battles/spritesets/spriteset31.asm` supplies the fifteen
rows below, all NOTHING/filler0/STARTING0. GetTriggerRegions splits primary
high/secondary low nibbles. A held non15 primary wins and ORs bit0; only a
missing/clear primary allows a held non15 secondary to OR bits0/1. Misses
and both15 preserve the word. Existing bit1 and unrelated bits survive even
when primary wins. Activation does not choose or execute an AI action.

Words are hexadecimal; changed-flag effects assume the initialized word:

| Row / ID | Type | X,Y | Commandset | Primary order / region | Secondary order / region | Fresh word | Held relevant flag |
| --- | --- | --- | --- | --- | --- | --- | --- |
|0 /128|Devil Soldier|23,27|LEADER|MOVE_TO\|0 /2|NONE /15|`2000`|F92 -> `2001`|
|1 /129|Dark Bishop|21,19|ATTACKER3|FOLLOW_ENEMY\|0 /1|NONE /15|`3000`|F91 -> `3001`|
|2 /130|Necromancer|15,20|HEALER3|FOLLOW_ENEMY\|0 /1|NONE /15|`2000`|F91 -> `2001`|
|3 /131|Harpy Queen|23,26|SENTRY|NONE /15|NONE /15|`2000`|no activation write|
|4 /132|Harpy Queen|32,23|ATTACKER1|FOLLOW_TARGET\|0 /2|NONE /15|`2000`|F92 -> `2001`|
|5 /133|Dark Knight|25,20|SENTRY|NONE /15|NONE /15|`2000`|no activation write|
|6 /134|Orc Lord|22,18|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|`2000`|F91 -> `2001`|
|7 /135|Master Monk|19,18|HEALER4|FOLLOW_ENEMY\|6 /1|FOLLOW_ENEMY\|0 /15|`3000`|F91 -> `3001`; secondary15 cannot select|
|8 /136|Harpy Queen|10,16|ATTACKER1|FOLLOW_TARGET\|0 /1|NONE /15|`2000`|F91 -> `2001`|
|9 /137|Dark Knight|17,11|ATTACKER1|NONE /15|NONE /15|`2000`|no activation write|
|10 /138|Orc Lord|29,11|ATTACKER1|NONE /15|NONE /15|`2000`|no activation write|
|11 /139|Master Monk|31,10|HEALER3|FOLLOW_ENEMY\|10 /15|NONE /15|`3000`|no activation write|
|12 /140|Orc Lord|37,4|HEALER3|FOLLOW_ENEMY\|13 /0|FOLLOW_ENEMY\|13 /3|`2000`|F90 -> `2001`; only F93 -> `2003`|
|13 /141|Dark Knight|39,2|ATTACKER2|NONE /0|NONE /3|`2000`|F90 -> `2001`; only F93 -> `2003`|
|14 /142|Harpy Queen|23,15|SENTRY|NONE /15|NONE /15|`2000`|no activation write|

With both F90/F93 held, rows12/13 take primary; a preexisting bit1 still
survives. Row12's identical order payload13 does not collapse its two
triggers. Row13's NONE orders do not suppress activation. Row7's secondary
FOLLOW_ENEMY0 is a real stored order with sentinel15, not another region1.
MOVE_TO0 references point0 `(23,27)`; FOLLOW_ENEMY operands are local
ordinals (6=Orc134,10=Orc138,13=Knight141). FOLLOW_TARGET0 is not point0.
Priority/LEADER/SENTRY/HEALER labels prove no eventual tactic or turn owner.

Absent143–159 have X=-1, max/current HP0, activation0 and trigger2F from
the initializer's **unoffset first enemy row**. Other Y/type/item/order
fields are retained, not newly initialized. They fail activation; changing
only X/HP can expose primary2/F92. They are not Battle30's hidden143.

ActivateEnemies has no RTS before its helper label. After32 iterations
d0.w=160/d7.w=FFFF falls through once; the helper's RTS returns to BattleLoop.
Reuse the checked [RAM/deals derivation](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias):
GetCombatantEntryAddress's `bhi` admits160, mapping to CURRENT_GOLD `$FFF600`.
Pseudo-X `$FFF62E` packs item counts84/85; pseudo-HP `$FFF60E` packs20–23;
trigger `$FFF636` packs100/101; activation `$FFF634` packs96–99. Only if
pseudo-X is nonnegative, pseudo-HP nonzero and a non15 trigger's flag90–104
is held can the activation write OR bit0 or bits0/1 into Goddess Staff99's
count nibble. Fresh clear flags imply no write even if pseudo-X/HP pass.
Changed state can differ. This is no unconditional shop addition,
saturation, visible inventory or persistence result.

## Region selector and spawn list

**Confirmed:** the complete `data/battles/cutscenes/regionactivatedcutscenes.asm`
table has four8-byte records, then a word terminator:

| Battle | Region | Played flag | Script |
| --- | --- | --- | --- |
|32|0|386|rbcs_battle32|
|32|1|386|rbcs_battle32|
|40|0|387|rbcs_battle40_1|
|40|1|388|rbcs_battle40_2|

`cutscenes/regionactivatedcutscenes.asm:ExecuteBattleRegionCutscene`
advances8, checks terminator, then compares live CURRENT_BATTLE.b. With31
throughout, it skips all rows before played/region checks, flag writes or
MAPSCRIPT. Changed selectors32/40 can match: played check precedes region
check, flag set precedes script, and the wrapper returns after one script.
An interrupted script may leave its played flag held.

`battleloop/populateenemyspawns.asm` (`$1ACF30..$1AD090`) scans128–159,
masking activation with0F00. Every declared fresh2000/3000 and absent0 masks
to0, skipping HP, trigger, reset and item work. Active bits0/1 cannot change
that mode. Thus **all32 slots** skip; TARGETS_LIST_LENGTH.w becomes0,
stale byte entries are not consumed, and no SpawnEnemyWithCamera call occurs.
Held flags alone cannot create a spawn from these modes.

Supplied modes retain the accepted shared branches:0100 requires HP0;
0200 requires HP0 plus held non15 trigger and then maxHP0;0300 requires the
dead/trigger helper, without that extra maxHP test. Other modes append
nothing. The helper has no X/Y guard, primary wins, and its active-bit write
can precede a later max-HP/reset rejection. ResetSpawningEnemyStats uses
live selected enemy count OR80 as an exclusive bound: Battle31's15 gives143,
so absent143–159 cannot reset from31 even if their mode/HP/trigger changes.
For a declared row it checks starting X/Y against all30 allies/32 enemies,
without HP/neutral/self exclusion; dead or self occupants can block it.
Rejection clears max/current HP, sets X=-1 and carry. Success initializes
the selected source row and restores the saved full activation word.

Each successful reset appends once in ascending ID order; subsequent mode
comparisons use returned full d1. If a changed declared row reaches it,
SpawnEnemyWithCamera/SpawnEnemy call the actual SpawnEnemyEntity using
current position/type/movetype, lookup allocation, sprite/DMA, camera,
spin/sound/text397 services. These must return before generation. No fresh
spawn effect, changed-state capacity safety or natural reveal is established.
The unchanged31 empty-list deduction needs none of that service execution.

## Turn eligibility, current AGI and first call

**Confirmed:** `battleloop/turnorderfunctions.asm` (`$25544..$25610`)
clears64 two-byte records to FFFF, then offers allies0–29 and enemies128–157
in ascending ID order. The helper rejects negative X.b or zero HP.w only:
no Y, membership, activation, neutral, commandset or LEADER test. Thus all
fifteen declared enemies enter even with inactive words; absent143–157 fail
X and158/159 are outside this turn scan.

The predecessor checks all seven `data/stats/enemies/enemydefs.asm`
definitions. `InitializeEnemyStats` copies them, restores HP/MP and updates
stats. `code/common/stats/updatecombatantstats.asm` first copies base AGI,
applies status then equipped effects. All seven start with NONE status;
their five selected weapon definitions have only INCREASE_ATT (then NONE
effects), while Harpy has no equipped item. Zero BOOST/SLOW/STUN and the
actual AGI clamp/getter consumers leave these raw AGI bytes unchanged;
difficulty adjusts ATT only. There is no Battle31 random-battle upgrade.

| Type / eligible IDs | Current HP | Base = current raw AGI | A = raw&127 | A>>3 | Records each |
| --- | --- | --- | --- | --- | --- |
|Devil Soldier /128|60|43|43|5|1|
|Dark Bishop /129|39|27|27|3|1|
|Necromancer /130|47|51|51|6|1|
|Harpy Queen /131,132,136,142|53|43|43|5|1|
|Dark Knight /133,137,141|50|40|40|5|1|
|Orc Lord /134,138,140|46|37|37|4|1|
|Master Monk /135,139|47|31|31|3|1|

Reuse the [RNG/score derivation](./battle26-activation-and-first-turn-dispatch.md#turn-buffer-randomized-agility-and-first-dispatch).
First score is `A+R(A>>3)-R(A>>3)+R(3)-1`. Current **raw** AGI>=128,
not priority or commandset, adds an immediate second record with
`B=floor(A*5/6)` and score `B+R(B>>3)-R(B>>3)`. Each direct RNG call updates
`seed=(seed*13+7)&65535`, even range0; for these ranges
`R(n)=floor(seed*n/65536)`. Singles consume3 sequential draws, doubles5,
rejects0. Direct generation does not use the debug RNG override; earlier
reached services determine its incoming seed.

All selected31 enemy raw bytes are below128: **15 records/45 draws**, after
actual ally draws. Conservative score ranges by table order are38..48,
24..30,45..57,38..48,35..45,33..41,28..34; coupled draws do not guarantee
independently attainable extremes. Every enemy score is positive below128.
With N eligible fresh placed allies and T of those having current raw
AGI>=128, total records are `15+N+T`, at most39 for twelve formation slots.
Dead/fewer members reduce N. Changed membership/placements/stats do not
inherit that bound: all30 ally slots are offered. Devil Soldier's43/LEADER
does not import Zalbard's180/two-record result or establish the first actor.

Sort is62 passes of63 adjacent comparisons across all64 slots, signed
`cmp.b`/`ble`, swapping only when the right score is strictly greater.
Equal scores retain append order, including allies before enemies.
Padding255 is-1; the existing rawAGI128 scores0/255 and rawAGI127 score135
fixture retains its seam. Source has no append-capacity guard; changed many
doubled actors can overflow64, and62 passes do not prove complete sorting
for arbitrary64 inputs. Fresh maximum39 fits; positive enemy entries ensure
a non-sentinel front, conditional on the earlier prefix returning.

Generation clears CURRENT_BATTLE_TURN.b (`$FFF79A`, just after the buffer).
Fresh first read therefore uses offset0 and reaches the call. Actor identity
depends on actual party/current stats and sequential RNG. An empty eligible
set would leave FF and repeat @Start; supplied scores below-1 may sort behind
padding. No selected actor's AI, movement, action or turn result is evaluated.

## Actual preparation and execution consumers

**Confirmed about the accepted repository:** production consumes prepared
Content. Research facts, expressible shapes, actual admission and running
Application/Godot scope are separate; the
[capability owner](../../../../remake/docs/capability-status.md) controls runtime claims.

| Surface / actual consumer | Expressible or consumed | Refusal, missing binding or unassessed fidelity |
| --- | --- | --- |
|Research/preparer, [OriginalPrograms](../../../../src/sf2tool/remake_exploration_programs.py)|Spriteset fixture/schema retains summary/address facts and rowContentTracked=false; pinned source supplies full31 rows. Existing scene corpus/lowering has shapes.|No new extraction/package. Reused Battle01 geometry/RNG/secondary fixtures do not establish natural31 state. Opening's corrected lowered15 nested ac_animSpeedX2 can refuse during enabled entity service before top-level cameraSpeed18; preserve the [installation/service boundary](./battle31-initialization-and-opening.md#actual-preparation-and-consumer-gaps). Lowering is no completed prerequisite scene.|
|[BattleEncounterDefinition](../../../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs), [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Coordinates, commandsets/orders/items/spawn expressions, four regions and point have declarations; STARTING placement validation exists.|Pin admits1/2/3, explicitly rejects31. Missing31 encounter/party/enemy/stat/policy/material binding; the twelve ordinals are no fixed party.|
|[PrivateBattleScenarioReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Separates numerical AGI&127 and ExtraRoundAction; STARTING/NOTHING rows and both trigger/order fields have shapes.|region-program-table admits1/2/3. source-commandset lacks HEALER3/HEALER4/SENTRY; enemy-derived-state requires REGULAR spell power, while all seven31 types are ENHANCED. source-mover also lacks Necromancer's MAGE. These are inspected guards beyond the first Pin rejection, not reached31 runtime errors.|
|[BattleActivationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [BattleTurnFlow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs)|Activation precedes generation; primary/secondary OR precedence exists; RegionProgram.None/SpawnMode0 fit31's selected fresh no-hook/no-spawn shape.|Nonzero spawn modes reject spawn-mode; no hidden reset/entity-spawn consumer. Cross-product geometry/local tested=0 and actor records differ from original truncated projections/supplied checked word/raw slots/160 deals alias. Fresh all-miss agreement establishes no general changed-state parity.|
|[BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [TurnOrderRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs)|Numerical AGI<=127 plus separate extra action, sequential draws, signed62x63 sort and64 slots exist. All selected fresh enemy AGI values are expressible without extra action.|Enemy equipment refresh requires item words0/127 and rejects equipped31 weapons as enemy-equipment-refresh. General selected stats/status/equipment need binding; queue validation/overflow rejection is not original unsafe buffer/byte-offset evidence.|
|[BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [BattleAdvancer](../../../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs)|Fresh hooks/membership and regions-tested/region-program-none/spawn-modes-admitted/round-rng observations have consumers.|NewBattle F88 rejects suspended-battle-entry; save/suspend Unsupported. Full31 world/encounter and opening are unbound. No original first actor or natural control follows.|
|[ExplorationViewRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../../../remake/game/src/Battles/BattleMapViewport.cs), [ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs)|Predecessor checks Map2's layer0/parallax256/zero autoscroll and board profile guards.|Passing metadata guards is separate from missing Map2/31 resources, changed-area fallback and scene/entity/text/audio/camera/effect fidelity. No adapter/Godot/emulator was launched.|

These findings support later binding decisions; no Battle31 runtime-readiness
claim or blanket refusal of already admitted engine mechanisms follows.

## Changed states, verification and stopping condition

| Changed premise | Boundary to preserve |
| --- | --- |
|Opening/F481/interruption|Latch is not completion; all prior services must return before this fresh prefix.|
|F88/offset/buffer/current selectors|Supplied dispatch bypasses preparation; FF can re-enter with actual live state.|
|Party/HP/X/Y|Formation ordinal is not identity; geometry needs positive signed HP and X/Y, activation/turn only nonzero HP and X.|
|Checked bits/flags90–105|Checked misses suppress rescan; persistent held flags and checked word have distinct lifetimes.|
|Rows7/12/13/active bit1|Order and trigger are separate; primary wins, secondary15 skips, previous secondary bit persists.|
|Absent143–159|Partial zero/2F is not hidden initialization; changing X/HP does not create a declared reset row.|
|Spawn modes/occupancy/selectors|Flags alone cannot change mode0; dead/self occupants and live row/count selection affect conditional reset.|
|Pseudo160/deals|Only actual pseudo-X/HP/trigger premises permit count-bit writes.|
|AGI/status/equipment/RNG|Current raw byte controls extra record; seed order, score wrapping, capacity and sorting need actual inputs.|
|Interrupted prefix|Reached mutations persist without rollback or implied usable control.|

Load ignored `local/private-inputs.ps1` in each tool-launch process. Direct
bounded reproduction (configured source selected read-only):

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1078/review.py <configured-source-disasm-root>
git diff --check
```

Review complete27 placements/four polygons/point/seven definitions, all48
integer results and selected changed thresholds; actual activation/alias,
full region wrapper/table, spawn/reset/stat/RNG/turn and Content/Domain/
Application/Godot joins; all prose tables, UTF-8/LF, links/anchors, private
boundary and exact two-path/navigation-only diff. On clean committed HEAD
run `uv run sf2 verify plan --base origin/main --head HEAD`; receipts,
diagnostics and actual exact-head scoped CI belong in the Issue/PR handoff.
Documentation scope skips blanket normal/full Python/.NET/Godot/H3/H4,
review-program tests and old comparisons.

Preserve #1076's original author0.236s/independent0.240s/CI39483112493, which
missed the nested-action refusal, and its focused correction independent
0.123s/CI39484532491. Retain #1074 LEADER, #1070 N==V, #1068 HP and #1066
integer-projection corrections and their failure/Unknown chain. The unchanged
pair and twelve completed old private comparison failures remain outside this
repair/rerun queue. No acceptance boundary is weakened.

Stop at a clean current-main-based committed/pushed Draft PR, Project Review /
Verify, exact head/tree/base/checks/CI and successful main-gate callback with
writer/process stopped. No merge, cleanup, archive or next Issue.

**Unknown:** natural admission, actual party/classes/items/HP/coordinates,
checked/flag/deals state, live/saved selectors, seed and first actor, service
returns/interleavings, entity allocation, precise rendering/audio/timing,
usable control and persistence. No AI/action/combat/outcome or later story
is established. These Unknowns are not an automatic acquisition queue.
