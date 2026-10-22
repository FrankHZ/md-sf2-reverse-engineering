# Battle43 Zeon Guard laser action and results

Continue with [Battle43 outcomes and ending admission](./battle43-outcomes-and-ending-admission.md).

Evidence date: 2026-10-22. **Confirmed (static):** Guards132/135 use the
shared action6 laser, with base power16 and ordinary damage12–16 per target.
Preparation tests allies; script construction rebuilds occupancy from both
sides and sorts the ray by combatant ID. The old focus target is not the
damage list. Kind38 selects message321, while its Prism Flower battle sprite
selects that sprite's animation setup branch. These are conditional source
results, not a naturally reached attack, completed playback or playable43.

## Agreement and evidence route

Standard rigor for [Issue #1180](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1180)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect accepted Guard preparation to execution, rebuilt targets, damage,
emitted results and general post-action interfaces. Accepted base is
`7b546b4ecf72d8ab502d43b3248fdd6b43b04b93`, tree
`b883dcf50720450b3c179135df3f1fc6e48a8659`. Own this document and
navigation only in [first control](./battle43-activation-and-first-control.md).
No stacked dependency or shared index/schema/fixture/contract/code change.
The live position, two occupancy policies and pointer-role changes justify
Standard rigor. Stop before whole-round simulation, Zeon AI, Battle43
victory/defeat, after43 and ending; no engine implementation is authorized.

Reuse [Battle36's laser owner](./battle36-prism-flower-laser.md), especially
its [shared damage/replay derivation](./battle36-prism-flower-laser.md#base-power-rng-hp-and-later-result-interfaces),
[actions](./battle-actions.md), [loop](./battle-loop.md),
[RNG/math](./runtime-rng-and-battle-math.md),
[control lifecycle](../design/contracts/battle-control-lifecycle.md),
[AI](../design/contracts/battle-ai-decision.md) and
[action construction](../design/contracts/battle-action-construction.md).
[Initialization](./battle43-initialization.md) owns roster/definitions,
Zeon128→physical47 with ENTNUM32, and neutral159→Elis/script31.
[First control](./battle43-activation-and-first-control.md#guard-preparation-and-the-action-boundary)
owns activation/status/control admission and the ally-only preparation.
The selected43 joins and changed-state results below are independently checked.

Original paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` abbreviates `code/gameflow/battle/`. Addresses/packed words are hex,
intervals half-open, IDs byte combatant IDs, enemy ordinals zero-based.
sf2const.asm/sf2enums.asm own RAM/constants. Registered USA ROM SHA-256 is
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM comparison, terrain decoding or runtime evidence is claimed.

Method: linear selected source/actual-consumer reads, at most62 occupancy
insertions and48 ray squares per supplied state, plus a few sequential RNG
examples. No seed enumeration. Budget per direct run<60s/<512MiB; peak memory
is **unmeasured**. Small recipe/receipts in ignored `local/issue1180/` target
<2MiB, total outputs<4MiB. Reassess material growth, a full renderer, whole
round or new platform. The immutable18,285,979-byte init/script pair and
`local/issue1005/pair.txt` are neither read nor regenerated. No lowering,
export, asset extraction, runtime collection or new verification framework.

## Pending action through the execution services

**Confirmed:** `battle/battlefunctions/executeindividualturn.asm:ExecuteIndividualTurn`
(`$23EB0..$24242`) can divert to NoAction for Sleep then Stun after
StartAiControl has prepared scratch. It rereads control classification before
calling ExecuteAiControl; preparation does not guarantee that call.

`battle/battlefunctions/battlefunctions_2.asm:ExecuteAiControl` first calls
BuildMovementRangeGrid, creates blocks, selects movement sound and calls
MoveBattleEntityByMoveString. Only after its return does it write d2/d3 to
the actor's live X/Y and delete the blocks. The prepared FF-first string
offers no steps but still enters that service. It then sets blinking and
checks STAY3. An action6 branch calls GetLaserFacing again, waits for view
scroll, creates ray blocks, clears CURSOR_RADIUS.b and copies the **old
offset2 word** to itemOrSpellIndex(a6). It does not first rebuild occupancy
here; common range services have already reused scratch grids.

PerformAiTargetingVisualAct reads that old ID's **current** X/Y, chooses a
display facing from actor-to-target displacement (vertical wins equal
absolute displacement), updates the entity, moves/focuses the cursor, waits
for cursor/view, opens mini-status, requests Sleep15 and SFX_VALIDATION67.
This focus can disagree with the ordinal ray or refer to a now-dead/off-line
target. It neither fixes the final hit list nor proves the services completed.

If action6 remains after those returns, the caller bypasses Egress, Angel
Wing, Stay and physical-only Kiwi conversion, requests two WaitForVInt calls
and calls WriteBattlesceneScript. Debug action selection can still replace
the action there. Scene initialization/execution/end and LoadBattle follow
only if earlier services return. LoadBattle receives itemOrSpellIndex,
retaining the predecessor's view/alias boundary; it does not turn that ID
into a unique damage target or prove successful reload/control.

## Two Guard rays and live target reconstruction

**Confirmed:** `battle/getlaserfacing.asm:GetLaserFacing` (`$1AC8A0..$1AC9B8`)
selects CURRENT_BATTLE43 through `data/battles/global/laserbattles.asm`
(`$1AC9B8..$1AC9FC`), then indexes `(actor byte & $7F)`. The sixteen-byte
43 table is FF except ordinal4=Right0 and ordinal7=Left2. It saves/restores
d0–a6; its outputs are RAM writes, not a returned facing register. Each call
reads **current** actor X/Y. Neither stored mapsprite facing nor initial
coordinates restrict later gameplay positions.

| Guard / ordinal | Initial origin | Direction | First → last ray square | Count | Fresh enemies on ray, spatial order |
| --- | --- | --- | --- | --- | --- |
|132 /4|7,9|Right0|8,9 →47,9|40|131 Horseman at8,9;137 Arch Demon at19,9|
|135 /7|19,7|Left2|18,7 →0,7|19|143 Blue Shaman at12,7|

The complete twelve fresh formation positions miss both rays. Preparation's
BuildTargetsArrayWithAllies therefore conditionally selects Stay, despite
these enemies. An eligible ally makes action6 with its first near-to-far
target in offset2; offset4 is not assigned. Both decisions terminate the
move string with FF and draw no decision RNG. Fresh geometry is an input,
not a prediction of the first reached Guard turn.

`battle/battleactions/battleactionsengine_1.asm:WriteBattlesceneScript`
(`$9B92..$9EC4`) clears EXP/gold, attack-type and transient toggles, then
DetermineTargetsByAction's6 branch calls BuildTargetsArrayWithAllCombatants,
writes FF to TARGETS_LIST[0], reloads the scene actor byte and calls
GetLaserFacing. It reads neither action offset2 nor offset4 for this list.
SortTargets then runs even for an empty result.

`battle/battlefield/buildtargetsarray.asm` (`$C7F2..$C87A`) clears2304
occupancy bytes atFF5600, inserts ally0..29 ascending, then enemy128..159
ascending **without clearing again**. Each insertion requires HP.w≠0,
NEUTRAL bit3 clear and unsigned sign-extended X/Y<48. Negative bytes and
coordinates48..127 fail; HP8000 passes this nonzero test. Membership,
PRIMARY, class, terrain and battle crop are not filters. Eligible later IDs
overwrite earlier same-square occupants; an ineligible later slot does not.
Elis159 is excluded by fresh HP0 and neutral0008, not by logical alias31.
Zeon's physical47 is not a second occupancy record for logical128.

The valid-direction ray clears the2304-byte grids atFF4400/FF4D00, marks
each ray square0, reads occupancy and appends every non-FF ID toFFB652;
length.w is stored atFFB63E. Storage is48×48 with address base+48*y+x.
It ignores terrain/walls, ordinary spell range/radius and28×38 battle crop,
penetrates occupants and excludes the origin for interior starts. Bounds
are tested **after** processing a square: outward-edge Right atx47 touches
x48 once; Left atx0 touches−1 once. This is not a safely clipped edge API;
arbitrary out-of-grid memory effects and natural edge reach remain Unknown.
Unmatched battle or facingFF sets length0 without rebuilding grids/bytes;
there is no table-count guard for arbitrary ordinals.

`battle/battleactions/sorttargets.asm` (`$A9CC..$AA98`) orders unsigned
IDs, so allies precede enemies regardless of distance. The shared Burst Rock
sort-marker/HP pass remains as documented in [actions](./battle-actions.md#items-and-special-gates);
selected43 has no Burst Rock. The following supplied controls retain all
sixteen declared living, nonneutral enemies at source positions unless stated;
all unspecified ally slots are absent. These are static deductions.

| Changed valid inputs | Ally preparation | Rebuilt ray → sorted damage list |
| --- | --- | --- |
|132; ally9 at10,9 and ally2 at15,9|[9,2], action6, focus9|[131,9,2,137] →[2,9,131,137]|
|135; ally9 at18,7 and ally2 at10,7|[9,2], action6, focus9|[9,143,2] →[2,9,143]|
|132; enemies only|[], Stay|No laser script requested by those enemies alone|
|132; only triggering ally9 at10,9 becomes HP0, neutral, X−1, Y−1, X48 or Y48 before decision (each separately)|[], Stay|The same insertion filter applies during reconstruction|
|132; ally9 at10,9, HP8000, neutral clear|[9], action6|[131,9,137] →[9,131,137]; eligibility only, not ordinary HP arithmetic|
|132; ally2 and9 overlap8,9|[9], action6|Horseman131 overwrites both: [131,137]; neither ally is hit|
|Same overlap, but131 HP0 or neutral (separately)|[9], action6|[9,137]; ineligible131 does not erase9|
|132; ally9 at30,9|[9], action6|[131,137,9] →[9,131,137]; beyond crop still included|
|Move132 to7,10; ally9 at8,10|[9], action6|[9]; current origin changed, ordinal direction stays Right|
|Move135 to19,8; ally9 at18,8|[9], action6|[9,130,129] →[9,129,130]; direction stays Left|
|After first132 example's preparation, both allies leave row9 or die|Action6/focus9 already pending|[131,137]; those enemies still take friendly fire|
|Same, and131/137 leave row9 or die before construction|Action6/focus9 pending|[]; no effect/reaction loop, no retroactive Stay|
|135;159 supplied HP20 at18,7 but neutral held|Cannot trigger as enemy|Omitted; with neutral clear159 enters reconstruction, while still unable to trigger preparation|

Zero targets still reaches action message/animation construction before the
length−1 carry exits to End. Property initialization snapshots all62 HP words even for length0,
bypasses ordinary MUDDLE2/same-side classification and attack curse/stun or
cast silence/property setup. InitializeActors initially displays enemy Guard
alone (allyFF). SwitchTargets later selects each target's side, including
enemy friendly fire; generic same-side rejection does not apply here.

## Shared damage, replay and the counter pointer boundary

**Confirmed:** `battle/battleactions/battleactionsengine_2.asm:ApplyActionEffect`
(`$A34E..$A49C` file) reloads BATTLEACTION_PRISM_LASER_POWER=16 into d6.w
for **each** target and calls InflictDamage (`$ACEA..$AE32`), then death
message if targetDies. It does not call Attack/CastSpell, ATT−DEF/terrain,
resistance/spell enhancement, MP cost, learned-spell lookup, physical dodge/critical/
ailment/curse or DetermineDoubleAndCounter. Guard's ENHANCED spell power,
MP150, ATT109, DEMON spell entry and movement5 do not create another laser power or ordinary
spell action. Battle43 leaves the Taros ineffective toggle clear; initial
attack-type0 avoids counter halving. Shared Burst Rock exceptions are not
selected43 targets.

Reuse the [shared derivation](./battle36-prism-flower-laser.md#base-power-rng-hp-and-later-result-interfaces):
two successive GenerateRandomOrDebugNumber calls use range `(16>>3)+1=3`,
subtract0..2 each under ordinary RNG, then enforce minimum1. Thus ordinary
damage is12..16. Seed recurrence is `(13*s+7)&FFFF`; each returned range3
value is `floor(seed*3/65536)`. Both calls retain range3, and **sorted targets
share one continuing stream**. Debug directional priority Right/Up/Left/Down
returns0/1/2/3 without advancing seed; Down can produce10 damage. Debug
enabled without direction uses ordinary draws. Forced debug action/hit
selection is a separate path, not a normal Guard property.

| Supplied construction seed / target HP | Draws / damage / temporary HP |
| --- | --- |
|Seed0000, first sorted target HP20|0,0; seed0062; damage16; HP4|
|Continue0062, next sorted target HP20|0,0; seed4114; damage16; HP4|
|Seed1234, target HP10|2,0; seed04B6; damage14; HP0; reaction−14 and message amount14|
|Continue04B6, next target HP20|0,0; seed1C88; damage16; HP4|
|Held debug Down, seed1234, HP20|3,3; seed unchanged; damage10; HP10|

For ordinary HP≤maxHP<8000, temporary HP is max(0,HP−damage); malformed
signed-high HP retains native word subtraction/clamp semantics. For each
target, InflictDamage emits executeAllyReaction or executeEnemyReaction with
HP delta−damage, MP0, current status and flags1, then damage message285.
Calculated amount is not capped to remaining HP. Lethal result sets targetDies
and emits291 for ally or290 for enemy. Enemy actor early returns in
CalculateDamageExp/AddExpAndGoldForKill and DropEnemyItem, so neither friendly
fire nor a death message proves ally-earned EXP/gold/item drop. BreakUsedItem
ignores action6. No ordinary double/counter toggle was set by this action.

The counter restriction must be read from the **caller's pointers**, not the
comments in `battle/battleactions/isabletocounterattack.asm` (`$A49C..$A54E`):
before ValidateCounterAttack, WriteBattlesceneScript sets a4=TARGETS_LIST and
a5=BATTLESCENE_ACTOR. Therefore a4 is the original victim/prospective
counterattacker; a5 is the original attacker/prospective counter target.
The Taros test uses **a5**; Burst Rock/Kraken Head/Prism Flower/**Zeon Guard**
tests use **a4**. The latter prevents a Guard victim from counterattacking,
not a general immunity to counters against a Guard attacker. For an ordinary
Guard laser counterAttack already0 takes @ClearCounterToggle before these
kind checks. Sleep/Stun are tested on the prospective counterattacker as well.
Both validators finally apply debugDouble/debugCounter **after** their clear
branches; forced debug can override ordinary restrictions. No broader debug
follow-up simulation is performed here.

End restores the saved62 HP words and terminates the script; it does not
restore RNG. The constructed loss is not persistent HP yet.
`battle/battlescenes/battlesceneengine_0.asm:bsc0B_executeAllyReaction` /
`bsc0A_executeEnemyReaction` later consume HP/MP/status/flags, call the shared
clamped HP routine and refresh stats; the [accepted generic replay evidence](./runtime-rng-and-battle-math.md#confirmed-statically-and-at-runtime-physical-base-damage)
does not constitute a Guard observation. Death-list and subsequent
ProcessKilledCombatants/statistics, CountRemainingCombatants and after-turn
interfaces retain the [loop owner](./battle-loop.md#top-level-battle-control).
Full damage construction does not prove completed replay, cleanup, a round,
Battle43 outcome or save persistence. That battle-outcome boundary is next.

## Guard message and sprite-dependent presentation selection

**Confirmed (requests only):** DisplayActionMessage in
`battle/battleactions/createbattlescenemessage.asm` uses enemy kind38 to
select MESSAGE_BATTLE_DEMON_LASER321, rather than flower319. Relevant
`data/scripting/text/gamescript.txt` lines retain only IDs, author paraphrases
and ordered control tokens:

| ID | Role / author paraphrase | Ordered tokens |
| --- | --- | --- |
|321|Guard announces demon laser|none|
|285|Enemy damage amount, either target side|`{NAME}{N}{#}`|
|291|Ally exhausted|`{NAME}{N}`|
|290|Enemy defeated|`{NAME}{N}`|

The [message command owner](./battle-actions.md#battle-scene-message-commands)
owns six-word displayMessage$10 and wait limits. Action6 constructs no
spell-cost message or MP-cost reaction.

GetSpellanimation selects `$15` with enemy mirrored bit7→`$95`;
PerformAnimation keeps attack type0 and AnimateAction emits animateEnemyAction.
`data/stats/enemies/enemybattlesprites.asm` maps **kind38 to PRISM_FLOWER
sprite49, palette1**. GetBattlespriteAndPalette and InitializeBattlescene
write that sprite to BATTLESCENE_ENEMYBATTLESPRITE. Consequently
`battle/battlescenes/animation/prismlaser.asm` (`$1B406..$1B488`) takes
the **Prism Flower sprite** branch (property word4=40), not its non-flower
branch (property word2=5). The selector checks sprite, not kind/name/palette;
a supplied changed sprite would change that branch without changing321.

The shared updater (`animation/update/prismlaser.asm`, `$1E2D4..$1E5D0`)
requests SFX_LOADING103 and SFX_FIRING108; these are distinct from cutscene111.
ExecuteIndividualTurn selects ordinary enemy attack music5 for kind38;
kind98 Zeon's separate music is not a Guard property. Setup/updater also
draw RNG, so two damage draws per target do not count an entire animation's
draws. No complete renderer audit, assets, timing, shape, sound completion,
DMA/VInt persistence or original visual result is claimed.

## Current consumers and explicit missing behavior

**Confirmed by actual accepted-code read:** production uses prepared Content;
the [capability owner](../../remake/docs/capability-status.md) remains authoritative.

| Actual consumer | Existing expression / selected43 boundary |
| --- | --- |
|[Encounter reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [scenario assembly](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Pin/region table admits1/2/3, not43. Placement/definition/loadout metadata exists; no43 content/material package or laser-facing/occupancy binding. Predecessor's ENHANCED, HEALER5, neutral/special and derived-stat gaps remain.|
|[SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [composition](../../remake/src/Sf2.Remake.Application/Gameplay/RuleCompositions.cs)|Actual source-orders strategy handles admitted ordinary sets6/7/9/14 and restricted physical loadouts; no kind38 special branch before LEADER commands. Metadata or LASER spell text is not Guard decision support.|
|[Action state](../../remake/src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs), [decision/resolution](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs)|Kinds Stay/Healing/Physical/Item; automatic decision has one optional opposite-faction target. Reactions can carry damage and ordered arrays, but Physical validation binds one opposing pair and≤3 first/second/counter reactions, not same-side ray multi-target semantics.|
|[Advancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs), [movement continuation](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleMovementContinuation.cs), [physical rule](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs)|Decision precedes movement; FinishAutomatic constructs Physical for any target. Physical rejects same side and uses ATT/DEF/terrain/physical rolls. It cannot substitute for action6 base16, ally-only preparation then all-slot rebuild and ID-ordered RNG.|
|[Scene continuation](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs), [commit](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs)|Scene-phase HP/reward/death publications and post-action interfaces exist; no laser producer selects native targets/messages/animation. Generic reaction capacity does not implement a laser.|
|[Godot scene](../../remake/game/src/Battles/BattleSceneView.cs), [board presentation](../../remake/game/src/Battles/BattlePresentation.cs), [viewport](../../remake/game/src/Battles/BattleMapViewport.cs)|Actual consumers display reactions/HP/text/audio. ActionMessage defaults273 outside current cases; generic285/290/291 cannot bind321, selector95, sprite49/palette1, SFX103/108 or battlefield ray. No43 action/control observation exists.|

**Inferred:** future integration needs selected content admission and an
explicit line-action producer carrying live origin/direction, distinct
preparation/rebuild policies, sorted both-side reactions with shared RNG,
then corresponding scene projection. This describes a gap; it authorizes
neither implementation nor an automatic runtime collection queue.

## Reproduction, retained failures and stop

The ignored one-off recipe checks clean pinned source, use-site/order/width
joins, actual roster and changed-state arithmetic, selected message tokens,
actual consumers, UTF8/LF/links and the exact two-path/nav-only/private diff.
It retains small source/consumer snapshots for direct comparison on repeat;
it is not a maintained parser or test of a verification program. Load current
configuration in the **same process**, with a fresh nonexistent receipt:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1180/review.py <configured-source-disasm-root> --receipt local/issue1180/direct-reproduction-01.json --snapshots local/issue1180/analysis-01.json --committed
git diff --check origin/main
uv run sf2 verify plan --base origin/main --head HEAD
```

Source argument ends in disasm. Change the receipt name after use. Exact
head/tree/base, command results, scoped CI including skips, process/output
state and self-review corrections belong to the frozen Issue/PR handoff.
Normal/full/old comparisons, SDK/Godot/emulator/H3/H4 and screenshots are
**SKIPPED** by this documentation-only scope, not passed. ADR0004's evidence,
provenance/semantic/private checklist applies; new parser/schema/fixture
mutation and H3 callback obligations are N/A. One-off guards do not promise
maintained mutation coverage.

Preserve [#1178's retained failures and Unknowns](./battle43-activation-and-first-control.md#reproduction-retained-failures-and-open-boundary):
its direct/CI success stops before execution; #1176's missed ENTNUM32 and
scene declaration62,62 versus later/combat13,6 corrections remain explicit;
#1172 Sleep/Stun/publication, prior geometry/read failures, after42 special251
refusal/five-op partial/14-action eas_Init cache with no published world,
Map54 overwrite, Map53 failed import/zero partial and twelve old completed
comparison failures remain with their named owners. Preserve
[#1132's forced-debug and cross-faction boundaries](./battle36-prism-flower-laser.md#reproduction-preserved-results-and-unknowns).
No prior direct recipe, lowering or completed suite is rerun or reclassified.

**Unknown:** natural43 reach and Guard turn, actual party/HP/status/control,
positions/overlap/neutral, incoming seed/debug input, intermediate service
mutation/return, movement/focus/view/scene/reload completion, animation/audio
and hardware timing, persistent playback/save, original-ROM equality and
original/remake playability. Supplied valid states establish conditional
logic only; they do not close these questions. Stop at the action/result
and general post-action interface, before Battle43 outcomes/ending.

Freeze a clean committed/pushed Draft PR, record exact scoped CI and handoff,
move Project to Review / Verify and notify main-gate once. Independent review
and merge remain main-gate-owned; stop writer, with corrections in this task.
No merge, next Issue, environment/ref cleanup or archive is authorized.
