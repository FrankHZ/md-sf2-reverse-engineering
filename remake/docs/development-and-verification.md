# Remake Development and Verification

## Scope

This document owns the remake-specific build and gate route. Global verification selection remains
owned by [ADR 0012](../../docs/decisions/0012-dependency-aware-partitioned-verification.md), the root
repository README, and the committed planner. Local private input layout remains owned by
[Local Private Inputs](../../docs/operations/local-private-inputs.md).

Do not replace planner output with a remembered command list. Run only the affected partitions plus
any explicitly justified semantic dependency.

## Locked .NET Workflow

Run from `remake/` so the pinned SDK is authoritative:

```powershell
dotnet restore Sf2.Remake.sln --locked-mode
dotnet build Sf2.Remake.sln --configuration Release --no-restore
dotnet test Sf2.Remake.sln --configuration Release --no-build --no-restore
```

The solution contains the four production assemblies and their four owning test projects. Package
versions are centrally pinned and each project uses its tracked lock file. Do not install a parallel
SDK or edit lock files merely to make a local build pass.

Use `dotnet format` in verify mode or against explicitly named changed projects when formatting is part
of the owning slice. A documentation-only change does not run .NET unless the committed planner selects
`remake-dotnet`.

## Optional Private .NET Checks

Private checks can consume explicit local inputs. They use the existing xUnit skip mechanism and keep
their production admission and private assertions unchanged. No private input is required by Public CI.

| Test method | Required environment variables |
| --- | --- |
| `AcceptedIgnoredCanonicalImportCanBeCheckedLocallyWithoutBecomingATestInput` | `SF2_PRIVATE_CANONICAL_MAP_IMPORT` |
| `AcceptedIgnoredInputsCloseExactPayloadAndMutationBoundaries` | `SF2_PRIVATE_ROM`, `SF2_PRIVATE_MAP_TILESET_METADATA`, `SF2_PRIVATE_MAP_PALETTE_METADATA` |
| `ExactLocalMap3BaseAtlasMountCanBeCheckedWithoutBecomingATestInput` | `SF2_PRIVATE_PRESENTATION_ASSET_ROOT` |
| `ExactLocalMap21AndMap40BucketsRejectMissingOrChangedPayloads` | `SF2_PRIVATE_PRESENTATION_ASSET_ROOT` |
| `AcceptedSelectedBattle01InputsAreRequiredToExerciseTheRealReader` | `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE`, `SF2_PRIVATE_BATTLE01_TERRAIN` |

For each test, no selected inputs means an explicit **skipped / no private assertions ran** result.
Configuring any of its input variables selects that private check; partial configuration fails instead
of skipping. Complete configuration runs all its existing assertions, including fixed input identity
and admission checks. Empty or whitespace-only values count as missing. Configuration diagnostics
name environment variables without printing their values or local paths.

For an explicitly required private run, also set `SF2_REQUIRE_PRIVATE_TESTS=1`. Only the exact value
`1` requires execution of the selected private tests, so missing configuration produces a test failure
and a nonzero process exit. `--filter` narrows tests but does not itself opt into private execution.
Set the switch before invoking `dotnet test`, since xUnit decides skips during discovery. Use the
existing project and fully qualified method filter, for example after the locked build and after
configuring the canonical input:

```powershell
$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
try {
    dotnet test tests/Sf2.Remake.Content.Tests/Sf2.Remake.Content.Tests.csproj --configuration Release --no-build --no-restore --filter 'FullyQualifiedName~AcceptedIgnoredCanonicalImportCanBeCheckedLocallyWithoutBecomingATestInput'
    if ($LASTEXITCODE -ne 0) { throw 'Required private canonical check failed.' }
} finally {
    Remove-Item Env:SF2_REQUIRE_PRIVATE_TESTS
}
```

