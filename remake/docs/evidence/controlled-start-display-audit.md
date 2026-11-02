# Controlled-start display audit

## Decision and scope

**Standard rigor:** [#1433](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1433)
investigates nine retained diagnostic failures and their product impact. The dependency crosses
Content admission, Application startup and transfer, but the deliverable is bounded evidence and a
repair proposal. Production, tests and reference inputs remain unchanged. No aggregate rerun,
emulator acquisition, Godot launch, screenshots, asset export or audio work is required.

The inspected and reproduced base is accepted `966728ebee075f208c3af7cd4c8afff46c840adf`, tree
`a43d4393ed9bc16d6a4e284c83e0b4f2229ad145`. Original-source identities remain SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6` and USA ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This audit adds remake execution evidence, not a new original runtime observation.

**Confirmed:** all nine historical failures stop at the first ordinary warp because the selected
diagnostic session has no `Story.Display`. There are two missing initialization owners: the R1
checkpoint omits display state; the Map21 helpers construct another start without display state.
The historical world also omits map palettes, a separate content dependency masked by the first
failure. A palette-complete world still leaves the diagnostic display null.

**Disposition:** correct diagnostic start declarations/construction and their selected content in a
separate bounded slice. No production fade change or assertion retirement is justified here.
No historical FAIL becomes PASS. Later assertions remain unexecuted or unverified by this audit.

## Invariant and failure boundary

[`MapTransfer.ValidateDisplay`](../../src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)
requires a positive byte period, valid BASE/CURRENT logical palette pairs, a defined settled
visibility, black/white CURRENT for those visibility states, and CURRENT equal to BASE otherwise.
`Transitioning` is invalid at this admission boundary; an already running fade has its own typed
wait. These pairs represent palette 1 colors 2 and 3, not the full rendered frame.

An ordinary Move previews its warp in
[`ExplorationDispatcher.Move`](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationDispatcher.cs).
`MapTransfer.Validate` first previews destination construction, then validates the live display
and Rebuild palette. Both observed first warps reach this validation before `BeginWarp` or
`BeginFade`. The failure is `UnsupportedCapability`, code `full-fade-state`, field `story.display`.
No movement/warp observation is emitted and the same exploration world/position remains; a new
snapshot records `StopReason.Unsupported`. This is not a failed palette interpolation or audio fade.

[`ExplorationContentReader`](../../src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)
keeps map `basePalette` and optional `start.display` separate. An embedded map palette or
`start.mapPalettes` normalizes into map definitions; neither initializes live display state.
[`ExplorationDispatcher.Start`](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationDispatcher.cs)
validates a supplied display and passes it into StoryState, but permits null for entries that do
not use these operations. It deliberately invents no period, CURRENT or visibility.
Application owns the invariant and live evolution; Content/the explicit start owns initialization.
The [existing display contract](../verification/field-programs.md#ordinary-warp-visible-return)
and [source proof](../../../docs/research/chapters/grans-opening/map3-controlled-start-egress-transition.md#selected-r1-period-and-initial-display-binding)
retain that boundary.

## Actual input and start chains

The retained `local/issue1045/inputs.ps1` selections resolve all six required encounter/static inputs
and the world successfully. `PrivateBattleScenarioTests.Selected(controlled)` explicitly replaces
the environment's `SF2_PRIVATE_CONTROLLED_START` for these nine nodes. Changing that environment
variable alone cannot repair them. The Engine.Tests project copies the tracked starts into its
`controlled/` output; there is no hidden alternate checkpoint.

| Chain | Actual declared state and owner | First failing input |
| --- | --- | --- |
| R1 | [`map3-opening-start.json`](../../reference/inputs/map3-opening-start.json), Map3 `(56,3)`, Down, speed32, F0/F32, retained walking phases; [`map3-opening-party.json`](../../reference/inputs/map3-opening-party.json). `StartSource` → `RunOpening` in [`PrivateExplorationTests`](../../tests/Sf2.Remake.Engine.Tests/PrivateExplorationTests.cs). No display or mapPalettes is declared. | R2 logicalInputTrace input 2: Left from Map3 `(55,3)` previews the Preserve warp at `(54,3)` to Map3 `(3,3)`. Failure precedes any Yes/No answer. |
| Bridge | [`map40-intro-start.json`](../../reference/inputs/map40-intro-start.json) is read with [`battle01-player-ready.json`](../../reference/inputs/battle01-player-ready.json); `Bridge` in [`PrivateBattleEntryProgramTests`](../../tests/Sf2.Remake.Engine.Tests/PrivateBattleEntryProgramTests.cs) replaces it with Map21 `(5,15)`, Down, speed32, F0/F1/F2/F32/F33/F34/F401. Its new `ExplorationStartInput` omits display. | player-ready inputPlan input 18: Up from Map21 `(9,2)` previews the Rebuild warp at `(9,1)` to Map40 `(4,30)`. Before-battle code has not started. |
| Membership | Same `Bridge(seen:true, sarah:false)`, flags F0/F2/F32/F34/F401/F451. The pure `BattleTurnFlow.Start` variations execute before `Traverse`; they do not install session display. | Same Map21 input 18, before the session reaches battle. |
| Defeat seam | `Read` in [`PrivateBattleOutcomeProgramTests`](../../tests/Sf2.Remake.Engine.Tests/PrivateBattleOutcomeProgramTests.cs) reads Map40 with opening-party; `StartBattle` constructs Map21 `(5,15)`, Down, speed32, gold123 and F0/F1/F2/F32/F33/F34/F401/F600–605/F607/F608. It also omits display. | Same Map21 input 18 inside `StartBattle` → `Traverse`, before `Play(lose:true)`. |

The baseline world `private-map3-programs` is 103,238,115 bytes, profile
`private-local-controlled-start`. Its six maps (3/19/20/21/40/57) all lack `basePalette`.
The existing `world-services-issue1424.json` is 125,828,417 bytes; its seven retained maps all have
palettes. Both were read in place. The four chains above were observed with both selections:
display remains null, the same input fails, and only map-palette availability changes.

**Confirmed (code):** Map21 helpers omit display even if their upstream `Read()` later supplies one.
Copying a Map40 display would also bind the wrong map's BASE. A Map21 start needs its own binding.
**Inferred next blocker:** adding display alone against the old palette-less world cannot complete
the Rebuild path: `MapTransfer.Apply` requires the target palette when display is present, and
`Validate` independently requires it for Rebuild. This was reviewed directly, not demonstrated by
injecting an invented display state. Preserve warps still need the initial live BASE/CURRENT pair.

## Nine-node disposition

All names below are under `Sf2.Remake.Engine.Tests`. Every historical stack in
`local/issue1421/engine-final-01.log` names `full-fade-state`; the nine-node inventory remains
`local/issue1423/inherited-failures.txt`. Stack inspection and the exact common caller establish
each attribution; current individual reproduction is distinguished from a shared-prefix finding.

| Node | Historical failing caller / chain | Current status and decision |
| --- | --- | --- |
| `PrivateBattleOutcomeProgramTests.ActualCommandsCompleteTheWholeVictoryProgramFlagsAndUsableReturn` | `ContinuousBattle` → `RunOpening`, R1 | Not individually rerun. Confirmed shared R1 prefix; victory/reward/return assertions never reached. Diagnostic checkpoint/content correction; retain assertions. |
| `PrivateBattleEntryProgramTests.ContinuousOpeningCastleTowerAndBeforeBattleRetainTheLivePartyAndReachActualBattleInput` | `ContinuousBattle` → `RunOpening`, R1 | Not individually rerun. Confirmed same prefix, before castle navigation or battle admission. Same correction; retain live-party assertions. |
| `PrivateBattleEntryProgramTests.ExplicitH3BridgeRunsTheFullBeforeBodyAndMatchesPlayerReadyWithoutClaimingR1Continuity` | `Traverse` → `Input`, Bridge | Individually reproduced FAIL; confirmed null display and first-warp preflight. Correct Map21 diagnostic construction/content; retain H3 comparison and its no-natural-continuity boundary. |
| `PrivateBattleOutcomeProgramTests.ContinuousOpeningDefeatKeepsOneSessionThroughTheWholeProgramAndReturn` | `ContinuousBattle` → `RunOpening`, R1 | Not individually rerun. Confirmed same prefix, before battle or defeat. Same correction; retain one-session defeat/return assertions. |
| `PrivateExplorationTests.RefusalPreservesUnjoinedStateAndARealSarahInteractionCanLaterJoinTheParty` | `RunOpening(false)`, R1 | Not individually rerun. Confirmed same prefix before any answer; refusal cannot cause this failure. Same correction; retain refusal/retry membership assertions. |
| `PrivateBattleOutcomeProgramTests.OrdinaryLeaderLossKeepsItsOwnRecoveryAndRunsTheRealPostMessengerReload` | `StartBattle` → `Traverse` → `Input`, Defeat seam | Individually reproduced FAIL; confirmed null display before battle, not recovery/reload failure. Correct its separate Map21 constructor/content; retain defeat accounting/reload assertions. |
| `PrivateBattleEntryProgramTests.LiveActiveMembershipPacksFormationWithoutAddingSarahOrReplacingThePartyResources` | `Traverse` → `Input`, Membership | Individually reproduced FAIL; confirmed null display despite F451 and the omitted Sarah. Correct Bridge construction/content; retain changed-membership/resource assertions. |
| `PrivateExplorationTests.FullOriginalOpeningConsumesR1InputsAndMatchesR2AndR2aThroughStableFieldControl` | `RunOpening(true)`, R1 | Individually reproduced FAIL with baseline and palette-complete worlds. Confirmed checkpoint omission at input 2. Correct R1 declaration/content; retain R1/R2/R2a comparisons. |
| `PrivateExplorationTests.CastlePalaceAndAstralRunFromTheLiveOpeningWithFirstRepeatAndChoiceVariants` | `RunOpening` before castle fixture traversal, R1 | Not individually rerun. Confirmed same prefix, before palace/Astral/choice variants. Same correction; retain first/repeat/choice assertions. |

The common immediate cause is established for all nine, not merely inferred from their names.
**Unknown:** downstream results after correct initialization, including exact RNG/phase comparisons.
No nine-node PASS, original display-timing closure or obsolete-test retirement is claimed.

## Product impact and smallest follow-up

| Entry or transition | Dependency review and impact |
| --- | --- |
| Ordinary New Game | [`GameRoot.BeginNewGame`](../../game/src/GameRoot.cs) uses `OpeningNewGameReader`, not `StartSource`/Bridge. [`PrivateBattleContentReader.Initialize`](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleContentReader.cs) requires palettes on every retained map and explicitly constructs Map3 BASE=CURRENT, BaseRestored, period3. Missing palette rejects startup. It does not read either checkpoint. No shared missing-initialization defect found. |
| Continue | [`OpeningRestoreReader`](../../src/Sf2.Remake.Content/Persistence/OpeningRestoreReader.cs) admits immutable definitions without initializing a new party. [`OpeningRestore.Build`](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningRestore.cs) validates positive saved FadePeriod, requires the reconstructed map palette, and constructs BASE=CURRENT/BaseRestored. Save capture requires settled restored display. No helper-start dependency found. |
| Ordinary map transfer | Application retains the existing StoryState, fades its CURRENT, updates BASE only for admitted Rebuild, and returns through `ReturnVisible`. It does not construct a nullable diagnostic start. Both valid product entries supply its prerequisite; their accepted actual route exercised this consumer. |
| Battle entry and return | [`BattleEntry`](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [`BattleOutcome`](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs) and `StoryState.Copy` preserve display through battle/field switching; scene/return programs use the same palette guards. The nine failures precede these consumers. Accepted ordinary victory and defeat evidence remains applicable. |
| Explicit diagnostic host entry | `--private-exploration-start` uses the same nullable controlled reader. Selecting either incomplete checkpoint can expose the defect during diagnostic play, even with product world palettes. This is a real diagnostic-input failure; no Godot rendering result is inferred from the engine run. Public authored or arbitrary later starts are outside this audit. |

The [composed ordinary route](opening-loop-acceptance.md),
[Save/Continue and actual defeat/retry](../verification/host.md#opening-save-and-continue), and
[Church defeat/recovery](../verification/host.md#opening-church-services) already retain actual
native observations. No unresolved dependency in this review requires repeating those routes or
launching another host. This is a bounded impact assessment, not universal startup/restore parity.
[#1410 audio lifetime warnings](../verification/host.md#audio-resource-lifetime-observation) concern
different resources and remain outside the display investigation.

The smallest follow-up should own the two diagnostic test constructors, the R1 start declaration
and the specifically selected palette binding, using the existing reader and palette helper:

1. Bind R1's explicit display to its existing
   [period3/restored-pair source proof](../../../docs/research/chapters/grans-opening/map3-controlled-start-egress-transition.md#selected-r1-period-and-initial-display-binding).
   Reuse `selected_map_palette_bindings` only if a missing small binding must be reproduced; no asset
   export or new runtime acquisition is implied. Do not derive observed CURRENT from palette data alone.
2. Give each Map21 seam an explicit Map21 display declaration and pass it to the constructed start.
   Reuse an evidenced binding if one covers that exact seam. Otherwise record the missing
   period/CURRENT/visibility as **Unknown** before changing an original-comparison input. The R1
   proof does not automatically prove the separately seeded H3 bridge's display state. Any declared
   remake-only control must be labelled as such, with its effect on the H3 endpoint assessed.
3. Select the already prepared palette-complete content where its other admitted behavior matches
   the diagnostic scope, or supply the minimum source-backed `mapPalettes` for the retained old
   world. These are alternative bindings: the reader rejects embedded-plus-start duplicates even
   when equal. Do not silently switch all tests to a different world or overwrite the old input.
4. Rerun only the affected nodes after correction and investigate any new failure at its own seam.
   Preserve party/seeds/phase assertions and historical results. Do not add a default display in
   `GameSession`, relax `ValidateDisplay`, copy Map40 BASE into Map21, or skip the nine nodes.

No production-code owner, new schema, startup framework or repair batch is needed for this audit.
Whether all comparisons still match after the bounded correction remains **Unknown**, not a
reason to retire useful engine assertions in advance.

## Reproduction and retained results

All commands run from the owning repository root with existing inputs/installations/caches, after
loading `local/issue1045/inputs.ps1` in the same process and forcing
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. `DOTNET_BIN` and `DOTNET_CLI_HOME` remain configured absolute
selections; the SDK is pinned to 10.0.204. Set `SF2_REQUIRE_PRIVATE_TESTS=1`; no missing-input skip
can count as execution. Output stays under ignored `local/issue1433`.

```powershell
. ./local/issue1045/inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
$r1 = 'Sf2.Remake.Engine.Tests.PrivateExplorationTests.FullOriginalOpeningConsumesR1InputsAndMatchesR2AndR2aThroughStableFieldControl'
$bridge = 'Sf2.Remake.Engine.Tests.PrivateBattleEntryProgramTests.ExplicitH3BridgeRunsTheFullBeforeBodyAndMatchesPlayerReadyWithoutClaimingR1Continuity'
$membership = 'Sf2.Remake.Engine.Tests.PrivateBattleEntryProgramTests.LiveActiveMembershipPacksFormationWithoutAddingSarahOrReplacingThePartyResources'
$defeat = 'Sf2.Remake.Engine.Tests.PrivateBattleOutcomeProgramTests.OrdinaryLeaderLossKeepsItsOwnRecoveryAndRunsTheRealPostMessengerReload'
$project = 'remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj'
& $env:DOTNET_BIN test $project --configuration Release --no-restore `
  --filter "FullyQualifiedName=$r1" --logger 'trx;LogFileName=map3-baseline.trx' --results-directory local/issue1433
& $env:DOTNET_BIN test $project --configuration Release --no-restore `
  --filter "FullyQualifiedName=$bridge|FullyQualifiedName=$membership|FullyQualifiedName=$defeat" `
  --logger 'trx;LogFileName=map21-baseline.trx' --results-directory local/issue1433
$env:SF2_PRIVATE_EXPLORATION_CONTENT = (Resolve-Path -LiteralPath 'local/issue1045/world-services-issue1424.json').Path
& $env:DOTNET_BIN test $project --configuration Release --no-restore `
  --filter "FullyQualifiedName=$r1" --logger 'trx;LogFileName=map3-product-world.trx' --results-directory local/issue1433
```

**Confirmed, completed results:** baseline R1 0 pass/1 fail/0 skip (949 ms); three Map21 nodes
0 pass/3 fail/0 skip (692 ms); palette-complete R1 0 pass/1 fail/0 skip (460 ms). Each test command
exited 1 with the same `full-fade-state` boundary. Logs/TRX retain exact stacks. These are successful
failure reproductions, not passing behavior tests. Only four distinct named nodes were rerun.

The retained scratch `observe-prefix.ps1` directly loads the same Release assemblies, uses actual
production readers and start constructors, sends the existing fixture's Move prefix through
`GameSession.Submit`, and reuses `RunUntilStop` only to deliver its existing typed waits. It sets no
session state and installs no display. `pwsh -NoProfile -File local/issue1433/observe-prefix.ps1`,
then the same command with `-ProductWorld`, both exit 0 and produce `prefix-baseline.json` and
`prefix-product.json`. Their four rows per selection establish the null display, first failure
ordinals/positions, palette distinction and unchanged exploration world described above. The
Membership prefix intentionally observes only the same Bridge start/first warp; its independent
formation assertions are covered by the named node reproduction, not duplicated in the observer.
`input-summary.py` records existence, size, identity and omitted/present keys without copying assets.

The old aggregate remains **completed 1692 pass/20 fail/1 skip**, with its exhaustive dispositions
and subsequent narrow results in `local/issue1421`–`local/issue1425`. Neither it nor a full/private
profile was rerun. New compact outputs occupy under 0.1 MB, with about 3.09 GB free on the execution
drive. No new environment, SDK, asset copy, native instance or save slot was created.

For this documentation diff, perform direct ownership, links/anchors, tables/fences, whitespace
and private-boundary checks, then inspect `uv run sf2 verify plan --base origin/main --head HEAD`
on the clean committed candidate. Exact candidate/remote/CI identity belongs to the PR handoff;
a documentation-only CI skip is not a runtime PASS. Main-gate retains independent acceptance.
