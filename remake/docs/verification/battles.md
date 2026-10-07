# Battle and Rule Verification

Select the affected admitted action, rule composition, battle state or scene boundary.

Load the owning worktree's private-input configuration and protected SDK/Godot environment from
[development](../development-and-verification.md#locked-net-workflow) in the same launching process.
Unless a recipe explicitly changes directory, commands run from the repository root. Reuse the
existing instance when suitable and choose fresh ignored outputs. These are focused recipes, not
an aggregate checklist. No screenshots; no new tests of probes/helpers. Read completed failure and
Unknown boundaries before reproducing a claim. Full historical receipts remain in the
[pre-reorganization guide](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/development-and-verification.md).

| Affected behavior | Recipe |
| --- | --- |
| Authored starts and deployment | [Start state](#authored-start-state-observation), [extra turn](#authored-extra-round-action-observation), [faction/order](#authored-faction-and-order-observation) |
| Selected algorithms | [HEAL](#replaceable-heal-observation), [semantic actions](#semantic-action-selection-observation), [automatic decisions](#replaceable-automatic-decision-observation), [progression/outcomes](#replaceable-progression-and-outcome-observation) |
| Ordinary authored battle | [Input/session](#authored-battle-observation), [enemy actions](#enemy-action-observation), [targets](#multi-target-selection-observation), [commandset06](#commandset06-continuation-observation) |
| Private source admission and actions | [Initialized entry](#private-initialized-entry-observation), [spell/action selection](#private-action-and-spell-selection-observations) |
| Actual scene/field consumers | [Message/font](#battle-message-operand-and-font-observation), [HEAL](#heal-scene-verification), [death](#battlefield-death-consumer), [movement](#battlefield-movement-consumer), [Herb](#ordinary-controls-and-reproduction) |

## Replaceable Progression and Outcome Observation

Use the ordinary host and
[`engine_progression_outcome_observation.gd`](../../game/probes/engine_progression_outcome_observation.gd)
with the two small `remake/content/authored/progression-outcome-{victory,defeat}.json` packages.
The observer extends the existing exploration observer, submits actual keys and reads the current
battle/field view plus ordered result signals. No state setter, direct receipt or screenshot supplies
acceptance. Select `RuleCompositions.ForGame()=>Sf2()` or `AuthoredProgressionOutcome()` in C#, then
rebuild/restart the same installed host; both factories use the identical inputs. Restore `Sf2()`
and rebuild before handoff. The EXPECT variables below describe the observer's expectation only.

The victory route interacts on quay, enters arena through its program, acknowledges before text,
then confirms movement, physical action, target and commit. Its leader begins at EXP99/level1 and
HP10, companion dead and enemy HP1. Defeat instead uses Stay and a real automatic lethal attack.
Observe separate construction/HP/reward/growth/death publications, actual outcome dialogue,
living-only victory recovery before explicit reset, join/unlock/completion ordering, return fades,
two settled field reads and one actual west movement. Defeat has no victory tail.

Independent expectations follow the accepted main LCG high-word recurrence `(13*s+7)&65535`,
unchanged low word `0x1234`, six living turn draws for victory/nine for defeat, four lethal strike
draws, source two award draws (authored zero),24 reaction draws and ten growth draws. The authored
curves use cumulative `floor(256*(i+1)/29)` with successive increments and projected-start29.
These produce five +1 gains, keeping current HP/MP10/0 until recovery; they are not values learned
from the observer. Thinking seed stays `0xBEEF0042` in these cases.

| Case | First control seed | Construction seed | Pre-growth seed | Final seed | EXP/level; returned gold |
| --- | --- | --- | --- | --- | --- |
| SF2 victory | 3361018420 | 3657634356 | 3529183796 | 3330085428 | award49;99→148→48/2;84 |
| Authored victory | 3361018420 | 2486768180 | 147919412 | 2535658036 | award7;99→106→6/2;84 |
| Either defeat | 1182405172 | 305009204 | no growth | 2930119220 | no award; source gold36, authored63 |

Run each case serially from the owning checkout with a fresh ignored destination. The controlled
SDK environment comes from [local private inputs](../../../docs/operations/local-private-inputs.md)
and the [locked SDK workflow](../development-and-verification.md#locked-net-workflow). The restart is needed to select compiled policy
and ordinary fixture startup; no separate installation/project is created.

```powershell
$ErrorActionPreference = 'Stop'
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
Push-Location -LiteralPath remake
try {
    & $env:DOTNET_BIN restore game/Sf2.Remake.Godot.csproj --locked-mode
    if ($LASTEXITCODE -ne 0) { throw 'restore failed' }
    & $env:DOTNET_BIN build game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'build failed' }
} finally { Pop-Location }
$policy = 'sf2' # or authored, matching the compiled factory
$outcome = 'victory' # or defeat
$run = Join-Path (Get-Location).Path "local/progression-observation/$policy-$outcome-01"
if (Test-Path -LiteralPath $run) { throw 'Choose a fresh output directory' }
New-Item -ItemType Directory -Path $run | Out-Null
foreach ($key in @('APPDATA','LOCALAPPDATA','TEMP','TMP')) {
    $dir = Join-Path $run $key.ToLowerInvariant()
    New-Item -ItemType Directory -Path $dir | Out-Null
    [Environment]::SetEnvironmentVariable($key,$dir,'Process')
}
$env:SF2_OBSERVATION_EXPECT_POLICY = $policy
$env:SF2_OBSERVATION_EXPECT_OUTCOME = $outcome
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $run 'observation.json'
$package = (Resolve-Path -LiteralPath "remake/content/authored/progression-outcome-$outcome.json").Path
& $godotBinary --headless --path remake/game --log-file (Join-Path $run 'godot.log') `
    --script res://probes/engine_progression_outcome_observation.gd -- --authored-package $package `
    *> (Join-Path $run 'process.log')
$nativeExit = $LASTEXITCODE
$nativeExit | Set-Content -LiteralPath (Join-Path $run 'process.exit') -Encoding utf8NoBOM
```

Require exit0, `passed:true`, no observer failures and no Godot errors in either log. The four bounded
cases pass with current source/factory semantics; this does not establish private HEAL/world/growth
assets or replay the private outcome route. Existing CP2059–2091 and #675/#676 failures remain with
their original owners. Engine behavior assertions cover source caps/spells/equipment, selected
preflight, post-fairy growth, malformed policies and reached-stage retry. Run `uv run sf2 verify engine`
and `uv run sf2 verify adapter`; after a completed failing aggregate, correct with affected files,
retaining the original result. No full Python/H3/H4 or private whole-route run is implied.

Resource boundary: two inputs in tens of KiB, one action per victory or Stay plus automatic action
per defeat,120-second hard observer deadline and under5 MiB expected output per case. Retain stage
samples/events only; a private resource dependency or new observation surface requires replanning.

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
& $godotBinary --headless --path remake/game --log-file (Join-Path $env:SF2_RUN_OUTPUT 'godot.log') --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
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
& $godotBinary --headless --path remake/game --log-file (Join-Path $env:SF2_RUN_OUTPUT 'godot.log') --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
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
& $godotBinary --headless --path remake/game --log-file (Join-Path $env:SF2_RUN_OUTPUT 'godot.log') --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Require construction and completed-scene checkpoints, real target input/node removal/next control and clean process logs.
Apply the same order/array transform after constructing the `secondary` multi-target input below to
observe actual AI selection with sparse orders; its five independent checkpoints and RNG expectations
are unchanged. `BattleFactionOrderTests` separately exercises a low-order enemy, renamed high-order
ally, healing/opposing movement, atomic rejection and rewards. `TurnOrderRulesTests` retains the signed
boundary fixture with `ActorRef` identities, a real order255 and separate null sentinels. The selected
existing reference methods `BaselineTestsThreeRegionsWithoutActivatingThemAndComputesTheAcceptedRound`
and `RoundGenerationRetainsTheIndependentSeedCopyWithoutUsingIt` exercise the actual source projection;
no old aggregate or new H3 observation is required.

## Replaceable HEAL observation

The bounded [HEAL seam](../domain/gameplay-rules.md#implemented-heal-seam) uses the same authored reader,
session query/command path and actual Godot presenter for two C# algorithms. Default direct session
starts keep SF2; both ordinary Godot source-start entries use `RuleCompositions.ForGame()`.
A project author changes that method to `AuthoredHealingA()` or `AuthoredHealingB()`, rebuilds the
Debug adapter and restarts the owned instance because its compiled composition changed. No package
name, environment variable or view branch selects gameplay. Return `ForGame()` to `Sf2()` and rebuild
after the demonstration when default source behavior is wanted.

`remake/content/authored/healing-rule-demo.json` initializes HP30/max40, MP8/cost3, EXP0, power10 and
an adjacent full-HP ally. A recovers10/awards10 with two construction award draws; B recovers5,
awards0 and omits those draws. Both charge MP once and retain the existing later fairy/scene work.
The actual query/target input exposes full-HP legality for A and `target-not-injured` for B. This
demonstrates authored algorithm replacement, not an original rebalance or timing/fidelity claim.

After loading the existing ignored environment in the launching process and choosing the factory:

```powershell
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore
$env:SF2_RUN_OUTPUT = $variantRunDirectory # separate ignored worktree-local A/B destinations
$env:SF2_OBSERVATION_CASE = 'healing-rule'
$env:SF2_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'healing-rule.json'
$packagePath = (Resolve-Path -LiteralPath 'remake/content/authored/healing-rule-demo.json').Path
& $env:GODOT_BIN --log-file (Join-Path $env:SF2_RUN_OUTPUT 'godot.log') --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

The existing observer delegates this responsibility to `engine_healing_rule_observation.gd` to keep
its source below1,000 lines. It drives actual movement/cancel, spell/self/other selection and message
input, waits for actual movement/presenter completion, and reads query/state/HUD/nodes/errors.
Mandatory scene work and completion come from the running consumer; no state setter or synthetic
completion supplies acceptance. It retains named stage changes and semantic events, checks single
MP/HP/turn publication and every carried RNG draw, emits no images, and exits nonzero on failure.
A/B observations pass this bounded path; the ignored reports retain their selected compiled identity
and prior observer failures. They do not prove private source presentation or broader equivalence.
The explicit `--log-file` also keeps Godot's runtime log out of its default Windows user-data location.

Use `uv run sf2 verify engine` for engine behavior and `uv run sf2 verify adapter` for compilation.
During correction, run only the affected behavior filter:

```powershell
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj --no-restore --configuration Release --filter 'FullyQualifiedName~HealingRuleReplacementTests|FullyQualifiedName~HealingRulesTests|FullyQualifiedName~HealingFairyTests|FullyQualifiedName~MedicalHerbTests|FullyQualifiedName~EngineSessionTests|FullyQualifiedName~SpellSelectionTests|FullyQualifiedName~BattleSceneTests'
```

`HealingRuleReplacementTests` covers real Content/session choices, default source assertions,
variant effects, stale actor/history, illegal targets/resources, malformed/throwing rules, missing
private cast support and later failure retention. Existing scene tests retain token/replay and
post-fairy growth-seed assertions. `MedicalHerbTests` independently runs Herb under both selected
spell variants, preserving target legality, capped recovery, inventory, EXP and RNG. Unknown source
EXP remains rejected at preparation; query does not construct an award or spend draws. The completed
engine-suite discovery failure and its focused corrections remain in the slice handoff; no local
whole-suite rerun is required merely to replace that red aggregate record. Preserve the historical
CP2059–2091 cursor FAIL under [HEAL scene verification](#heal-scene-verification).

## Semantic action selection observation

The [implemented semantic actions](../domain/gameplay-rules.md#implemented-semantic-actions) expose finite
Physical, Healing, Item and Stay references through one immutable selected rule composition.
`BattleActionChoiceTests` exercises the actual query, admission, commit and automatic continuation:
ordered disabled candidates, semantic empty slots, unchanged raw inventory words, stale/rejected
commands, changed physical targeting/damage/estimate, source-AI pass-through, malformed results and
later failure retention. Preview never prepares effects or spends RNG. Item/physical source arithmetic
remains independently asserted by the existing behavior and controlled private action assertions.

After loading the protected environment above, use the dedicated engine and adapter gates. A focused
correction can select `BattleActionChoiceTests`, `PhysicalBattleTests`, `PhysicalRuleConfigurationTests`,
`MedicalHerbTests`, `BattleSceneTests`, `SpellSelectionTests`, `EngineSessionTests` and
`BattleOutcomeProgramTests` with the existing .NET test `--filter` option. Private assertions require
the named external inputs; a skipped assertion is not private fidelity acceptance. No broad H4,
original capture, source rebuild or observer test belongs to this bounded action seam.

The existing battle observer delegates physical/follow-up/target/item consumers to
[`engine_action_choice_observation.gd`](../../game/probes/engine_action_choice_observation.gd). It observes
canonical `battleChoices`, actual HUD labels and real input, retains construction facts before HP
publication, then waits for actual presenter stages and player acknowledgements. Final checks retain
HP/MP, EXP, inventory, gold, death projection, queue/next control and ordered carried RNG facts.
Construction and damaging-reaction draws are checked separately. No direct completion API, runtime
state setter or screenshot supplies these observations. Run the four physical package/order cases,
four follow-up shapes and three-allies target-cycle recipe below serially with fresh ignored outputs.
All ten cases, including the item recipe below, pass this bounded source-default consumer boundary.
They do not replace the earlier HEAL A/B demonstration or close original timing/H4 Unknowns.

The physical stage expectations are derived independently from accepted source rules: the high word
advances as `(13 * seed + 7) & 65535`, preserving low word `0x1234`. A damaging reaction has twelve
logical steps with two draws each; four living ordinary actors contribute twelve round-generation
draws. The second lethal action contributes six construction draws, then twenty-four reaction draws.
These counts produce the following expectations for both legal kill orders; they are not copied from
native output. The second action's source damage/award is23/24 for stone-court and27/49 for river-post.

| Stage | Expected seed |
| --- | --- |
| First construction | `0x0A7F1234` |
| First scene complete | `0x41571234` |
| Second round | `0x01231234` |
| Second construction | `0x7AE11234` |
| Second scene complete | `0xCE791234` |

Final EXP is47 for stone-court and97 for river-post. The existing physical behavior assertion checks
these separate stages. Follow-up cases retain their original construction vectors and independently
advance twenty-four draws per damaging hit; source-specific draw counts are not generic rule validators.
Completed early-read and parser failures remain in the slice handoff, with only affected cases rerun.

For item selection, create this minimal startup variant in a fresh ignored `SF2_RUN_OUTPUT` directory:

```powershell
$data = Get-Content -LiteralPath 'remake/content/authored/practice-yard.json' -Raw | ConvertFrom-Json -AsHashtable
$data.items = @(
    @{id=0; name='Recovery leaf'; effect='consumable-healing'; power=10; minimumRange=0; maximumRange=1},
    @{id=6; name='Small remedy'; effect='consumable-healing'; power=4; minimumRange=0; maximumRange=2})
$data.actors[0].items = @(0,6,127,127)
$data.actors[1].items = @(0,127,127,127)
$data.start.actors[0].hp = 60
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'package.json'
$data | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$settingsPath = Join-Path $env:SF2_RUN_OUTPUT 'input-settings.json'
'{"formatVersion":1,"bindings":{"item":{"keys":["J"],"buttons":["Start"],"axes":[]}}}' | Set-Content -LiteralPath $settingsPath -Encoding utf8NoBOM
$env:SF2_OBSERVATION_CASE = 'item-selection'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'observation.json'
& $godotBinary --headless --path remake/game --log-file (Join-Path $env:SF2_RUN_OUTPUT 'godot.log') --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath --input-settings $settingsPath
```

This observes remapped J input, two named slots, an empty-slot rejection, selected range, cancel,
actual consumption/compaction, capped HP recovery, EXP/RNG, HUD help and usable next control. Each
native launch uses the existing protected environment/project/installation and its own worktree-local
output and user-data locations. Preserve failure logs and use a new output directory for corrections.

## Replaceable Automatic Decision Observation

Select `RuleCompositions.AuthoredFirstLegal()` or `AuthoredLowestHp()` in the ordinary `ForGame()`
factory, compile Debug with the existing locked environment, then restart the ordinary host using
one identical [`decision-rule-demo.json`](../../content/authored/decision-rule-demo.json). The demo uses
source physical profiles/terrain/accounting: swordsman/raider/lookout agility63/40/1, allies current
HP400/100 and all three maxHP500. Both candidates are reachable with a real raider movement segment.
Other enemies keep Stay. No runtime plugin/CLI selection or scheduler/view strategy switch is used.
Restore `ForGame()=>Sf2()` and build before freezing a candidate.

Actual behavior owners are `BattleDecisionReplacementTests`, `BattleControlAiTests`,
`SourceEnemyAiTests`, `TargetSelectionTests`, `CommandsetContinuationTests` and
`BattleActionChoiceTests`. They cover same-state replacement, stable ties/changed legal targets,
start binding of later/dead deployments, source reverse thinking/signed priority/class selection,
standby/memory/zero-target pursuit and rejected current transactions after earlier commits. Keep
HEAL release-to-next-AI estimate/preflight/post-decision failures and late preflight/queue retention.
Run `uv run sf2 verify engine`, `uv run sf2 verify adapter`; after a completed failure rerun its nodes
and the affected scope instead of repeating an aggregate to replace the failure record.

After configuration and the selected Debug build, use a fresh worktree-local case directory and the
existing actual input/state observer:

```powershell
$env:SF2_OBSERVATION_CASE = 'decision-replacement'
$env:SF2_OBSERVATION_EXPECT_TARGET = 'swordsman' # 'lookout' for LowestHp
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'observation.json'
$packagePath = (Resolve-Path -LiteralPath 'remake/content/authored/decision-rule-demo.json').Path
& $godotBinary --headless --path remake/game --log-file (Join-Path $env:SF2_RUN_OUTPUT 'godot.log') --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

The bounded `engine_decision_rule_observation.gd` reuses the actual action consumer's presenter
settling and result events. Real Enter/Space/Enter commits the player's Stay, one selected decision
starts movement, presenter completions deliver its segments, then physical construction, damage,
counter/reward and next player input settle. Expected variants exist only in the external selector.
Inspect actual stage/result/field-node readback and errors; screenshots are prohibited.

Independent expectations come from the accepted source scalar rules: high seed55 followed by five
ordinary three-draw round candidates gives main0x00C01234. Authored priorities draw no thinking RNG.
The physical first hit does1 damage; the live counter does13 from swordsman ATT30 or1 from lookout
ATT5, awards1 EXP and leaves gold100. Fourteen construction draws give0x557E1234; two damaging scene
bodies add24 draws each, giving final0x976E1234. Thus FirstLegal yields ally HP399/100 and raider487;
LowestHp yields400/99 and raider499. The selected target's memory, one decision, actual movement,
one raider action/queue consumption and no construction before movement completion are observed.
An early PresentationWait sample is not the final result.

After restoring source composition, run only the affected [enemy movement/counter recipes](#enemy-action-observation).
Their expected construction seed remains0x557E1234; actual scene-complete seed is0x976E1234 after48
reaction draws. The observer now retains the full real event chain and waits for settled next input;
phase samples supplement the four named initial/ready/result/stable checkpoints.

For affected private binding/source AI, select the retained six admitted battle inputs in the same
launch process and the tracked `remake/reference/inputs/battle01-player-ready.json` controlled start.
Run `PrivateSourceAiTests` and the affected `PrivateBattleScenarioTests` nodes directly. The latter's
reader has no HEAL scene sidecar: SelectSpell succeeds, SelectTarget rejects healing-scene-content
with Unsupported and the same snapshot/no observations, then Cancel allows the original standby
trajectory. That case cannot reach the old EXP confirmation without the consumer; existing initialized
HEAL and source-AI physical cases retain their own reached unknown-accounting boundaries. No new
source collection/export, private HEAL success, full route or fidelity claim follows.

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
initially selected, Tab for living allied targets, Space for STAY and Escape for cancel. Physical attack selection defaults to F; Tab then selects living opponents. Packages without physical
definitions report Unsupported. Application runs AI and rounds automatically.

Tab cycles from the last attempted UI candidate, including a rejected one. A range rejection preserves
the session's accepted target and battle state; subsequent Tab input can reach later living targets of the selected action. The HUD
shows both attempted and accepted target after rejection. Cancel or the next action clears the UI cursor.
The map viewport keeps the acting origin, preview and target visible with clipping/pan/zoom. Wide
windows place a scrollable HUD beside the map; narrow windows place it below. Authored UI uses actual
window dimensions.

From the repository root, the direct observation command is:

```powershell
$packageName = 'practice-yard' # or garden-watch
$packagePath = (Resolve-Path -LiteralPath "remake/content/authored/$packageName.json").Path
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'observation.json'
$env:SF2_OBSERVATION_CASE = ''
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --log-file (Join-Path $env:SF2_RUN_OUTPUT 'godot.log') --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
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
& $godotBinary --headless --path remake/game --log-file (Join-Path $env:SF2_RUN_OUTPUT 'godot.log') --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Repeat with `river-post.json`; `SF2_OBSERVATION_REVERSE_KILLS=1` selects the opposite legal kill order. The observation
uses actual F/Tab/Enter/Space input, including a rejected distant target followed by a valid target,
then checks exact carried RNG/resources, removed coordinates/nodes, next living control and the next
round. All four package/order combinations use common product commands. Reached unsupported
level/leader/outcome atomicity and independent arithmetic expectations belong to
`PhysicalBattleTests`, not tests of this observer. The legacy Domain comparisons of the shared scalar
extraction were retired at M5; shared strike, counter and reward changes use `PhysicalBattleTests`,
`PhysicalRuleConfigurationTests` and, with private inputs required, `PrivateActionBindingTests`.

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
& $godotBinary --headless --path remake/game --log-file (Join-Path $env:SF2_RUN_OUTPUT 'godot.log') --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Use a fresh output directory per shape and run serially in the same installed project. The observer
presses actual Enter/F/Tab/Space, checks Actor/Target reversal and ordered first/second/counter
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

The transitional reference readers, wrappers and test projects are retired. Their
[completed admission/weighted-propagation comparisons](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/development-and-verification.md#authored-battle-observation)
remain historical evidence, not runnable current checks. Current private Content admission and
`WeightedMovementReferenceTests` / `BattleMovementTests` protect the consumed input/rule boundaries;
original flat-storage behavior and the authored logical row edge retain their stated distinction.

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
| `automatic` | private `source-orders` | Bind source standby/activation/set6/set7 with retained source admission guards. |
| `automatic` | `authored-priority` in the explicit authored composition | FirstLegal or LowestHp selects a legal physical target through the same movement/action path. |

Missing or incompatible pairs reject; all current/later/dead deployments resolve before session start.
Unknown/duplicate/missing bindings reject, and no unknown strategy defaults to Stay. The existing engine
control/AI tests exercise shared actor definitions under distinct encounter assignments, actual
player/automatic results, invalid Content and reusable starts. Existing enemy, target-selection and
commandset-continuation cases retain independent damage, queue, memory and seed expectations.

Use the player HEAL/STAY and physical recipes plus the enemy-actions, target-selection and
commandset-continuation variants below with the retained editor/project. Their configuration selects
`placements[2].aiStrategy`; source command observations remain ATTACK1/script3, HEAL1, SUPPORT and MOVE1.
Observe complete startup, attack/counter, no-target pursuit, occupied origin Stay, next input and
Unsupported atomicity checkpoints with clean logs. Reuse the existing probe and bounded action/decision
consumer helpers for real scene settling; intermediate PresentationWait is not a finished action.

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
& $godotBinary --headless --path remake/game --log-file (Join-Path $env:SF2_RUN_OUTPUT 'godot.log') --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

The four checkpoints cover initial state, real player STAY selection, automatic enemy action and
stable next control/Unsupported. The counter hits the ally for22 and the enemy for7, awards the
ally1 EXP, constructs at main0x557E1234 and completes two damaging scene bodies at
main0x976E1234/thinking0x02EF0042. Counter kill instead awards24 EXP and19 gold,
removes the enemy node and constructs at main0xB1BC1234 before the damaging scene bodies. The late level-up shape preserves all enemy-action
state while retaining the preceding player's committed turn. Movement chooses (4,3) from (7,3).
These are reference expectations for the supplied configurations, never production dispatch rules.
Inspect native logs for script/process errors and require all four named checkpoints plus the real
staged completion/event samples, as well as the
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
& $godotBinary --headless --path remake/game --log-file (Join-Path $env:SF2_RUN_OUTPUT 'godot.log') --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
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
& $godotBinary --headless --path remake/game --log-file (Join-Path $env:SF2_RUN_OUTPUT 'godot.log') --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
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

## Private Initialized Entry Observation

After loading the retained SDK/Godot/worktree environment, select the existing read-only encounter
inputs in `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE` and `SF2_PRIVATE_BATTLE01_TERRAIN`.
Select pinned existing static-data and enemy-promotion exports in `SF2_PRIVATE_STATIC_DATA` and
`SF2_PRIVATE_ENEMY_DATA`, plus the existing enemy-gold export in `SF2_PRIVATE_ENEMY_GOLD`; their identities remain owned by the extraction manifests. If missing,
reproduce them with the existing export owners and explicit ignored destinations from the pinned
read-only source. Never change the registered source or upload exports. The
[trust owner](../content/profiles-and-trust.md#private-initialized-common-battle) describes the seven-input
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

For changes to these shared initialization seams, run `PrivateBattleInitializationTests`,
`PrivateBattleScenarioTests` and the affected movement/turn-order engine tests with private inputs
required. For shared standby/pursuit changes, run `SourceEnemyAiTests` and `PrivateSourceAiTests`.
The legacy reference groups that previously compared these calculators were retired at M5; the actual
common private facts and the native continuous-input observation supply acceptance. Do not run new H3
work for this bounded entry. Preserve any completed failure and rerun its owning nodes after
correction. The clean committed planner selects engine/adapter; documentation uses direct
links/anchors/fences/tables/examples and scope/private-boundary checks.
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
or running-session setters. Keep natural accounting/seed producers Unknown. The former grouped
reference comparison in the legacy Domain test project was retired at M5.

For G6, make an ignored copy of `practice-yard.json`, set medic-a start HP60, and append a second
learned spell `restore` level2, cost5, range0–2, ordinary heal power30/fullRecovery:false after `mend`.
Run the ordinary authored path with `SF2_OBSERVATION_CASE=spell-selection`. The four-checkpoint probe
uses H twice, observes the selected second spell and HUD, then commits HP90/MP15 and next control.
`SpellSelectionTests` exercises the same independent costs, target reset, cancellation and rejection
semantics. The public package order remains unchanged; diagnostics stay in the external probe.

## Battle message operand and font observation

`BattleSceneView.Observe()` retains `reactionAmount` directly from the current scene's `Amount`.
This is the raw reaction/message operand, not a damage calculation or the effective HP difference.
An overkill result can therefore retain damage greater than the target's remaining HP. The existing
message, phase, wait token and visible-character fields identify the corresponding mounted Label.
`messageFont` reads that Label's resolved settings/theme font and size, its visibility, resource class,
and ordered font-cache face identities (family, style, face index and system-fallback permission).
The existing battle scene probe retains these fields through its generic scene projection.

The [Font API](https://docs.godotengine.org/en/4.7/classes/class_font.html) and
[TextServer API](https://docs.godotengine.org/en/4.7/classes/class_textserver.html) supply those face
identities; an available file or opaque RID does not identify the mounted consumer. These are the
resolved configured font/fallback caches. **Unknown:** which face, including an automatic system
fallback, shapes each individual character; the public Label API does not expose its shaped buffers.
Modern font/layout remains permitted. Exact shaped glyph runs, original bitmaps and pixels are not
new acceptance gates. Observation performs no input, logical update or RNG operation.

For the bounded direct check, load the current ignored private configuration in the same launching
process. Reuse the registered installations and owning project. A Debug rebuild/restart is needed
to load the changed adapter when no suitable owned instance is running:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify adapter
# The local launcher uses shared_dotnet_environment for Debug/native child launches.
uv run python -X utf8 local/issue534/battle-message-observation-01/run.py build
uv run python -X utf8 local/issue534/battle-message-observation-01/run.py check
uv run python -X utf8 local/issue534/battle-message-observation-01/run.py lethal <fresh-lethal-run>
uv run python -X utf8 local/issue534/battle-message-observation-01/run.py heal <fresh-heal-run>
```

This local check uses `GameRoot --private-battle-start`, the retained selected private inputs and an
ignored script extending `engine_battle_scene_observation.gd`. It reuses ordinary movement/target/
attack and HEAL input, scene settling, actual node reads and process errors. Only disclosed controlled
ally starts differ: the lethal actor has increased attack/movement and EXP0; HEAL selects the caster
at full HP. Read the raw amount beside the committed HP effect and token. The lethal observation
retains Amount52 against HP5→0; the full-HP HEAL retains recovery0, one MP payment and both message
timeout paths. The mounted consumer reports Open Sans SemiBold / SemiBold, face0, size9, with system
fallback allowed. Consecutive synchronous observation reads preserve session/revision/observation
sequence, actor state, semantic observations and both RNG images. These checks prove the observation
mechanism, not independently calculated original damage or per-character fallback selection.

The direct reader does not provide ally Growth definitions. The completed EXP99 attempt is a
preserved `level-up` Unsupported failure, not an interrupted run or passing growth check. Direct
battle initialization also refreshes HP to maximum; the completed injured-HEAL assertion failure is
preserved, and the corrected window explicitly checks full-HP recovery. No Domain/Content fix or
world-route extension follows from these probe-input limitations. Existing retained typed growth
effects remain the growth evidence dependency; this check supplies no new GrowthMessage witness.

Keep fresh outputs under `local/issue534/battle-message-observation-01`; retain the completed failure
logs, process receipts, actual projections, input/effect stream and read-pair invariants. Do not alter
old captures: new windows cannot backfill six missing lethal operands or the previously unobserved
font of new-A02. The complete binding uses its separately allocated continuous A in the
[comparison route](../evidence/retained-comparisons.md#continuous-text-material-comparison). No partial PASS, subset, count change, full winning
route, screenshot, emulator, export or broad/helper test is part of this observation patch.

## HEAL scene verification

Use the current locked environment/private-input configuration before controlled .NET or Godot
launches, including the shared CLI-home and `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false` policy above.
The bounded HEAL path and its completed original-comparison failure are owned by
[HEAL spell scenes](../godot/battle-scenes.md#heal-spell-scenes). No new original launch,
screenshot, full route, aggregate rerun or asset promotion is needed for this boundary.

The affected engine checks are `HealingRulesTests`, `HealingFairyTests`, `BattleSceneTests`, the
three existing completed-action cases `NaturalHistoriesHealThroughTheSameContentAndSessionPath`,
`SharedDefinitionSupportsIndependentStartsAndNaturalActionHistories`,
`SecondLearnedSpellUsesItsOwnCostAndPowerWithoutReorderingContent`, and `PrivateActionBindingTests`.
`HealingNeutralTimeoutNeedsDeliveryButNoAdditionalAcknowledgement` covers both action/recovery
messages and both readiness orders: exhaust the source65 neutral polls, then complete without any
extra Ack or RNG/update opportunity. The neutral completed-scene helper uses presentation delivery
instead of hiding a missing timeout behind an Ack. Early input-first acknowledgement remains covered
by `TimedInputTestsAcknowledgementBeforeItsNextFairyOpportunity`.
Construction expectations still assert the original two award draws; final assertions follow the
real scene and independently check each main-seed transition. Growth starts from the seed carried
through fairy retirement. `PlayerWait*` in `ExplorationSessionTests` preserves field/plain-text guard
coverage. Existing `PrivateBattleOutcomeProgramTests.Settle` reuses the real scene drain, but its
long whole-route tests were not rerun for this slice. Compile acceptance is not a route PASS.

```powershell
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj --no-restore --configuration Release --filter 'FullyQualifiedName~HealingRulesTests|FullyQualifiedName~HealingFairyTests|FullyQualifiedName~BattleSceneTests|FullyQualifiedName~PrivateActionBindingTests|FullyQualifiedName~NaturalHistoriesHealThroughTheSameContentAndSessionPath|FullyQualifiedName~SharedDefinitionSupportsIndependentStartsAndNaturalActionHistories|FullyQualifiedName~SecondLearnedSpellUsesItsOwnCostAndPowerWithoutReorderingContent|FullyQualifiedName~PlayerWait'
uv run sf2 verify adapter
```

Private binding checks require the existing selected private battle inputs together with
`SF2_PRIVATE_BATTLE_SCENE_CONTENT` pointing at the newly prepared HEAL sidecar. An older physical/
Herb sidecar remains valid for those actions, but cannot silently supply authored HEAL graphics.
The maintained `uv run python -m sf2tool.remake_battle_scene_content --rom <selected-rom> --upstream <pinned-checkout> --output <fresh-ignored-directory>` command creates the candidate; retain source/generator provenance and leave accepted assets untouched.

The existing `engine_battle_scene_observation.gd` accepts `SF2_BATTLE_SCENE_HEAL=1` with world/wounded
observation enabled. The retained local launcher `local/issue523/heal-scene/run-heal.ps1` selects
source world/audio/scene inputs and a disclosed controlled party: Chester HP/maxHP40 DEF20; Sarah
MP/maxMP32 and learned HEAL3. It uses actual field/battle input and enemy injury; no runtime state
setter or original-playthrough claim. Run fresh normal and `-ReducedFlash` outputs, then compare
ordered semantic events (kind, actor/target, before/after, random range/value, detail) and each final
main/thinking seed, resources and turn cursor. Host timestamps/revisions are delivery metadata.
A source logical-work or host-readiness change invalidates this pair; documentation/test-only changes
do not. The first HEAL additionally records `healTimeoutCases` for both action and recovery text:
natural reveal/readiness, idle callbacks with no logical progress,65 actual V inputs, no Confirm,
and completion on the last neutral input. The final-input receipt proves no manufactured delivery/
Ack; subsequent scene work remains a separate operation. Later HEAL cases retain early acknowledgements.

Retained read-only reference comparisons after building the production assemblies:

```powershell
& $env:DOTNET_BIN run --project local/issue523/heal-scene/comparison/Compare.csproj -- $selectedPrepared04Checkpoints
& $env:DOTNET_BIN run --project local/issue523/heal-scene/cursor-comparison/Compare.csproj -- $env:SF2_PRIVATE_BATTLE_SCENE_CONTENT
```

These ignored inspection aids use the exact production kernel/cursor; the tracked source and named
original records own the facts. The first supplies the trace's update/control schedule and passes244
returns. The second supplies only the post-award entry seed and neutral input, and retains a completed
**FAIL** at recovery CP2059–2091. Its final coincident seed is not a passed continuous comparison.
Do not replace that failure with native settings equality or call it unavailable/interrupted.

Completed correction records remain under `local/issue523/heal-scene/`: the initial xUnit analyzer
failure, one invalid test target placement, incomplete private-environment selection, and the idle
helper test's obsolete sleep-only drain. Each received a narrow correction/rerun. PR557's independent
review also found that neutral timeout incorrectly required an extra Ack; that completed review
failure remains retained even after the focused engine/host correction. The earlier
completed slow-suite failure record remains unchanged; this work neither reruns nor relabels it.

## Battlefield death consumer

The field-death scope is status-free, ATT-only post-action processing. `BattleSceneTests` checks the
persistent HP/position/stat boundaries, twelve/three whole-batch stages, token rejection, empty
paths, capped ordered cleanup and victory/defeat release; `PhysicalBattleTests` also checks lethal
counter continuation. Use the locked environment above and the narrow filter
`FullyQualifiedName~BattleSceneTests|FullyQualifiedName~PhysicalBattleTests`, then the affected
engine and adapter entries. Do not run the retired aggregate or full H4 merely for this consumer.

A selected scene without `fieldDeath` still admits its existing close-up resources, but private
field-death delivery rejects missing content explicitly. Prepare only the bounded addition from a
previous selected scene with its adjacent `source/battle-scenes/selection.json` provenance:

```powershell
uv run python -m sf2tool.remake_battle_scene_content --rom $selectedRom --upstream $pinnedSource `
  --field-death-base $selectedScene --output local/issue523/field-death/candidate-new
```

The output must be a fresh ignored worktree-local directory. The producer checks retail ROM and
pinned source identity and previous scene provenance; it reuses the established Basic decoder,
palette and map-sprite raster composer. It writes a selected scene document with three63 sheets,
source ally/Gizmo selectors and a bounded provenance report. It does not copy/alter the selected
world/audio package, promote a material library or raise the scene reader's four-MiB limit.
Select its `battle-scenes.json` through `SF2_PRIVATE_BATTLE_SCENE_CONTENT`; keep
`SF2_PRIVATE_EXPLORATION_CONTENT` on the existing world with116/BD.

The actual `res://probes/engine_battle_scene_observation.gd` has a bounded
`SF2_BATTLE_SCENE_FIELD_DEATH=1` observation mode. Retain the existing ordinary world start and
`SF2_BATTLE_SCENE_WORLD=1`; the ordinary reward case (`SF2_BATTLE_SCENE_REWARD=1`) exercises enemy
death. `SF2_FIELD_DEATH_COUNTER=1` selects a live adjacent enemy from observed terrain and submits
ordinary approach/target/attack inputs. A declared controlled Chester HP/maxHP1, ATT3, DEF0, AGI99,
MOVE63 and initial mainSeed1048576 reaches an actual enemy counter against him; these are input
conditions, not runtime overrides or a natural original-route claim. The retained missing81/C6
failure is resolved by the accepted modern finite-PCM selection policy: actual receipts show81/CC
with requestedC6, followed by116/BD. PCM identities and the selected world/audio pack are unchanged.

Observe actual nodes/resources/facing/cells and hidden close-up, input/turn blocking, the single
116/BD AudioStreamPlayer start and finite completion, cleanup and subsequent usable input/outcome.
Normal60FPS and reduced30FPS enemy-death and counter-death observations must agree on complete ordered semantic events
(excluding delivery revision/sequence) and gameplay state. No screenshots, injected death lists,
mid-session seed changes, extra logical ticks or original-emulator launches are part of this gate.
Natural multi-death and after-turn death have no admitted producer and cannot be claimed from the
multi-member domain test. The [timing and asset boundary](../godot/battle-scenes.md#battlefield-death-batches)
retains the unresolved dependencies and prior HEAL/H4 failures.

With `SF2_AUDIO_SELECTION=1` and field-death mode disabled, the same actual adapter probe checks
exact65/BD preference among variants, unique81/CC reuse in music2/C6, consistent named-resource
playback, missing80 and ambiguous65/C6 rejection, actual Started/Finished receipts and unchanged
gameplay state. This is a direct consumer observation, not a unit test of the probe. The normal and
reduced counter pair also confirms exact83/C6 and116/BD selections.

## Battlefield movement consumer

`BattleMovementTests` exercises per-segment completion, wrong/duplicate tokens, busy commands,
provisional placement, friendly traversal and decreasing-cost return distinct from input reversal.
`EnemyActionTests` checks one thinking decision, unchanged main RNG/resources during delivery and
construction only after arrival. Source/commandset tests retain target, memory, legal-stop and
Unsupported boundaries. Callers that need completed movement explicitly use `FinishMovement`;
`Accept` and `Start` never drain it implicitly. Use the affected engine files and adapter compilation
under the locked environment. Preserve completed failing runs and rerun only failed files after
call-site migration; touching an outcome helper does not require its long complete-route observation.

For actual host acceptance, reuse the selected world containing79/BD and the existing field-selector
scene, with `SF2_PRIVATE_EXPLORATION_CONTENT` and `SF2_PRIVATE_BATTLE_SCENE_CONTENT`. The
`engine_battle_scene_observation.gd` mode `SF2_BATTLE_MOVEMENT=1`, together with
`SF2_BATTLE_SCENE_WORLD=1`, drives ordinary movement, bend, Cancel, blocked input and live-board
approach/Stay inputs through AI movement and attack. Other scene-specific modes must be disabled.
Use the existing ordinary world start and a declared external controlled party: HP/maxHP100,
DEF40, ATT5 and EXP0 for allies, Chester AGI99/MOV6, initial mainSeed2568421376. These initial inputs
make a bounded survivable observation; they do not establish a natural original route.

The observer records actual intermediate node position/facing/walking frames, stable gameplay
within each token, busy-input rejection, segment start/arrival and actual79/BD AudioStreamPlayer
receipts. Check one cue per segment revision, no cue on blocked/origin operations, retained AI
multistep Stay and move-then-attack, and finite PCM completion/replacement. Run normal60FPS and
reduced30FPS from the same declared inputs; compare complete ordered semantic events excluding
delivery revision/sequence, and final gameplay state including positions, resources, memory, turn
and both seeds. Probes and launch wrappers are observations, not engine behavior to unit-test.
No screenshots, injected paths, runtime seed changes, original runtime, new assets or resource
copies are required. The [source audit and limits](../godot/battle-scenes.md#battlefield-movement-and-walking-audio)
remain separate from native-consumer claims.

### Ordinary controls and reproduction

This recipe exercises the [Medical Herb action](../domain/gameplay-rules.md#ordinary-medical-herb-battle-action).

I / gamepad Back selects and cycles held slots; Tab cycles living allied targets, Enter commits
and Escape cancels. The inventory HUD shows slot, item name, selection and rejected attempts.
`bindings.item` in the existing version-1 input settings remaps both keyboard and gamepad.
The adapter initially targets self after selection; legality and all resource changes stay
in the common session.

Authored format-7 packages may supply an optional root `items` list of
`{id, name, effect: "consumable-healing", power, minimumRange, maximumRange}` definitions.
IDs are 0–126; 127 is the empty slot. `actors[].items` contains up to four ordered item words,
padded with 127. This supports independently authored powers/ranges without changing private
source admission. There is no new public schema or original asset in that format.

For affected item/scene changes, use the locked SDK's `dotnet test --filter`
`'FullyQualifiedName~MedicalHerbTests|FullyQualifiedName~BattleSceneTests'`, followed by
`uv run sf2 verify adapter` and the committed engine-scope planner. Existing completed failures remain
in their named handoff; these commands do not request a full slow-suite rerun.

The [scene owner](../godot/battle-scenes.md#medical-herb-scenes) gives the current private candidate
and ordinary-input native reproduction. Its existing observer reads actual session inventory/HP,
scene nodes, wait tokens and audio receipts. Source-world startup covers full-HP use after the real
entry program heals the party; a separate controlled battle start covers wounded recovery. Authored
item legality/changed-content assertions remain in `MedicalHerbTests`. The older scalar item observer
is not the scene acceptance command. Preserve failed and successful outputs separately. No original
emulator, screenshot or continuous H4 comparison is implied.
