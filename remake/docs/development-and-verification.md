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

M0 provides the engine unit project, consumed RNG/healing/turn-order/range rules, local engine and
adapter entries, and scoped public jobs. M1's common session and authored connected battle remain
planned. Main-gate owns required-check configuration during integration; a candidate must report its
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
```

`verify engine` restores in locked mode, builds and tests
`remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj`; it currently references Domain.
`verify adapter` restores in locked mode and builds `remake/game/Sf2.Remake.Godot.csproj` without tests
or native Godot. Both launch the selected executable from `remake/`, honoring `global.json` and the
protected environment. Neither needs private inputs. The engine project is selected directly;
the current `Sf2.Remake.sln` still includes four production and four legacy test projects.
Whole-solution commands remain available for an explicitly applicable legacy/release check, not every
engine or documentation change. Use formatting only for affected product projects when needed.

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
`adapter-build` for remake product/test paths, without the old solution or always-run `public-core`.
Non-research documentation and retired engine-test paths add no execution. Engine test changes select
unit tests; adapter changes select compilation; shared product/build inputs select both.

Shared CLI/harness/planner changes retain conservative research selection by default. For a declared
engine-only wiring change in those three shared modules, inspect the actual diff and use:

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

## Local Official Godot Gate

The existing runner is still available for an actually affected import, source/export execution or
process-lifecycle boundary:

```powershell
uv run python -m sf2tool.remake_godot
```

Current behavior: it verifies [toolchain.json](../toolchain.json), creates editor/template and project
scratch, performs locked restore/build, imports, runs source smoke, exports, runs exported smoke,
checks the output boundary and reaps its owned processes. Explicit `--toolchain-root`,
`--scratch-parent`, `--manifest-path` and `--project-path` remain available. This runner has not been
changed into a reusable live-debug session by the documentation audit.

Select it only when that particular lifecycle contract is needed. Repeated behavior observations use
the existing installation/project/instance instead; running this scratch-producing pipeline for each
check would violate the reuse policy. Any future runner change is separately owned and verified by
the needed actual launch, without new tests of the runner. Public CI does not launch native Godot.

## Private-Local Smoke Routing

An applicable private observation explicitly selects `private-local`, canonical/content inputs and
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
| `scope` | Compare the actual event Git range; fail on a missing/invalid range or failed comparison. Emit the three affected-path booleans. |
| `engine-unit` | Locked restore/build/test of Engine.Tests and its actual production dependencies, using the pinned .NET SDK/runtime. No Python, native Godot, private input or legacy test project. |
| `adapter-build` | Locked restore/build of the actual Godot C# project; no old Godot tests or native editor launch. |
| `research-public` | Locked Python/uv dependencies, Ruff, direct design-contract traceability and research-index checks. No engine or verification-tool pytest families. |

Product/build inputs select engine and adapter; engine tests select engine; `remake/game/` selects
adapter. Research/contracts/source/fixtures/manifests/schema inputs select research. Workflow and
shared CLI/harness/planner changes select all three. Non-research documentation and legacy remake
test edits select no product jobs. Other non-remake inputs conservatively select research. The full
predicate lives in the workflow; add a genuinely consumed external unit input when that dependency
is introduced. M0 has no external fixture-file dependency.

Main-gate must replace the old `tracked-inputs` required check with `scope`, `engine-unit`,
`adapter-build`, and `research-public` under the `Public checks` workflow, inspecting the actual
GitHub check names during integration. Path-irrelevant jobs report skipped; applicable jobs need a
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
