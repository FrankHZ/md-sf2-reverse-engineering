# Development and Verification

## Hidden-spawn authored observation

Start `remake/content/authored/hidden-spawns.json` through `--authored-package`. Up moves the ally
into the trigger; Enter/Space/Enter commits Stay. The source enemy's ordinary turn clears the checked
word, and the next round resets/appears the returner then hidden enemy. Enter/F/Tab/Enter attacks the
returner; Enter acknowledges combat messages. Its ordinary death is followed by a later-round actual
appearance. Down then demonstrates usable movement. No actor IDs or this route define legality.

Load the retained `local/private-inputs.ps1` in the launch process, force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, and build the actual adapter Debug configuration with the
selected absolute SDK. Reuse the project/installation; a new instance is justified when none is
retained. Use a fresh worktree-local ignored directory for APPDATA/LOCALAPPDATA/TEMP/logs/report:

```powershell
$project = (Resolve-Path -LiteralPath 'remake/game').Path
$package = (Resolve-Path -LiteralPath 'remake/content/authored/hidden-spawns.json').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --resolution 1280x720 --path $project `
  --script res://probes/engine_hidden_spawns_observation.gd -- --authored-package $package
```

The bounded probe supplies ordinary keys only after startup. It reads real appearance completion,
intermediate marker rotation/alpha, settled values, pending-member hiding, ordered queue/appearance
facts, ordinary death and subsequent movement, with errors surfaced. Each run is limited to180s and
5MiB semantic output. No screenshots, state mutation or fabricated completion. Engine behavior tests
cover mode2 death, mode3 return, source geometry/checked bits, reset refusal, input validation, failure
prefixes and ordinary leader return/retry with retained custody. Use `uv run sf2 verify engine`,
affected adapter/Debug build, clean committed planner and exact-head CI. Preserve completed failures;
correct only affected cases instead of rerunning an aggregate to erase a red result.

Prior #1351's seven failures/corrections remain in its
[handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1352#issuecomment-6434280116), along
with the earlier #1341/#1343/#1345/#1347/#1349 failures and twelve unresolved #1047 comparisons.
No full/retired/H4/emulator run or private Battle44 acquisition follows from this slice.

## Prism laser authored observation

Use `remake/content/authored/prism-laser.json` with ordinary `--authored-package` startup. Both allies
start off the ray. Enter/Space/Enter commits Stay for each, producing no laser. On their next turns,
Up moves each onto the ray; wait for movement delivery, then Enter/Space/Enter commits Stay. Enter
acknowledges laser and damage/death messages. The Flower hits companion, leader and sentry in source
slot order, removes the two HP1 targets without ally kill credit, and returns leader control.
Down demonstrates usable movement. No particular IDs/order are engine legality conditions.

Load `local/private-inputs.ps1` in the launch process, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`,
and build the actual adapter Debug configuration using the configured absolute SDK. Reuse the owned
Godot/project; only startup needs a new instance when none is retained. With a fresh ignored
worktree-local run directory for APPDATA/LOCALAPPDATA/TEMP/logs/report, run from repository root:

```powershell
$project = (Resolve-Path -LiteralPath 'remake/game').Path
$package = (Resolve-Path -LiteralPath 'remake/content/authored/prism-laser.json').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --resolution 1280x720 --path $project `
  --script res://probes/engine_prism_laser_observation.gd -- --authored-package $package
