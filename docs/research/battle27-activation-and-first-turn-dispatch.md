# Battle27 activation and first-turn dispatch

Evidence date: 2026-10-14. **Confirmed:** after completed fresh initialization
and opening, all twelve declared formation positions are outside Battle27's
four regions under the actual integer consumer. The sixteen placed, living
enemies still qualify for turn records: activation, spawning and turn
admission have different predicates. This note follows round preparation to
the first ExecuteIndividualTurn call and stops before its body. **Inferred:**
the formation stages contact before movement into the declared regions.
**Unknown:** natural reach, actual party/state/RNG, first actor, completed
services, timing, presentation, usable control and persistence.

## Agreement and provenance

Standard rigor for [Issue #1037](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1037)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
checked regions, persistent flags, activation words, spawn modes and turn
records are distinct state. Accepted starting base is
`85016e1c27af3fd91cd82d0468b37c896c69f3b4`, including #1033/PR1036. This
is the sole tracked owner; no shared code, schema, fixture, index, design,
remake or translation write. Concurrent #1038 and UI692 supply no evidence.

Start at the accepted [Battle27 initialization/opening endpoint](./battle27-initialization-and-opening.md),
`BattleLoop:@Start`, after LoadBattle and the start wrapper returned. Preserve
its F477-before-scene, missing-mapping and F88 supplied-state limits. Consume
the accepted [Battle26 activation and turn consumers](./battle26-activation-and-first-turn-dispatch.md),
[battle loop](./battle-loop.md), [lifecycle contract](../design/contracts/battle-control-lifecycle.md),
[placement/activation](./battle01-placement.md), [RNG/turn evidence](./runtime-rng-and-battle-math.md),
[cutscene routing](./battle-cutscenes.md) and [routing contract](../design/contracts/battle-cutscene-routing.md).
Their observations prove their named seams, not a natural Battle27 traversal.
No AI/action body, movement, damage, complete turn, outcomes, Battle32/later
story, runtime, capture, extraction or seed enumeration is included.

All source paths are relative to `disasm/` at pinned
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained source is clean and read-only. Existing evidence binds USA
retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Constants/RAM are `sf2enums.asm` and `sf2const.asm`; Fnnn denotes flag nnn.
Short `battleloop/` and `cutscenes/` paths are under `code/gameflow/battle/`.
All deductions below are conditional source/data results, not new runtime.

Minimum evidence is selected pinned source, current Battle27 rows, existing
public fixtures/schemas and retained payloads. Reuse the unchanged
18,285,979-byte map-init/script pair and accepted 0.083s provenance without
regeneration. Selected linear reads/joins are budgeted under one minute and
512 MiB, with small prose/receipts, no acquisition or model/API processing.
Replan contradictory evidence or material supporting-system expansion;
no new maintained machinery or generic geometry/AI/shop audit is needed.

## Ordered prefix and supplied continuation

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) executes this ordered prefix at `@Start`:

1. ActivateEnemies, including its extra fall-through helper call for160.
2. `j_ExecuteBattleRegionCutscene`.
3. Test DEBUG_MODE_TOGGLE.b; zero skips PrintAllActivatedDefCons, nonzero
   calls it before spawn admission. Its sixteen flag checks90–105 request
   text463 for held flags, with DIALOGUE_NUMBER.l set to the region index,
   then close text. These services must return; no display is observed.
4. `j_PopulateTargetsListWithSpawningEnemies`; read TARGETS_LIST_LENGTH.w.
   Zero skips spawning. Otherwise decrement the count, walk TARGETS_LIST
   bytes in stored order, clearing d0.w before each ID read, and call
   SpawnEnemyWithCamera for each entry via DBF.
5. GenerateBattleTurnOrder, then `@IndividualTurns_Loop`.
6. Clear d0.w, read CURRENT_BATTLE_TURN.b as a **byte offset**, point a0 to
   BATTLE_TURN_ORDER and read the selected combatant byte into d0.b. `$FF`
   branches back to `@Start`; otherwise `bsr.w ExecuteIndividualTurn` is
   this note's endpoint. No score, HP, membership, activation or end-of-battle
   check intervenes. d0.w is the zero-extended ID; upper d0 bits are not
   cleared. a0 addresses the buffer. No body execution/return is claimed.

