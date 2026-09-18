# Remake Development and Verification

## Scope

This document owns current remake commands and verification selection. The user's test policy,
recorded in [ADR 0019](../../docs/decisions/0019-state-and-content-driven-remake-engine.md), controls
over older blanket gate and test-preservation recipes:

- Add small unit tests of actual new-engine behavior, with independent expected values and meaningful
  state/configuration variation. A small session using real reducers is a useful unit boundary.
- Reference comparisons, validation, probes, fixture drivers, planners, gates, reports and test helpers
  are themselves verification. Use them directly when needed; do not add tests of those programs.
- Migrate useful behavior assertions or retire obsolete tests with their owning capability. Test
  counts, a replacement for every deletion, and a green old aggregate are not acceptance targets.
- Documentation-only changes use direct link/anchor/fence/table/example, scope and private-boundary
  checks. They do not require a new normal/full, .NET, Godot or H3 run.

The engine unit project covers actual Domain/Application/Content behavior for four authored battle
packages, two connected exploration packages and the bounded private entries, with scoped engine/adapter entries and public jobs. The native observation directly drives
the same session with Godot input and reads real state/HUD/nodes; it emits no images. Main-gate owns required-check configuration during integration; a candidate must report its
actual CI outcome and that configuration boundary explicitly.
[ADR 0012](../../docs/decisions/0012-dependency-aware-partitioned-verification.md) continues to own
affected research evidence selection. Shared research changes keep their separate owning requirements.

Godot acceptance observes the actual running instance: Application snapshots, commands, presenter
projections, node geometry/visibility/focus where relevant, and process errors. Screenshots, image
comparison and frame-inspection recipes are prohibited. Existing completed images remain historical
artifacts. A probe that still emits images needs that side effect disabled in an owned change before
a future required run. A remote debug server is not an established dependency; first use an existing
native probe or suitable debug/state interface that answers the concrete question.

Reuse the same verified Godot installation, owning project and running debug instance. A concrete
failure, necessary code/import restart, observed contamination, or test of startup/export/cleanup can
justify a bounded restart or isolated instance; record the reason. A new slice or comparison does not
justify copying the project, extracting another editor, or building a debug framework.

## Locked .NET Workflow

Load the existing ignored host/worktree environment before every SDK command, including informational
commands. Select one existing absolute `DOTNET_BIN` and one absolute `DOTNET_CLI_HOME` shared by this
project's worktrees. Force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false` at the actual child launch.
The maintained entrypoints reject missing/relative selections. No launch may change registry PATH.