```

The probe uses ordinary keys only, with compact semantic view/input/error observations. Bound each
run to180s and5MiB; no screenshots, media capture, post-start mutation or fabricated completion.
Actual `PrismLaserTests`/`PrismLaserSessionTests` also cover empty reconstruction/scene completion,
changed directions/positions/slots, normal damage, rejection, leader sweep and retained failure prefixes.
Run `uv run sf2 verify engine`, affected adapter, clean committed dependency plan and exact-head CI.
Preserve completed failures and correct their affected tests rather than repeat a completed aggregate.
Historical #1341/#1343/#1345/#1347/#1349 failures and twelve unresolved #1047 comparisons remain with
their existing owners. No retired/full/H4/emulator run follows from this authored slice.


Use this entry for environment, scope and check selection. Choose a focused recipe only for an
affected behavior: [host/input](verification/host.md), [battle/rules/scenes](verification/battles.md)
or [field/programs](verification/field-programs.md). [Retained evidence](evidence/retained-comparisons.md)
separates accepted composition from historical failures and retired run instructions.

The selected [Battle01-through-Battle02 recipe](verification/field-programs.md#battle01-through-battle02)
owns private preparation, ordinary human-play launch and semantic observation for the second encounter.
It starts at an explicit tower boundary, keeps the subsequent route continuous, and stops before
Battle02 resolution. Its candidate results do not extend the independently accepted milestone.

Documentation upkeep uses Standard rigor: current behavior/navigation and preserved evidence are
the acceptance boundary. It changes no gameplay, original evidence, UX policy or verification
framework. Historical dispatch plans and audit dispositions belong to Git/Issues, not this guide.

## Battle item-drop authored observations

Use `battle-drops.json` and `battle-drops-full.json` under `remake/content/authored/` with the ordinary
`--authored-package` host. Enter interacts and acknowledges the before text; Enter/F/Tab/Enter selects
and commits the enemy attack. Enter acknowledges scene text. Direct receipt displays dropped and
received messages; full-pack Deals fallback is silent. After usable field return, Left walks onto
the authored retry event; repeat the attack. Both reached drop draws are zero, so the second kill
exercises the held once bit rather than merely a failed chance roll.

Load `local/private-inputs.ps1` in the launching process, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`,
and build `game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore` from `remake/` using the
configured absolute `DOTNET_BIN`. Reuse the existing Godot/project; each variant needs its own startup
package, with no hot state injection. Set worktree-local APPDATA/LOCALAPPDATA/TEMP, logs and a fresh
ignored report destination. From the repository root:

```powershell
$project = (Resolve-Path -LiteralPath 'remake/game').Path
$package = (Resolve-Path -LiteralPath 'remake/content/authored/battle-drops.json').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --resolution 1280x720 --path $project `
  --script res://probes/engine_battle_drops_observation.gd -- --authored-package $package
```

Repeat with `battle-drops-full.json` and a separate destination. The probe submits ordinary keys,
observes real session/presenter errors, message/token/field-death waits, inventory/accounting,
once flags and Deals through return and walking reentry, then repeats the kill. It never supplies
successful completion or post-start state. Compact samples and selected semantic events fit the
180s/5MiB per-run budget. No screenshots, exports, new media or bulk capture are involved.

Run `EnemyItemDropTests`/`EnemyItemDropSessionTests`, `uv run sf2 verify engine`, affected adapter
build, and the clean committed planner. Preserve completed failures and rerun only corrected nodes
or newly affected checks. Native results establish authored engine/host behavior; static source
provenance and original runtime limits remain with the rule/research owners. Independent review
and exact-head CI remain required; no H4/emulator or retired aggregate is implied.

## Leader cleanup authored observation

Use `--authored-package <absolute path to remake/content/authored/leader-cleanup.json>` with the
ordinary host. Enter interacts with the ferryman; Enter acknowledges the before text. At battle
control, Enter confirms the position, F chooses ATTACK, Tab selects `enemy`, and Enter commits.
Acknowledge ordinary scene/result text with Enter. The gate and living survivor use the existing
field death phases, followed by victory text and usable exploration; Left moves the returned player.

After loading `local/private-inputs.ps1`, forcing `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, and building
the actual adapter Debug configuration from `remake/`, run from the repository root:

```powershell
$package = (Resolve-Path -LiteralPath 'remake/content/authored/leader-cleanup.json').Path
$project = (Resolve-Path -LiteralPath 'remake/game').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --resolution 1280x720 --path $project `
  --script res://probes/engine_leader_cleanup_observation.gd -- --authored-package $package