The [F88 continuation](./battle27-initialization-and-opening.md#f88-continuation-and-changed-state-boundary)
restores saved seconds, clears88/AI memory and view target, calls LoadBattle,
then branches directly to `@IndividualTurns_Loop`; the subsequent branch to
`@Start` is unreachable. Initially it bypasses this entire preparation and
uses supplied turn/order bytes without alignment/range validation. A non-FF
byte reaches the call even if its ID would fail fresh admission. A sentinel
instead enters `@Start` using actual CURRENT_BATTLE and supplied flags,
positions, HP, modes, AGI and RNG. Incoming map74/battle27 arguments do not
repair current selectors on F88. The predecessor's d0=74 out-of-list view
lookup and missing physical mapping remain limits; loading is not a
byte-for-byte preservation or safe-state validation promise.

## Activation scan and all formation positions

**Confirmed:** `battleloop/activateenemies.asm` (`$2550C..$25544`) uses
start128/counter31:32 calls for128–159, preserving the loop counter around
each helper. UpdateEnemyActivationBitfield requires X.b nonnegative signed,
current HP.w nonzero, and d0.b negative. It does not test Y, membership,
neutral bit or move orders. A high-bit HP word passes this nonzero test.
Fresh128–143 pass; absent144–159 have X=-1/HP0 and fail. Battle27 has no
neutral group or random-enemy upgrade, even with completion527 held.

`battleloop/triggerregions.asm:TriggerRegionsAndActivateEnemies` preserves
d0–a6, selects the actual current battle's region subsection, and returns
without activation writes if its count is zero. Battle27 count4 scans0–3.
For each it tests NEWLY_TRIGGERED_BATTLE_REGIONS.w (`$FFB20C`): held checked
bits skip geometry. An unchecked region calls IsRegionTriggered and sets
F90+index only if d1.b is nonzero. It then sets the checked bit even on a
geometry miss. Neither this helper nor this prefix clears the checked word
or persistent flags. Fresh initialization cleared F90–105, **not** the checked
word. The later `ai/startaicontrol.asm:@NonSwarmAi` zero write is outside
this boundary and cannot establish the incoming word.

### Integer region join

**Confirmed:** `data/battles/spritesets/spriteset27.asm` declares four
12-byte type4 regions. IsRegionTriggered reads vertex words at2/4/6/8 and
tries triangle1/2/4, then3/2/4 only if no eligible ally matched the first.
The complete vertices are:

| Region / flag | Vertices1,2,3,4 |
| --- | --- |
|0 /90|`(3,0),(3,47),(31,22),(31,0)`|
|1 /91|`(0,0),(0,37),(31,37),(31,0)`|
|2 /92|`(0,0),(0,18),(28,47),(28,0)`|
|3 /93|`(0,0),(0,29),(31,29),(31,0)`|

AreAnyAlliesInTriangle scans all30 slots0–29, requiring **positive signed**
HP.w and nonnegative signed X.b/Y.b. It does not consult active/joined
membership. IsAllyInTriangle returns zero inside; the caller returns -1
for any match, otherwise0, and IsRegionTriggered copies that word to d1.
Thus living placed nonmembers can trigger after a state change; HP0,
negative-HP words or negative coordinates cannot. Activation/turn helpers
instead admit nonzero HP and do not check Y. No enemy/neutral enters the
region's ally scan.

Reuse the actual integer edge consumer, not a floating-point polygon test:
CheckTestPointPositionRelativeToSegment sign-extends coordinate bytes,
chooses the larger absolute delta (horizontal on equality), and uses signed
multiply/divide with truncation toward zero, **anchored at endpoint B**.
IsAllyInTriangle tests edges in source order, accepts result1 immediately,
otherwise compares the ally side with the opposite vertex and handles
bit0-set exceptions. All selected points/vertices are nonnegative and the
triangles nondegenerate. The index guard is `cmp.b d1,d7` / `ble`, not a
strict index<count check; normal iteration supplies only0–3. `$FF` has a
separate false return; trigger15 is a sentinel, not a polygon lookup.

Every declared formation ordinal has the same four-region result:

| Ordinal | X/Y | Region0 | Region1 | Region2 | Region3 |
| --- | --- | --- | --- | --- | --- |
|0|`(14,44)`|outside|outside|outside|outside|
|1|`(15,44)`|outside|outside|outside|outside|
|2|`(14,45)`|outside|outside|outside|outside|
|3|`(15,45)`|outside|outside|outside|outside|
|4|`(14,46)`|outside|outside|outside|outside|
|5|`(15,46)`|outside|outside|outside|outside|
|6|`(16,46)`|outside|outside|outside|outside|
|7|`(17,46)`|outside|outside|outside|outside|
|8|`(16,45)`|outside|outside|outside|outside|
|9|`(17,45)`|outside|outside|outside|outside|
|10|`(16,44)`|outside|outside|outside|outside|
|11|`(17,44)`|outside|outside|outside|outside|

For reproducible rejection arithmetic, let trunc mean truncation toward zero.
Region0's first triangle rejects on edge4->2: projected X is
`3+trunc(-28*(Y-47)/47)`, only3–4, below every X14–17; vertex1 is on the
other side. Its second triangle rejects on edge3->2: projected Y is
`47+trunc(-25*(X-3)/28)`,35–38, below every Y44–46; vertex4 is above.
Region2's first triangle rejects on edge4->2 with projected Y
`18+trunc(-18*X/28)`,8–9; its second rejects on edge3->2 with projected X
`trunc(28*(Y-18)/29)`,25–27, above every X. Opposite vertices1/4 respectively
lie across those edges. Region1's first triangle rejects at edge4->2: its X projection
`trunc(-31*(Y-37)/37)` is negative, so the actual `move #1,ccr` /
SynthesizeFlag path returns2, unlike vertex1's result0. Region3's first
triangle projects Y as `29+trunc(-29*X/31)`,14–16, below every Y44–46.
Their second triangles reject at horizontal edge3->2, Y37/29. Earlier
tested edges do not produce an immediate in-bounds-collinear acceptance
for these twelve points. Negative-projection source comments are not used
in place of the actual CCR/flag-synthesis instructions.

With unchanged fresh state, fewer/dead party members occupy a subset of these
positions; initialization gave other ally slots X=-1. F90–93 stay clear
regardless of incoming checked bits. The first eligible enemy sets the low
four checked bits; the resulting word is incoming OR `$000F`, not necessarily
exactly15. Other flags90–105 remain clear. Changed placed allies, flags or
mid-prefix mutation invalidate that conclusion; already checked bits prevent
a later position change from automatically causing a rescan on this prefix.

## All sixteen row effects and the extra call

**Confirmed:** GetTriggerRegions splits the stored byte into primary high
and secondary low nibbles. Both15 returns without an activation write.
Otherwise a held non15 primary wins and sets bit0; only a missing/clear
primary permits a non15 held secondary to set bits0/1. Misses preserve the
word. The masks/bsets preserve other bits, including a previously held bit1
on the primary path. All fresh primary/secondary-active bits are clear.

| Row / ID | Type | Trigger primary/secondary | Fresh word | Held relevant flag |
| --- | --- | --- | --- | --- |
|0 /128|Lizardman|3/3|`2000`|F93 -> `2001`, primary wins|
|1 /129|Wyvern|3/3|`2000`|F93 -> `2001`, primary wins|
|2 /130|Dark Bishop|3/3|`3000`|F93 -> `3001`, primary wins|
|3 /131|Dark Soldier|15/15|`2000`|no activation write|
|4 /132|Dark Soldier|0/15|`2000`|F90 -> `2001`|
|5 /133|Dark Soldier|15/15|`2000`|no activation write|
|6 /134|Wyvern|2/15|`2000`|F92 -> `2001`|
|7 /135|Dark Sniper|2/2|`2000`|F92 -> `2001`, primary wins|
|8 /136|Lizardman|2/15|`2000`|F92 -> `2001`|
|9 /137|Dark Sniper|15/15|`2000`|no activation write|
|10 /138|Wizard|0/0|`2000`|F90 -> `2001`, primary wins|
|11 /139|Dark Sniper|0/0|`2000`|F90 -> `2001`, primary wins|
|12 /140|Dark Bishop|0/15|`3000`|F90 -> `3001`; no secondary trigger|
|13 /141|Dark Bishop|1/15|`3000`|F91 -> `3001`; no secondary trigger|
|14 /142|Lizardman|1/15|`2000`|F91 -> `2001`|
|15 /143|Dark Sniper|2/2|`2000`|F92 -> `2001`, primary wins|

Words are hexadecimal. Rows12/13 store primary FOLLOW_ENEMY10/14 and
secondary FOLLOW_ENEMY0, but move orders are **not trigger regions**:
their actual secondary trigger is15. They have neither a same-region
secondary nor an alternate region0 activation path. No unchanged row takes
a secondary-only path: same-region pairs select primary and all other
secondary triggers are15. Changed trigger bytes can admit secondary; held
bit1 can survive primary selection. With fresh flags/positions all sixteen
words remain unchanged. Commandset/priority/order names do not prove tactics,
control ownership, movement or a chosen target.

After32 iterations, d0.w=160 and d7.w=`$FFFF`. No RTS/branch/data separates
ActivateEnemies from UpdateEnemyActivationBitfield, so the helper executes
once more before returning to BattleLoop. The accepted
[slot160 address/alias join](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
applies unchanged: `GetCombatantEntryAddress` uses `bhi` above160, admitting
160 and mapping `(160-96)*56+$FFE800` to CURRENT_GOLD `$FFF600`.

Its pseudo-X byte `$FFF62E` is quantities84/85; pseudo-HP word `$FFF60E`
contains ring20–23 nibbles; trigger byte `$FFF636` is quantities100/101;
activation word `$FFF634` is quantities96–99. Nonnegative pseudo-X,
nonzero pseudo-HP and a held non15 trigger flag90–104 can OR bit0, or bits0/1
on secondary selection, into Goddess Staff99's count nibble. Other bits are
preserved. This is neither unconditional item addition nor a shop routine.
For unchanged fresh Battle27 no region flag is held, so even an eligible
pseudo-combatant makes no deals write. Changed allies/flags/deals may change
that result; prior per-enemy writes are visible to the extra call. No shop
visit, persistence, saturating increment or source-comment-only claim follows.

## Region hook and spawn admission

**Confirmed:** the complete `data/battles/cutscenes/regionactivatedcutscenes.asm`
table has four8-byte records then a word terminator:

| Current battle | Region | Played flag | Script |
| --- | --- | --- | --- |
|32|0|386|rbcs_battle32|
|32|1|386|rbcs_battle32|
|40|0|387|rbcs_battle40_1|
|40|1|388|rbcs_battle40_2|

The actual `cutscenes/regionactivatedcutscenes.asm:ExecuteBattleRegionCutscene`
wrapper advances by8, tests terminator, then compares CURRENT_BATTLE.b.
With27 throughout it skips every record and returns without checking played
or region flags, setting a played flag or dispatching MAPSCRIPT. This follows
the complete table, not absence of a guessed file. A changed current battle
is a distinct premise: matching rows check played flag then region, set the
played flag **before** MAPSCRIPT, and return after the selected script.
Interruption can leave its flag set before completion; no later story is
traversed here. Debug-dependent diagnostics follow this wrapper.

`battleloop/populateenemyspawns.asm` scans32 slots128–159 and masks activation
with `$0F00`. Zero skips directly without HP/region/position/reset/item calls.
All sixteen fresh rows are STARTING0; absent144–159 have activation0. Region
activation changes only bits0/1, so all32 still skip. The routine writes
TARGETS_LIST_LENGTH.w=0; stale list bytes remain but are not consumed.
SpawnEnemyWithCamera is skipped: no spawning service/animation/item effect
is established by fresh round preparation.

For changed modes, reuse the verified
[spawn/reset consumer](./battle26-activation-and-first-turn-dispatch.md#spawn-list-and-changed-state-conditions):
`$0200` requires HP0 and a held non15 primary/secondary trigger, then max HP0;
`$0100` requires HP0 without a region; `$0300` requires HP0 and the trigger
but no additional max-HP test. Other nonzero masked values append nothing.
The trigger helper writes active bits before a possible max-HP/reset rejection.

ResetSpawningEnemyStats uses the actual row count16 OR `$80` as bound144:
IDs144–159 fail the row bound here, even though they were scanned. For a real
row it checks starting X/Y against all30 allies and32 enemies without HP,
neutral or self exclusion. A dead actor still at its own start can block
itself. Invalid row/occupied start writes max/current HP0, X=-1 and carry;
no ID is appended. Success reinitializes the real row's stats/position/items,
then restores its saved full activation word and returns clear carry through
the setter path. Battle27 does not upgrade; all row item fields are NOTHING,
so no row-specific AddItem override occurs. Copied definition equipment
remains the initializer's input, not a new item award.

Appending is once per successful slot in ascending order; length is a word,
IDs bytes. Later mode comparisons use returned full d1 rather than remasking,
so the accepted fall-through does not duplicate hidden/respawn appends. A
nonempty list calls SpawnEnemyWithCamera, its entity/camera/wait services,
twelve orientation/Sleep3 iterations, sound and text397, preserving outer
iteration registers. Their completion/timing/appearance remains unobserved;
these services must return before turn generation. No animation/AI is run.

## Turn eligibility, records and call boundary

**Confirmed:** `battleloop/turnorderfunctions.asm:GenerateBattleTurnOrder`
(`$25544`, helper `$255A4`) clears64 two-byte records at `$FFF71A` to `$FFFF`.
It offers ally slots0–29 then enemy slots128–157, using
BATTLE_ENEMY_ENTITIES_COUNTER29 rather than COMBATANT_ENEMIES_COUNTER31.
Slots158/159 are excluded even if changed into living placed actors.
AddCombatantAndRandomizedAgiToTurnOrder tests X.b nonnegative signed then
HP.w nonzero. Y, membership, neutral, active-region and commandset are not
tested. Fresh144–157 fail X; all sixteen128–143 qualify with these actual
current stats, joined through definitions, clear initial status and equipment:

| ID | Type | Current HP | Current AGI | A>>3 | Records |
| --- | --- | --- | --- | --- | --- |
|128|Lizardman|40|38|4|1|
|129|Wyvern|46|36|4|1|
|130|Dark Bishop|39|27|3|1|
|131|Dark Soldier|35|34|4|1|
|132|Dark Soldier|35|34|4|1|
|133|Dark Soldier|35|34|4|1|
|134|Wyvern|46|36|4|1|
|135|Dark Sniper|38|35|4|1|
|136|Lizardman|40|38|4|1|
|137|Dark Sniper|38|35|4|1|
|138|Wizard|37|44|5|1|
|139|Dark Sniper|38|35|4|1|
|140|Dark Bishop|39|27|3|1|
|141|Dark Bishop|39|27|3|1|
|142|Lizardman|40|38|4|1|
|143|Dark Sniper|38|35|4|1|

Difficulty changes ATT, not this AGI join; completion527 does not upgrade
Battle27. Changed current HP/X/AGI still require their actual consumer
values. Names, positive HP and clear activation bits determine no first actor.

Reuse the accepted [score/RNG/sort derivation](./battle26-activation-and-first-turn-dispatch.md#turn-buffer-randomized-agility-and-first-dispatch),
verified against these same callers. Current AGI is an unsigned byte;
`A=rawAGI&127`. First score is `A+R(A>>3)-R(A>>3)+R(3)-1`. For rawAGI>=128,
append a second record immediately with `B=floor(A*5/6)` and score
`B+R(B>>3)-R(B>>3)`. Each score stores its low byte after word arithmetic.
The direct RNG advances `(seed*13+7)&65535` on every call, including range0;
these ranges return `floor(seed*n/65536)`. Singles consume3 successive draws,
doubles5, rejects0. These are coupled draws, not independent choices; no
Battle27 seed is selected or enumerated.

All sixteen fresh enemies are single-record, positive-score actors for all
permitted draws:48 enemy draws after actual ally draws. With N eligible
initialized allies and T of them having rawAGI>=128, total records are
`16+N+T`, at most40 for the accepted twelve-position formation. Dead/fewer
members reduce N; changed membership/position/stats cannot inherit this bound
without review. All30 slots are offered even for nonmembers in supplied state.

The constructor sorts the entire64-record buffer with62 passes of63 adjacent
comparisons. `cmp.b`/signed `ble` swaps only when the right score is strictly
greater: ties preserve append order, allies before enemies on equal scores,
and each actor's second entry initially follows its first. Padding255 sorts
as -1, and a wrapped135 as -121. The accepted boundary fixture's rawAGI128
scores0/255 and rawAGI127 score135 remain boundary examples, not Battle27
runtime. No append-capacity guard exists; changed many/doubled candidates
can exceed64, and62 passes do not promise complete sorting of arbitrary64
entries. The fresh maximum40 fits, and positive enemy scores bring a
non-sentinel record to the front within the available passes.

CURRENT_BATTLE_TURN.b (`$FFF79A`, immediately after128 buffer bytes) is then
cleared. The ordered prefix reads offset0 and reaches the first call,
conditional on prior services returning and unchanged fresh premises. Actual
actor depends on party/current AGIs and sequential RNG. An empty admitted
set leaves `$FF` first and repeats preparation, not victory/defeat; changed
scores below -1 can also sort behind padding. The F88 supplied offset may
be odd or out of the buffer; there is no validation before its byte read.

## Changed states, interruptions and reproduction

| Changed premise | Required distinction |
| --- | --- |
|F477 held or opening interrupted|The predecessor's flag-before-scene write does not prove completion; start here only after the wrapper returns.|
|F88/current map/battle/turn differs|Initial direct dispatch uses supplied state; sentinel re-enters preparation with actual selectors.|
|Fewer/dead/changed/nonmember allies|Formation ordinal is not identity; geometry scans30 positive-HP/placed slots; turn admission uses different HP/Y predicates.|
|Checked bits held or coordinates change later|Checked bits skip geometry, including an earlier miss; fresh flag reset alone does not clear them.|
|Region flags/active bits held|No prefix reset; primary precedence, sentinel15 and preserved bit1 still apply.|
|Bishop12/13 secondary orders|FOLLOW_ENEMY0 does not replace secondary trigger15 with region0.|
|Absent/neutral/HP-high-bit slot changed|Activation/spawn/turn have different bounds and tests; no general neutral exclusion or safe-state promise.|
|Deals fields changed|The extra160 call can write count bits only under its actual X/HP/trigger gates.|
|Debug byte nonzero/current battle changed|Diagnostic services or a matching region hook can intervene before spawn/generation.|
|Spawn mode/occupancy changed|Flags can be written before reset rejection; row-bound/occupancy failure changes HP/X; success resets stats before append.|
|AGI/RNG/membership changed|Extra records, score wrapping, finite-buffer writes and first actor require actual inputs.|
|Interruption before endpoint|Earlier checked/persistent/active/deals/list/stats/seed/buffer prefix writes are not rolled back; preparation is not atomic.|

**Unknown:** natural reach and actual party/class/HP/items/flags/deals/checked
word/current/saved state/seed; completed service returns, rendering/audio,
control, persistence and the first actor. No ExecuteIndividualTurn body,
action/AI decision, complete turn or outcome is established. Optional missing
compressed layout remains a no-decoded-route limit, not acquisition authority
or a ROM blocker. No collision-route inference follows from these regions.

Load ignored `local/private-inputs.ps1` in the launching process. Reuse the
clean pinned source and retained-pair metadata/provenance without regeneration.
Direct review joins the full spriteset27/definition data with actual activation,
integer edges, trigger splitting, alias, region wrapper, spawn/reset and turn
consumers; existing lifecycle/control/cutscene facts and their schemas; and
selected RNG/region/turn boundary fixtures. Public spriteset summaries have
`rowContentTracked=false`; they cannot replace the full source row check.
Prior runtime fixtures remain their own seam evidence, not new Battle27 runs.

Reproduce the bounded ignored review with `uv run --locked python -X utf8
local/issue1037/review.py <configured-source-disasm-root>`. It checks the
selected joins, all twelve/four geometry results, sixteen activation/turn
rows and prose boundaries; it is not new maintained verifier infrastructure.
Review full prose/changed states/cached diff, links/anchors, UTF-8/LF,
sole-path/private boundary and `git diff --check`. On a clean committed head
run `uv run sf2 verify plan --base origin/main --head HEAD`; retain actual
selection and exact-head scoped CI in the Issue handoff. Documentation scope
skips blanket local normal/full/Python/.NET/Godot/H3/H4 suites and tests of
review programs. No completed result is reset.

Preserve [#1033 final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1033#issuecomment-6183638591)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1036#issuecomment-6183661030):
final direct0.202s/independent0.195s/CI38675636173 remain distinct from
original head7fee56d2/direct0.207s/CI38675382781. The checker schema-path
FileNotFoundError and corrected local-$ref guard remain preserved, as do the
linked #1029 and predecessor receipts/failures/Unknowns. The accepted
[Battle26 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1009)
retains its neutral-source and endpoint-B integer-projection corrections.
Freeze a clean current-main-based committed/pushed Draft PR, Project Review
and exact identity/results handoff with stopped writer/processes and successful
main-gate callback. No merge, cleanup, archive or next Issue.
