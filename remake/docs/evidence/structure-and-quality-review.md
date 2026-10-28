# Remake Structure and Quality Review

## Decision and scope

Retain the four-assembly architecture. Fix **F1's host lifecycle defect before work relying on initial
battle appearances, region programs or direct battle-to-world return**. Schedule **F2's typed admission
repair** as a bounded Content follow-up. Address **F3's definition-copy risk with the first connected
capability extension that needs it**, before relaxing existing admission restrictions. None warrants a
general engine rewrite, generic capability registry or new verification platform.

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

### F3 — P2: avoid silent defaults when reconstructing definitions

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
2. **Bounded follow-up — F2:** restore typed authored-input rejection through ordered shape checks.
   Unrelated numerical engine work need not wait.
3. **With affected integration — F3:** centralize narrow immutable definition replacement before
   enabling connected capabilities whose fields would otherwise default away. Keep current guards.
4. **Acceptable as-is within limits:** retain the modular monolith, one state authority, finite rule
   seams, explicit prefix/token model and separated private trust. Continue bounded feature work with
   changed-state and actual host-composition acceptance; no speculative refactor queue or clean-system claim.