```

Keep APPDATA/LOCALAPPDATA/TEMP, report and logs inside the fresh owning-worktree run directory;
absolute package paths avoid Godot's project-cwd resolution. Reuse the selected installation and
project. Reuse an owned running instance when available; otherwise start it for actual presenter
observation. The probe submits ordinary keys and reads real state, phase tokens, marker visibility,
HP, rewards, ordered events and errors. No screenshots, injected state, synthetic successful
completion, exports or bulk capture. Budget <180s and <5MiB/run; retain launch and completed failures.

`LeaderCleanupTests` covers real attack/return, source-slot/caller variations, simultaneous leader
loss, held completion, unplaced survivors, accounting and retry boundaries. Run affected scene,
physical, sleep, Taros and outcome behavior plus `uv run sf2 verify engine`, `uv run sf2 verify adapter`
and the clean committed planner. Corrections rerun their affected tests; exact-head CI establishes
the final public selection. Prior results and twelve unresolved #1047 comparisons stay preserved.
This is authored engine/presenter acceptance, not full Battle18, private outcome or hardware parity.

## Taros authored observation

Use the ordinary host with `--authored-package remake/content/authored/taros-effectiveness.json`.
The controlled package starts two allies beside a mapped Taros-type guardian. With the default
keyboard, move North, Enter, F (ATTACK), Tab (guardian), Enter. The apprentice's equipped61 is
ineffective because it is not bound Bowie. Repeat those keys with champion for normal damage.
Ordinary scene acknowledgements are Enter; both paths return to usable battle control.

After loading the worktree's `local/private-inputs.ps1`, forcing
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false` and building the actual adapter Debug configuration from
`remake/`, the bounded native recipe is:

```powershell
$env:SF2_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --path remake/game `
  --script res://probes/engine_taros_effectiveness_observation.gd -- `
  --authored-package remake/content/authored/taros-effectiveness.json
