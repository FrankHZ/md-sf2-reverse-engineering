# SF2 Remake

This directory contains the independently maintained Godot 4.7.2 .NET/C# remake. It consumes
accepted repository contracts and uses project-authored or caller-supplied local content without
making engine code an evidence owner.

## Current Status

Phase 4 implementation is active through bounded Map 3 slices. The current runtime supports a
tracked public-synthetic exploration shell, one project-authored 3-by-2 tactical micro-battle with
completion, an atomic once-only synthetic world-state effect, exploration return, and a separate
private-local traversal shell with an opt-in project-authored Map 3 base view. The micro-battle proves only the synthetic Exploration -> Battle ->
Completed -> project-authored completion state -> Exploration product topology. It is not Battle 01,
an original after-battle program, or the accepted continuous Map 3-through-Battle 01 milestone, which
remains **NOT READY**.

The two persistent runtime disclosures are part of the product boundary:

- `PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY`
- `PRIVATE LOCAL — NOT FULL ORIGINAL FIDELITY`

Current capabilities and retained Unknowns are summarized in
[Capability Status](./docs/capability-status.md).

## Runtime Profiles

| Profile | Input boundary | Current purpose |
| --- | --- | --- |
| `public-synthetic` | tracked, project-authored content only | default interactive shell, logic tests, local Godot gate, and redistribution-safe export smoke |
| `private-local` | explicit caller-selected ignored inputs with fixed admission checks | bounded original Map 3 traversal, local diagnostics, and an optional project-authored base view; not a full-original runtime |

Private execution is never inferred from a file's presence and never silently falls back while
reporting private success. See [Runtime Profiles and Trust](./docs/runtime-profiles-and-trust.md).

## Build and Test

Run the locked .NET workflow from this directory:

```powershell
dotnet restore Sf2.Remake.sln --locked-mode
dotnet build Sf2.Remake.sln --configuration Release --no-restore
dotnet test Sf2.Remake.sln --configuration Release --no-build --no-restore
```

The pinned SDK, package versions, and NuGet source are tracked beside the solution. Relevant runtime
changes also use the repository-maintained local official Godot gate. GitHub Public intentionally
remains lightweight and does not download or run Godot.

See [Development and Verification](./docs/development-and-verification.md) for planner routing,
local Godot validation, private-smoke boundaries, and process cleanup requirements.

## Repository Layout

```text
remake/
  Sf2.Remake.sln                 locked whole-remake solution
  src/
    Sf2.Remake.Domain/           deterministic state, values, and reducers
    Sf2.Remake.Application/      GameSession, commands, ports, and observations
    Sf2.Remake.Content/          validated public/private input adapters
  game/                          Godot project and thin host adapters
  tests/                         Domain, Application, Content, and Godot tests
  docs/                          implementation architecture, profiles, status, and workflow
  global.json                    pinned .NET SDK
  toolchain.json                 official Godot artifact identity and bounded timeouts
```

The dependency and delegation map is documented in [Architecture](./docs/architecture.md).

## Boundaries

- `GameSession` is the sole logical gameplay mutation facade.
- Domain and Application code do not depend on Godot, JSON, machine paths, or original payloads.
- Content validates external identities and structure before constructing admitted definitions.
- Godot maps input to semantic commands and projects authoritative observations; it does not own
  gameplay rules.
- The public-synthetic tactical micro-battle is deterministic project-authored content. Its actor,
  enemy, grid, hit points, damage, completion flag/effect/setup, cues, and return state make no claim
  about the original game. Completion state is applied once by `GameSession`; it prevents synthetic
  re-entry and remains isolated from restart.
- Private ROMs, canonical imports, decoded payloads, captures, tools, and generated exports remain
  ignored and local. None is committed, uploaded, or embedded in the public package.
- Public-synthetic import/export success grants no right to distribute original content.
- Natural route, Battle 01 continuity, original rendering/audio/assets, persistence, complete H4,
  and 8C fidelity remain incomplete, deferred, Unsupported, or Unknown at their existing owners.

## Architecture Decisions

- [ADR 0008](../docs/decisions/0008-godot-csharp-cli-first-remake-tooling.md) fixes Godot 4.7.2
  .NET/C# and the CLI-first, plain-C# core.
- [ADR 0011](../docs/decisions/0011-phase4-remake-runtime-architecture.md) fixes the four assemblies,
  state ownership, Content ports, Godot adapter, and H4 layering.
- [ADR 0017](../docs/decisions/0017-heavy-boundaries-light-internals.md) retains heavy trust,
  mutation, versioned-port, and observation boundaries while keeping same-process internals light.

Historical implementation slices and review chronology remain in Git and merged pull requests rather
than this entry document.
