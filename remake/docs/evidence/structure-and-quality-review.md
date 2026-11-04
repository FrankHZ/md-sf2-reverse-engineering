# Remake Structure and Quality Review

## Finite pre-voyage audit

**Current disposition: F4/F5 are implemented and locally verified under
[#1536](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1536), pending independent main-gate
acceptance and integration.** Continue bounded development; no P1 or whole-engine redesign was
identified. The audit-base failures below remain historical evidence. This is not acceptance of the
entire pre-departure campaign. The [boundary review](engine-script-boundary-review.md#finite-pre-voyage-audit)
owns the common scope, Standard rigor, accepted diff/base/tree, dependency map and continuation limits.
Existing High-rigor service/save contracts remain unchanged. This report owns the findings, copy
design and sole new experiment for [#1528](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1528).

All paths and line numbers in F4/F5 refer to accepted **`7fcd630d7474586e3b91aaa69889ca7e294e6007`**
(tree `b6d8b2f22e2a0b65f18d5c2d6718f1f2df1e9a3c`); the earlier review below retains its own base.
**Confirmed** denotes inspected source/assertions or the explicitly executed API result;
**Inferred** denotes an unexecuted consequence; **Unknown** denotes an unobserved boundary.

### F4/F5 bounded repair disposition

**Standard rigor:** two finite engine admission/preservation repairs within the existing architecture;
service/save High-rigor boundaries and the finite campaign scope are unchanged. Production ownership
is `BattleStartInput`, `BattleActionRules`, `Sf2OutcomeReturnPolicy`, `OutcomeReturnRules`, and the
same-lifetime `JoinResources`/`ClearOutcomePartyAi` consumers. Tests and these two existing audit owners
complete the scope. Fresh start, battle-to-field rebinding, save format, action effects/payment,
original runtime evidence and later campaign work are outside this repair.

**Confirmed:** `BattleStartInput.WithResources` sits beside `WithSeeds` and replaces only actors/gold,
retaining entry policy, active membership, custody, flags, visuals, retained slots and death credit.
Recovery and defeat leader/gold edits use it. JoinResources appends only the newly admitted actor;
AI clear changes only that actor's activation bit. These remain the same carried-party lifetime.
`BattleOutcome` and `BattleEntry` retain their explicit lifecycle rebindings. `OutcomeReturnRules.Party`
now rejects recovery policies that alter unowned flags, visuals, retained slots or credit before
publication. Nonzero recovery status still reports `return-status-refresh` atomically.

**Confirmed:** preparation requires a target for Healing, AttackSpell and SupportSpell after action
admission and before silence suppression, using the existing missing-target codes. Supplied-target
validation still runs first; silenced valid spells retain their paid no-effect resolution. Stay,
item and EGRESS behavior keeps its existing admission and payment path.

Actual behavior acceptance lives in
[`PartyRecoveryContextTests`](../../tests/Sf2.Remake.Engine.Tests/PartyRecoveryContextTests.cs) and
[`SilenceSpellTests`](../../tests/Sf2.Remake.Engine.Tests/SilenceSpellTests.cs). It covers the real reset
program through poison-enabled entry with changed seeds/gold/HP/MP/accounting/credit, reordered smaller
parties, dead ordinary/immortal members, nondefault retained context, joining/retry, AI clear, defeat
recovery, refused status and unowned policy writes. The spell matrix covers all three kinds with
normal/silenced caster and null/valid/invalid target, without live resource/RNG publication on refusal.
Existing Session/AI, item, EGRESS and outcome behavior is selected directly alongside these tests.
After the [locked environment setup](../development-and-verification.md#locked-net-workflow), from
`remake/`, reproduce the bounded new behavior with:

```powershell
& $env:DOTNET_BIN restore tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj --locked-mode
& $env:DOTNET_BIN test tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj --configuration Debug --no-restore --filter 'FullyQualifiedName~PartyRecoveryContextTests|FullyQualifiedName~SilenceSpellTests'
```

The unchanged public F4 recipe below was rerun against freshly built corrected Debug assemblies:
**both control and reset variants reach Battle / PlayerInput with `medic-a` credit and no failure**.
Before the F5 correction, the three new silenced/null-target cases each failed because no exception
was thrown; normal and Stay controls passed. After correction the matrix passes. Prior F4 refusal,
initial launcher failures and all retained aggregate failures are not replaced by these results.
No native launch, original acquisition, screenshot or new comparison infrastructure is needed.
ObjectDB exit cleanup, original natural reach/timing and wider campaign capabilities remain Unknown
at their existing owners. Exact local/CI results and the candidate Git objects belong to the PR handoff.

The F4/F5 sections below describe the accepted audit base, including its then-open repair proposals;
this disposition owns current implementation status.

### F4 — P2: retain carried context when recovering party resources

**Confirmed at the audit base: reachable defect in changed authored content.**
[`Sf2OutcomeReturnPolicy.Heal:88`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/7fcd630d7474586e3b91aaa69889ca7e294e6007/remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs#L88)
reconstructs `BattleStartInput` only through `ItemCustody`, silently defaulting `StoryFlags`,
`Visuals`, `RetainedSourceSlots` and **`DeathCredit`**. `ProgramRunner:163–164` calls that policy for
the admitted `reset-party-battle-stats` instruction; this is not limited to a battle outcome.
`OutcomeReturnRules.Party` in `IOutcomeReturnPolicy.cs:67–87` validates resources, seeds, membership
and custody but omits those carried fields, so it accepts this loss. `BattleEntry:81–85` correctly
forwards the resulting credit; `BattleTurnFlow:71` then requires it for the connected poison profile.

**Trigger and observed impact:** take tracked `remake/content/authored/shop-supplies.json`, set all
`battle.start.actors[*].status` to `none` in both variants (recovery's existing status refusal is not
under test), and prepend `{ "op": "reset-party-battle-stats" }` to `world.programs[id=before-battle].instructions`
in only the second variant. Start through public `AuthoredScenarioPackageReader`/`GameSession.Start`,
walk South/East/East/East/North, and service ordinary simulation/dialogue waits with current envelopes.
Both starts retain initial credit `medic-a`. The control reaches **Battle / PlayerInput**, credit
`medic-a`, flags13/14/15/16/20, no failure. The reset variant reaches **Exploration / Unsupported**,
credit null, flags13/14/15, and `UnsupportedCapability / poison-death-context / start.firstAlly`.
No battle is published; the prior recovery/program prefix remains. This is a rejected valid
composition, not observed corruption of a save or a claim about a natural original route.

**Origin:** the incomplete recovery copy predates this batch (same reconstruction at `9a99132a`).
#1527 admits the connected poison/supply composition and repairs the exercised field RNG/entry copies,
making this remaining sibling relevant now. Do not describe the already corrected movement/text/
portrait/entry paths as broken. `ProgramRunner.JoinResources:549` and
`OutcomePartyOperations.ClearOutcomePartyAi:30` also stop copying at `ItemCustody`; they confirm the
same maintenance hazard. Their downstream failure combinations were **not executed** and are not
additional reproduced findings. Outcome-to-field reconstruction has different initialization semantics;
do not blindly carry battle-only context across it.

**Smallest design:** extend the existing `BattleStartInput` immutable replacement owner, alongside
`WithSeeds`, with a narrow actor/resource replacement operation that preserves the other carried
fields. Use it for recovery and the directly related actor-only copies after verifying their intended
lifetime. Keep legitimate fresh battle/restore construction explicit. Extend `OutcomeReturnRules.Party`
to enforce preservation of fields the recovery policy does not own. Do not invent a generic clone
service, reflection copier, capability bag or second roster. An immediate explicit argument fix is
possible, but repeated omissions in #1527 and this execution justify one local preservation owner.

**Scope and acceptance:** Domain `BattleStartInput`, Application recovery policy/validator and the
named carried-party copies; keep status recovery, outcome rebinding and save format unchanged.
Repair before depending on reset/recovery plus connected poison entry or expanding those copying
callers. Unrelated rules/UI work can continue. A small engine behavior assertion should run the
actual admitted reset program through entry, vary retained accounting/resources, and verify one
recovery with preserved seeds/custody/context and unchanged invalid-status refusal. Use the concrete
counterexample below; do not test constructor parameter counts or add a copier test framework.

### F5 — P2: require a target before the silenced-cast shortcut

**Confirmed source invariant gap; Inferred risk to future callers, no current ordinary-game failure
established.** In
[`BattleActionRules.Prepare:45–56`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/7fcd630d7474586e3b91aaa69889ca7e294e6007/remake/src/Sf2.Remake.Domain/Battles/Rules/IBattleActionRule.cs#L45),
`RequireTarget` runs only when the nullable target is supplied. `SilenceRules.Suppress:28–34` then
returns a zero-reaction resolution, bypassing the concrete spell's missing-target check.
`BattleActionResolution.Validate:115–121` accepts that suppressed shape and never requires the target.
Thus a valid silenced caster/action with `target: null` is accepted internally where the equivalent
unsilenced targeted spell rejects. This was introduced by #1526's shortcut and carried into this
audit by its [main-gate comment](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1528#issuecomment-6640797017).
It was source-traced here, not rerun as another experiment.

**Reachability and impact:** `BattleCommandDispatcher.Select` puts targeted actions in TargetChoice;
only `SelectTarget` reaches CommitReady after `BattleChoices.RequireTarget`. Automatic chosen spells
require a nonnull target in `BattleAutomaticAction.Validate` and carry it through movement completion.
These callers prevent the missing-target case today. The latent result would permit a future internal
caller to start a paid no-effect action without its required selection; it does not demonstrate a
current UI bypass, missing MP check, invalid supplied-target bypass or double payment.

**Smallest design:** before suppression, explicitly require a target for the finite affected targeted
kinds (Healing, AttackSpell, SupportSpell), retaining their existing error category and supplied-target
validation. Keep Stay and source EGRESS/item semantics distinct. No generic validation platform or
target-schema rewrite is needed. Scope the shared preparation owner and its actual behavior test;
include null versus valid target under silenced/unsilenced states and unchanged no-publication failure.
Repair before introducing another preparation caller; it does not block current admitted Session/AI
work. This audit does not implement the correction.

### Copy design, responsibilities and accepted boundaries

**Confirmed:** F3's narrow `BattleDefinition.WithOutcome/WithDeployments` now funnels immutable
replacement through one definition-owned copy, retaining current attack/support/healing/item/equipment
capabilities. `BattleActorDefinition.WithGrowth/WithEquipment` retains resistance. The actual connected
growth/drop/outcome/walking-return assertion in `AttackSpellAdmissionTests` exercises the affected
behavior. `PrivateFieldPreparation` reuses the same deployment replacement. This is an appropriate
repair at the observed owner, not proof that every future added constructor field is safe.

`BattleStartInput.WithSeeds` likewise fixes the seed-only copies in entity/text/portrait/random-battle
work; the supply session test traverses those seams. F4 demonstrates why these local remedies do not
establish whole-graph copy completeness. Keep definitions, live carried party and fresh starts distinct:
their lifetimes differ. A future field addition needs the owning replacement and its real consumer
reviewed together, without a broad repository constructor migration.

The inspected project references still point inward: Application→Domain, Content→Application/Domain,
Godot→those assemblies; no reference-data/test/tool dependency was added. New attack/support/AI/shop
files have coherent responsibilities. Similar healing/damage/support action branches reflect distinct
target/reward/lifecycle rules; merging them into a universal effect pipeline is not justified by this
diff. Shop selection and existing Church selection share tokens/commit/inventory mechanisms while
owning different transactions; no additional service framework is warranted.

A direct physical-line check found every changed handwritten `.cs`, `.py` and `.gd` file below1,000
lines at this base. The source program compiler is close to the limit and the exploration view remains
large; the next touched growth should extract its concrete responsibility under the existing size
rule. Size alone is not an additional finding or evidence of product completion. No fresh census of
unrelated files, performance benchmark or entire historical subsystem audit was performed.

The DETOX fairy dependency and enemy HEAL cure alias are **corrected**, with the current trace and
assertions in the [boundary assessment](engine-script-boundary-review.md#accepted-corrections-and-explicit-continuation-limits).
Shop confirmation keeps gold/inventory atomic and stale input free; completed trades survive later
caller failure. Existing save authority captures completed resources only at admitted stable boundaries;
no Map16 save eligibility, in-progress shop serialization or new save schema is claimed. These good
boundaries can remain as they are.

### F4 direct API reproduction and validation limits

The paired experiment reused the retained #1527 Debug Domain/Application/Content assemblies, after
confirming its corrected candidate `959cacbc` and accepted `7fcd630d` have the same production tree.
The retained build/native records identify that build; this audit did not build it again. Run the
following in PowerShell7 from a script with an explicit checkout parameter, using the established
environment's already-built assemblies. It reads only a public fixture and writes no input or gameplay
state directly. New output may be redirected to a worktree-local ignored destination.

```powershell
using namespace Sf2.Remake.Application.Runtime
using namespace Sf2.Remake.Application.Runtime.Exploration
using namespace Sf2.Remake.Content.Scenarios
using namespace Sf2.Remake.Domain.Maps
param([Parameter(Mandatory)][string]$RepositoryRoot)
$ErrorActionPreference = 'Stop'
$auditRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path
foreach ($assembly in @('Domain','Application','Content')) {
    Add-Type -Path (Join-Path $auditRoot "remake/src/Sf2.Remake.$assembly/bin/Debug/net8.0/Sf2.Remake.$assembly.dll")
}
foreach ($reset in @($false,$true)) {
    $d = Get-Content -LiteralPath (Join-Path $auditRoot 'remake/content/authored/shop-supplies.json') -Raw | ConvertFrom-Json -AsHashtable
    foreach ($actor in $d.battle.start.actors) { $actor.status = 'none' }
    if ($reset) {
        $program = $d.world.programs | Where-Object id -eq 'before-battle'
        $program.instructions = @(@{op='reset-party-battle-stats'}) + $program.instructions
    }
    $bytes = [Text.Encoding]::UTF8.GetBytes(($d | ConvertTo-Json -Depth 80))
    $started = [GameSession]::Start([AuthoredScenarioPackageReader]::FromDocumentBytes($bytes))
    if ($started -isnot [SessionStarted]) { throw "Admission failed: $started" }
    $s = $started.Session; $failure = $null
    foreach ($direction in @('South','East','East','East','North')) {
        $c = $s.Current
        $r = $s.Submit([CommandEnvelope]::new($c.SessionId,$c.Revision,$null,[Move]::new([ExplorationDirection]$direction)))
        if ($r.Failure) { $failure = $r.Failure; break }
        for ($tick=0; $tick -lt 400 -and $s.Current.Mode -eq 'Exploration' -and -not $s.Current.CanWaitAtInput; $tick++) {
            $c = $s.Current
            $command = if ($c.Story.Wait -is [DialogueWait]) { [Acknowledge]::new($c.Story.Wait.Token) }
                else { [AdvanceSimulation]::new($c.Story.Wait.Token,1) }
            $r = $s.Submit([CommandEnvelope]::new($c.SessionId,$c.Revision,$null,$command))
            if ($r.Failure) { $failure = $r.Failure; break }
        }
        if ($failure) { break }
    }
    [pscustomobject]@{ Reset=$reset; Mode=$s.Current.Mode; Stop=$s.Current.StopReason; Failure=$failure;
        Credit=if ($s.Current.Mode -eq 'Battle') { $s.Current.Battle.DeathCredit } else { $s.Current.Exploration.Party.DeathCredit } }
}
```

Observed control **PASS at battle entry**; changed-content case **FAIL with the exact F4 refusal**,
not an interrupted run. The first launcher attempt omitted the Maps namespace and stopped before
movement; the corrected paired run above completed. The mode, failure, flags and credit in local
`local/issue1528/reset-result.json` retain the outcomes; the recipe and result above are the durable
reproduction, independent of that ignored report.
No repeated engine suite, new native launch, screenshot, new comparator or original-game collection
was performed. F5 uses source and the prior independent observation only. Existing failures and
Unknowns remain with their owners; current document/plan/CI/freeze status belongs in the handoff.

## Earlier structural review and dispositions

The following sections preserve the earlier audit's evidence, failures and bounded implementation
dispositions. Their original acceptance-stage wording is historical; the finite audit above owns
the current continuation decision. No historical repair queue is restarted by this report.

## Decision and scope

Retain the four-assembly architecture. Fix **F1's host lifecycle defect before work relying on initial
battle appearances, region programs or direct battle-to-world return**. F2's bounded typed admission
repair is implemented; see its disposition below. Address **F3's definition-copy risk with the first connected
capability extension that needs it**, before relaxing existing admission restrictions. None warrants a
general engine rewrite, generic capability registry or new verification platform.
F1 now has a [bounded implementation disposition](#f1--p1-make-host-ownership-and-return-wiring-cover-every-admitted-entry);
the audit-base evidence below remains preserved. F2 is implemented pending independent integration; F3 has the bounded attack-spell disposition below.

This is the deliverable for [Issue #1373](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1373).
**Standard rigor:** established architecture with concrete cross-layer composition risks. The outcome is
an independently checkable decision about safe continuation and bounded repairs, not full-game
acceptance. Tracked ownership is this review and its [architecture](../architecture.md) navigation link.
Production, tests, research, UI and agent guidance are read-only. The inspected accepted source is
**`0514cb7ff24a58657160f1b6d04891084713eb4f`**, tree `151da340f4f20b868f7396e6466c587265f24001`.
There was no delta from the Issue's accepted base. All source lines and symbols below refer to that
commit, not future versions of the linked files.

Acceptance: responsibility/state map, justified deeper selections, actionable findings with concrete
evidence, explicit no-action areas/limits, direct document/private-boundary checks and clean committed
dependency plan. Repairs and new tickets require separate scope. The only other open PR at ownership
check was frozen [PR #692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692); it overlaps neither
owned path. Its UI decisions and unmerged implementation were not consumed.

**Confirmed** means directly inspected source or the named execution. **Inferred** denotes a consequence
not independently observed at that boundary. **Unknown** retains unresolved questions, without creating
an observation queue. No original-game fact is inferred from remake code. The [six-family owner](../domain/special-mechanics-design.md#current-closure-scope)
and [capability ledger](../capability-status.md) still distinguish authored capability from chapter
composition. #638 remains paused; #1047's twelve unresolved comparisons and all prior failures are
unchanged. Main-gate owns independent acceptance and integration.

## Dependency, responsibility and state map

The production project files confirm inward dependencies and no Reference/test/verification-project
dependency. This map describes inspected responsibilities, not a claim that every line was audited.

| Layer | Actual owners and responsibilities | Authority and dependencies |
| --- | --- | --- |
| Domain | [Definitions/state](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs), rules, numerical RNG, movement, activation, party membership and entity motion | Base .NET only. Reducers compute candidates. Battle state carries resources, main/thinking seeds, queue, death credit and custody. Inspected definition/start constructors copy collections. |
| Application | [GameSession](../../src/Sf2.Remake.Application/Runtime/GameSession.cs), dispatchers, scene/round/program/outcome continuation, Content ports and observations | Domain dependency. Private `_current` is published by the session; collaborators return snapshots. `ActiveBattle`/`ActiveExploration` separate modes; `StoryState` carries flags, cursor, stack and waits. |
| Content | [Authored reader](../../src/Sf2.Remake.Content/Scenarios/AuthoredScenarioPackageReader.cs), [world reader](../../src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), private provenance/encounter readers and finite capability readers | Application/Domain dependencies. Bytes are admitted once into definitions/starts, not reread during play. Public and private admission remain distinct. F2 affects the typed failure boundary. |
| Godot | [GameRoot](../../game/src/GameRoot.cs), semantic input, battle/field views, audio, resources and presentation completion | Inward dependencies plus Godot. Views retain UI candidates and delivery progress, not gameplay truth. F1 exposes incomplete ownership selection and callback setup. |

The inspected command path is device input → semantic view command → `GameSession.Submit` → selected
rule/continuation → snapshot/observations → view → matching completion. Session/revision/actor checks
precede rule invocation. `BattleChoices.Query` and selection do not publish an action;
`BattleCommandDispatcher.Commit` rechecks the item word and prepares against live state. The no-scene
committer and scene/Give/retreat/round paths retain their distinct mutation and completion boundaries.

`SessionRules` binds before start. Action, decision, progression, outcome-return and story interfaces
cross real authority boundaries; fixed services remain explicit. Internal construction access through
`InternalsVisibleTo` means references alone cannot prove semantic separation, so actual caller paths
were also inspected. No additional public protocol is justified merely to shorten a helper.

### Deeper review selection

Recent work adds optional definition fields and pending substates. The review therefore traced:

- Definition reconstruction in `BattleGrowthReader.BindRows`, `ExplorationContentReader.Read` and
  `ExplorationMapDefinition.SelectSetup`, with immutable ownership and admission restrictions.
- Session dispatch precedence, reset/program/appearance ordering, action construction, delivered
  reaction/death/accounting and outcome return: these share commits, resources and RNG.
- Godot startup and field-to-battle ownership, then appearance/story/scene/retreat consumers.
- Existing assertions for changed actors/content and later failures, to assess testability without
  building another verification system.

A census of production C# under `remake/src` and `remake/game/src` found no file at or above 1,000
physical lines. Selected hotspots: `ExplorationSessionView` 833, `ExplorationPresentation` 759,
`ExplorationContentReader` 750, `ProgramRunner` 559, `ExplorationDispatcher` 537, `BattleSessionView` 525,
`BattleSceneContinuation` 511. These are audit-base observations, not maintained targets. Size prompted
inspection, not a defect verdict. The field view combines input/projection/observations; the world
reader combines program/map/cross-reference admission. Extract a coherent touched responsibility when
a real change needs it; file splitting alone would not fix F1/F3.

## Findings

### F1 — P1: make host ownership and return wiring cover every admitted entry

**Current disposition — Confirmed bounded correction, Issue #1375:** the ordinary host shares one
mode/entry-continuation ownership decision and installs entry/return callbacks before attachment.
Eight direct Godot witnesses passed: initial ordered appearances; initial region program with two
real text acknowledgements; connected first-round appearance; direct battle win and same-session
field movement; initial field-owned battle load plus start; ordinary player/automatic continuation;
terminal defeat; and visible stopped-program failure. Load/start waits retain field ownership even
with `ActiveBattle`. No Domain/Application/Content behavior changed. See the
[reproducible variants, commands and retained failure history](../development-and-verification.md#ordinary-host-lifecycle-observation).
Independent review/integration remains required. F2 has the bounded disposition below; F3 has the bounded attack-spell disposition below.

The following **audit-base** observations and Inferred limits describe the inspected commit above;
they are not relabeled by the new authored observations. The connected/native return boundaries now
have their separately named bounded witnesses, without establishing private/original-game reach.

**Confirmed startup defect.** [GameRoot.cs:111](../../game/src/GameRoot.cs#L111), `_Ready.BeginSource`,
attaches the battle view only for `HasBattleControl`, `BattleScene` or `BattleMovement`. The same
incomplete condition appears in [ExplorationSessionView.cs:386](../../game/src/Exploration/ExplorationSessionView.cs#L386),
`Send`. First-round appearance and battle-region program states can have `Mode=Battle` with none of
those three values.

A public fixture reproduction changed only `hidden-spawns.json`'s leader start position to `(2,3)`,
inside its existing trigger. `GameSession.Start` returned `SessionStarted`, no failure, `Mode=Battle`,
round 0, appearances `[returner, hidden]`, no scene/movement/control and no exploration definition.
Godot 4.7.2 Debug selected `ShowExploration` and logged `NullReferenceException` in
`ExplorationSessionView.Begin:150` (`audio.Observe(result)`), from `GameRoot:114/127`. The four-frame
process completed with exit **0**; this is **FAIL** because of the error. The unmodified package's same
four-frame startup had no errors. This establishes startup behavior, not complete rendering/gameplay.

The engine path is `BattleAdvancer.Start/AdvanceCore` → `BattleRoundContinuation.Begin/Populate` →
pending `BattleRoundState` before queue generation. `BattleSessionView._Process:234–245` already has
appearance/program consumers; routing strands them. `BattleRegionProgramTests` also asserts an initial
battle-program wait. The assertions were inspected, not rerun.

**Inferred connected consequence:** a field transition to the same first-round state misses the field
view's handoff. `HiddenSpawnSessionTests.OrdinaryLeaderTerminationAndWalkingRetryResetDormancyButKeepStoryAndItemCustody`
constructs that connected state through engine commands, but directly drives completion rather than
Godot routing. No native connected freeze was observed here; it remains a concrete dependent risk.

**Confirmed adjacent setup defect; return symptom Inferred.** `GameRoot.BeginSource:111–115` returns
from the initial battle `Attach` before assigning `view.LeaveBattle`. This branch is reachable for
admitted connected content: start `battle-drops.json` on `arena`, include unlock flag `401`, and replace
only the `before` program's instructions with `[{"op":"end"}]`. The executed public start returned
`SessionStarted`, no failure, battle control for `leader`, an exploration definition and an outcome
route, with no story wait. A preliminary variant without flag `401` correctly remained in exploration;
that was not a reproduction of the defect.

The later source consumer [BattleSessionView.Send:366](../../game/src/Battles/BattleSessionView.cs#L366)
invokes `LeaveBattle?.Invoke` and returns on exploration mode. The skipped callback assignment means
no field view is created after an outcome on this startup path. A complete native victory from this
variant was not run, so the stranded return is an inference from confirmed reachable setup and the
callback consumer, not a newly observed visual failure. It is the same lifecycle-setup repair scope.

**Smallest maintainable remedy:** wire both direction callbacks before any attach/return branch, and
centralize the host presentation-ownership decision in one internal adapter helper based on existing
mode and story/entry continuation. Include pending battle states and terminal/failure display.
Preserve field-owned pre-battle load/start programs: `BattleEntry.Continue` publishes `ActiveBattle`
before its load program completes, so unconditional `Mode == Battle` is insufficient. No new gameplay
state, registry or event bus is needed. Adding `BattleRound` to two lists leaves the omission pattern;
changing fixtures or auto-completing appearances would hide it.

**Focused acceptance:** ordinary-host initial appearance, initial region text wait, connected first-round
appearance, and direct battle start through victory/usable field return. Read actual view ownership,
tokens, completion, queue generation and errors. Preserve pre-battle load/start ownership, ordinary
player/automatic starts, outcome return and failure display. Use existing observation seams without
screenshots or state injection; engine tests only if engine behavior changes. Coordinate GameRoot,
field view and any shared helper with the UI owner; no UI product redesign is included.

### F2 — P2: validate JSON object shape before optional-field discovery

**Confirmed defect.** [AuthoredScenarioPackageReader.DecodeBattle:41](../../src/Sf2.Remake.Content/Scenarios/AuthoredScenarioPackageReader.cs#L41)
evaluates `root.TryGetProperty(...)` while constructing arguments for `ObjectOptional`; the `start`
argument construction repeats it at lines 57–59. Evaluation precedes
[ScenarioJson.ObjectOptional](../../src/Sf2.Remake.Content/Scenarios/ScenarioJson.cs#L16)'s `ValueKind`
check. `Read` catches admission, rule, JSON syntax and I/O errors but not this `InvalidOperationException`.

Direct execution of the public `FromDocumentBytes(...).Read()` API produced:

| Input | Observed result |
| --- | --- |
| `[]` | Uncaught `InvalidOperationException`, Object required, Array received |
| `null` | Uncaught `InvalidOperationException`, Object required, Null received |
| `{}` | `ScenarioReadRejected`, code `missing-field` |
| Valid `hidden-spawns.json`, `start` replaced by `[]` | Uncaught `InvalidOperationException` |

These tiny inputs are syntactically valid JSON. Invalid content is not accepted, but the typed admission
result is bypassed. `GameSession.Start(IScenarioSource)` calls `source.Read()` outside its battle-start
catches; `GameRoot.BeginSource` expects the typed result. The API escape is reproduced; a second native
malformed-input experiment is unnecessary to establish that boundary.

**Smallest maintainable remedy:** check object shape before optional-member queries, using the existing
`ScenarioJson` admission error. Inspect sibling argument-building sites for the same evaluation order
within that bounded repair. An internal optional-key helper is useful only if it removes repeated unsafe
ordering; no second parser/schema framework is needed. Blanket `catch (Exception)` hides programming
faults and field attribution. Catching this exception outside can contain the crash but does not replace
ordered shape validation.

**Focused acceptance:** directly invoke the public reader for the four cases above, another nested
optional-object case and the valid package. Require typed `ContentError` with useful code/field,
unchanged valid admission and unknown/duplicate-field rejection. Preserve private trust ordering. No
tests of the parser harness or new fuzzing platform. Repair before broadening authored input work;
unrelated numerical rule work need not wait.

**Current implementation agreement ([#1377](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1377)):**
Standard rigor for a bounded Content correction: shared optional-field construction has concrete typed
admission escapes. Preserve accepted definitions, strict unknown/duplicate/missing fields, existing
failure kinds and private provenance order. Own only `ScenarioJson`, `AuthoredScenarioPackageReader`,
`ExplorationContentReader` and the concrete `ExplorationAssetReader` portrait sibling, plus this
review, verification recipe and stale capability status. No Application/Domain/host changes, broad
exception catch, parser framework, new helper tests or F3 work. Acceptance is direct public `Read`
and `GameSession.Start`, the committed scope plan and selected existing engine/adapter checks.
Inputs are two small public packages and in-memory mutations; linear byte processing, compact reports
below 1 MiB and minutes of execution. A generalized parser or unrelated semantic defect requires
reassessment; no wider malformed-input campaign or native Godot observation is implied.

**Confirmed implementation and direct API result:** `ScenarioJson.ObjectOptional` now accepts required
and optional key lists, checks object shape first, then discovers present optional keys and retains
`Object`'s exact field validation. Existing single-optional callers delegate to that same check.
The original failures above remain audit evidence. The [direct recipe](../development-and-verification.md#typed-json-object-shape-observation)
produces the following results through both public APIs (`ScenarioReadRejected` / `SessionStartFailed`):

| Input boundary, replaced by `[]` unless specified | Current code / field (`ContentError`) |
| --- | --- |
| Root `[]`, `null`, number, boolean or string; connected `battle=[]` | `object-required` / `document` |
| Standalone `start` | `object-required` / `start` |
| Battle actor / encounter / placement / start actor | `object-required` / `actor`, `encounter`, `placement`, `start.actor` respectively |
| Connected world / start / program / map / area / entity / battle route | `object-required` / `world`, `exploration-start`, `program`, `map`, `area`, `entity`, `battle-route` respectively |
| Connected `world.presentation={maps:[],sprites:[],portraits:[null]}` | `object-required` / `portraitVisual` |
| Root `{}` or missing `profile` | `missing-field` / `document` |
| Root `{` | `json-syntax` / `document` |
| Unknown or duplicate root/start field | `unknown-or-duplicate-field` / `document` or `start` |

`hidden-spawns.json` and `battle-drops.json` still return battle/exploration acceptance and
`SessionStarted`. An unknown `ruleProfile` remains `UnsupportedCapability/rule-profile/ruleProfile`.
No exercised case escapes. The first portrait construction accidentally serialized an empty list;
that observation did not exercise a portrait. The corrected explicit `[null]` counterexample was
reproduced before the fix and is the portrait result above; both local records are retained.

**Sibling inspection boundary:** dynamic optional-key lists were corrected for battle root/start,
actor, encounter, placement and start actor; connected world, program, map, area, start, battle route
and entity; and portrait rows. Encounter weapon discovery queried each encounter before the later
field check, so it now requires object shape at that earlier parent call. Connected start similarly
requires shape before its early `mapPalettes` query; full field validation remains at its existing
position. `show-text` and `present` argument lists use the checked helper too, although successful
opcode discovery already establishes an object at those call sites. Fixed-key `Object` and
single-optional calls already guard before property discovery; terrain legend has an explicit shape
check. Initialization, equipment, drops, laser, cleanup and story readers receive their parent rows
after the owning shape checks and guard their own fixed/optional objects. Private party admission,
root/provenance identity and controlled-start checks still precede shared exploration decoding;
those readers are unchanged. This is not proof for every malformed field or opcode discriminator.

Selected existing checks: engine **PASS** (1,594 passed, 30 optional-private skips, zero failures),
adapter Release build **PASS** (zero warnings/errors). No private payload observation, native Godot,
research/full suite or new parser/probe test was run. Private skips remain unobserved, not PASS.
Independent acceptance and integration remain main-gate-owned; F3 is unchanged.

### F3 — P2: avoid silent defaults when reconstructing definitions

**Implemented bounded disposition:** the [finite player attack-spell integration](../domain/gameplay-rules.md#finite-player-attack-spells)
adds an actually consumed definition field at the named outcome/growth boundary.
`BattleDefinition.WithOutcome`/`WithDeployments` now own immutable replacement; outcome, growth and
opening loadout callers specify only their changed responsibility. Actor growth/equipment copies
also retain resistance. `AttackSpellAdmissionTests.ConnectedOutcomeGrowthDropAndWalkingReturnRetainAttackDefinitionAndLiveAccounting`
executes a supported learned spell with outcome, growth, drop, return and walking re-entry. Existing
connected Give/Discard/poison/equipment restrictions remain intact. No generic capability bag,
reflection copier, new program platform or full-constructor audit is introduced. Independent
integration remains main-gate-owned. The following audit-base diagnosis is retained as provenance.

**Confirmed source asymmetry; Inferred future integration risk, not current capability loss.**
[BattleDefinition:158](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs#L158) has optional
`Give`, `Discard` and `Poison` parameters. [BattleGrowthReader.BindRows:90](../../src/Sf2.Remake.Content/Scenarios/BattleGrowthReader.cs#L90)
forwards all three when replacing deployments. [ExplorationContentReader.Read:255](../../src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs#L255)
reconstructs a definition to add `Outcome` but stops forwarding at `PrismLaser`, defaulting later
parameters to null/false. Constructor growth thus requires unrelated callers to know every capability.

Current admission prevents loss: `BattleGiveReader.Read` rejects connected Give,
`BattleDiscardReader.Read` rejects connected Discard, and `DecodeBattle` rejects connected poison.
`BattleEquipmentReader.Weapons` keeps its standalone boundary explicit too. Growth's current copy is
complete. Do not describe accepted standalone packages as broken or remove guards as an audit fix.
The risk becomes reachable if connected capability work relaxes admission without updating the
independent outcome reconstruction.

**Smallest maintainable remedy:** during that integration, place narrow immutable `WithOutcome` and
deployment-replacement operations with `BattleDefinition`, so callers specify only the intended change
and its owner preserves other fields. Keep construction/admission internal. Two reconstruction paths
already demonstrate the need; a generic capability bag, mutable setters, reflection copier or
compatibility layer would add unnecessary machinery. Appending today's missing arguments is a smaller
immediate patch, but retains the next-constructor-extension omission risk.

**Focused acceptance:** before opening admission, exercise supported changed content combining the
new connected capability with outcome return and growth where admitted. Verify behavior and preservation
of unrelated capabilities/resources through reconstruction/re-entry. Do not test parameter counts or
promise unsupported combinations. Scope Domain definition ownership and these Content call sites;
a broad constructor rewrite is unnecessary.

## Six mechanisms and accepted foundations

These are source traces and assessments of existing assertions. Except for the explicit F1/F2
executions, they are not fresh test passes or natural original-game observations.

| Mechanism | Inspected path and relevant assertions | Assessment and remaining boundary |
| --- | --- | --- |
| Taros | `TarosEffectiveness.PrepareInitial` → physical/strike construction → scene continuation; `TarosEffectivenessTests` varies actor/weapon, ineffective/counter/sleep, seeds and later reward failure. Leader cleanup and drops feed ordered death/custody. | Retain source identity and live equipment operands. No audit-case ID selects the rule. Actual encounter/aftermath/Caravan acquisition is outside authored proof. No required redesign identified here. |
| Jaro | `BattleStoryReader`, round continuation, shared `ProgramRunner`, `BattleStoryOperations.Convert/JoinAi`, then outcome-party operations/return; `JaroVictoryProgramTests` covers later faults, retained join/AI cursor, retry, member substitution and reserve return. | Keep two logical actors and visual alias distinct. `PrefixFailure` carries accepted conversion writes; selected outcome policy precedes the shared tail. F1 affects entry and return. Actual Battle32/Moun remains unbound. |
| Prism Flowers | `PrismLaser.RequireDefinition/Targets/Prepare`, automatic decision → action/scene/death continuation; `PrismLaserSessionTests` covers ordered both-side deaths, empty reconstruction, known-no-ally credit and accounting retry. | Keep decision focus separate from live construction targets/main-RNG draws. Source-slot order is data. Non15 activation, private Battle36 and broader original media are excluded. No generic targeting framework is warranted. |
| Fairy Woods | Authored initialization reader, `BattleSpawnRules.Populate`, round programs/appearances; `HiddenSpawnSessionTests` covers reset, reappearance, connected retry and failure after delivery. | Reset commits before appearances; queue generation follows delivery. First-round native variation exposes F1. Trial score/result/session-best and actual Battle44 remain gaps. |
| Special exits | `Sf2RetreatAction`, item/spell preparation → `BattleRetreatContinuation` → `BattleOutcome.ReturnFromRetreat`; `BattleRetreatTests`/`AngelWingTests` cover payment, stale input and walking retry. | Payment differs from text/effect/close/return tokens; later failure retains it. Keep finite EGRESS/Wing behavior. Battle0 terminal caller and refused Suspend/product semantics remain separate work. |
| Raft/Caravan | Caravan reader/programs/runtime, entity service and field sprite readiness; `CaravanTests` varies logical/physical mapping, lists, missing mapping, undelivered sprites, landing and later failures. | Typed instructions reuse program/entity machinery. Logical members and raw slots intentionally differ. No second mutable vehicle controller is needed. Raft, acquisition, broader map transfer and services remain excluded. |

Common foundations inspected around those paths:

- **AURA/healing:** `HealingTargetRules` separates centers from source-ordered recipients;
  `BattleSceneContinuation` retains ordered reactions/healing work. Priest/slot/HP restrictions are
  explicit, not a promise of every spell/class combination.
- **Equipment/Give/Discard:** equipment computes live ATT/range from validated loadouts; Give retains
  a transfer wait and runs its after-turn tail after reveal/acknowledgement; Discard returns to action
  selection without consuming the turn. Distinct lifecycles should not be unified mechanically.
  Existing session assertions vary slots, exchange, equipment removal, stale selections and custody.
- **Poison/death/rewards:** poison and field-death continuations separate damage, removal/accounting
  and finalization. Death credit distinguishes Unknown from known no ally. Later faults retain the
  current token/prefix instead of rebuilding an action. Poison/Prism/Taros tests target these seams.
- **Story/outcome retry:** program cursor/callers persist across waits; `BattleOutcome.Continue`
  resolves its policy before common-tail writes. Program policy faults deliberately retain a resumable
  snapshot while the result reports failure, unlike terminal Unsupported. Jaro's retry assertions show
  why globally forcing every failure snapshot to stopped would change behavior. Automatic host retry
  cadence for custom transient policies was not observed and is not claimed safe.

## What can remain as it is

**Confirmed at the inspected seams:** inward dependency direction, one session snapshot publisher,
collection copies at the inspected battle/start/story constructors, query/selection separate from
publication, and explicit wait identity/actual completion. Public/private distinction and selected
private encounter pins remain intact. No reason was found to collapse Domain/Application or add a
general event system. This does not certify every nested object or third-party rule implementation as
immutable; the audit traced the admitted constructors and consumers named above.

Existing tests contain useful changed states/content and later faults: Taros actor/weapon substitution,
Prism live/empty targets, hidden-spawn re-entry, Caravan physical/list variation, and Jaro member and
retry variation. Internal-continuation tests help isolate exact failure seams but cannot replace host
composition; F1 is the concrete blind spot. Apply the [current verification policy](../development-and-verification.md#scope):
behavior tests and direct adapter observation, without counts or tests of verification tools.

The inspected capability/admission owners agree about standalone Give/Discard/equipment/poison and
incomplete real-chapter closure. This review adds a host correctness exception without overturning
accepted bounded results. Large capability/verification documents were not globally audited or rewritten;
the two-path scope does not authorize changes to those owners or the UI topic.

## Reproduction and validation

Use the owning checkout root and existing environment. Startup variants change authored inputs before
admission; they do not inject state into a running game.

```powershell
$ErrorActionPreference = 'Stop'
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
New-Item -ItemType Directory -Path local/issue1373 -Force | Out-Null
$d = Get-Content -LiteralPath remake/content/authored/hidden-spawns.json -Raw | ConvertFrom-Json -AsHashtable
$d.start.actors[0].positionOverride = @{ x = 2; y = 3 }
$d | ConvertTo-Json -Depth 80 | Set-Content -LiteralPath local/issue1373/initial-appearance.json -Encoding utf8
& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore
```

Existing locked restore state was reused. For each native launch, create fresh `appdata`, `localappdata`
and `temp` directories beneath `local/issue1373/<run>`; point APPDATA, LOCALAPPDATA and TEMP/TMP there,
retaining the absolute shared SDK/CLI-home selections. Executed native arguments:

```powershell
& $env:GODOT_BIN --headless --path (Join-Path (Get-Location) 'remake/game') --quit-after 4 -- --authored-package (Join-Path (Get-Location) 'local/issue1373/initial-appearance.json')
```

The baseline used `remake/content/authored/hidden-spawns.json` instead. Both processes completed; none
was retained. No screenshots, input automation or fabricated completions. The build bound the necessary
adapter reproduction to inspected source; it was not a blanket audit gate. Build: **PASS**, zero
warnings/errors. Changed-start native: **FAIL**, preserved exception above. Baseline: **PASS only at
the four-frame startup boundary**. No repair or correction rerun occurred.

Public API checks used PowerShell 7.6.9 and the just-built production assemblies:

```powershell
foreach ($assembly in @('Domain','Application','Content')) {
    Add-Type -Path (Join-Path (Get-Location) "remake/src/Sf2.Remake.$assembly/bin/Debug/net8.0/Sf2.Remake.$assembly.dll")
}
$reader = [Sf2.Remake.Content.Scenarios.AuthoredScenarioPackageReader]::new(
    (Join-Path (Get-Location) 'local/issue1373/initial-appearance.json'))
$started = [Sf2.Remake.Application.Runtime.GameSession]::Start($reader)
$started.Session.Current # Inspect the specific fields reported in F1; do not dump private payloads.
```

For F2 call `[Sf2.Remake.Content.Scenarios.AuthoredScenarioPackageReader]::FromDocumentBytes(
[Text.Encoding]::UTF8.GetBytes($json)).Read()` inside try/catch, inspecting the base exception.
Use `[]`, `null`, `{}`, then the tracked hidden-spawns document with `start=[]`. Results are in F2.
For the callback case, load tracked `battle-drops.json`, set `start.map='arena'`, append `401` to
`start.flags`, replace the `before` program instructions with one `end`, serialize to a new ignored
file and call the same public `GameSession.Start` API. The version without `401` remained exploration;
the unlocked version reached battle control. These are direct public API checks, not a new test harness.

Retained ignored evidence under `local/issue1373`: `adapter-build.log`, `start-state.json`,
`admission-results.json`, `native-initial.log`, `native-baseline.log`, both direct-battle state summaries
and their authored variants. Facts and reconstruction above are durable; ignored logs are not the sole
basis of a finding. No private inputs were read for these reproductions.

Documentation acceptance is exact-path review, `git diff --check`, local link/anchor/fence checks,
private-boundary inspection and `uv run sf2 verify plan --base origin/main --head HEAD` on a clean
committed candidate. Actual plan/CI outcomes and candidate/base/tree/remote identity belong in the
Issue/PR handoff. No normal/full Python, blanket engine, research, H3 or emulator suite was run.
Existing failures remain preserved; unrun gates are not PASS.

## Limits and ordered recommendation

This responsibility-led sample does not prove the whole engine clean. It did not exhaustively audit
all decoders/resources, private payloads, malformed shapes, command histories, custom policies,
full-game performance, save/load or later chapters. Original timing, aesthetics, #1047 Unknowns and
unmerged PR #692 were not reassessed. No performance finding or large acquisition follows.

1. **Fix before dependent work — F1:** correct host ownership/callback setup before relying on initial
   appearance/region waits or direct battle return in further six-family composition.
2. **Implemented, awaiting independent integration — F2:** ordered shape admission and direct public
   API controls pass within the declared sibling boundary; no whole-decoder cleanliness claim follows.
3. **Implemented bounded F3:** narrow immutable replacement owns the attack-spell outcome/growth
   integration; its actual supported composition is exercised. Keep unrelated admission guards.
4. **Acceptable as-is within limits:** retain the modular monolith, one state authority, finite rule
   seams, explicit prefix/token model and separated private trust. Continue bounded feature work with
   changed-state and actual host-composition acceptance; no speculative refactor queue or clean-system claim.