```

Resolve arguments from the repository root (or supply absolute package/project paths). Keep
APPDATA/LOCALAPPDATA/TEMP and the report/logs inside the owning worktree's ignored run directory;
use the existing selected editor/project, with no copied installation or new assets. The probe
submits ordinary keys and reads actual state, result label, delivery events and errors; it has no
state setters, synthetic completions or images. It checks ineffective HP/gold/status and no recoil,
effective damage/recoil, and movement/cancel after scene return. Store semantic boundary samples,
not per-frame world dumps. Budget <180s and <5MiB/run; retain completed failures with their cause.

Run `TarosEffectivenessTests` plus affected `SleepAndFlyingTests`/physical/scene behavior directly,
then the current `uv run sf2 verify engine`, `uv run sf2 verify adapter` and clean-head planner.
The observation proves modern authored physical feedback only. No H4, emulator, full/retired suite,
private Battle18 import, natural original route, all-action immunity, equipment UI or hardware
presentation claim follows. Earlier #1047/#1341/#1343 failures/results remain preserved.

## Scope

### Retired bulk frame workflow

The user's 2026-10-04 decision retires the previous whole-route per-frame H4 capture,
processing and audit workflow. Its [historical recipes](evidence/retained-comparisons.md) do not authorize another bulk run,
processing pass or frame-by-frame audit. The old C comparison was terminated by user retirement;
D was already cancelled. Large raw frame captures and reader-derived databases from the named
#534 C/D and #605 allocations are authorized for disposal under the exact inventory in #615.
Cleanup status belongs to that Issue; authorization is not proof that deletion completed.
Small failure/acceptance records, necessary reproduction slices and published compact reports
remain retained. Original ROMs, save states, source and asset libraries are outside that cleanup.

Future work starts from the missing behavior or evidence relation, reuses accepted static rules
and bounded results, and observes necessary logical actions, state changes and actual consumer
boundaries. Use the [existing scale plan](#observation-and-comparison-planning) before acquisition.
Per-frame measurements remain appropriate only for a named timing claim over a bounded interval;
they are not the default format for whole-route state or resource evidence. Retiring this workflow
does not turn unresolved assertions into PASS or erase the completed historical failures.

On this host, future mutable execution uses the selected H-drive checkout and its local outputs,
TEMP, Python/NuGet environments and caches. Exact paths stay in ignored machine configuration.
Existing shared read-only inputs and tool installations are reused. The former C-drive execution
checkout is retained history, not a destination for new work; do not migrate its retired bulk data
into the new checkout merely to continue the old workflow.

Current product acceptance uses the [explicit default-keyboard scope](evidence/retained-comparisons.md#current-keyboard-comparison-scope).
Older four-variant recipes in the retained record are retained history, not authorization to run B/D or require C.

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

The engine unit project covers actual Domain/Application/Content behavior for authored battle
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

## Observation and Comparison Planning

Before a large or expanded capture/comparison, apply the
[scale-planning owner](../../docs/operations/bounded-inspection-and-review.md#plan-before-scaling).
Declare the claim and minimum sufficient granularity, input/intermediate/output cardinalities,
algorithmic cost, resource budgets, stages, concurrency and publication headroom. Reuse relevant
measurements or a representative small pilot before the full run. Evaluate native collection and
offline comparison separately: acceptable frame time, an indexed database or a bounded writer queue
does not establish acceptable whole-run storage or join cost. Include repeated resource uses,
requirement/candidate products, copies, journals and final report companions in the estimate.

For future probes, start ordinary CPU/memory/progress and coarse state sampling around1Hz, while
retaining necessary semantic and actual-consumption events at their originating boundaries with
occurrence/order identity. Choose finer sampling for a named claim; do not miss a short event by
sampling once per second. A draw callback may itself occur every frame for many resources, so
event-driven collection still needs an explicit total-volume estimate.

Reuse stable descriptors and compact unchanged uses into counts or delimited spans only when the
accepted predicate permits it and completeness, changes and contradictions remain auditable.
Per-frame motion/camera/draw evidence needs a bounded interval and byte budget. Frame-time
percentiles may require per-frame timing statistics, not per-frame full-state serialization.
Sampling, serialization and flush rates are separate: batched writes preserve all required events,
overflow/error behavior and complete terminal records.

Select offline scope before expensive import and derivation, preserving required context and
explicitly marking unselected or missing obligations. Estimate peak logical bytes separately from
physical volume headroom; compression does not remove redundant work. Preserve diagnostics and
replan when observed costs materially exceed the estimate. These design rules do not alter existing
captures, acceptance predicates, running-process control or cleanup authority. A guidance-only change
uses direct document checks and does not trigger native, SDK, normal/full or H3 verification.

## Locked .NET Workflow

Load the existing ignored host/worktree environment before every SDK command, including informational
commands. Select one existing absolute `DOTNET_BIN` and one absolute `DOTNET_CLI_HOME` shared by this
project's worktrees. Force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false` at the actual child launch.
The ignored machine configuration places complete .NET and Godot installations beneath its selected
shared tool root; `GODOT_BIN` selects the editor and `$godotBinary = $env:GODOT_BIN` supplies the
examples below. Reuse those installations without repeating setup or gates merely to change paths.
The maintained .NET helper preserves valid local NuGet selections, rejects external cache paths and
selects worktree-local TEMP/TMP. See the [shared tool owner](../../docs/operations/local-private-inputs.md)
for the BizHawk runtime-copy exception and preparation/runtime distinction.
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
```

`verify engine` restores in locked mode, builds and tests
`remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj`; it references production
Domain/Application/Content and copies the actual `remake/content/authored/*.json` inputs.
`verify adapter` restores in locked mode and builds `remake/game/Sf2.Remake.Godot.csproj` without tests
or native Godot. These commands launch the selected executable from `remake/`, honoring `global.json`
and the protected environment. Neither needs private inputs. `Sf2.Remake.sln` contains the four
production projects and Engine.Tests; the legacy test projects and reference host were retired at M5. Use formatting only for affected product projects when needed.

## Optional Private .NET Checks

Private engine checks use xUnit's explicit private-input selection through `PrivateInputFact`.
`PrivateBattleEncounterTests` and `PrivateBattleInitializationTests` are ordinary facts over in-memory
authored records and always run.

| Engine.Tests family | Required selections |
| --- | --- |
| `PrivateBattleScenarioTests`, `PrivateSourceAiTests`, `PrivateActionBindingTests` | `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE`, `SF2_PRIVATE_BATTLE01_TERRAIN`, `SF2_PRIVATE_STATIC_DATA`, `SF2_PRIVATE_ENEMY_DATA`, `SF2_PRIVATE_ENEMY_GOLD`, plus `SF2_PRIVATE_CONTROLLED_START` for the scenario and source-AI checks |
| `PrivateExplorationTests`, `PrivateBattleEntryProgramTests`, `PrivateBattleOutcomeProgramTests` | the six battle inputs above plus `SF2_PRIVATE_EXPLORATION_CONTENT` |

`PrivatePresentationBytesAndRequiredSpriteLinksAreAdmittedBeforeStartup` additionally requires a world
prepared with `--rom-path` and `--presentation-root`; a world prepared without the local asset pack
cannot exercise it. The Battle01 exports and enemy gold come from the pinned H2 rails below; the
canonical import from `uv run sf2 h2 map-import`; the world from the
[exploration preparation](verification/field-programs.md#reproduction).

With no selected inputs, an optional check reports **skipped / no private assertions ran**. Partial
selection fails. For a required check set `SF2_REQUIRE_PRIVATE_TESTS=1` before discovery; missing inputs
then fail. `--filter` does not opt into private execution and other switch values do not require it.
Scope variables to that run and keep logs/output local. Do not interpret public success with private
skips as private acceptance.

[Local Private Inputs](../../docs/operations/local-private-inputs.md) owns registered read-only input
selection. Load it in the same process as any command that needs those inputs. Check the configured
shared ROM and its narrow identity command before reporting it missing; keep configuration, missing
input, identity mismatch, and later upstream/toolchain failure distinct. No copied input is required
solely to repair the appearance of an aggregate result.

## Selected Battle01 Startup Inputs

The production `PrivateBattleEncounterReader` consumes the selected Battle01 data and scene JSON
exports plus compressed terrain. Their exact identities remain in
[battle01-data](../../manifests/extractions/battle01-data.json),
[battle01-scene](../../manifests/extractions/battle01-scene.json), and that Content reader. These
transport identities describe fixed private admission; they are not the general authored-content
schema or a gameplay legality condition. The compressed terrain input is the H1 split output
`disasm/data/battles/entries/battle01/terrain.bin`, so it exists only after a passing H1 rebuild.

Reuse accepted exports read-only after checking identity. If the owning task actually needs to
reproduce them, the existing `scripts/Export-Battle01Data.ps1` and `Export-Battle01Scene.ps1` consume
the pinned upstream and explicit ignored destinations. New maintained tooling still belongs in
`src/sf2tool/`; this is a frozen compatibility route, not permission to add more PowerShell rails.

The [Map 3 implementation/reference record](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/map03-playability-plan.md) retains selected inputs,
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
`adapter-build` for ordinary product/test paths, without the old solution or always-run `public-core`.
Non-research documentation and retired engine-test paths add no execution. Engine test changes select
unit tests; ordinary game changes select ordinary compilation. Controlled inputs under
`remake/reference/inputs/` select `engine-unit`, because Engine.Tests copies them and actual private
engine facts consume them. Shared product/build inputs select both actual downstream builds.

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

## GitHub Public

Current [public-checks.yml](../../.github/workflows/public-checks.yml) remains triggered on every pull
request and main push. Its Windows jobs are:

| Job | Current execution |
| --- | --- |
| `scope` | Compare the actual event Git range; fail on a missing/invalid range or failed comparison. Emit engine, adapter, research and H4 affected-path booleans. |
| `engine-unit` | Locked restore/build/test of Engine.Tests and its actual production dependencies, using the pinned .NET SDK/runtime. No Python, native Godot, private input or legacy test project. |
| `adapter-build` | Locked restore/build of the ordinary game C# project. No Godot tests or native editor launch. |
| `research-public` | Locked Python/uv dependencies, Ruff, direct design-contract traceability and research-index checks. No engine or verification-tool pytest families. |
| `h4-comparison-build` | Build the standalone C# comparison executable through the controlled Python launcher with pinned SDK/runtime. No native game or original input. |

Shared product/build inputs select engine and adapter; engine tests select engine; `remake/game/`
selects the ordinary adapter. `remake/reference/inputs/` selects engine for its Engine.Tests consumers. Research/contracts/source/fixtures/manifests/schema inputs select
research. Workflow and shared CLI/harness/planner changes select
engine/adapter/research; the workflow also selects H4 for its explicit comparison/build inputs.
Changes under `tools/h4-comparison/` or to the named H4 bridge/comparator/environment/planner/workflow
paths select `h4-comparison-build`. Non-research documentation and legacy remake test edits select no product jobs. Other non-remake inputs conservatively select research. The full
predicate lives in the workflow; add a genuinely consumed external unit input when that dependency
is introduced. M1's consumed authored JSON lives under `remake/content/`, which conservatively selects all product/host builds.

Main-gate owns required-check configuration. The reference host and its build job are retired;
do not request that obsolete context as a current gate.
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
