# SF2 Remake

This directory contains the independently maintained Godot 4.7.2 .NET/C# remake. It consumes
accepted repository contracts and uses project-authored or caller-supplied local content without
making engine code an evidence owner.

## Current Status

Phase 4 implementation is active through bounded Map 3 slices. The current runtime supports a
tracked public-synthetic exploration shell, one project-authored 3-by-2 tactical micro-battle with a
deterministic enemy response, typed victory/defeat, same-definition retry, an atomic once-only
synthetic victory effect, and exploration return. A separate private-local traversal shell with an
opt-in project-authored Map 3 base view can explicitly enter the same tactical reducer at the
controlled start; defeat/retry and victory preserve the exact private traversal snapshot and never
apply the public synthetic world effect. This manual bridge is not natural battle admission, Battle
01, an original after-battle program, or the accepted continuous Map 3-through-Battle 01 milestone,
which remains **NOT READY**.

The two persistent runtime disclosures are part of the product boundary:

- `PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY`
- `PRIVATE LOCAL — NOT FULL ORIGINAL FIDELITY`

Current capabilities and retained Unknowns are summarized in
[Capability Status](./docs/capability-status.md).

## Runtime Profiles

| Profile | Input boundary | Current purpose |
| --- | --- | --- |
| `public-synthetic` | tracked, project-authored content only | default interactive shell, logic tests, local Godot gate, and redistribution-safe export smoke |
| `private-local` | explicit caller-selected ignored inputs with fixed admission checks | bounded original Map 3 traversal, local diagnostics, an optional project-authored base view, and a manual project-authored battle bridge; not a full-original runtime |

Private execution is never inferred from a file's presence and never silently falls back while
reporting private success. See [Runtime Profiles and Trust](./docs/runtime-profiles-and-trust.md).

## Local Presentation Asset Preflight

The local-only `md-sf2-remake-assets` checkout remains a separate product-art repository. Before a
checkout or exported pack can be offered to the accepted Content reader, the repository-owned
preflight verifies an explicitly pinned commit, tree, and manifest digest, a clean local-only Git
state, the closed manifest, and every referenced runtime payload:

```powershell
uv run python -m sf2tool.remake_assets checkout `
  --asset-root <fully-qualified-asset-checkout> `
  --expected-commit <40-lowercase-hex> `
  --expected-tree <40-lowercase-hex> `
  --expected-manifest-sha256 <64-uppercase-hex>

uv run python -m sf2tool.remake_assets export `
  --asset-root <fully-qualified-asset-checkout> `
  --expected-commit <40-lowercase-hex> `
  --expected-tree <40-lowercase-hex> `
  --expected-manifest-sha256 <64-uppercase-hex> `
  --destination <fully-qualified-new-export-directory>
```

The path-free descriptor carries the exact asset commit, tree, manifest digest, capability, and
bounded pack totals required by outer composition. Export writes only the manifest and its referenced
`runtime/` payloads to a fresh sibling staging directory, verifies the copy, writes the descriptor
last, and atomically promotes it without modifying the source checkout or overwriting an earlier
export. It never copies `.git`, `source/`, `masters/`, ignored caches, previews, or Godot import state.

This is transport and checkout preflight, not asset generation or product admission by itself. A
separate candidate builder now closes the first deterministic HUD SVG derivation boundary:

```powershell
uv run python -m sf2tool.remake_asset_build hud-svg-candidate `
  --asset-root <fully-qualified-asset-checkout> `
  --expected-commit <40-lowercase-hex> `
  --expected-tree <40-lowercase-hex> `
  --asset-id hud.<name> `
  --expected-master-sha256 <64-uppercase-hex> `
  --resvg-archive <fully-qualified-resvg-win64.zip> `
  --candidate-name <fresh-cache-child>
```

The builder admits exactly one nonignored untracked `masters/ui/<name>.svg` over an otherwise exact
local-only checkout, verifies the pinned resvg archive and executable version, renders deterministic
2x/4x RGBA8 PNGs twice, validates the existing pack schema, and atomically publishes only a fresh
direct child under ignored `cache/`. It never stages, promotes, commits, or prints a local path.

The first reviewed local transaction now owns `hud.yes-no-window-frame`: a project-authored 112-by-24
SVG master plus deterministic 2x/4x runtime PNGs and one closed manifest. An explicit PrivateLocal
launch may opt into a bounded top-right diagnostic preview by supplying `--private-hud-preview`
together with a fully qualified asset root, its exact lowercase commit, and its exact uppercase
manifest SHA-256. The Content reader admits the complete pack before `GameSession` starts; the thin
Godot catalog resolves only the semantic asset ID and accepted 2x/4x bucket. It asks the same Content
reader to reopen the fixed manifest, resolve and recheck the contained path/length/digest, and return a
defensive byte copy before Godot decodes it. No runtime path crosses into Application or Godot. Partial
values, an implicit mount, or a failed private mount never fall back while reporting private success.

The current preview uses the limiting physical dimension of the centered 16:9 frame at 100% UI scale;
user-selectable UI scaling remains deferred rather than being guessed by this slice.

This preview is chrome only. It is not a Yes/No menu, has no text, icon, input, selection, Theme, or
gameplay semantics, and creates no new stable smoke marker. The separate `md-sf2-gfx-remake`
repository remains non-authoritative R&D: its measurements informed the product-authored frame, but
its ignored experimental SVG contains forbidden text and an embedded raster and is not an admitted
product master or runtime input.
Because the product asset repository intentionally has no remote, an exact local commit proves
identity but does not provide off-machine recovery; source/master backup remains a separate local
operational responsibility. Rollback selects a prior reachable local commit or an immutable prior
export and never rewrites or overwrites accepted history.

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
  enemy, grid, movement/attack ranges, hit points, damage, fixed north/east/south/west enemy movement
  tie-break, victory/defeat, retry, completion flag/effect/setup, cues, and return state make no claim
  about the original game. Only victory applies completion state once through `GameSession`; defeat
  applies no world effect and exact acknowledgement restarts the same definition.
- The private-local battle bridge reuses only that tactical definition, commands, cues, and Domain
  reducer. It pauses private traversal while active, preserves the same private snapshot through
  defeat/retry and victory, and does not import the public completion flag, effect, setup, facing, or
  return-map state.
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
