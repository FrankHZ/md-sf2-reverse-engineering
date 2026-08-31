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