The SDK otherwise adds its tools directory by default; a new CLI home can repeat first-run setup.
`DOTNET_SKIP_FIRST_TIME_EXPERIENCE` does not supply the PATH protection. The
[official variable reference](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-environment-variables#dotnet_add_global_tools_to_path)
owns that SDK behavior. Use the SDK pinned by [global.json](../global.json); do not install another one
to configure sharing or make a check pass. Keep NuGet packages/HTTP cache worktree-local, outside the
shared CLI home, and preserve the tracked package versions and locks.

For an affected current product project, run from `remake/` after that setup. For example, when
Domain itself is affected, select its existing project explicitly:

```powershell
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$project = 'src/Sf2.Remake.Domain/Sf2.Remake.Domain.csproj'
& $env:DOTNET_BIN restore $project --locked-mode
& $env:DOTNET_BIN build $project --configuration Release --no-restore
```

From the repository root after loading that environment, the maintained entries are:

```powershell
uv run sf2 verify engine
uv run sf2 verify adapter
uv run sf2 verify reference-host
```

`verify engine` restores in locked mode, builds and tests
`remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj`; it references production
Domain/Application/Content and copies the actual `remake/content/authored/*.json` inputs.
`verify adapter` restores in locked mode and builds `remake/game/Sf2.Remake.Godot.csproj` without tests
or native Godot. `verify reference-host` separately restores/builds
`remake/reference/game/Sf2.Remake.Reference.Godot.csproj`, which owns the remaining legacy and smoke
consumers. These commands launch the selected executable from `remake/`, honoring `global.json` and the
protected environment. Neither needs private inputs. The engine project is selected directly;
the current `Sf2.Remake.sln` still includes four production and four legacy test projects.
Whole-solution commands remain available for an explicitly applicable legacy/release check, not every
engine or documentation change. Use formatting only for affected product projects when needed.

## Ordinary and Reference Host Startup

After loading the retained worktree SDK/Godot environment, ordinary play uses:

```powershell
& $godotBinary --path remake/game
& $godotBinary --path remake/game -- --authored-package (Resolve-Path -LiteralPath 'remake/content/authored/garden-watch.json').Path
```

The same project accepts `--private-battle-start` and `--private-exploration-start` with the selected private inputs described below. These and `--authored-package` are the only game options; each takes one path and may appear only once, and the options
are mutually exclusive. Unknown/positional, duplicate, conflicting and missing-path arguments report
startup ContentError in the common view before creating a session. They cannot select legacy play.

The external observer owns `SF2_OBSERVATION_CASE`, `OUTPUT`, `FOLLOWUP_SHAPE`, `TARGET_SHAPE`,
`ENEMY_SHAPE`, `CONTINUATION_SHAPE`, `LONG_PATH` and `REVERSE_KILLS` (all with the same prefix).
Unset/empty CASE runs the default observation; flags use1. Former `--observation-*`/shape options are
not game arguments. Each example assigns its case/output; optional shapes belong to that named case.
Diagnostics do not affect ordinary launches without the script. Direct startup observation uses:

```powershell
$env:SF2_OBSERVATION_CASE = 'startup'
$env:SF2_OBSERVATION_EXPECT_FAILURE = ''
$env:SF2_OBSERVATION_EXPECT_ORIGIN = 'public-authored-controlled-start'
$env:SF2_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'startup.json'
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd
```

For a rejected real-argument launch, set EXPECT_FAILURE to `unknown-startup-option`,
`duplicate-startup-option`, `conflicting-startup-options` or `missing-startup-path` and append the
corresponding arguments after Godot’s `--`. Private success uses EXPECT_ORIGIN
`private-local-controlled-start`. The script observes actual Main/GameRoot/BattleSessionView and
published startup state; it does not set engine state. Preserve process output and require passed:true.

Existing legacy arguments and observations instead select the explicit reference project:

```powershell
uv run sf2 verify reference-host
& $godotBinary --headless --path remake/reference/game -- --map3-smoke
```

This retained source smoke does not authorize replaying the full legacy suite. Its private inputs,
programs and native recipes stay under their current reference owners. Ordinary export configuration
excludes `probes/*`; complete ordinary export/assets remain unverified.

## Exploration and Program Observations

Use [the exploration/program owner](./exploration-programs.md#reproduction) for the current
Content preparation, controlled starts, selected private comparisons and ordinary native recipes.
The native observers exercise real input and read the same live session across mode changes. `engine_map3_opening_observation.gd` reads the R2 fixture only as an external input trace and covers complete acceptance or decline/re-prompt. `map3-opening-party.json` supplies the named opening party. Later reference comparisons may explicitly select `SF2_REFERENCE_POST_OPENING_START` with `map3-post-opening-reference-start.json`; that context has no executed opening history. The
private source programs retain their exact unsupported native/population frontiers; a stopped prefix
is not a completed original route. `EntityMotionTests` directly compares the extracted core and
destination admission to the13 owned H3 cases; it does not test a verification program.

## Authored Start State Observation

The format-v7 reader returns immutable definitions and explicit start input. After loading the existing
worktree environment, refresh the actual Debug adapter assembly before the existing no-image probe.
Run the four tracked packages through the same public session: the two HEAL packages use the default
observer (eight checkpoints each), and the two physical packages use `SF2_OBSERVATION_CASE=physical`
(seven checkpoints each). Existing integer/RNG/action expectations remain unchanged.

A differing controlled start reuses the yard's definitions and deployment; only its start object changes:

```powershell
$data = Get-Content -LiteralPath 'remake/content/authored/practice-yard.json' -Raw | ConvertFrom-Json
$data.start.actors[0].positionOverride = [pscustomobject]@{x=4; y=3}
$data.start.actors[0].mp = 9
$data.start.actors[0].kills = 12
$data.start.actors[0].defeats = 3
$data.start.gold = 1234
$data.start.thinkingSeed = [uint32]0x12344321
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'controlled-start.json'
$data | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'controlled-start-observation.json'
$env:SF2_OBSERVATION_CASE = ''
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Inspect all eight checkpoints and the process log, not just exit0: the actual actor starts at(4,3),
MP9 becomes6 through HEAL, gold1234/kills12/defeats3 and thinking seed12344321h remain carried. The
probe still drives movement/cancel, HEAL/STAY, next round and attributed rejection. It changes no live
state and emits no images. `BattleStartStateTests` separately reuses the same admitted definition
object with two explicit typed starts and independent histories; no tests of this observer are added.

All mutation examples below keep actor/rule maxima in `actors`, current resources in `start.actors`,
and actual encounter layout changes in `encounters[].placements`. Adding a deployed actor also
requires its explicit start record; no default counter or runtime actor is synthesized from a definition.

## Authored Extra Round Action Observation

Use the same installed editor/project after the affected Debug build. This format-v7 input changes only
one actor's explicit eligibility; its numerical agility remains12. Outputs stay in a fresh ignored run:

```powershell
$data = Get-Content -LiteralPath 'remake/content/authored/practice-yard.json' -Raw | ConvertFrom-Json
$data.actors[0].extraRoundAction = $true
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'extra-turn-input.json'
$data | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'extra-turn-observation.json'
$env:SF2_OBSERVATION_CASE = 'extra-turn'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Require all seven checkpoints and clean logs: initial control, uncommitted STAY selection, consumption
of the first and second entries, natural round2, and both entries again. Real Enter/Space input reaches
the same actor at queue cursor1, then the other ally at3 after automatic Stay AI. From the configured
high word1234, eleven draws end atFF4D and twenty-two at887A; low word1234 and thinking seed are carried.
The ordinary package still consumes nine draws. STAY leaves HP/MP, gold and actor placement unchanged.
The script reads the existing result/state/node interface; it adds no gameplay setter or new adapter
observation fields. Use the four package observations below for the preserved ordinary configurations.

`BattleAgilityTurnsTests` owns equal-agility definition variation, actual Content validation, capacity
and cross-round behavior. `TurnOrderRulesTests` keeps the independent signed0/127 fixture and scalar
seed expectations, including zero-range draws and the truncated secondary basis. `EnemyActionTests`
retains death/counter results and skips already-generated extra entries for dead actors. The selected
existing first-round reference methods named below exercise the actual source projection; no old
aggregate, new H3 or test of the observer is required.

## Authored Faction and Order Observation

Run the four package observations below, then prepare a changed-order physical input in a fresh ignored
run directory. This format-v7 variant keeps independent accepted combat/RNG expectations while moving
all orders beyond the original byte-side boundary and reversing all three JSON arrays:

```powershell
$data = Get-Content -LiteralPath 'remake/content/authored/stone-court.json' -Raw | ConvertFrom-Json
$orders = @(255, 1000, 70000, 80000, [int]::MaxValue)
$ordered = @($data.encounters[0].placements | Sort-Object processingOrder)
for ($i = 0; $i -lt $ordered.Count; $i++) { $ordered[$i].processingOrder = $orders[$i] }
[array]::Reverse($data.actors)
[array]::Reverse($data.encounters[0].placements)
[array]::Reverse($data.start.actors)
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'sparse-order-input.json'
$data | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'sparse-order-observation.json'
$env:SF2_OBSERVATION_CASE = 'physical'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Require seven physical checkpoints, real target input/node removal/next control and clean process logs.
Apply the same order/array transform after constructing the `secondary` multi-target input below to
observe actual AI selection with sparse orders; its five independent checkpoints and RNG expectations
are unchanged. `BattleFactionOrderTests` separately exercises a low-order enemy, renamed high-order
ally, healing/opposing movement, atomic rejection and rewards. `TurnOrderRulesTests` retains the signed
boundary fixture with `ActorRef` identities, a real order255 and separate null sentinels. The selected
existing reference methods `BaselineTestsThreeRegionsWithoutActivatingThemAndComputesTheAcceptedRound`
and `RoundGenerationRetainsTheIndependentSeedCopyWithoutUsingIt` exercise the actual source projection;
no old aggregate or new H3 observation is required.

## Authored Battle Observation

After loading the existing worktree environment, build the actual Debug assembly once when needed
by the existing Godot instance. Release adapter verification alone does not refresh Godot's Debug DLL.
From `remake/`:

```powershell
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
& $env:DOTNET_BIN build game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore
```

No-argument local Godot startup opens the authored yard. To select either real package, use the
existing verified Godot executable with `--path remake/game -- --authored-package <package-path>`.
Input is WASD/arrows for preview, Enter for action choice/commit, H for the known HEAL with self
initially selected, Tab for living allied targets, Space for STAY and Escape for cancel. Physical attack selection is X; Tab then selects living opponents. Packages without physical
definitions report Unsupported. Application runs AI and rounds automatically.

Tab cycles from the last attempted UI candidate, including a rejected one. A range rejection preserves
the session's accepted target and battle state; subsequent Tab input can reach later living targets of the selected action. The HUD
shows both attempted and accepted target after rejection. Cancel or the next action clears the UI cursor.
The map viewport keeps the acting origin, preview and target visible with clipping/pan/zoom. Wide
windows place a scrollable HUD beside the map; narrow windows place it below. Authored UI uses actual
window dimensions, while the legacy reference path retains its original window policy.

From the repository root, the direct observation command is:

```powershell
$packageName = 'practice-yard' # or garden-watch
$packagePath = (Resolve-Path -LiteralPath "remake/content/authored/$packageName.json").Path
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'observation.json'
$env:SF2_OBSERVATION_CASE = ''
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

`$godotBinary` is the already verified local installation, and `SF2_RUN_OUTPUT` is an explicit ignored
worktree-local destination from the existing environment. Run packages serially in that same project;
their startup selection is the concrete reason for separate observation processes. The script injects
real `InputEventKey` events, reads the common session result and actual HUD/node geometry/visibility,
checks cancellation, HEAL/STAY, carried RNG and automatic progression, and exits nonzero on failure.
Its JSON/log output is local generated evidence; it never emits images or modifies session state.
No tests of this observation script are required.

For the connected physical chain use the same installation/project and an ignored output directory:

```powershell
$packagePath = (Resolve-Path -LiteralPath 'remake/content/authored/stone-court.json').Path
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'physical.json'
$env:SF2_OBSERVATION_CASE = 'physical'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Repeat with `river-post.json`; `SF2_OBSERVATION_REVERSE_KILLS=1` selects the opposite legal kill order. The observation
uses actual X/Tab/Enter/Space input, including a rejected distant target followed by a valid target,
then checks exact carried RNG/resources, removed coordinates/nodes, next living control and the next
round. All four package/order combinations use common product commands. Reached unsupported
level/leader/outcome atomicity and independent arithmetic expectations belong to
`PhysicalBattleTests`, not tests of this observer. Selected existing reference comparisons for the
shared scalar extraction are `SourceArithmeticKeepsZeroIntermediateAndBothDownwardDrawsAtTheOriginalRange`,
`MissAndCriticalUseRealSeedsAndPreserveSourceCallOrder`, `RealSeedsExerciseMissCriticalAndIndependentExpVariance`
and `KillAccountingUsesSourceCapsAndKeepsUnknownInputsUnknown` in the legacy Domain test project.
For shared counter/reward changes also select `CounterReversesRolesAndCommitsPrimaryThenCounterAndExpAsOneEnemyReceipt`,
`CounterHalvesBeforeSpreadAndConsumesItsOwnFlagsWithoutAnotherAttack` and
`PostHealBowieCounterUsesItsOwnPermissionAndOneEnemyReceipt`; run
those methods with a `dotnet test --filter` expression when their shared calculation changes. They
are bounded existing comparisons, not a new full legacy/H3 obligation.

For follow-up observation create one supported variant in the existing ignored output directory.
The four shapes use `stone-court` with initial high seed73 for `sticky`/`second-death`, seed55 for
`ally-death`, and `river-post` with seed55 for `counter`. All use low seed word0x1234, unchanged
placements and Stay AI. This is controlled startup input; the running session is never reseeded.

```powershell
$shape = 'sticky' # counter, ally-death, second-death
$packageName = if ($shape -eq 'counter') { 'river-post' } else { 'stone-court' }
$initialSeed = if ($shape -in @('sticky', 'second-death')) { 73 } else { 55 }
$data = Get-Content -Raw -LiteralPath "remake/content/authored/$packageName.json" | ConvertFrom-Json -AsHashtable
$data.start.mainSeed = [uint32]($initialSeed * 65536 + 4660)
foreach ($index in @(0, 2)) { $data.start.actors[$index].hp = 500; $data.actors[$index].maxHp = 500 }
$data.actors[2].attack = if ($shape -eq 'counter') { 26 } else { 18 }
if ($shape -eq 'ally-death') {
    $data.start.actors[0].hp = 1; $data.start.actors[0].exp = 99; $data.start.actors[0].defeats = 6
}
if ($shape -eq 'second-death') { $data.start.actors[2].hp = 35 }
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'package.json'
$data | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'followups.json'
$env:SF2_OBSERVATION_CASE = 'followups'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
$env:SF2_OBSERVATION_FOLLOWUP_SHAPE = $shape
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Use a fresh output directory per shape and run serially in the same installed project. The observer
presses actual Enter/X/Tab/Space, checks Actor/Target reversal and ordered first/second/counter
observations, exact draw count/seed/resources, death visibility/accounting and subsequent control.
The ally-death shape checks that EXP99 stays99 and no award draws occur. The second-death shape
checks cancellation of the first hit's already-set counter. The observer is executed directly,
without tests of the observer or helper.

The same direct script also accepts `SF2_OBSERVATION_CASE=target-cycle` and `SF2_OBSERVATION_CASE=layout`.
It sets a representative 960×540 host window because a headless SceneTree script otherwise starts a
64×64 physical window. The layout case resizes the actual window to 640×480 and 1280×720, checks real
map/HUD and actor/preview rectangles, scrolls the HUD with real mouse-wheel events and commits movement
with real keys. `SF2_OBSERVATION_LONG_PATH=1` additionally traverses a valid long preview before that observation.

Prepare minimal public authored variants in a fresh ignored output directory, without editing packages:

```powershell
$caseDirectory = Join-Path $env:SF2_RUN_OUTPUT 'inputs'
New-Item -ItemType Directory -Path $caseDirectory | Out-Null
$cycle = Get-Content -LiteralPath 'remake/content/authored/practice-yard.json' -Raw | ConvertFrom-Json
$laterAlly = $cycle.actors[1] | ConvertTo-Json -Depth 8 | ConvertFrom-Json
$laterAlly.id = 'guard-c'
$laterAlly.agility = 1
$cycle.actors += $laterAlly
$laterStart = $cycle.start.actors[1] | ConvertTo-Json -Depth 8 | ConvertFrom-Json
$laterStart.actor = 'guard-c'
$cycle.start.actors += $laterStart
$cycle.encounters[0].placements += [pscustomobject]@{actor='guard-c'; faction='ally'; processingOrder=10; control='player'; aiStrategy=$null; x=3; y=4}
$cycle | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath (Join-Path $caseDirectory 'three-allies.json') -Encoding utf8NoBOM
$layout = Get-Content -LiteralPath 'remake/content/authored/practice-yard.json' -Raw | ConvertFrom-Json
$layout.terrains[0].rows = @(1..48 | ForEach-Object { 'g' * 48 })
$layout.actors[0].move = 255
$layout.encounters[0].placements[0].x = 47
$layout | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath (Join-Path $caseDirectory 'full-square.json') -Encoding utf8NoBOM
```

Use those respective paths with `--authored-package`, the selected observation case and a fresh
`SF2_OBSERVATION_OUTPUT` path. The targeting variant places the rejected ally before a legal adjacent
ally; the layout variant spans both admitted dimensions. Both execute the same production session and view,
with no runtime seed/state injection. Preserve original failed observations and run only the affected
case after a correction; adapter-only fixes do not require replaying unchanged engine/reference suites.

Private trust remains in the transitional reference reader until its capability migrates. Its affected
direct comparison is the existing `PrivateCanonicalMap3ImportReaderTests.UnknownShapeAndProvenanceDriftFailClosed`
case, selected alone from the Content.Tests project; it passes after relocation without selecting the
old aggregate or adding a dependency from Engine.Tests to Reference. Do not turn this selected reference
check into an automatic legacy-suite obligation for every new engine change.
The extracted weighted rule also retains the selected Domain.Tests
`Battle01PlayerMovementTests.WeightedPropagationMatchesTheAcceptedRuntimeMatrix` comparison, including
its flat-row and bucket-wrap cases. It passes through the reference wrapper; the authored engine's
logical row-edge behavior has its own actual movement unit assertion. No original fixture changed.

### Semantic Terrain Observation

Format-v7 terrain rows reference explicit local `legend` definitions. Current packages use
`g = {surface: open, protection: light}`, `p = {surface: open, protection: none}`,
`b = {surface: brush, protection: heavy}` and `# = {surface: barrier, protection: none}` where used.
The weighted AI recipe explicitly adds `d = {surface: deep, protection: heavy}` before using that glyph.
These are authored references, not original terrain indexes; missing glyph definitions reject.

`BattleTerrainTests` varies surface through actual preview/cancel/commit, and target protection through
AI lethality selection and physical settlement. Existing movement, critical and moved-original-actor
counter cases preserve their independent cost/HP/RNG expectations. Run the four package recipes and
the enemy/target/continuation variants below with the existing probe, including weighted, occupied,
unreachable and startup shapes. Require complete checkpoints, actual input/state and clean logs.
No new native branch, screenshot or state setter is needed for these consumers.

Run affected references together: `WeightedPropagationMatchesTheAcceptedRuntimeMatrix` (all five
controlled source cases), `ClassZeroUsesSourceRegularCostsWithBowiesTwelvePointBudgetAndObstructedSky`,
`HealerTerrainWeightsChangeBudgetAdmissionAndLandEffectRemainsSeparate`,
`CentaurForestHillsAndDesertCostsChangeReachabilityUnderTheSameBudget`,
`AlliesAreTraversableButOnlyVacantDestinationsCanBeConfirmedAndEnemiesBlockPropagation`,
`ActualOrderIncludesTheInactivePrefixOccupiedFallbackAndBothCommandsets`,
`SourceDestinationFailureCompletesOriginStayAndKeepsTheThinkingHistory`,
`RadiusSearchUsesStrictLowerCostFirstTieAndOwnCellZeroBeforeOccupancy`,
`SourceArithmeticKeepsZeroIntermediateAndBothDownwardDrawsAtTheOriginalRange`,
`HoveringTerrainZeroUsesUnreducedDamageAndTheSameLethalEarlyReturn`,
`CounterHalvesBeforeSpreadAndConsumesItsOwnFlagsWithoutAnotherAttack`, and
`ScriptThreeAndSelectionRetainLethalityBranchClassCohortAndMovementTieOrder`.
Source mover tables, occupancy bits and explicit flat-row behavior stay at the real reference
projection into the same kernel. These bounded comparisons do not complete private battle
continuation, broader mover admission or full ADR0009/0010 acceptance.

### Independent Control and AI Strategy

Current format-v7 `encounters[].placements[]` owns both choices, separately from actor capabilities:

| control | aiStrategy | Executed behavior |
| --- | --- | --- |
| `player` | `null` | Application yields actual player input; currently ally-only. |
| `automatic` | `stay` | Explicitly consume the automatic entry without attack or movement; currently enemy-only. |
| `automatic` | `attack-then-approach` | Attempt the supported physical attack; if none can be selected, run the accepted approach or origin-Stay continuation and end the action. |

Missing or incompatible pairs reject; no unknown strategy defaults to Stay. The existing engine
control/AI tests exercise shared actor definitions under distinct encounter assignments, actual
player/automatic results, invalid Content and reusable starts. Existing enemy, target-selection and
commandset-continuation cases retain independent damage, queue, memory and seed expectations.

Use the player HEAL/STAY and physical recipes plus the enemy-actions, target-selection and
commandset-continuation variants below with the retained editor/project. Their configuration selects
`placements[2].aiStrategy`; source command observations remain ATTACK1/script3, HEAL1, SUPPORT and MOVE1.
Observe complete startup, attack/counter, no-target pursuit, occupied origin Stay, next input and
Unsupported atomicity checkpoints with clean logs. The existing native probe is unchanged.

Run related control/target/continuation references as one selected group, including
`ActualOrderIncludesTheInactivePrefixOccupiedFallbackAndBothCommandsets`,
`SourceDestinationFailureCompletesOriginStayAndKeepsTheThinkingHistory`,
`RadiusSearchUsesStrictLowerCostFirstTieAndOwnCellZeroBeforeOccupancy`,
`ScriptThreeAndSelectionRetainLethalityBranchClassCohortAndMovementTieOrder`,
`CompletedEnemyPrefixHandsOnlyActualBowieHisOwnRegularRangeAndCancelOrigin`, and
`CounterReversesRolesAndCommitsPrimaryThenCounterAndExpAsOneEnemyReceipt`.
These comparisons preserve their source domains; they do not admit activation, wider AI or private
initialized entry and do not constitute complete ADR0009 acceptance.

### Physical Critical Configuration

Format-v7 physical definitions explicitly pair `critical.chance` and `critical.damageBonus`.
The stone package uses `one-in-16` / `quarter`; the river package uses `one-in-32` / `half`.
The existing physical, follow-up and enemy-action recipes select these fields directly and use the
same installed Godot project/probe. For this configuration boundary, observe both ordinary physical
packages, all four follow-up shapes (`sticky`, `counter`, `ally-death`, `second-death`), and the enemy
`counter` shape with both packages. Require all38 checkpoints across those eight cases and clean logs.
No new native branch, screenshot or runtime seed setter is needed.

`PhysicalRuleConfigurationTests` also varies both rules for the same player/enemy attack, preserving
independent damage/RNG/reward values and checking the reversed counter's own critical range. Its
controlled start gives critical seed1 after the ordinary round and dodge; the next spread words20/267
both yield zero. Base78 therefore becomes117 for a half bonus or97 for a quarter bonus. Existing
seed55 counter expectations remain; the test does not derive expectations from product output.

Run the affected reference group together: `SourceArithmeticKeepsZeroIntermediateAndBothDownwardDrawsAtTheOriginalRange`,
`MissAndCriticalUseRealSeedsAndPreserveSourceCallOrder`,
`CounterHalvesBeforeSpreadAndConsumesItsOwnFlagsWithoutAnotherAttack`,
`CounterReversesRolesAndCommitsPrimaryThenCounterAndExpAsOneEnemyReceipt`,
`SecondDefeatPreservesTheFirstCorpseAndCreditsOnlyTheNewTarget`,
`RealSeedsExerciseMissCriticalAndIndependentExpVariance` and
`KillAccountingUsesSourceCapsAndKeepsUnknownInputsUnknown`. These existing tests compare real scalar,
reaction, reward and death boundaries without a reference aggregate or private input regeneration.

### Enemy action observation

The enemy branch uses the same reader, session and existing native observer. Start from either
physical package and prepare this controlled input in an ignored worktree-local output directory:

```powershell
$packageName = 'stone-court' # river-post also supports the counter shape
$shape = 'counter' # movement, enemy-death, unsupported use stone-court
$data = Get-Content -Raw -LiteralPath "remake/content/authored/$packageName.json" | ConvertFrom-Json -AsHashtable
$data.start.mainSeed = [uint32](55 * 65536 + 4660)
foreach ($index in @(0, 2)) {
    $data.start.actors[$index].hp = 500
    $data.actors[$index].maxHp = 500
    $data.actors[$index].defense = 4
}
$data.actors[0].attack = 18
$data.actors[0].physical.critical = @{chance='one-in-32'; damageBonus='half'}
$data.actors[2].attack = 30
$data.actors[2].physical.critical = @{chance='one-in-16'; damageBonus='quarter'}
$data.encounters[0].placements[2].aiStrategy = 'attack-then-approach'
$data.actors[2].move = 1
if ($shape -eq 'movement') {
    $data.actors[2].move = 3
    $data.encounters[0].placements[2].x = 7
    $data.encounters[0].placements[3].x = 6
    $data.encounters[0].placements[3].y = 5
}
if ($shape -in @('enemy-death', 'unsupported')) { $data.start.actors[2].hp = 1 }
if ($shape -eq 'unsupported') { $data.start.actors[0].exp = 99 }
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'enemy-input.json'
$data | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'enemy-observation.json'
$env:SF2_OBSERVATION_CASE = 'enemy-actions'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
$env:SF2_OBSERVATION_ENEMY_SHAPE = $shape
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

The four checkpoints cover initial state, real player STAY selection, automatic enemy action and
stable next control/Unsupported. The counter hits the ally for22 and the enemy for7, awards the
ally1 EXP, and carries main0x557E1234/thinking0x02EF0042. Counter kill instead awards24 EXP and19 gold,
removes the enemy node and ends at main0xB1BC1234. The late level-up shape preserves all enemy-action
state while retaining the preceding player's committed turn. Movement chooses (4,3) from (7,3).
These are reference expectations for the supplied configurations, never production dispatch rules.
Inspect native logs for script/process errors and require all four checkpoints, as well as the
reported pass and exit status; a script that stops before its final checkpoint is incomplete.

`EnemyActionTests` owns independent behavior checks for changed targets/configurations, continued
history, dead secondary queue entries, automatic startup and per-enemy failure boundaries. For the
shared thinking calculation, select existing reference methods
`SignedRangeBoundariesStillAdvanceTheHighByteOnce` and `HighByteSignedEdgesMatchTheExistingThinkingHelper`.
`ScriptThreeAndSelectionRetainLethalityBranchClassCohortAndMovementTieOrder` and the affected scalar/
counter methods above retain the existing source comparison. No new H3 or legacy aggregate is required.

### Multi-target selection observation

Use the same installed editor, project and fresh ignored output with competing configured actors:

```powershell
$packageName = 'stone-court' # river-post also supports secondary
$shape = 'secondary' # primary, movement, missing-class use stone-court
$data = Get-Content -Raw -LiteralPath "remake/content/authored/$packageName.json" | ConvertFrom-Json -AsHashtable
$data.start.mainSeed = [uint32](55 * 65536 + 4660)
$data.start.thinkingSeed = [uint32]0x00EF0042
foreach ($index in @(0, 1, 2)) {
    $data.start.actors[$index].hp = 500
    $data.actors[$index].maxHp = 500
    $data.actors[$index].defense = 4
    $data.actors[$index].attack = if ($index -eq 2) { 30 } else { 18 }
    $data.actors[$index].physical.critical = if ($index -eq 2) { @{chance='one-in-16'; damageBonus='quarter'} } else { @{chance='one-in-32'; damageBonus='half'} }
}
$data.actors[0].classRule = if ($shape -eq 'primary') { 'unpromoted-swordsman' } else { 'unpromoted-warrior' }
$data.actors[1].classRule = if ($shape -eq 'primary') { 'unpromoted-warrior' } else { 'unpromoted-swordsman' }
$data.encounters[0].placements[2].aiStrategy = 'attack-then-approach'
$data.actors[2].move = 1
$data.encounters[0].placements[1].x = $data.encounters[0].placements[2].x - 1
$data.encounters[0].placements[1].y = $data.encounters[0].placements[2].y
if ($shape -eq 'movement') {
    $data.actors[0].classRule = 'ordinary'
    $data.actors[1].classRule = 'ordinary'
    $data.actors[2].move = 8
    $data.encounters[0].placements[0].x = 1; $data.encounters[0].placements[0].y = 1
    $data.encounters[0].placements[1].x = 1; $data.encounters[0].placements[1].y = 4
    $data.encounters[0].placements[2].x = 7; $data.encounters[0].placements[2].y = 3
}
if ($shape -eq 'missing-class') { $data.actors[0].classRule = 'ordinary' }
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'targets-input.json'
$data | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'targets-observation.json'
$env:SF2_OBSERVATION_CASE = 'target-selection'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
$env:SF2_OBSERVATION_TARGET_SHAPE = $shape
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Both adjacent candidates consume thinking draws in reverse processing order: 1 then2 produce raw19 each,
reported cap15, so changing only the class definitions changes the chosen actor. The movement shape
has costs12/14 and priority1, selecting the farther target without requiring class metadata. Each
successful shape has five checkpoints through actual player commands into round2: main RNG carries
0x557E1234 → 0x97231234 → 0xE0E11234, thinking carries0x02EF0042 → 0x01EF0042, and the second enemy
action selects the primary actor from the evolving candidates. Missing class in the critical cohort
has four checkpoints and preserves the entire failed enemy action. Require the exact checkpoint
count, clean logs and successful result. JSON numbers are floats in GDScript; nested expected numeric
arrays must preserve that representation rather than failing an otherwise equal observed value.

`TargetSelectionTests` owns actual rule/content/session expectations, including raw/capped and
signed-byte edges, class tables, same-class/equal-movement ties, reordered configuration/processing orders and
atomic missing-data/late-settlement rejection. Direct affected reference selections are
`ScriptThreeAndSelectionRetainLethalityBranchClassCohortAndMovementTieOrder`,
`ActualRoundSixAttackReplaysHpOnceAndAdvancesToBowieWithAllRandomChannels`,
`ActualRoundEightChesterHitReplaysTheSelectedProfileAndPreservesFirstDefeatAccounting`,
`DamagedEnemyAfterChesterAttackSelectsBowieAndPreservesEarnedExp`,
`CounterReversesRolesAndCommitsPrimaryThenCounterAndExpAsOneEnemyReceipt` and
`PostHealBowieCounterUsesItsOwnPermissionAndOneEnemyReceipt`. These execute the retained real ranking
consumers through the shared selector; no reference runner or native-probe tests are added.

### Commandset06 continuation observation

Use the existing installed editor/project and fresh ignored output. Both physical packages support
`move-attack`; the other shapes below use `stone-court`. No live state or seed setter is used.

```powershell
$packageName = 'stone-court' # river-post also supports move-attack
$shape = 'move-attack' # occupied, unreachable, startup, weighted, secondary
$data = Get-Content -Raw -LiteralPath "remake/content/authored/$packageName.json" | ConvertFrom-Json -AsHashtable
$data.start.mainSeed = [uint32](42 * 65536 + 4660)
foreach ($index in @(0, 2)) {
    $data.start.actors[$index].hp = 500
    $data.actors[$index].maxHp = 500
    $data.actors[$index].defense = 4
    $data.actors[$index].attack = if ($index -eq 2) { 30 } else { 18 }
    $data.actors[$index].physical.critical = if ($index -eq 2) { @{chance='one-in-16'; damageBonus='quarter'} } else { @{chance='one-in-32'; damageBonus='half'} }
}
$data.encounters[0].placements[2].aiStrategy = 'attack-then-approach'
$data.actors[2].move = 3
$placements = $data.encounters[0].placements
$placements[0].x = 1
$placements[0].y = if ($packageName -eq 'stone-court') { 3 } else { 4 }
$placements[2].x = 7
$placements[2].y = $placements[0].y
if ($shape -eq 'occupied') {
    $data.actors[2].move = 1
    $placements[3].x = 6; $placements[3].y = 3
}
if ($shape -eq 'unreachable') {
    foreach ($y in 1..5) { $data.terrains[0].rows[$y] = '#pp#pppp#' }
}
if ($shape -eq 'startup') { $data.actors[2].agility = 60 }
if ($shape -eq 'secondary') { $placements[0].y = 1; $placements[1].y = 3 }
if ($shape -eq 'weighted') {
    $data.actors[2].move = 1
    $placements[0].x = 4; $placements[0].y = 3
    $placements[1].x = 7; $placements[1].y = 1
    $placements[3].x = 2; $placements[3].y = 5
    $data.terrains[0].legend.d = @{surface='deep'; protection='heavy'}
    foreach ($y in 1..2) { $data.terrains[0].rows[$y] = '#ppppppd#' }
}
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'commandset-package.json'
$data | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'commandset-observation.json'
$env:SF2_OBSERVATION_CASE = 'commandset-continuation'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
$env:SF2_OBSERVATION_CONTINUATION_SHAPE = $shape
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Require exit0, `passed: true`, clean logs without script errors, and all checkpoints: six for
`move-attack`, three for `startup`, four for each other shape. The ordinary move ends at x5 after
fixed cost4 and keeps main0xDC7F1234/thinking0xBEEF0042. Next-round input produces main0xEE281234,
then ATTACK1 at x2 with allyHP478/enemyHP493/allyEXP1 and final main0xDAA61234/thinking0x02EF0042.
Occupied MOV1 ends at origin x7 but MOVE1 returns0. Weighted costs prefer the farther swordsman and
correct the station to x6. Changed positions select the secondary actor without a thinking draw.
Unreachable costs preserve the player commit and failed enemy queue entry as Unsupported.

`CommandsetContinuationTests` owns Content/session behavior, source walk/unsigned-cost boundaries,
missing rewards at the reached attack, and the independently specified continued RNG results.
Affected actual reference methods are `ActualOrderIncludesTheInactivePrefixOccupiedFallbackAndBothCommandsets`,
`RepeatedPursuitStopsAtTheActualRoundSixAttackCohortBeforeAnyMutation`,
`SourceDestinationFailureCompletesOriginStayAndKeepsTheThinkingHistory`,
`RadiusSearchUsesStrictLowerCostFirstTieAndOwnCellZeroBeforeOccupancy`,
`PreliminaryWalkRetainsAccumulatedBitsAndSupportsAValidEmptyPath`,
`IncompleteOrHighTargetCostsRejectBeforeTheDisputedClassBranch`,
`LaterPursuitAcceptsTheRewoundPhysicalMainSeedAndRetainsDamagedAlly`,
`FirstAllyDefeatRemovesChesterFromBlockingAndBothTargetCohorts`,
`SourceDirectionMaskRetainsAnEarlierHigherCostBranchAndRejectsAnIncompletePath`,
`RemainingEnemiesUseTheirOwnMemoryAndEvolvingOccupancyBeforeActualBowieEntry` and
`ActualRoundSixAttackReplaysHpOnceAndAdvancesToBowieWithAllRandomChannels` in the existing Domain test
project. Select those methods with `dotnet test --filter`; no new helper/probe tests, H3 replay or
legacy aggregate is needed. The committed planner and engine/adapter entries remain the gate owners.

## Optional Private .NET Checks

Existing legacy checks use xUnit's explicit private-input selection. This is their current command
contract, not a retention obligation for every method during engine migration.

| Existing check family | Required selections when that check is actually needed |
| --- | --- |
| Canonical import | `SF2_PRIVATE_CANONICAL_MAP_IMPORT` |
| Original map visual payload | `SF2_PRIVATE_ROM`, `SF2_PRIVATE_MAP_TILESET_METADATA`, `SF2_PRIVATE_MAP_PALETTE_METADATA` |
| Local presentation catalog/atlas | `SF2_PRIVATE_PRESENTATION_ASSET_ROOT`; canonical import where the projection consumes it |
| Battle01 startup | `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE`, `SF2_PRIVATE_BATTLE01_TERRAIN`; canonical import for connected session checks |

With no selected inputs, an optional check reports **skipped / no private assertions ran**. Partial
selection fails. For a required check set `SF2_REQUIRE_PRIVATE_TESTS=1` before discovery; missing inputs
then fail. `--filter` does not opt into private execution and other switch values do not require it.
Select the actual owning method/project, scope variables to that run and keep logs/output local.
Do not interpret public success with private skips as private acceptance.

[Local Private Inputs](../../docs/operations/local-private-inputs.md) owns registered read-only input
selection. Load it in the same process as any command that needs those inputs. Check the configured
shared ROM and its narrow identity command before reporting it missing; keep configuration, missing
input, identity mismatch, and later upstream/toolchain failure distinct. No copied input is required
solely to repair the appearance of an aggregate result.

## Selected Battle01 Startup Inputs

The current legacy reader consumes the selected Battle01 data and scene JSON exports plus compressed
terrain. Their exact identities remain in [battle01-data](../../manifests/extractions/battle01-data.json),
[battle01-scene](../../manifests/extractions/battle01-scene.json), and the existing Content reader.
These transport identities describe current fixed reference admission; they are not the proposed
general authored-content schema or a gameplay legality condition.

Reuse accepted exports read-only after checking identity. If the owning task actually needs to
reproduce them, the existing `scripts/Export-Battle01Data.ps1` and `Export-Battle01Scene.ps1` consume
the pinned upstream and explicit ignored destinations. New maintained tooling still belongs in
`src/sf2tool/`; this is a frozen compatibility route, not permission to add more PowerShell rails.

The [Map 3 implementation/reference record](./map03-playability-plan.md) retains selected inputs,
exact control histories, source-derived expectations, test method names and completed failures.
Use the named seam needed by a comparison. Its historical instructions to replay prefixes, preserve
every refusal, run full managed suites or inspect frame totals are not current engine acceptance rules.
Fixed traces remain external comparison data; obsolete trace guards and their tests can retire through
the separately owned engine migration. Preserve original H2/H3 goldens and their provenance.

## Repository Planner

On a clean committed head, inspect current selection without executing gates:

```powershell
uv run sf2 verify plan --base origin/main --head HEAD
```

The [planner](../../src/sf2tool/verification_plan.py) automatically selects `engine-unit` and/or
`adapter-build` for ordinary product/test paths and `reference-host-build` for the explicit reference
host/assembly, without the old solution or always-run `public-core`.
Non-research documentation and retired engine-test paths add no execution. Engine test changes select
unit tests; ordinary game changes select ordinary compilation; reference changes select the reference
host. The exact `remake/reference/inputs/battle01-player-ready.json` and `battle01-actions.json` inputs select `engine-unit`: the
existing Engine.Tests project copies it and actual-private engine facts consume it. Reference host
and assembly code still select only `reference-host-build`. Shared product/build inputs select all
actual downstream hosts.

Shared CLI/harness/planner changes retain conservative research selection by default. For a declared
engine-only wiring change in those shared modules,
inspect the actual diff and use:

```powershell
uv run sf2 verify plan --scope engine --base origin/main --head HEAD
```

That explicit scope adds `research-public` direct lint/traceability/index checks for the wiring and
rejects research artifacts or other shared inputs. Path admission cannot detect a semantic research
change inside an allowed module: such a change must retain its research plan/dependency. Unknown
research paths keep conservative fanout. Retired verification tests do not fall back to the old suite.

The existing normal `uv run sf2 verify` command still performs its public stages followed by selected
private identity/provenance stages. It remains the research lane's normal route under its owner.
It is not a new-engine or documentation-only default. Preserve completed stage results and failures;
do not rerun it just to replace a known upstream-availability failure with green.

`uv run sf2 verify --full` remains exceptional for an applicable research milestone, materially shared
evidence-harness change, release boundary, or explicit full-parity request. An ordinary engine slice
does not inherit it. Test/gate wall time and earlier SHA changes do not by themselves invalidate results.

## Retired Official Godot Gate

The former `sf2tool.remake_godot` import/export gate verified only the explicit public-synthetic
reference host, not ordinary game packaging, and is retired together with that profile. Its bounded
native process runner now lives in `sf2tool.bounded_process` for the asset candidate builder. No
maintained gate verifies ordinary package/export contents; claim none without an actually executed,
separately owned export check. Repeated behavior observations use the existing
installation/project/instance.

## Private-Local Smoke Routing

An applicable legacy private observation selects `--path remake/reference/game`, then `private-local`, canonical/content inputs and
the needed mode. It uses the actual affected assembly, a bounded process owner, and checks relevant
session/input/state, errors, exit and cleanup. Private user arguments retain the existing exact
`--name=value` form after Godot's separator; diagnostics do not print private paths.

Reuse sufficient completed observations. A future probe run must follow Scope, including suppressing
image output; the older [native recipe](./presentation-and-assets.md#diagnostic-battle01-launch-and-native-review)
is retained for legacy argument/provenance lookup, not permission to replay its frame checklist.
Do not weaken Content trust or bypass the real Application path to make an observation pass.
Never delete a retained wrapper or failure artifact merely because its run ended.

## GitHub Public

Current [public-checks.yml](../../.github/workflows/public-checks.yml) remains triggered on every pull
request and main push. Its Windows jobs are:

| Job | Current execution |
| --- | --- |
| `scope` | Compare the actual event Git range; fail on a missing/invalid range or failed comparison. Emit the four affected-path booleans. |
| `engine-unit` | Locked restore/build/test of Engine.Tests and its actual production dependencies, using the pinned .NET SDK/runtime. No Python, native Godot, private input or legacy test project. |
| `adapter-build` | Locked restore/build of the ordinary game C# project, which has no Reference/smoke dependency. No Godot tests or native editor launch. |
| `reference-host-build` | Locked restore/build of the explicit Reference.Godot project and its actual production/Reference dependencies; no legacy scenario replay. |
| `research-public` | Locked Python/uv dependencies, Ruff, direct design-contract traceability and research-index checks. No engine or verification-tool pytest families. |

Shared product/build inputs select engine and both hosts; engine tests select engine; `remake/game/`
selects the ordinary adapter. The exact `remake/reference/inputs/battle01-player-ready.json` and `battle01-actions.json` inputs
select engine for its direct Engine.Tests consumer; other `remake/reference/` paths select the
explicit reference-host build. Research/contracts/source/fixtures/manifests/schema inputs select
research. Workflow and shared CLI/harness/planner changes select
all four. Non-research documentation and legacy remake test edits select no product jobs. Other non-remake inputs conservatively select research. The full
predicate lives in the workflow; add a genuinely consumed external unit input when that dependency
is introduced. M1's consumed authored JSON lives under `remake/content/`, which conservatively selects all product/host builds.

Main-gate owns required-check configuration, including integration of `reference-host-build`.
Verify the actual candidate CI/check state at integration; changing configuration remains main-gate
authority. Path-irrelevant jobs report skipped; applicable jobs need a
real successful result. Review the diff and first applicable CI outcome directly, without tests of
job selection, workflow text or the migration. Research/private protections remain independently owned.

## Process, Path, and Artifact Safety

### Worktree Environment and Run Evidence

Reuse the dedicated isolated worktree after its previous topic is merged, tracked state is clean and
owned processes are settled. Start the next topic at accepted main in that worktree. Separate writers
and a specifically required reproduction environment justify another worktree; a slice number does not.

| State | Lifetime |
| --- | --- |
| Python environment and uv cache | Reuse worktree-local `UV_PROJECT_ENVIRONMENT` and `UV_CACHE_DIR`; sync the lock only when needed. |
| NuGet packages and HTTP cache | Reuse worktree-local selections; only SDK-owned CLI state is shared. |
| .NET CLI | Existing absolute `DOTNET_BIN`, shared absolute `DOTNET_CLI_HOME`, forced PATH opt-out at every controlled launch. |
| Godot | Existing verified installation/project/debug instance unless a concrete exception applies. |
| Outputs | Separate logs, receipts and failure evidence for actual new runs; reuse build/import state unless that state is the reason for isolation. |

Keep configuration in the existing ignored worktree environment script. Historical per-slice
launchers are evidence, not current environment setup. No user/machine PATH edits, populated-cache
copies, or environment installation merely because a topic changes. Registered immutable inputs keep
their read-only selection rules. Writable emulator/import/export state stays with its owner.

Use argument lists, bounded timeouts and owned process-tree cleanup. Inspect actual errors as well as
exit status. A zero-exit process or source-only reading does not prove a running adapter observation.
Preserve the first failure and its node/command IDs, logs and process-completion state; after a
correction run only affected engine behavior or directly invalidated verification. No full rerun
solely to replace a completed red aggregate, and no timeout increase as the first fix.

Cleanup is separately authorized work: inspect live references/processes and exact paths, record the
delete/retain list, and preserve private inputs, completed evidence and required reproduction state.
Do not replay earlier cleanup scripts. Keep private/generated inputs, tools, `.godot/`, `bin/`, `obj/`,
exports and receipts out of Git; inspect staged paths and any actually produced export boundary.

## Proportional Gate Guide

| Change | Required kind of evidence |
| --- | --- |
| New-engine rule/state/content behavior | Small meaningful engine unit tests; affected product build; direct reference comparison only for its affected claim |
| Godot input/presentation | Affected adapter build and needed actual-instance observation; no new probe/helper tests or screenshots |
| Verification/probe/planner/report code | Execute and inspect the needed verification directly; add no tests of verification infrastructure |
| Documentation/instructions | Direct document and scope checks, committed planner inspection, honest existing CI outcome |
| Original research or genuinely shared evidence input | Owning research requirements and affected dependencies; keep original evidence/provenance intact |

These scope rules do not waive accepted 8C/H4 evidence. A migration handoff names any pending remote
required-check change and unsupported product capability. Use
[Bounded Inspection and Review](../../docs/operations/bounded-inspection-and-review.md) for exact
identity, semantic review and completed-result handoff.

## Private Initialized Entry Observation

After loading the retained SDK/Godot/worktree environment, select the existing read-only encounter
inputs in `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE` and `SF2_PRIVATE_BATTLE01_TERRAIN`.
Select pinned existing static-data and enemy-promotion exports in `SF2_PRIVATE_STATIC_DATA` and
`SF2_PRIVATE_ENEMY_DATA`, plus the existing enemy-gold export in `SF2_PRIVATE_ENEMY_GOLD`; their identities remain owned by the extraction manifests. If missing,
reproduce them with the existing export owners and explicit ignored destinations from the pinned
read-only source. Never change the registered source or upload exports. The
[trust owner](./runtime-profiles-and-trust.md#private-initialized-common-battle) describes the seven-input
contract and controlled policy.

```powershell
$env:SF2_PRIVATE_CONTROLLED_START = (Resolve-Path -LiteralPath 'remake/reference/inputs/battle01-player-ready.json').Path
$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
foreach ($variable in @('SF2_PRIVATE_BATTLE01_DATA', 'SF2_PRIVATE_BATTLE01_SCENE', 'SF2_PRIVATE_BATTLE01_TERRAIN', 'SF2_PRIVATE_STATIC_DATA', 'SF2_PRIVATE_ENEMY_DATA', 'SF2_PRIVATE_ENEMY_GOLD')) {
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($variable))) { throw "Required private input not selected: $variable" }
}
uv run sf2 verify engine
```

This invokes the actual common-engine private facts. With no private inputs and no mandatory flag,
public CI explicitly skips them; any partial selection or mandatory flag makes missing required inputs
fail. A skipped public result never supplies private acceptance. The real comparison checks initialized
HP/MP/effective versus source ATT, class/movers/equipment/spells, unknown accounting, full first queue,
region words, independent RNG and first-player control against the existing H3 PlayerReady fixture.
It then uses common commands for movement/cancel, the next Centaur player and actual inactive enemy
standby back to Bowie. `PrivateSourceAiTests` continues real player commands through region activation
and set7 pursuit through the first actual enemy attack to player control, then atomic rejection of
a player attack with Unknown EXP.
The comparison preserves source anchors/orders, evolving positions/memory, activation/tested words,
resources/loadouts/unknown accounting, last target and both RNG channels. Meaningful authored variations
cover other IDs, movers, occupancy, memory, commandsets and transaction rejection; no live setter or
legacy session drives this private comparison.

Refresh the affected Debug Godot assembly, then run the existing native observer. The restart is for
the changed startup composition and assembly, using the retained editor/project:

```powershell
& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'private-initialized-observation.json'
$env:SF2_OBSERVATION_CASE = 'private-initialized'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --private-battle-start $env:SF2_PRIVATE_CONTROLLED_START
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'private-source-ai-observation.json'
$env:SF2_OBSERVATION_CASE = 'private-source-ai'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --private-battle-start $env:SF2_PRIVATE_CONTROLLED_START
```

Require all seven initialized-entry checkpoints or all thirteen continuous source-AI checkpoints,
`passed:true`, no failures and clean process logs. The source-AI case includes the entry checks, then
real movement/STAY through activation, set7 pursuit and the first actual physical attack, followed by
stable Unknown-EXP rejection on the selected player attack. These observe actual
Godot input, shared session state, actor nodes, private-origin HUD and explicit Unknown accounting;
no images are emitted. Private output stays ignored. This comparison preserves the fixture's
non-natural R2a→R2b bridge, explicit intro skip and candidate-only missing-word policy. It does not
claim natural map programs, original presentation or a completed battle.

For changes to these shared initialization seams, use one related existing reference group from
`Battle01InitializationTests`, `Battle01FirstRoundTests`, `Battle01FirstControlTests` and
`Battle01PlayerMovementTests`: new-battle data/healing/sourceATT/independent seed; initial, edge and
secondary activation; quad geometry; unsupported spawn; actual candidate/status/word and failed
movement admission; required regular/healer/Centaur and weighted-grid comparisons. Do not replay old
round/receipt aggregates or run new H3 work for this bounded entry. Preserve any completed failure and
rerun its owning group/nodes after correction. The clean committed planner selects engine/adapter;
documentation uses direct links/anchors/fences/tables/examples and scope/private-boundary checks.
For shared standby/pursuit changes, run one related reference group from
`Battle01EnemyStandbyTests` and `Battle01EnemyPursuitTests`: actual first/remaining standby,
immediate idle and packed-memory tables, eligibility/unknown occupancy/source direction masks,
actual inactive/active/set6/set7 ordering, retained activation flags/tested-mask clearing, pursuit
station/fallback/ties and the repeated pursuit’s first physical cohort. Keep the existing reference
profile/history projections; do not run their whole receipt/history mutation aggregates. The actual
common private fact and native continuous-input observation supply acceptance beyond these calculators.
The private action binding and its actual enemy/player/kill/HEAL continuation are implemented at the
bounded scope below. Wider effects, natural map programs and outcome/return retain their stated
Unsupported or Unknown boundaries.


### Private action and spell selection observations

The seven-input private selection additionally binds the existing `enemy-gold-data` export. If absent,
use `uv run sf2 h2 enemy-gold --upstream-path <selected-pinned-source> --output-path <ignored-output>`
with the registered ROM selection; this existing narrow extractor checks source/ROM parity and the
manifest digest. No ROM or generated export is committed or packaged.

After the existing environment setup and Debug adapter build, run:

```powershell
$env:SF2_OBSERVATION_CASE = 'private-actions'
$env:SF2_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'private-actions.json'
$start = (Resolve-Path -LiteralPath 'remake/reference/inputs/battle01-actions.json').Path
& $env:SF2_CASTLE_REVIEW_EDITOR --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --private-battle-start $start
```

Require passed:true, exit0 and five checkpoints: initial state, actual enemy hit, player hit/next
control, first kill/rewards/death/next control, and HEAL/next control. Exact first player and kill
HP/EXP/gold/main/thinking values match the retained `ManualAttackCommitsReceipt52AndActualDispatchReachesRoundSevenPlayerTwo`
and `FirstEnemyDefeatCommitsOrderedAwardsCleanupAndActualPlayerOneControl` reference comparisons.
The older `private-source-ai` case now observes the first hit and a subsequent Unknown-EXP rejection;
its thirteen checkpoints still use the unchanged PlayerReady input.

`PrivateActionBindingTests` covers actual source/effective stats and equipment, source gold, all three
party attackers, exact hovering land reduction, nonleader death/unknown defeat count, HEAL and
lower learned levels. Its explicitly constructed rule seams are unit inputs, not natural reach claims
or running-session setters. Keep natural accounting/seed producers Unknown. The grouped retained
reference comparison selects related physical/counter/kill/defeat/healing/after-turn cases in the
existing Domain test project, without legacy aggregate replay or tests of verification programs.

For G6, make an ignored copy of `practice-yard.json`, set medic-a start HP60, and append a second
learned spell `restore` level2, cost5, range0–2, ordinary heal power30/fullRecovery:false after `mend`.
Run the ordinary authored path with `SF2_OBSERVATION_CASE=spell-selection`. The four-checkpoint probe
uses H twice, observes the selected second spell and HUD, then commits HP90/MP15 and next control.
`SpellSelectionTests` exercises the same independent costs, target reset, cancellation and rejection
semantics. The public package order remains unchanged; diagnostics stay in the external probe.