The [Map 3 route command](./map03-playability-plan.md#royal-route-acceptance-boundary) includes its
input selection. The visual test belongs to the same Content test project; the asset-mount test
belongs to `tests/Sf2.Remake.Godot.Tests/Sf2.Remake.Godot.Tests.csproj`. Use their tabled method names
with the same required switch and filter. Scope the input variables to the owning run, and keep
payloads and TRX/log output in that worktree's ignored directories. A required run with missing
inputs is a failed availability check, not a private success or product regression. A public run
with skipped private tests is public evidence only.

## Selected Battle01 Startup Inputs

The new Content check actually reads the two fixed selected source exports and the compressed
terrain input, runs the existing C# Stack decoder and checks the exact full deployment/terrain
contract. Its public semantic tests use authored placements, regions and terrain. They bypass only
the transport identity for parser checks and cannot claim admission of the canonical payload.
Application tests use the existing internal projection seam for authored data; the required-private
check exercises real digest computation and the same custom-port admission predicate.

From the repository root, set `SF2_PINNED_UPSTREAM` to the registered read-only upstream checkout
at commit `c834c652b6862bc5679fd7f69a38a7093206efc6`. Use a fresh owning worktree's ignored output
paths. These are the existing selected exporters; no whole battle/map extraction or legacy rail is
required, and the startup reader does not need a ROM.

```powershell
$upstream = $env:SF2_PINNED_UPSTREAM
$env:SF2_PRIVATE_BATTLE01_DATA = Join-Path (Get-Location).Path 'local/derived/battle01-data.json'
$env:SF2_PRIVATE_BATTLE01_SCENE = Join-Path (Get-Location).Path 'local/derived/battle01-scene.json'
$env:SF2_PRIVATE_BATTLE01_TERRAIN = Join-Path $upstream 'disasm/data/battles/entries/battle01/terrain.bin'
& ./scripts/Export-Battle01Data.ps1 -UpstreamPath $upstream -OutputPath $env:SF2_PRIVATE_BATTLE01_DATA
& ./scripts/Export-Battle01Scene.ps1 -UpstreamPath $upstream -OutputPath $env:SF2_PRIVATE_BATTLE01_SCENE
$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
dotnet test remake/tests/Sf2.Remake.Content.Tests/Sf2.Remake.Content.Tests.csproj --configuration Release --no-build --no-restore --filter 'FullyQualifiedName~PrivateOriginalBattle01StartupReaderTests'
```

Verify the two export SHA values against `manifests/extractions/battle01-data.json` and
`battle01-scene.json`; the production reader also enforces them. The scene export contains metadata
rather than decoded grid bytes, so the third input is explicitly required. Its compressed SHA is
`A0E6B0D4F656C7BD893923330148B3F9366CA7D839F5A0676272A2C95DAABC4A`, length 284. The decoded
terrain is 2,304 bytes with the accepted scene digest; it is not a 16-by-20 cropped array.
Missing/partial required configuration fails, while an unselected optional run explicitly skips.
Keep these environment variables scoped to the owning command and put TEMP/TMP, logs and generated
outputs in the same worktree. Never mutate the supplied upstream terrain file.

The owning Application filter is `FullyQualifiedName~PrivateOriginalBattle01StartupTests`. It checks
valid/repeated preparation, exact current Pending, rejection before source reads, custom-port
validation, party/RNG constraints and read-only collection/stride behavior. Use the committed planner
for subsequent .NET and official Godot selection. This data-only boundary adds no native image review
and does not rerun the already accepted twelve-frame pending-admission recipe or any H3/H4 seam.

## Repository Planner

Run from the repository root on a clean committed head:

```powershell
uv run sf2 verify plan --base origin/main --head HEAD
```

The plan reports changed paths, selected partitions, reasons, resource locks, commands, external gates,
unclassified paths, and whether execution semantics changed. Unknown runtime-shaped `remake/**` paths
fan out conservatively; tracked Markdown is public-core documentation.

The normal repository gate is:

```powershell
uv run sf2 verify
```

It may require registered local private inputs after its public stages. Do not copy private inputs into
a worktree merely to convert an unavailable optional local stage into a claimed pass. Follow the owning
slice and [Local Private Inputs](../../docs/operations/local-private-inputs.md).

`uv run sf2 verify --full` is reserved for milestone, release, shared-harness, or explicit full-parity
work. It is not the default remake feature or documentation gate.

## Local Official Godot Gate

Relevant runtime, Application, Content, Domain, Godot project, toolchain, or runner changes use the
maintained local gate from the repository root:

```powershell
uv run python -m sf2tool.remake_godot
```

The runner consumes [the tracked toolchain manifest](../toolchain.json), verifies the official Godot
version and artifact identities, creates fresh ignored scratch, performs locked restore and Debug build,
imports the project, runs the public-synthetic source smoke, exports the public preset, runs the exported
smoke, scans the output boundary, writes an ignored receipt, and cleans its owned process tree.

The gate accepts explicit `--toolchain-root`, `--scratch-parent`, `--manifest-path`, and `--project-path`
overrides when the owning run requires them. Values remain local; do not commit machine-specific paths.

Official Godot validation is local-only. GitHub Public intentionally does not download or run Godot.

## Private-Local Smoke Routing

Private smoke is not part of the public export runner. It requires:

- an explicit `private-local` profile;
- one explicit fully qualified ignored canonical-import path;
- the private smoke option;
- a current committed Debug assembly;
- a bounded process owner with finite launch, termination, and reap timeouts; and
- assertions for the already accepted marker count, ordering, status, path-free output, exit result,
  timeout result, and owned-process cleanup.

The Godot user arguments use exact `--name=value` forms for profile and canonical import. Do not pass a
private path through a split argument form, print it in a receipt, or infer it from the environment.

Private smoke wrappers remain ignored and are removed after the run. A feature must not weaken fixed
Content trust or bypass the accepted Application session merely to make a smoke pass.

## GitHub Public

The sole public workflow job is `tracked-inputs`. It runs:

- locked Python dependency sync and Ruff;
- the shared tracked-input harness plus architecture/planner checks;
- design-contract traceability; and
- locked whole-solution .NET restore, Release build, and tests.

It uses tracked redistribution-safe inputs only. It does not run H3, H4, private profiles, emulator
sessions, original-fidelity checks, or Godot import/export.

## Process, Path, and Artifact Safety

- Launch native tools with argument lists and finite timeouts.
- Cleanup must own the launched process tree; do not kill machine-global processes by name or PID delta.
- Use a fresh ignored scratch directory per gate and verify it is inside the intended local root.
- Keep immutable shared private inputs read-only and writable scratch worktree-local.
- Never place ROMs, canonical imports, decoded assets, engine archives, templates, `.godot/`, `bin/`,
  `obj/`, exports, captures, or receipts in Git.
- Scan staged paths and exported payloads before acceptance.
- Preserve the first failed attempt and its cleanup result; do not enlarge a timeout as the first fix.

Use [Bounded Inspection and Review](../../docs/operations/bounded-inspection-and-review.md) for large
artifacts, cached-diff review, and exact-identity handoff.

## Proportional Gate Guide

| Change | Expected local gates |
| --- | --- |
| Domain or Application behavior | owning focused tests, locked solution, architecture checks, committed planner, selected local Godot smoke, normal verify where inputs are available |
| Content admission or trust | focused adversarial Content/Application tests, locked solution, private scan, committed planner, selected local Godot smoke |
| Godot adapter or project | Godot tests, locked solution, official local import/source/export/export-run gate, process cleanup checks |
| remake documentation only | link/fence/table/private/diff checks, committed planner, public-core verification, lightweight Public |
| shared gate, planner, toolchain, or workflow | owning focused tooling tests plus every partition selected by the committed planner |

H1, H2, H3, H4, full verification, or private-input runs are added only when their accepted owner or the
committed planner requires them. A zero-exit editor launch, manual screenshot, or unbounded local command
is not a substitute for the maintained gate.
