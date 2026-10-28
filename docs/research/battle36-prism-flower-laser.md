# Battle36 Prism Flower laser targeting and results

Continue with [Battle36 outcomes and Galam return](./battle36-outcomes-and-galam-return.md).

Evidence date: 2026-10-20. **Confirmed (static):** the eight kind93
Prism Flowers decide whether to fire using an ally-only occupancy array,
then action6 reconstructs its ray using both sides and sorts its targets.
The ray penetrates occupied squares and ignores terrain and the Battle36
crop. Enemy targets can therefore take friendly fire. Base damage16 enters
the shared damage routine; ordinary non-debug variance makes it12–16,
not a fixed16 loss. Script construction changes temporary HP, restores the
HP snapshot, and emits reactions for later playback. These are supplied-state
source deductions, not a reached Battle36 attack or playable acceptance.

## Agreement and evidence route

Standard rigor for [Issue #1132](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1132)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect the accepted [activation and first dispatch](./battle36-activation-and-first-turn-dispatch.md)
to this special attack's decision, actual action handoff, target reconstruction,
damage/result commands and general post-action interfaces. Accepted base is
`f6a6cbf019bba5e37a8fa2cd153ebdbedfc7bbfb` (#1130/PR1131).
Own this document and navigation only in its predecessor. No stacked dependency,
shared index/schema/fixture, design contract or implementation change is needed.
Whole rounds, Battle36 outcomes/after scenes, Battle37 and a full scene-engine
or statistics audit are excluded. Initial positions are inputs, not legality rules.

Reuse [battle AI](./battle-ai.md#dispatcher-and-standby-control),
[battle actions](./battle-actions.md#confirmed-action-pipeline),
[battle loop](./battle-loop.md), [RNG/math](./runtime-rng-and-battle-math.md),
the [control lifecycle](../design/contracts/battle-control-lifecycle.md),
[AI contract](../design/contracts/battle-ai-decision.md) and
[action contract](../design/contracts/battle-action-construction.md).
The [initialization owner](./battle36-initialization-and-opening.md#eight-laser-facing-declarations-and-consumer-boundary)
owns formation/definitions, independent scene actors/facings, fresh/repeat/F88
initialization and load limits. This document supplies the selected36 joins.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`ai/`, `battlefield/`, `battlefunctions/`, `battleactions/`, `battlescenes/`
abbreviate children of `code/gameflow/battle/`; `stats/` means
`code/common/stats/`. Constants/RAM come from `sf2enums.asm`/`sf2const.asm`.
Combatant IDs are byte IDs, enemy ordinals zero-based, intervals half-open.
The registered USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source equality or runtime observation is claimed.

Minimum evidence is selected source/consumer joins and small position/HP/RNG
deltas. Linear reads of a few dozen source files plus constants, at most62
occupancy insertions and48 ray squares per supplied valid state, need no export,
seed enumeration or canonical input load. A direct command has a one-minute,
512MiB budget; peak memory is unmeasured. Small unique receipts and the ignored
recipe live in `local/issue1132`. The immutable18,285,979-byte init/script pair
and `local/issue1005/pair.txt` provenance remain reusable but are not loaded here.
No new parser/platform/tool tests, normal/full/old comparisons, SDK, Godot,
emulator, H3/H4, acquisition or screenshots. Reassess any material contradiction
or supporting-tool expansion before scaling.

## Selected flowers and ray coordinates

**Confirmed:** `ai/startaicontrol.asm:StartAiControl` checks kind93 before
ordinary commandsets and calls ProcessLineAttackerAi; the predecessor owns
15/15 activation bypass and control/status admissions. MOV0, HEALER1 and
the LASER spell-list entry do not implement this decision.

`data/battles/global/laserbattles.asm` (`$1AC9B8..$1AC9FC`) and
`getlaserfacing.asm:GetLaserFacing` (`$1AC8A0..$1AC9B8`) select live
CURRENT_BATTLE36's first table, then use `(combatant low byte & $7F)`.
The table contains24 bytes: ordinals1–8 below, all other entries FF.
Scene/entity facing is not consulted. The actor's current signed-byte X/Y
getters determine the origin on every call; the table direction stays tied
to the combatant ordinal even if that actor moves or changes kind.

| Ordinal / ID | Initial X,Y | Direction / value | First square → last square | Squares |
| --- | --- | --- | --- | --- |
|1 /129|11,7|Down /3|11,8 → 11,47|40|
|2 /130|6,11|Right /0|7,11 → 47,11|41|
|3 /131|12,16|Right /0|13,16 → 47,16|35|
|4 /132|13,17|Up /1|13,16 → 13,0|17|
|5 /133|14,19|Left /2|13,19 → 0,19|14|
|6 /134|15,20|Up /1|15,19 → 15,0|20|
|7 /135|18,23|Left /2|17,23 → 0,23|18|
|8 /136|7,26|Up /1|7,25 → 7,0|26|

The helper saves/restores d0–a6; results are RAM side effects, not a returned
facing register. No matching battle or FF facing writes length.w=0 and
does not clear/rebuild the movement grids or target bytes. A valid facing
clears the total-movecost and movable grids to FF, starts one cardinal step
beyond the actor, then for each square:

1. SetMovableSpace writes0 to both grid bytes.
2. GetCombatantOccupyingSpace reads one occupancy byte. An ID other than FF
   is appended to TARGETS_LIST at `$FFB652`, length accumulated in d3.w.
3. Advance one step in the fixed direction, then test the terminating bound.
   Right/Down continue through47; Left/Up through0. Store the final word
   count at TARGETS_LIST_LENGTH `$FFB63E`.

`battlefield/battlefieldengine.asm` independently establishes48×48 storage,
the address `base + 48*y + x`, and the separate occupancy/grid buffers
`$FF5600`, `$FF4400`, `$FF4D00`. This path never tests terrain, walls,
LOS, movement costs, ordinary spell range/radius or crop0,0/28×32. It neither
stops on a target nor clears occupancy while clearing the movement grids.
All eligible occupants along the line can be collected, including beyond
the visible battle crop. The origin square is excluded for these valid
interior starts.

Boundary tests occur **after** processing a square. A changed actor at the
outward edge (e.g. x47/Right) processes x48 once before stopping; there is
no initial-coordinate guard here. Do not generalize this into a safe clipped
ray API for malformed/edge starts. All eight declared origins avoid that
case; runtime reach and arbitrary invalid-state memory effects are Unknown.

## Occupancy, decision and changed-state examples

**Confirmed:** ProcessLineAttackerAi (`ai/startaicontrol.asm`) saves d0–a6,
retains the actor in d7.w, calls BuildTargetsArrayWithAllies and GetLaserFacing.
`battlefield/buildtargetsarray.asm` (`$C7F2..$C87A`) clears all2304 occupancy
bytes to FF and visits ally slots0–29 in ascending order. BuildTargetsArray
requires HP.w **nonzero**, activation neutral bit3 clear, unsigned Y.w<48
and X.w<48. X/Y are sign-extended bytes, so negatives fail those unsigned
checks; HP8000–FFFF is nonzero and is not excluded by a positive-signed test.
There is no joined/active-member, class, primary-active, terrain or party-list
predicate. A living, positioned nonmember can qualify. A later eligible slot
on the same square overwrites an earlier one; an ineligible slot writes nothing.

The ray list is near-to-far spatial order, not combatant order. Nonzero
length writes action.type.w=6 at offset0, zero-extends the first target byte
and writes it to item-or-spell.w at offset2. Offset4 (spell/item target) and
other fields are not assigned here. Zero length writes Stay.type.w=3 and
leaves the old offset2 word untouched. **Both** paths write FF to the first
byte of BATTLE_ENTITY_MOVE_STRING, terminating it; neither asks movement AI
to reposition, searches another direction nor consumes decision RNG.

The following supplied states vary only the named inputs; all other slots
are absent/ineligible and positions are in48×48. IDs are actual slots, not
formation ordinals. These are static deductions, not recorded play.

| Supplied state | AI list / decision | Action reconstruction consequence |
| --- | --- | --- |
|Flower130 at6,11; ally9 at8,11, ally2 at12,11|[9,2]; action6, offset2=9|Same ray sorted to[2,9]; both are hit|
|Same, enemy140 at10,11|Still[9,2], enemy cannot trigger the decision|All-combatants ray[9,140,2] sorts to[2,9,140]; friendly fire|
|Only enemy140 at10,11, no eligible allies|[]; Stay|No laser script is requested merely because an enemy is in line|
|Ally9 HP0, neutral bit3 set, X−1, Y−1, X48 or Y48 (each separately)|That slot is omitted; an otherwise empty line Stays|The same insertion filter applies later|
|Ally9 HP8000, neutral clear, at8,11|[9]; action6|Raw signed-high HP differs from region-trigger eligibility|
|Ally2 and9 both at8,11|[9]; later ally wins|Add enemy140 at8,11 and it overwrites both on reconstruction, yielding[140]|
|Ally9 at30,11|[9]; action6|Outside the28-wide crop still qualifies in48-wide storage|
|Ally9 at8,12 instead of8,11|[]; Stay|No width/radius expansion around the ray|
|Flower130 moved to6,12; ally9 at8,12|[9]; action6|Origin changes; its table direction remains Right|
|After decision, ally9 leaves the ray or dies; no other target remains|Previously selected action6 persists|Rebuild can produce length0; the old offset2 target does not force damage|

With unchanged complete fresh placement and no additional positioned slots,
none of the twelve declared ally positions intersects any of the eight rays.
The conditional initial decision is therefore Stay for every flower.
This does not predict the first reached flower turn: prior actors, live slots,
incoming state and service returns have not been observed.

## Actual action handoff and reconstruction

**Confirmed:** `battlefunctions/executeindividualturn.asm:ExecuteIndividualTurn`
(`$23EB0..$24242`) calls StartAiControl before cursor/view/window/range
preparation; sleep/stun can then divert to NoAction. Unchanged AI control
classification reaches ExecuteAiControl. Status/control changes and incomplete
services can prevent that handoff; deciding action6 is not executing it.

`battlefunctions/battlefunctions_2.asm:ExecuteAiControl` first performs its
movement-range/grid and move-string services, writes the returned d2/d3
positions back to combatant X/Y, deletes blocks and checks Stay. An FF-first
move string supplies no movement steps but still enters the general entity
service. In the laser branch it calls GetLaserFacing again, waits for view
scroll, creates ray blocks, clears CURSOR_RADIUS.b and loads the **old offset2
word** into itemOrSpellIndex(a6). PerformAiTargetingVisualAct uses that ID's
current coordinates to choose the actor's display facing, focus the cursor,
open a mini-status window, request Sleep15 and SFX_VALIDATION67. That is a
visual/focus input, not an authoritative single damage target. This laser call
does not itself rebuild occupancy; earlier range services may have used the
scratch buffers, so it is not an immutable copy of the initial AI list.

After return, ExecuteIndividualTurn's action checks allow6 through (it is
neither Egress, Angel Wing, Stay nor a physical Kiwi conversion), request two
WaitForVInt calls and invoke WriteBattlesceneScript. Each later step depends
on prior services returning. The routine later passes itemOrSpellIndex to
LoadBattle; the initialization/activation owners retain that reload/view
alias boundary. It does not make the first AI target the final hit list.

`battleactions/battleactionsengine_1.asm:WriteBattlesceneScript`
(`$9B92..$9EC4`) initializes actor bytes, EXP/gold/attack-type and transient
toggles, then calls DetermineTargetsByAction. Its action6 branch:

1. Calls BuildTargetsArrayWithAllCombatants: clear/populate0–29, then128–159
   **without another clear**, using the same HP/neutral/coordinate filter.
   Enemies overwrite allies at overlapping squares; later enemy IDs overwrite
   earlier ones. Positions/HP/neutral changes since the decision are live inputs.
2. Writes FF to TARGETS_LIST[0], loads the scene actor byte into d0 and calls
   GetLaserFacing. The action's offset2/offset4 are not read by this branch.
3. Always calls battlesceneScript_SortTargets. Ordinary selected36 targets
   sort by unsigned combatant ID, hence allies before enemies, independently
   of distance. The existing Burst Rock marker/HP pass remains in the shared
   [sort owner](./battle-actions.md#items-and-special-gates); selected36's
   declared roster contains no Burst Rock.

Laser property initialization snapshots HP for30+32 slots and bypasses
ordinary MUDDLE2/same-side classification. It does not enter attack curse/stun
rolls or cast-spell silence/property setup. InitializeActors initially selects
the enemy actor alone for action6 (ally display byte FF); SwitchTargets later
selects each target's side for reactions. A friendly-fire target is not
filtered out. Length0 still passes action-message/animation construction,
then skips the effect loop; it does not retroactively turn action6 into Stay.
No original service completion is implied by this source order.

## Base power, RNG, HP and later result interfaces

**Confirmed:** `battleactions/battleactionsengine_2.asm:ApplyActionEffect`
(`$A34E..$A49C` file) loads BATTLEACTION_PRISM_LASER_POWER=16 into d6.w
for **each** target and calls InflictDamage (`$ACEA..$AE32`), then emits a
death message if targetDies.b is nonzero. It does not call Attack, CastSpell,
ATT−DEF/terrain protection, spell-power/resistance, MP-cost, learned-spell,
dodge/critical/ailment/curse or double/counter determination. The selected
flower's MP0 and enhanced spell-power definition do not modify laser power.
The action's first-target word is not a spell ID in this path.

Reuse the [shared RNG/debug](./runtime-rng-and-battle-math.md#confirmed-debug-aware-rng-override)
and damage/stat/replay evidence. The selected connection is:

1. The Taros ineffective gate is still inside InflictDamage, but the
   battle-specific initializer leaves it clear in Battle36. Attack-type.w
   starts0, so the generic counter halving does not apply to this first laser.
2. A Burst Rock target bypasses variance and takes base16; selected36's
   unchanged roster has none. Every other target calls
   GenerateRandomOrDebugNumber **twice**, each with range `(16 >> 3)+1=3`,
   subtracting both results from d6.w and enforcing a signed-positive minimum1.
   With ordinary RNG, each result is0–2, giving12–16. Draws advance the shared
   RANDOM_SEED word, not thinking-AI RNG. The established recurrence is
   `s'=(13*s+7)&FFFF`, result `floor(s'*3/65536)` for range3. The range does
   not shrink after the first subtraction. Targets consume these calls in
   **sorted** order; they do not each restart from the same seed.
3. Debug with a direction returns Right0, Up1, Left2, Down3, first matching
   in that order, without advancing the seed. Down is not clamped to range3;
   two held-Down overrides produce10 damage. Debug enabled/no direction uses
   ordinary RNG. DEBUG_MODE action-selection services can also change the
   action before construction; the calculations here assume action6 remains.
4. CalculateDamageExp and AddExpAndGoldForKill return early for enemy actors.
   DecreaseCurrentHp performs the shared word subtraction/clamp; targetDies
   is set if the temporary result is0. For valid ordinary HP≤maxHP<8000,
   `HPafter=max(0,HPbefore-damage)`; malformed signed-high words follow the
   native signed-subtract/clamp rather than a new unsigned-health promise.
5. Emit executeAllyReaction or executeEnemyReaction with signed HP delta
   `-damage`, MP delta0, current status word and flags1, then the damage
   message. The command/message amount remains the calculated damage even
   when current HP is smaller. Selected ordinary laser flags for critical,
   cutoff, ailment and double/counter remain0.

| Supplied arithmetic | Calculated damage / temporary HP | Result boundary |
| --- | --- | --- |
|Ordinary seed0, first target HP20|Draws0,0; seed7 then98; damage16, HP4|Reaction−16, amount16|
|Next sorted target HP20, same stream|Seeds1281 then16660, draws0,0; damage16, HP4|Continue the stream, do not reset it|
|Ordinary seed1234(hex), HP20|SeedsECAB then04B6; draws2,0; damage14, HP6|Two range3 calls|
|Same seed1234, HP10|Damage14, HP0|Reaction−14 and amount14, then ally/enemy death message|
|Held debug Down, HP20|3+3 subtracted; damage10, HP10; seed unchanged|Override lies outside ordinary0–2 outputs|
|Changed Burst Rock target, HP20|Damage16, HP4, no variance draw|Generic exception; not an initial Battle36 actor|

During script construction each target also enters DropEnemyItem, which
returns immediately for the enemy flower actor. The general MakeActorIdle,
BreakUsedItem, ValidateDoubleAttack/ValidateCounterAttack and End interfaces
follow; no physical determination set follow-up toggles for this ordinary laser.
The separate debug hit menu (`code/gameflow/special/debugmodebattleactions.asm`,
enabled by debug mode plus player2 Start) can set debugDouble/debugCounter;
both validators apply those overrides **after** their ordinary clear branches.
Consequently the no-follow-up result assumes no forced debug hit flags, in
addition to leaving action6 unchanged. Broader forced-debug follow-ups retain
the general action owner rather than becoming normal laser rules.
End does not grant this enemy actor EXP absent a counter, then restores all62
saved HP words and emits the script terminator. This restoration does not
undo RNG or make emitted damage a persistent HP result yet.

`battlescenes/battlesceneengine_0.asm:bsc0B_executeAllyReaction` and
`bsc0A_executeEnemyReaction` later consume signed HP/MP/status/flags words,
call the clamped HP routine and refresh stats. The
[accepted scene-replay evidence](./runtime-rng-and-battle-math.md#confirmed-statically-and-at-runtime-physical-base-damage)
owns the general temporary/restored/playback distinction; it is not a new
laser runtime observation. Subsequent scene termination, LoadBattle and the
battle loop's dead-list/statistics/after-turn interfaces retain their
[loop owner](./battle-loop.md). In particular an enemy friendly-fire death
is not evidence of an ally-earned kill/drop/EXP award. Do not infer Battle36
victory, a whole round, completed cleanup or persistent save state here.

## Message, animation and audio requests

**Confirmed (selection only):** DisplayActionMessage selects319 for kind93;
the same action6 selector would use321 for kind38 Zeon Guard. InflictDamage
selects285 for an ordinary enemy attacker regardless of the target's side;
DisplayDeathMessage selects291 for an ally target,290 for an enemy target.
Only IDs, author paraphrases and complete control-token sequences are retained:

| ID | Author paraphrase / selected role | Ordered control tokens |
| --- | --- | --- |
|319|Prism Flower announces laser|none|
|321|Zeon Guard variant; outside selected36 roster|none|
|285|Enemy attack damage amount|`{NAME}{N}{#}`|
|291|Ally exhaustion|`{NAME}{N}`|
|290|Enemy defeat|`{NAME}{N}`|

These entries are independently checked in
`data/scripting/text/gamescript.txt`; no dialogue corpus is reproduced.
The battle-scene [message owner](./battle-actions.md#battle-scene-message-commands)
owns displayMessage command$10 and its six-word format; no new text-wait
timing is proved. No spell-cost message or MP reaction is constructed for6.

GetSpellanimation selects SPELLANIMATION_PRISM_LASER=$15, sets mirrored bit7
for the enemy actor (selector$95); PerformAnimation keeps attack animation
type0 and AnimateAction emits animateEnemyAction. Scene setup dispatches to
`battlescenes/animation/prismlaser.asm`; its setup selects the laser tileset
and tests the Prism Flower **battle sprite**, independently of kind-based
AI/message selection. The updater requests SFX_PRISM_LASER_LOADING103 and
SFX_PRISM_LASER_FIRING108. ExecuteIndividualTurn selects enemy attack music5
for kind93 (it is not one of the listed boss-music overrides). These requests
are separate from cutscene firing SFX111. No tiles/sprites/audio are exported.
Animation setup itself draws RNG; damage draws described above are not an
assertion that an entire attack consumes only those draws. Renderer/VInt,
sound completion, shape, timing and final display remain Unknown.

## Actual current consumers and missing binding

**Confirmed about accepted code, by static read:** production consumes
prepared Content, not disassembly at runtime. The
[capability owner](../../remake/docs/capability-status.md) remains authoritative
for actual supported play. Generic reaction/scene types do not imply a laser.

| Actual consumer | Existing expression | Selected36 gap |
| --- | --- | --- |
|[Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [encounter pins](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Placements, source orders/loadouts and enemy definitions exist; source formation coordinates fit.|Pin and region-program-table admit1/2/3, not36. Existing selected mover/commandset/enhanced-state/BIT4 limits stay with the predecessor. No36 laser-facing table/occupancy policy is bound.|
|[SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [RuleCompositions](../../remake/src/Sf2.Remake.Application/Gameplay/RuleCompositions.cs)|Actual source-orders decision strategy clears Tested and supports15/15 admission.|No kind93/38 line-attacker branch. After admission, commandsets are restricted to6/7/9/14 and supported physical-only loadouts; LASER in a loadout is no substitute.|
|[Action/state types](../../remake/src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs), [automatic decision/result](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs)|BattleActionKind is Stay/Healing/Physical/Item. BattleReaction can represent damage and a result can hold reactions.|Automatic decisions carry an optional single opposite-faction target, no laser action kind or near-to-far/rebuild distinction. Generic arrays/strings are capacity, not native laser semantics.|
|[BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs), [movement continuation](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleMovementContinuation.cs), [physical action](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs)|The actual automatic decision runs before movement and action construction; scene reactions can later apply HP.|FinishAutomatic turns any target into Physical. Physical target admission rejects same faction; damage depends on ATT/DEF/terrain and physical rolls. Reusing this path would change the laser's both-side targets, base16 and RNG.|
|[Scene continuation](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs), [action commit](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs)|Scene phases, HP/reward/death handling and observation boundaries exist.|No selected laser producer, action6 message319, native target reconstruction or laser power binding is supplied by these generic continuations.|
|[Godot scene view](../../remake/game/src/Battles/BattleSceneView.cs), [board projection](../../remake/game/src/Battles/BattlePresentation.cs), [viewport](../../remake/game/src/Battles/BattleMapViewport.cs)|Actual consumers render actor/target, Damage/Recovery/Dodge reactions, text/audio and board HP.|ActionMessage defaults to273 outside its existing cases; generic damage285 does not bind laser319/selector95/SFX103/108 or ray projection. No selected36 material/effect/usable-control observation exists.|

Minimum future integration needs selected36 content/material admission plus
an explicit special-kind decision/action path, current-position facing data,
the two occupancy policies and sorted multi-target damage, then corresponding
scene projection. This is a gap description, not implementation authorization,
an engine-ready label or a runtime acquisition queue.

## Reproduction, preserved results and Unknowns

The ignored recipe verifies clean pinned source, selected table/coordinates,
instruction branch/order/width, occupancy/changed-state examples, source-backed
RNG/HP examples, message tokens, actual consumers and two-path/nav-only scope,
UTF8/LF, links and private boundary. Load current selections in the same process
and choose unused receipt names:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1132/review.py <configured-source-disasm-root> --receipt local/issue1132/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The source argument ends in disasm and contains sf2enums.asm, code/ and data/.
Check actual exact-head scoped CI and committed planner output; suggested
normal verification is **SKIPPED** under the explicit docs-only agreement,
not an executed PASS. ADR0004's applicable evidence/semantic/private checklist
applies; parser/schema/fixture mutation and H3 obligations are N/A here.

Preserve #1130 author/main direct0.332s PASS and CI39543645357; that accepted
finding stops at the laser entry. Preserve #1128's original success that missed
three prose defects and its accepted focused corrections: clear bits5/6 disable
collision checks; HasSameDestinationAsOtherEntity tests moving a0's bit5;
global leader-presenceFF is nonzero. The predecessor's named result/handoff
retains the exact original and correction evidence. The
[admission owner](./roft-to-galam-and-battle36-admission.md) retains Map53
event-enabled import UnboundLocalError/zero partial programs. Earlier named
alias/collision/follow/import/statistics corrections and twelve completed old
comparison failures stay with their existing owners. No result here closes
them, repeats a completed suite or changes a completed failure to interrupted.

Freeze a clean committed/pushed Draft PR with exact base/head/tree, actual
commands/CI/failures/Unknowns and ignored handoff, move Project to Review / Verify,
notify main-gate once and stop writer/processes. Independent acceptance/merge
remains main-gate-owned; no next Issue, merge, cleanup/archive or private upload.

**Unknown:** natural Battle36 reach, actual party/slot/state/coordinates,
HP/MP/status/equipment/neutral/control changes and overlap, incoming RNG/debug
input and reached actor/target list; service interruption/interleaving and
completed movement/cursor/view/window/reload/scene/message/animation/audio/DMA,
renderer/VInt timing, usable control and persistence. Broader malformed-state
behavior, original terrain/material decoding and equality beyond existing
evidence remain open. Battle36 outcomes and Battle37 remain outside this stop.
